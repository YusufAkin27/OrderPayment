# OrderPayment

ASP.NET Core 8.0 tabanlı sipariş ve ödeme yönetim uygulaması. Kullanıcı kaydı, giriş, sepet, sipariş ve Iyzipay entegrasyonu ile ödeme işlemlerini destekler.

## Özellikler

- **Kimlik doğrulama:** Kayıt, giriş, şifremi unuttum (SMS doğrulama kodu ile)
- **Kullanıcı:** Profil, adres yönetimi, kayıtlı kartlar
- **Sepet:** Ürün ekleme, güncelleme, silme
- **Sipariş:** Adres ve kart seçimi, sipariş onayı
- **Ödeme:** Iyzipay entegrasyonu (kredi kartı ödemesi)
- **Admin paneli:** Kullanıcılar, ürünler, sipariş listesi ve yönetimi

## Gereksinimler

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB veya tam sürüm)
- (İsteğe bağlı) Twilio hesabı – SMS doğrulama için
- (İsteğe bağlı) Iyzipay API anahtarları – ödeme için

## Kurulum

1. Depoyu klonlayın veya indirin.

2. Bağlantı dizesini ayarlayın. `appsettings.json` içindeki `ConnectionStrings:DefaultConnection` değerini kendi SQL Server bilgilerinize göre güncelleyin:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=SUNUCU_ADI;Database=OrderPaymentDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

3. Veritabanı migration’larını uygulayın:

```bash
dotnet ef database update
```

4. Uygulamayı çalıştırın:

```bash
dotnet run
```

Varsayılan olarak uygulama şu adreslerde açılır:

- HTTPS: `https://localhost:7297`
- HTTP: `http://localhost:5033`

## Proje yapısı

| Klasör / Dosya   | Açıklama                          |
|------------------|-----------------------------------|
| `Controllers/`   | Auth, Cart, Order, Payment, Admin, User, Address, UserCard |
| `Models/`        | Entity ve request modelleri       |
| `Views/`         | Razor sayfaları (Auth, Cart, Order, Admin, User, Address) |
| `wwwroot/`       | CSS, JS, Bootstrap, jQuery        |
| `Migrations/`    | EF Core veritabanı migration’ları  |

## Kullanılan teknolojiler

- **ASP.NET Core 8.0** – MVC, Cookie Authentication, Session
- **Entity Framework Core 8** – SQL Server
- **BCrypt.Net-Next** – Şifre hash
- **Iyzipay** – Ödeme altyapısı
- **Twilio** – SMS doğrulama kodu
- **Bootstrap** – Arayüz
- **jQuery** – İstemci tarafı script

## Yapılandırma notları

- **SMS (Twilio):** `SmsService` ve ilgili ayarlar `appsettings` veya environment değişkenleri ile yapılandırılmalıdır.
- **Iyzipay:** `PaymentController` / `PaymentsController` içindeki API anahtarı ve BaseUrl değerleri güvenli bir yapılandırmadan (ör. `appsettings.json`, User Secrets veya environment) okunmalıdır.
- **Veritabanı:** İlk çalıştırmada `dotnet ef database update` ile tablolar oluşturulur.

## Lisans

Bu proje eğitim / kişisel kullanım amaçlıdır.
