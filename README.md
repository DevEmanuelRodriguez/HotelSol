# 🏨 Hotel SOL - Management System

Backend-oriented hotel management application that handles reservations, customer data, room availability, and billing processes.

The system supports role-based access (administrator and receptionist), seasonal pricing, and different accommodation plans (half board, full board, all-inclusive).

It also integrates with Odoo via Python scripts and API Key to synchronize business data between both platforms.

---

## ✨ Features

* Customer management
* Room types and availability tracking
* Reservation system with date validation
* Seasonal pricing (high, mid, low season)
* Check-in / Check-out management
* Incident handling (no-shows)
* Billing system (including extra services)
* Walk-in customers (no reservation)
* Reservation modification and cancellation
* ERP integration with Odoo

---

## 🧱 Tech Stack

* SQL Server Express
* Python (integration scripts)
* Docker (Odoo deployment)
* Desktop application (Windows)

---

## ⚙️ Installation Guide

### 1. Prerequisites

Make sure you have installed:

* SQL Server Express
* Docker Desktop
* Python 3.x

---

### 2. Database Setup

Restore the database using:

* `HotelSOL.bak`
  or
* `HotelSOL.sql`

(using SQL Server Management Studio)

---

### 3. Start Odoo (ERP)

Navigate to the Docker folder and run:

```bash
docker-compose up -d
```

Access Odoo at:

http://localhost:8069

---

### 4. Install Python Dependencies

Navigate to:

```
IntegracionPython/
```

Then run:

```bash
pip install -r requirements.txt
```

---

### 5. Run the Application

Execute:

* `Setup.exe`
  or
* `HotelSol.exe`

---

### 6. Login

```
User: 4545453
Password: 123
```

---

## 🔄 Odoo Integration

The system integrates with Odoo using Python scripts and API Key authentication to synchronize hotel data with ERP processes.

---

## ⚠️ Troubleshooting

* Ensure SQL Server is running
* Ensure Docker is running
* Ensure Python is installed
* Ensure Odoo is accessible at port 8069

---

## 📌 Notes

This project was designed using UML modeling and focuses on real-world business logic for hotel management systems.

