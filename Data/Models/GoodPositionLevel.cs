using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Models
{
    [Table("GoodPositionLevel")]
    public class GoodPositionLevel
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public int ChrominoId { get; set; }
        public int Level { get; set; }

        public Game Game { get; set; }
        public Player Bot { get; set; }
        public Chromino Chromino { get; set; }
    }
}