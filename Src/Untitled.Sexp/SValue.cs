using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Untitled.Sexp.Formatting;
using static Untitled.Sexp.Utils;

namespace Untitled.Sexp
{
    /// <summary>
    /// Represent an sexp value.
    /// </summary>
    public sealed class SValue
    {
        /// <summary>
        /// The Null value.
        /// </summary>
        public static readonly SValue Null = new SValue(new object(), SValueType.Null);

        /// <summary>
        /// The eof object.
        /// </summary>
        public static readonly SValue Eof = new SValue(new object(), SValueType.Eof);

        /// <summary>
        /// The #t object.
        /// </summary>
        public static readonly SValue True = new SValue(true, SValueType.Boolean);

        /// <summary>
        /// The #f object.
        /// </summary>
        public static readonly SValue False = new SValue(false, SValueType.Boolean);

        /// <summary>
        /// Get the type of value.
        /// </summary>
        public SValueType Type { get; }

        internal object _value;

        /// <summary>
        /// Formatting of the sexp value.
        /// </summary>
        /// <value></value>
        public SValueFormatting? Formatting { get; internal set; }

        internal SValue(object value, SValueType type, SValueFormatting? formatting = null)
        {
            _value = value;
            Type = type;
            Formatting = formatting;
        }

        /// <summary>
        /// Instantiate an <see cref="SValue" /> with another <see cref="SValue" />.
        /// </summary>
        public SValue(SValue other)
            : this(other._value, other.Type, other.Formatting)
        { }

        /// <summary>
        /// Instantiate an <see cref="SValue" /> with a bool value.
        /// </summary>
        public SValue(bool b, BooleanFormatting? formatting = null)
            : this(b, SValueType.Boolean, formatting)
        { }

        /// <summary>
        /// Instantiate an <see cref="SValue" /> with an int value.
        /// </summary>
        public SValue(int n, NumberFormatting? formatting = null)
            : this(n, SValueType.Number, formatting)
        { }

        /// <summary>
        /// Instantiate an <see cref="SValue" /> with an long value.
        /// </summary>
        public SValue(long n, NumberFormatting? formatting = null)
            : this(n, SValueType.Number, formatting)
        { }

        /// <summary>
        /// Instantiate an <see cref="SValue" /> with a double value.
        /// </summary>
        public SValue(double d, NumberFormatting? formatting = null)
            : this(d, SValueType.Number, formatting)
        { }

        /// <summary>
        /// Instantiate an <see cref="SValue" /> with a char.
        /// </summary>
        public SValue(char c, CharacterFormatting? formatting = null)
            : this((int)c, SValueType.Char, formatting)
        {
            if (char.IsSurrogate(c))
            {
                throw new ArgumentException($"Incomplete character: surrogate \\u{((int)c).ToHex()}", nameof(c));
            }
        }

        /// <summary>
        /// Create an <see cref="SValue" /> with a Unicode scalar value.
        /// </summary>
        public static SValue Char(int n, CharacterFormatting? formatting = null)
        {
            if (n < 0x10000 && char.IsSurrogate((char)n))
            {
                throw new ArgumentException($"Incomplete character: surrogate \\u{n.ToHex()}", nameof(n));
            }
            if (n > 0x10FFFF)
            {
                throw new ArgumentException($"Unicode scalar value out of range: \\u{n.ToHex()}", nameof(n));
            }
            return new SValue(n, SValueType.Char, formatting);
        }

        /// <summary>
        /// Instantiate an <see cref="SValue" /> with a string.
        /// </summary>
        public SValue(string s, CharacterFormatting? formatting = null)
            : this(s, SValueType.String, formatting)
        { }

        /// <summary>
        /// Instantiate an <see cref="SValue" /> with a byte array.
        /// </summary>
        public SValue(IEnumerable<byte> bytes, BytesFormatting? formatting = null)
            : this(bytes.ToArray(), SValueType.Bytes, formatting)
        { }

        /// <summary>
        /// Instantiate an <see cref="SValue" /> with a symbol.
        /// </summary>
        public SValue(SSymbol symbol, CharacterFormatting? formatting = null)
            : this(symbol, SValueType.Symbol, formatting)
        { }

        /// <summary>
        /// Create an <see cref="SValue" /> symbol with a string.
        /// </summary>
        public static SValue Symbol(string s, CharacterFormatting? formatting = null)
            => new SValue(SSymbol.FromString(s), formatting);

