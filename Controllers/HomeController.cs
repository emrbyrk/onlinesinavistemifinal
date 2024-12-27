using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using onlinesinavsistemifinal.Models;

namespace onlinesinavsistemifinal.Controllers
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
            string userRole = User.Identity != null && User.IsInRole("��renci") ? "��renci" :
                              User.Identity != null && User.IsInRole("��retmen") ? "��retmen" : "Rol Yok";

            ViewData["userRole"] = userRole;
            return View();
        }




        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Kullan�c� rollerini kontrol etme
        [Authorize]
        public IActionResult CheckRoles([FromServices] UserManager<ApplicationUser> userManager)
        {
            var userRoles = User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            return Content($"Kullan�c� Roller: {string.Join(", ", userRoles)}");
        }

        // Kullan�c� oturumunu yenileme
        [Authorize]
        public async Task<IActionResult> RefreshUserSession(
            [FromServices] UserManager<ApplicationUser> userManager,
            [FromServices] SignInManager<ApplicationUser> signInManager)
        {
            var user = await userManager.GetUserAsync(User);
            if (user != null)
            {
                await signInManager.RefreshSignInAsync(user);
                return Content("Kullan�c� oturumu yenilendi ve roller g�ncellendi.");
            }
            return Content("Kullan�c� bulunamad�.");
        }
    }
}

