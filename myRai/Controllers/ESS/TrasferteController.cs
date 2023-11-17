using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MyRaiServiceInterface.MyRaiServiceReference1;

using myRaiHelper;
using myRaiData;
using myRaiCommonModel.ess;
using myRaiCommonManager;
using myRaiHelper;

namespace myRai.Controllers.ess
{
	public class TrasferteController : BaseCommonController
    {
		/// <summary>
		/// Render della view di index
		/// </summary>
		/// <returns></returns>
        public ActionResult Index(int daRendicontare = 0)
        {
			TrasferteControllerScope.Instance.Trasferta = new Trasferta();

			TrasferteResponseControllerScope.Instance.TrasferteResponse = null;

            return View( daRendicontare );
        }

        [HttpPost]
        public PartialViewResult GetTrasferteDaRendicontare()
        {
            TrasferteDaRendicontareVM model = new TrasferteDaRendicontareVM( );

            Trasferta trasferteDaRendicontare = null;

            if ( Session["TrasferteDaRendicontare"] == null )
            {
                Session["TrasferteDaRendicontare"] = TrasferteManager.GetTrasferteAnniPrecedenti( CommonHelper.GetCurrentUserMatricola( ) );
            }
            trasferteDaRendicontare = ( Trasferta ) Session["TrasferteDaRendicontare"];

            if ( trasferteDaRendicontare != null &&
                trasferteDaRendicontare.Viaggi != null &&
                trasferteDaRendicontare.Viaggi.Any( ) )
            {
                var fogliV = ( from v in trasferteDaRendicontare.Viaggi select v.FoglioViaggio ).ToList( );
                int tot = 0;
                if ( fogliV != null && fogliV.Any( ) )
                {
                    tot = fogliV.Distinct( ).Count( );
                }
                model.Conteggio = tot;
                string messaggio = tot > 1 ? "Hai " + tot + " fogli viaggio da rendicontare" : "Hai " + tot + " foglio viaggio da rendicontare";
                messaggio = tot == 0 ? "Hai " + tot + " fogli viaggio da rendicontare" : messaggio;
                model.Messaggio = messaggio;
                model.Url = Url.Action( "Index" , "Trasferte" , new { daRendicontare = 1 } );
            }
            else
            {
                string messaggio = "Hai 0 fogli viaggio da rendicontare";
                model.Messaggio = messaggio;
                model.Url = Url.Action( "Index" , "Trasferte" , new { daRendicontare = 1 } );
            }

            return PartialView( "~/Views/Scrivania/subpartial/trasfertePrecedenti.cshtml" , model );
        }

        /// <summary>
        /// Metodo chiamato alla pressione del tasto reset dei filtri di ricerca
        /// </summary>
        /// <param name="dataDa"></param>
        /// <param name="dataA"></param>
        /// <param name="foglioViaggio"></param>
        /// <param name="scopo"></param>
        /// <returns></returns>
        [HttpPost]
        public PartialViewResult ResetLoadTableTrasferte(string dataDa, string dataA, string foglioViaggio, string scopo, int daRendicontare = 0, TrasferteMacroStato macroStato = TrasferteMacroStato.Aperte)
        {
            AzzeraDatiSessione();
            return LoadTableTrasferte(dataDa, dataA, foglioViaggio, scopo, daRendicontare, macroStato);
        }

