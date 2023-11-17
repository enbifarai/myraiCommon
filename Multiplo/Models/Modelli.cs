using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Multiplo.Models
{
    public enum EnumParametriSistema
    {
        MaxRowsVisualizzabiliDaApprovare,
        AccountUtenteServizio,
        MatricolaSimulata,
        MailApprovazioneSubject,
        MailApprovazioneFrom,
        MailApprovazioneTemplate,
        MailRifiutaTemplate,
        UrlImmagineDipendente,
        OrariGapp,
        ValidazioneGenericaEccezioni,
        RowsPerScroll,
        LimiteMesiBackPerEvidenze,
        MessaggioAssenteIngiustificato,
        PosizioneNumDoc,
        TipiDipQuadraturaSettimanale,
        TipiDipQuadraturaGiornaliera,
        GestisciDateSuDocumenti,
        OreRichiesteUrgenti,
        SovrascritturaTipoDipendente,
        CodiceCSharp,
        IgnoraAssenzeIngiustificatePerMatricole,
        GiornoEsecuzioneBatchPDF,
        IntervalloBatchSecondi,
        LivelloUtenteListaEccezioni,
        AutorizzaMinimale,
        SoppressePOH,
        ApprovaPresenzeDopoGG,
        POHperMese,
        MailTemplateOmaggi,
        AppKeyhrce,
        NotificheRangeOre,
        MailTemplateAvvisoDelega,
        MailTemplateRevocaDelega,
        MailTemplateOmaggiUte,
        MinutiPrenotazione,
        MatricoleAdmin,
        ColoriCharts,
        MailNotificaQuestionario,
        ServizioTema,
        ServizioTemaMapping,
        MailAbbonamentiFrom,
        MailTemplateAbbonamentiRoma,
        MailTemplateAbbonamentiRomaCanc,
        MailTemplateAbbonamentiTorino,
        MailTemplateAbbonamentiTorinoCanc,
        BrowserAmmessi,
        MessaggioBadBrowser,
        ShareAllegati,
        TestoPrivacy,
        ForzaFiguraProfessionale,
        RedirectEmailSuSviluppo,
        RangeMensa,
        MatricoleTester,
        EtichettaFirmaSuPDF,
        TestoPolicyAbbonamenti,
        TestoPolicyAbbonamentiRoma,
        TestoPolicyAbbonamentiTorino,
        ToolbarSummernote,
        NumeriMassimoAllegati,
        DimensioneMassimaAllegati,
        AccettaSoloDurata,
        IndirizzoComnitNotFound,
        IndirizzoComnit3,
        IndirizzoComnit4,
        IndirizzoComnit5,
        IndirizzoComnit6,
        IndirizzoComnit7,
        CorsoOnline,
        TipiContrattoCorsiOlnlie,
        SocietaCorsiOnline,
        UsaServizioPerProfiloPersonale,
        MailEventiFrom,
        MailEventiBody,
        MailEventiBodyDel,
        MailCCRoma,
        MailCCTorino,
        MatricoleAutorizzatePDF,
        MessaggioChiusura,
        EccezioniNoQuantita,
        ForzaAbilitazioneGapp,
        EccezioniIniettate,
        EccezioniOneClick,
        MailDebug,
        ServiziChiSono,
        EccezioniDigitaSoloQuantita,
        UrlNotificheBustaPagaDocumenti,
        EccezioneFittiziaSpostamento,
        EccezioneFittiziaIgnora,
        MailIscrizioneCorsoFrom,
        MailIscrizioneCorsoBody,
        CVEditorialiSezContabiliAbilitate,
        AbilitatiSimulazione,
        StopMail,
        TestoPrivacyGenerale,
        MaxDatePolicy,
        NotificaCV100,
        NotificaCVLess100,
        NotificaCVZero,
        GetCategoriaDatoNetCached,
        GetCategoriaDatoNetCachedL2,
        GetCategoriaDatoNetCachedL3,
        GetCategoriaDatoNetCachedL4,
        GetCategoriaDatoNetCachedL5,
        GetCategoriaDatoNetCachedNolevel,
        ServizioCeitonGetAttivita,
        AcademyTematicheAggiuntive,
        AcademyUrlIlias,
        MailMappaturaPrenotazione,
        MailMappaturaCancellazione,
        AbbonamentiMatricoleCancellazione,
        PathImmaginiFittizie,
        EccezioniMotivoObbligatorioSeServizioEsterno,
        EccezioniMotivoObbligatorioDescrizione,
        UtentePerConvalida,
        CeitonPosizionamentoEccezioni,
        AcademyCatalogoPDF,
        NewsTemplate,
        NewsTitle,
        SediEffettivamenteGestiteSirio,
        AcademyModalScorm,
        MessaggioHome,
        ProponiAttivitaSeAttivitaNelGiornoAnchePerAltreEccezioni,
        DateConControlloSNM_SNP,
        AccountUtenteServizioIlias,
        PianoFeriePercentualeSpettanzaArretrati,
        PianoFeriePercentualeSuTotaleEntro,
        EccezioniNoOrarioRosso,
        PoliticheEccezioniAssenze,
        MinimoMetaTurnoSeMR_MF_MN,
        PoliticheAbilitaGestVis,
        MatricoleVisibilitaPianoFerie,
        OrariGiornateNoPianoFerie,
        PolRetrCostoAnnuo,
        ProposteAutoRicalcoloQuantita,
        ProposteAutoIniettateRicalcolo,
        SpegniResoconti,
        SpegniPresenze,
        FirmaMatricoleParticolari,
        ChiusuraMese,
        IntervalloMinTech2,
        AttivaListaEvidenze3VA,
        BoxDetassazione,
        BoxDetassazioneMessaggi,
        MessaggioAlberghi,
        ImportoRKMF,
        ImportoRVIV,
        DetaxLimit,
        AssistenteTipiDip,
        AssistenteMatricole,
        AssistenteEccezioni,
        DetaxAbilitatiRipristinoDati,
        AbilitaWizardFS,
        EccezioniDaRimuovereTinMNS,
        AbilitaTaglioMaggCoda,
        AbilitaSceltaApprovatore,
        BypassOrariGappMatricole,
        InsediamentiFittizi,
        EccezioniEscluseL4,
        AccountBatchAbilitato,
        BypassApprovazioni,
        AbilitaApprovatoriL4L5,
        GiorniRiaperturaDopoSecondoGiornoC,
        PoliticheAbilitaGestLettera,
        EccezioniFrag
    }

    public class WeekPlan
    {
        public WeekPlan ( )
        {
            Days = new List<DayPlan>( );
        }
        public string Matricola { get; set; }
        public List<DayPlan> Days { get; set; }
        public string Cod_Eccezione { get; set; }
    }

    public class DayPlan
    {
        public DayPlan ( )
        {
            Activities = new List<DayActivity>( );
        }
        public DateTime Date { get; set; }
        public List<DayActivity> Activities { get; set; }

        public override string ToString ( )
        {
            return String.Format( "{0:dd/MM/yyyy} - {1} attività" , Date , Activities.Count );
        }
    }

    public class DayActivity
    {
        public string Location { get; set; }
        public string Title { get; set; }
        public string Schedule { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string MainActivity { get; set; }
        public string SecActivity { get; set; }
        public string DoneActivity { get; set; }
        public string Manager { get; set; }
        public string Matricola { get; set; }
        public DateTime Date { get; set; }
        public void SetStartTime ( string startTimeStr )
        {
            TimeSpan temp;
            TimeSpan.TryParseExact( startTimeStr , "g" , null , out temp );
            StartTime = temp;
        }
        public void SetEndTime ( string endTimeStr )
        {
            TimeSpan temp;
            TimeSpan.TryParseExact( endTimeStr , "g" , null , out temp );
            EndTime = temp;
        }

        public string idAttivita { get; set; }
        public string Note { get; set; }
        public string Uorg { get; set; }

        public int Eccezioni { get; set; }
    }

    public class ArretratiExcel2019
    {
        public string Matricola { get; set; }
        public string Nominativo { get; set; }
        public string RapportoLavoro { get; set; }
        public string ServizioContabile { get; set; }
        public string Categoria { get; set; }
        public int DaFare { get; set; }
        public int Fruite { get; set; }
    }

    public class ArretratiExcel2019Ext : ArretratiExcel2019
    {
        public string SedeGapp { get; set; }
        public int Richiste { get; set; }
        public float EffettivamenteDaFare { get; set; }
        public float RRArretrati { get; set; }
        public float RFArretrati { get; set; }
        public float FEArretrati { get; set; }
        public float FECE_MRCE_MNCE_Gapp { get; set; }
        public bool GiaApprovato { get; set; }
        public float FerieRimanenti { get; set; }
        public float FerieMinime { get; set; }
    }



    public class GetAttivitaResponse
    {
        public string IdAttivita { get; set; }
        public string Nome { get; set; }
        public DateTime Data { get; set; }
    }
}
