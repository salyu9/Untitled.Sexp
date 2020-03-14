using System;
using System.Collections.Generic;
using System.IO;
using Untitled.Sexp.Formatting;
using static Untitled.Sexp.Utils;

namespace Untitled.Sexp
{
    /// <summary>
    /// Represents a text reader that can read sexp values from text readers.
    /// </summary>
    public sealed class SexpTextReader
    {
        private TextReader _reader;

        private int? _ungotten; // allow one step unget

        private int _prevLineLength;

        private char[] _charBuffer = new char[128];

        private char[] _escapeBuffer = new char[2];

        private byte[] _byteBuffer = new byte[128];

        /// <summary>
        /// Get current line number of the reader.
        /// </summary>
        public int LineNumber { get; private set; } = 1;

        /// <summary>
        /// Get current line position of the reader.
        /// </summary>
        public int LinePosition { get; private set; }

        /// <summary>
        /// Settings of the reader.
        /// </summary>
        public SexpTextReaderSettings Settings { get; set; }

        /// <summary>
        /// Initialize reader with a base text-reader (non-own).
        /// </summary>
        public SexpTextReader(TextReader reader)
        {
            _reader = reader;
            Settings = new SexpTextReaderSettings();
        }

        /// <summary>
        /// Intialize reader with a base text-reader (non-own) and settings.
        /// </summary>
        public SexpTextReader(TextReader reader, SexpTextReaderSettings settings)
        {
            _reader = reader;
            Settings = new SexpTextReaderSettings(settings);
        }

        private SexpReaderException MakeError(string message, Exception? inner = null)
        {
            return inner == null ? new SexpReaderException(message, this) : new SexpReaderException(message, this, inner);
        }

        private int Peek()
        {
            if (_ungotten != null) return _ungotten.Value;
            return _reader.Peek();
        }

        private int GetCh()
        {
            int read;
            if (_ungotten != null)
            {
                read = _ungotten.Value;
                _ungotten = null;
            }
            else read = _reader.Read();
            if (read >= 0)
            {
                if (read == '\r' && Peek() == '\n') // crlf: increase line number at lf.
                {
                }
                else if (IsLineEnd(read))
                {
                    ++LineNumber;
                    _prevLineLength = LinePosition;
                    LinePosition = 0;
                }
                else if (!IsHighSurrogate(read)) // only increase line position after full surrogate pair
                {
                    ++LinePosition;
                }
            }
            return read;
        }

        private void UngetCh(int ch)
        {
            System.Diagnostics.Debug.Assert(_ungotten == null, "Cannot do multiple ungets");
            System.Diagnostics.Debug.Assert(LineNumber > 0 && LinePosition >= 0, "Cannot do unget before get any char");
            _ungotten = ch;

            // crlf / surrogate already handled by GetCh.
            if (IsLineEnd(ch))
            {
                LinePosition = _prevLineLength;
                --LineNumber;
            }
            else if (ch >= 0)
            {
                --LinePosition;
                System.Diagnostics.Debug.Assert(LinePosition >= 0);
            }
        }

