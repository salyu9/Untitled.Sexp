using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Untitled.Sexp.Conversion;
using System.Reflection;
using Untitled.Sexp.Attributes;
using Untitled.Sexp.Formatting;
using System.Collections;

namespace Untitled.Sexp
{
    /// <summary>
    /// Convert sexp values to objects and vice versa.
    /// </summary>
    public static class SexpConvert
    {
        private static readonly Dictionary<Type, SexpConverter> ConverterTable = new Dictionary<Type, SexpConverter>
        {
            [typeof(object)] = new AbstractConverter(typeof(object)),

            [typeof(SValue)] = new SimpleConverter<SValue>(v => v, v => v),

            [typeof(bool)] = new SimpleConverter<bool>(v => v.AsBoolean(), o => new SValue(o)),

            [typeof(byte)] = new SimpleConverter<byte>(v => checked((byte)v.AsLong()), o => new SValue(o)),
            [typeof(sbyte)] = new SimpleConverter<sbyte>(v => checked((sbyte)v.AsLong()), o => new SValue(o)),
            [typeof(short)] = new SimpleConverter<short>(v => checked((short)v.AsLong()), o => new SValue(o)),
            [typeof(ushort)] = new SimpleConverter<ushort>(v => checked((ushort)v.AsLong()), o => new SValue(o)),
            [typeof(int)] = new SimpleConverter<int>(v => checked((int)v.AsLong()), o => new SValue(o)),
            [typeof(uint)] = new SimpleConverter<uint>(v => checked((uint)v.AsLong()), o => new SValue(o)),
            [typeof(long)] = new SimpleConverter<long>(v => checked((long)v.AsLong()), o => new SValue(o)),
            [typeof(ulong)] = new SimpleConverter<ulong>(v => checked((ulong)v.AsLong()), o => new SValue(o)),
            [typeof(float)] = new SimpleConverter<float>(v => (float)v.AsDouble(), o => new SValue(o)),
            [typeof(double)] = new SimpleConverter<double>(v => v.AsDouble(), o => new SValue(o)),

            [typeof(char)] = new SimpleConverter<char>(v => v.AsChar(), o => new SValue(o)),
            [typeof(string)] = new SimpleConverter<string>(v => v.AsString(), o => new SValue(o)),
            [typeof(byte[])] = new SimpleConverter<byte[]>(v => v.AsBytes().ToArray(), o => new SValue(o)),

            [typeof(Symbol)] = new SimpleConverter<Symbol>(v => v.AsSymbol(), o => new SValue(o)),
            [typeof(Pair)] = new SimpleConverter<Pair>(v => v.AsPair(), o => new SValue(o)),

            [typeof(TypeIdentifier)] = new SimpleConverter<TypeIdentifier>(v => v.AsTypeIdentifier(), o => new SValue(o)),
        };

        private class SimpleConverter<T> : SexpConverter
        {
            private readonly Func<SValue, T> _toObjectFunc;
            private readonly Func<T, SValue> _toValueFunc;
            private static readonly Type _type = typeof(T);

            public SimpleConverter(Func<SValue, T> toObjectFunc, Func<T, SValue> toValueFunc)
            {
                _toObjectFunc = toObjectFunc;
                _toValueFunc = toValueFunc;
            }

            public override object ToObject(SValue value)
            {
                try
                {
                    return _toObjectFunc(value)!;
                }
                catch (Exception inner)
                {
                    throw new SexpConvertException(typeof(T), value, inner);
                }
            }

            public override SValue ToValue(object? obj)
            {
                try
                {
                    return _toValueFunc((T)obj!);
                }
                catch (Exception inner)
                {
                    throw new SexpConvertException(typeof(T), obj!, inner);
                }
            }

            public override bool CanConvert(Type type)
                => type == _type;
        }

        private static readonly Type NullableType = typeof(Nullable<>);
        private static readonly Type IListType = typeof(IList);
        private static readonly Type GenericICollectionType = typeof(ICollection<>);
        private static readonly Type IDictionaryType = typeof(IDictionary);
        private static readonly Type GenericIDictionaryType = typeof(IDictionary<,>);

