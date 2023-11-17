using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using myRai.DataAccess;
using System.Data;
using myRaiData;

using System.Web.Mvc;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using myRaiData.Incentivi;
using System.Xml.Serialization;
using System.Xml;
using Newtonsoft.Json;
using iTextSharp.text.pdf;
using System.IO;
using iTextSharp.text;
using System.Net;
using System.Web.Script.Serialization;
using myRaiHelper;
using myRaiCommonModel;
using myRaiServiceHub.Autorizzazioni;
using myRaiCommonModel.DocFirmaModels;
using myRaiServiceHub.it.rai.servizi.raiconnectcoll;
using myRai.Business;
using myRai.Models;

namespace myRaiCommonManager
{
    public class DaFirmareManager
    {


        public static TotaliDaFirmareModel GetTotaliDaFirmareModel(List<Sede> Sedi)
        {
            DaFirmareManager.CheckCarrello();

            if (Sedi == null) Sedi = GetDaFirmareModel();

            var TotaliDaFirmare = new TotaliDaFirmareModel();
            TotaliDaFirmare.RowPerSede = new List<TotDaFirmare>();
            var db = new digiGappEntities();
            string matr = CommonManager.GetCurrentUserMatricola();
            var tuttixmatricola = db.MyRai_Carrello.Where(x => x.matricola == matr).ToList();
            var sedim = Utility.GetSediGappResponsabileLiv2List();
            foreach (MyRai_Carrello car in tuttixmatricola)
            {
                if (sedim.Where(x => x.CodiceSede == car.DIGIRESP_Archivio_PDF.sede_gapp).Count() == 0)
                {
                    db.MyRai_Carrello.Remove(car);
                }
            }
            db.SaveChanges();


            var query = db.MyRai_Carrello.Where(x => x.matricola == matr)
                   .GroupBy(x => x.DIGIRESP_Archivio_PDF.sede_gapp)
                   .Select(y => new
                   {
                       sede = y.Key,
                       desc = db.L2D_SEDE_GAPP.Where(x => x.cod_sede_gapp == y.Key).Select(s => s.desc_sede_gapp).FirstOrDefault(),
                       quan = y.Count()
                   });
            foreach (var item in query)
            {

                TotaliDaFirmare.RowPerSede.Add(new TotDaFirmare()
                {
                    codiceSede = item.sede,
                    descSede = item.desc,
                    InCarrello = item.quan,
                    TotaleSede = Sedi.Where(x => x.CodiceSede == item.sede).Select(x => x.PeriodiPDF.Count()).FirstOrDefault()
                });

            }
            TotaliDaFirmare.Totale = TotaliDaFirmare.RowPerSede.Sum(x => x.InCarrello);

            TotaliDaFirmare.Totale += db.MyRai_CarrelloGenerico.Where(x => x.matricola == matr).Count();
            return TotaliDaFirmare;
        }

        public static List<String> GetMatricolaLivelloPerSede(string sedegapp, int livelloResponsabile_1_2)
        {
            CategorieDatoAbilitate response = CommonHelper.Get_CategoriaDato_Net_Cached(livelloResponsabile_1_2);
            //service.Get_CategoriaDato_Net("sedegapp", "", "HRUP", "0" + livelloResponsabile_1_2.ToString() + "GEST");
            var item = response.CategorieDatoAbilitate_Array
                .Where(sede => sede.Codice_categoria_dato.Trim().ToUpper() == sedegapp.Trim().ToUpper())
                       .FirstOrDefault();
            if (item == null) return null;

            List<string> Matricole = item.DT_Utenti_CategorieDatoAbilitate
                                         .AsEnumerable()
                                         .Select(p => (String)p.ItemArray[0]).ToList();
            return Matricole;
        }

        public static List<InfoSedeModel> GetSediDaFirmare()
        {
            List<InfoSedeModel> L = new List<InfoSedeModel>();

            Abilitazioni AB = CommonManager.getAbilitazioni();
            string matr = CommonManager.GetCurrentUserPMatricola();

            var sedi = AB.ListaAbilitazioni.Where(x => x.MatrLivello2.Any(a => a.Matricola == matr));
            foreach (var s in sedi)
            {
                L.Add(new InfoSedeModel()
                {
                    CodiceSede = s.Sede,
                    NomeSede = s.DescrSede,
                    TotaliSede = 0
                });
            }
            return L;

            //Autorizzazioni.Sedi SEDI2 = new Autorizzazioni.Sedi();

            //var profili = SEDI2.Get_ProfiliAssociati_Net(CommonManager.GetCurrentUserPMatricola(), "HRUP");
            //foreach (myRai.Autorizzazioni.Profilo profilo in profili.ProfiliArray)
            //{

            //    if (profilo.DT_ProfiliFunzioni.Rows.Count > 0)
            //    {
            //        string codSottofunzione = profilo.DT_ProfiliFunzioni.Rows[0]["Codice_sottofunzione"].ToString();

            //        foreach (System.Data.DataRow riga in profilo.DT_CategorieDatoAbilitate.Rows)
            //        {
            //            Boolean AccessoFirma = (codSottofunzione.Substring(0, 2) == "02");
            //            if (AccessoFirma)
            //            {
            //                InfoSedeModel i = new InfoSedeModel()
            //                {
            //                    CodiceSede = riga["Codice_categoria_dato"].ToString(),
            //                    NomeSede = riga["Descrizione_categoria_dato"].ToString(),
            //                    TotaliSede = 0
            //                };
            //                L.Add(i);
            //            }
            //        }
            //    }
            //}
            //return L;

        }
        public static RicercaPdfModel GetRicercaPdfModel()
        {
            //model.StatoGiornataList = new SelectList(
            //                  new List<SelectListItem>
            //                    {
            //                        new SelectListItem { Text = "", Value = ""},
            //                        new SelectListItem { Text = "ASSING", Value ="ASSING"},
            //                    }, "Value", "Text");

            List<SelectListItem> list = new List<SelectListItem>();
            //Autorizzazioni.Sedi service = new Autorizzazioni.Sedi();
            //Autorizzazioni.CategorieDatoAbilitate response = service.Get_CategoriaDato_Net("sedegapp", 
            //    CommonManager.GetCurrentUserPMatricola(), "HRUP", "02GEST");
            //for (int x = 0; x < response.DT_CategorieDatoAbilitate.Rows.Count; x++)
            //{
            //    list.Add(new SelectListItem()
            //    {
            //        Value = response.DT_CategorieDatoAbilitate.Rows[x].ItemArray[0].ToString(),
            //        Text = response.DT_CategorieDatoAbilitate.Rows[x].ItemArray[0].ToString() + "-" + response.DT_CategorieDatoAbilitate.Rows[x].ItemArray[1].ToString(),
            //        Selected = false
            //    });
            //}

            Abilitazioni AB = CommonManager.getAbilitazioni();
            var ab = AB.ListaAbilitazioni.Where(x => x.MatrLivello2.Select(a => a.Matricola)
                .Contains(CommonManager.GetCurrentUserPMatricola()));
            foreach (var a in ab)
            {
                list.Add(new SelectListItem()
                {
                    Value = a.Sede,
                    Text = a.Sede + "-" + a.DescrSede,
                    Selected = false
                });
            }

            RicercaPdfModel model = new RicercaPdfModel();
            model.listasedi = new SelectList(list, "Value", "Text");

            List<SelectListItem> listStati = new List<SelectListItem>();

            listStati.Add(new SelectListItem() { Value = "S", Text = "Stampati" });
            listStati.Add(new SelectListItem() { Value = "C", Text = "Firmati" });
            model.listastati = new SelectList(listStati, "Value", "Text");

            return model;
        }
        public static List<Sede> GetDaFirmareModel(string data1 = null, string data2 = null, string sede = null, string stato = null)
        {
            List<Sede> LS = Utility.GetSediGappResponsabileLiv2List();
            string[] SediPerFirma = LS.Select(x => x.CodiceSede).ToArray();// Utente.SediGappAccessoFirma();
            var db = new myRaiData.digiGappEntities();
            string matricola = CommonManager.GetCurrentUserMatricola();
            DateTime DateStartRicerca = default(DateTime);
            DateTime DateEndRicerca = default(DateTime);

            if (!String.IsNullOrWhiteSpace(data1))
                DateTime.TryParseExact(data1, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateStartRicerca);
            if (!String.IsNullOrWhiteSpace(data2))
                DateTime.TryParseExact(data2, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateEndRicerca);
            if (sede != null && sede.Trim() == "") sede = null;
            if (stato != null && stato.Trim() == "") stato = null;


            List<Sede> sedi = db.DIGIRESP_Archivio_PDF
                                .Where(x => SediPerFirma.Contains(x.sede_gapp))
                                .Where(x => DateStartRicerca == default(DateTime) || (DateStartRicerca != default(DateTime) && x.data_inizio >= DateStartRicerca))
                                .Where(x => DateEndRicerca == default(DateTime) || (DateEndRicerca != default(DateTime) && x.data_inizio <= DateEndRicerca))
                                .Where(x => sede == null || (sede != null && x.sede_gapp == sede))
                                .Where(x => (stato == null && x.stato_pdf == "S_OK") || (stato != null && x.stato_pdf == stato + "_OK"))
                                .OrderBy(x => x.data_inizio)
                                .Select(x => new
                                {
                                    sede = x.sede_gapp,
                                    datainizio = x.data_inizio,
                                    datafine = x.data_fine,
                                    idpdf = x.ID,
                                    versione = x.numero_versione,
                                    tipo = x.tipologia_pdf,
                                    dstampa = x.data_stampa,
                                    stato = x.stato_pdf,
                                    descsede = db.L2D_SEDE_GAPP
                                     .Where(z => z.cod_sede_gapp == x.sede_gapp)
                                     .Select(z => z.desc_sede_gapp).FirstOrDefault()


                                }).GroupBy(
                                    p => new
                                    {
                                        cod = p.sede,
                                        desc = p.descsede
                                    },
                                    p => new myRaiHelper.PeriodoPDF
                                    {
                                        idPdf = p.idpdf,
                                        DateStart = p.datainizio,
                                        DateEnd = p.datafine,
                                        codSede = p.sede,
                                        descSede = p.descsede,
                                        Versione = p.versione,
                                        Data_generazione = p.dstampa,
                                        tipoPDF = p.tipo,
                                        statoPDF = p.stato,
                                        inFirma = db.MyRai_Carrello.Where(x => x.id_archivio_pdf == p.idpdf && x.matricola == matricola).Any(),
                                        Data_letto = db.MyRai_ArchivioPDF_viewers.Where(x => x.id_archivio_pdf == p.idpdf && x.matricola == matricola)
                                                    .Select(pdf => pdf.data).FirstOrDefault()


                                    },
                                    (key, g) => new Sede
                                    {
                                        CodiceSede = key.cod,
                                        DescrizioneSede = key.desc,
                                        PeriodiPDF = g
                                                    .GroupBy(p => new { p.DateStart, p.DateEnd, p.tipoPDF })
                                                    .Select(c => c.OrderByDescending(pdf => pdf.Versione).FirstOrDefault())

                                        .OrderBy(da => da.DateStart)
                                        .ThenByDescending(ver => ver.Versione)
                                        .AsEnumerable()

                                    }).ToList();

            List<MyRai_PianoFerieSedi> allPF = new List<MyRai_PianoFerieSedi>();
            if (String.IsNullOrWhiteSpace(stato) || stato == "S")
                allPF = db.MyRai_PianoFerieSedi.Where(x => x.data_approvata != null && x.anno == DateTime.Now.Year && x.data_firma == null).ToList();
            else
                allPF = db.MyRai_PianoFerieSedi.Where(x => x.data_approvata != null && x.anno == DateTime.Now.Year && x.data_firma != null).ToList();


            if (DateStartRicerca != default(DateTime))
                allPF = allPF.Where(x => x.data_approvata >= DateStartRicerca).ToList();
            if (DateEndRicerca != default(DateTime))
                allPF = allPF.Where(x => x.data_approvata <= DateEndRicerca).ToList();


            if (!String.IsNullOrWhiteSpace(sede)) SediPerFirma = new List<string> { sede }.ToArray();
            foreach (string s in SediPerFirma)
            {

                var sediPFapprovati = allPF.Where(x => x.sedegapp.StartsWith(s)).Select(x => x.sedegapp).Distinct().ToList();
                if (sediPFapprovati.Count > 0)
                {
                    Sede sedeInModel = sedi.Where(x => x.CodiceSede == s).FirstOrDefault();
                    if (sedeInModel == null)
                    {
                        sedeInModel = new Sede() { CodiceSede = s, DescrizioneSede = LS.Where(x => x.CodiceSede == s).Select(x => x.DescrizioneSede).FirstOrDefault() };
                        sedi.Add(sedeInModel);
                    }
                    sedeInModel.ListaPianiFerie = new List<MyRai_PianoFerieSedi>();

                    foreach (string sedePFapprovato in sediPFapprovati)
                    {
                        var pdf = allPF.Where(x => x.sedegapp == sedePFapprovato).OrderByDescending(a => a.numero_versione).FirstOrDefault();
                        sedeInModel.ListaPianiFerie.Add(pdf);
                    }
                }
            }
            return sedi;

        }
        public static MeseAnnoModel GetMeseAnno(string codiceSede, int mese, int anno)
        {

            MeseAnnoModel M = new MeseAnnoModel() { Mese = mese, Anno = anno };

            WSDigigapp datiBack = new WSDigigapp();
            DateTime Dmese = new DateTime(anno, mese, 1);
            DateTime DmesePrec = new DateTime(Dmese.AddMonths(-1).Year, Dmese.AddMonths(-1).Month, 1);
            DateTime DmeseNext = new DateTime(Dmese.AddMonths(1).Year, Dmese.AddMonths(1).Month, 1);

            var response = datiBack.getScadenzario(CommonManager.GetCurrentUserMatricola(),
                                                    DmesePrec.ToString("ddMMyyyy"),
                                                    DateTime.Now.ToString("ddMMyyyy"),
                                                    "02",
                                                    new string[] { codiceSede },
                                                    75);
            if (response == null || response.Length == 0) return M;

            M.Etichetta = Dmese.ToString("MMMM yyyy").ToUpper();
            if (DateTime.Now.Year < anno || (DateTime.Now.Year == anno && DateTime.Now.Month > mese))
            {
                M.AllowNext = DmeseNext.Month.ToString() + "," + DmeseNext.Year.ToString();
            }
            if (response[0].statoMese.Trim() != "C")
            {
                M.AllowPrevious = DmesePrec.Month.ToString() + "," + DmesePrec.Year.ToString();
            }

            return M;
        }
        public static GiornateModel GetGiornateModel(string codiceSede, int mese, int anno)
        {
            GiornateModel model = new GiornateModel();

            WSDigigapp datiBack = new WSDigigapp();
            DateTime Dmese = new DateTime(anno, mese, 1);

            var response = datiBack.getScadenzario(CommonManager.GetCurrentUserMatricola(),
                                                    Dmese.ToString("ddMMyyyy"),
                                                    DateTime.Now.ToString("ddMMyyyy"),
                                                    "02",
                                                    new string[] { codiceSede },
                                                    75);
            if (response == null || response.Length == 0) return model;
            DateTime d = DateTime.Now.Date;

            model.Giornate = response[0].giornate
                            .Where(x => x.data < d && (x.statoTotale == "S" || x.statoTotale == "X"))
                            .Select(x => new GiornataContentModel() { data = x.data }).ToArray();
            return model;
        }

