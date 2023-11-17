using myRaiData.Incentivi;
using myRaiHelper;
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
        Bozza = 5,

        [Display(Name = "Richiesta creata")]
        [DescriptionAttribute("Richiesta creata")]
        RichiestaCreata = 10,

        [Display(Name = "Documento pronto alla visione")]
        [DescriptionAttribute("Documento pronto alla visione")]
        ProntoVisione = 20,

        [Display(Name = "Posizione del protocollo impostata")]
        [DescriptionAttribute("Posizione del protocollo impostata")]
        ProtocolloPosizionato = 25,

        [Display(Name = "Posizione del protocollo modificata")]
        [DescriptionAttribute("Posizione del protocollo modificata")]
        ModificaPosizioneProtocollo = 26,

        [Display(Name = "Documento modificato")]
        [DescriptionAttribute("Documento modificato")]
        DocumentoModificato = 28,

        [Display(Name = "Documento visionato")]
        [DescriptionAttribute("Documento visionato")]
        Visionato = 30,

        [Display(Name = "Accettato e/o Protocollato")]
        [DescriptionAttribute("Accettato e/o Protocollato")]
        Accettato = 40,

        [Display(Name = "Protocollato")]
        [DescriptionAttribute("Protocollato")]
        Protocollato = 45,

        [Display(Name = "Documento firmato digitalmente")]
        [DescriptionAttribute("Documento firmato digitalmente")]
        FirmatoDigitalmente = 50,

        [Display(Name = "Inviato al dipendente")]
        [DescriptionAttribute("Inviato al dipendente")]
        InviatoAlDipendente = 60,

        [Display(Name = "Accettato dal dipendente")]
        [DescriptionAttribute("Accettato dal dipendente")]
        AccetttatoDalDipendente = 70,

        [Display(Name = "Rifiutato dal dipendente")]
        [DescriptionAttribute("Rifiutato dal dipendente")]
        RifiutatoDalDipendente = 80,

        [Display(Name = "Rifiutato dall'approvatore")]
        [DescriptionAttribute("Rifiutato dall'approvatore")]
        RifiutoApprovatore = 90,

        [Display(Name = "Presa in carico")]
        [DescriptionAttribute("Presa in carico")]
        PresaInCarico = 100,

        [Display(Name = "Rifiutato in firma")]
        [DescriptionAttribute("Rifiutato in firma")]
        RifiutatoFirma = 110,

        [Display(Name = "Pratica conclusa")]
        [DescriptionAttribute("Pratica conclusa")]
        PraticaConclusa = 120,

        [Display(Name = "Pratica cancellata")]
        [DescriptionAttribute("Pratica cancellata")]
        PraticaCancellata = 200
    }

    public enum DEM_TIPOLOGIE_DOCUMENTALI_ENUM
    {
        EXTRUO = 1,
        VSDIP = 2,
        VSEXT = 3,
        VSRUO = 4
    }

    public enum DEM_TIPI_DOCUMENTO_ENUM
    {
        [Display(Name = "Assegnazioni definitive e temporanee")]
        [DescriptionAttribute("Assegnazioni definitive e temporanee")]
        ADT = 1,
        [Display(Name = "Trasferimenti definitivi e temporanei")]
        [DescriptionAttribute("Trasferimenti definitivi e temporanei")]
        TDT = 2,
        [Display(Name = "Distacchi Consociate")]
        [DescriptionAttribute("Distacchi Consociate")]
        DC = 3,
        [Display(Name = "Lettere di trasmissione/comunicazione alle consociate")]
        [DescriptionAttribute("Lettere di trasmissione/comunicazione alle consociate")]
        LT = 4,
        [Display(Name = "Riconoscimenti part-time ed eventuali variazioni")]
        [DescriptionAttribute("Riconoscimenti part-time ed eventuali variazioni")]
        RPT = 5,
        [Display(Name = "Aspettative")]
        [DescriptionAttribute("Aspettative")]
        ASP = 6,
        [Display(Name = "Cambi qualifica")]
        [DescriptionAttribute("Cambi qualifica")]
        CQ = 7,
        [Display(Name = "Provvedimenti di politiche retributive")]
        [DescriptionAttribute("Provvedimenti di politiche retributive")]
        PPR = 8,
        [Display(Name = "Assegnazioni livello da vertenza sindacale")]
        [DescriptionAttribute("Assegnazioni livello da vertenza sindacale")]
        ALVS = 9,
        [Display(Name = "Comporti")]
        [DescriptionAttribute("Comporti")]
        C = 10,
        [Display(Name = "Attribuzioni di responsabilità da Disposizione organizzativa")]
        [DescriptionAttribute("Attribuzioni di responsabilità da Disposizione organizzativa")]
        ARDO = 11,
        [Display(Name = "Assegnazioni Indennità contrattuale")]
        [DescriptionAttribute("Assegnazioni Indennità contrattuale")]
        AIC = 12,
        [Display(Name = "Dichiarazioni di servizio e Varianti")]
        [DescriptionAttribute("Dichiarazioni di servizio e Varianti")]
        DSERV = 13,
        [Display(Name = "Dichiarazioni agli eredi in caso di decessi")]
        [DescriptionAttribute("Dichiarazioni agli eredi in caso di decessi")]
        DERE = 14,
        [Display(Name = "Provvedimenti esecuzioni sentenze")]
        [DescriptionAttribute("Provvedimenti esecuzioni sentenze")]
        PES = 15,
        [Display(Name = "Comunicazioni relative all’accesso ai dati")]
        [DescriptionAttribute("Comunicazioni relative all’accesso ai dati")]
        CRAD = 16,
        [Display(Name = "Provvedimenti per addebito multe")]
        [DescriptionAttribute("Provvedimenti per addebito multe")]
        PAM = 17,
        [Display(Name = "Altri provvedimenti")]
        [DescriptionAttribute("Altri provvedimenti")]
        AP = 18,
        [Display(Name = "Assunzioni")]
        [DescriptionAttribute("Assunzioni")]
        ASS = 19,
        [Display(Name = "Cessazioni")]
        [DescriptionAttribute("Cessazioni")]
        CESS = 20,
        [Display(Name = "Conferme in servizio da contenzioso")]
        [DescriptionAttribute("Conferme in servizio da contenzioso")]
        CSC = 21,
        [Display(Name = "Reintegrazioni in servizio")]
        [DescriptionAttribute("Reintegrazioni in servizio")]
        RIS = 22,
        [Display(Name = "Autorizzazioni attività extra-rai")]
        [DescriptionAttribute("Autorizzazioni attività extra-rai")]
        AAE = 23
    }

    public enum WKF_TIPOLOGIA_ENUM
    {
        DEMDOC_VSDIP = 8,
        DEMDOC_VSDIP_NODIC = 9,
        DEMDOC_VSRUO = 10,
        DEMDOC_VSRUO_C = 11,
        DEMDOC_VSRUO_CON = 12,
        DEMDOC_VSEXT = 16,
        DEMDOC_EXTRUO = 17
    }

    public class ListaDematerializzazione : BaseAnagrafica
    {
        public List<XR_RICHIESTE> Richieste { get; set; }
    }

    public class InsRicModel : _IdentityData
    {
        public WKF_TIPOLOGIA_ENUM TipologiaWKF { get; set; }
        public DEM_TIPOLOGIE_DOCUMENTALI_ENUM TipologiaDocumentale { get; set; }
        public DEM_TIPI_DOCUMENTO_ENUM TipologiaDocumento { get; set; }
        public string MatricolaDestinatario { get; set; }
        public int IdPersonaDestinatario { get; set; }
        public string Descrizione { get; set; }
        public string Note { get; set; }
        public string IncaricatoFirma { get; set; }
        public string MatricolaApprovatore { get; set; }
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
    public class destClass
    {
        public string destinatario { get; set; }
    }

    [Serializable]
    public class protocollo
    {
        public protocollo()
        {
            tipo_protocollo = 2;
        }
        public int tipo_protocollo { get; set; }
        public string oggetto { get; set; }
        public string mittente { get; set; }

        [XmlElement("destinatari", typeof(destClass))]
        public List<destClass> destinatari { get; set; }
        public string data_spedizione { get; set; }
    }

    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
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
    }

    public class Dematerializzazione_FiltriApprovatore
    {
        public string MatricolaONominativo { get; set; }
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
    }

    public class Dematerializzazione_TRFileUploadVM
    {
        public List<XR_ALLEGATI> Allegati { get; set; }
        public bool Principale { get; set; }
        public bool InModifica { get; set; }
    }

    public class RiepilogoVM
    {
        public RichiestaDoc Documento { get; set; }
        public string NominativoUtenteApprovatore { get; set; }
        public string NominativoUtenteCreatore { get; set; }
        public string NominativoUtenteDestinatario { get; set; }
        public string NominativoUtenteIncaricatoFirma { get; set; }
        public string NominativoUtenteIncaricato { get; set; }
    }

    public class Dem_ModificaDocumentoVM : InsRicModel
    {
        public RichiestaDoc Richiesta { get; set; }
        public string NominativoUtenteApprovatore { get; set; }
        public string NominativoUtenteCreatore { get; set; }
        public string NominativoUtenteDestinatario { get; set; }
        public string NominativoUtenteFirma { get; set; }
        public string NominativoUtenteIncaricato { get; set; }
        public XR_DEM_TIPIDOC_COMPORTAMENTO Comportamento { get; set; }
        public string WKF_TIPOLOGIA { get; set; }
        public DematerializzazioneCustomDataView ModelDatiAggiuntivi { get; set; }

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
    }

    public class DematerializzazioneDocumentiInPartenzaVM : _IdentityData
    {
        public bool IsOperatore { get; set; }
        public bool IsApprovatore { get; set; }
        public bool IsVisionatore { get; set; }
        public bool IsPreview { get; set; }
        public Dematerializzazione_FiltriApprovatore Filtri { get; set; }
    }

    public enum DEM_TIPO_RIFIUTATI
    {
        ALL = 1,
        RifiutateInApprovazione = 2,
        RifiutateInFirma = 3,
        RifiutateDalDipendente = 4
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

        [XmlElement(ElementName = "Label")]
        public string Label { get; set; }

        [XmlElement(ElementName = "Title")]
        public string Title { get; set; }

        [XmlElement(ElementName = "Nome")]
        public string Nome { get; set; }

        [XmlElement(ElementName = "Testo")]
        public string Testo { get; set; }

        [XmlElement(ElementName = "Azioni")]
        public List<Action> Azioni { get; set; }

        [XmlElement(ElementName = "Ordinamento")]
        public int Ordinamento { get; set; }

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

        [XmlElement(ElementName = "SelectListItems")]
        public List<SelectListItem> SelectListItems { get; set; }

        [XmlElement(ElementName = "UrlLoadData")]
        public string UrlLoadData { get; set; }

        [XmlElement(ElementName = "MinLength")]
        public int MinLength { get; set; }

        [XmlElement(ElementName = "MaxLength")]
        public int MaxLength { get; set; }
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
        Importo = 7
    }

    [Serializable]
    public class Action
    {
        /// <summary>
        /// Es. onchange.. onkeypress...
        /// </summary>
        public string Metodo { get; set; }
        /// <summary>
        /// Funzione js
        /// </summary>
        public string Funzione { get; set; }
    }

    public class DematerializzazioneCustomDataView : _IdentityData
    {
        //public int IdDoc { get; set; }
        public List<AttributiAggiuntivi> Attributi { get; set; }
    }

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
}