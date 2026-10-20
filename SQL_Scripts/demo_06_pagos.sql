USE NotalegisDB;
GO

-- Carga pagos de ejemplo contra los cobros (algunos pagados completos, algunos parciales,
-- algunos sin pagos) y después recalcula Estado/MetodoPago de Cobros exactamente
-- igual que lo hace la aplicación real al registrar un pago (PagadosWindow.ActualizarCobroEnBaseDatos).

;WITH Metodos AS (
    SELECT Metodo, ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS rn FROM (VALUES
        (N'Efectivo'),(N'Transferencia Bancaria'),(N'Cheque'),(N'Tarjeta de Débito'),(N'Tarjeta de Crédito')
    ) AS T(Metodo)
)
INSERT INTO Pagos (CobroId, MontoPagado, FechaPago, MetodoPago, NumeroComprobante, Observaciones, UsuarioRegistro, FechaRegistro)
SELECT
    co.Id,
    CASE
        WHEN co.Id % 3 = 1 THEN co.TotalGeneral               -- pago completo
        ELSE CAST(co.TotalGeneral * 0.5 AS DECIMAL(18,2))     -- pago parcial
    END,
    DATEADD(DAY, 5 + (co.Id % 20), co.FechaCobro),
    met.Metodo,
    N'',
    CASE WHEN co.Id % 3 = 1 THEN N'Pago total del presupuesto.' ELSE N'Primer pago parcial del presupuesto.' END,
    N'admin',
    DATEADD(DAY, 5 + (co.Id % 20), co.FechaCobro)
FROM Cobros co
JOIN Metodos met ON met.rn = ((co.Id - 1) % 5) + 1
WHERE co.Id % 3 IN (1, 2);   -- Id % 3 = 0 se deja sin pagos (queda Pendiente)

PRINT 'Pagos insertados: ' + CAST(@@ROWCOUNT AS VARCHAR);
GO

-- Recalcular Estado y MetodoPago de cada Cobro según el total realmente pagado (misma lógica que la app)
UPDATE co
SET
    co.Estado = CASE
        WHEN ISNULL(pg.TotalPagado, 0) <= 0 THEN N'Pendiente'
        WHEN co.TotalGeneral - ISNULL(pg.TotalPagado, 0) > 0.01 THEN N'Parcial'
        ELSE N'Pagado'
    END,
    co.MetodoPago = pg.UltimoMetodo,
    co.FechaActualizacion = GETDATE(),
    co.UsuarioActualizacion = N'admin'
FROM Cobros co
OUTER APPLY (
    SELECT SUM(p.MontoPagado) AS TotalPagado,
           (SELECT TOP 1 p2.MetodoPago FROM Pagos p2 WHERE p2.CobroId = co.Id ORDER BY p2.FechaPago DESC) AS UltimoMetodo
    FROM Pagos p WHERE p.CobroId = co.Id
) pg;

PRINT 'Estados de Cobros recalculados.';
GO
