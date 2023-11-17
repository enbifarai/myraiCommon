using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using myRaiData;
using System.Web.SessionState;
using myRaiHelper;
using myRaiCommonManager;

namespace myRai.Controllers
{
    [SessionState(SessionStateBehavior.ReadOnly)]
    public class BaseCommonController2 : Controller
    {

        public BaseCommonController2()
        {

        }
        ExceptionContext exceptionContext { get; set; }

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
            
            // Risolto con:
            // Tabella MyRai_ParametriSistema
            // BrowserAmmessi > IE Versione 0
            //if (browserName == "IE")
            //{
            //    //controllo la versione di Trident per verificare se è attiva la compatibilty view
            //    if (userAgent.Contains("Trident/7.0"))
            //    {
            //        browserName = "InternetExplorer";
            //        majorVersion = 11;
            //    }
            //    else if (userAgent.Contains("Trident/6.0"))
            //    {
            //        browserName = "InternetExplorer";
            //        majorVersion = 10;
            //    }               
            //}

            // Risolto con:
            // Tabella MyRai_ParametriSistema
            // BrowserAmmessi > Safari Versione 0
            //if (browserName == "Safari" && majorVersion == 0 
            //    && userAgent.Contains("CriOS"))
            //{
            //    //In questo caso l'accesso è stato fatto da Chrome su iOS
            //    browserName = "Chrome";
            //    Regex regex = new Regex(@"CriOS/(?'version'(?'major'\d+)(\.(?'minor'\d+)?)\w*)");
            //    Match match = regex.Match(userAgent);
            //    majorVersion = Convert.ToInt32(match.Groups["major"].Value);
            //}

            // Risolto con:
            // Tabella MyRai_ParametriSistema
            // BrowserAmmessi > Mozilla Versione 0
            //if (browserName == "Mozilla" && majorVersion == 0
            //    && userAgent.Contains("IEMobile"))
            //{
            //    //Accesso da IE Mobile (Windows Phone)
            //    browserName = "IEMobile";
            //    Regex regex = new Regex(@"IEMobile/(?'version'(?'major'\d+)(\.(?'minor'\d+)?)\w*)");
            //    Match match = regex.Match(userAgent);
            //    majorVersion = Convert.ToInt32(match.Groups["major"].Value);                
            //}

            digiGappEntities db = new digiGappEntities();
            string browsersKey=EnumParametriSistema.BrowserAmmessi.ToString();
            var Ammesso = db.MyRai_ParametriSistema
                .Where(x => 
                    x.Chiave== browsersKey &&
                    x.Valore1 == browserName).FirstOrDefault();

            double v;
            if (Ammesso == null || Ammesso.Valore2 == "*"
                    || String.IsNullOrWhiteSpace(Ammesso.Valore2)
                    || !double.TryParse(Ammesso.Valore2, out v)
                    || majorVersion < v)
            {
                MyRai_LogErrori err = new MyRai_LogErrori();
                err.matricola = CommonHelper.GetCurrentUserMatricola( );
                err.data = DateTime.Now;
                err.error_message = String.Format("Browser: {0}\r\nMajor Version: {1}\r\nUser Agent: {2}", browserName, majorVersion, userAgent);
                err.applicativo = "Portale";
                err.provenienza = "BadBrowser";
                Logger.LogErrori(err);
                return false;
            }
            return true;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string Destinazione = filterContext.Controller.ToString() + "/" + filterContext.ActionDescriptor.ActionName;

            // se l'utente non è autorizzato allora deve essere rimandato alla pagina "/Home/notAuth"
            string sMat = CommonHelper.GetCurrentUserMatricola( );
            int iMat = -1;

			string controllerName = filterContext.Controller.ControllerContext.RouteData.Values["controller"].ToString().ToLower();
			string actionName = filterContext.Controller.ControllerContext.RouteData.Values["action"].ToString().ToLower();

