using myRaiData.Incentivi;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace myRaiCommonModel
{
    public enum IbanType
    {
        [Description("NonDefinito")]
        [AmbientValue("Non definito")]
        NonDefinito = 0,
        [Description("AccreditoStipendio")]
        [AmbientValue("Accredito stipendio")]
        AccreditoStipendio = 1,
        [Description("AnticipoTrasferte")]
        [AmbientValue("Anticipo trasferte")]
        AnticipoTrasferte = 2,
        [Description("AnticipoSpese")]
        [AmbientValue("Anticipo spese di produzione")]
        AnticipoSpese = 3,
        [Description("Anticipi")]
        [AmbientValue("Tutti gli anticipi")]
        Anticipi = 4

    }

    public enum IndirizzoType
    {
        Residenza,
        Domicilio
    }

    public enum SezioniAnag
    {
        NonDefinito,
        Anagrafici,
        Residenza,
        Domicilio,
        TitoliStudio,
        Bancari,
        StatoRapporto,
        TipoContratti,
        Sedi,
        Servizi,
        Qualifiche,
        Ruoli,
        Retribuzione,
        Debitoria,
        Formazione,
        Struttura,
        Presenze,
        Contenzioso,
        Sezioni,
        MieiDoc, // sezione miei documenti
    }

    public enum TipoEvento
    {
        Contratto,
        Sede,
        Servizio,
        Qualifica,
        Mansione,
        Stato,
        Sezione
    }

    public enum TipoAnagEcc
    {
        DaGapp = 1,
        Pianificato = 2,
        Standard = 3
    }

    public enum TipoRichiestaAnag
    {
        Tutte,
        [AmbientValue("Variazione IBAN")]
        IBAN,
        [AmbientValue("Congedo")]
        Congedo,
        Dematerializzazione,
        [AmbientValue("Variazione contrattuale")]
        VariazioneContrattuale
    }

    public enum TipoGestioneAnag
    {
        Cessazione,
        ProvvRetr,
        Valutazione,
        Dematerializzazione
    }

    public enum TipoAnaVar
    {
        Anagrafica,
        Domicilio,
        Immatricolazione
    }

    public class AnaVar
    {
        public AnaVar()
        {
            ForzaCF = "#";
        }
        public int IdEvento { get; set; }
        public string Matricola { get; set; }
        public string Nominativo { get; set; }
        public DateTime? DtaNascita { get; set; }
        public string Sesso { get; set; }
        public string StatoCivile { get; set; }
        public string ForzaCF { get; set; }
        public string CodCittad { get; set; }
        public string MatColl { get; set; }
        public string CF { get; set; }
        public string CittaNasc { get; set; }
        public string ProvNasc { get; set; }
        public string IndirizzoDom { get; set; }
        public string CittaDom { get; set; }
        public string ProvDom { get; set; }
        public string Casagit { get; set; }
        public string TitoloOnor { get; set; }
        public DateTime DtaInizio { get; set; }
        public string CodUser { get; set; }
        public DateTime Timestamp { get; set; }
        public string CapDom { get; set; }
    }

    //Spostate in RichiesteModel.cs
    //public class RichiestaLoader
    //{
    //    public RichiestaLoader()
    //    {
    //        Tipologie = new List<TipoRichiestaAnag>();
    //    }
    //    public List<TipoRichiestaAnag> Tipologie { get; set; }
    //    public string Matricola { get; set;}
    //}

    //public class RichiestaAnag : _IdentityData
    //{
    //    public int IdRichiesta { get; set; }
    //    public TipoRichiestaAnag Tipologia { get; set; }
    //    public DateTime DataRichiesta { get; set; }
    //    public string Descrizione { get; set; }
    //    public string Note { get; set; }
    //    public object ObjInfo { get; set; }
    //    public DateTime? DataMemo { get; set; }
    //    public DateTime? DataScadenza { get; set; }
    //    public bool HasError { get; set; }
    //    public string ErrorMsg { get; set; }
    //    public DEM_TIPI_DOCUMENTO_ENUM TipologiaDoc { get; set; }
    //}

    public class GestioneAnag : _IdentityData
    {
        public int IdGestione { get; set; }
        public TipoGestioneAnag Tipologia { get; set; }
        public string Descrizione { get; set; }
        public object ObjInfo { get; set; }

        public bool HasError { get; set; }
        public string ErrorMsg { get; set; }
    }

    public class SezListLoadOption
    {
        public SezListLoadOption(SezioniAnag sezione, bool asyncLoad = false)
        {
            Sezione = sezione;
            AsyncLoad = asyncLoad;
        }

        public SezioniAnag Sezione { get; set; }
        public bool AsyncLoad { get; set; }
    }

    public class AnagraficaLoader
    {
        private void Init()
        {
            Sezioni = new List<SezioniAnag>();
            SezioniAsync = new List<SezioniAnag>();

            EnabledAdd = true;
            EnabledDelete = true;
            EnabledModify = true;

            Parametri = new List<myRaiDataTalentia.XR_HRIS_PARAM>();
        }

        public AnagraficaLoader(int anno, params SezioniAnag[] sezList)
        {
            Init();

            Anno = anno;
            Sezioni.AddRange(sezList);
        }

        public AnagraficaLoader(params SezioniAnag[] sezList)
        {
            Init();
            Sezioni.AddRange(sezList);
        }

        public AnagraficaLoader(params SezListLoadOption[] sezListLoadOptions)
        {
            Init();
            Sezioni.AddRange(sezListLoadOptions.Where(x => !x.AsyncLoad).Select(x => x.Sezione));
            SezioniAsync.AddRange(sezListLoadOptions.Where(x => x.AsyncLoad).Select(x => x.Sezione));
        }

        public List<myRaiDataTalentia.XR_HRIS_PARAM> Parametri { get; set; }

        public int Anno { get; set; }
        public DateTime? CarrieraInizio { get; set; }
        public DateTime? CarrieraFine { get; set; }
        public List<SezioniAnag> Sezioni { get; set; }
        public List<SezioniAnag> SezioniAsync { get; set; }
        public bool EnabledAdd { get; set; }
        public bool EnabledModify { get; set; }
        public bool EnabledDelete { get; set; }
    }

    public class AnagraficaSettings
    {
        public const int MAX_IBAN_NUM = 3;
    }

    public class _IdentityData
    {
        public int IdPersona { get; set; }
        public string Matricola { get; set; }
        public string Nominativo { get; set; }

        public DateTime LastModifiedTime { get; set; }
    }

    public class BaseAnagrafica : _IdentityData
    {
        public string Nome { get; set; }
        public string Cognome { get; set; }

        public DateTime DataAssunzione { get; set; }
        public DateTime DataCessazione { get; set; }
        public string TipoContratto { get; set; }
        public string CodSede { get; set; }
        public string Sede { get; set; }
        public string CodServizio { get; set; }
        public string Servizio { get; set; }
        public string CodQualifica { get; set; }
        public string Qualifica { get; set; }

    }

    public class AnagraficaModel : BaseAnagrafica
    {
        public AnagraficaModel()
        {
            DatiAnagrafici = new AnagraficaDatiAnag();
            DatiResidenzaDomicilio = new AnagraficaResDom();
            DatiTitoliStudio = new AnagraficaTitoliStudio();
            DatiBancari = new AnagraficaIban();
            DatiContratti = new AnagraficaContratti();
            DatiStatiRapporti = new AnagraficaStatiRapporti();
            DatiSedi = new AnagraficaSedi();
            DatiSezioni = new AnagraficaSezioni();
            DatiServizi = new AnagraficaServizi();
            DatiQualifiche = new AnagraficaQualifiche();
            DatiRuoli = new AnagraficaRuoli();
            DatiRedditi = new AnagraficaRedditi();
            DatiSituazioneDebitoria = new AnagraficaSitDebit();
            DatiFormazione = new AnagraficaFormazione();
            DatiStruttOrg = new AnagraficaStruttOrg();
            DatiPresenze = new AnagraficaPresenze();
            DatiContenzioso = new AnagraficaContenzioso();

            IsNeoMatr = false;

            ViewInfo = new string[] { "matricola", "data_assunzione", "sede", "servizio" };
        }

        public AnagraficaDatiAnag DatiAnagrafici { get; set; }
        public AnagraficaResDom DatiResidenzaDomicilio { get; set; }
        public AnagraficaTitoliStudio DatiTitoliStudio { get; set; }
        public AnagraficaIban DatiBancari { get; set; }
        public AnagraficaContratti DatiContratti { get; set; }
        public AnagraficaStatiRapporti DatiStatiRapporti { get; set; }
        public AnagraficaSedi DatiSedi { get; set; }
        public AnagraficaSezioni DatiSezioni { get; set; }
        public AnagraficaServizi DatiServizi { get; set; }
        public AnagraficaQualifiche DatiQualifiche { get; set; }
        public AnagraficaRuoli DatiRuoli { get; set; }
        public AnagraficaRedditi DatiRedditi { get; set; }
        public AnagraficaSitDebit DatiSituazioneDebitoria { get; set; }
        public AnagraficaFormazione DatiFormazione { get; set; }
        public AnagraficaStruttOrg DatiStruttOrg { get; set; }
        public AnagraficaPresenze DatiPresenze { get; set; }

        public AnagraficaContenzioso DatiContenzioso { get; set; }

        public AnagraficaDematerializzazioneDocumenti DematerializzazioneMieiDocumenti { get; set; }

        public bool ActionToAnagrafica { get; set; }
        public bool ActionState { get; set; }
        public bool IsNeoMatr { get; set; }

        public string CodErrorMsg { get; set; }
        public string ErrorMsg { get; set; }
        public string Sesso { get; set; }

        /// <summary>
        /// Elenco delle informazioni da mostrare in Header_DatiDipendente.
        /// I valori possibili sono matricola,data_assunzione,servizio,sede,qualifica.
        /// Il default è matricola,data_assunzione,servizio,sede.
        /// </summary>
        public string[] ViewInfo { get; set; }
        public string Sezione { get; set; }
    }


    public class BaseAnagraficaData : _IdentityData
    {
        public BaseAnagraficaData()
        {
            CanModify = false;
            CanDelete = false;
            CanAdd = false;
            IsEnabled = false;
        }
        public bool CanModify { get; set; }
        public bool CanDelete { get; set; }
        public bool CanAdd { get; set; }
        public bool IsEnabled { get; set; }
        public int IdRichiesta { get; set; }
    }
    public class BaseEventiContainerModel : BaseAnagraficaData
    {
        public BaseEventiContainerModel() : base()
        {
            Eventi = new List<EventoModel>();
        }
        public List<EventoModel> Eventi { get; set; }

        public string ModifyAction { get; set; }

        //Per il disegno
        public TipoEvento Tipologia { get; set; }
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
    }

    public class AnagraficaDatiAnag : BaseAnagraficaData
    {
        public AnagraficaDatiAnag() : base()
        {
        }
        public AnagraficaDatiAnag(ANAGPERS dbRec):base()
        {
            Cognome = dbRec.DES_COGNOMEPERS;
            Nome = dbRec.DES_NOMEPERS;
            CodiceFiscale = dbRec.CSF_CFSPERSONA;
            Sesso = dbRec.COD_SESSO;
            DataNascita = dbRec.DTA_NASCITAPERS;
            CodLuogoNascita = dbRec.COD_CITTA;
            Nominativo = Cognome + " " + Nome;
        }

        public string Nome { get; set; }
        public string Cognome { get; set; }
        [DisplayName("Secondo cognome")]
        public string SecondoCognome { get; set; }
        public string Sesso { get; set; }

        [DisplayName("Data di nascita")]
        [DataType(DataType.Date)]
        public DateTime DataNascita { get; set; }

        [DisplayName("Luogo di nascita")]
        public string CodLuogoNascita { get; set; }
        public string LuogoNascita { get; set; }

        [DisplayName("Cittadinanza")]
        public string CodCittadinanza { get; set; }
        public string Cittadinanza { get; set; }

        [DisplayName("Stato civile")]
        public string CodStatoCivile { get; set; }
        public string StatoCivile { get; set; }

        [DisplayName("Codice fiscale")]
        public string CodiceFiscale { get; set; }
    }
    public class AnagraficaResDom : BaseAnagraficaData
    {
        public AnagraficaResDom() : base()
        {
            Residenza = new IndirizzoModel();
            Domicilio = new IndirizzoModel();
        }
        public IndirizzoModel Residenza { get; set; }
        public IndirizzoModel Domicilio { get; set; }
    }
    public class AnagraficaTitoliStudio : BaseAnagraficaData
    {
        public AnagraficaTitoliStudio() : base()
        {
            Studi = new List<StudioModel>();
        }

        public List<StudioModel> Studi { get; set; }
    }
    public class AnagraficaIban : BaseAnagraficaData
    {
        public AnagraficaIban() : base()
        {
            Ibans = new List<IbanModel>();
            IbanLiberi = new List<IbanType>();
        }
        public List<IbanModel> Ibans { get; set; }
        public List<IbanType> IbanLiberi { get; set; }
    }
    public class AnagraficaContratti : BaseEventiContainerModel
    {

    }
    public class AnagraficaStatiRapporti : BaseEventiContainerModel
    {

    }
    public class AnagraficaSedi : BaseEventiContainerModel
    {
        public AnagraficaSedi() : base()
        {
            Tipologia = TipoEvento.Sede;
        }
    }
    public class AnagraficaSezioni : BaseEventiContainerModel
    {
        public AnagraficaSezioni() : base()
        {
            Tipologia = TipoEvento.Sezione;
        }
    }
    public class AnagraficaServizi : BaseEventiContainerModel
    {
        public AnagraficaServizi() : base()
        {
            Tipologia = TipoEvento.Servizio;
        }
    }
    public class AnagraficaQualifiche : BaseEventiContainerModel
    {
        public AnagraficaQualifiche() : base()
        {
            Tipologia = TipoEvento.Qualifica;
        }
    }
    public class AnagraficaRuoli : BaseEventiContainerModel
    {
        public AnagraficaRuoli() : base()
        {
            Tipologia = TipoEvento.Mansione;
        }
    }
    public class AnagraficaRedditi : BaseAnagraficaData
    {
        public AnagraficaRedditi() : base()
        {
            Redditi = new List<RedditoModel>();
        }

        public List<RedditoModel> Redditi { get; set; }
    }
    public class AnagraficaSitDebit : BaseAnagraficaData
    {
        public AnagraficaSitDebit() : base()
        {
            Dati = new List<SitDebitModel>();
        }
        public List<SitDebitModel> Dati { get; set; }
    }
    public class AnagraficaFormazione : BaseAnagraficaData
    {
        public AnagraficaFormazione() : base()
        {
            CorsiFatti = new List<CorsoFormazione>();
        }

        public List<CorsoFormazione> CorsiFatti { get; set; }
    }
    public class AnagraficaStruttOrg : BaseAnagraficaData
    {
        public AnagraficaStruttOrg() : base()
        {
            Sezioni = new List<SezioneModel>();
            Incarichi = new List<IncaricoModel>();
        }

        public List<SezioneModel> Sezioni { get; set; }
        public List<IncaricoModel> Incarichi { get; set; }
    }
    public class AnagraficaPresenze : BaseAnagraficaData
    {
        public AnagraficaPresenze() : base()
        {
            Eccezioni = new List<AnagEcc>();
            Giornate = new List<AnagGiornata>();
        }

        public int Anno { get; set; }
        public List<AnagEcc> Eccezioni { get; set; }
        public List<AnagGiornata> Giornate { get; set; }
    }
    public class AnagraficaDatiContr : BaseAnagraficaData
    {
        public AnagraficaDatiContr() : base()
        {
        }

        public string CodServizio { get; set; }
        public string CodSezione { get; set; }
        public string CodSede { get; set; }

        public EventoModel Servizio { get; set; }
        public EventoModel Sezione { get; set; }
        public EventoModel Sede { get; set; }
        public string CodEventoServizio { get; set; }
        public string CodEventoSede { get; set; }
        public string CodEventoSezione { get; set; }
    }

    public class AnagraficaContenzioso : BaseAnagraficaData
    {
        public AnagraficaContenzioso()
        {
            this.Cause = new List<Causa>();
            this.Provvedimenti = new List<ProvvedimentoExt>();
        }

        public int CauseAperte { get; set; }

        public int ProvvedimentiAperti { get; set; }

        public int CauseChiuse { get; set; }

        public int ProvvedimentiChiusi { get; set; }

        public List<Causa> Cause { get; set; }

        public List<ProvvedimentoExt> Provvedimenti { get; set; }
    }


    public class AnagraficaDematerializzazioneDocumenti : BaseAnagraficaData
    {
        public AnagraficaDematerializzazioneDocumenti()
        {
            this.Documenti = new List<XR_DEM_DOCUMENTI_EXT>();
        }

        public List<XR_DEM_DOCUMENTI_EXT> Documenti { get; set; }
    }

    public class ProvvedimentoExt : Provvedimento
    {
        public string Durata { get; set; }
        public string Stato { get; set; }
    }

    public class IbanModel : BaseAnagraficaData
    {
        public IbanModel()
        {

        }

        public IbanModel(XR_DATIBANCARI dbRec)
        {
            Tipologia = IbanType.NonDefinito;

            IdDatiBancari = dbRec.ID_XR_DATIBANCARI;
            IdPersona = dbRec.ID_PERSONA;
            IBAN = dbRec.COD_IBAN;
            Intestatario = dbRec.DES_INTESTATARIO;
            if (CezanneHelper.GetAnagBanca(dbRec.COD_ABI, dbRec.COD_CAB, true, out XR_ANAGBANCA anagBanca))
            {
                IndirizzoAgenzia = (anagBanca.DES_INDIRIZZO.Trim() + " - " + (anagBanca.TB_COMUNE != null ? anagBanca.TB_COMUNE.DES_CITTA : "")).TitleCase();
                Agenzia = anagBanca.DES_RAG_SOCIALE;
            }
            else
            {
                Agenzia = "-";
                IndirizzoAgenzia = "-";
            }
            DataInizio = dbRec.DTA_INIZIO;
            DataFine = dbRec.DTA_FINE.GetValueOrDefault();
            LastModifiedTime = dbRec.TMS_TIMESTAMP.GetValueOrDefault();
        }
        public int IdDatiBancari { get; set; }
        public IbanType Tipologia { get; set; }
        public string IBAN { get; set; }
        public string Intestatario { get; set; }
        public string Agenzia { get; set; }
        public string IndirizzoAgenzia { get; set; }
        public string Vincoli { get; set; }

        public bool IndCongelato { get; set; }
        public bool IndVincoli { get; set; }

        public DateTime DataInizio { get; set; }
        public DateTime DataFine { get; set; }
        public DateTime DataModifica { get; set; }

        public int IdRichiestaMod { get; set; }
        public string OperazioneRichiesta { get; set; }
        public IbanModel DatiRichiesta { get; set; }

        public List<IbanType> IbanLiberi { get; set; }
    }
    public class EventoModel : _IdentityData
    {
        public EventoModel() : base()
        {
            MaxDate = new DateTime(2999, 12, 31);
            Principale = true;
        }
        public int IdEvento { get; set; }
        public string Codice { get; set; }
        public string Descrizione { get; set; }
        public string CodiceEvento { get; set; }
        public string DescrizioneEvento { get; set; }
        public bool Principale { get; set; }
        public DateTime DataInizio { get; set; }
        public DateTime DataFine { get; set; }
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
        public TipoEvento Tipo { get; set; }

        public string TipologiaAccordo { get; set; }
        public DateTime? NotificaDipendente { get; set; }
        public DateTime? NotificaEnte { get; set; }

        public DateTime? ValiditaInizio { get; set; }
        public DateTime? ValiditaFine { get; set; }
        public int? IdEventoPrec { get; set; }

        /// <summary>
        /// Servizio per cambio sezione - la sezione se cambiata singolarmente può essere modificata solo nell'ambito dello stesso servizio
        /// </summary>
        public string CodiceRif { get; set; }

        /// <summary>
        /// Sezione per cambio servizio - la modifica del servizio implica un cambio sezione
        /// </summary>
        public string CodiceSec { get; set; }

        public int IdRichiesta { get; set; }
    }
    public class StudioModel : _IdentityData
    {
        public StudioModel()
        {
            DataInizio = new DataModel();
            DataFine = new DataModel();
        }
        public DataModel DataInizio { get; set; }
        public DataModel DataFine { get; set; }

        public string DataInizioStr { get; set; }
        public string DataFineStr { get; set; }

        public bool IsSpecializzazione { get; set; }

        public string CodTitolo { get; set; }
        public string DesTitolo { get; set; }

        public short CodTipoTitolo { get; set; }
        public string DesTipoTitolo { get; set; }

        public string TitoloTesi { get; set; }
        [MaxLength(250)]
        public string CorsoLaurea { get; set; }

        public string CodIstituto { get; set; }
        [MaxLength(250)]
        public string Istituto { get; set; }

        public string Voto { get; set; }
        public string Scala { get; set; }

        public string Cod_TipoPunteggio { get; set; }
        public string Cod_PunteggioMax { get; set; }

        public string CodCitta { get; set; }
        public string DesCitta { get; set; }

        public bool Lode { get; set; }
        [MaxLength(250)]
        public string Riconoscimento { get; set; }
        [MaxLength(255)]
        public string Nota { get; set; }

        /*Proprietà per operazioni CRUD*/
        public string FormAction { get; set; }
        public string FormController { get; set; }
        public string SubmitAction { get; set; }
        /*Prop discriminante per Piano Formativo*/
        public string idTitoloStudioLocal { get; set; }
    }
    public class IndirizzoModel : BaseAnagraficaData
    {
        public IndirizzoModel():base()
        {

        }
        public IndirizzoModel(RESIDENZA recRes):base()
        {
            LoadFromDB(recRes);
        }

        public void LoadFromDB(RESIDENZA recRes)
        {
            Tipologia = IndirizzoType.Residenza;
            IdPersona = recRes.ID_PERSONA;
            Decorrenza = recRes.DTA_INIZIO;
            DataFine = recRes.DTA_FINE;
            CodCitta = recRes.COD_CITTA;
            Indirizzo = recRes.DES_INDIRRESID.TitleCase();
            Citta = String.Format("{0}, {1}", recRes.TB_COMUNE.DES_CITTA.TitleCase(), recRes.TB_COMUNE.COD_PROV_STATE);
            CAP = recRes.CAP_CAPRESID;
            CodStato = recRes.TB_COMUNE.COD_SIGLANAZIONE;
            Stato = recRes.TB_COMUNE.TB_NAZIONE.DES_NAZIONE;
        }

        public IndirizzoType Tipologia { get; set; }

        public DateTime Decorrenza { get; set; }
        public DateTime DataFine { get; set; }
        public string Indirizzo { get; set; }
        public string Civico { get; set; }
        [DisplayName("Città")]
        public string CodCitta { get; set; }
        public string Citta { get; set; }

        [StringLength(maximumLength: 5, MinimumLength = 1)]
        public string CAP { get; set; }
        public string CodStato { get; set; }
        public string Stato { get; set; }
        public bool AssegnaDomilicio { get; set; }

        public bool G_Contabilita { get; set; }
        public bool G_PrimoIndirizzo { get; set; }
        public DateTime G_CambioRes { get; set; }
    }
    public class DataModel
    {
        private int? _year;
        private int? _month;
        private int? _day;

        private bool _hasValue;

        public void Set(DateTime? date)
        {
            _hasValue = date.HasValue;

            if (!_hasValue)
                return;

            _year = date.Value.Year;
            _month = date.Value.Month;
            _day = date.Value.Day;
        }
        public void Set(string inputDate, string separator = "", int yearLength = 0, int monthLength = 0, int dayLength = 0, bool isReverse = false)
        {
            if (String.IsNullOrWhiteSpace(inputDate))
                return;

            _hasValue = true;

            string date = inputDate;
            if (!String.IsNullOrWhiteSpace(separator))
                date = date.Replace(separator, "");

            if (isReverse)
            {
                if (yearLength > 0)
                    _year = Convert.ToInt32(date.Substring(0, yearLength));
                if (monthLength > 0)
                    _month = Convert.ToInt32(date.Substring(0 + yearLength, monthLength));
                if (dayLength > 0)
                    _day = Convert.ToInt32(date.Substring(0 + yearLength + monthLength, dayLength));
            }
            else
            {
                if (dayLength > 0)
                    _day = Convert.ToInt32(date.Substring(0, dayLength));
                if (monthLength > 0)
                    _month = Convert.ToInt32(date.Substring(0 + dayLength, monthLength));
                if (yearLength > 0)
                    _year = Convert.ToInt32(date.Substring(0 + dayLength + monthLength, yearLength));
            }
        }
        public string Get(bool isReverse = false, string separator = "")
        {
            string result = "";

            if (isReverse)
            {
                result += _year.HasValue ? _year.Value.ToString("0000") : "";
                if (!String.IsNullOrWhiteSpace(separator) && result != "") result += separator;
                result += _month.HasValue ? _month.Value.ToString("00") : "";
                if (!String.IsNullOrWhiteSpace(separator) && result != "") result += separator;
                result += _day.HasValue ? _day.Value.ToString("00") : "";
            }
            else
            {
                result += _day.HasValue ? _day.Value.ToString("00") : "";
                if (!String.IsNullOrWhiteSpace(separator) && result != "") result += separator;
                result += _month.HasValue ? _month.Value.ToString("00") : "";
                if (!String.IsNullOrWhiteSpace(separator) && result != "") result += separator;
                result += _year.HasValue ? _year.Value.ToString("0000") : "";
            }

            return result;
        }

        public bool HasValue
        {
            get { return _hasValue; }
        }

        public DateTime GetDate()
        {
            return new DateTime(_year.HasValue ? _year.Value : 1900, _month.HasValue ? _month.Value : 1, _day.HasValue ? _day.Value : 1);
        }
    }
    public class RedditoModel : _IdentityData
    {
        public RedditoModel()
        {
            Maggiorazioni = new List<MaggiorazModel>();
        }
        public int Anno { get; set; }
        public decimal? Ral_media { get; set; }
        public List<MaggiorazModel> Maggiorazioni { get; set; }
    }
    public class MaggiorazModel : _IdentityData
    {
        public string matricola_dp { get; set; }
        public int Anno { get; set; }
        public string cod_aggregato_costi { get; set; }
        public string desc_aggregato_costi { get; set; }
        public string cod_voce_cedolino { get; set; }
        public string desc_voce_cedolino { get; set; }
        public decimal Ore { get; set; }
        public decimal Giorni { get; set; }
        public decimal NumeroPrestazioni { get; set; }
        public decimal Importo { get; set; }
    }

    public class SitDebitModel : _IdentityData
    {
        /// <summary>
        /// Nome della compagnia.
        /// Questo dato verrà rappresentato in grassetto nella tabella
        /// </summary>
        public string Descrizione { get; set; }

        /// <summary>
        /// Importo totale del debito
        /// </summary>
        public double Addebito { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string MeseDa { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string MeseA { get; set; }

        /// <summary>
        /// Importo della singola rata
        /// </summary>
        public double ImportoRata { get; set; }

        /// <summary>
        /// Numero di rate del contratto
        /// </summary>
        public int NumeroRate { get; set; }

        /// <summary>
        /// Importo rimanente
        /// </summary>
        public double ImportoRateResidue { get; set; }

        /// <summary>
        /// Numero di rate rimanenti
        /// </summary>
        public int NumeroRateResidue { get; set; }

        public int IntMeseDa { get; set; }
        public int IntMeseA { get; set; }
        public int AnnoDa { get; set; }
        public int AnnoA { get; set; }
    }
    public class CorsoFormazione : _IdentityData
    {
        public string Codice { get; set; }
        public Nullable<System.DateTime> DataInizioDate { get; set; }
        public string DataInizio { get; set; }
        public string DataFine { get; set; }
        public string TitoloCorso { get; set; }
        public Nullable<decimal> Durata { get; set; }
        public string Societa { get; set; }
        public int flagImage { get; set; }
    }
    public class SezioneModel
    {
        public SezioneModel()
        {
            Responsabili = new List<IncaricoModel>();
        }
        public string Codice { get; set; }
        public string Descrizione { get; set; }
        public List<IncaricoModel> Responsabili { get; set; }
    }
    public class IncaricoModel
    {
        public string Matricola { get; set; }
        public string Nominativo { get; set; }
        public string Descrizione { get; set; }
        public string CodIncarico { get; set; }
        public string DesIncarico { get; set; }
        public string CodStruttura { get; set; }
        public string DesStruttura { get; set; }
    }

    public class SintesiModel
    {
        private myRaiData.Incentivi.SINTESI1 _cezanne;
        private myRaiDataTalentia.SINTESI1 _talentia;

        public SintesiModel(myRaiData.Incentivi.SINTESI1 sint)
        {
            _cezanne = sint;
        }
        public SintesiModel(myRaiDataTalentia.SINTESI1 sint)
        {
            _talentia = sint;
        }

        public int ID_PERSONA
        {
            get
            {
                if (_cezanne != null)
                    return _cezanne.ID_PERSONA;
                else
                    return _talentia.ID_PERSONA;
            }
        }
        public string COD_MATLIBROMAT
        {
            get
            {
                if (_cezanne != null)
                    return _cezanne.COD_MATLIBROMAT;
                else
                    return _talentia.COD_MATLIBROMAT;
            }
        }
    }

    public class AnagEcc
    {
        public AnagEcc()
        {
            this.tipo_eccezione = TipoAnagEcc.Standard;
        }
        public int cod_mese { get; set; }
        public int num_anno { get; set; }
        public int num_giorno { get; set; }
        public string cod_eccez_padre { get; set; }
        public string desc_cod_eccez_padre { get; set; }
        public string cod_eccezione { get; set; }
        public string desc_eccezione { get; set; }
        public string unita_misura { get; set; }
        public decimal? quantita_numero { get; set; }
        public decimal? quantita_ore { get; set; }
        public string tipo_giorno { get; set; }
        public TipoAnagEcc tipo_eccezione { get; set; }
    }

    public class AnagGiornata : _IdentityData
    {
        public int num_anno { get; set; }
        public int cod_mese { get; set; }
        public int num_giorno { get; set; }
        public string ora_entrata { get; set; }
        public string ora_uscita { get; set; }
        public string sintesi_giornata { get; set; }
        public string sintesi2_giornata { get; set; }
        public string sintesi3_giornata { get; set; }
        public string ore_lavorate { get; set; }
        public string ore_presenza { get; set; }
    }


    public class RecordStorico : _IdentityData
    {
        public RecordStorico() : base()
        {
            Properties = new Dictionary<string, string>();
        }
        public DateTime Date { get; set; }
        public string Title { get; set; }
        public Dictionary<string, string> Properties { get; set; }
    }
}
