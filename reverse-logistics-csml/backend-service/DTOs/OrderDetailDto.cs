namespace BackendService.DTOs;

/// <summary>
/// Lightweight DTO for listing orders on the frontend.
/// </summary>
public class OrderDetailDto
{
    public string OrderId { get; set; } = string.Empty;
    public string? OrderStatus { get; set; }
    public DateTime? OrderPurchaseTimestamp { get; set; }
    public string? CustomerCity { get; set; }
    public string? CustomerState { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal TotalFreight { get; set; }
    public int ItemCount { get; set; }
}
