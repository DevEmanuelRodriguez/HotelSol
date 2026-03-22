/*
 Controller generado por ASP.NET. para gestionar la pagina de inicio >Home<
*/

using System.Diagnostics;
using HotelSol.Models;
using Microsoft.AspNetCore.Mvc;

namespace HotelSol.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        // Página principal
        public IActionResult Index()
        {
            return View();
        }

       
        public IActionResult Privacy()
        {
            return View();
        }

       
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
