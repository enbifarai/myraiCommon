using myRaiData.Incentivi;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Xml.Serialization;

namespace myRaiHelper
{
    public enum AbilState
    {
        Enabled,
        NotEnabled,
        Error
    }

    public enum AbilProvenienza
    {
        HRGA,
        DB,
        Override
    }
    public enum AbilOrigine
    {
        HRGA,
        DB,
        Override,
        Integrazione
    }
    public class TemporaryInfo<T>
    {
        private int _minuteScope;

        [JsonProperty]
        private T _information;
        [JsonProperty]
        private DateTime? _lastUpdate;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minuteScope">Durata dello scope in minuti. 0: aggiorna sempre; -1: non aggiorna mai</param>
        //public TemporaryInfo(int minuteScope = 5)
        public TemporaryInfo(int minuteScope = 5)
        {
            _minuteScope = minuteScope;
        }

        public void SetInfo(T info)
        {
            _lastUpdate = DateTime.Now;
            _information = info;
        }
        public T GetInfo()
        {
            if (_information == null)
                return default(T);
            else
                return _information;
        }

        public bool NeedRefresh()
        {
            bool needIt = false;
            DateTime current = DateTime.Now;

            switch (_minuteScope)
            {
                case -1:
                    needIt = false;
                    break;
                case 0:
                    needIt = true;
                    break;
                default:
                    if (_lastUpdate.HasValue)
                    {
                        TimeSpan t1 = current.Subtract(_lastUpdate.Value);
                        double minuti = t1.TotalMinutes;

                        if (minuti > _minuteScope)
                        {
                            // se son passati più di 5 minuti azzera lo scope
                            needIt = true;
                        }
                    }
                    else
                    {
                        needIt = true;
                    }
                    break;
            }

            return needIt;
        }
    }


    public enum AbilFilterData
    {
        Cat,
        Dir,
        Sedi,
        Societa,
        Matr,
        Tip,
        Contr
    }
    public enum AbilFilterTipo
    {
        CategoriaDato,
        Livelli,
        Nessuno
    }
    public class BaseAbil
    {
        public BaseAbil()
        {
            CategorieAbilitate = new TemporaryInfo<AbilCat>();
            DirezioniAbilitate = new TemporaryInfo<AbilDir>();
            SediAbilitate = new TemporaryInfo<AbilSedi>();
            SocietaAbilitate = new TemporaryInfo<AbilSocieta>();
            MatricoleAbilitate = new TemporaryInfo<AbilMatr>();
            TipologieAbilitate = new TemporaryInfo<AbilTip>();
            ContrattiAbilitati = new TemporaryInfo<AbilContr>();
            MatricoleLivInfo = new Dictionary<string, TemporaryInfo<AbilMatrLiv>>();
            State = AbilState.NotEnabled;
        }

        public string Nome { get; set; }
        public string Matricola { get; set; }
        public AbilState State { get; set; }

        public TemporaryInfo<AbilCat> CategorieAbilitate;
        public TemporaryInfo<AbilDir> DirezioniAbilitate;
        public TemporaryInfo<AbilSedi> SediAbilitate;
        public TemporaryInfo<AbilSocieta> SocietaAbilitate;
        public TemporaryInfo<AbilMatr> MatricoleAbilitate;
        public TemporaryInfo<AbilContr> ContrattiAbilitati;
        public TemporaryInfo<AbilTip> TipologieAbilitate;

        public TemporaryInfo<T> GetTemporary<T>(AbilFilterData data) where T : BaseAbilFilter
        {
            TemporaryInfo<T> temporary = null;

            switch (data)
            {
                case AbilFilterData.Cat:
                    temporary = CategorieAbilitate as TemporaryInfo<T>;
                    break;
                case AbilFilterData.Dir:
                    temporary = DirezioniAbilitate as TemporaryInfo<T>;
                    break;
                case AbilFilterData.Sedi:
                    temporary = SediAbilitate as TemporaryInfo<T>;
                    break;
                case AbilFilterData.Societa:
                    temporary = SocietaAbilitate as TemporaryInfo<T>;
                    break;
                case AbilFilterData.Matr:
                    temporary = MatricoleAbilitate as TemporaryInfo<T>;
                    break;
                case AbilFilterData.Tip:
                    temporary = TipologieAbilitate as TemporaryInfo<T>;
                    break;
                case AbilFilterData.Contr:
                    temporary = ContrattiAbilitati as TemporaryInfo<T>;
                    break;
                default:
                    throw new NotImplementedException();
            }

            return temporary;
        }