        /// <summary>
        /// Read escaped char into _escapeBuffer, return char count read.
        /// </summary>
        /// <param name="ch">The first escaping char.</param>
        /// <param name="isSymbol">Allow escaped whitespace and '|' when reading symbols.</param>
        /// <returns>How many chars read into _escapeBuffer.</returns>
        private int ReadEscapedChar(char ch, bool isSymbol)
        {
            var isX = ch == 'x' || ch == 'X';
            var isU = ch == 'u' || ch == 'U';
            if (isX || isU)
            {
                if (isU && !Settings.AllowUInEscaping) throw MakeError($"\\{ch} is not allowed");
                var max = ch == 'u' ? 4 : 8;
                int n = 0;
                for (int i = 0; i < max; ++i)
                {
                    int next = Peek();
                    if (next > 0 && IsHexDigit(next))
                    {
                        GetCh();
                        n = (n << 4) | HexDigitToInt(next);
                    }
                    else break;
                }
                if (n < 0 || IsSurrogate(n) || n > 0x10FFFF) throw MakeError($"Invalid unicode scalar value: {n:X}");
                if (isX)
                {
                    var ending = GetCh();
                    if (ending != ';') throw MakeError($"Expecting ';' for \\x ending, found {ending}");
                }
                if (n >= 0x10000)
                {
                    int high, low;
                    DispartSurrogate(n, out high, out low);
                    _escapeBuffer[0] = (char)high;
                    _escapeBuffer[1] = (char)low;
                    return 2;
                }
                else
                {
                    _escapeBuffer[0] = (char)n;
                    return 1;
                }
            }
            if (ch == '\r' && Peek() == '\n')
            {
                GetCh();
                _escapeBuffer[0] = '\r';
                _escapeBuffer[1] = '\n';
                return 2;
            }
            switch (ch)
            {
                case 'a':
                    _escapeBuffer[0] = '\x70'; // alarm
                    break;
                case 'b':
                    _escapeBuffer[0] = '\b';
                    break;
                case 'e':
                    _escapeBuffer[0] = '\x1B'; // escape
                    break;
                case 'f':
                    _escapeBuffer[0] = '\f';
                    break;
                case 'n':
                    _escapeBuffer[0] = '\n';
                    break;
                case 'r':
                    _escapeBuffer[0] = '\r';
                    break;
                case 't':
                    _escapeBuffer[0] = '\t';
                    break;
                case 'v':
                    _escapeBuffer[0] = '\v';
                    break;
                case '\"':
                    _escapeBuffer[0] = '\"';
                    break;
                case '\'':
                    _escapeBuffer[0] = '\'';
                    break;
                case '\\':
                    _escapeBuffer[0] = '\\';
                    break;
                case '\r':
                    _escapeBuffer[0] = '\r';
                    break;
                case '\n':
                    _escapeBuffer[0] = '\n';
                    break;
                default:
                    if (isSymbol && (ch == '|' || char.IsWhiteSpace((char)ch))) // ch <= 0xFFFF
                    {
                        _escapeBuffer[0] = (char)ch;
                        break;
                    }
                    throw MakeError($"Unknown escape {(char)ch}");
            }
            return 1;
        }

        /// <summary>
        /// Read the next sexp value.
        /// </summary>
        public SValue Read()
        {
            return ReadValue();
        }

        /// <summary>
        /// Read all rest sexp values.
        /// </summary>
        public IEnumerable<SValue> ReadAll()
        {
            while (true)
            {
                var read = ReadValue();
                if (read.IsEof) break;
                yield return read;
            }
        }

