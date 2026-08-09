namespace BackendService.Helpers;

/// <summary>
/// Calculates the great-circle distance between two geographic points
/// using the Haversine formula, as described in the MSc thesis for
/// computing shipping distance between customer and seller locations.
/// </summary>
public static class HaversineCalculator
{
    private const double EarthRadiusKm = 6371.0;

    /// <summary>
    /// Calculate distance in kilometres between two lat/lng points.
    /// </summary>
    public static double CalculateDistanceKm(double lat1, double lng1, double lat2, double lng2)
    {
        var dLat = DegreesToRadians(lat2 - lat1);
        var dLng = DegreesToRadians(lng2 - lng1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(lat1)) * Math.Cos(DegreesToRadians(lat2)) *
                Math.Sin(dLng / 2) * Math.Sin(dLng / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return EarthRadiusKm * c;
    }

    private static double DegreesToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }
}
