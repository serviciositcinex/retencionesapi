# API Retenciones Cinex (restAPI_RetencionesV1)

**Versión:** 1.0 (Migrado a .NET Framework 4.6.2)  
**Repositorio:** [https://github.com/serviciositcinex/retencionesapi](https://github.com/serviciositcinex/retencionesapi)

## 📌 Descripción del Proyecto

Este proyecto es una **API RESTful** construida en **ASP.NET (Web API)** que provee métodos para la consulta, listado y exportación de **Retenciones (IVA, ISLR y ARCV)** asociadas a proveedores.

El sistema se conecta a las bases de datos transaccionales, utilizando múltiples Procedimientos Almacenados (SPs) para extraer información fiscal y de facturación. Además, cuenta con la capacidad de exportar los resultados directamente a archivos Excel (`.xlsx`) utilizando plantillas `.xlsx` ubicadas en `Resources\Template\`.

## ⚙ Servidores y Bases de Datos
Las conexiones se definen en el archivo `Web.config` y en los queries internos (Servidores Vinculados).

### 🖥️ Servidores Identificados:
- **172.20.4.70** (Conexión "SAP" en Web.config)
- **SOFCDBS01.advt.intra** (Conexión "OFC" / Server donde habitan los SPs del Script SQL)
- **SPRDDGP01** (Linked Server/Instancia llamado dentro de las consultas SQL para consultar los datos en Dynamics/Bases dinámicas)

### 🗄️ Bases de Datos:
- **SBO_SASO_TEST3** (Catálogo principal de la conexion SAP actual).
- **InterfacesYProgramas** (Donde se alojan / despliegan los SPs auxiliares: `Lee_Retenciones...`).
- **DYNAMICS** (Para consultas sobre metadatos y empresas `SY01500`).
- Esquemas dinámicos por ID de empresa (Ej: Empresa *RPSAS*, dinámicamente resueltos).

---

## 🔗 Endpoints y Procedimientos Almacenados (SP)

A continuación, la relación de los principales Endpoints presentes en `RetencionesController.cs` y los SPs que ejecutan:

| Método / Endpoint | Procedimiento Almacenado principal | Descripción |
|---|---|---|
| `GET /api/Retenciones/GetRetenciones` | `SP_CRI` | Obtiene el listado crudo de retenciones (IVA) asociadas a un vendedor. |
| `GET /api/Retenciones/GetRetenciones_xls` | `SP_CRI` | Obtiene y exporta a Excel la data de Retenciones de IVA. |
| `GET /api/Retenciones/GetProveedores_Retenciones`| `SP_CRI` | Retorna los comprobantes de un proveedor. |
| `GET /api/Retenciones/GetProveedores_RetencionesDetalles`| `SP_CRI` | Muestra el detalle de los comprobantes. |
| `GET /api/Retenciones/GetProveedores_Datos` | `SP_CRI` | Consulta los datos biográficos de un proveedor (RIF, Nombre, Tlf, Correo). |
| `GET /api/Retenciones/GetProveedores_Empresas` | `SP_CRI` | Obtiene las empresas en las que el proveedor está registrado. |
| `GET /api/Retenciones/GetRetenciones_ISLR_Preview`| `SP_CR_ISLR_PORTAL`| Preview de Retenciones ISLR. |
| `GET /api/Retenciones/GetRetenciones_ISLR_xls` | `SP_CR_ISLR_PORTAL`| Descarta archivo Excel con el detalle del ISLR. |
| `GET /api/Retenciones/GetRetenciones_ARCV_xls` | `Lee_RetencionesARCV_ProveedorDetallesV1` | Exportación a EXCEL de la relación ARCV de un año específico. |

---

## 📑 Tablas involucradas
De acuerdo a lo examinado en la carpeta `SQL`, los procedimientos y funciones hacen joins cruzados entre las siguientes tablas (generalmente en tablas bajo el Linked Server `SPRDDGP01` e InterIDs dinámicos):

* **Tablas Maestras Dynamics y Configuración:**
  * `SY01500` (Listado de empresas "InterID", Nombres, y RIFs)
  * `SY01200` (Correos y Master IDs de proveedores)
  * `PM00200` (Vendor Master, data biográfica y direcciones)
* **Tablas y Vistas Transaccionales Payables (PM):**
  * `PM30200` y `PM30700` (Histórico de transacciones/Documentos y Tax)
  * `PM10500` y `PM20000` 
* **Tablas de Impuestos (TAX):**
  * `TX00201` (Tax Details, Porcentajes aplicados)
* **Tablas Personalizadas / Work (Retenciones):**
  * `SIN_Print_Comp_ISLR_LINE` (Líneas de comprobantes ISLR)
  * `APG_Print_Comp_LINE_WORK` y `APG_Print_Comp_HDR_WORK` (Líneas y Headers de Trabajo de comprobantes)
  * `SIN_ControlNum_PM` (Tabla puente de Número de Control)

---

## 🛠 Instalación y Despliegue

1. Clonar el repositorio.
2. Abrir la solución `restAPI_Retenciones.sln` con Microsoft Visual Studio 2019/2022.
3. Restaurar los paquetes NuGet (Contiene Newtonsoft.Json, EPPlus para generación de Excel, y Cors WebAPI).
4. El proyecto debe ser compilado bajo el Framework **.NET 4.6.2** a destino `Any CPU`.
5. Asegurar tener acceso TCP/IP habilitado en la red a la IP del Servidor (`172.20.4.70`) o actualizar el `Web.config` con la cadena de conexión correspondiente.
6. Publicar la aplicación web a IIS, asegurando que el Application Pool corra bajo .NET CLR Versión v4.0 y en modo Integrado.
