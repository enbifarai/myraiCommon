using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using myRai.Business;
using myRai.DataAccess;
using myRaiCommonManager;
using myRaiCommonModel.Gestionale;
using myRaiData.Incentivi;
using myRaiHelper;

namespace myRai.Controllers.Gestionale
{
    public class ValutazioniController : BaseCommonController
    {
        public ActionResult Index()
        {
            ValutazioniPermission permission = ValutazioniManager.GetPermission(CommonManager.GetCurrentUserMatricola());

            return View(permission);
        }

        #region GestioneDeleghe
        public ActionResult Widget_Deleghe()
        {
            string matricola = CommonManager.GetCurrentUserMatricola();

            DelegheContainer box = new DelegheContainer();
            box.Delegator = ValutazioniManager.GetElencoDeleghe(matricola);
            box.Delegate = ValutazioniManager.GetElencoDelegheRicevute(matricola);
            box.RuoliDelegabili = ValutazioniManager.GetRuoliDelegabili(null, matricola);

            return View("subpartial/Widget_deleghe", box);
        }
        public ActionResult Modal_Delega(int idDelega = 0)
        {
            string matricola = CommonManager.GetCurrentUserMatricola();

            DelegaModel delega = ValutazioniManager.GetDelega(idDelega, matricola);

            return View("subpartial/Modal_Delega", delega);
        }
        public ActionResult GetAJAXPersoneDelegabili(int evaluatorRole)
        {
            List<SINTESI1> people = new List<SINTESI1>();

            using (IncentiviEntities db = new IncentiviEntities())
            {
                string matr = CommonManager.GetCurrentUserMatricola();
                //var incarichi = ValutazioniManager.GetMieiIncarichi(matr, null);
                var filterQual = ValutazioniManager.FilterDifferentEvaluator(db, matr);
                var me = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == matr);
                people = db.SINTESI1.Where(filterQual).Where(x => x.COD_MATLIBROMAT != matr && x.DTA_FINE_CR > DateTime.Today && x.ID_UNITAORG == me.ID_UNITAORG).ToList().OrderBy(x => x.Nominativo()).ToList();
            }

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new
                {
                    result = people.Select(x => new SelectListItem() { Value = x.ID_PERSONA.ToString(), Text = x.Nominativo() }).ToList()
                }
            };
        }
        public ActionResult GetValutazioniDelegabili(int evaluatorRole, int? personChoosen)
        {
            DelegaModel delega = new DelegaModel();
            using (IncentiviEntities db = new IncentiviEntities())
            {
                XR_VAL_EVALUATOR eval = db.XR_VAL_EVALUATOR.FirstOrDefault(x => x.ID_EVALUATOR == evaluatorRole);
                delega.ValutazioniDelegabili.AddRange(ValutazioniManager.GetValutazioniDelegabili(db, eval));
            }

            return View("subpartial/Modal_Delega_ElencoPersone", delega);
        }
        public ActionResult Save_Delega(DelegaModel model)
        {
            IncentiviEntities db = new IncentiviEntities();

            XR_VAL_DELEGATION delegation = new XR_VAL_DELEGATION();
            if (model.ID == 0)
            {
                delegation = new XR_VAL_DELEGATION();
                delegation.ID_DELEGATION = db.XR_VAL_DELEGATION.GeneraPrimaryKey();
                delegation.VALID_DTA_INI = DateTime.Now;
                delegation.VALID_DTA_END = null;
                delegation.ID_DELEGATOR = model.RuoloDelegato;

                var delegator = db.XR_VAL_EVALUATOR.FirstOrDefault(x => x.ID_EVALUATOR == model.RuoloDelegato);

                XR_VAL_EVALUATOR @delegate = new XR_VAL_EVALUATOR();
                @delegate.ID_EVALUATOR = db.XR_VAL_EVALUATOR.GeneraPrimaryKey();
                @delegate.ID_CAMPAIGN = delegator.ID_CAMPAIGN;
                @delegate.ID_CAMPAIGN_SHEET = delegator.ID_CAMPAIGN_SHEET;
                @delegate.VALID_DTA_INI = DateTime.Now;
                @delegate.VALID_DTA_END = null;
                @delegate.ID_PERSONA = model.PersonaDelegata;
                @delegate.COD_USER = CommonManager.GetCurrentUserPMatricola();
                @delegate.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
                @delegate.TMS_TIMESTAMP = DateTime.Now;
                db.XR_VAL_EVALUATOR.Add(@delegate);

                delegation.ID_DELEGATE = @delegate.ID_EVALUATOR;
            }
            else
            {
                delegation = db.XR_VAL_DELEGATION.FirstOrDefault(x => x.ID_DELEGATION == model.ID);
            }

            delegation.DTA_START = model.DateStart;
            delegation.DTA_END = model.DateEnd;

            foreach (var item in delegation.XR_VAL_DELEGATION_PERS.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now).Where(x => !model.ValutazioniDelegabiliSel.Contains(x.ID_PERSONA)))
            {
                item.VALID_DTA_END = DateTime.Now;
                item.COD_USER = CommonManager.GetCurrentUserPMatricola();
                item.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
                item.TMS_TIMESTAMP = DateTime.Now;
            }

            foreach (var item in model.ValutazioniDelegabiliSel.Where(y => !delegation.XR_VAL_DELEGATION_PERS.Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now).Any(x => x.ID_PERSONA == y)))
            {
                XR_VAL_DELEGATION_PERS delPers = new XR_VAL_DELEGATION_PERS();
                delPers.ID_DEL_PERS = db.XR_VAL_DELEGATION_PERS.GeneraPrimaryKey();
                delPers.ID_DELEGATION = delegation.ID_DELEGATION;
                delPers.ID_PERSONA = item;
                delPers.VALID_DTA_INI = DateTime.Today;
                delPers.VALID_DTA_END = null;
                delPers.COD_USER = CommonManager.GetCurrentUserPMatricola();
                delPers.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
                delPers.TMS_TIMESTAMP = DateTime.Now;
                db.XR_VAL_DELEGATION_PERS.Add(delPers);
            }

            delegation.COD_USER = CommonManager.GetCurrentUserPMatricola();
            delegation.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
            delegation.TMS_TIMESTAMP = DateTime.Now;

            if (model.ID == 0) db.XR_VAL_DELEGATION.Add(delegation);

            if (!DBHelper.Save(db, CommonManager.GetCurrentUserMatricola()))
                return Content("Errore durante il salvataggio dei dati della delega");

            return Content("OK");
        }
        public ActionResult Cancella_delega(int idDelega)
        {
            using (IncentiviEntities db = new IncentiviEntities())
            {
                var delega = db.XR_VAL_DELEGATION.FirstOrDefault(x => x.ID_DELEGATION == idDelega);
                if (delega != null)
                {
                    delega.VALID_DTA_END = DateTime.Now.AddSeconds(-1);
                    delega.DELEGATO.VALID_DTA_END = delega.VALID_DTA_END;
                    if (!DBHelper.Save(db, CommonManager.GetCurrentUserMatricola()))
                        return Content("Errore durante la cancellazione");
                }
                else
                {
                    return Content("Delega non trovata");
                }
            }
            return Content("OK");
        }
        #endregion

        #region GestioneValutazioni
        public ActionResult Elenco_Valutazioni(RicercaValutazione model = null)
        {
            var valutazioni = ValutazioniManager.GetSheetContainers(CommonManager.GetCurrentUserMatricola(), model, true);
            return View("subpartial/Elenco_Valutazioni", valutazioni);
        }
        public ActionResult Elenco_Valutazioni_Sheet(int idCampaignSheet)
        {
            var sheetContainer = ValutazioniManager.GetSheetContainers(CommonManager.GetCurrentUserMatricola(), new RicercaValutazione() { HasFilter = true, CampagnaScheda = idCampaignSheet }, true).First();
            return View("subpartial/Elenco_Valutazioni_Scheda", sheetContainer);
        }
        public ActionResult Modal_Valutazione(int idEvaluation, bool openAsResp, bool asValued = false, string owner="")
        {
            string matricola = CommonManager.GetCurrentUserMatricola();

            if (String.IsNullOrWhiteSpace(owner))
                owner = "Superiore";

            Valutazione valutazione = ValutazioniManager.GetValutazione(idEvaluation, false, owner);
            valutazione.VistaResponsabile = openAsResp;
            valutazione.CanModify = valutazione.CanModify && !openAsResp;

            if (asValued)
                valutazione.CanModify = false;

            return View("subpartial/Modal_Valutazione", valutazione);
        }
        public static Valutazione Modal_Valutazione_Other(int idEvaluation, string owner)
        {
            if (!String.IsNullOrWhiteSpace(owner))
                owner = "Superiore";

            Valutazione valutazione = ValutazioniManager.GetValutazione(idEvaluation, false, owner);
            valutazione.VistaResponsabile = false;
            valutazione.CanModify = false;

            return valutazione;
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

            if (owner=="Autovalutazione")
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

            if (!DBHelper.Save(db, CommonManager.GetCurrentUserMatricola()))
                return Content("Errore durante il salvataggio della valutazione");

            return Content("OK");
        }

        public ActionResult Save_Valutazione_PresaVisione(int evaluation, string note, bool approved)
        {
            IncentiviEntities db = new IncentiviEntities();
            int idPersona = CommonManager.GetCurrentIdPersona();

            var eval = db.XR_VAL_EVALUATION.Find(evaluation);
            var sint = db.SINTESI1.Find(idPersona);

            foreach (var item in db.XR_VAL_EVALUATION_NOTE.Where(x => x.ID_EVALUATION == evaluation && x.COD_TIPO == "PresaVisione" && (x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now)))
            {
                item.VALID_DTA_END = DateTime.Now;
                item.COD_USER = CommonManager.GetCurrentUserPMatricola();
                item.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
                item.TMS_TIMESTAMP = DateTime.Now;
            }
            foreach (var item in db.XR_VAL_OPER_STATE.Where(x => x.ID_EVALUATION == evaluation && x.ID_STATE == (int)ValutazioniState.PresaVisione))
            {
                item.VALID_DTA_END = DateTime.Now;
                item.COD_USER = CommonManager.GetCurrentUserPMatricola();
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
                COD_TIPO = "PresaVisione",
                COD_USER = CommonManager.GetCurrentUserPMatricola(),
                COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress,
                TMS_TIMESTAMP = DateTime.Now
            };
            db.XR_VAL_EVALUATION_NOTE.Add(nota);

            XR_VAL_OPER_STATE state = new XR_VAL_OPER_STATE()
            {
                ID_OPER = db.XR_VAL_OPER_STATE.GeneraPrimaryKey(),
                ID_PERSONA = sint.ID_PERSONA,
                ID_EVALUATION = eval.ID_EVALUATION,
                ID_STATE = (int)ValutazioniState.PresaVisione,
                VALID_DTA_INI = DateTime.Now,
                VALID_DTA_END = null,
                COD_USER = CommonManager.GetCurrentUserPMatricola(),
                COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress,
                TMS_TIMESTAMP = DateTime.Now
            };
            db.XR_VAL_OPER_STATE.Add(state);

            if (!DBHelper.Save(db, CommonManager.GetCurrentUserMatricola()))
                return Content("Errore durante il salvataggio della valutazione");

            return Content("OK");
        }
        [HttpPost]
        public ActionResult Save_Piano(PianoContainer piano)
        {
            IncentiviEntities db = new IncentiviEntities();
            var eval = db.XR_VAL_EVALUATION.Find(piano.IdValutazione);

            PianoSviluppo_Base tmp = (PianoSviluppo_Base)piano.Piano;
            tmp.IdPersonaAutore = CommonManager.GetCurrentIdPersona();
            tmp.MatricolaAutore = CommonManager.GetCurrentUserMatricola();
            tmp.RuoloAutore = eval.ID_PERSONA == tmp.IdPersonaAutore ? "Valutato" : "Valutatore";

            string serPiano = Newtonsoft.Json.JsonConvert.SerializeObject(piano.Piano);
            eval.COD_PIANOSVIL = piano.NomePiano;
            eval.OBJ_PIANOSVIL = serPiano;

            CezanneHelper.GetCampiFirma(out string user, out string termid, out DateTime tms);
            eval.COD_USER = user;
            eval.COD_TERMID = termid;
            eval.TMS_TIMESTAMP = tms;

            ValutazioniManager.SalvaStato(db, piano.IdValutazione, (int)ValutazioniState.SviluppoCompilato);

            if (!DBHelper.Save(db, CommonManager.GetCurrentUserMatricola()))
                return Content("Errore durante il salvataggio del piano");

            return Content("OK");
        }


        [HttpPost]
        public ActionResult Save_Piano_Approva(int evaluation, string note, bool approved)
        {
            IncentiviEntities db = new IncentiviEntities();
            int idPersona = CommonManager.GetCurrentIdPersona();

            var eval = db.XR_VAL_EVALUATION.Find(evaluation);

            foreach (var item in db.XR_VAL_EVALUATION_NOTE.Where(x => x.ID_EVALUATION == evaluation && x.COD_TIPO == "AnalisiPianoSviluppo" && (x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now)))
            {
                item.VALID_DTA_END = DateTime.Now;
                item.COD_USER = CommonManager.GetCurrentUserPMatricola();
                item.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
                item.TMS_TIMESTAMP = DateTime.Now;
            }

            XR_VAL_EVALUATION_NOTE nota = new XR_VAL_EVALUATION_NOTE()
            {
                ID_EVAL_NOTE = db.XR_VAL_EVALUATION_NOTE.GeneraPrimaryKey(),
                ID_PERSONA = idPersona,
                ID_EVALUATION = eval.ID_EVALUATION,
                NOT_TEXT = note ?? "",
                IND_APPROVED = approved,
                VALID_DTA_INI = DateTime.Now,
                VALID_DTA_END = null,
                COD_TIPO = "AnalisiPianoSviluppo",
                COD_USER = CommonManager.GetCurrentUserPMatricola(),
                COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress,
                TMS_TIMESTAMP = DateTime.Now
            };
            db.XR_VAL_EVALUATION_NOTE.Add(nota);

            if (approved)
                ValutazioniManager.SalvaStato(db, evaluation, (int)ValutazioniState.SviluppoApprovato);
            else
                ValutazioniManager.InvalidaStato(db, evaluation, (int)ValutazioniState.SviluppoCompilato);

            if (!DBHelper.Save(db, CommonManager.GetCurrentUserMatricola()))
                return Content("Errore durante il salvataggio della valutazione");

            return Content("OK");
        }
        #endregion

        #region GestioneSceltaValutatoreEsterno
        public ActionResult Modal_ValutatoreEsterno()
        {
            var model = ValutazioniManager.GetEvaluatorFromPerson(CommonManager.GetCurrentUserMatricola(), true);
            return View("subpartial/Modal_ValutatoreEsterno", model);
        }
        public ActionResult Save_ValutatoreEsterno(ValutatoreEsterno valutatoreEsterno)
        {
            string matr = CommonManager.GetCurrentUserMatricola();
            IncentiviEntities db = new IncentiviEntities();
            XR_VAL_EVALUATOR_EXT valExt = new XR_VAL_EVALUATOR_EXT()
            {
                ID_EVALUATOR_EXT = db.XR_VAL_EVALUATOR_EXT.GeneraPrimaryKey(),
            ID_PERSONA_VALUED = valutatoreEsterno.IdPersonaValued,
                ID_PERSONA_CHOOSEN = valutatoreEsterno.IdPersonaSel,
                ID_EVALUATOR = valutatoreEsterno.IdValutatore,
                NOT_REQUEST = valutatoreEsterno.NoteRequest,
                VALID_DTA_INI = DateTime.Now,
                VALID_DTA_END = null,
                COD_USER = CommonManager.GetCurrentUserPMatricola(),
                COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress,
                TMS_TIMESTAMP = DateTime.Now
            };

            db.XR_VAL_EVALUATOR_EXT.Add(valExt);

            if (!DBHelper.Save(db, matr))
                return Content("Errore durante il salvataggio della richiesta");

            return Content("OK");
        }
        #endregion

        #region GestioneRichiesteValutatoreEsterno
        public ActionResult Widget_ExtEvaluator()
        {
            var sheetContainer = ValutazioniManager.GetSheetContainers(CommonManager.GetCurrentUserMatricola(), new RicercaValutazione() { HasFilter = true, RichiestaExtVal = (int)ValutazioniExtEval.ConValutatoreEsterno }, false);
            return View("subpartial/Widget_ValutatoreEsterno", sheetContainer);
        }
        public ActionResult Save_RichiestaValutatoreEsterno(ValutatoreEsterno valutatoreEsterno)
        {
            IncentiviEntities db = new IncentiviEntities();

            XR_VAL_EVALUATOR_EXT valExt = db.XR_VAL_EVALUATOR_EXT.FirstOrDefault(x => x.ID_EVALUATOR_EXT == valutatoreEsterno.IdExtVal);
            valExt.IND_APPROVED = valutatoreEsterno.Approved;
            valExt.NOT_APPROVED = valutatoreEsterno.NoteApproved;
            valExt.COD_USER = CommonManager.GetCurrentUserPMatricola();
            valExt.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
            valExt.TMS_TIMESTAMP = DateTime.Now;

            if (valutatoreEsterno.Approved.GetValueOrDefault())
            {
                XR_VAL_EVALUATOR evaluator = new XR_VAL_EVALUATOR()
                {
                    ID_EVALUATOR = db.XR_VAL_EVALUATOR.GeneraPrimaryKey(),
                    ID_CAMPAIGN_SHEET = valExt.XR_VAL_EVALUATOR.ID_CAMPAIGN_SHEET,
                    ID_CAMPAIGN = valExt.XR_VAL_EVALUATOR.ID_CAMPAIGN,
                    ID_PERSONA = valExt.ID_PERSONA_CHOOSEN,
                    VALID_DTA_INI = DateTime.Now,
                    VALID_DTA_END = null,
                    COD_USER = CommonManager.GetCurrentUserPMatricola(),
                    COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress,
                    TMS_TIMESTAMP = DateTime.Now
                };
                valExt.ID_EVALUATOR_CHOOSEN = evaluator.ID_EVALUATOR;

                db.XR_VAL_EVALUATOR.Add(evaluator);
            }

            if (!DBHelper.Save(db, CommonManager.GetCurrentUserMatricola()))
                return Content("Errore durante " + (valExt.IND_APPROVED.GetValueOrDefault() ? "l'approvazione" : "il rifiuto") + " della richiesta");

            return Content("OK");
        }
        #endregion

        #region ListItem
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
        public static List<ListItem> GetSelectDelega()
        {
            return ValutazioniManager.GetSelectDeleghe();
        }
        public static List<ListItem> GetSelectExtVal()
        {
            return ValutazioniManager.GetSelectExtVal();
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

            var iniziative = ValutazioniManager.GetCampagne(CommonManager.GetCurrentUserMatricola()).Where(x => x.IsActive());

            lists.AddRange(iniziative.Select(x => new ListItem() { Value = x.ID.ToString(), Text = x.Name }));

            return lists;
        }
        public static List<SelectListGroup> GetSelectCampagneSchede()
        {
            List<SelectListGroup> list = new List<SelectListGroup>();

            var activeRoles = ValutazioniManager.GetActiveRoles(null, CommonManager.GetCurrentUserMatricola(), false).Where(x => x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now)
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

        public static List<SelectListItem> GetPersonList(int idPersonaActualEval)
        {
            List<SelectListItem> list = new List<SelectListItem>();

            DateTime today = DateTime.Today;

            using (IncentiviEntities db = new IncentiviEntities())
            {
                string currentUserMatricola = CommonManager.GetCurrentUserMatricola();
                var filterQual = ValutazioniManager.FilterDifferentEvaluator(db, currentUserMatricola);

                var me = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == currentUserMatricola);

                list.AddRange(db.SINTESI1.Where(filterQual).Where(x => x.ID_PERSONA != idPersonaActualEval && x.COD_SERVIZIO == me.COD_SERVIZIO && x.ID_UNITAORG != me.ID_UNITAORG && x.DTA_FINE_CR > today).ToList()
                    .OrderBy(x => x.Nominativo())
                    .Select(x => new SelectListItem()
                    {
                        Value = x.ID_PERSONA.ToString(),
                        Text = x.Nominativo()
                    }));
            }

            return list;
        }
        #endregion

        [HttpPost]
        public ActionResult GetCorsi(string[] areeMiglioramento)
        {
            var corsi = ValutazioniManager.GetCorsi(areeMiglioramento);
            return PartialView("pianosviluppo/PianoSviluppo_Corsi", corsi);
        }
    }
}
