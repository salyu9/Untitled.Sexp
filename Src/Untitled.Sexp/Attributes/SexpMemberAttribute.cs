using System;
using Untitled.Sexp.Conversion;
using Untitled.Sexp.Formatting;

namespace Untitled.Sexp.Attributes
{
    /// <summary>
    /// Specify serialization format.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class SexpMemberAttribute : Attribute
    {
        /// <summary>
        /// Specify name of the member.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Specify the member order in the list.
        /// </summary>
        public int Order { get; set; } = -1;

        /// <summary>
        /// Initialize new instance of <see cref="SexpMemberAttribute" />
        /// </summary>
        public SexpMemberAttribute(string? name = null)
        {
            Name = name;
        }
    }
}
