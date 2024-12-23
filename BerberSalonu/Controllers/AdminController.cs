using BerberSalonu.Models;
using BerberSalonu.Veritabanı;
using BerberSalonu.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BerberSalonu.Controllers
{
    [Route("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly BerberContext _context;

        public AdminController(BerberContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Yeni berber ekleme
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

            var kullaniciVarMi = await _context.Kullanicilar
                .AnyAsync(k => k.Eposta == veri.Email);

            if (kullaniciVarMi)
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten mevcut.");
                return View(veri);
            }

            var hash = BCrypt.Net.BCrypt.HashPassword(veri.Password);
            var rol = await _context.Roller.FirstOrDefaultAsync(r => r.Name == "Berber");

            if (rol == null)
            {
                ModelState.AddModelError("", "Berber rolü bulunamadı.");
                return View(veri);
            }

            var yeniKullanici = new Kullanici
            {
                Ad = veri.FirstName,
                Soyad = veri.LastName,
                Eposta = veri.Email,
                SifreHashi = hash,
                Rol = rol
            };

            var yeniBerber = new Berber
            {
                Kullanici = yeniKullanici,
            };

            _context.Add(yeniBerber);
            await _context.SaveChangesAsync();

            TempData["Mesaj"] = "Berber başarıyla eklendi.";
            return RedirectToAction("Index");
        }

        // Yetenek Ekleme İşlemi
        [HttpGet("Berber/YetenekEkle")]
        public async Task<IActionResult> YetenekEkle([FromQuery] int? berberId)
        {
            var model = await YetenekViewModelHazirla(berberId);
            return View(model);
        }

        [HttpPost("Berber/YetenekEkle")]
        public async Task<IActionResult> YetenekEkle(BerberYetenekEkleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model = await YetenekViewModelHazirla(model.BerberId);
                return View(model);
            }

            if (await _context.BerberYetenekler
                .AnyAsync(b => b.BerberId == model.BerberId && b.YetenekId == model.YetenekId))
            {
                TempData["Hata"] = "Bu yetenek zaten eklenmiş!";
                model = await YetenekViewModelHazirla(model.BerberId);
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
            return RedirectToAction(nameof(YetenekEkle), new { berberId = model.BerberId });
        }

    
       [HttpGet("Berber/YetenekSil")]
        public async Task<IActionResult> YetenekSil([FromQuery] int? berberId)
        {
            var model = await YetenekViewModelHazirla(berberId, true);
            return View(model);
        }

        [HttpPost("Berber/YetenekSil")]
        public async Task<IActionResult> YetenekSil(BerberYetenekEkleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model = await YetenekViewModelHazirla(model.BerberId, true);
                return View(model);
            }

            var mevcutIliski = await _context.BerberYetenekler
                .FirstOrDefaultAsync(b => b.BerberId == model.BerberId && b.YetenekId == model.YetenekId);

            if (mevcutIliski == null)
            {
                TempData["Hata"] = "Bu yetenek zaten ekli değil!";
                model = await YetenekViewModelHazirla(model.BerberId, true);
                return View(model);
            }

            _context.BerberYetenekler.Remove(mevcutIliski);
            await _context.SaveChangesAsync();

            TempData["Başarı"] = "Berberden yetenek silme başarılı!";
            return RedirectToAction(nameof(YetenekSil), new { berberId = model.BerberId });
        }

        [HttpGet("Berber/Sil")]
        public async Task<IActionResult> BerberSil()
        {
            var model = new BerberSilViewModel
            {
                BerberlerSelectList = await _context.Berberler
                    .Include(b => b.Kullanici)
                    .Select(b => new SelectListItem
                    {
                        Value = b.Id.ToString(),
                        Text = $"{b.Kullanici.Ad} {b.Kullanici.Soyad}"
                    }).ToListAsync()
            };

            return View(model);
        }

        [HttpPost("Berber/Sil")]
        public async Task<IActionResult> BerberSil(BerberSilViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.BerberlerSelectList = await _context.Berberler
                    .Include(b => b.Kullanici)
                    .Select(b => new SelectListItem
                    {
                        Value = b.Id.ToString(),
                        Text = $"{b.Kullanici.Ad} {b.Kullanici.Soyad}"
                    }).ToListAsync();

                return View(model);
            }

            var berber = await _context.Berberler
                .FirstOrDefaultAsync(b => b.Id == model.BerberId);

            if (berber == null)
            {
                TempData["Hata"] = "Berber bulunamadı!";
                return RedirectToAction(nameof(BerberSil));
            }

            // Randevuları kontrol et
            var aktifRandevu = await _context.Randevular
                .AnyAsync(r => r.BerberId == model.BerberId && r.RandevuTarihi >= DateOnly.FromDateTime(DateTime.Now));

            if (aktifRandevu)
            {
                TempData["Hata"] = "Bu berberin aktif randevuları olduğu için silinemez!";
                return RedirectToAction(nameof(BerberSil));
            }

            _context.Berberler.Remove(berber);
            await _context.SaveChangesAsync();

            TempData["Başarı"] = "Berber başarıyla silindi!";
            return RedirectToAction(nameof(BerberSil));
        }

        // Kazançları Listele
        [HttpGet("Berber/KazanclariListele")]
        public async Task<IActionResult> KazanclariListele()
        {
            var kazancListesi = await _context.Randevular
                .Include(r => r.Berber)
                .Include(r => r.Yetenek)
                .Where(r => r.IsOnaylandi == true)
                .GroupBy(r => r.BerberId)
                .Select(g => new BerberKazanciViewModel
                {
                    BerberAd = g.First().Berber.Kullanici.Ad + " " + g.First().Berber.Kullanici.Soyad,
                    ToplamKazanc = g.Sum(r => r.Yetenek.Price)
                }).ToListAsync();

            return View(new AdminViewModel { BerberKazancListesi = kazancListesi });
        }

        // Yetenek ViewModel Hazırlama
        private async Task<BerberYetenekEkleViewModel> YetenekViewModelHazirla(int? berberId, bool silinebilir = false)
        {
            var model = new BerberYetenekEkleViewModel();
            model.BerberlerSelectList = new SelectList(await _context.Berberler
                .Include(b => b.Kullanici)
                .Select(b => new
                {
                    b.Id,
                    FullName = b.Kullanici.Ad + " " + b.Kullanici.Soyad
                }).ToListAsync(), "Id", "FullName");

            if (berberId.HasValue)
            {
                var mevcutYetenekler = _context.BerberYetenekler
                    .Where(by => by.BerberId == berberId.Value)
                    .Select(by => by.YetenekId);

                model.YeteneklerSelectList = new SelectList(await _context.Yetenekler
                    .Where(y => silinebilir ? mevcutYetenekler.Contains(y.Id) : !mevcutYetenekler.Contains(y.Id))
                    .ToListAsync(), "Id", "Name");
            }
            return model;
        }
    }
}