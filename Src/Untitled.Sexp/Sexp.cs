using System.Collections.Generic;
using System.IO;
using static Untitled.Sexp.Utils;

namespace Untitled.Sexp
{
    /// <summary>
    /// Static entries.
    /// </summary>
    public static class Sexp
    {
        /// <summary>
        /// Parse a string, return an sexp value.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <param name="settings">Text reader settings. if not specified, default settings will be used.</param>
        /// <returns>The sexp value parsed. If no value available, return SValue.Eof object.</returns>
        public static SValue Parse(string s, SexpTextReaderSettings? settings = null)
        {
            using var reader = new StringReader(s);
            return new SexpTextReader(reader, settings ?? SexpTextReaderSettings.Default).Read();
        }

        /// <summary>
        /// Parse a string, return all sexp values.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <param name="settings">Text reader settings. if not specified, default settings will be used.</param>
        /// <returns>An IEnumerable object iterates over all sexp values parsed.</returns>
        public static IEnumerable<SValue> ParseAll(string s, SexpTextReaderSettings? settings = null)
        {
            using var reader = new StringReader(s);
            foreach (var value in new SexpTextReader(reader, settings ?? SexpTextReaderSettings.Default).ReadAll())
            {
                yield return value;
            }
        }

        /// <summary>
        /// Parse a file, return an sexp value.
        /// </summary>
        /// <param name="filePath">Path to the file.</param>
        /// <param name="settings">Text reader settings. if not specified, default settings will be used.</param>
        /// <returns>The sexp value parsed. If no value available, return SValue.Eof object.</returns>
        public static SValue ParseFile(string filePath, SexpTextReaderSettings? settings = null)
        {
            using var reader = new StreamReader(filePath, Utf8, true);
            return new SexpTextReader(reader).Read();
        }

        /// <summary>
        /// Parse a file, return all sexp values.
        /// </summary>
        /// <param name="filePath">Path to the file.</param>
        /// <param name="settings">Text reader settings. if not specified, default settings will be used.</param>
        /// <returns>An IEnumerable object iterates over all sexp values parsed.</returns>
        public static IEnumerable<SValue> ParseFileAll(string filePath, SexpTextReaderSettings? settings = null)
        {
            using var reader = new StreamReader(filePath, Utf8, true);
            foreach (var value in new SexpTextReader(reader, settings ?? SexpTextReaderSettings.Default).ReadAll())
            {
                yield return value;
            }
        }

    }
}
