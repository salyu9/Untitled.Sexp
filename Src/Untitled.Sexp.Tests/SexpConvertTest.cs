using System;
using System.Collections.Generic;
using System.IO;
using Untitled.Sexp.Attributes;
using Untitled.Sexp.Conversion;
using Xunit;
using static Untitled.Sexp.SValue;

namespace Untitled.Sexp.Tests
{
    public class SexpConvertTest
    {
        private static readonly IEqualityComparer<SValue> Comparer = SValueComparer.Default;

        [Fact]
        public void ConvertBasicTypes()
        {
            Assert.Equal("#t", SexpConvert.Serialize(true));
            Assert.Equal("#f", SexpConvert.Serialize(false));
            Assert.Equal("123", SexpConvert.Serialize(123));
            Assert.Equal("1231231414515", SexpConvert.Serialize(1231231414515u));
            Assert.Equal("-1231231414515", SexpConvert.Serialize(-1231231414515));
            Assert.Equal("123.321", SexpConvert.Serialize(123.321));
            Assert.Equal("-inf.0", SexpConvert.Serialize(double.NegativeInfinity));
            Assert.Equal(@"#\t", SexpConvert.Serialize('t'));
            Assert.Equal(@"#\linefeed", SexpConvert.Serialize('\n'));
            Assert.Equal(@"""hello""", SexpConvert.Serialize("hello"));
            Assert.Equal("#u8(12 23 34)", SexpConvert.Serialize(new byte[] { 12, 23, 34 }));
            Assert.Equal("|sym bol|", SexpConvert.Serialize(Symbol.FromString("sym bol")));
            Assert.Equal(@"(45 . ""test"")", SexpConvert.Serialize(new Pair(45, "test")));
            Assert.Equal("#type:System.Text.Encoding", SexpConvert.Serialize(TypeIdentifier.FromString("System.Text.Encoding")));
        }

        [Fact]
        public void InverseConvertBasicTypes()
        {
            Assert.True(SexpConvert.Deserialize<bool>("#t"));
            Assert.False(SexpConvert.Deserialize<bool>("#f"));
            Assert.Equal(123, SexpConvert.Deserialize<int>("123"));
            Assert.Equal(1231231414515u, SexpConvert.Deserialize<ulong>("1231231414515"));
            Assert.Equal(-1231231414515, SexpConvert.Deserialize<long>("-1231231414515"));
            Assert.Equal(123.321, SexpConvert.Deserialize<float>("123.321"), 5);
            Assert.Equal(double.NegativeInfinity, SexpConvert.Deserialize<double>("-inf.0"));
            Assert.Equal('t', SexpConvert.Deserialize<char>(@"#\t"));
            Assert.Equal('\n', SexpConvert.Deserialize<char>(@"#\linefeed"));
            Assert.Equal("hello", SexpConvert.Deserialize<string>(@"""hello"""));
            Assert.Equal(new byte[] { 12, 23, 34 }, SexpConvert.Deserialize<byte[]>("#u8(12 23 34)"));
            Assert.Equal(Symbol.FromString("sym bol"), SexpConvert.Deserialize<Symbol>("|sym bol|"));
            Assert.Equal(new Pair(45, "test"), SexpConvert.Deserialize<Pair>(@"(45 . ""test"")"), Comparer);
            Assert.Equal(TypeIdentifier.FromString("System.Text.Encoding"), SexpConvert.Deserialize<TypeIdentifier>("#type:System.Text.Encoding"));
        }

        [Fact]
        public void ConvertCorelibTypes()
        {
            Assert.Equal("()", SexpConvert.Serialize((int?)null));
            Assert.Equal("5", SexpConvert.Serialize((int?)5));

            Assert.Equal("(1 2 3)", SexpConvert.Serialize(new int[] { 1, 2, 3 }));

        }

        [Fact]
        public void InverseConvertCorelibTypes()
        {
            Assert.Null(SexpConvert.Deserialize<int?>("()"));
            Assert.Equal(5, SexpConvert.Deserialize<int?>("5"));

            Assert.Equal(new int[] { 1, 2, 3 }, SexpConvert.Deserialize<int[]>("(1 2 3)"));

            Assert.Throws<SexpConvertException>(() => SexpConvert.Deserialize<int?>("(1 2)"));
        }

        enum TestEnum
        {
            Default = 0,
            ValueA = 5,
        }

        [SexpSymbolEnum]
        enum TestSymbolEnum
        {
            Default = 0,
            ValueA = 5,
        }

