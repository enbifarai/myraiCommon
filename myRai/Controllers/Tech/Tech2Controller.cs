using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using myRaiData;
using System.Text;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using myRaiCommonModel.Tech;
using myRaiService;
using myRaiHelper;
using myRaiCommonModel;

namespace myRai.Controllers
{
    public partial class TechController : Controller
    {
		public ActionResult ChartLogs ()
		{
			List<Punto> punti = new List<Punto>();

			punti = GetDataInternal();

			return View( "~/Views/Tech/ChartLogs/ChartLogs.cshtml", punti );
		}

		public ActionResult ChartRichieste ()
		{
			List<PuntoRichiesta> punti = new List<PuntoRichiesta>();

			punti = GetStatisticheRichieste();

			return View( "~/Views/Tech/ChartRichieste/ChartRichieste.cshtml", punti );
		}

		public ActionResult Esporta ()
		{
			byte[] content = null;

			var csv = new StringBuilder();

			// Intestazione
			var newLine = string.Format( "Codice,Descrizione,Totale richieste" );
			csv.AppendLine( newLine );

			List<PuntoRichiesta> punti = new List<PuntoRichiesta>();

			punti = GetStatisticheRichieste();

			if ( punti != null && punti.Any() )
			{
				foreach ( var item in punti.OrderByDescending( m => m.Valore ) )
                {
					var nl = string.Format( "{0},{1},{2}", item.Codice, item.Tooltip, item.Valore.ToString() );
					csv.AppendLine( nl );
                }
			}

			content = Encoding.ASCII.GetBytes( csv.ToString() );

			string filename = "ExportStatisticheEccezioni.csv";
			string mime = "text/csv";
			return File( content.ToArray(), mime, filename );
		}

		public ActionResult GetData (ChartLogTypesEnum? tipo = null)
		{
			List<Punto> punti = new List<Punto>();

			punti = GetDataInternal( new DateTime( DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0 ), tipo );

			return Json( punti, JsonRequestBehavior.AllowGet );
		}

        /// <summary>
        /// Metodo che restituisce la view che ci dice
        /// in media dopo quanti gg una richiesta viene approvata
        /// </summary>
        /// <returns></returns>
        public ActionResult ChartRichiesteApprovate()
        {
            return null;
            int totalRichieste = 0;
            double totalDays = 0;
            int parzialDays = 0;

            TechApprovazioneRichiesteVM model = new TechApprovazioneRichiesteVM()
            {
                GiorniAttesaApprovazione = 0,
                GiorniAttesaConsuntivazioneRichiesta = 0
            };

            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    #region Calcolo media attesa richiesta approvata
                    // Tempo che intercorre tra una richiesta e la sua approvazione di primo livello
                    var ecc = from eccezioni in db.MyRai_Eccezioni_Richieste
                              join r in db.MyRai_Richieste
                              on eccezioni.id_richiesta equals r.id_richiesta
                              where eccezioni.id_stato == 20 &&
                                    r.id_stato == 20 &&
                                    eccezioni.data_validazione_primo_livello.HasValue
                              select eccezioni;

                    if (ecc != null && ecc.Any())
                    {
                        // conta il numero di richieste
                        totalRichieste = (from e in ecc
                                          select e.id_richiesta).Distinct().Count();

                        foreach (var e in ecc)
                        {
                            //totalRichieste++;
                            DateTime dStart = e.data_creazione;
                            DateTime dEnd = e.data_validazione_primo_livello.GetValueOrDefault();

                            parzialDays += (int)(dEnd - dStart).TotalDays;
                        }

                        if (parzialDays > 0)
                        {
                            totalDays = (double)(parzialDays / totalRichieste);
                        }
                    }

                    model.GiorniAttesaApprovazione = totalDays;
                    #endregion

                    #region Calcolo media attesa richiesta consuntivata
                    // Tempo che intercorre tra la data di approvazione e la data stampa pdf
                    int totPdf = 0;
                    int daysWaiting = 0;
                    double saldo = 0;

                    var pdfs = from p in db.DIGIRESP_Archivio_PDF
                                where p.data_convalida.HasValue &&
                                        p.data_stampa.HasValue
                                select new
                                {
                                    p.data_inizio,
                                    p.data_fine,
                                    p.sede_gapp,
                                    p.data_convalida,
                                    p.data_stampa
                                };

                    if (pdfs != null && pdfs.Any())
                    {
                        totPdf = pdfs.Count();

                        foreach(var p in pdfs)
                        {
                            daysWaiting += (int)(p.data_convalida.GetValueOrDefault() - p.data_stampa.GetValueOrDefault()).TotalDays;
                        }
                        saldo = (double)(daysWaiting / totPdf);
                    }

                    model.GiorniAttesaConsuntivazioneRichiesta = saldo;

                    #endregion

                    #region Calcolo tempo di consuntivazione di una giornata da parte dell'utente. Tempo che intercorre tra la richiesta e la data richiesta

                    int ppDays = 0;
                    DateTime current = DateTime.Now;

                    // prende tutte le eccezioni la cui data_eccezione è inferiore alla data di creazione ed è inferiore ad oggi
                    var pp = db.MyRai_Eccezioni_Richieste.Where(w => w.data_eccezione < w.data_creazione && w.data_creazione <= current).ToList();

                    if (pp != null && pp.Any())
                    {
                        pp.ForEach(w =>
                       {
                           ppDays += (int)(w.data_creazione - w.data_eccezione).TotalDays;
                       });

                        totalDays = (double)ppDays / pp.Count();
                    }

                    model.GiorniConsuntivazioneEccezioneUtente = totalDays;

                    #endregion
                }
            }
            catch(Exception ex)
            {
                return View("~/Views/Shared/TblError.cshtml", new HandleErrorInfo(ex, "TechController", "ChartRichiesteApprovate"));
            }

