using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BerberSalonu.ViewModel
{
    public class YetenekSilViewModel
{
    [Required(ErrorMessage = "Lütfen bir yetenek seçin.")]
    public int YetenekId { get; set; }

    public List<SelectListItem> YeteneklerSelectList { get; set; } = new List<SelectListItem>();
}
}