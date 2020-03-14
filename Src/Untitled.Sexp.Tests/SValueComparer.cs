using System.Collections.Generic;

namespace Untitled.Sexp.Tests
{
    public class SValueComparer : IEqualityComparer<SValue>
    {
        public static readonly SValueComparer Default = new SValueComparer();

        public bool Equals(SValue? x, SValue? y)
            => SValue.DeepEquals(x, y);

        public int GetHashCode(SValue obj)
            => obj.ToString().GetHashCode(); // why do we even need this?
    }
}