        public Dictionary<string, TemporaryInfo<AbilMatrLiv>> MatricoleLivInfo;
    }

    public class AbilFunc : BaseAbil
    {
        public AbilFunc() : base()
        {
            SubFuncs = new Dictionary<string, AbilSubFunc>();
        }
        public Dictionary<string, AbilSubFunc> SubFuncs { get; set; }
        public AbilProvenienza Provenienza { get; set; }
    }

    public class AbilSubFunc : BaseAbil
    {
        public AbilSubFunc()
        {
            Create = true;
            Read = true;
            Update = true;
            Delete = true;
        }
        public string Funzione { get; set; }
        public bool IsIntegration { get; set; }
        public bool Create { get; set; }
        public bool Read { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }
        public AbilProvenienza Provenienza { get; set; }
    }


    public class BaseAbilFilter
    {
        public BaseAbilFilter()
        {
            Enabled = false;
            HasFilter = false;

            Filter = new LivFilter();
            OrigFilter = new LivFilter();
        }

        public string Funzione { get; set; }
        public string Sottofunzione { get; set; }
        public bool HasFilter { get; set; }
        public bool OriginalHasFilter { get; set; }
        public bool Enabled { get; set; }
        public List<string> Incluse(params LivFilter.LivEnum[] livelli)
        {
            return Filter.GetIncluse(livelli);
        }
        public List<string> Escluse(params LivFilter.LivEnum[] livelli)
        {
            return Filter.GetEscluse(livelli);
        }
        public LivFilter GetFilter(bool total=true)
        {
            if (total)
                return Filter;
            else
                return Origine == AbilOrigine.Integrazione ? OrigFilter : Filter;
        }
        public bool GetHasFilter(bool total=true)
        {
            if (total)
                return HasFilter;
            else
                return Origine == AbilOrigine.Integrazione ? OriginalHasFilter : HasFilter;
        }
        public LivFilter Filter { get; set; }
        public LivFilter OrigFilter { get; set; }
        public AbilOrigine Origine { get; set; }
    }

    public class BaseFilter
    {
        public BaseFilter()
        {
            Incluse = new List<string>();
            Escluse = new List<string>();
        }
        public bool IsEnabled { get; set; }
        public List<string> Incluse { get; set; }
        public List<string> Escluse { get; set; }

        internal void SetInfo(IEnumerable<string> incluse, IEnumerable<string> escluse)
        {
            IsEnabled = true;

            var incl = incluse.Union(Incluse.Where(x => !escluse.Contains(x))).Distinct().Where(x => !String.IsNullOrWhiteSpace(x)).ToList();
            var escl = escluse.Union(Escluse.Where(x => !incluse.Contains(x))).Distinct().Where(x => !String.IsNullOrWhiteSpace(x)).ToList();

            Incluse.Clear();
            Escluse.Clear();

            Incluse.AddRange(incl);
            Escluse.AddRange(escl);
        }
    }

    public class LivFilter
    {
        public enum LivEnum
        {
            Anagrafico,
            Gestionale,
            Retributivo
        }

        Dictionary<LivEnum, BaseFilter> _livFilters;

        public LivFilter()
        {
            _livFilters = new Dictionary<LivEnum, BaseFilter>();
            _livFilters.Add(LivEnum.Anagrafico, new BaseFilter());
            _livFilters.Add(LivEnum.Gestionale, new BaseFilter());
            _livFilters.Add(LivEnum.Retributivo, new BaseFilter());
        }
        public void CopyTo(LivFilter dest)
        {
            foreach (var item in _livFilters)
                dest.SetInfo(item.Value.Incluse, item.Value.Escluse, item.Key);
        }

        public void GetInfo(out List<string> incluse, out List<string> escluse, params LivEnum[] livelli)
        {
            incluse = new List<string>();
            escluse = new List<string>();

            var _incluse = new List<string>();
            var _escluse = new List<string>();

            var list = GetLivelli(livelli).Select(x => x.Value);
            _incluse.AddRange(list.SelectMany(x => x.Incluse).Distinct());
            _escluse.AddRange(list.SelectMany(x => x.Escluse).Distinct().Where(x => !_incluse.Contains(x)));

            incluse.AddRange(_incluse);
            escluse.AddRange(_escluse);
        }

