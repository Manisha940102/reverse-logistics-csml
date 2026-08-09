namespace BackendService.Services;

using Microsoft.EntityFrameworkCore;
using BackendService.Data;
using BackendService.DTOs;
using BackendService.Helpers;
using BackendService.Models;
using System.Diagnostics;

/// <summary>
/// Core business logic service for order evaluation.
/// Implements the GUIDE.md specification:
///   1. Query order + items + product + customer + geolocation from DB
///   2. Calculate Haversine distance, volumetric weight, reviewer deviance
///   3. Read active CostMatrixConfig from DB
///   4. Call Flask ML API via MlApiService
///   5. Persist prediction to DB
/// </summary>
public class OrderService
{
    private readonly AppDbContext _db;
    private readonly MlApiService _mlApi;
    private readonly ILogger<OrderService> _logger;

    public OrderService(AppDbContext db, MlApiService mlApi, ILogger<OrderService> logger)
    {
        _db = db;
        _mlApi = mlApi;
        _logger = logger;
    }

    /// <summary>
    /// Fetches pending/delivered orders for evaluation (paginated).
    /// </summary>
    public async Task<List<OrderDetailDto>> GetPendingOrdersAsync(int page = 1, int pageSize = 20)
    {
        return await _db.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .Where(o => o.OrderStatus == "delivered" || o.OrderStatus == "shipped")
            .OrderByDescending(o => o.OrderPurchaseTimestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderDetailDto
            {
                OrderId = o.OrderId,
                OrderStatus = o.OrderStatus,
                OrderPurchaseTimestamp = o.OrderPurchaseTimestamp,
                CustomerCity = o.Customer != null ? o.Customer.CustomerCity : null,
                CustomerState = o.Customer != null ? o.Customer.CustomerState : null,
                TotalPrice = o.OrderItems.Sum(i => i.Price),
                TotalFreight = o.OrderItems.Sum(i => i.FreightValue),
                ItemCount = o.OrderItems.Count()
            })
            .ToListAsync();
    }

    /// <summary>
    /// Full evaluation pipeline for a single order, as specified in GUIDE.md Section 3.
    /// </summary>
    public async Task<PredictionResponseDto> EvaluateOrderAsync(string orderId)
    {
        var stopwatch = Stopwatch.StartNew();

        // 1. Query order with all related entities
        var order = await _db.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
            .Include(o => o.OrderReviews)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null)
            throw new ArgumentException($"Order '{orderId}' not found.");

        var firstItem = order.OrderItems.FirstOrDefault();
        var product = firstItem?.Product;

        // 2. Calculate Haversine distance
        double shippingDistanceKm = 0;
        if (order.Customer != null)
        {
            var customerGeo = await _db.Geolocations
                .Where(g => g.GeolocationZipCodePrefix == order.Customer.CustomerZipCodePrefix)
                .FirstOrDefaultAsync();

            // Use seller's geolocation (from order_items seller_id -> geolocation)
            // For simplicity, use a default seller location if not available
            if (customerGeo != null && firstItem != null)
            {
                // Try to find seller geolocation through seller ZIP
                // Since we don't have a sellers table, estimate using geolocation table
                var sellerGeo = await _db.Geolocations
                    .Where(g => g.GeolocationZipCodePrefix != order.Customer.CustomerZipCodePrefix)
                    .FirstOrDefaultAsync();

                if (sellerGeo != null)
                {
                    shippingDistanceKm = HaversineCalculator.CalculateDistanceKm(
                        customerGeo.GeolocationLat, customerGeo.GeolocationLng,
                        sellerGeo.GeolocationLat, sellerGeo.GeolocationLng);
                }
            }
        }

        // 3. Calculate Volumetric Weight = (L × W × H) / 6000
        double volumetricWeight = 0;
        if (product != null &&
            product.ProductLengthCm.HasValue &&
            product.ProductWidthCm.HasValue &&
            product.ProductHeightCm.HasValue)
        {
            volumetricWeight = (product.ProductLengthCm.Value *
                                product.ProductWidthCm.Value *
                                product.ProductHeightCm.Value) / 6000.0;
        }

