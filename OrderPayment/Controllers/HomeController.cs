using Microsoft.AspNetCore.Mvc;
using OrderPayment.Models;
using Microsoft.EntityFrameworkCore;

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
            return View();

        }
        // Ana Sayfa: Tüm Ürünleri Listele
        public async Task<IActionResult> HomePage(int page = 1, int pageSize = 10)
        {
            var totalProducts = await _context.products.CountAsync();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            var products = await _context.products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;

            return View(products);
        }

        // Ürün Adına Göre Arama
        public async Task<IActionResult> Search(string searchQuery, int page = 1, int pageSize = 10)
        {
            var query = _context.products.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(p => p.Name.Contains(searchQuery));
            }

            var totalProducts = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;
            ViewData["SearchQuery"] = searchQuery;

            return View("HomePage", products);
        }

        // Kategoriye Göre Ürünleri Listele
        public async Task<IActionResult> Category(Category category, int page = 1, int pageSize = 10)
        {
            var query = _context.products.Where(p => p.Category == category);

            var totalProducts = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["TotalPages"] = totalPages;
            ViewData["CurrentPage"] = page;
            ViewData["Category"] = category;

            return View("HomePage", products);
        }
    }
}
