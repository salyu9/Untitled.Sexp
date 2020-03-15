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
        public bool NullAsList { get; set; } = true;

        /// <summary>
        /// If true, values will be separated with newline. else space is used as separator.
        /// </summary>
        public bool NewLineAsSeparator { get; set; } = true;
    }
}
