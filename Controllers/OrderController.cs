using Iyzipay.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OrderPayment.Models;
using OrderPayment.Models.request;
using System.Linq;

namespace OrderPayment.Controllers
{
    public class OrderController : Controller
    {
        private readonly OrderPaymentDbContext _context;
        

        public OrderController(OrderPaymentDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult SelectionAddress()
        {
            // Kullanıcı oturum kontrolü
            var user = GetAuthenticatedUser();
            if (user == null)
            {
                return Unauthorized("Kullanıcı oturum açmamış.");
            }

            // Kullanıcıya ait adresleri çek
            var addresses = _context.Address
                .Where(a => a.UserId == user.Id)
                .Select(a => new AddressViewModel
                {
                    Id = a.Id,
                    AddressTitle = a.AddressTitle,
                    StreetAddress = a.StreetAddress,
                    Neighborhood = a.Neighborhood,
                    District = a.District,
                    City = a.City,
                    PhoneNumber = a.PhoneNumber
                })
                .ToList();

            return View(addresses); // Adresleri View'a gönder
        }

        [HttpGet]
        public IActionResult SelectionCard()
        {

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = GetAuthenticatedUser();
            if (user == null)
                return Unauthorized("Kullanıcı oturum açmamış.");


            // Kullanıcının kartlarını veritabanından al
            var cards = _context.UserCards
                .Where(c => c.UserId == user.Id) // Sadece oturum açan kullanıcıya ait kartları getir
                .Select(c => new UserCardViewModel
                {
                    CardNumber = c.CardNumber.Length >= 4
        ? $"**** **** **** {c.CardNumber.Substring(c.CardNumber.Length - 4)}" // Kart numarasının sadece son 4 hanesi
        : "Invalid card number", // Hatalı kart numarası durumu
                    CardHolderName = c.CardHolderName, // Kart sahibi adı
                    ExpiryMonth = c.ExpiryMonth, // Son kullanma ayı
                    ExpiryYear = c.ExpiryYear, // Son kullanma yılı
                    CVV = c.CVV, // CVV bilgisi
                    Id = c.Id,
                })
                .ToList();

            // Kartlar bulunmuyorsa mesaj ayarla
            if (!cards.Any())
            {
                ViewBag.Message = "Kullanıcıya ait kayıtlı kart bulunmamaktadır.";
            }

            // Kartları View ile gönder
            return View(cards);
        }

        [HttpPost]
        public IActionResult SelectionAddress([FromBody] AddressSelectionRequest request)
        {
            // Kullanıcı bilgilerini oturumdan al
            var user = GetAuthenticatedUser();
            if (user == null)
            {
                return Unauthorized("Kullanıcı oturum açmamış.");
            }

            // Adres servisiyle adresi al
            var address = _context.Address
                .FirstOrDefault(a => a.Id == request.AddressId);  // request.AddressId ile adresi alıyoruz
            if (address == null)
            {
                // Eğer adres bulunamazsa hata mesajı göster
                return RedirectToAction("Error", "Home");
            }

            // Kullanıcının seçtiği adresin kendi adresi olup olmadığını kontrol et
            if (address.UserId != user.Id)
            {
                // Adres başka bir kullanıcıya aitse, hata mesajı göster
                return Unauthorized("Bu adres sizin adresiniz değil.");
            }

            // Seçilen adresi oturumda saklama
            HttpContext.Session.SetInt32("SelectedAddressId", address.Id);

            // Başarılı işlemi bildiren JSON mesajı döndürme
            return Json(new { message = "Adres başarıyla seçildi!" });
        }

        [HttpPost]
        public IActionResult SelectionCard([FromBody] SelectionCardRequest request)
        {
            var user = GetAuthenticatedUser();
            if (user == null)
            {
                return Unauthorized("Kullanıcı oturum açmamış.");
            }

            // Seçilen adresi session'dan al
            var selectedAddressId = HttpContext.Session.GetInt32("SelectedAddressId");
            if (selectedAddressId == null)
            {
                return Unauthorized("Seçili bir adres yok.");
            }

            // Adres verisini veritabanından al
            var address = _context.Address
                .FirstOrDefault(a => a.Id == selectedAddressId.Value);

            if (address == null || address.UserId != user.Id)
            {
                return Unauthorized("Geçersiz adres.");
            }

            // Kartı al
            var card = _context.UserCards
                .FirstOrDefault(c => c.Id == request.CardId);

            if (card == null)
            {
                // Kart bulunamadı veya kullanıcı bu karta erişim iznine sahip değil
                return NotFound("Kart bulunamadı veya bu karta erişim izniniz yok.");
            }

            if (card.UserId != user.Id)
            {
                // Kart başka bir kullanıcıya aitse, hata mesajı göster
                return Unauthorized("Bu kart sizin kartınız değil.");
            }

            // Seçilen kartı session'da sakla
            HttpContext.Session.SetInt32("SelectedCardId", request.CardId);

            // Başarıyla yönlendirme
            return RedirectToAction("Confirm", "Order");
        }



        [HttpGet]
        public IActionResult Confirm()
        {
            // Kullanıcı bilgilerini oturumdan al
            var user = GetAuthenticatedUser();
            if (user == null)
            {
                return Unauthorized("Kullanıcı oturum açmamış.");
            }

            // Seçili adresi session'dan al
            var selectedAddressId = HttpContext.Session.GetInt32("SelectedAddressId");
            if (selectedAddressId == null)
            {
                return Unauthorized("Seçili bir adres yok.");
            }

            // Seçilen adresi veritabanından al
            var address = _context.Address
                .FirstOrDefault(a => a.Id == selectedAddressId.Value && a.UserId == user.Id);
            if (address == null)
            {
                return Unauthorized("Geçersiz adres.");
            }

            // Seçili kartı session'dan al
            var selectedCardId = HttpContext.Session.GetInt32("SelectedCardId");
            if (selectedCardId == null)
            {
                return Unauthorized("Seçili bir kart yok.");
            }

            // Seçilen kartı veritabanından al
            var card = _context.UserCards
                .FirstOrDefault(c => c.Id == selectedCardId.Value && c.UserId == user.Id);
            if (card == null)
            {
                return Unauthorized("Geçersiz kart.");
            }

            // Sipariş oluşturma (basit bir modelleme, isteğe göre genişletilebilir)

            var cart = _context.Carts
                   .Include(c => c.CartItems)
                   .ThenInclude(ci => ci.Product)
                   .FirstOrDefault(c => c.UserId == user.Id);


            var viewModel = new ConfirmOrderViewModel
            {
                Address = address,
                Card = card,
                Cart = cart
            };

            return View(viewModel);
        }





        [HttpGet]
        public IActionResult UserGetOrders()
        {
            // Kullanıcının oturum açıp açmadığını kontrol et
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            if (isAuthenticated != "true")
            {
                return RedirectToAction("Login", "Auth");
            }

            // Kullanıcı bilgilerini oturumdan al
            var userJson = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(userJson))
            {
                // Kullanıcı bilgisi oturumda bulunmazsa login sayfasına yönlendir
                return RedirectToAction("Login", "Auth");
            }

            var user = JsonConvert.DeserializeObject<User>(userJson);

            if (user == null)
            {
                // Oturumdaki kullanıcı verisi geçersizse login sayfasına yönlendir
                return RedirectToAction("Login", "Auth");
            }

            // Kullanıcıyı doğrulama
            var dbUser = _context.Users.FirstOrDefault(u => u.Id == user.Id);
            if (dbUser == null)
            {
                // Kullanıcı veritabanında bulunamazsa oturumu sonlandır
                return RedirectToAction("Login", "Auth");
            }

            // Kullanıcının siparişlerini OrderDate'e göre sıralayıp al
            var orders = _context.Orders
                .Where(o => o.UserId == user.Id)  // Kullanıcıya ait siparişler
                .OrderByDescending(o => o.OrderDate)  // Tarihe göre azalan sırala
                .ToList();

            // Sipariş yoksa kullanıcıya bilgilendirme mesajı göster
            if (!orders.Any())
            {
                TempData["InfoMessage"] = "Henüz bir siparişiniz bulunmamaktadır.";
            }

            // Eğer veritabanında bir hata oluşursa
            try
            {
                // Siparişleri başarıyla al
                return View(orders);
            }
            catch (Exception ex)
            {
                // Hata mesajını logla ve kullanıcıyı bilgilendir
                TempData["ErrorMessage"] = "Bir hata oluştu, lütfen tekrar deneyin.";
                return RedirectToAction("Index", "Home");
            }
        }




