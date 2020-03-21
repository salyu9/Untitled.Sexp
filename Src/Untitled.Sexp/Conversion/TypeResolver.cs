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
        public abstract Type Resolve(TypeIdentifier typeid);

        /// <summary>
        /// Get typeid for the type.
        /// </summary>
        public abstract TypeIdentifier GetTypeId(Type type);

        /// <summary>
        /// Default type resolver.
        /// </summary>
        public static TypeResolver Default { get; } = new DefaultTypeResolver();
    }
}
