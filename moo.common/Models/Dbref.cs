using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using moo.common.Database;
using static moo.common.Models.Thing;

namespace moo.common.Models
{
    [JsonConverter(typeof(DbrefSerializer))]
    public struct Dbref
    {
        internal const int DBREF_NUMBER_NOT_FOUND = -1;
        internal const int DBREF_NUMBER_AMBIGUOUS = -2;
        internal const int DBREF_NUMBER_HOME = -3;
        internal const int DBREF_NUMBER_NIL = -4;

        public enum DbrefObjectType : UInt16
        {
            Unknown = '?',
            Garbage = 'G',
            Thing = 'T',
            Room = 'R',
            Player = 'P',
            Exit = 'E',
            Program = 'F'
        }

        public static readonly Dbref GOD = new(1, DbrefObjectType.Player);
        public static readonly Dbref AETHER = new(0, DbrefObjectType.Room);
        public static readonly Dbref NOT_FOUND = new(DBREF_NUMBER_NOT_FOUND, DbrefObjectType.Thing);
        public static readonly Dbref AMBIGUOUS = new(DBREF_NUMBER_AMBIGUOUS, DbrefObjectType.Thing);
        public static readonly Dbref HOME = new(DBREF_NUMBER_HOME, DbrefObjectType.Thing);
        public static readonly Dbref NIL = new(DBREF_NUMBER_NIL, DbrefObjectType.Thing);

        private readonly int id;
        private readonly DbrefObjectType type;

        [JsonIgnore]
        public int Number => id;

        [JsonIgnore]
        public DbrefObjectType Type => type;

        public Dbref(int id, DbrefObjectType type)
        {
            this.id = id;
            this.type = type;
        }

        public Dbref(string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Value is null or whitespace", nameof(id));
            if (!id.StartsWith("#"))
                throw new ArgumentException("Value does not start with #", nameof(id));

            char designator = id[^1];
            type = designator switch
            {
                'E' => DbrefObjectType.Exit,
                'G' => DbrefObjectType.Garbage,
                'P' => DbrefObjectType.Player,
                'F' => DbrefObjectType.Program,
                'R' => DbrefObjectType.Room,
                'T' => DbrefObjectType.Thing,
                '0' or '1' or '2' or '3' or '4' or '5' or '6' or '7' or '8' or '9' => DbrefObjectType.Unknown,
                _ => throw new System.ArgumentException($"Unknown designator for {id}{designator}"),
            };
            if (type == DbrefObjectType.Unknown)
            {
                if (!int.TryParse(id[1..], out int idInt))
                    throw new System.ArgumentException($"Could not parse '{id}' as a dbref");

                this.id = idInt;
            }
            else
                this.id = int.Parse(id[1..^1]);
        }

        public static explicit operator int(Dbref dbref) => dbref.id;

        // Required for deserialization from JSON
        public static explicit operator Dbref(string refId) => new(refId);

        public static bool operator ==(Dbref item1, Dbref item2) => item1.id == item2.id;

        public static bool operator !=(Dbref item1, Dbref item2) => item1.id != item2.id;

        public static bool operator <(Dbref item1, Dbref item2) => item1.id < item2.id;

        public static bool operator >(Dbref item1, Dbref item2) => item1.id > item2.id;

        public static bool operator <=(Dbref item1, Dbref item2) => item1.id <= item2.id;

        public static bool operator >=(Dbref item1, Dbref item2) => item1.id >= item2.id;

        public static Dbref operator |(Dbref left, Dbref right)
        {
            if (AMBIGUOUS.Equals(left) || AMBIGUOUS.Equals(right))
                return AMBIGUOUS;
            if (NOT_FOUND.Equals(left) && NOT_FOUND.Equals(right))
                return NOT_FOUND;

            var leftDefined = !AMBIGUOUS.Equals(left) && !NOT_FOUND.Equals(left) && !default(Dbref).Equals(left);
            var rightDefined = !AMBIGUOUS.Equals(right) && !NOT_FOUND.Equals(right) && !default(Dbref).Equals(right);

            if (leftDefined && rightDefined)
                return AMBIGUOUS;
            if (!leftDefined && !rightDefined)
                return NOT_FOUND;
            return leftDefined ? left : right;
        }

        public static async Task<bool> CanLink(Dbref whoDbref, Dbref whatDbref, CancellationToken cancellationToken)
        {
            // Anyone can link an exit that is currently unlinked.
            var (controls, _, what) = await whoDbref.Controls(whatDbref, cancellationToken);
            return controls
                || (what != null && what.Type == DbrefObjectType.Exit && (await ThingRepository.Instance.GetAsync<Exit>(whatDbref, cancellationToken)).value?.LinkTargets.Count == 0);
        }

