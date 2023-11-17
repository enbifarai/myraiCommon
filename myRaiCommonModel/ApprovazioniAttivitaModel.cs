using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel
{
    public class ApprovazioniAttivitaModel
    {
        public ApprovazioniAttivitaModel()
        {
            this.ListaDate = new List<DataRichiesta>();
            var db= new myRaiData.digiGappEntities();
            this.EccezioniAll = db.L2D_ECCEZIONE.Select(x => new ShortEcc { codice = x.cod_eccezione, 
                                                                            descr = x.desc_eccezione }).ToList();
            this.RicercaModel = new RicercaDaApprovareAttivita();
        }
        public List<DataRichiesta> ListaDate { get; set; }
        public string matricola { get; set; }
        public List<ShortEcc> EccezioniAll { get; set; }
        public Boolean IsPreview { get; set; }
        public RicercaDaApprovareAttivita RicercaModel { get; set; }
        public Boolean MostraApprovaTutti { get; set; }

    }

    public class DataRichiesta
    {
        public DataRichiesta()
        {
            this.ListaAttivita = new List<myRaiData.MyRai_AttivitaCeiton>();
            this.MatricoleVisualizzateQuestaGiornata = new List<string>();
        }
        public DateTime data {get;set;}
        public List<myRaiData.MyRai_AttivitaCeiton> ListaAttivita { get; set; }
        public List<string> MatricoleVisualizzateQuestaGiornata { get; set; }
    }
    public class ShortEcc
    {
        public string codice { get; set; }
        public string descr { get; set; }
    }
}