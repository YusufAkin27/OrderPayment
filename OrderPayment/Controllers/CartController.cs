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
            // Session validation
            var userJson = HttpContext.Session.GetString("LoggedInUser");
            if (string.IsNullOrEmpty(userJson))
            {
                return RedirectToAction("Login", "User");
            }

            var user = JsonConvert.DeserializeObject<User>(userJson);
            if (user == null)
            {
                return RedirectToAction("Login", "User");
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




        // Sepetteki ürünü ekleme veya güncelleme işlemi (POST isteği)
        [HttpPost]
        public IActionResult UpdateCart(int productId, int quantity)
        {
            var userJson = HttpContext.Session.GetString("LoggedInUser");
            if (string.IsNullOrEmpty(userJson))
            {
                return Json(new { success = false, message = "User session invalid. Please log in again." });
            }

            var user = JsonConvert.DeserializeObject<User>(userJson);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.UserId == user.Id);

            if (cart == null)
            {
                return Json(new { success = false, message = "Cart not found." });
            }

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
                if (cartItem.Quantity <= 0)
                {
                    cart.CartItems.Remove(cartItem);
                }
                _context.SaveChanges();
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Product not found in cart." });
        }

        // Confirm Cart
        [HttpPost]
        public IActionResult ConfirmCart()
        {
            var userJson = HttpContext.Session.GetString("LoggedInUser");
            if (string.IsNullOrEmpty(userJson))
            {
                return Json(new { success = false, message = "User session invalid. Please log in again." });
            }

            var user = JsonConvert.DeserializeObject<User>(userJson);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefault(c => c.UserId == user.Id);

            if (cart == null || !cart.CartItems.Any())
            {
                return Json(new { success = false, message = "Your cart is empty." });
            }

            // Implement order processing logic here
            // ...

            // Clear the cart after confirmation
            _context.CartItems.RemoveRange(cart.CartItems);
            _context.SaveChanges();

            return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
        }
    }
}
