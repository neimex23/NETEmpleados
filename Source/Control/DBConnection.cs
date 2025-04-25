using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using Npgsql;

namespace NetEmpleados
{

    /// <summary>
    /// La interfaz IDBConnection implementa los metodos de DBConnection y se encarga de gestionar la conexión y las operaciones con una base de datos PostgreSQL.
    /// Ofrece métodos para abrir una conexión con la base de datos, obtener datos de las tablas empleados y secciones, y realizar inserciones o modificaciones de registros en ambas tablas.
    /// </summary>
    public interface IDBConnection {

        // <summary>
        /// Obtiene una tabla con todos los empleados de la base
        /// </summary>
        DataTable GetEmployers();

        // <summary>
        /// Obtiene una tabla con todos las secciones de la base
        /// </summary>
        DataTable GetSecctions();

        // <summary>
        /// Retorna un booleano si Existe Empleado
        /// </summary>
        /// <param name="code">Codigo identificador del empleado</param>
        bool AvalaibleEmployer(int code);

        // <summary>
        /// Sube un empleado a la base de datos
        /// </summary>
        /// <param name="emp">Objeto Empleado que se subira</param>
        void UploadEmployer(Employers emp);

        // <summary>
        /// Actualiza un empleado a la base de datos
        /// </summary>
        /// <param name="emp">Objeto Empleado que se actualizara</param>
        void UpdateEmployer(Employers emp);

        // <summary>
        /// Elimina un empleado a la base de datos
        /// </summary>
        /// <param name="emp">Objeto Empleado que se eliminara</param>
        void DeleteEmployer(Employers emp);

        // <summary>
        /// Sube una seccion a la base de datos
        /// </summary>
        /// <param name="sec">Objeto Seccion que se subira</param>
        void UploadSecction(Secctions sec);


        // <summary>
        /// Actualiza una seccion a la base de datos
        /// </summary>
        /// <param name="sec">Objeto Seccion que se actualizara</param>
        void UpdateSecction(Secctions sec);

        // <summary>
        /// Sube una seccion a la base de datos
        /// </summary>
        /// <param name="sec">Objeto Seccion que se eliminara</param>
        void DeleteSecction(Secctions sec);
    }

    public class DBConnection : IDBConnection
    {
        #region DBConnections
        private static DBConnection connection = null;

        // Para un manejo seguro del conecction string ya que tiene datos sensibles se puede implementar algun metodo de encriptado (AES-256, aspnet_regiis sobre configuraciones app.config o aws secret manager)
        // https://www.notion.so/Manejo-de-Secretos-c781ca2f65c449f4b9a6aa82fef3ab0a dejo mi investigacion aqui.
        // Como esto es una prueba en un medio local el resultado final que se obtiene es la cadena>  Host=localhost;Port=5432;Username=dev;Password=root;Database=netempleados desde App.config
        private string connectionString = ConfigurationManager.ConnectionStrings["PostgresConnectionString"].ConnectionString;

        public static IDBConnection GetConnection() { if (connection == null) { connection = new DBConnection(); } return connection; }

        // <summary>
        /// Establece Conexion sobre el servidor Postgres
        /// </summary>
        private NpgsqlConnection StartConnection()
        {
            var connection = new NpgsqlConnection(connectionString);

            try
            {
                connection.Open();
                Console.WriteLine("Conexión exitosa a PostgreSQL.");
                return connection;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al conectar a la base de datos: {ex.Message}");
                connection.Dispose(); 
                return null;
            }
        }
        #endregion

        //Todo los manejos de excepciones en ControlObjects.cs
        #region QuerysFunctions
            #region GetQuerys
        private DataTable GetTableDB(string query)
        {
            // Crear y abrir la conexión
            using (var connection = StartConnection())
            {
                if (connection == null)
                    throw new Exception("No se pudo establecer la conexión.");

                // Crear el adaptador de datos
                using (var dataAdapter = new NpgsqlDataAdapter(query, connection))
                {
                    DataTable dataTable = new DataTable();
                    dataAdapter.Fill(dataTable);
                    connection.Close();

                    // Alinear números a la derecha en las columnas que contengan datos numéricos
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        // Verificamos si la columna es de tipo numérico
                        if (column.DataType == typeof(int) || column.DataType == typeof(decimal) || column.DataType == typeof(double) || column.DataType == typeof(float))
                        {
                            // Aplicar formato de alineación a la derecha
                            column.DefaultValue = null; // Se puede agregar formato adicional si es necesario
                        }
                    }

                    return dataTable; //Retorna tabla completa
                }
            }
        }

        public DataTable GetEmployers()
        {
            DataTable result = new DataTable();
            try
            {
                string query = "SELECT * FROM empleados";
                result = GetTableDB(query);

                /// Caso Especial de antiguedad y adicional

                // Agregar columnas adicionales
                result.Columns.Add("antigüedad", typeof(int));
                result.Columns.Add("adicional", typeof(string));

                foreach (DataRow row in result.Rows)
                {
                    DateTime fechaIngreso = Convert.ToDateTime(row["fechaIng"]);
                    decimal salarioHora = Convert.ToDecimal(row["salarioHora"]);

                    // Calcular antigüedad en años
                    int antigüedad = DateTime.Now.Year - fechaIngreso.Year;
                    if (DateTime.Now < fechaIngreso.AddYears(antigüedad)) // Corrige si aún no cumple el año
                    {
                        antigüedad--;
                    }

                    // Calcular adicional (1% por año desde el 4to año)
                    decimal adicional = 0;
                    if (antigüedad >= 4)
                    {
                        adicional = salarioHora * (antigüedad - 3) / 100; // Desde el 4to año
                    }
                    string adicionalString = "$ " + Convert.ToString(adicional);

                    row["antigüedad"] = antigüedad;
                    row["adicional"] = adicionalString;
                }
            }catch (Exception) { }

            return result;
        }

