using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using OrderPayment.Controllers;
using OrderPayment.Models;

var builder = WebApplication.CreateBuilder(args);

// CORS yap�land�rmas� (�erezlere izin verilecek �ekilde d�zenlendi)
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials() // �erezlerin kullan�lmas�na izin ver
              .SetIsOriginAllowed(_ => true); // Geli�tirme i�in t�m kaynaklara izin ver
    });
});

// Kimlik do�rulama ve yetkilendirme yap�land�rmas�
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Admin/AdminLogin"; // Giri� sayfas�
        options.AccessDeniedPath = "/Admin/AccessDenied"; // Eri�im reddedildi�inde y�nlendirilecek sayfa
        options.Cookie.HttpOnly = true; // �erezlere yaln�zca HTTP �zerinden eri�im
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // HTTPS kullan�yorsan�z "Always" olarak ayarlay�n
        options.Cookie.SameSite = SameSiteMode.Strict; // �erezin ba�ka sitelerden eri�ilmesini engelle
    });

// MVC ve API Controller servislerini ekleyin
builder.Services.AddControllersWithViews();

// SmsService ba��ml�l���n� singleton olarak ekleyin
builder.Services.AddSingleton<SmsService>();

// SQL Server ba�lant�s� ile DbContext ekleyin
builder.Services.AddDbContext<OrderPaymentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// IPasswordHasher<Admin> servisini ekleyin
builder.Services.AddScoped<IPasswordHasher<Admin>, PasswordHasher<Admin>>();

// Session ve MemoryCache ekleyin
builder.Services.AddDistributedMemoryCache(); // Session verilerini bellekte saklamak i�in
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Oturum s�resi 30 dakika
    options.Cookie.HttpOnly = true; // �erezlere yaln�zca HTTP �zerinden eri�im
    options.Cookie.IsEssential = true; // GDPR uyumlulu�u i�in bu �erezi gerekli k�l
    options.Cookie.SameSite = SameSiteMode.Strict; // �erezlerin ba�ka sitelerden eri�ilmesini engelle
});

var app = builder.Build();

// Hata i�leme ve g�venlik yap�land�rmalar�
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Geli�tirme ortam�nda ayr�nt�l� hata sayfalar�n� g�ster
}
else
{
    app.UseExceptionHandler("/Home/Error"); // �retim ortam�nda hata y�netimi i�in kullan�c� dostu sayfa
    app.UseHsts(); // HTTPS zorunlu hale getirilir (HTTP Strict Transport Security)
}

app.UseHttpsRedirection();  // HTTP'den HTTPS'ye y�nlendirme
app.UseStaticFiles();       // Statik dosyalar� sunma (CSS, JS, resimler vs.)
app.UseRouting();           // Y�nlendirme i�lemlerini etkinle�tir

// Session'� ve CORS'i etkinle�tir
app.UseSession();
app.UseCors();

// Kimlik do�rulama ve yetkilendirme middleware'lerini etkinle�tir
app.UseAuthentication(); // Kimlik do�rulamay� etkinle�tir
app.UseAuthorization();  // Yetkilendirme kontrol�n� etkinle�tir

// Varsay�lan rota ayar�n� d�zenleyin
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}"
);

app.Run(); // Uygulamay� �al��t�r
