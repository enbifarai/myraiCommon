using myRaiData;
using myRaiData.Incentivi;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace myRaiCommonModel.Gestionale.Cessazione
{
    public class GestioneStatiClass
    {
        public int IdPersona;
        public int CurrentState;
        public int IdCurrentState;
        public bool PraticaInLavorazione;
        public bool MiaPratica;
        public bool InServizio;
        public int percCompl;

        public bool praticaChiusa;

        public bool showPrendiInCarico;
        public bool showAvviaPratica;

        public bool showTab;

        public string classTabCont;
        public string classTabApp;
        public string classTabVerbFirm;
        public string classTabVerbUpload;
        public string classTabPag;

        public bool showTabPanelCont;
        public bool showTabPanelApp;
        public bool showTabPanelVerbFirm;
        public bool showTabPanelVerbUpload;
        public bool showTabPanelPag;
    }
}


namespace myRaiCommonModel.Gestionale
{
    public class CessazioneModel
    {
        public CessazioneModel()
        {
            Scadenze = new CessazioneScadenze();
        }
        public bool IsPreview { get; set; }
        public XR_WKF_TIPOLOGIA Tipologia { get; set; }
        public List<XR_WKF_WORKFLOW> Workflow { get; set; }

        public XR_INC_STATI Stato { get; set; }
        /// <summary>
        /// Contiene le info del dipendente che ha in carico la pratica
        /// </summary>
        public SINTESI1 InCarico { get; set; }


        public XR_INC_DIPENDENTI Pratica { get; set; }
        public XR_INC_OPERSTATI OperStato { get; set; }
        public bool AbilitaPresaInCarico { get; set; }
        public bool AbilitaAvviaPratica { get; set; }

        public string Scelta { get; set; }

        public XR_WKF_WORKFLOW Ordine { get; set; }
        public CessazioneHRDWData DatiQuota100 { get; set; }
        public bool TempoInd_19780520 { get; set; }

        public Dictionary<int, string> PratichePrecedenti { get; set; }
        public bool HasDocPraticaPrecedente { get; set; }
        public List<XR_INC_OPERSTATI_DOC> Allegati { get; set; }
        public List<XR_INC_OPERSTATI_DOC> AllegatiCont { get; set; }
        public List<XR_INC_OPERSTATI_DOC> AllegatiProp { get; set; }
        public List<XR_INC_OPERSTATI_DOC> AllegatiAccett { get; set; }
        public List<XR_INC_OPERSTATI_DOC> AllegatiAnpal { get; set; }
        public Dictionary<int, List<XR_INC_OPERSTATI_DOC>> DictAllegati { get; set; }
        public bool SuperUser { get; set; }
        public string Nominativo { get; set; }
        public int NumNote { get; set; }
        public SINTESI1 Sintesi { get; set; }
        public QUALIFICA Qualifica { get; set; }
        public TB_QUALSTD QualStd { get; set; }
        public ANAGPERS ANAGPERS { get; set; }
        public TB_CITTAD CITTAD { get; set; }
        public bool IsAnpal { get; set; }
        public string OpenFunction { get; set; }
        public CessazioneDati HrdwData { get; set; }
        public TB_LIVSTUD Studi { get; set; }
        public bool CanAnpal { get; set; }
        public List<string> AbilFunctions { get; set; }

        public bool IndItl { get; set; }
        public List<XR_INC_OPERSTATI_DOC> AllegatiApp { get; set; }

        public DateTime? NotificaSollecito { get; set; }
        public bool PercipientePensioneInvalidita { get; set; }
        public string[] TipiVertenze { get; set; }

        //ExtraField - Tessere/Matcon/Qual
        public bool NotHasTessereContr { get; set; }
        public bool NotHasMaternita { get; set; }
        public bool NotHasCambioQualifica { get; set; }

