using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using Notalegis.Datos.Conexion;
using Notalegis.Datos.Modelos;

namespace Notalegis.Datos.Repositorios
{
    public class AuditoriaDAO
    {
        private readonly string connectionString = ConfiguracionSegura.Cadena;

        public void Registrar(string usuario, string accion, string modulo, int? entidadId = null, string descripcion = null)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(
                        "INSERT INTO RegistrosAuditoria (FechaHora, Usuario, Accion, Modulo, EntidadId, Descripcion) " +
                        "VALUES (GETDATE(), @Usuario, @Accion, @Modulo, @EntidadId, @Descripcion)", con);
                    cmd.Parameters.AddWithValue("@Usuario", usuario ?? "Sistema");
                    cmd.Parameters.AddWithValue("@Accion", accion);
                    cmd.Parameters.AddWithValue("@Modulo", modulo);
                    cmd.Parameters.AddWithValue("@EntidadId", (object)entidadId ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Descripcion", (object)descripcion ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                // La auditoría es no crítica — si falla, no interrumpe la operación principal
            }
        }

        public List<RegistroAuditoria> ObtenerTodos(int limite = 500)
        {
            var lista = new List<RegistroAuditoria>();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    $"SELECT TOP {limite} Id, FechaHora, Usuario, Accion, Modulo, EntidadId, Descripcion " +
                    "FROM RegistrosAuditoria ORDER BY FechaHora DESC", con);
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        lista.Add(new RegistroAuditoria
                        {
                            Id          = Convert.ToInt32(r["Id"]),
                            FechaHora   = Convert.ToDateTime(r["FechaHora"]),
                            Usuario     = r["Usuario"].ToString(),
                            Accion      = r["Accion"].ToString(),
                            Modulo      = r["Modulo"].ToString(),
                            EntidadId   = r["EntidadId"] != DBNull.Value ? (int?)Convert.ToInt32(r["EntidadId"]) : null,
                            Descripcion = r["Descripcion"] != DBNull.Value ? r["Descripcion"].ToString() : null
                        });
                    }
                }
            }
            return lista;
        }

        public List<RegistroAuditoria> ObtenerPorUsuario(string usuario, int limite = 200)
        {
            var lista = new List<RegistroAuditoria>();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand(
                    $"SELECT TOP {limite} Id, FechaHora, Usuario, Accion, Modulo, EntidadId, Descripcion " +
                    "FROM RegistrosAuditoria WHERE Usuario = @Usuario ORDER BY FechaHora DESC", con);
                cmd.Parameters.AddWithValue("@Usuario", usuario);
                using (SqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        lista.Add(new RegistroAuditoria
                        {
                            Id          = Convert.ToInt32(r["Id"]),
                            FechaHora   = Convert.ToDateTime(r["FechaHora"]),
                            Usuario     = r["Usuario"].ToString(),
                            Accion      = r["Accion"].ToString(),
                            Modulo      = r["Modulo"].ToString(),
                            EntidadId   = r["EntidadId"] != DBNull.Value ? (int?)Convert.ToInt32(r["EntidadId"]) : null,
                            Descripcion = r["Descripcion"] != DBNull.Value ? r["Descripcion"].ToString() : null
                        });
                    }
                }
            }
            return lista;
        }
    }
}
