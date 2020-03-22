using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        private static char GetOpeningChar(ParenthesesType parentheses)
        {
            return parentheses switch
            {
                ParenthesesType.Parentheses => '(',
                ParenthesesType.Brackets => '[',
                ParenthesesType.Braces => '{',
                _ => throw CreateInvalidEnumException(nameof(parentheses), parentheses)
            };
        }

        private static char GetClosingChar(ParenthesesType parentheses)
        {
            return parentheses switch
            {
                ParenthesesType.Parentheses => ')',
                ParenthesesType.Brackets => ']',
                ParenthesesType.Braces => '}',
                _ => throw CreateInvalidEnumException(nameof(parentheses), parentheses)
            };
        }

        private SexpWriterException MakeError(string message, Exception? inner = null)
        {
            return inner == null ? new SexpWriterException(message, this) : new SexpWriterException(message, this, inner);
        }

        private TextWriter _writer;

        private int _linePosition = 0;

        private bool _needSeparator = false;

        private void Put(char c)
        {
            _writer.Write(c);
            ++_linePosition;
        }

        private void Put(string s)
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

        private void PutNewLine()
        {
            _writer.WriteLine();
            _linePosition = 0;
        }

        private void WriteEscapedCharacter(int ch, bool isSymbol, bool uStyle)
        {
            switch (ch)
            {
                case 0x70:
                    Put(@"\a"); // alarm
                    break;
                case '\b':
                    Put(@"\b");
                    break;
                case 0x1B:
                    Put(@"\e"); // escape
                    break;
                case '\f':
                    Put(@"\f");
                    break;
                case '\n':
                    Put(@"\n");
                    break;
                case '\r':
                    Put(@"\r");
                    break;
                case '\t':
                    Put(@"\t");
                    break;
                case '\v':
                    Put(@"\v");
                    break;
                case '\"':
                    Put("\\\"");
                    break;
                case '\'':
                    Put("\\\'");
                    break;
                case '\\':
                    Put(@"\\");
                    break;
                default:
                    if (isSymbol && (ch == '|'))
                    {
                        Put('\\');
                        Put((char)ch);
                    }
                    else
                    {
                        Put('\\');
                        if (uStyle)
                        {
                            if (ch >= 0x10000)
                            {
                                Put('U');
                                Put(ch.ToString("x08"));
                            }
                            else
                            {
                                Put('u');
                                Put(ch.ToString("x04"));
                            }
                        }
                        else
                        {
                            Put('x');
                            Put(ch.ToString("x"));
                            Put(';');
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
        public SexpTextWriter(TextWriter writer)
        {
            _writer = writer;
            Settings = new SexpTextWriterSettings();
        }

        /// <summary>
        /// Initialize new instance of <see cref="SexpTextWriter" />.
        /// </summary>
        public SexpTextWriter(TextWriter writer, SexpTextWriterSettings settings)
        {
            _writer = writer;
            Settings = new SexpTextWriterSettings(settings);
        }

        /// <summary>
        /// Write an sexp value to the writer.
        /// </summary>
        /// <param name="value">The sexp value to write.</param>
        /// <param name="formatting">Formatting for the sexp value.</param>
        public void Write(SValue value, SValueFormatting? formatting = null)
        {
            if (_needSeparator)
            {
                switch (Settings.SeparatorType)
                {
                    case WriterSeparatorType.Newline:
                        {
                            PutNewLine();
                            break;
                        }
                    case WriterSeparatorType.DoubleNewline:
                        {
                            PutNewLine();
                            PutNewLine();
                            break;
                        }
                    case WriterSeparatorType.Space:
                        {
                            Put(' ');
                            break;
                        }
                    case WriterSeparatorType.Custom:
                        {
                            Put(Settings.CustomSeparator);
                            break;
                        }
                    default:
                        throw new ArgumentException($"Invalid {nameof(WriterSeparatorType)}: {Settings.SeparatorType}");
                }
            }
            WriteValue(value, formatting);
            _needSeparator = true;
        }

        /// <summary>
        /// Write all sexp values to the writer.
        /// </summary>
        /// <param name="values">A collection of sexp values to write.</param>
        public void WriteAll(IEnumerable<SValue> values)
        {
            foreach (var v in values)
            {
                Write(v);
            }
        }

        private static TFormatting GetFormatting<TFormatting>(SValue value, SValueFormatting? formatting, TFormatting defaultFormatting)
            where TFormatting : SValueFormatting
        {
            if (formatting != null)
            {
                return (TFormatting)formatting;
            }
            return (TFormatting?)value.Formatting ?? defaultFormatting;
        }

        private void WriteValue(SValue value, SValueFormatting? formatting = null, int depth = 0)
        {
            switch (value.Type)
            {
                case SValueType.Null:
                    {
                        Put(Settings.NullLiteral switch
                        {
                            NullLiteralType.EmptyList => "()",
                            NullLiteralType.Null => "null",
                            NullLiteralType.Nil => "nil",
                            _ => throw CreateInvalidEnumException("Type", value.Type)
                        });
                        return;
                    }
                case SValueType.Boolean:
                    {
                        WriteBoolean((bool)value._value, GetFormatting(value, formatting, DefaultBooleanFormatting));
                        return;
                    }
                case SValueType.Number:
                    {
                        var numFormatting = GetFormatting(value, formatting, DefaultNumberFormatting);
                        if (value._value is long l) WriteInteger(l, numFormatting);
                        else if (value._value is double d) WriteDouble(d, numFormatting);
                        else throw MakeError($"Number contains invalid data {value._value}");
                        return;
                    }
                case SValueType.Char:
                    {
                        WriteChar((int)value._value, GetFormatting(value, formatting, DefaultCharacterFormatting));
                        return;
                    }
                case SValueType.Symbol:
                    {
                        WriteSymbol((Symbol)value._value, GetFormatting(value, formatting, DefaultCharacterFormatting));
                        return;
                    }
                case SValueType.String:
                    {
                        WriteString((string)value._value, GetFormatting(value, formatting, DefaultCharacterFormatting));
                        return;
                    }
                case SValueType.Bytes:
                    {
                        WriteBytes((byte[])value._value, GetFormatting(value, formatting, DefaultBytesFormatting));
                        return;
                    }
                case SValueType.Pair:
                    {
                        WritePair((Pair)value._value, GetFormatting(value, formatting, DefaultListFormatting), depth);
                        return;
                    }
                case SValueType.TypeIdentifier:
                    {
                        WriteTypeIdentifier((TypeIdentifier)value._value, GetFormatting(value, formatting, DefaultCharacterFormatting));
                        return;
                    }
                default:
                    throw new SexpException($"Invalid value type {value.Type}");
            }
        }

        private void PutIndent(int count)
        {
            while (count-- > 0) Put(' ');
        }

        private void WriteBoolean(bool value, BooleanFormatting formatting)
        {
            if (formatting.LongForm == true)
            {
                if (value) Put("#true");
                else Put("#false");
            }
            else
            {
                if (value) Put("#t");
                else Put("#f");
            }
        }

        private void WriteInteger(long n, NumberFormatting formatting)
        {
            switch (formatting.Radix ?? default)
            {
                case NumberRadix.Decimal:
                    Put(n.ToString());
                    return;
                case NumberRadix.PrefixedDecimal:
                    Put("#d");
                    Put(n.ToString());
                    return;
                case NumberRadix.Hexadecimal:
                    Put("#x");
                    Put(Convert.ToString(n, 16));
                    return;
                case NumberRadix.Octal:
                    Put("#o");
                    Put(Convert.ToString(n, 8));
                    return;
                case NumberRadix.Binary:
                    Put("#b");
                    Put(Convert.ToString(n, 2));
                    return;
                default:
                    throw MakeError($"Invalid number radix {formatting.Radix}");
            }
        }

        private void WriteDouble(double d, NumberFormatting formatting)
        {
            var radix = formatting.Radix ?? default;
            if (radix != NumberRadix.Decimal
                && radix != NumberRadix.PrefixedDecimal
                && !double.IsNaN(d) && !double.IsInfinity(d))
            {
                throw MakeError($"Invalid radix {radix} for floating number");
            }

            switch (radix)
            {
                case NumberRadix.PrefixedDecimal:
                    Put("#d");
                    break;
                case NumberRadix.Hexadecimal:
                    Put("#x");
                    break;
                case NumberRadix.Octal:
                    Put("#o");
                    break;
                case NumberRadix.Binary:
                    Put("#b");
                    break;
            }

            if (double.IsNaN(d)) Put("+nan.0");
            else if (double.IsPositiveInfinity(d)) Put("+inf.0");
            else if (double.IsNegativeInfinity(d)) Put("-inf.0");
            else Put(d.ToString());
        }

        private void WriteChar(int ch, CharacterFormatting formatting)
        {
            switch (ch)
            {
                case 0x07:
                    Put(@"#\alarm");
                    return;
                case '\b':
                    Put(@"#\backspace");
                    return;
                case 0x7F:
                    Put(@"#\delete");
                    return;
                case 0x1B:
                    Put(@"#\escape");
                    return;
                case '\f':
                    Put(@"#\formfeed");
                    return;
                case '\n':
                    Put(@"#\linefeed");
                    return;
                case '\0':
                    Put(@"#\null");
                    return;
                case '\r':
                    Put(@"#\return");
                    return;
                case ' ':
                    Put(@"#\space");
                    return;
                case '\t':
                    Put(@"#\tab");
                    return;
                case '\v':
                    Put(@"#\vtab");
                    return;
            }

            var shouldEscape = (formatting.AsciiOnly == true && ch > 0x7e)
                || !IsPrintable(ch);

            if (!shouldEscape)
            {
                Put(@"#\");
                if (ch <= 0xFFFF) Put((char)ch);
                else
                {
                    DispartSurrogateToBuffer(ch, _surrogateBuffer);
                    Put(_surrogateBuffer[0]);
                    Put(_surrogateBuffer[1]);
                }
                return;
            }

            if (formatting.Escaping == EscapingStyle.UStyle)
            {
                if (ch <= 0xFFFF)
                {
                    Put(@"#\u");
                    Put(ch.ToString("x04"));
                }
                else
                {
                    Put(@"#\U");
                    Put(ch.ToString("x06"));
                }
            }
            else
            {
                Put(@"#\x");
                Put(ch.ToString("x"));
            }
        }

        private void WriteSymbol(Symbol value, CharacterFormatting characterFormatting)
        {
            var name = value.Name;
            var shouldEscape = name.Length == 0;

            var asciiOnly = characterFormatting.AsciiOnly ?? false;
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
                Put('|');
                foreach (var scalar in EnumerateScalarValues(name))
                {
                    if ((asciiOnly && (scalar < 0x20 || scalar > 0x7e)) || scalar == '|' || scalar == '\\' || !IsPrintable(scalar))
                    {
                        WriteEscapedCharacter(scalar, true, uStyleEscape);
                    }
                    else if (scalar >= 0x10000)
                    {
                        DispartSurrogateToBuffer(scalar, _surrogateBuffer);
                        Put(_surrogateBuffer[0]);
                        Put(_surrogateBuffer[1]);
                    }
                    else
                    {
                        Put((char)scalar);
                    }
                }
                Put('|');
            }
            else
            {
                Put(name);
            }
        }

        private void WriteTypeIdentifier(TypeIdentifier value, CharacterFormatting characterFormatting)
        {
            Put("#t:");
            WriteSymbol(value._symbol, characterFormatting);
        }

        private void WriteString(string value, CharacterFormatting characterFormatting)
        {
            var asciiOnly = characterFormatting.AsciiOnly ?? false;
            var uStyleEscape = characterFormatting.Escaping == EscapingStyle.UStyle;

            Put('"');
            foreach (var scalar in EnumerateScalarValues(value))
            {
                if ((asciiOnly && (scalar < 0x20 || scalar > 0x7e)) || scalar == '"' || !IsPrintable(scalar))
                {
                    WriteEscapedCharacter(scalar, false, uStyleEscape);
                }
                else if (scalar >= 0x10000)
                {
                    DispartSurrogateToBuffer(scalar, _surrogateBuffer);
                    Put(_surrogateBuffer[0]);
                    Put(_surrogateBuffer[1]);
                }
                else
                {
                    Put((char)scalar);
                }
            }
            Put('"');
        }

        private void WriteByte(byte b, NumberRadix radix)
        {
            switch (radix)
            {
                case NumberRadix.Decimal:
                    Put(b.ToString());
                    break;
                case NumberRadix.PrefixedDecimal:
                    Put("#d");
                    Put(b.ToString());
                    break;
                case NumberRadix.Hexadecimal:
                    Put("#x");
                    Put(Convert.ToString(b, 16));
                    break;
                case NumberRadix.Octal:
                    Put("#o");
                    Put(Convert.ToString(b, 8));
                    break;
                case NumberRadix.Binary:
                    Put("#b");
                    Put(Convert.ToString(b, 2));
                    break;
                default:
                    throw new ArgumentException($"Invalid {nameof(NumberRadix)}: {radix}");
            }
        }

        private void WriteBytes(byte[] value, BytesFormatting bytesFormatting)
        {
            if (bytesFormatting.ByteString == true)
            {
                Put('#');
                Put('"');
                foreach (var b in value)
                {
                    if (b == '"' || (b < 0x20 || b > 0x7e))
                    {
                        WriteEscapedCharacter(b, false, false);
                    }
                    else
                    {
                        Put((char)b);
                    }
                }
                Put('"');
            }
            else
            {
                Put("#u8");
                Put(GetOpeningChar(bytesFormatting.Parentheses ?? default));

                var limit = bytesFormatting.LineLimit ?? 0;
                var radix = bytesFormatting.Radix ?? default;
                var i = 0;
                if (limit <= 0)
                {
                    foreach (var b in value)
                    {
                        if (i != 0) Put(' ');
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
                                PutNewLine();
                                PutIndent(l);
                            }
                            else Put(' ');
                        }
                        WriteByte(b, radix);
                        ++i;
                    }
                }

                Put(GetClosingChar(bytesFormatting.Parentheses ?? default));
            }
        }

        private void WritePair(Pair pair, ListFormatting listFormatting, int depth)
        {
            var l = _linePosition + (listFormatting.LineExtraSpaces ?? 1);

            Put(GetOpeningChar(listFormatting.Parentheses ?? default));

            var i = 0;
            var breakIndex = listFormatting.LineBreakIndex;
            var lineElemsCount = listFormatting.LineElemsCount;
            SValue current = pair;
            while (current.IsPair)
            {
                var currentPair = (Pair)current._value;
                if (i > 0)
                {
                    if (lineElemsCount != null)
                    {
                        if (i % lineElemsCount == 0)
                        {
                            PutNewLine();
                            PutIndent(l);
                        }
                        else
                        {
                            Put(' ');
                        }
                    }
                    else if (breakIndex != null && i >= breakIndex)
                    {
                        PutNewLine();
                        PutIndent(l);
                    }
                    else
                    {
                        Put(' ');
                    }
                }
                WriteValue(currentPair._car, depth: depth + 1);
                current = currentPair._cdr;
                ++i;
            }

            if (!current.IsNull) // improper list
            {
                if (i > breakIndex)
                {
                    PutNewLine();
                    PutIndent(l);
                    Put('.');
                    PutNewLine();
                    PutIndent(l);
                }
                else
                {
                    Put(" . ");
                }
                WriteValue(current, depth: depth + 1);
            }

            Put(GetClosingChar(listFormatting.Parentheses ?? default));
        }

    }
}
