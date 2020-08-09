using Newtonsoft.Json;

namespace Glaz.Server.Data.Vuforia.Responses
{
    public sealed class CreateTargetResponse : CommonResponse
    {
        [JsonProperty("target_id")]
        public string TargetId { get; set; }
    }
}