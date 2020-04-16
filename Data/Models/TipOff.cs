using Data.Enumeration;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Models
{
    [Table("TipOff")]
    public partial class TipOff
    {
        [Key]
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public int TipId { get; set; }

        public virtual Player Player { get; set; }
        public virtual Tip Tip { get; set; }
    }
}
