using Newtonsoft.Json;

namespace Glaz.Server.Data.Vuforia
{
    public sealed class TargetModel
    {
        public const float DefaultWidth = 10f;

        public string Name { get; set; }

        public float Width { get; set; }

        [JsonProperty("image")]
        public string ImageBase64 { get; set; }
    }
}