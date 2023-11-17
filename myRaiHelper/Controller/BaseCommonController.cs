using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using myRaiData;
using System.Web.SessionState;

namespace myRaiHelper
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class BaseCommonController : Controller
    {
        public BaseCommonController()
        {

        }
        ExceptionContext exceptionContext { get; set; }

        public ActionResult RenderModalBody(string title, string url, object o)
        {
            ModalTemplateBody model = new ModalTemplateBody()
            {
                Title = title,
                Model = o,
                ViewUrl = url
            };

            return View("~/Views/_RaiDesign/ModalTemplateBody.cshtml", model);
        }

        private void LogAuthError(string controllerName, string actionName, ActionExecutingContext filterContext, List<string> Controllers)
        {
            var sidebar = UtenteHelper.getSidebarModel();
            Logger.LogErrori(new MyRai_LogErrori()
            {
                applicativo = "PORTALE",
                data = DateTime.Now,
                matricola = CommonHelper.GetCurrentUserMatricola(),
                provenienza = "BaseCommonController.OnActionExecuting",
                error_message = "NonAutorizzato per " + controllerName + "/" + actionName
                + " -Url:" + filterContext.HttpContext.Request.Url
                + " -Controllers Auth:" + String.Join(",", Controllers.ToArray())
                + " -Profili:" +
                    (sidebar != null && sidebar.myProfili != null
                    ? String.Join(",", sidebar.myProfili.Select(x => x.nome_profilo).ToArray())
                    : "Nessuna voce")
            });
        }

        private bool BrowserAmmesso(ActionExecutingContext filterContext)
        {
            string browserName = filterContext.HttpContext.Request.Browser.Browser;
            int majorVersion = filterContext.HttpContext.Request.Browser.MajorVersion;
            string userAgent = filterContext.HttpContext.Request.UserAgent;

            digiGappEntities db = new digiGappEntities();
            string browsersKey = EnumParametriSistema.BrowserAmmessi.ToString();
            var Ammesso = db.MyRai_ParametriSistema
                .Where(x =>
                    x.Chiave == browsersKey &&
                    x.Valore1 == browserName).FirstOrDefault();

            double v;
            if (Ammesso == null || Ammesso.Valore2 == "*"
                    || String.IsNullOrWhiteSpace(Ammesso.Valore2)
                    || !double.TryParse(Ammesso.Valore2, out v)
                    || majorVersion < v)
            {
                MyRai_LogErrori err = new MyRai_LogErrori();
                err.matricola = CommonHelper.GetCurrentUserMatricola();
                err.data = DateTime.Now;
                err.error_message = String.Format("Browser: {0}\r\nMajor Version: {1}\r\nUser Agent: {2}", browserName, majorVersion, userAgent);
                err.applicativo = "Portale";
                err.provenienza = "BadBrowser";
                Logger.LogErrori(err);
                return false;
            }

            return true;

            //if (!double.TryParse(Ammesso.Valore2, out v)) return false;

            //return (filterContext.HttpContext.Request.Browser.MajorVersion >= v);
        }


        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var appSettings = System.Configuration.ConfigurationManager.AppSettings;
            ApplicationType _appType = CommonHelper.GetApplicationType();


            string Destinazione = filterContext.Controller.ToString() + "/" + filterContext.ActionDescriptor.ActionName;


            // se l'utente non è autorizzato allora deve essere rimandato alla pagina "/Home/notAuth"
            string sMat = CommonHelper.GetCurrentUserMatricola();
            int iMat = -1;

            string controllerName = filterContext.Controller.ControllerContext.RouteData.Values["controller"].ToString().ToLower();
            string actionName = filterContext.Controller.ControllerContext.RouteData.Values["action"].ToString().ToLower();

            if (controllerName != null && controllerName.ToLower() == "strutturaorganizzativa"
                && actionName != null && actionName.ToLower() == "getincarichiallext")
            {
                return;
            }
            bool redirectBustaPaga = _appType == ApplicationType.RaiPerMe && CommonHelper.IsGiornoCedolino(sMat);
            bool redirectDocAmm = _appType == ApplicationType.RaiPerMe && CommonHelper.IsGiornoDocAmm();

            //if (Destinazione != "myRai.Controllers.HomeController/clear" &&
            //   Destinazione != "myRai.Controllers.HomeController/Loader" &&
            //   Destinazione != "myRai.Controllers.HomeController/getjs")

            if (controllerName != "home" || !"clear,loader,getjs".Contains(actionName))
            {
                if (SessionHelper.Get("startup") == null)
                {
                    if (!Request.IsAjaxRequest() && _appType == ApplicationType.RaiPerMe)
                    {
                        if (redirectBustaPaga && SessionHelper.Get("RedirectBustaPaga") == null)
                        {
                            Session["RedirectBustaPaga"] = "true";
                            filterContext.Result = new RedirectResult("/BustaPaga");
                            return;
                        }
                        if (redirectDocAmm && SessionHelper.Get("RedirectDocAmm") == null)
                        {
                            Session["RedirectDocAmm"] = "true";
                            filterContext.Result = new RedirectResult("/DocumentiAmministrativi");
                            return;
                        }
                    }

                    SessionHelper.Set("startup", "ok");
                    SessionHelper.Set("_tms_startup", DateTime.Now);

                    if (Request.IsAjaxRequest())
                    {
                        //filterContext.Result = new RedirectResult("/Home/Loader?dest=/scrivania/index&par=");
                    }
                    else if(actionName != "getfilefirmato")
                    {
                        string qsParams = null;
                        if (filterContext.HttpContext.Request.QueryString != null)
                            qsParams = HttpUtility.UrlDecode(filterContext.HttpContext.Request.QueryString.ToString());

                        filterContext.Result = new RedirectResult("/Home/Loader?dest=/" + controllerName + "/"
                            + actionName + (qsParams != null ? "&par=" + HttpUtility.UrlEncode(qsParams) : ""));
                    }

                    return;
                }
                else
                {
                    if (controllerName.ToLower()=="scrivania" && actionName.ToLower()=="index")
                    {
                        using (var db = new digiGappEntities())
                        {
                            var paramClear = db.MyRai_ParametriPersonali.Where(x => x.matricola == sMat && x.nome_parametro.Equals("ForceClearSession")).FirstOrDefault();
                            if (paramClear != null && !String.IsNullOrWhiteSpace(paramClear.valore_parametro))
                            {
                                DateTime? tmsStart = SessionHelper.Get<DateTime>("_tms_startup", null);
                                if (tmsStart.HasValue
                                    && DateTime.Now >= paramClear.valore_parametro.ToDateTime("dd/MM/yyyy HH:mm")
                                    && tmsStart <= paramClear.valore_parametro.ToDateTime("dd/MM/yyyy HH:mm")
                                    )
                                {
                                    SessionHelper.Set("startup", null);
                                    SessionHelper.Set("_tms_startup", null);

                                    string qsParams = null;
                                    if (filterContext.HttpContext.Request.QueryString != null)
                                        qsParams = HttpUtility.UrlDecode(filterContext.HttpContext.Request.QueryString.ToString());

                                    filterContext.Result = new RedirectResult("/Home/clear?dest=/" + controllerName + "/"
                                        + actionName + (qsParams != null ? "&par=" + HttpUtility.UrlEncode(qsParams) : ""));

                                    return;
                                }
                            }
                        }
                    }
                }
            }

            bool logAzioneAttivo = false;

            using (digiGappEntities db = new digiGappEntities())
            {
                var parametri = db.MyRai_ParametriSistema.Where(w => w.Chiave.Equals("AttivaLogAzione")).FirstOrDefault();

                if (parametri != null)
                {
                    bool.TryParse(parametri.Valore1, out logAzioneAttivo);
                }
            }

            if (logAzioneAttivo)
            {
                Logger.LogAzione(new MyRai_LogAzioni()
                {
                    applicativo = "PORTALE",
                    data = DateTime.Now,
                    matricola = CommonHelper.GetCurrentUserMatricola(),
                    descrizione_operazione = String.Format("{0} - {1}", controllerName, actionName),
                    operazione = actionName,
                    provenienza = "BaseCommonController.OnActionExecuting"
                });
            }

            List<string> Controllers = new List<string>();
            if ((SessionHelper.Get("startup") == null && controllerName == "scrivania" && actionName == "index")
                || (controllerName == "home" && new string[] { "loader", "clear", "badbrowser", "getjs" }.Contains(actionName)))
            {
                Controllers.Add("scrivania");
                Controllers.Add("home");
            }
            else
            {


                Controllers = UtenteHelper.getAuthorizedControllers();
                // if (CommonHelper.GetCurrentUserMatricola() == "451598") Controllers.Add("maternitacongedi");
            }

            if (filterContext.ActionDescriptor.ActionName != "GetFileFirmato")
            {
                //se non va già verso BadBrowser e Browser non ammesso
                if (filterContext.ActionDescriptor.ActionName != "BadBrowser" && !BrowserAmmesso(filterContext))
                {
                    filterContext.Result = new RedirectResult("/Home/BadBrowser");
                    return;
                }
            }
            

            //se proviene da chiamata ajax
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                //se la sessione è scaduta, ricrea tutto
                if (SessionHelper.Get("startup") == null)
                    SessionHelper.SessionStart();

                // prosegui ajax
                return;
            }

            if (controllerName == "home" && (new string[] { "badbrowser", "notauth", "getjs", "clear", "loader", "chisono", "simula", "getimg" }.Contains(actionName)))
            {
                return;
            }

            if (controllerName == "tech")
            {
                return;
            }

            if (
                controllerName == "dispositivi"
                ||
                (UtenteHelper.IsAdmin() && (controllerName == "profilimenu" || controllerName == "gestione" || controllerName == "ajax"))
               )
            {
                //autorizzato per definizione
            }
            else
            {
                if (!Controllers.Contains(controllerName))
                {
                    if (_appType == ApplicationType.Gestionale)
                        HrisHelper.LogOperazione("401_Controller", "Controller: " + controllerName + ", Action:" + actionName, false);

                    LogAuthError(controllerName, actionName, filterContext, Controllers);
                    filterContext.Result = new RedirectResult("/Home/notAuth");
                    return;
                }
            }

            try
            {
                string Parameters = "";
                foreach (var item in filterContext.ActionParameters)
                    Parameters += item.Key + "=" + item.Value;

                string chiamante = this.Url.RequestContext.RouteData.Values["Controller"] + "/" + this.Url.RequestContext.RouteData.Values["Action"];
                var db = new myRaiData.digiGappEntities();
                string act = this.Url.RequestContext.RouteData.Values["Action"].ToString().ToUpper().TrimEnd();
                if (act.Contains("INDEX") && act.Length < 7)
                {
                    var help = db.MyRai_Help.Where(x => x.chiamante.TrimEnd() == chiamante).Select(x => x.nomeview).FirstOrDefault();
                    if (!string.IsNullOrEmpty(help))
                    {
                        string path = Server.MapPath(@"~/Views/Guida\subpartial\" + help + ".cshtml");
                        if (System.IO.File.Exists(path))
                        {
                            ViewData["viewhelp"] = db.MyRai_Help.Where(x => x.chiamante.TrimEnd() == chiamante).Select(x => x.nomeview).FirstOrDefault();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewData["viewhelp"] = ex.Message;
            }
        }
        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {

        }
        protected override void OnException(ExceptionContext filterContext)
        {
            string ecc = filterContext.Exception.ToString();

            if (CommonHelper.GetApplicationType() == ApplicationType.Gestionale)
                HrisHelper.LogOperazione("Exception", ecc, false, ex: filterContext.Exception);

            using (digiGappEntities db = new digiGappEntities())
            {
                MyRai_LogErrori err = new MyRai_LogErrori();
                err.matricola = CommonHelper.GetCurrentUserMatricola();
                err.data = DateTime.Now;
                err.error_message = filterContext.Exception.ToString() + "\n" + filterContext.HttpContext.Request.Url;
                err.applicativo = "Portale";
                err.provenienza = filterContext.Controller.ToString();
                Logger.LogErrori(err);

                SessionHelper.Set("ErrorId", err.Id);
            }
        }
        protected override void HandleUnknownAction(string actionName)
        {
            Http404().ExecuteResult(ControllerContext);
        }
        protected virtual ActionResult Http404()
        {
            ViewBag.url = Request.Url.AbsoluteUri;
            ViewBag.userAgent = Request.UserAgent;
            ViewBag.userIp = Request.UserHostAddress;
            return View("404");
        }
    }
}