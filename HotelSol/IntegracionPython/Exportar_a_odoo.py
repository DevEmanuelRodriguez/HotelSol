import xml.etree.ElementTree as ET
import xmlrpc.client
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
# -----------------------------
ruta_xml = os.path.abspath(
    os.path.join(base, "..", "wwwroot", "Producto.xml")
)

# -----------------------------
# FUNCION NORMALIZAR
# -----------------------------
def normalizar(texto):
    if texto is None:
        return ""
    return texto.strip().upper()

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
# OBTENER PRODUCTOS EXISTENTES
# -----------------------------
productos_odoo = models.execute_kw(
    db, uid, password,
    'product.template', 'search_read',
    [[]],
    {
        'fields': ['id', 'name'],
        'limit': 500
    }
)

# -----------------------------
# PROCESAR XML
# -----------------------------
for nodo in root.findall("Producto"):

    nombre = nodo.findtext("Nombre", "").strip()
    detalle = nodo.findtext("Detalle", "").strip()
    precio = float(nodo.findtext("Precio", "0"))

    nombre_normalizado = normalizar(nombre)

    template_id = False

    # Buscar si existe ignorando mayúsculas/minúsculas
    for p in productos_odoo:
        if normalizar(p["name"]) == nombre_normalizado:
            template_id = p["id"]
            break

    # ---------------------------------
    # SI NO EXISTE -> CREAR
    # ---------------------------------
    if not template_id:

        nuevo_id = models.execute_kw(
            db, uid, password,
            'product.template', 'create',
            [{
                'name': nombre,
                'description': detalle,
                'list_price': precio
            }]
        )

        print(f"Creado: {nombre}")

        # añadir a lista local para evitar duplicados
        productos_odoo.append({
            "id": nuevo_id,
            "name": nombre
        })

    # ---------------------------------
    # SI EXISTE -> ACTUALIZAR
    # ---------------------------------
    else:

        models.execute_kw(
            db, uid, password,
            'product.template', 'write',
            [[template_id], {
                'description': detalle,
                'list_price': precio
            }]
        )

        print(f"Actualizado: {nombre}")

print("Proceso terminado")