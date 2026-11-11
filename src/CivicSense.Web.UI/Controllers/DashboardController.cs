
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CivicSense.Web.UI.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.FullName =
                User.FindFirstValue(ClaimTypes.Name);

            ViewBag.Role =
                User.FindFirstValue(ClaimTypes.Role);

            ViewBag.UserId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            return View();
        }
    }
}
