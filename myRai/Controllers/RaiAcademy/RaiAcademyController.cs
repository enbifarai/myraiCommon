using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using myRaiData;
using myRaiDataTalentia;
using System.Text.RegularExpressions;
using myRai.DataAccess;
using System.IO;
using iTextSharp.text.pdf;
using iTextSharp.text;
using myRaiIlias;
using myRaiCommonModel.RaiAcademy;
using myRaiHelper;
using myRaiCommonModel.raiplace;
using myRaiCommonModel;
using myRaiCommonModel.cvModels.Pdf;
using myRaiServiceHub.it.rai.servizi.sendmail;
using myRaiCommonModel.DataControllers.RaiAcademy;
using myRai.Business;
using myRai.Models;
using myRaiCommonManager;

namespace myRai.Controllers.RaiAcademy
{
    //Questo controller contiene le action relative alla pagina di dettaglio del corso e della dashboard
    public partial class RaiAcademyController : Controller
    {
        private static bool apply_request_oper = false;

        private RaiAcademyDataController dataController = new RaiAcademyDataController();

		/// <summary>
		/// Render dell'elenco dei corsi disponibili
		/// </summary>
		/// <returns></returns>
		public PartialViewResult CatalogoCorsi ()
		{
			return PartialView();
		}

        [HttpPost]
        public ActionResult AccettazionePrivacy()
        {
            bool result = RaiAcademyManager.PrivacyAcceptance(CommonManager.GetCurrentUserMatricola(), out string errore);
            if (result)
                errore = "OK";
            return Content(errore);
        }

		/// <summary>
		/// Render del dettaglio di un corso
		/// </summary>
		/// <param name="idCorso">Identificativo del corso di cui si intendono visualizzare le informazioni</param>
		/// <returns></returns>
		public ActionResult DettaglioCorso ( int idCorso )
		{
            System.Web.HttpContext.Current.Session["RaiAcademyBack"] = true;

            //System.Web.HttpContext.Current.Session["RaiAcademyTest"] = true;

			try
			{
                RaiAcademyControllerScope.Instance.IdCorso = idCorso;
				DettaglioCorsoVM model = new DettaglioCorsoVM();

                var tmp = dataController.EstraiCorsi(CommonHelper.GetCurrentUserMatricola(), true, idCorso, true, true);
                if (tmp!=null && tmp.Count()>0)
                {
                    model.Corso = tmp.First();
                    if (model.Corso.AltriDettagli.PercorsiFormativi.Any())
                        model.Consigliati = new CorsoList() { Items = this.dataController.GetCorsiConsigliati(CommonManager.GetCurrentUserMatricola(), idCorso, 3, 1), PageSize = 3 };


                    model.Privacy = RaiAcademyManager.GetPrivacyData(CommonManager.GetCurrentUserMatricola());

                    return PartialView("~/Views/RaiAcademy/DettaglioCorso.cshtml", model);
                }
                else
                {
                    return View("~/Views/Shared/404.cshtml");
                }
			}
			catch ( Exception ex )
			{
				throw new Exception( ex.Message );
			}
		}

        public PartialViewResult PreviewCorso(int id)
        {
            try
            {
                RaiAcademyControllerScope.Instance.IdCorso = id;
                DettaglioCorsoVM model = new DettaglioCorsoVM();
                model.Corso = this.dataController.GetDettaglioCorso(CommonManager.GetCurrentUserMatricola(), id, true);
                if (model.Corso.AltriDettagli.PercorsiFormativi.Any())
                {
                    model.Consigliati = new CorsoList() { Items = this.dataController.GetCorsiConsigliati(CommonManager.GetCurrentUserMatricola(), id, 3, 1, true), PageSize = 3 };

                    if (TempData.Keys.Contains("academy_corso_preview_cons"))
                        TempData["academy_corso_preview_cons"] = true;
                    else
                        TempData.Add("academy_corso_preview_cons", true);
                }

                if (TempData.Keys.Contains("academy_corso_preview_att"))
                    TempData["academy_corso_preview_att"] = true;
                else
                    TempData.Add("academy_corso_preview_att", true);

                return PartialView("~/Views/RaiAcademy/DettaglioCorso.cshtml", model);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public ActionResult ElencoPillole(int idCorso, string extCourse, IscrittoEnum iscritto, StatoCorsoTipoOffertaEnum stato)
        {

            Pacchetto model = new Pacchetto();

            model.IdCorso = idCorso;
            model.Iscritto = iscritto;
            model.TipoOfferta = stato;
            string[] paramIlias = CommonManager.GetParametri<string>(EnumParametriSistema.AcademyUrlIlias);

            //Se il corso è obbligatorio e a distanza, nel caso in cui l'edizione attiva sia marchiata con COD_AUTHORIZATION="M"
            //bisogna verificare l'iscrizione (ad opera della formazione)
            bool skipLoad = false;
            if (iscritto == IscrittoEnum.NonIscritto && stato == StatoCorsoTipoOffertaEnum.Obbligatoria)
            {
                var db = new myRaiDataTalentia.TalentiaEntities();
                var corso = db.CORSO.Find(idCorso);
                if (RaiAcademyDataController.GetTipoMetodoDidattico(corso.TB_MTDDID.DES_METODODID)==MetodoEnum.FAD)
                {
                    var ediz = corso.EDIZIONE.Where(x => x.DTA_INIZIO >= DateTime.Now || x.DTA_FINE >= DateTime.Now).OrderBy(y => y.DTA_INIZIO).Distinct();
                    if (ediz != null && ediz.FirstOrDefault(x => x.DTA_INIZIO <= DateTime.Now && DateTime.Now < x.DTA_FINE).COD_AUTHORIZATION == "M")
                        skipLoad = true;
                }
            }

            if (!skipLoad)
            {
                Ilias ilias = new Ilias(paramIlias[0], paramIlias[1]);
                var accountsService = CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio);
                var accountsServiceIlias = CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizioIlias);
                ilias.SetCredentials(accountsService[0], accountsService[1], "RAI");
                ilias.Login(accountsServiceIlias[0], accountsServiceIlias[1]);

                string pMatricola = CommonManager.GetCurrentUserPMatricola();

                List<string> resources = null;
            if (ilias.GetResource(extCourse, ResourceType.Scorm, pMatricola, out resources))
                {
                    foreach (string resource in resources)
                    {
                        Risorsa ris = new Risorsa();
                        string[] elems = resource.Split('|');
                        if (elems.Length > 3 )
                        {
                            ris.Prog = Convert.ToInt32(elems[0]);
                            ris.Nome = elems[1];
                            ris.Url = elems[2];
                            ris.Completato = elems[3] == "1" ? true : false;
                            ris.Iniziato = elems[4] == "1" ? true : false;
                            ris.Vincoli = new List<int>();
                            if (elems.Length > 5)
                            {
                                if (!String.IsNullOrWhiteSpace(elems[5]))
                                    ris.Vincoli.AddRange(elems[5].Split(',').Select(x => Convert.ToInt32(x)));

                                if (!String.IsNullOrWhiteSpace(elems[6]))
                                    ris.IliasId = Convert.ToInt32(elems[6]);
                            }
                            if (elems.Length > 7)
                            {
                                if (!String.IsNullOrWhiteSpace(elems[7]))
                                    ris.Tipo = elems[7];
                            }

                            model.Pillole.Add(ris);
                        }
                    }

                    model.Pillole = model.Pillole.OrderBy(x => x.Prog).ToList();

                    foreach (var item in model.Pillole)
                    {
                        //item.Abilitato = item.Vincoli.Count == 0 || item.Vincoli.All(x => model.Pillole[x - 1].Completato);
                        item.Abilitato = item.Vincoli.Count == 0 || item.Vincoli.All(x => model.Pillole.FirstOrDefault(y => y.IliasId == x) != null ? model.Pillole.FirstOrDefault(y => y.IliasId == x).Completato : true);
                    }
                    }
                }

            return PartialView("~/Views/RaiAcademy/subpartial/ElencoPillole.cshtml", model);
        }
        public ActionResult ElencoRisorse(int idCorso, string extCourse, IscrittoEnum iscritto, StatoCorsoTipoOffertaEnum stato)
        {
            Materiali model = new Materiali();
            
            model.IdCorso = idCorso;
            model.Iscritto = iscritto;
            model.TipoOfferta = stato;
            string[] paramIlias = CommonManager.GetParametri<string>(EnumParametriSistema.AcademyUrlIlias);

            //Se il corso è obbligatorio e a distanza, nel caso in cui l'edizione attiva sia marchiata con COD_AUTHORIZATION="M"
            //bisogna verificare l'iscrizione (ad opera della formazione)
            bool skipLoad = false;
            if (iscritto == IscrittoEnum.NonIscritto && stato == StatoCorsoTipoOffertaEnum.Obbligatoria)
            {
                var db = new myRaiDataTalentia.TalentiaEntities();
                var corso = db.CORSO.Find(idCorso);
                if (RaiAcademyDataController.GetTipoMetodoDidattico(corso.TB_MTDDID.DES_METODODID) == MetodoEnum.FAD)
                {
                    var ediz = db.EDIZIONE.Where(x => x.DTA_INIZIO >= DateTime.Now || x.DTA_FINE >= DateTime.Now).OrderBy(y => y.DTA_INIZIO).Distinct();
                    if (ediz != null && ediz.FirstOrDefault(x => x.DTA_INIZIO <= DateTime.Now && DateTime.Now < x.DTA_FINE).COD_AUTHORIZATION == "M")
                        skipLoad = true;
                }
            }

            if (!skipLoad)
            {
                Ilias ilias = new Ilias(paramIlias[0], paramIlias[1]);
                var accountsService = CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio);
                var accountsServiceIlias = CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizioIlias);
                ilias.SetCredentials(accountsService[0], accountsService[1], "RAI");
                ilias.Login(accountsServiceIlias[0], accountsServiceIlias[1]);

                string pMatricola = CommonManager.GetCurrentUserPMatricola();

                List<string> resources = null;
            if (ilias.GetResource(extCourse, ResourceType.File, pMatricola, out resources))
                {
                    foreach (string resource in resources)
                    {
                        Risorsa ris = new Risorsa();
                        string[] elems = resource.Split('|');
                        if (elems.Length > 3)
                        {
                            ris.Prog = Convert.ToInt32(elems[0]);
                            ris.Nome = elems[1];
                            ris.Url = elems[2];
                            ris.Tipo = elems[3];
                            if (elems.Length>4) ris.Completato = elems[4] == "1" ? true : false;
                            if (elems.Length > 5) ris.Iniziato = elems[5] == "1" ? true : false;
                            model.Risorse.Add(ris);
                        }
                    }
                }
            }

