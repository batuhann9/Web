using BerberSalonu.Veritabanı;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

[Route("api/admin")]
[Authorize(Roles = "Admin")]
[ApiController]
public class AdminApiController : ControllerBase
{
    private readonly BerberContext _context;

    // Doğru Yapıcı Metot (Constructor)
    public AdminApiController(BerberContext context)
    {
        _context = context;
    }

    // API Metodu
    [HttpGet("berberler")]
    public async Task<IActionResult> GetBerberler()
    {
        var data = await _context.Berberler
            .Include(b => b.Kullanici)
            .Select(b => new BerberViewModel
            {
                Id = b.Id,
                AdSoyad = b.Kullanici.Ad + " " + b.Kullanici.Soyad,
                Email = b.Kullanici.Eposta
            }).ToListAsync();

        return Ok(data);
    }
}
