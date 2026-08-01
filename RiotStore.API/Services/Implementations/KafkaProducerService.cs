using Microsoft.AspNetCore.Mvc;

namespace RiotStore.API.Services.Implementations
{
    public class KafkaProducerService : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
