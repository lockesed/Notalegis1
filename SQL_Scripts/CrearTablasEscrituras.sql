-- =============================================
-- Script para crear las tablas del Módulo de Escrituras
-- Base de datos: NotalegisDB
-- Autor: Sistema Notalegis
-- Fecha: Noviembre 2024
-- =============================================

USE NotalegisDB;
GO

PRINT '========================================';
PRINT 'Iniciando creación de tablas de Escrituras';
PRINT '========================================';
GO

-- =============================================
-- Verificar que existe la tabla Cliente
-- =============================================
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Cliente')
BEGIN
    PRINT 'ERROR: La tabla Cliente no existe. Debe crearla primero.';
    RETURN;
END

PRINT '✓ Verificación exitosa: La tabla Cliente existe.';
GO

-- =============================================
-- TABLA 1: TipoEscritura (Catálogo de tipos)
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
    
    PRINT '✓ Tabla TipoEscritura creada exitosamente.';
END
ELSE
BEGIN
    PRINT '⚠ La tabla TipoEscritura ya existe.';
END
GO

-- =============================================
-- TABLA 2: Escritura (Tabla principal)
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
        [Estado] NVARCHAR(50) NOT NULL DEFAULT 'Borrador', -- Borrador, Finalizada, Protocolizada, Archivada
        [TramiteId] INT NULL, -- Relación opcional con Tramite
        [CobroId] INT NULL, -- Relación opcional con Cobros
        [Observaciones] NVARCHAR(MAX) NULL,
        [FechaCreacion] DATETIME NOT NULL DEFAULT GETDATE(),
        [FechaProtocolizacion] DATETIME NULL,
        [UsuarioCreacion] NVARCHAR(100) NULL,
        
        -- Claves foráneas
        CONSTRAINT FK_Escritura_Cliente FOREIGN KEY (ClienteId) REFERENCES Cliente(Id),
        CONSTRAINT FK_Escritura_TipoEscritura FOREIGN KEY (TipoEscrituraId) REFERENCES TipoEscritura(Id)
    );
    
    PRINT '✓ Tabla Escritura creada exitosamente.';
END
ELSE
BEGIN
    PRINT '⚠ La tabla Escritura ya existe.';
END
GO

-- =============================================
-- ÍNDICES para optimizar consultas
-- =============================================
PRINT 'Creando índices...';
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Escritura_ClienteId' AND object_id = OBJECT_ID('Escritura'))
BEGIN
    CREATE INDEX IX_Escritura_ClienteId ON Escritura(ClienteId);
    PRINT '✓ Índice IX_Escritura_ClienteId creado.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Escritura_TipoEscrituraId' AND object_id = OBJECT_ID('Escritura'))
BEGIN
    CREATE INDEX IX_Escritura_TipoEscrituraId ON Escritura(TipoEscrituraId);
    PRINT '✓ Índice IX_Escritura_TipoEscrituraId creado.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Escritura_NumeroProtocolo' AND object_id = OBJECT_ID('Escritura'))
BEGIN
    CREATE INDEX IX_Escritura_NumeroProtocolo ON Escritura(NumeroProtocolo);
    PRINT '✓ Índice IX_Escritura_NumeroProtocolo creado.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Escritura_Fecha' AND object_id = OBJECT_ID('Escritura'))
BEGIN
    CREATE INDEX IX_Escritura_Fecha ON Escritura(Fecha);
    PRINT '✓ Índice IX_Escritura_Fecha creado.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Escritura_Estado' AND object_id = OBJECT_ID('Escritura'))
BEGIN
    CREATE INDEX IX_Escritura_Estado ON Escritura(Estado);
    PRINT '✓ Índice IX_Escritura_Estado creado.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Escritura_TramiteId' AND object_id = OBJECT_ID('Escritura'))
BEGIN
    CREATE INDEX IX_Escritura_TramiteId ON Escritura(TramiteId);
    PRINT '✓ Índice IX_Escritura_TramiteId creado.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Escritura_CobroId' AND object_id = OBJECT_ID('Escritura'))
BEGIN
    CREATE INDEX IX_Escritura_CobroId ON Escritura(CobroId);
    PRINT '✓ Índice IX_Escritura_CobroId creado.';
END
GO

-- =============================================
-- VISTA para consultas con JOIN
-- =============================================
IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_EscriturasCompletas')
BEGIN
    DROP VIEW vw_EscriturasCompletas;
    PRINT '⚠ Vista vw_EscriturasCompletas eliminada para recrear.';
