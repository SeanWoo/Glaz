using Newtonsoft.Json;

namespace Glaz.Server.Data.Vuforia.Responses
{
    public sealed class GetTargetResponse : CreateTargetResponse
    {
        [JsonProperty("target_record")]
        public TargetRecord TargetRecord { get; set; }
    }
}