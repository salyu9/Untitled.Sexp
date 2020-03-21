using System;

namespace Untitled.Sexp.Attributes
{
    /// <summary>
    /// This type will be converted by custom converter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public sealed class SexpCustomConverterAttribute : Attribute
    {
        /// <summary>
        /// Converter type.
        /// </summary>
        public Type ConverterType { get; set; }

        /// <summary>
        /// Initialize an new <see cref="SexpCustomConverterAttribute" />
        /// </summary>
        /// <param name="converterType">The custom converter type.</param>
        public SexpCustomConverterAttribute(Type converterType)
        {
            ConverterType = converterType;
        }
    }
}
