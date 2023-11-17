using myRaiData;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
namespace System.Runtime.CompilerServices
{
    public class ExtensionAttribute : Attribute { }
}
namespace TimbratureCore
{

    public static class TimbratureManager
    {
        public static string GetSegmento(int minuto)
        {
            if (minuto >= 0 && minuto <= 6 * 60) return "24_6";
            else if (minuto > 6 * 60 && minuto <= 21 * 60) return "6_21";
            else if (minuto > 21 * 60 && minuto <= 24 * 60) return "21_24";
            else return "24_6";
        }
        public static string TestEcc(string url, string e)
        {
            var db = new digiGappEntities();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("TEST " + e.ToUpper() + ";;;;;;<br />");
            sb.AppendLine("GAPP;Ricalcolo;Differenza; SimulaUrl Locale ;SimulaUrl Prod;TestUrl Locale;Testurl Prod <br />");

            WSDigigapp serviceWS = new WSDigigapp
            {
                Credentials = new System.Net.NetworkCredential("srvruofpo", "zaq22?mk")
            };

            DateTime D = DateTime.Today;

            var EccList = db.MyRai_Eccezioni_Richieste.Where(item => item.cod_eccezione == e.ToUpper() && item.id_stato == 20 &&
            item.data_eccezione.Year == 2019
            //item.data_creazione>D
            ).OrderBy(item => item.data_eccezione).ToList();
            int counter = 0;

            var L = new List<dynamic>();

            foreach (var ecc in EccList)
            {
                counter++;
                //if (counter > 100) break;
                dayResponse day = serviceWS.getEccezioni(ecc.MyRai_Richieste.matricola_richiesta, ecc.data_eccezione.ToString("ddMMyyyy"), "BU", 80);
                dayResponse dayRespYesterday = null;
                if (e.ToUpper() == "AT30")
                {
                    dayRespYesterday = serviceWS.getEccezioni(ecc.MyRai_Richieste.matricola_richiesta, ecc.data_eccezione.AddDays(-1).ToString("ddMMyyyy"), "BU", 80);
                }

                if (day.eccezioni != null)
                {
                    var eccezioneGapp = day.eccezioni.Where(i => i.cod.Trim() == e.ToUpper()).FirstOrDefault();
                    if (eccezioneGapp != null)
                    {
                        DayAnalysisBase da = DayAnalysisFactory.GetDayAnalysisClass(e.ToUpper(), day, true, dayRespYesterday);//attenzione - presume QUADR GIORNALIERA

                        if (da != null)
                        {
                            EccezioneQuantita result = da.GetEccezioneQuantita();

                            L.Add(new
                            {
                                qtaGapp = eccezioneGapp.qta,
                                resultHHMM = result.QuantitaMinutiHHMM,
                                url1 = url + "/tech/getag?m=" + ecc.MyRai_Richieste.matricola_richiesta + "&d=" + ecc.data_eccezione.ToString("ddMMyyyy") + "&e=" + e,
                                url1a = "http://raiperme.intranet.rai.it" + "/tech/getag?m=" + ecc.MyRai_Richieste.matricola_richiesta + "&d=" + ecc.data_eccezione.ToString("ddMMyyyy") + "&e=" + e,
                                url2 = url + "/home/simula?m=" + ecc.MyRai_Richieste.matricola_richiesta,
                                url2a = "http://raiperme.intranet.rai.it" + "/home/simula?m=" + ecc.MyRai_Richieste.matricola_richiesta,
                                diff = (eccezioneGapp.qta.ToMinutes() - result.QuantitaMinuti)
                            });
                        }
                    }
                }
            }
            L = L.OrderByDescending(x => Math.Abs((int)x.diff)).ToList();
            foreach (var item in L)
            {
                sb.Append(item.qtaGapp + " ; ");
                sb.Append(item.resultHHMM + " ; ");
                sb.Append(item.diff.ToString().PadLeft(4, ' ') + " ; ");
                sb.Append(item.url2 + " ; ");
                sb.Append(item.url2a + " ; ");
                sb.Append(item.url1 + " ; ");
                sb.Append(item.url1a + " ; <br />");
            }

            return sb.ToString();
        }






















        public static Boolean TurnoMaggioreDiSogliaNotturno(string TurnoStart, string TurnoEnd)
        {
            if (String.IsNullOrWhiteSpace(TurnoStart) || String.IsNullOrWhiteSpace(TurnoEnd)) return false;

            return GetPercentualeNotturnoSuOrario(TurnoStart, TurnoEnd) >= myRaiCommonTasks.CommonTasks.GetParametro<float>(myRaiCommonTasks.CommonTasks.EnumParametriSistema.SogliaNotturno);
        }
        private static float GetPercentualeNotturnoSuOrario(string TurnoStart, string TurnoEnd)
        {
            string[] EstremiNotturno = myRaiCommonTasks.CommonTasks.GetParametri<String>(myRaiCommonTasks.CommonTasks.EnumParametriSistema.EstremiNotturno);
            if (EstremiNotturno == null || EstremiNotturno.Length != 2 || String.IsNullOrWhiteSpace(EstremiNotturno[0]) || String.IsNullOrWhiteSpace(EstremiNotturno[1]))
                return 0;
            List<int> MinutiRangeTurno = GetMinutiRange(TurnoStart, TurnoEnd);
            List<int> MinutiRangeNotturno = GetMinutiRange(EstremiNotturno[0], EstremiNotturno[1]);


            return ((float)(MinutiRangeTurno.Intersect(MinutiRangeNotturno).Count()) / (float)MinutiRangeTurno.Count()) * 100f;
        }
        private static Boolean CadeInServizio(dayResponse resp, int minuto)
        {
            if (resp == null || resp.timbrature == null) return false;
            List<int> L = new List<int>();
            foreach (var t in resp.timbrature)
            {
                if (t.entrata == null || t.uscita == null || String.IsNullOrWhiteSpace(t.entrata.orario) || String.IsNullOrWhiteSpace(t.uscita.orario)) continue;
                L.AddRange(GetMinutiRange(t.entrata.orario, t.uscita.orario));
            }
            return L.Contains(minuto);
        }
        private static List<int> GetMinutiRange(string TimeFromHHMM, string TimeToHHMM)
        {
            List<int> ListMinuti = new List<int>();
            if (TimeFromHHMM.ToMinutes() < TimeToHHMM.ToMinutes())
                for (int k = TimeFromHHMM.ToMinutes(); k <= TimeToHHMM.ToMinutes(); k++) ListMinuti.Add(k);
            else
            {
                for (int k = TimeFromHHMM.ToMinutes(); k <= 1440; k++) ListMinuti.Add(k);
                for (int k = 0; k <= TimeToHHMM.ToMinutes(); k++) ListMinuti.Add(k);
            }
            return ListMinuti;
        }
        public static string Serialize<T>(T v)
        {
            string json = JsonConvert.SerializeObject(v);
            // string json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(v);
            return json;
        }
        public static T DeSerialize<T>(string json)
        {
            var p = JsonConvert.DeserializeObject<T>(json);
            // var p= new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<T>(json);
            return p;
        }
        public static Eccezione[] Verifica(dayResponse R, Eccezione[] eccezioni)
        {
            return null;
        }

        public static DIGIRESP_Archivio_PDF InvalidaResoconto(Boolean QuadraturaSettimanale, string matricola,
            string sedegapp, DateTime data, string reparto)
        {
            if (!QuadraturaSettimanale) return null;

            var resoconto = ResocontoDaInvalidare(matricola, sedegapp, data, reparto);
            if (resoconto == null) return null;

            var db = new digiGappEntities();
            if (resoconto.stato_pdf == "S_OK")
            {
                var arch = db.DIGIRESP_Archivio_PDF.Find(resoconto.ID);
                if (arch != null)
                {
                    db.DIGIRESP_Archivio_PDF.Remove(arch);
                    db.SaveChanges();
                    TimbratureLogger.LogAzione(new MyRai_LogAzioni()
                    {
                        applicativo = "PORTALE",
                        data = DateTime.Now,
                        matricola = matricola,
                        provenienza = "TimbratureManager.InvalidaResoconto",
                        descrizione_operazione = "Resoconto eliminato id " + arch.ID
                    }, matricola);
                }
            }
            if (resoconto.stato_pdf == "C_OK")
            {
                List<string> matricole = myRaiCommonTasks.CommonTasks.GetMatricolaLivelloPerSede(sedegapp, 2);
                foreach (string ma in matricole)
                {
                    MyRai_Notifiche n = new MyRai_Notifiche()
                    {
                        data_inserita = DateTime.Now,
                        id_riferimento = resoconto.ID,
                        data_inviata = null,
                        data_letta = null,
                        tipo = null,
                        descrizione = "Il resoconto per la sede " + sedegapp + " dal " + resoconto.data_inizio.ToString("dd/MM/yyyy") +
                        " al " + resoconto.data_fine.ToString("dd/MM/yyyy") + " è variato rispetto alla versione firmata.",
                        inserita_da = matricola,
                        matricola_destinatario = ma,
                        categoria = "Resoconto settimanale",
                        email_destinatario = "P" + ma + "@rai.it"

                    };
                }
                db.SaveChanges();
            }
            return resoconto;
        }

