using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Models
{
    [Table("ChrominoInHand")]
    public class ChrominoInHand
    {
        public int Id { get; set; }
        public int ChrominoId { get; set; }
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public byte Position { get; set; }

        public Chromino Chromino { get; set; }
        public Game Game { get; set; }
        public Player Player { get; set; }
    }
}
