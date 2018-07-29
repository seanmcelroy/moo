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

    public bool isTrue() {
        return !isFalse();
    }
}