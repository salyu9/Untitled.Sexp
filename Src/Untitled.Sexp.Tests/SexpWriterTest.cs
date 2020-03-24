using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Untitled.Sexp.Formatting;
using Xunit;
using static Untitled.Sexp.SValue;

namespace Untitled.Sexp.Tests
{
    public class SexpWriterTest
    {
        private static readonly SexpTextWriterSettings DefaultSettings = new SexpTextWriterSettings();

        private static string Write(SValue value, SexpTextWriterSettings? settings = null)
        {
            using var stringWriter = new StringWriter();
            stringWriter.NewLine = "\n";
            var writer = new SexpTextWriter(stringWriter, settings ?? DefaultSettings);
            writer.Write(value);
            return stringWriter.ToString();
        }

        private static string WriteAll(SexpTextWriterSettings? settings, params SValue[] values)
        {
            using var stringWriter = new StringWriter();
            stringWriter.NewLine = "\n";
            var writer = new SexpTextWriter(stringWriter, settings ?? DefaultSettings);
            writer.WriteAll(values);
            return stringWriter.ToString();
        }

        private static string WriteAll(params SValue[] values)
            => WriteAll(null, values);

        [Fact]
        public void WriterSettings()
        {
            Assert.Equal("()", Write(Null));
            Assert.Equal("null", Write(Null, new SexpTextWriterSettings { NullLiteral = NullLiteralType.Null }));
            Assert.Equal("nil", Write(Null, new SexpTextWriterSettings { NullLiteral = NullLiteralType.Nil }));


            Assert.Equal("() ()", WriteAll(new SexpTextWriterSettings { SeparatorType = WriterSeparatorType.Space }, Null, Null));
            Assert.Equal("()\n\n()", WriteAll(new SexpTextWriterSettings { SeparatorType = WriterSeparatorType.DoubleNewline }, Null, Null));
            Assert.Equal("()I-am-separator()", WriteAll(new SexpTextWriterSettings { SeparatorType = WriterSeparatorType.Custom, CustomSeparator = "I-am-separator" }, Null, Null));
            Assert.Equal("()\n()", WriteAll(Null, Null));
        }

        [Fact]
        public void WriteBoolean()
        {
            var longForm = new BooleanFormatting { LongForm = true };

            Assert.Equal("#t", Write(True));
            Assert.Equal("#true", Write(new SValue(true, longForm)));
            Assert.Equal("#f", Write(False));
            Assert.Equal("#false", Write(new SValue(false, longForm)));
        }

        [Fact]
        public void WriteNumber()
        {
            var pdec = new NumberFormatting { Radix = NumberRadix.PrefixedDecimal };
            var hex = new NumberFormatting { Radix = NumberRadix.Hexadecimal };
            var oct = new NumberFormatting { Radix = NumberRadix.Octal };
            var bin = new NumberFormatting { Radix = NumberRadix.Binary };
            Assert.Equal("123", Write(new SValue(123)));
            Assert.Equal("#d123", Write(new SValue(123, pdec)));
            Assert.Equal("#x7b", Write(new SValue(123, hex)));
            Assert.Equal("#o173", Write(new SValue(123, oct)));
            Assert.Equal("#b1111011", Write(new SValue(123, bin)));

            Assert.Equal("+nan.0", Write(new SValue(double.NaN)));
            Assert.Equal("+inf.0", Write(new SValue(double.PositiveInfinity)));
            Assert.Equal("-inf.0", Write(new SValue(double.NegativeInfinity)));
            Assert.Equal("#x+inf.0", Write(new SValue(double.PositiveInfinity, hex)));
            Assert.Throws<SexpWriterException>(() => Write(new SValue(1.0, oct)));
        }

