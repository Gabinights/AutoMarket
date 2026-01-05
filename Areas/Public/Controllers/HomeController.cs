using System.Diagnostics;
using AutoMarket.Models.ViewModels;

namespace AutoMarket.Areas.Public.Controllers
{
    /// <summary>
    /// Provides basic site pages such as the home, privacy, and error views.
    /// </summary>
    [Area("Public")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="logger">Logger used for diagnostic information.</param>
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Renders the home page.
        /// </summary>
        /// <returns>A view representing the home page.</returns>
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Renders the privacy policy page.
        /// </summary>
        /// <returns>A view containing the privacy policy.</returns>
        public IActionResult Privacy()
        {
            return View();
        }


        /// <summary>
        /// Renders the error page including the current request identifier.
        /// </summary>
        /// <returns>A view with error details for troubleshooting.</returns>
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
