using Microsoft.AspNetCore.Authorization; // Eklendi
using Microsoft.AspNetCore.Mvc;
using BerberSalonu.Models;
using BerberSalonu.ViewModel;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using BerberSalonu.Veritabanı;

namespace BerberSalonu.Controllers
{
    [Authorize(Roles = "Müşteri")] // Sadece "Musteri" rolüne izin verildi
    public class RandevuController : Controller
    {
        private readonly BerberContext _context;

        public RandevuController(BerberContext context)
        {
            _context = context;
        }

        // GET: RandevuOlustur
        public IActionResult RandevuOlustur()
        {
            var viewModel = new RandevuViewModel
            {
                Yetenekler = _context.Yetenekler.ToList(), // Yetenekler dropdown için
                Berberler = new List<Berber>()            // Başlangıçta boş olacak
            };

            return View(viewModel);
        }

        // GET: BerberleriGetir (yetenekId ile filtrelenmiş Berberler)
        public IActionResult BerberleriGetir(int yetenekId)
        {
            var berberler = _context.Berberler
                .Include(b => b.Kullanici) // Kullanıcı bilgilerini dahil et
                .Where(b => b.Yetenekler.Any(y => y.Id == yetenekId)) // Yeteneğe göre filtrele
                .Select(b => new
                {
                    b.Id,
                    Ad = b.Kullanici.Ad + " " + b.Kullanici.Soyad // Kullanıcı adını birleştir
                })
                .ToList();

            return Json(berberler);
        }
    }
}