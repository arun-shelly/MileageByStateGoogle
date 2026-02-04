namespace MileageByStateGoogle.Utils;


public static class Haversine
{
    public static double Calculate(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 3958.8;
        double dLat = (lat2 - lat1) * Math.PI / 180;
        double dLon = (lon2 - lon1) * Math.PI / 180;
        lat1 *= Math.PI / 180;
        lat2 *= Math.PI / 180;

        double a = Math.Sin(dLat/2)*Math.Sin(dLat/2) +
                   Math.Cos(lat1)*Math.Cos(lat2) *
                   Math.Sin(dLon/2)*Math.Sin(dLon/2);

        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}
