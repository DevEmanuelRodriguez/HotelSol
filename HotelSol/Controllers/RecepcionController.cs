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

        // CAMBIO: función para calcular multiplicador de temporada
        private decimal ObtenerMultiplicador(DateTime fecha)
        {
            // TEMPORADA ALTA (junio-agosto)
            if (fecha.Month >= 6 && fecha.Month <= 8)
                return 1.5m;

            // TEMPORADA BAJA (enero-febrero)
            if (fecha.Month == 1 || fecha.Month == 2)
                return 0.8m;

            // TEMPORADA MEDIA
            return 1m;
        }


        // LISTA DE HABITACIONES

        public async Task<IActionResult> Index(int? pisoId)
        {
            var habitacionesQuery = _context.Habitacions
                .Include(h => h.IdCategoriaNavigation)
                .Include(h => h.IdPisoNavigation)
                .Include(h => h.IdEstadoHabitacionNavigation)
                .AsQueryable();

            // FILTRO CORREGIDO
            if (pisoId.HasValue && pisoId.Value > 0)
            {
                habitacionesQuery = habitacionesQuery
                    .Where(h => h.IdPiso == pisoId.Value);
            }

            var habitaciones = await habitacionesQuery.ToListAsync();

            // HABITACIONES OCUPADAS
            var habitacionesActivas = await _context.Recepcions
                .Where(r => r.FechaSalida == null)
                .Select(r => r.IdHabitacion)
                .Distinct()
                .ToListAsync();

            var vm = new RecepcionIndexVM
            {
                PisoSeleccionado = pisoId,

                Pisos = await _context.Pisos
                    .Where(p => p.Estado == true)
                    .OrderBy(p => p.IdPiso)
                    .ToListAsync(),

                Habitaciones = habitaciones.Select(h =>
                {
                    var ocupada = habitacionesActivas.Contains(h.IdHabitacion);
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
                        PuedeReservar = puedeReservar
                    };
                }).ToList()
            };

            return View(vm);
        }


        // FORMULARIO DE RESERVA

        public async Task<IActionResult> Create(int id)
        {
            var habitacion = await _context.Habitacions
                .Include(h => h.IdCategoriaNavigation)
                .Include(h => h.IdPisoNavigation)
                .FirstOrDefaultAsync(h => h.IdHabitacion == id);

            if (habitacion == null)
                return NotFound();

            // VALIDAR DISPONIBILIDAD
            var ocupada = await _context.Recepcions
                .AnyAsync(r => r.IdHabitacion == id && r.FechaSalida == null);

            if (ocupada || habitacion.IdEstadoHabitacion == 3)
            {
                TempData["Error"] = "La habitación no está disponible.";
                return RedirectToAction(nameof(Index));
            }

            var vm = new RecepcionCreateVM
            {
                IdHabitacion = habitacion.IdHabitacion,
                NumeroHabitacion = habitacion.Numero ?? "",
                Categoria = habitacion.IdCategoriaNavigation?.Descripcion ?? "",
                Piso = habitacion.IdPisoNavigation?.Descripcion ?? "",
                DetalleHabitacion = habitacion.Detalle ?? "",
                PrecioHabitacion = habitacion.Precio ?? 0,
                FechaEntrada = DateTime.Today,
                FechaSalida = DateTime.Today,
                PrecioInicial = habitacion.Precio ?? 0,
                Adelanto = 0,

                Clientes = await _context.Personas
                    .Where(p => p.IdTipoPersona == 3)
                    .ToListAsync()
            };

            return View(vm);
        }


        // BUSCAR CLIENTE

        [HttpGet]
        public async Task<IActionResult> BuscarCliente(string documento)
        {
            if (string.IsNullOrWhiteSpace(documento))
                return Json(null);

            var cliente = await _context.Personas
                .Where(p => p.IdTipoPersona == 3 && p.Documento == documento)
                .Select(p => new
                {
                    idPersona = p.IdPersona,
                    tipoDocumento = p.TipoDocumento,
                    documento = p.Documento,
                    nombre = p.Nombre,
                    apellido = p.Apellido,
                    correo = p.Correo
                })
                .FirstOrDefaultAsync();

            return Json(cliente);
        }


        // GUARDAR RESERVA

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RecepcionCreateVM vm)
        {
            var habitacion = await _context.Habitacions
                .FirstOrDefaultAsync(h => h.IdHabitacion == vm.IdHabitacion);

            if (habitacion == null)
                return NotFound();

            // VALIDAR SI YA ESTÁ OCUPADA
            var ocupada = await _context.Recepcions
                .AnyAsync(r => r.IdHabitacion == vm.IdHabitacion && r.FechaSalida == null);

            if (ocupada || habitacion.IdEstadoHabitacion == 3)
            {
                ModelState.AddModelError("", "La habitación ya no está disponible.");
            }

            // VALIDAR FECHAS
            if (vm.FechaEntrada > vm.FechaSalida)
            {
                ModelState.AddModelError("", "La fecha de salida no puede ser menor que la de entrada.");
            }

            // CAMBIO: calcular número de noches
            var dias = (vm.FechaSalida.Value - vm.FechaEntrada.Value).Days;

            // CAMBIO: validar mínimo 1 noche
            if (dias <= 0)
            {
                ModelState.AddModelError("", "Debe reservar al menos 1 noche.");
            }

            // SI HAY ERRORES
            if (!ModelState.IsValid)
            {
                vm.NumeroHabitacion = habitacion.Numero ?? "";
                vm.DetalleHabitacion = habitacion.Detalle ?? "";
                vm.PrecioHabitacion = habitacion.Precio ?? 0;

                var categoria = await _context.Categoria
                    .FirstOrDefaultAsync(c => c.IdCategoria == habitacion.IdCategoria);

                var piso = await _context.Pisos
                    .FirstOrDefaultAsync(p => p.IdPiso == habitacion.IdPiso);

                vm.Categoria = categoria?.Descripcion ?? "";
                vm.Piso = piso?.Descripcion ?? "";

                vm.Clientes = await _context.Personas
                    .Where(p => p.IdTipoPersona == 3)
                    .ToListAsync();

                return View(vm);
            }

            int idCliente;

            if (vm.IdClienteExistente.HasValue)
            {
                idCliente = vm.IdClienteExistente.Value;
            }
            else
            {
                var clienteExistente = await _context.Personas
                    .FirstOrDefaultAsync(p => p.Documento == vm.Documento && p.IdTipoPersona == 3);

                if (clienteExistente != null)
                {
                    idCliente = clienteExistente.IdPersona;
                }
                else
                {
                    var nuevoCliente = new Persona
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

                    _context.Personas.Add(nuevoCliente);
                    await _context.SaveChangesAsync();

                    idCliente = nuevoCliente.IdPersona;
                }
            }

            // CAMBIO: calcular precio por día con temporada
            decimal precioTotal = 0;

            for (var fecha = vm.FechaEntrada.Value; fecha < vm.FechaSalida.Value; fecha = fecha.AddDays(1))
            {
                var multiplicador = ObtenerMultiplicador(fecha);
                precioTotal += (habitacion.Precio ?? 0) * multiplicador;
            }

            // CREAR RECEPCIÓN
            var recepcion = new Recepcion
            {
                IdCliente = idCliente,
                IdHabitacion = vm.IdHabitacion,
                FechaEntrada = vm.FechaEntrada,
                FechaSalida = null,

                PrecioInicial = precioTotal,
                Adelanto = vm.Adelanto ?? 0,
                PrecioRestante = precioTotal - (vm.Adelanto ?? 0),
                TotalPagado = vm.Adelanto ?? 0,
                Observacion = vm.Observacion,
                Estado = true
            };

            _context.Recepcions.Add(recepcion);

            // ACTUALIZAR ESTADO A OCUPADO
            habitacion.IdEstadoHabitacion = 2;

            await _context.SaveChangesAsync();

            TempData["Ok"] = "Reserva registrada correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}