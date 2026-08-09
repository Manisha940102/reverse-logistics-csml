namespace BackendService.Controllers;

using Microsoft.AspNetCore.Mvc;
using BackendService.DTOs;
using BackendService.Services;
using BackendService.Data;
using BackendService.Models;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// OrdersController - GUIDE.md Section 3, Item 1
/// GET /api/orders/pending  — Fetches pending orders for evaluation
/// POST /api/orders/evaluate/{orderId} — Full evaluation pipeline
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;
    private readonly AppDbContext _db;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(OrderService orderService, AppDbContext db, ILogger<OrdersController> logger)
    {
        _orderService = orderService;
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var orders = await _db.Orders
            .OrderByDescending(o => o.OrderPurchaseTimestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        var total = await _db.Orders.CountAsync();
        return Ok(new { data = orders, total, page, pageSize });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
    {
        var entity = new OlistOrder
        {
            OrderId = string.IsNullOrWhiteSpace(dto.OrderId) ? Guid.NewGuid().ToString("N") : dto.OrderId,
            CustomerId = dto.CustomerId,
            OrderStatus = dto.OrderStatus,
            OrderPurchaseTimestamp = dto.OrderPurchaseTimestamp ?? DateTime.Now,
            OrderApprovedAt = dto.OrderApprovedAt,
            OrderDeliveredCarrierDate = dto.OrderDeliveredCarrierDate,
            OrderDeliveredCustomerDate = dto.OrderDeliveredCustomerDate,
            OrderEstimatedDeliveryDate = dto.OrderEstimatedDeliveryDate
        };

        _db.Orders.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = entity.OrderId }, entity);
    }

    /// <summary>
    /// GET /api/orders/pending?page=1&pageSize=20
    /// Returns paginated list of orders available for evaluation.
    /// </summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var orders = await _orderService.GetPendingOrdersAsync(page, pageSize);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch pending orders");
            return StatusCode(500, new { error = "Failed to fetch orders", detail = ex.Message });
        }
    }

    /// <summary>
    /// POST /api/orders/evaluate/{orderId}
    /// Executes the full evaluation pipeline:
    ///   DB query → Haversine → Volumetric Weight → Reviewer Deviance
    ///   → CostMatrixConfig → Flask ML API → Persist → Return DTO
    /// </summary>
    [HttpPost("evaluate/{orderId}")]
    public async Task<IActionResult> EvaluateOrder(string orderId)
    {
        try
        {
            _logger.LogInformation("Evaluating order: {OrderId}", orderId);
            var result = await _orderService.EvaluateOrderAsync(orderId);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Flask ML API communication error for order {OrderId}", orderId);
            return StatusCode(502, new { error = "ML Service unavailable", detail = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error evaluating order {OrderId}", orderId);
            return StatusCode(500, new { error = "Evaluation failed", detail = ex.Message });
        }
    }

    /// <summary>
    /// POST /api/orders/evaluate-custom
    /// Evaluates a custom order provided manually by the user via the workstation UI.
    /// </summary>
    [HttpPost("evaluate-custom")]
    public async Task<IActionResult> EvaluateCustomOrder([FromBody] CustomEvaluationRequestDto request)
    {
        try
        {
            _logger.LogInformation("Evaluating custom order");
            var result = await _orderService.EvaluateCustomOrderAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Flask ML API communication error for custom order evaluation");
            return StatusCode(502, new { error = "ML Service unavailable", detail = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error evaluating custom order");
            return StatusCode(500, new { error = "Evaluation failed", detail = ex.Message });
        }
    }
}