        private static DIGIRESP_Archivio_PDF ResocontoDaInvalidare(string matricola, string sedegapp, DateTime data, string reparto)
        {
            var db = new digiGappEntities();
            var resoconto = db.DIGIRESP_Archivio_PDF
                .Where(x => x.data_inizio <= data &&
                            x.data_fine >= data &&
                            x.sede_gapp == sedegapp &&
                            x.tipologia_pdf == "P").FirstOrDefault();

            if (resoconto == null) return null;

            if (String.IsNullOrWhiteSpace(resoconto.contenuto_eccezioni)) return null;

            WSDigigapp service = new WSDigigapp()
            {
                Credentials = new System.Net.NetworkCredential(
                    TimbratureManager.GetParametri<string>("AccountUtenteServizio")[0],
                    TimbratureManager.GetParametri<string>("AccountUtenteServizio")[1])
            };
            presenzeResponse resp = service.getPresenzeNoPDF(matricola,
                                   "*",
                                   resoconto.data_inizio.ToString("ddMMyyyy"),
                                   resoconto.data_fine.ToString("ddMMyyyy"),
                                   sedegapp,
                                   75,
                                   reparto);

            presenzeResponse resp_db = DeSerialize<presenzeResponse>(resoconto.contenuto_eccezioni);

            if (resp == null || resp_db == null || resp.periodi == null || resp_db.periodi == null)
                return null;

            if (resp.periodi.Length != resp_db.periodi.Length)
            {
                return resoconto;
            }

            for (int p = 0; p < resp.periodi.Length; p++)
            {
                if ((resp.periodi[p].giornate.Length != resp_db.periodi[p].giornate.Length)
                    ||
                   (resp.periodi[p].deltaCarenze != resp_db.periodi[p].deltaCarenze)
                    ||
                   (resp.periodi[p].deltaMaggioriPresenze != resp_db.periodi[p].deltaMaggioriPresenze)
                    ||
                   (resp.periodi[p].deltaTotale != resp_db.periodi[p].deltaTotale))
                {
                    return resoconto;
                }

                for (int g = 0; g < resp.periodi[p].giornate.Length; g++)
                {
                    if ((resp.periodi[p].giornate[g].carenza != resp_db.periodi[p].giornate[g].carenza)
                       ||
                       (resp.periodi[p].giornate[g].maggiorPresenza != resp_db.periodi[p].giornate[g].maggiorPresenza))
                    {
                        return resoconto;
                    }
                    if (resp.periodi[p].giornate[g].eccezioni == null && resp_db.periodi[p].giornate[g].eccezioni == null)
                        continue;

                    if ((resp.periodi[p].giornate[g].eccezioni == null && resp_db.periodi[p].giornate[g].eccezioni != null)
                        ||
                       (resp.periodi[p].giornate[g].eccezioni != null && resp_db.periodi[p].giornate[g].eccezioni == null)
                        ||
                       (resp.periodi[p].giornate[g].eccezioni.Length != resp_db.periodi[p].giornate[g].eccezioni.Length)
                        ||
                      !resp.periodi[p].giornate[g].eccezioni.Select(x => x.cod).OrderBy(x => x).ToList()
                       .SequenceEqual(resp_db.periodi[p].giornate[g].eccezioni.Select(x => x.cod).OrderBy(x => x).ToList())
                        )
                    {
                        return resoconto;
                    }
                }
            }
            return null;
        }

