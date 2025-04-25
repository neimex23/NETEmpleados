using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using static NetEmpleados.Enum;

namespace NetEmpleados
{
    public partial class ControlUser : Form
    {
        ControlObjects control = ControlObjects.Instance();
        IDBConnection conn = DBConnection.GetConnection();

        Enum.TypeData type; Enum.TypeAction action;
        Employers selectEmployer = null;
        Secctions selectSecctions = null;


        public ControlUser(object dataClass, Enum.TypeData type, Enum.TypeAction action)
        {
            InitializeComponent();

            this.type = type; this.action = action;

            if (dataClass is Employers)
            {
                selectEmployer = (Employers)dataClass;
            }
            else if (dataClass is Secctions)
            {
                selectSecctions = (Secctions)dataClass;
            }
        }

        private void ControlUser_Load(object sender, EventArgs e)
        {
            #region Employer
            if (type == Enum.TypeData.Employer)
            {
                tabControlGeneral.TabPages.Remove(this.tabSecctions);
                tabEmployers.Focus();

                var Secctions = conn.GetSecctions();

                if (Secctions.Rows.Count == 0)
                {
                    MessageBox.Show("Debe Existir al menos una seccion para ingresar un nuevo empleado", "Error");
                    Close();
                }
                else
                {
                    foreach (DataRow sec in Secctions.Rows) // Carga de Secciones
                    {
                        string item = $"{sec["Codigo"]} - {sec["nombre"]}";
                        comboBoxSection.Items.Add(item);
                    }
                }
                comboBoxEnableEmployer.SelectedIndex = 0; //Habilitado por defecto

                //Modo Edicion Carga los datos
                if (action == TypeAction.Edit)
                {
                    textBoxCodeEmp.Text = selectEmployer.Code.ToString();
                    textBoxCodeEmp.Enabled = false; // Una Primary key no se puede modificar

                    textBoxFullName.Text = selectEmployer.FullName;
                    dateTimePickerDateBirth.Value = selectEmployer.DateBirth;
                    dateTimePickerDateJoin.Value = selectEmployer.DateJoin;
                    textBoxSalary.Text = selectEmployer.Salary.ToString();

                    int indexSelect;
                    _ = selectEmployer.NotAvailable ? indexSelect = 1 : indexSelect = 0;
                    comboBoxEnableEmployer.SelectedIndex = indexSelect;

                    string secctionString = selectEmployer.Secction.ToString(); //Busca la seccion en el comboBox
                    var index = comboBoxSection.Items.Cast<string>()
                        .ToList()
                        .FindIndex(item => item.Contains(secctionString));

                    comboBoxSection.SelectedIndex = index;
                }
            }
            #endregion
            #region Secction
            if (type == Enum.TypeData.Secction)
            {
                tabControlGeneral.TabPages.Remove(this.tabEmployers);
                tabSecctions.Focus();

                int lastcode = -1;

                if (action == TypeAction.New)
                {
                    textBoxCodeSec.Hide();
                    labelCode.Hide();
                }
                else //Modo edicion Secciones
                {
                    lastcode = selectSecctions.Code;
                    textBoxNameSec.Text = selectSecctions.Name;
                    textBoxIDEmp.Text = selectSecctions.IdRes == -1 ? string.Empty : selectSecctions.IdRes.ToString();
                }

                textBoxCodeSec.Text = lastcode.ToString();
            }
            #endregion
        }

        private void btnExit_Click(object sender, EventArgs e) => Close();


        /// <summary>
        /// Employer Save button Process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSaveEmp_Click(object sender, EventArgs e)
        {
            try
            {
                int code;
                if (!int.TryParse(textBoxCodeEmp.Text, out code)) throw new Exception("Revise el valor codigo debe ser un numero");

                if (action == TypeAction.New && conn.AvalaibleEmployer(code)) throw new Exception("El Empleado que intenta añadir ya existe, porfavor elija otro codigo.");

                string fullName = textBoxFullName.Text;
                if(string.IsNullOrEmpty(fullName)) throw new Exception("Rellene el campo de nombre");

                DateTime dateJoin, dateBirth;
                DateTime.TryParse(dateTimePickerDateJoin.Text, out dateJoin);
                DateTime.TryParse(dateTimePickerDateBirth.Text, out dateBirth);

                if (dateJoin < dateBirth) throw new Exception("La fecha de ingreso es menor al cumpleaños, verifique de nuevo");
                if (DateTime.Now.Year - dateBirth.Year < 18) throw new Exception("El empleado que se ingresa debe ser mayor de 18 años");

                decimal salary;

                textBoxSalary.Text = textBoxSalary.Text.Replace(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator) //Para inputs con punto o coma
                       .Replace(".", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

                if (decimal.TryParse(textBoxSalary.Text, NumberStyles.Number, CultureInfo.CurrentCulture, out salary))
                {
                    // Redondear a 2 decimales si es necesario
                    salary = Math.Round(salary, 2);
                }
                else throw new FormatException("El valor ingresado como salario no es un número decimal válido.");


                int idSecction = -1;
                if (comboBoxSection.SelectedIndex != -1) // Se asume que comboBoxsecctions tiene la misma cantidad, orden e items que la base de datos (igualmente se valida mas adelante)
                {
                    var sections = conn.GetSecctions();

                    // Validar si el índice seleccionado es válido en el DataTable
                    if (comboBoxSection.SelectedIndex < sections.Rows.Count)
                    {
                        int comboBoxIndex = comboBoxSection.SelectedIndex;
                        int.TryParse(sections.Rows[comboBoxIndex]["codigo"].ToString(), out idSecction);
                    }
                    else
                    {
                        throw new Exception("El índice seleccionado no es válido.");
                    }
                }
                else throw new Exception("Elija una Seccion a la que el empleado pertenece");

                bool notAvalaible = true;
                if (comboBoxEnableEmployer.SelectedIndex == 0) notAvalaible = false; //Index 0 = habilitado, Index 1 = deshabilitado - en la base de dato se entiende baja logica true como un empleado no habilitado.

                Employers employerReady = new Employers(code, fullName, salary, dateBirth, dateJoin, notAvalaible, idSecction);
                if (action == TypeAction.New) control.AddUploadEmployer(employerReady);
                if (action == TypeAction.Edit) control.AddUpdateEmployer(employerReady);

                btnExit_Click(sender, e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ha occuido un error con un dato ingresado:\n {ex.Message}", "Error");
            }
        }

        /// <summary>
        /// Secction save button Process
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSaveSec_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBoxNameSec.Text))
            {
                try
                {
                    int code;
                    int.TryParse(textBoxCodeSec.Text, out code);

                    string name = textBoxNameSec.Text;

                    int idEmp = -1;

                    if (!string.IsNullOrEmpty(textBoxIDEmp.Text))
                    {
                        idEmp = int.Parse(textBoxIDEmp.Text);
                        if (!conn.AvalaibleEmployer(idEmp)) throw new Exception("El empleado asignado para ser responsable no existe");
                    }

                    Secctions secctionReady = new Secctions(code, name, idEmp);
                    if (action == TypeAction.New)  control.AddUploadSecctions(secctionReady);
                    if (action == TypeAction.Edit) control.AddUpdateSecctions(secctionReady);
                    btnExit_Click(sender, e);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ha occuido un error con un dato ingresado:\n {ex.Message}", "Error");
                }
            }else
            {
                MessageBox.Show("Debes Elejir un nombre antes de agregar una seccion", "Error");
            }
        }        
    }
}
