using System;
using System.Collections.Generic;
using System.Linq;
using Untitled.Sexp.Formatting;
using Untitled.Sexp.Utilities;

namespace Untitled.Sexp.Conversion
{
    internal sealed class AsListConverter : MembersConverter
    {
        private List<MemberHolder> _members;

        public AsListConverter(Type type)
            : base(type)
        {
            _members = GetMembers();

            // if order is not specified manually, MetadataToken order will be the member order.
            // therefore all members should be the same type.
            if (_members.Any(m => m.Order < 0))
            {
                var hasField = false;
                var hasProperty = false;
                foreach (var member in GetMembers())
                {
                    if (member is PropertyHolder) hasProperty = true;
                    else hasField = true;
                }
                if (hasField && hasProperty)
                {
                    throw new SexpConvertException($"AsList types with default order should have only properties or only fields.");
                }
            }
        }

        public override object? ToObjectExactType(SValue value)
        {
            if (!value.IsList) throw new SexpConvertException(_type, value);
            if (value.Length != _members.Count)
            {
                throw new SexpConvertException($"list length not match, expecting {_members.Count}", _type, value);
            }
            var result = Activator.CreateInstance(_type);
            var i = 0;
            foreach (var elem in value.AsEnumerable())
            {
                var member = _members[i++];
                member.Set(result, SexpConvert.ToObject(member.Type, elem));
            }
            return result;
        }

        public override SValue ToValueExactType(object obj)
        {
            var builder = new ListBuilder();
            foreach (var member in _members)
            {
                builder.Add(SexpConvert.ToValue(member.Type, member.Get(obj), member.Formatting));
            }

            return builder.ToValue();
        }
    }
}
