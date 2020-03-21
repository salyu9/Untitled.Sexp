using System;
using Untitled.Sexp.Formatting;

namespace Untitled.Sexp.Attributes
{
    /// <summary>
    /// Specify number formatting.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class SexpBytesFormattingAttribute : SexpFormattingAttribute
    {
        /// <summary>
        /// If true, use racket style byte string style, otherwise use R7RS byte vector style.
        /// </summary>
        public bool ByteString { get; set; }

        /// <summary>
        /// When using byte vector style, specify the parentheses type.
        /// </summary>
        public ParenthesesType Parentheses { get; set; }

        /// <summary>
        /// Specify radix.
        /// </summary>
        public NumberRadix Radix { get; set; }

        /// <summary>
        /// When using byte vector style, specify how many bytes will be written in one line.
        /// </summary>
        public int LineLimit { get; set; }

        internal override bool AcceptType(Type type)
            => type == typeof(byte[]);

        internal override SValueFormatting Formatting
            => new BytesFormatting { Radix = Radix, LineLimit = LineLimit, Parentheses = Parentheses };
    }
}
