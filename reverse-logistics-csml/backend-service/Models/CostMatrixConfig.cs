namespace BackendService.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("CostMatrixConfig")]
public class CostMatrixConfig
{
    [Key]
    [Column("ConfigId")]
    public int ConfigId { get; set; }

    [Column("ProfitMarginPercentage")]
    public decimal ProfitMarginPercentage { get; set; }

    [Column("HandlingCostPerOrder")]
    public decimal HandlingCostPerOrder { get; set; }

    [Column("DynamicThreshold")]
    public decimal DynamicThreshold { get; set; }

    [Column("ActiveStatus")]
    public bool ActiveStatus { get; set; }

    [Column("UpdatedAt")]
    public DateTime UpdatedAt { get; set; }
}