        public List<string> GetIncluse(params LivEnum[] livelli)
        {
            var incluse = new List<string>();
            var list = GetLivelli(livelli).Select(x => x.Value);
            incluse.AddRange(list.SelectMany(x => x.Incluse).Distinct());
            return incluse;
        }
        public List<string> GetEscluse(params LivEnum[] livelli)
        {
            var incluse = new List<string>();
            var escluse = new List<string>();

            var list = GetLivelli(livelli).Select(x => x.Value);
            incluse.AddRange(list.SelectMany(x => x.Incluse).Distinct());
            escluse.AddRange(list.SelectMany(x => x.Escluse).Distinct().Where(x => !incluse.Contains(x)));

            return escluse;
        }

        public void SetInfo(string strIncluse, string strEscluse, params LivEnum[] livelli)
        {
            var incluse = strIncluse.Split(',');
            var escluse = strEscluse.Split(',');

            SetInfo(incluse, escluse, livelli);
        }
        public void SetInfo(IEnumerable<string> incluse, IEnumerable<string> escluse, params LivEnum[] livelli)
        {
            foreach (var item in _livFilters)
            {
                if (livelli == null || !livelli.Any() || livelli.Contains(item.Key))
                    item.Value.SetInfo(incluse, escluse);
            }
        }
        public void SetIncluse(IEnumerable<string> incluse, params LivEnum[] livelli)
        {
            foreach (var item in _livFilters)
            {
                if (livelli == null || !livelli.Any() || livelli.Contains(item.Key))
                    item.Value.SetInfo(incluse, Enumerable.Empty<string>());
            }
        }
        public void SetEscluse(IEnumerable<string> escluse, params LivEnum[] livelli)
        {
            foreach (var item in _livFilters)
            {
                if (livelli == null || !livelli.Any() || livelli.Contains(item.Key))
                    item.Value.SetInfo(Enumerable.Empty<string>(), escluse);
            }
        }

        public void PatchData(string check, string add)
        {
            foreach (var item in _livFilters)
            {
                if (item.Value.Incluse.Contains(check)) item.Value.Incluse.Add(add);
                if (item.Value.Escluse.Contains(check)) item.Value.Escluse.Add(add);
            }
        }

