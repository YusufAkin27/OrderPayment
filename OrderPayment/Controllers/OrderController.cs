using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using OrderPayment.Models;
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
        public IActionResult Confirm()
        {
            return View();
        }

        // Confirm action method to finalize the user's cart into an order
        [HttpPost]
        public IActionResult ConfirmPayment()
        {
            // Kullanıcının oturumda olup olmadığını kontrol et
            var isAuthenticated = HttpContext.Session.GetString("IsAuthenticated");
            var loggedInUserJson = HttpContext.Session.GetString("LoggedInUser");

            if (isAuthenticated != "true" || string.IsNullOrEmpty(loggedInUserJson))
            {
                return Unauthorized("Kullanıcı oturum açmamış.");
            }

            // Oturumdaki kullanıcıyı al
            var loggedInUser = JsonConvert.DeserializeObject<User>(loggedInUserJson);
            if (loggedInUser == null || !loggedInUser.IsActive)
            {
                return Unauthorized("Kullanıcı oturum bilgileri geçersiz.");
            }

            // Kullanıcıyı ve onun sepetini doğrula
            var user = _context.Users.FirstOrDefault(u => u.Id == loggedInUser.Id && u.IsActive);
            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı veya aktif değil.");
            }

            var userCart = _context.Carts
                .Include(c => c.CartItems) // Sepet öğelerini dahil et
                .ThenInclude(ci => ci.Product) // Ürün bilgilerini dahil et
                .FirstOrDefault(c => c.UserId == user.Id);

            if (userCart == null || !userCart.CartItems.Any())
            {
                return BadRequest("Sepet boş veya geçerli bir sepet bulunamadı.");
            }

            // Sepet toplam tutarını güncelle
            userCart.UpdateTotalAmount();

            var paymentSuccessful = true;
                if (!paymentSuccessful)
            {
                return BadRequest("Ödeme işlemi başarısız.");
            }

            // Siparişi oluştur
            var newOrder = new Order
            {
                UserId = user.Id,
                TotalAmount = userCart.TotalAmount,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Beklemede, // Varsayılan durum: Beklemede
                OrderItems = userCart.CartItems.Select(ci => new OrderItem
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name, // Ürün adı
                    Quantity = (int)ci.Quantity,  // Sepetteki miktar
                    UnitPrice = ci.Price          // Birim fiyat
                }).ToList()
            };

            // Siparişi veritabanına kaydet
            _context.Orders.Add(newOrder);

            // Sepeti temizle
            userCart.CartItems.Clear();
            userCart.TotalAmount = 0;

            // Değişiklikleri kaydet
            _context.SaveChanges();

            return Ok("Sipariş başarıyla onaylandı!");
        }
    }



    }