        private SValue ReadValue(int recursiveDepth = 0)
        {
            int ch;
            while (true)
            {
                // skip whitespace
                while (true)
                {
                    ch = GetCh();
                    if (ch < 0) return SValue.Eof;
                    if (!char.IsWhiteSpace((char)ch)) break;
                }

                switch (ch)
                {
                    case ')':
                    case ']':
                    case '}':
                        throw MakeError($"Unexpected closer: {(char)ch}");

                    case '(':
                        return ReadList(')', ParentheseType.Parenthese, recursiveDepth);

                    case '[':
                        if (!Settings.AllowBracket) throw MakeError("[] is not allowed");
                        return ReadList(']', ParentheseType.Bracket, recursiveDepth);

                    case '{':
                        if (!Settings.AllowBrace) throw MakeError("{} is not allowed");
                        return ReadList('}', ParentheseType.Brace, recursiveDepth);

                    case '|':
                        UngetCh(ch);
                        return ReadNumberOrSymbol();

                    case '"':
                        return ReadString('"', isBytes: false);

                    case '\'':
                        throw MakeError("quoting is not supported yet");

                    case '`':
                        throw MakeError("quasiquoting is not supported yet");

                    case ',':
                        throw MakeError("unquoting is not supported yet");

                    case ';':
                        {
                            while (!IsLineEnd(ch = GetCh()))
                            {
                                if (ch < 0) return SValue.Eof;
                            }
                        }
                        continue;

                    case '#':
                        {
                            ch = GetCh();
                            if (ch < 0) throw MakeError("Reached end after #");

                            switch (ch)
                            {
                                case '|': // "#|"
                                    {
                                        int depth = 1;
                                        int prevCh = 0;
                                        do
                                        {
                                            ch = GetCh();
                                            if (ch < 0) throw MakeError("Reached end in #| comment");
                                            if (prevCh == '|' && ch == '#')
                                            {
                                                --depth;
                                                if (depth == 0) break;
                                                ch = 0;
                                            }
                                            else if (prevCh == '#' && ch == '|')
                                            {
                                                ++depth;
                                                ch = 0;
                                            }
                                            prevCh = ch;
                                        } while (true);
                                    }
                                    continue;

                                case ';': // "#;"
                                    if (ReadValue().IsEof) throw MakeError("Expect skipped value after #;");
                                    continue;

                                case 't':
                                case 'T':
                                    if (IsDelimiter(Peek())) return SValue.True;
                                    return ReadExpecting("true", SValue.True);

                                case 'f':
                                case 'F':
                                    if (IsDelimiter(Peek())) return SValue.False;
                                    return ReadExpecting("false", SValue.False);

                                case '\\':
                                    return ReadChar();

                                case 'x':
                                case 'X':
                                    return ReadNumber(16, NumberRadix.Hexadecimal);

                                case 'd':
                                case 'D':
                                    return ReadNumber(10, NumberRadix.PrefixedDecimal);

                                case 'o':
                                case 'O':
                                    return ReadNumber(8, NumberRadix.Octal);

                                case 'b':
                                case 'B':
                                    return ReadNumber(2, NumberRadix.Binary);

                                case '"':
                                    if (!Settings.AllowRacketStyleByteString) throw MakeError("#\"\" bytes is not allowed");
                                    return ReadString('"', isBytes: true);

                                case 'u':
                                case 'U':
                                    {
                                        ch = GetCh();
                                        if (ch == '8')
                                        {
                                            if (!Settings.AllowR7rsStyleByteVector) throw MakeError("#u8() bytes is not allowed");
                                            ch = GetCh(); // open
                                            switch (ch)
                                            {
                                                case '(':
                                                    return ReadByteVector(')', ParentheseType.Parenthese);
                                                case '[':
                                                    if (!Settings.AllowBracket) throw MakeError("[] is not allowed");
                                                    return ReadByteVector(']', ParentheseType.Bracket);
                                                case '{':
                                                    if (!Settings.AllowBrace) throw MakeError("{} is not allowed");
                                                    return ReadByteVector('}', ParentheseType.Brace);
                                            }
                                        }
                                        throw MakeError($"Unknown datum #u{(char)ch}");
                                    }

                                default:
                                    throw MakeError($"Unknown datum #{(char)ch}");
                            }


                        } // case '#'

                    default:
                        UngetCh(ch);
                        return ReadNumberOrSymbol();
                }
            }
        }

        private SValue ReadExpecting(string expecting, SValue result)
        {
            for (int i = 1; i < expecting.Length; ++i)
            {
                var ch = (char)GetCh();
                if (ToLower(ch) != expecting[i]) throw MakeError($"Expecting \"{expecting}\", found 'ch' at position {i}");
            }
            if (!IsDelimiter(Peek())) throw MakeError($"Expecting delimiter after {expecting}");
            return result;
        }

