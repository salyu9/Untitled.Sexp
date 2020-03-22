using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using static Untitled.Sexp.SValue;

namespace Untitled.Sexp.Tests
{
    public class SexpReaderTest
    {
        private static readonly SexpTextReaderSettings DefaultSettings = new SexpTextReaderSettings();

        private static readonly SexpTextReaderSettings ForbidAllSettings = new SexpTextReaderSettings
        {
            AllowBracket = false,
            AllowBrace = false,
            AllowR7rsStyleByteVector = false,
            AllowRacketStyleByteString = false,
            AllowUInCharacter = false,
            AllowUInEscaping = false,
        };

        private static SValue Read(string value, SexpTextReaderSettings? settings = null, bool singleSexp = true)
        {
            using var stringReader = new StringReader(value);
            var reader = new SexpTextReader(stringReader, settings ?? DefaultSettings);
            var sexp = reader.Read();
            
            if (singleSexp)
            {
                var remain = reader.Read();
                if (!remain.IsEof) throw new SexpReaderException($"Didn't read out: {remain}", reader);
            }

            return sexp;
        }

        [Fact]
        public void ReadComment()
        {
            Assert.Equal(True, Read("; #f \r#t"));
            Assert.Equal(True, Read("#| #f |##t"));
            Assert.Equal(True, Read("#| #| #f |# |##t"));
            Assert.Throws<SexpReaderException>(() => Read("#|  "));
            Assert.Throws<SexpReaderException>(() => Read("#| #| |# "));
            Assert.Equal(True, Read("#; #f #t"));
            Assert.Throws<SexpReaderException>(() => Read("#;"));
        }

        [Fact]
        public void ReadNull()
        {
            Assert.Equal(SValue.Null, Read("null"));
            Assert.Equal(SValue.Null, Read("nil"));

            Assert.Equal(Symbol.FromString("null"), Read("null", new SexpTextReaderSettings { AcceptNull = false }));
            Assert.Equal(Symbol.FromString("nil"), Read("nil", new SexpTextReaderSettings { AcceptNil = false }));
        }

        [Fact]
        public void ReadBoolean()
        {
            Assert.Equal(True, Read(@"#t"));
            Assert.Equal(True, Read(@"#true"));
            Assert.Equal(False, Read(@"#f"));
            Assert.Equal(False, Read(@"#false"));
            Assert.Throws<SexpReaderException>(() => Read("#t1"));
            Assert.Throws<SexpReaderException>(() => Read("#true1"));
            Assert.Throws<SexpReaderException>(() => Read("#f1"));
            Assert.Throws<SexpReaderException>(() => Read("#false1"));
        }

        [Fact]
        public void ReadChar()
        {
            Assert.Equal(new SValue('a'), Read(@"#\a"));
            Assert.Equal(new SValue('\a'), Read(@"#\alarm"));
            Assert.Equal(new SValue('\x04'), Read(@"#\x04"));
            Assert.Throws<SexpReaderException>(() => Read(@"#\xD801"));
            Assert.Throws<SexpReaderException>(() => Read(@"#\x110000"));
            Assert.Equal(new SValue('\u0004'), Read(@"#\u0004"));
            Assert.Throws<SexpReaderException>(() => Read(@"#\u0004", ForbidAllSettings));
            Assert.Equal(new SValue('\0'), Read(@"#\u00004", singleSexp: false));
            Assert.Equal(Char(0x2A6A5), Read(@"#\U02A6A5"));
            Assert.Throws<SexpReaderException>(() => Read(@"#\U02A6A5", ForbidAllSettings));
            Assert.Equal(Char(0x00002A6A), Read(@"#\U00002A6A5", singleSexp: false));
            Assert.Equal(new SValue(' '), Read(@"#\ "));
        }

        [Fact]
        public void ReadNumberOrSymbol()
        {
            Assert.Equal(Symbol("..."), Read("..."));
            Assert.Equal(Symbol("+"), Read("+"));
            Assert.Equal(Symbol("+soup+"), Read("+soup+"));
            Assert.Equal(Symbol("<=?"), Read("<=?"));
            Assert.Equal(Symbol("->string"), Read("->string"));
            Assert.Equal(Symbol("two words"), Read("|two words|"));
            Assert.Equal(Symbol("two words"), Read(@"two\ words"));
            Assert.Equal(Symbol(@"two\ words"), Read(@"|two\\ words|"));
            Assert.Throws<SexpReaderException>(() => Read("|two words"));
            Assert.Equal(Symbol("two words"), Read(@"|two\x20;words|"));
            Assert.Equal(Symbol("two|words"), Read(@"|two\|words|"));
            Assert.Throws<SexpReaderException>(() => Read(@"\x20"));
            Assert.Throws<SexpReaderException>(() => Read(@"\x20k"));
            Assert.Equal(Symbol("two words"), Read(@"|two\u20words|"));
            Assert.Throws<SexpReaderException>(() => Read(@"|two\u20words|", ForbidAllSettings));
            Assert.Equal(Symbol("two words"), Read(@"|two\U0020words|"));
            Assert.Throws<SexpReaderException>(() => Read(@"|two\U0020words|", ForbidAllSettings));
            Assert.Equal(Symbol("the-word-recursion-has-many-meanings"), Read("the-word-recursion-has-many-meanings"));
            Assert.Equal(new SValue(123), Read("123"));
            Assert.Equal(new SValue(-123), Read("-123"));
            Assert.Equal(new SValue(0x7FFFFFFF), Read("2147483647"));
            Assert.Equal(new SValue(0x80000000L), Read("2147483648"));
            Assert.Equal(new SValue(4.0), Read("4.0"));
            Assert.Equal(new SValue(-4.0), Read("-4.0"));
            Assert.Equal(new SValue(2e5), Read("2e5"));
            Assert.Equal(new SValue(2.1e5), Read("2.1e5"));
            Assert.Equal(new SValue(double.NaN), Read("+nan.0"));
            Assert.Equal(new SValue(-double.NaN), Read("-nan.0"));
            Assert.Equal(new SValue(double.PositiveInfinity), Read("+inf.0"));
            Assert.Equal(new SValue(double.NegativeInfinity), Read("-inf.0"));
        }

