using Microsoft.AspNetCore.Mvc;

namespace AutoMarket.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
