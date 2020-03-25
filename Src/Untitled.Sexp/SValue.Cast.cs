using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static Untitled.Sexp.Utils;

namespace Untitled.Sexp
{
    /// <summary>
    /// Represent an sexp value.
    /// </summary>
    public sealed partial class SValue : IEquatable<SValue>
    {
        private static Dictionary<Type, Func<SValue, object>> CasterTable
            = new Dictionary<Type, Func<SValue, object>>
            {
                [typeof(bool)] = v => v.AsBoolean(),
                [typeof(int)] = v => v.AsInt(),
                [typeof(long)] = v => v.AsLong(),
                [typeof(double)] = v => v.AsDouble(),
                [typeof(char)] = v => v.AsChar(),
                [typeof(Symbol)] = v => v.AsSymbol(),
                [typeof(string)] = v => v.AsString(),
                [typeof(Pair)] = v => v.AsPair(),
                [typeof(byte[])] = v => v.AsBytes().ToArray(),
                [typeof(TypeIdentifier)] = v => v.AsTypeIdentifier(),
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
            if (!IsNumber) throw new InvalidCastException($"Cannot cast {Type} to long");

            return _value switch
            {
                long l => l,
                double d => (long)d,
                _ => throw new SexpException("Unknown underlying type of sexp number")
            };
        }

        /// <summary>
        /// Cast to double.
        /// </summary>
        public double AsDouble()
        {
            if (!IsNumber) throw new InvalidCastException($"Cannot cast {Type} to double");

            return _value switch
            {
                double d => d,
                long l => (double)l,
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
        public Symbol AsSymbol()
        {
            if (!IsSymbol) throw new InvalidCastException($"Cannot cast {Type} to symbol");

            return (Symbol)_value;
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
        public Pair AsPair()
        {
            if (!IsPair) throw new InvalidCastException($"Cannot cast {Type} to pair");

            return (Pair)_value;
        }

        /// <summary>
        /// Cast to <see cref="TypeIdentifier"/>
        /// </summary>
        public TypeIdentifier AsTypeIdentifier()
        {
            if (!IsTypeIdentifier) throw new InvalidCastException($"Cannot cast {Type} to type-identifier");

            return (TypeIdentifier)_value;
        }

        /// <summary>
        /// Get a enumrable reference to the list. Throws if the value is not list.
        /// </summary>
        public IEnumerable<SValue> AsEnumerable()
        {
            if (!IsList) throw new InvalidCastException($"The sexp value ({this}) is not a list");

            var current = this;
            while (!current.IsNull)
            {
                var pair = (Pair)current._value;
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
                var pair = (Pair)current._value;
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
        /// Cast to array of sexp values.
        /// </summary>
        public SValue[] ToArray()
            => AsEnumerable().ToArray();

        /// <summary>
        /// Cast to array of sexp values.
        /// </summary>
        public T[] ToArray<T>()
            => AsEnumerable<T>().ToArray();

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
        public static implicit operator SValue(Symbol s) => new SValue(s);

        /// <summary>
        /// Implicit cast string to sexp value.
        /// </summary>
        public static implicit operator SValue(string s) => new SValue(s);

        /// <summary>
        /// Implicit cast pair to sexp value.
        /// </summary>
        public static implicit operator SValue(Pair p) => new SValue(p);

        /// <summary>
        /// Implicit cast <see cref="TypeIdentifier" /> to sexp value.
        /// </summary>
        public static implicit operator SValue(TypeIdentifier id) => new SValue(id);

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
        public static explicit operator Symbol(SValue v) => v.AsSymbol();

        /// <summary>
        /// Explicit cast sexp value to string.
        /// </summary>
        public static explicit operator string(SValue v) => v.AsString();

        /// <summary>
        /// Explicit cast sexp value to pair.
        /// </summary>
        public static explicit operator Pair(SValue v) => v.AsPair();

        /// <summary>
        /// Explicit cast sexp value to byte[]. The result is a copy of underlying byte array.
        /// </summary>
        public static explicit operator byte[](SValue v) => v.AsBytes().ToArray();

        /// <summary>
        /// Explicit cast sexp value to <see cref="TypeIdentifier" />.
        /// </summary>
        public static explicit operator TypeIdentifier(SValue v) => v.AsTypeIdentifier();
    }
}
