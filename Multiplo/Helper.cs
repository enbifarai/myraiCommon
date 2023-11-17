using Multiplo.Models;
using myRaiData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Xml.Serialization;

namespace Multiplo
{
    public class Helper
    {

        public static T[] GetParametri<T> ( EnumParametriSistema chiave )
        {
            var db = new digiGappEntities( );
            String NomeParametro = chiave.ToString( );

            MyRai_ParametriSistema p = db.MyRai_ParametriSistema.FirstOrDefault( x => x.Chiave == NomeParametro );
            if ( p == null )
                return null;
            else
            {
                T[] parametri = new T[] { ( T ) Convert.ChangeType( p.Valore1 , typeof( T ) ) , ( T ) Convert.ChangeType( p.Valore2 , typeof( T ) ) };
                return parametri;
            }
        }

        public Helper ( )
        {

        }

        public static bool GetAttivitaSirio ( string pMatricola , DateTime dataInizio , DateTime dataFine , out Multiplo.Models.Sirio.Response.Envelope response )
        {

            //myRai.Models.Sirio.Response .Envelope resp;
            //myRai.Models.Sirio.Helper.GetAttivitaSirio("P028475", new DateTime(2018, 07, 01), new DateTime(2018, 07, 01), out resp);


            bool result = false;
            response = null;

            if ( String.IsNullOrWhiteSpace( pMatricola ) )
                return false;

            string[] par = GetParametri<string>( EnumParametriSistema.ServizioCeitonGetAttivita );

            var _url = par[0];
            var _action = par[1];

            try
            {
                Models.Sirio.Request.Envelope env = new Models.Sirio.Request.Envelope( );
                env.Body = new Models.Sirio.Request.EnvelopeBody( );
                env.Body.CTWS_REQ = new Models.Sirio.Request.EnvelopeBodyCTWS_REQ( );
                env.Body.CTWS_REQ.CTWS_config_xmlc = "RAI_Activity_export_SOAP";
                env.Body.CTWS_REQ.userNumber = pMatricola;
                env.Body.CTWS_REQ.startDate = dataInizio.ToString( "yyyy-MM-dd" );
                env.Body.CTWS_REQ.endDate = dataFine.ToString( "yyyy-MM-dd" );

                HttpWebRequest webRequest = ( HttpWebRequest ) WebRequest.Create( _url );
                webRequest.Headers.Add( "SOAPAction" , _action );
                webRequest.ContentType = "text/xml;charset=\"utf-8\"";
                webRequest.Accept = "text/xml";
                webRequest.Method = "POST";
                //webRequest.UseDefaultCredentials = true;
                string[] AccountUtenteServizio = GetParametri<string>( EnumParametriSistema.AccountUtenteServizio );
                webRequest.Credentials = new System.Net.NetworkCredential( AccountUtenteServizio[0] , AccountUtenteServizio[1] , "RAI" );

                using ( Stream stream = webRequest.GetRequestStream( ) )
                {
                    //soapEnvelopeXml.Save(stream);
                    XmlSerializer xx = new XmlSerializer( typeof( Models.Sirio.Request.Envelope ) );
                    xx.Serialize( stream , env );
                }

                // begin async call to web request.
                IAsyncResult asyncResult = webRequest.BeginGetResponse( null , null );

                // suspend this thread until call is complete. You might want to
                // do something usefull here like update your UI.
                asyncResult.AsyncWaitHandle.WaitOne( );

                // get the response from the completed web request.
                response = new Models.Sirio.Response.Envelope( );
                using ( WebResponse webResponse = webRequest.EndGetResponse( asyncResult ) )
                {
                    using ( StreamReader rd = new StreamReader( webResponse.GetResponseStream( ) ) )
                    {
                        XmlSerializer x = new XmlSerializer( typeof( Models.Sirio.Response.Envelope ) );
                        response = ( Models.Sirio.Response.Envelope ) x.Deserialize( rd );
                    }
                }

                result = response != null && response.Body != null && response.Body.CTWS_RES != null && response.Body.CTWS_RES.ActivityExport != null;
            }
            catch ( Exception ex )
            {

            }
            return result;
        }

        public static DateTime GetLimitDate ( )
        {
            DateTime limitDate = DateTime.Now;
            //if (startDate.DayOfWeek >= DayOfWeek.Friday)
            if ( limitDate.DayOfWeek > DayOfWeek.Friday
                || ( limitDate.DayOfWeek == DayOfWeek.Friday && limitDate.TimeOfDay >= new TimeSpan( 19 , 30 , 0 ) ) )
                limitDate = limitDate.AddDays( 6 );
            else
                limitDate = limitDate.AddDays( 7 - ( int ) DateTime.Today.DayOfWeek );

            return limitDate;
        }

