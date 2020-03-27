using Data.Enumeration;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Models
{
    [Table("ComputedChrominoOk")]
    public class ComputedChrominoOk
    {
        public int Id { get; set; }
        public int GameId { get; set; }
        public int BotId { get; set; }
        public int ChrominoId { get; set; }
        public int Level { get; set; }

        public Game Game { get; set; }
        public Player Bot { get; set; }
        public Chromino Chromino { get; set; }
    }
}