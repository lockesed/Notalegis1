-- ============================================================
-- Notalegis - Índices de rendimiento
-- Ejecutar UNA SOLA VEZ sobre NotalegisDB
-- Es seguro ejecutarlo de nuevo (usa IF NOT EXISTS)
-- ============================================================

-- Índice en Tramites.FechaVencimiento
-- Acelera filtros de vencimientos y la ventana VencimientosHoy
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Tramites_FechaVencimiento' AND object_id = OBJECT_ID('Tramites'))
BEGIN
    CREATE INDEX IX_Tramites_FechaVencimiento ON Tramites(FechaVencimiento);
    PRINT 'Índice IX_Tramites_FechaVencimiento creado.';
END
ELSE
    PRINT 'Índice IX_Tramites_FechaVencimiento ya existía, se omite.';

-- Índice en Tramites.Estado
-- Acelera los filtros por estado en TramitesView
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Tramites_Estado' AND object_id = OBJECT_ID('Tramites'))
BEGIN
    CREATE INDEX IX_Tramites_Estado ON Tramites(Estado);
    PRINT 'Índice IX_Tramites_Estado creado.';
END
ELSE
    PRINT 'Índice IX_Tramites_Estado ya existía, se omite.';

-- Índice en Tramites.IdCliente (si existe la columna; depende del esquema)
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Tramites' AND COLUMN_NAME = 'IdCliente')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Tramites_IdCliente' AND object_id = OBJECT_ID('Tramites'))
    BEGIN
        CREATE INDEX IX_Tramites_IdCliente ON Tramites(IdCliente);
        PRINT 'Índice IX_Tramites_IdCliente creado.';
    END
    ELSE
        PRINT 'Índice IX_Tramites_IdCliente ya existía, se omite.';
END

-- Índice en Turnos.FechaHora
-- Acelera la carga del calendario en MainView y TurnosView
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Turnos_FechaHora' AND object_id = OBJECT_ID('Turnos'))
BEGIN
    CREATE INDEX IX_Turnos_FechaHora ON Turnos(FechaHora);
    PRINT 'Índice IX_Turnos_FechaHora creado.';
END
ELSE
    PRINT 'Índice IX_Turnos_FechaHora ya existía, se omite.';

-- Índice en Turnos.IdCliente
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Turnos' AND COLUMN_NAME = 'IdCliente')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Turnos_IdCliente' AND object_id = OBJECT_ID('Turnos'))
    BEGIN
        CREATE INDEX IX_Turnos_IdCliente ON Turnos(IdCliente);
        PRINT 'Índice IX_Turnos_IdCliente creado.';
    END
    ELSE
        PRINT 'Índice IX_Turnos_IdCliente ya existía, se omite.';
END

-- Índice en Cobros.IdCliente
-- Acelera el historial de cobros filtrado por cliente
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Cobros' AND COLUMN_NAME = 'IdCliente')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Cobros_IdCliente' AND object_id = OBJECT_ID('Cobros'))
    BEGIN
        CREATE INDEX IX_Cobros_IdCliente ON Cobros(IdCliente);
        PRINT 'Índice IX_Cobros_IdCliente creado.';
    END
    ELSE
        PRINT 'Índice IX_Cobros_IdCliente ya existía, se omite.';
END

PRINT '';
PRINT '=== Proceso de creación de índices finalizado ===';
