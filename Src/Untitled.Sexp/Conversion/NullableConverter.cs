using System;
using Untitled.Sexp.Formatting;

namespace Untitled.Sexp.Conversion
{
    internal class NullableConverter : SexpConverter
    {
        private readonly Type _type;
        private readonly Type _elemType;
        private SexpConverter _elemConverter;

        public NullableConverter(Type type)
        {
            System.Diagnostics.Debug.Assert(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));

            _type = type;
            _elemType = Nullable.GetUnderlyingType(type);
            _elemConverter = SexpConvert.GetConverter(_elemType);
        }

        public override bool CanConvert(Type type)
            => type == _type;

        public override object? ToObjectExactType(SValue value)
        {
            if (value.IsNull) return null;

            return _elemConverter.ToObject(value);
        }

        public override SValue ToValue(Type type, object? obj)
        {
            if (obj == null) return SValue.Null;

            if (obj.GetType() != _elemType) throw new SexpConvertException(type, obj);

            return ToValueExactType(obj);
        }

        public override SValue ToValueExactType(object obj)
        {
            return _elemConverter.ToValue(_elemType, obj);
        }
    }
}
