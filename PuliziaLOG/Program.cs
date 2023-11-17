using Newtonsoft.Json;
using PuliziaLOG.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace PuliziaLOG
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime current = DateTime.Now;
            DateTime currentMin = DateTime.Now;
            DateTime currentMax = DateTime.Now;

            if ( args.Length == 1 && args[0].ToUpper( ) == "LOG" )
            {
                currentMin = new DateTime( current.Year , current.Month , current.Day , 00 , 01 , 00 );
                currentMax = new DateTime( current.Year , current.Month , current.Day , 04 , 00 , 00 );
                PuliziaTabellaLog( currentMin, currentMax );
            }

            if ( args.Length == 1 && args[0].ToUpper( ) == "API" )
            {
                currentMin = new DateTime( current.Year , current.Month , current.Day , 03 , 01 , 00 );
                currentMax = new DateTime( current.Year , current.Month , current.Day , 07 , 00 , 00 );
                PuliziaLogAPI( currentMin , currentMax );
            }

            if ( args.Length == 1 && args[0].ToUpper( ) == "AZIONI" )
            {
                currentMin = new DateTime( current.Year , current.Month , current.Day , 02 , 01 , 00 );
                currentMax = new DateTime( current.Year , current.Month , current.Day , 06 , 00 , 00 );
                PuliziaLogAzioni( currentMin , currentMax );
            }

            if ( args.Length == 1 && args[0].ToUpper( ) == "LOGDB" )
            {
                currentMin = new DateTime( current.Year , current.Month , current.Day , 21 , 25 , 00 );
                currentMax = new DateTime( current.Year , current.Month , current.Day , 23 , 30 , 00 );
                PuliziaLogDB( currentMin , currentMax );
            }

            if ( args.Length == 1 && args[0].ToUpper( ) == "ERRORI" )
            {
                currentMin = new DateTime( current.Year , current.Month , current.Day , 01 , 30 , 00 );
                currentMax = new DateTime( current.Year , current.Month , current.Day , 05 , 30 , 00 );
                PuliziaLogErrori( currentMin , currentMax );
            }

            if ( args.Length == 1 && args[0].ToUpper( ) == "MENSA" )
            {
                currentMin = new DateTime( current.Year , current.Month , current.Day , 01 , 01 , 00 );
                currentMax = new DateTime( current.Year , current.Month , current.Day , 03 , 00 , 00 );
                PuliziaLogMensa( currentMin , currentMax );
            }

        }

        #region metodi comuni
        private static bool IsOrarioChiusura ( DateTime currentMin , DateTime currentMax )
        {
            bool result = false;

            DateTime current = DateTime.Now;

            if ( current < currentMin ||
                current > currentMax )
            {
                result = true;
            }

            return result;
        }

        private static Attachment ScriviLogSlack ( string testo , string color = "good" , bool istantaneo = false )
        {
            string SlackMainTitle = "Batch PuliziaLOG";

            string tx = String.Format( "{0} - {1}\r\n" , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) , testo );

            Attachment slackA = new Attachment( ) { Color = color , Title = tx };
            if ( istantaneo )
            {
                List<Attachment> SlackAttachments = new List<Attachment>( );
                SlackAttachments.Add( slackA );

                PostMessageAdvanced( SlackMainTitle , SlackAttachments.ToArray( ) );
                SlackAttachments.Clear( );
            }

            return slackA;
        }

        private static void ScriviLogFile ( string testo )
        {
            string tx = String.Format( "{0} - {1}\r\n" , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) , testo );
            Log( tx );
        }

        private static Attachment ScriviLog ( string testo , string color = "good" , bool istantaneo = false )
        {
            Attachment slackA = new Attachment( );

            ScriviLogFile( testo );
            slackA = ScriviLogSlack( testo , color , istantaneo );

            return slackA;
        }

        public static void PostMessageAdvanced ( string text , Attachment[] attachments , string username = "RAIPERME" , string channel = "#general" )
        {
            PayloadAdvanced payload = new PayloadAdvanced( )
            {
                Channel = channel ,
                Username = username ,
                Text = text ,
                Attachments = attachments
            };

            PostMessageAdvanced( payload );
        }

        public static void PostMessageAdvanced ( PayloadAdvanced payload )
        {
            string payloadJson = JsonConvert.SerializeObject( payload );

            using ( WebClient client = new WebClient( ) )
            {
                NameValueCollection data = new NameValueCollection( );
                data["payload"] = payloadJson;

                var response = client.UploadValues( "https://hooks.slack.com/services/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" ,
                "POST" , data );

                //The response text is usually "ok"
                string responseText = new UTF8Encoding( ).GetString( response );
            }
        }

        public static void Log ( string message , string methodName = null )
        {
            try
            {
                string filelog = "";
                var location = System.Reflection.Assembly.GetEntryAssembly( ).Location;

                var directoryPath = Path.GetDirectoryName( location );
                var logPath = System.IO.Path.Combine( directoryPath , "log" );
                if ( !System.IO.Directory.Exists( logPath ) )
                    System.IO.Directory.CreateDirectory( logPath );

                if ( !string.IsNullOrEmpty( methodName ) )
                {
                    filelog = System.IO.Path.Combine( logPath , "log_" + methodName + "_" + DateTime.Now.ToString( "yyyyMMdd" ) + ".txt" );
                }
                else
                {
                    filelog = System.IO.Path.Combine( logPath , "log_" + DateTime.Now.ToString( "yyyyMMdd" ) + ".txt" );
                }

                System.IO.File.AppendAllText( filelog , DateTime.Now.ToString( "HH:mm " ) + message + "\r\n" );
                Console.WriteLine( message );
            }
            catch ( Exception ex )
            {
                Console.WriteLine( ex.ToString( ) );
            }
        }

        #endregion

        private static void PuliziaTabellaLog( DateTime currentMin , DateTime currentMax )
        {
            string msg = "";
            int conteggio = 0;
            int totale = 0;
            MyRai_ParametriSistema createBackupFile = new MyRai_ParametriSistema();
            DateTime start = DateTime.Now;
            int maxRecords = 200000;

            if ( !IsOrarioChiusura( currentMin , currentMax ) )
            {
                try
                {
                    var connection = ConfigurationManager.ConnectionStrings["digiGappEntities"].ConnectionString;

                    if (connection != null && connection.Contains("ZTOLS420"))
                    {
                        ScriviLog("Avvio batch pulizia tabella LOG (PRODUZIONE)", "good", true);
                    }
                    else
                    {
                        ScriviLog("Avvio batch pulizia tabella LOG (SVILUPPO)", "good", true);
                    }

                    string query = "";
                    DateTime oggi = DateTime.Now;
                    oggi = oggi.AddDays( -15 );
                    DateTime dataLimite = new DateTime( oggi.Year , oggi.Month , oggi.Day , 23 , 59 , 59 );

                    using ( digiGappEntities db = new digiGappEntities( ) )
                    {
                        do
                        {
                            query = "SELECT COUNT (A.ID) FROM (SELECT TOP ##MAXRECORDS## (id) FROM [digiGapp].[dbo].[LOG] WHERE Timestamp <= '##DATALIMITE##') A";
                            query = query.Replace( "##MAXRECORDS##" , maxRecords.ToString( ) );
                            query = query.Replace( "##DATALIMITE##" , dataLimite.ToString( "yyyy-MM-dd HH:mm:ss.000" ) );
                            conteggio = db.Database.SqlQuery<int>( query ).FirstOrDefault( );

                            if ( conteggio == 0 )
                            {
                                msg = String.Format( "Non ci sono records da eliminare" );
                                ScriviLogFile( msg );
                                ScriviLog( msg , "good" , true );
                                break;
                            }

                            query = "DELETE FROM [digiGapp].[dbo].[LOG] WHERE ID in ( SELECT top ##MAXRECORDS## ID FROM LOG WHERE Timestamp <= '##DATALIMITE##' order by Timestamp ) ";
                            query = query.Replace( "##MAXRECORDS##" , maxRecords.ToString( ) );
                            query = query.Replace( "##DATALIMITE##" , dataLimite.ToString( "yyyy-MM-dd HH:mm:ss.000" ) );
                            db.Database.ExecuteSqlCommand( query );

                            totale += conteggio;

                        } while ( !IsOrarioChiusura( currentMin , currentMax ) );

                        if (IsOrarioChiusura( currentMin , currentMax ) )
                        {
                            msg = String.Format( "Orario di chiusura raggiunto" );
                            ScriviLog( msg , "good" , true );
                        }
                    }
                }
                catch (Exception ex)
                {
                    ScriviLog("Si è verificato un errore: " + ex.Message, "danger", true);
                }

                msg = String.Format( "Sono stati rimossi {0} records totali " , totale );
                ScriviLog( msg , "good" , true );
            }
            else
            {
                msg = String.Format( "Orario di chiusura raggiunto" );
                ScriviLog( msg , "good" , true );
            }

            msg = String.Format("Termine batch pulizia tabella LOG");
            ScriviLog(msg, "good", true);

            DateTime end = DateTime.Now;

            TimeSpan t = end.Subtract( start );
            msg = String.Format( "Tempo di esecuzione {0} giorni, {1} ore, {2} minuti, {3} secondi" , t.Days , t.Hours , t.Minutes , t.Seconds );
            ScriviLog( msg , "good" , true );
        }

        private static void PuliziaLogAPI ( DateTime currentMin , DateTime currentMax )
        {
            string msg = "";
            int conteggio = 0;
            int totale = 0;

            DateTime start = DateTime.Now;
            int maxRecords = 500000;

            if ( !IsOrarioChiusura( currentMin , currentMax ) )
            {
                try
                {
                    var connection = ConfigurationManager.ConnectionStrings["digiGappEntities"].ConnectionString;

                    if ( connection != null && connection.Contains( "ZTOLS420" ) )
                    {
                        ScriviLog( "Avvio batch pulizia tabella MyRai_LogAPI (PRODUZIONE)" , "good" , true );
                    }
                    else
                    {
                        ScriviLog( "Avvio batch pulizia tabella MyRai_LogAPI (SVILUPPO)" , "good" , true );
                    }

                    string query = "";
                    DateTime oggi = DateTime.Now;
                    oggi = oggi.AddMonths( -2 );
                    DateTime dataLimite = new DateTime( oggi.Year , oggi.Month , oggi.Day , 23 , 59 , 59 );

                    using ( digiGappEntities db = new digiGappEntities( ) )
                    {
                        do
                        {
                            query = "SELECT COUNT (A.ID) FROM (SELECT TOP ##MAXRECORDS## (id) FROM [digiGapp].[dbo].[MyRai_LogAPI] WHERE [Data] <= '##DATALIMITE##') A";
                            query = query.Replace( "##MAXRECORDS##" , maxRecords.ToString( ) );
                            query = query.Replace( "##DATALIMITE##" , dataLimite.ToString( "yyyy-MM-dd HH:mm:ss.000" ) );
                            conteggio = db.Database.SqlQuery<int>( query ).FirstOrDefault( );

                            if ( conteggio == 0 )
                            {
                                msg = String.Format( "Non ci sono records da eliminare" );
                                ScriviLogFile( msg );
                                ScriviLog( msg , "good" , true );
                                break;
                            }

                            query = "DELETE FROM [digiGapp].[dbo].[MyRai_LogAPI] WHERE ID in ( SELECT TOP ##MAXRECORDS## (id) FROM [digiGapp].[dbo].[MyRai_LogAPI] WHERE [Data] <= '##DATALIMITE##' order by [Data] )";
                            query = query.Replace( "##MAXRECORDS##" , maxRecords.ToString( ) );
                            query = query.Replace( "##DATALIMITE##" , dataLimite.ToString( "yyyy-MM-dd HH:mm:ss.000" ) );
                            db.Database.ExecuteSqlCommand( query );

                            totale += conteggio;

                        } while ( !IsOrarioChiusura( currentMin , currentMax ) );

                        if ( IsOrarioChiusura( currentMin , currentMax ) )
                        {
                            msg = String.Format( "Orario di chiusura raggiunto" );
                            ScriviLog( msg , "good" , true );
                        }
                    }
                }
                catch ( Exception ex )
                {
                    ScriviLog( "Si è verificato un errore: " + ex.Message , "danger" , true );
                }

                msg = String.Format( "Sono stati rimossi {0} records totali " , totale );
                ScriviLog( msg , "good" , true );
            }
            else
            {
                msg = String.Format( "Orario di chiusura raggiunto" );
                ScriviLog( msg , "good" , true );
            }

            msg = String.Format( "Termine batch pulizia tabella MyRai_LogAPI" );
            ScriviLog( msg , "good" , true );

            DateTime end = DateTime.Now;

            TimeSpan t = end.Subtract( start );
            msg = String.Format( "Tempo di esecuzione {0} giorni, {1} ore, {2} minuti, {3} secondi" , t.Days , t.Hours , t.Minutes , t.Seconds );
            ScriviLog( msg , "good" , true );
        }

        private static void PuliziaLogAzioni ( DateTime currentMin , DateTime currentMax )
        {
            string msg = "";
            int conteggio = 0;
            int totale = 0;

            DateTime start = DateTime.Now;
            int maxRecords = 50000;

            if ( !IsOrarioChiusura( currentMin , currentMax ) )
            {
                try
                {
                    var connection = ConfigurationManager.ConnectionStrings["digiGappEntities"].ConnectionString;

                    if ( connection != null && connection.Contains( "ZTOLS420" ) )
                    {
                        ScriviLog( "Avvio batch pulizia tabella MyRai_LogAzioni (PRODUZIONE)" , "good" , true );
                    }
                    else
                    {
                        ScriviLog( "Avvio batch pulizia tabella MyRai_LogAzioni (SVILUPPO)" , "good" , true );
                    }

                    string query = "";
                    DateTime oggi = DateTime.Now;
                    oggi = oggi.AddMonths( -1 );
                    DateTime dataLimite = new DateTime( oggi.Year , oggi.Month , oggi.Day , 23 , 59 , 59 );

                    using ( digiGappEntities db = new digiGappEntities( ) )
                    {
                        do
                        {
                            query = "SELECT COUNT (A.ID) FROM (SELECT TOP ##MAXRECORDS## (id) FROM [digiGapp].[dbo].[MyRai_LogAzioni] WHERE [data] <= '##DATALIMITE##' AND (operazione is null OR operazione not like '%Rewrite%' )) A";
                            query = query.Replace( "##MAXRECORDS##" , maxRecords.ToString( ) );
                            query = query.Replace( "##DATALIMITE##" , dataLimite.ToString( "yyyy-MM-dd HH:mm:ss.000" ) );
                            conteggio = db.Database.SqlQuery<int>( query ).FirstOrDefault( );

                            if ( conteggio == 0 )
                            {
                                msg = String.Format( "Non ci sono records da eliminare" );
                                ScriviLogFile( msg );
                                ScriviLog( msg , "good" , true );
                                break;
                            }

                            query = "DELETE FROM [digiGapp].[dbo].[MyRai_LogAzioni] WHERE ID in ( SELECT TOP ##MAXRECORDS## (id) FROM [digiGapp].[dbo].[MyRai_LogAzioni] WHERE [data] <= '##DATALIMITE##' AND (operazione is null OR operazione not like '%Rewrite%') order by [data] )";
                            query = query.Replace( "##MAXRECORDS##" , maxRecords.ToString( ) );
                            query = query.Replace( "##DATALIMITE##" , dataLimite.ToString( "yyyy-MM-dd HH:mm:ss.000" ) );
                            db.Database.ExecuteSqlCommand( query );

                            totale += conteggio;

                        } while ( !IsOrarioChiusura( currentMin , currentMax ) );

                        if ( IsOrarioChiusura( currentMin , currentMax ) )
                        {
                            msg = String.Format( "Orario di chiusura raggiunto" );
                            ScriviLog( msg , "good" , true );
                        }
                    }
                }
                catch ( Exception ex )
                {
                    ScriviLog( "Si è verificato un errore: " + ex.Message , "danger" , true );
                }

                msg = String.Format( "Sono stati rimossi {0} records totali " , totale );
                ScriviLog( msg , "good" , true );
            }
            else
            {
                msg = String.Format( "Orario di chiusura raggiunto" );
                ScriviLog( msg , "good" , true );
            }

            msg = String.Format( "Termine batch pulizia tabella MyRai_LogAzioni" );
            ScriviLog( msg , "good" , true );

            DateTime end = DateTime.Now;

            TimeSpan t = end.Subtract( start );
            msg = String.Format( "Tempo di esecuzione {0} giorni, {1} ore, {2} minuti, {3} secondi" , t.Days , t.Hours , t.Minutes , t.Seconds );
            ScriviLog( msg , "good" , true );
        }

        private static void PuliziaLogDB ( DateTime currentMin , DateTime currentMax )
        {
            string msg = "";
            int conteggio = 0;
            int totale = 0;

            DateTime start = DateTime.Now;
            int maxRecords = 50000;

            if ( !IsOrarioChiusura( currentMin , currentMax ) )
            {
                try
                {
                    var connection = ConfigurationManager.ConnectionStrings["digiGappEntities"].ConnectionString;

                    if ( connection != null && connection.Contains( "ZTOLS420" ) )
                    {
                        ScriviLog( "Avvio batch pulizia tabella MyRai_LogDB (PRODUZIONE)" , "good" , true );
                    }
                    else
                    {
                        ScriviLog( "Avvio batch pulizia tabella MyRai_LogDB (SVILUPPO)" , "good" , true );
                    }

                    string query = "";
                    DateTime oggi = DateTime.Now;
                    oggi = oggi.AddMonths( -1 );
                    DateTime dataLimite = new DateTime( oggi.Year , oggi.Month , oggi.Day , 23 , 59 , 59 );

                    using ( digiGappEntities db = new digiGappEntities( ) )
                    {
                        do
                        {
                            query = "SELECT COUNT (A.ID) FROM (SELECT TOP ##MAXRECORDS## (id) FROM [digiGapp].[dbo].[MyRai_LogDB] WHERE [Data] <= '##DATALIMITE##') A";
                            query = query.Replace( "##MAXRECORDS##" , maxRecords.ToString( ) );
                            query = query.Replace( "##DATALIMITE##" , dataLimite.ToString( "yyyy-MM-dd HH:mm:ss.000" ) );
                            conteggio = db.Database.SqlQuery<int>( query ).FirstOrDefault( );

                            if ( conteggio == 0 )
                            {
                                msg = String.Format( "Non ci sono records da eliminare" );
                                ScriviLogFile( msg );
                                ScriviLog( msg , "good" , true );
                                break;
                            }

                            query = "DELETE FROM [digiGapp].[dbo].[MyRai_LogDB] WHERE ID in ( SELECT TOP ##MAXRECORDS## (id) FROM [digiGapp].[dbo].[MyRai_LogDB] WHERE [Data] <= '##DATALIMITE##' order by [Data] )";
                            query = query.Replace( "##MAXRECORDS##" , maxRecords.ToString( ) );
                            query = query.Replace( "##DATALIMITE##" , dataLimite.ToString( "yyyy-MM-dd HH:mm:ss.000" ) );
                            db.Database.ExecuteSqlCommand( query );

                            totale += conteggio;

                        } while ( !IsOrarioChiusura( currentMin , currentMax ) );

                        if ( IsOrarioChiusura( currentMin , currentMax ) )
                        {
                            msg = String.Format( "Orario di chiusura raggiunto" );
                            ScriviLog( msg , "good" , true );
                        }
                    }
                }
                catch ( Exception ex )
                {
                    ScriviLog( "Si è verificato un errore: " + ex.Message , "danger" , true );
                }

                msg = String.Format( "Sono stati rimossi {0} records totali " , totale );
                ScriviLog( msg , "good" , true );
            }
            else
            {
                msg = String.Format( "Orario di chiusura raggiunto" );
                ScriviLog( msg , "good" , true );
            }

            msg = String.Format( "Termine batch pulizia tabella MyRai_LogDB" );
            ScriviLog( msg , "good" , true );

            DateTime end = DateTime.Now;

            TimeSpan t = end.Subtract( start );
            msg = String.Format( "Tempo di esecuzione {0} giorni, {1} ore, {2} minuti, {3} secondi" , t.Days , t.Hours , t.Minutes , t.Seconds );
            ScriviLog( msg , "good" , true );
        }

        private static void PuliziaLogErrori ( DateTime currentMin , DateTime currentMax )
        {
            string msg = "";
            int conteggio = 0;
            int totale = 0;

            DateTime start = DateTime.Now;
            int maxRecords = 50000;

            if ( !IsOrarioChiusura( currentMin , currentMax ) )
            {
                try
                {
                    var connection = ConfigurationManager.ConnectionStrings["digiGappEntities"].ConnectionString;

                    if ( connection != null && connection.Contains( "ZTOLS420" ) )
                    {
                        ScriviLog( "Avvio batch pulizia tabella MyRai_LogErrori (PRODUZIONE)" , "good" , true );
                    }
                    else
                    {
                        ScriviLog( "Avvio batch pulizia tabella MyRai_LogErrori (SVILUPPO)" , "good" , true );
                    }

                    string query = "";
                    DateTime oggi = DateTime.Now;
                    oggi = oggi.AddMonths( -2 );
                    DateTime dataLimite = new DateTime( oggi.Year , oggi.Month , oggi.Day , 23 , 59 , 59 );

                    using ( digiGappEntities db = new digiGappEntities( ) )
                    {
                        do
                        {
                            query = "SELECT COUNT (A.ID) FROM (SELECT TOP ##MAXRECORDS## (id) FROM [digiGapp].[dbo].[MyRai_LogErrori] WHERE [Data] <= '##DATALIMITE##') A";
                            query = query.Replace( "##MAXRECORDS##" , maxRecords.ToString( ) );
                            query = query.Replace( "##DATALIMITE##" , dataLimite.ToString( "yyyy-MM-dd HH:mm:ss.000" ) );
                            conteggio = db.Database.SqlQuery<int>( query ).FirstOrDefault( );

                            if ( conteggio == 0 )
                            {
                                msg = String.Format( "Non ci sono records da eliminare" );
                                ScriviLogFile( msg );
                                ScriviLog( msg , "good" , true );
                                break;
                            }

                            query = "DELETE FROM [digiGapp].[dbo].[MyRai_LogErrori] WHERE ID in ( SELECT TOP ##MAXRECORDS## (id) FROM [digiGapp].[dbo].[MyRai_LogErrori] WHERE [Data] <= '##DATALIMITE##' order by [Data] )";
                            query = query.Replace( "##MAXRECORDS##" , maxRecords.ToString( ) );
                            query = query.Replace( "##DATALIMITE##" , dataLimite.ToString( "yyyy-MM-dd HH:mm:ss.000" ) );
                            db.Database.ExecuteSqlCommand( query );

                            totale += conteggio;

                        } while ( !IsOrarioChiusura( currentMin , currentMax ) );

                        if ( IsOrarioChiusura( currentMin , currentMax ) )
                        {
                            msg = String.Format( "Orario di chiusura raggiunto" );
                            ScriviLog( msg , "good" , true );
                        }
                    }
                }
                catch ( Exception ex )
                {
                    ScriviLog( "Si è verificato un errore: " + ex.Message , "danger" , true );
                }

                msg = String.Format( "Sono stati rimossi {0} records totali " , totale );
                ScriviLog( msg , "good" , true );
            }
            else
            {
                msg = String.Format( "Orario di chiusura raggiunto" );
                ScriviLog( msg , "good" , true );
            }

            msg = String.Format( "Termine batch pulizia tabella MyRai_LogErrori" );
            ScriviLog( msg , "good" , true );

            DateTime end = DateTime.Now;

            TimeSpan t = end.Subtract( start );
            msg = String.Format( "Tempo di esecuzione {0} giorni, {1} ore, {2} minuti, {3} secondi" , t.Days , t.Hours , t.Minutes , t.Seconds );
            ScriviLog( msg , "good" , true );
        }

        private static void PuliziaLogMensa ( DateTime currentMin , DateTime currentMax )
        {
            string msg = "";
            int conteggio = 0;
            int totale = 0;
            string nomeReport = "";

            DateTime start = DateTime.Now;
            int maxRecords = 200000;
            MyRai_ParametriSistema createBackupFile = new MyRai_ParametriSistema( );

            if ( !IsOrarioChiusura( currentMin , currentMax ) )
            {
                try
                {
                    var connection = ConfigurationManager.ConnectionStrings["digiGappEntities"].ConnectionString;

                    if ( connection != null && connection.Contains( "ZTOLS420" ) )
                    {
                        ScriviLog( "Avvio batch pulizia tabella MyRai_MensaXML (PRODUZIONE)" , "good" , true );
                    }
                    else
                    {
                        ScriviLog( "Avvio batch pulizia tabella MyRai_MensaXML (SVILUPPO)" , "good" , true );
                    }

                    string query = "";
                    DateTime oggi = DateTime.Now;
                    oggi = oggi.AddYears( -1 );
                    DateTime dataLimite = new DateTime( oggi.Year , oggi.Month , oggi.Day , 23 , 59 , 59 );

                    using ( digiGappEntities db = new digiGappEntities( ) )
                    {
                        do
                        {
                            createBackupFile = db.MyRai_ParametriSistema.Where( w => w.Chiave.Equals( "PuliziaLogMensaBackupFile" ) ).FirstOrDefault( );

                            query = "SELECT COUNT (A.ID) FROM (SELECT TOP ##MAXRECORDS## (id) FROM [digiGapp].[dbo].[MyRai_MensaXML] WHERE [TransactionDateTime] <= '##DATALIMITE##') A";
                            query = query.Replace( "##MAXRECORDS##" , maxRecords.ToString( ) );
                            query = query.Replace( "##DATALIMITE##" , dataLimite.ToString( "yyyy-MM-dd HH:mm:ss.000" ) );
                            conteggio = db.Database.SqlQuery<int>( query ).FirstOrDefault( );

                            if ( conteggio == 0 )
                            {
                                msg = String.Format( "Non ci sono records da eliminare" );
                                ScriviLogFile( msg );
                                ScriviLog( msg , "good" , true );
                                break;
                            }

                            if ( createBackupFile != null )
                            {
                                if ( createBackupFile.Valore1.ToUpper( ) == "TRUE" )
                                {
                                    msg = String.Format( "Generazione file xml di backup" );
                                    ScriviLogFile( msg );

                                    query = "SELECT TOP ##MAXRECORDS## [id],[LocationID],[PosID],[TransactionID],[TransactionDateTime],[Total],[Badge],[XMLorig] FROM [digiGapp].[dbo].[MyRai_MensaXML] WHERE [TransactionDateTime] <= '##DATALIMITE##' order by [TransactionDateTime]";
                                    query = query.Replace( "##MAXRECORDS##" , maxRecords.ToString( ) );
                                    query = query.Replace( "##DATALIMITE##" , dataLimite.ToString( "yyyy-MM-dd HH:mm:ss.000" ) );
                                    var logs = db.Database.SqlQuery<MyRai_MensaXML>( query ).ToList( );

                                    if ( logs != null && logs.Any( ) )
                                    {
                                        XElement xmlElements = new XElement( "Records" , logs.Select( i => new XElement( "Record" ,
                                                                   new XAttribute( "id" , i.id ) ,
                                                                   new XAttribute( "LocationID" , i.LocationID ) ,
                                                                   new XAttribute( "PosID" , i.PosID ) ,
                                                                   new XAttribute( "TransactionID" , i.TransactionID ) ,
                                                                   new XAttribute( "TransactionDateTime" , i.TransactionDateTime.ToString( "dd/MM/yyyy HH:mm:ss" ) ) ,
                                                                   new XAttribute( "Total" , i.Total ) ,
                                                                   new XAttribute( "Badge" , i.Badge ) ,
                                                                   new XAttribute( "XMLorig" , i.XMLorig )
                                                               ) ) );

                                        nomeReport = AppDomain.CurrentDomain.BaseDirectory + String.Format( "PuliziaLogMensa_{0}.xml" , DateTime.Now.ToString( "ddMMyyyy_HHmmss" ) );
                                        xmlElements.Save( nomeReport );

                                        msg = String.Format( "File xml salvato in {0}" , nomeReport );
                                        ScriviLogFile( msg );
                                    }
                                }
                            }

                            query = "DELETE FROM [digiGapp].[dbo].[MyRai_MensaXML] WHERE ID in ( SELECT TOP ##MAXRECORDS## (id) FROM [digiGapp].[dbo].[MyRai_MensaXML] WHERE [TransactionDateTime] <= '##DATALIMITE##' order by [TransactionDateTime] )";
                            query = query.Replace( "##MAXRECORDS##" , maxRecords.ToString( ) );
                            query = query.Replace( "##DATALIMITE##" , dataLimite.ToString( "yyyy-MM-dd HH:mm:ss.000" ) );
                            db.Database.ExecuteSqlCommand( query );

                            totale += conteggio;

                        } while ( !IsOrarioChiusura( currentMin , currentMax ) );

                        if ( IsOrarioChiusura( currentMin , currentMax ) )
                        {
                            msg = String.Format( "Orario di chiusura raggiunto" );
                            ScriviLog( msg , "good" , true );
                        }
                    }
                }
                catch ( Exception ex )
                {
                    ScriviLog( "Si è verificato un errore: " + ex.Message , "danger" , true );
                }

                msg = String.Format( "Sono stati rimossi {0} records totali " , totale );
                ScriviLog( msg , "good" , true );
            }
            else
            {
                msg = String.Format( "Orario di chiusura raggiunto" );
                ScriviLog( msg , "good" , true );
            }

            msg = String.Format( "Termine batch pulizia tabella MyRai_MensaXML" );
            ScriviLog( msg , "good" , true );

            DateTime end = DateTime.Now;

            TimeSpan t = end.Subtract( start );
            msg = String.Format( "Tempo di esecuzione {0} giorni, {1} ore, {2} minuti, {3} secondi" , t.Days , t.Hours , t.Minutes , t.Seconds );
            ScriviLog( msg , "good" , true );
        }

    }
}