        /// <summary>
        /// Get converter for specified type.
        /// </summary>
        public static SexpConverter GetConverter(Type type)
        {
            if (ConverterTable.TryGetValue(type, out var converter)) return converter;

            var asList = false;
            Type? customConverterType = null;
            var attrMatched = 0;
            SexpAsAssociationListAttribute? assocListAttrib = null;
            TypeResolver? customTypeResolver = null;
            foreach (var attribute in type.GetCustomAttributes(true))
            {
                if (attribute is SexpCustomTypeResolverAttribute typeResolverAttr)
                {
                    customTypeResolver = (TypeResolver)Activator.CreateInstance(typeResolverAttr.ResolverType);
                }
                if (attribute is SexpCustomConverterAttribute customConverterAttr)
                {
                    customConverterType = customConverterAttr.ConverterType;
                    ++attrMatched;
                }
                else if (attribute is SexpAsListAttribute asListAttr)
                {
                    asList = true;
                    ++attrMatched;
                }
                else if (attribute is SexpAsAssociationListAttribute asAssocListAttr)
                {
                    assocListAttrib = asAssocListAttr;
                    ++attrMatched;
                }
            }
            if (attrMatched > 1) throw new SexpException($"Cannot apply multiple convertion attributes to the same type {type.FullName}");

            SexpConverter newConverter;
            if (customConverterType != null)
            {
                newConverter = (SexpConverter)Activator.CreateInstance(customConverterType);
            }
            else if (assocListAttrib != null)
            {
                newConverter = new AsAssociationListConverter(type, assocListAttrib);
            }
            else if (asList)
            {
                newConverter = new AsListConverter(type);
            }
            else if (type.IsAbstract) // includes interfaces
            {
                newConverter = new AbstractConverter(type);
            }
            else if (type.IsArray)
            {
                newConverter = new ArrayConverter(type);
            }
            else if (type.IsEnum)
            {
                newConverter = new EnumConverter(type);
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == NullableType)
            {
                newConverter = new NullableConverter(type);
            }
#if !AOT
            else if (type.GetInterface(typeof(IDictionary<,>).FullName) != null)
            {
                var dictInterface = type.GetInterface(typeof(IDictionary<,>).FullName);
                var converterType = typeof(GenericDictionaryConverter<,>).MakeGenericType(dictInterface.GetGenericArguments());
                newConverter = (SexpConverter)Activator.CreateInstance(converterType, type);
            }
            else if (type.GetInterface(typeof(ICollection<>).FullName) != null)
            {
                var collectionInterface = type.GetInterface(typeof(ICollection<>).FullName);
                var converterType = typeof(GenericCollectionConverter<>).MakeGenericType(collectionInterface.GetGenericArguments());
                newConverter = (SexpConverter)Activator.CreateInstance(converterType, type);
            }
#else
            else if (type.GetInterface(typeof(IDictionary<,>).FullName) != null)
            {
                var dictInterface = type.GetInterface(typeof(IDictionary<,>).FullName);
                newConverter = (SexpConverter)Activator.CreateInstance(converterType, type);
            }
            else if (type.GetInterface(typeof(ICollection<>).FullName) != null)
            {
                var collectionInterface = type.GetInterface(typeof(ICollection<>).FullName);
                newConverter = (SexpConverter)Activator.CreateInstance(converterType, type);
            }
#endif
            else if (IDictionaryType.IsAssignableFrom(type))
            {
                newConverter = new NonGenericDictionaryConverter(type);
            }
            else if (IListType.IsAssignableFrom(type))
            {
                newConverter = new NonGenericListConverter(type);
            }
            else
            {
                newConverter = new AsAssociationListConverter(type, null);
            }

            lock (ConverterTable)
            {
                // double check
                if (ConverterTable.TryGetValue(type, out converter)) return converter;

                ConverterTable.Add(type, newConverter);
                if (customTypeResolver != null) newConverter.TypeResolver = customTypeResolver;
                return newConverter;
            }
        }

        /// <summary>
        /// Convert sexp value to specified type.
        /// </summary>
        public static object? ToObject(Type toType, SValue value)
        {
            return GetConverter(toType).ToObjectWithTypeCheck(value);
        }

        /// <summary>
        /// Convert object to sexp value.
        /// </summary>
        public static SValue ToValue(Type fromType, object? obj, SValueFormatting? formatting = null)
        {
            var value = GetConverter(fromType).ToValueWithTypeCheck(fromType, obj);
            if (value.Formatting != null) value.Formatting.MergeWith(formatting);
            else value.Formatting = formatting;
            return value;
        }

        /// <summary>
        /// Convert sexp value to specified type.
        /// </summary>
        public static T ToObject<T>(SValue value)
            => (T)ToObject(typeof(T), value)!;

        /// <summary>
        /// Convert object to sexp value.
        /// </summary>
        public static SValue ToValue<T>(T obj, SValueFormatting? formatting = null)
            => ToValue(typeof(T), obj, formatting);

        /// <summary>
        /// Serialize object to sexp string.
        /// </summary>
        public static string Serialize<T>(T obj, SValueFormatting? formatting = null, string? newline = null)
            => Serialize(obj, SexpTextWriterSettings.Default, formatting, newline);

        /// <summary>
        /// Serialize object to sexp string.
        /// </summary>
        public static string Serialize<T>(T obj, SexpTextWriterSettings writerSettings, SValueFormatting? formatting = null, string? newline = null)
        {
            using var writer = new StringWriter();
            if (newline != null) writer.NewLine = newline;
            var sexpWriter = new SexpTextWriter(writer, writerSettings);
            Write(sexpWriter, obj, formatting);
            return writer.ToString();
        }

        /// <summary>
        /// Deserialize sexp string to specified typed object.
        /// </summary>
        public static T Deserialize<T>(string s)
            => Deserialize<T>(s, SexpTextReaderSettings.Default);

        /// <summary>
        /// Deserialize sexp string to specified typed object.
        /// </summary>
        public static T Deserialize<T>(string s, SexpTextReaderSettings readerSettings)
        {
            using var reader = new StringReader(s);
            var sexpReader = new SexpTextReader(reader, readerSettings);
            return Read<T>(sexpReader);
        }

        /// <summary>
        /// Read specified typed object from reader.
        /// </summary>
        public static T Read<T>(SexpTextReader reader)
        {
            var value = reader.Read();
            if (value.IsEof) throw new SexpException("Expect a value in reader, found eof");

            return ToObject<T>(value);
        }

        /// <summary>
        /// Write object to writer.
        /// </summary>
        public static void Write<T>(SexpTextWriter writer, T obj, SValueFormatting? formatting = null)
        {
            writer.Write(ToValue(obj, formatting));
        }
    }
}
