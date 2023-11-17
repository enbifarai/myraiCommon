using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using myRai.Business;
using myRai.DataControllers;
using myRai.Models;
using myRaiCommonModel;
using myRaiData;
using myRaiHelper;
using MyRaiServiceInterface.it.rai.servizi.digigappws;

namespace myRaiCommonManager
{
    public class ResocontiManager
    {
        public enum StatoPdfResoconto
        {
            PresenteStaBeneCosi,
            PresenteDaRigenerare,
            NonPresenteDaGenerare,
            NonPresenteMaMieiRepartiOK
        }

        public static presenzeResponse GetPresenzeFromDB(string sede, DateTime DataDa, DateTime DataA)
        {
            var db = new myRaiData.digiGappEntities();
            var row = db.MyRai_Resoconti_GetPresenze.Where(x => x.sede == sede && x.data_inizio == DataDa && x.data_fine == DataA).FirstOrDefault();
            if (row == null) return null;

            presenzeResponse pres = Newtonsoft.Json.JsonConvert.DeserializeObject<presenzeResponse>(row.contenuto);
            return pres;
        }

        public static ResocontiModel GetResocontiModel(string matricola, string datada = null, string dataa = null, bool avanti = true, bool onlypreview = true, 
            string sediVisualizzate = null, string sederichiesta=null)
        {
            var db = new myRaiData.digiGappEntities();

            WSDigigapp service = new WSDigigapp()
            {
                Credentials = CommonHelper.GetUtenteServizioCredentials()
            };

            ResocontiModel model = new ResocontiModel();
            model.onlypreview = onlypreview;

            DateTime dateStartWeek;
            DateTime dateEndWeek;

            List<Sede> sedi = Utility.GetSediGappResponsabileList();

            model.SediPresenze = new List<SedePresenzeModel>();

            int gg = CommonManager.GetParametro<int>(EnumParametriSistema.ApprovaPresenzeDopoGG);
            foreach (var sede in sedi.OrderBy(x=>x.CodiceSede))
            {
                if (!String.IsNullOrWhiteSpace(sederichiesta) && sede.CodiceSede != sederichiesta)
                    continue;

                if (datada == null)
                {
                    DateTime[] Da = CommonManager.GetIntervalloPerResocontiLiv1(sede.CodiceSede);
                    dateStartWeek = Da[0];
                    dateEndWeek = Da[1];
                }
                else
                {
                    DateTime.TryParseExact(datada, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dateStartWeek);
                    DateTime.TryParseExact(dataa, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dateEndWeek);
                    if (avanti)
                    {
                        dateStartWeek = dateStartWeek.AddDays(7);
                        dateEndWeek = dateEndWeek.AddDays(7);
                    }
                    else
                    {
                        dateStartWeek = dateStartWeek.AddDays(-7);
                        dateEndWeek = dateEndWeek.AddDays(-7);
                    }
                }
                SedePresenzeModel s = new SedePresenzeModel();
                s.CodiceSede = sede.CodiceSede;
                s.DescrizioneSede = sede.DescrizioneSede;
                s.DataDa = dateStartWeek;
                s.DataA = dateEndWeek;
                s.IsNextWeekBrowsable = dateEndWeek.AddDays(7) < DateTime.Today;
                s.IsPDFapprovabile = s.DataA.AddDays(gg) < DateTime.Now;

                List<string> MatricoleRiepSett = new List<string>();



                s.presenze = GetPresenzeFromDB(sede.CodiceSede, dateStartWeek, dateEndWeek);
                bool fromDB = false;

                if (s.presenze == null)
                {
                    Logger.LogAzione(new MyRai_LogAzioni()
                    {
                        applicativo = "PORTALE",
                        data = DateTime.Now,
                        descrizione_operazione = "GetPresenzeFromDB " + sede.CodiceSede + " " + dateStartWeek.ToString("ddMMyyyy") + "-" + dateEndWeek.ToString("ddMMyyyy"),
                        matricola = CommonManager.GetCurrentUserMatricola(),
                        provenienza = "GetResocontiModel",
                        operazione = "GetPresenzeFromDB"
                    });

                    s.presenze = service.getPresenzeNoPDF(CommonManager.GetCurrentUserMatricola(), "*",
                    dateStartWeek.ToString("ddMMyyyy"),
                    dateEndWeek.ToString("ddMMyyyy"),
                    sede.CodiceSede, 75, "**");
                }
                else fromDB = true;

                MatricoleRiepSett = s.presenze.periodi.Select(x => x.dipendente.matricola).ToList();

                var pdf = db.DIGIRESP_Archivio_PDF.Where(x => x.sede_gapp == sede.CodiceSede
                                 && x.data_inizio == dateStartWeek
                                 && x.data_fine == dateEndWeek
                                 && x.tipologia_pdf == "P")
                                 .Select(x=>new {data_stampa=x.data_stampa, data_convalida=x.data_convalida })
                                 .FirstOrDefault();

                s.IsPDFpresent = pdf != null;
                s.DataPDFgenerato = pdf != null ? pdf.data_stampa : null;
                s.DataPDFfirmato = pdf != null ? pdf.data_convalida : null;
                NeedToReprint N = ResocontiCheckerManager.IsToReprint(dateStartWeek, dateEndWeek, sede.CodiceSede);


                s.PDFdaRigenerare = N.Esito;
                if (s.PDFdaRigenerare)
                {
                    s.DifferenzeCachePDF = N.DettaglioDiff;
                    foreach (var diff in N.DettaglioDiff)
                    {
                        var per = s.presenze.periodi.Where(x => x.dipendente.matricola == diff.matricola).FirstOrDefault();
                        if (per != null)
                            if (per.deltaTotale!= diff.deltaTotalePDF)
                                per.DeltaTotale_OldVersion = diff.deltaTotalePDF;
                    }
                }


                if (onlypreview == false)
                {
                    List<DateTime> DateInizio = QuantiPdfDaGenerare(matricola, sede.CodiceSede, MatricoleRiepSett);

                    s.DateConPDFDaRigenerareOFirmare = new List<DateTime>();

                    s.DateConPDFDaRigenerareOFirmare = DateInizio;

                    s.PdfAncoraDaGenerare = DateInizio.Count();

                    s.PDFdaRigenerare = DateInizio.Contains(dateStartWeek);
                }

               

                DateTime? DataBlocco = GetDataBloccoInizialePresenze(sede.CodiceSede);
                s.IsCurrentWeekPrimaDelBlocco = (DataBlocco == null ? true : s.DataDa < DataBlocco);

                s.SonoResponsabileDiReparti = RepartiManager.SonoResponsabileDiReparti(sede.CodiceSede);
                s.MieiRepartiGiaVisionati = RepartiManager.MieiRepartiGiaVisionati(sede.CodiceSede, dateStartWeek, dateEndWeek);
                s.MieiReparti = RepartiManager.RepartiDiCuiSonoResponsabile(sede.CodiceSede);
                s.MieiRepartiDes = new Dictionary<string, string>();

                if (s.SonoResponsabileDiReparti && s.MieiRepartiGiaVisionati && s.PdfAncoraDaGenerare > 0)
                    s.PdfAncoraDaGenerare--;

                if (s.MieiReparti.Count > 0)
                {
                    for (int i = 0; i < s.MieiReparti.Count; i++)
                        s.MieiReparti[i] = s.MieiReparti[i].Substring(5);
                    //   s.MieiReparti.ForEach(x => x=x.Substring(5));
                }
                s.MieiRepartiDes = new LinkedTableDataController().GetDettagliReparti(sede.CodiceSede,matricola);

                //s.PDFdaRigenerare = HasNewsPdf(sede.CodiceSede, dateStartWeek, dateEndWeek) == StatoPdfResoconto.PresenteDaRigenerare;

                //List<DateTime> DateInizio2= HowManyNewsPdf(sede.CodiceSede, dateStartWeek, dateEndWeek);
                //s.ConteggioPDFDaRigenerare = DateInizio2.Count();
                s.ConteggioPDFDaRigenerare = 0;

                //if (s.ConteggioPDFDaRigenerare + s.PdfAncoraDaGenerare == 1)
                //{
                //    DateInizio.AddRange(DateInizio2);
                //    s.DataInizioPDFseUnoSoloDaGenerare = DateInizio.FirstOrDefault();
                //}
                model.SediPresenze.Add(s);



            }
            return model;
        }

