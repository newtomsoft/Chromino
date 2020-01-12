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
        
        [Required]
        public int GameId { get; set; }
        
        [Required]
        public int PlayerId { get; set; }
        
        [Required]
        public bool PlayerTurn { get; set; }

        [Required]
        public int PlayerPoints { get; set; }

        public Game Game { get; set; }
        public Player Player { get; set; }
    }
}
