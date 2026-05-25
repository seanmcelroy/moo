using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using moo.common.Scripting;

namespace moo.common.Models
{
    // TODO: In the future with .NET 11 / C# langversion 15.0, make this a union type
    [DebuggerDisplay("{Name}={(directory != null ? \"directory\" : value)} ({Type})")]
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
            Lock = 6,
            Array = 9
        }

        public string Name;

        public PropertyDirectory? directory;

        public object? value;

        public object? Value
        {
            readonly get
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
                if (value is PropertyDirectory directory1)
                    this.directory = directory1;
                else
                    this.value = value;
            }
        }

        public PropertyType Type;

        public Property(string name, string value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            this.Name = name;
            this.directory = null;
            this.value = value;
            this.Type = PropertyType.String;
        }

        public Property(string name, int value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            this.Name = name;
            this.directory = null;
            this.value = value;
            this.Type = PropertyType.Integer;
        }

        public Property(string name, Lock value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            if (default(Lock).Equals(value))
                throw new ArgumentNullException(nameof(value));

            this.Name = name;
            this.directory = null;
            this.value = value;
            this.Type = PropertyType.Lock;
        }

        public Property(string name, ForthDictionaryArray value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            if (default(ForthDictionaryArray).Equals(value))
                throw new ArgumentNullException(nameof(value));

            this.Name = name;
            this.directory = null;
            this.value = value.ToString(); // Serialize the array to a string
            this.Type = PropertyType.Array;
        }

        public Property(string name, ForthListArray value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            if (default(ForthListArray).Equals(value))
                throw new ArgumentNullException(nameof(value));

            this.Name = name;
            this.directory = null;
            this.value = value.ToString(); // Serialize the array to a string
            this.Type = PropertyType.Array;
        }

        public Property(string name, Dbref value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            if (default(Dbref).Equals(value))
                throw new ArgumentNullException(nameof(value));

            this.Name = name;
            this.directory = null;
            this.value = value;
            this.Type = PropertyType.DbRef;
        }

        public Property(string name, float value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);

            this.Name = name;
            this.directory = null;
            this.value = value;
            this.Type = PropertyType.Float;
        }

        public Property(string name, PropertyDirectory value)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            ArgumentNullException.ThrowIfNull(value);

            this.Name = name;
            this.directory = value;
            this.value = null;
            this.Type = PropertyType.Directory;
        }

        public readonly string Serialize()
        {
            if (string.IsNullOrWhiteSpace(Name))
                throw new InvalidOperationException("Property name is not set");

            if (Value == null)
            {
                throw new InvalidOperationException($"Cannot serialize null value for {Name}: {Type}");
            }

            return Type switch
            {
                PropertyType.String => Serialize((string)Value),
                PropertyType.Integer => Serialize((int)Value),
                PropertyType.DbRef => Serialize((Dbref)Value, 0),
                PropertyType.Lock => Serialize((Lock)Value, 0),
                PropertyType.Array => Serialize((Array)Value, 0),
                PropertyType.Float => Serialize((float)Value),
                PropertyType.Directory => PropertyDirectory.Serialize((PropertyDirectory)Value),
                _ => throw new InvalidOperationException($"Unknown property type for {Name}: {Type}"),
            };
        }

        public static string Serialize(Property prop)
        {
            if (PropertyType.DbRef == prop.Type && prop.Value != null)
                return $"<prop><name>{prop.Name}</name>" + Serialize((Dbref)prop.Value, 0) + "</prop>";
            if (PropertyType.Lock == prop.Type && prop.Value != null)
                return $"<prop><name>{prop.Name}</name>" + Serialize((Lock)prop.Value, 0) + "</prop>";
            if (PropertyType.Array == prop.Type && prop.Value != null)
                return $"<prop><name>{prop.Name}</name>" + Serialize((Array)prop.Value, 0) + "</prop>";
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

            throw new InvalidOperationException($"Cannot handle object of type {prop.Type} (value={prop.Value ?? "NULL"})");
        }

        public static async Task<(Property, Dbref)> ScanEnvironmentForProperty(Dbref where, string propertyName, PropertyType propertyType, CancellationToken cancellationToken)
        {
            // See envprop in fuzzball property.c
            var envRef = where;
            do
            {
                if (!envRef.IsValid())
                    return (default(Property), Dbref.NOT_FOUND);
                var env = await ThingRepository.Instance.GetAsync(envRef, cancellationToken);
                if (!env.isSuccess || env.value == null)
                    return (default(Property), Dbref.NOT_FOUND);
                var property = env.value.properties.GetPropertyPathValue(propertyName);
                if (!default(Property).Equals(property) && (propertyType == PropertyType.Unknown || propertyType == property.Type))
                    return (property, envRef);

                if (envRef == Dbref.AETHER)
                    return (default(Property), Dbref.NOT_FOUND); // Once we get to AETHER, don't keep looping.

                envRef = env.value.Location;
            } while (true);
        }

        public static string Serialize(Dbref value, byte dud) => Thing.Serialize(value, 0);
        public static string Serialize(Lock value, byte dud) => Thing.Serialize(value, 0);
        public static string Serialize(Array value, byte dud) => Thing.Serialize(value, 0);
        public static string Serialize(string? value) => Thing.Serialize(value);
        public static string Serialize(float value) => Thing.Serialize(value);
        public static string Serialize(int value) => Thing.Serialize(value);

        public static bool TryInferType([NotNullWhen(true)] string? value, [NotNullWhen(true)] out Tuple<PropertyType, object>? result)
        {
            if (value == null)
            {
                result = default;
                return false;
            }

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

            result = default;
            return false;
        }

        public readonly override bool Equals(object? obj)
        {
            if (obj is not Property other) return false;
            if (Name != other.Name || Type != other.Type) return false;
            if (Name == null) return true; // Default=Default

            return Type switch
            {
                PropertyType.DbRef => Value is Dbref d && d.Equals(other.Value as Dbref?),
                PropertyType.Directory => directory?.Equals(other.directory) == true,
                PropertyType.Float => Convert.ToSingle(value) == Convert.ToSingle(other.value),
                PropertyType.Integer => Convert.ToInt32(value) == Convert.ToInt32(other.value),
                PropertyType.Lock => value is Lock l && l.Equals(other.value as Lock?),
                PropertyType.Array => value is Array a && (a.Equals(other.value as ForthListArray?) || a.Equals(other.value as ForthDictionaryArray?)),
                PropertyType.String => value is string s && string.CompareOrdinal(s, other.value as string) == 0,
                _ => false
            };
        }
    }
}