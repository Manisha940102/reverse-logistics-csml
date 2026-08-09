namespace BackendService.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackendService.Data;
using BackendService.Models;

/// <summary>
/// ConfigurationController - GUIDE.md Section 3, Item 2
/// GET  /api/config — Reads active parameters from CostMatrixConfig table
/// PUT  /api/config — Updates parameters dynamically (no code rebuild needed)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ConfigurationController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<ConfigurationController> _logger;

    public ConfigurationController(AppDbContext db, ILogger<ConfigurationController> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/config
    /// Returns the currently active CostMatrixConfig from the database.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetActiveConfig()
    {
        var config = await _db.CostMatrixConfigs
            .Where(c => c.ActiveStatus)
            .OrderByDescending(c => c.UpdatedAt)
            .FirstOrDefaultAsync();

        if (config == null)
            return NotFound(new { error = "No active configuration found." });

        return Ok(config);
    }

    /// <summary>
    /// PUT /api/config
    /// Updates the active configuration parameters in the database.
    /// Allows logistics managers to adjust business logic without rebuilding code.
    /// </summary>
    [HttpPut]
    public async Task<IActionResult> UpdateConfig([FromBody] UpdateConfigRequest request)
    {
        var config = await _db.CostMatrixConfigs
            .Where(c => c.ActiveStatus)
            .OrderByDescending(c => c.UpdatedAt)
            .FirstOrDefaultAsync();

        if (config == null)
            return NotFound(new { error = "No active configuration found to update." });

        // Update fields
        if (request.ProfitMarginPercentage.HasValue)
            config.ProfitMarginPercentage = request.ProfitMarginPercentage.Value;

        if (request.HandlingCostPerOrder.HasValue)
            config.HandlingCostPerOrder = request.HandlingCostPerOrder.Value;

        if (request.DynamicThreshold.HasValue)
            config.DynamicThreshold = request.DynamicThreshold.Value;

        config.UpdatedAt = DateTime.Now;

        await _db.SaveChangesAsync();

        _logger.LogInformation(
            "CostMatrixConfig updated: Margin={Margin}, Handling={Handling}, Threshold={Threshold}",
            config.ProfitMarginPercentage, config.HandlingCostPerOrder, config.DynamicThreshold);

        return Ok(config);
    }
}

/// <summary>
/// Request body for PUT /api/config — all fields optional so partial updates work.
/// </summary>
public class UpdateConfigRequest
{
    public decimal? ProfitMarginPercentage { get; set; }
    public decimal? HandlingCostPerOrder { get; set; }
    public decimal? DynamicThreshold { get; set; }
}
