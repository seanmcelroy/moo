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

    public ForthVariable(Dbref value, byte dud, int? lineNumber = null, bool isConstant = false)
    {
        this.IsConstant = isConstant;
        this.Value = value;
        this.Type = VariableType.DbRef;
    }

    public ForthVariable(string value, int? lineNumber = null, int? columnNumber = null, bool isConstant = false)
    {
        this.IsConstant = isConstant;
        this.Value = value;
        this.Type = VariableType.String;
    }

    public ForthVariable(int? value, int? lineNumber = null, int? columnNumber = null, bool isConstant = false)
    {
        this.IsConstant = isConstant;
        this.Value = value;
        this.Type = VariableType.Integer;
    }

    public ForthVariable(float? value, int? lineNumber = null, int? columnNumber = null, bool isConstant = false)
    {
        this.IsConstant = isConstant;
        this.Value = value;
        this.Type = VariableType.Float;
    }
}