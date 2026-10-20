-- =============================================
-- SCRIPT COMPLETO DE BASE DE DATOS: NOTALEGIS
-- =============================================
-- Base de datos: NotalegisDB
-- Fecha: 2024
-- =============================================

USE master;
GO

-- Crear Base de Datos si no existe
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'NotalegisDB')
BEGIN
    CREATE DATABASE NotalegisDB;
    PRINT 'Base de datos NotalegisDB creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La base de datos NotalegisDB ya existe.';
END
GO

USE NotalegisDB;
GO

PRINT '========================================';
PRINT 'Iniciando creación de tablas...';
PRINT '========================================';
GO

-- =============================================
-- 1. TABLA USUARIOS (Sistema de Login)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Usuarios]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Usuarios](
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [NombreUsuario] NVARCHAR(50) NOT NULL UNIQUE,
        [Contraseña] NVARCHAR(100) NOT NULL, -- Se recomienda hashear en producción
        [Rol] NVARCHAR(50) NOT NULL DEFAULT 'Usuario', -- Admin, Usuario, etc.
        [FechaCreacion] DATETIME NOT NULL DEFAULT GETDATE(),
        [Activo] BIT NOT NULL DEFAULT 1
    );
    
    -- Insertar usuario Admin por defecto si la tabla está vacía
    INSERT INTO Usuarios (NombreUsuario, Contraseña, Rol) VALUES ('admin', 'admin123', 'Admin');
    
    PRINT 'Tabla Usuarios creada.';
END
GO

-- =============================================
-- 2. TABLA CLIENTE (Entidad Principal)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Cliente]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Cliente](
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Nombre] NVARCHAR(100) NOT NULL,
        [Apellido] NVARCHAR(100) NOT NULL,
        [DNI] NVARCHAR(20) NOT NULL UNIQUE,
        [Edad] INT NULL,
        [Telefono] NVARCHAR(50) NULL,
        [Email] NVARCHAR(100) NULL,
        [Direccion] NVARCHAR(200) NULL,
        [Estado] NVARCHAR(50) NOT NULL DEFAULT 'Activo',
        [Observaciones] NVARCHAR(MAX) NULL
    );
    PRINT 'Tabla Cliente creada.';
END
GO

-- =============================================
-- 3. TABLA ESTADOCLIENTE (Historial de estados)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[EstadoCliente]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[EstadoCliente](
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [IdCliente] INT NOT NULL,
        [Estado] NVARCHAR(50) NOT NULL,
        [FechaRegistro] DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_EstadoCliente_Cliente FOREIGN KEY (IdCliente) REFERENCES Cliente(Id) ON DELETE CASCADE
    );
    PRINT 'Tabla EstadoCliente creada.';
END
GO

-- =============================================
-- 4. TABLA TRAMITE (Gestión de Trámites)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[tramite]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[tramite](
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Cliente] NVARCHAR(200) NOT NULL, -- Nombre completo o denormalizado para búsquedas rápidas
        [TipoTramite] NVARCHAR(100) NOT NULL,
        [NumeroExpediente] NVARCHAR(50) NULL,
        [FechaCreacion] DATETIME NOT NULL DEFAULT GETDATE(),
        [FechaInicio] DATETIME NOT NULL,
        [FechaVencimiento] DATETIME NOT NULL,
        [Estado] NVARCHAR(50) NOT NULL DEFAULT 'Iniciado',
        [Prioridad] NVARCHAR(50) NULL, -- Alta, Media, Baja
        [MontoEstimado] DECIMAL(18,2) NULL,
        [DocumentacionCompleta] BIT NOT NULL DEFAULT 0,
        [Observaciones] NVARCHAR(MAX) NULL
    );
    PRINT 'Tabla tramite creada.';
END
GO

