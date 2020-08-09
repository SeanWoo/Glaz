using Newtonsoft.Json;

namespace Glaz.Server.Data.Vuforia.Responses
{
    public class CreateTargetResponse : CommonResponse
    {
        [JsonProperty("target_id")]
        public string TargetId { get; set; }
    }
}