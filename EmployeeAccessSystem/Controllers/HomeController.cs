using System.Diagnostics;
using EmployeeAccessSystem.Models;
using Microsoft.AspNetCore.Mvc;
namespace EmployeeAccessSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }
        public IActionResult Index()
        {
            _logger.LogInformation("Home page opened at " + DateTime.Now);

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var hasFullAccessClaim = User.FindFirst("HasFullAccess")?.Value;
                if (bool.TryParse(hasFullAccessClaim, out bool hasFullAccess) && hasFullAccess)
                {
                    return RedirectToAction("Index", "AccessControl");
                }

                return View("Dashboard");
            }

            return RedirectToAction("Login", "Account");
        }
        public IActionResult Privacy()
        {
            return View();
        }
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
