using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using myRaiHelper;
using myRaiCommonModel.DataControllers.RaiAcademy;
using myRaiDataTalentia;

namespace myRaiCommonModel.RaiAcademy
{
    public class Corso
	{
		public Corso ()
		{
			this.TipoCorso = TipoCorsoEnum.CorsoSingolo;
			this.CertificazioneOttenuta = "nessuna certificazione";
			this.Completamento = 0;
            this.Pacchetto = new Pacchetto();
            this.Risorse = new Materiali();
		}

		/// <summary>
		/// Identificativo univoco del corso in esame
		/// </summary>
		public int Id { get; set; }

        public int IdTematica { get; set; }

		/// <summary>
		/// Area tematica a cui appartiene il corso
		/// </summary>
		public string Tematica { get; set; }

		/// <summary>
		/// Titolo del corso
		/// </summary>
		public string Titolo { get; set; }

		/// <summary>
		/// Descrizione del corso
		/// </summary>
		public string Descrizione { get; set; }

		/// <summary>
		/// A chi è rivolto il corso
		/// </summary>
		public string Target { get; set; }

		/// <summary>
		/// Numero di partecipanti
		/// </summary>
		public int NumeroPartecipanti { get; set; }

		/// <summary>
		/// Portale al quale il corso appartiene. Es: RaiAcademy
		/// </summary>
		public string NomePortale { get; set; }

		/// <summary>
		/// Data di partenza del corso
		/// </summary>
		public DateTime? DataInizioDisponibilita { get; set; }

		/// <summary>
		/// Durata. Es: 15 minuti
		/// </summary>
		public string Durata { get; set; }

        public decimal QtaDurata { get; set; }
        public string UdmDurata { get; set; }

		/// <summary>
		/// Valore che indica la percentuale di completamento del corso in esame
		/// </summary>
		public int Completamento { get; set; }

		/// <summary>
		/// Stato del corso. Es: Aperto
		/// </summary>
		public StatoCorsoTipoOffertaEnum Stato { get; set; }

        public string StatoStr { get; set; }

		/// <summary>
		/// Tipologia del corso ex: Singolo oppure pacchetto
		/// </summary>
		public TipoCorsoEnum TipoCorso { get; set; }

		/// <summary>
		/// Descrizione dell'eventuale certificazione conseguita al termine del corso
		/// </summary>
		public string CertificazioneOttenuta { get; set; }

		/// <summary>
		/// Ids di eventuali corsi che vanno conseguiti prima di poter seguire il corso corrente
		/// </summary>
		public List<int> IdCorsiPropedeutici { get; set; }

		/// <summary>
		/// Immagine in formato stringa base64
		/// </summary>
		public byte[] Immagine { get; set; }

        /// <summary>
        /// Didascalia immagine
        /// </summary>
        public string ImmagineDidascalia { get; set; }
		
		/// <summary>
		/// Oggetto che rappresenta i dati inseriti nel tab Obiettivi e contenuti
		/// </summary>
		public ObiettiviEContenuti ObiettiviEContenuti { get; set; }

		/// <summary>
		/// Oggetto che rappresenta i dati contenuti nel tab Altri dettagli
		/// </summary>
		public AltriDettagli AltriDettagli { get; set; }

		/// <summary>
		/// Valorizzato se il corso è un pacchetto. Questo oggetto include
		/// le info riguardanti il pacchetto stesso più i dati dei corsi che lo 
		/// compongono
		/// </summary>
		public Pacchetto Pacchetto { get; set; }

        /// <summary>
        /// Indica dove si terrà il corso
        /// </summary>
        public string Sede { get; set; }

        /// <summary>
        /// Indica se il corso sarà fruito in aula, online, o secondo altre metodologie
        /// </summary>
        public string MetodoFormativo { get; set; }
        
        public string DesMetodoFormativoStr { get; set; }

        public MetodoEnum TipoMetodoFormativo { get; set; }

        public int IdAreaFormativa { get; set; }

