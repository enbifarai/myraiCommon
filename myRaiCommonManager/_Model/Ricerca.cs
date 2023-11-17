using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel
{
    public class Ricerca
    {
        public List<Sede> ListaSedi { get; set; }
        public int selectedId { get; set; }
        public List<String> TipoEccezione { get; set; }

        public class Sede
        {
            public string CodiceSede { get; set; }
            public string DescrizioneSede { get; set; }
        }



    }
}