using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Untitled.Sexp.Formatting;
using static Untitled.Sexp.Utils;

namespace Untitled.Sexp
{
    /// <summary>
    /// Represent an sexp value.
    /// </summary>
    public sealed partial class SValue : IEquatable<SValue>
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

        internal SValueFormatting? _formatting;

        internal int _hash;

        /// <summary>
        /// Formatting of the sexp value.
        /// </summary>
        /// <value></value>
        public SValueFormatting? Formatting
        {
            get => _formatting;
            set
            {
                if (value == null)
                {
                    _formatting = null;
                    return;
                }
                var formattingType = Type switch
                {
                    SValueType.Null => throw new SexpException("Cannot set null formatting"),
                    SValueType.Eof => throw new SexpException("Cannot set eof formatting."),
                    SValueType.Boolean => typeof(BooleanFormatting),
                    SValueType.Number => typeof(NumberFormatting),
                    SValueType.Char => typeof(CharacterFormatting),
                    SValueType.String => typeof(CharacterFormatting),
                    SValueType.Symbol => typeof(CharacterFormatting),
                    SValueType.Pair => typeof(ListFormatting),
                    SValueType.Bytes => typeof(BytesFormatting),
                    SValueType.TypeIdentifier => typeof(CharacterFormatting),
                    _ => throw new SexpException($"Invalid svalue type {Type}")
                };
                if (value.GetType() != formattingType) throw new SexpException($"Cannot set {value.GetType().Name} to SValue of type {Type}");
                _formatting = value;
            }
        }

        // stolen from array.cs, the times 33 hash with xor, that is djb2a
        private static int CombineHashCodes(int h1, int h2)
        {
            return (((h1 << 5) + h1) ^ h2);
        }

        internal SValue(object value, SValueType type, SValueFormatting? formatting = null)
        {
            _value = value;
            Type = type;
            Formatting = formatting;

            if (value is byte[] bytes) // calc hash for bytes, using last 8 elem
            {
                var ret = 0;
                for (int i = bytes.Length > 8 ? bytes.Length - 8 : 0; i < bytes.Length; ++i)
                {
                    ret = CombineHashCodes(ret, bytes[i]);
                }
                _hash = ret;
            }
            else
            {
                _hash = value.GetHashCode();
            }
        }

        /// <summary>
        /// Initialize an <see cref="SValue" /> with another <see cref="SValue" />.
        /// </summary>
        public SValue(SValue other)
            : this(other._value, other.Type, other.Formatting)
        { }

        /// <summary>
        /// Initialize an <see cref="SValue" /> with a bool value.
        /// </summary>
        public SValue(bool b, BooleanFormatting? formatting = null)
            : this(b, SValueType.Boolean, formatting)
        { }

        /// <summary>
        /// Initialize an <see cref="SValue" /> with an int value.
        /// </summary>
        public SValue(int n, NumberFormatting? formatting = null)
            : this((long)n, SValueType.Number, formatting)
        { }

        /// <summary>
        /// Initialize an <see cref="SValue" /> with an long value.
        /// </summary>
        public SValue(long n, NumberFormatting? formatting = null)
            : this(n, SValueType.Number, formatting)
        { }

        /// <summary>
        /// Initialize an <see cref="SValue" /> with a double value.
        /// </summary>
        public SValue(double d, NumberFormatting? formatting = null)
            : this(d, SValueType.Number, formatting)
        { }

        /// <summary>
        /// Initialize an <see cref="SValue" /> with a char.
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
        /// Initialize an <see cref="SValue" /> with a string.
        /// </summary>
        public SValue(string s, CharacterFormatting? formatting = null)
            : this(s, SValueType.String, formatting)
        { }

        /// <summary>
        /// Initialize an <see cref="SValue" /> with a byte array.
        /// </summary>
        public SValue(IEnumerable<byte> bytes, BytesFormatting? formatting = null)
            : this(bytes.ToArray(), SValueType.Bytes, formatting)
        { }

        /// <summary>
        /// Initialize an <see cref="SValue" /> with a symbol.
        /// </summary>
        public SValue(Symbol symbol, CharacterFormatting? formatting = null)
            : this(symbol, SValueType.Symbol, formatting)
        { }

        /// <summary>
        /// Create an <see cref="SValue" /> symbol with a string.
        /// </summary>
        public static SValue Symbol(string s, CharacterFormatting? formatting = null)
            => new SValue(Untitled.Sexp.Symbol.FromString(s), formatting);

        /// <summary>
        /// Initialize an <see cref="SValue" /> pair.
        /// </summary>
        public SValue(Pair pair, ListFormatting? formatting = null)
            : this(pair, SValueType.Pair, formatting)
        { }

        /// <summary>
        /// Initialize an <see cref="SValue" /> pair with car and cdr.
        /// </summary>
        public SValue(SValue car, SValue cdr, ListFormatting? formatting = null)
            : this(new Pair(car, cdr), formatting)
        { }

        /// <summary>
        /// Initialize an <see cref="SValue" /> with type identifier.
        /// </summary>
        public SValue(TypeIdentifier identifier, CharacterFormatting? formatting = null)
            : this(identifier, SValueType.TypeIdentifier, formatting)
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
        public bool IsTypeIdentifier
            => Type == SValueType.TypeIdentifier;

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
                    var pair = (Pair)current._value;
                    current = pair._cdr;
                }
                return true;
            }
        }

        /// <summary>
        /// Compare two sexp values.
        /// </summary>
        public bool Equals(SValue other)
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
                var p1 = (Pair)_value;
                var p2 = (Pair)other._value;
                return p1.Car.Equals(p2.Car) && p1.Cdr.Equals(p2.Cdr);
            }

            return _value.Equals(other._value);
        }

        /// <summary />
        public override bool Equals(object obj)
            => obj is SValue value && Equals(value);

        /// <summary>
        /// Compare two sexp values.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool Equals(SValue? a, SValue? b)
        {
            if (a == null && b == null) return true;
            if (a != null && b != null) return a.Equals(b);
            return false;
        }

        /// <summary />
        public override int GetHashCode()
            => _hash;

        /// <summary />
        public override string ToString()
        {
            using var writer = new StringWriter();
            new SexpTextWriter(writer).Write(this);
            return writer.ToString();
        }

    }
}
