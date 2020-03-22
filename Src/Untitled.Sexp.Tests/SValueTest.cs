using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Untitled.Sexp.Tests
{
    public class SValueTest
    {
        [Fact]
        public void ImplicitCast()
        {
            Assert.Equal(new SValue(true), (SValue)true);
            Assert.Equal(new SValue(false), (SValue)false);
            Assert.Equal(new SValue(10), (SValue)10);
            Assert.Equal(new SValue(10.0), (SValue)10.0);
            Assert.Equal(new SValue('r'), (SValue)'r');
            Assert.Equal(new SValue("test"), (SValue)"test");
            Assert.Equal(new SValue(Symbol.FromString("test")), Symbol.FromString("test"));
            Assert.Equal(new SValue(1, 2), new Pair(1, 2));
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
            Assert.Equal(Symbol.FromString("test"), (Symbol)new SValue(Symbol.FromString("test")));
            Assert.Equal(new Pair(1, 2), (Pair)new SValue(1, 2));
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
            Assert.Equal(Symbol.FromString("test"), new SValue(Symbol.FromString("test")).Cast<Symbol>());
            Assert.Equal(new Pair(1, 2), new SValue(1, 2).Cast<Pair>());
            Assert.Equal(new byte[]{ 0x30, 0x4A }, new SValue(new byte[]{ 0x30, 0x4A }).Cast<byte[]>());

            Assert.Equal(new []{ 5, 6, 3 }, SValue.List(5, 6, 3).ToList<int>());
        }

        [Fact]
        public void Equality()
        {
            Assert.Equal(true, SValue.True);
            Assert.Equal(false, SValue.False);
            Assert.Equal(10, new SValue(10));
            Assert.Equal(10.0, new SValue(10.0));
            Assert.Equal('r', new SValue('r'));
            Assert.Equal(SValue.Char(0x2A6A5), SValue.Char(0x2A6A5));
        }

    }
}
