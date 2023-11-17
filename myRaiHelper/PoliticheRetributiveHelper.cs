using myRai.DataAccess;
using myRaiData.Incentivi;
using myRaiServiceHub.Autorizzazioni;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Helpers;

namespace myRaiHelper
{
    public class PolRetrOvverideDir
    {
        public string DirOrig { get; set; }
        public string DirDest { get; set; }
    }
    public class ReportResult
    {
        public string Prov { get; set; }
        public string Provv { get; set; }
        public string Area { get; set; }
        public XR_PRV_DIPENDENTI Pratica { get; set; }
    }
    public class PolRetrOvverideDirReq
    {
        public string Servizio { get; set; }
        public string ServizioOper { get; set; }
        public string Struttura { get; set; }
        public string DirDest { get; set; }

        public bool Corrisponde(string serv, string str)
        {
            bool result = false;

            bool hasfServ = false;
            bool fServ = false;
            bool hasfStru = false;
            bool fStru = false;

            if (!String.IsNullOrWhiteSpace(Servizio))
            {
                hasfServ = true;
                switch (ServizioOper)
                {
                    case "NOT_EQUAL":
                        fServ = serv != Servizio;
                        break;
                    default:
                        fServ = serv == Servizio;
                        break;
                }
            }

            if (!String.IsNullOrWhiteSpace(Struttura))
            {
                hasfStru = true;

                if (Struttura.StartsWith("*") && Struttura.EndsWith("*"))
                    fStru = str.Contains(Struttura.Replace("*", ""));
                else if (Struttura.StartsWith("*"))
                    fStru = str.EndsWith(Struttura.Replace("*", ""));
                else if (Struttura.EndsWith("*"))
                    fStru = str.StartsWith(Struttura.Replace("*", ""));
                else if (Struttura.Contains("*"))
                {
                    string[] token = Struttura.Split('*');
                    fStru = str.StartsWith(token[0]) && str.EndsWith(token[1]);
                }
                else
                    fStru = str == Struttura;
            }

            if (hasfServ && hasfStru)
                result = fServ && fStru;
            else
                result = hasfServ && fServ || hasfStru && fStru;

            return result;
        }

        public bool GetSintesiFilter(out Expression<Func<SINTESI1, bool>> funcFilter)
        {
            bool result = false;
            funcFilter = null;

            bool hasFServ = false;
            bool hasFStru = false;
            Expression<Func<SINTESI1, bool>> fServ = null;
            Expression<Func<SINTESI1, bool>> fStru = null;

            if (!String.IsNullOrWhiteSpace(Servizio))
            {
                hasFServ = true;
                switch (ServizioOper)
                {
                    case "NOT_EQUAL":
                        fServ = x => x.COD_SERVIZIO != Servizio;
                        break;
                    default:
                        fServ = x => x.COD_SERVIZIO == Servizio;
                        break;
                }
            }

            if (!String.IsNullOrWhiteSpace(Struttura))
            {
                hasFStru = true;
                string tmpToken = Struttura.Replace("*", "");
                if (Struttura.StartsWith("*") && Struttura.EndsWith("*"))
                    fStru = x => x.COD_UNITAORG.Contains(tmpToken);
                else if (Struttura.StartsWith("*"))
                    fStru = x => x.COD_UNITAORG.EndsWith(tmpToken);
                else if (Struttura.EndsWith("*"))
                    fStru = x => x.COD_UNITAORG.StartsWith(tmpToken);
                else if (Struttura.Contains("*"))
                {
                    string[] token = Struttura.Split('*');
                    string iniToken = token[0];
                    string endToken = token[1];
                    fStru = x => x.COD_UNITAORG.StartsWith(iniToken) && x.COD_UNITAORG.EndsWith(endToken);
                }
                else
                    fStru = x => x.COD_UNITAORG == Struttura;
            }

            if (hasFServ && hasFStru)
                funcFilter = LinqHelper.PutInAndTogether(fServ, fStru);
            else if (hasFServ)
                funcFilter = fServ;
            else if (hasFStru)
                funcFilter = fStru;

            result = hasFServ || hasFStru;

            return result;
        }
    }

    public class PolRetrParam
    {
        public List<string> MatrAdmin { get; set; }
        public List<PolRetrOvverideDir> OvverideDir { get; set; }

        public List<PolRetrOvverideDirReq> OvverideDirReq { get; set; }
    }

    public class PoliticheRetributiveHelper
    {
        public const string SIGLA_PROMOZIONI = "PD";
        public const string SIGLA_AUMENTI = "MD";
        public const string SIGLA_GRATIFICHE = "GD";
        public const string SIGLA_NESSUNO = "";

        private const string POLRETR_HRGA_FUNC = "P_RETRIB";
        private const string POLRETR_SESSION_AUTH = "PolRetrAuth";

        private const string ADM_HRGA_SOTTO_FUNC = "ADM";
        private const string GEST_HRGA_SOTTO_FUNC = "GEST";
        private const string VIS_HRGA_SOTTO_FUNC = "VIS";

        public const string RICHIESTE_HRGA_SOTTO_FUNC = "RICHIESTE";
        private const string RICHIESTE_SESSION_AUTH = "PolRetrRichiesteAuth";

        public const string BUDGET_HRGA_SOTTO_FUNC = "BDGQIO";
        private const string BUDGET_SESSION_AUTH = "PolRetrBudgetAuth";

        public const string LETTERA_HRGA_SOTTO_FUNC = "LETTERE";
        private const string LETTERA_SESSION_AUTH = "PolRetrLettereAuth";

        public const string BUDGETRS_HRGA_SOTTO_FUNC = "BDGRS";

        public const string AMM_HRGA_SOTTO_FUNC = "AMM";
        private const string AMM_SESSION_AUTH = "PolRetrAmmAuth";

        private static string[] unita = { "zero", "uno", "due", "tre", "quattro", "cinque", "sei", "sette", "otto", "nove", "dieci", "undici", "dodici", "tredici", "quattordici", "quindici", "sedici", "diciassette", "diciotto", "diciannove" };
        private static string[] decine = { "", "dieci", "venti", "trenta", "quaranta", "cinquanta", "sessanta", "settanta", "ottanta", "novanta" };

