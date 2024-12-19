using BerberSalonu.Veritabani;
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
            var berberEmail = User.Identity.Name; // Oturumdaki berberin email'i
            var berber = await _context.Berberler
                .Include(b => b.Kullanici)
                .FirstOrDefaultAsync(b => b.Kullanici.Eposta == berberEmail);

            if (berber == null)
            {
                return RedirectToAction("Giris", "Hesap");
            }

            // Berberin onaylanmamış randevuları
            var talepler = await _context.Randevular
                .Include(r => r.Musteri)
                .ThenInclude(m => m.Kullanici)
                .Include(r => r.Yetenek)
                .Where(r => r.BerberId == berber.Id && !r.IsOnaylandi)
                .ToListAsync();

            return View(talepler);
        }

        // Randevu Onaylama
        [HttpPost]
        public async Task<IActionResult> RandevuOnayla(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);

            if (randevu != null)
            {
                randevu.IsOnaylandi = true; // Randevuyu onayla
                _context.Randevular.Update(randevu);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("RandevuTalepleri");
        }

        // Randevu Reddetme
        [HttpPost]
        public async Task<IActionResult> RandevuReddet(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);

            if (randevu != null)
            {
                _context.Randevular.Remove(randevu); // Randevuyu sil
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("RandevuTalepleri");
        }
        // Onaylanmış randevuları listeleme
        public async Task<IActionResult> Randevularim()
        {
            var berberEmail = User.Identity.Name; // Oturumdaki berberin email'i
            var berber = await _context.Berberler
                .Include(b => b.Kullanici)
                .FirstOrDefaultAsync(b => b.Kullanici.Eposta == berberEmail);

            if (berber == null)
            {
                return RedirectToAction("Giris", "Hesap");
            }

            // Berberin onaylanmış randevuları
            var onaylanmisRandevular = await _context.Randevular
                .Include(r => r.Musteri)
                .ThenInclude(m => m.Kullanici)
                .Include(r => r.Yetenek)
                .Where(r => r.BerberId == berber.Id && r.IsOnaylandi)
                .ToListAsync();

            return View(onaylanmisRandevular);
        }

    }
}
