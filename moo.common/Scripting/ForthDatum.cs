using System;

public struct ForthDatum
{
    public enum DatumType
    {
        Unknown = 0,
        String = 1,
        Integer = 2,
        Primitive = 3,
        Marker = 4,
        DbRef = 5,
        Float = 6
    }

    public object Value;
    public DatumType Type;

    public ForthDatum(object value, DatumType type)
    {
        this.Value = value;
        this.Type = type;
    }

    public ForthDatum(Dbref value, byte dud)
    {
        this.Value = value;
        this.Type = DatumType.DbRef;
    }

    public ForthDatum(string value)
    {
        this.Value = value;
        this.Type = DatumType.String;
    }

    public ForthDatum(int? value)
    {
        this.Value = value;
        this.Type = DatumType.Integer;
    }

    public ForthDatum(float? value)
    {
        this.Value = value;
        this.Type = DatumType.Float;
    }

    public bool isFalse()
    {
        switch (Type)
        {
            case DatumType.Integer:
                return (int)Value == 0;
            case DatumType.Float:
                return (float)Value == 0;
            case DatumType.DbRef:
                return (int)Value == -1;
            case DatumType.String:
                return string.IsNullOrEmpty((string)Value);
        }

        return false;
    }

    public bool isTrue()
    {
        return !isFalse();
    }

    public ForthDatum ToInteger()
    {
        switch (Type)
        {
            case DatumType.Float:
                return new ForthDatum(Convert.ToInt32((float)Value), DatumType.Integer);
            case DatumType.Integer:
                return this;
            case DatumType.DbRef:
                return new ForthDatum((Dbref)Value, DatumType.Integer);
        }

        return new ForthDatum(0);
    }

    public override string ToString()
    {
        return $"({Enum.GetName(typeof(DatumType), Type)}){Value}";
    }

    public Dbref UnwrapDbref()
    {
        if (Type != DatumType.DbRef)
            throw new InvalidCastException("Cannot unwrap property as dbref, since it is of type: " + Type);

        if (Value.GetType() == typeof(Dbref)) {
            return (Dbref)Value;
        }

        if (Value.GetType() == typeof(string)) {
            return new Dbref((string)Value);
        }

        if (Value.GetType() == typeof(int)) {
            return new Dbref((int)Value);
        }

        throw new InvalidCastException("Cannot unwrap property as dbref, underlying type is: " + Value.GetType().Name);
    }
}