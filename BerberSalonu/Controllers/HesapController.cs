using BerberSalonu.Models;
using BerberSalonu.Veritabanı;
using BerberSalonu.ViewModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BerberSalonu.Controllers
{
    public class HesapController : Controller
    {
        private readonly BerberContext _context;

        public HesapController(BerberContext context)
        {
            _context = context;
        }

        [HttpGet("Kayit")]
        public IActionResult Kayit()
        {
            return View();
        }

        [HttpPost("Kayit")]
        public async Task<IActionResult> Kayit(KayitViewModel veri)
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
            var rol =  await _context.Roller.FirstOrDefaultAsync(r => r.Id == 2)!;

            var yeni = new Kullanici
            {
                Ad = veri.FirstName,
                Soyad = veri.LastName,
                Eposta = veri.Email,
                SifreHashi = hash,
                Rol = rol!
            };

            _context.Add(yeni);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        [HttpGet("Giris")]
        public IActionResult Giris()
        {
            return View();
        }


        [HttpPost("Giris")]
        public async Task<IActionResult> Giris(GirisViewModel veri)
        {
            if (!ModelState.IsValid)
            {
                return View(veri);
            }

            var kullanici = await _context.Kullanicilar
                .Include(k => k.Rol)
                .FirstOrDefaultAsync(k => k.Eposta == veri.Email);

            if (kullanici == null)
            {
                ModelState.AddModelError("", "Eposta veya şifre yanlış.");
                return View(veri);
            }

            if (!BCrypt.Net.BCrypt.Verify(veri.Password, kullanici.SifreHashi))
            {
                ModelState.AddModelError("", "Eposta veya şifre yanlış.");
                return View(veri);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, kullanici.Eposta),
                new Claim(ClaimTypes.Role, kullanici.Rol.Name)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            
            var claimPrincipal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimPrincipal);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Cikis(string returnUrl = null!)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);


            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

    }
}
