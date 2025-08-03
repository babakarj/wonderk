using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WonderK.RuleChecker.Models;

namespace WonderK.RuleChecker.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IWebHostEnvironment env, ILogger<HomeController> logger)
        {
            _env = env;
            _logger = logger;
        }

        public IActionResult Index()
        {
            string filePath = Path.Combine(_env.ContentRootPath, "rules-book.txt");
            string fileContent = System.IO.File.Exists(filePath)
                ? System.IO.File.ReadAllText(filePath)
                : "File not found.";

            ViewBag.FileContent = fileContent;

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
