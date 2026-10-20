USE NotalegisDB;
GO

-- Carga 50 escrituras de ejemplo (datos de relleno para la presentación).

;WITH Estados AS (
    SELECT Estado, ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn FROM (VALUES
        (N'Borrador'),(N'Finalizada'),(N'Protocolizada'),(N'Protocolizada'),(N'Archivada')
    ) AS T(Estado)
)
INSERT INTO Escritura (ClienteId, TipoEscrituraId, NumeroProtocolo, Fecha, RutaArchivo, FormatoArchivo, Estado, Observaciones, FechaCreacion, FechaProtocolizacion, UsuarioCreacion, Eliminado)
SELECT
    c.Id,
    ((c.Id - 1) % 12) + 1,
    CAST(2024 + (c.Id % 2) AS NVARCHAR(4)) + N'/' + RIGHT(N'000' + CAST(c.Id AS NVARCHAR(10)), 3),
    DATEADD(DAY, (c.Id * 5) - 120, CAST(GETDATE() AS DATE)),
    NULL,
    NULL,
    est.Estado,
    CASE WHEN c.Id % 7 = 0 THEN N'Escritura con anexos pendientes de digitalizar.' ELSE NULL END,
    DATEADD(DAY, (c.Id * 5) - 120, GETDATE()),
    CASE WHEN est.Estado = N'Protocolizada' THEN DATEADD(DAY, (c.Id * 5) - 115, GETDATE()) ELSE NULL END,
    N'admin',
    0
FROM Cliente c
JOIN Estados est ON est.rn = ((c.Id * 3 - 1) % 5) + 1;

PRINT 'Escrituras insertadas: ' + CAST(@@ROWCOUNT AS VARCHAR);
GO
