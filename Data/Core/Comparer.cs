using Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Core
{
    public class PossiblesPositionsEqualityComparer : IEqualityComparer<PossiblesPositions>
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

    public class GameEqualityComparer : IEqualityComparer<Game>
    {
        public bool Equals(Game g1, Game g2)
        {
            if (g2 == null && g1 == null)
                return true;
            else if (g1 == null || g2 == null)
                return false;
            else if (g1.Id == g2.Id)
                return true;
            else
                return false;
        }
        public int GetHashCode(Game g)
        {
            int hCode = g.Id;
            return hCode.GetHashCode();
        }
    }
}
