-- =============================================================
-- NOTALEGIS — Script de datos de prueba
-- Ejecutar en SQL Server Management Studio contra la BD de Notalegis
-- =============================================================

SET DATEFORMAT ymd;   -- fuerza interpretación YYYY-MM-DD
BEGIN TRANSACTION;

-- ============================================================
-- 1. TIPOS DE ESCRITURA (solo agrega los que no existen)
-- ============================================================
IF NOT EXISTS (SELECT 1 FROM TipoEscritura WHERE Nombre = 'Constitución de Sociedad')
    INSERT INTO TipoEscritura (Nombre, Descripcion, Activo, FechaCreacion)
    VALUES ('Constitución de Sociedad', 'Acta constitutiva de sociedad comercial', 1, '2024-01-01');
IF NOT EXISTS (SELECT 1 FROM TipoEscritura WHERE Nombre = 'Permuta')
    INSERT INTO TipoEscritura (Nombre, Descripcion, Activo, FechaCreacion)
    VALUES ('Permuta', 'Intercambio de bienes', 1, '2024-01-01');
IF NOT EXISTS (SELECT 1 FROM TipoEscritura WHERE Nombre = 'Cesión de Derechos')
    INSERT INTO TipoEscritura (Nombre, Descripcion, Activo, FechaCreacion)
    VALUES ('Cesión de Derechos', 'Transferencia de derechos litigiosos', 1, '2024-01-01');
IF NOT EXISTS (SELECT 1 FROM TipoEscritura WHERE Nombre = 'Declaratoria de Herederos')
    INSERT INTO TipoEscritura (Nombre, Descripcion, Activo, FechaCreacion)
    VALUES ('Declaratoria de Herederos', 'Reconocimiento de herederos ab intestato', 1, '2024-01-01');

-- ============================================================
-- 2. CLIENTES
-- ============================================================
DECLARE @c1  INT, @c2  INT, @c3  INT, @c4  INT, @c5  INT,
        @c6  INT, @c7  INT, @c8  INT, @c9  INT, @c10 INT,
        @c11 INT, @c12 INT, @c13 INT, @c14 INT, @c15 INT,
        @c16 INT, @c17 INT, @c18 INT;

INSERT INTO Cliente (Nombre, Apellido, DNI, Edad, Telefono, Email, Direccion, Estado, Observaciones)
VALUES ('Martín',    'González',   '28541236', 42, '3515551001', 'mgonzalez@ejemplo.com',  'Av. Colón 1425, Córdoba',          'Activo',     'Cliente frecuente');
SET @c1 = SCOPE_IDENTITY();

INSERT INTO Cliente (Nombre, Apellido, DNI, Edad, Telefono, Email, Direccion, Estado, Observaciones)
VALUES ('Laura',     'Fernández',  '31874521', 35, '3515551002', 'lfernandez@ejemplo.com', 'Bv. San Juan 876, Córdoba',         'Activo',     '');
SET @c2 = SCOPE_IDENTITY();

INSERT INTO Cliente (Nombre, Apellido, DNI, Edad, Telefono, Email, Direccion, Estado, Observaciones)
VALUES ('Carlos',    'Ramírez',    '25963147', 51, '3515551003', 'cramirez@ejemplo.com',   'Calle Tucumán 345, Córdoba',        'En trámite', 'Tramite de sucesión abierto');
SET @c3 = SCOPE_IDENTITY();

INSERT INTO Cliente (Nombre, Apellido, DNI, Edad, Telefono, Email, Direccion, Estado, Observaciones)
VALUES ('Sofía',     'López',      '38412587', 29, '3515551004', 'slopez@ejemplo.com',     'Rivera Indarte 222, Córdoba',       'Activo',     '');
SET @c4 = SCOPE_IDENTITY();

INSERT INTO Cliente (Nombre, Apellido, DNI, Edad, Telefono, Email, Direccion, Estado, Observaciones)
VALUES ('Diego',     'Martínez',   '22785412', 58, '3515551005', 'dmartinez@ejemplo.com',  'Av. Rafael Núñez 1800, Córdoba',    'Activo',     'Empresario, operaciones frecuentes');
SET @c5 = SCOPE_IDENTITY();

INSERT INTO Cliente (Nombre, Apellido, DNI, Edad, Telefono, Email, Direccion, Estado, Observaciones)
VALUES ('Valeria',   'Torres',     '35698741', 33, '3515551006', 'vtorres@ejemplo.com',    'Gral. Paz 987, Córdoba',            'Activo',     '');
SET @c6 = SCOPE_IDENTITY();