        public static bool EnabledTo(string matricola)
        {
            if (String.IsNullOrWhiteSpace(matricola)) matricola = CommonHelper.GetCurrentUserMatricola();
            bool? auth = null;
            bool? authRich = null;
            bool? authBudget = null;
            bool? authLett = null;
            bool? authAmm = null;

            IncentiviEntities db = new IncentiviEntities();


            try
            {
                auth = SessionHelper.Get(POLRETR_SESSION_AUTH) as bool?;
                if (auth == null)
                {
                    auth = false;
                    authRich = false;
                    authBudget = false;
                    authLett = false;
                    authAmm = false;

                    AbilFunc abilFunc = null;
                    if (AuthHelper.EnabledTo(matricola, POLRETR_HRGA_FUNC, out abilFunc))
                    {
                        //06/08/2021 - Workaround per evitare il caricamento del modello di Cezanne poichè dispendioso
                        List<XR_PRV_ABIL> listAbil = db.Database.SqlQuery<XR_PRV_ABIL>("SELECT * FROM XR_PRV_ABIL").ToList();

                        auth = true;
                        foreach (var subFunc in abilFunc.SubFuncs)
                        {
                            string cod = subFunc.Key;
                            var abil = listAbil.FirstOrDefault(x => x.SOTTO_FUNZIONE == cod);
                            if (abil != null)
                            {
                                switch (abil.FUNZIONE)
                                {
                                    case "*":
                                        authRich = true;
                                        authBudget = true;
                                        break;
                                    case RICHIESTE_HRGA_SOTTO_FUNC:
                                        authRich = true;
                                        break;
                                    case BUDGET_HRGA_SOTTO_FUNC:
                                    case BUDGETRS_HRGA_SOTTO_FUNC:
                                        authBudget = true;
                                        break;
                                    case LETTERA_HRGA_SOTTO_FUNC:
                                        authLett = true;
                                        break;
                                    case AMM_HRGA_SOTTO_FUNC:
                                        authAmm = true;
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }

                        if (!abilFunc.SubFuncs.Any(x => x.Key == "BDGQIO" || x.Key == "BDGRS"))
                            authBudget = false;
                    }

                    if (auth.GetValueOrDefault())
                    {
                        bool saveLog = true;
                        if (CommonHelper.IsProduzione() && !System.Diagnostics.Debugger.IsAttached && CommonHelper.GetCurrentRealUsername() != CommonHelper.GetCurrentUserPMatricola())
                        {
                            PolRetrParam param = HrisHelper.GetParametroJson<PolRetrParam>(HrisParam.PoliticheParametri);
                            if (param.MatrAdmin == null || !param.MatrAdmin.Contains(CommonHelper.GetCurrentRealUsername()))
                            {
                                saveLog = false;

                                auth = false;
                                authRich = false;
                                authBudget = false;
                                authLett = false;
                                authAmm = false;
                            }
                        }

                        if (saveLog)
                        {
                            //06/08/2021 - Workaround per evitare il caricamento del modello di Cezanne poichè dispendioso
                            db.Database.ExecuteSqlCommand("INSERT INTO XR_PRV_LOG VALUES(0, @matr, 0, @message, @tmsTimestamp)",
                                new SqlParameter("matr", CommonHelper.GetCurrentUserMatricola()),
                                new SqlParameter("message", "AUTORIZZAZIONI - " + string.Join(",", abilFunc.SubFuncs.Select(x => x.Key))),
                                new SqlParameter("tmsTimestamp", DateTime.Now));
                            //db.XR_PRV_LOG.Add(new XR_PRV_LOG()
                            //{
                            //    ID_PERSONA = 0,
                            //    MATRICOLA = CommonHelper.GetCurrentUserMatricola(),
                            //    ID_DIPENDENTE = 0,
                            //    MESSAGGIO = "AUTORIZZAZIONI - " + string.Join(",", abilFunc.SubFuncs.Select(x => x.Key)),
                            //    TMS_TIMESTAMP = DateTime.Now
                            //});
                            //db.SaveChanges();
                        }
                    }

                    SessionHelper.Set(POLRETR_SESSION_AUTH, auth);
                    SessionHelper.Set(RICHIESTE_SESSION_AUTH, authRich);
                    SessionHelper.Set(BUDGET_SESSION_AUTH, authBudget);
                    SessionHelper.Set(LETTERA_SESSION_AUTH, authLett);
                    SessionHelper.Set(AMM_SESSION_AUTH, authAmm);
                }
            }
            catch (Exception ex)
            {
                auth = false;
                authRich = false;
                authBudget = false;
                authLett = false;
                authAmm = false;

                SessionHelper.Set(POLRETR_SESSION_AUTH, auth);
                SessionHelper.Set(RICHIESTE_SESSION_AUTH, authRich);
                SessionHelper.Set(BUDGET_SESSION_AUTH, authBudget);
                SessionHelper.Set(LETTERA_SESSION_AUTH, authLett);
                SessionHelper.Set(AMM_SESSION_AUTH, authAmm);
            }

            return auth == true;
        }

        public static bool EnabledToRichieste(string matricola = null)
        {
            if (matricola == null) matricola = CommonHelper.GetCurrentUserMatricola();
            bool? auth = null;
            if (EnabledTo(matricola))
            {
                auth = SessionHelper.Get(RICHIESTE_SESSION_AUTH) as bool?;
            }
            return auth.GetValueOrDefault() == true;
        }

        public static bool EnabledToBudget(string matricola = null)
        {
            if (matricola == null) matricola = CommonHelper.GetCurrentUserMatricola();
            bool? auth = null;
            if (EnabledTo(matricola))
            {
                auth = SessionHelper.Get(BUDGET_SESSION_AUTH) as bool?;
            }
            return auth.GetValueOrDefault() == true;
        }

        public static bool EnabledToLettere(string matricola = null)
        {
            if (matricola == null) matricola = CommonHelper.GetCurrentUserMatricola();
            bool? auth = null;
            if (EnabledTo(matricola))
            {
                auth = SessionHelper.Get(LETTERA_SESSION_AUTH) as bool?;
            }
            return auth.GetValueOrDefault() == true;
        }

        public static bool EnabledToAmm(string matricola = null)
        {
            if (matricola == null) matricola = CommonHelper.GetCurrentUserMatricola();
            bool? auth = null;
            if (EnabledTo(matricola))
            {
                auth = SessionHelper.Get(AMM_SESSION_AUTH) as bool?;
            }
            return auth.GetValueOrDefault() == true;
        }

        public static bool EnableGest(PolRetrChiaveEnum chiave, string matricola = null)
        {
            bool enabled = true;
            if (String.IsNullOrWhiteSpace(matricola)) matricola = CommonHelper.GetCurrentUserMatricola();

            //if (CommonHelper.GetParametro<bool>(EnumParametriSistema.PoliticheAbilitaGestVis))
            {
                AbilFunc abilFunc = null;
                if (AuthHelper.EnabledTo(matricola, POLRETR_HRGA_FUNC, out abilFunc))
                {
                    IncentiviEntities db = new IncentiviEntities();
                    List<string> subFunc = abilFunc.SubFuncs.Select(x => x.Key).ToList();


                    string funcKey = "";
                    switch (chiave)
                    {
                        case PolRetrChiaveEnum.Richieste:
                            funcKey = RICHIESTE_HRGA_SOTTO_FUNC;
                            break;
                        case PolRetrChiaveEnum.BudgetQIO:
                            funcKey = BUDGET_HRGA_SOTTO_FUNC;
                            break;
                        case PolRetrChiaveEnum.BudgetRS:
                            funcKey = BUDGETRS_HRGA_SOTTO_FUNC;
                            break;
                    }

                    enabled = EnabledToRichieste()
                        && db.XR_PRV_ABIL.Any(x => subFunc.Contains(x.SOTTO_FUNZIONE) && x.FUNZIONE == funcKey && x.FLAG_GEST);
                }
            }

            return enabled;
        }
        public static bool EnableVis(PolRetrChiaveEnum chiave, string matricola = null)
        {
            bool enabled = true;
            if (String.IsNullOrWhiteSpace(matricola)) matricola = CommonHelper.GetCurrentUserMatricola();

            //if (CommonHelper.GetParametro<bool>(EnumParametriSistema.PoliticheAbilitaGestVis))
            {
                AbilFunc abilFunc = null;
                if (AuthHelper.EnabledTo(matricola, POLRETR_HRGA_FUNC, out abilFunc))
                {
                    IncentiviEntities db = new IncentiviEntities();
                    List<string> subFunc = abilFunc.SubFuncs.Select(x => x.Key).ToList();


                    string funcKey = "";
                    switch (chiave)
                    {
                        case PolRetrChiaveEnum.Richieste:
                            funcKey = RICHIESTE_HRGA_SOTTO_FUNC;
                            break;
                        case PolRetrChiaveEnum.BudgetQIO:
                            funcKey = BUDGET_HRGA_SOTTO_FUNC;
                            break;
                        case PolRetrChiaveEnum.BudgetRS:
                            funcKey = BUDGETRS_HRGA_SOTTO_FUNC;
                            break;
                    }

                    enabled = EnabledToRichieste()
                        && db.XR_PRV_ABIL.Any(x => subFunc.Contains(x.SOTTO_FUNZIONE) && x.FUNZIONE == funcKey);
                }
            }

            return enabled;
        }

        public static bool EnableGestCampagna(string chiave, string matricola = null)
        {
            switch (chiave)
            {
                case BUDGET_HRGA_SOTTO_FUNC:
                    return EnableGest(PolRetrChiaveEnum.BudgetQIO, matricola);
                case BUDGETRS_HRGA_SOTTO_FUNC:
                    return EnableGest(PolRetrChiaveEnum.BudgetRS, matricola);
                default:
                    return true;
            }
        }

        public static List<string> EnableSubFunction(string matricola = null)
        {
            if (String.IsNullOrWhiteSpace(matricola)) matricola = CommonHelper.GetCurrentUserMatricola();
            return AuthHelper.EnabledSubFunc(matricola, POLRETR_HRGA_FUNC);
        }

        #region GestioneAbilitazione

        delegate bool ScopeChecker();
        delegate void GetAbilScope(out bool applyFilters, out List<int> enabledStates);
        delegate void SetAbilScope(bool applyFilters, List<int> enabledStates);

        public static bool GetEnabledCategory(IncentiviEntities db, string matricola, out List<string> categoryIncluded, out List<string> categoryExcluded)
        {
            bool applyFilters = false;
            categoryIncluded = new List<string>();
            categoryExcluded = new List<string>();

            if (!AuthHelper.EnabledToSubFunc(matricola, POLRETR_HRGA_FUNC, "ADM"))
            {
                var abilCat = AuthHelper.EnabledCategory(matricola, POLRETR_HRGA_FUNC);
                applyFilters = abilCat.HasFilter;
                categoryIncluded = abilCat.CategorieIncluse;
                categoryExcluded = abilCat.CategorieEscluse;
            }

            return applyFilters;
        }

        public static List<XR_PRV_AREA> GetEnabledAreas(IncentiviEntities db, string matricola)
        {
            if (db == null)
                db = new IncentiviEntities();

            List<int> enabledDir = new List<int>();
            bool risp = GetEnabledDirezioni(db, matricola, out enabledDir);

            var areas = (from dir in db.XR_PRV_DIREZIONE
                         join area in db.XR_PRV_AREA
                         on dir.ID_AREA equals area.ID_AREA
                         where !risp || enabledDir.Contains(dir.ID_DIREZIONE)
                         select area).Distinct().ToList();

            return areas;
        }
        public static bool GetEnabledDirezioniSoloRichieste(IncentiviEntities db, string matricola, out List<int> enabledDir)
        {
            bool applyFilters = false;
            enabledDir = new List<int>();

            if (db == null)
            {
                db = new IncentiviEntities();
            }

            if (!AuthHelper.EnabledToSubFunc(matricola, POLRETR_HRGA_FUNC, "ADM"))
            {
                var listAbilDir = AuthHelper.EnabledDirectionBySub(matricola, POLRETR_HRGA_FUNC);
                if (listAbilDir != null && listAbilDir.Any())
                {
                    applyFilters = true;
                    List<string> direz = listAbilDir.Where(x => x != null && (x.Sottofunzione == "RICHIESTE" || x.Sottofunzione=="VIS_LET")).Select(x => x.DirezioniIncluse).FirstOrDefault();
                    if (direz != null && direz.Any())
                    {
                        foreach (string dir in direz)
                        {
                            enabledDir.AddRange(db.XR_PRV_DIREZIONE.Where(x => x.CODICE == dir).Select(x => x.ID_DIREZIONE).ToList());
                        }
                    }
                    var dir20 = db.XR_PRV_DIREZIONE.Where(x => x.CODICE == "20").ToList();
                    foreach (var d in dir20)
                    {
                        if (enabledDir.Contains(d.ID_DIREZIONE))
                        {
                            var dir20b = db.XR_PRV_DIREZIONE.Where(x => x.CODICE == "20_B" && x.ID_AREA==d.ID_AREA).FirstOrDefault();
                            if (dir20b != null)
                                enabledDir.Add(dir20b.ID_DIREZIONE);
                        }
                    }


                    var b =enabledDir.Contains(360012957);
                    var c = enabledDir.Contains(674207969);
                    return applyFilters;


                    var elencoDirDB = db.XR_PRV_DIREZIONE.Include("XR_PRV_AREA").ToList();
                    var elencoAbil = db.XR_PRV_ABIL.ToList();
                    foreach (var abilDir in listAbilDir.Where(x => x != null))
                    {
                        var dbAbil = elencoAbil.FirstOrDefault(x => x.SOTTO_FUNZIONE == abilDir.Sottofunzione);
                        if (abilDir.HasFilter)
                        {
                            enabledDir.AddRange(elencoDirDB.Where(x => x.XR_PRV_AREA.LV_ABIL.Contains(dbAbil.FUNZIONE) && abilDir.DirezioniIncluse.Contains(x.CODICE)).Select(x => x.ID_DIREZIONE));

                            //se è presente l'abilitazione al servizio CANONE
                            //bisogna aggiungere l'abilitazione al servizio fittizio 20_B - Canone nelle sedi
                            if (abilDir.DirezioniIncluse.Contains("20"))
                                enabledDir.AddRange(elencoDirDB.Where(x => x.XR_PRV_AREA.LV_ABIL.Contains(dbAbil.FUNZIONE) && x.CODICE == "20_B").Select(x => x.ID_DIREZIONE));
                        }
                        else
                        {
                            enabledDir.AddRange(elencoDirDB.Where(x => x.XR_PRV_AREA.LV_ABIL.Contains(dbAbil.FUNZIONE)).Select(y => y.ID_DIREZIONE));
                        }
                    }

                    enabledDir = enabledDir.Distinct().ToList();
                }

            }

            return applyFilters;
        }
        public static bool GetEnabledDirezioni(IncentiviEntities db, string matricola, out List<int> enabledDir)
        {
            bool applyFilters = false;
            enabledDir = new List<int>();

            if (db == null)
            {
                db = new IncentiviEntities();
            }

            if (!AuthHelper.EnabledToSubFunc(matricola, POLRETR_HRGA_FUNC, "ADM"))
            {
                var listAbilDir = AuthHelper.EnabledDirectionBySub(matricola, POLRETR_HRGA_FUNC);
                if (listAbilDir != null && listAbilDir.Any())
                {
                    applyFilters = true;

                    var elencoDirDB = db.XR_PRV_DIREZIONE.Include("XR_PRV_AREA").ToList();
                    var elencoAbil = db.XR_PRV_ABIL.ToList();
                    foreach (var abilDir in listAbilDir.Where(x => x != null))
                    {
                        var dbAbil = elencoAbil.FirstOrDefault(x => x.SOTTO_FUNZIONE == abilDir.Sottofunzione);
                        if (abilDir.HasFilter)
                        {
                            enabledDir.AddRange(elencoDirDB.Where(x => x.XR_PRV_AREA.LV_ABIL.Contains(dbAbil.FUNZIONE) && abilDir.DirezioniIncluse.Contains(x.CODICE)).Select(x => x.ID_DIREZIONE));

                            //se è presente l'abilitazione al servizio CANONE
                            //bisogna aggiungere l'abilitazione al servizio fittizio 20_B - Canone nelle sedi
                            if (abilDir.DirezioniIncluse.Contains("20"))
                                enabledDir.AddRange(elencoDirDB.Where(x => x.XR_PRV_AREA.LV_ABIL.Contains(dbAbil.FUNZIONE) && x.CODICE == "20_B").Select(x => x.ID_DIREZIONE));
                        }
                        else
                        {
                            enabledDir.AddRange(elencoDirDB.Where(x => x.XR_PRV_AREA.LV_ABIL.Contains(dbAbil.FUNZIONE)).Select(y => y.ID_DIREZIONE));
                        }
                    }

                    enabledDir = enabledDir.Distinct().ToList();
                }

            }

            return applyFilters;
        }


        #region FunzioniFiltro
        public static bool CanSeeProvvData(IncentiviEntities db, string catPrevista)
        {
            if (db == null)
                db = new IncentiviEntities();

            List<string> categoryIncluded = null;
            List<string> categoryExcluded = null;
            bool applyFilter = GetEnabledCategory(db, CommonHelper.GetCurrentUserMatricola(), out categoryIncluded, out categoryExcluded);

            return !applyFilter || categoryIncluded.Any(a => catPrevista.StartsWith(a))
                && !categoryExcluded.Any(b => catPrevista.StartsWith(b));
        }
        public static Expression<Func<XR_PRV_DIPENDENTI, bool>> FuncFilterMatr(IncentiviEntities db)
        {
            List<string> categoryIncluded = null;
            List<string> categoryExcluded = null;
            bool applyFilter = GetEnabledCategory(db, CommonHelper.GetCurrentUserMatricola(), out categoryIncluded, out categoryExcluded);

            Expression<Func<XR_PRV_DIPENDENTI, bool>> funcFilterFigPro;

            if (!applyFilter)
                funcFilterFigPro = x => true;
            else
            {
                funcFilterFigPro = x =>
                categoryIncluded.Any(a => x.SINTESI1.COD_QUALIFICA.StartsWith(a))
                && !categoryExcluded.Any(b => x.SINTESI1.COD_QUALIFICA.StartsWith(b));
            }

            return funcFilterFigPro;
        }
        public static Expression<Func<XR_PRV_DIPENDENTI, bool>> FuncFilterDirezione(IncentiviEntities db, bool IsFromRichieste = false)
        {
            List<int> enabledDir = null;
            bool applyFilterStates =
                (IsFromRichieste ? GetEnabledDirezioniSoloRichieste(db, CommonHelper.GetCurrentUserMatricola(), out enabledDir)
                                 : GetEnabledDirezioni(db, CommonHelper.GetCurrentUserMatricola(), out enabledDir));
            Expression<Func<XR_PRV_DIPENDENTI, bool>> funcFilterAbil = null;
            if (applyFilterStates)
                funcFilterAbil = x => enabledDir.Contains(x.ID_DIREZIONE);
            else
                funcFilterAbil = x => true;

            //x => !applyFilterStates || 

            return funcFilterAbil;
        }
        public static Expression<Func<XR_PRV_DIPENDENTI, bool>> FuncFilterAreaPratica(bool IsFromRichieste = false)
        {
            Expression<Func<XR_PRV_DIPENDENTI, bool>> funcFilterArea = null;
            bool enableQIO = EnableVis(PolRetrChiaveEnum.BudgetQIO);
            bool enableRS = EnableVis(PolRetrChiaveEnum.BudgetRS);
            bool enableRichieste = EnableVis(PolRetrChiaveEnum.Richieste);

            bool enableLett = EnabledToLettere();
            bool enableAmm = EnabledToAmm();

            //if (IsFromRichieste && enableLett)
            //{
            //    funcFilterArea = x => x.XR_PRV_OPERSTATI.Select(z => z.ID_STATO).Max() == 2;
            //    return funcFilterArea;
            //}
            if (IsFromRichieste && enableRichieste)
            {
                if (enableRS)
                    funcFilterArea = x => true;
                else
                    funcFilterArea = x =>  ! x.XR_PRV_DIREZIONE.XR_PRV_AREA.LV_ABIL.Contains(BUDGETRS_HRGA_SOTTO_FUNC);
                return funcFilterArea;
            }

            if (enableRS && enableQIO)
            {
                funcFilterArea = x => true;
            }
            else if (enableRS)
            {
                funcFilterArea = x => x.XR_PRV_DIREZIONE.XR_PRV_AREA.LV_ABIL == null || x.XR_PRV_DIREZIONE.XR_PRV_AREA.LV_ABIL.Contains(BUDGETRS_HRGA_SOTTO_FUNC);
            }
            else if (enableQIO)
            {
                funcFilterArea = x => x.XR_PRV_DIREZIONE.XR_PRV_AREA.LV_ABIL == null || x.XR_PRV_DIREZIONE.XR_PRV_AREA.LV_ABIL.Contains(BUDGET_HRGA_SOTTO_FUNC);
            }
            else if (enableLett)
            {
                funcFilterArea = x => x.XR_PRV_DIREZIONE.XR_PRV_AREA.LV_ABIL == null || x.XR_PRV_DIREZIONE.XR_PRV_AREA.LV_ABIL.Contains(LETTERA_HRGA_SOTTO_FUNC);
            }
            else if (enableAmm)
            {
                funcFilterArea = x => x.XR_PRV_DIREZIONE.XR_PRV_AREA.LV_ABIL == null || x.XR_PRV_DIREZIONE.XR_PRV_AREA.LV_ABIL.Contains(AMM_HRGA_SOTTO_FUNC);
            }
            else if (enableRichieste)
            {
                funcFilterArea = x => x.XR_PRV_DIREZIONE.XR_PRV_AREA.LV_ABIL == null || x.XR_PRV_DIREZIONE.XR_PRV_AREA.LV_ABIL.Contains(RICHIESTE_HRGA_SOTTO_FUNC);
            }
            else
            {
                funcFilterArea = x => false;
            }

            return funcFilterArea;
        }
        public static Expression<Func<SINTESI1, bool>> FuncFilterMatrSintesi(IncentiviEntities db)
        {
            List<string> categoryIncluded = null;
            List<string> categoryExcluded = null;
            bool applyFilter = GetEnabledCategory(db, CommonHelper.GetCurrentUserMatricola(), out categoryIncluded, out categoryExcluded);

            Expression<Func<SINTESI1, bool>> funcFilterFigPro = null;

            if (applyFilter)
                funcFilterFigPro = x => (categoryIncluded.Any(a => x.COD_QUALIFICA.StartsWith(a)) && !categoryExcluded.Any(b => x.COD_QUALIFICA.StartsWith(b)));
            else
                funcFilterFigPro = x => true;

            return funcFilterFigPro;
        }
        public static Expression<Func<SINTESI1, bool>> FuncFilterDirezioneSintesi(IncentiviEntities db)
        {
            List<int> enableDir = null;
            bool applyFilterStates = GetEnabledDirezioni(db, CommonHelper.GetCurrentUserMatricola(), out enableDir);


            if (!CommonHelper.IsProduzione())
                applyFilterStates = false;

            Expression<Func<SINTESI1, bool>> funcFilterAbil = null;

            var elencoDBDir = db.XR_PRV_DIREZIONE.Where(x => enableDir.Contains(x.ID_DIREZIONE)).Select(y => y.CODICE).Distinct();

            if (applyFilterStates)
                funcFilterAbil = x => elencoDBDir.Contains(x.COD_SERVIZIO.Substring(0, 2));
            else
                funcFilterAbil = x => true;

            if (applyFilterStates)
            {
                var param = HrisHelper.GetParametroJson<PolRetrParam>(HrisParam.PoliticheParametri);
                if (param != null && param.OvverideDirReq != null && param.OvverideDirReq.Any())
                {
                    foreach (var rule in param.OvverideDirReq)
                    {
                        if (elencoDBDir.Contains(rule.DirDest) && rule.GetSintesiFilter(out Expression<Func<SINTESI1, bool>> filter))
                            funcFilterAbil = LinqHelper.PutInOrTogether(funcFilterAbil, filter);
                    }
                }
            }

            return funcFilterAbil;
        }
        public static Expression<Func<XR_PRV_DIREZIONE, bool>> FuncFilterDir(IncentiviEntities db)
        {
            List<int> enableDir = null;
            bool applyFilterStates = GetEnabledDirezioni(db, CommonHelper.GetCurrentUserMatricola(), out enableDir);

            Expression<Func<XR_PRV_DIREZIONE, bool>> funcFilterAbil = null;

            if (applyFilterStates)
                funcFilterAbil = x => enableDir.Contains(x.ID_DIREZIONE);
            else
                funcFilterAbil = x => !applyFilterStates;

            return funcFilterAbil;
        }

        public static Expression<Func<XR_PRV_CAMPAGNA, bool>> FuncFilterCampagna()
        {
            Expression<Func<XR_PRV_CAMPAGNA, bool>> funcFilterCampagna = null;

            bool enableQIO = EnableVis(PolRetrChiaveEnum.BudgetQIO);
            bool enableRS = EnableVis(PolRetrChiaveEnum.BudgetRS);

            bool enableAmm = EnabledToAmm();

            if (enableRS && enableQIO)
            {
                funcFilterCampagna = x => true;
            }
            else if (enableRS)
            {
                funcFilterCampagna = x => x.LV_ABIL == null || x.LV_ABIL.Contains(BUDGETRS_HRGA_SOTTO_FUNC);
            }
            else if (enableQIO)
            {
                funcFilterCampagna = x => x.LV_ABIL == null || x.LV_ABIL.Contains(BUDGET_HRGA_SOTTO_FUNC);
            }
            else if (enableAmm)
            {
                //funcFilterCampagna = x => x.LV_ABIL == null || x.LV_ABIL.Contains(AMM_HRGA_SOTTO_FUNC);
                funcFilterCampagna = x => true;
            }
            else
            {
                funcFilterCampagna = x => false;
            }

            return funcFilterCampagna;
        }
        public static Expression<Func<XR_PRV_AREA, bool>> FuncFilterArea()
        {
            Expression<Func<XR_PRV_AREA, bool>> funcFilterArea = null;

            bool enableQIO = EnableVis(PolRetrChiaveEnum.BudgetQIO);
            bool enableRS = EnableVis(PolRetrChiaveEnum.BudgetRS);

            bool enableAmm = EnabledToAmm();

            if (enableRS && enableQIO)
            {
                funcFilterArea = x => true;
            }
            else if (enableRS)
            {
                funcFilterArea = x => x.LV_ABIL == null || x.LV_ABIL.Contains(BUDGETRS_HRGA_SOTTO_FUNC);
            }
            else if (enableQIO)
            {
                funcFilterArea = x => x.LV_ABIL == null || x.LV_ABIL.Contains(BUDGET_HRGA_SOTTO_FUNC);
            }
            else if (enableAmm)
            {
                funcFilterArea = x => x.LV_ABIL == null || x.LV_ABIL.Contains(AMM_HRGA_SOTTO_FUNC);
            }
            else
            {
                funcFilterArea = x => false;
            }

            return funcFilterArea;
        }

        public static Expression<Func<XR_PRV_DIPENDENTI, bool>> FuncFilterAbil()
        {
            Expression<Func<XR_PRV_DIPENDENTI, bool>> funcFilterAbil = null;

            if (EnabledToAmm())
            {
                funcFilterAbil = x => x.XR_PRV_OPERSTATI.Where(y => y.DATA_FINE_VALIDITA == null || y.DATA_FINE_VALIDITA > DateTime.Today).Max(y => y.ID_STATO) >= (int)ProvvStatoEnum.Consegnato && x.STATO_LETTERA == 1;
            }
            else
            {
                funcFilterAbil = x => true;
            }

            return funcFilterAbil;
        }

        public static Expression<Func<XR_PRV_DIREZIONE, bool>> FuncFilterDirByArea()
        {
            Expression<Func<XR_PRV_DIREZIONE, bool>> funcFilterDirByArea = null;

            bool enableQIO = EnableVis(PolRetrChiaveEnum.BudgetQIO);
            bool enableRS = EnableVis(PolRetrChiaveEnum.BudgetRS);

            if (enableRS && enableQIO)
            {
                funcFilterDirByArea = x => true;
            }
            else if (enableRS)
            {
                funcFilterDirByArea = x => x.XR_PRV_AREA.LV_ABIL == null || x.XR_PRV_AREA.LV_ABIL.Contains(BUDGETRS_HRGA_SOTTO_FUNC);
            }
            else if (enableQIO)
            {
                funcFilterDirByArea = x => x.XR_PRV_AREA.LV_ABIL == null || x.XR_PRV_AREA.LV_ABIL.Contains(BUDGET_HRGA_SOTTO_FUNC);
            }
            else
            {
                funcFilterDirByArea = x => false;
            }

            return funcFilterDirByArea;
        }
        public static Expression<Func<XR_PRV_DIPENDENTI, bool>> FuncFilterConsolidated()
        {
            //return x=> true;
            return x => x.XR_PRV_OPERSTATI.Where(y => y.DATA_FINE_VALIDITA == null || y.DATA_FINE_VALIDITA > DateTime.Now).Max(y => y.ID_STATO) == (int)ProvvStatoEnum.Convalidato;
        }
        #endregion
        #endregion

        #region GestioneWidget
        public static List<string> MatricoleDoppieRichieste()
        {
            List<string> elencoMatricole = new List<string>();

            using (IncentiviEntities db = new IncentiviEntities())
            {
                Expression<Func<XR_PRV_DIPENDENTI, bool>> funcFilterAbilServizio = PoliticheRetributiveHelper.FuncFilterDirezione(db);
                Expression<Func<XR_PRV_DIPENDENTI, bool>> funcFilterAbilMatr = PoliticheRetributiveHelper.FuncFilterMatr(db);
                Expression<Func<XR_PRV_DIPENDENTI, bool>> funcFilterAreaPratica = PoliticheRetributiveHelper.FuncFilterAreaPratica();


                var tmp = db.XR_PRV_DIPENDENTI.Include("SINTESI1")
                            .Where(funcFilterAreaPratica)
                            .Where(funcFilterAbilServizio)
                            .Where(funcFilterAbilMatr)
                            .Where(x => x.ID_CAMPAGNA > 2 && (x.XR_PRV_CAMPAGNA.DTA_FINE == null || x.XR_PRV_CAMPAGNA.DTA_FINE.Value > DateTime.Today))
                            .ToList()
                            .GroupBy(x => x.ID_PERSONA)
                            .Where(y => y.Count() > 1);

                if (tmp.Count() > 0)
                {
                    elencoMatricole.AddRange(tmp.Select(z => z.First().MATRICOLA));
                }
            }

            return elencoMatricole;
        }
        public static bool GetHomeWidgetPratiche(out int totPratiche, out int totInCarico)
        {
            bool auth = false;
            totPratiche = 0;
            totInCarico = 0;

            string matricola = CommonHelper.GetCurrentUserMatricola();
            int idPers = CommonHelper.GetCurrentIdPersona();

            if (PoliticheRetributiveHelper.EnabledToRichieste())
            {
                auth = true;
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    Expression<Func<XR_PRV_DIPENDENTI, bool>> funcFilterAbilServizio = FuncFilterDirezione(db);
                    Expression<Func<XR_PRV_DIPENDENTI, bool>> funcFilterAbilMatr = FuncFilterMatr(db);
                    Expression<Func<XR_PRV_DIPENDENTI, bool>> funcFilterAreaPratica = FuncFilterAreaPratica();

                    var tmp = db.XR_PRV_DIPENDENTI.Include("SINTESI1")
                                .Where(funcFilterAreaPratica)
                                .Where(funcFilterAbilServizio)
                                .Where(funcFilterAbilMatr)
                                .Where(x => x.XR_PRV_OPERSTATI.Where(y => y.DATA_FINE_VALIDITA == null || y.DATA_FINE_VALIDITA < DateTime.Now).Max(y => y.ID_STATO) != (int)ProvvStatoEnum.Conclusa);

                    totPratiche = tmp.Count();
                    totInCarico = tmp.Where(x => x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == 0 && y.ID_PERSONA == idPers)).Count();
                }
            }

            return auth;
        }

