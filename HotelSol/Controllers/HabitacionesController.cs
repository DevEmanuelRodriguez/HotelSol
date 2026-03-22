/*
 Este controller gestiona, visualiza y crear habitaciones en la base de datos utilizando Entity Framework.
Se ha realizado a modo de ejemplo únicamente en la clase habitación
*/

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelSol.Data;
using HotelSol.Models;

namespace HotelSol.Controllers;

public class HabitacionesController : Controller
{
    // Acceso a la base de datos
    private readonly DbHotelContext _context;

    public HabitacionesController(DbHotelContext context)
    {
        _context = context;
    }

    // Muestra todas las habitaciones
    public async Task<IActionResult> Index()
    {
        // Obtiene los datos desde la BD
        return View(await _context.Habitacions.ToListAsync());
    }

    // Muestra formulario para crear habitación
    public IActionResult Create()
    {
        return View();
    }

    // Recibe datos del formulario
    [HttpPost]
    public async Task<IActionResult> Create(Habitacion habitacion)
    {
        // Validación del modelo
        if (!ModelState.IsValid) {
            return View(habitacion); 
        }

        // Inserta en BD
        _context.Add(habitacion);

        // Guarda cambios
        await _context.SaveChangesAsync();

        // Redirige a la lista
        return RedirectToAction(nameof(Index));
    }
}