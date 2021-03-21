using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using moo.common.Models;
using static moo.common.Models.Dbref;

namespace moo.common.Scripting
{
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
            Lock = 8,
            Array = 9
        }

        public readonly string? Key;
        public readonly object? Value;
        public DatumType Type;
        public readonly string? WordName;
        public readonly int? WordLineNumber;
        public readonly int? FileLineNumber;
        public readonly int? ColumnNumber;

        public ForthDatum(Property property, int? fileLineNumber = null, int? columnNumber = null, string? wordName = null, int? wordLineNumber = null)
        {
            Key = null;
            Value = property.Value;
            Type = property.Type switch
            {
                Property.PropertyType.DbRef => DatumType.DbRef,
                Property.PropertyType.Float => DatumType.Float,
                Property.PropertyType.Integer => DatumType.Integer,
                Property.PropertyType.String => DatumType.String,
                Property.PropertyType.Lock => DatumType.Lock,
                Property.PropertyType.Unknown => DatumType.Unknown,
                _ => throw new System.ArgumentException($"Unhandled variable type: {property.Type}", nameof(property)),
            };
            FileLineNumber = fileLineNumber;
            ColumnNumber = columnNumber;
            WordName = wordName;
            WordLineNumber = wordLineNumber;
        }

        public ForthDatum(ForthVariable variable, int? fileLineNumber = null, int? columnNumber = null, string? wordName = null, int? wordLineNumber = null)
        {
            Key = null;
            Value = variable.Value;
            Type = variable.Type switch
            {
                ForthVariable.VariableType.DbRef => DatumType.DbRef,
                ForthVariable.VariableType.Float => DatumType.Float,
                ForthVariable.VariableType.Integer => DatumType.Integer,
                ForthVariable.VariableType.String => DatumType.String,
                ForthVariable.VariableType.Unknown => DatumType.Unknown,
                _ => throw new System.ArgumentException($"Unhandled variable type: {variable.Type}", nameof(variable)),
            };
            FileLineNumber = fileLineNumber;
            ColumnNumber = columnNumber;
            WordName = wordName;
            WordLineNumber = wordLineNumber;
        }

        public ForthDatum(object value, DatumType type, int? fileLineNumber = null, int? columnNumber = null, string? wordName = null, int? wordLineNumber = null, string? key = null)
        {
            Key = key;
            Value = value;
            Type = type;
            FileLineNumber = fileLineNumber;
            ColumnNumber = columnNumber;
            WordName = wordName;
            WordLineNumber = wordLineNumber;
        }

        public ForthDatum(Dbref value, int? fileLineNumber = null, int? columnNumber = null, string? wordName = null, int? wordLineNumber = null, string? key = null)
        {
            Key = key;
            Value = value;
            Type = DatumType.DbRef;
            FileLineNumber = fileLineNumber;
            ColumnNumber = columnNumber;
            WordName = wordName;
            WordLineNumber = wordLineNumber;
        }

        public ForthDatum(string? value, int? fileLineNumber = null, int? columnNumber = null, string? wordName = null, int? wordLineNumber = null, string? key = null)
        {
            Key = key;
            Value = value;
            Type = DatumType.String;
            FileLineNumber = fileLineNumber;
            ColumnNumber = columnNumber;
            WordName = wordName;
            WordLineNumber = wordLineNumber;
        }

        public ForthDatum(int? value, int? fileLineNumber = null, int? columnNumber = null, string? wordName = null, int? wordLineNumber = null, string? key = null)
        {
            Key = key;
            Value = value;
            Type = DatumType.Integer;
            FileLineNumber = fileLineNumber;
            ColumnNumber = columnNumber;
            WordName = wordName;
            WordLineNumber = wordLineNumber;
        }

        public ForthDatum(float? value, int? fileLineNumber = null, int? columnNumber = null, string? wordName = null, int? wordLineNumber = null, string? key = null)
        {
            Key = key;
            Value = value;
            Type = DatumType.Float;
            FileLineNumber = fileLineNumber;
            ColumnNumber = columnNumber;
            WordName = wordName;
            WordLineNumber = wordLineNumber;
        }

        public ForthDatum(ForthDatum[] elements, int? fileLineNumber = null, int? columnNumber = null, string? wordName = null, int? wordLineNumber = null)
        {
            Key = null;
            Value = $"{elements.Select(e => $"{e.Key}\x7{e.SerializeString()}").Aggregate((c, n) => $"{c}\0{n}")}";
            Type = DatumType.Array;
            FileLineNumber = fileLineNumber;
            ColumnNumber = columnNumber;
            WordName = wordName;
            WordLineNumber = wordLineNumber;
        }

        public static bool TryInferType([NotNullWhen(true)] string? value, out Tuple<DatumType, object>? result)
        {
            if (value == null)
            {
                result = default;
                return false;
            }

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

            result = default;
            return false;
        }

        public bool IsFalse() => Type switch
        {
            DatumType.Integer => UnwrapInt() == 0,
            DatumType.Float => Value == null || (float)Value == 0,
            DatumType.DbRef => UnwrapDbref().ToInt32() == -1,
            DatumType.String => Value == null || string.IsNullOrEmpty((string)Value),
            _ => false,
        };

        public bool IsTrue() => !IsFalse();

        public string? SerializeString()
        {
            switch (Type)
            {
                case DatumType.DbRef:
                    return $"{UnwrapDbref()}";
                case DatumType.String:
                    return $"\"{Value as string ?? string.Empty}\"";
                case DatumType.Integer:
                    return $"{UnwrapInt()}";
                case DatumType.Float:
                    var f1 = $"{(float?)Value ?? 0F}";
                    if (!f1.Contains('.'))
                        return $"{f1}.0";
                    return f1;
                default:
                    throw new InvalidOperationException();
            }
        }

        public ForthDatum ToInteger() => Type switch
        {
            DatumType.Float => new ForthDatum(Value == null ? (int?)null : (int?)Convert.ToInt32((float)Value), DatumType.Integer),
            DatumType.Integer => this,
            DatumType.DbRef => new ForthDatum(UnwrapDbref().ToInt32()),
            _ => new ForthDatum(0),
        };

        public override string ToString() => $"({Enum.GetName(typeof(DatumType), Type)}){Value}";

        public Dbref UnwrapDbref()
        {
            if (Type != DatumType.DbRef)
                throw new InvalidCastException($"Cannot unwrap property as dbref, since it is of type: {Type}");

            if (Value == null)
                return NOT_FOUND;

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

        public ForthDatum[] UnwrapArray()
        {
            if (Type != DatumType.Array)
                throw new InvalidCastException($"Cannot unwrap property as array, since it is of type: {Type}");

            if (Value == null || Value as string == null)
                return Array.Empty<ForthDatum>();

            List<ForthDatum> ret = new();
            foreach (var part in ((string)Value).Split('\0'))
            {
                var kvps = part.Split('\x7');
                var key = kvps[0];
                var val = kvps[1];
                if (TryInferType(val, out Tuple<DatumType, object>? result))
                {
                    if (result!.Item1 == DatumType.String
                        && result.Item2 != null
                        && ((string)result.Item2).StartsWith('\"')
                        && ((string)result.Item2).EndsWith('\"'))
                        ret.Add(new ForthDatum(((string)result.Item2)[1..^1], DatumType.String, key: !string.IsNullOrWhiteSpace(key) ? key : null));
                    else
                        ret.Add(new ForthDatum(result!.Item2, result.Item1, key: !string.IsNullOrWhiteSpace(key) ? key : null));
                }
                else
                    throw new InvalidOperationException($"CANNOT PARSE? {part} as array element");
            }

            return ret.ToArray();
        }

        public int UnwrapInt()
        {
            if (Type != DatumType.Integer)
                throw new InvalidCastException($"Cannot unwrap property as int, since it is of type: {Type}");

            if (Value == null)
                return 0;

            if (Value.GetType() == typeof(int))
                return (int)Value;

            if (Value.GetType() == typeof(string))
            {
                if (int.TryParse((string)Value, out int i))
                    return i;

                throw new InvalidCastException($"Cannot unwrap property as int, unable to parse: {Value}");
            }

            throw new InvalidCastException($"Cannot unwrap property as dbref, underlying type is: {Value.GetType().Name}");
        }

        public override bool Equals(object? obj) => obj is ForthDatum datum &&
                   EqualityComparer<object?>.Default.Equals(Value, datum.Value) &&
                   Type == datum.Type &&
                   string.CompareOrdinal(WordName, datum.WordName) == 0;

        public override int GetHashCode() => HashCode.Combine(Value, Type, WordName);

        public static bool operator ==(ForthDatum left, ForthDatum right) => left.Equals(right);

        public static bool operator !=(ForthDatum left, ForthDatum right) => !(left == right);
    }
}