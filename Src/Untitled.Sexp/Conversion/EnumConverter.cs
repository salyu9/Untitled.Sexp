using System;
using System.Linq;
using Untitled.Sexp.Attributes;
using Untitled.Sexp.Formatting;
using Untitled.Sexp.Utilities;

namespace Untitled.Sexp.Conversion
{
    internal class EnumConverter : SexpConverter
    {
        private readonly Type _type;
        private readonly bool _asSymbol;
        private readonly bool _isFlag;

        private string[]? _names;
        private long[]? _values;

        public EnumConverter(Type type)
        {
            System.Diagnostics.Debug.Assert(type.IsEnum);
            _type = type;

            if (type.GetEnumUnderlyingType() == typeof(ulong))
            {
                throw new NotSupportedException($"Type {_type}: enum of ulong is not supported.");
            }

            foreach (var attribute in type.GetCustomAttributes(false))
            {
                if (attribute is FlagsAttribute)
                {
                    _isFlag = true;
                }
                else if (attribute is SexpSymbolEnumAttribute)
                {
                    _asSymbol = true;
                }
            }
            if (_asSymbol)
            {
                _names = Enum.GetNames(type);
                _values = Enum.GetValues(type).Cast<object>().Select(o => Convert.ToInt64(o)).ToArray();
            }
        }

        public override bool CanConvert(Type type)
            => type == _type;

        public override object? ToObjectExactType(SValue value)
        {
            try
            {
                if (_asSymbol)
                {
                    if (_isFlag)
                    {
                        if (value.IsNull) return Enum.ToObject(_type, 0);
                        if (!value.IsList) throw new SexpConvertException(_type, value);
                        long result = 0;
                        foreach (var s in value.AsEnumerable<Symbol>())
                        {
                            result |= Convert.ToInt64(Enum.Parse(_type, s.Name));
                        }
                        return Enum.ToObject(_type, result);
                    }
                    else
                    {
                        if (!value.IsSymbol) throw new SexpConvertException(_type, value);

                        return Enum.Parse(_type, value.AsSymbol().Name);
                    }
                }
                else
                {
                    if (!value.IsNumber) throw new SexpConvertException(_type, value);
                    return Enum.ToObject(_type, value.AsLong());
                }
            }
            catch (ArgumentException)
            {
                throw new SexpConvertException(_type, value);
            }
        }

        public override SValue ToValueExactType(object obj)
        {
            if (_asSymbol)
            {
                if (_isFlag)
                {
                    var current = SValue.Null;
                    // from mscorlib Enum.cs
                    var index = _values!.Length - 1;
                    var result = Convert.ToInt64(obj);
                    while (index >= 0)
                    {
                        if ((index == 0) && (_values[index] == 0))
                            break;

                        if ((result & _values[index]) == _values[index])
                        {
                            result -= _values[index];

                            current = new SValue(Symbol.FromString(_names![index]), current);
                        }

                        index--;
                    }

                    if (result != 0) throw new SexpConvertException($"Cannot represent {obj} as names of {_type}");

                    if (current.IsNull)
                    {
                        if (_values.Length > 0 && _values[0] == 0) return SValue.List(Symbol.FromString(_names![0]));
                        return SValue.Null;
                    }
                    return current;
                }
                else
                {
                    var name = Enum.GetName(_type, obj);
                    if (name == null) throw new SexpConvertException($"Cannot get value name of {_type} with value {obj}");
                    return Symbol.FromString(name);
                }
            }
            else
            {
                return Convert.ToInt64(obj);
            }
        }
    }
}
