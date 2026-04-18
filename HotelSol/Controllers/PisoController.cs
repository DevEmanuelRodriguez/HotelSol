using HotelSol.Data;
using HotelSol.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


public class PisoController : Controller
{
    private readonly DbHotelContext _context;

    public PisoController(DbHotelContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var lista = await _context.Pisos.ToListAsync();
        return View(lista);
    }

    public async Task<IActionResult> Obtener(int id)
    {
        return Json(await _context.Pisos.FindAsync(id));
    }

    [HttpPost]
    public async Task<IActionResult> Guardar(Piso model)
    {
        var descripcion = model.Descripcion.Trim();

        // 🔥 VALIDAR DUPLICADO
        var existe = await _context.Pisos
            .AnyAsync(p => p.Descripcion.ToLower() == descripcion.ToLower()
                        && p.IdPiso != model.IdPiso);

        if (existe)
        {
            return Json(new { ok = false, msg = "El piso ya existe." });
        }

        if (model.IdPiso == 0)
        {
            model.Estado = true;
            model.FechaCreacion = DateTime.Now;

            _context.Pisos.Add(model);
        }
        else
        {
            var p = await _context.Pisos.FindAsync(model.IdPiso);

            p.Descripcion = descripcion;
        }

        await _context.SaveChangesAsync();

        return Json(new { ok = true });
    }

    [HttpPost]
    public async Task<IActionResult> Eliminar(int id)
    {
        var p = await _context.Pisos.FindAsync(id);
        if (p != null)
        {
            _context.Pisos.Remove(p);
            await _context.SaveChangesAsync();
        }

        return Json(new { ok = true });
    }
}