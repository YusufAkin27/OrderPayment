using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderPayment.Models;
using System.Diagnostics;

namespace OrderPayment.Controllers
{
    public class HomeController : Controller
    {
        private readonly OrderPaymentDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(OrderPaymentDbContext context)
        {
            _context = context;
        }
      
       

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [HttpPost]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            try
            {
                // Kullanýcý oturumunu kontrol et
                var userJson = HttpContext.Session.GetString("LoggedInUser");

                if (string.IsNullOrEmpty(userJson))
                {
                    return Json(new { success = false, message = "Lütfen giriþ yapýn." });
                }

                var user = JsonConvert.DeserializeObject<User>(userJson);
                if (user == null)
                {
                    return Json(new { success = false, message = "Geçersiz oturum. Tekrar giriþ yapýn." });
                }

                // Kullanýcýnýn sepetini al veya oluþtur
                var cart = _context.Carts
                    .Include(c => c.CartItems)
                    .FirstOrDefault(c => c.UserId == user.Id);

                if (cart == null)
                {
                    cart = new Cart
                    {
                        UserId = user.Id,
                        CreatedAt = DateTime.UtcNow,
                        CartItems = new List<CartItem>()
                    };
                    _context.Carts.Add(cart);
                    _context.SaveChanges();
                }

                // Ürün var mý kontrol et
                var product = _context.products.FirstOrDefault(p => p.Id == productId);
                if (product == null)
                {
                    return Json(new { success = false, message = "Ürün bulunamadý." });
                }

                // Sepette ürün var mý kontrol et
                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

                if (cartItem != null)
                {
                    // Ürün sepette mevcutsa miktarý artýr
                    cartItem.Quantity += quantity;
                }
                else
                {
                    // Yeni ürün ekle
                    cartItem = new CartItem
                    {
                        CartId = cart.Id,
                        ProductId = productId,
                        Quantity = quantity,
                        Price = product.Price
                    };
                    cart.CartItems.Add(cartItem);
                }

                // Deðiþiklikleri kaydet
                _context.SaveChanges();

                return Json(new { success = true, message = "Ürün sepete eklendi!", cart });
            }
            catch (Exception ex)
            {
                // Genel hata iþleme
                return Json(new { success = false, message = "Bir hata oluþtu: " + ex.Message });
            }
        }

    }
}
