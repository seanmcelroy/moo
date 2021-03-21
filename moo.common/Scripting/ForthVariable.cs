using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using moo.common.Models;

namespace moo.common.Scripting
{
    public struct ForthVariable
    {
        public static readonly ForthVariable UNINITIALIZED = new(null, VariableType.Unknown, true);
        public enum VariableType
        {
            Unknown,
            String,
            Integer,
            DbRef,
            Float,
            Array
        }

        public bool IsConstant;
        public VariableType Type;
        public object? Value;

        public ForthVariable(ForthDatum value, bool isConstant = false)
        {
            IsConstant = isConstant;
            Type = value.Type switch
            {
                ForthDatum.DatumType.DbRef => VariableType.DbRef,
                ForthDatum.DatumType.Float => VariableType.Float,
                ForthDatum.DatumType.Integer => VariableType.Integer,
                ForthDatum.DatumType.String => VariableType.String,
                ForthDatum.DatumType.Array => VariableType.Array,
                _ => throw new System.ArgumentException($"Unhandled variable type: {value.Type}", nameof(value)),
            };
            Value = value.Value;
        }

        public ForthVariable(object? value, VariableType type, bool isConstant)
        {
            IsConstant = isConstant;
            Type = type;
            Value = value;
        }

        public ForthVariable(Dbref value, byte dud, int? lineNumber = null, bool isConstant = false)
        {
            IsConstant = isConstant;
            Value = value;
            Type = VariableType.DbRef;
        }

        public ForthVariable(string value, int? lineNumber = null, int? columnNumber = null, bool isConstant = false)
        {
            IsConstant = isConstant;
            Value = value;
            Type = VariableType.String;
        }

        public ForthVariable(int? value, int? lineNumber = null, int? columnNumber = null, bool isConstant = false)
        {
            IsConstant = isConstant;
            Value = value;
            Type = VariableType.Integer;
        }

        public ForthVariable(float? value, int? lineNumber = null, int? columnNumber = null, bool isConstant = false)
        {
            IsConstant = isConstant;
            Value = value;
            Type = VariableType.Float;
        }

        public static bool TryInferType([NotNullWhen(true)] string? value, out Tuple<VariableType, object?>? result)
        {
            if (value == null)
            {
                result = default;
                return false;
            }

            if (int.TryParse(value.Trim(), out int i))
            {
                result = new Tuple<VariableType, object?>(VariableType.Integer, i);
                return true;
            }

            if (float.TryParse(value.Trim(), out float f))
            {
                result = new Tuple<VariableType, object?>(VariableType.Float, f);
                return true;
            }

            if (Regex.IsMatch(value, @"#(\-?\d+|\d+[A-Z]?)", RegexOptions.Compiled))
            {
                result = new Tuple<VariableType, object?>(VariableType.DbRef, new Dbref(value));
                return true;
            }

            if (value.StartsWith('\"') && value.EndsWith('\"'))
            {
                result = new Tuple<VariableType, object?>(VariableType.String, value);
                return true;
            }

            result = new Tuple<VariableType, object?>(VariableType.Unknown, null);
            return false;
        }
    }
}