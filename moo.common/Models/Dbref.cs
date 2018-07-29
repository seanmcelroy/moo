using System;
using Newtonsoft.Json;

[JsonConverter(typeof(DbrefSerializer))]
public struct Dbref
{
    public static readonly Dbref NOT_FOUND = new Dbref(-1);
    public static readonly Dbref AMBIGUOUS = new Dbref(-2);
    public static readonly Dbref HOME = new Dbref(-3);

    private readonly int id;

    public Dbref(int id)
    {
        this.id = id;
    }

    public Dbref(string id)
    {
        if (id == null)
            throw new ArgumentNullException(nameof(id));
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Value is null or whitespace", nameof(id));
        if (!id.StartsWith("#"))
            throw new ArgumentException("Value does not start with #", nameof(id));

        this.id = int.Parse(id.Substring(1));
    }

    public static explicit operator int(Dbref dbref) => dbref.id;
    public static explicit operator Dbref(string dbref) => new Dbref(dbref);

    // Required for deserialization from JSON
    public static implicit operator Dbref(Int64 refId) => new Dbref(Convert.ToInt32(refId));

    public static bool operator ==(Dbref item1, Dbref item2)
    {
        return item1.id == item2.id;
    }

    public static bool operator !=(Dbref item1, Dbref item2)
    {
        return item1.id != item2.id;
    }

    public override int GetHashCode()
    {
        return id;
    }

    public bool Equals(Dbref obj)
    {
        return obj.id == this.id;
    }

    public override string ToString()
    {
        return "#" + id;
    }
}