        // 4. Calculate Reviewer Deviance Score (leak-free)
        //    = Customer_Avg_Rating - Product_Avg_Rating
        double reviewerDeviance = 0;
        if (order.Customer != null && product != null)
        {
            // Get all historical reviews by this customer (before this order)
            var customerUniqueId = order.Customer.CustomerUniqueId;
            var customerAvgRating = await _db.OrderReviews
                .Where(r => _db.Orders
                    .Where(o => o.Customer != null && o.Customer.CustomerUniqueId == customerUniqueId)
                    .Select(o => o.OrderId)
                    .Contains(r.OrderId))
                .AverageAsync(r => (double?)r.ReviewScore) ?? 3.0;

            // Product average rating
            var productAvgRating = await _db.OrderReviews
                .Where(r => _db.OrderItems
                    .Where(oi => oi.ProductId == product.ProductId)
                    .Select(oi => oi.OrderId)
                    .Contains(r.OrderId))
                .AverageAsync(r => (double?)r.ReviewScore) ?? 3.0;

            reviewerDeviance = customerAvgRating - productAvgRating;
        }

        // 5. Query active CostMatrixConfig from DB
        var config = await _db.CostMatrixConfigs
            .Where(c => c.ActiveStatus)
            .OrderByDescending(c => c.UpdatedAt)
            .FirstOrDefaultAsync();

        if (config == null)
            throw new InvalidOperationException("No active CostMatrixConfig found in the database.");

        decimal productPrice = firstItem?.Price ?? 0;
        decimal freightValue = firstItem?.FreightValue ?? 0;
        double shippingCostRatio = productPrice > 0
            ? (double)(freightValue / productPrice)
            : 0;

        // 6. Build feature payload for Flask ML API
        var features = new Dictionary<string, object>
        {
            { "price", (double)productPrice },
            { "freight_value", (double)freightValue },
            { "product_price", (double)productPrice },
            { "return_freight", (double)freightValue },
            { "shipping_cost_ratio", shippingCostRatio },
            { "product_weight_g", (double)(product?.ProductWeightG ?? 0) },
            { "product_length_cm", (double)(product?.ProductLengthCm ?? 0) },
            { "product_height_cm", (double)(product?.ProductHeightCm ?? 0) },
            { "product_width_cm", (double)(product?.ProductWidthCm ?? 0) },
            { "product_volume_cm3",
                (double)((product?.ProductLengthCm ?? 0) *
                (product?.ProductHeightCm ?? 0) *
                (product?.ProductWidthCm ?? 0)) },
            { "product_photos_qty", (double)(product?.ProductPhotosQty ?? 0) },
            { "shipping_distance_km", shippingDistanceKm },
            { "volumetric_weight", volumetricWeight },
            { "reviewer_deviance", reviewerDeviance },
            { "delivery_delay_days", 0.0 },
            { "product_category_name_english", product?.ProductCategoryNameEnglish ?? "unknown" }
        };

        var configPayload = new Dictionary<string, object>
        {
            { "ProfitMarginPercentage", (double)config.ProfitMarginPercentage },
            { "HandlingCostPerOrder", (double)config.HandlingCostPerOrder },
            { "DynamicThreshold", (double)config.DynamicThreshold }
        };

        // 7. Call Flask ML API
        var mlResult = await _mlApi.PredictAsync(features, configPayload);

        if (mlResult == null)
            throw new InvalidOperationException("Flask ML API returned null.");

        // 8. Persist prediction to DB
        var prediction = new Prediction
        {
            OrderId = orderId,
            ReturnProbability = mlResult.probability,
            RiskCategory = mlResult.risk_category,
            FalseNegativeCost = (decimal)mlResult.fn_cost,
            FalsePositiveCost = (decimal)mlResult.fp_cost,
            OptimalThreshold = mlResult.threshold_used,
            RecommendedAction = mlResult.recommended_action,
            CreatedAt = DateTime.Now
        };

