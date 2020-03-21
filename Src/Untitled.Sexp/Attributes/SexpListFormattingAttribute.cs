using System;
using Untitled.Sexp.Formatting;

namespace Untitled.Sexp.Attributes
{
    /// <summary>
    /// Specify bool formatting.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class SexpListFormattingAttribute : SexpFormattingAttribute
    {
        /// <summary>
        /// Specify parentheses.
        /// </summary>
        public ParenthesesType Parentheses { get; set; }

        internal override bool AcceptType(Type type)
            => true;

        internal override SValueFormatting Formatting
            => new ListFormatting { Parentheses = Parentheses };
    }
}
