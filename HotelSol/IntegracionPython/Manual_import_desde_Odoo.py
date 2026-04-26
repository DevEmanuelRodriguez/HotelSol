import xmlrpc.client
import xml.etree.ElementTree as ET
import re
import os

base = os.path.dirname(__file__)

# CONFIGURACION ODOO
url = "http://localhost:8069"
db = "odoo18"
username = "milenamartinez091993@gmail.com"
password = "53efe908442501a9fc1b1ff4cbaa059c239a263d"

# RUTA SALIDA XML
uta_salida = r"C:\Users\Acer\Desktop\FP UOC\Semestre 1 2026\.NET\P4 punto NET\IntegracionPython\ProductosDesdeOdoo.xml"

# FUNCION LIMPIAR HTML
def limpiar_html(texto):
    if not texto:
        return ""

    # quitar etiquetas HTML
    limpio = re.sub(r"<.*?>", "", texto)

    # limpiar espacios
    limpio = limpio.strip()

    return limpio

# CONEXION ODOO
common = xmlrpc.client.ServerProxy(f"{url}/xmlrpc/2/common")

uid = common.authenticate(db, username, password, {})

if not uid:
    print("Error login")
    exit()

models = xmlrpc.client.ServerProxy(f"{url}/xmlrpc/2/object")

print("Conectado a Odoo")

# LEER PRODUCTOS
productos = models.execute_kw(
    db,
    uid,
    password,
    'product.template',
    'search_read',
    [[]],
    {
        'fields': ['name', 'description', 'list_price', 'qty_available'],
        'limit': 100
    }
)

# CREAR XML
root = ET.Element("Productos")

for p in productos:
    nodo = ET.SubElement(root, "Producto")

    nombre = str(p['name'] or "")
    detalle = limpiar_html(str(p['description'] or ""))
    precio = str(p['list_price'] or 0)
    cantidad = str(int(p['qty_available'] or 0))

    ET.SubElement(nodo, "Nombre").text = nombre
    ET.SubElement(nodo, "Detalle").text = detalle
    ET.SubElement(nodo, "Precio").text = precio
    ET.SubElement(nodo, "Cantidad").text = cantidad

tree = ET.ElementTree(root)

tree.write(
    ruta_salida,
    encoding="utf-8",
    xml_declaration=True
)

print("XML generado correctamente")
print("Ruta:", ruta_salida)