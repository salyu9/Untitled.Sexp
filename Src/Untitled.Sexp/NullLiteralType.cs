namespace Untitled.Sexp
{
    /// <summary>
    /// Specify how () will be written.
    /// </summary>
    public enum NullLiteralType
    {
        /// <summary>
        /// () will be written as ()
        /// </summary>
        EmptyList,

        /// <summary>
        /// () will be written as null
        /// </summary>
        Null,

        /// <summary>
        /// () will be written as nil
        /// </summary>
        Nil,
    }
}
