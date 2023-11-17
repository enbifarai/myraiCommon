using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using myRaiCommonTasks.it.rai.servizi.svildigigappws;
using myRaiCommonTasks.it.rai.servizi.HRGA;
using myRaiData;
using System.Data;
using System.Web.Script.Serialization;
using myRaiCommonTasks.sendMail;
using Newtonsoft.Json;
using System.Net;
using System.Collections.Specialized;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Net.Http;

namespace myRaiCommonTasks
{
    public class CommonTasks
    {
        public enum CachedMethods
        {
            DataChiusuraSede
        }
        public enum EnumStatiRichiesta
        {
            InseritoUfficioPersonale = 5,
            InProgressUfficioPersonale = 6,

            InseritoSegreteria = 7,
            InProgressSegreteria = 8,

            InApprovazione = 10,
            Approvata = 20,
            Stampata = 30,
            Convalidata = 40,
            Rifiutata = 50,
            Cancellata = 60,
            Eliminata = 70,

            TuttiGliStati = 0
        }
        public enum EnumTipoDiStampa
        {
            ro,
            ss,
            sc
        }
        public enum CancellazioneFerie
        {
            CancellazioneNormale,
            CancellazioneConRipianificazione,
            CancellazioneNonPossibile
        }
        public enum EnumParametriSistema
        {
            MaxRowsVisualizzabiliDaApprovare,
            AccountUtenteServizio,
            MatricolaSimulata,
            MailApprovazioneSubject,
            MailApprovazioneFrom,
            MailApprovazioneTemplate,
            MailRifiutaTemplate,
            UrlImmagineDipendente,
            OrariGapp,
            ValidazioneGenericaEccezioni,
            RowsPerScroll,
            LimiteMesiBackPerEvidenze,
            MessaggioAssenteIngiustificato,
            PosizioneNumDoc,
            TipiDipQuadraturaSettimanale,
            GestisciDateSuDocumenti,
            OreRichiesteUrgenti,
            SovrascritturaTipoDipendente,
            CodiceCSharp,
            IgnoraAssenzeIngiustificatePerMatricole,
            GiornoEsecuzioneBatchPDF,
            IntervalloBatchSecondi,
            LivelloUtenteListaEccezioni,
            AutorizzaMinimale,
            UtentePerConvalida,
            MailTemplateSollecitoDipendente,
            MailTemplatePDFinFirma,
            MailTemplateNotificheDipendente,
            NotificheRangeOre,
            MailTemplateNotificheBossL1,
            MailTemplateAvvisoDelega,
            MailTemplateLiv1SollecitoApprovazioni,
            RedirectEmailSuSviluppo,
            MailTemplateNotificheStorniBossL1,
            MailTemplateNotificheScaduteL1,
            MailTemplateNotificheUrgentiL1,
            MailTemplateLiv2SollecitoFirma,
            MailLiv1SollecitoApprovazioniRange,
            MailLiv2FirmaPDFRange,
            ForzaturaPDFperiodo,
            ForzaturaPDFsedi,
            MailTemplateReportGenPDF,
            GetCategoriaDatoNetCached,
            GetCategoriaDatoNetCachedL2,
            GetCategoriaDatoNetCachedNolevel,
            IntervalloUrgentiScadute,
            EsecuzioneInvioEmail,
            MailRichiesteSubject,
            MailTemplateUrgentiScadute,
            MailTemplateScadenzario,
            NotificaEliminataGapp,
            EstremiNotturno,
            SogliaNotturno,
            EliminaSeOltreSogliaNotturno,
            SediEffettivamenteGestiteSirio,
            MailApprovazionePianoFerie,
            MailTemplateSollecitoApprovazionePianoFerie,
            MailFirmaPianoFerie,
            MailTemplateSollecitoFirmaPianoFerie,
            MailTemplateNotaDallaSegreteriaFrom,
            MailTemplateNotificaRifornimento,
            MailTemplateNotificaRifornimentoFromTo,
            MailTemplateModuloDetassazione1C,
            MailTemplateModuloDetassazione2C,
            MailTemplateModuloDetassazioneFrom,
            ApprovaPresenzeDopoGG,
            UltimaEsecuzioneCacheResoconti_Creazione,
            UltimaEsecuzioneCacheResoconti_Refresh,
            AbilitaTaglioMaggCoda,
            SediRuoloAutomatico,
            EccezioniFrag,
            MailTemplateNotificaSmistamentoRichiesta,
            MailNotificaSmistamentoRichiesta,
            MailTemplateModuloBonus100,
            MailTemplateDefault,
            Regalate,
            DateRMTRVenezia,
            BatchCacheStatoFerie,
            TemplateMailProrogaSW,
            TemplateMailProrogaSW2,
            TemplateMailPrimaCollocazioneSW,
            TemplateMailCollocazioneRicollocazioneSW,
            TemplateMailRettificaAssegnazioneSW,
            TemplateMailProrogaSW3,
            TemplateMailCollocazioneRicollocazioneSW2,
            TemplateMailProrogaSWGennaio2021,
            TemplateMailCollocazioneRicollocazioneSWGennaio2021,
            TemplateMailProrogaSWFinoAMarzo2021,
            TemplateMailCollocazioneRicollocazioneSWFinoAMarzo2021,
            TemplateMailProrogaSWFinoAdAprile2021,
            TemplateMailCollocazioneRicollocazioneSWFinoAdAprile2021,
            GiorniRiaperturaDopoSecondoGiornoC,
            ChiusuraMese,
            UrlTeams,
            PianoStraordinarioIncentivazioneEsodoEsaurimentoFondi,
            TemplateMailProrogaSWFinoALuglio2021,
            TemplateMailCollocazioneRicollocazioneSWFinoALuglio2021,
            GdelleRetiMotivoObbligatorio,
            CodiciRecordRew,
            PathFileEvidenze,
            SimulazioniPolRetr_TREC,
            SimulazioniPolRetr_INDTE,
            AbilitaApi,
            GiorniAssenzaProlungata,
            API_SW_UrlToken
        }

        public enum enumPhotoFormat { Original = 1, Medium = 2, Small = 3 }


        public static bool API_SW_Abilitate(string matricolaUtente)
        {
            string[] par = GetParametri<string>(myRaiCommonTasks.CommonTasks.EnumParametriSistema.AbilitaApi);
            bool abi = (par!=null && par[0] == "true" && par[1] != null && (par[1].Contains(matricolaUtente) || par[1]=="*"));
            return abi;
        }

        public static DateTime? GetDateTimeWithOverflow(string data, string HHmm)
        {
            DateTime dt;
            if (!DateTime.TryParseExact(data, "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out dt))
            {
                if (!DateTime.TryParseExact(data, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dt))
                    return null;
            }
            if (String.IsNullOrWhiteSpace(HHmm)) return dt;

            HHmm = HHmm.Trim().Replace(":", "");
            if (HHmm.Length != 4) return dt;

            int h;
            if (!Int32.TryParse(HHmm.Substring(0, 2), out h)) return dt;

            int m;
            if (!Int32.TryParse(HHmm.Substring(2, 2), out m)) return dt;

            dt = dt.AddHours(h);
            dt = dt.AddMinutes(m);
            return dt;
        }

        public static string GetEmailPerMatricola(string matricola)
        {
            try
            {
                it.rai.servizi.hrgb.Service service = (it.rai.servizi.hrgb.Service)ServiceCreate(AsmxServices.HRGB);
                string r = service.EsponiAnagrafica("raicv;" + matricola + ";;e;0");
                return r.Split(';')[15];
            }
            catch (Exception ex)
            {
                LogErrore("Matricola:'" + matricola + "'\r\n " + ex.Message + " " + ex.StackTrace, "GetEmailPerMatricola");
                return null;
            }
        }

        public static Boolean IsDataFineMese(DateTime d)
        {
            return (d.AddDays(1).Month != d.Month);
        }

        //public static string Task_VerificaChiusureMese()
        //{
        //    string mese = DateTime.Now.AddMonths(-1).Month.ToString().PadLeft(2, '0');
        //    string anno = DateTime.Now.AddMonths(-1).Year.ToString();
        //    string[] utenteConv = GetParametri<string>(EnumParametriSistema.UtentePerConvalida);
        //    DateTime PrimoGiornoMeseCorrente = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        //    DateTime UltimoGiornoMeseScorso = new DateTime(
        //        PrimoGiornoMeseCorrente.AddDays(-1).Year,
        //        PrimoGiornoMeseCorrente.AddDays(-1).Month,
        //        PrimoGiornoMeseCorrente.AddDays(-1).Day);
        //    List<string> Lsedi;

        //    Log("Verifica data odierna con DataChiusura1");
        //    if (myRaiCommonTasks.CommonTasks.IsDataChiusura1(mese, anno, utenteConv[0], 75))
        //    {
        //        Log("Data coincidente");
        //        //genera PDF ultimo segmento mese precedente
        //        Lsedi = GetSediGappPDF();
        //        foreach (string sede in Lsedi)
        //        {
        //            Log("Verifica PDF necessari alla sede " + sede);
        //            List<PdfStartEnd> Lpdf = myRaiCommonTasks.CommonTasks.GetPDFneeded(sede);
        //            Log("PDF necessari: " + Lpdf.Count());
        //            if (Lpdf.Count > 0)
        //            {
        //                PdfStartEnd p = Lpdf.Where(x => x.DateEnd == UltimoGiornoMeseScorso).FirstOrDefault();
        //                if (p == null)
        //                {
        //                    Log("Periodo non trovato per sede " + sede + " data fine " + UltimoGiornoMeseScorso.ToString("dd/MM/yyyy"));
        //                    continue;
        //                }
        //                Log("Generazione PDF in corso per periodo con data fine " + UltimoGiornoMeseScorso.ToString("dd/MM/yyyy"));
        //                if (SedeProcessata(sede, p.DateStart, p.DateEnd))
        //                {
        //                    Log("PDF già esistente  " + sede + " " + p.DateStart.ToString("dd/MM/YYYY") + " " + p.DateEnd.ToString("dd/MM/yyyy"));
        //                    continue;
        //                }
        //                DateTime LastDayBefore = p.DateStart.AddDays(-1);

        //                //se ultimo giorno prima non è convalidato:
        //                Boolean? convalidato = VerificaGiornoConvalidato(LastDayBefore, utenteConv[0], utenteConv[1], sede);
        //                if (convalidato == null) continue;

        //                if (convalidato == false)
        //                {
        //                    Log("Ultimo giorno precedente NON convalidato " + LastDayBefore.ToString("dd/MM/yyyy"));

        //                    string Errore = ConvalidaNoStampa(utenteConv[0], utenteConv[1], LastDayBefore, LastDayBefore, sede);
        //                    if (!String.IsNullOrWhiteSpace(Errore))
        //                    {
        //                        Log("Errore durante convalida " + LastDayBefore.ToString("ddMMyyyy") + " - " + Errore);
        //                        continue;
        //                    }
        //                    Log("Convalida ok per " + LastDayBefore.ToString("dd/MM/yyyy"));
        //                }
        //                Log("Processo PDF");
        //                EsitoPDF esito = ProcessaSedeGapp(sede, p.DateStart, p.DateEnd);
        //                Log("ID pdf generato:" + esito.idPdf);

        //                if (esito.idPdf > 0) AggiungiNotificaSedeGapp(sede, p.DateStart, p.DateEnd, esito.idPdf);
        //            }
        //        }
        //    }
        //    //else Log("Data odierna non coincidente con DataChiusura1");


        //    Log("Verifica data odierna con DataChiusura2");
        //    if (myRaiCommonTasks.CommonTasks.IsDataChiusura2(mese, anno, utenteConv[0], 75))
        //    {
        //        Log("Data coincidente");

        //        //convalida ultimo giorno mese precedente
        //        Lsedi = GetSediGappPDF();
        //        foreach (string sede in Lsedi)
        //        {
        //            Boolean? convalidato = VerificaGiornoConvalidato(UltimoGiornoMeseScorso, utenteConv[0], utenteConv[1], sede);
        //            if (convalidato == null) continue;
        //            if (convalidato == false)
        //            {
        //                Log("Ultimo giorno del mese precedente NON convalidato " + UltimoGiornoMeseScorso.ToString("dd/MM/yyyy"));
        //                var db = new digiGappEntities();
        //                var PDF = db.DIGIRESP_Archivio_PDF.Where(x => x.sede_gapp == sede && x.tipologia_pdf == "R" && x.data_fine == UltimoGiornoMeseScorso).FirstOrDefault();
        //                if (PDF == null)
        //                {
        //                    Log("PDF con data fine " + UltimoGiornoMeseScorso.ToString("dd/MM/yyyy") + " non trovato per " + sede);
        //                    continue;
        //                }
        //                String Errore = "";
        //                if (PDF.stato_pdf == "C_OK")
        //                {
        //                    Errore = ConvalidaNoStampa(PDF.matricola_convalida.PadLeft(7, '0'), PDF.matricola_convalida.PadLeft(7, '0'), UltimoGiornoMeseScorso, UltimoGiornoMeseScorso, sede);
        //                }
        //                else
        //                {
        //                    Errore = ConvalidaNoStampa(utenteConv[0], utenteConv[1], UltimoGiornoMeseScorso, UltimoGiornoMeseScorso, sede);
        //                }
        //                if (!String.IsNullOrWhiteSpace(Errore))
        //                {
        //                    Log("Errore durante convalida " + UltimoGiornoMeseScorso.ToString("ddMMyyyy") + " - " + Errore);
        //                    continue;
        //                }
        //                Log("Convalida ok per " + UltimoGiornoMeseScorso.ToString("dd/MM/yyyy"));
        //            }
        //            else Log("Giorno già convalidato " + UltimoGiornoMeseScorso.ToString("dd/MM/yyyy"));
        //        }
        //    }
        //    else Log("Data odierna non coincidente con DataChiusura2");
        //    return "";
        //}

        public static void TeamsEnd(DateTime D1, DateTime D2)
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;
            string url = myRaiCommonTasks.CommonTasks.GetParametro<string>(myRaiCommonTasks.CommonTasks.EnumParametriSistema.UrlTeams);
            if (String.IsNullOrWhiteSpace(url))
                return;

            Avviso A = new Avviso()
            {
                Ambiente = "Produzione",
                Corpo = "La procedura di creazione del pdf relativo al periodo **" + D1.ToString("dd/MM") + " - " + D2.ToString("dd/MM") + "** è terminato.",
                Titolo = "Procedura creazione pdf"
            };

            string body = JsonConvert.SerializeObject(A);

            using (var client = new HttpClient())
            {
                var content = new StringContent(body);
                content.Headers.ContentType.CharSet = string.Empty;
                content.Headers.ContentType.MediaType = "application/json";
                //var content = new StringContent(body, Encoding.UTF8, "application/json");
                var response = client.PostAsync(url, content);
                Console.WriteLine(response.Result);
            }
        }
        public static void TeamsStart(DateTime D1, DateTime D2)
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)768 | (SecurityProtocolType)3072;

