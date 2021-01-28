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
    public enum Flag : UInt16
    {
        //ABCDHJKLMQSW
        LEVEL_0 = '0',
        LEVEL_1 = '1',
        LEVEL_2 = '2',
        LEVEL_3 = '3',
        ABODE = 'A',
        BUILDER = 'B',
        CHOWN_OK = 'C',
        DARK = 'D',
        HAVEN = 'H',
        JUMP_OK = 'J',
        KILL_OK = 'K',
        LINK_OK = 'L',
        MUCKER = 'M',
        QUELL = 'Q',
        STICKY = 'S',
        VEHICLE = 'V',
        WIZARD = 'W',
        ZOMBIE = 'Z'
    }

    public Dbref id;
    public string name;

    public Dbref[] templates = Dbref.EMPTY_SET;

    public Flag[] flags;

    public Dbref location;
    public Dbref owner;
    public string externalDescription;
    public int pennies;
    public int type;
    public PropertyDirectory properties = new PropertyDirectory();

    public Thing()
    {
        this.type = (int)Dbref.DbrefObjectType.Thing;
    }

    public Dbref[] links = Dbref.EMPTY_SET;
    public virtual Dbref[] Links => links;
    public virtual Dbref Owner => owner;
    public Dbref.DbrefObjectType Type => (Dbref.DbrefObjectType)type;

    public async Task<VerbResult> MoveToAsync(Dbref targetId, CancellationToken cancellationToken)
    {
        if (targetId == id)
            return new VerbResult(false, "I am already in that.");

        var targetLookup = await ThingRepository.GetAsync<Container>(targetId, cancellationToken);
        if (!targetLookup.isSuccess)
            return new VerbResult(false, $"Unable to find {targetId}");

        return await MoveToAsync(targetLookup.value, cancellationToken);
    }

    public async Task<VerbResult> MoveToAsync(PlayerConnection target, CancellationToken cancellationToken)
    {
        if (id == target.Dbref)
            return new VerbResult(false, "I am already in that.");

        var currentLocationLookup = await ThingRepository.GetAsync<Container>(location, cancellationToken);
        var resultTakeOut = currentLocationLookup.value.Remove(id);
        if (currentLocationLookup.isSuccess)
        {
            currentLocationLookup.value.Remove(id);
            var resultPutIn = target.GetPlayer().Add(id);
            if (resultPutIn)
            {
                location = target.Dbref;
                return new VerbResult(true, "Moved");
            }
            else
                return resultPutIn;
        }
        else
            return resultTakeOut;
    }

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

    public virtual void SetLinkTargets(params Dbref[] targets)
    {
        this.links = targets;
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
        else if (serialized.StartsWith("<lock/>"))
        {
            var substring = serialized.Substring("<lock/>".Length);
            yield return Tuple.Create<object, string>(Dbref.NOT_FOUND, substring);
        }
        else if (serialized.StartsWith("<lock>"))
        {
            var r = new Regex(@"<lock>(?<value>(?:.*?))<\/lock>(?:.*?)");
            var m = r.Match(serialized);

            var substring = serialized.Substring(m.Length);
            yield return Tuple.Create<object, string>(new Lock(m.Groups["value"].Value), substring);
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
                throw new InvalidOperationException($"Unknown property type: {propertyValue.GetType()}");

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
            throw new InvalidOperationException($"Can't handle: {serialized}");
        }
    }

    public async Task<Property> GetPropertyPathValueAsync(string path, CancellationToken cancellationToken)
    {
        if (this.properties == null)
            return default(Property);

        var result = this.properties.GetPropertyPathValue(path);

        // Rooms inherit the properties of their parents
        if (result.Equals(default(Property)) && this.id != Dbref.AETHER)
        {
            var parentLookup = await ThingRepository.GetAsync<Container>(this.location, cancellationToken);
            if (!parentLookup.isSuccess)
                return result;

            return await parentLookup.value.GetPropertyPathValueAsync(path, cancellationToken);
        }

        return result;
    }

    public bool HasFlag(Flag flag)
    {
        return this.flags != null && this.flags.Any(f => f == flag);
    }

    public async Task<bool> IsControlledByAsync(Dbref playerId, CancellationToken cancellationToken)
    {
        /*
        You control anything you own.
        A wizard or God controls everything.
        Anybody controls an unlinked exit, even if it is locked. Builders should beware of 3, lest their exits be linked or stolen.
        Players control all exits which are linked to their areas, to better facilitate border control.
        If an object is set CHOWN_OK, anyone may @chown object=me and gain control of the object.
        */
        if (this.owner.Equals(playerId))
            return true;

        if (playerId.Equals(Dbref.GOD))
            return true;

        var playerLookup = await ThingRepository.GetAsync<Thing>(playerId, cancellationToken);
        if (!playerLookup.isSuccess)
            return false;

        var player = playerLookup.value;
        if (player.HasFlag(Flag.WIZARD))
            return true;

        if (this.Type == Dbref.DbrefObjectType.Exit)
        {
            if (this.links.Length == 0)
                return true;

            var linkTargetTasks = this.links.Select(l => ThingRepository.GetAsync<Thing>(l, cancellationToken)).ToArray();
            await Task.WhenAll(linkTargetTasks);
            var linkTargets = linkTargetTasks.Where(l => l.IsCompleted).Select(l => l.Result);
            if (linkTargets.Any(l => l.isSuccess && l.value.owner == playerId))
                return true;
        }

        return false;
    }

    public async Task<bool> IsControlledByAsync(PlayerConnection connection, CancellationToken cancellationToken)
    {
        /*
        You control anything you own.
        A wizard or God controls everything.
        Anybody controls an unlinked exit, even if it is locked. Builders should beware of 3, lest their exits be linked or stolen.
        Players control all exits which are linked to their areas, to better facilitate border control.
        If an object is set CHOWN_OK, anyone may @chown object=me and gain control of the object.
        */
        if (this.owner.Equals(connection.Dbref))
            return true;

        if (connection.Dbref.Equals(Dbref.GOD))
            return true;

        if (connection.HasFlag(Flag.WIZARD))
            return true;

        if (this.Type == Dbref.DbrefObjectType.Exit)
        {
            if (this.links.Length == 0)
                return true;

            var linkTargetTasks = this.links.Select(l => ThingRepository.GetAsync<Thing>(l, cancellationToken)).ToArray();
            await Task.WhenAll(linkTargetTasks);
            var linkTargets = linkTargetTasks.Where(l => l.IsCompleted).Select(l => l.Result);
            if (linkTargets.Any(l => l.isSuccess && l.value.owner == connection.Dbref))
                return true;
        }

        return false;
    }

    public void ClearFlag(Flag flag)
    {
        if (!HasFlag(flag))
            return;

        this.flags = this.flags.Except(new[] { flag }).ToArray();
    }

    public void SetFlag(Flag flag)
    {
        if (HasFlag(flag))
            return;
        if (this.flags == null)
            this.flags = new Flag[] { flag };
        else
        {
            var newArray = new Flag[this.flags.Length + 1];
            Array.Copy(this.flags, newArray, this.flags.Length);
            newArray[newArray.Length - 1] = flag;
            this.flags = newArray;
        }
    }

    public void ClearProperties()
    {
        this.properties = new PropertyDirectory();
    }

    public void ClearPropertyPath(string path)
    {
        if (this.properties != null)
            this.properties.ClearPropertyPathValue(path);
    }

    public void SetPropertyPathValue(string path, Dbref value)
    {
        if (this.properties == null)
            this.properties = new PropertyDirectory();

        this.properties.SetPropertyPathValue(path, new ForthVariable(value, 0));
    }

    public void SetPropertyPathValue(string path, float value)
    {
        if (this.properties == null)
            this.properties = new PropertyDirectory();

        this.properties.SetPropertyPathValue(path, PropertyType.Float, value);
    }

    public void SetPropertyPathValue(string path, PropertyType type, string value)
    {
        if (this.properties == null)
            this.properties = new PropertyDirectory();

        this.properties.SetPropertyPathValue(path, type, value);
    }

    public void SetPropertyPathValue(string path, PropertyType type, object value)
    {
        if (this.properties == null)
            this.properties = new PropertyDirectory();

        this.properties.SetPropertyPathValue(path, type, value);
    }

    public void SetPropertyPathValue(string path, ForthVariable value)
    {
        if (this.properties == null)
            this.properties = new PropertyDirectory();

        this.properties.SetPropertyPathValue(path, value);
    }

    public string Serialize() => Serialize(GetSerializedElements());

    protected virtual Dictionary<string, object> GetSerializedElements()
    {
        return new Dictionary<string, object> {
            { "id", id},
            { "name", name},
            { "location", location },
            { "links", links },
            { "templates", templates },
            { "flags", flags },
            { "externalDescription", externalDescription },
            { "pennies", pennies },
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

    public static string Serialize(Lock value, byte dud)
    {
        return $"<lock>{value}</lock>";
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

    public string UnparseObject()
    {
        var flagString = this.flags == null || this.flags.Length == 0 ? string.Empty : this.flags.Select(f => ((char)f).ToString()).Aggregate((c, n) => $"{c}{n}");
        return $"{this.name}(#{this.id.ToInt32()}{(char)this.id.Type}{flagString})";
    }
}