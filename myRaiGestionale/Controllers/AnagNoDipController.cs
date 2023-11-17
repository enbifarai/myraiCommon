using myRaiCommonModel;
using myRaiCommonManager;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using myRaiGestionale.Services;
using myRaiData.Incentivi;
using myRaiDataTalentia;

namespace myRaiGestionale.Controllers
{
    public class AnagNoDipController : BaseCommonController
    {
        //
        // GET: /AnagNoDip/
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            SessionHelper.Set("GEST_SECTION", "PERSONE");
            base.OnActionExecuting(filterContext);
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LoadFromDB2()
        {
            AnagraficaNoDipManager.LoadFromDB2();
            return Content("OK");
        }

        public ActionResult Elenco_Anagrafiche(AnagNoDipRicerca ricerca = null)
        {
            if (ricerca == null)
                ricerca = new AnagNoDipRicerca();

            List<AnagraficaNoDipModel> model = AnagraficaNoDipManager.GetAnagrafiche(ricerca);
            return View(model);
        }
        public ActionResult Modal_Anagrafica(int idAnag, DateTime? dtaInizio)
        {
            AnagraficaNoDipModel model = AnagraficaNoDipManager.GetAnagrafica(idAnag, dtaInizio);
            return View(model);
        }
        public ActionResult Save_Anagrafica(AnagraficaNoDipModel model)
        {
            bool esito = AnagraficaNoDipManager.Save_Anagrafica(model, out int idModel, out string matricola, out string errorMsg);
            return new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = esito, idModel = idModel, message = errorMsg, matricola = matricola }
            };
        }
        public ActionResult Delete_Anagrafica(int idAnag, DateTime dataInizio)
        {
            if (!AnagraficaNoDipManager.EnabledToSubFunc(CommonHelper.GetCurrentUserMatricola(), "NDIADM"))
                return new RedirectResult("/Home/notAuth");

            bool esito = AnagraficaNoDipManager.Delete_Anagrafica(idAnag, dataInizio, out string errorMsg);
            return new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = esito, message = errorMsg }
            };
        }
        [HttpPost]
        public ActionResult Remove_Lock(int idAnag, bool force=false)
        {
            AnagraficaNoDipManager.RilasciaAnagrafica(idAnag, force);
            return Content("OK");
        }
        [HttpPost]
        public ActionResult Elenco_Lock()
        {
            var list = AnagraficaNoDipManager.GetLock();
            return new JsonResult() { Data = list, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult Modal_Rapporto(int idAnag, string codTipoSogg, string codImpresa, DateTime? dataInizio)
        {
            AnagNoDipRapportoModel model = AnagraficaNoDipManager.GetRapporto(idAnag, codTipoSogg, codImpresa, dataInizio);
            return View(model);
        }

        public ActionResult Elenco_Rapporti(int idAnag)
        {
            var model = AnagraficaNoDipManager.GetRapporti(idAnag);
            return View(model);
        }

        public ActionResult Save_Rapporto(AnagNoDipRapportoModel model)
        {
            bool esito = AnagraficaNoDipManager.Save_Rapporto(model, out string errorMsg);
            return new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = esito, message = errorMsg }
            };
        }
        public ActionResult Delete_Rapporto(int idAnag, string codTipoSogg, string codImpresa, DateTime dataInizio)
        {
            if (!AnagraficaNoDipManager.EnabledToSubFunc(CommonHelper.GetCurrentUserMatricola(), "NDIADM"))
                return new RedirectResult("/Home/notAuth");

            bool esito = AnagraficaNoDipManager.Delete_Rapporto(idAnag, codTipoSogg, codImpresa, dataInizio, out string errorMsg);
            return new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = esito, message = errorMsg }
            };
        }

        public ActionResult Modal_Indirizzo(int idAnag, DateTime? dataInizio)
        {
            AnagNoDipIndirizziModel model = AnagraficaNoDipManager.GetIndirizzo(idAnag, dataInizio);
            return View(model);
        }

        public ActionResult Save_Indirizzo(AnagNoDipIndirizziModel model)
        {
            bool esito = AnagraficaNoDipManager.Save_Indirizzo(model, out string errorMsg);
            return new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = esito, message = errorMsg }
            };
        }
        public ActionResult Delete_Indirizzo(int idAnag, DateTime dataInizio)
        {
            if (!AnagraficaNoDipManager.EnabledToSubFunc(CommonHelper.GetCurrentUserMatricola(), "NDIADM"))
                return new RedirectResult("/Home/notAuth");

            bool esito = AnagraficaNoDipManager.Delete_Indirizzo(idAnag, dataInizio, out string errorMsg);
            return new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = esito, message = errorMsg }
            };
        }
        public ActionResult Storico_Indirizzi(int idAnag)
        {
            List<AnagNoDipIndirizziModel> model = AnagraficaNoDipManager.GetIndirizzi(idAnag);
            return View(model);
        }

        public ActionResult Modal_Iban(int idAnag, int idIban)
        {
            AnagNoDipIbanModel model = AnagraficaNoDipManager.GetIban(idAnag, idIban);
            return View(model);
        }
        public ActionResult Save_Iban(AnagNoDipIbanModel model)
        {
            bool esito = AnagraficaNoDipManager.Save_Iban(model, out string errorMsg);
            return new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = esito, message = errorMsg }
            };
        }
        public ActionResult Delete_Iban(int idAnag, int idIban)
        {
            if (!AnagraficaNoDipManager.EnabledToSubFunc(CommonHelper.GetCurrentUserMatricola(), "NDIADM"))
                return new RedirectResult("/Home/notAuth");

            bool esito = AnagraficaNoDipManager.Delete_Iban(idAnag, idIban, out string errorMsg);
            return new JsonResult()
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = esito, message = errorMsg }
            };
        }
        public ActionResult Storico_Iban(int idAnag)
        {
            List<AnagNoDipIbanModel> model = AnagraficaNoDipManager.GetElencoIban(idAnag);
            return View(model);
        }


        [HttpPost]
        public ActionResult CalcolaCF(string cognome, string nome, string sesso, string dataNascita, string luogoNascita)
        {
            string cf = "";
            object result;
            try
            {
                DateTime date = DateTime.Parse(dataNascita);
                var gen = Convert.ToChar(sesso);
                cf = GeneraCodiceFiscaleServizio.CalcolaCodiceFiscale(nome.ToUpper(), cognome.ToUpper(), date, gen, luogoNascita);
                result = new { cf = cf, Esito = true };
            }
            catch (Exception ex)
            {
                result = new { cf = "", Esito = false, Message = ex.Message };
            }
            return new JsonResult { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = result };
        }

        [HttpPost]
        public ActionResult RicercaMatricola(string matricola)
        {
            var db = new IncentiviEntities();
            var sint = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == matricola);

            if (sint != null)
            {
                return new JsonResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new
                    {
                        found = true,
                        message = "",
                        matricola = matricola,
                        idpersona = sint.ID_PERSONA,
                        nominativo = sint.Nominativo().ToUpper(),
                        cf = sint.CSF_CFSPERSONA,
                        cognome = sint.DES_COGNOMEPERS,
                        nome = sint.DES_NOMEPERS,
                        dataNascita = sint.DTA_NASCITAPERS.Value.ToString("dd/MM/yyyy"),
                        sesso = sint.COD_SESSO,
                        codComuneNascita = sint.COD_CITTANASC
                    }
                };
            }
            else
            {
                return new JsonResult()
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { found = false, message = "Non trovato", matricola = "", nominativo = "", cf = "" }
                };
            }
        }

    }
}
