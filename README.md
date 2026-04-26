# README - Instalación rápida HotelSOL

## Pasos obligatorios para ejecutar correctamente

## 1. Instalar requisitos previos

Debe estar instalado en el equipo:

* SQL Server Express
* Docker Desktop
* Python 3.x

---

## 2. Restaurar base de datos

Abrir SQL Server Management Studio y restaurar:

HotelSOL.bak


o ejecutar:


HotelSOL.sql


---

## 3. Iniciar Odoo

Abrir Docker Desktop

Entrar en carpeta Docker y ejecutar:


docker-compose up -d

Acceso:

http://localhost:8069


---

## 4. Instalar dependencias Python

Entrar en carpeta `IntegracionPython` y ejecutar (sirve para instalar automáticamente las librerías de Python que necesitan los scripts de integración con Odoo):


pip install -r requirements.txt


---

## 5. Instalar HotelSOL

Ejecutar:


Setup.exe


o abrir:

HotelSol.exe


---

## 6. Iniciar aplicación

Abrir acceso directo de escritorio o ejecutar:


HotelSOL


---

## 7. Login inicial


Usuario: 4545453
Contraseña: 123


---

## Si algo falla

* Verificar SQL Server iniciado
* Verificar Docker iniciado
* Verificar Python instalado
* Verificar Odoo activo en puerto 8069
