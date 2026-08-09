namespace BackendService.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendService.Data;
using BackendService.Models;
using BackendService.DTOs;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ProductsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var products = await _db.Products
            .OrderBy(p => p.ProductId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        var total = await _db.Products.CountAsync();
        return Ok(new { data = products, total, page, pageSize });
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _db.Products
            .Where(p => !string.IsNullOrEmpty(p.ProductCategoryNameEnglish))
            .Select(p => p.ProductCategoryNameEnglish)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
        
        return Ok(categories);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        var entity = new OlistProduct
        {
            ProductId = string.IsNullOrWhiteSpace(dto.ProductId) ? Guid.NewGuid().ToString("N") : dto.ProductId,
            ProductCategoryName = dto.ProductCategoryName,
            ProductCategoryNameEnglish = dto.ProductCategoryNameEnglish,
            ProductNameLength = dto.ProductNameLength,
            ProductDescriptionLength = dto.ProductDescriptionLength,
            ProductPhotosQty = dto.ProductPhotosQty,
            ProductWeightG = dto.ProductWeightG,
            ProductLengthCm = dto.ProductLengthCm,
            ProductHeightCm = dto.ProductHeightCm,
            ProductWidthCm = dto.ProductWidthCm
        };

        _db.Products.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = entity.ProductId }, entity);
    }
}
