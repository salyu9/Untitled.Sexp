using System;
using System.Collections;
using Untitled.Sexp.Formatting;
using Untitled.Sexp.Utilities;

namespace Untitled.Sexp.Conversion
{
    internal class NonGenericListConverter : SexpConverter
    {
        private readonly Type _type;
        private static readonly Type _objectType = typeof(object);
        private readonly SexpConverter _elemConverter;

        public NonGenericListConverter(Type type)
        {
            _type = type;
            _elemConverter = SexpConvert.GetConverter(_objectType);
        }

        public override bool CanConvert(Type type)
            => type == _type;

        public override object? ToObject(SValue value)
        {
            var collection = (IList)Activator.CreateInstance(_type);
            foreach (var v in value.AsEnumerable())
            {
                collection.Add(_elemConverter.ToObjectWithTypeCheck(v)!);
            }
            return collection;
        }

        public override SValue ToValue(object? obj)
        {
            if (obj == null) return SValue.Null;

            var builder = new ListBuilder();
            foreach (var elem in (IList)obj)
            {
                builder.Add(_elemConverter.ToValueWithTypeCheck(_objectType, elem));
            }
            return builder.ToValue();
        }
    }
}
