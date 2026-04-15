using HotelSol.Data;
using HotelSol.Models;
using HotelSol.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelSol.Controllers
{
    public class RecepcionController : Controller
    {
        private readonly DbHotelContext _context;

        public RecepcionController(DbHotelContext context)
        {
            _context = context;
        }

        // ================================
        // FUNCIÓN TEMPORADAS
        // ================================
        private decimal ObtenerMultiplicador(DateTime fecha)
        {
            if (fecha.Month >= 6 && fecha.Month <= 8)
                return 1.5m; // 🔴 ALTA

            if (fecha.Month == 1 || fecha.Month == 2)
                return 0.8m; // 🟢 BAJA

            return 1m; // 🟡 MEDIA
        }

        // ================================
        // INDEX (DISPONIBILIDAD REAL)
        // ================================
        public async Task<IActionResult> Index(int? pisoId, DateTime? fechaEntrada, DateTime? fechaSalida)
        {
            var habitacionesQuery = _context.Habitacions
                .Include(h => h.IdCategoriaNavigation)
                .Include(h => h.IdPisoNavigation)
                .Include(h => h.IdEstadoHabitacionNavigation)
                .AsQueryable();

            if (pisoId.HasValue && pisoId.Value > 0)
            {
                habitacionesQuery = habitacionesQuery
                    .Where(h => h.IdPiso == pisoId.Value);
            }

            var habitaciones = await habitacionesQuery.ToListAsync();

            // 🔥 HABITACIONES OCUPADAS EN RANGO
            List<int?> habitacionesOcupadas = new();

            if (fechaEntrada.HasValue && fechaSalida.HasValue)
            {
                habitacionesOcupadas = await _context.Recepcions
                    .Where(r =>
                        r.IdHabitacion != null &&
                        r.FechaEntrada != null &&
                        r.FechaSalida != null &&
                        fechaEntrada.Value < r.FechaSalida.Value &&
                        fechaSalida.Value > r.FechaEntrada.Value
                    )
                    .Select(r => r.IdHabitacion)
                    .Distinct()
                    .ToListAsync();
            }

            var vm = new RecepcionIndexVM
            {
                PisoSeleccionado = pisoId,
                FechaEntrada = fechaEntrada,
                FechaSalida = fechaSalida,

                Pisos = await _context.Pisos
                    .Where(p => p.Estado == true)
                    .OrderBy(p => p.IdPiso)
                    .ToListAsync(),

                Habitaciones = habitaciones.Select(h =>
                {
                    var ocupada = habitacionesOcupadas.Contains(h.IdHabitacion);
                    var limpieza = h.IdEstadoHabitacion == 3;

                    string estadoTexto;
                    string estadoCss;
                    bool puedeReservar;

                    if (ocupada)
                    {
                        estadoTexto = "OCUPADO";
                        estadoCss = "ocupado";
                        puedeReservar = false;
                    }
                    else if (limpieza)
                    {
                        estadoTexto = "LIMPIEZA";
                        estadoCss = "limpieza";
                        puedeReservar = false;
                    }
                    else
                    {
                        estadoTexto = "DISPONIBLE";
                        estadoCss = "disponible";
                        puedeReservar = true;
                    }

                    return new HabitacionCardVM
                    {
                        IdHabitacion = h.IdHabitacion,
                        Numero = h.Numero ?? "",
                        Categoria = h.IdCategoriaNavigation?.Descripcion ?? "",
                        Piso = h.IdPisoNavigation?.Descripcion ?? "",
                        EstadoTexto = estadoTexto,
                        EstadoCss = estadoCss,
                        PuedeReservar = puedeReservar,

                        // 🔥 IMPORTANTE PARA LA VISTA
                        OcupadaEnFechas = ocupada
                    };
                }).ToList()
            };

            return View(vm);
        }

        // ================================
        // FORMULARIO CREATE
        // ================================
        public async Task<IActionResult> Create(int id, DateTime? fechaEntrada, DateTime? fechaSalida)
        {
            var habitacion = await _context.Habitacions
                .Include(h => h.IdCategoriaNavigation)
                .Include(h => h.IdPisoNavigation)
                .FirstOrDefaultAsync(h => h.IdHabitacion == id);

            if (habitacion == null)
                return NotFound();

            var vm = new RecepcionCreateVM
            {
                IdHabitacion = habitacion.IdHabitacion,
                NumeroHabitacion = habitacion.Numero ?? "",
                Categoria = habitacion.IdCategoriaNavigation?.Descripcion ?? "",
                Piso = habitacion.IdPisoNavigation?.Descripcion ?? "",
                DetalleHabitacion = habitacion.Detalle ?? "",
                PrecioHabitacion = habitacion.Precio ?? 0,

                // 🔥 FIX FECHAS (SIN HORA)
                FechaEntrada = fechaEntrada?.Date ?? DateTime.Today,
                FechaSalida = fechaSalida?.Date ?? DateTime.Today,

                PrecioInicial = habitacion.Precio ?? 0,

                // 🔥 FIX ADELANTO
                Adelanto = 0.00m,

                Clientes = await _context.Personas
                    .Where(p => p.IdTipoPersona == 3)
                    .ToListAsync()
            };

            return View(vm);
        }

        // ================================
        // GUARDAR RESERVA
        // ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RecepcionCreateVM vm)
        {
            var habitacion = await _context.Habitacions
                .FirstOrDefaultAsync(h => h.IdHabitacion == vm.IdHabitacion);

            if (habitacion == null)
                return NotFound();

            // VALIDAR FECHAS
            if (!vm.FechaEntrada.HasValue || !vm.FechaSalida.HasValue)
            {
                ModelState.AddModelError("", "Debe ingresar fechas.");
            }

            // 🔥 VALIDACIÓN SOLAPAMIENTO
            var conflicto = await _context.Recepcions
                .AnyAsync(r =>
                    r.IdHabitacion == vm.IdHabitacion &&
                    r.FechaEntrada != null &&
                    r.FechaSalida != null &&
                    vm.FechaEntrada.Value < r.FechaSalida.Value &&
                    vm.FechaSalida.Value > r.FechaEntrada.Value
                );

            if (conflicto)
            {
                ModelState.AddModelError("", "La habitación ya está reservada en esas fechas.");
            }

            if (vm.FechaEntrada > vm.FechaSalida)
            {
                ModelState.AddModelError("", "La fecha de salida no puede ser menor.");
            }

            var dias = (vm.FechaSalida.Value - vm.FechaEntrada.Value).Days;

            if (dias <= 0)
            {
                ModelState.AddModelError("", "Debe reservar al menos 1 noche.");
            }

            if (!ModelState.IsValid)
            {
                vm.Clientes = await _context.Personas
                    .Where(p => p.IdTipoPersona == 3)
                    .ToListAsync();

                return View(vm);
            }

            // ================= CLIENTE =================
            int idCliente;

            if (vm.IdClienteExistente.HasValue)
            {
                idCliente = vm.IdClienteExistente.Value;
            }
            else
            {
                var cliente = await _context.Personas
                    .FirstOrDefaultAsync(p => p.Documento == vm.Documento);

                if (cliente != null)
                {
                    idCliente = cliente.IdPersona;
                }
                else
                {
                    var nuevo = new Persona
                    {
                        TipoDocumento = vm.TipoDocumento,
                        Documento = vm.Documento,
                        Nombre = vm.Nombre,
                        Apellido = vm.Apellido,
                        Correo = vm.Correo,
                        IdTipoPersona = 3,
                        Estado = true,
                        FechaCreacion = DateTime.Now
                    };

                    _context.Personas.Add(nuevo);
                    await _context.SaveChangesAsync();

                    idCliente = nuevo.IdPersona;
                }
            }

            // ================= PRECIO =================
            decimal precioTotal = 0;

            for (var fecha = vm.FechaEntrada.Value; fecha < vm.FechaSalida.Value; fecha = fecha.AddDays(1))
            {
                var mult = ObtenerMultiplicador(fecha);
                precioTotal += (habitacion.Precio ?? 0) * mult;
            }

            var recepcion = new Recepcion
            {
                IdCliente = idCliente,
                IdHabitacion = vm.IdHabitacion,
                FechaEntrada = vm.FechaEntrada,
                FechaSalida = vm.FechaSalida,
                PrecioInicial = precioTotal,
                Adelanto = vm.Adelanto ?? 0,
                PrecioRestante = precioTotal - (vm.Adelanto ?? 0),
                TotalPagado = vm.Adelanto ?? 0,
                Observacion = vm.Observacion,
                Estado = true
            };

            _context.Recepcions.Add(recepcion);

            habitacion.IdEstadoHabitacion = 2;

            await _context.SaveChangesAsync();

            TempData["Ok"] = "Reserva registrada correctamente.";

            // 🔥 FIX FORMATO FECHAS EN REDIRECT
            return RedirectToAction(nameof(Index), new
            {
                fechaEntrada = vm.FechaEntrada.Value.ToString("yyyy-MM-dd"),
                fechaSalida = vm.FechaSalida.Value.ToString("yyyy-MM-dd")
            });
        }
    }
}