using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using myRaiData;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using System.Data;
using System.Net;
using System.Web.Routing;
using System.Xml.Serialization;
using Logger = myRaiHelper.Logger;
using myRaiHelper;
using myRaiServiceHub.it.rai.servizi.hrpaga;
using Newtonsoft.Json;
using myRai.DataAccess;

namespace myRaiGestionale.Controllers
{
    public class XmlResult : ActionResult
    {
        private object objectToSerialize;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlResult"/> class.
        /// </summary>
        /// <param name="objectToSerialize">The object to serialize to XML.</param>
        public XmlResult(object objectToSerialize)
        {
            this.objectToSerialize = objectToSerialize;
        }

        /// <summary>
        /// Gets the object to be serialized to XML.
        /// </summary>
        public object ObjectToSerialize
        {
            get { return this.objectToSerialize; }
        }

        /// <summary>
        /// Serialises the object that was passed into the constructor to XML and writes the corresponding XML to the result stream.
        /// </summary>
        /// <param name="context">The controller context for the current request.</param>
        public override void ExecuteResult(ControllerContext context)
        {
            if (this.objectToSerialize != null)
            {
                context.HttpContext.Response.Clear();
                XmlRootAttribute root = new XmlRootAttribute("response");

                var xs = new System.Xml.Serialization.XmlSerializer(this.objectToSerialize.GetType(), root);
                context.HttpContext.Response.ContentType = "text/xml";

                xs.Serialize(context.HttpContext.Response.Output, this.objectToSerialize);
            }
        }
    }

    public class HomeController : BaseCommonController
    {
        myRaiCommonModel.ModelDash pr = new myRaiCommonModel.ModelDash();
        
        public ActionResult getimg(string matr)
        {
            return null;

            string s = DateTime.Now.ToString("ddMMyyyy") + matr;
            int tot = 0;
            foreach (var c in s)
            {
                if (int.TryParse(c.ToString(), out int num))
                    tot += num;
            }

            return RedirectToAction("getimage", "api/foto", new
            {
                risoluzione = 3,
                matricola = matr,
                check = tot.ToString().PadLeft(3, '0')
            });


            string FotoDip = CommonHelper.GetParametro<string>(EnumParametriSistema.FotoDipendenti);
            if (FotoDip == "0")
                return null;

            ImageInfo im = CommonHelper.GetImageWithOverride(matr);
            if (im != null) return File(im.image, "image/" + im.ext);

            try
            {
                if (System.Diagnostics.Debugger.IsAttached && CommonHelper.CheckForVPNInterface())
                    return null;
                else
                {
                    string url = CommonHelper.GetUrlFotoExternal(matr);

                    WebClient w = new WebClient();
                    w.Credentials = CommonHelper.GetUtenteServizioCredentials();

                    DateTime D1 = DateTime.Now;

                    byte[] b = w.DownloadData(url);

                    if (FotoDip == "1")
                    {
                        DateTime D2 = DateTime.Now;
                        double diff = (D2 - D1).TotalMilliseconds;
                        Logger.LogAzione(new MyRai_LogAzioni()
                        {
                            operazione = "TimeImmagine",
                            descrizione_operazione = diff.ToString() + " ms"
                        });
                    }


                    return File(b, "image/png");
                }
            }
            catch
            {
                return null;
            }
        }

