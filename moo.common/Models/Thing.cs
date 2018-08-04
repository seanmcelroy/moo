using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using static ThingRepository;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using static Property;

public class Thing : IStorable<Thing>
{
    public Dbref id;
    public string name;

    public List<Dbref> templates = new List<Dbref>();

    public string[] flags;

    public Dbref location;
    public Dbref owner;
    public string externalDescription;
    public PropertyDirectory properties = new PropertyDirectory();

    public Thing()
    {
    }

    public virtual Dbref Owner => owner;

    public async Task<VerbResult> MoveToAsync(Container target, CancellationToken cancellationToken)
    {
        if (target.Contains(id))
        {
            return new VerbResult(false, "I am already in that.");
        }

        var currentLocationLookup = await ThingRepository.GetAsync<Container>(location, cancellationToken);
        var resultTakeOut = currentLocationLookup.value.Remove(id);
        if (currentLocationLookup.isSuccess)
        {
            currentLocationLookup.value.Remove(id);
            var resultPutIn = target.Add(id);
            if (resultPutIn)
            {
                location = target.id;
                return new VerbResult(true, "Moved");
            }
            else
                return resultPutIn;
        }
        else
            return resultTakeOut;
    }

    public static T Deserialize<T>(string serialized) where T : Thing, new()
    {
        if (serialized == null)
            throw new System.ArgumentNullException(nameof(serialized));

        var root = DeserializePart(serialized).Select(k => k.Item1).Cast<Dictionary<string, object>>().Single();

        dynamic expando = new ExpandoObject();
        foreach (var prop in root)
        {
            var propValue = ((Tuple<object, string>)prop.Value).Item1;
            if (propValue is Dictionary<string, object>)
            {
                ((IDictionary<string, object>)expando)[prop.Key] =
                    ((Dictionary<string, object>)propValue)
                    .Select(x => new { Key = x.Key, Value = ((Tuple<object, string>)x.Value).Item1 })
                    .ToDictionary(k => k.Key, v => v.Value);
            }
            else
            {
                ((IDictionary<string, object>)expando)[prop.Key] = propValue;
            }
        }

        string json = Newtonsoft.Json.JsonConvert.SerializeObject(expando);
        var result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json);

