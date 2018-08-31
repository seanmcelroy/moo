
using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class PropertyDirectorySerializer : JsonConverter
{
    public override bool CanConvert(Type objectType) => typeof(PropertyDirectory).IsAssignableFrom(objectType);

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) => new Dbref(reader.Value.ToString());

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => writer.WriteValue(((PropertyDirectory)value).ToString());
}