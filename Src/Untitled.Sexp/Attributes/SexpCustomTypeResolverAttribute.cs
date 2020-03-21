using System;
using Untitled.Sexp.Conversion;

namespace Untitled.Sexp.Attributes
{
    /// <summary>
    /// This type is a base type and will have a custom type resolver.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SexpCustomTypeResolverAttribute : Attribute
    {
        /// <summary>
        /// The resolver type of this type.
        /// </summary>
        public Type ResolverType { get; set; }

        /// <summary>
        /// Initialize new instance of <see cref="SexpCustomTypeResolverAttribute" /> with resolver type.
        /// </summary>
        public SexpCustomTypeResolverAttribute(Type resolverType)
        {
            ResolverType = resolverType;
        }
    }
}