            return View("~/Views/Tech/RichiesteApprovate/RichiesteApprovate.cshtml", model);
        }

        private List<Punto> GetDataInternal ( DateTime? curr = null, ChartLogTypesEnum? tipo = null)
		{
			try
			{
				List<GetLogsStatistics_Result> temp = new List<GetLogsStatistics_Result>();
				List<Punto> punti = new List<Punto>();

				DateTime current = DateTime.Now;

				DateTime start = new DateTime( current.Year, current.Month, current.Day, 0, 0, 0 );
				DateTime end = DateTime.Now;

				if ( curr != null )
				{
					start = new DateTime( curr.Value.Year, curr.Value.Month, curr.Value.Day, curr.Value.Hour, curr.Value.Minute, 0 );
				}

				List<MyRai_LogErrori> logs = new List<MyRai_LogErrori>();

				using ( digiGappEntities db = new digiGappEntities() )
				{
					string tx = null;
					
					if ( tipo != null )
					{
						tx = tipo.Value.GetAmbientValue();
					}

					List<LogsStatistics> results = db.GetLogsStatistics( start, end, tx, null ).ToList();

					if ( results != null && results.Count() > 0  )
					{
						foreach ( var log in results )
						{
							punti.Add( new Punto()
							{
								Ordinata = log.Occorrenze.GetValueOrDefault(),
								Ascissa = log.Hour.GetValueOrDefault()
							} );
						}
					}
				}

				if ( punti != null && punti.Any() )
				{
					punti = punti.OrderBy( c => c.Ascissa ).ToList();
				}

				return punti;
				
			}
			catch(Exception ex)
			{
				return new List<Punto>();
			}
		}

		private List<PuntoRichiesta> GetStatisticheRichieste ()
		{
			List<PuntoRichiesta> results = new List<PuntoRichiesta>();

			try
			{
				using ( digiGappEntities db = new digiGappEntities() )
				{
					var distinctList = db.MyRai_Eccezioni_Richieste.GroupBy( r => r.cod_eccezione ).Select( i => i.FirstOrDefault() ).ToList();

					if ( distinctList != null && distinctList.Any() )
					{
						distinctList.ForEach( dl =>
						{
							string ascissa = "";
							int ordinata = 0;

							ascissa = db.MyRai_Eccezioni_Ammesse.Where( e => e.cod_eccezione.Equals( dl.cod_eccezione ) ).FirstOrDefault().desc_eccezione;

							ordinata = db.MyRai_Eccezioni_Richieste.Where( e => e.cod_eccezione.Equals( dl.cod_eccezione ) ).Count();

							results.Add( new PuntoRichiesta()
							{
								Codice = dl.cod_eccezione,
								Valore = ordinata,
								Tooltip = ascissa
							} );
						} );
					}
				}
			}
			catch ( Exception ex )
			{
			}

			return results;
		}

		/// <summary>
		/// Ricerca delle richieste aventi eccezioni con stato diverse dal padre e che non siano storni
		/// Nel caso di richieste in stato 70, controlla sul GAPP lo stato della richiesta e verifica 
		/// l'effettiva cancellazione, altrimenti ripristina lo stato della richiesta allo stato più 
		/// basso tra quelli dell'eccezioni figlie
		/// </summary>
		public void AllineaRichieste ()
		{
			string lineaReport = "";

			try
			{
				using ( digiGappEntities db = new digiGappEntities() )
				{
					// recupero tutti i record che sono disallineati. Cioè lo stato del padre
					// è diverso dallo stato dei figli

					/*
					* SELECT e.[id_eccezioni_richieste], e.[id_stato] as statoEccezione, 
					*					r.id_stato as statoRichiesta, e.id_richiesta, r.*
					* FROM [digiGapp].[dbo].[MyRai_Richieste] r
					* join [digiGapp].[dbo].myrai_eccezioni_richieste e
					* on r.id_richiesta = e.id_richiesta
					* where r.data_richiesta >= '2018-10-01 00:00:00.000'
					* and r.data_richiesta <= '2018-10-31 23:59:59.000'
					* and r.id_stato <> e.id_stato
					* and e.azione <> 'C'
					*/

					AllineaStatoRichiesta( 78183 );
					return;

                    var records = from richieste in db.MyRai_Richieste
                                  join eccezioni in db.MyRai_Eccezioni_Richieste
                                  on richieste.id_richiesta equals eccezioni.id_richiesta
                                  where richieste.id_stato != eccezioni.id_stato
                                  orderby richieste.id_richiesta
                                  //&& richieste.id_stato == 70
                                  select new
                                  {
                                      richiesta = richieste
                                  };

                    // è stato trovato qualche elemento
                    if ( records != null && records.Any( ) )
                    {
                        foreach ( var item in records.Distinct( ) )
                        {
                            lineaReport += String.Format( "Richiesta id: {0} in stato {1} " , item.richiesta.id_richiesta , item.richiesta.id_stato );

                            string esito = AllineaStatoRichiesta( item.richiesta.id_richiesta );

                            lineaReport += String.Format( " , esito: {0} \r\n" , esito );
                        }
                    }
				}
			}
			catch ( Exception ex )
			{
				lineaReport += "Si è verificato un errore: " + ex.ToString() + "\r\n";
			}

			System.IO.File.WriteAllText( AppDomain.CurrentDomain.BaseDirectory + "\\reportSanificazioneRichieste.txt", lineaReport );
		}

		public string AllineaStatoRichiesta ( int idRichiesta )
		{
			string esito = "OK";
			try
			{
				WSDigigapp service = new WSDigigapp();

				using ( digiGappEntities db = new digiGappEntities() )
				{
					var richiesta = db.MyRai_Richieste.Where( w => w.id_richiesta.Equals( idRichiesta ) ).FirstOrDefault();

					if ( richiesta == null )
					{
						throw new Exception( "Richiesta non trovata" );
					}

					var eccezioni = db.MyRai_Eccezioni_Richieste.Where( w => w.id_richiesta.Equals( idRichiesta ) ).ToList();

					if ( eccezioni == null )
					{
						throw new Exception( "Nessuna eccezione trovata" );
					}

					bool disallineato = eccezioni.Count( w => w.id_stato != richiesta.id_stato ) > 0;

					if ( !disallineato )
					{
						throw new Exception( "Dato non disallineato" );
					}

					int statoMinimo = eccezioni.Min( w => w.id_stato );
					bool hasChildInGapp = false;
					int childInGapp = 0;

					foreach ( var e in eccezioni )
					{
						// recupero dal GAPP le eccezioni richieste dall'utente nella giornata in esame
						var response = service.getEccezioni( richiesta.matricola_richiesta, e.data_eccezione.ToString( "ddMMyyyy" ), "BU", 75 );

						if ( !response.esito )
						{
							throw new Exception( "errore in getEccezioni" );
						}

						// cerco nelle eccezioni restituite dal GAPP l'eccezioni correntemente esaminata
						var eccGapp = response.eccezioni.Where( k => k.cod.Trim() == e.cod_eccezione.Trim() ).FirstOrDefault();

						// se l'eccezione è in stato 10 ma è presente sul GAPP allora deve passare in stato 20
						// se è stato convalidato
						if ( e.id_stato == 10 && eccGapp != null )
						{
							if ( eccGapp.stato_eccezione.Trim() == "C" )
							{
								e.id_stato = 20;
								esito += String.Format( " eccezione id {0} in stato {1}", e.id_eccezioni_richieste, e.id_stato );
							}
						}	// se risulta approvato, ma non è presente sul GAPP
						else if ( e.id_stato == 20 && eccGapp == null )
						{
							// va verificato se è stato stornato
							var storno = eccezioni.Where( w => w.numero_documento_riferimento.Equals( e.numero_documento ) && w.azione.Equals( "C" ) ).FirstOrDefault();

							// se c'è uno storno e lo stato è 20 allora è normale che sul GAPP non
							// sia presente l'eccezione e.
							// se invece lo stato dello storno è diverso da 20, allora va controllato 
							// che non ci sia un disallineamento dello storno stesso, 
							// Ad esempio lo storno è stato approvato ma non è stato aggiornato il record
							if ( storno != null && storno.id_stato != 20 )
							{
								// presumibilmente lo storno è stato approvato sul gapp
								if ( storno.id_stato == 10 )
								{
									storno.id_stato = 20;
									esito += String.Format( " storno id {0} in stato {1}", storno.id_eccezioni_richieste, storno.id_stato );
								}
								else
								{
									// a questo punto l'eccezione è stata cancellata dal GAPP
									e.id_stato = 70;
									esito += String.Format( " eccezione id {0} in stato {1}", e.id_eccezioni_richieste, e.id_stato );
								}
							}
							else if ( storno == null )
							{
								// se non c'è lo storno allora l'eccezione è da considerarsi come cancellata sul GAPP ?
								e.id_stato = 70;
								esito += String.Format( " eccezione id {0} in stato {1}", e.id_eccezioni_richieste, e.id_stato );
							}
						}
						else if ( e.id_stato == 20 && eccGapp != null )
						{
							// se l'eccezione è nello stato Approvata ed è presente sul GAPP
							// Ma in stato D, allora lo stato dell'eccezione deve tornare in approvazione
							if ( eccGapp.stato_eccezione.Trim() == "D" )
							{
								e.id_stato = 10;
								esito += String.Format( " eccezione id {0} in stato {1}", e.id_eccezioni_richieste, e.id_stato );
							}
						}

						if ( eccGapp != null )
						{
							hasChildInGapp = true;
							childInGapp++;
						}
					}

					// al termine del ciclo, dopo aver allineato le eccezioni richieste
					// bisogna verificare lo stato della richiesta padre
					if ( richiesta.id_stato == 70 )
					{
						// se lo stato richiesta è 70 allora risulta cancellata
						// dal GAPP, se c'è almeno un figlio presente sul GAPP allora
						// la richiesta non può avere lo stato 70
						if ( hasChildInGapp )
						{
							richiesta.id_stato = statoMinimo;
							esito += String.Format( " richiesta id {0} in stato {1}", richiesta.id_richiesta, richiesta.id_stato );
						}
						else
						{
							foreach ( var e in eccezioni )
							{
								e.id_stato = 70;
								esito += String.Format( " eccezione id {0} in stato {1}", e.id_eccezioni_richieste, e.id_stato );
							}
						}
					}
					else
					{
						// se lo stato è diverso da 70, allora
						// la richiesta deve prendere lo stato più basso 
						// tra le varie eccezioni figlie
						richiesta.id_stato = statoMinimo;
						esito += String.Format( " richiesta id {0} in stato {1}", richiesta.id_richiesta, richiesta.id_stato );
					}

					db.SaveChanges();
				}
			}
			catch ( Exception ex )
			{
				esito = String.Format( "KO, {0}", ex.Message );
			}

			return esito;
		}

        public void testEccGiornalisti ( )
        {
            MyRaiService1 serv = new MyRaiService1( );
            var ReportEccezioni = serv.GetLista_Eccezioni_Giornalisti( "103650" , "9EC02" , 7 , 2019 );
        }

        [HttpGet]
        public ActionResult ControllaApprovatoriProduzione ( string dtStart = null , string dtEnd = null )
        {
            TechApprovatoriProduzioneVM model = new TechApprovatoriProduzioneVM( );
            model.ApprovatoriStati = new List<TechApprovatoreProduzioneItem>( );

            DateTime dStart;
            DateTime dEnd;

            if ( String.IsNullOrEmpty( dtStart ) )
            {
                dStart = DateTime.Now;
                dStart = dStart.AddMonths( -2 );
            }
            else
            {
                DateTime.TryParseExact( dtStart , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out dStart );
            }

            if ( String.IsNullOrEmpty( dtEnd ) )
            {
                //dEnd = new DateTime( 9999 , 12 , 31 );
                dEnd = DateTime.Now;
            }
            else
            {
                DateTime.TryParseExact( dtEnd , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out dEnd );
            }

            model.ApprovatoriStati = GetApprovatoriProduzione( dStart , dEnd );
            model.DataDA = dStart;
            model.DataA = dEnd;

            //if (!String.IsNullOrEmpty( dtStart ) )
            //{
            //    model.DataDA = dStart;
            //}

            //if ( !String.IsNullOrEmpty( dtEnd ) )
            //{
            //    model.DataA = dEnd;
            //}

            return View( "~/Views/Tech/ApprovatoriProduzione/VisualizzaStato.cshtml" , model );
        }

        private List<TechApprovatoreProduzioneItem> GetApprovatoriProduzione ( DateTime dtStart , DateTime dtEnd )
        {
            List<TechApprovatoreProduzioneItem> result = new List<TechApprovatoreProduzioneItem>( );

            try
            {
                using ( digiGappEntities db = new digiGappEntities( ) )
                {
                    string query = "";
                    query = "SELECT DISTINCT a.MatricolaApprovatore as Matricola,                   " +
                            "(                                                                      " +
                            "    SELECT TOP 1[Nominativo]                                           " +
                            "                                                                       " +
                            "    FROM[digiGapp].[dbo].[MyRai_ApprovatoriProduzioni] a1              " +
                            "                                                                       " +
                            "    WHERE a1.MatricolaApprovatore = a.MatricolaApprovatore             " +
                            ") as Nominativo,                                                       " +
                            "                                                                       " +
                            "(                                                                      " +
                            "      SELECT COUNT( r.id_richiesta )                                   " +
                            "        FROM MyRai_Richieste r                                         " +
                            "        WHERE r.id_stato = 10                                          " +
                            "        AND r.data_richiesta between '#DT1' and '#DT2'                 " +
                            "	and r.codice_sede_gapp = a.SedeGapp														" +
                            "	AND (r.ApprovatoreSelezionato = a.MatricolaApprovatore  OR								" +
                            "	r.ApprovatoreSelezionato in (SELECT [CodUfficio]      									" +
                            "								FROM [digiGapp].[dbo].[MyRai_UffProduzioni_Approvatori]		" +
                            "								where MatricolaApprovatore = a.MatricolaApprovatore ))		" +
                            ") as InApprovazione,                                                   " +
                            "(                                                                      " +
                            "      SELECT COUNT( r.id_richiesta )                                   " +
                            "        FROM MyRai_Richieste r                                         " +
                            "        WHERE r.id_stato = 20                                          " +
                            "        AND r.data_richiesta between '#DT1' and '#DT2'                 " +
                            "	and r.codice_sede_gapp = a.SedeGapp														" +
                            "	AND (r.ApprovatoreSelezionato = a.MatricolaApprovatore  OR								" +
                            "	r.ApprovatoreSelezionato in (SELECT [CodUfficio]      									" +
                            "								FROM [digiGapp].[dbo].[MyRai_UffProduzioni_Approvatori]		" +
                            "								where MatricolaApprovatore = a.MatricolaApprovatore ))		" +
                            ") as Approvate ,																			" +
                            "a.SedeGapp as SedeGapp																		" +
                            "FROM[digiGapp].[dbo].[MyRai_ApprovatoriProduzioni] a                   ";

                    query = query.Replace( "#DT1" , dtStart.ToString( "yyyy-MM-dd 00:00:00.000" ) );
                    query = query.Replace( "#DT2" , dtEnd.ToString( "yyyy-MM-dd 23:59:59.990" ) );

                    result = db.Database.SqlQuery<TechApprovatoreProduzioneItem>( query ).ToList( );

                    if ( result != null && result.Any( ) )
                    {
                        result = result.OrderBy( w => w.SedeGapp ).ThenBy( w => w.Nominativo ).ToList( );
                    }
                }
            }
            catch ( Exception ex )
            {
                result = new List<TechApprovatoreProduzioneItem>( );
            }

            return result;
        }

        public ActionResult EsportaStatoApprovatori ( string dtStart = null , string dtEnd = null )
        {
            TechApprovatoriProduzioneVM model = new TechApprovatoriProduzioneVM( );
            model.ApprovatoriStati = new List<TechApprovatoreProduzioneItem>( );

            DateTime dStart;
            DateTime dEnd;

            if ( String.IsNullOrEmpty( dtStart ) )
        {
                dStart = new DateTime( 1900 , 1 , 1 );
            }
            else
            {
                DateTime.TryParseExact( dtStart , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out dStart );
            }

            if ( String.IsNullOrEmpty( dtEnd ) )
            {
                dEnd = new DateTime( 9999 , 12 , 31 );
            }
            else
            {
                DateTime.TryParseExact( dtEnd , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out dEnd );
            }

            model.ApprovatoriStati = GetApprovatoriProduzione( dStart , dEnd );
            byte[] content = null;

            var csv = new StringBuilder( );

            // Intestazione
            var newLine = string.Format( "Matricola,Nominativo,In approvazione,Approvate,Sede gapp" );
            csv.AppendLine( newLine );

            if ( model.ApprovatoriStati != null && model.ApprovatoriStati.Any( ) )
            {
                foreach ( var item in model.ApprovatoriStati )
                {
                    var nl = string.Format( "{0},{1},{2},{3},{4}" , item.Matricola , item.Nominativo , item.InApprovazione , item.Approvate , item.SedeGapp );
                    csv.AppendLine( nl );
                }
            }

            content = Encoding.ASCII.GetBytes( csv.ToString( ) );

            string filename = "ExportStatoApprovatori.csv";
            string mime = "text/csv";
            return File( content.ToArray( ) , mime , filename );
        }

        public ActionResult GetBonusStats ( )
        {
            AlertModel model = new AlertModel( )
            {
                IntestazioneWidget = "DICHIARAZIONE PER LA FRUIZIONE DEL PREMIO RICONOSCIUTO AI LAVORATORI DIPENDENTI" ,
                Titolo = "Premio ai lavoratori dipendenti D.L. 18/20" ,
                CifraPrincipale = GetContatoreElenco().ToString(),
                TestoPulsante = "Esporta",
                HrefPulsante = Url.Action("EsportaBonus100", "Tech")                
            };           

            return View( "~/Views/_raiDesign/Widget_home.cshtml", model );
        }

        public ActionResult EsportaBonus100 ( )
        {
            byte[] content = null;

            var csv = new StringBuilder( );

            // Intestazione
            var newLine = string.Format( "Matricola;Nominativo;Data;Scelta" );
            csv.AppendLine( newLine );

            List<DettaglioSceltaDipendente> elenco = GetElenco( );

            if ( elenco != null && elenco.Any( ) )
            {
                foreach ( var item in elenco )
                {
                    var nl = string.Format( "{0};{1};{2};{3}" , item.Matricola , item.Nominativo , item.DataScelta.ToString( "dd/MM/yyyy" ) , item.Scelta );
                    csv.AppendLine( nl );
                }
            }

            content = Encoding.ASCII.GetBytes( csv.ToString( ) );

            string filename = "ExportScelteBonus100.csv";
            string mime = "text/csv";
            return File( content.ToArray( ) , mime , filename );
        }

        private int GetContatoreElenco ( )
        {
            int azioni = 0;
            try
            {
                using ( digiGappEntities db = new digiGappEntities( ) )
                {
                    azioni = db.MyRai_LogAzioni.Count( w => w.operazione.Equals( "Salvataggio Bonus 100" ) );
                }
            }
            catch ( Exception ex )
            {
                azioni = 0;
            }
            return azioni;
        }

        private List<DettaglioSceltaDipendente> GetElenco ( )
        {
            List<DettaglioSceltaDipendente> elenco = new List<DettaglioSceltaDipendente>( );
            List<MyRai_LogAzioni> azioni = new List<MyRai_LogAzioni>( );
            try
            {
                using ( digiGappEntities db = new digiGappEntities( ) )
                {
                    azioni = db.MyRai_LogAzioni.Where( w => w.operazione.Equals( "Salvataggio Bonus 100" ) ).ToList( );
                }

                if ( azioni != null && azioni.Any( ) )
                {
                    foreach ( var a in azioni )
                    {
                        elenco.Add( new DettaglioSceltaDipendente( )
                        {
                            Matricola = a.matricola ,
                            DataScelta = a.data ,
                            Scelta = a.descrizione_operazione.Replace( "Salvataggio scelta bonus100 - scelta - " , "" ) ,
                            Nominativo = CommonHelper.GetNominativoPerMatricola( a.matricola )
                        } );
                    }
                }
            }
            catch ( Exception ex )
            {

            }
            return elenco;
        }
    }
}