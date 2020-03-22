using System;

namespace Untitled.Sexp.Conversion
{
    /// <summary>
    /// Helper for polymophic type converter.
    /// </summary>
    public abstract class TypeResolver
    {
        /// <summary>
        /// Get type for the typeid value.
        /// </summary>
        public abstract Type Resolve(SValue typeid);

        /// <summary>
        /// Get typeid for the type.
        /// </summary>
        public abstract SValue GetTypeId(Type type);

        /// <summary>
        /// If <see cref="GetTypeId" />() always yield <see cref="TypeIdentifier" />, returns false, otherwise true. <br />
        /// If this property is true, typeid will always be written by this type resolver.
        /// </summary>
        public abstract bool GeneralTypeIdentifier { get; }

        /// <summary>
        /// Default type resolver.
        /// </summary>
        public static TypeResolver Default { get; } = new DefaultTypeResolver();
    }
}
