namespace BackendService.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendService.Data;
using BackendService.Models;
using BackendService.DTOs;

[ApiController]
[Route("api/orderreviews")]
public class OrderReviewsController : ControllerBase
{
    private readonly AppDbContext _db;

    public OrderReviewsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var reviews = await _db.OrderReviews
            .OrderBy(o => o.ReviewId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        var total = await _db.OrderReviews.CountAsync();
        return Ok(new { data = reviews, total, page, pageSize });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderReviewDto dto)
    {
        var entity = new OlistOrderReview
        {
            ReviewId = string.IsNullOrWhiteSpace(dto.ReviewId) ? Guid.NewGuid().ToString("N") : dto.ReviewId,
            OrderId = dto.OrderId,
            ReviewScore = dto.ReviewScore,
            ReviewCommentTitle = dto.ReviewCommentTitle,
            ReviewCommentTitleEnglish = dto.ReviewCommentTitleEnglish,
            ReviewCommentMessage = dto.ReviewCommentMessage,
            ReviewCommentMessageEnglish = dto.ReviewCommentMessageEnglish,
            ReviewCreationDate = dto.ReviewCreationDate ?? DateTime.Now,
            ReviewAnswerTimestamp = dto.ReviewAnswerTimestamp
        };

        _db.OrderReviews.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), null, entity);
    }
}
