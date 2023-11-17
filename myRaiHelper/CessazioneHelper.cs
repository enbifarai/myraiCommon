using myRaiData.Incentivi;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Xceed.Words.NET;
using Xceed.Document.NET;
using myRai.DataAccess;
using myRaiCommonTasks;
using System.Text.RegularExpressions;
using System.Globalization;

namespace myRaiHelper
{
    public enum IncStato
    {
        InCarico = 0,

        RichiestaInserita = 9,
        RichiestaAccettata = 10,

        RecessoEffettuato = 1,
        Controllato = 2,
        Conteggio = 3,
        BozzaVerbale = 14,
        Appuntamento = 4,
        VerbaleFirmato = 5,
        VerbaleCaricato = 6,
        Cedolini = 7,
        Conclusa = 8,

        RichiestaRifiutata = 11,
        RichiestaAnnullata = 12,
        RichiestaDecaduta = 13,
        PropostaRifiutata = 14,
        PropostaDecaduta = 15,
        MancatoSeguito = 99,

        FileCambioQual = 881,
        FileMat = 882,
        FileTessContr = 883,

        FileOverride = 884,

        FileAnpal = 885,
        TempFileAnpal = 995,

        FileAccettazione = 886,
        TempFileAccettazione = 996,

        FileProposta = 887,
        TempFileProposta = 997,

        TempFileContabili = 998,
        TempFileEstratti = 999
    }

    public enum CessazioneTipo
    {
        Tutti = 0,
        Incentivazione = 1,
        RisoluzioneConsensuale = 2,
        Pensione = 15
    }

    public enum AbilOper
    {
        Reading = 0,
        Writing = 1
    }

    public class CessazioneControllerScope : SessionScope<CessazioneControllerScope>
    {
        public CessazioneControllerScope()
        {

        }

        public void Clear()
        {
            _readLastUpdate = null;
            _writeLastUpdate = null;
            _sediLastUpdate = null;
        }

        private bool _readApplyFilters = false;
        private List<int> _readEnabledStates = null;
        private DateTime? _readLastUpdate { get; set; }

        private bool _writeApplyFilters = false;
        private List<int> _writeEnabledStates = null;
        private DateTime? _writeLastUpdate { get; set; }

        private bool _sediApplyFilters = false;
        private List<string> _enabledSedi = null;
        private DateTime? _sediLastUpdate { get; set; }

        private bool _noteApplyFilters = false;
        private List<string> _noteEnabledMatr = null;
        private DateTime? _noteLastUpdate { get; set; }

        public void SetNoteAbil(bool applyFilters, List<string> enabledMatr)
        {
            _noteApplyFilters = applyFilters;
            _noteEnabledMatr = null;
            if (enabledMatr != null)
            {
                _noteEnabledMatr = new List<string>();
                _noteEnabledMatr.AddRange(enabledMatr);
            }
            _noteLastUpdate = DateTime.Now;
        }
        public void GetNoteAbil(out bool applyFilters, out List<string> enabledMatr)
        {
            applyFilters = _noteApplyFilters;
            enabledMatr = null;
            if (_noteEnabledMatr != null)
            {
                enabledMatr = new List<string>();
                enabledMatr.AddRange(_noteEnabledMatr);
            }
        }
        public bool NeedNoteRefresh()
        {
            bool needIt = false;
            DateTime current = DateTime.Now;

            if (_noteLastUpdate.HasValue)
            {
                TimeSpan t1 = current.Subtract(_noteLastUpdate.Value);
                double minuti = t1.TotalMinutes;

                if (minuti > 5)
                {
                    // se son passati più di 5 minuti azzera lo scope
                    needIt = true;
                }
            }
            else
            {
                needIt = true;
            }

            return needIt;
        }

        public void SetReadAbil(bool applyFilters, List<int> enabledStates)
        {
            _readApplyFilters = applyFilters;
            _readEnabledStates = null;
            if (enabledStates != null)
            {
                _readEnabledStates = new List<int>();
                _readEnabledStates.AddRange(enabledStates);
            }
            _readLastUpdate = DateTime.Now;
        }
        public void GetReadAbil(out bool applyFilters, out List<int> enabledStates)
        {
            applyFilters = _readApplyFilters;
            enabledStates = null;
            if (_readEnabledStates != null)
            {
                enabledStates = new List<int>();
                enabledStates.AddRange(_readEnabledStates);
            }
        }
        public bool NeedReadRefresh()
        {
            bool needIt = false;
            DateTime current = DateTime.Now;

            if (_readLastUpdate.HasValue)
            {
                TimeSpan t1 = current.Subtract(_readLastUpdate.Value);
                double minuti = t1.TotalMinutes;

                if (minuti > 5)
                {
                    // se son passati più di 5 minuti azzera lo scope
                    needIt = true;
                }
            }
            else
            {
                needIt = true;
            }

            return needIt;
        }

        public void SetWriteAbil(bool applyFilters, List<int> enabledStates)
        {
            _writeApplyFilters = applyFilters;
            _writeEnabledStates = null;
            if (enabledStates != null)
            {
                _writeEnabledStates = new List<int>();
                _writeEnabledStates.AddRange(enabledStates);
            }
            _writeLastUpdate = DateTime.Now;
        }
        public void GetWriteAbil(out bool applyFilters, out List<int> enabledStates)
        {
            applyFilters = _writeApplyFilters;
            enabledStates = null;
            if (_writeEnabledStates != null)
            {
                enabledStates = new List<int>();
                enabledStates.AddRange(_writeEnabledStates);
            }
        }
        public bool NeedWriteRefresh()
        {
            bool needIt = false;
            DateTime current = DateTime.Now;

            if (_writeLastUpdate.HasValue)
            {
                TimeSpan t1 = current.Subtract(_writeLastUpdate.Value);
                double minuti = t1.TotalMinutes;

                if (minuti > 5)
                {
                    // se son passati più di 5 minuti azzera lo scope
                    needIt = true;
                }
            }
            else
            {
                needIt = true;
            }

            return needIt;
        }
    }

    public class IncentivazioneHelper : CessazioneHelper
    {

    }

    public class CessazioneOneri
    {
        public string[] FiltriQualifiche { get; set; }
        public decimal Percentuale { get; set; }
    }

    public class CessazioneBozzaSample
    {
        public string Gruppo { get; set; }
        public string Descrizione { get; set; }
        public string Sede { get; set; }
        public string Template { get; set; }
        public string Qualifica { get; set; }
        public string Sindacato { get; set; }
    }

    public class CessazioneCertServizio
    {
        public XR_INC_DIPENDENTI Dipendente { get; set; }
        public SINTESI1 Sintesi { get; set; }
        public bool Custom { get; set; }
        public string PrimoParagrafo { get; set; }
        public string SecondoParagrafo { get; set; }
        public string TerzoParagrafo { get; set; }
        public string Protocollo { get; set; }
        public DateTime? DataGenerazione { get; set; }
        public bool CertServInGenerazione { get; set; }
        public bool HasPDF { get; set; }
        public DateTime? MailDataInvio { get; set; }
        public string MailUtenteInvio { get; set; }
        public string MailCC { get; set; }
        public string MailCCN { get; set; }
        public string MailTesto { get; set; }
        public DateTime? MailDataModifica { get; set; }
        public string MailUtenteModifica { get; set; }
    }

    public class CessazioneHelper
    {
        public const string INCENTIVI_HRGA_FUNC = "INCENTIVI";
        public const string INCENTIVI_INC_EXTRA = "INC_EXTRA";

        public static bool EnabledToIncentivi(string matricola = null)
        {
            if (String.IsNullOrWhiteSpace(matricola))
                matricola = CommonHelper.GetCurrentUserMatricola();

            bool auth = false;
            auth = EnabledTo(matricola) || AuthHelper.EnabledTo(matricola, INCENTIVI_INC_EXTRA);

            return auth == true;
        }

        private static string GetFunzioneAbil(out bool isOld)
        {
            isOld = false;
            string funzione = HrisHelper.GetParametro<string>(HrisParam.INCAbilFunc);
            if (String.IsNullOrWhiteSpace(funzione))
                funzione = INCENTIVI_HRGA_FUNC;

            isOld = funzione == INCENTIVI_HRGA_FUNC;

            return funzione;
        }
        private static string[] ConvertToHrgaSubFunc(params string[] hrisSubFunc)
        {
            string funzione = GetFunzioneAbil(out bool isOld);
            string[] result = hrisSubFunc;

            if (!isOld)
                result = result.Select(x => "INC" + x).ToArray();

            return result;
        }
        private static List<string> ConvertToHrisSubFunc(List<string> hrgaSubfunc)
        {
            List<string> result = new List<string>();

            if (hrgaSubfunc.Any() && hrgaSubfunc.Any(x => x.StartsWith("INC")))
                result.AddRange(hrgaSubfunc.Where(x => x.StartsWith("INC")).Select(x =>  x.Substring(3)));
            else
                result = hrgaSubfunc;

            return result;
        }

        public static bool EnabledTo(string matricola)
        {
            string funzione = GetFunzioneAbil(out bool isOld);
            bool result = false;

            if (isOld)
                result = AuthHelper.EnabledTo(matricola, funzione);
            else
            {
                var gestFunc = AuthHelper.EnabledSubFunc(matricola, funzione);
                result = gestFunc.Any(x => x.StartsWith("INC"));
            }

            return result;
        }
        public static bool EnabledToSubFunc(string matricola, string subFunc)
        {
            string funzione = GetFunzioneAbil(out bool isOld);
            return AuthHelper.EnabledToSubFunc(matricola, funzione, ConvertToHrgaSubFunc(subFunc)[0]);
        }
        public static List<string> EnabledSubFunc(string matricola)
        {
            string funzione = GetFunzioneAbil(out bool isOld);

            return ConvertToHrisSubFunc(AuthHelper.EnabledSubFunc(matricola, funzione));
        }
        public static bool EnabledToAnySubFunc(string matricola, params string[] sottofunzione)
        {
            string funzione = GetFunzioneAbil(out bool isOld);
            return AuthHelper.EnabledToAnySubFunc(matricola, funzione, ConvertToHrgaSubFunc(sottofunzione));
        }
        public static List<NominativoMatricola> GetAllEnabledAs(string sottofunzione, bool escludiDelegati = false)
        {
            string funzione = GetFunzioneAbil(out bool isOld);
            return AuthHelper.GetAllEnabledAs(funzione, ConvertToHrgaSubFunc(sottofunzione)[0], escludiDelegati);
        }
        public static bool IsRoleAdmin(string matricola)
        {
            return EnabledToSubFunc(matricola, "ADM");
        }

        delegate bool ScopeChecker();
        delegate void GetAbilScope(out bool applyFilters, out List<int> enabledStates);
        delegate void SetAbilScope(bool applyFilters, List<int> enabledStates);

        public static bool GetEnabledNoteChange(string matricola, out List<string> enabledMatr)
        {
            bool applyFilters = false;
            enabledMatr = new List<string>();

            if (!CessazioneControllerScope.Instance.NeedNoteRefresh())
                CessazioneControllerScope.Instance.GetNoteAbil(out applyFilters, out enabledMatr);
            else
            {
                //Nel caso degli incentivi, si è abilitati a una sola sottofunzione
                var elencoSubFunc = EnabledSubFunc(matricola);
                if (elencoSubFunc.Any())
                {
                    string sottoFunz = elencoSubFunc[0];

                    if (sottoFunz == "ADM")
                        applyFilters = false;
                    else
                    {
                        applyFilters = true;
                        var elenco = CessazioneHelper.GetAllEnabledAs(sottoFunz);
                        foreach (var item in elenco)
                            enabledMatr.Add(item.Matricola);
                    }
                }
                else
                    applyFilters = true;

                CessazioneControllerScope.Instance.SetNoteAbil(applyFilters, enabledMatr);
            }

            return applyFilters;
        }

