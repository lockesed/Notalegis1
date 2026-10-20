# Notalegis — Qué hace el programa

Resumen funcional de todas las gestiones (módulos) del sistema, pensado para explicar el funcionamiento del programa en la presentación. No incluye detalles técnicos de programación: describe qué hace cada pantalla, cómo se usa y qué controla.

Notalegis es un sistema de escritorio para escribanías. Centraliza en una sola base de datos toda la información de la escribanía: clientes, turnos, trámites, cobros, pagos, escrituras, pedidos de insumos y usuarios del sistema, con distintos niveles de acceso según el rol de cada persona (Administrador, Escribano, Secretaria).

---

## Índice

1. [Turnos](#1-turnos)
2. [Clientes](#2-clientes)
3. [Trámites](#3-trámites)
4. [Gestión de Cobros (presupuestos)](#4-gestión-de-cobros-presupuestos)
5. [Registro y control de Pagos](#5-registro-y-control-de-pagos)
6. [Pedidos de Insumos](#6-pedidos-de-insumos)
7. [Escrituras](#7-escrituras)
8. [Generador de Escrituras con Inteligencia Artificial](#8-generador-de-escrituras-con-inteligencia-artificial)
9. [Tipos de Escritura](#9-tipos-de-escritura)
10. [Estadísticas](#10-estadísticas)
11. [Reportes](#11-reportes)
12. [Usuarios](#12-usuarios)
13. [Auditoría](#13-auditoría)
14. [Ingreso al sistema y seguridad](#14-ingreso-al-sistema-y-seguridad)
15. [Configuración del sistema](#15-configuración-del-sistema)

---

## 1. Turnos

Permite agendar y organizar las citas que los clientes tienen en la escribanía.

- Al crear un turno se elige el cliente, la fecha, la hora, el tipo de trámite y observaciones. Todo turno nuevo arranca siempre en estado **Pendiente** — no se puede cargar un turno que ya figure como confirmado.
- El sistema no deja agendar un turno en una fecha que ya pasó, ni dos turnos para el mismo día con menos de 15 minutos de diferencia entre sí (para evitar superposiciones).
- Desde el listado de turnos se puede: **confirmar** un turno, **cancelarlo**, **editarlo** (ahí sí se puede cambiar el estado a En Proceso o Finalizado, y corregir fecha/hora si hace falta), **eliminarlo** o **consultarlo** en modo de solo lectura.
- Hay un buscador y un filtro por estado para encontrar turnos rápidamente.
- Se puede imprimir un comprobante del turno, y enviar por WhatsApp un recordatorio o una confirmación al cliente (abre WhatsApp Web con el mensaje ya redactado).
- Los turnos eliminados no se borran realmente de la base: quedan marcados como inactivos, para no perder el historial.

## 2. Clientes

Es la ficha de cada persona que es o fue cliente de la escribanía.

- Se cargan datos personales y de contacto: nombre, apellido, DNI/CUIT, teléfono, email, dirección, estado (Activo / En Trámite / Finalizado / Suspendido) y observaciones.
- El sistema valida que el nombre y apellido tengan solo letras, que el DNI/CUIT tenga entre 7 y 11 dígitos y no esté repetido en la base, que el teléfono tenga un formato válido y que el email tenga formato de correo electrónico.
- El listado se puede buscar por nombre o DNI, filtrar por estado, y está paginado (se muestra de a 50 clientes por página) para que sea manejable aunque haya muchos registros.
- Un cliente **no se puede eliminar si tiene turnos asociados** — hay que resolver eso antes.
- La eliminación de un cliente es reversible: queda oculto del listado principal, pero se puede ver y reactivar desde "Clientes inactivos" sin perder ninguno de sus datos.
- Desde la ficha del cliente se puede consultar su información, editarla, o abrir su carpeta de documentos adjuntos.
- Exportar el listado completo a Excel o imprimirlo está reservado solo para el Administrador.

## 3. Trámites

Es el seguimiento de cada expediente o gestión notarial en curso para un cliente (una escritura, un poder, una sociedad, etc.).

- Se carga el cliente, el tipo de trámite (hay una lista predefinida, pero también se puede escribir uno personalizado si no está en la lista), fecha de inicio, fecha de vencimiento, estado, prioridad y observaciones. También se puede indicar si la documentación requerida está completa.
- La fecha de vencimiento siempre tiene que ser posterior a la de inicio.
- Los trámites tienen estados (Pendiente, En Proceso, Documentación Incompleta, Esperando Cliente, Finalizado, Vencido, Cancelado) y niveles de prioridad (Baja, Media, Alta, Urgente).
- El listado se puede filtrar por estado, prioridad, y por vencimiento (vencen hoy, esta semana, este mes, o ya vencidos), además de buscar por cliente o tipo de trámite.
- Se puede enviar por WhatsApp un aviso de que el trámite requiere firma, o un mensaje de seguimiento que cambia su texto según en qué estado está el trámite.
- Desde acá también se accede a los vencimientos del día y a las estadísticas generales.
- Igual que en Clientes y Turnos, exportar el listado completo está reservado para Administradores.

## 4. Gestión de Cobros (presupuestos)

Es donde se arma el presupuesto de honorarios y gastos de un trámite, para presentárselo al cliente.

- Se elige el cliente y se completa el número de expediente y la fecha.
- Se cargan los impuestos y tasas aplicables (Impuesto a las Ganancias, ITI, Impuesto de Sellos, Tasa General, Sello Matriz, Certificados, Diligenciamiento, Honorarios), cada uno con su porcentaje, su base de cálculo y el importe resultante.
- Se cargan los importes correspondientes al tipo de escritura o documento (Compraventa, Constitución de Sociedad, Poder General, Certificado de Dominio, Testamento, Donación, Hipoteca, Cancelación de Hipoteca, Usufructo, Servidumbre).
- Si el trámite no está en esa lista, existe el campo **"Otra Gestión"**, donde se escribe el nombre del trámite y su importe; ese monto se suma al total igual que los demás y aparece identificado con su nombre en el comprobante final.
- Antes de generar el presupuesto, se completan algunos datos adicionales del escribano y de la operación (nombre, dirección, matrícula, tipo de operación, importe, y algunos conceptos extra con cantidad y precio unitario).
- Con todo eso, el sistema genera un **comprobante en PDF** con el detalle línea por línea de cada concepto y el total, que se guarda en la carpeta de presupuestos y se le puede entregar al cliente. Este comprobante **no es una factura fiscal** — es el presupuesto que el cliente usa para decidir si abona el trámite.
- El presupuesto queda guardado en el sistema con estado "Pendiente" hasta que se registre algún pago contra él.

## 5. Registro y control de Pagos

Es donde se hace el seguimiento de qué presupuestos se cobraron, parcial o totalmente.

- La pantalla de "Pagados" lista todos los presupuestos generados, con indicadores de cuántos están pagados, en pago parcial o pendientes, y el total de ganancias acumuladas. Se puede filtrar por cliente, estado de pago y rango de fechas.
- Al registrar un pago se indica el monto que se abona, el método de pago (Efectivo, Transferencia, Cheque, Tarjeta, etc.), la fecha real en que se recibió el pago y observaciones opcionales — todo lo que se carga ahí queda efectivamente guardado.
- El sistema no permite registrar un pago por más del saldo pendiente del presupuesto.
- El estado del presupuesto se actualiza solo, según lo que ya se cobró: **Pendiente** (nada abonado), **Parcial** (se abonó algo pero no el total) o **Pagado** (se cubrió el total).
- Se puede ver el **historial completo de pagos** de un presupuesto: cada pago individual con su fecha, monto, método y observaciones.
- El listado de pagados se puede exportar a un archivo para archivo o análisis externo.

## 6. Pedidos de Insumos

Gestiona el pedido de hojas protocolares (el papel especial que se usa para las escrituras) al Colegio de Escribanos.

- Se carga la cantidad, quién lo solicita y observaciones.
- El pedido se puede enviar por email directamente al Colegio de Escribanos desde el propio sistema, y queda registrada la fecha en que se envió.
- Se puede marcar el pedido como firmado una vez que se recibió la aprobación.
- El sistema lleva un control del **stock actual** de hojas protocolares frente a un mínimo configurado; cuando el stock cae por debajo de ese mínimo, queda señalado para avisar que hay que reponer.
- Se puede consultar el historial de pedidos por mes, e imprimir tanto un pedido individual como el listado mensual completo.

## 7. Escrituras

Es el registro de cada escritura pública que otorga la escribanía.

- Se carga el cliente, el tipo de escritura, el número de protocolo, la fecha y observaciones. El sistema no permite cargar dos escrituras con el mismo número de protocolo.
- Toda escritura nueva se crea en estado **Borrador**.
- Se puede adjuntar el archivo definitivo de la escritura (Word, PDF o texto), previsualizarlo dentro del mismo programa sin necesidad de tener Word o Adobe instalado, y abrirlo con el programa que corresponda.
- Cada escritura tiene su propia carpeta de documentos adjuntos, por si hay más de un archivo relacionado.
- Los estados posibles son Borrador, Finalizada, Protocolizada y Archivada. Cuando una escritura pasa a "Protocolizada", el sistema registra automáticamente la fecha en que eso ocurrió.
- Hay un buscador por cliente, protocolo, tipo o DNI, y la eliminación de una escritura es reversible (queda oculta, no se borra el archivo ni el registro).

## 8. Generador de Escrituras con Inteligencia Artificial

Ayuda a redactar el borrador de una escritura, a partir de una plantilla de referencia (por ejemplo, una escritura anterior similar).

- Se adjunta una plantilla (Word, PDF o texto) y, opcionalmente, se completan datos del otorgante (nacionalidad, estado civil, cónyuge, fecha de nacimiento, CUIL/CUIT) y las instrucciones particulares de esa escritura (cláusulas especiales, condiciones puntuales).
- El sistema envía esa información a un asistente de inteligencia artificial, que redacta un borrador siguiendo el estilo y la estructura de la plantilla adjuntada, respetando las fórmulas notariales fijas y dejando los espacios que el escribano debe completar a mano.
- El borrador generado se puede corregir libremente en pantalla antes de usarlo.
- Desde ahí se puede **exportar el texto a un documento Word** con el formato de página notarial correspondiente, o **guardarlo directamente como una nueva escritura** en el sistema (con su número de protocolo y fecha), quedando disponible como cualquier otra escritura.
- Los datos del otorgante que se completan acá (nacionalidad, estado civil, etc.) se usan solo como información para que la inteligencia artificial redacte mejor el texto; no quedan guardados como datos separados del cliente, porque la idea es que el escribano revise y complete el documento final a mano antes de darlo por definitivo.

## 9. Tipos de Escritura

Es el catálogo de los distintos actos notariales que la escribanía puede otorgar (Compraventa, Poder General, Testamento, Hipoteca, etc.), usado como lista desplegable al cargar una escritura nueva.

- Se puede agregar, editar o dar de baja un tipo de escritura, y marcarlo como activo o inactivo.
- Los tipos inactivos dejan de aparecer para elegir en escrituras nuevas, pero no afectan a las escrituras ya cargadas con ese tipo.

## 10. Estadísticas

Da una vista general y visual del funcionamiento de la escribanía, para tomar decisiones o presentar información a terceros.

- Se elige un período (hoy, esta semana, este mes, este año, o un rango de fechas a medida) y una de las 14 métricas disponibles: trámites por estado o por tipo, vencimientos por mes, cobros por estado, monto facturado y monto realmente cobrado por mes, método de pago más usado, top 10 de clientes por facturación, escrituras por estado/tipo/mes, turnos por semana, y el tipo de trámite que más ingresos genera.
- Los resultados se pueden ver como gráfico de barras, de torta, de líneas, o solo como tabla de datos.
- Todo se puede exportar a PDF (con gráfico y tabla), a Excel, o a CSV, para compartir o archivar.

## 11. Reportes

A diferencia de Estadísticas (que resume y grafica), Reportes da listados detallados, fila por fila, para uso administrativo del día a día.

- Hay 13 reportes disponibles: clientes activos e inactivos, top de clientes por monto facturado, trámites activos, trámites por vencer (próximos 30 días), trámites vencidos, cobros pendientes de pago, facturación del período, escrituras del período, tipos de escritura más solicitados, turnos del período, pedidos de insumos, y stock actual de insumos.
- Los reportes que corresponden a un rango de tiempo se pueden filtrar por fecha desde/hasta.
- Los reportes de facturación muestran además el total facturado, y si corresponde, cuánto de eso ya se cobró y cuánto sigue pendiente.
- Todos los reportes se pueden exportar a Excel con un solo clic.

## 12. Usuarios

Es donde el Administrador gestiona las cuentas de acceso al sistema.

- Se da de alta un usuario con nombre de usuario, contraseña y un rol: **Administrador**, **Escribano** o **Secretaria**. Las contraseñas se guardan siempre cifradas, nunca en texto plano.
- No se puede crear un usuario con un nombre que ya exista.
- Al editar un usuario, si no se escribe una contraseña nueva, se mantiene la anterior sin cambios.
- Por seguridad, el sistema **no permite que un usuario se elimine a sí mismo** mientras está usando esa cuenta, ni eliminar al **último Administrador** que quede en el sistema (para que nunca se quede la escribanía sin nadie que pueda gestionar usuarios).
- Este módulo es exclusivo para Administradores; el resto de los roles ni siquiera ve la opción en el menú.

## 13. Auditoría

Es el historial de quién hizo qué en el sistema, para tener trazabilidad de las operaciones importantes.

- Registra altas, modificaciones y bajas en los distintos módulos (clientes, trámites, turnos, escrituras, cobros, usuarios, etc.), indicando qué usuario la realizó, cuándo, y una breve descripción de la acción.
- Se puede filtrar por usuario, por módulo y por tipo de acción, y exportar el resultado a Excel.
- Es una pantalla exclusiva para Administradores y de solo lectura: no se puede editar ni borrar nada desde ahí.

## 14. Ingreso al sistema y seguridad

Controla quién puede entrar al sistema y protege las cuentas contra intentos de acceso indebido.

- Para ingresar se pide usuario y contraseña. Si son incorrectos, el sistema muestra un mensaje genérico (no indica si el problema es el usuario o la contraseña, por seguridad).
- Si se falla el ingreso **5 veces seguidas**, la cuenta queda bloqueada durante **30 minutos** antes de poder volver a intentar.
- Al ingresar correctamente, ese contador de intentos fallidos se reinicia.
- Si es la primera vez que se instala el sistema (todavía no hay ningún usuario cargado), antes de mostrar la pantalla de ingreso se pide crear el primer usuario Administrador.

## 15. Configuración del sistema

Reúne los datos y parámetros generales que usa el resto del programa.

- **Datos de la escribanía:** nombre, dirección, teléfono, email, y los datos notariales (ciudad, número de registro, provincia, nombre del escribano titular) que se usan en documentos y encabezados.
- **Impuestos y tasas por defecto:** los porcentajes que se precargan al armar un nuevo presupuesto de cobro (IVA, Ganancias, Honorarios sugeridos, Sellos, ITI, Tasa General).
- **Plantillas de WhatsApp:** los mensajes predefinidos que se usan para avisar a los clientes sobre firmas pendientes o hacer seguimiento de sus trámites.
- **Inteligencia Artificial:** la configuración necesaria para que funcione el generador de escrituras (clave de acceso al servicio de IA, qué modelo usar, y un documento de normativa vigente opcional que la IA puede usar como referencia adicional).
- Esta pantalla es exclusiva para Administradores.