        /// <summary>
        /// Instantiate an <see cref="SValue" /> pair.
        /// </summary>
        public SValue(SPair pair, ListFormatting? formatting = null)
            : this(pair, SValueType.Pair, formatting)
        { }

        /// <summary>
        /// Instantiate an <see cref="SValue" /> pair with car and cdr.
        /// </summary>
        public SValue(SValue car, SValue cdr, ListFormatting? formatting = null)
            : this(new SPair(car, cdr), formatting)
        { }

        /// <summary>
        /// Cons two sexp values to generate a pair.
        /// </summary>
        /// <param name="car">The car field of the pair.</param>
        /// <param name="cdr">The cdr field of the pair.</param>
        /// <param name="formatting"></param>
        /// <returns></returns>
        public static SValue Cons(SValue car, SValue cdr, ListFormatting? formatting = null)
            => new SValue(car, cdr, formatting);

        /// <summary>
        /// Create an list with given values.
        /// </summary>
        /// <param name="values">Values that will be in the list.</param>
        public static SValue List(params SValue[] values)
            => List((IEnumerable<SValue>)values);

        /// <summary>
        /// Create an list with formatting and given values.
        /// </summary>
        /// <param name="formatting"></param>
        /// <param name="values">Values that will be in the list.</param>
        public static SValue List(ListFormatting formatting, params SValue[] values)
            => List((IEnumerable<SValue>)values, formatting);

        /// <summary>
        /// Create an list with given values.
        /// </summary>
        /// <param name="values">Values that will be in the list.</param>
        /// <param name="formatting"></param>
        public static SValue List(IEnumerable<SValue> values, ListFormatting? formatting = null)
            => new Utilities.ListBuilder(values, formatting).ToValue();

        /// <summary />
        public bool IsNull
            => Type == SValueType.Null;

        /// <summary />
        public bool IsEof
            => Type == SValueType.Eof;

        /// <summary />
        public bool IsBoolean
            => Type == SValueType.Boolean;

        /// <summary />
        public bool IsNumber
            => Type == SValueType.Number;

        /// <summary />
        public bool IsString
            => Type == SValueType.String;

        /// <summary />
        public bool IsSymbol
            => Type == SValueType.Symbol;

        /// <summary />
        public bool IsChar
            => Type == SValueType.Char;

        /// <summary />
        public bool IsBytes
            => Type == SValueType.Bytes;

        /// <summary />
        public bool IsPair
            => Type == SValueType.Pair;

        /// <summary />
        public bool IsList
        {
            get
            {
                var current = this;
                while (!current.IsNull)
                {
                    if (!current.IsPair) return false;
                    var pair = (SPair)current._value;
                    current = pair._cdr;
                }
                return true;
            }
        }

        /// <summary />
        public int CompareTo(SValue other)
        {
            if (ReferenceEquals(this, other)) return 0;

            var t = Type.CompareTo(other.Type);
            if (t != 0) return t;

            if (IsBytes)
            {
                var b1 = (byte[])_value;
                var b2 = (byte[])other._value;
                foreach (var i in Range(Math.Min(b1.Length, b2.Length)))
                {
                    var b = b1[i].CompareTo(b2[i]);
                    if (b != 0) return b;
                }
                return b1.Length.CompareTo(b2.Length);
            }

            return ((IComparable)_value).CompareTo((IComparable)other._value);
        }

        /// <summary>
        /// Compare two sexp values.
        /// </summary>
        public bool DeepEquals(SValue other)
        {
            if (ReferenceEquals(this, other)) return true;

            if (Type != other.Type) return false;

            if (IsBytes)
            {
                var b1 = (byte[])_value;
                var b2 = (byte[])other._value;
                return b1.SequenceEqual(b2);
            }

            if (IsPair)
            {
                var p1 = (SPair)_value;
                var p2 = (SPair)other._value;
                return p1.Car.DeepEquals(p2.Car) && p1.Cdr.DeepEquals(p2.Cdr);
            }

            return _value.Equals(other._value);
        }

        /// <summary>
        /// Compare two sexp values.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool DeepEquals(SValue? a, SValue? b)
        {
            if (a == null && b == null) return true;
            if (a != null && b != null) return a.DeepEquals(b);
            return false;
        }

        /// <summary />
        public override string ToString()
        {
            using var writer = new StringWriter();
            new SexpTextWriter(writer).Write(this);
            return writer.ToString();

            // if (IsNull) return "<SValue type = Null>";
            // if (IsBytes) return $"<SValue type = Bytes, value = {string.Join(", ", ((byte[])_value).Select(b => b.ToString("X02")))}";
            // return $"<SValue type = {Type}, value = {_value}>";
        }

