using Data.Core;
using Data.Enumeration;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Data.Models
{
    [Table("Chromino")]
    public partial class Chromino
    {
        [Key]
        public int Id { get; set; }
        public Color FirstColor { get; set; }
        public Color SecondColor { get; set; }
        public Color ThirdColor { get; set; }
        public int Points { get; set; }
    }
}
