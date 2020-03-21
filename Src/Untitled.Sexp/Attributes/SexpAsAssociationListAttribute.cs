using System;
using Untitled.Sexp.Formatting;

namespace Untitled.Sexp.Attributes
{
    /// <summary>
    /// Serialize this type as association list.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class SexpAsAssociationListAttribute : Attribute
    {
        /// <summary>
        /// Specify the inner parentheses of the association list.
        /// </summary>
        public ParenthesesType InnerParentheses { get; set; }

        /// <summary>
        /// Specify the presentation style of association list. 
        /// </summary>
        public AssociationListStyle Style { get; set; }

        /// <summary>
        /// If multiline, each key-value pair will be in separate lines.
        /// </summary>
        public bool Multiline { get; set; } = true;
    }
}
