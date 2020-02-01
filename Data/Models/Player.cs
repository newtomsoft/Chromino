using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Models
{
    [Table("Player")]
    public class Player
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Pseudo { get; set; }
        
        [Required]
        [StringLength(64)]
        [MinLength(4, ErrorMessage = "your password must contain at least 4 characters")]
        public string Password { get; set; }
        public int PlayedGames { get; set; }
        public int Points { get; set; }
        public int WonGames { get; set; }
        public int FinishedSinglePlayerGames { get; set; }
        public int PointsSinglePlayerGames { get; set; }
        public bool Bot { get; set; }
    }
}
