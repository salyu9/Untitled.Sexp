using System;

namespace Untitled.Sexp
{
    /// <summary>
    /// Exception when read sexps.
    /// </summary>
    public class SexpReaderException : SexpException
    {
        /// <summary>
        /// Get the line number of the reader.
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// Get the line position of the reader.
        /// </summary>
        public int LinePosition { get; }

        /// <summary>
        /// Initialize a new instance of <see cref="SexpReaderException" />.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="reader">The reader.</param>
        public SexpReaderException(string message, SexpTextReader reader)
            : base(message)
        {
            LineNumber = reader.LineNumber;
            LinePosition = reader.LinePosition;
        }

        /// <summary>
        /// Initialize a new instance of <see cref="SexpReaderException" />.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="reader">The reader.</param>
        /// <param name="inner">Inner exception.</param>
        public SexpReaderException(string message, SexpTextReader reader, Exception inner)
            : base(message, inner)
        {
            LineNumber = reader.LineNumber;
            LinePosition = reader.LinePosition;
        }

        /// <summary />
        public override string ToString()
        {
            return $"{Message}\nat line: {LineNumber}, pos: {LinePosition}";
        }
    }
}
