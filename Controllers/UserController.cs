using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OrderPayment.Models;
using OrderPayment.Models.request;
using OrderPayment.Models.Request;

namespace OrderPayment.Controllers
{
    public class UserController : Controller
    {

        private readonly OrderPaymentDbContext _context;

        public UserController(OrderPaymentDbContext context)
        {
            _context = context;
        }

     

        [HttpGet]
        public IActionResult UserProfile()
        {
            // Kullanıcı oturum durumunu kontrol et
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            if (isAuthenticated != "true")
            {
                // Giriş yapmamışsa yönlendir
                return RedirectToAction("Login", "Auth");
            }

            // Oturumdan kullanıcı bilgilerini al
            var userJson = HttpContext.Session.GetString("User");
            var user = JsonConvert.DeserializeObject<User>(userJson);

            if (user == null)
            {
                return RedirectToAction("Login", "Auth"); // Kullanıcı bilgisi bulunamazsa giriş sayfasına yönlendir
            }

            // Kullanıcının veritabanındaki bilgilerini al
            var dbUser = _context.Users.FirstOrDefault(u => u.Id == user.Id);
            if (dbUser == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // ViewModel oluştur
            var userProfile = new UserProfileViewModel
            {
                FirstName = dbUser.FirstName,
                LastName = dbUser.LastName,
                PhoneNumber = dbUser.PhoneNumber,
                Password = dbUser.Password,
                CreatedAt = dbUser.CreatedAt.ToString("yyyy-MM-dd"),
                IsActive = dbUser.IsActive,
                Id = dbUser.Id
            };

            // View ile kullanıcı bilgisini döndür
            return View(userProfile);
        }

        [HttpPatch]
        [Route("User/EditProfile")]
        public IActionResult EditProfile([FromBody] UpdateUserRequest updateUserRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verilen ID ile kullanıcıyı veritabanından bul
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            if (isAuthenticated != "true")
            {
                // Giriş yapmamışsa yönlendir
                return RedirectToAction("Login", "Auth");
            }

            // Oturumdan kullanıcı bilgilerini al
            var userJson = HttpContext.Session.GetString("User");
            var user = JsonConvert.DeserializeObject<User>(userJson);

            if (user == null)
            {
                return RedirectToAction("Login", "Auth"); // Kullanıcı bilgisi bulunamazsa giriş sayfasına yönlendir
            }

            // Kullanıcının veritabanındaki bilgilerini al
            var dbUser = _context.Users.FirstOrDefault(u => u.Id == user.Id);
            if (dbUser == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Telefon numarasının benzersiz olduğunu kontrol et (sadece telefon numarasını değiştirenler için)
            if (!string.IsNullOrEmpty(updateUserRequest.PhoneNumber) && updateUserRequest.PhoneNumber != user.PhoneNumber)
            {
                var existingUserWithPhone = _context.Users
                                                    .FirstOrDefault(u => u.PhoneNumber == updateUserRequest.PhoneNumber && u.Id != updateUserRequest.Id);
                if (existingUserWithPhone != null)
                {
                    return BadRequest(new { error = "Bu telefon numarası zaten başka bir kullanıcıya ait." });
                }

                dbUser.PhoneNumber = updateUserRequest.PhoneNumber;
            }

            // Şifre geçerliliğini kontrol et (şifreyi değiştirenler için)
            if (!string.IsNullOrEmpty(updateUserRequest.Password) && updateUserRequest.Password.Length < 6)
            {
                return BadRequest(new { error = "Şifre en az 6 karakter olmalıdır." });
            }

            // Kullanıcı bilgilerini sadece güncellenmiş alanlarla değiştir
            if (!string.IsNullOrEmpty(updateUserRequest.FirstName) && updateUserRequest.FirstName != dbUser.FirstName)
            {
                dbUser.FirstName = updateUserRequest.FirstName;
            }

            if (!string.IsNullOrEmpty(updateUserRequest.LastName) && updateUserRequest.LastName != dbUser.LastName)
            {
                dbUser.LastName = updateUserRequest.LastName;
            }

            if (!string.IsNullOrEmpty(updateUserRequest.Password) && updateUserRequest.Password != dbUser.Password)
            {
                if (updateUserRequest.Password.Length < 6)
                {
                    return BadRequest(new { error = "Şifre en az 6 karakter olmalıdır." });
                }

                dbUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(updateUserRequest.Password);
                dbUser.Password = updateUserRequest.Password;
            }


            // Veritabanında değişiklikleri kaydet
            _context.SaveChanges();

            // Güncellenmiş kullanıcı bilgilerini session'a kaydedelim (isteğe bağlı)
            HttpContext.Session.SetString("User", JsonConvert.SerializeObject(dbUser));

            return Ok(new { message = "Kullanıcı başarıyla güncellendi." });
        }

        [HttpGet]
        public IActionResult DeleteUser()
        {
            // Kullanıcının oturum açıp açmadığını kontrol et
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            if (isAuthenticated != "true")
            {
                // Giriş yapmamışsa, giriş sayfasına yönlendir
                return RedirectToAction("Login", "Auth");
            }

            // Oturumdan kullanıcı bilgilerini al
            var userJson = HttpContext.Session.GetString("User");
            var user = JsonConvert.DeserializeObject<User>(userJson);

            // Kullanıcı bilgisi yoksa, giriş sayfasına yönlendir
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Kullanıcının veritabanındaki bilgilerini al
            var dbUser = _context.Users.FirstOrDefault(u => u.Id == user.Id);
            if (dbUser == null)
            {
                // Kullanıcı veritabanında bulunamazsa, giriş sayfasına yönlendir
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                // Kullanıcıyı veritabanından sil
                _context.Users.Remove(dbUser);
                _context.SaveChanges();

                // Silme işlemi başarılıysa, kullanıcıyı giriş sayfasına yönlendir
                return RedirectToAction("Login", "Auth");
            }
            catch (Exception ex)
            {
                // Hata oluşursa, hata mesajını göster
                TempData["ErrorMessage"] = "Bir hata oluştu: " + ex.Message;
                return RedirectToAction("Profile", "User"); // Hata durumunda kullanıcıyı profil sayfasına yönlendirebilirsiniz.
            }
        }






    }
}
