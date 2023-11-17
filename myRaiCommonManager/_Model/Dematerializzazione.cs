using myRaiData.Incentivi;
using myRaiHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml.Serialization;

namespace myRaiCommonModel
{
    public class DematerializzazioneModel
    {
        public sidebarModel menuSidebar { get; set; }
    }

    public class Dematerializzazione_ElencoDocumenti
    {
        public Dematerializzazione_ElencoDocumenti()
        {
            this.MieiDocumenti = new List<XR_DEM_DOCUMENTI_EXT>();
            this.AnnoCorrente = true;
            this.ElementiNonLetti = 0;
            this.Filtro = new List<Dematerializzazione_FiltroTipologia>();
            UlterioriAnni = false;
        }

        public List<XR_DEM_DOCUMENTI_EXT> MieiDocumenti { get; set; }
        public bool AnnoCorrente { get; set; }

        public List<Dematerializzazione_FiltroTipologia> Filtro { get; set; }

        public int ElementiNonLetti { get; set; }

        public bool UlterioriAnni { get; set; }

        public string FiltroDescrizioneTipologia { get; set; }
        public int FiltroIdTipologia { get; set; }

        public bool FiltroSoloDaLeggere { get; set; }
    }

    public class Dematerializzazione_FiltroTipologia
    {
        public int IdTipologiaDocumento { get; set; }
        public string Tipologia { get; set; }
        public int NumeroElementi { get; set; }
        public DateTime? Data { get; set; }
    }

    public class Dematerializzazione_FiltroFirmaVM
    {
        public int IdTipologiaDocumento { get; set; }
        public string Tipologia { get; set; }
        public string NominativoOMatricola { get; set; }
        public DateTime? Data { get; set; }
        public int StatoRichiesta { get; set; }
    }

    public class Dematerializzazione
    {
        public int Id { get; set; }
        public string Descrizione { get; set; }
        public int IdTipoDocumento { get; set; }
        public string TipoDocumento { get; set; }
        public DateTime DataCreazione { get; set; }
        public string MatricolaCreatore { get; set; }
        public string MatricolaDestinatario { get; set; }
        public string MatricolaApprovatore { get; set; }
        public string MatricolaResponsabileFirma { get; set; }
        public DateTime? DataApprovazione { get; set; }
        public DateTime? DataFirma { get; set; }
        public string NumeroProtocollo { get; set; }
        public string CodiceProtocollatore { get; set; }
        public DateTime? DataInvioNotifica { get; set; }
        public DateTime Timestamp { get; set; }
    }

    /*
     * 
     * 10	Richiesta creata
        20	Documento pronto alla visione
        30	Visionato
        40	Accettato/Protocollato
        50	Documento firmato digitalmente
        60	Inviato al dipendente
        70	Accettato dal dipendente
        80	Rifiutato dal dipendente
     * */
    public enum StatiDematerializzazioneDocumenti
    {
        [Display(Name = "Bozza")]
        [DescriptionAttribute("Bozza")]
        [AmbientValue("Bozza")]
        [VisibilitaAttribute("Visionatore||Operatore")]
        Bozza = 5,

        [Display(Name = "Richiesta creata")]
        [DescriptionAttribute("Richiesta creata")]
        [AmbientValue("Pratica creata")]
        [VisibilitaAttribute("Visionatore||Operatore")]
        RichiestaCreata = 10,

        [Display(Name = "Azione automatica in fase di creazione documento")]
        [DescriptionAttribute("Azione automatica in fase di creazione documento")]
        [AmbientValue("")]
        [VisibilitaAttribute("")]
        AzioneAutomaticaInCreazione = 15,

        [Display(Name = "Documento pronto alla visione")]
        [DescriptionAttribute("Documento pronto alla visione")]
        [AmbientValue("Pratica in approvazione")]
        [VisibilitaAttribute("Visionatore||Operatore||Approvatore")]
        ProntoVisione = 20,

        [Display(Name = "Posizione del protocollo impostata")]
        [DescriptionAttribute("Posizione del protocollo impostata")]
        [AmbientValue("")]
        [VisibilitaAttribute("")]
        ProtocolloPosizionato = 25,

        [Display(Name = "Posizione del protocollo modificata")]
        [DescriptionAttribute("Posizione del protocollo modificata")]
        [AmbientValue("")]
        [VisibilitaAttribute("")]
        ModificaPosizioneProtocollo = 26,

        [Display(Name = "Azione Automatica")]
        [DescriptionAttribute("Azione Automatica")]
        [AmbientValue("")]
        [VisibilitaAttribute("")]
        AzioneAutomatica = 27,

        [Display(Name = "Documento modificato")]
        [DescriptionAttribute("Documento modificato")]
        [AmbientValue("")]
        [VisibilitaAttribute("")]
        DocumentoModificato = 28,

        [Display(Name = "Azione Automatica variazione contabile")]
        [DescriptionAttribute("Azione Automatica variazione contabile")]
        [AmbientValue("")]
        [VisibilitaAttribute("")]
        AzioneAutomaticaContabile = 29,

        [Display(Name = "Documento visionato")]
        [DescriptionAttribute("Documento visionato")]
        [AmbientValue("Pratica vistata")]
        [VisibilitaAttribute("Visionatore||Operatore||Approvatore")]
        Visionato = 30,

        [Display(Name = "Documento visionato dalla segreteria")]
        [DescriptionAttribute("Documento visionato dalla segreteria")]
        [AmbientValue("Pratica vistata dalla segreteria")]
        [VisibilitaAttribute("")]
        VisionatoSegreteria = 32,

        [Display(Name = "Azione Automatica Assunzione PreApprovazione")]
        [DescriptionAttribute("Azione Automatica Assunzione")]
        [AmbientValue("")]
        [VisibilitaAttribute("")]
        AzioneAutomaticaAssunzionePreApprovazione = 39,

        [Display(Name = "Accettato e/o Protocollato")]
        [DescriptionAttribute("Accettato e/o Protocollato")]
        [AmbientValue("Pratica approvata")]
        [VisibilitaAttribute("Visionatore||Operatore||Approvatore")]
        Accettato = 40,

        [Display(Name = "Protocollato")]
        [DescriptionAttribute("Protocollato")]
        [AmbientValue("Pratica protocollata")]
        [VisibilitaAttribute("Visionatore||Operatore||Approvatore")]
        Protocollato = 45,

        [Display(Name = "Azione Automatica Assunzione PreFirma")]
        [DescriptionAttribute("Azione Automatica Assunzione")]
        [AmbientValue("")]
        [VisibilitaAttribute("")]
        AzioneAutomaticaAssunzionePreFirma = 46,

        [Display(Name = "Documento firmato digitalmente")]
        [DescriptionAttribute("Documento firmato digitalmente")]
        [AmbientValue("Pratica firmata digitalmente")]
        [VisibilitaAttribute("Visionatore||Operatore||Approvatore")]
        FirmatoDigitalmente = 50,

        [Display(Name = "Inviato al dipendente")]
        [DescriptionAttribute("Inviato al dipendente")]
        [AmbientValue("")]
        [VisibilitaAttribute("")]
        InviatoAlDipendente = 60,

        [Display(Name = "Accettato dal dipendente")]
        [DescriptionAttribute("Accettato dal dipendente")]
        [AmbientValue("")]
        [VisibilitaAttribute("")]
        AccetttatoDalDipendente = 70,

        [Display(Name = "Rifiutato dal dipendente")]
        [DescriptionAttribute("Rifiutato dal dipendente")]
        [AmbientValue("")]
        [VisibilitaAttribute("")]
        RifiutatoDalDipendente = 80,

        [Display(Name = "Rifiutato dall'approvatore")]
        [DescriptionAttribute("Rifiutato dall'approvatore")]
        [AmbientValue("Pratica rifiutata")]
        [VisibilitaAttribute("Visionatore||Operatore||Approvatore")]
        RifiutoApprovatore = 90,

        [Display(Name = "Presa in carico")]
        [DescriptionAttribute("Presa in carico")]
        [AmbientValue("Presa in carico")]
        [VisibilitaAttribute("Visionatore||Operatore||Approvatore")]
        PresaInCarico = 100,

        [Display(Name = "Rifiutato in firma")]
        [DescriptionAttribute("Rifiutato in firma")]
        [VisibilitaAttribute("Visionatore||Operatore||Approvatore")]
        [AmbientValue("Rifiutato in firma")]
        RifiutatoFirma = 110,

        [Display(Name = "Azione automatica invio documento al dipendente")]
        [DescriptionAttribute("Azione automatica invio documento al dipendente")]
        [AmbientValue("")]
        [VisibilitaAttribute("")]
        AzioneAutomaticaInvioDocumentoAlDipendente = 119,

        [Display(Name = "Pratica conclusa")]
        [DescriptionAttribute("Pratica conclusa")]
        [VisibilitaAttribute("Visionatore||Operatore||Approvatore")]
        [AmbientValue("Pratica conclusa")]
        PraticaConclusa = 120,