        public static CarenzaTimbrature getCarenzaTimbrature(string matricola, dayResponse giornata = null,
            string data = null, string sedegapp = null, bool SpezzaConIntervalliMensa = true)
        {
            if (giornata == null)
            {
                WSDigigapp service = new WSDigigapp()
                {
                    Credentials = new System.Net.NetworkCredential(
                        TimbratureManager.GetParametri<string>("AccountUtenteServizio")[0],
                        TimbratureManager.GetParametri<string>("AccountUtenteServizio")[1])
                };
                giornata = service.getEccezioni(matricola, data, "BU", 75);
            }

            if (giornata == null
              || giornata.timbrature == null
              || giornata.timbrature.Length < 1)
                return null;

            if (giornata.timbrature.Last().uscita == null) return null;

            //Ottieni sedegapp se non passata/////////////////////////////////////////////////
            if (String.IsNullOrWhiteSpace(sedegapp))
            {
                MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client wcf1 = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
                wcf1.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(
                    TimbratureManager.GetParametri<string>("AccountUtenteServizio")[0],
                    TimbratureManager.GetParametri<string>("AccountUtenteServizio")[1]);

                var resp = wcf1.GetRecuperaUtente(matricola, DateTime.Today.ToString("ddMMyyyy"));
                if (resp.data != null)
                    sedegapp = resp.data.sede_gapp;
            }

            //Ottieni inizio e fine mensa per sedegapp //////////////////////////////////////////
            int inizioMensaMin = calcolaMinuti("11:45");
            string inizioMensa = "11:45";

            int fineMensaMin = calcolaMinuti("15:15");
            string fineMensa = "15:15";

            string inizioMensaSerale = "19:00";
            int inizioMensaSeraleMin = calcolaMinuti(inizioMensaSerale);

            string fineMensaSerale = "21:00";
            int fineMensaSeraleMin = calcolaMinuti(fineMensaSerale);

            if (!String.IsNullOrWhiteSpace(sedegapp))
            {
                var db = new digiGappEntities();
                var sede = db.L2D_SEDE_GAPP.Where(x => x.cod_sede_gapp == sedegapp).FirstOrDefault();
                if (sede != null)
                {
                    string intervMensa = sede.intervallo_variabile_mensa(giornata.giornata.data);
                    string intervMensaSerale = sede.intervallo_variabile_mensa_serale(giornata.giornata.data);

                    if (!String.IsNullOrWhiteSpace(intervMensa) && intervMensa.Contains("/")
                    && intervMensa.Trim().Length == 9)
                    {
                        inizioMensaMin = calcolaMinuti(intervMensa.Split('/')[0].Trim());
                        inizioMensa = intervMensa.Split('/')[0].Trim();
                        fineMensaMin = calcolaMinuti(intervMensa.Split('/')[1].Trim());
                        fineMensa = intervMensa.Split('/')[1].Trim();
                    }
                    if (!String.IsNullOrWhiteSpace(intervMensaSerale) &&
                        intervMensaSerale.Contains("/") &&
                        intervMensaSerale.Trim().Length == 9)
                    {
                        inizioMensaSeraleMin = calcolaMinuti(intervMensaSerale.Split('/')[0].Trim());
                        inizioMensaSerale = intervMensaSerale.Split('/')[0].Trim();
                        fineMensaSeraleMin = calcolaMinuti(intervMensaSerale.Split('/')[1].Trim());
                        fineMensaSerale = intervMensaSerale.Split('/')[1].Trim();
                    }
                }

            }

            //Ottieni Orario////////////////////////////////////////////////////////
            MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client cl = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
            cl.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(
                TimbratureManager.GetParametri<string>("AccountUtenteServizio")[0],
               TimbratureManager.GetParametri<string>("AccountUtenteServizio")[1]);

            MyRaiServiceInterface.MyRaiServiceReference1.getOrarioResponse responseOrario =
                cl.getOrario(giornata.orario.cod_orario, giornata.giornata.data.ToString("ddMMyyyy"), matricola, "BU", 75);

            //Inizializza oggetto principale/////////////////////////////////////

            CarenzaTimbrature G = new CarenzaTimbrature()
            {
                Intervalli = new List<IntervalloTimb>(),
                Data = giornata.giornata.data,
                ScontrinoMensaPresente = DipendenteFruitoMensa(matricola, giornata.giornata.data),
                RICOpresente = GiornataHaRICO(giornata),
                Giustificativi = new List<Giustificativo>()
            };
            var se = new digiGappEntities().L2D_SEDE_GAPP.Where(x => x.cod_sede_gapp == sedegapp)
                                 .FirstOrDefault();
            if (se != null && !String.IsNullOrWhiteSpace(se.minimo_car))
            {
                G.CarenzaBonus = Convert.ToInt32(se.minimo_car);
            }
            G.GiornataSupportaURH = (se != null && !String.IsNullOrWhiteSpace(se.periodo_mensa) && se.periodo_mensa != "0");


            //// CARENZA SU PRIMA TIMBRATURA //////////////////////////////////////////////
            int primaTimbraturaIngresso = calcolaMinuti(giornata.timbrature[0].entrata.orario);
            int ultimaTimbraturaUscita = calcolaMinuti(giornata.timbrature[giornata.timbrature.Length - 1].uscita.orario);
            if (primaTimbraturaIngresso > Convert.ToInt32(responseOrario.OrarioEntrataFinaleMin))
            {
                IntervalloTimb gt = new IntervalloTimb
                {
                    Start = responseOrario.OrarioEntrataIniziale,
                    StartMinuti = Convert.ToInt32(responseOrario.OrarioEntrataInizialeMin),
                    End = giornata.timbrature[0].entrata.orario,
                    EndMinuti = primaTimbraturaIngresso,
                    EndInsediamento = giornata.timbrature[0].entrata.insediamento,
                    EndDescrittivaInsediamento = giornata.timbrature[0].entrata.descrittivaInsediamento
                };

                gt.SetMensa(inizioMensaMin, fineMensaMin, inizioMensaSeraleMin, fineMensaSeraleMin);

                gt.InIngresso = true;
                gt.MinutiTotali = gt.EndMinuti - gt.StartMinuti;
                G.Intervalli.Add(gt);
            }

            //// CARENZA TIMBRATURE INTERMEDIE ///////////////////////////////////////////
            for (int i = 0; i < giornata.timbrature.Length - 1; i++)
            {
                var t1 = giornata.timbrature[i];
                var t2 = giornata.timbrature[i + 1];
                IntervalloTimb gt = new IntervalloTimb
                {
                    Start = t1.uscita.orario,
                    StartMinuti = calcolaMinuti(t1.uscita.orario),
                    StartInsediamento = t1.uscita.insediamento,
                    StartDescrittivaInsediamento = t1.uscita.descrittivaInsediamento,

                    End = t2.entrata.orario,
                    EndMinuti = calcolaMinuti(t2.entrata.orario),
                    EndInsediamento = t2.entrata.insediamento,
                    EndDescrittivaInsediamento = t2.entrata.descrittivaInsediamento
                };

                gt.SetMensa(inizioMensaMin, fineMensaMin, inizioMensaSeraleMin, fineMensaSeraleMin);

                gt.MinutiTotali = gt.EndMinuti - gt.StartMinuti;

                G.Intervalli.Add(gt);
            }

            //// CARENZA IN USCITA ///////////////////////////////////////////////////
            int MaxMinutiIngresso = Convert.ToInt32(responseOrario.OrarioEntrataFinaleMin);
            int? orarioUscitaMinuti = null;
            if (primaTimbraturaIngresso > MaxMinutiIngresso)
            {
                orarioUscitaMinuti = Convert.ToInt32(responseOrario.OrarioUscitaInizialeMin);
            }
            else
            {
                int diff = MaxMinutiIngresso - primaTimbraturaIngresso;
                int MinutiUscita = calcolaMinuti(responseOrario.OrarioUscitaFinale) - diff;
                if (MinutiUscita < Convert.ToInt32(responseOrario.OrarioUscitaInizialeMin))
                {
                    MinutiUscita = Convert.ToInt32(responseOrario.OrarioUscitaInizialeMin);
                }
                orarioUscitaMinuti = MinutiUscita;
            }
            if (ultimaTimbraturaUscita < orarioUscitaMinuti)
            {
                IntervalloTimb gt = new IntervalloTimb
                {
                    Start = giornata.timbrature[giornata.timbrature.Length - 1].uscita.orario,
                    StartMinuti = calcolaMinuti(giornata.timbrature[giornata.timbrature.Length - 1].uscita.orario),
                    StartInsediamento = giornata.timbrature[giornata.timbrature.Length - 1].uscita.insediamento,
                    StartDescrittivaInsediamento = giornata.timbrature[giornata.timbrature.Length - 1].uscita.descrittivaInsediamento,
                    EndMinuti = (int)orarioUscitaMinuti
                };
                gt.End = gt.EndMinuti.ToHHMM();

                gt.SetMensa(inizioMensaMin, fineMensaMin, inizioMensaSeraleMin, fineMensaSeraleMin);

                gt.MinutiTotali = gt.EndMinuti - gt.StartMinuti;
                gt.InUscita = true;

                G.Intervalli.Add(gt);
            }

            if (SpezzaConIntervalliMensa)
            {
                //// SPLIT A CAVALLO IN/OUT MENSA GIORNO ////////////////////////////////////////
                List<IntervalloTimb> L = new List<IntervalloTimb>();
                foreach (var item in G.Intervalli)
                {

                    //          |   M.....|......M
                    if (!item.StartInMensa_giorno && item.EndInMensa_giorno)
                    {
                        IntervalloTimb i1 = new IntervalloTimb();
                        Copy(i1, item);
                        // i1.End = inizioMensa;
                        i1.EndMinuti = inizioMensaMin;
                        i1.End = inizioMensaMin.ToHHMM();
                        i1.MinutiTotali = i1.EndMinuti - i1.StartMinuti;
                        i1.EndInMensa = false;
                        i1.StartInMensa = false;
                        L.Add(i1);

                        IntervalloTimb i2 = new IntervalloTimb();
                        Copy(i2, item);
                        // i2.Start = inizioMensa;
                        i2.StartMinuti = inizioMensaMin;
                        i2.Start = inizioMensaMin.ToHHMM();
                        i2.MinutiTotali = i2.EndMinuti - i2.StartMinuti;
                        i2.EndInMensa = true;
                        i2.StartInMensa = true;
                        L.Add(i2);
                    }
                    //         M.........|......M         |
                    else if (item.StartInMensa_giorno && !item.EndInMensa_giorno)
                    {
                        IntervalloTimb i1 = new IntervalloTimb();
                        Copy(i1, item);
                        // i1.End = fineMensa;
                        i1.EndMinuti = fineMensaMin;
                        i1.End = fineMensaMin.ToHHMM();
                        i1.MinutiTotali = i1.EndMinuti - i1.StartMinuti;
                        i1.StartInMensa = true;
                        i1.EndInMensa = true;
                        L.Add(i1);

                        IntervalloTimb i2 = new IntervalloTimb();
                        Copy(i2, item);
                        //  i2.Start = fineMensa;
                        i2.StartMinuti = fineMensaMin;
                        i2.Start = fineMensaMin.ToHHMM();
                        i2.MinutiTotali = i2.EndMinuti - i2.StartMinuti;
                        i2.EndInMensa = false;
                        i2.StartInMensa = false;
                        L.Add(i2);
                    }
                    else L.Add(item);
                }
                G.Intervalli = L;


                //// SPLIT A CAVALLO IN/OUT MENSA SERA ////////////////////////////////////////
                L = new List<IntervalloTimb>();
                foreach (var item in G.Intervalli)
                {

                    //          |   M.....|......M
                    if (!item.StartInMensa_serale && item.EndInMensa_serale)
                    {
                        IntervalloTimb i1 = new IntervalloTimb();
                        Copy(i1, item);
                        i1.End = inizioMensaSerale;
                        i1.EndMinuti = inizioMensaSeraleMin;
                        i1.MinutiTotali = i1.EndMinuti - i1.StartMinuti;
                        i1.EndInMensa = false;
                        i1.StartInMensa = false;
                        L.Add(i1);

                        IntervalloTimb i2 = new IntervalloTimb();
                        Copy(i2, item);
                        i2.Start = inizioMensaSerale;
                        i2.StartMinuti = inizioMensaSeraleMin;
                        i2.MinutiTotali = i2.EndMinuti - i2.StartMinuti;
                        i2.EndInMensa = true;
                        i2.StartInMensa = true;
                        L.Add(i2);
                    }
                    //         M.........|......M         |
                    else if (item.StartInMensa_serale && !item.EndInMensa_serale)
                    {
                        IntervalloTimb i1 = new IntervalloTimb();
                        Copy(i1, item);
                        i1.End = fineMensaSeraleMin.ToHHMM();
                        i1.EndMinuti = fineMensaSeraleMin;
                        i1.MinutiTotali = i1.EndMinuti - i1.StartMinuti;
                        i1.StartInMensa = true;
                        i1.EndInMensa = true;
                        L.Add(i1);

                        IntervalloTimb i2 = new IntervalloTimb();
                        Copy(i2, item);
                        i2.Start = fineMensaSeraleMin.ToHHMM();
                        i2.StartMinuti = fineMensaSeraleMin;
                        i2.MinutiTotali = i2.EndMinuti - i2.StartMinuti;
                        i2.EndInMensa = false;
                        i2.StartInMensa = false;
                        L.Add(i2);
                    }
                    else L.Add(item);
                }
                G.Intervalli = L;

                //// SPLIT A 35 MIN SE NON HAI RICO E NON HAI SCONTRINO MENSA
                G.GiornataSupportaUMH = !G.RICOpresente && !G.ScontrinoMensaPresente && !G.GiornataSupportaURH;
                if (G.GiornataSupportaUMH)
                {
                    G.Intervalli = SplitIntervalloMensaMax35Min(G.Intervalli);
                }
            }
            //// CERCA INTERVALLI GIA COPERTI //////////////////////////////////////////////
            G.Intervalli = GetIntervalliGiaCoperti(G, matricola);

            //// CERCA INTERVALLI SPLITTATI PRECEDENTEMENTI E COPERTI PARZIALMENTE


            Boolean SplitEseguito = true;
            while (SplitEseguito)
            {
                List<IntervalloTimb> Lafter = new List<IntervalloTimb>();
                SplitEseguito = false;
                foreach (var inter in G.Intervalli)
                {
                    List<IntervalloTimb> Lsost = GetIntervalliCompresi(inter, G.Data, matricola);
                    if (Lsost.Count == 0)
                        Lafter.Add(inter);
                    else
                    {
                        Lafter.AddRange(Lsost);
                        SplitEseguito = true;
                    }
                }
                G.Intervalli = Lafter;
            }



            if (G.Intervalli.Count > 0 && SpezzaConIntervalliMensa)
            {
                //CONTROLLA INTERVALLI IN MENSA
                G.MinutiCarenzaIntervalloMensa = G.Intervalli.Where(x => x.StartInMensa).ToList().Sum(x => x.MinutiTotali);
                G.MinutiCarenzaFuoriMensa = G.Intervalli.Where(x => !x.StartInMensa).ToList().Sum(x => x.MinutiTotali);

                if (G.ScontrinoMensaPresente)
                {
                    G.MinutiCarenzaIntervalloMensa -= G.CarenzaBonus;
                }
                else if (G.GiornataSupportaUMH && giornata.eccezioni.Any(x => x.cod.Trim() == "UMH"))
                {
                    var umh = giornata.eccezioni.Where(x => x.cod.Trim() == "UMH").FirstOrDefault();
                    if (umh != null && !String.IsNullOrWhiteSpace(umh.qta))
                    {
                        G.MinutiCarenzaIntervalloMensa -= calcolaMinuti(umh.qta);
                    }
                }
                else if (G.RICOpresente && giornata.eccezioni.Any(x => x.cod.Trim() == "URH"))
                {
                    var urh = giornata.eccezioni.Where(x => x.cod.Trim() == "URH").FirstOrDefault();
                    if (urh != null && !String.IsNullOrWhiteSpace(urh.qta))
                    {
                        G.MinutiCarenzaIntervalloMensa -= calcolaMinuti(urh.qta);
                    }
                }


                //CONTROLLA GIUSTIFICATIVI - Eccezioni flag_eccezione 0
                if (giornata.eccezioni != null && giornata.eccezioni.Count() > 0)
                {
                    var EccezioniTipo0 = new digiGappEntities().L2D_ECCEZIONE.Where(x => x.flag_eccez == "0")
                        .Select(x => x.cod_eccezione).ToList();
                    foreach (var ecc in giornata.eccezioni)
                    {
                        if (ecc.cod.Trim() == "CAR") continue;
                        if (EccezioniTipo0.Contains(ecc.cod.Trim()))
                        {
                            G.Giustificativi.Add(new Giustificativo()
                            {
                                Codice = ecc.cod.Trim(),
                                Minuti = calcolaMinuti(ecc.qta)
                            });
                        }
                    }
                    if (G.Giustificativi.Count > 0)
                    {
                        G.MinutiCarenzaFuoriMensa -= G.Giustificativi.Sum(x => x.Minuti);
                    }
                }

                if (G.MinutiCarenzaIntervalloMensa < 0) G.MinutiCarenzaIntervalloMensa = 0;
                if (G.MinutiCarenzaFuoriMensa < 0) G.MinutiCarenzaFuoriMensa = 0;

                G.CarenzaMinuti = G.MinutiCarenzaIntervalloMensa + G.MinutiCarenzaFuoriMensa;
                if (G.CarenzaMinuti < 0) G.CarenzaMinuti = 0;
                G.Carenza = G.CarenzaMinuti.ToHHMM();
            }


            G.Intervalli.RemoveAll(x => x.StartMinuti < Convert.ToInt32(responseOrario.OrarioEntrataInizialeMin) && x.EndMinuti <= Convert.ToInt32(responseOrario.OrarioEntrataFinaleMin));

            IntervalloTimb interv = (G.Intervalli.Where(x => x.StartMinuti < Convert.ToInt32(responseOrario.OrarioEntrataInizialeMin) && x.EndMinuti > Convert.ToInt32(responseOrario.OrarioEntrataFinaleMin))).FirstOrDefault();
            if (interv != null)
            {
                interv.StartMinuti = Convert.ToInt32(responseOrario.OrarioEntrataInizialeMin);
                interv.Start = interv.StartMinuti.ToHHMM();
                interv.MinutiTotali = interv.EndMinuti - interv.StartMinuti;
            }



            //responseOrario.OrarioEntrataInizialeMin
            return G;
        }

