using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using myRai.DataAccess;
using myRaiData;
using System.Data;
using System.Net;
using System.Web.Routing;
using Newtonsoft.Json;
using System.Xml.Serialization;
using System.Globalization;
using myRaiHelper;
using myRaiCommonModel;
using MyRaiServiceInterface.it.rai.servizi.digigappws;
using myRaiCommonManager;
using myRai.Business;
using myRai.Models;

namespace myRai.Controllers
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
        ModelDash pr = new ModelDash();
        daApprovareModel daApprov;
        WSDigigapp datiBack = new WSDigigapp();
        WSDigigapp datiBack_ws1 = new WSDigigapp();
        MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client wcf1 = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();

        public ActionResult getimg(string matr)
        {
            ImageInfo im = CommonManager.GetImageWithOverride(matr);
            if (im != null) return File(im.image, "image/" + im.ext);

            string url = CommonManager.GetUrlFotoExternal(matr);

            WebClient w = new WebClient();
            w.Credentials = new System.Net.NetworkCredential(
                CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            byte[] b = w.DownloadData(url);
            return File(b, "image/png");

        }
        public ActionResult GetAbS()
        {
            if (!Utente.IsAdmin()) return null;
            var AB = BatchManager.GetAbilitazioniSeg();
            return new XmlResult(AB);
        }
        public ActionResult GetAbU()
        {
            if (!Utente.IsAdmin()) return null;
            var AB = BatchManager.GetAbilitazioniUffPers();
            return new XmlResult(AB);
        }

        public ActionResult getab(string s, string m)
        {
            if (!Utente.IsAdmin()) return null;
            var AB = CommonManager.getAbilitazioni();
            AB.ListaAbilitazioni = AB.ListaAbilitazioni.OrderBy(x => x.Sede).ToList();
            if (!string.IsNullOrWhiteSpace(s))
                AB.ListaAbilitazioni = AB.ListaAbilitazioni.Where(x => x.Sede.ToUpper() == s.ToUpper()).ToList();

            if (!string.IsNullOrWhiteSpace(m))
                AB.ListaAbilitazioni = AB.ListaAbilitazioni.Where(x => x.MatrLivello1.Select(a => a.Matricola).Contains("P" + m) ||
                    x.MatrLivello2.Select(a => a.Matricola).Contains("P" + m)).ToList();

            return new XmlResult(AB);
        }

        public ActionResult test(string s, int liv)
        {
            myRaiCommonTasks.CommonTasks.Log("test");
            return null;
        }

        public ActionResult BadBrowser()
        {
            string msg = CommonManager.GetParametro<string>(EnumParametriSistema.MessaggioBadBrowser);
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

        public ActionResult testCore(string m, string d)
        {
            if (!Utente.IsAdmin()) return Content("Non autorizzato");

            DateTime da;
            if (!DateTime.TryParseExact(d, "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out da))
                DateTime.TryParseExact(d, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out da);

            TimbratureCore.TimbratureInfo T = new TimbratureCore.TimbratureInfo(m, da);

            var respGiornata = T.GetGiornataInfo(da, m);
            string[] rangeMensa = CommonManager.GetParametri<string>(EnumParametriSistema.RangeMensa);
            var respCarenza = T.GetCarenzeInfo(da, m, rangeMensa[0], rangeMensa[1]);

            string r = (JsonConvert.SerializeObject(new object[] { respGiornata, respCarenza }, Formatting.Indented));
            return Content(r.Replace("\r\n", "<br/>").Replace(" ", "&nbsp;"));
        }

        public ActionResult test_timb(string d, string m)
        {
            if (!Utente.IsAdmin()) return Content("NON AUTORIZZATO");

            //WSDigigapp service = new WSDigigapp() { Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]) };
            ////stato giornata per data/matricola
            //var resp = service.getEccezioni(m, d, "BU", 70);

            WSDigigappDataController service = new WSDigigappDataController();

            dayResponse resp = service.GetEccezioni( CommonHelper.GetCurrentUserMatricola( ) , m , d , "BU" , 70 );

            var response = TimbratureCore.TimbratureManager.getCarenzaTimbrature(m, null, d);

            return new XmlResult(response);
        }

        public ActionResult chisono()
        {
            try
            {
                //MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client c = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
                //c.GetRecuperaUtente("103650", DateTime.Now.ToString("ddMMyyyy"));

                string resp = "Utente Nativo:" + System.Security.Principal.WindowsIdentity.GetCurrent().Name;
                resp += "<br />Utente Rimappato:" + CommonManager.GetCurrentUsername();
                resp += "<br />Matricola Rimappata:" + CommonManager.GetCurrentUserMatricola();
                resp += "<br />Matricola7 Rimappata:" + CommonManager.GetCurrentUserMatricola7chars();
                resp += "<br />PMatricola Rimappata:" + CommonManager.GetCurrentUserPMatricola();
                resp += "<br />IP:" + Request.UserHostAddress;
                resp += "<br />Server:" + Logger.GetServerName();
                if (Session !=null)
                    resp += "<br /> Sessione ID:" + Session.SessionID;


                MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client cl = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
                var response = cl.GetRecuperaUtente(CommonManager.GetCurrentUserMatricola7chars(), DateTime.Now.ToString("ddMMyyyy"));
                resp += "<br />DataNascita:" + response.data.data_nascita;
                resp += "<br />          -------------- TEST PAGA ------------------";
                try
                {
                    it.rai.servizi.hrpaga.HrPaga hrp = new it.rai.servizi.hrpaga.HrPaga();
                    hrp.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                    hrp.Url = "http://hrpaga.servizi.rai.it/hrpaga.asmx";
                    it.rai.servizi.hrpaga.ListaDocumenti lista = hrp.ElencoDocumentiPersonali("00", String.Empty, String.Empty);

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
                    it.rai.servizi.hrpaga.HrPaga hrp = new it.rai.servizi.hrpaga.HrPaga();
                    hrp.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
                    hrp.Url = "http://hrpaga.intranet.rai.it/hrpaga.asmx";
                    it.rai.servizi.hrpaga.ListaDocumenti lista = hrp.ElencoDocumentiPersonali("00", String.Empty, String.Empty);

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

                //string services = "http://digigapp.servizi.rai.it/wsdigigapp.asmx," +
                //    "http://hrga.servizi.rai.it/Filtro/sedi.asmx," +
                //    "http://sendmail.servizi.rai.it/mail.asmx," +
                //    "http://wiahrss.servizi.rai.it/rp2/ezService/ezService.asmx," +
                //    "http://hrgb.servizi.rai.it/ws/Service.asmx," +
                //    "http://adweb.servizi.rai.it/ADWeb.asmx," +
                //    "http://firmaremota.servizi.rai.it:8080/FirmaRemota/services/RemoteSignature," +
                //    "http://hrpaga.servizi.rai.it/hrpaga.asmx," +
                //    "http://hrpaganew.intranet.rai.it/hrpaga.asmx";

                string services = CommonManager.GetParametro<string>(EnumParametriSistema.ServiziChiSono);

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
                    wc.Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
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
            if (!Utente.IsAbilitatoGapp()) return;

            string rep = Utente.Reparto();
            if (rep == null || rep.Trim() == "" || rep.Trim() == "00") rep = "";
            string sede = Utente.SedeGapp(DateTime.Now) + rep;

            if (string.IsNullOrWhiteSpace(sede)) return;

            var AB = CommonManager.getAbilitazioni();
            var abil = AB.ListaAbilitazioni.Where(x => x.Sede == sede).FirstOrDefault();
            if (abil == null || abil.MatrLivello1==null || ! abil.MatrLivello1.Any())
            {
                Logger.LogErrori(new MyRai_LogErrori()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    error_message = "Nessun L1 per " + sede,
                    matricola = CommonManager.GetCurrentUserMatricola(),
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
                            matricola = CommonManager.GetCurrentUserMatricola(),
                            provenienza = "home/clear"
                        });
                    }
                }
            }
        }
        public ActionResult clear(string dest, string par)
        {
            Session.RemoveAll();
            Session["startup"] = "OK";

            SessionHelper.SessionStart();

            WSDigigapp datiBack_ws1 = new WSDigigapp();
            datiBack_ws1.Credentials = new System.Net.NetworkCredential(
                            CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0],
                            CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]
                            );
            string dateBack = Utente.GetDateBackPerEvidenze();

            var listaEvidenzeResponse = ServiceWrapper.ListaEvidenzWrapper(datiBack_ws1, Utente.Matricola(), dateBack, DateTime.Now.AddDays(-1).ToString("ddMMyyyy"), 70);
            // var listaEvidenzeResponse = datiBack_ws1.ListaEvidenze(Utente.Matricola(), dateBack, DateTime.Now.AddDays(-1).ToString("ddMMyyyy"), 70);

            myRaiHelper.SessionListaEvidenzeModel sessionModel = new myRaiHelper.SessionListaEvidenzeModel( )
            {
                UltimaScrittura = DateTime.Now,
                ListaEvidenze = listaEvidenzeResponse
            };

            // set della sessione per impostare i dati della chiamata ListaEvidenze
            SessionHelper.Set(SessionVariables.ListaEvidenzeScrivania, sessionModel);

            try
            {
                CheckNoBoss();
            }
            catch (Exception ex)
            {
 
            }

            if (String.IsNullOrWhiteSpace(dest))
                return RedirectToAction("/", "");
            else
            {
                dest = dest.Trim(new[] { ' ', '/' });
                return RedirectToAction(
                                dest.Split('/')[1],
                                dest.Split('/')[0],
                                (String.IsNullOrWhiteSpace(par) ? null : GetRouteParams(par))
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

        public ActionResult Index(int? idscelta)
        {
            SessionHelper.Set(SessionVariables.AnnoFeriePermessi, null);

            //  var obj = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(s);

            wcf1.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
            //  wcf1.Credentials  =new System.Net.NetworkCredential( CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]  );
            //  it.rai.servizi.hrpaga.HrPaga wsHrPaga = new it.rai.servizi.hrpaga.HrPaga();
            //  wsHrPaga.Credentials =new System.Net.NetworkCredential( CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]  );
            //  it.rai.servizi.hrpaga.ListaDocumenti lista = wsHrPaga.ElencoDocumenti("01", "");

            //  Response.Write(lista.StringaErrore);

            //  digigappws_wcf1.wApiUtilitydipendente_resp resp = wcf1.recuperaUtente("103650", "01032017");
            //  resp.data.
            string userName = CommonManager.GetCurrentUsername();// System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            //  Response.Write(userName);
            datiBack.Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
            datiBack_ws1.Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
            Autorizzazioni.Sedi SEDI = new Autorizzazioni.Sedi();
            SEDI.Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            pr.menuSidebar = Utente.getSidebarModel();// new sidebarModel(3);
            pr.MieRichieste = new List<MiaRichiesta>();
            pr.digiGAPP = true;
            //pr.dettaglioGiornata = HomeManager.GetTimbratureTodayModel();// new digigppws.dayResponse() { HideRefresh = false };
            //pr.dettaglioGiornata = new digigappws.dayResponse() { HideRefresh=false };
            pr.dettaglioGiornata = HomeManager.GetTimbratureTodayModel();
            pr.Raggruppamenti = HomeManager.GetRaggruppamenti();

            pr.ValidazioneGenericaEccezioni = CommonManager.GetParametro<string>(EnumParametriSistema.ValidazioneGenericaEccezioni);
            pr.SceltePercorso = HomeManager.GetSceltepercorsoModel("PR");
            pr.JsInitialFunction = HomeManager.GetJSfunzioneIniziale(idscelta);

            //pr.presenzaDipendenti = HomeManager.GetPresenzaDipendenti();

            return View(pr);
        }

        public ActionResult PresenzaDipendenti()
        {
            PresenzaDipendenti model = new PresenzaDipendenti();
            //model = HomeManager.GetPresenzaDipendenti();
            model = HomeManager.GetPresenzaDipendenti_ElencoSedi();
            return View("~/Views/Approvazione/_presenzeDipendenti.cshtml", model);
        }

        public ActionResult PresenzaDipedentiSede(string codiceSedeGapp,string desSedeGapp)
        {
            desSedeGapp = System.Web.Helpers.Json.Decode(desSedeGapp);
            PresenzaDipendentiPerSede model = new PresenzaDipendentiPerSede();
            model = HomeManager.GetPresenzaDipendenti_Sede(codiceSedeGapp, desSedeGapp);
            return View("~/Views/Approvazione/subpartial/_sub_presenzeDipendentiPerSede.cshtml", model);
        }

        public ActionResult Index_section2(bool GappClosed)
        {
            SectionDayModel model = new SectionDayModel() { GappClosed = GappClosed };
            string matr = CommonManager.GetCurrentUserMatricola();
            if (!GappClosed)
            {
                //WSDigigapp service = new WSDigigapp()
                //{
                //	Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1])
                //};
                //service.Url = "http://svildigigappws.servizi.rai.it/WSDigigapp.asmx";


                //WSDigigapp service = new WSDigigapp()
                //{
                //	Credentials = new System.Net.NetworkCredential( CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[0], CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[1] )
                //};



                //dayResponse resp = service.getEccezioni(matr, DateTime.Now.ToString("ddMMyyyy"), "BU", 70);


                WSDigigappDataController service = new WSDigigappDataController();
                dayResponse resp = service.GetEccezioni( matr , matr , DateTime.Now.ToString( "ddMMyyyy" ) , "BU" , 70 );
                model.DayResponse = resp;
            }
            return View(model);
        }

        [HttpPost]
        public ActionResult GetEccezioniMadre(string data, string sessionId,string matr=null)
        {
            try
            {
                string m = CommonManager.GetCurrentUserMatricola();
                if (matr != null) m = matr;


                datiBack.Credentials = new System.Net.NetworkCredential(CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
                dayResponse resp = datiBack.getEccezioni(m, data.Replace("/", ""), "BU", 70);

                for (int i =0; i<Session.Keys.Count;i++)
                {
                    if (Session.Keys[i].StartsWith("GECC"))
                        Session[Session.Keys[i]] = null;
                }
                Session[sessionId] = resp;
                
            }
            catch (Exception ex)
            {
                Logger.LogErrori(new MyRai_LogErrori() { error_message=ex.ToString() });
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = false }
                };
            }

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { result = true }
            };
        }

        public ActionResult dettagliogiornata(string data, int IdEccezioneRichiesta)
        {
            string cod_eccezione = String.Empty;
            string userName = CommonManager.GetCurrentUsername();

            datiBack.Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            datiBack_ws1.Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            var db = new digiGappEntities();

            string matricola = CommonManager.GetCurrentUserMatricola();
            var eccrich = db.MyRai_Eccezioni_Richieste.Where(x => x.id_eccezioni_richieste == IdEccezioneRichiesta).FirstOrDefault();

            if (eccrich != null)
            {
                matricola = eccrich.MyRai_Richieste.matricola_richiesta;
                cod_eccezione = eccrich.cod_eccezione;
            }

            try
            {
                pr.dettaglioGiornata = datiBack.getEccezioni(matricola, data.Replace("/", ""), "BU", 70);
            }
            catch (Exception ex)
            {
            }

            pr.PopupDettaglioGiornataModel = HomeManager.GetPopupDettaglioGiornataModel(IdEccezioneRichiesta);
            pr.dettaglioGiornata = HomeManager.GetTimbratureTodayModel(pr.PopupDettaglioGiornataModel.DataEccezione, matricola);
            pr.dettaglioGiornata.HideRefresh = true;
            pr.PopupDettaglioGiornataModel.IdRichiestaEccezione = IdEccezioneRichiesta;

            var eccAmmessa = db.MyRai_Eccezioni_Ammesse.Where(x => x.cod_eccezione.Trim().ToUpper() == eccrich.cod_eccezione.Trim().ToUpper())
                .FirstOrDefault();

            if (eccAmmessa != null && eccAmmessa.id_raggruppamento == 1)
            {
                pr.DipendentiAssenti = ApprovazioniManager.GetDatiDipendentiAssenti(eccrich.MyRai_Richieste.matricola_richiesta,
                    eccrich.codice_sede_gapp, eccrich.MyRai_Richieste.periodo_dal, eccrich.MyRai_Richieste.periodo_al);
            }
            else
                pr.DipendentiAssenti = null;

            pr.Cod_Eccezione = cod_eccezione;
            pr.MatricolaVisualizzata = matricola;
            if (pr.DataVisualizzata == null)
            {
                DateTime D;
                if (DateTime.TryParseExact(data, "dd/MM/yyyy", null, DateTimeStyles.None, out D)) pr.DataVisualizzata = D;
            }
            return View(pr);
        }

        public ActionResult RefreshPOH(Boolean LastMonth = true)
        {
            POHmodel model = new POHmodel();

            string matr = CommonManager.GetCurrentUserMatricola();
            var db = new digiGappEntities();
            var response = Utente.GetAnalisiEccezioni();
            if (response != null)
            {
                foreach (var item in response.DettagliEccezioni)
                {
                    if (item.data.Year != DateTime.Now.Year) model.Anno = item.data.Year;

                    int BilancioParzialeDopoPOH = HomeManager.GetSaldoPohRohAdOggi(response.DettagliEccezioni, item.data, "POH");
                    int BilancioParzialeDopoROH = HomeManager.GetSaldoPohRohAdOggi(response.DettagliEccezioni, item.data, "ROH");

                    if (item.eccezione == "POH")
                    {
                        var rich = db.MyRai_Eccezioni_Richieste.Where(x => x.cod_eccezione == "POH"
                                  && x.data_eccezione == item.data
                                  && x.MyRai_Richieste.matricola_richiesta == matr).FirstOrDefault();


                        model.GiorniPOH.Add(new SingleDay()
                        {
                            data = item.data,
                            minuti = item.minuti,
                            IdStato = rich != null ? rich.id_stato : 0,
                            IdRichiesta = rich != null ? rich.MyRai_Richieste.id_richiesta : 0,
                            codice = "POH",
                            SaldoAttualeCompresaEccezione = BilancioParzialeDopoPOH,
                            SaldoAttualeCompresaEccezioneHHMM = BilancioParzialeDopoPOH.ToHHMM()
                        });
                    }
                    if (item.eccezione == "ROH")
                    {
                        var rich = db.MyRai_Eccezioni_Richieste.Where(x => x.cod_eccezione == "ROH"
                                && x.data_eccezione == item.data
                                && x.MyRai_Richieste.matricola_richiesta == matr).FirstOrDefault();

                        model.GiorniROH.Add(new SingleDay()
                        {
                            data = item.data,
                            minuti = item.minuti,
                            IdStato = rich != null ? rich.id_stato : 0,
                            IdRichiesta = rich != null ? rich.MyRai_Richieste.id_richiesta : 0,
                            codice = "ROH",
                            SaldoAttualeCompresaEccezione = BilancioParzialeDopoROH,
                            SaldoAttualeCompresaEccezioneHHMM = BilancioParzialeDopoROH.ToHHMM()
                        });
                        model.GiorniPOH.Add(new SingleDay()
                        {
                            data = item.data,
                            minuti = item.minuti,
                            IdStato = rich != null ? rich.id_stato : 0,
                            IdRichiesta = rich != null ? rich.MyRai_Richieste.id_richiesta : 0,
                            codice = "ROH",
                            SaldoAttualeCompresaEccezione = BilancioParzialeDopoROH,
                            SaldoAttualeCompresaEccezioneHHMM = BilancioParzialeDopoROH.ToHHMM()
                        });
                    }
                }

                DateTime DataChiusura;
                DateTime.TryParseExact(Utente.GetDateBackPerEvidenze(), "ddMMyyyy", null, System.Globalization.DateTimeStyles.None, out DataChiusura);
                if (LastMonth)
                {
                    model.GiorniPOH = model.GiorniPOH
                        .Where(x => x.data >= DataChiusura)
                        .ToList();
                }
                model.GiorniPOH = model.GiorniPOH.OrderByDescending(x => x.data).ToList();
            }

            if (LastMonth)
                return View("~/Views/tabelle/subpartial/poh.cshtml", model);
            else
                return View("~/Views/feriepermessi/subpartial/poh.cshtml", model);
        }

        public ActionResult RefreshMieRichieste(int? idstato = null, string ecc = null, string dal = null, string al = null, string search = null)
        {
            ModelDash model = new ModelDash();
            model.MieRichieste = HomeManager.GetMieRichiesteModel(idstato, ecc, dal, al, search);

            string mese = DateTime.Now.AddMonths(-1).Month.ToString().PadLeft(2, '0');
            string anno = DateTime.Now.AddMonths(-1).Year.ToString();
            string[] utenteConv = CommonManager.GetParametri<string>(EnumParametriSistema.UtentePerConvalida);

            DateTime PrimoGiornoMeseCorrente = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            DateTime UltimoGiornoMeseScorso = new DateTime(
                PrimoGiornoMeseCorrente.AddDays(-1).Year,
                PrimoGiornoMeseCorrente.AddDays(-1).Month,
                PrimoGiornoMeseCorrente.AddDays(-1).Day);

            string sede = Utente.SedeGapp(DateTime.Now);

            // verifica se l'ultimo giorno del mese precedente è stato convalidato
            //Boolean? convalidato = CommonTasks.VerificaGiornoConvalidato(UltimoGiornoMeseScorso, utenteConv[0], utenteConv[1], sede);

            string tempData = Utente.GetDateBackPerEvidenze();
            DateTime _data = DateTime.ParseExact(tempData, "ddMMyyyy", CultureInfo.InvariantCulture);

            model.MieRichiesteVM = new MieRichiesteVM()
            {
                DataUltimaConvalida = _data
            };

            return View("~/Views/tabelle/subpartial/lemierichieste.cshtml", model);
        }

        public ActionResult RefreshInEvidenza()
        {
            ModelDash model = new ModelDash();
            string dateBack = UtenteHelper.GetDateBackPerEvidenze();
            datiBack_ws1.Credentials = new System.Net.NetworkCredential(CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

            var getListaEvidenze = SessionHelper.Get(SessionVariables.ListaEvidenzeScrivania);

            if (getListaEvidenze != null)
            {
                var sessionData = (SessionListaEvidenzeModel)getListaEvidenze;
                model.listaEvidenze = sessionData.ListaEvidenze;
            }
            else
            {
                model.listaEvidenze = new monthResponseEccezione();
            }

            if (model.listaEvidenze.data != null &&
                model.listaEvidenze.data.giornate.Where(a =>
                    a.TipoEcc == TipoEccezione.AssenzaIngiustificata
                    && (a.timbrature == null || a.timbrature.Count() == 0)).Count() > 0)
            {
                Session["FlagEvidenze"] = true;
            }
            else
            {
                Session["FlagEvidenze"] = false;
            }
            if (UtenteHelper.GetQuadratura() == Quadratura.Settimanale)
            {
                using (myRaiData.digiGappEntities db = new myRaiData.digiGappEntities())
                {
                    string sede = UtenteHelper.SedeGapp(DateTime.Now);
                    var listagiornate = model.listaEvidenze.data.giornate.ToList();
                    if (db.DIGIRESP_Archivio_PDF.Any(y => y.sede_gapp == sede && y.tipologia_pdf == "P"))
                    {
                        listagiornate.RemoveAll(x => x.TipoEcc == TipoEccezione.Carenza && x.data <= (db.DIGIRESP_Archivio_PDF.Where(y => y.sede_gapp == sede && y.tipologia_pdf == "P").Max(z => z.data_fine)));
                    }
                    model.listaEvidenze.data.giornate = listagiornate.ToArray();
                }
                model.listaEvidenze.data.giornate = ScrivaniaManager.ClearCarenze(model.listaEvidenze.data.giornate);

            }
            model.listaEvidenze.data.giornate = ScrivaniaManager.MarcaturaQuadrabili(model.listaEvidenze.data.giornate);
            model.listaEvidenze.data.giornate = ScrivaniaManager.SopprimiQuadrateDaDipendente(model.listaEvidenze.data.giornate);
            model.listaEvidenze.data.giornate = model.listaEvidenze.data.giornate.OrderBy(x => x.data).ToArray();
            //foreach (var evi in model.listaEvidenze.data.giornate)
            //{
            //    if (evi.TipoEcc == digigappWS_ws1.TipoEccezione.TimbratureInSW)
            //    {
            //        string matr = CommonManager.GetCurrentUserMatricola();
            //        var db = new digiGappEntities();
            //        var rich = db.MyRai_Richieste.Where(x =>x.matricola_richiesta==matr && x.MyRai_Eccezioni_Richieste.Any(z => 
            //                                          z.data_eccezione == evi.data &&
            //                                          z.cod_eccezione == "SW")).FirstOrDefault();

            //        if (rich != null)
            //        {
            //            evi.IdSWdaStornare = rich.id_richiesta;
            //        }

            //    }
            //}
            return View("~/Views/tabelle/subpartial/inevidenza.cshtml", model);
        }

        public ActionResult RefreshDaApprovare(string richiedevisti)
        {
            //richiedevisti="1" se da vistare
            string nominativo = Request.QueryString["nome"];
            string StatoVisti = Request.QueryString["OpzioneRicercaVisti"];
            string sede = Request.QueryString["sede"];
            int stato = 0;
            string eccezione = Request.QueryString["eccezione"];
            string dataDa = Request.QueryString["data_da"];
            string dataA = Request.QueryString["data_a"];
            string livelloDip = Request.QueryString["livelloDip"];
            string soloUffProd = Request.QueryString["solouffprod"];

            if (!String.IsNullOrEmpty( soloUffProd ) )
            {
                if (soloUffProd.ToUpper() == "FALSE" || soloUffProd.ToUpper( ) == "UNDEFINED" )
                {
                    soloUffProd = "";
                }
            }

            bool? visualizzati = null;
            try
            {
                if (Request.QueryString["stato"] == "") { stato = 10; } else { stato = Convert.ToInt32(Request.QueryString["stato"]); }
            }
            catch (Exception)
            {
            }

            try
            {
                if (Request.QueryString["visualizzati"] != "")
                {
                    int _myTemp = Convert.ToInt32(Request.QueryString["visualizzati"]);

                    if (_myTemp == 1)
                    {
                        visualizzati = true;
                    }
                    else if (_myTemp == 0)
                    {
                        visualizzati = false;
                    }
                    else
                    {
                        visualizzati = null;
                    }
                }
            }
            catch (Exception)
            {
            }

            ModelDash model = HomeManager.GetDaApprovareModel(pr, false, 0, sede, stato, nominativo, eccezione, dataDa, dataA, visualizzati, livelloDip, soloUffProd,
                richiedevisti == "1", StatoVisti);
            ModelState.Clear();
            model = HomeManager.MarcaLivelloPerRichiedenti(model);
            model.RicercaVisti = StatoVisti;

            //if (Request.QueryString["livelloDip"] != "")
            //{
            //    //// TODO: importante DA RIMUOVERE
            //    //if ( model.elencoProfilieSedi != null )
            //    //{
            //    //	var item = model.elencoProfilieSedi.elencoSediEccezioni.First();

            //    //	item.Codice_sede_gapp = "DDE40";
            //    //	model.elencoProfilieSedi.elencoSediEccezioni.Add( item );
            //    //}


            //    return View("~/Views/responsabile/da_approvare_filtroLiv1.cshtml", model);
            //}
            //else
            //{
            //    return View("~/Views/responsabile/da_approvare.cshtml", model);
            //}

            model.RichiedeVisti = (richiedevisti == "1");

            

            if (Request.QueryString["fromapp"] == "true")
            {
                
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = model.elencoProfilieSedi,
                    MaxJsonLength = int.MaxValue
                };
            }
          else
            return View("~/Views/responsabile/da_approvare.cshtml", model);
        }

        

        public ActionResult RefreshDaApprovareData()
        {
            string nominativo = Request.QueryString["nome"];
            string sede = Request.QueryString["sede"];
            int stato = 0;
            string eccezione = Request.QueryString["eccezione"];
            string dataDa = Request.QueryString["data_da"];
            string dataA = Request.QueryString["data_a"];
            bool? visualizzati = null;
            try
            {
                if (Request.QueryString["stato"] == "") { stato = 10; } else { stato = Convert.ToInt32(Request.QueryString["stato"]); }
            }
            catch (Exception)
            {
            }

            try
            {
                if (Request.QueryString["visualizzati"] != "")
                {
                    int _myTemp = Convert.ToInt32(Request.QueryString["visualizzati"]);

                    if (_myTemp == 1)
                    {
                        visualizzati = true;
                    }
                    else if (_myTemp == 0)
                    {
                        visualizzati = false;
                    }
                    else
                    {
                        visualizzati = null;
                    }
                }
            }
            catch (Exception)
            {

            }

            ModelDash model = HomeManager.GetDaApprovareModel(pr, false, 0, sede, stato, nominativo, eccezione, dataDa, dataA, visualizzati);
            ModelState.Clear();
            model = HomeManager.MarcaLivelloPerRichiedenti(model);

            return View("~/Views/responsabile/da_approvare3.cshtml", model);
        }

        public ActionResult simula(string m)
        {
            string RealName = System.Security.Principal.WindowsIdentity.GetCurrent().Name.ToUpper();
            RealName = RealName.Replace("RAI\\", "").TrimStart(new char[] { 'P' });
            bool myself = (RealName == m);

            string abilitati = CommonManager.GetParametro<string>(EnumParametriSistema.AbilitatiSimulazione);
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
        public ActionResult RefreshDettSettWidget(string CurrentFrom = null, string CurrentTo = null, string dir = null)
        {

            ModelDash model = new ModelDash();

            try
            {
                if (CurrentFrom == null)
                {
                    DateTime[] Da = CommonManager.GetIntervalloSettimanaleSede();
                    DateTime datainizio = Da[0];
                    DateTime datafine = Da[1];
                    //DateTime dt = DateTime.Now;
                    //while (dt.DayOfWeek != DayOfWeek.Monday) dt = dt.AddDays(-1);
                    //model.dettaglioSettimanaleModel = new Models.DettaglioSettimanaleModel(
                    //    wcf1.PresenzeSettimanali(CommonManager.GetCurrentUserMatricola(),
                    //    dt.ToString("ddMMyyyy"), dt.AddDays(6).ToString("ddMMyyyy")));
                    model.dettaglioSettimanaleModel = new DettaglioSettimanaleModel(
                     wcf1.GetPresenzeSettimanaliProtected(CommonManager.GetCurrentUserMatricola(),
                     datainizio.ToString("ddMMyyyy"), datafine.ToString("ddMMyyyy"), Utente.DataInizioValidita(), datainizio));
                }
                else
                {
                    DateTime dfrom;
                    DateTime.TryParseExact(CurrentFrom, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dfrom);

                    DateTime dto;
                    DateTime.TryParseExact(CurrentTo, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dto);

                    if (dir == "a")
                    {
                        //model.dettaglioSettimanaleModel = new Models.DettaglioSettimanaleModel(
                        //  wcf1.PresenzeSettimanali(CommonManager.GetCurrentUserMatricola(),
                        //  dto.AddDays(1).ToString("ddMMyyyy"), dto.AddDays(7).ToString("ddMMyyyy")));
                        model.dettaglioSettimanaleModel = new DettaglioSettimanaleModel(
                         wcf1.GetPresenzeSettimanaliProtected(CommonManager.GetCurrentUserMatricola(),
                         dto.AddDays(1).ToString("ddMMyyyy"), dto.AddDays(7).ToString("ddMMyyyy"), Utente.DataInizioValidita(),
                         dto.AddDays(1)));
                    }
                    else if (dir == "i")
                    {
                        //model.dettaglioSettimanaleModel = new Models.DettaglioSettimanaleModel(
                        //  wcf1.PresenzeSettimanali(CommonManager.GetCurrentUserMatricola(),
                        //  dfrom.AddDays(-7).ToString("ddMMyyyy"), dto.AddDays(-7).ToString("ddMMyyyy")));
                        model.dettaglioSettimanaleModel = new DettaglioSettimanaleModel(
                        wcf1.GetPresenzeSettimanaliProtected(CommonManager.GetCurrentUserMatricola(),
                        dfrom.AddDays(-7).ToString("ddMMyyyy"), dto.AddDays(-7).ToString("ddMMyyyy"), Utente.DataInizioValidita(),
                        dfrom.AddDays(-7)));
                    }
                    else
                    {
                        model.dettaglioSettimanaleModel = new DettaglioSettimanaleModel(
                        wcf1.GetPresenzeSettimanaliProtected(CommonManager.GetCurrentUserMatricola(),
                        dfrom.ToString("ddMMyyyy"), dto.ToString("ddMMyyyy"), Utente.DataInizioValidita(),
                        dfrom));
                    }
                }

                if (model.dettaglioSettimanaleModel.Settimana.Last().GiornoData > DateTime.Today)
                {
                    int deltaCarenze = 0;
                    int deltaMaggioriPresenze = 0;
                    foreach (var giorno in model.dettaglioSettimanaleModel.Settimana.Where(x => x.GiornoData <= DateTime.Today))
                    {
                        string[] elemsCar = giorno.Carenza.Split(':');
                        deltaCarenze -= (Convert.ToInt32(elemsCar[0]) * 60 + Convert.ToInt32(elemsCar[1]));

                        elemsCar = giorno.MaggiorPresenza.Split(':');
                        deltaMaggioriPresenze += Convert.ToInt32(elemsCar[0]) * 60 + Convert.ToInt32(elemsCar[1]);
                    }

                    TimeSpan ts = TimeSpan.FromMinutes(deltaMaggioriPresenze + deltaCarenze);

                    model.dettaglioSettimanaleModel.DeltaTotale = (deltaMaggioriPresenze + deltaCarenze) < 0 ? "-" + ts.ToString(@"hh\:mm") : "+" + ts.ToString(@"hh\:mm");
                    if (model.dettaglioSettimanaleModel.DeltaTotale == "+00:00") model.dettaglioSettimanaleModel.DeltaTotale = "00:00";

                }
            }
            catch (Exception ex)
            {
                //errore servizio
            }
            var Evidenze = SessionManager.Get(SessionVariables.ListaEvidenzeScrivania);
            if (Evidenze != null)
            {
                var sessionData = (SessionListaEvidenzeModel)Evidenze;
                if (sessionData != null && sessionData.ListaEvidenze != null && sessionData.ListaEvidenze.data.giornate.Any())
                {
                    var swtim = sessionData.ListaEvidenze.data.giornate.OrderBy(x => x.data).ToList()
                        .Where(x => x.TipoEcc == TipoEccezione.TimbratureInSW).FirstOrDefault();
                    if (swtim != null)
                    {
                        var db = new digiGappEntities();
                        string matr = CommonManager.GetCurrentUserMatricola();
                        var rich = db.MyRai_Richieste.Where(x => x.matricola_richiesta == matr &&
                                                       x.MyRai_Eccezioni_Richieste.Any(z =>
                                                       z.data_eccezione == swtim.data &&
                                                       z.cod_eccezione == "SW")).FirstOrDefault();
                        //var rich = db.MyRai_Richieste.Where(x => x.MyRai_Eccezioni_Richieste.Any(z => z.data_eccezione == swtim.data &&
                        //           z.cod_eccezione == "SW")).FirstOrDefault();
                        if (rich != null)
                        {
                            model.dettaglioSettimanaleModel.EvidenzaBloccanteIDSWdaStornare = rich.id_richiesta;
                            model.dettaglioSettimanaleModel.EvidenzaBloccanteTipo = "SW";
                            model.dettaglioSettimanaleModel.EvidenzaBloccanteData = swtim.data;
                        }
                    } 
                    else
                    {
                        var assing = sessionData.ListaEvidenze.data.giornate.OrderBy(x => x.data).ToList()
                       .Where(x => x.TipoEcc == TipoEccezione.AssenzaIngiustificata).FirstOrDefault();
                        if (assing != null)
                        {
                            model.dettaglioSettimanaleModel.EvidenzaBloccanteTipo = "ASSEN";
                            model.dettaglioSettimanaleModel.EvidenzaBloccanteData = assing.data;
                        }
                    }
                }
            }
            return View("~/Views/tabelle/dettagliosettimanalewidget.cshtml", model);
        }
        public ActionResult refreshDettaglioGiornata()
        {

            ModelDash model = new ModelDash();
            if (!Utente.GappChiuso())
            {
                model.dettaglioGiornata = HomeManager.GetTimbratureTodayModel();
            }

            if (model.dettaglioGiornata != null) model.dettaglioGiornata.HideRefresh = false;

            return View("~/Views/home/_timbraturetoday.cshtml", model);

        }
        public ActionResult RefreshToday()
        {

            ModelDash model = new ModelDash();
            if (!Utente.GappChiuso())
            {
                model.dettaglioGiornata = HomeManager.GetTimbratureTodayModel();
            }
            if (model.dettaglioGiornata != null) model.dettaglioGiornata.HideRefresh = false;

            return View("~/Views/home/today.cshtml", model);

        }


        public ActionResult getjs()
        {
            string eccSoloQuan = CommonManager.GetParametro<string>(EnumParametriSistema.AccettaSoloDurata);
            string eccDigitaSoloQuant = CommonManager.GetParametro<string>(EnumParametriSistema.EccezioniDigitaSoloQuantita);
            List<string> ListaEcc = new List<string>();
            if (!String.IsNullOrWhiteSpace(eccSoloQuan))
                ListaEcc.AddRange(eccSoloQuan.Split(',').ToList());
            if (!String.IsNullOrWhiteSpace(eccDigitaSoloQuant))
                ListaEcc.AddRange(eccDigitaSoloQuant.Split(',').ToList());

            var db = new digiGappEntities();

            string JS = CommonManager.GetParametro<string>(EnumParametriSistema.ValidazioneGenericaEccezioni)
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
            return Content(CommonManager.GetCurrentUsername());
        }

        public ActionResult Batch()
        {
            var db = new digiGappEntities();
            var Dnow = DateTime.Now;
            int hoursFromNow = CommonManager.GetParametro<int>(EnumParametriSistema.OreRichiesteUrgenti);
            var D48 = Dnow.AddHours(hoursFromNow);

            db.MyRai_Richieste.Where(x => x.id_stato == (int)EnumStatiRichiesta.InApprovazione
                                        && x.data_richiesta < x.periodo_dal
                                        && x.periodo_dal > Dnow
                                        && x.periodo_dal < D48)
                .ToList()
                .ForEach(item => item.urgente = true);

            db.MyRai_Richieste.Where(x => x.id_stato == (int)EnumStatiRichiesta.InApprovazione
                                         && x.data_richiesta < x.periodo_dal
                                         && x.periodo_dal < Dnow)
                 .ToList()
                 .ForEach(item =>
                 {
                     item.scaduta = true;
                     item.urgente = false;
                 });

            if ( DBHelper.Save( db , CommonHelper.GetCurrentUserMatricola( ) ) )
                return Content( "OK" );
            else
                return Content( "Errore salvataggio DB" );
        }

        public ActionResult NotAuthExt()
        {
            ViewBag.unframed = true;
            return View("NonAutorizzato");
        }

        public ActionResult NotAuth()
        {
            return View("NonAutorizzato");
        }
        public ActionResult version(string set )
        {
            if (!String.IsNullOrWhiteSpace(set) && set.ToLower() == "all")
            {
                string[] AllFiles = System.IO.Directory.GetFiles(Server.MapPath("~"), "*.*", System.IO.SearchOption.AllDirectories);
                var T = new List<Tuple<string, long, string>>();

                foreach (string f in AllFiles)
                {
                    System.IO.FileInfo F = new System.IO.FileInfo(f);
                    T.Add(new Tuple<string, long, string>(f, F.Length, F.LastWriteTime.ToString("dd/MM/yyyy HH.mm")));
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
            service.Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);

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
    }
}
