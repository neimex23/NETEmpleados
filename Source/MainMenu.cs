using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace NetEmpleados
{
    public partial class MainMenu : Form
    {
        IDBConnection conn = DBConnection.GetConnection();
        ControlObjects control = ControlObjects.Instance();

        public MainMenu()
        {
            InitializeComponent();
        }

        private void Control_Load(object sender, EventArgs e)
        {
            labelDateNow.Text = DateTime.Now.ToString("dd/MM/yyyy");
            comboBoxMode.SelectedIndex = 0;
            RefreshView();
        }

        private void btnAbout_Click(object sender, EventArgs e) => new About().ShowDialog();
        private void comboBoxMode_SelectedIndexChanged(object sender, EventArgs e) => RefreshView();
        private void btnRefresh_Click(object sender, EventArgs e) => RefreshView();

        /// <summary>
        /// Actualiza el DataViewGrid con la base de datos en PostgresSQL
        /// </summary>
        private void RefreshView()
        {
            try
            {
                DataViewDB.DataSource = null;
                Thread.Sleep(300);
                var dataSources = new Dictionary<int, Func<object>>
            {
                { 0, conn.GetEmployers },
                { 1, conn.GetSecctions }
            };

                if (dataSources.TryGetValue(comboBoxMode.SelectedIndex, out var getData))
                {
                    DataViewDB.DataSource = getData();
                }
                else
                {
                    MessageBox.Show("Modo no válido seleccionado.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }catch (Exception ex) { }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            if (!Enum.typeMap.TryGetValue(comboBoxMode.SelectedIndex, out var type))
            {
                MessageBox.Show("Por favor, selecciona un modo antes de continuar.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning); //El programa al iniciar establece directamente los combobox en 0 o 1
                return;
            }

            // Crear el formulario con el tipo seleccionado
            using (var form = new ControlUser(null, type, Enum.TypeAction.New))
            {
                form.ShowDialog();
                RefreshView(); // Actualizar la vista después de agregar un nuevo elemento
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (!Enum.typeMap.TryGetValue(comboBoxMode.SelectedIndex, out var type))
            {
                MessageBox.Show("Por favor, selecciona un modo antes de continuar.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning); //El programa al iniciar establece directamente los combobox en 0 o 1
                return;
            }
           
            if (DataViewDB.SelectedRows.Count > 0) // Verifica si hay una fila seleccionada
            {              
                var objectDownloaded = control.DownloadInfoDB(type, DataViewDB.SelectedRows[0]);
                Form form = new ControlUser(objectDownloaded, type, Enum.TypeAction.Edit);
                form.ShowDialog();
                RefreshView();
            }
            else
            {
                MessageBox.Show("Por favor, seleccione una fila para editar.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }          
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (!Enum.typeMap.TryGetValue(comboBoxMode.SelectedIndex, out var type))
            {
                MessageBox.Show("Por favor, selecciona un modo antes de continuar.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning); //El programa al iniciar establece directamente los combobox en 0 o 1
                return;
            }

            try
            {
                if (DataViewDB.SelectedRows.Count > 0)
                {
                    var objectDownloaded = control.DownloadInfoDB(type, DataViewDB.SelectedRows[0]);
                    switch (type)
                    {
                        case Enum.TypeData.Employer:
                            control.AddRemoveEmployer((Employers)objectDownloaded);
                            break;
                        case Enum.TypeData.Secction:
                            if (MessageBox.Show("Eliminar una seccion ocacionara que todo empleado asociada a la misma se elimine, desea continuar?", "Advertencia", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                            {
                                control.AddRemoveSecctions((Secctions)objectDownloaded);
                            }
                            break;
                        default:
                            break;
                    }
                    RefreshView();
                } else MessageBox.Show("Por favor, seleccione una fila para eliminar.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            } catch (Exception ex)
            {
                MessageBox.Show("Ocurrio un error: \n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }         
            
        }
    }
}
