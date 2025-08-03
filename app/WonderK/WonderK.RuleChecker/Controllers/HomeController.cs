using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using WonderK.RuleChecker.Models;

namespace WonderK.RuleChecker.Controllers
{
    public class HomeController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<HomeController> _logger;
        private readonly string _rulebookFile;

        public HomeController(IWebHostEnvironment env, ILogger<HomeController> logger)
        {
            _env = env;
            _logger = logger;
            _rulebookFile = Path.Combine(_env.ContentRootPath, "rules-book.txt");
        }

        public IActionResult Index()
        {
            string filePath = _rulebookFile;
            string fileContent = System.IO.File.Exists(filePath)
                ? System.IO.File.ReadAllText(filePath)
                : "File not found.";

            ViewBag.FileContent = fileContent;

            return View();
        }

        [HttpPost]
        public IActionResult EditRuleBook(string fileContent)
        {
            try
            {
                _ = Rule.ParseRules(fileContent);

                System.IO.File.WriteAllText(_rulebookFile, fileContent ?? "");
                ViewBag.FileContent = fileContent;
                ViewBag.Error = null;
                ViewBag.Success = "Rules updated successfully.";
            }
            catch (Exception ex)
            {
                ViewBag.FileContent = fileContent;
                ViewBag.Error = $"Error: {ex.Message}";
                ViewBag.Success = null;
            }

            return View("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
