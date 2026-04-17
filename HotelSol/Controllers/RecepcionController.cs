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

            // 🔥 CLAVE: SI NO HAY FECHAS → USAR HOY
            var fechaInicio = fechaEntrada ?? DateTime.Today;
            var fechaFin = fechaSalida ?? DateTime.Today.AddDays(1);

            // 🔥 OCUPACIÓN REAL
            var habitacionesOcupadas = await _context.Recepcions
                .Where(r =>
                    r.IdHabitacion != null &&
                    r.FechaEntrada != null &&
                    r.FechaSalida != null &&

                    // 🔥 SOLAPAMIENTO REAL
                    fechaInicio < r.FechaSalida.Value &&
                    fechaFin > r.FechaEntrada.Value
                )
                .Select(r => r.IdHabitacion)
                .Distinct()
                .ToListAsync();

            var vm = new RecepcionIndexVM
            {
                PisoSeleccionado = pisoId,

                // 🔥 IMPORTANTE: enviar fechas a la vista
                FechaEntrada = fechaInicio,
                FechaSalida = fechaFin,

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

                        // 🔥 PARA LA VISTA
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

        //DETALLE DE RESERVA
        public async Task<IActionResult> Detalle(int idHabitacion)
        {
            var habitacion = await _context.Habitacions
                .Include(h => h.IdCategoriaNavigation)
                .Include(h => h.IdPisoNavigation)
                .FirstOrDefaultAsync(h => h.IdHabitacion == idHabitacion);

            if (habitacion == null)
                return NotFound();

            // Recepción activa de esa habitación
            var recepcion = await _context.Recepcions
                .FirstOrDefaultAsync(r =>
                    r.IdHabitacion == idHabitacion &&
                    r.FechaEntrada != null &&
                    r.FechaSalida != null &&
                    DateTime.Today >= r.FechaEntrada.Value.Date &&
                    DateTime.Today < r.FechaSalida.Value.Date);

            if (recepcion == null)
            {
                TempData["Error"] = "La habitación no tiene una recepción activa.";
                return RedirectToAction(nameof(Index));
            }

            var cliente = await _context.Personas
                .FirstOrDefaultAsync(p => p.IdPersona == recepcion.IdCliente);

            // PRODUCTOS COMPRADOS EN TIENDA
            // Aquí supongo esta relación:
            // VENTA -> DETALLEVENTA -> PRODUCTO
            // y que Venta tiene relación con la recepción o con cliente/habitación.
            // Si todavía no tienes esa relación hecha, de momento deja esta lista vacía.
            //var productos = new List<RecepcionDetalleProductoVM>();
            var productos = await _context.Venta
            .Where(v => v.IdRecepcion == recepcion.IdRecepcion)
            .Include(v => v.DetalleVenta)
            .ThenInclude(d => d.IdProductoNavigation)
            .SelectMany(v => v.DetalleVenta.Select(d => new RecepcionDetalleProductoVM
            {
                Producto = d.IdProductoNavigation!.Nombre!,
                Cantidad = d.Cantidad ?? 0,
                PrecioUnitario = d.IdProductoNavigation.Precio ?? 0,
                EstadoVenta = v.Estado ?? "",
                SubTotal = d.SubTotal ?? 0
             }))
    .ToListAsync();

            var vm = new RecepcionDetalleVM
            {
                IdHabitacion = habitacion.IdHabitacion,
                NumeroHabitacion = habitacion.Numero ?? "",
                DetalleHabitacion = habitacion.Detalle ?? "",
                Categoria = habitacion.IdCategoriaNavigation?.Descripcion ?? "",
                Piso = habitacion.IdPisoNavigation?.Descripcion ?? "",

                IdRecepcion = recepcion.IdRecepcion,
                IdCliente = cliente?.IdPersona ?? 0,
                Cliente = cliente == null ? "" : $"{cliente.Nombre} {cliente.Apellido}",
                Documento = cliente?.Documento ?? "",
                Correo = cliente?.Correo ?? "",

                FechaEntrada = recepcion.FechaEntrada,
                FechaSalida = recepcion.FechaSalida,
                CostoHabitacion = recepcion.PrecioInicial ?? 0,
                CantidadAdelantado = recepcion.Adelanto ?? 0,
                CantidadRestante = recepcion.PrecioRestante ?? 0,

                Productos = productos
            };

            return View(vm);
        }

        // ================================
        // SALIDAS (TARJETAS DE HABITACIONES OCUPADAS)
        // ================================
        public async Task<IActionResult> Salidas(int? pisoId)
        {
            var hoy = DateTime.Today;

            var habitacionesQuery = _context.Habitacions
                .Include(h => h.IdCategoriaNavigation)
                .Include(h => h.IdPisoNavigation)
                .Include(h => h.IdEstadoHabitacionNavigation)
                .AsQueryable();

            if (pisoId.HasValue && pisoId.Value > 0)
            {
                habitacionesQuery = habitacionesQuery.Where(h => h.IdPiso == pisoId.Value);
            }

            var habitaciones = await habitacionesQuery.ToListAsync();

            var habitacionesOcupadas = habitaciones
                .Where(h => _context.Recepcions.Any(r =>
                    r.IdHabitacion == h.IdHabitacion &&
                    r.FechaEntrada != null &&
                    r.FechaSalida != null &&
                    hoy >= r.FechaEntrada.Value.Date &&
                    hoy < r.FechaSalida.Value.Date))
                .ToList();

            ViewBag.Pisos = await _context.Pisos
                .Where(p => p.Estado == true)
                .OrderBy(p => p.IdPiso)
                .ToListAsync();

            ViewBag.PisoSeleccionado = pisoId;

            return View(habitacionesOcupadas);
        }

        // ================================
        // CHECKOUT FORM
        // ================================
        public async Task<IActionResult> Checkout(int idHabitacion)
        {
            var hoy = DateTime.Today;

            var habitacion = await _context.Habitacions
                .Include(h => h.IdCategoriaNavigation)
                .Include(h => h.IdPisoNavigation)
                .FirstOrDefaultAsync(h => h.IdHabitacion == idHabitacion);

            if (habitacion == null)
                return NotFound();

            var recepcion = await _context.Recepcions
                .FirstOrDefaultAsync(r =>
                    r.IdHabitacion == idHabitacion &&
                    r.FechaEntrada != null &&
                    r.FechaSalida != null &&
                    hoy >= r.FechaEntrada.Value.Date &&
                    hoy < r.FechaSalida.Value.Date);

            if (recepcion == null)
            {
                TempData["Error"] = "La habitación no tiene una recepción activa.";
                return RedirectToAction(nameof(Salidas));
            }

            var cliente = await _context.Personas
                .FirstOrDefaultAsync(p => p.IdPersona == recepcion.IdCliente);

            var productos = await _context.Venta
                .Where(v => v.IdRecepcion == recepcion.IdRecepcion)
                .Include(v => v.DetalleVenta)
                    .ThenInclude(d => d.IdProductoNavigation)
                .SelectMany(v => v.DetalleVenta.Select(d => new RecepcionDetalleProductoVM
                {
                    Producto = d.IdProductoNavigation != null ? d.IdProductoNavigation.Nombre ?? "" : "",
                    Cantidad = d.Cantidad ?? 0,
                    //usamos ternaria
                    PrecioUnitario = d.IdProductoNavigation != null
                    ? (d.IdProductoNavigation.Precio ?? 0) : 0,
                    EstadoVenta = v.Estado ?? "",
                    SubTotal = d.SubTotal ?? 0
                }))
                .ToListAsync();

            var totalConsumos = productos.Sum(x => x.SubTotal);
            var costoHabitacion = recepcion.PrecioInicial ?? 0;
            var adelanto = recepcion.Adelanto ?? 0;
            var restante = recepcion.PrecioRestante ?? 0;

            var vm = new CheckoutVM
            {
                IdRecepcion = recepcion.IdRecepcion,
                IdHabitacion = habitacion.IdHabitacion,

                NumeroHabitacion = habitacion.Numero ?? "",
                DetalleHabitacion = habitacion.Detalle ?? "",
                Categoria = habitacion.IdCategoriaNavigation?.Descripcion ?? "",
                Piso = habitacion.IdPisoNavigation?.Descripcion ?? "",

                Cliente = cliente == null ? "" : $"{cliente.Nombre} {cliente.Apellido}",
                Documento = cliente?.Documento ?? "",
                Correo = cliente?.Correo ?? "",

                FechaEntrada = recepcion.FechaEntrada,
                FechaSalida = recepcion.FechaSalida,

                CostoHabitacion = costoHabitacion,
                Adelanto = adelanto,
                CantidadRestante = restante,
                Penalidad = recepcion.CostoPenalidad ?? 0,

                TotalConsumos = totalConsumos,
                TotalPagar = restante + totalConsumos + (recepcion.CostoPenalidad ?? 0),

                Productos = productos
            };

            return View(vm);
        }

        // ================================
        // CONFIRMAR CHECKOUT
        // ================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmarCheckout(int IdRecepcion, decimal Penalidad)
        {
            var recepcion = await _context.Recepcions
                .Include(r => r.IdHabitacionNavigation)
                .FirstOrDefaultAsync(r => r.IdRecepcion == IdRecepcion);

            if (recepcion == null)
                return NotFound();

            var totalConsumos = await _context.Venta
                .Where(v => v.IdRecepcion == IdRecepcion)
                .SumAsync(v => (decimal?)v.Total) ?? 0;

            var restante = recepcion.PrecioRestante ?? 0;
            var totalFinal = restante + totalConsumos + Penalidad;

            recepcion.CostoPenalidad = Penalidad;
            recepcion.TotalPagado = (recepcion.TotalPagado ?? 0) + totalFinal;
            recepcion.PrecioRestante = 0;
            recepcion.FechaSalidaConfirmacion = DateTime.Now;
            recepcion.Estado = false;

            if (recepcion.IdHabitacionNavigation != null)
            {
                recepcion.IdHabitacionNavigation.IdEstadoHabitacion = 1; // disponible
            }

            await _context.SaveChangesAsync();

            TempData["Ok"] = "Recepción finalizada correctamente.";
            return RedirectToAction(nameof(Salidas));
        }

    }
}