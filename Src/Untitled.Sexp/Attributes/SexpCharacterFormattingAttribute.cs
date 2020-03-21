using System;
using Untitled.Sexp.Formatting;

namespace Untitled.Sexp.Attributes
{
    /// <summary>
    /// Specify bool formatting.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class SexpCharacterFormattingAttribute : SexpFormattingAttribute
    {
        /// <summary>
        /// Whether to use ascii only. If true, all character larger than 127 will be escaped.
        /// </summary>
        public bool AsciiOnly { get; set; }

        /// <summary>
        /// Specify how string will be escaped.
        /// </summary>
        public EscapingStyle Escaping { get; set; }

        internal override bool AcceptType(Type type)
            => type == typeof(string) || type == typeof(char);

        internal override SValueFormatting Formatting
            => new CharacterFormatting { AsciiOnly = AsciiOnly, Escaping = Escaping };
    }
}
