using System;
using System.Collections;
using System.Collections.Generic;

namespace Untitled.Sexp.Conversion
{
    /// <summary>
    /// Simple type resolver with lookup table.
    /// </summary>
    public class LookupTypeResolver : TypeResolver, IEnumerable<KeyValuePair<SValue, Type>>
    {
        private readonly Dictionary<SValue, Type> _forwardTable = new Dictionary<SValue, Type>();
        private readonly Dictionary<Type, SValue> _backwardTable = new Dictionary<Type, SValue>();

        private bool _generalTypeId = false;

        /// <summary>
        /// Add a entry.
        /// </summary>
        public void Add(TypeIdentifier id, Type type)
        {
            _forwardTable.Add(id, type);
            _backwardTable.Add(type, id);
        }

        /// <summary>
        /// Add a entry.
        /// </summary>
        public void Add(string name, Type type)
            => Add(TypeIdentifier.FromString(name), type);

        /// <summary>
        /// Add a entry.
        /// </summary>
        public void Add(Type type, TypeIdentifier id)
            => Add(id, type);

        /// <summary>
        /// Add a entry.
        /// </summary>
        public void Add(Type type, string name)
            => Add(name, type);

        /// <summary>
        /// Add a entry with general typeid.
        /// </summary>
        public void AddGeneral(SValue id, Type type)
        {
            _forwardTable.Add(id, type);
            _backwardTable.Add(type, id);
            if (!id.IsTypeIdentifier) _generalTypeId = true;
        }

        /// <summary>
        /// Get type for the type identifier value.
        /// </summary>
        public override Type Resolve(SValue typeid)
        {
            if (_forwardTable.TryGetValue(typeid, out var type)) return type;
            throw new SexpConvertException($"Cannot resolve type {typeid}");
        }

        /// <summary>
        /// Get typeid for the type.
        /// </summary>
        public override SValue GetTypeId(Type type)
        {
            if (_backwardTable.TryGetValue(type, out var id)) return id;
            throw new SexpConvertException($"Cannot get type id of {type}");
        }

        /// <summary />
        public override bool GeneralTypeIdentifier
            => _generalTypeId;

        /// <summary />
        public IEnumerator<KeyValuePair<SValue, Type>> GetEnumerator()
        {
            return _forwardTable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
