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
    public class DelegheController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            SessionHelper.Set("GEST_SECTION", "GESTIONE");
            base.OnActionExecuting(filterContext);
        }

        private void SetFiltroCodiceFunzione(string area, List<string> COD_FUNZIONI = null)
        {
            string matricola = UtenteHelper.Matricola();

            SessionHelper.Set(matricola + "DELEGHE_AREA_UTILIZZO", area);

            SessionHelper.Set(matricola + "_" + area + "_" + "DELEGHE_SOTTOFUNZIONI_VISIBILI", null);

            if (COD_FUNZIONI != null && COD_FUNZIONI.Any())
            {
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    var funzioni = db.XR_HRIS_ABIL_FUNZIONE.Where(w => COD_FUNZIONI.Contains(w.COD_FUNZIONE)).ToList();

                    if (funzioni != null && funzioni.Any())
                    {
                        // recupero tutti gli ID sotto funzione

                        List<int> idFunzioni = new List<int>();
                        idFunzioni.AddRange(funzioni.Select(w => w.ID_FUNZIONE).ToList());

                        var sottoFunzioni = db.XR_HRIS_ABIL_SUBFUNZIONE.Where(w => idFunzioni.Contains(w.ID_FUNZIONE)).ToList();

                        if (sottoFunzioni != null && sottoFunzioni.Any())
                        {
                            List<int> idSottoFunzioni = new List<int>();
                            idSottoFunzioni.AddRange(sottoFunzioni.Select(w => w.ID_SUBFUNZ).ToList());
                            SessionHelper.Set(matricola + "_" + area + "_" + "DELEGHE_SOTTOFUNZIONI_VISIBILI", idSottoFunzioni);
                        }
                    }
                }
            }
        }

        public ActionResult Index(string area = "", List<string> COD_FUNZIONI = null)
        {
            SetFiltroCodiceFunzione(area, COD_FUNZIONI);
            DelegheVM model = new DelegheVM();
            model.MatricolaCorrente = UtenteHelper.Matricola();
            model.DelegheConcesse = null;
            model.DelegheRicevute = null;

            SessionHelper.Set(model.MatricolaCorrente + "DELEGHE_SETSCELTE_ABILITAZIONI", null);

            return View("~/Views/Deleghe/Index.cshtml", model);
        }

        [HttpGet]
        public ActionResult GetContentDelegheConcesse(string matricola)
        {
            if (!String.IsNullOrEmpty(matricola))
            {
                matricola = UtenteHelper.Matricola();
            }
            List<AbilitazioniPersExt> model = new List<AbilitazioniPersExt>();

            List<int> idSottoFunzioni = new List<int>();

            string area = (string)SessionHelper.Get(matricola + "DELEGHE_AREA_UTILIZZO");

            idSottoFunzioni = (List<int>)SessionHelper.Get(matricola + "_" + area + "_" + "DELEGHE_SOTTOFUNZIONI_VISIBILI");

            model = DelegheManager.GetDelegheConcesse(matricola, idSottoFunzioni);

            return View("~/Views/Deleghe/subpartial/deleghe_concesse.cshtml", model);
        }

        [HttpGet]
        public ActionResult GetContentDelegheRicevute(string matricola)
        {
            if (!String.IsNullOrEmpty(matricola))
            {
                matricola = UtenteHelper.Matricola();
            }
            List<AbilitazioniPersExt> model = new List<AbilitazioniPersExt>();

            List<int> idSottoFunzioni = new List<int>();

            string area = (string)SessionHelper.Get(matricola + "DELEGHE_AREA_UTILIZZO");

            idSottoFunzioni = (List<int>)SessionHelper.Get(matricola + "_" + area + "_" + "DELEGHE_SOTTOFUNZIONI_VISIBILI");

            model = DelegheManager.GetDelegheRicevute(matricola, idSottoFunzioni);

            return View("~/Views/Deleghe/subpartial/deleghe_ricevute.cshtml", model);
        }

        [HttpPost]
        public ActionResult GetModalCreaDelega(string m, string area = "", List<string> COD_FUNZIONI = null)
        {
            SetFiltroCodiceFunzione(area, COD_FUNZIONI);
            DelegaModelVM model = new DelegaModelVM();
            model.MatricolaDelegante = m;
            var abilitazioniUtente = AbilitazioniManager.GetPersAbil(m);

            List<int> idSottoFunzioni = new List<int>();
            idSottoFunzioni = (List<int>)SessionHelper.Get(model.MatricolaDelegante + "_" + area + "_" + "DELEGHE_SOTTOFUNZIONI_VISIBILI");

            if (idSottoFunzioni != null && idSottoFunzioni.Any())
            {
                model.Abilitazioni = abilitazioniUtente.Abilitazioni.Where(w => w.ID_SUBFUNZ != null && idSottoFunzioni.Contains(w.ID_SUBFUNZ.Value)).ToList();
            }
            else
            {
                model.Abilitazioni = abilitazioniUtente.Abilitazioni;
            }

            SessionHelper.Set(model.MatricolaDelegante + "DELEGHE_SETSCELTE_ABILITAZIONI", null);

            return View("~/Views/Deleghe/CreaDelega.cshtml", model);
        }

        public ActionResult GetDestinatario(string filter, string value)
        {
            return Json(DelegheManager.GetDestinatario(filter, value), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult SetScelte(string matricolaDelegato, string nomeDelega, DateTime dataDal, DateTime dataA, List<int> abilitazioni)
        {
            DateTime adesso = DateTime.Now;
            DelegaModelVM model = new DelegaModelVM();

            try
            {
                string matricola = UtenteHelper.Matricola();

                var abilitazioniUtente = AbilitazioniManager.GetPersAbil(matricola);
                var filtrate = abilitazioniUtente.Abilitazioni.Where(w => abilitazioni.Contains(w.ID_ABIL)).ToList();

                if (filtrate == null || !filtrate.Any())
                {
                    throw new Exception("Impossibile reperire le abilitazioni");
                }

                model.MatricolaDelegante = matricola;
                model.MatricolaDelegato = matricolaDelegato;
                model.NomeDelega = nomeDelega;
                model.DataInizioDelega = dataDal;
                model.DataFineDelega = dataA;
                model.DataCreazioneDelega = adesso;
                model.NominativoDelegante = CezanneHelper.GetNominativoByMatricola(matricola);
                model.NominativoDelegato = CezanneHelper.GetNominativoByMatricola(matricolaDelegato);
                model.Abilitazioni = filtrate;

                SessionHelper.Set(matricola + "DELEGHE_SETSCELTE_ABILITAZIONI", model);
            }
            catch (Exception ex)
            {
                HandleErrorInfo eModel = new HandleErrorInfo(ex, "Deleghe", "SetScelta");

                return View("~/Views/Shared/Error.cshtml", eModel);
            }

            return View("~/Views/Deleghe/subpartial/_riepilogoCreaDelega.cshtml", model);
        }

        [HttpPost]
        public JsonResult ConfermaCreazioneDelega(string matricolaDelegato, string nomeDelega, DateTime dataDal, DateTime dataA, List<int> abilitazioni)
        {
            DateTime adesso = DateTime.Now;
            DelegaModelVM model = new DelegaModelVM();
            DelegheResult result = new DelegheResult();

            try
            {
                nomeDelega = CezanneHelper.GetNominativoByMatricola(matricolaDelegato).Trim() + "_" + adesso.ToString("dd-MM-yyyy HH-mm-ss");

                string matricola = UtenteHelper.Matricola();

                var datiDaSessione = (DelegaModelVM)SessionHelper.Get(matricola + "DELEGHE_SETSCELTE_ABILITAZIONI");

                if (datiDaSessione == null)
                {
                    var abilitazioniUtente = AbilitazioniManager.GetPersAbil(matricola);
                    var filtrate = abilitazioniUtente.Abilitazioni.Where(w => abilitazioni.Contains(w.ID_ABIL)).ToList();

                    if (filtrate == null || !filtrate.Any())
                    {
                        throw new Exception("Impossibile reperire le abilitazioni");
                    }

                    model.MatricolaDelegante = matricola;
                    model.MatricolaDelegato = matricolaDelegato;
                    model.NomeDelega = nomeDelega;
                    model.DataInizioDelega = dataDal;
                    model.DataFineDelega = dataA;
                    model.DataCreazioneDelega = adesso;
                    model.NominativoDelegante = CezanneHelper.GetNominativoByMatricola(matricola);
                    model.NominativoDelegato = CezanneHelper.GetNominativoByMatricola(matricolaDelegato);
                    model.Abilitazioni = filtrate;

                    SessionHelper.Set(matricola + "DELEGHE_SETSCELTE_ABILITAZIONI", model);
                }
                else
                {
                    model = datiDaSessione;
                }

                result = DelegheManager.CreaDelega(model);
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetModalDettaglioDelega(int idDelega)
        {
            DelegaModelVM model = new DelegaModelVM();

            DelegheResult info = DelegheManager.GetDelega(idDelega);

            if (!info.Esito)
                throw new Exception(info.Errore);

            model = (DelegaModelVM)info.Obj;

            return View("~/Views/Deleghe/DettaglioDelega.cshtml", model);
        }

        [HttpPost]
        public JsonResult EliminaDelega(int idDelega)
        {
            DelegheResult result = new DelegheResult();

            try
            {
                result = DelegheManager.DeleteDelega(idDelega);
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult EsercitaDelega(int idDelega)
        {
            DelegheResult result = new DelegheResult();

            try
            {
                result = DelegheManager.Esercita(idDelega, true);
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult FermaEsercizioDelega(int idDelega)
        {
            DelegheResult result = new DelegheResult();

            try
            {
                result = DelegheManager.Esercita(idDelega, false);
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ElenchiDeleghe(string area = "", List<string> COD_FUNZIONI = null)
        {
            SetFiltroCodiceFunzione(area, COD_FUNZIONI);
            DelegheVM model = new DelegheVM();
            model.MatricolaCorrente = UtenteHelper.Matricola();
            model.DelegheConcesse = null;
            model.DelegheRicevute = null;

            SessionHelper.Set(model.MatricolaCorrente + "DELEGHE_SETSCELTE_ABILITAZIONI", null);

            return View("~/Views/Deleghe/ElenchiDeleghe.cshtml", model);
        }

        [HttpPost]
        public ActionResult GetModalCreaDelegaNoWizard(string m, string area = "", List<string> COD_FUNZIONI = null)
        {
            SetFiltroCodiceFunzione(area, COD_FUNZIONI);
            DelegaModelVM model = new DelegaModelVM();
            model.MatricolaDelegante = m;
            var abilitazioniUtente = AbilitazioniManager.GetPersAbil(m);

            List<int> idSottoFunzioni = new List<int>();
            idSottoFunzioni = (List<int>)SessionHelper.Get(model.MatricolaDelegante + "_" + area + "_" + "DELEGHE_SOTTOFUNZIONI_VISIBILI");

            if (idSottoFunzioni != null && idSottoFunzioni.Any())
            {
                model.Abilitazioni = abilitazioniUtente.Abilitazioni.Where(w => w.ID_SUBFUNZ != null && idSottoFunzioni.Contains(w.ID_SUBFUNZ.Value)).ToList();
            }
            else
            {
                model.Abilitazioni = abilitazioniUtente.Abilitazioni;
            }

            SessionHelper.Set(model.MatricolaDelegante + "DELEGHE_SETSCELTE_ABILITAZIONI", null);

            return View("~/Views/Deleghe/CreaDelegaNoWizard.cshtml", model);
        }

        [HttpPost]
        public JsonResult AggiornaConteggioDeleghe()
        {
            string matricola = UtenteHelper.Matricola();

            DelegheResult result = new DelegheResult();

            try
            {
                int nDeleghe = DelegheManager.GetNumeroDelegheConcesse(matricola);

                result.Esito = true;
                result.Errore = String.Empty;
                result.Obj = nDeleghe;
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
