using Newtonsoft.Json;

namespace Glaz.Server.Data.Vuforia
{
    public sealed class TargetModel
    {
        public string Name { get; set; }

        public float Width { get; set; }

        [JsonProperty("image")]
        public string ImageBase64 { get; set; }

        [JsonProperty("application_metadata")]
        public string ApplicationMetadataBase64 { get; set; }

        [JsonProperty("active_flag")]
        public bool ActiveFlag { get; set; }
    }
}