INSERT INTO Cliente (Nombre, Apellido, DNI, Edad, Telefono, Email, Direccion, Estado, Observaciones)
VALUES ('Roberto',   'Sánchez',    '19874563', 67, '3515551007', 'rsanchez@ejemplo.com',   'Lima 554, Córdoba',                 'En trámite', 'Sucesión en proceso');
SET @c7 = SCOPE_IDENTITY();

INSERT INTO Cliente (Nombre, Apellido, DNI, Edad, Telefono, Email, Direccion, Estado, Observaciones)
VALUES ('Ana',       'Pereyra',    '40123654', 26, '3515551008', 'apereyra@ejemplo.com',   'Av. Vélez Sársfield 3200, Córdoba', 'Activo',     '');
SET @c8 = SCOPE_IDENTITY();

INSERT INTO Cliente (Nombre, Apellido, DNI, Edad, Telefono, Email, Direccion, Estado, Observaciones)
VALUES ('Pablo',     'Herrera',    '27456123', 47, '3515551009', 'pherrera@ejemplo.com',   'Marcelo T. de Alvear 1100, Córdoba','Activo',     'Socio de empresa local');
SET @c9 = SCOPE_IDENTITY();

INSERT INTO Cliente (Nombre, Apellido, DNI, Edad, Telefono, Email, Direccion, Estado, Observaciones)
VALUES ('Marina',    'Castro',     '33874569', 38, '3515551010', 'mcastro@ejemplo.com',    'Chacabuco 654, Córdoba',            'Activo',     '');
SET @c10 = SCOPE_IDENTITY();

INSERT INTO Cliente (Nombre, Apellido, DNI, Edad, Telefono, Email, Direccion, Estado, Observaciones)
VALUES ('Lucas',     'Ríos',       '37521489', 31, '3515551011', 'lrios@ejemplo.com',      'Belgrano 1547, Córdoba',            'Activo',     '');
SET @c11 = SCOPE_IDENTITY();

INSERT INTO Cliente (Nombre, Apellido, DNI, Edad, Telefono, Email, Direccion, Estado, Observaciones)
VALUES ('Cecilia',   'Moreno',     '29638745', 43, '3515551012', 'cmoreno@ejemplo.com',    'Independencia 2345, Córdoba',       'Activo',     'Abogada, conoce trámites');
SET @c12 = SCOPE_IDENTITY();

INSERT INTO Cliente (Nombre, Apellido, DNI, Edad, Telefono, Email, Direccion, Estado, Observaciones)
VALUES ('Jorge',     'Vega',       '21456987', 61, '3515551013', 'jvega@ejemplo.com',      'San Lorenzo 789, Córdoba',          'Activo',     '');
SET @c13 = SCOPE_IDENTITY();

INSERT INTO Cliente (Nombre, Apellido, DNI, Edad, Telefono, Email, Direccion, Estado, Observaciones)
VALUES ('Natalia',   'Acosta',     '36987412', 30, '3515551014', 'nacosta@ejemplo.com',    'Corro 345, Córdoba',                'Activo',     '');
SET @c14 = SCOPE_IDENTITY();

INSERT INTO Cliente (Nombre, Apellido, DNI, Edad, Telefono, Email, Direccion, Estado, Observaciones)
VALUES ('Gustavo',   'Mendoza',    '24563218', 52, '3515551015', 'gmendoza@ejemplo.com',   'Dean Funes 1200, Córdoba',          'Activo',     'Propietario de local comercial');
SET @c15 = SCOPE_IDENTITY();

INSERT INTO Cliente (Nombre, Apellido, DNI, Edad, Telefono, Email, Direccion, Estado, Observaciones)
VALUES ('Patricia',  'Ruiz',       '32145698', 40, '3515551016', 'pruiz@ejemplo.com',      'Duarte Quirós 987, Córdoba',        'Inactivo',   'No continúa con el trámite');
SET @c16 = SCOPE_IDENTITY();

INSERT INTO Cliente (Nombre, Apellido, DNI, Edad, Telefono, Email, Direccion, Estado, Observaciones)
VALUES ('Héctor',    'Gutiérrez',  '18745236', 69, '3515551017', 'hgutierrez@ejemplo.com', 'Obispo Trejo 450, Córdoba',         'Inactivo',   'Falleció, herederos en trámite');
SET @c17 = SCOPE_IDENTITY();

INSERT INTO Cliente (Nombre, Apellido, DNI, Edad, Telefono, Email, Direccion, Estado, Observaciones)
VALUES ('Florencia', 'Ibáñez',     '41256987', 24, '3515551018', 'fibañez@ejemplo.com',    'Av. Castro Barros 678, Córdoba',    'Activo',     'Primera compra inmueble');
SET @c18 = SCOPE_IDENTITY();

