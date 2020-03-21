using System;
using Untitled.Sexp.Formatting;

namespace Untitled.Sexp.Attributes
{
    /// <summary>
    /// Specify number formatting.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class SexpNumberFormattingAttribute : SexpFormattingAttribute
    {
        /// <summary>
        /// Specify radix.
        /// </summary>
        public NumberRadix Radix { get; set; }

        internal override bool AcceptType(Type type)
            => Type.GetTypeCode(type) switch 
            {
                TypeCode.Byte => true,
                TypeCode.SByte => true,
                TypeCode.UInt16 => true,
                TypeCode.UInt32 => true,
                TypeCode.UInt64 => true,
                TypeCode.Int16 => true,
                TypeCode.Int32 => true,
                TypeCode.Int64 => true,
                TypeCode.Double => true,
                TypeCode.Single => true,
                _ => false,
            };

        internal override SValueFormatting Formatting
            => new NumberFormatting { Radix = Radix };
    }
}
