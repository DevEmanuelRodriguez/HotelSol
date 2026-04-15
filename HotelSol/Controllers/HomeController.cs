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
            var hoy = DateTime.Today;
            var mañana = hoy.AddDays(1);

            // TOTAL habitaciones
            var total = _context.Habitacions.Count();

            // 🔥 OCUPADAS HOY (SOLAPAMIENTO REAL)
            var habitacionesOcupadas = _context.Recepcions
                .Where(r =>
                    r.IdHabitacion != null &&
                    r.FechaEntrada != null &&
                    r.FechaSalida != null &&

                    // 🔥 CLAVE
                    hoy < r.FechaSalida &&
                    mañana > r.FechaEntrada
                )
                .Select(r => r.IdHabitacion)
                .Distinct()
                .ToList();

            var ocupadas = habitacionesOcupadas.Count;

            // LIMPIEZA
            var limpieza = _context.Habitacions
                .Count(h => h.IdEstadoHabitacion == 3);

            // DISPONIBLES
            var disponibles = total - ocupadas - limpieza;

            // VIEWBAG
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