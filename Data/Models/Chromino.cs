using Data.Enumeration;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Models
{
    [Table("Chromino")]
    public partial class Chromino
    {
        [Key]
        public int Id { get; set; }
        public ColorCh FirstColor { get; set; }
        public ColorCh SecondColor { get; set; }
        public ColorCh ThirdColor { get; set; }
        public int Points { get; set; }
    }
}
