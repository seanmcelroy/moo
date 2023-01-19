using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using moo.common.Connections;
using moo.common.Database;
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
        public PropertyDirectory properties = new();// public for serialization
        public ConcurrentDbrefSet contents = new();// public for serialization
        public ConcurrentDbrefSet linkTargets = new(); // public for serialization

        public ImmutableList<Dbref> Contents { get => contents.ToImmutableList(); }
        public ImmutableList<Dbref> LinkTargets { get => linkTargets.ToImmutableList(); }
        public bool Dirty { get; protected set; }

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

            if (this.contents.TryAdd(id))
            {
                Dirty = true;
                return new VerbResult(true, "");
            }

            return new VerbResult(false, "Item is already in that container");
        }

        public VerbResult Add(Thing thing) => Add(thing.id);

        public bool Contains(Dbref id) => contents.Contains(id);

        public bool Contains(Thing thing) => contents.Contains(thing.id);

        public Dbref FirstContent()
        {
            return contents.Count == 0
                ? Dbref.NOT_FOUND
                : contents.ToImmutableList()
                    .OrderBy(d => d.ToInt32())
                    .First();
        }

        public Dbref NextContent(Dbref lastContent)
        {
            if (this.contents.Count == 0)
                return Dbref.NOT_FOUND;

            return this.contents.ToImmutableList()
                .OrderBy(k => k.ToInt32())
                .SkipWhile(k => k <= lastContent)
                .DefaultIfEmpty(Dbref.NOT_FOUND)
                .FirstOrDefault();
        }

        public VerbResult Remove(Dbref id)
        {
            if (id == this.id)
                return new VerbResult(false, "A thing cannot come from itself");

            if (this.contents.TryRemove(id))
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
                while (!linkTargets.TryAdd(linkTarget)) { }
            Dirty = true;
        }

        public static T? Deserialize<T>(string serialized) where T : Thing, new()
        {
            if (serialized == null)
                throw new System.ArgumentNullException(nameof(serialized));

            var root = DeserializePart(serialized).Select(k => k.Item1).Cast<Dictionary<string, object>>().Single();

            dynamic expando = new ExpandoObject();
            foreach (var prop in root)
            {
                if (prop.Value is Tuple<object, string> propAsTuple)
                {
                    var propValue = propAsTuple.Item1;
                    if (propValue is Dictionary<string, object> dictionary)
                    {
                        ((IDictionary<string, object>)expando)[prop.Key] =
                            dictionary
                            .Select(x => new { x.Key, Value = ((Tuple<object, string>)x.Value).Item1 })
                            .ToDictionary(k => k.Key, v => v.Value);
                    }
                    else
                        ((IDictionary<string, object>)expando)[prop.Key] = propValue;
                }
                else
                    ((IDictionary<string, object>)expando)[prop.Key] = prop.Value;
            }

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(expando, new Newtonsoft.Json.JsonConverter[] {
                new ConcurrentDbrefSetSerializer(),
                 new DbrefSerializer()
            });
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(json, new Newtonsoft.Json.JsonConverter[] {
                new ConcurrentDbrefSetSerializer(),
                new DbrefSerializer()
            });

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
                var inner = serialized.FindInnerXml("string");
                yield return Tuple.Create<object?, string>(System.Web.HttpUtility.HtmlDecode(inner.inner), serialized[inner.endOfClosingTag..]);
            }
            else if (serialized.StartsWith("<lock/>"))
            {
                var substring = serialized[7..]; // "<lock/>".Length = 7
                yield return Tuple.Create<object?, string>(Dbref.NOT_FOUND, substring);
            }
            else if (serialized.StartsWith("<lock>"))
            {
                var inner = serialized.FindInnerXml("lock");
                yield return Tuple.Create<object?, string>(new Lock(inner.inner), serialized[inner.endOfClosingTag..]);
            }
            else if (serialized.StartsWith("<dbref/>"))
            {
                var substring = serialized[8..]; // "<dbref/>".Length = 8
                yield return Tuple.Create<object?, string>(Dbref.NOT_FOUND, substring);
            }
            else if (serialized.StartsWith("<dbref>"))
            {
                var inner = serialized.FindInnerXml("dbref");
                yield return Tuple.Create<object?, string>(new Dbref(inner.inner), serialized[inner.endOfClosingTag..]);
            }
            else if (serialized.StartsWith("<float/>"))
                yield return Tuple.Create<object?, string>((float?)null, serialized[8..]); // "<float/>".Length = 8
            else if (serialized.StartsWith("<float>"))
            {
                var inner = serialized.FindInnerXml("float");
                yield return Tuple.Create<object?, string>(float.Parse(inner.inner), serialized[inner.endOfClosingTag..]);
            }
            else if (serialized.StartsWith("<integer/>"))
            {
                var substring = serialized[10..]; // "<integer/>".Length = 10
                yield return Tuple.Create<object?, string>((int?)null, substring);
            }
            else if (serialized.StartsWith("<integer>"))
            {
                var inner = serialized.FindInnerXml("integer");
                yield return Tuple.Create<object?, string>(int.Parse(inner.inner), serialized[inner.endOfClosingTag..]);
            }
            else if (serialized.StartsWith("<uint16/>"))
                yield return Tuple.Create<object?, string>(null, serialized[9..]); // "<uint16/>".Length = 9
            else if (serialized.StartsWith("<uint16>"))
            {
                var inner = serialized.FindInnerXml("uint16");
                yield return Tuple.Create<object?, string>(UInt16.Parse(inner.inner), serialized[inner.endOfClosingTag..]);
            }
            else if (serialized.StartsWith("<date/>"))
                yield return Tuple.Create<object?, string>(null, serialized[7..]); // "<date/>".Length = 7
            else if (serialized.StartsWith("<date>"))
            {
                var inner = serialized.FindInnerXml("date");
                var dto = DateTimeOffset.Parse(inner.inner, null, DateTimeStyles.RoundtripKind);
                yield return Tuple.Create<object?, string>(dto.UtcDateTime, serialized[inner.endOfClosingTag..]);
            }
            else if (serialized.StartsWith("<array/>"))
                yield return Tuple.Create<object?, string>(null, serialized[8..]); // "<array/>".Length = 8
            else if (serialized.StartsWith("<array>"))
            {
                var inner = serialized.FindInnerXml("array");
                var array = new List<object>();
                string substring = inner.inner;

                while (!substring.StartsWith("</array>") && substring.Length > 0)
                {
                    var valueResult = DeserializePart(substring).Single();
                    substring = valueResult.Item2;
                    if (valueResult.Item1 != null)
                        array.Add(valueResult.Item1);
                }
                yield return Tuple.Create<object?, string>(array, serialized[inner.endOfClosingTag..]);
            }
            else if (serialized.StartsWith("<refs/>"))
                yield return Tuple.Create<object?, string>(null, serialized[7..]); // "<refs/>".Length = 7
            else if (serialized.StartsWith("<refs>"))
            {
                var inner = serialized.FindInnerXml("refs");
                var cds = new ConcurrentDbrefSet();
                string substring = inner.inner;

                while (!substring.StartsWith("</refs>") && substring.Length > 0)
                {
                    var valueResult = DeserializePart(substring).Single();
                    substring = valueResult.Item2;
                    if (valueResult.Item1 != null && valueResult.Item1 is Dbref dbref)
                        cds.TryAdd(dbref);
                }
                yield return Tuple.Create<object?, string>(cds, serialized[inner.endOfClosingTag..]);
            }
            else if (serialized.StartsWith("<propdir>"))
            {
                var inner = serialized.FindInnerXml("propdir");

                var propdir = new PropertyDirectory();
                var idx = 0;
                while (true)
                {
                    var keyInner = serialized[idx..].FindInnerXml("key");
                    if (keyInner.endOfClosingTag == -1)
                        break;
                    idx += keyInner.endOfClosingTag;
                    var valueInner = serialized[idx..].FindInnerXml("value");
                    idx += valueInner.endOfClosingTag;

                    var valueResult = DeserializePart(valueInner.inner).Single();
                    propdir.AddInPath(keyInner.inner, (Property)valueResult.Item1);
                    //propdir.Add((string)keyResult.Item1, (Property)((Tuple<object, string>)valueResult.Item1).Item1);
                }
                yield return Tuple.Create<object?, string>(propdir, serialized[inner.endOfClosingTag..]); // "</propdir>".Length = 10
            }
            else if (serialized.StartsWith("<prop>"))
            {
                var inner = serialized.FindInnerXml("prop");

                var nameInner = serialized.FindInnerXml("name");
                var propertyName = nameInner.inner!;
                var propertyValue = DeserializePart(serialized[nameInner.endOfClosingTag..]).Single().Item1;

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

                yield return Tuple.Create<object?, string>(property, serialized[inner.endOfClosingTag..]);
            }
            else if (serialized.StartsWith("<dict/>"))
            {
                var substring = serialized[7..]; // "<dict/>".Length = 7
                yield return Tuple.Create<object?, string>(null, substring);
            }
            else if (serialized.StartsWith("<dict>"))
            {
                var inner = serialized.FindInnerXml("dict");
                var dict = new Dictionary<string, object?>();
                var idx = 0;

                while (true)
                {
                    var keyInner = serialized[idx..].FindInnerXml("key");
                    if (keyInner.endOfClosingTag == -1)
                        break;
                    idx += keyInner.endOfClosingTag;
                    var valueInner = serialized[idx..].FindInnerXml("value");
                    idx += valueInner.endOfClosingTag;

                    var valueResult = DeserializePart(valueInner.inner).Single();
                    dict.Add(keyInner.inner, valueResult.Item1);
                }
                yield return Tuple.Create<object?, string>(dict, serialized[inner.endOfClosingTag..]);
            }
            else if (serialized.StartsWith("<value>"))
            {
                var inner = serialized.FindInnerXml("value");
                var valueResult = DeserializePart(inner.inner).Single();
                yield return Tuple.Create<object?, string>(valueResult.Item1, serialized[inner.endOfClosingTag..]);
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
            properties?.ClearPropertyPathValue(path);
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
            { nameof(id), id },
            { nameof(name), name },
            { "location", Location },
            { "contents", contents },
            { "linkTargets", linkTargets },
            { "templates", templates },
            { "flags", flags },
            { "externalDescription", externalDescription },
            { "owner", Owner },
            { "pennies", pennies },
            { "properties", properties }
        };

        public static string Serialize(PropertyDirectory value) => PropertyDirectory.Serialize(value);

        public static string Serialize(Dictionary<string, object?>? value)
        {
            if (value == null)
                return "<dict/>";

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

        public static string Serialize(ConcurrentDbrefSet? value)
        {
            if (value == null)
                return "<refs/>";

            var sb = new StringBuilder();
            sb.Append("<refs>");
            foreach (var dbref in value.ToImmutableArray().OrderBy(v => v))
            {
                sb.Append(Serialize(dbref));
            }
            sb.Append("</refs>");
            return sb.ToString();
        }

        public static string Serialize<T>(T[]? array)
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
            if (typeof(ConcurrentDbrefSet).IsAssignableFrom(valueType))
                return Serialize((ConcurrentDbrefSet)value);
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

        public async Task<string> UnparseObject(Dbref player, CancellationToken cancellationToken)
        {
            // See https://github.com/fuzzball-muck/fuzzball/blob/b0ea12f4d40a724a16ef105f599cb8b6a037a77a/src/db.c#L1381
            if (player != Dbref.NOT_FOUND)
                player = await player.GetOwner(cancellationToken); // In case the object is a zombie.

            switch (player.Number)
            {
                case Dbref.DBREF_NUMBER_NOT_FOUND: // If only .NET has const structs..
                    return "*NOTHING*";
                case Dbref.DBREF_NUMBER_AMBIGUOUS:
                    return "*AMBIGUOUS*";
                case Dbref.DBREF_NUMBER_HOME:
                    return "*HOME*";
                case Dbref.DBREF_NUMBER_NIL:
                    return "*NIL*";
                default:
                    /* Show the details if:
                     * player == NOT_FOUND
                     * or player does not have STICKY flag AND:
                     *   'loc' has flags that can be seen - @see can_see_flags
                     *   or 'loc' is not a player and 'player' controls 'loc'
                     *   or 'loc' is CHOWN_OK
                     */
                    if (player == Dbref.NOT_FOUND)
                        return UnparseObjectInternal();

                    var playerObj = await player.Get(cancellationToken);
                    if (playerObj != null
                        && !playerObj.HasFlag(Flag.STICKY)
                        && (
                            HasFlag(Flag.CHOWN_OK)
                            || Type != Dbref.DbrefObjectType.Player && this.IsControlledBy(playerObj)
                            || playerObj.CanTeleportTo(this)))
                        return UnparseObjectInternal();

                    // Otherwise, we just share the name.
                    return name ?? "*UNNAMED*";
            }
        }

        public string UnparseObjectInternal()
        {
            var flagString = this.flags == null || this.flags.Length == 0 ? string.Empty : this.flags.Select(f => ((char)f).ToString()).Aggregate((c, n) => $"{c}{n}");
            return $"{this.name}(#{this.id.ToInt32()}{(char)this.id.Type}{flagString})";
        }

        public bool IsGod() => this.id == Dbref.GOD;

        public bool IsLinkable => (Type == Dbref.DbrefObjectType.Room || Type == Dbref.DbrefObjectType.Thing)
                ? HasFlag(Flag.ABODE)
                : HasFlag(Flag.LINK_OK);
    }
}