        public static int GetTotalePianiFerieDisponibiliFirma(List<Sede> sedi)
        {
            var db = new myRaiData.digiGappEntities();
            string[] cod = sedi.Select(x => x.CodiceSede).ToArray();
            return db.MyRai_PianoFerieSedi.Where(x => x.data_approvata != null && x.data_firma == null && cod.Any(z => x.sedegapp.StartsWith(z))).Count();
        }

        public static EccezioniGiornataModel getEccezioniGiornataModel(string giorno, string codsede, string eticsede)
        {
            EccezioniGiornataModel model = new EccezioniGiornataModel();
            DateTime D;
            DateTime.TryParseExact(giorno, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D);
            model.Data = D;
            model.etichettaSede = eticsede.Trim();
            return model;
        }

        public static Tuple<int, string> CancellaDaCarrello(int idPdf, string matricola)
        {
            var db = new myRaiData.digiGappEntities();
            var item = db.MyRai_Carrello.Where(x => x.matricola == matricola && x.id_archivio_pdf == idPdf).FirstOrDefault();
            if (item != null)
            {
                db.MyRai_Carrello.Remove(item);
                if (DBHelper.Save(db, matricola))
                {
                    int n = db.MyRai_Carrello.Where(x => x.matricola == matricola).Count();
                    return new Tuple<int, string>(n, "OK");
                }
                else
                {
                    int n = db.MyRai_Carrello.Where(x => x.matricola == matricola).Count();
                    return new Tuple<int, string>(n, "Errore scrittura DB");
                }

            }
            else return new Tuple<int, string>(0, "Errore dati DB");
        }

        public static Tuple<int, string> AggiungiCarrello(int idPdf, string matricola)
        {
            var db = new myRaiData.digiGappEntities();

            int q = db.MyRai_Carrello.Where(x => x.matricola == matricola && x.id_archivio_pdf == idPdf).Count();
            if (q > 0)
            {
                return new Tuple<int, string>(0, "Documento già presente nel carrello");
            }

            db.MyRai_Carrello.Add(new myRaiData.MyRai_Carrello()
            {
                data_pdf_aggiunto = DateTime.Now,
                id_archivio_pdf = idPdf,
                matricola = matricola
            });

            if (DBHelper.Save(db, matricola))
            {
                int n = db.MyRai_Carrello.Where(x => x.matricola == matricola).Count();
                return new Tuple<int, string>(n, "OK");
            }
            else
            {
                int n = db.MyRai_Carrello.Where(x => x.matricola == matricola).Count();
                return new Tuple<int, string>(n, "Errore salvataggio DB");
            }
        }

        public static Boolean RimuoviDaCarrello(int idPdf)
        {
            var db = new digiGappEntities();
            var item = db.MyRai_Carrello.Where(x => x.id_archivio_pdf == idPdf).FirstOrDefault();
            if (item != null)
            {
                db.MyRai_Carrello.Remove(item);
                db.SaveChanges();
                return true;
            }
            else return false;
        }
        public static Boolean RimuoviDaCarrelloGenerico(int idCarrelloGen)
        {
            var db = new digiGappEntities();
            var item = db.MyRai_CarrelloGenerico.Where(x => x.id == idCarrelloGen).FirstOrDefault();
            if (item != null)
            {
                db.MyRai_CarrelloGenerico.Remove(item);
                db.SaveChanges();
                return true;
            }
            else return false;
        }
        public static Boolean AggiornaPianoFerieFirmato(int idPianoFerieSede, byte[] pdfFirmato)
        {
            var db = new digiGappEntities();
            var row = db.MyRai_PianoFerieSedi.Where(x => x.id == idPianoFerieSede).FirstOrDefault();
            if (row != null)
            {
                row.data_firma = DateTime.Now;
                row.matricola_firma = CommonManager.GetCurrentUserMatricola();
                row.pdf = pdfFirmato;
                try
                {
                    row.useragent_firma = HttpContext.Current.Request.UserAgent;
                    row.ip_firma = HttpContext.Current.Request.UserHostAddress;
                }
                catch (Exception ex)
                {
                }
                db.SaveChanges();
                return true;
            }
            else return false;
        }
        public static Boolean AggiornaPDFconvalidato(int idPdf, byte[] buffer)
        {

            var db = new digiGappEntities();
            var item = db.DIGIRESP_Archivio_PDF.Where(x => x.ID == idPdf).FirstOrDefault();
            if (item != null)
            {
                item.pdf = buffer;
                item.stato_pdf = EnumStatiPDF.Convalidato;
                item.data_convalida = DateTime.Now;
                item.ip_postazione_firma = HttpContext.Current.Request.UserHostAddress;
                item.ip_useragent_firma = HttpContext.Current.Request.UserAgent;
                item.matricola_convalida = CommonManager.GetCurrentUserMatricola();
                db.SaveChanges();

                var ListaVersioniPrec = db.DIGIRESP_Archivio_PDF.Where(x => x.tipologia_pdf == item.tipologia_pdf
                    && x.numero_versione < item.numero_versione
                    && x.sede_gapp == item.sede_gapp
                    && x.data_inizio == item.data_inizio
                    && x.data_fine == item.data_fine).ToList();

                if (ListaVersioniPrec.Count > 0)
                {
                    //mette le versioni prec a convalidato altrimenti sarebbero visualizzate nella pagina 
                    foreach (var vp in ListaVersioniPrec)
                        vp.stato_pdf = EnumStatiPDF.Convalidato;

                    db.SaveChanges();
                }
                return true;
            }
            else return false;
        }

