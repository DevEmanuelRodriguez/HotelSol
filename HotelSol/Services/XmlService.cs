/*
 XmlService contiene toda la lógica relacionada con XML para exportar e importar datos, se procesa XML usando LINQ to XML
*/
using System.Xml.Linq;
using HotelSol.Data;
using Microsoft.EntityFrameworkCore;

namespace HotelSol.Services;

public class XmlService
{   // Acceso a la base de datos
    private readonly DbHotelContext _context;
    //Permite acceder a wwwroot para guardar los XML exportados
    private readonly IWebHostEnvironment _env;

    public XmlService(DbHotelContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    // Para exportar cualquier tabla
    public async Task ExportarTabla(string tabla)
    {
        //ruta donde se guardará el XML
        var ruta = Path.Combine(_env.WebRootPath, $"{tabla}.xml");

        XDocument xml;

        //switch para gestionar la tabla seleccionada
        switch (tabla)
        {
            //de cada clase podemos selccionar aquellos atributos que nos interesen 
            case "Categoria":
                //await para esperar a que termine la operacion sin bloquear la aplicación
                var categorias = await _context.Categoria.ToListAsync();
                xml = new XDocument(
                    new XElement("Categorias",//nodo raiz 
                        categorias.Select(c =>
                            new XElement("Categoria",//nodo interno
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
                                new XElement("Precio", p.Precio ?? 0),
                                new XElement("Cantidad", p.Cantidad ?? 0)
                            )
                        )
                    )
                );
                break;

            case "Persona":
                var personas = await _context.Personas
                    .Where(p => p.IdTipoPersona == 3)
                    .ToListAsync();

                xml = new XDocument(
                    new XElement("Clientes",
                        personas.Select(p =>
                            new XElement("Cliente",
                                new XElement("Id", p.IdPersona),
                                new XElement("Nombre", p.Nombre ?? ""),
                                new XElement("Apellido", p.Apellido ?? ""),
                                new XElement("Documento", p.Documento ?? ""),
                                new XElement("Correo", p.Correo ?? "")
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
                                new XElement("Detalle", h.Detalle ?? ""),
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
    public async Task ImportarDesdeArchivo(IFormFile archivo)
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
                    var nombre = (string?)nodo.Element("Nombre") ?? "";
                    var detalle = (string?)nodo.Element("Detalle") ?? "";
                    var precio = (decimal?)nodo.Element("Precio") ?? 0;
                    var cantidad = (int?)nodo.Element("Cantidad") ?? 0;

                    var producto = await _context.Productos
                        .FirstOrDefaultAsync(p =>
                            p.Nombre != null &&
                            p.Nombre.Trim().ToUpper() == nombre.Trim().ToUpper());

                    if (producto == null)
                    {
                        _context.Productos.Add(new Models.Producto
                        {
                            Nombre = nombre,
                            Detalle = detalle,
                            Precio = precio,
                            Cantidad = cantidad,
                            Estado = true,
                            FechaCreacion = DateTime.Now
                        });
                    }
                    else
                    {
                        producto.Detalle = detalle;
                        producto.Precio = precio;

                        // NO tocar cantidad
                    }
                }

                break;

            case "Clientes":
                foreach (var nodo in xml.Descendants("Cliente"))
                {
                    var documento = (string?)nodo.Element("Documento") ?? "";
                    var correo = (string?)nodo.Element("Correo") ?? "";
                    var nombre = (string?)nodo.Element("Nombre") ?? "";
                    var apellido = (string?)nodo.Element("Apellido") ?? "";

                    // Bloquear admin por seguridad extra
                    if ((nombre + " " + apellido).ToUpper().Contains("ADMIN"))
                        continue;

                    var existe = await _context.Personas.AnyAsync(p =>
                        (p.Documento ?? "") == documento ||
                        (correo != "" && (p.Correo ?? "") == correo));

                    if (!existe)
                    {
                        _context.Personas.Add(new Models.Persona
                        {
                            TipoDocumento = "DNI",
                            Nombre = nombre,
                            Apellido = apellido,
                            Documento = documento,
                            Correo = correo,
                            IdTipoPersona = 3,
                            Estado = true,
                            FechaCreacion = DateTime.Now
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
                            Detalle = (string?)nodo.Element("Detalle") ?? "",
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

        // guardamos cambios en la DB
        await _context.SaveChangesAsync();
    }



    
}