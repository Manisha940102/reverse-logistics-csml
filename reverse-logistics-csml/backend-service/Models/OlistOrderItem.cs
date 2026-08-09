namespace BackendService.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("olist_order_items")]
public class OlistOrderItem
{
    [Column("order_id")]
    [StringLength(50)]
    public string OrderId { get; set; } = string.Empty;

    [Column("order_item_id")]
    public int OrderItemId { get; set; }

    [Column("product_id")]
    [StringLength(50)]
    public string ProductId { get; set; } = string.Empty;

    [Column("seller_id")]
    [StringLength(50)]
    public string SellerId { get; set; } = string.Empty;

    [Column("shipping_limit_date")]
    public DateTime? ShippingLimitDate { get; set; }

    [Column("price")]
    public decimal Price { get; set; }

    [Column("freight_value")]
    public decimal FreightValue { get; set; }

    // Navigation
    [ForeignKey("OrderId")]
    public OlistOrder? Order { get; set; }

    [ForeignKey("ProductId")]
    public OlistProduct? Product { get; set; }
}
