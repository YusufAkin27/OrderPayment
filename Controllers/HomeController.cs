using Microsoft.AspNetCore.Mvc;
using OrderPayment.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace OrderPayment.Controllers
{
    public class HomeController : Controller
    {
        private readonly OrderPaymentDbContext _context;

        public HomeController(OrderPaymentDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult HomePage()
        {
            var products = _context.Products.ToList();
            return View(products);
        }

        [HttpGet]
        public IActionResult GetProductsByCategory(string category)
        {
            // Kategoriyi enum olarak almak için
            if (Enum.TryParse<Category>(category, out var categoryEnum))
            {
                var products = _context.Products.Where(p => p.Category == categoryEnum).ToList();
                return PartialView("_ProductListPartial", products);
            }

            return BadRequest("Geçersiz kategori.");
        }

        [HttpPost]
        public IActionResult AddToCart(int productId)
        {
            // Kullanıcının oturum bilgisini al
            var userJson = HttpContext.Session.GetString("User");

            if (string.IsNullOrEmpty(userJson))
            {
                // Kullanıcı oturum açmamışsa, giriş sayfasına yönlendir
                return Json(new { success = false, message = "Lütfen giriş yapın.", redirectTo = "/Auth/Login" });
            }

            // Kullanıcıyı oturumdan çözümle
            var user = JsonConvert.DeserializeObject<User>(userJson);

            // Kullanıcının sepetini veritabanından al
            var cart = _context.Carts.Include(c => c.CartItems).FirstOrDefault(c => c.UserId == user.Id);

            // Sepet yoksa, yeni bir sepet oluştur
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Carts.Add(cart); // Yeni sepeti veritabanına ekle
                _context.SaveChanges();    // Veritabanına kaydet
            }

            // Sepeti kullanıcıya ata (isteğe bağlı, başka bir yerde kullanılıyorsa)
            user.Cart = cart;

            // Ürünü veritabanından bul
            var product = _context.Products.FirstOrDefault(p => p.Id == productId);

            if (product == null)
            {
                return Json(new { success = false, message = "Ürün bulunamadı." });
            }

            // Ürün sepette mevcut mu kontrol et
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

            if (cartItem != null)
            {
                // Eğer ürün zaten sepette varsa, miktarı artır
                cartItem.Quantity += 1;
            }
            else
            {
                // Ürün sepette yoksa, yeni bir öğe olarak ekle
                cart.CartItems.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = 1,
                    Price = (decimal)product.Price,  // Fiyatın decimal türünde olduğundan emin ol
                    AddedAt = DateTime.UtcNow
                });
            }

            // Toplam tutarı güncelle
            cart.UpdateTotalAmount();
            _context.SaveChanges();

            // Başarıyla sepete ekleme işlemi bitti, güncellenmiş sepet toplamıyla yanıt döndür
            return Json(new { success = true, message = "Ürün sepete eklendi.", cartTotal = cart.TotalAmount.ToString("C") });
        }
    }
}