        public string AreaFormativa { get; set; }

        public Materiali Risorse { get; set; }

        public bool MiIncuriosisce { get; set; }

        public DateTime DataInizio { get; set; }
        
        public DateTime DataFine { get; set; }

        public string Societa { get; set; }

        public string DisponibileDal { get; set; }

        public string Gruppo { get; set; }

        public IscrittoEnum Iscritto { get; set; }

        public string ExternalCourse { get; set; }

        public bool DaCatalogo { get; set; }
        public bool HasAttestato { get; internal set; }
	}


    public enum StatoPubblicazione
    {
        NonDefinito, NonPubblicato, Pubblicato
    }

    /// <summary>
    /// Enum con le possili metodologie di corso
    /// </summary>
    public enum MetodoEnum
    {
        [Display(Name = "Non Definito")]
        [AmbientValue("Non definito")]
        NonDefinito = 0,
        [Display(Name = "Corso in presenza")]
        [AmbientValue("Corso in presenza")]
        FPRES = 1,
        [Display(Name = "Corsi a distanza")]
        [AmbientValue("Corsi a distanza")]
        FAD = 2,
        [Display(Name = "Altro")]
        [AmbientValue("Altro")]
        Altro = 3
    }

    public enum IscrittoEnum
    {
        NonIscritto = 0,
        RichiestaInAttesa = 1,
        Iscritto = 2
    }

	/// <summary>
	/// Enum con le possibili tipologie per il corso
	/// </summary>
	public enum TipoCorsoEnum
	{
        [IconAttribute("fa-circle circle-orange")]
        [Display(Name = "Non Definito")]
        [AmbientValue("Non definito")]
        NonDefinito = 0,
		[IconAttribute( "fa-circle circle-orange" )]
		[Display( Name = "CorsoSingolo" )]
		[AmbientValue( "Corso singolo" )]
		CorsoSingolo = 1,
		[IconAttribute( "fa-circle circle-green" )]
		[Display( Name = "Pacchetto" )]
		[AmbientValue( "Pacchetto" )]
		Pacchetto = 2
	}

	/// <summary>
	/// Enum che rappresenta i possibili valori assunti dallo stato
	/// </summary>
	public enum StatoCorsoTipoOffertaEnum
	{
        [IconAttribute("fa-circle circle-orange")]
        [Display(Name = "Non Definito")]
        [AmbientValue("Non Definito")]
        NonDefinito = 0,
		[IconAttribute( "fa-circle circle-orange" )]
		[Display( Name = "Su Richiesta" )]
        [AmbientValue("Su Richiesta")]
		SuRichiesta = 1,
		[IconAttribute( "fa-circle circle-green" )]
        [Display(Name = "Aperto a tutti")]
        [AmbientValue("Aperto a tutti")]
		Aperta = 2,
        [IconAttribute( "fa-circle circle-green" )]
        [Display(Name = "Obbligatoria")]
		[AmbientValue( "Obbligatoria" )]
		Obbligatoria = 3
    }

    /// <summary>
    /// Enum che rappresenta i possibili valori assunti dallo stato
    /// </summary>
    public enum StatoCorsoEnum
    {
        [IconAttribute("fa-circle circle-orange")]
        [Display(Name = "Aperto")]
        [AmbientValue("Offerta aperta")]
        Aperto = 1,
        [IconAttribute("fa-circle circle-green")]
        [Display(Name = "InDefinizione")]
        [AmbientValue("Offerta in definizione")]
        InDefinizione = 2
    }

	/// <summary>
	/// Definizione dell'oggetto Pacchetto
	/// </summary>
	public class Pacchetto
	{
        public Pacchetto()
        {
            Pillole = new List<Risorsa>();
        }
		/// <summary>
		/// Identificativo univoco del pacchetto
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Titolo del pacchetto
		/// </summary>
		public string Titolo { get; set; }

		/// <summary>
		/// Descrizione del pacchetto
		/// </summary>
		public string Descrizione { get; set; }