        #endregion

        public static int SalvaStato(IncentiviEntities db, int idDip, int stato, bool saveChanges = true, int idPersona = 0)
        {
            int result = 0;

            if (db == null)
                db = new IncentiviEntities();
            XR_PRV_OPERSTATI operStato = db.XR_PRV_OPERSTATI.Where(y => !y.DATA_FINE_VALIDITA.HasValue).FirstOrDefault(x => x.ID_DIPENDENTE == idDip && x.ID_STATO == stato);

            bool isNew = false;
            if (operStato == null)
            {
                isNew = true;
                operStato = new XR_PRV_OPERSTATI();
                operStato.ID_OPER = db.XR_PRV_OPERSTATI.GeneraPrimaryKey();
            }
            operStato.ID_DIPENDENTE = idDip;
            if (idPersona == 0)
                operStato.ID_PERSONA = CommonHelper.GetCurrentIdPersona();
            else
                operStato.ID_PERSONA = idPersona;

            operStato.ID_STATO = stato;
            operStato.DATA = DateTime.Now;
            string codUser = "";
            string termid = "";
            DateTime timestamp;
            CezanneHelper.GetCampiFirma(out codUser, out termid, out timestamp);
            operStato.COD_USER = codUser;
            operStato.COD_TERMID = termid;
            operStato.TMS_TIMESTAMP = timestamp;
            if (isNew)
                db.XR_PRV_OPERSTATI.Add(operStato);

            if (!saveChanges || DBHelper.Save(db, "PoliticheRetributive - "))
                result = operStato.ID_OPER;
            else
                result = 0;

            return result;
        }

