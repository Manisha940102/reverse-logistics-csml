namespace BackendService.DTOs;

public class CreateCustomerDto
{
    public string? CustomerId { get; set; }
    public string? CustomerUniqueId { get; set; }
    public int CustomerZipCodePrefix { get; set; }
    public string? CustomerCity { get; set; }
    public string? CustomerState { get; set; }
}

public class CreateGeolocationDto
{
    public int GeolocationZipCodePrefix { get; set; }
    public double GeolocationLat { get; set; }
    public double GeolocationLng { get; set; }
    public string? GeolocationCity { get; set; }
    public string? GeolocationState { get; set; }
}

public class CreateOrderDto
{
    public string? OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string? OrderStatus { get; set; }
    public DateTime? OrderPurchaseTimestamp { get; set; }
    public DateTime? OrderApprovedAt { get; set; }
    public DateTime? OrderDeliveredCarrierDate { get; set; }
    public DateTime? OrderDeliveredCustomerDate { get; set; }
    public DateTime? OrderEstimatedDeliveryDate { get; set; }
}

public class CreateOrderItemDto
{
    public string OrderId { get; set; } = string.Empty;
    public int OrderItemId { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string SellerId { get; set; } = string.Empty;
    public DateTime? ShippingLimitDate { get; set; }
    public decimal Price { get; set; }
    public decimal FreightValue { get; set; }
}

public class CreateOrderReviewDto
{
    public string? ReviewId { get; set; }
    public string OrderId { get; set; } = string.Empty;
    public int ReviewScore { get; set; }
    public string? ReviewCommentTitle { get; set; }
    public string? ReviewCommentTitleEnglish { get; set; }
    public string? ReviewCommentMessage { get; set; }
    public string? ReviewCommentMessageEnglish { get; set; }
    public DateTime? ReviewCreationDate { get; set; }
    public DateTime? ReviewAnswerTimestamp { get; set; }
}

public class CreateProductDto
{
    public string? ProductId { get; set; }
    public string? ProductCategoryName { get; set; }
    public string? ProductCategoryNameEnglish { get; set; }
    public int? ProductNameLength { get; set; }
    public int? ProductDescriptionLength { get; set; }
    public int? ProductPhotosQty { get; set; }
    public double? ProductWeightG { get; set; }
    public double? ProductLengthCm { get; set; }
    public double? ProductHeightCm { get; set; }
    public double? ProductWidthCm { get; set; }
}
