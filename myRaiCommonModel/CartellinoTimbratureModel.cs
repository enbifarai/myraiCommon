using MyRaiServiceInterface.MyRaiServiceReference1;
using myRaiData;
using System.Collections.Generic;
using myRaiHelper;
using System;

namespace myRaiCommonModel
{
    public class CartellinoTimbratureModel
    {
        public CartellinoTimbratureModel()
        {
            giorni = new List<giorno>();
            Raggruppamenti = CommonHelper.GetRaggruppamenti();
        }
        public string MeseCorrenteString { get; set; }
        public int MeseCorrente { get; set; }
        public int AnnoCorrente { get; set; }
        public bool FrecciaAvanti { get; set; }
        public bool FrecciaIndietro { get; set; }
        public List<giorno> giorni { get; set; }

        public GetTimbratureMeseResponse DettaglioTimbrature { get; set; }
        public List<MyRai_Raggruppamenti> Raggruppamenti { get; set; }

        public GetSchedaPresenzeMeseResponse DettaglioPresenze { get; set; }

        public CalendarioFerie ListaGiorniEvidenza { get; set; }

        public List<DaEvidenziare> ListaEccezioniMese { get; set; }
    }

    public class giorno
    {
        public string day { get; set; }
        public int dayNumber { get; set; }
    }

    /// <summary>
    /// Oggetto utilizzato per mettere in evidenza una particolare
    /// eccezione in una determinata giornata
    /// Utilizzato nella vista delle Visualizzazioni Mensili per
    /// mettere in risalto eventuali eccezioni approvate/da approvare/in approvazione/rifiutate
    /// </summary>
    public class DaEvidenziare
    {
        public DateTime Data { get; set; }
        public string Cod_Eccezione { get; set; }
        public myRaiHelper.EnumStatiRichiesta StatoRichiesta { get; set; }
    }
}