		/// <summary>
		/// Presentazione è il testo che verrà visualizzato come descrizione estesa del pacchetto
		/// </summary>
		public string Presentazione { get; set; }

		/// <summary>
		/// Corsi che fanno parte del pacchetto
		/// </summary>
		public List<CorsoExt> Corsi { get; set; }

        public List<Risorsa> Pillole { get; set; }

        public IscrittoEnum Iscritto { get; set; }

        public int IdCorso { get; set; }
        public StatoCorsoTipoOffertaEnum TipoOfferta { get; set; }
    }

    public class Materiali
    {
        public Materiali()
        {
            Risorse = new List<Risorsa>();
        }
        public int IdCorso { get; set; }
        public StatoCorsoTipoOffertaEnum TipoOfferta { get; set; }
        public IscrittoEnum Iscritto { get; set; }
        public List<Risorsa> Risorse { get; set; }
    }

	/// <summary>
	/// Oggetto che rappresenta i dati che andranno a comporre il primo tab dell'interfaccia di dettaglio di un corso
	/// </summary>
	public class ObiettiviEContenuti
	{
		public string Serve { get; set; }

		public string Imparerai { get; set; }
	}

	/// <summary>
	/// Oggetto che rappresenta i dati che andranno a comporre il secondo tab dell'interfaccia di dettaglio di un corso
	/// </summary>
	public class AltriDettagli
	{
        public AltriDettagli()
        {
            PercorsiFormativi = new List<PercorsoFormativo>();
        }

		public string Requisiti { get; set; }

		public string ArticolazioneCorso { get; set; }

		public string Docenti { get; set; }

		public List<Edizione> Edizioni{ get; set; }
        public string NoteEdizioni { get; set; }

		public string Note { get; set; }

        public List<PercorsoFormativo> PercorsiFormativi { get; set; }
    }

    public class PercorsoFormativo
    {
        public int Id { get; set; }
        public string Nome { get; set; }
	}

    public class Edizione
    {
        public Edizione()
        {
            Giornate = new List<EdizioneGiornata>();
        }

        public int IdEdizione { get; set; }
        public string Nome { get; set; }
        public DateTime DataInizio { get; set; }
        public DateTime DataFine { get; set; }
        public string Luogo { get; set; }
        public string DesLuogo { get; set; }
        public string Orario { get; set; }
        public int FromHour { get; set; }
        public int FromMinute { get; set; }
        public int ToHour { get; set; }
        public int ToMinute { get; set; }
        public string Note { get; set; }
        public List<EdizioneGiornata> Giornate { get; set; }
        public IscrittoEnum Iscritto { get; set; }

        /// <summary>
        /// E - Disponibile, C - Cancellato, R - Rimandato, P - Previsto, S - Sospeso, M - In manutenzione, X - Registrazioni chiuse
        /// </summary>
        public string Stato { get; set; }
    }
    public class EdizioneGiornata
    {
        public DateTime Data { get; set; }
        public int DaOra { get; set; }
        public int DaMinuti { get; set; }
        public int AOra { get; set; }
        public int AMinuti { get; set; }
    }

    public class Risorsa
    {
        public int Prog { get; set; }
        public string Nome { get; set; }
        public string Tipo { get; set; }
        public string Url { get; set; }
        public bool Completato { get; set; }
        public bool Iniziato { get; set; }
        public List<int> Vincoli { get; set; }
        public bool Abilitato { get; set; }
        public string VincoloEdizione { get; set; }
        public int IliasId { get; set; }
    }

	/// <summary>
	/// Estensione dell'oggetto Corso. Questi dati verranno valorizzati solo se il corso è un pacchetto
	/// </summary>
	public class CorsoExt : Corso
	{
		/// <summary>
		/// Posizione del corso all'interno di un pacchetto. 
		/// Tale attributo viene utilizzato per definire un ordine gerarchico all'interno del pacchetto
		/// </summary>
		public int Posizione { get; set; }

