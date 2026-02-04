namespace MileageByStateGoogle.Utils;

public static class PolylineDecoder
{
    public static List<(double lat, double lon)> Decode(string polyline)
    {
        var list = new List<(double, double)>();
        int index = 0, lat = 0, lng = 0;

        while (index < polyline.Length)
        {
            int b, shift = 0, result = 0;
            do { b = polyline[index++] - 63; result |= (b & 0x1f) << shift; shift += 5; }
            while (b >= 0x20);
            lat += ((result & 1) != 0 ? ~(result >> 1) : result >> 1);

            shift = 0; result = 0;
            do { b = polyline[index++] - 63; result |= (b & 0x1f) << shift; shift += 5; }
            while (b >= 0x20);
            lng += ((result & 1) != 0 ? ~(result >> 1) : result >> 1);

            list.Add((lat / 1E5, lng / 1E5));
        }

        return list;
    }
}