        private IEnumerable<KeyValuePair<LivEnum, BaseFilter>> GetLivelli(LivEnum[] livelli)
        {
            var result = _livFilters.AsEnumerable();
            if (livelli != null && livelli.Any())
                result = result.Where(x => livelli.Contains(x.Key));
            return result;
        }
        public static LivEnum ConvertStrToLiv(string liv)
        {
            switch (liv.ToUpper())
            {
                case "ANAG":
                    return LivEnum.Anagrafico;
                case "GEST":
                    return LivEnum.Gestionale;
                case "RETR":
                    return LivEnum.Retributivo;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    public class AbilInfo
    {
        public AbilCat AbilCat { get; set; }
        public AbilDir AbilDir { get; set; }
        public AbilSedi AbilSedi { get; set; }
        public AbilSocieta AbilSocieta { get; set; }
        public AbilMatr AbilMatr { get; set; }
        public AbilTip AbilTip { get; set; }
        public AbilContr AbilContr { get; set; }

        public Expression<Func<T, bool>> GetSintesiFilter<T>(bool total, params LivFilter.LivEnum[] livelli) where T : class, myRai.Data.Interface.ISintesi1
        {
            Expression<Func<T, bool>> expr = null;

            if (AbilCat.GetHasFilter(total))
            {
                var incluse = AbilCat.GetFilter(total).GetIncluse(livelli);
                var escluse = AbilCat.GetFilter(total).GetEscluse(livelli);

                if (incluse.Any() || !escluse.Any())
                    ////result = result.Where(x => incluse.Any(y => x.COD_QUALIFICA.StartsWith(y)));
                    expr = LinqHelper.PutInAndTogether(expr, x => incluse.Any(y => x.COD_QUALIFICA.StartsWith(y)));

                if (escluse.Any())
                    //result = result.Where(x => !escluse.Any(y => x.COD_QUALIFICA.StartsWith(y)));
                    expr = LinqHelper.PutInAndTogether(expr, x => !escluse.Any(y => x.COD_QUALIFICA.StartsWith(y)));
            }

            // FRANCESCO 05/12/2022
            if (AbilDir.GetHasFilter(total)|| AbilSedi.GetHasFilter(total))
            {
                var dirIncluse = AbilDir.GetFilter(total).GetIncluse(livelli);
                var dirEscluse = AbilDir.GetFilter(total).GetEscluse(livelli);
                var sediIncluse = AbilSedi.GetFilter(total).GetIncluse(livelli);
                var sediEscluse = AbilSedi.GetFilter(total).GetEscluse(livelli);

                if (dirIncluse == null)
                {
                    dirIncluse = new List<string>();
                }

                if (dirEscluse == null)
                {
                    dirEscluse = new List<string>();
                }

                if (sediIncluse == null)
                {
                    sediIncluse = new List<string>();
                }

                if (sediEscluse == null)
                {
                    sediEscluse = new List<string>();
                }

                expr = LinqHelper.PutInAndTogether(expr,
                    x => (dirIncluse.Contains(x.COD_SERVIZIO) && !dirEscluse.Contains(x.COD_SERVIZIO))
                    || (
                        (sediIncluse.Any(y => y == x.COD_SEDE || y == x.COD_SEDE.Substring(0, 2) + "*" || y == x.COD_SEDE.Substring(0, 2) + x.COD_SERVIZIO)) &&
                        (!sediEscluse.Any(y => y == x.COD_SEDE || y == x.COD_SEDE.Substring(0, 2) + "*" || y == x.COD_SEDE.Substring(0, 2) + x.COD_SERVIZIO))
                        ));
            }


            //if (AbilDir.GetHasFilter(total))
            //{
            //    var incluse = AbilDir.GetFilter(total).GetIncluse(livelli);
            //    var escluse = AbilDir.GetFilter(total).GetEscluse(livelli);

            //    if (incluse.Any() || !escluse.Any())
            //        //result = result.Where(x => incluse.Contains(x.COD_SERVIZIO));
            //        expr = LinqHelper.PutInAndTogether(expr, x => incluse.Contains(x.COD_SERVIZIO));

            //    if (escluse.Any())
            //        //result = result.Where(x => !escluse.Contains(x.COD_SERVIZIO));
            //        expr = LinqHelper.PutInAndTogether(expr, x => !escluse.Contains(x.COD_SERVIZIO));
            //}

            //if (AbilSedi.GetHasFilter(total))
            //{
            //    var incluse = AbilSedi.GetFilter(total).GetIncluse(livelli);
            //    var escluse = AbilSedi.GetFilter(total).GetEscluse(livelli);

            //    if (incluse.Any() || !escluse.Any())
            //        //result = result.Where(x => incluse.Any(y => y == x.COD_SEDE || y == x.COD_SEDE.Substring(0, 2) + "*" || y == x.COD_SEDE.Substring(0, 2) + x.COD_SERVIZIO));
            //        expr = LinqHelper.PutInAndTogether(expr, x => incluse.Any(y => y == x.COD_SEDE || y == x.COD_SEDE.Substring(0, 2) + "*" || y == x.COD_SEDE.Substring(0, 2) + x.COD_SERVIZIO));

            //    if (escluse.Any())
            //        //result = result.Where(x => !escluse.Any(y => y == x.COD_SEDE || y == x.COD_SEDE.Substring(0, 2) + "*" || y == x.COD_SEDE.Substring(0, 2) + x.COD_SERVIZIO));
            //        expr = LinqHelper.PutInAndTogether(expr, x => !escluse.Any(y => y == x.COD_SEDE || y == x.COD_SEDE.Substring(0, 2) + "*" || y == x.COD_SEDE.Substring(0, 2) + x.COD_SERVIZIO));
            //}

            if (AbilSocieta.GetHasFilter(total))
            {
                var incluse = AbilSocieta.GetFilter(total).GetIncluse(livelli);
                var escluse = AbilSocieta.GetFilter(total).GetEscluse(livelli);

                if (incluse.Any() || !escluse.Any())
                    //result = result.Where(x => incluse.Contains(x.COD_IMPRESACR));
                    expr = LinqHelper.PutInAndTogether(expr, x => incluse.Contains(x.COD_IMPRESACR));

                if (escluse.Any())
                    //result = result.Where(x => !escluse.Contains(x.COD_IMPRESACR));
                    expr = LinqHelper.PutInAndTogether(expr, x => !escluse.Contains(x.COD_IMPRESACR));
            }

            if (AbilMatr.GetHasFilter(total))
            {
                var incluse = AbilMatr.GetFilter(total).GetIncluse(livelli);
                var escluse = AbilMatr.GetFilter(total).GetEscluse(livelli);

                if (incluse.Any() || !escluse.Any())
                    //result = result.Where(x => incluse.Contains(x.COD_MATLIBROMAT));
                    expr = LinqHelper.PutInAndTogether(expr, x => incluse.Contains(x.COD_MATLIBROMAT));

                if (escluse.Any())
                    //result = result.Where(x => !escluse.Contains(x.COD_MATLIBROMAT));
                    expr = LinqHelper.PutInAndTogether(expr, x => !escluse.Contains(x.COD_MATLIBROMAT));
            }

            //Tipologie non supportate
            //Contratti non supportati

            return expr;
        }
    }


    public class AbilCat : BaseAbilFilter
    {
        public AbilCat() : base()
        {
        }
        public List<string> CategorieIncluse
        {
            get { return Incluse(); }
        }
        public List<string> CategorieEscluse
        {
            get { return Escluse(); }
        }
    }

    public class AbilDir : BaseAbilFilter
    {
        public AbilDir() : base()
        {
        }
        public List<string> DirezioniIncluse
        {
            get { return Incluse(); }
        }
        public List<string> DirezioniEscluse
        {
            get { return Escluse(); }
        }
    }

    public class AbilSedi : BaseAbilFilter
    {
        public AbilSedi() : base()
        {
        }
        public List<string> SediIncluse
        {
            get { return Incluse(); }
        }
        public List<string> SediEscluse
        {
            get { return Escluse(); }
        }
    }

    public class AbilSocieta : BaseAbilFilter
    {
        public AbilSocieta() : base()
        {
            SocietaSezIncluse = new List<string>();
            SocietaSezEscluse = new List<string>();
        }
        public List<string> SocietaIncluse
        {
            get { return Incluse(); }
        }
        public List<string> SocietaEscluse
        {
            get { return Escluse(); }
        }

        public List<string> SocietaSezIncluse { get; set; }
        public List<string> SocietaSezEscluse { get; set; }
    }

    public class AbilMatr : BaseAbilFilter
    {
        public AbilMatr() : base()
        {
        }
        public List<string> MatricoleIncluse
        {
            get { return Incluse(); }
        }
        public List<string> MatricoleEscluse
        {
            get { return Escluse(); }
        }
    }
    public class AbilTip : BaseAbilFilter
    {
        public AbilTip() : base()
        {

        }
        public List<string> TipologieIncluse
        {
            get { return Incluse(); }
        }
        public List<string> TipologieEscluse
        {
            get { return Escluse(); }
        }
    }
    public class AbilContr : BaseAbilFilter
    {
        public AbilContr() : base()
        {

        }
        public List<string> TipologieIncluse
        {
            get { return Incluse(); }
        }
        public List<string> TipologieEscluse
        {
            get { return Escluse(); }
        }
    }

    public class AbilMatrLiv : BaseAbilFilter
    {
        public enum VisibilitaEnum
        {
            Totale,
            Filtrata,
            HRGA
        }
        public AbilMatrLiv()
        {
            Visibilita = VisibilitaEnum.Totale;
        }
        public string Matricola { get; set; }
        public bool LivelloAnagrafico { get; set; }
        public bool LivelloGestionale { get; set; }
        public bool LivelloRetributivo { get; set; }
        public VisibilitaEnum Visibilita { get; set; }
    }

    public class AbilDb
    {
        public AbilDb()
        {

        }
        public XR_HRIS_ABIL Abil { get; set; }
        public string Funzione { get; set; }
        public string Sottofunzione { get; set; }
        public string Profilo { get; set; }
        public bool Create { get; set; }
        public bool Read { get; set; }
        public bool Update { get; set; }
        public bool Delete { get; set; }

        private XR_HRIS_ABIL_SUBFUNZIONE _dbSubFunc;

        public XR_HRIS_ABIL_SUBFUNZIONE DbSubFunc
        {
            get { return _dbSubFunc; }
            set
            {
                _dbSubFunc = value;
                Create = _dbSubFunc.IND_CREATE;
                Read = _dbSubFunc.IND_READ;
                Update = _dbSubFunc.IND_UPDATE;
                Delete = _dbSubFunc.IND_DELETE;
            }
        }

        public override string ToString()
        {
            return String.Format("Funzione {0} - Sottofunzione {1} - Profilo {2}", Funzione, Sottofunzione, Profilo);
        }
    }

    public class AbilDBFilter
    {
        public enum EnumPriorita
        {
            Abil = 0,
            ModelloAbil = 1,
            ModelloPers = 2
        }
        public AbilDBFilter()
        {
            _DirIncluse = new List<string>();
            _DirEscluse = new List<string>();
            _CatIncluse = new List<string>();
            _CatEscluse = new List<string>();
            _SediIncluse = new List<string>();
            _SediEscluse = new List<string>();
            _TipIncluse = new List<string>();
            _TipEscluse = new List<string>();
            _SocIncluse = new List<string>();
            _SocEscluse = new List<string>();
            _MatrIncluse = new List<string>();
            _MatrEscluse = new List<string>();
            _ContrInclusi = new List<string>();
            _ContrEsclusi = new List<string>();
        }
        public static AbilDBFilter FromAbil(XR_HRIS_ABIL abil)
        {
            AbilDBFilter filter = abil.CopyProperty<XR_HRIS_ABIL, AbilDBFilter>();
            filter.Priorita = EnumPriorita.Abil;
            return filter;
        }
        public static AbilDBFilter FromModelAbil(XR_HRIS_ABIL_MODELLO modello)
        {
            AbilDBFilter filter = modello.CopyProperty<XR_HRIS_ABIL_MODELLO, AbilDBFilter>();
            filter.Priorita = EnumPriorita.ModelloAbil;
            return filter;
        }
        public static AbilDBFilter FromModelPers(XR_HRIS_ABIL_MODELLO modello)
        {
            AbilDBFilter filter = modello.CopyProperty<XR_HRIS_ABIL_MODELLO, AbilDBFilter>();
            filter.Priorita = EnumPriorita.ModelloPers;
            return filter;
        }
        public EnumPriorita Priorita { get; set; }
        public string GR_CATEGORIE { get; set; }
        public string GR_AREA { get; set; }
        public string DIR_INCLUSE { get; set; }
        public string DIR_ESCLUSE { get; set; }
        public string CAT_INCLUSE { get; set; }
        public string CAT_ESCLUSE { get; set; }
        public string SEDI_INCLUSE { get; set; }
        public string SEDI_ESCLUSE { get; set; }
        public string TIP_INCLUSE { get; set; }
        public string TIP_ESCLUSE { get; set; }
        public string SOC_INCLUSE { get; set; }
        public string SOC_ESCLUSE { get; set; }
        public string MATR_INCLUSE { get; set; }
        public string MATR_ESCLUSE { get; set; }
        public string CONTR_INCLUSI { get; set; }
        public string CONTR_ESCLUSI { get; set; }

        private List<string> _DirIncluse { get; set; }
        private List<string> _DirEscluse { get; set; }
        private List<string> _CatIncluse { get; set; }
        private List<string> _CatEscluse { get; set; }
        private List<string> _SediIncluse { get; set; }
        private List<string> _SediEscluse { get; set; }
        private List<string> _TipIncluse { get; set; }
        private List<string> _TipEscluse { get; set; }
        private List<string> _SocIncluse { get; set; }
        private List<string> _SocEscluse { get; set; }
        private List<string> _MatrIncluse { get; set; }
        private List<string> _MatrEscluse { get; set; }
        private List<string> _ContrInclusi { get; set; }
        private List<string> _ContrEsclusi { get; set; }


        public bool IsEmpty()
        {
            return String.IsNullOrWhiteSpace(GR_CATEGORIE)
            && String.IsNullOrWhiteSpace(GR_AREA)
            && String.IsNullOrWhiteSpace(DIR_INCLUSE)
            && String.IsNullOrWhiteSpace(DIR_ESCLUSE)
            && String.IsNullOrWhiteSpace(CAT_INCLUSE)
            && String.IsNullOrWhiteSpace(CAT_ESCLUSE)
            && String.IsNullOrWhiteSpace(SEDI_INCLUSE)
            && String.IsNullOrWhiteSpace(SEDI_ESCLUSE)
            && String.IsNullOrWhiteSpace(TIP_INCLUSE)
            && String.IsNullOrWhiteSpace(TIP_ESCLUSE)
            && String.IsNullOrWhiteSpace(SOC_INCLUSE)
            && String.IsNullOrWhiteSpace(SOC_ESCLUSE)
            && String.IsNullOrWhiteSpace(MATR_INCLUSE)
            && String.IsNullOrWhiteSpace(MATR_ESCLUSE)
            && String.IsNullOrWhiteSpace(CONTR_INCLUSI)
            && String.IsNullOrWhiteSpace(CONTR_ESCLUSI);
        }

        public void NormalizeGroup()
        {
            if (!String.IsNullOrWhiteSpace(GR_CATEGORIE))
            {
                var idQualFilters = GR_CATEGORIE.Split(',').Select(x => Convert.ToInt32(x));
                IncentiviEntities db = new IncentiviEntities();
                Dictionary<int, XR_HRIS_QUAL_FILTER> dictQualFilter = db.XR_HRIS_QUAL_FILTER.Where(x => idQualFilters.Contains(x.ID_QUAL_FILTER)).ToDictionary(x => x.ID_QUAL_FILTER);
                foreach (int qualFilter in idQualFilters)
                {
                    if ((dictQualFilter[qualFilter].QUAL_INCLUDED ?? "*") != "*")
                        CAT_INCLUSE = CAT_INCLUSE + (!String.IsNullOrWhiteSpace(CAT_INCLUSE) ? "," : "") + dictQualFilter[qualFilter].QUAL_INCLUDED;

                    if ((dictQualFilter[qualFilter].QUAL_EXCLUDED ?? "*") != "*")
                        CAT_ESCLUSE = CAT_ESCLUSE + (!String.IsNullOrWhiteSpace(CAT_ESCLUSE) ? "," : "") + dictQualFilter[qualFilter].QUAL_EXCLUDED;
                }
            }

            if (!String.IsNullOrWhiteSpace(GR_AREA))
            {
                var idAreaFilter = GR_AREA.Split(',').Select(x => Convert.ToInt32(x));
                IncentiviEntities db = new IncentiviEntities();
                Dictionary<int, XR_HRIS_DIR_FILTER> dictDirFilter = db.XR_HRIS_DIR_FILTER.Where(x => idAreaFilter.Contains(x.ID_AREA_FILTER)).ToDictionary(x => x.ID_AREA_FILTER);
                foreach (var dirFilter in idAreaFilter)
                {
                    if ((dictDirFilter[dirFilter].DIR_INCLUDED ?? "*") != "*")
                        DIR_INCLUSE = DIR_INCLUSE + (!String.IsNullOrWhiteSpace(DIR_INCLUSE) ? "," : "") + dictDirFilter[dirFilter].DIR_INCLUDED;

                    if ((dictDirFilter[dirFilter].DIR_EXCLUDED ?? "*") != "*")
                        DIR_ESCLUSE = DIR_ESCLUSE + (!String.IsNullOrWhiteSpace(DIR_ESCLUSE) ? "," : "") + dictDirFilter[dirFilter].DIR_EXCLUDED;
                }
            }
        }
        public void LoadAry()
        {
            //Direzioni
            if (!String.IsNullOrWhiteSpace(DIR_INCLUSE))
                _DirIncluse.AddRange(DIR_INCLUSE.Split(','));
            if (!String.IsNullOrWhiteSpace(DIR_ESCLUSE))
                _DirEscluse.AddRange(DIR_ESCLUSE.Split(',').Where(x => !_DirIncluse.Contains(x)).ToArray());

            //Categorie
            if (!String.IsNullOrWhiteSpace(CAT_INCLUSE))
                _CatIncluse.AddRange(CAT_INCLUSE.Split(','));
            if (!String.IsNullOrWhiteSpace(CAT_ESCLUSE))
                _CatEscluse.AddRange(CAT_ESCLUSE.Split(',').Where(x => !_CatIncluse.Contains(x)).ToArray());

            //Sedi
            if (!String.IsNullOrWhiteSpace(SEDI_INCLUSE))
                _SediIncluse.AddRange(SEDI_INCLUSE.Split(','));
            if (!String.IsNullOrWhiteSpace(SEDI_ESCLUSE))
                _SediEscluse.AddRange(SEDI_ESCLUSE.Split(',').Where(x => !_SediIncluse.Contains(x)).ToArray());

            //Tipologie
            if (!String.IsNullOrWhiteSpace(TIP_INCLUSE))
                _TipIncluse.AddRange(TIP_INCLUSE.Split(','));
            if (!String.IsNullOrWhiteSpace(TIP_ESCLUSE))
                _TipEscluse.AddRange(TIP_ESCLUSE.Split(',').Where(x => !_TipIncluse.Contains(x)).ToArray());

            //Societa
            if (!String.IsNullOrWhiteSpace(SOC_INCLUSE))
                _SocIncluse.AddRange(SOC_INCLUSE.Split(','));
            if (!String.IsNullOrWhiteSpace(SOC_ESCLUSE))
                _SocEscluse.AddRange(SOC_ESCLUSE.Split(',').Where(x => !_SocIncluse.Contains(x)).ToArray());

            //Matricole
            if (!String.IsNullOrWhiteSpace(MATR_INCLUSE))
                _MatrIncluse.AddRange(MATR_INCLUSE.Split(','));
            if (!String.IsNullOrWhiteSpace(MATR_ESCLUSE))
                _MatrEscluse.AddRange(MATR_ESCLUSE.Split(',').Where(x => !_MatrIncluse.Contains(x)).ToArray());

            //TipiContratti
            if (!String.IsNullOrWhiteSpace(CONTR_INCLUSI))
                _ContrInclusi.AddRange(CONTR_INCLUSI.Split(','));
            if (!String.IsNullOrWhiteSpace(CONTR_ESCLUSI))
                _ContrEsclusi.AddRange(CONTR_ESCLUSI.Split(',').Where(x => !_ContrInclusi.Contains(x)).ToArray());

        }

        public Expression<Func<T, bool>> GetSintesiFilter<T>() where T : class, myRai.Data.Interface.ISintesi1
        {
            Expression<Func<T, bool>> expr = null;

            NormalizeGroup();
            LoadAry();

            if (_CatIncluse.Any())
                expr = LinqHelper.PutInAndTogether(expr, x => _CatIncluse.Any(y => x.COD_QUALIFICA.StartsWith(y)));
            if (_CatEscluse.Any())
                expr = LinqHelper.PutInAndTogether(expr, x => !_CatEscluse.Any(y => x.COD_QUALIFICA.StartsWith(y)));

            if (_DirIncluse.Any())
                expr = LinqHelper.PutInAndTogether(expr, x => _DirIncluse.Contains(x.COD_SERVIZIO));
            if (_DirEscluse.Any())
                expr = LinqHelper.PutInAndTogether(expr, x => !_DirEscluse.Contains(x.COD_SERVIZIO));

            if (_SediIncluse.Any())
                expr = LinqHelper.PutInAndTogether(expr, x => _SediIncluse.Any(y => y == x.COD_SEDE || y == x.COD_SEDE.Substring(0, 2) + "*" || y == x.COD_SEDE.Substring(0, 2) + x.COD_SERVIZIO));
            if (_SediEscluse.Any())
                expr = LinqHelper.PutInAndTogether(expr, x => !_SediEscluse.Any(y => y == x.COD_SEDE || y == x.COD_SEDE.Substring(0, 2) + "*" || y == x.COD_SEDE.Substring(0, 2) + x.COD_SERVIZIO));

            if (_SocIncluse.Any())
                expr = LinqHelper.PutInAndTogether(expr, x => _SocIncluse.Contains(x.COD_IMPRESACR));
            if (_SocEscluse.Any())
                expr = LinqHelper.PutInAndTogether(expr, x => !_SocEscluse.Contains(x.COD_IMPRESACR));

            if (_MatrIncluse.Any())
                expr = LinqHelper.PutInAndTogether(expr, x => _MatrIncluse.Contains(x.COD_MATLIBROMAT));
            if (_MatrEscluse.Any())
                expr = LinqHelper.PutInAndTogether(expr, x => !_MatrEscluse.Contains(x.COD_MATLIBROMAT));

            if (_ContrInclusi.Any())
                expr = LinqHelper.PutInAndTogether(expr, x => x.COD_TPCNTR==null || _ContrInclusi.Contains(x.COD_TPCNTR));
            if (_ContrEsclusi.Any())
                expr = LinqHelper.PutInAndTogether(expr, x => x.COD_TPCNTR!=null && !_ContrEsclusi.Contains(x.COD_TPCNTR));

            //Le tipologie non possono essere applicate sulla SINTESI1

            if (expr == null)
                return x => true;
            else
                return expr;
        }
    }
}

