using System;
using System.Web.Mvc;
using myRaiCommonManager;
using myRaiCommonModel;
using myRaiHelper;
using Utente = myRaiHelper.UtenteHelper;

namespace myRai.Controllers
{
    public class PopupBossController : BaseCommonController
    {
        public ActionResult GetPeriodoSW(string matricola)
        {
            var p = Utente.GetPeriodoSW(matricola);
            if (p==null)
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result =false}
                };
            else
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = true,
                        period ="Dal " +p.DTA_INIZIO.ToString("dd/MM/yyyy") + " al " +p.DTA_FINE.ToString("dd/MM/yyyy") }
                };
        }
		public ActionResult GetEvidenzePerPopupBoss ( string matricola, string data = null )
        {
            ModelDash model = PopupBossManager.GetEvidenzeModelPerMatricola(matricola);

			if ( !String.IsNullOrEmpty( data ) )
			{
				model.DataVisualizzata = DateTime.Parse( data );
			}

            return View("~/Views/tabelle/subpartial/inevidenza.cshtml", model);
        }

        public ActionResult GetAnagrafica(string matricola, string data = null)
        {
            UtenteTerzo u = new UtenteTerzo();
            if (matricola != null && matricola.Length == 7) matricola = matricola.Substring(1);
            UtenteTerzoAnagrafica model = u.EsponiAnagrafica(matricola);

            return View("~/Views/Scrivania/subpartial/anagrafica.cshtml", model);
        }

        public ActionResult GetTimbraturePopover(string matricola, string data)
        {

            return Content("test timbrature");
        }
    }
}