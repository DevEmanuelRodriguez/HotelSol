import xml.etree.ElementTree as ET
import xmlrpc.client
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
# RUTA XML
# --------------------------------------------------

ruta_xml = os.path.join(carpeta, "Producto.xml")

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
    if texto is None:
        return ""
    return str(texto).strip().upper()

# --------------------------------------------------
# COMPROBAR XML
# --------------------------------------------------

if not os.path.exists(ruta_xml):
    print("No existe Producto.xml")
    sys.exit()

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
# LEER XML
# --------------------------------------------------

try:
    tree = ET.parse(ruta_xml)
    root = tree.getroot()

except Exception as e:
    print("Error leyendo Producto.xml")
    print(str(e))
    sys.exit()

# --------------------------------------------------
# PRODUCTOS EXISTENTES EN ODOO
# --------------------------------------------------

try:
    productos_odoo = models.execute_kw(
        db, uid, password,
        'product.template', 'search_read',
        [[]],
        {
            'fields': ['id', 'name'],
            'limit': 500
        }
    )

except Exception as e:
    print("Error leyendo productos Odoo")
    print(str(e))
    sys.exit()

# --------------------------------------------------
# PROCESAR PRODUCTOS
# --------------------------------------------------

for nodo in root.findall("Producto"):

    try:
        nombre = nodo.findtext("Nombre", "").strip()
        detalle = nodo.findtext("Detalle", "").strip()

        try:
            precio = float(nodo.findtext("Precio", "0"))
        except:
            precio = 0

        nombre_normalizado = normalizar(nombre)

        template_id = False

        for p in productos_odoo:
            if normalizar(p["name"]) == nombre_normalizado:
                template_id = p["id"]
                break

        # CREAR
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

            productos_odoo.append({
                "id": nuevo_id,
                "name": nombre
            })

            print("Creado:", nombre)

        # ACTUALIZAR
        else:

            models.execute_kw(
                db, uid, password,
                'product.template', 'write',
                [[template_id], {
                    'description': detalle,
                    'list_price': precio
                }]
            )

            print("Actualizado:", nombre)

    except Exception as e:
        print("Error procesando producto:", str(e))

print("Proceso terminado correctamente")