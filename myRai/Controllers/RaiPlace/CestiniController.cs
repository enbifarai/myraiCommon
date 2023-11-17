using myRaiData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using System.Transactions;
using myRaiHelper;
using System.Globalization;
using iTextSharp.text;
using iTextSharp.text.pdf;
using myRaiHelper;
using myRaiCommonModel.raiplace;
using myRai.Data.CurriculumVitae;
using myRaiServiceHub.it.rai.servizi.sendmail;

namespace myRai.Controllers.RaiPlace
{
    public class CestiniController : BaseCommonController
    {
		private StampaCestiniDataController printCestiniDataController = new StampaCestiniDataController();

        /// <summary>
        /// View principale
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            List<CestiniModel> listacestini = this.GetListaCestini();
            return View("~/Views/RaiPlace/Cestini/IndexUser.cshtml", listacestini);
        }

        private void InitSessionScopeData(Ordine ordine)
        {
            it.rai.servizi.hrgb.Service s = new it.rai.servizi.hrgb.Service();
            // reperimento della sede dell'utente corrente
            var r = s.EsponiAnagrafica_Net("COMINT", UtenteHelper.EsponiAnagrafica()._matricola);
            string sede = UtenteHelper.EsponiAnagrafica()._sedeApppartenenza;
            string puntoOrdinante = UtenteHelper.EsponiAnagrafica()._sedeApppartenenza;
            if (r.DT_Anagrafica.Rows != null)
            {
                foreach (System.Data.DataRow item in r.DT_Anagrafica.Rows)
                {
                    puntoOrdinante = item[24].ToString(); // Es. Roma-Direzione Generale
                    sede = item[2].ToString(); // Es. Roma
                }
            }
            CestiniControllerScope.Instance.Cestino = new CestiniModel();
            CestiniControllerScope.Instance.Cestino.ordine = new Ordine();
            CestiniControllerScope.Instance.Cestino.ordine = ordine;

			if ( ordine.dataOraPasto.Equals( DateTime.MinValue ) )
			{
				CestiniControllerScope.Instance.Cestino.ordine.dataOraPasto = DateTime.Now;
			}

            CestiniControllerScope.Instance.Cestino.ordine.matricolaOrdine = UtenteHelper.EsponiAnagrafica()._matricola;
            CestiniControllerScope.Instance.Cestino.ordine.richiedente = String.Format("{0} {1}", UtenteHelper.EsponiAnagrafica()._cognome, UtenteHelper.EsponiAnagrafica()._nome);
            CestiniControllerScope.Instance.Cestino.ordine.telefonoRichiedente = UtenteHelper.EsponiAnagrafica()._telefono;
            CestiniControllerScope.Instance.Cestino.ordine.struttura = sede;
            CestiniControllerScope.Instance.Cestino.ordine.puntoOrdinante = puntoOrdinante;
        }

        /// <summary>
        /// View per l'inserimento di un nuovo cestino
        /// </summary>
        /// <returns></returns>
        public ActionResult ShowGestisciCestino()
        {
            this.InitializeViewBagInformation();
            this.InitSessionScopeData(new Ordine() { statusOrdine = StatusOrdiniEnum.Bozza, DestinatarioCestino = DestinatarioEnum.Me });
            CestiniControllerScope.Instance.Cestino.richieste = new List<Richiesta>();
            CestiniControllerScope.Instance.Cestino.richiestaCorrente = new Richiesta();
            CestiniControllerScope.Instance.Cestino.richiestaCorrente.flagRisorsa = true;
            return PartialView("~/Views/RaiPlace/Cestini/subpartial/_gestisciCestino.cshtml", CestiniControllerScope.Instance.Cestino);
        }

		[HttpGet]
		public ActionResult LoadEditOrdineTab ( )
		{
			return PartialView( "~/Views/RaiPlace/Cestini/subpartial/Ordine.cshtml", CestiniControllerScope.Instance.Cestino );
		}

		[HttpGet]
		public ActionResult loadOrdineTab ( DestinatarioEnum tipoDestinatario )
		{
			this.InitializeViewBagInformation();
			this.InitSessionScopeData( new Ordine() { statusOrdine = StatusOrdiniEnum.Bozza, DestinatarioCestino = DestinatarioEnum.Me } );
			CestiniControllerScope.Instance.Cestino.ordine.DestinatarioCestino = tipoDestinatario;
			CestiniControllerScope.Instance.Cestino.ordine.matricolaReferenteConsegna = UtenteHelper.EsponiAnagrafica()._matricola;

			string descrizioneReferente = UtenteHelper.EsponiAnagrafica()._cognome + " " + UtenteHelper.EsponiAnagrafica()._nome;
			
			CestiniControllerScope.Instance.Cestino.ordine.referenteConsegna = descrizioneReferente;
			CestiniControllerScope.Instance.Cestino.ordine.telefonoReferente = UtenteHelper.EsponiAnagrafica()._telefono;

			if ( tipoDestinatario == DestinatarioEnum.Me ||
				tipoDestinatario == DestinatarioEnum.Interni )
			{
				CestiniControllerScope.Instance.Cestino.ordine.centroCosto = "0000";
				CestiniControllerScope.Instance.Cestino.ordine.matricolaSpettacolo = "000000";
				CestiniControllerScope.Instance.Cestino.ordine.titoloProduzione = "spese di mensa generiche";
			}

			return PartialView( "~/Views/RaiPlace/Cestini/subpartial/Ordine.cshtml", CestiniControllerScope.Instance.Cestino );
		}

