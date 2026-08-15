namespace BackendService.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("ModelComparison")]
public class ModelComparison
{
    [Key]
    [Column("ModelId")]
    public int ModelId { get; set; }

    [Column("ModelName")]
    [StringLength(100)]
    public string ModelName { get; set; } = string.Empty;

    [Column("ClassificationAccuracy")]
    public double ClassificationAccuracy { get; set; }

    [Column("PrecisionScore")]
    public double PrecisionScore { get; set; }

    [Column("RecallScore")]
    public double RecallScore { get; set; }

    [Column("F1Score")]
    public double F1Score { get; set; }

    [Column("AUCROC")]
    public double AucRoc { get; set; }

    [Column("FalseNegativesCount")]
    public int FalseNegativesCount { get; set; }

    [Column("FalsePositivesCount")]
    public int FalsePositivesCount { get; set; }

    [Column("TotalFinancialLoss")]
    public decimal TotalFinancialLoss { get; set; }

    [Column("LossPerOrder")]
    public decimal? OperationalSavings { get; set; }

    [Column("IsOptimal")]
    public bool IsOptimal { get; set; }
}
