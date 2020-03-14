using System;

namespace Untitled.Sexp.Formatting
{
    /// <summary>
    /// Specify how bytes will be formatted.
    /// </summary>
    public sealed class BytesFormatting : SValueFormatting
    {
        /// <summary>
        /// If true, use racket style byte string style, otherwise use R7RS byte vector style.
        /// </summary>
        public bool ByteString { get; set; } = false;

        /// <summary>
        /// When using byte vector style, specify the parenthese type.
        /// </summary>
        public ParentheseType Parenthese { get; set; }

        /// <summary>
        /// When using byte vector style, specify the radix of each byte.
        /// </summary>
        public NumberRadix Radix { get; set; }

        /// <summary>
        /// When using byte vector style, specify how many bytes will be written in one line.
        /// </summary>
        public int LineLimit { get; set; }
    }
}
