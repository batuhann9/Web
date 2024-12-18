using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BerberSalonu.Models;
using BerberSalonu.ViewModel;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BerberSalonu.Veritabanı;

namespace BerberSalonu.Controllers
{
    [Authorize(Roles = "Müşteri")]
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
                Yetenekler = _context.Yetenekler.ToList()
            };

            return View(viewModel);
        }

        // GET: BerberleriGetir
        public IActionResult BerberleriGetir(int yetenekId)
        {
            var berberler = _context.Berberler
                .Include(b => b.Kullanici)
                .Where(b => b.Yetenekler.Any(y => y.Id == yetenekId))
                .Select(b => new
                {
                    b.Id,
                    Ad = b.Kullanici.Ad + " " + b.Kullanici.Soyad
                })
                .ToList();

            return Json(berberler);
        }

        // POST: RandevuOlustur
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RandevuOlustur(RandevuViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kullanıcı kimliğini al
                    var kullaniciEposta = User.Identity.Name;

                    if (string.IsNullOrEmpty(kullaniciEposta))
                    {
                        TempData["Hata"] = "Kimlik doğrulama hatası. Lütfen tekrar giriş yapın.";
                        return RedirectToAction("Giris", "Hesap");
                    }

                    // Kullanıcıdan müşteri bilgilerini al
                    var musteri = await _context.Musteriler
                        .Include(m => m.Kullanici)
                        .FirstOrDefaultAsync(m => m.Kullanici.Eposta == kullaniciEposta);

                    if (musteri == null)
                    {
                        TempData["Hata"] = "Müşteri bilgisi bulunamadı.";
                        return RedirectToAction("Giris", "Hesap");
                    }

                    // Yeni randevuyu oluştur
                    var yeniRandevu = new Randevu
                    {
                        MusteriId = musteri.Id,
                        YetenekId = model.YetenekId,
                        BerberId = model.BerberId,
                        RandevuTarihi = model.RandevuTarihi
                    };

                    _context.Randevular.Add(yeniRandevu);
                    await _context.SaveChangesAsync();

                    TempData["Mesaj"] = "Randevu başarıyla oluşturuldu.";
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    TempData["Hata"] = $"Bir hata oluştu: {ex.Message}";
                }
            }

            // Hata durumunda dropdown'ları tekrar doldur
            model.Yetenekler = _context.Yetenekler.ToList();
            model.Berberler = new List<Berber>();

            return View(model);
        }
        // GET: Randevularım
        public async Task<IActionResult> Randevularim()
        {
            // Oturumdaki kullanıcının kimliğini al
            var kullaniciEposta = User.Identity.Name;

            var musteri = await _context.Musteriler
                .Include(m => m.Kullanici)
                .FirstOrDefaultAsync(m => m.Kullanici.Eposta == kullaniciEposta);

            if (musteri == null)
            {
                TempData["Hata"] = "Müşteri bulunamadı. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Giris", "Hesap");
            }

            // Onaylanmış randevuları getir
            var randevular = await _context.Randevular
                .Include(r => r.Berber)
                .ThenInclude(b => b.Kullanici)
                .Include(r => r.Yetenek)
                .Where(r => r.MusteriId == musteri.Id && r.IsOnaylandi == true)
                .ToListAsync();

            return View(randevular);
        }

        // POST: Randevu Iptal Et
        [HttpPost]
        public async Task<IActionResult> RandevuIptal(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);

            if (randevu == null)
            {
                TempData["Hata"] = "Randevu bulunamadı.";
                return RedirectToAction("Randevularim");
            }

            _context.Randevular.Remove(randevu);
            await _context.SaveChangesAsync();

            TempData["Mesaj"] = "Randevu başarıyla iptal edildi.";
            return RedirectToAction("Randevularim");
        }
    }
}
