# Convertion and Serialization

Untitled.Sexp supports converting between .Net objects and ```SValue```s.

## SexpConvert

Simply use ```SexpConvert``` for convertion and serialization. The ```ToObject()``` and ```ToValue()``` methods provide convertions between .Net objects and ```SValue```. The ```Serialize()``` and ```Deserialize()``` methods converts them to/from strings further more.

```csharp
SexpConvert.ToValue(123)  // an SValue containing number 123
SexpConvert.ToObject<int>(new SValue(123)) // int 123

SexpConvert.Serialize(123) //  "123"
SexpConvert.Deserialize<int>("123") // int 123
```

## Arrays, Lists and Dictionaries

Arrays will simply be converted to sexp lists.

If a type is inherited from ```IDictionary<TKey, TValue>```, an instance of it will be converted to a list of pairs, the ```Car``` and ```Cdr``` of each pair are the key and the value of each entry in the dictionary, converted as type ```TKey``` and ```TValue```.

If a type is inherited from ```ICollection<T>```, an instance of it will be converted to a list. The elements in the collection is converted as type ```T```.

If a type is inherited from non-generic ```IDictionary``` or ```IList```, the convertion is similar to the generic version, but the elements are converted as ```object```, see polymorphic convertions below.

```csharp
SexpConvert.Serialize(new int[] { 1, 2, 3 })
// (1 2 3)

SexpConvert.Serialize(new Dictionary<string, int>{ ["foo"] = 4, ["bar"] = 5 })
// (("foo" . 4) ("bar" . 5))
```

## Enums

Enums will be converted to numbers representing their values. If the enum type has attribute ```[SexpSymbolEnum]```, its instances will be converted to symbols of value names, throws if name not found.

If the enum type is a ```[Flags]``` enum, instead of a symbol an instance of it will be converted to a list of symbols, containing members set by the instance, throws if the value cannot be represented with enum members.

```csharp
SexpConvert.Serialize(TestEnum.A | TestEnum.B) // (A B)
SexpConvert.Deserialize<TestEnum>(9999) // throws SexpConvertException
```

## ValueTuples

ValueTuples will be converted to list of tuple elements. Available if ValueTuple is supported (net47/netstandard2.0 and higher).

```csharp
SexpConvert.Serialize((1, "str")) // (1 "str")
SexpConvert.Deserialize<(int, string)>("(1 \"str\"") // (1, "str")
```

## Custom types

If a type is marked as ```[SexpAssociationList]```, an instance of it will be converted to an association-list, with the ```AssociationListStyle``` specified by the attribute. The list contains properties and fields of the instance. Members marked as ```[SexpIgnore]``` will be ignored. Use ```[SexpMember]``` attribute to specify the name and order of the member. If any member has its order specified, all members should has positive orders specified.

Formatting attributes such as ```[SexpNumberFormatting]``` can be applied to members with corresponding types.

If a property is read-only, it will be included in the converted ```SValue``` but will not be converted back. Similarly, write-only properties will not be included in ```SValue```s but will be converted back if the source ```SValue``` has the key-value pair.

```csharp
[SexpAsAssociationList]
class Point
{
    [SexpMember("x", Order = 1)]
    public int X { get; set; }

    [SexpMember("y", Order = 2)]
    [SexpNumberFormatting(Radix = NumberRadix.Binary)]
    public int Y;

    [SexpIgnore]
    private int _ignoreMe;
}
SexpConvert.Serialize(new Point { X = 1, Y = 2 }) // ((x . 1) (y . #b10))

[SexpAsAssociationList(Style = AssociationListStyle.Flat)]
...
// (x 1 y 2)

[SexpAsAssociationList(Style = AssociationListStyle.ListOfList)]
// ((x 1) (y 2))
```

If a type is marked as ```[SexpAsList]```, its value will be converted to a list. The member values will be sequentially included in the list. Read-only properties and write-only properties are all ignored.

