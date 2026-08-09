namespace BackendService.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("Predictions")]
public class Prediction
{
    [Key]
    [Column("PredictionId")]
    public int PredictionId { get; set; }

    [Column("OrderId")]
    [StringLength(50)]
    public string OrderId { get; set; } = string.Empty;

    [Column("ReturnProbability")]
    public double ReturnProbability { get; set; }

    [Column("RiskCategory")]
    [StringLength(10)]
    public string RiskCategory { get; set; } = string.Empty;

    [Column("FalseNegativeCost")]
    public decimal FalseNegativeCost { get; set; }

    [Column("FalsePositiveCost")]
    public decimal FalsePositiveCost { get; set; }

    [Column("OptimalThreshold")]
    public double OptimalThreshold { get; set; }

    [Column("RecommendedAction")]
    [StringLength(500)]
    public string RecommendedAction { get; set; } = string.Empty;

    [Column("CreatedAt")]
    public DateTime CreatedAt { get; set; }
}
