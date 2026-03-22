/*
 Este controller permite al usuario seleccionar una tabla de la base de datos
 y visualizar su contenido.
 Utiliza Entity Framework para obtener los datos desde SQL Server.
*/

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelSol.Data;

namespace HotelSol.Controllers;

public class DatosController : Controller
{
    // DbContext: permite acceder a la base de datos
    private readonly DbHotelContext _context;

    // Inyección de dependencias del DbContext
    public DatosController(DbHotelContext context)
    {
        _context = context;
    }

    // Vista inicial donde se muestra el selector de tablas
    public IActionResult Index()
    {
        return View();
    }

    // Método que recibe la tabla seleccionada por el usuario
    [HttpPost]
    public async Task<IActionResult> VerTabla(string tabla)
    {
        // Según la tabla elegida, se consulta la BD
        switch (tabla)
        {
            case "Categoria":
                var categorias = await _context.Categoria.ToListAsync();
                return View("Categoria", categorias);

            case "Piso":
                var pisos = await _context.Pisos.ToListAsync();
                return View("Piso", pisos);

            case "EstadoHabitacion":
                var estados = await _context.EstadoHabitacions.ToListAsync();
                return View("EstadoHabitacion", estados);

            case "TipoPersona":
                var tipos = await _context.TipoPersonas.ToListAsync();
                return View("TipoPersona", tipos);

            case "Producto":
                var productos = await _context.Productos.ToListAsync();
                return View("Producto", productos);

            case "Persona":
                var personas = await _context.Personas.ToListAsync();
                return View("Persona", personas);

            case "Habitacion":
                var habitaciones = await _context.Habitacions.ToListAsync();
                return View("Habitacion", habitaciones);

            case "Recepcion":
                var recepciones = await _context.Recepcions.ToListAsync();
                return View("Recepcion", recepciones);

            case "Venta":
                var ventas = await _context.Venta.ToListAsync();
                return View("Venta", ventas);

            case "DetalleVenta":
                var detalles = await _context.DetalleVenta.ToListAsync();
                return View("DetalleVenta", detalles);

            default:
                // Si no coincide, vuelve al selector
                return RedirectToAction("Index");
        }
    }
}