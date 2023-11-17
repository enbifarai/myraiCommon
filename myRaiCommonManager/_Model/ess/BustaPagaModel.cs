using myRaiHelper;
using System.Collections.Generic;
using System.Linq;

namespace myRaiCommonModel.ess
{
    public class BustaPagaModel
    {
        //public List<it.rai.servizi.hrpaga.ListaDatiDocumenti> elencoDocumenti = new List<it.rai.servizi.hrpaga.ListaDatiDocumenti>();
        public IEnumerable<IGrouping<string, myRaiServiceHub.it.rai.servizi.hrpaga.ListaDatiDocumenti>> elencoDocumenti;
        public IEnumerable<IGrouping<string, myRaiServiceHub.it.rai.servizi.hrpaga.ListaDatiDocumenti>> elencoDocumentiperTipo;
        public IEnumerable<myRaiServiceHub.it.rai.servizi.hrpaga.ListaDatiDocumenti> elencoDocumentiLungo;
        public sidebarModel menuSidebar { get; set; }
        public bool flagUltimoAnno { get; set; }
        public bool nonLetti { get; set; }
        public int daleggere { get; set; }
        public string ultimabusta { get; set; }
        public bool pertipo { get; set; }
        public string descrizioneTipo { get; set; }
    }


}