        #region GestioneTabelle
        /// <summary>
        /// Restituisce la query per l'estrazione delle pratiche.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="filterConsolidated"></param>
        /// <param name="includes">Elenco delle tabelle da includere nel resulset (XR_PRV_DIPENDENTI_VARIAZIONI già inclusa)</param>
        /// <returns></returns>
        public static IQueryable<XR_PRV_DIPENDENTI> GetPratiche(IncentiviEntities db, bool filterConsolidated, params string[] includes)
        {
            Expression<Func<XR_PRV_DIPENDENTI, bool>> funcFilterAbilServizio = PoliticheRetributiveHelper.FuncFilterDirezione(db);
            Expression<Func<XR_PRV_DIPENDENTI, bool>> funcFilterAbilMatr = PoliticheRetributiveHelper.FuncFilterMatr(db);
            Expression<Func<XR_PRV_DIPENDENTI, bool>> funcFilterAreaPratica = PoliticheRetributiveHelper.FuncFilterAreaPratica();
            Expression<Func<XR_PRV_DIPENDENTI, bool>> funcFilterConsolidated = null;
            if (filterConsolidated)
                funcFilterConsolidated = PoliticheRetributiveHelper.FuncFilterConsolidated();

            var dbQuery = db.XR_PRV_DIPENDENTI
                                .Include("XR_PRV_DIPENDENTI_VARIAZIONI");

            if (includes != null && includes.Any())
            {
                foreach (var item in includes)
                    dbQuery = dbQuery.Include(item);
            }

            IQueryable<XR_PRV_DIPENDENTI> pratiche = dbQuery.Where(funcFilterAreaPratica)
                                                           .Where(funcFilterAbilServizio)
                                                           .Where(funcFilterAbilMatr);

            if (filterConsolidated)
                pratiche = pratiche.Where(funcFilterConsolidated);

            return pratiche;
        }
        public static void SetProvEffettivo(IncentiviEntities db, XR_PRV_DIPENDENTI dip, int? idProv, int gruppo = 0, int ipotesi = 0)
        {
            bool usePrvVariazioniFlag = HrisHelper.GetParametro<string>(HrisParam.PRetribPrvSuVariazioni) == "TRUE";
            dip.ID_PROV_EFFETTIVO = idProv;
            if (usePrvVariazioniFlag)
            {
                var listNessunProv = GetProvBySigla(db, SIGLA_NESSUNO);
                foreach (var item in dip.XR_PRV_DIPENDENTI_VARIAZIONI.Where(x => x.IND_PRV_EFFETTIVO.HasValue))
                    item.IND_PRV_EFFETTIVO = null;
                if (idProv != null && !listNessunProv.Contains(idProv.Value))
                    dip.XR_PRV_DIPENDENTI_VARIAZIONI.FirstOrDefault(x => x.ID_PROV == idProv).IND_PRV_EFFETTIVO = true;
            }
        }
        public static XR_PRV_DIPENDENTI_VARIAZIONI GetDipProv(XR_PRV_DIPENDENTI dip, int? idProv, int gruppo = 0, int ipotesi = 0)
        {
            //#@
            XR_PRV_DIPENDENTI_VARIAZIONI result = null;
            if (idProv.HasValue)
                result = dip.XR_PRV_DIPENDENTI_VARIAZIONI.FirstOrDefault(x => x.ID_PROV == idProv && x.ID_GRUPPO == gruppo && x.ID_IPOTESI == ipotesi);
            return result;
        }

