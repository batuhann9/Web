using BerberSalonu.Models;
using BerberSalonu.Veritabanı;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BerberSalonu.Controllers
{
    [Authorize(Roles = "Berber")]
    public class BerberController : Controller
    {
        private readonly BerberContext _context;

        public BerberController(BerberContext context)
        {
            _context = context;
        }

        // Gelen randevu taleplerini listeleme
        public async Task<IActionResult> RandevuTalepleri()
        {
            var berberEmail = User.Identity.Name;
            var berber = await _context.Berberler
                .Include(b => b.Kullanici)
                .FirstOrDefaultAsync(b => b.Kullanici.Eposta == berberEmail);

            if (berber == null)
            {
                TempData["Hata"] = "Berber bilgisi bulunamadı.";
                return RedirectToAction("Giris", "Hesap");
            }

            var talepler = await _context.Randevular
                .Include(r => r.Musteri)
                .ThenInclude(m => m.Kullanici)
                .Include(r => r.Yetenek)
                .Where(r => r.BerberId == berber.Id && r.Durum == RandevuDurum.OnayBekliyor)
                .OrderBy(r => r.RandevuTarihi)
                .ThenBy(r => r.RandevuSaati)
                .ToListAsync();

            return View(talepler);
        }

        // Randevu Onaylama
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RandevuOnayla(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                randevu.Durum = RandevuDurum.Onaylandi;
                _context.Randevular.Update(randevu);
                await _context.SaveChangesAsync();
                TempData["Mesaj"] = "Randevu başarıyla onaylandı.";
            }
            else
            {
                TempData["Hata"] = "Randevu bulunamadı.";
            }

            return RedirectToAction("RandevuTalepleri");
        }

        // Randevu Reddetme (İptal Etme)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RandevuReddet(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                randevu.Durum = RandevuDurum.IptalEdildi;
                _context.Randevular.Update(randevu);
                await _context.SaveChangesAsync();
                TempData["Mesaj"] = "Randevu başarıyla iptal edildi.";
            }
            else
            {
                TempData["Hata"] = "Randevu bulunamadı.";
            }

            return RedirectToAction("RandevuTalepleri");
        }

        public async Task<IActionResult> Randevularim()
        {
            var berberEmail = User.Identity.Name;
            var berber = await _context.Berberler
                .Include(b => b.Kullanici)
                .FirstOrDefaultAsync(b => b.Kullanici.Eposta == berberEmail);

            if (berber == null)
            {
                TempData["Hata"] = "Berber bilgisi bulunamadı.";
                return RedirectToAction("Giris", "Hesap");
            }

            var randevular = await _context.Randevular
                .Include(r => r.Musteri)
                .ThenInclude(m => m.Kullanici)
                .Include(r => r.Yetenek)
                .Where(r => r.BerberId == berber.Id &&
                            (r.Durum == RandevuDurum.Onaylandi || r.Durum == RandevuDurum.Gerceklesti))
                .OrderBy(r => r.RandevuTarihi)
                .ThenBy(r => r.RandevuSaati)
                .ToListAsync();

            var suAn = DateTime.Now;

            foreach (var randevu in randevular)
            {
                var randevuTarihSaati = randevu.RandevuTarihi.ToDateTime(randevu.RandevuSaati);
                if (randevuTarihSaati <= suAn && randevu.Durum == RandevuDurum.Onaylandi)
                {
                    randevu.Durum = RandevuDurum.Gerceklesti;
                    _context.Randevular.Update(randevu);
                }
            }

            await _context.SaveChangesAsync();

            return View(randevular);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RandevuIptal(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);

            if (randevu != null)
            {
                // Sadece Onaylandı veya Onay Bekliyor durumundaki randevular iptal edilebilir
                if (randevu.Durum == RandevuDurum.Onaylandi || randevu.Durum == RandevuDurum.OnayBekliyor)
                {
                    randevu.Durum = RandevuDurum.IptalEdildi;
                    _context.Randevular.Update(randevu);
                    await _context.SaveChangesAsync();
                    TempData["Mesaj"] = "Randevu başarıyla iptal edildi.";
                }
                else
                {
                    TempData["Hata"] = "Bu randevu iptal edilemez.";
                }
            }
            else
            {
                TempData["Hata"] = "Randevu bulunamadı.";
            }

            return RedirectToAction("Randevularim");
        }

        [HttpGet("Berber/BerberYetenekleriGetir")]
        public async Task<IActionResult> BerberYetenekleriGetir(int berberId)
        {
            var mevcutYetenekler = await _context.BerberYetenekler
                .Where(by => by.BerberId == berberId)
                .Select(by => by.YetenekId)
                .ToListAsync();

            var eksikYetenekler = await _context.Yetenekler
                .Where(y => !mevcutYetenekler.Contains(y.Id))
                .ToListAsync();

            if (!eksikYetenekler.Any())
            {
                return Json(new { success = false, message = "Berbere eklenebilecek yetenek bulunamadı." });
            }

            return Json(eksikYetenekler.Select(y => new
            {
                id = y.Id,
                name = y.Name
            }));
        }

        [HttpPost]
        public async Task<IActionResult> RandevuSil(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null && (randevu.Durum == RandevuDurum.Gerceklesti || randevu.Durum == RandevuDurum.IptalEdildi))
            {
                _context.Randevular.Remove(randevu);
                await _context.SaveChangesAsync();
                TempData["Mesaj"] = "Randevu başarıyla silindi.";
            }
            else
            {
                TempData["Hata"] = "Randevu bulunamadı veya silinemez.";
            }
            return RedirectToAction(nameof(Randevularim));
        }

    }
}