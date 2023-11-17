using myRaiData.Incentivi;
using myRaiServiceHub.Autorizzazioni;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace myRaiHelper
{
    internal class AuthControllerScope : SessionScope<AuthControllerScope>
    {
        private Dictionary<string, TemporaryInfo<AbilFunc>> _abilitazioni;

        public Dictionary<string, TemporaryInfo<AbilFunc>> Abilitazioni
        {
            get
            {
                if (_abilitazioni == null)
                    _abilitazioni = new Dictionary<string, TemporaryInfo<AbilFunc>>();
                return _abilitazioni;
            }
        }
    }

    public static class AuthHelper
    {
        public static Dictionary<string, TemporaryInfo<AbilFunc>> GetAbilitazioni()
        {
            return AuthControllerScope.Instance.Abilitazioni;
        }

        public static string HrisFuncAbil(string matricola)
        {
            string hrisAbil = "";
            var param = HrisHelper.GetParametro(HrisParam.HRISAbilFunc);

            if (!String.IsNullOrWhiteSpace(param.COD_VALUE4) && (param.COD_VALUE4 == "*" || param.COD_VALUE4.Contains(matricola)))
                hrisAbil = param.COD_VALUE3;
            else
                hrisAbil = param.COD_VALUE1;

            return hrisAbil;
        }

        private static string GeneraChiave(string matricola, string funzione)
        {
            return matricola + "_" + funzione;
        }
        private static string GeneraChiaveSessione(string use, string matricola, string funzione, string sottofunzione = "")
        {
            return "_ABIL_DB_" + matricola + "_" + funzione + (!String.IsNullOrWhiteSpace(sottofunzione) ? "_" + sottofunzione : "") + "_" + use;
        }
        public static void EnabledFuncs(string matricola)
        {
            var abilFuncs = AuthControllerScope.Instance.Abilitazioni;
            var db = new IncentiviEntities();
            var funzioni = db.XR_HRIS_ABIL_FUNZIONE.Where(x => x.IND_ATTIVO);

            Dictionary<string, bool> hrgaFunc = new Dictionary<string, bool>();

            foreach (var funzione in funzioni)
            {
                if (funzione.NMB_PROVENIENZA != (int)AbilProvenienza.HRGA || HrisHelper.GetOvverideAbil(funzione.COD_FUNZIONE, out Dictionary<string, string> abilitazioni))
                    EnabledTo(matricola, funzione.COD_FUNZIONE);
                else
                {
                    TemporaryInfo<AbilFunc> tmpAbilFunc = null;
                    string chiave = GeneraChiave(matricola, funzione.COD_FUNZIONE);
                    if (!abilFuncs.TryGetValue(chiave, out tmpAbilFunc) || tmpAbilFunc.NeedRefresh())
                        hrgaFunc.Add(funzione.COD_FUNZIONE, funzione.IND_ABIL_INTEGRATION);
                }
            }


            string loadAllHrga = HrisHelper.GetParametro<string>(HrisParam.LoadAllHrga);
            if (loadAllHrga == "TRUE" && hrgaFunc.Any())
            {
                string callHrgaFunc = String.Join("|", hrgaFunc.Select(x => x.Key));
                Sedi servizio = new Sedi();
                servizio.Credentials = CommonHelper.GetUtenteServizioCredentials();
                Funzione_Sottofunzione response = null;

                try
                {
                    response = servizio.Get_FunzioniSottofunzioniAssociati_Net("P" + matricola, callHrgaFunc);
                }
                catch (Exception ex)
                {
                    HrisHelper.LogOperazione("Abilitazioni", "Errore reperimento abilitazioni", false, ex.Message, null, ex);
                }

                bool anyResponse = response != null && response.Cod_Errore == "0"
                    && response.DT_Funzione_Sottofunzione != null && response.DT_Funzione_Sottofunzione.Rows.Count > 0;

                IEnumerable<DataRow> rows = null;
                if (anyResponse)
                    rows = response.DT_Funzione_Sottofunzione.Rows.Cast<DataRow>();

                foreach (var funzione in hrgaFunc)
                {
                    AbilFunc abilFunc = new AbilFunc();
                    abilFunc.Nome = funzione.Key;
                    abilFunc.Matricola = matricola;
                    abilFunc.Provenienza = AbilProvenienza.HRGA;

                    if (anyResponse)
                    {
                        var listSubFunz = rows.Where(x => x.Field<string>("COD_FUNZIONE") == funzione.Key).Select(x => x.Field<string>("COD_SOTTOFUNZIONE"));
                        if (listSubFunz.Any())
                        {
                            abilFunc.State = AbilState.Enabled;
                            foreach (var subFunc in listSubFunz)
                                abilFunc.SubFuncs.Add(subFunc, new AbilSubFunc() { Funzione = funzione.Key, Nome = subFunc, State = AbilState.Enabled, Provenienza = AbilProvenienza.HRGA });
                        }

                        if (abilFunc.State == AbilState.Enabled && funzione.Value)
                            LoadEnabledSubFunc(matricola, funzione.Key, abilFunc, funzione.Value);

                        CheckCRUDSubFunc(abilFunc);
                    }
                    else
                    {
                        abilFunc.State = AbilState.NotEnabled;
                    }

                    SetTemporaryInfoFunc(abilFunc);
                }
            }
        }
        public static bool EnabledTo(string matricola, string funzione)
        {
            AbilFunc abil = null;
            return EnabledTo(matricola, funzione, out abil);
        }
        public static bool EnabledTo(string matricola, string funzione, out AbilFunc abilFunc)
        {
            var abilFuncs = AuthControllerScope.Instance.Abilitazioni;
            abilFunc = null;

            TemporaryInfo<AbilFunc> tmpAbilFunc = null;

            string chiave = GeneraChiave(matricola, funzione);

            if (abilFuncs.TryGetValue(chiave, out tmpAbilFunc) && !tmpAbilFunc.NeedRefresh())
            {
                abilFunc = tmpAbilFunc.GetInfo();
            }
            else
            {
                abilFunc = new AbilFunc();
                abilFunc.Nome = funzione;
                abilFunc.Matricola = matricola;

                if (IsDbAbil(funzione, out bool isIntegration, out bool isFilterIntegration) && !isIntegration)
                {
                    abilFunc.Provenienza = AbilProvenienza.DB;
                    if (LoadEnabledSubFunc(matricola, funzione, abilFunc, false))
                        abilFunc.State = AbilState.Enabled;
                    else
                        abilFunc.State = AbilState.NotEnabled;
                }
                else if (HrisHelper.GetOvverideAbil(funzione, out Dictionary<string, string> abilitazioni))
                {
                    abilFunc.Provenienza = AbilProvenienza.Override;

                    if (abilitazioni.TryGetValue(matricola, out string subFunc))
                    {
                        abilFunc.State = AbilState.Enabled;

                        foreach (var item in subFunc.Split(','))
                        {
                            abilFunc.SubFuncs.Add(item, new AbilSubFunc() { Funzione = funzione, Nome = item, State = AbilState.Enabled, Provenienza = AbilProvenienza.Override });
                        }
                    }
                }
                else
                {
                    abilFunc.Provenienza = AbilProvenienza.HRGA;

                    Sedi servizio = new Sedi();
                    servizio.Credentials = CommonHelper.GetUtenteServizioCredentials();
                    var response = servizio.Autorizzazioni_Net("P" + matricola, funzione, "", "", "", "", "1");
                    if (response != null && response.Cod_Errore == "0"
                        && response.DT_SottofunzioniAssociate != null && response.DT_SottofunzioniAssociate.Rows.Count > 0)
                    {
                        abilFunc.State = AbilState.Enabled;

                        foreach (DataRow item in response.DT_SottofunzioniAssociate.Rows)
                        {
                            string cod = item.Field<string>("COD");
                            abilFunc.SubFuncs.Add(cod, new AbilSubFunc() { Funzione = funzione, Nome = cod, State = AbilState.Enabled, Provenienza = AbilProvenienza.HRGA });
                        }
                    }
                    else
                    {
                        abilFunc.State = AbilState.NotEnabled;
                    }
                }

                if (abilFunc.State == AbilState.Enabled && isIntegration)
                    LoadEnabledSubFunc(matricola, funzione, abilFunc, true);

                CheckCRUDSubFunc(abilFunc);

                SetTemporaryInfoFunc(abilFunc);
            }

            return abilFunc.State == AbilState.Enabled;
        }

        private static void SetTemporaryInfoFunc(AbilFunc abilFunc)
        {
            var abilFuncs = AuthControllerScope.Instance.Abilitazioni;
            string chiave = GeneraChiave(abilFunc.Matricola, abilFunc.Nome);
            abilFuncs.TryGetValue(chiave, out TemporaryInfo<AbilFunc> tmpAbilFunc);

            if (tmpAbilFunc == null)
            {
                tmpAbilFunc = new TemporaryInfo<AbilFunc>();
                tmpAbilFunc.SetInfo(abilFunc);
                abilFuncs.Remove(chiave);
                abilFuncs.AddWithCheck(chiave, tmpAbilFunc);
            }
            else
            {
                tmpAbilFunc.SetInfo(abilFunc);
            }
        }
        private static bool LoadEnabledSubFunc(string matricola, string funzione, AbilFunc abilFunc, bool isIntegration)
        {
            bool anySubFunc = false;
            var list = GetAbilSubFunc(matricola, funzione);
            if (list.Any())
            {
                anySubFunc = true;
                foreach (var item in list)
                {
                    abilFunc.SubFuncs.AddWithCheck(item.Sottofunzione, new AbilSubFunc()
                    {
                        Funzione = funzione,
                        Nome = item.Sottofunzione,
                        State = AbilState.Enabled,
                        IsIntegration = isIntegration,
                        Create = item.Create,
                        Read = item.Read,
                        Update = item.Update,
                        Delete = item.Delete,
                        Provenienza = AbilProvenienza.DB
                    });
                }
            }
            return anySubFunc;
        }

        private static void CheckCRUDSubFunc(AbilFunc abilFunc)
        {
            if (abilFunc.Provenienza == AbilProvenienza.HRGA)
            {
                var db = new IncentiviEntities();
                var funz = db.XR_HRIS_ABIL_FUNZIONE.FirstOrDefault(x => x.COD_FUNZIONE == abilFunc.Nome && x.IND_ATTIVO);
                if (funz != null)
                {
                    foreach (var subFunc in abilFunc.SubFuncs.Where(x => x.Value.Provenienza == AbilProvenienza.HRGA))
                    {
                        var dbSubFunc = funz.XR_HRIS_ABIL_SUBFUNZIONE.FirstOrDefault(x => x.COD_SUBFUNZIONE == subFunc.Key && x.IND_ATTIVO);
                        if (dbSubFunc != null)
                        {
                            subFunc.Value.Create = dbSubFunc.IND_CREATE;
                            subFunc.Value.Read = dbSubFunc.IND_READ;
                            subFunc.Value.Update = dbSubFunc.IND_UPDATE;
                            subFunc.Value.Delete = dbSubFunc.IND_DELETE;
                        }
                    }
                }
            }
        }

        public static List<string> EnabledSubFunc(string matricola, string funzione)
        {
            List<string> result = new List<string>();

            AbilFunc abilFunc;
            if (EnabledTo(matricola, funzione, out abilFunc))
                result.AddRange(abilFunc.SubFuncs.Keys);

            return result;
        }
        public static bool EnabledToSubFunc(string matricola, string funzione, string sottoFunzione)
        {
            AbilSubFunc abilSubFunc = null;
            return EnabledToSubFunc(matricola, funzione, sottoFunzione, out abilSubFunc);
        }
        public static bool EnabledToAnySubFunc(string matricola, string funzione, params string[] sottofunzione)
        {
            bool result = false;

            AbilFunc abilFunc;
            if (EnabledTo(matricola, funzione, out abilFunc))
                result = sottofunzione == null || !sottofunzione.Any() || (abilFunc.SubFuncs != null && abilFunc.SubFuncs.Any(x => sottofunzione.Contains(x.Key)));

            if ((matricola == "103650" ) && funzione =="HRIS_SVIL")
            {
                result = sottofunzione == null || !sottofunzione.Any() || (abilFunc.SubFuncs != null && abilFunc.SubFuncs.Any(x => sottofunzione.Any(z=>x.Key.ToString().Contains(z))));
            }
            return result;
        }
        public static bool EnabledToSubFunc(string matricola, string funzione, string sottoFunzione, out AbilSubFunc abilSubFunc)
        {
            AbilFunc abilFunc = null;
            abilSubFunc = null;

            if (!EnabledTo(matricola, funzione, out abilFunc) || !abilFunc.SubFuncs.TryGetValue(sottoFunzione, out abilSubFunc))
            {
                abilSubFunc = new AbilSubFunc()
                {
                    Funzione = funzione,
                    Nome = sottoFunzione,
                    State = AbilState.NotEnabled
                };
            }

            return abilSubFunc.State == AbilState.Enabled;
        }

        private static bool GetBaseAbil(string matricola, string funzione, string sottofunzione, out BaseAbil abil)
        {
            bool found = false;
            abil = null;

            if (EnabledTo(matricola, funzione, out AbilFunc abilFunc))
            {
                abil = abilFunc;
                if (!String.IsNullOrWhiteSpace(sottofunzione))
                {
                    if (abilFunc.SubFuncs.TryGetValue(sottofunzione, out AbilSubFunc abilSubFunc))
                        abil = abilSubFunc;
                    else
                        abil = null;
                }
            }

            found = abil != null;

            return found;
        }

        private static AbilCat InternalEnabledCategory(string matricola, string funzione, string sottofunzione = "")
        {
            AbilCat abilCat = InternalEnabledFilter<AbilCat>(AbilFilterData.Cat, matricola, funzione, sottofunzione, x => x.CAT_INCLUSE, x => x.CAT_ESCLUSE);
            return abilCat;
        }
        public static AbilCat EnabledCategory(string matricola, string funzione, string sottofunzione = "")
        {
            return GetEnabledData(AbilFilterData.Cat, InternalEnabledCategory, matricola, funzione, sottofunzione);
        }

        private static AbilDir InternalEnabledDirection(string matricola, string funzione, string sottofunzione = "")
        {
            AbilDir abilDir = InternalEnabledFilter<AbilDir>(AbilFilterData.Dir, matricola, funzione, sottofunzione, x => x.DIR_INCLUSE, x => x.DIR_ESCLUSE);

            if (abilDir.HasFilter)
            {
                //*******************************************************************************
                //2021-10-07
                //FIX: Data l'assenza della storicità dei servizi, non è possibile chiudere RaiNet
                //Al momento RaiNet=N2 e RaiCom=N8
                //La situazione corretta dovrebbe essere RaiNet=chiusa e RaiCom=N2
                //Pertanto se in uno delle due liste esiste RaiCom=N8 deve includere N2
                //*******************************************************************************
                abilDir.Filter.PatchData("N8", "N2");
            }

            return abilDir;
        }
        public static AbilDir EnabledDirection(string matricola, string funzione, string sottofunzione = "")
        {
            return GetEnabledData(AbilFilterData.Dir, InternalEnabledDirection, matricola, funzione, sottofunzione);
        }
        public static List<AbilDir> EnabledDirectionBySub(string matricola, string funzione)
        {
            List<AbilDir> result = new List<AbilDir>();

            AbilFunc abilFunc = null;
            if (EnabledTo(matricola, funzione, out abilFunc))
            {
                if (abilFunc.DirezioniAbilitate.NeedRefresh() || abilFunc.DirezioniAbilitate.GetInfo() == null)
                {
                    string sottofunzioni = String.Join("|", abilFunc.SubFuncs.Keys);
                    Sedi servizio = new Sedi();
                    servizio.Credentials = CommonHelper.GetUtenteServizioCredentials();
                    var respSedeServ = servizio.Get_CategoriaDato_Elenco_Net("sedeserv", "P" + matricola, funzione, sottofunzioni);
                    if (respSedeServ != null && respSedeServ.Cod_Errore == "0" && respSedeServ.DT_Utenti_CategorieDatoAbilitate != null)
                    {
                        var tmp = respSedeServ.DT_Utenti_CategorieDatoAbilitate.AsEnumerable()
                            .Select(x => new { SubFunc = x["Codice_sottofunzione"].ToString(), Cod = x["Cod"].ToString() });

                        if (tmp.Any(x => x.Cod == "TUTTO"))
                        {
                            foreach (var item in tmp)
                            {
                                if (item.Cod == "TUTTO")
                                {
                                    AbilSubFunc abilSubFunc = null;
                                    if (abilFunc.SubFuncs.TryGetValue(item.SubFunc, out abilSubFunc))
                                    {
                                        abilSubFunc.DirezioniAbilitate = new TemporaryInfo<AbilDir>();
                                        abilSubFunc.DirezioniAbilitate.SetInfo(new AbilDir()
                                        {
                                            Funzione = funzione,
                                            Sottofunzione = item.SubFunc,
                                            HasFilter = false
                                        });
                                    }
                                }
                            }
                        }
                    }

                    var respServ = servizio.Get_CategoriaDato_Elenco_Net("serv", "P" + matricola, funzione, sottofunzioni);
                    if (respServ != null && respServ.Cod_Errore == "0" && respServ.DT_Utenti_CategorieDatoAbilitate != null)
                    //if (respServ != null && respServ.Cod_Errore == "0" && respSedeServ.DT_Utenti_CategorieDatoAbilitate != null)
                    {
                        var tmp = respServ.DT_Utenti_CategorieDatoAbilitate.AsEnumerable()
                            .Select(x => new { SubFunc = x["Codice_sottofunzione"].ToString(), Cod = x["Cod"].ToString() })
                            .GroupBy(y => y.SubFunc);

                        foreach (var item in tmp)
                        {
                            AbilSubFunc abilSubFunc = null;
                            if (abilFunc.SubFuncs.TryGetValue(item.Key, out abilSubFunc))
                            {
                                abilSubFunc.DirezioniAbilitate = new TemporaryInfo<AbilDir>();
                                var tmpDir = new AbilDir()
                                {
                                    Funzione = funzione,
                                    Sottofunzione = item.Key,
                                    HasFilter = true,
                                };
                                tmpDir.Filter.SetIncluse(item.Select(x => x.Cod).ToList());
                                abilSubFunc.DirezioniAbilitate.SetInfo(tmpDir);
                            }
                        }
                    }

                    abilFunc.DirezioniAbilitate.SetInfo(new AbilDir()
                    {
                        HasFilter = true
                    });
                }

                result.AddRange(abilFunc.SubFuncs.Select(x => x.Value.DirezioniAbilitate.GetInfo()));
            }

            return result;
        }

        private static AbilSedi InternalEnabledSedi(string matricola, string funzione, string sottofunzione = "")
        {
            AbilSedi abilSedi = InternalEnabledFilter<AbilSedi>(AbilFilterData.Sedi, matricola, funzione, sottofunzione, x => x.SEDI_INCLUSE, x => x.SEDI_ESCLUSE);
            return abilSedi;
        }
        public static AbilSedi EnabledSedi(string matricola, string funzione, string sottofunzione = "")
        {
            return GetEnabledData(AbilFilterData.Sedi, InternalEnabledSedi, matricola, funzione, sottofunzione);
        }

        private static AbilSocieta InternalEnabledSocieta(string matricola, string funzione, string sottofunzione = "")
        {
            AbilSocieta abilSocieta = new AbilSocieta();
            bool checkDBAbil = false;

            if (IsDbAbil(funzione, out bool isIntegration, out bool isFilterIntegration) && !isIntegration)
            {
                checkDBAbil = true;
            }
            else
            {
                abilSocieta.HasFilter = false;
            }

            if (checkDBAbil || isFilterIntegration)
                LoadBaseAbilFilters(matricola, funzione, sottofunzione, abilSocieta, isFilterIntegration, x => x.SOC_INCLUSE, x => x.SOC_ESCLUSE);

            if (abilSocieta.HasFilter)
            {
                //    //*******************************************************************************
                //    //2021-10-07
                //    //FIX: Data l'assenza della storicità delle società, non è possibile chiudere RaiNet
                //    //Al momento RaiNet=N e RaiCom=8
                //    //La situazione corretta dovrebbe essere RaiNet=chiusa e RaiCom=N
                //    //Pertanto se in uno delle due liste è stata selezionata RaiCom devo inserire anche N
                //    //*******************************************************************************
                abilSocieta.Filter.PatchData("8", "N");

                //    //*******************************************************************************
                //    //2021-10-07
                //    //FIX: A livello di struttura organizzativa, le società sono indicate con codici diversi. 
                //    //Pertanto, per i filtri sulla str.org, viene fatta qui la decodifica
                //    //*******************************************************************************

                var decodMap = HrisHelper.GetParametriJson<HrisMapSocieta>(HrisParam.DecodSocietaStrOrg);
                if (decodMap != null)
                {
                    abilSocieta.SocietaSezIncluse.AddRange(decodMap.Where(x => abilSocieta.Incluse().Contains(x.Cezanne)).Select(x => x.Struttura));
                    abilSocieta.SocietaSezEscluse.AddRange(decodMap.Where(x => abilSocieta.Escluse().Contains(x.Cezanne)).Select(x => x.Struttura));
                }
            }

            return abilSocieta;
        }
        public static AbilSocieta EnabledSocieta(string matricola, string funzione, string sottofunzione = "")
        {
            return GetEnabledData(AbilFilterData.Societa, InternalEnabledSocieta, matricola, funzione, sottofunzione);
        }

        private static AbilMatr InternalEnabledMatr(string matricola, string funzione, string sottofunzione = "")
        {
            AbilMatr abilMatr = new AbilMatr();
            bool checkDBAbil = false;

            if (IsDbAbil(funzione, out bool isIntegration, out bool isFilterIntegration) && !isIntegration)
            {
                checkDBAbil = true;
            }
            else if (HrisHelper.GetOvverideAbil(funzione, out Dictionary<string, string> abilitazioni))
            {
                //Gli ovveride dovrebbero essere momentanei, quindi per ora non si mettono filtri
                abilMatr.HasFilter = false;
            }
            else
            {
                abilMatr.HasFilter = false;
            }

            if (checkDBAbil || isFilterIntegration)
                LoadBaseAbilFilters(matricola, funzione, sottofunzione, abilMatr, isFilterIntegration, x => x.MATR_INCLUSE, x => x.MATR_ESCLUSE);

            return abilMatr;
        }
        public static AbilMatr EnabledMatr(string matricola, string funzione, string sottofunzione = "")
        {
            return GetEnabledData(AbilFilterData.Matr, InternalEnabledMatr, matricola, funzione, sottofunzione);
        }

        private static AbilTip InternalEnabledTip(string matricola, string funzione, string sottofunzione = "")
        {
            AbilTip abilTip = new AbilTip();
            bool checkDBAbil = false;

            if (IsDbAbil(funzione, out bool isIntegration, out bool isFilterIntegration) && !isIntegration)
            {
                checkDBAbil = true;
            }
            else if (HrisHelper.GetOvverideAbil(funzione, out Dictionary<string, string> abilitazioni))
            {
                //Gli ovveride dovrebbero essere momentanei, quindi per ora non si mettono filtri
                abilTip.HasFilter = false;
            }
            else
            {
                abilTip.HasFilter = false;
            }

            if (checkDBAbil || isFilterIntegration)
                LoadBaseAbilFilters(matricola, funzione, sottofunzione, abilTip, isFilterIntegration, x => x.TIP_INCLUSE, x => x.TIP_ESCLUSE);

            return abilTip;
        }
        public static AbilTip EnabledTip(string matricola, string funzione, string sottofunzione = "")
        {
            return GetEnabledData(AbilFilterData.Tip, InternalEnabledTip, matricola, funzione, sottofunzione);
        }

        private static AbilContr InternalEnabledContr(string matricola, string funzione, string sottofunzione = "")
        {
            AbilContr abilContr = InternalEnabledFilter<AbilContr>(AbilFilterData.Contr, matricola, funzione, sottofunzione, x => x.CONTR_INCLUSI, x => x.CONTR_ESCLUSI);
            return abilContr;
        }
        public static AbilContr EnabledContr(string matricola, string funzione, string sottofunzione = "")
        {
            return GetEnabledData(AbilFilterData.Contr, InternalEnabledContr, matricola, funzione, sottofunzione);
        }

        delegate T InternalGetData<T>(string matricola, string funzione, string sottofunzione);
        private static T GetEnabledData<T>(AbilFilterData data, InternalGetData<T> dataLoader, string matricola, string funzione, string sottofunzione = "") where T : BaseAbilFilter, new()
        {
            T abilData = null;

            if (GetBaseAbil(matricola, funzione, sottofunzione, out BaseAbil abil))
            {
                TemporaryInfo<T> temporary = abil.GetTemporary<T>(data);

                if (temporary.NeedRefresh() || temporary.GetInfo() == null)
                {
                    abilData = dataLoader(matricola, funzione, sottofunzione);
                    temporary.SetInfo(abilData);
                }
                else
                {
                    abilData = temporary.GetInfo();
                }
            }

            if (abilData == null)
            {
                abilData = new T();
                abilData.Enabled = false;
                abilData.HasFilter = true;
            }

            return abilData;
        }
        private static T InternalEnabledFilter<T>(AbilFilterData filter, string matricola, string funzione, string sottofunzione, Func<AbilDBFilter, string> inclSelector, Func<AbilDBFilter, string> esclSelector) where T : BaseAbilFilter, new()
        {
            T abilObj = new T();
            bool checkDBAbil = false;

            if (IsDbAbil(funzione, out bool isIntegration, out bool isFilterIntegration) && !isIntegration)
            {
                abilObj.Origine = AbilOrigine.DB;
                checkDBAbil = true;
            }
            else if (funzione == "HRIS" && HrisHelper.GetParametro<string>(HrisParam.AbilHRISDaCodice) == "TRUE")
            {
                abilObj.Origine = AbilOrigine.Override;
                if (filter == AbilFilterData.Cat)
                {
                    abilObj.HasFilter = true;
                    abilObj.Filter.SetInfo("B,C,D,E,F,G,H,I,K,L,N,Q,R,S,T,V,X,Y,Z,Q10,Q12", "A,A7,M");
                }
                else
                    abilObj.HasFilter = false;
            }
            else if (HrisHelper.GetOvverideAbil(funzione, out Dictionary<string, string> abilitazioni))
            {
                //Gli ovveride dovrebbero essere momentanei, quindi per ora non si mettono filtri
                abilObj.Origine = AbilOrigine.Override;
                abilObj.HasFilter = false;
            }
            else
            {
                abilObj.Origine = AbilOrigine.HRGA;
                AbilFilterTipo tipo = AbilFilterTipo.Nessuno;
                string codice = "";
                string macrocodice = "";
                switch (filter)
                {
                    case AbilFilterData.Cat:
                        tipo = AbilFilterTipo.Livelli;
                        break;
                    case AbilFilterData.Dir:
                        codice = "serv";
                        macrocodice = "sedeserv";
                        tipo = AbilFilterTipo.CategoriaDato;
                        break;
                    case AbilFilterData.Sedi:
                        codice = "sede";
                        macrocodice = "sedeserv";
                        tipo = AbilFilterTipo.CategoriaDato;
                        break;
                    case AbilFilterData.Societa:
                        break;
                    case AbilFilterData.Matr:
                        break;
                    case AbilFilterData.Tip:
                        break;
                    case AbilFilterData.Contr:
                        break;
                    default:
                        throw new NotImplementedException();
                }

                string tmpSottofunzione = sottofunzione;
                if (isIntegration && !String.IsNullOrWhiteSpace(sottofunzione))
                {
                    //In questo caso, la richiesta della sottofunzione potrebbe dare esito negativo perchè abilitato da db e non da HRGA
                    var enabledSubFunc = EnabledToSubFunc(matricola, funzione, sottofunzione, out AbilSubFunc abilSubFunc);
                    if (abilSubFunc.IsIntegration)
                        tmpSottofunzione = "";
                }

                switch (tipo)
                {
                    case AbilFilterTipo.CategoriaDato:
                        InternalGetCategoriaDato(matricola, funzione, tmpSottofunzione, abilObj, codice, macrocodice);
                        break;
                    case AbilFilterTipo.Livelli:
                        Sedi servizio = new Sedi();
                        servizio.Credentials = CommonHelper.GetUtenteServizioCredentials();
                        var response = servizio.Get_LivelliAccesso_Net("P" + matricola, funzione, "", sottofunzione);
                        if (response != null && response.Cod_Errore == "0" && response.DT_LivelliAccesso != null)
                        {
                            EnumerableRowCollection<Tuple<string, string>> resp = response.DT_LivelliAccesso.AsEnumerable().Select(x => new Tuple<string, string>(x.Field<string>("Categoria_Ammessa"), x.Field<string>("Categoria_Esclusa")));

                            var tmp = response.DT_LivelliAccesso.AsEnumerable().GroupBy(x => x.Field<string>("Codice_tipo_dato"));
                            foreach (var item in tmp)
                            {
                                var ammesse = item.SelectMany(x => x.Field<string>("Categoria_ammessa").Split(','));
                                var escluse = item.SelectMany(x => x.Field<string>("Categoria_Esclusa").Split(','));
                                abilObj.Filter.SetInfo(ammesse, escluse, LivFilter.ConvertStrToLiv(item.Key));
                            }

                            abilObj.HasFilter = true;
                        }
                        else
                        {
                            abilObj.HasFilter = true;
                        }
                        break;
                    case AbilFilterTipo.Nessuno:
                        abilObj.HasFilter = false;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            if (checkDBAbil || isFilterIntegration)
                LoadBaseAbilFilters(matricola, funzione, sottofunzione, abilObj, isFilterIntegration, inclSelector, esclSelector);

            return abilObj;
        }

        private static CategorieDatoAbilitate_DT GetCategoriaDato_Session(string codiceCategoriaDato, string matricola, string funzione, string sottofunzione)
        {
            CategorieDatoAbilitate_DT result = null;
            string key = String.Format("_HRGA_CATDATO_{0}_{1}_{2}_{3}_", codiceCategoriaDato, matricola, funzione, sottofunzione);

            result = SessionHelper.Get<CategorieDatoAbilitate_DT>(key, default(CategorieDatoAbilitate_DT));
            if (result == null)
            {
                Sedi servizio = new Sedi();
                servizio.Credentials = CommonHelper.GetUtenteServizioCredentials();
                result = servizio.Get_CategoriaDato_Elenco_Net(codiceCategoriaDato, "P" + matricola, funzione, sottofunzione);
                SessionHelper.Set(key, result);
            }

            return result;
        }

        public static void LoadFromHRGAModel(string matricola, string funzione)
        {
            //Questa funzione permette di caricare i filtri con una sola chiamata
            //Tuttavia per SintesiFilter, i modelli devono essere considerati in OR

            var db = new IncentiviEntities();
            EnabledTo(matricola, funzione, out AbilFunc abilFunc);

            Sedi servizio = new Sedi();
            servizio.Credentials = CommonHelper.GetUtenteServizioCredentials();
            ModelliAssociati response = servizio.Get_ModelliAssociati_Net("P" + matricola, funzione);
            if (response != null && response.ModelliArray!=null)
            {
                foreach (var modello in response.ModelliArray)
                {
                    var funzSubFunc = modello.DT_ModelliFunzioni.AsEnumerable().Select(x => new { Funzione = x.Field<string>("Codice_funzione"), Sottofunzione = x.Field<string>("Codice_sottofunzione") });
                    if (funzSubFunc == null || !funzSubFunc.Any() || funzSubFunc.Any(x => x.Funzione == funzione))
                    {
                        Expression<Func<AbilSubFunc, bool>> subfuncExpr = null;
                        if (funzSubFunc != null)
                        {
                            foreach (var item in funzSubFunc.Where(x => x.Funzione == funzione))
                                subfuncExpr = LinqHelper.PutInOrTogether(subfuncExpr, x => x.Nome == item.Sottofunzione);
                        }
                        if (subfuncExpr == null)
                            subfuncExpr = x => true;

                        Func<AbilSubFunc, bool> subfuncFilter = subfuncExpr.Compile();


                        //Analizzo i livelli di accesso
                        if (modello.DT_LivelliAccesso != null)
                        {
                            var tmp = modello.DT_LivelliAccesso.AsEnumerable().GroupBy(x => x.Field<string>("Codice_tipo_dato"));
                            foreach (var item in tmp)
                            {
                                var elencotipi = item.Select(x => x.Field<string>("Codice_tipologia_dipendente"));
                                var qualFilter = db.XR_HRIS_QUAL_FILTER.Where(x => elencotipi.Contains(x.COD_QUAL_FILTER))
                                                    .Select(x => new { x.QUAL_INCLUDED, x.QUAL_EXCLUDED }).ToList();

                                var ammesse = qualFilter.SelectMany(x => x.QUAL_INCLUDED.Split(','));
                                var escluse = qualFilter.SelectMany(x => x.QUAL_EXCLUDED.Split(','));

                                foreach (var subfunc in abilFunc.SubFuncs.Values.Where(x => subfuncFilter(x)))
                                {
                                    AbilCat abilCat = new AbilCat();
                                    abilCat.HasFilter = true;
                                    abilCat.Filter.SetInfo(ammesse, escluse, LivFilter.ConvertStrToLiv(item.Key));
                                    subfunc.CategorieAbilitate.SetInfo(abilCat);
                                }
                            }
                        }
                        else
                        {
                            foreach (var subfunc in abilFunc.SubFuncs.Values.Where(x => subfuncFilter(x)))
                            {
                                AbilCat abilCat = new AbilCat();
                                abilCat.HasFilter = false;
                                subfunc.CategorieAbilitate.SetInfo(abilCat);
                            }
                        }

                        //Analizzo le categorie di dato
                        bool setSede = false;
                        bool setServ = false;
                        if (modello.DT_CategorieDatoAbilitate != null)
                        {
                            var tmp = modello.DT_CategorieDatoAbilitate.AsEnumerable().GroupBy(x => x.Field<string>("Codice_tipo_categoria_dato"));
                            foreach (var item in tmp)
                            {
                                if (item.Key == "SEDESERV")
                                {
                                    var elencoSedeServ = item.Select(x => x.Field<string>("Codice_categoria_dato")).Distinct();
                                    if (elencoSedeServ.Contains("TUTTO"))
                                    {
                                        //Se ha visibilità totale, non ha senso continuare
                                        break;
                                    }
                                    else
                                    {
                                        //altrimenti imposta sede (i codici saranno nella forma xxyy xx=sede yy=servizio)
                                        setSede = true;
                                        var elencoSedi = item.Select(x => x.Field<string>("Codice_categoria_dato").Trim() + "*").Distinct();
                                        foreach (var subfunc in abilFunc.SubFuncs.Values.Where(x => subfuncFilter(x)))
                                        {
                                            AbilSedi abilSedi = new AbilSedi();
                                            abilSedi.HasFilter = true;
                                            abilSedi.Filter.SetIncluse(elencoSedi);
                                            subfunc.SediAbilitate.SetInfo(abilSedi);
                                        }
                                    }
                                }
                                else if (item.Key == "SERV")
                                {
                                    setServ = true;
                                    var elencoServ = item.Select(x => x.Field<string>("Codice_categoria_dato")).Distinct();
                                    foreach (var subfunc in abilFunc.SubFuncs.Values.Where(x => subfuncFilter(x)))
                                    {
                                        AbilDir abilDir = new AbilDir();
                                        abilDir.HasFilter = true;
                                        abilDir.Filter.SetIncluse(elencoServ);
                                        subfunc.DirezioniAbilitate.SetInfo(abilDir);
                                    }
                                }
                                else if (item.Key == "SEDE")
                                {
                                    setSede = true;
                                    var elencoSedi = item.Select(x => x.Field<string>("Codice_categoria_dato").Trim()+"*").Distinct();
                                    foreach (var subfunc in abilFunc.SubFuncs.Values.Where(x => subfuncFilter(x)))
                                    {
                                        AbilSedi abilSedi = new AbilSedi();
                                        abilSedi.HasFilter = true;
                                        abilSedi.Filter.SetIncluse(elencoSedi);
                                        subfunc.SediAbilitate.SetInfo(abilSedi);
                                    }
                                }
                            }
                        }

                        foreach (var subfunc in abilFunc.SubFuncs.Values.Where(x => subfuncFilter(x)))
                        {
                            if (!setServ)
                            {
                                AbilDir abilDir = new AbilDir();
                                abilDir.HasFilter = false;
                                subfunc.DirezioniAbilitate.SetInfo(abilDir);
                            }

                            if (!setSede)
                            {
                                AbilSedi abilSedi = new AbilSedi();
                                abilSedi.HasFilter = false;
                                subfunc.SediAbilitate.SetInfo(abilSedi);
                            }
                        }

                    }
                }
            }
        }

        private static void InternalGetCategoriaDato<T>(string matricola, string funzione, string sottofunzione, T abilObj, string codice, string macrocodice = "") where T : BaseAbilFilter, new()
        {
            Sedi servizio = new Sedi();
            servizio.Credentials = CommonHelper.GetUtenteServizioCredentials();
            //CategorieDatoAbilitate_DT respSedeServ =  servizio.Get_CategoriaDato_Elenco_Net(macrocodice, "P" + matricola, funzione, sottofunzione);
            CategorieDatoAbilitate_DT respSedeServ = GetCategoriaDato_Session(macrocodice, matricola, funzione, sottofunzione);
            if (respSedeServ != null && respSedeServ.Cod_Errore == "0")
            {
                bool all = false;
                if (respSedeServ.DT_Utenti_CategorieDatoAbilitate != null)
                {
                    var tmp = respSedeServ.DT_Utenti_CategorieDatoAbilitate.AsEnumerable()
                        .Select(x => new { SubFunc = x["Codice_sottofunzione"].ToString(), Cod = x["Cod"].ToString() });

                    if (tmp.Any(x => x.Cod == "TUTTO"))
                    {
                        all = true;
                        abilObj.HasFilter = false;
                    }
                    //Se non ha visibilità totale, quindi sono presenti dei filtri,
                    //devono essere impostati solo se il codice="sede" (perchè arrivano nella forma
                    //{sede}{serv}) o se il codice è vuoto (perchè in quel caso non fa la doppia chiamata)
                    else if (tmp.Any() && (String.IsNullOrWhiteSpace(codice) || codice == "sede"))
                    {
                        abilObj.HasFilter = true;
                        abilObj.Filter.SetIncluse(tmp.Select(x => x.Cod).Distinct());
                    }
                }

                if (!all && !String.IsNullOrWhiteSpace(codice))
                {
                    //var respServ = servizio.Get_CategoriaDato_Elenco_Net(codice, "P" + matricola, funzione, sottofunzione);
                    CategorieDatoAbilitate_DT respServ = GetCategoriaDato_Session(codice, matricola, funzione, sottofunzione);
                    if (respServ != null && respServ.Cod_Errore == "0")
                    {
                        IEnumerable<string> listValueCod = null;
                        if (respServ.DT_Utenti_CategorieDatoAbilitate != null)
                            listValueCod = respServ.DT_Utenti_CategorieDatoAbilitate.AsEnumerable()
                                .Select(x => x["Cod"].ToString() + (codice == "sede" ? "*" : "")).Distinct();

                        abilObj.HasFilter = listValueCod != null && listValueCod.Any();
                        if (abilObj.HasFilter)
                            abilObj.Filter.SetIncluse(listValueCod);
                    }
                }
            }
        }

        public static AbilInfo EnabledInfo(string matricola, string funzione, string sottofunzione = "")
        {
            AbilInfo abilInfo = new AbilInfo();

            AbilFunc abilFunc = null;
            if (EnabledTo(matricola, funzione, out abilFunc))
            {
                SessionHelper.Set(GeneraChiaveSessione("USE", matricola, funzione, sottofunzione), true);
                abilInfo.AbilCat = EnabledCategory(matricola, funzione, sottofunzione);
                abilInfo.AbilDir = EnabledDirection(matricola, funzione, sottofunzione);
                abilInfo.AbilSedi = EnabledSedi(matricola, funzione, sottofunzione);
                abilInfo.AbilSocieta = EnabledSocieta(matricola, funzione, sottofunzione);
                abilInfo.AbilMatr = EnabledMatr(matricola, funzione, sottofunzione);
                abilInfo.AbilTip = EnabledTip(matricola, funzione, sottofunzione);
                abilInfo.AbilContr = EnabledContr(matricola, funzione, sottofunzione);
                SessionHelper.Set(GeneraChiaveSessione("USE", matricola, funzione, sottofunzione), false);
                SessionHelper.Set(GeneraChiaveSessione("DATA", matricola, funzione, sottofunzione), null);
            }
            else
            {
                abilInfo.AbilCat = new AbilCat() { HasFilter = true };
                abilInfo.AbilDir = new AbilDir() { HasFilter = true };
                abilInfo.AbilSedi = new AbilSedi() { HasFilter = true };
                abilInfo.AbilSocieta = new AbilSocieta() { HasFilter = true };
                abilInfo.AbilMatr = new AbilMatr() { HasFilter = true };
                abilInfo.AbilTip = new AbilTip() { HasFilter = true };
                abilInfo.AbilContr = new AbilContr() { HasFilter = true };
            }

            return abilInfo;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matrCaller"></param>
        /// <param name="matrCalled"></param>
        /// <param name="funzione"></param>
        /// <param name="sottofunzione"></param>
        /// <param name="FRANCESCO">Aggiunta la chiamata ad HRGA per gestire i livelli anche nel caso di 
        /// IsDbAbil true</param>
        /// <returns></returns>
        private static AbilMatrLiv InternalEnabledMatrInfo(string matrCaller, string matrCalled, string funzione, string sottofunzione = "")
        {
            AbilMatrLiv abilMatr = new AbilMatrLiv();
            abilMatr.Matricola = matrCalled;

            //if (IsDbAbil(funzione, out bool isIntegration, out bool isFilterIntegration) && (!isIntegration || isFilterIntegration))
            //Il controllo cambia in quanto ora sono mappate anche le funzioni di HRGA sul DB. 
            //Quindi IsDbABil in quel caso, se la provenienza è impostata come HRGA, restituisce false
            if (IsDbAbil(funzione, out bool isIntegration, out bool isFilterIntegration) || isFilterIntegration)
            {
                AbilInfo abilInfo = EnabledInfo(matrCaller, funzione, sottofunzione);
                AbilCat enabledCat = abilInfo.AbilCat;
                AbilDir enabledSer = abilInfo.AbilDir;
                AbilSedi enabledSed = abilInfo.AbilSedi;
                AbilSocieta enabledSoc = abilInfo.AbilSocieta;
                AbilMatr enabledMatr = abilInfo.AbilMatr;
                AbilContr enabledContr = abilInfo.AbilContr;

                var db = new myRaiData.Incentivi.IncentiviEntities();
                var ris = db.SINTESI1.Where(x => x.COD_MATLIBROMAT == matrCalled);
                ris = SintesiFilter(ris, matrCaller, "", funzione, sottofunzione);

                bool hasFilter = enabledCat.HasFilter || enabledSer.HasFilter || enabledSed.HasFilter || enabledSoc.HasFilter || enabledMatr.HasFilter;

                if (ris.Any())
                {
                    // 
                    Sedi hrga = new Sedi();
                    hrga.Credentials = CommonHelper.GetUtenteServizioCredentials();
                    AutorizzazioniResponse auth = hrga.Autorizzazioni_Net("P" + matrCaller, funzione, sottofunzione, matrCalled, "", "", "1");
                    abilMatr.Enabled = true;

                    if (auth != null && auth.Abilitato_MacroFunzione)
                    {
                        abilMatr.LivelloAnagrafico = auth.LivelloAccesso_Anagrafica;
                        abilMatr.LivelloGestionale = auth.LivelloAccesso_Gestionale;
                        abilMatr.LivelloRetributivo = auth.LivelloAccesso_Retributiva;
                        abilMatr.Visibilita = AbilMatrLiv.VisibilitaEnum.HRGA;
                    }
                    else
                    {
                        abilMatr.Visibilita = hasFilter ? AbilMatrLiv.VisibilitaEnum.Filtrata : AbilMatrLiv.VisibilitaEnum.Totale;
                    }

                    ////Per il momento non sono gestite differenze a livelli
                    //abilMatr.Enabled = true;
                    //abilMatr.LivelloAnagrafico = true;
                    //abilMatr.LivelloGestionale = true;
                    //abilMatr.LivelloRetributivo = true;
                    //abilMatr.Visibilita = hasFilter ? AbilMatrLiv.VisibilitaEnum.Filtrata : AbilMatrLiv.VisibilitaEnum.Totale;
                }
            }
            else if (funzione == "HRIS" && HrisHelper.GetParametro<string>(HrisParam.AbilHRISDaCodice) == "TRUE")
            {
                //Provvisoriamente salta il controllo
                abilMatr.Enabled = true;
                abilMatr.LivelloAnagrafico = true;
                abilMatr.LivelloGestionale = true;
                if (matrCaller == "103650" || matrCaller == "909317")
                    abilMatr.LivelloRetributivo = true;
            }
            else if (HrisHelper.GetOvverideAbil(funzione, out Dictionary<string, string> abilitazioni))
            {
                //Gli ovveride dovrebbero essere momentanei, quindi per ora non si mettono filtri
                abilMatr.Enabled = true;
                abilMatr.LivelloAnagrafico = true;
                abilMatr.LivelloGestionale = true;
                if (matrCaller == "103650" || matrCaller == "909317")
                    abilMatr.LivelloRetributivo = true;
            }
            else
            {
                Sedi hrga = new Sedi();
                hrga.Credentials = CommonHelper.GetUtenteServizioCredentials();
                AutorizzazioniResponse auth = hrga.Autorizzazioni_Net("P" + matrCaller, funzione, sottofunzione, matrCalled, "", "", "1");

                if (auth != null && auth.Abilitato_MacroFunzione)
                {
                    abilMatr.Enabled = true;
                    abilMatr.LivelloAnagrafico = auth.LivelloAccesso_Anagrafica;
                    abilMatr.LivelloGestionale = auth.LivelloAccesso_Gestionale;
                    abilMatr.LivelloRetributivo = auth.LivelloAccesso_Retributiva;
                    abilMatr.Visibilita = AbilMatrLiv.VisibilitaEnum.HRGA;
                }
            }

            return abilMatr;
        }
        public static AbilMatrLiv EnableToMatr(string matrCaller, string matrCalled, string funzione, string sottofunzione = "")
        {
            TemporaryInfo<AbilMatrLiv> abilMatr = null;
            AbilMatrLiv abilMatrInfo = null;

            if (GetBaseAbil(matrCaller, funzione, sottofunzione, out BaseAbil abil))
            {
                if (abil.MatricoleLivInfo.TryGetValue(matrCalled, out abilMatr))
                {
                    if (abilMatr.NeedRefresh() || abilMatr.GetInfo() == null)
                    {
                        abilMatrInfo = InternalEnabledMatrInfo(matrCaller, matrCalled, funzione);
                        abilMatr.SetInfo(abilMatrInfo);
                    }
                    else
                    {
                        abilMatrInfo = abilMatr.GetInfo();
                    }
                }
                else
                {
                    abilMatrInfo = InternalEnabledMatrInfo(matrCaller, matrCalled, funzione, sottofunzione);
                    abilMatr = new TemporaryInfo<AbilMatrLiv>();
                    abilMatr.SetInfo(abilMatrInfo);
                    abil.MatricoleLivInfo.AddWithCheck(matrCalled, abilMatr);
                }
            }

            if (abilMatrInfo == null)
            {
                abilMatrInfo = new AbilMatrLiv();
                abilMatrInfo.Enabled = false;
            }

            return abilMatrInfo;
        }

        public static List<NominativoMatricola> GetAllEnabledAs(string funzione, string sottofunzione = "", bool escludiDelegati = false)
        {
            List<NominativoMatricola> list = new List<NominativoMatricola>();

            if (IsDbAbil(funzione, out bool isIntegration, out bool isFilterIntegration) && !isIntegration)
            {
                list = GetMatrAbilSubFunc(funzione, sottofunzione, escludiDelegati);
            }
            else if (HrisHelper.GetOvverideAbil(funzione, out Dictionary<string, string> abilitazioni))
            {
                var db = new IncentiviEntities();

                List<string> matricole = new List<string>();
                if (abilitazioni != null && abilitazioni.Any())
                {
                    matricole = abilitazioni.Where(w => w.Value.Split(',').ToList().Contains(sottofunzione)).Select(w => w.Key).ToList();
                }

                if (matricole != null && matricole.Any())
                {
                    foreach (var m in matricole)
                    {
                        SINTESI1 sint = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == m);

                        if (sint != null)
                        {
                            list.Add(new NominativoMatricola()
                            {
                                Matricola = m.Replace("P", "").Trim(),
                                Cognome = sint.DES_COGNOMEPERS.Trim(),
                                Nome = sint.DES_NOMEPERS.Trim()
                            });
                        }
                    }
                }
            }
            else
            {
                Sedi serv = new Sedi();
                serv.Credentials = CommonHelper.GetUtenteServizioCredentials();
                UtentiAssociati response = serv.Get_UtentiAssociati_Funzioni_Net(funzione, sottofunzione);


                if (response != null && response.Cod_Errore == "0" && response.DT_UtentiAssociati != null
                    && response.DT_UtentiAssociati.Rows != null && response.DT_UtentiAssociati.Rows.Count > 0)
                {
                    foreach (System.Data.DataRow r in response.DT_UtentiAssociati.Rows)
                    {
                        if (r.ItemArray != null && r.ItemArray.Length >= 3)
                        {
                            list.Add(new NominativoMatricola()
                            {
                                Matricola = r.ItemArray[0].ToString().Replace("P", "").Trim(),
                                Cognome = r.ItemArray[1].ToString(),
                                Nome = r.ItemArray[2].ToString()
                            });
                        }
                    }
                }
            }

            return list;

        }
        public static List<NominativoMatricola> GetAllEnabledAsOffice(string funzione, string ufficio)
        {
            List<NominativoMatricola> list = new List<NominativoMatricola>();

            if (IsDbAbil(funzione, out bool isIntegration, out bool isFilterIntegration) && !isIntegration)
                list = GetMatrAbilOffice(funzione, ufficio);
            //Questa informazione si può reperire solo per le abilitazioni su db

            return list;

        }
        public static List<NominativoMatricola> GetAllEnabledFromDBAs(string funzione, string sottofunzione, Dictionary<string, string> abilitazioni)
        {
            // 103650|ADM,02GEST,02ADM;
            // 909317 | ADM,02GEST;
            // 912685 | 01GEST;
            // 652740 | 01GEST,01ADM;
            // 451598 | 03GEST;
            // 332783 | 01GEST,01ADM;
            var db = new IncentiviEntities();

            List<NominativoMatricola> list = new List<NominativoMatricola>();
            List<string> matricole = new List<string>();
            if (abilitazioni != null && abilitazioni.Any())
            {
                matricole = abilitazioni.Where(w => w.Value.Split(',').ToList().Contains(sottofunzione)).Select(w => w.Key).ToList();
            }

            if (matricole != null && matricole.Any())
            {
                foreach (var m in matricole)
                {
                    SINTESI1 sint = db.SINTESI1.FirstOrDefault(x => x.COD_MATLIBROMAT == m);

                    if (sint != null)
                    {
                        list.Add(new NominativoMatricola()
                        {
                            Matricola = m.Replace("P", "").Trim(),
                            Cognome = sint.DES_COGNOMEPERS.Trim(),
                            Nome = sint.DES_NOMEPERS.Trim()
                        });
                    }
                }
            }

            return list;

        }

        /// <summary>
        /// Restituisce se la funzione è presente sul db e la provenienza non è HRGA
        /// </summary>
        /// <param name="funzione">Nome dela funzione</param>
        /// <param name="isIntegration">Se la provenienza è HRGA, integra le sottofunzioni dal DB</param>
        /// <param name="isFilterIntegration">Se la provenienza è HRGA, integra i filtri dal DB</param>
        /// <returns></returns>
        private static bool IsDbAbil(string funzione, out bool isIntegration, out bool isFilterIntegration)
        {
            bool result = false;
            isIntegration = false;
            isFilterIntegration = false;
            IncentiviEntities db = new IncentiviEntities();
            var funz = db.XR_HRIS_ABIL_FUNZIONE.FirstOrDefault(x => x.COD_FUNZIONE == funzione && x.IND_ATTIVO);
            result = funz != null && funz.NMB_PROVENIENZA != (int)AbilProvenienza.HRGA;
            isIntegration = funz != null && funz.IND_ABIL_INTEGRATION;
            isFilterIntegration = funz != null && funz.IND_FILTER_INTEGRATION;

            return result;
        }

        private static List<NominativoMatricola> GetMatrAbilSubFunc(string funzione, string sottofunzione = "", bool escludiDelegati = false)
        {
            List<NominativoMatricola> result = new List<NominativoMatricola>();
            var db = new IncentiviEntities();

            var list = db.XR_HRIS_ABIL_SUBFUNZIONE.Where(x => x.IND_ATTIVO && x.XR_HRIS_ABIL_FUNZIONE.IND_ATTIVO && x.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == funzione);
            if (!String.IsNullOrWhiteSpace(sottofunzione))
                list = list.Where(x => x.COD_SUBFUNZIONE == sottofunzione);

            DateTime today = DateTime.Today;
            foreach (var subfunz in list)
            {
                var tmpElenco = subfunz.XR_HRIS_ABIL.Where(x => x.IND_ATTIVO && (x.DTA_INIZIO == null || x.DTA_INIZIO <= today) && (x.DTA_FINE == null || x.DTA_FINE >= today));
                if (escludiDelegati)
                    tmpElenco = tmpElenco.Where(x => !x.ID_DELEGA.HasValue);

                var elencoMatr = tmpElenco.Select(x => x.MATRICOLA);
                result.AddRange(db.SINTESI1.Where(x => elencoMatr.Contains(x.COD_MATLIBROMAT) && x.DTA_FINE_CR != null && x.DTA_FINE_CR > DateTime.Today)
                             .Select(x => new { x.COD_MATLIBROMAT, x.DES_COGNOMEPERS, x.DES_NOMEPERS }).ToList()
                             .Select(y => new NominativoMatricola() { Matricola = y.COD_MATLIBROMAT, Cognome = y.DES_COGNOMEPERS, Nome = y.DES_NOMEPERS }));

                if (subfunz.XR_HRIS_ABIL_PROFILO_ASSOCIAZIONI.Any())
                {
                    foreach (var grAssoc in subfunz.XR_HRIS_ABIL_PROFILO_ASSOCIAZIONI.GroupBy(x => x.ID_PROFILO))
                        result.AddRange(GetMatrAbilPerProfilo(db, grAssoc.First().XR_HRIS_ABIL_PROFILO));
                }
            }

            return result;
        }
        private static List<NominativoMatricola> GetMatrAbilOffice(string funzione, string ufficio)
        {
            List<NominativoMatricola> result = new List<NominativoMatricola>();
            var db = new IncentiviEntities();

            var list = db.XR_HRIS_ABIL_SUBFUNZIONE.Where(x => x.IND_ATTIVO && x.XR_HRIS_ABIL_FUNZIONE.IND_ATTIVO && x.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == funzione && x.NOT_UFFICIO == ufficio);

            DateTime today = DateTime.Today;
            foreach (var subfunz in list)
            {
                var elencoMatr = subfunz.XR_HRIS_ABIL.Where(x => x.IND_ATTIVO && (x.DTA_INIZIO == null || x.DTA_INIZIO <= today) && (x.DTA_FINE == null || x.DTA_FINE >= today)).Select(x => x.MATRICOLA);
                result.AddRange(db.SINTESI1.Where(x => elencoMatr.Contains(x.COD_MATLIBROMAT) && x.DTA_FINE_CR != null && x.DTA_FINE_CR > DateTime.Today)
                             .Select(x => new { x.COD_MATLIBROMAT, x.DES_COGNOMEPERS, x.DES_NOMEPERS }).ToList()
                             .Select(y => new NominativoMatricola() { Matricola = y.COD_MATLIBROMAT, Cognome = y.DES_COGNOMEPERS, Nome = y.DES_NOMEPERS }));

                if (subfunz.XR_HRIS_ABIL_PROFILO_ASSOCIAZIONI.Any())
                {
                    foreach (var grAssoc in subfunz.XR_HRIS_ABIL_PROFILO_ASSOCIAZIONI.GroupBy(x => x.ID_PROFILO))
                        result.AddRange(GetMatrAbilPerProfilo(db, grAssoc.First().XR_HRIS_ABIL_PROFILO));
                }
            }

            return result;
        }
        private static List<NominativoMatricola> GetMatrAbilPerProfilo(IncentiviEntities db, XR_HRIS_ABIL_PROFILO profilo)
        {
            List<NominativoMatricola> result = new List<NominativoMatricola>();

            DateTime today = DateTime.Today;
            var abilitazioni = db.XR_HRIS_ABIL.Where(x => x.ID_PROFILO == profilo.ID_PROFILO && x.IND_ATTIVO && (x.DTA_INIZIO == null || x.DTA_INIZIO <= today) && (x.DTA_FINE == null || x.DTA_FINE >= today));
            if (abilitazioni != null)
            {
                var elencoMatr = abilitazioni.Select(x => x.MATRICOLA);
                db.SINTESI1.Where(x => elencoMatr.Contains(x.COD_MATLIBROMAT) && x.DTA_FINE_CR != null && x.DTA_FINE_CR > DateTime.Today)
                             .Select(x => new { x.COD_MATLIBROMAT, x.DES_COGNOMEPERS, x.DES_NOMEPERS }).ToList()
                             .Select(y => new NominativoMatricola() { Matricola = y.COD_MATLIBROMAT, Cognome = y.DES_COGNOMEPERS, Nome = y.DES_NOMEPERS }).ToList();
            }
            if (profilo.XR_HRIS_ABIL_PROFILO_ASSOCIAZIONI1 != null)
            {
                foreach (var ancProfilo in profilo.XR_HRIS_ABIL_PROFILO_ASSOCIAZIONI1)
                    result.AddRange(GetMatrAbilPerProfilo(db, ancProfilo.XR_HRIS_ABIL_PROFILO));
            }

            return result;
        }

        private static List<AbilDBFilter> GetAbilFilters(string matricola, string funzione, string sottofunzione = "")
        {
            List<AbilDb> abilDb = GetAbilSubFunc(matricola, funzione, sottofunzione, true);

            bool loadPers = false;
            List<XR_HRIS_ABIL_MODELLO> persModel = null;

            List<AbilDBFilter> filters = new List<AbilDBFilter>();
            foreach (var item in abilDb)
                filters.AddRange(SingleGetAbilFilters(matricola, ref loadPers, ref persModel, item, out bool isModelAbli));

            if (filters.Any(x => !String.IsNullOrWhiteSpace(x.GR_CATEGORIE)))
            {
                IncentiviEntities db = new IncentiviEntities();
                IEnumerable<int> idQualFilter = filters.Where(x => !String.IsNullOrWhiteSpace(x.GR_CATEGORIE))
                                                    .Select(x => x.GR_CATEGORIE)
                                                    .Distinct()
                                                    .SelectMany(x => x.Split(','))
                                                    .Distinct()
                                                    .Select(x => Convert.ToInt32(x));

                Dictionary<int, XR_HRIS_QUAL_FILTER> dictQualFilter = db.XR_HRIS_QUAL_FILTER.Where(x => idQualFilter.Contains(x.ID_QUAL_FILTER)).ToDictionary(x => x.ID_QUAL_FILTER);
                foreach (var filter in filters.Where(x => !String.IsNullOrWhiteSpace(x.GR_CATEGORIE)))
                {
                    foreach (int qualFilter in filter.GR_CATEGORIE.Split(',').Select(x => Convert.ToInt32(x)))
                    {
                        if ((dictQualFilter[qualFilter].QUAL_INCLUDED ?? "*") != "*")
                            filter.CAT_INCLUSE = filter.CAT_INCLUSE + (!String.IsNullOrWhiteSpace(filter.CAT_INCLUSE) ? "," : "") + dictQualFilter[qualFilter].QUAL_INCLUDED;

                        if ((dictQualFilter[qualFilter].QUAL_EXCLUDED ?? "*") != "*")
                            filter.CAT_ESCLUSE = filter.CAT_ESCLUSE + (!String.IsNullOrWhiteSpace(filter.CAT_ESCLUSE) ? "," : "") + dictQualFilter[qualFilter].QUAL_EXCLUDED;
                    }
                }
            }

            if (filters.Any(x => !String.IsNullOrWhiteSpace(x.GR_AREA)))
            {
                IncentiviEntities db = new IncentiviEntities();
                IEnumerable<int> idAreaFilter = filters.Where(x => !String.IsNullOrWhiteSpace(x.GR_AREA))
                                                        .Select(x => x.GR_AREA)
                                                        .Distinct()
                                                        .SelectMany(x => x.Split(','))
                                                        .Distinct()
                                                        .Select(x => Convert.ToInt32(x));

                Dictionary<int, XR_HRIS_DIR_FILTER> dictDirFilter = db.XR_HRIS_DIR_FILTER.Where(x => idAreaFilter.Contains(x.ID_AREA_FILTER)).ToDictionary(x => x.ID_AREA_FILTER);
                foreach (var filter in filters.Where(x => !String.IsNullOrWhiteSpace(x.GR_AREA)))
                {
                    foreach (var dirFilter in filter.GR_AREA.Split(',').Select(x => Convert.ToInt32(x)))
                    {
                        if ((dictDirFilter[dirFilter].DIR_INCLUDED ?? "*") != "*")
                            filter.DIR_INCLUSE = filter.DIR_INCLUSE + (!String.IsNullOrWhiteSpace(filter.DIR_INCLUSE) ? "," : "") + dictDirFilter[dirFilter].DIR_INCLUDED;

                        if ((dictDirFilter[dirFilter].DIR_EXCLUDED ?? "*") != "*")
                            filter.DIR_ESCLUSE = filter.DIR_ESCLUSE + (!String.IsNullOrWhiteSpace(filter.DIR_ESCLUSE) ? "," : "") + dictDirFilter[dirFilter].DIR_EXCLUDED;
                    }
                }
            }

            return filters;
        }
        private static List<AbilDBFilter> SingleGetAbilFilters(string matricola, ref bool loadPers, ref List<XR_HRIS_ABIL_MODELLO> persModel, AbilDb item, out bool isModelAbil)
        {
            isModelAbil = false;
            List<AbilDBFilter> temp = new List<AbilDBFilter>();
            AbilDBFilter abilFilter = AbilDBFilter.FromAbil(item.Abil);
            if (!abilFilter.IsEmpty())
                temp.Add(abilFilter);
            else if (item.Abil.ID_MODELLO != null && item.Abil.XR_HRIS_ABIL_MODELLO.IND_ATTIVO)
                temp.Add(AbilDBFilter.FromModelAbil(item.Abil.XR_HRIS_ABIL_MODELLO));
            else if (item.Abil.XR_HRIS_ABIL_ASSOC_MODELLO.Any(x => x.XR_HRIS_ABIL_MODELLO.IND_ATTIVO))
            {
                isModelAbil = true;
                temp.AddRange(item.Abil.XR_HRIS_ABIL_ASSOC_MODELLO.Where(x => x.XR_HRIS_ABIL_MODELLO.IND_ATTIVO).Select(x => AbilDBFilter.FromModelAbil(x.XR_HRIS_ABIL_MODELLO)));
            }
            else
            {
                if (!loadPers)
                {
                    loadPers = true;
                    persModel = LoadPersModel(matricola);
                }
                temp.AddRange(persModel.Select(x => AbilDBFilter.FromModelPers(x)));
            }

            return temp;
        }

        private static void LoadBaseAbilFilters(string matricola, string funzione, string sottofunzione, BaseAbilFilter abilObj, bool isFilterIntegration, Func<AbilDBFilter, string> inclSelector, Func<AbilDBFilter, string> esclSelector)
        {
            List<AbilDBFilter> rec = GetAbilFilters(matricola, funzione, sottofunzione);
            if (rec.Any())
            {
                if (isFilterIntegration)
                {
                    abilObj.Origine = AbilOrigine.Integrazione;
                    abilObj.OriginalHasFilter = abilObj.HasFilter;
                    abilObj.Filter.CopyTo(abilObj.OrigFilter);
                }

                var dbIncl = rec.Select(inclSelector).Distinct().Where(x => x != null).ToList().SelectMany(x => x.Split(',')).Distinct();
                var dbExcl = rec.Select(esclSelector).Distinct().Where(x => x != null).ToList().SelectMany(x => x.Split(',')).Distinct().Where(y => !dbIncl.Contains(y));

                abilObj.Filter.SetInfo(dbIncl, dbExcl);
                abilObj.HasFilter = abilObj.Incluse().Count() > 0 || abilObj.Escluse().Count() > 0;
            }
            else if (!isFilterIntegration)
                abilObj.HasFilter = false;
        }

        private static List<XR_HRIS_ABIL_MODELLO> LoadPersModel(string matricola)
        {
            List<XR_HRIS_ABIL_MODELLO> persModel = new List<XR_HRIS_ABIL_MODELLO>();
            using (IncentiviEntities db = new IncentiviEntities())
            {
                DateTime today = DateTime.Today;
                persModel.AddRange(db.XR_HRIS_ABIL.Where(x => x.MATRICOLA == matricola && x.IND_ATTIVO && (x.DTA_INIZIO == null || x.DTA_INIZIO <= today) && (x.DTA_FINE == null || x.DTA_FINE >= today)
                                                            && x.ID_PROFILO == null && x.ID_SUBFUNZ == null && x.XR_HRIS_ABIL_ASSOC_MODELLO.Any())
                                            .SelectMany(x => x.XR_HRIS_ABIL_ASSOC_MODELLO.Where(y => y.XR_HRIS_ABIL_MODELLO.IND_ATTIVO)
                                                                    .Select(y => y.XR_HRIS_ABIL_MODELLO)));
            }

            return persModel;
        }

        private static List<AbilDb> GetAbilSubFunc(string matricola, string funzione, string sottofunzione = "", bool isForFilter = false)
        {
            List<AbilDb> result = new List<AbilDb>();

            bool useCache = SessionHelper.Get(GeneraChiaveSessione("USE", matricola, funzione, sottofunzione), () => { return false; });
            List<AbilDb> recCache = SessionHelper.Get<List<AbilDb>>(GeneraChiaveSessione("DATA", matricola, funzione, sottofunzione), () => { return null; });

            if (!useCache || recCache == null)
            {
                Console.WriteLine("GetAbilSubFunc");
                var db = new IncentiviEntities();
                DateTime today = DateTime.Today;
                //verifico se presente un'abilitazione senza profilo a quella funzione/sottofunzione
                var listDirect = db.XR_HRIS_ABIL
                                   .Include("XR_HRIS_ABIL_SUBFUNZIONE")
                                   .Include("XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE")
                                   .Where(x => x.MATRICOLA == matricola && x.IND_ATTIVO && (x.DTA_INIZIO == null || x.DTA_INIZIO <= today) && (x.DTA_FINE == null || x.DTA_FINE >= today)
                                                && x.ID_SUBFUNZ != null && x.XR_HRIS_ABIL_SUBFUNZIONE.IND_ATTIVO
                                                && x.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == funzione && x.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.IND_ATTIVO);

                if (!String.IsNullOrWhiteSpace(sottofunzione))
                    listDirect = listDirect.Where(x => x.XR_HRIS_ABIL_SUBFUNZIONE.COD_SUBFUNZIONE == sottofunzione);
                else if (isForFilter)
                    listDirect = listDirect.Where(x => !x.XR_HRIS_ABIL_SUBFUNZIONE.IND_NOFILTERS);

                result.AddRange(listDirect.Select(x => new AbilDb()
                {
                    Abil = x,
                    Funzione = x.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE,
                    Sottofunzione = x.XR_HRIS_ABIL_SUBFUNZIONE.COD_SUBFUNZIONE,
                    Profilo = null,
                    DbSubFunc = x.XR_HRIS_ABIL_SUBFUNZIONE
                }));

                //verifico se è abilitato a qualche profilo
                var listAbilProfili = db.XR_HRIS_ABIL
                                   .Include("XR_HRIS_ABIL_PROFILO")
                                   .Where(x => x.MATRICOLA == matricola && x.IND_ATTIVO && (x.DTA_INIZIO == null || x.DTA_INIZIO <= today) && (x.DTA_FINE == null || x.DTA_FINE >= today)
                                                && x.ID_PROFILO != null && x.XR_HRIS_ABIL_PROFILO.IND_ATTIVO);

                foreach (var abilProfilo in listAbilProfili)
                    result.AddRange(GetAbilSubFuncPerProfilo(db, abilProfilo, abilProfilo.XR_HRIS_ABIL_PROFILO, funzione, sottofunzione));

                if (useCache)
                    SessionHelper.Set(GeneraChiaveSessione("DATA", matricola, funzione, sottofunzione), result);
            }
            else
                result.AddRange(recCache);

            return result;
        }
        private static List<AbilDb> GetAbilSubFuncPerProfilo(IncentiviEntities db, XR_HRIS_ABIL dbAbil, XR_HRIS_ABIL_PROFILO profilo, string funzione, string sottofunzione = "")
        {
            List<AbilDb> result = new List<AbilDb>();

            //var assocFunc = profilo.XR_HRIS_ABIL_PROFILO_ASSOCIAZIONI.Where(x => x.IND_ATTIVO
            //                                                                    && x.ID_SUBFUNZ != null && x.XR_HRIS_ABIL_SUBFUNZIONE.IND_ATTIVO
            //                                                                    && x.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == funzione && x.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.IND_ATTIVO);
            var assocFunc = db.XR_HRIS_ABIL_PROFILO_ASSOCIAZIONI
                                .Include("XR_HRIS_ABIL_PROFILO")
                                .Include("XR_HRIS_ABIL_SUBFUNZIONE")
                                .Include("XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE")
                                .Where(x => x.ID_PROFILO == profilo.ID_PROFILO && x.IND_ATTIVO
                                                && x.ID_SUBFUNZ != null && x.XR_HRIS_ABIL_SUBFUNZIONE.IND_ATTIVO
                                                && x.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == funzione
                                                && x.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.IND_ATTIVO);

            if (!String.IsNullOrWhiteSpace(sottofunzione))
                assocFunc = assocFunc.Where(x => x.XR_HRIS_ABIL_SUBFUNZIONE.COD_SUBFUNZIONE == sottofunzione);


            result.AddRange(assocFunc.ToList().Select(x => new AbilDb()
            {
                Abil = dbAbil,
                Funzione = x.XR_HRIS_ABIL_SUBFUNZIONE.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE,
                Sottofunzione = x.XR_HRIS_ABIL_SUBFUNZIONE.COD_SUBFUNZIONE,
                Profilo = dbAbil.XR_HRIS_ABIL_PROFILO.COD_PROFILO,
                DbSubFunc = x.XR_HRIS_ABIL_SUBFUNZIONE
            }));

            //var assocProfili = profilo.XR_HRIS_ABIL_PROFILO_ASSOCIAZIONI.Where(x => x.IND_ATTIVO && x.ID_PROFILO_SUB != null && x.XR_HRIS_ABIL_PROFILO.IND_ATTIVO);
            var assocProfili = db.XR_HRIS_ABIL_PROFILO_ASSOCIAZIONI
                                    .Include("XR_HRIS_ABIL_PROFILO1")
                                    .Where(x => x.ID_PROFILO == profilo.ID_PROFILO
                                                && x.ID_PROFILO_SUB != null
                                                && x.XR_HRIS_ABIL_PROFILO.IND_ATTIVO);


            foreach (var assocProfilo in assocProfili)
                result.AddRange(GetAbilSubFuncPerProfilo(db, dbAbil, assocProfilo.XR_HRIS_ABIL_PROFILO1, funzione, sottofunzione));

            return result;
        }

        public static IQueryable<T> SintesiFilter<T>(IQueryable<T> input, string matricola, string provenienza, string funzione, string sottofunzione = "", params LivFilter.LivEnum[] livelli) where T : class, myRai.Data.Interface.ISintesi1
        {
            IQueryable<T> result = input;

            string[] noAbil = new string[] { "ABIL", "NODIP" };

            bool checkDb = false;

            if (!noAbil.Contains(provenienza))
            {
                Expression<Func<T, bool>> expr = null;
                if (IsDbAbil(funzione, out bool isIntegration, out bool isFilterIntegration) && !isIntegration && !isFilterIntegration)
                {
                    checkDb = true;
                }
                else
                {
                    AbilInfo abilInfo = AuthHelper.EnabledInfo(matricola, funzione, sottofunzione);
                    expr = abilInfo.GetSintesiFilter<T>(!isFilterIntegration, livelli);
                }

                if (checkDb || isFilterIntegration)
                {
                    List<AbilDb> abilDb = GetAbilSubFunc(matricola, funzione, sottofunzione, true);
                    bool loadPers = false;
                    List<XR_HRIS_ABIL_MODELLO> persModel = null;
                    if (abilDb != null && abilDb.Any())
                    {
                        foreach (var item in abilDb)
                        {
                            Expression<Func<T, bool>> tmpExpr = null;
                            List<AbilDBFilter> temp = SingleGetAbilFilters(matricola, ref loadPers, ref persModel, item, out bool isModelAbil);
                            string condition = isModelAbil ? item.Abil.COD_CONDITION : "OR";
                            CalcFilterExpr(ref expr, ref tmpExpr, temp, condition);
                        }
                    }
                    else
                    {
                        if (!loadPers)
                        {
                            loadPers = true;
                            persModel = LoadPersModel(matricola);
                        }
                        Expression<Func<T, bool>> tmpExpr = null;
                        List<AbilDBFilter> temp = new List<AbilDBFilter>();
                        temp.AddRange(persModel.Select(x => AbilDBFilter.FromModelPers(x)));
                        CalcFilterExpr(ref expr, ref tmpExpr, temp, "OR");
                    }
                }

                if (expr == null)
                    expr = x => true;

                result = input.Where(expr);
            }

            return result;
        }

        private static void CalcFilterExpr<T>(ref Expression<Func<T, bool>> expr, ref Expression<Func<T, bool>> tmpExpr, List<AbilDBFilter> temp, string condition) where T : class, myRai.Data.Interface.ISintesi1
        {
            foreach (var filter in temp)
            {
                switch (condition)
                {
                    case "OR":
                        tmpExpr = LinqHelper.PutInOrTogether(tmpExpr, filter.GetSintesiFilter<T>());
                        break;
                    default:
                        tmpExpr = LinqHelper.PutInAndTogether(tmpExpr, filter.GetSintesiFilter<T>());
                        break;
                }
            }

            if (tmpExpr != null)
                expr = LinqHelper.PutInAndTogether(expr, tmpExpr);
        }

        public static string GetUfficioAbil(string funzione, string sottofunzione)
        {
            var db = new IncentiviEntities();
            string ufficio = db.XR_HRIS_ABIL_SUBFUNZIONE.Where(x => x.COD_SUBFUNZIONE == sottofunzione && x.XR_HRIS_ABIL_FUNZIONE.COD_FUNZIONE == funzione).Select(x => x.NOT_UFFICIO).FirstOrDefault();
            return ufficio;
        }
    }
    public class NominativoMatricola
    {
        public string Cognome { get; set; }
        public string Nome { get; set; }
        public string Matricola { get; set; }
    }
}
