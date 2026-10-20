-- Script para crear las tablas de Pedidos de Insumos
-- Base de datos: Notalegis

USE Notalegis;
GO

-- Tabla de Pedidos de Insumos
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
    
    PRINT 'Tabla PedidoInsumo creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla PedidoInsumo ya existe.';
END
GO

-- Tabla de Stock de Insumos
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[StockInsumo]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[StockInsumo](
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [TipoInsumo] NVARCHAR(100) NOT NULL UNIQUE,
        [CantidadActual] INT NOT NULL DEFAULT 0,
        [StockMinimo] INT NOT NULL DEFAULT 10,
        [UltimaActualizacion] DATETIME NOT NULL DEFAULT GETDATE()
    );
    
    PRINT 'Tabla StockInsumo creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla StockInsumo ya existe.';
END
GO

-- Insertar datos iniciales de stock (opcional)
IF NOT EXISTS (SELECT * FROM StockInsumo WHERE TipoInsumo = 'Hojas Protocolares')
BEGIN
    INSERT INTO StockInsumo (TipoInsumo, CantidadActual, StockMinimo, UltimaActualizacion)
    VALUES 
        ('Hojas Protocolares', 100, 50, GETDATE()),
        ('Sellos', 20, 5, GETDATE()),
        ('Papel Membretado', 500, 100, GETDATE()),
        ('Sobres', 200, 50, GETDATE()),
        ('Carpetas', 50, 10, GETDATE()),
        ('Formularios', 150, 30, GETDATE());
    
    PRINT 'Datos iniciales de stock insertados.';
END
GO

-- Crear índices para mejorar el rendimiento
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PedidoInsumo_FechaPedido')
BEGIN
    CREATE INDEX IX_PedidoInsumo_FechaPedido ON PedidoInsumo(FechaPedido);
    PRINT 'Índice IX_PedidoInsumo_FechaPedido creado.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_PedidoInsumo_Estado')
BEGIN
    CREATE INDEX IX_PedidoInsumo_Estado ON PedidoInsumo(Estado);
    PRINT 'Índice IX_PedidoInsumo_Estado creado.';
END
GO

PRINT 'Script completado exitosamente.';
GO