        public static bool GetEnabledStates(IncentiviEntities db, string matricola, AbilOper abilOper, out List<int> enabledState)
        {
            bool applyFilters = false;
            enabledState = new List<int>();

            if (db == null)
                db = new IncentiviEntities();

            ScopeChecker scopeNeedRefresh = null;
            GetAbilScope abilGetter = null;
            SetAbilScope abilSetter = null;

            if (abilOper == AbilOper.Reading)
            {
                scopeNeedRefresh = CessazioneControllerScope.Instance.NeedReadRefresh;
                abilSetter = CessazioneControllerScope.Instance.SetReadAbil;
                abilGetter = CessazioneControllerScope.Instance.GetReadAbil;
            }
            else
            {
                scopeNeedRefresh = CessazioneControllerScope.Instance.NeedWriteRefresh;
                abilSetter = CessazioneControllerScope.Instance.SetWriteAbil;
                abilGetter = CessazioneControllerScope.Instance.GetWriteAbil;
            }

            if (!scopeNeedRefresh())
            {
                abilGetter(out applyFilters, out enabledState);
                return applyFilters;
            }
            else
            {
                //Nel caso degli incentivi, si è abilitati a una sola sottofunzione
                var elencoSubFunc = EnabledSubFunc(matricola);
                if (elencoSubFunc.Any())
                {
                    string sottoFunz = elencoSubFunc[0];
                    ExtractStatesAbil(db, matricola, abilOper, ref enabledState, ref applyFilters, sottoFunz);
                }
                else
                {
                    applyFilters = true;
                }

                abilSetter(applyFilters, enabledState);
            }

            return applyFilters;
        }
        public static bool GetEnabledSedi(string matricola, out List<string> enabledSedi)
        {
            bool applyFilters = false;
            enabledSedi = new List<string>();

            if (!IsRoleAdmin(matricola))
            {
                AbilSedi abilSedi = null;
                var funzione = GetFunzioneAbil(out bool isOld);
                if (isOld)
                    abilSedi = AuthHelper.EnabledSedi(matricola, funzione);
                else
                {
                    var listsubfunc = EnabledSubFunc(matricola);
                    if (listsubfunc != null && listsubfunc.Any())
                    {
                        var subfunc = ConvertToHrgaSubFunc(listsubfunc[0])[0];
                        abilSedi = AuthHelper.EnabledSedi(matricola, funzione, subfunc);
                    }
                    else
                        abilSedi = new AbilSedi() { Enabled = false, HasFilter = true };
                }
                applyFilters = abilSedi.HasFilter;
                enabledSedi = abilSedi.SediIncluse;
            }

            return applyFilters;
        }
        public static bool GetEnabledCategory(IncentiviEntities db, string matricola, out List<string> categoryIncluded, out List<string> categoryExcluded)
        {
            bool applyFilters = false;
            categoryIncluded = new List<string>();
            categoryExcluded = new List<string>();

            if (!IsRoleAdmin(matricola)
            && !EnabledToSubFunc(matricola, "ARCAL"))
            {
                AbilCat abilCatFilter = null;
                var funzione = GetFunzioneAbil(out bool isOld);

                if (isOld)
                    abilCatFilter = AuthHelper.EnabledCategory(matricola, funzione);
                else
                {
                    var listsubfunc = EnabledSubFunc(matricola);
                    if (listsubfunc != null && listsubfunc.Any())
                    {
                        var subfunc = ConvertToHrgaSubFunc(listsubfunc[0])[0];
                        abilCatFilter = AuthHelper.EnabledCategory(matricola, funzione, subfunc);
                    }
                    else
                        abilCatFilter = new AbilCat() { Enabled = false, HasFilter = true };
                }

                applyFilters = abilCatFilter.HasFilter;
                categoryIncluded = abilCatFilter.CategorieIncluse;
                categoryExcluded = abilCatFilter.CategorieEscluse;
            }

            return applyFilters;
        }
        public static void ExtractStatesAbil(IncentiviEntities db, string matricola, AbilOper abilOper, ref List<int> enabledState, ref bool applyFilters, string sottFunz)
        {
            if (db == null)
                db = new IncentiviEntities();

            switch (abilOper)
            {
                case AbilOper.Reading:
                    if (!db.XR_INC_STATI_ABIL.Any(x => x.PROFILO == sottFunz && x.ID_STATO == null && x.IND_LETTURA == "Y"))
                    {
                        applyFilters = true;
                        enabledState = db.XR_INC_STATI_ABIL.Where(x => x.PROFILO == sottFunz && x.ID_STATO.Value > 0 && x.IND_LETTURA == "Y").Select(y => y.ID_STATO.Value).ToList();
                    }
                    break;
                case AbilOper.Writing:
                    if (!db.XR_INC_STATI_ABIL.Any(x => x.PROFILO == sottFunz && x.ID_STATO == null && x.IND_SCRITTURA == "Y"))
                    {
                        applyFilters = true;
                        enabledState = db.XR_INC_STATI_ABIL.Where(x => x.PROFILO == sottFunz && x.ID_STATO.Value > 0 && x.IND_SCRITTURA == "Y").Select(y => y.ID_STATO.Value).ToList();
                    }
                    break;
            }

            var paramVerbali = HrisHelper.GetParametro(HrisParam.IncentiviStatiVerbale);
            if (paramVerbali != null)
            {
                var listStati = paramVerbali.COD_VALUE1.Split(',').Select(x => Convert.ToInt32(x)).ToArray();
                var listAbil = paramVerbali.COD_VALUE2.Split(',');
                if (listAbil.Contains(sottFunz))
                {
                    if (AuthHelper.EnabledToAnySubFunc(matricola, INCENTIVI_INC_EXTRA, "VERBALI", "CONTENZIOSO"))
                        enabledState = enabledState.Where(x => !listStati.Contains(x)).ToList();
                }
            }
        }


        public static Expression<Func<XR_INC_DIPENDENTI, bool>> IsAnyState(params int[] stato)
        {
            return x => x.XR_INC_OPERSTATI.Where(y => y.ID_STATO != 0 && !y.DATA_FINE_VALIDITA.HasValue).Select(w => w.ID_STATO).Any(y => stato.Contains(y));
        }
        public static Expression<Func<XR_INC_DIPENDENTI, bool>> NotIsAnyState(params int[] stato)
        {
            return x => x.XR_INC_OPERSTATI.Where(y => y.ID_STATO != 0 && !y.DATA_FINE_VALIDITA.HasValue).Select(w => w.ID_STATO).Any(y => !stato.Contains(y));
        }
        public static Expression<Func<XR_INC_DIPENDENTI, bool>> IsCurrentState(params int[] stato)
        {
            return x => stato.Any(f => f == x.XR_INC_OPERSTATI.Where(y => y.ID_STATO != 0 && !y.DATA_FINE_VALIDITA.HasValue)
                            .Select(w => w.XR_INC_STATI)
                            .OrderByDescending(z => z.XR_WKF_WORKFLOW.FirstOrDefault(a => a.ID_TIPOLOGIA == x.ID_TIPOLOGIA).ORDINE)
                            .Select(b => b.ID_STATO)
                            .FirstOrDefault());
        }
        public static Expression<Func<XR_INC_DIPENDENTI, bool>> NotIsCurrentState(params int[] stato)
        {
            return x => !stato.Any(f => f == x.XR_INC_OPERSTATI.Where(y => y.ID_STATO != 0 && !y.DATA_FINE_VALIDITA.HasValue)
                            .Select(w => w.XR_INC_STATI)
                            .OrderByDescending(z => z.XR_WKF_WORKFLOW.FirstOrDefault(a => a.ID_TIPOLOGIA == x.ID_TIPOLOGIA).ORDINE)
                            .Select(b => b.ID_STATO)
                            .FirstOrDefault());
        }

        public static int[] InactiveState()
        {
            //return new int[] { (int)IncStato.RichiestaAnnullata, (int)IncStato.RichiestaDecaduta, (int)IncStato.RichiestaRifiutata };
            var db = new IncentiviEntities();
            return db.XR_INC_STATI.Where(x => x.IND_INACTIVE_STATE != null && x.IND_INACTIVE_STATE.Value).Select(x => x.ID_STATO).ToArray();
        }

        public static Expression<Func<XR_INC_DIPENDENTI, bool>> IsActive()
        {
            return NotIsCurrentState(InactiveState());
        }

        public static Expression<Func<XR_INC_DIPENDENTI, bool>> FuncFilterAdditional(IncentiviEntities db, string matricola = "")
        {
            if (String.IsNullOrWhiteSpace(matricola))
                matricola = CommonHelper.GetCurrentUserMatricola();

            if (db == null)
                db = new IncentiviEntities();

            List<string> dirIncluse = new List<string>();
            List<string> dirEscluse = new List<string>();
            List<string> qualIncluse = new List<string>();
            List<string> qualEscluse = new List<string>();

            var filters = db.XR_INC_ADD_FILTER.Where(x => x.LIST_MATRICOLE.Contains(matricola));

            if (filters.Any())
            {
                foreach (var filter in filters)
                {
                    if (!String.IsNullOrWhiteSpace(filter.LIST_CAT_INCL))
                        qualIncluse.AddRange(filter.LIST_CAT_INCL.Split(','));
                    if (!String.IsNullOrWhiteSpace(filter.LIST_CAT_ESCL))
                        qualEscluse.AddRange(filter.LIST_CAT_ESCL.Split(',').Where(x => !qualIncluse.Contains(x)));

                    if (!String.IsNullOrWhiteSpace(filter.LIST_DIR_INCL))
                        dirIncluse.AddRange(filter.LIST_DIR_INCL.Split(','));
                    if (!String.IsNullOrWhiteSpace(filter.LIST_DIR_ESCL))
                        dirEscluse.AddRange(filter.LIST_DIR_ESCL.Split(',').Where(x => !dirIncluse.Contains(x)));
                }

                return x => (!qualIncluse.Any() || qualIncluse.Any(y => x.SINTESI1.COD_QUALIFICA.StartsWith(y)))
                            && (!qualEscluse.Any() || !qualEscluse.Any(y => x.SINTESI1.COD_QUALIFICA.StartsWith(y)))
                            && (!dirIncluse.Any() || dirIncluse.Contains(x.SINTESI1.COD_SERVIZIO))
                            && (!dirEscluse.Any() || !dirEscluse.Contains(x.SINTESI1.COD_SERVIZIO));
            }
            else
                return x => true;
        }


        public static Expression<Func<XR_INC_DIPENDENTI, bool>> FuncFilterVerbali(IncentiviEntities db, string matricola = "", bool soloScrittura = true)
        {
            Expression<Func<XR_INC_DIPENDENTI, bool>> result = null;

            if (String.IsNullOrWhiteSpace(matricola))
                matricola = CommonHelper.GetCurrentUserMatricola();

            if (db == null)
                db = new IncentiviEntities();

            var paramVerbali = HrisHelper.GetParametro(HrisParam.IncentiviStatiVerbale);
            if (paramVerbali != null)
            {
                var listStati = paramVerbali.COD_VALUE1.Split(',').Select(x => Convert.ToInt32(x)).ToArray();
                var listAbil = paramVerbali.COD_VALUE2.Split(',');

                if (EnabledToAnySubFunc(matricola, listAbil))
                {
                    AbilSubFunc abilSubFunc = null;
                    var abilVerb = AuthHelper.EnabledToSubFunc(matricola, INCENTIVI_INC_EXTRA, "VERBALI", out abilSubFunc)
                                    || AuthHelper.EnabledToSubFunc(matricola, INCENTIVI_INC_EXTRA, "CONTENZIOSO", out abilSubFunc);

                    if (abilVerb)
                    {
                        result = GetAbilIncDipendentiFilter(matricola, INCENTIVI_INC_EXTRA, abilSubFunc.Nome);

                        if (abilSubFunc.Nome == "CONTENZIOSO")
                        {
                            var idPersona = CommonHelper.GetCurrentIdPersona();

                            var isContenziosoFilter = IsContenziosoFilter();

                            if (soloScrittura)
                                result = LinqHelper.PutInOrTogether(result,
                                                                        //x => (x.CAUSE_VERTENZE != null && x.CAUSE_VERTENZE != "") || (x.IND_TIPO_VERTENZE != null && x.IND_TIPO_VERTENZE!="VERT"),
                                                                        isContenziosoFilter,
                                                                        x => x.XR_INC_OPERSTATI.Any(y => y.ID_STATO == (int)IncStato.InCarico && y.ID_PERSONA == idPersona));//Se dalla visualizzazione prendono in carico delle pratiche, queste poi devono essere visibili
                            else
                                //Nel caso del contenzioso in lettura deve vedere tutto
                                result = x => true;

                            if (soloScrittura)
                                result = LinqHelper.PutInAndTogether(IsActive(), NotIsCurrentState((int)IncStato.Conclusa), result);
                            else
                                result = LinqHelper.PutInAndTogether(IsCurrentState(listStati), result);
                        }
                        else if (EnabledToSubFunc(matricola, "GEST"))
                        {
                            //result = LinqHelper.PutInAndTogether(result, x => (x.CAUSE_VERTENZE == null || x.CAUSE_VERTENZE == "") && (x.IND_TIPO_VERTENZE == null || x.IND_TIPO_VERTENZE == "" || x.IND_TIPO_VERTENZE=="VERT"));
                            if (soloScrittura)
                                result = LinqHelper.PutInAndTogether(result, x => (x.IND_TIPO_VERTENZE == null || x.IND_TIPO_VERTENZE == "" || x.IND_TIPO_VERTENZE == "VERT"));

                            result = LinqHelper.PutInAndTogether(IsCurrentState(listStati), result);
                        }
                        else
                            result = LinqHelper.PutInAndTogether(IsCurrentState(listStati), result);
                    }
                }
            }

            return result;
        }

        public static Expression<Func<XR_INC_DIPENDENTI, bool>> IsContenziosoFilter()
        {
            return x => (x.IND_TIPO_VERTENZE != null && x.IND_TIPO_VERTENZE != "" && x.IND_TIPO_VERTENZE != "VERT");
        }
        public static bool IsConteziosoDip(XR_INC_DIPENDENTI dip)
        {
            return dip.IND_TIPO_VERTENZE != null && dip.IND_TIPO_VERTENZE != "" && dip.IND_TIPO_VERTENZE != "VERT";
        }

