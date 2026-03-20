using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelSol.Data;
using HotelSol.Models;

namespace HotelSol.Controllers;

public class HabitacionesController : Controller
{
    private readonly DbHotelContext _context;

    public HabitacionesController(DbHotelContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.Habitacions.ToListAsync());
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Habitacion habitacion)
    {
        if (!ModelState.IsValid) return View(habitacion);

        _context.Add(habitacion);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}