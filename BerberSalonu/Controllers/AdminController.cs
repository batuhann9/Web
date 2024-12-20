using BerberSalonu.Models;
using BerberSalonu.Veritabani;
using BerberSalonu.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BerberSalonu.Controllers
{
    [Route("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController(BerberContext context) : Controller
    {
        private readonly BerberContext _context = context;

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("Berber/Ekle")]
        public IActionResult BerberEkle()
        {
            return View();
        }

        [HttpPost("Berber/Ekle")]
        public async Task<IActionResult> BerberEkle(KayitViewModel veri)
        {
            if (!ModelState.IsValid)
            {
                return View(veri);
            }

            var kullanici = await _context.Kullanicilar.FirstOrDefaultAsync(k => k.Eposta == veri.Email);

            if (kullanici != null)
            {
                ModelState.AddModelError("Email", "Eposta mevcut");
                return View(veri);
            }

            var hash = BCrypt.Net.BCrypt.HashPassword(veri.Password);
            var rol = await _context.Roller.FirstOrDefaultAsync(r => r.Name == "Berber")!;

            var yeniKullanici = new Kullanici
            {
                Ad = veri.FirstName,
                Soyad = veri.LastName,
                Eposta = veri.Email,
                SifreHashi = hash,
                Rol = rol!
            };

            var yeniBerber = new Berber
            {
                Kullanici = yeniKullanici,
            };

            _context.Add(yeniBerber);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Admin");
        }

        [HttpGet("Berber/YetenekEkle")]
        public async Task<IActionResult> YetenekEkle([FromQuery] int? berberId)
        {
            var model = new BerberYetenekEkleViewModel();
            await EklenebilecekYeteneklerinSelectListiniGetir(model, berberId);
            return View(model);
        }

        [HttpPost("Berber/YetenekEkle")]
        public async Task<IActionResult> YetenekEkle(BerberYetenekEkleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await EklenebilecekYeteneklerinSelectListiniGetir(model);
                return View(model);
            }

            var berber = await _context.Berberler
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == model.BerberId);

            var yetenek = await _context.Yetenekler
                .AsNoTracking()
                .FirstOrDefaultAsync(y => y.Id == model.YetenekId);

            if (berber == null || yetenek == null)
            {
                TempData["Hata"] = "Geçersiz veriler.";
                return RedirectToAction(nameof(YetenekEkle));
            }

            var mevcutIliski = await _context.BerberYetenekler
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BerberId == model.BerberId && b.YetenekId == model.YetenekId);

            if (mevcutIliski != null)
            {
                TempData["Hata"] = "Bu yetenek zaten eklenmiş!";
                await EklenebilecekYeteneklerinSelectListiniGetir(model);
                return View(model);
            }

            var yeniIliski = new BerberYetenek
            {
                BerberId = model.BerberId,
                YetenekId = model.YetenekId
            };

            _context.BerberYetenekler.Add(yeniIliski);
            await _context.SaveChangesAsync();

            TempData["Başarı"] = "Berbere yetenek ekleme başarılı!";
            return RedirectToAction(nameof(YetenekEkle));
        }

        [HttpGet("Berber/YetenekSil")]
        public async Task<IActionResult> YetenekSil([FromQuery] int? berberId)
        {
            var model = new BerberYetenekEkleViewModel();
            await SilinebilecekYeteneklerinSelectListiniGetir(model, berberId);
            return View(model);
        }

        [HttpPost("Berber/YetenekSil")]
        public async Task<IActionResult> YetenekSil(BerberYetenekEkleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await SilinebilecekYeteneklerinSelectListiniGetir(model);
                return View(model);
            }

            var berber = await _context.Berberler
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == model.BerberId);

            var yetenek = await _context.Yetenekler
                .AsNoTracking()
                .FirstOrDefaultAsync(y => y.Id == model.YetenekId);

            if (berber == null || yetenek == null)
            {
                TempData["Hata"] = "Geçersiz veriler.";
                return RedirectToAction(nameof(YetenekEkle));
            }

            var mevcutIliski = await _context.BerberYetenekler
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BerberId == model.BerberId && b.YetenekId == model.YetenekId);

            if (mevcutIliski == null)
            {
                TempData["Hata"] = "Bu yetenek zaten berbere ekli değil!";
                await SilinebilecekYeteneklerinSelectListiniGetir(model);
                return View(model);
            }

            _context.BerberYetenekler.Remove(mevcutIliski);
            await _context.SaveChangesAsync();

            TempData["Başarı"] = "Berberden yetenek silme başarılı!";
            return RedirectToAction(nameof(YetenekSil));
        }

        [HttpGet("kazanclarilistele")]
        public async Task<IActionResult> KazanclariListele()
        {
            // Berberlerin kazançlarını hesapla (Sadece onaylı randevular)
            var kazancListesi = await _context.Randevular
                .Include(r => r.Berber)   // Berber bilgilerini dahil et
                .Include(r => r.Yetenek)  // Yetenek bilgilerini dahil et
                .Where(r => r.IsOnaylandi == true)  // Sadece onaylanmış randevuları al
                .GroupBy(r => r.BerberId)  // BerberId'ye göre grupla
                .Select(g => new BerberKazanciViewModel
                {
                    BerberAd = g.FirstOrDefault().Berber.Kullanici.Ad + " " + g.FirstOrDefault().Berber.Kullanici.Soyad,
                    ToplamKazanc = g.Sum(r => r.Yetenek.Price)  // Yeteneklerin fiyatlarını topla
                }).ToListAsync();

            // Admin paneli için model oluştur
            var model = new AdminViewModel
            {
                BerberKazancListesi = kazancListesi
            };

            // View'e model gönder
            return View(model);
        }

        private async Task EklenebilecekYeteneklerinSelectListiniGetir(BerberYetenekEkleViewModel model,int? berberId = null)
        {
            var berberler = await _context.Berberler
                .AsNoTracking()
                .Include(b => b.Kullanici)
                .Select(b => new
                {
                    b.Id,
                    FullName = b.Kullanici.Ad + " " + b.Kullanici.Soyad
                })
                .ToListAsync();

            model.BerberlerSelectList = new SelectList(berberler, "Id", "FullName");

            if (berberId.HasValue)
            {
                var mevcutYetenekler = await _context.BerberYetenekler
                    .AsNoTracking()
                    .Where(by => by.BerberId == berberId.Value)
                    .Select(by => by.YetenekId)
                    .ToListAsync();

                var yetenekler = await _context.Yetenekler
                    .AsNoTracking()
                    .Where(y => !mevcutYetenekler.Contains(y.Id))
                    .Select(y => new
                    {
                        y.Id,
                        Ad = y.Name
                    })
                    .ToListAsync();

                model.YeteneklerSelectList = new SelectList(yetenekler, "Id", "Ad");

                if (!yetenekler.Any())
                {
                    TempData["Bilgi"] = "Berbere eklenebilecek başka bir yetenek yok.";
                }
            }
            else
            {
                model.YeteneklerSelectList = new SelectList(Enumerable.Empty<SelectListItem>());
            }
        }

        private async Task SilinebilecekYeteneklerinSelectListiniGetir(BerberYetenekEkleViewModel model, int? berberId = null)
        {
            var berberler = await _context.Berberler
                .AsNoTracking()
                .Include(b => b.Kullanici)
                .Select(b => new
                {
                    b.Id,
                    FullName = b.Kullanici.Ad + " " + b.Kullanici.Soyad
                })
                .ToListAsync();

            model.BerberlerSelectList = new SelectList(berberler, "Id", "FullName");

            if (berberId.HasValue)
            {
                var mevcutYetenekler = await _context.BerberYetenekler
                    .AsNoTracking()
                    .Where(by => by.BerberId == berberId.Value)
                    .Select(by => by.YetenekId)
                    .ToListAsync();

                var yetenekler = await _context.Yetenekler
                    .AsNoTracking()
                    .Where(y => mevcutYetenekler.Contains(y.Id))
                    .Select(y => new
                    {
                        y.Id,
                        Ad = y.Name
                    })
                    .ToListAsync();

                model.YeteneklerSelectList = new SelectList(yetenekler, "Id", "Ad");

                if (!yetenekler.Any())
                {
                    TempData["Bilgi"] = "Berberin silinebilecek herhangi bir yeteneği yok.";
                }
            }
            else
            {
                model.YeteneklerSelectList = new SelectList(Enumerable.Empty<SelectListItem>());
            }
        }

    }
}