        public static Expression<Func<XR_INC_DIPENDENTI, bool>> GetAbilIncDipendentiFilter(string matricola, string funzione, string sottofunzione)
        {
            Expression<Func<XR_INC_DIPENDENTI, bool>> result = null;

            if (String.IsNullOrWhiteSpace(matricola))
                matricola = CommonHelper.GetCurrentUserMatricola();

            var abilInfo = AuthHelper.EnabledInfo(matricola, funzione, sottofunzione);
            AbilCat enabledCat = abilInfo.AbilCat;
            AbilDir enabledSer = abilInfo.AbilDir;
            AbilSedi enabledSedi = abilInfo.AbilSedi;
            AbilSocieta enabledSocieta = abilInfo.AbilSocieta;
            AbilMatr enabledAbilMatr = abilInfo.AbilMatr;
            AbilTip enabledTip = abilInfo.AbilTip;

            if (enabledCat.HasFilter)
            {
                var incluse = enabledCat.Incluse();
                var escluse = enabledCat.Escluse();

                if (incluse.Any() || !escluse.Any())
                    result = LinqHelper.PutInAndTogether(result, x => incluse.Any(y => x.SINTESI1.COD_QUALIFICA.StartsWith(y)));

                if (escluse.Any())
                    result = LinqHelper.PutInAndTogether(result, x => !escluse.Any(y => x.SINTESI1.COD_QUALIFICA.StartsWith(y)));
            }

            if (enabledSer.HasFilter)
            {
                var incluse = enabledSer.Incluse();
                var escluse = enabledSer.Escluse();

                if (incluse.Any() || !escluse.Any())
                    result = LinqHelper.PutInAndTogether(result, x => incluse.Contains(x.SINTESI1.COD_SERVIZIO));

                if (escluse.Any())
                    result = LinqHelper.PutInAndTogether(result, x => !escluse.Contains(x.SINTESI1.COD_SERVIZIO));
            }

            if (enabledSedi.HasFilter)
            {
                var incluse = enabledSedi.Incluse();
                var escluse = enabledSedi.Escluse();

                if (incluse.Any() || !escluse.Any())
                    result = LinqHelper.PutInAndTogether(result, x => incluse.Any(y => y == x.SINTESI1.COD_SEDE || y == x.SINTESI1.COD_SEDE.Substring(0, 2) + "*" || y == x.SINTESI1.COD_SEDE.Substring(0, 2) + x.SINTESI1.COD_SERVIZIO));

                if (escluse.Any())
                    result = LinqHelper.PutInAndTogether(result, x => !escluse.Any(y => y == x.SINTESI1.COD_SEDE || y == x.SINTESI1.COD_SEDE.Substring(0, 2) + "*" || y == x.SINTESI1.COD_SEDE.Substring(0, 2) + x.SINTESI1.COD_SERVIZIO));
            }

            if (enabledSocieta.HasFilter)
            {
                var incluse = enabledSocieta.Incluse();
                var escluse = enabledSocieta.Escluse();

                if (incluse.Any() || !escluse.Any())
                    result = LinqHelper.PutInAndTogether(result, x => incluse.Contains(x.SINTESI1.COD_IMPRESACR));

                if (escluse.Any())
                    result = LinqHelper.PutInAndTogether(result, x => !escluse.Contains(x.SINTESI1.COD_IMPRESACR));
            }

            if (enabledAbilMatr.HasFilter)
            {
                var incluse = enabledAbilMatr.Incluse();
                var escluse = enabledAbilMatr.Escluse();

                if (incluse.Any() || !escluse.Any())
                    result = LinqHelper.PutInAndTogether(result, x => incluse.Contains(x.SINTESI1.COD_MATLIBROMAT));

                if (escluse.Any())
                    result = LinqHelper.PutInAndTogether(result, x => !escluse.Contains(x.SINTESI1.COD_MATLIBROMAT));
            }

            if (enabledTip.HasFilter)
            {
                var incluse = enabledTip.Incluse().Select(x => Convert.ToInt32(x));
                var escluse = enabledTip.Escluse().Select(x => Convert.ToInt32(x));

                if (incluse.Any() || !escluse.Any())
                    result = LinqHelper.PutInAndTogether(result, x => incluse.Contains(x.ID_TIPOLOGIA));

                if (escluse.Any())
                    result = LinqHelper.PutInAndTogether(result, x => !escluse.Contains(x.ID_TIPOLOGIA));
            }

            return result;
        }

        public static Func<XR_INC_DIPENDENTI, int> GetCurrentState()
        {
            return x => x.XR_INC_OPERSTATI.Where(y => y.ID_STATO != 0 && !y.DATA_FINE_VALIDITA.HasValue)
                            .Select(w => w.XR_INC_STATI)
                            .OrderByDescending(z => z.XR_WKF_WORKFLOW.FirstOrDefault(a => a.ID_TIPOLOGIA == x.ID_TIPOLOGIA).ORDINE)
                            .Select(b => b.ID_STATO)
                            .FirstOrDefault();
        }
        public static Func<XR_INC_DIPENDENTI, XR_INC_STATI> GetCurrentStateRec()
        {
            return x => x.XR_INC_OPERSTATI.Where(y => y.ID_STATO != 0 && !y.DATA_FINE_VALIDITA.HasValue)
                            .Select(w => w.XR_INC_STATI)
                            .OrderByDescending(z => z.XR_WKF_WORKFLOW.FirstOrDefault(a => a.ID_TIPOLOGIA == x.ID_TIPOLOGIA).ORDINE)
                            .FirstOrDefault();
        }

        public static Expression<Func<XR_INC_DIPENDENTI, bool>> FuncFilterAbil(bool soloScrittura, IncentiviEntities db, bool getExtra = false, string extraFunc = "")
        {
            Expression<Func<XR_INC_DIPENDENTI, bool>> funcFilterAbil = null;
            bool applyFilterStates = false;
            List<int> enabledStates = null;

            if (!getExtra)
            {
                applyFilterStates = GetEnabledStates(db, CommonHelper.GetCurrentUserMatricola(), soloScrittura ? AbilOper.Writing : AbilOper.Reading, out enabledStates);

                //if (soloScrittura && applyFilterStates && enabledStates.Count == 0)
                //    applyFilterStates = GetEnabledStates(db, CommonHelper.GetCurrentUserMatricola(), AbilOper.Reading, out enabledStates);
            }
            else
            {
                ExtractStatesAbil(db, CommonHelper.GetCurrentUserMatricola(), soloScrittura ? AbilOper.Writing : AbilOper.Reading, ref enabledStates, ref applyFilterStates, extraFunc == "ANPAL" ? "INC_ANPAL" : extraFunc);
            }

            if (!applyFilterStates)
                funcFilterAbil = x => true;
            else
                funcFilterAbil = IsCurrentState(enabledStates.ToArray());

            return funcFilterAbil;
        }
        public static Expression<Func<XR_INC_DIPENDENTI, bool>> FuncFilterMatr(IncentiviEntities db)
        {
            List<string> categoryIncluded = null;
            List<string> categoryExcluded = null;
            bool applyFilter = GetEnabledCategory(db, CommonHelper.GetCurrentUserMatricola(), out categoryIncluded, out categoryExcluded);

            Expression<Func<XR_INC_DIPENDENTI, bool>> funcFilterFigPro;

            if (!applyFilter)
                funcFilterFigPro = x => true;
            else
                funcFilterFigPro = x => (categoryIncluded.Any(a => x.SINTESI1.COD_QUALIFICA.StartsWith(a)) && !categoryExcluded.Any(b => x.SINTESI1.COD_QUALIFICA.StartsWith(b)));

            return funcFilterFigPro;
        }
        public static Expression<Func<XR_INC_DIPENDENTI, bool>> FuncFilterAbilSede()
        {
            List<string> enabledSedi = null;
            bool applyFilterSedi = GetEnabledSedi(CommonHelper.GetCurrentUserMatricola(), out enabledSedi);

            Expression<Func<XR_INC_DIPENDENTI, bool>> funcFilterAbilSede;

            if (!applyFilterSedi)
                funcFilterAbilSede = x => true;
            else
                funcFilterAbilSede = x => enabledSedi.Contains(x.SINTESI1.COD_SEDE.Substring(0, 2) + "*") || enabledSedi.Contains(x.SINTESI1.COD_SEDE.Substring(0, 2) + x.SINTESI1.COD_SERVIZIO);

            return funcFilterAbilSede;
        }

        public static XR_INC_STATI GetPraticaStato(XR_INC_DIPENDENTI pratica)
        {
            return pratica.XR_INC_OPERSTATI.Where(y => !y.XR_INC_STATI.IND_SYSTEM.Value && !y.DATA_FINE_VALIDITA.HasValue)
                    .Select(w => w.XR_INC_STATI).OrderByDescending(z => z.XR_WKF_WORKFLOW.FirstOrDefault(a => a.ID_TIPOLOGIA == pratica.ID_TIPOLOGIA).ORDINE)
                    .FirstOrDefault();
        }
        public static XR_INC_OPERSTATI GetPraticaOperStato(XR_INC_DIPENDENTI pratica)
        {
            return pratica.XR_INC_OPERSTATI.Where(y => !y.XR_INC_STATI.IND_SYSTEM.Value && !y.DATA_FINE_VALIDITA.HasValue)
                    .OrderByDescending(z => z.XR_INC_STATI.XR_WKF_WORKFLOW.FirstOrDefault(a => a.ID_TIPOLOGIA == pratica.ID_TIPOLOGIA).ORDINE)
                    .FirstOrDefault();
        }

        public static bool GetHomeWidgetPratiche(CessazioneTipo tipo, out int totPratiche, out int totInCarico)
        {
            bool auth = false;
            totPratiche = 0;
            totInCarico = 0;

            string matricola = CommonHelper.GetCurrentUserMatricola();
            int idPers = CommonHelper.GetCurrentIdPersona();
            //var list = EnabledSubFunc(matricola);
            //if (list.Any())
            //{
            //    if (list.Count() == 1 && list[0] == "GEST")
            //    {
            //        //nel caso della gestione non ha senso fa vedere il widget
            //        //Questa sottofunzione non da diritto alla modifica delle pratiche
            //    }
            //    else
            //    {
            //        auth = true;
            //        using (IncentiviEntities db = new IncentiviEntities())
            //        {
            //            var funcFilterAbil = FuncFilterAbil(true, db);
            //            var funcFilterMatr = FuncFilterMatr(db);
            //            var funcFilterAbilSede = FuncFilterAbilSede();
            //            var funcNotConcluse = NotIsCurrentState((int)IncStato.Conclusa);

            //            var tmp = db.XR_INC_DIPENDENTI.Where(funcFilterAbil)
            //                        .Where(funcFilterAbilSede)
            //                        .Where(funcFilterMatr)
            //                        .Where(funcNotConcluse);

            //            if (tipo != CessazioneTipo.Tutti)
            //                tmp = tmp.Where(x => x.ID_TIPOLOGIA == (int)tipo);

            //            totPratiche = tmp.Count();
            //            totInCarico = tmp.Where(x => x.XR_INC_OPERSTATI.Any(y => y.ID_STATO == 0 && y.ID_PERSONA == idPers)).Count();
            //        }
            //    }
            //}

            if (EnabledToIncentivi(matricola))
            {
                auth = true;
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    var funcFilterAbil = FuncFilterAbil(true, db);
                    var funcFilterMatr = FuncFilterMatr(db);
                    var funcFilterAbilSede = FuncFilterAbilSede();
                    var funcNotConcluse = NotIsCurrentState((int)IncStato.Conclusa);

                    var tmp = db.XR_INC_DIPENDENTI.Where(funcFilterAbil)
                                .Where(funcFilterAbilSede)
                                .Where(funcFilterMatr)
                                .Where(funcNotConcluse);

                    if (tipo != CessazioneTipo.Tutti)
                        tmp = tmp.Where(x => x.ID_TIPOLOGIA == (int)tipo);

                    totPratiche = tmp.Count();
                    totInCarico = tmp.Where(x => x.XR_INC_OPERSTATI.Any(y => y.ID_STATO == 0 && y.ID_PERSONA == idPers)).Count();
                }
            }

            return auth;
        }

        public static bool GetHomeWidgetAppuntamenti(out int totAppuntamenti, out int totNelMese)
        {
            bool auth = false;
            totAppuntamenti = 0;
            totNelMese = 0;

            string matricola = CommonHelper.GetCurrentUserMatricola();
            if (EnabledToAnySubFunc(matricola, "ADM", "RELIND"))
            {
                auth = true;
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    var funcFilterAbil = FuncFilterAbil(false, db);
                    var funcFilterMatr = FuncFilterMatr(db);
                    var funcFilterAbilSede = FuncFilterAbilSede();

                    Expression<Func<XR_INC_DIPENDENTI, bool>> funcCurrentState = IsCurrentState((int)IncStato.Appuntamento);

                    var tmp2 = db.XR_INC_DIPENDENTI
                                   .Where(x => x.ID_TIPOLOGIA == (int)CessazioneTipo.Incentivazione)
                                   .Where(funcFilterAbil)
                                   .Where(funcFilterAbilSede)
                                   .Where(funcFilterMatr)
                                   .Where(funcCurrentState)
                                   .Select(x => new { x.DATA_APPUNTAMENTO });

                    var tmp = tmp2.ToList();

                    totAppuntamenti = tmp.Count(x => !x.DATA_APPUNTAMENTO.HasValue || x.DATA_APPUNTAMENTO.Value >= DateTime.Today);
                    totNelMese = tmp.Count(x => x.DATA_APPUNTAMENTO.HasValue && x.DATA_APPUNTAMENTO.Value >= DateTime.Today && x.DATA_APPUNTAMENTO.Value.Year == DateTime.Today.Year && x.DATA_APPUNTAMENTO.Value.Month == DateTime.Today.Month);
                }
            }

