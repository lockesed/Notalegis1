-- ============================================================
-- Notalegis - Tabla de Auditoría (FASE 1.3)
-- Ejecutar UNA SOLA VEZ sobre NotalegisDB
-- ============================================================

IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'RegistrosAuditoria')
BEGIN
    CREATE TABLE RegistrosAuditoria (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        FechaHora       DATETIME NOT NULL DEFAULT GETDATE(),
        Usuario         NVARCHAR(100) NOT NULL,
        Accion          NVARCHAR(50)  NOT NULL,  -- 'ALTA', 'MODIFICACION', 'BAJA'
        Modulo          NVARCHAR(100) NOT NULL,  -- 'Clientes', 'Tramites', 'Turnos', etc.
        EntidadId       INT NULL,
        Descripcion     NVARCHAR(500) NULL
    );

    CREATE INDEX IX_RegistrosAuditoria_FechaHora ON RegistrosAuditoria(FechaHora DESC);
    CREATE INDEX IX_RegistrosAuditoria_Usuario   ON RegistrosAuditoria(Usuario);

    PRINT 'Tabla RegistrosAuditoria creada correctamente.';
END
ELSE
    PRINT 'RegistrosAuditoria ya existía, se omite.';

PRINT '=== Tabla de auditoría lista. ===';
