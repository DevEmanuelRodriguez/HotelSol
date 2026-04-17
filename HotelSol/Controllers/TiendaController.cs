using System.Text.Json;
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

            var habitacionesQuery = _context.Habitacions
                .Include(h => h.IdCategoriaNavigation)
                .Include(h => h.IdPisoNavigation)
                .AsQueryable();

            if (pisoId.HasValue && pisoId.Value > 0)
            {
                habitacionesQuery = habitacionesQuery
                    .Where(h => h.IdPiso == pisoId.Value);
            }

            var habitaciones = await habitacionesQuery.ToListAsync();

            // SOLO HABITACIONES CON RECEPCION ACTIVA HOY
            var habitacionesOcupadas = habitaciones
            .Where(h => _context.Recepcions.Any(r =>
                r.IdHabitacion == h.IdHabitacion &&
                r.FechaEntrada != null &&
                r.FechaSalida != null &&
                r.FechaSalidaConfirmacion == null && // 🔥 CLAVE
                hoy >= r.FechaEntrada.Value.Date &&
                hoy < r.FechaSalida.Value.Date))
            .ToList();

            ViewBag.PisoSeleccionado = pisoId;

            ViewBag.Pisos = await _context.Pisos
                .Where(p => p.Estado == true)
                .OrderBy(p => p.IdPiso)
                .ToListAsync();

            return View(habitacionesOcupadas);
        }

        // ============================
        // FORMULARIO VENTA
        // ============================
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
                r.Estado == true && // 🔥 CLAVE
                r.FechaSalidaConfirmacion == null && // 🔥 CLAVE
                r.FechaEntrada != null &&
                r.FechaSalida != null &&
                hoy >= r.FechaEntrada.Value.Date &&
                hoy < r.FechaSalida.Value.Date);

            if (recepcion == null)
            {
                TempData["Error"] = "No hay una recepción activa para esta habitación.";
                return RedirectToAction(nameof(Index));
            }

            var cliente = await _context.Personas
                .FirstOrDefaultAsync(p => p.IdPersona == recepcion.IdCliente);

            var vm = new TiendaVentaVM
            {
                IdHabitacion = habitacion.IdHabitacion,
                IdRecepcion = recepcion.IdRecepcion,
                NumeroHabitacion = habitacion.Numero ?? "",
                Cliente = cliente == null ? "" : $"{cliente.Nombre} {cliente.Apellido}",
                Documento = cliente?.Documento ?? "",
                Categoria = habitacion.IdCategoriaNavigation?.Descripcion ?? "",
                Piso = habitacion.IdPisoNavigation?.Descripcion ?? "",
                FechaEntrada = recepcion.FechaEntrada,
                Productos = await _context.Productos
                    .Where(p => p.Estado == true && (p.Cantidad ?? 0) > 0)
                    .OrderBy(p => p.Nombre)
                    .ToListAsync()
            };

            return View(vm);
        }

        // ============================
        // GUARDAR VENTA
        // ============================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizarVenta(int IdRecepcion, string detalleJson)
        {
            Console.WriteLine("ENTRE");
            Console.WriteLine(detalleJson);
            Console.WriteLine("LLEGÓ AL CONTROLLER");
            Console.WriteLine(detalleJson);

            if (string.IsNullOrWhiteSpace(detalleJson))
            {
                TempData["Error"] = "No hay productos para registrar.";
                return RedirectToAction(nameof(Index));
            }

            var detalles = JsonSerializer.Deserialize<List<DetalleTemp>>(detalleJson);

            if (detalles == null || !detalles.Any())
            {
                TempData["Error"] = "No hay productos válidos para registrar.";
                return RedirectToAction(nameof(Index));
            }

            var recepcion = await _context.Recepcions
                .FirstOrDefaultAsync(r => r.IdRecepcion == IdRecepcion);

            if (recepcion == null)
            {
                TempData["Error"] = "La recepción no existe.";
                return RedirectToAction(nameof(Index));
            }

            decimal total = 0;

            foreach (var d in detalles)
            {
                if (d.cantidad <= 0)
                {
                    TempData["Error"] = "Hay cantidades inválidas en la venta.";
                    return RedirectToAction(nameof(Venta), new { idHabitacion = recepcion.IdHabitacion });
                }

                var productoValidacion = await _context.Productos
                    .FirstOrDefaultAsync(p => p.IdProducto == d.idProducto);

                if (productoValidacion == null)
                {
                    TempData["Error"] = "Uno de los productos ya no existe.";
                    return RedirectToAction(nameof(Venta), new { idHabitacion = recepcion.IdHabitacion });
                }

                if ((productoValidacion.Cantidad ?? 0) < d.cantidad)
                {
                    TempData["Error"] = $"No hay stock suficiente para el producto {productoValidacion.Nombre}.";
                    return RedirectToAction(nameof(Venta), new { idHabitacion = recepcion.IdHabitacion });
                }

                total += d.subTotal;
            }

            var venta = new Ventum
            {
                IdRecepcion = IdRecepcion,
                Total = total,
                Estado = "PAGADO"
            };

            _context.Venta.Add(venta);
            await _context.SaveChangesAsync();

            foreach (var d in detalles)
            {
                var producto = await _context.Productos
                    .FirstOrDefaultAsync(p => p.IdProducto == d.idProducto);

                var detalle = new DetalleVentum
                {
                    IdVenta = venta.IdVenta,
                    IdProducto = d.idProducto,
                    Cantidad = d.cantidad,
                    SubTotal = d.subTotal
                };

                _context.DetalleVenta.Add(detalle);

                if (producto != null)
                {
                    producto.Cantidad = (producto.Cantidad ?? 0) - d.cantidad;
                }
            }

            await _context.SaveChangesAsync();

            TempData["Ok"] = "Venta registrada correctamente.";

            return RedirectToAction("Detalle", "Recepcion", new { idHabitacion = recepcion.IdHabitacion });
        }
    }

    public class DetalleTemp
    {
        public int idProducto { get; set; }
        public int cantidad { get; set; }
        public decimal precio { get; set; }
        public decimal subTotal { get; set; }
    }
}