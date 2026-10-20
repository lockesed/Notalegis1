-- Agrega soporte de bloqueo por intentos fallidos de login a la tabla Usuarios.
-- Ejecutar una sola vez sobre NotalegisDB.

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Usuarios' AND COLUMN_NAME = 'IntentosLogin'
)
BEGIN
    ALTER TABLE Usuarios ADD IntentosLogin INT NOT NULL DEFAULT 0;
END

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Usuarios' AND COLUMN_NAME = 'BloqueadoHasta'
)
BEGIN
    ALTER TABLE Usuarios ADD BloqueadoHasta DATETIME NULL;
END
