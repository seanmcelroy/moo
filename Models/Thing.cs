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

public class Thing : IStorable<Thing>
{
    public int id;
    public string name;

    public List<int> templates = new List<int>();

    public int location;
    public string externalDescription;

    public async Task<VerbResult> MoveToAsync(Container target, CancellationToken cancellationToken)
    {
        if (target.contains(id))
        {
            return new VerbResult(false, "I am already in that.");
        }

        GetResult<Container> currentLocationLookup = await ThingRepository.GetAsync<Container>(location, cancellationToken);
        VerbResult resultTakeOut = currentLocationLookup.value.remove(id);
        if (currentLocationLookup.isSuccess && currentLocationLookup.value.remove(id))
        {
            VerbResult resultPutIn = target.add(id);
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
            ((IDictionary<string, object>)expando)[prop.Key] = ((Tuple<object, string>)prop.Value).Item1;
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
            Regex r = new Regex(@"<string>(?<value>(?:.*?))<\/string>(?:.*?)");
            Match m = r.Match(serialized);

            var substring = serialized.Substring(m.Length);
            yield return Tuple.Create<object, string>(m.Groups["value"].Value, substring);
        }
        else if (serialized.StartsWith("<integer/>"))
        {
            var substring = serialized.Substring("<integer/>".Length);
            yield return Tuple.Create<object, string>((int?)null, substring);
        }
        else if (serialized.StartsWith("<integer>"))
        {
            Regex r = new Regex(@"<integer>(?<value>(?:.*?))<\/integer>(?:.*?)");
            Match m = r.Match(serialized);

            var substring = serialized.Substring(m.Length);
            yield return Tuple.Create<object, string>(int.Parse(m.Groups["value"].Value), substring);
        }
        else if (serialized.StartsWith("<array>")) {
            var array = new List<object>();
            var substring = serialized.Substring("<array>".Length);

            while (!substring.StartsWith("</array>")) {
                var valueResult = DeserializePart(substring).Single();
                substring = valueResult.Item2;

                array.Add(valueResult.Item1);
            }
            substring = substring.Substring("</array>".Length);

            yield return Tuple.Create<object, string>(array, substring);
        }
        else if (serialized.StartsWith("<dict>")) {
            var dict = new Dictionary<string, object>();
            var substring = serialized.Substring("<dict>".Length);

            while (!substring.StartsWith("</dict>")) {
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
            Regex r = new Regex(@"<key>(?<value>(?:.*?))<\/key>(?:.*?)");
            Match m = r.Match(serialized);

            var substring = serialized.Substring(m.Length);
            yield return Tuple.Create<object, string>(m.Groups["value"].Value, substring);
        }
        else if (serialized.StartsWith("<value>"))
        {
            var value = new Dictionary<string, object>();

            Regex r = new Regex(@"<value>(?<value>(?:.*?))<\/value>(?:.*?)");
            Match m = r.Match(serialized);

            var valueResult = DeserializePart(m.Groups["value"].Value).Single(); // WARNING, dictionaries in dictionaries would fail with this.

            var substring = serialized.Substring(m.Length);
            yield return Tuple.Create<object, string>(valueResult, substring);
        }
        else
        {
            throw new InvalidOperationException("Can't handle: " + serialized);
        }
    }

    public string Serialize()
    {
        return Serialize(new Dictionary<string, object> {
            { "id", id},
            { "name", name},
            { "location", location },
            { "templates", templates },
            { "externalDescription", externalDescription },
        });
    }

    protected string Serialize(Dictionary<string, object> value)
    {
        if (value == null)
            return $"<dict/>";

        var sb = new StringBuilder();
        sb.Append("<dict>");
        foreach (KeyValuePair<string, object> kvp in value)
        {
            sb.AppendFormat($"<key>{kvp.Key}</key><value>{Serialize(kvp.Value)}</value>");
        }
        sb.Append("</dict>");
        return sb.ToString();
    }

    protected string Serialize<T>(T[] array)
    {
        if (array == null)
            return $"<array/>";

        var sb = new StringBuilder();
        sb.Append("<array>");
        foreach (T element in array)
        {
            sb.Append(this.Serialize(element));
        }
        sb.Append("</array>");
        return sb.ToString();
    }

    protected string Serialize(object value)
    {
        if (value == null)
        {
            return "<null/>";
        }
        else if (typeof(string).IsAssignableFrom(value.GetType()))
            return Serialize((String)value);
        if (typeof(int?).IsAssignableFrom(value.GetType()))
            return Serialize((int?)value);
        if (typeof(bool).IsAssignableFrom(value.GetType()))
            return Serialize((bool)value);
        if (typeof(DateTime?).IsAssignableFrom(value.GetType()))
            return Serialize((DateTime?)value);
        if (typeof(IEnumerable).IsAssignableFrom(value.GetType()))
        {
            var array = ((IEnumerable)value).Cast<object>().ToArray();
            return Serialize(array);
        }

        throw new System.InvalidOperationException($"Cannot handle object of type {value.GetType().Name}");
    }

    protected string Serialize(string value)
    {
        if (value == null)
            return $"<string/>";

        return $"<string>{value}</string>";
    }

    protected string Serialize(int? value)
    {
        if (value == null)
            return $"<integer/>";

        return $"<integer>{value.Value}</integer>";
    }

    protected string Serialize(bool value)
    {
        return value ? "<true/>" : "</false>";
    }

    protected string Serialize(DateTime? value)
    {
        if (default(DateTime?) == value)
            return $"<date/>";

        return $"<date>{value.Value.ToString("o")}</date>";
    }
}