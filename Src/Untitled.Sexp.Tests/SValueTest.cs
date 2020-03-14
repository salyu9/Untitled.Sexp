using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Untitled.Sexp.Tests
{
    public class SValueTest
    {
        private static readonly IEqualityComparer<SValue> Comparer = SValueComparer.Default;

        [Fact]
        public void ImplicitCast()
        {
            Assert.Equal(new SValue(true), (SValue)true, Comparer);
            Assert.Equal(new SValue(false), (SValue)false, Comparer);
            Assert.Equal(new SValue(10), (SValue)10, Comparer);
            Assert.Equal(new SValue(10.0), (SValue)10.0, Comparer);
            Assert.Equal(new SValue('r'), (SValue)'r', Comparer);
            Assert.Equal(new SValue("test"), (SValue)"test", Comparer);
            Assert.Equal(new SValue(SSymbol.FromString("test")), SSymbol.FromString("test"), Comparer);
            Assert.Equal(new SValue(1, 2), new SPair(1, 2), Comparer);
        }

        [Fact]
        public void ExplicitCast()
        {
            Assert.True((bool)new SValue(true));
            Assert.False((bool)new SValue(false));
            Assert.Equal(10, (int)new SValue(10));
            Assert.Equal(10, (long)new SValue(10));
            Assert.Equal(10, (double)new SValue(10));
            Assert.Equal(10.0, (double)new SValue(10.0));
            Assert.Equal(10, (int)new SValue(10.0));
            Assert.Equal('r', (char)new SValue('r'));
            Assert.Equal("test", (string)new SValue("test"));
            Assert.Equal(SSymbol.FromString("test"), (SSymbol)new SValue(SSymbol.FromString("test")));
            Assert.Equal(new SPair(1, 2), (SPair)new SValue(1, 2), Comparer);
            Assert.Equal(new byte[]{ 0x30, 0x4A }, (byte[])new SValue(new byte[]{ 0x30, 0x4A }));
        }

        [Fact]
        public void Cast()
        {
            Assert.True(new SValue(true).Cast<bool>());
            Assert.False(new SValue(false).Cast<bool>());
            Assert.Equal(10, new SValue(10).Cast<int>());
            Assert.Equal(10, new SValue(10).Cast<long>());
            Assert.Equal(10, new SValue(10).Cast<double>());
            Assert.Equal(10.0, new SValue(10.0).Cast<double>());
            Assert.Equal('r', new SValue('r').Cast<char>());
            Assert.Equal("test", new SValue("test").Cast<string>());
            Assert.Equal(SSymbol.FromString("test"), new SValue(SSymbol.FromString("test")).Cast<SSymbol>());
            Assert.Equal(new SPair(1, 2), new SValue(1, 2).Cast<SPair>(), Comparer);
            Assert.Equal(new byte[]{ 0x30, 0x4A }, new SValue(new byte[]{ 0x30, 0x4A }).Cast<byte[]>());

            Assert.Equal(new []{ 5, 6, 3 }, SValue.List(5, 6, 3).ToList<int>());
        }

        [Fact]
        public void Equality()
        {
            Assert.Equal(true, SValue.True, Comparer);
            Assert.Equal(false, SValue.False, Comparer);
            Assert.Equal(10, new SValue(10), Comparer);
            Assert.Equal(10.0, new SValue(10.0), Comparer);
            Assert.Equal('r', new SValue('r'), Comparer);
            Assert.Equal(SValue.Char(0x2A6A5), SValue.Char(0x2A6A5), Comparer);
        }

    }
}
