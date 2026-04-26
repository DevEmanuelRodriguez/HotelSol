import xmlrpc.client
import xml.etree.ElementTree as ET
import os

base = os.path.dirname(__file__)

url = "http://localhost:8069"
db = "odoo18"
username = "milenamartinez091993@gmail.com"
password = "TU_API_KEY"

ruta_salida = os.path.abspath(
    os.path.join(base, "..", "wwwroot", "ClientesDesdeOdoo.xml")
)

common = xmlrpc.client.ServerProxy(f"{url}/xmlrpc/2/common")
uid = common.authenticate(db, username, password, {})

models = xmlrpc.client.ServerProxy(f"{url}/xmlrpc/2/object")

clientes = models.execute_kw(
    db, uid, password,
    'res.partner', 'search_read',
    [[]],
    {
        'fields': ['name', 'vat', 'email'],
        'limit': 500
    }
)

root = ET.Element("Clientes")

for c in clientes:

    nombre_completo = str(c['name'] or "").strip()
    documento = str(c['vat'] or "").strip()
    correo = str(c['email'] or "").strip()

    # excluir admin
    if nombre_completo.upper() in ["ADMINISTRATOR", "ADMIN", "ODOOBOT"]:
        continue

    # si no tiene documento no importar
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

tree = ET.ElementTree(root)

# FORMATO BONITO
ET.indent(tree, space="    ", level=0)

tree.write(ruta_salida, encoding="utf-8", xml_declaration=True)

print("Clientes exportados correctamente")