-- ============================================================
-- 3. TRÁMITES (con nombre de cliente como texto)
-- ============================================================
INSERT INTO tramite (Cliente, TipoTramite, NumeroExpediente, FechaCreacion, FechaInicio, FechaVencimiento, Estado, Prioridad, MontoEstimado, DocumentacionCompleta, Observaciones)
VALUES
('Martín González',    'Compraventa inmueble',     'EXP-2025-001', '2025-02-01', '2025-02-01', '2025-04-30', 'Finalizado',  'Alta',  2500000, 1, 'Operación completada'),
('Laura Fernández',    'Sucesión testamentaria',   'EXP-2025-002', '2025-02-10', '2025-02-10', '2026-02-10', 'En proceso',  'Alta',  1200000, 0, 'Esperando informe de bienes'),
('Carlos Ramírez',     'Declaratoria de herederos','EXP-2025-003', '2025-03-05', '2025-03-05', '2026-03-05', 'En proceso',  'Alta',  800000,  0, 'Dos herederos identificados'),
('Sofía López',        'Compraventa departamento', 'EXP-2025-004', '2025-03-15', '2025-03-15', '2025-06-15', 'Finalizado',  'Media', 1800000, 1, ''),
('Diego Martínez',     'Constitución de SRL',      'EXP-2025-005', '2025-03-20', '2025-03-20', '2025-05-20', 'Finalizado',  'Alta',  350000,  1, 'Sociedad constituida'),
('Diego Martínez',     'Poder general notarial',   'EXP-2025-006', '2025-04-01', '2025-04-01', '2025-05-01', 'Finalizado',  'Baja',  80000,   1, ''),
('Valeria Torres',     'Compraventa local',        'EXP-2025-007', '2025-04-10', '2025-04-10', '2025-07-10', 'En proceso',  'Alta',  3200000, 1, 'Aguarda libre deuda municipal'),
('Roberto Sánchez',    'Sucesión ab intestato',    'EXP-2025-008', '2025-04-15', '2025-04-15', '2026-04-15', 'En proceso',  'Alta',  0,       0, 'Sin testamento'),
('Ana Pereyra',        'Hipoteca sobre inmueble',  'EXP-2025-009', '2025-05-01', '2025-05-01', '2025-08-01', 'Finalizado',  'Alta',  5000000, 1, 'Banco Nación como acreedor'),
('Pablo Herrera',      'Constitución de SA',       'EXP-2025-010', '2025-05-10', '2025-05-10', '2025-08-10', 'En proceso',  'Alta',  600000,  1, 'Capital inicial $2M'),
('Marina Castro',      'Compraventa casa',         'EXP-2025-011', '2025-06-01', '2025-06-01', '2025-09-01', 'Finalizado',  'Media', 4500000, 1, ''),
('Lucas Ríos',         'Permuta de inmuebles',     'EXP-2025-012', '2025-06-15', '2025-06-15', '2025-09-15', 'Finalizado',  'Baja',  900000,  1, 'Permuta entre familiares'),
('Cecilia Moreno',     'Cesión de derechos',       'EXP-2025-013', '2025-07-01', '2025-07-01', '2025-10-01', 'Finalizado',  'Media', 250000,  1, ''),
('Jorge Vega',         'Testamento',               'EXP-2025-014', '2025-07-20', '2025-07-20', '2025-09-20', 'Finalizado',  'Media', 120000,  1, 'Testamento ológrafo protocolizado'),
('Natalia Acosta',     'Compraventa terreno',      'EXP-2025-015', '2025-08-01', '2025-08-01', '2025-11-01', 'Finalizado',  'Alta',  2100000, 1, 'Zona Norte'),
('Gustavo Mendoza',    'Hipoteca comercial',       'EXP-2025-016', '2025-08-15', '2025-08-15', '2025-12-15', 'En proceso',  'Alta',  8000000, 0, 'Garantía sobre local comercial'),
('Florencia Ibáñez',   'Compraventa PH',           'EXP-2025-017', '2025-09-01', '2025-09-01', '2025-12-01', 'En proceso',  'Alta',  6200000, 1, 'Primera vivienda'),
('Martín González',    'Donación a hijo',          'EXP-2025-018', '2025-09-10', '2025-09-10', '2025-11-10', 'Finalizado',  'Baja',  0,       1, 'Donación de rodado'),
('Diego Martínez',     'Compraventa campo',        'EXP-2025-019', '2025-10-01', '2025-10-01', '2026-02-01', 'En proceso',  'Alta',  15000000,1, 'Campo agrícola 300 has'),
('Cecilia Moreno',     'Poder especial',           'EXP-2025-020', '2025-10-15', '2025-10-15', '2025-12-15', 'Finalizado',  'Baja',  60000,   1, 'Para gestiones bancarias'),
('Laura Fernández',    'Compraventa cochera',      'EXP-2025-021', '2025-11-01', '2025-11-01', '2026-02-01', 'En proceso',  'Media', 850000,  0, 'Documentación incompleta'),
('Valeria Torres',     'Cancelación hipoteca',     'EXP-2025-022', '2025-11-15', '2025-11-15', '2026-01-15', 'En proceso',  'Alta',  0,       1, 'Hipoteca saldada'),
('Pablo Herrera',      'Compraventa oficina',      'EXP-2025-023', '2025-12-01', '2025-12-01', '2026-03-01', 'Pendiente',   'Media', 3800000, 0, 'En negociación'),
('Ana Pereyra',        'Constitución de SAS',      'EXP-2026-001', '2026-01-10', '2026-01-10', '2026-04-10', 'Pendiente',   'Media', 280000,  0, 'Nuevo emprendimiento'),
('Natalia Acosta',     'Compraventa casa nueva',   'EXP-2026-002', '2026-02-01', '2026-02-01', '2026-05-01', 'En proceso',  'Alta',  7500000, 1, 'Zona Arguello'),
('Marina Castro',      'Poder general',            'EXP-2026-003', '2026-03-01', '2026-03-01', '2026-05-01', 'Finalizado',  'Baja',  80000,   1, ''),
('Florencia Ibáñez',   'Compraventa cochera',      'EXP-2026-004', '2026-04-01', '2026-04-01', '2026-07-01', 'En proceso',  'Media', 600000,  1, ''),
('Gustavo Mendoza',    'Constitución de SA',       'EXP-2026-005', '2026-04-15', '2026-04-15', '2026-08-15', 'Pendiente',   'Alta',  900000,  0, 'Segunda empresa'),
('Lucas Ríos',         'Compraventa casa',         'EXP-2026-006', '2026-05-01', '2026-05-01', '2026-09-01', 'En proceso',  'Alta',  5800000, 1, ''),
('Martín González',    'Hipoteca vivienda',        'EXP-2026-007', '2026-06-01', '2026-06-01', '2026-10-01', 'Pendiente',   'Alta',  12000000,0, 'Banco Galicia');

