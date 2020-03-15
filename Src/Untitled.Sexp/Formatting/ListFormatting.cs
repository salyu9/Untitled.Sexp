namespace Untitled.Sexp.Formatting
{
    /// <summary>
    /// Specify how lists will be formatted.
    /// </summary>
    public sealed class ListFormatting : SValueFormatting
    {
        /// <summary>
        /// Specify parentheses type.
        /// </summary>
        public ParenthesesType Parentheses { get; set; }

        /// <summary>
        /// Specify single line element count. <br />
        /// For example with index = 2, (a b c d) will be written with "(a b\n  c\n  d)". <br />
        /// Default is int.MaxValue so that all elements will be written in single line.
        /// </summary>
        public int LineBreakIndex { get; set; } = int.MaxValue;

        /// <summary>
        /// When write in multiline, specify extra space count before each new lines.
        /// </summary>
        public int LineExtraSpaces { get; set; } = 2;
    }
}