END
GO

CREATE VIEW vw_EscriturasCompletas AS
SELECT 
    -- Datos de la Escritura
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
    
    -- Datos del Cliente
    e.ClienteId,
    c.Nombre + ' ' + c.Apellido AS NombreCliente,
    c.DNI AS ClienteDNI,
    c.Telefono AS ClienteTelefono,
    c.Email AS ClienteEmail,
    c.Direccion AS ClienteDireccion,
    
    -- Datos del Tipo de Escritura
    e.TipoEscrituraId,
    te.Nombre AS TipoEscritura,
    te.Descripcion AS TipoDescripcion,
    te.RutaPlantilla,
    
    -- Relaciones opcionales
    e.TramiteId,
    e.CobroId
FROM Escritura e
INNER JOIN Cliente c ON e.ClienteId = c.Id
INNER JOIN TipoEscritura te ON e.TipoEscrituraId = te.Id;
GO

PRINT '✓ Vista vw_EscriturasCompletas creada exitosamente.';
GO

-- =============================================
-- DATOS INICIALES: Tipos de Escritura
-- =============================================
IF NOT EXISTS (SELECT * FROM TipoEscritura)
BEGIN
    PRINT 'Insertando tipos de escritura predefinidos...';
    
    INSERT INTO TipoEscritura (Nombre, Descripcion, Activo)
    VALUES 
        ('Escritura de Compraventa', 'Transferencia de propiedad de inmueble mediante compraventa', 1),
        ('Constitución de Sociedad', 'Creación de persona jurídica (SRL, SA, etc.)', 1),
        ('Poder General', 'Otorgamiento de representación legal amplia', 1),
        ('Poder Especial', 'Otorgamiento de representación legal específica', 1),
        ('Testamento', 'Disposición de última voluntad del testador', 1),
        ('Donación', 'Transferencia gratuita de bienes', 1),
        ('Hipoteca', 'Constitución de garantía real sobre inmueble', 1),
        ('Cancelación de Hipoteca', 'Levantamiento de garantía hipotecaria', 1),
        ('Usufructo', 'Derecho de uso y goce de bien ajeno', 1),
        ('Servidumbre', 'Gravamen sobre inmueble en beneficio de otro', 1),
        ('Declaratoria de Herederos', 'Sucesión intestada - determinación de herederos', 1),
        ('Protocolización', 'Incorporación de documento al protocolo notarial', 1),
        ('Cesión de Derechos', 'Transferencia de derechos y acciones', 1),
        ('Constitución de Bien de Familia', 'Protección del inmueble familiar', 1),
        ('Acta Notarial', 'Constancia de hechos o actos jurídicos', 1);
    
    PRINT '✓ ' + CAST(@@ROWCOUNT AS VARCHAR) + ' tipos de escritura insertados.';
END
ELSE
BEGIN
    PRINT '⚠ Ya existen tipos de escritura en la base de datos.';
END
GO

-- =============================================
-- STORED PROCEDURE: Obtener escrituras por cliente
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_ObtenerEscriturasPorCliente')
    DROP PROCEDURE sp_ObtenerEscriturasPorCliente;
GO

CREATE PROCEDURE sp_ObtenerEscriturasPorCliente
    @ClienteId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT * 
    FROM vw_EscriturasCompletas
    WHERE ClienteId = @ClienteId
    ORDER BY Fecha DESC;
END
GO

PRINT '✓ Stored Procedure sp_ObtenerEscriturasPorCliente creado.';
GO

-- =============================================
-- STORED PROCEDURE: Buscar escrituras
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_BuscarEscrituras')
    DROP PROCEDURE sp_BuscarEscrituras;
GO

CREATE PROCEDURE sp_BuscarEscrituras
    @Filtro NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT * 
    FROM vw_EscriturasCompletas
    WHERE NombreCliente LIKE '%' + @Filtro + '%'
       OR NumeroProtocolo LIKE '%' + @Filtro + '%'
       OR TipoEscritura LIKE '%' + @Filtro + '%'
       OR ClienteDNI LIKE '%' + @Filtro + '%'
    ORDER BY Fecha DESC;
END
GO

PRINT '✓ Stored Procedure sp_BuscarEscrituras creado.';
GO