-- =============================================
-- 5. TABLA TURNO (Agenda)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Turno]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Turno](
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [IdCliente] INT NOT NULL,
        [FechaHora] DATETIME NOT NULL,
        [Motivo] NVARCHAR(200) NOT NULL,
        [TipoTramite] NVARCHAR(100) NULL,
        [Estado] NVARCHAR(50) NOT NULL DEFAULT 'Pendiente', -- Pendiente, Confirmado, Cancelado, Realizado
        [Observaciones] NVARCHAR(MAX) NULL,
        [Confirmado] BIT NOT NULL DEFAULT 0,
        CONSTRAINT FK_Turno_Cliente FOREIGN KEY (IdCliente) REFERENCES Cliente(Id)
    );
    PRINT 'Tabla Turno creada.';
END
GO

-- =============================================
-- 6. TABLA COBROS (Facturación y Honorarios)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Cobros]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Cobros](
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [ClienteId] INT NOT NULL,
        [NumeroExpediente] NVARCHAR(50) NULL,
        [FechaCobro] DATETIME NOT NULL,
        [TipoTramite] NVARCHAR(100) NULL,

        -- Impuestos y Tasas
        [ImpGananciasPorc] DECIMAL(5,2) DEFAULT 0,
        [ImpGananciasBase] DECIMAL(18,2) DEFAULT 0,
        [ImpGananciasImporte] DECIMAL(18,2) DEFAULT 0,

        [ITIPorc] DECIMAL(5,2) DEFAULT 0,
        [ITIBase] DECIMAL(18,2) DEFAULT 0,
        [ITIImporte] DECIMAL(18,2) DEFAULT 0,

        [ImpSellosPorc] DECIMAL(5,2) DEFAULT 0,
        [ImpSellosBase] DECIMAL(18,2) DEFAULT 0,
        [ImpSellosImporte] DECIMAL(18,2) DEFAULT 0,

        [TasaGeneralPorc] DECIMAL(5,2) DEFAULT 0,
        [TasaGeneralBase] DECIMAL(18,2) DEFAULT 0,
        [TasaGeneralImporte] DECIMAL(18,2) DEFAULT 0,

        [SelloMatrizPorc] DECIMAL(5,2) DEFAULT 0,
        [SelloMatrizBase] DECIMAL(18,2) DEFAULT 0,
        [SelloMatrizImporte] DECIMAL(18,2) DEFAULT 0,

        -- Certificados y Diligenciamiento
        [CertificadosPorc] DECIMAL(5,2) DEFAULT 0,
        [CertificadosBase] DECIMAL(18,2) DEFAULT 0,
        [CertificadosImporte] DECIMAL(18,2) DEFAULT 0,

        [DiligenciamientoPorc] DECIMAL(5,2) DEFAULT 0,
        [DiligenciamientoBase] DECIMAL(18,2) DEFAULT 0,
        [DiligenciamientoImporte] DECIMAL(18,2) DEFAULT 0,

        -- Honorarios
        [HonorariosPorc] DECIMAL(5,2) DEFAULT 0,
        [HonorariosBase] DECIMAL(18,2) DEFAULT 0,
        [HonorariosImporte] DECIMAL(18,2) DEFAULT 0,

        [IVAPorc] DECIMAL(5,2) DEFAULT 0,
        [IVABase] DECIMAL(18,2) DEFAULT 0,
        [IVAImporte] DECIMAL(18,2) DEFAULT 0,

        -- Otra gestión (trámite personalizado, no predefinido)
        [OtraGestionNombre] NVARCHAR(200) NULL,
        [OtraGestionImporte] DECIMAL(18,2) DEFAULT 0,

        -- Totales
        [SubtotalImpuestos] DECIMAL(18,2) DEFAULT 0,
        [SubtotalCertificados] DECIMAL(18,2) DEFAULT 0,
        [TotalGeneral] DECIMAL(18,2) DEFAULT 0,

        -- Estado y Auditoría
        [Estado] NVARCHAR(50) NOT NULL DEFAULT 'Pendiente', -- Pendiente, Pagado, Anulado
        [MetodoPago] NVARCHAR(50) NULL,
        [Observaciones] NVARCHAR(MAX) NULL,
        [FechaCreacion] DATETIME NOT NULL DEFAULT GETDATE(),
        [FechaActualizacion] DATETIME NULL,
        [UsuarioCreacion] NVARCHAR(100) NULL,
        [UsuarioActualizacion] NVARCHAR(100) NULL,

        CONSTRAINT FK_Cobros_Cliente FOREIGN KEY (ClienteId) REFERENCES Cliente(Id)
    );
    PRINT 'Tabla Cobros creada.';
