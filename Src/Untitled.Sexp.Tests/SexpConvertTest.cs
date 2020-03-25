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
        private static string Serialize<T>(T obj)
        {
            return SexpConvert.Serialize(obj, newline: "\n");
        }
        private static T Deserialize<T>(string s)
        {
            return SexpConvert.Deserialize<T>(s);
        }

        [Fact]
        public void ConvertBasicTypes()
        {
            Assert.Equal("#t", Serialize(true));
            Assert.Equal("#f", Serialize(false));
            Assert.Equal("123", Serialize(123));
            Assert.Equal("1231231414515", Serialize(1231231414515u));
            Assert.Equal("-1231231414515", Serialize(-1231231414515));
            Assert.Equal("123.321", Serialize(123.321));
            Assert.Equal("-inf.0", Serialize(double.NegativeInfinity));
            Assert.Equal(@"#\t", Serialize('t'));
            Assert.Equal(@"#\linefeed", Serialize('\n'));
            Assert.Equal(@"""hello""", Serialize("hello"));
            Assert.Equal("#u8(12 23 34)", Serialize(new byte[] { 12, 23, 34 }));
            Assert.Equal("|sym bol|", Serialize(Symbol.FromString("sym bol")));
            Assert.Equal(@"(45 . ""test"")", Serialize(new Pair(45, "test")));
            Assert.Equal("#t:System.Text.Encoding", Serialize(TypeIdentifier.FromString("System.Text.Encoding")));
        }

        [Fact]
        public void InverseConvertBasicTypes()
        {
            Assert.True(Deserialize<bool>("#t"));
            Assert.False(Deserialize<bool>("#f"));
            Assert.Equal(123, Deserialize<int>("123"));
            Assert.Equal(1231231414515u, Deserialize<ulong>("1231231414515"));
            Assert.Equal(-1231231414515, Deserialize<long>("-1231231414515"));
            Assert.Equal(123.321, Deserialize<float>("123.321"), 5);
            Assert.Equal(double.NegativeInfinity, Deserialize<double>("-inf.0"));
            Assert.Equal('t', Deserialize<char>(@"#\t"));
            Assert.Equal('\n', Deserialize<char>(@"#\linefeed"));
            Assert.Equal("hello", Deserialize<string>(@"""hello"""));
            Assert.Equal(new byte[] { 12, 23, 34 }, Deserialize<byte[]>("#u8(12 23 34)"));
            Assert.Equal(Symbol.FromString("sym bol"), Deserialize<Symbol>("|sym bol|"));
            Assert.Equal(new Pair(45, "test"), Deserialize<Pair>(@"(45 . ""test"")"));
            Assert.Equal(TypeIdentifier.FromString("System.Text.Encoding"), Deserialize<TypeIdentifier>("#t:System.Text.Encoding"));
        }

        [Fact]
        public void ConvertCorelibTypes()
        {
            Assert.Equal("()", Serialize((int?)null));
            Assert.Equal("5", Serialize((int?)5));

            Assert.Equal("(1 2 3)", Serialize(new int[] { 1, 2, 3 }));

            #if !NO_VALUETUPLE
            Assert.Equal(@"(5 ""str"" ++ (1 2 (3)))", Serialize((5, "str", Symbol("++"), (1, 2.0, ValueTuple.Create(3)))));
            Assert.Equal("(1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18 19 20)", Serialize((1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20)));
            #endif

        }

        [Fact]
        public void InverseConvertCorelibTypes()
        {
            Assert.Null(Deserialize<int?>("()"));
            Assert.Equal(5, Deserialize<int?>("5"));

            Assert.Equal(new int[] { 1, 2, 3 }, Deserialize<int[]>("(1 2 3)"));

            Assert.Throws<SexpConvertException>(() => Deserialize<int?>("(1 2)"));

            #if !NO_VALUETUPLE
            Assert.Equal((5, "str", Symbol("++"), (1, 2.0, ValueTuple.Create(3))),
                Deserialize<(int, string, Symbol, (int, double, ValueTuple<int>))>(@"(5 ""str"" ++ (1 2 (3)))"));
            Assert.Equal((1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20),
                Deserialize<(int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int)>("(1 2 3 4 5 6 7 8 9 10 11 12 13 14 15 16 17 18 19 20)"));

            #endif

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
            Assert.Equal("0", Serialize(TestEnum.Default));
            Assert.Equal("5", Serialize(TestEnum.ValueA));
            Assert.Equal("Default", Serialize(TestSymbolEnum.Default));
            Assert.Equal("ValueA", Serialize(TestSymbolEnum.ValueA));

            Assert.Equal("(None)", Serialize(TestFlagsEnum.None));
            Assert.Equal("(ValueA)", Serialize(TestFlagsEnum.ValueA));
            Assert.Equal("(ValueA ValueB)", Serialize(TestFlagsEnum.ValueA | TestFlagsEnum.ValueB));
            Assert.Equal("(ValueA ValueC)", Serialize(TestFlagsEnum.ValueA | TestFlagsEnum.ValueC));
            Assert.Equal("(ValueC)", Serialize(TestFlagsEnum.None | TestFlagsEnum.ValueC));

            Assert.Equal("()", Serialize((TestNoEmptyFlagsEnum)0));

            Assert.Throws<SexpConvertException>(() => Serialize((TestSymbolEnum)4));
            Assert.Throws<SexpConvertException>(() => Serialize((TestFlagsEnum)8));
        }

        [Fact]
        public void InverseConvertEnums()
        {
            Assert.Equal(TestEnum.Default, Deserialize<TestEnum>("0"));
            Assert.Equal(TestEnum.ValueA, Deserialize<TestEnum>("5"));
            Assert.Equal(TestSymbolEnum.Default, Deserialize<TestSymbolEnum>("Default"));
            Assert.Equal(TestSymbolEnum.ValueA, Deserialize<TestSymbolEnum>("ValueA"));

            Assert.Equal(TestFlagsEnum.None, Deserialize<TestFlagsEnum>("(None)"));
            Assert.Equal(TestFlagsEnum.ValueA, Deserialize<TestFlagsEnum>("(ValueA)"));
            Assert.Equal(TestFlagsEnum.ValueA | TestFlagsEnum.ValueB, Deserialize<TestFlagsEnum>("(ValueA ValueB)"));
            Assert.Equal(TestFlagsEnum.ValueA | TestFlagsEnum.ValueC, Deserialize<TestFlagsEnum>("(ValueA ValueC)"));
            Assert.Equal(TestFlagsEnum.None | TestFlagsEnum.ValueC, Deserialize<TestFlagsEnum>("(ValueC)"));

            Assert.Equal((TestNoEmptyFlagsEnum)0, Deserialize<TestNoEmptyFlagsEnum>("()"));

            Assert.Throws<SexpConvertException>(() => Deserialize<TestEnum>("ValueA"));
            Assert.Throws<SexpConvertException>(() => Deserialize<TestSymbolEnum>("ValueC"));

        }

        [Fact]
        public void ConvertCollections()
        {
            Assert.Equal("(2 3 5 7)", Serialize(new[] { 2, 3, 5, 7 }));

            Assert.Equal("(2 3 5 7)", Serialize(new List<int> { 2, 3, 5, 7 }));

            Assert.Equal(@"((""foo"" . 5) (""bar"" . 7))", Serialize(new Dictionary<string, int> { ["foo"] = 5, ["bar"] = 7 }));
        }

        [Fact]
        public void InverseConvertCollections()
        {
            Assert.Equal(new[] { 2, 3, 5, 7 }, Deserialize<int[]>("(2 3 5 7)"));

            Assert.Equal(new List<int> { 2, 3, 5, 7 }, Deserialize<List<int>>("(2 3 5 7)"));

            Assert.Equal("(2 3 5 7)", Serialize(new List<int> { 2, 3, 5, 7 }));
        }

    }
}
