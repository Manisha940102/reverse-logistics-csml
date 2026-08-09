namespace BackendService.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("olist_customers")]
public class OlistCustomer
{
    [Key]
    [Column("customer_id")]
    [StringLength(50)]
    public string CustomerId { get; set; } = string.Empty;

    [Column("customer_unique_id")]
    [StringLength(50)]
    public string CustomerUniqueId { get; set; } = string.Empty;

    [Column("customer_zip_code_prefix")]
    public int CustomerZipCodePrefix { get; set; }

    [Column("customer_city")]
    [StringLength(100)]
    public string? CustomerCity { get; set; }

    [Column("customer_state")]
    [StringLength(5)]
    public string? CustomerState { get; set; }

    // Navigation
    public ICollection<OlistOrder> Orders { get; set; } = new List<OlistOrder>();
}
