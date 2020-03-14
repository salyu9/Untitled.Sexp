using System;

namespace Untitled.Sexp.Formatting
{
    /// <summary>
    /// Specify how bools will be formatted.
    /// </summary>
    public sealed class BooleanFormatting : SValueFormatting
    {
        /// <summary>
        /// Whether use long form. If true, bools will be written as #true/#false, otherwise #t/#f.
        /// </summary>
        public bool LongForm { get; set; } = false;
    }
}
