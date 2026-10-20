USE NotalegisDB;
GO

-- Carga 50 trámites de ejemplo (datos de relleno para la presentación).

;WITH Tipos AS (
    SELECT Tipo, ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn FROM (VALUES
        (N'Escritura de Compraventa'),(N'Constitución de Sociedad'),(N'Poder General'),(N'Certificado de Dominio'),
        (N'Testamento'),(N'Donación'),(N'Hipoteca'),(N'Cesión de Derechos'),(N'Declaratoria de Herederos'),(N'Consulta General')
    ) AS T(Tipo)
),
Estados AS (
    SELECT Estado, ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn FROM (VALUES
        (N'Pendiente'),(N'En Proceso'),(N'En Proceso'),(N'Documentación Incompleta'),(N'Esperando Cliente'),
        (N'Finalizado'),(N'Finalizado'),(N'Vencido'),(N'Cancelado')
    ) AS T(Estado)
),
Prioridades AS (
    SELECT Prioridad, ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn FROM (VALUES
        (N'Baja'),(N'Media'),(N'Media'),(N'Alta'),(N'Urgente')
    ) AS T(Prioridad)
)
INSERT INTO tramite (Cliente, TipoTramite, FechaInicio, FechaVencimiento, Estado, Prioridad, DocumentacionCompleta, Observaciones, NumeroExpediente, FechaCreacion, MontoEstimado, Eliminado)
SELECT
    c.Nombre + N' ' + c.Apellido,
    tip.Tipo,
    DATEADD(DAY, (c.Id * 4) - 90, CAST(GETDATE() AS DATE)) AS FechaInicio,
    DATEADD(DAY, (c.Id * 4) - 90 + 30 + (c.Id % 60), CAST(GETDATE() AS DATE)) AS FechaVencimiento,
    est.Estado,
    pri.Prioridad,
    CASE WHEN c.Id % 3 = 0 THEN 1 ELSE 0 END,
    CASE WHEN c.Id % 4 = 0 THEN N'A la espera de documentación adicional del cliente.' ELSE NULL END,
    N'EXP-' + CAST(2025 + (c.Id % 2) AS NVARCHAR(4)) + N'-' + RIGHT(N'0000' + CAST(c.Id AS NVARCHAR(10)), 4),
    DATEADD(DAY, (c.Id * 4) - 90, GETDATE()),
    CASE WHEN c.Id % 5 = 0 THEN NULL ELSE CAST(50000 + (c.Id * 4137 % 450000) AS DECIMAL(18,2)) END,
    0
FROM Cliente c
JOIN Tipos tip ON tip.rn = ((c.Id - 1) % 10) + 1
JOIN Estados est ON est.rn = ((c.Id * 7 - 1) % 9) + 1
JOIN Prioridades pri ON pri.rn = ((c.Id * 3 - 1) % 5) + 1;

PRINT 'Trámites insertados: ' + CAST(@@ROWCOUNT AS VARCHAR);
GO
