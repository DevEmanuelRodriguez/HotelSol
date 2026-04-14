/*
 Controller generado por ASP.NET. para gestionar la pagina de inicio >Home<
*/

using System.Diagnostics;
using HotelSol.Models;
using HotelSol.Data;//acceder a la DB
using Microsoft.AspNetCore.Mvc;

namespace HotelSol.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DbHotelContext _context;

        public HomeController(ILogger<HomeController> logger, DbHotelContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Dashboard principal
        public IActionResult Index()
        {
            // TOTAL habitaciones
            var total = _context.Habitacions.Count();

            // HABITACIONES OCUPADAS
            // (tienen recepción sin fecha de salida)
            var ocupadas = _context.Recepcions
                .Where(r => r.FechaSalida == null)
                .Select(r => r.IdHabitacion)
                .Distinct()
                .Count();

            // DISPONIBLES
            var disponibles = total - ocupadas;

            // EN LIMPIEZA (según estado)
            var limpieza = _context.Habitacions
                .Count(h => h.IdEstadoHabitacion == 3); //  ajusta si cambia en tu BD

            //  Enviamos datos a la vista
            ViewBag.TotalHabitaciones = total;
            ViewBag.Ocupadas = ocupadas;
            ViewBag.Disponibles = disponibles;
            ViewBag.Limpieza = limpieza;

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