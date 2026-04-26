/*
 Este controller gestiona:
 - Exportar datos desde la base de datos a XML
 - Importar datos desde un archivo XML a la base de datos
 - Integración automática con Odoo mediante Python
*/
using HotelSol.Filters;
using Microsoft.AspNetCore.Mvc;
using HotelSol.Services;
using System.Diagnostics;

namespace HotelSol.Controllers;

[RolAdmin]
public class XmlController : Controller
{
    private readonly XmlService _xml;
    private readonly IWebHostEnvironment _env;

    public XmlController(XmlService xml, IWebHostEnvironment env)
    {
        _xml = xml;
        _env = env;
    }

    // -----------------------------
    // VISTA EXPORTAR
    // -----------------------------
    public IActionResult Exportar()
    {
        return View();
    }

    // -----------------------------
    // EXPORTAR XML NORMAL
    // -----------------------------
    [HttpPost]
    public async Task<IActionResult> ExportarTabla(string tabla)
    {
        try
        {
            await _xml.ExportarTabla(tabla);

            ViewBag.Mensaje = $"XML de {tabla} generado correctamente";
        }
        catch
        {
            ViewBag.Mensaje = "Error al exportar XML";
        }

        return View("Exportar");
    }

    // -----------------------------
    // EXPORTAR A ODOO
    // -----------------------------
    [HttpPost]
    public async Task<IActionResult> ExportarOdoo(string tabla)
    {
        try
        {
            if (tabla == "Producto")
            {
                await _xml.ExportarTabla("Producto");
                EjecutarPython("Exportar_a_odoo.py");

                ViewBag.Mensaje = "Productos exportados correctamente a Odoo";
            }
            else if (tabla == "Cliente")
            {
                await _xml.ExportarTabla("Persona");
                EjecutarPython("Exportar_cliente_odoo.py");

                ViewBag.Mensaje = "Clientes exportados correctamente a Odoo";
            }
        }
        catch
        {
            ViewBag.Mensaje = "Error al exportar a Odoo";
        }

        return View("Exportar");
    }

    // -----------------------------
    // VISTA IMPORTAR
    // -----------------------------
    public IActionResult Importar()
    {
        return View();
    }

    // -----------------------------
    // IMPORTAR XML MANUAL
    // -----------------------------
    [HttpPost]
    public async Task<IActionResult> ImportarArchivo(IFormFile archivo)
    {
        try
        {
            await _xml.ImportarDesdeArchivo(archivo);

            ViewBag.Mensaje = "Archivo importado correctamente";
        }
        catch
        {
            ViewBag.Mensaje = "Error al importar XML";
        }

        return View("Importar");
    }

    // -----------------------------
    // IMPORTAR DESDE ODOO
    // -----------------------------
    [HttpPost]
    public async Task<IActionResult> ImportarOdoo(string tabla)
    {
        try
        {
            string ruta = "";

            if (tabla == "Producto")
            {
                EjecutarPython("Importar_desde_odoo.py");

                ruta = Path.Combine(_env.WebRootPath, "ProductosDesdeOdoo.xml");

                ViewBag.Mensaje = "Productos importados correctamente desde Odoo";
            }
            else if (tabla == "Cliente")
            {
                EjecutarPython("Importar_cliente_odoo.py");

                ruta = Path.Combine(_env.WebRootPath, "ClientesDesdeOdoo.xml");

                ViewBag.Mensaje = "Clientes importados correctamente desde Odoo";
            }

            using var stream = System.IO.File.OpenRead(ruta);

            var archivo = new FormFile(stream, 0, stream.Length, "archivo", Path.GetFileName(ruta));

            await _xml.ImportarDesdeArchivo(archivo);
        }
        catch
        {
            ViewBag.Mensaje = "Error al importar desde Odoo";
        }

        return View("Importar");
    }

    // -----------------------------
    // EJECUTAR PYTHON
    // -----------------------------
    private void EjecutarPython(string archivoPy)
    {
        var rutaScripts = @"C:\Users\Acer\Desktop\FP UOC\Semestre 1 2026\.NET\P4 punto NET\IntegracionPython";

        Process proceso = new Process();

        proceso.StartInfo.FileName = "python";
        proceso.StartInfo.Arguments = archivoPy;
        proceso.StartInfo.WorkingDirectory = rutaScripts;
        proceso.StartInfo.CreateNoWindow = true;
        proceso.StartInfo.UseShellExecute = false;

        proceso.Start();
        proceso.WaitForExit();
    }
}