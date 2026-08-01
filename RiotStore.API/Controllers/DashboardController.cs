using Microsoft.AspNetCore.Mvc;

namespace RiotStore.API.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
