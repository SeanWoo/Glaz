using Glaz.Server.Data.JsonConverters;
using Newtonsoft.Json;

namespace Glaz.Server.Data.Vuforia.Responses
{
    public class CommonResponse
    {
        [JsonProperty("result_code")]
        [JsonConverter(typeof(VuforiaSuccessCodeToBooleanConverter))]
        public bool IsSuccess { get; set; }

        [JsonProperty("transaction_id")]
        public string TransactionId { get; set; }
    }
}
