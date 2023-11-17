using myRaiHelper;
using myRaiCommonModel.Gestionale;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using myRaiCommonManager;
using myRaiData.Incentivi;
using System.IO;
using myRai.DataAccess;
using ClosedXML.Excel;

namespace myRaiGestionale.Controllers
{
    public class MboController : BaseCommonController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            SessionHelper.Set("GEST_SECTION", "GESTIONE");
            base.OnActionExecuting(filterContext);
        }

        public ActionResult Index()
        {
            return View();
        }


        public void AggiungiEvaluator(IncentiviEntities db, List<XR_VAL_EVALUATION> ListEvaluation, int idPersonaEvaluator)
        {
            foreach (var eval in ListEvaluation)
            {
                XR_VAL_EVALUATOR e = new XR_VAL_EVALUATOR()
                {
                    COD_TERMID = "::1",
                    COD_USER = "P103650",
                    ID_CAMPAIGN = eval.ID_CAMPAIGN,
                    ID_CAMPAIGN_SHEET = eval.ID_CAMPAIGN_SHEET,
                    ID_PERSONA = idPersonaEvaluator,
                    ID_EVALUATOR = db.XR_VAL_EVALUATOR.GeneraPrimaryKey(),
                    TMS_TIMESTAMP = DateTime.Now,
                    VALID_DTA_INI = DateTime.Now,
                    VALID_DTA_END = null
                };
                db.XR_VAL_EVALUATOR.Add(e);
                eval.ID_EVALUATOR = e.ID_EVALUATOR;
            }
        }

        public void CheckEvaluator(IncentiviEntities db, List<XR_VAL_EVALUATION> ListEvaluation, int idPersonaEvaluator)
        {
            foreach (var eval in ListEvaluation)
            {
                var evaluator = db.XR_VAL_EVALUATOR.Where(x => x.ID_PERSONA == idPersonaEvaluator
                && x.ID_CAMPAIGN== eval.ID_CAMPAIGN
                && x.ID_CAMPAIGN_SHEET== eval.ID_CAMPAIGN_SHEET).FirstOrDefault();
                if (eval.ID_EVALUATOR != evaluator.ID_EVALUATOR)
                {
                    eval.ID_EVALUATOR = evaluator.ID_PERSONA;
                }
            }
        }
        public ActionResult AllineaVal(string matr)
        {
            var db = new IncentiviEntities();
            List< XR_MBO_SCHEDA> schede = db.XR_MBO_SCHEDA.ToList();
            if (!String.IsNullOrWhiteSpace(matr))
            {
                int? idpers = db.SINTESI1.Where(x => x.COD_MATLIBROMAT == matr).Select(x => x.ID_PERSONA).FirstOrDefault();
                if (idpers == null) return Content("NOT FOUND");

                schede = schede.Where(x => x.ID_PERSONA_VALUTATO == idpers).ToList();
            }
            List<string> Report = new List<string>();
            foreach (var scheda in schede)
            {
                if (scheda.ID_PERSONA_RESP == 0) continue;
                var sint = db.SINTESI1.Where(x => x.ID_PERSONA == scheda.ID_PERSONA_VALUTATO).FirstOrDefault();
                //if (sint == null || sint.COD_SERVIZIO != "54") continue;

                XR_VAL_CAMPAIGN_SHEET sheet = db.XR_VAL_CAMPAIGN_SHEET.Where(x => x.ID_OBJ_COLL == scheda.ID_INIZIATIVA).FirstOrDefault();
                if (sheet != null)
                {
                    var evaluation = db.XR_VAL_EVALUATION.Where(x =>
                    x.ID_PERSONA == scheda.ID_PERSONA_VALUTATO && 
                    x.ID_CAMPAIGN == sheet.ID_CAMPAIGN && x.ID_CAMPAIGN_SHEET == sheet.ID_CAMPAIGN_SHEET).FirstOrDefault();

                    if (evaluation != null)
                    {
                        var evaluator = db.XR_VAL_EVALUATOR.Where(x => x.ID_EVALUATOR == evaluation.ID_EVALUATOR
                        && x.ID_CAMPAIGN== sheet.ID_CAMPAIGN && x.ID_CAMPAIGN_SHEET == sheet.ID_CAMPAIGN_SHEET).FirstOrDefault();
                        if (evaluator != null)
                        {
                            if (evaluator.ID_PERSONA != scheda.ID_PERSONA_RESP)
                            {
                                Report.Add("idScheda:" + scheda.ID_SCHEDA +
                                    " : su scheda:" + scheda.ID_PERSONA_RESP + " - su evaluation:" + evaluator.ID_PERSONA);

                                XR_VAL_EVALUATOR EvalCorrispondente = db.XR_VAL_EVALUATOR.Where(x => x.ID_PERSONA == scheda.ID_PERSONA_RESP
                                  && x.ID_CAMPAIGN == sheet.ID_CAMPAIGN && x.ID_CAMPAIGN_SHEET == sheet.ID_CAMPAIGN_SHEET
                                ).FirstOrDefault();
                                if (EvalCorrispondente != null)
                                {
                                    // metti evaluator corretto in EVALUATION
                                    int oldID_Evaluator = evaluation.ID_EVALUATOR;

                                    evaluation.ID_EVALUATOR = EvalCorrispondente.ID_EVALUATOR;
                                    
                                    db.SaveChanges();

                                    Logger.LogAzione(new myRaiData.MyRai_LogAzioni()
                                    {
                                        operazione = "EVALUATION correction",
                                        descrizione_operazione =
                                        "ID_EVALUATION " + evaluation.ID_EVALUATION.ToString() +
                                        " : old ID_EVALUATOR:" + oldID_Evaluator + " - "
                                        + " new ID_EVALUATOR: " + evaluation.ID_EVALUATOR.ToString()
                                    });
                                    Report.Add(" evaluator da corretto nel DB per EVALUATION:" + EvalCorrispondente.ID_EVALUATOR);
                                }

                            }
                            else
                                Report.Add("idScheda:" + scheda.ID_SCHEDA + " - Evaluator coincidente");
                        }
                    }
                }
            }
       
            Report.Add("Terminata");
            return new JsonResult() { JsonRequestBehavior= JsonRequestBehavior.AllowGet,  Data= Report};
        }
        [HttpPost]
        public ActionResult Modal_Iniziativa(int idIniz = 0)
        {
            MboIniziativa iniziativa = MboManager.GetIniziativa(idIniz);

            return PartialView(iniziativa);
        }
        public ActionResult Widget_Iniziative()
        {
            List<MboIniziativa> iniziative = MboManager.GetIniziative();
            return PartialView(iniziative);
        }
        [HttpPost]
        public ActionResult Save_Iniziativa(MboIniziativa iniziativa, MboScheda[] mSchede)
        {
            List<MboScheda> schede = new List<MboScheda>();
            if (mSchede != null)
                schede.AddRange(mSchede);


            if (MboManager.SaveIniziativa(iniziativa, schede, out string errorMsg))
                return Content("OK");
            else
                return Content(errorMsg);
        }
        public ActionResult Get_Elenco_Schede(int idIni, decimal importoTop, decimal importoFull, decimal importoManager, bool forceReload = false)
        {
            List<MboScheda> schede = MboManager.GetSchedeMaster(idIni, importoTop, importoFull, importoManager, forceReload);

            return PartialView("Elenco_schede_master", schede);
        }
        //public ActionResult ExportSchedeMaster()
        //{
        //    List<MboScheda> schede = MboManager.GetSchedeMaster(0,0,0,0, true);

        //    var wb = new XLWorkbook();
        //    var ws = wb.AddWorksheet();

        //    int row = 2;
        //    foreach (var item in schede)
        //    {
        //        ws.Cell(row, 1).SetValue(item.PersValutato.Nominativo);
        //        ws.Cell(row, 2).SetValue(item.PersValutato.Nominativo);
        //        ws.Cell(row, 3).SetValue(item.PersResp);
        //        ws.Cell(row, 4).SetValue(item.PersResp);
        //    }


        //}


        public ActionResult Elenco_Schede(MboRicerca model = null)
        {
            List<MboScheda> schede = MboManager.GetSchede(model);
            return PartialView(schede);
        }
        [HttpPost]
        public ActionResult Modal_Scheda(int idScheda, bool loadIncarichi = true)
        {
            MboScheda scheda = MboManager.GetScheda(idScheda, loadIncarichi);
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
        public ActionResult Save_Tuning(MboScheda model)
        {
            if (MboManager.SaveTuning(model, out string errorMsg))
                return Content("OK");
            else
                return Content(errorMsg);
        }


        [HttpPost]
        public ActionResult Scheda_ConfermaAssegnazione(int idScheda)
        {
            if (MboManager.SaveAssegnazione(idScheda, false, out string errorMsg))
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
        public ActionResult Scheda_AnnullaConsuntivazione(int idScheda)
        {
            if (MboManager.AnnullaConsuntivazione(idScheda, out string errorMsg))
                return Content("OK");
            else
                return Content(errorMsg);
        }

        [HttpPost]
        public ActionResult Save_Convalida(int idScheda, bool parere, string nota = "")
        {
            if (MboManager.Save_Convalida(idScheda, parere, nota, out string errorMsg))
                return Content("OK");
            else
                return Content(errorMsg);
        }

        public ActionResult GetIncarichiResponsabili(int idPers, int? idPersAdd = null)
        {
            List<SelectListItem> result = InternalGetIncarichiResponsabili(idPers, idPersAdd);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public static List<SelectListItem> GetListIncarichiResponsabili(int idPers)
        {
            return InternalGetIncarichiResponsabili(idPers);
        }

        private static List<SelectListItem> InternalGetIncarichiResponsabili(int idPers, int? idPersAdd = null)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            result.Add(new SelectListItem() { Value = "0", Text = "Non assegnato" });
            foreach (var item in MboManager.GetIncarichiResponsabili(idPers, null, idPersAdd))
            {
                result.Add(new SelectListItem()
                {
                    Value = item.Id.ToString(),
                    Text = item.Nominativo
                });
            }

            return result;
        }

        [HttpPost]
        public ActionResult GetSecondoRiporto(int idPers)
        {
            MboManager.GetSecondoRiporto(null, idPers, out int idPersRiporto);
            return Json(idPersRiporto, JsonRequestBehavior.AllowGet);
        }

        public static List<SelectListItem> GetStati(bool addDefault = false)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            if (addDefault)
                result.Add(new SelectListItem() { Value = "0", Text = "Seleziona uno stato", Selected = true });
            result.AddRange(MboManager.GetStati().Select(x => new SelectListItem() { Value = x.ID_STATO.ToString(), Text = x.DES_DESCRIZIONE }));
            return result;
        }
        public static List<SelectListItem> GetResponsabili(bool addDefault = false)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            if (addDefault)
                result.Add(new SelectListItem() { Value = "", Text = "Seleziona un responsabile", Selected = true });
            result.Add(new SelectListItem() { Value = "0", Text = "Non assegnato" });
            result.AddRange(MboManager.GetPersoneResp().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Nominativo }));
            return result;
        }
        public static List<SelectListItem> GetSecondoRiporto(bool addDefault = false)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            if (addDefault)
                result.Add(new SelectListItem() { Value = "", Text = "Seleziona un secondo riporto", Selected = true });
            result.Add(new SelectListItem() { Value = "0", Text = "Non assegnato" });
            result.AddRange(MboManager.GetPersoneSecRiporto().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Nominativo }));
            return result;
        }
        public static List<SelectListItem> GetConsuntivatori(bool addDefault = false)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            if (addDefault)
                result.Add(new SelectListItem() { Value = "", Text = "Seleziona un responsabile", Selected = true });
            result.Add(new SelectListItem() { Value = "0", Text = "Non assegnato" });
            result.AddRange(MboManager.GetPersoneConsuntivatori().Select(x => new SelectListItem() { Value = x.Id.ToString(), Text = x.Nominativo }));
            return result;
        }
        public static List<SelectListItem> GetDirezioni(bool addDefault = false)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            if (addDefault)
                result.Add(new SelectListItem() { Value = "", Text = "Seleziona una direzione", Selected = true });
            result.AddRange(MboManager.GetDirezioniSch());
            return result;
        }

        [HttpPost]
        public ActionResult UploadFile(int idScheda, int idTipologia, HttpPostedFileBase _file, string fileName, string titolo, string desc)
        {
            IncentiviEntities db = new IncentiviEntities();

            XR_MBO_ALLEGATI doc = new XR_MBO_ALLEGATI();
            doc.ID_SCHEDA = idScheda;
            doc.ID_TIPOLOGIA = idTipologia;
            doc.NME_FILENAME = fileName;
            doc.COD_TITLE = titolo;
            doc.DES_ALLEGATO = desc;
            doc.CONTENT_TYPE = _file.ContentType;
            using (MemoryStream ms = new MemoryStream())
            {
                _file.InputStream.CopyTo(ms);
                doc.OBJ_OBJECT = ms.ToArray();
            }
            doc.NMB_SIZE = _file.ContentLength;
            doc.COD_USER = CommonHelper.GetCurrentUserMatricola();
            doc.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
            doc.TMS_TIMESTAMP = DateTime.Now;
            doc.IND_TEMP = true;
            db.XR_MBO_ALLEGATI.Add(doc);
            db.SaveChanges();

            return Content("OK");
        }
        public ActionResult Widget_allegati(int idScheda, int idTipologia, bool addNew = false)
        {
            var db = new IncentiviEntities();
            List<XR_MBO_ALLEGATI> Allegati = MboManager.InternalGetAllegati(idScheda, idTipologia, db, addNew);

            return View("widget_allegati", Allegati);
        }

        public ActionResult EliminaGenericDoc(int idDoc)
        {
            IncentiviEntities db = new IncentiviEntities();

            var doc = db.XR_MBO_ALLEGATI.Find(idDoc);
            db.XR_MBO_ALLEGATI.Remove(doc);

            string result = "";
            if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                result = "OK";
            else
                result = "Errore durante il salvataggio";

            return Content(result);
        }

        public ActionResult InvioARuo()
        {
            var tmp = MboManager.InvioARuo(out string message);
            return Content(message);
        }

        public ActionResult Ricerca(string matricola, string nominativo)
        {
            List<myRaiCommonModel.CercaDipendentiItem> elenco = new List<myRaiCommonModel.CercaDipendentiItem>(); //RicercaController.CercaDipendenti(nominativo, matricola);

            IncentiviEntities db = new IncentiviEntities();
            IQueryable<SINTESI1> list = db.SINTESI1.Where(x => x.COD_QUALIFICA == "A01" && (x.DTA_FINE_CR == null || x.DTA_FINE_CR > DateTime.Now));
            if (!string.IsNullOrEmpty(matricola))
                list = list.Where(w => matricola.Contains(w.COD_MATLIBROMAT));

            if (!string.IsNullOrEmpty(nominativo))
                list = list.Where(w => (w.DES_NOMEPERS + " " + w.DES_COGNOMEPERS).ToUpper().Contains(nominativo.ToUpper())
                                       || (w.DES_COGNOMEPERS + " " + w.DES_NOMEPERS).ToUpper().Contains(nominativo.ToUpper()));

            elenco.AddRange(list.OrderBy(x => x.DES_COGNOMEPERS + " " + x.DES_NOMEPERS).Select(s => new myRaiCommonModel.CercaDipendentiItem()
            {
                MATRICOLA = s.COD_MATLIBROMAT,
                NOME = s.DES_NOMEPERS,
                COGNOME = s.DES_COGNOMEPERS,
                SECONDO_COGNOME = s.DES_SECCOGNOME,
                DATA_ASSUNZIONE = s.DTA_INIZIO_CR,
                CONTRATTO = s.DES_TPCNTR,
                ID_PERSONA = s.ID_PERSONA,
                COD_SEDE = s.COD_SEDE,
                DES_SEDE = s.DES_SEDE,
                COD_SERVIZIO = s.COD_SERVIZIO,
                DES_SERVIZIO = s.DES_SERVIZIO,
                SmartWorker = false
            }));

            return PartialView("Elenco_Matricole", elenco);
        }

        public ActionResult GetSchedaPDF(int idScheda)
        {
            MemoryStream ms = null;
            string title = "";
            if (MboManager.CreaPdfScheda(idScheda, out title, out ms))
            {
                Response.AddHeader("Content-Disposition", String.Format("inline; filename={0}", title));
                return File(ms, "application/pdf");
                //return File(ms, "application/pdf", title);
            }
            else
                return View("~/Views/Shared/404.cshtml");
        }

        public ActionResult GetReportIniziativa(int idIniziativa, string tipo)
        {
            MemoryStream ms = null;
            string title = "";
            if (MboManager.CreaReportIniziativa(idIniziativa, tipo, out title, out ms, out string contentType))
            {
                Response.AddHeader("Content-Disposition", String.Format("inline; filename={0}", title));
                return File(ms, contentType);
                //return File(ms, "application/pdf", title);
            }
            else
                return View("~/Views/Shared/404.cshtml");
        }

        [HttpPost]
        public ActionResult InviaReminderScheda(int idScheda)
        {
            if (MboManager.InviaReminderCompilazione(idScheda, 0, 0, out string errorMsg))
                return Content("OK");
            else
                return Content(errorMsg);
        }

        [HttpPost]
        public ActionResult InviaReminderIniziativa(int idIniziativa)
        {
            if (MboManager.InviaReminderCompilazione(0, idIniziativa, 0, out string errorMsg))
                return Content("OK");
            else
                return Content(errorMsg);
        }

        [HttpPost]
        public ActionResult Modal_Reminder()
        {
            MboManager.AnyIncomplete(true, out int numSchede, out MboReminder model);
            return PartialView("Modal_Reminder", model);
        }

        [HttpPost]
        public ActionResult InviaReminderPersona(int[] iniziativa, int[] persone)
        {
            if (MboManager.InviaReminder("ReminderCompilazione", iniziativa, persone, out string errorMsg))
                return Content("OK");
            else
                return Content(errorMsg);
        }
    }
}
