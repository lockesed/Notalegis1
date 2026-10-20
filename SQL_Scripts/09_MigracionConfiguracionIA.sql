USE NotalegisDB;
GO

-- Migración: Agrega claves de configuración para datos notariales e IA avanzada
-- Ejecutar una sola vez sobre una instalación existente.
-- Si la clave ya existe, no hace nada (MERGE con condición NOT MATCHED).

PRINT '========================================';
PRINT 'Migración 09: Configuración notarial e IA';
PRINT '========================================';

-- Helper: insertar solo si la clave no existe
MERGE Configuracion AS target
USING (VALUES
    -- Datos notariales (usados por el módulo de IA al generar escrituras)
    ('Escribania_Ciudad',          '',     'General', 'Ciudad de otorgamiento de actos notariales'),
    ('Escribania_Provincia',       '',     'General', 'Provincia donde funciona la escribanía'),
    ('Escribania_NumeroRegistro',  '',     'General', 'Número de Registro Notarial (solo el número)'),
    ('Escribania_Escribano',       '',     'General', 'Nombre completo del escribano/a titular'),

    -- IA avanzada
    ('IA_MaxTokens',               '8192', 'IA',      'Límite de tokens por respuesta (mín. 1024; Haiku soporta hasta 8192, Sonnet hasta 16384)'),
    ('IA_RutaNormativaPDF',        '',     'IA',      'Ruta local al PDF de normativa vigente del Colegio de Escribanos')
) AS source (Clave, Valor, TipoConfiguracion, Descripcion)
ON target.Clave = source.Clave
WHEN NOT MATCHED THEN
    INSERT (Clave, Valor, TipoConfiguracion, Descripcion, FechaActualizacion)
    VALUES (source.Clave, source.Valor, source.TipoConfiguracion, source.Descripcion, GETDATE());

PRINT 'Migración completada. Filas procesadas: ' + CAST(@@ROWCOUNT AS VARCHAR);
GO