            string url = myRaiCommonTasks.CommonTasks.GetParametro<string>(myRaiCommonTasks.CommonTasks.EnumParametriSistema.UrlTeams);

            Avviso A = new Avviso()
            {
                Ambiente = "Produzione",
                Corpo = "La procedura di creazione del pdf relativo al periodo **" + D1.ToString("dd/MM") + " - " + D2.ToString("dd/MM") +
                "** è stato avviato alle ore **" + DateTime.Now.ToString("HH:mm") + "**. La procedura impiega mediamente **3 ore** per completare i pdf di tutte le sedi gapp.",
                Titolo = "Procedura creazione pdf"
            };
            string body = JsonConvert.SerializeObject(A);

            try
            {
                using (var client = new HttpClient())
                {
                    var content = new StringContent(body);
                    content.Headers.ContentType.CharSet = string.Empty;
                    content.Headers.ContentType.MediaType = "application/json";
                    //var content = new StringContent(body, Encoding.UTF8, "application/json");
                    var response = client.PostAsync(url, content);
                    Console.WriteLine(response.Result);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }


        }
        public static string Task_GeneraPdfEccezioni(string sedeG = null, DateTime? D1 = null, DateTime? D2 = null, Boolean AmmettiUltimoPDFdelMese = false, Boolean FromBatchGeneraPDF = false)
        {
            Log("Task_GeneraPdfEccezioni chiamato");
            string[] utenteConv = GetParametri<string>(EnumParametriSistema.UtentePerConvalida);

            //check sedi Gapp abilitate
            List<string> ListaSediGapp = new List<string>();
            if (sedeG == null)
                ListaSediGapp = GetSediGappPDF();
            else
                ListaSediGapp.Add(sedeG.ToUpper());


            if (ListaSediGapp == null || ListaSediGapp.Count == 0)
            {
                Log("Lista Sedi Gapp vuota");
                return "Lista sedi vuota";
            }
            else
            {
                Log("Esecuzione in corso per sedi:" + String.Join(",", ListaSediGapp.ToArray()) + "\r\n" + ListaSediGapp.Count().ToString() + " sedi");
                PostMessage(ListaSediGapp.Count() + " sedi da processare...");
            }


            string mailReport = null;
            int GeneratiOK = 0;
            List<string> sediErrore = new List<string>();

            int counter = 0;
            int tot = ListaSediGapp.Count();
            string last = null;
            Boolean TeamsSent = false;
            DateTime D1Teams = DateTime.Today;
            DateTime D2Teams = DateTime.Today;

            foreach (string sedeGapp in ListaSediGapp)
            {

                counter++;

                Boolean SedeInErrore = false;

                Log("\r\n\r\nProcesso sede " + sedeGapp + " - " + counter.ToString() + "/" + tot.ToString());

                List<PdfStartEnd> ListPDFneeded = new List<PdfStartEnd>();
                if (D1 == null || D2 == null)
                    ListPDFneeded = GetPDFneeded(sedeGapp);
                else
                {
                    PdfStartEnd p = new PdfStartEnd();
                    p.DateStart = (DateTime)D1;
                    p.DateEnd = (DateTime)D2;
                    ListPDFneeded.Add(p);
                }
                if (AmmettiUltimoPDFdelMese)
                {
                    DateTime firstOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                    ListPDFneeded.RemoveAll(x => x.DateStart == firstOfMonth);

                    DateTime lastOfPreviousMonth = firstOfMonth.AddDays(-1);
                    if (!ListPDFneeded.Any(x => x.DateEnd == lastOfPreviousMonth))
                    {
                        ListPDFneeded.Add(new PdfStartEnd()
                        {
                            DateEnd = lastOfPreviousMonth,
                            DateStart = GetlastMonday(lastOfPreviousMonth)
                        });
                    }
                }

                if (last == null && (D1 == null || D2 == null) && ListPDFneeded.Any())
                {
                    last = ListPDFneeded.OrderByDescending(x => x.DateEnd).First().DateStart.ToString("dd/MM/yyyy")
                        + "-" + ListPDFneeded.OrderByDescending(x => x.DateEnd).First().DateEnd.ToString("dd/MM/yyyy");
                }


                myRaiData.digiGappEntities db = new digiGappEntities();




                //DateTime d1 = new DateTime(2019, 6, 24);
                //DateTime d2 = new DateTime(2019, 6, 30);

                //if (db.DIGIRESP_Archivio_PDF.Any(x =>x.sede_gapp==sedeGapp &&  x.data_inizio == d1 && x.data_fine == d2 && x.tipologia_pdf == "R" && x.data_convalida == null))
                //{
                //    CommonTasks.StampaOnly(utenteConv[0], utenteConv[1], d1, d2, sedeGapp);
                //}
                /////////////////////////////////////////////////////

                //continue;













                Log("PDF da generare:" + ListPDFneeded.Count().ToString());
                if (ListPDFneeded.Count > 0)
                {
                    foreach (var item in ListPDFneeded)
                    {
                        Log(item.DateStart.ToString("dd/MM/yyyy") + "-" + item.DateEnd.ToString("dd/MM/yyyy"));
                    }
                    try
                    {
                        if (TeamsSent == false)
                        {
                            var lastPDF = ListPDFneeded.OrderByDescending(x => x.DateStart).FirstOrDefault();
                            if (lastPDF != null)
                            {
                                D1Teams = lastPDF.DateStart;
                                D2Teams = lastPDF.DateEnd;
                                if (FromBatchGeneraPDF)
                                {
                                    if (!IsDataFineMese(lastPDF.DateEnd))
                                    {
                                        TeamsStart(lastPDF.DateStart, lastPDF.DateEnd);
                                        TeamsSent = true;
                                    }
                                    else if (IsDataFineMese(lastPDF.DateEnd) && AmmettiUltimoPDFdelMese)
                                    {
                                        TeamsStart(lastPDF.DateStart, lastPDF.DateEnd);
                                        TeamsSent = true;
                                    }
                                    
                                }
                               
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                else
                    continue;

                foreach (PdfStartEnd PDFsegmento in ListPDFneeded)
                {
                    if (PDFsegmento.DateStart.Month != PDFsegmento.DateEnd.Month)
                    {
                        Log("Mesi diversi per segmento " + PDFsegmento.DateStart.ToString("dd/MM/yyyy") + " - " + PDFsegmento.DateEnd.ToString("dd/MM/yyyy"));
                        continue;
                    }
                    if (SedeInErrore) break;

                    Log("Verifica periodo " + sedeGapp + " " + PDFsegmento.DateStart.ToString("dd/MM/yyyy") + " " + PDFsegmento.DateEnd.ToString("dd/MM/yyyy"));
                    try
                    {
                        mailReport += "\r\n\r\n" + sedeGapp + " - " +
                               PDFsegmento.DateStart.ToString("dd/MM/yyyy") + " " +
                               PDFsegmento.DateEnd.ToString("dd/MM/yyyy") + " : ";

                        //se sede-data gia processata, salta
                        if (SedeProcessata(sedeGapp, PDFsegmento.DateStart, PDFsegmento.DateEnd))
                        {
                            Log("Periodo gia nel DB, PDF presente");
                            mailReport += " PDF gia presente";
                            continue;
                        }
                        else
                        {


                            if (IsDataFineMese(PDFsegmento.DateEnd) && !AmmettiUltimoPDFdelMese)
                            {
                                Log("PDF con data fine " + PDFsegmento.DateEnd.ToString("dd/MM/yyyy") + " - non eseguito");
                                continue;
                            }
                            if (PDFsegmento.DateStart.Month == PDFsegmento.DateEnd.Month)
                            {


                                DateTime LastDayBefore = PDFsegmento.DateStart.AddDays(-1);
                                //if (IsUltimoDelMese(LastDayBefore))
                                //{
                                //    Log(LastDayBefore.ToString("dd/MM/yyyy") + " ultimo del mese, convalida bypassata");

                                //}
                                //else
                                //{
                                Log("Verifica ultimo giorno precedente : " + LastDayBefore.ToString("dd/MM/yyyy"));
                                //se ultimo giorno prima non è convalidato:
                                Boolean? convalidato = VerificaGiornoConvalidato(LastDayBefore, utenteConv[0], utenteConv[1], sedeGapp);

                                if (PDFsegmento.DateStart.Day == 1 && (convalidato == null || convalidato == false))
                                {
                                    Log("Errore PDF primo del mese ma " + LastDayBefore.ToString("dd/MM/yyyy") + " NON CONVALIDATO");
                                    PostMessage("Errore PDF primo del mese ma " + LastDayBefore.ToString("dd/MM/yyyy") + " NON CONVALIDATO");

                                    SedeInErrore = true;
                                    continue;
                                }

                                if (convalidato == null)
                                {
                                    Log("Verifica convalidato ha restituito NULL");
                                    mailReport += " Impossibile verificare convalida";
                                    SedeInErrore = true;
                                    continue;
                                }

                                if (convalidato == false)
                                {
                                    Log("Ultimo giorno precedente NON convalidato " + LastDayBefore.ToString("dd/MM/yyyy"));
                                    string Errore = ConvalidaNoStampa(utenteConv[0], utenteConv[1], LastDayBefore, LastDayBefore, sedeGapp);
                                    if (!String.IsNullOrWhiteSpace(Errore) && !Errore.Contains("ELABORAZIONE") && !Errore.Contains("REGOLARE"))
                                    {
                                        mailReport += sedeGapp + " Errore durante convalida " + LastDayBefore.ToString("ddMMyyyy") + " - " + Errore;
                                        Log(sedeGapp + " Errore durante convalida " + LastDayBefore.ToString("ddMMyyyy") + " - " + Errore);
                                        PostMessage(" Errore durante convalida " + sedeGapp + " - " + LastDayBefore.ToString("ddMMyyyy") + " - " + Errore);

                                        SedeInErrore = true;
                                        continue;
                                    }
                                    Log("Convalida ok per " + LastDayBefore.ToString("dd/MM/yyyy"));
                                }
                                //  }

                                Log("Processo PDF");
                                EsitoPDF esito = ProcessaSedeGapp(sedeGapp, PDFsegmento.DateStart, PDFsegmento.DateEnd);
                                Log("ID pdf generato:" + esito.idPdf + " " + sedeGapp);

                                if (esito.idPdf > 0)
                                {
                                    AggiungiNotificaSedeGapp(sedeGapp, PDFsegmento.DateStart, PDFsegmento.DateEnd, esito.idPdf);
                                    mailReport += sedeGapp + " ID pdf : " + esito.idPdf;
                                    GeneratiOK++;
                                }
                                else
                                {
                                    PostMessage("PDF in errore per " + sedeGapp + ": " + esito.errore);
                                    mailReport += esito.errore;
                                    SedeInErrore = true;
                                }
                            }
                            else
                            {
                                Log(" Non eseguito-fine mese");
                                mailReport += " Non eseguito-fine mese";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogErrore(ex.ToString(), "Task_GeneraPdfEccezioni");
                        mailReport += ex.Message;
                        SedeInErrore = true;
                    }
                }
                if (SedeInErrore) sediErrore.Add(sedeGapp);
            }

            if (D1Teams != DateTime.Today && D2Teams != DateTime.Today)
            {
                if (FromBatchGeneraPDF && TeamsSent==true ) TeamsEnd(D1Teams, D2Teams);
            }

            if (sediErrore.Count > 0)
            {
                Log("In errore: " + String.Join(",", sediErrore.ToArray()));
                PostMessage("In errore: " + String.Join(",", sediErrore.ToArray()));
            }

            if (!String.IsNullOrWhiteSpace(last)) last = last + "|";

            if (sediErrore.Count == 0)
                return last;
            else
                return last + "Sedi in errore: " + String.Join(",", sediErrore.ToArray());

        }

        public static Boolean IsUltimoDelMese(DateTime d)
        {
            return (d.Month != d.AddDays(1).Month);
        }

        private static List<PdfStartEnd> ChunkDays(DateTime DStart, DateTime DEnd)
        {
            if (DStart.Month == DEnd.Month)
            {
                PdfStartEnd p = new PdfStartEnd()
                {
                    DateStart = DStart,
                    DateEnd = DEnd,
                    IsFirstSegmentPerMonth = DStart.Day == 1,
                    IsLastSegmentPerMonth = false,
                    ContainsEndMonth = (DEnd.Month != DEnd.AddDays(1).Month)
                };
                return new List<PdfStartEnd>() { p };
            }
            else
            {
                DateTime DstartEndMonth = new DateTime(DStart.Year, DStart.Month, 1).AddMonths(1).AddDays(-1);
                PdfStartEnd p1 = new PdfStartEnd()
                {
                    DateStart = DStart,
                    DateEnd = DstartEndMonth,
                    IsLastSegmentPerMonth = true,
                    IsFirstSegmentPerMonth = DStart.Day == 1,
                    ContainsEndMonth = true
                };
                PdfStartEnd p2 = new PdfStartEnd()
                {
                    DateStart = new DateTime(DEnd.Year, DEnd.Month, 1),
                    DateEnd = DEnd,
                    IsLastSegmentPerMonth = false,
                    IsFirstSegmentPerMonth = DStart.Day == 1,
                    ContainsEndMonth = false
                };
                return new List<PdfStartEnd>() { p1, p2 };
            }
        }

        public static List<PdfStartEnd> GetPDFneeded(string sedeGapp)
        {
            var db = new digiGappEntities();
            List<PdfStartEnd> LPeriodi = new List<PdfStartEnd>();

            string[] par = CommonTasks.GetParametri<string>(EnumParametriSistema.ForzaturaPDFperiodo);
            if (par != null && par.Length == 2)
            {
                DateTime Da;
                bool d1 = DateTime.TryParseExact(par[0], "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out Da);

                DateTime A;
                bool d2 = DateTime.TryParseExact(par[1], "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out A);
                if (d1 && d2)
                {
                    DateTime dayBefore = Da.AddDays(-1);
                    if (db.DIGIRESP_Archivio_PDF.Any(x => x.sede_gapp == sedeGapp && x.tipologia_pdf == "R" && x.data_fine == dayBefore))
                    {
                    PdfStartEnd p = new PdfStartEnd()
                    {
                        DateStart = Da,
                        DateEnd = A
                    };
                    LPeriodi.Add(p);
                    Log("Esecuzione con periodo forzato da DB per PDF : " + Da.ToString("dd/MM/yyyy") + "-" + A.ToString("dd/MM/yyyy"));
                    }
                    else
                        Log("Pdf precedente non trovato, impossibile generare pdf richiesto");

                    return LPeriodi;
                }
            }

            var pdf = db.DIGIRESP_Archivio_PDF.Where(x => x.sede_gapp == sedeGapp
                                                        && x.tipologia_pdf == "R")
                                                      .OrderByDescending(x => x.data_fine)
                                                      .FirstOrDefault();
            if (pdf == null)
            {

                DateTime DEnd = GetlastSunday(DateTime.Now);
                DateTime DStart = DEnd.AddDays(-6);
                LPeriodi = ChunkDays(DStart.Date, DEnd.Date);
            }
            else
            {
                DateTime DStart = pdf.data_fine.AddDays(1);
                int giornoSettimana = (int)DStart.DayOfWeek;
                if (giornoSettimana == 0) giornoSettimana = 7;
                int giorniPerDomenica = 7 - giornoSettimana;

                //if (DStart.DayOfWeek != DayOfWeek.Monday)
                //{
                //    Log(sedeGapp + " Il giorno successivo all'ultimo PDF trovato NON è LUNEDI ! " + DStart.ToString("dd/MM/yyyy"));
                //}
                //else
                //{
                DateTime Dend = DStart.AddDays(giorniPerDomenica);
                if (Dend > DateTime.Today && DStart.Month != Dend.Month)
                {
                    Dend = new DateTime(DStart.Year, DStart.Month, DateTime.DaysInMonth(DStart.Year, DStart.Month));
                    //LPeriodi.AddRange(ChunkDays(DStart, Dend));
                }

                while (Dend < DateTime.Today)
                {
                    LPeriodi.AddRange(ChunkDays(DStart, Dend));
                    DStart = Dend.AddDays(1);
                    Dend = DStart.AddDays(6);
                    if (Dend > DateTime.Today && DStart.Month != Dend.Month)
                    {
                        Dend = new DateTime(DStart.Year, DStart.Month, DateTime.DaysInMonth(DStart.Year, DStart.Month));
                    }
                }
                //}
            }
            //se oggi è minore del giorno schedulato per esecuzione, togli ultima settimana
            if ((int)DateTime.Today.DayOfWeek < GetParametri<int>(EnumParametriSistema.GiornoEsecuzioneBatchPDF)[0])
            {
                if (LPeriodi.Count > 0)
                    LPeriodi.Remove(LPeriodi[LPeriodi.Count - 1]);
            }

            DateTime D = DateTime.Now;
            var s = db.L2D_SEDE_GAPP.Where(x => x.cod_sede_gapp == sedeGapp && x.data_inizio_validita <= D && x.data_fine_validita >= D).FirstOrDefault();
            if (s != null && s.partenza_fase_3 != null)
            {
                Log("Partenza fase 3: " + ((DateTime)s.partenza_fase_3).ToString("dd/MM/yyyy") + ", eliminazione eventuali periodi precedenti");
                LPeriodi.RemoveAll(x => x.DateStart < s.partenza_fase_3);
            }
            return LPeriodi;

        }

        /// <summary>
        /// Verifica data convalidata tramite getScadenzario
        /// </summary>
        /// <param name="d"></param>
        /// <param name="matricola"></param>
        /// <param name="nominativo"></param>
        /// <param name="sede"></param>
        /// <returns></returns>
        public static Boolean? VerificaGiornoConvalidato(DateTime d, string matricola, string nominativo, string sede)
        {
            try
            {
                WSDigigapp service = (WSDigigapp)ServiceCreate(AsmxServices.Digigapp);
                Periodo[] resp = service.getScadenzarioSingolaSede(matricola, d.ToString("ddMMyyyy"), d.ToString("ddMMyyyy"), "75", sede, 75);
                if (resp != null && resp.Count() > 0)
                {


                    foreach (Periodo p in resp)
                    {
                        if (p.giornate == null) continue;
                        var day = p.giornate.Where(x => x.data.Date == d.Date).FirstOrDefault();
                        if (day != null && (day.statoTotale == "L" || day.statoTotale == "C"))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                var db = new digiGappEntities();
                MyRai_LogErrori err = new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = matricola,
                    provenienza = "VerificaGiornoConvalidato"
                };
                db.MyRai_LogErrori.Add(err);
                db.SaveChanges();

                return null;
            }
        }

        public static string StampaOnly(string matricola, string nominativo, DateTime datada, DateTime dataa, string sede)
        {
            try
            {
                WSDigigapp service = (WSDigigapp)ServiceCreate(AsmxServices.Digigapp);

                var resp = service.getStampa(matricola, nominativo, datada.ToString("ddMMyyyy"), dataa.ToString("ddMMyyyy"), sede, true, 80);
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Convalida uno o piu giorni tramite getConvalida del servizio digiGapp (no stampa)
        /// </summary>
        /// <param name="matricola"></param>
        /// <param name="nominativo"></param>
        /// <param name="datada"></param>
        /// <param name="dataa"></param>
        /// <param name="sede"></param>
        /// <returns>Null se nessun errore o stringa di errore valorizzata</returns>
        public static string ConvalidaNoStampa(string matricola, string nominativo, DateTime datada, DateTime dataa, string sede)
        {
            try
            {
                WSDigigapp service = (WSDigigapp)ServiceCreate(AsmxServices.Digigapp);
                riepilogoSedeGappResponse resp = service.getConvalida(matricola, nominativo, datada.ToString("ddMMyyyy"), dataa.ToString("ddMMyyyy"), sede, false, 75);
                Log(sede + " getConvalida response raw:" + resp.raw);
                if (resp.esito == true) return null;
                else
                {
                    if (resp.raw != null && resp.raw.ToUpper().Contains("ELABORAZIONE$REGOLARE"))
                        return null;
                    else return resp.raw;
                }
            }
            catch (Exception ex)
            {
                LogErrore("ConvalidaNoStampa, " + ex.ToString(), "CommonTask");
                return ex.Message;
            }
        }

        public static EsitoPDF ProcessaSedeGapp(string sede, DateTime dateStart, DateTime dateEnd, string TipoDiStampa = "sc")
        {
            string[] utenteConv = GetParametri<string>(EnumParametriSistema.UtentePerConvalida);
            var db = new digiGappEntities();

            EsitoPDF esito = new EsitoPDF();


            WSDigigapp service = (WSDigigapp)ServiceCreate(AsmxServices.Digigapp);
            try
            {
                riepilogoSedeGappResponse resp = new riepilogoSedeGappResponse();
                if (TipoDiStampa == EnumTipoDiStampa.ro.ToString())
                {
                    resp = service.getRiepilogoReadOnly(utenteConv[0], utenteConv[1],
                                  dateStart.ToString("ddMMyyyy"),
                                  dateEnd.ToString("ddMMyyyy"),
                                  sede,
                                  true,
                                  75);
                }
                if (TipoDiStampa == EnumTipoDiStampa.ss.ToString())
                {
                    resp = service.getStampa(utenteConv[0], utenteConv[1],
                                  dateStart.ToString("ddMMyyyy"),
                                  dateEnd.ToString("ddMMyyyy"),
                                  sede,
                                  true,
                                  75);
                }
                if (TipoDiStampa == EnumTipoDiStampa.sc.ToString())
                {
                    resp = service.getRiepilogo(utenteConv[0], utenteConv[1],
                                  dateStart.ToString("ddMMyyyy"),
                                  dateEnd.ToString("ddMMyyyy"),
                                  sede,
                                  75);
                }
                if (resp.esito != true)
                {
                    LogErrore(resp.errore + resp.raw, "GetRiepilogo");
                    esito.errore = resp.errore + resp.raw;
                    esito.idPdf = -1;
                    return esito;
                }
                string eccezioniSerial = Serialize(resp.eccezioni);



                var pdfrow = db.DIGIRESP_Archivio_PDF.Where(x => x.ID == resp.ID).FirstOrDefault();

                if (pdfrow != null)
                {
                    pdfrow.contenuto_eccezioni = eccezioniSerial;
                    db.SaveChanges();
                    esito.idPdf = resp.ID;
                    return esito;
                }
            }
            catch (Exception ex)
            {
                Log("Errore DB ProcessaSedeGapp : " + ex.ToString());
                esito.errore = ex.ToString();
            }

            esito.idPdf = -1;
            return esito;
        }

        public static DateTime? GetDataChiusura2(string mese, string anno, string matricola, int livelloUtente, int offset = 0)
        {
            var service = (WSDigigapp)ServiceCreate(AsmxServices.Digigapp);

            try
            {
                DateChiusuraResponse response = service.getDateChiusura(matricola, anno, mese, livelloUtente);

                var item = response.Chiusure.Where(x => x.Mese == Convert.ToInt32(mese) && x.Anno == Convert.ToInt32(anno))
                    .FirstOrDefault();
                if (item == null)
                    return null;
                else
                    return item.DataChiusura2;
            }
            catch (Exception ex)
            {
                Log("IsDataChiusura1 Error:" + ex.ToString());
                return null;
            }
        }
        public static MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.Tracciato_richiamato[]
            GetRecordRewDaCancellare(MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.Tracciato_richiamato[] InfoTracciati,
            myRaiData.Incentivi.XR_MAT_RICHIESTE Richiesta, DateTime dataprimo)
        {
            List<MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.Tracciato_richiamato> 
            LtracciatiDaCancellare = InfoTracciati.Where(x => x.Testo_record.Contains(Richiesta.ECCEZIONE)).ToList();

            string codici = CommonTasks.GetParametro<string>(EnumParametriSistema.CodiciRecordRew);
            // "0166,0461,0462,0463,0490,0656,0901";
            foreach (string cod in codici.Split(','))
            {
                LtracciatiDaCancellare.AddRange(InfoTracciati.Where(x =>
                x.Testo_record.StartsWith(Richiesta.MATRICOLA + dataprimo.ToString("yyMM") + cod)).ToList());
            }

            LtracciatiDaCancellare.AddRange(InfoTracciati.Where(x =>
                            x.Testo_record.StartsWith(Richiesta.MATRICOLA + dataprimo.ToString("yyMM") + "0105") &&
                            x.Testo_record.Contains(dataprimo.ToString("MMyyMMyy"))
                            ).ToList());

            int[] IDs = LtracciatiDaCancellare.Select(x => x.Id_tracciato_richiamato).ToArray();
            var AllRows = InfoTracciati.ToList();
            AllRows.RemoveAll(x => IDs.Contains(x.Id_tracciato_richiamato));
            int NonAB = AllRows.Where(x => !x.Testo_record.StartsWith(Richiesta.MATRICOLA + dataprimo.ToString("yyMM") + "0A")
            && !x.Testo_record.StartsWith(Richiesta.MATRICOLA + dataprimo.ToString("yyMM") + "0B")).Count();
            if (NonAB == 0)
            {
                LtracciatiDaCancellare.AddRange(AllRows.Where(x => x.Testo_record.StartsWith(Richiesta.MATRICOLA + dataprimo.ToString("yyMM") + "0A")
                || x.Testo_record.StartsWith(Richiesta.MATRICOLA + dataprimo.ToString("yyMM") + "0B")));
            }
            return LtracciatiDaCancellare.ToArray();
        }
        public static DateTime? GetDataChiusura1(string mese, string anno, string matricola, int livelloUtente, int offset = 0)
        {
            var service = (WSDigigapp)ServiceCreate(AsmxServices.Digigapp);

            try
            {
                DateChiusuraResponse response = service.getDateChiusura(matricola, anno, mese, livelloUtente);

                var item = response.Chiusure.Where(x => x.Mese == Convert.ToInt32(mese) && x.Anno == Convert.ToInt32(anno))
                    .FirstOrDefault();
                if (item == null)
                    return null;
                else
                    return item.DataChiusura1;
            }
            catch (Exception ex)
            {
                Log("IsDataChiusura1 Error:" + ex.ToString());
                return null;
            }
        }

        public static Boolean IsDataChiusura1(string mese, string anno, string matricola, int livelloUtente, int offset = 0)
        {
            var service = (WSDigigapp)ServiceCreate(AsmxServices.Digigapp);

            try
            {
                DateChiusuraResponse response = service.getDateChiusura(matricola, anno, mese, livelloUtente);

                var item = response.Chiusure.Where(x => x.Mese == Convert.ToInt32(mese) && x.Anno == Convert.ToInt32(anno))
                    .FirstOrDefault();
                if (item == null)
                    return false;
                else
                    return DateTime.Today.AddDays(offset) == item.DataChiusura1;
            }
            catch (Exception ex)
            {
                Log("IsDataChiusura1 Error:" + ex.ToString());
                return false;
            }
        }

        public static Boolean IsDataChiusura2(string mese, string anno, string matricola, int livelloUtente)
        {
            var service = (WSDigigapp)ServiceCreate(AsmxServices.Digigapp);

            try
            {
                DateChiusuraResponse response = service.getDateChiusura(matricola, anno, mese, livelloUtente);

                var item = response.Chiusure.Where(x => x.Mese == Convert.ToInt32(mese) && x.Anno == Convert.ToInt32(anno))
                    .FirstOrDefault();
                if (item == null)
                    return false;
                else
                    return DateTime.Today == item.DataChiusura2;
            }
            catch (Exception ex)
            {

                Log("IsDataChiusura2 Error:" + ex.ToString());
                return false;
            }
        }

        public static List<string> GetSediGappPDF()
        {
            var service = (it.rai.servizi.HRGA.Sedi)ServiceCreate(AsmxServices.HRGA);
            try
            {
                //  CategorieDatoAbilitate response = service.Get_CategoriaDato_Net_RaiPerMe("sedegapp", "", "hrup", "02GEST");

                string[] responseDB = GetParametri<string>(EnumParametriSistema.GetCategoriaDatoNetCachedL2);

                CategorieDatoAbilitate response = new CategorieDatoAbilitate();
                response = Newtonsoft.Json.JsonConvert.DeserializeObject<CategorieDatoAbilitate>(responseDB[0]);


                if (response != null && !String.IsNullOrWhiteSpace(response.Descrizione_Errore))
                {
                    Log("GetSediGapp Errore servizio: " + response.Descrizione_Errore);
                    return null;
                }

                if (response != null
                    && response.DT_CategorieDatoAbilitate != null
                    && response.DT_CategorieDatoAbilitate.Rows.Count > 0)
                {
                    List<string> sedi = response.DT_CategorieDatoAbilitate
                                        .AsEnumerable()
                                        .Select(p => p.Field<string>("Cod")
                                        .Trim())
                                        .Distinct()
                                        .Select(x => x.Substring(0, 5))
                                        .OrderBy(cod => cod)
                                        .ToList();

                    string[] par = GetParametri<string>(EnumParametriSistema.ForzaturaPDFsedi);
                    if (par != null && par.Length > 0)
                    {
                        List<string> SediPrescelte = new List<string>();
                        if (!String.IsNullOrWhiteSpace(par[0])) // 8AH00, DDE30,.....
                        {
                            SediPrescelte = par[0].Split(',').ToList();
                            Log("Esecuzione con sedi forzate da DB :" + par[0]);
                        }
                        if (!String.IsNullOrWhiteSpace(par[1])) // AN, ROMA .......
                        {
                            string[] SediMain = par[1].Split(',');
                            var db = new digiGappEntities();
                            foreach (string sedeMain in SediMain)
                            {
                                var sediFiglie = db.L2D_SEDE_GAPP.Where(x => x.CalendarioDiSede.Trim() == sedeMain)
                                    .Select(x => x.cod_sede_gapp).ToList();
                                SediPrescelte.AddRange(sediFiglie);
                            }
                            Log("Esecuzione con SEDI forzate da DB :" + par[1]);
                        }
                        if (SediPrescelte.Count > 0)
                            sedi = sedi.Intersect(SediPrescelte).ToList();
                    }
                    return sedi;
                }
            }
            catch (Exception ex)
            {
                Log("GetSediGapp Error: " + ex.ToString());
            }

            return null;
        }

        public static T GetParametro<T>(EnumParametriSistema chiave)
        {
            var db = new digiGappEntities();

            String NomeParametro = chiave.ToString();
            MyRai_ParametriSistema p = db.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == NomeParametro);
            if (p == null) return default(T);
            else return (T)Convert.ChangeType(p.Valore1, typeof(T));

        }

        public static T[] GetParametri<T>(EnumParametriSistema chiave)
        {
            var db = new digiGappEntities();
            String NomeParametro = chiave.ToString();

            MyRai_ParametriSistema p = db.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == NomeParametro);
            if (p == null) return null;
            else
            {
                T[] parametri = new T[] { (T)Convert.ChangeType(p.Valore1, typeof(T)), (T)Convert.ChangeType(p.Valore2, typeof(T)) };
                return parametri;
            }

        }

        public static Boolean IsFromBatch()
        {
            var a = System.Reflection.Assembly.GetEntryAssembly();
            if (a != null && a.GetName() != null)
            {
                string n = a.GetName().Name;
                if (n != null && (n.ToUpper().Contains("MYRAIWINDOWSSERVICE") || n.ToUpper().Contains("PDFCLI")))
                    return true;
            }
            return false;
        }

        public static void Log(string message, string methodName = null)
        {
            try
            {
                string filelog = "";
                var location = System.Reflection.Assembly.GetEntryAssembly().Location;

                var directoryPath = Path.GetDirectoryName(location);
                var logPath = System.IO.Path.Combine(directoryPath, "log");
                if (!System.IO.Directory.Exists(logPath)) System.IO.Directory.CreateDirectory(logPath);

                if (!string.IsNullOrEmpty(methodName))
                {
                    filelog = System.IO.Path.Combine(logPath, "log_" + methodName + "_" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
                }
                else
                {
                    filelog = System.IO.Path.Combine(logPath, "log_" + DateTime.Now.ToString("yyyyMMdd") + ".txt");
                }

                System.IO.File.AppendAllText(filelog, DateTime.Now.ToString("HH:mm ") + message + "\r\n");
                Console.WriteLine(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static DateTime GetlastSunday(DateTime dateStart)
        {
            DateTime LastSunday = dateStart.AddDays(-1);

            while (LastSunday.DayOfWeek != DayOfWeek.Sunday)
            {
                LastSunday = LastSunday.AddDays(-1);
            }
            return LastSunday;
        }

        public static DateTime GetlastMonday(DateTime dateStart)
        {
            if (dateStart.DayOfWeek == DayOfWeek.Monday)
                return dateStart;

            DateTime LastMonday = dateStart.AddDays(-1);

            while (LastMonday.DayOfWeek != DayOfWeek.Monday)
            {
                LastMonday = LastMonday.AddDays(-1);
            }
            return LastMonday;
        }
        public static DateTime GetlastSaturday(DateTime dateStart)
        {
            DateTime LastSaturday = dateStart.AddDays(-1);

            while (LastSaturday.DayOfWeek != DayOfWeek.Saturday)
            {
                LastSaturday = LastSaturday.AddDays(-1);
            }
            return LastSaturday;
        }
        public static DIGIRESP_Archivio_PDF SedeProcessataCheckPDF(string sede, DateTime datestart, DateTime dateend)
        {
            var db = new digiGappEntities();

            var row = db.DIGIRESP_Archivio_PDF.Where(x => x.sede_gapp == sede &&
                x.tipologia_pdf == "R" &&
                x.data_inizio == datestart &&
                x.data_fine == dateend).FirstOrDefault();
            return row;
        }

        public static Boolean SedeProcessata(string sede, DateTime datestart, DateTime dateend)
        {
            var db = new digiGappEntities();

            var row = db.DIGIRESP_Archivio_PDF.Where(x => x.sede_gapp == sede &&
                x.tipologia_pdf == "R" &&
                x.data_inizio == datestart &&
                x.data_fine == dateend).FirstOrDefault();
            return (row != null);
        }

        public static void LogErrore(string errore, string provenienza)
        {
            try
            {
                if (Environment.UserInteractive) Console.WriteLine(errore);
                MyRai_LogErrori err = new MyRai_LogErrori()
                {
                    applicativo = IsFromBatch() ? "BATCH" : "PORTALE",
                    data = DateTime.Now,
                    error_message = errore,
                    matricola = "000000",
                    provenienza = provenienza
                };
                using (var db = new digiGappEntities())
                {
                    db.MyRai_LogErrori.Add(err);
                    db.SaveChanges();
                }
                Log("Errore da " + provenienza + ": " + errore);
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }
        }

        public static string Serialize(Eccezione[] v)
        {
            var j = new JavaScriptSerializer();
            j.MaxJsonLength = 100000000;
            var json = j.Serialize(v);
            return json;
        }

        public static void AggiungiNotificaSedeGapp(string sede, DateTime dateStart, DateTime dateEnd, int idPdf)
        {
            var db = new digiGappEntities();
            var tt = GetMatricolaLivelloPerSede(sede, 2);
            if (tt == null || tt.Count == 0)
            {
                Log("AggiungiNotificaSedeGapp: nessuna matricola liv 2 resp per " + sede);
                return;
            }

            String matricola = "";
            for (int i = 0; i < tt.Count; i++)
            {
                if (String.IsNullOrWhiteSpace(tt[i])) continue;

                if (tt[i].ToString().Substring(0, 1) == "P")
                { matricola = tt[i].Substring(1, tt[i].Length - 1); }
                else
                { matricola = tt[i].ToString(); }

                myRaiData.MyRai_Notifiche n = new MyRai_Notifiche()
                {
                    categoria = "PDF firma",
                    data_inserita = DateTime.Now,
                    descrizione = "Nuovo doc in firma per sede " + sede + ":" + dateStart.ToString("dd MMMM") + "/" + dateEnd.ToString("dd MMMM"),
                    inserita_da = "ServizioWindows",
                    id_riferimento = idPdf,
                    matricola_destinatario = matricola,
                    email_destinatario = GetEmailPerMatricola(matricola)
                };
                db.MyRai_Notifiche.Add(n);
                Log("Aggiunta notifica di nuovo doc in firma per " + matricola);
                try
                {
                    db.SaveChanges();
                    Log("Salvataggio notifiche OK");
                }
                catch (Exception ex)
                {
                    Log("AggiungiNotificaSedeGapp errore:" + ex.ToString());
                }
            }


        }

        public static string CheckCacheL2()
        {
            try
            {
                string[] responseDB = GetParametri<string>(EnumParametriSistema.GetCategoriaDatoNetCachedL2);
                if (
                       String.IsNullOrWhiteSpace(responseDB[0])
                    || responseDB[1] != DateTime.Today.ToString("dd/MM/yyyy")
                    )
                {
                    it.rai.servizi.HRGA.Sedi service = (it.rai.servizi.HRGA.Sedi)ServiceCreate(AsmxServices.HRGA);

                    var response = service.Get_CategoriaDato_Net_RaiPerMe("sedegapp", "", "HRUP", "02GEST");
                    if (response.Cod_Errore == "0")
                    {
                        var db = new digiGappEntities();
                        string chiave = EnumParametriSistema.GetCategoriaDatoNetCachedL2.ToString();
                        var par = db.MyRai_ParametriSistema.Where(x => x.Chiave == chiave).FirstOrDefault();
                        if (par != null)
                        {
                            par.Valore1 = Newtonsoft.Json.JsonConvert.SerializeObject(response);
                            par.Valore2 = DateTime.Today.ToString("dd/MM/yyyy");
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

            return null;
        }



        public static void PostMessage(string text, string username = "RAIPERME", string channel = "#general")
        {
            Payload payload = new Payload()
            {
                Channel = channel,
                Username = username,
                Text = text
            };

            PostMessage(payload);
        }
        public static void PostMessageAdvanced(string text, Attachment[] attachments, string username = "RAIPERME", string channel = "#general")
        {
            PayloadAdvanced payload = new PayloadAdvanced()
            {
                Channel = channel,
                Username = username,
                Text = text,
                Attachments = attachments
            };

            PostMessageAdvanced(payload);
        }
        /*
         { "text":"Batch Marcatura Urgenti/Scadute",
    "attachments": [
        {
            "color": "good",
            "title": "Avviato alle 12:50.30",
           "text": "Schedulato alle ore 8,12,16,20"
        },
		{
            "color": "danger",
            "title": "Sedi in errore:xxxx"
        },
		 {
            "color": "good",
            "title": "Terminato alle 12:53.30",
            "text": "Optional text that appears within the attachment"
        }
    ]
}
             */
        public static void PostMessageAdvanced(PayloadAdvanced payload)
        {
            try
            {
                string payloadJson = JsonConvert.SerializeObject(payload);

                using (WebClient client = new WebClient())
                {
                    NameValueCollection data = new NameValueCollection();
                    data["payload"] = payloadJson;

                    var response = client.UploadValues("https://hooks.slack.com/services/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",
                    "POST", data);

                    //The response text is usually "ok"
                    string responseText = new UTF8Encoding().GetString(response);
                }
            }
            catch (Exception ex)
            {

            }
        }
        public static void PostMessage(Payload payload)
        {


            try
            {
                string payloadJson = JsonConvert.SerializeObject(payload);

                using (WebClient client = new WebClient())
                {
                    NameValueCollection data = new NameValueCollection();
                    data["payload"] = payloadJson;

                    var response = client.UploadValues("https://hooks.slack.com/services/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",
                    "POST", data);

                    //The response text is usually "ok"
                    string responseText = new UTF8Encoding().GetString(response);
                }
            }
            catch (Exception ex)
            {

            }
        }
        public static List<String> GetMatricolaLivelloPerSede(string sedegapp, int livelloResponsabile_1_2)
        {
            string[] responseDB = null;

            if (livelloResponsabile_1_2 == 2)
            {
                responseDB = GetParametri<string>(EnumParametriSistema.GetCategoriaDatoNetCachedL2);
            }
            if (livelloResponsabile_1_2 == 1)
            {
                responseDB = GetParametri<string>(EnumParametriSistema.GetCategoriaDatoNetCached);
            }
            CategorieDatoAbilitate response = new CategorieDatoAbilitate();
            if (!String.IsNullOrWhiteSpace(responseDB[0]))
            {
                response = Newtonsoft.Json.JsonConvert.DeserializeObject<CategorieDatoAbilitate>(responseDB[0]);
            }
            else
            {
                it.rai.servizi.HRGA.Sedi service = (it.rai.servizi.HRGA.Sedi)ServiceCreate(AsmxServices.HRGA);
                response = service.Get_CategoriaDato_Net("sedegapp", "", "HRUP", "0" + livelloResponsabile_1_2.ToString() + "GEST");
            }

            var item = response.CategorieDatoAbilitate_Array
                .Where(sede => sede.Codice_categoria_dato.Trim().ToUpper() == sedegapp.Trim().ToUpper())
                       .FirstOrDefault();
            if (item == null) return null;

            List<string> Matricole = item.DT_Utenti_CategorieDatoAbilitate
                                         .AsEnumerable()
                                         .Select(p => (String)p.ItemArray[0]).ToList();
            return Matricole;
        }

        public static System.Web.Services.Protocols.SoapHttpClientProtocol ServiceCreate(AsmxServices serv)
        {
            switch (serv)
            {
                case AsmxServices.HRGA:
                    return new it.rai.servizi.HRGA.Sedi()
                    {
                        Credentials = new System.Net.NetworkCredential(GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]),
                        Url = System.Configuration.ConfigurationManager.AppSettings["URLhrga"]
                    };
                case AsmxServices.HRGB:
                    return new it.rai.servizi.hrgb.Service()
                    {
                        Credentials = new System.Net.NetworkCredential(GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]),
                        Url = System.Configuration.ConfigurationManager.AppSettings["URLhrgb"]
                    };
                case AsmxServices.Digigapp:
                    var ser = new it.rai.servizi.svildigigappws.WSDigigapp()
                    {
                        Credentials = new System.Net.NetworkCredential(GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]),
                        Url = System.Configuration.ConfigurationManager.AppSettings["URLdigigapp"]
                    };
                    return ser;

            }
            return null;
        }

        public static Abilitazioni getAbilitazioni()
        {
            Abilitazioni AB = new Abilitazioni();
            try
            {
                it.rai.servizi.HRGA.Sedi service = new it.rai.servizi.HRGA.Sedi();
                service.Credentials = new System.Net.NetworkCredential("srvruofpo", "zaq22?mk");

                it.rai.servizi.HRGA.CategorieDatoAbilitate response = Get_CategoriaDato_Net_Cached(0);

                foreach (var item in response.CategorieDatoAbilitate_Array)
                {
                    AbilitazioneSede absede = new AbilitazioneSede()
                    {
                        Sede = item.Codice_categoria_dato,
                        DescrSede = item.Descrizione_categoria_dato
                    };

                    foreach (System.Data.DataRow row in item.DT_Utenti_CategorieDatoAbilitate.Rows)
                    {
                        if (row["codice_sottofunzione"].ToString() == "01GEST")
                        {
                            MatrSedeAppartenenza ms = new MatrSedeAppartenenza();
                            ms.Matricola = row["logon_id"].ToString();
                            ms.SedeAppartenenza = row["SedeGappAppartenenza"].ToString();
                            ms.Delegante = row["Delegante"].ToString();
                            ms.Delegato = row["Delegato"].ToString();
                            absede.MatrLivello1.Add(ms);
                        }
                        if (row["codice_sottofunzione"].ToString() == "02GEST")
                        {
                            MatrSedeAppartenenza ms = new MatrSedeAppartenenza();
                            ms.Matricola = row["logon_id"].ToString();
                            ms.SedeAppartenenza = row["SedeGappAppartenenza"].ToString();
                            ms.Delegante = row["Delegante"].ToString();
                            ms.Delegato = row["Delegato"].ToString();
                            absede.MatrLivello2.Add(ms);
                        }
                    }
                    AB.ListaAbilitazioni.Add(absede);
                }
                AB.ListaAbilitazioni = AB.ListaAbilitazioni.OrderBy(x => x.Sede).ToList();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString());
            }
            return AB;
        }

        public static it.rai.servizi.HRGA.CategorieDatoAbilitate Get_CategoriaDato_Net_Cached(int Liv)
        {
            string[] responseDB;

            if (Liv == 1)
            {
                responseDB = CommonTasks.GetParametri<string>(CommonTasks.EnumParametriSistema.GetCategoriaDatoNetCached);
                if (
                       String.IsNullOrWhiteSpace(responseDB[0])
                    //|| responseDB[1] != DateTime.Today.ToString("dd/MM/yyyy")
                    )
                {
                    it.rai.servizi.HRGA.Sedi service = new it.rai.servizi.HRGA.Sedi();
                    it.rai.servizi.HRGA.CategorieDatoAbilitate response = service.Get_CategoriaDato_Net("sedegapp", "", "HRUP", "01GEST");
                    if (response.Cod_Errore == "0")
                    {
                        using (var db = new digiGappEntities())
                        {
                            string chiave = CommonTasks.EnumParametriSistema.GetCategoriaDatoNetCached.ToString();
                            var par = db.MyRai_ParametriSistema.Where(x => x.Chiave == chiave).FirstOrDefault();
                            if (par != null)
                            {
                                par.Valore1 = Newtonsoft.Json.JsonConvert.SerializeObject(response);
                                par.Valore2 = DateTime.Today.ToString("dd/MM/yyyy");
                                db.SaveChanges();
                            }
                        }
                    }

                    return response;
                }
                else
                {
                    it.rai.servizi.HRGA.CategorieDatoAbilitate response =
                        Newtonsoft.Json.JsonConvert.DeserializeObject<it.rai.servizi.HRGA.CategorieDatoAbilitate>(responseDB[0]);

                    return response;
                }
            }
            else if (Liv == 2)
            {
                responseDB = CommonTasks.GetParametri<string>(CommonTasks.EnumParametriSistema.GetCategoriaDatoNetCachedL2);
                if (
                       String.IsNullOrWhiteSpace(responseDB[0])
                    //|| responseDB[1] != DateTime.Today.ToString("dd/MM/yyyy")
                    )
                {
                    it.rai.servizi.HRGA.Sedi service = new it.rai.servizi.HRGA.Sedi();
                    it.rai.servizi.HRGA.CategorieDatoAbilitate response =
                        service.Get_CategoriaDato_Net("sedegapp", "", "HRUP", "02GEST");
                    if (response.Cod_Errore == "0")
                    {
                        using (var db = new digiGappEntities())
                        {
                            string chiave = CommonTasks.EnumParametriSistema.GetCategoriaDatoNetCachedL2.ToString();
                            var par = db.MyRai_ParametriSistema.Where(x => x.Chiave == chiave).FirstOrDefault();
                            if (par != null)
                            {
                                par.Valore1 = Newtonsoft.Json.JsonConvert.SerializeObject(response);
                                par.Valore2 = DateTime.Today.ToString("dd/MM/yyyy");
                                db.SaveChanges();
                            }
                        }
                    }

                    return response;
                }
                else
                {
                    it.rai.servizi.HRGA.CategorieDatoAbilitate response =
                        Newtonsoft.Json.JsonConvert.DeserializeObject<it.rai.servizi.HRGA.CategorieDatoAbilitate>(responseDB[0]);

                    return response;
                }
            }
            else  //senza livello
            {
                responseDB = CommonTasks.GetParametri<string>(CommonTasks.EnumParametriSistema.GetCategoriaDatoNetCachedNolevel);
                if (
                       String.IsNullOrWhiteSpace(responseDB[0])
                    //|| responseDB[1] != DateTime.Today.ToString("dd/MM/yyyy")
                    )
                {
                    it.rai.servizi.HRGA.Sedi service = new it.rai.servizi.HRGA.Sedi();
                    it.rai.servizi.HRGA.CategorieDatoAbilitate response =
                        service.Get_CategoriaDato_Net("sedegapp", "", "HRUP", "01GEST|02GEST");

                    if (response.Cod_Errore == "0")
                    {
                        using (var db = new digiGappEntities())
                        {
                            string chiave = CommonTasks.EnumParametriSistema.GetCategoriaDatoNetCachedNolevel.ToString();
                            var par = db.MyRai_ParametriSistema.Where(x => x.Chiave == chiave).FirstOrDefault();
                            if (par != null)
                            {
                                par.Valore1 = Newtonsoft.Json.JsonConvert.SerializeObject(response);
                                par.Valore2 = DateTime.Today.ToString("dd/MM/yyyy");
                                db.SaveChanges();
                            }
                        }
                    }

                    return response;
                }
                else
                {
                    it.rai.servizi.HRGA.CategorieDatoAbilitate response =
                        Newtonsoft.Json.JsonConvert.DeserializeObject<it.rai.servizi.HRGA.CategorieDatoAbilitate>(responseDB[0]);

                    return response;
                }
            }
        }

        public static List<string> GetSediReparti_MatricolaLivello1(string matricolaL1)
        {
            Abilitazioni AB = getAbilitazioni();
            List<string> sedi =
                AB.ListaAbilitazioni.Where(x => x.MatrLivello1.Select(a => a.Matricola).Contains(matricolaL1)).Select(x => x.Sede).ToList();

            return sedi;
        }

        public static void InviaNotificaPerEliminataSuGapp(MyRai_Eccezioni_Richieste er, string applicativo)
        {
            try
            {
                if (er.MyRai_Richieste.id_stato == 10)
                {
                    NotificaEliminataDipendente(er);
                }
                else if (er.MyRai_Richieste.id_stato == 20)
                {
                    NotificaEliminataDipendente(er);
                    NotificaEliminataResponsabile(er);
                }
            }
            catch (Exception ex)
            {
                var db = new digiGappEntities();
                db.MyRai_LogErrori.Add(new MyRai_LogErrori()
                {
                    applicativo = applicativo,
                    data = DateTime.Now,
                    error_message = ex.ToString(),
                    matricola = er.MyRai_Richieste.matricola_richiesta,
                    provenienza = "InviaNotificaPerEliminataSuGapp"
                });
                db.SaveChanges();
            }
        }

        public static void NotificaEliminataDipendente(MyRai_Eccezioni_Richieste er)
        {
            string[] par = GetParametri<string>(EnumParametriSistema.NotificaEliminataGapp);
            if (par == null || string.IsNullOrWhiteSpace(par[0])) return;

            if (EsisteNotifica(EnumCategoriaEccezione.EliminataGapp, er.MyRai_Richieste.id_richiesta, er.MyRai_Richieste.matricola_richiesta))
                return;

            var db = new digiGappEntities();
            MyRai_Notifiche noti = new MyRai_Notifiche()
            {
                data_inserita = DateTime.Now,
                categoria = EnumCategoriaEccezione.EliminataGapp.ToString(),
                data_inviata = null,
                data_letta = null,
                descrizione = par[0].Replace("#ECC#", er.cod_eccezione).Replace("#DATA#", er.data_eccezione.ToString("dd/MM/yyyy")),
                matricola_destinatario = er.MyRai_Richieste.matricola_richiesta,
                id_riferimento = er.MyRai_Richieste.id_richiesta,
                inserita_da = "Portale",
                tipo = null,
                email_destinatario = null
            };
            db.MyRai_Notifiche.Add(noti);
            db.SaveChanges();
        }

        public static void NotificaEliminataResponsabile(MyRai_Eccezioni_Richieste er)
        {
            string[] par = GetParametri<string>(EnumParametriSistema.NotificaEliminataGapp);
            if (par == null || string.IsNullOrWhiteSpace(par[1]))
                return;

            Abilitazioni AB = getAbilitazioni();
            AbilitazioneSede absede = null;
            if (!String.IsNullOrWhiteSpace(er.MyRai_Richieste.reparto) && er.MyRai_Richieste.reparto != "00")
                absede = AB.ListaAbilitazioni.Where(x => x.Sede == er.MyRai_Richieste.codice_sede_gapp + er.MyRai_Richieste.reparto).FirstOrDefault();

            if (absede == null)
                absede = AB.ListaAbilitazioni.Where(x => x.Sede == er.MyRai_Richieste.codice_sede_gapp).FirstOrDefault();

            if (absede == null || absede.MatrLivello1 == null || absede.MatrLivello1.Count() == 0)
                return;

            var db = new digiGappEntities();
            foreach (var m in absede.MatrLivello1)
            {
                if (EsisteNotifica(EnumCategoriaEccezione.EliminataGapp, er.MyRai_Richieste.id_richiesta, m.Matricola))
                    continue;

                MyRai_Notifiche noti = new MyRai_Notifiche()
                {
                    data_inserita = DateTime.Now,
                    categoria = EnumCategoriaEccezione.EliminataGapp.ToString(),
                    data_inviata = null,
                    data_letta = null,
                    descrizione = par[1].Replace("#ECC#", er.cod_eccezione)
                                        .Replace("#DATA#", er.data_eccezione.ToString("dd/MM/yyyy"))
                                        .Replace("#MATR#", er.MyRai_Richieste.matricola_richiesta),
                    matricola_destinatario = m.Matricola,
                    id_riferimento = er.MyRai_Richieste.id_richiesta,
                    inserita_da = "Portale",
                    tipo = null,
                    email_destinatario = null
                };
                db.MyRai_Notifiche.Add(noti);
            }
            db.SaveChanges();
        }

        public static Boolean EsisteNotifica(EnumCategoriaEccezione categoria, int idRif, string matricolaDest)
        {
            var db = new digiGappEntities();
            string cat = categoria.ToString();
            return db.MyRai_Notifiche.Any(x => x.categoria == cat && x.id_riferimento == idRif && x.matricola_destinatario == matricolaDest);
        }

        public static Boolean IsProduzione()
        {
            return System.Configuration.ConfigurationManager.ConnectionStrings["digiGappEntities2"].ConnectionString.ToLower().Contains("zto");
        }

        public static string[] RewriteAddress(string[] addressList, Email eml, string[] allowed)
        {
            if (addressList == null || addressList.Length == 0) return addressList;
            for (int i = 0; i < addressList.Length; i++)
            {
                if (String.IsNullOrWhiteSpace(addressList[i])) continue;
                if (allowed.Select(x => x.Trim()).Contains(addressList[i], StringComparer.InvariantCultureIgnoreCase)) continue;

                //sovrascrivi indirizzo e annota nella mail
                eml.Body = "<p>Reinstradato da sviluppo: " + addressList[i] + "</p>" + eml.Body;
                addressList[i] = "ruo.sip.presidioopen@rai.it";
            }
            return addressList;
        }

        public static List<myRaiData.MyRai_Richieste> GetRichiesteInApprovazioneMesiPrecedenti()
        {
            var db = new digiGappEntities();
            DateTime DataLimite = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            List<MyRai_Richieste> ListaRichieste = db.MyRai_Richieste.Where(x => x.id_stato == 10 && x.MyRai_Eccezioni_Richieste.All(a => a.data_eccezione < DataLimite)).ToList();
            return ListaRichieste;
        }

        public static MyRaiServiceInterface.MyRaiServiceReference1.AllineaGiornataResponse AllineaGiornata(DateTime date, string matricola, string applicativo = "WCFservice")
        {
            MyRaiServiceInterface.MyRaiServiceReference1.AllineaGiornataResponse response = new MyRaiServiceInterface.MyRaiServiceReference1.AllineaGiornataResponse();

            WSDigigapp datiBack = new WSDigigapp();

            try
            {
                datiBack.Credentials = new System.Net.NetworkCredential(
                       GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                       GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]
                       );

                var dettaglioGiornata = datiBack.getEccezioni(matricola, date.ToString("ddMMyyyy"), "BU", 70);

                var db = new digiGappEntities();
                var EccDB = db.MyRai_Eccezioni_Richieste.Where(x => x.data_eccezione == date)
                             .Where(x => x.azione == "I" && x.id_stato != 60 && x.id_stato != 50 && x.MyRai_Richieste.matricola_richiesta == matricola)
                             .ToList();

                Boolean DBtoUpdate = false;
                List<int> Lid = new List<int>();
                foreach (MyRai_Eccezioni_Richieste m in EccDB.Where(x => x.id_stato == 10))
                {
                    if (m.numero_documento == 0) continue;

                    if (!dettaglioGiornata.eccezioni.Select(x => x.ndoc)
                        .ToList()
                        .Contains(m.numero_documento.ToString().PadLeft(6, '0')))
                    {
                        var stornoDB = db.MyRai_Eccezioni_Richieste.Where(x => x.azione == "C" &&
                                 x.numero_documento_riferimento == m.numero_documento &&
                                 x.codice_sede_gapp == m.codice_sede_gapp &&
                                 x.id_stato == (int)EnumStatiRichiesta.Approvata).FirstOrDefault();
                        if (m.id_stato == (int)EnumStatiRichiesta.Approvata && stornoDB != null)
                        {
                            //è normale che non ci sia
                        }
                        else
                        {
                            myRaiCommonTasks.CommonTasks.InviaNotificaPerEliminataSuGapp(m, applicativo);

                            m.id_stato = (int)EnumStatiRichiesta.Eliminata;
                            m.MyRai_Richieste.id_stato = (int)EnumStatiRichiesta.Eliminata;
                            MyRai_LogAzioni la = new MyRai_LogAzioni()
                            {
                                applicativo = applicativo,
                                data = DateTime.Now,
                                matricola = matricola,
                                provenienza = "CommonTask.AllineaGiornata",
                                operazione = "ELIMINATA SU GAPP",
                                descrizione_operazione = "id rich:" + m.MyRai_Richieste.id_richiesta + " id ecc_rich:" + m.id_eccezioni_richieste
                            };
                            db.MyRai_LogAzioni.Add(la);
                            DBtoUpdate = true;
                            Lid.Add(m.MyRai_Richieste.id_richiesta);
                        }
                    }
                }
                if (DBtoUpdate) db.SaveChanges();
                response.success = true;
                response.error = null;
                response.IdRichiesteEliminateDB = Lid.ToArray();
            }
            catch (Exception ex)
            {
                response.success = false;
                response.error = ex.Message;
                response.IdRichiesteEliminateDB = null;
            }
            return response;

        }



        public static String GetNominativo(string matricola, MyRaiServiceInterface.it.rai.servizi.digigappws.pianoFerieSedeGapp resp)
        {
            return resp.dipendenti.Where(x => x.matricola == "0" + matricola).Select(x => x.cognome + " " + x.nome).FirstOrDefault();
        }

        public static PdfPTable GetTable()
        {
            var Table = new PdfPTable(2);
            Table.SetWidths(new int[] { 50, 50 });

            Table.WidthPercentage = 100;
            Table.DefaultCell.Padding = 1;
            Table.DefaultCell.Border = 0;
            Table.DefaultCell.BorderWidthTop = 0;
            Table.DefaultCell.BorderWidthLeft = 0;
            Table.DefaultCell.BorderWidthRight = 0;
            Table.DefaultCell.BorderWidthBottom = 0;
            return Table;
        }
        public static PdfPTable GetTableLegenda(PdfContentByte cbb, Font FontTestoSmall, PianoFeriePDFImages Images)
        {
            Font FontCella = FontFactory.GetFont(FontFactory.COURIER, 6, iTextSharp.text.Font.BOLD);
            var TableLegenda = new PdfPTable(2);
            TableLegenda.SetWidths(new int[] { 5, 50 });

            TableLegenda.WidthPercentage = 50;
            TableLegenda.DefaultCell.Padding = 1;
            TableLegenda.DefaultCell.Border = 0;
            TableLegenda.DefaultCell.BorderWidthTop = 0;
            TableLegenda.DefaultCell.BorderWidthLeft = 0;
            TableLegenda.DefaultCell.BorderWidthRight = 0;
            TableLegenda.DefaultCell.BorderWidthBottom = 0;
            TableLegenda.HorizontalAlignment = 0;

            PdfPCell pc = new PdfPCell(new Phrase("LEGENDA", FontTestoSmall)) { Colspan = 2 };
            TableLegenda.AddCell(pc);
            TableLegenda.AddCell(new PdfPCell(Images.ImageFEPF) { FixedHeight = 18, VerticalAlignment = Element.ALIGN_MIDDLE });
            TableLegenda.AddCell(new PdfPCell(new Phrase("FE - RICHIESTE SU PIANO FERIE", FontCella)) { FixedHeight = 18, VerticalAlignment = Element.ALIGN_MIDDLE });

            TableLegenda.AddCell(new PdfPCell(Images.ImageRRPF) { FixedHeight = 18, VerticalAlignment = Element.ALIGN_MIDDLE });
            TableLegenda.AddCell(new PdfPCell(new Phrase("RR - RICHIESTI SU PIANO FERIE", FontCella)) { FixedHeight = 18, VerticalAlignment = Element.ALIGN_MIDDLE });

            TableLegenda.AddCell(new PdfPCell(Images.ImageRNPF) { FixedHeight = 18, VerticalAlignment = Element.ALIGN_MIDDLE });
            TableLegenda.AddCell(new PdfPCell(new Phrase("RN - RICHIESTI SU PIANO FERIE", FontCella)) { FixedHeight = 18, VerticalAlignment = Element.ALIGN_MIDDLE });

            TableLegenda.AddCell(new PdfPCell(Images.ImageFEGapp) { FixedHeight = 18, VerticalAlignment = Element.ALIGN_MIDDLE });
            TableLegenda.AddCell(new PdfPCell(new Phrase("FE - GIA PRESENTI SU GAPP", FontCella)) { FixedHeight = 18, VerticalAlignment = Element.ALIGN_MIDDLE });

            TableLegenda.AddCell(new PdfPCell(Images.ImageRRGapp) { FixedHeight = 18, VerticalAlignment = Element.ALIGN_MIDDLE });
            TableLegenda.AddCell(new PdfPCell(new Phrase("RR - GIA PRESENTI SU GAPP", FontCella)) { FixedHeight = 18, VerticalAlignment = Element.ALIGN_MIDDLE });

            TableLegenda.AddCell(new PdfPCell(Images.ImageRNGapp) { FixedHeight = 18, VerticalAlignment = Element.ALIGN_MIDDLE });
            TableLegenda.AddCell(new PdfPCell(new Phrase("RN - GIA PRESENTI SU GAPP", FontCella)) { FixedHeight = 18, VerticalAlignment = Element.ALIGN_MIDDLE });

            return TableLegenda;
        }

        public static String GetImportoRMTR_Venezia(DateTime D)
        {
            string[] par = GetParametri<string>(EnumParametriSistema.DateRMTRVenezia);
            DateTime D1;
            DateTime.TryParseExact(par[0], "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D1);
            DateTime D2;
            DateTime.TryParseExact(par[1], "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D2);


            // if (D >= new DateTime(2019, 6, 29) && D <= new DateTime(2019, 9, 22))
            if (D >= D1 && D <= D2)
            {
                return "0001549";
            }
            else
            {
                if ((int)D.DayOfWeek >= 1 && (int)D.DayOfWeek <= 5)
                    return "0003000";
                else
                    return "0001549";
            }
        }
        public static CancellazioneFerie IsDayToReplan(int id_richiesta, string matr, string sede, string rep)
        {
            var db = new digiGappEntities();
            var rich = db.MyRai_Richieste.Where(x => x.id_richiesta == id_richiesta).FirstOrDefault();
            DateTime D = DateTime.Now;
            if (rich == null) throw new Exception("Richiesta non trovata ID " + id_richiesta);
            bool GiorniMultipli = rich.periodo_dal != rich.periodo_al;

            if (rich.da_pianoferie && !GiorniMultipli)
                return CancellazioneFerie.CancellazioneConRipianificazione;

            if (rich.da_pianoferie && GiorniMultipli)
                return CancellazioneFerie.CancellazioneNonPossibile;


            if (rich != null && rich.periodo_dal > D && rich.MyRai_Eccezioni_Richieste.Any(x => x.cod_eccezione.Trim() == "FE" || x.cod_eccezione.Trim() == "RR" || x.cod_eccezione.Trim() == "RN"))
            {



                if (String.IsNullOrWhiteSpace(rep) || rep.Trim() == "0" || rep.Trim() == "00") rep = null;
                sede = sede + rep;

                bool PianoFerieInviato = db.MyRai_PianoFerie.Any(x => x.matricola == matr && x.anno == rich.periodo_dal.Year && x.data_consolidato != null && rich.periodo_dal > x.data_consolidato);
                if (PianoFerieInviato)
                {
                    DateTime DataInvio = (DateTime)db.MyRai_PianoFerie.Where(x => x.matricola == matr && x.anno == rich.periodo_dal.Year).Select(x => x.data_consolidato).FirstOrDefault();
                    if (rich.data_richiesta > DataInvio)
                        return CancellazioneFerie.CancellazioneNormale;

                    if (GiorniMultipli)
                        return CancellazioneFerie.CancellazioneNonPossibile;

                    var PFsede = db.MyRai_PianoFerieSedi.Where(x => x.sedegapp == sede && x.anno == rich.periodo_dal.Year).OrderByDescending(x => x.numero_versione).FirstOrDefault();
                    if (PFsede == null)
                        return CancellazioneFerie.CancellazioneConRipianificazione;

                    if (PFsede.data_approvata != null && PFsede.data_firma == null)
                        return CancellazioneFerie.CancellazioneNonPossibile;

                    if (PFsede.data_approvata == null && PFsede.data_firma != null)
                        return CancellazioneFerie.CancellazioneConRipianificazione;

                    if (PFsede.data_approvata == null && PFsede.data_firma == null)
                        return CancellazioneFerie.CancellazioneConRipianificazione;

                    if (PFsede.data_approvata != null && PFsede.data_firma != null)
                        return CancellazioneFerie.CancellazioneConRipianificazione;
                }
            }
            return CancellazioneFerie.CancellazioneNormale;
        }
        public static bool Richiede2021(string matricola)
        {
            return false;

            if (matricola.Length == 7)
                matricola = matricola.Substring(1);

            var db = new digiGappEntities();
            return db.MyRai_ArretratiExcel2019.Any(x => x.matricola == matricola && x.estensione_marzo == true & x.da_fare > 0);
        }

        public static byte[] GeneraPdfPianoFerie(string sedeCodice, string sedeDesc, int anno, string matricola,
            string descrizioneReparto, int versione, bool modificaDB, string matricole,
            MyRaiServiceInterface.it.rai.servizi.digigappws.pianoFerieSedeGapp PFsede =null)
        {

            MyRaiServiceInterface.it.rai.servizi.digigappws.WSDigigapp wsService = new MyRaiServiceInterface.it.rai.servizi.digigappws.WSDigigapp()
            {
                Credentials = new System.Net.NetworkCredential(
                       GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                       GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1])
            };
            MyRaiServiceInterface.it.rai.servizi.digigappws.pianoFerieSedeGapp resp = null;
            if (PFsede != null)
                resp = PFsede;
            else
                resp =
                wsService.getPianoFerieSedeGapp("P" + matricola, "01" + "01" + anno, "", "", sedeCodice.ToUpper().Substring(0, 5), 75);

            //if (Dip != null)
            //    resp.dipendenti = Dip;


            Font titolo = FontFactory.GetFont(FontFactory.COURIER, 20, iTextSharp.text.Font.BOLD);
            Font FontGrassetto = FontFactory.GetFont(FontFactory.COURIER, 10, iTextSharp.text.Font.BOLD);
            Font FontTesto = FontFactory.GetFont(FontFactory.COURIER, 10, iTextSharp.text.Font.NORMAL);
            Font FontTestoSmall = FontFactory.GetFont(FontFactory.COURIER, 9, iTextSharp.text.Font.NORMAL);

            var Table = GetTable();

            List<MyRai_PianoFerieBatch> Lrip = GetRipianificazioni(sedeCodice, anno);

            using (var stream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 30, 30, 30, 30);
                var writer = PdfWriter.GetInstance(document, stream);

                document.Open();
                PdfContentByte cbb = writer.DirectContent;

                PianoFeriePDFImages Images = new PianoFeriePDFImages(cbb);

                PdfPCell EmptyCell = new PdfPCell(new Phrase(" ")) { Border = 0, Colspan = 2 };

                PdfPCell c2 = new PdfPCell(new iTextSharp.text.Phrase("PIANO FERIE - ANNO " + anno, FontGrassetto)) { HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER };
                c2.Colspan = 2;
                c2.Border = 0;
                Table.AddCell(c2);

                PdfPCell ca = new PdfPCell(new iTextSharp.text.Phrase(sedeCodice.ToUpper() + " - " + sedeDesc, FontTesto)) { HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT, Border = 0 };
                PdfPCell cb = new PdfPCell(new iTextSharp.text.Phrase("Versione " + versione, FontTesto)) { HorizontalAlignment = iTextSharp.text.Element.ALIGN_RIGHT, Border = 0 };
                Table.AddCell(ca);
                Table.AddCell(cb);

                if (descrizioneReparto != null)
                {
                    PdfPCell cellReparto = new PdfPCell(new Phrase(descrizioneReparto, FontTestoSmall)) { Colspan = 2, HorizontalAlignment = iTextSharp.text.Element.ALIGN_LEFT, Border = 0 };
                    Table.AddCell(cellReparto);
                }

                Table.AddCell(EmptyCell);
                Table.AddCell(new PdfPCell(new Phrase("DETTAGLIO PER MESE", FontTestoSmall)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_CENTER, Border = 0 });
                Table.AddCell(EmptyCell);

                document.Add(Table);

                var db = new digiGappEntities();
                List<MyRai_PianoFerie> dipendenti = db.MyRai_PianoFerie.Where(x => x.sedegapp == sedeCodice.Replace("-", "") && x.anno == anno && x.data_approvato != null).ToList();

                List<String> MatricoleNonInQuestaSedeGapp = new List<string>();

                foreach (var DBdipendente in dipendenti)
                {
                    var GAPPdipendente =resp.dipendenti.Where(x => x.matricola == "0" + DBdipendente.matricola).FirstOrDefault();
                    if (GAPPdipendente == null)
                    {
                        MatricoleNonInQuestaSedeGapp.Add(DBdipendente.matricola);
                    }
                }
                if (MatricoleNonInQuestaSedeGapp.Any())
                {
                    foreach (string m in MatricoleNonInQuestaSedeGapp)
                    {
                        dipendenti.RemoveAll(x => x.matricola == m);
                        MyRai_LogAzioni A = new MyRai_LogAzioni()
                        {
                            applicativo = "PORTALE",
                            data = DateTime.Now,
                            matricola = matricola,
                            provenienza = "GeneraPdfPianoFerie",
                            operazione = "AllineamentoPF",
                            descrizione_operazione = m + " scartata da PF (DB sede " + sedeCodice + " non per gapp) "
                        };
                        db.MyRai_LogAzioni.Add(A);
                    }
                    db.SaveChanges();
                }

                for (int mese = 1; mese <= 12; mese++)
                {
                    document.Add(GetTableMeseDipendenti(anno, mese, dipendenti, resp, cbb, Images, Lrip, modificaDB));
                }


                List<MyRai_PianoFerie> DipRichiedenti2021 = new List<MyRai_PianoFerie>();
                //CheckAndAddTables2021(sedeCodice, matricola, modificaDB, wsService, document, cbb, Images, dipendenti);


                var Table2 = GetTable();
                Table2.AddCell(EmptyCell);
                Table2.AddCell(new PdfPCell(new Phrase("DETTAGLIO PER DIPENDENTE", FontTestoSmall)) { Colspan = 2, HorizontalAlignment = Element.ALIGN_CENTER, Border = 0 });
                Table2.AddCell(EmptyCell);
                document.Add(Table2);

                MyRaiServiceInterface.it.rai.servizi.digigappws.pianoFerieSedeGapp resp2021 = null;
                string d = string.Join(",", dipendenti.Select(x => x.matricola).OrderBy(x => x).ToArray());

                foreach (var dip in dipendenti.OrderBy(x => x.matricola))
                {
                    System.Diagnostics.Debug.Print(dip.matricola);
                    if (dip.matricola == "770660")
                    {

                    }
                    string nominativo = GetNominativo(dip.matricola, resp);
                    
                    document.Add(GetTableYearmatricola(anno, dip.matricola, nominativo, cbb, resp, Images, Lrip, modificaDB));
                    //if (DipRichiedenti2021.Select(x => x.matricola).Contains(dip.matricola))
                    //{
                    //    if (resp2021 == null)
                    //        resp2021 = wsService.getPianoFerieSedeGapp("P" + matricola, "01012021", "", "",
                    //                    sedeCodice.ToUpper().Substring(0, 5), 75);

                    //    document.Add(GetTableYearmatricola(2021, dip.matricola, GetNominativo(dip.matricola, resp2021),
                    //       cbb, resp2021, Images, new List<MyRai_PianoFerieBatch>(), modificaDB));
                    //}
                }


                if (!modificaDB)
                {


                    if (!Lrip.Any())
                        document.Add(new Phrase("\r\nNESSUNA RIPIANIFICAZIONE", FontGrassetto));
                    else
                    {
                        PdfPTable TableRip = GetTableRip(Lrip, FontTestoSmall, resp, FontGrassetto);
                        document.Add(TableRip);
                    }
                }


                document.Add(new Phrase("    ", FontTestoSmall));
                PdfPTable TableLegenda = GetTableLegenda(cbb, FontTestoSmall, Images);
                document.Add(TableLegenda);


                if (!modificaDB)
                    document.Add(new Phrase("\r\nPDF Aggiornato al " + DateTime.Now.ToString("dd/MM/yyyy HH.mm"), FontTestoSmall));

                document.Close();
                writer.Close();
                byte[] buff = stream.ToArray();


                return buff;
            }
        }

        private static List<MyRai_PianoFerie> CheckAndAddTables2021(string sedeCodice, string matricola, bool modificaDB, MyRaiServiceInterface.it.rai.servizi.digigappws.WSDigigapp wsService, Document document, PdfContentByte cbb, PianoFeriePDFImages Images, List<MyRai_PianoFerie> dipendenti)
        {
            List<MyRai_PianoFerie> DipExt2021 = new List<MyRai_PianoFerie>();
            foreach (var d in dipendenti)
            {
                if (Richiede2021(d.matricola))
                {
                    DipExt2021.Add(d);
                }
            }
            if (DipExt2021.Any())
            {
                MyRaiServiceInterface.it.rai.servizi.digigappws.pianoFerieSedeGapp resp2021 =
                    wsService.getPianoFerieSedeGapp("P" + matricola, "01012021", "", "",
                    sedeCodice.ToUpper().Substring(0, 5), 75);

                for (int mese = 1; mese <= 3; mese++)
                {
                    document.Add(GetTableMeseDipendenti(2021, mese, DipExt2021, resp2021, cbb, Images,
                        new List<MyRai_PianoFerieBatch>(), modificaDB));
                }
            }
            return DipExt2021;
        }

        private static PdfPTable GetTableRip(List<MyRai_PianoFerieBatch> lrip, Font FontTestoSmall, MyRaiServiceInterface.it.rai.servizi.digigappws.pianoFerieSedeGapp resp, Font FontGrassetto)
        {
            Font FontCella = FontFactory.GetFont(FontFactory.COURIER, 8, iTextSharp.text.Font.BOLD);
            Font FontCellaPlus = FontFactory.GetFont(FontFactory.COURIER, 8, iTextSharp.text.Font.BOLD);

            var TableRip = new PdfPTable(5);
            TableRip.SetWidths(new int[] { 10, 40, 10, 15, 15 });

            TableRip.WidthPercentage = 100;
            TableRip.DefaultCell.Padding = 1;
            TableRip.DefaultCell.Border = 0;
            TableRip.DefaultCell.BorderWidthTop = 0;
            TableRip.DefaultCell.BorderWidthLeft = 0;
            TableRip.DefaultCell.BorderWidthRight = 0;
            TableRip.DefaultCell.BorderWidthBottom = 0;


            TableRip.HorizontalAlignment = 0;
            TableRip.SpacingAfter = 10;

            PdfPCell Intestazione = new PdfPCell(new Phrase("RIPIANIFICAZIONI", FontGrassetto)) { Colspan = 5, BackgroundColor = BaseColor.LIGHT_GRAY, FixedHeight = 18 };
            TableRip.AddCell(Intestazione);





            PdfPCell pc = new PdfPCell(new Phrase("MATRICOLA", FontCellaPlus)) { FixedHeight = 18 };
            TableRip.AddCell(pc);

            pc = new PdfPCell(new Phrase("NOMINATIVO", FontCellaPlus)) { FixedHeight = 18 };
            TableRip.AddCell(pc);

            pc = new PdfPCell(new Phrase("ECCEZIONE", FontCellaPlus)) { FixedHeight = 18 };
            TableRip.AddCell(pc);

            pc = new PdfPCell(new Phrase("DATA", FontCellaPlus)) { FixedHeight = 18 };
            TableRip.AddCell(pc);

            pc = new PdfPCell(new Phrase("RIPIAN. DAL", FontCellaPlus)) { FixedHeight = 18 };
            TableRip.AddCell(pc);

            foreach (var r in lrip.OrderBy(x => x.matricola).ThenBy(x => x.data_eccezione))
            {
                string ripda = "";
                if (!String.IsNullOrWhiteSpace(r.provenienza) && r.provenienza.Length > 43)
                    ripda = r.provenienza.Substring(33, 10);

                // pc = new PdfPCell(new Phrase(r.matricola, FontCella));
                pc = new PdfPCell(new Phrase(new Chunk(r.matricola, FontCella).SetLocalGoto(r.matricola))) { FixedHeight = 18 };

                TableRip.AddCell(pc);

                pc = new PdfPCell(new Phrase(new Chunk(GetNominativo(r.matricola, resp), FontCella).SetLocalGoto(r.matricola))) { FixedHeight = 18 };
                TableRip.AddCell(pc);

                pc = new PdfPCell(new Phrase(r.codice_eccezione, FontCella)) { FixedHeight = 18 };
                TableRip.AddCell(pc);

                pc = new PdfPCell(new Phrase(r.data_eccezione.ToString("dd/MM/yyyy"), FontCella)) { FixedHeight = 18 };
                TableRip.AddCell(pc);

                pc = new PdfPCell(new Phrase(ripda, FontCella)) { FixedHeight = 18 };
                TableRip.AddCell(pc);
            }

            return TableRip;
        }

        private static List<MyRai_PianoFerieBatch> GetRipianificazioni(string sedeCodice, int anno)
        {
            var db = new digiGappEntities();
            string text = "PianoFerie " + anno + "-Ripianificato";
            var list = db.MyRai_PianoFerieBatch.Where(x => x.error == null &&
                                                        x.data_inserimento_db != null &&
                                                        x.data_inserimento_gapp != null &&
                                                        x.sedegapp == sedeCodice &&
                                                        x.data_eccezione.Year == anno &&
                                                        x.provenienza.StartsWith(text)
            ).ToList();
            foreach (var item in list)
            {
                DateTime D;
                if (DateTime.TryParseExact(item.provenienza.Substring(33, 10), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D))
                    item.DataProvenienzaRipianificazione = D;
            }
            return list;
        }

        public static PdfPTable GetTable32()
        {
            var Table = new PdfPTable(32);
            Table.SetWidths(new int[] { 10, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3 });

            Table.WidthPercentage = 100;
            Table.DefaultCell.Padding = 1;
            Table.DefaultCell.Border = 0;
            Table.DefaultCell.BorderWidthTop = 0;
            Table.DefaultCell.BorderWidthLeft = 0;
            Table.DefaultCell.BorderWidthRight = 0;
            Table.DefaultCell.BorderWidthBottom = 0;
            Table.SpacingAfter = 10;
            return Table;
        }


        public static List<DateTime> GetFEgapp(MyRaiServiceInterface.it.rai.servizi.digigappws.pianoFerieSedeGapp resp, string matricola, string ecc)
        {
            Dictionary<string, string> Tipi = new Dictionary<string, string>() {
                {"FE","D" }, {"RR","E" },   {"RN","7" }
            };
            List<DateTime> L = new List<DateTime>();
            var dipGapp = resp.dipendenti.Where(x => x.matricola == matricola.PadLeft(7, '0')).FirstOrDefault();
            if (dipGapp != null && dipGapp.ferie != null && dipGapp.ferie.giornate != null)
            {
                L = dipGapp.ferie.giornate.Where(x => x.tipoGiornata == Tipi[ecc]).Select(x => x.data).ToList();
            }
            return L;
        }
        public static PdfPTable GetTableMeseDipendenti(int anno, int mese, List<MyRai_PianoFerie> dipendenti, MyRaiServiceInterface.it.rai.servizi.digigappws.pianoFerieSedeGapp resp, PdfContentByte cbb, PianoFeriePDFImages Images, List<MyRai_PianoFerieBatch> lrip, bool modificaDB)
        {
            digiGappEntities db = new digiGappEntities();
            Font FontGrassetto = FontFactory.GetFont(FontFactory.COURIER, 10, iTextSharp.text.Font.BOLD);
            Font FontCella = FontFactory.GetFont(FontFactory.COURIER, 6, iTextSharp.text.Font.BOLD);



            var Table = GetTable32();

            DateTime Dapp = new DateTime(DateTime.Now.Year, mese, 1);
            PdfPCell Intestazione = new PdfPCell(new Phrase(Dapp.ToString("MMMM").ToUpper() + " " + anno, FontGrassetto)) { Colspan = 32, BackgroundColor = BaseColor.LIGHT_GRAY, FixedHeight = 18 };
            Table.AddCell(Intestazione);

            for (int i = 0; i <= 31; i++)
            {
                PdfPCell C = new PdfPCell(new Phrase(i < 1 ? "" : i.ToString(), FontCella)) { FixedHeight = 18 };
                Table.AddCell(C);
            }
            foreach (var dip in dipendenti.OrderBy(x => GetNominativo(x.matricola, resp)))
            {
                List<DateTime> GiorniFerieGapp = GetFEgapp(resp, dip.matricola, "FE");
                List<DateTime> GiorniRRGapp = GetFEgapp(resp, dip.matricola, "RR");
                List<DateTime> GiorniRNGapp = GetFEgapp(resp, dip.matricola, "RN");

                List<MyRai_PianoFerieGiorni> giorni = db.MyRai_PianoFerieGiorni.Where(x => x.data.Year == anno &&
                x.matricola == dip.matricola).ToList();

                List<DateTime> GiorniFerie = giorni.Where(x => x.eccezione == "FE").Select(x => x.data).ToList();
                List<DateTime> GiorniRR = giorni.Where(x => x.eccezione == "RR").Select(x => x.data).ToList();
                List<DateTime> GiorniRN = giorni.Where(x => x.eccezione == "RN").Select(x => x.data).ToList();

                PdfPCell CellNominativo = new PdfPCell(new Phrase(GetNominativo(dip.matricola, resp), FontCella)) { FixedHeight = 18, VerticalAlignment = Element.ALIGN_MIDDLE };
                Table.AddCell(CellNominativo);
                for (int d = 1; d <= 31; d++)
                {
                    bool dayok = true;
                    DateTime Dday = new DateTime();
                    try
                    {
                        Dday = new DateTime(anno, mese, d);
                    }
                    catch (Exception ex)
                    {
                        dayok = false;
                    }

                    PdfPCell CellDay = null;


                    if (dayok)
                    {
                        if (modificaDB)
                        {
                            if (GiorniFerie.Contains(Dday) && !lrip.Any(x => x.matricola == dip.matricola && x.DataProvenienzaRipianificazione == Dday && x.codice_eccezione == "FE"))
                                CellDay = new PdfPCell(Images.ImageFEPF) { FixedHeight = 18 };
                            else if (GiorniFerieGapp.Contains(Dday))
                                CellDay = new PdfPCell(Images.ImageFEGapp) { FixedHeight = 18 };
                            else if (GiorniRRGapp.Contains(Dday))
                                CellDay = new PdfPCell(Images.ImageRRGapp) { FixedHeight = 18 };
                            else if (GiorniRNGapp.Contains(Dday))
                                CellDay = new PdfPCell(Images.ImageRNGapp) { FixedHeight = 18 };
                            else if (GiorniRR.Contains(Dday) && !lrip.Any(x => x.matricola == dip.matricola && x.DataProvenienzaRipianificazione == Dday && x.codice_eccezione == "RR"))
                                CellDay = new PdfPCell(Images.ImageRRPF) { FixedHeight = 18 };
                            else if (GiorniRN.Contains(Dday) && !lrip.Any(x => x.matricola == dip.matricola && x.DataProvenienzaRipianificazione == Dday && x.codice_eccezione == "RN"))
                                CellDay = new PdfPCell(Images.ImageRNPF) { FixedHeight = 18 };
                            else
                                CellDay = new PdfPCell(new Phrase("")) { FixedHeight = 18 };
                        }
                        else
                        {
                            if (GiorniFerieGapp.Contains(Dday))
                                CellDay = new PdfPCell(Images.ImageFEGapp) { FixedHeight = 18 };
                            else if (GiorniFerie.Contains(Dday) && !lrip.Any(x => x.matricola == dip.matricola && x.DataProvenienzaRipianificazione == Dday && x.codice_eccezione == "FE"))
                                CellDay = new PdfPCell(Images.ImageFEPF) { FixedHeight = 18 };
                            else if (GiorniRRGapp.Contains(Dday))
                                CellDay = new PdfPCell(Images.ImageRRGapp) { FixedHeight = 18 };
                            else if (GiorniRNGapp.Contains(Dday))
                                CellDay = new PdfPCell(Images.ImageRNGapp) { FixedHeight = 18 };
                            else if (GiorniRR.Contains(Dday) && !lrip.Any(x => x.matricola == dip.matricola && x.DataProvenienzaRipianificazione == Dday && x.codice_eccezione == "RR"))
                                CellDay = new PdfPCell(Images.ImageRRPF) { FixedHeight = 18 };
                            else if (GiorniRN.Contains(Dday) && !lrip.Any(x => x.matricola == dip.matricola && x.DataProvenienzaRipianificazione == Dday && x.codice_eccezione == "RN"))
                                CellDay = new PdfPCell(Images.ImageRNPF) { FixedHeight = 18 };
                            else
                                CellDay = new PdfPCell(new Phrase("")) { FixedHeight = 18 };
                        }


                        if (Dday.DayOfWeek == DayOfWeek.Sunday || Dday.DayOfWeek == DayOfWeek.Saturday)
                            CellDay.BackgroundColor = new BaseColor(220, 220, 220);
                    }
                    else
                    {
                        CellDay = new PdfPCell(new Phrase("")) { FixedHeight = 18 };
                        CellDay.BackgroundColor = new BaseColor(240, 240, 240);
                    }

                    Table.AddCell(CellDay);
                }
            }
            return Table;
        }
        public static PdfPTable GetTableYearmatricola(int anno, string matricola, string nominativo, PdfContentByte cbb, MyRaiServiceInterface.it.rai.servizi.digigappws.pianoFerieSedeGapp resp, PianoFeriePDFImages Images, List<MyRai_PianoFerieBatch> lrip, bool modificaDB)
        {

            digiGappEntities db = new digiGappEntities();
            var giorni = db.MyRai_PianoFerieGiorni.Where(x => x.data.Year == anno && x.matricola == matricola).ToList();

            List<DateTime> GiorniFerie = giorni.Where(x => x.eccezione == "FE").Select(x => x.data).ToList();
            List<DateTime> GiorniRR = giorni.Where(x => x.eccezione == "RR").Select(x => x.data).ToList();
            List<DateTime> GiorniRN = giorni.Where(x => x.eccezione == "RN").Select(x => x.data).ToList();
            List<DateTime> GiorniFerieGapp = GetFEgapp(resp, matricola, "FE");
            List<DateTime> GiorniRRGapp = GetFEgapp(resp, matricola, "RR");
            List<DateTime> GiorniRNGapp = GetFEgapp(resp, matricola, "RN");

            Font FontGrassetto = FontFactory.GetFont(FontFactory.COURIER, 10, iTextSharp.text.Font.BOLD);
            Font FontCella = FontFactory.GetFont(FontFactory.COURIER, 6, iTextSharp.text.Font.BOLD);

            var Table = GetTable32();

            PdfPCell Intestazione = new PdfPCell(new Phrase(matricola + " - " + nominativo.ToUpper()
                + (anno == 2021 ? " (2021)" : "")
                , FontGrassetto))
            { Colspan = 32, BackgroundColor = BaseColor.LIGHT_GRAY, FixedHeight = 18 };
            Table.AddCell(Intestazione);

            for (int i = 0; i <= 31; i++)
            {

                PdfPCell C = new PdfPCell(new Phrase(new Chunk(i < 1 ? "" : i.ToString(), FontCella).SetLocalDestination(matricola))) { FixedHeight = 18 };
                Table.AddCell(C);
            }
            for (int m = 1; m <= (anno == 2021 ? 3 : 12); m++)
            {
                DateTime D = new DateTime(anno, m, 1);
                PdfPCell Cellmese = new PdfPCell(new Phrase(D.ToString("MMMM").ToUpper(), FontCella)) { FixedHeight = 18, VerticalAlignment = Element.ALIGN_MIDDLE };
                Table.AddCell(Cellmese);

                for (int d = 1; d <= 31; d++)
                {

                    bool dayok = true;
                    DateTime Dday = new DateTime();
                    try
                    {
                        Dday = new DateTime(anno, m, d);
                    }
                    catch (Exception ex)
                    {
                        dayok = false;
                    }

                    PdfPCell CellDay = null;


                    if (dayok)
                    {
                        if (modificaDB)
                        {
                            if (GiorniFerie.Contains(Dday) && !lrip.Any(x => x.matricola == matricola && x.DataProvenienzaRipianificazione == Dday && x.codice_eccezione == "FE"))
                                CellDay = new PdfPCell(Images.ImageFEPF) { FixedHeight = 18 };
                            else if (GiorniFerieGapp.Contains(Dday))
                                CellDay = new PdfPCell(Images.ImageFEGapp) { FixedHeight = 18 };
                            else if (GiorniRRGapp.Contains(Dday))
                                CellDay = new PdfPCell(Images.ImageRRGapp) { FixedHeight = 18 };
                            else if (GiorniRNGapp.Contains(Dday))
                                CellDay = new PdfPCell(Images.ImageRNGapp) { FixedHeight = 18 };
                            else if (GiorniRR.Contains(Dday) && !lrip.Any(x => x.matricola == matricola && x.DataProvenienzaRipianificazione == Dday && x.codice_eccezione == "RR"))
                                CellDay = new PdfPCell(Images.ImageRRPF) { FixedHeight = 18 };
                            else if (GiorniRN.Contains(Dday) && !lrip.Any(x => x.matricola == matricola && x.DataProvenienzaRipianificazione == Dday && x.codice_eccezione == "RN"))
                                CellDay = new PdfPCell(Images.ImageRNPF) { FixedHeight = 18 };
                            else
                                CellDay = new PdfPCell(new Phrase("")) { FixedHeight = 18 };

                        }
                        else
                        {
                            if (GiorniFerieGapp.Contains(Dday))
                                CellDay = new PdfPCell(Images.ImageFEGapp) { FixedHeight = 18 };

                            else if (GiorniFerie.Contains(Dday) && !lrip.Any(x => x.matricola == matricola && x.DataProvenienzaRipianificazione == Dday && x.codice_eccezione == "FE"))
                                CellDay = new PdfPCell(Images.ImageFEPF) { FixedHeight = 18 };

                            else if (GiorniRRGapp.Contains(Dday))
                                CellDay = new PdfPCell(Images.ImageRRGapp) { FixedHeight = 18 };
                            else if (GiorniRNGapp.Contains(Dday))
                                CellDay = new PdfPCell(Images.ImageRNGapp) { FixedHeight = 18 };
                            else if (GiorniRR.Contains(Dday) && !lrip.Any(x => x.matricola == matricola && x.DataProvenienzaRipianificazione == Dday && x.codice_eccezione == "RR"))
                                CellDay = new PdfPCell(Images.ImageRRPF) { FixedHeight = 18 };
                            else if (GiorniRN.Contains(Dday) && !lrip.Any(x => x.matricola == matricola && x.DataProvenienzaRipianificazione == Dday && x.codice_eccezione == "RN"))
                                CellDay = new PdfPCell(Images.ImageRNPF) { FixedHeight = 18 };
                            else
                                CellDay = new PdfPCell(new Phrase("")) { FixedHeight = 18 };

                        }


                        if (Dday.DayOfWeek == DayOfWeek.Sunday || Dday.DayOfWeek == DayOfWeek.Saturday)
                            CellDay.BackgroundColor = new BaseColor(220, 220, 220);
                    }
                    else
                    {
                        CellDay = new PdfPCell(new Phrase("")) { FixedHeight = 18 };
                        CellDay.BackgroundColor = new BaseColor(240, 240, 240);
                    }

                    Table.AddCell(CellDay);
                }
            }
            return Table;
        }


        public static string GeneraPdfSostitutivo(string sedeGapp, DateTime d1, DateTime d2, string fileTemplate, string[] coord, string PathPdfEsistente)
        {
            var db = new digiGappEntities();
            if (db.DIGIRESP_Archivio_PDF.Any(x => x.tipologia_pdf == "R" && x.sede_gapp == sedeGapp && x.data_inizio == d1 && x.data_fine == d2))
            {
                return ("File già esistente - PDF sede:" + sedeGapp + " dal " + d1.ToString("dd/MM/yyyy") + " al " + d2.ToString("dd/MM/yyyy"));
            }
            if (!string.IsNullOrWhiteSpace(PathPdfEsistente))
            {
                byte[] buff = System.IO.File.ReadAllBytes(PathPdfEsistente);

                DIGIRESP_Archivio_PDF PdfRow = new DIGIRESP_Archivio_PDF()
                {
                    attivo = true,
                    data_fine = d2,
                    data_inizio = d1,
                    data_stampa = DateTime.Now,
                    da_rigenerare = false,
                    matricola_stampa = "ESSWEB",
                    numero_versione = 1,
                    stato_pdf = "S_OK",
                    tipologia_pdf = "R",
                    sede_gapp = sedeGapp,
                    pdf = buff
                };
                db.DIGIRESP_Archivio_PDF.Add(PdfRow);
                db.SaveChanges();
                return null;
            }
            try
            {
                using (var reader = new PdfReader(fileTemplate))
                {
                    using (var stream = new MemoryStream())
                    {
                        var document = new Document(reader.GetPageSizeWithRotation(1));
                        var writer = PdfWriter.GetInstance(document, stream);

                        document.Open();

                        for (var i = 1; i <= reader.NumberOfPages; i++)
                        {
                            document.NewPage();

                            var baseFont = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                            var importedPage = writer.GetImportedPage(reader, i);

                            var contentByte = writer.DirectContent;

                            contentByte.BeginText();
                            contentByte.SetFontAndSize(baseFont, 12);
                            contentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, sedeGapp.ToUpper(), Convert.ToInt32(coord[0]), Convert.ToInt32(coord[1]), 0);
                            contentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, d1.ToString("dd/MM/yyyy") + " - " + d2.ToString("dd/MM/yyyy"), Convert.ToInt32(coord[2]), Convert.ToInt32(coord[3]), 0);
                            contentByte.EndText();

                            contentByte.AddTemplate(importedPage, 0, 0);
                        }

                        document.Close();
                        writer.Close();

                        DIGIRESP_Archivio_PDF PdfRow = new DIGIRESP_Archivio_PDF()
                        {
                            attivo = true,
                            data_fine = d2,
                            data_inizio = d1,
                            data_stampa = DateTime.Now,
                            da_rigenerare = false,
                            matricola_stampa = "ESSWEB",
                            numero_versione = 1,
                            stato_pdf = "S_OK",
                            tipologia_pdf = "R",
                            sede_gapp = sedeGapp,
                            pdf = stream.ToArray()
                        };
                        db.DIGIRESP_Archivio_PDF.Add(PdfRow);
                        db.SaveChanges();
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }

        }

        public static byte[] GetFoto(string matricola, enumPhotoFormat formato)
        {
            return leggiFotoDaDb(matricola, formato);
        }

        private static byte[] leggiFotoDaDb(string matricola, enumPhotoFormat tipoFoto)
        {
            HRGBEntities db = new HRGBEntities();
            //var anagrafica = db.Anagrafica_Foto.Where(a => a.Matricola == matricola).FirstOrDefault();
            //if (anagrafica == null)
            //    // se non trova la matricola prende l'immagine prefinita
            //    anagrafica = db.Anagrafica_Foto.Where(a => a.Matricola == "000000").FirstOrDefault();
            //byte[] rit = null;
            //if (anagrafica != null)
            //{
            //    switch (tipoFoto)
            //    {
            //        case enumPhotoFormat.Original:
            //            rit = anagrafica.Foto;
            //            break;
            //        case enumPhotoFormat.Medium:
            //            rit = anagrafica.Foto_Media;
            //            break;
            //        case enumPhotoFormat.Small:
            //            rit = anagrafica.Foto_Piccola;
            //            break;
            //    }
            //}
            //return rit;

            if (!db.Anagrafica_Foto.Any(a => a.Matricola == matricola))
            {
                matricola = "000000";
            }
            byte[] rit = null;
            var qry = db.Anagrafica_Foto.Where(a => a.Matricola == matricola);
            switch (tipoFoto)
            {
                case enumPhotoFormat.Original:
                    rit = qry.Select(a => a.Foto).FirstOrDefault();
                    break;
                case enumPhotoFormat.Medium:
                    rit = qry.Select(a => a.Foto_Media).FirstOrDefault();
                    break;
                case enumPhotoFormat.Small:
                    rit = qry.Select(a => a.Foto_Piccola).FirstOrDefault();
                    break;
            }
            return rit;
        }



    }
    public enum AsmxServices
    {
        HRGA,
        Digigapp,
        HRGB
    }
    public class PianoFeriePDFImages
    {
        public PianoFeriePDFImages(PdfContentByte cb)
        {
            cbb = cb;
            ImageFEPF = getImageFerie(cbb, "F");
            ImageRRPF = getImageFerie(cbb, "R");
            ImageRNPF = getImageFerie(cbb, "N");

            ImageFEGapp = getImageFerie(cbb, "FE", true);
            ImageRRGapp = getImageFerie(cbb, "RR", true);
            ImageRNGapp = getImageFerie(cbb, "RN", true);
        }
        public PdfContentByte cbb { get; set; }
        public Image ImageFEPF { get; set; }
        public Image ImageRRPF { get; set; }
        public Image ImageRNPF { get; set; }

        public Image ImageFEGapp { get; set; }
        public Image ImageRRGapp { get; set; }
        public Image ImageRNGapp { get; set; }

        private Image getImageFerie(PdfContentByte cbb, string Ecc, bool GiornoSuGapp = false)
        {
            BaseColor colorFEgapp = new BaseColor(72, 191, 255);
            BaseColor colorRRgapp = new BaseColor(234, 169, 33);
            BaseColor colorRNgapp = new BaseColor(0, 170, 0);

            BaseColor colorEccGapp = null;
            if (Ecc == "FE")
            {
                colorEccGapp = colorFEgapp;
                Ecc = "";
            }
            if (Ecc == "RR")
            {
                colorEccGapp = colorRRgapp;
                Ecc = "";
            }
            if (Ecc == "RN")
            {
                colorEccGapp = colorRNgapp;
                Ecc = "";
            }
            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

            PdfTemplate template = cbb.CreateTemplate(30, 30);
            if (!GiornoSuGapp)
                template.SetColorFill(new BaseColor(0, 255, 0));
            else
                template.SetColorFill(colorEccGapp);

            template.Circle(12, 12, 6f);
            if (!GiornoSuGapp)
                template.SetColorStroke(new BaseColor(0, 240, 0));
            else
                template.SetColorStroke(colorEccGapp);

            template.FillStroke();
            template.BeginText();
            template.SetFontAndSize(bf, 12);
            template.SetColorStroke(BaseColor.WHITE);
            template.SetColorFill(BaseColor.WHITE);
            template.ShowTextAligned(1, Ecc, 12, 8, 0);
            template.EndText();

            return Image.GetInstance(template);

        }



    }

    public class PdfStartEnd
    {
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public Boolean IsLastSegmentPerMonth { get; set; }
        public Boolean IsFirstSegmentPerMonth { get; set; }
        public Boolean ContainsEndMonth { get; set; }
    }

    public class EsitoPDF
    {
        public int idPdf { get; set; }
        public string errore { get; set; }
    }

    public class Abilitazioni
    {
        public Abilitazioni()
        {
            ListaAbilitazioni = new List<AbilitazioneSede>();
        }
        public List<AbilitazioneSede> ListaAbilitazioni { get; set; }
    }

    public class AbilitazioneSede
    {
        public AbilitazioneSede()
        {
            MatrLivello1 = new List<MatrSedeAppartenenza>();
            MatrLivello2 = new List<MatrSedeAppartenenza>();
        }
        public string Sede { get; set; }
        public string DescrSede { get; set; }
        public List<MatrSedeAppartenenza> MatrLivello1 { get; set; }
        public List<MatrSedeAppartenenza> MatrLivello2 { get; set; }
    }

    public class MatrSedeAppartenenza
    {
        public string Matricola { get; set; }
        public string SedeAppartenenza { get; set; }
        public string Delegante { get; set; }
        public string Delegato { get; set; }
    }

    public class SollecitoAppr
    {
        public string sedeGapp { get; set; }
        public int RichiesteUrg { get; set; }
        public int RichiesteSca { get; set; }
        public int RichiesteOrd { get; set; }
        public int RichiesteUrgS { get; set; }
        public int RichiesteScaS { get; set; }
        public int RichiesteOrdS { get; set; }
        public IEnumerable<MyRai_Richieste> Richieste { get; set; }
    }
    public class Payload
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
    public class PayloadAdvanced
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("attachments")]
        public Attachment[] Attachments { get; set; }
    }
    public class Attachment
    {
        [JsonProperty("color")]
        public string Color { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }


}