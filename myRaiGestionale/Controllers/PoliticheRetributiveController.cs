using DocumentFormat.OpenXml.EMMA;
using Microsoft.Ajax.Utilities;
using myRai.DataAccess;
using myRaiCommonManager;
using myRaiCommonModel;
using myRaiCommonModel.Gestionale;
using myRaiCommonModel.Gestionale.HRDW;
using myRaiData.Incentivi;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;

namespace myRaiGestionale.Controllers
{
    public class UtenteAutorizzatoAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!System.Diagnostics.Debugger.IsAttached || !UtenteHelper.IsAdmin(CommonHelper.GetCurrentUserMatricola()))
                filterContext.Result = new RedirectResult("/home/NotAuth");
        }
    }

    public class PoliticheRetributiveController : BaseCommonController
    {
        private static string _urlFotoParam = "";

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!PoliticheRetributiveHelper.EnabledToRichieste(CommonHelper.GetCurrentUserMatricola()) && !PoliticheRetributiveHelper.EnabledToLettere(CommonHelper.GetCurrentUserMatricola()))
            {
                filterContext.Result = new RedirectResult("/Home/notAuth");
                return;
            }

            SessionHelper.Set("GEST_SECTION", "GESTIONE");

            base.OnActionExecuting(filterContext);
        }

        public ActionResult Index()
        {
            PolRetrLayout layout = new PolRetrLayout();

            List<string> abilSottoFunz = PoliticheRetributiveHelper.EnableSubFunction();

            HrisHelper.LogOperazione("PRetribLogAuth", String.Join(",", abilSottoFunz), true);

            using (IncentiviEntities db = new IncentiviEntities())
            {
                foreach (var item in db.XR_PRV_BOX.Where(x => x.SEZIONE == "WDGT_DX" && (x.LV_ABIL == "" || abilSottoFunz.Any(a => x.LV_ABIL.Contains(a)))).OrderBy(y => y.ORDINE))
                {
                    switch (item.NOME)
                    {
                        case "ADD_DIP":
                            if (!PoliticheRetributiveHelper.EnableGest(PolRetrChiaveEnum.Richieste)) continue;
                            break;
                        case "BUDGET":
                            if (!PoliticheRetributiveHelper.EnableVis(PolRetrChiaveEnum.BudgetQIO) && !PoliticheRetributiveHelper.EnableVis(PolRetrChiaveEnum.BudgetRS)) continue;
                            break;
                    }
                    layout.BoxAbilitati.Add(item);
                }

                Expression<Func<XR_PRV_DIPENDENTI, bool>> funcFilterAbilServizio = PoliticheRetributiveHelper.FuncFilterDirezione(db);
                Expression<Func<XR_PRV_DIPENDENTI, bool>> funcFilterAbilMatr = PoliticheRetributiveHelper.FuncFilterMatr(db);
                Expression<Func<XR_PRV_DIPENDENTI, bool>> funcFilterAreaPratica = PoliticheRetributiveHelper.FuncFilterAreaPratica();
                Expression<Func<XR_PRV_DIPENDENTI, bool>> funcFilterConsolidated = PoliticheRetributiveHelper.FuncFilterConsolidated();

                layout.HasAnyConsolidated = db.XR_PRV_DIPENDENTI.Where(funcFilterAreaPratica).Where(funcFilterAbilServizio).Where(funcFilterAbilMatr).Any(funcFilterConsolidated);

                if (db.XR_PRV_BOX.Any(x => x.SEZIONE == "EXTRA_DX" && x.NOME == "RDOPPIE" && (x.LV_ABIL == "" || abilSottoFunz.Any(a => x.LV_ABIL.Contains(a)))))
                    layout.RichiesteDoppie = PoliticheRetributiveHelper.MatricoleDoppieRichieste();
            }

            return View("~/Views/PoliticheRetributive/Index.cshtml", layout);
        }

        public static bool AbilitaGestioneLettera()
        {
            bool abilitaLettere = false;

            using (myRaiData.digiGappEntities db = new myRaiData.digiGappEntities())
            {
                var param = db.MyRai_ParametriSistema.FirstOrDefault(x => x.Chiave == "PoliticheAbilitaGestLettera");
                abilitaLettere = param != null && !String.IsNullOrWhiteSpace(param.Valore1) && (param.Valore1 == "*" || param.Valore1.Contains(CommonHelper.GetCurrentUserMatricola()));
            }

            return abilitaLettere;
        }

        #region CaricamentoDati
        public ActionResult CaricamentoDati(bool clearData = false, bool reloadData = false, bool correctData = false)
        {
            bool needRedirect = false;

            if (System.Diagnostics.Debugger.IsAttached && clearData)
            {
                IncentiviEntities db = new IncentiviEntities();
                db.XR_PRV_TEMPLATE.Clear();
                DBHelper.Save(db, "PoliticheRetributive - ");

                needRedirect = true;
            }

            if (System.Diagnostics.Debugger.IsAttached && reloadData)
            {
                CaricamentoTemplate();
                needRedirect = true;
            }

            if (System.Diagnostics.Debugger.IsAttached && correctData)
            {
                PoliticheRetributiveHelper.ConversionePrEffettivo();
                needRedirect = true;
            }

            if (!needRedirect)
                return RedirectToAction("Index");

            return Content("Something change");
        }

        private void CaricamentoTemplate()
        {
            IncentiviEntities db = new IncentiviEntities();

            var signFv = System.IO.File.ReadAllBytes(@"c:\tmp\sign FV.png");
            foreach (var item in db.XR_PRV_TEMPLATE.Where(x => x.NOME == "SIGN FV"))
            {
                item.TEMPLATE = signFv;
            }

            var signFDL = System.IO.File.ReadAllBytes(@"c:\tmp\sign FDL.png");
            foreach (var item in db.XR_PRV_TEMPLATE.Where(x => x.NOME == "SIGN FDL"))
            {
                item.TEMPLATE = signFDL;
            }

            var signSB = System.IO.File.ReadAllBytes(@"c:\tmp\sign SB.png");
            foreach (var item in db.XR_PRV_TEMPLATE.Where(x => x.NOME == "SIGN SB"))
            {
                item.TEMPLATE = signSB;
            }

            db.SaveChanges();
        }

        private static void CaricamentoRisorseChiave()
        {
            IncentiviEntities db = new IncentiviEntities();
            XR_PRV_AREA area = null;
            XR_PRV_DIREZIONE direzione = null;

            var dirList = db.XR_PRV_DIREZIONE.ToList();

            area = db.XR_PRV_AREA.FirstOrDefault(x => x.NOME == "AREA RISORSE CHIAVE");

            if (area == null)
            {
                //creazione area/direzioni risorse chiave
                area = new XR_PRV_AREA();
                area.ID_AREA = db.XR_PRV_AREA.GeneraPrimaryKey();
                area.NOME = "AREA RISORSE CHIAVE";
                area.LV_ABIL = "BDGRS";
                area.COD_USER = "BATCHSESSION";
                area.COD_TERMID = "::1";
                area.TMS_TIMESTAMP = DateTime.Now;

                db.XR_PRV_AREA.Add(area);

                foreach (var dir in dirList)
                {
                    direzione = new XR_PRV_DIREZIONE();
                    direzione.ID_AREA = area.ID_AREA;
                    direzione.ID_DIREZIONE = db.XR_PRV_DIREZIONE.GeneraPrimaryKey();
                    direzione.CODICE = dir.CODICE;
                    direzione.NOME = dir.NOME;
                    direzione.ORDINE = dir.ORDINE;
                    direzione.COD_USER = "BATCHSESSION";
                    direzione.COD_TERMID = "::1";
                    direzione.TMS_TIMESTAMP = DateTime.Now;
                    area.XR_PRV_DIREZIONE.Add(direzione);
                }
            }

            //creazione "Nessun piano" per risorse chiave
            XR_PRV_CAMPAGNA campagna = new XR_PRV_CAMPAGNA();
            campagna.ID_CAMPAGNA = 2;
            campagna.NOME = "Nessun piano";
            campagna.DTA_INIZIO = new DateTime(2019, 3, 1);
            campagna.DTA_FINE = null;

            string codUser = "";
            string codTermid = "";
            DateTime timestamp;
            CezanneHelper.GetCampiFirma(out codUser, out codTermid, out timestamp);
            campagna.COD_USER = codUser;
            campagna.COD_TERMID = codTermid;
            campagna.TMS_TIMESTAMP = timestamp;
            db.XR_PRV_CAMPAGNA.Add(campagna);

            XR_PRV_CAMPAGNA_BUDGET budget = new XR_PRV_CAMPAGNA_BUDGET();
            budget.ID_BUDGET = db.XR_PRV_CAMPAGNA_BUDGET.GeneraPrimaryKey();
            budget.ID_CAMPAGNA = campagna.ID_CAMPAGNA;
            budget.ID_AREA = area.ID_AREA;
            budget.BUDGET = 0;
            budget.BUDGET_PERIODO = 0;
            CezanneHelper.GetCampiFirma(out codUser, out codTermid, out timestamp);
            budget.COD_USER = codUser;
            budget.COD_TERMID = codTermid;
            budget.TMS_TIMESTAMP = timestamp;
            db.XR_PRV_CAMPAGNA_BUDGET.Add(budget);

            foreach (var dir in area.XR_PRV_DIREZIONE)
            {
                XR_PRV_CAMPAGNA_DIREZIONE dirBudget = new XR_PRV_CAMPAGNA_DIREZIONE();
                dirBudget.ID_CAMPAGNA_DIR = db.XR_PRV_CAMPAGNA_DIREZIONE.GeneraPrimaryKey();
                dirBudget.ID_DIREZIONE = dir.ID_DIREZIONE;
                dirBudget.ORGANICO = 0;
                dirBudget.ORGANICO_AD = 0;
                dirBudget.BUDGET = 0;
                dirBudget.BUDGET_PERIODO = 0;
                dirBudget.ID_CAMPAGNA = budget.ID_CAMPAGNA;
                CezanneHelper.GetCampiFirma(out codUser, out codTermid, out timestamp);
                dirBudget.COD_USER = codUser;
                dirBudget.COD_TERMID = codTermid;
                dirBudget.TMS_TIMESTAMP = timestamp;
                db.XR_PRV_CAMPAGNA_DIREZIONE.Add(dirBudget);
            }

            db.SaveChanges();
        }

        #endregion

        #region AggiornamentoDati
        public ActionResult AggiornamentoVariabili()
        {
            //Il controllo IsAttached serve solo perchè così si controlla la condizione prima di lanciare l'aggiornamento
            if (!System.Diagnostics.Debugger.IsAttached || !UtenteHelper.IsAdmin(CommonHelper.GetCurrentUserMatricola()))
                return Redirect("/Home/notAuth");

            IncentiviEntities db = new IncentiviEntities();
            string termId = System.Web.HttpContext.Current.Request.UserHostAddress;

            PoliticheRetributiveManager.WriteLog(db, 0, "Aggiornamento massivo variabili");

            CezanneHelper.GetCampiFirma(out CampiFirma campiFirma);
            //var listDip = db.XR_PRV_DIPENDENTI.ToList();
            var listDip = db.XR_PRV_DIPENDENTI.Where(x => x.XR_PRV_CAMPAGNA.NOME.Contains("2022"));
            foreach (var dip in listDip)
            {
                string matricola = dip.MATRICOLA;

                HRDWData.RecuperoVariabili(db, dip, false, campiFirma);
                HRDWData.RecuperoReperibilita(db, dip, false, campiFirma);
                HRDWData.CalcoloCosti(db, dip, false, campiFirma); //Aggiornamento variabili
            }

            return RedirectToAction("Index");
        }

        public ActionResult AggiornamentoCosti()
        {
            //Il controllo IsAttached serve solo perchè così si controlla la condizione prima di lanciare l'aggiornamento
            if (!System.Diagnostics.Debugger.IsAttached || !UtenteHelper.IsAdmin(CommonHelper.GetCurrentUserMatricola()))
                return Redirect("/Home/notAuth");

            IncentiviEntities db = new IncentiviEntities();

            //string elencoMatricole = " select dip.matricola from xr_prv_dipendenti dip join xr_prv_campagna cam on dip.id_campagna=cam.id_campagna where cam.nome like '%2022%' ";
            //string elencoMatricole = " select matricola from xr_prv_dipendenti where decorrenza>getdate() ";
            //string elencoMatricole = "select matricola from xr_prv_dipendenti dip left join XR_PRV_DIPENDENTI_VARIAZIONI variazioni on dip.id_dipendente = variazioni.id_dipendente where variazioni.id_dipendente is null";
            //string elencoMatricole = "select t0.matricola from xr_prv_dipendenti t0 join xr_prv_campagna t1 on t0.id_campagna = t1.id_campagna where t0.decorrenza > getdate() and t1.lv_abil = 'BDGRS'";


            //string elencoMatricole = " select matricola from xr_prv_dipendenti where decorrenza = '2022-12-01 00:00:00.000' ";

            // prende tutte le matricole che fanno parte della campagna 2022, ma che non hanno simulazioni attive
            //string elencoMatricole = " Select myq.* from ( " +
            //                            " select DISTINCT(dip.matricola)" +
            //                            " from xr_prv_dipendenti dip" +
            //                            " join xr_prv_campagna cam" +
            //                            " on dip.id_campagna = cam.id_campagna" +
            //                            " join XR_PRV_DIPENDENTI_VARIAZIONI v" +
            //                            " on dip.id_dipendente = v.id_dipendente" +
            //                            " where cam.nome like '%2022%'" +
            //                            " and id_simulazione_provenienza is null ) myq ";


            //string elencoMatricole = "Select myq.* from ( " +
            //        "					select DISTINCT(dip.matricola)" +
            //        "					from xr_prv_dipendenti dip" +
            //        "					join xr_prv_campagna cam" +
            //        "					on dip.id_campagna = cam.id_campagna" +
            //        "					join XR_PRV_DIPENDENTI_VARIAZIONI v" +
            //        "					on dip.id_dipendente = v.id_dipendente" +
            //        "					where cam.nome like '%2022%'" +
            //        "					and id_simulazione_provenienza is null " +
            //        "				) myq" +
            //        "	where " +
            //        "	myq.matricola in (" +
            //        "'359105'," +
            //        "'283320'," +
            //        "'418005'," +
            //        "'289839'," +
            //        "'096650'," +
            //        "'027550'," +
            //        "'236080'," +
            //        "'239215'," +
            //        "'497581'," +
            //        "'311010'," +
            //        "'360998'," +
            //        "'114793'," +
            //        "'369380'," +
            //        "'334323'," +
            //        "'903370'," +
            //        "'859779'," +
            //        "'643561'," +
            //        "'846945'," +
            //        "'689209'," +
            //        "'253742'," +
            //        "'134234'," +
            //        "'833030'," +
            //        "'839058'," +
            //        "'839058'," +
            //        "'335222'," +
            //        "'335222'," +
            //        "'303230'," +
            //        "'803213'," +
            //        "'214315'," +
            //        "'356240'," +
            //        "'356240'," +
            //        "'056828'," +
            //        "'354174'," +
            //        "'099512'," +
            //        "'869520'," +
            //        "'869520'," +
            //        "'906066'," +
            //        "'686865'," +
            //        "'686865'," +
            //        "'628174'," +
            //        "'628174'," +
            //        "'910095'," +
            //        "'204716'," +
            //        "'204716'," +
            //        "'212401'," +
            //        "'212401'," +
            //        "'831010'," +
            //        "'831010'," +
            //        "'906350'," +
            //        "'797635'," +
            //        "'679935'," +
            //        "'408147'," +
            //        "'761186'," +
            //        "'118175'," +
            //        "'118175'," +
            //        "'209601'," +
            //        "'394183'," +
            //        "'181642'," +
            //        "'734013'," +
            //        "'974203'," +
            //        "'258575'," +
            //        "'069864'," +
            //        "'529919'," +
            //        "'570838'," +
            //        "'657950'," +
            //        "'702780'," +
            //        "'523380'," +
            //        "'717598'," +
            //        "'870143'," +
            //        "'549816'," +
            //        "'420747'," +
            //        "'420747'," +
            //        "'383970'," +
            //        "'123130'," +
            //        "'123130'," +
            //        "'807223'," +
            //        "'887560'," +
            //        "'107486'," +
            //        "'267548'," +
            //        "'059640'," +
            //        "'059640'," +
            //        "'332021'," +
            //        "'332021'," +
            //        "'190937'," +
            //        "'983114'," +
            //        "'983114'," +
            //        "'311166'," +
            //        "'953750'," +
            //        "'953750'," +
            //        "'787295'," +
            //        "'468320'," +
            //        "'945094'," +
            //        "'036882'," +
            //        "'036882'," +
            //        "'671978'," +
            //        "'671978'," +
            //        "'071870'," +
            //        "'712930'," +
            //        "'712930'," +
            //        "'580920'," +
            //        "'149921'," +
            //        "'723782'," +
            //        "'841443'," +
            //        "'802401'," +
            //        "'010537'," +
            //        "'400629'," +
            //        "'816380'," +
            //        "'878979'," +
            //        "'031610'," +
            //        "'031610'," +
            //        "'817010'," +
            //        "'817010'," +
            //        "'791165'," +
            //        "'791165'," +
            //        "'187285'," +
            //        "'256223'," +
            //        "'533591'," +
            //        "'679254'," +
            //        "'799483'," +
            //        "'082000'," +
            //        "'562690'," +
            //        "'583152'," +
            //        "'347492'," +
            //        "'177368'," +
            //        "'732427'," +
            //        "'830200' " +
            //        ")";

            //string elencoMatricole = "Select myq.* from ( " +
            //        "					select DISTINCT(dip.matricola)" +
            //        "					from xr_prv_dipendenti dip" +
            //        "					join xr_prv_campagna cam" +
            //        "					on dip.id_campagna = cam.id_campagna" +
            //        "					join XR_PRV_DIPENDENTI_VARIAZIONI v" +
            //        "					on dip.id_dipendente = v.id_dipendente" +
            //        "					where cam.nome like '%2022%' and dip.STATO_LETTERA is null " +
            //        "					and id_simulazione_provenienza is null " +
            //        "				) myq" +
            //        "	where " +
            //        "	myq.matricola in (" +
            //        "'515682'" +
            //        ")";

            // FRANCESCO, rigenerati i conteggi perchè mancavano i record sulla XR_PRV_DIPENDENTI_VARIAZIONI
            //string elencoMatricole = "Select MATRICOLA from XR_PRV_DIPENDENTI WHERE matricola ='029130'";
            //string elencoMatricole = "Select MATRICOLA from XR_PRV_DIPENDENTI WHERE matricola ='436876'";

            //string elencoMatricole = "Select MATRICOLA from XR_PRV_DIPENDENTI WHERE matricola ='058198'";

            string elencoMatricole = "Select MATRICOLA from XR_PRV_DIPENDENTI WHERE matricola in ('058198','948615','754590','000232') and YEAR(TMS_TIMESTAMP) = 2023 and YEAR(DECORRENZA) = 2023";

            PoliticheRetributiveManager.WriteLog(db, 0, "Aggiornamento massivo costi provvedimenti");
            CezanneHelper.GetCampiFirma(out var campiFirma);
            HRDWData.CalcoloCostiMassivo(db, campiFirma, elencoMatricole, x => x.XR_PRV_CAMPAGNA.NOME.Contains("2022"), updateManual: true);

            return RedirectToAction("Index");
        }
        #endregion

        #region Action
        public ActionResult SelezioneDipendenti()
        {
            return View("~/Views/PoliticheRetributive/subpartial/SelezioneDipendenti.cshtml");
        }
        [HttpPost]
        public ActionResult RicercaAnagrafiche(RicercaAnagrafica model)
        {
            IncentiviEntities db = new IncentiviEntities();
            ElencoAnagrafiche elencoAnagrafiche = new ElencoAnagrafiche();
            elencoAnagrafiche.IdCampagna = model.Piano;
            if (model.Piano > 1 && !String.IsNullOrWhiteSpace(model.Decorrenza))
                elencoAnagrafiche.Decorrenza = DateTime.ParseExact(model.Decorrenza, "dd/MM/yyyy", null);


            Expression<Func<SINTESI1, bool>> funcFilterAbilServizio = PoliticheRetributiveHelper.FuncFilterDirezioneSintesi(db);
            Expression<Func<SINTESI1, bool>> funcFilterMatr = PoliticheRetributiveHelper.FuncFilterMatrSintesi(db);

            string parametri = "";
            bool applyFilterStates = PoliticheRetributiveHelper.GetEnabledDirezioni(db, CommonHelper.GetCurrentUserMatricola(), out List<int> enableDir);
            parametri += String.Format("Direzione: {0} - {1}\r\n", applyFilterStates, String.Join(",", enableDir));
            applyFilterStates = PoliticheRetributiveHelper.GetEnabledCategory(db, CommonHelper.GetCurrentUserMatricola(), out List<string> categoryIncluded, out List<string> categoryExcluded);
            parametri += String.Format("Categorie: {0} - Incluse: {1} - Escluse: {2}", applyFilterStates, categoryIncluded != null ? String.Join(",", categoryIncluded) : "null", categoryExcluded != null ? String.Join(",", categoryExcluded) : "null");

            HrisHelper.LogOperazione("PRetribLogAuth", parametri, true);

            if (model.HasFilter)
            {
                IQueryable<SINTESI1> tmp = db.SINTESI1.Include("XR_PRV_DIPENDENTI").Where(x => x.DTA_FINE_CR > DateTime.Today);

                if (!String.IsNullOrWhiteSpace(model.Matricola))
                    tmp = tmp.Where(x => model.Matricola.Contains(x.COD_MATLIBROMAT));

                if (!String.IsNullOrWhiteSpace(model.Cognome))
                    tmp = tmp.Where(x => x.DES_COGNOMEPERS.StartsWith(model.Cognome.ToUpper()));

                if (!String.IsNullOrWhiteSpace(model.Nome))
                    tmp = tmp.Where(x => x.DES_NOMEPERS.StartsWith(model.Nome.ToUpper()));

                if (!String.IsNullOrWhiteSpace(model.Servizio))
                    tmp = tmp.Where(x => x.COD_SERVIZIO == model.Servizio);

                tmp = tmp.Where(funcFilterAbilServizio).Where(funcFilterMatr);

                tmp = tmp.OrderBy(y => y.DES_COGNOMEPERS).ThenBy(z => z.DES_NOMEPERS);

                elencoAnagrafiche.anagrafiche = tmp.ToList();
            }

            return View("~/Views/PoliticheRetributive/subpartial/" + model.ResultView + ".cshtml", elencoAnagrafiche);
        }

        public ActionResult GetElencoPratiche()
        {
            return RicercaPratiche(new RicercaAnagrafica() { ResultView = "ElencoPratiche" });
        }
        

        public ActionResult RicercaPratiche(RicercaAnagrafica model)
        {
            IncentiviEntities db = new IncentiviEntities();
            IQueryable<XR_PRV_DIPENDENTI> qryPratiche = null;

            Expression<Func<XR_PRV_DIPENDENTI, bool>> funcFilterAbilServizio = PoliticheRetributiveHelper.FuncFilterDirezione(db,true);
            Expression<Func<XR_PRV_DIPENDENTI, bool>> funcFilterAbilMatr = PoliticheRetributiveHelper.FuncFilterMatr(db);
            Expression<Func<XR_PRV_DIPENDENTI, bool>> funcFilterAreaPratica = PoliticheRetributiveHelper.FuncFilterAreaPratica(true);
            Expression<Func<XR_PRV_DIPENDENTI, bool>> funcFilterAbil = PoliticheRetributiveHelper.FuncFilterAbil();

            qryPratiche = db.XR_PRV_DIPENDENTI
                .Include("SINTESI1")
                .Include("XR_PRV_OPERSTATI")
                .Include("XR_PRV_PROV_RICH")
                //.Include("XR_PRV_PROV_EFFETTIVO")
                .Include("XR_PRV_DIPENDENTI_VARIAZIONI")
                .Include("XR_PRV_CAMPAGNA")
                .Include("XR_PRV_DIPENDENTI_NOTE")
                .Include("XR_PRV_DIREZIONE")
            .Where(funcFilterAbil)
            .Where(funcFilterAreaPratica)
            .Where(funcFilterAbilServizio)
            .Where(funcFilterAbilMatr);
          
            if (!String.IsNullOrWhiteSpace(model.Matricola))
                qryPratiche = qryPratiche.Where(x => model.Matricola.Contains(x.MATRICOLA));

            if (!String.IsNullOrWhiteSpace(model.Cognome))
                qryPratiche = qryPratiche.Where(x => x.SINTESI1.DES_COGNOMEPERS.StartsWith(model.Cognome.ToUpper()));

            if (!String.IsNullOrWhiteSpace(model.Nome))
                qryPratiche = qryPratiche.Where(x => x.SINTESI1.DES_NOMEPERS.StartsWith(model.Nome.ToUpper()));

            if (model.Provvedimento > 0)
            {
                var anyOfThisProv = PoliticheRetributiveHelper.AnyOfProv(model.Provvedimento);
                qryPratiche = qryPratiche.Where(anyOfThisProv);
            }

            if (model.Stato > 0)
                qryPratiche = qryPratiche.Where(x => x.XR_PRV_OPERSTATI.Any() && x.XR_PRV_OPERSTATI.Where(y => y.DATA_FINE_VALIDITA == null).Max(z => z.ID_STATO) == model.Stato);

            if (model.IdDirezionePratica > 0)
                qryPratiche = qryPratiche.Where(x => x.ID_DIREZIONE == model.IdDirezionePratica);

            if (model.IdAreaPratica > 0)
                qryPratiche = qryPratiche.Where(x => x.XR_PRV_DIREZIONE.ID_AREA == model.IdAreaPratica);

            if (model.Piano > 0)
                qryPratiche = qryPratiche.Where(x => x.ID_CAMPAGNA == model.Piano);

            if (!String.IsNullOrWhiteSpace(model.GestioneManuale))
            {
                switch (model.GestioneManuale)
                {
                    case "0":
                        qryPratiche = qryPratiche.Where(x => x.CUSTOM_PROV == null || !x.CUSTOM_PROV.Value);
                        break;
                    case "1":
                        qryPratiche = qryPratiche.Where(x => x.CUSTOM_PROV != null && x.CUSTOM_PROV.Value);
                        break;
                    default:
                        break;
                }
            }

            if (!String.IsNullOrWhiteSpace(model.GestioneEsterna))
            {
                switch (model.GestioneEsterna)
                {
                    case "0":
                        qryPratiche = qryPratiche.Where(x => x.IND_PRATICA_EXT == null || !x.IND_PRATICA_EXT.Value);
                        break;
                    case "1":
                        qryPratiche = qryPratiche.Where(x => x.IND_PRATICA_EXT != null && x.IND_PRATICA_EXT.Value);
                        break;
                    default:
                        break;
                }
            }

            if (!String.IsNullOrWhiteSpace(model.Categoria))
                qryPratiche = qryPratiche.Where(x => x.SINTESI1.COD_QUALIFICA == model.Categoria);

            if (!String.IsNullOrWhiteSpace(model.Decorrenza))
            {
                DateTime dt = DateTime.ParseExact(model.Decorrenza, "dd/MM/yyyy", null);
                qryPratiche = qryPratiche.Where(x => x.DECORRENZA != null && x.DECORRENZA.Value == dt);
            }

            if (String.IsNullOrWhiteSpace(model.Matricola)
                && String.IsNullOrWhiteSpace(model.Cognome)
                && String.IsNullOrWhiteSpace(model.Nome)
                && model.Provvedimento <= 0
                && model.Stato <= 0
                && model.IdDirezionePratica <= 0
                && model.IdAreaPratica <= 0
                && model.Piano <= 0
                && String.IsNullOrWhiteSpace(model.GestioneManuale)
                && String.IsNullOrWhiteSpace(model.GestioneEsterna)
                && String.IsNullOrWhiteSpace(model.Categoria)
                && String.IsNullOrWhiteSpace(model.Decorrenza)
                )
                qryPratiche = qryPratiche.Where(x => !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa) && (x.XR_PRV_CAMPAGNA.DTA_FINE == null || x.XR_PRV_CAMPAGNA.DTA_FINE > DateTime.Today));

            //qryPratiche = qryPratiche.OrderBy(y => y.SINTESI1.DES_COGNOMEPERS).ThenBy(y=>y.SINTESI1.DES_NOMEPERS);
            //qryPratiche = qryPratiche.OrderBy(x => x.MATRICOLA);

            List<XR_PRV_DIPENDENTI> pratiche = new List<XR_PRV_DIPENDENTI>();
            var tmp = qryPratiche.ToList();
            pratiche.AddRange(tmp.OrderBy(x => x.SINTESI1.DES_COGNOMEPERS).ThenBy(x => x.SINTESI1.DES_NOMEPERS));

            ViewData["EnableGestRich"] = PoliticheRetributiveHelper.EnableGest(PolRetrChiaveEnum.Richieste);
            ViewData["EnableRich"] = PoliticheRetributiveHelper.EnabledToRichieste(CommonHelper.GetCurrentUserMatricola());
            ViewData["EnableLettere"] = PoliticheRetributiveHelper.EnabledToLettere(CommonHelper.GetCurrentUserMatricola());
            ViewData["EnableAmm"] = PoliticheRetributiveHelper.EnabledToAmm(CommonHelper.GetCurrentUserMatricola());

            return View("~/Views/PoliticheRetributive/subpartial/" + model.ResultView + ".cshtml", pratiche);
        }
        public ActionResult GetStringaFiltri(RicercaAnagrafica model)
        {
            string text = "";

            if (model.HasFilter)
            {
                IncentiviEntities db = new IncentiviEntities();

                if (!String.IsNullOrWhiteSpace(model.Matricola))
                    text += " con matricola " + model.Matricola;
                if (!String.IsNullOrWhiteSpace(model.Cognome))
                    text += " con cognome '" + model.Cognome + "'";
                if (!String.IsNullOrWhiteSpace(model.Nome))
                    text += " con nome '" + model.Nome + "'";
                if (!String.IsNullOrWhiteSpace(model.Sede))
                {
                    var sede = db.XR_TB_SEDECONT.FirstOrDefault(x => x.COD_SEDECONT == model.Sede);
                    text += " per la sede '" + sede.DES_SEDECONT + "'";
                }
                if (!String.IsNullOrWhiteSpace(model.Servizio))
                {
                    var servizio = db.XR_TB_SERVIZIO.FirstOrDefault(x => x.COD_SERVIZIO == model.Servizio);
                    if (servizio != null)
                        text += " della direzione '" + servizio.DES_SERVIZIO + "'";
                    else
                    {
                        var prvDir = db.XR_PRV_DIREZIONE.FirstOrDefault(x => x.CODICE == model.Servizio);
                        if (prvDir != null)
                            text += " della direzione '" + servizio.DES_SERVIZIO + "'";
                    }
                }
                if (!String.IsNullOrWhiteSpace(model.Categoria))
                {
                    var categoria = db.QUALIFICA.FirstOrDefault(x => x.COD_QUALIFICA == model.Categoria);
                    text += " con categoria '" + categoria.DES_QUALIFICA + "'";
                }
                if (!String.IsNullOrWhiteSpace(model.PraticaInCarico))
                {
                    switch (model.PraticaInCarico)
                    {
                        case "0":
                            text += " non in carico";
                            break;
                        case "1":
                            text += "in carico";
                            break;
                    }
                }

                if (model.MatricoleMultiple)
                {
                    text = " con richieste multiple";
                }

                if (!String.IsNullOrWhiteSpace(model.GestioneManuale))
                {
                    switch (model.GestioneManuale)
                    {
                        case "0":
                            text += "";
                            break;
                        case "1":
                            text += "";
                            break;
                        default:
                            break;
                    }
                }

                if (!String.IsNullOrWhiteSpace(model.GestioneEsterna))
                {
                    switch (model.GestioneEsterna)
                    {
                        case "0":
                            text += " gestite internamente al sistema";
                            break;
                        case "1":
                            text += " gestite esternamente al sistema";
                            break;
                        default:
                            break;
                    }
                }

                if (!String.IsNullOrWhiteSpace(model.Decorrenza))
                {
                    text += " con decorrenza al " + model.Decorrenza;
                }

                //if (model.EscludiCessati)
                //    text += ", cessati esclusi";
                //else
                //    text += ", cessati inclusi";
            }

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { Filtro = text }
            };
        }

        public ActionResult GetOverview()
        {
            IncentiviEntities db = new IncentiviEntities();

            List<PieItem> listDistroProvv = new List<PieItem>
            {
                new PieItem()
                {
                    color= "#ff0000",
                    label = "Aumento di livello",
                    data = new List<List<int>> { new List<int>() { 1, db.XR_PRV_DIPENDENTI.Count(x => x.ID_PROV_RICH == (int)ProvvedimentiEnum.AumentoLivello) } }
                },
                new PieItem()
                {
                    color= "#ff0000",
                    label = "Aumento di livello con riassorbimento",
                    data = new List<List<int>> { new List<int>() { 1, db.XR_PRV_DIPENDENTI.Count(x => x.ID_PROV_RICH == (int)ProvvedimentiEnum.AumentoLivelloNoAssorbimento) } }
                },
                new PieItem()
                {
                    color= "#00ff00",
                    label = "Aumento di merito",
                    data = new List<List<int>> { new List<int>() { 1, db.XR_PRV_DIPENDENTI.Count(x => x.ID_PROV_RICH == (int)ProvvedimentiEnum.AumentoMerito) } }
                },
                new PieItem()
                {
                    color= "#0000ff",
                    label = "Gratifica",
                    data = new List<List<int>> { new List<int>() { 1, db.XR_PRV_DIPENDENTI.Count(x => x.ID_PROV_RICH == (int)ProvvedimentiEnum.Gratifica) } }
                }
            };

            var cc = db.XR_PRV_DIPENDENTI.GroupBy(x => x.SINTESI1.DES_SEDE);

            var listDistroPerSede = new
            {
                labels = cc.Select(x => x.Key),
                series = new List<List<int>>()
                {
                    cc.Select(x => x.Count(y => y.ID_PROV_RICH == (int)ProvvedimentiEnum.AumentoLivello)).ToList(),
                    cc.Select(x=> x.Count(y => y.ID_PROV_RICH == (int)ProvvedimentiEnum.AumentoMerito)).ToList(),
                    cc.Select(x=> x.Count(y => y.ID_PROV_RICH == (int)ProvvedimentiEnum.Gratifica)).ToList(),
                    cc.Select(x=> x.Count(y => y.ID_PROV_RICH == (int)ProvvedimentiEnum.AumentoLivelloNoAssorbimento)).ToList()
                }
            };

            List<PieItem> listDistroPerSedePie = new List<PieItem>();
            foreach (var item in cc)
            {
                listDistroPerSedePie.Add(new PieItem()
                {
                    label = item.Key,
                    data = new List<List<int>> { new List<int>() { 1, item.Count() } }
                });
            }

            JavaScriptSerializer jsS = new JavaScriptSerializer();
            PraticaOverview praticaOverview = new PraticaOverview()
            {
                DistroProvv = jsS.Serialize(listDistroProvv),
                DistroProvvPerSedeBar = jsS.Serialize(listDistroPerSede),
                DistroProvvPerSede = jsS.Serialize(listDistroPerSedePie)
            };

            return View("~/Views/PoliticheRetributive/subpartial/Overview.cshtml", praticaOverview);
        }

        #region GestionePratica
        [HttpPost]
        public ActionResult CreaPratica(int? idCampagna, int idPersona, int provv, string decorrenza = "")
        {
            string result = "";
            int idDip = 0;

            try
            {
                IncentiviEntities db = new IncentiviEntities();

                int? selCampagna = null;
                if (idCampagna.GetValueOrDefault() > 0)
                    selCampagna = idCampagna;

                XR_PRV_CAMPAGNA campagna = db.XR_PRV_CAMPAGNA.FirstOrDefault(x => x.ID_CAMPAGNA == selCampagna);

                SINTESI1 sint = db.SINTESI1.FirstOrDefault(x => x.ID_PERSONA == idPersona);

                XR_PRV_DIPENDENTI dip = new XR_PRV_DIPENDENTI();
                dip.ID_DIPENDENTE = db.XR_PRV_DIPENDENTI.GeneraPrimaryKey();
                dip.ID_PERSONA = idPersona;
                dip.ID_CAMPAGNA = selCampagna;
                dip.ID_PROV_RICH = provv;
                dip.ID_PROV_EFFETTIVO = provv;
                dip.COD_SEDE = sint.COD_SEDE;
                dip.DES_SEDE = sint.DES_SEDE;
                dip.MATRICOLA = sint.COD_MATLIBROMAT.Trim();
                if (!String.IsNullOrWhiteSpace(decorrenza))
                    dip.DECORRENZA = DateTime.ParseExact(decorrenza, "dd/MM/yyyy", null);

                //Eccezioni particolari
                //string codServ = sint.COD_SERVIZIO.Substring(0, 2);
                string codServ = sint.XR_SERVIZIO.OrderByDescending(x => x.DTA_FINE).FirstOrDefault().COD_SERVIZIO.Substring(0, 2);
                //if (codServ != "20" && sint.COD_UNITAORG.StartsWith("GA") && sint.COD_UNITAORG.EndsWith("1100")) //Chi è della direzione canone, potrebbe essere inquadrato nella direzione coord.sedi
                //    codServ = "20_B";
                //else if (codServ == "64") //Chi ha il servizio 64 - Bolzano deve rientrare sotto 24 - Coord. Sedi Regionali ed estere
                //    codServ = "24";

                var param = HrisHelper.GetParametroJson<PolRetrParam>(HrisParam.PoliticheParametri);
                if (param != null && param.OvverideDirReq != null && param.OvverideDirReq.Any())
                {
                    if (CommonHelper.IsProduzione())
                    {
                        var rule = param.OvverideDirReq.FirstOrDefault(x => x.Corrisponde(codServ, sint.COD_UNITAORG));
                        if (rule != null)
                            codServ = rule.DirDest;
                    }
                }

                XR_PRV_DIREZIONE dir = db.XR_PRV_DIREZIONE.FirstOrDefault(x => x.XR_PRV_AREA.LV_ABIL.Contains(campagna.LV_ABIL) && x.CODICE == codServ);
                if (dir != null)
                {
                    dip.ID_DIREZIONE = dir.ID_DIREZIONE;
                }
                string codUser = "";
                string termID = "";
                DateTime timeStamp;
                CezanneHelper.GetCampiFirma(out CampiFirma campiFirma);
                dip.COD_USER = campiFirma.CodUser;
                dip.COD_TERMID = campiFirma.CodTermid;
                dip.TMS_TIMESTAMP = campiFirma.Timestamp;
                db.XR_PRV_DIPENDENTI.Add(dip);

                DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), "PoliticheRetributive - ");


                bool getHrdwData = false;
                //if (CommonHelper.IsProduzione())
                {
                    try
                    {
                        if (HRDWData.RecuperoProvvedimenti(db, dip, false, campiFirma)
                            && HRDWData.RecuperoRal(db, dip, false, campiFirma)
                            && HRDWData.RecuperoAssenze(db, dip, false, campiFirma)
                            && HRDWData.RecuperoVariabili(db, dip, false, campiFirma)
                            && HRDWData.RecuperoReperibilita(db, dip, false, campiFirma))
                        {
                            HRDWData.CalcoloCosti(db, dip, false, campiFirma); //Creazione pratica
                            PoliticheRetributiveHelper.SetProvEffettivo(db, dip, provv);
                            getHrdwData = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                        }
                        else
                        {
                            getHrdwData = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        getHrdwData = false;
                    }
                }

                if (!getHrdwData)
                {
                    EliminaPratica(db, dip);
                    result = "Errore durante l'acquisizione dei dati";
                }
                else
                {
                    result = "OK";
                    idDip = dip.ID_DIPENDENTE;
                    SalvaStato(dip.ID_DIPENDENTE, 1);
                }

            }
            catch (Exception)
            {
                result = "Errore durante la creazione delle pratica";
            }

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { result = result, idPratica = idDip }
            };
        }

        private static bool EliminaPratica(IncentiviEntities db, XR_PRV_DIPENDENTI dip)
        {
            bool result = false;

            if (db == null)
                db = new IncentiviEntities();

            if (dip != null)
            {
                try
                {
                    PoliticheRetributiveManager.WriteLog(db, 0, "Cancellazione pratica " + dip.ID_DIPENDENTE + " " + dip.SINTESI1.DES_COGNOMEPERS + " " + dip.SINTESI1.DES_NOMEPERS);
                    db.XR_PRV_DIPENDENTI_CAUSESERV.RemoveWhere(x => x.ID_DIPENDENTE == dip.ID_DIPENDENTE);
                    db.XR_PRV_DIPENDENTI_STRAGIUDIZIALE.RemoveWhere(x => x.ID_DIPENDENTE == dip.ID_DIPENDENTE);
                    db.XR_PRV_DIPENDENTI_PROVVD.RemoveWhere(x => x.ID_DIPENDENTE == dip.ID_DIPENDENTE);
                    db.XR_PRV_DIPENDENTI_PROV.RemoveWhere(x => x.ID_DIPENDENTE == dip.ID_DIPENDENTE);
                    db.XR_PRV_DIPENDENTI_RAL.RemoveWhere(x => x.ID_DIPENDENTE == dip.ID_DIPENDENTE);
                    db.XR_PRV_DIPENDENTI_VARIAZIONI.RemoveWhere(x => x.ID_DIPENDENTE == dip.ID_DIPENDENTE);
                    db.XR_PRV_DIPENDENTI_VAR.RemoveWhere(x => x.ID_DIPENDENTE == dip.ID_DIPENDENTE);
                    db.XR_PRV_DIPENDENTI_ASSENZE.RemoveWhere(x => x.ID_DIPENDENTE == dip.ID_DIPENDENTE);
                    db.XR_PRV_DIPENDENTI_NOTE.RemoveWhere(x => x.ID_DIPENDENTE == dip.ID_DIPENDENTE);
                    db.XR_PRV_OPERSTATI.RemoveWhere(x => x.ID_DIPENDENTE == dip.ID_DIPENDENTE);
                    db.XR_PRV_DIPENDENTI.Remove(dip);
                    db.XR_PRV_DIPENDENTI_SIMULAZIONI.RemoveWhere(x => x.ID_DIPENDENTE == dip.ID_DIPENDENTE);
                    DBHelper.Save(db, "PoliticheRetributive - ");

                    result = true;
                }
                catch (Exception)
                {

                }

            }

            return result;
        }
        [HttpPost]
        public ActionResult CancellaPratica(int idDip)
        {
            IncentiviEntities db = new IncentiviEntities();
            XR_PRV_DIPENDENTI dip = db.XR_PRV_DIPENDENTI.FirstOrDefault(x => x.ID_DIPENDENTE == idDip);
            if (EliminaPratica(db, dip))
                return Content("OK");
            else
                return Content("Errore durante la cancellazione della pratica");
        }
        public ActionResult RefreshDatiPratica(int idDip, bool reloadData, string catArrivo, bool IsFromComboChange=false)//M-POLRETR
        {
            IncentiviEntities db = new IncentiviEntities();
            if (reloadData)
            {
                
                CezanneHelper.GetCampiFirma(out var campiFirma);
                XR_PRV_DIPENDENTI dip = db.XR_PRV_DIPENDENTI.FirstOrDefault(x => x.ID_DIPENDENTE == idDip);
                if (dip != null)
                {
                    HRDWData.CalcoloCosti(db, dip, false, campiFirma, true, catArrivo,IsFromComboChange); //Refresh Dati pratica
                    if (catArrivo == "HRDW")
                    {
                        dip.CAT_RICHIESTA = null;//ripristina
                        db.SaveChanges();
                    }
                }

                PoliticheRetributiveManager.WriteLog(db, idDip, "Richiesto aggiornamento costi");
            }


            if (catArrivo == "HRDW")
            {
                catArrivo = null;
            }
            Pratica pratica = GetPratica(idDip);
            pratica.CatArrivo = catArrivo;
            pratica = GetDatiConsiderandoSimulazioni(pratica);
            return View("~/Views/PoliticheRetributive/subpartial/DettaglioAnagrafica.cshtml", pratica);
           
        }

        [HttpPost]
        public ActionResult EliminaRiprovaSimulazione(int idsim, string action)
        {
            var db = new IncentiviEntities();
            var sim = db.XR_PRV_DIPENDENTI_SIMULAZIONI.Where(x => x.ID_SIMULAZIONE == idsim).FirstOrDefault();
            if (sim == null)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore="Simulazione non trovata" }
                };
            }
            else
            {
                if (action == "R")
                {
                    sim.DTA_ACQUISIZIONE = null;
                    sim.DTA_ESECUZIONE = null;
                    sim.NOT_ERROR = null;
                }
                else
                {
                    db.XR_PRV_DIPENDENTI_SIMULAZIONI.Remove(sim);
                }
                db.SaveChanges();
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito =true }
                };
            }
        }

        [HttpPost]
        public ActionResult SimCompleted(int idsim)
        {
            var db = new IncentiviEntities();
            bool t = db.XR_PRV_DIPENDENTI_SIMULAZIONI.Any(x => x.ID_SIMULAZIONE == idsim && 
                        !String.IsNullOrWhiteSpace(x.JSON_SIMULAZIONE));

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = t }
            };
        }

        [HttpPost]
        public ActionResult RichiediSimulazione(int iddip, string catArrivo) //M-POLRETR
        {
            if (String.IsNullOrWhiteSpace(catArrivo))
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = "Categoria di arrivo non trovata" }
                };
            }
            IncentiviEntities db = new IncentiviEntities();
            string matr = db.XR_PRV_DIPENDENTI.Where(x => x.ID_DIPENDENTE == iddip).Select(x => x.MATRICOLA).FirstOrDefault();
            if (matr == null)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = "matricola non trovata"}
                };
            }
            string catAttuale = db.SINTESI1.Where(x => x.COD_MATLIBROMAT == matr).Select(x => x.COD_QUALIFICA).FirstOrDefault();
            if (catAttuale == null)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = "Categoria attuale non trovata" }
                };
            }

            
            var sim = db.XR_PRV_DIPENDENTI_SIMULAZIONI.Where(x => x.MATRICOLA == matr && x.CAT_PARTENZA == catAttuale 
            && x.CAT_ARRIVO == catArrivo && x.ID_DIPENDENTE==iddip).FirstOrDefault();


            if (sim == null)
            {
                XR_PRV_DIPENDENTI_SIMULAZIONI newSim = new XR_PRV_DIPENDENTI_SIMULAZIONI()
                {
                    CAT_ARRIVO = catArrivo,
                    CAT_PARTENZA = catAttuale,
                    MATRICOLA = matr,
                    DTA_RICHIESTA = DateTime.Now,
                    MATR_RICHIESTA = CommonHelper.GetCurrentUserMatricola(),
                    ID_DIPENDENTE = iddip
                };
                db.XR_PRV_DIPENDENTI_SIMULAZIONI.Add(newSim);
                db.SaveChanges();
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true, info = "Simulazione richiesta." }
                };
            }
            else
            {
                if (sim.DTA_ACQUISIZIONE == null || sim.DTA_ESECUZIONE == null)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = true, info = "La stessa simulazione è gia in coda nelle richieste." }
                    };
                }
            }
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = false, errore = "Simulazione già presente e disponibile" }
            };
        }
        public decimal GetDecimalForImporto(string imp)
        {
            decimal d;
            decimal.TryParse(imp.Replace("€", "").Replace(".", "").Replace("-", "").Replace(" ", ""), out d);
            return d;
        }

        public string GetDescIndennita(string cod)
        {
            var query = "SELECT [desc_indennita] FROM [LINK_HRIS_HRLIV2].[HR_LIV2].[dbo].[L2D_INDENNITA] where cod_indennita='" + cod + "' ";
            var db = new IncentiviEntities();
            string desc = db.Database.SqlQuery<string>(query).FirstOrDefault();
            if (desc != null) desc = desc.Trim();
            return desc;

        }
        [HttpPost]
        public ActionResult saveSimulazione(string jsontext)//M-POLRETR
        {
            SalvaDatiSimulazione s = Newtonsoft.Json.JsonConvert.DeserializeObject<SalvaDatiSimulazione>(jsontext);
            if (s != null)
            {
                var db = new IncentiviEntities();
                var simRow = db.XR_PRV_DIPENDENTI_SIMULAZIONI.Where(x => x.ID_SIMULAZIONE == s.id_simulazione).FirstOrDefault();

                foreach (var importi in s.importi)
                {
                    int provv = importi.prov;
                    decimal d1 = GetDecimalForImporto(importi.importo1);
                    decimal d2 = GetDecimalForImporto(importi.importo2);
                    decimal d3 = GetDecimalForImporto(importi.importo3);
                    var Variaz = db.XR_PRV_DIPENDENTI_VARIAZIONI.Where(x => x.ID_DIPENDENTE == s.id_dip && x.ID_PROV == provv).FirstOrDefault();
                    if (Variaz != null)
                    {
                        Variaz.DIFF_RAL = d1;
                        Variaz.COSTO_ANNUO = d2;
                        Variaz.COSTO_PERIODO = d3;
                        Variaz.ID_SIMULAZIONE_PROVENIENZA = s.id_simulazione;
                        if (Variaz.CAT_PREVISTA != null) Variaz.CAT_PREVISTA = s.cat_prevista;

                        if (provv == 1 || provv == 4 || provv == 6 || provv == 7)
                        {
                            if (simRow != null && ! String.IsNullOrWhiteSpace(simRow.JSON_SIMULAZIONE ))
                            {
                                var ind = PoliticheRetributiveManager.GetDatiPrevisionaliFromJSON(simRow.JSON_SIMULAZIONE);
                                if (ind != null && ind.VariazioniIndennita != null && ind.VariazioniIndennita.Any())
                                {
                                    List<string> Acq = new List<string>();
                                    List<string> Perse = new List<string>();
                                    List<string> Delta = new List<string>();
                                    foreach (var indVar in ind.VariazioniIndennita)
                                    {
                                        if (indVar.IMPO_INDENN_INDTE != null)
                                        {
                                            decimal Imp;
                                            decimal.TryParse(indVar.IMPO_INDENN_INDTE, out Imp);
                                            Imp = Imp / 100;
                                            string labelInd = indVar.COD_INDENN_INDTE + " " + GetDescIndennita(indVar.COD_INDENN_INDTE)
                                                           + " €" + Imp.ToString().Replace(",",".");

                                            if (indVar.FLAG == "+") Acq.Add(labelInd);
                                            if (indVar.FLAG == "-") Perse.Add(labelInd);
                                            if (indVar.FLAG == "D") Delta.Add(labelInd);
                                        }
                                    }
                                    Variaz.INDENNITA_ACQUISITE = String.Join(",", Acq.ToArray());
                                    Variaz.INDENNITA_DELTA = String.Join(",", Delta.ToArray());
                                    Variaz.INDENNITA_PERSE = String.Join(",", Perse.ToArray());
                                }
                            }
                        }
                    }
                }

                db.SaveChanges();
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true }
                };
            }
            else
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore="Impossibile recuperare i valori per la categoria di arrivo" }
                };
            }

            
        }
        [HttpGet]
        public ActionResult GetAnteprimaPratica(int idPersona, string decorrenza = "")
        {
            IncentiviEntities db = new IncentiviEntities();
            XR_PRV_DIPENDENTI dip = new XR_PRV_DIPENDENTI();
            dip.ID_PERSONA = idPersona;
            //dip.ANAGPERS = db.ANAGPERS.FirstOrDefault(x => x.ID_PERSONA==idPersona);
            dip.SINTESI1 = db.SINTESI1.FirstOrDefault(x => x.ID_PERSONA == idPersona);
            dip.MATRICOLA = dip.SINTESI1.COD_MATLIBROMAT;

            if (!String.IsNullOrWhiteSpace(decorrenza))
                dip.DECORRENZA = DateTime.ParseExact(decorrenza, "dd/MM/yyyy", null);

            //HRDWData.EstraiSede(db, dip, true);
            dip.DES_SEDE = dip.SINTESI1.DES_SEDE;
            CezanneHelper.GetCampiFirma(out CampiFirma campiFirma);
            HRDWData.RecuperoAssenze(db, dip, true, campiFirma);
            HRDWData.RecuperoRal(db, dip, true, campiFirma);
            HRDWData.RecuperoProvvedimenti(db, dip, true, campiFirma);
            HRDWData.RecuperoVariabili(db, dip, true, campiFirma);
            HRDWData.RecuperoReperibilita(db, dip, true, campiFirma);
            HRDWData.CalcoloCosti(db, dip, true, campiFirma); //Anteprima pratica

            string currentServ = dip.SINTESI1.XR_SERVIZIO.OrderByDescending(x => x.DTA_FINE).FirstOrDefault().COD_SERVIZIO.Substring(0, 2);

            var dir = db.XR_PRV_DIREZIONE.FirstOrDefault(x => x.CODICE == currentServ);
            if (dir != null)
            {
                dip.XR_PRV_DIREZIONE = dir;
            }

            Pratica pratica = new Pratica();
            pratica.IsPreview = true;
            pratica.Dipendente = dip;

            MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client cl = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
            cl.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
            var resp = cl.GetProvvedimentiCause(CommonHelper.GetCurrentUserMatricola(), pratica.Dipendente.MATRICOLA);
            if (resp.Esito)
            {
                pratica.CauseAperte = resp.Caperte;
                pratica.CauseChiuse = resp.Cchiuse;
                pratica.ProvvedimentiAperti = resp.PDaperti;
                pratica.ProvvedimentiChiusi = resp.PDchiusi;

                pratica.Cause = resp.Cause;
                pratica.Provvedimenti = resp.Provvedimenti;
            }

            pratica.VertenzeSind = new List<XR_PRV_DIPENDENTI_VERTENZE>();
            pratica.VertenzeSind.AddRange(db.XR_PRV_DIPENDENTI_VERTENZE.Where(x => x.MATRICOLA == dip.MATRICOLA));
            pratica.CauseDB = new List<XR_PRV_DIPENDENTI_CAUSE>();
            pratica.CauseDB.AddRange(db.XR_PRV_DIPENDENTI_CAUSE.Where(x => x.MATRICOLA == dip.MATRICOLA));

            GetStragiudiziali(pratica);

            pratica.BoxAbilitati = new List<XR_PRV_BOX>();
            pratica.BoxAbilitati.AddRange(db.XR_PRV_BOX.Where(x => x.SEZIONE == "DETT_DX").OrderBy(x => x.ORDINE));

            var prvProm = PoliticheRetributiveHelper.GetDipProv(pratica.Dipendente, 1);
            pratica.CanShowData = true;
            if (prvProm != null && prvProm.CAT_PREVISTA != null && prvProm.CAT_PREVISTA != "$$$")
                pratica.CanShowData = PoliticheRetributiveHelper.CanSeeProvvData(db, prvProm.CAT_PREVISTA);

            return View("~/Views/PoliticheRetributive/subpartial/DettaglioAnagrafica.cshtml", pratica);
        }

        public Pratica GetPratica(int idDip)
        {
            int idPersona = CommonHelper.GetCurrentIdPersona();

            IncentiviEntities db = new IncentiviEntities();
            XR_PRV_DIPENDENTI dip = db.XR_PRV_DIPENDENTI
                                        .Include("XR_PRV_OPERSTATI")
                                        .Include("XR_PRV_OPERSTATI.SINTESI1")
                                        .Include("XR_PRV_CAMPAGNA")
                                        .Include("XR_PRV_DIPENDENTI_VARIAZIONI")
                                        .Include("XR_PRV_DIPENDENTI_PROV")
                                        .Include("XR_PRV_DIPENDENTI_VAR")
                                        .Include("XR_PRV_DIPENDENTI_RAL")
                                        .Include("XR_PRV_DIPENDENTI_NOTE")
                                        .Include("XR_PRV_DIPENDENTI_ASSENZE")
                                        .Include("XR_PRV_DIREZIONE")
                                        .Include("SINTESI1")
                                        .Include("SINTESI1.QUALIFICA")
                                        .Include("SINTESI1.QUALIFICA.TB_QUALSTD")
                                        .FirstOrDefault(x => x.ID_DIPENDENTE == idDip);

            Pratica pratica = DettaglioPratica(db, dip);
            return pratica;
        }
        [HttpGet]
        public ActionResult GetDettaglioPratica(int idDip)
        {
            int idPersona = CommonHelper.GetCurrentIdPersona();

            IncentiviEntities db = new IncentiviEntities();
            XR_PRV_DIPENDENTI dip = db.XR_PRV_DIPENDENTI
                                        .Include("XR_PRV_OPERSTATI")
                                        .Include("XR_PRV_OPERSTATI.SINTESI1")
                                        .Include("XR_PRV_CAMPAGNA")
                                        .Include("XR_PRV_DIPENDENTI_VARIAZIONI")
                                        .Include("XR_PRV_DIPENDENTI_PROV")
                                        .Include("XR_PRV_DIPENDENTI_VAR")
                                        .Include("XR_PRV_DIPENDENTI_RAL")
                                        .Include("XR_PRV_DIPENDENTI_NOTE")
                                        .Include("XR_PRV_DIPENDENTI_ASSENZE")
                                        .Include("XR_PRV_DIREZIONE")
                                        .Include("SINTESI1")
                                        .Include("SINTESI1.QUALIFICA")
                                        .Include("SINTESI1.QUALIFICA.TB_QUALSTD")
                                        .FirstOrDefault(x => x.ID_DIPENDENTE == idDip);

            Pratica pratica = DettaglioPratica(db, dip);
            pratica = GetDatiConsiderandoSimulazioni(pratica);

            return View("~/Views/PoliticheRetributive/subpartial/DettaglioAnagrafica.cshtml", pratica);
        }

        public ActionResult AggiornaSim(string cat, string matricola)
        {
            var db = new IncentiviEntities();
            cat = cat.Split('-')[0].Trim();
            var ListaSim = db.XR_PRV_DIPENDENTI_SIMULAZIONI.Where(x => x.MATRICOLA == matricola && x.CAT_ARRIVO == cat).ToList();
            if (ListaSim.Any()  )
            {
                foreach (var sim in ListaSim)
                {
                    sim.DTA_ACQUISIZIONE = null;
                    sim.DTA_ESECUZIONE = null;
                    sim.IND_ESITO_ESECUZIONE = null;
                    sim.IND_ESITO_ACQUISIZIONE = null;
                    sim.NOT_ERROR = null;

                    sim.JSON_SIMULAZIONE = null;
                }
              
                db.SaveChanges();
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true }
                };
            }
            else return   new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = false, errore = "Impossibile recuperare la simulazione" }
            };
        }
        private Pratica GetDatiConsiderandoSimulazioni(Pratica pratica) //M-POLRETR appendice model simulazioni
        {
            var db = new IncentiviEntities();
            DatiConsiderandoSimulazioniModel D = new DatiConsiderandoSimulazioniModel();


            D.EsisteSimulazioneIncompleta =   db.XR_PRV_DIPENDENTI_SIMULAZIONI
                .Where(x => x.ID_DIPENDENTE == pratica.Dipendente.ID_DIPENDENTE && x.DTA_ESECUZIONE == null).FirstOrDefault();

            D.EsisteSimulazioneCompletata = db.XR_PRV_DIPENDENTI_SIMULAZIONI.Where(x => x.ID_DIPENDENTE == pratica.Dipendente.ID_DIPENDENTE &&
                                        x.DTA_ESECUZIONE != null).ToList();

            List<string> catArrivoDisponibileInTabSimulazioni = new List<string> ();
            if (D.EsisteSimulazioneCompletata.Any())
            {
                catArrivoDisponibileInTabSimulazioni = D.EsisteSimulazioneCompletata.Select (x=>x.CAT_ARRIVO).ToList();
            }
            D.catArrivoDisponibileInTabSimulazioni = catArrivoDisponibileInTabSimulazioni;

            D.catArrivoDisponibileInTabVariazioni = db.XR_PRV_DIPENDENTI_VARIAZIONI.Where (x=>x.ID_DIPENDENTE==pratica.Dipendente.ID_DIPENDENTE
                                                          && x.CAT_PREVISTA!=null).Select(x=>x.CAT_PREVISTA).FirstOrDefault();

            D.IsFromIndex = pratica.CatArrivo == null;
            D.IsFromCombo = pratica.CatArrivo != null;

            
            if (D.IsFromIndex)
                D.PrendiDatiDa = "V";

            if (D.IsFromCombo)
            {
                D.AbilitaButtonRichiediSimulazione = D.EsisteSimulazioneIncompleta == null && D.IsFromCombo &&
                                                        D.catArrivoDisponibileInTabVariazioni != pratica.CatArrivo
                                                        && (!D.catArrivoDisponibileInTabSimulazioni.Contains(pratica.CatArrivo));

                if (D.catArrivoDisponibileInTabVariazioni == pratica.CatArrivo || D.EsisteSimulazioneIncompleta !=null)
                    D.PrendiDatiDa = "V";
                else
                    D.PrendiDatiDa = "S";

                if (D.PrendiDatiDa == "S" && D.catArrivoDisponibileInTabSimulazioni.Any())
                {
                    //D.AbilitaButtonSalvaSimulazioneSuVariazioni = true;
                    D.SimulazioneCorrenteVisualizzata = D.EsisteSimulazioneCompletata.Where (x=>x.CAT_ARRIVO==pratica.CatArrivo).FirstOrDefault();
                    if (D.SimulazioneCorrenteVisualizzata != null && D.SimulazioneCorrenteVisualizzata.CAT_ARRIVO != D.catArrivoDisponibileInTabVariazioni)
                    {
                        D.AbilitaButtonSalvaSimulazioneSuVariazioni = true;
                    }
                }
            }

            if (D.catArrivoDisponibileInTabSimulazioni.Any())
            {
                if (D.PrendiDatiDa == "V")// && D.catArrivoDisponibileInTabSimulazioni.Any(z => z != D.catArrivoDisponibileInTabVariazioni))
                {
                    var Darr = D.catArrivoDisponibileInTabSimulazioni//.Where(x => x != D.catArrivoDisponibileInTabVariazioni)
                        .Select(x=>x).ToList();

                    List<string> CatDescrittive = new List<string>();
                    foreach (var item in Darr)
                    {
                        string desc =db.QUALIFICA.Where(x => x.COD_QUALIFICA == item).Select(x => x.DES_QUALIFICA).FirstOrDefault();
                        if (desc != null)
                            CatDescrittive.Add(desc);
                        else
                            CatDescrittive.Add(item);
                    }
                    D.ShowNotaSimulazioniDisponibili = String.Join(",", CatDescrittive.ToArray()  );
                }
            }
            D.TabVariazioniSovrascrittaDaSimulazione = db.XR_PRV_DIPENDENTI_VARIAZIONI
                                                        .Any(x =>x.ID_DIPENDENTE==pratica.Dipendente.ID_DIPENDENTE
                                                              && x.ID_SIMULAZIONE_PROVENIENZA != null);

            pratica.DatiConsiderandoSimulazioni = D;
            return pratica;
        }

        private Pratica DettaglioPratica(IncentiviEntities db, XR_PRV_DIPENDENTI dip)
        {
            Pratica pratica = new Pratica();
            pratica.IsAbilAmm = PoliticheRetributiveHelper.EnabledToAmm();
            pratica.EnableGest = PoliticheRetributiveHelper.EnableGest(PolRetrChiaveEnum.Richieste) && !pratica.IsAbilAmm;
            ViewData["EnableGestBudget"] = PoliticheRetributiveHelper.EnableGestCampagna(dip.XR_PRV_CAMPAGNA.LV_ABIL);

            pratica.Dipendente = dip;

            if (!pratica.IsAbilAmm)
            {
                MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client cl = new MyRaiServiceInterface.MyRaiServiceReference1.MyRaiService1Client();
                cl.ClientCredentials.Windows.ClientCredential = new System.Net.NetworkCredential(CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
                var resp = cl.GetProvvedimentiCause(CommonHelper.GetCurrentUserMatricola(), pratica.Dipendente.MATRICOLA);
                if (resp.Esito)
                {
                    pratica.CauseAperte = resp.Caperte;
                    pratica.CauseChiuse = resp.Cchiuse;
                    pratica.ProvvedimentiAperti = resp.PDaperti;
                    pratica.ProvvedimentiChiusi = resp.PDchiusi;

                    pratica.Cause = resp.Cause;
                    pratica.Provvedimenti = resp.Provvedimenti;
                }

                pratica.VertenzeSind = new List<XR_PRV_DIPENDENTI_VERTENZE>();
                pratica.VertenzeSind.AddRange(db.XR_PRV_DIPENDENTI_VERTENZE.Where(x => x.MATRICOLA == dip.MATRICOLA));
                pratica.CauseDB = new List<XR_PRV_DIPENDENTI_CAUSE>();
                pratica.CauseDB.AddRange(db.XR_PRV_DIPENDENTI_CAUSE.Where(x => x.MATRICOLA == dip.MATRICOLA));
                GetStragiudiziali(pratica);
            }

            pratica.LogOperazioni = new List<XR_PRV_LOG>();
            var richiesta = pratica.Dipendente.XR_PRV_OPERSTATI.FirstOrDefault(x => x.ID_STATO == (int)ProvvStatoEnum.Richiesta);
            if (richiesta != null)
            {
                pratica.LogOperazioni.Add(new XR_PRV_LOG()
                {
                    ID_LOG = 0,
                    ID_DIPENDENTE = pratica.Dipendente.ID_DIPENDENTE,
                    ID_PERSONA = richiesta.ID_PERSONA,
                    MESSAGGIO = "Creazione richiesta",
                    TMS_TIMESTAMP = richiesta.TMS_TIMESTAMP,
                    SINTESI1 = richiesta.SINTESI1
                });
            }

            pratica.LogOperazioni.AddRange(db.XR_PRV_LOG.Include("SINTESI1")
                .Where(x => x.ID_DIPENDENTE == pratica.Dipendente.ID_DIPENDENTE && x.MESSAGGIO.ToLower() != "richiesto aggiornamento costi"));

            pratica.PossibiliPassaggi = new List<XR_PRV_PROV_PASSCAT>();
            try
            {
                List<XR_PRV_PROV_PASSCAT> templist = db.XR_PRV_PROV_PASSCAT.Where(x => x.CAT_PARTENZA == pratica.Dipendente.SINTESI1.COD_QUALIFICA).ToList();
                pratica.PossibiliPassaggi.AddRange(templist);
            }
            catch (Exception ex)
            {
                int a = 5 + 5;
               
            }

            pratica.BoxAbilitati = new List<XR_PRV_BOX>();
            pratica.BoxAbilitati.AddRange(db.XR_PRV_BOX.Where(x => x.SEZIONE == "DETT_DX").OrderBy(x => x.ORDINE));

            pratica.Simulazioni = new List<XR_PRV_DIPENDENTI_SIMULAZIONI>();
           // pratica.Simulazioni.AddRange(db.XR_PRV_DIPENDENTI_SIMULAZIONI.Where(x => x.ID_DIPENDENTE == dip.ID_DIPENDENTE && (x.DTA_ESECUZIONE == null || x.DTA_ACQUISIZIONE == null)));
            pratica.EnableGest &= !pratica.Simulazioni.Any();

            var prvProm = PoliticheRetributiveHelper.GetDipProv(pratica.Dipendente, 1);
            pratica.CanShowData = true;
            if (prvProm != null && prvProm.CAT_PREVISTA != null && prvProm.CAT_PREVISTA != "$$$")
                pratica.CanShowData = PoliticheRetributiveHelper.CanSeeProvvData(db, prvProm.CAT_PREVISTA);
            return pratica;
        }

        public static List<SelectListItem> GetPossibiliCategorie(string catCorrente, string catPrevista, bool? customProv)
        {
            var db = new IncentiviEntities();
            var possibiliPassaggi = db.XR_PRV_PROV_PASSCAT
                                        .Join(db.QUALIFICA, x => x.CAT_ARRIVO, y => y.COD_QUALIFICA, (x, y) => new { pc = x, qual = y })
                                        .Where(x => x.pc.CAT_PARTENZA == catCorrente)
                                        .ToList();


            var tmp = possibiliPassaggi.Select(x =>
                    new SelectListItem
                    {
                        Value = x.pc.CAT_ARRIVO,
                        Text = x.pc.CAT_ARRIVO + " - " + CezanneHelper.GetDes(x.pc.CAT_ARRIVO, x.qual.DES_QUALIFICA.TitleCase()),
                        Selected = (catPrevista ?? "") == x.pc.CAT_ARRIVO
                    })
                    .ToList();

            return tmp;

            //if (customProv.GetValueOrDefault())
            //    return AnagraficaManager.GetCategorieHRDW("", "", true, null, catPrevista);
            //else
            //{
            //    return AnagraficaManager.GetCategorieHRDW("", catPrevista, false, null, catPrevista);
            //}
        }

        private static void GetStragiudiziali(Pratica pratica)
        {
            MyRaiServiceInterface.it.rai.servizi.hrgb.Service s = new MyRaiServiceInterface.it.rai.servizi.hrgb.Service();
            s.Credentials = CommonHelper.GetUtenteServizioCredentials();
            var respHrgb = s.Get_HrExtra_FascicoliMatricola(pratica.Dipendente.MATRICOLA, "");
            if (respHrgb != null && respHrgb.DT_HrExtra_FascicoliMatricola != null)
            {
                pratica.Stragiudiziali = new List<Stragiudiziale>();
                foreach (DataRow item in respHrgb.DT_HrExtra_FascicoliMatricola.Rows)
                {
                    Stragiudiziale stra = new Stragiudiziale
                    {
                        Matricola = item.Field<string>("Matricola"),
                        Soggetto = item.Field<string>("Soggetto"),
                        NumeroDossier = item.Field<string>("NumeroDossier"),
                        DataCreazione = item.Field<string>("DataCreazione"),
                        Oggetto = item.Field<string>("Oggetto"),
                        DescrizioneStato = item.Field<string>("DescrizioneStato"),
                        DataStato = item.Field<string>("DataStato"),
                        Note = item.Field<string>("Note")
                    };
                    pratica.Stragiudiziali.Add(stra);
                }
            }
        }

        [HttpPost]
        public ActionResult UpdatePratica(int idDip, int idProv, string dataDec, XR_PRV_DIPENDENTI_VARIAZIONI[] customProv, 
            string catRich, int piano, bool isGestExt, int idTemplate, string codMansione, string statoLettera, string livRich)
        {
            IncentiviEntities db = new IncentiviEntities();
            var dipendente = db.XR_PRV_DIPENDENTI.FirstOrDefault(x => x.ID_DIPENDENTE == idDip);
            if (dipendente == null)
            {
                return Content("Dipendente non trovato");
            }
            else
            {
                CezanneHelper.GetCampiFirma(out var campiFirma);
                int oldProv = dipendente.ID_PROV_EFFETTIVO.GetValueOrDefault();
                DateTime? oldDec = dipendente.DECORRENZA;
                string oldCatRich = dipendente.CAT_RICHIESTA;
                bool? oldGestExt = dipendente.IND_PRATICA_EXT;
                int? oldIdTemplate = dipendente.ID_TEMPLATE;
                string oldCodMansione = dipendente.COD_MANSIONE;
                string oldLivRich = dipendente.LIV_RICHIESTA;

                dipendente.ID_PROV_EFFETTIVO = idProv;
                PoliticheRetributiveHelper.SetProvEffettivo(db, dipendente, idProv);
                dipendente.CAT_RICHIESTA = catRich;
                dipendente.IND_PRATICA_EXT = isGestExt;
                if (idTemplate > 0)
                    dipendente.ID_TEMPLATE = idTemplate;
                else
                    dipendente.ID_TEMPLATE = null;
                dipendente.COD_MANSIONE = codMansione;
                dipendente.LIV_RICHIESTA = livRich;

                bool pianoIsChanged = false;
                string msgPiano = "";
                if (dipendente.ID_CAMPAGNA != piano)
                {
                    XR_PRV_CAMPAGNA newPiano = db.XR_PRV_CAMPAGNA.FirstOrDefault(x => x.ID_CAMPAGNA == piano);
                    if (newPiano.LV_ABIL != dipendente.XR_PRV_CAMPAGNA.LV_ABIL)
                    {
                        var newDir = db.XR_PRV_DIREZIONE.FirstOrDefault(x => x.ID_DIREZIONE != dipendente.ID_DIREZIONE && x.CODICE == dipendente.XR_PRV_DIREZIONE.CODICE);
                        dipendente.ID_DIREZIONE = newDir.ID_DIREZIONE;
                    }
                    dipendente.ID_CAMPAGNA = piano;

                    msgPiano = "Cambiato piano da '" + dipendente.XR_PRV_CAMPAGNA.NOME + "' a '" + newPiano.NOME + "'";

                    pianoIsChanged = true;
                }

                var statoConsegna = dipendente.XR_PRV_OPERSTATI.FirstOrDefault(x => x.ID_STATO == (int)ProvvStatoEnum.Consegnato);
                if (String.IsNullOrWhiteSpace(statoLettera) || statoLettera == "0")
                {
                    dipendente.STATO_LETTERA = null;
                    if (statoConsegna != null)
                        db.XR_PRV_OPERSTATI.Remove(statoConsegna);
                }
                else
                {
                    dipendente.STATO_LETTERA = Convert.ToInt32(statoLettera);
                    if (statoConsegna == null)
                    {
                        statoConsegna = new XR_PRV_OPERSTATI();
                        statoConsegna.ID_OPER = db.XR_PRV_OPERSTATI.GeneraPrimaryKey();
                        statoConsegna.ID_DIPENDENTE = idDip;
                        statoConsegna.ID_PERSONA = CommonHelper.GetCurrentIdPersona();
                        statoConsegna.ID_STATO = (int)ProvvStatoEnum.Consegnato;
                        statoConsegna.DATA = DateTime.Now;
                        statoConsegna.COD_USER = campiFirma.CodUser;
                        statoConsegna.COD_TERMID = campiFirma.CodTermid;
                        statoConsegna.TMS_TIMESTAMP = campiFirma.Timestamp;
                        db.XR_PRV_OPERSTATI.Add(statoConsegna);
                    }
                }

                bool decIsChanged = false;
                List<string> msgCustomProv = new List<string>();

                if (!String.IsNullOrWhiteSpace(dataDec))
                {
                    DateTime tmp = DateTime.ParseExact(dataDec, "dd/MM/yyyy", null);
                    decIsChanged = tmp != dipendente.DECORRENZA;

                    dipendente.DECORRENZA = tmp;

                    if (decIsChanged && customProv == null)
                    {
                        //string elencoMatricole = " SELECT MATRICOLA FROM XR_PRV_DIPENDENTI WHERE ID_DIPENDENTE = " + idDip;
                        //HRDWData.CalcoloCostiMassivo(db, campiFirma, elencoMatricole, x => x.ID_DIPENDENTE == idDip, true, true, true, true, tmp, false);
                        HRDWData.CalcoloCosti(db, dipendente, false, campiFirma, false); //Update pratica
                    }
                }

                if (customProv != null)
                {
                    foreach (var custom in customProv.Where(x => x.ID_PROV != 5 && x.ID_PROV != 10))
                    {
                        string message = "";

                        var v = PoliticheRetributiveHelper.GetDipProv(dipendente, custom.ID_PROV);
                        string messagePrefix = v.XR_PRV_PROV.NOME;

                        if (v.DIFF_RAL != custom.DIFF_RAL)
                            message += messagePrefix + " - Differenza RAL da " + v.DIFF_RAL.ToString("N2") + " a " + custom.DIFF_RAL.ToString("N2") + "\r\n";
                        if (v.COSTO_ANNUO != custom.COSTO_ANNUO)
                            message += messagePrefix + " - Costo annuo da " + v.COSTO_ANNUO.ToString("N2") + " a " + custom.COSTO_ANNUO.ToString("N2") + "\r\n";
                        if (v.COSTO_PERIODO != custom.COSTO_PERIODO)
                            message += messagePrefix + " - Costo periodo da " + v.COSTO_PERIODO.ToString("N2") + " a " + custom.COSTO_PERIODO.ToString("N2") + "\r\n";

                        v.DIFF_RAL = custom.DIFF_RAL;
                        v.COSTO_ANNUO = custom.COSTO_ANNUO;
                        v.COSTO_PERIODO = custom.COSTO_PERIODO;

                        if (!String.IsNullOrWhiteSpace(message))
                            msgCustomProv.Add(message);
                    }
                }

                try
                {
                    DBHelper.Save(db, "PoliticheRetributive - ");

                    //log
                    if (oldProv != idProv)
                        PoliticheRetributiveManager.LogChangeProvv(db, dipendente.ID_DIPENDENTE, oldProv, idProv);
                    if (decIsChanged)
                        PoliticheRetributiveManager.LogChangeEffectiveDate(db, dipendente.ID_DIPENDENTE, oldDec, dipendente.DECORRENZA);
                    if (msgCustomProv.Count() > 0)
                        PoliticheRetributiveManager.WriteLog(db, dipendente.ID_DIPENDENTE, String.Join("\r\n", msgCustomProv));
                    if (pianoIsChanged)
                        PoliticheRetributiveManager.WriteLog(db, dipendente.ID_DIPENDENTE, msgPiano);
                    if (oldGestExt.GetValueOrDefault() != isGestExt)
                        PoliticheRetributiveManager.WriteLog(db, dipendente.ID_DIPENDENTE, isGestExt ? "Impostata gestione esterna" : "Rimossa gestione esterna");
                    if (oldIdTemplate.GetValueOrDefault() != idTemplate)
                        PoliticheRetributiveManager.WriteLog(db, dipendente.ID_DIPENDENTE, String.Format("Modificato template da {0} a {1}", oldIdTemplate.GetValueOrDefault(), idTemplate));
                    if (oldCodMansione != codMansione)
                        PoliticheRetributiveManager.WriteLog(db, dipendente.ID_DIPENDENTE, String.Format("Modificata mansione da {0} a {1}", oldCodMansione, codMansione));
                    if (oldCatRich != catRich || oldLivRich != livRich)
                        PoliticheRetributiveManager.WriteLog(db, dipendente.ID_DIPENDENTE, String.Format("Modificata categoria da {0}/{1} a {2}/{3}", oldCatRich, oldLivRich, catRich, livRich));

                    if (decIsChanged)
                        return Content("update");
                    return Content("OK");
                }
                catch (Exception ex)
                {
                    return Content("Errore durante il salvataggio dei dati");
                }

            }
        }
        public ActionResult ConsolidaPratica(int idDip)
        {
            SalvaStato(idDip, (int)ProvvStatoEnum.Convalidato);

            return Content("OK");
        }
        public ActionResult RimuoviConvalida(int idOper)
        {
            try
            {
                InvalidaStatoInternal(idOper);
                return Content("OK");
            }
            catch (Exception)
            {
                return Content("Impossibile rimuovere la convalida");
            }

        }
        public ActionResult ToggleCustomProvv(int idDip, bool enableCustom)
        {
            string codUser = "";
            string termId = "";
            DateTime timeStamp;

            using (IncentiviEntities db = new IncentiviEntities())
            {
                XR_PRV_DIPENDENTI dip = db.XR_PRV_DIPENDENTI.FirstOrDefault(x => x.ID_DIPENDENTE == idDip);
                int oldProvv = dip.ID_PROV_EFFETTIVO.GetValueOrDefault();

                if (enableCustom)
                {
                    dip.CUSTOM_PROV = true;
                    dip.ID_PROV_EFFETTIVO = db.XR_PRV_PROV.FirstOrDefault(x => x.BASE_PROV.Value == dip.ID_PROV_EFFETTIVO).ID_PROV;
                    CezanneHelper.GetCampiFirma(out codUser, out termId, out timeStamp);
                    dip.COD_USER = codUser;
                    dip.COD_TERMID = termId;
                    dip.TMS_TIMESTAMP = timeStamp;
                    DBHelper.Save(db, "PoliticheRetributive - ");

                    var list = dip.XR_PRV_DIPENDENTI_VARIAZIONI.ToList();
                    foreach (var item in list)
                    {
                        XR_PRV_DIPENDENTI_VARIAZIONI provv = new XR_PRV_DIPENDENTI_VARIAZIONI
                        {
                            ID_DIP_COSTO = db.XR_PRV_DIPENDENTI_VARIAZIONI.GeneraPrimaryKey(),
                            ID_DIPENDENTE = dip.ID_DIPENDENTE,
                            ID_PROV = db.XR_PRV_PROV.FirstOrDefault(x => x.BASE_PROV.Value == item.ID_PROV).ID_PROV,
                            DIFF_RAL = item.DIFF_RAL,
                            COSTO_ANNUO = item.COSTO_ANNUO,
                            COSTO_PERIODO = item.COSTO_PERIODO,
                            LIV_ATTUALE = item.LIV_ATTUALE,
                            LIV_PREVISTO = item.LIV_PREVISTO,
                            CAT_PREVISTA = item.CAT_PREVISTA,
                            COSTO_REC_STR = item.COSTO_REC_STR,
                            INDENNITA_ACQUISITE = item.INDENNITA_ACQUISITE,
                            INDENNITA_DELTA = item.INDENNITA_DELTA,
                            INDENNITA_PERSE = item.INDENNITA_PERSE
                        };
                        CezanneHelper.GetCampiFirma(out codUser, out termId, out timeStamp);
                        provv.COD_USER = codUser;
                        provv.COD_TERMID = termId;
                        provv.TMS_TIMESTAMP = timeStamp;
                        db.XR_PRV_DIPENDENTI_VARIAZIONI.Add(provv);
                    }
                }
                else
                {
                    dip.CUSTOM_PROV = false;
                    dip.ID_PROV_EFFETTIVO = db.XR_PRV_PROV.FirstOrDefault(x => x.ID_PROV == dip.ID_PROV_EFFETTIVO).BASE_PROV.Value;
                    db.XR_PRV_DIPENDENTI_VARIAZIONI.RemoveWhere(x => x.ID_DIPENDENTE == idDip && x.XR_PRV_PROV.CUSTOM != null && x.XR_PRV_PROV.CUSTOM.Value);
                }

                PoliticheRetributiveHelper.SetProvEffettivo(db, dip, dip.ID_PROV_EFFETTIVO);
                DBHelper.Save(db, "PoliticheRetributive - ");

                PoliticheRetributiveManager.LogGestioneManuale(db, idDip, dip.CUSTOM_PROV.GetValueOrDefault());
                PoliticheRetributiveManager.LogChangeProvv(db, idDip, oldProvv, dip.ID_PROV_EFFETTIVO.GetValueOrDefault());
            }

            return GetDettaglioPratica(idDip);
        }

        public ActionResult ConsegnaLetteraPratica(int idDip)
        {
            SalvaStato(idDip, (int)ProvvStatoEnum.Consegnato);

            return Content("OK");
        }
        #endregion

        #region GestioneCampagna

        public ActionResult OpenCampagna()
        {
            List<myRaiCommonModel.Gestionale.HRDW.HRDWDirezione> organico = null;
            myRaiCommonModel.Gestionale.HRDW.HRDWData.getOrganico(null, out organico);

            Expression<Func<XR_PRV_AREA, bool>> funcFilterArea = PoliticheRetributiveHelper.FuncFilterArea();

            Budget budget = new Budget();
            budget.abilDisponibili = new List<Tuple<string, string>>();
            if (PoliticheRetributiveHelper.EnableGest(PolRetrChiaveEnum.BudgetRS))
                budget.abilDisponibili.Add(new Tuple<string, string>("BDGRS", "Risorse Chiave"));
            if (PoliticheRetributiveHelper.EnableGest(PolRetrChiaveEnum.BudgetQIO))
                budget.abilDisponibili.Add(new Tuple<string, string>("BDGQIO", "Non Risorse Chiave"));

            //Serve a gestire casi come Bolzano (64), che deve rientrare sotto il CSR (24)
            PolRetrParam param = HrisHelper.GetParametroJson<PolRetrParam>(HrisParam.PoliticheParametri);
            List<PolRetrOvverideDir> ovverideRule = param != null && param.OvverideDir != null ? param.OvverideDir : new List<PolRetrOvverideDir>();

            using (IncentiviEntities db = new IncentiviEntities())
            {
                foreach (var dbArea in db.XR_PRV_AREA.Where(funcFilterArea))
                {
                    BudgetArea area = new BudgetArea()
                    {
                        Id = dbArea.ID_AREA,
                        Nome = dbArea.NOME,
                        Direzioni = dbArea.XR_PRV_DIREZIONE.Where(x => x.VALID_DTA_END == null && !ovverideRule.Any(y => y.DirOrig == x.CODICE)).OrderBy(x => x.ORDINE).Select(y => new BudgetDirezione()
                        {
                            Id = y.ID_DIREZIONE,
                            Codice = y.CODICE,
                            Nome = y.NOME,
                            Organico = organico.Any(a => a.Codice == y.CODICE) ? organico.FirstOrDefault(a => a.Codice == y.CODICE).Organico : 0,
                            OrganicoM = organico.Any(a => a.Codice == y.CODICE) ? organico.FirstOrDefault(a => a.Codice == y.CODICE).OrganicoMaschile : 0,
                            OrganicoF = organico.Any(a => a.Codice == y.CODICE) ? organico.FirstOrDefault(a => a.Codice == y.CODICE).OrganicoFemminile : 0,
                            OrganicoGiaProvv = organico.Any(a => a.Codice == y.CODICE) ? y.XR_PRV_DIREZIONE_PROV.Where(z => z.ANNO == DateTime.Today.Year).Sum(w => w.NM_PROV) : 0
                        }).ToList()
                    };
                    budget.Aree.Add(area);
                }
            }
            budget.EnableGest = true;

            return View("~/Views/PoliticheRetributive/subpartial/GestioneBudget.cshtml", budget);
        }

        public ActionResult ModificaCampagna(int idCampagna)
        {
            Budget budget = new Budget();
            IncentiviEntities db = new IncentiviEntities();

            XR_PRV_CAMPAGNA campagna = db.XR_PRV_CAMPAGNA.FirstOrDefault(x => x.ID_CAMPAGNA == idCampagna);
            if (campagna != null)
            {
                budget.EnableGest = PoliticheRetributiveHelper.EnableGestCampagna(campagna.LV_ABIL);

                budget.IdCampagna = campagna.ID_CAMPAGNA;
                budget.Nome = campagna.NOME;
                budget.DataInizio = campagna.DTA_INIZIO.Value;
                budget.DataFine = campagna.DTA_FINE;
                budget.Riserva = campagna.DEC_RISERVA;
                budget.DateDecorrenza.AddRange(campagna.XR_PRV_CAMPAGNA_DECORRENZA.OrderBy(x => x.DT_DECORRENZA).ToList().Select(x => x.DT_DECORRENZA.ToString("dd/MM/yyyy")));
                foreach (var dbArea in campagna.XR_PRV_CAMPAGNA_BUDGET)
                {
                    BudgetArea area = new BudgetArea()
                    {
                        Id = dbArea.ID_AREA,
                        Nome = dbArea.XR_PRV_AREA.NOME,
                        Importo = dbArea.BUDGET,
                        ImportoPeriodo = dbArea.BUDGET_PERIODO.Value,
                        Direzioni = campagna.XR_PRV_CAMPAGNA_DIREZIONE.Where(x => x.XR_PRV_DIREZIONE.ID_AREA == dbArea.ID_AREA).OrderBy(x => x.XR_PRV_DIREZIONE.ORDINE).Select(y => new BudgetDirezione()
                        {
                            Id = y.ID_DIREZIONE,
                            Codice = y.XR_PRV_DIREZIONE.CODICE,
                            Nome = y.XR_PRV_DIREZIONE.NOME,
                            Organico = y.ORGANICO,
                            OrganicoAD = y.ORGANICO_AD,
                            Budget = y.BUDGET,
                            BudgetPeriodo = y.BUDGET_PERIODO.Value
                        }).ToList()
                    };
                    budget.Aree.Add(area);
                }
            }


            return View("~/Views/PoliticheRetributive/subpartial/GestioneBudget.cshtml", budget);
        }

        public ActionResult EliminaCampagna(int idCampagna)
        {
            string result = "";

            using (IncentiviEntities db = new IncentiviEntities())
            {
                XR_PRV_CAMPAGNA campagna = db.XR_PRV_CAMPAGNA.FirstOrDefault(x => x.ID_CAMPAGNA == idCampagna);
                if (campagna == null)
                {
                    result = "Piano non trovato";
                }
                else if (campagna.XR_PRV_DIPENDENTI.Any())
                {
                    result = "Sono stati trovate delle pratiche associate a questo piano";
                }
                else
                {
                    db.XR_PRV_CAMPAGNA_DIREZIONE.RemoveWhere(x => x.ID_CAMPAGNA == idCampagna);
                    db.XR_PRV_CAMPAGNA_DECORRENZA.RemoveWhere(x => x.ID_CAMPAGNA == idCampagna);
                    db.XR_PRV_CAMPAGNA_BUDGET.RemoveWhere(x => x.ID_CAMPAGNA == idCampagna);
                    db.XR_PRV_CAMPAGNA.Remove(campagna);

                    try
                    {
                        DBHelper.Save(db, "PoliticheRetributive - ");
                        result = "OK";
                    }
                    catch (Exception)
                    {
                        result = "Errore durante la rimozione del piano";
                    }

                }
            }

            return Content(result);
        }

        [HttpPost]
        public ActionResult SaveCampagna(string nomeCampagna, string dataInizio, string dataFine, BudgetArea[] aree, string[] dateDecorrenza, string lvAbil, decimal riserva)
        {
            string result = "";
            string codUser = "";
            string codTermid = "";
            DateTime timestamp;

            IncentiviEntities db = new IncentiviEntities();

            XR_PRV_CAMPAGNA campagna = new XR_PRV_CAMPAGNA();
            campagna.ID_CAMPAGNA = db.XR_PRV_CAMPAGNA.GeneraPrimaryKey();
            campagna.NOME = nomeCampagna;
            campagna.DTA_INIZIO = DateTime.ParseExact(dataInizio, "dd/MM/yyyy", null);
            if (String.IsNullOrWhiteSpace(dataFine))
                campagna.DTA_FINE = null;
            else
                campagna.DTA_FINE = DateTime.ParseExact(dataFine, "dd/MM/yyyy", null);
            campagna.LV_ABIL = lvAbil;
            campagna.DEC_RISERVA = riserva;
            CezanneHelper.GetCampiFirma(out codUser, out codTermid, out timestamp);
            campagna.COD_USER = codUser;
            campagna.COD_TERMID = codTermid;
            campagna.TMS_TIMESTAMP = timestamp;
            db.XR_PRV_CAMPAGNA.Add(campagna);
            //DBHelper.Save(db, "PoliticheRetributive - ");

            if (dateDecorrenza != null && dateDecorrenza.Any())
            {
                foreach (var data in dateDecorrenza)
                {
                    XR_PRV_CAMPAGNA_DECORRENZA dec = new XR_PRV_CAMPAGNA_DECORRENZA();
                    dec.ID_DECORRENZA = db.XR_PRV_CAMPAGNA_DECORRENZA.GeneraPrimaryKey();
                    dec.ID_CAMPAGNA = campagna.ID_CAMPAGNA;
                    dec.DT_DECORRENZA = DateTime.ParseExact(data, "dd/MM/yyyy", null);
                    CezanneHelper.GetCampiFirma(out codUser, out codTermid, out timestamp);
                    dec.COD_USER = codUser;
                    dec.COD_TERMID = codTermid;
                    dec.TMS_TIMESTAMP = timestamp;
                    db.XR_PRV_CAMPAGNA_DECORRENZA.Add(dec);
                    //DBHelper.Save(db, "PoliticheRetributive - ");
                }
            }

            if (aree != null)
            {

                foreach (var area in aree)
                {
                    XR_PRV_CAMPAGNA_BUDGET budget = new XR_PRV_CAMPAGNA_BUDGET();
                    budget.ID_BUDGET = db.XR_PRV_CAMPAGNA_BUDGET.GeneraPrimaryKey();
                    budget.ID_CAMPAGNA = campagna.ID_CAMPAGNA;
                    budget.ID_AREA = area.Id;
                    budget.BUDGET = Decimal.Parse(area.ImportoStr, new NumberFormatInfo() { CurrencyDecimalSeparator = "." });
                    budget.BUDGET_PERIODO = Decimal.Parse(area.ImportoPeriodoStr, new NumberFormatInfo() { CurrencyDecimalSeparator = "." });
                    CezanneHelper.GetCampiFirma(out codUser, out codTermid, out timestamp);
                    budget.COD_USER = codUser;
                    budget.COD_TERMID = codTermid;
                    budget.TMS_TIMESTAMP = timestamp;
                    db.XR_PRV_CAMPAGNA_BUDGET.Add(budget);
                    //DBHelper.Save(db, "PoliticheRetributive - ");

                    foreach (var dir in area.Direzioni)
                    {
                        XR_PRV_CAMPAGNA_DIREZIONE direzione = new XR_PRV_CAMPAGNA_DIREZIONE();
                        direzione.ID_CAMPAGNA_DIR = db.XR_PRV_CAMPAGNA_DIREZIONE.GeneraPrimaryKey();
                        direzione.ID_DIREZIONE = dir.Id;
                        direzione.ORGANICO = dir.Organico;
                        direzione.ORGANICO_AD = dir.OrganicoAD;
                        direzione.BUDGET = Decimal.Parse(dir.BudgetStr, new NumberFormatInfo() { CurrencyDecimalSeparator = "." });
                        direzione.BUDGET_PERIODO = Decimal.Parse(dir.BudgetPeriodoStr, new NumberFormatInfo() { CurrencyDecimalSeparator = "." });
                        direzione.ID_CAMPAGNA = budget.ID_CAMPAGNA;
                        CezanneHelper.GetCampiFirma(out codUser, out codTermid, out timestamp);
                        direzione.COD_USER = codUser;
                        direzione.COD_TERMID = codTermid;
                        direzione.TMS_TIMESTAMP = timestamp;
                        db.XR_PRV_CAMPAGNA_DIREZIONE.Add(direzione);
                        //DBHelper.Save(db, "PoliticheRetributive - ");
                    }
                }
            }
            else
                return Content("Errore durante l'invio dei dati");

            if (DBHelper.Save(db, "Politiche retributive - "))
                result = "OK";
            else
                result = "Errore durante il salvataggio dei dati";

            return Content(result);
        }

        [HttpPost]
        public ActionResult SaveModCampagna(int idCampagna, string dataInizio, string dataFine, BudgetArea[] aree, string[] dateDecorrenza, decimal riserva, string nome)
        {
            string result = "";
            string codUser = "";
            string codTermid = "";
            DateTime timestamp;

            IncentiviEntities db = new IncentiviEntities();

            XR_PRV_CAMPAGNA campagna = db.XR_PRV_CAMPAGNA.FirstOrDefault(x => x.ID_CAMPAGNA == idCampagna);
            if (campagna != null)
            {
                if (String.IsNullOrWhiteSpace(dataFine))
                    campagna.DTA_FINE = null;
                else
                    campagna.DTA_FINE = DateTime.ParseExact(dataFine, "dd/MM/yyyy", null);
                campagna.NOME = nome;
                campagna.DEC_RISERVA = riserva;
                CezanneHelper.GetCampiFirma(out codUser, out codTermid, out timestamp);
                campagna.COD_USER = codUser;
                campagna.COD_TERMID = codTermid;
                campagna.TMS_TIMESTAMP = timestamp;
                DBHelper.Save(db,"PoliticheRetributive - ") ;

                if (dateDecorrenza != null && dateDecorrenza.Any())
                {
                    foreach (var data in dateDecorrenza)
                    {
                        DateTime tmp = DateTime.ParseExact(data, "dd/MM/yyyy", null);
                        if (!campagna.XR_PRV_CAMPAGNA_DECORRENZA.Any(x => x.DT_DECORRENZA == tmp))
                        {

                            XR_PRV_CAMPAGNA_DECORRENZA dec = new XR_PRV_CAMPAGNA_DECORRENZA();
                            dec.ID_DECORRENZA = db.XR_PRV_CAMPAGNA_DECORRENZA.GeneraPrimaryKey();
                            dec.ID_CAMPAGNA = campagna.ID_CAMPAGNA;
                            dec.DT_DECORRENZA = tmp;
                            CezanneHelper.GetCampiFirma(out codUser, out codTermid, out timestamp);
                            dec.COD_USER = codUser;
                            dec.COD_TERMID = codTermid;
                            dec.TMS_TIMESTAMP = timestamp;
                            db.XR_PRV_CAMPAGNA_DECORRENZA.Add(dec);
                            DBHelper.Save(db, "PoliticheRetributive - ");
                        }
                    }
                }

                if (aree != null)
                {

                    foreach (var area in aree)
                    {
                        XR_PRV_CAMPAGNA_BUDGET budget = db.XR_PRV_CAMPAGNA_BUDGET.FirstOrDefault(x => x.ID_CAMPAGNA == idCampagna && x.ID_AREA == area.Id);
                        budget.BUDGET = Decimal.Parse(area.ImportoStr, new NumberFormatInfo() { CurrencyDecimalSeparator = "." });
                        budget.BUDGET_PERIODO = Decimal.Parse(area.ImportoPeriodoStr, new NumberFormatInfo() { CurrencyDecimalSeparator = "." });
                        CezanneHelper.GetCampiFirma(out codUser, out codTermid, out timestamp);
                        budget.COD_USER = codUser;
                        budget.COD_TERMID = codTermid;
                        budget.TMS_TIMESTAMP = timestamp;
                        //db.XR_PRV_CAMPAGNA_BUDGET.Add(budget);
                        DBHelper.Save(db,"PoliticheRetributive - ") ;

                        foreach (var dir in area.Direzioni)
                        {
                            XR_PRV_CAMPAGNA_DIREZIONE direzione = db.XR_PRV_CAMPAGNA_DIREZIONE.FirstOrDefault(x => x.ID_CAMPAGNA == idCampagna && x.ID_DIREZIONE == dir.Id);
                            direzione.ORGANICO = dir.Organico;
                            direzione.ORGANICO_AD = dir.OrganicoAD;
                            direzione.BUDGET = Decimal.Parse(dir.BudgetStr, new NumberFormatInfo() { CurrencyDecimalSeparator = "." });
                            direzione.BUDGET_PERIODO = Decimal.Parse(dir.BudgetPeriodoStr, new NumberFormatInfo() { CurrencyDecimalSeparator = "." });
                            CezanneHelper.GetCampiFirma(out codUser, out codTermid, out timestamp);
                            direzione.COD_USER = codUser;
                            direzione.COD_TERMID = codTermid;
                            direzione.TMS_TIMESTAMP = timestamp;
                            //db.XR_PRV_CAMPAGNA_DIREZIONE.Add(direzione);
                            DBHelper.Save(db,"PoliticheRetributive - ") ;
                        }
                    }


                }
                else
                {
                    result = "Errore nell'invio dei dati";
                }


                try
                {
                    if (DBHelper.Save(db, "PoliticheRetributive - "))
                        result = "OK";
                    else
                        result = "Errore durante il salvataggio dei dati";
                }
                catch (Exception)
                {
                    result = "Errore durante il salvataggio dei dati";
                }
            }
            else
            {
                result = "Campagna non trovata";
            }

            return Content(result);
        }

        public ActionResult GetCampagne()
        {
            return View("~/Views/PoliticheRetributive/subpartial/Widget_Budget.cshtml");
        }
        #endregion

        #endregion

        #region NotePratica
        private string InternalAggiungiNotaPratica(int idDipendente, string notaPratica, int idPersona = 0)
        {
            string result = "";

            if (idPersona == 0)
                idPersona = CommonHelper.GetCurrentIdPersona();

            string codUser = "";
            string codTermid = "";
            DateTime timestamp;

            IncentiviEntities db = new IncentiviEntities();
            XR_PRV_DIPENDENTI_NOTE nota = new XR_PRV_DIPENDENTI_NOTE();
            nota.ID_NOTA = db.XR_PRV_DIPENDENTI_NOTE.GeneraPrimaryKey();
            nota.ID_DIPENDENTE = idDipendente;
            nota.ID_PERSONA = idPersona;
            nota.NOTA = notaPratica;
            CezanneHelper.GetCampiFirma(out codUser, out codTermid, out timestamp);
            nota.COD_USER = codUser;
            nota.COD_TERMID = codTermid;
            nota.TMS_TIMESTAMP = timestamp;
            db.XR_PRV_DIPENDENTI_NOTE.Add(nota);

            try
            {
                DBHelper.Save(db, "PoliticheRetributive - ");
                result = "OK";
            }
            catch (Exception)
            {
                result = "Errore nel salvataggio";
            }
            return result;
        }
        public ActionResult AggiungiNotaPratica(int idDipendente, string notaPratica)
        {
            string result = InternalAggiungiNotaPratica(idDipendente, notaPratica);

            return Content(result);
        }
        public ActionResult CancellaNotaPratica(int idNotaPratica)
        {
            string result = "";

            IncentiviEntities db = new IncentiviEntities();
            XR_PRV_DIPENDENTI_NOTE nota = db.XR_PRV_DIPENDENTI_NOTE.FirstOrDefault(x => x.ID_NOTA == idNotaPratica);
            if (nota != null)
            {
                string messaggio = "";
                int idPers = CommonHelper.GetCurrentIdPersona();
                if (nota.ID_PERSONA != idPers)
                {
                    string matrNota, nomeNota, matrCanc, nomeCanc;
                    GetPersData(nota.ID_PERSONA, out matrNota, out nomeNota);
                    GetPersData(idPers, out matrCanc, out nomeCanc);

                    messaggio = "Nota: '" + nota.NOTA + "' (Autore: " + matrNota + " - " + nomeNota + ") cancellata da " + matrCanc + " - " + nomeCanc;

                    PoliticheRetributiveManager.WriteLog(db, nota.XR_PRV_DIPENDENTI.ID_DIPENDENTE, messaggio);
                }

                db.XR_PRV_DIPENDENTI_NOTE.Remove(nota);
                try
                {
                    DBHelper.Save(db, "PoliticheRetributive - ");
                    result = "OK";
                }
                catch (Exception)
                {
                    result = "Errore nella cancellazione";
                }
            }
            else
            {
                result = "Nota non trovata";
            }

            return Content(result);
        }

        [HttpGet]
        public ActionResult GetNotePratica(int idDip)
        {
            XR_PRV_DIPENDENTI pratica = new XR_PRV_DIPENDENTI();
            IncentiviEntities db = new IncentiviEntities();
            pratica = db.XR_PRV_DIPENDENTI.FirstOrDefault(x => x.ID_DIPENDENTE == idDip);

            return PartialView("~/Views/PoliticheRetributive/subpartial/Dettaglio_Note.cshtml", pratica);
        }
        #endregion

        #region GestionePresaInCarico
        public ActionResult PrendiPratica(int idDip)
        {
            string result = "";

            RilasciaPratica(idDip);

            string coduser = "";
            string termid = "";
            DateTime timestamp;

            IncentiviEntities db = new IncentiviEntities();
            XR_PRV_OPERSTATI stato = new XR_PRV_OPERSTATI();
            stato.ID_OPER = db.XR_PRV_OPERSTATI.GeneraPrimaryKey();
            stato.ID_DIPENDENTE = idDip;
            stato.ID_PERSONA = CommonHelper.GetCurrentIdPersona();
            stato.ID_STATO = (int)ProvvStatoEnum.InCarico;
            stato.DATA = DateTime.Now;
            CezanneHelper.GetCampiFirma(out coduser, out termid, out timestamp);
            stato.COD_USER = coduser;
            stato.COD_TERMID = termid;
            stato.TMS_TIMESTAMP = timestamp;
            db.XR_PRV_OPERSTATI.Add(stato);

            try
            {
                DBHelper.Save(db, "PoliticheRetributive - ");
                result = "OK";
            }
            catch (Exception)
            {
                result = "Errore nel salvataggio";
            }

            return Content(result);
        }
        public ActionResult RilasciaPraticaAjax(int idDip)
        {
            RilasciaPratica(idDip);
            return Content("OK");
        }
        public void RilasciaPratica(int idDip)
        {
            IncentiviEntities db = new IncentiviEntities();
            XR_PRV_OPERSTATI stato = db.XR_PRV_OPERSTATI.FirstOrDefault(x => x.ID_DIPENDENTE == idDip && x.ID_STATO == (int)ProvvStatoEnum.InCarico);
            if (stato != null)
            {
                db.XR_PRV_OPERSTATI.Remove(stato);
                DBHelper.Save(db, "PoliticheRetributive - ");
            }
        }
        private void InvalidaStatoInternal(int idOper)
        {
            IncentiviEntities db = new IncentiviEntities();
            XR_PRV_OPERSTATI oper = db.XR_PRV_OPERSTATI.FirstOrDefault(x => x.ID_OPER == idOper);
            oper.DATA_FINE_VALIDITA = DateTime.Now;
            DBHelper.Save(db, "PoliticheRetributive - ");

        }
        private int SalvaStato(int idDip, int stato)
        {
            return PoliticheRetributiveHelper.SalvaStato(null, idDip, stato);
        }
        #endregion

        #region Utility
        public static decimal GetAliq(int idProv, string cat)
        {
            decimal result = 0;
            using (IncentiviEntities db = new IncentiviEntities())
            {
                XR_PRV_PROV prov = db.XR_PRV_PROV.FirstOrDefault(x => x.ID_PROV == idProv);
                result = HRDWData.GetProvvAliq(db, prov.BASE_PROV.Value, cat);
            }
            return result;
        }
        public void GetPersData(int idPersona, out string matricola, out string nominativo)
        {
            matricola = "";
            nominativo = "";
            using (IncentiviEntities db = new IncentiviEntities())
            {
                SINTESI1 pers = db.SINTESI1.FirstOrDefault(x => x.ID_PERSONA == idPersona);
                if (pers != null)
                {
                    matricola = pers.COD_MATLIBROMAT;
                    nominativo = pers.DES_COGNOMEPERS.TitleCase() + " " + pers.DES_NOMEPERS.TitleCase();
                }
            }
        }
        public static myRaiCommonModel.Gestionale.GestioneStatiClass GetGestStati(int idPersona, bool inServizio, XR_PRV_DIPENDENTI dip)
        {
            myRaiCommonModel.Gestionale.GestioneStatiClass gestClass = new myRaiCommonModel.Gestionale.GestioneStatiClass();


            var currentState = dip.XR_PRV_OPERSTATI.Where(y => !y.DATA_FINE_VALIDITA.HasValue).OrderByDescending(x => x.ID_STATO).FirstOrDefault();
            if (currentState != null)
            {
                gestClass.CurrentState = currentState.ID_STATO;
                gestClass.IdCurrentState = currentState.ID_OPER;
            }

            bool consDone = gestClass.CurrentState >= (int)ProvvStatoEnum.Convalidato;

            //bool vaglDone = gestClass.CurrentState >= (int)IncStato.Controllato;
            //bool contDone = gestClass.CurrentState >= (int)IncStato.Conteggio;
            //bool appDone = gestClass.CurrentState >= (int)IncStato.Appuntamento;
            //bool verbFirmDone = gestClass.CurrentState >= (int)IncStato.VerbaleFirmato;
            //bool verbUploadDone = gestClass.CurrentState >= (int)IncStato.VerbaleCaricato;
            //bool pagDone = gestClass.CurrentState >= (int)IncStato.Cedolini;

            //gestClass.praticaChiusa = pagDone;

            //gestClass.classTabCont = contDone ? "completed" : "active";
            //gestClass.classTabApp = appDone & verbFirmDone ? "completed" : contDone ? "active" : "";
            //gestClass.classTabVerbFirm = verbFirmDone ? "completed" : appDone ? "active" : "";
            //gestClass.classTabVerbUpload = verbUploadDone ? "completed" : verbFirmDone ? "active" : "";
            //gestClass.classTabPag = pagDone ? "completed" : verbUploadDone ? "active" : "";

            gestClass.showTabSceltaProvv = !consDone;
            gestClass.showTabPanelCons = consDone;
            //gestClass.showTabPanelCont = !contDone;
            //gestClass.showTabPanelApp = contDone && !appDone;
            //gestClass.showTabPanelVerbFirm = contDone && appDone && !verbFirmDone;
            //gestClass.showTabPanelVerbUpload = contDone && appDone && verbFirmDone && !verbUploadDone;
            //gestClass.showTabPanelPag = contDone && appDone && verbFirmDone && verbUploadDone && !pagDone;

            gestClass.PraticaInLavorazione = dip.XR_PRV_OPERSTATI.Any(x => x.ID_STATO == (int)ProvvStatoEnum.InCarico);
            gestClass.MiaPratica = dip.XR_PRV_OPERSTATI.Any(x => x.ID_STATO == (int)ProvvStatoEnum.InCarico && x.ID_PERSONA == idPersona);
            //&& (!applyFilterStates || enabledStates.Contains(gestClass.CurrentState)); //Questa parte serve per la presentazione

            gestClass.showPrendiInCarico = //vaglDone
                                           //&& !pagDone
                                           //&& (!applyFilterStates || enabledStates.Contains(gestClass.CurrentState))
                                           //&&
                                            (
                                                (!gestClass.PraticaInLavorazione && !gestClass.MiaPratica)
                                                ||
                                                (!gestClass.MiaPratica && !inServizio)
                                               );
            gestClass.showAvviaPratica = false;// !vaglDone && (!applyFilterStates || enabledStates.Contains((int)IncStato.DaAvviare));
            gestClass.showTab = gestClass.MiaPratica;

            //gestClass.percCompl = 0 + (contDone ? 35 : 0) + (verbUploadDone ? 35 : 0);

            return gestClass;
        }
        public static List<XR_PRV_PROV> GetProvS(bool escludiNessuno)
        {
            IncentiviEntities db = new IncentiviEntities();
            return db.XR_PRV_PROV.Where(y => !escludiNessuno || y.ID_PROV != (int)ProvvedimentiEnum.Nessuno).OrderBy(x => x.ORDINE).ToList();
        }
        public static IEnumerable<CampagnaWrapper> GetCampagneAttive()
        {
            IncentiviEntities db = new IncentiviEntities();

            List<CampagnaWrapper> campagne = new List<CampagnaWrapper>();

            var funcFilter = PoliticheRetributiveHelper.FuncFilterCampagna();

            //var tmp = db.XR_PRV_CAMPAGNA.Where(x => x.DTA_INIZIO <= DateTime.Today && (x.DTA_FINE == null || x.DTA_FINE >= DateTime.Today));
            //tmp = tmp.Where(funcFilter);

            foreach (var campagna in db.XR_PRV_CAMPAGNA.Where(x => (x.IND_CONTENITORE == null || !x.IND_CONTENITORE.Value) && x.DTA_INIZIO <= DateTime.Today && (x.DTA_FINE == null || x.DTA_FINE >= DateTime.Today)).Where(funcFilter))
            {
                campagne.Add(new CampagnaWrapper()
                {
                    campagna = campagna,
                    EnableGest = PoliticheRetributiveHelper.EnableGestCampagna(campagna.LV_ABIL)
                });
            }

            return campagne;
        }
        #endregion

        #region Dropdown
        public static List<ListItem> getBoolList()
        {
            List<ListItem> lista = new List<ListItem>();

            ListItem listItem = new ListItem()
            {
                Value = "true",
                Text = "Escludi cessati",
                Selected = true
            };
            lista.Add(listItem);

            listItem = new ListItem()
            {
                Value = "false",
                Text = "Includi cessati"
            };
            lista.Add(listItem);

            return lista;
        }
        public static List<ListItem> getSediList()
        {
            List<ListItem> list = new List<ListItem>();
            list.Add(new ListItem() { Value = "", Text = "Sede", Selected = true });

            IncentiviEntities db = new IncentiviEntities();
            list.AddRange(db.SINTESI1.Where(y => y.DTA_FINE_CR == null || y.DTA_FINE_CR > DateTime.Today).Select(x => new ListItem() { Value = x.COD_SEDE, Text = x.DES_SEDE }).Distinct().OrderBy(y => y.Text));

            return list;
        }
        public static List<SelectListGroup> getServizioList()
        {
            IncentiviEntities db = new IncentiviEntities();
            List<SelectListGroup> list = new List<SelectListGroup>();

            string matricola = CommonHelper.GetCurrentUserMatricola();
            List<int> enableDir = null;
            bool applyFilters = PoliticheRetributiveHelper.GetEnabledDirezioni(db, CommonHelper.GetCurrentUserMatricola(), out enableDir);

            Expression<Func<XR_PRV_AREA, bool>> funcArea = PoliticheRetributiveHelper.FuncFilterArea();

            foreach (var area in db.XR_PRV_AREA.Where(funcArea))
            {
                if (!applyFilters || area.XR_PRV_DIREZIONE.Any(x => enableDir.Contains(x.ID_DIREZIONE)))
                {
                    SelectListGroup listGroup = new SelectListGroup();
                    listGroup.Name = area.NOME;
                    listGroup.ListItems = new List<ListItem>();
                    listGroup.ListItems.AddRange(area.XR_PRV_DIREZIONE.Where(x => !applyFilters || enableDir.Contains(x.ID_DIREZIONE)).OrderBy(x => x.NOME).Select(y => new ListItem() { Value = y.CODICE, Text = y.NOME }));
                    list.Add(listGroup);
                }
            }

            return list;
        }
        public static List<ListItem> getQualList()
        {
            List<ListItem> list = new List<ListItem>();
            list.Add(new ListItem() { Value = "", Text = "Seleziona una categoria", Selected = true });

            IncentiviEntities db = new IncentiviEntities();
            Expression<Func<XR_PRV_DIPENDENTI, bool>> funcFilterAbilServizio = PoliticheRetributiveHelper.FuncFilterDirezione(db);
            Expression<Func<XR_PRV_DIPENDENTI, bool>> funcFilterAbilMatr = PoliticheRetributiveHelper.FuncFilterMatr(db);
            Expression<Func<XR_PRV_DIPENDENTI, bool>> funcFilterAreaPratica = PoliticheRetributiveHelper.FuncFilterAreaPratica();

            list.AddRange(
            db.XR_PRV_DIPENDENTI.Where(funcFilterAreaPratica).Where(funcFilterAbilServizio).Where(funcFilterAbilMatr)
            .Select(x => new ListItem() { Value = x.SINTESI1.COD_QUALIFICA, Text = x.SINTESI1.DES_QUALIFICA }).Distinct().OrderBy(y => y.Text)
            );

            return list;
        }
        public static List<ListItem> getProvvList()
        {
            List<ListItem> list = new List<ListItem>();
            list.Add(new ListItem() { Value = "0", Text = "Seleziona un provvedimento", Selected = true });

            IncentiviEntities db = new IncentiviEntities();
            foreach (var item in db.XR_PRV_PROV.OrderBy(x => x.ORDINE))
            {
                list.Add(new ListItem()
                {
                    Value = item.ID_PROV.ToString(),
                    Text = item.NOME + (item.CUSTOM.GetValueOrDefault() ? " (manuale)" : "")
                });
            }

            return list;
        }
        public static List<ListItem> getGestioneManuale()
        {
            List<ListItem> list = new List<ListItem>();

            list.Add(new ListItem() { Value = "", Text = "Seleziona il tipo di gestione" });
            list.Add(new ListItem() { Value = "0", Text = "Standard" });
            list.Add(new ListItem() { Value = "1", Text = "Manuale" });

            return list;
        }
        public static List<ListItem> getGestExtList()
        {
            List<ListItem> list = new List<ListItem>();

            list.Add(new ListItem() { Value = "", Text = "Seleziona il tipo di gestione" });
            list.Add(new ListItem() { Value = "0", Text = "Standard" });
            list.Add(new ListItem() { Value = "1", Text = "Esterna al sistema" });

            return list;
        }
        public static List<ListItem> getStatiList()
        {
            List<ListItem> list = new List<ListItem>();
            list.Add(new ListItem() { Value = "0", Text = "Seleziona uno stato", Selected = true });

            IncentiviEntities db = new IncentiviEntities();
            foreach (var item in db.XR_PRV_STATI.Where(y => y.ID_STATO > 0).OrderBy(x => x.ID_STATO))
            {
                list.Add(new ListItem()
                {
                    Value = item.ID_STATO.ToString(),
                    Text = item.DESCRIZIONE
                });
            }

            return list;
        }
        public ActionResult getAJAXProvvList()
        {
            IncentiviEntities db = new IncentiviEntities();

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { result = db.XR_PRV_PROV.Select(x => new { Value = x.ID_PROV, Text = x.NOME }) }
            };
        }
        public static List<ListItem> getElabList()
        {
            List<ListItem> lista = new List<ListItem>();

            ListItem listItem = new ListItem()
            {
                Value = "",
                Text = "Elaborazione",
                Selected = true
            };
            lista.Add(listItem);

            listItem = new ListItem()
            {
                Value = "0",
                Text = "Non in carico",
                Selected = true
            };
            lista.Add(listItem);

            listItem = new ListItem()
            {
                Value = "1",
                Text = "In carico"
            };
            lista.Add(listItem);

            return lista;
        }
        public static List<ListItem> getUnitaOrgList()
        {
            List<ListItem> list = new List<ListItem>();
            list.Add(new ListItem() { Value = "", Text = "Direzione", Selected = true });

            IncentiviEntities db = new IncentiviEntities();
            list.AddRange(db.SINTESI1.Where(y => y.COD_UNITAORG != null && (y.DTA_FINE_CR == null || y.DTA_FINE_CR > DateTime.Today))
                .Select(x => new ListItem() { Value = x.COD_UNITAORG, Text = x.DES_DENOMUNITAORG + " (" + x.COD_UNITAORG + ")" })
                .Distinct()
                .OrderBy(y => y.Text));

            return list;
        }
        public static List<ListItem> getCampagnaList(bool addNessunFiltro = false, bool mostraChiuse = false)
        {
            List<ListItem> list = new List<ListItem>();

            if (addNessunFiltro)
                list.Add(new ListItem() { Value = "0", Text = "Seleziona un piano" });

            var funcFilter = PoliticheRetributiveHelper.FuncFilterCampagna();

            IncentiviEntities db = new IncentiviEntities();
            var listCamp = db.XR_PRV_CAMPAGNA.AsQueryable();
            if (!mostraChiuse)
                listCamp = listCamp.Where(y => y.DTA_INIZIO <= DateTime.Today && (y.DTA_FINE == null || y.DTA_FINE >= DateTime.Today));


            list.AddRange(listCamp
                .Where(funcFilter)
                .OrderBy(x => x.ID_CAMPAGNA > 2)
                .ThenBy(x => x.DTA_INIZIO)
                .ToList()
                .Select(x => new ListItem()
                {
                    Value = x.ID_CAMPAGNA.ToString(),
                    Text = x.NOME + (x.ID_CAMPAGNA > 2 ? " - Inizio: " + x.DTA_INIZIO.Value.ToString("dd/MM/yyyy") + (x.DTA_FINE.HasValue ? " - Fine: " + x.DTA_FINE.Value.ToString("dd/MM/yyyy") : "") : "") + (x.LV_ABIL == PoliticheRetributiveHelper.BUDGETRS_HRGA_SOTTO_FUNC ? " - Risorse chiave" : "")
                }));

            return list;
        }
        public static List<ListItem> getCampagnaDecorrenza(int idCampagna, DateTime? dec)
        {
            List<ListItem> list = new List<ListItem>();
            list.Add(new ListItem() { Value = "", Text = "Data decorrenza" });

            IncentiviEntities db = new IncentiviEntities();
            list.AddRange(db.XR_PRV_CAMPAGNA_DECORRENZA.Where(y => y.ID_CAMPAGNA == idCampagna)
                .OrderBy(x => x.DT_DECORRENZA)
                .ToList()
                .Select(x => new ListItem()
                {
                    Value = x.DT_DECORRENZA.ToString("dd/MM/yyyy"),
                    Text = x.DT_DECORRENZA.ToString("dd/MM/yyyy"),
                    Selected = dec.HasValue && dec.Value.ToString("dd/MM/yyyy") == x.DT_DECORRENZA.ToString("dd/MM/yyyy")
                }));

            return list;
        }
        public static List<ListItem> getDecorrenzeVisibili()
        {
            List<ListItem> list = new List<ListItem>();
            list.Add(new ListItem() { Value = "", Text = "Scegli la data di decorrenza" });

            var funcFilter = PoliticheRetributiveHelper.FuncFilterCampagna();

            IncentiviEntities db = new IncentiviEntities();
            list.AddRange(db.XR_PRV_CAMPAGNA
                .Where(funcFilter)
                .ToList()
                .SelectMany(x => x.XR_PRV_CAMPAGNA_DECORRENZA)
                .Select(y => y.DT_DECORRENZA)
                .Distinct()
                .OrderBy(w => w)
                .Select(z => new ListItem()
                {
                    Value = z.ToString("dd/MM/yyyy"),
                    Text = z.ToString("dd/MM/yyyy")
                })
            );

            return list;
        }
        public ActionResult getAJAXDecorrenzaCampagna(int idCampagna)
        {
            IncentiviEntities db = new IncentiviEntities();

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { result = db.XR_PRV_CAMPAGNA_DECORRENZA.Where(y => y.ID_CAMPAGNA == idCampagna).OrderBy(x => x.DT_DECORRENZA).ToList().Select(x => new { Value = x.DT_DECORRENZA.ToString("dd/MM/yyyy"), Text = x.DT_DECORRENZA.ToString("dd/MM/yyyy") }) }
            };
        }
        public static List<SelectListGroup> getDirezioniPratica()
        {
            IncentiviEntities db = new IncentiviEntities();
            List<SelectListGroup> list = new List<SelectListGroup>();

            Expression<Func<XR_PRV_DIPENDENTI, bool>> funcArea = PoliticheRetributiveHelper.FuncFilterAreaPratica();
            Expression<Func<XR_PRV_DIPENDENTI, bool>> funcDirezioni = PoliticheRetributiveHelper.FuncFilterDirezione(db);

            var tmp = db.XR_PRV_DIPENDENTI
                .Where(funcArea)
                .Where(funcDirezioni)
                .GroupBy(x => x.XR_PRV_DIREZIONE.ID_AREA).Select(y => new
                {
                    Id = y.Key,
                    Nome = y.FirstOrDefault().XR_PRV_DIREZIONE.XR_PRV_AREA.NOME,
                    Direzioni = y.GroupBy(a => a.ID_DIREZIONE).Select(b => new
                    {
                        Id = b.Key,
                        Nome = b.FirstOrDefault().XR_PRV_DIREZIONE.NOME
                    })
                });

            foreach (var area in tmp)
            {
                SelectListGroup listGroup = new SelectListGroup();
                listGroup.Name = area.Nome;
                listGroup.ListItems = new List<ListItem>();
                listGroup.ListItems.AddRange(area.Direzioni.OrderBy(x => x.Nome).Select(y => new ListItem() { Value = y.Id.ToString(), Text = y.Nome }));
                list.Add(listGroup);
            }

            return list;
        }

        public static List<SelectListGroup> getBozzeList()
        {
            IncentiviEntities db = new IncentiviEntities();
            List<SelectListGroup> list = new List<SelectListGroup>();
            DateTime oggi = DateTime.Today;

            var tmp = db.XR_PRV_TEMPLATE.Where(x => x.IND_BODY &&
             (x.VALID_DTA_END == null || 
             (x.VALID_DTA_END != null && x.VALID_DTA_END >= oggi))).GroupBy(x => x.ID_PROV);

            foreach (var y in tmp)
            {
                SelectListGroup listGroup = new SelectListGroup();
                listGroup.Name = y.Key == 1 || y.Key == 4 ? "Promozione" : y.Key == 2 ? "Aumento di merito" : "Gratifica";
                listGroup.ListItems = new List<ListItem>();
                listGroup.ListItems.AddRange(y.OrderBy(x => x.NOME).Select(x => new ListItem() { Value = x.ID_TEMPLATE.ToString(), Text = x.NOME }));
                list.Add(listGroup);
            }

            return list;
        }
        #endregion

        [HttpPost]
        public ActionResult AggiornaConteggioUtente(int idDip, DateTime? dataDecorrenza, int piano, int idProv, XR_PRV_DIPENDENTI_VARIAZIONI[] customProv, string catRich, bool isGestExt, int idTemplate, string codMansione, string statoLettera, string livRich)
        {
            //if (!UtenteHelper.IsAdmin())
            //    return Redirect("/Home/notAuth");

            IncentiviEntities db = new IncentiviEntities();

            PoliticheRetributiveManager.WriteLog(db, 0, "Aggiornamento conteggio costi utente " + idDip);

            string elencoMatricole = " SELECT MATRICOLA FROM XR_PRV_DIPENDENTI WHERE ID_DIPENDENTE = " + idDip;

            CezanneHelper.GetCampiFirma(out var campiFirma);
            //HRDWData.CalcoloCostiMassivo(db, campiFirma, elencoMatricole, x => x.ID_DIPENDENTE == idDip, true, true, true, true, dataDecorrenza, true);
            XR_PRV_DIPENDENTI dip = db.XR_PRV_DIPENDENTI.FirstOrDefault(x => x.ID_DIPENDENTE == idDip);
            //A differenza del massivo che se gli passi una data decorrenza aggiorna il dipendente,
            //con la CalcoloCosti è necessario farlo prima
            if (dataDecorrenza.HasValue)
                dip.DECORRENZA = dataDecorrenza.Value;
            HRDWData.CalcoloCosti(db, dip, false, campiFirma); //Aggiorna conteggio utente


            int idPersona = CommonHelper.GetCurrentIdPersona();

            if (piano != dip.ID_CAMPAGNA)
            {
                dip.XR_PRV_CAMPAGNA = db.XR_PRV_CAMPAGNA.FirstOrDefault(x => x.ID_CAMPAGNA == piano);
            }
            if (idProv != dip.ID_PROV_EFFETTIVO)
            {
                dip.ID_PROV_EFFETTIVO = idProv;
                PoliticheRetributiveHelper.SetProvEffettivo(db, dip, idProv);
            }
            if (catRich != dip.CAT_RICHIESTA)
            {
                dip.CAT_RICHIESTA = catRich;
            }
            if (isGestExt != dip.IND_PRATICA_EXT)
            {
                dip.IND_PRATICA_EXT = isGestExt;
            }
            if (idTemplate != dip.ID_TEMPLATE.GetValueOrDefault())
            {
                dip.ID_TEMPLATE = idTemplate;
            }
            if (codMansione != dip.COD_MANSIONE)
            {
                dip.COD_MANSIONE = codMansione;
            }
            if (livRich != dip.LIV_RICHIESTA)
            {
                dip.LIV_RICHIESTA = livRich;
            }

            if (String.IsNullOrWhiteSpace(statoLettera))
                dip.STATO_LETTERA = null;
            else
                dip.STATO_LETTERA = Convert.ToInt32(statoLettera);


            if (dip.CUSTOM_PROV.GetValueOrDefault() && customProv != null)
            {
                foreach (var custom in customProv.Where(x => x.ID_PROV != 5 && x.ID_PROV != 10))
                {
                    var v = PoliticheRetributiveHelper.GetDipProv(dip, custom.ID_PROV);
                    v.DIFF_RAL = custom.DIFF_RAL;
                    v.COSTO_ANNUO = custom.COSTO_ANNUO;
                    v.COSTO_PERIODO = custom.COSTO_PERIODO;
                }
            }

            Pratica pratica = DettaglioPratica(db, dip);

            return View("~/Views/PoliticheRetributive/subpartial/Modifica_Provvedimento.cshtml", pratica);
        }

        #region GestioneLettera
        public ActionResult ScaricaLettera(int idPratica)
        {
            IncentiviEntities db = new IncentiviEntities();

            var pratica = db.XR_PRV_DIPENDENTI.Include("XR_PRV_OPERSTATI").Include("XR_PRV_OPERSTATI.XR_PRV_OPERSTATI_DOC").Include("XR_PRV_PROV_EFFETTIVO").FirstOrDefault(x => x.ID_DIPENDENTE == idPratica);
            string nomeFile = "";
            MemoryStream ms = PoliticheRetributiveManager.CreaLettera(db, pratica, out nomeFile);
            return new FileStreamResult(ms, "application/pdf") { FileDownloadName = nomeFile };
        }

        public ActionResult ScaricaLettere()
        {
            try
            {
                string nomeFile = "";
                MemoryStream outputMemStream = PoliticheRetributiveManager.CreaLettere(out nomeFile, null, null, null);
                return new FileStreamResult(outputMemStream, "application/pdf") { FileDownloadName = nomeFile };
            }
            catch (Exception ex)
            {
                throw ex;
                //return View("~/Views/Shared/404.cshtml");
            }
        }

        public ActionResult PreparaLettereStampa()
        {
            PoliticheRetributiveManager.CreaDirectoryLettere();

            return Content("OK");
        }

        public ActionResult AggiungiLetteraModificata(int idOper, HttpPostedFileBase _fileUpload, string fileName, string descr)
        {
            IncentiviEntities db = new IncentiviEntities();

            XR_PRV_OPERSTATI_DOC doc = new XR_PRV_OPERSTATI_DOC();
            doc.ID_ALLEGATO = db.XR_PRV_OPERSTATI_DOC.GeneraPrimaryKey();
            doc.ID_OPER = idOper;
            doc.NME_FILENAME = fileName;
            doc.DES_ALLEGATO = descr;
            doc.CONTENT_TYPE = _fileUpload.ContentType;
            using (MemoryStream ms = new MemoryStream())
            {
                _fileUpload.InputStream.CopyTo(ms);
                doc.OBJ_OBJECT = ms.ToArray();
            }
            doc.COD_USER = CommonHelper.GetCurrentUserPMatricola();
            doc.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
            doc.TMS_TIMESTAMP = DateTime.Now;
            db.XR_PRV_OPERSTATI_DOC.Add(doc);
            db.SaveChanges();

            return Content("OK");
        }

        public ActionResult ModificaLettera(int idPratica)
        {
            IncentiviEntities db = new IncentiviEntities();
            var pratica = db.XR_PRV_DIPENDENTI.Include("XR_PRV_OPERSTATI").Include("XR_PRV_OPERSTATI.XR_PRV_OPERSTATI_DOC").Include("XR_PRV_PROV_EFFETTIVO").FirstOrDefault(x => x.ID_DIPENDENTE == idPratica);
            return View("~/Views/PoliticheRetributive/subpartial/Modifica_DatiLettera.cshtml", PoliticheRetributiveManager.GetLetteraModel(db, pratica, false));
        }

        public ActionResult SaveModificaLettera(Lettera lettera)
        {
            IncentiviEntities db = new IncentiviEntities();

            bool isNew = false;
            var modLettera = db.XR_PRV_DIPENDENTI_DOC.FirstOrDefault(x => x.ID_DIPENDENTE == lettera.IdPratica);
            if (modLettera == null)
            {
                isNew = true;
                modLettera = new XR_PRV_DIPENDENTI_DOC()
                {
                    ID_DIP_DOC = db.XR_PRV_DIPENDENTI_DOC.GeneraPrimaryKey(),
                    ID_DIPENDENTE = lettera.IdPratica
                };
            }

            modLettera.IND_HEADER = true;
            modLettera.HEADER_TEXT = lettera.HeaderText;
            modLettera.IND_BODY = true;
            modLettera.BODY_TEXT = lettera.BodyText;
            modLettera.IND_FOOTER = true;
            modLettera.FOOTER_TEXT = lettera.FooterText;
            modLettera.DATA = lettera.Data;

            string codUser, codTermid;
            DateTime timestamp;
            CezanneHelper.GetCampiFirma(out codUser, out codTermid, out timestamp);
            modLettera.COD_USER = codUser;
            modLettera.COD_TERMID = codTermid;
            modLettera.TMS_TIMESTAMP = timestamp;

            if (isNew)
                db.XR_PRV_DIPENDENTI_DOC.Add(modLettera);

            if (!DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                return Content("Errore durante il salvataggio");

            return Content("OK");
        }
        #endregion

        public ActionResult AggiungiNuovaDirezione()
        {
            IncentiviEntities db = new IncentiviEntities();

            int idAreaRS = 503234587;
            //int idAreaReg = 1032326076; //staff
            int idAreaReg = 1681299465; //editoriale

            XR_PRV_DIREZIONE newDirRS = new XR_PRV_DIREZIONE()
            {
                ID_DIREZIONE = db.XR_PRV_DIREZIONE.GeneraPrimaryKey(),
                ID_AREA = idAreaRS,
                NOME = "PUBBLICA UTILITA'",
                CODICE = "5G",
                PROTOCOLLO = "RUO/GSR/S",
                ORDINE = db.XR_PRV_DIREZIONE.Where(x => x.ID_AREA == idAreaRS).Max(y => y.ORDINE) + 1,
                COD_USER = "ADMIN",
                COD_TERMID = "BATCHSESSION",
                TMS_TIMESTAMP = DateTime.Now
            };
            db.XR_PRV_DIREZIONE.Add(newDirRS);

            XR_PRV_DIREZIONE newDirReg = new XR_PRV_DIREZIONE()
            {
                ID_DIREZIONE = db.XR_PRV_DIREZIONE.GeneraPrimaryKey(),
                ID_AREA = idAreaReg,
                NOME = "PUBBLICA UTILITA'",
                CODICE = "5G",
                PROTOCOLLO = "RUO/GSR/S",
                ORDINE = db.XR_PRV_DIREZIONE.Where(x => x.ID_AREA == idAreaReg).Max(y => y.ORDINE) + 1,
                COD_USER = "ADMIN",
                COD_TERMID = "BATCHSESSION",
                TMS_TIMESTAMP = DateTime.Now
            };
            db.XR_PRV_DIREZIONE.Add(newDirReg);

            foreach (var campagna in db.XR_PRV_CAMPAGNA.Where(x => x.LV_ABIL == "BDGRS"))
            {
                XR_PRV_CAMPAGNA_DIREZIONE cDRS = new XR_PRV_CAMPAGNA_DIREZIONE()
                {
                    ID_CAMPAGNA_DIR = db.XR_PRV_CAMPAGNA_DIREZIONE.GeneraPrimaryKey(),
                    ID_CAMPAGNA = campagna.ID_CAMPAGNA,
                    ID_DIREZIONE = newDirRS.ID_DIREZIONE,
                    ORGANICO = 0,
                    ORGANICO_M = 0,
                    ORGANICO_F = 0,
                    ORGANICO_M_AD = 0,
                    ORGANICO_F_AD = 0,
                    BUDGET = 0,
                    BUDGET_PERIODO = 0,
                    COD_USER = "ADMIN",
                    COD_TERMID = "BATCHSESSION",
                    TMS_TIMESTAMP = DateTime.Now
                };
                db.XR_PRV_CAMPAGNA_DIREZIONE.Add(cDRS);
            }

            foreach (var campagna in db.XR_PRV_CAMPAGNA.Where(x => x.LV_ABIL == "BDGQIO"))
            {
                XR_PRV_CAMPAGNA_DIREZIONE cDReg = new XR_PRV_CAMPAGNA_DIREZIONE()
                {
                    ID_CAMPAGNA_DIR = db.XR_PRV_CAMPAGNA_DIREZIONE.GeneraPrimaryKey(),
                    ID_CAMPAGNA = campagna.ID_CAMPAGNA,
                    ID_DIREZIONE = newDirReg.ID_DIREZIONE,
                    ORGANICO = 0,
                    ORGANICO_M = 0,
                    ORGANICO_F = 0,
                    ORGANICO_M_AD = 0,
                    ORGANICO_F_AD = 0,
                    BUDGET = 0,
                    BUDGET_PERIODO = 0,
                    COD_USER = "ADMIN",
                    COD_TERMID = "BATCHSESSION",
                    TMS_TIMESTAMP = DateTime.Now
                };
                db.XR_PRV_CAMPAGNA_DIREZIONE.Add(cDReg);
            }

            db.SaveChanges();

            return Content("OK");
        }

        public ActionResult ScaricaFirme()
        {
            IncentiviEntities db = new IncentiviEntities();
            var signFV = db.XR_PRV_TEMPLATE.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).Where(x => x.NOME == "SIGN FV");
            foreach (var item in signFV)
            {
                item.TEMPLATE = System.IO.File.ReadAllBytes(@"C:\Users\EDXC707\Desktop\Esempi\signFV_1.png");
            }


            var signDL = db.XR_PRV_TEMPLATE.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).Where(x => x.NOME == "SIGN FDL");
            foreach (var item in signDL)
            {
                item.TEMPLATE = System.IO.File.ReadAllBytes(@"C:\Users\EDXC707\Desktop\Esempi\signFDL_1_2.png");
            }

            var signSB = db.XR_PRV_TEMPLATE.Where(a => a.VALID_DTA_END == null || a.VALID_DTA_END > DateTime.Now).Where(x => x.NOME == "SIGN SB");
            foreach (var item in signSB)
            {
                item.TEMPLATE = System.IO.File.ReadAllBytes(@"C:\Users\EDXC707\Desktop\Esempi\signSB_1.png");
            }

            db.SaveChanges();
            return Content("OK");
        }

        public ActionResult AddBaseTemplate()
        {
            IncentiviEntities db = new IncentiviEntities();
            db.XR_PRV_TEMPLATE.Add(new XR_PRV_TEMPLATE()
            {
                ID_TEMPLATE = 3,
                NOME = "BASE",
                CAT_INCLUSE = null,
                CAT_ESCLUSE = null,
                ID_PROV = 0,
                DIR_INCLUSE = null,
                DIR_ESCLUSE = null,
                IND_BODY = false,
                IND_HEADER = false,
                IND_FOOTER = false,
                IND_SIGN = false,
                TEMPLATE = System.IO.File.ReadAllBytes(@"C:\tmp\base.pdf"),
                TEMPLATE_TEXT = "",
                VALID_DTA_INI = new DateTime(2019, 12, 1),
                VALID_DTA_END = null,
                COD_USER = "ADMIN",
                COD_TERMID = "BATCHSESSION",
                TMS_TIMESTAMP = DateTime.Now,
            });

            db.SaveChanges();

            return Content("OK");
        }

        public ActionResult AddFirme()
        {
            IncentiviEntities db = new IncentiviEntities();
            var signSB = db.XR_PRV_TEMPLATE.FirstOrDefault(x => x.NOME == "SIGN SB");
            db.XR_PRV_TEMPLATE.Add(new XR_PRV_TEMPLATE()
            {
                ID_TEMPLATE = db.XR_PRV_TEMPLATE.GeneraPrimaryKey(),
                NOME = signSB.NOME,
                CAT_INCLUSE = signSB.CAT_INCLUSE,
                CAT_ESCLUSE = signSB.CAT_ESCLUSE,
                ID_PROV = 2,
                DIR_INCLUSE = signSB.DIR_INCLUSE,
                DIR_ESCLUSE = signSB.DIR_ESCLUSE,
                IND_BODY = false,
                IND_HEADER = false,
                IND_FOOTER = false,
                IND_SIGN = true,
                TEMPLATE = signSB.TEMPLATE,
                TEMPLATE_TEXT = signSB.TEMPLATE_TEXT,
                VALID_DTA_INI = new DateTime(2019, 12, 1),
                VALID_DTA_END = null,
                COD_USER = "ADMIN",
                COD_TERMID = "BATCHSESSION",
                TMS_TIMESTAMP = DateTime.Now,
            });
            db.XR_PRV_TEMPLATE.Add(new XR_PRV_TEMPLATE()
            {
                ID_TEMPLATE = db.XR_PRV_TEMPLATE.GeneraPrimaryKey(),
                NOME = signSB.NOME,
                CAT_INCLUSE = signSB.CAT_INCLUSE,
                CAT_ESCLUSE = signSB.CAT_ESCLUSE,
                ID_PROV = 3,
                DIR_INCLUSE = signSB.DIR_INCLUSE,
                DIR_ESCLUSE = signSB.DIR_ESCLUSE,
                IND_BODY = false,
                IND_HEADER = false,
                IND_FOOTER = false,
                IND_SIGN = true,
                TEMPLATE = signSB.TEMPLATE,
                TEMPLATE_TEXT = signSB.TEMPLATE_TEXT,
                VALID_DTA_INI = new DateTime(2019, 12, 1),
                VALID_DTA_END = null,
                COD_USER = "ADMIN",
                COD_TERMID = "BATCHSESSION",
                TMS_TIMESTAMP = DateTime.Now,
            });

            db.SaveChanges();

            return Content("OK");
        }

        public ActionResult AggiungiConvalida()
        {
            int idPersona = CommonHelper.GetCurrentIdPersona();

            IncentiviEntities db = new IncentiviEntities();

            var list = db.XR_PRV_DIPENDENTI.Include("XR_PRV_OPERSTATI")
                        .Where(x => x.ID_CAMPAGNA > 2 && !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO >= 2));

            foreach (var item in list)
            {
                string codUser = "";
                string termid = "";
                DateTime timestamp;

                CezanneHelper.GetCampiFirma(out codUser, out termid, out timestamp);
                db.XR_PRV_OPERSTATI.Add(new XR_PRV_OPERSTATI()
                {
                    ID_OPER = db.XR_PRV_OPERSTATI.GeneraPrimaryKey(),
                    ID_DIPENDENTE = item.ID_DIPENDENTE,
                    ID_STATO = 2,
                    DATA = DateTime.Now,
                    ID_PERSONA = idPersona,
                    COD_USER = codUser,
                    COD_TERMID = termid,
                    TMS_TIMESTAMP = timestamp
                });
            }

            db.SaveChanges();

            return Content("OK");
        }

        public ActionResult CaricaProtProd()
        {
            IncentiviEntities db = new IncentiviEntities();

            Dictionary<string, string> matrProt = System.IO.File.ReadAllLines(@"c:\tmp\protPRD.csv").Select(x => new KeyValuePair<string, string>(x.Split(';')[1], x.Split(';')[0])).ToDictionary(y => y.Key, z => z.Value);

            var list = db.XR_PRV_DIPENDENTI.Where(x => matrProt.Keys.Contains(x.MATRICOLA));

            foreach (var pratica in list)
            {
                if (!db.XR_PRV_DIPENDENTI_DOC.Any(x => x.ID_DIPENDENTE == pratica.ID_DIPENDENTE))
                {
                    db.XR_PRV_DIPENDENTI_DOC.Add(new XR_PRV_DIPENDENTI_DOC()
                    {
                        ID_DIP_DOC = db.XR_PRV_DIPENDENTI_DOC.GeneraPrimaryKey(),
                        ID_DIPENDENTE = pratica.ID_DIPENDENTE,
                        IND_HEADER = true,
                        HEADER_TEXT = matrProt[pratica.MATRICOLA],
                        IND_BODY = false,
                        IND_FOOTER = false,
                        BODY_TEXT = null,
                        FOOTER_TEXT = null,
                        COD_USER = "ADMIN",
                        COD_TERMID = "BATCHSESSION",
                        TMS_TIMESTAMP = DateTime.Now
                    });
                }
                else
                {
                    ;
                }
            }

            db.SaveChanges();

            return Content("OK");
        }

        public ActionResult ShowConsegnaLettera(int idDip)
        {
            return PartialView("~/Views/Gestionale/PoliticheRetributive/subpartial/Overview.cshtml", idDip);
        }

        public ActionResult GetDesLivello(string codCat)
        {
            IncentiviEntities db = new IncentiviEntities();
            V_HRDW_L2D_CATEGORIA cat = db.V_HRDW_L2D_CATEGORIA.FirstOrDefault(x => x.cod_categoria == codCat);

            bool found = cat != null;
            string desLivello = cat != null ? cat.desc_liv_tipo_categoria.Trim() : "";

            return new JsonResult() { JsonRequestBehavior = JsonRequestBehavior.AllowGet, Data = new { found = found, desLivello = desLivello } };
        }

        #region AGGIORNA CONTEGGIO DIPENDENTI IN DIREZIONE
        [UtenteAutorizzatoAttribute]
        public ActionResult AggiornaDipendentiInDirezione()
        {
            List<myRaiCommonModel.Gestionale.HRDW.HRDWDirezione> organico = null;
            myRaiCommonModel.Gestionale.HRDW.HRDWData.getOrganico(null, out organico);

            Expression<Func<XR_PRV_AREA, bool>> funcFilterArea = PoliticheRetributiveHelper.FuncFilterArea();

            Budget budget = new Budget();
            budget.abilDisponibili = new List<Tuple<string, string>>();
            if (PoliticheRetributiveHelper.EnableGest(PolRetrChiaveEnum.BudgetRS))
                budget.abilDisponibili.Add(new Tuple<string, string>("BDGRS", "Risorse Chiave"));
            if (PoliticheRetributiveHelper.EnableGest(PolRetrChiaveEnum.BudgetQIO))
                budget.abilDisponibili.Add(new Tuple<string, string>("BDGQIO", "Non Risorse Chiave"));

            //Serve a gestire casi come Bolzano (64), che deve rientrare sotto il CSR (24)
            PolRetrParam param = HrisHelper.GetParametroJson<PolRetrParam>(HrisParam.PoliticheParametri);
            List<PolRetrOvverideDir> ovverideRule = param != null && param.OvverideDir != null ? param.OvverideDir : new List<PolRetrOvverideDir>();
            StringBuilder myStringBuilder = new StringBuilder();
            using (IncentiviEntities db = new IncentiviEntities())
            {
                var campagne = db.XR_PRV_CAMPAGNA.Where(w => w.NOME.Contains("2022")).ToList();
                foreach (var campagna in campagne)
                {
                    foreach (var dbArea in db.XR_PRV_AREA.Where(funcFilterArea))
                    {
                        var Direzioni = dbArea.XR_PRV_DIREZIONE.Where(x => x.VALID_DTA_END == null && !ovverideRule.Any(y => y.DirOrig == x.CODICE)).OrderBy(x => x.ORDINE).ToList();
                        if (Direzioni != null && Direzioni.Any())
                        {
                            foreach (var d in Direzioni)
                            {
                                int id = d.ID_DIREZIONE;
                                int idCampagna = campagna.ID_CAMPAGNA;
                                string codice = d.CODICE;

                                var item = db.XR_PRV_CAMPAGNA_DIREZIONE.Where(w => w.ID_CAMPAGNA == idCampagna &&
                                w.ID_DIREZIONE == id).FirstOrDefault();

                                if (item != null)
                                {
                                    string tx = String.Format("{0},{1},{2},{3}", item.ID_CAMPAGNA_DIR, item.ORGANICO, item.ORGANICO_M, item.ORGANICO_F);
                                    myStringBuilder.Append(tx);
                                    item.ORGANICO = organico.Any(a => a.Codice == codice) ? organico.FirstOrDefault(a => a.Codice == codice).Organico : 0;
                                    item.ORGANICO_M = organico.Any(a => a.Codice == codice) ? organico.FirstOrDefault(a => a.Codice == codice).OrganicoMaschile : 0;
                                    item.ORGANICO_F = organico.Any(a => a.Codice == codice) ? organico.FirstOrDefault(a => a.Codice == codice).OrganicoFemminile : 0;

                                    item.ORGANICO_AD = organico.Any(a => a.Codice == codice) ? organico.FirstOrDefault(a => a.Codice == codice).Organico : 0;
                                    item.ORGANICO_M_AD = organico.Any(a => a.Codice == codice) ? organico.FirstOrDefault(a => a.Codice == codice).OrganicoMaschile : 0;
                                    item.ORGANICO_F_AD = organico.Any(a => a.Codice == codice) ? organico.FirstOrDefault(a => a.Codice == codice).OrganicoFemminile : 0;

                                    tx = String.Format(",{0},{1},{2}\r\n", item.ORGANICO, item.ORGANICO_M, item.ORGANICO_F);
                                    myStringBuilder.Append(tx);
                                }
                            }
                        }
                    }
                }
                db.SaveChanges();
            }

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"c:\\RAI\testAggiornamento.txt"))
            {
                file.WriteLine(myStringBuilder.ToString());
            }

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = true }
            };
        }

        public class AggiornaOrganicoInteressatoClass
        {
            public string CodiceDirezione { get; set; }
            public int Dipendenti { get; set; }
        }

        [UtenteAutorizzatoAttribute]
        public ActionResult AggiornaOrganicoInteressato()
        {
            try
            {
                List<myRaiCommonModel.Gestionale.HRDW.HRDWDirezione> organico = null;
                myRaiCommonModel.Gestionale.HRDW.HRDWData.getOrganico(null, out organico);

                Expression<Func<XR_PRV_AREA, bool>> funcFilterArea = PoliticheRetributiveHelper.FuncFilterArea();

                Budget budget = new Budget();
                budget.abilDisponibili = new List<Tuple<string, string>>();
                if (PoliticheRetributiveHelper.EnableGest(PolRetrChiaveEnum.BudgetRS))
                    budget.abilDisponibili.Add(new Tuple<string, string>("BDGRS", "Risorse Chiave"));
                if (PoliticheRetributiveHelper.EnableGest(PolRetrChiaveEnum.BudgetQIO))
                    budget.abilDisponibili.Add(new Tuple<string, string>("BDGQIO", "Non Risorse Chiave"));

                //Serve a gestire casi come Bolzano (64), che deve rientrare sotto il CSR (24)
                PolRetrParam param = HrisHelper.GetParametroJson<PolRetrParam>(HrisParam.PoliticheParametri);
                List<PolRetrOvverideDir> ovverideRule = param != null && param.OvverideDir != null ? param.OvverideDir : new List<PolRetrOvverideDir>();

                List<string> valoriInput = new List<string>();
                valoriInput.Add("5J,35");
                valoriInput.Add("5Y,63");
                valoriInput.Add("04,34");
                valoriInput.Add("08,10");
                valoriInput.Add("19,18");
                valoriInput.Add("1A,4");
                valoriInput.Add("42,13");
                valoriInput.Add("49,3");
                valoriInput.Add("50,20");
                valoriInput.Add("58,108");
                valoriInput.Add("5D,24");
                valoriInput.Add("5E,4");
                valoriInput.Add("5V,155");
                valoriInput.Add("5Z,21");
                valoriInput.Add("80,22");
                valoriInput.Add("09,183");
                valoriInput.Add("10,42");
                valoriInput.Add("12,57");
                valoriInput.Add("20,81");
                valoriInput.Add("24,735");
                valoriInput.Add("26,27");
                valoriInput.Add("27,111");
                valoriInput.Add("28,9");
                valoriInput.Add("30,43");
                valoriInput.Add("32,16");
                valoriInput.Add("45,132");
                valoriInput.Add("47,10");
                valoriInput.Add("48,36");
                valoriInput.Add("53,106");
                valoriInput.Add("54,126");
                valoriInput.Add("57,296");
                valoriInput.Add("5C,3");
                valoriInput.Add("5F,114");
                valoriInput.Add("5G,80");
                valoriInput.Add("64,120");
                valoriInput.Add("72,85");
                valoriInput.Add("16,51");
                valoriInput.Add("21,60");
                valoriInput.Add("22,50");
                valoriInput.Add("23,50");
                valoriInput.Add("36,27");
                valoriInput.Add("38,174");
                valoriInput.Add("43,78");
                valoriInput.Add("59,2");
                valoriInput.Add("01,2");
                valoriInput.Add("02,1");
                valoriInput.Add("15,50");
                valoriInput.Add("33,6");
                valoriInput.Add("52,86");
                valoriInput.Add("5L,143");
                valoriInput.Add("5M,317");
                valoriInput.Add("5N,404");
                valoriInput.Add("5P,228");
                valoriInput.Add("5Q,3");
                valoriInput.Add("5R,40");
                valoriInput.Add("5S,28");
                valoriInput.Add("5T,59");
                valoriInput.Add("61,1");
                valoriInput.Add("81,19");
                valoriInput.Add("14,303");
                valoriInput.Add("31,70");
                valoriInput.Add("39,61");
                valoriInput.Add("40,23");
                valoriInput.Add("44,73");
                valoriInput.Add("41,611");
                valoriInput.Add("71,597");
                valoriInput.Add("73,367");
                valoriInput.Add("79,1580");
                valoriInput.Add("82,299");
                valoriInput.Add("92,2");
                valoriInput.Add("96,1");
                List<AggiornaOrganicoInteressatoClass> listaOggettiAggiornamento = new List<AggiornaOrganicoInteressatoClass>();

                foreach (var i in valoriInput)
                {
                    List<string> _tempSplit = i.Split(',').ToList();
                    int dipCount = 0;
                    bool converti = int.TryParse(_tempSplit[1], out dipCount);
                    if (converti)
                    {
                        listaOggettiAggiornamento.Add(new AggiornaOrganicoInteressatoClass()
                        {
                            CodiceDirezione = _tempSplit[0],
                            Dipendenti = dipCount
                        });
                    }
                    else
                    {
                        throw new Exception("Errore in conversione numero dipendenti");
                    }
                }

                StringBuilder myStringBuilder = new StringBuilder();
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    var campagne = db.XR_PRV_CAMPAGNA.Where(w => w.NOME.Contains("2022")).ToList();
                    foreach (var campagna in campagne)
                    {
                        foreach (var dbArea in db.XR_PRV_AREA.Where(funcFilterArea))
                        {
                            var Direzioni = dbArea.XR_PRV_DIREZIONE.Where(x => x.VALID_DTA_END == null && !ovverideRule.Any(y => y.DirOrig == x.CODICE)).OrderBy(x => x.ORDINE).ToList();
                            if (Direzioni != null && Direzioni.Any())
                            {
                                foreach (var d in listaOggettiAggiornamento)
                                {
                                    var toUpdate = Direzioni.Where(w => w.CODICE == d.CodiceDirezione).FirstOrDefault();
                                    if (toUpdate != null)
                                    {
                                        int id = toUpdate.ID_DIREZIONE;
                                        int idCampagna = campagna.ID_CAMPAGNA;
                                        string codice = toUpdate.CODICE;

                                        var item = db.XR_PRV_CAMPAGNA_DIREZIONE.Where(w => w.ID_CAMPAGNA == idCampagna &&
                                        w.ID_DIREZIONE == id).FirstOrDefault();

                                        if (item != null)
                                        {
                                            string tx = String.Format("{0},{1}", item.ID_CAMPAGNA_DIR, item.ORGANICO_INTERESSATO.GetValueOrDefault());
                                            myStringBuilder.Append(tx);
                                            item.ORGANICO_INTERESSATO = d.Dipendenti;
                                            tx = String.Format(",{0}\r\n", item.ORGANICO_INTERESSATO);
                                            myStringBuilder.Append(tx);
                                        }
                                    }
                                    else
                                    {
                                        string tx = String.Format("Campagna {0}, Area {1}, Direzione con codice {2}, non trovata \r\n", campagna.ID_CAMPAGNA, dbArea.ID_AREA, d.CodiceDirezione);
                                        myStringBuilder.Append(tx);
                                    }
                                }
                            }
                        }
                    }
                    db.SaveChanges();
                }

                using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"c:\\RAI\testAggiornamento.txt"))
                {
                    file.WriteLine(myStringBuilder.ToString());
                }
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = ex.Message }
                };
            }

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = true }
            };
        }

        #endregion
    }
}