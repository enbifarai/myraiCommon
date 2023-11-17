using myRai.Business;
using myRai.Models;
using myRaiCommonManager;
using myRaiCommonModel;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace myRai.Controllers
{
    public class BatchAccessController : Controller
    {
        //http://localhost:53693/batchaccess/getpropauto?date=20112019
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (CommonHelper.IsBatchAccess()==null)
            {
                filterContext.Result = new RedirectResult("/Home/notAuth");
                return;
            }
        }

        public ActionResult GetPropAuto(string date)
        {
            SessionHelper.SessionStart();
            ajaxController ac = new ajaxController();
            var model = ac.GetProposteAssistenteModel(date);
            if (model.EccezioniProposte != null)
            {
                var tn35 = model.EccezioniProposte.Where(x => x.cod != null && x.cod.Trim().ToUpper() == "TN35").FirstOrDefault();
                if (tn35 != null)
                {
                    PropostaAutoToSave p = new PropostaAutoToSave()
                    {
                        cod = "TN35",
                        d=date,
                        quantita = tn35.qta,
                        dalle = tn35.dalle,
                        alle = tn35.alle,
                        index = 1
                    };
                    JsonResult json= (JsonResult) ac.saveProposteAuto(new PropostaAutoToSave[] { p });
                    string jsonString = new JavaScriptSerializer().Serialize(json);
                }
            }
            return null;

        }
    }
}