        public static void EliminaNotaPdfInFirma(int idPdf)
        {
            var db = new digiGappEntities();
            try
            {
                var notifica = db.MyRai_Notifiche.Where(x => x.id_riferimento == idPdf && x.categoria == "PDF firma").FirstOrDefault();
                if (notifica != null)
                {
                    notifica.data_letta = DateTime.Now;
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {

            }
        }
        public static string InserimentoPianoFerieBatch(string sede, int anno, MyRai_PianoFerieSedi PFsede)
        {
            var db = new digiGappEntities();

            //var pf = db.MyRai_PianoFerieSedi.Where(x => x.sedegapp == sede && x.anno == anno && x.data_approvata != null && x.data_firma != null).OrderByDescending(x => x.numero_versione).ToList();
            //if (!pf.Any()) return "La sede non è approvata e firmata";

            List<string> Matricole = db.MyRai_PianoFerie.Where(x => x.Id_pdf_pianoferie_inclusa == PFsede.id &&
                                                                    x.sedegapp == sede &&
                                                                    x.anno == anno &&
                                                                    x.data_approvato != null
            ).Select(x => x.matricola).OrderBy(x => x).ToList();


            // DateTime D1 = new DateTime(2021, 1, 1);
            // DateTime D2 = new DateTime(2021, 3, 31);
            foreach (var m in Matricole)
            {
                var dayList = db.MyRai_PianoFerieGiorni.Where(x => x.matricola == m && x.data.Year == anno).OrderBy(x => x.data).ToList();
                //if (myRaiCommonTasks.CommonTasks.Richiede2021(m))
                //{
                //    dayList.AddRange(db.MyRai_PianoFerieGiorni.Where(x => x.matricola == m
                //                            && x.data >= D1 && x.data <= D2).OrderBy(x=>x.data));
                //}
                foreach (var day in dayList)
                {
                    if (db.MyRai_PianoFerieBatch.Any(x => x.matricola == m && x.codice_eccezione == day.eccezione && x.data_eccezione == day.data))
                        continue;

                    MyRai_PianoFerieBatch pb = new MyRai_PianoFerieBatch()
                    {
                        alle = "",
                        codice_eccezione = day.eccezione,
                        dalle = "",
                        data_creazione_record = DateTime.Now,
                        data_eccezione = day.data,
                        importo = "",
                        matricola = m,
                        quantita = "1",
                        sedegapp = sede,
                        provenienza = "PianoFerie " + anno,
                        data_swap_turno = day.data_swap_turno
                    };
                    db.MyRai_PianoFerieBatch.Add(pb);
                }
            }
            try
            {
                db.SaveChanges();
                return null;
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "InserimentoPianoFerieBatch",
                    error_message = sede + "-" + anno.ToString() + "-" + ex.ToString()
                });
                return ex.Message;
            }
        }
        public static List<Alias> GetAlias()
        {
            List<Alias> Lalias = new List<Alias>();

            var db = new myRaiData.digiGappEntities();
            var par = db.MyRai_ParametriSistema.Where(x => x.Chiave == "FirmaMatricoleParticolari").FirstOrDefault();
            if (par != null && !String.IsNullOrWhiteSpace(par.Valore1))
            {
                string[] items = par.Valore1.Split(',');
                var myItems = items.Where(x => x.Contains("$") && x.StartsWith(CommonManager.GetCurrentUserPMatricola() + "|")).ToList();
                if (myItems.Any())
                {
                    foreach (var item in myItems)
                    {

                        string[] parti = item.Split(new char[] { '|', '$' });
                        Lalias.Add(new Alias()
                        {
                            PmatricolaDaImpersonare = parti[1],
                            NominativoDaImpersonare = parti[2]
                        });

                    }
                }
            }
            return Lalias;
        }
        public static FirmaDocumentiResponse FirmaDocumenti(string pin, string pwd, string pmatricolaImpersonata, string nominativoImpersonato)
        {

            string matricola = CommonManager.GetCurrentUserMatricola();
            var db = new digiGappEntities();
            string pmatr = CommonManager.GetCurrentUserPMatricola();
            string nominativo = Utente.Nominativo().Trim();

            pmatr = CheckMatricoleParticolari(pmatr);

            if (!String.IsNullOrWhiteSpace(pmatricolaImpersonata) && !String.IsNullOrWhiteSpace(nominativoImpersonato))
            {
                pmatr = pmatricolaImpersonata.Trim();
                nominativo = nominativoImpersonato.Trim();
            }


            var Docs = db.MyRai_Carrello.Where(x => x.matricola == matricola).ToList();
            var PDFdaCarrelloGenerico = db.MyRai_CarrelloGenerico.Where(x => x.matricola == matricola && !x.tabella.Equals("XR_DEM_DOCUMENTI")).ToList();

            if (Docs.Count == 0 && PDFdaCarrelloGenerico.Count == 0) return new FirmaDocumentiResponse()
            {
                esito = "Nessun documento da firmare",
                FirmatiOk = 0,
                InCarrello = db.MyRai_Carrello.Where(x => x.matricola == matricola).Count()
            };

            //if (pmatr.Contains("$"))
            //{
            //    var p = pmatr.Split('$').ToList();
            //    if (String.IsNullOrEmpty(p[1]))
            //    {
            //        // prende il corrente
            //        nominativo = Utente.Nominativo().Trim();
            //    }
            //    else
            //    {
            //        nominativo = p[1];
            //    }
            //    pmatr = p[0];
            //}
            //else
            //{
            //    nominativo = Utente.Nominativo().Trim();
            //}

            myRaiServiceHub.firmaDigitale.remoteSignature r = new myRaiServiceHub.firmaDigitale.remoteSignature();
            myRaiServiceHub.firmaDigitale.RemoteSignatureCredentials cred = new myRaiServiceHub.firmaDigitale.RemoteSignatureCredentials();
            cred.userid = pmatr;

            cred.password = pwd;
            cred.extAuth = pin;

            myRaiServiceHub.firmaDigitale.SignatureFlags d = new myRaiServiceHub.firmaDigitale.SignatureFlags();

            String sessionToken;
            try
            {
                sessionToken = r.openSignatureSession(cred);
            }
            catch (Exception ex)
            {
                return new FirmaDocumentiResponse()
                {
                    esito = "Errore iniziazione firma digitale:" + ex,
                    FirmatiOk = 0,
                    InCarrello = db.MyRai_Carrello.Where(x => x.matricola == matricola).Count()
                };
            }


            int FirmatiConSuccesso = 0;

            FirmaDocumentiResponse Response = new FirmaDocumentiResponse();
            Response.DocsInErrore = new List<DocInErrore>();



            foreach (MyRai_Carrello car in Docs.OrderBy(x => x.DIGIRESP_Archivio_PDF.data_inizio))
            {
                DIGIRESP_Archivio_PDF pdf = db.DIGIRESP_Archivio_PDF.Where(x => x.ID == car.id_archivio_pdf).FirstOrDefault();
                if (pdf == null)
                {
                    Response.DocsInErrore.Add(new DocInErrore()
                    {
                        data_fine = pdf.data_fine.ToString("ddMMyyyy"),
                        data_inizio = pdf.data_inizio.ToString("ddMMyyyy"),
                        sedegapp = pdf.sede_gapp,
                        esito = "Documento PDF non trovato"
                    });
                    continue;
                }


                // 1. FIRMA DIGITALE SU PDF
                byte[] response;

                try
                {
                    string formatoData = "ddMMyyyy";
                    string labelFirma = "Firmato digitalmente da " + nominativo + " in data " +
                                         DateTime.Now.ToString("dd/MM/yyyy HH:mm");

                    string[] EtichettaFirma = CommonHelper.GetParametri<string>(EnumParametriSistema.EtichettaFirmaSuPDF);
                    if (EtichettaFirma != null
                        && EtichettaFirma.Length == 2
                        && !String.IsNullOrWhiteSpace(EtichettaFirma[0])
                        && !String.IsNullOrWhiteSpace(EtichettaFirma[1]))
                    {
                        labelFirma = EtichettaFirma[0].Replace("#UTENTE", nominativo);
                        formatoData = EtichettaFirma[1];
                    }

                    response = r.signPDF(cred, pdf.pdf,
                                       false, "SHA256", null, d, "FirmaDigitale", -1, 16, 760, 400, 20,
                                       pmatr, "FirmaDigitale", pdf.sede_gapp,
                                       formatoData,//"ddMMyyyy",
                                       labelFirma,//"Firmato digitalmente da " + Utente.Nominativo() + " in data {date}",
                                       10);
                    if (response == null || response.Length == 0)
                    {
                        Response.DocsInErrore.Add(new DocInErrore()
                        {
                            data_fine = pdf.data_fine.ToString("ddMMyyyy"),
                            data_inizio = pdf.data_inizio.ToString("ddMMyyyy"),
                            sedegapp = pdf.sede_gapp,
                            esito = "La firma digitale ha restituito un doc vuoto"
                        });
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    var erroreFirmaAuthentication = db.MyRai_ErroriFirma.Where(x => x.codice == ex.Message).FirstOrDefault();
                    if (erroreFirmaAuthentication != null)
                    {
                        Response.DocsInErrore.Add(new DocInErrore()
                        {
                            data_fine = pdf.data_fine.ToString("ddMMyyyy"),
                            data_inizio = pdf.data_inizio.ToString("ddMMyyyy"),
                            sedegapp = pdf.sede_gapp,
                            esito = "Errore bloccante durante autenticazione: " + erroreFirmaAuthentication.codice + "-" + erroreFirmaAuthentication.errore
                        });
                        Response.IsAuthError = true;
                        break;
                    }
                    else Response.DocsInErrore.Add(new DocInErrore()
                    {
                        data_fine = pdf.data_fine.ToString("ddMMyyyy"),
                        data_inizio = pdf.data_inizio.ToString("ddMMyyyy"),
                        sedegapp = pdf.sede_gapp,
                        esito = "La firma digitale ha risposto con errori:" + ex.Message
                    });

                    //PIN ERRATO SE EX.MESSAGE = "010"

                    // if (ex.Message != "010")
                    // {
                    continue;
                    // }
                }



                //if ( pdf.tipologia_pdf == "R")
                //{
                //    //2. CONVALIDA CICS (solo se tipo riepilogo, non presenze)
                //    string res;

                //    try
                //    {
                //        res = ConvalidaCics(
                //                     pdf.data_inizio.ToString("ddMMyyyy"),
                //                     pdf.data_fine.ToString("ddMMyyyy"),
                //                     pdf.sede_gapp);

                //        if (res != null)
                //        {
                //            Response.DocsInErrore.Add(new DocInErrore()
                //            {
                //                data_fine = pdf.data_fine.ToString("ddMMyyyy"),
                //                data_inizio = pdf.data_inizio.ToString("ddMMyyyy"),
                //                sedegapp = pdf.sede_gapp,
                //                esito = "Errore Convalida Cics:" + res
                //            });
                //            continue;
                //        }
                //    }
                //    catch (Exception ex)
                //    {
                //        Response.DocsInErrore.Add(new DocInErrore()
                //        {
                //            data_fine = pdf.data_fine.ToString("ddMMyyyy"),
                //            data_inizio = pdf.data_inizio.ToString("ddMMyyyy"),
                //            sedegapp = pdf.sede_gapp,
                //            esito = "Errore Convalida Cics:" + ex.Message
                //        });
                //        continue;
                //    }
                //}

                EliminaNotaPdfInFirma(pdf.ID);
                FirmatiConSuccesso++;

                //3. CANCELLA DA CARRELLO
                try
                {
                    RimuoviDaCarrello(car.id_archivio_pdf);
                }
                catch (Exception ex)
                {
                    Logger.LogErrori(new MyRai_LogErrori()
                    {
                        applicativo = "Portale",
                        data = DateTime.Now,
                        error_message = ex.ToString(),
                        matricola = CommonManager.GetCurrentUserMatricola(),
                        provenienza = "RimuoviDaCarrello"
                    });

                    Response.DocsInErrore.Add(new DocInErrore()
                    {
                        data_fine = pdf.data_fine.ToString("ddMMyyyy"),
                        data_inizio = pdf.data_inizio.ToString("ddMMyyyy"),
                        sedegapp = pdf.sede_gapp,
                        esito = "Errore DB cancellazione da carrello-" + ex.Message
                    });
                }

                //4. AGGIORNA STATO SU DB
                try
                {
                    AggiornaPDFconvalidato(car.id_archivio_pdf, response);
                }
                catch (Exception ex)
                {

                    Response.DocsInErrore.Add(new DocInErrore()
                    {
                        data_fine = pdf.data_fine.ToString("ddMMyyyy"),
                        data_inizio = pdf.data_inizio.ToString("ddMMyyyy"),
                        sedegapp = pdf.sede_gapp,
                        esito = "Errore DB aggiornamento stato-" + ex.Message
                    });
                }

                //5. CONVALIDA ULTIMO GIORNO SE NECESSARIO
                if (pdf.tipologia_pdf == "R")
                {
                    Boolean? conv = myRaiCommonTasks.CommonTasks.VerificaGiornoConvalidato(pdf.data_fine,
                        CommonManager.GetCurrentUserMatricola(),
                        nominativo,
                        pdf.sede_gapp);
                    string convalidato = "NULL";
                    if (conv != null) convalidato = conv.ToString();

                    Logger.LogAzione(new MyRai_LogAzioni()
                    {
                        applicativo = "PORTALE",
                        data = DateTime.Now,
                        descrizione_operazione = "VerificaGiornoConvalidato " + convalidato + " - " + pdf.sede_gapp + " - " + pdf.data_fine,
                        matricola = CommonManager.GetCurrentUserMatricola(),
                        provenienza = "FirmaDocumenti",
                        operazione = "firma"
                    });

                    if (conv == false || conv == null)
                    {
                        string esito = myRaiCommonTasks.CommonTasks.ConvalidaNoStampa(CommonManager.GetCurrentUserMatricola(),
                             nominativo,
                             pdf.data_fine,
                             pdf.data_fine,
                             pdf.sede_gapp);
                        if (esito != null)
                        {
                            Response.DocsInErrore.Add(new DocInErrore()
                            {
                                data_fine = pdf.data_fine.ToString("ddMMyyyy"),
                                data_inizio = pdf.data_inizio.ToString("ddMMyyyy"),
                                sedegapp = pdf.sede_gapp,
                                esito = "Errore Convalida Ultimo Giorno " + esito
                            });
                        }
                    }
                }
            }

            foreach (MyRai_CarrelloGenerico pdfCarrGen in PDFdaCarrelloGenerico)
            {
                if (pdfCarrGen.tabella == "MyRai_PianoFerieSedi")
                {
                    MyRai_PianoFerieSedi doc = db.MyRai_PianoFerieSedi.Where(x => x.id == pdfCarrGen.id_documento).FirstOrDefault();
                    if (doc == null)
                    {
                        Response.DocsInErrore.Add(new DocInErrore()
                        {
                            data_fine = "",
                            data_inizio = "",
                            sedegapp = "",
                            esito = "Documento PDF da CarrelloGenerico non trovato id_doc " + pdfCarrGen.id_documento
                        });
                        continue;
                    }

                    // 1. FIRMA DIGITALE SU PDF
                    byte[] response;
                    try
                    {
                        string formatoData = "ddMMyyyy";
                        string labelFirma = "Firmato digitalmente da " + nominativo + " in data " +
                                             DateTime.Now.ToString("dd/MM/yyyy HH:mm");

                        string[] EtichettaFirma = CommonManager.GetParametri<string>(EnumParametriSistema.EtichettaFirmaSuPDF);
                        if (EtichettaFirma != null
                            && EtichettaFirma.Length == 2
                            && !String.IsNullOrWhiteSpace(EtichettaFirma[0])
                            && !String.IsNullOrWhiteSpace(EtichettaFirma[1]))
                        {
                            labelFirma = EtichettaFirma[0].Replace("#UTENTE", nominativo);
                            formatoData = EtichettaFirma[1];
                        }

                        response = r.signPDF(cred, doc.pdf,
                                           false, "SHA256", null, d, "FirmaDigitale", -1, 16, 760, 400, 20,
                                           pmatr, "FirmaDigitale", doc.sedegapp,
                                           formatoData,//"ddMMyyyy",
                                           labelFirma,//"Firmato digitalmente da " + Utente.Nominativo() + " in data {date}",
                                           10);
                        if (response == null || response.Length == 0)
                        {
                            Response.DocsInErrore.Add(new DocInErrore()
                            {
                                data_fine = "",
                                data_inizio = "",
                                sedegapp = doc.sedegapp,
                                esito = "La firma digitale ha restituito un doc vuoto"
                            });
                            continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        var erroreFirmaAuthentication = db.MyRai_ErroriFirma.Where(x => x.codice == ex.Message).FirstOrDefault();
                        if (erroreFirmaAuthentication != null)
                        {
                            Response.DocsInErrore.Add(new DocInErrore()
                            {
                                data_fine = "",
                                data_inizio = "",
                                sedegapp = doc.sedegapp,
                                esito = "Errore bloccante durante autenticazione: " + erroreFirmaAuthentication.codice + "-" + erroreFirmaAuthentication.errore
                            });
                            Response.IsAuthError = true;
                            break;
                        }
                        else Response.DocsInErrore.Add(new DocInErrore()
                        {
                            data_fine = "",
                            data_inizio = "",
                            sedegapp = doc.sedegapp,
                            esito = "La firma digitale ha risposto con errori:" + ex.Message
                        });

                        continue;
                    }

                    FirmatiConSuccesso++;

                    //3. CANCELLA DA CARRELLO
                    try
                    {
                        RimuoviDaCarrelloGenerico(pdfCarrGen.id);
                    }
                    catch (Exception ex)
                    {
                        Logger.LogErrori(new MyRai_LogErrori()
                        {
                            applicativo = "Portale",
                            data = DateTime.Now,
                            error_message = ex.ToString(),
                            matricola = CommonManager.GetCurrentUserMatricola(),
                            provenienza = "RimuoviDaCarrelloGenerico"
                        });

                        Response.DocsInErrore.Add(new DocInErrore()
                        {
                            data_fine = "",
                            data_inizio = "",
                            sedegapp = doc.sedegapp,
                            esito = "Errore DB cancellazione da carrelloGenerico-" + ex.Message
                        });
                    }

                    //4. AGGIORNA STATO SU DB
                    try
                    {
                        AggiornaPianoFerieFirmato(doc.id, response);
                    }
                    catch (Exception ex)
                    {
                        Response.DocsInErrore.Add(new DocInErrore()
                        {
                            data_fine = "",
                            data_inizio = "",
                            sedegapp = doc.sedegapp,
                            esito = "Errore DB aggiornamento stato-" + ex.Message
                        });
                    }

                    //5.  BATCH
                    string batchResult = InserimentoPianoFerieBatch(doc.sedegapp, doc.anno, doc);
                    if (batchResult != null)
                    {
                        Response.DocsInErrore.Add(new DocInErrore()
                        {
                            data_fine = "",
                            data_inizio = "",
                            sedegapp = doc.sedegapp,
                            esito = "Errore da InserimentoBatch: " + batchResult
                        });
                    }
                    else
                    {
                        try
                        {
                            InformaDipendentiPianoFerieConvalidato(doc);
                        }
                        catch (Exception ex)
                        {
                            Logger.LogErrori(new MyRai_LogErrori()
                            {
                                applicativo = "PORTALE",
                                data = DateTime.Now,
                                matricola = CommonManager.GetCurrentUserMatricola(),
                                provenienza = "FirmaDocumenti",
                                error_message = ex.ToString()
                            });
                        }
                    }
                }
            }

            r.closeSignatureSession(cred, sessionToken);
            if (Response.DocsInErrore == null || Response.DocsInErrore.Count == 0)
                Response.esito = "OK";
            else
                Response.esito = "ERRORI";

            Response.FirmatiOk = FirmatiConSuccesso;
            Response.InCarrello = db.MyRai_Carrello.Where(x => x.matricola == matricola).Count();
            return Response;
        }

        public static void InformaDipendentiPianoFerieConvalidato(MyRai_PianoFerieSedi doc)
        {
            var db = new myRaiData.digiGappEntities();

            List<MyRai_PianoFerie> PianoFerieDip = db.MyRai_PianoFerie.Where(x => x.Id_pdf_pianoferie_inclusa == doc.id).ToList();
            List<string> Matricole = PianoFerieDip.Select(x => x.matricola).ToList();

            myRaiCommonTasks.GestoreMail gm = new myRaiCommonTasks.GestoreMail();
            foreach (string m in Matricole)
            {
                gm.InvioMail(
                    "raiplace.selfservice@rai.it",
                    "Convalida piano ferie",
                    "P" + m + "@rai.it",
                    null,
                    "Convalida piano ferie",
                    null,
                    "Il tuo piano ferie è stato convalidato dal responsabile"
                    );

                NotificheManager.InserisciNotifica("Piano ferie convalidato dal responsabile",
                                                    "PianoFerie", m, "Portale", 0);
            }


            //WSDigigapp wsService = new WSDigigapp()
            //{
            //    Credentials = new System.Net.NetworkCredential(
            //          CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
            //          CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1])
            //};
            //string sede = doc.sedegapp;
            //string repartoReq = null;

            //if (sede.Length > 5)
            //{
            //    repartoReq = sede.Substring(5);
            //}

            //pianoFerieSedeGapp resp = wsService.getPianoFerieSedeGapp("P" + Utente.Matricola(), "01" + "01" + doc.anno, "", "", sede.Substring(0, 5), 75);



            //var AB = CommonManager.getAbilitazioni();
            //List<string> SediAb = AB.ListaAbilitazioni.Where(x => x.Sede.ToUpper().StartsWith(sede.Substring(0, 5).ToUpper())).Select(x => x.Sede).ToList();

            //Dipendente[] MieiDipendenti = null;
            //if (repartoReq == null)
            //    MieiDipendenti = resp.dipendenti.Where(x => !SediAb.Contains(sede + x.reparto) || String.IsNullOrWhiteSpace(x.reparto) || x.reparto.Trim() == "0" || x.reparto.Trim() == "00").ToArray();
            //else
            //    MieiDipendenti = resp.dipendenti.Where(x => x.reparto == repartoReq).ToArray();


            //DateTime D = DateTime.Now;
            //int? IdTipologiaEsenteFerie = db.MyRai_InfoDipendente_Tipologia.Where(x => x.info == "Esente Piano Ferie" && x.data_inizio <= D && (x.data_fine >= D || x.data_fine == null))
            //                       .Select(x => x.id).FirstOrDefault();

            //foreach (var dip in MieiDipendenti)
            //{
            //    if (dip.matricola != null && dip.matricola.Length == 7) dip.matricola = dip.matricola.Substring(1);

            //    if (db.MyRai_InfoDipendente.Any(x =>
            //                                    x.matricola == dip.matricola &&
            //                                    x.id_infodipendente_tipologia == IdTipologiaEsenteFerie &&
            //                                    D >= x.data_inizio &&
            //                                    D <= x.data_fine
            //                                    && x.valore != null
            //                                    && x.valore.ToLower() == "true")
            //          )
            //        continue;


            //    gm.InvioMail("raiplace.selfservice@rai.it",
            //        "Convalida piano ferie",
            //        "P" + dip.matricola + "@rai.it",
            //        null,
            //        "Convalida piano ferie",
            //        null,
            //        "Il tuo piano ferie è stato convalidato dal responsabile"
            //        );

            //    NotificheManager.InserisciNotifica("Piano ferie convalidato dal responsabile",
            //                                        "PianoFerie", dip.matricola, "Portale", 0);
            //}

        }

        public static string CheckMatricoleParticolari(string pmatr)
        {
            string differentUsers = CommonManager.GetParametro<string>(EnumParametriSistema.FirmaMatricoleParticolari);
            if (!String.IsNullOrWhiteSpace(differentUsers))
            {
                Dictionary<string, string> keyValues = differentUsers.Split(',').Select(x => new { Originale = x.Split('|')[0], Modificata = x.Split('|')[1] })
                    .ToDictionary(y => y.Originale, y => y.Modificata);

                string mod = "";
                if (keyValues.TryGetValue(pmatr, out mod))
                    pmatr = mod;
            }

            return pmatr;
        }

        public static string ConvalidaCics(string dataDa, string dataA, string sedegapp)
        {
            WSDigigapp service = new WSDigigapp()
            {
                Credentials = CommonHelper.GetUtenteServizioCredentials()
            };
            var resp = service.getConvalida(CommonManager.GetCurrentUserMatricola(), Utente.Nominativo(),
                dataDa,
                dataA,
                sedegapp,
                false,
                75);
            if (resp.esito == true)
                return null;
            else
                return resp.raw;

        }

        public static bool CheckConsecutivo(int idPdf, string matr = null)
        {
            if (matr == null) matr = CommonManager.GetCurrentUserMatricola();
            var db = new digiGappEntities();
            var pdf = db.DIGIRESP_Archivio_PDF.Where(x => x.ID == idPdf).FirstOrDefault();
            if (pdf == null) return false;

            if (pdf.tipologia_pdf == "P") return true;

            int quanti = db.DIGIRESP_Archivio_PDF.Where(x => x.tipologia_pdf == "R" && x.sede_gapp == pdf.sede_gapp && x.data_inizio < pdf.data_inizio).Count();
            if (quanti == 0) return true;

            var UltimoGiornoPrecedente = pdf.data_inizio.AddDays(-1);
            int precedentiConvalidati = db.DIGIRESP_Archivio_PDF.Where(x =>
                                                x.tipologia_pdf == "R" &&
                                                x.sede_gapp == pdf.sede_gapp &&
                                                x.stato_pdf == "C_OK" &&
                                                x.data_fine == UltimoGiornoPrecedente).Count();
            if (precedentiConvalidati > 0) return true;

            int precedenteInCarrello = db.MyRai_Carrello.Where(x =>
                                                x.matricola == matr &&
                                                 x.DIGIRESP_Archivio_PDF.tipologia_pdf == "R" &&
                                                x.DIGIRESP_Archivio_PDF.sede_gapp == pdf.sede_gapp &&
                                                x.DIGIRESP_Archivio_PDF.data_fine == UltimoGiornoPrecedente).Count();
            return (precedenteInCarrello > 0);


        }

        public static string CheckRemovibile(int idPdf, string matr = null)
        {
            if (matr == null) matr = CommonManager.GetCurrentUserMatricola();
            var db = new digiGappEntities();
            var pdf = db.DIGIRESP_Archivio_PDF.Where(x => x.ID == idPdf).FirstOrDefault();
            if (pdf == null) return " : NON INDIVIDUATO";

            var successivoInCarrello = db.MyRai_Carrello.Where(x =>
                                                x.matricola == matr &&
                                                x.DIGIRESP_Archivio_PDF.sede_gapp == pdf.sede_gapp &&
                                                x.DIGIRESP_Archivio_PDF.data_inizio > pdf.data_fine)
                                                .OrderBy(x => x.DIGIRESP_Archivio_PDF.data_inizio).FirstOrDefault();
            if (successivoInCarrello == null) return "OK";
            else return successivoInCarrello.DIGIRESP_Archivio_PDF.data_inizio.ToString("dd/MM/yyyy") + "-" +
                        successivoInCarrello.DIGIRESP_Archivio_PDF.data_fine.ToString("dd/MM/yyyy");


        }

        public static void CheckCarrello()
        {
            var db = new digiGappEntities();
            string matr = CommonManager.GetCurrentUserMatricola();

            try
            {
                List<int> IdMioCarrello = db.MyRai_Carrello.Where(x => x.matricola == matr && x.DIGIRESP_Archivio_PDF.data_convalida != null).Select(x => x.id).ToList();

                if (IdMioCarrello.Count == 0) return;

                foreach (int id in IdMioCarrello)
                {
                    var itemCarrello = db.MyRai_Carrello.Find(id);
                    if (itemCarrello != null) db.MyRai_Carrello.Remove(itemCarrello);
                }



                db.SaveChanges();
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "Portale",
                    data = DateTime.Now,
                    matricola = matr,
                    error_message = ex.ToString(),
                    provenienza = "CheckCarrello"
                });
            }
        }


