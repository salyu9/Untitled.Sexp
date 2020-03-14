namespace Untitled.Sexp
{
    /// <summary>
    /// Sexp value type.
    /// </summary>
    public enum SValueType
    {
        /// <summary>
        /// Null, the value is ().
        /// </summary>
        Null,

        /// <summary>
        /// End-of-file.
        /// </summary>
        Eof,

        /// <summary>
        /// Boolean type, the value is #t or #f.
        /// </summary>
        Boolean,

        /// <summary>
        /// Number type, the value is integer or floaing.
        /// </summary>
        Number,

        /// <summary>
        /// Char type, the value is a Unicode scalar value. <br />
        /// The inner value is int if scalar value is larger than 0xFFFF.
        /// </summary>
        Char,

        /// <summary>
        /// String type.
        /// </summary>
        String,

        ///<summary>
        /// Bytes type.
        /// </summary>
        Bytes,

        /// <summary>
        /// Symbol.
        /// </summary>
        Symbol,

        /// <summary>
        /// Pair.
        /// </summary>
        Pair,
    }
}
