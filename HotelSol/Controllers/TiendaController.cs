using HotelSol.Filters;
using System.Text.Json;
using HotelSol.Data;
using HotelSol.Models;
using HotelSol.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelSol.Controllers
{
    [Login]
    public class TiendaController : Controller
    {
        private readonly DbHotelContext _context;

        public TiendaController(DbHotelContext context)
        {
            _context = context;
        }

        // INDEX (SOLO HABITACIONES OCUPADAS - CHECKIN REALIZADO)
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

            // SOLO OCUPADAS (Estado = true)
            var habitacionesOcupadas = habitaciones
                .Where(h => _context.Recepcions.Any(r =>
                    r.IdHabitacion == h.IdHabitacion &&
                    r.Estado == true &&
                    r.FechaSalidaConfirmacion == null &&
                    r.FechaEntrada != null &&
                    r.FechaSalida != null &&
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

       
        // FORMULARIO VENTA
        
        public async Task<IActionResult> Venta(int idHabitacion)
        {
            var hoy = DateTime.Today;

            var habitacion = await _context.Habitacions
                .Include(h => h.IdCategoriaNavigation)
                .Include(h => h.IdPisoNavigation)
                .FirstOrDefaultAsync(h => h.IdHabitacion == idHabitacion);

            if (habitacion == null)
                return NotFound();

            // SOLO RECEPCION OCUPADA
            var recepcion = await _context.Recepcions
                .FirstOrDefaultAsync(r =>
                    r.IdHabitacion == idHabitacion &&
                    r.Estado == true &&
                    r.FechaSalidaConfirmacion == null &&
                    r.FechaEntrada != null &&
                    r.FechaSalida != null &&
                    hoy >= r.FechaEntrada.Value.Date &&
                    hoy < r.FechaSalida.Value.Date);

            if (recepcion == null)
            {
                TempData["Error"] = "No hay una habitación ocupada para vender.";
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

       
        // FINALIZAR VENTA
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizarVenta(int IdRecepcion, string detalleJson)
        {
            if (string.IsNullOrWhiteSpace(detalleJson))
            {
                TempData["Error"] = "No hay productos para registrar.";
                return RedirectToAction(nameof(Index));
            }

            var detalles = JsonSerializer.Deserialize<List<DetalleTemp>>(detalleJson);

            if (detalles == null || !detalles.Any())
            {
                TempData["Error"] = "No hay productos válidos.";
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
                    TempData["Error"] = "Cantidad inválida.";
                    return RedirectToAction(nameof(Venta),
                        new { idHabitacion = recepcion.IdHabitacion });
                }

                var producto = await _context.Productos
                    .FirstOrDefaultAsync(p => p.IdProducto == d.idProducto);

                if (producto == null)
                {
                    TempData["Error"] = "Producto no existe.";
                    return RedirectToAction(nameof(Venta),
                        new { idHabitacion = recepcion.IdHabitacion });
                }

                if ((producto.Cantidad ?? 0) < d.cantidad)
                {
                    TempData["Error"] = $"Stock insuficiente: {producto.Nombre}";
                    return RedirectToAction(nameof(Venta),
                        new { idHabitacion = recepcion.IdHabitacion });
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

            return RedirectToAction("Detalle", "Recepcion",
                new { idHabitacion = recepcion.IdHabitacion });
        }

       
        // PRODUCTOS
        
        public async Task<IActionResult> Productos()
        {
            var productos = await _context.Productos
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            return View(productos);
        }

       
        // OBTENER PRODUCTO
        
        public async Task<IActionResult> ObtenerProducto(int id)
        {
            var prod = await _context.Productos.FindAsync(id);
            return Json(prod);
        }

        
        // GUARDAR PRODUCTO
        [HttpPost]
        public async Task<IActionResult> GuardarProducto(Producto model)
        {
            if (model.IdProducto == 0)
            {
                model.Estado = true;
                model.FechaCreacion = DateTime.Now;

                _context.Productos.Add(model);
            }
            else
            {
                var prod = await _context.Productos.FindAsync(model.IdProducto);

                if (prod == null)
                    return Json(new { ok = false });

                prod.Nombre = model.Nombre;
                prod.Detalle = model.Detalle;
                prod.Precio = model.Precio;
                prod.Cantidad = model.Cantidad;
            }

            await _context.SaveChangesAsync();

            return Json(new { ok = true });
        }

        
        // ELIMINAR PRODUCTO
        
        [HttpPost]
        public async Task<IActionResult> EliminarProducto(int id)
        {
            var prod = await _context.Productos.FindAsync(id);

            if (prod != null)
            {
                _context.Productos.Remove(prod);
                await _context.SaveChangesAsync();
            }

            return Json(new { ok = true });
        }
    }

    
    // CLASE TEMPORAL 

    public class DetalleTemp
    {
        public int idProducto { get; set; }
        public int cantidad { get; set; }
        public decimal precio { get; set; }
        public decimal subTotal { get; set; }
    }
}