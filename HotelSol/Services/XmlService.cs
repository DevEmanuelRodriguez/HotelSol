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
    }

    public List<string> Leer()
    {
        var ruta = Path.Combine(_env.WebRootPath, "habitaciones.xml");

        if (!File.Exists(ruta))
            return new List<string>();

        var xml = XDocument.Load(ruta);

        //si es null, pone vacío ""
        return xml.Descendants("Habitacion")
          .Select(x => (string?)x.Element("Numero") ?? "")
          .ToList();
    }
}