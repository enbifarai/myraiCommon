using myRaiCommonManager;
using myRaiDataTalentia;
using myRaiData.Incentivi;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace myRaiGestionale.Controllers
{
    public class AbilitazioniController : BaseCommonController
    {
        //
        // GET: /Abilitazioni/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ElencoProfili()
        {
            List<XR_HRIS_ABIL_PROFILO> model = AbilitazioniManager.GetProfili();
            return View(model);
        }
        public ActionResult ElencoFunzioni()
        {
            List<XR_HRIS_ABIL_FUNZIONE> model = AbilitazioniManager.GetFunzioni();
            return View(model);
        }

        [HttpPost]
        public ActionResult Modal_Funzione(int id)
        {
            XR_HRIS_ABIL_FUNZIONE model = AbilitazioniManager.GetFunzione(id);
            return View(model);
        }
        [HttpPost]
        public ActionResult Save_Funzione(XR_HRIS_ABIL_FUNZIONE funzione)
        {
            bool esito = AbilitazioniManager.SaveFunzione(funzione, out int idFunz, out string errorMsg);
            return new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = esito, idFunz = idFunz, message = errorMsg }
            };
        }

        [HttpPost]
        public ActionResult Modal_Sottofunzione(int idFunz, int idSubFunz)
        {
            XR_HRIS_ABIL_SUBFUNZIONE model = AbilitazioniManager.GetSottofunzione(idFunz, idSubFunz);
            return View(model);
        }
        [HttpPost]
        public ActionResult Save_sottofunzione(XR_HRIS_ABIL_SUBFUNZIONE model)
        {
            bool esito = AbilitazioniManager.SaveSottofunzione(model, out int idSubFunz, out string errorMsg);
            return new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = esito, idSubFunz = idSubFunz, message = errorMsg }
            };
        }
        [HttpPost]
        public ActionResult Delete_Sottofunzione(int idSubFunz)
        {
            bool esito = AbilitazioniManager.DeleteSottofunzione(idSubFunz, out string errorMsg);
            return Content(esito ? "OK" : errorMsg);
        }

        [HttpPost]
        public ActionResult Modal_AbilPers(int idAbil, int? idSubFunz, int? idProfilo, int? idModello, string matricola)
        {
            XR_HRIS_ABIL model = AbilitazioniManager.GetAbilPers(idAbil, idSubFunz, idProfilo, idModello, matricola);
            return View("Modal_Abilitazione", model);
        }
        [HttpPost]
        public ActionResult Save_AbilPers(AbilAbilitazione model)
        {
            bool esito = AbilitazioniManager.SaveAbilPers(model, out int idAbil, out string errorMsg);
            return new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = esito, idAbil = idAbil, message = errorMsg }
            };
        }
        [HttpPost]
        public ActionResult Delete_Abilitazione(int idAbil, int? idModello)
        {
            bool esito = AbilitazioniManager.DeleteAbilitazione(idAbil, idModello, out string errorMsg);
            return Content(esito ? "OK" : errorMsg);
        }

        [HttpPost]
        public ActionResult Modal_Profilo(int id)
        {
            XR_HRIS_ABIL_PROFILO model = AbilitazioniManager.GetProfilo(id);
            return View(model);
        }
        [HttpPost]
        public ActionResult Save_Profilo(XR_HRIS_ABIL_PROFILO profilo)
        {
            bool esito = AbilitazioniManager.SaveProfilo(profilo, out int idProfilo, out string errorMsg);
            return new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = esito, idProfilo = idProfilo, message = errorMsg }
            };
        }
        [HttpPost]
        public ActionResult Save_ProfiloAssoc(int idProfilo, int[] subfunc, int[] profili)
        {
            List<int> subList = new List<int>();
            List<int> prof = new List<int>();
            if (subfunc != null)
                subList.AddRange(subfunc);
            if (profili != null)
                prof.AddRange(profili);

            bool esito = AbilitazioniManager.SaveProfiloAssoc(idProfilo, subList, prof, out string errorMsg);
            return new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = esito, message = errorMsg }
            };
        }

        [HttpPost]
        public ActionResult Modal_Persona(string matricola)
        {
            var model = AbilitazioniManager.GetPersAbil(matricola);
            return View("Modal_Persona", model);
        }

        public ActionResult ElencoRegoleMenu()
        {
            List<RegolaVoceMenu> model = AbilitazioniManager.GetRegole();
            return View("ElencoRegole",model);
        }
        [HttpPost]
        public ActionResult Modal_Regola(int idRegola)
        {
            RegolaVoceMenu model = AbilitazioniManager.GetRegola(idRegola);
            return View("Modal_Regola", model);
        }
        [HttpPost]
        public ActionResult Save_Regola(RegolaVoceMenu model)
        {
            bool esito = AbilitazioniManager.SaveRegola(model, out int idRegola, out string errorMsg);
            return new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = esito, idRegola = idRegola, message = errorMsg }
            };
        }
        [HttpPost]
        public ActionResult Delete_Regola(int idRegola)
        {
            bool esito = AbilitazioniManager.DeleteRegola(idRegola, out string errorMsg);
            return Content(esito ? "OK" : errorMsg);
        }

        public ActionResult ElencoModelli()
        {
            List<XR_HRIS_ABIL_MODELLO> model = AbilitazioniManager.GetModelli();
            return View("ElencoModelli", model);
        }
        [HttpPost]
        public ActionResult Modal_Modello(int idModello)
        {
            AbilModello model = AbilitazioniManager.GetModello(idModello);
            return View("Modal_Modello", model);
        }
        [HttpPost]
        public ActionResult Save_Modello(AbilModello model)
        {
            bool esito = AbilitazioniManager.SaveModello(model, out int idModello, out string errorMsg);
            return new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = esito, idModello = idModello, message = errorMsg }
            };
        }
    }
}
