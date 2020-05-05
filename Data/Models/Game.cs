using Data.Enumeration;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Models
{
    [Table("Game")]
    public class Game
    {
        [Key]
        public int Id { get; set; }

        [StringLength(32)]
        public string Guid { get; set; }
        public DateTime CreateDate { get; set; }
        public GameStatus Status { get; set; }
        public DateTime PlayedDate { get; set; }
        public byte Move { get; set; }
    }
}
