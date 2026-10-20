USE NotalegisDB;
GO

-- Vacía las tablas operativas (Clientes, Turnos, Trámites, Escrituras, Cobros, Pagos, Pedidos de Insumo)
-- en el orden correcto según las FK, y reinicia los IDs para que arranquen limpios desde 1.
-- NO toca Usuarios, Configuracion, TipoEscritura, StockInsumo ni RegistrosAuditoria.

DELETE FROM Pagos;
DBCC CHECKIDENT ('Pagos', RESEED, 0);

DELETE FROM Escritura;
DBCC CHECKIDENT ('Escritura', RESEED, 0);

DELETE FROM Cobros;
DBCC CHECKIDENT ('Cobros', RESEED, 0);

DELETE FROM tramite;
DBCC CHECKIDENT ('tramite', RESEED, 0);

DELETE FROM Turno;
DBCC CHECKIDENT ('Turno', RESEED, 0);

DELETE FROM Cliente;
DBCC CHECKIDENT ('Cliente', RESEED, 0);

DELETE FROM PedidoInsumo;
DBCC CHECKIDENT ('PedidoInsumo', RESEED, 0);

PRINT 'Tablas operativas vaciadas y reiniciadas.';
GO
