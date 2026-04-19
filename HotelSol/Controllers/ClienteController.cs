using HotelSol.Filters;//para mostrar segun sesion
using HotelSol.Data;
using HotelSol.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelSol.Controllers
{
    [Login]
    public class ClienteController : Controller
    {
        private readonly DbHotelContext _context;

        public ClienteController(DbHotelContext context)
        {
            _context = context;
        }

        // ==========================
        // INDEX
        // ==========================
        public async Task<IActionResult> Index()
        {
            var lista = await _context.Personas
                .Where(x => x.IdTipoPersona == 3)
                .ToListAsync();

            return View(lista);
        }

        // ==========================
        // OBTENER
        // ==========================
        public async Task<IActionResult> Obtener(int id)
        {
            var c = await _context.Personas.FindAsync(id);

            return Json(c);
        }

        // ==========================
        // GUARDAR
        // ==========================
        [HttpPost]
        public async Task<IActionResult> Guardar(Persona model)
        {
            if (model.IdPersona == 0)
            {
                model.IdTipoPersona = 3;
                model.Estado = true;
                model.FechaCreacion = DateTime.Now;

                _context.Personas.Add(model);
            }
            else
            {
                var c = await _context.Personas.FindAsync(model.IdPersona);

                c.TipoDocumento = model.TipoDocumento;
                c.Documento = model.Documento;
                c.Nombre = model.Nombre;
                c.Apellido = model.Apellido;
                c.Correo = model.Correo;
                c.Estado = model.Estado;
            }

            await _context.SaveChangesAsync();

            return Json(new { ok = true });
        }

        // ==========================
        // ELIMINAR
        // ==========================
        [HttpPost]
        public async Task<IActionResult> Eliminar(int id)
        {
            var c = await _context.Personas.FindAsync(id);

            if (c != null)
            {
                _context.Personas.Remove(c);
                await _context.SaveChangesAsync();
            }

            return Json(new { ok = true });
        }
    }
}