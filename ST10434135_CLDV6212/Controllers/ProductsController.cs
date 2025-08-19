using Microsoft.AspNetCore.Mvc;

namespace ST10434135_CLDV6212.Controllers
{
    public class ProductsController : Controller
    {
        public IActionResult Index() => View();
        public IActionResult Create() => View();
        public IActionResult Details(int id) => View();
    }
}
