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
        public ParenthesesType? Parentheses { get; set; }

        /// <summary>
        /// Specify single line element count. <br />
        /// For example with index = 3, (a b c d e f g) will be written as "(a b  c\n  d\n e\n f\n g)". <br />
        /// Will has no effect when <see cref="LineElemsCount" /> != null .
        /// </summary>
        public int? LineBreakIndex { get; set; }

        /// <summary>
        /// Specify elems count in one line. <br />
        /// For example with count = 2, (a b c d e f g) will be written as "(a b\n c d\n e f\n g)". <br />
        /// </summary>
        public int? LineElemsCount { get; set; }

        /// <summary>
        /// When write in multiline, specify extra space count before each new lines.
        /// </summary>
        public int? LineExtraSpaces { get; set; }

        internal override void MergeWith(SValueFormatting? other)
        {
            if (other == null) return;
            var f = (ListFormatting)other;
            Parentheses = f.Parentheses ?? Parentheses;
            LineBreakIndex = f.LineBreakIndex ?? LineBreakIndex;
            LineElemsCount = f.LineElemsCount ?? LineElemsCount;
            LineExtraSpaces = f.LineExtraSpaces ?? LineExtraSpaces;
        }
    }
}