END
GO

-- =============================================
-- 7. TABLA PAGOS (Detalle de pagos parciales)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Pagos]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Pagos](
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [CobroId] INT NOT NULL,
        [MontoPagado] DECIMAL(18,2) NOT NULL,
        [FechaPago] DATETIME NOT NULL DEFAULT GETDATE(),
        [MetodoPago] NVARCHAR(50) NOT NULL, -- Efectivo, Transferencia, Cheque
        [NumeroComprobante] NVARCHAR(50) NULL,
        [Observaciones] NVARCHAR(MAX) NULL,
        [UsuarioRegistro] NVARCHAR(100) NULL,
        [FechaRegistro] DATETIME NOT NULL DEFAULT GETDATE(),
        CONSTRAINT FK_Pagos_Cobros FOREIGN KEY (CobroId) REFERENCES Cobros(Id) ON DELETE CASCADE
    );
    PRINT 'Tabla Pagos creada.';
END
GO

-- =============================================
-- 8. TABLA TIPOESCRITURA (Catálogo)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TipoEscritura]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[TipoEscritura](
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Nombre] NVARCHAR(100) NOT NULL UNIQUE,
        [Descripcion] NVARCHAR(500) NULL,
        [RutaPlantilla] NVARCHAR(500) NULL,
        [Activo] BIT NOT NULL DEFAULT 1,
        [FechaCreacion] DATETIME NOT NULL DEFAULT GETDATE()
    );
    
    -- Insertar Tipos de Escritura por defecto
    INSERT INTO TipoEscritura (Nombre, Descripcion) VALUES 
    ('Escritura de Compraventa', 'Transferencia de propiedad de inmueble'),
    ('Poder General', 'Otorgamiento de representación amplia'),
    ('Poder Especial', 'Otorgamiento de representación específica'),
    ('Testamento', 'Acto de última voluntad'),
    ('Donación', 'Transferencia gratuita de bienes'),
    ('Hipoteca', 'Garantía real sobre inmueble'),
    ('Cancelación de Hipoteca', 'Levantamiento de gravamen'),
    ('Acta Notarial', 'Constatación de hechos');

    PRINT 'Tabla TipoEscritura creada.';
END
GO

-- =============================================
-- 9. TABLA ESCRITURA (Protocolo Notarial)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Escritura]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Escritura](
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [ClienteId] INT NOT NULL,
        [TipoEscrituraId] INT NOT NULL,
        [NumeroProtocolo] NVARCHAR(50) NOT NULL,
        [Fecha] DATETIME NOT NULL,
        [RutaArchivo] NVARCHAR(500) NULL,
        [FormatoArchivo] NVARCHAR(10) NULL, -- .docx, .pdf
        [Estado] NVARCHAR(50) NOT NULL DEFAULT 'Borrador', -- Borrador, Finalizada, Protocolizada
        [TramiteId] INT NULL, 
        [CobroId] INT NULL,
        [Observaciones] NVARCHAR(MAX) NULL,
        [FechaCreacion] DATETIME NOT NULL DEFAULT GETDATE(),
        [FechaProtocolizacion] DATETIME NULL,
        [UsuarioCreacion] NVARCHAR(100) NULL,
        
        CONSTRAINT FK_Escritura_Cliente FOREIGN KEY (ClienteId) REFERENCES Cliente(Id),
        CONSTRAINT FK_Escritura_TipoEscritura FOREIGN KEY (TipoEscrituraId) REFERENCES TipoEscritura(Id),
        CONSTRAINT FK_Escritura_Tramite FOREIGN KEY (TramiteId) REFERENCES tramite(Id),
        CONSTRAINT FK_Escritura_Cobros FOREIGN KEY (CobroId) REFERENCES Cobros(Id)
    );
    PRINT 'Tabla Escritura creada.';