        public static DateTime? GetDataBloccoInizialePresenze(string sede)
        {
            var db = new myRaiData.digiGappEntities();

            DIGIRESP_Archivio_PDF PrimoPdfEccezioni = db.GetDIGIRESP_Archivio_PDF(sede, "R").FirstOrDefault();

            if (PrimoPdfEccezioni == null) return null;

            int dayStartPerSede = 1;
            var sedeg = db.L2D_SEDE_GAPP.Where(x => x.cod_sede_gapp == sede).FirstOrDefault();
            if (sedeg != null && sedeg.giorno_inizio_settimana != null) dayStartPerSede = (int)sedeg.giorno_inizio_settimana;

            DateTime Dstart = PrimoPdfEccezioni.data_inizio;
            if (dayStartPerSede == 7) dayStartPerSede = 0;

            if ((int)Dstart.DayOfWeek == dayStartPerSede)
            {
                //Dstart = Dstart.AddDays(7);
            }
            else
            {
                while ((int)Dstart.DayOfWeek != dayStartPerSede)
                {
                    Dstart = Dstart.AddDays(1);
                }
            }
            return Dstart;
        }

        public static DateTime? GetDataBloccoInizialePresenze2(string sede)
        {

            var db = new myRaiData.digiGappEntities();

            DIGIRESP_Archivio_PDF PrimoPdfEccezioni = db.GetDIGIRESP_Archivio_PDF(sede, "R").FirstOrDefault();

            DateTime current = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            int mesi = 3;

            var rec = db.MyRai_ParametriSistema.Where(w => w.Chiave.Equals("GetDataBloccoInizialePresenze")).FirstOrDefault();

            if (rec != null)
            {
                try
                {
                    mesi = int.Parse(rec.Valore1);
                }
                catch (Exception ex)
                {
                    mesi = 3;
                }
            }

            current = current.AddMonths(-mesi);

            if (PrimoPdfEccezioni == null) return null;

            int dayStartPerSede = 1;
            var sedeg = db.L2D_SEDE_GAPP.Where(x => x.cod_sede_gapp == sede).FirstOrDefault();
            if (sedeg != null && sedeg.giorno_inizio_settimana != null) dayStartPerSede = (int)sedeg.giorno_inizio_settimana;

            DateTime Dstart = PrimoPdfEccezioni.data_inizio;

            if (Dstart < current)
            {
                Dstart = current;
            }

            if (dayStartPerSede == 7) dayStartPerSede = 0;

            if ((int)Dstart.DayOfWeek == dayStartPerSede)
            {
                //Dstart = Dstart.AddDays(7);
            }
            else
            {
                while ((int)Dstart.DayOfWeek != dayStartPerSede)
                {
                    Dstart = Dstart.AddDays(1);
                }
            }
            return Dstart;
        }