        /// <summary>
        /// Restituzione della vista con la tabella riferita alle trasferte effettuate
        /// dall'utente corrente, in base ai filtri passati
        /// </summary>
        /// <param name="dataDa"></param>
        /// <param name="dataA"></param>
        /// <param name="foglioViaggio"></param>
        /// <param name="scopo"></param>
        /// <returns></returns>
        [HttpPost]
        public PartialViewResult LoadTableTrasferte(string dataDa, string dataA, string foglioViaggio, string scopo, int daRendicontare = 0, TrasferteMacroStato macroStato = TrasferteMacroStato.Aperte)
		{
			try
			{
                TrasferteViewModel model = new TrasferteViewModel();

                Trasferta t = new Trasferta();
                model.Data = new Trasferta();

                if (!String.IsNullOrEmpty(dataA) ||
                    !String.IsNullOrEmpty(dataDa) ||
                    !String.IsNullOrEmpty(foglioViaggio) ||
                    !String.IsNullOrEmpty(scopo))
                {
                    AzzeraDatiSessione();
                    DateTime dt1;
                    DateTime dt2;
                    bool conv1 = DateTime.TryParseExact(dataDa, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dt1);
                    bool conv2 = DateTime.TryParseExact(dataA, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dt2);

                    if (!conv1)
                    {
                        dt1 = DateTime.MinValue;
                    }

                    if (!conv2)
                    {
                        dt2 = DateTime.MaxValue;
                    }

                    if (daRendicontare == 1)
                    {
                        t = TrasferteManager.GetTrasferteAnniPrecedenti(dt1, dt2, foglioViaggio, scopo);
                    }
                    else
                    {
                        t = GetTrasferte( dt1 , dt2 , foglioViaggio , scopo );
                    }
                }
                else
                {
                    if ( daRendicontare == 1 )
                    {
                        t = TrasferteManager.GetTrasferteAnniPrecedenti( );
                    }
                    else
                    {
                        t = GetTrasferte( );
                    }
                }

                if (t.Viaggi != null && t.Viaggi.Any())
                {
                    string tx = null;
                    string tconcl = null;

                    using (var db = new myRaiData.digiGappEntities())
                    {
                        var item = db.MyRai_ParametriSistema.Where(p => p.Chiave.Equals("TrasferteStatiEsclusi")).FirstOrDefault();

                        if (item != null)
                        {
                            tx = item.Valore1;
                        }

                        item = db.MyRai_ParametriSistema.Where(p => p.Chiave.Equals("TrasferteStatiConclusi")).FirstOrDefault();

                        if (item != null)
                        {
                            tconcl = item.Valore1;
                        }
                    }


                    var listViaggi = t.Viaggi.ToList();
                    switch (macroStato)
                    {
                        case TrasferteMacroStato.Aperte:
                            listViaggi.RemoveWhere(x => tconcl.Contains(x.Stato));
                            break;
                        case TrasferteMacroStato.Concluse:
                            listViaggi.RemoveWhere(x => !tconcl.Contains(x.Stato));
                            break;
                        default:
                            break;
                    }


                    if (listViaggi.Count() > GetLimitTrasferte())
                    {
                        model.HasNext = true;

                        listViaggi.Take(GetLimitTrasferte()).ToList().ForEach(w =>
                        {
                            if (w.Stato != null)
                            {
                                if (!tx.Contains(w.Stato))
                                {
                                    w.Rimborso = (double)TrasferteManager.CalcolaResiduoNetto(w.FoglioViaggio);
                                }
                            }
                        });

                        var arr = listViaggi.ToArray();
                        var newArray = new MyRaiServiceInterface.MyRaiServiceReference1.Viaggio[GetLimitTrasferte()];

                        Array.Copy(arr, newArray, GetLimitTrasferte());

                        model.Data = new Trasferta();

                        model.Data.Viaggi = newArray;

                        model.Data.CompetenzaDefinizione = t.CompetenzaDefinizione;
                    }
                    else
                    {
                        model.HasNext = false;

                        listViaggi.ToList().ForEach(w =>
                        {
                            if (!tx.Contains(w.Stato))
                            {
                                w.Rimborso = (double)TrasferteManager.CalcolaResiduoNetto(w.FoglioViaggio);
                            }
                        });

                        var arr = listViaggi.ToArray();
                        var newArray = new MyRaiServiceInterface.MyRaiServiceReference1.Viaggio[arr.Length];

                        Array.Copy(arr, newArray, arr.Length);

                        model.Data = new Trasferta();

                        model.Data.Viaggi = newArray;

                        model.Data.CompetenzaDefinizione = t.CompetenzaDefinizione;
                    }
                }
                else
                {
                    model.Data.CompetenzaDefinizione = t.CompetenzaDefinizione;
                }

                model.MacroStato = macroStato;
                model.Stati = TrasferteManager.GetStatiTrasferta();
                model.Page = 1;
                model.Size = GetLimitTrasferte();
                return PartialView( "~/Views/Trasferte/subpartial/TblTrasferte.cshtml", model );
			}
			catch ( Exception ex )
			{
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "LoadTableTrasferte"
                });

