import xml.etree.ElementTree as ET
import xmlrpc.client
import os

base = os.path.dirname(__file__)

# -----------------------------
# CONFIGURACION ODOO
# -----------------------------
url = "http://localhost:8069"
db = "odoo18"
username = "milenamartinez091993@gmail.com"   # o tu email si entras con email
password = "53efe908442501a9fc1b1ff4cbaa059c239a263d"

# -----------------------------
# RUTA XML
# -----------------------------
ruta_xml = r"C:\Users\Acer\Desktop\FP UOC\Semestre 1 2026\.NET\P4 punto NET\HotelSol\HotelSol\wwwroot\Producto.xml"

# -----------------------------
# CONEXION ODOO
# -----------------------------
common = xmlrpc.client.ServerProxy(f"{url}/xmlrpc/2/common")

uid = common.authenticate(db, username, password, {})

if not uid:
    print("Error login Odoo")
    exit()

models = xmlrpc.client.ServerProxy(f"{url}/xmlrpc/2/object")

print("Conectado a Odoo")

# -----------------------------
# LEER XML
# -----------------------------
tree = ET.parse(ruta_xml)
root = tree.getroot()

# -----------------------------
# INSERTAR PRODUCTOS
# -----------------------------
for nodo in root.findall("Producto"):

    nombre = nodo.find("Nombre").text
    detalle = nodo.find("Detalle").text
    precio = float(nodo.find("Precio").text)

    # comprobar si existe
    existente = models.execute_kw(
        db,
        uid,
        password,
        'product.template',
        'search',
        [[['name', '=', nombre]]]
    )

    if existente:
        print(f"Ya existe: {nombre}")
        continue

    # crear producto
    models.execute_kw(
        db,
        uid,
        password,
        'product.template',
        'create',
        [{
            'name': nombre,
            'description': detalle,
            'list_price': precio
        }]
    )

    print(f"Creado: {nombre}")

print("Proceso terminado")