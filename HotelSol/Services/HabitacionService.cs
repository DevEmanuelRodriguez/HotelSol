using HotelSol.Data;
using HotelSol.Models;

namespace HotelSol.Services
{
    public class HabitacionService
    {
        private readonly DbHotelContext _context;

        public HabitacionService(DbHotelContext context)
        {
            _context = context;
        }

        public void GuardarHabitacion(Habitacion habitacion)
        {
            _context.Habitacions.Add(habitacion);
            _context.SaveChanges();
        }

        public List<Habitacion> ObtenerHabitaciones()
        {
            return _context.Habitacions.ToList();
        }
    }
}