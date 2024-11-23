using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using OrderPayment.Controllers;
using OrderPayment.Models;

var builder = WebApplication.CreateBuilder(args);

// CORS yapýlandýrmasý (çerezlere izin verilecek þekilde düzenlendi)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials() // Çerezlerin kullanýlmasýna izin ver
              .SetIsOriginAllowed(_ => true); // Geliþtirme için tüm kaynaklara izin ver
    });
});

// Kimlik doðrulama ve yetkilendirme yapýlandýrmasý
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/AdminLogin"; // Giriþ sayfasý
        options.AccessDeniedPath = "/Admin/AccessDenied"; // Eriþim reddedildiðinde yönlendirilecek sayfa
        options.Cookie.HttpOnly = true; // Çerezlere yalnýzca HTTP üzerinden eriþim
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTPS kullanýyorsanýz "Always" olarak ayarlayýn
        options.Cookie.SameSite = SameSiteMode.Strict; // Çerezin baþka sitelerden eriþilmesini engelle
    });

// MVC ve API Controller servislerini ekleyin
builder.Services.AddControllersWithViews();

// SmsService baðýmlýlýðýný singleton olarak ekleyin
builder.Services.AddSingleton<SmsService>();

// SQL Server baðlantýsý ile DbContext ekleyin
builder.Services.AddDbContext<OrderPaymentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// IPasswordHasher<Admin> servisini ekleyin
builder.Services.AddScoped<IPasswordHasher<Admin>, PasswordHasher<Admin>>();

// Session ve MemoryCache ekleyin
builder.Services.AddDistributedMemoryCache(); // Session verilerini bellekte saklamak için
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Oturum süresi 30 dakika
    options.Cookie.HttpOnly = true; // Çerezlere yalnýzca HTTP üzerinden eriþim
    options.Cookie.IsEssential = true; // GDPR uyumluluðu için bu çerezi gerekli kýl
    options.Cookie.SameSite = SameSiteMode.Strict; // Çerezlerin baþka sitelerden eriþilmesini engelle
});

var app = builder.Build();

// Hata iþleme ve güvenlik yapýlandýrmalarý
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Geliþtirme ortamýnda ayrýntýlý hata sayfalarýný göster
}
else
{
    app.UseExceptionHandler("/Home/Error"); // Üretim ortamýnda hata yönetimi için kullanýcý dostu sayfa
    app.UseHsts(); // HTTPS zorunlu hale getirilir (HTTP Strict Transport Security)
}

app.UseHttpsRedirection();  // HTTP'den HTTPS'ye yönlendirme
app.UseStaticFiles();       // Statik dosyalarý sunma (CSS, JS, resimler vs.)
app.UseRouting();           // Yönlendirme iþlemlerini etkinleþtir

// Session'ý ve CORS'i etkinleþtir
app.UseSession();
app.UseCors();

// Kimlik doðrulama ve yetkilendirme middleware'lerini etkinleþtir
app.UseAuthentication(); // Kimlik doðrulamayý etkinleþtir
app.UseAuthorization();  // Yetkilendirme kontrolünü etkinleþtir

// Varsayýlan rota ayarýný düzenleyin
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}"
);

app.Run(); // Uygulamayý çalýþtýr
