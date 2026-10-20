-- Script para crear la tabla Cobros en NotalegisDB
-- Ejecutar este script en SQL Server Management Studio

USE NotalegisDB;
GO

-- Verificar si existe la tabla Cliente y su estructura
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Cliente')
BEGIN
    PRINT 'ERROR: La tabla Cliente no existe. Debe crearla primero.';
    RETURN;
END

-- Verificar si la tabla Cliente tiene la columna Id
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Cliente' AND COLUMN_NAME = 'Id')
BEGIN
    PRINT 'ERROR: La tabla Cliente no tiene una columna Id. Verifique la estructura.';
    RETURN;
END

PRINT 'Verificación exitosa: La tabla Cliente existe con columna Id.';

-- Crear tabla Cobros para almacenar el historial de facturación
CREATE TABLE Cobros (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ClienteId INT NOT NULL,
    NumeroExpediente NVARCHAR(50) NOT NULL,
    FechaCobro DATETIME NOT NULL DEFAULT GETDATE(),
    TipoTramite NVARCHAR(100),
    
    -- Impuestos y Honorarios (Sección 1)
    ImpGananciasPorc DECIMAL(5,2) DEFAULT 0,
    ImpGananciasBase DECIMAL(18,2) DEFAULT 0,
    ImpGananciasImporte DECIMAL(18,2) DEFAULT 0,
    
    ITIPorc DECIMAL(5,2) DEFAULT 0,
    ITIBase DECIMAL(18,2) DEFAULT 0,
    ITIImporte DECIMAL(18,2) DEFAULT 0,
    
    ImpSellosPorc DECIMAL(5,2) DEFAULT 0,
    ImpSellosBase DECIMAL(18,2) DEFAULT 0,
    ImpSellosImporte DECIMAL(18,2) DEFAULT 0,
    
    TasaGeneralPorc DECIMAL(5,2) DEFAULT 0,
    TasaGeneralBase DECIMAL(18,2) DEFAULT 0,
    TasaGeneralImporte DECIMAL(18,2) DEFAULT 0,
    
    SelloMatrizPorc DECIMAL(5,2) DEFAULT 0,
    SelloMatrizBase DECIMAL(18,2) DEFAULT 0,
    SelloMatrizImporte DECIMAL(18,2) DEFAULT 0,
    
    CertificadosPorc DECIMAL(5,2) DEFAULT 0,
    CertificadosBase DECIMAL(18,2) DEFAULT 0,
    CertificadosImporte DECIMAL(18,2) DEFAULT 0,
    
    DiligenciamientoPorc DECIMAL(5,2) DEFAULT 0,
    DiligenciamientoBase DECIMAL(18,2) DEFAULT 0,
    DiligenciamientoImporte DECIMAL(18,2) DEFAULT 0,
    
    HonorariosPorc DECIMAL(5,2) DEFAULT 0,
    HonorariosBase DECIMAL(18,2) DEFAULT 0,
    HonorariosImporte DECIMAL(18,2) DEFAULT 0,
    
    IVAPorc DECIMAL(5,2) DEFAULT 0,
    IVABase DECIMAL(18,2) DEFAULT 0,
    IVAImporte DECIMAL(18,2) DEFAULT 0,
    
    -- Certificados y Gestiones (Sección 2)
    TituloPropiedad DECIMAL(18,2) DEFAULT 0,
    VEP DECIMAL(18,2) DEFAULT 0,
    LibreDeudaInmueble DECIMAL(18,2) DEFAULT 0,
    LibreDeudaJudicial DECIMAL(18,2) DEFAULT 0,
    LibreDeudaAgua DECIMAL(18,2) DEFAULT 0,
    COTI DECIMAL(18,2) DEFAULT 0,
    SentenciaDivorcio DECIMAL(18,2) DEFAULT 0,
    Conformidad DECIMAL(18,2) DEFAULT 0,
    SaldoDGR DECIMAL(18,2) DEFAULT 0,
    DecretoAdjudicaciones DECIMAL(18,2) DEFAULT 0,
    
    -- Totales
    SubtotalImpuestos DECIMAL(18,2) NOT NULL DEFAULT 0,
    SubtotalCertificados DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalGeneral DECIMAL(18,2) NOT NULL DEFAULT 0,
    
    -- Información adicional
    Estado NVARCHAR(20) NOT NULL DEFAULT 'Pendiente', -- Pendiente, Pagado, Parcial
    MetodoPago NVARCHAR(50),
    Observaciones NVARCHAR(500),
    
    -- Auditoría
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
    FechaActualizacion DATETIME,
    UsuarioCreacion NVARCHAR(100),
    UsuarioActualizacion NVARCHAR(100),
    
    -- Clave foránea
    CONSTRAINT FK_Cobros_Cliente FOREIGN KEY (ClienteId) REFERENCES Cliente(Id)
);
GO

-- Crear índices para mejorar el rendimiento
CREATE INDEX IX_Cobros_ClienteId ON Cobros(ClienteId);
CREATE INDEX IX_Cobros_FechaCobro ON Cobros(FechaCobro);
CREATE INDEX IX_Cobros_NumeroExpediente ON Cobros(NumeroExpediente);
CREATE INDEX IX_Cobros_Estado ON Cobros(Estado);
GO

-- Crear vista para consultas frecuentes
CREATE VIEW vw_CobrosConCliente AS
SELECT 
    c.Id,
    c.ClienteId,
    cl.Nombre + ' ' + cl.Apellido AS NombreCliente,
    cl.DNI,
    c.NumeroExpediente,
    c.FechaCobro,
    c.TipoTramite,
    c.SubtotalImpuestos,
    c.SubtotalCertificados,
    c.TotalGeneral,
    c.Estado,
    c.MetodoPago,
    c.Observaciones,
    c.FechaCreacion
FROM Cobros c
INNER JOIN Cliente cl ON c.ClienteId = cl.Id;
GO

PRINT 'Tabla Cobros creada exitosamente con índices y vista.';
