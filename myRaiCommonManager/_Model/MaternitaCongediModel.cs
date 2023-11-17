using myRaiCommonModel.Gestionale.HRDW;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiCommonModel
{
    public class MaternitaApprovazioniModel
    {
        

        public MaternitaApprovazioniModel()
        {
            RichiesteAggregate = new List<RichiestePerMatricola>();
        }
        public bool IsPreview { get; set; }
        public MaternitaCongediHelper.MaternitaCongediUffici MyOffice { get; set; }
        public List<RichiestePerMatricola> RichiesteAggregate { get; set; }
    }
    public class MaternitaIndexModel
    {
        public MaternitaIndexModel()
        {
            Categorie = new List<myRaiData.Incentivi.XR_MAT_MACROCATEGORIE>();
            Sedi = new List<SedeModel>();
            Stati = new List<myRaiData.Incentivi.XR_MAT_STATI>();
            this.StraordinariCaricati = myRaiCommonManager.MaternitaCongediManager.StraordinariCaricati();
        }
        public List<myRaiData.Incentivi.XR_MAT_MACROCATEGORIE> Categorie { get; set; }
        public List<myRaiData.Incentivi.XR_MAT_STATI> Stati { get; set; }
        public List<SedeModel> Sedi { get; set; }
        
        public bool EnabledToGestioneGeneric { get; set; }
        public string JSstringApriRichiesta { get; set; }

        public bool IsResponsabileGestione { get; set; }
        public bool MostraSoloDaRiavviare { get; set; }

        public bool StraordinariCaricati { get; set; }

    }
    public class MaternitaCongediModel
    {
        public MaternitaCongediModel()
        {
            RichiesteInCaricoAltri = new List<myRaiData.Incentivi.XR_MAT_RICHIESTE>();
            RichiesteInCaricoAme = new List<myRaiData.Incentivi.XR_MAT_RICHIESTE>();
            RichiesteInCaricoNessuno = new List<myRaiData.Incentivi.XR_MAT_RICHIESTE>();

            RichiesteAggregateInCaricoAme = new List<RichiestePerMatricola>();
            RichiesteAggregateInCaricoAltri = new List<RichiestePerMatricola>();
            RichiesteAggregateInCaricoNessuno = new List<RichiestePerMatricola>();
        }
        public bool IsPreview { get; set; }
        public List<myRaiData.Incentivi.XR_MAT_RICHIESTE> RichiesteInCaricoAme { get; set; }
        public List<myRaiData.Incentivi.XR_MAT_RICHIESTE> RichiesteInCaricoAltri { get; set; }
        public List<myRaiData.Incentivi.XR_MAT_RICHIESTE> RichiesteInCaricoNessuno { get; set; }

        public bool OpenByUfficioPersonale { get; set; }
        public int? RicercaPerID_STATO { get; set; }
        public MaternitaCongediHelper.MaternitaCongediUffici MyOffice { get; set; }

        public List<RichiestePerMatricola> RichiesteAggregateInCaricoAme { get; set; }
        public List<RichiestePerMatricola> RichiesteAggregateInCaricoAltri { get; set; }
        public List<RichiestePerMatricola> RichiesteAggregateInCaricoNessuno { get; set; }
        public bool OpenByUfficioAmministrazione { get; set; }
        public string OrdineRichiesto { get; set; }
        public bool FromRicerca { get; set; }
    }
    public class RichiestePerMatricola
    {
        public RichiestePerMatricola()
        {
            ListaRichiesteAggregate = new List<myRaiData.Incentivi.XR_MAT_RICHIESTE>();
        }
        public string Matricola { get; set; }
        public DateTime DataRiferimentoOrdineVisualizzazione { get; set; }
        public List<myRaiData.Incentivi.XR_MAT_RICHIESTE> ListaRichiesteAggregate { get; set; }
    }
    public class MaternitaDettagliGestioneModel
    {
        public MaternitaDettagliGestioneModel()
        {
            ListInfoMatricola = new List<InfoMatricola>();
            EccezioniPossibili = new List<myRaiData.L2D_ECCEZIONE>();
        }
        public myRaiData.Incentivi.XR_MAT_RICHIESTE Richiesta { get; set; }
        public bool InCaricoAMe { get; set; }

        public List<InfoMatricola> ListInfoMatricola { get; set; }
        public bool FromApprovatoreGestione { get; set; }
        public List<myRaiData.L2D_ECCEZIONE> EccezioniPossibili { get; set; }
    }
    public class InfoMatricola
    {
        public string matricola { get; set; }
        public string nominativo { get; set; }
        public string posizione { get; set; }
    }
    public class SediMaternitaModel
    {
        public SediMaternitaModel()
        {
            Sedi = new List<SedeModel>();
        }
        public List<SedeModel> Sedi { get; set; }
    }
    public class SedeModel
    {
        public string CodiceSede { get; set; }
        public string DescSede { get; set; }
    }
    public class MaternitaSegnalazioneModel
    {
        public List<InfoMatricola> ListInfoMatricole { get; set; }
        public myRaiData.Incentivi.XR_MAT_SEGNALAZIONI Segnalazione { get; set; }
    }
    public class MaternitaDettagliRichiestaGestioneModel
    {
        public myRaiData.Incentivi.XR_MAT_RICHIESTE Richiesta { get; set; }
    }
    public class AllegatiReviewModel
    {
        public AllegatiReviewModel()
        {
            Allegati = new List<myRaiData.Incentivi.XR_MAT_ALLEGATI>();
            TipoAllegati = new myRaiData.Incentivi.IncentiviEntities().XR_MAT_TIPOALLEGATI.ToList();
            //new List<myRaiData.Incentivi.XR_MAT_TIPOALLEGATI>();
        }
        public List<myRaiData.Incentivi.XR_MAT_ALLEGATI> Allegati { get; set; }
        public List<myRaiData.Incentivi.XR_MAT_TIPOALLEGATI> TipoAllegati { get; set; }
        public bool IsSegnalazione { get; set; }
        public bool NoMarginTop { get; set; }
        public bool InCaricoAMe { get; set; }
        public int StatoRichiesta { get; set; }
        public string Categoria { get; set; }
    }

    public class ChatItemModel
    {
        public ChatItemModel()
        {
            IdAllegatiGiaInValutazione = new List<int>();
        }
        public string matricola { get; set; }
        public myRaiData.Incentivi.XR_MAT_SEGNALAZIONI_COMUNICAZIONI chatitem { get; set; }
        public bool IsOpeningItem { get; set; }
        public List<InfoMatricola> ListInfoMatricole { get; set; }
        public List<int> IdAllegatiGiaInValutazione { get; set; }
    }

    public class AnnullamentoBoxModel
    {

        public AnnullamentoBoxModel()
        {
            IsGestOrAdm =
               MaternitaCongediHelper.EnabledToMaternitaCongediDetail(MaternitaCongediHelper.MaternitaCongediUffici.Gestione,
                MaternitaCongediHelper.MaternitaCongediGradiAbil.ADM)
                ||
                MaternitaCongediHelper.EnabledToMaternitaCongediDetail(MaternitaCongediHelper.MaternitaCongediUffici.Gestione,
                MaternitaCongediHelper.MaternitaCongediGradiAbil.GEST);
        }
        public bool IsGestOrAdm { get; set; }
        public myRaiData.Incentivi.XR_MAT_RICHIESTE Richiesta { get; set; }
    }
    public class AssegnazioneBoxModel
    {
        public AssegnazioneBoxModel()
        {
            IsGestOrAdm =
                MaternitaCongediHelper.EnabledToMaternitaCongediDetail(MaternitaCongediHelper.MaternitaCongediUffici.Gestione,
                 MaternitaCongediHelper.MaternitaCongediGradiAbil.ADM)
                 ||
                 MaternitaCongediHelper.EnabledToMaternitaCongediDetail(MaternitaCongediHelper.MaternitaCongediUffici.Gestione,
                 MaternitaCongediHelper.MaternitaCongediGradiAbil.GEST);
        }
        public myRaiData.Incentivi.XR_MAT_RICHIESTE Richiesta { get; set; }
        public bool PossibileAssegnare { get; set; }
        public bool IsGestOrAdm { get; set; }
        public List<NominativoMatricola> AssegnatariPossibili { get; set; }

    }
    public class ProfiloImageModel
    {
        public string Row1 { get; set; }
        public string Row2 { get; set; }
        public string Matricola { get; set; }
        public myRaiHelper.EnumPresenzaDip? InServizio { get; set; }
        public string display { get; set; }
        public bool NascondiImage { get; set; }
        public bool Checkbox { get; set; }
    }
    public class MatPianificazioneModel
    {
        public myRaiData.Incentivi.XR_MAT_RICHIESTE Richiesta { get; set; }
        public MyRaiServiceInterface.it.rai.servizi.digigappws.pianoFerie pf { get; set; }
        public DateTime DataInizialeCalendario { get; set; }
        public DateTime DataFinaleCalendario { get; set; }
        public bool isFromApprovazione { get; set; }
    }
    public class MatCalendarioTracciatiModel
    {
        public myRaiData.Incentivi.XR_MAT_RICHIESTE Richiesta { get; set; }
        public MyRaiServiceInterface.it.rai.servizi.digigappws.pianoFerie pf { get; set; }
        public DateTime DataInizialeCalendario { get; set; }
        public DateTime DataFinaleCalendario { get; set; }
        public bool isFromApprovazione { get; set; }
        public bool IsFromDem { get; set; }
        public int? IDdocumentoDEM { get; set; }
        public string EccezioneFromDEM { get; set; }
        public string NominativoFromDEM { get; set; }
        public DateTime? InizioPeriodoDEM { get; set; }
        public DateTime? FinePeriodoDEM { get; set; }
        public List<myRaiData.MyRai_PianoFerieBatch> ListaPianoFerieBatch { get; set; }
    }
    public class UffPersBoxModel
    {
        public myRaiData.Incentivi.XR_MAT_RICHIESTE Richiesta { get; set; }
        public bool isUffPers { get; set; }
        public string EccezioniInserite { get; set; }
    }

    public class DettaglioAmministrazioneModel
    {
        public DettaglioAmministrazioneModel()
        {
            ElencoGiorniPerMese = new List<DettaglioGiorniPerMese>();
            MesiAnnoCompetenza = new List<DateTime>();
        }
        public float TotaleGiorni { get; set; }
        public float ImportoFinale { get; set; }
        public bool InCaricoAMe { get; set; }
        public myRaiData.Incentivi.XR_MAT_RICHIESTE Richiesta { get; set; }
        public string MeseCompetenza { get; set; }
        public string AnnoCompetenza { get; set; }
        public string MeseAnnoCompetenza { get; set; }
        public List<DettaglioGiorniModel> ElencoGiorni { get; set; }
        public List<DettaglioGiorniPerMese> ElencoGiorniPerMese { get; set; }
        public bool OperazioniAvviate { get; set; }
        public string EccezioneRisultante { get; set; }
        public List<DateTime> MesiAnnoCompetenza { get; set; }
        public string MesePerCalcoloCedolino { get; set; }
        public string AnnoPerCalcoloCedolino { get; set; }
        public bool DataAttualeOltreChiusura { get; set; }
        public RaccoltaMesePrecedente RaccoltaMesePrec { get; set; }
        public string FormaContratto { get; set; }
        public int AggiungiGiorniCount { get; set; }
        public int AggiungiGiorniMonth { get; set; }
        public int AggiungiGiorniYear { get; set; }
        public double? TotaleGiornalieroHC { get; set; }
        public List<HRDWdettagliSTR> DettagliSTR { get; set; }
    }
    public class RaccoltaMesePrecedente
    {
        public int Anno { get; set; }
        public int Mese { get; set; }
        public float TotaleGiorni { get; set; }
        public MyRaiServiceInterface.MyRaiServiceReference1.GetRuoliResponse Response { get; set; }
    }
    public class DettaglioGiorniPerMese
    {
        public DettaglioGiorniPerMese()
        {
            ElencoGiorni = new List<DettaglioGiorniModel>();
        }
        public DateTime RiferimentoPrimoDelMese { get; set; }
        public List<DettaglioGiorniModel> ElencoGiorni { get; set; }
        public float TotaleGiorni { get; set; }
        public bool CompresoTotalmente { get; set; }
    }
    public class DettaglioGiorniModel
    {
        public DettaglioGiorniModel()
        {
            IntervalliFusi = new List<string>();
        }
        public DateTime DataDa { get; set; }
        public DateTime? DataA { get; set; }
        public string CodiceEccezione { get; set; }
        public string DescEccezione { get; set; }
        public float NumeroGiorniRuoli { get; set; }
        public float NumeroGiorniGapp { get; set; }
        public bool NonPresenteSuGetAnalisiEccezioni { get; set; }
        public bool NonPresenteSuGetRuoli { get; set; }
        public string NotaFusione { get; set; }
        public bool Soppresso { get; set; }
        public string StatoEccez { get; set; }
        public string TipoTask { get; set; }
        public int Fusioni { get; set; }
        public List<string> IntervalliFusi { get; set; }
        public bool AggiuntoForzato { get; set; }
        public bool IsMTfiller { get; set; }
        public bool Stornato { get; internal set; }
        public bool Storno { get; internal set; }
        public string CodiceOrario { get; set; }
    }
    public class DettagliRichiestaAmmModel
    {
        public myRaiData.Incentivi.XR_MAT_RICHIESTE Richiesta { get; set; }
        public bool IsFromPeriodiPopup { get; set; }
        public string InCarico { get; set; }
        public List<XR_DEM_DOCUMENTI_EXT> InfoDematerializzazione { get; set; }
        public bool InCaricoAMe { get; set; }
    }
    public class AnnullamentoAmmModel : MCmodel
    {
        public AnnullamentoAmmModel(int idrichiesta) : base(idrichiesta)
        {

        }
    }
    public class AssegnazioneAmmModel
    {
        public myRaiData.Incentivi.XR_MAT_RICHIESTE Richiesta { get; set; }
        public bool PossibileAssegnare { get; set; }
        public bool PuoiPrendereIncarico { get; set; }
        public bool PuoiRilasciare { get; set; }
        public List<NominativoMatricola> AssegnatariPossibili { get; set; }
        public bool SonoADM { get; set; }
        public bool InCaricoACollegaAssente { get; set; }
        public string FunzioneJSPrendiInCarico { get; set; }
        public string FunzioneJSRilascia { get; set; }
        public string FunzioneJSAssegna { get; set; }
    }

    public class EccezioneCongedi
    {
        public string eccezione { get; set; }
        public string datada { get; set; }
        public string dataa { get; set; }
        public float giorni { get; set; }
        public float giorni_old { get; set; }
        public int idrichiesta { get; set; }
        public string meserif { get; set; }
        public string statoeccezione { get; set; }
    }
    public class TaskSavingModel
    {
        public int idrichiesta { get; set; }
        public int idtask { get; set; }
        public int progressivotask { get; set; }
        public int attivo { get; set; }
        public string periododa { get; set; }
        public string periodoa { get; set; }
        public string eccezionerisultante { get; set; }
        public int mese { get; set; }
        public int anno { get; set; }
        // public string giorni26mi { get; set; }
        // public string importofinale { get; set; }
        public string tracciatointero { get; set; }
        public int idtaskincorso { get; set; }
        public int idaltrapratica { get; set; }
        public string isfromTE { get; set; }
    }
    public class CedolinoGenerico
    {

        public string meseannocedolino { get; set; }
        public int idrichiesta { get; set; }

        public float stipendio { get; set; }
        public float stipendio_old { get; set; }

        public float ind_contingenza { get; set; }
        public float ind_contingenza_old { get; set; }

        public float ind_mensa { get; set; }
        public float ind_mensa_old { get; set; }

        public float elemento_distinto { get; set; }
        public float elemento_distinto_old { get; set; }

        public float edr { get; set; }
        public float edr_old { get; set; }

        public float ind_11C { get; set; }
        public float ind_11C_old { get; set; }

        public float ind_11e { get; set; }
        public float ind_11e_old { get; set; }

        public float ind_106 { get; set; }
        public float ind_106_old { get; set; }

        public float ind_111 { get; set; }
        public float ind_111_old { get; set; }

        public float ind_131 { get; set; }
        public float ind_131_old { get; set; }

        public float ind_116 { get; set; }
        public float ind_116_old { get; set; }

        public float ind_11A { get; set; }
        public float ind_11A_old { get; set; }

        public float ind_108 { get; set; }
        public float ind_108_old { get; set; }

        public float ind_109 { get; set; }
        public float ind_109_old { get; set; }

        public float ind_133 { get; set; }
        public float ind_133_old { get; set; }

        public float ind_146 { get; set; }
        public float ind_146_old { get; set; }

        public float ind_147 { get; set; }
        public float ind_147_old { get; set; }

        public float ind_159 { get; set; }
        public float ind_159_old { get; set; }

        public float ind_141 { get; set; }
        public float ind_141_old { get; set; }

        public float ind_altre { get; set; }
        // public float ind_altre_old { get; set; }

        public float redazionale { get; set; }
        public float redazionale_old { get; set; }

        public float str_precedente { get; set; }
        public float mens_13ma { get; set; }
        public float mens_14ma { get; set; }
        public float premio_prod { get; set; }
        public float ImportoTotale { get;
            set;
        }

        //GIORNALISTI
        public float ind_110 { get; set; }
        public float ind_110_old { get; set; }

        public float ind_120 { get; set; }
        public float ind_120_old { get; set; }

        public float ind_123 { get; set; }
        public float ind_123_old { get; set; }

        public float ind_129 { get; set; }
        public float ind_129_old { get; set; }

        public float ind_130 { get; set; }
        public float ind_130_old { get; set; }

        public float ind_114 { get; set; }
        public float ind_114_old { get; set; }

        public float ind_119 { get; set; }
        public float ind_119_old { get; set; }

        public float ind_12B { get; set; }
        public float ind_12B_old { get; set; }

        public float ind_13B { get; set; }
        public float ind_13B_old { get; set; }

        public float ind_14E { get; set; }
        public float ind_14E_old { get; set; }

        public float ind_14F { get; set; }
        public float ind_14F_old { get; set; }

        public float minimo { get; set; }
        public float minimo_old { get; set; }

        public float superminimo { get; set; }
        public float superminimo_old { get; set; }

        public float DG55 { get; set; }
        public float LN20 { get; set; }
        public float LN16 { get; set; }
        public float LN50 { get; set; }
        public float PDCO { get; set; }
        public float LF80 { get; set; }
        public float LF36 { get; set; }
        public float LEXF { get; set; }
        public float AR20 { get; set; }
        public float RPAF { get; set; }
        public float LN25 { get; set; }

        //Orchestrali
        public float ind_102 { get; set; }
        public float ind_102_old { get; set; }

        public float ind_149 { get; set; }
        public float ind_149_old { get; set; }

        //ultime inserite 21/02/2022--------------------------------------------------------------------
        
        //public float ind_108 { get; set; }
        //public float ind_108_old { get; set; }

        //public float ind_109 { get; set; }
        //public float ind_109_old { get; set; }

        //public float ind_110 { get; set; }
        //public float ind_110_old { get; set; }

        //public float ind_111 { get; set; }
        //public float ind_111_old { get; set; }

        public float ind_113 { get; set; }
        public float ind_113_old { get; set; }

        //public float ind_114 { get; set; }
        //public float ind_114_old { get; set; }

        public float ind_115 { get; set; }
        public float ind_115_old { get; set; }

        //public float ind_116 { get; set; }
        //public float ind_116_old { get; set; }

        //public float ind_119 { get; set; }
        //public float ind_119_old { get; set; }

        //public float ind_11A{ get; set; }
        //public float ind_11A_old { get; set; }

        public float ind_11B { get; set; }
        public float ind_11B_old { get; set; }

        //public float ind_11C { get; set; }
        //public float ind_11C_old { get; set; }

        public float ind_11E { get; set; }
        public float ind_11E_old { get; set; }

        public float ind_11F { get; set; }
        public float ind_11F_old { get; set; }

        public float ind_11G { get; set; }
        public float ind_11G_old { get; set; }

        //public float ind_120 { get; set; }
        //public float ind_120_old { get; set; }

        //public float ind_123 { get; set; }
        //public float ind_123_old { get; set; }

        public float ind_124 { get; set; }
        public float ind_124_old { get; set; }

        public float ind_125 { get; set; }
        public float ind_125_old { get; set; }

        public float ind_126 { get; set; }
        public float ind_126_old { get; set; }

        public float ind_127 { get; set; }
        public float ind_127_old { get; set; }

        public float ind_128 { get; set; }
        public float ind_128_old { get; set; }

        //public float ind_129 { get; set; }
        //public float ind_129_old { get; set; }

        //public float ind_12B { get; set; }
        //public float ind_12B_old { get; set; }

        //public float ind_130 { get; set; }
        //public float ind_130_old { get; set; }

        //public float ind_131 { get; set; }
        //public float ind_131_old { get; set; }

        //public float ind_133 { get; set; }
        //public float ind_133_old { get; set; }

        public float ind_138 { get; set; }
        public float ind_138_old { get; set; }

        public float ind_13A { get; set; }
        public float ind_13A_old { get; set; }

        //public float ind_13B { get; set; }
        //public float ind_13B_old { get; set; }

        public float ind_13E { get; set; }
        public float ind_13E_old { get; set; }

        public float ind_13P { get; set; }
        public float ind_13P_old { get; set; }

        public float ind_13R { get; set; }
        public float ind_13R_old { get; set; }

        //public float ind_141 { get; set; }
        //public float ind_141_old { get; set; }

        public float ind_142 { get; set; }
        public float ind_142_old { get; set; }

        public float ind_145 { get; set; }
        public float ind_145_old { get; set; }

        //public float ind_146 { get; set; }
        //public float ind_146_old { get; set; }

        //public float ind_147 { get; set; }
        //public float ind_147_old { get; set; }

        public float ind_14A { get; set; }
        public float ind_14A_old { get; set; }

        public float ind_150 { get; set; }
        public float ind_150_old { get; set; }

        public float ind_152 { get; set; }
        public float ind_152_old { get; set; }

        public float ind_153 { get; set; }
        public float ind_153_old { get; set; }

        public float ind_157 { get; set; }
        public float ind_157_old { get; set; }
 

        //public float ind_159 { get; set; }
        //public float ind_159_old { get; set; }

    }


    public class CedolinoGiornalisti
    {
        public int idrichiesta { get; set; }
        public float stipendio { get; set; }
        public float ind_contingenza { get; set; }
        public float ind_mensa { get; set; }
        public float ind_110 { get; set; }
        public float ind_120 { get; set; }
        public float ind_123 { get; set; }
        public float ind_129 { get; set; }
        public float ind_130 { get; set; }
        public float ind_114 { get; set; }
        public float ind_119 { get; set; }
        public float ind_12B { get; set; }
        public float ind_13B { get; set; }
        public float ind_14E { get; set; }
        public float ind_14F { get; set; }
        public float minimo { get; set; }
        public float superminimo { get; set; }
        public float DG55 { get; set; }
        public float LN20 { get; set; }
        public float LN25 { get; set; }
        public float PDCO { get; set; }
        public float LF80 { get; set; }
        public float LF36 { get; set; }
        public float LEXF { get; set; }
        public float AR20 { get; set; }
        public float RPAF { get; set; }
        public float mens_13ma { get; set; }
        public float mens_14ma { get; set; }
        public float premio_prod { get; set; }
        public float redazionale { get; set; }
        public float ImportoTotale { get; set; }
    }

    public class DettaglioCedolinoItemModel
    {
        public string Etichetta { get; set; }
        public string NameAttuale { get; set; }
        public string NameHRDW { get; set; }
        public string CellaCalcoloDaExcel { get; set; }
        public float ValoreAttuale { get; set; }
        public float ValoreHRDW { get; set; }
        public bool ModificaAbilitata { get; set; }
        public string TipiDipendenteSpettanti { get; set; }
        public DettaglioCedolinoItemCalcolatoModel CalcolatoModel { get; set; }
        public bool IsGiornalista { get; set; }
    }
    public class SavePeriodModel
    {
        public int id { get; set; }
        public string cf { get; set; }
        public string p1 { get; set; }
        public string p2 { get; set; }
        public string inps { get; set; }
    }
    public class PeriodoArretratoDipendente
    {
        public DateTime D1 { get; set; }
        public DateTime D2 { get; set; }
        public string Eccezione { get; set; }
        public float Quantita { get; set; }
        public string QuantitaMesiGiorni { get; set; }
        public float GiorniSpuri { get; set; }
        public int MesiInteri { get; set; }
        public int? IdRichiestaInCorso { get; set; }
        public string CFfiglio { get; set; }
    }
    public class DettaglioCedolinoItemCalcolatoModel
    {
        public string Etichetta { get; set; }
        public float ValoreAttuale { get; set; }
        public string id_label { get; set; }
        public string id_hidden { get; set; }
        public string NameAttuale { get; set; }
        public float? GiorniEccezione { get; set; }
        public List<myRaiCommonModel.Gestionale.HRDW.HRDWdettagliSTR> DettagliSTR { get; set; }
        public bool IsFromFormaContrattoK { get; set; }
    }
    public class DettaglioCedolinoModel : MCmodel
    {
        public DettaglioCedolinoModel(int idrichiesta) : base(idrichiesta)
        {
            VociGeneriche = new CedolinoGenerico();
            VociGiornalisti = new CedolinoGiornalisti();
        }

        public CedolinoGenerico VociGeneriche { get; set; }
        public CedolinoGiornalisti VociGiornalisti { get; set; }
        public bool ModificaAbilitata { get; set; }
        public string TipoDipendente { get; set; }
        public List<TotaleEccezione> QuantitaEccezioni { get; set; }
        public List<DettaglioCedolinoItemModel> ListaItemCedolino { get; set; }
        public string MesePerCalcolo { get; set; }
        public string AnnoPerCalcolo { get; set; }
        public string FormaContratto { get; set; }
        public List<myRaiCommonModel.Gestionale.HRDW.HRDWdettagliSTR> DettagliSTR {get;set;}
        public decimal LordoMedioMensile { get; set; }
        public decimal Medio175Mensile { get; set; }
        public decimal Medio178Mensile { get; set; }
        public decimal Medio179Mensile { get; set; }
    }
    public class TotaleEccezione
    {
        public string eccezione { get; set; }
        public float totali { get; set; }
    }
    public class MCmodel
    {
        public MCmodel(int idrichiesta)
        {
            var db = new myRaiData.Incentivi.IncentiviEntities();
            this.Richiesta = db.XR_MAT_RICHIESTE.Where(x => x.ID == idrichiesta).FirstOrDefault();
        }
        public myRaiData.Incentivi.XR_MAT_RICHIESTE Richiesta { get; set; }
    }
    public class NotaModel : MCmodel
    {
        public NotaModel(int idrichiesta) : base(idrichiesta)
        {

        }
        public bool InCaricoAMe { get; set; }
        public string MioUfficioPerNote { get; set; }
    }

    public class PromemoriaModel
    {
        public myRaiData.Incentivi.XR_MAT_RICHIESTE Richiesta { get; set; }
        public bool InCaricoAMe { get; set; }
        public string UfficioGPA { get; set; }
    }
    public class IsAvviataResponse
    {
        public bool Avviata { get; internal set; }
        public string MatricolaOperatore { get; internal set; }
        public DateTime? DataAvviata { get; internal set; }
    }
    public class SingolaNotaModel
    {
        public myRaiData.Incentivi.XR_MAT_NOTE nota { get; set; }
    }
    public class ContentRowModel  
    {
        
        public bool OpenByUfficioPersonale { get; set; }
        public bool OpenByUfficioAmministrazione { get; set; }
        public int? IDSTATOricercato { get; set; }
        public MaternitaCongediHelper.MaternitaCongediUffici MyOffice { get; set; }
        public bool hideImage { get; set; }
        public bool OperazioniAvviateAny { get; set; }
        public bool IncaricoAnessuno { get; set; }
        public bool NeedCheckbox { get; set; }
        public bool IsOpenByApprovazioneResponsabileGestione { get; set; }
        public bool SegnalaInterventoRichiesto { get; set; }
        public myRaiData.Incentivi.XR_MAT_RICHIESTE Richiesta { get; set; }

    }
    public class Mesi26fromFrontEnd
    {
        public DateTime PrimoDelMese { get; set; }
        public double Giorni26mi { get; set; }
        public bool ModificatoDaCalendarioPDF { get; set; }
    }
    public class TaskModel : MCmodel
    {
        public TaskModel(int idrichiesta) : base(idrichiesta)
        {
            TaskDaVisualizzareModel = new  List<TaskDaVisualizzare>();
        }
        public string GiorniForzatiCSV { get; set; }
        public int StatoFinalePratica { get; set; }
        public List<ContenutoCampiPerTask> ContenutoCampi { get; set; }
        public List<DateTime> DateRiferimentoPrimoDelMeseTaskNecessari { get; set; }
        public DettaglioAmministrazioneModel DettaglioAmmModel { get; set; }
        public string EccezioneRisultante { get; set; }
        public List<ContenutoCampiPerMeseTask> TracciatiEsplosi { get; set; }

        public List<TaskDaVisualizzare> TaskDaVisualizzareModel { get; set; }

        public bool PuoConcluderePratica { get; set; }

        public string Importo13ma { get; set; }
        public string Importo14ma { get; set; }
        public string ImportoPremio { get; set; }
        public List<TaskPronto> ListaTaskPronti { get; set; }

        public List<myRaiData.Incentivi.XR_MAT_RICHIESTE> RichiesteCompreseInQuestoPeriodo { get; set; }
        public bool IsTrattamentoEconomico { get; set; }
        public List<Mesi26fromFrontEnd> Giorni26FrontEnd { get; set; }
    }

    public class TaskDaVisualizzare
    {
        public TaskDaVisualizzare()
        {
            Info = new List<TaskDaVisualizzareInfo>();
        }
        public DateTime DataPrimoDelMese { get; set; }
        public List<TaskDaVisualizzareInfo> Info { get; set; }
        public int IdRichiestaCheGiaVisualizzaTask { get; set; }
    }
    public class TaskDaVisualizzareInfo
    {
        public string Nome { get; set; }
        public int IdTask { get; set; }
        public int Anno { get; set; }
        public int Mese { get; set; }
        public string Tipo { get; set; }
        public DateTime PeriodoDa { get; set; }
        public DateTime PeriodoA { get; set; }
    }
    public class TaskSwitchModel
    {
        public int counter { get; set; }
        public bool stato { get; set; }
        public string statoeccezione { get; set; }
        public bool disabilitato { get; set; }
        public bool IsStornoCedolino { get; set; }
        public string mese { get; set; }
        public myRaiData.Incentivi.XR_MAT_RICHIESTE Richiesta { get; set; }
    }
    public class TaskStatusModel
    {
        public myRaiData.Incentivi.XR_MAT_TASK_IN_CORSO TaskInCorso { get; set; }
        public int counter { get; set; }
        public bool switch_disabilitato { get; set; }
        public bool IsStornoCedolino { get; set; }
        public TaskPronto taskPronto { get; set; }
    }
    public class ContenutoCampiPerTask
    {
        public ContenutoCampiPerTask()
        {
            Contenuti = new List<CampoContent>();
        }
        public int ID_task { get; set; }
        public List<CampoContent> Contenuti { get; set; }
    }

    public class TaskTracciatoExpModel
    {
        public ContenutoCampiPerMeseTask TracciatoEsploso { get; set; }
        public bool EditPermesso { get; set; }
        public int counter { get; set; }
    }

    public class TaskPronto
    {
        public myRaiData.Incentivi.XR_MAT_TASK_IN_CORSO TaskInCorso { get;  set; }
        public DateTime DataRiferimentoMeseAnno { get;  set; }
         
        public TaskTracciatoExpModel TracciatoEsplosoModel { get; set; }
        public string DicituraPeriodo { get; set; }
        public int IdAltraPraticaGiaSuDB {
            get;
            set;
        }
        public int NumeroFusioni { get; set; }
        public List<string> IntervalliFusi { get; set; }
        public DateTime? PeriodoDa { get; set; }
        public DateTime? PeriodoA { get; set; }
        public MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.Tracciato_richiamato[] TracciatiRew { get; internal set; }
         
    }
    public class DettaglioGestionePeriodiModel
    {
        public List<DettaglioGestionePeriodoItem> Periodi { get; set; }
        public myRaiData.Incentivi.XR_MAT_RICHIESTE Richiesta { get; set; }

    }
    public class DettaglioGestionePeriodoItem
    {
        public DateTime Data { get; set; }
        public string Eccezione { get; set; }
        public string CF { get; set; }
        public string INPS { get; set; }
        public string PeriodoInizio { get; set; }
        public string PeriodoFine { get; set; }
        public int id { get; set; }
    }
}
