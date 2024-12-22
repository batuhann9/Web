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
        [Required(ErrorMessage = "Lütfen bir randevu tarihi seçin.")]
        [DataType(DataType.Date)]
        public DateOnly RandevuTarihi { get; set; }

        [Required(ErrorMessage = "Lütfen bir randevu saati seçin.")]
        [DataType(DataType.Time)]
        public TimeOnly RandevuSaati { get; set; }

        public List<Berber> Berberler { get; set; } = new List<Berber>();
        public List<Yetenek> Yetenekler { get; set; } = new List<Yetenek>();
    }
}
