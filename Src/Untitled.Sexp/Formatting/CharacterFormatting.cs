namespace Untitled.Sexp.Formatting
{
    /// <summary>
    /// Specify how strings will be formatted.
    /// </summary>
    public sealed class CharacterFormatting : SValueFormatting
    {
        /// <summary>
        /// Whether to use ascii only. If true, all character larger than 127 will be escaped.
        /// </summary>
        public bool AsciiOnly { get; set; }

        /// <summary>
        /// Specify how string will be escaped.
        /// </summary>
        public EscapingStyle Escaping { get; set; }
    }
}
