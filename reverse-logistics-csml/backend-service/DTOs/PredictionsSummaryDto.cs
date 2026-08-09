namespace BackendService.DTOs;

/// <summary>
/// Aggregate counts of Green/Yellow/Red predictions from the database.
/// </summary>
public class PredictionsSummaryDto
{
    public int TotalPredictions { get; set; }
    public int GreenCount { get; set; }
    public int YellowCount { get; set; }
    public int RedCount { get; set; }
    public decimal TotalFnCost { get; set; }
    public decimal TotalFpCost { get; set; }
}
