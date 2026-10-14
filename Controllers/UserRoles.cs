using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Controllers
{
    public class UserRoles : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
