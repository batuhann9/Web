using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BerberSalonu.Models;
using BerberSalonu.ViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
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

                var baslangicSaati = new TimeOnly(10, 0);  // 10:00
                var bitisSaati = new TimeOnly(22, 0);      // 22:00

                var saatAraliklari = new List<TimeOnly>();
                for (var zaman = baslangicSaati; zaman < bitisSaati; zaman = zaman.AddMinutes(20))
                {
                    saatAraliklari.Add(zaman);
                }

                var yetenek = await _context.Yetenekler.FindAsync(yetenekId);
                if (yetenek == null)
                {
                    return Json(new { success = false, message = "Yetenek bulunamadı." });
                }

                var mevcutRandevular = await _context.Randevular
                    .Include(r => r.Yetenek)
                    .Where(r => r.BerberId == berberId && r.RandevuTarihi == secilenTarih)
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

                    var randevuBaslangic = model.RandevuTarihi.ToDateTime(model.RandevuSaati);
                    var randevuBitis = randevuBaslangic.AddMinutes(yetenek.Sure);

                    var cakisanRandevu = await _context.Randevular
                        .Where(r => r.BerberId == model.BerberId && r.RandevuTarihi == model.RandevuTarihi)
                        .Include(r => r.Yetenek)
                        .ToListAsync();

                    if (cakisanRandevu.Any(r =>
                    {
                        var mevcutBaslangic = r.RandevuTarihi.ToDateTime(r.RandevuSaati);
                        var mevcutBitis = mevcutBaslangic.AddMinutes(r.Yetenek.Sure);
                        return mevcutBaslangic < randevuBitis && mevcutBitis > randevuBaslangic;
                    }))
                    {
                        TempData["Hata"] = "Seçilen saat aralığında çakışma var.";
                        return View(model);
                    }

                    var yeniRandevu = new Randevu
                    {
                        MusteriId = musteri.Id,
                        YetenekId = model.YetenekId,
                        BerberId = model.BerberId,
                        RandevuTarihi = model.RandevuTarihi,
                        RandevuSaati = model.RandevuSaati
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

            return View(randevular);
        }

        [HttpPost("RandevuIptal")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RandevuIptal(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);

            if (randevu != null)
            {
                _context.Randevular.Remove(randevu);
                await _context.SaveChangesAsync();
                TempData["Mesaj"] = "Randevu başarıyla iptal edildi.";
            }
            else
            {
                TempData["Hata"] = "Randevu bulunamadı.";
            }

            return RedirectToAction("Randevularim");
        }
    }
}