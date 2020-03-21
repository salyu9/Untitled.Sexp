using System;
using System.Collections;
using System.Collections.Generic;

namespace Untitled.Sexp.Conversion
{
    /// <summary>
    /// Simple type resolver with lookup table.
    /// </summary>
    public class LookupTypeResolver : TypeResolver, IEnumerable<KeyValuePair<TypeIdentifier, Type>>
    {
        private readonly Dictionary<TypeIdentifier, Type> _forwardTable = new Dictionary<TypeIdentifier, Type>();
        private readonly Dictionary<Type, TypeIdentifier> _backwardTable = new Dictionary<Type, TypeIdentifier>();

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
        /// Get type for the type identifier value.
        /// </summary>
        public override Type Resolve(TypeIdentifier typeid)
            => _forwardTable[typeid];

        /// <summary>
        /// Get typeid for the type.
        /// </summary>
        public override TypeIdentifier GetTypeId(Type type)
            => _backwardTable[type];

        /// <summary />
        public IEnumerator<KeyValuePair<TypeIdentifier, Type>> GetEnumerator()
        {
            return _forwardTable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
