using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        internal static readonly Regex DBREF_REGEX = new(@"#(\-?\d+|\d+[A-Z]?)", RegexOptions.Compiled);

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
                Property.PropertyType.Array => DatumType.Array,
                Property.PropertyType.Unknown => DatumType.Unknown,
                _ => throw new ArgumentException($"Unhandled variable type: {property.Type}", nameof(property)),
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
                ForthVariable.VariableType.Lock => DatumType.Lock,
                ForthVariable.VariableType.Array => DatumType.Array,
                ForthVariable.VariableType.Unknown => DatumType.Unknown,
                _ => throw new ArgumentException($"Unhandled variable type: {variable.Type}", nameof(variable)),
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

        public ForthDatum(Lock lck, int? fileLineNumber = null, int? columnNumber = null, string? wordName = null, int? wordLineNumber = null)
        {
            Key = null;
            Value = lck;
            Type = DatumType.Lock;
            FileLineNumber = fileLineNumber;
            ColumnNumber = columnNumber;
            WordName = wordName;
            WordLineNumber = wordLineNumber;
        }

        public ForthDatum(ForthDictionaryArray dict, int? fileLineNumber = null, int? columnNumber = null, string? wordName = null, int? wordLineNumber = null)
        {
            Key = null;
            Value = dict;
            Type = DatumType.Array;
            FileLineNumber = fileLineNumber;
            ColumnNumber = columnNumber;
            WordName = wordName;
            WordLineNumber = wordLineNumber;
        }

        public ForthDatum(ForthListArray list, int? fileLineNumber = null, int? columnNumber = null, string? wordName = null, int? wordLineNumber = null)
        {
            Key = null;
            Value = list;
            Type = DatumType.Array;
            FileLineNumber = fileLineNumber;
            ColumnNumber = columnNumber;
            WordName = wordName;
            WordLineNumber = wordLineNumber;
        }

        public static bool TryInferType([NotNullWhen(true)] string? value, [NotNullWhen(true)] out Tuple<DatumType, object>? result)
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

            if (value.Length > 1 && value[0] == '#' && DBREF_REGEX.IsMatch(value))
            {
                result = new Tuple<DatumType, object>(DatumType.DbRef, new Dbref(value));
                return true;
            }

            if (value.StartsWith('\"') && value.EndsWith('\"'))
            {
                result = new Tuple<DatumType, object>(DatumType.String, value[1..^1]);
                return true;
            }

            if (value.StartsWith("lok:"))
            {
                result = new Tuple<DatumType, object>(DatumType.Lock, value[4..]);
                return true;
            }

            if (value.StartsWith('[') && value.EndsWith(']') 
                && ForthListArray.TryParse(value, out var forthArray))
            {
                result = new Tuple<DatumType, object>(DatumType.Array, forthArray);
                return true;
            }

            result = default;
            return false;
        }

        public static bool TryConvert([NotNullWhen(true)] string? value, [NotNullWhen(true)] out ForthDatum? result)
        {
            if (!TryInferType(value, out var resultTypeInference))
            {
                result = default;                
                return false;
            }

            result = resultTypeInference.Item1 switch
            {
                DatumType.Array => (ForthDatum?)new ForthDatum((ForthListArray)resultTypeInference.Item2),
                DatumType.DbRef => (ForthDatum?)new ForthDatum((Dbref)resultTypeInference.Item2),
                DatumType.Float => (ForthDatum?)new ForthDatum((float)resultTypeInference.Item2),
                DatumType.Lock => (ForthDatum?)new ForthDatum((Lock)resultTypeInference.Item2),
                DatumType.Integer => (ForthDatum?)new ForthDatum((int)resultTypeInference.Item2),
                DatumType.String => (ForthDatum?)new ForthDatum((string)resultTypeInference.Item2),
                _ => throw new InvalidCastException("Unhandled type conversion for {resultTypeInference.Item1}"),
            };

            return true;
        }

        public readonly bool IsFalse() => Type switch
        {
            DatumType.Integer => UnwrapInt() == 0,
            DatumType.Float => Value == null || (float)Value == 0,
            DatumType.DbRef => UnwrapDbref().ToInt32() == -1,
            DatumType.String => Value == null || string.IsNullOrEmpty((string)Value),
            _ => false,
        };

        public readonly bool IsTrue() => !IsFalse();

        public readonly string? SerializeString()
        {
            switch (Type)
            {
                case DatumType.DbRef:
                    // DBREFs will have a "#" prefix per Dbref.ToString()
                    return $"{UnwrapDbref()}";
                case DatumType.String:
                    // STRING types are quoted.
                    return $"\"{Value as string ?? string.Empty}\"";
                case DatumType.Integer:
                    // Integers start with a numeral and have no decimal.
                    return $"{UnwrapInt()}";
                case DatumType.Float:
                    // Floats start with a numeral and have a decimal.
                    var f1 = $"{(float?)Value ?? 0F}";
                    if (!f1.Contains('.'))
                        return $"{f1}.0";
                    return f1;
                case DatumType.Lock:
                    return $"lok:{Value as string ?? string.Empty}";
                case DatumType.Array:
                    if (Value == null)
                        return "[]";
                    if (Value is ForthDictionaryArray da)
                        return da.ToString();
                    if (Value is ForthListArray la)
                        return la.ToString();

                    throw new InvalidOperationException($"Unable to serialize array with unknwon type {Value.GetType().Name}");
                default:
                    throw new InvalidOperationException();
            }
        }

        public readonly ForthDatum ToInteger() => Type switch
        {
            DatumType.Float => new ForthDatum(Value == null ? null : (int?)Convert.ToInt32((float)Value), DatumType.Integer),
            DatumType.Integer => this,
            DatumType.DbRef => new ForthDatum(UnwrapDbref().ToInt32()),
            _ => new ForthDatum(0),
        };

        public override readonly string ToString() => $"({Enum.GetName(Type)}){Value}";

        public readonly Dbref UnwrapDbref()
        {
            if (Type != DatumType.DbRef)
                throw new InvalidCastException($"Cannot unwrap property as dbref, since it is of type: {Type}");

            if (Value == null)
                return NOT_FOUND;

            if (Value.GetType() == typeof(Dbref))
            {
                return (Dbref)Value;
            }

            if (Value.GetType() == typeof(string) && Dbref.TryParse((string)Value, out Dbref dbref))
            {
                return dbref;
            }

            if (Value.GetType() == typeof(int))
            {
                return new Dbref((int)Value, DbrefObjectType.Thing);
            }

            throw new InvalidCastException($"Cannot unwrap property as dbref, underlying type is: {Value.GetType().Name}");
        }

        public readonly ForthListArray UnwrapListArray()
        {
            if (Type != DatumType.Array)
                throw new InvalidCastException($"Cannot unwrap property as a list array, since it is of type: {Type}");

            if (Value is ForthListArray la)
                return la;

            if (ForthListArray.TryParse(Value as string, out var la2))
                return la2.Value;

            throw new InvalidCastException($"Cannot unwrap property as a list array, underlying value: {Value}");
        }

        public readonly ForthDictionaryArray UnwrapDictionaryArray()
        {
            if (Type != DatumType.Array)
                throw new InvalidCastException($"Cannot unwrap property as a dictionary array, since it is of type: {Type}");

            if (Value is ForthDictionaryArray da)
                return da;

            if (ForthDictionaryArray.TryParse(Value as string, out var da2))
                return da2.Value;

            throw new InvalidCastException($"Cannot unwrap property as a dictionary array, underlying value: {Value}");
        }

        public readonly int UnwrapInt()
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

        public override readonly bool Equals(object? obj) => obj is ForthDatum datum &&
                   EqualityComparer<object?>.Default.Equals(Value, datum.Value) &&
                   Type == datum.Type &&
                   string.CompareOrdinal(WordName, datum.WordName) == 0;

        public override readonly int GetHashCode() => HashCode.Combine(Value, Type, WordName);

        public static bool operator ==(ForthDatum left, ForthDatum right) => left.Equals(right);

        public static bool operator !=(ForthDatum left, ForthDatum right) => !(left == right);
    }
}