using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Untitled.Sexp
{
    internal static class Utils
    {
        public static bool IsLineEnd(int ch)
            => ch == '\r'
                || ch == '\n'
                || ch == 0x85 // NextLine
                || ch == 0x2028 // LineSeparator
                || ch == 0x2029 // ParagraphSeparator
                ;

        public static bool IsWhiteSpace(int ch)
            => ch <= 0xFFFF && char.IsWhiteSpace((char)ch); // ignore whitespaces larger than 0xFFFF

        public static bool IsHighSurrogate(int ch)
            => ch >= 0xD800 && ch <= 0xDBFF;

        public static bool IsLowSurrogate(int ch)
            => ch >= 0xDC00 && ch <= 0xDFFF;

        public static bool IsSurrogate(int ch)
            => ch >= 0xD800 && ch <= 0xDFFF;

        public static int MergeSurrogate(int high, int low)
        {
            System.Diagnostics.Debug.Assert(IsHighSurrogate(high));
            System.Diagnostics.Debug.Assert(IsLowSurrogate(low));
            return ((high - 0xD800) << 10) + (low - 0xDC00) + 0x10000;
        }

        public static void DispartSurrogate(int point, out int high, out int low)
        {
            System.Diagnostics.Debug.Assert(point >= 0x10000 && point <= 0x10FFFF);
            point -= 0x10000;
            high = (point >> 10) + 0xD800;
            low = (point & 0x03FF) + 0xDC00;
        }

        public static void DispartSurrogateToBuffer(int point, char[] buffer)
        {
            System.Diagnostics.Debug.Assert(point >= 0x10000 && point <= 0x10FFFF);
            point -= 0x10000;
            buffer[0] = (char)((point >> 10) + 0xD800);
            buffer[1] = (char)((point & 0x03FF) + 0xDC00);
        }

        public static bool IsDelimiter(int ch)
            => ch < 0
                || ch == ' '
                || ch == '(' || ch == '[' || ch == '{'
                || ch == ')' || ch == ']' || ch == '}'
                || ch == '"' || ch == '\'' || ch == ';' || ch == ','
                || (ch <= 0xFF && char.IsWhiteSpace((char)ch));

        public static bool IsDigit(int ch)
            => ch >= '0' && ch <= '9';

        public static int DigitToInt(int ch)
            => ch - '0';

        public static bool IsHexDigit(int ch)
            => (ch >= '0' && ch <= '9')
                || (ch >= 'a' && ch <= 'f')
                || (ch >= 'A' && ch <= 'F');

        public static int HexDigitToInt(int next)
            => next <= '9' ? next - '0' : (next <= 'A' ? next - 'A' : next - 'a') + 10;

        public static bool IsAlphabet(int ch)
            => (ch >= 'a' && ch <= 'z')
                || (ch >= 'A' && ch <= 'Z');

        public static char ToLower(char ch)
            => ch >= 'A' && ch <= 'Z' ? (char)(ch - 'A' + 'a') : ch;

        public static char ToUpper(char ch)
            => ch >= 'a' && ch <= 'z' ? (char)(ch - 'a' + 'A') : ch;

        public static bool Matches(char[] chars, string expected)
        {
            for (int i = 0; i < expected.Length; ++i)
            {
                if (chars[i] != expected[i]) return false;
            }
            return true;
        }

        public static IEnumerable<int> EnumerateScalarValues(string s)
        {
            int i = 0;
            while (i < s.Length)
            {
                var ch = s[i];
                if (IsLowSurrogate(ch))
                {
                    throw new ArgumentException($"string contains invalid low surrogate U+{(int)ch:X04} at position {i}");
                }
                if (IsHighSurrogate(ch))
                {
                    if (i == s.Length)
                    {
                        throw new ArgumentException($"string contains high surrogate U+{(int)ch:X04} at the end ");
                    }
                    ++i;
                    // At now surrogates cannot represent code point larger than 0x10FFFF, no need to check.
                    yield return MergeSurrogate(ch, s[i]);
                }
                else
                {
                    yield return ch;
                }
                ++i;
            }
        }

        public static int CompareString(string a, string b)
            => string.CompareOrdinal(a, b);

        public static bool StringEquals(string a, string b)
            => string.Equals(a, b, StringComparison.Ordinal);

        public static void DoubleBuffer<T>(ref T[] buffer)
        {
            var newBuffer = new T[buffer.Length * 2];
            Array.Copy(buffer, newBuffer, buffer.Length);
            buffer = newBuffer;
        }

        private static IEnumerable<int> ForwardRange(int start, int end, int step)
        {
            while (start < end)
            {
                yield return start;
                start += step;
            }
        }

        private static IEnumerable<int> BackwardRange(int start, int end, int step)
        {
            while (start > end)
            {
                yield return start;
                start += step;
            }
        }

        public static IEnumerable<int> Range(int start, int end, int step = 1)
        {
            if (step == 0) throw new ArgumentException($"Range step should not be zero");
            return step > 0 ? ForwardRange(start, end, step) : BackwardRange(start, end, step);
        }

        public static IEnumerable<int> Range(int length)
            => Range(0, length);

        private static string[] ByteHexTable = Range(0, 0x100).Select(n => n.ToString("X02")).ToArray();

        public static string ToHex(this byte b)
            => ByteHexTable[b];

        public static string ToHex(this char c)
            => ByteHexTable[c >> 8] + ByteHexTable[c & 0xFF];

        public static string ToHex(this short s)
            => ByteHexTable[s >> 8] + ByteHexTable[s & 0xFF];

        public static string ToHex(this int s)
            => ByteHexTable[s >> 24] + ByteHexTable[(s >> 16) & 0xFF] + ByteHexTable[(s >> 8) & 0xFF] + ByteHexTable[s & 0xFF];

    }
}
