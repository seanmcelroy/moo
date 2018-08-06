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
        Variable = 7
    }

    public readonly object Value;
    public DatumType Type;
    public readonly int? LineNumber;
    public readonly int? ColumnNumber;

    public ForthDatum(object value, DatumType type, int? lineNumber = null, int? columnNumber = null)
    {
        this.Value = value;
        this.Type = type;
        this.LineNumber = lineNumber;
        this.ColumnNumber = columnNumber;
    }

    public ForthDatum(Dbref value, byte dud, int? lineNumber = null, int? columnNumber = null)
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
                return (int)Value == 0;
            case DatumType.Float:
                return (float)Value == 0;
            case DatumType.DbRef:
                return ((Dbref)Value).ToInt32() == -1;
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

        throw new InvalidCastException("Cannot unwrap property as dbref, underlying type is: " + Value.GetType().Name);
    }
}