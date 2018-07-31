
using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class PropertyDirectorySerializer : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return typeof(PropertyDirectory).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        return new Dbref(reader.Value.ToString());
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        PropertyDirectory propertyDirectory = (PropertyDirectory)value;
        writer.WriteValue(propertyDirectory.ToString());
    }
}