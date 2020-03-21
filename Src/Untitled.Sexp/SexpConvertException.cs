using System;

namespace Untitled.Sexp
{
    /// <summary>
    /// Exception when convert sexps.
    /// </summary>
    public sealed class SexpConvertException : SexpException
    {
        /// <summary>
        /// Initialize a new instance of <see cref="SexpConvertException" />.
        /// </summary>
        /// <param name="message">Error message.</param>
        public SexpConvertException(string message)
            : base(message)
        { }

        /// <summary>
        /// Initialize a new instance of <see cref="SexpConvertException" />.
        /// </summary>
        /// <param name="type">Convert source/target type.</param>
        /// <param name="value">The value to be converted.</param>
        public SexpConvertException(Type type, object value)
            : base($"Cannot convert {value} from/to {type}")
        { }

        /// <summary>
        /// Initialize a new instance of <see cref="SexpConvertException" />.
        /// </summary>
        /// <param name="targetType">Convert source/target type.</param>
        /// <param name="value">The value to be converted.</param>
        /// <param name="inner">Inner exception.</param>
        public SexpConvertException(Type targetType, object value, Exception inner)
            : base($"Cannot convert {value} from/to {targetType}", inner)
        { }
        
        /// <summary>
        /// Initialize a new instance of <see cref="SexpConvertException" />.
        /// </summary>
        /// <param name="message">Extra message.</param>
        /// <param name="type">Convert source/target type.</param>
        /// <param name="value">The value to be converted.</param>
        public SexpConvertException(string message, Type type, object value)
            : base($"Cannot convert {value} from/to {type}: " + message)
        { }
    }
}
