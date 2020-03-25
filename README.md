# Untitled.Sexp

Untitled.Sexp is a simple .Net library for reading, writing and serializing s-expressions.

```csharp
foreach (var elem in Sexp.Parse("(+ - abc -> |hello world|)").AsEnumerable())
{
    Console.WriteLine(elem.AsSymbol().Name);
}
// adsfs
// +
// -
// abc
// ->
// hello world

var info = new BookInfo{
    Name = "SICP",
    Price = 25,
    Language = Language.English,
    Versions = new string[] { "Hardcover", "Paperback", "Digital" },
};
var output = SexpConvert.Serialize(info);
// ((Name . "SICP")
//  (Price . 25)
//  (Language . English)
//  (Versions "Hardcover" "Paperback" "Digital"))

var tuple = SexpConvert.Deserialize<(int, string, Symbol, (int, double, ValueTuple<int>))>(@"(5 ""str"" ++ (1 2 (3)))");
// (5, "str", Symbol("++"), (1, 2.0, ValueTuple.Create(3))
```

## Usage

- [SValue](Doc/SValue.md)
- [Convertion and Serialization](Doc/Convertion.md)
