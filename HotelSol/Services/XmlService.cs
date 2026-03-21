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
                                new XElement("Detalle", p.Detalle ?? ""),
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
                                new XElement("Nombre", p.Nombre ?? ""),
                                new XElement("Documento", p.Documento ?? "")
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

    //Para importar XML con nuevo registro a DB
    public async Task ImportarDesdeArchivoAsync(IFormFile archivo)
    {
        // Validación 
        if (archivo == null || archivo.Length == 0)
            return;

        using var stream = archivo.OpenReadStream();

        var xml = XDocument.Load(stream);

        // Detectamos el tipo por nodo raiz del XML
        var root = xml.Root?.Name.LocalName;

        switch (root)
        {
            case "Categorias":
                foreach (var nodo in xml.Descendants("Categoria"))
                {
                    var descripcion = (string?)nodo.Element("Descripcion") ?? "";

                    // comprobar si ya existe
                    bool existe = await _context.Categoria
                        .AnyAsync(c => c.Descripcion == descripcion);

                    if (!existe)
                    {
                        _context.Categoria.Add(new Models.Categorium
                        {
                            Descripcion = descripcion
                        });
                    }
                }
                break;

            case "Pisos":
                foreach (var nodo in xml.Descendants("Piso"))
                {
                    var descripcion = (string?)nodo.Element("Descripcion") ?? "";

                    bool existe = await _context.Pisos
                        .AnyAsync(p => p.Descripcion == descripcion);

                    if (!existe)
                    {
                        _context.Pisos.Add(new Models.Piso
                        {
                            Descripcion = descripcion
                        });
                    }
                }
                break;

            case "EstadosHabitacion":
                foreach (var nodo in xml.Descendants("EstadoHabitacion"))
                {
                    var descripcion = (string?)nodo.Element("Descripcion") ?? "";

                    bool existe = await _context.EstadoHabitacions
                        .AnyAsync(e => e.Descripcion == descripcion);

                    if (!existe)
                    {
                        _context.EstadoHabitacions.Add(new Models.EstadoHabitacion
                        {
                            Descripcion = descripcion
                        });
                    }
                }
                break;

            case "TiposPersona":
                foreach (var nodo in xml.Descendants("TipoPersona"))
                {
                    var descripcion = (string?)nodo.Element("Descripcion") ?? "";

                    bool existe = await _context.TipoPersonas
                        .AnyAsync(t => t.Descripcion == descripcion);

                    if (!existe)
                    {
                        _context.TipoPersonas.Add(new Models.TipoPersona
                        {
                            Descripcion = descripcion
                        });
                    }
                }
                break;

            case "Productos":
                foreach (var nodo in xml.Descendants("Producto"))
                {
                    // leer datos del XML
                    var nombre = (string?)nodo.Element("Nombre") ?? "";
                    var detalle = (string?)nodo.Element("Detalle") ?? "";
                    var precio = (decimal?)nodo.Element("Precio") ?? 0;

                    // comprobar duplicado por combinación
                    bool existe = await _context.Productos
                        .AnyAsync(p => p.Nombre == nombre && p.Detalle == detalle);

                    // insertar solo si no existe producto con nombre y detalle iguales
                    if (!existe)
                    {
                        _context.Productos.Add(new Models.Producto
                        {
                            Nombre = nombre,
                            Detalle = detalle,
                            Precio = precio
                        });
                    }
                }
                break;

            case "Personas":
                foreach (var nodo in xml.Descendants("Persona"))
                {
                    var documento = (string?)nodo.Element("Documento") ?? "";

                    bool existe = await _context.Personas
                        .AnyAsync(p => p.Documento == documento);

                    if (!existe)
                    {
                        _context.Personas.Add(new Models.Persona
                        {
                            Nombre = (string?)nodo.Element("Nombre") ?? "",
                            Documento = documento
                        });
                    }
                }
                break;

            case "Habitaciones":
                foreach (var nodo in xml.Descendants("Habitacion"))
                {
                    var numero = (string?)nodo.Element("Numero") ?? "";

                    // evitamos duplicados
                    bool existe = await _context.Habitacions
                        .AnyAsync(h => h.Numero == numero);

                    //si no existe agragmos
                    if (!existe)
                    {
                        _context.Habitacions.Add(new Models.Habitacion
                        {
                            Numero = numero,
                            Precio = (decimal?)nodo.Element("Precio") ?? 0
                        });
                    }
                }
                break;

            case "Recepciones":
                foreach (var nodo in xml.Descendants("Recepcion"))
                {
                    _context.Recepcions.Add(new Models.Recepcion
                    {
                        FechaEntrada = (DateTime?)nodo.Element("FechaEntrada"),
                        FechaSalida = (DateTime?)nodo.Element("FechaSalida")
                    });
                }
                break;

            case "Ventas":
                foreach (var nodo in xml.Descendants("Venta"))
                {
                    _context.Venta.Add(new Models.Ventum
                    {
                        Total = (decimal?)nodo.Element("Total") ?? 0
                    });
                }
                break;

            case "DetallesVenta":
                foreach (var nodo in xml.Descendants("DetalleVenta"))
                {
                    _context.DetalleVenta.Add(new Models.DetalleVentum
                    {
                        IdProducto = (int?)nodo.Element("Producto"),
                        Cantidad = (int?)nodo.Element("Cantidad") ?? 0,
                        SubTotal = (decimal?)nodo.Element("SubTotal") ?? 0
                    });
                }
                break;

            default:
                throw new Exception("XML no reconocido");
        }

        // guardamos todo
        await _context.SaveChangesAsync();
    }



    // leer XML EN DESARROLLO >>>>NO FUNCIONA<<<<< 
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