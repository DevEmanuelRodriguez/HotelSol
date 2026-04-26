import xml.etree.ElementTree as ET
import xmlrpc.client
import os

base = os.path.dirname(__file__)


# CONFIGURACION ODOO

url = "http://localhost:8069"
db = "odoo18"
username = "milenamartinez091993@gmail.com"
password = "53efe908442501a9fc1b1ff4cbaa059c239a263d"


# RUTA XML

ruta_xml = os.path.abspath(
    os.path.join(base, "..", "wwwroot", "Persona.xml")
)


# NORMALIZAR

def normalizar(texto):
    if texto is None or texto is False:
        return ""
    return str(texto).strip().upper()


# CONEXION

common = xmlrpc.client.ServerProxy(f"{url}/xmlrpc/2/common")
uid = common.authenticate(db, username, password, {})

if not uid:
    print("Error login")
    exit()

models = xmlrpc.client.ServerProxy(f"{url}/xmlrpc/2/object")

print("Conectado a Odoo")


# LEER XML

tree = ET.parse(ruta_xml)
root = tree.getroot()


# CLIENTES EXISTENTES

clientes = models.execute_kw(
    db, uid, password,
    'res.partner', 'search_read',
    [[]],
    {
        'fields': ['id', 'name', 'vat', 'email'],
        'limit': 500
    }
)


# PROCESAR

for nodo in root.findall("Cliente"):

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

print("Proceso terminado")