using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Models
{
    [Table("ChrominoInHandLast")]
    public class ChrominoInHandLast
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public int ChrominoId { get; set; }

        public Game Game { get; set; }
        public Player Player { get; set; }
        public Chromino Chromino { get; set; }
    }
}