-- ============================================================
-- 4. TURNOS
-- ============================================================
INSERT INTO Turno (IdCliente, FechaHora, Motivo, TipoTramite, Estado, Observaciones, Confirmado)
VALUES
(@c1,  '2026-07-16 09:00', 'Firma escritura hipoteca',      'Hipoteca vivienda',           'Confirmado',  'Traer DNI y escribano presente', 1),
(@c2,  '2026-07-16 10:30', 'Consulta sucesión',             'Sucesión testamentaria',       'Pendiente',   '',                               0),
(@c5,  '2026-07-16 11:00', 'Firma escritura campo',         'Compraventa campo',            'Confirmado',  'Vendedor y comprador presentes', 1),
(@c10, '2026-07-16 15:00', 'Firma poder general',           'Poder General',                'Pendiente',   '',                               0),
(@c14, '2026-07-17 09:30', 'Primera consulta compraventa',  'Compraventa casa nueva',       'Confirmado',  '',                               1),
(@c11, '2026-07-17 10:00', 'Revisión documentación',        'Compraventa casa',             'Pendiente',   'Llevar todos los certificados',  0),
(@c18, '2026-07-17 11:30', 'Firma escritura cochera',       'Compraventa cochera',          'Confirmado',  '',                               1),
(@c6,  '2026-07-18 09:00', 'Firma cancelación hipoteca',    'Cancelación hipoteca',         'Pendiente',   '',                               0),
(@c9,  '2026-07-18 10:30', 'Revisión estatuto SA',          'Constitución de SA',           'Cancelado',   'Reagendar',                      0),
(@c12, '2026-07-22 09:00', 'Consulta cesión derechos',      'Cesión de Derechos',           'Pendiente',   '',                               0),
(@c4,  '2026-07-22 10:00', 'Entrega documentación',         'Compraventa departamento',     'Confirmado',  '',                               1),
(@c15, '2026-07-23 09:00', 'Firma constitución SA',         'Constitución de Sociedad',     'Pendiente',   '',                               0),
(@c3,  '2026-07-23 11:00', 'Estado declaratoria herederos', 'Declaratoria de Herederos',    'Confirmado',  'Heredero 1 y 2 presentes',       1),
(@c8,  '2026-07-24 09:30', 'Firma escritura hipoteca',      'Hipoteca sobre inmueble',      'Pendiente',   '',                               0),
(@c1,  '2026-07-25 10:00', 'Consulta donación rodado',      'Donación',                     'Pendiente',   '',                               0),
(@c6,  '2026-07-28 09:00', 'Firma escritura local',         'Compraventa local',            'Confirmado',  'Gerente de banco presente',      1),
(@c14, '2026-07-29 10:00', 'Presentación documentación',    'Compraventa casa nueva',       'Pendiente',   '',                               0),
(@c5,  '2026-07-30 09:00', 'Reunión constitución SA',       'Constitución de Sociedad',     'Pendiente',   'Socios confirmados 3',           0);

