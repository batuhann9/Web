using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BerberSalonu.Models;

namespace BerberSalonu.ViewModel
{
    public class RandevuViewModel
    {
        public int MusteriId {  get; set; }

        [Required(ErrorMessage = "Lütfen bir yetenek seçin.")]
        public int YetenekId { get; set; }

        [Required(ErrorMessage = "Lütfen bir berber seçin.")]
        public int BerberId { get; set; }

        [Required(ErrorMessage = "Lütfen bir randevu tarihi ve saati seçin.")]
        [DataType(DataType.DateTime)]
        public DateTime RandevuTarihi { get; set; }

        // Dropdown'lar için listeler
        public List<Berber> Berberler { get; set; } = new List<Berber>();
        public List<Yetenek> Yetenekler { get; set; } = new List<Yetenek>();
    }
}
