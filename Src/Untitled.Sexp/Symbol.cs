using System;
using System.Collections.Generic;
using static Untitled.Sexp.Utils;

namespace Untitled.Sexp
{
    /// <summary>
    /// Represent a sexp symbol.
    /// </summary>
    public sealed class Symbol : IEquatable<Symbol>, IComparable<Symbol>
    {
        private static readonly Dictionary<string, Symbol> SymbolTable = new Dictionary<string, Symbol>();

        /// <summary>
        /// Get the name of the symbol.
        /// </summary>
        public string Name { get; }

        private Symbol(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Get symbol with string.
        /// </summary>
        /// <param name="name">The name of the symbol.</param>
        /// <returns>A symbol object with given name.</returns>
        public static Symbol FromString(string name)
        {
            if (SymbolTable.TryGetValue(name, out var symbol))
            {
                return symbol;
            }
            lock (SymbolTable)
            {
                if (SymbolTable.TryGetValue(name, out symbol))
                {
                    return symbol;
                }
                symbol = new Symbol(name);
                SymbolTable.Add(name, symbol);
                return symbol;
            }
        }

        /// <summary />
        public int CompareTo(Symbol other)
            => CompareString(Name, other.Name);

        /// <summary />
        public override bool Equals(object obj)
            => obj is Symbol symbol && Equals(symbol);

        /// <summary />
        public bool Equals(Symbol other)
            => StringEquals(Name, other.Name);

        /// <summary />
        public static bool operator ==(Symbol a, Symbol b)
            => StringEquals(a.Name, b.Name);

        /// <summary />
        public static bool operator !=(Symbol a, Symbol b)
            => !StringEquals(a.Name, b.Name);
        
        /// <summary />
        public override int GetHashCode()
            => Name.GetHashCode();
    }
}
