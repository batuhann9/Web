using BerberSalonu.Veritabaný;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Net;

// 1. Servisler ekleniyor
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// 2. Cookie tabanlý Authentication yapýlandýrmasý
builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
    .AddCookie(o =>
    {
        o.ExpireTimeSpan = TimeSpan.FromMinutes(30);  // Oturum süresi
        o.SlidingExpiration = true;
        o.LoginPath = "/giris";  // Giriþ yapýlmadýysa yönlendirilecek sayfa
        o.AccessDeniedPath = "/yetkisiz";  // Yetkisiz giriþ için yönlendirme
    });

// 3. DbContext (veritabaný baðlantýsý)
builder.Services.AddDbContext<BerberContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// 4. HttpClient için Cookie Desteði
builder.Services.AddHttpClient("AuthorizedClient")
    .ConfigurePrimaryHttpMessageHandler(() =>
    {
        return new HttpClientHandler
        {
            UseCookies = true,  // Cookie kullan
            CookieContainer = new CookieContainer(),  // Cookie taþýyýcýsý
            UseDefaultCredentials = true  // Varsayýlan oturumu kullan
        };
    });

builder.Services.AddHttpContextAccessor();  // HttpContext eriþimi için ekleme

var app = builder.Build();

// 5. Hata yönetimi
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