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
            var products = _context.products.ToList();
            return View(products);
        }


        [HttpPost]
        public IActionResult AddToCart(int productId)
        {
            // Retrieve the cart for the logged-in user
            var userJson = HttpContext.Session.GetString("LoggedInUser");

            if (string.IsNullOrEmpty(userJson))
            {
                // User is not logged in, redirect to login page
                return Json(new { success = false, message = "Lütfen giriş yapın.", redirectTo = "/User/Login" });
            }

            var user = JsonConvert.DeserializeObject<User>(userJson);

            // Retrieve the cart for the user
            var cart = _context.Carts.Include(c => c.CartItems).FirstOrDefault(c => c.UserId == user.Id);

            if (cart == null)
            {
                return Json(new { success = false, message = "Sepet bulunamadı." });
            }

            // Find the product by ID
            var product = _context.products.FirstOrDefault(p => p.Id == productId);

            if (product == null)
            {
                return Json(new { success = false, message = "Ürün bulunamadı." });
            }

            // Check if the product is already in the cart
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);

            if (cartItem != null)
            {
                // If the product is already in the cart, increase the quantity
                cartItem.Quantity += 1;
            }
            else
            {
                // If the product is not in the cart, add it
                cart.CartItems.Add(new CartItem
                {
                    ProductId = productId,
                    Quantity = 1,
                    Price = (decimal)product.Price,  // Ensure the price is a decimal
                    AddedAt = DateTime.UtcNow
                });
            }

            // Recalculate the total amount of the cart using the UpdateTotalAmount method
            cart.UpdateTotalAmount();
            _context.SaveChanges();

            // Return success response with the updated cart total
            return Json(new { success = true, message = "Ürün sepete eklendi.", cartTotal = cart.TotalAmount.ToString("C") });
        }



    }
}
