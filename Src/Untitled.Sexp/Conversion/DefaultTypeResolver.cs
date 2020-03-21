using System;

namespace Untitled.Sexp.Conversion
{
    /// <summary>
    /// Default type resolver, map the types to object of <see cref="TypeIdentifier"/>.
    /// </summary>
    public sealed class DefaultTypeResolver : TypeResolver
    {
        /// <summary>
        /// Get type for the typeid value.
        /// </summary>
        public override Type Resolve(TypeIdentifier typeid)
        {
            return Type.GetType(typeid.Name, true, true);
        }

        /// <summary>
        /// Get typeid for the type.
        /// </summary>
        public override TypeIdentifier GetTypeId(Type type)
        {
            return TypeIdentifier.FromString(type.AssemblyQualifiedName);
        }
    }
}
