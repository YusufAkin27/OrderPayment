using Microsoft.EntityFrameworkCore;
using OrderPayment.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(crf=>
{
    crf.AddDefaultPolicy(policy =>
    policy.AllowAnyHeader().AllowCredentials().AllowAnyMethod().SetIsOriginAllowed(policy => true));
});
// Servisleri ekleyin
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();

// SmsService baðýmlýlýðýný ekleyin
builder.Services.AddSingleton<SmsService>();

// SQL Server baðlantýsý ile DbContext ekleyin
builder.Services.AddDbContext<OrderPaymentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Session ve MemoryCache ekleyin
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Oturum süresi
    options.Cookie.HttpOnly = true;                // Güvenlik için sadece HTTP eriþimi
    options.Cookie.IsEssential = true;             // GDPR uyumluluðu
});



var app = builder.Build();

// Hata iþleme ve güvenlik yapýlandýrmalarý
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Geliþtirme ortamýnda hata mesajlarýný göster
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Session'ý etkinleþtir
app.UseSession();

app.UseCors();
app.UseAuthorization();

// Varsayýlan rota ayarýný SmsController'daki SendSms action'ýna yönlendirin
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Cart}/{action=Cart}/{id?}"
);

app.Run();
