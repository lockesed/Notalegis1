USE NotalegisDB;
GO

-- Carga 50 cobros/presupuestos de ejemplo (datos de relleno para la presentación).
-- El Estado y MetodoPago se recalculan después, en el script de Pagos (demo_06_pagos.sql),
-- igual que hace la aplicación real al registrar un pago.

;WITH Tipos AS (
    SELECT Tipo, ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn FROM (VALUES
        (N'Escritura de Compraventa'),(N'Constitución de Sociedad'),(N'Poder General'),(N'Certificado de Dominio'),
        (N'Testamento'),(N'Donación'),(N'Hipoteca'),(N'Cesión de Derechos')
    ) AS T(Tipo)
),
Base AS (
    SELECT
        c.Id AS ClienteId,
        CAST(500000 + (c.Id * 15731 % 4500000) AS DECIMAL(18,2)) AS BaseOperacion
    FROM Cliente c
)
INSERT INTO Cobros (
    ClienteId, NumeroExpediente, FechaCobro, TipoTramite,
    ImpGananciasPorc, ImpGananciasBase, ImpGananciasImporte,
    ITIPorc, ITIBase, ITIImporte,
    ImpSellosPorc, ImpSellosBase, ImpSellosImporte,
    TasaGeneralPorc, TasaGeneralBase, TasaGeneralImporte,
    HonorariosPorc, HonorariosBase, HonorariosImporte,
    IVAPorc, IVABase, IVAImporte,
    OtraGestionNombre, OtraGestionImporte,
    SubtotalImpuestos, SubtotalCertificados, TotalGeneral,
    Estado, Observaciones, FechaCreacion, UsuarioCreacion, Eliminado
)
SELECT
    b.ClienteId,
    N'EXP-COB-' + RIGHT(N'0000' + CAST(b.ClienteId AS NVARCHAR(10)), 4),
    DATEADD(DAY, (b.ClienteId * 3) - 150, GETDATE()),
    tip.Tipo,

    2.00, b.BaseOperacion, CAST(b.BaseOperacion * 0.02 AS DECIMAL(18,2)),
    1.50, b.BaseOperacion, CAST(b.BaseOperacion * 0.015 AS DECIMAL(18,2)),
    1.50, b.BaseOperacion, CAST(b.BaseOperacion * 0.015 AS DECIMAL(18,2)),
    1.50, b.BaseOperacion, CAST(b.BaseOperacion * 0.015 AS DECIMAL(18,2)),
    3.00, b.BaseOperacion, CAST(b.BaseOperacion * 0.03 AS DECIMAL(18,2)),
    21.00, CAST(b.BaseOperacion * 0.03 AS DECIMAL(18,2)), CAST(b.BaseOperacion * 0.03 * 0.21 AS DECIMAL(18,2)),

    CASE WHEN b.ClienteId % 10 = 0 THEN N'Cesión de Derechos Hereditarios' ELSE NULL END,
    CASE WHEN b.ClienteId % 10 = 0 THEN CAST(50000 + (b.ClienteId * 777 % 100000) AS DECIMAL(18,2)) ELSE 0 END,

    CAST(b.BaseOperacion * (0.02 + 0.015 + 0.015 + 0.015 + 0.03) + b.BaseOperacion * 0.03 * 0.21 AS DECIMAL(18,2)),
    CASE WHEN b.ClienteId % 10 = 0 THEN CAST(50000 + (b.ClienteId * 777 % 100000) AS DECIMAL(18,2)) ELSE 0 END,
    CAST(b.BaseOperacion * (0.02 + 0.015 + 0.015 + 0.015 + 0.03) + b.BaseOperacion * 0.03 * 0.21 AS DECIMAL(18,2))
      + CASE WHEN b.ClienteId % 10 = 0 THEN CAST(50000 + (b.ClienteId * 777 % 100000) AS DECIMAL(18,2)) ELSE 0 END,

    N'Pendiente',
    NULL,
    DATEADD(DAY, (b.ClienteId * 3) - 150, GETDATE()),
    N'admin',
    0
FROM Base b
JOIN Tipos tip ON tip.rn = ((b.ClienteId - 1) % 8) + 1;

PRINT 'Cobros insertados: ' + CAST(@@ROWCOUNT AS VARCHAR);
GO
