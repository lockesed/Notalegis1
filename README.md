# Notalegis

**Sistema de gestión para escribanías.** Aplicación de escritorio para Windows que reúne en un solo lugar los clientes, turnos, trámites, escrituras, cobros, pagos e insumos de una escribanía. Incluye un generador de escrituras asistido por inteligencia artificial.

Proyecto desarrollado por **Facundo Lisandro Mato**.

> Todos los datos de la escribanía, los clientes y los usuarios incluidos en este repositorio son ficticios.

<!-- Capturas de pantalla: agregá imágenes en una carpeta docs/ y enlazalas acá, por ejemplo:
![Pantalla principal](docs/principal.png)
-->

## Qué hace

| Módulo | Para qué sirve |
|---|---|
| **Turnos** | Agenda de citas con control de horarios superpuestos, confirmación, comprobante y recordatorio por WhatsApp. |
| **Clientes** | Alta, edición y búsqueda de clientes, con validación de datos. |
| **Trámites** | Seguimiento de cada trámite: estado, prioridad, fechas y honorarios. |
| **Cobros y pagos** | Presupuestos, registro de pagos y control de saldos. |
| **Escrituras** | Registro de escrituras y tipos de escritura. |
| **Generador de escrituras con IA** | Redacta borradores de escrituras a partir de los datos cargados y de plantillas, usando la API de Claude (Anthropic). El resultado se exporta a Word con el formato notarial. |
| **Pedidos de insumos** | Pedidos de hojas protocolares, sellos y otros insumos, con control de stock. |
| **Estadísticas y reportes** | 14 métricas con gráficos por período y exportación a Excel y PDF. |
| **Usuarios y roles** | Administrador, escribana adjunta, secretaria y procurador, cada uno con sus permisos. |
| **Auditoría** | Registro de quién hizo cada cambio y cuándo. |

El detalle de cada módulo está en [DOCUMENTACION_FUNCIONAL.md](DOCUMENTACION_FUNCIONAL.md).

## Tecnologías

- **C# y .NET 8** con **WPF** y **Material Design** para la interfaz.
- **SQL Server** como base de datos, con consultas parametrizadas.
- **Arquitectura en tres capas:**
  - `NotalegisWPF`: interfaz;
  - `Notalegis.Negocio`: reglas y servicios;
  - `Notalegis.Datos`: acceso a datos.
- Librerías:
  - **ClosedXML** (Excel);
  - **OpenXML** y **Xceed DocX** (Word);
  - **PdfSharpCore**, **iText** y **PdfPig** (PDF);
  - **ScottPlot** (gráficos);
  - **NLog** (registro de eventos);
  - **BCrypt** (contraseñas).
- **API de Claude (Anthropic)** para la redacción asistida.

## Seguridad

- Contraseñas guardadas con **BCrypt**, nunca en texto plano.
- **Bloqueo temporal** del usuario después de varios intentos fallidos de ingreso.
- **Consultas parametrizadas** en todo el acceso a datos, como protección contra inyección SQL.
- Cadena de conexión **cifrada con DPAPI** de Windows.
- La **clave de la API de IA no está en el código**: se carga desde la configuración del programa.
- **Auditoría** de cambios y **borrado lógico**: los registros eliminados se conservan para no perder el historial.

## Cómo ejecutarlo

**Requisitos:**
- Windows 10 u 11;
- [.NET 8 SDK](https://dotnet.microsoft.com/download);
- SQL Server Express (instancia `SQLEXPRESS`);
- SQL Server Management Studio;
- opcionalmente, Visual Studio 2022.

1. **Crear la base de datos.** En SQL Server Management Studio conectate a `.\SQLEXPRESS` y ejecutá [`NOTALEGIS_BASE_COMPLETA.sql`](NOTALEGIS_BASE_COMPLETA.sql). Crea la base `NotalegisDB`, sus tablas, usuarios de demostración y datos de ejemplo.
2. **Revisar la conexión.** Si tu instancia de SQL Server tiene otro nombre, cambiá `Data Source` en `NotalegisWPF/App.config`.
3. **Ejecutar.** Abrí `NotalegisWPF.sln` en Visual Studio y presioná F5, o desde una terminal:
   ```
   dotnet run --project NotalegisWPF
   ```
4. **(Opcional) Activar la IA.** En el programa, entrá a **Configuración → Inteligencia Artificial** y pegá tu propia clave de API de Anthropic.

### Usuarios de demostración

| Usuario | Contraseña | Rol |
|---|---|---|
| admin | Test1234 | Administrador |
| secretaria1 | Test1234 | Secretaria |
| legal1 | Test1234 | Escribana adjunta |
| gestor1 | Test1234 | Procurador (solo trámites, clientes, turnos y escrituras) |

Son credenciales de prueba. Si usás el programa con datos reales, cambialas.

## Licencia

[MIT](LICENSE.txt)
