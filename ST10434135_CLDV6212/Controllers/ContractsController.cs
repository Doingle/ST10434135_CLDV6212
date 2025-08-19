using Microsoft.AspNetCore.Mvc;

namespace ST10434135_CLDV6212.Controllers
{
    public class ContractsController : Controller
    {
        public IActionResult Index() => View();
        public IActionResult Upload() => View();

    }
}
