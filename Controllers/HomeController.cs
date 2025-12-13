using System.Diagnostics;
using AutoMarket.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace AutoMarket.Controllers
{
    /// <summary>
    /// Provides basic site pages such as the home, privacy, and error views.
    /// </summary>
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
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Renders the privacy policy page.
        /// </summary>
        /// <returns>A view containing the privacy policy.</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        /// <summary>
        /// Renders the error page including the current request identifier.
        /// </summary>
        /// <returns>A view with error details for troubleshooting.</returns>
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
