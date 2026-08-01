using Microsoft.AspNetCore.Mvc;

namespace RiotStore.API.Controllers
{
    public class OrdersController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
