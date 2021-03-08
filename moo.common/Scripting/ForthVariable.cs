using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using moo.common.Models;

namespace moo.common.Scripting
{
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
        public object? Value;

        public ForthVariable(ForthDatum value, bool isConstant = false)
        {
            this.IsConstant = isConstant;
            this.Type = value.Type switch
            {
                ForthDatum.DatumType.DbRef => VariableType.DbRef,
                ForthDatum.DatumType.Float => VariableType.Float,
                ForthDatum.DatumType.Integer => VariableType.Integer,
                ForthDatum.DatumType.String => VariableType.String,
                _ => throw new System.ArgumentException($"Unhandled variable type: {value.Type}", nameof(value)),
            };
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

            if (Regex.IsMatch(value, @"#(\-?\d+|\d+[A-Z]?)"))
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