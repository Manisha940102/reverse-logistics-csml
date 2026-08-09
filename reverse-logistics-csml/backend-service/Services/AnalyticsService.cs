namespace BackendService.Services;

using Microsoft.EntityFrameworkCore;
using BackendService.Data;
using BackendService.DTOs;
using BackendService.Models;

/// <summary>
/// Service for analytics queries: model comparison benchmarks and prediction summaries.
/// All data is read dynamically from the database — never hardcoded.
/// </summary>
public class AnalyticsService
{
    private readonly AppDbContext _db;

    public AnalyticsService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Returns all model comparison benchmark rows from the ModelComparison table.
    /// </summary>
    public async Task<List<ModelComparison>> GetModelComparisonAsync()
    {
        return await _db.ModelComparisons
            .OrderBy(m => m.TotalFinancialLoss)
            .ToListAsync();
    }

    /// <summary>
    /// Returns aggregated prediction counts by risk category from the Predictions table.
    /// </summary>
    public async Task<PredictionsSummaryDto> GetPredictionsSummaryAsync()
    {
        var predictions = _db.Predictions;

        return new PredictionsSummaryDto
        {
            TotalPredictions = await predictions.CountAsync(),
            GreenCount = await predictions.CountAsync(p => p.RiskCategory == "Green"),
            YellowCount = await predictions.CountAsync(p => p.RiskCategory == "Yellow"),
            RedCount = await predictions.CountAsync(p => p.RiskCategory == "Red"),
            TotalFnCost = await predictions.SumAsync(p => p.FalseNegativeCost),
            TotalFpCost = await predictions.SumAsync(p => p.FalsePositiveCost)
        };
    }

    /// <summary>
    /// Returns paginated prediction history from the Predictions table.
    /// </summary>
    public async Task<List<Prediction>> GetPredictionHistoryAsync(int page = 1, int pageSize = 20)
    {
        return await _db.Predictions
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
