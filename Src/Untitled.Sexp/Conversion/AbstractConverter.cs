using System;
using Untitled.Sexp.Formatting;
using Untitled.Sexp.Utilities;

namespace Untitled.Sexp.Conversion
{
    internal class AbstractConverter : SexpConverter
    {
        private readonly Type _type;

        private bool _isObject;

        public AbstractConverter(Type type)
        {
            System.Diagnostics.Debug.Assert(type == typeof(object) || type.IsAbstract);
            _type = type;
            _isObject = type == typeof(object);
        }

        public override bool CanConvert(Type type)
            => type == _type;

        public override object? ToObject(SValue value)
        {
            if (_isObject) return new object();

            throw new SexpConvertException($"Abstract converter cannot handle type {_type}");
        }

        public override SValue ToValue(object obj)
        {
            if (_isObject) return SValue.Null;

            throw new SexpConvertException($"Abstract converter cannot handle type {_type}");
        }
    }
}
