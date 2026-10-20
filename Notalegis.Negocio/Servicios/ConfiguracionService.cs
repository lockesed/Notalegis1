using System;
using System.Collections.Generic;
using Notalegis.Datos.Modelos;
using Notalegis.Datos.Repositorios;

namespace Notalegis.Negocio.Servicios
{
    public static class ClavesConfiguracion
    {
        // General
        public const string EscribaniaNombre = "Escribania_Nombre";
        public const string EscribaniaDireccion = "Escribania_Direccion";
        public const string EscribaniaTelefono = "Escribania_Telefono";
        public const string EscribaniaEmail = "Escribania_Email";

        // Impuestos
        public const string ImpuestoIVAPorc = "Impuesto_IVAPorc";
        public const string ImpuestoGananciasPorc = "Impuesto_GananciasPorc";
        public const string ImpuestoSellosPorc = "Impuesto_SellosPorc";
        public const string ImpuestoITIPorc = "Impuesto_ITIPorc";
        public const string HonorariosGeneralesPorc = "Honorarios_GeneralesPorc";
        public const string ImpuestoTasaGeneralPorc = "Impuesto_TasaGeneralPorc";

        // Alertas
        public const string WhatsAppMensajeFirma = "WhatsApp_MensajeFirma";
        public const string WhatsAppMensajeSeguimiento = "WhatsApp_MensajeSeguimiento";

        // Datos notariales de la escribanía
        public const string EscribaniaCiudad = "Escribania_Ciudad";
        public const string EscribaniaProvincia = "Escribania_Provincia";
        public const string EscribaniaNumeroRegistro = "Escribania_NumeroRegistro";
        public const string EscribaniaEscribano = "Escribania_Escribano";

        // Inteligencia Artificial
        public const string IAClaudeApiKey = "IA_ClaudeApiKey";
        public const string IAModelo = "IA_Modelo";
        public const string IAPromptBase = "IA_PromptBase";
        public const string IAMaxTokens = "IA_MaxTokens";
        public const string IARutaNormativaPDF = "IA_RutaNormativaPDF";
    }

    public class ConfiguracionService
    {
        private readonly ConfiguracionDAO dao;

        public ConfiguracionService()
        {
            dao = new ConfiguracionDAO();
        }

        public List<Configuracion> ObtenerTodas()
        {
            return dao.ObtenerTodas();
        }

        public Configuracion ObtenerPorClave(string clave)
        {
            return dao.ObtenerPorClave(clave);
        }

        public string ObtenerValorString(string clave, string valorPorDefecto = "")
        {
            var conf = dao.ObtenerPorClave(clave);
            return conf != null ? conf.Valor : valorPorDefecto;
        }

        public decimal ObtenerValorDecimal(string clave, decimal valorPorDefecto = 0)
        {
            var conf = dao.ObtenerPorClave(clave);
            if (conf != null && decimal.TryParse(conf.Valor, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }
            return valorPorDefecto;
        }

        public List<Configuracion> ObtenerPorCategoria(string tipoConfiguracion)
        {
            return dao.ObtenerPorCategoria(tipoConfiguracion);
        }

        public void ActualizarConfiguraciones(List<Configuracion> configuraciones)
        {
            if (configuraciones == null || configuraciones.Count == 0) return;
            dao.ActualizarConfiguraciones(configuraciones);
        }

        public void ActualizarConfiguracionSingular(string clave, string valor)
        {
            var conf = dao.ObtenerPorClave(clave);
            if (conf != null)
            {
                conf.Valor = valor;
                dao.ActualizarConfiguracion(conf);
            }
        }

        public void GuardarValor(string clave, string valor, string tipo = "General", string descripcion = null)
        {
            dao.ActualizarConfiguracion(new Configuracion
            {
                Clave = clave,
                Valor = valor,
                TipoConfiguracion = tipo,
                Descripcion = descripcion,
                FechaActualizacion = DateTime.Now
            });
        }
    }
}