        [Fact]
        public void ReadNumber()
        {
            Assert.Equal(new SValue(0x1AF), Read("#x1AF"));
            Assert.Equal(new SValue(0x7FFFFFFF), Read("#x7FFFFFFF"));
            Assert.Equal(new SValue(0x80000000L), Read("#x80000000"));
            Assert.Equal(new SValue(5214), Read("#d5214"));
            Assert.Equal(new SValue(0xF56), Read("#o7526"));
            Assert.Equal(new SValue(0b10101011), Read("#b10101011"));
            Assert.Throws<SexpReaderException>(() => Read("#xtaes"));
            Assert.Throws<SexpReaderException>(() => Read("#d45AF"));
            Assert.Throws<SexpReaderException>(() => Read("#o5478"));
            Assert.Throws<SexpReaderException>(() => Read("#b12012"));
            Assert.Equal(new SValue(double.NaN), Read("#x+nan.0"));
            Assert.Equal(new SValue(-double.NaN), Read("#o-nan.0"));
            Assert.Equal(new SValue(double.PositiveInfinity), Read("#d+inf.0"));
            Assert.Equal(new SValue(double.NegativeInfinity), Read("#b-inf.0"));
        }

        [Fact]
        public void ReadList()
        {
            Assert.Equal(Null, Read("()"));
            Assert.Equal(List(new SValue(1)), Read("(1)"));
            Assert.Equal(List(True), Read("[#t]"));
            Assert.Equal(List(new SValue('\n')), Read(@"{#\newline}"));
            Assert.Throws<SexpReaderException>(() => Read("[1]", ForbidAllSettings));
            Assert.Throws<SexpReaderException>(() => Read("{1}", ForbidAllSettings));
            Assert.Throws<SexpReaderException>(() => Read("(1]"));
            Assert.Equal(List(List(new SValue(1))), Read("((1))"));

            Assert.Equal(new SValue(new SValue(1), new SValue(2)), Read("(1 . 2)"));
            Assert.Equal(List(new SValue(1), new SValue(0.2)), Read("(1 .2)"));
            Assert.Equal(List(new SValue(1.0), new SValue(2)), Read("(1. 2)"));
            Assert.Equal(new SValue(new SValue(1), new SValue(new SValue(2), new SValue(3))), Read("(1 2 . 3)"));
            Assert.Throws<SexpReaderException>(() => Read("(1 . 2 . 3)"));
            Assert.Throws<SexpReaderException>(() => Read("(1 2 . )"));

            Assert.Equal(List(1, 2, List(3, 4), List(5)), Read("{1 2 [3 4] (5)}"));

        }

        [Fact]
        public void ReadString()
        {
            Assert.Equal(new SValue(""), Read("\"\""));
            Assert.Equal(new SValue("abc"), Read("\"abc\""));
            Assert.Equal(new SValue("\x20"), Read("\"\\x20;\""));
            Assert.Throws<SexpReaderException>(() => Read("\"\\x20\""));
            Assert.Equal(new SValue("\x20"), Read("\"\\u20\""));
            Assert.Equal(new SValue("\x20"), Read("\"\\U000020\""));
            Assert.Throws<SexpReaderException>(() => Read("\"\\u20\"", ForbidAllSettings));
            Assert.Throws<SexpReaderException>(() => Read("\"\\U000020\"", ForbidAllSettings));
            Assert.Throws<SexpReaderException>(() => Read("\"\\xD801;\""));
            Assert.Throws<SexpReaderException>(() => Read("\"\\x110000;\""));
            Assert.Equal(new SValue("The word \"recursion\" has many meanings."), Read("\"The word \\\"recursion\\\" has many meanings.\""));
            Assert.Equal(new SValue("Here's text \r\ncontaining just one line"), Read("\"Here's text \r\ncontaining just one line\""));
            Assert.Equal(new SValue("Here's text \ncontaining just one line"), Read("\"Here's text \ncontaining just one line\""));
        }

        [Fact]
        public void ReadByteString()
        {
            Assert.Equal(new SValue(Array.Empty<byte>()), Read("#\"\""));
            Assert.Equal(new SValue(new byte[]{ 0xFF }), Read("#\"\\xff;\""));
            Assert.Equal(new SValue(new byte[]{ 0xFF, 0x20, (byte)'a' }), Read("#\"\\xff; a\""));
            Assert.Throws<SexpReaderException>(() => Read("#\"\\xffff; a\""));
            Assert.Throws<SexpReaderException>(() => Read("#\"\\x0a "));
        }

        [Fact]
        public void ReadByteVector()
        {
            Assert.Equal(new SValue(Array.Empty<byte>()), Read("#u8()"));
            Assert.Equal(new SValue(new byte[]{ 0xFF }), Read("#u8(#xff)"));
            Assert.Equal(new SValue(new byte[]{ 0xFF, 0x20 }), Read("#u8(#xff #x20)"));
            Assert.Throws<SexpReaderException>(() => Read("#u8(#xffff)"));
            Assert.Throws<SexpReaderException>(() => Read("#u8(#x0a"));
        }
    }
}
