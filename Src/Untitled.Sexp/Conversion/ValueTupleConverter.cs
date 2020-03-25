using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Untitled.Sexp.Formatting;
using Untitled.Sexp.Utilities;

namespace Untitled.Sexp.Conversion
{
#if !NO_VALUETUPLE

    internal class ValueTupleConverter : SexpConverter
    {
        private readonly Type _type;
        private readonly Type[] _elemTypes;
        private readonly List<Func<object, object>> _elemGetters = new List<Func<object, object>>();

        public ValueTupleConverter(Type type)
        {
            _type = type;
            _elemTypes = type.GetGenericArguments();
            System.Diagnostics.Debug.Assert(_elemTypes.Length <= 8);

            for (int i = 0; i < (_elemTypes.Length == 8 ? 7 : _elemTypes.Length); ++i)
            {
                var getter = type.GetField("Item" + (i + 1));
                _elemGetters.Add(value => getter.GetValue(value));
            }
            if (_elemTypes.Length == 8)
            {
                var getter = type.GetField("Rest");
                _elemGetters.Add(value => getter.GetValue(value));
            }
        }

        public override bool CanConvert(Type type)
            => type == _type;

        public override object? ToObject(SValue value)
        {
            var args = new object?[_elemTypes.Length];
            var current = value;
            if (!current.IsList)
            {
                throw new SexpConvertException(_type, value);
            }
            for (var i = 0; i < (_elemTypes.Length == 8 ? 7 : _elemTypes.Length); ++i)
            {
                if (current.IsNull) throw new SexpConvertException(_type, value);

                var pair = current.AsPair();
                args[i] = SexpConvert.ToObject(_elemTypes[i], pair.Car);
                current = pair.Cdr;
            }
            if (_elemTypes.Length == 8)
            {
                if (current.IsNull) throw new SexpConvertException(_type, value);

                args[7] = SexpConvert.ToObject(_elemTypes[7], current);
            }
            return Activator.CreateInstance(_type, args);
        }

        public override SValue ToValue(object? obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            var builder = new ListBuilder();
            for (int i = 0; i < (_elemTypes.Length == 8 ? 7 : _elemTypes.Length); ++i)
            {
                builder.Add(SexpConvert.ToValue(_elemTypes[i], _elemGetters[i](obj)));
            }
            if (_elemTypes.Length == 8)
            {
                builder.AddRange(SexpConvert.ToValue(_elemTypes[7], _elemGetters[7](obj)));
            }
            return builder.ToValue();
        }
    }
#endif // !VALUE_TUPLE
}
