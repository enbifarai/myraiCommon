using Multiplo.Helpers;
using Multiplo.Models;
using myRaiData;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Objects;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Multiplo
{
    public static class CaricaAttivitaCeitonPerApprovatori
    {
        private static void Write(string msg)
        {
            Output.WriteLine(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " " + msg);
        }

        private static void ScriviFile(string msg, string nomeFile = "")
        {
            if (!String.IsNullOrEmpty(msg))
            {
                string filelog1 = "";
                var location = System.Reflection.Assembly.GetEntryAssembly().Location;

                var directoryPath = Path.GetDirectoryName(location);
                var logPath = Path.Combine(directoryPath, nomeFile);
                if (!Directory.Exists(logPath))
                    Directory.CreateDirectory(logPath);

                filelog1 = Path.Combine(logPath, "ConsoleLog_" + DateTime.Now.ToString("yyyyMMdd") + ".txt");

                System.IO.File.AppendAllText(filelog1, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " " + msg + "\r\n");
            }
        }

        private static void ScriviLog(string msg, string fullFilePath, string separatore = ",")
        {
            if (!String.IsNullOrEmpty(msg))
            {
                System.IO.File.AppendAllText(fullFilePath, DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + separatore + msg + "\r\n");
            }
        }

        public static void PopolaTabellaApprovatoriProduzioni(string[] args)
        {
            List<string> toSlack = new List<string>();
            bool giaScritto = false;
            string messaggio = "";

            try
            {
                var connection = ConfigurationManager.ConnectionStrings["digiGappEntities"].ConnectionString;
                messaggio = "Avvio batch popola MyRai_ApprovatoriProduzioni";
                Write(messaggio);
                Console.WriteLine(messaggio);
                ScriviFile(messaggio);
                if (connection != null && connection.Contains("ZTOLS420"))
                {
                    ScriviFile("PopolaTabellaApprovatoriProduzioni PRODUZIONE");

                }
                else
                {
                    ScriviFile("PopolaTabellaApprovatoriProduzioni SVILUPPO");
                }

                List<string> matricole = new List<string>();
                using (digiGappEntities db = new digiGappEntities())
                {
                    messaggio = "Reperimento delle matricole degli approvatori di produzione";
                    Console.WriteLine(messaggio);
                    ScriviFile(messaggio);
                    Write(messaggio);

                    matricole = db.MyRai_ApprovatoriProduzioni.Where(w => !w.MatricolaApprovatore.StartsWith("UFF")).ToList().Select(w => w.MatricolaApprovatore).Distinct().ToList();

                    messaggio = String.Format("Trovati {0} approvatori", matricole.Count());
                    Console.WriteLine(messaggio);
                    Write(messaggio);
                    ScriviFile(messaggio);
                }

                if (matricole != null && matricole.Any())
                {
                    messaggio = String.Format("Avvio reperimento attività");
                    Write(messaggio);
                    Console.WriteLine(messaggio);
                    ScriviFile(messaggio);

                    int riga = 0;

                    DateTime oggi = DateTime.Now;
                    DateTime inizioMese = new DateTime(oggi.Year, oggi.Month, 1);
                    //DateTime inizioMese = new DateTime(oggi.Year, 1, 1);

                    // da 1 a 15
                    DateTime inizioSecondoGiro = inizioMese.AddDays(14);
                    DateTime currentMaxDate = inizioSecondoGiro;

                    // fine mese
                    DateTime fineMese = inizioMese.AddMonths(1);
                    fineMese = fineMese.AddDays(-1);

                    if (args != null && args.Any())
                    {
                        string dt1 = args[0];
                        string dt2 = "";
                        if (args.Count() > 1)
                        {
                            dt2 = args[1];
                        }

                        Boolean datavalida = DateTime.TryParseExact(dt1, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out inizioMese);

                        if (!datavalida)
                        {
                            throw new Exception("La data inizio inserita non è valida");
                        }

                        if (!String.IsNullOrEmpty(dt2))
                        {
                            datavalida = DateTime.TryParseExact(dt2, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out fineMese);

                            if (!datavalida)
                            {
                                throw new Exception("La data fine inserita non è valida");
                            }
                        }
                        else
                        {
                            fineMese = new DateTime(inizioMese.Year, inizioMese.Month, 1);
                            fineMese = fineMese.AddDays(-1);
                        }

                        inizioSecondoGiro = inizioMese.AddDays(14);
                        currentMaxDate = inizioSecondoGiro;
                    }

                    DateTime inizio = inizioMese;

                    do
                    {
                        matricole.ForEach(w =>
                        {
                            riga++;
                            messaggio = String.Format("{0}) Reperimento attività per la matricola {1}", riga, w);
                            Console.WriteLine(messaggio);
                            ScriviFile(messaggio);

                            int inseriti = Helper.GetWeekPlan("P" + w, inizio, currentMaxDate);
                            toSlack.Add(String.Format("Inserite {0} attività per la matricola {1}", inseriti, w));
                            Write(String.Format("Inserite {0} attività per la matricola {1} per il periodo dal {2} al {3}", inseriti, w, inizio, currentMaxDate));
                        });

                        inizio = currentMaxDate.AddDays(1);
                        currentMaxDate = currentMaxDate.AddDays(15);
                        if (currentMaxDate > fineMese)
                        {
                            currentMaxDate = fineMese;
                        }

                    } while (inizio <= currentMaxDate);
                }
            }
            catch (Exception ex)
            {
                string scarico = "";
                foreach (var s in toSlack)
                {
                    scarico += s + "\n\r";
                }
                ScriviFile("Si è verificato un errore: " + ex.ToString());
                Write("Si è verificato un errore: " + ex.ToString());
                giaScritto = true;
            }

            if (!giaScritto)
            {
                string scarico = "";
                foreach (var s in toSlack)
                {
                    scarico += s + "\n\r";
                }
            }

            messaggio = "Fine batch popola MyRai_ApprovatoriProduzioni";
            Console.WriteLine(messaggio);
            ScriviFile(messaggio);
        }

        private static T[] GetParametri<T>(EnumParametriSistema chiave)
        {
            var db = new digiGappEntities();
            String NomeParametro = chiave.ToString();

            MyRai_ParametriSistema p = db.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == NomeParametro);
            if (p == null)
                return null;
            else
            {
                T[] parametri = new T[] { (T)Convert.ChangeType(p.Valore1, typeof(T)), (T)Convert.ChangeType(p.Valore2, typeof(T)) };
                return parametri;
            }
        }

        private static bool GetAttivitaSirio(string pMatricola, DateTime dataInizio, DateTime dataFine, out Multiplo.Models.Sirio.Response.Envelope response)
        {
            bool result = false;
            response = null;

            if (String.IsNullOrWhiteSpace(pMatricola))
                return false;

            string[] par = GetParametri<string>(EnumParametriSistema.ServizioCeitonGetAttivita);

            var _url = par[0];
            var _action = par[1];

            try
            {
                Models.Sirio.Request.Envelope env = new Models.Sirio.Request.Envelope();
                env.Body = new Models.Sirio.Request.EnvelopeBody();
                env.Body.CTWS_REQ = new Models.Sirio.Request.EnvelopeBodyCTWS_REQ();
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
                string[] AccountUtenteServizio = GetParametri<string>(EnumParametriSistema.AccountUtenteServizio);
                webRequest.Credentials = new System.Net.NetworkCredential(AccountUtenteServizio[0], AccountUtenteServizio[1], "RAI");

                using (Stream stream = webRequest.GetRequestStream())
                {
                    //soapEnvelopeXml.Save(stream);
                    XmlSerializer xx = new XmlSerializer(typeof(Models.Sirio.Request.Envelope));
                    xx.Serialize(stream, env);
                }

                // begin async call to web request.
                IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);

                // suspend this thread until call is complete. You might want to
                // do something usefull here like update your UI.
                asyncResult.AsyncWaitHandle.WaitOne();

                // get the response from the completed web request.
                response = new Models.Sirio.Response.Envelope();
                using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult))
                {
                    using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
                    {
                        XmlSerializer x = new XmlSerializer(typeof(Models.Sirio.Response.Envelope));
                        response = (Models.Sirio.Response.Envelope)x.Deserialize(rd);
                    }
                }

                result = response != null && response.Body != null && response.Body.CTWS_RES != null && response.Body.CTWS_RES.ActivityExport != null;
            }
            catch (Exception ex)
            {

            }
            return result;
        }

        private static List<GetAttivitaResponse> GetAttivita(string pMatricola, DateTime giorno)
        {
            List<GetAttivitaResponse> response = new List<GetAttivitaResponse>();            
            string messaggio = "";

            WeekPlan weekPlan = new WeekPlan();

            DateTime startDate = giorno;
            DateTime endDate = giorno;

            weekPlan = new WeekPlan();
            int numDays = (endDate - startDate).Days + 1;

            for (int i = 0; i < numDays; i++)
                weekPlan.Days.Add(new DayPlan() { Date = startDate.AddDays(i) });

            Models.Sirio.Response.Envelope envRes = new Models.Sirio.Response.Envelope();

            if (GetAttivitaSirio(pMatricola, startDate, endDate, out envRes))
            {
                digiGappEntities db = new digiGappEntities();

                messaggio = String.Format("Trovate {0} attività", envRes.Body.CTWS_RES.ActivityExport.Count());
                Console.WriteLine(messaggio);
                ScriviFile(messaggio);

                foreach (Models.Sirio.Response.CTWS_RESActivityExport item in envRes.Body.CTWS_RES.ActivityExport)
                {
                    response.Add(new GetAttivitaResponse()
                    {
                        Nome = item.titolo,
                        Data = giorno,
                        IdAttivita = item.idAttivita
                    });
                }
            }
            else
            {
                messaggio = String.Format("Nessuna attività trovata");
                Console.WriteLine(messaggio);
                ScriviFile(messaggio);
            }

            return response;
        }

        public static void Prova()
        {
            const string FILEINPUT = @"C:\\RAI\\Eccezioni_Senza_attivita.csv";
            const Int32 BUFFERSIZE = 4096;
            string log = String.Empty;
            string dtNow = DateTime.Now.ToString("dd_MM_yyyy");
            
            #region CREAZIONE PATH PER LE LOG
            var locationLogPathBase = System.Configuration.ConfigurationManager.AppSettings["PercorsoLog_CercaAttivitaPerUtenti"] ?? System.Reflection.Assembly.GetEntryAssembly().Location;
            var logPath = Path.Combine(locationLogPathBase, dtNow);

            if (!Directory.Exists(logPath))
                Directory.CreateDirectory(logPath);

            logPath = Path.Combine(logPath, "log.txt");
            #endregion

            string logIntestazione = String.Format("ID,MATRICOLA,DATA,NOME ATTIVITA, IDATTIVITA_CEITON,ERRORE");
            string logTemplate = "{0},{1},{2},{3},{4},{5}";

            digiGappEntities db = new digiGappEntities();
            int riga = 0;
            // legge il txt            
            using (var fileStream = File.OpenRead(FILEINPUT))
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BUFFERSIZE))
            {
                String line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    riga++;
                    Console.WriteLine("riga " + riga);
                    string temp_idRichiesta = String.Empty;
                    string matricola = String.Empty;
                    string temp_data = String.Empty;
                    string temp_nome_attivita = String.Empty;
                    int idAttivitaCeiton = 0;
                    try
                    {
                        // esempio di riga
                        // 11599493;622600;2022-12-30
                        // IDRICHIESTA;MATRICOLA;DATA
                        List<string> elementiRiga = new List<string>();
                        elementiRiga.AddRange(line.Split(';').ToList());
                        temp_idRichiesta = elementiRiga[0];
                        matricola = elementiRiga[1];
                        temp_data = elementiRiga[2];

                        DateTime dt;
                        if (!DateTime.TryParseExact(temp_data, "yyyy-MM-dd", null, DateTimeStyles.None, out dt))
                        {
                            throw new Exception("Errore nella conversione della data");
                        }

                        List<GetAttivitaResponse> attivita = GetAttivita("P" + matricola, dt);
                        if (attivita != null && attivita.Any())
                        {
                            foreach (var a in attivita)
                            {
                                Output.WriteLine(String.Format("{0},{1},{2}",matricola, a.Nome.Trim(), dt.ToString("dd/MM/yyyy")));

                                var attCeiton = from c in db.MyRai_AttivitaCeiton
                                                where c.idCeiton == a.IdAttivita
                                                && c.Titolo.Trim() == a.Nome.Trim()
                                                select c;

                                List<MyRai_AttivitaCeiton> atts = new List<MyRai_AttivitaCeiton>();
                                atts = attCeiton.ToList();

                                if (atts != null && atts.Any())
                                {
                                    foreach (var _a in atts)
                                    {
                                        temp_nome_attivita = _a.Titolo.Trim();
                                        idAttivitaCeiton = _a.id;
                                        log = String.Format(logTemplate, temp_idRichiesta, matricola, temp_data, temp_nome_attivita, idAttivitaCeiton, "");
                                        ScriviLog(log, logPath, ",");
                                    }
                                }
                                else
                                {
                                    throw new Exception("Nessuna attività trovata in MyRai_AttivitaCeiton per la matricola e data cercata");
                                }
                            }
                        }
                        else
                        {
                            throw new Exception("Nessuna attività trovata per la matricola e data cercata");
                        }
                    }
                    catch (Exception ex)
                    {
                        log = String.Format(logTemplate, temp_idRichiesta, matricola, temp_data, temp_nome_attivita, idAttivitaCeiton, ex.Message);
                        ScriviLog(log, logPath, ",");
                    }
                    System.Threading.Thread.Sleep(2000);
                }
            }
        }
    }
}