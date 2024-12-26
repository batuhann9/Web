using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BerberSalonu.Models;
using BerberSalonu.ViewModel;
using Microsoft.EntityFrameworkCore;
using BerberSalonu.Veritabanı;

namespace BerberSalonu.Controllers
{
    [Authorize(Roles = "Müşteri")]
    [Route("Randevu")]
    public class RandevuController : Controller
    {
        private readonly BerberContext _context;

        public RandevuController(BerberContext context)
        {
            _context = context;
        }

        [HttpGet("RandevuOlustur")]
        public IActionResult RandevuOlustur()
        {
            var viewModel = new RandevuViewModel
            {
                Yetenekler = _context.Yetenekler.ToList()
            };
            return View(viewModel);
        }

        [HttpGet("BerberleriGetir")]
        public async Task<IActionResult> BerberleriGetir(int yetenekId)
        {
            try
            {
                var berberler = await _context.BerberYetenekler
                    .Include(by => by.Berber)
                    .ThenInclude(b => b.Kullanici)
                    .Where(by => by.YetenekId == yetenekId)
                    .Select(by => new
                    {
                        by.Berber.Id,
                        AdSoyad = by.Berber.Kullanici.Ad + " " + by.Berber.Kullanici.Soyad
                    })
                    .ToListAsync();

                if (!berberler.Any())
                {
                    return Json(new { success = false, message = "Bu yetenek için uygun berber bulunamadı." });
                }

                return Json(berberler);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }

        [HttpGet("SaatAraliklariGetir")]
        public async Task<IActionResult> SaatAraliklariGetir(int berberId, string tarih, int yetenekId)
        {
            try
            {
                if (!DateOnly.TryParse(tarih, out DateOnly secilenTarih))
                {
                    return Json(new { success = false, message = "Geçerli bir tarih girin." });
                }

                var suAn = DateTime.Now;
                var baslangicSaati = new TimeOnly(10, 0);
                var bitisSaati = new TimeOnly(22, 0);

                if (secilenTarih < DateOnly.FromDateTime(suAn.Date))
                {
                    return Json(new { success = false, message = "Geçmiş tarihe randevu alınamaz." });
                }

                var saatAraliklari = new List<TimeOnly>();
                for (var zaman = baslangicSaati; zaman < bitisSaati; zaman = zaman.AddMinutes(20))
                {
                    if (secilenTarih == DateOnly.FromDateTime(suAn.Date) && zaman <= TimeOnly.FromDateTime(suAn))
                    {
                        continue;
                    }
                    saatAraliklari.Add(zaman);
                }

                var yetenek = await _context.Yetenekler.FindAsync(yetenekId);
                if (yetenek == null)
                {
                    return Json(new { success = false, message = "Yetenek bulunamadı." });
                }

                var mevcutRandevular = await _context.Randevular
                    .Include(r => r.Yetenek)
                    .Where(r => r.BerberId == berberId && r.RandevuTarihi == secilenTarih &&
                           (r.Durum == RandevuDurum.OnayBekliyor || r.Durum == RandevuDurum.Onaylandi))
                    .ToListAsync();

                var uygunSaatler = saatAraliklari
                    .Where(zaman =>
                        !mevcutRandevular.Any(r =>
                        {
                            var randevuBaslangic = r.RandevuTarihi.ToDateTime(r.RandevuSaati);
                            var randevuBitis = randevuBaslangic.AddMinutes(r.Yetenek?.Sure ?? 0);

                            var secilenBaslangic = secilenTarih.ToDateTime(zaman);
                            var secilenBitis = secilenBaslangic.AddMinutes(yetenek.Sure);

                            return !(secilenBitis <= randevuBaslangic || secilenBaslangic >= randevuBitis);
                        }))
                    .Select(zaman => zaman.ToString("HH:mm"))
                    .ToList();

                return Json(uygunSaatler);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Saat aralıkları yüklenirken hata oluştu: {ex.Message}" });
            }
        }

        [HttpPost("RandevuOlustur")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RandevuOlustur(RandevuViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var kullaniciEposta = User.Identity.Name;
                    var musteri = await _context.Musteriler
                        .Include(m => m.Kullanici)
                        .FirstOrDefaultAsync(m => m.Kullanici.Eposta == kullaniciEposta);

                    if (musteri == null)
                    {
                        TempData["Hata"] = "Müşteri bilgisi bulunamadı.";
                        return RedirectToAction("Giris", "Hesap");
                    }

                    var yetenek = await _context.Yetenekler.FindAsync(model.YetenekId);
                    if (yetenek == null)
                    {
                        TempData["Hata"] = "Seçilen yetenek bulunamadı.";
                        return RedirectToAction("RandevuOlustur");
                    }

                    var yeniRandevu = new Randevu
                    {
                        MusteriId = musteri.Id,
                        YetenekId = model.YetenekId,
                        BerberId = model.BerberId,
                        RandevuTarihi = model.RandevuTarihi,
                        RandevuSaati = model.RandevuSaati,
                        Durum = RandevuDurum.OnayBekliyor
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

            return View(model);
        }

        [HttpGet("Randevularim")]
        public async Task<IActionResult> Randevularim()
        {
            var kullaniciEposta = User.Identity.Name;

            var musteri = await _context.Musteriler
                .Include(m => m.Kullanici)
                .FirstOrDefaultAsync(m => m.Kullanici.Eposta == kullaniciEposta);

            if (musteri == null)
            {
                TempData["Hata"] = "Müşteri bilgisi bulunamadı.";
                return RedirectToAction("Giris", "Hesap");
            }

            var randevular = await _context.Randevular
                .Include(r => r.Berber)
                .ThenInclude(b => b.Kullanici)
                .Include(r => r.Yetenek)
                .Where(r => r.MusteriId == musteri.Id)
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

        public IActionResult RandevuIptal(int id)
        {
            var randevu = _context.Randevular.Include(r => r.Berber).FirstOrDefault(r => r.Id == id);

            if (randevu == null)
            {
                TempData["Hata"] = "Randevu bulunamadı.";
                return RedirectToAction("Randevularim");
            }

            if (randevu.Durum == RandevuDurum.IptalEdildi)
            {
                TempData["Hata"] = "Bu randevu zaten iptal edilmiş.";
                return RedirectToAction("Randevularim");
            }

            // Onaylanmış randevuları da iptal etmeye izin ver
            if (randevu.Durum == RandevuDurum.Onaylandi || randevu.Durum == RandevuDurum.OnayBekliyor)
            {
                randevu.Durum = RandevuDurum.IptalEdildi;
                _context.SaveChanges();
                TempData["Mesaj"] = "Randevu başarıyla iptal edildi.";
            }
            else
            {
                TempData["Hata"] = "Bu randevu iptal edilemez.";
            }

            return RedirectToAction("Randevularim");
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