using myRai.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Reflection;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using TimbratureCore;
using System.Text;
using myRaiHelper;
using myRaiCommonModel;
using myRaiCommonManager;
using System.Web;
using myRaiData;
using System.Web.Hosting;
using System.Diagnostics;
using myRai.Business;

namespace myRai.Controllers
{
    [SessionState(System.Web.SessionState.SessionStateBehavior.ReadOnly)]
    public partial class TechController : Controller
    {
        private const string v = "{id}";
        public ActionResult Index()


        {
            if (!Utente.IsAdmin())
            {
                throw ( new Exception( "Unauthorized" ) );
            }

            var db = new myRaiData.digiGappEntities();
            TechDashModel model = new TechDashModel();

            //var richieste = db.MyRai_Richieste.Select(x => new DaMatr() { d = x.data_richiesta, m = x.matricola_richiesta }).ToList();

            model.RichiestePerData = new List<data_Rich>();// GetRichiestePerData(richieste);
            model.MatricoleAllaData = new List<data_Matr>();// GetMatricoleAllaData(richieste);
            model.SediPdf = new List<sediPdfModel>();// GetSediPdfModel();
            model.SediDaAppr = new ApprovazioniPendingModel();
            model.AverageResponseTime = 0;
            model.AverageResponseTimeHH = 0;
            model.Intervallo = new IntervalloOsservazione();
            model.Intervallo.IntervalloRefresh = GetRefreshInterval();

            return View(model);
        }
        public ActionResult modpf(int anno, string matr, string sede)
        {
            var db = new digiGappEntities();
            var pf = db.MyRai_PianoFerie.Where(x => x.anno == anno && x.matricola == matr).FirstOrDefault();
            if (pf == null)
            {
                return Content("Piano ferie non trovato");
            }
            pf.sedegapp = sede.Trim().ToUpper();
            try
            {
                db.SaveChanges();
                Logger.LogAzione(new MyRai_LogAzioni()
                {
                    applicativo = "portale",
                    data = DateTime.Now,
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "tech/modpf",
                    descrizione_operazione = "anno:" + anno + ", matr:" + matr + ", sede:" + sede,
                    operazione = "Modifica sede PF"
                });
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
            return Content("OK eseguito ");
        }

        [HttpPost]
        public ActionResult DeployFiles(string ids,string action)
        {
            try
            {
                Process process = new Process();
                string path = HostingEnvironment.ApplicationPhysicalPath;
                path = Path.GetFullPath(Path.Combine(path, @"..\"));
                process.StartInfo.FileName = System.IO.Path.Combine(path, "pubb", "pubblicatore.exe");
                process.StartInfo.Arguments = " " + ids + " " + CommonManager.GetCurrentUserMatricola() + " " + action;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                process.WaitForExit();
                return Content("OK");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }

        }
        public ActionResult Deploy(string rb)
        {
            DeployModel model = new DeployModel();
            var db = new digiGappEntities();
            if (rb == "1")
            {
                model.IsRollBack = true;
                model.ListaFileDaPubblicare = db.MyRai_Pubblicazioni.Where(x => x.data_pubblicazione != null)
               .OrderBy(x => x.data_richiesta)
               .ToList();
            }
            else
                model.ListaFileDaPubblicare = db.MyRai_Pubblicazioni.Where(x => x.data_pubblicazione == null)
                    .OrderBy(x => x.data_richiesta)
                    .ToList();

            return View(model);

        }

        public ActionResult resetsede(int anno, string sede)
        {
            var db = new digiGappEntities();
            var pfs = db.MyRai_PianoFerieSedi.Where(x => x.anno == anno && x.sedegapp == sede).FirstOrDefault();
            if (pfs == null)
            {
                return Content("Sede non trovata");
            }
            pfs.sedegapp = "_" + pfs.sedegapp;
            try
            {
                db.SaveChanges();
                Logger.LogAzione(new MyRai_LogAzioni()
                {
                    applicativo = "portale",
                    data = DateTime.Now,
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    provenienza = "tech/resetsede",
                    descrizione_operazione = "anno:" + anno + ", sede:" + sede,
                    operazione = "Reset sede PF"
                });
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
            return Content("OK eseguito ");
        }
        public ActionResult Reset()
        {
            if (!Utente.IsAdmin())
            {
                throw (new Exception("Unauthorized"));
            }

            for (int i=0;i<=5;i++)
                CommonManager.Get_CategoriaDato_Net_Cached(i, false, null, true);

            try
            {
                Logger.LogAzione(new myRaiData.MyRai_LogAzioni()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    descrizione_operazione = "Cache reset",
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    operazione = "Cache reset",
                    provenienza = "Tech/Reset"
                });
                return Content("Cache resettata");
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        public ActionResult Write(WriteTestModel model)
        {
            return View(model);
        }

        [HttpPost]
        public ActionResult WritePost(WriteTestModel model)
        {
            try
            {
                string a = System.Web.HttpContext.Current.Server.MapPath("~/" + model.VirtualDir + "/" + model.File);
                System.IO.File.WriteAllText(a, model.Text);
                return Content("OK su " + a);
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        [HttpPost]
        public ActionResult uppdf(string sede, string pdfda, string pdfa, HttpPostedFileBase pdfile)
        {
            if (!Utente.IsAdmin())
            {
                throw (new Exception("Unauthorized"));
            }

            if (sede == null || sede.Length != 5) return Content("Nome sede non valido");
            sede = sede.ToUpper();
            DateTime d1;
            bool b1 = DateTime.TryParseExact(pdfda, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out d1);

            DateTime d2;
            bool b2 = DateTime.TryParseExact(pdfa, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out d2);

            if (!b1 || !b2) return Content("Data non valida");

            if (d2 < d1) return Content("Periodo non valido");

            if (pdfile == null || pdfile.ContentLength <= 0 || string.IsNullOrWhiteSpace(pdfile.FileName) || !pdfile.FileName.ToLower().EndsWith("pdf"))
                return Content("File non valido");


            var db = new myRaiData.digiGappEntities();
            var pdf = db.DIGIRESP_Archivio_PDF.Where(x => x.sede_gapp == sede && x.data_inizio == d1 && x.data_fine == d2 && x.tipologia_pdf == "R").FirstOrDefault();
            if (pdf != null) return Content("PDF già esistente");

            if (db.DIGIRESP_Archivio_PDF.Any(x => x.sede_gapp == sede && x.tipologia_pdf == "R" &&
                ((d1 >= x.data_inizio && d1 <= x.data_fine) || (d2 >= x.data_inizio && d2 <= x.data_fine))
            ))
            {
                return Content("Periodo non valido");
            }

            MemoryStream target = new MemoryStream();
            pdfile.InputStream.CopyTo(target);


            try
            {
                DIGIRESP_Archivio_PDF p = new DIGIRESP_Archivio_PDF()
                {
                    attivo = true,
                    data_fine = d2,
                    data_inizio = d1,
                    data_stampa = DateTime.Now,
                    matricola_stampa = "0ESSWEB",
                    numero_versione = 1,
                    pdf = target.ToArray(),
                    tipologia_pdf = "R",
                    sede_gapp = sede,
                    stato_pdf = "S_OK"
                };
                db.DIGIRESP_Archivio_PDF.Add(p);
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }

            return Content("OK");

        }
        public ActionResult IsSW(string d)
        {
            DateTime D;
            DateTime.TryParseExact(d.Replace("/", "").Replace("-", ""), "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out D);
            bool s= Utente.IsSmartWorker(D);
            return Content(s.ToString());
        }


        [HttpPost]
        public ActionResult crpdf(string sede, string pdfda, string pdfa)
        {
            if (!Utente.IsAdmin())
            {
                throw (new Exception("Unauthorized"));
            }

            if (sede == null || sede.Length != 5) return Content("Nome sede non valido");
            DateTime d1;
            bool b1 = DateTime.TryParseExact(pdfda, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out d1);

            DateTime d2;
            bool b2 = DateTime.TryParseExact(pdfa, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out d2);

            if (!b1 || !b2) return Content("Data non valida");

            if (d1.AddDays(6) < d2) return Content("Periodo max 7 giorni");

            var db = new myRaiData.digiGappEntities();
            var pdf = db.DIGIRESP_Archivio_PDF.Where(x => x.sede_gapp == sede.ToUpper() && x.data_inizio == d1
                      && x.data_fine == d2 && x.tipologia_pdf == "R").FirstOrDefault();
            if (pdf != null) return Content("PDF già esistente");

            Abilitazioni AB = CommonManager.getAbilitazioni();
            if (!AB.ListaAbilitazioni.Any(x => x.Sede == sede.ToUpper())) return Content("Sede non abilitata");

            string esito = myRaiCommonTasks.CommonTasks.Task_GeneraPdfEccezioni(sede.ToUpper(), d1, d2, true);
            var pdfCreato = db.DIGIRESP_Archivio_PDF.Where(x => x.sede_gapp == sede.ToUpper() && x.data_inizio == d1
                      && x.data_fine == d2 && x.tipologia_pdf == "R").FirstOrDefault();
            if (pdfCreato != null) return Content("OK");
            else return Content(esito);
        }

        public ActionResult getrich()
        {
            DateTime dt1 = DateTime.Now;
            dt1 = dt1.AddMonths( -2 );

            var db = new myRaiData.digiGappEntities();

            var richieste = db.MyRai_Richieste.Where( b => b.data_richiesta > dt1 ).Select( x => new DaMatr( ) { d = x.data_richiesta , m = x.matricola_richiesta } ).ToList( );

            //var richieste = db.MyRai_Richieste.Where(b => b.data_richiesta > new DateTime(2018, 5, 1)).Select(x => new DaMatr() { d = x.data_richiesta, m = x.matricola_richiesta }).ToList();
            var model = GetRichiestePerData(richieste);
            return View("_chartrichieste", model);
        }

        public ActionResult getmatr()
        {
            DateTime dt1 = DateTime.Now;
            dt1 = dt1.AddMonths( -2 );

            var db = new myRaiData.digiGappEntities();
            var richieste = db.MyRai_Richieste.Where( b => b.data_richiesta > dt1 ).Select( x => new DaMatr( ) { d = x.data_richiesta , m = x.matricola_richiesta } ).ToList( );
            //var richieste = db.MyRai_Richieste.Where(b => b.data_richiesta > new DateTime(2018, 5, 1)).Select(x => new DaMatr() { d = x.data_richiesta, m = x.matricola_richiesta }).ToList();
            var model = GetMatricoleAllaData(richieste);
            return View("_chartmatricole", model);
        }

        public ActionResult getdoc()
        {
            var model = GetSediPdfModel();
            return View("_pdfstatus", model);
        }

        public ActionResult getappr(string ord = null)
        {
            DateTime d1 = new DateTime(DateTime.Now.AddMonths(-1).Year, DateTime.Now.AddMonths(-1).Month, 1);
            DateTime d2 = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var model = new ApprovazioniPendingModel();
            model.MesePrec = GetSediDaAppr(d1, d2);
            model.MeseCorr = GetSediDaAppr(d2, d2.AddMonths(1));
            if (ord == "p")
            {
                model.MesePrec = model.MesePrec.OrderByDescending(x => x.richiesteDaApprovare).ToList();
                model.MeseCorr = model.MeseCorr.OrderByDescending(x => x.richiesteDaApprovare).ToList();
            }
            return View("_daapprstatus", model);
        }

        #region calcolo Barbara
        public ActionResult getresptime()
        {
            var db = new myRaiData.digiGappEntities();
            DateTime d1 = DateTime.Today;
            DateTime d2 = d1.AddDays(1);

            double average = 0;
            var tmp = db.LOG.Where(xx => (xx.Timestamp >= d1) && (xx.Timestamp <= d2) && (xx.Livello > 1)).Select(x => x.Livello);
            if (tmp != null && tmp.Any())
                average = tmp.Average();
      
            var model = new IntervalloOsservazione();
            model.avg1 = Math.Round(average);
            // model.AverageResponseTime = average;
            return View("_averageResponseTime", model  );
        }

        public ActionResult getresptimeHH(int id=0)
        {
            var model = new IntervalloOsservazione();
            if (id == 0)
            {
                model.IntervalloMin = CommonManager.GetParametri<int>(EnumParametriSistema.IntervalloMinTech2)[0];
            }
            else
            {
                model.IntervalloMin = id;
            }
            //model.IntervalloRefresh = CommonManager.GetParametri<int>(EnumParametriSistema.IntervalloMinTech2)[1];
            var db = new myRaiData.digiGappEntities();
            DateTime d1 = DateTime.Now;
            TimeSpan d2 = new TimeSpan(0, 0, model.IntervalloMin, 00, 00);

            DateTime d3 = d1.Subtract(d2);
            double average = 0;
           
            var tmp = db.LOG.Where(xx => (xx.Timestamp >= d3) && (xx.Timestamp <= d1) && (xx.Livello > 1)).Select(x => x.Livello);
            if (tmp!=null && tmp.Any())
                average = tmp.Average();
        
            model.da = d3;
            model.a = d1;
            model.avg = Math.Round(average);

            List<call_Avg> list3 = db.LOG.Where(xx => (xx.Timestamp >= d3) && (xx.Timestamp <= d1) && (xx.Livello > 1)).GroupBy(x => x.Autore).OrderBy(g => g.Key).Select(x =>
            new call_Avg { NomeChiamata = x.Key, AvgCall = x.Count()}).ToList();
             model.Avg_Call = list3;
            return View("_averageResponseTimeHH", model );
        }

        public int GetRefreshInterval()
        {
            return CommonManager.GetParametri<int>(EnumParametriSistema.IntervalloMinTech2)[1];
        }

        public double getRTimeChart()
        {
            var db = new myRaiData.digiGappEntities();
            DateTime d1 = DateTime.Today;
            DateTime d2 = d1.AddDays(1);

            double average = db.LOG.Where(xx => (xx.Timestamp >= d1) && (xx.Timestamp <= d2) && (xx.Livello > 1)).Select(x => x.Livello).Average();



            // model.AverageResponseTime = average;
            return Math.Round(average);
        }
        #endregion

        private List<data_Matr> GetMatricoleAllaData(List<DaMatr> richieste)
        {
            var matricoleAllaData = richieste.Select(x =>
                x.d.Date)
                .Distinct()
                .Select(a => new data_Matr
                {
                    matricoleTOT = richieste.Where(z =>
                                   z.d <= a)
                                   .Select(m => m.m)
                                   .Distinct()
                                   .Count(),
                    da = (DateTime)a
                })
                .OrderBy(o => o.da)
                .ToList();
            matricoleAllaData.Where(
                   (x, i) => i % 15 == 0

                 )
                .ToList()
                    .ForEach(w => w.data = "'" + w.da.ToString("dd-MM-yy") + "'");
            return matricoleAllaData;
        }

        private List<data_Rich> GetRichiestePerData(List<DaMatr> richieste)
        {
            var richiesteData = richieste.Select(x =>
                 x.d.Date)
                 .Distinct()
                 .Select(a => new data_Rich
                 {
                     richiesteTOT = richieste.Where(z =>
                                    z.d.Date == a)
                                    .Count(),
                     da = (DateTime)a
                 })
                 .OrderBy(o => o.da)
                 .ToList();
            richiesteData.Where(
                (x, i) => i % 15 == 0

                )
                .ToList()
                    .ForEach(w => w.data = "'" + w.da.ToString("dd-MM-yy") + "'");

            return richiesteData;
        }
     
        private List<sediPdfModel> GetSediPdfModel()
        {
            List<sediPdfModel> ListaSedi = new List<sediPdfModel>();

            Abilitazioni abi = CommonManager.getAbilitazioni();
            var db = new myRaiData.digiGappEntities();
            foreach (AbilitazioneSede abs in abi.ListaAbilitazioni)
            {
                sediPdfModel model = new sediPdfModel();

                model.sede = abs.Sede;
                model.files = db.DIGIRESP_Archivio_PDF.Where(x => x.tipologia_pdf == "R" && x.sede_gapp == abs.Sede)
                      .Select(a => new pdfFile
                      {
                          al = a.data_fine,
                          dal = a.data_inizio,
                          data_conv = a.data_convalida,
                          data_stampa = a.data_stampa,
                          id = a.ID,
                          matricola_conv = a.matricola_convalida,
                          matricola_stampa = a.matricola_stampa,
                          status = a.stato_pdf,
                          versione = a.numero_versione
                      })
                      .OrderByDescending(z => z.dal)
                      .Take(4)
                      .ToList();
                ListaSedi.Add(model);

            }
            return ListaSedi.OrderBy(x => x.sede).ToList();
        }

        private string[] getNominativiL1(string sede)
        {
            Abilitazioni ab = CommonManager.getAbilitazioni();
            string[] s = ab.ListaAbilitazioni.Where(x => x.Sede == sede)
                .SelectMany(x => x.MatrLivello1)
                .Select(z => z.Matricola).ToArray();

            return s;
        }

        private List<sediDaApprovareModel> GetSediDaAppr(DateTime d1, DateTime d2)
        {
            var db = new myRaiData.digiGappEntities();
            List<sediDaApprovareModel> list =
                db.MyRai_Richieste
                .Where(x => x.id_stato == 10 && x.periodo_dal >= d1 && x.periodo_dal < d2)
                .GroupBy(x => x.codice_sede_gapp)
                .OrderBy(g => g.Key)
                .Select(x => new sediDaApprovareModel
                {
                    sede = x.Key,
                    richiesteDaApprovare = x.Count()
                }).ToList();

            foreach (var k in list) k.NominativiL1 = getNominativiL1(k.sede);

            return list;
        }

        public ActionResult GetPdf(int idPdf)
        {
            var db = new myRaiData.digiGappEntities();
            var pdf = db.DIGIRESP_Archivio_PDF.Where(x => x.ID == idPdf).FirstOrDefault();

            if (pdf == null) return null;

            byte[] byteArray = pdf.pdf;
            MemoryStream pdfStream = new MemoryStream();
            pdfStream.Write(byteArray, 0, byteArray.Length);
            pdfStream.Position = 0;

            Response.AppendHeader("content-disposition", "inline; filename=" + pdf.sede_gapp + "_" + pdf.data_inizio.ToString("ddMMyyyy") + "_" + pdf.data_fine.ToString("ddMMyyyy") + "pdf");
            return new FileStreamResult(pdfStream, "application/pdf");
        }

        public List<SessionResult> GetSession_StartPerDay()
        {
            var db = new myRaiData.digiGappEntities();

            List<SessionResult> resultPerGiorno = db.MyRai_LogAzioni.Where(xx => xx.operazione == "SESSION_START")
           .GroupBy(item => new { y = item.data.Year, m = item.data.Month, d = item.data.Day })
           .Select(
             grp => new SessionResult
             {
                 d = grp.Key.d,
                 m = grp.Key.m,
                 y = grp.Key.y,
                 tot = grp.Count()
             }).OrderBy(z => z.y).ThenBy(z => z.m).ThenBy(z => z.d).ToList();

            return resultPerGiorno;
        }

        public List<SessionResult> GetSession_StartPerHour()
        {
            var db = new myRaiData.digiGappEntities();

            List<SessionResult> resultPerGiorno = db.MyRai_LogAzioni.Where(xx => xx.operazione == "SESSION_START")
           .GroupBy(item => new { y = item.data.Year, m = item.data.Month, d = item.data.Day, h = item.data.Hour })
           .Select(
             grp => new SessionResult
             {
                 h = grp.Key.h,
                 d = grp.Key.d,
                 m = grp.Key.m,
                 y = grp.Key.y,
                 tot = grp.Count()
             }).OrderBy(z => z.y).ThenBy(z => z.m).ThenBy(z => z.d).ToList();

            return resultPerGiorno;
        }

        public object Esegui ( string classe , string metodo , string parametri = null )
        {
            if (!Utente.IsAdmin())
            {
                throw ( new Exception( "Unauthorized" ) );
            }

            Type type = Type.GetType( classe );

            Object obj = Activator.CreateInstance( type );

            MethodInfo methodInfo = type.GetMethod( metodo );

            List<object> methodParams = new List<object>( );

            if ( !String.IsNullOrEmpty( parametri ) )
            {
                List<string> myParams = parametri.Split( '|' ).ToList( );

                if ( myParams != null && myParams.Any( ) )
                {
                    foreach ( var p in myParams )
                    {
                        string[] coppia = p.Split( '^' );

                        if ( coppia != null )
                        {
                            string tipo = coppia[0];

                            switch ( tipo )
                            {
                                case "i":
                                    methodParams.Add( Int16.Parse( coppia[1] ) );
                                    break;
                                case "I":
                                    methodParams.Add( Int32.Parse( coppia[1] ) );
                                    break;
                                case "II":
                                    methodParams.Add( Int64.Parse( coppia[1] ) );
                                    break;
                                case "s":
                                    methodParams.Add( ( string ) coppia[1] );
                                    break;
                                case "f":
                                    methodParams.Add( float.Parse( coppia[1] ) );
                                    break;
                                case "d":
                                    methodParams.Add( decimal.Parse( coppia[1] ) );
                                    break;
                                case "dt":
                                    methodParams.Add( DateTime.Parse( coppia[1] ) );
                                    break;
                                case "dd":
                                    methodParams.Add( DateTime.Parse( coppia[1] ).Date );
                                    break;
                                case "l":
                                    methodParams.Add( long.Parse( coppia[1] ) );
                                    break;
                                case "b":
                                    methodParams.Add( bool.Parse( coppia[1] ) );
                                    break;
                                default:
                                    break;
                            }
                        }

                    }
                }
            }

            if ( methodParams != null && methodParams.Any( ) )
            {
                return new XmlResult( methodInfo.Invoke( obj , methodParams.ToArray( ) ) );
            }
            else
            {
                return new XmlResult( methodInfo.Invoke( obj , null ) );
            }
        }

        public ActionResult Test(string e)
        {
            if (!Utente.IsAdmin())
            {
                throw (new Exception("Unauthorized"));
            }
            string url = "http://" + HttpContext.Request.Url.Authority;

            string resp = TimbratureCore.TimbratureManager.TestEcc(url, e);
            return Content(resp);
        }

        public ActionResult GetAG(string m, string d, string e, string smap1, string skipcar, string skipcoda, string esim)
        {

            if (!Utente.IsAdmin())
            {
                throw (new Exception("Unauthorized"));
            }

            d = d.Replace("/", "");

            WSDigigapp serviceWS = new WSDigigapp()
            {
                Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1])
            };

            var resp = serviceWS.getEccezioni(m, d, "BU", 70);
            dayResponse respYesterday = null;
            if (e.ToUpper() == "AT30")
            {
                DateTime D;
                DateTime.TryParseExact(d, "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out D);

                respYesterday = serviceWS.getEccezioni(m, D.AddDays(-1).ToString("ddMMyyyy"), "BU", 80);
            }
            var dati = BatchManager.GetUserData(m);
            string tipiQG = CommonManager.GetParametro<string>(EnumParametriSistema.TipiDipQuadraturaGiornaliera);

            if (!String.IsNullOrWhiteSpace(esim))
            {
                esim = esim.ToUpper().Trim();
                var L = resp.eccezioni.ToList();
                if (!L.Any(x => x.cod == esim))
                {
                    L.Add(new Eccezione() { cod = esim });
                }
                resp.eccezioni = L.ToArray();
            }
            DayAnalysisBase da = DayAnalysisFactory.GetDayAnalysisClass(e.ToUpper(), resp, tipiQG.Contains(dati.tipo_dipendente), respYesterday,
                smap1 == "1" ? true : false,
                skipcar == "1" ? true : false,
                skipcoda == "1" ? true : false


                );


            if (da != null)
            {
                EccezioneQuantita response = da.GetEccezioneQuantita();

                 return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = response }
                };
            }
            else return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { result = "UNKNOWN" }
            };
        }

        public ActionResult rkmf()
        {
            WSDigigapp service = new WSDigigapp()
            {
                Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1])
            };
            var db = new myRaiData.digiGappEntities();
            var list = db.MyRai_Eccezioni_Richieste.Where(x => (x.id_stato == 10 || x.id_stato == 20) && x.cod_eccezione == "RKMF").OrderBy(x=>x.data_eccezione).ThenBy(x=>x.MyRai_Richieste.matricola_richiesta).ToList();
             
            StringBuilder sb = new StringBuilder();
            foreach (var item in list)
            {
                sb.Append(item.data_eccezione.ToString("ddMMyyyy")+";"+item.MyRai_Richieste.matricola_richiesta+";");
                var response = service.getEccezioni(item.MyRai_Richieste.matricola_richiesta, item.data_eccezione.ToString("ddMMyyyy"), "BU", 80);
                var EccRkmf = response.eccezioni.Where(x => x.cod.Trim() == "RKMF").FirstOrDefault();
                if (EccRkmf == null)
                    sb.Append("NO");
                else
                {
                    sb.Append(EccRkmf.importo);
                }

                sb.Append("<br />");
            }
            return Content(sb.ToString());
        }
    }

    class DaMatr
    {
        public DateTime d { get; set; }
        public string m { get; set; }
    }

    class SessioniOra
    {
        public string Ora { get; set; }
        public int Sessioni { get; set; }
    }

    public class SessionResult
    {
        public int y { get; set; }
        public int m { get; set; }
        public int d { get; set; }
        public int h { get; set; }
        public int tot { get; set; }
    }
}