-- ============================================================
-- 5. ESCRITURAS
-- ============================================================
DECLARE @te1 INT, @te2 INT, @te3 INT, @te4 INT, @te5 INT;
SELECT @te1 = Id FROM TipoEscritura WHERE Nombre = 'Escritura de Compraventa';
SELECT @te2 = Id FROM TipoEscritura WHERE Nombre = 'Constitución de Sociedad';
SELECT @te3 = Id FROM TipoEscritura WHERE Nombre = 'Hipoteca';
SELECT @te4 = Id FROM TipoEscritura WHERE Nombre = 'Testamento';
SELECT @te5 = Id FROM TipoEscritura WHERE Nombre = 'Poder General';

INSERT INTO Escritura (ClienteId, TipoEscrituraId, NumeroProtocolo, Fecha, Estado, Observaciones, FechaCreacion, UsuarioCreacion)
VALUES
(@c1,  @te1, '2025-001', '2025-02-28', 'Protocolizada', 'Compraventa casa Barrio Güemes',   '2025-02-28', 'admin'),
(@c4,  @te1, '2025-002', '2025-03-20', 'Protocolizada', 'Compraventa depto 3 ambientes',    '2025-03-20', 'admin'),
(@c5,  @te2, '2025-003', '2025-04-10', 'Protocolizada', 'Constitución SRL "Martínez Hnos."','2025-04-10', 'admin'),
(@c5,  @te5, '2025-004', '2025-04-25', 'Protocolizada', 'Poder general a apoderado',        '2025-04-25', 'admin'),
(@c8,  @te3, '2025-005', '2025-05-15', 'Protocolizada', 'Hipoteca Banco Nación $5M',        '2025-05-15', 'admin'),
(@c11, @te1, '2025-006', '2025-06-10', 'Protocolizada', 'Compraventa casa B° Argüello',     '2025-06-10', 'admin'),
(@c12, @te1, '2025-007', '2025-06-20', 'Protocolizada', 'Permuta inmuebles familiares',     '2025-06-20', 'admin'),
(@c13, @te4, '2025-008', '2025-07-25', 'Protocolizada', 'Testamento Sr. Vega',              '2025-07-25', 'admin'),
(@c14, @te1, '2025-009', '2025-08-05', 'Protocolizada', 'Compraventa terreno zona norte',   '2025-08-05', 'admin'),
(@c1,  @te1, '2025-010', '2025-09-12', 'Protocolizada', 'Donación rodado a hijo',           '2025-09-12', 'admin'),
(@c12, @te5, '2025-011', '2025-10-18', 'Protocolizada', 'Poder especial gestiones bancarias','2025-10-18','admin'),
(@c10, @te5, '2026-001', '2026-03-05', 'Protocolizada', 'Poder general Castro',             '2026-03-05', 'admin'),
(@c18, @te1, '2026-002', '2026-04-15', 'Borrador',      'Compraventa cochera Ibáñez',       '2026-04-15', 'admin'),
(@c11, @te1, '2026-003', '2026-05-20', 'En proceso',    'Compraventa casa Ríos',            '2026-05-20', 'admin'),
(@c1,  @te3, '2026-004', '2026-06-10', 'Borrador',      'Hipoteca vivienda González',       '2026-06-10', 'admin');

