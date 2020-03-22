using System;
using System.Collections.Generic;
using System.Reflection;
using Untitled.Sexp.Formatting;
using Untitled.Sexp.Utilities;

namespace Untitled.Sexp.Conversion
{
    internal class GenericDictionaryConverter<TKey, TValue> : SexpConverter
    {
        private readonly Type _type;
        private readonly Type _keyType;
        private readonly Type _valueType;
        private readonly SexpConverter _keyConverter;
        private readonly SexpConverter _valueConverter;

        public GenericDictionaryConverter(Type type)
        {
            _type = type;
            _keyType = typeof(TKey);
            _valueType = typeof(TValue);
            _keyConverter = SexpConvert.GetConverter(_keyType);
            _valueConverter = SexpConvert.GetConverter(_valueType);
        }

        public override bool CanConvert(Type type)
            => type == _type;

        public override object? ToObject(SValue value)
        {
            var dict = (IDictionary<TKey, TValue>)Activator.CreateInstance(_type);
            foreach (var kv in value.AsEnumerable<Pair>())
            {
                dict.Add((TKey)_keyConverter.ToObjectWithTypeCheck(kv._car)!, (TValue)_valueConverter.ToObjectWithTypeCheck(kv._cdr)!);
            }
            return dict;
        }

        public override SValue ToValue(object obj)
        {
            var builder = new ListBuilder();
            foreach (var kv in (IDictionary<TKey, TValue>)obj)
            {
                builder.Add(new Pair(_keyConverter.ToValueWithTypeCheck(_keyType, kv.Key!), _valueConverter.ToValueWithTypeCheck(_valueType, kv.Value)));
            }
            return builder.ToValue();
        }
    }
}
