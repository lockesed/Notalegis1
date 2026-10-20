USE NotalegisDB;
GO

-- Carga 50 clientes de ejemplo (datos de relleno para la presentación).

;WITH Nombres AS (
    SELECT Nombre, ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn FROM (VALUES
        (N'Diego'),(N'Natalia'),(N'Lucas'),(N'Pablo'),(N'Florencia'),(N'Valeria'),(N'Marina'),(N'Ariel'),(N'Gabriel'),(N'Sofía'),
        (N'Martín'),(N'Camila'),(N'Nicolás'),(N'Julieta'),(N'Federico'),(N'Agustina'),(N'Tomás'),(N'Milagros'),(N'Santiago'),(N'Rocío'),
        (N'Franco'),(N'Micaela'),(N'Emiliano'),(N'Antonella'),(N'Bruno'),(N'Lucía'),(N'Matías'),(N'Carla'),(N'Joaquín'),(N'Daniela'),
        (N'Alejandro'),(N'Victoria'),(N'Gonzalo'),(N'Paula'),(N'Ramiro'),(N'Belén'),(N'Ezequiel'),(N'Constanza'),(N'Ignacio'),(N'Delfina'),
        (N'Maximiliano'),(N'Guadalupe'),(N'Rodrigo'),(N'Melina'),(N'Cristian'),(N'Yamila'),(N'Sebastián'),(N'Abril'),(N'Leandro'),(N'Elena')
    ) AS T(Nombre)
),
Apellidos AS (
    SELECT Apellido, ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn FROM (VALUES
        (N'Martínez'),(N'Acosta'),(N'Ríos'),(N'Herrera'),(N'Ibáñez'),(N'Torres'),(N'Castro'),(N'Graziano'),(N'Fernández'),(N'Gómez'),
        (N'Pereyra'),(N'Sosa'),(N'Molina'),(N'Suárez'),(N'Romero'),(N'Ledesma'),(N'Aguirre'),(N'Benítez'),(N'Cabrera'),(N'Domínguez'),
        (N'Escobar'),(N'Flores'),(N'Giménez'),(N'Herrera'),(N'Juárez'),(N'Luna'),(N'Medina'),(N'Núñez'),(N'Ortiz'),(N'Paz'),
        (N'Quiroga'),(N'Rodríguez'),(N'Sánchez'),(N'Toledo'),(N'Urquiza'),(N'Vega'),(N'Weber'),(N'Ximénez'),(N'Yañez'),(N'Zárate'),
        (N'Aráoz'),(N'Bustos'),(N'Correa'),(N'Duarte'),(N'Espínola'),(N'Farías'),(N'Godoy'),(N'Heredia'),(N'Ibarra'),(N'Jofré')
    ) AS T(Apellido)
),
Calles AS (
    SELECT Calle, ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn FROM (VALUES
        (N'Av. San Martín'),(N'Belgrano'),(N'Mitre'),(N'9 de Julio'),(N'Sarmiento'),(N'Rivadavia'),(N'Moreno'),(N'Alberdi'),
        (N'Av. Argentina'),(N'Roca'),(N'Yrigoyen'),(N'Av. Olascoaga'),(N'Corrientes'),(N'Entre Ríos'),(N'San Juan'),(N'Tucumán')
    ) AS T(Calle)
),
Estados AS (
    SELECT Estado, ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn FROM (VALUES
        (N'Activo'),(N'Activo'),(N'Activo'),(N'En Trámite'),(N'Finalizado'),(N'Suspendido')
    ) AS T(Estado)
),
Numeros AS (
    SELECT TOP 50 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
    FROM sys.all_objects
)
INSERT INTO Cliente (Nombre, Apellido, DNI, Edad, Telefono, Email, Direccion, Estado, Observaciones, Eliminado)
SELECT
    nom.Nombre,
    ape.Apellido,
    CAST(24000000 + n * 137 AS NVARCHAR(20)) AS DNI,
    20 + (n * 7) % 55 AS Edad,
    N'11-5' + RIGHT(N'000' + CAST(100 + n AS NVARCHAR(10)), 3) + N'-' + RIGHT(N'0000' + CAST(1000 + n * 17 % 9000 AS NVARCHAR(10)), 4) AS Telefono,
    LOWER(nom.Nombre) + N'.' + LOWER(ape.Apellido) + CAST(n AS NVARCHAR(10)) + N'@ejemplo.com' AS Email,
    cal.Calle + N' ' + CAST(100 + n * 13 % 4800 AS NVARCHAR(10)) AS Direccion,
    est.Estado,
    CASE WHEN n % 6 = 0 THEN N'Cliente frecuente de la escribanía.' ELSE NULL END AS Observaciones,
    0 AS Eliminado
FROM Numeros num
JOIN Nombres nom ON nom.rn = ((num.n - 1) % 50) + 1
JOIN Apellidos ape ON ape.rn = ((num.n * 3 - 1) % 50) + 1
JOIN Calles cal ON cal.rn = ((num.n - 1) % 16) + 1
JOIN Estados est ON est.rn = ((num.n - 1) % 6) + 1;

PRINT 'Clientes insertados: ' + CAST(@@ROWCOUNT AS VARCHAR);
GO
