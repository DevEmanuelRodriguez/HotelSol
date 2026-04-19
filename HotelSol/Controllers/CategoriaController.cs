using HotelSol.Filters;//filtro para mostrar segun sesion
using HotelSol.Data;
using HotelSol.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[RolAdmin]
public class CategoriaController : Controller
{
    private readonly DbHotelContext _context;

    public CategoriaController(DbHotelContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var lista = await _context.Categoria.ToListAsync();
        return View(lista);
    }

    public async Task<IActionResult> Obtener(int id)
    {
        var item = await _context.Categoria.FindAsync(id);
        return Json(item);
    }

    [HttpPost]
    public async Task<IActionResult> Guardar(Categorium model)
    {
        if (model.IdCategoria == 0)
        {
            model.Estado = true;
            model.FechaCreacion = DateTime.Now;

            _context.Categoria.Add(model);
        }
        else
        {
            // NO tocar Estado ni FechaCreacion
            var cat = await _context.Categoria.FindAsync(model.IdCategoria);

            cat.Descripcion = model.Descripcion;
            
        }

        await _context.SaveChangesAsync();
        return Json(new { ok = true });
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar(int id)
    {
        var cat = await _context.Categoria.FindAsync(id);

        if (cat != null)
        {
            _context.Categoria.Remove(cat);
            await _context.SaveChangesAsync();
        }

        return Json(new { ok = true });
    }
}