using Data.Enumeration;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Models
{
    [Table("ChrominoInGame")]
    public class ChrominoInGame
    {
        public int Id { get; set; }
        public int ChrominoId { get; set; }
        public int GameId { get; set; }
        public Orientation Orientation { get; set; }
        public int XPosition { get; set; }
        public int YPosition { get; set; }
        public byte Move { get; set; }
        public int? PlayerId { get; set; }

        public Chromino Chromino { get; set; }
        public Game Game { get; set; }
        public Player Player { get; set; }

        public static ChrominoInGame From(ComputedChromino computedChromino)
        {
            return new ChrominoInGame
            {
                GameId = computedChromino.GameId,
                PlayerId = computedChromino.BotId,
                ChrominoId = computedChromino.ChrominoId,
                Orientation = computedChromino.Orientation,
                XPosition = computedChromino.X,
                YPosition = computedChromino.Y
            };
        }
    }
}
