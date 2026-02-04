namespace MileageByStateGoogle.Models
{
    public class GeocodeResponse
    {
        public List<GeocodeResult> results { get; set; }
    }

    public class GeocodeResult
    {
        public List<AddressComponent> address_components { get; set; }
    }

    public class AddressComponent
    {
        public string long_name { get; set; }
        public string short_name { get; set; }
        public List<string> types { get; set; }
    }
}