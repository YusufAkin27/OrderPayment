using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using OrderPayment.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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
            // Session'dan kullanıcı bilgilerini al
            var userJson = HttpContext.Session.GetString("LoggedInUser");

            // Eğer session'da kullanıcı bilgisi yoksa kullanıcıyı login sayfasına yönlendir
            if (string.IsNullOrEmpty(userJson))
            {
                return RedirectToAction("Login", "User");
            }

            // JSON verisini User nesnesine dönüştür
            var user = JsonConvert.DeserializeObject<User>(userJson);

            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }

            // Kullanıcının sepetini al (ve sepet içindeki ürün bilgilerini dahil et)
            var cart = _context.Carts
                .Include(c => c.CartItems) // CartItem'ları dahil et
                .ThenInclude(ci => ci.Product) // Product bilgilerini dahil et
                .FirstOrDefault(c => c.UserId == user.Id);

            if (cart == null)
            {
                // Eğer sepet yoksa, yeni bir sepet oluştur
                cart = new Cart
                {
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                };
                _context.Carts.Add(cart);
                _context.SaveChanges();
            }

          

            // Güncellenmiş sepeti (ürünler eklenmiş haliyle) tekrar al
            cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault(c => c.UserId == user.Id);

            // Sepet ve ürün bilgilerini View'a gönder
            return View(cart);
        }

      


        // Sepetteki ürünü ekleme veya güncelleme işlemi (POST isteği)
        [HttpPost]
        public IActionResult UpdateCart(int productId, int quantity)
        {
            // Session'dan kullanıcı bilgilerini al
            var userJson = HttpContext.Session.GetString("LoggedInUser");

            if (string.IsNullOrEmpty(userJson))
            {
                return RedirectToAction("Login", "User"); // Login sayfasına yönlendir
            }

            var user = JsonConvert.DeserializeObject<User>(userJson);
            if (user == null)
            {
                return RedirectToAction("Login", "User");
            }

            // Kullanıcının sepetini al
            var cart = _context.Carts
                .Where(c => c.UserId == user.Id)
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .FirstOrDefault();

            if (cart == null)
            {
                // Sepet bulunamazsa, yeni bir sepet oluştur
                cart = new Cart { UserId = user.Id };
                _context.Carts.Add(cart);
            }

            // Sepet item'ı (ürün) ekle veya güncelle
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem != null)
            {
                // Sepette mevcutsa, miktarı güncelle
                cartItem.Quantity = quantity;

                // Eğer miktar 0 ise, ürünü sepetten sil
                if (cartItem.Quantity <= 0)
                {
                    cart.CartItems.Remove(cartItem);
                }
            }
            else if (quantity > 0)
            {
                // Sepette değilse, yeni bir CartItem oluştur
                var product = _context.products.FirstOrDefault(p => p.Id == productId);
                if (product != null)
                {
                    cartItem = new CartItem
                    {
                        ProductId = productId,
                        Quantity = quantity,
                        Price = product.Price // Ürün fiyatı
                    };
                    cart.CartItems.Add(cartItem);
                }
            }

            _context.SaveChanges();

            // Sepeti tekrar göster
            return RedirectToAction("Cart");
        }
    }
}
