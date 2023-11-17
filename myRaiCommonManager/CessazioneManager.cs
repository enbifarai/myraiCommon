using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using myRai.DataAccess;
using myRaiCommonManager;
using myRaiCommonModel;
using myRaiCommonModel.cvModels.Pdf;
using myRaiCommonModel.Gestionale;
using myRaiCommonTasks.Helpers;
using myRaiData;
using myRaiData.Incentivi;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Objects;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Xceed.Words.NET;

namespace myRaiCommonManager.Cessazione
{
    public class TempModulo
    {
        public myRaiDataTalentia.XR_MOD_DIPENDENTI Modulo { get; set; }
        public DateTime DataRichiesta { get; set; }
        public DateTime DataNascita { get; set; }
    }
    public class CessazioneManager
    {
        public static List<CessazioneModel> GetElenco(CessazioneFilter filtri, bool completeData, string incExtra, Expression<Func<XR_INC_DIPENDENTI, bool>> extraFilter = null, bool loadHistorySolleciti = false, bool loadField = true)
        {
            string matricola = CommonHelper.GetCurrentUserMatricola();
            List<CessazioneModel> result = new List<CessazioneModel>();
            filtri = filtri ?? new CessazioneFilter();

            IncentiviEntities db = new IncentiviEntities();
            IQueryable<XR_INC_DIPENDENTI> tmp = null;
            Expression<Func<XR_INC_DIPENDENTI, bool>> abilFilters = null;
            var listParam = HrisHelper.GetParametriJson<XR_HRIS_PARAM>(HrisParam.IncentiviParametri);
            string codGruppo = HrisHelper.GetParametro<string>(HrisParam.IncentiviRifGruppo);

            if (String.IsNullOrWhiteSpace(incExtra) || incExtra == "__SOLLECITI__" || incExtra == "AMM_CONT")
            {
                Expression<Func<XR_INC_DIPENDENTI, bool>> funcFilterAbil = CessazioneHelper.FuncFilterAbil(!filtri.HasFilter, db, incExtra == "AMM_CONT", incExtra == "AMM_CONT" ? incExtra : "");
                Expression<Func<XR_INC_DIPENDENTI, bool>> funcFilterMatr = CessazioneHelper.FuncFilterMatr(db);
                Expression<Func<XR_INC_DIPENDENTI, bool>> funcFilterAbilSede = CessazioneHelper.FuncFilterAbilSede();
                Expression<Func<XR_INC_DIPENDENTI, bool>> funcFilterAdditional = CessazioneHelper.FuncFilterAdditional(db);
                Expression<Func<XR_INC_DIPENDENTI, bool>> funcFilterVerbali = CessazioneHelper.FuncFilterVerbali(db, CommonHelper.GetCurrentUserMatricola(), !filtri.HasFilter);

                abilFilters = LinqHelper.PutInAndTogether(funcFilterAbil, funcFilterMatr, funcFilterAbilSede, funcFilterAdditional);
                if (funcFilterVerbali != null)
                    abilFilters = LinqHelper.PutInOrTogether(abilFilters, funcFilterVerbali);

                tmp = db.XR_INC_DIPENDENTI.AsQueryable();
            }
            else
            {
                var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.Where(x => x.XR_INC_DIPENDENTI.Any()), matricola, null, CessazioneHelper.INCENTIVI_INC_EXTRA, incExtra);
                tmp = tmpSint.SelectMany(x => x.XR_INC_DIPENDENTI);

                abilFilters = CessazioneHelper.FuncFilterAbil(!filtri.HasFilter, db, true, incExtra);

                //Il filtro per tipologia non può essere applicato nella SintesiFilter
                var enabledTip = AuthHelper.EnabledTip(matricola, CessazioneHelper.INCENTIVI_INC_EXTRA, incExtra);
                if (enabledTip.HasFilter)
                {
                    var incluse = enabledTip.Incluse().Select(x => Convert.ToInt32(x));
                    var escluse = enabledTip.Escluse().Select(x => Convert.ToInt32(x));

                    if (incluse.Any() || !escluse.Any())
                        tmp = tmp.Where(x => incluse.Contains(x.ID_TIPOLOGIA));

                    if (escluse.Any())
                        tmp = tmp.Where(x => !escluse.Contains(x.ID_TIPOLOGIA));
                }

                if (incExtra == "TESSCONTR")
                    tmp = tmp.Where(x => x.DATA_RICH_TESS_CONTR != null);
                else if (incExtra == "DICHMAT")
                    tmp = tmp.Where(x => x.DTA_RICH_MATCON != null);

                if (incExtra == "TESSCONTR" && !filtri.HasFilter)
                    tmp = tmp.Where(x => x.COD_GRUPPO == codGruppo);

            }

            if (incExtra == "__SOLLECITI__")
            {
                var paramSollecito = listParam.FirstOrDefault(x => x.COD_PARAM == "LimiteSollecitoEstratti");
                var dataSollecito = Convert.ToInt32(paramSollecito.COD_VALUE1);
                var param = listParam.FirstOrDefault(x => x.COD_PARAM == "LimiteConsegnaEstratti");
                var dataScadenza = Convert.ToInt32(param.COD_VALUE1);

                if (filtri == null) filtri = new CessazioneFilter();
                filtri.HasFilter = true;
                filtri.CodiceGruppo = codGruppo;
                filtri.Stato = Convert.ToString((int)IncStato.RichiestaInserita);
                filtri.Tipologia = Convert.ToString((int)CessazioneTipo.Incentivazione);

                Expression<Func<XR_INC_DIPENDENTI, bool>> filter = x => x.DATA_ARRIVO_DOC == null
                                                            && (EntityFunctions.TruncateTime(EntityFunctions.AddDays(x.DTA_RICHIESTA, dataSollecito)) == DateTime.Today
                                                                || EntityFunctions.TruncateTime(EntityFunctions.AddDays(x.DTA_RICHIESTA, dataScadenza)) < DateTime.Today);

                if (loadHistorySolleciti)
                    filter = LinqHelper.PutInOrTogether(filter, x => db.XR_INC_PARAM_MAIL.Any(y => y.ID_DIPENDENTE == x.ID_DIPENDENTE && y.COD_CAMPO == "RichiestaInseritaSollecito"));

                tmp = tmp.Where(filter);
            }
            else if (incExtra == "AMM_CONT")
                tmp = tmp.Where(x => x.COD_GRUPPO == codGruppo && x.ID_TIPOLOGIA == (int)CessazioneTipo.Incentivazione);

            tmp = ApplicaFiltri(db, tmp, filtri);

            //Se è incentivazione bisogna nascondere le pratiche senza progressivo
            //tmp = tmp.Where(x => x.ID_TIPOLOGIA != (int)CessazioneTipo.Incentivazione || x.COD_GRUPPO == "INCENTIVAZIONE2018" || x.PROGR_RICHIESTA.HasValue);

            if (extraFilter != null)
                tmp = tmp.Where(extraFilter);

            tmp = tmp.Where(abilFilters)
                        .OrderBy(x => x.PROGR_RICHIESTA)
                        .ThenBy(x => x.DATA_CESSAZIONE)
                        .ThenBy(x => x.SINTESI1.DES_COGNOMEPERS)
                        .ThenBy(y => y.SINTESI1.DES_NOMEPERS);


            IQueryable<CessazioneModel> tmp2 = null;

            tmp2 = tmp.Select(x => new CessazioneModel()
            {
                Pratica = completeData ? x : null,
                Stato = x.XR_INC_OPERSTATI.Where(y => !y.XR_INC_STATI.IND_SYSTEM.Value && !y.DATA_FINE_VALIDITA.HasValue)
                    .Select(w => w.XR_INC_STATI).OrderByDescending(z => z.XR_WKF_WORKFLOW.FirstOrDefault(a => a.ID_TIPOLOGIA == x.ID_TIPOLOGIA).ORDINE)
                    .FirstOrDefault(),
                InCarico = x.XR_INC_OPERSTATI.Where(y => !y.DATA_FINE_VALIDITA.HasValue && y.ID_STATO == 0).Select(y => y.SINTESI1).FirstOrDefault(),
                Ordine = x.XR_INC_OPERSTATI.Where(y => !y.XR_INC_STATI.IND_SYSTEM.Value && !y.DATA_FINE_VALIDITA.HasValue)
                    .Select(w => w.XR_INC_STATI).OrderByDescending(z => z.XR_WKF_WORKFLOW.FirstOrDefault(a => a.ID_TIPOLOGIA == x.ID_TIPOLOGIA).ORDINE)
                    .FirstOrDefault().XR_WKF_WORKFLOW.FirstOrDefault(z => z.ID_TIPOLOGIA == x.ID_TIPOLOGIA),
                NumNote = completeData ? x.XR_INC_DIPENDENTI_NOTE.Count() : 0,
                Tipologia = x.XR_WKF_TIPOLOGIA,
                Sintesi = completeData ? x.SINTESI1 : null,
                OpenFunction = incExtra,
                IsAnpal = incExtra == "ANPAL",
                Fields = x.XR_INC_DIPENDENTI_FIELD.Where(y => y.COD_FIELD.StartsWith("Limite"))
            });

            result = tmp2.ToList();

            //if (completeData)
            //    foreach (var item in result.Where(x => x.HasFields))
            //        item.Fields = item.Pratica.XR_INC_DIPENDENTI_FIELD.Where(x=>x.COD_FIELD!= "AccettazioneBase64").ToList();


            if (completeData && (String.IsNullOrWhiteSpace(incExtra) || incExtra == "__SOLLECITI__"))
                result.ForEach(x => x.Scadenze.Load(listParam, x.Pratica, x.Stato, x.Fields.ToList()));

            return result;
        }
        public static IQueryable<XR_INC_DIPENDENTI> ApplicaFiltri(IncentiviEntities db, IQueryable<XR_INC_DIPENDENTI> tmp, CessazioneFilter filtri = null)
        {
            if (filtri.HasFilter)
            {
                if (!String.IsNullOrWhiteSpace(filtri.Tipologia))
                {
                    int tipo = Convert.ToInt32(filtri.Tipologia);
                    tmp = tmp.Where(x => x.ID_TIPOLOGIA == tipo);
                }

                if (!String.IsNullOrWhiteSpace(filtri.Stato))
                {
                    int intStato = Convert.ToInt32(filtri.Stato);
                    var isCurrentState = CessazioneHelper.IsCurrentState(intStato);
                    tmp = tmp.Where(isCurrentState);
                }

                if (!String.IsNullOrWhiteSpace(filtri.InCarico))
                {
                    if (filtri.InCarico == "0")
                        tmp = tmp.Where(x => !x.XR_INC_OPERSTATI.Any(z => z.ID_STATO == (int)IncStato.InCarico));
                    else if (filtri.InCarico == "1")
                        tmp = tmp.Where(x => x.XR_INC_OPERSTATI.Any(z => z.ID_STATO == (int)IncStato.InCarico));
                }

                if (!String.IsNullOrWhiteSpace(filtri.Matricola))
                    tmp = tmp.Where(x => filtri.Matricola.Contains(x.MATRICOLA));

                if (!String.IsNullOrWhiteSpace(filtri.Nominativo))
                    tmp = tmp.Where(x => (x.SINTESI1.DES_COGNOMEPERS + " " + x.SINTESI1.DES_NOMEPERS).ToUpper().Contains(filtri.Nominativo.ToUpper())
                                        || (x.SINTESI1.DES_NOMEPERS + " " + x.SINTESI1.DES_COGNOMEPERS).ToUpper().Contains(filtri.Nominativo.ToUpper()));

                if (!String.IsNullOrWhiteSpace(filtri.DataCessazione))
                {
                    DateTime dateEnd = filtri.DataCessazione.ToDateTime("yyyyMMdd");
                    tmp = tmp.Where(x => x.DATA_CESSAZIONE.Value == dateEnd);
                }

                if (!String.IsNullOrWhiteSpace(filtri.Sede))
                    tmp = tmp.Where(x => x.SEDE == filtri.Sede);

                if (!String.IsNullOrWhiteSpace(filtri.Causa))
                {
                    if (filtri.Causa == "0")
                        tmp = tmp.Where(x => x.CAUSE_VERTENZE == null || x.CAUSE_VERTENZE == "");
                    else if (filtri.Causa == "1")
                        tmp = tmp.Where(x => x.CAUSE_VERTENZE != null && x.CAUSE_VERTENZE != "");
                }

                if (!String.IsNullOrWhiteSpace(filtri.CodiceGruppo))
                    tmp = tmp.Where(x => x.COD_GRUPPO == filtri.CodiceGruppo);

                if (!String.IsNullOrWhiteSpace(filtri.DataRichiesta))
                {
                    DateTime dateEnd = filtri.DataRichiesta.ToDateTime("dd/MM/yyyy");
                    tmp = tmp.Where(x => x.DTA_RICHIESTA != null && EntityFunctions.TruncateTime(x.DTA_RICHIESTA.Value) == dateEnd);
                }

                if (filtri.QualFilter != null && filtri.QualFilter.Any())
                {
                    int[] qualFilter = filtri.QualFilter.Select(x => Convert.ToInt32(x)).ToArray();
                    var dbQualFilter = db.XR_INC_ADD_FILTER.Where(x => qualFilter.Contains(x.ID_FILTER));
                    var incl = String.Join(",", dbQualFilter.Select(x => x.LIST_CAT_INCL)).Split(',').Where(x => !String.IsNullOrWhiteSpace(x));
                    var escl = String.Join(",", dbQualFilter.Select(x => x.LIST_CAT_ESCL)).Split(',').Where(x => !String.IsNullOrWhiteSpace(x) && !incl.Contains(x));

                    if (incl.Any())
                        tmp = tmp.Where(x => incl.Any(y => x.SINTESI1.COD_QUALIFICA.StartsWith(y)));
                    if (escl.Any())
                        tmp = tmp.Where(x => !escl.Any(y => x.SINTESI1.COD_QUALIFICA.StartsWith(y)));
                }

                if (filtri.TipoVertenza != null && filtri.TipoVertenza.Any())
                    tmp = tmp.Where(x => x.IND_TIPO_VERTENZE != null && filtri.TipoVertenza.Any(y => x.IND_TIPO_VERTENZE.Contains(y)));
            }

            return tmp;
        }

