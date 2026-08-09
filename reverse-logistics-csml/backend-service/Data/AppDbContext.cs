namespace BackendService.Data;

using Microsoft.EntityFrameworkCore;
using BackendService.Models;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Olist dataset tables
    public DbSet<OlistCustomer> Customers => Set<OlistCustomer>();
    public DbSet<OlistGeolocation> Geolocations => Set<OlistGeolocation>();
    public DbSet<OlistOrder> Orders => Set<OlistOrder>();
    public DbSet<OlistProduct> Products => Set<OlistProduct>();
    public DbSet<OlistOrderItem> OrderItems => Set<OlistOrderItem>();
    public DbSet<OlistOrderReview> OrderReviews => Set<OlistOrderReview>();

    // Operational tables
    public DbSet<CostMatrixConfig> CostMatrixConfigs => Set<CostMatrixConfig>();
    public DbSet<Prediction> Predictions => Set<Prediction>();
    public DbSet<ModelComparison> ModelComparisons => Set<ModelComparison>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // We use a composite key so EF Core can track inserts/updates for the management module.
        modelBuilder.Entity<OlistGeolocation>(entity =>
        {
            entity.HasKey(e => new { e.GeolocationZipCodePrefix, e.GeolocationLat, e.GeolocationLng });
            entity.ToTable("olist_geolocation");
        });

        // Composite PK for order_items
        modelBuilder.Entity<OlistOrderItem>(entity =>
        {
            entity.HasKey(e => new { e.OrderId, e.OrderItemId });
        });

        // Composite PK for order_reviews
        modelBuilder.Entity<OlistOrderReview>(entity =>
        {
            entity.HasKey(e => new { e.ReviewId, e.OrderId });
        });

        // CostMatrixConfig: identity PK is auto-generated
        modelBuilder.Entity<CostMatrixConfig>(entity =>
        {
            entity.Property(e => e.ConfigId).ValueGeneratedOnAdd();
        });

        // Predictions: identity PK is auto-generated
        modelBuilder.Entity<Prediction>(entity =>
        {
            entity.Property(e => e.PredictionId).ValueGeneratedOnAdd();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETDATE()");
        });

        // ModelComparison: identity PK
        modelBuilder.Entity<ModelComparison>(entity =>
        {
            entity.Property(e => e.ModelId).ValueGeneratedOnAdd();
        });
    }
}
