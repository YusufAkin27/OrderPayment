using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OrderPayment.Models;

using ForgotPasswordRequest = OrderPayment.Models.request.ForgotPasswordRequest;
using ResetPasswordRequest = OrderPayment.Models.request.ResetPasswordRequest;
using RegisterRequest = OrderPayment.Models.request.RegisterRequest;


namespace OrderPayment.Controllers
{
    public class AuthController : Controller
    {

        private readonly SmsService _smsService;
        private readonly OrderPaymentDbContext _context;

        public AuthController(SmsService smsService, OrderPaymentDbContext context)
        {
            _smsService = smsService;
            _context = context;
        }


        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpGet]
        public IActionResult VerifyForgotPasswordCode()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword()
        {
            return View();
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
        public IActionResult Register([FromBody] RegisterRequest registerRequest)
        {
            // Model doğrulama
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return Json(new { success = false, message = "Geçersiz giriş.", errors });
            }

            // Telefon numarasına göre kullanıcı kontrolü
            var existingUser = _context.Users.FirstOrDefault(u => u.PhoneNumber == registerRequest.PhoneNumber);
            if (existingUser != null)
            {
                return Json(new { success = false, message = "Bu telefon numarasıyla zaten kayıtlı bir kullanıcı var." });
            }

            // Şifreyi hash'le
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password);

            // Kullanıcı oluştur
            var user = new User
            {
                FirstName = registerRequest.FirstName,
                LastName = registerRequest.LastName,
                PhoneNumber = registerRequest.PhoneNumber,
                PasswordHash = hashedPassword,
                Password=registerRequest.Password,
                IsActive = false, // Kullanıcı aktif değil
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.SaveChanges();

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

            // Doğrulama kodunu kaydet
            var verification = new VerificationCode
            {
                Code = verificationCode,
                SentAt = DateTime.UtcNow,
                ExpiryInSeconds = 180, // 3 dakika geçerlilik süresi
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
                ExpiryInSeconds = 180 // Kodun geçerlilik süresi 3 dakika
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

        private (bool isValid, string message, VerificationCode? verification) ValidateVerificationCode(int userId, string verificationCode)
        {
            // Kullanıcının doğrulama kodunu veritabanında ara
            var verification = _context.VerificationCodes
                .Where(vc => vc.UserId == userId && vc.Code == verificationCode)
                .OrderByDescending(vc => vc.SentAt) // En son gönderilen kodu alır
                .FirstOrDefault();

            if (verification == null)
            {
                return (false, "Geçersiz doğrulama kodu.", null);
            }

            if (verification.IsExpired())
            {
                return (false, "Süresi dolmuş doğrulama kodu.", null);
            }

            return (true, "Doğrulama başarılı.", verification);
        }

        [HttpPost]
        public IActionResult VerifyForgotPasswordCode(string verificationCode)
        {
            var userJson = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(userJson))
            {
                return Json(new { success = false, message = "Geçersiz kullanıcı oturumu. Lütfen tekrar şifre sıfırlama işlemini başlatın." });
            }

            var user = JsonConvert.DeserializeObject<User>(userJson);
            if (user == null)
            {
                return Json(new { success = false, message = "Kullanıcı bilgisi bulunamadı!" });
            }

            var (isValid, message, verification) = ValidateVerificationCode(user.Id, verificationCode);

            if (!isValid)
            {
                return Json(new { success = false, message });
            }

            // Kod geçerli, doğrulama kodunu geçersiz yap
            verification!.ExpiryInSeconds = 5;
            _context.VerificationCodes.Update(verification);
            _context.SaveChanges();

            HttpContext.Session.SetString("PasswordResetRequested", "true");
            return Json(new { success = true, message = "Doğrulama başarılı! Şifre sıfırlama sayfasına yönlendiriliyorsunuz.", redirectUrl = Url.Action("ResetPassword", "Auth") });
        }


        [HttpPost]
        public IActionResult VerifyCode(string verificationCode)
        {
            // Kullanıcıyı session'dan al
            var userJson = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(userJson))
            {
                return Json(new { success = false, message = "Geçersiz kullanıcı verisi!" });
            }

            // Kullanıcıyı deserialization ile al
            var user = JsonConvert.DeserializeObject<User>(userJson);
            if (user == null)
            {
                return Json(new { success = false, message = "Kullanıcı bulunamadı!" });
            }

            // Kullanıcıyı veritabanından al
            user = _context.Users.FirstOrDefault(u => u.Id == user.Id);
            if (user == null)
            {
                return Json(new { success = false, message = "Kullanıcı veritabanında bulunamadı!" });
            }

            // Doğrulama kodunu kontrol et
            var (isValid, validationMessage, verification) = ValidateVerificationCode(user.Id, verificationCode);

            if (!isValid)
            {
                return Json(new { success = false, message = validationMessage });
            }

            // Kullanıcıyı aktif yap
            user.IsActive = true;
            _context.Users.Update(user);

            // Doğrulama kodunu geçersiz yap
            if (verification != null)
            {
                verification.ExpiryInSeconds = 5; // Kodun süresini hemen dolmuş yap
                _context.VerificationCodes.Update(verification);
            }

            try
            {
                _context.SaveChanges();

                // Güncellenmiş kullanıcıyı session'a kaydet
                HttpContext.Session.SetString("User", JsonConvert.SerializeObject(user, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                }));

                return Json(new { success = true, message = "Kullanıcı başarıyla kaydedildi ve aktif edildi." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Bir hata oluştu. Lütfen tekrar deneyin.", error = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult VerifyLoginCode(string verificationCode)
        {
            var userJson = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(userJson))
            {
                return Json(new { success = false, message = "Geçersiz kullanıcı oturumu. Lütfen tekrar giriş yapın." });
            }

            var user = JsonConvert.DeserializeObject<User>(userJson);
            if (user == null)
            {
                return Json(new { success = false, message = "Kullanıcı bulunamadı!" });
            }

            var (isValid, message, verification) = ValidateVerificationCode(user.Id, verificationCode);

            if (!isValid)
            {
                return Json(new { success = false, message });
            }

            // Kod geçerli, giriş işlemini tamamla
            HttpContext.Session.SetString("IsAuthenticated", "true");
            HttpContext.Session.SetString("User", JsonConvert.SerializeObject(user));

            verification!.ExpiryInSeconds = 10;
            _context.VerificationCodes.Update(verification);
            _context.SaveChanges();

            return Json(new { success = true, message = "Doğrulama başarılı, giriş yapıldı.", redirectUrl = Url.Action("Home", "HomePage") });
        }


        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            HttpContext.SignOutAsync();
            return RedirectToAction("Login", "Auth");
        }



        [HttpPost]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordRequest forgotPasswordRequest)
        {
            // Model doğrulaması
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Lütfen geçerli bir telefon numarası giriniz." });
            }

