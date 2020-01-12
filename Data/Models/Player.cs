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
        [MinLength(8)]
        public string Password { get; set; }
        
        [Required]
        public int PlayedGames { get; set; }
        
        [Required]
        public int WonGames { get; set; }

    }
}
