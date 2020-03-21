using System;
using Untitled.Sexp.Formatting;

namespace Untitled.Sexp.Attributes
{
    /// <summary>
    /// Specify bool formatting.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class SexpBooleanFormattingAttribute : SexpFormattingAttribute
    {
        /// <summary>
        /// Specify the value form. #true/#false if true, otherwse #t/#f.
        /// </summary>
        public bool LongForm { get; set; }

        internal override bool AcceptType(Type type)
            => type == typeof(bool);

        internal override SValueFormatting Formatting
            => new BooleanFormatting { LongForm = LongForm };
    }
}