        //ExtraField - Veicolo (per A01)
        public string VeicoloTipologia { get; set; }
        public string VeicoloTarga { get; set; }

        //ExtraField - Accettazione
        public DateTime? AccettazioneGenerata { get; set; }

        public CessazioneScadenze Scadenze { get; set; }
        public bool HasFields { get; set; }
        public IEnumerable<XR_INC_DIPENDENTI_FIELD> Fields { get; set; }
        public bool HasRuoloAccettazione { get; set; }
        public string RuoloAccettazione { get; set; }
        public bool TfrAnte2007FPGCI { get; set; }
        public bool CanCertificatoServizio { get; set; }
    }

    public class CessazioneScadenze
    {
        //ExtraField - Scadenze
        public DateTime? LimiteConsegnaEstratti { get; set; }
        public DateTime? LimiteRichiestaIntegrazione { get; set; }
        public DateTime? LimiteProposta { get; set; }
        public DateTime? LimiteFirmaAccettazioneAzienda { get; set; }
        public DateTime? LimiteRecessoOnline { get; set; }

        public bool HasExpiredAlert { get; set; }
        public string ExpiredAlert { get; set; }

        public bool HasExpireSoonAlert { get; set; }
        public string ExpireSoonAlert { get; set; }

        public DateTime? DataScadenza { get; set; }

        public void Load(List<XR_HRIS_PARAM> parametriIncentivi, XR_INC_DIPENDENTI inc, XR_INC_STATI stato, ICollection<XR_INC_DIPENDENTI_FIELD> fields)
        {
            if (inc.ID_TIPOLOGIA == 1)
            {
                if (inc.DTA_RICHIESTA.HasValue)
                {
                    XR_HRIS_PARAM paramLimite = null;
                    int limite = 0;
                    int currentStato = stato.ID_STATO;

                    if (currentStato == (int)IncStato.RichiestaInserita)
                    {
                        paramLimite = parametriIncentivi.FirstOrDefault(x => x.COD_PARAM == "LimiteConsegnaEstratti");
                        limite = Convert.ToInt32(paramLimite.COD_VALUE1);
                        LimiteConsegnaEstratti = XR_INC_DIPENDENTI.GetField(fields, "LimiteConsegnaEstratti", inc.DTA_RICHIESTA.Value.AddDays(limite));
                        if (!inc.DATA_ARRIVO_DOC.HasValue)
                            CheckExpireDate(LimiteConsegnaEstratti.Value, paramLimite.COD_VALUE2, paramLimite.COD_VALUE3);
                        else if (inc.DATA_RICH_INT.HasValue)
                        {
                            paramLimite = parametriIncentivi.FirstOrDefault(x => x.COD_PARAM == "LimiteRichiestaIntegrazione");
                            limite = Convert.ToInt32(paramLimite.COD_VALUE1);
                            LimiteRichiestaIntegrazione = XR_INC_DIPENDENTI.GetField(fields, "LimiteRichiestaIntegrazione", inc.DATA_RICH_INT.Value.AddDays(limite));
                            if (!inc.DATA_ARRIVO_INT.HasValue)
                                CheckExpireDate(LimiteRichiestaIntegrazione.Value, paramLimite.COD_VALUE2, paramLimite.COD_VALUE3);
                        }
                    }
                    else if (currentStato == (int)IncStato.RichiestaAccettata)
                    {
                        if (inc.DATA_INVIO_PROP.HasValue)
                        {
                            paramLimite = parametriIncentivi.FirstOrDefault(x => x.COD_PARAM == "LimiteProposta");
                            limite = Convert.ToInt32(paramLimite.COD_VALUE1);
                            LimiteProposta = XR_INC_DIPENDENTI.GetField(fields, "LimiteProposta", inc.DATA_INVIO_PROP.Value.AddDays(limite));

                            if (!inc.DATA_FIRMA_DIP.HasValue)
                                CheckExpireDate(LimiteProposta.Value, paramLimite.COD_VALUE2, paramLimite.COD_VALUE3);
                            else if (inc.DTA_ACCETT_AZ.HasValue)
                            {
                                paramLimite = parametriIncentivi.FirstOrDefault(x => x.COD_PARAM == "LimiteFirmaAccettazioneAzienda");
                                limite = Convert.ToInt32(paramLimite.COD_VALUE1);
                                LimiteFirmaAccettazioneAzienda = XR_INC_DIPENDENTI.GetField(fields, "LimiteFirmaAccettazioneAzienda", inc.DTA_ACCETT_AZ.Value.AddDays(limite));

                                paramLimite = parametriIncentivi.FirstOrDefault(x => x.COD_PARAM == "LimiteRecessoOnline");
                                limite = Convert.ToInt32(paramLimite.COD_VALUE1);
                                LimiteRecessoOnline = XR_INC_DIPENDENTI.GetField(fields, "LimiteRecessoOnline", inc.DTA_ACCETT_AZ.Value.AddDays(limite));

                                if (!inc.DTA_RECESSO.HasValue)
                                    CheckExpireDate(LimiteRecessoOnline.Value, paramLimite.COD_VALUE2, paramLimite.COD_VALUE3);
                            }
                        }
                    }
                }
            }
        }

