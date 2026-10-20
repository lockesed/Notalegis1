using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Notalegis.Datos.Repositorios;

namespace Notalegis.Negocio.Servicios
{
    public class ReporteDefinicion
    {
        public string Clave { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public bool UsaFechas { get; set; } = true;
    }

    public class ReportesService
    {
        private readonly ClienteDAO clienteDAO = new ClienteDAO();
        private readonly TramiteDAO tramiteDAO = new TramiteDAO();
        private readonly TurnoDAO turnoDAO = new TurnoDAO();
        private readonly EscrituraDAO escrituraDAO = new EscrituraDAO();
        private readonly CobroDAO cobroDAO = new CobroDAO();
        private readonly PedidoInsumoDAO pedidoDAO = new PedidoInsumoDAO();
        private readonly StockInsumoDAO stockDAO = new StockInsumoDAO();

        public static readonly List<ReporteDefinicion> Reportes = new()
        {
            new() { Clave = "ClientesActivos",     Nombre = "Clientes activos",                    Descripcion = "Listado completo de clientes activos con sus datos",         UsaFechas = false },
            new() { Clave = "ClientesInactivos",   Nombre = "Clientes inactivos",                  Descripcion = "Clientes dados de baja del sistema",                         UsaFechas = false },
            new() { Clave = "TopClientesMonto",    Nombre = "Top clientes por facturado",          Descripcion = "Ranking de clientes por monto total facturado en el período", UsaFechas = true  },
            new() { Clave = "TramitesActivos",     Nombre = "Trámites activos",                    Descripcion = "Trámites pendientes o en proceso",                           UsaFechas = false },
            new() { Clave = "TramitesPorVencer",   Nombre = "Trámites por vencer (30 días)",       Descripcion = "Trámites con vencimiento en los próximos 30 días",           UsaFechas = false },
            new() { Clave = "TramitesVencidos",    Nombre = "Trámites vencidos",                   Descripcion = "Trámites cuya fecha de vencimiento ya pasó",                 UsaFechas = false },
            new() { Clave = "CobrosPendientes",    Nombre = "Cobros pendientes de pago",           Descripcion = "Cobros en estado Pendiente o Parcial",                       UsaFechas = false },
            new() { Clave = "FacturacionPeriodo",  Nombre = "Facturación del período",             Descripcion = "Todos los cobros registrados en el rango de fechas",         UsaFechas = true  },
            new() { Clave = "EscriturasPeriodo",   Nombre = "Escrituras del período",              Descripcion = "Escrituras registradas en el rango de fechas",               UsaFechas = true  },
            new() { Clave = "EscriturasPopulares", Nombre = "Tipos de escritura más solicitados",  Descripcion = "Ranking de tipos de escritura por cantidad",                 UsaFechas = true  },
            new() { Clave = "TurnosPeriodo",       Nombre = "Turnos del período",                  Descripcion = "Agenda de turnos en el rango de fechas",                     UsaFechas = true  },
            new() { Clave = "PedidosInsumos",      Nombre = "Pedidos de insumos",                  Descripcion = "Pedidos realizados en el período",                           UsaFechas = true  },
            new() { Clave = "StockActual",         Nombre = "Stock actual de insumos",             Descripcion = "Inventario actual con alertas de stock bajo",                UsaFechas = false },
        };

        public DataTable GenerarReporte(string clave, DateTime desde, DateTime hasta)
        {
            return clave switch
            {
                "ClientesActivos"     => ReporteClientesActivos(),
                "ClientesInactivos"   => ReporteClientesInactivos(),
                "TopClientesMonto"    => ReporteTopClientes(desde, hasta),
                "TramitesActivos"     => ReporteTramitesActivos(),
                "TramitesPorVencer"   => ReporteTramitesPorVencer(),
                "TramitesVencidos"    => ReporteTramitesVencidos(),
                "CobrosPendientes"    => ReporteCobrosPendientes(),
                "FacturacionPeriodo"  => ReporteFacturacion(desde, hasta),
                "EscriturasPeriodo"   => ReporteEscrituras(desde, hasta),
                "EscriturasPopulares" => ReporteEscriturasPopulares(desde, hasta),
                "TurnosPeriodo"       => ReporteTurnos(desde, hasta),
                "PedidosInsumos"      => ReportePedidos(desde, hasta),
                "StockActual"         => ReporteStock(),
                _                     => new DataTable()
            };
        }

        private DataTable ReporteClientesActivos()
        {
            var dt = Tabla("Nombre", "Apellido", "DNI", "Teléfono", "Email", "Dirección", "Estado");
            foreach (var c in clienteDAO.ObtenerTodos().OrderBy(c => c.Apellido))
                dt.Rows.Add(c.Nombre, c.Apellido, c.DNI, c.Telefono, c.Email, c.Direccion, c.Estado);
            return dt;
        }

