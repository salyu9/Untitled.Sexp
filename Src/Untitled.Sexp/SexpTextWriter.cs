using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Untitled.Sexp.Formatting;
using static Untitled.Sexp.Utils;

namespace Untitled.Sexp
{
    /// <summary>
    /// Represents a write that can write sexp values into text writers.
    /// </summary>
    public class SexpTextWriter
    {
        private static readonly BooleanFormatting DefaultBooleanFormatting = new BooleanFormatting();

        private static readonly NumberFormatting DefaultNumberFormatting = new NumberFormatting();

        private static readonly CharacterFormatting DefaultCharacterFormatting = new CharacterFormatting();

        private static readonly ListFormatting DefaultListFormatting = new ListFormatting();

        private static readonly BytesFormatting DefaultBytesFormatting = new BytesFormatting();

        private static bool IsPrintable(byte b)
        {
            return (b >= 0x20 && b <= 0x7E) || b >= 0xA0;
        }

        private static bool IsPrintable(char ch)
        {
            if (ch <= 0xFF) // latin-1
            {
                return IsPrintable((byte)ch);
            }
            var cat = char.GetUnicodeCategory(ch);
            return cat != UnicodeCategory.Control
                && cat != UnicodeCategory.Format
                && cat != UnicodeCategory.PrivateUse
                && cat != UnicodeCategory.OtherNotAssigned;
        }

        private char[] _surrogateBuffer = new char[2];

        private bool IsPrintable(int point)
        {
            if (point <= 0xFFFF) return IsPrintable((char)point);

            DispartSurrogateToBuffer(point, _surrogateBuffer);
            var cat = char.GetUnicodeCategory(new string(_surrogateBuffer), 0);
            return cat != UnicodeCategory.Control
                && cat != UnicodeCategory.Format
                && cat != UnicodeCategory.PrivateUse
                && cat != UnicodeCategory.OtherNotAssigned;
        }

        private SexpWriterException MakeError(string message, Exception? inner = null)
        {
            return inner == null ? new SexpWriterException(message, this) : new SexpWriterException(message, this, inner);
        }

        private TextWriter _writer;

        private int _linePosition = 0;

        private bool _needSeparator = false;

        private void WriteFinal(char c)
        {
            _writer.Write(c);
            ++_linePosition;
        }

        private void WriteFinal(string s)
        {
            System.Diagnostics.Debug.Assert(s.All(c => IsPrintable(c)));
            int n = 0;
            foreach (var c in s)
            {
                if (char.IsHighSurrogate(c)) ++n;
            }
            _writer.Write(s);
            _linePosition += s.Length - n;
        }

        private void WriteLine()
        {
            _writer.WriteLine();
            _linePosition = 0;
        }