        public static List<SelectListItem> QualFilter()
        {
            var db = new IncentiviEntities();
            var list = db.XR_INC_ADD_FILTER.Where(x => x.LIST_MATRICOLE == "QualFilter");
            List<SelectListItem> result = list.Select(x => new { x.ID_FILTER, x.COD_FILTER }).ToList().Select(x => new SelectListItem() { Value = x.ID_FILTER.ToString(), Text = x.COD_FILTER }).ToList();
            return result;
        }
        public static List<SelectListItem> GetDatePagamenti(DateTime? filter)
        {
            var db = new IncentiviEntities();

            List<SelectListItem> result = new List<SelectListItem>();
            result.Add(new SelectListItem() { Value = "", Text = "Nessuna data selezionata" });
            if (filter.HasValue)
            {
                var list = db.XR_INC_DATE_PAGAMENTO.Where(x => x.DTA_CESSAZIONE == filter && x.DTA_PAGAMENTO != null).Select(x => x.DTA_PAGAMENTO).ToList();
                result.AddRange(list.Select(x => new SelectListItem() { Value = x.Value.ToString(), Text = x.Value.ToString("dd/MM/yyyy") }));
            }
            return result;
        }
        public static void RendiAllegatiEffettivi(int idDip, IncentiviEntities db, int? statoPartenza, int statoDest, string tag = null)
        {
            if (statoPartenza.HasValue)
            {
                var allegati = db.XR_INC_OPERSTATI_DOC
                    .Where(x => x.XR_INC_OPERSTATI.ID_DIPENDENTE == idDip && x.XR_INC_OPERSTATI.ID_STATO == statoPartenza)
                    .Select(x => x.ID_ALLEGATO);
                if (allegati != null && allegati.Any())
                {
                    int idStato = CessazioneHelper.SalvaStato(db, idDip, statoDest, CommonHelper.GetCurrentIdPersona(), false);

                    foreach (var item in allegati)
                    {
                        var all = db.XR_INC_OPERSTATI_DOC.FirstOrDefault(x => x.ID_ALLEGATO == item);
                        all.ID_OPER = idStato;
                    }

                    var operTemp = db.XR_INC_OPERSTATI.FirstOrDefault(x => x.ID_DIPENDENTE == idDip && x.ID_STATO == statoPartenza);
                    if (operTemp != null)
                        db.XR_INC_OPERSTATI.Remove(operTemp);
                }
            }
        }

        public static void RendiAllegatiDipEffettivi(int idDip, string matricola, IncentiviEntities db, int statoTemplate, int statoDest, string tag = null)
        {
            myRaiCommonTasks.Helpers.FileResult files = null;
            var list = InternalGetAllegatiDip(ref files, idDip, matricola, statoTemplate, statoTemplate, tag, true);

            if (list.Any())
            {
                CezanneHelper.GetCampiFirma(out string user, out string termid, out DateTime tms);
                var dbDg = new digiGappEntities();
                foreach (var item in list)
                {
                    int _state = statoDest;
                    if (item.ElencoTag != null && item.ElencoTag.Contains("Proposta"))
                        _state = (int)IncStato.FileProposta;
                    else if (item.ElencoTag != null && item.ElencoTag.Contains("Accettazione"))
                        _state = (int)IncStato.FileAccettazione;

                    int idStato = CessazioneHelper.SalvaStato(db, idDip, _state, CommonHelper.GetCurrentIdPersona(), false);
                    item.IND_RILEVANTE = item.FromTemplate;
                    item.ID_ALLEGATO = db.XR_INC_OPERSTATI_DOC.GeneraPrimaryKey();
                    item.COD_USER = user;
                    item.ID_OPER = idStato;
                    item.COD_TERMID = "RAIPERME";
                    item.NOT_TAG = "Rai per Me" + (!String.IsNullOrWhiteSpace(item.NOT_TAG) ? ";" + item.NOT_TAG : "");
                    item.VALID_DTA_INI = tms;
                    item.VALID_DTA_END = null;
                    db.XR_INC_OPERSTATI_DOC.Add(item);


                    var dbFile = dbDg.MyRai_Files.Find(item.FileId);
                    IncentivazioneFile info = Newtonsoft.Json.JsonConvert.DeserializeObject<IncentivazioneFile>(dbFile.Json);
                    info.Approvato = true;
                    info.MatricolaApprv = CommonHelper.GetCurrentUserMatricola();
                    dbFile.Json = Newtonsoft.Json.JsonConvert.SerializeObject(info);
                }

                DBHelper.Save(dbDg, CommonHelper.GetCurrentUserMatricola());
            }
        }

        public static List<XR_INC_OPERSTATI_DOC> InternalGetAllegatiDip(ref myRaiCommonTasks.Helpers.FileResult files, int idDip, string matricola, int idStatoRif, int stato, string tag = "", bool contentByte = false, bool ancheApprovati = false)
        {
            List<XR_INC_OPERSTATI_DOC> result = new List<XR_INC_OPERSTATI_DOC>();

            var db = new IncentiviEntities();
            if (files == null)
                files = FileManager.GetFiles(matricola, "INC", getContentByte: contentByte);

            var search = INCFileManager.GeneraChiaveRicerca(idDip, true, stato);
            foreach (var file in files.Files.Where(x => x.Chiave.Trim().StartsWith(search)))
            {
                var fileInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<IncentivazioneFile>(file.Json);
                if ((!fileInfo.Approvato.HasValue || ancheApprovati) && (String.IsNullOrWhiteSpace(tag) && String.IsNullOrWhiteSpace(fileInfo.Tag) || fileInfo.Tag == tag))
                {
                    var template = db.XR_INC_TEMPLATE.Find(fileInfo.Template.Value);
                    bool checkTipo = false;
                    if (template != null)
                    {
                        var dict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(template.TEMPLATE_TEXT);
                        string propName, intCheckName, intPropName = "";
                        object intDate = null;


                        if (dict.TryGetValue("CampoDataAggiornamento", out propName))
                        {
                            var pratica = db.XR_INC_DIPENDENTI.Find(fileInfo.IdDipendente);
                            //Controllo se è presente anche un campo di richiesta integrazioni
                            if (dict.TryGetValue("CampoRichiestaInt", out intCheckName))
                            {
                                checkTipo = true;
                                dict.TryGetValue("CampoDataAggiornamentoInt", out intPropName);
                                intDate = pratica.GetType().GetProperty(intCheckName).GetValue(pratica, null);

                                if (intDate != null)
                                    checkTipo = false;
                            }
                        }
                    }


                    result.Add(new XR_INC_OPERSTATI_DOC()
                    {
                        ID_ALLEGATO = -1,
                        COD_TITLE = fileInfo.Titolo,
                        CONTENT_TYPE = file.MimeType,
                        DES_ALLEGATO = fileInfo.Descrizione,
                        NMB_SIZE = file.Length,
                        NME_FILENAME = file.NomeFile,
                        COD_USER = file.MatricolaCreatore,
                        TMS_TIMESTAMP = file.DataCreazione,
                        ID_STATO_RIF = idStatoRif,
                        NOT_TAG = tag,
                        OBJ_OBJECT = file.ContentByte,
                        ElencoTag = new string[] { tag },
                        IsExternal = true,
                        ShowApproveBtn = true,
                        DisableDelete = true,
                        ShowTipoRifiuto = checkTipo,
                        FromTemplate = true,
                        ExternalAction = "/Cessazione/GetDipFile?key=" + file.Chiave.Trim(),
                        Chiave = file.Chiave.Trim(),
                        FileId = file.Id
                    });
                }
            }

            //if (String.IsNullOrWhiteSpace(tag))
            {
                search = INCFileManager.GeneraChiaveRicerca(idDip, false, stato);
                foreach (var file in files.Files.Where(x => x.Chiave.Trim().StartsWith(search)))
                {
                    var fileInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<IncentivazioneFile>(file.Json);
                    if ((!fileInfo.Approvato.HasValue || ancheApprovati) && (String.IsNullOrWhiteSpace(tag) && String.IsNullOrWhiteSpace(fileInfo.Tag) || fileInfo.Tag == tag))
                    {
                        result.Add(new XR_INC_OPERSTATI_DOC()
                        {
                            ID_ALLEGATO = -1,
                            COD_TITLE = fileInfo.Titolo,
                            CONTENT_TYPE = file.MimeType,
                            DES_ALLEGATO = fileInfo.Descrizione,
                            NMB_SIZE = file.Length,
                            NME_FILENAME = file.NomeFile,
                            COD_USER = file.MatricolaCreatore,
                            TMS_TIMESTAMP = file.DataCreazione,
                            ID_STATO_RIF = idStatoRif,
                            OBJ_OBJECT = file.ContentByte,
                            NOT_TAG = null,
                            ElencoTag = null,
                            IsExternal = true,
                            ShowApproveBtn = true,
                            DisableDelete = true,
                            FromTemplate = false,
                            ExternalAction = "/Cessazione/GetDipFile?key=" + file.Chiave.Trim(),
                            Chiave = file.Chiave.Trim(),
                            FileId = file.Id
                        });
                    }
                }
            }

            return result;
        }

        public static bool ImportFromModuli(int idMod, myRaiDataTalentia.XR_MOD_DIPENDENTI modulo, out string errorMsg)
        {
            bool result = false;
            errorMsg = "";

            List<TempModulo> list = new List<TempModulo>();

            string codGruppo = HrisHelper.GetParametro<string>(HrisParam.IncentiviRifGruppo);

            var db = new IncentiviEntities();

            var dbTal = new myRaiDataTalentia.TalentiaEntities();
            list = (from mod in dbTal.XR_MOD_DIPENDENTI
                    join sint in dbTal.SINTESI1 on mod.ID_PERSONA equals sint.ID_PERSONA
                    where mod.COD_MODULO == codGruppo && !mod.MATRICOLA.StartsWith("$") && !mod.MATRICOLA.EndsWith("$")
                    select new TempModulo
                    {
                        Modulo = mod,
                        DataRichiesta = mod.DATA_COMPILAZIONE.Value,
                        DataNascita = sint.DTA_NASCITAPERS.Value
                    }
                    ).ToList();

            bool anyTalChanges = false;

            int progressivo = 1;
            foreach (var item in list.OrderBy(x => x.DataRichiesta.Date).ThenBy(x => x.DataNascita))
            {
                var inc = db.XR_INC_DIPENDENTI.FirstOrDefault(x => x.ID_PERSONA == item.Modulo.ID_PERSONA && x.DTA_RICHIESTA == item.Modulo.DATA_COMPILAZIONE);
                if (inc == null)
                {
                    //CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime tmsTimestamp);
                    string codUser = "ADMIN";
                    string codTermid = "BATCHSESSION";
                    DateTime tmsTimestamp = DateTime.Now;

                    var sint = db.SINTESI1.Find(item.Modulo.ID_PERSONA);

                    string sede = CezanneHelper.GetDes(sint.COD_SEDE, sint.DES_SEDE);
                    if (sede.ToUpper().Contains("ROMA"))
                        sede = "ROMA";

                    string strDtaCess = item.Modulo.SCELTA.Substring(item.Modulo.SCELTA.IndexOf("#") + 1);
                    DateTime dtCess;
                    DateTime.TryParseExact(strDtaCess, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dtCess);

                    inc = new XR_INC_DIPENDENTI()
                    {
                        ID_DIPENDENTE = db.XR_INC_DIPENDENTI.GeneraPrimaryKey(),
                        ID_TIPOLOGIA = (int)CessazioneTipo.Incentivazione,
                        ID_PERSONA = item.Modulo.ID_PERSONA,
                        DATA_ANZIANITA = sint.DTA_ANZCONV.GetValueOrDefault(),
                        DATA_ASSUNZIONE = sint.DTA_INIZIO_CR.GetValueOrDefault(),
                        DATA_CESSAZIONE = dtCess,
                        DATA_USCITA_RICH = dtCess,
                        MATRICOLA = sint.COD_MATLIBROMAT,
                        DTA_RICHIESTA = item.Modulo.DATA_COMPILAZIONE,
                        SEDE = sede,
                        NOT_TIP_SCELTA = item.Modulo.SCELTA.Substring(0, item.Modulo.SCELTA.IndexOf("#")),
                        PROGR_RICHIESTA = progressivo,
                        COD_GRUPPO = item.Modulo.COD_MODULO,
                        COD_USER = codUser,
                        COD_TERMID = codTermid,
                        TMS_TIMESTAMP = tmsTimestamp
                    };

                    db.XR_INC_DIPENDENTI.Add(inc);

                    XR_INC_OPERSTATI statoDaAvviare = new XR_INC_OPERSTATI();
                    statoDaAvviare.ID_OPER = db.XR_INC_OPERSTATI.GeneraPrimaryKey();
                    statoDaAvviare.ID_DIPENDENTE = inc.ID_DIPENDENTE;
                    statoDaAvviare.ID_PERSONA = item.Modulo.ID_PERSONA;
                    statoDaAvviare.ID_STATO = (int)IncStato.RichiestaInserita;
                    statoDaAvviare.DATA = DateTime.Now;
                    statoDaAvviare.COD_USER = inc.COD_USER;
                    statoDaAvviare.COD_TERMID = inc.COD_TERMID;
                    statoDaAvviare.TMS_TIMESTAMP = inc.TMS_TIMESTAMP;
                    db.XR_INC_OPERSTATI.Add(statoDaAvviare);
                }
                else
                {
                    string strDtaCess = item.Modulo.SCELTA.Substring(item.Modulo.SCELTA.IndexOf("#") + 1);
                    DateTime dtCess;
                    DateTime.TryParseExact(strDtaCess, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out dtCess);

                    inc.DATA_USCITA_RICH = dtCess;
                    inc.DTA_RICHIESTA = item.Modulo.DATA_COMPILAZIONE;
                    inc.NOT_TIP_SCELTA = item.Modulo.SCELTA.Substring(0, item.Modulo.SCELTA.IndexOf("#"));
                    inc.PROGR_RICHIESTA = progressivo;
                    inc.COD_GRUPPO = item.Modulo.COD_MODULO;
                }

                if (item.Modulo.PROGRESSIVO != progressivo)
                {
                    anyTalChanges = true;
                    dbTal.XR_MOD_DIPENDENTI.Find(item.Modulo.XR_MOD_DIPENDENTI1).PROGRESSIVO = progressivo;
                }

                progressivo++;
            }

            try
            {
                db.SaveChanges();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                errorMsg = "Errore durante il salvataggio - " + ex.Message;
            }

            if (result)
            {
                if (anyTalChanges)
                {
                    try
                    {
                        dbTal.SaveChanges();
                        result = true;
                    }
                    catch (Exception ex)
                    {
                        result = false;
                        errorMsg = "Errore durante il salvataggio di Talentia - " + ex.Message;
                    }
                }
            }

            return result;
        }

