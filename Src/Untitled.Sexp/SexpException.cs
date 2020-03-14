using System;

namespace Untitled.Sexp
{
    /// <summary>
    /// Base type of all Sexp exceptions.
    /// </summary>
    public class SexpException : Exception
    {
        /// <summary>
        /// Initialize new instance of <see cref="SexpException" />.
        /// </summary>
        public SexpException(string message) : base(message)
        {
        }

        /// <summary>
        /// Initialize new instance of <see cref="SexpException" /> with inner exception.
        /// </summary>
        public SexpException(string message, Exception inner) : base(message, inner)
        {
        }
    }
}
