using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel
{
    public class VisualizzaEccezioniModel
    {
        public VisualizzaEccezioniModel()
        {
            Eccezioni = new List<myRaiData.MyRai_Regole_SchedeEccezioni>();
        }
        public List<myRaiData.MyRai_Regole_SchedeEccezioni> Eccezioni { get; set; }
        public List<myRaiData.MyRai_Regole_Tematiche> Tematiche { get; set; }

        public List<myRaiData.MyRai_Regole_TipoAssenza> TipiAssenza { get; set; }

        public List<string> Visibili { get; set; }
    }
}