using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using myRaiData;

namespace myRaiCommonModel
{
    public class MiaRichiesta
    {
        public string TestoRichiesta { get; set; }
        public DateTime Data { get; set; }
        public long IdRichiesta { get; set; }
        public string Stato { get; set; }
        public string Periodo { get; set; }
        public int IdStatoRichiesta { get; set; }
        public MyRai_Eccezioni_Richieste EccezioneDiRiferimentoPerStorno { get; set; }
        public int? NumeroDocumento { get; set; }
        public Boolean IsStorno { get; set; }

        public DateTime? DataRifiutoLiv1 { get; set; }
        public string NotaRifiutoOApprovazione { get; set; }
        public string NominativoLiv1 { get; set; }
        public DateTime?  DataValidazioneLiv1 { get; set; }

        public string NdocChildrenCsv { get; set; }

        public int? IdDocumentoAssociato { get; set; }
        public MyRai_Eccezioni_Richieste EccezioneCorrenteDaDB { get; set; }

        public string PeriodoRichiesta1 { get; set; }
        public string PeriodoRichiesta2 { get; set; }

        public bool no_corrispondenza_gapp { get; set; }
    }
}