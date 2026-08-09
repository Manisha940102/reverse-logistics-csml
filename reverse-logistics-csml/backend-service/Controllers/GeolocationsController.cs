namespace BackendService.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendService.Data;
using BackendService.Models;
using BackendService.DTOs;

[ApiController]
[Route("api/[controller]")]
public class GeolocationsController : ControllerBase
{
    private readonly AppDbContext _db;

    public GeolocationsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var geolocations = await _db.Geolocations
            .OrderBy(g => g.GeolocationZipCodePrefix)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        
        var total = await _db.Geolocations.CountAsync();
        return Ok(new { data = geolocations, total, page, pageSize });
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

    [HttpGet("zipcode")]
    public async Task<IActionResult> GetZipCode([FromQuery] string city, [FromQuery] string state)
    {
        var zip = await _db.Customers
            .Where(c => c.CustomerCity == city && c.CustomerState == state)
            .Select(c => c.CustomerZipCodePrefix)
            .FirstOrDefaultAsync();
            
        return Ok(new { zipCodePrefix = zip });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateGeolocationDto dto)
    {
        var entity = new OlistGeolocation
        {
            GeolocationZipCodePrefix = dto.GeolocationZipCodePrefix,
            GeolocationLat = dto.GeolocationLat,
            GeolocationLng = dto.GeolocationLng,
            GeolocationCity = dto.GeolocationCity,
            GeolocationState = dto.GeolocationState
        };

        _db.Geolocations.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), null, entity);
    }
}
