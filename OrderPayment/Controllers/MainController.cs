using Microsoft.AspNetCore.Mvc;

namespace OrderPayment.Controllers
{
    public class MainController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
