using myRai.Business;
using myRai.Models;
using myRaiData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml.Serialization;

namespace myRaiHelper.Sirio
{
    public class Helper
    {
        public Helper()
        {

        }
        public static bool GetAttivitaSirio(string pMatricola, string matricola, DateTime dataInizio, DateTime dataFine, out Response.Envelope response)
        {

            //myRai.Models.Sirio.Response .Envelope resp;
            //myRai.Models.Sirio.Helper.GetAttivitaSirio("P028475", new DateTime(2018, 07, 01), new DateTime(2018, 07, 01), out resp);


            bool result = false;
            response = null;

            if (String.IsNullOrWhiteSpace(pMatricola))
                return false;

            string[] par = CommonHelper.GetParametri<string>(EnumParametriSistema.ServizioCeitonGetAttivita);

            var _url = par[0];
            var _action = par[1];

            try
            {
                Request.Envelope env = new Request.Envelope();
                env.Body = new Request.EnvelopeBody();
                env.Body.CTWS_REQ = new Request.EnvelopeBodyCTWS_REQ();
                env.Body.CTWS_REQ.CTWS_config_xmlc = "RAI_Activity_export_SOAP";
                env.Body.CTWS_REQ.userNumber = pMatricola;
                env.Body.CTWS_REQ.startDate = dataInizio.ToString("yyyy-MM-dd");
                env.Body.CTWS_REQ.endDate = dataFine.ToString("yyyy-MM-dd");

                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(_url);
                webRequest.Headers.Add("SOAPAction", _action);
                webRequest.ContentType = "text/xml;charset=\"utf-8\"";
                webRequest.Accept = "text/xml";
                webRequest.Method = "POST";
                //webRequest.UseDefaultCredentials = true;
                webRequest.Credentials = CommonHelper.GetUtenteServizioCredentials();

                using (Stream stream = webRequest.GetRequestStream())
                {
                    //soapEnvelopeXml.Save(stream);
                    XmlSerializer xx = new XmlSerializer(typeof(Request.Envelope));
                    xx.Serialize(stream, env);
                    stream.Close();
                }


                // begin async call to web request.
                IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);

                // suspend this thread until call is complete. You might want to
                // do something usefull here like update your UI.
                asyncResult.AsyncWaitHandle.WaitOne();

                // get the response from the completed web request.
                response = new Response.Envelope();
                using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult))
                {
                    using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
                    {
                        XmlSerializer x = new XmlSerializer(typeof(Response.Envelope));
                        response = (Response.Envelope)x.Deserialize(rd);
                    }
                }

