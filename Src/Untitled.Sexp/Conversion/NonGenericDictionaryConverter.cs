using System;
using System.Collections;
using Untitled.Sexp.Formatting;
using Untitled.Sexp.Utilities;

namespace Untitled.Sexp.Conversion
{
    internal class NonGenericDictionaryConverter : SexpConverter
    {
        private readonly Type _type;
        private static readonly Type _objectType = typeof(object);
        private readonly SexpConverter _elemConverter;

        public NonGenericDictionaryConverter(Type type)
        {
            _type = type;
            _elemConverter = SexpConvert.GetConverter(_objectType);
        }

        public override bool CanConvert(Type type)
            => type == _type;

        public override object? ToObject(SValue value)
        {
            var dict = (IDictionary)Activator.CreateInstance(_type);
            foreach (var kv in value.AsEnumerable<Pair>())
            {
                dict.Add(_elemConverter.ToObjectWithTypeCheck(kv._car)!, _elemConverter.ToObjectWithTypeCheck(kv._cdr)!);
            }
            return dict;
        }

        public override SValue ToValue(object? obj)
        {
            if (obj == null) return SValue.Null;

            var builder = new ListBuilder();
            foreach (DictionaryEntry kv in (IDictionary)obj)
            {
                builder.Add(new Pair(_elemConverter.ToValueWithTypeCheck(_objectType, kv.Key), _elemConverter.ToValueWithTypeCheck(_objectType, kv.Value)));
            }
            return builder.ToValue();
        }
    }
}