        public static List<DateTime> QuantiPdfDaGenerare(string matricola, string sede, List<string> MatricoleRiepSettimanale=null)
        {
            var db = new myRaiData.digiGappEntities();
            int gg = CommonManager.GetParametro<int>(EnumParametriSistema.ApprovaPresenzeDopoGG);

            //DateTime? Dstart = GetDataBloccoInizialePresenze(sede);

            DateTime? Dstart = GetDataBloccoInizialePresenze2(sede);
            List<DateTime> missing = new List<DateTime>();

            if (Dstart == null) return missing;

            int day = (int)DateTime.Now.DayOfWeek;
            DateTime LastWeekEnd = DateTime.Today.AddDays(-day);
            DateTime LastWeekStart = LastWeekEnd.AddDays(-6);



            if (( MatricoleRiepSettimanale!=null && MatricoleRiepSettimanale.Any())|| HasQuadraturaSettimanale(matricola, sede))
            {
                while (Dstart <= LastWeekStart)
                {
                    if (((DateTime)Dstart).AddDays(6).AddDays(gg) <= DateTime.Today)
                    {

                        DateTime Dend = ((DateTime)Dstart).AddDays(6);
                        if (!db.DIGIRESP_Archivio_PDF.Any(x => x.data_inizio == Dstart && x.data_fine == Dend && x.sede_gapp == sede && x.tipologia_pdf == "P"))
                        {
                            if (RepartiManager.SonoResponsabileDiReparti(sede) && RepartiManager.MieiRepartiGiaVisionati(sede, (DateTime)Dstart, Dend))
                        {
                                //    'ttapposssst
                            }
                            else
                            missing.Add((DateTime)Dstart);
                        }
                        else
                        {
                            if (ResocontiCheckerManager.IsToReprint((DateTime)Dstart, Dend, sede).Esito)
                            {
                            missing.Add((DateTime)Dstart);
                        }
                    }
                    }

                    Dstart = ((DateTime)Dstart).AddDays(7);
                }
            }

            return missing;
        }