                result = response != null && response.Body != null && response.Body.CTWS_RES != null && response.Body.CTWS_RES.ActivityExport != null;
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "Portale",
                    data = DateTime.Now,
                    matricola = matricola,
                    provenienza = "GetAttivitaSirio",
                    error_message = ex.ToString()
                });
            }
            return result;
        }

        /// <summary>
        /// Metodo per il reperimento delle attività Ceiton per l'utente PMatricola
        /// Se il giorno corrente è inferiore al 10 del mese allora verrà richiesto anche il mese precedente
        /// in quanto può essere che ancora siamo entro i giorni di chiusura in cui gli utenti possono
        /// richiedere eccezioni per il mese precedente.
        /// Nel caso in cui il giorno corrente fosse superiore al 10, allora verrà richiesto soltanto dal giorno 1 del mese corrente
        /// </summary>
        /// <param name="Pmatricola"></param>
        /// <param name="DataDiInteresse"></param>
        /// <returns></returns>
        public static WeekPlan GetWeekPlanCached(string Pmatricola, string matricola, DateTime? DataDiInteresse = null)
        {
            try
            {
                if ( HttpContext.Current.Session["AttivitaCeiton"] == null )
                {
                    DateTime Dstart = DateTime.Today;
                    if ( DateTime.Today.Day <= 10 )
                    {
                        Dstart = new DateTime( DateTime.Now.AddMonths( -1 ).Year , DateTime.Now.AddMonths( -1 ).Month , 1 );

                    }
                    else
                    {
                        Dstart = new DateTime( DateTime.Now.Year , DateTime.Now.Month , 1 );
                    }
                    SessionHelper.Set( "AttivitaCeiton" , GetWeekPlan( Pmatricola , Dstart , DateTime.Today ) );

                    //HttpContext.Current.Session["AttivitaCeiton"] = GetWeekPlan( Pmatricola , Dstart , DateTime.Today );
                }
                else
                {
                    try
                    {
                        // FRANCESCO
                        // se i dati in sessione sono presenti e la data di interesse è un dato valido
                        if ( DataDiInteresse.HasValue )
                        {
                            // prende i dati in sessione e verifica se per quel giorno in esame ci sono attività
                            // nel caso non ci fossero attività rifà il get per quella giornata e la carica in sessione
                            WeekPlan tempWP = ( WeekPlan ) SessionHelper.Get( "AttivitaCeiton" );
                            //WeekPlan tempWP = ( WeekPlan ) HttpContext.Current.Session["AttivitaCeiton"];

                            var exists = tempWP.Days.Where( w => w.Date.Equals( DataDiInteresse.GetValueOrDefault( ) ) ).FirstOrDefault( );

                            // se trova il record per quella data
                            if ( exists != null )
                            {
                                // verifica se ci sono attività
                                if ( exists.Activities == null || !exists.Activities.Any( ) )
                                {
                                    // fa il get per la giornata richiesta
                                    var outputData = GetWeekPlan( Pmatricola , DataDiInteresse.GetValueOrDefault( ) , DataDiInteresse.GetValueOrDefault( ) );

                                    if ( outputData != null )
                                    {
                                        // cerca i dati nella nuova risposta
                                        var newData = outputData.Days.Where( w => w.Date.Equals( DataDiInteresse.GetValueOrDefault( ) ) ).FirstOrDefault( );
                                        // se ci sono dati per la giornata richiesta allora sostituisce la giornata nell'oggetto in sessione
                                        // inserendo i dati aggiornati e con le attività valorizzate
                                        if ( newData != null && newData.Activities != null && newData.Activities.Any( ) )
                                        {
                                            if ( tempWP.Days.Remove( exists ) )
                                            {
                                                tempWP.Days.Add( newData );
                                            }
                                            SessionHelper.Set( "AttivitaCeiton" , tempWP );
                                            //HttpContext.Current.Session["AttivitaCeiton"] = tempWP;
                                        }
                                    }
                                }
                                else if ( exists.Activities != null &&
                                            exists.Activities.Any( ) &&
                                            exists.Activities.Count( w => w.idAttivita.Equals( "000000" ) ) > 0 )
                                {
                                    var outputData = GetWeekPlan( Pmatricola , DataDiInteresse.GetValueOrDefault( ) , DataDiInteresse.GetValueOrDefault( ) );

                                    if ( outputData != null )
                                    {
                                        // cerca i dati nella nuova risposta
                                        var newData = outputData.Days.Where( w => w.Date.Equals( DataDiInteresse.GetValueOrDefault( ) ) ).FirstOrDefault( );
                                        // se ci sono dati per la giornata richiesta allora sostituisce la giornata nell'oggetto in sessione
                                        // inserendo i dati aggiornati e con le attività valorizzate
                                        if ( newData != null && newData.Activities != null && newData.Activities.Any( ) )
                                        {
                                            if ( tempWP.Days.Remove( exists ) )
                                            {
                                                tempWP.Days.Add( newData );
                                            }
                                            SessionHelper.Set( "AttivitaCeiton" , tempWP );
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch ( Exception ex )
                    {
                        Logger.LogErrori( new myRaiData.MyRai_LogErrori( )
                        {
                            applicativo = "PORTALE" ,
                            data = DateTime.Now ,
                            error_message = ex.ToString( ) ,
                            matricola = CommonHelper.GetCurrentUserMatricola( ) ,
                            provenienza = "GetWeekPlanCached"
                        } );
                    }
                }

                //WeekPlan wp = ( WeekPlan ) HttpContext.Current.Session["AttivitaCeiton"];
                WeekPlan wp = ( WeekPlan ) SessionHelper.Get( "AttivitaCeiton" );
                if ( wp == null )
                    return null;
                if ( DataDiInteresse == null )
                    return wp;
                else
                {
                    List<DayPlan> LD = new List<DayPlan>( );
                    foreach ( var item in wp.Days )
                    {
                        if ( item.Date == DataDiInteresse )
                        {
                            LD.Add( item );
                            var sediSimulazioneAttivitaCeiton = CommonHelper.GetParametro<string>( EnumParametriSistema.SediSimulazioneAttivitaCeiton );

                            List<string> sedi = new List<string>( );
                            if ( !String.IsNullOrEmpty( sediSimulazioneAttivitaCeiton ) )
                            {
                                sedi.AddRange( sediSimulazioneAttivitaCeiton.Split( ',' ).ToList( ) );
                            }

                            //FRANCESCO
                            // se la sede è una di quelle simulata ceiton cioè che se non hanno attività vanno aggiunte a mano
                            // se per quella giornata non ci sono attività ne aggiunge una fake
                            if ( sedi.Contains( UtenteHelper.SedeGapp( ) ) && UtenteHelper.GestitoSirio( ) && !item.Activities.Any( ) )
                            {
                                string subMatricola = Pmatricola.Replace( "P" , "" );
                                digiGappEntities db = new digiGappEntities( );
                                item.Activities = new List<DayActivity>( );

                                item.Activities.Add( new DayActivity( )
                                {
                                    Title = "" ,
                                    Location = "" ,
                                    idAttivita = "000000" ,
                                    Date = item.Date ,
                                    Schedule = " - " ,
                                    MainActivity = "" ,
                                    DoneActivity = "" ,
                                    Manager = "" ,
                                    Uorg = "" ,
                                    Note = "" ,
                                    Matricola = "" ,
                                    Eccezioni = db.MyRai_AttivitaCeiton.Where( x => x.idCeiton == "000000" && x.MyRai_Richieste.Count( y => y.matricola_richiesta == subMatricola ) > 0 ).Select( z => z.MyRai_Richieste ).Count( )
                                } );
                            }
                        }
                    }

                    return new WeekPlan( ) { Days = LD };
                }
            }
            catch ( Exception ex )
            {
                Logger.LogErrori( new myRaiData.MyRai_LogErrori( )
                {
                    applicativo = "PORTALE" ,
                    data = DateTime.Now ,
                    error_message = ex.ToString( ) ,
                    matricola = CommonHelper.GetCurrentUserMatricola( ) ,
                    provenienza = "GetWeekPlanCached"
                } );
                return null;
            }
        }

        public static DateTime GetLimitDate()
        {
            DateTime limitDate = DateTime.Now;
            //if (startDate.DayOfWeek >= DayOfWeek.Friday)
            if (limitDate.DayOfWeek > DayOfWeek.Friday
                || (limitDate.DayOfWeek == DayOfWeek.Friday && limitDate.TimeOfDay >= new TimeSpan(19, 30, 0)))
                limitDate = limitDate.AddDays(6);
            else
                limitDate = limitDate.AddDays(7 - (int)DateTime.Today.DayOfWeek);

            return limitDate;
        }

        public static WeekPlan GetWeekPlan(string pMatricola, DateTime dataDa, DateTime dataA)
        {
            if ( String.IsNullOrWhiteSpace( pMatricola ) )
                pMatricola = CommonHelper.GetCurrentUserPMatricola( );

            digiGappEntities db = new digiGappEntities( );

            var sediSimulazioneAttivitaCeiton = CommonHelper.GetParametro<string>( EnumParametriSistema.SediSimulazioneAttivitaCeiton );

            List<string> sedi = new List<string>( );
            if ( !String.IsNullOrEmpty( sediSimulazioneAttivitaCeiton ) )
            {
                sedi.AddRange( sediSimulazioneAttivitaCeiton.Split( ',' ).ToList( ) );
            }

            string matricola = pMatricola.Substring(1);

            WeekPlan weekPlan = new WeekPlan();

            DateTime startDate = dataDa;
            DateTime endDate = dataA;

            DateTime limitDate = GetLimitDate();

            if (endDate > limitDate)
                endDate = limitDate;

            weekPlan = new WeekPlan();
            int numDays = (endDate - startDate).Days + 1;

            for (int i = 0; i < numDays; i++)
                weekPlan.Days.Add(new DayPlan() { Date = startDate.AddDays(i) });

            Response.Envelope envRes = new Response.Envelope();

            if (GetAttivitaSirio(pMatricola, matricola, startDate, endDate, out envRes))
            {
                foreach (Response.CTWS_RESActivityExport item in envRes.Body.CTWS_RES.ActivityExport)
                {
                    string actDateStr = item.dataAttivita;
                    DateTime actDate;
                    DateTime.TryParseExact(actDateStr, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out actDate);
                    DayPlan dayPlan = weekPlan.Days.FirstOrDefault(x => x.Date == actDate);
                    DayActivity dayAct = new DayActivity();
                    dayAct.Title = item.titolo;
                    dayAct.Location = item.risorseTecniche != null && item.risorseTecniche.Count() > 0 ? item.risorseTecniche[0].descrizione : "";
                    dayAct.SetStartTime(item.oraInizioAttivita);
                    dayAct.SetEndTime(item.oraFineAttivita);
                    dayAct.Schedule = item.oraInizioAttivita + " - " + item.oraFineAttivita;
                    dayAct.MainActivity = item.attivitaPrimaria;
                    dayAct.DoneActivity = item.attivitaSvolta;
                    dayAct.Manager = item.matricolaResponsabile;
                    dayAct.idAttivita = item.idAttivita;
                    dayAct.Uorg = item.uorg;
                    dayAct.Note = item.note;
                    dayAct.Matricola = item.matricola;
                    dayAct.Date = actDate;
                    dayAct.Eccezioni = db.MyRai_AttivitaCeiton.Where(x => x.idCeiton == item.idAttivita && x.MyRai_Richieste.Count(y=>y.matricola_richiesta==matricola)>0).Select(z=>z.MyRai_Richieste).Count();
                    dayPlan.Activities.Add(dayAct);
                }

                //FRANCESCO
                if ( sedi.Contains( Utente.SedeGapp( ) ) && Utente.GestitoSirio( ) )
                {
                    for ( int giorno = 0 ; giorno < numDays ; giorno++ )
                    {
                        DateTime actDate = new DateTime( dataDa.Year , dataDa.Month , dataDa.Day , 0 , 0 , 0 );
                        actDate = actDate.AddDays( giorno );

                        DayPlan dayPlan = weekPlan.Days.FirstOrDefault( x => x.Date == actDate );
                        if ( !dayPlan.Activities.Any( ) )
                        {
                            DayActivity dayAct = new DayActivity( );

                            dayPlan.Activities.Add( new DayActivity( )
                            {
                                Title = "" ,
                                Location = "" ,
                                idAttivita = "000000" ,
                                Date = actDate ,
                                Schedule = " - " ,
                                MainActivity = "" ,
                                DoneActivity = "" ,
                                Manager = "" ,
                                Uorg = "" ,
                                Note = "" ,
                                Matricola = "" ,
                                Eccezioni = db.MyRai_AttivitaCeiton.Where( x => x.idCeiton == "000000" && x.MyRai_Richieste.Count( y => y.matricola_richiesta == matricola ) > 0 ).Select( z => z.MyRai_Richieste ).Count( )
                            } );
                        }
                    }
                }
            }
            else
            {
                //FRANCESCO
                if ( sedi.Contains( Utente.SedeGapp( ) ) && Utente.GestitoSirio( ) )
                {
                    for ( int giorno = 0 ; giorno < numDays ; giorno++ )
                    {
                        DateTime actDate = new DateTime( dataDa.Year , dataDa.Month , dataDa.Day , 0 , 0 , 0 );
                        actDate = actDate.AddDays( giorno );

                        DayPlan dayPlan = weekPlan.Days.FirstOrDefault( x => x.Date == actDate );
                        DayActivity dayAct = new DayActivity( );

                        dayPlan.Activities.Add( new DayActivity( )
                        {
                            Title = "" ,
                            Location = "" ,
                            idAttivita = "000000" ,
                            Date = actDate ,
                            Schedule = " - " ,
                            MainActivity = "" ,
                            DoneActivity = "" ,
                            Manager = "" ,
                            Uorg = "" ,
                            Note = "" ,
                            Matricola = "" ,
                            Eccezioni = db.MyRai_AttivitaCeiton.Where( x => x.idCeiton == "000000" && x.MyRai_Richieste.Count( y => y.matricola_richiesta == matricola ) > 0 ).Select( z => z.MyRai_Richieste ).Count( )
                        } );
                    }
                }
            }

            return weekPlan;
        }
    }
}