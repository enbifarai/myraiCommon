using myRaiCommonManager;
using myRaiData.Incentivi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiCommonModel
{
    public class DelegheVM
    {
        public string MatricolaCorrente { get; set; }

        public List<AbilitazioniPersExt> DelegheRicevute { get; set; }
        public List<AbilitazioniPersExt> DelegheConcesse { get; set; }
    }

    public class AbilitazioniPersExt: AbilitazioniPers
    {
        public int IdDelega { get; set; }
        public string Nominativo { get; set; }
        public string Descrizione { get; set; }
        public bool InEsercizio { get; set; }
    }

    public class DelegaModelVM
    {
        public string NomeDelega { get; set; }
        public string MatricolaDelegante { get; set; }
        public string NominativoDelegante { get; set; }
        public string MatricolaDelegato { get; set; }
        public string NominativoDelegato { get; set; }
        public DateTime DataCreazioneDelega { get; set; }
        public DateTime DataInizioDelega { get; set; }
        public DateTime DataFineDelega { get; set; }
        public List<XR_HRIS_ABIL> Abilitazioni { get; set; }
    }

    public class DelegheResult
    {
        public bool Esito { get; set; }
        public string Errore { get; set; }
        public Object Obj { get; set; }
      
    }
}