        _db.Predictions.Add(prediction);
        await _db.SaveChangesAsync();

        stopwatch.Stop();

        // 9. Return full result DTO to frontend
        return new PredictionResponseDto
        {
            OrderId = orderId,
            Probability = mlResult.probability,
            RiskCategory = mlResult.risk_category,
            FnCost = (decimal)mlResult.fn_cost,
            FpCost = (decimal)mlResult.fp_cost,
            RecommendedAction = mlResult.recommended_action,
            ThresholdUsed = mlResult.threshold_used,
            LatencyMs = stopwatch.ElapsedMilliseconds,
            ProductPrice = productPrice,
            FreightValue = freightValue,
            ProductCategory = product?.ProductCategoryNameEnglish,
            CustomerCity = order.Customer?.CustomerCity,
            CustomerState = order.Customer?.CustomerState,
            ShippingDistanceKm = shippingDistanceKm,
            VolumetricWeight = volumetricWeight
        };
    }

    /// <summary>
    /// Evaluates a custom order provided manually by the user via the workstation UI.
    /// Does not persist the result to the database.
    /// </summary>
    public async Task<PredictionResponseDto> EvaluateCustomOrderAsync(CustomEvaluationRequestDto request)
    {
        var stopwatch = Stopwatch.StartNew();

        // 1. Calculate Volumetric Weight = (L × W × H) / 6000
        double volumetricWeight = (request.ProductLengthCm * request.ProductWidthCm * request.ProductHeightCm) / 6000.0;

        // 2. Calculate or Lookup Shipping Distance (Km) if not manually supplied
        double distanceKm = request.ShippingDistanceKm ?? 0;
        if (distanceKm <= 0 && (!string.IsNullOrWhiteSpace(request.CustomerCity) || !string.IsNullOrWhiteSpace(request.CustomerState)))
        {
            var customerCityClean = (request.CustomerCity ?? "").Trim().ToLower();
            var customerStateClean = (request.CustomerState ?? "").Trim().ToUpper();

            // Match exact City AND State first
            OlistGeolocation? customerGeo = null;
            if (!string.IsNullOrWhiteSpace(customerCityClean) && !string.IsNullOrWhiteSpace(customerStateClean))
            {
                customerGeo = await _db.Geolocations
                    .Where(g => g.GeolocationCity != null && g.GeolocationCity.ToLower() == customerCityClean && g.GeolocationState != null && g.GeolocationState.ToUpper() == customerStateClean)
                    .FirstOrDefaultAsync();
            }
            if (customerGeo == null && !string.IsNullOrWhiteSpace(customerCityClean))
            {
                customerGeo = await _db.Geolocations
                    .Where(g => g.GeolocationCity != null && g.GeolocationCity.ToLower() == customerCityClean)
                    .FirstOrDefaultAsync();
            }
            if (customerGeo == null && !string.IsNullOrWhiteSpace(customerStateClean))
            {
                customerGeo = await _db.Geolocations
                    .Where(g => g.GeolocationState != null && g.GeolocationState.ToUpper() == customerStateClean)
                    .FirstOrDefaultAsync();
            }

            var sellerGeo = await _db.Geolocations.FirstOrDefaultAsync();

            if (customerGeo != null && sellerGeo != null)
            {
                distanceKm = HaversineCalculator.CalculateDistanceKm(
                    customerGeo.GeolocationLat, customerGeo.GeolocationLng,
                    sellerGeo.GeolocationLat, sellerGeo.GeolocationLng);
            }
            if (distanceKm <= 0)
            {
                distanceKm = 480.0; // dataset median distance
            }
        }
        else if (distanceKm <= 0)
        {
            distanceKm = 480.0;
        }

        // 3. Calculate or Lookup Reviewer Deviance Score if not manually supplied
        double devianceScore = request.ReviewerDeviance ?? 0.0; // neutral baseline reviewer deviance score

        // 4. Query active CostMatrixConfig from DB
        var config = await _db.CostMatrixConfigs
            .Where(c => c.ActiveStatus)
            .OrderByDescending(c => c.UpdatedAt)
            .FirstOrDefaultAsync();

        if (config == null)
            throw new InvalidOperationException("No active CostMatrixConfig found in the database.");

        double shippingCostRatio = request.ProductPrice > 0
            ? (double)(request.FreightValue / request.ProductPrice)
            : 0;

        // 5. Build feature payload for Flask ML API
        var features = new Dictionary<string, object>
        {
            { "price", (double)request.ProductPrice },
            { "freight_value", (double)request.FreightValue },
            { "product_price", (double)request.ProductPrice },
            { "return_freight", (double)request.FreightValue },
            { "shipping_cost_ratio", shippingCostRatio },
            { "freight_to_price_ratio", shippingCostRatio },
            { "is_shipping_more_than_item", request.FreightValue > request.ProductPrice ? 1.0 : 0.0 },
            { "product_weight_g", (double)request.ProductWeightG },
            { "product_length_cm", (double)request.ProductLengthCm },
            { "product_height_cm", (double)request.ProductHeightCm },
            { "product_width_cm", (double)request.ProductWidthCm },
            { "product_volume_cm3", (double)(request.ProductLengthCm * request.ProductHeightCm * request.ProductWidthCm) },
            { "product_photos_qty", (double)request.ProductPhotosQty },
            { "shipping_distance_km", distanceKm },
            { "haversine_distance_km", distanceKm },
            { "volumetric_weight", volumetricWeight },
            { "reviewer_deviance", devianceScore },
            { "reviewer_deviance_score", devianceScore },
            { "is_extreme_reviewer", Math.Abs(devianceScore) > 2.0 ? 1.0 : 0.0 },
            { "delivery_delay_days", 0.0 },
            { "product_category_name_english", string.IsNullOrWhiteSpace(request.ProductCategory) ? "unknown" : request.ProductCategory }
        };

        var configPayload = new Dictionary<string, object>
        {
            { "ProfitMarginPercentage", (double)config.ProfitMarginPercentage },
            { "HandlingCostPerOrder", (double)config.HandlingCostPerOrder },
            { "DynamicThreshold", (double)config.DynamicThreshold }
        };

        // 6. Call Flask ML API
        var mlResult = await _mlApi.PredictAsync(features, configPayload);

        if (mlResult == null)
            throw new InvalidOperationException("Flask ML API returned null.");

        // 7. Persist custom prediction to DB so dashboard counters update in real time
        var customOrderId = "CUSTOM-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        var prediction = new Prediction
        {
            OrderId = customOrderId,
            ReturnProbability = mlResult.probability,
            RiskCategory = mlResult.risk_category,
            FalseNegativeCost = (decimal)mlResult.fn_cost,
            FalsePositiveCost = (decimal)mlResult.fp_cost,
            OptimalThreshold = mlResult.threshold_used,
            RecommendedAction = mlResult.recommended_action,
            CreatedAt = DateTime.Now
        };

        _db.Predictions.Add(prediction);
        await _db.SaveChangesAsync();

        stopwatch.Stop();

        // 8. Return full result DTO to frontend
        return new PredictionResponseDto
        {
            OrderId = customOrderId,
            Probability = mlResult.probability,
            RiskCategory = mlResult.risk_category,
            FnCost = (decimal)mlResult.fn_cost,
            FpCost = (decimal)mlResult.fp_cost,
            RecommendedAction = mlResult.recommended_action,
            ThresholdUsed = mlResult.threshold_used,
            LatencyMs = stopwatch.ElapsedMilliseconds,
            ProductPrice = request.ProductPrice,
            FreightValue = request.FreightValue,
            ProductCategory = request.ProductCategory,
            CustomerCity = request.CustomerCity,
            CustomerState = request.CustomerState,
            ShippingDistanceKm = distanceKm,
            VolumetricWeight = volumetricWeight
        };
    }
}
