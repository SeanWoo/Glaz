using Newtonsoft.Json;

namespace Glaz.Server.Data.Vuforia.Responses
{
    public sealed class TargetRecord
    {
        [JsonProperty("target_id")]
        public string TargetId { get; set; }

        [JsonProperty("active_flag")]
        public bool ActiveFlag { get; set; }

        public string Name { get; set; }

        public float Width { get; set; }

        [JsonProperty("tracking_rating")]
        public int TrackingRating { get; set; }
    }
}