        private static bool HasQuadraturaSettimanale(string matricola, string sede)
        {
            bool result = false;

            try
            {
                MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client service = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();

                MyRaiServiceInterface.MyRaiServiceReference1.wApiUtilitypresenzeGiornaliere_resp resp =
                    service.presenzeGiornaliere(Utente.Matricola(), sede, DateTime.Today.ToString("ddMMyyyy"));

                if (resp.dati.Count() == 0)
                {
                    return result;
                }

                foreach (var item in resp.dati)
                {
                    var quad = GetQuadratura(item.tipoDip);

                    if (quad != null)
                    {
                        if (quad.GetValueOrDefault() == Quadratura.Settimanale)
                        {
                            result = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }

        private static Quadratura? GetQuadratura(string tipoDipendente)
        {
            string tipi = CommonManager.GetParametro<String>(EnumParametriSistema.TipiDipQuadraturaSettimanale);
            if (tipi != null && tipoDipendente != null && tipi.ToUpper().Contains(tipoDipendente.ToUpper()))
                return Quadratura.Settimanale;

            string tipiGiorn = CommonManager.GetParametro<String>(EnumParametriSistema.TipiDipQuadraturaGiornaliera);
            if (tipiGiorn != null && tipoDipendente != null && tipiGiorn.ToUpper().Contains(tipoDipendente.ToUpper()))
                return Quadratura.Giornaliera;

            return null;

        }

        /// <summary>
        /// Metodo che verifica se son presenti eccezioni approvate
        /// dopo la data di creazione del pdf, ma che son riferite al
        /// periodo esaminato.
        /// </summary>
        /// <param name="sede"></param>
        /// <param name="dataDa"></param>
        /// <param name="dataA"></param>
        /// <returns></returns>
        private static StatoPdfResoconto HasNewsPdf(string sede, DateTime dataDa, DateTime dataA, List<string> MatricoleRiepilogoSettimanale=null)
        {
            var db = new digiGappEntities();
            List<string> MieiReparti = new List<string>();
            if (RepartiManager.SonoResponsabileDiReparti(sede))
            {
                MieiReparti = RepartiManager.RepartiDiCuiSonoResponsabile(sede).Select(x => x.Substring(5)).ToList();
            }

            //var PDFresoconto = db.DIGIRESP_Archivio_PDF.Where(x => x.tipologia_pdf == "P" &&
            //                                                     x.sede_gapp == sede &&
            //                                                     x.data_inizio == dataDa &&
            //                                                     x.data_fine == dataA).FirstOrDefault();

            var PDFresoconto = db.GetDIGIRESP_Archivio_PDF_By_Date(sede, "P", dataDa, dataA).FirstOrDefault();

            if (PDFresoconto == null)
            {
                if (MieiReparti.Count == 0)
                    return StatoPdfResoconto.NonPresenteDaGenerare;
                else
                {
                    //bool MieiRepartiVisionati= db.MyRai_PdfReparti.Any(x => x.periodo_dal == dataDa && x.periodo_al == dataA && x.sedegapp == sede && MieiReparti.Contains(x.reparto));

                    var reparti = db.MyRai_PdfReparti.Where(x => x.sedegapp == sede && x.periodo_dal == dataDa && x.periodo_al == dataA).Select(x => x.reparto).ToList();
                    //db.GetReparti(sede, dataDa, dataA).ToList();

                    bool MieiRepartiVisionati = false;

                    if (reparti != null && reparti.Any())
                    {
                        // MieiRepartiVisionati = reparti.Any(w => MieiReparti.Contains(w));
                        MieiRepartiVisionati = MieiReparti.All(r => reparti.Contains(r));
                    }

                    if (MieiRepartiVisionati)
                        return StatoPdfResoconto.NonPresenteMaMieiRepartiOK;
                    else
                        return StatoPdfResoconto.NonPresenteDaGenerare;
                }
            }

            List<MyRai_Eccezioni_Richieste> myList = db.MyRai_Eccezioni_Richieste.Where(x => x.MyRai_Richieste.codice_sede_gapp == sede.Substring(0, 5) &&
                                                                                       (sede.Length > 5 ? x.MyRai_Richieste.reparto == sede.Substring(5) : true) &&
                                                                                        x.data_eccezione >= dataDa &&
                                                                                        x.data_eccezione <= dataA).ToList();
            List<string> MatricoleEsaminate = new List<string>();
            MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client wcf1 = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
            wcf1.ClientCredentials.Windows.ClientCredential = CommonHelper.GetUtenteServizioCredentials();

            foreach (var item in myList)
            {
                if (item.data_validazione_primo_livello == null && item.data_rifiuto_primo_livello == null) continue;
                if (item.data_validazione_primo_livello <= PDFresoconto.data_stampa || item.data_rifiuto_primo_livello <= PDFresoconto.data_stampa) continue;

                if (MatricoleEsaminate.Contains(item.MyRai_Richieste.matricola_richiesta)) continue;

                if (MatricoleRiepilogoSettimanale != null)
                {
                    if (MatricoleRiepilogoSettimanale.Contains(item.MyRai_Richieste.matricola_richiesta))
                    {
                        return StatoPdfResoconto.PresenteDaRigenerare;
                    }
                }
                else
                {
                    var resp = wcf1.GetRecuperaUtente(item.MyRai_Richieste.matricola_richiesta, DateTime.Today.ToString("ddMMyyyy"));
                    if (!MatricoleEsaminate.Contains(item.MyRai_Richieste.matricola_richiesta)) MatricoleEsaminate.Add(item.MyRai_Richieste.matricola_richiesta);

                    string tipoDipendente = resp.data.tipo_dipendente;

                    string tipi = CommonManager.GetParametro<String>(EnumParametriSistema.TipiDipQuadraturaSettimanale);

                    if (tipi != null && tipoDipendente != null && tipi.ToUpper().Contains(tipoDipendente.ToUpper()))
                    {
                        return StatoPdfResoconto.PresenteDaRigenerare;
                    }
                }
            }
            return StatoPdfResoconto.PresenteStaBeneCosi;
            ///////////////////////////////////////////////////////////////////////////////////////////////////////
        }

        public static List<DateTime> HowManyNewsPdf(string pMatricola, string sede, DateTime dataDa, DateTime dataA)
        {
            List<DateTime> result = new List<DateTime>();

            try
            {
                using (digiGappEntities db = new digiGappEntities())
                {
                    List<MyRai_Eccezioni_Richieste> myList = new List<MyRai_Eccezioni_Richieste>();

                    List<string> listaSedi = CommonHelper.GetSediL1(pMatricola);

                    if (listaSedi != null && listaSedi.Any())
                    {
                        foreach (var x in listaSedi)
                        {
                            if (x.Length > 5)
                            {
                                var temp = from eccRic in db.MyRai_Eccezioni_Richieste
                                           join ric in db.MyRai_Richieste
                                           on eccRic.id_richiesta equals ric.id_richiesta
                                           where ric.codice_sede_gapp.Equals(x.Substring(0, 5)) &&
                                                ric.reparto == x.Substring(5)
                                           select eccRic;

                                if (temp != null && temp.Any())
                                {
                                    foreach (var yy in temp)
                                    {
                                        myList.Add(yy);
                                    }
                                }
                            }
                            else
                            {
                                // prendo tutte le eccezioni la cui data_eccezione è inclusa nel range passato
                                // e che sono state approvate dopo la data di fine del range
                                myList = db.MyRai_Eccezioni_Richieste.Where(r => r.data_validazione_primo_livello != null &&
                                    r.codice_sede_gapp.Equals(sede) &&
                                    r.data_eccezione >= dataDa && r.data_eccezione <= dataA &&
                                    r.data_validazione_primo_livello > dataA).ToList();
                            }
                        }
                    }
                    else
                    {
                        // prendo tutte le eccezioni la cui data_eccezione è inclusa nel range passato
                        // e che sono state approvate dopo la data di fine del range
                        myList = db.MyRai_Eccezioni_Richieste.Where(r => r.data_validazione_primo_livello != null &&
                            r.codice_sede_gapp.Equals(sede) &&
                            r.data_eccezione >= dataDa && r.data_eccezione <= dataA &&
                            r.data_validazione_primo_livello > dataA).ToList();
                    }

                    // se sono presenti eccezioni che rispondono ai criteri definiti sopra
                    if (myList != null && myList.Any())
                    {
                        // per ogni eccezione verifichiamo se la data di stampa è minore alla data di approvazione
                        // Se si allora il pdf va rigenerato
                        foreach (var item in myList)
                        {
                            // recupero la richiesta a partire dal suo identificativo
                            var richiesta = db.MyRai_Richieste.Where(r => r.id_richiesta.Equals(item.id_richiesta)).FirstOrDefault();

                            if (richiesta != null)
                            {
                                string matricolaRichiedente = richiesta.matricola_richiesta;

                                MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client wcf1 = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
                                wcf1.ClientCredentials.Windows.ClientCredential = CommonHelper.GetUtenteServizioCredentials();

                                var resp = wcf1.GetRecuperaUtente(matricolaRichiedente, DateTime.Today.ToString("ddMMyyyy"));

                                string tipoDipendente = resp.data.tipo_dipendente;

                                Quadratura? q = null;

                                string tipi = CommonManager.GetParametro<String>(EnumParametriSistema.TipiDipQuadraturaSettimanale);

                                if (tipi != null && tipoDipendente != null && tipi.ToUpper().Contains(tipoDipendente.ToUpper()))
                                {
                                    q = Quadratura.Settimanale;
                                    // l'utente è a Quadratura Settimanale
                                    result = db.DIGIRESP_Archivio_PDF.Where(
                                        i => i.data_stampa != null &&
                                        i.data_inizio != null &&
                                        i.data_fine != null &&
                                        i.data_inizio == dataDa && i.data_fine == dataA &&
                                        i.sede_gapp == sede &&
                                        i.data_stampa < item.data_validazione_primo_livello &&
                                        i.tipologia_pdf == "P"
                                    ).Select(x => x.data_inizio).ToList();
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return result;
        }

        public static presenzeResponse GeneraPresenze(string sede, string da, string a)
        {
            bool generazione = RepartiManager.ServeGenerazionePdf(sede, da, a);

            if (generazione)
            {
                WSDigigapp service = new WSDigigapp()
                {
                    Credentials = CommonHelper.GetUtenteServizioCredentials()
                };
                var response = service.getPresenze(CommonHelper.GetCurrentUserMatricola(),
                     "*",
                     da,
                     a,
                     sede,
                     75,
                     "**");//tutti i reparti per ora
                if (response.esito == true && response.PDF != null && response.PDF.Length > 0)
                {
                    DateTime Da;
                    DateTime.TryParseExact(da, "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out Da);
                    DateTime A;
                    DateTime.TryParseExact(a, "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out A);

                    ResocontiCheckerManager.ClearAlert(sede, Da, A);
                }
                return response;
            }
            else
                return new presenzeResponse() { esito = true, errore = null };
        }

        public static bool GiaCopertaDaResoconto(string sede, DateTime data_dal, DateTime data_al, Boolean MarcaDaRigenerareSeNecessario)
        {
            var db = new digiGappEntities();
            //var pdf = db.DIGIRESP_Archivio_PDF.Where(x =>
            //    x.tipologia_pdf == "P" &&
            //    x.stato_pdf == "S_OK" &&
            //    x.sede_gapp == sede &&

            //    ((data_dal >= x.data_inizio && data_dal <= x.data_fine)
            //    ||
            //    (data_al >= x.data_inizio && data_al <= x.data_fine))

            //    ).FirstOrDefault();

            var pdf = db.GiaCopertaDaResoconto("P", "S_OK", sede, data_dal, data_al).FirstOrDefault();

            if (MarcaDaRigenerareSeNecessario && pdf != null)
            {
                pdf.da_rigenerare = true;
                db.SaveChanges();
            }
            return (pdf != null);
        }
    }
}