        private DataTable ReporteClientesInactivos()
        {
            var dt = Tabla("Nombre", "Apellido", "DNI", "Teléfono", "Email", "Dirección");
            foreach (var c in clienteDAO.ObtenerInactivos().OrderBy(c => c.Apellido))
                dt.Rows.Add(c.Nombre, c.Apellido, c.DNI, c.Telefono, c.Email, c.Direccion);
            return dt;
        }

        private DataTable ReporteTopClientes(DateTime desde, DateTime hasta)
        {
            var dt = new DataTable();
            dt.Columns.Add("#",               typeof(int));
            dt.Columns.Add("Cliente");
            dt.Columns.Add("DNI");
            dt.Columns.Add("Cantidad cobros", typeof(int));
            dt.Columns.Add("Total facturado ($)", typeof(decimal));

            var agrupados = cobroDAO.ObtenerTodos()
                .Where(c => c.FechaCobro >= desde && c.FechaCobro <= hasta)
                .GroupBy(c => c.ClienteId)
                .Select(g => new { ClienteId = g.Key, Cantidad = g.Count(), Total = g.Sum(x => x.TotalGeneral) })
                .OrderByDescending(x => x.Total)
                .ToList();

            int pos = 1;
            foreach (var x in agrupados)
            {
                var cl = TryGetCliente(x.ClienteId);
                dt.Rows.Add(pos++, cl.nombre, cl.dni, x.Cantidad, x.Total);
            }
            return dt;
        }

        private DataTable ReporteTramitesActivos()
        {
            var dt = Tabla("N° Expediente", "Cliente", "Tipo de trámite", "Estado", "Prioridad",
                           "Fecha inicio", "Fecha vencimiento", "Monto est. ($)", "Doc. completa");
            foreach (var t in tramiteDAO.ObtenerTodos()
                .Where(t => t.Estado != "Finalizado" && t.Estado != "Cancelado")
                .OrderBy(t => t.FechaVencimiento))
            {
                dt.Rows.Add(t.NumeroExpediente, t.Cliente, t.TipoTramite, t.Estado, t.Prioridad,
                    t.FechaInicio.ToString("dd/MM/yyyy"), t.FechaVencimiento.ToString("dd/MM/yyyy"),
                    (t.MontoEstimado ?? 0).ToString("N0"), t.DocumentacionCompleta ? "Sí" : "No");
            }
            return dt;
        }

        private DataTable ReporteTramitesPorVencer()
        {
            var limite = DateTime.Today.AddDays(30);
            var dt = Tabla("N° Expediente", "Cliente", "Tipo de trámite", "Estado", "Días restantes", "Fecha vencimiento");
            foreach (var t in tramiteDAO.ObtenerTodos()
                .Where(t => t.FechaVencimiento >= DateTime.Today && t.FechaVencimiento <= limite
                         && t.Estado != "Finalizado" && t.Estado != "Cancelado")
                .OrderBy(t => t.FechaVencimiento))
            {
                int dias = (t.FechaVencimiento.Date - DateTime.Today).Days;
                dt.Rows.Add(t.NumeroExpediente, t.Cliente, t.TipoTramite, t.Estado,
                    dias, t.FechaVencimiento.ToString("dd/MM/yyyy"));
            }
            return dt;
        }

        private DataTable ReporteTramitesVencidos()
        {
            var dt = Tabla("N° Expediente", "Cliente", "Tipo de trámite", "Estado", "Días vencido", "Fecha vencimiento");
            foreach (var t in tramiteDAO.ObtenerTodos()
                .Where(t => t.FechaVencimiento < DateTime.Today
                         && t.Estado != "Finalizado" && t.Estado != "Cancelado")
                .OrderBy(t => t.FechaVencimiento))
            {
                int dias = (DateTime.Today - t.FechaVencimiento.Date).Days;
                dt.Rows.Add(t.NumeroExpediente, t.Cliente, t.TipoTramite, t.Estado,
                    dias, t.FechaVencimiento.ToString("dd/MM/yyyy"));
            }
            return dt;
        }

        private DataTable ReporteCobrosPendientes()
        {
            var dt = Tabla("N° Expediente", "Cliente", "Tipo trámite", "Fecha cobro", "Estado", "Total ($)");
            foreach (var c in cobroDAO.ObtenerTodos()
                .Where(c => c.Estado == "Pendiente" || c.Estado == "Parcial")
                .OrderBy(c => c.FechaCobro))
            {
                dt.Rows.Add(c.NumeroExpediente, c.Cliente, c.TipoTramite,
                    c.FechaCobro.ToString("dd/MM/yyyy"), c.Estado, c.TotalGeneral.ToString("N0"));
            }
            return dt;
        }

