public struct ForthVariable
{
    public enum VariableType
    {
        Unknown,
        String,
        Integer,
        DbRef,
        Float
    }

    public bool IsConstant;
    public VariableType Type;
    public object Value;

    public ForthVariable(object value, VariableType type, bool isConstant)
    {
        this.IsConstant = isConstant;
        this.Type = type;
        this.Value = value;
    }
}