END
GO

-- =============================================
-- 10. TABLA PEDIDOINSUMO (Listas de Compras)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PedidoInsumo]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[PedidoInsumo](
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [FechaPedido] DATETIME NOT NULL,
        [TipoInsumo] NVARCHAR(100) NOT NULL,
        [Cantidad] INT NOT NULL,
        [Solicitante] NVARCHAR(200) NOT NULL,
        [Observaciones] NVARCHAR(MAX) NULL,
        [Estado] NVARCHAR(50) NOT NULL DEFAULT 'Pendiente',
        [EmailDestino] NVARCHAR(200) NULL,
        [Firmado] BIT NOT NULL DEFAULT 0,
        [FechaEnvio] DATETIME NULL,
        [FechaRecepcion] DATETIME NULL
    );
    PRINT 'Tabla PedidoInsumo creada.';
END
GO

-- =============================================
-- 11. TABLA STOCKINSUMO (Inventario)
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StockInsumo]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[StockInsumo](
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [TipoInsumo] NVARCHAR(100) NOT NULL UNIQUE,
        [CantidadActual] INT NOT NULL DEFAULT 0,
        [StockMinimo] INT NOT NULL DEFAULT 10,
        [UltimaActualizacion] DATETIME NOT NULL DEFAULT GETDATE()
    );
    
    -- Insertar stock inicial
    INSERT INTO StockInsumo (TipoInsumo, CantidadActual, StockMinimo) VALUES 
    ('Hojas Protocolares', 100, 50),
    ('Sellos', 20, 5),
    ('Papel Membretado', 500, 100),
    ('Sobres', 200, 50),
    ('Carpetas', 50, 10);

    PRINT 'Tabla StockInsumo creada.';
END
GO

-- =============================================
-- VISTAS Y PROCEDIMIENTOS ALMACENADOS
-- =============================================

PRINT 'Creando vistas y procedimientos...';
GO

-- Vista vw_EscriturasCompletas
IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_EscriturasCompletas') DROP VIEW vw_EscriturasCompletas;
GO

CREATE VIEW vw_EscriturasCompletas AS
SELECT 
    e.Id,
    e.NumeroProtocolo,
    e.Fecha,
    e.Estado,
    e.FormatoArchivo,
    e.RutaArchivo,
    e.Observaciones,
    e.FechaCreacion,
    e.FechaProtocolizacion,
    e.UsuarioCreacion,
    e.ClienteId,
    c.Nombre + ' ' + c.Apellido AS NombreCliente,
    c.DNI AS ClienteDNI,
    c.Telefono AS ClienteTelefono,
    c.Email AS ClienteEmail,
    c.Direccion AS ClienteDireccion,
    e.TipoEscrituraId,
    te.Nombre AS TipoEscritura,
    te.Descripcion AS TipoDescripcion,
    te.RutaPlantilla,
    e.TramiteId,
    e.CobroId
FROM Escritura e
INNER JOIN Cliente c ON e.ClienteId = c.Id
INNER JOIN TipoEscritura te ON e.TipoEscrituraId = te.Id;
GO

PRINT '✓ Vista vw_EscriturasCompletas creada.';
GO

-- Trigger para cerrar estados automáticamente
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_ActualizarFechaProtocolizacion') DROP TRIGGER trg_ActualizarFechaProtocolizacion;
GO

CREATE TRIGGER trg_ActualizarFechaProtocolizacion
ON Escritura
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE Escritura
    SET FechaProtocolizacion = GETDATE()
    FROM Escritura e
    INNER JOIN inserted i ON e.Id = i.Id
    INNER JOIN deleted d ON e.Id = d.Id
    WHERE i.Estado = 'Protocolizada'
      AND d.Estado <> 'Protocolizada'
      AND e.FechaProtocolizacion IS NULL;
END
GO

PRINT '✓ Trigger de protocolización creado.';
GO

PRINT '========================================';
PRINT '✓✓✓ BASE DE DATOS GENERADA EXITOSAMENTE ✓✓✓';
PRINT '========================================';
