using Data.Core;
using Data.Enumeration;
using Data.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Models
{
    [Table("Square")]
    public partial class Square
    {
        [Key]
        public int Id { get; set; }
        public int GameId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public Color Color { get; set; }
        public OpenEdge Edge { get; set; }


        

        public Game Game { get; set; }
    }
}