        public class AccountCredentialsExtended
        {
            public string userid { get; set; }
            public string password { get; set; }
            public string signaturePassword { get; set; }
            public string activationPassword { get; set; }
            public int oneshot { get; set; }
            public string customerCode { get; set; }
        }

        public class SignatureDataPdf
        {
            /// <summary>
            /// binary Base64 content
            /// </summary>
            public string content { get; set; }
            /// <summary>
            /// digest algorithm: SHA256
            /// </summary>
            public string digestType { get; set; }
            /// <summary>
            /// binary Base64 X509 certificate,
            /// if null the default certificarte will
            /// be used
            /// </summary>
            public string X509certificate { get; set; }
            public Boolean timestamp { get; set; }
            public Boolean ltv { get; set; }
            public string fieldName { get; set; }
            public int page { get; set; }
            public int x { get; set; }
            public int y { get; set; }
            public int width { get; set; }
            public int height { get; set; }
            public string userName { get; set; }
            public string reason { get; set; }
            public string location { get; set; }
            public string dateFormat { get; set; }
            public string text { get; set; }
            public int fontSize { get; set; }
            public string counterSignaturePath { get; set; }
            public Boolean graphicalSignature { get; set; }
            public Boolean ocsp { get; set; }
            public Boolean cosignCoordinates { get; set; }
            public string timestampCode { get; set; }
            /// <summary>
            /// Signature layout, 0 = horizontal, 1 = vertical
            /// </summary>
            public int pdfSignatureLayout { get; set; }
            public string password { get; set; }
            /// <summary>
            /// Certification level (1=Certified
            /// form filling, 2=Certified form
            /// filling and annotation, 3=No
            /// changes allowed)
            /// </summary>
            public int pdfSignatureCertificationLevel { get; set; }
            public Boolean cadesDetached { get; set; }
        }

        public class SignBaseResponse
        {
            public Boolean success { get; set; }
            public string errorCode { get; set; }
            public string errorMessage { get; set; }
            public int elapsed { get; set; }
            public string errorSubCode { get; set; }
            public string errorSubMessage { get; set; }
        }

        public class SignPDFResponse : SignBaseResponse
        {
            public string signature { get; set; }
        }