        [HttpPost]
        public IActionResult CancelOrder([FromBody] int orderId)
        {
            // Kullanıcının giriş yapıp yapmadığını kontrol et
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            if (isAuthenticated != "true")
            {
                return Json(new { success = false, message = "Lütfen giriş yapın." });
            }

            // Kullanıcı bilgilerini oturumdan al
            var userJson = HttpContext.Session.GetString("User");
            var user = JsonConvert.DeserializeObject<User>(userJson);

            if (user == null)
            {
                return Json(new { success = false, message = "Kullanıcı bilgileri bulunamadı. Lütfen giriş yapın." });
            }

            // Siparişi veritabanından al ve sipariş öğelerini dahil et
            var order = _context.Orders
                                .Include(o => o.OrderItems)
                                .FirstOrDefault(o => o.OrderId == orderId);

            // Sipariş bulunamadıysa veya sipariş durumu iptal edilmiş ya da teslim edilmişse
            if (order == null)
            {
                return Json(new { success = false, message = "Sipariş bulunamadı." });
            }

            // Siparişin kullanıcıya ait olup olmadığını kontrol et
            if (order.UserId != user.Id)
            {
                return Json(new { success = false, message = "Bu sipariş size ait değil." });
            }

            // Siparişin durumu uygun olmalı (Teslim Edildi veya İptal Edildi olmamalı)
            if (order.Status == OrderStatus.TeslimEdildi || order.Status == OrderStatus.IptalEdildi)
            {
                return Json(new { success = false, message = "Bu sipariş iptal edilemez." });
            }

            // Siparişin durumu onaylanmış veya beklemede olmalı (bu iki durum iptal edilebilir)
            if (order.Status != OrderStatus.Beklemede && order.Status != OrderStatus.Onaylandi)
            {
                return Json(new { success = false, message = "Sadece bekleyen veya onaylanmış siparişler iptal edilebilir." });
            }

            // Siparişi iptal et
            order.Status = OrderStatus.IptalEdildi;
            _context.SaveChanges();

            // Başarılı işlem sonucu
            return Json(new { success = true, message = "Sipariş başarıyla iptal edilmiştir." });
        }





