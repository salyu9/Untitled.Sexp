using System;

namespace Untitled.Sexp
{
    /// <summary>
    /// Exception when write sexps.
    /// </summary>
    public class SexpWriterException : SexpException
    {

        /// <summary>
        /// Initialize a new instance of <see cref="SexpWriterException" />.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="writer">The writer.</param>
        public SexpWriterException(string message, SexpTextWriter writer)
            : base(message)
        {
        }

        /// <summary>
        /// Initialize a new instance of <see cref="SexpWriterException" />.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="writer">The writer.</param>
        /// <param name="inner">Inner exception.</param>
        public SexpWriterException(string message, SexpTextWriter writer, Exception inner)
            : base(message, inner)
        {
        }
    }
}
