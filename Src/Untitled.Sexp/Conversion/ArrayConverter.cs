using System;
using Untitled.Sexp.Formatting;
using Untitled.Sexp.Utilities;

namespace Untitled.Sexp.Conversion
{
    internal class ArrayConverter : SexpConverter
    {
        private readonly Type _type;

        private readonly Type _elemType;

        private SexpConverter _elemConverter;

        public ArrayConverter(Type type)
        {
            System.Diagnostics.Debug.Assert(type.IsArray);
            _type = type;
            _elemType = type.GetElementType();
            _elemConverter = SexpConvert.GetConverter(_elemType);
        }

        public override bool CanConvert(Type type)
            => type == _type;

        public override object? ToObjectExactType(SValue value)
        {
            var array = Array.CreateInstance(_elemType, value.Length);
            int i = 0;
            foreach (var elemValue in value.AsEnumerable())
            {
                array.SetValue(_elemConverter.ToObject(elemValue), i++);
            }
            return array;
        }

        public override SValue ToValueExactType(object obj)
        {
            var array = (Array)obj;
            
            var builder = new ListBuilder();
            foreach (var elem in array)
            {
                builder.Add(_elemConverter.ToValue(_elemType, elem));
            }

            return builder.ToValue();
        }
    }
}
