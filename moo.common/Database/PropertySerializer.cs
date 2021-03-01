
using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class PropertySerializer : JsonConverter
{
    public override bool CanConvert(Type objectType) => typeof(Property).IsAssignableFrom(objectType);

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        // Load JObject from stream
        var jObject = JObject.Load(reader);

        // Create target object based on JObject
        var target = new Property();

        // Populate the object properties
        serializer.Populate(jObject.CreateReader(), target);

        return target;
    }

    public override bool CanRead => true;
    public override bool CanWrite => false;

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => serializer.Serialize(writer, value);
}