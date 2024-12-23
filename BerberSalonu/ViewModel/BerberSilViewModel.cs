using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace BerberSalonu.ViewModel
{
    public class BerberSilViewModel
    {
        public int? BerberId { get; set; }  // Seçilen berberin Id'si
        public List<SelectListItem> BerberlerSelectList { get; set; }  // Dropdown için berber listesi

        public BerberSilViewModel()
        {
            BerberlerSelectList = new List<SelectListItem>();
        }
    }
}
