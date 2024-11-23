using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderPayment.Models;
using OrderPayment.Models.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using OrderPayment.Models.request;

namespace OrderPayment.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly OrderPaymentDbContext _context;
        private readonly IPasswordHasher<Admin> _passwordHasher;

        public AdminController(OrderPaymentDbContext context, IPasswordHasher<Admin> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AdminLogin()
        {
            return View();
        }

        // POST: Admin Login
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> AdminLogin(string username, string password)
        {
            var admin = await _context.Admins.FirstOrDefaultAsync(a => a.Username == username);
            if (admin != null)
            {
                
                    await SignInAdmin(admin);
                    return RedirectToAction("AdminPanel");
                
            }

            ModelState.AddModelError("", "Kullanıcı adı veya şifre yanlış.");
            return View();
        }

        private async Task SignInAdmin(Admin admin)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, admin.AdminId.ToString()),
            new Claim(ClaimTypes.Name, admin.Username),
            new Claim(ClaimTypes.Role, "Admin")
        };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
            HttpContext.Session.SetInt32("AdminId", admin.AdminId);
        }


        public async Task<IActionResult> Logout()
        {
            // Kullanıcının tüm oturumunu sonlandır
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Session'ı temizle
            HttpContext.Session.Clear();

            // Giriş sayfasına yönlendir
            return RedirectToAction("AdminLogin", "Admin");
        }




        // Admin Paneli Sayfası
        public IActionResult AdminPanel()
        {
            var products = _context.Products.AsNoTracking().ToList();
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
                    _context.Products.Add(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("AdminPanel");
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError(string.Empty, $"Veritabanı hatası: {ex.Message}");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Beklenmeyen bir hata oluştu: {ex.Message}");
                }
            }
            return View(product);
        }

        // Ürün Güncelle Sayfası (GET)
        [HttpGet]
        public IActionResult EditProduct(int id)
        {
            var product = _context.Products.AsNoTracking().FirstOrDefault(p => p.Id == id);
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
                    var existingProduct = await _context.Products.FindAsync(product.Id);
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
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError(string.Empty, $"Veritabanı hatası: {ex.Message}");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, $"Beklenmeyen bir hata oluştu: {ex.Message}");
                }
            }

            return View(product);
        }

        // Ürün Silme
        [HttpPost]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return Json(new { success = false, message = "Ürün bulunamadı." });
            }

            _context.Products.Remove(product);
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

            // Kullanıcının tamamlanmamış siparişleri varsa silinemez
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
