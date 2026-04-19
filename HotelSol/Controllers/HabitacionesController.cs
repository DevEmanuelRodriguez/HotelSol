/*
 Este controller gestiona, visualiza y crear habitaciones en la base de datos utilizando Entity Framework.
Se ha realizado a modo de ejemplo únicamente en la clase habitación
*/
using HotelSol.Filters;//filtro para mostrar segun sesion
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelSol.Data;
using HotelSol.Models;

namespace HotelSol.Controllers;

using HotelSol.Data;
using HotelSol.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[RolAdmin]
public class HabitacionController : Controller
{
    private readonly DbHotelContext _context;

    public HabitacionController(DbHotelContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.Categorias = await _context.Categoria
            .Where(c => c.Estado == true)
            .ToListAsync();

        ViewBag.Pisos = await _context.Pisos
            .Where(p => p.Estado == true)
            .ToListAsync();

        var lista = await _context.Habitacions
            .Include(h => h.IdCategoriaNavigation)
            .Include(h => h.IdPisoNavigation)
            .ToListAsync();

        return View(lista);
    }

    public async Task<IActionResult> Obtener(int id)
    {
        var item = await _context.Habitacions.FindAsync(id);
        return Json(item);
    }

    [HttpPost]
    public async Task<IActionResult> Guardar(Habitacion model)
    {
        if (model.IdHabitacion == 0)
        {
            model.Estado = true;
            model.FechaCreacion = DateTime.Now;

            // Estado habitacion
            model.IdEstadoHabitacion = 1; // 1 = DISPONIBLE

            _context.Habitacions.Add(model);
        }
        else
        {
            var h = await _context.Habitacions.FindAsync(model.IdHabitacion);

            h.Numero = model.Numero;
            h.Detalle = model.Detalle;
            h.Precio = model.Precio;
            h.IdCategoria = model.IdCategoria;
            h.IdPiso = model.IdPiso;
        }

        await _context.SaveChangesAsync();
        return Json(new { ok = true });
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar(int id)
    {
        var h = await _context.Habitacions.FindAsync(id);
        if (h != null)
        {
            _context.Habitacions.Remove(h);
            await _context.SaveChangesAsync();
        }
        return Json(new { ok = true });
    }
}