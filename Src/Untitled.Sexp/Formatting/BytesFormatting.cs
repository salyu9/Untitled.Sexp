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
        public bool ByteString { get; set; }

        /// <summary>
        /// When using byte vector style, specify the parentheses type.
        /// </summary>
        public ParenthesesType Parentheses { get; set; }

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
