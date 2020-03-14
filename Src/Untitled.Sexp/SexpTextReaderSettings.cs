namespace Untitled.Sexp
{
    /// <summary>
    /// Settings for <see cref="SexpTextReader" />.
    /// </summary>
    public class SexpTextReaderSettings
    {
        /// <summary>
        /// Get default settings. Allows all.
        /// </summary>
        public static SexpTextReaderSettings Default
            => new SexpTextReaderSettings();

        /// <summary>
        /// Whether [] allowed for list and bytes representation.
        /// </summary>
        public bool AllowBracket { get; set; } = true;

        /// <summary>
        /// Whether {} allowed for list and bytes representation.
        /// </summary>
        public bool AllowBrace { get; set; } = true;

        /// <summary>
        /// Whether #u8() byte vector allowed for byte[].
        /// </summary>
        public bool AllowR7rsStyleByteVector { get; set; } = true;

        /// <summary>
        /// Whether #"" byte string allowed for byte[].
        /// </summary>
        /// <value></value>
        public bool AllowRacketStyleByteString { get; set; } = true;

        /// <summary>
        /// Whether #\u and #\U allowed in character.
        /// </summary>
        public bool AllowUInCharacter { get; set; } = true;

        /// <summary>
        /// Whether \u and \U allowed in string and symbol.
        /// </summary>
        /// <value></value>
        public bool AllowUInEscaping { get; set; } = true;

        /// <summary>
        /// Initialize default settings.
        /// </summary>
        public SexpTextReaderSettings()
        { }

        /// <summary>
        /// Initialize settings according to another settings.
        /// </summary>
        public SexpTextReaderSettings(SexpTextReaderSettings other)
        {
            AllowBracket = other.AllowBracket;
            AllowBrace = other.AllowBrace;
            AllowR7rsStyleByteVector = other.AllowR7rsStyleByteVector;
            AllowRacketStyleByteString = other.AllowRacketStyleByteString;
            AllowUInCharacter = other.AllowUInCharacter;
            AllowUInEscaping = other.AllowUInEscaping;
        }
    }
}
