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

// SmsService ba��ml�l���n� ekleyin
builder.Services.AddSingleton<SmsService>();

// SQL Server ba�lant�s� ile DbContext ekleyin
builder.Services.AddDbContext<OrderPaymentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Session ve MemoryCache ekleyin
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Oturum s�resi
    options.Cookie.HttpOnly = true;                // G�venlik i�in sadece HTTP eri�imi
    options.Cookie.IsEssential = true;             // GDPR uyumlulu�u
});



var app = builder.Build();

// Hata i�leme ve g�venlik yap�land�rmalar�
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage(); // Geli�tirme ortam�nda hata mesajlar�n� g�ster
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Session'� etkinle�tir
app.UseSession();

app.UseCors();
app.UseAuthorization();

// Varsay�lan rota ayar�n� SmsController'daki SendSms action'�na y�nlendirin
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=HomePage}/{id?}"
);

app.Run();
