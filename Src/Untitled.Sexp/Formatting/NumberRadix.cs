namespace Untitled.Sexp.Formatting
{
    /// <summary>
    /// How number is represented.
    /// </summary>
    public enum NumberRadix
    {
        /// <summary>
        /// Decimal, such as 12345.
        /// </summary>
        Decimal,

        /// <summary>
        /// Prefixed decimal, such as #d12345.
        /// </summary>
        PrefixedDecimal,

        /// <summary>
        /// Hexadecimal, such as #x0af01.
        /// </summary>
        Hexadecimal,

        /// <summary>
        /// Octal, such as #o147.
        /// </summary>
        Octal,

        /// <summary>
        /// Binary, such as #b0101101011.
        /// </summary>
        Binary,
    }
}
