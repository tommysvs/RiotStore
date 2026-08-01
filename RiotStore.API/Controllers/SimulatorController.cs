using Microsoft.AspNetCore.Mvc;

namespace RiotStore.API.Controllers
{
    public class SimulatorController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
