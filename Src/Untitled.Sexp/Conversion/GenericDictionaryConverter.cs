using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Untitled.Sexp.Formatting;
using Untitled.Sexp.Utilities;

namespace Untitled.Sexp.Conversion
{
    internal class GenericDictionaryConverter : SexpConverter
    {
        private readonly Type _type;
        private readonly Type _keyType;
        private readonly Type _valueType;
        private readonly SexpConverter _keyConverter;
        private readonly SexpConverter _valueConverter;
        private readonly MethodInfo _addMethod;
        private readonly PropertyInfo _keyProperty;
        private readonly PropertyInfo _valueProperty;

        public GenericDictionaryConverter(Type type, Type interfaceType)
        {
            _type = type;
            var genericArgs = interfaceType.GetGenericArguments();
            _keyType = genericArgs[0];
            _valueType = genericArgs[1];
            _keyConverter = SexpConvert.GetConverter(_keyType);
            _valueConverter = SexpConvert.GetConverter(_valueType);
            _addMethod = interfaceType.GetMethod("Add", genericArgs);

            var kvType = interfaceType.GetInterface(typeof(System.Collections.Generic.IEnumerable<>).FullName).GetGenericArguments()[0];
            _keyProperty = kvType.GetProperty("Key");
            _valueProperty = kvType.GetProperty("Value");
        }

        public override bool CanConvert(Type type)
            => type == _type;

        public override object? ToObjectExactType(SValue value)
        {
            var dict = Activator.CreateInstance(_type);
            foreach (var kv in value.AsEnumerable<Pair>())
            {
                _addMethod.Invoke(dict, new object[] { _keyConverter.ToObject(kv._car)!, _keyConverter.ToObject(kv._cdr)! });
            }
            return dict;
        }

        public override SValue ToValueExactType(object obj)
        {
            var builder = new ListBuilder();
            foreach (var kv in (IEnumerable)obj)
            {
                builder.Add(new Pair(
                    _keyConverter.ToValue(_keyType, _keyProperty.GetValue(kv, null)),
                    _keyConverter.ToValue(_valueType, _valueProperty.GetValue(kv, null))
                ));
            }
            return builder.ToValue();
        }
    }
}
