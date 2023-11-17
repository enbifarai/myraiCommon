using myRaiCommonModel;
using myRaiData;
using myRaiHelper;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using System;
using System.Collections.Generic;
using System.Linq;

namespace myRaiCommonManager
{

    public static class ResocontiCheckerManager
    {

       


        public static void FillAlerts()
        {
            DateTime Dstart = new DateTime(2019, 7, 1);

            var db = new digiGappEntities();
            var listaPDF = db.DIGIRESP_Archivio_PDF.Where(x =>
                            x.tipologia_pdf == "P" &&
                            x.data_inizio > Dstart
                            )
                            .OrderBy(x => x.sede_gapp)
                            .ThenBy(x => x.data_inizio)
                            .Select(x => new
                            {
                                data_inizio = x.data_inizio,
                                data_fine = x.data_fine,
                                data_stampa = x.data_stampa,
                                sede = x.sede_gapp,
                                idpdf = x.ID
                            })
                            .ToList();

            foreach (var item in listaPDF)
            {
                var eccRich = db.MyRai_Eccezioni_Richieste.Where(x =>
                           x.data_eccezione >= item.data_inizio &&
                           x.data_eccezione <= item.data_fine &&
                           x.id_stato == 20 &&
                           x.codice_sede_gapp == item.sede &&
                           x.data_validazione_primo_livello != null &&
                           x.data_validazione_primo_livello > item.data_stampa &&
                           x.MyRai_Richieste.TipoQuadratura == 1
                           ).ToList();
                if (eccRich.Any())
                {
                    foreach (var ecc in eccRich)
                    {
                        string rep = null;
                        if (ecc.MyRai_Richieste.reparto == null || ecc.MyRai_Richieste.reparto.Trim() == "0" || ecc.MyRai_Richieste.reparto.Trim() == "00" || ecc.MyRai_Richieste.reparto.Trim() == "")
                            rep = "";
                        else
                            rep = ecc.MyRai_Richieste.reparto;

                        myRai_Richieste_Alert_Resoconti alert = new myRai_Richieste_Alert_Resoconti()
                        {
                            alert_attivo = true,
                            data_fine_pdf = item.data_fine,
                            data_inizio_pdf = item.data_inizio,
                            data_ins_record = DateTime.Now,
                            id_pdf_da_rigenerare = item.idpdf,
                            id_richiesta = ecc.id_richiesta,
                            sedegapp = ecc.codice_sede_gapp == null ? ecc.MyRai_Richieste.codice_sede_gapp : ecc.codice_sede_gapp,
                            reparto = rep
                        };
                        db.myRai_Richieste_Alert_Resoconti.Add(alert);
                    }
                    db.SaveChanges();
                }
            }
        }


        public static NeedToReprint IsToReprint(DateTime dataInizio, DateTime dataFine, string sede)
        {
            var db = new digiGappEntities();
            NeedToReprint N = new NeedToReprint() { Esito=false };

            //var pdf = db.DIGIRESP_Archivio_PDF.Where(x =>x.tipologia_pdf=="P" && x.sede_gapp == sede && x.data_inizio == dataInizio && x.data_fine == dataFine).FirstOrDefault();
            var pdf = db.DIGIRESP_Archivio_PDF.Where(x => x.tipologia_pdf == "P" && x.sede_gapp == sede && x.data_inizio == dataInizio && x.data_fine == dataFine).Select(x=>new { x.data_stampa, x.contenuto_eccezioni }).FirstOrDefault();
            if (pdf == null)
                return N;

            var CacheResoconto = db.MyRai_Resoconti_GetPresenze.Where(x => x.sede == sede && x.data_inizio == dataInizio && x.data_fine == dataFine).FirstOrDefault();
            if (CacheResoconto == null)
                return N;

            List<DettaglioDifferenze> LD = new List<DettaglioDifferenze>();

            N.Esito= CacheResoconto.data_aggiornamento > pdf.data_stampa;
            if (N.Esito == true)
            {
                var presCache = Newtonsoft.Json.JsonConvert.DeserializeObject<presenzeResponse>(CacheResoconto.contenuto);
                var presPDF = Newtonsoft.Json.JsonConvert.DeserializeObject<presenzeResponse>(pdf.contenuto_eccezioni);
                if (presCache != null && presPDF != null && presCache.periodi != null && presPDF.periodi != null && presCache.periodi.Any() && presPDF.periodi.Any())
                {
                    foreach (var p in presPDF.periodi)
                    {
                        var pcache = presCache.periodi.Where(x => x.dipendente.matricola == p.dipendente.matricola).FirstOrDefault();
                        if (pcache != null)
                        {
                            if (pcache.deltaTotale != p.deltaTotale)
                            {
                                DettaglioDifferenze D = new DettaglioDifferenze();
                                D.matricola = pcache.dipendente.matricola;
                                D.deltaTotalePDF = p.deltaTotale;
                                D.deltaTotaleCache = pcache.deltaTotale;
                                LD.Add(D);
                            }
                        }
                    }
                }
                if (LD.Any())
                {
                    N.DettaglioDiff = LD;
                }
                else
                    N.Esito = false;
            }
             
            return N;
        }

