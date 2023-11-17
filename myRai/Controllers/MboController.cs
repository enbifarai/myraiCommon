using myRaiCommonModel.Gestionale;
using myRaiCommonManager;
using myRaiData.Incentivi;
using myRaiHelper;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace myRai.Controllers
{
    public class MboController : BaseCommonController
    {
        public ActionResult Index()
        {
            if (!MboHelper.IsEnabled())
                return new RedirectResult("/Home/notAuth");

            return View();
        }

        public ActionResult Elenco_Schede(MboRicerca model = null)
        {
            List<MboScheda> schede = MboManager.GetSchede(model, true, CommonHelper.GetCurrentUserMatricola());
            return PartialView(schede);
        }
        [HttpPost]
        public ActionResult Modal_Scheda(int idScheda)
        {
            MboScheda scheda = MboManager.GetScheda(idScheda);
            return PartialView(scheda);
        }

        [HttpPost]
        public ActionResult Dettaglio_Obiettivo(int idScheda, string tipo, int idOb)
        {
            MboObiettivo obiettivo = MboManager.GetObiettivo(idScheda, tipo, idOb);
            return PartialView(obiettivo);
        }

        public ActionResult Widget_Ricerca()
        {
            return PartialView(new MboRicerca());
        }

        [HttpPost]
        public ActionResult Save_Obiettivo(MboObiettivo model)
        {
            if (MboManager.SaveObiettivo(model, out string errorMsg))
                return Content("OK");
            else
                return Content(errorMsg);
        }
        [HttpPost]
        public ActionResult Elimina_Obiettivo(int idOb)
        {
            if (MboManager.EliminaObiettivo(idOb, out string errorMsg))
                return Content("OK");
            else
                return Content(errorMsg);
        }

        [HttpPost]
        public ActionResult Annulla_Obiettivo(int idOb, int idScheda, bool annulla, string nota)
        {
            if (MboManager.ToggleAnnullaObiettivo(idOb, idScheda, annulla, nota, out string errorMsg))
                return Content("OK");
            else
                return Content(errorMsg);
        }

        [HttpPost]
        public ActionResult Save_ObiettiviPerc(int[] ids, decimal[] values)
        {
            if (MboManager.SaveObiettiviPerc(ids, values, out string errorMsg))
                return Content("OK");
            else
                return Content(errorMsg);
        }

        [HttpPost]
        public ActionResult Save_ConsObiettiviPerc(int[] ids, decimal?[] values)
        {
            if (MboManager.SaveConsObiettiviPerc(ids, values, out string errorMsg))
                return Content("OK");
            else
                return Content(errorMsg);
        }

        [HttpPost]
        public ActionResult Scheda_ConfermaAssegnazione(int idScheda)
        {
            if (MboManager.SaveAssegnazione(idScheda, true, out string errorMsg))
                return Content("OK");
            else
                return Content(errorMsg);
        }

        [HttpPost]
        public ActionResult Scheda_ConfermaConsuntivazione(int idScheda)
        {
            if (MboManager.SaveConsuntivazione(idScheda, out string errorMsg))
                return Content("OK");
            else
                return Content(errorMsg);
        }

        [HttpPost]
        public ActionResult Save_ValutazioneDirettore(int idScheda, bool parere, string nota="")
        {
            if (MboManager.SaveValutazioneDirettore(idScheda, parere, nota, out string errorMsg))
                return Content("OK");
            else
                return Content(errorMsg);
        }


        public static List<SelectListItem> GetStati(bool addDefault = false)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            if (addDefault)
                result.Add(new SelectListItem() { Value = "0", Text = "Seleziona uno stato", Selected = true });
            result.AddRange(MboManager.GetStati().Select(x => new SelectListItem() { Value = x.ID_STATO.ToString(), Text = x.DES_DESCRIZIONE }));
            return result;
        }

        public ActionResult Widget_allegati(int idScheda, int idTipologia, bool addNew=false)
        {
            var db = new IncentiviEntities();
            List<XR_MBO_ALLEGATI> Allegati = MboManager.InternalGetAllegati(idScheda, idTipologia, db, addNew);

            return View("widget_allegati", Allegati);
        }
        
        public ActionResult GetDoc(int idDoc)
        {
            using (IncentiviEntities db = new IncentiviEntities())
            {
                XR_MBO_ALLEGATI doc = db.XR_MBO_ALLEGATI.Find(idDoc);
                if (doc == null)
                    return View("~/Views/Shared/404.cshtml");
                else
                    return File(doc.OBJ_OBJECT, doc.CONTENT_TYPE);//, doc.NME_FILENAME);
            }
        }

        public ActionResult GetSchedaPDF(int idScheda)
        {
            MemoryStream ms = null;
            string title = "";
            if (MboManager.CreaPdfScheda(idScheda, out title, out ms))
                return File(ms, "application/pdf", title);
            else
                return View("~/Views/Shared/404.cshtml");
        }
    }
}