-- ============================================================
-- 6. COBROS
-- ============================================================
INSERT INTO Cobros (
    ClienteId, NumeroExpediente, FechaCobro, TipoTramite,
    ImpGananciasPorc, ImpGananciasBase, ImpGananciasImporte,
    ITIPorc, ITIBase, ITIImporte,
    ImpSellosPorc, ImpSellosBase, ImpSellosImporte,
    TasaGeneralPorc, TasaGeneralBase, TasaGeneralImporte,
    SelloMatrizPorc, SelloMatrizBase, SelloMatrizImporte,
    CertificadosPorc, CertificadosBase, CertificadosImporte,
    DiligenciamientoPorc, DiligenciamientoBase, DiligenciamientoImporte,
    HonorariosPorc, HonorariosBase, HonorariosImporte,
    IVAPorc, IVABase, IVAImporte,
    TituloPropiedad, VEP, LibreDeudaInmueble, LibreDeudaJudicial,
    LibreDeudaAgua, COTI, SentenciaDivorcio, Conformidad, SaldoDGR, DecretoAdjudicaciones,
    SubtotalImpuestos, SubtotalCertificados, TotalGeneral,
    Estado, MetodoPago, Observaciones, UsuarioCreacion, FechaCreacion
)
VALUES
-- González - Compraventa 2025-02 (Pagado)
(@c1,  'EXP-2025-001', '2025-02-28', 'Compraventa inmueble',
 1.5, 2500000, 37500,   1.5, 2500000, 37500,   1.2, 2500000, 30000,  0,0,0,  0,0,0,  0,0,0,  0,0,0,
 3.0, 2500000, 75000,   21, 75000, 15750,
 5000, 3000, 4500, 2000, 1500, 0, 0, 0, 0, 0,
 195750, 16000, 287500,
 'Pagado', 'Transferencia', '', 'admin', '2025-02-28'),

-- López - Compraventa depto 2025-03 (Pagado)
(@c4,  'EXP-2025-004', '2025-03-20', 'Compraventa departamento',
 1.5, 1800000, 27000,   1.5, 1800000, 27000,   1.2, 1800000, 21600,  0,0,0,  0,0,0,  0,0,0,  0,0,0,
 3.0, 1800000, 54000,   21, 54000, 11340,
 5000, 3000, 4500, 2000, 1500, 0, 0, 0, 0, 0,
 140940, 16000, 213280,
 'Pagado', 'Transferencia', '', 'admin', '2025-03-20'),

-- Martínez - Constitución SRL 2025-04 (Pagado)
(@c5,  'EXP-2025-005', '2025-04-10', 'Constitución de SRL',
 0,0,0,  0,0,0,  0,0,0,  0,0,0,  0,0,0,  0,0,0,  0,0,0,
 3.0, 350000, 10500,   21, 10500, 2205,
 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
 12705, 0, 12705,
 'Pagado', 'Efectivo', '', 'admin', '2025-04-10'),

-- Martínez - Poder general 2025-04 (Pagado)
(@c5,  'EXP-2025-006', '2025-04-25', 'Poder general notarial',
 0,0,0,  0,0,0,  0,0,0,  0,0,0,  0,0,0,  0,0,0,  0,0,0,
 3.0, 80000, 2400,   21, 2400, 504,
 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
 2904, 0, 2904,
 'Pagado', 'Efectivo', '', 'admin', '2025-04-25'),

-- Pereyra - Hipoteca 2025-05 (Pagado)
(@c8,  'EXP-2025-009', '2025-05-15', 'Hipoteca sobre inmueble',
 0,0,0,  0,0,0,  1.2, 5000000, 60000,  0,0,0,  0,0,0,  0,0,0,  0,0,0,
 3.0, 5000000, 150000,  21, 150000, 31500,
 5000, 3000, 4500, 2000, 1500, 0, 0, 0, 0, 0,
 241500, 16000, 273500,
 'Pagado', 'Transferencia', 'Banco Nación', 'admin', '2025-05-15'),

-- Ríos - Compraventa 2025-06 (Pagado)
(@c11, 'EXP-2025-011', '2025-06-10', 'Compraventa casa',
 1.5, 4500000, 67500,   1.5, 4500000, 67500,   1.2, 4500000, 54000,  0,0,0,  0,0,0,  0,0,0,  0,0,0,
 3.0, 4500000, 135000,  21, 135000, 28350,
 5000, 3000, 4500, 2000, 1500, 0, 0, 0, 0, 0,
 352350, 16000, 505850,
 'Pagado', 'Transferencia', '', 'admin', '2025-06-10'),

-- Moreno - Permuta 2025-06 (Pagado)
(@c12, 'EXP-2025-012', '2025-06-20', 'Permuta de inmuebles',
 0,0,0,  0,0,0,  1.2, 900000, 10800,  0,0,0,  0,0,0,  0,0,0,  0,0,0,
 3.0, 900000, 27000,   21, 27000, 5670,
 3000, 0, 0, 0, 0, 0, 0, 0, 0, 0,
 43470, 3000, 52470,
 'Pagado', 'Cheque', '', 'admin', '2025-06-20'),

