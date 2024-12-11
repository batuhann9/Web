using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BerberSalonu.Models
{
    public class Kullanici
    {
        public int Id { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 2)]
        public required string Ad { get; set; }
        [Required]
        [StringLength(30, MinimumLength = 2)]
        public required string Soyad { get; set; }
        public required string Eposta { get; set; }
        public required string SifreHashi { get; set; }

        public Rol Rol { get; set; }
    }
}