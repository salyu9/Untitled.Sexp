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

        internal bool ShouldEscape { get; }

        private Symbol(string name)
        {
            Name = name;
            ShouldEscape = name.Length == 0 || name[0] == '#' || Utils.TryParseDecimalNumber(name.ToLowerInvariant(), out var l, out var d, out var exn);
            if (!ShouldEscape)
            {
                foreach (var ch in name)
                {
                    if (ch == '|' || ch == '\\' || IsDelimiter(ch) || !IsPrintable(ch))
                    {
                        ShouldEscape = true;
                        break;
                    }
                }
            }
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
            var newSymbol = new Symbol(name);
            lock (SymbolTable)
            {
                if (SymbolTable.TryGetValue(name, out symbol))
                {
                    return symbol;
                }
                SymbolTable.Add(name, newSymbol);
                return newSymbol;
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

        /// <summary />
        public override string ToString()
            => "Symbol(" + Name + ")";
    }
}
