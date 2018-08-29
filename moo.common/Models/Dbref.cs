using System;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

[JsonConverter(typeof(DbrefSerializer))]
public struct Dbref
{
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

    public static readonly Dbref[] EMPTY_SET = new Dbref[0];
    public static readonly Dbref GOD = new Dbref(1, DbrefObjectType.Player);
    public static readonly Dbref AETHER = new Dbref(0, DbrefObjectType.Room);
    public static readonly Dbref NOT_FOUND = new Dbref(-1, DbrefObjectType.Thing);
    public static readonly Dbref AMBIGUOUS = new Dbref(-2, DbrefObjectType.Thing);
    public static readonly Dbref HOME = new Dbref(-3, DbrefObjectType.Thing);

    private readonly int id;
    private readonly DbrefObjectType type;

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

        char designator = id[id.Length - 1];
        switch (designator)
        {
            case 'E':
                type = DbrefObjectType.Exit;
                break;
            case 'P':
                type = DbrefObjectType.Player;
                break;
            case 'F':
                type = DbrefObjectType.Program;
                break;
            case 'R':
                type = DbrefObjectType.Room;
                break;
            case 'T':
                type = DbrefObjectType.Thing;
                break;
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
                type = DbrefObjectType.Unknown;
                break;
            default:
                throw new System.ArgumentException($"Unknown designator for #{id}{designator}");
        }

        if (type == DbrefObjectType.Unknown)
        {
            if (!int.TryParse(id.Substring(1, id.Length - 1), out int idInt))
                throw new System.ArgumentException($"Could not parse '{id.Substring(1, id.Length - 1)}' as a dbref");

            this.id = idInt;
        }
        else
            this.id = int.Parse(id.Substring(1, id.Length - 2));
    }

    public static explicit operator int(Dbref dbref) => dbref.id;

    // Required for deserialization from JSON
    public static implicit operator Dbref(string refId) => new Dbref(refId);

    public static bool operator ==(Dbref item1, Dbref item2)
    {
        return item1.id == item2.id;
    }

    public static bool operator !=(Dbref item1, Dbref item2)
    {
        return item1.id != item2.id;
    }

    public static bool operator <(Dbref item1, Dbref item2)
    {
        return item1.id < item2.id;
    }

    public static bool operator >(Dbref item1, Dbref item2)
    {
        return item1.id > item2.id;
    }

    public static bool operator <=(Dbref item1, Dbref item2)
    {
        return item1.id <= item2.id;
    }

    public static bool operator >=(Dbref item1, Dbref item2)
    {
        return item1.id >= item2.id;
    }

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

    public static bool TryParse(string s, out Dbref result)
    {
        s = s.Trim();
        if (Regex.IsMatch(s, @"#\d+[A-Z]?"))
        {
            DbrefObjectType type;
            switch (s[s.Length - 1])
            {
                case 'E':
                    type = DbrefObjectType.Exit;
                    break;
                case 'P':
                    type = DbrefObjectType.Player;
                    break;
                case 'F':
                    type = DbrefObjectType.Program;
                    break;
                case 'R':
                    type = DbrefObjectType.Room;
                    break;
                default:
                    type = DbrefObjectType.Thing;
                    break;
            }

            result = new Dbref(int.Parse(s.Substring(1)), type);
            return true;
        }

        result = default(Dbref);
        return false;
    }

    public override int GetHashCode()
    {
        return id;
    }

    public bool Equals(Dbref obj)
    {
        return obj.id == this.id;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is Dbref))
            return false;

        return this.Equals((Dbref)obj);
    }

    public bool IsValid()
    {
        return !(Equals(Dbref.NOT_FOUND) || Equals(Dbref.AMBIGUOUS) || Equals(default(Dbref)));
    }

    public int ToInt32()
    {
        return this.id;
    }

    public override string ToString()
    {
        if (id < 0)
            return $"#{id}";

        return $"#{id}{(char)type}";
    }
}