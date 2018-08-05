using System;
using Newtonsoft.Json;

[JsonConverter(typeof(DbrefSerializer))]
public struct Dbref
{
    public enum DbrefObjectType
    {
        Unknown = 0,
        Thing = 1,
        Room = 2,
        Player = 3,
        Exit = 4
    }

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
                throw new System.ArgumentException("Unknown designator for #" + id + designator);
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

        var leftDefined = !AMBIGUOUS.Equals(left) && !AMBIGUOUS.Equals(left) & !AMBIGUOUS.Equals(left) && !default(Dbref).Equals(left);
        var rightDefined = !AMBIGUOUS.Equals(right) && !AMBIGUOUS.Equals(right) & !AMBIGUOUS.Equals(right) && !default(Dbref).Equals(right);

        if (leftDefined && rightDefined)
            return AMBIGUOUS;
        if (!leftDefined && !rightDefined)
            return NOT_FOUND;
        return leftDefined ? left : right;
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

    public int ToInt32()
    {
        return this.id;
    }

    public override string ToString()
    {
        if (id <= 0)
            return "#" + id;

        char designator;
        switch (type)
        {
            case DbrefObjectType.Exit:
                designator = 'E';
                break;
            case DbrefObjectType.Player:
                designator = 'P';
                break;
            case DbrefObjectType.Room:
                designator = 'R';
                break;
            case DbrefObjectType.Thing:
                designator = 'T';
                break;
            default:
                designator = '?';
                break;
        }

        return "#" + id + designator;
    }
}