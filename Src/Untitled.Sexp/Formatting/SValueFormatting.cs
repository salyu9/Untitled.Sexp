namespace Untitled.Sexp.Formatting
{
    /// <summary>
    /// Base class for all formattings.
    /// </summary>
    public abstract class SValueFormatting
    {
        internal abstract void MergeWith(SValueFormatting? other);
    }
}
