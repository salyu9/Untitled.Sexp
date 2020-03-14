# Untitled.Sexp

Untitled.Sexp is a simple .Net library for reading and writing s-expressions.

## Usage

The core type in Untitled.Sexp is ```SValue```, which represents an s-expression value/object. The content of an ```SValue``` can be a number, boolean, character, symbol, string, pair/list, or bytes.
The syntax of values of these types is similar to scheme.

### Symbols

```scheme
a34kTMNs
list->vector
|two words|
|symbol with\x20;escaping|
```

### Numbers

An ```SValue``` can hold a number with type ```int```, ```long``` or ```double```. Rational, complex and big integers are not supported yet.

```scheme
123
#x0eaf
#b10010
+nan.0
-inf.0
```

### Booleans

Short and long form are supported.

```scheme
#t
#false
```

### Characters

A character is an unicode scalar value (0 ~ 0x10FFFF without surrogates). The underlying type is ```int```. If there is no need to deal with character larger than 0xFFFF, you can use ```AsChar()``` or just cast ```SValue``` to ```char```, otherwise ```CharToString()``` must be called to fetch characters correctly.
Single character, escaped and character name are allowed.

```scheme
#\g
#\x48
#\linefeed
```

### Strings

A string can contain escaping characters in the form R7RS specified, and if allowed, also in racket style (\u0000 and \U000000). The same goes for symbols.

```scheme
"This is a string."
"With\x20;escaping."  ; R7RS style escaping
"With\u0020escaping." ; Racket style escaping
```

### Bytes

An ```SValue``` can hold a ```byte[]``` data and exposes it as read-only collection. Two forms are supported: R7RS byte vector form and Racket byte string form.

```scheme
#u8(0 0 0 14 #x0d)  ; byte vector
#"bytestring"       ; byte string
```

### Pais and lists

Pairs and lists are just like those in lisp languages. Default reader setting allows brackets and braces as delimiters.

```scheme
(1 2 3)
[a . b]
{a b . c} // improper list, equivalent to (a . (b . c))
```

### Comments

The reader accepts these kinds of comments.

```scheme
; single line comments
#| nestable #| block comments |# |#
#;(s-expression comments)
```

### Formatting

An ```SValue``` can be constructed with a formatting setting that matches the value type, specify how the value will be written. For example, for numbers there is ```NumberFormatting``` with property ```Radix```, tolding the writer to write the number with hexadecimal or binary form.

### Example

Here is a simple example shows how to create various types of values. ```SValue``` constructors and implicit casts can be used to create simple values. ```SValue.List``` and ```SValue.Cons``` can create list and pairs. There is also a helper class ```ListBuilder``` that can append values to a list under building.

```csharp
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
```

Call ```ToString()``` on an ```SValue``` will create an ```SexpTextWriter``` to write the object and convert it to string. For the ```sexp``` object above, the result will be:

```scheme
{|| 100 200 #x12c
    #b110010000
    "テスト"
    "\U02a6a5"
    |hello world|
    ("sub" "list" "with"
      "bytes:"
      #u8(65 32 98 121 116 101 97 114 114 97 121))
    #\s
    #\t
    #\linefeed
    #\r}
```

Use ```SValue.Parse``` or an ```SexpTextReader``` to parse strings to ```SValue``` object. Note that only radix and list delimiters will be kept by reader, other formatting informations are ignored.
