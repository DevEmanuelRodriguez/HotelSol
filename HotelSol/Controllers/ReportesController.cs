using ClosedXML.Excel;
using HotelSol.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace HotelSol.Controllers
{
    public class ReportesController : Controller
    {
        private readonly DbHotelContext _context;

        public ReportesController(DbHotelContext context)
        {
            _context = context;
        }

        // ============================
        // VISTAS
        // ============================
        public IActionResult Recepcion()
        {
            return View();
        }

        public IActionResult Productos()
        {
            return View();
        }

        // ============================
        // EXCEL RECEPCIONES
        // ============================
        [HttpGet]
        public async Task<IActionResult> ExportarRecepciones(string estado, DateTime? fechaInicio, DateTime? fechaFin)
        {
            var query = _context.Recepcions
                .Include(r => r.IdHabitacionNavigation)
                .Include(r => r.IdClienteNavigation)
                .AsQueryable();

            if (estado == "Activo")
            {
                query = query.Where(r => r.Estado == true);
            }
            else if (estado == "No Activo")
            {
                query = query.Where(r => r.Estado == false);
            }

            if (fechaInicio.HasValue)
            {
                query = query.Where(r => r.FechaEntrada != null && r.FechaEntrada.Value.Date >= fechaInicio.Value.Date);
            }

            if (fechaFin.HasValue)
            {
                query = query.Where(r => r.FechaEntrada != null && r.FechaEntrada.Value.Date <= fechaFin.Value.Date);
            }

            var lista = await query
                .OrderByDescending(r => r.IdRecepcion)
                .ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Recepciones");

            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "Habitación";
            ws.Cell(1, 3).Value = "Cliente";
            ws.Cell(1, 4).Value = "Documento";
            ws.Cell(1, 5).Value = "Fecha Entrada";
            ws.Cell(1, 6).Value = "Fecha Salida";
            ws.Cell(1, 7).Value = "Precio Inicial";
            ws.Cell(1, 8).Value = "Adelanto";
            ws.Cell(1, 9).Value = "Precio Restante";
            ws.Cell(1, 10).Value = "Total Pagado";
            ws.Cell(1, 11).Value = "Penalidad";
            ws.Cell(1, 12).Value = "Estado";

            var fila = 2;

            foreach (var r in lista)
            {
                ws.Cell(fila, 1).Value = r.IdRecepcion;
                ws.Cell(fila, 2).Value = r.IdHabitacionNavigation?.Numero ?? "";
                ws.Cell(fila, 3).Value = r.IdClienteNavigation == null
                    ? ""
                    : $"{r.IdClienteNavigation.Nombre} {r.IdClienteNavigation.Apellido}";
                ws.Cell(fila, 4).Value = r.IdClienteNavigation?.Documento ?? "";
                ws.Cell(fila, 5).Value = r.FechaEntrada?.ToString("dd/MM/yyyy");
                ws.Cell(fila, 6).Value = r.FechaSalida?.ToString("dd/MM/yyyy");
                ws.Cell(fila, 7).Value = r.PrecioInicial ?? 0;
                ws.Cell(fila, 8).Value = r.Adelanto ?? 0;
                ws.Cell(fila, 9).Value = r.PrecioRestante ?? 0;
                ws.Cell(fila, 10).Value = r.TotalPagado ?? 0;
                ws.Cell(fila, 11).Value = r.CostoPenalidad ?? 0;
                ws.Cell(fila, 12).Value = r.Estado == true ? "Activo" : "No Activo";
                fila++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            var contenido = stream.ToArray();

            return File(
                contenido,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Reporte_Recepciones.xlsx"
            );
        }

        // ============================
        // EXCEL PRODUCTOS
        // ============================
        [HttpGet]
        public async Task<IActionResult> ExportarProductos(string estado, DateTime? fechaInicio, DateTime? fechaFin)
        {
            var query = _context.Productos.AsQueryable();

            if (estado == "Activo")
            {
                query = query.Where(p => p.Estado == true);
            }
            else if (estado == "No Activo")
            {
                query = query.Where(p => p.Estado == false);
            }

            if (fechaInicio.HasValue)
            {
                query = query.Where(p => p.FechaCreacion != null && p.FechaCreacion.Value.Date >= fechaInicio.Value.Date);
            }

            if (fechaFin.HasValue)
            {
                query = query.Where(p => p.FechaCreacion != null && p.FechaCreacion.Value.Date <= fechaFin.Value.Date);
            }

            var lista = await query
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Productos");

            ws.Cell(1, 1).Value = "Id";
            ws.Cell(1, 2).Value = "Nombre";
            ws.Cell(1, 3).Value = "Detalle";
            ws.Cell(1, 4).Value = "Precio";
            ws.Cell(1, 5).Value = "Cantidad";
            ws.Cell(1, 6).Value = "Estado";
            ws.Cell(1, 7).Value = "Fecha Creación";

            var fila = 2;

            foreach (var p in lista)
            {
                ws.Cell(fila, 1).Value = p.IdProducto;
                ws.Cell(fila, 2).Value = p.Nombre ?? "";
                ws.Cell(fila, 3).Value = p.Detalle ?? "";
                ws.Cell(fila, 4).Value = p.Precio ?? 0;
                ws.Cell(fila, 5).Value = p.Cantidad ?? 0;
                ws.Cell(fila, 6).Value = p.Estado == true ? "Activo" : "No Activo";
                ws.Cell(fila, 7).Value = p.FechaCreacion?.ToString("dd/MM/yyyy");
                fila++;
            }

            ws.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            wb.SaveAs(stream);
            var contenido = stream.ToArray();

            return File(
                contenido,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Reporte_Productos.xlsx"
            );
        }
    }
}