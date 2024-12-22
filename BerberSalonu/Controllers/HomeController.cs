using BerberSalonu.Models;
using BerberSalonu.Veritabaný;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BerberSalonu.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BerberContext _context;

        public HomeController(ILogger<HomeController> logger, BerberContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // Hizmetlerimiz sayfasý
        public async Task<IActionResult> Hizmetlerimiz()
        {
            var hizmetler = await _context.Yetenekler.ToListAsync();
            return View(hizmetler);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
