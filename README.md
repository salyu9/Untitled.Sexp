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
```

## Usage

[SValue](Doc/SValue.md)
[Convertion and Serialization](Doc/Convertion.md)
