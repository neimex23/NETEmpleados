using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetEmpleados
{
    /// <summary>
    /// Representa a los Empleados
    /// </summary>
    public class Employers
    {
        private int _code;
        private string _fullName;
        private decimal _salary = 0;
        private DateTime _dateBirth;
        private DateTime _dateJoin;
        private bool _available = false;
        private int _secction;

        public int Code { get => _code; set => _code = value; }
        public string FullName { get => _fullName; set => _fullName = value; }
        public decimal Salary { get => _salary; set => _salary = value; }
        public DateTime DateBirth { get => _dateBirth; set => _dateBirth = value; }
        public DateTime DateJoin { get => _dateJoin; set => _dateJoin = value; }
        public bool NotAvailable { get => _available; set => _available = value; }
        public int Secction { get => _secction; set => _secction = value; }

        private Employers() { }

        public Employers(int code, string fullName, decimal salary, DateTime dateBirth, DateTime dateJoin, bool avalible, int secction)
        {
            this._code = code;
            this._fullName = fullName;
            this._dateBirth = dateBirth;
            this._dateJoin = dateJoin;
            this._available = avalible;
            this._secction = secction;
            this._salary = salary;
        }
    }
}
