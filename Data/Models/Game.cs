using Data.Enumeration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Models
{
    [Table("Game")]
    public class Game
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(32)]
        public string Guid { get; set; }

        public DateTime CreateDate { get; set; }

        public GameStatus Status { get; set; }

    }
}
