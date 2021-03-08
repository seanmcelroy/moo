using System;
using moo.common.Models;
using Newtonsoft.Json;

namespace moo.common.Database
{
    public class LockSerializer : JsonConverter
    {
        public override bool CanConvert(Type objectType) => typeof(Lock).IsAssignableFrom(objectType);

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) => new Lock(reader.Value!.ToString()!);

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => writer.WriteValue(((Lock)value!).ToString());
    }
}