using HotelSol.Data;
using HotelSol.Models;
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
        // VENTA
        // ============================
        public async Task<IActionResult> Venta(int idRecepcion)
        {
            var recepcion = await _context.Recepcions
                .Include(r => r.IdHabitacionNavigation)
                .Include(r => r.IdClienteNavigation)
                .FirstOrDefaultAsync(r => r.IdRecepcion == idRecepcion);

            var productos = await _context.Productos
                .Where(p => p.Estado == true)
                .ToListAsync();

            ViewBag.Recepcion = recepcion;
            ViewBag.Productos = productos;

            return View();
        }

        // ============================
        // GUARDAR VENTA
        // ============================
        [HttpPost]
        public async Task<IActionResult> Venta(int idRecepcion, int idProducto, int cantidad)
        {
            var producto = await _context.Productos.FindAsync(idProducto);

            if (producto == null) return RedirectToAction("Venta", new { idRecepcion });

            var subtotal = (producto.Precio ?? 0) * cantidad;

            // 🔥 CREAR VENTA
            var venta = new Ventum
            {
                IdRecepcion = idRecepcion,
                Total = subtotal,
                Estado = "PENDIENTE"
            };

            _context.Venta.Add(venta);
            await _context.SaveChangesAsync();

            // 🔥 DETALLE
            var detalle = new DetalleVentum
            {
                IdVenta = venta.IdVenta,
                IdProducto = idProducto,
                Cantidad = cantidad,
                SubTotal = subtotal
            };

            _context.DetalleVenta.Add(detalle);

            // 🔥 RESTAR STOCK
            producto.Cantidad -= cantidad;

            await _context.SaveChangesAsync();

            return RedirectToAction("Detalle", "Recepcion", new { idRecepcion });
        }

        // ============================
        // CREAR PRODUCTO
        // ============================
        public IActionResult CrearProducto()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CrearProducto(Producto p)
        {
            p.Estado = true;
            p.FechaCreacion = DateTime.Now;

            _context.Productos.Add(p);
            await _context.SaveChangesAsync();

            return RedirectToAction("CrearProducto");
        }
    }
}
