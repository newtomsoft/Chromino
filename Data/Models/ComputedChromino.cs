using Data.Enumeration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Models
{
    [Table("ComputedChromino")]
    public class ComputedChromino
    {
        public int Id { get; set; }
        public int ChrominoId { get; set; }
        public int GameId { get; set; }
        public int BotId { get; set; }
        public Orientation Orientation { get; set; }
        public int XPosition { get; set; }
        public int YPosition { get; set; }
        public int? ParentId { get; set; }


        public Chromino Chromino { get; set; }
        public Game Game { get; set; }
        public Player Bot { get; set; }
        public ComputedChromino Parent { get; set; }
    }
}