        private void CheckExpireDate(DateTime dataScadenza, string expireSoonAlert, string expiredAlert)
        {
            DataScadenza = dataScadenza;
            if (dataScadenza < DateTime.Today)
            {
                HasExpiredAlert = true;
                ExpiredAlert = String.Format(expiredAlert, dataScadenza);
            }
            else
            {
                HasExpireSoonAlert = true;
                ExpireSoonAlert = String.Format(expireSoonAlert, dataScadenza);
            }
        }
    }

    public class IncModuloMaternita
    {
        public int IdPratica { get; set; }
        public string CodGruppo { get; set; }
        public string[] TipoAssenza { get; set; }
        public List<IncFiglio> Figli { get; set; }
    }
    public class IncFiglio
    {
        public string Nominativo { get; set; }
        public string Cognome { get; set; }
        public string Nome { get; set; }
        public string CF { get; set; }
        public DateTime? NascitaData { get; set; }
        public List<int> Anni { get; set; }
    }

    public class CessazioneFilter
    {
        public CessazioneFilter()
        {
            SoloScrittura = true;
        }
        public bool HasFilter { get; set; }

        public string Matricola { get; set; }
        public string Nominativo { get; set; }
        public string Stato { get; set; }
        public string InCarico { get; set; }
        public string DataCessazione { get; set; }
        public string Sede { get; set; }
        public string Causa { get; set; }
        public bool SoloScrittura { get; set; }
        public string Tipologia { get; set; }
        public string CodiceGruppo { get; set; }
        public string DataRichiesta { get; set; }
        public string[] QualFilter { get; set; }
        public string[] TipoVertenza { get; set; }
    }

    public class CessazioneDati
    {
        public string matricola_dp { get; set; }
        public string voce_te { get; set; }
        public DateTime? data_cessazione { get; set; }
        public string cod_mansione { get; set; }
        public string desc_mansione { get; set; }
        public string istituto_assicuratore_mansione { get; set; }
        public string tipo_minimo { get; set; }
        public string cod_serv_cont { get; set; }
        public string desc_serv_cont { get; set; }
        public string cod_macro_categoria { get; set; }
        public string desc_macro_categoria { get; set; }
        public string cod_categoria { get; set; }
        public string desc_categoria { get; set; }
        public string desc_livello { get; set; }
        public string desc_liv_professionale { get; set; }
        public string desc_aggregato_sede { get; set; }
        public string desc_sede { get; set; }
        public string ccl { get; set; }
        public string societa { get; set; }
        public string cod_insediamento { get; set; }
        public string desc_insediamento { get; set; }
        public decimal tot_retrib_annua { get; set; }
    }

