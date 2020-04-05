using Data.Models;
using System.Collections.Generic;

namespace Tool
{
    public class GoodPositionComparer : IEqualityComparer<GoodPosition>
    {
        public bool Equals(GoodPosition gp1, GoodPosition gp2)
        {
            if (gp1 == null || gp2 == null)
                return false;
            else
                return (gp1.Flip == gp2.Flip) && (gp1.ChrominoId == gp2.ChrominoId) && (gp1.X == gp2.X) && (gp1.Y == gp2.Y) && (gp1.Orientation == gp2.Orientation);
        }

        public int GetHashCode(GoodPosition obj) => (obj.ChrominoId * obj.X * obj.Y * (int)obj.Orientation).GetHashCode();
    }
}