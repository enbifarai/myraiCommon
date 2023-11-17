using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiCommonModel
{
    class StatiRapportoModel
    {
    }

    public class DipendentiRapporto : BaseAnagrafica
    {
        public AnagraficaStatiRapporti StatiRapporti { get; set; }
    }
}
