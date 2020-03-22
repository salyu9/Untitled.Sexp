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

        public override object? ToObject(SValue value)
        {
            if (value.IsNull) return null;

            return _elemConverter.ToObjectWithTypeCheck(value);
        }

        public override SValue ToValueWithTypeCheck(Type type, object? obj)
        {
            if (obj == null) return SValue.Null;

            if (obj.GetType() != _elemType) throw new SexpConvertException(type, obj);

            return ToValue(obj);
        }

        public override SValue ToValue(object obj)
        {
            return _elemConverter.ToValueWithTypeCheck(_elemType, obj);
        }
    }
}
