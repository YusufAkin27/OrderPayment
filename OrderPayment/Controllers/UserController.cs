using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OrderPayment.Models;
using YourProjectNamespace.Models;

public class UserController : Controller
{
    private readonly SmsService _smsService;
    private readonly OrderPaymentDbContext _context;

    public UserController(SmsService smsService, OrderPaymentDbContext context)
    {
        _smsService = smsService;
        _context = context;
    }

	[HttpGet]
    public ActionResult UserHome()
    {
        var products = _context.products.ToList();
        return View(products);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }
    [HttpGet]
    public IActionResult VerifyCode()
    {
        return View();
    }

    [HttpGet]
    public IActionResult VerifyLoginCode()
    {
        return View();
    }


    [HttpPost]
    public IActionResult Register(User user)
    {
        // Telefon numarasının boş olup olmadığı kontrolü
        if (string.IsNullOrWhiteSpace(user.PhoneNumber))
        {
            return Json(new { success = false, message = "Telefon numarası gereklidir." });
        }

        // Şifre kontrolü (en az 6 karakter)
        if (string.IsNullOrWhiteSpace(user.Password) || user.Password.Length < 6)
        {
            return Json(new { success = false, message = "Şifre en az 6 karakter uzunluğunda olmalıdır." });
        }

        // Telefon numarasına göre kullanıcı kontrolü
        if (_context.Users.Any(u => u.PhoneNumber == user.PhoneNumber))
        {
            return Json(new { success = false, message = "Bu telefon numarasıyla zaten kayıtlı bir kullanıcı var." });
        }

        // Şifreyi hash'le
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password);

        // Doğrulama kodu oluştur
        var verificationCode = GenerateVerificationCode();

        // SMS mesajı oluştur
        var message = $"Sayın {user.FirstName} {user.LastName},\n\n" +
            "Hesabınızın güvenliği için aşağıdaki doğrulama kodunu kullanarak işleminizi tamamlayabilirsiniz:\n\n" +
            $"Doğrulama Kodu: {verificationCode}\n\n" +
            "Lütfen kodunuzu en kısa sürede giriniz. Kodunuzun geçerlilik süresi sınırlıdır.\n\n" +
            "Herhangi bir sorunuz varsa, destek ekibimizle iletişime geçebilirsiniz.\n\n" +
            "Teşekkür ederiz,\n" +
            "Hizmet Sağlayıcınız";

        // SMS gönderimi
        if (!_smsService.SendSms(user.PhoneNumber, message))
        {
            return Json(new { success = false, message = "SMS gönderilemedi. Lütfen daha sonra tekrar deneyin." });
        }

        // Kullanıcıyı kaydet
        user.IsActive = false; // Kullanıcı aktif değil
        user.CreatedAt = DateTime.UtcNow;

        _context.Users.Add(user);
        _context.SaveChanges();

        // Doğrulama kodunu kaydet
        var verification = new VerificationCode
        {
            Code = verificationCode,
            SentAt = DateTime.UtcNow,
            ExpiryInSeconds = 120, // 2 dakika geçerlilik süresi
            UserId = user.Id
        };

        _context.VerificationCodes.Add(verification);
        _context.SaveChanges();

