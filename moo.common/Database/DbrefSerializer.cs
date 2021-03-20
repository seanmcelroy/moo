using System;
using moo.common.Models;
using Newtonsoft.Json;

namespace moo.common.Database
{
    public class DbrefSerializer : JsonConverter
    {
        public override bool CanConvert(Type objectType) => typeof(Dbref).IsAssignableFrom(objectType);

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) => reader.Value == null ? Dbref.NOT_FOUND : new Dbref(reader.Value.ToString()!);

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => writer.WriteValue(((Dbref)(value ?? Dbref.NOT_FOUND)).ToString());
    }
}