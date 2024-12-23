using BerberSalonu.Models;
using BerberSalonu.Veritabaný;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Drawing;
using System.Drawing.Imaging;

namespace BerberSalonu.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BerberContext _context;
        private readonly IConfiguration _configuration;

        public HomeController(ILogger<HomeController> logger, BerberContext context, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

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

        [HttpPost]
        public async Task<IActionResult> FotografYukle(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Lütfen bir fotoðraf yükleyin.");
                return View("Index");
            }

            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var filePath = Path.Combine(uploadPath, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var resizedFilePath = ResizeImage(filePath, 512, 512); // Dinamik boyutlandýrma
            var analizSonucu = await FotografAnalizEt(resizedFilePath);

            ViewBag.Analiz = analizSonucu;
            return View("Index");
        }

        private string ResizeImage(string filePath, int width, int height)
        {
            using (var img = Image.FromFile(filePath))
            {
                var resized = new Bitmap(img, new Size(width, height));
                var resizedPath = Path.Combine(Path.GetDirectoryName(filePath), $"resized_{Path.GetFileName(filePath)}");

                var jpegEncoder = ImageCodecInfo.GetImageDecoders().First(c => c.FormatID == ImageFormat.Jpeg.Guid);
                var encoderParams = new EncoderParameters(1);
                encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 50L);  // %50 kalite

                resized.Save(resizedPath, jpegEncoder, encoderParams);
                return resizedPath;
            }
        }

        private int TahminiTokenSayisi(string base64Image)
        {
            return (base64Image.Length * 3) / 4 / 1024; // Daha hassas token hesaplama
        }

        private async Task<string> FotografAnalizEt(string filePath)
        {
            var httpClient = new HttpClient();
            var imageBytes = System.IO.File.ReadAllBytes(filePath);
            var base64Image = Convert.ToBase64String(imageBytes);

            var apiKey = _configuration["GroqCloud:ApiKey"];

            if (TahminiTokenSayisi(base64Image) > 7000)
            {
                return "Fotoðraf çok büyük, lütfen daha düþük çözünürlüklü bir fotoðraf yükleyin.";
            }

            var requestBody = new
            {
                model = "llama-3.2-90b-vision-preview",
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new { type = "text", text = "Paylaþtýðým görsele uygun bir saç modeli veya bir saç rengi önerir misin?" },
                            new { type = "image_url", image_url = new { url = $"data:image/jpeg;base64,{base64Image}" } }
                        }
                    }
                },
                max_tokens = 500
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var stringContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            var response = await httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", stringContent);

            if (!response.IsSuccessStatusCode)
            {
                var hataMesaji = await response.Content.ReadAsStringAsync();
                return $"API hatasý: {hataMesaji}. Lütfen fotoðraf boyutunu kontrol edin veya daha düþük çözünürlüklü bir fotoðraf yükleyin.";
            }

            var result = await response.Content.ReadAsStringAsync();
            var jsonResponse = JsonConvert.DeserializeObject<dynamic>(result);
            return jsonResponse.choices[0].message.content.ToString();
        }
    }
}