    public enum CessTipoBozza
    {
        Word,
        Html
    }
    public class CessazioneBozza
    {
        public bool AdminMode { get; set; }
        public string Codice { get; set; }
        public string Tipo { get; set; }
        public int IdDipendente { get; set; }
        public string HtmlText { get; set; }
        public string HtmlTextLastMod { get; set; }

        public bool HasPDFTemplate { get; set; }
        public string TemplateLastMod { get; set; }

        public int IdBozza { get; set; }
        public string InfoInvio { get; set; }
        public bool IsViewMode { get; set; }
        public string IndirizziCC { get; set; }
        public string TemplateBozza { get; set; }
        public string TipologiaBozza { get; set; }

        public bool HasCronologia { get; set; }
        /// <summary>
        /// Per la TipologiaBozza="verbale", bisogna gestire il meccanismo di sblocco per le pratiche delle sedi
        /// </summary>
        public bool AbilitaInvio { get; set; }
        public bool AbilitaGestione { get; set; }
        public string TipoVertenze { get; set; }
    }

    public class CessazioneCosti
    {
        public CessazioneCosti()
        {
            Saving = new List<CessazioneSaving>();
        }
        public decimal MaxValue { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal Percentage
        {
            get
            {
                return CurrentValue > 0 && MaxValue > 0 ? CurrentValue / MaxValue * 100 : 0;
            }
        }
        public List<CessazioneSaving> Saving { get; set; }
    }
    public class CessazioneSaving
    {
        public string matricola_dp { get; set; }
        public string ccl { get; set; }
        public string MacroCategoria { get; set; }
        public decimal UltimaRal { get; set; }
        public DateTime DataCessazioneConcordata { get; set; }
        public DateTime DataLimiteEta { get; set; }
        public decimal Coefficiente { get; set; }
        public int Anno { get; set; }
        public decimal? Risparmio { get; set; }
    }

    public class CessazioneAllContainer
    {
        public int Stato { get; set; }
        public List<XR_INC_OPERSTATI_DOC> Allegati { get; set; }
        public bool EnabledAdd { get; set; }
        public int IdDipendente { get; set; }
    }

    public class CessazioneHRDWData
    {
        public string matricola_dp { get; set; }
        public DateTime data_prima_assunzione_subordinato { get; set; }
        public DateTime data_nascita { get; set; }
        public DateTime data_compimento_62_anni_eta { get; set; }
        public DateTime data_38_anni_anzianita_servizio { get; set; }
    }

    public class CessazioneHRDWSaving
    {
        public string matricola_dp { get; set; }
        public string ccl { get; set; }
        public string MacroCategoria { get; set; }
        public decimal UltimaRal { get; set; }
        public DateTime DataCessazioneConcordata { get; set; }
        public DateTime DataLimiteEta { get; set; }
        public decimal Coefficiente { get; set; }
        public int Anno { get; set; }
        public decimal? Risparmio { get; set; }
    }

    public class AggiuntaDipendente
    {
        public int IdPersona { get; set; }
        public string Matricola { get; set; }
        public string Cognome { get; set; }
        public string Nome { get; set; }
        public string Sede { get; set; }
        public DateTime DataAssunzione { get; set; }
        public DateTime DataAnzianita { get; set; }
        public DateTime DataCessazione { get; set; }
        public string Parttime { get; set; }
        public string CauseVertenze { get; set; }
        public decimal UnaTantum { get; set; }
        public decimal IncentivoLordo { get; set; }
        public DateTime? DataPagamento { get; set; }
        public string NumeroTelefono { get; set; }
        public string Nota { get; set; }
        public int Tipologia { get; set; }
    }

    public class CessazioneList : List<CessazioneModel>
    {
        public CessazioneList() : base()
        {

        }
        public string Parent { get; set; }
        public string IncExtra { get; set; }
    }
}
