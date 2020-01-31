using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Models
{
    [Table("GamePlayer")]
    public class GamePlayer
    {
        [Key]
        public int Id { get; set; }
        public int GameId { get; set; }
        public int PlayerId { get; set; }
        public bool PlayerTurn { get; set; }
        public bool PreviouslyDraw { get; set; }
        public bool PreviouslyPass { get; set; }
        public int PlayerPoints { get; set; }

        public Game Game { get; set; }
        public Player Player { get; set; }
    }
}