        public string Nome { get; set; }

        public string Url { get; set; }
	}

	/// <summary>
	/// Modello che rappresenta l'oggetto passato alla view di dettaglio corso
	/// </summary>
	public class DettaglioCorsoVM
	{
		public Corso Corso { get; set; }
		public CorsoList Consigliati { get; set; }
		public List<Corso> AttinentiArea { get; set; }

        public AcademyPrivacy Privacy { get; set; }
	}

	/// <summary>
	/// 
	/// </summary>
	public class RaiAcademyVM
	{
		public RaiAcademyVM ()
		{
            this.Corsi = new CorsoList();
		}

		public CorsoList Corsi { get; set; }

        public FiltriCatalogoModel filtri { get; set; }
	}

    public enum CorsoDetail
    {
        Informazioni, DataInizio, CorsiFatti
    }

    public enum CorsiFilter
    {
        All = 0,
        Iniziati,
        Obbligatori,
        MiIncuriscono,
		Consigliati,
		Attinenti,
        InApprovazione,
        DaFare
    }

    public class ItemList<T>
    {
        public ItemList()
        {
            ShowAll = true;
            ShowLimit = 3;
            AddLayout = false;
            NoItemLabel = "Nessun elemento trovato";
			this.PageSize = 9;
        }

        public string Titolo { get; set; }
        public List<T> Items { get; set; }
        public bool ShowAll { get; set; }
        public int ShowLimit { get; set; }
        public string NoItemLabel { get; set; }

        public string ActionName { get; set; }
        public string ControllerName { get; set; }
        public object RouteValues { get; set; }
        public bool AddLayout { get; set; }
		public int PageSize { get; set; }
    }

    public class CorsoList : ItemList<Corso>
    {
        public CorsoList():base()
        {
            Items = new List<Corso>();

            ShowProgress = false;
            Dettagli = CorsoDetail.Informazioni;
            ActionName = "ElencoCorsi";
            ControllerName = "RaiAcademy";
            NoItemLabel = "Nessun corso trovato";
            ShowDashboardButton = false;
        }

        public bool ShowProgress { get; set; }
        public CorsoDetail Dettagli { get; set; }
        public CorsiFilter ActionFilter { get; set; }
		public int PageSize { get; set; }
		public int TotalCourses { get; set; }
		public int CurrentPage { get; set; }
        public bool ShowDashboardButton { get; set; }
    }

    public class LinkList : ItemList<Link>
    {
        public LinkList():base()
        {
            NoItemLabel = "Nessun riconoscimento trovato";
            ControllerName = "RaiAcademy";
            Titolo = "Riconoscimenti";
            Items = new List<Link>();
        }
    }

    /// <summary
    /// Modello per dashboard
    /// </summary>
    public class Dashboard
    {
        public AlertModel AlertCorsiDaFare { get; set; }
        public AlertModel AlertCorsiDaApprovare { get; set; }
        public AlertModel AlertCorsiIniziati { get; set; }
        public AlertModel AlertCorsiFatti { get; set; }

        public List<Link> Interessi { get; set; }