        public static MemoryStream CreaPDFPagamenti(string imgLogo, string imgInt, string title, IQueryable<XR_INC_DIPENDENTI> dipendenti)
        {
            MemoryStream ms = null;

            PdfPrinter pdf = new PdfPrinter(imgLogo, imgInt, title);
            pdf.Apri(0, "Incentivi 2018");

            pdf.WriteLine(title, true);
            pdf.WriteLine(" ", false);
            pdf.WriteLine("Data ultimo aggiornamento: " + DateTime.Now.ToString("dd/MM/yyyy"), false);
            pdf.WriteLine("Numero pratiche: " + dipendenti.Count(), true);
            pdf.WriteLine("Totale: " + dipendenti.Select(x => x.IMPORTO_NETTO != null ? x.IMPORTO_NETTO : 0).Sum().Value.ToString("N") + " €", true);
            pdf.WriteLine(" ", false);

            if (dipendenti != null && dipendenti.Count() > 0)
            {
                string headerString = "Nominativo:Sede:Data cessazione:Importo";
                string headerAlign = "C:C:C:C";

                foreach (var groupPag in dipendenti.GroupBy(x => x.DATA_PAGAMENTO).OrderBy(y => y.Key))
                {
                    pdf.WriteLine("Data pagamento " + groupPag.Key.Value.ToString("dd/MM/yyyy"), true);
                    pdf.AggiungiTablePagamenti(headerString, headerAlign, true, null, null);
                    foreach (var dip in groupPag.OrderBy(z => z.ANAGPERS.DES_COGNOMEPERS).ThenBy(w => w.ANAGPERS.DES_NOMEPERS))
                    {
                        var datiBancari = dip.ANAGPERS.XR_DATIBANCARI.OrderByDescending(y => y.DTA_FINE).FirstOrDefault(x => x.XR_UTILCONTO.Count() == 1 && x.XR_UTILCONTO.First().COD_UTILCONTO == "01");

                        pdf.AggiungiTablePagamenti(dip.ANAGPERS.DES_COGNOMEPERS.TitleCase() + " " + dip.ANAGPERS.DES_NOMEPERS.TitleCase()
                            + (datiBancari.IND_VINCOLATO == "Y" ? " (conto vincolato)" : "") + ":"
                            + dip.SEDE + ":"
                            + dip.DATA_CESSAZIONE.Value.ToString("dd/MM/yyyy") + ":"
                            + (dip.IMPORTO_NETTO.HasValue ? dip.IMPORTO_NETTO.Value.ToString("N") : "0.00") + " €", "L:L:L:R", false,
                            headerString, headerAlign);
                    }
                    pdf.AggiungiTablePagamenti(":Sub Totale:" + groupPag.Select(x => x.IMPORTO_NETTO).Sum().Value.ToString("N") + " €", "L:L:R", true, null, null);
                    pdf.WriteLine(" ", false);
                }
            }

            pdf.Chiudi(out ms);

            return ms;
        }

        public static MemoryStream CreaPDFProvvista(bool isSum, string imgLogo, string imgInt, string title, IQueryable<XR_INC_DIPENDENTI> dipendenti)
        {
            Dictionary<string, string> gruppi = HrisHelper.GetParametroJson<Dictionary<string, string>>(HrisParam.IncentiviGruppi);

            MemoryStream ms = null;

            PdfPrinter pdf = new PdfPrinter(imgLogo, imgInt, title);

            var gruppiDip = dipendenti.Select(x => x.COD_GRUPPO).Distinct();
            string intestazione = "";
            foreach (var item in gruppiDip)
            {
                string tmp = "";
                if (gruppi.TryGetValue(item, out tmp))
                    intestazione = intestazione+(!String.IsNullOrWhiteSpace(intestazione) ? " - ":"") + tmp;
            }

            pdf.Apri(0, intestazione);

            pdf.WriteLine(title, true);
            pdf.WriteLine(" ", false);
            pdf.WriteLine("Data ultimo aggiornamento: " + DateTime.Now.ToString("dd/MM/yyyy"), false);
            pdf.WriteLine("Numero pratiche: " + dipendenti.Count(), true);
            pdf.WriteLine("Totale: " + dipendenti.Select(x => x.IMPORTO_NETTO != null ? x.IMPORTO_NETTO : 0).Sum().Value.ToString("N") + " €", true);
            pdf.WriteLine(" ", false);

            if (dipendenti != null && dipendenti.Count() > 0)
            {
                if (!isSum)
                {
                    string headerString = "Nominativo:Data pagamento:Importo";
                    string headerAlign = "C:C:C";

                    foreach (var groupPag in dipendenti.GroupBy(x => x.DATA_CESSAZIONE).OrderBy(y => y.Key))
                    {
                        pdf.WriteLine("Cessati il " + groupPag.Key.Value.ToString("dd/MM/yyyy"), true);
                        pdf.AggiungiTablePagamenti(headerString, headerAlign, true, null, null);
                        foreach (var dip in groupPag.OrderBy(z => z.ANAGPERS.DES_COGNOMEPERS).ThenBy(w => w.ANAGPERS.DES_NOMEPERS))
                        {
                            var datiBancari = dip.ANAGPERS.XR_DATIBANCARI.OrderByDescending(y => y.DTA_FINE).FirstOrDefault(x => x.XR_UTILCONTO.Count() == 1 && x.XR_UTILCONTO.First().COD_UTILCONTO == "01");

                            pdf.AggiungiTablePagamenti(dip.ANAGPERS.DES_COGNOMEPERS.TitleCase() + " " + dip.ANAGPERS.DES_NOMEPERS.TitleCase()
                                + (datiBancari.IND_VINCOLATO == "Y" ? " (conto vincolato)" : "") + ":"
                                + dip.DATA_PAGAMENTO.Value.ToString("dd/MM/yyyy") + ":"
                                + (dip.IMPORTO_NETTO.HasValue ? dip.IMPORTO_NETTO.Value.ToString("N") : "0.00") + " €", "L:L:R", false,
                                headerString, headerAlign);
                        }
                        pdf.AggiungiTablePagamenti(":Sub Totale:" + groupPag.Select(x => x.IMPORTO_NETTO).Sum().Value.ToString("N") + " €", "L:L:R", true, null, null);
                        pdf.WriteLine(" ", false);
                    }
                }
                else
                {
                    string headerString = "Cessazione:Numero:Importo";
                    string headerAlign = "C:C:C";

                    pdf.AggiungiTablePagamenti(headerString, headerAlign, true, null, null, true);
                    foreach (var groupPag in dipendenti.GroupBy(x => x.DATA_CESSAZIONE).OrderBy(y => y.Key))
                    {

                        pdf.AggiungiTablePagamenti(groupPag.Key.Value.ToString("dd/MM/yyyy") + ":"
                                                   + groupPag.Count() + ":"
                                                   + groupPag.Select(x => x.IMPORTO_NETTO).Sum().Value.ToString("N") + " €", "L:R:R", false,
                                                    headerString, headerAlign, true);
                    }
                    pdf.AggiungiTablePagamenti("Totale:" + dipendenti.Count() + ":" + dipendenti.Select(x => x.IMPORTO_NETTO).Sum().Value.ToString("N") + " €", "R:R:R", true, null, null, true);
                    pdf.WriteLine(" ", false);
                }
            }

            pdf.Chiudi(out ms);

            return ms;
        }

        public static MemoryStream CreaPDFCosti(bool isSum, string imgLogo, string imgInt, string title, IQueryable<XR_INC_DIPENDENTI> dipendenti)
        {
            MemoryStream ms = null;

            PdfPrinter pdf = new PdfPrinter(imgLogo, imgInt, title);
            pdf.Apri(0, "Incentivi");

            pdf.WriteLine(title, true);
            pdf.WriteLine(" ", false);
            pdf.WriteLine("Data ultimo aggiornamento: " + DateTime.Now.ToString("dd/MM/yyyy"), false);
            pdf.WriteLine("Numero pratiche: " + dipendenti.Count(), true);
            pdf.WriteLine("Totale: " + dipendenti.Select(x => x.IMPORTO_NETTO != null ? x.IMPORTO_NETTO : 0).Sum().Value.ToString("N") + " €", true);
            pdf.WriteLine(" ", false);

            if (dipendenti != null && dipendenti.Count() > 0)
            {
                if (!isSum)
                {
                    string headerString = "Nominativo:Data pagamento:Lordo:Netto";
                    string headerAlign = "C:C:C:C";

                    foreach (var groupPag in dipendenti.GroupBy(x => x.DATA_CESSAZIONE).OrderBy(y => y.Key))
                    {
                        pdf.WriteLine("Cessati il " + groupPag.Key.Value.ToString("dd/MM/yyyy"), true);
                        pdf.AggiungiTablePagamenti(headerString, headerAlign, true, null, null);
                        foreach (var dip in groupPag.OrderBy(z => z.ANAGPERS.DES_COGNOMEPERS).ThenBy(w => w.ANAGPERS.DES_NOMEPERS))
                        {
                            var datiBancari = dip.ANAGPERS.XR_DATIBANCARI.OrderByDescending(y => y.DTA_FINE).FirstOrDefault(x => x.XR_UTILCONTO.Count() == 1 && x.XR_UTILCONTO.First().COD_UTILCONTO == "01");

                            pdf.AggiungiTablePagamenti(dip.ANAGPERS.DES_COGNOMEPERS.TitleCase() + " " + dip.ANAGPERS.DES_NOMEPERS.TitleCase()
                                + (datiBancari.IND_VINCOLATO == "Y" ? " (conto vincolato)" : "") + ":"
                                + (dip.DATA_PAGAMENTO.HasValue ? dip.DATA_PAGAMENTO.Value.ToString("dd/MM/yyyy") : "-") + ":"
                                + ((dip.INCENTIVO_LORDO.HasValue ? dip.INCENTIVO_LORDO.Value : 0) + (dip.EX_FISSA.HasValue ? dip.EX_FISSA.Value : 0) + (dip.IMPORTO_LORDO.HasValue ? dip.IMPORTO_LORDO.Value : 0)).ToString("N") + " €" + ":"
                                + (dip.IMPORTO_NETTO.HasValue ? dip.IMPORTO_NETTO.Value.ToString("N") : "0.00") + " €"
                                , "L:L:R:R", false,
                                headerString, headerAlign);
                        }
                        pdf.AggiungiTablePagamenti(":Sub Totale:" + groupPag.Select(x => x.IMPORTO_NETTO).Sum().Value.ToString("N") + " €", "L:L:R", true, null, null);
                        pdf.WriteLine(" ", false);
                    }
                }
                else
                {
                    string headerString = "Cessazione:Numero:Lordo:Netto";
                    string headerAlign = "C:C:C:C";

                    pdf.AggiungiTablePagamenti(headerString, headerAlign, true, null, null, true);
                    foreach (var groupPag in dipendenti.GroupBy(x => x.DATA_CESSAZIONE).OrderBy(y => y.Key))
                    {

                        pdf.AggiungiTablePagamenti(groupPag.Key.Value.ToString("dd/MM/yyyy") + ":"
                                                   + groupPag.Count() + ":"
                                                   + groupPag.Select(x => x.INCENTIVO_LORDO + x.EX_FISSA + x.IMPORTO_LORDO).Sum().Value.ToString("N") + " €" + ":"
                                                   + groupPag.Select(x => x.IMPORTO_NETTO).Sum().Value.ToString("N") + " €"
                                                   , "L:R:R:R", false,
                                                    headerString, headerAlign, true);
                    }
                    pdf.AggiungiTablePagamenti("Totale:"
                                                + dipendenti.Count() + ":"
                                                + dipendenti.Select(x => x.INCENTIVO_LORDO + x.EX_FISSA + x.IMPORTO_LORDO).Sum().Value.ToString("N") + " €" + ":"
                                                + dipendenti.Select(x => x.IMPORTO_NETTO).Sum().Value.ToString("N") + " €", "R:R:R:R", true, null, null, true);
                    pdf.WriteLine(" ", false);
                }
            }

            pdf.Chiudi(out ms);

            return ms;
        }

        public static MemoryStream CreaXLSPagamenti(string imgLogo, string imgInt, string title, IQueryable<XR_INC_DIPENDENTI> dipendenti)
        {
            XLWorkbook workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(title);


            worksheet.Cell(1, 1).Value = "Data ultimo aggiornamento";
            worksheet.Cell(1, 2).Value = DateTime.Now.ToString("dd/MM/yyyy");
            worksheet.Cell(2, 1).Value = "Numero pratiche";
            worksheet.Cell(2, 2).Value = dipendenti.Count();

            int counter = 4;
            worksheet.Cell(counter, 1).Value = "Matricola";
            worksheet.Cell(counter, 2).Value = "Nominativo";
            worksheet.Cell(counter, 3).Value = "Sede";
            worksheet.Cell(counter, 4).Value = "Vincolo conto";
            worksheet.Cell(counter, 5).Value = "Data cessazione";
            worksheet.Cell(counter, 6).Value = "Data pagamento";
            worksheet.Cell(counter, 7).Value = "Importo";
            worksheet.Cell(counter, 8).Value = "Vincolo BCCR";

            if (dipendenti != null && dipendenti.Count() > 0)
            {
                foreach (var groupPag in dipendenti.GroupBy(x => x.DATA_PAGAMENTO).OrderBy(y => y.Key))
                {

                    foreach (var dip in groupPag.OrderBy(z => z.ANAGPERS.DES_COGNOMEPERS).ThenBy(w => w.ANAGPERS.DES_NOMEPERS))
                    {
                        counter++;
                        var datiBancari = dip.ANAGPERS.XR_DATIBANCARI.OrderByDescending(y => y.DTA_FINE).FirstOrDefault(x => x.XR_UTILCONTO.Count() == 1 && x.XR_UTILCONTO.First().COD_UTILCONTO == "01");

                        worksheet.Cell(counter, 1).SetValue<string>(dip.MATRICOLA);
                        worksheet.Cell(counter, 2).SetValue<string>(dip.ANAGPERS.DES_COGNOMEPERS.TitleCase() + " " + dip.ANAGPERS.DES_NOMEPERS.TitleCase());
                        worksheet.Cell(counter, 3).SetValue<string>(dip.SEDE);
                        worksheet.Cell(counter, 4).SetValue<string>(datiBancari.IND_VINCOLATO == "Y" ? "Vincolato" : "");
                        worksheet.Cell(counter, 5).SetValue<string>(dip.DATA_CESSAZIONE.Value.ToString("dd/MM/yyyy"));
                        worksheet.Cell(counter, 6).SetValue<string>(dip.DATA_PAGAMENTO.Value.ToString("dd/MM/yyyy"));
                        worksheet.Cell(counter, 7).SetValue<decimal>(dip.IMPORTO_NETTO.HasValue ? dip.IMPORTO_NETTO.Value : 0);
                        worksheet.Cell(counter, 8).SetValue<string>(dip.IND_PROPRIO_IBAN == "B" ? "Vincolo BCCR" : "");
                    }
                }
            }

            worksheet.Columns().AdjustToContents();

            MemoryStream ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;

            return ms;
        }

