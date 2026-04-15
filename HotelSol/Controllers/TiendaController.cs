using HotelSol.Data;
using HotelSol.Models;
using HotelSol.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelSol.Controllers
{
    public class TiendaController : Controller
    {
        private readonly DbHotelContext _context;

        public TiendaController(DbHotelContext context)
        {
            _context = context;
        }

        // ============================
        // INDEX (HABITACIONES OCUPADAS)
        // ============================
        public async Task<IActionResult> Index(int? pisoId)
        {
            var hoy = DateTime.Today;

            var habitaciones = await _context.Habitacions
                .Include(h => h.IdCategoriaNavigation)
                .Include(h => h.IdPisoNavigation)
                .Where(h =>
                    h.IdEstadoHabitacion == 2 // 🔴 OCUPADO
                )
                .ToListAsync();

            // 🔥 SOLO HABITACIONES CON RECEPCIÓN ACTIVA
            var habitacionesOcupadas = new List<Habitacion>();

            foreach (var h in habitaciones)
            {
                var tieneRecepcion = await _context.Recepcions.AnyAsync(r =>
                    r.IdHabitacion == h.IdHabitacion &&
                    r.FechaEntrada <= hoy &&
                    r.FechaSalida > hoy
                );

                if (tieneRecepcion)
                    habitacionesOcupadas.Add(h);
            }

            ViewBag.PisoSeleccionado = pisoId;

            ViewBag.Pisos = await _context.Pisos
                .Where(p => p.Estado == true)
                .ToListAsync();

            return View(habitacionesOcupadas);
        }
        public async Task<IActionResult> Venta(int idHabitacion)
        {
            var hoy = DateTime.Today;

            var habitacion = await _context.Habitacions
                .Include(h => h.IdCategoriaNavigation)
                .Include(h => h.IdPisoNavigation)
                .FirstOrDefaultAsync(h => h.IdHabitacion == idHabitacion);

            if (habitacion == null)
                return NotFound();

            var recepcion = await _context.Recepcions
                .FirstOrDefaultAsync(r =>
                    r.IdHabitacion == idHabitacion &&
                    r.FechaEntrada <= hoy &&
                    r.FechaSalida > hoy);

            if (recepcion == null)
            {
                TempData["Error"] = "No hay hospedaje activo.";
                return RedirectToAction("Index");
            }

            var cliente = await _context.Personas
                .FirstOrDefaultAsync(p => p.IdPersona == recepcion.IdCliente);

            var vm = new TiendaVentaVM
            {
                IdHabitacion = habitacion.IdHabitacion,
                IdRecepcion = recepcion.IdRecepcion,

                NumeroHabitacion = habitacion.Numero ?? "",
                Categoria = habitacion.IdCategoriaNavigation?.Descripcion ?? "",
                Piso = habitacion.IdPisoNavigation?.Descripcion ?? "",

                Cliente = cliente == null ? "" : $"{cliente.Nombre} {cliente.Apellido}",
                Documento = cliente?.Documento ?? "",

                FechaEntrada = recepcion.FechaEntrada,

                Productos = await _context.Productos
                    .Where(p => p.Estado == true)
                    .ToListAsync()
            };

            return View(vm);
        }
    }
}
