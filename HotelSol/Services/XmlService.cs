using System.Xml.Linq;
using HotelSol.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelSol.Services;

public class XmlService
{
    private readonly DbHotelContext _context;
    private readonly IWebHostEnvironment _env;

    public XmlService(DbHotelContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // Para realizar prueba unitaria
    /*
    public async Task ExportarAsync()
    {
        var habitaciones = await _context.Habitacions.ToListAsync();

        var xml = new XDocument(
            new XElement("Habitaciones",
                habitaciones.Select(h =>
                    new XElement("Habitacion",
                        new XElement("Numero", h.Numero ?? ""),
                        new XElement("Precio", h.Precio ?? 0)
                    )
                )
            )
        );

        var ruta = Path.Combine(_env.WebRootPath, "habitaciones.xml");
        xml.Save(ruta);
    }*/

    // exportar cualquier tabla
    public async Task ExportarTablaAsync(string tabla)
    {
        var ruta = Path.Combine(_env.WebRootPath, $"{tabla}.xml");

        XDocument xml;

        switch (tabla)
        {
            case "Categoria":
                var categorias = await _context.Categoria.ToListAsync();
                xml = new XDocument(
                    new XElement("Categorias",
                        categorias.Select(c =>
                            new XElement("Categoria",
                                new XElement("Id", c.IdCategoria),
                                new XElement("Descripcion", c.Descripcion ?? "")
                            )
                        )
                    )
                );
                break;

            case "Piso":
                var pisos = await _context.Pisos.ToListAsync();
                xml = new XDocument(
                    new XElement("Pisos",
                        pisos.Select(p =>
                            new XElement("Piso",
                                new XElement("Id", p.IdPiso),
                                new XElement("Descripcion", p.Descripcion ?? "")
                            )
                        )
                    )
                );
                break;

            case "EstadoHabitacion":
                var estados = await _context.EstadoHabitacions.ToListAsync();
                xml = new XDocument(
                    new XElement("EstadosHabitacion",
                        estados.Select(e =>
                            new XElement("EstadoHabitacion",
                                new XElement("Id", e.IdEstadoHabitacion),
                                new XElement("Descripcion", e.Descripcion ?? "")
                            )
                        )
                    )
                );
                break;

            case "TipoPersona":
                var tipos = await _context.TipoPersonas.ToListAsync();
                xml = new XDocument(
                    new XElement("TiposPersona",
                        tipos.Select(t =>
                            new XElement("TipoPersona",
                                new XElement("Id", t.IdTipoPersona),
                                new XElement("Descripcion", t.Descripcion ?? "")
                            )
                        )
                    )
                );
                break;

            case "Producto":
                var productos = await _context.Productos.ToListAsync();
                xml = new XDocument(
                    new XElement("Productos",
                        productos.Select(p =>
                            new XElement("Producto",
                                new XElement("Id", p.IdProducto),
                                new XElement("Nombre", p.Nombre ?? ""),
                                new XElement("Precio", p.Precio ?? 0)
                            )
                        )
                    )
                );
                break;

            case "Persona":
                var personas = await _context.Personas.ToListAsync();
                xml = new XDocument(
                    new XElement("Personas",
                        personas.Select(p =>
                            new XElement("Persona",
                                new XElement("Id", p.IdPersona),
                                new XElement("Nombre", p.Nombre ?? "")
                            )
                        )
                    )
                );
                break;

            case "Habitacion":
                var habitaciones = await _context.Habitacions.ToListAsync();
                xml = new XDocument(
                    new XElement("Habitaciones",
                        habitaciones.Select(h =>
                            new XElement("Habitacion",
                                new XElement("Id", h.IdHabitacion),
                                new XElement("Numero", h.Numero ?? ""),
                                new XElement("Precio", h.Precio ?? 0)
                            )
                        )
                    )
                );
                break;

            case "Recepcion":
                var recepciones = await _context.Recepcions.ToListAsync();
                xml = new XDocument(
                    new XElement("Recepciones",
                        recepciones.Select(r =>
                            new XElement("Recepcion",
                                new XElement("Id", r.IdRecepcion),
                                new XElement("FechaEntrada", r.FechaEntrada),
                                new XElement("FechaSalida", r.FechaSalida)
                            )
                        )
                    )
                );
                break;

            case "Venta":
                var ventas = await _context.Venta.ToListAsync();
                xml = new XDocument(
                    new XElement("Ventas",
                        ventas.Select(v =>
                            new XElement("Venta",
                                new XElement("Id", v.IdVenta),
                                new XElement("Total", v.Total)
                            )
                        )
                    )
                );
                break;

            case "DetalleVenta":
                var detalles = await _context.DetalleVenta.ToListAsync();
                xml = new XDocument(
                    new XElement("DetallesVenta",
                        detalles.Select(d =>
                            new XElement("DetalleVenta",
                                new XElement("Id", d.IdDetalleVenta),
                                new XElement("Producto", d.IdProducto),
                                new XElement("Cantidad", d.Cantidad),
                                new XElement("SubTotal", d.SubTotal)
                            )
                        )
                    )
                );
                break;

            default:
                throw new Exception("Tabla no válida");
        }

        xml.Save(ruta);
    }

    // leer XML 
    public List<string> Leer()
    {
        var ruta = Path.Combine(_env.WebRootPath, "habitaciones.xml");

        if (!File.Exists(ruta))
            return new List<string>();

        var xml = XDocument.Load(ruta);

        return xml.Descendants("Habitacion")
                  .Select(x => (string?)x.Element("Numero") ?? "")
                  .ToList();
    }
}