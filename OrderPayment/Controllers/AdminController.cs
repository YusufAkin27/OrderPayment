using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderPayment.Models;
using OrderPayment.Models.request;
using OrderPayment.Models.Request;
using System.Threading.Tasks;

namespace OrderPayment.Controllers
{
    public class AdminController : Controller
    {
        private readonly OrderPaymentDbContext _context;

        public AdminController(OrderPaymentDbContext context)
        {
            _context = context;
        }

        // Admin Paneli Sayfası
        public IActionResult AdminPanel()
        {
            var products = _context.products.AsNoTracking().ToList(); // AsNoTracking ekledik
            return View(products);
        }

        // Ürün Ekle Sayfası (GET)
        [HttpGet]
        public IActionResult AddProduct()
        {
            return View();
        }

        // Ürün Ekleme İşlemi (POST)
        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Eğer resim URL'si girildiyse, URL'yi kaydediyoruz
                    if (!string.IsNullOrEmpty(product.Image))
                    {
                        product.Image = product.Image;  // Resim URL'sini doğrudan modelden al
                    }

                    _context.products.Add(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("AdminPanel");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Ürün eklenirken bir hata oluştu.");
                    ModelState.AddModelError(string.Empty, $"Hata Detayı: {ex.Message}");
                }
            }

            return View(product);
        }

        // Ürün Güncelle Sayfası (GET)
        [HttpGet]
        public IActionResult EditProduct(int id)
        {
            var product = _context.products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // Ürün Güncelleme İşlemi (POST)
        [HttpPost]
        public async Task<IActionResult> EditProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = _context.products.FirstOrDefault(p => p.Id == product.Id);
                    if (existingProduct == null)
                    {
                        return NotFound();
                    }

                    // Ürün bilgilerini güncelle
                    existingProduct.Name = product.Name;
                    existingProduct.Quantity = product.Quantity;
                    existingProduct.Price = product.Price;
                    existingProduct.Category = product.Category;
                    existingProduct.Image = product.Image;

                    await _context.SaveChangesAsync();
                    return RedirectToAction("AdminPanel");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Ürün güncellenirken bir hata oluştu.");
                    ModelState.AddModelError(string.Empty, $"Hata Detayı: {ex.Message}");
                }
            }

            return View(product);
        }

        // Ürün Silme
        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.products.Remove(product);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // Siparişlerin listelendiği sayfa
        public async Task<IActionResult> OrderList()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.products)
                .AsNoTracking()  // Veritabanından alınan verilerin takibi yapılmaz
                .ToListAsync();

            return View(orders);
        }

        // Sipariş detayları sayfası
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.products)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // Sipariş durumu güncelleme
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus newStatus)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
                return NotFound();

            order.Status = newStatus;
            await _context.SaveChangesAsync();

            return RedirectToAction("OrderDetails", new { id = orderId });  // OrderId parametre olarak verildi
        }


        public IActionResult Users()
        {
            var users = _context.Users.AsNoTracking().ToList(); // AsNoTracking ekledik
            return View(users);
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteUser([FromBody] DeleteUserRequest request)
        {
            var user = await _context.Users
                .Include(u => u.Orders) // Kullanıcıyla ilişkili siparişleri kontrol edebilmek için dahil ediyoruz
                .FirstOrDefaultAsync(u => u.Id == request.Id);

            if (user == null)
            {
                return Json(new { success = false, message = "Kullanıcı bulunamadı." });
            }

            // Ek kontrol: Kullanıcının herhangi bir siparişi var mı?
            if (user.Orders.Any())
            {
                // Kullanıcının aktif siparişlerinin durumunu kontrol et
                bool hasUnfinishedOrders = user.Orders.Any(o =>
                    o.Status == OrderStatus.Beklemede ||
                    o.Status == OrderStatus.Onaylandi ||
                    o.Status == OrderStatus.Kargolandı);

                if (hasUnfinishedOrders)
                {
                    return Json(new { success = false, message = "Kullanıcının tamamlanmamış siparişleri olduğu için silinemez." });
                }
            }

            try
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Hata: {ex.Message}" });
            }
        }







        // Kullanıcı düzenleme sayfasına yönlendiren eylemi unutmayın
        [HttpGet]
        public IActionResult EditUser(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            // Kullanıcıyı düzenleme sayfasına gönderiyoruz
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(UpdateUserRequest updateRequest)
        {
            // Eğer model geçerli değilse (boş alan varsa), hatayı döndürüyoruz
            if (!ModelState.IsValid)
            {
                return View(updateRequest);
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == updateRequest.Id);
            if (existingUser == null)
            {
                return NotFound();
            }

            // Yalnızca dolu alanları güncelleme işlemi
            if (!string.IsNullOrEmpty(updateRequest.FirstName))
            {
                existingUser.FirstName = updateRequest.FirstName;
            }

            if (!string.IsNullOrEmpty(updateRequest.LastName))
            {
                existingUser.LastName = updateRequest.LastName;
            }

            if (!string.IsNullOrEmpty(updateRequest.PhoneNumber))
            {
                existingUser.PhoneNumber = updateRequest.PhoneNumber;
            }

            // `IsActive` bool olduğu için null kontrolü yapılmaz, doğrudan güncellenebilir
            existingUser.IsActive = updateRequest.IsActive;

            try
            {
                // Veritabanını güncelliyoruz
                await _context.SaveChangesAsync();
                // Başarıyla güncellendiyse kullanıcı listesine yönlendiriyoruz
                return RedirectToAction("Users");
            }
            catch (Exception ex)
            {
                // Hata durumunda mesaj gösteriyoruz
                ModelState.AddModelError("", $"Hata: {ex.Message}");
                return View(updateRequest);
            }
        }

    }
}