        public Dashboard()
        {
            AlertCorsiDaFare = new AlertModel
            {
                CifraPrincipale = "0",
                ClasseIcona = "icons icon-notebook",
                ColoreClasseIcona = "cda",
                HrefPulsante = "#",
                TestoPulsante = "Dettaglio",
                Titolo = "Corsi da fare",
                TestoRosso = "",
                TraParentesi = "",
                Visibile = true,
                TipoAlert = 1,
                intro_datastep = "1",
                intro_dataintro = "Questi sono i corsi che hai da fare oggi"
            };
            AlertCorsiDaApprovare = new AlertModel
            {
                CifraPrincipale = "0",
                ClasseIcona = "icons icon-notebook",
                ColoreClasseIcona = "cda",
                HrefPulsante = "#",
                TestoPulsante = "Dettaglio",
                Titolo = "Corsi da approvare",
                TraParentesi = "",
                Visibile = true,
                TipoAlert = 1,
                intro_datastep = "1",
                intro_dataintro = "Questi sono i corsi che devono ancora essere approvati"
            };
            AlertCorsiIniziati = new AlertModel
            {
                CifraPrincipale = "0",
                ClasseIcona = "icons icon-notebook",
                ColoreClasseIcona = "cda",
                HrefPulsante = "",
                TestoPulsante = "Dettaglio",
                Titolo = "Corsi iniziati",
                TraParentesi = "",
                Visibile = true,
                TipoAlert = 1,
                intro_datastep = "1",
                intro_dataintro = "Questi sono i corsi iniziati"
            };
            AlertCorsiFatti = new AlertModel
            {
                CifraPrincipale = "0",
                ClasseIcona = "icons icon-notebook",
                ColoreClasseIcona = "cda",
                HrefPulsante = "#",
                TestoPulsante = "Dettaglio",
                Titolo = "Corsi fatti",
                TraParentesi = "",
                Visibile = true,
                TipoAlert = 1,
                intro_datastep = "1",
                intro_dataintro = "Questi sono i corsi che hai già fatto"
            };

            Interessi = new List<Link>();
        }
    }

    public class Link
    {
        public string Testo { get; set; }
        public string Href { get; set; }
    }

    public class AgendaCorsi : Agenda
    {
        private bool ShowRecord(DateTime date)
        {
            return date.Year == AnnoCorrente && date.Month == MeseCorrente;
        }

        public void CaricaCorsi(string matricola)
        {
            RaiAcademyDataController dataController = new RaiAcademyDataController();
            List<int> listCorsi = new List<int>();
            using (TalentiaEntities db = new TalentiaEntities())
            {
                listCorsi.AddRange(db.CURRFORM
                    .Where(x => x.SINTESI1.COD_MATLIBROMAT == matricola && x.IND_STATOEVENTO == "E" && x.IND_PARTICIPANT == "N")
                    .Select(y => y.EDIZIONE.ID_CORSO));

                listCorsi.AddRange(db.TREQUESTS
                    .Where(x => x.SINTESI1.COD_MATLIBROMAT == matricola && !x.TREQUESTS_STEP.Any(y => y.IND_CURRFORM != null))
                    .Select(y => y.ID_CORSO.Value));
            }

            if (listCorsi.Count() == 0)
                return;

            var elencoCorsi = dataController.EstraiCorsi(matricola, true, 0, false, false, StatoPubblicazione.Pubblicato, listCorsi);
            foreach (var corso in elencoCorsi.Where(x=>x.Iscritto!=IscrittoEnum.NonIscritto))
            {
                var ediz = corso.AltriDettagli.Edizioni.FirstOrDefault(x => x.Iscritto != IscrittoEnum.NonIscritto);

                AppuntamentoStato stato = ediz.Iscritto == IscrittoEnum.Iscritto ? AppuntamentoStato.Approvato : AppuntamentoStato.DaApprovare;

                if (corso.TipoMetodoFormativo==MetodoEnum.FAD)
                {
                    Appuntamento app = new Appuntamento();
                    app.Testo = corso.Titolo;
                    app.Stato = stato;
                    app.Orario = "Online";
                    app.Sede = corso.NomePortale;
                    if (ShowRecord(ediz.DataInizio))
                    {
                        app.Giorno = ediz.DataInizio;
                        Appuntamenti.Add(app);
                    }
                    else if (ShowRecord(ediz.DataFine))
                    {
                        app.Giorno = ediz.DataFine;
                        app.Testo += " (Termine edizione)";
                        Appuntamenti.Add(app);
                    }
                }
                else if (ediz.Giornate!=null && ediz.Giornate.Count()>0)
                {
                    foreach (var gg in ediz.Giornate.Where(x=>ShowRecord(x.Data)))
                    {
                        Appuntamento app = new Appuntamento();
                        app.Giorno = gg.Data;
                        app.Testo = corso.Titolo;
                        app.Stato = stato;
                        app.Orario = String.Format("{0:00}.{1:00}/{2:00}.{3:00}", gg.DaOra, gg.DaMinuti, gg.AOra, gg.AMinuti);
                        app.Sede = ediz.DesLuogo;
                        Appuntamenti.Add(app);
                    }
                }
                else
                {
                    Appuntamento app = new Appuntamento();
                    app.Testo = corso.Titolo;
                    app.Stato = stato;
                    app.Orario = "-";
                    app.Sede = ediz.DesLuogo;
                    if (ShowRecord(ediz.DataInizio))
                    {
                        app.Giorno = ediz.DataInizio;
                        Appuntamenti.Add(app);
                    }
                    else if (ShowRecord(ediz.DataFine))
                    {
                        app.Giorno = ediz.DataFine;
                        app.Testo += " (Termine edizione)";
                        Appuntamenti.Add(app);
                    }
                }
            }
        }
    }