        public static MemoryStream CreaXLSProvvista(string imgLogo, string imgInt, string title, IQueryable<XR_INC_DIPENDENTI> dipendenti)
        {
            XLWorkbook workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(title);

            Dictionary<string, string> gruppi = HrisHelper.GetParametroJson<Dictionary<string, string>>(HrisParam.IncentiviGruppi);

            var gruppiDip = dipendenti.Select(x => x.COD_GRUPPO).Distinct();
            string intestazione = "";
            foreach (var item in gruppiDip)
            {
                string tmp = "";
                if (gruppi.TryGetValue(item, out tmp))
                    intestazione = intestazione + (!String.IsNullOrWhiteSpace(intestazione) ? " - " : "") + tmp;
            }

            worksheet.Cell(1, 1).Value = intestazione;
            worksheet.Cell(1, 1).Style.Font.Bold = true;
            worksheet.Cell(1, 1).Style.Font.FontSize++;

            worksheet.Cell(2, 1).Value = "Data aggiornamento";
            worksheet.Cell(2, 2).SetValue<DateTime>(DateTime.Now);
            worksheet.Cell(3, 1).Value = "Numero pratiche";
            worksheet.Cell(3, 2).Value = dipendenti.Count();

            int counter = 5;
            worksheet.Cell(counter, 1).Value = "Cessazione";
            worksheet.Cell(counter, 2).Value = "Numero";
            worksheet.Cell(counter, 3).Value = "Incentivo Lordo";
            worksheet.Cell(counter, 4).Value = "TFR Lordo";
            worksheet.Cell(counter, 5).Value = "Totale Lordo";
            worksheet.Cell(counter, 6).Value = "Importo Netto";
            worksheet.Cell(counter, 7).Value = "Una Tantum Lordo";

            if (dipendenti != null && dipendenti.Count() > 0)
            {
                foreach (var groupPag in dipendenti.GroupBy(x => x.DATA_CESSAZIONE).OrderBy(y => y.Key))
                {

                    counter++;
                    worksheet.Cell(counter, 1).SetValue<DateTime>(groupPag.Key.Value);
                    worksheet.Cell(counter, 2).SetValue<int>(groupPag.Count());
                    worksheet.Cell(counter, 3).SetValue<decimal>(groupPag.Sum(b => (b.INCENTIVO_LORDO ?? 0) + (b.EX_FISSA ?? 0)));
                    worksheet.Cell(counter, 4).SetValue<decimal>(groupPag.Sum(b => b.IMPORTO_LORDO ?? 0));
                    worksheet.Cell(counter, 5).SetValue<decimal>(groupPag.Sum(b => (b.INCENTIVO_LORDO ?? 0) + (b.EX_FISSA ?? 0) + (b.IMPORTO_LORDO ?? 0)));
                    worksheet.Cell(counter, 6).SetValue<decimal>(groupPag.Sum(b => b.IMPORTO_NETTO ?? 0));
                    worksheet.Cell(counter, 7).SetValue<decimal>(groupPag.Sum(b => b.UNA_TANTUM_LORDA ?? 0));

                }

                var range = worksheet.Range("A5:G" + counter);
                range.Column(3).Style.NumberFormat.NumberFormatId = 15; range.Column(3).Style.NumberFormat.Format = " 0,00 €";
                range.Column(4).Style.NumberFormat.NumberFormatId = 15; range.Column(4).Style.NumberFormat.Format = " 0,00 €";
                range.Column(5).Style.NumberFormat.NumberFormatId = 15; range.Column(5).Style.NumberFormat.Format = " 0,00 €";
                range.Column(6).Style.NumberFormat.NumberFormatId = 15; range.Column(6).Style.NumberFormat.Format = " 0,00 €";
                range.Column(7).Style.NumberFormat.NumberFormatId = 15; range.Column(7).Style.NumberFormat.Format = " 0,00 €";

                var table = range.CreateTable();
                table.ShowTotalsRow = true;
                table.Field(0).TotalsRowLabel = "Totale";
                table.Field(1).TotalsRowFunction = XLTotalsRowFunction.Sum;
                table.Field(2).TotalsRowFunction = XLTotalsRowFunction.Sum;
                table.Field(3).TotalsRowFunction = XLTotalsRowFunction.Sum;
                table.Field(4).TotalsRowFunction = XLTotalsRowFunction.Sum;
                table.Field(5).TotalsRowFunction = XLTotalsRowFunction.Sum;
                table.Field(6).TotalsRowFunction = XLTotalsRowFunction.Sum;
            }

            worksheet.Columns().AdjustToContents();
            worksheet.Columns().Width = 18;

            MemoryStream ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;

            return ms;
        }


        public static MemoryStream CreaPDFProspetto(string imgLogo, string imgFirma, string imgInt, string title, string nominativo, string dataCessazione, DataRowCollection rows, int pageStart = 0)
        {
            MemoryStream ms = null;

            PdfPrinter pdf = new PdfPrinter(imgLogo, imgInt, title);
            pdf.Apri(pageStart, nominativo, dataCessazione);
            bool isTable = false;
            bool isNewPage = false;

            bool tableDeterAliq = false;

            // disegno del logo
            iTextSharp.text.Image png = null;
            if (!String.IsNullOrWhiteSpace(imgFirma))
            {
                png = PdfPrinter.GetImage(imgFirma);
                png.ScalePercent(17.5f);
            }

            for (int i = 0; i < rows.Count - 1; i++)
            {
                string row = rows[i].Field<string>("RIGA");

                if (row.StartsWith("1"))
                {
                    isNewPage = true;
                    isTable = false;
                }
                else if (row.StartsWith(" ="))
                {
                    if (i != rows.Count - 2)
                    {
                        isNewPage = true;
                        isTable = false;
                        if (png != null)
                            pdf.AggiungiFirma(png);
                        pdf.NewPage();
                    }
                }
                else if (row.Trim().All(x => x == '-'))
                {
                    //isTable = false; 
                    if (isTable)
                        pdf.AggiungiTable("-:-:-:-:", false);
                    else
                        pdf.WriteLine(" ");

                }
                else if (row.Trim().All(x => x == '-' | x == ':'))
                {
                    ;
                }
                else if (row.Count(x => x == ':') == 4)
                {
                    pdf.AggiungiTable(row, !isTable);
                    isTable = true;
                }
                else if (row.Count(x => x == ':') == 3)
                {
                    pdf.AggiungiTableAnzianita(row);
                    isTable = false;
                }
                else
                {
                    if (!tableDeterAliq)
                    {
                        int indexRag = row.IndexOf("PAG.");
                        if (indexRag > -1)
                            row = row.Substring(0, indexRag);

                        pdf.WriteLine(row.Substring(1), isNewPage);
                        isTable = false;
                        isNewPage = false;

                        if (row.Contains("DETERMINAZIONE DELL'ALIQUOTA"))
                            tableDeterAliq = true;
                    }
                    else
                    {
                        pdf.AggiungiTableRedditoRif(row, rows[i + 1].Field<string>("RIGA"), rows[i + 2].Field<string>("RIGA"));
                        i += 2;
                        tableDeterAliq = false;
                    }
                }
            }

            if (png != null)
                pdf.AggiungiFirma(png);

            pdf.Chiudi(out ms);

            return ms;
        }

        public static MemoryStream CreaPDFTotale(MemoryStream riepilogo, MemoryStream prospetto, params MemoryStream[] allegati)
        {
            MemoryStream returnMs = new MemoryStream();

            MemoryStream _ms = new MemoryStream();
            Document _document = new Document(PageSize.A4, 0f, 0f, 0f, 0f);
            PdfCopy pdf = new PdfCopy(_document, _ms);
            _document.Open();

            PdfReader.unethicalreading = true;
            PdfReader reader = new PdfReader(riepilogo);
            pdf.AddDocument(reader);
            reader.Close();

            if (allegati != null)
            {
                for (int i = 0; i < allegati.Length; i++)
                {
                    var allegato = allegati[i];
                    reader = new PdfReader(allegato);
                    pdf.AddDocument(reader);
                    reader.Close();
                }
            }

            //reader = new PdfReader(prospetto);
            //pdf.AddDocument(reader);
            //reader.Close();

            _document.Close();

            byte[] byteInfo = _ms.ToArray();
            returnMs.Write(byteInfo, 0, byteInfo.Length);
            returnMs.Position = 0;

            return returnMs;
        }

        public static MemoryStream CreaPDFRiepilogo(IncentiviEntities db, XR_INC_DIPENDENTI dip, string imagePath, out int pageNumber)
        {
            pageNumber = 0;
            string matricola = dip.MATRICOLA;

            float indentationLeft = 150f;

            MemoryStream workStream = new MemoryStream();
            iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 24f, 24f, 24f, 24f);
            PdfWriter writer = PdfWriter.GetInstance(document, workStream);
            writer.CloseStream = false;

            List<RenderableItem> rowItemList = new List<RenderableItem>();

            AggiungiDatiDipendente(dip, rowItemList);

            byte[] tmp = File.ReadAllBytes(imagePath);
            //string image = anagrafica._foto;
            string image = Convert.ToBase64String(tmp);

            writer.PageEvent = new myRaiCommonModel.cvModels.Pdf.ITextEvents(image, rowItemList);

            document.Open();

            List<myRaiCommonModel.cvModels.Pdf.ContentBlockInfo> contentBlockInfoList = new List<myRaiCommonModel.cvModels.Pdf.ContentBlockInfo>();

            AggiungiDatiPratica(db, dip, contentBlockInfoList);

