using myRaiData;
using myRaiData.Incentivi;
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

        public List<XR_INC_OPERSTATI_DOC> Allegati { get; set; }
        public List<XR_INC_OPERSTATI_DOC> AllegatiCont { get; set; }
        public List<XR_INC_OPERSTATI_DOC> AllegatiProp { get; set; }
        public List<XR_INC_OPERSTATI_DOC> AllegatiAccett { get; set; }
        public List<XR_INC_OPERSTATI_DOC> AllegatiAnpal { get; set; }
        public bool SuperUser { get; set; }
        public string Nominativo { get; set; }
        public int NumNote { get; set; }
        public SINTESI1 Sintesi { get; set; }
        public QUALIFICA Qualifica { get; set; }
        public TB_QUALSTD QualStd { get; set; }
        public ANAGPERS ANAGPERS { get; set; }
        public TB_CITTAD CITTAD { get; set; }
        public bool IsAnpal { get; set; }
        public CessazioneDati HrdwData { get; set; }
        public TB_LIVSTUD Studi { get; set; }
        public bool CanAnpal { get; set; }

        public bool IndItl { get; set; }
        public List<XR_INC_OPERSTATI_DOC> AllegatiApp { get; set; }
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


    public class CessazioneFilter
    {
        public CessazioneFilter()
        {
            SoloScrittura = true;
        }
        public bool HasFilter { get; set; }

        public string Matricola { get; set; }
        public string Nominativo { get; set; }
        public int Stato { get; set; }
        public string InCarico { get; set; }
        public string DataCessazione { get; set; }
        public string Sede { get; set; }
        public string Causa { get; set; }
        public bool SoloScrittura { get; set; }
    }

    public class CessazioneBozzaSample
    {
        public string Gruppo { get; set; }
        public string Descrizione { get; set; }
        public string Sede { get; set; }
        public string Template { get; set; }
        public string Qualifica { get; set; }
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
        public CessazioneList():base()
        {

        }
        public string Parent { get; set; }
    }
}
