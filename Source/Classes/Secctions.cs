using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetEmpleados
{
    /// <summary>
    /// Representa a las Secciones
    /// </summary>
    public class Secctions
    {
        private int _code;
        private string _name;
        private int _idEmp;

        public int Code { get => _code; set => _code = value; }
        public string Name { get => _name; set => _name = value; }
        public int IdRes { get => _idEmp; set => _idEmp = value; }

        private Secctions() { }

        public Secctions(int code, string name, int idEmp)
        {
            this._code = code;
            this._name = name;
            this._idEmp = idEmp;
        }
    }
}