        public static XR_PRV_DIPENDENTI_VARIAZIONI GetDipProvEffettivo(int idDip)
        {
            var db = new IncentiviEntities();
            var dip = db.XR_PRV_DIPENDENTI.Find(idDip);
            return GetDipProvEffettivo(dip);
        }
        public static XR_PRV_DIPENDENTI_VARIAZIONI GetDipProvEffettivo(XR_PRV_DIPENDENTI dip)
        {
            bool usePrvVariazioniFlag = HrisHelper.GetParametro<string>(HrisParam.PRetribPrvSuVariazioni) == "TRUE";
            XR_PRV_DIPENDENTI_VARIAZIONI result = null;
            if (!usePrvVariazioniFlag)
                result = dip.XR_PRV_DIPENDENTI_VARIAZIONI.FirstOrDefault(x => x.ID_PROV == dip.ID_PROV_EFFETTIVO);
            else
                result = dip.XR_PRV_DIPENDENTI_VARIAZIONI.FirstOrDefault(x => x.IND_PRV_EFFETTIVO.HasValue && x.IND_PRV_EFFETTIVO.Value);

            return result;
        }
        public static XR_PRV_DIPENDENTI_VARIAZIONI GetDipBaseProv(XR_PRV_DIPENDENTI dip, int? idProv, int gruppo = 0, int ipotesi = 0)
        {
            //#@
            XR_PRV_DIPENDENTI_VARIAZIONI result = null;
            if (idProv.HasValue)
                result = dip.XR_PRV_DIPENDENTI_VARIAZIONI.FirstOrDefault(x => x.XR_PRV_PROV.BASE_PROV == idProv);
            return result;
        }
        public static Expression<Func<XR_PRV_DIPENDENTI, XR_PRV_DIPENDENTI_VARIAZIONI>> GetDipProvEff()
        {
            bool usePrvVariazioniFlag = HrisHelper.GetParametro<string>(HrisParam.PRetribPrvSuVariazioni) == "TRUE";
            Expression<Func<XR_PRV_DIPENDENTI, XR_PRV_DIPENDENTI_VARIAZIONI>> result = null;

            if (!usePrvVariazioniFlag)
                result = x => x.XR_PRV_DIPENDENTI_VARIAZIONI.FirstOrDefault(y => y.ID_PROV == x.ID_PROV_EFFETTIVO);
            else
                result = x => x.XR_PRV_DIPENDENTI_VARIAZIONI.FirstOrDefault(y => y.IND_PRV_EFFETTIVO.HasValue && y.IND_PRV_EFFETTIVO.Value);

            return result;
        }
        public static Expression<Func<XR_PRV_DIPENDENTI, bool>> AnyDipProvEff()
        {
            bool usePrvVariazioniFlag = HrisHelper.GetParametro<string>(HrisParam.PRetribPrvSuVariazioni) == "TRUE";
            Expression<Func<XR_PRV_DIPENDENTI, bool>> result = null;
            if (!usePrvVariazioniFlag)
                result = x => x.ID_PROV_EFFETTIVO != null && x.XR_PRV_DIPENDENTI_VARIAZIONI.Any(y => y.ID_PROV == x.ID_PROV_EFFETTIVO);
            else
                result = x => x.XR_PRV_DIPENDENTI_VARIAZIONI.Any(y => y.IND_PRV_EFFETTIVO.HasValue && y.IND_PRV_EFFETTIVO.Value);

            return result;
        }
        public static Expression<Func<XR_PRV_DIPENDENTI, bool>> NotAnyOfProv(params int[] prov)
        {
            //Con il cambio della gestione del provvedimento effettivo, cercare che non ci sia nessun record di tipo "Nessuno (tipo di provvedimento)
            //è errato, in quanto non viene generato un record su XR_PRV_DIPENDENTI_VARIAZIONI.
            //Di fatto, cercare i record con tipo provvedimento "Nessuno" (vecchia gestione) equivale a cercare che non ci sia nessun record selezionato (nuova gestione)

            bool usePrvVariazioniFlag = HrisHelper.GetParametro<string>(HrisParam.PRetribPrvSuVariazioni) == "TRUE";
            Expression<Func<XR_PRV_DIPENDENTI, bool>> result = null;
            if (!usePrvVariazioniFlag)
            {
                if (prov == null || !prov.Any())
                    prov = GetProvBySigla(null, SIGLA_NESSUNO);

                result = x => x.ID_PROV_EFFETTIVO != null && !prov.Contains(x.ID_PROV_EFFETTIVO.Value);
            }
            else
            {
                if (prov != null && prov.Any())
                    result = x => x.XR_PRV_DIPENDENTI_VARIAZIONI.Any(y => y.IND_PRV_EFFETTIVO.HasValue && y.IND_PRV_EFFETTIVO.Value && !prov.Contains(y.ID_PROV));
                else
                    result = x => !x.XR_PRV_DIPENDENTI_VARIAZIONI.Any(y => y.IND_PRV_EFFETTIVO.HasValue && y.IND_PRV_EFFETTIVO.Value);
            }

            return result;
        }
        public static Expression<Func<XR_PRV_DIPENDENTI, bool>> AnyOfProv(params int[] prov)
        {
            bool usePrvVariazioniFlag = HrisHelper.GetParametro<string>(HrisParam.PRetribPrvSuVariazioni) == "TRUE";
            Expression<Func<XR_PRV_DIPENDENTI, bool>> result = null;

            if (!usePrvVariazioniFlag)
            {
                if (prov != null && prov.Any())
                    result = x => x.ID_PROV_EFFETTIVO != null && prov.Contains(x.ID_PROV_EFFETTIVO.Value);
                else
                    result = x => x.ID_PROV_EFFETTIVO != null;
            }
            else
            {
                if (prov != null && prov.Any())
                    result = x => x.XR_PRV_DIPENDENTI_VARIAZIONI.Any(y => y.IND_PRV_EFFETTIVO.HasValue && y.IND_PRV_EFFETTIVO.Value && prov.Contains(y.ID_PROV));
                else
                    result = x => x.XR_PRV_DIPENDENTI_VARIAZIONI.Any(y => y.IND_PRV_EFFETTIVO.HasValue && y.IND_PRV_EFFETTIVO.Value);
            }

            return result;
        }

