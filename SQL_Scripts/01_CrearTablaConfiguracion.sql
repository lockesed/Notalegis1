USE NotalegisDB;
GO

PRINT '========================================';
PRINT 'Iniciando creación de tabla Configuración...';
PRINT '========================================';
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Configuracion]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Configuracion](
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Clave] NVARCHAR(100) NOT NULL UNIQUE,
        [Valor] NVARCHAR(MAX) NULL,
        [TipoConfiguracion] NVARCHAR(50) NOT NULL, -- Ej: 'General', 'Impuestos', 'Alertas'
        [Descripcion] NVARCHAR(255) NULL,
        [FechaActualizacion] DATETIME NOT NULL DEFAULT GETDATE()
    );
    
    -- Insertar Valores por defecto (Datos Generales)
    INSERT INTO Configuracion (Clave, Valor, TipoConfiguracion, Descripcion) VALUES 
    ('Escribania_Nombre', 'Escribanía Ejemplo', 'General', 'Nombre completo de la Escribanía'),
    ('Escribania_Direccion', 'Calle Falsa 123', 'General', 'Dirección comercial de la Escribanía'),
    ('Escribania_Telefono', '5491123456789', 'General', 'Teléfono principal (Ej: 549...)'),
    ('Escribania_Email', 'contacto@ejemplo.com', 'General', 'Email de contacto oficial');

    -- Insertar Valores por defecto (Impuestos y Tasas)
    INSERT INTO Configuracion (Clave, Valor, TipoConfiguracion, Descripcion) VALUES 
    ('Impuesto_IVAPorc', '21.00', 'Impuestos', 'Porcentaje de IVA por defecto'),
    ('Impuesto_GananciasPorc', '3.00', 'Impuestos', 'Porcentaje de retención de Ganancias por defecto'),
    ('Impuesto_SellosPorc', '1.20', 'Impuestos', 'Porcentaje de Impuesto a los Sellos por defecto'),
    ('Impuesto_ITIPorc', '1.50', 'Impuestos', 'Porcentaje de ITI por defecto'),
    ('Honorarios_GeneralesPorc', '2.00', 'Impuestos', 'Porcentaje de Honorarios sugerido');

    -- Insertar Valores por defecto (Alertas y Comunicaciones - WhatsApp)
    INSERT INTO Configuracion (Clave, Valor, TipoConfiguracion, Descripcion) VALUES 
    ('WhatsApp_MensajeFirma', 
    'Hola {0}, le escribimos desde {Escribania_Nombre} para informarle que su trámite está listo para la firma: Tipo: {1}, Vencimiento: {2}. ¡Lo esperamos en {Escribania_Direccion}!',
    'Alertas', 'Plantilla para aviso de firma (0=Nombre, 1=Tipo, 2=Venc.)'),
    
    ('WhatsApp_MensajeSeguimiento', 
    'Hola {0}, desde {Escribania_Nombre} le informamos que el estado de su trámite {1} es: {2}.',
    'Alertas', 'Plantilla para seguimiento (0=Nombre, 1=Tipo, 2=Estado)');

    PRINT 'Tabla Configuracion creada e inicializada con valores por defecto.';
END
ELSE
BEGIN
    PRINT 'La tabla Configuracion ya existe.';
END
GO
