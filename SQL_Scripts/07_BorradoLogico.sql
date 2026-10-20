-- ============================================================
-- Notalegis - Borrado lógico (FASE 1.9)
-- Ejecutar UNA SOLA VEZ sobre NotalegisDB
-- Es seguro ejecutarlo de nuevo (usa IF NOT EXISTS)
--
-- QUÉ HACE: Agrega una columna "Eliminado" (BIT, default 0) a las
-- tablas principales. Los DAOs se actualizan para filtrar registros
-- con Eliminado = 1 y para marcarlos en lugar de borrarlos físicamente.
-- ============================================================

-- Cliente
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Cliente' AND COLUMN_NAME = 'Eliminado')
BEGIN
    ALTER TABLE Cliente ADD Eliminado BIT NOT NULL DEFAULT 0;
    PRINT 'Columna Eliminado agregada a Cliente.';
END
ELSE
    PRINT 'Cliente.Eliminado ya existía, se omite.';

-- tramite
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'tramite' AND COLUMN_NAME = 'Eliminado')
BEGIN
    ALTER TABLE tramite ADD Eliminado BIT NOT NULL DEFAULT 0;
    PRINT 'Columna Eliminado agregada a tramite.';
END
ELSE
    PRINT 'tramite.Eliminado ya existía, se omite.';

-- Turno
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Turno' AND COLUMN_NAME = 'Eliminado')
BEGIN
    ALTER TABLE Turno ADD Eliminado BIT NOT NULL DEFAULT 0;
    PRINT 'Columna Eliminado agregada a Turno.';
END
ELSE
    PRINT 'Turno.Eliminado ya existía, se omite.';

-- Escritura
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Escritura')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Escritura' AND COLUMN_NAME = 'Eliminado')
    BEGIN
        ALTER TABLE Escritura ADD Eliminado BIT NOT NULL DEFAULT 0;
        PRINT 'Columna Eliminado agregada a Escritura.';
    END
    ELSE
        PRINT 'Escritura.Eliminado ya existía, se omite.';
END

-- PedidoInsumo
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PedidoInsumo')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PedidoInsumo' AND COLUMN_NAME = 'Eliminado')
    BEGIN
        ALTER TABLE PedidoInsumo ADD Eliminado BIT NOT NULL DEFAULT 0;
        PRINT 'Columna Eliminado agregada a PedidoInsumo.';
    END
    ELSE
        PRINT 'PedidoInsumo.Eliminado ya existía, se omite.';
END

-- Cobros
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Cobros')
BEGIN
    IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Cobros' AND COLUMN_NAME = 'Eliminado')
    BEGIN
        ALTER TABLE Cobros ADD Eliminado BIT NOT NULL DEFAULT 0;
        PRINT 'Columna Eliminado agregada a Cobros.';
    END
    ELSE
        PRINT 'Cobros.Eliminado ya existía, se omite.';
END

PRINT '';
PRINT '=== Borrado lógico configurado. ===';
PRINT 'IMPORTANTE: Los DAOs deben actualizarse para:';
PRINT '  1. Filtrar WHERE Eliminado = 0 en los SELECT';
PRINT '  2. Cambiar DELETE por UPDATE ... SET Eliminado = 1';
PRINT 'Ver sección FASE 1.9 del plan de mejoras.';
