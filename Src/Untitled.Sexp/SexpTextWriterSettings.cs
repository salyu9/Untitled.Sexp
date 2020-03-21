namespace Untitled.Sexp
{
    /// <summary>
    /// Represents a write that can write sexp values into text writers.
    /// </summary>
    public class SexpTextWriterSettings
    {
        /// <summary>
        /// Get default settings. 
        /// </summary>
        /// <returns></returns>
        public static SexpTextWriterSettings Default
            => new SexpTextWriterSettings();

        /// <summary>
        /// Write null as list. If true, null will be written as "()", otherwise "null".
        /// </summary>
        public NullLiteralType NullLiteral { get; set; }

        /// <summary>
        /// Specify how values will be separated by writer. If set to custom, the value of <see cref="CustomSeparator" /> will be used.
        /// </summary>
        public WriterSeparatorType SeparatorType { get; set; }

        /// <summary>
        /// Get or set the custom separator.
        /// </summary>
        public string CustomSeparator { get; set; } = " ";

        /// <summary>
        /// Initialize default settings.
        /// </summary>
        public SexpTextWriterSettings()
        { }

        /// <summary>
        /// Initialize settings according to another settings.
        /// </summary>
        /// <param name="other"></param>
        public SexpTextWriterSettings(SexpTextWriterSettings other)
        {
            NullLiteral = other.NullLiteral;
            SeparatorType = other.SeparatorType;
            CustomSeparator = other.CustomSeparator;
        }
    }
}
