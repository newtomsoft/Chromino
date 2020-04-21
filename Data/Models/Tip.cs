using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Models
{
    [Table("Tip")]
    public partial class Tip
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string DomElementId { get; set; }

        [Required]
        public string HeadPictureClass { get; set; }

        [Required]
        public string Description { get; set; }

        public string IllustrationPictureClass { get; set; }

        public int? NextTipId { get; set; }


        public virtual Tip NextTip { get; set; }
    }
}