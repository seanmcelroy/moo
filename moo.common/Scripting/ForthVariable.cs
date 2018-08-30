using System;
using System.Text.RegularExpressions;

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

    public ForthVariable(ForthDatum value, bool isConstant = false)
    {
        this.IsConstant = isConstant;
        switch (value.Type)
        {
            case ForthDatum.DatumType.DbRef:
                this.Type = VariableType.DbRef;
                break;
            case ForthDatum.DatumType.Float:
                this.Type = VariableType.Float;
                break;
            case ForthDatum.DatumType.Integer:
                this.Type = VariableType.Integer;
                break;
            case ForthDatum.DatumType.String:
                this.Type = VariableType.String;
                break;
            default:
                throw new System.ArgumentException($"Unhandled variable type: {value.Type}", nameof(value));
        }
        this.Value = value.Value;
    }

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

    public static bool TryInferType(string value, out Tuple<VariableType, object> result)
    {
        if (int.TryParse(value.Trim(), out int i))
        {
            result = new Tuple<VariableType, object>(VariableType.Integer, i);
            return true;
        }

        if (float.TryParse(value.Trim(), out float f))
        {
            result = new Tuple<VariableType, object>(VariableType.Float, f);
            return true;
        }

        if (Regex.IsMatch(value, @"#(\-?\d+|\d+[A-Z]?)"))
        {
            result = new Tuple<VariableType, object>(VariableType.DbRef, new Dbref(value));
            return true;
        }

        if (value.StartsWith('\"') && value.EndsWith('\"'))
        {
            result = new Tuple<VariableType, object>(VariableType.String, value);
            return true;
        }

        result = default(Tuple<VariableType, object>);
        return false;
    }
}