        [Fact]
        public void WriteChar()
        {
            var ascii = new CharacterFormatting { AsciiOnly = true };
            var asciiUStyle = new CharacterFormatting { AsciiOnly = true, Escaping = EscapingStyle.UStyle };

            Assert.Equal(@"#\linefeed", Write(new SValue('\n')));
            Assert.Equal(@"#\x6d4b", Write(new SValue('测', ascii)));
            Assert.Equal(@"#\u6d4b", Write(new SValue('测', asciiUStyle)));
            Assert.Equal(@"#\𪚥", Write(Char(0x2A6A5)));
            Assert.Equal(@"#\U02a6a5", Write(Char(0x2A6A5, asciiUStyle)));

        }

        [Fact]
        public void WriteSymbol()
        {
            var ascii = new CharacterFormatting { AsciiOnly = true };
            var asciiUStyle = new CharacterFormatting { AsciiOnly = true, Escaping = EscapingStyle.UStyle };

            Assert.Equal("...", Write(Symbol("...")));
            Assert.Equal("+", Write(Symbol("+")));
            Assert.Equal("+soup+", Write(Symbol("+soup+")));
            Assert.Equal("<=?", Write(Symbol("<=?")));
            Assert.Equal("->string", Write(Symbol("->string")));
            Assert.Equal("|two words|", Write(Symbol("two words")));
            Assert.Equal(@"|two\|words|", Write(Symbol(@"two|words")));
            Assert.Equal(@"|two \| words|", Write(Symbol(@"two | words")));
            Assert.Equal(@"|two \| words|", Write(Symbol(@"two | words", ascii)));
            Assert.Equal(@"|two\\words|", Write(Symbol(@"two\words")));
            Assert.Equal(@"|""quotedwords""|", Write(Symbol(@"""quotedwords""")));
            Assert.Equal(@"|two(words|", Write(Symbol(@"two(words")));
            Assert.Equal(@"テスト", Write(Symbol("テスト")));
            Assert.Equal(@"|\x30c6;\x30b9;\x30c8;|", Write(Symbol("テスト", ascii)));
            Assert.Equal(@"|\u30c6\u30b9\u30c8|", Write(Symbol("テスト", asciiUStyle)));
            Assert.Equal(@"𪚥", Write(Symbol(@"𪚥")));
            Assert.Equal(@"|\x2a6a5;|", Write(Symbol(@"𪚥", ascii)));
            Assert.Equal(@"|\U0002a6a5|", Write(Symbol(@"𪚥", asciiUStyle)));

            Assert.Equal(@"|#t|", Write(Symbol("#t")));
            Assert.Equal(@"|123|", Write(Symbol("123")));
            Assert.Equal(@"||", Write(Symbol("")));
            Assert.Equal(@"|+1|", Write(Symbol("+1")));
            Assert.Equal(@"|-inf.0|", Write(Symbol("-inf.0")));
            Assert.Equal(@"inf.0", Write(Symbol("inf.0")));
            Assert.Equal(@"|1.6|", Write(Symbol("1.6")));
            Assert.Equal(@"1.6.2", Write(Symbol("1.6.2")));
            Assert.Equal(@"|1e6|", Write(Symbol("1e6")));
            Assert.Equal(@"1e6e2", Write(Symbol("1e6e2")));
        }

        [Fact]
        public void WriteString()
        {
            var ascii = new CharacterFormatting { AsciiOnly = true };
            var asciiUStyle = new CharacterFormatting { AsciiOnly = true, Escaping = EscapingStyle.UStyle };

            Assert.Equal(@"""""", Write(""));
            Assert.Equal(@"""abc""", Write("abc"));
            Assert.Equal(@""" """, Write(" "));
            Assert.Equal(@"""two words""", Write("two words"));
            Assert.Equal(@"""two|words""", Write("two|words"));
            Assert.Equal(@"""two\""words""", Write("two\"words"));
            Assert.Equal(@"""テスト""", Write("テスト"));
            Assert.Equal(@"""\x30c6;\x30b9;\x30c8;""", Write(new SValue("テスト", ascii)));
            Assert.Equal(@"""\u30c6\u30b9\u30c8""", Write(new SValue("テスト", asciiUStyle)));
            Assert.Equal(@"""𪚥""", Write("𪚥"));
            Assert.Equal(@"""\x2a6a5;""", Write(new SValue("𪚥", ascii)));
            Assert.Equal(@"""\U0002a6a5""", Write(new SValue("𪚥", asciiUStyle)));
            Assert.Equal(@"""The word \""recursion\"" has many meanings.""", Write("The word \"recursion\" has many meanings."));
            Assert.Equal(@"""Here's text \r\ncontaining just one line""", Write("Here's text \r\ncontaining just one line"));
            Assert.Equal(@"""Here's text \ncontaining just one line""", Write("Here's text \ncontaining just one line"));
        }