            bool redirectBustaPaga = CommonHelper.IsGiornoCedolino( CommonHelper.GetCurrentUserMatricola( ) );
            bool redirectDocAmm = CommonHelper.IsGiornoDocAmm();
            
            if (Destinazione != "myRai.Controllers.HomeController/clear" &&
               Destinazione != "myRai.Controllers.HomeController/Loader" &&
               Destinazione != "myRai.Controllers.HomeController/getjs" )
            {
                if (Session["startup"] == null)
                {
                    if (!Request.IsAjaxRequest())
                    {
                        if (redirectBustaPaga && Session["RedirectBustaPaga"] == null)
                    {
                        Session["RedirectBustaPaga"] = "true";
                        filterContext.Result = new RedirectResult("/BustaPaga");
                        return;
                    }
                        if (redirectDocAmm && Session["RedirectDocAmm"]==null)
                        {
                            Session["RedirectDocAmm"] = "true";
                            filterContext.Result = new RedirectResult("/DocumentiAmministrativi");
                            return;
                        }
                    }

                    Session["startup"] = "ok";

                    if (Request.IsAjaxRequest())
                    {
                        //filterContext.Result = new RedirectResult("/Home/Loader?dest=/scrivania/index&par=");
                    }
                    else
                    {
                        string qsParams = null;
                        if (filterContext.HttpContext.Request.QueryString != null)
                            qsParams = HttpUtility.UrlDecode(filterContext.HttpContext.Request.QueryString.ToString());

                        filterContext.Result = new RedirectResult("/Home/Loader?dest=/" + controllerName + "/"
                            + actionName + (qsParams != null ? "&par=" + HttpUtility.UrlEncode(qsParams) : ""));
                    }
                    
                    return;
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
            if ((Session["startup"] == null && controllerName == "scrivania" && actionName == "index")
                || (controllerName == "home" && new string[]{ "loader","clear","badbrowser","getjs"}.Contains(actionName)))
            {
                Controllers.Add("scrivania");
                Controllers.Add("home");
            }
            else
            {
                Controllers = UtenteHelper.getAuthorizedControllers();
            }

            //se non va già verso BadBrowser e Browser non ammesso
            if (filterContext.ActionDescriptor.ActionName != "BadBrowser" && !BrowserAmmesso(filterContext))
            {
                filterContext.Result = new RedirectResult("/Home/BadBrowser");
                return;
            }

            //se proviene da chiamata ajax
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                //se la sessione è scaduta, ricrea tutto
                if (Session["Utente"] == null)
                    SessionHelper.SessionStart();

                // prosegui ajax
                return;
            }

            if (controllerName == "home" && (new string[] { "badbrowser", "notauth", "getjs", "clear", "loader", "chisono","simula","getimg" }.Contains(actionName)))
            {
                return;
            }

            if (controllerName == "tech")
            {
                return;
            }

            if(
                controllerName=="dispositivi"
                ||
                ( UtenteHelper.IsAdmin() && (controllerName == "profilimenu" || controllerName == "gestione" || controllerName=="ajax" ))
               )
            { 
                //autorizzato per definizione
            }
            else
            {
                if (!Controllers.Contains(controllerName))
                {
                    LogAuthError(controllerName, actionName, filterContext, Controllers);
                    filterContext.Result = new RedirectResult("/Home/notAuth");
                    return;
                }
            }
            try
            {
                string Parameters = "";
                foreach (var item in filterContext.ActionParameters) Parameters += item.Key + "=" + item.Value;

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

            using (digiGappEntities db = new digiGappEntities())
            {
                MyRai_LogErrori err = new MyRai_LogErrori();
                err.matricola = CommonHelper.GetCurrentUserMatricola( );
                err.data = DateTime.Now;
                err.error_message = filterContext.Exception.ToString() + "\n" + filterContext.HttpContext.Request.Url;
                err.applicativo = "Portale";
                err.provenienza = filterContext.Controller.ToString();
                Logger.LogErrori(err);

                Session["ErrorId"] = err.Id;
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