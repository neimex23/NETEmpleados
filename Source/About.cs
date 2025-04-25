using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetEmpleados
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
        }

        private void linkLabelGitHub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url = "https://github.com/neimex23";

            // Abre la URL en el navegador predeterminado
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        private void About_Load(object sender, EventArgs e)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            // Obtiene la versión del ensamblado
            Version version = assembly.GetName().Version;

            linkLabelVersion.Text = version.ToString();
        }
    }
}
