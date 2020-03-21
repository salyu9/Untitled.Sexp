using System;
using System.Collections;
using System.Collections.Generic;
using Untitled.Sexp.Formatting;

namespace Untitled.Sexp.Utilities
{
    /// <summary>
    /// Helper class to create list.
    /// </summary>
    public sealed class ListBuilder : IEnumerable<SValue>
    {
        private Pair _root;

        private Pair _current;

        private ListFormatting? _formatting;

        private int _version;

        /// <summary>
        /// Initialize a new <see cref="ListBuilder" />.
        /// </summary>
        public ListBuilder(ListFormatting? formatting = null)
        {
            _root = new Pair(SValue.Null, SValue.Null);
            _current = _root;
            _formatting = formatting;
            _version = 0;
        }

        /// <summary>
        /// Initialize a new <see cref="ListBuilder" /> with a collection of values.
        /// </summary>
        public ListBuilder(IEnumerable<SValue> values, ListFormatting? formatting = null)
            : this(formatting)
        {
            foreach (var v in values) Add(v);
        }

        /// <summary>
        /// Reset the builder to empty state.
        /// </summary>
        public void Reset()
        {
            _root = new Pair(SValue.Null, SValue.Null);
            _current = _root;
            ++_version;
        }

        /// <summary>
        /// Append a sexp value into result list.
        /// </summary>
        /// /// <param name="value">Sexp value to append.</param>
        public ListBuilder Add(SValue value)
        {
            var pair = new Pair(value, SValue.Null);
            _current._cdr = pair;
            _current = pair;
            ++_version;
            return this;
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        /// <summary />
        public IEnumerator<SValue> GetEnumerator()
            => new Enumerator(this);

        /// <summary>
        /// Specialized enumerator for <see cref="ListBuilder" />.
        /// </summary>
        public struct Enumerator : IEnumerator<SValue>
        {
            private ListBuilder _builder;
            private Pair _current;
            private int _version;
            internal Enumerator(ListBuilder builder)
            {
                _builder = builder;
                _current = builder._root;
                _version = builder._version;
            }

            /// <summary />
            public SValue Current
                => _current._car;

            /// <summary />
            object IEnumerator.Current
                => Current;

            /// <summary />
            public void Dispose()
            { }

            /// <summary />
            public bool MoveNext()
            {
                if (_version != _builder._version)
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                if (_current._cdr.IsNull) return false;
                _current = (Pair)_current._cdr;
                return true;
            }

            /// <summary />
            public void Reset()
            {
                if (_version != _builder._version)
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
                _current = _builder._root;
            }
        }

        /// <summary>
        /// Get the result list and reset the builder.
        /// </summary>
        public SValue ToValue()
        {
            var result = _root._cdr;
            Reset();
            result.Formatting = _formatting;
            return result;
        }

    }
}
