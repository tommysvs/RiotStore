using Microsoft.AspNetCore.Mvc;

namespace RiotStore.API.Services.Implementations
{
    public class SimulatorService : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
