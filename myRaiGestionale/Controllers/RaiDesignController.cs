using myRaiData.Incentivi;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace myRaiGestionale.Controllers
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

        public ActionResult GetNewSelectOption(string filter, string value, string optional="")
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
            FileHelper.UploadFile("103650", "Test", new FileOperLog("P103650", "test"), fileUpload);

            return Content("OK");
        }

        public ActionResult TestGetFile()
        {
            var result = FileHelper.DownloadFile("103650", "Test", "Elenco collassato.JPG");

            return File(result.File.Content, result.File.ContentType);
        }

        public ActionResult SubmitCheckMulti(string[] flagTipoConto)
        {

            return Content("OK");
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
