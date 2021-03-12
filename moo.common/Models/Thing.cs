using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using moo.common.Connections;
using moo.common.Scripting;
using static moo.common.Models.Property;

namespace moo.common.Models
{
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
        public string? name;

        public Dbref[] templates = Array.Empty<Dbref>();

        public Flag[] flags = Array.Empty<Flag>();

        private Dbref location;
        public Dbref owner;
        public string? externalDescription;
        public int pennies;
        public int type;
        public PropertyDirectory properties = new();
        private readonly ConcurrentDictionary<Dbref, int> contents = new();
        private readonly ConcurrentDictionary<Dbref, int> linkTargets = new();

        public ImmutableList<Dbref> Contents { get => contents.Keys.ToImmutableList(); }
        public ImmutableList<Dbref> LinkTargets { get => linkTargets.Keys.ToImmutableList(); }
        public bool Dirty { get; private set; }

        public Thing()
        {
            type = (int)Dbref.DbrefObjectType.Thing;
        }

        public virtual Dbref Owner => owner;
        public Dbref.DbrefObjectType Type => (Dbref.DbrefObjectType)type;

        public Dbref Location
        {
            get => location;
            set
            {
                location = value;
                Dirty = true;
            }
        }

        public VerbResult Add(Dbref id)
        {
            if (id == this.id)
                return new VerbResult(false, "A thing cannot go into itself");

            // TODO: Where I'm going can't be inside of me.

            if (this.contents.TryAdd(id, 0))
            {
                Dirty = true;
                return new VerbResult(true, "");
            }

            return new VerbResult(false, "Item is already in that container");
        }

        public VerbResult Add(Thing thing) => Add(thing.id);

        public bool Contains(Dbref id) => contents.ContainsKey(id);

        public bool Contains(Thing thing) => contents.ContainsKey(thing.id);

        public Dbref FirstContent()
        {
            return contents.Keys.Count == 0
                ? Dbref.NOT_FOUND
                : contents.Keys
                    .OrderBy(d => d.ToInt32())
                    .First();
        }

        public Dbref NextContent(Dbref lastContent)
        {
            if (this.contents.Keys.Count == 0)
                return Dbref.NOT_FOUND;

            return this.contents.Keys
                .OrderBy(k => k.ToInt32())
                .SkipWhile(k => k <= lastContent)
                .DefaultIfEmpty(Dbref.NOT_FOUND)
                .FirstOrDefault();
        }

        public VerbResult Remove(Dbref id)
        {
            if (id == this.id)
                return new VerbResult(false, "A thing cannot come from itself");

            if (this.contents.TryRemove(id, out _))
            {
                Dirty = true;
                return new VerbResult(true, "");
            }

            return new VerbResult(false, "Item was not in that container");
        }

        public async Task<VerbResult> MoveToAsync(Dbref targetId, CancellationToken cancellationToken)
        {
            if (targetId == id)
                return new VerbResult(false, "I am already in that.");

            var targetLookup = await ThingRepository.Instance.GetAsync<Thing>(targetId, cancellationToken);
            if (!targetLookup.isSuccess || targetLookup.value == null)
                return new VerbResult(false, $"Unable to find {targetId}");

            return await MoveToAsync(targetLookup.value, cancellationToken);
        }

        public async Task<VerbResult> MoveToAsync(PlayerConnection target, CancellationToken cancellationToken)
        {
            if (id == target.Dbref)
                return new VerbResult(false, "I am already in that.");

            var currentLocationLookup = await ThingRepository.Instance.GetAsync<Thing>(Location, cancellationToken);
            var resultTakeOut = currentLocationLookup.value.Remove(id);
            if (currentLocationLookup.isSuccess)
            {
                currentLocationLookup.value.Remove(id);
                var resultPutIn = target.GetPlayer().Add(id);
                if (resultPutIn)
                {
                    Location = target.Dbref;
                    return new VerbResult(true, "Moved");
                }
                else
                    return resultPutIn;
            }
            else
                return resultTakeOut;
        }

