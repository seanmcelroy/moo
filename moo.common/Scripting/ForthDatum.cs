public struct ForthDatum
{
    public enum DatumType
    {
        Unknown = 0,
        String = 1,
        Integer = 2,
        Primitive = 3,
        Marker = 4,
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
}