        private void WriteEscapedCharacter(int ch, bool isSymbol, bool uStyle)
        {
            switch (ch)
            {
                case 0x70:
                    WriteFinal(@"\a"); // alarm
                    break;
                case '\b':
                    WriteFinal(@"\b");
                    break;
                case 0x1B:
                    WriteFinal(@"\e"); // escape
                    break;
                case '\f':
                    WriteFinal(@"\f");
                    break;
                case '\n':
                    WriteFinal(@"\n");
                    break;
                case '\r':
                    WriteFinal(@"\r");
                    break;
                case '\t':
                    WriteFinal(@"\t");
                    break;
                case '\v':
                    WriteFinal(@"\v");
                    break;
                case '\"':
                    WriteFinal("\\\"");
                    break;
                case '\'':
                    WriteFinal("\\\'");
                    break;
                case '\\':
                    WriteFinal(@"\\");
                    break;
                default:
                    if (isSymbol && (ch == '|'))
                    {
                        WriteFinal('\\');
                        WriteFinal((char)ch);
                    }
                    else
                    {
                        WriteFinal('\\');
                        if (uStyle)
                        {
                            if (ch >= 0x10000)
                            {
                                WriteFinal('U');
                                WriteFinal(ch.ToString("x06"));
                            }
                            else
                            {
                                WriteFinal('u');
                                WriteFinal(ch.ToString("x04"));
                            }
                        }
                        else
                        {
                            WriteFinal('x');
                            WriteFinal(ch.ToString("x"));
                            WriteFinal(';');
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Settings of the writer.
        /// </summary>
        /// <value></value>
        public SexpTextWriterSettings Settings { get; set; }

        /// <summary>
        /// Initialize new instance of <see cref="SexpTextWriter" />.
        /// </summary>
        public SexpTextWriter(TextWriter writer, SexpTextWriterSettings? settings = null)
        {
            _writer = writer;
            Settings = settings ?? SexpTextWriterSettings.Default;
        }

        /// <summary>
        /// Write an sexp value to the writer.
        /// </summary>
        /// <param name="value">The sexp value to write.</param>
        public void Write(SValue value)
        {
            if (_needSeparator)
            {
                if (Settings.NewLineAsSeparator) WriteLine();
                else WriteFinal(' ');
            }
            WriteValue(value);
            _needSeparator = true;
        }

        /// <summary>
        /// Write all sexp values to the writer.
        /// </summary>
        /// <param name="values">A collection of sexp values to write.</param>
        /// <param name="extraNewline">Should an extra newline be appended after each value.</param>
        public void WriteAll(IEnumerable<SValue> values, bool extraNewline = false)
        {
            foreach (var v in values)
            {
                Write(v);
                if (extraNewline) _writer.WriteLine();
            }
        }

        private void WriteValue(SValue value, int depth = 0)
        {
            switch (value.Type)
            {
                case SValueType.Null:
                    {
                        if (Settings.NullAsList) WriteFinal("()");
                        else WriteFinal("null");
                        return;
                    }
                case SValueType.Boolean:
                    {
                        WriteBoolean((bool)value._value, (BooleanFormatting?)value.Formatting ?? DefaultBooleanFormatting);
                        return;
                    }
                case SValueType.Number:
                    {
                        var formatting = (NumberFormatting?)value.Formatting ?? DefaultNumberFormatting;
                        if (value._value is int i) WriteInteger(i, formatting);
                        else if (value._value is long l) WriteInteger(l, formatting);
                        else if (value._value is double d) WriteDouble(d, formatting);
                        else throw MakeError($"Number contains invalid data {value._value}");
                        return;
                    }
                case SValueType.Char:
                    {
                        WriteChar((int)value._value, (CharacterFormatting?)value.Formatting ?? DefaultCharacterFormatting);
                        return;
                    }
                case SValueType.Symbol:
                    {
                        WriteSymbol((SSymbol)value._value, (CharacterFormatting?)value.Formatting ?? DefaultCharacterFormatting);
                        return;
                    }
                case SValueType.String:
                    {
                        WriteString((string)value._value, (CharacterFormatting?)value.Formatting ?? DefaultCharacterFormatting);
                        return;
                    }
                case SValueType.Bytes:
                    {
                        WriteBytes((byte[])value._value, (BytesFormatting?)value.Formatting ?? DefaultBytesFormatting);
                        return;
                    }
                case SValueType.Pair:
                    {
                        WritePair((SPair)value._value, (ListFormatting?)value.Formatting ?? DefaultListFormatting, depth);
                        return;
                    }
                default:
                    throw new SexpException($"Invalid value type {value.Type}");
            }
        }

        private void WriteIndent(int count)
        {
            while (count-- > 0) WriteFinal(' ');
        }

        private void WriteBoolean(bool value, BooleanFormatting formatting)
        {
            if (formatting.LongForm)
            {
                if (value) WriteFinal("#true");
                else WriteFinal("#false");
            }
            else
            {
                if (value) WriteFinal("#t");
                else WriteFinal("#f");
            }
        }

        private void WriteInteger(long n, NumberFormatting formatting)
        {
            switch (formatting.Radix)
            {
                case NumberRadix.Decimal:
                    WriteFinal(n.ToString());
                    return;
                case NumberRadix.PrefixedDecimal:
                    WriteFinal("#d");
                    WriteFinal(n.ToString());
                    return;
                case NumberRadix.Hexadecimal:
                    WriteFinal("#x");
                    WriteFinal(Convert.ToString(n, 16));
                    return;
                case NumberRadix.Octal:
                    WriteFinal("#o");
                    WriteFinal(Convert.ToString(n, 8));
                    return;
                case NumberRadix.Binary:
                    WriteFinal("#b");
                    WriteFinal(Convert.ToString(n, 2));
                    return;
                default:
                    throw MakeError($"Invalid number radix {formatting.Radix}");
            }
        }

        private void WriteDouble(double d, NumberFormatting formatting)
        {
            var radix = formatting.Radix;
            if (radix != NumberRadix.Decimal
                && radix != NumberRadix.PrefixedDecimal
                && !double.IsNaN(d) && !double.IsInfinity(d))
            {
                throw MakeError($"Invalid radix {radix} for floating number");
            }

            switch (radix)
            {
                case NumberRadix.PrefixedDecimal:
                    WriteFinal("#d");
                    break;
                case NumberRadix.Hexadecimal:
                    WriteFinal("#x");
                    break;
                case NumberRadix.Octal:
                    WriteFinal("#o");
                    break;
                case NumberRadix.Binary:
                    WriteFinal("#b");
                    break;
            }

            if (double.IsNaN(d)) WriteFinal("+nan.0");
            else if (double.IsPositiveInfinity(d)) WriteFinal("+inf.0");
            else if (double.IsNegativeInfinity(d)) WriteFinal("-inf.0");
            else WriteFinal(d.ToString());
        }

        private void WriteChar(int ch, CharacterFormatting formatting)
        {
            switch (ch)
            {
                case 0x07:
                    WriteFinal(@"#\alarm");
                    return;
                case '\b':
                    WriteFinal(@"#\backspace");
                    return;
                case 0x7F:
                    WriteFinal(@"#\delete");
                    return;
                case 0x1B:
                    WriteFinal(@"#\escape");
                    return;
                case '\f':
                    WriteFinal(@"#\formfeed");
                    return;
                case '\n':
                    WriteFinal(@"#\linefeed");
                    return;
                case '\0':
                    WriteFinal(@"#\null");
                    return;
                case '\r':
                    WriteFinal(@"#\return");
                    return;
                case ' ':
                    WriteFinal(@"#\space");
                    return;
                case '\t':
                    WriteFinal(@"#\tab");
                    return;
                case '\v':
                    WriteFinal(@"#\vtab");
                    return;
            }

            var shouldEscape = (formatting.AsciiOnly && ch > 0x7e)
                || !IsPrintable(ch);

            if (!shouldEscape)
            {
                WriteFinal(@"#\");
                if (ch <= 0xFFFF) WriteFinal((char)ch);
                else
                {
                    DispartSurrogateToBuffer(ch, _surrogateBuffer);
                    WriteFinal(_surrogateBuffer[0]);
                    WriteFinal(_surrogateBuffer[1]);
                }
                return;
            }

            if (formatting.Escaping == EscapingStyle.UStyle)
            {
                if (ch <= 0xFFFF)
                {
                    WriteFinal(@"#\u");
                    WriteFinal(ch.ToString("x04"));
                }
                else
                {
                    WriteFinal(@"#\U");
                    WriteFinal(ch.ToString("x06"));
                }
            }
            else
            {
                WriteFinal(@"#\x");
                WriteFinal(ch.ToString("x"));
            }
        }

        private void WriteSymbol(SSymbol value, CharacterFormatting characterFormatting)
        {
            var name = value.Name;
            var shouldEscape = name.Length == 0;

            var asciiOnly = characterFormatting.AsciiOnly;
            var uStyleEscape = characterFormatting.Escaping == EscapingStyle.UStyle;

            if (!shouldEscape)
            {
                foreach (var scalar in EnumerateScalarValues(name))
                {
                    if ((asciiOnly && (scalar < 0x20 || scalar > 0x7e)) || scalar == '|' || scalar == '\\' || IsDelimiter(scalar) || !IsPrintable(scalar))
                    {
                        shouldEscape = true;
                        break;
                    }
                }
            }

            if (shouldEscape)
            {
                WriteFinal('|');
                foreach (var scalar in EnumerateScalarValues(name))
                {
                    if ((asciiOnly && (scalar < 0x20 || scalar > 0x7e)) || scalar == '|' || scalar == '\\' || !IsPrintable(scalar))
                    {
                        WriteEscapedCharacter(scalar, true, uStyleEscape);
                    }
                    else if (scalar >= 0x10000)
                    {
                        DispartSurrogateToBuffer(scalar, _surrogateBuffer);
                        WriteFinal(_surrogateBuffer[0]);
                        WriteFinal(_surrogateBuffer[1]);
                    }
                    else
                    {
                        WriteFinal((char)scalar);
                    }
                }
                WriteFinal('|');
            }
            else
            {
                WriteFinal(name);
            }
        }

        private void WriteString(string value, CharacterFormatting characterFormatting)
        {
            var asciiOnly = characterFormatting.AsciiOnly;
            var uStyleEscape = characterFormatting.Escaping == EscapingStyle.UStyle;

            WriteFinal('"');
            foreach (var scalar in EnumerateScalarValues(value))
            {
                if ((asciiOnly && (scalar < 0x20 || scalar > 0x7e)) || scalar == '"' || !IsPrintable(scalar))
                {
                    WriteEscapedCharacter(scalar, false, uStyleEscape);
                }
                else if (scalar >= 0x10000)
                {
                    DispartSurrogateToBuffer(scalar, _surrogateBuffer);
                    WriteFinal(_surrogateBuffer[0]);
                    WriteFinal(_surrogateBuffer[1]);
                }
                else
                {
                    WriteFinal((char)scalar);
                }
            }
            WriteFinal('"');
        }

        private void WriteByte(byte b, NumberRadix radix)
        {
            switch (radix)
            {
                case NumberRadix.Decimal:
                    WriteFinal(b.ToString());
                    break;
                case NumberRadix.PrefixedDecimal:
                    WriteFinal("#d");
                    WriteFinal(b.ToString());
                    break;
                case NumberRadix.Hexadecimal:
                    WriteFinal("#x");
                    WriteFinal(Convert.ToString(b, 16));
                    break;
                case NumberRadix.Octal:
                    WriteFinal("#o");
                    WriteFinal(Convert.ToString(b, 8));
                    break;
                case NumberRadix.Binary:
                    WriteFinal("#b");
                    WriteFinal(Convert.ToString(b, 2));
                    break;
                default:
                    throw new ArgumentException($"Invalid {nameof(NumberRadix)}: {radix}");
            }
        }

        private void WriteBytes(byte[] value, BytesFormatting bytesFormatting)
        {
            if (bytesFormatting.ByteString)
            {
                WriteFinal('#');
                WriteFinal('"');
                foreach (var b in value)
                {
                    if (b == '"' || (b < 0x20 || b > 0x7e))
                    {
                        WriteEscapedCharacter(b, false, false);
                    }
                    else
                    {
                        WriteFinal((char)b);
                    }
                }
                WriteFinal('"');
            }
            else
            {
                var parenthese = bytesFormatting.Parentheses;
                WriteFinal("#u8");
                WriteFinal(parenthese switch
                {
                    ParenthesesType.Parentheses => '(',
                    ParenthesesType.Brackets => '[',
                    ParenthesesType.Braces => '{',
                    _ => throw new ArgumentException($"Invalid {nameof(ParenthesesType)}: {parenthese}")
                });

                var limit = bytesFormatting.LineLimit;
                var radix = bytesFormatting.Radix;
                var i = 0;
                if (limit == 0)
                {
                    foreach (var b in value)
                    {
                        if (i != 0) WriteFinal(' ');
                        WriteByte(b, radix);
                        ++i;
                    }
                }
                else
                {
                    var l = _linePosition;
                    foreach (var b in value)
                    {
                        if (i != 0)
                        {
                            if (i % limit == 0)
                            {
                                WriteLine();
                                WriteIndent(l);
                            }
                            else WriteFinal(' ');
                        }
                        WriteByte(b, radix);
                        ++i;
                    }
                }

                WriteFinal(parenthese switch
                {
                    ParenthesesType.Parentheses => ')',
                    ParenthesesType.Brackets => ']',
                    ParenthesesType.Braces => '}',
                    _ => throw new ArgumentException($"Invalid {nameof(ParenthesesType)}: {parenthese}")
                });
            }
        }

        private void WritePair(SPair pair, ListFormatting listFormatting, int depth)
        {
            var l = _linePosition + listFormatting.LineExtraSpaces;

            var parenthese = listFormatting.Parentheses;
            WriteFinal(parenthese switch
            {
                ParenthesesType.Parentheses => '(',
                ParenthesesType.Brackets => '[',
                ParenthesesType.Braces => '{',
                _ => throw new ArgumentException($"Invalid {nameof(ParenthesesType)}: {parenthese}")
            });

            var i = 0;
            var breakIndex = listFormatting.LineBreakIndex;
            SValue current = pair;
            while (current.IsPair)
            {
                var currentPair = (SPair)current._value;
                if (i >= breakIndex)
                {
                    WriteLine();
                    WriteIndent(l);
                }
                else if (i > 0)
                {
                    WriteFinal(' ');
                }
                WriteValue(currentPair._car, depth + 1);
                current = currentPair._cdr;
                ++i;
            }

            if (!current.IsNull) // improper list
            {
                if (i > breakIndex)
                {
                    WriteLine();
                    WriteIndent(l);
                    WriteFinal('.');
                    WriteLine();
                    WriteIndent(l);
                }
                else
                {
                    WriteFinal(" . ");
                }
                WriteValue(current, depth + 1);
            }

            WriteFinal(parenthese switch
            {
                ParenthesesType.Parentheses => ')',
                ParenthesesType.Brackets => ']',
                ParenthesesType.Braces => '}',
                _ => throw new ArgumentException($"Invalid {nameof(ParenthesesType)}: {parenthese}")
            });
        }

    }
}
