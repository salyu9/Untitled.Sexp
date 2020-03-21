using System;

namespace Untitled.Sexp
{
    /// <summary>
    /// Represent a type-identifier.
    /// </summary>
    public sealed class TypeIdentifier : IEquatable<TypeIdentifier>, IComparable<TypeIdentifier>
    {
        internal Symbol _symbol;

        /// <summary>
        /// Get the name of the type-identifier.
        /// </summary>
        public string Name
            => _symbol.Name;

        private TypeIdentifier(string name)
        {
            _symbol = Symbol.FromString(name);
        }

        /// <summary>
        /// Get type-identifier with string.
        /// </summary>
        /// <param name="name">The name of the type-identifier.</param>
        /// <returns>A type-identifier object with given name.</returns>
        public static TypeIdentifier FromString(string name)
            => new TypeIdentifier(name);

        /// <summary />
        public int CompareTo(TypeIdentifier other)
            => _symbol.CompareTo(other._symbol);

        /// <summary />
        public override bool Equals(object obj)
            => obj is TypeIdentifier identifier && Equals(identifier);

        /// <summary />
        public bool Equals(TypeIdentifier other)
            => _symbol.Equals(other._symbol);

        /// <summary />
        public static bool operator ==(TypeIdentifier a, TypeIdentifier b)
            => a._symbol.Equals(b._symbol);

        /// <summary />
        public static bool operator !=(TypeIdentifier a, TypeIdentifier b)
            => !a._symbol.Equals(b._symbol);

        /// <summary />
        public override int GetHashCode()
            => Name.GetHashCode();
        
    }
}
