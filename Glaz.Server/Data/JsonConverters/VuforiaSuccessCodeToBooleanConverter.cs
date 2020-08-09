using System;
using Newtonsoft.Json;

namespace Glaz.Server.Data.JsonConverters
{
    public sealed class VuforiaSuccessCodeToBooleanConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            // We would not send Vuforia's responses — only read
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var value = reader.Value;

            if (value is null || string.IsNullOrEmpty(value.ToString()))
            {
                return false;
            }

            return "Success".Equals(value.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string) || objectType == typeof(bool);
        }
    }
}