namespace BackendService.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("olist_orders")]
public class OlistOrder
{
    [Key]
    [Column("order_id")]
    [StringLength(50)]
    public string OrderId { get; set; } = string.Empty;

    [Column("customer_id")]
    [StringLength(50)]
    public string CustomerId { get; set; } = string.Empty;

    [Column("order_status")]
    [StringLength(20)]
    public string? OrderStatus { get; set; }

    [Column("order_purchase_timestamp")]
    public DateTime? OrderPurchaseTimestamp { get; set; }

    [Column("order_approved_at")]
    public DateTime? OrderApprovedAt { get; set; }

    [Column("order_delivered_carrier_date")]
    public DateTime? OrderDeliveredCarrierDate { get; set; }

    [Column("order_delivered_customer_date")]
    public DateTime? OrderDeliveredCustomerDate { get; set; }

    [Column("order_estimated_delivery_date")]
    public DateTime? OrderEstimatedDeliveryDate { get; set; }

    // Navigation
    [ForeignKey("CustomerId")]
    public OlistCustomer? Customer { get; set; }

    public ICollection<OlistOrderItem> OrderItems { get; set; } = new List<OlistOrderItem>();
    public ICollection<OlistOrderReview> OrderReviews { get; set; } = new List<OlistOrderReview>();
}
