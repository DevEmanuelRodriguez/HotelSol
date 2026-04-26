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

ruta_xml = os.path.join(carpeta, "Persona.xml")

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
    if texto is None or texto is False:
        return ""
    return str(texto).strip().upper()

# --------------------------------------------------
# COMPROBAR XML
# --------------------------------------------------

if not os.path.exists(ruta_xml):
    print("No existe Persona.xml")
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
    print("Error leyendo Persona.xml")
    print(str(e))
    sys.exit()

# --------------------------------------------------
# CLIENTES EXISTENTES
# --------------------------------------------------

try:
    clientes = models.execute_kw(
        db, uid, password,
        'res.partner', 'search_read',
        [[]],
        {
            'fields': ['id', 'name', 'vat', 'email'],
            'limit': 500
        }
    )

except Exception as e:
    print("Error leyendo clientes Odoo")
    print(str(e))
    sys.exit()

# --------------------------------------------------
# PROCESAR CLIENTES
# --------------------------------------------------

for nodo in root.findall("Cliente"):

    try:
        nombre = nodo.findtext("Nombre", "").strip()
        apellido = nodo.findtext("Apellido", "").strip()
        documento = nodo.findtext("Documento", "").strip()
        correo = nodo.findtext("Correo", "").strip()

        nombre_completo = f"{nombre} {apellido}".strip()

        partner_id = False

        # Buscar por documento
        if documento != "":
            for c in clientes:
                if normalizar(c.get("vat")) == normalizar(documento):
                    partner_id = c["id"]
                    break

        # Buscar por correo
        if not partner_id and correo != "":
            for c in clientes:
                if normalizar(c.get("email")) == normalizar(correo):
                    partner_id = c["id"]
                    break

        # CREAR
        if not partner_id:

            models.execute_kw(
                db, uid, password,
                'res.partner', 'create',
                [{
                    'name': nombre_completo,
                    'vat': documento,
                    'email': correo
                }]
            )

            print("Creado:", nombre_completo)

        # ACTUALIZAR
        else:

            models.execute_kw(
                db, uid, password,
                'res.partner', 'write',
                [[partner_id], {
                    'name': nombre_completo,
                    'vat': documento,
                    'email': correo
                }]
            )

            print("Actualizado:", nombre_completo)

    except Exception as e:
        print("Error procesando cliente:", str(e))

print("Proceso terminado correctamente")