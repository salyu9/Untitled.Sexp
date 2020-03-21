namespace Untitled.Sexp.Formatting
{
    /// <summary>
    /// Specify how numbers will be formatted.
    /// </summary>
    public sealed class NumberFormatting : SValueFormatting
    {
        /// <summary>
        /// The radix format of number.
        /// </summary>
        /// <value></value>
        public NumberRadix? Radix { get; set; }

        internal override void MergeWith(SValueFormatting? other)
        {
            if (other == null) return;
            Radix = ((NumberFormatting)other).Radix ?? Radix;
        }
    }
}
