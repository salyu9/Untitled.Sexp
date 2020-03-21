using System;

namespace Untitled.Sexp.Attributes
{
    /// <summary>
    /// Ignore this field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class SexpIgnoreAttribute : Attribute
    {
        
    }
}
