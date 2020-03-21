using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Untitled.Sexp.Formatting;
using Untitled.Sexp.Utilities;

namespace Untitled.Sexp.Conversion
{
    internal class GenericCollectionConverter : SexpConverter
    {
        private readonly Type _type;
        private readonly Type _elemType;
        private readonly SexpConverter _elemConverter;

        private readonly MethodInfo _addMethod;

        public GenericCollectionConverter(Type type, Type interfaceType)
        {
            _type = type;
            var genericArgs = interfaceType.GetGenericArguments();
            _elemType = genericArgs[0];
            _elemConverter = SexpConvert.GetConverter(_elemType);
            _addMethod = type.GetMethod("Add", genericArgs);
        }

        public override bool CanConvert(Type type)
            => type == _type;

        public override object? ToObjectExactType(SValue value)
        {
            var collection = Activator.CreateInstance(_type);
            foreach (var v in value.AsEnumerable())
            {
                _addMethod.Invoke(collection, new object[]{ _elemConverter.ToObject(v)! });
            }
            return collection;
        }

        public override SValue ToValueExactType(object obj)
        {
            var builder = new ListBuilder();
            foreach (var elem in (IEnumerable)obj)
            {
                builder.Add(_elemConverter.ToValue(_elemType, elem));
            }
            return builder.ToValue();
        }
    }
}
