namespace Untitled.Sexp
{
    /// <summary>
    /// Specify how values will be separated by writer.
    /// </summary>
    public enum WriterSeparatorType
    {
        /// <summary>
        /// Separate by a newline.
        /// </summary>
        Newline,

        /// <summary>
        /// Separate by two newlines.
        /// </summary>
        DoubleNewline,

        /// <summary>
        /// Separate by a space.
        /// </summary>
        Space,

        /// <summary>
        /// Separate by custom string.
        /// </summary>
        Custom,
    }
}
