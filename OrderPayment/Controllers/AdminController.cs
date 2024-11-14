using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderPayment.Models;
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
    }
}
