using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NetEmpleados
{
    /// <summary>
    /// La clase Enum contiene dos enumeraciones.
    /// 
    /// - **TypeData**: Define los tipos de fechas que se manejan en la aplicación, con los valores:
    ///     - `Employer`: Representa una fecha relacionada con un empleado.
    ///     - `Secction`: Representa una fecha relacionada con una sección.
    ///     - `None`: Indica que no hay un tipo de fecha específico.
    /// 
    /// - **TypeAction**: Define las acciones que se pueden realizar en los datos, con los valores:
    ///     - `New`: Indica una acción de creación de un nuevo registro.
    ///     - `Edit`: Indica una acción de edición de un registro existente.
    ///     - `Remove`: Indica una acción de eliminación de un registro.
    /// </summary>
    public class Enum
    {
        public enum TypeData { Employer, Secction, None }

        public enum TypeAction { New, Edit, Remove }

        public static Dictionary<int, Enum.TypeData> typeMap = new Dictionary<int, Enum.TypeData>
        {
            { 0, Enum.TypeData.Employer },
            { 1, Enum.TypeData.Secction }
        };
    }
}
