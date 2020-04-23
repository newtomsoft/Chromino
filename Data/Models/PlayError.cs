using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Models
{
    [Table("PlayError")]
    public partial class PlayError
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(25)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [MaxLength(25)]
        public string IllustrationPictureClass { get; set; }

        [MaxLength(70)]
        public string IllustrationPictureCaption { get; set; }
    }
}