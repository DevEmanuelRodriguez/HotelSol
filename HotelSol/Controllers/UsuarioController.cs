using HotelSol.Data;
using HotelSol.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelSol.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly DbHotelContext _context;

        public UsuarioController(DbHotelContext context)
        {
            _context = context;
        }

        // =========================
        // INDEX
        // =========================
        public async Task<IActionResult> Index()
        {
            var lista = await _context.Personas
                .Include(x => x.IdTipoPersonaNavigation)
                .Where(x => x.IdTipoPersona == 1 || x.IdTipoPersona == 2)
                .ToListAsync();

            ViewBag.Tipos = await _context.TipoPersonas
                .Where(x => x.IdTipoPersona == 1 || x.IdTipoPersona == 2)
                .ToListAsync();

            return View(lista);
        }

        // =========================
        // OBTENER
        // =========================
        public async Task<IActionResult> Obtener(int id)
        {
            var p = await _context.Personas.FindAsync(id);

            if (p == null) return Json(null);

            return Json(p);
        }

        // =========================
        // GUARDAR
        // =========================
        [HttpPost]
        public async Task<IActionResult> Guardar(Persona model)
        {
            if (model.IdPersona == 0)
            {
                model.Estado = true;
                model.FechaCreacion = DateTime.Now;

                _context.Personas.Add(model);
            }
            else
            {
                var persona = await _context.Personas.FindAsync(model.IdPersona);

                persona.TipoDocumento = model.TipoDocumento;
                persona.Documento = model.Documento;
                persona.Nombre = model.Nombre;
                persona.Apellido = model.Apellido;
                persona.Correo = model.Correo;
                persona.Clave = model.Clave;
                persona.IdTipoPersona = model.IdTipoPersona;
                persona.Estado = model.Estado;
            }

            await _context.SaveChangesAsync();

            return Json(new { ok = true });
        }

        // =========================
        // ELIMINAR
        // =========================
        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var persona = await _context.Personas.FindAsync(id);

            if (persona != null)
            {
                _context.Personas.Remove(persona);
                await _context.SaveChangesAsync();
            }

            return Json(new { ok = true });
        }
    }
}