        public static async Task<bool> CanLinkTo(Dbref whoDbref, DbrefObjectType whatType, Dbref whereDbref, CancellationToken cancellationToken)
        {
            // Can always link to HOME 
            if (whereDbref == HOME)
                return true;

            // Exits can be linked to NIL 
            if (whatType == DbrefObjectType.Exit && whereDbref == NIL)
                return true;

            // Can't link to an invalid or recycled dbref
            if (!whereDbref.IsValid() || whereDbref.ToInt32() < 0 || whereDbref.Type == DbrefObjectType.Garbage)
                return false;

            // Players can only be linked to rooms
            if (whatType == DbrefObjectType.Player && whereDbref.type != DbrefObjectType.Room)
                return false;

            // Rooms can only be linked to things or other rooms
            if (whatType == DbrefObjectType.Room
                && whereDbref.Type != DbrefObjectType.Thing && whereDbref.Type != DbrefObjectType.Room)
                return false;

            // Things cannot be linked to exits or programs
            if (whatType == DbrefObjectType.Thing
                && (whereDbref.Type == DbrefObjectType.Exit || whereDbref.Type == DbrefObjectType.Program))
                return false;

            // Programs cannot be linked
            if (whatType == DbrefObjectType.Program)
                return false;

            // Target must be controlled or publicly linkable with its linklock passed
            var (whoControlsWhere, _, where) = await whoDbref.Controls(whereDbref, cancellationToken);
            return whoControlsWhere
                || (where.IsLinkable && true); // TODO: test_lock(NOTHING, who, where, MESGPROP_LINKLOCK));
        }

        public static Dbref Parse(string? s) => !TryParse(s, out Dbref result) ? NOT_FOUND : result;

        public static bool TryParse([NotNullWhen(true)] string? s, out Dbref result)
        {
            if (s == null)
            {
                result = NOT_FOUND;
                return false;
            }

            s = s.Trim();

            var m = Regex.Match(s, @"^#(?<num>\d+)(?<type>[EGPFRT]?)$");
            if (m.Success)
            {
                var num = int.Parse(m.Groups["num"].Value);
                char? typeChar = m.Groups["type"].Success && !string.IsNullOrEmpty(m.Groups["type"].Value)
                    ? m.Groups["type"].Value[0]
                    : null;
                var type = (typeChar) switch
                {
                    'E' => DbrefObjectType.Exit,
                    'G' => DbrefObjectType.Garbage,
                    'P' => DbrefObjectType.Player,
                    'F' => DbrefObjectType.Program,
                    'R' => DbrefObjectType.Room,
                    _ => DbrefObjectType.Thing,
                };
                result = new Dbref(num, type);
                return true;
            }

            result = NOT_FOUND;
            return false;
        }

        public override int GetHashCode() => id;

        private static async Task<Dbref> GetParent(Dbref obj, CancellationToken cancellationToken)
        {
            if (obj == NOT_FOUND)
                return NOT_FOUND;

            var objLookup = await ThingRepository.Instance.GetAsync<Thing>(obj, cancellationToken);
            if (!objLookup.isSuccess || objLookup.value == null)
                return NOT_FOUND;

            return GetParent(objLookup.value);
        }

        private static Dbref GetParent(Thing obj)
        {
            return obj.Location; // TODO: See https://github.com/fuzzball-muck/fuzzball/blob/b0ea12f4d40a724a16ef105f599cb8b6a037a77a/src/db.c#L1734
        }

        public static async Task<int> EnvironmentalDistance(Dbref from, Dbref to, CancellationToken cancellationToken)
        {
            int distance = 0;
            var dest = await GetParent(to, cancellationToken);

            if (from == dest)
                return 0;

            do
            {
                distance++;
            } while ((from = await GetParent(from, cancellationToken)) != dest && from != NOT_FOUND);

            return distance;
        }

        public bool Equals(Dbref obj)
        {
            return obj.id == this.id;
        }

        public override bool Equals(object? obj) => obj is Dbref dbref && this.Equals(dbref);

        public bool IsGod() => id == GOD.id;

        public async Task<bool> IsWizard(CancellationToken cancellationToken)
        {
            if (IsGod())
                return true;
            if (Type != DbrefObjectType.Player)
                return false;
            var playerLookup = await ThingRepository.Instance.GetAsync<Thing>(this, cancellationToken);
            return playerLookup.isSuccess && playerLookup.value != null && playerLookup.value.HasFlag(Flag.WIZARD);
        }

        public bool IsValid() => !(Equals(NOT_FOUND) || Equals(AMBIGUOUS));

        public int ToInt32() => this.id;

        public override string ToString()
        {
            var ct = (char)type;
            return id < 0 || type == DbrefObjectType.Unknown || ct == '\0' ? $"#{id}" : $"#{id}{(char)type}";
        }
    }
}