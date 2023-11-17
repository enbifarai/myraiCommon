using myRai.Data.Interface;
using myRai.DataAccess;
using myRaiData.Incentivi;
using myRaiDataTalentia;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Xceed.Words.NET;

namespace myRaiHelper
{
    public enum HrisParam
    {
        AbilitaLog,
        LogSimulata,
        LogDump,
        PathModificaAnagrafica,
        PathModificaResidenza,
        AbilHRISDaCodice,
        IncentiviCostoMax,
        IncentiviMensilitaAggiuntive,
        IncentiviAbilitaMail,
        IncentiviEsecuzioneFNL930,
        IncentiviTokenStra,
        IncentiviQueryBudget,
        IncentiviQuerySaving,
        IncentiviQueryQ100Rai,
        IncentiviFilterAbilSaving,
        IncentiviSuperUser,
        IncentiviMailRiepilogoSaving,
        IncentiviMailRiepilogoPratiche,
        IncentiviAbilitaMailVerbale,
        IncentiviMailVerbaleTokenQIO,
        IncentiviMailVerbaleTokenItl,
        IncentiviMailVerbaleTokenRoma,
        IncentiviLimiteBozzaVerbale,
        IncentiviRiferimentoConteggi,
        IncentiviQualificheSaltaUfficioPrestiti,
        IncentiviStatiVerbale,
        IncentiviMailVerbaleTokenIban,
        IncentiviVerbaleToken,
        IncentiviRifGruppo,
        CredenzialiServerCezanne,
        PoliticheParametri,
        HrisLogParam,
        DecodSocietaStrOrg,
        ImmatricolazioneServizio,
        AbilLockAnagNoDip,
        IncentiviGruppi,
        CezanneUltimoAggiornamentoBatch,
        IncentiviOneri,
        IncentiviParametri,
        HRISAbilFunc,
        LoadAllHrga,
        IncentiviAbilitaMailAccettazione,
        AccountUtenteServizio,
        SftpIFIKey,
        IncentiviBozzeVerbali,
        PRetribPrvSuVariazioni,
        NDIAbilFunc,
        SWAbilFunc,
        ValAbilFunc,
        INCAbilFunc
    }

    public class HrisLogParam
    {
        public HrisLogFunc[] Funzioni { get; set; }
        public bool LogSimulata { get; set; }
        public bool LogDump { get; set; }
    }
    public class HrisLogFunc
    {
        public bool Attivo { get; set; }
        public string Chiave { get; set; }
        public string[] MatricoleAbilitate { get; set; }
        public string[] MatricoleDisabilitate { get; set; }
        public bool? LogSimulata { get; set; }
        public bool? LogDump { get; set; }

        public bool IsEnabled(string matr)
        {
            return Attivo && (MatricoleAbilitate == null || MatricoleAbilitate.Contains(matr)) && (MatricoleDisabilitate == null || !MatricoleDisabilitate.Contains(matr));
        }

    }
    public class NotificationSubscription
    {
        public string endpoint { get; set; }
        public NotificationSubsciptionKey keys { get; set; }
    }
    public class NotificationSubsciptionKey
    {
        public string p256dh { get; set; }
        public string auth { get; set; }
    }

    public class HrisMapSocieta
    {
        public string Descrizione { get; set; }
        public string Cezanne { get; set; }
        public string Struttura { get; set; }
        public string CICS { get; set; }
        public string NoDip { get; set; }
        public int Stato { get; set; }
    }