                return PartialView( "~/Views/Shared/TblError.cshtml", new HandleErrorInfo( ex, "TrasferteController", "LoadTableTrasferte" ) );
			}
		}

        /// <summary>
        /// Restituisce la vista con l'elenco delle trasferte per l'utente corrente in base ai filtri passati
        /// Tale metodo viene richiamato quando l'utente clicca sul bottone successivi N elementi
        /// </summary>
        /// <param name="dataDa"></param>
        /// <param name="dataA"></param>
        /// <param name="foglioViaggio"></param>
        /// <param name="scopo"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        [HttpPost]
        public PartialViewResult LoadTableTrasferteNext(string dataDa, string dataA, string foglioViaggio, string scopo, int page = 1, int size = 20, int daRendicontare = 0, TrasferteMacroStato macroStato = TrasferteMacroStato.Aperte)
        {
            try
            {
                TrasferteViewModel model = new TrasferteViewModel();

                Trasferta t = new Trasferta();
                model.Data = new Trasferta();

                if (!String.IsNullOrEmpty(dataA) ||
                    !String.IsNullOrEmpty(dataDa) ||
                    !String.IsNullOrEmpty(foglioViaggio))
                {
                    DateTime dt1;
                    DateTime dt2;
                    bool conv1 = DateTime.TryParseExact(dataDa, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dt1);
                    bool conv2 = DateTime.TryParseExact(dataA, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dt2);

                    if (!conv1)
                    {
                        dt1 = DateTime.MinValue;
                    }

                    if (!conv2)
                    {
                        dt2 = DateTime.MaxValue;
                    }

                    if ( daRendicontare == 1 )
                    {
                        t = TrasferteManager.GetTrasferteAnniPrecedenti(dt1, dt2, foglioViaggio, scopo);
                    }
                    else
                    {
                        t = GetTrasferte( dt1 , dt2 , foglioViaggio , scopo );
                    }
                }
                else
                {
                    if ( daRendicontare == 1 )
                    {
                        t = TrasferteManager.GetTrasferteAnniPrecedenti( );
                    }
                    else
                    {
                        t = GetTrasferte( );
                    }
                }

                if (t.Viaggi != null && t.Viaggi.Any())
                {
                    string tx = null;
                    string tconcl = null;
                    using (var db = new myRaiData.digiGappEntities())
                    {
                        var item = db.MyRai_ParametriSistema.Where(p => p.Chiave.Equals("TrasferteStatiEsclusi")).FirstOrDefault();

                        if (item != null)
                        {
                            tx = item.Valore1;
                        }

                        item = db.MyRai_ParametriSistema.Where(p => p.Chiave.Equals("TrasferteStatiConclusi")).FirstOrDefault();

                        if (item != null)
                        {
                            tconcl = item.Valore1;
                        }
                    }

                    var listViaggi = t.Viaggi.ToList();
                    switch (macroStato)
                    {
                        case TrasferteMacroStato.Aperte:
                            listViaggi.RemoveWhere(x => tconcl.Contains(x.Stato));
                            break;
                        case TrasferteMacroStato.Concluse:
                            listViaggi.RemoveWhere(x => !tconcl.Contains(x.Stato));
                            break;
                        default:
                            break;
                    }

                    if (listViaggi.Skip(page * size).Count() > GetLimitTrasferte())
                    {
                        model.HasNext = true;

                        listViaggi.Skip(page * size).Take(GetLimitTrasferte()).ToList().ForEach(w =>
                        {
                            if (w.Stato != null)
                            {
                                if (!tx.Contains(w.Stato))
                                {
                                    w.Rimborso = (double)TrasferteManager.CalcolaResiduoNetto(w.FoglioViaggio);
                                }
                            }
                        });

                        var newArray = new MyRaiServiceInterface.MyRaiServiceReference1.Viaggio[20];

                        var arr = listViaggi.Skip(page * size).ToArray();
                        Array.Copy(arr, newArray, (arr.Length > size ? size : arr.Length));

                        model.Data = new Trasferta();

                        model.Data.Viaggi = newArray;

                        model.Data.CompetenzaDefinizione = t.CompetenzaDefinizione;
                    }
                    else
                    {
                        model.HasNext = false;

                        listViaggi.Skip(page * size).ToList().ForEach(w =>
                        {
                            if (w.Stato != null)
                            {
                                if (!tx.Contains(w.Stato))
                                {
                                    w.Rimborso = (double)TrasferteManager.CalcolaResiduoNetto(w.FoglioViaggio);
                                }
                            }
                        });

                        var arr = listViaggi.Skip(page * size).ToArray();
                        var newArray = new MyRaiServiceInterface.MyRaiServiceReference1.Viaggio[arr.Length];

                        Array.Copy(arr, newArray, (arr.Length > size ? size : arr.Length));

                        model.Data = new Trasferta();

                        model.Data.Viaggi = newArray;

                        model.Data.CompetenzaDefinizione = t.CompetenzaDefinizione;
                    }
                }
                else
                {
                    model.Data.CompetenzaDefinizione = t.CompetenzaDefinizione;
                }
                model.MacroStato = macroStato;
                model.Stati = TrasferteManager.GetStatiTrasferta();
                model.Page = page + 1;
                model.Size = GetLimitTrasferte();
                return PartialView("~/Views/Trasferte/subpartial/TrTblTrasferte.cshtml", model);
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "LoadTableTrasferteNext"
                });

                return PartialView("~/Views/Shared/TblError.cshtml", new HandleErrorInfo(ex, "TrasferteController", "LoadTableTrasferte"));
            }
        }

        /// <summary>
        /// Render del box riepilogativo
        /// </summary>
        /// <returns></returns>
        [HttpPost]
		public PartialViewResult RiepilogoTrasferte ( int daRendicontare = 0 )
		{
			try
			{
				RiepilogoTrasferteVM model = new RiepilogoTrasferteVM();
                Trasferta data = new Trasferta( );

                if ( daRendicontare == 1 )
                {
                    data = TrasferteManager.GetTrasferteAnniPrecedenti( );
                }
                else
                {
                    data = GetTrasferte( );
                }

				if ( data.CompetenzaDefinizione != null )
				{
					model.TotaleAnticipi = data.CompetenzaDefinizione.CAnticipo;
					model.TotaleRimborsi = data.CompetenzaDefinizione.CRimborso;
					model.MeseCompetenza = data.CompetenzaDefinizione.MeseCompetenza.Trim();
				}
				else
				{
					model.TotaleAnticipi = 0.0;
					model.TotaleRimborsi = 0.0;
					model.MeseCompetenza = String.Empty;
				}

				return PartialView( "~/Views/Trasferte/subpartial/RiepilogoTrasferte.cshtml", model );
			}
			catch ( Exception ex )
			{
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "RiepilogoTrasferte"
                });
                return PartialView( "~/Views/Shared/TblError.cshtml", new HandleErrorInfo( ex, "TrasferteController", "RiepilogoTrasferte" ) );
			}
		}

		/// <summary>
		/// Reperimento delle informazioni riguardanti le trasferte in definizione
		/// e render delle info
		/// </summary>
		/// <returns></returns>
		[HttpPost]
		public PartialViewResult RiepilogoTrasferteInDefinizione ( int daRendicontare = 0 )
		{
			try
			{
				RiepilogoTrasferteInDefinizioneVM model = new RiepilogoTrasferteInDefinizioneVM();

                Trasferta data = new Trasferta( );

                if ( daRendicontare == 1 )
                {
                    data = TrasferteManager.GetTrasferteAnniPrecedenti( );
                }
                else
                {
                    data = GetTrasferte( );
                }

                if ( data.CompetenzaDefinizione != null )
				{
					model.SpesePreviste = data.CompetenzaDefinizione.DefPreviste;
					model.Anticipi = data.CompetenzaDefinizione.DefAnticipo;
					model.Rimborsi = data.CompetenzaDefinizione.DefRimborso;
				}
				else
				{
					model.SpesePreviste = 0.0;
					model.Anticipi = 0.0;
					model.Rimborsi = 0.0;
				}

				return PartialView( "~/Views/Trasferte/subpartial/RiepilogoTrasferteInDefinizione.cshtml", model );
			}
			catch ( Exception ex )
			{
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "RiepilogoTrasferteInDefinizione"
                });

                return PartialView( "~/Views/Shared/TblError.cshtml", new HandleErrorInfo( ex, "TrasferteController", "RiepilogoTrasferteInDefinizione" ) );
			}
		}

		/// <summary>
		/// Render della view di dettaglio di una trasferta
		/// </summary>
		/// <param name="foglioViaggio">Numero del foglio viaggio di cui si intende ottenere le info di dettaglio</param>
		/// <returns></returns>
		public PartialViewResult DettaglioTrasferta(string foglioViaggio)
		{
			try
			{
				DettaglioTrasfertaVM vm = new DettaglioTrasfertaVM();

				vm.FoglioViaggio = TrasferteManager.GetFoglioViaggio( foglioViaggio );
				vm.FViaggio = foglioViaggio;

				if ( vm.FoglioViaggio != null )
				{
					vm.StatoTrasferta = TrasferteManager.GetStatoTrasferta( vm.FoglioViaggio.STATO );
					vm.GrandeEvento = TrasferteManager.GetGrandeEvento( vm.FoglioViaggio.COD_GRANDI_EVENTI );
					vm.Itinerario = TrasferteManager.GetItinerario( foglioViaggio );
					vm.NotaSpeseTrasferta = TrasferteManager.GetNotaSpeseTrasferta( foglioViaggio );
					vm.Alberghi = TrasferteManager.GetAlberghi( foglioViaggio );
					vm.BigliettiRai = TrasferteManager.GetBigliettoRai( foglioViaggio );
					vm.Diaria = TrasferteManager.GetDiaria( foglioViaggio );
					vm.ResiduoNetto = TrasferteManager.CalcolaResiduoNetto( foglioViaggio );
				}

				return PartialView( "~/Views/Trasferte/subpartial/DettaglioTrasferta.cshtml", vm );
			}
			catch ( Exception ex )
			{
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "DettaglioTrasferta"
                });

                throw new Exception( ex.Message );
			}
		}
        
        /// <summary>
        /// Nel caso in cui non siano disponibili le info di dettaglio del foglio viaggio
        /// Verrà renderizzata la view Visualizza Biglietti che renderizza le info riguardanti
        /// i biglietti di viaggio associati a quel foglio viaggio
        /// </summary>
        /// <param name="foglioViaggio">Numero del foglio viaggio di cui si intende ottenere le info di dettaglio</param>
        /// <returns></returns>
        public PartialViewResult VisualizzaBiglietti(string foglioViaggio)
        {
            try
            {
                DettaglioTrasfertaVM vm = new DettaglioTrasfertaVM();

                vm.FoglioViaggio = TrasferteManager.GetFoglioViaggio(foglioViaggio);
                vm.FViaggio = foglioViaggio;

                if (vm.FoglioViaggio != null)
                {
                    vm.Itinerario = TrasferteManager.GetItinerario(foglioViaggio);
                }

                return PartialView("~/Views/Trasferte/subpartial/VisualizzaBiglietti.cshtml", vm);
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "VisualizzaBiglietti"
                });

                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Verifica se per quell'albergo l'utente ha già espresso un parere
        /// </summary>
        /// <param name="idAlbergo"></param>
        /// <returns></returns>
        [HttpPost]
		public ActionResult VerificaFeedbackAlbergo ( string idAlbergo )
		{
			CustomJsonResponse result = new CustomJsonResponse();
			try
			{
				int idForm = -1;

				if ( String.IsNullOrEmpty( idAlbergo ) )
				{
					throw new Exception( "Si è verificato un errore nel reperimento del questionario" );
				}

				//reperimento dell'idform per la compilazione del questionario degli alberghi
				using ( var db = new digiGappEntities() )
				{
					// reperimento della tipologia di questionario
					var tipologia = db.MyRai_FormTipologiaForm.Where( t => t.tipologia == "Hotel" ).FirstOrDefault();

					if ( tipologia == null )
					{
						throw new Exception( "Si è verificato un errore nel reperimento del questionario" );
					}

					var form = db.MyRai_FormPrimario.Where( f => f.attivo == true && f.id_tipologia == tipologia.id ).FirstOrDefault();

					if ( form == null )
					{
						throw new Exception( "Si è verificato un errore nel reperimento del questionario" );
					}

					idForm = form.id;

				}

				// a questo punto deve verificare se per quell'albergo l'utente ha già
				// espresso la valutazione
				using ( var db = new digiGappEntities() )
				{
					var fs = db.MyRai_FormSecondario.Where( s => s.attivo == true && s.id_form_primario == idForm && s.titolo.Equals( "Valutazione albergo" ) ).FirstOrDefault();

					if ( fs == null )
					{
						throw new Exception( "Si è verificato un errore nel reperimento del questionario" );
					}

					var fd = db.MyRai_FormDomande.Where( d => d.id_form_secondario.Equals( fs.id ) ).ToList();

					if ( fd == null || !fd.Any() )
					{
						throw new Exception( "Si è verificato un errore nel reperimento del questionario" );
					}

					string matricola = UtenteHelper.EsponiAnagrafica()._matricola;

					var myqy = from risposteDate in db.MyRai_FormRisposteDate
								  join formDomande in db.MyRai_FormDomande
									on risposteDate.id_domanda equals formDomande.id
								  where risposteDate.matricola == matricola &&
									  formDomande.id_form_secondario == fs.id
								  select new { risposteDate = risposteDate };

					// se è stata data almeno una risposta allora il form è da considerarsi
					// già valorizzato e non bisogna navigare al questionario
					if ( myqy.Count() == 0 )
					{
						result.Azione = Url.Action( "Fill", "FormUser", new
						{
							idform = idForm,
							idHotel = idAlbergo
						} );
					}
					else
					{
						result.Azione = String.Empty;
					}
				}
			}
			catch( Exception ex )
			{
				result.Errore = true;
				result.Messaggio = ex.Message;
				result.Azione = string.Empty;

                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "VerificaFeedback"
                });
            }
			return Json( result, JsonRequestBehavior.AllowGet );
		}

        /// <summary>
        /// Carica la lista delle trasferte in sessione
        /// Questo metodo viene chiamato prima di caricare i vari box, in questo modo verranno prima
        /// caricati i dati in sessione e successivamente i box effettueranno le loro operazioni
        /// sui dati memorizzati, tale scelta è stata fatta per evitare che venga chiamato più volte il
        /// servizio di gettrasferte per reperire le stesse info
        /// </summary>
        /// <returns></returns>
		[HttpPost]
		public ActionResult CaricaTrasferte (int daRendicontare = 0)
		{
			try
			{
				MyRaiService1Client service = new MyRaiService1Client();
				string matricola = UtenteHelper.EsponiAnagrafica()._matricola;
				TrasferteResponse serviceResponse = new TrasferteResponse();

				// se già è stato chiamato il servizio e i dati sono stati salvati in sessione
				// li recupera dalla sessione
				if ( TrasferteResponseControllerScope.Instance.TrasferteResponse != null )
				{
					serviceResponse = TrasferteResponseControllerScope.Instance.TrasferteResponse;
				}
				else
				{
                    if (daRendicontare == 0)
                    {
                        // chiama il servizio e salva la risposta in sessione
                        serviceResponse = service.GetTrasferte( matricola );
                        TrasferteResponseControllerScope.Instance.TrasferteResponse = new TrasferteResponse( );
                        TrasferteResponseControllerScope.Instance.TrasferteResponse = serviceResponse;
                    }
                    else
                    {
                        TrasferteResponseControllerScope.Instance.TrasferteResponse = null;
                    }
                }
			}
			catch ( Exception ex )
			{
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "CaricaTrasferte"
                });

                return Content( "false" );
			}

			return Content( "true" );
		}

        /// <summary>
        /// Reperimento delle trasferte per l'utente corrente
        /// </summary>
        /// <returns></returns>
        private Trasferta GetTrasferte ()
		{
			MyRaiService1Client service = new MyRaiService1Client();

			try
			{
				Trasferta t = new Trasferta();

				// se son già stati caricati i dati non verrà richiamato il servizio
				// in quanto non necessario
				if ( TrasferteControllerScope.Instance.Trasferta == null ||
					TrasferteControllerScope.Instance.Trasferta.Viaggi == null ||
					!TrasferteControllerScope.Instance.Trasferta.Viaggi.Any() )
				{
					string matricola = UtenteHelper.EsponiAnagrafica()._matricola;

					TrasferteResponse serviceResponse = new TrasferteResponse();

					// se già è stato chiamato il servizio e i dati sono stati salvati in sessione
					// li recupera dalla sessione
					if ( TrasferteResponseControllerScope.Instance.TrasferteResponse != null )
					{
						serviceResponse = TrasferteResponseControllerScope.Instance.TrasferteResponse;
					}
					else
					{
						// chiama il servizio e salva la risposta in sessione
						serviceResponse = service.GetTrasferte( matricola );
						TrasferteResponseControllerScope.Instance.TrasferteResponse = new TrasferteResponse();
						TrasferteResponseControllerScope.Instance.TrasferteResponse = serviceResponse;
					}

					if ( !serviceResponse.Esito )
					{
						if ( serviceResponse.ServiceResponse.Equals( "ACK91", StringComparison.InvariantCultureIgnoreCase ) )
						{
							serviceResponse.Errore = "DATI MOMENTANEAMENTE NON DISPONIBILI";
						}

						serviceResponse.Esito = true;
						serviceResponse.Trasferte = new Trasferta();

						// throw new Exception( serviceResponse.Errore );
					}

					List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio> lst = new List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio>();

					// se ci sono viaggi, va reperita la descrizione per ognuno di esso perchè non viene
					// restituita da CICS
					if ( serviceResponse.Trasferte.Viaggi != null && serviceResponse.Trasferte.Viaggi.Any() )
					{
                        var elencoFogli = "'" + String.Join("','", serviceResponse.Trasferte.Viaggi.Select(x => x.FoglioViaggio))+"'";
                        var fogliViaggio = TrasferteManager.GetFogliViaggioMulti(elencoFogli);

						foreach ( var v in serviceResponse.Trasferte.Viaggi )
						{
                            var fv = fogliViaggio.FirstOrDefault(x=>x.NUM_FOG==v.FoglioViaggio);

							if ( fv != null )
							{
								v.Descrizione = fv.SCOPO;
								v.Stato = fv.STATO;
                                v.Note = fv.ITINERARIO;
							}
                            else if (fv == null)
                            {
                                v.Descrizione = "";
                                if (String.IsNullOrEmpty(v.Stato))
                                {
                                    v.Stato = "";
                                }
                            }
                        }

                        lst = serviceResponse.Trasferte.Viaggi.OfType<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio>().ToList();
					}

					var vgg = TrasferteManager.GetTrasferteFromDB();

					if ( vgg != null && vgg.Any() )
					{
						if ( serviceResponse.Trasferte == null )
							serviceResponse.Trasferte = new Trasferta();

                        var newArray = new MyRaiServiceInterface.MyRaiServiceReference1.Viaggio[vgg.Count()];

                        Array.Copy(vgg.ToArray(), newArray, vgg.Count());

                        // rimuove tutti gli elementi da vgg che sono già contenuti in lst
                        vgg.RemoveAll(item => lst.Exists(i => i.FoglioViaggio.Equals(item.FoglioViaggio)));

                        lst.AddRange(vgg);

                        if (lst != null && lst.Any())
                        {
                            lst.ToList().ForEach(w =>
                            {
                                if (w.DataArrivoFromDB.Equals(DateTime.MinValue))
                                {
                                    // cerca in newArray il foglioviaggio
                                    // e prende il valore di DataArrivoFromDB

                                    var itemX = newArray.Where(r => r.FoglioViaggio.Equals(w.FoglioViaggio)).FirstOrDefault();
                                    if (itemX != null)
                                    {
                                        w.DataArrivoFromDB = itemX.DataArrivoFromDB;
                                    }

                                }
                            });
                        }
                    }

					if ( lst != null && lst.Any() )
					{
						lst.ForEach( i =>
						{
							if ( i.DataFromDB.Equals( DateTime.MinValue ) )
							{
								if ( i.Data.Length == 8 )
								{
									string sgg = i.Data.Substring( 0, 2 );
									string smm = i.Data.Substring( 3, 2 );
                                    string saa = i.Data.Substring(6, 2);

                                    int gg = int.Parse( sgg );
									int mm = int.Parse( smm );
                                    int aa = int.Parse(saa);
                                    aa += 2000;

									i.DataFromDB = new DateTime( aa, mm, gg );
								}
								else
								{
									i.DataFromDB = i.Data.ToDateTime( format: "ddMMyyyy" );
								}
							}

                           //switch (i.Stato)
                           //{
                           //    case "A":
                           //    case "B":
                           //        i.Note = string.Empty;
                           //        break;
                           //    case "C":
                           //    case "D":
                           //        i.Note = "Foglio di viaggio emesso";
                           //        break;
                           //    case "G":
                           //    case "H":
                           //    case "I":
                           //    case "L":
                           //    case "M":
                           //    case "N":
                           //    case "P":
                           //    case "Q":
                           //    case "R":
                           //    case "S":
                           //        i.Note = "Nota spese in elaborazione";
                           //        break;
                           //    case "T":
                           //    case "U":
                           //    case "V":
                           //    case "W":
                           //    case "X":
                           //        i.Note = "Nota spese completata";
                           //        break;
                           //    case "E":
                           //    case "F":
                           //    case "Y":
                           //        i.Note = "Foglio di viaggio annullato";
                           //        break;
                           //    default:
                           //        i.Note = "";
                           //        break;
                           //}
						} );


						lst = TrasferteManager.CalcolaRimborso( lst );

						serviceResponse.Trasferte.Viaggi = lst.OrderByDescending( d => d.DataFromDB ).ToArray();
					}

					TrasferteControllerScope.Instance.Trasferta = serviceResponse.Trasferte;

					return TrasferteControllerScope.Instance.Trasferta;
				}
				else
				{
					return TrasferteControllerScope.Instance.Trasferta;
				}
			}
			catch ( Exception ex )
			{
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "TrasferteController - GetTrasferte"
                });

                throw new Exception( ex.Message );
			}
		}

        /// <summary>
        /// Reperimento del limite di trasferte visualizzabili per volta
        /// di default 20
        /// </summary>
        /// <returns></returns>
        private int GetLimitTrasferte()
        {
            int result = 20;
            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    var rec = db.MyRai_ParametriSistema.Where(w => w.Chiave.Equals("GetLimitTrasferte")).FirstOrDefault();
                    if (rec != null)
                    {
                        result = int.Parse(rec.Valore1);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "GetLimitTrasferte"
                });

                result = 20;
            }

            return result;
        }

        /// <summary>
        /// Reperimento delle trasferte per l'utente corrente, in base ai filtri passati
        /// </summary>
        /// <param name="dt1"></param>
        /// <param name="dt2"></param>
        /// <param name="foglioViaggio"></param>
        /// <param name="scopo"></param>
        /// <returns></returns>
        private Trasferta GetTrasferte( DateTime dt1, DateTime dt2, string foglioViaggio, string scopo )
        {
            MyRaiService1Client service = new MyRaiService1Client();

            try
            {
                Trasferta t = new Trasferta();

                // se son già stati caricati i dati non verrà richiamato il servizio
                // in quanto non necessario
                if (TrasferteControllerScope.Instance.Trasferta == null ||
                    TrasferteControllerScope.Instance.Trasferta.Viaggi == null ||
                    !TrasferteControllerScope.Instance.Trasferta.Viaggi.Any())
                {
                    string matricola = UtenteHelper.EsponiAnagrafica()._matricola;

                    TrasferteResponse serviceResponse = new TrasferteResponse();

                    // se già è stato chiamato il servizio e i dati sono stati salvati in sessione
                    // li recupera dalla sessione
                    if (TrasferteResponseControllerScope.Instance.TrasferteResponse != null)
                    {
                        serviceResponse = TrasferteResponseControllerScope.Instance.TrasferteResponse;
                    }
                    else
                    {
                        // chiama il servizio e salva la risposta in sessione
                        serviceResponse = service.GetTrasferte(matricola);
                        TrasferteResponseControllerScope.Instance.TrasferteResponse = new TrasferteResponse();
                        TrasferteResponseControllerScope.Instance.TrasferteResponse = serviceResponse;
                    }

                    if (!serviceResponse.Esito)
                    {
                        if (serviceResponse.ServiceResponse.Equals("ACK91", StringComparison.InvariantCultureIgnoreCase))
                        {
                            serviceResponse.Errore = "DATI MOMENTANEAMENTE NON DISPONIBILI";
                        }

                        serviceResponse.Esito = true;
                        serviceResponse.Trasferte = new Trasferta();
                    }

                    List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio> lst = new List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio>();

                    // se ci sono viaggi, va reperita la descrizione per ognuno di esso perchè non viene
                    // restituita da CICS
                    if (serviceResponse.Trasferte.Viaggi != null && serviceResponse.Trasferte.Viaggi.Any())
                    {
                        var elencoFogli = "'" + String.Join("','", serviceResponse.Trasferte.Viaggi.Select(x => x.FoglioViaggio)) + "'";
                        var fogliViaggio = TrasferteManager.GetFogliViaggioMulti(elencoFogli);

                        foreach (var v in serviceResponse.Trasferte.Viaggi)
                        {
                            var fv = fogliViaggio.FirstOrDefault(x => x.NUM_FOG == v.FoglioViaggio);

                            if (fv != null)
                            {
                                v.Descrizione = fv.SCOPO;
                                v.Stato = fv.STATO;
                                v.Note = fv.ITINERARIO;
                            }
                            else if (fv == null)
                            {
                                v.Descrizione = "";
                                if (String.IsNullOrEmpty(v.Stato))
                                {
                                    v.Stato = "";
                                }
                            }
                        }

                        lst = serviceResponse.Trasferte.Viaggi.OfType<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio>().ToList();
                    }

                    if (lst != null && lst.Any())
                    {
                        List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio> tempLST = new List<MyRaiServiceInterface.MyRaiServiceReference1.Viaggio>();
                        DateTime tempDate = DateTime.Now;
                        foreach (var lstItem in lst)
                        {
                            if (!String.IsNullOrEmpty(foglioViaggio))
                            {
                                if (lstItem.FoglioViaggio.Equals(foglioViaggio))
                                {
                                    tempLST.Add(lstItem);
                                }
                            }
                            else
                            {
                                if (lstItem.Data.Length == 8)
                                {
                                    string sgg = lstItem.Data.Substring(0, 2);
                                    string smm = lstItem.Data.Substring(3, 2);
                                    string saa = lstItem.Data.Substring(6, 2);

                                    int gg = int.Parse(sgg);
                                    int mm = int.Parse(smm);
                                    int aa = int.Parse(saa);
                                    aa += 2000;

                                    tempDate = new DateTime(aa, mm, gg);
                                }
                                else if (lstItem.Data.Length == 10)
                                {
                                    string sgg = lstItem.Data.Substring(0, 2);
                                    string smm = lstItem.Data.Substring(3, 2);
                                    string saa = lstItem.Data.Substring(6, 4);

                                    int gg = int.Parse(sgg);
                                    int mm = int.Parse(smm);
                                    int aa = int.Parse(saa);

                                    tempDate = new DateTime(aa, mm, gg);
                                }

                                if (tempDate.Date >= dt1.Date &&
                                    tempDate.Date <= dt2.Date)
                                {
                                    tempLST.Add(lstItem);
                                }
                            }
                        }

                        if (tempLST != null && 
                            tempLST.Any() && 
                            !String.IsNullOrEmpty(scopo) && 
                            String.IsNullOrEmpty(foglioViaggio))
                        {
                            tempLST.RemoveAll(w => !w.Descrizione.Contains(scopo));
                        }

                        lst.Clear();
                        lst.AddRange(tempLST.ToList());
                    }
                    
                    var vgg = TrasferteManager.GetTrasferteFromDB(dt1, dt2, foglioViaggio, scopo);

                    if (vgg != null && vgg.Any())
                    {
                        if (serviceResponse.Trasferte == null)
                            serviceResponse.Trasferte = new Trasferta();

                        var newArray = new MyRaiServiceInterface.MyRaiServiceReference1.Viaggio[vgg.Count()];

                        Array.Copy(vgg.ToArray(), newArray, vgg.Count());

                        // rimuove tutti gli elementi da vgg che sono già contenuti in lst
                        vgg.RemoveAll(item => lst.Exists(i => i.FoglioViaggio.Equals(item.FoglioViaggio)));

                        lst.AddRange(vgg);

                        if (lst != null && lst.Any())
                        {
                            lst.ToList().ForEach(w =>
                            {
                                if (w.DataArrivoFromDB.Equals(DateTime.MinValue))
                                {
                                    // cerca in newArray il foglioviaggio
                                    // e prende il valore di DataArrivoFromDB
                                    var itemX = newArray.Where(r => r.FoglioViaggio.Equals(w.FoglioViaggio)).FirstOrDefault();
                                    if (itemX != null)
                                    {
                                        w.DataArrivoFromDB = itemX.DataArrivoFromDB;
                                    }
                                }
                            });
                        }
                    }

                    if (lst != null && lst.Any())
                    {
                        lst.ForEach(i =>
                        {
                            if (i.DataFromDB.Equals(DateTime.MinValue))
                            {
                                if (i.Data.Length == 8)
                                {
                                    string sgg = i.Data.Substring(0, 2);
                                    string smm = i.Data.Substring(3, 2);
                                    string saa = i.Data.Substring(6, 2);

                                    int gg = int.Parse(sgg);
                                    int mm = int.Parse(smm);
                                    int aa = int.Parse(saa);
                                    aa += 2000;

                                    i.DataFromDB = new DateTime(aa, mm, gg);
                                }
                                else
                                {
                                    i.DataFromDB = i.Data.ToDateTime(format: "ddMMyyyy");
                                }
                            }

                            //switch (i.Stato)
                            //{
                            //    case "A":
                            //    case "B":
                            //        i.Note = string.Empty;
                            //        break;
                            //    case "C":
                            //    case "D":
                            //        i.Note = "Foglio di viaggio emesso";
                            //        break;
                            //    case "G":
                            //    case "H":
                            //    case "I":
                            //    case "L":
                            //    case "M":
                            //    case "N":
                            //    case "P":
                            //    case "Q":
                            //    case "R":
                            //    case "S":
                            //        i.Note = "Nota spese in elaborazione";
                            //        break;
                            //    case "T":
                            //    case "U":
                            //    case "V":
                            //    case "W":
                            //    case "X":
                            //        i.Note = "Nota spese completata";
                            //        break;
                            //    case "E":
                            //    case "F":
                            //    case "Y":
                            //        i.Note = "Foglio di viaggio annullato";
                            //        break;
                            //    default:
                            //        i.Note = "";
                            //        break;
                            //}
                        });


                        lst = TrasferteManager.CalcolaRimborso(lst);

                        serviceResponse.Trasferte.Viaggi = lst.OrderByDescending(d => d.DataFromDB).ToArray();
                    }
                    else
                    {
                        serviceResponse.Trasferte.Viaggi = lst.ToArray();
                    }

                    TrasferteControllerScope.Instance.Trasferta = serviceResponse.Trasferte;

                    return TrasferteControllerScope.Instance.Trasferta;
                }
                else
                {
                    return TrasferteControllerScope.Instance.Trasferta;
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "TrasferteController - GetTrasferte"
                });
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Metodo per la cancellazione dei dati in sessione
        /// Richiamato quando un utente effettua una ricerca
        /// impostando dei filtri, oppure quando fa il reset della ricerca
        /// </summary>
        private void AzzeraDatiSessione()
        {
            TrasferteControllerScope.Instance.Trasferta = null;
        }
    }

	public class TrasferteControllerScope : SessionScope<TrasferteControllerScope>
	{
		public TrasferteControllerScope ()
		{
			this.Trasferta = new Trasferta();
		}

		public Trasferta Trasferta
		{
			get
			{
				return this._trasferta;
			}
			set
			{
				this._trasferta = value;
			}
		}

		private Trasferta _trasferta = null;
	}

	public class TrasferteResponseControllerScope : SessionScope<TrasferteResponseControllerScope>
	{
		public TrasferteResponseControllerScope ()
		{
			this.TrasferteResponse = new TrasferteResponse();
		}

		public TrasferteResponse TrasferteResponse
		{
			get
			{
				return this._trasferteResponse;
			}
			set
			{
				this._trasferteResponse = value;
			}
		}

		private TrasferteResponse _trasferteResponse = null;
	}
}