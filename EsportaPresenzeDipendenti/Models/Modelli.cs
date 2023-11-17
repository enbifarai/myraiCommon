using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;

namespace EsportaPresenzeDipendenti.Models
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

    /// <summary>
    /// CodiceInsediamento ; Descrizione ; Matricola ; PresenteOra ; DescrizioneAssenza ; AlmenoUnaTimbratura ; Nominativo
    /// </summary>
    public class PerExport : DatiAggiuntivi
    {
        [Required]
        [DisplayAttribute(Name = "Cod. Insediamento Timbratura")]
        public string CodiceInsediamento { get; set; }

        [Required]
        [DisplayAttribute(Name = "Insediamento Timbratura breve")]
        public string DescrizioneInsediamento { get; set; }

        [Required]
        [DisplayAttribute(Name = "Insediamento Timbratura")]
        public string DescrizioneInsediamentoEsteso { get; set; }

        [Required]
        [DisplayAttribute(Name = "Indirizzo Insediamento Timbratura")]
        public string Insediamento { get; set; }

        [Required]
        [DisplayAttribute(Name = "Data Timbratura")]
        public DateTime DataTimbratura { get; set; }

        [Required]
        [DisplayAttribute(Name = "Cod. Insediamento Inquadramento")]
        public string CodiceInsediamentoUbicazione { get; set; }

        [Required]
        [DisplayAttribute(Name = "Descrizione Insediamento Inquadramento")]
        public string DescrizioneInsediamentoUbicazione { get; set; }

        [Required]
        [DisplayAttribute(Name = "Matricola")]
        public string Matricola { get; set; }

        [Required]
        [DisplayAttribute(Name = "Nominativo")]
        public string Nominativo { get; set; }

        [Required]
        [DisplayAttribute(Name = "Presente Ora")]
        public string PresenteOra { get; set; }

        [Required]
        [DisplayAttribute(Name = "Cod. Assenza")]
        public string CodiceAssenza { get; set; }

        [Required]
        [DisplayAttribute(Name = "Descrizione Assenza")]
        public string DescrizioneAssenza { get; set; }

        [Required]
        [DisplayAttribute(Name = "Almeno Una Timbratura")]
        public string AlmenoUnaTimbratura { get; set; }

        public string IndirizzoInsediamento { get; set; }

        public bool Assente { get; set; }
        public string Info { get; set; }
        public string OrarioPrimaEntrata { get; set; }
        public string OrarioUltimaEntrata { get; set; }
        public string OrarioUltimaUscita { get; set; }
        public string SedePrevalente { get; set; }
    }

    public class DatiAggiuntivi
    {
        [Required]
        [DisplayAttribute(Name = "Societa'")]
        public string societa { get; set; }

        [Required]
        [DisplayAttribute(Name = "Servizio Contabile")]
        public string desc_serv_cont { get; set; }

        [Required]
        [DisplayAttribute(Name = "CCL")]
        public string CCL { get; set; }

        [Required]
        [DisplayAttribute(Name = "Cod. Macro Categoria")]
        public string cod_macro_categoria { get; set; }

        [Required]
        [DisplayAttribute(Name = "Macro Categoria")]
        public string desc_macro_categoria { get; set; }

        [Required]
        [DisplayAttribute(Name = "Cod. Categoria")]
        public string cod_categoria { get; set; }

        [Required]
        [DisplayAttribute(Name = "Categoria descrizione")]
        public string desc_categoria { get; set; }

        [Required]
        [DisplayAttribute(Name = "Cod. Profilo Professionale")]
        public string cod_liv_professionale { get; set; }

        [Required]
        [DisplayAttribute(Name = "Profilo Professionale")]
        public string desc_liv_professionale { get; set; }

        [Required]
        [DisplayAttribute(Name = "Cod. Aggregato Sede Contabile")]
        public string COD_SEDE_AGG { get; set; }

        [Required]
        [DisplayAttribute(Name = "Aggregato Sede Contabile")]
        public string DESC_AGGREGATO_SEDE { get; set; }

        [Required]
        [DisplayAttribute(Name = "Cod.Sede Contabile")]
        public string cod_sede { get; set; }

        [Required]
        [DisplayAttribute(Name = "Sede Contabile")]
        public string desc_sede { get; set; }

        [Required]
        [DisplayAttribute(Name = "Cod. Tipo Rapporto di Lavoro")]
        public string cod_rapp_lav { get; set; }

        [Required]
        [DisplayAttribute(Name = "Tipo Rapporto di Lavoro")]
        public string desc_rapp_lav { get; set; }

        [Required]
        [DisplayAttribute(Name = "Tipologia Dipendente")]
        public string CodiceRapportoDiLavoro { get; set; }

        public string cod_insediamento_ubicazione { get; set; }

        public string matricola_dp { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Insediamento
    {
        public string cod_insediamento { get; set; }
        public string desc_insediamento { get; set; }
        public string Indirizzo { get; set; }
        public string Sede { get; set; }
    }

    public class FileWriter
    {
        public string Filepath { get; set; }

        private static object locker = new Object();

        public void WriteToFile(StringBuilder text)
        {
            lock (locker)
            {
                using (FileStream file = new FileStream(Filepath, FileMode.Append, FileAccess.Write, FileShare.Read))
                using (StreamWriter writer = new StreamWriter(file, Encoding.Unicode))
                {
                    writer.Write(text.ToString());
                }
            }

        }
    }
}
