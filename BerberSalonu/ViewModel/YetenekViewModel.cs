using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BerberSalonu.ViewModel
{
    public class YetenekViewModel
    {
        [Required(ErrorMessage = "Yetenek ismi zorunludur.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Fiyat zorunludur.")]
        [Range(1, 1000, ErrorMessage = "Fiyat 1 ile 1000 arasında olmalıdır.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Süre zorunludur.")]
        [Range(10, 180, ErrorMessage = "Süre 10 ile 180 dakika arasında olmalıdır.")]
        public int Sure { get; set; }
    }
}
