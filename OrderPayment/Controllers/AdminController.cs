using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderPayment.Models;
using OrderPayment.Models.request;
using OrderPayment.Models.Request;
using System;
using System.Linq;
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
            var products = _context.products.AsNoTracking().ToList();
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
                    _context.products.Add(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("AdminPanel");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Ürün eklenirken bir hata oluştu: {ex.Message}");
                }
            }

            return View(product);
        }

        // Ürün Güncelle Sayfası (GET)
        [HttpGet]
        public IActionResult EditProduct(int id)
        {
            var product = _context.products.AsNoTracking().FirstOrDefault(p => p.Id == id);
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
                    var existingProduct = await _context.products.FindAsync(product.Id);
                    if (existingProduct == null)
                    {
                        return NotFound();
                    }

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
                    ModelState.AddModelError(string.Empty, $"Ürün güncellenirken bir hata oluştu: {ex.Message}");
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
                return Json(new { success = false, message = "Ürün bulunamadı." });
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
                .AsNoTracking()
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

            return RedirectToAction("OrderDetails", new { id = orderId });
        }

        // Kullanıcı listesi sayfası
        public IActionResult Users()
        {
            var users = _context.Users.AsNoTracking().ToList();
            return View(users);
        }

        // Kullanıcı Silme
        [HttpDelete]
        public async Task<IActionResult> DeleteUser([FromBody] DeleteUserRequest request)
        {
            var user = await _context.Users
                .Include(u => u.Orders)
                .FirstOrDefaultAsync(u => u.Id == request.Id);

            if (user == null)
            {
                return Json(new { success = false, message = "Kullanıcı bulunamadı." });
            }

            if (user.Orders.Any(o =>
                o.Status == OrderStatus.Beklemede ||
                o.Status == OrderStatus.Onaylandi ||
                o.Status == OrderStatus.Kargolandı))
            {
                return Json(new { success = false, message = "Kullanıcının tamamlanmamış siparişleri olduğu için silinemez." });
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

        // Kullanıcı Düzenleme Sayfası (GET)
        [HttpGet]
        public IActionResult EditUser(int id)
        {
            var user = _context.Users.AsNoTracking().FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var updateUserRequest = new UpdateUserRequest
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                IsActive = user.IsActive
            };

            return View(updateUserRequest);
        }

        // Kullanıcı Güncelleme İşlemi (POST)
        [HttpPost]
        public async Task<IActionResult> EditUser(UpdateUserRequest updateRequest)
        {
            if (!ModelState.IsValid)
            {
                return View(updateRequest);
            }

            var existingUser = await _context.Users.FindAsync(updateRequest.Id);
            if (existingUser == null)
            {
                return NotFound();
            }

            existingUser.FirstName = updateRequest.FirstName ?? existingUser.FirstName;
            existingUser.LastName = updateRequest.LastName ?? existingUser.LastName;
            existingUser.PhoneNumber = updateRequest.PhoneNumber ?? existingUser.PhoneNumber;
            existingUser.IsActive = updateRequest.IsActive;

            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction("Users");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Hata: {ex.Message}");
                return View(updateRequest);
            }
        }
    }
}
