namespace MileageByStateGoogle.Models
{
    public class DirectionsResponse
    {
        public string status { get; set; }   // <── THIS FIXES THE ERROR
        public List<Route> routes { get; set; }
    }

    public class Route
    {
        public OverviewPolyline overview_polyline { get; set; }
        public List<Leg> legs { get; set; }
    }

    public class Leg
    {
        public Distance distance { get; set; }
        public Duration duration { get; set; }
    }

    public class Distance
    {
        public string text { get; set; }
        public int value { get; set; }   // meters
    }

    public class Duration
    {
        public string text { get; set; }
        public int value { get; set; }   // seconds
    }

    public class OverviewPolyline
    {
        public string points { get; set; }
    }
}