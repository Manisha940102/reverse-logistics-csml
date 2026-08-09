namespace BackendService.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("olist_products")]
public class OlistProduct
{
    [Key]
    [Column("product_id")]
    [StringLength(50)]
    public string ProductId { get; set; } = string.Empty;

    [Column("product_category_name")]
    [StringLength(100)]
    public string? ProductCategoryName { get; set; }

    [Column("product_category_name_english")]
    [StringLength(100)]
    public string? ProductCategoryNameEnglish { get; set; }

    [Column("product_name_lenght")] // Original Olist typo preserved
    public int? ProductNameLength { get; set; }

    [Column("product_description_lenght")] // Original Olist typo preserved
    public int? ProductDescriptionLength { get; set; }

    [Column("product_photos_qty")]
    public int? ProductPhotosQty { get; set; }

    [Column("product_weight_g")]
    public double? ProductWeightG { get; set; }

    [Column("product_length_cm")]
    public double? ProductLengthCm { get; set; }

    [Column("product_height_cm")]
    public double? ProductHeightCm { get; set; }

    [Column("product_width_cm")]
    public double? ProductWidthCm { get; set; }
}