    public class CalendarioCorsi : Calendario
    {
        private bool ShowRecord(DateTime date)
        {
            return date.Year == this.Anno && date.Month == this.Mese;
        }

        public void CaricaCorsi(string matricola)
        {
            CalendarioGiorno day;
            RaiAcademyDataController dataController = new RaiAcademyDataController();

            List<int> listCorsi = new List<int>();
            using (TalentiaEntities db = new TalentiaEntities())
            {
                listCorsi.AddRange(db.CURRFORM
                    .Where(x => x.SINTESI1.COD_MATLIBROMAT == matricola && x.IND_STATOEVENTO == "E" && x.IND_PARTICIPANT == "N")
                    .Select(y => y.EDIZIONE.ID_CORSO));

                listCorsi.AddRange(db.TREQUESTS
                    .Where(x => x.SINTESI1.COD_MATLIBROMAT == matricola && !x.TREQUESTS_STEP.Any(y => y.IND_CURRFORM != null))
                    .Select(y => y.ID_CORSO.Value));
            }

            if (listCorsi.Count() == 0)
                return;

            var elencoCorsi = dataController.EstraiCorsi(matricola, true, 0, false, false, StatoPubblicazione.Pubblicato, listCorsi);
            foreach (var corso in elencoCorsi.Where(x => x.Iscritto != IscrittoEnum.NonIscritto))
            {
                var ediz = corso.AltriDettagli.Edizioni.FirstOrDefault(x => x.Iscritto != IscrittoEnum.NonIscritto);

                AppuntamentoStato stato = ediz.Iscritto == IscrittoEnum.Iscritto ? AppuntamentoStato.Approvato : AppuntamentoStato.DaApprovare;

                if (corso.TipoMetodoFormativo == MetodoEnum.FAD)
                {
                    Appuntamento app = new Appuntamento();
                    app.Testo = corso.Titolo;
                    app.Stato = stato;
                    app.Orario = "Online";
                    app.Sede = corso.NomePortale;
                    if (ShowRecord(ediz.DataInizio))
                    {
                        day = DaysShowed.First(x => x.giorno == ediz.DataInizio);
                        day.Frazione = 0;
                        day.Tooltip = corso.Titolo;
                        day.Stato = stato;
                    }
                    if (ShowRecord(ediz.DataFine))
                    {
                        day = DaysShowed.First(x => x.giorno == ediz.DataFine);
                        day.Frazione = 0;
                        day.Tooltip = (stato == AppuntamentoStato.DaApprovare ? "RICHIESTA IN ATTESA PER " : "") + corso.Titolo+" (Termine edizione)";
                        day.Stato = stato;
                    }
                }
                else if (ediz.Giornate != null && ediz.Giornate.Count() > 0)
                {
                    foreach (var gg in ediz.Giornate.Where(x => ShowRecord(x.Data)))
                    {
                        day = DaysShowed.First(x => x.giorno == gg.Data);
                        day.Frazione = 0;
                        day.Tooltip = (stato==AppuntamentoStato.DaApprovare?"RICHIESTA IN ATTESA PER ":"")+corso.Titolo;
                        day.Stato = stato;
                    }
                }
                else
                {
                    Appuntamento app = new Appuntamento();
                    app.Testo = corso.Titolo;
                    app.Stato = stato;
                    app.Orario = "-";
                    app.Sede = ediz.DesLuogo;
                    if (ShowRecord(ediz.DataInizio))
                    {
                        day = DaysShowed.First(x => x.giorno == ediz.DataInizio);
                        day.Frazione = 0;
                        day.Tooltip = (stato == AppuntamentoStato.DaApprovare ? "RICHIESTA IN ATTESA PER " : "") + corso.Titolo;
                        day.Stato = stato;
                    }
                    if (ShowRecord(ediz.DataFine))
                    {
                        day = DaysShowed.First(x => x.giorno == ediz.DataFine);
                        day.Frazione = 0;
                        day.Tooltip = (stato == AppuntamentoStato.DaApprovare ? "RICHIESTA IN ATTESA PER " : "") + corso.Titolo+" (Termine edizione)";
                        day.Stato = stato;
                    }
                }
            }

        }
    }

