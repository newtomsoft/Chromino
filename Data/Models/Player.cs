using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Models
{
    public class Player : IdentityUser <int>
    {
        [Required]
        [Column(TypeName = "varchar(25)")]
        public string Pseudo { get; set; }

        [Required]
        [StringLength(64)]
        [Display(Name = "Mot de passe")]
        [MinLength(4, ErrorMessage = "votre mot de passe doit contenir au moins 4 caractères")]
        public string Password { get; set; }
        public int PlayedGames { get; set; }
        public int Points { get; set; }
        public int WonGames { get; set; }
        public int FinishedSinglePlayerGames { get; set; }
        public int PointsSinglePlayerGames { get; set; }
        public bool Bot { get; set; }
    }
}