        #region Casts

        private static Dictionary<Type, Func<SValue, object>> CasterTable
            = new Dictionary<Type, Func<SValue, object>>
            {
                [typeof(bool)] = v => v.AsBoolean(),
                [typeof(int)] = v => v.AsInt(),
                [typeof(long)] = v => v.AsLong(),
                [typeof(double)] = v => v.AsDouble(),
                [typeof(char)] = v => v.AsChar(),
                [typeof(SSymbol)] = v => v.AsSymbol(),
                [typeof(string)] = v => v.AsString(),
                [typeof(SPair)] = v => v.AsPair(),
                [typeof(byte[])] = v => v.AsBytes().ToArray(),
            };

        /// <summary>
        /// Cast sexp value to type T.
        /// </summary>
        /// <typeparam name="T">Target type.</typeparam>
        public T Cast<T>()
        {
            if (!CasterTable.TryGetValue(typeof(T), out var caster))
            {
                throw new InvalidCastException($"Cannot cast sexp value to {typeof(T)}");
            }
            return (T)caster(this);
        }

#nullable disable // quirk nullable generics

        /// <summary>
        /// Try to cast sexp value to type T.
        /// </summary>
        /// <param name="result">Out parameter that will receive result.</param>
        /// <typeparam name="T">Target type.</typeparam>
        /// <returns>True if successfully casted, otherwise false.</returns>
        public bool TryCast<T>(out T result)
        {
            try
            {
                result = Cast<T>();
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }
#nullable enable

        /// <summary>
        /// Cast to bool.
        /// </summary>
        public bool AsBoolean()
        {
            if (!IsBoolean) throw new InvalidCastException($"Cannot cast {Type} to bool");

            return (bool)_value;
        }

        /// <summary>
        /// Cast to int.
        /// </summary>
        public int AsInt()
        {
            if (!IsNumber) throw new InvalidCastException($"Cannot cast {Type} to int");

            return _value switch
            {
                int i => i,
                long l => (int)l,
                double d => (int)d,
                _ => throw new SexpException("Unknown underlying type of sexp number")
            };
        }

        /// <summary>
        /// Cast to long.
        /// </summary>
        public long AsLong()
        {
            if (!IsNumber) throw new InvalidCastException($"Cannot cast {Type} to int");

            return _value switch
            {
                long l => l,
                int i => i,
                double d => (long)d,
                _ => throw new SexpException("Unknown underlying type of sexp number")
            };
        }

        /// <summary>
        /// Cast to double.
        /// </summary>
        public double AsDouble()
        {
            if (!IsNumber) throw new InvalidCastException($"Cannot cast {Type} to int");

            return _value switch
            {
                double d => (long)d,
                int i => i,
                long l => l,
                _ => throw new SexpException("Unknown underlying type of sexp number")
            };
        }

        /// <summary>
        /// Cast to char.
        /// </summary>
        public char AsChar()
        {
            if (!IsChar) throw new InvalidCastException($"Cannot cast {Type} to char");

            int scalar = (int)_value;
            if (scalar > char.MaxValue) throw new OverflowException($"Scalar value {scalar} is larger than the limit of char, consider use {nameof(AsUnicodeScalarValue)}()");
            return (char)scalar;
        }

        /// <summary>
        /// Cast to unicode scalar value (may exceed char type limit).
        /// </summary>
        public int AsUnicodeScalarValue()
        {
            if (!IsChar) throw new InvalidCastException($"Cannot cast {Type} to char");

            return (int)_value;
        }

        /// <summary>
        /// Cast char to a single character string. Useful when dealing with characters larger than 0xFFFF.
        /// </summary>
        public string CharToString()
        {
            if (!IsChar) throw new InvalidCastException($"Cannot cast {Type} to char");

            var value = (int)_value;
            if (value <= 0xFFFF) return new string((char)value, 1);
            
            var buffer = new char[2];
            DispartSurrogateToBuffer(value, buffer);
            return new string(buffer);
        }

        /// <summary>
        /// Cast to symbol.
        /// </summary>
        public SSymbol AsSymbol()
        {
            if (!IsSymbol) throw new InvalidCastException($"Cannot cast {Type} to symbol");

            return (SSymbol)_value;
        }

        /// <summary>
        /// Cast to string.
        /// </summary>
        public string AsString()
        {
            if (!IsString) throw new InvalidCastException($"Cannot cast {Type} to string");

            return (string)_value;
        }

        /// <summary>
        /// Cast to bytes. The result is an immutable reference to underlying byte array.
        /// </summary>
        /// <returns>An immutable reference to underlying byte array.</returns>
        public ReadOnlyCollection<byte> AsBytes()
        {
            if (!IsBytes) throw new InvalidCastException($"Cannot cast {Type} to bytes");

            return Array.AsReadOnly((byte[])_value);
        }

        /// <summary>
        /// Cast to pair. 
        /// </summary>
        public SPair AsPair()
        {
            if (!IsPair) throw new InvalidCastException($"Cannot cast {Type} to pair");

            return (SPair)_value;
        }

        /// <summary>
        /// Get a enumrable reference to the list. Throws if the value is not list.
        /// </summary>
        public IEnumerable<SValue> AsEnumerable()
        {
            if (!IsList) throw new InvalidCastException($"The sexp value is not a list");

            var current = this;
            while (!current.IsNull)
            {
                var pair = (SPair)current._value;
                yield return pair._car;
                current = pair._cdr;
            }
        }

        /// <summary>
        /// Get a enumrable reference to the list with type casting.
        /// </summary>
        /// <typeparam name="T">Type of list elements.</typeparam>
        public IEnumerable<T> AsEnumerable<T>()
        {
            if (!CasterTable.TryGetValue(typeof(T), out var caster))
            {
                throw new InvalidCastException($"Cannot cast sexp value to {typeof(T)}");
            }
            if (!IsList) throw new InvalidCastException($"The sexp value is not a list");

            var current = this;
            while (!current.IsNull)
            {
                var pair = (SPair)current._value;
                yield return (T)caster(pair._car);
                current = pair._cdr;
            }
        }

        /// <summary>
        /// Cast to list of sexp values.
        /// </summary>
        public List<SValue> ToList()
            => AsEnumerable().ToList();

        /// <summary>
        /// Cast to list of target type values.
        /// </summary>
        public List<T> ToList<T>()
            => AsEnumerable<T>().ToList();

        /// <summary>
        /// Implicit cast bool to sexp value.
        /// </summary>
        public static implicit operator SValue(bool b) => b ? True : False;

        /// <summary>
        /// Implicit cast int to sexp value.
        /// </summary>
        public static implicit operator SValue(int i) => new SValue(i);

        /// <summary>
        /// Implicit cast long to sexp value.
        /// </summary>
        public static implicit operator SValue(long l) => new SValue(l);

        /// <summary>
        /// Implicit cast double to sexp value.
        /// </summary>
        public static implicit operator SValue(double d) => new SValue(d);

        /// <summary>
        /// Implicit cast char to sexp value.
        /// </summary>
        public static implicit operator SValue(char c) => new SValue(c);

        /// <summary>
        /// Implicit cast symbol to sexp value.
        /// </summary>
        public static implicit operator SValue(SSymbol s) => new SValue(s);

        /// <summary>
        /// Implicit cast string to sexp value.
        /// </summary>
        public static implicit operator SValue(string s) => new SValue(s);

        /// <summary>
        /// Implicit cast pair to sexp value.
        /// </summary>
        public static implicit operator SValue(SPair p) => new SValue(p);

        /// <summary>
        /// Explicit cast sexp value to bool.
        /// </summary>
        public static explicit operator bool(SValue v) => v.AsBoolean();

        /// <summary>
        /// Explicit cast sexp value to int.
        /// </summary>
        public static explicit operator int(SValue v) => v.AsInt();

        /// <summary>
        /// Explicit cast sexp value to long.
        /// </summary>
        public static explicit operator long(SValue v) => v.AsLong();

        /// <summary>
        /// Explicit cast sexp value to double.
        /// </summary>
        public static explicit operator double(SValue v) => v.AsDouble();

        /// <summary>
        /// Explicit cast sexp value to char.
        /// </summary>
        public static explicit operator char(SValue v) => v.AsChar();

        /// <summary>
        /// Explicit cast sexp value to symbol.
        /// </summary>
        public static explicit operator SSymbol(SValue v) => v.AsSymbol();

        /// <summary>
        /// Explicit cast sexp value to string.
        /// </summary>
        public static explicit operator string(SValue v) => v.AsString();

        /// <summary>
        /// Explicit cast sexp value to pair.
        /// </summary>
        public static explicit operator SPair(SValue v) => v.AsPair();

        /// <summary>
        /// Explicit cast sexp value to byte[]. The result is a copy of underlying byte array.
        /// </summary>
        public static explicit operator byte[](SValue v) => v.AsBytes().ToArray();

        #endregion
    }
}
