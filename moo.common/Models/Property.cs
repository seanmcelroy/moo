using System;
using Newtonsoft.Json;

//[JsonConverter(typeof(PropertySerializer))]
[JsonObject(MemberSerialization.OptIn)]
public struct Property
{
    public enum PropertyType
    {
        Unknown = 0,
        String = 1,
        Integer = 2,
        DbRef = 3,
        Float = 4,
        Directory = 5,
        Lock = 6
    }

    [JsonProperty]
    public string Name;

    [JsonProperty]
    public PropertyDirectory? directory;

    [JsonProperty]
    public object? value;

    public object? Value
    {
        get
        {
            if (Type == PropertyType.DbRef)
            {
                if (value != null && value.GetType() == typeof(string))
                    return new Dbref((string)value);
            }
            return directory ?? value;
        }
        set
        {
            if (value is PropertyDirectory)
                this.directory = (PropertyDirectory)value;
            else
                this.value = value;
        }
    }

    [JsonProperty]
    public PropertyType Type;

    public Property(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new System.ArgumentNullException(nameof(name));

        this.Name = name;
        this.directory = null;
        this.value = value;
        this.Type = PropertyType.String;
    }

    public Property(string name, int value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new System.ArgumentNullException(nameof(name));

        this.Name = name;
        this.directory = null;
        this.value = value;
        this.Type = PropertyType.Integer;
    }

    public Property(string name, Lock value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new System.ArgumentNullException(nameof(name));
        if (default(Lock).Equals(value))
            throw new System.ArgumentNullException(nameof(value));

        this.Name = name;
        this.directory = null;
        this.value = value;
        this.Type = PropertyType.Lock;
    }

    public Property(string name, Dbref value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new System.ArgumentNullException(nameof(name));
        if (default(Dbref).Equals(value))
            throw new System.ArgumentNullException(nameof(value));

        this.Name = name;
        this.directory = null;
        this.value = value;
        this.Type = PropertyType.DbRef;
    }

    public Property(string name, float value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new System.ArgumentNullException(nameof(name));

        this.Name = name;
        this.directory = null;
        this.value = value;
        this.Type = PropertyType.Float;
    }
    public Property(string name, PropertyDirectory value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new System.ArgumentNullException(nameof(name));
        if (value == null)
            throw new System.ArgumentNullException(nameof(value));

        this.Name = name;
        this.directory = value;
        this.value = null;
        this.Type = PropertyType.Directory;
    }

    public string Serialize()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new System.InvalidOperationException("Property name is not set");

        switch (Type)
        {
            case PropertyType.String:
                return Serialize((string)Value);
            case PropertyType.Integer:
                return Serialize((int)Value);
            case PropertyType.DbRef:
                return Serialize((Dbref)Value, 0);
            case PropertyType.Lock:
                return Serialize((Lock)Value, 0);
            case PropertyType.Float:
                return Serialize((float)Value);
            case PropertyType.Directory:
                return PropertyDirectory.Serialize((PropertyDirectory)Value);
            default:
                throw new System.InvalidOperationException($"Unknown property type for {Name}: {Type}");
        }
    }

    public static string Serialize(Property prop)
    {
        if (PropertyType.DbRef == prop.Type)
            return $"<prop><name>{prop.Name}</name>" + Serialize((Dbref)prop.Value, 0) + "</prop>";
        if (typeof(string).IsAssignableFrom(prop.Value.GetType()))
            return $"<prop><name>{prop.Name}</name>" + Serialize((string)prop.Value) + "</prop>";
        if (typeof(int).IsAssignableFrom(prop.Value.GetType()))
            return $"<prop><name>{prop.Name}</name>" + Serialize((int)prop.Value) + "</prop>";
        if (typeof(long).IsAssignableFrom(prop.Value.GetType()))
            return $"<prop><name>{prop.Name}</name>" + Serialize(Convert.ToInt32((long)prop.Value)) + "</prop>";
        if (typeof(float).IsAssignableFrom(prop.Value.GetType()))
            return $"<prop><name>{prop.Name}</name>" + Serialize((float)prop.Value) + "</prop>";
        if (typeof(double).IsAssignableFrom(prop.Value.GetType()))
            return $"<prop><name>{prop.Name}</name>" + Serialize(Convert.ToSingle((double)prop.Value)) + "</prop>";
        if (typeof(PropertyDirectory).IsAssignableFrom(prop.Value.GetType()))
            return $"<prop><name>{prop.Name}</name>" + PropertyDirectory.Serialize((PropertyDirectory)prop.Value) + "</prop>";

        throw new System.InvalidOperationException($"Cannot handle object of type {prop.Type}");
    }

    public static string Serialize(Dbref value, byte dud) => Thing.Serialize(value, 0);
    public static string Serialize(Lock value, byte dud) => Thing.Serialize(value, 0);
    public static string Serialize(string value) => Thing.Serialize(value);
    public static string Serialize(float value) => Thing.Serialize(value);
    public static string Serialize(int value) => Thing.Serialize(value);

    public static bool TryInferType(string value, out Tuple<PropertyType, object> result)
    {
        if (int.TryParse(value.Trim(), out int i))
        {
            result = new Tuple<PropertyType, object>(PropertyType.Integer, i);
            return true;
        }

        if (float.TryParse(value.Trim(), out float f))
        {
            result = new Tuple<PropertyType, object>(PropertyType.Float, f);
            return true;
        }

        if (Dbref.TryParse(value.Trim(), out Dbref d))
        {
            result = new Tuple<PropertyType, object>(PropertyType.DbRef, d);
            return true;
        }

        if (value.StartsWith('\"') && value.EndsWith('\"'))
        {
            result = new Tuple<PropertyType, object>(PropertyType.String, value);
            return true;
        }

        result = default(Tuple<PropertyType, object>);
        return false;
    }
}