            return PartialView("~/Views/RaiAcademy/subpartial/ElencoRisorse.cshtml", model);
        }

        [HttpPost]
        public ActionResult ToggleIncuriosisce(int idCorso)
        {
            TalentiaEntities db = new TalentiaEntities();
            if (!db.CORSO.Any(x => x.ID_CORSO == idCorso))
                return Content("Corso non trovato");

            string matricola = CommonHelper.GetCurrentUserMatricola();

            bool changed = false;

            XR_CORSOINTPERSONA pref = null;

            //Aggiungo l'iscrizione
            pref = db.XR_CORSOINTPERSONA.FirstOrDefault(x => x.MATRICOLA == matricola && x.ID_CORSO == idCorso);
            if (pref == null)
            {
                //recupero l'id della persona
                var pers = db.V_XR_XANAGRA.FirstOrDefault(x => x.MATRICOLA == matricola);
                if (pers == null)
                    return Content("Matricola non identificata");

                pref = new XR_CORSOINTPERSONA();
                //calcolo l'id per la tabella
                pref.ID_CORSOINTPERSONA = db.XR_CORSOINTPERSONA.GeneraPrimaryKey();
                pref.ID_PERSONA = pers.ID_PERSONA;
                pref.MATRICOLA = matricola;
                pref.ID_CORSO = idCorso;
                db.XR_CORSOINTPERSONA.Add(pref);

                changed = true;
            }
            else
            {
                db.XR_CORSOINTPERSONA.Remove(pref);
                changed = true;
            }

            if ( changed && !DBHelper.Save( db , CommonHelper.GetCurrentUserMatricola( ), "Rai Academy" ) )
                return Content( "Errore durante il salvataggio" );

            return Content("OK");
        }

        [HttpGet]
        public ActionResult GetRisorsa(int id)
        {
            TalentiaEntities db = new TalentiaEntities();
            CZNDOCS doc = db.CZNDOCS.FirstOrDefault(x => x.ID_DOC == id);
            if (doc == null)
                return View("~/Views/Shared/404.cshtml");
            else
            {
                return File(new MemoryStream(doc.OBJ_OBJECT), "application/pdf");
            }
        }

        public ActionResult OpenScorm(int idCorso, string url)
        {
            string typeOpen = "redirect";
            string msgError = "";

            bool isIliasCourse = false;

            TalentiaEntities db = new TalentiaEntities();
            CORSO course = db.CORSO.FirstOrDefault(x => x.ID_CORSO == idCorso);

            string pMatricola = CommonManager.GetCurrentUserPMatricola();
            string matricola = CommonManager.GetCurrentUserMatricola();
            int idPersona = 0;
            var tmpPersona = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == matricola);
            if (tmpPersona != null)
                idPersona = tmpPersona.ID_PERSONA;

            bool isAuthorized = ControlloVincoliAccesso(course, tmpPersona, out msgError);

            if (isAuthorized)
            {
            if (!String.IsNullOrWhiteSpace(course.COD_CORSOLMS) && url.ToLower().Contains("ilias"))
            {
                isIliasCourse = true;

                bool isAssigned = false;
                bool addCurrForm = false;
                bool canAccess = true;

                //if (course.IND_OBBLIGATORIO == "3")
                //{
                //    if (course.EDIZIONE.Count() > 0)
                //    {
                //        int idEdiz = course.EDIZIONE.FirstOrDefault().ID_EDIZIONE;
                //        canAccess = idPersona > 0 && db.CURRFORM.Any(x => x.ID_EDIZIONE == idEdiz && x.ID_PERSONA == idPersona);
                //    }
                //}

                if (canAccess)
                {
                        string[] paramIlias = CommonManager.GetParametri<string>(EnumParametriSistema.AcademyUrlIlias);

                    Ilias ilias = new Ilias(paramIlias[0], paramIlias[1]);
                        var accountsService = CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio);
                        var accountsServiceIlias = CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizioIlias);
                    ilias.SetCredentials(accountsService[0], accountsService[1], "RAI");
                    ilias.Login(accountsServiceIlias[0], accountsServiceIlias[1]);

                    if (ilias.IsAssignToCourse(course.COD_CORSOLMS, pMatricola, out isAssigned))
                    {
                        if (!isAssigned)
                        {
                            isAssigned = ilias.AssignCourseMember(course.COD_CORSOLMS, pMatricola);
                            //if (isAssigned)
                            //addCurrForm = idPersona > 0;
                        }                       


                    }

                    if (isAssigned)
                    {
                        //verifica se iscrizione presente su CURRFORM
                        if (course.EDIZIONE.Count() > 0)
                        {
                            int idEdiz = course.EDIZIONE.FirstOrDefault().ID_EDIZIONE;
                            addCurrForm = idPersona > 0 && !db.CURRFORM.Any(x => x.ID_EDIZIONE == idEdiz && x.ID_PERSONA == idPersona);
                        }

                        if (addCurrForm)
                        {
                            CURRFORM newRec = new CURRFORM();
                            newRec.ID_CURRFORM = db.CURRFORM.GeneraPrimaryKey();
                            newRec.ID_PERSONA = idPersona;
                            newRec.ID_EDIZIONE = course.EDIZIONE.FirstOrDefault() != null ? course.EDIZIONE.FirstOrDefault().ID_EDIZIONE : 0;
                            newRec.DTA_STARTEDON = DateTime.Now;
                            newRec.IND_PARTICIPANT = "N";
                            newRec.COD_UNITAMIS = "O";
                            newRec.IND_STATOEVENTO = "E";
                            newRec.IND_ISDEDUCTABLE = "Y";
                            newRec.QTA_COMPLETION = 1;
                            newRec.COD_USER = "HRPROF";
                            newRec.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
                            newRec.TMS_TIMESTAMP = DateTime.Now;
                            db.CURRFORM.Add(newRec);
                            DBHelper.Save( db , CommonHelper.GetCurrentUserMatricola( ), "Rai Academy");
                        }
                    }
                    else
                    {
                        msgError = "Impossibile completare l'iscrizione al corso";
                    }
                }
                else
                {
                    msgError = "Non sei abilitato a visualizzare questo corso";
                }

                    string openWithModal = CommonManager.GetParametro<string>(EnumParametriSistema.AcademyModalScorm);
                if (isAssigned)
                {   
                    if (!String.IsNullOrWhiteSpace(openWithModal) && openWithModal=="TRUE")
                        typeOpen = "modal";
                }
                else
                    typeOpen = "error";
            }
            }
            else
            {
                typeOpen = "error";
            }
    
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { typeOpen = typeOpen, messageError = msgError, checkIliasIE = isIliasCourse && (Request.Browser.Browser == "IE" || Request.Browser.Browser == "InternetExplorer") }
            };

            //return Content(typeOpen);
        }

        private static bool ControlloVincoliAccesso(CORSO course, SINTESI1 tmpPersona, out string msgError)
        {
            bool isAuthorized = true;
            msgError = "";
                        
            var vincoloSocieta = course.GENNOTES.FirstOrDefault(x => x.COD_GRPNOTE == "A002" && x.COD_TPNOTE == "SOCIET");
            if (vincoloSocieta != null)
            {
                isAuthorized = tmpPersona != null && vincoloSocieta.NOT_NOTE.Contains(tmpPersona.COD_IMPRESACR);
                if (!isAuthorized)
                    msgError = "Accesso non consentito. Verifica le informazioni all’interno della scheda";
            }
            
            return isAuthorized;
        }

        public ActionResult OpenResource(int idCorso, string url)
        {
            string typeOpen = "redirect";
            string messageError = "";

            bool isIliasCourse = false;

            TalentiaEntities db = new TalentiaEntities();
            CORSO course = db.CORSO.FirstOrDefault(x => x.ID_CORSO == idCorso);
            
            string pMatricola = CommonManager.GetCurrentUserPMatricola();
            string matricola = CommonManager.GetCurrentUserMatricola();
            int idPersona = 0;
            var tmpPersona = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == matricola);
            if (tmpPersona != null)
                idPersona = tmpPersona.ID_PERSONA;

            bool isAuthorized = ControlloVincoliAccesso(course, tmpPersona, out messageError);

            if (isAuthorized)
            {
            if (!String.IsNullOrWhiteSpace(course.COD_CORSOLMS) && url.ToLower().Contains("ilias"))
            {
                isIliasCourse = true;

                bool isAssigned = false;
                bool canAccess = true;

                //if (course.IND_OBBLIGATORIO == "3")
                //{
                //    if (course.EDIZIONE.Count() > 0)
                //    {
                //        int idEdiz = course.EDIZIONE.FirstOrDefault().ID_EDIZIONE;
                //        canAccess = idPersona > 0 && db.CURRFORM.Any(x => x.ID_EDIZIONE == idEdiz && x.ID_PERSONA == idPersona);
                //    }
                //}

                if (canAccess)
                {
                        string[] paramIlias = CommonManager.GetParametri<string>(EnumParametriSistema.AcademyUrlIlias);

                    Ilias ilias = new Ilias(paramIlias[0], paramIlias[1]);
                        var accountsService = CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio);
                        var accountsServiceIlias = CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizioIlias);
                    ilias.SetCredentials(accountsService[0], accountsService[1], "RAI");
                    ilias.Login(accountsServiceIlias[0], accountsServiceIlias[1]);


                    
                    if (ilias.IsAssignToCourse(course.COD_CORSOLMS, pMatricola, out isAssigned))
                    {
                        if (!isAssigned)
                        {
                            isAssigned = ilias.AssignCourseMember(course.COD_CORSOLMS, pMatricola);
                        }
                    }
                }
                else
                {
                    messageError = "Non sei abilitato a visualizzare questa risorsa";
                }

                    string openWithModal = CommonManager.GetParametro<string>(EnumParametriSistema.AcademyModalScorm);
                if (isAssigned)
                {
                        //if (!String.IsNullOrWhiteSpace(openWithModal) && openWithModal == "TRUE")
                        //    typeOpen = "modal";
                }
                else
                    typeOpen = "error";
                }
            }
            else
            {
                typeOpen = "error";
            }

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { typeOpen = typeOpen, messageError = messageError, checkIliasIE = isIliasCourse && (Request.Browser.Browser == "IE" || Request.Browser.Browser == "InternetExplorer") }
            };
        }

        
        public ActionResult Test()
        {
            bool? check = (bool)System.Web.HttpContext.Current.Session["RaiAcademyTest"];

            if (check.HasValue && check.Value)
            {
                System.Web.HttpContext.Current.Session["RaiAcademyTest"] = false;
                return Redirect("http://iliasprod.intranet.rai.it/data/IliasClient/lm_data/lm_18263/1.1.mp4");
            }
            else
            {
                return new RedirectResult("/Home/notAuth");
            }
        }

        public ActionResult CloseScorm(int idCorso)
        {
            CheckCourseStatus(idCorso);
            return Content("OK");
        }

        private static void CheckCourseStatus(int idCorso)
        {
            TalentiaEntities db = new TalentiaEntities();
            CORSO course = db.CORSO.FirstOrDefault(x => x.ID_CORSO == idCorso);

            if (!String.IsNullOrWhiteSpace(course.COD_CORSOLMS))
            {
                string matricola = CommonManager.GetCurrentUserMatricola();
                string pMatricola = CommonManager.GetCurrentUserPMatricola();

                int idPersona = 0;
                var tmpPersona = db.V_XR_XANAGRA.FirstOrDefault(x => x.MATRICOLA == matricola);
                if (tmpPersona != null)
                    idPersona = tmpPersona.ID_PERSONA;

                if (idPersona > 0)
                {
                    int idEdiz = course.EDIZIONE.FirstOrDefault().ID_EDIZIONE;
                    CURRFORM progress = db.CURRFORM.FirstOrDefault(x => x.ID_PERSONA == idPersona && x.ID_EDIZIONE == idEdiz);

                    string[] paramIlias = CommonManager.GetParametri<string>(EnumParametriSistema.AcademyUrlIlias);

                    Ilias ilias = new Ilias(paramIlias[0], paramIlias[1]);
                    var accountsService = CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio);
                    var accountsServiceIlias = CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizioIlias);
                    ilias.SetCredentials(accountsService[0], accountsService[1], "RAI");
                    ilias.Login(accountsServiceIlias[0], accountsServiceIlias[1]);

                    List<LearningChanges> lpChanges = null;
                    if (ilias.GetProgressChanges(course.COD_CORSOLMS, pMatricola, progress.DTA_STARTEDON, out lpChanges))
                    {
                        if (lpChanges.Count > 0 && lpChanges.Last().Stato == ProgressStatus.Completato)
                        {
                            progress.QTA_COMPLETION = 100;
                            progress.DTA_COMPLETEDON = lpChanges.Last().Data;
                            DBHelper.Save( db , CommonHelper.GetCurrentUserMatricola( ), "Rai Academy");
                        }
                    }
                }
            }
        }

        public static string RenderText(string source)
        {
            string result = source;
            result = result.Replace("/$", "<ul>");
            result = result.Replace("$/", "</ul>");
            result = Regex.Replace(result, @"^\$(.+)$", "<li>$1</li>", RegexOptions.Multiline);
            result = Regex.Replace(result, @"\*([^\*]*)\*", "<span class='academy-tab-body-bold'>$1</span>");
            result = Regex.Replace(result, @"\^([^\^]*)\^", "<i>$1</i>");
            result = result.Replace("\r\n", "<br/>");
            result = result.Replace("<br/><li>", "<li>");
            result = result.Replace("</ul><br/>", "</ul>");
            return result;
        }

        /// <summary>
        /// Reperimento dei corsi consigliati
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public PartialViewResult ElencoCorsiConsigliati()
        {
            try
            {
                int idCorso = RaiAcademyControllerScope.Instance.IdCorso;
                var tmp = TempData["academy_corso_preview_cons"];
                bool isPreview = tmp != null && (bool)tmp;
                CorsoList model = new CorsoList() { Items = dataController.GetCorsiConsigliati(CommonManager.GetCurrentUserMatricola(), idCorso, 3, 1, isPreview), PageSize = 3 };

                return PartialView("~/Views/RaiAcademy/ElencoCorsi.cshtml", model);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Reperimento dei corsi attinenti in base all'area di appartenenza del
        /// corso esaminato
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public PartialViewResult ElencoCorsiAttinentiArea(int idCorso, int area)
        {
            try
            {
                //int idCorso = RaiAcademyControllerScope.Instance.IdCorso;
                var tmp = TempData["academy_corso_preview_att"];
                bool isPreview = tmp != null && (bool)tmp;
                CorsoList model = new CorsoList() { Items = dataController.GetCorsiAttinenti(CommonManager.GetCurrentUserMatricola(), idCorso, area, 3, 1, isPreview), PageSize = 3 };

                return PartialView("~/Views/RaiAcademy/ElencoCorsi.cshtml", model);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #region GestioneIscrizione
        public ActionResult RichiediIscrizione(int idCorso, int idEdiz = 0)
        {
            RichiestaIscrizione rich = new RichiestaIscrizione();
            rich.FromCatalogue = true;
            rich.Corso = this.dataController.GetDettaglioCorso(CommonHelper.GetCurrentUserMatricola(), idCorso);
            rich.IdEdizione = idEdiz;
            rich.TitoloCorso = rich.Corso.Titolo;

            return PartialView("~/Views/RaiAcademy/subpartial/Modale_RichiestaIscrizione.cshtml", rich);
        }

        public ActionResult SubmitIscrizione(RichiestaIscrizione model)
        {
            string matricola = CommonManager.GetCurrentUserMatricola();

            string mailFrom = "";
            string mailBody = "";
            using (digiGappEntities db = new digiGappEntities())
            {
                mailBody = db.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "MailRichiestaCorsoCatBody").Valore1;
            }

            MailSender invia = new MailSender();
            Email eml = new Email();

            eml.From = CommonManager.GetEmailPerMatricola(matricola);
            eml.toList = new string[] { CommonManager.GetEmailPerMatricola(model.DestinatarioRichiesta) };
            eml.ccList = new string[] { CommonManager.GetEmailPerMatricola(matricola) };
            eml.ContentType = "text/html";
            eml.Priority = 2;
            eml.SendWhen = DateTime.Now.AddSeconds(1);

            model.Corso = this.dataController.EstraiCorsi(CommonManager.GetCurrentUserMatricola(), true, model.Corso.Id).First();

            string corsoDescr = model.Corso.Descrizione;
            string tipologiaCorso = "";
            if (model.Corso.TipoMetodoFormativo == MetodoEnum.FAD)
                tipologiaCorso += "Formazione a distanza - ";
            else if (model.Corso.TipoMetodoFormativo == MetodoEnum.FPRES)
                tipologiaCorso += "Corso in presenza - ";
            tipologiaCorso += model.Corso.MetodoFormativo;

            corsoDescr += "<br/>" + tipologiaCorso;

            if (model.Corso.Durata != null)
            {
                corsoDescr += "<br/>Durata: " + model.Corso.Durata;
            }

            string corsoLink = System.Web.HttpContext.Current.Request.Url.Scheme + "://" + System.Web.HttpContext.Current.Request.Url.Authority + "/raiacademy/dettagliocorso?idCorso=" +model.Corso.Id.ToString();

            string edizDetail = "";
            string edizNome = "";
           
            if (model.IdEdizione > 0)
            {
                edizDetail = "<br/>";
                var edizione = model.Corso.AltriDettagli.Edizioni.FirstOrDefault(x => x.IdEdizione == model.IdEdizione);
                edizNome = edizione.Nome;
                edizDetail += edizione.Nome + " - ";
                if (edizione.DataInizio != edizione.DataFine)
                    edizDetail += String.Format("dal {0:d MMMM yyyy} al {1:d MMMM yyyy}", edizione.DataInizio, edizione.DataFine);
                else
                    edizDetail += String.Format("{0:d MMMM yyyy}", edizione.DataInizio);

                edizDetail += "<br/>" + edizione.Luogo;
                if (edizione.DesLuogo != "")
                    edizDetail += " - " + edizione.DesLuogo;

                if (edizione.Orario != "")
                    edizDetail += "<br/>" + edizione.Orario;
            }

            string[] inquadramento = Utente.EsponiAnagrafica()._inquadramento.Split(';');
            string addPrefix = "&#9675;";
            for (int i = 0; i < inquadramento.Count(); i++)
            {
                for (int j = 0; j < i; j++)
                    inquadramento[i] = addPrefix + inquadramento[i];
            }

            eml.Subject = "Iscrizione corso '" + model.TitoloCorso + "'";
            eml.Body = mailBody;
            eml.Body = eml.Body.Replace("#corsoNome", model.TitoloCorso)
                        .Replace("#matricola", matricola)
                        .Replace("#ufficio", "<br/>" + String.Join("<br/>", inquadramento))
                        .Replace("#nominativo", CommonManager.GetNominativoPerMatricola(matricola))
                        .Replace("#dataRichiesta", DateTime.Today.ToString("dd/MM/yyyy"))
                        .Replace("#titoloCorso", model.TitoloCorso)
                        .Replace("#linkCorso", corsoLink)
                        .Replace("#descrizioneCorso", corsoDescr)
                        .Replace("#dettaglioEdizione", edizDetail)
                        .Replace("#motivazione", model.Motivazione)
                        .Replace("#noteAggiuntive", model.NoteAggiuntive);

            string[] AccountUtenteServizio = CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio);
            invia.Credentials = new System.Net.NetworkCredential(AccountUtenteServizio[0], AccountUtenteServizio[1], "RAI");

            try
            {
                invia.Send(eml);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }

            using (TalentiaEntities db = new TalentiaEntities())
            {
                SINTESI1 sint = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == matricola);

                TREQUESTS request = new TREQUESTS();
                request.ID_TREQUESTS = db.TREQUESTS.GeneraPrimaryKey();
                request.ID_TPRIORITY = 87814269; //Priorità 1
                request.ID_TPERIODS = 223838156; //Per il momento I Trimestre
                request.ID_TREASONS = 790331359;
                request.ID_UNITAORG = sint.ID_UNITAORG;
                request.ID_PERSONA = sint.ID_PERSONA;
                request.ID_CORSOPLAN = model.Corso.Id;
                request.ID_CORSO = model.Corso.Id;
                request.DES_NEED = model.Motivazione;
                request.DTA_DATE = DateTime.Today;
                request.NOT_NOTE = model.NoteAggiuntive + (model.IdEdizione > 0 ? "\r\nEdizione: " + edizNome : "");// +"\r\nDestinatario richiesta:" + model.DestinatarioRichiesta;
                request.NME_REQUEST = model.DestinatarioRichiesta;
                request.IND_SELFREQUEST = "Y";
                request.IND_DONE = "N";
                request.IND_HRPROF = "Y";
                request.COD_USER = "HRPROF";
                request.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
                request.TMS_TIMESTAMP = DateTime.Now;
                request.ID_REQUESTNEED = 1; //
                request.COD_SOURCE = "Employee";
                request.IND_MANAPPROVED = "N";
                request.IND_SYSTEM = "N";

                db.TREQUESTS.Add(request);
                
                if (apply_request_oper)
                {
                    TREQUESTS_STEP step = new TREQUESTS_STEP();
                    step.ID_TREQUESTS_STEP = db.TREQUESTS_STEP.GeneraOid(x => x.ID_TREQUESTS_STEP);
                    step.ID_TREQUESTS = request.ID_TREQUESTS;
                    step.ID_EDIZIONE = model.IdEdizione > 0 ? model.IdEdizione : model.Corso.AltriDettagli.Edizioni.Count() > 0 ? model.Corso.AltriDettagli.Edizioni.FirstOrDefault().IdEdizione : 0;
                    step.NUM_PROGR = 1;
                    step.ID_PERSONA_FROM = request.ID_PERSONA.Value;
                    step.ID_PERSONA_TO = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == model.DestinatarioRichiesta).ID_PERSONA;
                    step.DTA_RICHIESTA = DateTime.Now;
                    step.COD_USER = "";
                    step.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
                    step.TMS_TIMESTAMP = step.DTA_RICHIESTA;
                    request.TREQUESTS_STEP.Add(step);

                    //NotificheManager.InserisciNotifica("", "Academy", matricola, model.DestinatarioRichiesta, request.ID_TREQUESTS);
                    //NotificheManager.InserisciNotifica("", "Academy", matricola, matricola, request.ID_TREQUESTS);
                }

                DBHelper.Save( db , CommonHelper.GetCurrentUserMatricola( ), "Rai Academy");
            }

            return Content("ok");
        }

        /// <summary>
        /// Ricerca autocomplete, ricerca un utente a partire
        /// dal suo cognome
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetCognomiRisorsa()
        {
            string matricola = CommonManager.GetCurrentUserMatricola();
            string codServ = Utente.EsponiAnagrafica()._sezContabile;

            string cog = Request.Params["cog"];
            List<AnagSelect> lista = new List<AnagSelect>();

            // Se la parola cercata è uguale alla precedente
            // Oppure la vecchia parola cercata è parte della nuova + 1 char (es. vecchia "BIF" nuova "BIFA")
            // e la lista è già stata popolata per la vecchia parola ("BIF"), allora verranno filtrati i 
            // risultati già ottenuti senza dover chiamare nuovamente il servizio

            if (!String.IsNullOrEmpty(cog) &&
                cog.Equals(RaiAcademyControllerScope.Instance.FiltroRicercaUtenti, StringComparison.InvariantCultureIgnoreCase) &&
                (RaiAcademyControllerScope.Instance.AcademyAnagraficaUtenti != null &&
                RaiAcademyControllerScope.Instance.AcademyAnagraficaUtenti.Any()))
            {
                RaiAcademyControllerScope.Instance.AcademyAnagraficaUtenti.ForEach(r =>
                {
                    if (r.Matricola != matricola)
                    {
                        AnagSelect anag = new AnagSelect()
                        {
                            id = r.Matricola,
                            text = r.Cognome + "/" + r.Nome + "/" + r.Direzione,
                            matricola = r.Matricola
                        };

                        lista.Add(anag);
                    }
                });
            }
            else if (!String.IsNullOrEmpty(cog) &&
                        cog.StartsWith(RaiAcademyControllerScope.Instance.FiltroRicercaUtenti) &&
                        (RaiAcademyControllerScope.Instance.AcademyAnagraficaUtenti != null &&
                          RaiAcademyControllerScope.Instance.AcademyAnagraficaUtenti.Any()) &&
                        cog.Length > RaiAcademyControllerScope.Instance.FiltroRicercaUtenti.Length)
            {
                List<AcademyAnagraficaUtente> subList = RaiAcademyControllerScope.Instance.AcademyAnagraficaUtenti.Where(i => i.Nominativo.ToUpper().Contains(cog.ToUpper())).ToList();

                if (subList != null && subList.Any())
                {
                    RaiAcademyControllerScope.Instance.AcademyAnagraficaUtenti = new List<AcademyAnagraficaUtente>();

                    subList.ForEach(r =>
                    {
                        if (r.Matricola != matricola)
                        {
                            RaiAcademyControllerScope.Instance.AcademyAnagraficaUtenti.Add(new AcademyAnagraficaUtente()
                            {
                                Matricola = r.Matricola,
                                Nome = r.Nome,
                                Cognome = r.Cognome,
                                Direzione = r.Direzione,
                                Nominativo = r.Nominativo
                            });

                            AnagSelect anag = new AnagSelect()
                            {
                                id = r.Matricola,
                                text = r.Cognome + "/" + r.Nome + "/" + r.Direzione,
                                matricola = r.Matricola
                            };

                            lista.Add(anag);
                        }
                    });
                }
            }
            else
            {
                using (PERSEOEntities db = new PERSEOEntities())
                {
                    string struttura = CommonManager.GetStruttura(CommonManager.GetCurrentUserMatricola());

                    string categoria = Utente.Categoria();
                    string gerarServ = codServ;
                    if (categoria.StartsWith("A"))
                    {
                        string tmpServ = db.sp_GERARSERVIZIO(struttura).FirstOrDefault();
                        if (!String.IsNullOrWhiteSpace(tmpServ))
                            gerarServ = tmpServ.Replace(';', ',').TrimStart(' ', ',').TrimEnd(' ', ',');
                    }
                    else
                        gerarServ = codServ;

                    var rows = db.sp_RicercaUtentiServizio(cog, gerarServ).ToList();// sp_RicercaUtenti(cog).ToList();

                    if (rows != null && rows.Any())
                    {
                        RaiAcademyControllerScope.Instance.AcademyAnagraficaUtenti = new List<AcademyAnagraficaUtente>();

                        rows.ForEach(r =>
                        {
                            if (r.Matricola!=matricola && 
                                (!(String.IsNullOrEmpty(r.Cognome) ||
                                String.IsNullOrEmpty(r.Nome) ||
                                String.IsNullOrEmpty(r.Sezione) ||
                                //String.IsNullOrEmpty(r.Descrizione_Sezione) ||
                                String.IsNullOrEmpty(r.Descrizione_Servizio)))
                                )
                            {
                                AnagSelect anag = new AnagSelect()
                                {
                                    id = r.Matricola,
                                    text = r.Cognome + "/" + r.Nome + "/" + r.Descrizione_Servizio,
                                    matricola = r.Matricola
                                };

                                lista.Add(anag);
                                RaiAcademyControllerScope.Instance.AcademyAnagraficaUtenti.Add(new AcademyAnagraficaUtente()
                                {
                                    Matricola = r.Matricola,
                                    Nome = r.Nome,
                                    Cognome = r.Cognome,
                                    Direzione = r.Descrizione_Sezione,
                                    Nominativo = String.Format("{0} {1}", r.Cognome, r.Nome)
                                });
                            }
                        });
                    }
                }
            }

            RaiAcademyControllerScope.Instance.FiltroRicercaUtenti = cog;

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { result = lista }
            };
        }

        [HttpPost]
        public ActionResult IscrizioneCorso(int idCorso, string mailResp)
        {

            DettaglioCorsoVM model = new DettaglioCorsoVM();
            model.Corso = this.dataController.GetDettaglioCorso(CommonManager.GetCurrentUserMatricola(), idCorso);
            if (model.Corso != null)
            {
                string matricola = CommonManager.GetCurrentUserMatricola();

                MailSender invia = new MailSender();
                Email eml = new Email();
                eml.From = CommonManager.GetParametro<string>(EnumParametriSistema.MailIscrizioneCorsoFrom);

                eml.toList = new string[] { mailResp };
                eml.ccList = new string[] { CommonManager.GetEmailPerMatricola(matricola) };
                eml.ContentType = "text/html";
                eml.Priority = 2;
                eml.SendWhen = DateTime.Now.AddSeconds(1);

                eml.Subject = "Iscrizione corso '" + model.Corso.Titolo + "'";
                eml.Body = CommonManager.GetParametro<string>(EnumParametriSistema.MailIscrizioneCorsoBody);
                eml.Body = eml.Body.Replace("#corsoNome", model.Corso.Titolo)
                            .Replace("#matricola", CommonManager.GetCurrentUserPMatricola())
                            .Replace("#nominativo", CommonManager.GetNominativoPerMatricola(matricola))
                            .Replace("#dataRichiesta", DateTime.Today.ToString("dd/MM/yyyy"));

                string[] AccountUtenteServizio = CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio);
                invia.Credentials = new System.Net.NetworkCredential(AccountUtenteServizio[0], AccountUtenteServizio[1], "RAI");

                try
                {
                    invia.Send(eml);
                }
                catch (Exception ex)
                {
                    return Content(ex.Message);
                }

            }

            return Content("OK");
        }
        #endregion

        #region GestioneDashboard
        #region CalendarioAgenda
        public PartialViewResult GetCalendario(int? AnnoRichiesto, int? MeseRichiesto)
        {
            CalendarioCorsi model = new CalendarioCorsi();
            model.NormalizzaMese(MeseRichiesto, AnnoRichiesto);
            model.CaricaCorsi( CommonHelper.GetCurrentUserMatricola( ) );
            return PartialView("~/Views/RaiAcademy/subpartial/Calendario.cshtml",model);
        }
        public PartialViewResult GetAgenda(int? mese, int? anno)
        {
            AgendaCorsi agendaCorsi = new AgendaCorsi();
            agendaCorsi.NormalizzaMese(anno, mese);
            agendaCorsi.CaricaCorsi( CommonHelper.GetCurrentUserMatricola( ) );

            return PartialView("~/Views/RaiAcademy/subpartial/Schedule.cshtml", agendaCorsi);
        }
        #endregion

        #region ElencoCorsiDashBoard
        public PartialViewResult ElencoCorsiDaFare(bool showAll = false)
        {
            return ElencoCorsi(CorsiFilter.DaFare, showAll);
        }
        public PartialViewResult ElencoCorsiIniziati(bool showAll = false)
        {
            return ElencoCorsi(CorsiFilter.Iniziati, showAll);
        }
        public PartialViewResult ElencoCorsiObbligatori(bool showAll = false)
        {
            return ElencoCorsi(CorsiFilter.Obbligatori, showAll);
        }
        public PartialViewResult ElencoCorsiMiIncuriosiscono(bool showAll = false)
        {
            return ElencoCorsi(CorsiFilter.MiIncuriscono, showAll);
        }
        public PartialViewResult InApprovazione(bool showAll=false)
        {
            return ElencoCorsi(CorsiFilter.InApprovazione, showAll);
        }

		//// Da sostituire con l'altro metodo ElencoCorsi
		//[Obsolete]
        public PartialViewResult ElencoCorsi(CorsiFilter filter = CorsiFilter.All, bool showAll = false)
        {
            try
            {
                int idCorso = RaiAcademyControllerScope.Instance.IdCorso;
				GetCorsiResult result = new GetCorsiResult();
                int numCorsi = 0;

                CorsoList model = new CorsoList();
                switch (filter)
                {
                    case CorsiFilter.DaFare:
                        model.Titolo = "Corsi da fare";
                        model.Dettagli = CorsoDetail.Informazioni;
                        model.ShowProgress = true;
                        model.ShowDashboardButton = true;
                        result = this.dataController.GetCorsi(CommonHelper.GetCurrentUserMatricola(), false);
                        if (result != null)
                        {
                            model.Items = result.List.Where(x => x.Iscritto == IscrittoEnum.Iscritto && x.Completamento < 2).ToList();
                            model.PageSize = showAll ? 9 : 3;
                            model.CurrentPage = 1;
                            model.TotalCourses = model.Items.Count();
                        }
                        break;
                    case CorsiFilter.Iniziati:
                        model.Titolo = "Corsi iniziati";
                        model.Dettagli = CorsoDetail.DataInizio;
                        model.ShowProgress = true;
                        model.ShowDashboardButton = true;
                        result = this.dataController.GetCorsi(CommonHelper.GetCurrentUserMatricola(), false);
						if ( result != null )
						{
							model.Items = result.List.Where(x=>x.Iscritto==IscrittoEnum.Iscritto && x.Completamento>1 && x.Completamento<100).ToList();
                            model.PageSize = showAll?9:3;
							model.CurrentPage = 1;
							model.TotalCourses = model.Items.Count();
						}
                        break;
                    case CorsiFilter.Obbligatori:
                        model.Titolo = "Obbligatori da fare";
						result = this.dataController.GetCorsi(CommonHelper.GetCurrentUserMatricola(), false);
						if ( result != null )
						{
							model.Items = result.List.Where(x=>x.Stato==StatoCorsoTipoOffertaEnum.Obbligatoria).ToList();
                            model.PageSize = showAll ? 9 : 3;
							model.CurrentPage = 1;
							model.TotalCourses = model.Items.Count();
						}
                        break;
                    case CorsiFilter.MiIncuriscono:
                        model.Titolo = "I miei preferiti";
                        model.ShowDashboardButton = true;
						result = this.dataController.GetCorsi(CommonHelper.GetCurrentUserMatricola(), false);
						if ( result != null )
						{
							model.Items = result.List.Where(x=>x.MiIncuriosisce).ToList();
                            model.PageSize = showAll ? 9 : 3;
							model.CurrentPage = 1;
							model.TotalCourses = model.Items.Count();
						}
                        break;
                    case CorsiFilter.InApprovazione:
                        model.Titolo = "In approvazione";
                        model.Dettagli = CorsoDetail.Informazioni;
                        model.ShowProgress = true;
                        model.ShowDashboardButton = true;
                        var tmp = this.dataController.EstraiCorsi(CommonHelper.GetCurrentUserMatricola(), true);
                        if (tmp != null && tmp.Count()>0)
                        {
                            model.Items = tmp.Where(x => x.Iscritto == IscrittoEnum.RichiestaInAttesa).ToList();
                            model.PageSize = showAll ? 9 : 3;
                            model.CurrentPage = 1;
                            model.TotalCourses = model.Items.Count();
                        }
                        break;
                    default:
                        numCorsi = 10;
						result = this.dataController.GetCorsi(CommonHelper.GetCurrentUserMatricola(), false);
						if ( result != null )
						{
							model.Items = result.List;
                            model.PageSize = showAll ? 9 : 3;
							model.CurrentPage = 1;
							model.TotalCourses = result.TotaleCorsi;
						}
                        break;
                }
                model.ShowAll = showAll;
                model.AddLayout = showAll;
                model.ActionFilter = filter;
                model.RouteValues = new { filter = filter, showAll = !showAll };
                
                return PartialView("~/Views/RaiAcademy/ElencoCorsi.cshtml", model);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

		[HttpPost]
		public PartialViewResult ElencoCorsi ( int requestedPage = 1, StatoCorsoTipoOffertaEnum tab = StatoCorsoTipoOffertaEnum.NonDefinito  )
		{
			try
			{
				int idCorso = RaiAcademyControllerScope.Instance.IdCorso;
				int pageSize = RaiAcademyControllerScope.Instance.PageSize;

				GetCorsiResult result = new GetCorsiResult();
				CorsoList model = new CorsoList();

				result = this.dataController.GetCorsi(CommonHelper.GetCurrentUserMatricola(), false);
				if ( result != null )
				{
					model.Items = result.List;
					model.PageSize = pageSize;
					model.CurrentPage = requestedPage;
					model.TotalCourses = result.TotaleCorsi;
				}

				model.ShowAll = true;
				model.AddLayout = false;
				model.ActionFilter = CorsiFilter.All;
				model.RouteValues = null;

				return PartialView( "~/Views/RaiAcademy/ElencoCorsi.cshtml", model );
			}
			catch ( Exception ex )
			{
				throw new Exception( ex.Message );
			}
		}

        public PartialViewResult ElencoCorsiFatti(bool showAll = false)
        {
            //Il metodo è temporaneo. Nel momento in cui verranno definite le regole per determinare 
            //i corsi fatti dovrà essere rivista anche la view
            string selMatricola = CommonHelper.GetCurrentUserMatricola();
                        
            CorsoList model = new CorsoList();

            model.Titolo = "I miei corsi";
            model.Dettagli = CorsoDetail.CorsiFatti;
            model.ShowProgress = true;
            model.Items = dataController.GetCorsiFatti(selMatricola);
            model.Items = model.Items.OrderByDescending(y => y.DataInizio).ToList();

            model.PageSize = showAll?9:3;
			model.CurrentPage = 1;
			model.TotalCourses = model.Items.Count();
            model.ShowAll = showAll;
            model.AddLayout = showAll;
            model.RouteValues = new { showAll = !showAll };
                
            return PartialView("~/Views/RaiAcademy/ElencoCorsi.cshtml", model);
        }
        
		#endregion

        #region Riconoscimenti
        public PartialViewResult GetRiconoscimenti(bool showAll = false)
        {
            LinkList linkList = new LinkList();

            linkList.Items.Add(new Link() { Testo = "Attestato per La gestione dei collaboratori Lorem Ipsum", Href = "#" });
            linkList.Items.Add(new Link() { Testo = "Certificazione per lorem ipsum dolor", Href = "#" });
            linkList.Items.Add(new Link() { Testo = "Attestato per La gestione dei collaboratori Lorem Ipsum 2", Href = "#" });
            linkList.Items.Add(new Link() { Testo = "Qualifica Lorem Ipsum", Href = "#" });
            linkList.Items.Add(new Link() { Testo = "Attestato per La gestione dei collaboratori Lorem Ipsum 3", Href = "#" });
            linkList.Items.Add(new Link() { Testo = "Qualifica Lorem Ipsum 2", Href = "#" });

            linkList.RouteValues = new { showAll = !showAll };
            linkList.ShowLimit = 6;
            linkList.ShowAll = showAll;
            linkList.AddLayout = showAll;

            return PartialView("~/Views/RaiAcademy/subpartial/Riconoscimenti.cshtml", linkList);
        }
        #endregion

        public PartialViewResult GetNotifiche()
        {
            myRaiData.digiGappEntities db = new myRaiData.digiGappEntities();
            List<myRaiData.MyRai_Notifiche> notifiche = new List<myRaiData.MyRai_Notifiche>();
            notifiche = db.MyRai_Notifiche.Where(x => x.categoria.Contains("Corso") || x.categoria=="Academy").ToList();
            return PartialView("~/Views/RaiAcademy/subpartial/notifiche.cshtml", notifiche);
        }
        #endregion

        public ActionResult RegistroFirme(int idEdizione)
        {
            string registroName = "Registro firme.pdf";

            TalentiaEntities db = new TalentiaEntities();
            EDIZIONE ediz = db.EDIZIONE.Include("CURRFORM").Include("CURRFORM.SINTESI1").FirstOrDefault(x => x.ID_EDIZIONE == idEdizione);

            var elencoSedi = db.SEDE.Include("TB_COMUNE").ToList();

            string numId = "";
            string titoloAzione = "";
            string titoloEdiz = ediz.CORSO.COD_CORSO;
            string periodoSvolg = "";
            if (ediz.DTA_INIZIO == ediz.DTA_FINE)
                periodoSvolg = ediz.DTA_INIZIO.ToString("dd/MM/yyyy");
            else
                periodoSvolg = ediz.DTA_INIZIO.ToString("dd/MM/yyyy") + " - " + ediz.DTA_FINE.ToString("dd/MM/yyyy");
            string sedeSvolg = "-";
            sedeSvolg = ediz.TRACENTER != null ? ediz.TRACENTER.TB_COMUNE.DES_CITTA + " - " + ediz.TRACENTER.DES_TRACENTER
                                               : ediz.ENTE != null ? ediz.ENTE.DES_ENTE : "";
            sedeSvolg += ediz.TRACENTER != null ? ediz.TRACENTER.DES_ADDRESS + (ediz.TRACLASSROOM != null ? " - " + ediz.TRACLASSROOM.NME_TRACLASSROOM : "") : "";


            MemoryStream ms = new MemoryStream();
            PdfPrinter pdf = new PdfPrinter("", "", "Registro firme");
            pdf.Apri();

            //Copertina
            string imgRai = "";
            using (digiGappEntities dbDG = new digiGappEntities())
            {
                var template = dbDG.MyRai_Incentivi_Template.FirstOrDefault(x => x.Sede == "LOGO");
                if (template != null)
                    imgRai = Convert.ToBase64String(template.Template);
            }

            pdf.WriteIntestazioneRai(imgRai);
            pdf.WriteRegistro();
            pdf.WriteAzioneFormativa(numId, titoloAzione, titoloEdiz, periodoSvolg, sedeSvolg);
            pdf.WritePageNumber("Il presente registro consta di n° ", 14f);

            string docente = "";//!String.IsNullOrWhiteSpace(ediz.CORSO.NOT_DOCENTE)?ediz.CORSO.NOT_DOCENTE:"";
            var docenti = db.DOCE_EDIZ.Include("SINTESI1").Where(x => x.ID_EDIZIONE == ediz.ID_EDIZIONE);
            foreach (var doc in docenti)
            {
                if (!String.IsNullOrWhiteSpace(docente)) docente += "/";
                docente += (!String.IsNullOrWhiteSpace(doc.SINTESI1.DES_TITOLOONOR) ? doc.SINTESI1.DES_TITOLOONOR + " " : "") + doc.SINTESI1.DES_COGNOMEPERS + " " + doc.SINTESI1.DES_NOMEPERS;
            }

            //Elenco partecipazioni
            foreach (var ggCal in ediz.EDITIONCALENDAR.OrderBy(x => x.DTA_DATE))
            {
                string giornata = ggCal.DTA_DATE.ToString("dd/MM/yyyy");
                string sede = sedeSvolg;
                string argomento = ediz.CORSO.COD_CORSO.Trim();
                string orario = String.Format("dalle ore: {0:00}:{1:00} alle ore {2:00}:{3:00}", ggCal.NMB_FROMHOUR, ggCal.NMB_FROMMINUTE, ggCal.NMB_TOHOUR, ggCal.NMB_TOMINUTE);
                string coordinatore = "";

                pdf.NewPage();
                pdf.WriteHeaderGiornata(giornata, sede, argomento, orario, docente, coordinatore);

                pdf.WriteElencoIscritti(ediz.CURRFORM, elencoSedi);
            }

            pdf.Chiudi(out ms);
            
            return new FileStreamResult(ms, "application/pdf") { FileDownloadName = registroName };
        }
    }

	/// <summary>
	/// Oggetto utilizzato per memorizzare i dati in modo da evitare il passaggio degli stessi in post
	/// tra più metodi.
	/// </summary>
	public class RaiAcademyControllerScope : SessionScope<RaiAcademyControllerScope>
	{
		public RaiAcademyControllerScope ()
		{
			this.IdCorso = 0;
			this.PageSize = 9;
            this.FiltroRicercaUtenti = String.Empty;
            this.AcademyAnagraficaUtenti = new List<AcademyAnagraficaUtente>();
		}

		/// <summary>
		/// Identificativo univoco del corso selezionato
		/// </summary>
		public int IdCorso
		{
			get;
			set;
		}

		/// <summary>
		/// Filtri utilizzati in ricerca
		/// </summary>
		public FiltriCatalogoModel Filtri
		{
			get;
			set;
		}

		public int PageSize
		{
			get;
			set;
		}

        public string FiltroRicercaUtenti { get; set; }
        public List<AcademyAnagraficaUtente> AcademyAnagraficaUtenti { get; set; }
	}

	/// <summary>
	/// Definizione dell'oggetto che rappresenta i dati di output del servizio di ricerca utenti
	/// </summary>
	public class AcademyAnagraficaUtente
	{
		public AcademyAnagraficaUtente ()
		{
		}

		public string Cognome { get; set; }
		public string Nome { get; set; }
		public string Matricola { get; set; }
		public string Direzione { get; set; }
        public string Nominativo
        {
			get
			{
				return String.Format( "{0} {1}", this.Cognome, this.Nome );
			}
			set
			{
			}
		}
	}


    class PdfPrinter
    {
        const int BORDER_NONE = 0;
        const int BORDER_ALL = Rectangle.LEFT_BORDER | Rectangle.TOP_BORDER | Rectangle.RIGHT_BORDER | Rectangle.BOTTOM_BORDER;

        const int fontSize = 10;
        const int fontTitleSize = 10;

        public static Image GetImage(string imagePath)
        {
            iTextSharp.text.Image png = null;
            string pattern = @"data:image/(gif|png|jpeg|jpg);base64,";
            string imgString = Regex.Replace(imagePath, pattern, string.Empty);
            png = Image.GetInstance(Convert.FromBase64String(imgString));
            png.ScaleAbsolute(50f, 50f);
            return png;
        }

        BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED);
        Font myFont;
        Font myFontBold;
        Font myFontTitle;

        BaseColor baseBlue = new BaseColor(31, 78, 121);

        private Document _document;
        private PdfWriter _writer;
        private MemoryStream _ms;
        private string _imagePath;
        private string _imageInt;
        private string _title;

        private int _maxYPage = 55;
        private int _startX = 45;

        public PdfPrinter(string imagePath, string imageInt, string title)
        {
            _imagePath = imagePath;
            _imageInt = imageInt;
            _title = title;

            myFont = new FontManager("", BaseColor.BLACK).Normal; //new iTextSharp.text.Font(bf, fontSize, iTextSharp.text.Font.NORMAL);
            myFontBold = new FontManager("", BaseColor.BLACK).Bold;// new iTextSharp.text.Font(bf, fontSize, iTextSharp.text.Font.BOLD);
            myFontTitle = new FontManager("", BaseColor.BLACK).Bold;
        }

        public bool Apri(int pageStart = 0)
        {
            bool isOpened = false;
            try
            {
                _ms = new MemoryStream();
                _document = new Document(PageSize.A4.Rotate(), 0f, 0f, 0f, 0f);
                
                _writer = PdfWriter.GetInstance(_document, _ms);
                _writer.PageEvent = new ITextEvents(_imagePath, _imageInt, _title, pageStart);
                _document.Open();
                isOpened = true;
            }
            catch (Exception)
            {

            }

            return isOpened;
        }

        public bool Chiudi(out byte[] bytes)
        {
            bytes = null;
            bool isClosed = false;

            try
            {
                _document.Close();
                _writer.Close();
                bytes = _ms.ToArray();
                isClosed = true;
            }
            catch (Exception)
            {

            }

            return isClosed;
        }

        public bool Chiudi(out MemoryStream ms)
        {
            ms = new MemoryStream();
            bool isClosed = false;

            try
            {
                _document.Close();
                _writer.Close();
                byte[] byteInfo = _ms.ToArray();
                ms.Write(byteInfo, 0, byteInfo.Length);
                ms.Position = 0;
                isClosed = true;

            }
            catch (Exception)
            {

            }

            return isClosed;
        }

        public void NewPage()
        {

            _document.NewPage();
        }

        internal static PdfPCell WriteCell(string text, int border, iTextSharp.text.Font f, 
            int colspan=1, int textAlign = PdfPCell.ALIGN_LEFT, int verticalAlign=PdfPCell.ALIGN_MIDDLE, BaseColor background=null, float fixedHeight = 16f, int rowspan=1)
        {
            string textToShow = text;
            if (textAlign == PdfPCell.ALIGN_LEFT)
                textToShow = " " + textToShow;

            PdfPCell cell = new PdfPCell(new Phrase(textToShow, f));            
            cell.FixedHeight = fixedHeight;
            cell.Border = border;
            cell.HorizontalAlignment = textAlign; //0=Left, 1=Centre, 2=Right
            cell.VerticalAlignment = verticalAlign;
            cell.Colspan = colspan;
            cell.Rowspan = rowspan;
            if (background!=null)
                cell.BackgroundColor = background;
            return cell;
        }

        internal static PdfPCell WriteCellKeyValue(string key, string value, int border, iTextSharp.text.Font fKey, Font fValue,
            int colspan = 1, int textAlign = PdfPCell.ALIGN_LEFT, int verticalAlign = PdfPCell.ALIGN_MIDDLE, BaseColor background = null, float fixedHeight = 16f)
        {
            Phrase phrase = new Phrase();
            phrase.Add(new Chunk(" "+key + ": ", fKey));
            phrase.Add(new Chunk(value, fValue));

            PdfPCell cell = new PdfPCell(phrase);
            cell.FixedHeight = fixedHeight;
            cell.Border = border;
            cell.HorizontalAlignment = textAlign; //0=Left, 1=Centre, 2=Right
            cell.VerticalAlignment = verticalAlign;
            cell.Colspan = colspan;
            if (background != null)
                cell.BackgroundColor = background;
            return cell;
        }

        public bool WritePageNumber(string headerText, float fontSize=18f)
        {
            bool isAdded = false;

            int currentY = 575;
            int lStartX = _startX;

            PdfContentByte cb = _writer.DirectContent;

            currentY = ((ITextEvents)_writer.PageEvent).CurrentY;

            if (currentY < _maxYPage)
            {
                _document.NewPage();
                currentY = ((ITextEvents)_writer.PageEvent).CurrentY;
            }

            PdfPTable headerBlock = new PdfPTable(1);
            float widthHeader = _document.PageSize.Width - lStartX * 2;
            headerBlock.TotalWidth = widthHeader;
            headerBlock.LockedWidth = true;
            headerBlock.SetWidths(new float[] { widthHeader });

            Font f = new Font(Font.FontFamily.HELVETICA, fontSize, iTextSharp.text.Font.BOLD, baseBlue);
            headerBlock.AddCell(WriteCell(headerText, BORDER_NONE, f, 1, PdfPCell.ALIGN_CENTER, PdfPCell.ALIGN_MIDDLE, new BaseColor(217, 217, 217), fontSize * 2));
            headerBlock.WriteSelectedRows(0, (headerBlock.Rows.Count + 1), lStartX, currentY, cb);

            ((ITextEvents)_writer.PageEvent).CurrentY -= (int)headerBlock.CalculateHeights();

            return isAdded;
        }

        public bool WriteRegistro()
        {
            bool isAdded = false;

            int currentY = 575;
            int lStartX = _startX;

            PdfContentByte cb = _writer.DirectContent;

            currentY = ((ITextEvents)_writer.PageEvent).CurrentY;

            if (currentY < _maxYPage)
            {
                _document.NewPage();
                currentY = ((ITextEvents)_writer.PageEvent).CurrentY;
            }

            PdfPTable headerBlock = new PdfPTable(2);
            float widthHeader = _document.PageSize.Width - lStartX * 2;
            headerBlock.TotalWidth = widthHeader;
            headerBlock.LockedWidth = true;
            headerBlock.SetWidths(new float[] { widthHeader / 3, widthHeader / 3 * 2 });

            Font fHeader = new Font(Font.FontFamily.HELVETICA, 18f, iTextSharp.text.Font.BOLD, baseBlue);
            Font fLeftCol = new Font(Font.FontFamily.HELVETICA, 12f, iTextSharp.text.Font.NORMAL, baseBlue);
            Font fRightCol = new Font(Font.FontFamily.HELVETICA, 14f, iTextSharp.text.Font.BOLD, baseBlue);

            headerBlock.AddCell(WriteCell("Registro Didattico e delle Presenze", BORDER_NONE, fHeader, 2, PdfPCell.ALIGN_CENTER, PdfPCell.ALIGN_MIDDLE, new BaseColor(217, 217, 217), 36f));
            headerBlock.AddCell(WriteCell(" ", BORDER_NONE, fHeader, 2, fixedHeight: 18f));

            headerBlock.AddCell(WriteCell("Titolo Piano Formativo:", BORDER_NONE, fLeftCol, fixedHeight:28f));
            headerBlock.AddCell(WriteCell("-", BORDER_NONE, fRightCol, fixedHeight: 28f));

            headerBlock.AddCell(WriteCell("Codice identificativo:", BORDER_NONE, fLeftCol, fixedHeight: 28f));
            headerBlock.AddCell(WriteCell("-", BORDER_NONE, fRightCol, fixedHeight: 28f));

            headerBlock.AddCell(WriteCell(" ", BORDER_NONE, fHeader, 2, fixedHeight: 18f));

            headerBlock.WriteSelectedRows(0, (headerBlock.Rows.Count + 1), lStartX, currentY, cb);

            ((ITextEvents)_writer.PageEvent).CurrentY -= (int)headerBlock.CalculateHeights();

            return isAdded;
        }

        public bool WriteAzioneFormativa(string numId, string titoloAzione, string titoloEdiz, string periodoSvolg, string sedeSvolg)
        {
            bool isAdded = false;

            int currentY = 575;
            int lStartX = _startX;

            PdfContentByte cb = _writer.DirectContent;

            currentY = ((ITextEvents)_writer.PageEvent).CurrentY;

            if (currentY < _maxYPage)
            {
                _document.NewPage();
                currentY = ((ITextEvents)_writer.PageEvent).CurrentY;
            }

            PdfPTable headerBlock = new PdfPTable(2);
            float widthHeader = _document.PageSize.Width - lStartX * 2;
            headerBlock.TotalWidth = widthHeader;
            headerBlock.LockedWidth = true;
            headerBlock.SetWidths(new float[] { widthHeader/3, widthHeader/3*2 });

            Font fHeader = new Font(Font.FontFamily.HELVETICA, 18f, iTextSharp.text.Font.BOLD, baseBlue);
            Font fLeftCol = new Font(Font.FontFamily.HELVETICA, 12f, iTextSharp.text.Font.NORMAL, baseBlue);
            Font fRightCol = new Font(Font.FontFamily.HELVETICA, 14f, iTextSharp.text.Font.BOLD, baseBlue);

            headerBlock.AddCell(WriteCell("Azione formativa", BORDER_NONE, fHeader, 2, PdfPCell.ALIGN_CENTER, PdfPCell.ALIGN_MIDDLE, new BaseColor(217, 217, 217), 36f));
            headerBlock.AddCell(WriteCell(" ", BORDER_NONE, fHeader, 2, fixedHeight:18f));

            headerBlock.AddCell(WriteCell("Numero identificativo azione:", BORDER_NONE, fLeftCol, fixedHeight: 28f));
            headerBlock.AddCell(WriteCell(numId, BORDER_NONE, fRightCol, fixedHeight: 28f));

            headerBlock.AddCell(WriteCell("Titolo azione:", BORDER_NONE, fLeftCol, fixedHeight: 28f));
            headerBlock.AddCell(WriteCell(titoloAzione, BORDER_NONE, fRightCol, fixedHeight: 28f));

            headerBlock.AddCell(WriteCell("Titolo Edizione:", BORDER_NONE, fLeftCol, fixedHeight: 28f));
            headerBlock.AddCell(WriteCell(titoloEdiz, BORDER_NONE, fRightCol, fixedHeight: 28f));

            headerBlock.AddCell(WriteCell("Periodo svolgimento azione:", BORDER_NONE, fLeftCol, fixedHeight: 28f));
            headerBlock.AddCell(WriteCell(periodoSvolg, BORDER_NONE, fRightCol));

            headerBlock.AddCell(WriteCell("Sedi di svolgimento:", BORDER_NONE, fLeftCol, fixedHeight: 28f));
            headerBlock.AddCell(WriteCell(sedeSvolg, BORDER_NONE, fRightCol, fixedHeight: 28f));

            headerBlock.AddCell(WriteCell(" ", BORDER_NONE, fHeader, 2, fixedHeight: 18f));

            headerBlock.WriteSelectedRows(0, (headerBlock.Rows.Count + 1), lStartX, currentY, cb);

            ((ITextEvents)_writer.PageEvent).CurrentY -= (int)headerBlock.CalculateHeights();

            return isAdded;
        }

        public bool WriteHeaderGiornata(string giornata, string sede, string argomento, string orario, string docente, string coordinatore)
        {
            bool isAdded = false;

            int currentY = 575;
            int lStartX = _startX;

            PdfContentByte cb = _writer.DirectContent;

            currentY = ((ITextEvents)_writer.PageEvent).CurrentY;

            if (currentY < _maxYPage)
            {
                _document.NewPage();
                currentY = ((ITextEvents)_writer.PageEvent).CurrentY;
            }

            PdfPTable headerBlock = new PdfPTable(5);
            float widthHeader = _document.PageSize.Width - lStartX * 2;
            float smallerCol = widthHeader / 8;

            headerBlock.TotalWidth = widthHeader;
            headerBlock.LockedWidth = true;
            headerBlock.SetWidths(new float[] { smallerCol, smallerCol, smallerCol*2, smallerCol, smallerCol*3 });

            Font fBoldBlack = new Font(Font.FontFamily.HELVETICA, 10f, iTextSharp.text.Font.BOLD, BaseColor.BLACK);
            Font fBold = new Font(Font.FontFamily.HELVETICA, 10f, iTextSharp.text.Font.BOLD, baseBlue);

            float fixedHeight = 26f;

            headerBlock.AddCell(WriteCellKeyValue("Presenze del giorno", giornata, BORDER_ALL, fBold, fBoldBlack, 2, fixedHeight: fixedHeight));

            headerBlock.AddCell(WriteCellKeyValue("Sede", sede, BORDER_ALL, fBold, fBoldBlack, 3, fixedHeight: fixedHeight));

            headerBlock.AddCell(WriteCell("Argomento", BORDER_ALL, fBold, fixedHeight: fixedHeight));
            headerBlock.AddCell(WriteCell(argomento, BORDER_ALL, fBoldBlack, 2, fixedHeight: fixedHeight));
            headerBlock.AddCell(WriteCell("Orario", BORDER_ALL, fBold, fixedHeight: fixedHeight));
            headerBlock.AddCell(WriteCell(orario, BORDER_ALL, fBoldBlack, fixedHeight: fixedHeight));

            headerBlock.AddCell(WriteCell("Docente", BORDER_ALL, fBold, fixedHeight: fixedHeight));
            headerBlock.AddCell(WriteCell(docente, BORDER_ALL, fBoldBlack, 2, fixedHeight: fixedHeight));
            headerBlock.AddCell(WriteCell("Firma", BORDER_ALL, fBold, fixedHeight: fixedHeight));
            headerBlock.AddCell(WriteCell("", BORDER_ALL, fBoldBlack, fixedHeight: 26));

            headerBlock.AddCell(WriteCell("Coordinatore", BORDER_ALL, fBold, fixedHeight: fixedHeight));
            headerBlock.AddCell(WriteCell(coordinatore, BORDER_ALL, fBoldBlack, 2, fixedHeight: fixedHeight));
            headerBlock.AddCell(WriteCell("Firma", BORDER_ALL, fBold, fixedHeight: fixedHeight));
            headerBlock.AddCell(WriteCell("", BORDER_ALL, fBoldBlack, fixedHeight: fixedHeight));

            headerBlock.AddCell(WriteCell("", BORDER_NONE, fBoldBlack, 5, fixedHeight: 14));

            headerBlock.WriteSelectedRows(0, (headerBlock.Rows.Count + 1), lStartX, currentY, cb);

            ((ITextEvents)_writer.PageEvent).CurrentY -= (int)headerBlock.CalculateHeights();

            return isAdded;
        }

        public bool WriteElencoIscritti(ICollection<CURRFORM> iscrizioni, ICollection<SEDE> elencoSedi)
        {
            bool isAdded = false;

            int currentY = 575;
            int lStartX = _startX;

            PdfContentByte cb = _writer.DirectContent;

            currentY = ((ITextEvents)_writer.PageEvent).CurrentY;

            if (currentY < _maxYPage)
            {
                _document.NewPage();
                currentY = ((ITextEvents)_writer.PageEvent).CurrentY;
            }

            PdfPTable tableBlock = new PdfPTable(7);
            float widthHeader = _document.PageSize.Width - lStartX * 2;
            float smallerCol = widthHeader / 23;

            tableBlock.TotalWidth = widthHeader;
            tableBlock.LockedWidth = true;
            tableBlock.SetWidths(new float[] { smallerCol, smallerCol * 7, smallerCol * 2, smallerCol * 2, smallerCol, smallerCol * 5, smallerCol * 5 });

            Font fNormalBlack = new Font(Font.FontFamily.HELVETICA, 10f, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
            Font fBold = new Font(Font.FontFamily.HELVETICA, 10f, iTextSharp.text.Font.BOLD, baseBlue);

            BaseColor bkgColor = new BaseColor(217, 217, 217);
            float rowHeight = 26f;

            void RenderTableHeader()
            {

                float headerHeight = 30f;

                tableBlock.AddCell(WriteCell("N", BORDER_ALL, fBold, 1, 0, 5, bkgColor, headerHeight, 2));
                tableBlock.AddCell(WriteCell("Cognome e Nome", BORDER_ALL, fBold, 1, 0, 5, bkgColor, headerHeight, 2));
                tableBlock.AddCell(WriteCell("Matricola", BORDER_ALL, fBold, 1, 0, 5, bkgColor, headerHeight, 2));
                tableBlock.AddCell(WriteCell("Direzione", BORDER_ALL, fBold, 1, 0, 5, bkgColor, headerHeight, 2));
                tableBlock.AddCell(WriteCell("Sede", BORDER_ALL, fBold, 1, 0, 5, bkgColor, headerHeight, 2));
                tableBlock.AddCell(WriteCell("Firma", BORDER_ALL, fBold, 2, PdfPCell.ALIGN_CENTER, 5, bkgColor, headerHeight / 2));
                tableBlock.AddCell(WriteCell("MATTINA", BORDER_ALL, fBold, 1, PdfPCell.ALIGN_CENTER, 5, bkgColor, headerHeight / 2));
                tableBlock.AddCell(WriteCell("POMERIGGIO", BORDER_ALL, fBold, 1, PdfPCell.ALIGN_CENTER, 5, bkgColor, headerHeight / 2));

                tableBlock.WriteSelectedRows(tableBlock.Rows.Count-2, tableBlock.Rows.Count, lStartX, currentY, cb);
                currentY -= (int)headerHeight;
            }

            RenderTableHeader();

            int indexIscritto = 0;

            if (iscrizioni.All(x => x.SINTESI1 != null))
            {
            foreach (var iscr in iscrizioni.OrderBy(x=>x.SINTESI1.DES_COGNOMEPERS+" "+x.SINTESI1.DES_NOMEPERS))
            {
                if (currentY < _maxYPage)
                {
                    _document.NewPage();
                    currentY = ((ITextEvents)_writer.PageEvent).CurrentY;
                    RenderTableHeader();
                }

                var sint = iscr.SINTESI1;
                string nominativo = sint.DES_COGNOMEPERS + ", " + sint.DES_NOMEPERS;
                string matricola = sint.COD_MATLIBROMAT;
                string direzione = "";
                    if (sint.XR_TB_SERVIZIO != null)
                        direzione = sint.XR_TB_SERVIZIO.COD_SIGLA;
                    string sede = "";
                    if (!String.IsNullOrWhiteSpace(sint.COD_SEDE))
                    {
                        var dbSEDE = elencoSedi.FirstOrDefault(x => x.COD_SEDE == sint.COD_SEDE);
                        if (dbSEDE != null && dbSEDE.TB_COMUNE != null)
                            sede = dbSEDE.TB_COMUNE.COD_PROV_STATE;
                    }

                tableBlock.AddCell(WriteCell((++indexIscritto).ToString(), BORDER_ALL, fNormalBlack, fixedHeight: rowHeight));
                tableBlock.AddCell(WriteCell(nominativo, BORDER_ALL, fNormalBlack, fixedHeight: rowHeight));
                tableBlock.AddCell(WriteCell(matricola, BORDER_ALL, fNormalBlack, fixedHeight: rowHeight));
                tableBlock.AddCell(WriteCell(direzione, BORDER_ALL, fNormalBlack, fixedHeight: rowHeight));
                    tableBlock.AddCell(WriteCell(sede, BORDER_ALL, fNormalBlack, fixedHeight: rowHeight));
                tableBlock.AddCell(WriteCell(" ", BORDER_ALL, fNormalBlack, fixedHeight: rowHeight));
                tableBlock.AddCell(WriteCell(" ", BORDER_ALL, fNormalBlack, fixedHeight: rowHeight));

                tableBlock.WriteSelectedRows(tableBlock.Rows.Count-1, tableBlock.Rows.Count, lStartX, currentY, cb);
                currentY -= (int)rowHeight;
            }
            }

            //tableBlock.WriteSelectedRows(0, (tableBlock.Rows.Count + 1), lStartX, currentY, cb);

            ((ITextEvents)_writer.PageEvent).CurrentY -= (int)tableBlock.CalculateHeights();

            return isAdded;
        }

        public bool WriteIntestazioneRai(string imgRai)
        {
            bool isAdded = false;

            int currentY = 575;
            int lStartX = _startX;

            PdfContentByte cb = _writer.DirectContent;

            currentY = ((ITextEvents)_writer.PageEvent).CurrentY;

            if (currentY < _maxYPage)
            {
                _document.NewPage();
                currentY = ((ITextEvents)_writer.PageEvent).CurrentY;
            }

            Image raiImage = PdfPrinter.GetImage(imgRai);

            PdfPTable headerBlock = new PdfPTable(2);
            float widthHeader = _document.PageSize.Width - lStartX * 2;
            headerBlock.TotalWidth = widthHeader;
            headerBlock.LockedWidth = true;
            headerBlock.SetWidths(new float[] { 50, widthHeader - 50});

            Font fFirstrow = new Font(Font.FontFamily.HELVETICA, 12f, iTextSharp.text.Font.BOLD, baseBlue);
            Font fOtherRow = new Font(Font.FontFamily.HELVETICA, 11f, iTextSharp.text.Font.NORMAL, baseBlue);

            PdfPCell cellImage = new PdfPCell(raiImage);
            cellImage.FixedHeight = 50;
            cellImage.Border = BORDER_NONE;
            cellImage.HorizontalAlignment = PdfPCell.ALIGN_CENTER; 
            cellImage.VerticalAlignment = PdfPCell.ALIGN_MIDDLE;
            cellImage.Rowspan = 3;
            cellImage.BackgroundColor = BaseColor.WHITE;
            headerBlock.AddCell(cellImage);

            headerBlock.AddCell(WriteCell("Rai Radiotelevisione Italiana Spa", BORDER_NONE, fFirstrow));
            headerBlock.AddCell(WriteCell("Direzione Risorse Umane e Organizzazione", BORDER_NONE, fOtherRow));
            headerBlock.AddCell(WriteCell("Formazione", BORDER_NONE, fOtherRow));

            headerBlock.AddCell(WriteCell(" ", BORDER_NONE, fOtherRow, 2, fixedHeight:26f));

            headerBlock.WriteSelectedRows(0, (headerBlock.Rows.Count + 1), lStartX, currentY, cb);

            ((ITextEvents)_writer.PageEvent).CurrentY -= (int)headerBlock.CalculateHeights();

            return isAdded;
        }
    }

    class ITextEvents : PdfPageEventHelper
    {
        string _imgPath;
        string _title;
        int _pageStart;
        string _imgInfo;

        iTextSharp.text.Image _pngInfo = null;
        iTextSharp.text.Image _pngImage = null;

        public PdfPCell PageCountTemplate = null;

        public ITextEvents(string imgPath = "", string imgInfo = "", string title = "", int pageStart = 0)
        {
            this._imgPath = imgPath;
            this._title = title;
            this._pageStart = pageStart;
            this._imgInfo = imgInfo;

            if (!String.IsNullOrWhiteSpace(_imgPath))
            {
                _pngImage = PdfPrinter.GetImage(_imgPath);
            }

            if (!String.IsNullOrWhiteSpace(_imgInfo))
            {
                _pngInfo = PdfPrinter.GetImage(_imgInfo);
                _pngInfo.ScalePercent(30f);
            }
        }

        #region Fields
        private string _header;
        #endregion

        #region Properties
        public string Header
        {
            get { return _header; }
            set { _header = value; }
        }

        public int CurrentY
        {
            get
            {
                return this.currentY;
            }
            set
            {
                this.currentY = value;
            }
        }

        #endregion

        // This is the contentbyte object of the writer
        PdfContentByte cb;

        // we will put the final number of pages in a template
        PdfTemplate headerTemplate, footerTemplate;

        // This keeps track of the creation time
        DateTime PrintTime = DateTime.Now;

        int currentY = 575;
        const int lStartX = 45;
        const int fontSize = 10;
        BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED);

        public override void OnStartPage(PdfWriter writer, Document document)
        {
            int intestazioneHeight = WriteIntestazione(cb, document);
            headerTemplate = cb.CreateTemplate(500, intestazioneHeight);
            cb.AddTemplate(headerTemplate, document.LeftMargin, document.PageSize.GetTop(document.TopMargin));

        }

        private int WriteIntestazione(PdfContentByte cb, Document document)
        {
            int _currentY = 575;
            const int lStartX = 45;
            const int fontSize = 10;

            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED);
            iTextSharp.text.Font myFont = new iTextSharp.text.Font(bf, fontSize, iTextSharp.text.Font.NORMAL, new BaseColor(95, 95, 95));

            // disegno del logo
            bool drawImage = false;
            if (!String.IsNullOrWhiteSpace(_imgPath))
                drawImage = true;

            PdfPTable table = new PdfPTable(2);
            table.DefaultCell.BorderWidth = 1;
            table.TotalWidth = document.PageSize.Width - 50;
            table.LockedWidth = true;
            int[] widths = new int[] { 50, 350 };
            table.SetWidths(widths);

            PdfPCell cell = null;
            if (drawImage)
            {
                PdfPCell cellImage = new PdfPCell(_pngImage);
                cellImage.Border = PdfPCell.NO_BORDER;
                cellImage.Rowspan = 3;
                cellImage.Colspan = 2;
                table.AddCell(cellImage);

                cell = new PdfPCell(new Phrase(" ", new FontManager("", BaseColor.BLACK).Normal));
                cell.Border = PdfPCell.NO_BORDER;
                cell.Colspan = 2;
                table.AddCell(cell);
            }

            cell = new PdfPCell(new Phrase(" ", new FontManager("", BaseColor.BLACK).Normal));
            cell.Border = PdfPCell.NO_BORDER;
            table.AddCell(cell);

            cell = new PdfPCell(new Phrase(" ", new FontManager("", BaseColor.BLACK).Normal));
            cell.Border = PdfPCell.NO_BORDER;
            cell.Colspan = 2;
            table.AddCell(cell);

            table.WriteSelectedRows(0, table.Rows.Count + 1, lStartX, _currentY, cb);

            _currentY = _currentY - (int)table.CalculateHeights();

            this.CurrentY = _currentY;

            return (int)table.CalculateHeights();
        }

        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            try
            {
                // Aggiunta metadata al documento
                document.AddAuthor("digiGapp");

                document.AddCreator("digiGapp con l'ausilio di iTextSharp");

                document.AddKeywords("PDF Registro Presenze");

                document.AddSubject("Registro Presenze");

                document.AddTitle(String.Format(_title));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            cb = writer.DirectContent;
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            base.OnEndPage(writer, document);


            int pageN = writer.PageNumber + _pageStart;
            float a4RotateWidth = PageSize.A4.Rotate().Width;

            PdfShading shading = PdfShading.SimpleAxial(writer, a4RotateWidth - 24f, 575, a4RotateWidth, 575, new BaseColor(0, 0, 153), new BaseColor(0, 0, 153)); //new BaseColor(180, 237, 80), new BaseColor(180, 237, 80));
            PdfShadingPattern pattern = new PdfShadingPattern(shading);
            ShadingColor color = new ShadingColor(pattern);

            PdfPTable t = new PdfPTable(3);
            t.SetTotalWidth(new float[] { 24, a4RotateWidth - 48, 24 });

            FontManager fm = new FontManager(HttpContext.Current.Server.MapPath("~/FONTS/opensans.ttf"), new BaseColor(255, 255, 255));

            t.LockedWidth = true;
            t.AddCell(new PdfPCell() { Border = 0 });
            t.AddCell(new PdfPCell() { FixedHeight = 24f, Border = 0, HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER, VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE });
            t.AddCell(new PdfPCell(new Phrase(string.Format("{0}", pageN), fm.N12)) { FixedHeight = 24f, BackgroundColor = color, Border = 0, HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER, VerticalAlignment = iTextSharp.text.Element.ALIGN_MIDDLE });
            t.WriteSelectedRows(0, -1, 0, 24f, writer.DirectContent);


            //String text = "Pagina " + pageN.ToString() + " di ";

            //float len = bf.GetWidthPoint(text, fontSize);

            //iTextSharp.text.Rectangle pageSize = document.PageSize;

            ////cb = writer.DirectContent;

            //cb.SetRGBColorFill(100, 100, 100);

            //cb.BeginText();
            //cb.SetFontAndSize(bf, fontSize);
            //cb.SetTextMatrix(document.LeftMargin, document.PageSize.GetBottom(document.BottomMargin));
            //cb.ShowText(text);
            //cb.EndText();

            //footerTemplate = writer.DirectContent.CreateTemplate(50, 50);
            //cb.AddTemplate(footerTemplate, document.LeftMargin + len, document.PageSize.GetBottom(document.BottomMargin));
        }

        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            base.OnCloseDocument(writer, document);

        }
    }
}
