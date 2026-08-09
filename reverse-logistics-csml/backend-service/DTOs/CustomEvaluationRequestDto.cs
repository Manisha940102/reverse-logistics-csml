namespace BackendService.DTOs;

/// <summary>
/// DTO for requesting a prediction using custom manual inputs from the workstation.
/// </summary>
public class CustomEvaluationRequestDto
{
    public decimal ProductPrice { get; set; }
    public decimal FreightValue { get; set; }
    public double ProductWeightG { get; set; }
    public double ProductLengthCm { get; set; }
    public double ProductHeightCm { get; set; }
    public double ProductWidthCm { get; set; }
    public int ProductPhotosQty { get; set; }
    public double? ShippingDistanceKm { get; set; }
    public double? ReviewerDeviance { get; set; }
    public string ProductCategory { get; set; } = string.Empty;
    public string CustomerCity { get; set; } = string.Empty;
    public string CustomerState { get; set; } = string.Empty;
}
