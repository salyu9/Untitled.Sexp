using System;
using System.IO;
using System.Text;
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
                Parenthese = ParentheseType.Brace,
                LineBreakIndex = 4,
                LineExtraSpaces = 4,
            };
            var builder = new ListBuilder(listFormatting) { SValue.Symbol(""), 100, 200 }
                .Add(new SValue(300, new NumberFormatting { Radix = NumberRadix.Hexadecimal }))
                .Add(new SValue(400, new NumberFormatting { Radix = NumberRadix.Binary }))
                .Add("テスト")
                .Add(new SValue("𪚥", new CharacterFormatting { AsciiOnly = true, Escaping = EscapingStyle.UStyle }))
                .Add(SSymbol.FromString("hello world"))
                .Add(SValue.List(new ListFormatting { LineBreakIndex = 3 },
                    "sub", "list", "with", "bytes:",
                    new SValue(Encoding.UTF8.GetBytes("A bytearray"))))
                ;

            foreach (var ch in "st\nr")
            {
                builder.Add(ch);
            }

            var sexp = builder.ToValue();

            var s = sexp.ToString();

            var sexp2 = Sexp.Parse(s);

            Console.WriteLine(sexp.DeepEquals(sexp2));
            Console.WriteLine(sexp);
        }
    }
}
