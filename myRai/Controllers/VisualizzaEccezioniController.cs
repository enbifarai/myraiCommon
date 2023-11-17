using myRaiCommonManager;
using myRaiCommonModel;
using myRaiHelper;
using System;
using System.Web.Mvc;

namespace myRai.Controllers
{
    public class VisualizzaEccezioniController :  BaseCommonController
    {
       
        public ActionResult Index()
        {
            VisualizzaEccezioniModel model = VisualizzaEccezioniManager.GetVisualizzaEccezioniModel();
            return View(model);
        }

        public ActionResult getecc(string tem, string tipiassenza)
        {
            int[] IdTematiche = null;
            if (!String.IsNullOrWhiteSpace(tem))
                     IdTematiche = Array.ConvertAll(tem.Split(','), item => Convert.ToInt32(item));

            int[] idTipi = null;
            if (!String.IsNullOrWhiteSpace(tipiassenza))
                idTipi = Array.ConvertAll(tipiassenza.Split(','), item => Convert.ToInt32(item));

            VisualizzaEccezioniModel model = VisualizzaEccezioniManager.GetVisualizzaEccezioniModel(IdTematiche,idTipi);

            return View("listaEccezioni", model);
        }
    }
}