        [Display(Name = "Pratica cancellata")]
        [DescriptionAttribute("Pratica cancellata")]
        [AmbientValue("Pratica eliminata")]
        [VisibilitaAttribute("Visionatore||Operatore||Approvatore")]
        PraticaCancellata = 200,

        [Display(Name = "Pratica inviata all'ufficio competente")]
        [DescriptionAttribute("Pratica inviata all'ufficio competente")]
        [AmbientValue("Pratica inviata all'ufficio competente")]
        //[VisibilitaAttribute("Visionatore||Operatore||Approvatore")]
        [VisibilitaAttribute("")]
        PraticaInviataUfficioCompetente = 220,

        [Display(Name = "Aggiunta dati JSON")]
        [DescriptionAttribute("Aggiunta dati JSON")]
        [AmbientValue("")]
        [VisibilitaAttribute("")]
        AggiuntaDatiJson = 300,

        [Display(Name = "STEPS DINAMICI CREA LISTA VISTATORI")]
        [DescriptionAttribute("STEPS DINAMICI CREA LISTA VISTATORI")]
        [AmbientValue("")]
        [VisibilitaAttribute("")]
        CreaStepsDinamiciVistatori = 500,

        [Display(Name = "PRE-VISIONATO")]
        [DescriptionAttribute("PRE-VISIONATO")]
        [AmbientValue("")]
        [VisibilitaAttribute("")]
        PreVisionato = 501,

        [Display(Name = "ESEGUI STEP DINAMICI")]
        [DescriptionAttribute("ESEGUI STEP DINAMICI")]
        [AmbientValue("")]
        [VisibilitaAttribute("")]
        EseguiStepsDinamici = 600,

        [Display(Name = "Azione automatica Post Firma - Creazione task automatici Mobilità Orizzonatale")]
        [DescriptionAttribute("Azione automatica Post Firma - Creazione task automatici Mobilità Orizzonatale")]
        [AmbientValue("")]
        [VisibilitaAttribute("")]
        AzioneAutomaticaPostFirma_CreazioneTask = 601,

        [Display(Name = "Azione automatica Post Firma - Aggiornamento tabelle Mobilità Orizzonatale")]
        [DescriptionAttribute("Azione automatica Post Firma - Aggiornamento tabelle Mobilità Orizzonatale")]
        [AmbientValue("")]
        [VisibilitaAttribute("")]
        AzioneAutomaticaPostFirma_AggiornamentoTabelleMobilitaOrizzontale = 602,

        [Display(Name = "Azione automatica Post Firma - Creazione dati per DEW")]
        [DescriptionAttribute("Azione automatica Post Firma - Creazione dati per DEW")]
        [AmbientValue("")]
        [VisibilitaAttribute("")]
        AzioneAutomaticaPostFirma_CreazioneDatiPerDEW = 603,

        [Display(Name = "Azione automatica Post Firma - Creazione task automatici Autorizzazioni AD Assunzioni")]
        [DescriptionAttribute("Azione automatica Post Firma - Creazione task automatici Autorizzazioni AD Assunzioni")]
        [AmbientValue("")]
        [VisibilitaAttribute("")]
        AzioneAutomaticaPostFirma_CreazioneTask_VSAD_ADASS = 604,

        [Display(Name = "Azione automatica Post Firma - Invio mail al dipendente destinatario")]
        [DescriptionAttribute("Azione automatica Post Firma - Invio mail al dipendente destinatario")]
        [AmbientValue("")]
        [VisibilitaAttribute("")]
        AzioneAutomaticaPostFirma_InvioMailAlDipendenteDestinatario = 605,

        [Display(Name = "Azione automatica Post Firma - Invio pratica ad HRDOC")]
        [DescriptionAttribute("Azione automatica Post Firma - Invio pratica ad HRDOC")]
        [AmbientValue("")]
        [VisibilitaAttribute("")]
        AzioneAutomaticaPostFirma_InvioPraticaAdHRDOC = 606,

        [Display(Name = "Azione automatica Post Firma - Creazione task automatici RUO Assunzione")]
        [DescriptionAttribute("Azione automatica Post Firma - Creazione task automatici RUO Assunzione")]
        [AmbientValue("")]
        [VisibilitaAttribute("")]
        AzioneAutomaticaPostFirma_RUO_Assunzione = 607,

        [Display(Name = "Azione automatica Post Firma - Task automatici Autorizzazioni per attività Extra Rai")]
        [DescriptionAttribute("Azione automatica Post Firma - Task automatici Autorizzazioni per attività Extra Rai")]
        [AmbientValue("")]
        [VisibilitaAttribute("")]
        AzioneAutomaticaPostFirma_AutorizzazioneAttivitaExtraRai = 608,

        [Display(Name = "Azione automatica Post Firma - Task automatici invio Mail by template")]
        [DescriptionAttribute("Azione automatica Post Firma - Task automatici invio Mail by template")]
        [AmbientValue("")]
        [VisibilitaAttribute("")]
        AzioneAutomaticaPostFirma_InvioMailByTemplate = 609
    }

    //public enum DEM_TIPOLOGIE_DOCUMENTALI_ENUM
    //{
    //    EXTRUO = 1,
    //    VSDIP = 2,
    //    VSEXT = 3,
    //    VSRUO = 4
    //}

