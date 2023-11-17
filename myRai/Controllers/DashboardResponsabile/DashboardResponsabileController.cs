using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using myRai.DataControllers;
using myRaiCommonModel.DashboardResponsabile;
using MyRaiServiceInterface.MyRaiServiceReference1;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using Utente = myRai.Models.Utente;
using pianoFerieSedeGapp = MyRaiServiceInterface.it.rai.servizi.digigappws.pianoFerieSedeGapp;
using myRaiData;
using myRaiHelper;
using Dipendente = MyRaiServiceInterface.it.rai.servizi.digigappws.Dipendente;
using myRaiCommonModel;
using myRai.Business;

namespace myRai.Controllers.DashboardResponsabile
{
    public class DashboardResponsabileController : BaseCommonController
    {
		/// <summary>
		/// Reperimento delle sedi per le quali l'utente è responsabile
		/// </summary>
		/// <returns></returns>
		private string GetSedeResponsabile ()
		{
			string sede = "";
			var response = Utility.GetSediGappResponsabileList();

			if ( response != null && response.Any() )
			{
				sede = response[0].CodiceSede;
			}

			return sede;
		}

        public ActionResult Index()
        {
            DashBoardResponsabileCustomVM model = new DashBoardResponsabileCustomVM( );

            model.Rows = new List<WidgetRow>( );
            WidgetRow row = new WidgetRow( );
            WidgetVM widget = new WidgetVM( );
            widget.WidgetId = "pianificazioneAttivita";
            widget.Title = "Pianificazione attività";
            widget.ActionName = "PianificazioneAttivita";
            row.Widgets.Add( widget );

            WidgetRow row2 = new WidgetRow( );
            WidgetVM widget2 = new WidgetVM( );
            widget2.WidgetId = "statoFerie";
            widget2.Title = "Ferie";
            widget2.ActionName = "ChartStatoFerie";
            row2.Widgets.Add( widget2 );

            model.Rows.Add( row );
            model.Rows.Add( row2 );

            return View( "~/Views/DashboardResponsabile/Index.cshtml" , model );
        }

