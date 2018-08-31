using System;
using System.Text.RegularExpressions;
using static Dbref;

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
        Float = 6,
        Variable = 7,
        Lock = 8
    }

    public readonly object Value;
    public DatumType Type;
    public readonly int? LineNumber;
    public readonly int? ColumnNumber;

    public ForthDatum(Property property, int? lineNumber = null, int? columnNumber = null)
    {
        this.Value = property.Value;
        switch (property.Type)
        {
            case Property.PropertyType.DbRef:
                this.Type = ForthDatum.DatumType.DbRef;
                break;
            case Property.PropertyType.Float:
                this.Type = ForthDatum.DatumType.Float;
                break;
            case Property.PropertyType.Integer:
                this.Type = ForthDatum.DatumType.Integer;
                break;
            case Property.PropertyType.String:
                this.Type = ForthDatum.DatumType.String;
                break;
                case Property.PropertyType.Lock:
                this.Type = ForthDatum.DatumType.Lock;
                break;
            case Property.PropertyType.Unknown:
                this.Type = ForthDatum.DatumType.Unknown;
                break;
            default:
                throw new System.ArgumentException($"Unhandled variable type: {property.Type}", nameof(property));
        }

        this.LineNumber = lineNumber;
        this.ColumnNumber = columnNumber;
    }

    public ForthDatum(ForthVariable variable, int? lineNumber = null, int? columnNumber = null)
    {
        this.Value = variable.Value;
        switch (variable.Type)
        {
            case ForthVariable.VariableType.DbRef:
                this.Type = ForthDatum.DatumType.DbRef;
                break;
            case ForthVariable.VariableType.Float:
                this.Type = ForthDatum.DatumType.Float;
                break;
            case ForthVariable.VariableType.Integer:
                this.Type = ForthDatum.DatumType.Integer;
                break;
            case ForthVariable.VariableType.String:
                this.Type = ForthDatum.DatumType.String;
                break;
            case ForthVariable.VariableType.Unknown:
                this.Type = ForthDatum.DatumType.Unknown;
                break;
            default:
                throw new System.ArgumentException($"Unhandled variable type: {variable.Type}", nameof(variable));
        }

        this.LineNumber = lineNumber;
        this.ColumnNumber = columnNumber;
    }

    public ForthDatum(object value, DatumType type, int? lineNumber = null, int? columnNumber = null)
    {
        this.Value = value;
        this.Type = type;
        this.LineNumber = lineNumber;
        this.ColumnNumber = columnNumber;
    }

    public ForthDatum(Dbref value, int? lineNumber = null, int? columnNumber = null)
    {
        this.Value = value;
        this.Type = DatumType.DbRef;
        this.LineNumber = lineNumber;
        this.ColumnNumber = columnNumber;
    }

    public ForthDatum(string value, int? lineNumber = null, int? columnNumber = null)
    {
        this.Value = value;
        this.Type = DatumType.String;
        this.LineNumber = lineNumber;
        this.ColumnNumber = columnNumber;
    }

    public ForthDatum(int? value, int? lineNumber = null, int? columnNumber = null)
    {
        this.Value = value;
        this.Type = DatumType.Integer;
        this.LineNumber = lineNumber;
        this.ColumnNumber = columnNumber;
    }

    public ForthDatum(float? value, int? lineNumber = null, int? columnNumber = null)
    {
        this.Value = value;
        this.Type = DatumType.Float;
        this.LineNumber = lineNumber;
        this.ColumnNumber = columnNumber;
    }

    public static bool TryInferType(string value, out Tuple<DatumType, object> result)
    {
        if (int.TryParse(value.Trim(), out int i))
        {
            result = new Tuple<DatumType, object>(DatumType.Integer, i);
            return true;
        }

        if (float.TryParse(value.Trim(), out float f))
        {
            result = new Tuple<DatumType, object>(DatumType.Float, f);
            return true;
        }

        if (Regex.IsMatch(value, @"#(\-?\d+|\d+[A-Z]?)"))
        {
            result = new Tuple<DatumType, object>(DatumType.DbRef, new Dbref(value));
            return true;
        }

        if (value.StartsWith('\"') && value.EndsWith('\"'))
        {
            result = new Tuple<DatumType, object>(DatumType.String, value);
            return true;
        }

        result = default(Tuple<DatumType, object>);
        return false;
    }

    public bool isFalse()
    {
        switch (Type)
        {
            case DatumType.Integer:
                return UnwrapInt() == 0;
            case DatumType.Float:
                return (float)Value == 0;
            case DatumType.DbRef:
                return UnwrapDbref().ToInt32() == -1;
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
                return new ForthDatum(UnwrapDbref().ToInt32());
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
            throw new InvalidCastException($"Cannot unwrap property as dbref, since it is of type: {Type}");

        if (Value.GetType() == typeof(Dbref))
        {
            return (Dbref)Value;
        }

        if (Value.GetType() == typeof(string))
        {
            return new Dbref((string)Value);
        }

        if (Value.GetType() == typeof(int))
        {
            return new Dbref((int)Value, DbrefObjectType.Thing);
        }

        throw new InvalidCastException($"Cannot unwrap property as dbref, underlying type is: {Value.GetType().Name}");
    }

    public int UnwrapInt()
    {
        if (Type != DatumType.Integer)
            throw new InvalidCastException($"Cannot unwrap property as int, since it is of type: {Type}");

        if (Value.GetType() == typeof(int))
        {
            return (int)Value;
        }

        if (Value.GetType() == typeof(string))
        {
            if (int.TryParse((string)Value, out int i))
                return i;

            throw new InvalidCastException($"Cannot unwrap property as int, unable to parse: {Value}");
        }

        throw new InvalidCastException($"Cannot unwrap property as dbref, underlying type is: {Value.GetType().Name}");
    }
}