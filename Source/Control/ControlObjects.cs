using System;
using System.Text;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Windows.Forms;

namespace NetEmpleados
{

    /// <summary>
    /// Esta clase gestiona la sincronización de datos entre una fuente de datos y una base de datos mediante colas concurrentes, permitiendo subir información sobre empleados (Employers) y secciones (Secctions) de manera asíncrona y eficiente. 
    /// Los elementos son procesados de manera concurrente en tareas asíncronas, asegurando que, en caso de error, ya sea por fallas de internet, en la base de datos o problemas de querys, los elementos fallidos se reencuelen y se intenten procesar nuevamente.
    /// </summary>
    public class ControlObjects
    {
        #region SingletonFactory
        private static ControlObjects instance;
        private ControlObjects() { }
        public static ControlObjects Instance() { if (instance == null) { instance = new ControlObjects(); } return instance;  }
        #endregion

        private IDBConnection connection = DBConnection.GetConnection();
      
        public int PendingElements
        {
            get { lock (syncLock) { return _pendingElements; } }
            private set { lock (syncLock) { _pendingElements = value; } }
        }
        private int _pendingElements;

        private readonly Dictionary<string, ConcurrentQueue<object>> pendingQueues = new Dictionary<string, ConcurrentQueue<object>>
        {
            {"UploadEmployers", new ConcurrentQueue<object>()},
            {"UpdateEmployers", new ConcurrentQueue<object>()},
            {"DeleteEmployers", new ConcurrentQueue<object>()},
            {"UploadSecctions", new ConcurrentQueue<object>()},
            {"UpdateSecctions", new ConcurrentQueue<object>()},
            {"DeleteSecctions", new ConcurrentQueue<object>()},
        };

        #region SyncingFunctions
        private bool syncing = false;
        private readonly object syncLock = new object();

        private void StartSync()
        {
            lock (syncLock)
            {
                if (syncing) return;
                syncing = true;
            }
            Task.Run(SyncDB);
        }

        private async Task SyncDB()
        {
            try
            {
                foreach (var queue in pendingQueues)
                {
                    if (!queue.Value.IsEmpty)
                    {
                        await ProcessQueue(queue.Key, queue.Value);
                    }
                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en SyncDB: {ex.Message}");
            }
            finally
            {
                lock (syncLock)
                {
                    syncing = false;
                }
            }
        }

        /// <summary>
        /// Recorre las listas de cola para procesar los elementos en la base de datos
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="queue"></param>
        /// <returns></returns>
        private async Task ProcessQueue(string operation, ConcurrentQueue<object> queue)
        {
            bool TryAgain = true;
            while (TryAgain && queue.TryDequeue(out var item))
            {
                try
                {
                    switch (operation)
                    {
                        case "UploadEmployers":
                            connection.UploadEmployer((Employers)item);
                            break;
                        case "UpdateEmployers":
                            connection.UpdateEmployer((Employers)item);
                            break;
                        case "DeleteEmployers":
                            connection.DeleteEmployer((Employers)item);
                            break;

                        case "UploadSecctions":
                            connection.UploadSecction((Secctions)item);
                            break;
                        case "UpdateSecctions":
                            connection.UpdateSecction((Secctions)item);
                            break;
                        case "DeleteSecctions":
                            connection.DeleteSecction((Secctions)item);
                            break;
                        default:
                            throw new InvalidOperationException($"Invalid operation cause on ProcessQueue: {operation}");
                    }

                    lock (syncLock) { _pendingElements--; }
                    MessageBox.Show("Operacion Realizada con exito.","Correcto",MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {                    
                    var Result = MessageBox.Show($"Se produjo un error al intentar realizar esta accion: {operation} \n{ex.Message} \n Abort: Descartar esta accion \n Retry: Intentarlo de vuelta \n Ignore: Intentar nuevamente mas tarde", "Error", MessageBoxButtons.AbortRetryIgnore, MessageBoxIcon.Error);
                    if (Result == DialogResult.Ignore) TryAgain = false;
                    if (Result != DialogResult.Abort) return;

                    // Reencolar el item para intentarlo más tarde
                    queue.Enqueue(item);
                    await Task.Delay(5000); //5 segundos de espera por intento
                }
            }
        }
        #endregion

        #region AddQueueProcess

        /// <summary>
        /// Agrega a la lista de cola un objeto Empleado o seccion para subir/editar/borrar de la base de datos
        /// </summary>
        /// <param name="operation">Accion hacia la base de datos</param>
        /// <param name="item">Objeto Empleado o seccion para subir</param>
        /// <exception cref="ArgumentException"></exception>
        private void AddToQueue(string operation, object item)
        {
            if (pendingQueues.TryGetValue(operation, out var queue))
            {
                queue.Enqueue(item);
                lock (syncLock) { _pendingElements++; }
                StartSync();
            }
            else
            {
                throw new ArgumentException($"Invalid Operation: {operation}");
            }
        }


     
        public void AddUploadEmployer(Employers emp) => AddToQueue("UploadEmployers", emp);
        public void AddUpdateEmployer(Employers emp) => AddToQueue("UpdateEmployers", emp);
        public void AddRemoveEmployer(Employers emp) => AddToQueue("DeleteEmployers", emp);

        public void AddUploadSecctions(Secctions secction) => AddToQueue("UploadSecctions", secction);
        public void AddUpdateSecctions(Secctions secction) => AddToQueue("UpdateSecctions", secction);
        public void AddRemoveSecctions(Secctions secction) => AddToQueue("DeleteSecctions", secction);

        #endregion

        #region DownloadProcess

        /// <summary>
        /// Obtiene un objeto Empleado/Seccion apartir de una tabala
        /// </summary>
        /// <param name="type">Employer/Secction</param>
        /// <param name="selectedRow">Fila seleccionada que contiene los datos de la clase</param>
        /// <returns></returns>
        public object DownloadInfoDB(Enum.TypeData type, DataGridViewRow selectedRow)
        {
            try
            {
                switch (type)
                {
                    case Enum.TypeData.Employer:
                        return GetEmployerData(selectedRow);

                    case Enum.TypeData.Secction:
                        return GetSecctionData(selectedRow);

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al obtener los datos de la fila seleccionada: \n {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }

        private Employers GetEmployerData(DataGridViewRow selectedRow)
        {
            int codeEmp = Convert.ToInt32(selectedRow.Cells["codigo"].Value);
            string fullName = selectedRow.Cells["nombre"].Value.ToString();
            DateTime dateBirth = Convert.ToDateTime(selectedRow.Cells["fechaNac"].Value);
            DateTime dateJoin = Convert.ToDateTime(selectedRow.Cells["fechaIng"].Value);
            decimal salary = Convert.ToDecimal(selectedRow.Cells["salarioHora"].Value);
            bool available = Convert.ToBoolean(selectedRow.Cells["bajaLogica"].Value);
            int secction = Convert.ToInt32(selectedRow.Cells["seccion"].Value);

            return new Employers(codeEmp, fullName, salary, dateBirth, dateJoin, available, secction);
        }

        private Secctions GetSecctionData(DataGridViewRow selectedRow)
        {
            int codeSec = Convert.ToInt32(selectedRow.Cells["codigo"].Value);
            string name = selectedRow.Cells["nombre"].Value.ToString();

            var idEmpValue = selectedRow.Cells["idResponsable"].Value;
            int responsibleId = idEmpValue is DBNull ? -1 : Convert.ToInt32(idEmpValue);

            return new Secctions(codeSec, name, responsibleId);
        }
        #endregion
    }
}
