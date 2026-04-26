import xmlrpc.client
import xml.etree.ElementTree as ET
import os
import sys

# --------------------------------------------------
# CARPETA DOCUMENTOS HOTELSOL
# --------------------------------------------------

carpeta = os.path.join(
    os.path.expanduser("~"),
    "Documents",
    "HotelSOL"
)

os.makedirs(carpeta, exist_ok=True)

# --------------------------------------------------
# RUTA SALIDA XML
# --------------------------------------------------

ruta_salida = os.path.join(carpeta, "ClientesDesdeOdoo.xml")

# --------------------------------------------------
# CONFIGURACIÓN ODOO
# --------------------------------------------------

url = "http://localhost:8069"
db = "odoo18"
username = "milenamartinez091993@gmail.com"
password = "53efe908442501a9fc1b1ff4cbaa059c239a263d"

# --------------------------------------------------
# CONEXIÓN ODOO
# --------------------------------------------------

try:
    common = xmlrpc.client.ServerProxy(f"{url}/xmlrpc/2/common")
    uid = common.authenticate(db, username, password, {})

    if not uid:
        print("Error login Odoo")
        sys.exit()

    models = xmlrpc.client.ServerProxy(f"{url}/xmlrpc/2/object")

except Exception as e:
    print("No se pudo conectar con Odoo")
    print(str(e))
    sys.exit()

print("Conectado a Odoo")

# --------------------------------------------------
# LEER CLIENTES ODOO
# --------------------------------------------------

try:
    clientes = models.execute_kw(
        db, uid, password,
        'res.partner', 'search_read',
        [[]],
        {
            'fields': ['name', 'vat', 'email'],
            'limit': 500
        }
    )

except Exception as e:
    print("Error obteniendo clientes desde Odoo")
    print(str(e))
    sys.exit()

# --------------------------------------------------
# CREAR XML
# --------------------------------------------------

root = ET.Element("Clientes")

for c in clientes:

    try:
        nombre_completo = str(c.get("name") or "").strip()
        documento = str(c.get("vat") or "").strip()
        correo = str(c.get("email") or "").strip()

        # excluir usuarios internos
        if nombre_completo.upper() in ["ADMINISTRATOR", "ADMIN", "ODOOBOT"]:
            continue

        # sin documento no importar
        if documento == "":
            continue

        partes = nombre_completo.split(" ", 1)

        nombre = partes[0]
        apellido = partes[1] if len(partes) > 1 else ""

        nodo = ET.SubElement(root, "Cliente")

        ET.SubElement(nodo, "TipoDocumento").text = "DNI"
        ET.SubElement(nodo, "Nombre").text = nombre
        ET.SubElement(nodo, "Apellido").text = apellido
        ET.SubElement(nodo, "Documento").text = documento
        ET.SubElement(nodo, "Correo").text = correo

    except Exception as e:
        print("Error procesando cliente:", str(e))

# --------------------------------------------------
# GUARDAR XML
# --------------------------------------------------

try:
    tree = ET.ElementTree(root)
    ET.indent(tree, space="    ", level=0)
    tree.write(ruta_salida, encoding="utf-8", xml_declaration=True)

except Exception as e:
    print("Error guardando ClientesDesdeOdoo.xml")
    print(str(e))
    sys.exit()

print("Clientes exportados correctamente")
print("Ruta:", ruta_salida)