        public static List<IntervalloTimb> GetIntervalliGiaCoperti(CarenzaTimbrature Car, string matricola)
        {
            var db = new myRaiData.digiGappEntities();

            foreach (IntervalloTimb inter in Car.Intervalli)
            {
                var cop = db.MyRai_CoperturaCarenze.Where(x => x.matricola == matricola && x.data_eccezione == Car.Data
                    && x.dalle.Trim() == inter.Start.Trim() && x.alle.Trim() == inter.End.Trim()).FirstOrDefault();
                if (cop != null)
                    inter.CopertaDa = cop.cod_eccezione;
            }
            return Car.Intervalli;
        }
        public static List<IntervalloTimb> GetIntervalliCompresi(IntervalloTimb inter, DateTime data, string matricola)
        {
            var db = new myRaiData.digiGappEntities();
            var coperture = db.MyRai_CoperturaCarenze.Where(x => x.matricola == matricola &&
                   x.data_eccezione == data).ToList();

            List<IntervalloTimb> L = new List<IntervalloTimb>();

            if (coperture.Count == 0) return L;


            //if (inter.Start != null && inter.Start.Contains(":") == false)
            //    inter.Start = inter.Start.Substring(0, 2) + ":" + inter.Start.Substring(2, 2);

            //if (inter.End != null && inter.End.Contains(":") == false)
            //    inter.End = inter.End.Substring(0, 2) + ":" + inter.End.Substring(2, 2);

            DateTime interStart = new DateTime(2018, 1, 1,
                   Convert.ToInt32(inter.Start.Split(':')[0]),
                   Convert.ToInt32(inter.Start.Split(':')[1]), 0);
            DateTime interEnd = new DateTime(2018, 1, 1,
                Convert.ToInt32(inter.End.Split(':')[0]),
                Convert.ToInt32(inter.End.Split(':')[1]), 0);

            Boolean Trovato = false;
            foreach (var copertura in coperture)
            {
                DateTime coperturaDalle = new DateTime(2018, 1, 1,
                    Convert.ToInt32(copertura.dalle.Split(':')[0]),
                    Convert.ToInt32(copertura.dalle.Split(':')[1]), 0);
                DateTime coperturaAlle = new DateTime(2018, 1, 1,
                    Convert.ToInt32(copertura.alle.Split(':')[0]),
                    Convert.ToInt32(copertura.alle.Split(':')[1]), 0);

                if (
                    (coperturaDalle == interStart && coperturaAlle < interEnd)
                    ||
                    (coperturaDalle > interStart && coperturaAlle == interEnd)
                    ||
                    (coperturaDalle > interStart && coperturaAlle < interEnd)
                    )
                {
                    IntervalloTimb i1 = new IntervalloTimb();
                    Copy(i1, inter);
                    i1.Start = interStart.ToString("HH:mm");
                    i1.StartMinuti = i1.Start.ToMinutes();
                    i1.End = coperturaDalle.ToString("HH:mm");
                    i1.EndMinuti = i1.End.ToMinutes();
                    i1.MinutiTotali = i1.EndMinuti - i1.StartMinuti;


                    IntervalloTimb i2 = new IntervalloTimb();
                    Copy(i2, inter);
                    i2.Start = coperturaDalle.ToString("HH:mm");
                    i2.StartMinuti = i2.Start.ToMinutes();
                    i2.End = coperturaAlle.ToString("HH:mm");
                    i2.EndMinuti = i2.End.ToMinutes();
                    i2.MinutiTotali = i2.EndMinuti - i2.StartMinuti;
                    i2.CopertaDa = copertura.cod_eccezione;


                    IntervalloTimb i3 = new IntervalloTimb();
                    Copy(i3, inter);
                    i3.Start = coperturaAlle.ToString("HH:mm");
                    i3.StartMinuti = i3.Start.ToMinutes();
                    i3.End = interEnd.ToString("HH:mm");
                    i3.EndMinuti = i3.End.ToMinutes();
                    i3.MinutiTotali = i3.EndMinuti - i3.StartMinuti;


                    if (i1.MinutiTotali > 0) L.Add(i1);
                    if (i2.MinutiTotali > 0) L.Add(i2);
                    if (i3.MinutiTotali > 0) L.Add(i3);

                    return L;
                }

            }
            return L;
        }

        public static List<IntervalloTimb> SplitIntervalloMensaMax35Min(List<IntervalloTimb> intervalli)
        {
            List<IntervalloTimb> Lout = new List<IntervalloTimb>();
            foreach (IntervalloTimb inter in intervalli)
            {
                if (inter.StartInMensa && inter.EndInMensa && inter.MinutiTotali > 35)
                {

                    IntervalloTimb i1 = new IntervalloTimb();
                    Copy(i1, inter);
                    i1.EndMinuti = i1.StartMinuti + 35;
                    i1.End = i1.EndMinuti.ToHHMM();
                    i1.MinutiTotali = i1.EndMinuti - i1.StartMinuti;
                    Lout.Add(i1);

                    IntervalloTimb i2 = new IntervalloTimb();
                    Copy(i2, inter);
                    i2.StartMinuti = i1.EndMinuti;
                    i2.Start = i2.StartMinuti.ToHHMM();
                    i2.MinutiTotali = i2.EndMinuti - i2.StartMinuti;
                    Lout.Add(i2);
                }
                else
                    Lout.Add(inter);
            }
            return Lout;
        }
        public static Boolean DipendenteFruitoMensa(string matricola, DateTime d)
        {
            var db = new digiGappEntities();
            DateTime d1 = d.AddDays(1);
            string badge = matricola.PadLeft(8, '0');
            var scontrino = db.MyRai_MensaXML.Where(x => x.TransactionDateTime >= d && x.TransactionDateTime < d1 &&
               x.Badge == badge).FirstOrDefault();
            return scontrino != null;
        }
        public static Boolean GiornataHaRICO(dayResponse giornata)
        {
            return giornata.eccezioni.Any(x => x.cod == "RICO");
        }