		[HttpGet]
		public ActionResult loadTabRichieste ( )
		{
			this.InitializeViewBagInformation();
			if ( CestiniControllerScope.Instance.Cestino.richiestaCorrente == null )
			{
				CestiniControllerScope.Instance.Cestino.richiestaCorrente = new Richiesta();
			}

			if ( CestiniControllerScope.Instance.Cestino.ordine.DestinatarioCestino == DestinatarioEnum.Me )
			{
				CestiniControllerScope.Instance.Cestino.richiestaCorrente.cognomeRisorsa = UtenteHelper.EsponiAnagrafica()._cognome;
				CestiniControllerScope.Instance.Cestino.richiestaCorrente.nomeRisorsa = UtenteHelper.EsponiAnagrafica()._nome;
				CestiniControllerScope.Instance.Cestino.richiestaCorrente.matricolaRisorsa = UtenteHelper.EsponiAnagrafica()._matricola;
				CestiniControllerScope.Instance.Cestino.richiestaCorrente.flagRisorsa = true;
			}
			else if ( CestiniControllerScope.Instance.Cestino.ordine.DestinatarioCestino == DestinatarioEnum.Interni )
			{
				CestiniControllerScope.Instance.Cestino.richiestaCorrente.flagRisorsa = true;
			}
			else if ( CestiniControllerScope.Instance.Cestino.ordine.DestinatarioCestino == DestinatarioEnum.Esterni )
			{
				CestiniControllerScope.Instance.Cestino.richiestaCorrente.flagRisorsa = false;
			}

			return PartialView( "~/Views/RaiPlace/Cestini/subpartial/SingolaRichiesta.cshtml", CestiniControllerScope.Instance.Cestino );
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cestino"></param>
        /// <returns></returns>
        public ActionResult RiepilogoCestino(CestiniModel cestino)
        {
            // init dell'ordine
            if (CestiniControllerScope.Instance.Cestino.ordine == null)
            {
                if (cestino.ordine.statusOrdine == 0)
                    cestino.ordine.statusOrdine = StatusOrdiniEnum.Bozza;

                this.InitSessionScopeData(cestino.ordine);
            }
            else if (CestiniControllerScope.Instance.Cestino.ordine.idOrdine == 0)
            {
                if (cestino.ordine.statusOrdine == 0)
                    cestino.ordine.statusOrdine = StatusOrdiniEnum.Bozza;

                this.InitSessionScopeData(cestino.ordine);
            }
            else
            {
                // ordine in modifica
                CestiniControllerScope.Instance.Cestino.ordine.centroCosto = cestino.ordine.centroCosto;
                CestiniControllerScope.Instance.Cestino.ordine.cespite = cestino.ordine.cespite;
                CestiniControllerScope.Instance.Cestino.ordine.dataOraPasto = cestino.ordine.dataOraPasto;
                CestiniControllerScope.Instance.Cestino.ordine.luogoConsegna = cestino.ordine.luogoConsegna;
                CestiniControllerScope.Instance.Cestino.ordine.matricolaReferenteConsegna = cestino.ordine.matricolaReferenteConsegna;
                CestiniControllerScope.Instance.Cestino.ordine.referenteConsegna = cestino.ordine.referenteConsegna;
                CestiniControllerScope.Instance.Cestino.ordine.telefonoReferente = cestino.ordine.telefonoReferente;
                CestiniControllerScope.Instance.Cestino.ordine.tipoPasto = cestino.ordine.tipoPasto;
                CestiniControllerScope.Instance.Cestino.ordine.titoloProduzione = cestino.ordine.titoloProduzione;
                CestiniControllerScope.Instance.Cestino.ordine.motivoOrdine = cestino.ordine.motivoOrdine;
                CestiniControllerScope.Instance.Cestino.ordine.matricolaSpettacolo = cestino.ordine.matricolaSpettacolo;
            }

            // init della lista richieste
            if (CestiniControllerScope.Instance.Cestino.richieste == null)
                CestiniControllerScope.Instance.Cestino.richieste = new List<Richiesta>();

            if (CestiniControllerScope.Instance.Cestino.richiestaCorrente == null)
                CestiniControllerScope.Instance.Cestino.richiestaCorrente = new Richiesta();

            CestiniControllerScope.Instance.Cestino.ReadOnlyMode = false;
            return PartialView("~/Views/RaiPlace/Cestini/subpartial/_riepilogoCestino.cshtml", CestiniControllerScope.Instance.Cestino);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cestino"></param>
        /// <returns></returns>
        public ActionResult Riepilogo()
        {
            CestiniControllerScope.Instance.Cestino.ReadOnlyMode = true;
            return PartialView("~/Views/RaiPlace/Cestini/subpartial/Riepilogo.cshtml", CestiniControllerScope.Instance.Cestino);
        }

        /// <summary>
        /// Caricamento dell'elenco degli ordini
        /// </summary>
        /// <returns></returns>
        public ActionResult CaricaElencoOrdini()
        {
            List<CestiniModel> listacestini = this.GetListaCestini();
            return PartialView("~/Views/RaiPlace/Cestini/subpartial/_elencoOrdini.cshtml", listacestini);
        }

        #region Gestione Ordini

        /// <summary>
        /// View per la modifica di un cestino esistente
        /// </summary>
        /// <param name="idOrdine"></param>
        /// <returns></returns>
        public ActionResult DettaglioOrdine(int idOrdine)
        {
			try
			{
				this.InitializeViewBagInformation();
				CestiniControllerScope.Instance.Cestino = new CestiniModel();
				CestiniControllerScope.Instance.Cestino = this.GetOrdine(idOrdine);
				this.CalcTipoDestinatario();
                return PartialView( "~/Views/RaiPlace/Cestini/subpartial/_gestisciCestino.cshtml" , CestiniControllerScope.Instance.Cestino );
			}
			catch ( Exception ex )
			{
				throw new Exception( ex.Message );
			}
        }

		private void CalcTipoDestinatario ()
		{
			bool ext = false;
			bool anyInternal = false;
			if ( CestiniControllerScope.Instance.Cestino.richieste != null &&
				CestiniControllerScope.Instance.Cestino.richieste.Any() )
			{
				int limit = CestiniControllerScope.Instance.Cestino.richieste.Count();
				int idx = 0;
				bool exit = false;

				// verifica se esiste almeno una richiesta con flagRisorsa a false, se si
				// significa che è stato inserito un utente occasionale (utente esterno)
				ext = CestiniControllerScope.Instance.Cestino.richieste.Exists( r => !r.flagRisorsa );

				// se c'è almeno un esterno verifica le richieste inserite,
				// se sono tutte per esterni allora la tipologia sarà esterno, nel caso di 
				// richieste miste allora i dati sono incongruenti e verrà solevata un eccezione
				if ( ext )
				{
					anyInternal = CestiniControllerScope.Instance.Cestino.richieste.Exists( r => r.flagRisorsa );

					if ( anyInternal )
					{
						throw new Exception( "Dati incongruenti." );
					}

					CestiniControllerScope.Instance.Cestino.ordine.DestinatarioCestino = DestinatarioEnum.Esterni;
				}
				else
				{
					//myRai.Models.Utente.EsponiAnagrafica

					// se è stata inserita una sola richiesta 
					// e l'utente corrente è sia il destinatario che il richiedente
					// allora la tipologia è "ME"

					if ( limit == 1 )
					{
                        // se la matricola richiedente è == alla matricola dell'utente corrente
                        // E la matricola della risorsa nella singola richiesta è == alla matricola dell'utente corrente
                        if ( CestiniControllerScope.Instance.Cestino.ordine.matricolaOrdine.Equals( UtenteHelper.EsponiAnagrafica( )._matricola , StringComparison.InvariantCultureIgnoreCase ) &&
                            ( CestiniControllerScope.Instance.Cestino.richieste.First( ).matricolaRisorsa.Equals( UtenteHelper.EsponiAnagrafica( )._matricola , StringComparison.InvariantCultureIgnoreCase ) ) )
                        {
                            CestiniControllerScope.Instance.Cestino.ordine.DestinatarioCestino = DestinatarioEnum.Me;
                        }
                        else
                        {
                            CestiniControllerScope.Instance.Cestino.ordine.DestinatarioCestino = DestinatarioEnum.Interni;
                        }
					}
					else
					{
						CestiniControllerScope.Instance.Cestino.ordine.DestinatarioCestino = DestinatarioEnum.Interni;
					}
				}
			}
		}

        /// <summary>
        /// Visualizzazione del riepilogo dell'ordine in modalità readonly
        /// </summary>
        /// <param name="idOrdine"></param>
        /// <returns></returns>
        public ActionResult DettaglioOrdineReadOnly(int idOrdine)
        {
            this.InitializeViewBagInformation();
            CestiniControllerScope.Instance.Cestino = new CestiniModel();
            CestiniControllerScope.Instance.Cestino = this.GetOrdine(idOrdine);
            CestiniControllerScope.Instance.Cestino.ReadOnlyMode = true;
            return PartialView("~/Views/RaiPlace/Cestini/subpartial/_dettaglioOrdineReadOnly.cshtml", CestiniControllerScope.Instance.Cestino);
        }

        /// <summary>
        /// Metodo per il salvataggio di un cestino
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult SalvaOrdine()
        {
            try
            {
                if (CestiniControllerScope.Instance.Cestino.ordine.idOrdine <= 0)
                {
                    // Inserimento di un nuovo cestino
                    CestiniControllerScope.Instance.Cestino.ordine = InserisciOrdine(CestiniControllerScope.Instance.Cestino.ordine);

					// reperimento degli elementi marcati come deleted
					var toRemove = CestiniControllerScope.Instance.Cestino.richieste.Where( r => r.idRichiesta > 0 && r.Deleted ).ToList();
					// se ci sono elementi marcati come deleted allora verra ciclata la lista e tali elementi verranno rimossi dal db
					if ( toRemove != null )
					{
						toRemove.ForEach( r =>
						{
							RimuoviRichiestaInternal( r.idRichiesta );
						} );
					}

					// reperimento della lista di elementi da inserire
					var toInsert = CestiniControllerScope.Instance.Cestino.richieste.Where( r => !r.Deleted ).ToList();
					// se ci sono elementi da inserire la lista verra ciclata e verranno effettuate le insert sul db
					if ( toInsert != null )
					{
						toInsert.ForEach( r =>
						{
							// Poichè le richieste hanno ancora idOrdine = 0, bisognerà aggiornare tale riferimento
							// inserendo l'identificativo dell'ordine appena creato
							r.idOrdine = CestiniControllerScope.Instance.Cestino.ordine.idOrdine;

							r = InserisciRichiesta( r );

							// Invio mail per il nuovo ordine
							if ( !String.IsNullOrEmpty( r.matricolaRisorsa ) )
								this.InviaMail( r.matricolaRisorsa,
									CestiniControllerScope.Instance.Cestino.ordine.richiedente,
									CestiniControllerScope.Instance.Cestino.ordine.dataInserimento,
									String.Format( "{0} ({1})", CestiniControllerScope.Instance.Cestino.ordine.luogoConsegna, CestiniControllerScope.Instance.Cestino.ordine.struttura ),
									CestiniControllerScope.Instance.Cestino.ordine.tipoPasto.ToUpper(),
									String.Format( "{0} ({1})", r.tipoCestino.GetDescription(), r.tipoCestino.GetAmbientValue() ) );
						} );
					}
                }
                else
                {
                    // Aggiornamento dei dati di un cestino esistente
                    CestiniControllerScope.Instance.Cestino.ordine = ModificaOrdine(CestiniControllerScope.Instance.Cestino.ordine);
                }

				return Json( new
				{
					success = true,
					errorMessage = String.Empty
				} );
            }
            catch (Exception ex)
            {
				return Json( new
				{
					success = false,
					errorMessage = ex.Message
				} );
            }
        }

        /// <summary>
        /// Rimozione di un ordine
        /// </summary>
        /// <param name="idOrdine"></param>
        /// <returns></returns>
        public JsonResult RimuoviOrdine(int idOrdine)
        {
            try
            {
                /*
                 * Req.04 – Modifica nuovo ordine
                 * Il sistema, a seguito dell’inserimento di un nuovo ordine da parte dell’utente User, 
                 * deve consentire al suddetto utente di modificare tale ordine, 
                 * modificando o i parametri dell’ordine stesso e/o quelli relativi alle singole richieste 
                 * in esso contenute. 
                 * Tale operazione è consentita al solo utente User inseritore 
                 * e solo qualora l’ordine si trovi ancora nello stato
                 * •	Bozza
                 * •	In attesa di convalida
                 * •	Convalidato
                 * Al contrario, gli ordini identificati dallo stato In lavorazione o Consuntivato 
                 * non potranno essere in alcun modo modificati da parte degli utenti.
                 */

                if (idOrdine <= 0)
                    CestiniControllerScope.Instance.Cestino = new CestiniModel(); // viene distrutto tutto l'oggetto in memoria
                else
                    RimuoviOrdineInternal(idOrdine);  // ordine già creato in precedenza
                // quindi da rimuovere dal db
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public ActionResult CercaOrdini(DateTime datada, DateTime dataal)
        {
			List<CestiniModel> listacestini = this.GetListaCestini( datada, dataal );
            return PartialView("~/Views/RaiPlace/Cestini/subpartial/_elencoOrdini.cshtml", listacestini);
        }

		/// <summary>
		/// Duplicazione di un ordine esistente
		/// </summary>
		/// <param name="idOrdine"></param>
		/// <param name="conDestinatari"></param>
		/// <returns></returns>
		public ActionResult DuplicaCestino ( int idOrdine, bool conDestinatari )
		{
			this.InitializeViewBagInformation();
			CestiniControllerScope.Instance.Cestino = new CestiniModel();
			CestiniControllerScope.Instance.Cestino = this.GetOrdine( idOrdine );

			CestiniControllerScope.Instance.Cestino.ordine.codiceOrdine = null;
			CestiniControllerScope.Instance.Cestino.ordine.idOrdine = -1;
			CestiniControllerScope.Instance.Cestino.ordine.dataApprovazione = null;
			CestiniControllerScope.Instance.Cestino.ordine.dataInserimento = DateTime.MinValue;
			CestiniControllerScope.Instance.Cestino.ordine.dataStatus = DateTime.MinValue;
			CestiniControllerScope.Instance.Cestino.ordine.statusOrdine = StatusOrdiniEnum.Bozza;
			CestiniControllerScope.Instance.Cestino.ordine.dataOraPasto = DateTime.Now;
			this.CalcTipoDestinatario();

			if ( !conDestinatari )
			{
				CestiniControllerScope.Instance.Cestino.richieste = new List<Richiesta>();
				CestiniControllerScope.Instance.Cestino.richiestaCorrente = new Richiesta();
			}
			else
			{
				CestiniControllerScope.Instance.Cestino.richieste.ForEach( r =>
				{
					r.idRichiesta = -1;
					r.idOrdine = -1;
				} );
				CestiniControllerScope.Instance.Cestino.richiestaCorrente = new Richiesta();
			}

			return PartialView( "~/Views/RaiPlace/Cestini/subpartial/_gestisciCestino.cshtml", CestiniControllerScope.Instance.Cestino );
		}

        #region Data Managment Actions

        /// <summary>
        /// Metodo per l'inserimento di un nuovo ordine
        /// </summary>
        /// <param name="toInsert"></param>
        /// <returns></returns>
        private Ordine InserisciOrdine(Ordine toInsert)
        {
            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    string code = GenerateOrderCode();
                    DateTime oggi = DateTime.Now;

                    // se la data consegna è oggi è un pranzo e la data supera le 9:30 allora
                    // sara' una richiesta in emergenza, lo stato non sarà bozza, ma "In attesa di convalida"
                    DateTime dataConsegnaPrevista = toInsert.dataOraPasto.Date;

                    if (toInsert.dataOraPasto.Date.Equals(oggi.Date) && 
                        toInsert.tipoPasto.Equals("pranzo", StringComparison.InvariantCultureIgnoreCase))
                    {
                        TimeSpan ts = new TimeSpan(9,31,00);
                        TimeSpan diff = oggi.TimeOfDay.Subtract(ts);
						// verifica la possibilità di gestire l'ordine in stato di emergenza
						if ( diff.TotalSeconds > 0 )
						{
							TimeSpan ts2 = new TimeSpan( 12, 30, 00 );
							TimeSpan diff2 = oggi.TimeOfDay.Subtract( ts2 );
							// se l'ora supera le 12:30 allora non è più possibile salvare l'ordine
							if ( diff2.TotalSeconds > 0 )
							{
								throw new Exception( " Non è possibile creare un nuovo ordine per il pranzo di oggi, in quanto fuori tempo massimo per gestirlo in emergenza " );
							}
							toInsert.statusOrdine = StatusOrdiniEnum.AttesaConvalida;
						}// TODO da verificare
						else
						{
							toInsert.statusOrdine = StatusOrdiniEnum.Convalidata;
						}
                    } // Se la data consegna è oggi è una cena e l'orario supera le 16:30,
                        // allora la richiesta sarà gestita come richiesta in emergenza ed il suo stato
                        // sarà "In attesa di convalida"
                    else if (toInsert.dataOraPasto.Date.Equals(oggi.Date) &&
                        toInsert.tipoPasto.Equals("cena", StringComparison.InvariantCultureIgnoreCase))
                    {
                        TimeSpan ts = new TimeSpan(16, 31, 00);
                        TimeSpan diff = oggi.TimeOfDay.Subtract(ts);
						// verifica la possibilità di gestire l'ordine in stato di emergenza
                        if (diff.TotalSeconds > 0)
                        {
							TimeSpan ts2 = new TimeSpan( 19, 00, 00 );
							TimeSpan diff2 = oggi.TimeOfDay.Subtract( ts2 );
							// se l'ora supera le 19:00 allora non è più possibile salvare l'ordine
							if ( diff2.TotalSeconds > 0 )
							{
								throw new Exception( " Non è possibile creare un nuovo ordine per la cena di oggi, in quanto fuori tempo massimo per gestirlo in emergenza " );
							}
                            toInsert.statusOrdine = StatusOrdiniEnum.AttesaConvalida;
						}// TODO da verificare
						else
						{
							toInsert.statusOrdine = StatusOrdiniEnum.Convalidata;
						}
					}
					else if ( toInsert.dataOraPasto.Date > oggi.Date )
					{
						toInsert.statusOrdine = StatusOrdiniEnum.Convalidata;
					}
					else if ( toInsert.dataOraPasto.Date < oggi.Date )
					{
						throw new Exception( "La data di consegna del cestino non può essere antecedente alla data odierna." );
					}
                    
                    B2RaiPlace_Cestini_Ordini tempToInsert = new B2RaiPlace_Cestini_Ordini()
                    {
                        Centro_costo = toInsert.centroCosto,
                        Cespite = toInsert.cespite,
                        Codice_ordine = code,
                        Data_inserimeto = oggi,
                        Data_ora_pasto = toInsert.dataOraPasto,
                        Data_status = oggi,
                        Id_status = (int)toInsert.statusOrdine,
                        Matricola_spettacolo = toInsert.matricolaSpettacolo,
                        Motivo = toInsert.motivoOrdine,
                        Note = toInsert.note,
                        Ora_pasto = toInsert.dataOraPasto.ToString("HH:mm:ss"),
                        Tel_referente = toInsert.telefonoReferente,
                        Tipo_pasto = toInsert.tipoPasto,
                        Titolo_produzione = toInsert.titoloProduzione,
                        Matricola_richiedente = toInsert.matricolaOrdine,
                        Matricola_Referente_consegna = toInsert.matricolaReferenteConsegna,
                        Luogo_consegna = toInsert.luogoConsegna,
                        Matricola_approvatore = null,
                        Struttura = toInsert.struttura,
                        Data_annullamento = null,
                        Data_approvazione = null,
                        Richiedente = toInsert.richiedente,
                        Referente_consegna = toInsert.referenteConsegna,
                        tel_richiedente = toInsert.telefonoRichiedente,
                        Punto_ordinante = toInsert.puntoOrdinante
                    };

                    db.B2RaiPlace_Cestini_Ordini.Add(tempToInsert);

                    TransactionScope scope = new TransactionScope(
                        TransactionScopeOption.Required,
                        new TransactionOptions()
                        {
                            IsolationLevel = IsolationLevel.ReadCommitted
                        });

                    using (scope)
                    {
                        db.SaveChanges();
                        scope.Complete();
                    }

                    toInsert.idOrdine = tempToInsert.Id_ordine;
                    toInsert.codiceOrdine = code;
                    toInsert.dataInserimento = oggi;
                }
                return toInsert;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Modifca di una richiesta di cestino/i esistente
        /// </summary>
        /// <param name="toModify"></param>
        /// <returns></returns>
        private Ordine ModificaOrdine(Ordine toModify)
        {
            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    // reperimento dell'elemento da eliminare
                    var itemToModify = db.B2RaiPlace_Cestini_Ordini.SingleOrDefault(r => r.Id_ordine.Equals(toModify.idOrdine));

                    if (itemToModify != null)
                    {
                        // se l'ordine è ancora in uno degli stati che ne consentono la modifica
                        if (itemToModify.Id_status == (int)StatusOrdiniEnum.Bozza ||
                            itemToModify.Id_status == (int)StatusOrdiniEnum.AttesaConvalida ||
                            itemToModify.Id_status == (int)StatusOrdiniEnum.Convalidata)
                        {
							itemToModify.Data_ora_pasto = toModify.dataOraPasto;
							itemToModify.Data_status = DateTime.Now;
							itemToModify.Id_status = (int)toModify.statusOrdine;
							itemToModify.Tipo_pasto = toModify.tipoPasto;

							DateTime oggi = DateTime.Now;

							// se la data consegna è oggi è un pranzo e la data supera le 9:30 allora
							// sara' una richiesta in emergenza, lo stato non sarà bozza, ma "In attesa di convalida"
							DateTime dataConsegnaPrevista = itemToModify.Data_ora_pasto.Date;

							if ( itemToModify.Data_ora_pasto.Date.Equals( oggi.Date ) &&
								itemToModify.Tipo_pasto.Equals( "pranzo", StringComparison.InvariantCultureIgnoreCase ) )
							{
								TimeSpan ts = new TimeSpan( 9, 31, 00 );
								TimeSpan diff = oggi.TimeOfDay.Subtract( ts );
								// verifica la possibilità di gestire l'ordine in stato di emergenza
								if ( diff.TotalSeconds > 0 )
								{
									TimeSpan ts2 = new TimeSpan( 12, 30, 00 );
									TimeSpan diff2 = oggi.TimeOfDay.Subtract( ts2 );
									// se l'ora supera le 12:30 allora non è più possibile salvare l'ordine
									if ( diff2.TotalSeconds > 0 )
									{
										throw new Exception( " Non è possibile modificare l'ordine per il pranzo di oggi, in quanto fuori tempo massimo per gestirlo in emergenza " );
									}
									itemToModify.Id_status = (int)StatusOrdiniEnum.AttesaConvalida;
								}// TODO da verificare
								else
								{
									itemToModify.Id_status = ( int )StatusOrdiniEnum.Convalidata;
								}
							} // Se la data consegna è oggi è una cena e l'orario supera le 16:30,
							// allora la richiesta sarà gestita come richiesta in emergenza ed il suo stato
							// sarà "In attesa di convalida"
							else if ( itemToModify.Data_ora_pasto.Date.Equals( oggi.Date ) &&
								itemToModify.Tipo_pasto.Equals( "cena", StringComparison.InvariantCultureIgnoreCase ) )
							{
								TimeSpan ts = new TimeSpan( 16, 31, 00 );
								TimeSpan diff = oggi.TimeOfDay.Subtract( ts );
								// verifica la possibilità di gestire l'ordine in stato di emergenza
								if ( diff.TotalSeconds > 0 )
								{
									itemToModify.Id_status = ( int )StatusOrdiniEnum.AttesaConvalida;
								}// TODO da verificare
								else
								{
									TimeSpan ts2 = new TimeSpan( 19, 00, 00 );
									TimeSpan diff2 = oggi.TimeOfDay.Subtract( ts2 );
									// se l'ora supera le 19:00 allora non è più possibile salvare l'ordine
									if ( diff2.TotalSeconds > 0 )
									{
										throw new Exception( " Non è possibile modificare l'ordine per la cena di oggi, in quanto fuori tempo massimo per gestirlo in emergenza " );
									}
									itemToModify.Id_status = ( int )StatusOrdiniEnum.Convalidata;
								}
							}
							else if ( itemToModify.Data_ora_pasto.Date > oggi.Date )
							{
								itemToModify.Id_status = ( int )StatusOrdiniEnum.Convalidata;
							}
							else if ( itemToModify.Data_ora_pasto.Date < oggi.Date )
							{
								throw new Exception( "La data di consegna del cestino non può essere antecedente alla data odierna." );
							}

                            // modifica dei dati
                            itemToModify.Centro_costo = toModify.centroCosto;
                            itemToModify.Cespite = toModify.cespite;
                            itemToModify.Codice_ordine = toModify.codiceOrdine;
                            itemToModify.Matricola_spettacolo = toModify.matricolaSpettacolo;
                            itemToModify.Motivo = toModify.motivoOrdine;
                            itemToModify.Note = toModify.note;
                            itemToModify.Ora_pasto = toModify.dataOraPasto.ToString("HH:mm:ss");
                            itemToModify.Tel_referente = toModify.telefonoReferente;
                            itemToModify.Titolo_produzione = toModify.titoloProduzione;
                            itemToModify.Matricola_richiedente = toModify.matricolaOrdine;
                            itemToModify.Matricola_Referente_consegna = toModify.matricolaReferenteConsegna;
                            itemToModify.Luogo_consegna = toModify.luogoConsegna;
                            itemToModify.Matricola_approvatore = null;
                            itemToModify.Punto_ordinante = toModify.puntoOrdinante;
                            itemToModify.Struttura = toModify.struttura;
                            itemToModify.Data_annullamento = null;
                            itemToModify.Data_approvazione = null;
                            itemToModify.Richiedente = toModify.richiedente;
                            itemToModify.Referente_consegna = toModify.referenteConsegna;
                            itemToModify.tel_richiedente = toModify.telefonoRichiedente;

                            if (CestiniControllerScope.Instance.Cestino != null &&
                                CestiniControllerScope.Instance.Cestino.richieste != null &&
                                CestiniControllerScope.Instance.Cestino.richieste.Any())
                            {

								// reperimento degli elementi marcati come deleted
								var toRemove = CestiniControllerScope.Instance.Cestino.richieste.Where( r => r.idRichiesta > 0 && r.Deleted ).ToList();
								// se ci sono elementi marcati come deleted allora verra ciclata la lista e tali elementi verranno rimossi dal db
								if ( toRemove != null )
								{
									toRemove.ForEach( r =>
									{
										RimuoviRichiestaInternal( r.idRichiesta );
									} );
								}

								// reperimento della lista di elementi da inserire
								var toInsert = CestiniControllerScope.Instance.Cestino.richieste.Where( r => !r.Deleted ).ToList();
								// se ci sono elementi da inserire la lista verra ciclata e verranno effettuate le insert sul db
								if ( toInsert != null )
								{
									toInsert.ForEach( o =>
									{
										if ( !this.CanInsertRichiesta( CestiniControllerScope.Instance.Cestino.ordine, o ) )
										{
											string err = String.Empty;
											if ( String.IsNullOrEmpty( o.matricolaRisorsa ) )
											{
												err = String.Format( " Per la data di consegna indicata risulta già presente una richiesta di cestino per l'utente {0} {1} ", o.cognomeRisorsa, o.nomeRisorsa );
											}
											else
											{
												err = String.Format( " Per la data di consegna indicata risulta già presente una richiesta di cestino per l'utente {0} {1} ({2})", o.cognomeRisorsa, o.nomeRisorsa, o.matricolaRisorsa );
											}
											throw new ApplicationException( err );
										}

										string code = null;
										// modifica richiesta esistente
										if ( o.idRichiesta > 0 )
										{
											var richiesta = db.B2RaiPlace_Cestini_Richieste.SingleOrDefault( r => r.Id_richiesta.Equals( o.idRichiesta ) );
											if ( richiesta != null )
											{
												// verifica se è il caso di ricalcolare il codice assegnato alla richiesta
												if ( ( !itemToModify.Cespite.Equals( toModify.cespite, StringComparison.InvariantCultureIgnoreCase ) ) || // cambiato il cespite
													( ( o.matricolaRisorsa != null ) && ( !o.matricolaRisorsa.Equals( richiesta.Matricola_risorsa, StringComparison.InvariantCultureIgnoreCase ) ) ) || // cambiato l'utente
													( ( int )o.tipoCestino != richiesta.Id_tipo_cestino ) || // cambiata la tipologia di cestino
													( itemToModify.Tipo_pasto != toModify.tipoPasto ) // cambiata la tipologia di pasto
													)
												{
													// ricalcolo del codice in quanto è cambiato qualcosa per il quale il codice precedente non è più corretto
													code = GenerateReqCode( itemToModify.Codice_ordine, itemToModify.Cespite, o );
													richiesta.Codice_richiesta = o.codiceRichiesta;
												}
												else
												{
													richiesta.Codice_richiesta = o.codiceRichiesta;
												}

												richiesta.Cognome_risorsa = o.cognomeRisorsa;
												richiesta.Data_inserimento = DateTime.Now;
												richiesta.Flag_tipo_risorsa = o.flagRisorsa;
												richiesta.Id_ordine = o.idOrdine;
												richiesta.Matricola_risorsa = o.matricolaRisorsa;
												richiesta.Motivo_esterno = o.motivoEsterno;
												richiesta.Nome_risorsa = o.nomeRisorsa;
												richiesta.Progressivo = o.progressivo;
												richiesta.Id_tipo_cestino = ( int )o.tipoCestino;
												richiesta.Note_richiesta = null;
											}
										}
										else
										{
											// Inserimento nuova richiesta
											code = GenerateReqCode( itemToModify.Codice_ordine, itemToModify.Cespite, o );

											db.B2RaiPlace_Cestini_Richieste.Add( new B2RaiPlace_Cestini_Richieste()
											{
												Codice_richiesta = code,
												Cognome_risorsa = o.cognomeRisorsa,
												Data_inserimento = DateTime.Now,
												Flag_tipo_risorsa = o.flagRisorsa,
												Id_ordine = itemToModify.Id_ordine,
												Matricola_risorsa = o.matricolaRisorsa,
												Motivo_esterno = o.motivoEsterno,
												Nome_risorsa = o.nomeRisorsa,
												Progressivo = o.progressivo,
												Id_tipo_cestino = ( int )o.tipoCestino,
												Note_richiesta = null
											} );
										}
									} );
								}
                            }
                        }
                        else
                        {
                            throw new ApplicationException("Impossibile modificare l'ordine selezionato.\nPotrebbe aver assunto uno stato che non consente modifiche.");
                        }
                    }
                    else
                    {
                        throw new ApplicationException("Non è stato trovato l'ordine da modificare");
                    }

                    TransactionScope scope = new TransactionScope(
                        TransactionScopeOption.Required,
                        new TransactionOptions()
                        {
                            IsolationLevel = IsolationLevel.ReadCommitted
                        });

                    using (scope)
                    {
                        db.SaveChanges();
                        scope.Complete();
                    }
                }
                return toModify;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Reperimento dei dati di un determinato cestino
        /// </summary>
        /// <param name="idOrdine"></param>
        /// <returns></returns>
        private CestiniModel GetOrdine(int idOrdine)
        {
            CestiniModel result = new CestiniModel();
            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    var cestino = db.B2RaiPlace_Cestini_Ordini.Where(c => c.Id_ordine.Equals(idOrdine)).FirstOrDefault();

                    if (cestino != null)
                    {
                        string descrizioneReferente = "";
                        string descrizioneRichiedente = "";
                        it.rai.servizi.hrgb.Service wsAnag = new it.rai.servizi.hrgb.Service();

                        DateTime _dateTime = DateTime.ParseExact(cestino.Ora_pasto, "HH:mm:ss",
                                        CultureInfo.InvariantCulture);

                        DateTime dataPasto = new DateTime(cestino.Data_ora_pasto.Year, cestino.Data_ora_pasto.Month,
                                                            cestino.Data_ora_pasto.Day, _dateTime.Hour, _dateTime.Minute, _dateTime.Millisecond);

                        result.ordine = new Ordine()
                        {
                            approvatore = cestino.Approvatore,
                            centroCosto = cestino.Centro_costo,
                            cespite = cestino.Cespite,
                            codiceOrdine = cestino.Codice_ordine,
                            dataOraPasto = dataPasto,
                            dataStatus = cestino.Data_status,
                            idOrdine = cestino.Id_ordine,
                            matricolaOrdine = cestino.Matricola_richiedente,
                            matricolaSpettacolo = cestino.Matricola_spettacolo,
                            motivoOrdine = cestino.Motivo,
                            note = cestino.Note,
                            matricolaReferenteConsegna = cestino.Matricola_Referente_consegna,
                            referenteConsegna = cestino.Referente_consegna,
                            statusOrdine = (StatusOrdiniEnum)cestino.B2RaiPlace_Cestini_Status_Ordini.Id,
                            telefonoReferente = cestino.Tel_referente,
                            tipoPasto = cestino.Tipo_pasto,
                            titoloProduzione = cestino.Titolo_produzione,
                            dataApprovazione = cestino.Data_approvazione,
                            luogoConsegna = cestino.Luogo_consegna,
                            dataInserimento = cestino.Data_inserimeto,
                            puntoOrdinante = cestino.Punto_ordinante,
                            richiedente = cestino.Richiedente,
                            telefonoRichiedente = cestino.tel_richiedente,
                            struttura = cestino.Struttura
                        };

                        // se il referente consegna non è valorizzato allora lo 
                        // reperiamo attraverso il servizio
                        if (String.IsNullOrEmpty(result.ordine.referenteConsegna) &&
                            !String.IsNullOrEmpty(cestino.Matricola_Referente_consegna))
                        {                            
                            var elenco = wsAnag.Get_RicercaAnagrafica_Net("CESTINI", cestino.Matricola_Referente_consegna, "", "", "", "false");
                            var rr = elenco.DT_RicercaAnagrafica.Rows;

                            if (elenco.DT_RicercaAnagrafica.Rows != null &&
                                elenco.DT_RicercaAnagrafica.Rows.Count > 0)
                            {
                                var item = elenco.DT_RicercaAnagrafica.Rows[0];
                                descrizioneReferente = item[1].ToString() + " " + item[2].ToString();
                                result.ordine.referenteConsegna = descrizioneReferente;
                            }
                        }

                        // se il richiedente ordine non è valorizzato allora lo 
                        // reperiamo attraverso il servizio
                        if (String.IsNullOrEmpty(result.ordine.richiedente) &&
                            !String.IsNullOrEmpty(cestino.Matricola_richiedente))
                        {
                            var elenco = wsAnag.Get_RicercaAnagrafica_Net("CESTINI", cestino.Matricola_richiedente, "", "", "", "false");
                            var rr = elenco.DT_RicercaAnagrafica.Rows;

                            if (elenco.DT_RicercaAnagrafica.Rows != null &&
                                elenco.DT_RicercaAnagrafica.Rows.Count > 0)
                            {
                                var item = elenco.DT_RicercaAnagrafica.Rows[0];
                                descrizioneRichiedente = item[1].ToString() + " " + item[2].ToString();
                                result.ordine.richiedente = descrizioneRichiedente;
                            }
                        }

                        //reperimento della lista di richieste per quel cestino
                        var req = db.B2RaiPlace_Cestini_Richieste.Where(r => r.Id_ordine.Equals(idOrdine)).ToList();

                        if (req != null)
                        {
                            result.richieste = new List<Richiesta>();
                            req.ForEach(r =>
                            {
                                result.richieste.Add(new Richiesta()
                                {
                                    codiceRichiesta = r.Codice_richiesta,
                                    cognomeRisorsa = r.Cognome_risorsa,
                                    dataInserimento = r.Data_inserimento,
                                    flagRisorsa = r.Flag_tipo_risorsa,
                                    idOrdine = r.Id_ordine,
                                    idRichiesta = r.Id_richiesta,
                                    matricolaRisorsa = r.Matricola_risorsa,
                                    motivoEsterno = r.Motivo_esterno,
                                    nomeRisorsa = r.Nome_risorsa,
                                    progressivo = r.Progressivo,
                                    tipoCestino = (TipoCestinoEnum)r.Id_tipo_cestino
                                });
                            });
                        }

                        result.richiestaCorrente = new Richiesta();
                    }

                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Metodo che si occupa della rimozione di un ordine e delle richieste che lo compongono
        /// </summary>
        /// <param name="idOrdine"></param>
        private void RimuoviOrdineInternal(int idOrdine)
        {
            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    // reperimento dell'elemento da eliminare
                    var itemToRemove = db.B2RaiPlace_Cestini_Ordini.SingleOrDefault(r => r.Id_ordine.Equals(idOrdine));

                    if (itemToRemove != null)
                    {
                        // se l'ordine è ancora in uno degli stati che ne consentono la rimozione
                        if (itemToRemove.Id_status == (int)StatusOrdiniEnum.Bozza ||
                            itemToRemove.Id_status == (int)StatusOrdiniEnum.AttesaConvalida ||
                            itemToRemove.Id_status == (int)StatusOrdiniEnum.Convalidata)
                        {
                            // reperimento delle singole richieste che compongono l'ordine
                            var list = db.B2RaiPlace_Cestini_Richieste.Where(r => r.Id_ordine.Equals(idOrdine)).ToList();

                            // rimozione di tutte le richieste che fanno parte dell'ordine
                            if (list != null)
                                list.ForEach(i =>
                                {
                                    var child = db.B2RaiPlace_Cestini_Richieste.SingleOrDefault(r => r.Id_richiesta.Equals(i.Id_richiesta));
                                    db.B2RaiPlace_Cestini_Richieste.Remove(child);
                                });

                            // rimozione dell'ordine
                            db.B2RaiPlace_Cestini_Ordini.Remove(itemToRemove);

                            // salvataggio delle modifiche
                            db.SaveChanges();
                        }
                        else
                        {
                            throw new ApplicationException("Impossibile eliminare la richiesta selezionata.\nIl cestino potrebbe aver assunto uno stato che non consente modifiche.");
                        }
                    }
                    else
                    {
                        throw new ApplicationException("Non è stato trovato l'ordine da rimuovere");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

		/// <summary>
		/// Verifica se la richiesta può essere aggiunta all'elenco delle richieste
		/// da salvare.
		/// Verifica se non è stata già effettuata una richiesta per l'utente per lo stesso
		/// giorno e per la stessa tipologia (pranzo o cena)
		/// </summary>
		/// <param name="ordine"></param>
		/// <param name="richiesta"></param>
		/// <returns></returns>
		private bool CanInsertRichiesta ( Ordine ordine, Richiesta richiesta )
		{
			DateTime data = ordine.dataOraPasto;
			string tipoPasto = ordine.tipoPasto;
			bool result = false;

			try
			{
				using ( digiGappEntities db = new digiGappEntities() )
				{
					var richieste = db.B2RaiPlace_Cestini_Richieste
						.Join( db.B2RaiPlace_Cestini_Ordini,
							r => r.Id_ordine,
							o => o.Id_ordine,
							( r, o ) => new { Richiesta = r, Ordine = o } )
						.Where( o => o.Ordine.Data_ora_pasto.Equals( data.Date ) &&
								o.Ordine.Tipo_pasto.Equals( tipoPasto, StringComparison.InvariantCultureIgnoreCase ) && o.Richiesta.Id_richiesta != richiesta.idRichiesta );

					if ( richieste != null && richieste.Any() )
					{
						if ( !String.IsNullOrEmpty( richiesta.matricolaRisorsa ) )
						{
							var exists = richieste.Where( r => r.Richiesta.Matricola_risorsa.Equals( richiesta.matricolaRisorsa, StringComparison.InvariantCultureIgnoreCase ) ).ToList();

							if ( exists != null && exists.Any() )
							{
								result = false;
							}
							else
							{
								result = true;
							}
						}
						else
						{
							var exists = richieste.Where( r =>
								r.Richiesta.Cognome_risorsa.Equals( richiesta.cognomeRisorsa, StringComparison.InvariantCultureIgnoreCase ) &&
								r.Richiesta.Nome_risorsa.Equals( richiesta.nomeRisorsa, StringComparison.InvariantCultureIgnoreCase )
								).ToList();

							if ( exists != null && exists.Any() )
							{
								result = false;
							}
							else
							{
								result = true;
							}
						}
					}
					else
					{
						result = true;
					}
				}

				return result;
			}
			catch ( Exception ex )
			{
				throw new Exception( ex.Message );
			}
		}

        #endregion

        #endregion

        #region Gestione Richieste

        /// <summary>
        /// Inserimento o modifica di una richiesta
        /// </summary>
        /// <param name="richiestaCorrente">Oggetto richiesta</param>
        /// <returns>Esito dell'operazione. True o False</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult CreaModificaRichiesta(Richiesta richiestaCorrente)
        {
            try
            {
                /*
                 * Req.04 – Modifica nuovo ordine
                 * Il sistema, a seguito dell’inserimento di un nuovo ordine da parte dell’utente User, 
                 * deve consentire al suddetto utente di modificare tale ordine, 
                 * modificando o i parametri dell’ordine stesso e/o quelli relativi alle singole richieste 
                 * in esso contenute. 
                 * Tale operazione è consentita al solo utente User inseritore 
                 * e solo qualora l’ordine si trovi ancora nello stato
                 * •	Bozza
                 * •	In attesa di convalida
                 * •	Convalidato
                 * Al contrario, gli ordini identificati dallo stato In lavorazione o Consuntivato 
                 * non potranno essere in alcun modo modificati da parte degli utenti.
                 */
                if ((CestiniControllerScope.Instance.Cestino.ordine.statusOrdine == StatusOrdiniEnum.Bozza) ||
                    (CestiniControllerScope.Instance.Cestino.ordine.statusOrdine == StatusOrdiniEnum.AttesaConvalida) ||
                    (CestiniControllerScope.Instance.Cestino.ordine.statusOrdine == StatusOrdiniEnum.Convalidata))
                {

					if ( !this.CanInsertRichiesta( CestiniControllerScope.Instance.Cestino.ordine, richiestaCorrente ) )
					{
						throw new ApplicationException( " Per la data di consegna indicata risulta già presente una richiesta di cestino per l'utente selezionato " );
					}

                    if (richiestaCorrente.idRichiesta == 0)
                    {
                        // assegna un identificativo temporaneo alla richiesta,
                        // così da distinguere le richieste non ancora salvate
                        // con quelle già presenti sul db
                        richiestaCorrente.idRichiesta = this.GetTempId();
                        //richiestaCorrente.progressivo = richiestaCorrente.idRichiesta * (-1);
                        richiestaCorrente.progressivo = this.GetProgressivo();
                    }

                    if (CestiniControllerScope.Instance.Cestino.richieste.Any())
                    {
                        var toEdit = CestiniControllerScope.Instance.Cestino.richieste.Where(r => r.idRichiesta == richiestaCorrente.idRichiesta).FirstOrDefault();
                        if (toEdit == null)
						{
							if ( !String.IsNullOrEmpty( richiestaCorrente.matricolaRisorsa ) )
							{
								var element = CestiniControllerScope.Instance.Cestino.richieste.Where( r => r.matricolaRisorsa != null && r.matricolaRisorsa.Trim().Equals( richiestaCorrente.matricolaRisorsa.Trim(), StringComparison.InvariantCultureIgnoreCase ) ).FirstOrDefault();

								if ( element != null && !element.Deleted )
								{
									throw new ApplicationException( "Utente già presente nella lista" );
								}
							}
							else
							{
								var element = CestiniControllerScope.Instance.Cestino.richieste.Where( r => r.cognomeRisorsa.Trim().Equals( richiestaCorrente.cognomeRisorsa.Trim(), StringComparison.InvariantCultureIgnoreCase ) && r.nomeRisorsa.Trim().Equals( richiestaCorrente.nomeRisorsa.Trim(), StringComparison.InvariantCultureIgnoreCase ) ).FirstOrDefault();

								if ( element != null )
								{
									throw new ApplicationException( "Utente già presente nella lista" );
								}
							}

							// verifica che non venga inserita una doppia richiesta per il singolo utente
							CestiniControllerScope.Instance.Cestino.richieste.Add( richiestaCorrente );
						}
                        else
                        {
                            toEdit.codiceRichiesta = richiestaCorrente.codiceRichiesta;
                            toEdit.cognomeRisorsa = richiestaCorrente.cognomeRisorsa;
                            // valore non riportato correttamente
                            //toEdit.dataInserimento = richiestaCorrente.dataInserimento;
                            toEdit.flagRisorsa = richiestaCorrente.flagRisorsa;
                            toEdit.idOrdine = richiestaCorrente.idOrdine;
                            toEdit.idRichiesta = richiestaCorrente.idRichiesta;
                            toEdit.matricolaRisorsa = richiestaCorrente.matricolaRisorsa;
                            toEdit.motivoEsterno = richiestaCorrente.motivoEsterno;
                            toEdit.nomeRisorsa = richiestaCorrente.nomeRisorsa;
                            toEdit.progressivo = richiestaCorrente.progressivo;
                            toEdit.tipoCestino = richiestaCorrente.tipoCestino;
                        }
                    }
                    else
                    {
                        CestiniControllerScope.Instance.Cestino.richieste.Add(richiestaCorrente);
                    }
                    CestiniControllerScope.Instance.Cestino.richiestaCorrente = richiestaCorrente;

					return Json( new
					{
						success = true,
						errorMessage = String.Empty,
						richiestaCorrente = richiestaCorrente
					} );
                }
                else
                {
                    throw new ApplicationException("Impossibile completare l'operazione desiderata.\nIl cestino potrebbe aver assunto uno stato che non consente modifiche.");
                }
            }
            catch (Exception ex)
            {

				return Json( new { 
					success = false,
					errorMessage = ex.Message,
					richiestaCorrente = String.Empty
				} );
            }
        }

        /// <summary>
        /// Metodo per la rimozione di una richiesta.
        /// </summary>
        /// <param name="idRichiesta">Identificativo della richiesta da rimuovere</param>
        /// <returns>Esito dell'operazione. True o False</returns>
        public JsonResult RimuoviRichiesta(int idRichiesta)
        {
            try
            {
                /*
                 * Req.04 – Modifica nuovo ordine
                 * Il sistema, a seguito dell’inserimento di un nuovo ordine da parte dell’utente User, 
                 * deve consentire al suddetto utente di modificare tale ordine, 
                 * modificando o i parametri dell’ordine stesso e/o quelli relativi alle singole richieste 
                 * in esso contenute. 
                 * Tale operazione è consentita al solo utente User inseritore 
                 * e solo qualora l’ordine si trovi ancora nello stato
                 * •	Bozza
                 * •	In attesa di convalida
                 * •	Convalidato
                 * Al contrario, gli ordini identificati dallo stato In lavorazione o Consuntivato 
                 * non potranno essere in alcun modo modificati da parte degli utenti.
                 */
                if (CestiniControllerScope.Instance.Cestino.richieste.Any() &&
                    ((CestiniControllerScope.Instance.Cestino.ordine.statusOrdine == StatusOrdiniEnum.Bozza) ||
                    (CestiniControllerScope.Instance.Cestino.ordine.statusOrdine == StatusOrdiniEnum.AttesaConvalida) ||
                    (CestiniControllerScope.Instance.Cestino.ordine.statusOrdine == StatusOrdiniEnum.Convalidata))
                    )
                {
                    var toRemove = CestiniControllerScope.Instance.Cestino.richieste.Where(r => r.idRichiesta == idRichiesta).FirstOrDefault();
                    if (toRemove == null)
                        return Json(new { success = false });
                    else
                    {
                        // operazione di rimozione della richiesta
						if ( toRemove.idRichiesta <= 0 )
							CestiniControllerScope.Instance.Cestino.richieste.Remove( toRemove );    // è una richiesta ancora non salvata
						else
						{
							//RimuoviRichiestaInternal(idRichiesta);  // richiesta creata in precedenza, 
							//										// quindi da rimuovere dal db
							var itm = CestiniControllerScope.Instance.Cestino.richieste.Where( r => r.idRichiesta.Equals( toRemove.idRichiesta ) ).FirstOrDefault();
							itm.Deleted = true;
						}
                    }
                }
                else
                {
                    throw new ApplicationException("Impossibile eliminare la richiesta selezionata.\nIl cestino potrebbe aver assunto uno stato che non consente modifiche.");
                }

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Reperimento dei dati per la modifica di una richiesta
        /// </summary>
        /// <param name="idRichiesta">Identificativo univoco della richiesta da modificare</param>
        /// <returns>Dati in formato json della richiesta da modificare</returns>
        public JsonResult DettaglioRichiesta(int idRichiesta)
        {
            this.InitializeViewBagInformation();

            if (CestiniControllerScope.Instance.Cestino != null &&
                CestiniControllerScope.Instance.Cestino.richieste != null &&
                CestiniControllerScope.Instance.Cestino.richieste.Any())
            {
                var toEdit = CestiniControllerScope.Instance.Cestino.richieste.Where(r => r.idRichiesta.Equals(idRichiesta)).FirstOrDefault();

                if (toEdit == null)
                    throw new Exception("Impossibile reperire i dati della richiesta desiderata.");
                else
                {
                    CestiniControllerScope.Instance.Cestino.richiestaCorrente = toEdit;
                }

                return Json(toEdit, JsonRequestBehavior.AllowGet);
            }
            else
            {
                throw new Exception("Impossibile reperire i dati della richiesta desiderata.");
            }
        }

        #region Data Managment Actions

        /// <summary>
        /// Metodo per l'inserimento di una singola richiesta
        /// </summary>
        /// <param name="richiesta"></param>
        /// <returns></returns>
        private Richiesta InserisciRichiesta(Richiesta richiesta)
        {
            try
            {
				if ( !this.CanInsertRichiesta( CestiniControllerScope.Instance.Cestino.ordine, richiesta ) )
				{
					string err = String.Empty;
					if ( String.IsNullOrEmpty( richiesta.matricolaRisorsa ) )
					{
						err = String.Format( " Per la data di consegna indicata risulta già presente una richiesta di cestino per l'utente {0} {1} ", richiesta.cognomeRisorsa, richiesta.nomeRisorsa );
					}
					else
					{
						err = String.Format( " Per la data di consegna indicata risulta già presente una richiesta di cestino per l'utente {0} {1} ({2})", richiesta.cognomeRisorsa, richiesta.nomeRisorsa, richiesta.matricolaRisorsa );
					}
					throw new ApplicationException( err );
				}

                using (digiGappEntities db = new digiGappEntities())
                {
                    string code = GenerateReqCode(CestiniControllerScope.Instance.Cestino.ordine.codiceOrdine,
                                                    CestiniControllerScope.Instance.Cestino.ordine.cespite, richiesta);

                    B2RaiPlace_Cestini_Richieste tempToInsert = new B2RaiPlace_Cestini_Richieste()
                    {
                        Codice_richiesta = code,
                        Cognome_risorsa = richiesta.cognomeRisorsa.ToUpper(),
                        Data_inserimento = DateTime.Now,
                        Flag_tipo_risorsa = richiesta.flagRisorsa,
                        Id_ordine = richiesta.idOrdine,
                        Matricola_risorsa = richiesta.matricolaRisorsa,
                        Motivo_esterno = richiesta.motivoEsterno,
                        Nome_risorsa = richiesta.nomeRisorsa.ToUpper(),
                        Progressivo = richiesta.progressivo,
                        Id_tipo_cestino = (int)richiesta.tipoCestino,
                        Note_richiesta = null
                    };

                    db.B2RaiPlace_Cestini_Richieste.Add(tempToInsert);

                    TransactionScope scope = new TransactionScope(
                        TransactionScopeOption.Required,
                        new TransactionOptions()
                        {
                            IsolationLevel = IsolationLevel.ReadCommitted
                        });

                    using (scope)
                    {
                        db.SaveChanges();
                        scope.Complete();
                    }

                    richiesta.idRichiesta = tempToInsert.Id_richiesta;
                }
                return richiesta;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Rimozione di una richiesta dal db
        /// </summary>
        /// <param name="idRichiesta">Identificativo univoco della richiesta da rimuovere</param>
        private void RimuoviRichiestaInternal(int idRichiesta)
        {
            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    // reperimento dell'elemento da eliminare
                    var itemToRemove = db.B2RaiPlace_Cestini_Richieste.SingleOrDefault(r => r.Id_richiesta == idRichiesta);

                    if (itemToRemove != null)
                    {
                        // rimozione dell'elemento
                        db.B2RaiPlace_Cestini_Richieste.Remove(itemToRemove);
                        // salvataggio delle modifiche
                        db.SaveChanges();

						var itm = CestiniControllerScope.Instance.Cestino.richieste.Where( r => r.idRichiesta.Equals( idRichiesta ) ).FirstOrDefault();

						itm.Deleted = true;
                    }
                    else
                    {
                        throw new ApplicationException("Non è stata trovata la richiesta da eliminare");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        
        #endregion

        #endregion

        #region Data Managment Actions

        /// <summary>
        /// Reperimento dei Cespiti
        /// </summary>
        /// <param name="cespite"></param>
        /// <returns></returns>
        public static List<System.Web.UI.WebControls.ListItem> GetCespiti(string cespite = null)
        {
            List<System.Web.UI.WebControls.ListItem> result = new List<System.Web.UI.WebControls.ListItem>();
            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    // reperimento dell'elenco dei cespiti per la zona di appartenenza dell'utente corrente
                    var list = db.B2RaiPlace_Cespiti_Tipologie.Where(c => c.Citta.Equals(CestiniControllerScope.Instance.Cestino.ordine.struttura, StringComparison.InvariantCultureIgnoreCase)).ToList();

                    if (list != null)
                    {
                        list.ForEach(l =>
                        {
                            bool selected = false;
                            
                            if (!String.IsNullOrEmpty(cespite) &&
                                cespite.Equals(l.Codice, StringComparison.InvariantCultureIgnoreCase))
                            {
                                selected = true;
                            }

                            result.Add(new System.Web.UI.WebControls.ListItem()
                            {
                                Value = l.Codice,
                                Text= l.Descrizione,
                                Selected = selected
                            });
                        });
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Ricerca autocomplete, ricerca un utente a partire
        /// dal suo cognome
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetCognomiRisorsa()
        {
			string cog = Request.Params["cog"];
			List<AnagSelect> lista = new List<AnagSelect>();

			// Se la parola cercata è uguale alla precedente
			// Oppure la vecchia parola cercata è parte della nuova + 1 char (es. vecchia "BIF" nuova "BIFA")
			// e la lista è già stata popolata per la vecchia parola ("BIF"), allora verranno filtrati i 
			// risultati già ottenuti senza dover chiamare nuovamente il servizio

			if ( !String.IsNullOrEmpty( cog ) &&
				cog.Equals( CestiniControllerScope.Instance.FiltroRicercaUtenti, StringComparison.InvariantCultureIgnoreCase ) &&
				( CestiniControllerScope.Instance.CestiniAnagraficaUtenti != null &&
				CestiniControllerScope.Instance.CestiniAnagraficaUtenti.Any() ) )
			{
				CestiniControllerScope.Instance.CestiniAnagraficaUtenti.ForEach( r =>
				{
					AnagSelect anag = new AnagSelect()
					{
						id = r.Matricola,
						text = r.Cognome + "/" + r.Nome + "/" + r.Direzione,
						matricola = r.Matricola
					};

					lista.Add( anag );
				} );
			}
			else if ( !String.IsNullOrEmpty( cog ) &&
						cog.StartsWith( CestiniControllerScope.Instance.FiltroRicercaUtenti ) &&
						( CestiniControllerScope.Instance.CestiniAnagraficaUtenti != null &&
						  CestiniControllerScope.Instance.CestiniAnagraficaUtenti.Any() ) &&
						cog.Length > CestiniControllerScope.Instance.FiltroRicercaUtenti.Length )
			{
				List<CestiniAnagraficaUtente> subList = CestiniControllerScope.Instance.CestiniAnagraficaUtenti.Where( i => i.Nominativo.ToUpper().Contains( cog.ToUpper() ) ).ToList();

				if ( subList != null && subList.Any() )
				{
					CestiniControllerScope.Instance.CestiniAnagraficaUtenti = new List<CestiniAnagraficaUtente>();

					subList.ForEach( r =>
					{
						CestiniControllerScope.Instance.CestiniAnagraficaUtenti.Add( new CestiniAnagraficaUtente()
						{
							Matricola = r.Matricola,
							Nome = r.Nome,
							Cognome = r.Cognome,
							Direzione = r.Direzione,
							Nominativo = r.Nominativo
						} );

						AnagSelect anag = new AnagSelect()
						{
							id = r.Matricola,
							text = r.Cognome + "/" + r.Nome + "/" + r.Direzione,
							matricola = r.Matricola
						};

						lista.Add( anag );
					} );
				}
			}
			else
			{
				using (cv_ModelEntities db = new cv_ModelEntities())
				{
					var rows = db.sp_RicercaUtenti( cog ).ToList();

					if ( rows != null && rows.Any() )
					{
						CestiniControllerScope.Instance.CestiniAnagraficaUtenti = new List<CestiniAnagraficaUtente>();

						rows.ForEach( r =>
						{
							if (!( String.IsNullOrEmpty( r.Cognome ) ||
								String.IsNullOrEmpty( r.Nome ) ||
								String.IsNullOrEmpty( r.Sezione ) ||
								String.IsNullOrEmpty( r.Descrizione_Sezione ) ||
								String.IsNullOrEmpty( r.Descrizione_Servizio ) ))
							{
								AnagSelect anag = new AnagSelect()
								{
									id = r.Matricola,
									text = r.Cognome + "/" + r.Nome + "/" + r.Descrizione_Servizio,
									matricola = r.Matricola
								};

								lista.Add( anag );
								CestiniControllerScope.Instance.CestiniAnagraficaUtenti.Add( new CestiniAnagraficaUtente()
								{
									Matricola = r.Matricola,
									Nome = r.Nome,
									Cognome = r.Cognome,
									Direzione = r.Descrizione_Sezione,
									Nominativo = String.Format( "{0} {1}", r.Cognome, r.Nome )
								} );
							}
						} );
					}
				}
				

				//// altrimenti richiama il servizio
				//it.rai.servizi.hrgb.Service wsAnag = new it.rai.servizi.hrgb.Service();
				//var elenco = wsAnag.Get_RicercaAnagrafica( "CESTINI", "", "", "", cog );

				//List<string> rows = elenco.Split( '|' ).ToList();

				//if ( rows != null && rows.Any() )
				//{
				//	CestiniControllerScope.Instance.CestiniAnagraficaUtenti = new List<CestiniAnagraficaUtente>();

				//	rows.ForEach( r =>
				//	{
				//		List<string> element = r.Split( ';' ).ToList();

				//		AnagSelect anag = new AnagSelect()
				//		{
				//			id = element[0].ToString(),
				//			text = element[2].ToString() + "/" + element[1].ToString() + "/" + element[3].ToString(),
				//			matricola = element[0].ToString()
				//		};

				//		lista.Add( anag );
				//		CestiniControllerScope.Instance.CestiniAnagraficaUtenti.Add( new CestiniAnagraficaUtente()
				//		{
				//			Matricola = element[0].ToString(),
				//			Nome = element[1].ToString(),
				//			Cognome = element[2].ToString(),
				//			Direzione = element[3].ToString(),
				//			Nominativo = String.Format( "{0} {1}", element[2].ToString(), element[1].ToString() )
				//		} );
				//	} );
				//}
			}

			CestiniControllerScope.Instance.FiltroRicercaUtenti = cog;

			return new JsonResult
			{
				JsonRequestBehavior = JsonRequestBehavior.AllowGet,
				Data = new { result = lista }
			};

			//var rr = elenco..DT_RicercaAnagrafica.Rows;

			//List<AnagSelect> lista = new List<AnagSelect>();
			//foreach ( System.Data.DataRow item in elenco.DT_RicercaAnagrafica.Rows )
			//{
			//	AnagSelect anag = new AnagSelect()
			//	{
			//		id = item[0].ToString(),
			//		text = item[1].ToString() + "/" + item[2].ToString() + "/" + item[3].ToString(),
			//		matricola = item[0].ToString()
			//	};

			//	lista.Add( anag );
			//}
			//return new JsonResult
			//{
			//	JsonRequestBehavior = JsonRequestBehavior.AllowGet,
			//	Data = new { result = lista }
			//};
		}

        /// <summary>
        /// Reperimento di un utente a partire dalla matricola
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult GetUtenteByMatricola(string term)
        {
            it.rai.servizi.hrgb.Service wsAnag = new it.rai.servizi.hrgb.Service();
            var elenco = wsAnag.Get_RicercaAnagrafica_Net("CESTINI", term, "", "", "", "false");
            var rr = elenco.DT_RicercaAnagrafica.Rows;

            List<AnagSelect> lista = new List<AnagSelect>();
            foreach (System.Data.DataRow item in elenco.DT_RicercaAnagrafica.Rows)
            {
                AnagSelect anag = new AnagSelect()
                {
                    id = item[0].ToString(),
                    text = item[1].ToString() + "/" + item[2].ToString(),
                    matricola = item[0].ToString()
                };

                lista.Add(anag);
            }

            return Json(lista, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Reperimento dei dati di una produzione a partire dal titolo
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult GetProduzioneByTitolo(string term)
        {
            List<RicercaProgrammaResult> lista = new List<RicercaProgrammaResult>();
            try
            {
                it.rai.servizi.anagraficaws1.APWS sr = new it.rai.servizi.anagraficaws1.APWS();
                sr.Credentials = new System.Net.NetworkCredential(@"srvanapro", "bc14a3", "RAI");
                it.rai.servizi.anagraficaws1.ObjTVRicercaAnagrafieResult res = new it.rai.servizi.anagraficaws1.ObjTVRicercaAnagrafieResult();

                it.rai.servizi.anagraficaws1.ObjInputRicercaTitolo ricercatitolo = new it.rai.servizi.anagraficaws1.ObjInputRicercaTitolo();
                //ricercatitolo.LivelloDiRicerca = anagraficaws.LivelloDiRicerca.Matricola;

                ricercatitolo.modalitaDiRicerca = it.rai.servizi.anagraficaws1.ModalitaDiRicercaStringa.IniziaCon;
                ricercatitolo.CodiceAnagraf = it.rai.servizi.anagraficaws1.CodiceAnagrafico.Programmi;
                //ricercatitolo.DirittiInVita = true;
                ricercatitolo.StatiInVita = true;
                ricercatitolo.Titolo = term;
                res = sr.TvRicercaAnagrafiaTitolo(ricercatitolo);

                var results = res.RisultatoTVRicercaAnagrafie.ToList();

                if (results != null && results.Any())
                {
                    results.ForEach(p =>
                    {
                        lista.Add(new RicercaProgrammaResult()
                        {
                            Matricola = p.MATRICOLA,
                            Titolo = p.TITOLO_DEFINIT,
                            UORG = p.UORG
                        });
                    });
                }

                return PartialView("~/Views/RaiPlace/Cestini/subpartial/_searchProduzioneResults.cshtml", lista);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Reperimento dei dati di una produzione a partire dal titolo
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult GetProduzioneByMatricola(string term)
        {
            List<RicercaProgrammaResult> lista = new List<RicercaProgrammaResult>();
            try
            {
                it.rai.servizi.anagraficaws1.APWS sr = new it.rai.servizi.anagraficaws1.APWS();
                sr.Credentials = new System.Net.NetworkCredential(@"srvanapro", "bc14a3", "RAI");
                it.rai.servizi.anagraficaws1.ObjTVRicercaAnagrafieResult res = new it.rai.servizi.anagraficaws1.ObjTVRicercaAnagrafieResult();

                it.rai.servizi.anagraficaws1.ObjInputRicercaMatricola ricercaMatricola = new it.rai.servizi.anagraficaws1.ObjInputRicercaMatricola();
                //ricercatitolo.LivelloDiRicerca = anagraficaws.LivelloDiRicerca.Matricola;

                ricercaMatricola.Matricola = term;
                ricercaMatricola.StatiInVita = true;

                res = sr.TvRicercaAnagrafiaMatricola(ricercaMatricola);

                var results = res.RisultatoTVRicercaAnagrafie.ToList();

                if (results != null && results.Any())
                {
                    results.ForEach(p =>
                    {
                        lista.Add(new RicercaProgrammaResult()
                        {
                            Matricola = p.MATRICOLA,
                            Titolo = p.TITOLO_DEFINIT,
                            UORG = p.UORG
                        });
                    });
                }

                return Json(lista, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Metodo che si occupa della generazione della ricevuta in formato pdf
        /// Col riepilogo dell'ordine 
        /// </summary>
        /// <returns></returns>
        public FileContentResult StampaPdf()
        {
            try
            {
				byte[] bytes = printCestiniDataController.StampaPdf( CestiniControllerScope.Instance.Cestino.ordine, Server.MapPath( "~/assets/img/rai.png" ) );
                
                return File(bytes, "application/pdf", "StampaRiepilogo.pdf");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Stampa dell'intestazione del riepilogo dell'ordine
        /// </summary>
        /// <param name="cb"></param>
        /// <param name="document"></param>
        /// <param name="ordine"></param>
        /// <returns></returns>
        private int WriteIntestazione(PdfContentByte cb, Document document, Ordine ordine)
        {
            int currentY = 750;
            const int lStartX = 25;
            const int fontSize = 10;

            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED);
            Font myFont = new Font(bf, fontSize, Font.NORMAL);
            Font myFontBold = new Font(bf, fontSize, Font.BOLD);

            Font titleFont = new Font(bf, 14, Font.BOLD);

            int border = 0; // none
            int textAlign = 0; // left

            // disegno del logo
            iTextSharp.text.Image png = iTextSharp.text.Image.GetInstance(Server.MapPath("~/assets/img/rai.png"));
            png.ScaleAbsolute(45, 45);
            png.SetAbsolutePosition(25, 750);
            cb.AddImage(png);

            PdfPTable table = new PdfPTable(5);
            table.DefaultCell.Border = Rectangle.NO_BORDER;
            table.DefaultCell.BorderWidth = 0;
            table.TotalWidth = document.PageSize.Width - 50;
            table.LockedWidth = true;
            int[] widths = new int[] { 100, 140, 20, 100, 140 };
            table.SetWidths(widths);

            table.AddCell(this.WriteCell("Richiesta pasti in catering ", border, 5, 1, titleFont));
            table.AddCell(this.WriteCell(" ", border, 5, textAlign, myFont));

            // Riga 1
            table.AddCell(this.WriteCell("Identificativo: ", border, 1, textAlign, myFontBold));
            table.AddCell(this.WriteCell(ordine.codiceOrdine, border, 1, textAlign, myFont));
            table.AddCell(this.WriteCell(" ", border, 1, textAlign, myFont));
            table.AddCell(this.WriteCell("Data inserimento: ", border, 1, textAlign, myFontBold));

            // controlla se la data di inserimento è valorizzata, poichè il campo non è nullable il controllo
            // viene effettuato sul valore minimo che può assumere un campo di tipo datetime
            if (ordine.dataInserimento.Equals(DateTime.MinValue))
            {
                table.AddCell(this.WriteCell(" ", border, 1, textAlign, myFont));
            }
            else
            {
                table.AddCell(this.WriteCell(ordine.dataInserimento.ToString("dd/MM/yyyy HH:mm:ss"), border, 1, textAlign, myFont));
            }

            // interlinea
            table.AddCell(this.WriteCell(" ", border, 5, textAlign, myFont));

            // Riga 2
            table.AddCell(this.WriteCell("Riferimenti: ", border, 1, textAlign, myFontBold));
            table.AddCell(this.WriteCell(ordine.telefonoReferente, border, 1, textAlign, myFont));
            table.AddCell(this.WriteCell(" ", border, 1, textAlign, myFont));
            table.AddCell(this.WriteCell(" ", border, 1, textAlign, myFont));
            table.AddCell(this.WriteCell(" ", border, 1, textAlign, myFont));

            // Linea
            table.AddCell(this.WriteLine(border, textAlign));

            // interlinea
            table.AddCell(this.WriteCell(" ", border, 5, textAlign, myFont));

            // Riga 3
            table.AddCell(this.WriteCell("Pasto: ", border, 1, textAlign, myFontBold));
            table.AddCell(this.WriteCell(ordine.tipoPasto.ToUpper(), border, 1, textAlign, myFont));
            table.AddCell(this.WriteCell(" ", border, 1, textAlign, myFont));
            table.AddCell(this.WriteCell("Cespite: ", border, 1, textAlign, myFontBold));
            table.AddCell(this.WriteCell(ordine.luogoConsegna, border, 1, textAlign, myFont));
            // interlinea
            table.AddCell(this.WriteCell(" ", border, 5, textAlign, myFont));

            // Riga 4
            table.AddCell(this.WriteCell("Data consegna: ", border, 1, textAlign, myFontBold));
            table.AddCell(this.WriteCell(ordine.dataOraPasto.ToString("dd/MM/yyyy"), border, 1, textAlign, myFont));
            table.AddCell(this.WriteCell("", border, 1, textAlign, myFont));
            table.AddCell(this.WriteCell("Orario di consegna: ", border, 1, textAlign, myFontBold));
            table.AddCell(this.WriteCell(ordine.dataOraPasto.ToString("HH:mm:ss"), border, 1, textAlign, myFont));
            // interlinea
            table.AddCell(this.WriteCell(" ", border, 5, textAlign, myFont));

            // Riga 5
            table.AddCell(this.WriteCell("Luogo di consegna: ", border, 1, textAlign, myFontBold));
            table.AddCell(this.WriteCell(ordine.luogoConsegna, border, 1, textAlign, myFont));
            table.AddCell(this.WriteCell("", border, 1, textAlign, myFont));
            table.AddCell(this.WriteCell("Referente Rai per la consegna: ", border, 1, textAlign, myFontBold));
            table.AddCell(this.WriteCell(ordine.referenteConsegna, border, 1, textAlign, myFont));

            // Riga 6
            table.AddCell(this.WriteCell("Note: ", border, 5, textAlign, myFontBold));

            // Riga 7
            table.AddCell(this.WriteCell(ordine.note, border, 5, textAlign, myFont));

            // Linea
            table.AddCell(this.WriteLine(border, textAlign));

            // interlinea
            table.AddCell(this.WriteCell(" ", border, 5, textAlign, myFont));

            // Riga 8
            table.AddCell(this.WriteCell("Matricola richiedente: ", border, 1, textAlign, myFontBold));
            table.AddCell(this.WriteCell(ordine.matricolaOrdine, border, 1, textAlign, myFont));
            table.AddCell(this.WriteCell(" ", border, 1, textAlign, myFont));
            table.AddCell(this.WriteCell("Nominativo richiedente: ", border, 1, textAlign, myFontBold));
            table.AddCell(this.WriteCell(ordine.richiedente, border, 1, textAlign, myFont));
            // interlinea
            table.AddCell(this.WriteCell(" ", border, 5, textAlign, myFont));

            // Riga 9
            table.AddCell(this.WriteCell("Struttura: ", border, 1, textAlign, myFontBold));
            table.AddCell(this.WriteCell(ordine.struttura, border, 1, textAlign, myFont));
            table.AddCell(this.WriteCell(" ", border, 1, textAlign, myFont));
            table.AddCell(this.WriteCell("Telefono: ", border, 1, textAlign, myFontBold));
            table.AddCell(this.WriteCell(ordine.telefonoReferente, border, 1, textAlign, myFont));
            table.AddCell(this.WriteCell(" ", border, 5, textAlign, myFont));
            // Riga 10
            table.AddCell(this.WriteCell("Motivo della richiesta: ", border, 5, textAlign, myFontBold));

            // Riga 11
            table.AddCell(this.WriteCell(ordine.motivoOrdine, border, 5, textAlign, myFont));

            // Linea
            table.AddCell(this.WriteLine(border, textAlign));

            // interlinea
            table.AddCell(this.WriteCell(" ", border, 5, textAlign, myFont));

            // Riga 12
            table.AddCell(this.WriteCell("Titolo: ", border, 1, textAlign, myFontBold));
            table.AddCell(this.WriteCell(ordine.titoloProduzione, border, 4, textAlign, myFont));
            table.AddCell(this.WriteCell("UORG: ", border, 1, textAlign, myFontBold));
            table.AddCell(this.WriteCell(ordine.centroCosto, border, 4, textAlign, myFont));
            // Linea
            table.AddCell(this.WriteLine(border, textAlign));
            // interlinea
            table.AddCell(this.WriteCell(" ", border, 5, textAlign, myFont));

            table.WriteSelectedRows(0, table.Rows.Count + 1, lStartX, currentY, cb);

            currentY = currentY - (int)table.CalculateHeights();

            return currentY;
        }

        /// <summary>
        /// Stampa dell'intestazione della tabella di riepilogo cestini richiesti
        /// </summary>
        /// <param name="cb"></param>
        /// <param name="document"></param>
        /// <param name="currentY"></param>
        /// <returns></returns>
        private int WriteIntestazioneTabellaCestini(PdfContentByte cb, Document document, int currentY)
        {
            const int fontSize = 10;
            const int lStartX = 25;

            int border = 0; // none
            int textAlign = 0; // left

            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED);
            Font myFont = new Font(bf, fontSize, Font.NORMAL);
            Font myFontBold = new Font(bf, fontSize, Font.BOLD);

            PdfPTable tableCestini = new PdfPTable(3);
            tableCestini.DefaultCell.BorderWidth = 1;
            tableCestini.TotalWidth = document.PageSize.Width - 50;
            //fix the absolute width of the table
            tableCestini.LockedWidth = true;
            int[] cestiniWidths = new int[] { 220, 80, 200 };
            tableCestini.SetWidths(cestiniWidths);
            tableCestini.AddCell(this.WriteCell("Destinatari pasti", border, 3, 1, myFontBold));
            tableCestini.AddCell(this.WriteCell(" ", border, 3, 1, myFont));

            tableCestini.AddCell(this.WriteCell("Destinatario ", border, 1, textAlign, myFontBold));
            tableCestini.AddCell(this.WriteCell("Tipologia ", border, 1, textAlign, myFontBold));
            tableCestini.AddCell(this.WriteCell("Codice ", border, 1, textAlign, myFontBold));
            tableCestini.AddCell(this.WriteCell(" ", border, 3, 1, myFont));
            tableCestini.WriteSelectedRows(0, (tableCestini.Rows.Count + 1), lStartX, currentY, cb);

            currentY = currentY - (int)tableCestini.CalculateHeights();
            tableCestini.FlushContent();
            return currentY;
        }

        /// <summary>
        /// Scrittura di un dato in una cella del documento pdf che si sta generando
        /// </summary>
        /// <param name="text"></param>
        /// <param name="border"></param>
        /// <param name="colspan"></param>
        /// <param name="textAlign"></param>
        /// <param name="f"></param>
        /// <returns></returns>
        private PdfPCell WriteCell(string text, int border, int colspan, int textAlign, Font f)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, f));
            cell.Border = border;
            cell.HorizontalAlignment = textAlign; //0=Left, 1=Centre, 2=Right
            cell.Colspan = colspan;
            return cell;
        }

        /// <summary>
        /// scrittura di una linea (HR) all'interno di una serie di celle
        /// </summary>
        /// <param name="border"></param>
        /// <param name="textAlign"></param>
        /// <returns></returns>
        private PdfPCell WriteLine(int border,int textAlign)
        {
            Chunk c = new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 530.0F, BaseColor.YELLOW, textAlign, 1));
            PdfPCell cellSeparator = new PdfPCell(new Phrase(c));
            cellSeparator.Border = border;
            cellSeparator.HorizontalAlignment = textAlign; //0=Left, 1=Centre, 2=Right
            return cellSeparator;
        }

        /// <summary>
        /// Metodo che ritorna un identificativo temporaneo da assegnare alla nuova richiesta
        /// </summary>
        /// <returns></returns>
        private int GetTempId()
        {
            int _tempId = -1;

            if (CestiniControllerScope.Instance.Cestino.richieste.Any())
            {
                _tempId = CestiniControllerScope.Instance.Cestino.richieste.Count;

                // in questo modo viene assegnato un identificativo negativo
                // per distinguere i record non ancora salvati da quelli già presenti sul db
                _tempId = _tempId * (-1);

                bool stopExecution = false;

                do
                {
                    var t = CestiniControllerScope.Instance.Cestino.richieste.Where(r => r.idRichiesta.Equals(_tempId)).FirstOrDefault();

                    // se non esistono altri elementi con lo stesso id temporaneo allora verrà restituito
                    // l'identificativo appena calcolato
                    if (t == null)
                        stopExecution = true;
                    else
                    {
                        // altrimenti decrementa di 1 e riesegue il controllo
                        _tempId--;
                    }
                } while (!stopExecution);
            }

            return _tempId;
        }

        /// <summary>
        /// Calcolo del progressivo da assegnare ad una nuova richiesta
        /// </summary>
        /// <returns></returns>
        private int GetProgressivo()
        {
            int _tempId = 1;

            if (CestiniControllerScope.Instance.Cestino.richieste.Any())
            {
                _tempId = CestiniControllerScope.Instance.Cestino.richieste.Count;

                bool stopExecution = false;

                do
                {
                    var progMax = CestiniControllerScope.Instance.Cestino.richieste.Max(p => p.progressivo);

                    // se il progressivo generato è superiore al progressivo massimo trovato nelle richieste
                    // allora si può terminare il ciclo
                    if (_tempId > progMax)
                    {
                        stopExecution = true;
                    }
                    else
                    {
                        _tempId++;
                    }

                } while (!stopExecution);
            }

            return _tempId;
        }

        /// <summary>
        /// Metodo per la generazione del codice dell'ordine
        /// </summary>
        /// <returns>Codice generato nel formato AAMMGG_NNN</returns>
        private string GenerateOrderCode()
        {
            string code = null;
            try
            {
                DateTime today = DateTime.Now.Date;
                int currentCestini = 0;

                // reperimento del numero di cestini per il giorno corrente
                using (digiGappEntities db = new digiGappEntities())
                {
                    var list = db.B2RaiPlace_Cestini_Ordini.Where(c => c.Data_inserimeto.Year == today.Year &&
                                                                    c.Data_inserimeto.Month == today.Month &&
                                                                    c.Data_inserimeto.Day == today.Day).ToList();
                    currentCestini = list.Count();
                    currentCestini++;
                }

                code = String.Format("{0:00}{1:00}{2:00}_{3:000}", today.ToString("yy"), today.Month, today.Day, currentCestini);
                return code;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Metodo per la generazione del codice per la singola richiesta
        /// associata all'ordine
        /// </summary>
        /// <param name="punnet">Codice dell'ordine al quale è associata la richiesta</param>
        /// <param name="cespite">Codice identificativo del cespite associato alla richiesta</param>
        /// <param name="richiesta">Metadati dell'ordine al quale associare la richiesta</param>
        /// <returns>Codice generato</returns>
        private string GenerateReqCode(string punnet, string cespite, Richiesta richiesta)
        {
            // AAMMGG_NNN_SSS_O_VVVVVV_P_YYY
            string code = null;
            try
            {
                /*
                 * O, 1 digit in cuui si specifica se la richiesta è a beneficio di una persona interna (I), 
                 * oppure esterna (E);
                 */
                string O = "";
                O = richiesta.flagRisorsa ? "I" : "E";

                /*
                 * SSS, 3 digit in cui si specifica il cespite di consegna 
                 * (usando la formalizzazione già prevista degli insediamenti RAI sul territorio);
                 * */
                string SSS = cespite;

                /*
                 * VVVVVV, 6 digit in cui si specifica la matricola della eventuale risorsa interna, 
                 * o le prime tre lettere del nome e del cognome nel caso di risorsa esterna;
                 * */
                string VVVVVV = richiesta.matricolaRisorsa;

                if (!richiesta.flagRisorsa)
                {
                    string nome = richiesta.nomeRisorsa.Replace(" ", "");
                    string cognome = richiesta.cognomeRisorsa.Replace(" ", "");

                    VVVVVV = String.Format("{0}{1}", nome.Substring(0, 3), cognome.Substring(0, 3));
                }

                /*
                 * P, 1 digit in cui si specifica se la richiesta sia per un pranzo (P) 
                 * o una cena (C), mentre nei successivi due digit si specifica 
                 * la relativa fascia oraria della richiesta (codificata in un formato numerico);
                 * */
                string P = String.Format("{0}{1:00}", CestiniControllerScope.Instance.Cestino.ordine.tipoPasto[0].ToString().ToUpper(),
                                                    CestiniControllerScope.Instance.Cestino.ordine.dataOraPasto.Hour);

                /*
                 * YYY, 3 digit in cui si specifica la tipologia di cestino 
                 * (codificata tramite un codice alfabetico semi parlante), 
                 * in relazione alla scelta del campo “tipologia di cestino”.
                 * */
                string YYY = richiesta.tipoCestino.GetDescription();

                code = String.Format("{0}_{1}_{2}_{3}_{4}_{5}", punnet, SSS, O, VVVVVV, P, YYY);
                return code;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Init dei viewbag utilizzati per il caricamento delle combo
        /// </summary>
        /// <param name="selectedItem"></param>
        private void InitializeViewBagInformation(int? selectedItem = null)
        {
            var cestiniTypes = new List<Lookup>();
            // Inserimento dell'elemento vuoto
            cestiniTypes.Add(new Lookup()
            {
                Id = new Nullable<int>(),
                Codice = null,
                Description = null
            });

            cestiniTypes.AddRange(GetCestiniTypes(selectedItem));
            ViewBag.CestiniTypes = cestiniTypes;
            //ViewBag.CestiniTypes = new SelectList(cestiniTypes, "Id", "Description", (selectedItem ?? 0));
        }

        /// <summary>
        /// Reperimento della lista delle tipologie di cestino
        /// </summary>
        /// <returns></returns>
        private List<Lookup> GetCestiniTypes(int? selectedItem = null)
        {
            List<Lookup> result = new List<Lookup>();
            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    var list = db.B2RaiPlace_Cestini_Tipologie.ToList();

                    if (list != null)
                    {
                        list.ForEach(l =>
                        {
                            result.Add(new Lookup()
                            {
                                Id = l.Id_tipo,
                                Codice = l.Codice,
                                Description = l.Descrizione,
                                Selected = (selectedItem != null ? (selectedItem == l.Id_tipo ? true: false) : false)
                            });
                        });
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Reperimento dell'elenco delle richieste cestini creati dall'utente corrente
        /// </summary>
        /// <returns></returns>
		private List<CestiniModel> GetListaCestini ( DateTime? datada = null, DateTime? dataal = null )
        {
            List<CestiniModel> result = new List<CestiniModel>();
            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    // reperimento della matricola dell'utente corrente
                    string matricola = UtenteHelper.EsponiAnagrafica()._matricola;

					List<B2RaiPlace_Cestini_Ordini> list = new List<B2RaiPlace_Cestini_Ordini>();

					// calcolo della data da cui partire per il reperimento delle richieste
					// il sistema deve caricare tutte le richieste degli ultimi 30 gg
					DateTime oggi = DateTime.Now;
					DateTime startDate = oggi.AddDays( -30 );

					// se è stato applicato un filtro
					if ( datada.HasValue || dataal.HasValue )
					{
						// se la data da non è valorizzata allora viene impostata la data minima consentita dal sistema
						if ( !datada.HasValue )
							datada = DateTime.MinValue;

						// se la data a non è valorizzata allora viene impostata la data massima consentita dal sistema
						if ( !dataal.HasValue )
							dataal = DateTime.MaxValue;

						DateTime dt1 = datada.Value.Date;
						DateTime dt2 = dataal.Value.Date;

						list = db.B2RaiPlace_Cestini_Ordini.Where( x => x.Matricola_richiedente == matricola &&
						x.Data_ora_pasto >= dt1 && x.Data_ora_pasto <= dt2 ).OrderByDescending( o => o.Data_ora_pasto ).ToList();
					}
					else
					{
						// reperimento di tutte le richieste cestino effettuate dall'utente corrente
						// per gli ultimi 30 gg
						list = db.B2RaiPlace_Cestini_Ordini.Where( x => x.Matricola_richiedente == matricola &&
							x.Data_ora_pasto >= startDate.Date ).OrderByDescending( o => o.Data_ora_pasto ).ToList();
					}

                    if (list != null)
                    {
                        list.ForEach(l =>
                        {
                            DateTime _dateTime = DateTime.ParseExact(l.Ora_pasto, "HH:mm:ss", CultureInfo.InvariantCulture);

                            DateTime dataPasto = new DateTime(l.Data_ora_pasto.Year, l.Data_ora_pasto.Month,
                                                                l.Data_ora_pasto.Day, _dateTime.Hour,
                                                                _dateTime.Minute, _dateTime.Millisecond);

                            // calcolo dei cestini per richiesta
                            int nCestini = db.B2RaiPlace_Cestini_Richieste.Where(c => c.Id_ordine.Equals(l.Id_ordine)).Count();

                            result.Add(new CestiniModel()
                            {
                                ordine = new Ordine()
                                {
                                    approvatore = l.Matricola_approvatore,
                                    centroCosto = l.Centro_costo,
                                    cespite = l.Cespite,
                                    codiceOrdine = l.Codice_ordine,
                                    dataOraPasto = dataPasto,
                                    dataStatus = l.Data_status,
                                    idOrdine = l.Id_ordine,
                                    matricolaOrdine = l.Matricola_richiedente,
                                    matricolaSpettacolo = l.Matricola_spettacolo,
                                    motivoOrdine = l.Motivo,
                                    note = l.Note,
                                    referenteConsegna = l.Matricola_Referente_consegna,
                                    statusOrdine = (StatusOrdiniEnum)l.B2RaiPlace_Cestini_Status_Ordini.Id,
                                    telefonoReferente = l.Tel_referente,
                                    tipoPasto = l.Tipo_pasto,
                                    titoloProduzione = l.Titolo_produzione,
                                    luogoConsegna = l.Luogo_consegna,
                                    matricolaReferenteConsegna = l.Matricola_Referente_consegna
                                },
                                richiestaCorrente = new Richiesta(),
                                richieste = new List<Richiesta>(),
                                CountRichieste = nCestini
                            });
                        });
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region Invio mail

        private void InviaMail(string destinatario, string utenteCreatore, 
                                        DateTime dataCreazione, string sede,
                                        string pasto, string cestino)
        {
            try
            {
                MailSender invia = new MailSender();
                Email eml = new Email();
                eml.From = "raiplace.selfservice@rai.it";

                eml.toList = new string[] { "p" + destinatario + "@rai.it" };

                // indirizzi email per test di invio mail per motivi di sviluppo
                //eml.toList = new string[] { "francesco.buonavita80@gmail.com", "vincenzo.bifano@rai.it" };
                //eml.toList = new string[] { "francesco.buonavita80@gmail.com" };
                eml.ContentType = "text/html";
                eml.Priority = 2;
                eml.SendWhen = DateTime.Now.AddSeconds(1);

                eml.Subject = "SelfService del Dipendente - Richiesta cestino";
                eml.Body = this.GetMailTemplateRichiestaCestinoUtente();

                eml.Body = eml.Body.Replace("#UTENTE", utenteCreatore)
                                    .Replace("#DATA", dataCreazione.ToString("dd/MM/yyyy HH:mm:ss"))
                                    .Replace("#SEDE", sede)
                                    .Replace("#TPASTO", pasto)
                                    .Replace("#TCESTINO", cestino);

                string[] parametriMail = GetDatiServizioMail();

                invia.Credentials = new System.Net.NetworkCredential(parametriMail[0], parametriMail[1], "RAI");

                try
                {
                    invia.Send(eml);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Reperimento delle credenziali da passare al servizio di invio mail
        /// </summary>
        /// <returns></returns>
        private string[] GetDatiServizioMail()
        {
            string[] dati = null;

            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    var result = db.MyRai_ParametriSistema.Where(p => p.Chiave.Equals("AccountUtenteServizio", StringComparison.InvariantCultureIgnoreCase)).ToList();

                    if (result == null)
                        throw new Exception("Parametri per invio mail non trovati");

                    dati = new string[] { result.First().Valore1, result.First().Valore2 };

                    return dati;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Reperimento del template per l'invio della mail di richiesta cestino
        /// </summary>
        /// <returns></returns>
        private string GetMailTemplateRichiestaCestinoUtente()
        {
            string template = null;

            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    var result = db.MyRai_ParametriSistema.Where(p => p.Chiave.Equals("MailTemplateRichiestaCestinoUtente", StringComparison.InvariantCultureIgnoreCase)).ToList();

                    if (result == null)
                        throw new Exception("Template non trovato");

                    template = result.First().Valore1;

                    return template;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        #endregion

    }

    public class CestiniControllerScope : SessionScope<CestiniControllerScope>
    {
        public CestiniControllerScope()
        {
			this._cestiniAnagraficaUtenti = new List<CestiniAnagraficaUtente>();
			this.FiltroRicercaUtenti = String.Empty;
        }

        public CestiniModel Cestino
        {
            get
            {
                return this._cestino;
            }
            set
            {
                this._cestino = value;
            }
        }

		public List<CestiniAnagraficaUtente> CestiniAnagraficaUtenti
		{
			get
			{
				return this._cestiniAnagraficaUtenti;
			}
			set
			{
				this._cestiniAnagraficaUtenti = value;
			}
		}

		public String FiltroRicercaUtenti
		{
			get
			{
				return this._filtroRicercaUtenti;
			}
			set
			{
				this._filtroRicercaUtenti = value;
			}
		}

        private CestiniModel _cestino = null;

		private List<CestiniAnagraficaUtente> _cestiniAnagraficaUtenti { get; set; }
		private string _filtroRicercaUtenti { get; set; }
    }

	/// <summary>
	/// Definizione dell'oggetto che rappresenta i dati di output del servizio di ricerca utenti
	/// </summary>
	public class CestiniAnagraficaUtente
	{
		public CestiniAnagraficaUtente ()
		{
		}

		public string Cognome { get; set; }
		public string Nome { get; set; }
		public string Matricola { get; set; }
		public string Direzione { get; set; }
		public string Nominativo {
			get
			{
				return String.Format( "{0} {1}", this.Cognome, this.Nome );
			}
			set
			{
			}
		}
	}
}