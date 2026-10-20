USE NotalegisDB;
GO

-- Carga 50 turnos de ejemplo, uno por cada cliente (datos de relleno para la presentación).

;WITH Tipos AS (
    SELECT Tipo, ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn FROM (VALUES
        (N'Escritura de Compraventa'),(N'Constitución de Sociedad'),(N'Poder General'),(N'Certificado de Dominio'),
        (N'Consulta General'),(N'Testamento'),(N'Donación'),(N'Hipoteca')
    ) AS T(Tipo)
),
Estados AS (
    SELECT Estado, ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn FROM (VALUES
        (N'Pendiente'),(N'Confirmado'),(N'Confirmado'),(N'Finalizado'),(N'Finalizado'),(N'Cancelado')
    ) AS T(Estado)
)
INSERT INTO Turno (IdCliente, FechaHora, Motivo, Estado, Observaciones, Confirmado, TipoTramite, Eliminado)
SELECT
    c.Id,
    DATEADD(HOUR, 9 + ((c.Id * 37) % 8), CAST(DATEADD(DAY, (c.Id * 3) - 75, CAST(GETDATE() AS DATE)) AS DATETIME)) AS FechaHora,
    N'Consulta por trámite notarial' AS Motivo,
    est.Estado,
    CASE WHEN c.Id % 5 = 0 THEN N'Cliente solicitó reprogramar horario.' ELSE NULL END,
    CASE WHEN est.Estado = N'Confirmado' THEN 1 ELSE 0 END,
    tip.Tipo,
    0
FROM Cliente c
JOIN Tipos tip ON tip.rn = ((c.Id - 1) % 8) + 1
JOIN Estados est ON est.rn = ((c.Id * 5 - 1) % 6) + 1;

PRINT 'Turnos insertados: ' + CAST(@@ROWCOUNT AS VARCHAR);
GO
