# Envíos Rápidos GT 🇬🇹
### Sistema de Gestión de Envíos Nacionales - Examen Final Análisis de Sistemas I

Este proyecto consiste en una API REST robusta desarrollada en **.NET 8 Web API** utilizando **Entity Framework Core** y **SQLite** como motor de persistencia. Está diseñado bajo los principios de arquitectura limpia y desarrollo guiado por reglas de negocio, satisfaciendo 10 Historias de Usuario (HU).

## 🚀 Características Clave
- **Cálculo Automatizado de Tarifas:** Rangos de precio dinámicos basados en el peso del paquete.
- **Validación Automatizada de NIT:** Implementación estricta del algoritmo Módulo 11 utilizado por la SAT de Guatemala (con soporte integrado para Consumidor Final "CF").
- **Control de Descuentos:** Reducciones comerciales del 5% si el remitente o el destinatario presentan identificación fiscal válida.
- **Máquina de Estados de Flujo de Envíos:** Validaciones de transiciones de estado para evitar saltos de proceso no permitidos (ej. de *Registrado* directo a *Entregado*).
- **Historial de Auditoría Inmutable:** Registro secuencial de cada cambio físico o de estado del paquete.

## ☁️ Despliegue en la Nube (Render)
La API se encuentra desplegada y funcionando en vivo a través de Render utilizando un contenedor Docker. 

**URL Base de Produccion:**
👉 `https://zero907-23-13365analisisa2026final.onrender.com`

**¿Cómo probarlo?**
Puedes utilizar herramientas como **Postman**, **Thunder Client** o comandos de terminal, apuntando a los endpoints descritos abajo. 

*Ejemplo de prueba rápida en PowerShell:*
```powershell
Invoke-RestMethod -Uri "[https://zero907-23-13365analisisa2026final.onrender.com/api/clientes](https://zero907-23-13365analisisa2026final.onrender.com/api/clientes)" -Method Get

## 🛠️ Tecnologías Utilizadas
- .NET 8.0 (C#)
- Entity Framework Core 8.0
- SQLite
- xUnit (Pruebas Unitarias Automatizadas)

## 📁 Estructura del Repositorio
```text
ExamenFinalAnalisis/
├── EnviosRapidosGT/          # Proyecto Principal (API REST)
│   ├── Controllers/          # Clientes, Envíos y Reportes
│   ├── Data/                 # AppDbContext y configuraciones
│   ├── DTOs/                 # Estructuras de Petición y Respuesta
│   ├── Models/               # Entidades de Dominio y Enums
│   └── Services/             # Lógica de negocio (Tarifas, NIT, Transiciones)
├── EnviosRapidosGT.Tests/    # Proyecto de Pruebas Unitarias (xUnit)
│   └── EnvioServiceTests.cs  # 25 casos de prueba ejecutados con éxito
├── Dockerfile                # Configuración de despliegue en la nube
└── README.md                 # Documentación técnica