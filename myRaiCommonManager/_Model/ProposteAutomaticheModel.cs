using System;
using System.Collections.Generic;

namespace myRaiCommonModel
{
    public class ProposteAutomaticheModel
    {

        public MyRaiServiceInterface.it.rai.servizi.digigappws.Eccezione[] EccezioniProposte { get; set; }
        public List<OpzioneProposta> MacroEccezioniProposte { get; set; }
        public Boolean ShowPannelloCarenzeButton { get; set; }

        
    }
    public class PropostaAutoToSave
    { 
        public string d{get;set;}
        public string cod{get;set;}
        public int index { get; set; }
        public string nota { get; set; }

        public string dalle { get; set; }
        public string alle { get; set; }
        public string quantita { get; set; }
        public string idAttivitaCeiton { get; set; }
        public string MatricolaApprovatoreProduzione { get; set; }
    }

    public class RisultatoAnalisiEccezione
    {
        public Boolean esito { get; set; }
        public string descrizioneErrore { get; set; }
    }

    public class OpzioneProposta
    {
        public string testo { get; set; }
        public List<MyRaiServiceInterface.it.rai.servizi.digigappws.Eccezione> eccezioniProposte { get; set; }
    }

}