    public class DematerializzaioneBaseEnumObject
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public string Display { get; set; }
        public int Valore { get; set; }        
    }

    public class ListaDematerializzazione : BaseAnagrafica
    {
        public List<XR_RICHIESTE> Richieste { get; set; }
    }

    public class InsRicModel : _IdentityData
    {
        public InsRicModel()
        {
            this.NumeroPaginaFirma = 1;
            this.NumeroPaginaProtocollo = 1;
        }
        // modifica modello
        public string TipologiaWKF { get; set; }
        // modifica modello
        //public WKF_TIPOLOGIA_ENUM TipologiaWKF { get; set; }
        // modifica modello
        //public DEM_TIPOLOGIE_DOCUMENTALI_ENUM TipologiaDocumentale { get; set; }
        // modifica modello
        public string TipologiaDocumentale { get; set; }
        // modifica modello
        public string TipologiaDocumento { get; set; }
        // modifica modello
        //public DEM_TIPI_DOCUMENTO_ENUM TipologiaDocumento { get; set; }
        public string MatricolaDestinatario { get; set; }
        public int IdPersonaDestinatario { get; set; }
        public string Descrizione { get; set; }
        public string Note { get; set; }
        public string IncaricatoFirma { get; set; }
        public string MatricolaApprovatore { get; set; }
        public string MatricolaVisionatore { get; set; }
        public string MatricolaApprovatoreRuo { get; set; }
        public string MatricolaApprovatoreRuoNoIncaricato { get; set; }
        public string MatricolaIncaricato { get; set; }
        public string Allegati { get; set; }
        public int IdDocumento { get; set; }
        public int IdAllegatoPrincipale { get; set; }
        public bool RicercaLibera { get; set; }
        public int NumeroPaginaFirma { get; set; }
        public List<TemplateModel> ListaTemplates { get; set; }
        public string CustomAttrs { get; set; }
        /// <summary>
        /// se true rende visibile il pannello per caricare allegati nel primo tab del wizard
        /// </summary>
        public bool AbilitaPannelloAllegati { get; set; }
        public int NumeroPaginaProtocollo { get; set; }
        public int idArea { get; set; }
    }

    public class TemplateModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class RichiestaDoc
    {
        public List<XR_ALLEGATI> Allegati { get; set; }
        public XR_DEM_DOCUMENTI Documento { get; set; }
        public string PNGBase64 { get; set; }
        public int IdPersona { get; set; }
        public string Matricola { get; set; }
    }

    public class CaricaTabellaDocumentiVM
    {
        public List<XR_DEM_DOCUMENTI_EXT> DocumentiDaLavorare { get; set; }
        public List<XR_DEM_DOCUMENTI_EXT> DocumentiRifiutati { get; set; }
        public List<XR_DEM_DOCUMENTI_EXT> Documenti { get; set; }
        public string Matricola { get; set; }
    }

    public class DematerializzazioneOggettoPerNavigazione
    {
        private bool _hasPrev = false;
        private bool _hasNext = false;

        public DematerializzazioneOggettoPerNavigazione()
        {
            this.DocumentiInFirma = new List<XR_DEM_DOCUMENTI_EXT>();
            this.DocumentiDaLavorare = new List<XR_DEM_DOCUMENTI_EXT>();
            this.DocumentiRifiutati = new List<XR_DEM_DOCUMENTI_EXT>();
            this.PosizioneCorrente = 1;
        }

        public int IdDocumentoCorrente { get; set; }
        public List<XR_DEM_DOCUMENTI_EXT> DocumentiInFirma { get; set; }
        public List<XR_DEM_DOCUMENTI_EXT> DocumentiDaLavorare { get; set; }
        public List<XR_DEM_DOCUMENTI_EXT> DocumentiRifiutati { get; set; }
        public string Matricola { get; set; }
        public int PosizioneCorrente { get; set; }

        public bool HasPrev
        {
            get
            {
                if (this.DocumentiDaLavorare == null || !this.DocumentiDaLavorare.Any())
                {
                    this._hasPrev = false;
                }
                else
                {
                    var temp = DocumentiDaLavorare.Select(w => w.Id).ToList();

                    int pos = temp.IndexOf(IdDocumentoCorrente);

                    if (pos == 0)
                    {
                        this._hasPrev = false;
                    }
                    else
                    {
                        this._hasPrev = true;
                    }
                }

                return this._hasPrev;
            }
            set
            {
                this._hasPrev = value;
            }
        }

        public bool HasNext
        {
            get
            {
                if (this.DocumentiDaLavorare == null || !this.DocumentiDaLavorare.Any())
                {
                    this._hasNext = false;
                }
                else
                {
                    var temp = DocumentiDaLavorare.Select(w => w.Id).ToList();

                    int pos = temp.IndexOf(IdDocumentoCorrente);

                    // se è 1 di 1 oppure è l'ultimo della lista allora non ha next
                    if ((pos + 1) == temp.Count())
                    {
                        this._hasNext = false;
                    }
                    else
                    {
                        this._hasNext = true;
                    }
                }

                return this._hasNext;
            }
            set
            {
                this._hasNext = value;
            }
        }
    }

    public class DematerializzazioneDocumentoInFirma
    {
        public int Id { get; set; }
        public bool InFirma { get; set; }
    }

    public class Dematerializzazione_DocumentiInFirmaVM
    {
        public Dematerializzazione_DocumentiInFirmaVM()
        {
            this.DocumentiInFirma = new List<XR_DEM_DOCUMENTI_EXT>();
            this.ConteggioDocumentiInFirma = 0;
        }

        public int ConteggioDocumentiInFirma { get; set; }
        public List<XR_DEM_DOCUMENTI_EXT> DocumentiInFirma { get; set; }
    }

    [DataContract]
    public class PosizioneProtocolloOBJ
    {
        [DataMember]
        public string Oggetto { get; set; }
        [DataMember]
        public float PosizioneLeft { get; set; }
        [DataMember]
        public float PosizioneTop { get; set; }
        [DataMember]
        public int NumeroPagina { get; set; }
    }

    [Serializable]
    [XmlRoot(ElementName = "protocollo")]
    public class Protocollo
    {
        public Protocollo()
        {
            TipoProtocollo = 2;
        }

        [XmlElement(ElementName = "tipo_protocollo")]
        public int TipoProtocollo { get; set; }

        [XmlElement(ElementName = "oggetto")]
        public string Oggetto { get; set; }

        [XmlElement(ElementName = "mittente")]
        public string Mittente { get; set; }

        [XmlElement("destinatari", typeof(Destinatari))]
        public Destinatari Destinatari { get; set; }

        [XmlElement(ElementName = "destinatari_cc")]
        public DestinatariCc DestinatariCc { get; set; }

        [XmlElement(ElementName = "data_spedizione")]
        public string DataSpedizione { get; set; }
    }

    [Serializable]
    [XmlRoot(ElementName = "destinatario")]
    public class Destinatario
    {
        [XmlAttribute(AttributeName = "indirizzo_mail")]
        public string IndirizzoMail { get; set; }

        [XmlAttribute(AttributeName = "tipo_canale")]
        public string TipoCanale { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [Serializable]
    [XmlRoot(ElementName = "destinatari")]
    public class Destinatari
    {
        [XmlElement(ElementName = "destinatario")]
        public List<Destinatario> Destinatario { get; set; }
    }

    [Serializable]
    [XmlRoot(ElementName = "destinatario_cc")]
    public class DestinatarioCc
    {
        [XmlAttribute(AttributeName = "indirizzo_mail")]
        public string IndirizzoMail { get; set; }

        [XmlAttribute(AttributeName = "tipo_canale")]
        public string TipoCanale { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [Serializable]
    [XmlRoot(ElementName = "destinatari_cc")]
    public class DestinatariCc
    {
        [XmlElement(ElementName = "destinatario_cc")]
        public DestinatarioCc DestinatarioCc { get; set; }
    }

    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }

    [Serializable]
    [XmlRoot(ElementName = "creaProtocollo")]
    public class CreaProtocollo
    {
        [XmlElement(ElementName = "ID")]
        public string ID { get; set; }
        [XmlElement(ElementName = "id_documento")]
        public string Id_documento { get; set; }
        [XmlElement(ElementName = "identificativo")]
        public string Identificativo { get; set; }
        [XmlElement(ElementName = "data_protocollo")]
        public string Data_protocollo { get; set; }
        [XmlElement(ElementName = "cod_aoo")]
        public string Cod_aoo { get; set; }
        [XmlElement(ElementName = "cod_amm")]
        public string Cod_amm { get; set; }
        [XmlElement(ElementName = "Errore")]
        public Errore Errore { get; set; }
    }

    [Serializable]
    [XmlRoot(ElementName = "inserisciAllegato")]
    public class InserisciAllegato
    {
        [XmlElement(ElementName = "id_attach")]
        public string Id_attach { get; set; }

        [XmlElement(ElementName = "Errore")]
        public Errore Errore { get; set; }
    }

    public class ApplicaProtocolloResult
    {
        public byte[] File { get; set; }
        public string Protocollo { get; set; }
        public string Id_Documento { get; set; }
        public string Data_protocollo { get; set; }
        public string Cod_aoo { get; set; }
        public string Cod_amm { get; set; }
    }

    [Serializable]
    [XmlRoot(ElementName = "confermaSpedizioni")]
    public class ConfermaSpedizione
    {
        [XmlElement(ElementName = "Errore")]
        public Errore Errore { get; set; }
    }

    [Serializable]
    [XmlRoot(ElementName = "Errore")]
    public class Errore
    {
        [XmlAttribute(AttributeName = "id_errore")]
        public string Id_errore { get; set; }
        [XmlText]
        public string Text { get; set; }
    }

    public class DematerializzazioneDocumentiVSSEttoriVM : _IdentityData
    {
        public bool PrendiInCaricoEnabled { get; set; }
        public bool ApprovazioneEnabled { get; set; }
        public bool IsPreview { get; set; }
        public List<XR_DEM_DOCUMENTI_EXT> DocumentiInCarico { get; set; }
        public List<XR_DEM_DOCUMENTI_EXT> DocumentiDaPrendereInCarico { get; set; }
        public List<XR_DEM_DOCUMENTI_EXT> DocumentiInCaricoAltri { get; set; }
        public Dematerializzazione_FiltriApprovatore Filtri { get; set; }
    }

    public class DematerializzazioneDocumentiVM : _IdentityData
    {
        public bool PrendiInCaricoEnabled { get; set; }
        public bool ApprovazioneEnabled { get; set; }
        public bool IsPreview { get; set; }
        public List<XR_DEM_DOCUMENTI_EXT> Documenti { get; set; }
        public List<XR_DEM_DOCUMENTI_EXT> DocumentiDaVisionare { get; set; }
        public List<XR_DEM_DOCUMENTI_EXT> DocumentiInCaricoAMe { get; set; }
        public List<XR_DEM_DOCUMENTI_EXT> DocumentiInCaricoAdAltri { get; set; }
        public List<XR_DEM_DOCUMENTI_EXT> DocumentiDaPrendereInCarico { get; set; }
        public Dematerializzazione_FiltriApprovatore Filtri { get; set; }

        public bool IsOperatore { get; set; }
        public bool IsApprovatore { get; set; }
        public bool IsVisionatore { get; set; }
        public bool NascondiCheckBox { get; set; }
        public bool IsSegreteria { get; set; }
    }

    public class Dematerializzazione_FiltriApprovatore
    {
        public string MatricolaONominativo { get; set; }
        public string Oggetto { get; set; }
        public int TipologiaDocumento { get; set; }
        public int StatoRichiesta { get; set; }
        public int Mese { get; set; }
        public string Sede { get; set; }

        public string Nominativo { get; set; }
    }

    public class DettaglioDematerializzazioneVM : _IdentityData
    {
        public RichiestaDoc Richiesta { get; set; }
        public string NominativoUtenteApprovatore { get; set; }
        public string NominativoUtenteCreatore { get; set; }
        public string NominativoUtenteDestinatario { get; set; }
        public string NominativoUtenteFirma { get; set; }
        public string NominativoUtenteIncaricato { get; set; }
        public string NominativoUtenteVisionatore { get; set; }
        public bool ApprovazioneEnabled { get; set; }
        public bool PresaInCaricoEnabled { get; set; }
        public bool PresaInVisioneEnabled { get; set; }
        public bool AbilitaModifica { get; set; }
        public bool MessaggioPraticaBloccata { get; set; }
        public string Msg1 { get; set; }
        public string Msg2 { get; set; }
        public bool IsOperatore { get; set; }
        public DematerializzazioneOggettoPerNavigazione Navigazione { get; set; }
        public bool DocumentoInFirma { get; set; }
        public List<AttributiAggiuntivi> Attributi { get; set; }
        public Dematerializzazione_Dettaglio_Mat_Richieste Dettaglio_Mat_Richieste { get; set; }
        /// <summary>
        /// se true, nella vista di dettaglio verrà visualizzato il bottone che consente di aggiungere
        /// dati in base al json contenuto nella tabella di workflow
        /// </summary>
        public bool AbilitaAggiuntaInfoJson { get; set; }
        /// <summary>
        /// Lista di attributi aggiunti in base all'eventuale json contenuto nella tabella
        /// del workflow
        /// </summary>
        public List<AttributiAggiuntivi> AttributiAggiuntiWKF { get; set; }
        public bool AbilitaModificaInfoJson { get; set; }
        public bool IsDuplicable { get; set; }
        public bool IsApprovatore { get; set; }
        public bool IsSegreteria { get; set; }

        public ReportMyRaiPianoFerieBatch ReportInserimentiEccezioniInGapp { get; set; }

        public List<Dematerializzazione_Tab_Dettaglio_Pratica> Tabs { get; set; }

        public List<string> NominativoUtenteVistatore { get; set; }

        public bool NascondiEliminaPratica { get; set; }
        public bool NascondiConcludiPratica { get; set; }
        public bool NascondiRiprendi { get; set; }
        public bool AbilitaPannelloLog { get; set; }

        public List<EventoLog> EventiLog { get; set; }

        public List<NotaVM> Note { get; set; }
        public int ProssimoStato { get; set; }

        // questi due campi sono legati alla nuova gestione del visto tramite la nuova tabella
        // [XR_WKF_WORKFLOW_DYNAMIC_STEPS], se in questa tabella c'è la matricola vistatore e
        // la data è valorizzata allora il booleano sarà true perchè il bottone visto deve essere
        // disabilitato ed il campo stringa conterrà una dicitura del tipo
        // Pratica vistata il xx/xx/xxxx oppure vistato il ..
        public bool BottoneVistaDisabilitato { get; set; }
        public string InfoBottoneVistaDisabilitato { get; set; }
    }

    public enum StatoInserimentoMyRaiPianoFerieBatchEnum
    {
        [AmbientValue("Nessuna giornata inserita")]
        DaCaricareInTabella = 0,
        [AmbientValue("Le eccezioni sono state inserite nel sistema, ma ")]
        DaLavorare = 1,
        InLavorazione = 2,
        Lavorati = 3
    }

    public class EsitoInserimentoMyRaiPianoFerieBatch
    {
        public int Id { get; set; }
        public int? IdRichiestaDB { get; set; }
        public int? NumeroDocumento { get; set; }
        public DateTime? DataUltimoTentativo { get; set; }
        public string Errore { get; set; }
    }

    public class ReportMyRaiPianoFerieBatch: Dematerializzazione_EsitoAjax
    {
        public ReportMyRaiPianoFerieBatch()
        {
            this.StatoInserimento = StatoInserimentoMyRaiPianoFerieBatchEnum.DaCaricareInTabella;
            this.Giorni = new List<EsitoInserimentoMyRaiPianoFerieBatch>();
            this.ErrorMessage = string.Empty;
            this.Esito = false;
            this.Id = 0;
        }

        public StatoInserimentoMyRaiPianoFerieBatchEnum StatoInserimento { get; set; }
        public List<EsitoInserimentoMyRaiPianoFerieBatch> Giorni { get; set; }
    }

    public class XR_DEM_DOCUMENTI_EXT : XR_DEM_DOCUMENTI
    {
        public string NominativoUtenteApprovatore { get; set; }
        public string NominativoUtenteCreatore { get; set; }
        public string NominativoUtenteDestinatario { get; set; }
        public string NominativoUtenteIncaricato { get; set; }
        public int Avanzamento { get; set; }
        public string MatricolaUtenteCorrente { get; set; }
        public int IdPersona { get; set; }
        public int Tab { get; set; }
        public XR_ALLEGATI AllegatoPrincipale { get; set; }
        public bool IsDuplicable { get; set; }
        public bool AlmenoUnCheck { get; set; }
        public bool NascondiBottoniPrendiInCarico { get; set; }
        public string NominativoUtenteVistatore { get; set; }

        /// <summary>
        /// Questo attributo che verrà calcolato e assegnato servirà per
        /// contenere il testo che verrà stampato a video nel momento della
        /// stampa della lista documenti per approvatori/operatori etc
        /// in questo modo verrà stampato a video il testo ad esempio
        /// Assegnazione temporanea (3R) nel caso di una variazione contabile con eccezione di
        /// tipo 3R. 
        /// Oppure Maternità e altri congedi parentali (AF) etc       
        /// </summary>
        private string _descrizionePerVisualizzazione { get; set; }

        /// <summary>
        /// Nel caso di documenti senza matricola destinatario
        /// viene calcolato un sottotitolo che viene aggiunto sotto all'attributo contenuto in 
        /// _descrizionePerVisualizzazione.
        /// Ad esempio per i mandati di cassa in _descrizionePerVisualizzazione
        /// viene riportato la tipologia del documento quindi mandato di cassa UAD o COCO etc
        /// mentre nel sottotitolo contenuto in questo attributo viene riportato
        /// l'oggetto del documento
        /// </summary>
        private string _descrizionePerVisualizzazioneSottotitolo { get; set; }

        public string DescrizionePerVisualizzazione
        {
            get
            {
                string _codiceEvento = "";
                _descrizionePerVisualizzazione = this.Descrizione;
                List<AttributiAggiuntivi> objModuloValorizzato = null;

                if (this.Id_Tipo_Doc == 35)
                {
                    // SE SI TRATTA DI UNA VARIAZIONE CONTABILE ALLORA LA DICITURA NON SARA' 
                    // DATA DA DESCRIZIONE (CODICE ECCEZIONE), MA AVRA' 
                    // NOME TIPO VARIAZIONE (CODICE EVENTO)
                    // ES. Assegnazione temporanea (3R)

                    if (!String.IsNullOrEmpty(this.CustomDataJSON) && this.CustomDataJSON != "[]")
                    {
                        objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(this.CustomDataJSON);
                        var tt = objModuloValorizzato.Where(w => w.Id == "EccezionePerAutomatismo").FirstOrDefault();
                        if (tt != null)
                        {
                            _codiceEvento = tt.Valore;
                        }

                        if (String.IsNullOrEmpty(_codiceEvento))
                        {
                            tt = objModuloValorizzato.Where(w => w.Id == "EccezioneSelezionataNascosta").FirstOrDefault();
                            if (tt != null)
                            {
                                _codiceEvento = tt.Valore;
                            }
                        }

                        if (String.IsNullOrEmpty(_codiceEvento))
                        {
                            throw new Exception("Impossibile proseguire tipo di variazione mancante");
                        }
                    }

                    /*
                        "text": "Assegnazione temporanea",
                        "value": "03R"
                        "text": "Fine assegnazione temporanea",
                        "value": "03S"
                        "text": "Fine assegnazione anticipata",
                        "value": "03S_A"
                        "text": "Proroga assegnazione temporanea",
                        "value": "PAT"
                        "text": "Nuova assegnazione temporanea",
                        "value": "03R"
                        "text": "Assegnazione definitiva",
                        "value": "030"
                        "text": "Trasferimento temporaneo",
                        "value": "03T"
                        "text": "Fine trasferimento temporaneo",
                        "value": "03U"
                        "text": "Distacco",
                        "value": "031"
                        "text": "Cambio sezione",
                        "value": "03Z"
                        "text": "Trasferimento definitivo",
                        "value": "005"
                        "text": "Trasferimento a domanda",
                        "value": "034"
                        */

                    switch (_codiceEvento)
                    {
                        case "03R":
                            _descrizionePerVisualizzazione = "Assegnazione temporanea";
                            break;
                        case "03S":
                            _descrizionePerVisualizzazione = "Fine assegnazione temporanea";
                            break;
                        case "03S_A":
                            _descrizionePerVisualizzazione = "Fine assegnazione anticipata";
                            break;
                        case "PAT":
                            _descrizionePerVisualizzazione = "Proroga assegnazione temporanea";
                            break;
                        case "030":
                            _descrizionePerVisualizzazione = "Assegnazione definitiva";
                            break;
                        case "03T":
                            _descrizionePerVisualizzazione = "Trasferimento temporaneo";
                            break;
                        case "03U":
                            _descrizionePerVisualizzazione = "Fine trasferimento temporaneo";
                            break;
                        case "031":
                            _descrizionePerVisualizzazione = "Distacco";
                            break;
                        case "03Z":
                            _descrizionePerVisualizzazione = "Cambio sezione";
                            break;
                        case "005":
                            _descrizionePerVisualizzazione = "Trasferimento definitivo";
                            break;
                        case "034":
                            _descrizionePerVisualizzazione = "Trasferimento a domanda";
                            break;
                    }

                    if (_codiceEvento.StartsWith("0"))
                    {
                        _codiceEvento = _codiceEvento.Substring(1);
                    }

                    _descrizionePerVisualizzazione = String.Format("{0} ({1})", _descrizionePerVisualizzazione, _codiceEvento);
                }
                else if (this.Id_Tipo_Doc == 38)
                {
                    // SE SI TRATTA DI UN APPUNTO ALLORA LA DICITURA NON SARA' 
                    // DATA DA DESCRIZIONE (CODICE ECCEZIONE), MA AVRA' 
                    // NOME APPUNTO (OGGETTO)
                    // ES. APPUNTO (OGGETTO INSERITO IN FASE DI CREAZIONE)
                    if (!String.IsNullOrEmpty(this.CustomDataJSON) && this.CustomDataJSON != "[]")
                    {
                        objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(this.CustomDataJSON);
                        var tt = objModuloValorizzato.Where(w => w.Id == "Oggetto").FirstOrDefault();
                        if (tt != null)
                        {
                            _codiceEvento = tt.Valore;
                        }

                        if (String.IsNullOrEmpty(_codiceEvento))
                        {
                            throw new Exception("Impossibile proseguire oggetto non trovato");
                        }
                    }

                    _descrizionePerVisualizzazione = String.Format("{0} ({1})", _descrizionePerVisualizzazione, _codiceEvento);
                }
                else if (!String.IsNullOrEmpty(this.CustomDataJSON) && !String.IsNullOrEmpty(this.MatricolaDestinatario))
                {
                    string ecc = null;
                    objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(this.CustomDataJSON);

                    if (objModuloValorizzato != null && objModuloValorizzato.Any())
                    {
                        var obj = objModuloValorizzato.Where(w => w.Id != null && w.Id.ToUpper() == "ECCEZIONEPERAUTOMATISMO").FirstOrDefault();
                        if (obj != null)
                        {
                            ecc = obj.Valore;
                        }

                        if (String.IsNullOrEmpty(ecc))
                        {
                            obj = objModuloValorizzato.Where(w => w.Id != null && w.Id.ToUpper() == "ECCEZIONESELEZIONATANASCOSTA").FirstOrDefault();
                            if (obj != null)
                            {
                                ecc = obj.Valore;
                            }
                        }

                        if (String.IsNullOrEmpty(ecc))
                        {
                            obj = objModuloValorizzato.Where(w => w.Id != null && w.Id.ToUpper() == "ECCEZIONE").FirstOrDefault();
                            if (obj != null)
                            {
                                ecc = obj.Valore;
                            }
                        }

                        if (!String.IsNullOrEmpty(ecc))
                        {
                            _descrizionePerVisualizzazione += " (" + ecc.ToUpper() + ")";
                        }
                    }
                }

                return _descrizionePerVisualizzazione;
            }
            set
            {
                _descrizionePerVisualizzazione = value;
            }
        
        }

        public string DescrizionePerVisualizzazioneSottotitolo
        {
            get
            {
                _descrizionePerVisualizzazioneSottotitolo = "";
                List<AttributiAggiuntivi> objModuloValorizzato = null;

                // prima di tutto calcola il tipo di documento
                //MDG ad una prima vista query  inutile
                //XR_DEM_TIPI_DOCUMENTO tipoDocumento = new XR_DEM_TIPI_DOCUMENTO();
                //using (IncentiviEntities db = new IncentiviEntities())
                //{
                //    tipoDocumento = db.XR_DEM_TIPI_DOCUMENTO.Where(w => w.Id == this.Id_Tipo_Doc).FirstOrDefault();
                //    if (tipoDocumento == null)
                //    {
                //        throw new Exception("Tipo documento non trovato");
                //    }
                //}

                if (!String.IsNullOrEmpty(this.CustomDataJSON) && String.IsNullOrEmpty(this.MatricolaDestinatario))
                {
                    // MDC, MDCO, VPA, MDCUAD
                    objModuloValorizzato = JsonConvert.DeserializeObject<List<AttributiAggiuntivi>>(this.CustomDataJSON);

                    if (objModuloValorizzato != null && objModuloValorizzato.Any())
                    {
                        var obj = objModuloValorizzato.Where(w => w.DBRefAttribute != null && w.DBRefAttribute.ToUpper() == "OGGETTOPERPROTOCOLLO").FirstOrDefault();
                        if (obj != null)
                        {
                            _descrizionePerVisualizzazioneSottotitolo = obj.Valore;
                        }
                    }
                }

                return _descrizionePerVisualizzazioneSottotitolo;
            }
            set
            {
                _descrizionePerVisualizzazioneSottotitolo = value;
            }

        }

        // attributo legato all'utente corrente che accede alla pratica
        public bool DaVistare { get; set; }
        public DateTime? DataVistoUtenteCorrente { get; set; }

        public int GetGiorniPerScadenzaPratica()
        {
            int day = 0;
            try
            {
                using (var digi = new myRaiData.digiGappEntities())
                {
                    day = Convert.ToInt32(digi.MyRai_ParametriSistema.Where(x => x.Chiave.Equals("GiorniPerScadenzaPratica")).Select(k => k.Valore1).FirstOrDefault());
                }
            }
            catch (Exception)
            {
            }
            
            return day;
        }
    }

    public class Dematerializzazione_TRFileUploadVM
    {
        public List<XR_ALLEGATI> Allegati { get; set; }
        public bool Principale { get; set; }
        public bool InModifica { get; set; }
        public bool SkipSalvataggioCompleto { get; set; }
        public bool DaSiglare { get; set; }
        public int TipologiaDocumento { get; set; }
    }

    public class RiepilogoVM
    {
        public RichiestaDoc Documento { get; set; }
        public string NominativoUtenteApprovatore { get; set; }
        public string NominativoUtenteCreatore { get; set; }
        public string NominativoUtenteDestinatario { get; set; }
        public string NominativoUtenteIncaricatoFirma { get; set; }
        public string NominativoUtenteIncaricato { get; set; }
        public List<string> NominativoUtenteVistatore { get; set; }
        public List<AttributiAggiuntivi> Attributi { get; set; }
        public List<NotaVM> Note { get; set; }
    }

    public class Dem_ModificaDocumentoVM : InsRicModel
    {
        public RichiestaDoc Richiesta { get; set; }
        public string NominativoUtenteApprovatore { get; set; }
        public string NominativoUtenteCreatore { get; set; }
        public string NominativoUtenteDestinatario { get; set; }
        public string NominativoUtenteFirma { get; set; }
        public string NominativoUtenteIncaricato { get; set; }
        public List<string> NominativoUtenteVistatore { get; set; }
        public XR_DEM_TIPIDOC_COMPORTAMENTO Comportamento { get; set; }
        public string WKF_TIPOLOGIA { get; set; }
        public DematerializzazioneCustomDataView ModelDatiAggiuntivi { get; set; }
        public bool IsSegreteria { get; set; }
        public List<string> CodiceProtocollatore { get; set; }
        /// <summary>
        /// Se true non segue tutto il percorso del wizard
        /// ma dopo l'inserimento dei dati custom, effettua
        /// direttamente il salvataggio dell'oggetto, così
        /// da aggiornare soltanto il CUSTOMJSON e non gli altri dati
        /// come Approvatore, incaricato firma etc
        /// </summary>
        public bool SkipSalvataggioCompleto { get; set; }
    }

    public class UpdateDocumento
    {
        public int Id { get; set; }
        public string Descrizione { get; set; }
        public string Cod_Tipologia_Documentale { get; set; }
        public int Id_Stato { get; set; }
        public string MatricolaCreatore { get; set; }
        public int IdPersonaCreatore { get; set; }
        public string MatricolaDestinatario { get; set; }
        public int? IdPersonaDestinatario { get; set; }
        public string MatricolaApprovatore { get; set; }
        public int? IdPersonaApprovatore { get; set; }
        public string MatricolaFirma { get; set; }
        public int? IdPersonaFirma { get; set; }
        public int Id_WKF_Tipologia { get; set; }
        public string Note { get; set; }
        public int Id_Tipo_Doc { get; set; }
        public string NotaApprovatore { get; set; }
        public string MatricolaIncaricato { get; set; }
        public string NotaFirma { get; set; }
        public string Allegati { get; set; }
        public int IdAllegatoPrincipale { get; set; }
        public int IdAllegatoPrincipaleOLD { get; set; }
        public string AllegatiEliminati { get; set; }
        public string CustomAttrs { get; set; }
    }

    public class DematerializzazioneDocumentiInPartenzaVM : _IdentityData
    {
        public bool IsOperatore { get; set; }
        public bool IsApprovatore { get; set; }
        public bool IsVisionatore { get; set; }
        public bool IsPreview { get; set; }
        public Dematerializzazione_FiltriApprovatore Filtri { get; set; }

        public bool IsSegreteria { get; set; }
    }

    public enum DEM_TIPO_RIFIUTATI
    {
        ALL = 1,
        RifiutateInApprovazione = 2,
        RifiutateInFirma = 3,
        RifiutateDalDipendente = 4
    }

    public enum TIPI_UTENTI
    {
        Operatore = 1,
        Vistatore,
        Approvatore,
        Segreteria
    }

    [Serializable]
    [XmlRoot(ElementName = "")]
    public class AttributiAggiuntiviBase
    {
        [XmlElement(ElementName = "Id")]
        public string Id { get; set; }

        [XmlElement(ElementName = "Valore")]
        public string Valore { get; set; }

        [XmlElement(ElementName = "Tipo")]
        public TipologiaAttributoEnum Tipo { get; set; }

        [XmlElement(ElementName = "ValoreInModifica")]
        public string ValoreInModifica { get; set; }

        [XmlElement(ElementName = "Gruppo")]
        public string Gruppo { get; set; }

        [XmlElement(ElementName = "Checked")]
        public bool Checked { get; set; }
    }

    [Serializable]
    [XmlRoot(ElementName = "")]
    public class AttributiAggiuntivi: AttributiAggiuntiviBase
    {
        /// <summary>
        /// Il contenuto di questo campo verrà utilizzato per
        /// valorizzare il testo della label accanto all'elemento
        /// Campi: text e title della label
        /// </summary>
        [XmlElement(ElementName = "Label")]
        public string Label { get; set; }

        /// <summary>
        /// Questo attributo viene utilizzato per valorizzare il placeholder
        /// dell'elemento
        /// </summary>
        [XmlElement(ElementName = "Title")]
        public string Title { get; set; }

        /// <summary>
        /// Attributo nome
        /// </summary>
        [XmlElement(ElementName = "Nome")]
        public string Nome { get; set; }

        [XmlElement(ElementName = "Testo")]
        public string Testo { get; set; }

        /// <summary>
        /// Utilizzato per costruire in fase di render della view
        /// funzioni JS definite nel JSON
        /// </summary>
        [XmlElement(ElementName = "Azioni")]
        public List<JsonTask> Azioni { get; set; }

        [XmlElement(ElementName = "Ordinamento")]
        public int Ordinamento { get; set; }

        /// <summary>
        /// Se true, l'elemento è obbligatorio
        /// </summary>
        [XmlElement(ElementName = "Required")]
        public bool Required { get; set; }

        /// <summary>
        /// Se valorizzato indica a quale valore è riferito l'oggetto.
        /// Es. Sede, servizio, settore.. anno primo contratto etc
        /// Viene utilizzato per valorizzare il campo coi dati presi 
        /// dalla query hrwd
        /// </summary>
        [XmlElement(ElementName = "TagHRDW")]
        public string TagHRDW { get; set; }

        /// <summary>
        /// Lista di voci selezionabili, utilizzata nel caso di
        /// elemento di tipo select, check, radio
        /// </summary>
        [XmlElement(ElementName = "SelectListItems")]
        public List<SelectListItem> SelectListItems { get; set; }

        /// <summary>
        /// Nel caso di elemento select, questo url serve
        /// per indicare quale metodo richiamare per il 
        /// caricamento dei dati di default
        /// </summary>
        [XmlElement(ElementName = "UrlLoadData")]
        public string UrlLoadData { get; set; }

        [XmlElement(ElementName = "MinLength")]
        public int MinLength { get; set; }

        [XmlElement(ElementName = "MaxLength")]
        public int MaxLength { get; set; }

        [XmlElement(ElementName = "Visible")]
        public bool Visible { get; set; }

        [XmlElement(ElementName = "OnChange")]
        public string OnChange { get; set; }

        [XmlElement(ElementName = "OnClick")]
        public string OnClick { get; set; }

        [XmlElement(ElementName = "OnBlur")]
        public string OnBlur { get; set; }

        [XmlElement(ElementName = "OnSelect")]
        public string OnSelect { get; set; }

        [XmlElement(ElementName = "OnReady")]
        public string OnReady { get; set; }

        [XmlElement(ElementName = "DBRefAttribute")]
        public string DBRefAttribute { get; set; }

        [XmlElement(ElementName = "Classe")]
        public string Classe { get; set; }

        [XmlElement(ElementName = "ClasseImg")]
        public string ClasseImg { get; set; }

        /// <summary>
        /// Utilizzato per renderizzare più elementi sulla stessa riga, come
        /// ad esempio le date. Dal - Al
        /// </summary>
        [XmlElement(ElementName = "InLine")]
        public List<AttributiAggiuntivi> InLine { get; set; }

        [XmlElement(ElementName = "DataAttributeElement")]
        public List<DataAttributeElement> DataAttributeElements { get; set; }

        [XmlElement(ElementName = "DivParent")]
        public string DivParent { get; set; }

        [XmlElement(ElementName = "Align")]
        public AttributiAggiuntiviAlign Align { get; set; }

        [XmlElement(ElementName = "RaggruppaConLabel")]
        public bool RaggruppaConLabel { get; set; }

        [XmlElement(ElementName = "Buttons")]
        public List<AttributiAggiuntivi> Buttons { get; set; }

        /// <summary>
        /// Se valorizzato, indica a quale colonna della tabella SINTESI1
        /// fa riferimento il dato.
        /// Questo attributo viene valorizzato quando è previsto un dato preesistente
        /// caricato al momento del render della view.
        /// Nel metodo CaricaCustomData, prende questo attributo effettua la query su 
        /// SINTESI1 ed il valore restituito dalla query lo assegna al campo VALORE
        /// </summary>
        [XmlElement(ElementName = "TagSINTESI1")]
        public string TagSINTESI1 { get; set; }
        
        /// <summary>
        /// Indica se l'elemento deve essere nascosto in modalità readonly
        /// </summary>
        [XmlElement(ElementName = "HideInReadOnly")]
        public bool HideInReadOnly { get; set; }

        /// <summary>
        /// Se true significa che nella schermata dovrà vedersi solo
        /// la parte sinistra, cioè il dato attuale, ma non sarà possibile
        /// assegnare alcun valore all'elemento
        /// </summary>
        //[XmlElement(ElementName = "VisibileSoloRead")]
        //public bool VisibileSoloRead { get; set; }

        /// <summary>
        /// Se è true ed è sviluppo orizzontale, significa che sarà
        /// visibile soltanto il valore dell'elemento, ma la parte destra, cioè
        /// quella editabile sarà disabilitata
        /// </summary>
        [XmlElement(ElementName = "ReadOnly")]
        public bool ReadOnly { get; set; }

        /// <summary>
        /// Poichè in alcuni casi non è stato possibile utilizzare 
        /// UrlLoadData, questo metodo si comporta allo stesso modo
        /// con la differenza che il dato viene caricato al momento
        /// del load degli elementi nel metodo CaricaCustomData e non 
        /// richiama un url con la chiamata ajax come fa UrlLoadData, 
        /// ma chiama direttamente un metodo del controller.
        /// </summary>
        [XmlElement(ElementName = "PreLoadSelectListItems")]
        public string PreLoadSelectListItems { get; set; }

        /// <summary>
        /// Funziona un pò come il PreLoadSelectListItems, con la differenza che 
        /// carica solo un testo e non una lista di SelectListItems
        /// </summary>
        [XmlElement(ElementName = "PreLoadData")]
        public string PreLoadData { get; set; }
    }

    [Serializable]
    public enum AttributiAggiuntiviAlign
    {
        None = 0,
        Left = 1,
        Right = 2
    }

    [Serializable]
    public class DataAttributeElement
    {
        public string Nome { get; set; }
        public string Valore { get; set; }
    }

    [Serializable]
    public enum TipologiaAttributoEnum
    {
        Data = 0,
        Testo = 1,
        Numero = 2,
        Select = 3,
        Nota = 4,
        Check = 5,
        Radio = 6,
        Importo = 7,
        FixedHiddenValue = 8,
        Button = 9, // bottone con testo
        ActionButton = 10, // bottone con img
        InLine = 11,
        SelectOption = 12,
        SelectEstesa = 13,
        SelectMultiSelezione = 14,
        SelectMultiSelezioneLibera = 15,
        Switch = 16,
        SelectMultiSelezioneBlank = 17
    }

    [Serializable]
    public class DematerializzazioneCustomDataView : _IdentityData
    {
        //public int IdDoc { get; set; }
        public List<AttributiAggiuntivi> Attributi { get; set; }

        public bool SviluppoOrizzontale { get; set; }

        public string SceltaViewAlternativa { get; set; }
        public List<SelectListItem> ElementiLstProtocolli { get; set; }
        public List<string> CodiceProtocollatore { get; set; }
        public List<TBEmailDirezioni> emailDirezione { get; set; }
        public string CodiceProtocollatoreInModifica { get; set; }
        public string CodiceProtocollatoreInModificaTesto { get; set; }
    }

    [Serializable]
    public class DatiAggiuntivi
    {
        public string Matricola { get; set; }
        public string AssicurazioneIinfortuni { get; set; }
        public string FormaContratto { get; set; }
        public DateTime AnzianitaCategoria { get; set; }
        public string SedeGapp { get; set; }
        public string Categoria { get; set; }
        public string Mansione { get; set; }
        public string Sezione { get; set; }
        public string Servizio { get; set; }
    }

    [Serializable]
    public class AzioneResult
    {
        public bool Esito { get; set; }
        public string DescrizioneErrore { get; set; }
    }

    [Serializable]
    public class AzioneAutomaticaResult: AzioneResult
    {
        public int NuovoStato { get; set; }
        public int IdRichiesta { get; set; }
        public string TxOutput { get; set; }
    }

    [Serializable]
    public class JsonTask
    {
        public string TargetID { get; set; }
        public JsonTaskEventTypes Event { get; set; }
        public List<JsonStepFunction> Functions { get; set; }
    }

    [Serializable]
    public enum JsonTaskEventTypes
    {
        [Display(Name = "click")]
        [DescriptionAttribute("click")]
        CLICK = 0,
        [Display(Name = "change")]
        [DescriptionAttribute("change")]
        CHANGE = 1,
        [Display(Name = "blur")]
        [DescriptionAttribute("blur")]
        BLUR = 2,
        [Display(Name = "dblclick")]
        [DescriptionAttribute("dblclick")]
        DBLCLICK = 3,
        [Display(Name = "focus")]
        [DescriptionAttribute("focus")]
        FOCUS = 4,
        [Display(Name = "focusin")]
        [DescriptionAttribute("focusin")]
        FOCUSIN = 5,
        [Display(Name = "focusout")]
        [DescriptionAttribute("focusout")]
        FOCUSOUT = 6,
        [Display(Name = "keydown")]
        [DescriptionAttribute("keydown")]
        KEYDOWN = 7,
        [Display(Name = "keypress")]
        [DescriptionAttribute("keypress")]
        KEYPRESS = 8,
        [Display(Name = "keyup")]
        [DescriptionAttribute("keyup")]
        KEYUP = 9,
        [Display(Name = "noevent")]
        [DescriptionAttribute("noevent")]
        NOEVENT = 10
    }

    [Serializable]
    public class JsonStepFunction
    {
        public string Name { get; set; }
        public string Arguments { get; set; }
        public string Body { get; set; }
        public string CallerArguments { get; set; }
        private string _body { get; set; }
    }

    [Serializable]
    public class Dematerializzazione_Dettaglio_Mat_Richieste
    {
        public string Titolo { get; set; }
        public List<Dematerializzazione_Dettaglio_Mat_Richieste_Item> Items { get; set; }
    }

    [Serializable]
    public class Dematerializzazione_Dettaglio_Mat_Richieste_Item
    {
        public string Label { get; set; }
        public string Text { get; set; }
    }

    public class Dematerializzazione_EsitoAjax
    {
        public bool Esito { get; set; }
        public string ErrorMessage { get; set; }
        public int Id { get; set; }
    }

    [Serializable]
    public class Dematerializzazione_Config_Tab
    {
        public string NomeTab { get; set; }
        public string NomeAttributo_XR_DEM_DOCUMENTI { get; set; }
        public string OrdineWKF { get; set; }
        public string Tag { get; set; }
        public string NomeAttributo_XR_DEM_WKF_OPERSTATI { get; set; }
        public string Stato_In_XR_DEM_WKF_OPERSTATI { get; set; }
    }

    [Serializable]
    public class Dematerializzazione_Tab_Dettaglio_Pratica
    {
        public string Title { get; set; }
        public string Descrizione { get; set; }
        public string SottoTitolo { get; set; }
        public bool Attivo { get; set; }
        public bool Completato { get; set; }
        public bool Fallito { get; set; }
    }

    [Serializable]
    public class GetDocumentiInBaseAlleTipologieAbilitateResult
    {
        public int Id_Tipo_Doc { get; set; }
        public string Cod_Tipologia_Documentale { get; set; }
        public string Codice_Tipo_Documento { get; set; }
    }

    [Serializable]
    public class CreaDocumentoResponse
    {
        public bool Esito { get; set; }
        public string Errore { get; set; }
        public XR_DEM_DOCUMENTI_EXT Documento { get; set; }
    }

    [Serializable]
    public class CreaDocumentoRequest
    {
        public string MatricolaCreatore { get; set; }
        public List<int> Files { get; set; }
    }

    [Serializable]
    public class CreaDocumentoAssunzioneRequest : CreaDocumentoRequest
    {
        public AssunzioniVM DatiAssunzione { get; set; }
    }

    [Serializable]
    public enum XR_DEM_LOG_STATI_ENUM
    {
        [AmbientValue( "Errore inizializzazione firma")]
        ErroreInitFirma = 5,

        [AmbientValue("Tipo documento non trovato")]
        TipoDocumentoNonTrovato = 10,

        [AmbientValue("Codice protocollatore non trovato")]
        CodiceProtocollatatoreNonTrovato = 15,

        [AmbientValue("Allegato non trovato")]
        AllegatoNonTrovato = 20,

        [AmbientValue("Posizione protocollo non disponibile")]
        PosizioneProtocolloNonDisponibile = 30,

        [AmbientValue("Errore nel reperimento del destinatario")]
        ErroreNelReperimentoDelDestinatario = 40,

        [AmbientValue("Errore nel servizio creazione protocollo")]
        ErroreNelServizioCreazioneProtocollo = 50,

        [AmbientValue("Protocollo creato")]
        ProtocolloCreato = 60,

        [AmbientValue("Errore applicazione protocollo al file")]
        ErroreApplicazioneProtocolloAlFile = 70,

        [AmbientValue("Protocollo applicato al file")]
        ProtocolloApplicatoAlFile = 80,

        [AmbientValue("Credenziali firma non corrette")]
        CredenzialiFirmaNonCorrette = 90,

        [AmbientValue("Posizione firma non disponibile")]
        PosizioneFirmaNonDisponibile = 100,

        [AmbientValue("File non firmato")]
        FileNonFirmato = 110,

        [AmbientValue("Errore generico firma")]
        ErroreGenericoFirma = 120,

        [AmbientValue("File firmato correttamente")]
        FileFirmatoCorrettamente = 130,

        [AmbientValue("Errore in aggiornamento file verso protocollo")]
        ErroreInAggiornamentoFileVersoProtocollo = 140,

        [AmbientValue("Allegato firmato aggiunto al protocollo")]
        AllegatoFirmatoAggiuntoAlProtocollo = 150,

        [AmbientValue("Errore in aggiornamento XR_ALLEGATI")]
        ErroreInAggiornamentoXR_ALLEGATI = 160,

        [AmbientValue("Errore durante la cancellazione del carrello")]
        ErroreDuranteLaCancellazioneDelCarrello = 170,

        [AmbientValue("Errore in invio mail")]
        ErroreInInvioMail = 180,

        [AmbientValue("Mail inviata")]
        MailInviata = 190,

        [AmbientValue("Documento inviato al dipendente")]
        DocumentoInviatoAlDipendente = 191,

        [AmbientValue("Documento protocollato e firmato correttamente")]
        DocumentoProtocollatoEFirmatoCorrettamente = 200
    }

    public class EventoLog
    {
        public DateTime DataEvento { get; set; }
        public string Descrizione { get; set; }
    }

    public class DEM_RicalcoloElementiDaAbilitareResponse
    {
        /// <summary>
        /// Se true deve ricaricare la vista con gli elementi in 
        /// un formato differente da quello standard dove sulla sinistra 
        /// ci saranno i dati attuali e sulla destra gli elementi modificabili
        /// </summary>
        public bool RicaricaVista { get; set; }
        public string Messaggio { get; set; }
        public List<DEM_RicalcoloElementiInVistaResponse_Item> Elementi { get; set; }
    }

    public class DEM_RicalcoloElementiInVistaResponse_Item
    {
        public DEM_RicalcoloElementiInVistaResponse_Item()
        {
            this.Modificabile = true;
        }
        public bool Visibile { get; set; }
        public string NomeElemento { get; set; }
        public bool CampoObbligatorio { get; set; }
        public string ValoreDefault { get; set; }
        public bool Selezionato { get; set; }
        public bool Modificabile { get; set; }
    }

    public enum XR_DEM_TIPI_NOTE_ENUM
    {
        [AmbientValue("Nota creata dall'operatore")]
        NOTAOPERATORE = 1,

        [AmbientValue("Nota creata dall'approvatore")]
        NOTAAPPROVATORE = 2,

        [AmbientValue("Nota creata dall'incaricato alla firma")]
        NOTAFIRMA = 3,

        [AmbientValue("Nota creata dal vistatore")]
        NOTAVISTATORE = 4,

        [AmbientValue("Nota creata dall'incaricato di segreteria")]
        NOTASEGRETERIA = 5
    }

    public class NotaVM
    {
        public string Matricola { get; set; }
        public string Nominativo { get; set; }
        public string Testo { get; set; }
        public string Commento { get; set;}
        public DateTime Data { get; set; }
        public int TipoNota { get; set; }
    }

    public class XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item
    {
        public string MatricolaVistatore { get; set; }
        public string MatricolaApprovatore { get; set; }
        public string MatricolaFirma { get; set; }
        public bool Vistato { get; set; }
        public bool Approvato { get; set; }
        public bool Firmato { get; set; }
        public DateTime? Data { get; set; }
        public XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item_SiglaDetails Sigla { get; set; }
    }

    public class XR_WKF_WORKFLOW_DYNAMIC_STEPS_Item_SiglaDetails
    {
        public bool DaApplicare { get; set; }
        public bool Applicata { get; set; }
        public double PosizioneX { get; set; }
        public double PosizioneY { get; set; }
        public double Lunghezza { get; set; }
        public double Altezza { get; set; }
        public DateTime? DataApplicazione { get; set; }
    }

    public class MultiSelezioneVM
    {
        public string ID { get; set; }
        public string Placeholder { get; set; }
        public string LabelText { get; set; }
        public List<SelectListItem> Elementi { get; set; }
    }
    public class DematerializzazioneBozza
    {
        public bool AdminMode { get; set; }
        public string Codice { get; set; }
        public string Tipo { get; set; }
        public int IdDocumento { get; set; }
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

    [Serializable]
    [XmlRoot(ElementName = "filtri_ricerca")]
    public class FiltriRicerca
    {
        [XmlElement(ElementName = "NumeroProtocollo")]
        public string NumeroProtocollo { get; set; }

        [XmlElement(ElementName = "identificativo")]
        public string Identificativo { get; set; }

        [XmlElement(ElementName = "oggetto")]
        public string Oggetto { get; set; }

        [XmlElement(ElementName = "mittente")]
        public string Mittente { get; set; }

        [XmlElement(ElementName = "destinatario")]
        public string Destinatario { get; set; }

        [XmlElement(ElementName = "destinatario_cc")]
        public string DestinatarioCc { get; set; }

        [XmlElement(ElementName = "data_protocollo_dal")]
        public string DataProtocolloDal { get; set; }

        [XmlElement(ElementName = "data_protocollo_al")]
        public string DataProtocolloAl { get; set; }

        [XmlElement(ElementName = "matricola")]
        public string Matricola { get; set; }

        [XmlElement(ElementName = "sped_destinatario_mail")]
        public string SpedDestinatarioMail { get; set; }

        [XmlElement(ElementName = "sped_data_invio_dal")]
        public string SpedDataInvioDal { get; set; }

        [XmlElement(ElementName = "sped_data_invio_al")]
        public string SpedDataInvioAl { get; set; }

        [XmlElement(ElementName = "sped_indirizzo_mail")]
        public string SpedIndirizzoMail { get; set; }

        [XmlElement(ElementName = "sped_stato")]
        public string SpedStato { get; set; }
    }

    [XmlRoot(ElementName = "Ricerca")]
    public class RicercaProt
    {
        [XmlElement(ElementName = "id_ricerca")]
        public string IdRicerca { get; set; }

        [XmlElement(ElementName = "id_anagrafica")]
        public string IdAnagrafica { get; set; }

        [XmlElement(ElementName = "nome_ricerca")]
        public string NomeRicerca { get; set; }

        [XmlElement(ElementName = "descrizione_ricerca")]
        public string DescrizioneRicerca { get; set; }

        [XmlElement(ElementName = "ricerca_sistema")]
        public string RicercaSistema { get; set; }

        [XmlElement(ElementName = "data_creazione")]
        public string DataCreazione { get; set; }

        [XmlElement(ElementName = "max_record")]
        public string MaxRecord { get; set; }

        [XmlElement(ElementName = "nome_view")]
        public string NomeView { get; set; }

        [XmlElement(ElementName = "tipo_output")]
        public string TipoOutput { get; set; }

        [XmlElement(ElementName = "nome_campi_vis")]
        public string NomeCampiVis { get; set; }

        [XmlElement(ElementName = "desc_campi_vis")]
        public string DescCampiVis { get; set; }

        [XmlElement(ElementName = "tipo_campi_vis")]
        public string TipoCampiVis { get; set; }

        [XmlElement(ElementName = "id_tipo_flusso")]
        public string IdTipoFlusso { get; set; }

        [XmlElement(ElementName = "filtro_sicurezza")]
        public string FiltroSicurezza { get; set; }

        [XmlElement(ElementName = "ordinamento")]
        public string Ordinamento { get; set; }

        [XmlElement(ElementName = "icone_vis")]
        public string IconeVis { get; set; }
    }

    [XmlRoot(ElementName = "Flusso")]
    public class Flusso
    {
        [XmlElement(ElementName = "id_tipo_flusso")]
        public string IdTipoFlusso { get; set; }

        [XmlElement(ElementName = "ids_tipo_ricerca")]
        public string IdsTipoRicerca { get; set; }

        [XmlElement(ElementName = "richiede_profilo")]
        public string RichiedeProfilo { get; set; }

        [XmlElement(ElementName = "id_contesto")]
        public string IdContesto { get; set; }

        [XmlElement(ElementName = "tipo_flusso")]
        public string TipoFlusso { get; set; }

        [XmlElement(ElementName = "nome_vista")]
        public string NomeVista { get; set; }

        [XmlElement(ElementName = "nome_campo_id")]
        public string NomeCampoId { get; set; }

        [XmlElement(ElementName = "pagina_apertura")]
        public string PaginaApertura { get; set; }

        [XmlElement(ElementName = "flag_giorni")]
        public string FlagGiorni { get; set; }

        [XmlElement(ElementName = "flag_storico")]
        public string FlagStorico { get; set; }

        [XmlElement(ElementName = "flag_allegato")]
        public string FlagAllegato { get; set; }

        [XmlElement(ElementName = "flag_estensione")]
        public string FlagEstensione { get; set; }

        [XmlElement(ElementName = "flag_spare")]
        public string FlagSpare { get; set; }

        [XmlElement(ElementName = "flag_allegato_secondario")]
        public string FlagAllegatoSecondario { get; set; }

        [XmlElement(ElementName = "assimila_al_flusso")]
        public string AssimilaAlFlusso { get; set; }

        [XmlElement(ElementName = "flag_paginazione")]
        public string FlagPaginazione { get; set; }

        [XmlElement(ElementName = "table_name")]
        public string TableName { get; set; }

        [XmlElement(ElementName = "id_contatore")]
        public string IdContatore { get; set; }

        [XmlElement(ElementName = "flag_check")]
        public string FlagCheck { get; set; }

        [XmlElement(ElementName = "tabella_storico")]
        public string TabellaStorico { get; set; }

        [XmlElement(ElementName = "nome_vista_campi")]
        public string NomeVistaCampi { get; set; }

        [XmlElement(ElementName = "flag_propaga_set")]
        public string FlagPropagaSet { get; set; }

        [XmlElement(ElementName = "flag_propaga_remove")]
        public string FlagPropagaRemove { get; set; }

        [XmlElement(ElementName = "flag_delete")]
        public string FlagDelete { get; set; }

        [XmlElement(ElementName = "flag_modifica")]
        public string FlagModifica { get; set; }

        [XmlElement(ElementName = "flag_form_cust")]
        public string FlagFormCust { get; set; }

        [XmlElement(ElementName = "flag_storico_seq")]
        public string FlagStoricoSeq { get; set; }

        [XmlElement(ElementName = "nome_campo_id_parent")]
        public string NomeCampoIdParent { get; set; }
    }

    [XmlRoot(ElementName = "Record")]
    public class Record
    {
        [XmlElement(ElementName = "identificativo")]
        public string Identificativo { get; set; }

        [XmlElement(ElementName = "data_protocollo")]
        public string DataProtocollo { get; set; }

        [XmlElement(ElementName = "mittente")]
        public string Mittente { get; set; }

        [XmlElement(ElementName = "destinatari")]
        public string Destinatari { get; set; }

        [XmlElement(ElementName = "oggetto")]
        public string Oggetto { get; set; }

        [XmlElement(ElementName = "richiedente")]
        public string Richiedente { get; set; }

        [XmlElement(ElementName = "nominativo_richiedente")]
        public string NominativoRichiedente { get; set; }

        [XmlElement(ElementName = "id_documento")]
        public string IdDocumento { get; set; }

        [XmlElement(ElementName = "id_documento1")]
        public string IdDocumento1 { get; set; }
    }

    [XmlRoot(ElementName = "Recordset")]
    public class Recordset
    {
        [XmlElement(ElementName = "Record")]
        public Record Record { get; set; }

        [XmlAttribute(AttributeName = "campi")]
        public string Campi { get; set; }

        [XmlAttribute(AttributeName = "labels")]
        public string Labels { get; set; }

        [XmlAttribute(AttributeName = "icone")]
        public string Icone { get; set; }

        [XmlAttribute(AttributeName = "tipi")]
        public string Tipi { get; set; }

        [XmlAttribute(AttributeName = "numerorecord")]
        public string Numerorecord { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "Errore")]
    public class ErroreProt
    {
        [XmlAttribute(AttributeName = "id_errore")]
        public string IdErrore { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    [XmlRoot(ElementName = "eseguiRicerca")]
    public class EseguiRicerca
    {
        [XmlElement(ElementName = "Ricerca")]
        public RicercaProt Ricerca { get; set; }

        [XmlElement(ElementName = "Flusso")]
        public Flusso Flusso { get; set; }

        [XmlElement(ElementName = "Recordset")]
        public Recordset Recordset { get; set; }

        [XmlElement(ElementName = "Errore")]
        public ErroreProt Errore { get; set; }

        [XmlAttribute(AttributeName = "titolo")]
        public string Titolo { get; set; }

        [XmlText]
        public string Text { get; set; }
    }

    public class TBEmailDirezioni
    {
        public string Alias { get; set; }
        public string Email { get; set; }
    }
}