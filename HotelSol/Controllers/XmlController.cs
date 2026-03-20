using Microsoft.AspNetCore.Mvc;
using HotelSol.Services;

namespace HotelSol.Controllers;

public class XmlController : Controller
{
    private readonly XmlService _xml;

    public XmlController(XmlService xml)
    {
        _xml = xml;
    }

    public async Task<IActionResult> Exportar()
    {
        await _xml.ExportarAsync();
        ViewBag.Mensaje = "XML generado correctamente";
        return View();
    }

    public IActionResult Leer()
    {
        var datos = _xml.Leer();
        return View(datos);
    }
}
