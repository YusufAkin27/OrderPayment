using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using OrderPayment.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Globalization;

namespace OrderPayment.Controllers
{
    public class CartController : Controller
    {
        private readonly OrderPaymentDbContext _context;

        public CartController(OrderPaymentDbContext context)
        {
            _context = context;
        }





        // Sepeti göster (GET isteği)
        [HttpGet]
        public IActionResult Cart()
        {
            // Session validation
            var userJson = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(userJson))
            {
                return RedirectToAction("Login", "Auth");
            }

            var user = JsonConvert.DeserializeObject<User>(userJson);
            if (user == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            // Fetch user's cart with products
            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserId == user.Id);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Carts.Add(cart);
                _context.SaveChanges();
            }

            return View(cart);
        }



        [HttpPost]
        public IActionResult UpdateCart(int productId, string action)
        {
            // Kullanıcı doğrulaması
            var userJson = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(userJson))
            {
                return Json(new { success = false, message = "Kullanıcı oturumu geçersiz. Lütfen tekrar giriş yapın." });
            }

            var user = JsonConvert.DeserializeObject<User>(userJson);
            if (user == null)
            {
                return Json(new { success = false, message = "Kullanıcı bulunamadı." });
            }

            // Sepet kontrolü
            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserId == user.Id);

            if (cart == null)
            {
                return Json(new { success = false, message = "Sepet bulunamadı." });
            }

            // Sepetteki ürün kontrolü
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem == null)
            {
                return Json(new { success = false, message = "Sepette bu ürün bulunamadı." });
            }

            // Stok kontrolü ve miktar güncellemeleri
            if (action == "increment")
            {
                // Stok kontrolü
                if (cartItem.Quantity + 1 > cartItem.Product.Quantity)
                {
                    return Json(new { success = false, message = "Bu ürün için yeterli stok bulunmamaktadır." });
                }
                cartItem.Quantity++;
            }
            else if (action == "decrement")
            {
                // Miktar 1'den az olamaz, 1'e kadar indirilebilir
                if (cartItem.Quantity > 1)
                {
                    cartItem.Quantity--;
                }
                else
                {
                    cart.CartItems.Remove(cartItem);  // Miktar 0 olursa ürünü sepetten kaldır
                }
            }
            else
            {
                return Json(new { success = false, message = "Geçersiz işlem." });
            }

            // Sepetin toplam tutarını güncelle
            cart.TotalAmount = cart.CartItems.Sum(ci => ci.Quantity * ci.Product.Price);

            // Veritabanına kaydet
            _context.SaveChanges();

            // Yeni sepet durumu döndür
            return Json(new
            {
                success = true,
                quantity = cartItem.Quantity,
                totalAmount = cart.TotalAmount.ToString("C", new CultureInfo("tr-TR")) // Toplam tutarı TL formatında döndür
            });
        }

        [HttpPost]
        public IActionResult RemoveItem(int productId)
        {
            // Kullanıcı doğrulaması
            var userJson = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(userJson))
            {
                return Json(new { success = false, message = "Kullanıcı oturumu geçersiz. Lütfen tekrar giriş yapın." });
            }

            var user = JsonConvert.DeserializeObject<User>(userJson);
            if (user == null)
            {
                return Json(new { success = false, message = "Kullanıcı bulunamadı." });
            }

            // Sepet kontrolü
            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserId == user.Id);

            if (cart == null)
            {
                return Json(new { success = false, message = "Sepet bulunamadı." });
            }

            // Sepetteki ürün kontrolü
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem == null)
            {
                return Json(new { success = false, message = "Sepette bu ürün bulunamadı." });
            }

            // Ürünü sepetten kaldır
            cart.CartItems.Remove(cartItem);

            // Sepetin toplam tutarını güncelle
            cart.TotalAmount = cart.CartItems.Sum(ci => ci.Quantity * ci.Product.Price);

            // Veritabanına kaydet
            _context.SaveChanges();

            // Yeni sepet durumu döndür
            return Json(new
            {
                success = true,
                message = "Ürün başarıyla sepetten kaldırıldı.",
                totalAmount = cart.TotalAmount.ToString("C", new CultureInfo("tr-TR")) // TotalAmount'u formatlı şekilde döndür
            });
        }


    }
}
