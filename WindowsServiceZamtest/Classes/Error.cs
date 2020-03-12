using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsServiceZamtest.Classes
{
    public class Error
    {
        public Dictionary<int, string> keyValues = new Dictionary<int, string>()
        {
            { 1, "No se ha iniciado el secuenciador" },
            { 2, "Error al inicializar algun instrumento" },
            { 3, "No se ha seleccionado ninguna variante" },
            { 4, "No se ha iniciado secuencia" }
        };

        public string Description { get; set; }
        public int Code { get; set; }

    }
}
