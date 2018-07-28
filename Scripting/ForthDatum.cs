public struct ForthDatum
{
    public enum DatumType
    {
        String = 1,
        Integer = 2,
        Primitive = 3
    }

    public object Value;
    public DatumType Type;

    public ForthDatum(object value, DatumType type)
    {
        this.Value = value;
        this.Type = type;
    }
}