	public class GetCorsiResult
	{
		public GetCorsiResult ()
		{
			this.List = new List<Corso>();
			this.TotaleCorsi = 0;
		}

		public List<Corso> List { get; set; }
		public int TotaleCorsi { get; set; }
	}

    public class RichiestaIscrizione
    {
        public bool FromCatalogue { get; set; }

        public Corso Corso { get; set; }

        [Required(ErrorMessage = "Campo obbligatorio")]
        public string TitoloCorso { get; set; }

        [Required(ErrorMessage = "Campo obbligatorio")]
        [MaxLength(2000)]
        public string Motivazione { get; set; }

        [MaxLength(2000)]
        public string NoteAggiuntive { get; set; }

        [Required(ErrorMessage = "Campo obbligatorio")]
        public string DestinatarioRichiesta { get; set; }

        public int IdEdizione { get; set; }
    }

    public class FiltriCatalogo
    {
        public string Cerca { get; set; }
        public string Area { get; set; }
        public string Tema { get; set; }
        public string Gruppo { get; set; }
    }

    public class CatalagoRaiPlace
    {
        public CatalagoRaiPlace()
        {
            Categorie = new List<CategoriaRaiPlace>();
        }

        public string urlCatalogoCorsi { get; set; }
        public List<CategoriaRaiPlace> Categorie { get; set; }

        public int CVPerc { get; set; }

        public List<Corso> CorsiFatti { get; set; }
    }

    public class CategoriaRaiPlace
    {
        public CategoriaRaiPlace ()
	    {
            tematiche = new List<TematicaRaiPlace>();
            corsi = new List<Corso>();
	    }

        public int idCategoria { get; set; }
        public string nomeCategoria { get; set; }
        public string urlCategoria { get; set; }
        public List<TematicaRaiPlace> tematiche { get; set; }
        public List<Corso> corsi { get; set; }
    }

    public class TematicaRaiPlace
    {
        public int idTematica { get; set; }
        public string codTematica { get; set; }
        public string url { get; set; }
    }

    public class AcademyPrivacy
    {
        public bool Active { get; set; }
        public bool IsAccepted { get; set; }
        public DateTime? AcceptanceDate { get; set; }
        public string Text { get; set; }
    }

    public class tmpV_CVCorsiRai
    {
        public string matricola { get; set; }
        public string codice { get; set; }
        public Nullable<System.DateTime> DataInizioDate { get; set; }
        public string DataInizio { get; set; }
        public string DataFine { get; set; }
        public string TitoloCorso { get; set; }
        public Nullable<decimal> Durata { get; set; }
        public string Societa { get; set; }
        public int flagImage { get; set; }
    }
}