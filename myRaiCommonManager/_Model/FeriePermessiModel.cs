using System;
using System.Collections.Generic;
using myRaiHelper;
using MyRaiServiceInterface.it.rai.servizi.digigappws;

namespace myRaiCommonModel
{
    public class FeriePermessiModel
    {
        public sidebarModel menuSidebar { get; set; }
        public pianoFerie pianoFerieModel { get; set; }

        public List<myRaiData.MyRai_Raggruppamenti> Raggruppamenti { get; set; }
        public int? anno { get; set; }
    }

    public class EccezioniGroupCounter
    {
        public EccezioniGroupCounter()
        {
            Eccezioni = new List<EccezioniCounter>();
        }
        public string Nome { get; set; }
        public List<EccezioniCounter> Eccezioni { get; set; }
    }

    public class EccezioniCounter
    {
        public string CodEccezione { get; set; }
        public string DesEccezione { get; set; }
        public MyRaiServiceInterface.MyRaiServiceReference1.GetContatoriEccezioniItem Response { get; set; }
    }

    public class GetPianoFerieMatricolaResponse
    {
        public List<DayInfo> Days { get; set; }
        public PianoFerieStatus PFstatus { get; set; }
        public Contatori ContatoriCics { get; set; }
        public AccordiSindacali2020 Accordi { get; set; }
        public Donazioni Donate { get; set; }
        public float MinimoDaPianificareEntroDataLimite { get; set; }
        public float MinimoDaPianificareEntro30Settembre { get; set; }
        public float MassimoPianificabile { get; set; }
    }
    public class AccordiSindacali2020
    {
        public float ArretratiDaFoglioExcel { get; set; }
        public bool HaEstensioneMarzo { get; set; }
        public float ArretratiRR { get; set; }
        public float ArretratiRF { get; set; }
        public float ArretratiFE { get; set; }
        public DateTime DataLimiteArretrati { get; set; }
    }
    public class Donazioni
    {
        public float FECE { get; set; }
        public float MNCE { get; set; }
        public float MFCE { get; set; }
        public float MRCE { get; set; }
        public float FEDO { get; set; }
        public float MNDO { get; set; }
        public float MRDO { get; set; }
    }

    public class DayInfo
    {
        public DateTime Data { get; set; }
        public GappInfo Gapp { get; set; }
        public PianoFerieInfo PianoFerie { get; set; }
    }
    public class PianoFerieStatus
    {
        public string sedegapp { get; set; }
        public int anno { get; set; }
        public DateTime? data_invio_dipendente { get; set; }
        public DateTime? data_invio_segreteria { get; set; }
        public string matricola_invio_segreteria { get; set; }
        public string nota_invio_segreteria { get; set; }
        public DateTime? data_approvata { get; set; }
        public int numero_versione { get; set; }
        public string matricola_approvatore { get; set; }
        public DateTime? data_firma { get; set; }
        public string matricola_firma { get; set; }
        public DateTime? data_storno_approvazione { get; set; }
        public string matricola_storno { get; set; }

    }
    public class PianoFerieInfo
    {
        public String EccezioneFeriePermesso { get; set; }
        public DateTime data_inserimento { get; set; }
    }
    public class GappInfo
    {
        public String EccezioneFeriePermesso { get; set; }
        public string TipoGiornata { get; set; }
        public string OrarioTeorico { get; set; }
        public string OrarioReale { get; set; }
    }

    public partial class Contatori
    {

        public float ferieAnniPrecedenti { get; set; }

        public float ferieSpettanti { get; set; }

        public float ferieUsufruite { get; set; }

        public float ferieRichieste { get; set; }

        public float feriePianificate { get; set; }

        public float ferieRimanenti { get; set; }

        public float exFestivitaAnniPrecedenti { get; set; }

        public float exFestivitaSpettanti { get; set; }

        public float exFestivitaUsufruite { get; set; }

        public float exFestivitaRichieste { get; set; }

        public float exFestivitaPianificate { get; set; }

        public float exFestivitaRimanenti { get; set; }

        public float permessiSpettanti { get; set; }

        public float permessiUsufruiti { get; set; }

        public float permessiRichiesti { get; set; }

        public float permessiPianificati { get; set; }

        public float permessiRimanenti { get; set; }

        public float permessiGiornalistiAnniPrecedenti { get; set; }

        public float permessiGiornalistiSpettanti { get; set; }

        public float permessiGiornalistiUsufruiti { get; set; }

        public float permessiGiornalistiRichiesti { get; set; }

        public float permessiGiornalistiPianificati { get; set; }

        public float permessiGiornalistiRimanenti { get; set; }

        public float mancatiNonLavoratiAnniPrecedenti { get; set; }

        public float mancatiNonLavoratiSpettanti { get; set; }

        public float recuperiNonLavoratiUsufruiti { get; set; }

        public float recuperiNonLavoratiRichiesti { get; set; }

        public float recuperiNonLavoratiPianificati { get; set; }

        public float recuperiNonLavoratiRimanenti { get; set; }

        public float mancatiRiposiAnniPrecedenti { get; set; }

        public float mancatiRiposiSpettanti { get; set; }

        public float recuperiMancatiRiposiUsufruiti { get; set; }

        public float recuperiMancatiRiposiRichiesti { get; set; }

        public float recuperiMancatiRiposiPianificati { get; set; }

        public float recuperiMancatiRiposiRimanenti { get; set; }

        public float mancatiFestiviAnniPrecedenti { get; set; }

        public float mancatiFestiviSpettanti { get; set; }

        public float recuperiMancatiFestiviUsufruiti { get; set; }

        public float recuperiMancatiFestiviRichiesti { get; set; }

        public float recuperiMancatiFestiviPianificati { get; set; }

        public float recuperiMancatiFestiviRimanenti { get; set; }

        public float ferieMinime { get; set; }

        public float totaleConvalidato { get; set; }

        public bool visualizzaFerie { get; set; }

        public bool visualizzaFC { get; set; }

        public bool visualizzaPermessi { get; set; }

        public bool visualizzaPermessiGiornalisti { get; set; }

        public bool visualizzaRecuperoRiposi { get; set; }

        public bool visualizzaRecuperoNonLavorati { get; set; }

        public bool visualizzaRecuperoFestivi { get; set; }

    }

    public class TestServiceResponse
    {
        public DateTime Data { get; set; }
        public string Eccezione { get; set; }
        public int ContatoreIntero { get; set; }
        public float ContatoreDecimal { get; set; }
        public bool Flag { get; set; }
    }
}
