using Newtonsoft.Json;

namespace Glaz.Server.Data.Vuforia.Responses
{
    public sealed class GetTargetResponse : CommonResponse
    {
        [JsonProperty("target_record")]
        public TargetRecord TargetRecord { get; set; }

        public string Status { get; set; }
    }
}