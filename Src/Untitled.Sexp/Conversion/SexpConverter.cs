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
        public abstract object? ToObject(SValue value);

        /// <summary>
        /// Convert sexp value to object with type checked.
        /// </summary>
        /// <param name="value">The sexp value to convert.</param>
        /// <returns>The converted object.</returns>
        public virtual object? ToObjectWithTypeCheck(SValue value)
        {
            if (TypeResolver.GeneralTypeIdentifier)
            {
                var pair = value.AsPair();
                var type = TypeResolver.Resolve(pair.Car);
                return SexpConvert.GetConverter(type).ToObject(pair.Cdr);
            }
            if (value.IsPair)
            {
                var pair = value.AsPair();
                if (pair.Car.IsTypeIdentifier)
                {
                    var type = TypeResolver.Resolve(pair.Car);
                    return SexpConvert.GetConverter(type).ToObject(pair.Cdr);
                }
            }
            return ToObject(value);
        }

        /// <summary>
        /// Convert object to sexp value from exact the object's type.
        /// </summary>
        /// <param name="obj">The object to convert.</param>
        /// <returns>The converted sexp value.</returns>
        public abstract SValue ToValue(object obj);

        /// <summary>
        /// Convert object to sexp value with type checked.
        /// </summary>
        /// <param name="type">The type to convert.</param>
        /// <param name="obj">The object to convert.</param>
        /// <returns>The converted sexp value.</returns>
        public virtual SValue ToValueWithTypeCheck(Type type, object? obj)
        {
            if (obj == null) return SValue.Null;

            var objType = obj.GetType();

            if (type != objType || TypeResolver.GeneralTypeIdentifier)
            {
                if (!type.IsInstanceOfType(obj)) throw new SexpConvertException($"Cannot convert {obj} to {type}");

                var value = SexpConvert.GetConverter(objType).ToValue(obj);
                var typeid = TypeResolver.GetTypeId(objType);

                return SValue.Cons(typeid, value);
            }

            return ToValue(obj);
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
