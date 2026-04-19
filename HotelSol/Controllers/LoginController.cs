using HotelSol.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelSol.Controllers
{
    public class LoginController : Controller
    {
        private readonly DbHotelContext _context;

        public LoginController(DbHotelContext context)
        {
            _context = context;
        }

 
        // VISTA LOGIN
       
        public IActionResult Index()
        {
            return View();
        }

        // INGRESAR
     
        [HttpPost]
        public async Task<IActionResult> Ingresar(string usuario, string clave)
        {
            var persona = await _context.Personas
                .Include(x => x.IdTipoPersonaNavigation)
                .FirstOrDefaultAsync(x =>
                    (x.Documento == usuario || x.Correo == usuario) &&
                    x.Clave == clave &&
                    x.Estado == true &&
                    (x.IdTipoPersona == 1 || x.IdTipoPersona == 2));

            if (persona == null)
            {
                ViewBag.Error = "Datos incorrectos";
                return View("Index");
            }

            HttpContext.Session.SetString(
                "Usuario",
                $"{persona.Nombre} {persona.Apellido}"
            );

            HttpContext.Session.SetString(
                "Rol",
                persona.IdTipoPersona == 1 ? "ADMIN" : "EMPLEADO"
            );

            HttpContext.Session.SetInt32("IdUsuario", persona.IdPersona);

            return RedirectToAction("Index", "Home");
        }


        // SALIR
        
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index");
        }
    }
}