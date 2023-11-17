using myRaiCommonManager;
using myRaiCommonModel;
using myRaiData.Incentivi;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace myRaiGestionale.Controllers
{
    public class TrattamentoEconomicoController : BaseCommonController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            SessionHelper.Set("GEST_SECTION", "AMMINISTRAZIONE");
            base.OnActionExecuting(filterContext);
        }

        public ActionResult Index()
        {
            TrattamentoEconomicoIndexModel model = TrattamentoEconomicoManager.GetTrattamentoEconomicoIndexModel();
            return View(model);
        }
        public ActionResult GetContent(SearchTeModel search)
        {
            TrattamentoEconomicoModel model = TrattamentoEconomicoManager.GetTrattamentoEconomicoModel(search);
            return View("content", model);
        }
        public ActionResult GetPopupContent(int id)
        {
            PopupTeModel model = TrattamentoEconomicoManager.GetPopupTEModel(id);
            return View("PopupTEcontent", model);
        }
        public ActionResult GetPopupContentAssegna(int id)
        {
            var model = TrattamentoEconomicoManager.GetPopupTEassegnaModel(id);
            return View("_popupTEAssegna", model);
        }
        public ActionResult GetPopupContentDettagliLaterale(int id)
        {
            PopupTeModel model = TrattamentoEconomicoManager.GetPopupTEModel(id);
            return View("_popupTEdettagliLaterale", model);
        }
        public ActionResult GetPopupContentCentro(int id)
        {
            PopupTeModel model = TrattamentoEconomicoManager.GetPopupTEModel(id);
            return View("_PopupTEcentro", model);
        }
    }
}