            return auth;
        }

        public static bool GetHomeWidgetPagamenti(out int totMese)
        {
            bool auth = false;
            totMese = 0;

            string matricola = CommonHelper.GetCurrentUserMatricola();
            if (EnabledToAnySubFunc(matricola, "ADM", "AMM"))
            {
                auth = true;
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    var funcFilterAbil = FuncFilterAbil(false, db);
                    var funcFilterMatr = FuncFilterMatr(db);
                    var funcFilterAbilSede = FuncFilterAbilSede();

                    var tmp = db.XR_INC_DIPENDENTI
                                   .Where(x => x.ID_TIPOLOGIA == (int)CessazioneTipo.Incentivazione)
                                   .Where(funcFilterAbil)
                                   .Where(funcFilterAbilSede)
                                   .Where(funcFilterMatr)
                                   .ToList();
                    totMese = tmp.Count(x => !x.DATA_PAGAMENTO.HasValue || (x.DATA_PAGAMENTO.Value.Year == DateTime.Today.Year && x.DATA_PAGAMENTO.Value.Month >= DateTime.Today.Month));
                }
            }

            return auth;
        }

        public static int SalvaStato(int idDip, int stato, int idPersona = 0)
        {
            return SalvaStato(null, idDip, stato, idPersona, true);
        }

        public static int SalvaStato(IncentiviEntities db, int idDip, int stato, int idPersona = 0, bool saveState = true)
        {
            int result = 0;
            if (idPersona == 0)
                idPersona = CommonHelper.GetCurrentIdPersona();

            bool _internalSaveState = saveState;

            if (db == null)
            {
                _internalSaveState = true;
                db = new IncentiviEntities();
            }

            XR_INC_OPERSTATI operStato = db.XR_INC_OPERSTATI.Where(y => !y.DATA_FINE_VALIDITA.HasValue).FirstOrDefault(x => x.ID_DIPENDENTE == idDip && x.ID_STATO == stato);

            bool isNew = false;
            if (operStato == null)
            {
                isNew = true;
                operStato = new XR_INC_OPERSTATI();
                operStato.ID_OPER = db.XR_INC_OPERSTATI.GeneraPrimaryKey();
            }
            operStato.ID_DIPENDENTE = idDip;
            operStato.ID_PERSONA = idPersona;
            operStato.ID_STATO = stato;
            operStato.DATA = DateTime.Now;
            CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime tmsTimeStamp);
            operStato.COD_USER = codUser;
            operStato.COD_TERMID = codTermid;
            operStato.TMS_TIMESTAMP = tmsTimeStamp;
            if (isNew)
                db.XR_INC_OPERSTATI.Add(operStato);

            if (_internalSaveState)
                db.SaveChanges();

            result = operStato.ID_OPER;

            return result;
        }

        public static bool AggiungiNotaPratica(IncentiviEntities db, int idDipendente, string notaPratica, int idPersona = 0, bool salvaDB = true, string matricola = "", string tag = "")
        {
            bool result;

            if (idPersona == 0)
                idPersona = CommonHelper.GetCurrentIdPersona();

            if (String.IsNullOrWhiteSpace(matricola))
                matricola = CommonHelper.GetCurrentUserMatricola();

            if (db == null)
            {
                db = new IncentiviEntities();
                salvaDB = true;
            }

            XR_INC_DIPENDENTI_NOTE nota = new XR_INC_DIPENDENTI_NOTE();
            nota.ID_NOTA = db.XR_INC_DIPENDENTI_NOTE.GeneraPrimaryKey();
            nota.ID_DIPENDENTE = idDipendente;
            nota.ID_PERSONA = idPersona;
            nota.NOTA = notaPratica;
            CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime tms);
            nota.COD_USER = codUser;
            nota.COD_TERMID = codTermid;
            nota.TMS_TIMESTAMP = tms;
            nota.NOT_TAG = tag;
            db.XR_INC_DIPENDENTI_NOTE.Add(nota);

            //if (notificaDip)
            //{
            //    string[] rifGruppo = HrisHelper.GetParametri<string>(HrisParam.IncentiviRifGruppo);
            //    var codGruppo = rifGruppo[0];
            //    var annoRif = rifGruppo[1];

            //    var dip = db.XR_INC_DIPENDENTI.Find(idDipendente);
            //    GestoreMail mail = new GestoreMail();
            //    string mailDest = CommonHelper.GetEmailPerMatricola(dip.MATRICOLA);
            //    string mailOggetto = "Esodi "+annoRif+" - Inserimento nota";
            //    string mailCorpo = notaPratica;
            //    mail.InvioMail(mailCorpo, mailOggetto, mailDest, null, "[CG] RaiPlace - Self Service <raiplace.selfservice@rai.it>", null, null);
            //}

            result = !salvaDB || DBHelper.Save(db, matricola);
            return result;
        }

        public static XR_INC_TEMPLATE GetTemplate(IncentiviEntities db, string codTipo, int idDip, string filtroTemplate, bool getPersonalTemplate, string qualifica = null)
        {
            XR_INC_TEMPLATE template = null;
            if (getPersonalTemplate)
                template = db.XR_INC_TEMPLATE.Where(x => x.VALID_DTA_INI < DateTime.Now && (x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now))
                                                .Where(x => x.COD_TIPO.ToUpper() == codTipo.ToUpper() && x.ID_DIPENDENTE == idDip).FirstOrDefault();

            if (template == null)
            {
                if (!String.IsNullOrWhiteSpace(qualifica))
                    template = db.XR_INC_TEMPLATE.Where(x => x.VALID_DTA_INI < DateTime.Now && (x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now))
                                                    .Where(x => x.COD_TIPO.ToUpper() == codTipo.ToUpper() && x.NME_TEMPLATE == filtroTemplate && x.ID_DIPENDENTE == null).AsEnumerable()
                                                    .Where(x => (x.CAT_INCLUSE == null || x.CAT_INCLUSE.Split(',').Any(y => qualifica.StartsWith(y)))
                                                                    && (x.CAT_ESCLUSE == null || !x.CAT_ESCLUSE.Split(',').Any(y => qualifica.StartsWith(y)))).FirstOrDefault();
                else
                    template = db.XR_INC_TEMPLATE.Where(x => x.VALID_DTA_INI < DateTime.Now && (x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now))
                                                    .Where(x => x.COD_TIPO.ToUpper() == codTipo.ToUpper() && x.NME_TEMPLATE == filtroTemplate && x.ID_DIPENDENTE == null).FirstOrDefault();
            }

            return template;
        }
        public static bool GetPropostaDocMail(IncentiviEntities db, XR_INC_DIPENDENTI dip, out MemoryStream st, out string nomeFile)
        {
            bool found = false;
            st = null;
            nomeFile = null;

            //string filtro = "AllegatoMail";
            //if (dip.SINTESI1.COD_QUALIFICA.StartsWith("M") || dip.SINTESI1.COD_QUALIFICA.StartsWith("A7"))
            //    filtro = "AllegatoMail G";
            string filtro = "AllegatoProposta";

            var templAll = GetTemplate(db, "Proposta", dip.ID_DIPENDENTE, filtro, true, dip.SINTESI1.COD_QUALIFICA);

            if (templAll != null)
            {
                MemoryStream ms = new MemoryStream(templAll.TEMPLATE);
                DocX doc = DocX.Load(ms);
                doc.ReplaceText("__DENOMINAZIONE__", dip.SINTESI1.COD_SESSO == "M" ? "Sig." : "Sig.ra");
                doc.ReplaceText("__NOMINATIVO__", dip.SINTESI1.DES_NOMEPERS.TitleCase() + " " + dip.SINTESI1.DES_COGNOMEPERS.TitleCase());
                doc.ReplaceText("__DATA_CESSAZIONE__", dip.DATA_CESSAZIONE.Value.ToString("dd/MM/yyyy"));
                doc.ReplaceText("__INCENTIVO_NUM__", dip.INCENTIVO_LORDO_IP.GetValueOrDefault().ToString("N2"));
                doc.ReplaceText("__INCENTIVO_LETT__", dip.INCENTIVO_LORDO_IP.GetValueOrDefault().AmountToReadableString());
                doc.ReplaceText("__UNA_TANTUM_NUM__", dip.UNA_TANTUM_LORDA_IP.GetValueOrDefault().ToString("N2"));
                doc.ReplaceText("__UNA_TANTUM_LETT__", dip.UNA_TANTUM_LORDA_IP.GetValueOrDefault().AmountToReadableString());
                doc.ReplaceText("__TFR_LORDO_NUM__", dip.TFR_LORDO_AZ_IP.GetValueOrDefault().ToString());
                doc.ReplaceText("__TFR_LORDO_LETT__", dip.TFR_LORDO_AZ_IP.GetValueOrDefault().AmountToReadableString());
                doc.ReplaceText("__ALIQ_TFR__", dip.ALIQ_TFR.GetValueOrDefault().ToString("N2"));
                doc.ReplaceText("__MATRICOLA__", dip.MATRICOLA);

                st = new MemoryStream();
                doc.SaveAs(st);
                st.Position = 0;
                nomeFile = "Modulo " + dip.SINTESI1.Nominativo().TitleCase() + ".docx";

                found = true;
            }

            return found;
        }

        public static List<XR_INC_TEMPLATE> GetListaAllegati(IncentiviEntities db, string codTipo, int idDipendente, string sede, string qualifica)
        {
            List<XR_INC_TEMPLATE> allegati = null;
            List<XR_INC_TEMPLATE> temporary = null;

            var tmp = db.XR_INC_TEMPLATE
                            .Where(x => x.VALID_DTA_INI < DateTime.Now && (x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now))
                            .Where(x => x.COD_TIPO == codTipo)
                            .Where(x => x.SEDE == null || x.SEDE.ToUpper() == sede.ToUpper())
                            .Where(x => x.ID_DIPENDENTE == null || x.ID_DIPENDENTE == idDipendente);

            //Cerca nome template generico(*)/sede specifica
            temporary = tmp.ToList();
            if (temporary != null && temporary.Any())
                allegati = temporary.Where(x => (x.CAT_INCLUSE == null || x.CAT_INCLUSE.Split(',').Any(y => qualifica.StartsWith(y)))
                                                            && (x.CAT_ESCLUSE == null || !x.CAT_ESCLUSE.Split(',').Any(y => qualifica.StartsWith(y)))).ToList();

            return allegati;
        }

        public static string GetIndirizzoMail(string codParam)
        {
            string result = null;
            using (var db = new IncentiviEntities())
            {
                var paramMail = db.XR_INC_PARAM_MAIL.FirstOrDefault(x => x.COD_CAMPO == codParam);
                if (paramMail != null && !String.IsNullOrWhiteSpace(paramMail.VALORE))
                {
                    if (paramMail.VALORE.Contains("#"))
                        result = GetIndirizzoMail(paramMail.VALORE.Replace("#", ""));
                    else
                        result = paramMail.VALORE;
                }
            }
            return result;
        }
        public static string ReplaceToken(XR_INC_DIPENDENTI dip, string input, string tokenDelimiter = "__")
        {
            string output = input;

            string[] rifGruppo = HrisHelper.GetParametri<string>(HrisParam.IncentiviRifGruppo);
            var codGruppo = rifGruppo[0];
            var annoRif = rifGruppo[1];

            Dictionary<string, object> additionalToken = new Dictionary<string, object>();
            additionalToken.Add("ANNO_RIF", annoRif);
            additionalToken.Add("GRUPPO_RIF", codGruppo);
            foreach (var item in dip.XR_INC_DIPENDENTI_FIELD)
            {
                object obj = null;
                if (item.DATE_VALUE.HasValue)
                    obj = item.DATE_VALUE;
                else if (item.BOOL_VALUE.HasValue)
                    obj = item.BOOL_VALUE;
                else if (item.DEC_VALUE.HasValue)
                    obj = item.DEC_VALUE;
                else if (item.INT_VALUE.HasValue)
                    obj = item.INT_VALUE;
                else if (!String.IsNullOrWhiteSpace(item.STR_VALUE))
                    obj = item.STR_VALUE;

                additionalToken.Add(item.COD_FIELD, obj);
            }
            return HrisHelper.ReplaceToken(dip.SINTESI1, dip, input, tokenDelimiter, additionalToken);
            //DateTime now = DateTime.Now;

            //MatchCollection matches = Regex.Matches(input, $@"(?'token'{tokenDelimiter}(?'key'[A-Z0-9_]+)(\$(?'format'[^\$]+)\$)?{tokenDelimiter})");
            //foreach (Match match in matches)
            //{
            //    if (match.Success)
            //    {
            //        string key = match.Groups["key"].Value;
            //        string token = match.Groups["token"].Value;
            //        string format = match.Groups["format"].Success? match.Groups["format"].Value:null;
            //        switch (key)
            //        {
            //            case "ANNO_RIF":
            //                output = output.Replace(token, annoRif);
            //                break;
            //            case "GRUPPO_RIF":
            //                output = output.Replace(token, codGruppo);
            //                break;
            //            case "NOMINATIVO":
            //                output = output.Replace(token, dip.SINTESI1.Nominativo().TitleCase());
            //                break;
            //            case "COGNOME":
            //                output = output.Replace(token, dip.SINTESI1.DES_COGNOMEPERS.TitleCase());
            //                break;
            //            case "NOME":
            //                output = output.Replace(token, dip.SINTESI1.DES_NOMEPERS.TitleCase());
            //                break;
            //            case "TODAY":
            //                output = output.Replace(token, now.ToString(format??"dd/MM/yyyy"));
            //                break;
            //            case "TIME":
            //                output = output.Replace(token, now.ToString(format??"HH:mm"));
            //                break;
            //            case "NOW":
            //                output = output.Replace(token, now.ToString(format??"dd/MM/yyyy HH:mm"));
            //                break;
            //            default:
            //                var prop = dip.GetType().GetProperty(key);
            //                if (prop != null)
            //                {
            //                    string tmpOutput = "";
            //                    object val = prop.GetValue(dip, null);
            //                    if (val != null)
            //                    {
            //                        Type outputType = prop.PropertyType;
            //                        Type baseType = outputType.IsGenericType && outputType.GetGenericArguments() != null && outputType.GetGenericArguments().Any() ? outputType.GetGenericArguments()[0] : outputType;

            //                        if (baseType == typeof(DateTime))
            //                            format = format ?? "dd/MM/yyyy";
            //                        else if (baseType == typeof(decimal))
            //                            format = format ?? "N2";
            //                        else
            //                            tmpOutput = String.Format("{0}", val);


            //                        string outformat = String.IsNullOrWhiteSpace(format) ? "{0}" : "{0:" + format + "}";
            //                        tmpOutput = String.Format(outformat, val);
            //                    }

            //                    output = output.Replace(token, tmpOutput);
            //                }
            //                break;
            //        }
            //    }
            //}

            //return output;
        }

        public static XR_INC_TEMPLATE InternalGetBozzaVerbale(IncentiviEntities db, string nomeTemplate, string sede, string qualifica, bool isHeader, bool isBody, bool isFooter)
        {
            XR_INC_TEMPLATE template = null;
            List<XR_INC_TEMPLATE> temporary = null;

            var tmp = db.XR_INC_TEMPLATE
                            .Where(x => x.VALID_DTA_INI < DateTime.Now && (x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now))
                            .Where(x => x.IND_HEADER == isHeader && x.IND_BODY == isBody && x.IND_FOOTER == isFooter);



            //Cerca nome template specifico/sede specifica
            if (!String.IsNullOrWhiteSpace(nomeTemplate))
            {
                temporary = tmp.Where(x => x.COD_TIPO == "VerbaleDOC" && x.NME_TEMPLATE.ToUpper().Contains(nomeTemplate.ToUpper()) && x.SEDE != null && x.SEDE.ToUpper() == sede.ToUpper()).ToList();
                if (temporary != null && temporary.Any())
                    template = temporary.FirstOrDefault(x => (x.CAT_INCLUSE == null || x.CAT_INCLUSE.Split(',').Any(y => qualifica.StartsWith(y)))
                                                                && (x.CAT_ESCLUSE == null || !x.CAT_ESCLUSE.Split(',').Any(y => qualifica.StartsWith(y))));

                if (template == null)
                {
                    temporary = tmp.Where(x => x.COD_TIPO == "VerbaleDOC" && x.NME_TEMPLATE.ToUpper().Contains(nomeTemplate.ToUpper()) && x.SEDE == null).ToList();
                    if (temporary != null && temporary.Any())
                        template = temporary.FirstOrDefault(x => (x.CAT_INCLUSE == null || x.CAT_INCLUSE.Split(',').Any(y => qualifica.StartsWith(y)))
                                                                    && (x.CAT_ESCLUSE == null || !x.CAT_ESCLUSE.Split(',').Any(y => qualifica.StartsWith(y))));
                }
            }

            //Cerca nome template generico(*)/sede specifica
            if (template == null)
            {
                temporary = tmp.Where(x => x.COD_TIPO == "VerbaleDOC" && x.NME_TEMPLATE.Contains("*") && x.SEDE != null && x.SEDE.ToUpper() == sede.ToUpper()).ToList();
                if (temporary != null && temporary.Any())
                    template = temporary.FirstOrDefault(x => (x.CAT_INCLUSE == null || x.CAT_INCLUSE.Split(',').Any(y => qualifica.StartsWith(y)))
                                                                && (x.CAT_ESCLUSE == null || !x.CAT_ESCLUSE.Split(',').Any(y => qualifica.StartsWith(y))));
            }

            //Cerca template generico
            if (template == null)
            {
                temporary = tmp.Where(x => x.COD_TIPO == "VerbaleDOC" && x.NME_TEMPLATE.Contains("*") && x.SEDE == null).ToList();
                if (temporary != null && temporary.Any())
                    template = temporary.FirstOrDefault(x => (x.CAT_INCLUSE == null || x.CAT_INCLUSE.Split(',').Any(y => qualifica.StartsWith(y)))
                                                                && (x.CAT_ESCLUSE == null || !x.CAT_ESCLUSE.Split(',').Any(y => qualifica.StartsWith(y))));
            }

            return template;
        }

        public static DocX GetBozzaVerbale(IncentiviEntities db, string sede, string templateName, string qualifica)
        {
            DocX doc = null;

            XR_INC_TEMPLATE allTemplate = InternalGetBozzaVerbale(db, templateName, sede, qualifica, true, true, true);// db.XR_INC_TEMPLATE.FirstOrDefault(x => x.COD_TIPO == "VerbaleDOC" && x.NME_TEMPLATE.ToUpper() == templateName && x.IND_HEADER && x.IND_BODY && x.IND_FOOTER);
            if (allTemplate != null)
            {
                MemoryStream ms = new MemoryStream(allTemplate.TEMPLATE);
                doc = DocX.Load(ms);
            }
            else
            {
                MemoryStream ms = null;

                bool hasHeader = false;

                XR_INC_TEMPLATE template = InternalGetBozzaVerbale(db, templateName, sede, qualifica, true, false, false);// db.XR_INC_TEMPLATE.FirstOrDefault(x => x.COD_TIPO == "VerbaleDOC" && x.NME_TEMPLATE.ToUpper().Contains(templateName.ToUpper()) && x.IND_HEADER && !x.IND_BODY && !x.IND_FOOTER);
                if (template != null)
                {
                    hasHeader = true;
                    ms = new MemoryStream(template.TEMPLATE);
                    doc = DocX.Load(ms);
                }

                template = InternalGetBozzaVerbale(db, templateName, sede, qualifica, false, true, false);
                //template = db.XR_INC_TEMPLATE.FirstOrDefault(x => x.COD_TIPO == "VerbaleDOC" && x.NME_TEMPLATE.ToUpper().Contains(templateName.ToUpper()) && x.IND_BODY && !x.IND_HEADER && !x.IND_FOOTER);
                //if (template == null)
                //    template = db.XR_INC_TEMPLATE.FirstOrDefault(x => x.COD_TIPO == "VerbaleDOC" && x.NME_TEMPLATE == "*" && x.IND_BODY);

                ms = new MemoryStream(template.TEMPLATE);
                if (hasHeader)
                {
                    DocX docxBody = DocX.Load(ms);
                    doc.InsertDocument(docxBody, true, false, MergingMode.Both);
                }
                else
                {
                    doc = DocX.Load(ms);
                }

                template = InternalGetBozzaVerbale(db, templateName, sede, qualifica, false, false, true);// db.XR_INC_TEMPLATE.FirstOrDefault(x => x.COD_TIPO == "VerbaleDOC" && x.NME_TEMPLATE.ToUpper().Contains(templateName.ToUpper()) && x.IND_FOOTER && !x.IND_HEADER && !x.IND_BODY);
                if (template != null)
                {
                    ms = new MemoryStream(template.TEMPLATE);
                    DocX docxFooter = DocX.Load(ms);
                    doc.InsertDocument(docxFooter, true, false, MergingMode.Both);
                }
            }



            return doc;
        }
        public static void CreaBozzaVerbale(int idDip, bool originale, out MemoryStream st, out string nomeFile, bool addWatermark = false)
        {
            IncentiviEntities db = new IncentiviEntities();
            XR_INC_DIPENDENTI dip = db.XR_INC_DIPENDENTI.Find(idDip);
            var operApp = dip.XR_INC_OPERSTATI.Where(x => x.DATA_FINE_VALIDITA == null).FirstOrDefault(x => x.ID_STATO == (int)IncStato.Appuntamento);

            XR_INC_TEMPLATE template = null;
            bool checkIncTemplate = !originale && (!dip.DATA_BOZZA_INVIO.HasValue || !dip.DATA_BOZZA_RICEZIONE.HasValue);
            if (checkIncTemplate)
                template = db.XR_INC_TEMPLATE.FirstOrDefault(x => x.ID_DIPENDENTE == idDip && x.COD_TIPO == "VerbaleDOC" && x.VALID_DTA_END == null);

            bool verbaleDefault = originale;
            if (!originale)
            {
                if ((!checkIncTemplate || template == null) && (operApp == null || operApp.XR_INC_OPERSTATI_DOC == null || operApp.XR_INC_OPERSTATI_DOC.Where(x => x.NOT_TAG == "BozzaVerbale").Count() == 0))
                    verbaleDefault = true;
            }

            string temporaryToken = "##COMPILAMI##";

            DocX doc = null;
            if (verbaleDefault)
            {
                //string sedeVerb = dip.SEDE;
                //string templateName = "";
                //if (sedeVerb.ToUpper().Contains("ROMA"))
                //    templateName = dip.ID_SIGLASIND.HasValue ? dip.XR_INC_SIGLESINDACALI.VERBALE.ToUpper() : "ROMA";
                //else
                //{
                //    if (sedeVerb.Contains('-'))
                //        sedeVerb = sedeVerb.Remove(sedeVerb.IndexOf('-')).Trim();

                //if (dip.ID_SIGLASIND.HasValue && dip.XR_INC_SIGLESINDACALI.VERBALE.ToUpper() != "ALTRO")
                //        sedeVerb = dip.XR_INC_SIGLESINDACALI.VERBALE;

                //}
                string sedeVerb = dip.SEDE;
                if (sedeVerb.Contains('-'))
                    sedeVerb = sedeVerb.Remove(sedeVerb.IndexOf('-')).Trim();
                string templateName = dip.ID_SIGLASIND.HasValue ? dip.XR_INC_SIGLESINDACALI.SINDACATO.ToUpper() : "";

                doc = GetBozzaVerbale(db, sedeVerb, templateName, dip.SINTESI1.COD_QUALIFICA);
            }
            else
            {
                if (checkIncTemplate && template != null)
                {
                    MemoryStream ms = new MemoryStream(template.TEMPLATE);
                    doc = DocX.Load(ms);
                }
                else
                {
                    var lastVersion = operApp.XR_INC_OPERSTATI_DOC.Where(x => x.NOT_TAG == "BozzaVerbale").OrderByDescending(x => x.TMS_TIMESTAMP).First();
                    MemoryStream ms = new MemoryStream(lastVersion.OBJ_OBJECT);
                    DocX temp = DocX.Load(ms);

                    if (dip.SEDE.ToUpper().Contains("ROMA") && temp.FindAll("<<INTESTAZIONE>>").Count > 0 && dip.ID_SIGLASIND.HasValue)
                    {
                        ms = new MemoryStream(db.XR_INC_TEMPLATE.FirstOrDefault(x => x.COD_TIPO == "VerbaleDOC" && x.NME_TEMPLATE.ToUpper().Contains(dip.XR_INC_SIGLESINDACALI.SINDACATO.ToUpper()) && x.IND_HEADER).TEMPLATE);
                        doc = DocX.Load(ms);
                        doc.InsertDocument(temp, true, false);
                        ms = new MemoryStream(db.XR_INC_TEMPLATE.FirstOrDefault(x => x.COD_TIPO == "VerbaleDOC" && x.NME_TEMPLATE.ToUpper().Contains(dip.XR_INC_SIGLESINDACALI.SINDACATO.ToUpper()) && x.IND_FOOTER).TEMPLATE);
                        DocX docxFooter = DocX.Load(ms);
                        doc.InsertDocument(docxFooter, true, false);

                        doc.ReplaceText("<<INTESTAZIONE>>", "");
                    }
                    else
                    {
                        doc = DocX.Load(ms);
                    }
                }
            }

            var listaToken = HrisHelper.GetParametriJson<HrisParamJson<string>>(HrisParam.IncentiviVerbaleToken);

            string tokenTFR_P4 = "";
            string tokenTFR_P5 = "";
            string tokenTFR_P6 = "";
            string tokenSomma_P6 = "";
            string[] paramTokenTFR_P4 = null;
            string[] paramTokenTFR_P5 = listaToken.FirstOrDefault(x => x.COD_PARAM == "IncentiviVerbaleTokenTFR_P5").ToArray();
            string[] paramTokenTFR_P6 = null;

            if (dip.SINTESI1.COD_QUALIFICA != "A01")
                paramTokenTFR_P6 = listaToken.FirstOrDefault(x => x.COD_PARAM == "IncentiviVerbaleTokenTFR_P6").ToArray();
            else
                paramTokenTFR_P6 = listaToken.FirstOrDefault(x => x.COD_PARAM == "IncentiviVerbaleTokenTFR_P6_A01").ToArray();


            if (dip.SINTESI1.COD_QUALIFICA == "A01")
                paramTokenTFR_P4 = listaToken.FirstOrDefault(x => x.COD_PARAM == "IncentiviVerbaleTokenTFR_P4_A01").ToArray();
            if (dip.SINTESI1.COD_QUALIFICA.StartsWith("M") || dip.SINTESI1.COD_QUALIFICA.StartsWith("A7"))
                paramTokenTFR_P4 = listaToken.FirstOrDefault(x => x.COD_PARAM == "IncentiviVerbaleTokenTFR_P4_M").ToArray();
            else
                paramTokenTFR_P4 = listaToken.FirstOrDefault(x => x.COD_PARAM == "IncentiviVerbaleTokenTFR_P4").ToArray();

            string[] paramTokenSomme_P6 = listaToken.FirstOrDefault(x => x.COD_PARAM == "IncentiviVerbaleTokenSomme_P6").ToArray();

            if (dip.IMPORTO_LORDO > 0)
            {
                tokenTFR_P4 = paramTokenTFR_P4[0];
                tokenTFR_P5 = !String.IsNullOrWhiteSpace(dip.IND_PROPRIO_IBAN) && dip.IND_PROPRIO_IBAN == "B" ? paramTokenTFR_P5[2] : paramTokenTFR_P5[0];

                if (dip.IND_PIGNORAMENTO.GetValueOrDefault()
                || dip.IND_ESTIN_ANT_PRESTITO.GetValueOrDefault()
                || dip.IND_CESSIONE_QUINTO_TFR.GetValueOrDefault())
                    tokenTFR_P6 = paramTokenTFR_P6[2];
                else
                    tokenTFR_P6 = paramTokenTFR_P6[0];

                tokenSomma_P6 = paramTokenSomme_P6[0];
            }
            else
            {
                tokenTFR_P4 = paramTokenTFR_P4[1];
                tokenTFR_P5 = !String.IsNullOrWhiteSpace(dip.IND_PROPRIO_IBAN) && dip.IND_PROPRIO_IBAN == "B" ? paramTokenTFR_P5[3] : paramTokenTFR_P5[1];

                if (dip.IND_PIGNORAMENTO.GetValueOrDefault()
                || dip.IND_ESTIN_ANT_PRESTITO.GetValueOrDefault()
                || dip.IND_CESSIONE_QUINTO_TFR.GetValueOrDefault())
                    tokenTFR_P6 = paramTokenTFR_P6[3];
                else
                    tokenTFR_P6 = paramTokenTFR_P6[1];

                tokenSomma_P6 = paramTokenSomme_P6[0];
            }
            doc.ReplaceText("<<TOKEN_TFR_P4>>", tokenTFR_P4);
            doc.ReplaceText("<<TOKEN_TFR_P5>>", tokenTFR_P5);
            doc.ReplaceText("<<TOKEN_TFR_P6>>", tokenTFR_P6);

            string[] paramTokenPuntoB = listaToken.FirstOrDefault(x => x.COD_PARAM == "IncentiviVerbaleTokenPuntoB").ToArray();
            string tokenPuntoB = "";
            if (!dip.IND_ITL.GetValueOrDefault())
                tokenPuntoB = paramTokenPuntoB[0];
            else
                tokenPuntoB = paramTokenPuntoB[1];

            doc.ReplaceText("<<TOKEN_PB>>", tokenPuntoB);

            string tokenSitDebit = "";
            string tokenSitDebitP7 = "";
            string tokenSitDebitTipo = "";
            string tokenSitDebitTipo_P6 = "";
            if (dip.IND_PIGNORAMENTO.GetValueOrDefault()
                || dip.IND_ESTIN_ANT_PRESTITO.GetValueOrDefault()
                || dip.IND_CESSIONE_QUINTO_TFR.GetValueOrDefault())
            {
                tokenSitDebit = listaToken.FirstOrDefault(x => x.COD_PARAM == "IncentiviVerbaleTokenSitDebit").COD_VALUE1;
                tokenSitDebitP7 = listaToken.FirstOrDefault(x => x.COD_PARAM == "IncentiviVerbaleTokenSitDebit_P7").COD_VALUE1;
                string chiaveTipo = "IncentiviVerbaleTokenSitDebit_Pignoramento";
                if (dip.IND_PIGNORAMENTO.GetValueOrDefault())
                    chiaveTipo = "IncentiviVerbaleTokenSitDebit_Pignoramento";
                else if (dip.IND_ESTIN_ANT_PRESTITO.GetValueOrDefault())
                    chiaveTipo = "IncentiviVerbaleTokenSitDebit_EstinzioneAnticipata";
                else if (dip.IND_CESSIONE_QUINTO_TFR.GetValueOrDefault())
                    chiaveTipo = "IncentiviVerbaleTokenSitDebit_CessioneQuinto";

                string[] tokenTipi = listaToken.FirstOrDefault(x => x.COD_PARAM == chiaveTipo).ToArray();
                tokenSitDebitTipo = tokenTipi[0];
                tokenSitDebitTipo_P6 = tokenTipi[1];
            }
            doc.ReplaceText("<<TOKEN_SITDEBIT>>", tokenSitDebit);
            doc.ReplaceText("<<TOKEN_SITDEBIT_P7>>", tokenSitDebitP7);
            doc.ReplaceText("<<TOKEN_SITDEBIT_TIPO>>", tokenSitDebitTipo);
            doc.ReplaceText("<<TOKEN_SITDEBIT_TIPO_P6>>", tokenSitDebitTipo);

            doc.ReplaceText("<<TOKEN_SOMMA_P6>>", tokenSomma_P6);

            //Dati anagrafici
            doc.ReplaceText("<<LAVORATORE>>", dip.ANAGPERS.COD_SESSO == "M" ? "il lavoratore" : "la lavoratrice");
            doc.ReplaceText("<<DENOMINAZIONE>>", dip.ANAGPERS.COD_SESSO == "M" ? "il sig." : "la sig.ra");
            doc.ReplaceText("<<NOMINATIVO>>", dip.ANAGPERS.DES_NOMEPERS.TitleCase() + " " + dip.ANAGPERS.DES_COGNOMEPERS.ToUpper());
            doc.ReplaceText("<<CITTA_RESIDENZA>>", dip.ANAGPERS.TB_COMUNE_DOM.DES_CITTA.TitleCase() + " (" + dip.ANAGPERS.TB_COMUNE_DOM.COD_PROV_STATE + ")");
            doc.ReplaceText("<<INDIRIZZO_RESIDENZA>>", dip.ANAGPERS.DES_INDIRDOM);
            doc.ReplaceText("<<NATO>>", dip.ANAGPERS.COD_SESSO == "M" ? "nato" : "nata");
            doc.ReplaceText("<<LUOGO_NASCITA>>", dip.ANAGPERS.TB_COMUNE.DES_CITTA.TitleCase() + " (" + dip.ANAGPERS.TB_COMUNE.COD_PROV_STATE + ")");
            doc.ReplaceText("<<DATA_NASCITA>>", dip.ANAGPERS.DTA_NASCITAPERS.ToString("dd MMMM yyyy"));
            doc.ReplaceText("<<CF>>", dip.ANAGPERS.CSF_CFSPERSONA);


            string qualificaSingolare = "";
            if (CommonHelper.IsProduzione())
                qualificaSingolare = db.Database.SqlQuery<string>("select DesCategoria FROM QUALIFICA_UPD where Categoria='" + dip.SINTESI1.COD_QUALIFICA + "'").FirstOrDefault();
            else
                qualificaSingolare = dip.SINTESI1.DES_QUALIFICA;

            doc.ReplaceText("<<QUALIFICA E LIVELLO>>", CezanneHelper.GetDes(dip.SINTESI1.COD_QUALIFICA, qualificaSingolare));

            //Dati contabili
            doc.ReplaceText("<<DATA_ASSUNZIONE>>", dip.DATA_ASSUNZIONE.ToString("dd MMMM yyyy"));
            doc.ReplaceText("<<DATA_CESSAZIONE>>", dip.DATA_CESSAZIONE.Value.ToString("dd MMMM yyyy"));

            long parteIntera = 0;
            int parteDecimale = 0;
            string result = "";

            if (dip.INCENTIVO_LORDO.HasValue || dip.EX_FISSA.HasValue)
            {
                decimal inc = dip.INCENTIVO_LORDO.GetValueOrDefault() + dip.EX_FISSA.GetValueOrDefault();
                doc.ReplaceText("<<INCENTIVO_NUM>>", inc.ToString("N"));
                parteIntera = (long)Decimal.Truncate(inc);
                parteDecimale = (int)(Math.Round(inc - parteIntera, 2) * 100);
                result = string.Format("{0}/{1:00}", parteIntera.ConvertNumberToReadableString(), parteDecimale);
                doc.ReplaceText("<<INCENTIVO_LETT>>", result);
            }

            if (dip.UNA_TANTUM_LORDA.HasValue)
            {
                decimal unat = dip.UNA_TANTUM_LORDA.Value;
                doc.ReplaceText("<<UNA_TANTUM_NUM>>", unat.ToString("N"));
                parteIntera = (long)Decimal.Truncate(unat);
                parteDecimale = (int)(Math.Round(unat - parteIntera, 2) * 100);
                result = string.Format("{0}/{1:00}", parteIntera.ConvertNumberToReadableString(), parteDecimale);
                doc.ReplaceText("<<UNA_TANTUM_LETT>>", result);
            }

            if (dip.IMPORTO_LORDO.HasValue)
            {
                decimal lordo = dip.IMPORTO_LORDO.Value;
                doc.ReplaceText("<<IMPORTO_LORDO_NUM>>", lordo.ToString("N"));
                parteIntera = (long)Decimal.Truncate(lordo);
                parteDecimale = (int)(Math.Round(lordo - parteIntera, 2) * 100);
                result = string.Format("{0}/{1:00}", parteIntera.ConvertNumberToReadableString(), parteDecimale);
                doc.ReplaceText("<<IMPORTO_LORDO_LETT>>", result);
            }

            var altreTratt = dip.GetField<decimal?>("AltreTrattenute", null);
            if (altreTratt.HasValue)
            {
                decimal lordo = altreTratt.Value;
                doc.ReplaceText("<<ALTRE_TRATTENUTE_NUM>>", lordo.ToString("N"));
                parteIntera = (long)Decimal.Truncate(lordo);
                parteDecimale = (int)(Math.Round(lordo - parteIntera, 2) * 100);
                result = string.Format("{0}/{1:00}", parteIntera.ConvertNumberToReadableString(), parteDecimale);
                doc.ReplaceText("<<ALTRE_TRATTENUTE_LETT>>", result);
            }

            if (!String.IsNullOrWhiteSpace(dip.IND_PROPRIO_IBAN))
            {
                string[] paramTokenAddebito = listaToken.FirstOrDefault(x => x.COD_PARAM == "IncentiviVerbaleTokenAccredito").ToArray();
                string accredito = "";
                switch (dip.IND_PROPRIO_IBAN)
                {
                    case "Y":
                    case "V":
                        //accredito = "sullo stesso conto corrente sul quale sono state finora accreditate le sue competenze mensili";
                        //doc.ReplaceText("<<ACCREDITO>>", accredito);
                        accredito = paramTokenAddebito[0];
                        break;
                    case "N":
                        //if (!String.IsNullOrWhiteSpace(dip.BANCA) && !String.IsNullOrWhiteSpace(dip.IBAN))
                        //{
                        //    accredito = "sul conto corrente a " + (dip.ANAGPERS.COD_SESSO == "M" ? "lui" : "lei") + " intestato presso la Banca " + dip.BANCA + " IBAN " + dip.IBAN;
                        //    doc.ReplaceText("<<ACCREDITO>>", accredito);
                        //}
                        accredito = paramTokenAddebito[1];
                        accredito = accredito.Replace("<<LUI_LEI>>", (dip.ANAGPERS.COD_SESSO == "M" ? "lui" : "lei"));
                        if (!String.IsNullOrWhiteSpace(dip.BANCA) && !String.IsNullOrWhiteSpace(dip.IBAN))
                        {
                            accredito = accredito.Replace("<<BANCA>>", dip.BANCA);
                            accredito = accredito.Replace("<<IBAN>>", dip.IBAN);
                        }
                        break;
                    case "B":
                        //accredito = "mediante accredito presso la Banca di Credito Cooperativo di Roma secondo le condizioni pattuite con l’Istituto";
                        //doc.ReplaceText("<<ACCREDITO>>", accredito);
                        accredito = paramTokenAddebito[2];
                        break;
                }
                doc.ReplaceText("<<ACCREDITO>>", accredito);
            }

            if (dip.DATA_PAGAMENTO.HasValue)
                doc.ReplaceText("<<DATA_PAGAMENTO>>", dip.DATA_PAGAMENTO.Value.ToString("dd MMMM yyyy"));

            //Dati appuntamento, facoltativi
            if (dip.DATA_APPUNTAMENTO.HasValue)
                doc.ReplaceText("<<DATA_APPUNTAMENTO>>", dip.DATA_APPUNTAMENTO.Value.ToString("dd MMMM yyyy"));

            string sede = dip.SEDE;
            if (!dip.SEDE.ToUpper().Contains("ROMA"))
            {
                if (dip.SEDE.ToUpper().Contains("TORINO")) sede = "Torino";
                sede = sede.TitleCase();
            }
            doc.ReplaceText("<<CITTA>>", sede);

            string sedeIndustria = temporaryToken;
            //doc.ReplaceText("<<SEDE_UNINDUSTRIA>>", sedeIndustria);
            var rapprIndustria = dip.XR_INC_RAPPRINDUSTRIA;
            if (dip.XR_INC_RAPPRINDUSTRIA == null)
                rapprIndustria = db.XR_INC_RAPPRINDUSTRIA.FirstOrDefault(x => x.SEDE.ToUpper() == sede.ToUpper() && x.COGNOME == "");

            if (dip.XR_INC_SIGLESINDACALI != null && dip.XR_INC_RAPPRINDUSTRIA != null && !String.IsNullOrWhiteSpace(dip.XR_INC_RAPPRINDUSTRIA.COGNOME))
            {
                string titolo = (!String.IsNullOrWhiteSpace(dip.XR_INC_RAPPRINDUSTRIA.TITOLO) ? dip.XR_INC_RAPPRINDUSTRIA.TITOLO + " " : "");
                if (dip.XR_INC_SIGLESINDACALI.VERBALE.ToUpper() == "ROMA 1")
                    titolo = (dip.XR_INC_RAPPRINDUSTRIA.GENERE == "M" ? "dal " : "dalla ") + titolo;
                else
                    titolo = (dip.XR_INC_RAPPRINDUSTRIA.GENERE == "M" ? "il " : "la ") + titolo;

                doc.ReplaceText("<<RAPPR_UNINDUSTRIA>>", titolo + dip.XR_INC_RAPPRINDUSTRIA.NOME + " " + dip.XR_INC_RAPPRINDUSTRIA.COGNOME);
            }

            if (rapprIndustria != null && !String.IsNullOrWhiteSpace(rapprIndustria.ORGANIZZAZIONE))
                doc.ReplaceText("<<ORG_UNINDUSTRIA>>", rapprIndustria.ORGANIZZAZIONE);

            if (dip.XR_INC_SIGLESINDACALI != null)
            {
                doc.ReplaceText("<<HEADER_SINDACATO>>", !String.IsNullOrWhiteSpace(dip.XR_INC_SIGLESINDACALI.INFO_HEADER) ? dip.XR_INC_SIGLESINDACALI.INFO_HEADER : "");
                doc.ReplaceText("<<SINDACATO>>", !String.IsNullOrWhiteSpace(dip.XR_INC_SIGLESINDACALI.DES_SINDACATO) ? dip.XR_INC_SIGLESINDACALI.DES_SINDACATO : dip.XR_INC_SIGLESINDACALI.SINDACATO);
                doc.ReplaceText("<<PREFIX_SINDACATO>>", !String.IsNullOrWhiteSpace(dip.XR_INC_SIGLESINDACALI.PREFIX_SINDACATO) ? dip.XR_INC_SIGLESINDACALI.PREFIX_SINDACATO : "");
                doc.ReplaceText("<<PREFIX_SINDACATO_FOOTER>>", !String.IsNullOrWhiteSpace(dip.XR_INC_SIGLESINDACALI.PREFIX_SINDACATO_FOOTER) ? dip.XR_INC_SIGLESINDACALI.PREFIX_SINDACATO_FOOTER : "");
            }

            if (dip.XR_INC_RAPPRSINDACATO != null && !String.IsNullOrWhiteSpace(dip.XR_INC_RAPPRSINDACATO.COGNOME))
                doc.ReplaceText("<<RAPPR_SIND>>", (!String.IsNullOrWhiteSpace(dip.XR_INC_RAPPRSINDACATO.TITOLO) ? dip.XR_INC_RAPPRSINDACATO.TITOLO + " " : "") + dip.XR_INC_RAPPRSINDACATO.NOME + " " + dip.XR_INC_RAPPRSINDACATO.COGNOME);

            if (dip.XR_INC_RAPPRRAI != null && dip.XR_INC_RAPPRRAI.ID_RAPPRRAI > 0)
                doc.ReplaceText("<<RAPPR_RAI>>", (!String.IsNullOrWhiteSpace(dip.XR_INC_RAPPRRAI.TITOLO) ? dip.XR_INC_RAPPRRAI.TITOLO + " " : "") + dip.XR_INC_RAPPRRAI.ANAGPERS.DES_NOMEPERS.TitleCase() + " " + dip.XR_INC_RAPPRRAI.ANAGPERS.DES_COGNOMEPERS.TitleCase());

            var paramTokenProcura = listaToken.FirstOrDefault(x => x.COD_PARAM == "IncentiviVerbaleTokenProcura");
            if (dip.SINTESI1.COD_QUALIFICA == "A72")
                doc.ReplaceText("<<PROCURA>>", paramTokenProcura.COD_VALUE2);
            else
                doc.ReplaceText("<<PROCURA>>", paramTokenProcura.COD_VALUE1);
            doc.ReplaceText("<<PROCURA_EXT>>", paramTokenProcura.COD_VALUE3);

            string[] rifGruppo = HrisHelper.GetParametri<string>(HrisParam.IncentiviRifGruppo);
            doc.ReplaceText("<<CIRCOLARE_PROT>>", rifGruppo[2]);
            doc.ReplaceText("<<CIRCOLARE_DATA>>", rifGruppo[3]);

            doc.ReplaceText("<<MODELLO>>", dip.GetField<string>("VeicoloTipologia", ""));
            doc.ReplaceText("<<TARGA>>", dip.GetField<string>("VeicoloTarga", ""));

            if (addWatermark)
            {
                doc.AddHeaders();

                Formatting formatting = new Formatting();

                var firstH = doc.Headers.First;
                var firstP = firstH.InsertParagraph();
                firstP.Append("Bozza verbale");
                firstP.Alignment = Alignment.right;
                //var templWM = db.XR_INC_TEMPLATE.FirstOrDefault(x => x.COD_TIPO == "watermark");
                //var image = doc.AddImage(new MemoryStream(templWM.TEMPLATE), "image/png");
                //var picture = image.CreatePicture(400, 200);
                //picture.Rotation = 45;
                //picture.SetPictureShape(RectangleShapes.rect);
                //firstP.InsertPicture(picture);

                var evenH = doc.Headers.Even;
                var evenP = evenH.InsertParagraph();
                evenP.Append("Bozza verbale");
                evenP.Alignment = Alignment.right;

                var oddH = doc.Headers.Odd;
                var oddP = oddH.InsertParagraph();
                oddP.Append("Bozza verbale");
                oddP.Alignment = Alignment.right;
            }

            st = new MemoryStream();
            doc.SaveAs(st);
            st.Position = 0;

            nomeFile = "Verbale " + dip.ANAGPERS.DES_COGNOMEPERS.TitleCase() + " " + dip.ANAGPERS.DES_NOMEPERS.TitleCase() + ".docx";
        }
        public static decimal CalcoloOneri(string qualifica, decimal una_tantum)
        {
            decimal oneri = 0;

            var listOneri = HrisHelper.GetParametriJson<CessazioneOneri>(HrisParam.IncentiviOneri);
            if (listOneri == null)
            {
                var fSuper = CezanneHelper.GetFSuperCat();
                var giornalisti = new string[] { "M", "A7" };

                if (una_tantum > 0)
                {
                    if (fSuper.Any(x => qualifica.StartsWith(x)))
                        oneri = (una_tantum / 100m * 28.53m);
                    else if (giornalisti.Any(x => qualifica.StartsWith(x)))
                        oneri = (una_tantum / 100m * 26m);
                    else
                        oneri = (una_tantum / 100m * 29.28m);
                }
            }
            else
            {
                var percOneri = listOneri.FirstOrDefault(x => x.FiltriQualifiche != null && x.FiltriQualifiche.Any(y => qualifica.StartsWith(y)));
                if (percOneri == null)
                    percOneri = listOneri.FirstOrDefault(x => x.FiltriQualifiche == null || !x.FiltriQualifiche.Any());

                oneri = (una_tantum / 100m * percOneri.Percentuale);
            }

            return oneri;
        }
        public static MemoryStream SampleBozzaVerbale(CessazioneBozzaSample model, IncentiviEntities db)
        {
            DocX doc = CessazioneHelper.GetBozzaVerbale(db, model.Sede ?? "", model.Template, model.Qualifica);

            doc.ReplaceText("<<CITTA>>", model.Sede ?? "".TitleCase());
            var rapprIndustria = db.XR_INC_RAPPRINDUSTRIA.FirstOrDefault(x => x.SEDE.ToUpper() == model.Sede.ToUpper() && x.COGNOME == "-");
            if (rapprIndustria != null && !String.IsNullOrWhiteSpace(rapprIndustria.ORGANIZZAZIONE))
                doc.ReplaceText("<<ORG_UNINDUSTRIA>>", rapprIndustria.ORGANIZZAZIONE);

            var listaToken = HrisHelper.GetParametriJson<HrisParamJson<string>>(HrisParam.IncentiviVerbaleToken);

            var paramTokenProcura = listaToken.FirstOrDefault(x => x.COD_PARAM == "IncentiviVerbaleTokenProcura");
            if (model.Qualifica == "A72")
                doc.ReplaceText("<<PROCURA>>", paramTokenProcura.COD_VALUE2);
            else
                doc.ReplaceText("<<PROCURA>>", paramTokenProcura.COD_VALUE1);
            doc.ReplaceText("<<PROCURA_EXT>>", paramTokenProcura.COD_VALUE3);

            if (!String.IsNullOrWhiteSpace(model.Sindacato))
            {
                var sind = db.XR_INC_SIGLESINDACALI.FirstOrDefault(x => x.SINDACATO == model.Sindacato && x.DATA_FINE_VALIDITA == null);
                doc.ReplaceText("<<HEADER_SINDACATO>>", !String.IsNullOrWhiteSpace(sind.INFO_HEADER) ? sind.INFO_HEADER : "");
                doc.ReplaceText("<<SINDACATO>>", !String.IsNullOrWhiteSpace(sind.DES_SINDACATO) ? sind.DES_SINDACATO : sind.SINDACATO);
                doc.ReplaceText("<<PREFIX_SINDACATO>>", !String.IsNullOrWhiteSpace(sind.PREFIX_SINDACATO) ? sind.PREFIX_SINDACATO : "");
                doc.ReplaceText("<<PREFIX_SINDACATO_FOOTER>>", !String.IsNullOrWhiteSpace(sind.PREFIX_SINDACATO_FOOTER) ? sind.PREFIX_SINDACATO_FOOTER : "");
            }

            string[] rifGruppo = HrisHelper.GetParametri<string>(HrisParam.IncentiviRifGruppo);
            doc.ReplaceText("<<CIRCOLARE_PROT>>", rifGruppo[2]);
            doc.ReplaceText("<<CIRCOLARE_DATA>>", rifGruppo[3]);

            MemoryStream st = new MemoryStream();
            doc.SaveAs(st);
            st.Position = 0;
            return st;
        }

        public static MemoryStream IncludeWatermark(XR_INC_TEMPLATE modifiedVerb)
        {
            MemoryStream ms = new MemoryStream(modifiedVerb.TEMPLATE);
            DocX docx = DocX.Load(ms);
            docx.Headers.First.ReplaceText("Bozza verbale", "");
            docx.Headers.Odd.ReplaceText("Bozza verbale", "");
            docx.Headers.Even.ReplaceText("Bozza verbale", "");
            docx.Save();

            MemoryStream st = new MemoryStream();
            docx.SaveAs(st);
            st.Position = 0;
            return st;
        }

        public static DateTime? GetDataPagamento(IncentiviEntities db, XR_INC_DIPENDENTI dip)
        {
            DateTime? dataPagamento = null;

            dataPagamento = dip.DATA_PAGAMENTO;

            return dataPagamento;
        }

        public static bool GetRuoloProtocollo(string matricola, out string ruolo)
        {
            bool result = false;
            ruolo = null;

            using (IncentiviEntities db = new IncentiviEntities())
            {
                XR_INC_PARAM_MAIL param = db.XR_INC_PARAM_MAIL.FirstOrDefault(x => x.COD_CAMPO == "ProtocolloAccettazione" && x.COD_MATRICOLA == matricola);
                if (param != null && !String.IsNullOrWhiteSpace(param.VALORE))
                {
                    result = true;
                    ruolo = param.VALORE;
                }
            }

            return result;
        }

        public static string GetMailDip(XR_INC_DIPENDENTI dip)
        {
            string result = "";

            if (dip.DATA_CESSAZIONE >= DateTime.Today || String.IsNullOrWhiteSpace(dip.MAIL))
                result = CommonHelper.GetEmailPerMatricola(dip.MATRICOLA);
            else
                result = dip.MAIL.Trim();

            if (String.IsNullOrWhiteSpace(dip.MAIL) && !String.IsNullOrWhiteSpace(result))
            {
                var db = new IncentiviEntities();
                var dipCZ = db.XR_INC_DIPENDENTI.Where(x => x.ID_DIPENDENTE == dip.ID_DIPENDENTE).FirstOrDefault();
                if (dipCZ != null)
                {
                    dipCZ.MAIL = result;
                    db.SaveChanges();
                }
            }

            return result;
        }


        public static bool NomeFunzione(XR_INC_DIPENDENTI dip, string mittente, string codTipo, string filtro, bool getPersonaleTemplate, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            IncentiviEntities db = new IncentiviEntities();
            var template = GetTemplate(db, codTipo, dip.ID_DIPENDENTE, filtro, getPersonaleTemplate, dip.SINTESI1.COD_QUALIFICA);
            GestoreMail mail = new GestoreMail();
            string mailDest = GetMailDip(dip);
            string mailOggetto = ReplaceToken(dip, template.DES_TEMPLATE);
            string mailTesto = ReplaceToken(dip, template.TEMPLATE_TEXT);
            string mailMittente = GetIndirizzoMail(mittente);
            string mailCC = mailMittente;
            var response = mail.InvioMail(mailTesto, mailOggetto, mailDest, mailCC, mittente, null, null);
            if (response == null || response.Errore != null)
            {
                errorMsg = response != null ? "Errore imprevisto" : response.Errore;
            }
            else
                result = true;

            return result;
        }

        public static CessazioneCertServizio GetCertificatoServizio(int idDip)
        {
            CessazioneCertServizio certificato = new CessazioneCertServizio();
            var db = new IncentiviEntities();
            var dip = db.XR_INC_DIPENDENTI.Find(idDip);
            certificato.Dipendente = dip;
            certificato.Sintesi = dip.SINTESI1;
            certificato.Protocollo = dip.GetField<string>("ProtocolloCertServizioCod");
            certificato.DataGenerazione = dip.GetField<DateTime?>("DataCertServizio");
            certificato.CertServInGenerazione = dip.GetField("CertServInGenerazione", false);
            certificato.MailDataInvio = dip.GetField<DateTime?>("DataInvioCertServizio");
            certificato.MailUtenteInvio = dip.GetField<string>("UtenteInvioCertServizio");

            var template = CessazioneHelper.GetTemplate(db, "CertificatoServizio", idDip, "", true, dip.SINTESI1.COD_QUALIFICA);
            var templatePDF = CessazioneHelper.GetTemplate(db, "CertificatoServizioPDF", idDip, "", true, dip.SINTESI1.COD_QUALIFICA);
            certificato.HasPDF = templatePDF != null;

            MemoryStream ms = new MemoryStream(template.TEMPLATE);
            DocX doc = DocX.Load(ms);
            Dictionary<string, object> additionalToken = new Dictionary<string, object>();

            var comune = db.TB_COMUNE.Find(dip.SINTESI1.COD_CITTANASC);
            string qualificaSingolare = "";
            var ctx = new myRaiData.CurriculumVitae.cv_ModelEntities();
            var figProf = ctx.DFiguraPro.FirstOrDefault(x => x.CodiceFiguraPro == dip.SINTESI1.QUALIFICA.COD_QUALSTD);
            if (figProf != null)
                qualificaSingolare = figProf.DescriFiguraPro.ToLower().Trim();

            if (qualificaSingolare.Contains("f1"))
                qualificaSingolare = qualificaSingolare.Replace("f1", "");

            string livello = "";
            var Livelli = new Dictionary<string, string>()
            {
                { "0A", "" },
                { "01", "1" },
                { "02", "2" },
                { "03", "3" },
                { "04", "4" },
                { "05", "5" },
                { "06", "6" },
                { "07", "7" }
            };

            string queryHrdw = String.Format("SELECT [classe_retribuz] FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_CATEGORIA_LIVELLO] where categoria='{0}'", dip.SINTESI1.COD_QUALIFICA);
            string tmpLivello = db.Database.SqlQuery<string>(queryHrdw).FirstOrDefault();
            if (!String.IsNullOrWhiteSpace(tmpLivello))
            {
                if (!Livelli.TryGetValue(tmpLivello, out livello))
                    livello = "--livello non trovato--";
            }

            additionalToken.Add("PROTOCOLLO", certificato.Protocollo ?? "__PROTOCOLLO__");
            additionalToken.Add("LUOGO_NASCITA", comune != null ? comune.DES_CITTA.TitleCase() + " (" + comune.COD_PROV_STATE + ")" : "");
            additionalToken.Add("DATA_NASCITA", dip.SINTESI1.DTA_NASCITAPERS);
            additionalToken.Add("INIZIO_TI", dip.DATA_ANZIANITA);
            additionalToken.Add("FINE_TI", dip.DATA_CESSAZIONE);
            additionalToken.Add("QUALIFICA", qualificaSingolare);
            additionalToken.Add("LIVELLO", livello);

            HrisHelper.DocReplaceToken(dip.SINTESI1, dip, doc, "__", additionalToken);
            certificato.Custom = template.ID_DIPENDENTE.HasValue;

            certificato.PrimoParagrafo = doc.Bookmarks.FirstOrDefault(x => x.Name == "PrimoParagrafo").Paragraph.Text;
            certificato.SecondoParagrafo = doc.Bookmarks.FirstOrDefault(x => x.Name == "SecondoParagrafo").Paragraph.Text;
            certificato.TerzoParagrafo = doc.Bookmarks.FirstOrDefault(x => x.Name == "TerzoParagrafo").Paragraph.Text;

            string mailCC = "";
            var tmpExtraCC = db.XR_INC_PARAM_MAIL.Where(x => x.COD_CAMPO == "CC Certificato").ToList();
            if (tmpExtraCC != null && tmpExtraCC.Any())
            {

                tmpExtraCC = tmpExtraCC.Where(x => (x.COD_SERVIZIO == null || x.COD_SERVIZIO.Contains(dip.SINTESI1.COD_SERVIZIO))
                                                && (x.COD_SERVIZIO_ESCL == null || !x.COD_SERVIZIO.Contains(dip.SINTESI1.COD_SERVIZIO))
                                                && (x.SEDI_INCLUSE == null || x.SEDI_INCLUSE.Split(',').Contains(dip.SINTESI1.COD_SEDE.Substring(0, 2) + "*") || x.SEDI_INCLUSE.Split(',').Contains(dip.SINTESI1.COD_SEDE.Substring(0, 2) + dip.SINTESI1.COD_SERVIZIO))
                                                && (x.SEDI_ESCLUSE == null || (!x.SEDI_ESCLUSE.Split(',').Contains(dip.SINTESI1.COD_SEDE.Substring(0, 2) + "*") && !x.SEDI_ESCLUSE.Split(',').Contains(dip.SINTESI1.COD_SEDE.Substring(0, 2) + dip.SINTESI1.COD_SERVIZIO)))
                                                ).ToList();

                var extraCC = tmpExtraCC.Select(x => x.VALORE);
                if (extraCC != null && extraCC.Any())
                {
                    foreach (var item in extraCC)
                    {
                        mailCC += (!String.IsNullOrWhiteSpace(mailCC) ? ";" : "") + item;
                    }
                }
            }
            certificato.MailCC = mailCC;

            var mailCCN = "";
            var tmpExtraCCN = db.XR_INC_PARAM_MAIL.Where(x => x.COD_CAMPO == "CCN Certificato").ToList();
            if (tmpExtraCCN != null && tmpExtraCCN.Any())
            {

                tmpExtraCCN = tmpExtraCCN.Where(x => (x.COD_SERVIZIO == null || x.COD_SERVIZIO.Contains(dip.SINTESI1.COD_SERVIZIO))
                                                && (x.COD_SERVIZIO_ESCL == null || !x.COD_SERVIZIO.Contains(dip.SINTESI1.COD_SERVIZIO))
                                                && (x.SEDI_INCLUSE == null || x.SEDI_INCLUSE.Split(',').Contains(dip.SINTESI1.COD_SEDE.Substring(0, 2) + "*") || x.SEDI_INCLUSE.Split(',').Contains(dip.SINTESI1.COD_SEDE.Substring(0, 2) + dip.SINTESI1.COD_SERVIZIO))
                                                && (x.SEDI_ESCLUSE == null || (!x.SEDI_ESCLUSE.Split(',').Contains(dip.SINTESI1.COD_SEDE.Substring(0, 2) + "*") && !x.SEDI_ESCLUSE.Split(',').Contains(dip.SINTESI1.COD_SEDE.Substring(0, 2) + dip.SINTESI1.COD_SERVIZIO)))
                                                ).ToList();

                var extraCCN = tmpExtraCCN.Select(x => x.VALORE);
                if (extraCCN != null && extraCCN.Any())
                {
                    foreach (var item in extraCCN)
                    {
                        mailCCN += (!String.IsNullOrWhiteSpace(mailCCN) ? ";" : "") + item;
                    }
                }
            }
            if (String.IsNullOrWhiteSpace(mailCCN))
                mailCCN = null;
            certificato.MailCCN = mailCCN;

            var templateMail = CessazioneHelper.GetTemplate(db, "TemplateMail", dip.ID_DIPENDENTE, "MailCertificatoServizio", true, dip.SINTESI1.COD_QUALIFICA);
            certificato.MailTesto = CessazioneHelper.ReplaceToken(dip, templateMail.TEMPLATE_TEXT);
            if (templateMail.ID_DIPENDENTE.HasValue)
            {
                //Il testo della mail quindi non è quello di default, ma uno modificato ad hoc per la persona
                certificato.MailUtenteModifica = templateMail.COD_USER;
                certificato.MailDataModifica = templateMail.TMS_TIMESTAMP;
            }

            return certificato;
        }

        public static bool GeneraCertificatoServizio(int idDip, string primo, string secondo, out string errorMsg)
        {
            bool result = false;
            errorMsg = null;

            var db = new IncentiviEntities();
            var dip = db.XR_INC_DIPENDENTI.Find(idDip);

            var template = CessazioneHelper.GetTemplate(db, "CertificatoServizio", idDip, "", true, dip.SINTESI1.COD_QUALIFICA);

            MemoryStream ms = new MemoryStream(template.TEMPLATE);
            DocX doc = DocX.Load(ms);

            Bookmark bmPrimo = doc.Bookmarks.FirstOrDefault(x => x.Name == "PrimoParagrafo");
            Bookmark bmSecondo = doc.Bookmarks.FirstOrDefault(x => x.Name == "SecondoParagrafo");

            bmPrimo.Paragraph.ReplaceText(bmPrimo.Paragraph.Text, primo);
            bmSecondo.Paragraph.ReplaceText(bmSecondo.Paragraph.Text, secondo);

            MemoryStream outMs = new MemoryStream();
            doc.SaveAs(outMs);

            CezanneHelper.GetCampiFirma(out CampiFirma campiFirma);
            if (template.ID_DIPENDENTE.HasValue)
            {
                template.TEMPLATE = outMs.ToArray();
                template.COD_USER = campiFirma.CodUser;
                template.COD_TERMID = campiFirma.CodTermid;
                template.TMS_TIMESTAMP = campiFirma.Timestamp;
            }
            else
            {
                var newTemplate = new XR_INC_TEMPLATE()
                {
                    CAT_ESCLUSE = null,
                    CAT_INCLUSE = null,
                    COD_TERMID = campiFirma.CodTermid,
                    COD_USER = campiFirma.CodUser,
                    COD_TIPO = "CertificatoServizio",
                    CONTENT_TYPE = "",
                    DES_TEMPLATE = "",
                    DIR_ESCLUSE = null,
                    DIR_INCLUSE = null,
                    ID_DIPENDENTE = dip.ID_DIPENDENTE,
                    IND_BODY = true,
                    IND_FOOTER = true,
                    IND_HEADER = true,
                    IND_SIGN = false,
                    NME_TEMPLATE = "",
                    SEDE = null,
                    TEMPLATE = outMs.ToArray(),
                    TEMPLATE_TEXT = null,
                    TMS_TIMESTAMP = campiFirma.Timestamp,
                    VALID_DTA_END = null,
                    VALID_DTA_INI = campiFirma.Timestamp
                };
                db.XR_INC_TEMPLATE.Add(newTemplate);
            }

            dip.SetField("CertServInGenerazione", true);
            result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), "Salvataggio certificato servizio doc");
            if (!result)
                errorMsg = "Errore durante il salvataggio";

            return result;
        }

        public static void GetImportoAltreTrattenute(IncentiviEntities db, XR_INC_DIPENDENTI dip)
        {
            string queryAltreTrattenute = String.Format("select RIGA from openquery(DB2LINK, 'select * from PROD.TLIQ_PROSPETTO  where matricola=''0{0}'' and riga like ''%ALTRE TRATTENUTE%''')", dip.MATRICOLA);
            var res = db.Database.SqlQuery<string>(queryAltreTrattenute);
            if (res != null && res.Any())
            {
                string[] tokenAT = res.First().Split(':');
                if (Decimal.TryParse(tokenAT[2].Trim(), NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands, new NumberFormatInfo() { CurrencyDecimalSeparator = ",", CurrencyGroupSeparator = ".", NumberDecimalSeparator = ",", NumberGroupSeparator = "." }, out var importAT))
                {
                    dip.SetField("AltreTrattenute", importAT);
                }
            }
        }
    }
}
