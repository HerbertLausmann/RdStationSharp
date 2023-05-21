using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace RdStationSharp.Helper
{
    class IntJsonConverter : JsonConverter<int?>
    {
        public override int? ReadJson(JsonReader reader, Type objectType, int? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            int? s = Helper.TryParseStringToInt(reader.Value?.ToString());
            return s;
        }

        public override void WriteJson(JsonWriter writer, int? value, JsonSerializer serializer)
        {
            writer.WriteValue(value?.ToString());
        }
    }
}