            RightContentManager rm = new RightContentManager(writer.DirectContentUnder, document, contentBlockInfoList);
            rm.Title = string.Format("{0} {1}", dip.SINTESI1.DES_NOMEPERS.TitleCase(), dip.SINTESI1.DES_COGNOMEPERS.TitleCase());
            rm.SubTitle = "Data cessazione: " + dip.DATA_CESSAZIONE.Value.ToString("dd/MM/yyyy");
            rm.IndentationLeft = indentationLeft;
            rm.Render();
            pageNumber = writer.PageNumber;
            document.Close();


            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            return workStream;
        }

        private static void AggiungiDatiDipendente(XR_INC_DIPENDENTI dip, List<RenderableItem> rowItemList)
        {
            string figProfessionale = dip.SINTESI1.ASSQUAL.QUALIFICA.TB_QUALSTD.DES_QUALSTD.TitleCase();
            string matricola = dip.MATRICOLA;
            DateTime? dataNascita = dip.SINTESI1.DTA_NASCITAPERS;
            string tipoContratto = dip.SINTESI1.DES_TPCNTR.TitleCase();
            string email = String.Format("p{0}@rai.it", matricola);
            string inquadramentoStr = "";

            using (var ctx = new myRai.Data.CurriculumVitae.cv_ModelEntities())
            {
                var param = new SqlParameter("@param", dip.SINTESI1.COD_UNITAORG);

                try
                {
                    var tmp = ctx.Database.SqlQuery<string>("exec sp_GERARSEZIONE @param", param).ToList();
                    inquadramentoStr = tmp[0].ToString();
                }
                catch (Exception ex)
                {
                }
            }

            string[] inquadramento = inquadramentoStr.ToString().Split(';');
            if (inquadramento.Length > 1)
            {
                SingleValue[] values = new SingleValue[inquadramento.Length - 1];

                for (int i = 1; i < inquadramento.Length; i++)
                {
                    if (i == 1)
                    {
                        rowItemList.Add(new KeyValue { key = "SETTORE", value = inquadramento[i] });
                        continue;
                    }

                    rowItemList.Add(new SingleValue { newLine = false, value = inquadramento[i], IndentationLeft = 5f * (i - 1) });
                }
            }

            rowItemList.Add(new KeyValue { key = "CONTRATTO", value = tipoContratto });
            rowItemList.Add(new KeyValue { key = "FIGURA PROFESSIONALE", value = figProfessionale });
            //rowItemList.Add(new KeyValue { key = "MANSIONE", value = anagrafica._qualifica });
            rowItemList.Add(new KeyValue { key = "MATRICOLA", value = matricola });

            string indirizzo = "";

            try
            {
                myRaiServiceHub.it.rai.servizi.hrce.retData retdata = new myRaiServiceHub.it.rai.servizi.hrce.retData();
                retdata.ds = ProfiloPersonaleManager.GetProfiloPersonaleFromDB(matricola);


                DataTable dr_recapiti = null;
                if (retdata.ds != null)
                    dr_recapiti = retdata.ds.Tables["Table1"];

                if (dr_recapiti != null && dr_recapiti.Rows[0]["Matricola"].ToString() != matricola)
                {
                    dr_recapiti = null;
                }

                if (dr_recapiti != null)
                {
                    indirizzo = string.Format("{0} {1} {2}",
                                dr_recapiti.Rows[0]["INDIRIZZODOM"].ToString(),
                                dr_recapiti.Rows[0]["CAPDOM"].ToString()
                                , dr_recapiti.Rows[0]["CITTADOM"].ToString()
                                );

                    indirizzo = indirizzo.TitleCase();

                    indirizzo = string.Format("{0} ({1})",
                                indirizzo
                                , dr_recapiti.Rows[0]["PROVDOM"].ToString().ToUpper()
                                );
                }

            }
            catch (Exception)
            {
            }

            rowItemList.Add(new LineBreak());
            rowItemList.Add(new KeyValue { key = "DATA DI NASCITA", value = String.Format("{0:dd/MM/yyyy}", dataNascita) });
            rowItemList.Add(new KeyValue { key = "INDIRIZZO", value = indirizzo });
            rowItemList.Add(new LineBreak());
            rowItemList.Add(new KeyValue { key = "EMAIL", value = email });

        }

        private static void AggiungiDatiPratica(IncentiviEntities db, XR_INC_DIPENDENTI dip, List<myRaiCommonModel.cvModels.Pdf.ContentBlockInfo> contentBlockInfoList)
        {
            List<myRaiCommonModel.cvModels.Pdf.RenderableItem> blockItemInfoList = new List<myRaiCommonModel.cvModels.Pdf.RenderableItem>();


            /*DATI RICHIESTA*/
            if (dip.XR_INC_OPERSTATI.Any(x => x.DATA_FINE_VALIDITA == null && x.ID_STATO == (int)IncStato.RichiestaAccettata))
            {
                blockItemInfoList = new List<RenderableItem>();

                var rows = new List<Tuple<string, string>>();

                rows.Add(new Tuple<string, string>("Data inserimento", dip.DTA_RICHIESTA.Value.ToString("dd/MM/yyyy")));
                rows.Add(new Tuple<string, string>("Data uscita richiesta", dip.DATA_USCITA_RICH.Value.ToString("dd/MM/yyyy")));
                rows.Add(new Tuple<string, string>("Tipologia scelta", DecodTipologiaUscita(dip.NOT_TIP_SCELTA)));
                if (!String.IsNullOrWhiteSpace(dip.NOT_TIP_ACCERT))
                    rows.Add(new Tuple<string, string>("Tipologia accertata", DecodTipologiaUscita(dip.NOT_TIP_ACCERT)));
                if (!String.IsNullOrWhiteSpace(dip.NOT_REQ_MATURATO))
                {
                    string maturazione = "";
                    var req = db.XR_INC_DATE_REQUISITI.FirstOrDefault(x => x.COD_REQUISITO == dip.NOT_REQ_MATURATO);
                    if (req != null)
                        maturazione = req.DATA_FINE.Value.ToString("dd/MM/yyyy");
                    else
                        maturazione = "-";

                    rows.Add(new Tuple<string, string>("Requisiti maturati", maturazione));
                }
                if (dip.DATA_PENSIONE_ANTICIPATA.HasValue)
                    rows.Add(new Tuple<string, string>("Data pensione anticipata", dip.DATA_PENSIONE_ANTICIPATA.Value.ToString("dd/MM/yyyy")));
                if (dip.DATA_ARRIVO_DOC.HasValue)
                    rows.Add(new Tuple<string, string>("Data arrivo estratti", dip.DATA_ARRIVO_DOC.Value.ToString("dd/MM/yyyy")));
                if (dip.DATA_RICH_INT.HasValue)
                    rows.Add(new Tuple<string, string>("Data richiesta integrazione", dip.DATA_RICH_INT.Value.ToString("dd/MM/yyyy")));
                if (dip.DATA_RICH_INT.HasValue && dip.DATA_ARRIVO_INT.HasValue)
                    rows.Add(new Tuple<string, string>("Data arrivo integrazione", dip.DATA_ARRIVO_INT.Value.ToString("dd/MM/yyyy")));

                blockItemInfoList.Add(new IncentiviBlockInfo
                {
                    rows = rows
                });
                contentBlockInfoList.Add(new ContentBlockInfo("Dati richiesta", blockItemInfoList));
            }

            /*DATI RECESSO*/
            if (dip.XR_INC_OPERSTATI.Any(x => x.DATA_FINE_VALIDITA == null && x.ID_STATO == (int)IncStato.RecessoEffettuato))
            {
                blockItemInfoList = new List<RenderableItem>();
                var rows = new List<Tuple<string, string>>();

                if (dip.DATA_INVIO_PROP.HasValue)
                    rows.Add(new Tuple<string, string>("Data invio proposta", dip.DATA_INVIO_PROP.Value.ToString("dd/MM/yyyy")));
                if (dip.DATA_FIRMA_DIP.HasValue)
                    rows.Add(new Tuple<string, string>("Data firma proposta", dip.DATA_FIRMA_DIP.Value.ToString("dd/MM/yyyy")));
                if (dip.DTA_ACCETT_AZ.HasValue)
                    rows.Add(new Tuple<string, string>("Data invio accettazione", dip.DTA_ACCETT_AZ.Value.ToString("dd/MM/yyyy")));
                if (dip.DATA_FIRMA_DIP_ACCETT_AZ.HasValue)
                    rows.Add(new Tuple<string, string>("Data firma accettazione", dip.DATA_FIRMA_DIP_ACCETT_AZ.Value.ToString("dd/MM/yyyy")));
                if (dip.DTA_RECESSO.HasValue)
                    rows.Add(new Tuple<string, string>("Data recesso", dip.DTA_RECESSO.Value.ToString("dd/MM/yyyy")));


                bool hasAnpal = false;
                if (dip.DATA_RECESSO_ANPAL.HasValue)
                {
                    var tmp = dip.XR_INC_OPERSTATI.FirstOrDefault(x => x.DATA_FINE_VALIDITA == null && x.ID_STATO == (int)IncStato.FileAnpal);
                    if (tmp != null)
                    {
                        hasAnpal = true;
                        if (tmp.XR_INC_OPERSTATI_DOC != null && tmp.XR_INC_OPERSTATI_DOC.Any(x => x.VALID_DTA_END == null))
                            rows.Add(new Tuple<string, string>("Data invio ANPAL", dip.DATA_RECESSO_ANPAL.Value.ToString("dd/MM/yyyy")));
                    }
                }

                if (dip.DATA_INVIO_PROP.HasValue
                    || dip.DATA_FIRMA_DIP.HasValue
                    || dip.DTA_ACCETT_AZ.HasValue
                    || dip.DATA_FIRMA_DIP_ACCETT_AZ.HasValue
                    || dip.DTA_RECESSO.HasValue
                    || hasAnpal)
                {
                    blockItemInfoList.Add(new IncentiviBlockInfo
                    {
                        rows = rows
                    });
                    contentBlockInfoList.Add(new ContentBlockInfo("Dati recesso", blockItemInfoList));
                }
            }

            /*DATI CONTABILI*/
            if (dip.XR_INC_OPERSTATI.Any(x => x.DATA_FINE_VALIDITA == null && x.ID_STATO == (int)IncStato.Conteggio))
            {
                blockItemInfoList = new List<RenderableItem>();
                blockItemInfoList.Add(new IncentiviBlockInfo
                {
                    rows = new List<Tuple<string, string>>()
                {
                    new Tuple<string,string>("Incentivo Lordo", "€ "+(dip.INCENTIVO_LORDO.GetValueOrDefault()+dip.EX_FISSA.GetValueOrDefault()).ToString("N")),
                    new Tuple<string,string>("Una Tantum Lordo", "€ "+dip.UNA_TANTUM_LORDA.GetValueOrDefault().ToString("N")),
                    new Tuple<string,string>("Importo Lordo TFR", "€ "+dip.IMPORTO_LORDO.GetValueOrDefault().ToString("N")),
                }
                });
                contentBlockInfoList.Add(new ContentBlockInfo("Dati contabili", blockItemInfoList));
            }

            /*DATI APPUNTAMENTO*/
            if (dip.XR_INC_OPERSTATI.Any(x => x.DATA_FINE_VALIDITA == null && x.ID_STATO == (int)IncStato.Appuntamento))
            {
                blockItemInfoList = new List<RenderableItem>();
                blockItemInfoList.Add(new IncentiviBlockInfo
                {
                    rows = new List<Tuple<string, string>>()
                {
                    new Tuple<string,string>("Data appuntamento", dip.DATA_APPUNTAMENTO.Value.ToString("dd/MM/yyyy")),
                    new Tuple<string,string>("Sigla sindacale", dip.XR_INC_SIGLESINDACALI!=null?dip.XR_INC_SIGLESINDACALI.SINDACATO:"-"),
                    new Tuple<string,string>("Rappresentante sindacato", dip.XR_INC_RAPPRSINDACATO!=null && !String.IsNullOrWhiteSpace(dip.XR_INC_RAPPRSINDACATO.COGNOME)?(dip.XR_INC_RAPPRSINDACATO.COGNOME + " " + dip.XR_INC_RAPPRSINDACATO.NOME):"-"),
                    new Tuple<string,string>("Rappresentante Industria", dip.XR_INC_RAPPRINDUSTRIA!=null && !String.IsNullOrWhiteSpace(dip.XR_INC_RAPPRINDUSTRIA.COGNOME)?dip.XR_INC_RAPPRINDUSTRIA.COGNOME + " " + dip.XR_INC_RAPPRINDUSTRIA.NOME:"-"),
                    new Tuple<string,string>("Rappresentante RAI", dip.XR_INC_RAPPRRAI!=null && dip.XR_INC_RAPPRRAI.ID_RAPPRRAI>0? dip.XR_INC_RAPPRRAI.ANAGPERS.DES_COGNOMEPERS.TitleCase() + " " + dip.XR_INC_RAPPRRAI.ANAGPERS.DES_NOMEPERS.TitleCase():"-")
                }
                });
                contentBlockInfoList.Add(new ContentBlockInfo("Dati appuntamento", blockItemInfoList));
            }

            if (dip.XR_INC_OPERSTATI.Any(x => x.DATA_FINE_VALIDITA == null && x.ID_STATO == (int)IncStato.Cedolini))
            {
                /*DATI PAGAMENTO*/
                blockItemInfoList = new List<RenderableItem>();
                IncentiviBlockInfo block = new IncentiviBlockInfo() { newLine = false, rows = new List<Tuple<string, string>>() };
                block.rows.Add(new Tuple<string, string>("Importo netto", "€ " + dip.IMPORTO_NETTO.Value.ToString("N")));
                block.rows.Add(new Tuple<string, string>("Data pagamento", dip.DATA_PAGAMENTO.Value.ToString("dd/MM/yyyy")));
                switch (dip.IND_PROPRIO_IBAN)
                {
                    case "Y":
                        block.rows.Add(new Tuple<string, string>("Tipo accredito", "proprio conto corrente"));
                        break;
                    case "N":
                        block.rows.Add(new Tuple<string, string>("Tipo accredito", "altro conto corrente"));
                        break;
                    case "V":
                        block.rows.Add(new Tuple<string, string>("Tipo accredito", "VINCOLATO"));
                        break;
                    case "B":
                        block.rows.Add(new Tuple<string, string>("Tipo accredito", "VINCOLO BCCR"));
                        break;
                }
                block.rows.Add(new Tuple<string, string>("Banca", dip.BANCA));
                block.rows.Add(new Tuple<string, string>("Intestatario conto", dip.INTESTATARIO_CONTO));
                block.rows.Add(new Tuple<string, string>("IBAN", dip.IBAN));
                blockItemInfoList.Add(block);
                contentBlockInfoList.Add(new ContentBlockInfo("Dati pagamento", blockItemInfoList));
            }

            /*NOTE*/
            string key = "", value = "";
            if (!String.IsNullOrWhiteSpace(dip.CAUSE_VERTENZE) || (dip.XR_INC_DIPENDENTI_NOTE != null && dip.XR_INC_DIPENDENTI_NOTE.Count() > 0))
            {
                blockItemInfoList = new List<RenderableItem>();
                if (!String.IsNullOrWhiteSpace(dip.CAUSE_VERTENZE))
                {
                    key = "CAUSE/VERTENZE";
                    value = dip.CAUSE_VERTENZE;
                    blockItemInfoList.Add(new BlockItemInfo
                    {
                        key = key,
                        value = value
                    });
                }
                if (dip.XR_INC_DIPENDENTI_NOTE != null)
                {
                    foreach (var item in dip.XR_INC_DIPENDENTI_NOTE)
                    {
                        key = "Nota:";
                        value = item.NOTA;
                        blockItemInfoList.Add(new BlockItemInfo
                        {
                            key = key,
                            value = value
                        });
                    }
                }
                contentBlockInfoList.Add(new ContentBlockInfo("Note", blockItemInfoList));
            }
        }

        public static ElencoAnagrafiche GetElencoAnagrafiche(RicercaAnagrafica model)
        {
            ElencoAnagrafiche elencoAnagrafiche = new ElencoAnagrafiche();
            IncentiviEntities db = new IncentiviEntities();

            elencoAnagrafiche.IdCampagna = model.Piano;
            if (model.Piano > 1 && !String.IsNullOrWhiteSpace(model.Decorrenza))
                elencoAnagrafiche.Decorrenza = DateTime.ParseExact(model.Decorrenza, "dd/MM/yyyy", null);

            if (model.HasFilter)
            {
                IQueryable<myRaiData.Incentivi.SINTESI1> tmp = db.SINTESI1.Include("XR_INC_DIPENDENTI");

                if (!String.IsNullOrWhiteSpace(model.Matricola))
                    tmp = tmp.Where(x => model.Matricola.Contains(x.COD_MATLIBROMAT));

                if (!String.IsNullOrWhiteSpace(model.Cognome))
                    tmp = tmp.Where(x => x.DES_COGNOMEPERS.StartsWith(model.Cognome.ToUpper()));

                if (!String.IsNullOrWhiteSpace(model.Nome))
                    tmp = tmp.Where(x => x.DES_NOMEPERS.StartsWith(model.Nome.ToUpper()));

                if (!String.IsNullOrWhiteSpace(model.Servizio))
                    tmp = tmp.Where(x => x.COD_SERVIZIO == model.Servizio);

                tmp = tmp.OrderBy(y => y.DES_COGNOMEPERS).ThenBy(z => z.DES_NOMEPERS);

                elencoAnagrafiche.anagrafiche = tmp.ToList();
            }
            elencoAnagrafiche.TreeSearch = model.TreeSearch;
            return elencoAnagrafiche;
        }

        public static MemoryStream ExportRiepilogoPratiche(string stato = "attive")
        {
            MemoryStream ms = new MemoryStream();
            XLWorkbook wb = new XLWorkbook();

            IncentiviEntities db = new IncentiviEntities();
            List<CessazioneModel> elenco = new List<CessazioneModel>();

            string codGruppo = HrisHelper.GetParametro<string>(HrisParam.IncentiviRifGruppo);
            var tmp = db.XR_INC_DIPENDENTI
                    .Where(x => x.ID_TIPOLOGIA == (int)CessazioneTipo.Incentivazione && x.COD_GRUPPO == codGruppo);

            if (stato == "attive")
            {
                var richiesteValide = CessazioneHelper.IsActive();
                tmp = tmp.Where(richiesteValide);
            }
            else if (stato == "inattive")
            {
                var richiesteValide = CessazioneHelper.IsCurrentState(IncentivazioneHelper.InactiveState());
                tmp = tmp.Where(richiesteValide);
            }

            elenco.AddRange(tmp
                    .OrderBy(x => x.PROGR_RICHIESTA)
                    .ThenBy(x => x.DATA_CESSAZIONE)
                    .ThenBy(x => x.SINTESI1.DES_COGNOMEPERS)
                    .ThenBy(y => y.SINTESI1.DES_NOMEPERS)
                    .Select(x => new CessazioneModel
                    {
                        Pratica = x,
                        Stato = x.XR_INC_OPERSTATI.Where(y => !y.XR_INC_STATI.IND_SYSTEM.Value && !y.DATA_FINE_VALIDITA.HasValue)
                              .Select(w => w.XR_INC_STATI).OrderByDescending(z => z.XR_WKF_WORKFLOW.FirstOrDefault(a => a.ID_TIPOLOGIA == x.ID_TIPOLOGIA).ORDINE)
                              .FirstOrDefault(),
                        Sintesi = x.SINTESI1,
                        Qualifica = x.SINTESI1.QUALIFICA,
                        QualStd = x.SINTESI1.QUALIFICA.TB_QUALSTD
                    }));

            var ws = wb.AddWorksheet("Riepilogo");


            int row = 1;
            ws.Cell(row, 1).SetValue("Progressivo");
            ws.Cell(row, 2).SetValue("Matricola");
            ws.Cell(row, 3).SetValue("Nominativo");
            ws.Cell(row, 4).SetValue("Direzione");
            ws.Cell(row, 5).SetValue("Categoria");
            ws.Cell(row, 6).SetValue("Data cessazione");
            ws.Cell(row, 7).SetValue("Tipologia");
            ws.Cell(row, 8).SetValue("Data invio proposta");
            ws.Cell(row, 9).SetValue("Data accettazione proposta");
            ws.Cell(row, 10).SetValue("Data recesso online");
            ws.Cell(row, 11).SetValue("Incentivo lordo");
            ws.Cell(row, 12).SetValue("Una Tantum");
            ws.Cell(row, 13).SetValue("Ex Fissa/Preavviso");
            ws.Cell(row, 14).SetValue("Oneri");
            ws.Cell(row, 15).SetValue("Costo");
            int offset = 0;

            ws.Cell(row, 16).SetValue("Stato");
            offset++;

            foreach (var item in elenco)
            {
                row++;
                ws.Cell(row, 1).SetValue<int>(item.Pratica.PROGR_RICHIESTA.GetValueOrDefault());
                ws.Cell(row, 2).SetValue<string>(item.Pratica.MATRICOLA);
                ws.Cell(row, 3).SetValue<string>(item.Sintesi.Nominativo());
                ws.Cell(row, 4).SetValue<string>(CezanneHelper.GetDes(item.Sintesi.COD_SERVIZIO, item.Sintesi.DES_SERVIZIO));
                ws.Cell(row, 5).SetValue<string>(item.QualStd.DES_QUALSTD.UpperFirst());

                ws.Cell(row, 6).SetValue<DateTime>(item.Pratica.DATA_CESSAZIONE.Value);
                string scelta = item.Pratica.NOT_TIP_ACCERT;
                if (String.IsNullOrWhiteSpace(scelta))
                    scelta = item.Pratica.NOT_TIP_SCELTA;

                string decodTipo = DecodTipologiaUscita(scelta);

                ws.Cell(row, 7).SetValue(decodTipo);

                if (item.Pratica.DATA_INVIO_PROP.HasValue)
                    ws.Cell(row, 8).SetValue<DateTime>(item.Pratica.DATA_INVIO_PROP.Value);
                if (item.Pratica.DATA_FIRMA_DIP.HasValue)
                    ws.Cell(row, 9).SetValue<DateTime>(item.Pratica.DATA_FIRMA_DIP.Value);
                if (item.Pratica.DTA_RECESSO.HasValue)
                    ws.Cell(row, 10).SetValue<DateTime>(item.Pratica.DTA_RECESSO.Value);
                ws.Cell(row, 11).SetValue<decimal>(item.Pratica.INCENTIVO_LORDO.GetValueOrDefault());
                ws.Cell(row, 12).SetValue<decimal>(item.Pratica.UNA_TANTUM_LORDA.GetValueOrDefault());
                ws.Cell(row, 13).SetValue<decimal>(item.Pratica.EX_FISSA.GetValueOrDefault());

                decimal costo = item.Pratica.INCENTIVO_LORDO.GetValueOrDefault() + item.Pratica.UNA_TANTUM_LORDA.GetValueOrDefault() + item.Pratica.EX_FISSA.GetValueOrDefault();
                decimal oneri = CessazioneHelper.CalcoloOneri(item.Sintesi.COD_QUALIFICA, item.Pratica.UNA_TANTUM_LORDA.GetValueOrDefault());
                costo = costo + oneri;

                ws.Cell(row, 14).SetValue<decimal>(oneri);
                ws.Cell(row, 15).SetValue<decimal>(costo);

                ws.Cell(row, 16).SetValue<string>(item.Stato.DESCRIZIONE);
            }

            ws.Range(2, 11, row, 15).Style.NumberFormat.Format = "#,##0.00 €";

            var table = ws.Range(1, 1, row, 16).CreateTable();

            ws.Columns().AdjustToContents();

            ms = new MemoryStream();
            wb.SaveAs(ms);
            ms.Position = 0;

            return ms;
        }

        public static List<SelectListItem> GetTipologiaUscita()
        {
            return new List<SelectListItem>()
                        {
                        new SelectListItem() { Value = "", Text = "Seleziona un valore" },
                        new SelectListItem(){Value="Quota100",Text="Quota 100"},
                        new SelectListItem(){Value="Quota102",Text="Quota 102"},
                        new SelectListItem(){Value="NoQuota100;PensioneVecchiaia",Text="No Quota 100 - Pensione di vecchiaia"},
                        new SelectListItem(){Value="NoQuota100;PensioneAnticipata",Text="No Quota 100 - Pensione anticipata"},
                        new SelectListItem(){Value="NoQuota100;PensioneAnzianita",Text="No Quota 100 - Pensione di anzianita"},
                        };
        }


        public static string DecodTipologiaUscita(string scelta)
        {
            string decodTipo = "";
            switch (scelta)
            {
                case "Quota100":
                    decodTipo = "Quota 100";
                    break;
                case "NoQuota100;PensioneVecchiaia":
                    decodTipo = "No Quota 100 - Pensione di vecchiaia";
                    break;
                case "NoQuota100;PensioneAnticipata":
                    decodTipo = "No Quota 100 - Pensione di anticipata";
                    break;
                case "NoQuota100;PensioneAnzianita":
                    decodTipo = "No Quota 100 - Pensione di anzianita";
                    break;
                case "Quota102":
                    decodTipo = "Quota 102";
                    break;
            }

            return decodTipo;
        }

        public static MemoryStream ExportRiepilogoSaving(bool cumulativo = false)
        {
            MemoryStream ms = new MemoryStream();
            XLWorkbook wb = new XLWorkbook();

            string codGruppo = HrisHelper.GetParametro<string>(HrisParam.IncentiviRifGruppo);

            IncentiviEntities db = new IncentiviEntities();
            var richiesteValide = CessazioneHelper.IsActive();
            var elenco = db.XR_INC_DIPENDENTI
                .Where(x => x.ID_TIPOLOGIA == (int)CessazioneTipo.Incentivazione && x.COD_GRUPPO == codGruppo)
                //.Where(richiesteValide)
                .OrderBy(x => x.PROGR_RICHIESTA)
                .Select(x => new CessazioneModel
                {
                    Pratica = x,
                    Stato = x.XR_INC_OPERSTATI.Where(y => !y.XR_INC_STATI.IND_SYSTEM.Value && !y.DATA_FINE_VALIDITA.HasValue)
                          .Select(w => w.XR_INC_STATI).OrderByDescending(z => z.XR_WKF_WORKFLOW.FirstOrDefault(a => a.ID_TIPOLOGIA == x.ID_TIPOLOGIA).ORDINE)
                          .FirstOrDefault(),
                    Sintesi = x.SINTESI1,
                    Qualifica = x.SINTESI1.QUALIFICA,
                    QualStd = x.SINTESI1.QUALIFICA.TB_QUALSTD
                })
                //.OrderBy(x=>x.Pratica.DATA_CESSAZIONE).ThenBy(x=>x.Sintesi.DES_COGNOMEPERS).ThenBy(x=>x.Sintesi.DES_NOMEPERS)
                .ToList();

            List<CessazioneSaving> listSaving = null;
            int minYearSaving = 0;
            int maxYearSaving = 0;
            var querySavingParam = myRaiHelper.HrisHelper.GetParametri<string>(myRaiHelper.HrisParam.IncentiviQuerySaving);
            string querySaving = "";
            if (querySavingParam != null && querySavingParam.Any())
            {
                minYearSaving = Convert.ToInt32(querySavingParam[1]);
                maxYearSaving = Convert.ToInt32(querySavingParam[2]);
                querySaving = querySavingParam[0].Replace("__MIN_YEAR__", querySavingParam[1]).Replace("__MAX_YEAR__", querySavingParam[2]);
                try
                {
                    listSaving = db.Database.SqlQuery<CessazioneSaving>(querySaving).ToList();
                }
                catch (Exception)
                {

                }
            }

            decimal costoMax = HrisHelper.GetParametro<decimal>(HrisParam.IncentiviCostoMax);
            decimal runningTotal = 0;

            var ws = wb.AddWorksheet(String.Format("Incentivi al {0:dd.MM.yyyy}", DateTime.Today));

            int row = 1;
            ws.Cell(row, 1).SetValue("Matricola");
            ws.Cell(row, 2).SetValue("Nominativo");
            ws.Cell(row, 3).SetValue("Data cessazione");
            ws.Cell(row, 4).SetValue("Tipologia uscita");
            ws.Cell(row, 5).SetValue("Incentivo lordo");
            ws.Cell(row, 6).SetValue("Ex Fissa/Preavviso");
            ws.Cell(row, 7).SetValue("Una Tantum");
            ws.Cell(row, 8).SetValue("Oneri su Una Tantum");
            ws.Cell(row, 9).SetValue("Costo");
            ws.Cell(row, 10).SetValue("Contratto");
            ws.Cell(row, 11).SetValue("Figura");
            ws.Cell(row, 12).SetValue("Direzione");
            ws.Cell(row, 13).SetValue("Stato");
            ws.Cell(row, 14).SetValue("Progressivo");
            int lastUsedCol = 14;
            int firstSavCol = 15;
            if (cumulativo)
            {
                ws.Cell(row, 15).SetValue("Costo progressivo");
                lastUsedCol++;
                firstSavCol++;
            }

            for (int i = minYearSaving; i <= maxYearSaving; i++)
            {
                lastUsedCol++;
                ws.Cell(row, lastUsedCol).SetValue("Saving " + i.ToString());
            }


            decimal costoProgr = 0;
            foreach (var item in elenco)
            {
                row++;
                var savings = listSaving.Where(x => x.matricola_dp == item.Pratica.MATRICOLA).OrderBy(x => x.Anno);

                ws.Cell(row, 1).SetValue<string>(item.Pratica.MATRICOLA);
                ws.Cell(row, 2).SetValue<string>(item.Sintesi.Nominativo());
                ws.Cell(row, 3).SetValue<DateTime>(item.Pratica.DATA_CESSAZIONE.Value);
                string scelta = item.Pratica.NOT_TIP_ACCERT;
                if (String.IsNullOrWhiteSpace(scelta))
                    scelta = item.Pratica.NOT_TIP_SCELTA;

                string decodTipo = DecodTipologiaUscita(scelta);
                ws.Cell(row, 4).SetValue(decodTipo);

                ws.Cell(row, 5).SetValue<decimal>(item.Pratica.INCENTIVO_LORDO.GetValueOrDefault());
                ws.Cell(row, 6).SetValue<decimal>(item.Pratica.EX_FISSA.GetValueOrDefault());
                ws.Cell(row, 7).SetValue<decimal>(item.Pratica.UNA_TANTUM_LORDA.GetValueOrDefault());

                decimal unaTantum = item.Pratica.UNA_TANTUM_LORDA.GetValueOrDefault();
                decimal oneri = CessazioneHelper.CalcoloOneri(item.Sintesi.COD_QUALIFICA, unaTantum);
                ws.Cell(row, 8).SetValue(oneri);

                decimal costo = item.Pratica.INCENTIVO_LORDO.GetValueOrDefault() + item.Pratica.UNA_TANTUM_LORDA.GetValueOrDefault() + item.Pratica.EX_FISSA.GetValueOrDefault() + oneri;
                ws.Cell(row, 9).SetValue<decimal>(costo);
                ws.Cell(row, 10).SetValue<string>(savings.Select(x => x.ccl).FirstOrDefault() ?? "");
                ws.Cell(row, 11).SetValue<string>(item.QualStd.DES_QUALSTD.UpperFirst());
                ws.Cell(row, 12).SetValue<string>(item.Sintesi.DES_SERVIZIO);


                if (runningTotal + costo <= costoMax)
                {
                    runningTotal += costo;
                    switch (item.Stato.ID_STATO)
                    {
                        case (int)IncStato.RichiestaInserita:
                            ws.Cell(row, 13).SetValue("Da verificare");
                            break;
                        case (int)IncStato.RichiestaAccettata:
                            if (!item.Pratica.DATA_FIRMA_DIP.HasValue)
                                ws.Cell(row, 13).SetValue("Situazione contributiva verificata");
                            else if (!item.Pratica.DTA_RECESSO.HasValue)
                                ws.Cell(row, 13).SetValue("Proposta accettata");
                            else
                                ws.Cell(row, 13).SetValue("Recesso online");
                            break;
                        case (int)IncStato.RecessoEffettuato:
                            ws.Cell(row, 13).SetValue("Recesso online");
                            break;
                        default:
                            ws.Cell(row, 13).SetValue(item.Stato.DESCRIZIONE);
                            break;
                    }
                }
                else
                {
                    runningTotal += costo;


                    switch (item.Stato.ID_STATO)
                    {
                        case (int)IncStato.RichiestaInserita:
                            ws.Cell(row, 13).SetValue("In attesa");
                            break;
                        case (int)IncStato.RichiestaAccettata:
                            if (!item.Pratica.DATA_FIRMA_DIP.HasValue)
                                ws.Cell(row, 13).SetValue("Situazione contributiva verificata");
                            else if (!item.Pratica.DTA_RECESSO.HasValue)
                                ws.Cell(row, 13).SetValue("Proposta accettata");
                            else
                                ws.Cell(row, 13).SetValue("Recesso online");
                            break;
                        case (int)IncStato.RecessoEffettuato:
                            ws.Cell(row, 13).SetValue("Recesso online");
                            break;
                        default:
                            ws.Cell(row, 13).SetValue(item.Stato.DESCRIZIONE);
                            break;
                    }
                }


                ws.Cell(row, 14).SetValue<int>(item.Pratica.PROGR_RICHIESTA.GetValueOrDefault());
                if (cumulativo)
                {
                    if (!item.Stato.IND_INACTIVE_STATE.GetValueOrDefault())
                        costoProgr += costo;
                    ws.Cell(row, 15).SetValue(costoProgr);
                    ws.Cell(row, 15).Style.NumberFormat.Format = "#,##0.00 €";
                }

                int i = firstSavCol;
                foreach (var save in savings)
                {
                    ws.Cell(row, i).SetValue(save.Risparmio);
                    i++;
                }
                //if (item.Pratica.DATA_INVIO_PROP.HasValue)
                //    ws.Cell(row, 8).SetValue<DateTime>(item.Pratica.DATA_INVIO_PROP.Value);
                //if (item.Pratica.DATA_FIRMA_DIP.HasValue)
                //    ws.Cell(row, 9).SetValue<DateTime>(item.Pratica.DATA_FIRMA_DIP.Value);
                //if (item.Pratica.DTA_RECESSO.HasValue)
                //    ws.Cell(row, 10).SetValue<DateTime>(item.Pratica.DTA_RECESSO.Value);
            }

            ws.Range(2, 5, row, 9).Style.NumberFormat.Format = "#,##0.00 €";
            ws.Range(2, firstSavCol, row, lastUsedCol).Style.NumberFormat.Format = "#,##0.00 €";

            var table = ws.Range(1, 1, row, lastUsedCol).CreateTable();

            ws.Columns().AdjustToContents();

            ms = new MemoryStream();
            wb.SaveAs(ms);
            ms.Position = 0;

            return ms;
        }
        
        public static CessazioneDati GetDatiHRDW(string matricola)
        {
            CessazioneDati dati = null;

            string query = " select t0.[matricola_dp] " +
                            "       ,t0.[data_cessazione] " +
                            "       ,substring(t1.[CODICINI], 3, 1) as voce_te " +
                            "       ,t2.[cod_mansione] " +
                            "       ,t2.[desc_mansione] " +
                            "       ,t11.[desc_macro_categoria] " +
                            "       ,t11.[cod_categoria] " +
                            "       ,t11.[desc_categoria] " +
                            "       ,t3.[desc_livello] " +
                            "       ,t11.[cod_liv_professionale] " +
                            "       ,t11.[desc_liv_professionale] " +
                            "       ,importi.[tot_retrib_annua] " +
                            "       ,t11.[ccl] " +
                            " FROM[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0 " +
                            " INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_TE] t1 ON(t1.[SKY_MATRICOLA] = t0.[sky_anagrafica_unica]) " +
                            " INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_MANSIONE] t2 on(t2.[sky_mansione] = t1.[sky_mansione]) " +
                            " INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_CATEGORIA_LIVELLO] t3 ON(t3.[sky_livello_categ] = t1.[sky_livello_categ]) " +
                            " INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_CATEGORIA] t11 on(t1.sky_categoria = t11.sky_categoria) " +
                            " INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_INSEDIAMENTO] t13 on(t0.cod_insediamento_ubicazione = t13.cod_insediamento) " +
                            " INNER JOIN[LINK_HRDW_LIV2].[HR_LIV2].[dbo].L2F_TE_IMPORTI importi ON t1.sky_riga_te = importi.sky_riga_te " +
                            " where t1.[flg_ultimo_record]= '$' and matricola_dp = '" + matricola + "'";

            var db = new IncentiviEntities();
            try
            {
                dati = db.Database.SqlQuery<CessazioneDati>(query).FirstOrDefault();
            }
            catch (Exception)
            {

            }

            if (dati == null)
                dati = new CessazioneDati();

            return dati;
        }

        public static List<XR_WKF_TIPOLOGIA> GetTipologie(IncentiviEntities db = null)
        {
            List<XR_WKF_TIPOLOGIA> result = new List<XR_WKF_TIPOLOGIA>();

            if (db == null)
                db = new IncentiviEntities();

            result.AddRange(db.XR_WKF_TIPOLOGIA.Where(x => x.COD_PREFIX_TABELLA == "INC"));

            return result;
        }

        public static void RimuoviModuloRichiesta(XR_INC_DIPENDENTI dip, string _oper, string _nota)
        {
            if (!String.IsNullOrWhiteSpace(dip.COD_GRUPPO))
            {
                var dbTal = new myRaiDataTalentia.TalentiaEntities();
                var modulo = dbTal.XR_MOD_DIPENDENTI.FirstOrDefault(x => x.ID_PERSONA == dip.ID_PERSONA && x.MATRICOLA == dip.MATRICOLA && x.COD_MODULO == dip.COD_GRUPPO);
                if (modulo != null)
                {
                    modulo.MATRICOLA = "$" + modulo.MATRICOLA + "$";

                    string nota = "";
                    if (_oper == "d")
                        nota += "Richiesta decaduta";
                    else if (_oper == "a")
                        nota += "Richiesta annullata";
                    else if (_oper == "r")
                        nota += "Richiesta rifiutata";
                    else if (_oper == "pr")
                        nota += "Proposta rifiutata";

                    nota += " - " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");

                    nota += "\r\nMatricola " + CommonHelper.GetCurrentUserMatricola();
                    if (!String.IsNullOrWhiteSpace(_nota))
                        nota += "\r\nNota:\r\n" + _nota;

                    modulo.NOT_NOTA = nota;

                    DBHelper.Save(dbTal, CommonHelper.GetCurrentUserMatricola(), "Cessazione");
                }
            }
        }
    }

    public class IncentiviBlockInfo : RenderableItem
    {
        public List<Tuple<string, string>> rows { get; set; }

        private PdfPTable CreateTable(Font keyFont, Font valueFont)
        {
            PdfPTable t = new PdfPTable(4);
            float delta = PageSize.A4.Width - this.IndentationLeft - 48;
            t.SetTotalWidth(new float[] { this.IndentationLeft + 48, delta / 3, delta / 3 * 2, 48f });
            t.LockedWidth = true;

            foreach (var item in rows)
            {
                t.AddCell(new PdfPCell() { Border = 0 });

                PdfPCell cell = new PdfPCell()
                {
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    VerticalAlignment = Element.ALIGN_TOP,
                    Border = 0
                };
                cell.AddElement(new Phrase(item.Item1, keyFont));
                t.AddCell(cell);

                cell = new PdfPCell()
                {
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    VerticalAlignment = Element.ALIGN_TOP,
                    Border = 0
                };
                cell.AddElement(new Phrase(item.Item2, valueFont));
                t.AddCell(cell);

                t.AddCell(new PdfPCell() { Border = 0 });
            }
            return t;
        }

        public override void Render(ColumnText ct)
        {
            ct.AddElement(CreateTable(new FontManager("", BaseColor.BLACK).Normal, new FontManager("", BaseColor.BLACK).Normal));
        }

        public virtual void Render(ColumnText ct, Font keyFont, Font valueFont)
        {
            ct.AddElement(CreateTable(keyFont, valueFont));
        }

        public virtual void Render(Document document, Font keyFont, Font valueFont)
        {
            document.Add(CreateTable(keyFont, valueFont));
        }

        public override void Render(Document document)
        {
            document.Add(CreateTable(new FontManager("", BaseColor.BLACK).Normal, new FontManager("", BaseColor.BLACK).Normal));
        }
    }

    class PdfPrinter
    {
        const int BORDER_NONE = 0;
        const int BORDER_ALL = Rectangle.LEFT_BORDER | Rectangle.TOP_BORDER | Rectangle.RIGHT_BORDER | Rectangle.BOTTOM_BORDER;

        const int fontSize = 10;
        const int fontTitleSize = 10;

        int textAlign = PdfPCell.ALIGN_LEFT;
        int centerAlign = PdfPCell.ALIGN_CENTER;
        int rightAlign = PdfPCell.ALIGN_RIGHT;

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

        private Document _document;
        private PdfWriter _writer;
        private MemoryStream _ms;
        private string _imagePath;
        private string _imageInt;
        private string _title;

        private int _maxYPage = 100;
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

        public bool Apri(int pageStart = 0, string nominativo = "", string dataCessazione = "")
        {
            bool isOpened = false;
            try
            {
                _ms = new MemoryStream();
                _document = new Document(PageSize.A4, 0f, 0f, 0f, 0f);
                _writer = PdfWriter.GetInstance(_document, _ms);
                _writer.PageEvent = new ITextEvents(_imagePath, _imageInt, _title, nominativo, dataCessazione, pageStart);
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



        public int GetAlign(string align)
        {
            int intAlign = textAlign;
            switch (align)
            {
                case "L":
                    intAlign = textAlign;
                    break;
                case "C":
                    intAlign = centerAlign;
                    break;
                case "R":
                    intAlign = rightAlign;
                    break;
            }
            return intAlign;
        }
        public bool AggiungiTableAnzianita(string respLine)
        {
            bool isAdded = false;

            int currentY = 800;
            int lStartX = _startX;

            PdfContentByte cb = _writer.DirectContent;

            currentY = ((ITextEvents)_writer.PageEvent).CurrentY;

            if (currentY < _maxYPage)
            {
                _document.NewPage();
                currentY = ((ITextEvents)_writer.PageEvent).CurrentY;
            }

            string[] elems = respLine.Split(':');
            string dicitura = elems[0].Substring(0, elems[0].Length - 12);
            string data = elems[0].Replace(dicitura, "");
            string[] elemsAnz = elems[2].Trim().Replace("  ", " ").Split(' ');
            int cellCount = elems.Count();

            PdfPTable tableDetail = new PdfPTable(6);
            tableDetail.DefaultCell.BorderWidth = 1;
            tableDetail.TotalWidth = _document.PageSize.Width - lStartX * 2;
            tableDetail.LockedWidth = true;
            int[] tableDetailWidth = new int[] { 240, 75, 75, 25, 25, 25 };
            tableDetail.SetWidths(tableDetailWidth);

            tableDetail.AddCell(WriteCell(AdaptTextCell(dicitura), elems[0].Trim() == "" ? BORDER_NONE : BORDER_ALL, 1, textAlign, myFont, false));
            tableDetail.AddCell(WriteCell(AdaptTextCell(data.Trim()), elems[0].Trim() == "" ? BORDER_NONE : BORDER_ALL, 1, centerAlign, myFont, false));
            tableDetail.AddCell(WriteCell(AdaptTextCell(elems[1].Trim()), elems[0].Trim() == "" ? BORDER_NONE : BORDER_ALL, 1, centerAlign, myFont, false));


            string anz = elems[2].Substring(2, 8);
            tableDetail.AddCell(WriteCell(AdaptTextCell(CheckColAnz(anz.Substring(0, 2))), BORDER_ALL, 1, centerAlign, myFont, false));
            tableDetail.AddCell(WriteCell(AdaptTextCell(CheckColAnz(anz.Substring(3, 2))), BORDER_ALL, 1, centerAlign, myFont, false));
            tableDetail.AddCell(WriteCell(AdaptTextCell(CheckColAnz(anz.Substring(6, 2))), BORDER_ALL, 1, centerAlign, myFont, false));
            //string toPrint = i<elemsAnz.Length?!String.IsNullOrWhiteSpace(elemsAnz[i]) ? elemsAnz[i] : "0" : "0";
            //tableDetail.AddCell(WriteCell(AdaptTextCell(toPrint), BORDER_ALL, 1, centerAlign, myFont, false));    


            tableDetail.WriteSelectedRows(0, (tableDetail.Rows.Count + 1), lStartX, currentY, cb);

            ((ITextEvents)_writer.PageEvent).CurrentY -= (int)tableDetail.CalculateHeights();

            return isAdded;
        }
        private string CheckColAnz(string input)
        {
            string output = "";
            if (String.IsNullOrWhiteSpace(input))
                output = "0";
            else
                output = input;
            return output;
        }

        public bool AggiungiTablePagamenti(string respLine, string alignment, bool isHeader, string headerString, string headerAlign, bool provvistaSint = false)
        {
            bool isAdded = false;

            int currentY = 800;
            int lStartX = _startX;

            PdfContentByte cb = _writer.DirectContent;

            currentY = ((ITextEvents)_writer.PageEvent).CurrentY;

            if (currentY < _maxYPage)
            {
                _document.NewPage();
                if (!isHeader)
                    AggiungiTablePagamenti(headerString, headerAlign, true, null, null);

                currentY = ((ITextEvents)_writer.PageEvent).CurrentY;
            }

            string[] elems = respLine.Split(':');
            string[] elemsAlign = alignment.Split(':');
            int cellCount = elems.Count();

            PdfPTable tableDetail = new PdfPTable(4);
            tableDetail.DefaultCell.BorderWidth = 1;

            tableDetail.LockedWidth = true;
            int[] tableDetailWidth = null;
            if (provvistaSint)
            {
                tableDetailWidth = new int[] { 75, 75, 75, 75 };
                tableDetail.TotalWidth = 300;
            }
            else
            {
                tableDetailWidth = new int[] { 240, 75, 75, 75 };
                tableDetail.TotalWidth = _document.PageSize.Width - lStartX * 2;
            }
            tableDetail.SetWidths(tableDetailWidth);

            if (isHeader)
            {
                tableDetail.AddCell(WriteCell(elems[0].Trim(), BORDER_ALL, cellCount > 3 ? 1 : 2, GetAlign(elemsAlign[0]), myFontBold, true));
                tableDetail.AddCell(WriteCell(elems[1].Trim(), BORDER_ALL, 1, GetAlign(elemsAlign[1]), myFontBold, true));
                tableDetail.AddCell(WriteCell(elems[2].Trim(), BORDER_ALL, 1, GetAlign(elemsAlign[2]), myFontBold, true));
                if (cellCount > 3)
                    tableDetail.AddCell(WriteCell(elems[3].Trim(), BORDER_ALL, 1, GetAlign(elemsAlign[3]), myFontBold, true));
            }
            else
            {
                tableDetail.AddCell(WriteCell(elems[0], BORDER_ALL, cellCount > 3 ? 1 : 2, GetAlign(elemsAlign[0]), myFont, elems[0].All(x => x == '-')));
                tableDetail.AddCell(WriteCell(elems[1], BORDER_ALL, 1, GetAlign(elemsAlign[1]), myFont, elems[1].All(x => x == '-')));
                tableDetail.AddCell(WriteCell(elems[2], BORDER_ALL, 1, GetAlign(elemsAlign[2]), myFont, elems[2].All(x => x == '-')));
                if (cellCount > 3)
                    tableDetail.AddCell(WriteCell(elems[3], BORDER_ALL, 1, GetAlign(elemsAlign[3]), myFont, elems[3].All(x => x == '-')));
            }

            tableDetail.WriteSelectedRows(0, (tableDetail.Rows.Count + 1), lStartX, currentY, cb);

            ((ITextEvents)_writer.PageEvent).CurrentY -= (int)tableDetail.CalculateHeights();

            return isAdded;
        }
        public bool AggiungiTable(string respLine, bool isHeader)
        {
            bool isAdded = false;

            int currentY = 800;
            int lStartX = 45;

            PdfContentByte cb = _writer.DirectContent;

            currentY = ((ITextEvents)_writer.PageEvent).CurrentY;

            if (currentY < _maxYPage)
            {
                _document.NewPage();
                currentY = ((ITextEvents)_writer.PageEvent).CurrentY;
            }

            string[] elems = respLine.Split(':');
            int cellCount = elems.Count();

            PdfPTable tableDetail = new PdfPTable(4);
            tableDetail.DefaultCell.BorderWidth = 1;
            tableDetail.TotalWidth = _document.PageSize.Width - lStartX * 2;
            tableDetail.LockedWidth = true;
            int[] tableDetailWidth = new int[] { 240, 75, 75, 75 };
            tableDetail.SetWidths(tableDetailWidth);

            if (isHeader)
            {
                tableDetail.AddCell(WriteCell(AdaptTextCell(elems[0]).Trim(), BORDER_ALL, cellCount == 5 ? 1 : 2, textAlign, myFontBold, true));
                tableDetail.AddCell(WriteCell(AdaptTextCell(elems[1]).Trim(), BORDER_ALL, 1, centerAlign, myFontBold, true));
                tableDetail.AddCell(WriteCell(AdaptTextCell(elems[2]).Trim(), BORDER_ALL, 1, centerAlign, myFontBold, true));
                if (cellCount == 5)
                    tableDetail.AddCell(WriteCell(AdaptTextCell(elems[3]).Trim(), BORDER_ALL, 1, centerAlign, myFontBold, true));
            }
            else
            {
                tableDetail.AddCell(WriteCell(AdaptTextCell(elems[0]), BORDER_ALL, cellCount == 5 ? 1 : 2, textAlign, myFont, elems[0].All(x => x == '-')));
                tableDetail.AddCell(WriteCell(AdaptTextCell(elems[1]), BORDER_ALL, 1, rightAlign, myFont, elems[1].All(x => x == '-')));
                tableDetail.AddCell(WriteCell(AdaptTextCell(elems[2]), BORDER_ALL, 1, rightAlign, myFont, elems[2].All(x => x == '-')));
                if (cellCount == 5)
                    tableDetail.AddCell(WriteCell(AdaptTextCell(elems[3]), BORDER_ALL, 1, rightAlign, myFont, elems[3].All(x => x == '-')));
            }

            tableDetail.WriteSelectedRows(0, (tableDetail.Rows.Count + 1), lStartX, currentY, cb);

            ((ITextEvents)_writer.PageEvent).CurrentY -= (int)tableDetail.CalculateHeights();

            return isAdded;
        }
        private string AdaptTextCell(string inputText)
        {
            string result = inputText;

            if (inputText.All(x => x == '-'))
                result = " ";
            else
                result = result.TrimEnd();

            if (result.Trim().Length > 0)
            {
                result = result.ToLower();
                string tmpLower = result.Trim();
                if (tmpLower == "aa" || tmpLower == "mm" || tmpLower == "gg")
                {
                    result = result.ToUpper();
                }
                else
                {
                    tmpLower = tmpLower.Replace("tfr", "TFR");
                    string tmpUpperFirst = char.ToUpper(tmpLower[0]) + tmpLower.Substring(1);
                    result = result.Replace(tmpLower, tmpUpperFirst);
                    result = result.Replace("totale tfr e altre indennita", "totale tfr e altre indennita di liquidazione");
                    result = result.Replace("tfr", "TFR");
                }
            }



            return result;
        }
        internal static PdfPCell WriteCell(string text, int border, int colspan, int textAlign, iTextSharp.text.Font f, bool greyBackground = false)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, f));
            cell.FixedHeight = 16f;
            cell.Border = border;
            cell.HorizontalAlignment = textAlign; //0=Left, 1=Centre, 2=Right
            cell.VerticalAlignment = PdfPCell.ALIGN_TOP;
            cell.Colspan = colspan;
            if (greyBackground)
                cell.BackgroundColor = new BaseColor(239, 239, 239);
            return cell;
        }

        internal void WriteLine(string p, bool isTitle = false)
        {
            int currentY = 800;
            int lStartX = _startX;

            PdfContentByte cb = _writer.DirectContent;

            currentY = ((ITextEvents)_writer.PageEvent).CurrentY;

            if (currentY < _maxYPage)
            {
                _document.NewPage();
                currentY = ((ITextEvents)_writer.PageEvent).CurrentY;
            }

            PdfPTable tableDetail = new PdfPTable(1);
            tableDetail.DefaultCell.BorderWidth = 1;
            tableDetail.TotalWidth = _document.PageSize.Width - lStartX * 2;
            tableDetail.LockedWidth = true;
            int[] tableDetailWidth = new int[] { 500 };
            tableDetail.SetWidths(tableDetailWidth);

            tableDetail.AddCell(WriteCell(p, BORDER_NONE, 1, textAlign, isTitle ? myFontTitle : myFont, false));

            tableDetail.WriteSelectedRows(0, (tableDetail.Rows.Count + 1), lStartX, currentY, cb);

            ((ITextEvents)_writer.PageEvent).CurrentY -= (int)tableDetail.CalculateHeights();
        }

        internal void AggiungiTableRedditoRif(string row, string p1, string p2)
        {
            int currentY = 800;
            int lStartX = _startX;

            PdfContentByte cb = _writer.DirectContent;

            currentY = ((ITextEvents)_writer.PageEvent).CurrentY;

            if (currentY < _maxYPage)
            {
                _document.NewPage();
                currentY = ((ITextEvents)_writer.PageEvent).CurrentY;
            }

            PdfPTable tableDetail = new PdfPTable(4);
            tableDetail.DefaultCell.BorderWidth = 1;
            tableDetail.TotalWidth = _document.PageSize.Width - lStartX * 2;
            tableDetail.LockedWidth = true;
            int[] tableDetailWidth = new int[] { 150, 80, 50, 200 };
            tableDetail.SetWidths(tableDetailWidth);

            tableDetail.AddCell(WriteCell("", BORDER_NONE, 1, textAlign, myFont, false));
            tableDetail.AddCell(WriteCell(AdaptTextCell(row.Trim()), BORDER_NONE, 1, centerAlign, myFont, false));
            tableDetail.AddCell(WriteCell(AdaptTextCell(""), BORDER_NONE, 1, rightAlign, myFont, false));
            tableDetail.AddCell(WriteCell(AdaptTextCell(""), BORDER_NONE, 1, rightAlign, myFont, false));

            tableDetail.AddCell(WriteCell("REDDITO DI RIFERIMENTO", BORDER_NONE, 1, textAlign, myFont, false));
            tableDetail.AddCell(WriteCell(AdaptTextCell("------------------ "), BORDER_NONE, 1, centerAlign, myFont, false));
            tableDetail.AddCell(WriteCell(AdaptTextCell("X 12 ="), BORDER_NONE, 1, textAlign, myFont, false));
            tableDetail.AddCell(WriteCell(AdaptTextCell(p1.Trim().Substring(p1.Trim().IndexOf('=') + 1)), BORDER_NONE, 1, textAlign, myFont, false));

            tableDetail.AddCell(WriteCell("", BORDER_NONE, 1, textAlign, myFont, false));
            tableDetail.AddCell(WriteCell(AdaptTextCell(p2.Trim()), BORDER_NONE, 1, centerAlign, myFont, false));
            tableDetail.AddCell(WriteCell(AdaptTextCell(""), BORDER_NONE, 1, textAlign, myFont, false));
            tableDetail.AddCell(WriteCell(AdaptTextCell(""), BORDER_NONE, 1, rightAlign, myFont, false));

            tableDetail.WriteSelectedRows(0, (tableDetail.Rows.Count + 1), lStartX, currentY, cb);

            ((ITextEvents)_writer.PageEvent).CurrentY -= (int)tableDetail.CalculateHeights();
        }


        internal void AggiungiFirma(iTextSharp.text.Image png)
        {
            int currentY = 800;
            int lStartX = _startX;

            PdfContentByte cb = _writer.DirectContent;

            currentY = ((ITextEvents)_writer.PageEvent).CurrentY;

            if (currentY < _maxYPage)
            {
                currentY = _maxYPage + 10;
            }

            Font firmaFont = new FontManager("", new BaseColor(0, 0, 153)).Normal;

            PdfPTable table = new PdfPTable(3);
            table.DefaultCell.BorderWidth = 1;
            table.TotalWidth = _document.PageSize.Width - lStartX * 2;
            table.LockedWidth = true;
            int[] widths = new int[] { 310, 160, 30 };
            table.SetWidths(widths);

            table.AddCell(WriteCell("", BORDER_NONE, 3, textAlign, myFont, false));
            //table.AddCell(WriteCell("", BORDER_NONE, 3, textAlign, myFont, false));

            table.AddCell(WriteCell("", BORDER_NONE, 1, centerAlign, myFont, false));
            table.AddCell(WriteCell("RAI RADIOTELEVISIONE ITALIANA", BORDER_NONE, 1, centerAlign, firmaFont, false));
            table.AddCell(WriteCell("", BORDER_NONE, 1, centerAlign, myFont, false));

            table.AddCell(WriteCell("", BORDER_NONE, 1, centerAlign, myFont, false));
            table.AddCell(WriteCell("Risorse Umane e Organizzazione", BORDER_NONE, 1, centerAlign, firmaFont, false));
            table.AddCell(WriteCell("", BORDER_NONE, 1, centerAlign, myFont, false));

            table.AddCell(WriteCell("", BORDER_NONE, 1, centerAlign, myFont, false));
            table.AddCell(WriteCell("Amministrazione", BORDER_NONE, 1, centerAlign, firmaFont, false));
            table.AddCell(WriteCell("", BORDER_NONE, 1, centerAlign, myFont, false));

            table.AddCell(WriteCell("", BORDER_NONE, 1, centerAlign, myFont, false));
            table.AddCell(WriteCell("(Barbara Borghese)", BORDER_NONE, 1, centerAlign, firmaFont, false));
            table.AddCell(WriteCell("", BORDER_NONE, 1, centerAlign, myFont, false));

            int tableHeight = (int)table.CalculateHeights();
            table.WriteSelectedRows(0, (table.Rows.Count + 1), lStartX, currentY, cb);
            ((ITextEvents)_writer.PageEvent).CurrentY -= (int)table.CalculateHeights();

            table = new PdfPTable(2);
            table.DefaultCell.BorderWidth = 1;
            table.TotalWidth = _document.PageSize.Width - lStartX * 2;
            table.LockedWidth = true;
            widths = new int[] { 470, 30 };
            table.SetWidths(widths);

            table.AddCell(WriteCell("", BORDER_NONE, 2, textAlign, myFont, false));
            table.AddCell(WriteCell("", BORDER_NONE, 2, textAlign, myFont, false));
            //table.AddCell(WriteCell("", BORDER_NONE, 2, textAlign, myFont, false));

            PdfPCell cellImage = new PdfPCell(png);
            cellImage.Border = BORDER_NONE;
            //cellImage.Rowspan = 3;
            cellImage.HorizontalAlignment = rightAlign;
            table.AddCell(cellImage);

            table.AddCell(WriteCell("", BORDER_NONE, 1, textAlign, myFont, false));
            table.AddCell(WriteCell("", BORDER_NONE, 1, textAlign, myFont, false));

            table.WriteSelectedRows(0, (table.Rows.Count + 1), lStartX, currentY, cb);
            ((ITextEvents)_writer.PageEvent).CurrentY -= (int)table.CalculateHeights();

        }
    }

    class ITextEvents : PdfPageEventHelper
    {
        string _imgPath;
        string _title;
        int _pageStart;
        string _nominativo;
        string _dataCessazione;
        string _imgInfo;

        iTextSharp.text.Image _pngInfo = null;
        iTextSharp.text.Image _pngImage = null;

        public ITextEvents(string imgPath = "", string imgInfo = "", string title = "", string nominativo = "", string dataCessazione = "", int pageStart = 0)
        {
            this._imgPath = imgPath;
            this._title = title;
            this._pageStart = pageStart;
            this._nominativo = nominativo;
            this._dataCessazione = dataCessazione;
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


        int currentY = 800;
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
            int _currentY = 800;
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

            if (!String.IsNullOrWhiteSpace(_nominativo))
            {
                //cell = new PdfPCell(new Paragraph(_nominativo.ToUpper(), new FontManager().H1));
                cell = new PdfPCell(new Paragraph(_nominativo.ToUpper(), new FontManager().H2));
                cell.Border = PdfPCell.NO_BORDER;
                cell.Colspan = 2;// drawImage ? 1 : 2;
                table.AddCell(cell);
            }


            if (!String.IsNullOrWhiteSpace(_dataCessazione))
            {
                //cell = new PdfPCell(new Paragraph("Data cessazione: " + _dataCessazione, new FontManager().H3));
                cell = new PdfPCell(new Paragraph("Data cessazione: " + _dataCessazione, new FontManager().H4));
                cell.Border = PdfPCell.NO_BORDER;
                cell.Colspan = 2; // drawImage ? 1 : 2;
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

                document.AddKeywords("PDF report ordine");

                document.AddSubject("Prospetto");

                document.AddTitle(String.Format(_title));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            cb = writer.DirectContent;
            footerTemplate = cb.CreateTemplate(50, 50);
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            base.OnEndPage(writer, document);
            if (!String.IsNullOrWhiteSpace(_imgInfo))
            {
                AggiungiInfoSocieta(writer, document);
                return;
            }
            //return;

            //Image image = Image.GetInstance(Cons.RAI_ICON);
            //image.ScaleAbsolute(24, 24);
            //image.SetAbsolutePosition(PageSize.A4.Width - 24, PageSize.A4.Height - 24);
            //cb.AddImage(image);

            int pageN = writer.PageNumber + _pageStart;

            //var rect = new iTextSharp.text.Rectangle(0, 0, 166, PageSize.A4.Height);
            //rect.BorderWidth = 0;
            //rect.BackgroundColor = new BaseColor(229, 229, 229, 50);
            //cb.Rectangle(rect);

            PdfShading shading = PdfShading.SimpleAxial(writer, PageSize.A4.Width - 24f, 700, PageSize.A4.Width, 700, new BaseColor(0, 0, 153), new BaseColor(0, 0, 153)); //new BaseColor(180, 237, 80), new BaseColor(180, 237, 80));
            PdfShadingPattern pattern = new PdfShadingPattern(shading);
            ShadingColor color = new ShadingColor(pattern);

            PdfPTable t = new PdfPTable(3);
            t.SetTotalWidth(new float[] { 24, PageSize.A4.Width - 48, 24 });

            FontManager fm = new FontManager(HttpContext.Current.Server.MapPath("~/FONTS/opensans.ttf"), new BaseColor(255, 255, 255));

            t.LockedWidth = true;
            t.AddCell(new PdfPCell() { Border = 0 });
            t.AddCell(new PdfPCell() { FixedHeight = 24f, Border = 0, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
            t.AddCell(new PdfPCell(new Phrase(string.Format("{0}", pageN), fm.N12)) { FixedHeight = 24f, BackgroundColor = color, Border = 0, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });
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

        private void AggiungiInfoSocieta(PdfWriter writer, Document document)
        {
            int currentY = 80;
            int lStartX = 45;

            PdfContentByte cb = writer.DirectContent;

            PdfPTable table = new PdfPTable(1);
            table.DefaultCell.BorderWidth = 1;
            table.TotalWidth = document.PageSize.Width - lStartX * 2;
            table.LockedWidth = true;
            int[] widths = new int[] { 500 };
            table.SetWidths(widths);

            PdfPCell cellImage = new PdfPCell(_pngInfo);
            cellImage.Border = PdfPCell.NO_BORDER;
            //cellImage.Rowspan = 3;
            cellImage.HorizontalAlignment = PdfPCell.ALIGN_LEFT;
            table.AddCell(cellImage);

            table.WriteSelectedRows(0, (table.Rows.Count + 1), lStartX, currentY, cb);
            ((ITextEvents)writer.PageEvent).CurrentY -= (int)table.CalculateHeights();
        }

        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            base.OnCloseDocument(writer, document);

            footerTemplate.BeginText();
            footerTemplate.SetFontAndSize(bf, fontSize);
            footerTemplate.SetTextMatrix(0, 0);
            footerTemplate.ShowText("" + (writer.PageNumber));
            footerTemplate.EndText();
        }
    }
}