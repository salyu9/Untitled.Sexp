using System;
using System.Collections.Generic;
using static Untitled.Sexp.Utils;

namespace Untitled.Sexp
{
    /// <summary>
    /// Represent a sexp symbol.
    /// </summary>
    public sealed class SSymbol : IEquatable<SSymbol>, IComparable<SSymbol>
    {
        private static readonly Dictionary<string, SSymbol> SymbolTable = new Dictionary<string, SSymbol>();
        
        /// <summary>
        /// Get the name of the symbol.
        /// </summary>
        public string Name { get; private set; }

        private SSymbol(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Get symbol with string.
        /// </summary>
        /// <param name="name">The name of the symbol.</param>
        /// <returns>A symbol object with given name.</returns>
        public static SSymbol FromString(string name)
        {
            if (SymbolTable.TryGetValue(name, out var symbol))
            {
                return symbol;
            }
            symbol = new SSymbol(name);
            SymbolTable.Add(name, symbol);
            return symbol;
        }

        /// <summary />
        public int CompareTo(SSymbol other)
            => CompareString(Name, other.Name);

        /// <summary />
        public override bool Equals(object obj)
        {
            if (!(obj is SSymbol symbol))
            {
                return false;
            }
            return Equals(symbol);
        }

        /// <summary />
        public bool Equals(SSymbol other)
            => StringEquals(Name, other.Name);

        /// <summary />
        public static bool operator ==(SSymbol a, SSymbol b)
            => StringEquals(a.Name, b.Name);

        /// <summary />
        public static bool operator !=(SSymbol a, SSymbol b)
            => !StringEquals(a.Name, b.Name);

        /// <summary />
        public override string ToString()
            => Name;

        /// <summary />
        public override int GetHashCode()
            => Name.GetHashCode();
    }
}
