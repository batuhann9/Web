using BerberSalonu.Veritabanı;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BerberSalonu.Components
{
    public class UserInfoViewComponent(BerberContext context) : ViewComponent
    {
        private readonly BerberContext _context = context;

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var email = HttpContext.User.FindFirst(ClaimTypes.Name)!.Value;

            var kullanici = await _context.Kullanicilar
                .FirstOrDefaultAsync(k => k.Eposta == email);

            if (kullanici == null)
            {
                throw new Exception();
            }

            return View("Default", kullanici);
        }
    }
}
