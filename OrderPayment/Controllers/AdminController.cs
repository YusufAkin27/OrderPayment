
using Microsoft.AspNetCore.Mvc;
using OrderPayment.Models;
using System.IO;
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
            var products = _context.products.ToList();
            return View(products);
        }

        // Ürün Ekle Sayfası (GET)
        [HttpGet]
        public IActionResult AddProduct()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product, string imageUrl)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Eğer resim URL'si girildiyse, URL'yi kaydediyoruz
                    if (!string.IsNullOrEmpty(imageUrl))
                    {
                        product.Image = imageUrl;
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




    }
}