    public class HrisHelper
    {
        public static bool UpdateParametro(HrisParam chiave, params string[] valore)
        {
            myRaiData.Incentivi.XR_HRIS_PARAM p = null;
            String NomeParametro = chiave.ToString();

            IncentiviEntities db = new IncentiviEntities();
            p = db.XR_HRIS_PARAM.FirstOrDefault(x => x.COD_PARAM == NomeParametro);

            bool isNew = p == null;
            if (isNew)
                p = new myRaiData.Incentivi.XR_HRIS_PARAM() { COD_PARAM = NomeParametro };

            int numElem = valore == null ? 0 : valore.Length;
            switch (numElem)
            {
                case 3:
                    p.COD_VALUE3 = valore[2];
                    goto case 2;
                case 2:
                    p.COD_VALUE2 = valore[1];
                    goto case 1;
                case 1:
                    p.COD_VALUE1 = valore[0];
                    break;
                default:
                    p.COD_VALUE1 = null;
                    p.COD_VALUE2 = null;
                    p.COD_VALUE3 = null;
                    break;
            }

            if (isNew)
                db.XR_HRIS_PARAM.Add(p);

            try
            {
                db.SaveChanges();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static myRaiData.Incentivi.XR_HRIS_PARAM GetParametro(HrisParam chiave)
        {
            myRaiData.Incentivi.XR_HRIS_PARAM p = null;
            String NomeParametro = chiave.ToString();

            switch (chiave)
            {
                case HrisParam.IncentiviRifGruppo:
                case HrisParam.HRISAbilFunc:
                case HrisParam.PRetribPrvSuVariazioni:
                case HrisParam.DecodSocietaStrOrg:
                case HrisParam.NDIAbilFunc:
                case HrisParam.SWAbilFunc:
                case HrisParam.ValAbilFunc:
                case HrisParam.INCAbilFunc:
                case HrisParam.IncentiviStatiVerbale:
                    if (SessionHelper.Get("HRIS-PARAM-" + NomeParametro) != null)
                        p = (myRaiData.Incentivi.XR_HRIS_PARAM)SessionHelper.Get("HRIS-PARAM-" + NomeParametro);
                    else
                    {
                        using (IncentiviEntities db = new IncentiviEntities())
                        {
                            p = db.XR_HRIS_PARAM.FirstOrDefault(x => x.COD_PARAM == NomeParametro);
                            SessionHelper.Set("HRIS-PARAM-" + NomeParametro, p);
                        }
                    }
                    break;
                default:
                    //System.Diagnostics.Debug.WriteLine("HRIS-" + NomeParametro);
                    using (IncentiviEntities db = new IncentiviEntities())
                        p = db.XR_HRIS_PARAM.FirstOrDefault(x => x.COD_PARAM == NomeParametro);
                    break;
            }



            return p;
        }
        public static T GetParametro<T>(HrisParam chiave)
        {
            myRaiData.Incentivi.XR_HRIS_PARAM p = GetParametro(chiave);
            if (p == null) return default(T);
            else return (T)Convert.ChangeType(p.COD_VALUE1, typeof(T));
        }

        public static T[] GetParametri<T>(HrisParam chiave)
        {
            myRaiData.Incentivi.XR_HRIS_PARAM p = GetParametro(chiave);
            if (p == null) return null;
            else
            {
                T[] parametri = new T[] { (T)Convert.ChangeType(p.COD_VALUE1, typeof(T)), (T)Convert.ChangeType(p.COD_VALUE2, typeof(T)), (T)Convert.ChangeType(p.COD_VALUE3, typeof(T)), (T)Convert.ChangeType(p.COD_VALUE4, typeof(T)) };
                return parametri;
            }
        }

        public static T GetParametroJson<T>(HrisParam chiave)
        {
            myRaiData.Incentivi.XR_HRIS_PARAM p = GetParametro(chiave);
            if (p == null)
                return default(T);
            else
            {
                try
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(p.COD_VALUE1);
                }
                catch (Exception)
                {

                    return default(T);
                }

            }
        }

        public static List<T> GetParametriJson<T>(HrisParam chiave)
        {
            List<T> result = new List<T>();
            myRaiData.Incentivi.XR_HRIS_PARAM p = GetParametro(chiave);
            if (p == null)
                return null;
            else
            {
                try
                {
                    result = Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(p.COD_VALUE1);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return result;
        }

        public static bool GetOvverideAbil(string funzione, out Dictionary<string, string> abilitazioni)
        {
            bool result = false;
            abilitazioni = null;

            var db = new IncentiviEntities();
            var param = db.XR_HRIS_PARAM.FirstOrDefault(x => x.COD_PARAM == "AbilOverride" && x.COD_VALUE1 == funzione);
            if (param != null)
            {
                result = true;
                abilitazioni = param.COD_VALUE2.Split(';')
                                    .Select(x => x.Trim())
                                    .Where(x => !String.IsNullOrWhiteSpace(x))
                                    .Select(x => x.Split('|'))
                                    .ToDictionary(y => y[0], y => y[1]);
            }

            return result;
        }

        private static bool IsEnableLogParam(string operazione, HrisParam param)
        {
            bool result = false;

            string matricola = CommonHelper.GetCurrentUserMatricola();

            var dbParam = GetParametri<string>(param);
            if (dbParam != null
                && (!String.IsNullOrWhiteSpace(dbParam[0]) && dbParam[0] == "TRUE")
                 && (String.IsNullOrWhiteSpace(dbParam[1]) || dbParam[1].Contains(operazione))
                  && (String.IsNullOrWhiteSpace(dbParam[2]) || dbParam[2].Contains(matricola))
                )
                result = true;

            return result;
        }

        private static bool IsEnableLog(string operazione, out HrisLogFunc func)
        {
            bool result = false;
            func = null;
            string matricola = CommonHelper.GetCurrentUserMatricola();

            var logParam = GetParametroJson<HrisLogParam>(HrisParam.HrisLogParam);
            if (logParam != null && logParam.Funzioni != null)
            {
                func = logParam.Funzioni.FirstOrDefault(x => x.Chiave == operazione && x.IsEnabled(matricola));
                if (func != null)
                {
                    result = true;
                    if (!func.LogDump.HasValue)
                        func.LogDump = logParam.LogDump;
                    if (!func.LogSimulata.HasValue)
                        func.LogSimulata = logParam.LogSimulata;
                }
            }

            return result;
        }

        public static void LogOperazione(string operazione, string parametri, bool esito, string errore = null, string noteEsito = null, Exception ex = null, object extra = null)
        {
            if (!IsEnableLog(operazione, out HrisLogFunc func))
                return;

            IncentiviEntities db = new IncentiviEntities();

            int currentIdPersona = CommonHelper.GetCurrentIdPersona();
            string currentMatricola = CommonHelper.GetCurrentUserMatricola();

            string exInfo = "";
            if (ex != null)
            {
                try
                {
                    exInfo = Newtonsoft.Json.JsonConvert.SerializeObject(ex);
                }
                catch (Exception)
                {

                    exInfo = "***Eccezione non disponibile***";
                }
            }

            myRaiData.Incentivi.XR_HRIS_LOG log = new myRaiData.Incentivi.XR_HRIS_LOG()
            {
                ID_PERSONA = currentIdPersona,
                COD_MATRICOLA = currentMatricola,
                DES_OPERAZIONE = operazione,
                NOT_PARAMETRI = parametri + " " + exInfo,
                IND_ESITO = esito,
                NOT_ERRORE = errore,
                NOT_ESITO = noteEsito
            };

            string additionalInfo = "";

            if (func.LogSimulata.GetValueOrDefault() && CommonHelper.GetCurrentRealUsername() != CommonHelper.GetCurrentUserPMatricola())
                additionalInfo = "*";

            if (func.LogDump.GetValueOrDefault())
            {
                var sidebar = UtenteHelper.getSidebarModel();
                try
                {
                    string dumpInfo = Newtonsoft.Json.JsonConvert.SerializeObject(new
                    {
                        Contesto = CommonHelper.GetApplicationType(),
                        RealName = CommonHelper.GetCurrentRealUsername(),
                        ProfiliAdatti = (sidebar != null && sidebar.myProfili != null ? sidebar.myProfili.Select(x => x.nome_profilo).ToList() : new List<string>()),
                        ControllerAutorizzati = UtenteHelper.getAuthorizedControllers(),
                        Server = HttpContext.Current.Server.MachineName,
                        Request = HttpContext.Current.Request.RawUrl
                    });
                    additionalInfo += dumpInfo;
                }
                catch (Exception exJ)
                {
                    additionalInfo += "**DUMP non disponibile**";
                    additionalInfo += "{ exMessage:'" + exJ.Message + "', exStack:'" + exJ.StackTrace + "'}";
                }


            }

            if (extra != null)
            {
                try
                {
                    additionalInfo += Newtonsoft.Json.JsonConvert.SerializeObject(extra, new Newtonsoft.Json.JsonSerializerSettings() { ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore });
                }
                catch (Exception)
                {
                    additionalInfo += "***extra non disponibile***";
                }
                
            }

            if (!String.IsNullOrWhiteSpace(additionalInfo))
                log.NOT_ADDITIONAL_INFO = additionalInfo;

            CezanneHelper.GetCampiFirma(out string cod_user, out string cod_termid, out DateTime tms_timestamp);
            log.COD_USER = cod_user;
            log.COD_TERMID = cod_termid;
            log.TMS_TIMESTAMP = tms_timestamp;

            db.XR_HRIS_LOG.Add(log);

            DBHelper.Save(db, currentMatricola, "HRIS - " + operazione);

            AggiungiNotificaMatr(String.Format("{0} - {1} - {2}", operazione, esito, errore), "103650");
        }

        public static DateTime? GetDataAnzCat(int idPersona)
        {
            DateTime? anzCat = null;

            var db = new IncentiviEntities();
            string query = "";

            if (db.Database.Connection.Database.ToUpper() == "CZNDB")
                query = String.Format("select top 1 DTA_ANZCAT from ASSQUAL join JSSQUAL on ASSQUAL.ID_ASSQUAL = JSSQUAL.ID_ASSQUAL where ID_PERSONA={0} and DTA_FINE>GETDATE() order by DTA_INIZIO", idPersona);
            else
                query = String.Format("SELECT TOP 1 DTA_ANZCAT FROM ASSQUAL WHERE ID_PERSONA={0} and DTA_FINE>GETDATE()", idPersona);

            try
            {
                var tmp = db.Database.SqlQuery<DateTime?>(query);

                if (tmp != null && tmp.Any())
                    anzCat = tmp.First();

            }
            catch (Exception)
            {

            }

            return anzCat;
        }

        public static string GetCodCasaGit(IncentiviEntities db, int idPersona)
        {
            string codCasaGit = null;

            string query = "";

            if (db.Database.Connection.Database.ToUpper() == "CZNDB")
                query = String.Format("select top 1 COD_CASAGIT from JNAGPERS join ANAGPERS on ANAGPERS.ID_PERSONA = JNAGPERS.ID_PERSONA where ANAGPERS.ID_PERSONA={0}", idPersona);
            else
                query = String.Format("SELECT TOP 1 COD_CASAGIT FROM ANAGPERS WHERE ID_PERSONA={0}", idPersona);

            try
            {
                var tmp = db.Database.SqlQuery<string>(query);

                if (tmp != null && tmp.Any())
                    codCasaGit = tmp.First();

            }
            catch (Exception)
            {

            }

            return codCasaGit;
        }
        public static string GetCodCasaGit(TalentiaEntities db, int idPersona)
        {
            string codCasaGit = null;

            string query = "";

            query = String.Format("SELECT TOP 1 COD_CASAGIT FROM ANAGPERS WHERE ID_PERSONA={0}", idPersona);

            try
            {
                var tmp = db.Database.SqlQuery<string>(query);

                if (tmp != null && tmp.Any())
                    codCasaGit = tmp.First();

            }
            catch (Exception)
            {

            }

            return codCasaGit;
        }

        public static string GetCodMatColl(IncentiviEntities db, int idPersona)
        {
            string codMatColl = null;

            string query = "";

            if (db.Database.Connection.Database.ToUpper() == "CZNDB")
                query = String.Format("select top 1 COD_MATCOLL from JNAGPERS join ANAGPERS on ANAGPERS.ID_PERSONA = JNAGPERS.ID_PERSONA where ANAGPERS.ID_PERSONA={0}", idPersona);
            else
                query = String.Format("SELECT TOP 1 COD_MATCOLL FROM ANAGPERS WHERE ID_PERSONA={0}", idPersona);

            try
            {
                var tmp = db.Database.SqlQuery<string>(query);

                if (tmp != null && tmp.Any())
                    codMatColl = tmp.First();

            }
            catch (Exception)
            {

            }

            return codMatColl;
        }
        public static string GetCodMatColl(TalentiaEntities db, int idPersona)
        {
            string codMatColl = null;

            string query = "";

            query = String.Format("SELECT TOP 1 COD_MATCOLL FROM ANAGPERS WHERE ID_PERSONA={0}", idPersona);

            try
            {
                var tmp = db.Database.SqlQuery<string>(query);

                if (tmp != null && tmp.Any())
                    codMatColl = tmp.First();

            }
            catch (Exception)
            {

            }

            return codMatColl;
        }

        public static List<myRaiData.Incentivi.STUPERSONA> GetStupersona(IncentiviEntities db, int idPersona)
        {
            List<myRaiData.Incentivi.STUPERSONA> result = new List<myRaiData.Incentivi.STUPERSONA>();

            if (db.Database.Connection.Database.ToUpper() == "CZNDB")
            {
                result.AddRange(db.STUPERSONA
                                    .Include("JTUPERSONA")
                                    .Include("TB_STUDIO").Include("TB_STUDIO.TB_LIVSTUD")
                                    .Include("TB_ATENEO").Include("TB_ATENEO.TB_COMUNE")
                                    .Include("TB_TPPUNT")
                                    .Include("TB_COMUNE")
                                    .Where(x => x.ID_PERSONA == idPersona));
            }
            else
            {
                var dbTal = new TalentiaEntities();
                var tmp = dbTal.STUPERSONA
                                .Include("TB_STUDIO").Include("TB_STUDIO.TB_LIVSTUD")
                                .Include("TB_ATENEO").Include("TB_ATENEO.TB_COMUNE")
                                .Include("TB_TPPUNT")
                                .Include("TB_COMUNE")
                                .Where(x => x.ID_PERSONA == idPersona);
                foreach (var item in tmp)
                {
                    result.Add(new myRaiData.Incentivi.STUPERSONA()
                    {
                        ID_PERSONA = item.ID_PERSONA,
                        COD_STUDIO = item.COD_STUDIO,
                        COD_ATENEO = item.COD_ATENEO,
                        COD_CITTA = item.COD_CITTA,
                        COD_LIVELLOPESO = item.COD_LIVELLOPESO,
                        COD_PUNTEGGIO = item.COD_PUNTEGGIO,
                        COD_PUNTEGGIONUM = item.COD_PUNTEGGIONUM,
                        COD_TERMID = item.COD_TERMID,
                        COD_TIPOPUNTEGGIO = item.COD_TIPOPUNTEGGIO,
                        COD_USER = item.COD_USER,
                        DTA_CONSEG = item.DTA_CONSEG,
                        NOT_NOTABREVE = item.NOT_NOTABREVE,
                        JTUPERSONA = new JTUPERSONA()
                        {
                            ID_PERSONA = item.ID_PERSONA,
                            COD_STUDIO = item.COD_STUDIO,
                            COD_LIVELLOSTUDIO = item.COD_LIVELLOSTUDIO,
                            DES_CORSO = item.DES_CORSO,
                            DES_ISTITUTO = item.DES_ISTITUTO,
                            DES_LIVELLOSTUDIO = item.DES_LIVELLOSTUDIO,
                            DES_RICONOSCIMENTO = item.DES_RICONOSCIMENTO,
                            DTA_INIZIO = item.DTA_INIZIO
                        },
                        TB_STUDIO = item.TB_STUDIO == null ? null : new myRaiData.Incentivi.TB_STUDIO()
                        {
                            COD_STUDIO = item.TB_STUDIO.COD_STUDIO,
                            COD_LIVELLOSTUDIO = item.TB_STUDIO.COD_LIVELLOSTUDIO,
                            COD_SETTORE = item.TB_STUDIO.COD_SETTORE,
                            DES_STUDIO = item.TB_STUDIO.DES_STUDIO,
                            IND_WEBVISIBLE = item.TB_STUDIO.IND_WEBVISIBLE,
                            QTA_ANNI = item.TB_STUDIO.QTA_ANNI,
                            QTA_ORDINE = item.TB_STUDIO.QTA_ORDINE,

                            TB_LIVSTUD = item.TB_STUDIO.TB_LIVSTUD == null ? null : new myRaiData.Incentivi.TB_LIVSTUD()
                            {
                                COD_LIVELLOSTUDIO = item.TB_STUDIO.TB_LIVSTUD.COD_LIVELLOSTUDIO,
                                DES_LIVELLOSTUDIO = item.TB_STUDIO.TB_LIVSTUD.DES_LIVELLOSTUDIO,
                                QTA_ORDINE = item.TB_STUDIO.TB_LIVSTUD.QTA_ORDINE
                            }
                        },
                        TB_ATENEO = item.TB_ATENEO == null ? null : new myRaiData.Incentivi.TB_ATENEO()
                        {
                            COD_ATENEO = item.TB_ATENEO.COD_ATENEO,
                            DES_ATENEO = item.TB_ATENEO.DES_ATENEO,
                            COD_CITTA = item.TB_ATENEO.COD_CITTA,
                            QTA_ORDINE = item.TB_ATENEO.QTA_ORDINE,
                            TB_COMUNE = item.TB_ATENEO.TB_COMUNE == null ? null : new myRaiData.Incentivi.TB_COMUNE()
                            {
                                COD_AREA = item.TB_ATENEO.TB_COMUNE.COD_AREA,
                                COD_CITTA = item.TB_ATENEO.TB_COMUNE.COD_CITTA,
                                COD_PROV_STATE = item.TB_ATENEO.TB_COMUNE.COD_PROV_STATE,
                                COD_SIGLANAZIONE = item.TB_ATENEO.TB_COMUNE.COD_SIGLANAZIONE,
                                DES_CITTA = item.TB_ATENEO.TB_COMUNE.DES_CITTA,
                                DES_PROV_STATE = item.TB_ATENEO.TB_COMUNE.DES_PROV_STATE,
                                DES_TERRCODE = item.TB_ATENEO.TB_COMUNE.DES_TERRCODE,
                                QTA_ORDINE = item.TB_ATENEO.TB_COMUNE.QTA_ORDINE
                            }
                        },
                        TB_COMUNE = item.TB_COMUNE == null ? null : new myRaiData.Incentivi.TB_COMUNE()
                        {
                            COD_AREA = item.TB_COMUNE.COD_AREA,
                            COD_CITTA = item.TB_COMUNE.COD_CITTA,
                            COD_PROV_STATE = item.TB_COMUNE.COD_PROV_STATE,
                            COD_SIGLANAZIONE = item.TB_COMUNE.COD_SIGLANAZIONE,
                            DES_CITTA = item.TB_COMUNE.DES_CITTA,
                            DES_PROV_STATE = item.TB_COMUNE.DES_PROV_STATE,
                            DES_TERRCODE = item.TB_COMUNE.DES_TERRCODE,
                            QTA_ORDINE = item.TB_COMUNE.QTA_ORDINE
                        },
                        TB_TPPUNT = item.TB_TPPUNT == null ? null : new myRaiData.Incentivi.TB_TPPUNT()
                        {
                            COD_PUNTEGGIOMAX = item.TB_TPPUNT.COD_PUNTEGGIOMAX,
                            COD_PUNTEGGIOMIN = item.TB_TPPUNT.COD_PUNTEGGIOMIN,
                            COD_SCALAGIUDIZIO = item.TB_TPPUNT.COD_SCALAGIUDIZIO,
                            COD_TIPOPUNTEGGIO = item.TB_TPPUNT.COD_TIPOPUNTEGGIO,
                            DES_TIPOPUNTEGGIO = item.TB_TPPUNT.DES_TIPOPUNTEGGIO,
                            IND_NUMCONTINUO = item.TB_TPPUNT.IND_NUMCONTINUO,
                            IND_WEBVISIBLE = item.TB_TPPUNT.IND_WEBVISIBLE,
                            QTA_ORDINE = item.TB_TPPUNT.QTA_ORDINE
                        }
                    });
                }
            }

            return result;
        }

        public static TDest ConvertWithJson<TOrig, TDest>(TOrig fromObj)
        {
            TDest toObj = default(TDest);

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(fromObj);
            try
            {
                toObj = Newtonsoft.Json.JsonConvert.DeserializeObject<TDest>(json);
            }
            catch (Exception)
            {
                toObj = default(TDest);
            }

            return toObj;
        }

        public static void AddSegnalazione(string tipologia, string ambito, string messaggio)
        {
            var dbTal = new myRaiData.Incentivi.IncentiviEntities();
            myRaiData.Incentivi.XR_HRIS_SEGNALAZIONE segnalazione = new myRaiData.Incentivi.XR_HRIS_SEGNALAZIONE()
            {
                MATR_INSERIMENTO = CommonHelper.GetCurrentUserMatricola(),
                COD_TIPOLOGIA = tipologia,
                DES_AMBITO = ambito,
                NOT_SEGNALAZIONE = messaggio,
                DTA_INSERIMENTO = DateTime.Now
            };
            dbTal.XR_HRIS_SEGNALAZIONE.Add(segnalazione);
            DBHelper.Save(dbTal, CommonHelper.GetCurrentUserMatricola(), "");
        }

        public static XR_HRIS_TEMPLATE GetTemplate(ISintesi1 sintesi, int? tipologia, string codTipo, string filtroTemplate)
        {
            XR_HRIS_TEMPLATE template = null;

            IncentiviEntities db = new IncentiviEntities();
            var query = db.XR_HRIS_TEMPLATE.Where(x => x.COD_TIPO.ToUpper() == codTipo.ToUpper() && x.NME_TEMPLATE == filtroTemplate && x.ID_GESTIONE == null);
            if (tipologia.HasValue)
                query = query.Where(x => x.ID_TIPOLOGIA == tipologia.Value);
            else
                query = query.Where(x => !x.ID_TIPOLOGIA.HasValue);

            query = query.Where(x => x.VALID_DTA_INI < DateTime.Now && (x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now));

            template = query.Where(x => (x.MATR_INCLUSE == null || x.MATR_INCLUSE.Contains(sintesi.COD_MATLIBROMAT))
                                            && (x.MATR_ESCLUSE == null || !x.MATR_ESCLUSE.Contains(sintesi.COD_MATLIBROMAT)))
                            .Where(x => (x.DIR_INCLUSE == null || x.DIR_INCLUSE.Contains(sintesi.COD_SERVIZIO))
                                            && (x.DIR_ESCLUSE == null || !x.DIR_ESCLUSE.Contains(sintesi.COD_SERVIZIO)))
                            .AsEnumerable()
                            .Where(x => (x.CAT_INCLUSE == null || x.CAT_INCLUSE.Split(',').Any(y => sintesi.COD_QUALIFICA.StartsWith(y)))
                                            && (x.CAT_ESCLUSE == null || !x.CAT_ESCLUSE.Split(',').Any(y => sintesi.COD_QUALIFICA.StartsWith(y))))
                            .FirstOrDefault();
            
            return template;
        }
        public static XR_HRIS_TEMPLATE GetTemplateById(int idTemplate)
        {
            XR_HRIS_TEMPLATE template = null;

            IncentiviEntities db = new IncentiviEntities();
            template = db.XR_HRIS_TEMPLATE.Where(x => x.ID_TEMPLATE == idTemplate).FirstOrDefault();
            //if (tipologia.HasValue)
            //    query = query.Where(x => x.ID_TIPOLOGIA == tipologia.Value);
            //else
            //    query = query.Where(x => !x.ID_TIPOLOGIA.HasValue);

            //query = query.Where(x => x.VALID_DTA_INI < DateTime.Now && (x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now));

            //template = query.Where(x => (x.MATR_INCLUSE == null || x.MATR_INCLUSE.Contains(sintesi.COD_MATLIBROMAT))
            //                                && (x.MATR_ESCLUSE == null || !x.MATR_ESCLUSE.Contains(sintesi.COD_MATLIBROMAT)))
            //                .Where(x => (x.DIR_INCLUSE == null || x.DIR_INCLUSE.Contains(sintesi.COD_SERVIZIO))
            //                                && (x.DIR_ESCLUSE == null || !x.DIR_ESCLUSE.Contains(sintesi.COD_SERVIZIO)))
            //                .AsEnumerable()
            //                .Where(x => (x.CAT_INCLUSE == null || x.CAT_INCLUSE.Split(',').Any(y => sintesi.COD_QUALIFICA.StartsWith(y)))
            //                                && (x.CAT_ESCLUSE == null || !x.CAT_ESCLUSE.Split(',').Any(y => sintesi.COD_QUALIFICA.StartsWith(y))))
            //                .FirstOrDefault();

            return template;
        }
        public static IEnumerable<XR_HRIS_TEMPLATE> GetTemplateList(int? tipologia, string codTipo, string filtroTemplate)
        {
            IncentiviEntities db = new IncentiviEntities();
            var query = db.XR_HRIS_TEMPLATE.Where(x => x.COD_TIPO.ToUpper() == codTipo.ToUpper() && x.NME_TEMPLATE == filtroTemplate && x.ID_GESTIONE == null);
            if (tipologia.HasValue)
                query = query.Where(x => x.ID_TIPOLOGIA == tipologia.Value);
            else
                query = query.Where(x => !x.ID_TIPOLOGIA.HasValue);

            query = query.Where(x => x.VALID_DTA_INI < DateTime.Now && (x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now));

            return query.ToList();
        }
        public static XR_HRIS_TEMPLATE GetTemplateFromList(IEnumerable<XR_HRIS_TEMPLATE> list, ISintesi1 sintesi, int? tipologia, string codTipo, string filtroTemplate)
        {
            XR_HRIS_TEMPLATE template = null;

            var query = list.Where(x => x.COD_TIPO.ToUpper() == codTipo.ToUpper() && x.NME_TEMPLATE == filtroTemplate && x.ID_GESTIONE == null);
            if (tipologia.HasValue)
                query = query.Where(x => x.ID_TIPOLOGIA == tipologia.Value);
            else
                query = query.Where(x => !x.ID_TIPOLOGIA.HasValue);

            query = query.Where(x => x.VALID_DTA_INI < DateTime.Now && (x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now));

            template = query.Where(x => (x.MATR_INCLUSE == null || x.MATR_INCLUSE.Contains(sintesi.COD_MATLIBROMAT))
                                            && (x.MATR_ESCLUSE == null || !x.MATR_ESCLUSE.Contains(sintesi.COD_MATLIBROMAT)))
                            .Where(x => (x.DIR_INCLUSE == null || x.DIR_INCLUSE.Contains(sintesi.COD_SERVIZIO))
                                            && (x.DIR_ESCLUSE == null || !x.DIR_ESCLUSE.Contains(sintesi.COD_SERVIZIO)))
                            .Where(x => (x.CAT_INCLUSE == null || x.CAT_INCLUSE.Split(',').Any(y => sintesi.COD_QUALIFICA.StartsWith(y)))
                                            && (x.CAT_ESCLUSE == null || !x.CAT_ESCLUSE.Split(',').Any(y => sintesi.COD_QUALIFICA.StartsWith(y))))
                            .FirstOrDefault();

            return template;
        }

        public static string ReplaceToken(ISintesi1 sintesi, object entity, string input, string tokenDelimiter = "__", Dictionary<string, object> additionalToken = null)
        {
            string output = input;

            DateTime now = DateTime.Now;

            MatchCollection matches = Regex.Matches(input, $@"(?'token'{tokenDelimiter}(?'key'[A-Za-z0-9_]+)(\$(?'format'[^\$]+)\$)?{tokenDelimiter})");
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    string key = match.Groups["key"].Value;
                    string token = match.Groups["token"].Value;
                    string format = match.Groups["format"].Success ? match.Groups["format"].Value : null;
                    switch (key)
                    {
                        case "NOMINATIVO":
                            output = output.Replace(token, sintesi.Nominativo().TitleCase());
                            break;
                        case "COGNOME":
                            output = output.Replace(token, sintesi.DES_COGNOMEPERS.TitleCase());
                            break;
                        case "NOME":
                            output = output.Replace(token, sintesi.DES_NOMEPERS.TitleCase());
                            break;
                        case "TODAY":
                            output = output.Replace(token, now.ToString(format ?? "dd/MM/yyyy"));
                            break;
                        case "TIME":
                            output = output.Replace(token, now.ToString(format ?? "HH:mm"));
                            break;
                        case "NOW":
                            output = output.Replace(token, now.ToString(format ?? "dd/MM/yyyy HH:mm"));
                            break;
                        default:
                            object additionalValue = null;
                            if (additionalToken != null && additionalToken.TryGetValue(key, out additionalValue))
                                output = output.Replace(token, FormatObjectValue(additionalValue, additionalValue.GetType(), format));
                            else
                            {
                                if (entity != null)
                                {
                                    var prop = entity.GetType().GetProperty(key);
                                    if (prop != null)
                                    {
                                        object val = prop.GetValue(entity, null);
                                        string tmpOutput = FormatObjectValue(val, prop.PropertyType, format);
                                        //if (val != null)
                                        //{
                                        //    Type outputType = prop.PropertyType;
                                        //    Type baseType = outputType.IsGenericType && outputType.GetGenericArguments() != null && outputType.GetGenericArguments().Any() ? outputType.GetGenericArguments()[0] : outputType;

                                        //    if (baseType == typeof(DateTime))
                                        //        format = format ?? "dd/MM/yyyy";
                                        //    else if (baseType == typeof(decimal))
                                        //        format = format ?? "N2";
                                        //    else
                                        //        tmpOutput = String.Format("{0}", val);

                                        //    var cultureInfo = CultureInfo.GetCultureInfo("it-IT");
                                        //    string outformat = String.IsNullOrWhiteSpace(format) ? "{0}" : "{0:" + format + "}";
                                        //    tmpOutput = String.Format(cultureInfo, outformat, val);
                                        //}

                                        output = output.Replace(token, tmpOutput);
                                    }
                                }
                            }
                            break;
                    }
                }
            }

            return output;
        }

        public static void DocReplaceToken(ISintesi1 sintesi, object entity, DocX doc, string tokenDelimiter = "__", Dictionary<string, object> additionalToken = null)
        {
            string input = doc.Text;

            DateTime now = DateTime.Now;

            MatchCollection matches = Regex.Matches(input, $@"(?'token'{tokenDelimiter}(?'key'[A-Za-z0-9_]+)(\$(?'format'[^\$]+)\$)?{tokenDelimiter})");
            foreach (Match match in matches)
            {
                if (match.Success)
                {
                    string key = match.Groups["key"].Value;
                    string token = match.Groups["token"].Value;
                    string format = match.Groups["format"].Success ? match.Groups["format"].Value : null;
                    switch (key)
                    {
                        case "NOMINATIVO":
                            doc.ReplaceText(token, sintesi.Nominativo().TitleCase());
                            break;
                        case "COGNOME":
                            doc.ReplaceText(token, sintesi.DES_COGNOMEPERS.TitleCase());
                            break;
                        case "NOME":
                            doc.ReplaceText(token, sintesi.DES_NOMEPERS.TitleCase());
                            break;
                        case "TODAY":
                            doc.ReplaceText(token, now.ToString(format ?? "dd/MM/yyyy"));
                            break;
                        case "TIME":
                            doc.ReplaceText(token, now.ToString(format ?? "HH:mm"));
                            break;
                        case "NOW":
                            doc.ReplaceText(token, now.ToString(format ?? "dd/MM/yyyy HH:mm"));
                            break;
                        default:
                            object additionalValue = null;
                            if (additionalToken != null && additionalToken.TryGetValue(key, out additionalValue))
                                doc.ReplaceText(token, FormatObjectValue(additionalValue, additionalValue.GetType(), format));
                            else
                            {
                                if (entity != null)
                                {
                                    var prop = entity.GetType().GetProperty(key);
                                    if (prop != null)
                                    {
                                        object val = prop.GetValue(entity, null);
                                        string tmpOutput = FormatObjectValue(val, prop.PropertyType, format);
                                        //if (val != null)
                                        //{
                                        //    Type outputType = prop.PropertyType;
                                        //    Type baseType = outputType.IsGenericType && outputType.GetGenericArguments() != null && outputType.GetGenericArguments().Any() ? outputType.GetGenericArguments()[0] : outputType;

                                        //    if (baseType == typeof(DateTime))
                                        //        format = format ?? "dd/MM/yyyy";
                                        //    else if (baseType == typeof(decimal))
                                        //        format = format ?? "N2";
                                        //    else
                                        //        tmpOutput = String.Format("{0}", val);

                                        //    var cultureInfo = CultureInfo.GetCultureInfo("it-IT");
                                        //    string outformat = String.IsNullOrWhiteSpace(format) ? "{0}" : "{0:" + format + "}";
                                        //    tmpOutput = String.Format(cultureInfo, outformat, val);
                                        //}

                                        doc.ReplaceText(token, tmpOutput);
                                    }
                                }
                            }
                            break;
                    }
                }
            }

            //return output;
        }

        public static byte[] DocReplaceToken(ISintesi1 sintesi, object entity, byte[] content, string tokenDelimiter = "__", Dictionary<string, object> additionalToken = null)
        {
            MemoryStream ms = new MemoryStream(content);
            DocX doc = DocX.Load(ms);
            DocReplaceToken(sintesi, entity, doc, tokenDelimiter, additionalToken);
            MemoryStream outMs = new MemoryStream();
            doc.SaveAs(outMs);
            return outMs.ToArray();
        }

        private static string FormatObjectValue(object val, Type objType, string format)
        {
            string tmpOutput = "";

            if (val != null)
            {
                Type outputType = objType;
                Type baseType = outputType.IsGenericType && outputType.GetGenericArguments() != null && outputType.GetGenericArguments().Any() ? outputType.GetGenericArguments()[0] : outputType;

                if (baseType == typeof(DateTime))
                    format = format ?? "dd/MM/yyyy";
                else if (baseType == typeof(decimal))
                    format = format ?? "N2";
                else
                    tmpOutput = String.Format("{0}", val);

                var cultureInfo = CultureInfo.GetCultureInfo("it-IT");
                string outformat = String.IsNullOrWhiteSpace(format) ? "{0}" : "{0:" + format + "}";
                tmpOutput = String.Format(cultureInfo, outformat, val);
            }

            return tmpOutput;
        }

        public static void AggiungiNotificaMatr(string testo, string matricola)
        {
            var db = new IncentiviEntities();
            db.XR_HRIS_NOTIFICHE.Add(new XR_HRIS_NOTIFICHE()
            {
                DTA_CREAZIONE = DateTime.Now,
                DEST_MATR = matricola,
                MESSAGE = testo
            });
            DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
        }
        public static void AggiungiNotificaAbil(string testo, string abilFunc, string abilSubFunc=null)
        {
            var db = new IncentiviEntities();
            db.XR_HRIS_NOTIFICHE.Add(new XR_HRIS_NOTIFICHE()
            {
                DTA_CREAZIONE = DateTime.Now,
                DEST_ABIL_FUNC = abilFunc,
                DEST_ABIL_SUBFUNC = abilSubFunc,
                MESSAGE = testo
            });
            DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
        }
    }
}