        public static Boolean DurataTroppoEstesa(string codiceEccezione, string dalle, string alle, string quantita, string tipodip, string matricola, string dataEccezione)
        {
            var db = new digiGappEntities();
            var ecc = db.L2D_ECCEZIONE.Where(x => x.cod_eccezione == codiceEccezione).FirstOrDefault();
            if (ecc == null)
                throw new Exception("Eccezione inesistente: " + codiceEccezione);
            if (codiceEccezione == "UME")
            {
                int durata = alle.ToMinutes() - dalle.ToMinutes();
                if (durata > 60)
                {
                    return true;
                }
            }


            if (codiceEccezione == "POH")
            {
                int limite = 119;

                try
                {
                    WSDigigapp service = new WSDigigapp();
                    service.Credentials = new System.Net.NetworkCredential(
                        myRaiCommonTasks.CommonTasks.GetParametri<string>(myRaiCommonTasks.CommonTasks.EnumParametriSistema.AccountUtenteServizio)[0],
                        myRaiCommonTasks.CommonTasks.GetParametri<string>(myRaiCommonTasks.CommonTasks.EnumParametriSistema.AccountUtenteServizio)[1]);

                    dayResponse gio = service.getEccezioni(matricola, dataEccezione.Replace("/", ""), "BU", 80);
                    int minutiPre = 0;
                    if (int.TryParse(gio.orario.prevista_presenza, out minutiPre))
                    {
                        if (minutiPre > 0)
                        {
                            limite = (minutiPre / 4) - 1;
                            var resp_pf = service.getPianoFerie(matricola, "0101" + dataEccezione.Split('/')[2], 80, tipodip);
                            if (resp_pf.dipendente.ferie.permessiGiornalistiRimanenti <= 0 &&
                                resp_pf.dipendente.ferie.permessiRimanenti <= 0 &&
                                resp_pf.dipendente.ferie.exFestivitaRimanenti <= 0)
                            {
                                limite = (minutiPre / 2) - 1;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    TimbratureLogger.LogErrori(new MyRai_LogErrori()
                    {
                        applicativo = "PORTALE",
                        data = DateTime.Now,
                        error_message = ex.ToString(),
                        matricola = matricola,
                        provenienza = "DurataTroppoEstesa"
                    }, matricola);
                }


                if (!String.IsNullOrWhiteSpace(quantita))
                {
                    int minQuantita = calcolaMinuti(quantita);
                    return minQuantita > limite;
                }
                if (!String.IsNullOrWhiteSpace(dalle) && !String.IsNullOrWhiteSpace(alle))
                {
                    int minQuantita = calcolaMinuti(alle) - calcolaMinuti(dalle);
                    return minQuantita > limite;
                }
            }

            if (ecc.unita_misura == "H")
            {
                if (!String.IsNullOrWhiteSpace(quantita))
                {
                    int minQuantita = calcolaMinuti(quantita);
                    return minQuantita > 1479; //vincenzo 30102018-da rivedere
                }
                if (!String.IsNullOrWhiteSpace(dalle) && !String.IsNullOrWhiteSpace(alle))
                {
                    int minQuantita = calcolaMinuti(alle) - calcolaMinuti(dalle);
                    return minQuantita > 1479; //vincenzo 30102018-da rivedere
                }
            }

            return false;
        }

        public static List<string> RimuoviPerchePresentiSuGAPP(dayResponse giornata)
        {

            StackTrace stackTrace = new StackTrace();           // get call stack
            StackFrame[] stackFrames = stackTrace.GetFrames();  // get method calls (frames)
            bool FromProp = stackFrames.Any(x => x.GetMethod().Name.ToLower().Contains("proposteauto"));


            var db = new digiGappEntities();
            List<string> listaEccDaRimuovere = new List<string>();
            try
            {
                if (giornata == null || giornata.eccezioni == null || giornata.eccezioni.Count() == 0)
                    return listaEccDaRimuovere;

                var conteggio = giornata.eccezioni.GroupBy(x => x.cod).Select(z => new { codice = z.Key, quantita = z.Count() });
                if (conteggio == null || conteggio.Count() == 0)
                    return listaEccDaRimuovere;

                foreach (var item in conteggio)
                {
                    if (!string.IsNullOrWhiteSpace(item.codice))
                    {
                        if (FromProp == true)
                        {
                            string eccFramm = myRaiCommonTasks.CommonTasks.GetParametro<string>(myRaiCommonTasks.CommonTasks.EnumParametriSistema.EccezioniFrag);
                            if (!String.IsNullOrWhiteSpace(eccFramm))
                            {
                                if (eccFramm.Split(',').Contains(item.codice))
                                {
                                    continue;
                                }
                            }
                        }
                        var eccAmmessa = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione.Trim() == item.codice.Trim()).FirstOrDefault();
                        if (eccAmmessa != null)
                        {
                            if (eccAmmessa.MaxPerGiorno == null) continue;
                            if (eccAmmessa.MaxPerGiorno <= item.quantita)
                                listaEccDaRimuovere.Add(item.codice.Trim().ToUpper());
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return listaEccDaRimuovere;
        }

        public static List<string> RimuoviPerchePresentiSuDB(DateTime data, string matr)
        {
            var db = new digiGappEntities();
            List<string> listaEccDaRimuovere = new List<string>();
            try
            {
                List<string> ListaEccDB = db.MyRai_Eccezioni_Richieste.Where(x =>
                                                      x.MyRai_Richieste.matricola_richiesta == matr &&
                                                      x.data_eccezione == data &&
                                                      (x.id_stato == 10 || x.id_stato == 20 || x.id_stato == 70) &&
                                                      x.MyRai_Richieste.MyRai_Eccezioni_Richieste.Where(a => a.azione == "C" && a.id_stato == 20).Count() == 0
                                                      )
                                                      .Select(x => x.cod_eccezione)
                                                      .ToList();

                var grouped = ListaEccDB.GroupBy(x => x).Select(z => new { codi = z.Key, quantita = z.Count() });
                foreach (var e in grouped)
                {
                    if (!string.IsNullOrWhiteSpace(e.codi))
                    {
                        var eccAmmessa = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione.Trim() == e.codi.Trim()).FirstOrDefault();
                        if (eccAmmessa != null)
                        {
                            if (eccAmmessa.MaxPerGiorno == null) continue;
                            if (eccAmmessa.MaxPerGiorno <= e.quantita) listaEccDaRimuovere.Add(e.codi.Trim().ToUpper());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return listaEccDaRimuovere;
        }

        public static int GiornataHaMaggiorPresenza(dayResponse giornata)
        {
            if (giornata == null ||
                giornata.giornata == null ||
                giornata.giornata.maggiorPresenza == null ||
                String.IsNullOrWhiteSpace(giornata.giornata.maggiorPresenza) ||
                giornata.giornata.maggiorPresenza.Trim() == "00:00")
            {
                return 0;
            }
            return calcolaMinuti(giornata.giornata.maggiorPresenza.Trim());
        }

        public static int? GiornataHaCarenza(dayResponse giornata, Boolean SottraiMinimo = true, string dataNoSlash = null, string matricola = null)
        {
            if (giornata.eccezioni == null || giornata.eccezioni.Count() == 0) return 0;

            Eccezione car = giornata.eccezioni.Where(x => x.cod != null && x.cod.Trim() == "CAR").FirstOrDefault();

            if (car == null) return 0;

            if (SottraiMinimo)
            {
                var db = new myRaiData.digiGappEntities();
                int minimoIntervallo = 0;
                string minimo = db.L2D_SEDE_GAPP.Where(x => x.cod_sede_gapp == giornata.giornata.sedeGapp).Select(x => x.minimo_car).FirstOrDefault();
                if (!String.IsNullOrWhiteSpace(minimo)) minimoIntervallo = Convert.ToInt32(minimo);

                int minutiCar = car.qta.ToMinutes() - minimoIntervallo;
                if (minutiCar < 0)
                    return 0;
                else
                    return minutiCar;
            }
            else
            {
                return car.qta.ToMinutes();
            }

        }
        public static Timbrature GetTimbraturaPiuVicina(dayResponse giornata, Eccezione ecc)
        {
            if (ecc == null || String.IsNullOrWhiteSpace(ecc.alle)) return null;
            Timbrature response = null;

            int minalle = (ecc.alle).ToMinutes();
            int differenza = 10000;

            foreach (Timbrature t in giornata.timbrature)
            {
                int min = (t.uscita.orario).ToMinutes();
                if (min >= minalle && min < differenza)
                {
                    differenza = min;
                    response = t;
                }
            }
            return response;
        }
        public static void Copy(object copyToObject, object copyFromObject)
        {
            foreach (PropertyInfo sourcePropertyInfo in copyFromObject.GetType().GetProperties())
            {
                PropertyInfo destPropertyInfo = copyToObject.GetType().GetProperty(sourcePropertyInfo.Name);

                if (destPropertyInfo != null)
                {
                    destPropertyInfo.SetValue(
                        copyToObject,
                        sourcePropertyInfo.GetValue(copyFromObject, null),
                        null);
                }
            }
        }


        public static List<Eccezione> GetProposteAutomaticheGiornalisti(string matricola, DateTime data,
            List<MyRai_Eccezioni_Ammesse> Epossibili, dayResponse resp, bool GiornalistaDelleReti)
        {

            var db = new digiGappEntities();
            var ListaEccGiornalista = Epossibili
                .Where(x => x.ProponiAutoTipiDip != null && x.ProponiAutoTipiDip.Contains("G"))
                .ToList();
            var l2dEccezioni = db.L2D_ECCEZIONE.Select(x => new { cod = x.cod_eccezione, u = x.unita_misura });

            List<Eccezione> Resp = new List<Eccezione>();
            if (resp.orario == null || resp.orario.uscita_iniziale == null) return Resp;
            int uscita = 0;
            int.TryParse(resp.orario.uscita_iniziale, out uscita);
            if (uscita == 0) return Resp;

            foreach (var ecc in ListaEccGiornalista)
            {
                var l2decc = l2dEccezioni.Where(x => x.cod == ecc.cod_eccezione).FirstOrDefault();
                if (l2decc == null || l2decc.u == "H") continue;

                var NewEcc = new Eccezione() { cod = ecc.cod_eccezione };
                NewEcc.dalle = "";
                NewEcc.alle = "";
                NewEcc.qta = "1";
                NewEcc.descrittiva_lunga = ecc.desc_eccezione;
                NewEcc.data = data.ToString("ddMMyyyy");
                Resp.Add(NewEcc);
                int? car = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione == ecc.cod_eccezione)
                    .Select(x => x.CaratteriMotivoRichiesta).FirstOrDefault();
                NewEcc.CaratteriObbligatoriNota = car == null ? 0 : (int)car;
                if (GiornalistaDelleReti)
                {
                    string eccGiornReti = myRaiCommonTasks.CommonTasks.GetParametro<string>(
                        myRaiCommonTasks.CommonTasks.EnumParametriSistema.GdelleRetiMotivoObbligatorio);
                    if (!String.IsNullOrWhiteSpace(eccGiornReti))
                    {
                        string[] eccMotivoObb = eccGiornReti.Split(',');
                        if (eccMotivoObb.Contains(NewEcc.cod))
                        {
                            NewEcc.CaratteriObbligatoriNota = 10;
                        }
                    }
                }
            }
            return Resp;
        }

        public static List<Eccezione> GeneraROH(dayResponse giornata, int MaxDurata)
        {
            List<Eccezione> LE = new List<Eccezione>();
            int minutiMP = giornata.giornata.maggiorPresenza.ToMinutes();
            if (minutiMP <= 0) return LE;
            string desc = new digiGappEntities().L2D_ECCEZIONE.Where(x => x.cod_eccezione == "ROH")
                                                .Select(x => x.desc_eccezione).FirstOrDefault();

            int strStart = 0;
            int strEnd = 0;

            var str = giornata.eccezioni.Where(x => x.cod.Trim() == "STR").FirstOrDefault();
            if (str != null)
            {
                strStart = str.dalle.ToMinutes();
                strEnd = str.alle.ToMinutes();
            }


            foreach (var t in giornata.timbrature.Reverse())
            {
                if (t.uscita.orario.ToMinutes() - t.entrata.orario.ToMinutes() >= minutiMP)
                {
                    int UscitaOrarioMinuti = t.uscita.orario.ToMinutes();

                    //if (str != null)
                    //{

                    //    int minutoEndROH = 0;
                    //    for (int m = UscitaOrarioMinuti; m >= t.entrata.orario.ToMinutes()+minutiMP; m--)
                    //    {
                    //        if (m <= strStart )
                    //        {
                    //            minutoEndROH = m;
                    //            UscitaOrarioMinuti = minutoEndROH;
                    //            break;
                    //        }
                    //    }
                    //    if (minutoEndROH == 0)
                    //        continue;
                    //}
                    Eccezione ec = new Eccezione()
                    {
                        cod = "ROH",
                        alle = UscitaOrarioMinuti.ToHHMM(),
                        data = giornata.giornata.data.ToString("ddMMyyyy"),
                        dalle = (UscitaOrarioMinuti - minutiMP).ToHHMM(),
                        qta = minutiMP.ToHHMM(),
                        descrittiva_lunga = desc
                    };
                    if (minutiMP > MaxDurata)
                    {
                        ec.qta = MaxDurata.ToHHMM();
                        ec.alle = (ec.dalle.ToMinutes() + MaxDurata).ToHHMM();
                    }
                    LE.Add(ec);
                    return LE;
                }
                else
                {
                    Eccezione ec = new Eccezione()
                    {
                        cod = "ROH",
                        alle = t.uscita.orario,
                        data = giornata.giornata.data.ToString("ddMMyyyy"),
                        dalle = t.entrata.orario,
                        qta = (t.uscita.orario.ToMinutes() - t.entrata.orario.ToMinutes()).ToHHMM(),
                        descrittiva_lunga = desc
                    };
                    LE.Add(ec);
                    minutiMP -= (t.uscita.orario.ToMinutes() - t.entrata.orario.ToMinutes());
                }
            }
            return LE;
        }

        public static Boolean IsTipoTinMNS(string tipoDipendente, dayResponse resp)
        {
            return tipoDipendente == "T" && resp.eccezioni != null && resp.eccezioni.Any(x => x.cod != null && (x.cod.Trim() == "MNS" || x.cod.Trim() == "MFS"));
        }
        public static proposteResponse ScalaCarenzaDaPa(proposteResponse propAuto, dayResponse g)
        {

            if (propAuto.eccezioni == null ||
                propAuto.eccezioni.Count() == 0 ||
                !propAuto.eccezioni.Any(x => x.unita_mis == "H") ||
                g.eccezioni == null ||
                g.eccezioni.Count() == 0 ||
                !g.eccezioni.Any(x => x.cod.Trim() == "CAR") ||
                !g.eccezioni.Any(x => x.cod == "MR" || x.cod == "MF" || x.cod == "MFS")

                )
                return propAuto;

            var eccCarenza = g.eccezioni.Where(x => x.cod.Trim() == "CAR").FirstOrDefault();
            if (String.IsNullOrWhiteSpace(eccCarenza.qta) || eccCarenza.qta.Trim() == "00:00" || eccCarenza.qta.Trim() == "0")
                return propAuto;

            List<Eccezione> ListaTemp = new List<Eccezione>();

            int carMinuti = eccCarenza.qta.ToMinutes();
            foreach (var ecc in propAuto.eccezioni)
            {
                if (ecc.unita_mis != "H")
                {
                    ListaTemp.Add(ecc);
                    continue;
                }
                int eccMinuti = ecc.qta.ToMinutes();
                if (eccMinuti > carMinuti)
                {
                    ecc.qta = (eccMinuti - carMinuti).ToHHMM();
                    ListaTemp.Add(ecc);
                }
            }
            propAuto.eccezioni = ListaTemp.ToArray();
            return propAuto;

        }

        public static Eccezione[] GetProposteAutomatiche(string matricola, DateTime data,
          Boolean QuadraturaGiornaliera, int minutiPOH, int minutiROH, Boolean UtenteTester, string[] SoppressePOH,
            string TipoDipendente, proposteResponse respPropAuto, dayResponse respG = null)
        {
            WSDigigapp service = new WSDigigapp()
            {
                Credentials = new System.Net.NetworkCredential(
                    GetParametri<string>("AccountUtenteServizio")[0],
                    GetParametri<string>("AccountUtenteServizio")[1])
            };


            //proposteResponse resp = service.getProposteAutomatiche(matricola, data.ToString("ddMMyyyy"), "BU", 70);
            //proposteResponse resp = service.getProposteAutomatiche(matricola, data.ToString("ddMMyyyy"), "BU", 70);

            if (respG == null)
                respG = service.getEccezioni(matricola, data.ToString("ddMMyyyy"), "BU", 70);

            bool TipoTinMNS = IsTipoTinMNS(TipoDipendente, respG);

            if (TipoTinMNS)
            {
                SoppressePOH = SoppressePOH.Where(x => x != "STRF").ToArray();
            }

            if (!QuadraturaGiornaliera) respPropAuto = ScalaCarenzaDaPa(respPropAuto, respG);

            if (UtenteTester)
            {
                TimbratureCore.TimbratureInfo Ti = new TimbratureCore.TimbratureInfo(matricola, data, respG);

                // proposta POH 16022018 ////////////////////////////////////////////////
                List<Eccezione> Lprop = new List<Eccezione>();

                if (QuadraturaGiornaliera && !respG.eccezioni.Any(x => x.cod != null && x.cod.Trim() == "POH"))
                {
                    List<Eccezione> Lpoh = TimbratureCore.TimbratureManager.GetProposteAutoPOH(matricola, data, Ti,
                        respG, QuadraturaGiornaliera);
                    if (Lpoh != null && Lpoh.Count > 0)
                    {
                        Lprop.AddRange(Lpoh);
                    }
                }

                // proposta UMH 16022018 ////////////////////////////////////////////////

                string sed = respG.giornata.sedeGapp;
                var db = new digiGappEntities();
                var sedeg = db.L2D_SEDE_GAPP.Where(x => x.cod_sede_gapp == sed).FirstOrDefault();
                string[] rangeMensa = null;
                string IntervalloMensa = null;
                if (sedeg != null) IntervalloMensa = sedeg.intervallo_variabile_mensa(data);

                if (sedeg != null && !String.IsNullOrEmpty(IntervalloMensa) && IntervalloMensa.Contains("/"))
                    rangeMensa = IntervalloMensa.Split('/');
                else
                    rangeMensa = GetParametri<string>("RangeMensa");

                List<Eccezione> Lumh = TimbratureCore.TimbratureManager.GetProposteAutoUMH(matricola, data, Ti,
                   rangeMensa,
                    QuadraturaGiornaliera
                    );
                if (Lumh != null && Lumh.Count > 0)
                {
                    Lprop.AddRange(Lumh);
                }






                List<Eccezione> Leccez = respPropAuto.eccezioni.ToList();

                //18042018 Vincenzo sostituire SEH con POH nelle proposte
                foreach (var item in Lprop)
                {
                    if (item.cod == "SEH" && QuadraturaGiornaliera)
                    {
                        item.cod = "POH";
                        item.descrittiva = new digiGappEntities().L2D_ECCEZIONE.Where(x => x.cod_eccezione.Trim() == "POH")
                            .Select(x => x.desc_eccezione).FirstOrDefault();
                    }
                }

                Leccez.AddRange(Lprop);


                respPropAuto.eccezioni = Leccez.ToArray();
                /////////////////////////////////////////////////////////////////////////



            }

            //se la giornata ha carenza togli tutti gli straordinari dalle proposte
            if (GiornataHaCarenza(respG) > 0)
            {
                List<Eccezione> L = respPropAuto.eccezioni.ToList();
                L.RemoveAll(x => SoppressePOH.Contains(x.cod.Trim().ToUpper()));
                return TimbratureCore.TimbratureManager.CheckEccezioniProposte(L.ToArray(), data, UtenteTester, TipoDipendente, respG);
            }



            if (QuadraturaGiornaliera && minutiPOH > minutiROH && !TipoTinMNS)
            {
                List<Eccezione> L = respPropAuto.eccezioni.ToList();
                L.RemoveAll(x => SoppressePOH.Contains(x.cod.Trim().ToUpper()) && x.cod.Trim().ToUpper() != "STR");

                List<Eccezione> SegmentiSTR = L.Where(x => x.cod.Trim().ToUpper() == "STR").ToList();

                List<Eccezione> DaInserire = new List<Eccezione>();
                int minDifettoPOH = minutiPOH - minutiROH;

                if (TipoDipendente == "F")
                {
                    if (minDifettoPOH > 0)
                    {
                        int minutiMP = GiornataHaMaggiorPresenza(respG);
                        if (!L.Any(x => x.cod == "ROH") &&
                            GiornataHaCarenza(respG) == 0 &&
                            !respG.eccezioni.Any(x => x.cod.Trim() == "ROH") &&
                            minutiMP > 0)
                        {
                            if (minutiMP > minDifettoPOH)
                                DaInserire = GeneraROH(respG, minDifettoPOH);
                            else
                                DaInserire = GeneraROH(respG, minutiMP);

                            L.AddRange(DaInserire);
                        }
                    }
                }
                else if (SegmentiSTR.Count == 0)
                {
                    if (!L.Any(x => x.cod == "ROH") &&
                         GiornataHaCarenza(respG) == 0 &&
                         GiornataHaMaggiorPresenza(respG) > 0 &&
                         GiornataHaMaggiorPresenza(respG) < 10)
                    {
                        DaInserire = GeneraROH(respG, minutiPOH - minutiROH);

                        L.AddRange(DaInserire);
                    }
                }
                else if (SegmentiSTR != null && SegmentiSTR.Count > 0)
                {
                    L.RemoveAll(x => x.cod.Trim().ToUpper() == "STR");

                    foreach (Eccezione eccezSTR in SegmentiSTR)
                    {
                        if (minDifettoPOH <= 0)
                        {
                            DaInserire.Add(eccezSTR);
                            continue;
                        }
                        int mindalle = eccezSTR.dalle.ToMinutes();
                        int minalle = eccezSTR.alle.ToMinutes();
                        int minPropostiStr = minalle - mindalle;

                        if (minDifettoPOH < minPropostiStr)
                        {
                            //se lo straordinario copre ed è maggiore del poh da recuperare
                            var NewEccSTR = new Eccezione();
                            Copy(NewEccSTR, eccezSTR);

                            minPropostiStr = minDifettoPOH;
                            eccezSTR.alle = (mindalle + minPropostiStr).ToHHMM();
                            eccezSTR.qta = (minPropostiStr).ToHHMM();

                            int DiffMinutiPerNewSTR = (minalle - mindalle) - minDifettoPOH;
                            if (DiffMinutiPerNewSTR >= 10)
                            {
                                DiffMinutiPerNewSTR = 5 * (DiffMinutiPerNewSTR / 5);
                                NewEccSTR.dalle = eccezSTR.alle;
                                NewEccSTR.alle = ((NewEccSTR.dalle.ToMinutes()) + DiffMinutiPerNewSTR).ToHHMM();
                                NewEccSTR.qta = (DiffMinutiPerNewSTR).ToHHMM();
                                if (PossibilePiu5(NewEccSTR, respG))//se ci sono altri 5 min dispo 
                                {
                                    DiffMinutiPerNewSTR += 5;
                                    NewEccSTR.alle = ((NewEccSTR.dalle.ToMinutes()) + DiffMinutiPerNewSTR).ToHHMM();
                                    NewEccSTR.qta = (DiffMinutiPerNewSTR).ToHHMM();
                                }
                                DaInserire.Add(NewEccSTR);
                            }
                            eccezSTR.cod = "ROH";
                            eccezSTR.descrittiva_lunga = new digiGappEntities().MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione.Trim().ToUpper() == "ROH").Select(d => d.desc_eccezione).FirstOrDefault();
                            DaInserire.Add(eccezSTR);
                            minDifettoPOH = 0;
                        }
                        else
                        {
                            //se tutto lo staordinario serve a coprire poh da recuperare

                            Timbrature T = GetTimbraturaPiuVicina(respG, eccezSTR);
                            int minutiUltimaTimbratura = (T.uscita.orario.ToMinutes());

                            if (minutiUltimaTimbratura > minalle)
                            {
                                int diffMinuti = 0;
                                minalle = minutiUltimaTimbratura;
                                if ((mindalle + minDifettoPOH) > minalle)
                                {
                                    eccezSTR.alle = (minalle).ToHHMM();
                                    diffMinuti = minalle - mindalle;
                                }
                                else
                                {
                                    eccezSTR.alle = (mindalle + minDifettoPOH).ToHHMM();
                                    diffMinuti = minDifettoPOH;
                                }
                                eccezSTR.qta = (diffMinuti).ToHHMM();
                            }

                            eccezSTR.cod = "ROH";
                            eccezSTR.descrittiva_lunga = new digiGappEntities().MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione.Trim().ToUpper() == "ROH").Select(d => d.desc_eccezione).FirstOrDefault();
                            DaInserire.Add(eccezSTR);
                            minDifettoPOH -= (eccezSTR.qta.ToMinutes());
                        }
                    }
                    L.AddRange(DaInserire);
                }
                return TimbratureCore.TimbratureManager.CheckEccezioniProposte(L.ToArray(), data, UtenteTester, TipoDipendente, respG);
            }


            return TimbratureCore.TimbratureManager.CheckEccezioniProposte(respPropAuto.eccezioni, data, UtenteTester, TipoDipendente, respG);
        }
        public static Boolean PossibilePiu5(Eccezione ecc, dayResponse resp)
        {
            return (CadeInServizio(resp, ecc.alle.ToMinutes() + 5));

            //if (resp.timbrature == null || resp.timbrature.Count() == 0)
            //    return false;

            //var UltimaTimb = resp.timbrature.LastOrDefault();
            //if (UltimaTimb.uscita == null)
            //    return false;

            //int minutiUscita = UltimaTimb.uscita.orario.ToMinutes();
            //return (ecc.alle.ToMinutes() + 5 <= minutiUscita);

        }

        public static List<Eccezione> SetCaratteriObbligatori(List<Eccezione> eccez)
        {
            foreach (var item in eccez)
            {
                int? car = new digiGappEntities().MyRai_Eccezioni_Ammesse
                                                .Where(x => x.cod_eccezione.Trim().ToUpper() == item.cod.Trim().ToUpper())
                                                .Select(x => x.CaratteriMotivoRichiesta).FirstOrDefault();
                item.CaratteriObbligatoriNota = car == null ? 0 : (int)car;
            }
            return eccez;
        }
        public static Eccezione[] GetSTRperMNS(string TipoDipendente, dayResponse respG, DateTime data, Eccezione[] eccProp)
        {
            bool TipoTinMNS = TipoDipendente == "T" && respG.eccezioni.Any(x => x.cod.Trim() == "MNS") && !respG.eccezioni.Any(x => x.cod.Trim() == "STRF");
            if (TipoTinMNS)
            {
                if (respG.timbrature != null)
                {
                    var t = respG.timbrature.LastOrDefault();
                    if (t != null && t.uscita != null && t.uscita.orario != null)
                    {
                        var minutesSTR = t.uscita.orario.ToMinutes() - respG.orario.hhmm_entrata_48.ToMinutes();
                        minutesSTR -= respG.orario.intervallo_mensa.ToMinutes();

                        if (minutesSTR < 240) minutesSTR = 240;
                        var EccezioneSTR = new Eccezione()
                        {
                            cod = "STRF",
                            descrittiva_lunga = new digiGappEntities().MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione.Trim().ToUpper() == "STR").Select(d => d.desc_eccezione).FirstOrDefault(),
                            dalle = respG.orario.hhmm_entrata_48,
                            alle = (respG.orario.hhmm_entrata_48.ToMinutes() + minutesSTR).ToHHMM(),
                            qta = minutesSTR.ToHHMM(),
                            data = data.ToString("ddMMyyyy")
                        };
                        if (eccProp == null) eccProp = new List<Eccezione>().ToArray();

                        List<Eccezione> L = eccProp.ToList();
                        L.RemoveAll(x => x.cod.Trim() == "STRF");
                        L.Add(EccezioneSTR);
                        eccProp = L.ToArray();
                    }
                }
            }
            return eccProp;
        }
        public static Eccezione[] CheckEccezioniProposte(Eccezione[] eccProp, DateTime data, Boolean UtenteTester, string TipoDipendente, dayResponse respG)
        {

            //eccProp = GetSTRperMNS(TipoDipendente, respG, data, eccProp);

            if (eccProp == null || eccProp.Length == 0 || data == null) return eccProp;

            List<Eccezione> Le = eccProp.ToList();
            Le = SetCaratteriObbligatori(Le);

            if (!UtenteTester) return Le.ToArray();


            if (Le.Where(x => x.cod.Trim() == "POH").Count() > 1)
            {
                int tot = 0;
                foreach (var ec in Le.Where(x => x.cod.Trim() == "POH"))
                {
                    tot += (ec.alle.ToMinutes() - ec.dalle.ToMinutes());
                }
                var EccezioneSomma = new Eccezione() { cod = "POH" };

                EccezioneSomma.qta = tot.ToHHMM();
                EccezioneSomma.descrittiva_lunga = new digiGappEntities().MyRai_Eccezioni_Ammesse
                                                .Where(x => x.cod_eccezione.Trim().ToUpper() == "POH")
                                                .Select(d => d.desc_eccezione).FirstOrDefault();
                EccezioneSomma.data = data.ToString("ddMMyyyy");
                Le.RemoveAll(x => x.cod.Trim() == "POH");
                Le.Add(EccezioneSomma);
            }
            //Le = SetCaratteriObbligatori(Le);
            return Le.ToArray();
        }

        public static List<Eccezione> GetProposteAutoPOH(string matricola, DateTime data,
            TimbratureCore.TimbratureInfo T, dayResponse giornata, Boolean QuadraturaGiornaliera)
        {
            List<Eccezione> Le = new List<Eccezione>();
            try
            {
                var res = T.GetGiornataInfo(data, matricola);
                if (res != null && res.IngressoInRitardo == true)
                {
                    int MinRitardo = res.OrarioIngressoEffettuatoMinuti - res.OrarioBaseMinimoIngresso.ToMinutes();

                    if (MinRitardo >= res.MinutiQuartoDiGiornata && res.PRQdisponibile
                       && giornata.eccezioni != null && !giornata.eccezioni.Any(x => x.cod.Trim() == "PRQ"))
                    {
                        var NewEccPRQ = new Eccezione() { cod = "PRQ" };
                        NewEccPRQ.dalle = res.OrarioBaseMinimoIngresso;
                        NewEccPRQ.descrittiva_lunga = T.LEccAmmesse.Where(x => x.cod_eccezione.Trim().ToUpper() == "PRQ").Select(d => d.desc_eccezione).FirstOrDefault();
                        NewEccPRQ.data = data.ToString("ddMMyyyy");
                        Le.Add(NewEccPRQ);
                        //sposta inizio POH considerando il PRQ
                        res.OrarioBaseMinimoIngresso = (res.OrarioBaseMinimoIngresso.ToMinutes() + res.MinutiQuartoDiGiornata).ToHHMM();
                    }
                    if (QuadraturaGiornaliera &&
                        res.OrarioIngressoEffettuatoMinuti - res.OrarioBaseMinimoIngresso.ToMinutes() > 0 &&
                        giornata.eccezioni != null && !giornata.eccezioni.Any(x => x.cod.Trim() == "POH")
                        )
                    {
                        var NewEccPOHingresso = new Eccezione() { cod = "POH" };
                        NewEccPOHingresso.dalle = res.OrarioBaseMinimoIngresso;
                        NewEccPOHingresso.alle = res.OrarioIngressoEffettuato;
                        NewEccPOHingresso.qta = (res.OrarioIngressoEffettuatoMinuti - res.OrarioBaseMinimoIngresso.ToMinutes()).ToHHMM();
                        NewEccPOHingresso.descrittiva_lunga = T.LEccAmmesse.Where(x => x.cod_eccezione.Trim().ToUpper() == "POH").Select(d => d.desc_eccezione).FirstOrDefault();
                        NewEccPOHingresso.data = data.ToString("ddMMyyyy");
                        Le.Add(NewEccPOHingresso);
                    }
                }
                if (res != null && res.UscitaAnticipata == true && QuadraturaGiornaliera &&
                    giornata.eccezioni != null && !giornata.eccezioni.Any(x => x.cod.Trim() == "POH")
                    )
                {
                    var NewEccPOHuscita = new Eccezione() { cod = "POH" };
                    NewEccPOHuscita.dalle = res.OrarioUscitaEffettuato;
                    NewEccPOHuscita.alle = res.OrarioUscitaDovuto;
                    NewEccPOHuscita.qta = (res.OrarioUscitaDovutoMinuti - res.OrarioUscitaEffettuatoMinuti).ToHHMM();
                    NewEccPOHuscita.descrittiva_lunga = T.LEccAmmesse.Where(x => x.cod_eccezione.Trim().ToUpper() == "POH").Select(d => d.desc_eccezione).FirstOrDefault();
                    NewEccPOHuscita.data = data.ToString("ddMMyyyy");
                    Le.Add(NewEccPOHuscita);
                }
                return Le;
            }
            catch (Exception ex)
            {
                TimbratureLogger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = matricola,
                    provenienza = "TimbratureHelper.GetProposteAutoPOH"
                }, matricola);

                return Le;
            }
        }
        public static Eccezione getPohSehFromIntervallo(TimbratureCore.Intervallo intervallo, DateTime data)
        {
            string codice = (intervallo.RitornoStessaSede ? "POH" : "SEH");
            var NewEcc = new Eccezione() { cod = codice };
            NewEcc.dalle = intervallo.Inizio;
            NewEcc.alle = intervallo.Fine;
            NewEcc.qta = intervallo.TotaleMinuti.ToHHMM();
            NewEcc.descrittiva_lunga = new digiGappEntities().MyRai_Eccezioni_Ammesse
                .Where(x => x.cod_eccezione.Trim().ToUpper() == codice).Select(d => d.desc_eccezione).FirstOrDefault();
            NewEcc.data = data.ToString("ddMMyyyy");
            return NewEcc;
        }
        public static List<Eccezione> GetProposteAutoUMH(string matricola, DateTime data, TimbratureCore.TimbratureInfo T,
            string[] orarioMensa, Boolean QuadraturaGiornaliera)
        {
            List<Eccezione> Le = new List<Eccezione>();

            var res = T.GetCarenzeInfo(data, matricola, orarioMensa[0], orarioMensa[1]);

            if (res.Intervalli == null) return Le;

            foreach (var intervallo in res.Intervalli)
            {
                if (!intervallo.InizioDurantePausaMensa)//fuori da periodo mensa 1200/1500
                {
                    Eccezione NewEcc = getPohSehFromIntervallo(intervallo, data);
                    Le.Add(NewEcc);
                }
                else // intervallo entro periodo mensa
                {
                    if (res.MinutiIntervalloMensa == 0) // non hai diritto a mensa
                    {
                        Eccezione NewEcc = getPohSehFromIntervallo(intervallo, data);
                        Le.Add(NewEcc);
                    }
                    else //hai diritto a mensa
                    {
                        // se non sei andato a mensa
                        if (res.FruitoMensa == null)
                        {
                            var NewEccUMH = new Eccezione() { cod = "UMH" };
                            NewEccUMH.dalle = intervallo.Inizio;
                            NewEccUMH.alle = intervallo.Fine;
                            NewEccUMH.qta = intervallo.TotaleMinuti.ToHHMM();
                            NewEccUMH.descrittiva_lunga = T.LEccAmmesse.Where(x =>
                                x.cod_eccezione.Trim().ToUpper() == "UMH").Select(d => d.desc_eccezione).FirstOrDefault();
                            NewEccUMH.data = data.ToString("ddMMyyyy");
                            Le.Add(NewEccUMH);
                        }
                        //sei andato a mensa
                        else
                        {
                            if (intervallo.TotaleMinuti > res.MinutiBonusMensa)
                            {
                                Eccezione NewEcc = getPohSehFromIntervallo(intervallo, data);
                                Le.Add(NewEcc);
                            }
                        }
                    }
                }
            }
            // elimina SEH POH UMH se nella giornata gia ci sono:
            foreach (string codice in "SEH,POH,UMH".Split(','))
            {
                if (T.giornata.eccezioni.Any(x => x.cod.Trim() == codice))
                {
                    Le.RemoveAll(x => x.cod.Trim() == codice);
                }
            }

            // raggruppa come durata eventuali SEH POH multipli
            foreach (string codice in "SEH,POH".Split(','))
            {
                if (Le.Where(x => x.cod.Trim() == codice).Count() > 1)
                {
                    int tot = 0;
                    foreach (var ec in Le.Where(x => x.cod.Trim() == codice))
                    {
                        tot += (ec.alle.ToMinutes() - ec.dalle.ToMinutes());
                    }
                    var EccezioneSomma = new Eccezione() { cod = codice };

                    EccezioneSomma.qta = tot.ToHHMM();
                    EccezioneSomma.descrittiva_lunga = T.LEccAmmesse.Where(x => x.cod_eccezione.Trim().ToUpper() == codice)
                        .Select(d => d.desc_eccezione).FirstOrDefault();
                    EccezioneSomma.data = data.ToString("ddMMyyyy");
                    Le.RemoveAll(x => x.cod.Trim() == codice);
                    Le.Add(EccezioneSomma);
                }
            }

            //se ci sono UMH multipli, considera solo quello di durata maggiore:
            if (Le.Where(x => x.cod.Trim() == "UMH").Count() > 1)
            {
                var ListaUmh = Le.Where(x => x.cod.Trim() == "UMH").OrderByDescending(x => x.qta.ToMinutes()).ToList();
                ListaUmh.RemoveRange(1, ListaUmh.Count() - 1);
                Le.RemoveAll(x => x.cod.Trim() == "UMH");
                Le.AddRange(ListaUmh);
            }

            // se UMH > 35 minuti, imposta la durata a 35 minuti , piu un POH/SEH con la rimanenza
            var umh = Le.Where(x => x.cod.Trim() == "UMH").FirstOrDefault();
            int IntervalloMensa = T.GetMinutiIntervalloMensa();
            if (umh != null && umh.qta.ToMinutes() > IntervalloMensa)
            {
                umh.qta = IntervalloMensa.ToHHMM();
                var inter = res.Intervalli.Where(x => x.Inizio == umh.dalle).FirstOrDefault();
                inter.Inizio = (inter.InizioMin + IntervalloMensa).ToHHMM();
                inter.TotaleMinuti = inter.Fine.ToMinutes() - inter.Inizio.ToMinutes();
                Eccezione NewEcc = getPohSehFromIntervallo(inter, data);
                Le.Add(NewEcc);
            }

            if (!QuadraturaGiornaliera) Le.RemoveAll(x => x.cod.Trim() == "POH");
            return Le;
        }

        public static bool ApplicabileUMH(string matricola)
        {
            return ApplicabilePDCOoPDRA(matricola, "UMH");
        }
        public static bool ApplicabilePDCOoPDRA(string matricola, string ecc)
        {
            ecc = ecc.ToUpper();
            string[] par = GetParametri<string>(ecc + "UltimoPeriodo");
            if (par == null || par.Length < 2 || String.IsNullOrWhiteSpace(par[0]) || String.IsNullOrWhiteSpace(par[1])) return false;
            var db = new digiGappEntities();
            int daysPeriodo = Convert.ToInt32(par[0]);
            int richieste = Convert.ToInt32(par[1]);
            DateTime D = DateTime.Today.AddDays(-daysPeriodo);

            var q = db.MyRai_Eccezioni_Richieste.Where(x => x.MyRai_Richieste.matricola_richiesta == matricola && x.cod_eccezione == ecc && x.azione == "I" && x.data_eccezione >= D).Count();
            return q >= richieste;
        }

        public static T[] GetParametri<T>(string chiave)
        {
            var db = new digiGappEntities();

            MyRai_ParametriSistema p = db.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == chiave);
            if (p == null) return null;
            else
            {
                T[] parametri = new T[] { (T)Convert.ChangeType(p.Valore1, typeof(T)),
                                              (T)Convert.ChangeType(p.Valore2, typeof(T)) };
                return parametri;
            }

        }
        public static int calcolaMinuti(string orarioHHMM)
        {
            int minuti = 0;
            if (orarioHHMM == null || orarioHHMM.Trim() == "" || orarioHHMM.Trim().Length < 4) return minuti;
            if (orarioHHMM.Contains("<") || orarioHHMM.Contains(">")) return minuti;

            string[] array = new string[2];

            if (orarioHHMM.IndexOf(':') > 0)
                array = orarioHHMM.Split(':');
            else
            {
                array[0] = orarioHHMM.Substring(0, 2);
                array[1] = orarioHHMM.Substring(2, 2);
            }

            minuti = Int32.Parse(array[0]) * 60 + Int32.Parse(array[1]);

            return minuti;
        }

        public static int GetMinutiPrimaInizioTurno(dayResponse resp)
        {
            int diff = 0;
            if (resp.timbrature != null && resp.timbrature.Any())
            {
                var t = resp.timbrature.First();
                if (t.entrata != null && t.entrata.orario != null)
                {
                    int minutiTimbratura = t.entrata.orario.ToMinutes();
                    int minutiOrario = resp.orario.hhmm_entrata_48.ToMinutes();
                    if (minutiOrario == 0 && minutiTimbratura > 21 * 60) minutiOrario = 24 * 60;
                    diff = minutiOrario - minutiTimbratura;
                    if (diff < 0) diff = 0;
                }
            }
            return diff;
        }
    }

    public class TimbratureLogger
    {
        public static Boolean LogAzione(MyRai_LogAzioni azione, string matricola)
        {
            using (var db = new digiGappEntities())
            {
                azione.data = DateTime.Now;
                azione.applicativo = "Portale";
                azione.matricola = matricola;
                db.MyRai_LogAzioni.Add(azione);

                try
                {
                    db.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

        }

        public static Boolean LogErrori(MyRai_LogErrori errore, string matricola)
        {
            using (var db = new digiGappEntities())
            {
                errore.data = DateTime.Now;
                errore.applicativo = "Portale";
                errore.matricola = matricola;
                db.MyRai_LogErrori.Add(errore);

                try
                {
                    db.SaveChanges();
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

        }
    }


    public class CarenzaTimbrature
    {
        public DateTime Data { get; set; }
        public string Carenza { get; set; }
        public int CarenzaMinuti { get; set; }
        public Boolean ScontrinoMensaPresente { get; set; }
        public Boolean RICOpresente { get; set; }
        public Boolean GiornataSupportaUMH { get; set; }
        public int CarenzaBonus { get; set; }
        public int MinutiCarenzaIntervalloMensa { get; set; }
        public int MinutiCarenzaFuoriMensa { get; set; }

        public Boolean GiornataSupportaURH { get; set; }

        public List<IntervalloTimb> Intervalli { get; set; }
        public List<Giustificativo> Giustificativi { get; set; }
    }
    public class IntervalloTimb
    {
        public string Start { get; set; }
        public int StartMinuti { get; set; }
        public Boolean StartInMensa { get; set; }
        public Boolean StartInMensa_giorno { get; set; }
        public Boolean StartInMensa_serale { get; set; }
        public string StartInsediamento { get; set; }
        public string StartDescrittivaInsediamento { get; set; }

        public string End { get; set; }
        public int EndMinuti { get; set; }
        public Boolean EndInMensa { get; set; }
        public Boolean EndInMensa_giorno { get; set; }
        public Boolean EndInMensa_serale { get; set; }
        public string EndInsediamento { get; set; }
        public string EndDescrittivaInsediamento { get; set; }

        public Boolean InIngresso { get; set; }
        public Boolean InUscita { get; set; }
        public int MinutiTotali { get; set; }

        public int? Riferimento { get; set; }
        public string CopertaDa { get; set; }

        public void SetMensa(int inizioMensaMin, int fineMensaMin, int inizioMensaSeraleMin, int fineMensaSeraleMin)
        {
            this.StartInMensa_giorno = (this.StartMinuti >= inizioMensaMin && this.StartMinuti <= fineMensaMin);
            this.StartInMensa_serale = (this.StartMinuti >= inizioMensaSeraleMin && this.StartMinuti <= fineMensaSeraleMin);
            this.EndInMensa_giorno = (this.EndMinuti >= inizioMensaMin && this.EndMinuti <= fineMensaMin);
            this.EndInMensa_serale = (this.EndMinuti >= inizioMensaSeraleMin && this.EndMinuti <= fineMensaSeraleMin);
            this.StartInMensa = this.StartInMensa_giorno || this.StartInMensa_serale;
            this.EndInMensa = this.EndInMensa_giorno || this.EndInMensa_serale;
        }
    }
    public class Giustificativo
    {
        public string Codice { get; set; }
        public int Minuti { get; set; }
    }
}