__NOTE__: Only use this feature for simple types. The order of members in a complex type is hard to keep. ```[SexpAsList]``` atrribute rejects types that have both properties and fields to be converted but have not specified orders of them.

```csharp
[SexpAsList]
class PropertyPoint
{
    public int X { get; set; }
    public int Y { get; set; }
}
SexpConvert.Serialize(new PropertyPoint { X = 1, Y = 2 }) // (1 2)

[SexpAsList]
class FieldPoint
{
    public int x;
    public int y;
}
SexpConvert.Serialize(new FieldPoint { x = 1, y = 2 }) // (1 2)

[SexpAsList]
class MessPoint
{
    public int X { get; set; }
    public int y;
}
SexpConvert.Serialize(new MessPoint { X = 1, y = 2 }) // throws SexpConvertException
```

## Polymorphic convertion (Experimental)

When specified type of a convertion is not the actual type of the instance, the result ```SValue``` will be a pair. This will happen in several situations such as converting an array of sub types, converting members with type ```object```, converting a variable whose type is abstract or interface, etc.

In this situation, a type-id will be added before the actual type converter generated value, shapes a pair. A type-id is normally a ```TypeIdentifier``` instance, which is a special ```SValue``` that starts with "#t:" followed by an identifier, specifying the type of the instance.

```csharp
[SexpAsList] class TypeWithObject { public object o; }

SexpConvert.Serialize(new TypeWithObject{ o = 123 })
// ((#t:|System.Int32, System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e| . 123))
```

```TypeIdentifier```s is generated by ```TypeResolver```s. Default ```TypeResolver``` just get the assembly qualified name of the type and convert it to an ```TypeIdentifier```.

```TypeResolver``` type can be specified by ```[SexpCustomTypeResolver]```. The resolver type should inherit ```TypeResolver```, implements ```Resolve(SValue)``` and ```GetTypeId(Type)``` methods. A simple lookup resolver can be implememted by inheriting ```LookupTypeResolver```, adding mappings of ```TypeIdentifier```s and ```Type```s.

```csharp
class MyResolver : LookupTypeResolver
{
    public MyResolver()
    {
        Add("+", typeof(Add));
        Add("-", typeof(Sub));
        ...
    }
}

[CustomTypeResolver(typeof(MyResolver))]
class MyType { ... }

SexpConvert.Serialize<MyType>(new Add(...)) // (#t:+ ...)
```

More generally, type-ids can be any other ```SValue``` types besides ```TypeIdentifier```. If a ```TypeResolver``` yields type-ids that are not ```TypeIdentifier```, it should override ```GeneralTypeIdentifier``` property and returns ```true```. In this situation, type-id will always be written dispite the actual type of instance, because there will be no way for default converters to distinguish between type-ids and normal data.

The ```LookupTypeResolver``` provides ```AddGeneral()``` method to add any ```SValue```s as type-id. If the type-id added is not ```TypeIdentifier```, it will automatically modify ```GeneralTypeIdentifier``` to ```true```.

```csharp
class MyGeneralResolver : LookupTypeResolver
{
    public MyGeneralResolver()
    {
        AddGeneral(Symbol.FromString("+"), typeof(Add));
        AddGeneral(Symbol.FromString("-"), typeof(Sub));
        ...
    }
}

[CustomTypeResolver(typeof(MyGeneralResolver))]
class MyType { ... }

SexpConvert.Serialize<MyType>(new Add(...)) // (+ ...)
```

## Custom converter

```SexpConvert``` just wraps ```SexpConverter```s. A type can have a custom converter if marked by ```[SexpCustomConverter]```.

For simple uses, ```ToObject()``` and ```ToValue()``` should be implemented by the custom converter. They handle instances of exactly the type specified (i.e. without type-id).

For custom type handling, override ```ToObjectWithTypeCheck()``` and ```ToValueWithTypeCheck()```. For example, just throw errors in these two methods to forbid polymorphically converting.
