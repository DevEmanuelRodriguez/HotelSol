import xmlrpc.client
import xml.etree.ElementTree as ET
import re
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

ruta_salida = os.path.join(carpeta, "ProductosDesdeOdoo.xml")

# --------------------------------------------------
# CONFIGURACIÓN ODOO
# --------------------------------------------------

url = "http://localhost:8069"
db = "odoo18"
username = "milenamartinez091993@gmail.com"
password = "53efe908442501a9fc1b1ff4cbaa059c239a263d"

# --------------------------------------------------
# FUNCIONES
# --------------------------------------------------

def normalizar(texto):
    if not texto:
        return ""
    return str(texto).strip().upper()

def limpiar_html(texto):
    if not texto:
        return ""

    limpio = re.sub(r"<.*?>", "", texto)
    return limpio.strip()

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
# LEER PRODUCTOS ODOO
# --------------------------------------------------

try:
    productos = models.execute_kw(
        db, uid, password,
        'product.template', 'search_read',
        [[]],
        {
            'fields': ['name', 'description', 'list_price', 'qty_available'],
            'limit': 500
        }
    )

except Exception as e:
    print("Error obteniendo productos de Odoo")
    print(str(e))
    sys.exit()

# --------------------------------------------------
# CREAR XML
# --------------------------------------------------

root = ET.Element("Productos")
nombres = set()

for p in productos:

    try:
        nombre = str(p.get("name") or "").strip()
        clave = normalizar(nombre)

        # evitar duplicados
        if clave in nombres:
            continue

        nombres.add(clave)

        detalle = limpiar_html(str(p.get("description") or ""))
        precio = str(p.get("list_price") or 0)
        cantidad = str(int(p.get("qty_available") or 0))

        nodo = ET.SubElement(root, "Producto")

        ET.SubElement(nodo, "Nombre").text = nombre
        ET.SubElement(nodo, "Detalle").text = detalle
        ET.SubElement(nodo, "Precio").text = precio
        ET.SubElement(nodo, "Cantidad").text = cantidad

    except Exception as e:
        print("Error procesando producto:", str(e))

# --------------------------------------------------
# GUARDAR XML
# --------------------------------------------------

try:
    tree = ET.ElementTree(root)
    ET.indent(tree, space="    ", level=0)
    tree.write(ruta_salida, encoding="utf-8", xml_declaration=True)

except Exception as e:
    print("Error guardando ProductosDesdeOdoo.xml")
    print(str(e))
    sys.exit()

print("Productos exportados correctamente")
print("Ruta:", ruta_salida)