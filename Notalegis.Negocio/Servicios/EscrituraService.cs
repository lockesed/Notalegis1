using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using Notalegis.Datos.Modelos;
using Notalegis.Datos.Repositorios;

namespace Notalegis.Negocio.Servicios
{
    public class EscrituraService
    {
        private readonly EscrituraDAO escrituraDAO = new EscrituraDAO();
        private readonly TipoEscrituraDAO tipoEscrituraDAO = new TipoEscrituraDAO();
        private readonly ClienteDAO clienteDAO = new ClienteDAO();
        private readonly AuditoriaService _auditoria = new AuditoriaService();
        
        private const string RUTA_BASE = @"C:\Notalegis\Escrituras\";

        public List<Escritura> ObtenerTodas()
        {
            return escrituraDAO.ObtenerTodos();
        }

        public List<Escritura> BuscarPorCliente(int clienteId)
        {
            return escrituraDAO.BuscarPorCliente(clienteId);
        }

        public List<Escritura> Buscar(string filtro)
        {
            if (string.IsNullOrWhiteSpace(filtro))
                return ObtenerTodas();

            return escrituraDAO.Buscar(filtro);
        }

        // Usado al crear, editar o generar con IA: evita que dos escrituras terminen
        // con el mismo número de protocolo (la base no lo restringe por sí sola).
        public bool ExisteNumeroProtocolo(string numeroProtocolo, int idExcluir = 0)
        {
            return escrituraDAO.ObtenerTodos()
                .Any(e => e.Id != idExcluir && e.NumeroProtocolo.Equals(numeroProtocolo, StringComparison.OrdinalIgnoreCase));
        }

        public void Guardar(Escritura escritura, string usuario = "Sistema")
        {
            if (escritura.Id == 0)
            {
                escrituraDAO.Insertar(escritura);
                _auditoria.Registrar(usuario, "ALTA", "Escrituras", null, $"Escritura protocolo '{escritura.NumeroProtocolo}' creada");
            }
            else
            {
                escrituraDAO.Modificar(escritura);
                _auditoria.Registrar(usuario, "MODIFICACION", "Escrituras", escritura.Id, $"Escritura protocolo '{escritura.NumeroProtocolo}' modificada");
            }
        }

        public void Eliminar(int id, string usuario = "Sistema")
        {
            var escritura = escrituraDAO.ObtenerPorId(id);
            escrituraDAO.Eliminar(id);
            string desc = escritura != null
                ? $"Escritura protocolo '{escritura.NumeroProtocolo}' del {escritura.Fecha:dd/MM/yyyy} (cliente: {escritura.NombreCliente}) eliminada"
                : $"Escritura ID {id} eliminada";
            _auditoria.Registrar(usuario, "BAJA", "Escrituras", id, desc);
        }

        public Escritura ObtenerPorId(int id)
        {
            return escrituraDAO.ObtenerPorId(id);
        }

        public void AbrirArchivo(string rutaArchivo)
        {
            if (File.Exists(rutaArchivo))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = rutaArchivo,
                    UseShellExecute = true
                });
            }
            else
            {
                throw new FileNotFoundException("El archivo no existe.");
            }
        }

        public string CopiarArchivoAdjunto(string rutaOrigen, Escritura escritura, Cliente cliente, TipoEscritura tipo)
        {
            try
            {
                // Crear estructura de carpetas
                string año = escritura.Fecha.Year.ToString();
                string carpetaAño = Path.Combine(RUTA_BASE, año);
                string nombreCarpetaCliente = $"Cliente_{cliente.Id:000}_{cliente.Nombre}_{cliente.Apellido}".Replace(" ", "_");
                string carpetaCliente = Path.Combine(carpetaAño, nombreCarpetaCliente);

                if (!Directory.Exists(carpetaCliente))
                    Directory.CreateDirectory(carpetaCliente);

                // Nombre del archivo
                string extension = Path.GetExtension(rutaOrigen);
                string nombreArchivo = $"ESC_{escritura.NumeroProtocolo.Replace("/", "-")}_{tipo.Nombre}_{escritura.Fecha:yyyy-MM-dd}{extension}";
                string rutaDestino = Path.Combine(carpetaCliente, nombreArchivo);

                // Copiar archivo
                File.Copy(rutaOrigen, rutaDestino, true);

                return rutaDestino;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al copiar archivo adjunto: {ex.Message}");
            }
        }

        public List<TipoEscritura> ObtenerTipos()
        {
            return tipoEscrituraDAO.ObtenerTodos();
        }

        public void GuardarTipo(TipoEscritura tipo, string usuario = "Sistema")
        {
            if (tipo.Id == 0)
            {
                tipoEscrituraDAO.Insertar(tipo);
                _auditoria.Registrar(usuario, "ALTA", "TiposEscritura", null, $"Tipo de escritura '{tipo.Nombre}' creado");
            }
            else
            {
                tipoEscrituraDAO.Modificar(tipo);
                _auditoria.Registrar(usuario, "MODIFICACION", "TiposEscritura", tipo.Id, $"Tipo de escritura '{tipo.Nombre}' modificado");
            }
        }

        public void EliminarTipo(int id, string usuario = "Sistema")
        {
            tipoEscrituraDAO.Eliminar(id);
            _auditoria.Registrar(usuario, "BAJA", "TiposEscritura", id, $"Tipo de escritura ID {id} eliminado");
        }
    }
}
