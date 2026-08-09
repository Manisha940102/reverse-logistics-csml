namespace BackendService.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendService.Data;
using BackendService.Models;
using BackendService.DTOs;

[ApiController]
[Route("api/orderitems")]
public class OrderItemsController : ControllerBase
{
    private readonly AppDbContext _db;

    public OrderItemsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var items = await _db.OrderItems
            .OrderBy(o => o.OrderId).ThenBy(o => o.OrderItemId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        var total = await _db.OrderItems.CountAsync();
        return Ok(new { data = items, total, page, pageSize });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderItemDto dto)
    {
        var entity = new OlistOrderItem
        {
            OrderId = string.IsNullOrWhiteSpace(dto.OrderId) ? Guid.NewGuid().ToString("N") : dto.OrderId,
            OrderItemId = dto.OrderItemId > 0 ? dto.OrderItemId : 1, // Simple default
            ProductId = dto.ProductId,
            SellerId = dto.SellerId,
            ShippingLimitDate = dto.ShippingLimitDate,
            Price = dto.Price,
            FreightValue = dto.FreightValue
        };

        _db.OrderItems.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), null, entity);
    }
}