        public ActionResult getab(string s, string m)
        {
            if (!UtenteHelper.IsAdmin(CommonHelper.GetCurrentUserMatricola())) return null;
            var AB = CommonHelper.getAbilitazioni();
            AB.ListaAbilitazioni = AB.ListaAbilitazioni.OrderBy(x => x.Sede).ToList();
            if (!string.IsNullOrWhiteSpace(s))
                AB.ListaAbilitazioni = AB.ListaAbilitazioni.Where(x => x.Sede.ToUpper() == s.ToUpper()).ToList();

            if (!string.IsNullOrWhiteSpace(m))
                AB.ListaAbilitazioni = AB.ListaAbilitazioni.Where(x => x.MatrLivello1.Select(a => a.Matricola).Contains("P" + m) ||
                    x.MatrLivello2.Select(a => a.Matricola).Contains("P" + m)).ToList();

            return new XmlResult(AB);
        }
        public ActionResult getVar(string name)
        {
            if (!UtenteHelper.IsAdmin()) return null;

            string result = "null";
            if (System.Web.HttpContext.Current.Session[name] != null)
                result = Newtonsoft.Json.JsonConvert.SerializeObject(System.Web.HttpContext.Current.Session[name], Formatting.Indented, new JsonSerializerSettings()
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    MaxDepth = 1
                });

            return Content(result);
        }

        public ActionResult getAuthHelper()
        {
            if (!UtenteHelper.IsAdmin(CommonHelper.GetCurrentRealUsername())) return null;
            var tmp = AuthHelper.GetAbilitazioni();
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(tmp, new JsonSerializerSettings()
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
            });

