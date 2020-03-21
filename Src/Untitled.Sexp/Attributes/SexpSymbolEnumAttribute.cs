using System;

namespace Untitled.Sexp.Attributes
{
    /// <summary>
    /// Specify the enum object will be serialized as symbols (or list of symbols if flag).
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum)]
    public sealed class SexpSymbolEnumAttribute : Attribute
    {
    }
}
