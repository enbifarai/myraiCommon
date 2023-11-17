using ClosedXML.Excel;
using myRai.DataAccess;
using myRaiCommonManager;
using myRaiCommonModel.Gestionale;
using myRaiData.Incentivi;
using myRaiHelper;
using myRaiServiceHub.it.rai.servizi.sendmail;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace myRaiGestionale.Controllers
{
    public class ValutazioniController : BaseCommonController
    {
        //
        // GET: /Valutazioni/
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            SessionHelper.Set("GEST_SECTION", "GESTIONE");

            base.OnActionExecuting(filterContext);
        }

        public ActionResult CaricamentoDati()
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                //ValutazioniManager.CaricamentoDati("ADMIN", "BATCHSESSION");
                //ValutazioniManager.CreazioneNuovaScheda();
                ValutazioniManager.CreazioneSchedaMBO();
            }
            return RedirectToAction("Index");
        }

        public ActionResult PuliziaDB()
        {
            if (CommonHelper.GetCurrentUserMatricola() == "103650" && System.Diagnostics.Debugger.IsAttached)
            {
                IncentiviEntities db = new IncentiviEntities();
                db.XR_VAL_OPER_STATE.Clear();
                db.XR_VAL_EVAL_RATING.Clear();
                //db.XR_VAL_EVAL_RATING_OWNER.Clear();
                db.XR_VAL_EVALUATION_NOTE.Clear();
                db.XR_VAL_EVALUATION.Clear();
                db.XR_VAL_DELEGATION_PERS.Clear();
                db.XR_VAL_DELEGATION.Clear();
                db.XR_VAL_EVALUATOR_EXT.Clear();
                db.XR_VAL_EVALUATOR.Clear();
                //db.XR_VAL_QUESTION_ANSWER.Clear();
                //db.XR_VAL_QUESTION_DISPLAY.Clear();
                //db.XR_VAL_EVAL_SHEET_QST.Clear();
                //db.XR_VAL_QUESTION.Clear();
                //db.XR_VAL_QUESTION_TYPE.Clear();
                //db.XR_VAL_QUESTION_GROUP.Clear();
                db.XR_VAL_CAMPAIGN_SHEET_UORG.Clear();
                db.XR_VAL_CAMPAIGN_SHEET_SER.Clear();
                db.XR_VAL_CAMPAIGN_SHEET_QUAL.Clear();
                db.XR_VAL_CAMPAIGN_SHEET.Clear();
                //db.XR_VAL_EVAL_SHEET.Clear();
                db.XR_VAL_CAMPAIGN.Clear();

                db.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Index()
        {
            ValutazioniPermission permission = ValutazioniManager.GetPermission(CommonHelper.GetCurrentUserMatricola());

            return View(permission);
        }

        public ActionResult Widget_RicercaValutazione()
        {
            return View("subpartial/Widget_RicercaValutazione", new myRaiCommonModel.Gestionale.RicercaValutazione() { BoxDest = "panelElencoValutazioni" });
        }

        #region CambiaValutatore
        public ActionResult Modal_CambioValutatore(int idEvaluation)
        {
            var model = ValutazioniManager.GetValutazione(idEvaluation, true);
            return View("subpartial/Modal_CambioValutatore", model);
        }

        public ActionResult Save_Valutazione(int idEvaluation, XR_VAL_EVAL_RATING[] questionAnswers, bool saveAsDraft, string owner)
        {
            CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime tms);

            IncentiviEntities db = new IncentiviEntities();

            XR_VAL_EVALUATION eval = null;
            eval = db.XR_VAL_EVALUATION.Include("XR_VAL_EVAL_RATING").FirstOrDefault(x => x.ID_EVALUATION == idEvaluation);

            eval.COD_USER = codUser;
            eval.COD_TERMID = codTermid;
            eval.TMS_TIMESTAMP = tms;

            int nQst = questionAnswers.Count();

            for (int i = 0; i < nQst; i++)
            {
                int qstId = questionAnswers[i].ID_QUESTION;

                var ratingOwner = db.XR_VAL_EVAL_RATING_OWNER.FirstOrDefault(x => x.NAME == owner);

                bool isNew = false;
                XR_VAL_EVAL_RATING rating = eval.XR_VAL_EVAL_RATING.FirstOrDefault(x => x.ID_QUESTION == qstId && x.ID_OWNER == ratingOwner.ID_OWNER);
                if (rating == null)
                {
                    isNew = true;
                    rating = new XR_VAL_EVAL_RATING();
                    rating.ID_RATING = db.XR_VAL_EVAL_RATING.GeneraPrimaryKey();
                    rating.ID_EVALUATION = eval.ID_EVALUATION;
                    rating.ID_OWNER = ratingOwner.ID_OWNER;
                    rating.ID_QUESTION = qstId;
                    rating.VALID_DTA_INI = tms;
                    rating.VALID_DTA_END = null;
                }

                rating.COD_USER = codUser;
                rating.COD_TERMID = codTermid;
                rating.TMS_TIMESTAMP = tms;
                rating.VALUE_INT = questionAnswers[i].VALUE_INT;
                rating.VALUE_STR = questionAnswers[i].VALUE_STR;

                if (isNew) db.XR_VAL_EVAL_RATING.Add(rating);
            }

            if (owner == "Autovalutazione")
            {
                if (!saveAsDraft)
                    eval.IND_AUTOEVAL = true;
            }

            if (ValutazioniManager.CanModifyState(owner))
            {
                int rifState = (int)ValutazioniState.Convalidata;
                if (saveAsDraft)
                    rifState = (int)ValutazioniState.Bozza;

                ValutazioniManager.SalvaStato(db, idEvaluation, rifState);

                //if (rifState == (int)ValutazioniState.Convalidata)
                //{
                //    eval.IND_OPERCHECKED = null;
                //    foreach (var note in eval.XR_VAL_EVALUATION_NOTE.Where(x => x.COD_TIPO == "PresaVisione" && (x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now)))
                //    {
                //        note.VALID_DTA_END = DateTime.Now;
                //    }
                //}

                //Controllo da aggiornare
                //Nel caso sia un direttore di direzione la richiesta deve essere auto-approvata
                //if (sint.COD_UNITAORG.Trim().Length <= 2)
                //{
                //    XR_VAL_EVALUATION_NOTE nota = new XR_VAL_EVALUATION_NOTE()
                //    {
                //        ID_EVAL_NOTE = db.XR_VAL_EVALUATION_NOTE.GeneraOid(x => x.ID_EVAL_NOTE),
                //        ID_PERSONA = sint.ID_PERSONA,
                //        ID_EVALUATION = eval.ID_EVALUATION,
                //        NOT_TEXT = "Valutazione auto-approvata",
                //        IND_APPROVED = true,
                //        VALID_DTA_INI = DateTime.Now,
                //        VALID_DTA_END = null,
                //        COD_USER = CommonManager.GetCurrentUserPMatricola(),
                //        COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress,
                //        TMS_TIMESTAMP = DateTime.Now
                //    };
                //    db.XR_VAL_EVALUATION_NOTE.Add(nota);

                //    XR_VAL_OPER_STATE state = new XR_VAL_OPER_STATE()
                //    {
                //        ID_OPER = db.XR_VAL_OPER_STATE.GeneraOid(x => x.ID_OPER),
                //        ID_PERSONA = sint.ID_PERSONA,
                //        ID_EVALUATION = eval.ID_EVALUATION,
                //        ID_STATE = (int)ValutazioniState.PresaVisione,
                //        VALID_DTA_INI = DateTime.Now,
                //        VALID_DTA_END = null,
                //        COD_USER = CommonManager.GetCurrentUserPMatricola(),
                //        COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress,
                //        TMS_TIMESTAMP = DateTime.Now
                //    };
                //    db.XR_VAL_OPER_STATE.Add(state);
                //}
            }

            if (!DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                return Content("Errore durante il salvataggio della valutazione");

            return Content("OK");
        }

        public ActionResult Save_Valutazione_AnalisiRuo(int evaluation, string note, bool approved)
        {
            IncentiviEntities db = new IncentiviEntities();
            string matricola = CommonHelper.GetCurrentUserMatricola();

            var eval = db.XR_VAL_EVALUATION.Find(evaluation);
            var sint = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == matricola);

            foreach (var item in db.XR_VAL_EVALUATION_NOTE.Where(x => x.ID_EVALUATION == evaluation && x.COD_TIPO == "AnalisiRUO" && (x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now)))
            {
                item.VALID_DTA_END = DateTime.Now;
                item.COD_USER = CommonHelper.GetCurrentUserPMatricola();
                item.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
                item.TMS_TIMESTAMP = DateTime.Now;
            }

            XR_VAL_EVALUATION_NOTE nota = new XR_VAL_EVALUATION_NOTE()
            {
                ID_EVAL_NOTE = db.XR_VAL_EVALUATION_NOTE.GeneraPrimaryKey(),
                ID_PERSONA = sint.ID_PERSONA,
                ID_EVALUATION = eval.ID_EVALUATION,
                NOT_TEXT = note ?? "",
                IND_APPROVED = approved,
                VALID_DTA_INI = DateTime.Now,
                VALID_DTA_END = null,
                COD_TIPO = "AnalisiRUO",
                COD_USER = CommonHelper.GetCurrentUserPMatricola(),
                COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress,
                TMS_TIMESTAMP = DateTime.Now
            };
            db.XR_VAL_EVALUATION_NOTE.Add(nota);

            if (approved)
            {
                eval.IND_OPERCHECKED = 1;
                ValutazioniManager.SalvaStato(db, evaluation, (int)ValutazioniState.Analizzata);
            }
            else
            {
                eval.IND_OPERCHECKED = 0;
                ValutazioniManager.SalvaStato(db, evaluation, (int)ValutazioniState.Bozza);
            }

            if (!DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                return Content("Errore durante il salvataggio della valutazione");

            return Content("OK");
        }

        public ActionResult Save_CambioValutatore(int idEvaluation, int idOldEval, int idNewEval)
        {
            IncentiviEntities db = new IncentiviEntities();

            var eval = db.XR_VAL_EVALUATION.Find(idEvaluation);
            if (eval == null)
                return Content("Valutazione non trovata");

            eval.IND_MANMODIFY = true;
            eval.VALID_DTA_END = DateTime.Now;
            eval.COD_USER = CommonHelper.GetCurrentUserPMatricola();
            eval.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
            eval.TMS_TIMESTAMP = DateTime.Now;


            var newEval = new XR_VAL_EVALUATION()
            {
                ID_EVALUATION = db.XR_VAL_EVALUATION.GeneraPrimaryKey(),
                ID_CAMPAIGN = eval.ID_CAMPAIGN,
                ID_CAMPAIGN_SHEET = eval.ID_CAMPAIGN_SHEET,
                ID_EVALUATOR = idNewEval,
                ID_PERSONA = eval.ID_PERSONA,
                VALID_DTA_INI = DateTime.Now,
                VALID_DTA_END = null,
                IND_MANMODIFY = true,
                COD_USER = CommonHelper.GetCurrentUserPMatricola(),
                COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress,
                TMS_TIMESTAMP = DateTime.Now
            };

            db.XR_VAL_EVALUATION.Add(newEval);

            if (!DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                return Content("Errore durante il salvataggio dei dati");

            return Content("OK");
        }

        public ActionResult Remove_Valutatore(int idEval)
        {
            IncentiviEntities db = new IncentiviEntities();

            var eval = db.XR_VAL_EVALUATION.Find(idEval);
            if (eval == null)
                return Content("Valutazione non trovata");

            eval.IND_MANMODIFY = true;
            eval.VALID_DTA_END = DateTime.Now;
            eval.COD_USER = CommonHelper.GetCurrentUserPMatricola();
            eval.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
            eval.TMS_TIMESTAMP = DateTime.Now;

            if (!DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                return Content("Errore durante il salvataggio dei dati");

            return Content("OK");
        }

        public static IEnumerable<SelectListItem> GetAvailableEvaluator(int campaignSheet, int currentEval)
        {
            var list = ValutazioniManager.GetActiveRoles(null, null, false, ValutazioniLoader.Completo, new RicercaValutazione()
            {
                HasFilter = true,
                CampagnaScheda = campaignSheet

            }).Where(x => x.ID_EVALUATOR != currentEval).OrderBy(x => x.ID_PERSONA != 0 ? x.SINTESI1.Nominativo() : "");

            return list.Select(x => new SelectListItem() { Value = x.ID_EVALUATOR.ToString(), Text = x.ID_PERSONA != 0 ? x.SINTESI1.Nominativo() : "Da definire" });
        }
        #endregion


        #region GestioneValutazioni
        public ActionResult Elenco_Valutazioni(RicercaValutazione model = null)
        {
            if (model == null)
            {
                model = new RicercaValutazione();
                model.HasFilter = false;
            }
            model.ActorView = ValutazioniView.Gestione;
            var valutazioni = ValutazioniManager.GetSheetContainers(null, model, false, model.MostraIniziativeChiuse);
            return View("subpartial/Elenco_Valutazioni", valutazioni);
        }
        public ActionResult Elenco_Valutazioni_Sheet(int idCampaignSheet)
        {
            var sheetContainer = ValutazioniManager.GetSheetContainers(null, new RicercaValutazione() { HasFilter = true, CampagnaScheda = idCampaignSheet, ActorView = ValutazioniView.Gestione }, false, true).First();
            return View("subpartial/Elenco_Valutazioni_Scheda", sheetContainer);
        }

        public ActionResult Modal_Valutazione(int idEvaluation, bool openAsResp, bool canModify=false)
        {
            Valutazione valutazione = ValutazioniManager.GetValutazione(idEvaluation, true);
            valutazione.VistaResponsabile = false;
            valutazione.CanModify = canModify;
            valutazione.Owner = canModify ? "Superiore" : "";
            //if (valutazione.Stato < (int)ValutazioniState.Convalidata)
            //    valutazione.Rating.Clear();

            return RenderModalBody("Gestione valutazione", "subpartial/Modal_Valutazione", valutazione);
        }

        public ActionResult ShowEvalPreview(int idSheet)
        {
            Valutazione valutazione = ValutazioniManager.GetPreviewValutazione(idSheet);
            return RenderModalBody("Gestione valutazione", "subpartial/Modal_Valutazione", valutazione);
        }

        public ActionResult EliminaValutazione(int idEval)
        {
            IncentiviEntities db = new IncentiviEntities();

            db.XR_VAL_EVAL_RATING.RemoveWhere(x => x.ID_EVALUATION == idEval);
            db.XR_VAL_EVALUATION_NOTE.RemoveWhere(x => x.ID_EVALUATION == idEval);
            db.XR_VAL_OPER_STATE.RemoveWhere(x => x.ID_EVALUATION == idEval);

            if (!DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
            {
                return Content("Errore durante la cancellazione");
            }

            return Content("OK");
        }
        #endregion

        #region GestioneSchede
        public ActionResult Widget_Schede()
        {
            var schede = ValutazioniManager.GetSchede();

            return View("subpartial/Widget_schede", schede);
        }
        public ActionResult Modal_Scheda(int idScheda = 0)
        {
            Sheet scheda = ValutazioniManager.GetScheda(idScheda);

            return View("subpartial/Modal_Scheda", scheda);
        }
        #endregion


        #region GestioneCampagne
        public ActionResult Widget_Campagne()
        {
            string matricola = CommonHelper.GetCurrentUserMatricola();

            var campagne = ValutazioniManager.GetCampagne(matricola);

            return View("subpartial/Widget_campagne", campagne);
        }
        public ActionResult Modal_Campagne(int idCampagna = 0)
        {
            Campagna campagna = ValutazioniManager.GetCampagna(idCampagna);

            return RenderModalBody("Gestione iniziativa", "subpartial/Modal_campagna", campagna);
        }
        public ActionResult Save_Campagna(Campagna model)
        {
            IncentiviEntities db = new IncentiviEntities();

            XR_VAL_CAMPAIGN campaign = null;
            if (model.ID == 0)
            {
                campaign = new XR_VAL_CAMPAIGN();
                campaign.ID_CAMPAIGN = db.XR_VAL_CAMPAIGN.GeneraPrimaryKey();
                campaign.VALID_DTA_INI = DateTime.Today;
                campaign.VALID_DTA_END = null;
            }
            else
            {
                campaign = db.XR_VAL_CAMPAIGN.FirstOrDefault(x => x.ID_CAMPAIGN == model.ID);
            }

            campaign.NAME = model.Name;
            campaign.DESCRIPTION = model.Description;
            campaign.COD_TIPOLOGIA = model.Tipologia;
            campaign.DTA_START = model.DateStart;
            campaign.DTA_END = new DateTime(model.DateEnd.Year, model.DateEnd.Month, model.DateEnd.Day, 23, 59, 59);
            campaign.COD_USER = CommonHelper.GetCurrentUserPMatricola();
            campaign.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
            campaign.TMS_TIMESTAMP = DateTime.Now;

            if (model.ID == 0) db.XR_VAL_CAMPAIGN.Add(campaign);

            string esito = "OK";

            if (!DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                esito = "Errore durante il salvataggio dei dati della campagna";

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { Esito = esito, IdCampagna = campaign.ID_CAMPAIGN }
            };
        }
        public ActionResult Cancella_campagna(int idCampagna)
        {
            using (IncentiviEntities db = new IncentiviEntities())
            {
                var campagna = db.XR_VAL_CAMPAIGN.FirstOrDefault(x => x.ID_CAMPAIGN == idCampagna);
                if (campagna != null)
                {
                    campagna.VALID_DTA_END = DateTime.Now.AddSeconds(-1);
                    if (!DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                        return Content("Errore durante la cancellazione");
                }
                else
                {
                    return Content("Campagna non trovata");
                }
            }
            return Content("OK");
        }
        #endregion

        #region GestioneCampagneSchede
        public ActionResult Modal_Campagna_Scheda(int idCampagna, int idCampagnaScheda)
        {
            var campagnaScheda = ValutazioniManager.GetCampagnaScheda(idCampagna, idCampagnaScheda);

            return RenderModalBody("Gestione scheda", "subpartial/Modal_Campagna_Scheda", campagnaScheda);
        }
        public ActionResult Save_Scheda(CampagnaScheda model)
        {
            IncentiviEntities db = new IncentiviEntities();

            XR_VAL_CAMPAIGN_SHEET campaignSheet = null;
            if (model.ID == 0)
            {
                campaignSheet = new XR_VAL_CAMPAIGN_SHEET();
                campaignSheet.ID_CAMPAIGN_SHEET = db.XR_VAL_CAMPAIGN_SHEET.GeneraPrimaryKey();
                campaignSheet.ID_CAMPAIGN = model.Id_Campagna;

            }
            else
            {
                campaignSheet = db.XR_VAL_CAMPAIGN_SHEET.FirstOrDefault(x => x.ID_CAMPAIGN_SHEET == model.ID);
            }
            campaignSheet.ID_SHEET = model.Id_Sheet;
            campaignSheet.DTA_OBSERVATION_INI = model.OsservazioneDataInizio.Value;
            campaignSheet.DTA_OBSERVATION_END = model.OsservazioneDataFine.Value;
            campaignSheet.VALID_DTA_INI = DateTime.Today;
            campaignSheet.VALID_DTA_END = null;
            campaignSheet.DESCRIPTION = model.Descrizione;
            campaignSheet.EMPLOYEE_VIEW = model.AllowEmployeeView;
            campaignSheet.EXT_EVALUATOR = model.AllowExtEvaluator;
            campaignSheet.AUTOEVAL = model.Autovalutazione;
            campaignSheet.DELEGATION = model.AllowDelegation;
            campaignSheet.COD_PIANOSVIL = model.CodicePianoSviluppo;
            if (!String.IsNullOrWhiteSpace(campaignSheet.COD_PIANOSVIL))
                campaignSheet.IND_PIANOSVIL = true;
            else
                campaignSheet.IND_PIANOSVIL = false;
            campaignSheet.ID_OBJ_COLL = model.IdObjColl;
            campaignSheet.COD_USER = CommonHelper.GetCurrentUserPMatricola();
            campaignSheet.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
            campaignSheet.TMS_TIMESTAMP = DateTime.Now;

            if (model.ID == 0) db.XR_VAL_CAMPAIGN_SHEET.Add(campaignSheet);

            bool anyImportantChange = false;

            XR_VAL_CAMPAIGN campaign = db.XR_VAL_CAMPAIGN.Find(model.Id_Campagna);

            foreach (var item in campaignSheet.XR_VAL_CAMPAIGN_SHEET_QUAL.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).Where(x => !model.QualificheInt.Contains(x.ID_QUAL_FILTER)))
            {
                item.VALID_DTA_END = DateTime.Now;
                item.COD_USER = CommonHelper.GetCurrentUserPMatricola();
                item.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
                item.TMS_TIMESTAMP = DateTime.Now;

                anyImportantChange = true;
            }
            foreach (var item in model.QualificheInt.Where(y => !campaignSheet.XR_VAL_CAMPAIGN_SHEET_QUAL.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).Any(x => x.ID_QUAL_FILTER == y)))
            {
                XR_VAL_CAMPAIGN_SHEET_QUAL qual = new XR_VAL_CAMPAIGN_SHEET_QUAL();
                qual.ID_SHEET_QUAL = db.XR_VAL_CAMPAIGN_SHEET_QUAL.GeneraPrimaryKey();
                qual.ID_CAMPAIGN_SHEET = campaignSheet.ID_CAMPAIGN_SHEET;
                qual.ID_QUAL_FILTER = item;
                qual.VALID_DTA_INI = DateTime.Today;
                qual.VALID_DTA_END = null;
                qual.COD_USER = CommonHelper.GetCurrentUserPMatricola();
                qual.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
                qual.TMS_TIMESTAMP = DateTime.Now;
                db.XR_VAL_CAMPAIGN_SHEET_QUAL.Add(qual);

                anyImportantChange = true;
            }

            foreach (var item in campaignSheet.XR_VAL_CAMPAIGN_SHEET_SER.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).Where(x => x.ID_CAMPAIGN_SHEET == campaignSheet.ID_CAMPAIGN_SHEET && !model.Servizi.Contains(x.COD_SERVIZIO)))
            {
                item.VALID_DTA_END = DateTime.Now;
                item.COD_USER = CommonHelper.GetCurrentUserPMatricola();
                item.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
                item.TMS_TIMESTAMP = DateTime.Now;

                anyImportantChange = true;
            }
            foreach (var item in model.Servizi.Where(x => !campaignSheet.XR_VAL_CAMPAIGN_SHEET_SER.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).Any(y => y.COD_SERVIZIO == x)))
            {
                XR_VAL_CAMPAIGN_SHEET_SER ser = new XR_VAL_CAMPAIGN_SHEET_SER();
                ser.ID_SHEET_DIR = db.XR_VAL_CAMPAIGN_SHEET_SER.GeneraPrimaryKey();
                ser.ID_CAMPAIGN_SHEET = campaignSheet.ID_CAMPAIGN_SHEET;
                ser.COD_SERVIZIO = item;
                ser.VALID_DTA_INI = DateTime.Today;
                ser.VALID_DTA_END = null;
                ser.COD_USER = CommonHelper.GetCurrentUserPMatricola();
                ser.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
                ser.TMS_TIMESTAMP = DateTime.Now;
                db.XR_VAL_CAMPAIGN_SHEET_SER.Add(ser);

                anyImportantChange = true;
            }

            foreach (var item in campaignSheet.XR_VAL_CAMPAIGN_SHEET_UORG.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).Where(x => x.ID_CAMPAIGN_SHEET == campaignSheet.ID_CAMPAIGN_SHEET && !model.Uorg.Contains(x.COD_UNITAORG)))
            {
                item.VALID_DTA_END = DateTime.Now;
                item.COD_USER = CommonHelper.GetCurrentUserPMatricola();
                item.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
                item.TMS_TIMESTAMP = DateTime.Now;

                anyImportantChange = true;
            }
            foreach (var item in model.Uorg.Where(x => !campaignSheet.XR_VAL_CAMPAIGN_SHEET_UORG.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).Any(y => y.ID_CAMPAIGN_SHEET == campaignSheet.ID_CAMPAIGN_SHEET && y.COD_UNITAORG == x)))
            {
                XR_VAL_CAMPAIGN_SHEET_UORG uorg = new XR_VAL_CAMPAIGN_SHEET_UORG();
                uorg.ID_SHEET_UORG = db.XR_VAL_CAMPAIGN_SHEET_UORG.GeneraPrimaryKey();
                uorg.ID_CAMPAIGN_SHEET = campaignSheet.ID_CAMPAIGN_SHEET;
                uorg.COD_UNITAORG = item;
                uorg.VALID_DTA_INI = DateTime.Today;
                uorg.VALID_DTA_END = null;
                uorg.COD_USER = CommonHelper.GetCurrentUserPMatricola();
                uorg.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
                uorg.TMS_TIMESTAMP = DateTime.Now;
                db.XR_VAL_CAMPAIGN_SHEET_UORG.Add(uorg);

                anyImportantChange = true;
            }

            if (campaign.COD_TIPOLOGIA != "MBO")
            {
                IncarichiContainer incarichiCont = ValutazioniManager.GetIncarichiContainer();
                var tmpElencoIncaricati = ValutazioniManager.GetResponsabili(incarichiCont, model.Uorg, model.Servizi);
                var availableEvaluation = ValutazioniManager.GetPeopleForEvaluation(db, campaignSheet, model.QualificheInt, model.Servizi);

                List<SINTESI1> sintesiIncaricati = db.SINTESI1.Where(x => tmpElencoIncaricati.Contains(x.COD_MATLIBROMAT)).ToList();

                List<SINTESI1> nonValutabili = new List<SINTESI1>();

                foreach (var incaricato in tmpElencoIncaricati)
                {
                    var sintesi1 = sintesiIncaricati.FirstOrDefault(x => x.COD_MATLIBROMAT == incaricato);
                    int idPersona = sintesi1.ID_PERSONA;
                    string qual = sintesi1.COD_QUALIFICA;


                    var filterCanEval = ValutazioniManager.FilterCanEvaluateByQual(incarichiCont, qual);
                    var matrAvailableEval = ValutazioniManager.GetMyAvailableEvaluation(campaignSheet, availableEvaluation, incaricato, incarichiCont);

                    ManageUpdateEvaluation(db, campaignSheet, idPersona, matrAvailableEval.Where(filterCanEval));
                    nonValutabili.AddRange(matrAvailableEval.Where(x => !filterCanEval(x)));
                }

                ManageUpdateEvaluation(db, campaignSheet, 0, nonValutabili);
            }
            else
            {
                if (model.IdObjColl != null)
                {
                    var schede = MboManager.GetSchede(new MboRicerca() { HasFilter = true, IdIniziativa = model.IdObjColl });
                    foreach (var resp in schede.GroupBy(x => x.IdPersonaResp))
                    {
                        XR_VAL_EVALUATOR evaluator = db.XR_VAL_EVALUATOR.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now)
                                                                        .FirstOrDefault(x => x.ID_CAMPAIGN_SHEET == campaignSheet.ID_CAMPAIGN_SHEET && x.ID_PERSONA == resp.Key);
                        if (evaluator == null)
                        {
                            evaluator = new XR_VAL_EVALUATOR
                            {
                                ID_EVALUATOR = db.XR_VAL_EVALUATOR.GeneraPrimaryKey(),
                                ID_CAMPAIGN = campaignSheet.ID_CAMPAIGN,
                                ID_CAMPAIGN_SHEET = campaignSheet.ID_CAMPAIGN_SHEET,
                                VALID_DTA_INI = DateTime.Today,
                                VALID_DTA_END = null,
                                ID_PERSONA = resp.Key,
                                COD_USER = CommonHelper.GetCurrentUserPMatricola(),
                                COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress,
                                TMS_TIMESTAMP = DateTime.Now
                            };
                            db.XR_VAL_EVALUATOR.Add(evaluator);
                        }

                        foreach (var scheda in resp)
                        {
                            XR_VAL_EVALUATION eval = db.XR_VAL_EVALUATION.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).FirstOrDefault(x => x.ID_EVALUATOR == evaluator.ID_EVALUATOR && x.ID_PERSONA == scheda.IdPersonaValutato);
                            if (eval == null)
                            {
                                eval = new XR_VAL_EVALUATION()
                                {
                                    ID_EVALUATION = db.XR_VAL_EVALUATION.GeneraPrimaryKey(),
                                    ID_EVALUATOR = evaluator.ID_EVALUATOR,
                                    ID_PERSONA = scheda.IdPersonaValutato,
                                    ID_CAMPAIGN = campaignSheet.ID_CAMPAIGN,
                                    ID_CAMPAIGN_SHEET = campaignSheet.ID_CAMPAIGN_SHEET,
                                    VALID_DTA_INI = DateTime.Today,
                                    VALID_DTA_END = null,
                                    COD_USER = CommonHelper.GetCurrentUserPMatricola(),
                                    COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress,
                                    TMS_TIMESTAMP = DateTime.Now
                                };
                                db.XR_VAL_EVALUATION.Add(eval);
                            }
                        }
                    }
                }
            }

            if (!DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                return Content("Errore durante il salvataggio dei dati della campagna");

            return Content("OK");
        }

        private static void ManageUpdateEvaluation(IncentiviEntities db, XR_VAL_CAMPAIGN_SHEET campaignSheet, int idPersona, IEnumerable<SINTESI1> canEvaluate)
        {
            if (canEvaluate != null && canEvaluate.Any())
            {
                var evaluator = campaignSheet.XR_VAL_EVALUATOR.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).FirstOrDefault(y => y.ID_CAMPAIGN_SHEET == campaignSheet.ID_CAMPAIGN_SHEET && y.ID_PERSONA == idPersona);
                if (evaluator == null)
                {
                    evaluator = new XR_VAL_EVALUATOR
                    {
                        ID_EVALUATOR = db.XR_VAL_EVALUATOR.GeneraPrimaryKey(),
                        ID_CAMPAIGN = campaignSheet.ID_CAMPAIGN,
                        ID_CAMPAIGN_SHEET = campaignSheet.ID_CAMPAIGN_SHEET,
                        VALID_DTA_INI = DateTime.Today,
                        VALID_DTA_END = null,
                        ID_PERSONA = idPersona,
                        COD_USER = CommonHelper.GetCurrentUserPMatricola(),
                        COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress,
                        TMS_TIMESTAMP = DateTime.Now
                    };
                    db.XR_VAL_EVALUATOR.Add(evaluator);
                }

                var idPers = canEvaluate.Select(x => x.ID_PERSONA);

                //invalido le valutazioni già presenti
                //Devono essere invalidate solo le valutazioni definite dall'algoritmo, quelle assegnate manualmente non devono essere toccate
                foreach (var item in evaluator.XR_VAL_EVALUATION.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).Where(x => x.ID_EVALUATOR == evaluator.ID_EVALUATOR && !idPers.Contains(x.ID_PERSONA) && !x.IND_MANMODIFY))
                {
                    item.VALID_DTA_END = DateTime.Now;
                    item.COD_USER = CommonHelper.GetCurrentUserPMatricola();
                    item.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
                    item.TMS_TIMESTAMP = DateTime.Now;
                }

                //creo le valutazioni assenti
                //In questo caso, le valutazioni assegnate a qualcun altro risulterebbero non valide ma con flag IND_MODIFY
                //Quindi aggiunge solo le persone per cui non c'è una valutazione valida o non modificata
                foreach (var item in idPers.Where(x => !evaluator.XR_VAL_EVALUATION.Any(y => y.ID_EVALUATOR == evaluator.ID_EVALUATOR && y.ID_PERSONA == x
                                && ((y.VALID_DTA_INI <= DateTime.Now && (y.VALID_DTA_END == null || DateTime.Now < y.VALID_DTA_END)) || y.IND_MANMODIFY))))
                {
                    XR_VAL_EVALUATION eval = new XR_VAL_EVALUATION()
                    {
                        ID_EVALUATION = db.XR_VAL_EVALUATION.GeneraPrimaryKey(),
                        ID_EVALUATOR = evaluator.ID_EVALUATOR,
                        ID_PERSONA = item,
                        ID_CAMPAIGN = campaignSheet.ID_CAMPAIGN,
                        ID_CAMPAIGN_SHEET = campaignSheet.ID_CAMPAIGN_SHEET,
                        VALID_DTA_INI = DateTime.Today,
                        VALID_DTA_END = null,
                        COD_USER = CommonHelper.GetCurrentUserPMatricola(),
                        COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress,
                        TMS_TIMESTAMP = DateTime.Now
                    };
                    db.XR_VAL_EVALUATION.Add(eval);
                }
            }
            else
            {
                var evaluator = campaignSheet.XR_VAL_EVALUATOR.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).FirstOrDefault(y => y.ID_CAMPAIGN_SHEET == campaignSheet.ID_CAMPAIGN_SHEET && y.ID_PERSONA == idPersona);
                if (evaluator != null)
                {
                    evaluator.VALID_DTA_END = DateTime.Now;
                    evaluator.COD_USER = CommonHelper.GetCurrentUserPMatricola();
                    evaluator.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
                    evaluator.TMS_TIMESTAMP = DateTime.Now;
                }
            }
        }

        public ActionResult TestValutazioni()
        {
            //var matricola = "951855";
            var matricola = "584889";
            var listStrutture = new List<string>() { };
            var listServizi = new List<string>() { };
            var listQual = new List<int>() { 6 };

            XR_VAL_CAMPAIGN_SHEET campaignSheet = new XR_VAL_CAMPAIGN_SHEET()
            {
                DTA_OBSERVATION_INI = new DateTime(2019, 1, 1),
                DTA_OBSERVATION_END = new DateTime(2019, 12, 31)
            };


            IncentiviEntities db = new IncentiviEntities();
            IncarichiContainer incarichiCont = ValutazioniManager.GetIncarichiContainer();
            var tmpElencoIncaricati = ValutazioniManager.GetResponsabili(incarichiCont, listStrutture, listServizi);
            var availableEvaluation = ValutazioniManager.GetPeopleForEvaluation(db, campaignSheet, listQual, listServizi);

            List<SINTESI1> sintesiIncaricati = db.SINTESI1.Where(x => x.COD_MATLIBROMAT == matricola).ToList();

            string output = "";

            foreach (var incaricato in tmpElencoIncaricati.Where(x => x == matricola))
            {
                var matrAvailableEval = ValutazioniManager.GetMyAvailableEvaluation(campaignSheet, availableEvaluation, incaricato, incarichiCont);
                output = String.Join("<br/>", matrAvailableEval.Select(x => x.COD_MATLIBROMAT + " - " + x.DES_COGNOMEPERS + " " + x.DES_NOMEPERS));
            }

            return Content(output);
        }

        public ActionResult Widget_Campagna_Schede(int idCampagna)
        {
            var campagnaSchede = ValutazioniManager.GetCampagnaSchede(idCampagna);

            return View("subpartial/Widget_Campagna_Schede", campagnaSchede);
        }
        public ActionResult Cancella_CampagnaScheda(int idCampagnaScheda)
        {
            using (IncentiviEntities db = new IncentiviEntities())
            {
                var campagnaScheda = db.XR_VAL_CAMPAIGN_SHEET.FirstOrDefault(x => x.ID_CAMPAIGN_SHEET == idCampagnaScheda);
                if (campagnaScheda != null)
                {
                    campagnaScheda.VALID_DTA_END = DateTime.Now.AddSeconds(-1);
                    if (!DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                        return Content("Errore durante la cancellazione");
                }
                else
                {
                    return Content("Scheda non trovata");
                }
            }
            return Content("OK");
        }

        public ActionResult Report_Mail_CampagnaScheda(int idCampagnaScheda, bool soloValutati = false)
        {
            var campagnaScheda = ValutazioniManager.GetCampagnaScheda(0, idCampagnaScheda);
            var scheda = ValutazioniManager.GetScheda(campagnaScheda.Id_Sheet);

            var sheets = ValutazioniManager.GetSheetContainers(null,
                new RicercaValutazione() { HasFilter = true, CampagnaScheda = idCampagnaScheda },
                false, true
                );

            Dictionary<string, string> responsabili = new Dictionary<string, string>();
            Dictionary<string, string> valutatori = new Dictionary<string, string>();
            Dictionary<string, string> valutati = new Dictionary<string, string>();


            XLWorkbook workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Elenco destinatari");

            int row = 1;
            ws.Cell(row, 1).SetValue("Valutatore"); ws.Range(row, 1, row, 5).Merge().Style.Font.Bold = true;
            ws.Cell(row, 4).SetValue("Valutato"); ws.Range(row, 6, row, 10).Merge().Style.Font.Bold = true;
            row++;
            ws.Cell(row, 1).SetValue("Matricola").Style.Font.Bold = true;
            ws.Cell(row, 2).SetValue("Nominativo").Style.Font.Bold = true;
            ws.Cell(row, 3).SetValue("Qualifica").Style.Font.Bold = true;
            ws.Cell(row, 4).SetValue("Direzione").Style.Font.Bold = true;
            ws.Cell(row, 5).SetValue("Email").Style.Font.Bold = true;

            ws.Cell(row, 6).SetValue("Matricola").Style.Font.Bold = true;
            ws.Cell(row, 7).SetValue("Nominativo").Style.Font.Bold = true;
            ws.Cell(row, 8).SetValue("Qualifica").Style.Font.Bold = true;
            ws.Cell(row, 9).SetValue("Direzione").Style.Font.Bold = true;
            ws.Cell(row, 10).SetValue("Email").Style.Font.Bold = true;
            ws.Cell(row, 11).SetValue("Stato").Style.Font.Bold = true;
            ws.Cell(row, 12).SetValue("Media").Style.Font.Bold = true;

            var weights = scheda.Groups.SelectMany(x => x.Questions.Where(y => y.Weight > 0));

            foreach (var sheet in sheets)
            {
                foreach (var role in sheet.Roles.Where(x => !x.Role.IsDelegate() && x.Evaluations.Count() > 0).OrderBy(x => x.Role.SINTESI1.Nominativo()))
                {
                    string matricolaValutatore = role.Role.SINTESI1.COD_MATLIBROMAT;
                    string nomeValutatore = role.Role.SINTESI1.Nominativo();
                    string qualifica = role.Role.SINTESI1.DES_QUALIFICA;
                    string direzione = role.Role.SINTESI1.DES_SERVIZIO;
                    string emailValutatore = String.Format("p{0}@rai.it", matricolaValutatore);//CommonHelper.GetEmailPerMatricola(matricolaValutatore);

                    foreach (EvaluationContainer evaluation in role.Evaluations.OrderBy(x => x.Person.Nominativo()))
                    {
                        int stato = ValutazioniManager.GetMaxEvalState(evaluation.Evaluation.XR_VAL_OPER_STATE);
                        if (soloValutati && stato < (int)ValutazioniState.Bozza) continue;


                        row++;
                        ws.Cell(row, 1).SetValue(matricolaValutatore);
                        ws.Cell(row, 2).SetValue(nomeValutatore);
                        ws.Cell(row, 3).SetValue(qualifica);
                        ws.Cell(row, 4).SetValue(direzione);
                        ws.Cell(row, 5).SetValue(emailValutatore);

                        ws.Cell(row, 6).SetValue(evaluation.Person.COD_MATLIBROMAT);
                        ws.Cell(row, 7).SetValue(evaluation.Person.Nominativo());
                        ws.Cell(row, 8).SetValue(evaluation.Person.DES_QUALIFICA);
                        ws.Cell(row, 9).SetValue(evaluation.Person.DES_SERVIZIO);
                        ws.Cell(row, 10).SetValue(String.Format("p{0}@rai.it", evaluation.Person.COD_MATLIBROMAT));//CommonHelper.GetEmailPerMatricola(evaluation.Person.COD_MATLIBROMAT);

                        switch (stato)
                        {
                            case (int)ValutazioniState.Bozza:
                                ws.Cell(row, 11).SetValue("Bozza");
                                break;
                            case (int)ValutazioniState.Convalidata:
                                ws.Cell(row, 11).SetValue("Valutato");
                                break;
                            case (int)ValutazioniState.PresaVisione:
                                ws.Cell(row, 11).SetValue("Visionata");
                                break;
                            default:
                                break;
                        }

                        double media = 0;
                        double sommaValPerPeso = 0;
                        double sommaPeso = 0;
                        foreach (var item in weights)
                        {
                            var tmpRating = evaluation.Evaluation.XR_VAL_EVAL_RATING.FirstOrDefault(x => x.ID_QUESTION == item.IdEvalQst);
                            int valueQst = tmpRating != null ? tmpRating.VALUE_INT.GetValueOrDefault() : 0;
                            sommaValPerPeso += valueQst * item.Weight;
                            sommaPeso += item.Weight;
                        }

                        if (sommaPeso > 0)
                            media = sommaValPerPeso / sommaPeso;

                        ws.Cell(row, 12).SetValue(media);//.Style.NumberFormat.SetFormat("0.00");
                    }
                }
            }

            ws.Columns().AdjustToContents();

            MemoryStream ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;

            string nomeFile = campagnaScheda.Campagna_Name + " - " + campagnaScheda.Descrizione;
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = nomeFile + ".xlsx" };
        }

        public ActionResult Report_UORG(int idCampagnaScheda)
        {
            IncentiviEntities db = new IncentiviEntities();
            var sheet = db.XR_VAL_CAMPAIGN_SHEET.FirstOrDefault(x => x.ID_CAMPAIGN_SHEET == idCampagnaScheda);

            var qualificheInt = sheet.XR_VAL_CAMPAIGN_SHEET_QUAL.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).Select(x => x.ID_QUAL_FILTER);
            var elencoServizi = sheet.XR_VAL_CAMPAIGN_SHEET_SER.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).Select(x => x.COD_SERVIZIO);

            var list = db.SINTESI1.Where(x => x.DTA_FINE_CR > DateTime.Today);

            if (qualificheInt != null && qualificheInt.Count() > 0)
            {
                var qualFilters = db.XR_VAL_QUAL_FILTER.Where(x => qualificheInt.Contains(x.ID_QUAL_FILTER)).ToList();

                IEnumerable<string> categoryIncluded = qualFilters.Where(x => !String.IsNullOrWhiteSpace(x.QUAL_INCLUDED)).SelectMany(x => x.QUAL_INCLUDED.Split(',')).Distinct();
                IEnumerable<string> categoryExcluded = qualFilters.Where(x => !String.IsNullOrWhiteSpace(x.QUAL_EXCLUDED)).SelectMany(x => x.QUAL_EXCLUDED.Split(',')).Distinct().Where(y => !categoryIncluded.Contains(y));
                list = list.Where(s => s.ASSQUAL_HISTORY.Any(x => x.DTA_INIZIO <= sheet.DTA_OBSERVATION_END && sheet.DTA_OBSERVATION_INI <= x.DTA_FINE && categoryIncluded.Any(a => x.COD_QUALIFICA.StartsWith(a)) && !categoryExcluded.Any(b => x.COD_QUALIFICA.StartsWith(b))));
            }

            if (elencoServizi != null && elencoServizi.Count() > 0)
                list = list.Where(s => s.XR_SERVIZIO_HISTORY.Any(x => x.DTA_INIZIO <= sheet.DTA_OBSERVATION_END && sheet.DTA_OBSERVATION_INI <= x.DTA_FINE && elencoServizi.Contains(x.COD_SERVIZIO)));



            XLWorkbook workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Elenco strutture");

            var uorg = list.Select(x => new { uorg = x.COD_UNITAORG, des = x.DES_DENOMUNITAORG }).Distinct();

            int row = 1;
            foreach (var item in uorg)
            {
                row++;
                ws.Cell(row, 1).SetValue(item.uorg);
                ws.Cell(row, 2).SetValue(item.des);
            }

            ws.Columns().AdjustToContents();

            MemoryStream ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;

            string nomeFile = sheet.XR_VAL_CAMPAIGN.NAME + " - " + sheet.XR_VAL_CAMPAIGN.DESCRIPTION;
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = nomeFile + ".xlsx" };
        }

        public ActionResult TestCreazioneValutazioni()
        {
            IncentiviEntities db = new IncentiviEntities();
            var cSheet = new XR_VAL_CAMPAIGN_SHEET()
            {
                DTA_OBSERVATION_INI = new DateTime(2020, 01, 01),
                DTA_OBSERVATION_END = new DateTime(2020, 12, 31)
            };

            List<string> strutture = new List<string>();
            List<string> servizi = new List<string>();
            List<int> qualifiche = new List<int>();

            string[] servEscl = new string[] { "88", "92", "95", "96", "97", "N2", "B2", "C2", "N8" };

            servizi.AddRange(db.XR_TB_SERVIZIO.Where(x => x.COD_SERVIZIO.Length == 2 && !servEscl.Contains(x.COD_SERVIZIO)).Select(x => x.COD_SERVIZIO));
            //qualifiche.AddRange(cSheet.XR_VAL_CAMPAIGN_SHEET_QUAL.Select(x => x.ID_QUAL_FILTER));
            qualifiche.Add(15);

            IncarichiContainer incarichiCont = ValutazioniManager.GetIncarichiContainer2();
            var tmpElencoIncaricati = ValutazioniManager.GetResponsabili(incarichiCont, strutture, servizi, false);
            var availableEvaluation = ValutazioniManager.GetPeopleForEvaluation(db, cSheet, qualifiche, servizi);

            List<SINTESI1> sintesiIncaricati = db.SINTESI1.Where(x => tmpElencoIncaricati.Contains(x.COD_MATLIBROMAT)).ToList();

            var workbook = new XLWorkbook();
            var ws = workbook.AddWorksheet("Schede F1");

            int row = 1;
            ws.Cell(row, 1).SetValue("Valutatore"); ws.Range(row, 1, row, 5).Merge().Style.Font.Bold = true;
            ws.Cell(row, 6).SetValue("Valutato"); ws.Range(row, 6, row, 10).Merge().Style.Font.Bold = true;
            row++;
            ws.Cell(row, 1).SetValue("Matricola").Style.Font.Bold = true;
            ws.Cell(row, 2).SetValue("Nominativo").Style.Font.Bold = true;
            ws.Cell(row, 3).SetValue("Qualifica").Style.Font.Bold = true;
            ws.Cell(row, 4).SetValue("Direzione").Style.Font.Bold = true;
            ws.Cell(row, 5).SetValue("Email").Style.Font.Bold = true;

            ws.Cell(row, 6).SetValue("Matricola").Style.Font.Bold = true;
            ws.Cell(row, 7).SetValue("Nominativo").Style.Font.Bold = true;
            ws.Cell(row, 8).SetValue("Qualifica").Style.Font.Bold = true;
            ws.Cell(row, 9).SetValue("Direzione").Style.Font.Bold = true;
            ws.Cell(row, 10).SetValue("Email").Style.Font.Bold = true;
            ws.Cell(row, 11).SetValue("DataNascita").Style.Font.Bold = true;

            foreach (var incaricato in tmpElencoIncaricati.OrderBy(x => x))
            {
                var sintVal = sintesiIncaricati.FirstOrDefault(x => x.COD_MATLIBROMAT == incaricato);
                var matrAvailableEval = ValutazioniManager.GetMyAvailableEvaluation(cSheet, availableEvaluation, incaricato, incarichiCont);
                if (matrAvailableEval != null && matrAvailableEval.Any())
                {
                    var filterCanEval = ValutazioniManager.FilterCanEvaluateByQual(incarichiCont, sintVal.COD_QUALIFICA);

                    foreach (var item in matrAvailableEval.Where(filterCanEval))
                    {
                        row++;

                        ws.Cell(row, 1).SetValue(sintVal.COD_MATLIBROMAT);
                        ws.Cell(row, 2).SetValue(sintVal.Nominativo());
                        ws.Cell(row, 3).SetValue(sintVal.DES_QUALIFICA);
                        ws.Cell(row, 4).SetValue(sintVal.DES_SERVIZIO);
                        ws.Cell(row, 5).SetValue("");

                        ws.Cell(row, 6).SetValue(item.COD_MATLIBROMAT);
                        ws.Cell(row, 7).SetValue(item.Nominativo());
                        ws.Cell(row, 8).SetValue(item.DES_QUALIFICA);
                        ws.Cell(row, 9).SetValue(item.DES_SERVIZIO);
                        ws.Cell(row, 10).SetValue("");
                        ws.Cell(row, 11).SetValue(item.DTA_NASCITAPERS.Value.ToString("dd/MM/yyyy"));

                    }

                    foreach (var item in matrAvailableEval.Where(x => !filterCanEval(x)).OrderBy(x => x.COD_MATLIBROMAT))
                    {
                        row++;

                        ws.Cell(row, 1).SetValue("Da definire");
                        ws.Cell(row, 2).SetValue("Da definire");
                        ws.Cell(row, 3).SetValue("Da definire");
                        ws.Cell(row, 4).SetValue("Da definire");
                        ws.Cell(row, 5).SetValue("");

                        ws.Cell(row, 6).SetValue(item.COD_MATLIBROMAT);
                        ws.Cell(row, 7).SetValue(item.Nominativo());
                        ws.Cell(row, 8).SetValue(item.DES_QUALIFICA);
                        ws.Cell(row, 9).SetValue(item.DES_SERVIZIO);
                        ws.Cell(row, 10).SetValue("");
                        ws.Cell(row, 11).SetValue(item.DTA_NASCITAPERS.Value.ToString("dd/MM/yyyy"));
                    }
                }
                else
                {

                }
            }

            ws.Columns().AdjustToContents();

            MemoryStream ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;

            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = "Schede F1.xlsx" };
        }
        #endregion

        #region ListItem
        public static List<ListItem> GetSelectDelega()
        {
            return ValutazioniManager.GetSelectDeleghe();
        }
        public static List<ListItem> GetSelectEvalSheet()
        {
            IncentiviEntities db = new IncentiviEntities();
            List<ListItem> lista = new List<ListItem>();

            lista.Add(new ListItem()
            {
                Value = "",
                Text = "Seleziona una scheda di valutazione"
            });

            foreach (var item in ValutazioniManager.GetSchedeValutazione())
            {
                lista.Add(new ListItem()
                {
                    Value = item.ID_SHEET.ToString(),
                    Text = item.NAME
                });
            }

            return lista;
        }
        public static List<ListItem> GetAllowExtEvaluator()
        {
            List<ListItem> lista = new List<ListItem>();

            lista.Add(new ListItem()
            {
                Value = "0",
                Text = "Non Ammessa"
            });
            lista.Add(new ListItem()
            {
                Value = "1",
                Text = "Ammessa"
            });
            lista.Add(new ListItem()
            {
                Value = "2",
                Text = "Obbligatoria"
            });

            return lista;
        }
        public static List<ListItem> GetAutoValutazioneList()
        {
            List<ListItem> lista = new List<ListItem>();

            foreach (var item in Enum.GetValues(typeof(ValutazioniAuto)))
            {
                lista.Add(new ListItem()
                {
                    Value = ((int)(ValutazioniAuto)item).ToString(),
                    Text = ((ValutazioniAuto)item).GetAmbientValue()
                });
            }

            return lista;
        }
        public static List<ListItem> GetSelectQualFilter()
        {
            IncentiviEntities db = new IncentiviEntities();
            List<ListItem> lista = new List<ListItem>();

            foreach (var item in db.XR_VAL_QUAL_FILTER)
            {
                lista.Add(new ListItem()
                {
                    Value = item.ID_QUAL_FILTER.ToString(),
                    Text = item.DESCRIPTION
                });
            }

            return lista;
        }
        public static List<ListItem> GetSelectDirezioniFilter()
        {
            IncentiviEntities db = new IncentiviEntities();
            List<ListItem> lista = new List<ListItem>();

            foreach (var item in db.XR_TB_SERVIZIO.Where(x => x.QTA_ORDINE == 0))
            {
                lista.Add(new ListItem()
                {
                    Value = item.COD_SERVIZIO,
                    Text = item.DES_SERVIZIO
                });
            }

            return lista;
        }
        public static List<ListItem> GetSelectCampagne()
        {
            List<ListItem> lists = new List<ListItem>();
            lists.Add(new ListItem()
            {
                Value = "0",
                Text = "Seleziona un'iniziativa"
            });

            var iniziative = ValutazioniManager.GetCampagne(CommonHelper.GetCurrentUserMatricola()).Where(x => x.IsActive());

            lists.AddRange(iniziative.Select(x => new ListItem() { Value = x.ID.ToString(), Text = x.Name }));

            return lists;
        }

        public static List<SelectListItem> GetStati()
        {
            List<SelectListItem> result = new List<SelectListItem>();
            result.Add(new SelectListItem()
            {
                Text = "Seleziona uno stato",
                Value = ""
            });
            var db = new IncentiviEntities();
            foreach (var item in db.XR_VAL_STATE.OrderBy(x => x.ID_STATE))
            {
                result.Add(new SelectListItem()
                {
                    Text = item.NAME,
                    Value = item.ID_STATE.ToString()
                });
            }

            return result;
        }

        public static List<SelectListGroup> GetSelectCampagneSchede()
        {
            List<SelectListGroup> list = new List<SelectListGroup>();

            var activeRoles = ValutazioniManager.GetActiveRoles(null, null, false).Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now)
                .Where(x => x.XR_VAL_CAMPAIGN_SHEET != null)
                .GroupBy(x => x.ID_CAMPAIGN);

            foreach (var group in activeRoles)
            {
                var slGroup = new SelectListGroup() { Name = group.First().XR_VAL_CAMPAIGN.NAME, ListItems = new List<ListItem>() };
                slGroup.ListItems.AddRange(group.Select(x => new ListItem() { Value = x.ID_CAMPAIGN_SHEET.ToString(), Text = x.XR_VAL_CAMPAIGN_SHEET.DESCRIPTION }).Distinct());
                list.Add(slGroup);
            }


            return list;
        }

        public static List<SelectListItem> GetSelectValutatori()
        {
            List<SelectListItem> list = new List<SelectListItem>();
            var activeRole = ValutazioniManager.GetActiveRoles(null, null, true, ValutazioniLoader.Minimo).Select(x => x.SINTESI1).OrderBy(x => x != null ? x.Nominativo() : "");

            list.Add(new SelectListItem()
            {
                Text = "Seleziona un valutatore",
                Value = ""
            });

            foreach (var item in activeRole.Distinct())
                list.Add(new SelectListItem() { Value = item != null ? item.ID_PERSONA.ToString() : "0", Text = item != null ? item.Nominativo() : "Da definire" });

            return list;
        }
        #endregion

        [HttpPost]
        public ActionResult GetReportScheda(int idScheda)
        {
            var scheda = ValutazioniManager.GetReportScheda(idScheda);
            return View("subpartial/Report_Valutazioni", scheda);
        }
        [HttpPost]
        public ActionResult GetReportDomandaRisposta(int idScheda, int idQst, string answer)
        {
            ReportCampagnaScheda report = ValutazioniManager.GetReportDomandaRisposta(idScheda, idQst, answer);
            var settings = new Newtonsoft.Json.JsonSerializerSettings() { NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore };
            return View("subpartial/Report_Valutazioni_Risposte", report);
        }
    }
}