        public DataTable GetSecctions()
        {
            string query = "SELECT * FROM secciones";
            DataTable ret = new DataTable();
            try
            {
              ret = GetTableDB(query);
            } catch (Exception ex) { }

            return ret;
        }

        public bool AvalaibleEmployer(int code)
        {
            string query = "SELECT COUNT(*) FROM empleados WHERE codigo = @code";

            using (var connection = StartConnection())
            {
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@code", code);

                    // Ejecutar la consulta y obtener el resultado
                    var result = (long)command.ExecuteScalar();

                    // Devolver true si existe al menos un registro, false en caso contrario
                    return result > 0;
                }
            }
        }
        #endregion
            #region Querys

        //Employers
        public void UploadEmployer(Employers emp)
        {
            string insertQuery = "INSERT INTO empleados (codigo, nombre, fechaNac, fechaIng, salarioHora, bajaLogica, seccion) VALUES (@codigo, @nombre, @fechaNac, @fechaIng, @salarioHora, @bajaLogica, @seccion)";

            using (var connection = StartConnection())
            {
                using (var command = new NpgsqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("codigo", emp.Code);
                    command.Parameters.AddWithValue("nombre", emp.FullName);
                    command.Parameters.AddWithValue("fechaNac", emp.DateBirth.Date);
                    command.Parameters.AddWithValue("fechaIng", emp.DateJoin.Date);
                    command.Parameters.AddWithValue("salarioHora", emp.Salary);
                    command.Parameters.AddWithValue("bajaLogica", emp.NotAvailable);
                    command.Parameters.AddWithValue("seccion", emp.Secction); 

                    int rowAffected = command.ExecuteNonQuery();

                    Console.WriteLine($"Filas insertadas: {rowAffected}");     
                    connection.Close();
                }
            } 
        }

        public void UpdateEmployer(Employers emp)
        {
            string updateQuery = "update empleados set nombre = @nombre, fechanac = @fechanac, fechaing = @fechaing, salariohora = @salariohora, bajalogica = @bajalogica, seccion = @seccion where codigo = @codigo";
            using (var connection = StartConnection())
            {
                using (var command = new NpgsqlCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("codigo", emp.Code);
                    command.Parameters.AddWithValue("nombre", emp.FullName);
                    command.Parameters.AddWithValue("fechanac", emp.DateBirth.Date);
                    command.Parameters.AddWithValue("fechaing", emp.DateJoin.Date);
                    command.Parameters.AddWithValue("salariohora", emp.Salary);
                    command.Parameters.AddWithValue("bajalogica", emp.NotAvailable);
                    command.Parameters.AddWithValue("seccion", emp.Secction);

                    int rowAffected = command.ExecuteNonQuery();
                    Console.WriteLine($"Filas actualizadas: {rowAffected}");
                }
            }
        }

        public void DeleteEmployer(Employers emp)
        {
            string deleteQuery = "delete from empleados where codigo = @codigo";
            using (var connection = StartConnection())
            {
                using (var command = new NpgsqlCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@codigo", emp.Code);
                    int rowAffected = command.ExecuteNonQuery();

                    Console.WriteLine($"Filas insertadas: {rowAffected}");
                    connection.Close();
                }
            }
        }

        //Secctions
        public void UploadSecction(Secctions sec)
        {
            string insertQuery = "INSERT INTO secciones (nombre, idResponsable) VALUES (@nombre, @idResponsable)";

            using (var connection = StartConnection())
            {
                using (var command = new NpgsqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@nombre", sec.Name);

                    _ = sec.IdRes != -1 ? command.Parameters.AddWithValue("@idResponsable", sec.IdRes) : command.Parameters.AddWithValue("@idResponsable", DBNull.Value);

                    int rowAffected = command.ExecuteNonQuery();

                    Console.WriteLine($"Filas insertadas: {rowAffected}");
                    connection.Close();
                }
            } 
        }

        public void UpdateSecction(Secctions sec)
        {
            string updateQuery = "Update secciones set nombre = @nombre, idResponsable = @idResponsable Where codigo = @codigo";
            using (var connection = StartConnection())
            {
                using (var command = new NpgsqlCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@codigo", sec.Code);
                    command.Parameters.AddWithValue("@nombre", sec.Name);

                    _ = sec.IdRes != -1 ? command.Parameters.AddWithValue("@idResponsable", sec.IdRes) : command.Parameters.AddWithValue("@idResponsable", DBNull.Value);

                    int rowAffected = command.ExecuteNonQuery();

                    Console.WriteLine($"Filas insertadas: {rowAffected}");
                    connection.Close();
                }
            }
        }


        public void DeleteSecction(Secctions sec)
        {
            string deleteQuery = "delete from secciones where codigo = @codigo";
            using (var connection = StartConnection())
            {
                using (var command = new NpgsqlCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@codigo", sec.Code);
                    int rowAffected = command.ExecuteNonQuery();

                    Console.WriteLine($"Filas insertadas: {rowAffected}");
                    connection.Close();
                }
            }
        }


        #endregion
        #endregion
    }
}
