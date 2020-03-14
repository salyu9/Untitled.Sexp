using System;

namespace Untitled.Sexp
{
    /// <summary>
    /// Represent an sexp pair.
    /// </summary>
    public sealed class SPair : IEquatable<SPair>
    {
        internal SValue _car;
        internal SValue _cdr;

        /// <summary>
        /// Instantiate new instance of SPair with car and cdr.
        /// </summary>
        public SPair(SValue car, SValue cdr)
        {
            _car = car;
            _cdr = cdr;
        }

        /// <summary>
        /// For pattern matching.
        /// </summary>
        public void Deconstruct(out SValue car, out SValue cdr)
        {
            car = _car;
            cdr = _cdr;
        }

        /// <summary>
        /// Get the car field of the pair.
        /// </summary>
        public SValue Car
            => _car;
        
        /// <summary>
        /// Get the cdr field of the pair.
        /// </summary>
        public SValue Cdr
            => _cdr;

        /// <summary />
        public bool Equals(SPair other)
        {
            return ReferenceEquals(this, other)
                || (_car.Equals(other._car) && _cdr.Equals(other._cdr));
        }

        /// <summary />
        public override bool Equals(object obj)
        {
            return obj is SPair spair && Equals(spair);
        }

        /// <summary />
        public override int GetHashCode()
        {
            return new { _car, _cdr }.GetHashCode();
        }

        /// <summary />
        public override string ToString()
        {
            return $"({_car} . {_cdr})";
        }
    }
}
