namespace BackendService.DTOs;

/// <summary>
/// Response DTO returned to the Angular frontend after evaluating an order.
/// </summary>
public class PredictionResponseDto
{
    public string OrderId { get; set; } = string.Empty;
    public double Probability { get; set; }
    public string RiskCategory { get; set; } = string.Empty;
    public decimal FnCost { get; set; }
    public decimal FpCost { get; set; }
    public string RecommendedAction { get; set; } = string.Empty;
    public double ThresholdUsed { get; set; }
    public long LatencyMs { get; set; }

    // Order context for display
    public decimal ProductPrice { get; set; }
    public decimal FreightValue { get; set; }
    public string? ProductCategory { get; set; }
    public string? CustomerCity { get; set; }
    public string? CustomerState { get; set; }
    public double? ShippingDistanceKm { get; set; }
    public double? VolumetricWeight { get; set; }
}
