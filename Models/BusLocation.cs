using Newtonsoft.Json;

namespace BusTracker.Models
{
    public partial class BusLocation
    {
        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        [JsonProperty("previousStop")]
        public string PreviousStop { get; set; }

        [JsonProperty("nextStop")]
        public string NextStop { get; set; }

        [JsonProperty("atStop")]
        public string AtStop { get; set; }
    }
}