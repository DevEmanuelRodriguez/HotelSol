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
    /*
    public async Task<IActionResult> Exportar()
    {
        await _xml.ExportarAsync();
        ViewBag.Mensaje = "XML generado correctamente";
        return View();
    }*/

    // pantalla inicial
    public IActionResult Exportar()
    {
        return View();
    }

    // exportar tabla seleccionada
    [HttpPost]
    public async Task<IActionResult> ExportarTabla(string tabla)
    {
        await _xml.ExportarTablaAsync(tabla);
        ViewBag.Mensaje = $"XML de {tabla} generado correctamente";
        return View("Exportar");
    }

    // leer XML (el simple)
    public IActionResult Leer()
    {
        var datos = _xml.Leer();
        return View(datos);
    }
}
