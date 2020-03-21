using System;
using System.Collections.Generic;

namespace Untitled.Sexp.Conversion
{
    /// <summary>
    /// Base type for sexp value converters.
    /// </summary>
    public abstract class SexpConverter
    {
        /// <summary>
        /// Convert sexp value to object to exact the object's type.
        /// </summary>
        /// <param name="value">The sexp value to convert.</param>
        /// <returns>The converted object.</returns>
        public abstract object? ToObjectExactType(SValue value);

        /// <summary>
        /// Convert sexp value to object with type checked.
        /// </summary>
        /// <param name="value">The sexp value to convert.</param>
        /// <returns>The converted object.</returns>
        public virtual object? ToObject(SValue value)
        {
            if (value.IsPair)
            {
                var pair = value.AsPair();
                if (pair.Car.IsTypeIdentifier)
                {
                    var type = TypeResolver.Resolve(pair.Car.AsTypeIdentifier());
                    return SexpConvert.ToObject(type, pair.Cdr);
                }
            }
            return ToObjectExactType(value);
        }

        /// <summary>
        /// Convert object to sexp value from exact the object's type.
        /// </summary>
        /// <param name="obj">The object to convert.</param>
        /// <returns>The converted sexp value.</returns>
        public abstract SValue ToValueExactType(object obj);

        /// <summary>
        /// Convert object to sexp value with type checked.
        /// </summary>
        /// <param name="type">The type to convert.</param>
        /// <param name="obj">The object to convert.</param>
        /// <returns>The converted sexp value.</returns>
        public virtual SValue ToValue(Type type, object? obj)
        {
            if (obj == null) return SValue.Null;

            Console.WriteLine($"Convert {obj.GetType()} to {type}");

            var objType = obj.GetType();

            if (type != objType)
            {
                if (!type.IsInstanceOfType(obj)) throw new SexpConvertException($"Cannot convert {obj} to {type}");

                var value = SexpConvert.ToValue(objType, obj);
                var typeid = TypeResolver.GetTypeId(objType);

                return SValue.Cons(typeid, value);
            }

            return ToValueExactType(obj);
        }

        /// <summary>
        /// Check if this converter can convert specified type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>True if this converter can convert the type, otherwise false.</returns>
        public abstract bool CanConvert(Type type);

        /// <summary>
        /// Type resolver for the type.
        /// </summary>
        public TypeResolver TypeResolver { get; set; } = TypeResolver.Default;
    }
}