        public static FirmaDocumentiResponse TestFirma(string otp, string pwd, string pmatricolaImpersonata, string nominativoImpersonato)
        {
            string matricola = CommonManager.GetCurrentUserMatricola();
            var db = new digiGappEntities();
            IncentiviEntities dbIncentivi = new IncentiviEntities();
            string pmatr = CommonManager.GetCurrentUserPMatricola();
            string nominativo = Utente.Nominativo().Trim();
            FirmaDocumentiResponse result = new FirmaDocumentiResponse();

            try
            {
                if (!String.IsNullOrWhiteSpace(pmatricolaImpersonata) && !String.IsNullOrWhiteSpace(nominativoImpersonato))
                {
                    pmatr = pmatricolaImpersonata.Trim();
                    nominativo = nominativoImpersonato.Trim();
                }

                var PDFdaCarrelloGenerico = db.MyRai_CarrelloGenerico.Where(x => x.matricola == matricola && x.tabella.Equals("XR_DEM_DOCUMENTI")).ToList();
                string url = "";

                if (!CommonManager.IsProduzione())
                {
                    url = "http://10.16.161.202:8080/FirmaRemota/rest/signature/signPDF";
                }
                else
                {
                    url = "http://firmaremota.servizi.rai.it:8080/FirmaRemota/rest/signature/signPDF";
                }

                AccountCredentialsExtended cred = new AccountCredentialsExtended();
                SignatureDataPdf par = new SignatureDataPdf();

                var pdfCarrGen = PDFdaCarrelloGenerico.First();

                RichiestaDoc doc = GetDocumentData(pdfCarrGen.id_documento);
                XR_ALLEGATI principale = new XR_ALLEGATI();

                principale = doc.Allegati.FirstOrDefault(w => w.IsPrincipal);
                string nomefile = principale.NomeFile;

                byte[] content = null;
                string contentBase64 = null;

                if (principale.ContentBytePDF != null)
                {
                    content = principale.ContentBytePDF;
                }
                else
                {
                    content = principale.ContentByte;
                }

                contentBase64 = Convert.ToBase64String(content);

                List<PosizioneProtocolloOBJ> obj = JsonConvert.DeserializeObject<List<PosizioneProtocolloOBJ>>(principale.PosizioneProtocollo);
                var firmaPos = obj.Where(w => w.Oggetto.Equals("Firma")).FirstOrDefault();
                int firmaPosLeft = 16;
                int firmaPosTop = 760;
                int pagina = -1;
                if (firmaPos != null)
                {
                    firmaPosLeft = (int)firmaPos.PosizioneLeft;
                    firmaPosTop = (int)firmaPos.PosizioneTop;
                    pagina = (int)firmaPos.NumeroPagina;
                }

                cred.userid = pmatr;
                cred.password = pwd;
                cred.oneshot = 0;
                cred.signaturePassword = otp;
                par.fieldName = "FirmaDigitale";
                par.page = pagina;
                par.x = firmaPosLeft;
                par.y = (firmaPosTop + 20);
                par.width = 400;
                par.height = 100;
                par.userName = "P103650";
                par.reason = "Per approvazione";
                par.location = "Roma";
                par.dateFormat = "dd/MM/yyyy";
                par.text = "      Firmato digitalmente";
                par.fontSize = 8;
                par.graphicalSignature = true;
                par.ocsp = false;
                par.cosignCoordinates = false;
                par.pdfSignatureLayout = 0;
                par.password = "Password01";
                par.pdfSignatureCertificationLevel = 0;
                par.cadesDetached = false;
                par.content = contentBase64;

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/json";
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                using (var sw = new StreamWriter(request.GetRequestStream()))
                {
                    string json = serializer.Serialize(cred);
                    sw.Write(json);
                    json = serializer.Serialize(par);
                    sw.Write(json);
                    sw.Flush();
                }

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new ApplicationException("Impossibile raggiungere il servizio " + response.StatusCode);
                    }

                    using (var reader = new StreamReader(response.GetResponseStream()))
                    {
                        JavaScriptSerializer js = new JavaScriptSerializer();
                        var objText = reader.ReadToEnd();
                        ScriviLogErrore(objText);
                        SignPDFResponse myobj = (SignPDFResponse)js.Deserialize(objText, typeof(SignPDFResponse));
                        result.esito = (myobj.success ? "OK" : "KO");

                        if (!myobj.success)
                        {
                            throw new Exception(myobj.errorCode + " - " + myobj.errorMessage);
                        }
                    }

                }

            }
            catch (Exception ex)
            {
                result.esito = "KO";
                result.InfoAggiuntive = ex.Message;
                ScriviLogErrore(ex.Message);
            }

