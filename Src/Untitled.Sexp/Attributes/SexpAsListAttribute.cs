using System;
using Untitled.Sexp.Formatting;

namespace Untitled.Sexp.Attributes
{
    /// <summary>
    /// Serialize this type as list (fields/properties in order).
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class SexpAsListAttribute : Attribute
    {
    }
}
