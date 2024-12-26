using BerberSalonu.Veritaban�;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Net;

// 1. Servisler ekleniyor
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// 2. Cookie tabanl� Authentication yap�land�rmas�
builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
    .AddCookie(o =>
    {
        o.ExpireTimeSpan = TimeSpan.FromMinutes(30);  // Oturum s�resi
        o.SlidingExpiration = true;
        o.LoginPath = "/giris";  // Giri� yap�lmad�ysa y�nlendirilecek sayfa
        o.AccessDeniedPath = "/yetkisiz";  // Yetkisiz giri� i�in y�nlendirme
    });

// 3. DbContext (veritaban� ba�lant�s�)
builder.Services.AddDbContext<BerberContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// 4. HttpClient i�in Cookie Deste�i
builder.Services.AddHttpClient("AuthorizedClient")
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            UseCookies = true,  // Cookie kullan
            CookieContainer = new CookieContainer(),  // Cookie ta��y�c�s�
            UseDefaultCredentials = true  // Varsay�lan oturumu kullan
        };
    });

builder.Services.AddHttpContextAccessor();  // HttpContext eri�imi i�in ekleme

var app = builder.Build();

// 5. Hata y�netimi
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// 6. HTTPS, dosyalar, routing
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();  // Authentication devreye al
app.UseAuthorization();   // Authorization devreye al

// 7. Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();