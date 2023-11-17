using myRaiCommonTasks.it.rai.servizi.svildigigappws;
using myRaiData.Incentivi;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace myRai.Controllers
{
    public class RaiDesignController : Controller
    {
        //
        // GET: /Design/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult OnePage()
        {
            return View();
        }

        public ActionResult SubPage1()
        {
            return View("subpartial/FormControl", new RaiSelectControl());
        }

        public ActionResult ModaleFull()
        {
            return View("subpartial/ModaleFull");
        }

        public ActionResult ModaleHalf()
        {
            return View("subpartial/ModaleHalf");
        }

        public ActionResult Save_Form()
        {
            return Content("OK");
        }

        public ActionResult GetSelectOption(string filter)
        {
            IncentiviEntities db = new IncentiviEntities();
            var listServizi = db.XR_TB_SERVIZIO.Where(x => x.COD_SERVIZIO.Length == 2 && x.DES_SERVIZIO.Contains(filter));

            return Json(listServizi.Select(x => new { id = x.COD_SERVIZIO, text = x.DES_SERVIZIO }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetNewSelectOption(string filter, string value)
        {
            IncentiviEntities db = new IncentiviEntities();
            IEnumerable<XR_TB_SERVIZIO> listServizi = null;
            if (value!=null)
                listServizi = db.XR_TB_SERVIZIO.Where(x => x.COD_SERVIZIO.Length == 2 && x.COD_SERVIZIO==value);
            else
                listServizi = db.XR_TB_SERVIZIO.Where(x => x.COD_SERVIZIO.Length == 2 && x.DES_SERVIZIO.Contains(filter));

            return Json(listServizi.Select(x => new SelectListItem{ Value = x.COD_SERVIZIO, Text = x.DES_SERVIZIO }), JsonRequestBehavior.AllowGet);
        }

        public ActionResult SubmitFileUpload(HttpPostedFileBase[] fileUpload)
        {
            return Content("OK");
        }

        public ActionResult AsyncSubmitFileUpload(HttpPostedFileBase fileUpload)
        {
            return Content("OK");
        }

        public ActionResult RaiSTWebServiceCall(int idProc, int idCall)
        {
            bool boolEsito = false;
            string msg = "";
            string durata = "";

            WSDigigapp service = new WSDigigapp()
            {
                Credentials = new System.Net.NetworkCredential("ssrvruofpo", "zaq22?mk")
            };

            DateTime before = DateTime.Now;
            try
            {

                var resp = service.getEccezioni("0103650", "15062020", "BU", 80);
                if (resp==null)
                {
                    msg = "No response";
                }
                else if (!String.IsNullOrWhiteSpace(resp.errore))
                {
                    msg = resp.errore;
                }
                else
                {
                    boolEsito = true;
                }
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            DateTime after = DateTime.Now;

            var diff = after.Subtract(before);
            durata = String.Format("{0} ms", diff.TotalMilliseconds);

            return Json(new { Esito = boolEsito, Message = msg, Durata= durata, IdProc = idProc, IdCall = idCall }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult NoContentPage(string c)
        {
            string pathView = "";
            switch (c)
            {
                case "404":
                    pathView = "~/Views/Shared/404.cshtml";
                    break;
                case "not_auth":
                    pathView = "~/Views/Shared/NonAbilitatoError.cshtml";
                    break;
                case "not_auth_2":
                    pathView = "~/Views/Shared/NonAbilitatoError2.cshtml";
                    break;
                case "errore":
                    pathView = "~/Views/Shared/Error.cshtml";
                    break;
                case "errore_2":
                    pathView = "~/Views/Shared/Error2.cshtml";
                    break;
                case "bad_browser":
                    pathView = "~/Views/Home/BadBrowser.cshtml";
                    break;
                case "home_not_auth":
                    pathView = "~/Views/Home/NonAutorizzato.cshtml";
                    break;
                default:
                    break;
            }

            return View(pathView);
        }
    }
}
