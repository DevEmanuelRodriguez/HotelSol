using HotelSol.Filters;//para mostrar segun sesion
using System.Diagnostics;
using HotelSol.Models;
using HotelSol.Data;
using Microsoft.AspNetCore.Mvc;

namespace HotelSol.Controllers
{
    [Login]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DbHotelContext _context;

        public HomeController(ILogger<HomeController> logger, DbHotelContext context)
        {
            _logger = logger;
            _context = context;
        }

        // ================================
        // DASHBOARD 
        // ================================
        public IActionResult Index()
        {
            var hoy = DateTime.Today;

            // TOTAL habitaciones
            var total = _context.Habitacions.Count();

            // 🔴 OCUPADAS HOY (VALIDACIÓN SEGURA)
            var ocupadasHoy = _context.Recepcions
            .Where(r =>
                r.IdHabitacion != null &&
                r.Estado == true && // 🔥 extra seguridad
                r.FechaSalidaConfirmacion == null &&
                r.FechaEntrada <= hoy &&
                r.FechaSalida > hoy
            )
            .Select(r => r.IdHabitacion)
            .Distinct()
            .Count();

            // 🟡 RESERVAS FUTURAS
            var futuras = _context.Recepcions
                .Where(r =>
                    r.FechaEntrada != null &&
                    r.FechaEntrada.Value.Date > hoy
                )
                .Count();

            // 🔵 LIMPIEZA
            var limpieza = _context.Habitacions
                .Count(h => h.IdEstadoHabitacion == 3);

            // 🟢 DISPONIBLES HOY
            var disponibles = total - ocupadasHoy - limpieza;

            // 📊 PORCENTAJE OCUPACIÓN
            var porcentaje = total > 0
                ? (ocupadasHoy * 100.0) / total
                : 0;

            // ================================
            // VIEWBAG
            // ================================
            ViewBag.TotalHabitaciones = total;
            ViewBag.OcupadasHoy = ocupadasHoy;
            ViewBag.ReservasFuturas = futuras;
            ViewBag.Disponibles = disponibles;
            ViewBag.Limpieza = limpieza;
            ViewBag.PorcentajeOcupacion = porcentaje.ToString("0.00");

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