namespace BackendService.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendService.Data;
using BackendService.Models;
using BackendService.DTOs;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _db;

    public CustomersController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 50, [FromQuery] string? search = null)
    {
        var query = _db.Customers.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(c => c.CustomerId.Contains(search));
        }

        var total = await query.CountAsync();

        var customers = await query
            .OrderBy(c => c.CustomerId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        return Ok(new { data = customers, total, page, pageSize });
    }

    [HttpGet("cities")]
    public async Task<IActionResult> GetCities()
    {
        var cities = await _db.Customers
            .Where(c => !string.IsNullOrEmpty(c.CustomerCity))
            .Select(c => c.CustomerCity)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
        return Ok(cities);
    }

    [HttpGet("states")]
    public async Task<IActionResult> GetStates([FromQuery] string? city)
    {
        var query = _db.Customers.AsQueryable();

        if (!string.IsNullOrEmpty(city))
        {
            query = query.Where(c => c.CustomerCity == city);
        }

        var states = await query
            .Where(c => !string.IsNullOrEmpty(c.CustomerState))
            .Select(c => c.CustomerState)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();
        return Ok(states);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCustomerDto dto)
    {
        var entity = new OlistCustomer
        {
            CustomerId = string.IsNullOrWhiteSpace(dto.CustomerId) ? Guid.NewGuid().ToString("N") : dto.CustomerId,
            CustomerUniqueId = string.IsNullOrWhiteSpace(dto.CustomerUniqueId) ? Guid.NewGuid().ToString("N") : dto.CustomerUniqueId,
            CustomerZipCodePrefix = dto.CustomerZipCodePrefix,
            CustomerCity = dto.CustomerCity,
            CustomerState = dto.CustomerState
        };

        _db.Customers.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = entity.CustomerId }, entity);
    }
}
