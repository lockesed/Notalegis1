USE NotalegisDB;
GO

-- Deja las 5 cuentas de prueba con roles válidos y una contraseña conocida (Test1234)
-- para poder loguearse y probar los 4 niveles de acceso durante la presentación.

UPDATE Usuarios SET Rol = N'Escribana Adjunta' WHERE NombreUsuario = N'legal1';

UPDATE Usuarios
SET Contraseña = N'$2a$11$6BU5co/FphMis1x8gmfs4OfiPR9DtwDZze8KeSeA2zd5PxabVgwHu' -- Test1234
WHERE NombreUsuario IN (N'admin', N'escribano', N'gestor1', N'secretaria1', N'legal1');

PRINT 'Usuarios de prueba actualizados (contraseña: Test1234).';
GO
