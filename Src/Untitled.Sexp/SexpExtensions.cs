using System;

namespace Untitled.Sexp
{
    /// <summary>
    /// Extensions for sexp values.
    /// </summary>
    public static class SexpExtensions
    {
        /// <summary>
        /// Get the length of sexp list. Throws if value is not list.
        /// </summary>
        public static int Length(this SValue value)
        {
                var n = 0;
                var current = value;
                while (!current.IsNull)
                {
                    if (!current.IsPair) throw new InvalidCastException($"The sexp value is not a list");
                    var pair = (Pair)current._value;
                    ++n;
                    current = pair._cdr;
                }
                return n;
        }
    }
}