        private void SkipWhitespaceAndComments()
        {
            int ch;
            while (true)
            {
                do { ch = GetCh(); } while (ch >= 0 && char.IsWhiteSpace((char)ch));
                if (ch == ';') // single line comment
                {
                    do
                    {
                        ch = GetCh();
                    } while (!IsLineEnd(ch));
                }
                else if (ch == '#')
                {
                    int next = Peek();
                    if (next == '|') // block
                    {
                        GetCh(); // eat '|'
                        int depth = 1;
                        int prevCh = 0;
                        do
                        {
                            ch = GetCh();
                            if (ch < 0) throw MakeError("Reached end in #| comment");
                            if (prevCh == '|' && ch == '#')
                            {
                                --depth;
                                if (depth == 0) break;
                                ch = 0;
                            }
                            else if (prevCh == '#' && ch == '|')
                            {
                                ++depth;
                                ch = 0;
                            }
                            prevCh = ch;
                        } while (true);
                    }
                    else if (next == ';')
                    {
                        GetCh(); // eat ';'
                        if (ReadValue().IsEof) throw MakeError("Expect skipped value after #;"); // skipped value
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }

            UngetCh(ch);
        }

        /// <summary>
        /// Read a symbol or number. <br />
        /// The number is not prefixed with '#', 
        /// </summary>
        private SValue ReadNumberOrSymbol()
        {
            var ch = GetCh();

            var multipleEscaping = false;
            var isSymbol = false;

            var buffer = _charBuffer;
            var i = 0;

            while (ch >= 0 && (multipleEscaping || (!char.IsWhiteSpace((char)ch) && !IsDelimiter(ch))))
            {
                if (ch == '\\') // single escape
                {
                    ch = GetCh();
                    if (ch < 0) throw MakeError("Reached end in symbol escaping");
                    var count = ReadEscapedChar((char)ch, isSymbol: true);
                    if (i + count >= buffer.Length) // extend buffer, assuming i + count < buffer.Length * 2
                    {
                        DoubleBuffer(ref buffer);
                        System.Diagnostics.Debug.Assert(i + count <= buffer.Length);
                    }
                    for (var k = 0; k < count; ++k)
                    {
                        buffer[i++] = _escapeBuffer[k];
                    }
                    isSymbol = true;
                }
                else if (ch == '|') // multiple escape
                {
                    multipleEscaping = !multipleEscaping;
                    isSymbol = true;
                }
                else
                {
                    if (i >= buffer.Length) DoubleBuffer(ref buffer);
                    buffer[i++] = (char)ch;
                }
                ch = GetCh();
            }
            if (multipleEscaping) throw MakeError($"Unbalanced | escaping");
            if (ch >= 0) UngetCh(ch);

            var str = new string(buffer, 0, i);

            if (isSymbol) // if contains escaping, must be symbol
            {
                return new SValue(SSymbol.FromString(str));
            }

            if (i == 6)
            {
                if (Utils.StringEquals(str, "+nan.0")) return new SValue(double.NaN);
                if (Utils.StringEquals(str, "+inf.0")) return new SValue(double.PositiveInfinity);
                if (Utils.StringEquals(str, "-nan.0")) return new SValue(-double.NaN);
                if (Utils.StringEquals(str, "-inf.0")) return new SValue(double.NegativeInfinity);
            }

            if (long.TryParse(str, out var longResult))
            {
                if (longResult > int.MaxValue || longResult < int.MinValue) return new SValue(longResult);
                return new SValue((int)longResult);
            }
            if (double.TryParse(str, out var doubleResult)) return new SValue(doubleResult);

            return new SValue(SSymbol.FromString(str));
        }

        private SValue ReadNumber(int radix, NumberRadix radixEnum)
        {
            int ch;
            var buffer = _charBuffer;
            int i = 0;
            while (!IsDelimiter(ch = GetCh()))
            {
                if (i == buffer.Length) DoubleBuffer(ref buffer);
                buffer[i++] = (char)ch;
            }
            var str = new string(buffer, 0, i);
            if (i == 6)
            {
                if (Utils.StringEquals(str, "+nan.0")) return new SValue(double.NaN);
                if (Utils.StringEquals(str, "+inf.0")) return new SValue(double.PositiveInfinity);
                if (Utils.StringEquals(str, "-nan.0")) return new SValue(-double.NaN);
                if (Utils.StringEquals(str, "-inf.0")) return new SValue(double.NegativeInfinity);
            }
            if (radix == 10)
            {
                if (long.TryParse(str, out var longResult))
                {
                    if (longResult > int.MaxValue || longResult < int.MinValue) return new SValue(longResult);
                    return new SValue((int)longResult);
                }
                if (double.TryParse(str, out var doubleResult)) return new SValue(doubleResult);
                throw MakeError($"Cannot parse number {str}");
            }
            else
            {
                try
                {
                    var n = Convert.ToInt64(str, radix);
                    if (n > int.MaxValue || n < int.MinValue) return new SValue(n);
                    return new SValue((int)n);
                }
                catch (Exception exn)
                {
                    throw MakeError($"Cannot parse number {str}" + exn.Message, exn);
                }
            }
        }

        private static SValue MakeCharUnchecked(int ch)
        {
            return new SValue(ch, SValueType.Char);
        }

        private SValue ReadChar()
        {
            var ch = GetCh();

            if (ch < 0) throw MakeError("Reached end in character");

            if (IsLowSurrogate(ch)) throw MakeError($"Invalid low-surrogate U+{ch:X04}");

            var next = Peek();

            if (IsHighSurrogate(ch))
            {
                if (!IsLowSurrogate(next)) throw MakeError($"Expecting low-surrogate after high-surrogate U+{ch:X04}, but found U+{next:X04}");
                GetCh();
                ch = MergeSurrogate(ch, next);
                // surrogate pair cannot exceed 0x10FFFF, no need to check.
            }
            else if (((ch == 'x' || ch == 'X') && IsHexDigit(next))
                || (ch == 'u' || ch == 'U') && IsHexDigit(next))
            {
                if ((ch == 'u' || ch == 'U') && !Settings.AllowUInCharacter)
                {
                    throw MakeError($"#\\u is not allowed");
                }
                int n = 0;
                int max = ch == 'u' ? 4 : 8;
                for (int i = 0; i < max; ++i)
                {
                    next = Peek();
                    if (next > 0 && IsHexDigit(next))
                    {
                        GetCh();
                        n = (n << 4) | HexDigitToInt(next);
                    }
                    else break;
                }
                if (n < 0 || IsSurrogate(n) || n > 0x10FFFF) throw MakeError($"Invalid unicode scalar value: {n:X}");
                ch = n;
            }
            else if (IsAlphabet(ch) && IsAlphabet(next))
            {
                _charBuffer[0] = ToLower((char)ch);
                int i = 1;
                for (; i < _charBuffer.Length; ++i)
                {
                    ch = Peek();
                    if (IsAlphabet(ch))
                    {
                        GetCh();
                        _charBuffer[i] = ToLower((char)ch);
                    }
                    else break;
                }

                switch (_charBuffer[0])
                {
                    case 'a':
                        if (Matches(_charBuffer, "alarm")) return MakeCharUnchecked(0x07);
                        break;
                    case 'b':
                        if (Matches(_charBuffer, "backspace")) return MakeCharUnchecked('\b');
                        break;
                    case 'd':
                        if (Matches(_charBuffer, "delete")) return MakeCharUnchecked(0x7F);
                        break;
                    case 'e':
                        if (Matches(_charBuffer, "escape")) return MakeCharUnchecked(0x1B);
                        break;
                    case 'f':
                        if (Matches(_charBuffer, "formfeed")) return MakeCharUnchecked('\f');
                        break;
                    case 'l':
                        if (Matches(_charBuffer, "linefeed")) return MakeCharUnchecked('\n');
                        break;
                    case 'n':
                        if (Matches(_charBuffer, "newline")) return MakeCharUnchecked('\n');
                        if (Matches(_charBuffer, "null") || Matches(_charBuffer, "nul")) return MakeCharUnchecked('\0');
                        break;
                    case 'p':
                        if (Matches(_charBuffer, "page")) return MakeCharUnchecked('\f');
                        break;
                    case 'r':
                        if (Matches(_charBuffer, "return")) return MakeCharUnchecked('\r');
                        if (Matches(_charBuffer, "rubout")) return MakeCharUnchecked(0x7F);
                        break;
                    case 's':
                        if (Matches(_charBuffer, "space")) return MakeCharUnchecked(' ');
                        break;
                    case 't':
                        if (Matches(_charBuffer, "tab")) return MakeCharUnchecked('\t');
                        break;
                    case 'v':
                        if (Matches(_charBuffer, "vtab")) return MakeCharUnchecked('\v');
                        break;
                }

                throw MakeError($"Unknown character name: {new string(_charBuffer, 0, i)}");
            }

            return MakeCharUnchecked(ch);
        }

        private SValue ReadString(int endCh, bool isBytes)
        {
            var buffer = _charBuffer;
            int i = 0;
            while (true)
            {
                var ch = GetCh();
                if (ch == endCh) break;
                if (ch < 0) throw MakeError("Reached end in string, expecting closing '\"'");

                if (ch == '\\')
                {
                    ch = GetCh();
                    if (ch < 0) throw MakeError("Reached end in string escaping");
                    var count = ReadEscapedChar((char)ch, isSymbol: false);
                    if (i + count >= buffer.Length) // extend buffer, assuming i + count < buffer.Length * 2
                    {
                        DoubleBuffer(ref buffer);
                        System.Diagnostics.Debug.Assert(i + count <= buffer.Length);
                    }
                    for (var k = 0; k < count; ++k)
                    {
                        buffer[i++] = _escapeBuffer[k];
                    }
                }
                else
                {
                    if (i >= buffer.Length) DoubleBuffer(ref buffer);
                    buffer[i++] = (char)ch;
                }
            }

            if (isBytes)
            {
                var bytes = new byte[i];
                for (int j = 0; j < i; ++j)
                {
                    if (buffer[j] > 0xFF) throw MakeError($"Byte out of range: {(int)buffer[j]:04X}");
                    bytes[j] = (byte)buffer[j];
                }
                return new SValue(bytes, SValueType.Bytes, new BytesFormatting{ ByteString = true });
            }

            return new SValue(new string(buffer, 0, i));
        }

        private byte ReadByteVectorElem()
        {
            int max;
            int radix;
            if (Peek() == '#')
            {
                GetCh();
                var radixCh = GetCh();
                switch (radixCh)
                {
                    case 'x':
                    case 'X':
                        max = 2;
                        radix = 16;
                        break;
                    case 'd':
                    case 'D':
                        max = 3;
                        radix = 10;
                        break;
                    case 'o':
                    case 'O':
                        max = 3;
                        radix = 8;
                        break;
                    case 'b':
                    case 'B':
                        max = 8;
                        radix = 2;
                        break;
                    default:
                        throw MakeError($"Unknown radix {radixCh}");
                }
            }
            else
            {
                max = 3;
                radix = 10;
            }
            int i = 0;
            int ch = 0;
            while (i <= max)
            {
                ch = GetCh();
                if (IsDelimiter(ch)) break;
                _charBuffer[i++] = (char)ch;
            }
            if (!IsDelimiter(ch)) throw MakeError("Byte vector elem too long");
            UngetCh(ch);
            var str = new string(_charBuffer, 0, i);
            int n;
            try
            {
                n = Convert.ToInt32(str, radix);
            }
            catch (Exception exn)
            {
                throw MakeError($"Error parsing byte vector elem {_charBuffer}", exn);
            }
            if (n < 0 || n > 0xFF) throw MakeError($"Byte vector elem out of range: {n}");
            return (byte)n;
        }

        private SValue ReadByteVector(int endCh, ParentheseType parenthese)
        {
            var buffer = _byteBuffer;
            int i = 0;
            while (true)
            {
                SkipWhitespaceAndComments();
                var ch = Peek();
                if (ch == endCh)
                {
                    GetCh();
                    break;
                }
                if (ch < 0) throw MakeError("Reached end in byte vector");
                if (ch == ')' || ch == ']' || ch == '}') throw MakeError($"Expecting closing {(char)endCh}, found {(char)ch}");

                var b = ReadByteVectorElem();
                if (i >= buffer.Length) DoubleBuffer(ref buffer);
                buffer[i++] = b;
            }
            if (buffer.Length != i)
            {
                var newBuffer = new byte[i];
                Array.Copy(buffer, newBuffer, i);
                buffer = newBuffer;
            }
            return new SValue(buffer, SValueType.Bytes, new BytesFormatting{ Parenthese = parenthese });
        }
        
        private SValue ReadList(int endCh, ParentheseType parenthese, int recursiveDepth = 0)
        {
            SkipWhitespaceAndComments();
            SPair? result = null;
            SPair? current = null;
            while (true)
            {
                var ch = Peek();
                if (ch < 0) throw MakeError($"Reached end in list, expecting closing {(char)endCh}");
                if (ch == endCh)
                {
                    GetCh();
                    if (result == null) return SValue.Null;
                    return new SValue(result, SValueType.Pair, new ListFormatting{ Parenthese = parenthese });
                }
                if (ch == ')' || ch == ']' || ch == '}') throw MakeError($"Expecting closing {(char)endCh}, found {(char)ch} instead");

                var car = ReadValue(recursiveDepth + 1);
                var pair = new SPair(car, SValue.Null);

                if (result == null)
                {
                    result = pair;
                    current = pair;
                }
                else
                {
                    System.Diagnostics.Debug.Assert(current != null);
                    current!._cdr = new SValue(pair);
                    current = pair;
                }

                SkipWhitespaceAndComments();
                if (Peek() == '.')
                {
                    GetCh();
                    if (!IsDelimiter(Peek()))
                    {
                        UngetCh('.');
                        continue;
                    }
                    if (Peek() < 0) throw MakeError($"Reached end in pair, expecting value");
                    var cdr = ReadValue(recursiveDepth + 1);
                    SkipWhitespaceAndComments();
                    ch = Peek();
                    if (ch != endCh) throw MakeError($"Expecting closing {(char)endCh}, found {(char)ch}");
                    pair._cdr = cdr;
                }
            }
        }

        /// <summary>
        /// For debug.
        /// </summary>
        public string DebugGetCurrent()
        {
            return $"Position: {LineNumber}, {LinePosition}: {Peek()}";
        }
    }
}
