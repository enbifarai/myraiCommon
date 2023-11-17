using myRaiCommonManager;
using myRaiCommonModel;
using myRaiData;
using myRaiHelper;
using System;
using System.Linq;
using System.Web.Mvc;

namespace myRai.Controllers
{
    public class ResocontiController : BaseCommonController
    {
        public ActionResult Index()
        {
            ResocontiModel model = ResocontiManager.GetResocontiModel( CommonHelper.GetCurrentUserMatricola( ) );
            model.menuSidebar = UtenteHelper.getSidebarModel();// new sidebarModel(3);
            return View(model);
        }

        public ActionResult GetResoconti(bool onlypreview=true , string sedi=null)
        {
            ResocontiModel model = ResocontiManager.GetResocontiModel( CommonHelper.GetCurrentUserMatricola( ), onlypreview :onlypreview, sediVisualizzate:sedi);
            return View("~/Views/resoconti/_resoconti.cshtml", model);
        }

        public ActionResult AggiornaResoconti(string sede, string datada, string dataa)
        {
            var db = new digiGappEntities();
            DateTime D1;
            DateTime.TryParseExact(datada, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D1);
            DateTime D2;
            DateTime.TryParseExact(dataa, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D2);
            var res = db.MyRai_Resoconti_GetPresenze.Where(x => x.sede == sede && x.data_inizio == D1 && x.data_fine == D2).FirstOrDefault();
            if (res != null)
            {
                db.MyRai_Resoconti_GetPresenze.Remove(res);
                db.SaveChanges();
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = true, data1 = D1.AddDays(7).ToString("dd/MM/yyyy"), data2 = D2.AddDays(7).ToString("dd/MM/yyyy") }
            };
        }
        public ActionResult GetResocontiParziale(string da, string a, bool avanti,string sede)
        {
            if (!String.IsNullOrWhiteSpace(sede)) sede = sede.Replace("tbody-", "");
            ResocontiModel model = ResocontiManager.GetResocontiModel(CommonHelper.GetCurrentUserMatricola(), da,a,avanti,false,null, sede);
            return View("~/Views/resoconti/_resoconti.cshtml", model);
        }

        public ActionResult GeneraPresenze(string sede, string da, string a)
        {
            var response = ResocontiManager.GeneraPresenze(sede,da,a);
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new {
                    result = response.esito, 
                    errore=response.errore 
                            }
            };
        }

        public ActionResult GetPdfMissing(string sede)
        {
            int response = ResocontiManager.QuantiPdfDaGenerare( CommonHelper.GetCurrentUserMatricola( ) , sede ).Count ();
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new
                {
                    result = response
                }
            };
        }
    }
}