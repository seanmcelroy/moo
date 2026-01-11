
using System;
using moo.common.Models;
using Newtonsoft.Json;

namespace moo.common.Database
{
    public class PropertyDirectorySerializer : JsonConverter
    {
        public override bool CanConvert(Type objectType) => typeof(PropertyDirectory).IsAssignableFrom(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) => new Dbref(reader.Value.ToString());

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
                return;
            writer.WriteValue(((PropertyDirectory)value).ToString());
        }
    }
}