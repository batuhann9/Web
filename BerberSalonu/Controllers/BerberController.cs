using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BerberSalonu.Veritabanı;
using BerberSalonu.Models;
using System.Linq;

namespace BerberSalonu.Controllers
{
    public class BerberController : Controller
    {
        private readonly BerberContext _context;

        public BerberController(BerberContext context)
        {
            _context = context;
        }

        // Randevuların onaylanması için ekranı getirme
        public IActionResult OnayEkrani()
        {
            // Bekleyen randevuları ve ilişkili müşteri ve yetenek bilgilerini al
            var bekleyenRandevular = _context.Randevular
                .Include(r => r.Musteri) // Müşteri bilgilerini dahil et
                .Include(r => r.Yetenek) // Yetenek bilgilerini dahil et
                .Where(r => !r.IsOnaylandi) // Onaylanmamış randevuları getir
                .ToList();

            return View(bekleyenRandevular);
        }

        // Randevuyu onaylamak için metot
        [HttpPost]
        public IActionResult Onayla(int id)
        {
            var randevu = _context.Randevular
                .Include(r => r.Musteri) // İlgili müşteri bilgisini de dahil et
                .FirstOrDefault(r => r.Id == id); // Randevuyu id ile bul

            if (randevu == null)
                return NotFound(); // Eğer randevu bulunamazsa 404 döner

            // Randevuyu onayla
            randevu.IsOnaylandi = true;

            // Randevuyu veritabanına kaydet
            _context.SaveChanges();

            // Kullanıcıya bilgi mesajı göster
            TempData["Mesaj"] = "Randevu başarıyla onaylandı.";

            return RedirectToAction(nameof(OnayEkrani)); // Onay ekranına yönlendir
        }

        // Randevuyu reddetmek için metot
        [HttpPost]
        public IActionResult Red(int id)
        {
            var randevu = _context.Randevular
                .Include(r => r.Musteri) // İlgili müşteri bilgisini dahil et
                .FirstOrDefault(r => r.Id == id); // Randevuyu id ile bul

            if (randevu == null)
                return NotFound(); // Eğer randevu bulunamazsa 404 döner

            // Randevuyu veritabanından sil
            _context.Randevular.Remove(randevu);
            _context.SaveChanges();

            // Kullanıcıya bilgi mesajı göster
            TempData["Mesaj"] = "Randevu başarıyla reddedildi.";

            return RedirectToAction(nameof(OnayEkrani)); // Onay ekranına yönlendir
        }
    }
}