-- =============================================
-- STORED PROCEDURE: Obtener estadísticas
-- =============================================
IF EXISTS (SELECT * FROM sys.procedures WHERE name = 'sp_EstadisticasEscrituras')
    DROP PROCEDURE sp_EstadisticasEscrituras;
GO

CREATE PROCEDURE sp_EstadisticasEscrituras
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Total de escrituras por estado
    SELECT 
        Estado,
        COUNT(*) AS Cantidad
    FROM Escritura
    GROUP BY Estado;
    
    -- Total de escrituras por tipo
    SELECT 
        te.Nombre AS TipoEscritura,
        COUNT(e.Id) AS Cantidad
    FROM TipoEscritura te
    LEFT JOIN Escritura e ON te.Id = e.TipoEscrituraId
    GROUP BY te.Nombre
    ORDER BY Cantidad DESC;
    
    -- Escrituras por mes (último año)
    SELECT 
        YEAR(Fecha) AS Año,
        MONTH(Fecha) AS Mes,
        COUNT(*) AS Cantidad
    FROM Escritura
    WHERE Fecha >= DATEADD(YEAR, -1, GETDATE())
    GROUP BY YEAR(Fecha), MONTH(Fecha)
    ORDER BY Año DESC, Mes DESC;
END
GO

PRINT '✓ Stored Procedure sp_EstadisticasEscrituras creado.';
GO

-- =============================================
-- TRIGGER: Actualizar fecha de protocolización
-- =============================================
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_ActualizarFechaProtocolizacion')
    DROP TRIGGER trg_ActualizarFechaProtocolizacion;
GO

CREATE TRIGGER trg_ActualizarFechaProtocolizacion
ON Escritura
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Si el estado cambió a 'Protocolizada' y no tiene fecha de protocolización
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

PRINT '✓ Trigger trg_ActualizarFechaProtocolizacion creado.';
GO

-- =============================================
-- VERIFICACIÓN FINAL
-- =============================================
PRINT '';
PRINT '========================================';
PRINT 'VERIFICACIÓN FINAL';
PRINT '========================================';

-- Contar tablas creadas
DECLARE @TablasCreadas INT = 0;
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'TipoEscritura' AND type = 'U')
    SET @TablasCreadas = @TablasCreadas + 1;
IF EXISTS (SELECT * FROM sys.objects WHERE name = 'Escritura' AND type = 'U')
    SET @TablasCreadas = @TablasCreadas + 1;

PRINT '✓ Tablas creadas: ' + CAST(@TablasCreadas AS VARCHAR) + '/2';

-- Contar índices creados
DECLARE @IndicesCreados INT;
SELECT @IndicesCreados = COUNT(*)
FROM sys.indexes
WHERE object_id IN (OBJECT_ID('Escritura'))
  AND name LIKE 'IX_Escritura_%';

PRINT '✓ Índices creados: ' + CAST(@IndicesCreados AS VARCHAR) + '/7';

-- Contar tipos de escritura
DECLARE @TiposEscritura INT;
SELECT @TiposEscritura = COUNT(*) FROM TipoEscritura;

PRINT '✓ Tipos de escritura: ' + CAST(@TiposEscritura AS VARCHAR);

-- Verificar vista
IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_EscriturasCompletas')
    PRINT '✓ Vista vw_EscriturasCompletas: OK';

-- Verificar stored procedures
DECLARE @StoredProcs INT;
SELECT @StoredProcs = COUNT(*)
FROM sys.procedures
WHERE name IN ('sp_ObtenerEscriturasPorCliente', 'sp_BuscarEscrituras', 'sp_EstadisticasEscrituras');

PRINT '✓ Stored Procedures creados: ' + CAST(@StoredProcs AS VARCHAR) + '/3';

-- Verificar trigger
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_ActualizarFechaProtocolizacion')
    PRINT '✓ Trigger trg_ActualizarFechaProtocolizacion: OK';

PRINT '';
PRINT '========================================';
PRINT '✓✓✓ SCRIPT COMPLETADO EXITOSAMENTE ✓✓✓';
PRINT '========================================';
PRINT '';
PRINT 'Próximos pasos:';
PRINT '1. Crear carpeta: C:\Notalegis\Escrituras\Plantillas\';
PRINT '2. Agregar plantillas .docx en la carpeta';
PRINT '3. Ejecutar la aplicación y probar el módulo';
PRINT '';
GO