        private DataTable ReporteFacturacion(DateTime desde, DateTime hasta)
        {
            var dt = Tabla("N° Expediente", "Cliente", "Tipo trámite", "Fecha", "Método pago", "Estado", "Total ($)");
            foreach (var c in cobroDAO.ObtenerTodos()
                .Where(c => c.FechaCobro >= desde && c.FechaCobro <= hasta)
                .OrderByDescending(c => c.FechaCobro))
            {
                dt.Rows.Add(c.NumeroExpediente, c.Cliente, c.TipoTramite,
                    c.FechaCobro.ToString("dd/MM/yyyy"), c.MetodoPago ?? "—", c.Estado, c.TotalGeneral.ToString("N0"));
            }
            return dt;
        }

        private DataTable ReporteEscrituras(DateTime desde, DateTime hasta)
        {
            var dt = Tabla("N° Protocolo", "Cliente", "DNI", "Tipo escritura", "Fecha", "Estado");
            foreach (var e in escrituraDAO.ObtenerTodos()
                .Where(e => e.Fecha >= desde && e.Fecha <= hasta)
                .OrderByDescending(e => e.Fecha))
            {
                dt.Rows.Add(e.NumeroProtocolo, e.NombreCliente, e.ClienteDNI,
                    e.TipoEscritura, e.Fecha.ToString("dd/MM/yyyy"), e.Estado);
            }
            return dt;
        }

        private DataTable ReporteEscriturasPopulares(DateTime desde, DateTime hasta)
        {
            var dt = new DataTable();
            dt.Columns.Add("Tipo de escritura");
            dt.Columns.Add("Cantidad", typeof(int));
            dt.Columns.Add("% del total");

            var escrituras = escrituraDAO.ObtenerTodos()
                .Where(e => e.Fecha >= desde && e.Fecha <= hasta).ToList();
            int total = escrituras.Count;

            var grupos = escrituras.GroupBy(e => e.TipoEscritura ?? "Sin tipo")
                .Select(g => new { Tipo = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count).ToList();

            foreach (var g in grupos)
            {
                double pct = total > 0 ? (double)g.Count / total * 100 : 0;
                dt.Rows.Add(g.Tipo, g.Count, $"{pct:F1}%");
            }
            return dt;
        }

        private DataTable ReporteTurnos(DateTime desde, DateTime hasta)
        {
            var clientes = clienteDAO.ObtenerTodos().ToDictionary(c => c.Id, c => $"{c.Nombre} {c.Apellido}");
            var dt = Tabla("Cliente", "Fecha", "Hora", "Tipo trámite", "Estado", "Confirmado");
            foreach (var t in turnoDAO.ObtenerTodos()
                .Where(t => t.FechaHora >= desde && t.FechaHora <= hasta)
                .OrderBy(t => t.FechaHora))
            {
                string nombre = clientes.TryGetValue(t.IdCliente, out var n) ? n : $"Cliente {t.IdCliente}";
                dt.Rows.Add(nombre, t.FechaHora.ToString("dd/MM/yyyy"),
                    t.FechaHora.ToString("HH:mm"), t.TipoTramite, t.Estado,
                    t.Confirmado ? "Sí" : "No");
            }
            return dt;
        }

        private DataTable ReportePedidos(DateTime desde, DateTime hasta)
        {
            var dt = Tabla("Tipo insumo", "Cantidad", "Solicitante", "Estado",
                           "Fecha pedido", "Fecha envío");
            foreach (var p in pedidoDAO.ObtenerTodos()
                .Where(p => p.FechaPedido >= desde && p.FechaPedido <= hasta)
                .OrderByDescending(p => p.FechaPedido))
            {
                dt.Rows.Add(p.TipoInsumo, p.Cantidad, p.Solicitante, p.Estado,
                    p.FechaPedido.ToString("dd/MM/yyyy"),
                    p.FechaEnvio?.ToString("dd/MM/yyyy") ?? "—");
            }
            return dt;
        }

        private DataTable ReporteStock()
        {
            var dt = Tabla("Tipo insumo", "Stock actual", "Stock mínimo", "Estado", "Última actualización");
            foreach (var s in stockDAO.ObtenerTodos().OrderBy(s => s.TipoInsumo))
            {
                string estado = s.CantidadActual <= s.StockMinimo ? "⚠ BAJO" : "OK";
                dt.Rows.Add(s.TipoInsumo, s.CantidadActual, s.StockMinimo,
                    estado, s.UltimaActualizacion.ToString("dd/MM/yyyy HH:mm"));
            }
            return dt;
        }

        private static DataTable Tabla(params string[] columnas)
        {
            var dt = new DataTable();
            foreach (var col in columnas) dt.Columns.Add(col);
            return dt;
        }

        private (string nombre, string dni) TryGetCliente(int id)
        {
            try
            {
                var c = clienteDAO.ObtenerPorId(id);
                return c != null ? ($"{c.Nombre} {c.Apellido}", c.DNI) : ($"Cliente {id}", "—");
            }
            catch { return ($"Cliente {id}", "—"); }
        }
    }
}
