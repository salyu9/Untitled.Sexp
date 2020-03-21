namespace Untitled.Sexp.Attributes
{
    /// <summary>
    /// Specify how association list is serialized.
    /// </summary>
    public enum AssociationListStyle
    {
        /// <summary>
        /// As list of pairs. For example: {a: 1, b: 2} -> ((a . 1) (b . 2))
        /// </summary>
        ListOfPairs,

        /// <summary>
        /// As flat list. For example: {a: 1, b: 2} -> (a 1 b 2)
        /// </summary>
        Flat,

        /// <summary>
        /// As list of lists. For example: {a: 1, b: 2} -> ((a 1) (b 2))
        /// </summary>
        ListOfLists,
    }
}
