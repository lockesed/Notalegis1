using Notalegis.Datos.Repositorios;
using Notalegis.Datos.Modelos;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Notalegis.Negocio.Servicios
{
    public class FiltroEstadisticas
    {
        public DateTime Desde { get; set; } = new DateTime(DateTime.Today.Year, 1, 1);
        public DateTime Hasta { get; set; } = DateTime.Today;
        public string Metrica { get; set; } = "TramitesPorEstado";
        public string TipoGrafico { get; set; } = "Barras";
    }

    public class PuntoEstadistica
    {
        public string Etiqueta { get; set; }
        public double Valor { get; set; }
        public string ValorFormateado { get; set; }
    }

    public class KpiEstadistica
    {
        public string Etiqueta { get; set; }
        public string Valor { get; set; }
        public string Color { get; set; }
    }

    public class EstadisticaResultado
    {
        public string Titulo { get; set; }
        public List<PuntoEstadistica> Datos { get; set; } = new();
        public List<KpiEstadistica> Kpis { get; set; } = new();
        public string UnidadSufijo { get; set; } = "";
    }

    public class EstadisticasService
    {
        private readonly TramiteDAO tramiteDAO = new TramiteDAO();
        private readonly CobroDAO cobroDAO = new CobroDAO();
        private readonly PagoDAO pagoDAO = new PagoDAO();
        private readonly EscrituraDAO escrituraDAO = new EscrituraDAO();
        private readonly TurnoDAO turnoDAO = new TurnoDAO();

        public static readonly Dictionary<string, string> MetricasDisponibles = new()
        {
            ["TramitesPorEstado"]      = "Trámites por estado",
            ["TramitesPorTipo"]        = "Trámites por tipo",
            ["TramitesPorMes"]         = "Trámites ingresados por mes",
            ["VencimientosPorMes"]     = "Vencimientos por mes",
            ["CobrosPorEstado"]        = "Cobros por estado de pago",
            ["MontoFacturadoPorMes"]   = "Monto facturado por mes ($)",
            ["MontoCobradoPorMes"]     = "Monto realmente cobrado por mes ($)",
            ["MetodoPagoMasUsado"]     = "Método de pago más usado",
            ["Top10ClientesPorMonto"]  = "Top 10 clientes por monto",
            ["EscriturasPorEstado"]    = "Escrituras por estado",
            ["EscriturasPorTipo"]      = "Escrituras por tipo",
            ["EscriturasPorMes"]       = "Escrituras realizadas por mes",
            ["TurnosPorSemana"]        = "Turnos por semana",
            ["TramiteYMontoTop"]       = "Tipo de trámite con más ingresos",
        };

        public EstadisticaResultado ObtenerDatos(FiltroEstadisticas filtro)
        {
            return filtro.Metrica switch
            {
                "TramitesPorEstado"     => ObtenerTramitesPorEstado(filtro),
                "TramitesPorTipo"       => ObtenerTramitesPorTipo(filtro),
                "TramitesPorMes"        => ObtenerTramitesPorMes(filtro),
                "VencimientosPorMes"    => ObtenerVencimientosPorMes(filtro),
                "CobrosPorEstado"       => ObtenerCobrosPorEstado(filtro),
                "MontoFacturadoPorMes"  => ObtenerMontoFacturadoPorMes(filtro),
                "MontoCobradoPorMes"    => ObtenerMontoCobradoPorMes(filtro),
                "MetodoPagoMasUsado"    => ObtenerMetodosPago(filtro),
                "Top10ClientesPorMonto" => ObtenerTop10Clientes(filtro),
                "EscriturasPorEstado"   => ObtenerEscriturasPorEstado(filtro),
                "EscriturasPorTipo"     => ObtenerEscriturasPorTipo(filtro),
                "EscriturasPorMes"      => ObtenerEscriturasPorMes(filtro),
                "TurnosPorSemana"       => ObtenerTurnosPorSemana(filtro),
                "TramiteYMontoTop"      => ObtenerTramiteYMontoTop(filtro),
                _                       => new EstadisticaResultado { Titulo = "Seleccione una métrica" }
            };
        }

        private EstadisticaResultado ObtenerTramitesPorEstado(FiltroEstadisticas f)
        {
            var tramites = tramiteDAO.ObtenerTodos()
                .Where(t => t.FechaInicio >= f.Desde && t.FechaInicio <= f.Hasta).ToList();

            var datos = tramites.GroupBy(t => t.Estado ?? "Sin estado")
                .OrderByDescending(g => g.Count())
                .Select(g => new PuntoEstadistica { Etiqueta = g.Key, Valor = g.Count(), ValorFormateado = g.Count().ToString() })
                .ToList();

            return new EstadisticaResultado
            {
                Titulo = "Trámites por estado",
                Datos = datos,
                Kpis = new List<KpiEstadistica>
                {
                    new() { Etiqueta = "Total", Valor = tramites.Count.ToString(), Color = "#1976D2" },
                    new() { Etiqueta = "Activos", Valor = tramites.Count(t => t.Estado != "Finalizado" && t.Estado != "Cancelado").ToString(), Color = "#388E3C" },
                    new() { Etiqueta = "Finalizados", Valor = tramites.Count(t => t.Estado == "Finalizado").ToString(), Color = "#7B1FA2" },
                }
            };
        }

        private EstadisticaResultado ObtenerTramitesPorTipo(FiltroEstadisticas f)
        {
            var tramites = tramiteDAO.ObtenerTodos()
                .Where(t => t.FechaInicio >= f.Desde && t.FechaInicio <= f.Hasta).ToList();

            var datos = tramites.GroupBy(t => t.TipoTramite ?? "Sin tipo")
                .OrderByDescending(g => g.Count())
                .Select(g => new PuntoEstadistica { Etiqueta = g.Key, Valor = g.Count(), ValorFormateado = g.Count().ToString() })
                .ToList();

            return new EstadisticaResultado
            {
                Titulo = "Trámites por tipo",
                Datos = datos,
                Kpis = new List<KpiEstadistica>
                {
                    new() { Etiqueta = "Total trámites", Valor = tramites.Count.ToString(), Color = "#1976D2" },
                    new() { Etiqueta = "Tipos distintos", Valor = datos.Count.ToString(), Color = "#F57F17" },
                }
            };
        }

        private EstadisticaResultado ObtenerTramitesPorMes(FiltroEstadisticas f)
        {
            var tramites = tramiteDAO.ObtenerTodos()
                .Where(t => t.FechaInicio >= f.Desde && t.FechaInicio <= f.Hasta).ToList();

            var datos = GenerarSeriesMensuales(f.Desde, f.Hasta,
                fecha => tramites.Count(t => t.FechaInicio.Year == fecha.Year && t.FechaInicio.Month == fecha.Month));

            return new EstadisticaResultado
            {
                Titulo = "Trámites ingresados por mes",
                Datos = datos,
                Kpis = new List<KpiEstadistica>
                {
                    new() { Etiqueta = "Total período", Valor = tramites.Count.ToString(), Color = "#1976D2" },
                    new() { Etiqueta = "Promedio mensual", Valor = datos.Count > 0 ? $"{datos.Average(d => d.Valor):F1}" : "0", Color = "#388E3C" },
                }
            };
        }

        private EstadisticaResultado ObtenerVencimientosPorMes(FiltroEstadisticas f)
        {
            var tramites = tramiteDAO.ObtenerTodos()
                .Where(t => t.FechaVencimiento >= f.Desde && t.FechaVencimiento <= f.Hasta
                         && t.Estado != "Finalizado" && t.Estado != "Cancelado").ToList();

            var datos = GenerarSeriesMensuales(f.Desde, f.Hasta,
                fecha => tramites.Count(t => t.FechaVencimiento.Year == fecha.Year && t.FechaVencimiento.Month == fecha.Month));

            return new EstadisticaResultado
            {
                Titulo = "Vencimientos por mes",
                Datos = datos,
                Kpis = new List<KpiEstadistica>
                {
                    new() { Etiqueta = "Total vencen", Valor = tramites.Count.ToString(), Color = "#F57F17" },
                    new() { Etiqueta = "Este mes", Valor = tramites.Count(t => t.FechaVencimiento.Month == DateTime.Today.Month && t.FechaVencimiento.Year == DateTime.Today.Year).ToString(), Color = "#D32F2F" },
                }
            };
        }

        private EstadisticaResultado ObtenerCobrosPorEstado(FiltroEstadisticas f)
        {
            var cobros = cobroDAO.ObtenerTodos()
                .Where(c => c.FechaCobro >= f.Desde && c.FechaCobro <= f.Hasta).ToList();

            var datos = cobros.GroupBy(c => c.Estado ?? "Pendiente")
                .OrderByDescending(g => g.Count())
                .Select(g => new PuntoEstadistica { Etiqueta = g.Key, Valor = g.Count(), ValorFormateado = g.Count().ToString() })
                .ToList();

            return new EstadisticaResultado
            {
                Titulo = "Cobros por estado de pago",
                Datos = datos,
                Kpis = new List<KpiEstadistica>
                {
                    new() { Etiqueta = "Total cobros", Valor = cobros.Count.ToString(), Color = "#1976D2" },
                    new() { Etiqueta = "Pagados", Valor = cobros.Count(c => c.Estado == "Pagado").ToString(), Color = "#388E3C" },
                    new() { Etiqueta = "Pendientes", Valor = cobros.Count(c => c.Estado == "Pendiente").ToString(), Color = "#F57F17" },
                }
            };
        }

        private EstadisticaResultado ObtenerMontoFacturadoPorMes(FiltroEstadisticas f)
        {
            var cobros = cobroDAO.ObtenerTodos()
                .Where(c => c.FechaCobro >= f.Desde && c.FechaCobro <= f.Hasta).ToList();

            var datos = GenerarSeriesMensuales(f.Desde, f.Hasta,
                fecha => (double)cobros
                    .Where(c => c.FechaCobro.Year == fecha.Year && c.FechaCobro.Month == fecha.Month)
                    .Sum(c => c.TotalGeneral));
            datos.ForEach(d => d.ValorFormateado = $"${d.Valor:N0}");

            return new EstadisticaResultado
            {
                Titulo = "Monto facturado por mes",
                Datos = datos,
                UnidadSufijo = "$",
                Kpis = new List<KpiEstadistica>
                {
                    new() { Etiqueta = "Total período", Valor = $"${cobros.Sum(c => c.TotalGeneral):N0}", Color = "#7B1FA2" },
                    new() { Etiqueta = "Promedio mensual", Valor = datos.Count > 0 ? $"${datos.Average(d => d.Valor):N0}" : "$0", Color = "#1976D2" },
                }
            };
        }

        private EstadisticaResultado ObtenerMontoCobradoPorMes(FiltroEstadisticas f)
        {
            var pagos = pagoDAO.ObtenerTodos()
                .Where(p => p.FechaPago >= f.Desde && p.FechaPago <= f.Hasta).ToList();

            var datos = GenerarSeriesMensuales(f.Desde, f.Hasta,
                fecha => (double)pagos
                    .Where(p => p.FechaPago.Year == fecha.Year && p.FechaPago.Month == fecha.Month)
                    .Sum(p => p.MontoPagado));
            datos.ForEach(d => d.ValorFormateado = $"${d.Valor:N0}");

            return new EstadisticaResultado
            {
                Titulo = "Monto realmente cobrado por mes",
                Datos = datos,
                UnidadSufijo = "$",
                Kpis = new List<KpiEstadistica>
                {
                    new() { Etiqueta = "Total recaudado", Valor = $"${pagos.Sum(p => p.MontoPagado):N0}", Color = "#388E3C" },
                    new() { Etiqueta = "Promedio mensual", Valor = datos.Count > 0 ? $"${datos.Average(d => d.Valor):N0}" : "$0", Color = "#1976D2" },
                }
            };
        }

        private EstadisticaResultado ObtenerMetodosPago(FiltroEstadisticas f)
        {
            var cobros = cobroDAO.ObtenerTodos()
                .Where(c => c.FechaCobro >= f.Desde && c.FechaCobro <= f.Hasta && !string.IsNullOrEmpty(c.MetodoPago)).ToList();

            var datos = cobros.GroupBy(c => c.MetodoPago)
                .OrderByDescending(g => g.Count())
                .Select(g => new PuntoEstadistica { Etiqueta = g.Key, Valor = g.Count(), ValorFormateado = g.Count().ToString() })
                .ToList();

            return new EstadisticaResultado
            {
                Titulo = "Método de pago más usado",
                Datos = datos,
                Kpis = new List<KpiEstadistica>
                {
                    new() { Etiqueta = "Métodos distintos", Valor = datos.Count.ToString(), Color = "#1976D2" },
                    new() { Etiqueta = "Más usado", Valor = datos.FirstOrDefault()?.Etiqueta ?? "—", Color = "#388E3C" },
                }
            };
        }

        private EstadisticaResultado ObtenerTop10Clientes(FiltroEstadisticas f)
        {
            var clienteDAO = new ClienteDAO();
            var cobros = cobroDAO.ObtenerTodos()
                .Where(c => c.FechaCobro >= f.Desde && c.FechaCobro <= f.Hasta).ToList();

            var agrupados = cobros.GroupBy(c => c.ClienteId)
                .Select(g => new { ClienteId = g.Key, Total = g.Sum(c => c.TotalGeneral) })
                .OrderByDescending(x => x.Total)
                .Take(10)
                .ToList();

            var datos = agrupados.Select(x =>
            {
                string nombre;
                try { nombre = clienteDAO.ObtenerPorId(x.ClienteId)?.NombreCompleto ?? $"Cliente {x.ClienteId}"; }
                catch { nombre = $"Cliente {x.ClienteId}"; }
                return new PuntoEstadistica { Etiqueta = nombre, Valor = (double)x.Total, ValorFormateado = $"${x.Total:N0}" };
            }).ToList();

            return new EstadisticaResultado
            {
                Titulo = "Top 10 clientes por monto",
                Datos = datos,
                UnidadSufijo = "$",
                Kpis = new List<KpiEstadistica>
                {
                    new() { Etiqueta = "Clientes únicos", Valor = cobros.Select(c => c.ClienteId).Distinct().Count().ToString(), Color = "#1976D2" },
                    new() { Etiqueta = "Total top 10", Valor = $"${datos.Sum(d => d.Valor):N0}", Color = "#7B1FA2" },
                }
            };
        }

        private EstadisticaResultado ObtenerEscriturasPorEstado(FiltroEstadisticas f)
        {
            var escrituras = escrituraDAO.ObtenerTodos()
                .Where(e => e.Fecha >= f.Desde && e.Fecha <= f.Hasta).ToList();

            var datos = escrituras.GroupBy(e => e.Estado ?? "Sin estado")
                .OrderByDescending(g => g.Count())
                .Select(g => new PuntoEstadistica { Etiqueta = g.Key, Valor = g.Count(), ValorFormateado = g.Count().ToString() })
                .ToList();

            return new EstadisticaResultado
            {
                Titulo = "Escrituras por estado",
                Datos = datos,
                Kpis = new List<KpiEstadistica>
                {
                    new() { Etiqueta = "Total escrituras", Valor = escrituras.Count.ToString(), Color = "#1976D2" },
                    new() { Etiqueta = "Con archivo", Valor = escrituras.Count(e => !string.IsNullOrEmpty(e.RutaArchivo)).ToString(), Color = "#388E3C" },
                }
            };
        }

        private EstadisticaResultado ObtenerEscriturasPorTipo(FiltroEstadisticas f)
        {
            var escrituras = escrituraDAO.ObtenerTodos()
                .Where(e => e.Fecha >= f.Desde && e.Fecha <= f.Hasta).ToList();

            var datos = escrituras.GroupBy(e => e.TipoEscritura ?? "Sin tipo")
                .OrderByDescending(g => g.Count())
                .Select(g => new PuntoEstadistica { Etiqueta = g.Key, Valor = g.Count(), ValorFormateado = g.Count().ToString() })
                .ToList();

            return new EstadisticaResultado
            {
                Titulo = "Escrituras por tipo",
                Datos = datos,
                Kpis = new List<KpiEstadistica>
                {
                    new() { Etiqueta = "Total escrituras", Valor = escrituras.Count.ToString(), Color = "#1976D2" },
                    new() { Etiqueta = "Tipos distintos", Valor = datos.Count.ToString(), Color = "#F57F17" },
                }
            };
        }

        private EstadisticaResultado ObtenerEscriturasPorMes(FiltroEstadisticas f)
        {
            var escrituras = escrituraDAO.ObtenerTodos()
                .Where(e => e.Fecha >= f.Desde && e.Fecha <= f.Hasta).ToList();

            var datos = GenerarSeriesMensuales(f.Desde, f.Hasta,
                fecha => escrituras.Count(e => e.Fecha.Year == fecha.Year && e.Fecha.Month == fecha.Month));

            return new EstadisticaResultado
            {
                Titulo = "Escrituras realizadas por mes",
                Datos = datos,
                Kpis = new List<KpiEstadistica>
                {
                    new() { Etiqueta = "Total período", Valor = escrituras.Count.ToString(), Color = "#1976D2" },
                    new() { Etiqueta = "Promedio mensual", Valor = datos.Count > 0 ? $"{datos.Average(d => d.Valor):F1}" : "0", Color = "#388E3C" },
                }
            };
        }

        private EstadisticaResultado ObtenerTurnosPorSemana(FiltroEstadisticas f)
        {
            var turnos = turnoDAO.ObtenerTodos()
                .Where(t => t.FechaHora >= f.Desde && t.FechaHora <= f.Hasta).ToList();

            var cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
            var datos = turnos
                .GroupBy(t =>
                {
                    var startOfWeek = t.FechaHora.Date.AddDays(-(int)t.FechaHora.DayOfWeek + (int)DayOfWeek.Monday);
                    return startOfWeek;
                })
                .OrderBy(g => g.Key)
                .Take(24)
                .Select(g => new PuntoEstadistica
                {
                    Etiqueta = g.Key.ToString("dd/MM"),
                    Valor = g.Count(),
                    ValorFormateado = g.Count().ToString()
                })
                .ToList();

            return new EstadisticaResultado
            {
                Titulo = "Turnos por semana",
                Datos = datos,
                Kpis = new List<KpiEstadistica>
                {
                    new() { Etiqueta = "Total turnos", Valor = turnos.Count.ToString(), Color = "#1976D2" },
                    new() { Etiqueta = "Confirmados", Valor = turnos.Count(t => t.Confirmado).ToString(), Color = "#388E3C" },
                    new() { Etiqueta = "Prom. semanal", Valor = datos.Count > 0 ? $"{datos.Average(d => d.Valor):F1}" : "0", Color = "#F57F17" },
                }
            };
        }

        private EstadisticaResultado ObtenerTramiteYMontoTop(FiltroEstadisticas f)
        {
            var cobros = cobroDAO.ObtenerTodos()
                .Where(c => c.FechaCobro >= f.Desde && c.FechaCobro <= f.Hasta && !string.IsNullOrEmpty(c.TipoTramite)).ToList();

            var datos = cobros.GroupBy(c => c.TipoTramite)
                .Select(g => new PuntoEstadistica
                {
                    Etiqueta = g.Key,
                    Valor = (double)g.Sum(c => c.TotalGeneral),
                    ValorFormateado = $"${g.Sum(c => c.TotalGeneral):N0}"
                })
                .OrderByDescending(d => d.Valor)
                .ToList();

            return new EstadisticaResultado
            {
                Titulo = "Tipos de trámite con más ingresos",
                Datos = datos,
                UnidadSufijo = "$",
                Kpis = new List<KpiEstadistica>
                {
                    new() { Etiqueta = "Mayor ingreso", Valor = datos.FirstOrDefault()?.Etiqueta ?? "—", Color = "#7B1FA2" },
                    new() { Etiqueta = "Monto top tipo", Valor = datos.FirstOrDefault()?.ValorFormateado ?? "$0", Color = "#1976D2" },
                }
            };
        }

        private List<PuntoEstadistica> GenerarSeriesMensuales(DateTime desde, DateTime hasta, Func<DateTime, double> selector)
        {
            var datos = new List<PuntoEstadistica>();
            var mes = new DateTime(desde.Year, desde.Month, 1);
            var fin = new DateTime(hasta.Year, hasta.Month, 1);
            var meses = new[] { "Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic" };

            while (mes <= fin)
            {
                var valor = selector(mes);
                datos.Add(new PuntoEstadistica
                {
                    Etiqueta = $"{meses[mes.Month - 1]} {mes.Year}",
                    Valor = valor,
                    ValorFormateado = valor.ToString("F0")
                });
                mes = mes.AddMonths(1);
            }
            return datos;
        }
    }
}