-- Vega - Testamento 2025-07 (Pagado)
(@c13, 'EXP-2025-014', '2025-07-25', 'Testamento',
 0,0,0,  0,0,0,  0,0,0,  0,0,0,  0,0,0,  0,0,0,  0,0,0,
 3.0, 120000, 3600,    21, 3600, 756,
 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
 4356, 0, 4356,
 'Pagado', 'Efectivo', '', 'admin', '2025-07-25'),

-- Acosta - Compraventa terreno 2025-08 (Pagado)
(@c14, 'EXP-2025-015', '2025-08-05', 'Compraventa terreno',
 1.5, 2100000, 31500,   1.5, 2100000, 31500,   1.2, 2100000, 25200,  0,0,0,  0,0,0,  0,0,0,  0,0,0,
 3.0, 2100000, 63000,   21, 63000, 13230,
 5000, 3000, 4500, 2000, 1500, 0, 0, 0, 0, 0,
 164430, 16000, 246930,
 'Pagado', 'Transferencia', '', 'admin', '2025-08-05'),

-- González - Donación 2025-09 (Pagado)
(@c1,  'EXP-2025-018', '2025-09-12', 'Donación a hijo',
 0,0,0,  0,0,0,  0,0,0,  0,0,0,  0,0,0,  0,0,0,  0,0,0,
 3.0, 180000, 5400,    21, 5400, 1134,
 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
 6534, 0, 6534,
 'Pagado', 'Efectivo', 'Rodado valuado en $180k', 'admin', '2025-09-12'),

-- Moreno - Poder especial 2025-10 (Pagado)
(@c12, 'EXP-2025-020', '2025-10-18', 'Poder especial',
 0,0,0,  0,0,0,  0,0,0,  0,0,0,  0,0,0,  0,0,0,  0,0,0,
 3.0, 60000, 1800,    21, 1800, 378,
 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
 2178, 0, 2178,
 'Pagado', 'Efectivo', '', 'admin', '2025-10-18'),

-- Castro - Poder general 2026-03 (Pagado)
(@c10, 'EXP-2026-003', '2026-03-05', 'Poder general',
 0,0,0,  0,0,0,  0,0,0,  0,0,0,  0,0,0,  0,0,0,  0,0,0,
 3.0, 80000, 2400,    21, 2400, 504,
 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
 2904, 0, 2904,
 'Pagado', 'Efectivo', '', 'admin', '2026-03-05'),

-- Ibáñez - Cochera 2026-04 (Pendiente)
(@c18, 'EXP-2026-004', '2026-04-15', 'Compraventa cochera',
 0,0,0,   0,0,0,   1.2, 600000, 7200,  0,0,0,  0,0,0,  0,0,0,  0,0,0,
 3.0, 600000, 18000,   21, 18000, 3780,
 3000, 0, 4500, 0, 0, 0, 0, 0, 0, 0,
 28980, 7500, 36480,
 'Pendiente', '', '', 'admin', '2026-04-15'),

-- Torres - Cancelación hipoteca (Pendiente)
(@c6,  'EXP-2025-022', '2026-05-10', 'Cancelación hipoteca',
 0,0,0,  0,0,0,  0,0,0,  0,0,0,  0,0,0,  0,0,0,  0,0,0,
 3.0, 200000, 6000,    21, 6000, 1260,
 0, 0, 4500, 2000, 0, 0, 0, 0, 0, 0,
 7260, 6500, 13760,
 'Pendiente', '', '', 'admin', '2026-05-10'),

-- Herrera - Compraventa oficina 2026-06 (Pendiente)
(@c9,  'EXP-2025-023', '2026-06-15', 'Compraventa oficina',
 1.5, 3800000, 57000,  1.5, 3800000, 57000,  1.2, 3800000, 45600,  0,0,0,  0,0,0,  0,0,0,  0,0,0,
 3.0, 3800000, 114000,  21, 114000, 23940,
 5000, 3000, 4500, 2000, 1500, 0, 0, 0, 0, 0,
 297540, 16000, 429540,
 'Pendiente', '', '', 'admin', '2026-06-15'),

-- Ríos - Compraventa casa 2026-07 (Pendiente)
(@c11, 'EXP-2026-006', '2026-07-01', 'Compraventa casa',
 1.5, 5800000, 87000,  1.5, 5800000, 87000,  1.2, 5800000, 69600,  0,0,0,  0,0,0,  0,0,0,  0,0,0,
 3.0, 5800000, 174000,  21, 174000, 36540,
 5000, 3000, 4500, 2000, 1500, 0, 0, 0, 0, 0,
 454140, 16000, 645640,
 'Pendiente', '', '', 'admin', '2026-07-01'),

