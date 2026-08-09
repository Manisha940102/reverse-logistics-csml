namespace BackendService.Models;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("olist_geolocation")]
public class OlistGeolocation
{
    [Column("geolocation_zip_code_prefix")]
    public int GeolocationZipCodePrefix { get; set; }

    [Column("geolocation_lat")]
    public double GeolocationLat { get; set; }

    [Column("geolocation_lng")]
    public double GeolocationLng { get; set; }

    [Column("geolocation_city")]
    [StringLength(100)]
    public string? GeolocationCity { get; set; }

    [Column("geolocation_state")]
    [StringLength(5)]
    public string? GeolocationState { get; set; }
}