        [Fact]
        public void WriteBytes()
        {
            var byteString = new BytesFormatting { ByteString = true };

            var lineLimit = new BytesFormatting { LineLimit = 3 };

            var pdec = new BytesFormatting { Radix = NumberRadix.PrefixedDecimal };
            var hex = new BytesFormatting { Radix = NumberRadix.Hexadecimal };
            var oct = new BytesFormatting { Radix = NumberRadix.Octal };
            var bin = new BytesFormatting { Radix = NumberRadix.Binary };

            var bracket = new BytesFormatting { Parentheses = ParenthesesType.Brackets };
            var brace = new BytesFormatting { Parentheses = ParenthesesType.Braces };

            var bytes = Encoding.UTF8.GetBytes("Untitled");
            //0x55, 0x6e, 0x74, 0x69, 0x74, 0x6c, 0x65, 0x64, 0x2e, 0x53, 0x65, 0x78, 0x70


            var nonPrintableBytes = new byte[] { 0x05, 0x07, 0xFF };

            Assert.Equal(@"#""Untitled""", Write(new SValue(bytes, byteString)));
            Assert.Equal(@"#""\x5;\x7;\xff;""", Write(new SValue(nonPrintableBytes, byteString)));

            Assert.Equal(@"#u8(85 110 116 105 116 108 101 100)", Write(new SValue(bytes)));
            Assert.Equal(@"#u8(#d85 #d110 #d116 #d105 #d116 #d108 #d101 #d100)",
                Write(new SValue(bytes, pdec)));
            Assert.Equal(@"#u8(#x55 #x6e #x74 #x69 #x74 #x6c #x65 #x64)",
                Write(new SValue(bytes, hex)));
            Assert.Equal(@"#u8(#o125 #o156 #o164 #o151 #o164 #o154 #o145 #o144)",
                Write(new SValue(bytes, oct)));
            Assert.Equal(@"#u8(#b1010101 #b1101110 #b1110100 #b1101001 #b1110100 #b1101100 #b1100101 #b1100100)",
                Write(new SValue(bytes, bin)));

            Assert.Equal(@"#u8[85 110 116 105 116 108 101 100]", Write(new SValue(bytes, bracket)));
            Assert.Equal(@"#u8{85 110 116 105 116 108 101 100}", Write(new SValue(bytes, brace)));
            
            Assert.Equal("#u8(85 110 116\n    105 116 108\n    101 100)", Write(new SValue(bytes, lineLimit)));
        }

        [Fact]
        public void WriteList()
        {
            var bracket = new ListFormatting { Parentheses = ParenthesesType.Brackets };
            var brace = new ListFormatting { Parentheses = ParenthesesType.Braces };

            var breakIndex = new ListFormatting { LineBreakIndex = 2, LineExtraSpaces = 5 };

            Assert.Equal("(1 2 3)", Write(List(1, 2, 3)));
            Assert.Equal("(1 . 3)", Write(Cons(1, 3)));
            Assert.Equal("(1 2 . 3)", Write(Cons(1, Cons(2, 3))));
            Assert.Equal("[1 2 3]", Write(List(bracket, 1, 2, 3)));
            Assert.Equal("{1 2 3}", Write(List(brace, 1, 2, 3)));
            Assert.Equal("[1 2 . 3]", Write(Cons(1, Cons(2, 3), bracket)));
            Assert.Equal("{1 2 . 3}", Write(Cons(1, Cons(2, 3), brace)));
            Assert.Equal("(1 2 3 (4 5))", Write(List(1, 2, 3, List(4, 5))));
        }
    }
}
