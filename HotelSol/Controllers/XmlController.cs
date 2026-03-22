/*
 Este controller gestiona:
 - Exportar datos desde la base de datos a XML
 - Importar datos desde un archivo XML a la base de datos

 Utiliza un servicio (XmlService).
*/

using Microsoft.AspNetCore.Mvc;
using HotelSol.Services;//para usar XmlService

namespace HotelSol.Controllers;

public class XmlController : Controller
{
    // Servicio que contiene la lógica XML
    private readonly XmlService _xml;

    public XmlController(XmlService xml)
    {
        _xml = xml;
    }

    // Pantalla inicial de exportación
    public IActionResult Exportar()
    {
        return View();
    }

    // Exporta la tabla seleccionada a XML
    [HttpPost]
    public async Task<IActionResult> ExportarTabla(string tabla)
    {
        // Llama al servicio
        await _xml.ExportarTabla(tabla);

        // Mensaje para el usuario
        ViewBag.Mensaje = $"XML de {tabla} generado correctamente";

        return View("Exportar");
    }

    // Pantalla inicial de importación
    public IActionResult Importar()
    {
        return View();
    }

    // Importa un archivo XML subido por el usuario
    [HttpPost]
    public async Task<IActionResult> ImportarArchivo(IFormFile archivo)
    {
        try
        {
            // Llama al servicio para procesar XML
            await _xml.ImportarDesdeArchivo(archivo);

            ViewBag.Mensaje = "Archivo importado correctamente";
        }
        catch
        {
            ViewBag.Mensaje = "Error al importar XML";
        }

        return View("Importar");
    }

   
}
