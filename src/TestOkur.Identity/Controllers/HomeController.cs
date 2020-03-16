namespace TestOkur.Identity.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    public class HomeController : Controller
    {
        public IActionResult Error(string errorId) => View();
    }
}