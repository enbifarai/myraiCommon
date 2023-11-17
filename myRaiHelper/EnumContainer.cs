using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace myRaiHelper
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
        GetCategoriaDatoNetCachedL00,
        GetCategoriaDatoNetCachedL20,
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
        AbilitaApprovatoriL6,
        GiorniRiaperturaDopoSecondoGiornoC,
        PoliticheAbilitaGestLettera,
        EccezioniFrag,
        ModalitaTabellaApprovatoriProduzioneAttivo,
        SediGappTerritoriali,
        IntervalloRMTR_SE,
        SediContabiliRMTR_SE,
        SediSimulazioneAttivitaCeiton,
        PushAPIkey,
        ImportoRPAF,
        EsportazionePresenzeDipendenti,
        EsportazionePresenzeDipendentiDestinatario,
        EsportazionePresenzeDipendentiAvvioForzato,
        AbilitaSchermataFlat,
        GiorniBackMessaggiSegreteria,
        MatricoleIncarichiVisMod,
        MatricoleValutazioniVisMod,
        MailTemplateNotificaRifornimento,
        MailTemplateNotificaRifornimentoFromTo,
        MailTemplateConvalidaIBAN,
        MailModificaIBANFrom,
        SediBypassControlloFerie,
        SogliaModificaPF,
        AbilitatiIban,
        FlagTalentiaCezanne,
        MatricoleMonitoraggioFerie,
        AccountUtenteServizio_AD,
        DisabilitaControlloEccezioniPianoFerie,
        MatricoleSW,
        EccezioniCongedi,
        EccezioniSostitutive,
        AbilitatiCell,
        PrivacyAcademyModulo,
        TestoPrivacyAcademy,
        MessaggioNonAbilitatoGapp,
        MatricoleAblititateMaternitaCongedi,
        CodiciStraordinario,
        CodiciOrarioValoreZeroPerCongedi,
        Giorni_AF_fittizi,
        RegexNumeroProtocollo,
        RegexCF,
        RegexDataNascita,
        RegexPeriodo,
        RegexPeriodo2,
        RegexDataPresunta,
        CambioTurniNLRIP,
        CambioTurniMatricola,
        UfficiAbilitatiSmistamento,
        QueryArretratiHRDW,
        DataSogliaPartenzaFase3,
        SkipPrivacyCongedi,
        ParametriServiceWrapper,
        ModuloRinuncia2020Params,
        EccezioniLimitateOrario,
        QueryGiorniArretratiCongediHrdw,
        GruppoADceiton,
        GiorniTestCongedi,
        EccezioniCongediMezzaGiornata,
        StatoEccezioneCongedi,
        SogliaGiorniPerTracciatoDescrittiva,
        StatoEccezioneRipristinaCongedi,
        QueryStrHRDWCongedi,
        MatricoleAbilitateUploadCongedi,
        PaginazioneCongedi,
        SogliaConcludiMeseMT,
        SogliaRedditoCF,
        SediConObbligoSelezioneIdAttivita,
        AbilitatiDomicilio,
        SediNoVincoliPF,
        GdelleRetiMotivoObbligatorio,
        LimitiGiorniPrimaInizio,
        EccezioniConVerificaStorno,
        AbilitaFirmaRest,
        MaternitaCongediTE1,
        MaternitaCongediTE2,
        QueryDettaglioStrHRDW,
        PathFileEvidenze,
        CodiciRecordRew,
        MatricoleCancellazionePratiche,
        QueryHrdwHC_GM,
        IDscadenzarioInvioTracciatiDEW,
        MaxSWBatchLimit,
        EccezioniDaInserireConAzioneAutomatica,
        DematerializzazioneEccezioniPerContatore,
        QueryHrdwIsMazzini,
        NonAggiungereSe90,
        MatricoleAbilitateDifferenzeDescrittive,
        IDscadenzarioInvioDescrittive,
        GetLimitTrasferte,
        CampiStornoTracciato,
        GetLimitElementForFoglioSpese,
        QueryHRDWimportoLordo,
        PoliticheRetributive_DataStampataNelPDF,
        API_SW_CreaComunicazioni,
        API_SW_ModificaComunicazioni,
        API_SW_AnnullaComunicazioni,
        API_SW_RecediComunicazioni,
        API_SW_ClientID_Secret,
        API_SW_RicercaComunicazioni,
        API_SW_DettaglioComunicazioni,
        PoliticheRetributive_ProtocolloStampatoNelPDF,
        SecondiAttesaSimulazionePolRetr,
        FotoDipendenti,
        API_SW_UrlToken,
        PoliticheRetributive_ForzaFirmaVenturaNelPDF,
        PoliticheRetributive_FirmaBalzolaSeProdTV,
        AbilitatoImportazioneDatiSW,
        //UrlFotoApiController,
        UrlFotoApiControllerHRIS,

        ConsideraAreeDematerializzazione,
        SapTestUrl,
        SapTestMatricole,
        PoliticheRetributive_CodiceFirmaPDF,
        URLClickOnceDownloadDocumentoPersonale,
        HRDOC
    }

    public enum EnumWorkflows
    {
        SelfService = 1,
        Segreteria = 2,
        UfficioPersonale = 3

    }

    public enum EnumStatiRichiesta
    {
        InseritoUffPersonale = 5,
        InProgressUffPersonale = 6,
        InseritoSegreteria = 7,
        InProgressSegreteria = 8,
        InApprovazione = 10,
        Approvata = 20,
        Stampata = 30,
        Convalidata = 40,
        Rifiutata = 50,
        Cancellata = 60,
        Eliminata = 70
    }

    public enum EnumFormatoPeriodo
    {
        DaApprovare1,
        DaApprovare2,
        MieRichieste
    }

    public enum EnumCategoriaNotifica
    {
        [Description("RifiutoEccezione")]
        [AmbientValue("Rifiuto eccezione")]
        RifiutoEccezione,
        [Description("ApprovazioneEccezione")]
        [AmbientValue("Approvazione eccezione")]
        ApprovazioneEccezione,
        [Description("InsRichiesta")]
        [AmbientValue("Inserimento richiesta")]
        InsRichiesta,
        [Description("InsStorno")]
        [AmbientValue("Inserimento storno")]
        InsStorno,
        [Description("MarcaturaUrgente")]
        [AmbientValue("Marcatura urgente")]
        MarcaturaUrgente,
        [Description("MarcaturaScaduta")]
        [AmbientValue("Marcatura scaduta")]
        MarcaturaScaduta
    }

    public enum EnumTipoEventoNotifica
    {
        APPR,
        RIF,
        SCAD,
        CH,
        INSR,
        INSS,
        URG,
        ST_RESO,
        ST_RICH,
        ST_FIRM
    }

    public enum EnumTipoDestinatarioNotifiche
    {
        ESS_DIP,
        ESS_LIV1,
        ESS_LIV2
    }

    public enum EnumTipoInvioNotifiche
    {
        I,
        G,
        S,
        N
    }

    public enum Quadratura
    {
        Giornaliera,
        Settimanale
    }

    public enum EnumPosizioniCampo
    {
        Prima_del_campo_DEFINIZIONE,
        Prima_del_campo_CRITERI_DI_INSERIMENTO,
        Prima_del_campo_TRATTAMENTO_ECONOMICO,
        Prima_del_campo_DOCUMENTAZIONE,
        Prima_del_campo_PRESUPPOSTI_E_PROCEDURE,
        Prima_del_campo_NOTE,
        Prima_del_campo_ALLEGATI,
        Prima_del_campo_FONTI_DELLA_DISCIPLINA,
        Prima_del_campo_ULTERIORI_INFORMAZIONI,
    }

    public enum PolRetrChiaveEnum
    {
        Richieste,
        BudgetQIO,
        BudgetRS
    }
    public enum ProvvStatoEnum
    {
        InCarico = 0,
        Richiesta = 1,
        Convalidato = 2,
        Consegnato = 3,
        Rifiutato = 4,
        CedoliniElaborati = 5,
        Conclusa = 6
    }
    public enum ProvvedimentiEnum
    {
        [Display(Name = "Aumento di livello")]
        [DescriptionAttribute("Promozione")]
        AumentoLivello = 1,
        [Display(Name = "Aumento di merito")]
        [DescriptionAttribute("Aumento di merito")]
        AumentoMerito = 2,
        [Display(Name = "Gratifica")]
        [DescriptionAttribute("Gratifica")]
        Gratifica = 3,
        [Display(Name = "Aumento di livello senza assorbimento")]
        [DescriptionAttribute("Promozione senza assorbimento")]
        AumentoLivelloNoAssorbimento = 4,
        [Display(Name = "Nessuno")]
        [DescriptionAttribute("Nessuno")]
        Nessuno = 5,
        [Display(Name = "Aumento di livello")]
        [DescriptionAttribute("Promozione")]
        CUSAumentoLivello = 6,
        [Display(Name = "Aumento di livello senza assorbimento")]
        [DescriptionAttribute("Promozione senza assorbimento")]
        CUSAumentoLivelloNoAssorbimento = 7,
        [Display(Name = "Aumento di merito")]
        [DescriptionAttribute("Aumento di merito")]
        CUSAumentoMerito = 8,
        [Display(Name = "Gratifica")]
        [DescriptionAttribute("Gratifica")]
        CUSGratifica = 9,
        [Display(Name = "Nessuno")]
        [DescriptionAttribute("Nessuno")]
        CUSNessuno = 10
    }

    public enum SessionVariables
    {
        AccountUtenteServizioUserName,
        AccountUtenteServizioUserPassword,
        FlagEvidenze,
        PianoFerie,
        UtenteAnagrafica,
        DatePerEvidenze,
        Utente,
        FotoUtente,
        UtenteListaProfili,
        AnnoFeriePermessi,
        ListaEvidenzeScrivania,
        DetassazioneListaDipendenti,
        GetPianoFerieWrapped,
        GetContatoriEccezioni
    }

    public static class EnumStatiPDF
    {
        public static string Stampato { get { return "S_OK"; } }
        public static string Convalidato { get { return "C_OK"; } }
    }

    public enum ApplicationType
    {
        [Description("RaiPerMe")]
        [AmbientValue("Rai per Me")]
        RaiPerMe = 0,
        [Description("HRIS")]
        [AmbientValue("HRIS")]
        Gestionale = 1
    }

    public enum ApprovazioneMassivaStatusEnum
    {
        Waiting = 1,
        InProgress = 2,
        Executed = 3,
        Failed = 4,
        Cancellation = 5
    }

    public enum EnumPresenzaDip
    {
        Presente,
        Assente,
        SmartWorking
    }
}
