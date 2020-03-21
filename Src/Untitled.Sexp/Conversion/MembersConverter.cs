using System;
using System.Reflection;
using System.Linq;
using Untitled.Sexp.Formatting;
using System.Collections.Generic;
using Untitled.Sexp.Attributes;
using System.Runtime.CompilerServices;

namespace Untitled.Sexp.Conversion
{
    internal abstract class MembersConverter : SexpConverter
    {
        protected abstract class MemberHolder
        {
            public Type Type { get; }
            public string Name { get; }
            public SValueFormatting? Formatting { get; }
            public int Order { get; }

            protected MemberHolder(Type type, string name, SValueFormatting? formatting, int order)
            {
                Type = type;
                Name = name;
                Formatting = formatting;
                Order = order;
            }

            public abstract object? Get(object target);

            public abstract void Set(object target, object? value);

            public abstract bool CanRead { get; }

            public abstract bool CanWrite { get; }
        }

        protected sealed class PropertyHolder : MemberHolder
        {
            private MethodInfo? _getter;

            private MethodInfo? _setter;

            public PropertyHolder(PropertyInfo info, string? name, SValueFormatting? formatting, int order)
                : base(info.PropertyType, name ?? info.Name, formatting, order)
            {
                _getter = info.GetGetMethod(true);
                _setter = info.GetSetMethod(true);
            }

            public override object? Get(object target)
                => _getter?.Invoke(target, null);

            public override void Set(object target, object? value)
                => _setter?.Invoke(target, new object?[] { value });

            public override bool CanRead
                => _getter != null;

            public override bool CanWrite
                => _setter != null;
        }

        protected sealed class FieldHolder : MemberHolder
        {
            private FieldInfo _info;

            public FieldHolder(FieldInfo info, string? name, SValueFormatting? formatting, int order)
                : base(info.FieldType, name ?? info.Name, formatting, order)
            {
                _info = info;
            }

            public override object? Get(object target)
                => _info.GetValue(target);

            public override void Set(object target, object? value)
                => _info.SetValue(target, value);

            public override bool CanRead
                => true;

            public override bool CanWrite
                => true;
        }

        protected readonly Type _type;

        protected MembersConverter(Type type)
        {
            _type = type;
        }

        protected List<MemberHolder> GetMembers()
        {
            var results = new List<MemberHolder>();

            var hasOrder = false;

            var query = from member in _type.GetMembers(BindingFlags.Public
                                                      | BindingFlags.NonPublic
                                                      | BindingFlags.Instance)
                        where member.MemberType == MemberTypes.Property || member.MemberType == MemberTypes.Field
                        orderby member.MetadataToken  // non-official documented declaration order uses: https://github.com/dotnet/runtime/issues/19732
                        select member;

            foreach (var member in query)
            {
                var ignored = false;
                string? name = null;
                SexpFormattingAttribute? formattingAttribute = null;
                int order = -1;
                foreach (var attr in member.GetCustomAttributes(false))
                {
                    if (attr is SexpIgnoreAttribute)
                    {
                        ignored = true;
                        break;
                    }
                    else if (attr is CompilerGeneratedAttribute)
                    {
                        ignored = true;
                        break;
                    }
                    else if (attr is SexpMemberAttribute memberAttr)
                    {
                        name = memberAttr.Name;
                        order = memberAttr.Order;
                        if (order >= 0) hasOrder = true;
                    }
                    else if (attr is SexpFormattingAttribute formattingAttr)
                    {
                        formattingAttribute = formattingAttr;
                    }
                }
                if (ignored) continue;

                if (member is PropertyInfo property)
                {
                    if (property.IsSpecialName || property.GetIndexParameters().Length > 0)
                    {
                        continue;
                    }
                    if (formattingAttribute != null && !formattingAttribute.AcceptType(property.PropertyType))
                    {
                        throw new SexpConvertException($"Cannot apply {formattingAttribute.GetType()} to property {_type.Name}.{property.Name}");
                    }
                    results.Add(new PropertyHolder(property, name, formattingAttribute?.Formatting, order));
                }
                else if (member is FieldInfo field)
                {
                    if (field.IsSpecialName)
                    {
                        continue;
                    }
                    if (formattingAttribute != null && !formattingAttribute.AcceptType(field.FieldType))
                    {
                        throw new SexpConvertException($"Cannot apply {formattingAttribute.GetType()} to field {_type.Name}.{field.Name}");
                    }
                    results.Add(new FieldHolder(field, name, formattingAttribute?.Formatting, order));
                }
            }

            // orderby SexpMember.Order or
            if (hasOrder)
            {
                if (results.Any(m => m.Order < 0))
                {
                    throw new SexpConvertException($"Not all members have orders in type {_type}, please specify all or none.");
                }

                results.Sort((a, b) => a.Order.CompareTo(b.Order));
            }

            return results;
        }

        public override bool CanConvert(Type type)
            => type == _type;
    }
}
