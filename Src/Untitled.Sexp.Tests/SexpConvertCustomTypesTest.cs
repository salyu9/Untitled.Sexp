using System;
using System.Collections.Generic;
using System.Linq;
using Untitled.Sexp.Attributes;
using Untitled.Sexp.Conversion;
using Untitled.Sexp.Formatting;
using Xunit;

namespace Untitled.Sexp.Tests
{
    public class SexpConvertCustomTypesTest
    {
        #region Custom Types

        [SexpAsList]
        class AsListTypeA : IEquatable<AsListTypeA>
        {
            public AsListTypeA()
            {

            }

            public AsListTypeA(int x, int y, int z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public int X { get; set; }

            [SexpIgnore]
            public int NotThis { get; set; }

            private int Y { get; set; }

            private int Z { get; set; }

            public bool Equals(AsListTypeA other)
                => X == other.X && Y == other.Y && Z == other.Z;
        }

        [SexpAsList]
        class AsListTypeB : IEquatable<AsListTypeB>
        {
            [SexpMember(Order = 1)]
            public int X { get; set; }

            [SexpMember(Order = 0)]
            public int Y;

            public bool Equals(AsListTypeB other)
                => X == other.X && Y == other.Y;
        }

        [SexpAsList]
        class ShouldNotBeList
        {
            public int X { get; set; }
            public int Y;
        }

        [SexpAsList]
        class ShouldBeAllOrdered
        {
            [SexpMember(Order = 2)]
            public int X { get; set; }

            public int Y { get; set; }
        }

        [Fact]
        public void ConvertAsList()
        {
            Assert.Equal("(5 6 7)", SexpConvert.Serialize(new AsListTypeA(5, 6, 7) { NotThis = 2 }));
            Assert.Equal("(5 2)", SexpConvert.Serialize(new AsListTypeB { X = 2, Y = 5 }));

            Assert.Throws<SexpConvertException>(() => SexpConvert.Serialize(new ShouldNotBeList { X = 1, Y = 2 }));
            Assert.Throws<SexpConvertException>(() => SexpConvert.Serialize(new ShouldBeAllOrdered { X = 1, Y = 2 }));
        }

        [Fact]
        public void InverseConvertAsList()
        {
            Assert.Equal(new AsListTypeA(5, 6, 7) { NotThis = 2 }, SexpConvert.Deserialize<AsListTypeA>("(5 6 7)"));
            Assert.Equal(new AsListTypeB { X = 2, Y = 5 }, SexpConvert.Deserialize<AsListTypeB>("(5 2)"));

        }

        [SexpAsList]
        class MemberFormatting
        {
            [SexpBooleanFormatting(LongForm = true)]
            public bool B { get; set; }

            [SexpNumberFormatting(Radix = NumberRadix.Hexadecimal)]
            public long N { get; set; }

            [SexpCharacterFormatting(AsciiOnly = true, Escaping = EscapingStyle.UStyle)]
            public string S { get; set; } = "";

            [SexpBytesFormatting(Radix = NumberRadix.Hexadecimal)]
            public byte[] BS { get; set; } = Array.Empty<byte>();

            [SexpListFormatting(Parentheses = ParenthesesType.Braces)]
            public Dictionary<string, int> D { get; set; } = new Dictionary<string, int>();
        }

        [Fact]
        public void ConvertWithFormatting()
        {
            var obj = new MemberFormatting
            {
                B = true,
                N = 177,
                S = "テキスト",
                BS = new byte[] { 0x42, 0x6a, 0x5b },
                D = new Dictionary<string, int> { ["foo"] = 5, ["bar"] = 10 },
            };
            Assert.Equal(@"(#true #xb1 ""\u30c6\u30ad\u30b9\u30c8"" #u8(#x42 #x6a #x5b) {(""foo"" . 5) (""bar"" . 10)})",
                SexpConvert.Serialize(obj));
        }
        #endregion

        #region Polymorphic

        [SexpCustomTypeResolver(typeof(OperationTypeResolver))]
        abstract class OperationBase
        {
            public class OperationTypeResolver : LookupTypeResolver
            {
                public OperationTypeResolver()
                {
                    Add("add", typeof(Add));
                    Add("sub", typeof(Sub));
                }
            }
        }
#pragma warning disable CS0659
        [SexpAsList]
        class Add : OperationBase
        {
            public List<int> Operands { get; set; } = new List<int>();

