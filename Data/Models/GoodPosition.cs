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
    }
}