-- Script para crear la tabla Pagos en NotalegisDB
-- Esta tabla registra los pagos realizados para cada cobro

USE NotalegisDB;
GO

-- Crear tabla Pagos si no existe
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Pagos' AND xtype='U')
BEGIN
    CREATE TABLE Pagos (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CobroId INT NOT NULL,
        MontoPagado DECIMAL(18,2) NOT NULL,
        FechaPago DATETIME NOT NULL,
        MetodoPago NVARCHAR(50) NULL,
        NumeroComprobante NVARCHAR(100) NULL,
        Observaciones NVARCHAR(500) NULL,
        UsuarioRegistro NVARCHAR(100) NULL,
        FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
        
        -- Clave foránea hacia la tabla Cobros
        CONSTRAINT FK_Pagos_Cobros FOREIGN KEY (CobroId) REFERENCES Cobros(Id)
    );
    
    -- Crear índices para mejorar el rendimiento
    CREATE INDEX IX_Pagos_CobroId ON Pagos(CobroId);
    CREATE INDEX IX_Pagos_FechaPago ON Pagos(FechaPago);
    
    PRINT 'Tabla Pagos creada exitosamente con índices.';
END
ELSE
BEGIN
    PRINT 'La tabla Pagos ya existe.';
END

-- Verificar la estructura de la tabla
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Pagos'
ORDER BY ORDINAL_POSITION;