            return result;
        }


        #region FIRMA DEMATERIALIZZAZIONE

        /// <summary>
        /// Metodo utilizzato dalla Dematerializzazione
        /// </summary>
        /// <param name="otp"></param>
        /// <param name="pwd"></param>
        /// <param name="pmatricolaImpersonata"></param>
        /// <param name="nominativoImpersonato"></param>
        /// <returns></returns>
        public static FirmaDocumentiResponse DEM_FirmaDocumenti(string otp, string pwd, string pmatricolaImpersonata, string nominativoImpersonato)
        {
            string matricola = CommonManager.GetCurrentUserMatricola();
            var db = new digiGappEntities();
            IncentiviEntities dbIncentivi = new IncentiviEntities();
            string pmatr = CommonManager.GetCurrentUserPMatricola();
            string nominativo = Utente.Nominativo().Trim();

            //pmatr = CheckMatricoleParticolari(pmatr);

            if (!String.IsNullOrWhiteSpace(pmatricolaImpersonata) && !String.IsNullOrWhiteSpace(nominativoImpersonato))
            {
                pmatr = pmatricolaImpersonata.Trim();
                nominativo = nominativoImpersonato.Trim();
            }

            var PDFdaCarrelloGenerico = db.MyRai_CarrelloGenerico.Where(x => x.matricola == matricola && x.tabella.Equals("XR_DEM_DOCUMENTI")).ToList();

            myRaiServiceHub.firmaDigitale.remoteSignature r = new myRaiServiceHub.firmaDigitale.remoteSignature();
            myRaiServiceHub.firmaDigitale.RemoteSignatureCredentials cred = new myRaiServiceHub.firmaDigitale.RemoteSignatureCredentials();

            //pmatr = "P103650";
            cred.userid = pmatr;
            cred.password = pwd;
            cred.extAuth = otp;

            // test sviluppo
            //cred.userid = "P103650";
            //cred.password = "Password01";
            //cred.extAuth = "033193";

            myRaiServiceHub.firmaDigitale.SignatureFlags d = new myRaiServiceHub.firmaDigitale.SignatureFlags();

            String sessionToken;
            try
            {
                sessionToken = r.openSignatureSession(cred);

                if (String.IsNullOrEmpty(sessionToken))
                {
                    throw new Exception("Impossibile inizializzare la sessione");
                }

                ScriviLogAzioni("Creazione sessione di firma per la matricola: " + pmatr, "openSignatureSession");
            }
            catch (Exception ex)
            {

                ScriviLogErrore("Errore iniziazione firma digitale: " + ex);
                return new FirmaDocumentiResponse()
                {
                    esito = "Errore iniziazione firma digitale: " + ex,
                    FirmatiOk = 0,
                    InCarrello = db.MyRai_Carrello.Where(x => x.matricola == matricola).Count(),
                    InfoAggiuntive = "Errore iniziazione firma digitale: " + ex
                };
            }

            int FirmatiConSuccesso = 0;

            FirmaDocumentiResponse Response = new FirmaDocumentiResponse();
            Response.DocsInErrore = new List<DocInErrore>();
            Response.InfoAggiuntive = "";

            foreach (MyRai_CarrelloGenerico pdfCarrGen in PDFdaCarrelloGenerico)
            {
                RichiestaDoc doc = GetDocumentData(pdfCarrGen.id_documento);
                if (doc == null)
                {
                    Response.InfoAggiuntive += "Documento con id " + pdfCarrGen.id_documento + " non firmato. Errore: documento non trovato \r\n";
                    Response.DocsInErrore.Add(new DocInErrore()
                    {
                        data_fine = "",
                        data_inizio = "",
                        sedegapp = "",
                        esito = "Documento PDF da CarrelloGenerico non trovato id_doc " + pdfCarrGen.id_documento
                    });
                    ScriviLogErrore("Documento PDF da CarrelloGenerico non trovato id_doc " + pdfCarrGen.id_documento);
                    continue;
                }

                XR_ALLEGATI principale = new XR_ALLEGATI();
                principale = doc.Allegati.FirstOrDefault(w => w.IsPrincipal);
                byte[] byteArray = null;
                string nomefile = principale.NomeFile;

                ApplicaProtocolloResult applicaPResult = null;

                if (!String.IsNullOrEmpty(doc.Documento.NumeroProtocollo))
                {
                    byteArray = principale.ContentByte;
                    applicaPResult = new ApplicaProtocolloResult()
                    {
                        File = byteArray,
                        Protocollo = doc.Documento.NumeroProtocollo,
                        Id_Documento = doc.Documento.Prot_Id_Documento
                    };
                }

                // se il bytearray è null allora il documento va protocollato
                // altrimenti significa che il documento aveva già il protocollo.
                if (byteArray == null)
                {
                    // 0. APPLICA PROTOCOLLO
                    string matricolaSenzaP = pmatr.Replace("P", "");
                    applicaPResult = ApplicaProtocollo(matricolaSenzaP, nominativo, doc.Documento.Id);

                    if (applicaPResult != null && applicaPResult.File != null)
                    {
                        ScriviLogAzioni("Generato protocollo per la matricola: " + pmatr + " nominativo: " + nominativo + " documento id:" + doc.Documento.Id + " Prot. num. " + applicaPResult.Protocollo, "ApplicaProtocollo");
                        byteArray = applicaPResult.File;

                        // reperisce la versione associata all'allegato
                        int idAllegato = principale.Id;
                        var temp1 = dbIncentivi.XR_DEM_ALLEGATI_VERSIONI.Where(w => w.IdAllegato.Equals(idAllegato)).OrderByDescending(w => w.IdVersione).FirstOrDefault();

                        int idLastVersione = temp1.IdVersione;
                        var lastVersione = dbIncentivi.XR_DEM_VERSIONI_DOCUMENTO.Where(w => w.Id.Equals(idLastVersione)).FirstOrDefault();

                        // reperimento metadati della richiesta
                        XR_DEM_DOCUMENTI docToUpdate = dbIncentivi.XR_DEM_DOCUMENTI.Where(w => w.Id.Equals(pdfCarrGen.id_documento)).FirstOrDefault();
                        if (docToUpdate == null)
                        {
                            throw new Exception("Richiesta non trovata");
                        }

                        docToUpdate.CodiceProtocollatore = "42936";
                        docToUpdate.NumeroProtocollo = applicaPResult.Protocollo;
                        docToUpdate.Prot_Id_Documento = applicaPResult.Id_Documento;
                        docToUpdate.Id_Stato = (int)StatiDematerializzazioneDocumenti.Protocollato;

                        // crea un nuovo allegato
                        XR_ALLEGATI toAddAllegato = new XR_ALLEGATI()
                        {
                            PosizioneProtocollo = principale.PosizioneProtocollo,
                            NomeFile = principale.NomeFile,
                            MimeType = principale.MimeType,
                            ContentByte = byteArray,
                            Length = byteArray.Length,
                            IsPrincipal = true
                        };

                        XR_DEM_VERSIONI_DOCUMENTO toAddVersione = new XR_DEM_VERSIONI_DOCUMENTO()
                        {
                            Id_Documento = lastVersione.Id_Documento,
                            N_Versione = lastVersione.N_Versione + 1,
                            DataUltimaModifica = DateTime.Now
                        };

                        XR_DEM_ALLEGATI_VERSIONI toAddAllegatoVersione = new XR_DEM_ALLEGATI_VERSIONI()
                        {
                            IdAllegato = toAddAllegato.Id,
                            IdVersione = toAddVersione.Id
                        };

                        dbIncentivi.XR_ALLEGATI.Add(toAddAllegato);
                        dbIncentivi.XR_DEM_VERSIONI_DOCUMENTO.Add(toAddVersione);
                        dbIncentivi.XR_DEM_ALLEGATI_VERSIONI.Add(toAddAllegatoVersione);
                        dbIncentivi.SaveChanges();

                        string txLog = String.Format("Aggiunto nuovo file allegato alla richiesta su XR_ALLEGATI per la matricola: {0} nominativo: {1} documento id: {2} Prot.num.: {3} riferimento idProtocollo: {4}", pmatr, nominativo, doc.Documento.Id, applicaPResult.Protocollo, applicaPResult.Id_Documento);

                        ScriviLogAzioni(txLog, "Insert XR_ALLEGATI");
                    }
                }

                if (byteArray == null)
                {
                    string descDest = "";
                    string errDesc = "";
                    if (!String.IsNullOrEmpty(doc.Documento.MatricolaDestinatario))
                    {
                        descDest = DematerializzazioneManager.GetNominativoByMatricola(doc.Documento.MatricolaDestinatario);
                        errDesc = descDest + " - " + doc.Documento.Descrizione + " non firmato. Errore: documento non protocollato \r\n";
                    }
                    else
                    {
                        errDesc = doc.Documento.Descrizione + " non firmato. Errore: documento non protocollato \r\n";
                    }
                    Response.InfoAggiuntive += errDesc;
                    Response.InfoAggiuntive += " \r\n";
                    ScriviLogErrore("Documento non protocollato id_doc " + pdfCarrGen.id_documento);
                    Response.DocsInErrore.Add(new DocInErrore()
                    {
                        data_fine = "",
                        data_inizio = "",
                        sedegapp = "",
                        esito = "Documento non protocollato id_doc " + pdfCarrGen.id_documento
                    });
                    continue;
                }

                // 1. FIRMA DIGITALE SU PDF
                byte[] response;
                try
                {
                    string formatoData = "dd/MM/yyyy";
                    // string labelFirma = "Firmato digitalmente da " + nominativo + " in data " +
                    //                    DateTime.Now.ToString("dd/MM/yyyy HH:mm");
                    string labelFirma = "Firmato ${date}";




                    List<PosizioneProtocolloOBJ> obj = JsonConvert.DeserializeObject<List<PosizioneProtocolloOBJ>>(principale.PosizioneProtocollo);
                    var firmaPos = obj.Where(w => w.Oggetto.Equals("Firma")).FirstOrDefault();
                    int firmaPosLeft = 16;
                    int firmaPosTop = 760;
                    int pagina = -1;
                    if (firmaPos != null)
                    {
                        firmaPosLeft = (int)firmaPos.PosizioneLeft;
                        firmaPosTop = (int)firmaPos.PosizioneTop;
                        pagina = (int)firmaPos.NumeroPagina;
                    }
                    d.graphicalSignature = true;
                    d.pdfSignatureLayout = 1;
                    d.ocsp = false;
                    d.timestamp = false;
                    d.cosignCoordinates = false;

                    response = r.signPDF(cred,
                        byteArray,
                        false,
                        "SHA256",
                        null,
                        d,
                        "FirmaDigitale",
                        pagina,
                        firmaPosLeft,
                        (firmaPosTop + 20), // + 20 perchè quando viene letta la posizione dall'interfaccia di hris, viene considerato il margine sinistro superiore e non il centro del rettangolo che è di altezza 40
                        150,
                        25,
                        pmatr,
                        "FirmaDigitale",
                        Utente.SedeGapp(DateTime.Now),
                        formatoData, //"ddMMyyyy",
                        labelFirma,
                        7);

                    if (response == null || response.Length == 0)
                    {
                        string descDest = "";
                        string errDesc = "";
                        if (!String.IsNullOrEmpty(doc.Documento.MatricolaDestinatario))
                        {
                            descDest = DematerializzazioneManager.GetNominativoByMatricola(doc.Documento.MatricolaDestinatario);
                            errDesc = descDest + " - " + doc.Documento.Descrizione + " non firmato. Errore: la firma digitale ha restituito un doc vuoto \r\n";
                        }
                        else
                        {
                            errDesc = doc.Documento.Descrizione + " non firmato. Errore: la firma digitale ha restituito un doc vuoto \r\n";
                        }
                        Response.InfoAggiuntive += errDesc;
                        Response.InfoAggiuntive += " \r\n";
                        ScriviLogErrore("La firma digitale ha restituito un doc vuoto id_doc " + pdfCarrGen.id_documento);
                        Response.DocsInErrore.Add(new DocInErrore()
                        {
                            data_fine = "",
                            data_inizio = "",
                            sedegapp = "",
                            esito = "La firma digitale ha restituito un doc vuoto id_doc " + pdfCarrGen.id_documento
                        });
                        continue;
                    }
                    ScriviLogAzioni("Applicata firma digitale per la matricola: " + pmatr + " nominativo: " + nominativo + " documento id:" + doc.Documento.Id + " Prot. num. " + applicaPResult.Protocollo, "signPDF");
                }
                catch (Exception ex)
                {
                    var erroreFirmaAuthentication = db.MyRai_ErroriFirma.Where(x => x.codice == ex.Message).FirstOrDefault();
                    if (erroreFirmaAuthentication != null)
                    {
                        string descDest = "";
                        string errDesc = "";
                        if (!String.IsNullOrEmpty(doc.Documento.MatricolaDestinatario))
                        {
                            descDest = DematerializzazioneManager.GetNominativoByMatricola(doc.Documento.MatricolaDestinatario);
                            errDesc = descDest + " - " + doc.Documento.Descrizione + " " + erroreFirmaAuthentication.codice + " - " + erroreFirmaAuthentication.errore + " \r\n";
                        }
                        else
                        {
                            errDesc = doc.Documento.Descrizione + " " + erroreFirmaAuthentication.codice + " - " + erroreFirmaAuthentication.errore + " \r\n";
                        }
                        Response.InfoAggiuntive += errDesc;
                        Response.InfoAggiuntive += " \r\n";
                        ScriviLogErrore("Errore bloccante durante autenticazione: " + erroreFirmaAuthentication.codice + "-" + erroreFirmaAuthentication.errore);
                        Response.DocsInErrore.Add(new DocInErrore()
                        {
                            data_fine = "",
                            data_inizio = "",
                            sedegapp = "", // doc.sedegapp ,
                            esito = "Errore bloccante durante autenticazione: " + erroreFirmaAuthentication.codice + "-" + erroreFirmaAuthentication.errore
                        });
                        Response.IsAuthError = true;
                        break;
                    }
                    else
                    {
                        string descDest = "";
                        string errDesc = "";
                        if (!String.IsNullOrEmpty(doc.Documento.MatricolaDestinatario))
                        {
                            descDest = DematerializzazioneManager.GetNominativoByMatricola(doc.Documento.MatricolaDestinatario);
                            errDesc = descDest + " - " + doc.Documento.Descrizione + " non firmato. Errore: " + ex.Message + " \r\n";
                        }
                        else
                        {
                            errDesc = doc.Documento.Descrizione + " non firmato. Errore: " + ex.Message + " \r\n";
                        }
                        Response.InfoAggiuntive += errDesc;
                        Response.InfoAggiuntive += " \r\n";
                        ScriviLogErrore("La firma digitale ha risposto con errori:" + ex.Message);
                        Response.DocsInErrore.Add(new DocInErrore()
                        {
                            data_fine = "",
                            data_inizio = "",
                            sedegapp = "", // doc.sedegapp ,
                            esito = "La firma digitale ha risposto con errori:" + ex.Message
                        });
                    }

                    continue;
                }

                // 2. AGGIUNGI DOCUMENTO AL PROTOCOLLO
                try
                {
                    if (response != null && response.Length > 0)
                    {
                        string nominativoDestinatario = DematerializzazioneManager.GetNominativoByMatricola(doc.Documento.MatricolaDestinatario);
                        string desc = doc.Documento.Descrizione + " " + nominativoDestinatario;
                        if (desc.Length > 80)
                        {
                            desc = desc.Substring(0, 80);
                        }

                        string base64String = Convert.ToBase64String(response, 0, response.Length);

                        bool uploadDocumentoProtocollato = InserisciAllegatoProtocollo("42936", pmatr, applicaPResult.Id_Documento,
                            nomefile, desc, base64String, "1", "0");

                        if (!uploadDocumentoProtocollato)
                        {
                            throw new Exception("Si è verificato un errore durante l'upload del documento al servizio di protocollo");
                        }

                        string txLog = String.Format("Allegato aggiunto al protocollo per la matricola: {0} nominativo: {1} documento id: {2} Prot.num.: {3} riferimento idProtocollo: {4}", pmatr, nominativo, doc.Documento.Id, applicaPResult.Protocollo, applicaPResult.Id_Documento);

                        ScriviLogAzioni(txLog, "InserisciAllegatoProtocollo");
                    }
                    else
                    {
                        // non dovrebbe farlo mai
                        Response.InfoAggiuntive += "Documento PDF " + doc.Documento.Descrizione + " con id " + pdfCarrGen.id_documento + " non firmato.\r\n";
                        Response.InfoAggiuntive += " \r\n";
                        throw new Exception("Documento non firmato");
                    }
                }
                catch (Exception ex)
                {
                    if (response != null && response.Length > 0)
                    {
                        string descDest = "";
                        string errDesc = "";
                        if (!String.IsNullOrEmpty(doc.Documento.MatricolaDestinatario))
                        {
                            descDest = DematerializzazioneManager.GetNominativoByMatricola(doc.Documento.MatricolaDestinatario);
                            errDesc = descDest + " - " + doc.Documento.Descrizione + " non firmato. Errore: Non è stato possibile associare l'allegato al protocollo: " + ex.Message + "\r\n";
                        }
                        else
                        {
                            errDesc = doc.Documento.Descrizione + " non firmato. Errore: Non è stato possibile associare l'allegato al protocollo: " + ex.Message + "\r\n";
                        }
                        Response.InfoAggiuntive += errDesc;
                        Response.InfoAggiuntive += " \r\n";
                    }

                    ScriviLogErrore("Non è stato possibile associare l'allegato al protocollo: " + ex.Message);
                    Response.DocsInErrore.Add(new DocInErrore()
                    {
                        data_fine = "",
                        data_inizio = "",
                        sedegapp = "", // doc.sedegapp ,
                        esito = "Non è stato possibile creare il nuovo allegato firmato digitalmente: " + ex.Message
                    });

                    continue;
                }

                // 3. INSERISCI NUOVO ALLEGATO SU XR_ALLEGATI
                try
                {
                    if (response != null && response.Length > 0)
                    {
                        // reperisce la versione associata all'allegato
                        int idAllegato = principale.Id;
                        var temp1 = dbIncentivi.XR_DEM_ALLEGATI_VERSIONI.Where(w => w.IdAllegato.Equals(idAllegato)).OrderByDescending(w => w.IdVersione).FirstOrDefault();

                        int idLastVersione = temp1.IdVersione;
                        var lastVersione = dbIncentivi.XR_DEM_VERSIONI_DOCUMENTO.Where(w => w.Id.Equals(idLastVersione)).FirstOrDefault();

                        // reperimento metadati della richiesta
                        XR_DEM_DOCUMENTI docToUpdate = dbIncentivi.XR_DEM_DOCUMENTI.Where(w => w.Id.Equals(pdfCarrGen.id_documento)).FirstOrDefault();
                        if (docToUpdate == null)
                        {
                            throw new Exception("Richiesta non trovata");
                        }

                        docToUpdate.CodiceProtocollatore = "42936";
                        docToUpdate.NumeroProtocollo = applicaPResult.Protocollo;
                        docToUpdate.Prot_Id_Documento = applicaPResult.Id_Documento;
                        // imposta lo stato del documento come firmato digitalmente
                        docToUpdate.Id_Stato = (int)StatiDematerializzazioneDocumenti.FirmatoDigitalmente;
                        docToUpdate.DataFirma = DateTime.Now;

                        // controlla se ci sono stati successivi a firmato digitalmente
                        // se si è se lo stato successivo è InviatoAlDipendente, allora imposta il nuovo
                        // stato del documento come InviatoAlDipendente
                        int nuovoStato = DematerializzazioneManager.GetNextIdStato((int)StatiDematerializzazioneDocumenti.FirmatoDigitalmente, docToUpdate.Id_WKF_Tipologia);
                        if (nuovoStato == (int)StatiDematerializzazioneDocumenti.InviatoAlDipendente)
                        {
                            docToUpdate.Id_Stato = (int)StatiDematerializzazioneDocumenti.InviatoAlDipendente;
                        }

                        // crea un nuovo allegato
                        XR_ALLEGATI toAddAllegato = new XR_ALLEGATI()
                        {
                            PosizioneProtocollo = principale.PosizioneProtocollo,
                            NomeFile = principale.NomeFile,
                            MimeType = principale.MimeType,
                            ContentByte = response,
                            Length = response.Length,
                            IsPrincipal = true
                        };

                        XR_DEM_VERSIONI_DOCUMENTO toAddVersione = new XR_DEM_VERSIONI_DOCUMENTO()
                        {
                            Id_Documento = lastVersione.Id_Documento,
                            N_Versione = lastVersione.N_Versione + 1,
                            DataUltimaModifica = DateTime.Now
                        };

                        XR_DEM_ALLEGATI_VERSIONI toAddAllegatoVersione = new XR_DEM_ALLEGATI_VERSIONI()
                        {
                            IdAllegato = toAddAllegato.Id,
                            IdVersione = toAddVersione.Id
                        };

                        dbIncentivi.XR_ALLEGATI.Add(toAddAllegato);
                        dbIncentivi.XR_DEM_VERSIONI_DOCUMENTO.Add(toAddVersione);
                        dbIncentivi.XR_DEM_ALLEGATI_VERSIONI.Add(toAddAllegatoVersione);
                        dbIncentivi.SaveChanges();

                        string txLog = String.Format("Aggiunto nuovo file allegato alla richiesta su XR_ALLEGATI per la matricola: {0} nominativo: {1} documento id: {2} Prot.num.: {3} riferimento idProtocollo: {4}", pmatr, nominativo, doc.Documento.Id, applicaPResult.Protocollo, applicaPResult.Id_Documento);

                        ScriviLogAzioni(txLog, "Insert XR_ALLEGATI");
                    }
                    else
                    {
                        // non dovrebbe farlo mai
                        Response.InfoAggiuntive += "Documento PDF " + doc.Documento.Descrizione + " con id " + pdfCarrGen.id_documento + " non firmato.\r\n";
                        Response.InfoAggiuntive += " \r\n";
                        throw new Exception("Documento non firmato");
                    }
                }
                catch (Exception ex)
                {
                    if (response != null && response.Length > 0)
                    {
                        string descDest = "";
                        string errDesc = "";
                        if (!String.IsNullOrEmpty(doc.Documento.MatricolaDestinatario))
                        {
                            descDest = DematerializzazioneManager.GetNominativoByMatricola(doc.Documento.MatricolaDestinatario);
                            errDesc = descDest + " - " + doc.Documento.Descrizione + " non firmato. Errore: Non è stato possibile creare il nuovo allegato firmato digitalmente " + ex.Message + " \r\n";
                        }
                        else
                        {
                            errDesc = doc.Documento.Descrizione + " non firmato. Errore: Non è stato possibile creare il nuovo allegato firmato digitalmente " + ex.Message + " \r\n";
                        }
                        Response.InfoAggiuntive += errDesc;
                        Response.InfoAggiuntive += " \r\n";
                    }

                    ScriviLogErrore("Non è stato possibile creare il nuovo allegato firmato digitalmente: " + ex.Message);
                    Response.DocsInErrore.Add(new DocInErrore()
                    {
                        data_fine = "",
                        data_inizio = "",
                        sedegapp = "", // doc.sedegapp ,
                        esito = "Non è stato possibile creare il nuovo allegato firmato digitalmente: " + ex.Message
                    });

                    continue;
                }

                //4. CANCELLA DA CARRELLO
                try
                {
                    if (response != null && response.Length > 0)
                    {
                        RimuoviDaCarrelloGenerico(pdfCarrGen.id);

                        ScriviLogAzioni("Eliminazione della richiesta id: " + pdfCarrGen.id_documento + " dal carrello", "RimuoviDaCarrelloGenerico");
                    }
                    else
                    {
                        // non dovrebbe farlo mai
                        Response.InfoAggiuntive += "Documento PDF " + doc.Documento.Descrizione + " con id " + pdfCarrGen.id_documento + " non firmato.\r\n";
                        Response.InfoAggiuntive += " \r\n";
                        throw new Exception("Documento non firmato");
                    }
                }
                catch (Exception ex)
                {
                    if (response != null && response.Length > 0)
                    {
                        string descDest = "";
                        string errDesc = "";
                        if (!String.IsNullOrEmpty(doc.Documento.MatricolaDestinatario))
                        {
                            descDest = DematerializzazioneManager.GetNominativoByMatricola(doc.Documento.MatricolaDestinatario);
                            errDesc = descDest + " - " + doc.Documento.Descrizione + " firmato. Errore DB cancellazione da carrelloGenerico: " + ex.Message + " \r\n";
                        }
                        else
                        {
                            errDesc = doc.Documento.Descrizione + " firmato. Errore DB cancellazione da carrelloGenerico: " + ex.Message + " \r\n";
                        }
                        Response.InfoAggiuntive += errDesc;
                        Response.InfoAggiuntive += " \r\n";
                    }

                    ScriviLogErrore("Errore DB cancellazione da carrelloGenerico-" + ex.Message);
                    Logger.LogErrori(new MyRai_LogErrori()
                    {
                        applicativo = "Portale",
                        data = DateTime.Now,
                        error_message = ex.ToString(),
                        matricola = CommonManager.GetCurrentUserMatricola(),
                        provenienza = "RimuoviDaCarrelloGenerico"
                    });

                    Response.DocsInErrore.Add(new DocInErrore()
                    {
                        data_fine = "",
                        data_inizio = "",
                        sedegapp = "", //doc.sedegapp ,
                        esito = "Errore DB cancellazione da carrelloGenerico-" + ex.Message
                    });
                    continue;
                }

                FirmatiConSuccesso++;
                string dDest = "";
                string dDesc = "";
                if (!String.IsNullOrEmpty(doc.Documento.MatricolaDestinatario))
                {
                    dDest = DematerializzazioneManager.GetNominativoByMatricola(doc.Documento.MatricolaDestinatario);
                    dDesc = dDest + " - " + doc.Documento.Descrizione + " esito ok \r\n";
                }
                else
                {
                    dDesc = doc.Documento.Descrizione + " esito ok \r\n";
                }
                Response.InfoAggiuntive += dDesc;
                Response.InfoAggiuntive += " \r\n";

            }

            r.closeSignatureSession(cred, sessionToken);

            ScriviLogAzioni("Chiusura sessione di firma per la matricola: " + pmatr, "closeSignatureSession");

            if (Response.DocsInErrore == null || Response.DocsInErrore.Count == 0)
                Response.esito = "OK";
            else
                Response.esito = "ERRORI";

            if (FirmatiConSuccesso == 0)
            {
                Response.esito = "KO";
            }

            Response.FirmatiOk = FirmatiConSuccesso;
            Response.InCarrello = 0;
            return Response;
        }

        private static RichiestaDoc GetDocumentData(int idDocument)
        {
            RichiestaDoc result = null;

            try
            {
                IncentiviEntities db = new IncentiviEntities();
                var item = db.XR_DEM_DOCUMENTI.Include("XR_DEM_VERSIONI_DOCUMENTO").Where(w => w.Id.Equals(idDocument)).FirstOrDefault();

                if (item != null)
                {
                    result = new RichiestaDoc();
                    result.Documento = new XR_DEM_DOCUMENTI();
                    result.Documento = item;
                    result.Allegati = new List<XR_ALLEGATI>();
                    var versioni = item.XR_DEM_VERSIONI_DOCUMENTO.Where(w => w.Id_Documento.Equals(item.Id) && !w.Deleted).OrderByDescending(w => w.Id).ThenByDescending(x => x.N_Versione).ToList();

                    if (versioni != null && versioni.Any())
                    {
                        List<int> idVersioni = new List<int>();
                        idVersioni.AddRange(versioni.Select(w => w.Id).ToList());

                        // prende tutti gli idversione e prende tutti gli idallegati associati agli idversione
                        var AllVers = db.XR_DEM_ALLEGATI_VERSIONI.Where(w => idVersioni.Contains(w.IdVersione)).OrderByDescending(w => w.IdAllegato).ToList();

                        if (AllVers != null && AllVers.Any())
                        {
                            List<int> idsAllegati = new List<int>();
                            idsAllegati.AddRange(AllVers.Select(w => w.IdAllegato).ToList());
                            if (idsAllegati != null && idsAllegati.Any())
                            {
                                var all = db.XR_ALLEGATI.Where(w => idsAllegati.Contains(w.Id)).OrderByDescending(w => w.Id).ToList();
                                if (all != null && all.Any())
                                {
                                    // presi tutti gli allegati associati al documento
                                    // bisogna scartare quelli che hanno più versioni e
                                    // va presa solo la versione più recente
                                    // purtroppo per i vecchi documenti per vedere se c'è 
                                    // una versione differente dello stesso file, bisogna
                                    // controllare il nome file e idallegato
                                    // stesso nome file idallegato maggiore sarà la versione più recente
                                    // dell'allegato
                                    foreach (var a in all)
                                    {
                                        // prende l'allegato con lo stesso nome ma con id più grande
                                        int idMax = all.Where(w => w.NomeFile == a.NomeFile).Max(x => x.Id);

                                        // se già presente nell'elenco allora l'elemento è stato già analizzato
                                        bool giaEsiste = result.Allegati.Count(w => w.Id == idMax) == 1;

                                        // se non esiste lo aggiunge all'elenco finale
                                        if (!giaEsiste)
                                        {
                                            var inserire = result.Allegati.Where(w => w.Id == idMax).FirstOrDefault();
                                            result.Allegati.Add(inserire);
                                        }
                                    }
                                }
                            }
                        }

                    }
                }

                //if (item != null)
                //{
                //    result = new RichiestaDoc();
                //    result.Documento = new XR_DEM_DOCUMENTI();
                //    result.Documento = item;
                //    result.Allegati = new List<XR_ALLEGATI>();

                //    var versioni = item.XR_DEM_VERSIONI_DOCUMENTO.Where(w => w.Id_Documento.Equals(item.Id)).OrderByDescending(w => w.Id).ToList();

                //    if (versioni != null && versioni.Any())
                //    {
                //        foreach (var v in versioni)
                //        {
                //            var AllVers = db.XR_DEM_ALLEGATI_VERSIONI.Where(w => w.IdVersione == v.Id).FirstOrDefault();

                //            if (AllVers != null)
                //            {
                //                var allegato = db.XR_ALLEGATI.Where(w => w.Id.Equals(AllVers.IdAllegato)).FirstOrDefault();

                //                if (allegato != null)
                //                {
                //                    result.Allegati.Add(allegato);
                //                }
                //            }
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                result = null;
            }

            return result;
        }

        private static string SerializeObject<T>(T oggetto) where T : class
        {
            string result;
            var emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            var serializer = new XmlSerializer(oggetto.GetType());
            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            using (Utf8StringWriter sw = new Utf8StringWriter())
            {
                using (var wri = XmlWriter.Create(sw, settings))
                {
                    serializer.Serialize(wri, oggetto, emptyNamespaces);
                    result = sw.ToString();
                }
            }

            return result;
        }

        private static ApplicaProtocolloResult ApplicaProtocollo(string matricolaFirma, string nominativo, int idDoc)
        {
            DateTime ora = DateTime.Now;
            IncentiviEntities db = new IncentiviEntities();
            XR_ALLEGATI allegato = null;
            //XR_DEM_VERSIONI_DOCUMENTO versione = null;
            ApplicaProtocolloResult result = new ApplicaProtocolloResult();
            result.File = null;
            result.Id_Documento = "";
            result.Protocollo = "";

            try
            {
                #region CreaProtocollo

                var item = db.XR_DEM_DOCUMENTI.Include("XR_DEM_VERSIONI_DOCUMENTO").Where(w => w.Id.Equals(idDoc)).FirstOrDefault();

                if (item != null)
                {
                    var versioni = item.XR_DEM_VERSIONI_DOCUMENTO.Where(w => w.Id_Documento.Equals(item.Id) && !w.Deleted).OrderByDescending(w => w.Id).ThenByDescending(x => x.N_Versione).ToList();

                    if (versioni != null && versioni.Any())
                    {
                        List<int> idVersioni = new List<int>();
                        idVersioni.AddRange(versioni.Select(w => w.Id).ToList());

                        // prende tutti gli idversione e prende tutti gli idallegati associati agli idversione
                        var AllVers = db.XR_DEM_ALLEGATI_VERSIONI.Where(w => idVersioni.Contains(w.IdVersione)).OrderByDescending(w => w.IdAllegato).ToList();

                        if (AllVers != null && AllVers.Any())
                        {
                            List<int> idsAllegati = new List<int>();
                            idsAllegati.AddRange(AllVers.Select(w => w.IdAllegato).ToList());
                            if (idsAllegati != null && idsAllegati.Any())
                            {
                                var allegati = db.XR_ALLEGATI.Where(w => idsAllegati.Contains(w.Id) && w.IsPrincipal).OrderByDescending(w => w.Id).ToList();
                                if (allegati != null && allegati.Any())
                                {
                                    // presi tutti gli allegati associati al documento
                                    // bisogna scartare quelli che hanno più versioni e
                                    // va presa solo la versione più recente
                                    // purtroppo per i vecchi documenti per vedere se c'è 
                                    // una versione differente dello stesso file, bisogna
                                    // controllare il nome file e idallegato
                                    // stesso nome file idallegato maggiore sarà la versione più recente
                                    // dell'allegato
                                    foreach (var a in allegati)
                                    {
                                        // prende l'allegato con lo stesso nome ma con id più grande
                                        int idMax = allegati.Where(w => w.NomeFile == a.NomeFile).Max(x => x.Id);

                                        // se già presente nell'elenco allora l'elemento è stato già analizzato
                                        bool giaEsiste = allegato.Id == idMax;

                                        // se non esiste lo aggiunge all'elenco finale
                                        if (!giaEsiste)
                                        {
                                            var inserire = allegati.Where(w => w.Id == idMax).FirstOrDefault();
                                            allegato = inserire;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                //if (item != null)
                //{
                //    var versioni = item.XR_DEM_VERSIONI_DOCUMENTO.Where(w => w.Id_Documento.Equals(item.Id)).OrderByDescending(w => w.Id).ToList();

                //    if (versioni != null && versioni.Any())
                //    {
                //        List<int> idsVersioni = new List<int>();
                //        idsVersioni.AddRange(versioni.Select(w => w.Id).ToList());

                //        var allVers = db.XR_DEM_ALLEGATI_VERSIONI.Where(w => idsVersioni.Contains(w.IdVersione)).ToList();

                //        if (allVers != null && allVers.Any())
                //        {
                //            List<int> idAllVers = new List<int>();
                //            idAllVers.AddRange(allVers.Select(w => w.IdAllegato).ToList());

                //            allegato = db.XR_ALLEGATI.Where(w => idAllVers.Contains(w.Id) && w.IsPrincipal).FirstOrDefault();
                //        }
                //    }
                //}

                // applicazione del protocollo
                if (!String.IsNullOrEmpty(allegato.PosizioneProtocollo))
                {
                    Protocollo prot = new Protocollo();
                    prot.Mittente = matricolaFirma;
                    string dest = "";
                    try
                    {
                        dest = CommonManager.GetEmailPerMatricola(item.MatricolaDestinatario);
                    }
                    catch (Exception ex)
                    {
                        ScriviLogErrore("Impossibile reperire l'indirizzo mail destinatario " + item.MatricolaDestinatario);
                        throw new Exception("Impossibile reperire l'indirizzo mail destinatario " + ex.Message);
                    }

                    List<Destinatario> _d = new List<Destinatario>();

                    prot.Destinatari = new Destinatari();
                    prot.Destinatari.Destinatario = new List<Destinatario>();
                    _d.Add(new Destinatario()
                    {
                        IndirizzoMail = dest,
                        TipoCanale = "mail",
                        Text = dest
                    });

                    prot.Destinatari.Destinatario = _d;

                    string nominativoDestinatario = DematerializzazioneManager.GetNominativoByMatricola(item.MatricolaDestinatario);

                    prot.Oggetto = item.Descrizione + " " + nominativoDestinatario;
                    prot.DataSpedizione = ora.ToString("yyyy-MM-dd");
                    string strProt = SerializeObject(prot);
                    string codiceProtocollatore = "42936";

                    //URL:  http://raiconnectcoll.servizi.rai.it/rai_ruo_ws.asmx
                    string user = "srv_raiconnect_ruo";
                    string pwd = "6E6asOXO";
                    rai_ruo_ws client = new rai_ruo_ws();
                    client.Credentials = new System.Net.NetworkCredential(user, pwd);
                    //client.UseDefaultCredentials = true;

                    // codice creatore del documento
                    var tmpProt = client.creaProtocollo(codiceProtocollatore, "P" + item.MatricolaCreatore, strProt);

                    CreaProtocollo outputServizio = new CreaProtocollo();

                    var p = SerializerHelper.DeserializeXml(tmpProt, outputServizio.GetType());
                    outputServizio = (CreaProtocollo)p;

                    if (outputServizio.Errore != null &&
                        outputServizio.Errore.Id_errore != "0" &&
                        !String.IsNullOrEmpty(outputServizio.Errore.Text))
                    {
                        throw new Exception(outputServizio.Errore.Text);
                    }

                    item.NumeroProtocollo = outputServizio.Identificativo;
                    item.CodiceProtocollatore = codiceProtocollatore;
                    result.Id_Documento = outputServizio.Id_documento;
                    result.Protocollo = outputServizio.Identificativo;

                    // applica il protocollo sul documento PDF
                    List<PosizioneProtocolloOBJ> obj = JsonConvert.DeserializeObject<List<PosizioneProtocolloOBJ>>(allegato.PosizioneProtocollo);
                    var protPos = obj.Where(w => w.Oggetto.Equals("Protocollo")).FirstOrDefault();
                    var dataPos = obj.Where(w => w.Oggetto.Equals("Data")).FirstOrDefault();
                    if (protPos != null)
                    {
                        float? dataPosLeft = null;
                        float? dataPosTop = null;
                        if (dataPos != null)
                        {
                            dataPosLeft = dataPos.PosizioneLeft;
                            dataPosTop = dataPos.PosizioneTop;
                        }

                        // alla coordinata top viene aggiunto 20, perchè quando da hris vengono posizionati protocollo e data
                        // le coordinate vengono prese in base al margine superiore sinistro del rettangolo di selezione e non dal centro
                        // poichè il rettangolo è di altezza 40, viene considerato + 20 perchè deve partire dalla sua metà
                        if (allegato.ContentBytePDF != null)
                        {
                            result.File = ApplicaProtocolloSulPDF(outputServizio.Identificativo, protPos.PosizioneLeft, (protPos.PosizioneTop + 20), dataPosLeft, (dataPosTop + 20), allegato.ContentBytePDF);
                        }
                        else
                        {
                            result.File = ApplicaProtocolloSulPDF(outputServizio.Identificativo, protPos.PosizioneLeft, (protPos.PosizioneTop + 20), dataPosLeft, (dataPosTop + 20), allegato.ContentByte);
                        }
                    }

                    if (result.File == null || result.File.Length == 0)
                    {
                        throw new Exception("Errore nell'applicazione del protocollo");
                    }
                }
                #endregion

            }
            catch (Exception ex)
            {
                result.File = null;
                ScriviLogErrore("Errore durante l'applicazione del protocollo " + ex.Message);
            }

            return result;
        }

        private static byte[] ApplicaProtocolloSulPDF(string protocollo, float left, float top,
                                    float? dataPosLeft, float? dataPosTop, byte[] originePDF)
        {
            byte[] result = null;

            try
            {
                using (var reader = new PdfReader(originePDF))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        Document document = null;
                        document = new Document(reader.GetPageSizeWithRotation(1));

                        var writer = PdfWriter.GetInstance(document, ms);
                        float width = document.PageSize.Width;
                        float height = document.PageSize.Height;

                        document.Open();

                        for (var i = 1; i <= reader.NumberOfPages; i++)
                        {
                            document.NewPage();

                            var baseFont = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                            var importedPage = writer.GetImportedPage(reader, i);

                            var contentByte = writer.DirectContent;

                            var rotation = reader.GetPageRotation(i);

                            switch (rotation)
                            {
                                case 90:
                                    contentByte.AddTemplate(importedPage, 0, -1, 1, 0, 0, reader.GetPageSizeWithRotation(i).Height);
                                    break;
                                // TODO case 180
                                case 270:
                                    contentByte.AddTemplate(importedPage, 0, 1, -1, 0, reader.GetPageSizeWithRotation(i).Width, 0);
                                    break;
                                default:
                                    contentByte.AddTemplate(importedPage, 0, 0);
                                    break;
                            }

                            contentByte.BeginText();
                            contentByte.SetFontAndSize(baseFont, 12);

                            if ((595 - left) < 142)
                            {
                                // il margine sinistro va oltre il documento
                                left = 595 - 142;
                            }

                            if (i == 1)
                            {
                                contentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT, protocollo, left, 842 - top - 12, 0);

                                if (dataPosLeft.HasValue && dataPosTop.HasValue)
                                {
                                    DateTime adesso = DateTime.Now;
                                    contentByte.ShowTextAligned(PdfContentByte.ALIGN_LEFT,
                                        adesso.ToString("dd/MM/yyyy"),
                                        dataPosLeft.GetValueOrDefault(),
                                        842 - dataPosTop.GetValueOrDefault() - 12,
                                        0);
                                }
                            }

                            contentByte.EndText();
                        }

                        document.Close();
                        writer.Close();
                        result = ms.ToArray();
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        private static void ScriviLogErrore(string testo)
        {
            var db = new myRaiData.digiGappEntities();
            //test inutile

            myRaiData.MyRai_LogErrori a = new myRaiData.MyRai_LogErrori()
            {
                matricola = CommonManager.GetCurrentUserMatricola(),
                data = DateTime.Now,
                error_message = testo,
                applicativo = "PORTALE",
                provenienza = "Dematerializzazione.FirmaDocumentiNelCarrello",
                feedback = ""
            };

            db.MyRai_LogErrori.Add(a);
            db.SaveChanges();
        }

        private static void ScriviLogAzioni(string testo, string operazione)
        {
            var db = new myRaiData.digiGappEntities();

            myRaiData.MyRai_LogAzioni a = new myRaiData.MyRai_LogAzioni()
            {
                matricola = CommonManager.GetCurrentUserMatricola(),
                data = DateTime.Now,
                descrizione_operazione = testo,
                applicativo = "PORTALE",
                provenienza = "Dematerializzazione.FirmaDocumentiNelCarrello",
                operazione = operazione
            };

            db.MyRai_LogAzioni.Add(a);
            db.SaveChanges();
        }

        public static bool CreaProtocollo(string idRuolo, string oggetto, string matrMittente, string matrDest, string pMatrCreatore, out CreaProtocollo outputServizio)
        {
            bool result = false;
            outputServizio = null;
            string tmpProt = "";

            try
            {
                Protocollo prot = new Protocollo();
                prot.Mittente = matrMittente;
                string dest = CommonHelper.GetEmailPerMatricola(matrDest);

                List<Destinatario> _d = new List<Destinatario>();

                prot.Destinatari = new Destinatari();
                prot.Destinatari.Destinatario = new List<Destinatario>();
                _d.Add(new Destinatario()
                {
                    IndirizzoMail = dest,
                    TipoCanale = "mail",
                    Text = dest
                });

                prot.Destinatari.Destinatario = _d;

                prot.Oggetto = oggetto;
                prot.DataSpedizione = DateTime.Today.ToString("yyyy-MM-dd");
                string strProt = SerializeObject(prot);

                //URL:  http://raiconnectcoll.servizi.rai.it/rai_ruo_ws.asmx
                string user = "srv_raiconnect_ruo";
                string pwd = "6E6asOXO";
                rai_ruo_ws client = new rai_ruo_ws();
                client.Credentials = new System.Net.NetworkCredential(user, pwd);
                //client.UseDefaultCredentials = true;

                // codice creatore del documento
                tmpProt = client.creaProtocollo(idRuolo, pMatrCreatore, strProt);

                outputServizio = new CreaProtocollo();
                var p = SerializerHelper.DeserializeXml(tmpProt, outputServizio.GetType());
                outputServizio = (CreaProtocollo)p;

                if (outputServizio.Errore != null &&
                    outputServizio.Errore.Id_errore != "0" &&
                    !String.IsNullOrEmpty(outputServizio.Errore.Text))
                {
                    throw new Exception(outputServizio.Errore.Text);
                }

                result = true;
            }
            catch (Exception ex)
            {
                ScriviLogErrore("Errore durante la creazione del protocollo " + ex.Message + "\r\n"+tmpProt);
            }
            return result;
        }

        public static bool InserisciAllegatoProtocollo(string id_ruolo, string pMatricola,
            string id_documento, string nome_file, string descrizione_file,
            string base64, string filePrincipale, string idAttach)
        {
            bool result = false;
            DateTime ora = DateTime.Now;
            string tmpProt = "";

            try
            {
                //id_ruolo = "42936";

                //URL:  http://raiconnectcoll.servizi.rai.it/rai_ruo_ws.asmx
                string user = "srv_raiconnect_ruo";
                string pwd = "6E6asOXO";
                rai_ruo_ws client = new rai_ruo_ws();
                client.Credentials = new System.Net.NetworkCredential(user, pwd);
                //client.UseDefaultCredentials = true;

                tmpProt = client.inserisciAllegato(id_ruolo, pMatricola, id_documento, nome_file, descrizione_file, base64, filePrincipale, idAttach);
                InserisciAllegato outputServizio = new InserisciAllegato();

                var p = SerializerHelper.DeserializeXml(tmpProt, outputServizio.GetType());
                outputServizio = (InserisciAllegato)p;

                if (outputServizio.Errore != null &&
                    outputServizio.Errore.Id_errore != "0" &&
                    !String.IsNullOrEmpty(outputServizio.Errore.Text))
                {
                    throw new Exception(outputServizio.Errore.Text);
                }

                result = true;
            }
            catch (Exception ex)
            {
                ScriviLogErrore("Errore durante l'applicazione del protocollo " + ex.Message+"\r\n"+tmpProt);
            }

            return result;
        }
    }

    public class FirmaDocumentiResponse
    {
        public string esito { get; set; }
        public int FirmatiOk { get; set; }
        public int InCarrello { get; set; }
        public bool IsAuthError { get; set; }
        public List<DocInErrore> DocsInErrore { get; set; }
        public string InfoAggiuntive { get; set; }
    }

    public class DocInErrore
    {
        public string data_inizio { get; set; }
        public string data_fine { get; set; }
        public string sedegapp { get; set; }
        public string esito { get; set; }
    }
}

/*
 000
Undefined error
001
No service specified. Usage: rest/provisioning | rest/profiling | rest/enroll
002
No action specified. Usage: rest/[service]/[action]
003
Service not found
004
Internal error
005
Unknown method/action
006
Error on method
007
Missing parameter
008
Parsing error: bad data structure
009
Mandatory value
010
Authentication failed
011
Not yet implemented
012
Invalid parameter
013
Module REST not active
014
Not found
015
015 Account has already an authentication token
016
016 No HSM available
017
017 Cluster not enabled. You can't specify certitifcate
018
018 CoSign error
019
019 Strong authentication failed
020
020 User already exists
021
021 Module SIGN not active
022
022 Module VERIFY not active
023
023 Module CIPHER not active
024
024 Module SIGNATUREFOLDERMOBILE not active
025
025 Service not available on Proxy instance
026
026 Device type not supported
027
027 User is locked
028
028 User no enabled
029
029 User is not activated
101
101 Generic error on signature
102
102 Content to sign is empty
ITAGILE S.r.l. Pag. 31 di 33
FirmaRemota – Modulo SIGN – XADES - VERIFY
Guida alla programmazione Versione 4.5.1.c
Code Description
103 103 Content to sign or to verify is not signed 104 104 Generic error in the method parameter 105 105 Parameter value is mandatory
106 106 Controfirma parameter is not correct 107 107 Signature field name already exist 108 108 Signature field name error 109 109 Wrong length of one or more digest 110 110 Digest algorithm is invalid 111 111 Digest algoritm SHA1 is not valid (CNAIPA 45) 112 112 Module not active 113 113 Signature not verified 114 114 Certificate not valid 115 115 CA Root non trusted 116 116 Generic error on signature verify
117 117 Content type parameter is mandatory
118 118 Digest is mandatory
119 119 Certificare reference is missing 120 120 Error on graphical signature 121 121 Error on sign for the text parameter 122 122 Error on sign for pdf page number parameter 123 123 Error on sign for pdf password 124 124 Error on sign for date formata parameter 125 125 Certificate is empty
126 126 Certificate is wrong 127 127 Certificate is expired 128 128 Certificate is not yet valid 129 129 Digest sign and timestamp are not equal 130 130 Timestamp authority is not trusted 131 131 Timestamp certificate is invalid 132 132 Generic error on timestamping 133 117 Signing time attributo is missing ITAGILE S.r.l. Pag. 32 di 33
FirmaRemota – Modulo SIGN – XADES - VERIFY
Guida alla programmazione Versione 4.5.1.c
Code Description
301 OCSP request is not accepted 302 OCSP response error 303 OCSP reqponse empty
304 OCSP response incomplete 305 Internal OCSP server error 306 Malformed request 307 Signature required for request 308 The server was too busy to answer 309 Not authorized to access server 310 Unknown OCSPResponse status code 311 The certificate is in a bad state 312 The certificate has been revoked 313 The certificate is in bad state 314 The certificate is not a valid date 315 OCSP connection timeout 316 Parser CRL error 317 Reader CRL error 318 CRL general error 319 Can not verify CRL for certificate 320 Can not download CRL from certificate from web 321 Can not download CRL from certificate from LDAP 322 CRL url is malformed 323 Generic error on get all certificate user 324 Generic error on get user default certificate
 
 */
