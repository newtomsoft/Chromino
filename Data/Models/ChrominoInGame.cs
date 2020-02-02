using Data.Core;
using Data.Enumeration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

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

        public Chromino Chromino { get; set; }
        public Game Game { get; set; }
    }
}
