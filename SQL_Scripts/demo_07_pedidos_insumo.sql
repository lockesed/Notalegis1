USE NotalegisDB;
GO

-- Carga 50 pedidos de insumos (hojas protocolares) de ejemplo.

;WITH Solicitantes AS (
    SELECT Nombre, ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn FROM (VALUES
        (N'Escribano Titular'),(N'Escribana Adjunta'),(N'Secretaría')
    ) AS T(Nombre)
),
Estados AS (
    SELECT Estado, ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn FROM (VALUES
        (N'Pendiente'),(N'Enviado'),(N'Enviado'),(N'Recibido'),(N'Recibido'),(N'Cancelado')
    ) AS T(Estado)
),
Numeros AS (
    SELECT TOP 50 ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n FROM sys.all_objects
)
INSERT INTO PedidoInsumo (FechaPedido, TipoInsumo, Cantidad, Solicitante, Observaciones, Estado, EmailDestino, Firmado, FechaEnvio, Eliminado)
SELECT
    DATEADD(DAY, (n * 4) - 180, GETDATE()),
    N'Hojas Protocolares',
    50 + (n * 7 % 200),
    sol.Nombre,
    CASE WHEN n % 6 = 0 THEN N'Pedido urgente por bajo stock.' ELSE NULL END,
    est.Estado,
    N'colegio.escribanos@ejemplo.org',
    CASE WHEN est.Estado IN (N'Enviado', N'Recibido') THEN 1 ELSE 0 END,
    CASE WHEN est.Estado IN (N'Enviado', N'Recibido') THEN DATEADD(DAY, (n * 4) - 178, GETDATE()) ELSE NULL END,
    0
FROM Numeros
JOIN Solicitantes sol ON sol.rn = ((n - 1) % 3) + 1
JOIN Estados est ON est.rn = ((n * 5 - 1) % 6) + 1;

PRINT 'Pedidos de insumo insertados: ' + CAST(@@ROWCOUNT AS VARCHAR);
GO
