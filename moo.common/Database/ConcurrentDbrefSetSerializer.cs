using System;
using System.Collections.Generic;
using moo.common.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace moo.common.Database
{
    public class ConcurrentDbrefSetSerializer : JsonConverter
    {
        public override bool CanConvert(Type objectType) => typeof(ConcurrentDbrefSet).IsAssignableFrom(objectType);

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            //if (reader.Value == null)
            //    return null;

            JArray array = JArray.Load(reader);
            var dbrefs = array.ToObject<IList<Dbref>>();
            return new ConcurrentDbrefSet(dbrefs);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            var cds = (ConcurrentDbrefSet?)value;
            if (cds != null)
            {
                var array = cds.ToImmutableArray();
                writer.WriteStartArray();
                foreach (var item in array)
                {
                    serializer.Serialize(writer, item);
                }
                writer.WriteEndArray();
            }
        }
    }
}