        // Session'a kullanıcı bilgisi kaydet
        HttpContext.Session.SetString("User", JsonConvert.SerializeObject(user, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        }));

        return Json(new { success = true, message = "Kayıt başarılı! Doğrulama kodu gönderildi." });
    }





    [HttpPost]
    public IActionResult VerifyCode(string verificationCode)
    {
        // Session'dan kullanıcıyı al
        var userJson = HttpContext.Session.GetString("User");
        if (string.IsNullOrEmpty(userJson))
        {
            return Json(new { success = false, message = "Geçersiz kullanıcı verisi!" });
        }

        // Kullanıcıyı veritabanından al
        var user = JsonConvert.DeserializeObject<User>(userJson);
        user = _context.Users.FirstOrDefault(u => u.Id == user.Id);

        if (user == null)
        {
            return Json(new { success = false, message = "Kullanıcı bulunamadı!" });
        }

        // Doğrulama kodunu veritabanından kontrol et
        var verification = _context.VerificationCodes
            .Where(vc => vc.UserId == user.Id && vc.Code == verificationCode)
            .OrderByDescending(vc => vc.SentAt)
            .FirstOrDefault();

        if (verification != null)
        {
            // Kod süresi dolmuş mu kontrol et
            if (verification.IsExpired())
            {
                return Json(new { success = false, message = "Geçersiz veya süresi dolmuş doğrulama kodu!" });
            }

            // Kullanıcıyı aktif yap
            user.IsActive = true;
            _context.Users.Update(user); // Update ile güncelle

            // Doğrulama kodunu geçersiz hale getir
            verification.ExpiryInSeconds = 0;
            _context.VerificationCodes.Update(verification);

            try
            {
                _context.SaveChanges();

                // Güncellenmiş kullanıcı verisini session'a kaydet
                HttpContext.Session.SetString("User", JsonConvert.SerializeObject(user, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }));

                return Json(new { success = true, message = "Kullanıcı başarıyla kaydedildi ve aktif edildi." });
            }
            catch (Exception ex)
            {
                // Hata mesajı döndür
                return Json(new { success = false, message = "Bir hata oluştu. Lütfen tekrar deneyin.", error = ex.Message });
            }
        }
        else
        {
            return Json(new { success = false, message = "Geçersiz doğrulama kodu veya kodu daha önce kullanmışsınız." });
        }
    }





    [HttpPost]
    public IActionResult ResendVerificationCode()
    {
        // Kullanıcı verisini session'dan al ve deserialize et
        var userJson = HttpContext.Session.GetString("User");
        if (string.IsNullOrEmpty(userJson))
        {
            return Json(new { success = false, message = "Geçersiz kullanıcı verisi!" });
        }

        var user = JsonConvert.DeserializeObject<User>(userJson);
        if (user == null)
        {
            return Json(new { success = false, message = "Kullanıcı bulunamadı!" });
        }

        // Kullanıcının en son doğrulama kodunu al
        var existingVerification = _context.VerificationCodes
            .Where(vc => vc.UserId == user.Id) // Gelen user nesnesindeki Id'yi kullanıyoruz
            .OrderByDescending(vc => vc.SentAt) // En son gönderilen doğrulama kodu
            .FirstOrDefault();

        // Eğer mevcut doğrulama kodu varsa
        if (existingVerification != null)
        {
            // Süresi dolmuşsa yeni bir doğrulama kodu gönder
            if (existingVerification.IsExpired()) // Kodun süresinin dolup dolmadığını kontrol et
            {
                // Yeni doğrulama kodu oluştur
                var verificationCode = GenerateVerificationCode();

                // SMS gönderimi
                var message = $"Sayın {user.FirstName} {user.LastName},\n\n" +
              "Hesabınızın güvenliği için aşağıdaki doğrulama kodunu kullanarak işleminizi tamamlayabilirsiniz:\n\n" +
              $"Doğrulama Kodu: {verificationCode}\n\n" +
              "Lütfen kodunuzu en kısa sürede giriniz. Kodunuzun geçerlilik süresi sınırlıdır.\n\n" +
              "Herhangi bir sorunuz varsa, destek ekibimizle iletişime geçebilirsiniz.\n\n" +
              "Teşekkür ederiz,\n" +
              "Hizmet Sağlayıcınız";

                bool smsSent = _smsService.SendSms(user.PhoneNumber, message); // Kullanıcıya SMS gönder

                if (smsSent)
                {
                    // Yeni doğrulama kodunu veritabanına kaydet
                    existingVerification.Code = verificationCode;
                    existingVerification.SentAt = DateTime.UtcNow; // Kodun gönderilme zamanı güncelleniyor
                    existingVerification.ExpiryInSeconds = 120; // Kod geçerlilik süresi 60 saniye olarak ayarlandı

                    _context.SaveChanges(); // Güncellenmiş doğrulama kodunu kaydet

                    return Json(new { success = true, message = "Yeni doğrulama kodu gönderildi." });
                }
                else
                {
                    return Json(new { success = false, message = "Yeni doğrulama kodu gönderilemedi." });
                }
            }
            else
            {
                var remainingTime = existingVerification.GetRemainingTime(); // Kalan süreyi hesapla
                return Json(new { success = false, message = $"Doğrulama kodunuz henüz geçerliliğini kaybetmedi. Lütfen {remainingTime} kadar bekleyin." });
            }
        }
        else
        {
            // Eğer kullanıcı hiç doğrulama kodu almadıysa
            return Json(new { success = false, message = "Doğrulama kodu bulunamadı." });
        }
    }





    [HttpPost]
    public IActionResult GetRemainingTime()
    {
        // Session'dan kullanıcı bilgisini al
        var userJson = HttpContext.Session.GetString("User");

        // Eğer session'da kullanıcı bilgisi yoksa hata döndür
        if (string.IsNullOrEmpty(userJson))
        {
            return Json(new { success = false, message = "Kullanıcı bulunamadı." });
        }

        // JSON verisini User nesnesine dönüştür
        User user;
        try
        {
            user = JsonConvert.DeserializeObject<User>(userJson);
        }
        catch (JsonException ex)
        {
            return Json(new { success = false, message = "Kullanıcı bilgisi hatalı." });
        }

        // Kullanıcının son doğrulama kodunu al
        var verificationCode = _context.VerificationCodes
            .Where(vc => vc.UserId == user.Id)
            .OrderByDescending(vc => vc.SentAt) // En son gönderilen doğrulama kodu
            .FirstOrDefault();

        // Eğer doğrulama kodu yoksa hata döndür
        if (verificationCode == null)
        {
            return Json(new { success = false, message = "Doğrulama kodu bulunamadı." });
        }

        // Kalan süreyi hesapla
        int remainingTimeInSeconds = GetRemainingTimeForVerificationCode(verificationCode);
        var remainingTime = TimeSpan.FromSeconds(remainingTimeInSeconds);

        return Json(new
        {
            success = true,
            verificationCode = verificationCode.Code,  // Son doğrulama kodu
            remainingTime = remainingTime.ToString(@"mm\:ss")  // Kalan süre (dakika:saniye cinsinden)
        });
    }

    // Kalan süreyi hesaplayan metod
    // Kalan süreyi hesaplayan metod
    private int GetRemainingTimeForVerificationCode(VerificationCode verificationCode)
    {
        var expiryTime = verificationCode.SentAt.AddSeconds(verificationCode.ExpiryInSeconds); // Kodun geçerlilik süresi hesaplanıyor

        // Eğer süre dolmuşsa 0 döndür, değilse kalan süreyi döndür
        var remainingTime = (expiryTime - DateTime.UtcNow).TotalSeconds;

        return remainingTime > 0 ? (int)remainingTime : 0; // Süre 0'dan küçükse 0 döndür
    }

    private string GenerateVerificationCode()
    {
        return new Random().Next(100000, 999999).ToString();
    }


    [HttpPost]
    public IActionResult Login([FromBody] LoginRequest loginRequest)
    {
        // Model doğrulaması
        if (!ModelState.IsValid)
        {
            return Json(new { success = false, message = "Telefon numarası veya şifre hatalı." });
        }

        // Kullanıcıyı telefon numarasına göre veritabanında ara
        var user = _context.Users.FirstOrDefault(u => u.PhoneNumber == loginRequest.PhoneNumber);

        // Kullanıcı bulunamazsa hata mesajı döndür
        if (user == null)
        {
            return Json(new { success = false, message = "Bu telefon numarasına kayıtlı bir kullanıcı bulunamadı." });
        }

        // Şifreyi doğrula
        if (!BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
        {
            return Json(new { success = false, message = "Şifre hatalı." });
        }

        // Kullanıcı aktif değilse hata mesajı gönder
        if (!user.IsActive)
        {
            return Json(new { success = false, message = "Hesabınız doğrulanmamış. Lütfen doğrulama işlemini tamamlayın." });
        }

        // Kullanıcı aktif ve giriş başarılıysa, doğrulama kodu oluştur ve gönder
        var verificationCode = GenerateVerificationCode();

        // SMS mesajı
        var message = $"Sayın {user.FirstName} {user.LastName},\n\n" +
              "Güvenliğiniz için, hesabınıza giriş yapmak üzere doğrulama kodunuz aşağıda belirtilmiştir:\n\n" +
              $"Doğrulama Kodu: {verificationCode}\n\n" +
              "Bu kod 3 dakika boyunca geçerlidir. Lütfen bu süre içinde giriş işleminizi tamamlayınız.\n\n" +
              "İyi günler dileriz,\n" +
              "Destek Ekibi";


        // SMS gönderimi
        if (!_smsService.SendSms(user.PhoneNumber, message))
        {
            return Json(new { success = false, message = "Doğrulama kodu gönderilemedi. Lütfen daha sonra tekrar deneyin." });
        }

        // Doğrulama kodunu veritabanına kaydet
        var newVerification = new VerificationCode
        {
            UserId = user.Id,
            Code = verificationCode,
            SentAt = DateTime.UtcNow,
            ExpiryInSeconds = 120 // Kodun geçerlilik süresi 3 dakika
        };
        _context.VerificationCodes.Add(newVerification);
        _context.SaveChanges();

        // Kullanıcı bilgilerini session'a kaydet
        HttpContext.Session.SetString("User", JsonConvert.SerializeObject(user, new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        }));

        return Json(new { success = true, message = "Giriş başarılı! Doğrulama kodu gönderildi." });
    }


    [HttpPost]
    public IActionResult VerifyLoginCode(string verificationCode)
    {
        // Session'dan kullanıcı bilgisini al
        var userJson = HttpContext.Session.GetString("User");

        // Eğer session'da kullanıcı bilgisi yoksa hata döndür
        if (string.IsNullOrEmpty(userJson))
        {
            return Json(new { success = false, message = "Geçersiz kullanıcı oturumu. Lütfen tekrar giriş yapın." });
        }

        // JSON verisini User nesnesine dönüştür
        var user = JsonConvert.DeserializeObject<User>(userJson);
        if (user == null)
        {
            return Json(new { success = false, message = "Kullanıcı bulunamadı!" });
        }

        // Kullanıcının doğrulama kodunu veritabanında ara
        var verification = _context.VerificationCodes
            .Where(vc => vc.UserId == user.Id && vc.Code == verificationCode)
            .OrderByDescending(vc => vc.SentAt) // En son gönderilen kodu alır
            .FirstOrDefault();

        // Doğrulama kodu yoksa veya süresi dolmuşsa hata döndür
        if (verification == null || verification.IsExpired())
        {
            return Json(new { success = false, message = "Geçersiz veya süresi dolmuş doğrulama kodu." });
        }

        // Kod geçerli ise kullanıcıyı giriş yapmış kabul et
        // Kullanıcının giriş durumunu ve bilgilerini session'da sakla
        HttpContext.Session.SetString("IsAuthenticated", "true");
        HttpContext.Session.SetString("LoggedInUser", JsonConvert.SerializeObject(user));

        // Doğrulama kodunu geçersiz hale getir
        verification.ExpiryInSeconds = 0; // Expire süresini sıfır yap
        _context.VerificationCodes.Update(verification);
        _context.SaveChanges();

        // Giriş işlemi başarılı, ana sayfaya yönlendir
        return Json(new { success = true, message = "Doğrulama başarılı, giriş yapıldı.", redirectUrl = Url.Action("Index", "Home") });
    }

}
