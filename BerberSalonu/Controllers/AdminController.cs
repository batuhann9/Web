using BerberSalonu.Models;
using BerberSalonu.Veritabanı;
using BerberSalonu.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        [Route("Berber/Ekle")]
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
        public async Task<IActionResult> YetenekEkle()
        {
            var berberler = await _context.Berberler
                .Include(b => b.Kullanici)
                .ToListAsync();

            var yetenekler = await _context.Yetenekler
                .ToListAsync();

            var model = new BerberYetenekEkleViewModel
            {
                Berberler = berberler,
                Yetenekler = yetenekler
            };

            return View(model);
        }

        [HttpPost("Berber/YetenekEkle")]
        public IActionResult YetenekEkle(BerberYetenekEkleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var berber = _context.Berberler
                    .FirstOrDefault(b => b.Id == model.BerberId);
                var yetenek = _context.Yetenekler
                    .FirstOrDefault(y => y.Id == model.YetenekId);

                // Berber ve Yetenek varsa, ilişkili tabloya ekleyin
                if (berber != null && yetenek != null)
                {
                    var mevcutIliski = _context.BerberYetenekler
                        .FirstOrDefault(b => b.BerberId == model.BerberId && b.YetenekId == model.YetenekId);

                    if (mevcutIliski == null)
                    {
                        var yeniIliski = new BerberYetenek
                        {
                            BerberId = model.BerberId,
                            YetenekId = model.YetenekId
                        };

                        _context.BerberYetenekler.Add(yeniIliski);
                        _context.SaveChanges();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Bu yetenek zaten eklenmiş!");
                        return View(model);
                    }

                    return RedirectToAction("Index"); // Başarılı yönlendirme
                }
                else
                {
                    ModelState.AddModelError("", "Geçersiz veriler!");
                }
            }

            return View(model);
        }

    }
}