        return result;
    }

    protected static IEnumerable<Tuple<object, string>> DeserializePart(String serialized)
    {
        if (serialized == null)
            throw new System.ArgumentNullException(nameof(serialized));

        if (serialized.StartsWith("<null/>"))
        {
            var substring = serialized.Substring("<null/>".Length);
            yield return Tuple.Create<object, string>(null, substring);
        }
        else if (serialized.StartsWith("<string/>"))
        {
            var substring = serialized.Substring("<string/>".Length);
            yield return Tuple.Create<object, string>((string)null, substring);
        }
        else if (serialized.StartsWith("<string>"))
        {
            var r = new Regex(@"<string>(?<value>(?:.*?))<\/string>(?:.*?)");
            var m = r.Match(serialized);

            var substring = serialized.Substring(m.Length);
            yield return Tuple.Create<object, string>(m.Groups["value"].Value, substring);
        }
        else if (serialized.StartsWith("<dbref/>"))
        {
            var substring = serialized.Substring("<dbref/>".Length);
            yield return Tuple.Create<object, string>(Dbref.NOT_FOUND, substring);
        }
        else if (serialized.StartsWith("<dbref>"))
        {
            var r = new Regex(@"<dbref>(?<value>(?:.*?))<\/dbref>(?:.*?)");
            var m = r.Match(serialized);

            var substring = serialized.Substring(m.Length);
            yield return Tuple.Create<object, string>(new Dbref(m.Groups["value"].Value), substring);
        }
        else if (serialized.StartsWith("<float/>"))
        {
            var substring = serialized.Substring("<float/>".Length);
            yield return Tuple.Create<object, string>((float?)null, substring);
        }
        else if (serialized.StartsWith("<float>"))
        {
            var r = new Regex(@"<float>(?<value>(?:.*?))<\/float>(?:.*?)");
            var m = r.Match(serialized);

            var substring = serialized.Substring(m.Length);
            yield return Tuple.Create<object, string>(float.Parse(m.Groups["value"].Value), substring);
        }
        else if (serialized.StartsWith("<integer/>"))
        {
            var substring = serialized.Substring("<integer/>".Length);
            yield return Tuple.Create<object, string>((int?)null, substring);
        }
        else if (serialized.StartsWith("<integer>"))
        {
            var r = new Regex(@"<integer>(?<value>(?:.*?))<\/integer>(?:.*?)");
            var m = r.Match(serialized);

            var substring = serialized.Substring(m.Length);
            yield return Tuple.Create<object, string>(int.Parse(m.Groups["value"].Value), substring);
        }
        else if (serialized.StartsWith("<array>"))
        {
            var array = new List<object>();
            var substring = serialized.Substring("<array>".Length);

            while (!substring.StartsWith("</array>"))
            {
                var valueResult = DeserializePart(substring).Single();
                substring = valueResult.Item2;

                array.Add(valueResult.Item1);
            }
            substring = substring.Substring("</array>".Length);

            yield return Tuple.Create<object, string>(array, substring);
        }
        else if (serialized.StartsWith("<propdir>"))
        {
            var propdir = new PropertyDirectory();
            var substring = serialized.Substring("<propdir>".Length);

            while (!substring.StartsWith("</propdir>"))
            {
                var keyResult = DeserializePart(substring).Single();
                substring = keyResult.Item2;
                var valueResult = DeserializePart(substring).Single();
                substring = valueResult.Item2;

                propdir.Add((string)keyResult.Item1, (Property)((Tuple<object, string>)valueResult.Item1).Item1);
            }
            substring = substring.Substring("</propdir>".Length);

            yield return Tuple.Create<object, string>(propdir, substring);
        }
        else if (serialized.StartsWith("<prop>"))
        {
            //var r = new Regex(@"<prop><name>(?<name>[^<]*)<\/name>(?<value>(?:.*?))<\/prop>(?:.*?)");
            var r = new Regex(@"<prop><name>(?<name>[^<]*)<\/name>(?<value>(?:<(?<open>\w+)>.*?)<\/\k<open>>)<\/prop>(?:.*?)");
            var m = r.Match(serialized);

            var propertyName = m.Groups["name"].Value;
            var propertyValue = DeserializePart(m.Groups["value"].Value).Single().Item1;
            Property property;
            if (typeof(string) == propertyValue.GetType())
                property = new Property(propertyName, (string)propertyValue);
            else if (typeof(float) == propertyValue.GetType())
                property = new Property(propertyName, (float)propertyValue);
            else if (typeof(int) == propertyValue.GetType())
                property = new Property(propertyName, (int)propertyValue);
            else if (typeof(Dbref) == propertyValue.GetType())
                property = new Property(propertyName, (Dbref)propertyValue);
            else if (typeof(PropertyDirectory) == propertyValue.GetType())
                property = new Property(propertyName, (PropertyDirectory)propertyValue);
            else
                throw new InvalidOperationException("Unknown property type: " + propertyValue.GetType());

            var substring = serialized.Substring(m.Length);
            yield return Tuple.Create<object, string>(property, substring);
        }
        else if (serialized.StartsWith("<dict>"))
        {
            var dict = new Dictionary<string, object>();
            var substring = serialized.Substring("<dict>".Length);

            while (!substring.StartsWith("</dict>"))
            {
                var keyResult = DeserializePart(substring).Single();
                substring = keyResult.Item2;
                var valueResult = DeserializePart(substring).Single();
                substring = valueResult.Item2;

                dict.Add((string)keyResult.Item1, valueResult.Item1);
            }
            substring = substring.Substring("</dict>".Length);

            yield return Tuple.Create<object, string>(dict, substring);
        }
        else if (serialized.StartsWith("<key>"))
        {
            var r = new Regex(@"<key>(?<value>(?:.*?))<\/key>(?:.*?)");
            var m = r.Match(serialized);

            var substring = serialized.Substring(m.Length);
            yield return Tuple.Create<object, string>(m.Groups["value"].Value, substring);
        }
        else if (serialized.StartsWith("<value>"))
        {
            var value = new Dictionary<string, object>();

            var r = new Regex(@"<value>(?<value>(?:.*?))<\/value>(?:.*?)");
            var m = r.Match(serialized);

            if (m.Groups["value"].Value.StartsWith("<dict>"))
            {
                // Handle dictionaries of dictionaries
                var substring = serialized.Substring("<value>".Length);
                var innerDictionary = DeserializePart(substring).Single();
                yield return Tuple.Create<object, string>(innerDictionary, substring.Substring(substring.IndexOf("</dict></value>") + "</dict></value>".Length));
            }
            else if (m.Groups["value"].Value.StartsWith("<propdir>"))
            {
                // Handle property directories
                var substring = serialized.Substring("<value>".Length);
                var propertyDirectory = DeserializePart(substring).Single();
                yield return Tuple.Create<object, string>(propertyDirectory, substring.Substring(substring.IndexOf("</propdir></value>") + "</propdir></value>".Length));
            }
            else if (m.Groups["value"].Value.StartsWith("<prop>"))
            {
                // Handle property directories
                var substring = serialized.Substring("<value>".Length);
                var property = DeserializePart(substring).Single();
                yield return Tuple.Create<object, string>(property, substring.Substring(substring.IndexOf("</prop></value>") + "</prop></value>".Length));
            }
            else
            {
                var valueResult = DeserializePart(m.Groups["value"].Value).Single();
                var substring = serialized.Substring(m.Length);
                yield return Tuple.Create<object, string>(valueResult, substring);
            }
        }
        else
        {
            throw new InvalidOperationException("Can't handle: " + serialized);
        }
    }

    public Property? GetPropertyPathValue(string path)
    {
        return this.properties?.GetPropertyPathValue(path);
    }

    public bool HasFlag(string flag)
    {
        return this.flags != null && this.flags.Any(f => string.Compare(f, flag, true) == 0);
    }

    public void SetFlag(string flag)
    {
        if (HasFlag(flag))
            return;
        if (this.flags == null)
            this.flags = new string[] { flag };
        else
        {
            var newArray = new string[this.flags.Length + 1];
            Array.Copy(this.flags, newArray, this.flags.Length);
            newArray[newArray.Length - 1] = flag;
            this.flags = newArray;
        }
    }

    public void SetPropertyPathValue(string path, ForthVariable value)
    {
        if (this.properties == null)
            this.properties = new PropertyDirectory();

        this.properties.SetPropertyPathValue(path, value);
    }

    public string Serialize()
    {
        return Serialize(GetSerializedElements());
    }

    protected virtual Dictionary<string, object> GetSerializedElements()
    {
        return new Dictionary<string, object> {
            { "id", id},
            { "name", name},
            { "location", location },
            { "templates", templates },
            { "flags", flags },
            { "externalDescription", externalDescription },
            { "properties", properties }
        };
    }

    public static string Serialize(PropertyDirectory value) => PropertyDirectory.Serialize(value);

    public static string Serialize(Dictionary<string, object> value)
    {
        if (value == null)
            return $"<dict/>";

        var sb = new StringBuilder();
        sb.Append("<dict>");
        foreach (var kvp in value)
        {
            sb.AppendFormat($"<key>{kvp.Key}</key><value>{Serialize(kvp.Value)}</value>");
        }
        sb.Append("</dict>");
        return sb.ToString();
    }

    public static string Serialize<T>(T[] array)
    {
        if (array == null)
            return $"<array/>";

        var sb = new StringBuilder();
        sb.Append("<array>");
        foreach (T element in array)
        {
            sb.Append(Serialize(element));
        }
        sb.Append("</array>");
        return sb.ToString();
    }

    public static string Serialize(object value)
    {
        if (value == null)
            return "<null/>";
        if (typeof(Dbref) == value.GetType())
            return Serialize((Dbref)value, 0);
        if (typeof(string).IsAssignableFrom(value.GetType()))
            return Serialize((String)value);
        if (typeof(int?).IsAssignableFrom(value.GetType()))
            return Serialize((int?)value);
        if (typeof(float?).IsAssignableFrom(value.GetType()))
            return Serialize((float?)value);
        if (typeof(bool).IsAssignableFrom(value.GetType()))
            return Serialize((bool)value);
        if (typeof(DateTime?).IsAssignableFrom(value.GetType()))
            return Serialize((DateTime?)value);
        if (typeof(PropertyDirectory).IsAssignableFrom(value.GetType()))
            return Serialize((PropertyDirectory)value);
        if (typeof(Dictionary<string, object>).IsAssignableFrom(value.GetType()))
            return Serialize((Dictionary<string, object>)value);
        if (typeof(IEnumerable).IsAssignableFrom(value.GetType()))
        {
            var array = ((IEnumerable)value).Cast<object>().ToArray();
            return Serialize(array);
        }

        throw new System.InvalidOperationException($"Cannot handle object of type {value.GetType().Name}");
    }

    public static string Serialize(Dbref value, byte dud)
    {
        return $"<dbref>{value}</dbref>";
    }


    public static string Serialize(string value)
    {
        if (value == null)
            return $"<string/>";

        return $"<string>{value}</string>";
    }

    public static string Serialize(float? value)
    {
        if (value == null)
            return $"<float/>";

        return $"<float>{value.Value}</float>";
    }

    public static string Serialize(int? value)
    {
        if (value == null)
            return $"<integer/>";

        return $"<integer>{value.Value}</integer>";
    }

    public static string Serialize(bool value)
    {
        return value ? "<true/>" : "</false>";
    }

    public static string Serialize(DateTime? value)
    {
        if (default(DateTime?) == value)
            return $"<date/>";

        return $"<date>{value.Value.ToString("o")}</date>";
    }
}