            public override bool Equals(object? obj)
            {
                if (obj is Add add)
                {
                    return Operands.SequenceEqual(add.Operands);
                }
                return false;
            }
        }

        [SexpAsList]
        class Sub : OperationBase
        {
            public int A { get; set; }
            public int B { get; set; }

            public override bool Equals(object? obj)
            {
                if (obj is Sub sub)
                {
                    return A == sub.A && B == sub.B;
                }
                return false;
            }
        }
#pragma warning restore CS0659

        [Fact]
        public void PolymorphicConvert()
        {
            System.Collections.IList o = new List<int> { 1, 2, 3, 4, 5 };
            Assert.Equal((List<int>)o, (List<int>)SexpConvert.Deserialize<object>(SexpConvert.Serialize(o)));

            object[] arr = { 2, 3, 5, 7 };
            Assert.Equal(arr, SexpConvert.Deserialize<object[]>(SexpConvert.Serialize(arr)));

            var ops = new List<OperationBase>
            {
                new Add { Operands = { 6, 7, 8 } },
                new Sub { A = 10, B = 6 }
            };
            Assert.Equal("((#type:add (6 7 8)) (#type:sub 10 6))", SexpConvert.Serialize(ops));

            Assert.Equal(ops, SexpConvert.Deserialize<List<OperationBase>>("((#type:add (6 7 8)) (#type:sub 10 6))"));
        }

        #endregion

        [SexpCustomConverter(typeof(CustomOperationConverter))]
        abstract class CustomOperationBase
        {
            public class CustomOperationConverter : SexpConverter
            {
                public override bool CanConvert(Type type)
                    => type == typeof(CustomOperationBase);

                public override object? ToObject(SValue value)
                {
                    var pair = value.AsPair();
                    var typeid = pair.Car.AsSymbol();
                    var args = pair.Cdr.ToList<int>();
                    if (typeid.Name == "add")
                    {
                        return new CustomAdd { Operands = args };
                    }
                    else if (typeid.Name == "sub")
                    {
                        return new CustomSub { A = args[0], B = args[1] };
                    }
                    throw new SexpConvertException(typeof(CustomOperationBase), value);
                }

                public override object? ToObjectExactType(SValue value)
                {
                    throw new NotImplementedException();
                }

                public override SValue ToValue(Type type, object? obj)
                {
                    System.Diagnostics.Debug.Assert(obj != null);
                    if (obj is CustomAdd add)
                    {
                        return SValue.Cons(Symbol.FromString("add"), SValue.List(add.Operands.Select(o => new SValue(o))));
                    }
                    else if (obj is CustomSub sub)
                    {
                        return SValue.List(Symbol.FromString("sub"), sub.A, sub.B);
                    }
                    throw new SexpConvertException(typeof(CustomOperationBase), obj);
                }

                public override SValue ToValueExactType(object obj)
                {
                    throw new NotImplementedException();
                }
            }
        }
#pragma warning disable CS0659
        [SexpAsList]
        class CustomAdd : CustomOperationBase
        {
            public List<int> Operands { get; set; } = new List<int>();

            public override bool Equals(object? obj)
            {
                if (obj is CustomAdd add)
                {
                    return Operands.SequenceEqual(add.Operands);
                }
                return false;
            }
        }

        [SexpAsList]
        class CustomSub : CustomOperationBase
        {
            public int A { get; set; }
            public int B { get; set; }

            public override bool Equals(object? obj)
            {
                if (obj is CustomSub sub)
                {
                    return A == sub.A && B == sub.B;
                }
                return false;
            }
        }

        [Fact]
        public void CustomConvert()
        {
            var ops = new List<CustomOperationBase>
            {
                new CustomAdd { Operands = { 6, 7, 8 } },
                new CustomSub { A = 10, B = 6 }
            };
            Assert.Equal("((add 6 7 8) (sub 10 6))", SexpConvert.Serialize(ops));

            Assert.Equal(ops, SexpConvert.Deserialize<List<CustomOperationBase>>("((add 6 7 8) (sub 10 6))"));
        }
    }
}
