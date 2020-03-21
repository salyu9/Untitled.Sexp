using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Untitled.Sexp.Attributes;
using Untitled.Sexp.Formatting;
using Untitled.Sexp.Utilities;

namespace Untitled.Sexp.Samples
{
    class Program
    {
        static void Main()
        {
            var listFormatting = new ListFormatting
            {
                Parentheses = ParenthesesType.Braces,
                LineBreakIndex = 4,
                LineExtraSpaces = 4,
            };
            var builder = new ListBuilder(listFormatting) { SValue.Symbol(""), 100, 200 }
                .Add(new SValue(300, new NumberFormatting { Radix = NumberRadix.Hexadecimal }))
                .Add(new SValue(400, new NumberFormatting { Radix = NumberRadix.Binary }))
                .Add("テスト")
                .Add(new SValue("𪚥", new CharacterFormatting { AsciiOnly = true, Escaping = EscapingStyle.UStyle }))
                .Add(Symbol.FromString("hello world"))
                .Add(SValue.List(new ListFormatting { LineBreakIndex = 3 },
                    "sub", "list", "with", "bytes:",
                    new SValue(Encoding.UTF8.GetBytes("A bytearray"))))
                ;

            foreach (var ch in "st\nr")
            {
                builder.Add(ch);
            }

            var sexp = builder.ToValue();

            Console.WriteLine(sexp);

            Console.WriteLine(SexpConvert.Serialize(new List<int> { 1, 2, 3, 4, 5 }));

            Console.WriteLine(SexpConvert.Serialize(new SortedDictionary<string, int?> { ["test"] = 1, ["asdf"] = 2 }));

            var dict = SexpConvert.Deserialize<SortedDictionary<string, int>>(@"((""asdf"" . 2) (""test"" . 1))");
            foreach (var kv in dict) Console.WriteLine($"{kv.Key} = {kv.Value}");

            var info = new BookInfo
            {
                Name = "SICP",
                Price = 25,
                Language = Language.English,
                Versions = new string[] { "Hardcover", "Paperback", "Digital" },
            };
            var output = SexpConvert.Serialize(info);
        }
        
        [SexpAsList] class TypeWithObject { public object o; }
        [SexpSymbolEnum]
        enum Language
        {
            English,
            French,
        }

        [SexpAsAssociationList(
            Style = AssociationListStyle.ListOfPairs,
            InnerParentheses = ParenthesesType.Brackets)]
        class BookInfo
        {
            public string Name { get; set; }
            public int Price { get; set; }
            public Language Language { get; set; }
            public string[] Versions { get; set; }
        }

        [SexpAsList]
        class TestType
        {
            [SexpBooleanFormatting(LongForm = true)]
            public bool B { get; set; }

            [SexpNumberFormatting(Radix = NumberRadix.Hexadecimal)]
            public long N { get; set; }
        }
    }
}
