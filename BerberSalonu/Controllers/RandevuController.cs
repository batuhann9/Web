using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BerberSalonu.Models;
using BerberSalonu.ViewModel;
using Microsoft.EntityFrameworkCore;
using BerberSalonu.Veritabani;

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RandevuOlustur(RandevuViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var kullaniciEposta = GetKullaniciEposta();
                    if (string.IsNullOrEmpty(kullaniciEposta))
                        return RedirectToAction("Giris", "Hesap");

                    var musteri = await GetMusteriAsync(kullaniciEposta);
                    if (musteri == null)
                        return RedirectToAction("Giris", "Hesap");

                    if (model.RandevuTarihi.Date < DateTime.Now.Date)
                    {
                        TempData["Hata"] = "Geçmiş bir tarihe randevu oluşturamazsınız.";
                        model.Yetenekler = _context.Yetenekler.ToList();
                        model.Berberler = _context.Berberler.ToList();
                        return View(model);
                    }

                    // Tarihi UTC olarak belirle
                    var randevuBaslangic = DateTime.SpecifyKind(model.RandevuTarihi, DateTimeKind.Utc);

                    // Yetenek süresi ile randevu bitişini belirle
                    var yetenek = await _context.Yetenekler.FindAsync(model.YetenekId);
                    if (yetenek == null)
                    {
                        TempData["Hata"] = "Seçilen yetenek bulunamadı.";
                        return RedirectToAction("RandevuOlustur");
                    }

                    var randevuBitis = randevuBaslangic.AddMinutes(yetenek.Sure);

                    // Çakışan onaylanmış randevuları kontrol et
                    var cakisanRandevu = await _context.Randevular
                        .Where(r => r.BerberId == model.BerberId
                                    && r.IsOnaylandi
                                    && r.RandevuTarihi < randevuBitis
                                    && r.RandevuTarihi.AddMinutes(r.Yetenek.Sure) > randevuBaslangic)
                        .FirstOrDefaultAsync();

                    if (cakisanRandevu != null)
                    {
                        TempData["Hata"] = "Seçilen tarih ve saatte bu berber için başka bir onaylanmış randevu bulunuyor.";
                        model.Yetenekler = _context.Yetenekler.ToList();
                        model.Berberler = _context.Berberler.ToList();
                        return View(model);
                    }

                    var yeniRandevu = new Randevu
                    {
                        MusteriId = musteri.Id,
                        YetenekId = model.YetenekId,
                        BerberId = model.BerberId,
                        RandevuTarihi = randevuBaslangic
                    };

                    _context.Randevular.Add(yeniRandevu);
                    await _context.SaveChangesAsync();

                    TempData["Mesaj"] = "Randevu başarıyla oluşturuldu.";
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    TempData["Hata"] = $"Bir hata oluştu: {ex.InnerException?.Message ?? ex.Message}";
                }
            }

            model.Yetenekler = _context.Yetenekler.ToList();
            model.Berberler = _context.Berberler.ToList();

            return View(model);
        }

        // GET: Randevularım
        public async Task<IActionResult> Randevularim()
        {
            var kullaniciEposta = GetKullaniciEposta();
            if (string.IsNullOrEmpty(kullaniciEposta))
                return RedirectToAction("Giris", "Hesap");

            var musteri = await GetMusteriAsync(kullaniciEposta);
            if (musteri == null)
                return RedirectToAction("Giris", "Hesap");

            // Onaylanmış randevuları getir
            var randevular = await _context.Randevular
                .AsNoTracking()
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

        private string? GetKullaniciEposta()
        {
            /* 
             * Authorized endpoint altinda oldugumuz icin,
             * Identity'nin varligindan eminiz.
             */
            var kullaniciEposta = User.Identity!.Name;

            if (string.IsNullOrEmpty(kullaniciEposta))
            {
                TempData["Hata"] = "Kimlik doğrulama hatası. Lütfen tekrar giriş yapın.";
                return null;
            }

            return kullaniciEposta;
        }

        private async Task<Musteri?> GetMusteriAsync(string kullaniciEposta)
        {
            var musteri = await _context.Musteriler
                .Include(m => m.Kullanici)
                .FirstOrDefaultAsync(m => m.Kullanici.Eposta == kullaniciEposta);

            if (musteri == null)
            {
                TempData["Hata"] = "Müşteri bilgisi bulunamadı.";
                return null;
            }

            return musteri;
        }

    }
}