            return Content(json, "application/json");
        }

        public ActionResult BadBrowser()
        {
            string msg = CommonHelper.GetParametro<string>(EnumParametriSistema.MessaggioBadBrowser);
            ViewBag.msg = msg;
            return View();
        }

        public ActionResult Loader(string dest, string par)
        {
            ViewBag.dest = dest;
            ViewBag.par = par;

            return View("~/Views/Shared/Loader.cshtml");
        }

        public ActionResult write(string vd)
        {
            try
            {
                string a = System.Web.HttpContext.Current.Server.MapPath("~/" + vd);
                System.IO.File.WriteAllText(a + "\\test.txt", "test");
                return Content("scrittura OK su " + a);
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        public ActionResult returnSessioneScaduta()
        {
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { result = "SESSIONE SCADUTA" }
            };
        }

        public ActionResult chisono()
        {
            try
            {
                string resp = "Utente Nativo:" + System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                resp += "<br />Utente Rimappato:" + CommonHelper.GetCurrentUsername();
                resp += "<br />Matricola Rimappata:" + CommonHelper.GetCurrentUserMatricola();
                resp += "<br />Matricola7 Rimappata:" + CommonHelper.GetCurrentUserMatricola7chars();
                resp += "<br />PMatricola Rimappata:" + CommonHelper.GetCurrentUserPMatricola();
                resp += "<br />IP:" + Request.UserHostAddress;
                resp += "<br />Server:" + Logger.GetServerName();
                if (Session !=null)
                    resp += "<br /> Sessione ID:" + Session.SessionID;

                MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client cl = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
                var response = cl.GetRecuperaUtente(CommonHelper.GetCurrentUserMatricola7chars( ), DateTime.Now.ToString("ddMMyyyy"));
                resp += "<br />DataNascita:" + response.data.data_nascita;
                resp += "<br />          -------------- TEST PAGA ------------------";
                try
                {
                    HrPaga hrp = new HrPaga();
                    hrp.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                    hrp.Url = "http://hrpaga.servizi.rai.it/hrpaga.asmx";
                    ListaDocumenti lista = hrp.ElencoDocumentiPersonali("00", String.Empty, String.Empty);

                    resp += "<br />" + "Errore documenti: " + lista.StringaErrore;
                    resp += "<br />" + "Esito documenti: " + lista.Esito;
                    if (lista.ListaDatiDocumenti != null)
                        resp += "<br />" + "Numero documenti: " + lista.ListaDatiDocumenti.Length.ToString();
                }
                catch (Exception ex)
                {

                    resp += "<br />" + ex.Message;
                }
                resp += "<br />          --------------------------------";

                resp += "<br />          -------------- TEST PAGA 2------------------";
                try
                {
                    HrPaga hrp = new HrPaga();
                    hrp.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                    hrp.Url = "http://hrpaga.intranet.rai.it/hrpaga.asmx";
                    ListaDocumenti lista = hrp.ElencoDocumentiPersonali("00", String.Empty, String.Empty);

                    resp += "<br />" + "Errore documenti: " + lista.StringaErrore;
                    resp += "<br />" + "Esito documenti: " + lista.Esito;
                    if (lista.ListaDatiDocumenti != null)
                        resp += "<br />" + "Numero documenti: " + lista.ListaDatiDocumenti.Length.ToString();
                }
                catch (Exception ex)
                {

                    resp += "<br />" + ex.Message;
                }
                resp += "<br />          --------------------------------";

     
                string services = CommonHelper.GetParametro<string>(EnumParametriSistema.ServiziChiSono);

                foreach (string url in services.Split(','))
                {
                    resp += "<br />" + url + " - Default Credentials : ";
                    WebClient wc = new WebClient();

                    wc.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                    try
                    {
                        byte[] buffer = wc.DownloadData(url);
                        resp += "OK";
                    }
                    catch (Exception ex)
                    {

                        resp += ex.Message;
                    }

                    resp += "<br />" + url + " - UtenteServizio : ";
                    wc.Credentials = CommonHelper.GetUtenteServizioCredentials();
                    try
                    {
                        byte[] buffer1 = wc.DownloadData(url);
                        resp += "OK";
                    }
                    catch (Exception ex)
                    {

                        resp += ex.Message;
                    }

                }
                return Content("<h2>" + resp + "</h2>");
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        public ActionResult nosess()
        {
            Session.RemoveAll();
            return null;
        }

        public void CheckNoBoss()
        {
            if ( ! UtenteHelper.IsAbilitatoGapp()) return;

            string rep = UtenteHelper.Reparto();
            if (rep == null || rep.Trim() == "" || rep.Trim() == "00") rep = "";
            string sede = UtenteHelper.SedeGapp(DateTime.Now) + rep;

            if (string.IsNullOrWhiteSpace(sede)) return;

            var AB = CommonHelper.getAbilitazioni();
            var abil = AB.ListaAbilitazioni.Where(x => x.Sede == sede).FirstOrDefault();
            if (abil == null || abil.MatrLivello1==null || ! abil.MatrLivello1.Any())
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = "Nessun L1 per " + sede,
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    provenienza = "home/clear"
                });
                if (sede.Length > 5)
                {
                    sede = sede.Substring(0, 5);
                    abil = AB.ListaAbilitazioni.Where(x => x.Sede == sede).FirstOrDefault();
                    if (abil == null || abil.MatrLivello1 == null || !abil.MatrLivello1.Any())
                    {
                        Logger.LogErrori(new MyRai_LogErrori()
                        {
                            applicativo = "PORTALE",
                            data = DateTime.Now,
                            error_message = "Nessun L1 per " + sede,
                            matricola = CommonHelper.GetCurrentUserMatricola(),
                            provenienza = "home/clear"
                        });
                    }
                }
            }
        }

        public ActionResult clear(string dest, string par)
        {
            Session.RemoveAll();
            SessionHelper.Set("startup","OK");
            SessionHelper.Set("_tms_startup", DateTime.Now);

            SessionHelper.SessionStart();

            if ( String.IsNullOrWhiteSpace( dest ) )
                return RedirectToAction( "/" , "" );
            else
            {
                dest = dest.Trim( new[] { ' ' , '/' } );
                return RedirectToAction(
                                dest.Split( '/' )[1] ,
                                dest.Split( '/' )[0] ,
                                ( String.IsNullOrWhiteSpace( par ) ? null : GetRouteParams( par ) )
                                );
            }
        }

        private RouteValueDictionary GetRouteParams(string queryString)
        {
            RouteValueDictionary r = new RouteValueDictionary();
            string[] q = queryString.Split('&');
            foreach (string s in q)
            {
                string[] seg = s.Split('=');
                if (seg.Length > 1) r.Add(seg[0], seg[1]);
            }
            return r;
        }

        public ActionResult simula(string m)
        {
            string RealName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToUpper();
            RealName = RealName.Replace("RAI\\", "").TrimStart(new char[] { 'P' });
            bool myself = (RealName == m);

            string abilitati = CommonHelper.GetParametro<string>(EnumParametriSistema.AbilitatiSimulazione);
            if (abilitati != null)
            {
                string[] ab = abilitati.ToUpper().Split(',').Select(x => x.Trim()).ToArray();
                if (ab.Any(x => RealName == x))
                {
                    var db = new digiGappEntities();
                    var par = db.MyRai_ParametriSistema.Where(x => x.Chiave == "MatricolaSimulata" && x.Valore2 == RealName).FirstOrDefault();
                    if (par != null)
                    {
                        if (myself)
                            db.MyRai_ParametriSistema.Remove(par);
                        else
                            par.Valore1 = "RAI\\P" + m;
                    }
                    else
                    {
                        if (!myself)
                        {
                            MyRai_ParametriSistema p = new MyRai_ParametriSistema()
                            {
                                Chiave = "MatricolaSimulata",
                                Valore1 = "RAI\\P" + m,
                                Valore2 = RealName
                            };
                            db.MyRai_ParametriSistema.Add(p);
                        }
                    }
                    db.SaveChanges();

                    return RedirectToAction("clear", "home");
                }
            }
            throw new Exception("UNAUTHORIZED");
        }
       
        public ActionResult getjs()
        {
            string eccSoloQuan = CommonHelper.GetParametro<string>(EnumParametriSistema.AccettaSoloDurata);
            string eccDigitaSoloQuant = CommonHelper.GetParametro<string>(EnumParametriSistema.EccezioniDigitaSoloQuantita);
            List<string> ListaEcc = new List<string>();
            if (!String.IsNullOrWhiteSpace(eccSoloQuan))
                ListaEcc.AddRange(eccSoloQuan.Split(',').ToList());
            if (!String.IsNullOrWhiteSpace(eccDigitaSoloQuant))
                ListaEcc.AddRange(eccDigitaSoloQuant.Split(',').ToList());

            var db = new digiGappEntities();

            string JS = CommonHelper.GetParametro<string>(EnumParametriSistema.ValidazioneGenericaEccezioni)
                .Replace("#ECC#", String.Join(",", ListaEcc.ToArray()))
                .Replace("#EIT#", String.Join(",",
                     db.MyRai_Eccezioni_Ammesse
                         .Where(x => x.CopertaDaTimbrature)
                         .Select(x => x.cod_eccezione)
                         .ToArray()))
                .Replace("#EOT#", String.Join(",",
                    db.MyRai_Eccezioni_Ammesse
                        .Where(x => x.FuoriDaTimbratureEntroOrario)
                        .Select(x => x.cod_eccezione)
                        .ToArray()))
                .Replace("#EOO#", String.Join(",",
                    db.MyRai_Eccezioni_Ammesse
                        .Where(x => x.FuoriDaTimbratureFuoriOrario)
                        .Select(x => x.cod_eccezione)
                        .ToArray()))
                .Replace("#FOIC#", String.Join(",",
                    db.MyRai_Eccezioni_Ammesse
                        .Where(x => x.FuoriOrarioInCoda)
                        .Select(x => x.cod_eccezione)
                        .ToArray()))
                    .Replace("#FOIT#", String.Join(",",
                    db.MyRai_Eccezioni_Ammesse
                        .Where(x => x.FuoriOrarioInTesta)
                        .Select(x => x.cod_eccezione)
                        .ToArray()))
                        ;

            return Content(JS);
        }

        public ActionResult GetUser()
        {
            return Content(CommonHelper.GetCurrentUsername());
        }

        public ActionResult NotAuth()
        {
            return View("NonAutorizzato");
        }

        public ActionResult version(string set)
        {
            if (!String.IsNullOrWhiteSpace(set) && set.ToLower() == "all")
            {
                string[] AllFiles = System.IO.Directory.GetFiles(Server.MapPath("~"), "*.*",System.IO.SearchOption.AllDirectories);
                var T = new List<Tuple<string, long, string>>();

                foreach (string f in AllFiles)
                {
                    System.IO.FileInfo F = new System.IO.FileInfo(f);
                    T.Add(new Tuple<string, long, string>(f,F.Length,F.LastWriteTime.ToString("dd/MM/yyyy HH.mm")));
                }
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = T }
                };
            }
            string[] files = System.IO.Directory.GetFiles(Server.MapPath("~/bin/"), "*.dll");
            string res = "<h3 style='font-family:courier;font-size:11px'>";

            foreach (string file in files.OrderBy(x => x))
            {
                var fi = new System.IO.FileInfo(file);
                res += "\\bin\\" + System.IO.Path.GetFileName(file) + " : " + fi.LastWriteTime.ToString("dd/MM/yyyy HH:mm") + " (" + fi.Length + " bytes)<br />";
            }

            res += "<br/><br/>";

            string[] views = System.IO.Directory.GetFiles(Server.MapPath("~/views/"), "*.cshtml", System.IO.SearchOption.AllDirectories);
            foreach (string view in views.OrderBy(x => x))
            {
                var fi = new System.IO.FileInfo(view);
                string vv = view.Split(new string[] { "\\views\\" }, StringSplitOptions.None)[1];

                res += "\\views\\" + vv + " : " + fi.LastWriteTime.ToString("dd/MM/yyyy HH:mm") + " (" + fi.Length + " bytes)<br />";
            }

            return Content(res + "</h3>");
        }

        public ActionResult testws()
        {
            WSDigigapp service = new WSDigigapp();
            service.Url = "http://digigappws-his2016vip.intranet.rai.it/wsdigigapp.asmx";
            string r = "";

            service.UseDefaultCredentials = true;
            dayResponse resp1 = new dayResponse();
            dayResponse resp2 = new dayResponse();

            try
            {
                resp1 = service.getEccezioni("103650", "18082017", "BU", 75);
                r += "<br />Default Credentials<br /> - Esito:" + resp1.esito + " - Errore:" + resp1.errore + "<br />";
            }
            catch (Exception ex)
            {
                r += "<br />Default Credentials:<br />" + ex.ToString() + "<br />";
            }

            service.UseDefaultCredentials = false;
            service.Credentials = CommonHelper.GetUtenteServizioCredentials();

            try
            {
                resp2 = service.getEccezioni("103650", "18082017", "BU", 75);
                r += "<br />DB Credentials<br />- Esito:" + resp2.esito + " - Errore:" + resp2.errore + "<br />";
            }
            catch (Exception ex)
            {
                r += "<br />DB Credentials:<br />" + ex.ToString() + "<br />";
            }

            return Content(r);

        }

        public ActionResult AreaTest()
        {
            return View();
        }

        [HttpPost]
        public ActionResult GetNotification()
        {
            var db = new myRaiData.Incentivi.IncentiviEntities();

            string matr = CommonHelper.GetCurrentUserMatricola();

            var list = db.XR_HRIS_NOTIFICHE.AsQueryable();
            //Controllo per matricola
            list = list.Where(x => x.DEST_MATR != null && x.DEST_MATR.Contains(matr));

            //Controllo di non averla già vista
            list = list.Where(x =>!x.XR_HRIS_NOTIFICHE_LOG.Any(y => y.MATRICOLA == matr));

            var result = list.ToList();

            foreach (var item in list)
            {
                item.XR_HRIS_NOTIFICHE_LOG.Add(new myRaiData.Incentivi.XR_HRIS_NOTIFICHE_LOG()
                {
                    MATRICOLA = matr,
                    DTA_SENT = DateTime.Now
                });
            }

            DBHelper.Save(db, matr);

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = result.Select(x => new { x.MESSAGE, x.ACTION_URL, x.ACTION_JS })
            };
        }
    }
}