using BerberSalonu.Models;
using BerberSalonu.Veritabanı;
using BerberSalonu.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BerberSalonu.Controllers
{
    [Route("Admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly BerberContext _context;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AdminController(BerberContext context, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient("AuthorizedClient");
            _httpContextAccessor = httpContextAccessor;
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

            // Onaylanmış randevuları kontrol et
            var onaylanmisRandevuVar = await _context.Randevular
                .AnyAsync(r => r.BerberId == model.BerberId
                            && r.YetenekId == model.YetenekId
                            && r.Durum == RandevuDurum.Onaylandi);

            if (onaylanmisRandevuVar)
            {
                TempData["Hata"] = "Bu yeteneğe ait onaylanmış randevular olduğu için silinemez!";
                model = await YetenekViewModelHazirla(model.BerberId, true);
                return View(model);
            }

            // Onay bekleyen randevuları iptal et
            var onayBekleyenRandevular = await _context.Randevular
                .Where(r => r.BerberId == model.BerberId
                         && r.YetenekId == model.YetenekId
                         && r.Durum == RandevuDurum.OnayBekliyor)
                .ToListAsync();

            if (onayBekleyenRandevular.Any())
            {
                foreach (var randevu in onayBekleyenRandevular)
                {
                    randevu.Durum = RandevuDurum.IptalEdildi;
                }
                await _context.SaveChangesAsync();  // Randevuların iptal edilmesi kaydediliyor
            }

            // Yetenek ve berber ilişkisini sil
            _context.BerberYetenekler.Remove(mevcutIliski);
            await _context.SaveChangesAsync();

            TempData["Başarı"] = "Berberden yetenek başarıyla silindi. Onay bekleyen randevular iptal edildi.";
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

            // Onaylanmış randevuları kontrol et
            var onaylanmisRandevuVar = await _context.Randevular
                .AnyAsync(r => r.BerberId == model.BerberId && r.Durum == RandevuDurum.Onaylandi);

            if (onaylanmisRandevuVar)
            {
                TempData["Hata"] = "Bu berberin onaylanmış randevuları olduğu için silinemez!";
                return RedirectToAction(nameof(BerberSil));
            }

            // Onay bekleyen randevuları iptal et
            var onayBekleyenRandevular = await _context.Randevular
                .Where(r => r.BerberId == model.BerberId && r.Durum == RandevuDurum.OnayBekliyor)
                .ToListAsync();

            if (onayBekleyenRandevular.Any())
            {
                foreach (var randevu in onayBekleyenRandevular)
                {
                    randevu.Durum = RandevuDurum.IptalEdildi; // Randevu durumu iptal olarak güncelleniyor
                }
                await _context.SaveChangesAsync(); // İptallerin veritabanına kaydedilmesi
            }

            // Berberi sil
            _context.Berberler.Remove(berber);
            await _context.SaveChangesAsync();

            TempData["Başarı"] = "Berber başarıyla silindi ve onay bekleyen randevuları iptal edildi!";
            return RedirectToAction(nameof(BerberSil));
        }

        [HttpGet("Berber/KazanclariListele")]
        public async Task<IActionResult> KazanclariListele()
        {
            var kazancListesi = await _context.Randevular
                .Include(r => r.Berber)
                .ThenInclude(b => b.Kullanici)
                .Include(r => r.Yetenek)
                .Where(r => r.Durum == RandevuDurum.Gerceklesti || r.Durum == RandevuDurum.Onaylandi)
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
        [HttpGet("BerberListesi")]
        public async Task<IActionResult> BerberListesi()
        {
            // 1. Kullanıcı oturum açmış mı kontrol et
            var isAuthenticated = _httpContextAccessor.HttpContext.User.Identity.IsAuthenticated;
            if (!isAuthenticated)
            {
                return RedirectToAction("Giris", "Hesap");
            }

            // 2. Cookie'yi HttpClient'a taşı
            var cookies = _httpContextAccessor.HttpContext.Request.Cookies;

            var cookieContainer = new System.Net.CookieContainer();
            foreach (var cookie in cookies)
            {
                cookieContainer.Add(new Uri("https://localhost:7186"), new System.Net.Cookie(cookie.Key, cookie.Value));
            }

            var handler = new HttpClientHandler()
            {
                UseCookies = true,
                CookieContainer = cookieContainer
            };

            var client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://localhost:7186")
            };

            // 3. API'ye istek gönder
            var response = await client.GetAsync("/api/admin/berberler");

            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                var berberler = JsonSerializer.Deserialize<List<BerberViewModel>>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return View("BerberListesi", berberler);
            }
            else
            {
                Console.WriteLine($"API Hatası: {response.StatusCode}");
                ViewBag.ErrorMessage = "API'den veri alınamadı.";
                return View("BerberListesi", new List<BerberViewModel>());
            }
        }

        // Yetenek Ekleme Sayfası (GET)
        [HttpGet("Berber/GenelYetenekEkle")]
        public IActionResult GenelYetenekEkle()
        {
            return View(new YetenekViewModel());
        }

        // Yetenek Ekleme (POST)
        [HttpPost("Berber/GenelYetenekEkle")]
        public async Task<IActionResult> GenelYetenekEkle(YetenekViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Hata"] = "Lütfen formu eksiksiz doldurun.";
                return View(model);
            }

            try
            {
                // Aynı isimde yetenek var mı kontrol et
                var mevcutYetenek = await _context.Yetenekler
                    .FirstOrDefaultAsync(y => y.Name == model.Name);

                if (mevcutYetenek != null)
                {
                    TempData["Hata"] = "Bu yetenek zaten eklenmiş.";
                    return View(model);
                }

                // Yetenek oluştur
                var yeniYetenek = new Yetenek
                {
                    Name = model.Name,
                    Price = model.Price,
                    Sure = model.Sure
                };

                _context.Yetenekler.Add(yeniYetenek);
                await _context.SaveChangesAsync();

                TempData["Başarı"] = "Yetenek başarıyla eklendi.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Hata"] = "Bir hata oluştu: " + ex.Message;
                return View(model);
            }
        }

        // Yetenek Silme İşlemi
        [HttpGet("Berber/GenelYetenekSil")]
        public async Task<IActionResult> GenelYetenekSil()
        {
            var model = new YetenekSilViewModel
            {
                YeteneklerSelectList = await _context.Yetenekler
                    .Select(y => new SelectListItem
                    {
                        Value = y.Id.ToString(),
                        Text = y.Name
                    })
                    .ToListAsync()
            };

            return View(model);
        }

        [HttpPost("Berber/GenelYetenekSil")]
        public async Task<IActionResult> GenelYetenekSil(YetenekSilViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.YeteneklerSelectList = await _context.Yetenekler
                    .Select(y => new SelectListItem
                    {
                        Value = y.Id.ToString(),
                        Text = y.Name
                    })
                    .ToListAsync();
                return View(model);
            }

            var yetenek = await _context.Yetenekler.FindAsync(model.YetenekId);
            if (yetenek == null)
            {
                TempData["Hata"] = "Yetenek bulunamadı!";
                return RedirectToAction(nameof(GenelYetenekSil));
            }

            // Onaylanmış randevuları kontrol et
            var onaylanmisRandevuVar = await _context.Randevular
                .AnyAsync(r => r.YetenekId == model.YetenekId && r.Durum == RandevuDurum.Onaylandi);

            if (onaylanmisRandevuVar)
            {
                TempData["Hata"] = "Bu yeteneğe ait onaylanmış randevular olduğu için silinemez!";
                return RedirectToAction(nameof(GenelYetenekSil));
            }

            // Onay bekleyen randevuları iptal et
            var onayBekleyenRandevular = await _context.Randevular
                .Where(r => r.YetenekId == model.YetenekId && r.Durum == RandevuDurum.OnayBekliyor)
                .ToListAsync();

            if (onayBekleyenRandevular.Any())
            {
                foreach (var randevu in onayBekleyenRandevular)
                {
                    randevu.Durum = RandevuDurum.IptalEdildi;  // Bekleyen randevular iptal ediliyor
                }
                await _context.SaveChangesAsync();  // İptal işlemleri veritabanına kaydediliyor
            }

            // Yetenek siliniyor
            _context.Yetenekler.Remove(yetenek);
            await _context.SaveChangesAsync();

            TempData["Başarı"] = "Yetenek başarıyla silindi ve onay bekleyen randevuları iptal edildi!";
            return RedirectToAction(nameof(GenelYetenekSil));
        }

    }
}