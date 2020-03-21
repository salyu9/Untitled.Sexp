using System;
using System.Collections.Generic;
using Untitled.Sexp.Formatting;
using Untitled.Sexp.Utilities;

namespace Untitled.Sexp.Conversion
{
    internal class GenericCollectionConverter<T> : SexpConverter
    {
        private readonly Type _type;
        private readonly Type _elemType;
        private readonly SexpConverter _elemConverter;

        public GenericCollectionConverter(Type type)
        {
            _type = type;
            _elemType = typeof(T);
            _elemConverter = SexpConvert.GetConverter(_elemType);
        }

        public override bool CanConvert(Type type)
            => type == _type;

        public override object? ToObjectExactType(SValue value)
        {
            var collection = (ICollection<T>)Activator.CreateInstance(_type);
            foreach (var v in value.AsEnumerable())
            {
                collection.Add((T)_elemConverter.ToObject(v)!);
            }
            return collection;
        }

        public override SValue ToValueExactType(object obj)
        {
            var builder = new ListBuilder();
            foreach (var elem in (ICollection<T>)obj)
            {
                builder.Add(_elemConverter.ToValue(_elemType, elem));
            }
            return builder.ToValue();
        }
    }
}
