USE NotalegisDB;
GO

-- Migración: Agrega columnas dedicadas para "Otra Gestión" en Cobros
-- (antes el importe quedaba mezclado en el subtotal, sin poder identificarse en reportes ni en el comprobante PDF).
-- Ejecutar una sola vez sobre una instalación existente.

PRINT '========================================';
PRINT 'Migración 10: Otra Gestión en Cobros';
PRINT '========================================';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Cobros') AND name = 'OtraGestionNombre')
BEGIN
    ALTER TABLE Cobros ADD OtraGestionNombre NVARCHAR(200) NULL;
    PRINT 'Columna OtraGestionNombre agregada.';
END
ELSE
    PRINT 'Columna OtraGestionNombre ya existía, no se modificó.';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Cobros') AND name = 'OtraGestionImporte')
BEGIN
    ALTER TABLE Cobros ADD OtraGestionImporte DECIMAL(18,2) DEFAULT 0 WITH VALUES;
    PRINT 'Columna OtraGestionImporte agregada.';
END
ELSE
BEGIN
    -- Por si la columna ya existía sin backfill (DEFAULT sin WITH VALUES no rellena filas existentes)
    UPDATE Cobros SET OtraGestionImporte = 0 WHERE OtraGestionImporte IS NULL;
    PRINT 'Columna OtraGestionImporte ya existía; filas con NULL actualizadas a 0.';
END

PRINT 'Migración completada.';
GO