            // Kullanıcıyı telefon numarasına göre ara
            var user = _context.Users.FirstOrDefault(u => u.PhoneNumber == forgotPasswordRequest.PhoneNumber);

            // Kullanıcı bulunamazsa hata mesajı döndür
            if (user == null)
            {
                return Json(new { success = false, message = "Bu telefon numarasına kayıtlı bir kullanıcı bulunamadı." });
            }

            // Kullanıcı aktif değilse hata döndür
            if (!user.IsActive)
            {
                return Json(new { success = false, message = "Hesabınız doğrulanmamış. Şifrenizi sıfırlayabilmek için önce hesabınızı doğrulayın." });
            }

            // Doğrulama kodu oluştur
            var verificationCode = GenerateVerificationCode();

            // SMS mesajı
            var message = $"Sayın {user.FirstName} {user.LastName},\n\n" +
                          "Şifrenizi sıfırlayabilmeniz için doğrulama kodunuz aşağıda belirtilmiştir:\n\n" +
                          $"Doğrulama Kodu: {verificationCode}\n\n" +
                          "Bu kod 3 dakika boyunca geçerlidir. Lütfen bu süre içinde şifre sıfırlama işlemini tamamlayınız.\n\n" +
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
                ExpiryInSeconds = 180 // Kodun geçerlilik süresi 2 dakika
            };
            _context.VerificationCodes.Add(newVerification);
            _context.SaveChanges();


            HttpContext.Session.SetString("PasswordResetRequested", "false");

            // Kullanıcı bilgilerini HTTP oturumuna kaydet
            HttpContext.Session.SetString("User", JsonConvert.SerializeObject(user, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            }));

            return Json(new { success = true, message = "Doğrulama kodu başarıyla gönderildi." });
        }






        [HttpPost]
        public IActionResult ResetPassword([FromBody] ResetPasswordRequest resetPasswordRequest)
        {
            // 1. Session'dan şifre sıfırlama talebini kontrol et
            var resetRequested = HttpContext.Session.GetString("PasswordResetRequested");

            if (string.IsNullOrEmpty(resetRequested) || resetRequested != "true")
            {
                return Json(new { success = false, message = "Şifre sıfırlama isteği bulunamadı. Lütfen işlemi yeniden başlatın." });
            }

            // 2. Model doğrulaması
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(resetPasswordRequest.Password))
            {
                return Json(new { success = false, message = "Geçerli bir şifre giriniz." });
            }

            // 3. Session'dan kullanıcı bilgilerini al
            var userJson = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(userJson))
            {
                return Json(new { success = false, message = "Kullanıcı bilgisi bulunamadı. Lütfen işlemi yeniden başlatın." });
            }

            var user = JsonConvert.DeserializeObject<User>(userJson);
            if (user == null)
            {
                return Json(new { success = false, message = "Kullanıcı bilgisi bulunamadı." });
            }

            // 4. Veritabanındaki kullanıcı kaydını getir
            var dbUser = _context.Users.FirstOrDefault(u => u.Id == user.Id);
            if (dbUser == null)
            {
                return Json(new { success = false, message = "Kullanıcı bilgisi doğrulanamadı." });
            }

            // 5. Şifreyi hashleyerek kaydet
            dbUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resetPasswordRequest.Password);

            // 6. Veritabanını güncelle
            _context.Users.Update(dbUser);
            _context.SaveChanges();

            // 7. Session temizliği
            HttpContext.Session.Remove("User");
            HttpContext.Session.Remove("PasswordResetRequested");

            return Json(new { success = true, message = "Şifreniz başarıyla sıfırlandı! Yeni şifrenizle giriş yapabilirsiniz.", redirectUrl = Url.Action("Login", "Auth") });
        }


    }
}