        public async Task<VerbResult> MoveToAsync(Thing target, CancellationToken cancellationToken)
        {
            if (target.Contains(id))
                return new VerbResult(false, "I am already in that.");

            var currentLocationLookup = await ThingRepository.Instance.GetAsync<Thing>(Location, cancellationToken);
            var resultTakeOut = currentLocationLookup.value.Remove(id);
            if (currentLocationLookup.isSuccess)
            {
                currentLocationLookup.value.Remove(id);
                var resultPutIn = target.Add(id);
                if (resultPutIn)
                {
                    Location = target.id;
                    Dirty = true;
                    return new VerbResult(true, "Moved");
                }
                else
                    return resultPutIn;
            }
            else
                return resultTakeOut;
        }

        public virtual void SetLinkTargets(IEnumerable<Dbref> targets)
        {
            linkTargets.Clear();
            foreach (var linkTarget in targets)
                while (!linkTargets.TryAdd(linkTarget, linkTarget.ToInt32())) { }
            Dirty = true;
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
                if (propValue is Dictionary<string, object> dictionary)
                {
                    ((IDictionary<string, object>)expando)[prop.Key] =
                        dictionary
                        .Select(x => new { x.Key, Value = ((Tuple<object, string>)x.Value).Item1 })
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

        public static IEnumerable<Tuple<object?, string>> DeserializePart(String serialized)
        {
            if (serialized == null)
                throw new System.ArgumentNullException(nameof(serialized));

            if (serialized.StartsWith("<null/>"))
            {
                var substring = serialized[7..]; // "<null/>".Length = 7
                yield return Tuple.Create<object?, string>(null, substring);
            }
            else if (serialized.StartsWith("<string/>"))
            {
                var substring = serialized[9..]; // "<string/>".Length = 9
                yield return Tuple.Create<object?, string>(null, substring);
            }
            else if (serialized.StartsWith("<string>"))
            {
                var r = new Regex(@"<string>(?<value>(?:.*?))<\/string>(?:.*?)", RegexOptions.Compiled);
                var m = r.Match(serialized);

                var substring = serialized[m.Length..];
                yield return Tuple.Create<object?, string>(System.Web.HttpUtility.HtmlDecode(m.Groups["value"].Value), substring);
            }
            else if (serialized.StartsWith("<lock/>"))
            {
                var substring = serialized[7..]; // "<lock/>".Length = 7
                yield return Tuple.Create<object?, string>(Dbref.NOT_FOUND, substring);
            }
            else if (serialized.StartsWith("<lock>"))
            {
                var r = new Regex(@"<lock>(?<value>(?:.*?))<\/lock>(?:.*?)", RegexOptions.Compiled);
                var m = r.Match(serialized);

                var substring = serialized[m.Length..];
                yield return Tuple.Create<object?, string>(new Lock(m.Groups["value"].Value), substring);
            }
            else if (serialized.StartsWith("<dbref/>"))
            {
                var substring = serialized[8..]; // "<dbref/>".Length = 8
                yield return Tuple.Create<object?, string>(Dbref.NOT_FOUND, substring);
            }
            else if (serialized.StartsWith("<dbref>"))
            {
                var r = new Regex(@"<dbref>(?<value>(?:.*?))<\/dbref>(?:.*?)", RegexOptions.Compiled);
                var m = r.Match(serialized);

                var substring = serialized[m.Length..];
                yield return Tuple.Create<object?, string>(new Dbref(m.Groups["value"].Value), substring);
            }
            else if (serialized.StartsWith("<float/>"))
            {
                var substring = serialized[8..]; // "<float/>".Length = 8
                yield return Tuple.Create<object?, string>((float?)null, substring);
            }
            else if (serialized.StartsWith("<float>"))
            {
                var r = new Regex(@"<float>(?<value>(?:.*?))<\/float>(?:.*?)", RegexOptions.Compiled);
                var m = r.Match(serialized);

                var substring = serialized[m.Length..];
                yield return Tuple.Create<object?, string>(float.Parse(m.Groups["value"].Value), substring);
            }
            else if (serialized.StartsWith("<integer/>"))
            {
                var substring = serialized[10..]; // "<integer/>".Length = 10
                yield return Tuple.Create<object?, string>((int?)null, substring);
            }
            else if (serialized.StartsWith("<integer>"))
            {
                var r = new Regex(@"<integer>(?<value>(?:.*?))<\/integer>(?:.*?)", RegexOptions.Compiled);
                var m = r.Match(serialized);

                var substring = serialized[m.Length..];
                yield return Tuple.Create<object?, string>(int.Parse(m.Groups["value"].Value), substring);
            }
            else if (serialized.StartsWith("<uint16/>"))
            {
                var substring = serialized[9..]; // "<uint16/>".Length = 9
                yield return Tuple.Create<object?, string>(null, substring);
            }
            else if (serialized.StartsWith("<uint16>"))
            {
                var r = new Regex(@"<uint16>(?<value>(?:.*?))<\/uint16>(?:.*?)", RegexOptions.Compiled);
                var m = r.Match(serialized);

                var substring = serialized[m.Length..];
                yield return Tuple.Create<object?, string>(UInt16.Parse(m.Groups["value"].Value), substring);
            }
            else if (serialized.StartsWith("<date/>"))
            {
                var substring = serialized[7..]; // "<date/>".Length = 7
                yield return Tuple.Create<object?, string>(null, substring);
            }
            else if (serialized.StartsWith("<date>"))
            {
                var r = new Regex(@"<date>(?<value>(?:.*?))<\/date>(?:.*?)", RegexOptions.Compiled);
                var m = r.Match(serialized);

                var substring = serialized[m.Length..];
                var dto = DateTimeOffset.Parse(m.Groups["value"].Value, null, DateTimeStyles.RoundtripKind);
                yield return Tuple.Create<object?, string>(dto.UtcDateTime, substring);
            }
            else if (serialized.StartsWith("<array>"))
            {
                var array = new List<object>();
                var substring = serialized[7..]; // "<array>".Length = 7

                while (!substring.StartsWith("</array>"))
                {
                    var valueResult = DeserializePart(substring).Single();
                    substring = valueResult.Item2;
                    if (valueResult.Item1 != null)
                        array.Add(valueResult.Item1);
                }
                substring = substring[8..]; // "</array>".Length = 8

                yield return Tuple.Create<object?, string>(array, substring);
            }
            else if (serialized.StartsWith("<propdir>"))
            {
                var propdir = new PropertyDirectory();
                var substring = serialized[9..]; // "<propdir>".Length = 9

                while (!substring.StartsWith("</propdir>"))
                {
                    var keyResult = DeserializePart(substring).Single();
                    substring = keyResult.Item2;
                    var valueResult = DeserializePart(substring).Single();
                    substring = valueResult.Item2;
                    propdir.Add((string)keyResult.Item1, (Property)((Tuple<object, string>)valueResult.Item1).Item1);
                }
                substring = substring[10..]; // "</propdir>".Length = 10

                yield return Tuple.Create<object?, string>(propdir, substring);
            }
            else if (serialized.StartsWith("<prop>"))
            {
                var r = new Regex(@"<prop><name>(?<name>[^<]*)<\/name>(?<value>(?:<(?<open>\w+)>.*?)<\/\k<open>>)<\/prop>(?:.*?)", RegexOptions.Compiled);
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

                var substring = serialized[m.Length..];
                yield return Tuple.Create<object?, string>(property, substring);
            }
            else if (serialized.StartsWith("<dict>"))
            {
                var dict = new Dictionary<string, object>();
                var substring = serialized[6..]; // "<dict>".Length = 6

                while (!substring.StartsWith("</dict>"))
                {
                    var keyResult = DeserializePart(substring).Single();
                    substring = keyResult.Item2;
                    var valueResult = DeserializePart(substring).Single();
                    substring = valueResult.Item2;

                    dict.Add((string)keyResult.Item1, valueResult.Item1);
                }
                substring = substring[7..]; // "</dict>".Length = 7

                yield return Tuple.Create<object?, string>(dict, substring);
            }
            else if (serialized.StartsWith("<key>"))
            {
                var r = new Regex(@"<key>(?<value>(?:.*?))<\/key>(?:.*?)", RegexOptions.Compiled);
                var m = r.Match(serialized);

                var substring = serialized[m.Length..];
                yield return Tuple.Create<object?, string>(m.Groups["value"].Value, substring);
            }
            else if (serialized.StartsWith("<value><dict>"))
            {
                var r = new Regex(@"<value>(?=<dict>)(?<value>(?:.*?))(?<=<\/dict>)<\/value>(?:.*?)", RegexOptions.Compiled);
                var m = r.Match(serialized);

                // Handle dictionaries of dictionaries
                var substring = serialized[7..]; // "<value>".Length = 7
                var innerDictionary = DeserializePart(substring).Single();
                yield return Tuple.Create<object?, string>(innerDictionary, substring[(substring.LastIndexOf("</dict></value>") + "</dict></value>".Length)..]);
            }
            else if (serialized.StartsWith("<value><propdir>"))
            {
                var r = new Regex(@"<value>(?=<propdir>)(?<value>(?:.*?))(?<=<\/propdir>)<\/value>(?:.*?)", RegexOptions.Compiled);
                var m = r.Match(serialized);

                // Handle property directories
                var substring = serialized[7..]; // "<value>".Length = 7
                var propertyDirectory = DeserializePart(substring).Single();
                yield return Tuple.Create<object?, string>(propertyDirectory, substring[(substring.IndexOf("</propdir></value>") + "</propdir></value>".Length)..]);
            }
            else if (serialized.StartsWith("<value><prop>"))
            {
                var r = new Regex(@"<value>(?=<prop>)(?<value>(?:.*?))(?<=<\/prop>)<\/value>(?:.*?)", RegexOptions.Compiled);
                var m = r.Match(serialized);

                // Handle property directories
                var substring = serialized[7..]; // "<value>".Length = 7
                var property = DeserializePart(substring).Single();
                yield return Tuple.Create<object?, string>(property, substring.Substring(substring.LastIndexOf("</prop></value>") + "</prop></value>".Length));
            }
            else if (serialized.StartsWith("<value>"))
            {
                var r = new Regex(@"<value>(?<value>(?:.*?))<\/value>(?:.*?)", RegexOptions.Compiled);
                var m = r.Match(serialized);

                var valueResult = DeserializePart(m.Groups["value"].Value).Single();
                var substring = serialized[m.Length..];
                yield return Tuple.Create<object?, string>(valueResult, substring);
            }
            else
            {
                throw new InvalidOperationException($"Can't handle: {serialized}");
            }
        }

        public async Task<Property> GetPropertyPathValueAsync(string path, CancellationToken cancellationToken)
        {
            if (this.properties == null)
                return default;

            var result = this.properties.GetPropertyPathValue(path);

            // Rooms inherit the properties of their parents
            if (result.Equals(default(Property)) && this.id != Dbref.AETHER)
            {
                var parentLookup = await ThingRepository.Instance.GetAsync<Thing>(this.Location, cancellationToken);
                if (!parentLookup.isSuccess || parentLookup.value == null)
                    return result;

                return await parentLookup.value.GetPropertyPathValueAsync(path, cancellationToken);
            }

            return result;
        }

        public bool HasFlag(Flag flag) => flags != null && flags.Any(f => f == flag);

        public void ClearFlag(Flag flag)
        {
            if (!HasFlag(flag))
                return;

            flags = flags.Except(new[] { flag }).ToArray();
            Dirty = true;
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
                newArray[^1] = flag;
                this.flags = newArray;
            }
            Dirty = true;
        }

        public void ClearProperties()
        {
            properties = new PropertyDirectory();
            Dirty = true;
        }

        public void ClearPropertyPath(string path)
        {
            if (properties != null)
                properties.ClearPropertyPathValue(path);
            Dirty = true;
        }

        public void SetPropertyPathValue(string path, Dbref value)
        {
            if (properties == null)
                properties = new PropertyDirectory();

            properties.SetPropertyPathValue(path, new ForthVariable(value, 0));
            Dirty = true;
        }

        public void SetPropertyPathValue(string path, float value)
        {
            if (properties == null)
                properties = new PropertyDirectory();

            properties.SetPropertyPathValue(path, PropertyType.Float, value);
            Dirty = true;
        }

        public void SetPropertyPathValue(string path, PropertyType type, string value)
        {
            if (properties == null)
                properties = new PropertyDirectory();

            properties.SetPropertyPathValue(path, type, value);
            Dirty = true;
        }

        public void SetPropertyPathValue(string path, PropertyType type, object value)
        {
            if (properties == null)
                properties = new PropertyDirectory();

            properties.SetPropertyPathValue(path, type, value);
            Dirty = true;
        }

        public void SetPropertyPathValue(string path, ForthVariable value)
        {
            if (properties == null)
                properties = new PropertyDirectory();

            properties.SetPropertyPathValue(path, value);
            Dirty = true;
        }

        public string Serialize() => Serialize(GetSerializedElements());

        protected virtual Dictionary<string, object?> GetSerializedElements() => new()
        {
            { "id", id },
            { "name", name },
            { "location", Location },
            { "contents", contents },
            { "link", linkTargets.Keys.ToArray() },
            { "templates", templates },
            { "flags", flags },
            { "externalDescription", externalDescription },
            { "owner", Owner },
            { "pennies", pennies },
            { "properties", properties }
        };

        public static string Serialize(PropertyDirectory value) => PropertyDirectory.Serialize(value);

        public static string Serialize(Dictionary<string, object?> value)
        {
            if (value == null)
                return $"<dict/>";

            var sb = new StringBuilder();
            sb.Append("<dict>");
            foreach (var kvp in value.OrderBy(v => string.Compare(v.Key, "properties") == 0 ? 1 : 2).ThenBy(v => v.Key))
            {
                try
                {
                    sb.Append($"<key>{kvp.Key}</key><value>{Serialize(kvp.Value)}</value>");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error on serialization of {kvp.Key}={kvp.Value}\r\n{ex}");
                }
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
            foreach (T? element in array)
            {
                sb.Append(Serialize(element));
            }
            sb.Append("</array>");
            return sb.ToString();
        }

        public static string Serialize(object? value)
        {
            if (value == null)
                return "<null/>";
            var valueType = value.GetType();

            if (typeof(Dbref) == valueType)
                return Serialize((Dbref)value, 0);
            if (typeof(string).IsAssignableFrom(valueType))
                return Serialize((String)value);
            if (typeof(int?).IsAssignableFrom(valueType))
                return Serialize((int?)value);
            if (typeof(float?).IsAssignableFrom(valueType))
                return Serialize((float?)value);
            if (typeof(Flag).IsAssignableFrom(valueType))
                return Serialize((UInt16)value);
            if (typeof(DateTime?).IsAssignableFrom(valueType))
                return Serialize((DateTime?)value);
            if (typeof(PropertyDirectory).IsAssignableFrom(valueType))
                return Serialize((PropertyDirectory)value);
            if (typeof(Dictionary<string, object?>).IsAssignableFrom(valueType))
                return Serialize((Dictionary<string, object?>)value);
            if (typeof(IEnumerable).IsAssignableFrom(valueType))
            {
                var array = ((IEnumerable)value).Cast<object>().ToArray();
                return Serialize(array);
            }

            throw new System.InvalidOperationException($"Cannot handle object of type {value.GetType().Name}");
        }

        public static string Serialize(Dbref value, byte dud) => $"<dbref>{value}</dbref>";

        public static string Serialize(Lock value, byte dud) => $"<lock>{value}</lock>";

        public static string Serialize(string? value) => value == null ? $"<string/>" : $"<string>{System.Web.HttpUtility.HtmlEncode(value)}</string>";

        public static string Serialize(float? value) => value == null ? $"<float/>" : $"<float>{value.Value}</float>";

        public static string Serialize(int? value) => value == null ? $"<integer/>" : $"<integer>{value.Value}</integer>";

        public static string Serialize(ushort? value) => value == null ? $"<uint16/>" : $"<uint16>{value.Value}</uint16>";

        public static string Serialize(DateTime? value) => default == value || value == null ? $"<date/>" : $"<date>{value.Value.ToUniversalTime():o}</date>";

        public string UnparseObject()
        {
            var flagString = this.flags == null || this.flags.Length == 0 ? string.Empty : this.flags.Select(f => ((char)f).ToString()).Aggregate((c, n) => $"{c}{n}");
            return $"{this.name}(#{this.id.ToInt32()}{(char)this.id.Type}{flagString})";
        }

        public bool IsGod() => this.id == Dbref.GOD;
    }
}