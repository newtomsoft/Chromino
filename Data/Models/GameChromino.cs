using Data.Core;
using Data.Enumeration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Models
{
    [Table("GameChromino")]
    public class GameChromino
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ChrominoId { get; set; }

        [Required]
        public int GameId { get; set; }

        [Required]
        public ChrominoStatus State { get; set; }

        public Orientation? Orientation { get; set; }
        public int? XPosition { get; set; }
        public int? YPosition { get; set; }
        public int? PlayerId { get; set; }

        public Chromino Chromino { get; set; }
        public Game Game { get; set; }
        public Player Player { get; set; }

    }
}
