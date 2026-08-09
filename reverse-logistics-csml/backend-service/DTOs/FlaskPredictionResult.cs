namespace BackendService.DTOs;

/// <summary>
/// DTO representing the JSON response from the Flask ML API /predict endpoint.
/// </summary>
public class FlaskPredictionResult
{
    public double probability { get; set; }
    public string risk_category { get; set; } = string.Empty;
    public double fn_cost { get; set; }
    public double fp_cost { get; set; }
    public string recommended_action { get; set; } = string.Empty;
    public double threshold_used { get; set; }
    public string? error { get; set; }
}
