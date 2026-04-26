import xmlrpc.client
import xml.etree.ElementTree as ET
import re
import os

base = os.path.dirname(__file__)
# -----------------------------
# CONFIGURACION ODOO
# -----------------------------
url = "http://localhost:8069"
db = "odoo18"
username = "milenamartinez091993@gmail.com"
password = "53efe908442501a9fc1b1ff4cbaa059c239a263d"

# -----------------------------
# RUTA XML
# (puedes dejar fija por ahora)
# -----------------------------
ruta_salida = os.path.abspath(
    os.path.join(base, "..", "wwwroot", "ProductosDesdeOdoo.xml")
)

# -----------------------------
# FUNCIONES
# -----------------------------
def normalizar(texto):
    if not texto:
        return ""
    return texto.strip().upper()

def limpiar_html(texto):
    if not texto:
        return ""

    limpio = re.sub(r"<.*?>", "", texto)
    return limpio.strip()

# -----------------------------
# CONEXION
# -----------------------------
common = xmlrpc.client.ServerProxy(f"{url}/xmlrpc/2/common")
uid = common.authenticate(db, username, password, {})

if not uid:
    print("Error login")
    exit()

models = xmlrpc.client.ServerProxy(f"{url}/xmlrpc/2/object")

print("Conectado a Odoo")

# -----------------------------
# LEER PRODUCTOS DESDE TEMPLATE
# -----------------------------
productos = models.execute_kw(
    db, uid, password,
    'product.template', 'search_read',
    [[]],
    {
        'fields': ['name', 'description', 'list_price', 'qty_available'],
        'limit': 500
    }
)

# -----------------------------
# CREAR XML
# -----------------------------
root = ET.Element("Productos")
nombres = set()

for p in productos:

    nombre = str(p['name'] or "").strip()
    clave = normalizar(nombre)

    # evitar duplicados Pizza / pizza / PIZZA
    if clave in nombres:
        continue

    nombres.add(clave)

    detalle = limpiar_html(str(p['description'] or ""))
    precio = str(p['list_price'] or 0)
    cantidad = str(int(p['qty_available'] or 0))

    nodo = ET.SubElement(root, "Producto")

    ET.SubElement(nodo, "Nombre").text = nombre
    ET.SubElement(nodo, "Detalle").text = detalle
    ET.SubElement(nodo, "Precio").text = precio
    ET.SubElement(nodo, "Cantidad").text = cantidad

# -----------------------------
# GUARDAR XML
# -----------------------------
tree = ET.ElementTree(root)
tree.write(ruta_salida, encoding="utf-8", xml_declaration=True)

print("XML generado correctamente")
print("Ruta:", ruta_salida)