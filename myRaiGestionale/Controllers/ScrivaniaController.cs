using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using myRai.DataAccess;
using myRaiHelper;
using myRaiCommonModel;
using myRaiData;
using myRaiCommonManager;
using myRaiDataTalentia;
using MyRaiServiceInterface.it.rai.servizi.svilruoesercizio;
using myRaiCommonTasks.Helpers;

namespace myRaiGestionale.Controllers
{
    public class ScrivaniaController : BaseCommonController
    {
        myRaiCommonModel.ModelDash pr = new myRaiCommonModel.ModelDash();
        MaternitaCongediModel md = null;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.ActionDescriptor.ActionName=="Index")
                SessionHelper.Set("MATCON_SCRIVANIA", null);

            md = GetMaternitaCongediModel();

            base.OnActionExecuting(filterContext);
        }

        public string SkipScrivania()
        {
            var db = new digiGappEntities();
            string m = CommonHelper.GetCurrentRealUsername();
            var q = db.MyRai_ParametriSistema.Where(x => x.Chiave == "QuickStart" && (x.Valore1 == m || x.Valore1 == "*")).FirstOrDefault();
            if (q == null || String.IsNullOrWhiteSpace(q.Valore2)) return null;
            else return q.Valore2;
        }

        public static MaternitaCongediModel GetMaternitaCongediModel()
        {
            if (SessionHelper.Get("GEST_SECTION", "GESTIONE") == "GESTIONE")
            {
                MaternitaCongediModel tmp = SessionHelper.Get<MaternitaCongediModel>("MATCON_SCRIVANIA", ()=> { return null; });
                if (tmp==null)
                {
                    tmp = myRaiCommonManager.MaternitaCongediManager.GetMaternitaCongediModel();
                    SessionHelper.Set("MATCON_SCRIVANIA", tmp);
                }
                return tmp;
            }
            else
                return null;
        }

        public ActionResult Index(string ver, string section)
        {
            string s = SkipScrivania();
            if (s != null) return RedirectToAction("/", s);

            ver = "3";

            pr.menuSidebar = UtenteHelper.getSidebarModel();// new sidebarModel(3);

            if (pr.menuSidebar.sezioni.Count == 0)
            {
                return View("~/Views/Shared/NonAbilitatoError.cshtml");
            }

            if (!String.IsNullOrWhiteSpace(section))
                SessionHelper.Set("GEST_SECTION", section);
            else
                SessionHelper.Set("GEST_SECTION", "GESTIONE");

            pr.SezioniVisibili = HomeManager.GetSezioniVisibili(CommonHelper.GetCurrentUserMatricola());
            SectionDayModel model = new SectionDayModel() { };
            pr.MessaggioHome = ScrivaniaManager.SetMessaggioHome();



            return View("~/Views/Scrivania/Index" + ver + ".cshtml", pr);
        }

        public ActionResult GetGestioneWidget()
        {
            string section = SessionHelper.Get<string>("GEST_SECTION", "GESTIONE");
            return View("~/Views/Scrivania/subpartial/GetGestioneWidget.cshtml", HrisWidgetManager.GetGestioneWidget(section));
        }

        public ActionResult Modal_GestioneWidget()
        {
            return View("~/Views/Scrivania/subpartial/Modal_GestioneWidget.cshtml", HrisWidgetManager.GetGestioneWidget());
        }

        [HttpPost]
        public ActionResult Save_GestioneWidget(string[] selectedWidget)
        {
            if (HrisWidgetManager.Save_GestioneWidget(selectedWidget, out string errorMsg))
                return Content("OK");
            else
                return Content(errorMsg);
        }

        public ActionResult GetEmptyWidget()
        {
            string section = SessionHelper.Get<string>("GEST_SECTION", "GESTIONE");
            List<HrisWidget> elencoWidget = HrisWidgetManager.GetWidgetSelezionati(sezione:section);
            if (elencoWidget.Any())
                return Content("");
            else
                return View("EmptyWidget");
        }

        public ActionResult GetWidgetSingoli()
        {
            string section = SessionHelper.Get<string>("GEST_SECTION", "GESTIONE");

            List<AlertModel> widgets = new List<AlertModel>();
            var elencoWidget = HrisWidgetManager.GetWidgetSelezionati("WIDGET_SINGOLO", section);


            foreach (var widget in elencoWidget) //.Where(x => x.COD_SEZIONE.Contains(section) && x.COD_TIPOLOGIA == "WIDGET_SINGOLO"))
            {
                switch (widget.COD_WIDGET)
                {
                    default:
                        if (widget.ExtraParam.WidgetModel != null)
                        {
                            int cifraPrincipale = 0;
                            int cifraTraParentesi = 0;
                            HrisWidgetManager.GetData(widget.ExtraParam.Data, out cifraPrincipale, out cifraTraParentesi);
                            AlertModel model = widget.ExtraParam.WidgetModel;
                            model.CifraPrincipale = String.Format("{0}", cifraPrincipale);
                            model.TraParentesi = String.Format(widget.ExtraParam.WidgetModel.TraParentesi, cifraTraParentesi);
                            widgets.Add(model);
                        }
                        break;
                }
            }

            return View("~/Views/_RaiDesign/Widget_Home_Container.cshtml", widgets);
        }

        public int GetPraticheDaRiavviare()
        {
            var db = new myRaiData.Incentivi.IncentiviEntities();
            return db.XR_MAT_RICHIESTE.Where(x => x.DA_RIAVVIARE == true && x.XR_WKF_OPERSTATI.Max(z => z.ID_STATO) < 80).Count();
        }
        public ActionResult GetWidgetCompressi()
        {
           string section = SessionHelper.Get<string>("GEST_SECTION", "GESTIONE");

            List<AlertModel> widgets = new List<AlertModel>();
            var elencoWidget = HrisWidgetManager.GetWidgetSelezionati("WIDGET_COMPATTO", section);

            foreach (var widget in elencoWidget)//.Where(x => x.COD_SEZIONE.Contains(section) && x.COD_TIPOLOGIA == "WIDGET_COMPATTO"))
            {
                switch (widget.COD_WIDGET)
                {
                    case "MATCON_PRATICHE":
                        widgets.Add(new AlertModel()
                        {
                            IntestazioneWidget = "Maternità e congedi",
                            TipoAlert = 1,
                            Titolo = "Da lavorare",
                            TraParentesi = "Pratiche da prendere in carico",
                            CifraPrincipale = md.RichiesteInCaricoNessuno.Count.ToString(),
                            HrefPulsante = Url.Action("Index", "Maternitacongedi"),
                            TestoPulsante = "Vedi"
                        });
                        break;
                    case "MATCON_INCARICO":
                        widgets.Add(new AlertModel()
                        {
                            IntestazioneWidget = "Maternità e congedi",
                            TipoAlert = 1,
                            Titolo = "In carico",
                            TraParentesi = "Pratiche in carico a me",
                            CifraPrincipale = md.RichiesteInCaricoAme.Count.ToString(),
                            HrefPulsante = Url.Action("Index", "Maternitacongedi"),
                            TestoPulsante = "Vedi"
                        });
                        break;
                    case "MATCON_INTERV":
                        widgets.Add(new AlertModel()
                        {
                            IntestazioneWidget = "Maternità e congedi",
                            TipoAlert = 1,
                            Titolo = "Richiedono intervento",
                            TraParentesi = "Pratiche che richiedono un intervento da parte dell'operatore",
                            CifraPrincipale = GetPraticheDaRiavviare().ToString(),
                            HrefPulsante = Url.Action("Index", "Maternitacongedi", new { dar=1}),
                            TestoPulsante = "Vedi"
                        });
                        break;
                    
                    case "MATCON_INERRORE":
                        widgets.Add(new AlertModel()
                        {
                            IntestazioneWidget = "Maternità e congedi",
                            TipoAlert = 1,
                            Titolo = "In errore",
                            TraParentesi = "Pratiche lavorate da me che hanno prodotto errori",
                            CifraPrincipale = md.RichiesteInCaricoAme.Select(z => z.XR_MAT_TASK_IN_CORSO.Any(d => d.ERRORE_BATCH != null)).ToList().Count().ToString(),
                            HrefPulsante = Url.Action("Index", "Maternitacongedi"),
                            TestoPulsante = "Vedi"
                        });
                        break;
                    case "INC_PRATICHE":
                        if (IncentivazioneHelper.GetHomeWidgetPratiche(CessazioneTipo.Incentivazione, out int totPratiche, out int totInCarico))
                        {
                            widgets.Add(new AlertModel()
                            {
                                IntestazioneWidget = "Cessazione",
                                TipoAlert = 2,
                                Titolo = "Incentivazione",
                                TraParentesi = "Pratiche disponibili (di cui " + totInCarico + " in carico)",
                                CifraPrincipale = totPratiche.ToString(),
                                HrefPulsante = Url.Action("Index", "Cessazione"),
                                TestoPulsante = "Vedi"
                            });
                        }
                        break;
                    case "RISCONS_PRATICHE":
                        if (IncentivazioneHelper.GetHomeWidgetPratiche(CessazioneTipo.RisoluzioneConsensuale, out totPratiche, out totInCarico))
                        {
                            widgets.Add(new AlertModel()
                            {
                                IntestazioneWidget = "Cessazione",
                                TipoAlert = 2,
                                Titolo = "Risoluzione consensuale",
                                TraParentesi = "Pratiche disponibili (di cui " + totInCarico + " in carico)",
                                CifraPrincipale = totPratiche.ToString(),
                                HrefPulsante = Url.Action("Index", "Cessazione"),
                                TestoPulsante = "Vedi"
                            });
                        }
                        break;
                    case "DEMA_INARRIVO":
                        List<XR_DEM_DOCUMENTI_EXT> inArrivo = DematerializzazioneManager.GetDocumentiInCaricoAMe(CommonHelper.GetCurrentUserMatricola());
                        widgets.Add(new AlertModel()
                        {
                            IntestazioneWidget = "Dematerializzazione",
                            TipoAlert = 2,
                            Titolo = "In arrivo",
                            TraParentesi = "In arrivo",
                            CifraPrincipale = (inArrivo != null ? inArrivo.Count() : 0).ToString(),
                            HrefPulsante = Url.Action("Index", "Dematerializzazione"),
                            TestoPulsante = "Vedi"
                        });
                        break;
                    default:
                        if (widget.ExtraParam.WidgetModel != null)
                        {
                            int cifraPrincipale = 0;
                            int cifraTraParentesi = 0;
                            HrisWidgetManager.GetData(widget.ExtraParam.Data, out cifraPrincipale, out cifraTraParentesi);
                            AlertModel model = widget.ExtraParam.WidgetModel;
                            model.CifraPrincipale = String.Format("{0}", cifraPrincipale);
                            model.TraParentesi = String.Format(widget.ExtraParam.WidgetModel.TraParentesi, cifraTraParentesi);
                            widgets.Add(model);
                        }
                        break;
                }
            }

            return View("~/Views/_RaiDesign/Widget_compatto_Container.cshtml", widgets);
        }

        public ActionResult GetWidgetList()
        {
            var elencoWidget = HrisWidgetManager.GetWidgetSelezionati("TODO_LIST");
            return View("~/Views/Scrivania/GetWidgetList.cshtml", elencoWidget);//.Where(x => x.COD_TIPOLOGIA == "TODO_LIST"));
        }
        public ActionResult GetRichieste()
        {
            string section = SessionHelper.Get<string>("GEST_SECTION", "GESTIONE");

            List<RichiestaAnag> elencoRichieste = null;
            var elencoWidget = HrisWidgetManager.GetWidgetSelezionati("TODO_LIST", section).Where(x => x.COD_GRUPPO == "Richieste");
            //.Where(x => x.COD_SEZIONE.Contains(section) && x.COD_TIPOLOGIA == "TODO_LIST" && x.COD_GRUPPO == "Richieste");
            if (elencoWidget.Any(x => x.COD_WIDGET == "RICH_IBAN"))
            {
                if (elencoRichieste == null)
                    elencoRichieste = new List<RichiestaAnag>();

                elencoRichieste.AddRange(AnagraficaManager.GetRichieste(new RichiestaLoader() { Tipologie = new List<TipoRichiestaAnag>() { TipoRichiestaAnag.IBAN } }));
            }

            if (elencoWidget.Any(x => x.COD_WIDGET == "RICH_CONGEDI"))
            {
                if (elencoRichieste == null)
                    elencoRichieste = new List<RichiestaAnag>();
                var db = new myRaiData.Incentivi.IncentiviEntities();
                //Richieste da visualizzare
                //in carico a me ma:
                // non avviate
                // in errore
                //non in carico a qualcuno ma
                // in scadenza?

                //-------- A tendere
                //in carico ad altri ma:
                // in scadenza e da avviare con operatore assente (no presente no smart worker)

                foreach (var ragg in md.RichiesteAggregateInCaricoNessuno)
                {
                    if (ragg.ListaRichiesteAggregate.Count == 1)
                    {
                        foreach (var item in ragg.ListaRichiesteAggregate)
                        {
                            //non in carico

                            RichiestaAnag newReq = new RichiestaAnag();
                            newReq.IdRichiesta = item.ID;
                            newReq.Tipologia = TipoRichiestaAnag.Congedo;
                            newReq.Descrizione = item.XR_MAT_CATEGORIE.TITOLO;
                            newReq.Matricola = item.MATRICOLA;
                            newReq.Nominativo = item.NOMINATIVO;
                            newReq.DataScadenza = item.DATA_SCADENZA;
                            if (item.XR_MAT_PROMEMORIA.Any())
                                newReq.DataMemo = item.XR_MAT_PROMEMORIA.First().DATA;
                            elencoRichieste.Add(newReq);

                        }

                    }
                }
                foreach (var ragg in md.RichiesteAggregateInCaricoAme)
                {
                    if (ragg.ListaRichiesteAggregate.Count == 1)
                    {
                        foreach (var item in ragg.ListaRichiesteAggregate)
                        {
                            //in carico a me ma non avviate o in errore
                            if ((!item.XR_MAT_TASK_IN_CORSO.Any()) || (item.XR_MAT_TASK_IN_CORSO.Any(x => x.ERRORE_BATCH != null)) || item.XR_MAT_PROMEMORIA.Any())
                            {
                                RichiestaAnag newReq = new RichiestaAnag();
                                newReq.IdRichiesta = item.ID;
                                newReq.Tipologia = TipoRichiestaAnag.Congedo;
                                newReq.Descrizione = item.XR_MAT_CATEGORIE.TITOLO;
                                newReq.Matricola = item.MATRICOLA;
                                newReq.Nominativo = item.NOMINATIVO;
                                newReq.DataScadenza = item.DATA_SCADENZA;
                                if (item.XR_MAT_PROMEMORIA.Any())
                                    newReq.DataMemo = item.XR_MAT_PROMEMORIA.First().DATA;
                                elencoRichieste.Add(newReq);
                            }
                        }

                    }
                }

                /*

                var elenco = db.XR_MAT_RICHIESTE.Where(x => x.XR_WKF_OPERSTATI.Where(y=>y.VALID_DTA_END==null || y.VALID_DTA_END>DateTime.Now).Max(y => y.ID_STATO) < (int)MaternitaCongediManager.EnumStatiRichiesta.Approvata);
                foreach (var item in elenco)
                {
                    RichiestaAnag newReq = new RichiestaAnag();
                    newReq.IdRichiesta = item.ID;
                    newReq.Tipologia = TipoRichiestaAnag.Congedo;
                    newReq.Descrizione = item.XR_MAT_CATEGORIE.TITOLO;
                    newReq.Matricola = item.MATRICOLA;
                    newReq.Nominativo = item.NOMINATIVO;
                    newReq.DataScadenza = item.DATA_SCADENZA;

                    elencoRichieste.Add(newReq);
                }*/
            }


            if (elencoWidget.Any(x => x.COD_WIDGET == "RICH_DEM"))
            {
                if (elencoRichieste == null)
                    elencoRichieste = new List<RichiestaAnag>();

                elencoRichieste.AddRange(AnagraficaManager.GetRichieste(new RichiestaLoader() { Tipologie = new List<TipoRichiestaAnag>() { TipoRichiestaAnag.Dematerializzazione } }));
            }

            return View("subpartial/Elenco_richieste", elencoRichieste);
        }
        public ActionResult Modal_Richiesta(string m, TipoRichiestaAnag tipo, int id)
        {
            switch (tipo)
            {
                case TipoRichiestaAnag.IBAN:
                    var richiesta = AnagraficaManager.GetRichiesta(m, tipo, id);
                    return View("~/Views/Anagrafica/subpartial/Modal_richiesta.cshtml", richiesta);
                    break;
                case TipoRichiestaAnag.VariazioneContrattuale:
                    var varContr = AnagraficaManager.GetRichiesta(m, tipo, id);
                    return View("~/Views/Anagrafica/subpartial/Modal_DatiContrattuali.cshtml", varContr.ObjInfo);
                case TipoRichiestaAnag.Congedo:
                    MaternitaDettagliGestioneModel model = myRaiCommonManager.MaternitaCongediManager.GetMaternitaDettagliGestioneModel(id);
                    return View("~/Views/maternitaCongedi/PopupVisGestContent", model);
                    break;
            }

            return Http404();
        }
        public ActionResult GetScadenzario()
        {
            List<ScadenzarioMeseAnnoItem> scadenze = null;

            var serv = new WSDew();
            serv.Credentials = CommonHelper.GetUtenteServizioCredentials();

            bool meseSucc = DateTime.Today.AddDays(5).Month != DateTime.Today.Month;

            try
            {
                if (!System.Diagnostics.Debugger.IsAttached || System.Security.Principal.WindowsIdentity.GetCurrent().Name.StartsWith("RAI"))
                {
                    List<ScadenzarioMeseAnnoItem> tmp = new List<ScadenzarioMeseAnnoItem>();
                    ScadenzarioMeseAnnoResponse resp = null;
                    resp = serv.GetScadenzarioMeseAnno("0", DateTime.Today.Month, DateTime.Today.Year);
                    if (resp != null && resp.esito)
                        tmp.AddRange(resp.scadenze);

                    if (meseSucc)
                    {
                        resp = serv.GetScadenzarioMeseAnno("0", DateTime.Today.AddDays(5).Month, DateTime.Today.AddDays(5).Year);
                        if (resp != null && resp.esito)
                            tmp.AddRange(resp.scadenze);
                    }

                    scadenze = tmp.Where(x => x.data_scadenza >= DateTime.Today).OrderBy(x => x.data_scadenza).ThenBy(x => x.ora).ToList();
                }
            }
            catch (Exception)
            {


            }

            return PartialView("subpartial/Scadenzario", scadenze);
        }

        public PartialViewResult GetCalendario(int? AnnoRichiesto, int? MeseRichiesto)
        {
            myRaiCommonModel.Calendario model = new myRaiCommonModel.Calendario();
            model.NormalizzaMese(MeseRichiesto, AnnoRichiesto);
            return PartialView("~/Views/Scrivania/subpartial/Calendario.cshtml", model);
        }

        public void TimeStamper(Dictionary<string, DateTime> times)
        {
            string desc = "";
            int counter = 0;
            foreach (var item in times)
            {
                if (counter == 0) desc += item.Key + ":0\n";
                else desc += item.Key + ":" + (times.ElementAt(counter).Value - times.ElementAt(counter - 1).Value).TotalMilliseconds.ToString() + "\n";
                counter++;
            }

            Logger.LogAzione(new MyRai_LogAzioni()
            {
                applicativo = "Portale",
                data = DateTime.Now,
                descrizione_operazione = desc,
                matricola = CommonHelper.GetCurrentUserMatricola(),
                operazione = "Timestamper",
                provenienza = "Index_section1"
            });
        }


        public ActionResult Index_section1()
        {
            try
            {

                SectionAlertModel model = new SectionAlertModel() { Alerts = new List<AlertModel>() };
                List<MyRai_Sezioni_Visibili> sezionivisibili = HomeManager.GetSezioniVisibili(CommonHelper.GetCurrentUserMatricola());


                return View(model);
            }
            catch (Exception ex)
            {
                return Content(ex.ToString());
            }
        }

        public ActionResult Index_section2()
        {
            SectionMetersModel model = new SectionMetersModel();

            return View(model);
        }

        public ActionResult Index_section3(bool GappClosed)
        {
            SectionDayModel model = new SectionDayModel() { GappClosed = GappClosed };
            string matr = CommonHelper.GetCurrentUserMatricola();


            List<MyRai_Sezioni_Visibili> sezionivisibili = HomeManager.GetSezioniVisibili(CommonHelper.GetCurrentUserMatricola());

            model.Notifiche = new myRaiData.digiGappEntities().MyRai_Notifiche
                              .Where(x => x.matricola_destinatario == matr && x.data_letta == null)
                              .OrderByDescending(x => x.data_inserita).ToList();

            model.NotificaVisibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "NOTIF").Count() > 0;
            model.AttivitaVisibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "ATTSET").Count() > 0;
            model.CoseDaFareVisibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "DAFARE").Count() > 0;
            model.BustaPagaVisibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "BUSTA").Count() > 0;
            model.OrarioVisibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "OGGI").Count() > 0;
            model.TimbraturaVisibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "TIMB").Count() > 0;
            model.ScelteVisibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "SCELTE").Count() > 0;
            model.WeekPlanVisibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "APLAN").Count() > 0;

            if (model.WeekPlanVisibile && UtenteHelper.GestitoSirio())
            {
                string attMatr = CommonHelper.GetCurrentUserPMatricola();
                DateTime rifDate = DateTime.Today;

                model.WeekPlan = myRaiHelper.Sirio.Helper.GetWeekPlan(attMatr, rifDate, rifDate.AddDays(6));
            }
            int y = DateTime.Now.Year;

            string rep = UtenteHelper.Reparto();
            if (String.IsNullOrWhiteSpace(rep) || rep.Trim() == "0" || rep.Trim() == "00") rep = null;
            string codSede = UtenteHelper.SedeGapp(DateTime.Now);
            string mysede = UtenteHelper.SedeGapp(DateTime.Now) + rep;


            return View(model);
        }

        public ActionResult Index_section4(bool GappClosed)
        {
            SituazioniEAnagraficaModel model = new SituazioniEAnagraficaModel() { GappClosed = GappClosed };
            model.Boxes = new List<BoxSituazioneModel>();
            List<MyRai_Sezioni_Visibili> sezionivisibili = HomeManager.GetSezioniVisibili(CommonHelper.GetCurrentUserMatricola());
            model.Visibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "ANAG").Count() > 0;
            model.BustaPagaVisibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "BUSTA").Count() > 0;
            model.Boxes.Add(new BoxSituazioneModel()
            {
                CifraPrincipale = "€89.90",
                AllaData = "al 24/05",
                HrefPulsante = "/home",
                TestoPulsante = "VEDI SPESE",
                Titolo = "Nota spese - Trasferte",
                ClasseIcona = "icons icon-briefcase fa-2x ",
                ColoreIcona = "text-primary",
                Visibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "TRASF").Count() > 0
            });
            model.Boxes.Add(new BoxSituazioneModel()
            {
                CifraPrincipale = "€109.90",
                AllaData = "al 20/05",
                HrefPulsante = "/home",
                TestoPulsante = "VEDI TUTTO",
                Titolo = "Situazione Debitoria",
                ClasseIcona = "icons icon-wallet fa-2x ",
                ColoreIcona = "text-primary",
                Visibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "DEB").Count() > 0
            });

            model.Boxes.Add(new BoxSituazioneModel()
            {
                CifraPrincipale = "€99.00",
                AllaData = "al 31/05",
                HrefPulsante = "/home",
                TestoPulsante = "VEDI TUTTO",
                Titolo = "Situazione ?",
                ClasseIcona = "icons icon-wallet fa-2x ",
                ColoreIcona = "text-primary",
                Visibile = sezionivisibili.Where(x => x.Sigla_Sezione.TrimEnd() == "SIT?").Count() > 0
            });

            return View(model);
        }

        public ActionResult CancellaNotifica(long id)
        {
            string matricola = CommonHelper.GetCurrentUserMatricola();
            var db = new myRaiData.digiGappEntities();
            var notifica = db.MyRai_Notifiche.Where(x => x.id == id).FirstOrDefault();
            if (notifica != null)
            {
                notifica.data_letta = DateTime.Now;
                if (DBHelper.Save(db, matricola))
                {
                    NotifichePopupModel m = new NotifichePopupModel(matricola);

                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new
                        {
                            result = "OK",
                            totalNow = m.NotificheTotali,
                            totalNow1 = m.NotificheTotaliTipo1
                        }
                    };
                }
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new
                {
                    result = "Errore dati DB"
                }
            };
        }

        public ActionResult CancellaNotificheAll(string matricola, int tipo)
        {
            var db = new myRaiData.digiGappEntities();
            var notifiche = db.MyRai_Notifiche.Where(x => x.matricola_destinatario == matricola);
            if (tipo == 1)
                notifiche = notifiche.Where(x => x.tipo == 1);
            else
                notifiche = notifiche.Where(x => x.tipo == 2 || x.tipo == null);

            if (notifiche != null)
            {
                foreach (var item in notifiche)
                {
                    item.data_letta = DateTime.Now;
                }
                if (DBHelper.Save(db, matricola))
                {
                    NotifichePopupModel m = new NotifichePopupModel(matricola);
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new
                        {
                            result = "OK",
                            totalNow = m.NotificheTotali,
                            totalNow1 = m.NotificheTotaliTipo1
                        }
                    };
                }
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new
                {
                    result = "Nessuna notifica"
                }
            };
        }

        public ActionResult GetNotifiche(int tipo)
        {
            NotifichePopupModel model = new NotifichePopupModel(CommonHelper.GetCurrentUserMatricola());
            model.tipoNotifiche = tipo;

            if (tipo == 1)
            {
                model.Notifiche = model.Notifiche.Where(x => x.tipo == 1).ToList();
                model.NotificheTotali = model.Notifiche.Count();
                model.Notifiche = model.Notifiche.Take(3).ToList();
            }
            if (tipo == 2)
            {
                model.Notifiche = model.Notifiche.Where(x => x.tipo == 2 || x.tipo == null).ToList();
                model.NotificheTotali = model.Notifiche.Count();
                model.Notifiche = model.Notifiche.Take(3).ToList();
            }
            return View("~/Views/Shared/header2_notifiche.cshtml", model);
        }

        public ActionResult GetNews()
        {
            RaipermeNewsModel model = ScrivaniaManager.GetRaipermeNewsModel("Main news");
            if (model.NewsItems.Count == 0)
                return Content("NONEWS");
            else
                return View(model);
        }

        public ActionResult GetCercaDipendente()
        {
            CercaDipendenteModel model = new CercaDipendenteModel();

            return View("~/Views/CercaDipendente/Index.cshtml", model);
        }
    }

    public class ScrivaniaControllerScope : SessionScope<ScrivaniaControllerScope>
    {
        public DateTime? DataChiusura1 { get; set; }
    }
}