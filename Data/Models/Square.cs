using Data.Enumeration;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Models
{
    [Table("Square")]
    public partial class Square
    {
        [Key]
        public int Id { get; set; }
        public int GameId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public Color Color { get; set; }
        public bool OpenRight { get; set; }
        public bool OpenBottom { get; set; }
        public bool OpenLeft { get; set; }
        public bool OpenTop { get; set; }

        public Game Game { get; set; }
    }
}
