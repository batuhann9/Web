using BerberSalonu.Models;
using BerberSalonu.Veritabaný;
using BerberSalonu.vm;
using Microsoft.AspNetCore.Authorization;
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

        //public async Task<IActionResult> Index()
        //{


        //    var berber = await _context.Berberler
        //        .Include(b => b.Yetenekler)
        //        .FirstOrDefaultAsync(b => b.Id == 1);

        //    if (berber == null)
        //    {
        //        return BadRequest("Berber yok kanka");
        //    }

        //    var berberViewModel = new BerberViewModel
        //    {
        //        Name = berber.Name,
        //        Age = berber.Age,
        //        Zaman = DateTime.Now.ToString(),
        //        Yetenekler = berber.Yetenekler,
        //    };


        //    return Ok(berberViewModel);
        //}

        public IActionResult Index()
        {
            //var berberler=_context.Berberler
            //    .Include(b => b.Yetenekler)
            //    .ToList();
            return View(/*berberler*/);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