        [HttpGet]
        public IActionResult OrderDetails(int id)
        {
            // Kullanıcının giriş yapıp yapmadığını kontrol et
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            if (isAuthenticated != "true")
            {
                // Giriş yapılmamışsa login sayfasına yönlendir
                return RedirectToAction("Login", "Auth");
            }

            // Kullanıcı bilgilerini oturumdan al
            var userJson = HttpContext.Session.GetString("User");
            var user = JsonConvert.DeserializeObject<User>(userJson);

            if (user == null)
            {
                // Kullanıcı bilgisi yoksa login sayfasına yönlendir
                TempData["ErrorMessage"] = "Kullanıcı bilgileri geçersiz. Lütfen tekrar giriş yapın.";
                return RedirectToAction("Login", "Auth");
            }

            // Siparişi veritabanından al ve sipariş öğelerini dahil et
            var order = _context.Orders
                .Include(o => o.OrderItems) // Sipariş öğelerini dahil et
                .FirstOrDefault(o => o.OrderId == id && o.UserId == user.Id); // Sadece bu kullanıcıya ait siparişi al

            // Sipariş bulunamadıysa veya kullanıcıya ait değilse hata mesajı göster
            if (order == null)
            {
                TempData["ErrorMessage"] = "Geçersiz sipariş. Bu sipariş size ait değil veya bulunamadı.";
                return RedirectToAction("UserGetOrders", "User");
            }


            // Eğer tüm kontroller geçildiyse, sipariş detayları sayfasını göster
            return View(order);
        }

        /*
        [HttpPost]
        public IActionResult CreateOrder()
        {
            var userJson = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(userJson))
                return RedirectToAction("Login", "Auth");

            var user = JsonConvert.DeserializeObject<User>(userJson);
            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserId == user.Id);

            if (cart == null || !cart.CartItems.Any())
                return BadRequest("Sepet boş.");

            // Transaction başlatıyoruz.
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                // Ürünlerin stok durumu kontrolü
                foreach (var item in cart.CartItems)
                {
                    if (item.Product.Quantity < item.Quantity)
                        return BadRequest($"'{item.Product.Name}' ürünü stokta yeterli değil.");
                }

                // Siparişi oluşturuyoruz
                var order = new Order
                {
                    UserId = user.Id,
                    TotalAmount = cart.TotalAmount,
                    Status = OrderStatus.Beklemede,
                    OrderItems = cart.CartItems.Select(ci => new OrderItem
                    {
                        ProductId = ci.ProductId,
                        ProductName = ci.Product.Name,
                        UnitPrice = ci.Price,
                        Quantity = (int)ci.Quantity
                    }).ToList()
                };

                // Ürünlerin stoklarını güncelle
                foreach (var item in cart.CartItems)
                {
                    var product = item.Product;
                    product.Quantity -= (int)item.Quantity;
                }

                // Siparişi ve ürünleri kaydet
                _context.Orders.Add(order);
                _context.CartItems.RemoveRange(cart.CartItems);
                cart.TotalAmount = 0;

                // Veritabanı işlemlerini kaydediyoruz
                _context.SaveChanges();

                // Transaction'ı commit ediyoruz
                transaction.Commit();

                return RedirectToAction("UserGetOrders");
            }
            catch (Exception ex)
            {
                // Hata durumunda tüm işlemleri geri alıyoruz
                transaction.Rollback();
                return BadRequest($"Sipariş oluşturulamadı: {ex.Message}");
            }
        }

        */


        private User? GetAuthenticatedUser()
        {
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            if (isAuthenticated != "true")
            {
                // Kullanıcı doğrulama başarısızsa login sayfasına yönlendir
                HttpContext.Response.Redirect("/Auth/Login");
                return null;
            }

            var userJson = HttpContext.Session.GetString("User");
            return string.IsNullOrEmpty(userJson) ? null : JsonConvert.DeserializeObject<User>(userJson);
        }
    }
}