        [SexpSymbolEnum]
        [Flags]
        enum TestFlagsEnum
        {
            None = 0,
            ValueA = 1,
            ValueB = 2,
            ValueC = 4,
        }

        [SexpSymbolEnum]
        [Flags]
        enum TestNoEmptyFlagsEnum
        {
            ValueA = 1,
            ValueB = 2,
            ValueC = 4,
        }

        [Fact]
        public void ConvertEnums()
        {
            Assert.Equal("0", SexpConvert.Serialize(TestEnum.Default));
            Assert.Equal("5", SexpConvert.Serialize(TestEnum.ValueA));
            Assert.Equal("Default", SexpConvert.Serialize(TestSymbolEnum.Default));
            Assert.Equal("ValueA", SexpConvert.Serialize(TestSymbolEnum.ValueA));

            Assert.Equal("(None)", SexpConvert.Serialize(TestFlagsEnum.None));
            Assert.Equal("(ValueA)", SexpConvert.Serialize(TestFlagsEnum.ValueA));
            Assert.Equal("(ValueA ValueB)", SexpConvert.Serialize(TestFlagsEnum.ValueA | TestFlagsEnum.ValueB));
            Assert.Equal("(ValueA ValueC)", SexpConvert.Serialize(TestFlagsEnum.ValueA | TestFlagsEnum.ValueC));
            Assert.Equal("(ValueC)", SexpConvert.Serialize(TestFlagsEnum.None | TestFlagsEnum.ValueC));

            Assert.Equal("()", SexpConvert.Serialize((TestNoEmptyFlagsEnum)0));

            Assert.Throws<SexpConvertException>(() => SexpConvert.Serialize((TestSymbolEnum)4));
            Assert.Throws<SexpConvertException>(() => SexpConvert.Serialize((TestFlagsEnum)8));
        }

        [Fact]
        public void InverseConvertEnums()
        {
            Assert.Equal(TestEnum.Default, SexpConvert.Deserialize<TestEnum>("0"));
            Assert.Equal(TestEnum.ValueA, SexpConvert.Deserialize<TestEnum>("5"));
            Assert.Equal(TestSymbolEnum.Default, SexpConvert.Deserialize<TestSymbolEnum>("Default"));
            Assert.Equal(TestSymbolEnum.ValueA, SexpConvert.Deserialize<TestSymbolEnum>("ValueA"));

            Assert.Equal(TestFlagsEnum.None, SexpConvert.Deserialize<TestFlagsEnum>("(None)"));
            Assert.Equal(TestFlagsEnum.ValueA, SexpConvert.Deserialize<TestFlagsEnum>("(ValueA)"));
            Assert.Equal(TestFlagsEnum.ValueA | TestFlagsEnum.ValueB, SexpConvert.Deserialize<TestFlagsEnum>("(ValueA ValueB)"));
            Assert.Equal(TestFlagsEnum.ValueA | TestFlagsEnum.ValueC, SexpConvert.Deserialize<TestFlagsEnum>("(ValueA ValueC)"));
            Assert.Equal(TestFlagsEnum.None | TestFlagsEnum.ValueC, SexpConvert.Deserialize<TestFlagsEnum>("(ValueC)"));

            Assert.Equal((TestNoEmptyFlagsEnum)0, SexpConvert.Deserialize<TestNoEmptyFlagsEnum>("()"));

            Assert.Throws<SexpConvertException>(() => SexpConvert.Deserialize<TestEnum>("ValueA"));
            Assert.Throws<SexpConvertException>(() => SexpConvert.Deserialize<TestSymbolEnum>("ValueC"));

        }

        [Fact]
        public void ConvertCollections()
        {
            Assert.Equal("(2 3 5 7)", SexpConvert.Serialize(new[] { 2, 3, 5, 7 }));

            Assert.Equal("(2 3 5 7)", SexpConvert.Serialize(new List<int> { 2, 3, 5, 7 }));

            Assert.Equal(@"((""foo"" . 5) (""bar"" . 7))", SexpConvert.Serialize(new Dictionary<string, int> { ["foo"] = 5, ["bar"] = 7 }));
        }

        [Fact]
        public void InverseConvertCollections()
        {
            Assert.Equal(new[] { 2, 3, 5, 7 }, SexpConvert.Deserialize<int[]>("(2 3 5 7)"));

            Assert.Equal(new List<int> { 2, 3, 5, 7 }, SexpConvert.Deserialize<List<int>>("(2 3 5 7)"));

            Assert.Equal("(2 3 5 7)", SexpConvert.Serialize(new List<int> { 2, 3, 5, 7 }));
        }

    }
}
