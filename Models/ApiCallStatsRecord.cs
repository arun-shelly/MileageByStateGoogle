using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MileageByStateGoogle.Models
{
    public class ApiCallStatsRecord
    {
        public string travel_id { get; set; }              // Travel ID
        public int DirectionsCalls { get; set; }           // Number of Directions API calls
        public int GeocodeCalls { get; set; }              // Number of Geocoding API calls
        public int TotalApiCalls { get; set; }             // Combined API calls

        // Optional: override ToString for debugging
        public override string ToString()
        {
            return $"{travel_id}: Directions={DirectionsCalls}, Geocode={GeocodeCalls}, Total={TotalApiCalls}";
        }
    }
}