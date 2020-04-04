using Data.Enumeration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Models
{
    [Table("GoodPosition")]
    public class GoodPosition
    {
        public int Id { get; set; }
        public int ChrominoId { get; set; }
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public Orientation Orientation { get; set; }
        public bool Flip { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int? ParentId { get; set; }

        public Chromino Chromino { get; set; }
        public Game Game { get; set; }
        public Player Bot { get; set; }
        public GoodPosition Parent { get; set; }

        public override bool Equals(object c)
        {
            if (c == null || !(c is GoodPosition))
                return false;
            else
                return X == ((GoodPosition)c).X && Y == ((GoodPosition)c).Y && Orientation == ((GoodPosition)c).Orientation;
        }

        public override int GetHashCode()
        {
            return (X * 10000 + Y * 10 + Orientation).GetHashCode();
        }
    }
}