-- Martínez - Compraventa campo (Parcial)
(@c5,  'EXP-2025-019', '2026-07-10', 'Compraventa campo',
 1.5, 15000000, 225000,  1.5, 15000000, 225000,  1.2, 15000000, 180000,  0,0,0,  0,0,0,  0,0,0,  0,0,0,
 3.0, 15000000, 450000,  21, 450000, 94500,
 5000, 3000, 4500, 2000, 1500, 0, 0, 0, 0, 0,
 1174500, 16000, 1661000,
 'Parcial', 'Transferencia', 'Seña del 30% recibida', 'admin', '2026-07-10'),

-- Acosta - Compraventa casa nueva (Pendiente)
(@c14, 'EXP-2026-002', '2026-07-12', 'Compraventa casa nueva',
 1.5, 7500000, 112500,  1.5, 7500000, 112500,  1.2, 7500000, 90000,  0,0,0,  0,0,0,  0,0,0,  0,0,0,
 3.0, 7500000, 225000,  21, 225000, 47250,
 5000, 3000, 4500, 2000, 1500, 0, 0, 0, 0, 0,
 587250, 16000, 830750,
 'Pendiente', '', '', 'admin', '2026-07-12');

-- ============================================================
-- 7. PEDIDOS DE INSUMOS
-- ============================================================
INSERT INTO PedidoInsumo (FechaPedido, TipoInsumo, Cantidad, Solicitante, Observaciones, Estado, EmailDestino, Firmado, FechaEnvio, FechaRecepcion)
VALUES
('2025-04-01', 'Hojas Protocolares',  500, 'Dra. García',  'Pedido trimestral',          'Recibido',  'colegio@ejemplo.com', 1, '2025-04-03', '2025-04-10'),
('2025-07-01', 'Hojas Protocolares',  500, 'Dra. García',  'Pedido trimestral',          'Recibido',  'colegio@ejemplo.com', 1, '2025-07-03', '2025-07-12'),
('2025-10-01', 'Hojas Protocolares',  500, 'Dra. García',  'Pedido trimestral',          'Recibido',  'colegio@ejemplo.com', 1, '2025-10-03', '2025-10-11'),
('2026-01-10', 'Hojas Protocolares',  300, 'Dra. García',  'Inicio de año',              'Recibido',  'colegio@ejemplo.com', 1, '2026-01-12', '2026-01-20'),
('2026-04-01', 'Hojas Protocolares',  500, 'Dra. García',  'Pedido trimestral',          'Recibido',  'colegio@ejemplo.com', 1, '2026-04-02', '2026-04-10'),
('2026-07-01', 'Hojas Protocolares',  500, 'Dra. García',  'Pedido trimestral',          'Enviado',   'colegio@ejemplo.com', 1, '2026-07-02', NULL),
('2025-06-15', 'Sellos Fiscales',     200, 'Dra. García',  'Stock bajo',                 'Recibido',  'colegio@ejemplo.com', 1, '2025-06-17', '2025-06-25'),
('2026-03-10', 'Sellos Fiscales',     150, 'Dra. García',  '',                           'Recibido',  'colegio@ejemplo.com', 1, '2026-03-11', '2026-03-18'),
('2026-06-20', 'Sellos Fiscales',     200, 'Dra. García',  'Reposición urgente',         'Pendiente', 'colegio@ejemplo.com', 0, NULL,          NULL),
('2026-05-15', 'Papel Membretado A4', 2000,'Dra. García',  'Para cartas y presupuestos', 'Recibido',  'papeleria@ejemplo.com',  1, '2026-05-16', '2026-05-20');

-- ============================================================
-- 8. STOCK DE INSUMOS
-- ============================================================
MERGE StockInsumo AS target
USING (VALUES
    ('Hojas Protocolares',  120, 100),
    ('Sellos Fiscales',      35,  50),
    ('Papel Membretado A4', 950, 200)
) AS source (TipoInsumo, CantidadActual, StockMinimo)
ON target.TipoInsumo = source.TipoInsumo
WHEN MATCHED THEN
    UPDATE SET CantidadActual = source.CantidadActual, StockMinimo = source.StockMinimo, UltimaActualizacion = GETDATE()
WHEN NOT MATCHED THEN
    INSERT (TipoInsumo, CantidadActual, StockMinimo, UltimaActualizacion)
    VALUES (source.TipoInsumo, source.CantidadActual, source.StockMinimo, GETDATE());

COMMIT TRANSACTION;

PRINT 'Datos de prueba insertados correctamente.';
PRINT 'Clientes: 18 | Trámites: 30 | Turnos: 18 | Escrituras: 15 | Cobros: 18 | Pedidos: 10';