        public static int GetWeekPlan ( string pMatricola , DateTime dataDa , DateTime dataA )
        {
            int inseriti = 0;
            string matricola = pMatricola.Substring( 1 );
            string messaggio = "";

            WeekPlan weekPlan = new WeekPlan( );

            DateTime startDate = dataDa;
            DateTime endDate = dataA;

            weekPlan = new WeekPlan( );
            int numDays = ( endDate - startDate ).Days + 1;

            for ( int i = 0 ; i < numDays ; i++ )
                weekPlan.Days.Add( new DayPlan( ) { Date = startDate.AddDays( i ) } );

            Models.Sirio.Response.Envelope envRes = new Models.Sirio.Response.Envelope( );

            if ( GetAttivitaSirio( pMatricola , startDate , endDate , out envRes ) )
            {
                digiGappEntities db = new digiGappEntities( );

                messaggio = String.Format( "Trovate {0} attività" , envRes.Body.CTWS_RES.ActivityExport.Count( ) );
                Console.WriteLine( messaggio );
                ScriviFile( messaggio );

                foreach ( Models.Sirio.Response.CTWS_RESActivityExport item in envRes.Body.CTWS_RES.ActivityExport )
                {
                    string actDateStr = item.dataAttivita;
                    DateTime actDate;
                    DateTime.TryParseExact( actDateStr , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out actDate );

                    bool exist = db.MyRai_AttivitaApprovatori.Count( w => w.Matricola.Equals( matricola ) && w.Titolo.Equals( item.titolo ) && w.Data.Equals( actDate ) ) > 0;

                    if ( !exist )
                    {
                        db.MyRai_AttivitaApprovatori.Add( new MyRai_AttivitaApprovatori( )
                        {
                            Matricola = matricola ,
                            Titolo = item.titolo ,
                            Data = actDate
                        } );

                        db.SaveChanges( );
                        messaggio = String.Format( "Attività {0} aggiunta" , item.titolo );
                        Console.WriteLine( messaggio );
                        ScriviFile( messaggio );
                        inseriti++;
                    }
                    else
                    {
                        messaggio = String.Format( "Attività {0} ià presente per la data {1}" , item.titolo , actDate.ToString( "dd/MM/yyyy" ) );
                        Console.WriteLine( messaggio );
                        ScriviFile( messaggio );
                    }
                }
            }
            else
            {
                messaggio = String.Format( "Nessuna attività trovata" );
                Console.WriteLine( messaggio );
                ScriviFile( messaggio );
            }

            return inseriti;
        }


        public static int GetWeekPlanPerMatricola ( string pMatricola , DateTime dataDa , DateTime dataA )
        {
            int inseriti = 0;
            string matricola = pMatricola.Substring( 1 );
            string messaggio = "";

            WeekPlan weekPlan = new WeekPlan( );

            DateTime startDate = dataDa;
            DateTime endDate = dataA;

            weekPlan = new WeekPlan( );
            int numDays = ( endDate - startDate ).Days + 1;

            for ( int i = 0 ; i < numDays ; i++ )
                weekPlan.Days.Add( new DayPlan( ) { Date = startDate.AddDays( i ) } );

            Models.Sirio.Response.Envelope envRes = new Models.Sirio.Response.Envelope( );

            if ( GetAttivitaSirio( pMatricola , startDate , endDate , out envRes ) )
            {
                messaggio = String.Format( "Trovate {0} attività" , envRes.Body.CTWS_RES.ActivityExport.Count( ) );
                Console.WriteLine( messaggio );
                ScriviFile( messaggio );

                foreach ( Models.Sirio.Response.CTWS_RESActivityExport item in envRes.Body.CTWS_RES.ActivityExport )
                {
                    string actDateStr = item.dataAttivita;
                    DateTime actDate;
                    DateTime.TryParseExact( actDateStr , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out actDate );

                    messaggio = String.Format( "{0};{1};{2};{3};{4};{5}" , actDate.ToString( "dd/MM/yyyy" ) , item.titolo , item.attivitaPrimaria, item.attivitaSvolta , item.oraInizioAttivita , item.oraFineAttivita );
                    ScriviFile( messaggio , "Attivita_" + pMatricola );
                }
            }
            else
            {
                messaggio = String.Format( "Nessuna attività trovata" );
                Console.WriteLine( messaggio );
                ScriviFile( messaggio );
            }

            return inseriti;
        }

        private static void ScriviFile ( string msg , string nomeFile = "" )
        {
            if ( !String.IsNullOrEmpty( msg ) )
            {
                string filelog1 = "";
                var location = System.Reflection.Assembly.GetEntryAssembly( ).Location;

                var directoryPath = Path.GetDirectoryName( location );
                var logPath = Path.Combine( directoryPath , nomeFile );
                if ( !Directory.Exists( logPath ) )
                    Directory.CreateDirectory( logPath );

                filelog1 = Path.Combine( logPath , "ConsoleLog_" + DateTime.Now.ToString( "yyyyMMdd" ) + ".txt" );

                File.AppendAllText( filelog1 , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) + " " + msg + "\r\n" );
            }
        }
    }
}