		public ActionResult Report_Carenza_MP ( string sede, int mese )
		{
			sede = GetSedeResponsabile();
            DashboardResponsabileVM model = new DashboardResponsabileVM();
            model.WidgetId = "car_mp_";
            //sede = "DDE30";
            //mese = 1;
            try
            {
                model.MeseRiferimento = mese;

                MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client service = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();

                int anno = DateTime.Now.Year;

                var result = service.GetReport_Carenza_MP(Utente.Matricola(), sede, mese, anno, true);

                if (result != null && result.Esito)
                {
                    if (result.Risposta != null && result.Risposta.Any())
                    {
                        foreach(var item in result.Risposta)
                        {
                            model.Items.Add(new DashboardResponsabileItem()
                            {
                                Nominativo = item.Nominativo,
                                Foto = CommonManager.GetUrlFoto(item.Matricola.Substring(1, item.Matricola.Length - 1)),
                                ListaCarMp = item.ListaCarMp.ToList().Select(x => new ItemCarenzaMaggiorPresenza { CAR = x.CAR, MP = x.MP, Settimana = x.Settimana }).ToList(),
                                Saldo = item.Saldo,
                                NumeroOccorrenze = item.NumeroOccorrenze,
                                TotaleOreLista1 = item.TotaleOreLista1,
                                TotaleOreLista2 = item.TotaleOreLista2
                            });
                        }
                    }
                }
                else if (result != null && !String.IsNullOrEmpty(result.Errore))
                {
                    myRaiCommonDatacontrollers.Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                    {
                        matricola = Utente.Matricola(),
                        error_message = result.Errore
                    });
                }
            }
            catch(Exception ex)
            {
                myRaiCommonDatacontrollers.Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    matricola = Utente.Matricola(),
                    error_message = ex.StackTrace
                });
            }

            return View("~/Views/DashboardResponsabile/Report_CAR_MP.cshtml", model);
        }

        public ActionResult Report_POH_ROH(string sede, int mese)
        {
            sede = GetSedeResponsabile();
            DashboardResponsabileVM model = new DashboardResponsabileVM();
            model.WidgetId = "roh_poh_";
            sede = "DDE40";
            mese = 5;
            try
            {
                model.MeseRiferimento = mese;

                MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client service = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();

                int anno = DateTime.Now.Year;

                var result = service.GetReport_POH_ROH(Utente.Matricola(), sede, mese, anno);

                if (result != null && result.Esito)
                {
                    if (result.Risposta != null && result.Risposta.Any())
                    {
                        foreach (var item in result.Risposta)
                        {
                            model.Items.Add(new DashboardResponsabileItem()
                            {
                                ListaPOH = item.ListaPOH.ToList().Select(x => new UtenteDataControllerResult { Giorno = x.Giorno, Minuti = x.Minuti }).ToList(),
                                ListaROH = item.ListaROH.ToList().Select(x => new UtenteDataControllerResult { Giorno = x.Giorno, Minuti = x.Minuti }).ToList(),
                                Nominativo = item.Nominativo,
                                Foto = CommonManager.GetUrlFoto(item.Matricola.Substring(1, item.Matricola.Length - 1)),
                                Saldo = item.Saldo,
                                NumeroOccorrenze = item.NumeroOccorrenze,
                                TotaleOreLista1 = item.TotaleOreLista1,
                                TotaleOreLista2 = item.TotaleOreLista2
                            });
                        }
                    }
                }
                else if (result != null && !String.IsNullOrEmpty(result.Errore))
                {
                    myRaiCommonDatacontrollers.Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                    {
                        matricola = Utente.Matricola(),
                        error_message = result.Errore
                    });
                }
            }
            catch (Exception ex)
            {
                myRaiCommonDatacontrollers.Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    matricola = Utente.Matricola(),
                    error_message = ex.StackTrace
                });
            }

            return View("~/Views/DashboardResponsabile/Report_POH_ROH.cshtml", model);
        }

        public ActionResult Report_STR_STRF(string sede, int mese)
        {
            sede = GetSedeResponsabile();
            DashboardResponsabileVM model = new DashboardResponsabileVM();
            model.WidgetId = "str_strf_";
            sede = "DDE30";
            mese = 1;
            try
            {
                model.MeseRiferimento = mese;

                MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client service = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();

                int anno = DateTime.Now.Year;

                var result = service.GetReport_STR_STRF(Utente.Matricola(), sede, mese, anno);

                if (result != null && result.Esito)
                {
                    if (result.Risposta != null && result.Risposta.Any())
                    {
                        foreach (var item in result.Risposta)
                        {
                            model.Items.Add(new DashboardResponsabileItem()
                            {
                                ListaSTR = item.ListaSTR.ToList().Select(x => new UtenteDataControllerResult { Giorno = x.Giorno, Minuti = x.Minuti }).ToList(),
                                ListaSTRF = item.ListaSTRF.ToList().Select(x => new UtenteDataControllerResult { Giorno = x.Giorno, Minuti = x.Minuti }).ToList(),
                                Nominativo = item.Nominativo,
                                Foto = CommonManager.GetUrlFoto(item.Matricola.Substring(1, item.Matricola.Length - 1)),
                                Saldo = item.Saldo,
                                NumeroOccorrenze = item.NumeroOccorrenze,
                                TotaleOreLista1 = item.TotaleOreLista1,
                                TotaleOreLista2 = item.TotaleOreLista2
                            });
                        }
                    }
                }
                else if (result != null && !String.IsNullOrEmpty(result.Errore))
                {
                    myRaiCommonDatacontrollers.Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                    {
                        matricola = Utente.Matricola(),
                        error_message = result.Errore
                    });
                }
            }
            catch (Exception ex)
            {
                myRaiCommonDatacontrollers.Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    matricola = Utente.Matricola(),
                    error_message = ex.StackTrace
                });
            }

            return View("~/Views/DashboardResponsabile/Report_STR_STRF.cshtml", model);
        }

        public ActionResult Report_Reperibilita(string sede, int mese)
        {
            sede = GetSedeResponsabile();
            DashboardResponsabileVM model = new DashboardResponsabileVM();
            model.WidgetId = "reperibilita_";
            sede = "DDE30";
            mese = 1;
            try
            {
                model.MeseRiferimento = mese;

                MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client service = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();

                int anno = DateTime.Now.Year;

                var result = service.GetReport_Reperibilita(Utente.Matricola(), sede, mese, anno);

                if (result != null && result.Esito)
                {
                    if (result.Risposta != null && result.Risposta.Any())
                    {
                        foreach (var item in result.Risposta)
                        {
                            model.Items.Add(new DashboardResponsabileItem()
                            {
                                ListaRE20 = item.ListaRE20.ToList().Select(x => new UtenteDataControllerResult { Giorno = x.Giorno, Minuti = x.Minuti }).ToList(),
                                ListaRE22 = item.ListaRE22.ToList().Select(x => new UtenteDataControllerResult { Giorno = x.Giorno, Minuti = x.Minuti }).ToList(),
                                ListaRE25 = item.ListaRE25.ToList().Select(x => new UtenteDataControllerResult { Giorno = x.Giorno, Minuti = x.Minuti }).ToList(),
                                Nominativo = item.Nominativo,
                                Foto = CommonManager.GetUrlFoto(item.Matricola.Substring(1, item.Matricola.Length - 1)),
                                Saldo = item.Saldo,
                                NumeroOccorrenze = item.NumeroOccorrenze,
                                TotaleOreLista1 = item.TotaleOreLista1,
                                TotaleOreLista2 = item.TotaleOreLista2,
                                TotaleOreLista3 = item.TotaleOreLista3
                            });
                        }
                    }
                }
                else if (result != null && !String.IsNullOrEmpty(result.Errore))
                {
                    myRaiCommonDatacontrollers.Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                    {
                        matricola = Utente.Matricola(),
                        error_message = result.Errore
                    });
                }
            }
            catch (Exception ex)
            {
                myRaiCommonDatacontrollers.Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                {
                    matricola = Utente.Matricola(),
                    error_message = ex.StackTrace
                });
            }

            return View("~/Views/DashboardResponsabile/Report_Reperibilita.cshtml", model);
        }

        public ActionResult PianificazioneAttivita()
        {
            PianificazioneAttivitaVM model = new PianificazioneAttivitaVM( );
            model.SediGapp = Utility.GetSediGappResponsabileList( );

            return View( "~/Views/DashboardResponsabile/_subpartial/PianificazioneAttivita.cshtml" , model );
        }

        public ActionResult CaricaDatiSede(string sede)
        {
            TabellaGiornateVM model = new TabellaGiornateVM( );
            MyRaiService1Client wcf = new MyRaiService1Client( );
            DateTime dataPartenza = DateTime.Now;
            dataPartenza = dataPartenza.AddDays( -15 );
            ConteggioGiorniConsecutivi_Response giorniConsecutivi = wcf.GetGiorniConsecutivi( CommonManager.GetCurrentUserMatricola( ) , sede , dataPartenza );

            model.DataPartenza = dataPartenza;

            if ( !giorniConsecutivi.Esito )
            {
                Logger.LogErrori( new myRaiData.MyRai_LogErrori( )
                {
                    applicativo = "Portale" ,
                    data = DateTime.Now ,
                    error_message = giorniConsecutivi.Errore ,
                    matricola = CommonManager.GetCurrentUserMatricola( ) ,
                    provenienza = "CaricaDatiSede - RaiPerMe "
                } );
                throw new Exception( giorniConsecutivi.Errore );
            }

            model.Dati = new List<ConteggioGiorniConsecutivi>( );
            //giorniConsecutivi.Risposta.ToList( ).ForEach(w => {
            //    var y = w.Giorni.Count( g => g.ConteggioOccorrenza > 6 );

            //    if (y > 0)
            //    {
            //        ConteggioGiorniConsecutivi newItem = new ConteggioGiorniConsecutivi( );
            //        newItem = w;
            //        model.Dati.Add( newItem );
            //    }
            //});
            model.Dati = giorniConsecutivi.Risposta.ToList( );

            return View( "~/Views/DashboardResponsabile/_subpartial/TabellaGiornate.cshtml" , model );
        }

        #region Chart Stato Ferie

        private StatoFerieVM GetModelCalcolato ( string sede = null, int? anno = null , int page = 1, int maxElement = 60 )
        {
            if ( anno == null )
                anno = DateTime.Now.Year;            

            StatoFerieVM model = new StatoFerieVM( );
            model.ItemsInlist = maxElement;

            var sedi = Utility.GetSediGappResponsabileList( );

            var ListSediReparti = getSediReparti( sedi , ( int ) anno );
            model.SediGapp = ListSediReparti;

            if ( sede == null )
            {
                sede = ListSediReparti.FirstOrDefault( ).value;
            }

            model.CodiceSedeSelected = sede;

            PianoFerieApprovatoreModel dati = null;

            string sessionN = "GetPianoFerieModel" + sede;

            dati = ( PianoFerieApprovatoreModel ) SessionManager.Get( sessionN );

            if ( dati == null )
            {
                dati = GetPianoFerieModel( sede , anno );
                SessionManager.Set( sessionN , dati );
            }

            ParametriGrafico temp = new ParametriGrafico( );
            temp.Giorno = new List<DateTime>( );
            temp.TotalePerGiorno = new List<int>( );

            DateTime begin = new DateTime( DateTime.Now.Year , 1 , 1 );
            DateTime end = new DateTime( DateTime.Now.Year , 12 , 31 );

            if ( dati.AlmenoUnUtenteRichiede2021 )
            {
                end = new DateTime( DateTime.Now.Year + 1 , 3 , 31 );
            }

            model.Pianificato = new ParametriGrafico( );
            model.Effettivo = new ParametriGrafico( );
            model.Pianificato.Giorno = new List<DateTime>( );
            model.Pianificato.TotalePerGiorno = new List<int>( );
            model.Effettivo.Giorno = new List<DateTime>( );
            model.Effettivo.TotalePerGiorno = new List<int>( );

            for ( DateTime date = begin ; date <= end ; date = date.AddDays( 1 ) )
            {
                temp.Giorno.Add( date );
                temp.TotalePerGiorno.Add( 0 );

                model.Pianificato.Giorno.Add( date );
                model.Pianificato.TotalePerGiorno.Add( 0 );
                model.Effettivo.Giorno.Add( date );
                model.Effettivo.TotalePerGiorno.Add( 0 );
            }

            #region CALCOLO PIANIFICATI
            foreach ( var item in dati.PianoFerieDipendenti )
            {
                foreach ( var g in item.Gapp_FE )
                {
                    if ( model.Pianificato.Giorno != null && model.Pianificato.Giorno.Any( ) && model.Pianificato.Giorno.Contains( g ) )
                    {
                        int index = model.Pianificato.Giorno.FindIndex( a => a.Equals( g ) );
                        model.Pianificato.TotalePerGiorno[index]++;
                    }
                }

                foreach ( var g in item.Gapp_PF )
                {
                    if ( model.Pianificato.Giorno != null && model.Pianificato.Giorno.Any( ) && model.Pianificato.Giorno.Contains( g ) )
                    {
                        int index = model.Pianificato.Giorno.FindIndex( a => a.Equals( g ) );
                        model.Pianificato.TotalePerGiorno[index]++;
                    }
                }

                foreach ( var g in item.Gapp_PR )
                {
                    if ( model.Pianificato.Giorno != null && model.Pianificato.Giorno.Any( ) && model.Pianificato.Giorno.Contains( g ) )
                    {
                        int index = model.Pianificato.Giorno.FindIndex( a => a.Equals( g ) );
                        model.Pianificato.TotalePerGiorno[index]++;
                    }
                }

                foreach ( var g in item.GiorniFerie )
                {
                    if ( model.Pianificato.Giorno != null && model.Pianificato.Giorno.Any( ) && model.Pianificato.Giorno.Contains( g ) )
                    {
                        int index = model.Pianificato.Giorno.FindIndex( a => a.Equals( g ) );
                        model.Pianificato.TotalePerGiorno[index]++;
                    }
                }

                foreach ( var g in item.GiorniPF )
                {
                    if ( model.Pianificato.Giorno != null && model.Pianificato.Giorno.Any( ) && model.Pianificato.Giorno.Contains( g ) )
                    {
                        int index = model.Pianificato.Giorno.FindIndex( a => a.Equals( g ) );
                        model.Pianificato.TotalePerGiorno[index]++;
                    }
                }

                foreach ( var g in item.GiorniPR )
                {
                    if ( model.Pianificato.Giorno != null && model.Pianificato.Giorno.Any( ) && model.Pianificato.Giorno.Contains( g ) )
                    {
                        int index = model.Pianificato.Giorno.FindIndex( a => a.Equals( g ) );
                        model.Pianificato.TotalePerGiorno[index]++;
                    }
                }

                foreach ( var g in item.GiorniPX )
                {
                    if ( model.Pianificato.Giorno != null && model.Pianificato.Giorno.Any( ) && model.Pianificato.Giorno.Contains( g ) )
                    {
                        int index = model.Pianificato.Giorno.FindIndex( a => a.Equals( g ) );
                        model.Pianificato.TotalePerGiorno[index]++;
                    }
                }

                foreach ( var g in item.GiorniRN )
                {
                    if ( model.Pianificato.Giorno != null && model.Pianificato.Giorno.Any( ) && model.Pianificato.Giorno.Contains( g ) )
                    {
                        int index = model.Pianificato.Giorno.FindIndex( a => a.Equals( g ) );
                        model.Pianificato.TotalePerGiorno[index]++;
                    }
                }

                foreach ( var g in item.GiorniRR )
                {
                    if ( model.Pianificato.Giorno != null && model.Pianificato.Giorno.Any( ) && model.Pianificato.Giorno.Contains( g ) )
                    {
                        int index = model.Pianificato.Giorno.FindIndex( a => a.Equals( g ) );
                        model.Pianificato.TotalePerGiorno[index]++;
                    }
                }
            }
            #endregion
            // a questo punto abbiamo calcolato tutto il periodo pianificato

            int sum = 0;
            foreach ( var t in model.Pianificato.TotalePerGiorno )
            {
                sum += t;
            }

            model.GiorniPianificatiTotali = sum;

            #region CALCOLO EFFETTIVI            

            WSDigigapp wsService = new WSDigigapp( )
            {
                Credentials = new System.Net.NetworkCredential(
                    CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[0] ,
                    CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[1] )
            };

            string sessionName = "getPianoFerieSedeGapp" + sede.Substring( 0 , 5 );

            pianoFerieSedeGapp resp = ( pianoFerieSedeGapp ) SessionManager.Get( sessionName );
            pianoFerieSedeGapp resp2 = null;

            if ( resp == null )
            {
                resp = wsService.getPianoFerieSedeGapp( "P" + Utente.Matricola( ) , "01" + "01" + anno , "" , "" , sede.Substring( 0 , 5 ) , 75 );
                SessionManager.Set( sessionName , resp );
            }

            if ( dati.AlmenoUnUtenteRichiede2021 )
            {
                sessionName = "getPianoFerieSedeGapp_2_" + sede.Substring( 0 , 5 );

                resp2 = ( pianoFerieSedeGapp ) SessionManager.Get( sessionName );

                if ( resp2 == null )
                {
                    resp2 = wsService.getPianoFerieSedeGapp( "P" + Utente.Matricola( ) , "01" + "01" + ( anno + 1 ) , "" , "" , sede.Substring( 0 , 5 ) , 75 );
                    SessionManager.Set( sessionName , resp2 );
                }
            }

            for ( DateTime date = begin ; date <= end ; date = date.AddDays( 1 ) )
            {
                int index = model.Effettivo.Giorno.FindIndex( a => a.Equals( date ) );

                if ( date <= DateTime.Now.Date )
                {
                    if ( resp.esito )
                    {
                        // calcolo del totale effettivo
                        foreach ( var item in dati.PianoFerieDipendenti )
                        {
                            if ( resp.dipendenti != null && resp.dipendenti.Any( ) )
                            {
                                var dip = resp.dipendenti.Where( w => w.matricola.Equals( "0" + item.Matricola ) ).FirstOrDefault( );
                                if ( dip != null )
                                {
                                    var giorno = dip.ferie.giornate.Where( w => w.data.Date.Equals( date ) ).FirstOrDefault( );
                                    if ( giorno != null )
                                    {
                                        if ( !string.IsNullOrEmpty( giorno.tipoGiornata ) &&
                                            ( giorno.tipoGiornata.Equals( "D" ) ||
                                            giorno.tipoGiornata.Equals( "1" ) ||
                                            giorno.tipoGiornata.Equals( "2" ) ) )
                                        {
                                            model.Effettivo.TotalePerGiorno[index]++;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if ( resp2 != null && resp2.esito )
                    {
                        // calcolo del totale effettivo
                        foreach ( var item in dati.PianoFerieDipendenti )
                        {
                            if ( resp2.dipendenti != null && resp2.dipendenti.Any( ) )
                            {
                                var dip = resp2.dipendenti.Where( w => w.matricola.Equals( "0" + item.Matricola ) ).FirstOrDefault( );
                                if ( dip != null )
                                {
                                    var giorno = dip.ferie.giornate.Where( w => w.data.Date.Equals( date ) ).FirstOrDefault( );
                                    if ( giorno != null )
                                    {
                                        if ( !string.IsNullOrEmpty( giorno.tipoGiornata ) &&
                                            ( giorno.tipoGiornata.Equals( "D" ) ||
                                            giorno.tipoGiornata.Equals( "1" ) ||
                                            giorno.tipoGiornata.Equals( "2" ) ) )
                                        {
                                            model.Effettivo.TotalePerGiorno[index]++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            #endregion

            sum = 0;
            foreach ( var t in model.Effettivo.TotalePerGiorno )
            {
                sum += t;
            }

            model.GiorniEffettiviTotali = sum;
            model.Page = page;

            return model;

        }

        public ActionResult AnteprimaStatoFerie ( )
        {
            StatoFerieStatsVM model = new StatoFerieStatsVM( );
            model.WidgetId = "wdgStatoFerieStats";
            model.Dati = new List<StatoFerieObj>( );

            var sedi = Utility.GetSediGappResponsabileList( );

            var listSediReparti = getSediReparti( sedi , ( int ) DateTime.Now.Year );

            foreach ( var sede in listSediReparti )
            {
                var itemResult = GetModelFromCache( sede.value.Replace( "-" , "" ) );
                if ( itemResult != null )
                {
                    model.Dati.Add( itemResult );
                }
            }

            return View( "~/Views/DashboardResponsabile/_subpartial/StatoFerieStats.cshtml" , model );
        }

        private StatoFerieObj GetModelFromCache ( string sede )
        {
            StatoFerieObj objDeserialized = null;

            try
            {
                using ( digiGappEntities db = new digiGappEntities( ) )
                {
                    sede = sede.Replace( "-" , "" );
                    string tag = "StatoFerie" + sede;
                    var exists = db.MyRai_CacheFunzioni.Where( w => w.oggetto.Equals( tag ) ).FirstOrDefault( );

                    if (exists != null)
                    {
                        objDeserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<StatoFerieObj>( exists.dati_serial );
                    }
                }
            }
            catch ( Exception ex )
            {

            }

            return objDeserialized;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sede"></param>
        /// <param name="anno"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult ChartStatoFerie ( string sede = null , int? anno = null , int page = 1, string descSede = "" )
        {
            StatoFerieVM model = new StatoFerieVM( );
            model.ItemsInlist = 60;
            model.EtichetteGrafico = new List<string>( );

            if ( anno == null )
                anno = DateTime.Now.Year;

            if ( !String.IsNullOrEmpty( descSede ))
            {
                model.DescrizioneSedeGapp = descSede;
            }

            WSDigigapp wsService = new WSDigigapp( )
            {
                Credentials = new System.Net.NetworkCredential(
                        CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[0] ,
                        CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[1] )
            };

            string repartoReq = null;

            if ( sede.Length > 5 )
            {
                repartoReq = sede.Substring( 5 );
            }

            string sessionName = "getPianoFerieSedeGapp" + sede.Substring( 0 , 5 );

            pianoFerieSedeGapp resp = ( pianoFerieSedeGapp ) SessionManager.Get( sessionName );

            if ( resp == null )
            {
                resp = wsService.getPianoFerieSedeGapp( "P" + Utente.Matricola( ) , "01" + "01" + anno , "" , "" , sede.Substring( 0 , 5 ) , 75 );
                SessionManager.Set( sessionName , resp );
            }

            var db = new myRaiData.digiGappEntities( );

            var AB = CommonManager.getAbilitazioni( );
            List<string> SediAb = AB.ListaAbilitazioni.Where( x => x.Sede.ToUpper( ).StartsWith( sede.Substring( 0 , 5 ).ToUpper( ) ) ).Select( x => x.Sede ).ToList( );

            Dipendente[] MieiDipendenti = null;

            if ( repartoReq == null )
                MieiDipendenti = resp.dipendenti.Where( x => !SediAb.Contains( sede + x.reparto ) || String.IsNullOrWhiteSpace( x.reparto ) || x.reparto.Trim( ) == "0" || x.reparto.Trim( ) == "00" ).ToArray( );
            else
                MieiDipendenti = resp.dipendenti.Where( x => x.reparto == repartoReq ).ToArray( );

            var itemResult = GetModelFromCache( sede.Replace( "-" , "" ) );

            if ( itemResult != null )
            {
                List<string> matricoleMieiDipendenti = new List<string>( );
                matricoleMieiDipendenti.AddRange( MieiDipendenti.Select( x => x.matricola.Substring( 1 ) ).ToList( ) );

                model.Pianificato = itemResult.Pianificato;
                model.Effettivo = itemResult.Effettivo;
                model.GiorniPianificatiTotali = itemResult.GiorniPianificatiTotali;
                model.GiorniEffettiviTotali = itemResult.GiorniEffettiviTotali;
                model.GiorniPianificatiAdOggi = itemResult.GiorniPianificatiAdOggi;
                model.GiorniEffettiviAdOggi = itemResult.GiorniEffettiviAdOggi;
                // reperimento dei soli dipendenti che hanno uno scostamento negativo
                //model.Dipendenti = itemResult.Dipendenti.Where( w => w.Scostamento < 100 && matricoleMieiDipendenti.Contains( w.Matricola ) ).ToList( );

                model.Dipendenti = itemResult.Dipendenti.ToList( );
            }

            int totItems = model.Pianificato.Giorno.Count( );
            int next = ( page - 1 ) * model.ItemsInlist;

            List<int> pianificatiSum = new List<int>( );
            List<int> effettiviSum = new List<int>( );

            //for ( DateTime dt = model.Pianificato.Giorno.First( ) ; dt.Date <= model.Pianificato.Giorno.Last( ) ; dt = dt.AddDays( 1 ) )
            //{
            //    int index = model.Pianificato.Giorno.FindIndex( a => a.Equals( dt ) );
            //    pianificatiSum.Add( Somma( index , model.Pianificato.TotalePerGiorno ) );
            //    effettiviSum.Add( Somma( index , model.Effettivo.TotalePerGiorno ) );
            //}

            //model.Pianificato.TotalePerGiorno = pianificatiSum;
            //model.Effettivo.TotalePerGiorno = effettiviSum;

            //if ( page > 1 )
            //{
            //    model.Pianificato.Giorno = model.Pianificato.Giorno.Skip( ( page - 1 ) * model.ItemsInlist ).ToList( );
            //    model.Pianificato.TotalePerGiorno = model.Pianificato.TotalePerGiorno.Skip( ( page - 1 ) * model.ItemsInlist ).ToList( );

            //    model.Effettivo.Giorno = model.Effettivo.Giorno.Skip( ( page - 1 ) * model.ItemsInlist ).ToList( );
            //    model.Effettivo.TotalePerGiorno = model.Effettivo.TotalePerGiorno.Skip( ( page - 1 ) * model.ItemsInlist ).ToList( );
            //}

            //if ( ( totItems - next ) >= model.ItemsInlist )
            //{
            //    model.Pianificato.Giorno = model.Pianificato.Giorno.Take( model.ItemsInlist ).ToList( );
            //    model.Pianificato.TotalePerGiorno = model.Pianificato.TotalePerGiorno.Take( model.ItemsInlist ).ToList( );
            //    model.Effettivo.Giorno = model.Effettivo.Giorno.Take( model.ItemsInlist ).ToList( );
            //    model.Effettivo.TotalePerGiorno = model.Effettivo.TotalePerGiorno.Take( model.ItemsInlist ).ToList( );
            //}
            //else
            //{
            //    model.Pianificato.Giorno = model.Pianificato.Giorno.ToList( );
            //    model.Pianificato.TotalePerGiorno = model.Pianificato.TotalePerGiorno.ToList( );
            //    model.Effettivo.Giorno = model.Effettivo.Giorno.ToList( );
            //    model.Effettivo.TotalePerGiorno = model.Effettivo.TotalePerGiorno.ToList( );
            //}


            int mesi = 12;

            if ( model.Pianificato.Giorno.Last( ).Year > DateTime.Now.Year )
            {
                mesi = 15;
            }

            List<int> totaliMensiliPianificati = new List<int>( );
            List<int> totaliMensiliEffettivi = new List<int>( );
            int y = DateTime.Now.Year;
            for ( int idx = 1 ; idx <= mesi ; idx++ )
            {
                int mese = idx;
                if ( idx > 12 )
                {
                    mese = idx - 12;
                    y += 1;
                }

                DateTime begin = new DateTime( y , mese , 1 );
                DateTime end = begin.AddMonths( 1 );
                end = end.AddDays( -1 );

                totaliMensiliPianificati.Add( 0 );

                if ( begin.Month <= DateTime.Now.Month )
                {
                    totaliMensiliEffettivi.Add( 0 );
                }

                string etichetta = begin.ToString( "MMMM" );

                if ( DateTime.Now.Date < end && !model.EtichetteGrafico.Contains("oggi") )
                {
                    model.EtichetteGrafico.Add( "oggi" );
                }

                if ( y > DateTime.Now.Year )
                {
                    etichetta = begin.ToString( "MMMM" ) + " " + y.ToString( );
                }

                model.EtichetteGrafico.Add( etichetta );

                for ( DateTime date = begin ; date <= end ; date = date.AddDays( 1 ) )
                {
                    int index = model.Pianificato.Giorno.FindIndex( a => a.Equals( date ) );
                    totaliMensiliPianificati[idx - 1] += model.Pianificato.TotalePerGiorno[index];

                    if ( begin.Month <= DateTime.Now.Month )
                    {
                        index = model.Effettivo.Giorno.FindIndex( a => a.Equals( date ) );
                        totaliMensiliEffettivi[idx - 1] += model.Effettivo.TotalePerGiorno[index];
                    }
                }
            }

            model.PianificateGrafico = totaliMensiliPianificati;
            model.EffettiveGrafico = totaliMensiliEffettivi;

            for ( int idx = 0 ; idx < model.PianificateGrafico.Count ; idx++ )
            {
                if ( idx == 0 )
                {
                    pianificatiSum.Add( model.PianificateGrafico[idx] );
                }
                else
                {
                    int s = pianificatiSum[idx - 1];
                    s += model.PianificateGrafico[idx];
                    pianificatiSum.Add( s );
                }
            }

            for ( int idx = 0 ; idx < model.EffettiveGrafico.Count ; idx++ )
            {
                if ( idx == 0 )
                {
                    effettiviSum.Add( model.EffettiveGrafico[idx] );
                }
                else
                {
                    int s = effettiviSum[idx - 1];
                    s += model.EffettiveGrafico[idx];
                    effettiviSum.Add( s );
                }
            }

            model.PianificateGrafico = pianificatiSum;
            model.EffettiveGrafico = effettiviSum;

            if ( model.EtichetteGrafico.Contains( "oggi" ) )
            {
                int spacePosition = model.EtichetteGrafico.IndexOf( "oggi" );
                model.PianificateGrafico.Insert( spacePosition , model.GiorniPianificatiAdOggi );
            }

            model.CodiceSedeSelected = sede;
            model.Page = page;

            return View( "~/Views/DashboardResponsabile/_subpartial/ModalStatoFerie.cshtml" , model );
        }

        public ActionResult GetStatoFerieBoss ( string matricola )
        {
            DipendenteScostamento model = new DipendenteScostamento( );
            string sede = "";
            string reparto = "";

            var ut = BatchManager.GetUserData( matricola );

            if (ut != null)
            {
                sede = ut.sede_gapp;
                reparto = ut.CodiceReparto;

                if ( !( String.IsNullOrWhiteSpace( reparto ) || reparto.Trim( ) == "0" || reparto.Trim( ) == "00" ) )
                {
                    sede = sede + reparto;
                }
            }

            StatoFerieObj dati = GetModelFromCache( sede );

            var dip = dati.Dipendenti.Where( w => w.Matricola.Equals( matricola.Substring( 1 ) ) ).FirstOrDefault( );

            if ( dip != null )
            {
                model.GiorniEffettivi = dip.GiorniEffettivi;
                model.GiorniPianificati = dip.GiorniPianificati;
                model.GiorniPianificatiAdOggi = dip.GiorniPianificatiAdOggi;
                model.Scostamento = dip.Scostamento;
                model.Percentuale = dip.Percentuale;
            }

            return View( "~/Views/Home/subpartial/_statoFerie.cshtml" , model );
        }

        private PianoFerieApprovatoreModel GetPianoFerieModel ( String sede , int? anno )
        {
            PianoFerieApprovatoreModel model = null;

            WSDigigapp wsService = new WSDigigapp( )
            {
                Credentials = new System.Net.NetworkCredential(
                        CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[0] ,
                        CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[1] )
            };

            if ( anno == null )
                anno = DateTime.Now.Year;

            bool almenoUnUtenteRichiede2021 = false;

            var sedi = Utility.GetSediGappResponsabileList( );
            var ListSediReparti = getSediReparti( sedi , ( int ) anno );
            if ( sedi.Any( ) )
            {
                if ( String.IsNullOrWhiteSpace( sede ) )
                    sede = ListSediReparti[0].value;

                string repartoReq = null;

                if ( sede.Length > 5 )
                {
                    repartoReq = sede.Substring( 6 );
                }

                string sessionName = "getPianoFerieSedeGapp" + sede.Substring( 0 , 5 );

                pianoFerieSedeGapp resp = ( pianoFerieSedeGapp ) SessionManager.Get( sessionName );

                if ( resp == null )
                {
                    resp = wsService.getPianoFerieSedeGapp( "P" + Utente.Matricola( ) , "01" + "01" + anno , "" , "" , sede.Substring( 0 , 5 ) , 75 );
                    SessionManager.Set( sessionName , resp );
                }

                pianoFerieSedeGapp resp2 = null;

                var db = new myRaiData.digiGappEntities( );

                var AB = CommonManager.getAbilitazioni( );
                List<string> SediAb = AB.ListaAbilitazioni.Where( x => x.Sede.ToUpper( ).StartsWith( sede.Substring( 0 , 5 ).ToUpper( ) ) ).Select( x => x.Sede ).ToList( );

                Dipendente[] MieiDipendenti = null;
                Dipendente[] MieiDipendenti2 = null;

                if ( repartoReq == null )
                    MieiDipendenti = resp.dipendenti.Where( x => !SediAb.Contains( sede + x.reparto ) || String.IsNullOrWhiteSpace( x.reparto ) || x.reparto.Trim( ) == "0" || x.reparto.Trim( ) == "00" ).ToArray( );
                else
                    MieiDipendenti = resp.dipendenti.Where( x => x.reparto == repartoReq ).ToArray( );

                resp.dipendenti = MieiDipendenti;

                List<string> cessate = CommonManager.GetCessate( MieiDipendenti.Select( x => x.matricola.Substring( 1 ) ).ToArray( ) );

                if ( cessate.Any( ) )
                {
                    List<Dipendente> LD = new List<Dipendente>( );
                    foreach ( var d in resp.dipendenti )
                    {
                        if ( !cessate.Contains( d.matricola.Substring( 1 ) ) )
                            LD.Add( d );
                    }
                    resp.dipendenti = LD.ToArray( );
                }

                foreach ( var e in resp.dipendenti )
                {
                    almenoUnUtenteRichiede2021 = myRaiCommonTasks.CommonTasks.Richiede2021( e.matricola );

                    if ( almenoUnUtenteRichiede2021 )
                    {
                        break;
                    }
                }

                if ( almenoUnUtenteRichiede2021 )
                {
                    string sessionName2 = "getPianoFerieSedeGapp_2_" + sede.Substring( 0 , 5 );

                    resp2 = ( pianoFerieSedeGapp ) SessionManager.Get( sessionName2 );

                    if ( resp2 == null )
                    {
                        resp2 = wsService.getPianoFerieSedeGapp( "P" + Utente.Matricola( ) , "01" + "01" + ( anno + 1 ) , "" , "" , sede.Substring( 0 , 5 ) , 75 );
                        SessionManager.Set( sessionName2 , resp2 );
                    }

                    if ( repartoReq == null )
                        MieiDipendenti2 = resp2.dipendenti.Where( x => !SediAb.Contains( sede + x.reparto ) || String.IsNullOrWhiteSpace( x.reparto ) || x.reparto.Trim( ) == "0" || x.reparto.Trim( ) == "00" ).ToArray( );
                    else
                        MieiDipendenti2 = resp2.dipendenti.Where( x => x.reparto == repartoReq ).ToArray( );

                    resp2.dipendenti = MieiDipendenti2;

                    List<string> cessate2 = CommonManager.GetCessate( MieiDipendenti2.Select( x => x.matricola.Substring( 1 ) ).ToArray( ) );

                    if ( cessate2.Any( ) )
                    {
                        List<Dipendente> LD = new List<Dipendente>( );
                        foreach ( var d in resp2.dipendenti )
                        {
                            if ( !cessate2.Contains( d.matricola.Substring( 1 ) ) )
                                LD.Add( d );
                        }
                        resp2.dipendenti = LD.ToArray( );
                    }
                }

                model = new PianoFerieApprovatoreModel( )
                {
                    response = resp ,
                    Sedi = sedi ,
                    SediRepartiList = getSediReparti( sedi , ( int ) anno ) ,
                    SedeSelezionataCodice = sede ,
                    SedeSelezionataDesc = sedi.Where( x => x.CodiceSede == sede.Substring( 0 , 5 ) ).Select( x => x.DescrizioneSede ).FirstOrDefault( ) ,
                    //    SedeSelezionataApprovataData = db.MyRai_PianoFerieSedi.Where(x => x.sedegapp == sede.Replace("-", "")).OrderByDescending(x => x.data_approvato).Select(x => x.data_approvato).FirstOrDefault()
                };

                var pfsede = db.MyRai_PianoFerieSedi.Where( x => x.data_approvata != null && x.anno == anno &&
                 x.sedegapp == sede.Replace( "-" , "" ) ).OrderByDescending( x => x.numero_versione ).FirstOrDefault( );

                if ( pfsede == null )
                {
                    model.SedeSelezionataApprovataData = null;
                    model.SedeSelezionataFirmataData = null;
                }
                else
                {
                    model.SedeSelezionataApprovataData = pfsede.data_approvata;
                    model.SedeSelezionataFirmataData = pfsede.data_firma;
                }

                if ( model.SedeSelezionataCodice.Length > 5 )
                {
                    LinkedTableDataController L = new LinkedTableDataController( );
                    var dett = L.GetDettagliReparto( model.SedeSelezionataCodice.Substring( 0 , 5 ) , model.SedeSelezionataCodice.Substring( 5 ) );
                    if ( dett != null )
                        model.SedeSelezionataRepartoDescrittiva = dett.Descr_Reparto;
                    else
                        model.SedeSelezionataRepartoDescrittiva = "Reparto " + model.SedeSelezionataCodice.Substring( 5 );
                }

                DateTime D = DateTime.Now;

                int? IdTipologiaEsenteFerie = db.MyRai_InfoDipendente_Tipologia.Where( x => x.info == "Esente Piano Ferie" && x.data_inizio <= D && ( x.data_fine >= D || x.data_fine == null ) )
                                    .Select( x => x.id ).FirstOrDefault( );

                model.EsentatiPianoFerie = new List<string>( );

                if ( IdTipologiaEsenteFerie != null )
                {
                    string[] MieiDipMatricole = MieiDipendenti.Select( x => x.matricola.Substring( 1 ) ).ToArray( );
                    model.EsentatiPianoFerie = db.MyRai_InfoDipendente.Where( x =>
                                                              MieiDipMatricole
                                                             .Contains( x.matricola ) && x.id_infodipendente_tipologia == IdTipologiaEsenteFerie && D >= x.data_inizio && D <= x.data_fine
                                                              && x.valore != null && x.valore.ToLower( ) == "true" )
                                                            .Select( x => x.matricola ).ToList( );
                }

                foreach ( var dip in MieiDipendenti )
                {
                    string matDip = dip.matricola.Substring( 1 );
                    if ( cessate.Contains( matDip ) )
                        continue;

                    var pfmatr = db.MyRai_PianoFerie.Where( x => x.matricola == matDip && x.anno == anno
                     && x.data_consolidato != null
                    ).FirstOrDefault( );

                    List<DateTime> FE = new List<DateTime>( );
                    List<DateTime> RR = new List<DateTime>( );
                    List<DateTime> RN = new List<DateTime>( );
                    List<DateTime> PR = new List<DateTime>( );
                    List<DateTime> PF = new List<DateTime>( );
                    List<DateTime> PX = new List<DateTime>( );
                    List<DateTime> ConNote = new List<DateTime>( );

                    List<DateTime> Gapp_FE = new List<DateTime>( );
                    List<DateTime> Gapp_PF = new List<DateTime>( );
                    List<DateTime> Gapp_PR = new List<DateTime>( );

                    FE.AddRange( db.MyRai_PianoFerieGiorni.Where( x => x.data.Year == anno && x.matricola == matDip && x.eccezione == "FE" ).Select( x => x.data ).ToList( ) );
                    RR.AddRange( db.MyRai_PianoFerieGiorni.Where( x => x.data.Year == anno && x.matricola == matDip && x.eccezione == "RR" ).Select( x => x.data ).ToList( ) );
                    RN.AddRange( db.MyRai_PianoFerieGiorni.Where( x => x.data.Year == anno && x.matricola == matDip && x.eccezione == "RN" ).Select( x => x.data ).ToList( ) );
                    PR.AddRange( db.MyRai_PianoFerieGiorni.Where( x => x.data.Year == anno && x.matricola == matDip && x.eccezione == "PR" ).Select( x => x.data ).ToList( ) );
                    PF.AddRange( db.MyRai_PianoFerieGiorni.Where( x => x.data.Year == anno && x.matricola == matDip && x.eccezione == "PF" ).Select( x => x.data ).ToList( ) );
                    PX.AddRange( db.MyRai_PianoFerieGiorni.Where( x => x.data.Year == anno && x.matricola == matDip && x.eccezione == "PX" ).Select( x => x.data ).ToList( ) );
                    ConNote.AddRange( db.MyRai_PianoFerieGiorni.Where( x => x.data.Year == anno && x.matricola == matDip && x.nota_responsabile != null && x.nota_responsabile.Trim( ) != "" ).Select( x => x.data ).ToList( ) );

                    Gapp_FE.AddRange( dip.ferie.giornate.Where( x => x.tipoGiornata == "D" ).Select( x => x.data ).ToList( ) );
                    Gapp_PF.AddRange( dip.ferie.giornate.Where( x => x.tipoGiornata == "1" ).Select( x => x.data ).ToList( ) );
                    Gapp_PR.AddRange( dip.ferie.giornate.Where( x => x.tipoGiornata == "2" ).Select( x => x.data ).ToList( ) );

                    if ( almenoUnUtenteRichiede2021 )
                    {
                        FE.AddRange( db.MyRai_PianoFerieGiorni.Where( x => ( x.data.Year == anno + 1 && x.data.Month <= 3 ) && x.matricola == matDip && x.eccezione == "FE" ).Select( x => x.data ).ToList( ) );

                        RR.AddRange( db.MyRai_PianoFerieGiorni.Where( x => ( x.data.Year == anno + 1 && x.data.Month <= 3 ) && x.matricola == matDip && x.eccezione == "RR" ).Select( x => x.data ).ToList( ) );

                        RN.AddRange( db.MyRai_PianoFerieGiorni.Where( x => ( x.data.Year == anno + 1 && x.data.Month <= 3 ) && x.matricola == matDip && x.eccezione == "RN" ).Select( x => x.data ).ToList( ) );

                        PR.AddRange( db.MyRai_PianoFerieGiorni.Where( x => ( x.data.Year == anno + 1 && x.data.Month <= 3 ) && x.matricola == matDip && x.eccezione == "PR" ).Select( x => x.data ).ToList( ) );

                        PF.AddRange( db.MyRai_PianoFerieGiorni.Where( x => ( x.data.Year == anno + 1 && x.data.Month <= 3 ) && x.matricola == matDip && x.eccezione == "PF" ).Select( x => x.data ).ToList( ) );

                        PX.AddRange( db.MyRai_PianoFerieGiorni.Where( x => ( x.data.Year == anno + 1 && x.data.Month <= 3 ) && x.matricola == matDip && x.eccezione == "PX" ).Select( x => x.data ).ToList( ) );

                        ConNote.AddRange( db.MyRai_PianoFerieGiorni.Where( x => ( x.data.Year == anno + 1 && x.data.Month <= 3 ) && x.matricola == matDip && x.nota_responsabile != null && x.nota_responsabile.Trim( ) != "" ).Select( x => x.data ).ToList( ) );

                        var dip2 = MieiDipendenti2.Where( w => w.matricola.Equals( dip.matricola ) ).FirstOrDefault( );
                        if ( dip2 != null )
                        {
                            Gapp_FE.AddRange( dip2.ferie.giornate.Where( x => x.tipoGiornata == "D" ).Select( x => x.data ).ToList( ) );
                            Gapp_PF.AddRange( dip2.ferie.giornate.Where( x => x.tipoGiornata == "1" ).Select( x => x.data ).ToList( ) );
                            Gapp_PR.AddRange( dip2.ferie.giornate.Where( x => x.tipoGiornata == "2" ).Select( x => x.data ).ToList( ) );
                        }
                    }

                    if ( pfmatr != null )
                    {
                        model.PianoFerieDipendenti.Add( new GiorniPianoFeriePerDip( )
                        {
                            Matricola = matDip ,
                            GiorniFerie = FE ,
                            GiorniRR = RR ,
                            GiorniRN = RN ,

                            GiorniPR = PR ,
                            GiorniPF = PF ,
                            GiorniPX = PX ,

                            GiorniConNote = ConNote ,
                            NotaSuPianoFerieDaResp = pfmatr.nota_responsabile ,
                            Gapp_FE = Gapp_FE ,
                            Gapp_PF = Gapp_PF ,
                            Gapp_PR = Gapp_PR ,
                            DataApprovazione = pfmatr.data_approvato
                        } );
                    }
                    else
                    {
                        if ( model.SedeSelezionataFirmataData != null )
                        {
                            model.PianoFerieDipendenti.Add( new GiorniPianoFeriePerDip( )
                            {
                                Matricola = matDip ,
                                GiorniFerie = FE ,
                                GiorniRR = RR ,
                                GiorniRN = RN ,

                                GiorniPR = PR ,
                                GiorniPF = PF ,
                                GiorniPX = PX ,

                                GiorniConNote = ConNote ,
                                NotaSuPianoFerieDaResp = null ,
                                Gapp_FE = Gapp_FE ,
                                Gapp_PF = Gapp_PF ,
                                Gapp_PR = Gapp_PR ,
                                DataApprovazione = null ,
                                NessunInvioPianoFerie = true

                            } );
                        }
                        else
                        {
                            model.PianoFerieDipendenti.Add( new GiorniPianoFeriePerDip( )
                            {
                                Matricola = matDip ,
                                GiorniFerie = new List<DateTime>( ) ,
                                GiorniRR = new List<DateTime>( ) ,
                                GiorniRN = new List<DateTime>( ) ,
                                GiorniPF = new List<DateTime>( ) ,
                                GiorniPR = new List<DateTime>( ) ,
                                GiorniPX = new List<DateTime>( ) ,

                                GiorniConNote = new List<DateTime>( ) ,
                                Gapp_FE = Gapp_FE ,
                                Gapp_PF = Gapp_PF ,
                                Gapp_PR = Gapp_PR ,
                                NonInviato = true
                            } );
                        }
                    }
                }
            }
            model.AlmenoUnUtenteRichiede2021 = almenoUnUtenteRichiede2021;

            var db2 = new digiGappEntities( );
            model.MatricoleCheRichiedonoAnnoSuccessivo = db2.MyRai_ArretratiExcel2019.Where( w => w.estensione_marzo == true ).ToList( ).Select( w => w.matricola ).ToList( );
            return model;
        }

        public void AddSedeReparti ( string text , string value , List<SedeRepartiForSelect> L )
        {
            if ( !L.Any( x => x.text == text ) )
                L.Add( new SedeRepartiForSelect( ) { text = text , value = value } );
        }

        public List<SedeRepartiForSelect> getSediReparti ( List<Sede> sedi , int anno )
        {
            var db = new digiGappEntities( );
            var AB = CommonManager.getAbilitazioni( );

            List<SedeRepartiForSelect> L = new List<SedeRepartiForSelect>( );
            LinkedTableDataController lt = new LinkedTableDataController( );

            foreach ( var sede in sedi )
            {
                if ( sede.RepartiSpecifici.Any( x => x.reparto == "*" ) )
                {
                    var results = AB.ListaAbilitazioni.Where( x => x.Sede.StartsWith( sede.CodiceSede ) );
                    if ( results.Any( x => x.Sede.Length > 5 ) )
                    {
                        foreach ( var result in results.Where( a => a.Sede.Length > 5 ) )
                        {
                            string repar = result.Sede.Substring( 5 );
                            var r = lt.GetDettagliReparto( result.Sede.Substring( 0 , 5 ) , repar );
                            if ( r != null )
                                AddSedeReparti( sede.CodiceSede + " " + sede.DescrizioneSede + " - " + r.Descr_Reparto , sede.CodiceSede + "-" + repar , L );
                            else
                                AddSedeReparti( sede.CodiceSede + " " + sede.DescrizioneSede + " - Reparto " + repar , sede.CodiceSede + "-" + repar , L );
                        }
                    }
                    // else
                    AddSedeReparti( sede.CodiceSede + " " + sede.DescrizioneSede , sede.CodiceSede , L );
                }
                else
                {
                    foreach ( var reparto in sede.RepartiSpecifici )
                    {
                        var r = lt.GetDettagliReparto( sede.CodiceSede.Substring( 0 , 5 ) , reparto.reparto );
                        if ( r != null )
                            AddSedeReparti( sede.CodiceSede + " " + sede.DescrizioneSede + " -  " + r.Descr_Reparto , sede.CodiceSede + "-" + reparto.reparto , L );
                        else
                            AddSedeReparti( sede.CodiceSede + " " + sede.DescrizioneSede + " - Reparto " + reparto.reparto , sede.CodiceSede + "-" + reparto.reparto , L );
                    }
                }
            }
            return L.OrderBy( x => x.text ).ToList( );
        }

        #endregion


    }
}