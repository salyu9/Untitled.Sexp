using System;
using System.Collections.Generic;
using System.Linq;
using Untitled.Sexp.Attributes;
using Untitled.Sexp.Formatting;
using Untitled.Sexp.Utilities;

namespace Untitled.Sexp.Conversion
{
    internal class AsAssociationListConverter : MembersConverter
    {
        private ListFormatting? _innerFormatting;

        private AssociationListStyle _style;

        private bool _multiline;

        private Dictionary<string, MemberHolder> _members;

        public AsAssociationListConverter(Type type, SexpAsAssociationListAttribute? attribute)
            : base(type)
        {
            var innerParentheses = attribute?.InnerParentheses ?? default;
            if (innerParentheses != default)
            {
                _innerFormatting = new ListFormatting { Parentheses = innerParentheses };
            }

            _multiline = attribute?.Multiline ?? true;

            _style = attribute?.Style ?? default;

            _members = GetMembers().ToDictionary(m => m.Name);
        }

        public override object? ToObject(SValue value)
        {
            if (!value.IsList) throw new SexpConvertException(_type, value);

            var result = Activator.CreateInstance(_type);

            using var enumerator = value.AsEnumerable().GetEnumerator();
            while (enumerator.MoveNext())
            {
                SValue k;
                SValue v;
                var current = enumerator.Current;
                switch (_style)
                {
                    case AssociationListStyle.ListOfPairs:
                        {
                            if (!current.IsPair) throw new SexpConvertException($"invalid elem type {current.Type}, expecting pair", _type, value);
                            var pair = current.AsPair();
                            k = pair._car;
                            v = pair._cdr;
                        }
                        break;
                    case AssociationListStyle.ListOfLists:
                        {
                            if (!current.IsPair) throw new SexpConvertException($"invalid elem type, expecting key-value list", _type, value);
                            var pair = current.AsPair();
                            k = pair._car;
                            if (!pair._cdr.IsPair) throw new SexpConvertException($"invalid elem type, expecting key-value list", _type, value);
                            var cdrPair = pair._car.AsPair();
                            v = cdrPair._car;
                            if (!cdrPair._cdr.IsNull) throw new SexpConvertException($"invalid elem type, expecting key-value list", _type, value);
                        }
                        break;
                    default: // as flat
                        {
                            k = current;
                            if (!enumerator.MoveNext()) throw new SexpConvertException($"expecting value in pairs", _type, value);
                            v = enumerator.Current;
                        }
                        break;
                }

                if (!k.IsSymbol) throw new SexpConvertException($"expecting symbol as key, found {k.Type}", _type, value);

                if (!_members.TryGetValue(k.AsSymbol().Name, out var member)) continue;

                if (!member.CanWrite) continue;

                member.Set(result, SexpConvert.ToObject(member.Type, v));
            }
            return result;
        }

        public override SValue ToValue(object obj)
        {
            var builder = _multiline
                ? new ListBuilder(new ListFormatting
                {
                    LineElemsCount = _style == AssociationListStyle.Flat ? 2 : 1,
                    LineExtraSpaces = 1,
                })
                : new ListBuilder();

            foreach (var kv in _members)
            {
                var member = kv.Value;
                if (!member.CanRead) continue;

                var k = Symbol.FromString(kv.Key);
                var v = SexpConvert.ToValue(member.Type, member.Get(obj), member.Formatting);
                switch (_style)
                {
                    case AssociationListStyle.ListOfPairs:
                        {
                            builder.Add(new SValue(k, v, _innerFormatting));
                        }
                        break;
                    case AssociationListStyle.ListOfLists:
                        {
                            builder.Add(new SValue(k, new Pair(v, SValue.Null), _innerFormatting));
                        }
                        break;
                    default: // as flat
                        {
                            builder.Add(k);
                            builder.Add(v);
                        }
                        break;
                }
            }

            return builder.ToValue();
        }
    }
}
