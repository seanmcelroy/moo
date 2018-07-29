using System;

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

    public static implicit operator int(Dbref dbref)
    {
        return dbref.id;
    }

    public static implicit operator Dbref(Int64 refId)
    {
        return new Dbref(Convert.ToInt32(refId));
    }


    public override string ToString()
    {
        return "#" + id;
    }
}