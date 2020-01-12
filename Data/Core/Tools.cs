using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Core
{
    public class Tools
    {
        class CommandeEqualityComparer : IEqualityComparer<PossiblesPositions>
        {
            public bool Equals(PossiblesPositions s1, PossiblesPositions s2)
            {
                if (s2 == null && s1 == null)
                    return true;
                else if (s1 == null || s2 == null)
                    return false;
                else if (s1.Coordinate == s2.Coordinate && s1.Orientation == s2.Orientation)
                    return true;
                else
                    return false;
            }
            public int GetHashCode(PossiblesPositions s)
            {
                return s.GetHashCode();
            }
        }
    }
}
