namespace BackendService.Controllers;

using Microsoft.AspNetCore.Mvc;
using BackendService.Services;

/// <summary>
/// AnalyticsController - GUIDE.md Section 3, Item 3
/// GET /api/analytics/model-comparison     — Benchmark data for the dashboard
/// GET /api/analytics/predictions-summary  — Aggregate Green/Yellow/Red counts
/// GET /api/analytics/predictions-history  — Paginated prediction log
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly AnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(AnalyticsService analyticsService, ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/analytics/model-comparison
    /// Returns all model comparison rows from the ModelComparison table.
    /// </summary>
    [HttpGet("model-comparison")]
    public async Task<IActionResult> GetModelComparison()
    {
        try
        {
            var models = await _analyticsService.GetModelComparisonAsync();
            return Ok(models);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch model comparison data");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/analytics/predictions-summary
    /// Returns aggregate counts of Green, Yellow, Red predictions.
    /// </summary>
    [HttpGet("predictions-summary")]
    public async Task<IActionResult> GetPredictionsSummary()
    {
        try
        {
            var summary = await _analyticsService.GetPredictionsSummaryAsync();
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch predictions summary");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// GET /api/analytics/predictions-history?page=1&pageSize=20
    /// Returns paginated prediction history from the Predictions table.
    /// </summary>
    [HttpGet("predictions-history")]
    public async Task<IActionResult> GetPredictionsHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var predictions = await _analyticsService.GetPredictionHistoryAsync(page, pageSize);
            return Ok(predictions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch predictions history");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