        public static void ClearAlert(string sede, DateTime da, DateTime a)
        {
            var db = new digiGappEntities();
            List<myRai_Richieste_Alert_Resoconti> Alerts = db.myRai_Richieste_Alert_Resoconti.Where(x => x.sedegapp == sede && x.data_inizio_pdf == da && x.data_fine_pdf == a && x.alert_attivo == true).ToList();
            if (Alerts.Any())
            {
                foreach (var alert in Alerts)
                {
                    alert.alert_attivo = false;
                }
                db.SaveChanges();
            }
        }

        //public static DIGIRESP_Archivio_PDF ExistPdfResocontoGiaStampato(string sede, DateTime D)
        //{
        //    var db = new digiGappEntities();
        //    var pdf = db.DIGIRESP_Archivio_PDF.Where(x =>
        //                                                x.data_inizio <= D &&
        //                                                x.data_fine >= D &&
        //                                                x.tipologia_pdf == "P" &&
        //                                                x.sede_gapp == sede)
        //                                                .FirstOrDefault();
        //    return pdf;
        //}

        public static MyRai_Resoconti_GetPresenze ExistCacheResoconti(string sede, DateTime D)
        {
            var db = new digiGappEntities();
            return db.MyRai_Resoconti_GetPresenze.Where(x => x.data_inizio <= D && x.data_fine >= D && x.sede == sede).FirstOrDefault();
        }

        public static List<myRaiData.MyRai_Resoconti_GetPresenze> GetResocontiCacheCoinvolti(MyRai_Richieste Richiesta)
        {
            var db = new digiGappEntities();

            List<myRaiData.MyRai_Resoconti_GetPresenze> CacheCoinvolte = new List<myRaiData.MyRai_Resoconti_GetPresenze>();
            DateTime D = Richiesta.periodo_dal;
            while (D <= Richiesta.periodo_al)
            {
                var cache = ExistCacheResoconti(Richiesta.codice_sede_gapp, D);
                if (cache !=null)
                {
                    CacheCoinvolte.Add(cache);
                }
            }
            return CacheCoinvolte;
        }


        //public static List<myRaiData.DIGIRESP_Archivio_PDF> GetPdfResocontiCoinvolti(myRaiData.MyRai_Richieste Richiesta)
        //{
        //    var db = new digiGappEntities();

        //    List<myRaiData.DIGIRESP_Archivio_PDF> PDFcoinvolti = new List<myRaiData.DIGIRESP_Archivio_PDF>();

        //    DateTime D = Richiesta.periodo_dal;
        //    while (D <= Richiesta.periodo_al)
        //    {
        //        var pdf = ExistPdfResocontoGiaStampato(Richiesta.codice_sede_gapp, D);
        //        if (pdf != null && !PDFcoinvolti.Any(x => x.ID == pdf.ID))
        //        {
        //            PDFcoinvolti.Add(pdf);
        //        }
        //        D = D.AddDays(1);
        //    }
        //    return PDFcoinvolti;
        //}

        public static string InserisciRichiesteAlertPDF(List<myRaiData.DIGIRESP_Archivio_PDF> ListaPDF, myRaiData.MyRai_Richieste Richiesta)
        {
            if (ListaPDF == null || !ListaPDF.Any())
                return null;

            var db = new digiGappEntities();
            string rep = null;
            if (Richiesta.reparto == null || Richiesta.reparto.Trim() == "0" || Richiesta.reparto.Trim() == "00" || Richiesta.reparto.Trim() == "")
                rep = "";
            else
                rep = Richiesta.reparto;

            foreach (var pdf in ListaPDF)
            {
                try
                {
                    myRai_Richieste_Alert_Resoconti R = new myRai_Richieste_Alert_Resoconti()
                    {
                        alert_attivo = true,
                        data_ins_record = DateTime.Now,
                        id_pdf_da_rigenerare = pdf.ID,
                        id_richiesta = Richiesta.id_richiesta,
                        sedegapp = Richiesta.codice_sede_gapp,
                        reparto = rep,
                        data_inizio_pdf = pdf.data_inizio,
                        data_fine_pdf = pdf.data_fine
                    };
                    db.myRai_Richieste_Alert_Resoconti.Add(R);
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.LogErrori(new MyRai_LogErrori()
                    {
                        applicativo = "PORTALE",
                        data = DateTime.Now,
                        error_message = ex.ToString(),
                        matricola = CommonHelper.GetCurrentUserMatricola(),
                        provenienza = "InserisciRichiesteAlertPDF"
                    });
                    return ex.Message;
                }
            }
            return null;

        }

    }
}