        public static int[] GetProvByNumber(IncentiviEntities db, List<int> provs)
        {
            List<int> decodProvs = new List<int>();
            if (provs.Contains(1))
            {
                decodProvs.AddRange(GetProvBySigla(db, SIGLA_PROMOZIONI));
            }
            if (provs.Contains(2))
            {
                decodProvs.AddRange(GetProvBySigla(db, SIGLA_AUMENTI));
            }
            if (provs.Contains(3))
            {
                decodProvs.AddRange(GetProvBySigla(db, SIGLA_GRATIFICHE));
            }
            return decodProvs.ToArray();
        }
        public static int[] GetProvBySigla(IncentiviEntities db, params string[] sigla)
        {
            int[] result;

            if (db == null)
                db = new IncentiviEntities();

            result = db.XR_PRV_PROV.Where(x => sigla.Contains(x.SIGLA)).Select(x => x.ID_PROV).ToArray();

            return result;
        }

        public static void ConversionePrEffettivo()
        {
            var db = new IncentiviEntities();

            var notNessuna = NotAnyOfProv();
            var richList = db.XR_PRV_DIPENDENTI.Where(notNessuna);

            foreach (var item in richList)
            {
                var provEff = item.XR_PRV_DIPENDENTI_VARIAZIONI.FirstOrDefault(x => x.ID_PROV == item.ID_PROV_EFFETTIVO);
                provEff.IND_PRV_EFFETTIVO = true;
            }

            var param = db.XR_HRIS_PARAM.FirstOrDefault(x => x.COD_PARAM == "PRetribPrvSuVariazioni");
            bool isNew = false;
            if (param == null)
            {
                isNew = true;
                param = new XR_HRIS_PARAM()
                {
                    COD_PARAM = "PRetribPrvSuVariazioni"
                };
            }
            param.COD_VALUE1 = "TRUE";
            if (isNew)
                db.XR_HRIS_PARAM.Add(param);

            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        public static string WriteTEX3041(string matricola, string catArrivo, string CAT_PARTENZA)
        {
            string result = "";
            char filler = ' ';
            string separator = " ";

            DateTime dtDec = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1).AddMonths(1);

            result += CezanneHelper.AddToken("PC", 2, filler, separator);
            result += CezanneHelper.AddToken(matricola, 6, filler, separator);
            result += CezanneHelper.AddToken(dtDec.ToString("yyyyMMdd"), 8, filler, separator);
            result += CezanneHelper.AddToken(catArrivo, 3, filler, separator);

            result += "".PadLeft(25, ' ');
            result += CAT_PARTENZA;
            result = result.PadRight(100, filler);

            return result;
        }
        public static bool InsertUpdateRequest(int idDip, string categoria, out string error)
        {
            bool result = false;
            error = null;
            IncentiviEntities db = new IncentiviEntities();
            var dip = db.XR_PRV_DIPENDENTI.Find(idDip);

            var anyRequest = db.XR_PRV_DIPENDENTI_SIMULAZIONI.Any(x => x.ID_DIPENDENTE == dip.ID_DIPENDENTE && x.CAT_ARRIVO == categoria && (x.DTA_ESECUZIONE == null || x.DTA_ACQUISIZIONE == null));

            if (!anyRequest)
            {
                XR_PRV_DIPENDENTI_SIMULAZIONI sim = new XR_PRV_DIPENDENTI_SIMULAZIONI()
                {
                    ID_DIPENDENTE = dip.ID_DIPENDENTE,
                    MATRICOLA = dip.MATRICOLA,
                    MATR_RICHIESTA = CommonHelper.GetCurrentUserMatricola(),
                    DTA_RICHIESTA = DateTime.Now,
                    CAT_PARTENZA = "", //Da decidere se usare
                    CAT_ARRIVO = categoria,
                    DTA_ESECUZIONE = null,
                    DTA_ACQUISIZIONE = null,
                    JSON_SIMULAZIONE = null
                };

                db.XR_PRV_DIPENDENTI_SIMULAZIONI.Add(sim);
                result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                if (!result)
                    error = "Errore durante l'inserimento della richiesta";
            }
            else
                error = "Richiesta già presente";

            return result;
        }

    }
}
