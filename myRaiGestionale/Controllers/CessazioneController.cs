using ClosedXML.Excel;
using myRai.DataAccess;
using myRaiHelper;
using myRaiCommonManager.Cessazione;
using myRaiCommonModel.Gestionale;
using myRaiCommonModel.SituazioneDebitoria.ESS;
using myRaiCommonTasks.sendMail;
using myRaiData;
using myRaiData.Incentivi;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using myRaiCommonModel.cvModels.Pdf;
using myRaiCommonTasks;
using System.ServiceModel;
using myRaiCommonManager;
using myRaiCommonTasks.Helpers;
using myRaiCommonModel;
using myRaiHelper.Task;
using myRaiServiceHub.it.rai.servizi.raiconnectcoll;
using iTextSharp.text.pdf;
using System.Xml;
using System.Xml.Serialization;
using System.Globalization;

namespace myRaiGestionale.Controllers
{
    public class CessazioneController : BaseCommonController
    {
        private static string _urlParam = "";

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.ActionDescriptor.ActionName != "SelezioneDipendenti")
            {
                if (!CessazioneHelper.EnabledToIncentivi(CommonHelper.GetCurrentUserMatricola()))
                {
                    filterContext.Result = new RedirectResult("/Home/notAuth");
                    return;
                }

                SessionHelper.Set("GEST_SECTION", "GESTIONE");
            }

            base.OnActionExecuting(filterContext);
        }

        public ActionResult Correzione()
        {
            if (!System.Diagnostics.Debugger.IsAttached)
                return Content("Not auth");

            //var db = new IncentiviEntities();
            //var isRecesso = CessazioneHelper.IsCurrentState((int)IncStato.RecessoEffettuato);
            //var notProp = CessazioneHelper.NotIsAnyState((int)IncStato.FileProposta);
            //var list = db.XR_INC_DIPENDENTI
            //                .Where(x => x.COD_GRUPPO == "INCENTIVAZIONE012022")
            //                .Where(isRecesso)
            //                .Where(x => !x.XR_INC_OPERSTATI.Any(y => y.ID_STATO == (int)IncStato.FileProposta && !y.DATA_FINE_VALIDITA.HasValue)
            //                            || !x.XR_INC_OPERSTATI.Any(y => y.ID_STATO == (int)IncStato.FileProposta && !y.DATA_FINE_VALIDITA.HasValue && y.XR_INC_OPERSTATI_DOC.Any(z => !z.VALID_DTA_END.HasValue)))
            //                .ToList();

            //foreach (var item in list)
            //{
            //    CessazioneManager.RendiAllegatiDipEffettivi(item.ID_DIPENDENTE, item.MATRICOLA, db, (int)IncStato.RichiestaAccettata, (int)IncStato.FileProposta, "Proposta");
            //}
            //db.SaveChanges();

            var db = new IncentiviEntities();
            var list = db.XR_INC_DIPENDENTI.Where(x => x.COD_GRUPPO == "INCENTIVAZIONE012022")
                            .Where(x => x.IND_CESSIONE_QUINTO_TFR != null && x.IND_CESSIONE_QUINTO_TFR.Value);

            foreach (var item in list)
            {
                CessazioneHelper.GetImportoAltreTrattenute(db, item);
            }

            db.SaveChanges();

            return Content("OK");
        }

        public ActionResult Index(string abil = "", string sedi = "", bool clearData = false, bool reloadData = false, bool updateData = false)
        {
            CessazioneControllerScope.Instance.Clear();

            bool needRedirect = false;

            if (System.Diagnostics.Debugger.IsAttached && updateData)
            {
                //CaricamentoDatiIncentivati();
                //UpdateRapprRai();
                //UpdateRapprIndu();

                needRedirect = true;
            }

            if (needRedirect)
                return RedirectToAction("Index");

            //UpdateDataPagamento();
            return View();
        }
        public PartialViewResult Widget_Ricerca()
        {
            return PartialView("subpartial/Widget_Ricerca", new CessazioneFilter());
        }

        public PartialViewResult GetElencoIncentivati(CessazioneFilter filtri = null)
        {
            CessazioneList model = new CessazioneList();
            model.AddRange(CessazioneManager.GetElenco(filtri, true, ""));
            return PartialView("~/Views/Cessazione/subpartial/elencoIncentivati.cshtml", model);
        }

        public PartialViewResult GetElencoExtra(CessazioneFilter filtri = null, string incExtra = "ANPAL", bool loadHistorySolleciti = false)
        {
            CessazioneList model = new CessazioneList();
            string[] extras = incExtra.Split(';');
            foreach (var extra in extras)
            {
                if (!String.IsNullOrWhiteSpace(extra))
                    model.AddRange(CessazioneManager.GetElenco(filtri, true, extra, null, loadHistorySolleciti));
            }

            //Questo è un caso particolare: viene richiamato in modo singolo dalla normativa
            if (incExtra == "__SOLLECITI__")
            {
                var db = new IncentiviEntities();
                var listMail = db.XR_INC_PARAM_MAIL.Where(x => x.COD_CAMPO == "RichiestaInseritaSollecito").ToList();
                foreach (var item in model)
                {
                    item.OpenFunction = "__SOLLECITI__";
                    var selectDate = listMail.Where(x => x.ID_DIPENDENTE == item.Pratica.ID_DIPENDENTE).Select(x => x.VALORE).FirstOrDefault();
                    if (!String.IsNullOrWhiteSpace(selectDate) && DateTime.TryParseExact(selectDate, "dd/MM/yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out DateTime res))
                        item.NotificaSollecito = res;
                }
            }

            model.IncExtra = incExtra;
            return PartialView("~/Views/Cessazione/subpartial/elencoIncentivati.cshtml", model);
        }

        [HttpPost]
        public PartialViewResult Modal_Extra(string extra)
        {
            return PartialView("subpartial/Modal_Anpal", extra);
        }

        [HttpGet]
        public PartialViewResult GetIncentivatiCount()
        {
            var tmp = CessazioneManager.GetElenco(null, false, "", null, false, false);
            return PartialView("~/Views/Cessazione/subpartial/Widget_avanzamento.cshtml", tmp);
        }

        [HttpGet]
        public PartialViewResult GetCalendarioAppuntamenti()
        {
            var isAppuntamentoOrConteggio = CessazioneHelper.IsCurrentState((int)IncStato.Appuntamento, (int)IncStato.Conteggio);
            var extraFilter = LinqHelper.PutInAndTogether(isAppuntamentoOrConteggio, x => x.DATA_APPUNTAMENTO.HasValue);
            var incentivati = CessazioneManager.GetElenco(null, true, "", extraFilter, false, false);

            return PartialView("~/Views/Cessazione/subpartial/Widget_calendario.cshtml", incentivati);
        }

        [HttpPost]
        public ActionResult GetDettaglioIncentivato(int idDip, string openFunction = "")
        {
            string matricola = CommonHelper.GetCurrentUserMatricola();
            if (!CessazioneHelper.EnabledToIncentivi(matricola))
                throw new Exception("UNAUTHORIZED");

            string codGruppo = HrisHelper.GetParametro<string>(HrisParam.IncentiviRifGruppo);
            IncentiviEntities db = new IncentiviEntities();

            CessazioneModel model = new CessazioneModel();
            model.OpenFunction = openFunction;
            model.AbilFunctions = new List<string>();
            model.AbilFunctions.AddRange(CessazioneHelper.EnabledSubFunc(matricola));
            model.AbilFunctions.AddRange(AuthHelper.EnabledSubFunc(matricola, CessazioneHelper.INCENTIVI_INC_EXTRA));

            model.DictAllegati = new Dictionary<int, List<XR_INC_OPERSTATI_DOC>>();

            var inc = db.XR_INC_DIPENDENTI.FirstOrDefault(x => x.ID_DIPENDENTE == idDip);
            model.Pratica = inc;
            model.Stato = CessazioneHelper.GetPraticaStato(inc);
            model.OperStato = CessazioneHelper.GetPraticaOperStato(inc);
            model.Tipologia = inc.XR_WKF_TIPOLOGIA;
            model.InCarico = inc.XR_INC_OPERSTATI.Where(x => x.ID_STATO == (int)IncStato.InCarico).Select(x => x.SINTESI1).FirstOrDefault();
            model.Workflow = new List<XR_WKF_WORKFLOW>(inc.XR_WKF_TIPOLOGIA.XR_WKF_WORKFLOW.OrderBy(x => x.ORDINE));
            model.Sintesi = inc.SINTESI1;
            model.PercipientePensioneInvalidita = inc.IND_INVALIDITA.GetValueOrDefault() == 1;
            model.TipiVertenze = !String.IsNullOrWhiteSpace(inc.IND_TIPO_VERTENZE) ? inc.IND_TIPO_VERTENZE.Split(';') : new string[] { };

            model.IndItl = inc.IND_ITL.GetValueOrDefault();
            if (!model.Pratica.NUM_BOZZA_GIORNI.HasValue)
            {
                int limitGG = HrisHelper.GetParametro<int>(HrisParam.IncentiviLimiteBozzaVerbale);
                if (limitGG > 0)
                    model.Pratica.NUM_BOZZA_GIORNI = limitGG;
            }

            model.IsAnpal = openFunction == "ANPAL" && model.AbilFunctions.Contains("ANPAL");
            model.CanAnpal = openFunction != "ANPAL" && model.AbilFunctions.Contains("ANPAL");

            var selectDate = db.XR_INC_PARAM_MAIL.Where(x => x.COD_CAMPO == "RichiestaInseritaSollecito" && x.ID_DIPENDENTE == inc.ID_DIPENDENTE).Select(x => x.VALORE).FirstOrDefault();
            if (!String.IsNullOrWhiteSpace(selectDate) && DateTime.TryParseExact(selectDate, "dd/MM/yyyy HH:mm", null, System.Globalization.DateTimeStyles.None, out DateTime res))
                model.NotificaSollecito = res;

            myRaiCommonTasks.Helpers.FileResult files = null;

            if (model.IsAnpal)
            {
                model.ANAGPERS = inc.ANAGPERS;
                var tmpCitta = db.CITTAD.Where(x => x.ID_PERSONA == model.Pratica.ID_PERSONA && x.DTA_INIZIO <= DateTime.Today && x.DTA_FINE > DateTime.Today).FirstOrDefault();
                if (tmpCitta != null)
                    model.CITTAD = db.TB_CITTAD.FirstOrDefault(x => x.COD_CITTAD == tmpCitta.COD_CITTADPERS);
                model.HrdwData = CessazioneManager.GetDatiHRDW(inc.MATRICOLA);
                model.Studi = db.STUPERSONA.Where(x => x.ID_PERSONA == inc.ID_PERSONA).Select(x => x.TB_STUDIO.TB_LIVSTUD).OrderByDescending(x => x.COD_LIVELLOSTUDIO).FirstOrDefault();
                model.AllegatiAnpal = new List<XR_INC_OPERSTATI_DOC>();
                model.AllegatiAnpal.AddRange(InternalGetAllegati(ref files, idDip, model.Pratica.MATRICOLA, db, !model.Pratica.DATA_RECESSO_ANPAL.HasValue, false, (int)IncStato.FileAnpal));

                model.AllegatiAccett = new List<XR_INC_OPERSTATI_DOC>();
                model.AllegatiAccett.AddRange(InternalGetAllegatiByTags(ref files, idDip, model.Pratica.MATRICOLA, db, false, false, new string[] { "recesso" }, false, true, (int)IncStato.RecessoEffettuato, (int)IncStato.FileProposta, (int)IncStato.FileAccettazione));
            }
            else
            {
                List<int> enabledStates = null;
                bool applyFilterStates = CessazioneHelper.GetEnabledStates(null, CommonHelper.GetCurrentUserMatricola(), AbilOper.Writing, out enabledStates);

                var listStati = HrisHelper.GetParametro(HrisParam.IncentiviStatiVerbale);
                if (listStati != null && CessazioneHelper.EnabledToAnySubFunc(CommonHelper.GetCurrentUserMatricola(), listStati.COD_VALUE2.Split(',')))
                    enabledStates.AddRange(listStati.COD_VALUE1.Split(',').Select(x => Convert.ToInt32(x)));

                bool isInLavorazione = model.InCarico != null;
                bool isMiaPratica = model.InCarico != null && model.InCarico.ID_PERSONA == CommonHelper.GetCurrentIdPersona();
                bool inServizio = false;
                //se la pratica è in carico a qualcuno, verifico che quella persona sia in servizio
                if (isInLavorazione && !isMiaPratica)
                    inServizio = HomeManager.IsInServizio(model.InCarico.COD_MATLIBROMAT) == EnumPresenzaDip.Presente;

                bool isAbilContenzioso = model.AbilFunctions.Contains("CONTENZIOSO");
                bool isStatoVerbale = listStati != null && listStati.COD_VALUE1.Split(',').Select(x => Convert.ToInt32(x)).Contains(model.Stato.ID_STATO);
                bool isContenzioso = model.Pratica.IND_TIPO_VERTENZE != null && model.Pratica.IND_TIPO_VERTENZE != "" && model.Pratica.IND_TIPO_VERTENZE != "VERT";

                model.AbilitaAvviaPratica = model.Stato.ID_STATO == (int)IncStato.RecessoEffettuato && (!applyFilterStates || enabledStates.Contains((int)IncStato.RecessoEffettuato));
                model.AbilitaPresaInCarico = (model.Tipologia.ID_TIPOLOGIA != (int)CessazioneTipo.Incentivazione || model.Stato.ID_STATO != (int)IncStato.RecessoEffettuato)
                                                && model.Stato.IND_CAN_CHARGE.GetValueOrDefault()
                                                && (!applyFilterStates || enabledStates.Contains(model.Stato.ID_STATO))
                                                && !isMiaPratica 
                                                && (!isInLavorazione || !inServizio || (isAbilContenzioso && isStatoVerbale))
                                                && (!isStatoVerbale || isAbilContenzioso || !model.AbilFunctions.Contains("GEST") || !isContenzioso)
                                                ;

                if (CommonHelper.IsProduzione() && (model.Stato.ID_STATO == (int)IncStato.RichiestaInserita
                    || model.Stato.ID_STATO == (int)IncStato.RichiestaAccettata
                    || model.Stato.ID_STATO == (int)IncStato.RichiestaRifiutata))
                {
                    try
                    {
                        string query = "" +
                                    " SELECT  " +
                                    " 	 anag.matricola_dp " +
                                    " 	,anag.data_prima_assunzione_subordinato " +
                                    " 	,anag.data_nascita " +
                                    " 	,dateadd(year, 62, anag.data_nascita) AS data_compimento_62_anni_eta " +
                                    " 	,dateadd(year, 38, anag.data_prima_assunzione_subordinato) AS data_38_anni_anzianita_servizio " +
                                    " FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].L2D_ANAGRAFICA_UNICA anag " +
                                    " INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].L2D_CATEGORIA cat ON (anag.categoria = cat.cod_categoria) " +
                                    " WHERE " +
                                    //" 	--solo TI  " +
                                    " 	anag.cod_serv_inquadram IN ('1', '3') " +
                                    " 	AND " +
                                    " 	cat.cod_tipo_dipendente NOT IN ('G', 'DG') " +
                                    " 	AND " +
                                    " 	convert(SMALLDATETIME, {fn CURDATE() }) BETWEEN anag.data_assunzione AND anag.data_cessazione " +
                                    " 	AND " +
                                    " 	anag.matricola_dp = '" + model.Pratica.MATRICOLA + "' ";
                        model.DatiQuota100 = db.Database.SqlQuery<CessazioneHRDWData>(query).FirstOrDefault();
                    }
                    catch (Exception ex)
                    {

                    }
                }

                //Controlla se la matricola era in servizio a tempo indeterminato il 20/05/1978
                DateTime d19780520 = new DateTime(1978, 05, 20);
                model.TempoInd_19780520 = db.ASSTPCONTR.Any(x => x.ID_PERSONA == model.Pratica.ID_PERSONA && x.DTA_INIZIO <= d19780520 && d19780520 <= x.DTA_FINE && x.COD_TPCNTR == "9");

                model.HasDocPraticaPrecedente = false;
                model.Allegati = new List<XR_INC_OPERSTATI_DOC>();
                if (model.Stato.ID_STATO == (int)IncStato.RichiestaInserita)
                    model.Allegati.AddRange(InternalGetAllegati(ref files, idDip, model.Pratica.MATRICOLA, db, String.IsNullOrWhiteSpace(model.OpenFunction), false, (int)IncStato.TempFileEstratti));

                model.AllegatiCont = new List<XR_INC_OPERSTATI_DOC>();
                if (!CessazioneHelper.InactiveState().Contains(model.Stato.ID_STATO))
                    model.AllegatiCont.AddRange(InternalGetAllegati(ref files, idDip, model.Pratica.MATRICOLA, db, false, false, (int)IncStato.TempFileContabili));

                model.AllegatiProp = new List<XR_INC_OPERSTATI_DOC>();
                model.AllegatiAccett = new List<XR_INC_OPERSTATI_DOC>();
                if (model.Stato.ID_STATO == (int)IncStato.RichiestaAccettata)
                {
                    model.AllegatiProp.AddRange(InternalGetAllegati(ref files, idDip, model.Pratica.MATRICOLA, db, true, false, (int)IncStato.TempFileProposta));
                    if (model.AllegatiProp.Count() == 1)
                    {
                        model.AllegatiProp[0].COD_TITLE = "Proposta firmata dal dipendente";
                    }
                    model.AllegatiAccett.AddRange(InternalGetAllegati(ref files, idDip, model.Pratica.MATRICOLA, db, true, false, (int)IncStato.TempFileAccettazione));
                    if (model.AllegatiAccett.Count() == 1)
                    {
                        model.AllegatiAccett[0].COD_TITLE = "Accettazione firmata dal dipendente";
                    }

                    model.HasRuoloAccettazione = CessazioneHelper.GetRuoloProtocollo(CommonHelper.GetCurrentUserMatricola(), out string ruolo);
                    model.RuoloAccettazione = ruolo;
                }

                if (model.Pratica.DATA_RECESSO_ANPAL.HasValue)
                {
                    model.AllegatiAnpal = new List<XR_INC_OPERSTATI_DOC>();
                    model.AllegatiAnpal.AddRange(InternalGetAllegati(ref files, idDip, model.Pratica.MATRICOLA, db, false, false, (int)IncStato.FileAnpal));
                }

                model.DictAllegati.Add((int)IncStato.FileTessContr, InternalGetAllegati(ref files, idDip, model.Pratica.MATRICOLA, db, !model.Pratica.DTA_ARR_TESS_CONTR.HasValue, model.Pratica.DTA_ARR_TESS_CONTR.HasValue, (int)IncStato.FileTessContr));
                model.DictAllegati.Add((int)IncStato.FileMat, InternalGetAllegati(ref files, idDip, model.Pratica.MATRICOLA, db, !model.Pratica.DTA_ARR_TESS_CONTR.HasValue, model.Pratica.DTA_ARR_TESS_CONTR.HasValue, (int)IncStato.FileMat));
                model.DictAllegati.Add((int)IncStato.FileCambioQual, InternalGetAllegati(ref files, idDip, model.Pratica.MATRICOLA, db, !model.Pratica.DTA_ARR_TESS_CONTR.HasValue, model.Pratica.DTA_ARR_TESS_CONTR.HasValue, (int)IncStato.FileCambioQual));


                model.AllegatiApp = new List<XR_INC_OPERSTATI_DOC>();
                if (model.Stato.ID_STATO == (int)IncStato.Appuntamento)
                    model.AllegatiApp.AddRange(InternalGetAllegati(ref files, idDip, model.Pratica.MATRICOLA, db, true, false, (int)IncStato.Appuntamento));

                var parametriIncentivi = HrisHelper.GetParametriJson<myRaiData.Incentivi.XR_HRIS_PARAM>(HrisParam.IncentiviParametri);
                model.Scadenze.Load(parametriIncentivi, inc, model.Stato, inc.XR_INC_DIPENDENTI_FIELD);
            }

            //ExtraField
            model.NotHasTessereContr = inc.GetField("NotHasTessereContr", false);
            model.NotHasMaternita = inc.GetField("NotHasMaternita", false);
            model.NotHasCambioQualifica = inc.GetField("NotHasCambioQualifica", false);
            model.VeicoloTipologia = inc.GetField<string>("VeicoloTipologia", null);
            model.VeicoloTarga = inc.GetField<string>("VeicoloTarga", null);
            model.TfrAnte2007FPGCI = inc.GetField<bool>("TfrAnte2007FPGCI", false);

            model.AccettazioneGenerata = inc.GetField<DateTime?>("AccettazioneGenerata", null);

            model.CanCertificatoServizio = CessazioneHelper.GetTemplate(db, "CertificatoServizio", idDip, "", true, model.Pratica.SINTESI1.COD_QUALIFICA) != null;

            return PartialView("~/Views/Cessazione/subpartial/DettaglioIncentivato.cshtml", model);
        }

        public ActionResult RichiestaIntegrazioneEstratti(int idDip, DateTime dataRich)
        {
            var db = new IncentiviEntities();
            var dip = db.XR_INC_DIPENDENTI.Find(idDip);
            dip.DATA_RICH_INT = dataRich;

            List<XR_HRIS_PARAM> listParam = HrisHelper.GetParametriJson<XR_HRIS_PARAM>(HrisParam.IncentiviParametri);
            var param = listParam.FirstOrDefault(x => x.COD_PARAM == "LimiteRichiestaIntegrazione");
            dip.SetField("LimiteRichiestaIntegrazione", dip.DATA_RICH_INT.Value.Date.AddDays(Convert.ToDouble(param.COD_VALUE1)));

            string result = "";

            if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                result = "OK";
            else
                result = "Errore durante il salvataggio";

            return Content(result);
        }

        [HttpPost]
        public ActionResult DuplicaDocumenti(int idDipOrig, int idDipDest, int? idStatoOrig, int idStatoDest)
        {
            var db = new IncentiviEntities();
            var dipOrig = db.XR_INC_DIPENDENTI.Find(idDipOrig);

            string result = "OK";

            List<int> stati = new List<int>();
            if (idStatoOrig.HasValue)
            {
                stati.Add(idStatoOrig.Value);
                switch (idStatoOrig)
                {
                    case (int)IncStato.RichiestaInserita:
                        stati.Add((int)IncStato.TempFileEstratti);
                        break;
                    case (int)IncStato.TempFileEstratti:
                        stati.Add((int)IncStato.RichiestaInserita);
                        break;
                    default:
                        break;
                }
            }

            var allegati = db.XR_INC_OPERSTATI_DOC
                .Where(x => x.XR_INC_OPERSTATI.ID_DIPENDENTE == idDipOrig);

            if (stati.Any())
                allegati.Where(x => stati.Contains(x.XR_INC_OPERSTATI.ID_STATO));

            if (allegati != null && allegati.Any())
            {
                int idOperStato = CessazioneHelper.SalvaStato(db, idDipDest, idStatoDest, CommonHelper.GetCurrentIdPersona(), false);

                foreach (var item in allegati)
                {
                    XR_INC_OPERSTATI_DOC newDoc = new XR_INC_OPERSTATI_DOC()
                    {
                        COD_TERMID = item.COD_TERMID,
                        COD_TITLE = item.COD_TITLE,
                        COD_USER = item.COD_USER,
                        CONTENT_TYPE = item.CONTENT_TYPE,
                        DES_ALLEGATO = item.DES_ALLEGATO,
                        ID_OPER = item.ID_OPER,
                        ID_STATO_RIF = item.ID_STATO_RIF,
                        IND_RILEVANTE = item.IND_RILEVANTE,
                        NMB_SIZE = item.NMB_SIZE,
                        NME_FILENAME = item.NME_FILENAME,
                        NOT_TAG = dipOrig.COD_GRUPPO + (!String.IsNullOrWhiteSpace(item.NOT_TAG) ? ";" + item.NOT_TAG : ""),
                        OBJ_OBJECT = item.OBJ_OBJECT,
                        TMS_TIMESTAMP = item.TMS_TIMESTAMP,
                        VALID_DTA_END = item.VALID_DTA_END,
                        VALID_DTA_INI = item.VALID_DTA_INI
                    };
                    newDoc.ID_ALLEGATO = db.XR_INC_OPERSTATI_DOC.GeneraPrimaryKey();
                    newDoc.ID_OPER = idOperStato;
                    db.XR_INC_OPERSTATI_DOC.Add(newDoc);
                }

                if (!DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                    result = "Errore durante la duplicazione dei dati";
            }
            else
                result = "Nessun file da duplicare";


            return Content(result);
        }

        [HttpPost]
        public ActionResult RicevutaAnpal(int idDip, DateTime dataInvio)
        {
            string result = "";

            var db = new IncentiviEntities();
            var dip = db.XR_INC_DIPENDENTI.Find(idDip);
            if (dip == null)
                result = "Dipendente non trovato";
            else
            {
                dip.DATA_RECESSO_ANPAL = dataInvio;
                dip.SetField("AnpalDataCaricamento", DateTime.Now);
                dip.SetField("AnpalMatricolaCaricamento", CommonHelper.GetCurrentUserMatricola());
                if (!DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                    result = "Errore durante il salvataggio";
                else
                    result = "OK";
            }

            return Content(result);
        }
        [HttpPost]
        //int idDip, string infoTessContr, string nota, string oper, bool conferma, string infoMaternita
        public ActionResult CaricamentoTessereContributive(CessazioneModel model, string _oper, bool _conferma)
        {
            //Serviranno altri campi
            string result = "";
            string[] rifGruppo = HrisHelper.GetParametri<string>(HrisParam.IncentiviRifGruppo);
            var codGruppo = rifGruppo[0];
            var annoRif = rifGruppo[1];

            var db = new IncentiviEntities();
            var dip = db.XR_INC_DIPENDENTI.Find(model.Pratica.ID_DIPENDENTE);
            if (dip == null)
                result = "Dipendente non trovato";
            else
            {
                if (_oper == "mat")
                {
                    dip.DTA_RICH_MATCON = DateTime.Now;
                    dip.NOT_MATCON = "";

                    var template = CessazioneHelper.GetTemplate(db, "TemplateMail", dip.ID_DIPENDENTE, "RichMaternita", true, dip.SINTESI1.COD_QUALIFICA);
                    GestoreMail mail = new GestoreMail();
                    string mailDest = CessazioneHelper.GetMailDip(dip);// CommonHelper.GetEmailPerMatricola(dip.MATRICOLA);
                    string mailOggetto = CessazioneHelper.ReplaceToken(dip, template.DES_TEMPLATE);

                    Dictionary<string, string> paramTemplate = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(template.TEMPLATE_TEXT);

                    string mailTesto = paramTemplate["Corpo"];

                    string mittente = CessazioneHelper.GetIndirizzoMail("FromRichMaternita");
                    string cc = mittente;
                    var response = mail.InvioMail(mailTesto, mailOggetto, mailDest, cc, mittente, null, null);
                    if (response != null && response.Errore != null)
                    {
                        HrisHelper.LogOperazione("InvioMail", "Invio Mail richiesta maternità", response.Esito, response.Errore);
                    }
                }

                dip.INFO_TESS_CONTR = model.Pratica.INFO_TESS_CONTR;
                dip.INFO_MATCON = model.Pratica.INFO_MATCON;

                dip.SetField("NotHasTessereContr", model.NotHasTessereContr);
                dip.SetField("NotHasMaternita", model.NotHasMaternita);
                dip.SetField("NotHasCambioQualifica", model.NotHasCambioQualifica);

                if (_conferma)
                    dip.DTA_ARR_TESS_CONTR = DateTime.Now;

                if (!DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                    result = "Errore durante il salvataggio";
                else
                    result = "OK";
            }

            return Content(result);
        }

        [HttpPost]
        public ActionResult ConfermaTessMat(int idDip)
        {
            string result = "";
            string[] rifGruppo = HrisHelper.GetParametri<string>(HrisParam.IncentiviRifGruppo);
            var codGruppo = rifGruppo[0];
            var annoRif = rifGruppo[1];

            var db = new IncentiviEntities();
            var dip = db.XR_INC_DIPENDENTI.Find(idDip);
            if (dip == null)
                result = "Dipendente non trovato";
            else
            {
                bool hasTessContr = !dip.GetField("NotHasTessereContr", false);
                bool hasMat = !dip.GetField("NotHasMaternita", false);
                bool hasCambioQual = !dip.GetField("NotHasCambioQualifica", false);

                if (hasMat)
                    EsponiFileDipendente(idDip, db, dip, (int)IncStato.FileMat, "Maternità");
                if (hasTessContr)
                    EsponiFileDipendente(idDip, db, dip, (int)IncStato.FileTessContr, "Tessere assicurative");
                if (hasCambioQual)
                    ;

                if (hasTessContr && hasMat)
                    MailFileDipendente(annoRif, db, dip, "TessereContributiveMaternita", dip.INFO_TESS_CONTR, dip.INFO_MATCON, (int)IncStato.FileTessContr, (int)IncStato.FileMat);
                else if (hasTessContr && !hasMat)
                    MailFileDipendente(annoRif, db, dip, "TessereContributive", dip.INFO_TESS_CONTR, null, (int)IncStato.FileTessContr);
                else if (!hasTessContr && hasMat)
                    MailFileDipendente(annoRif, db, dip, "Maternita", null, dip.INFO_MATCON, (int)IncStato.FileMat);

                dip.DTA_INVIO_TESS = DateTime.Now;

                if (!DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                    result = "Errore durante il salvataggio";
                else
                    result = "OK";
            }

            return Content(result);
        }


        private static void MailFileDipendente(string annoRif, IncentiviEntities db, XR_INC_DIPENDENTI dip, string nomeTemplate, string tokenString, string secTokenString, params int[] stati)
        {
            //Invio mail al dipendente
            var template = CessazioneHelper.GetTemplate(db, "TemplateMail", dip.ID_DIPENDENTE, nomeTemplate, false);
            if (template != null)
            {
                GestoreMail mail = new GestoreMail();
                string mailDest = CessazioneHelper.GetMailDip(dip);// CommonHelper.GetEmailPerMatricola(dip.MATRICOLA);
                string mailOggetto = CessazioneHelper.ReplaceToken(dip, template.DES_TEMPLATE);

                Dictionary<string, string> paramTemplate = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(template.TEMPLATE_TEXT);

                string mailTesto = paramTemplate["Corpo"];

                string mittente = CessazioneHelper.GetIndirizzoMail("FromTessContr");
                string cc = mittente;

                myRaiCommonTasks.Helpers.FileResult files = null;
                var list = InternalGetAllegati(ref files, dip.ID_DIPENDENTE, dip.MATRICOLA, db, false, false, stati);
                var attachment = new List<myRaiCommonTasks.sendMail.Attachement>();
                foreach (var item in list)
                {
                    attachment.Add(new myRaiCommonTasks.sendMail.Attachement()
                    {
                        AttachementName = item.NME_FILENAME,
                        AttachementType = item.CONTENT_TYPE,
                        AttachementValue = db.XR_INC_OPERSTATI_DOC.Find(item.ID_ALLEGATO).OBJ_OBJECT
                    });
                }

                var response = mail.InvioMail(mailTesto, mailOggetto, mailDest, cc, mittente, attachment, null, null);
                if (response != null && response.Errore != null)
                {
                    HrisHelper.LogOperazione("InvioMail", "Invio Mail " + nomeTemplate, response.Esito, response.Errore);
                }
            }
        }

        private static void EsponiFileDipendente(int idDip, IncentiviEntities db, XR_INC_DIPENDENTI dip, int statoRif, string tag = null)
        {
            //Visualizzazione file al dipendente
            myRaiCommonTasks.Helpers.FileResult files = null;
            var list = InternalGetAllegati(ref files, idDip, dip.MATRICOLA, db, false, false, statoRif);
            int index = 0;
            foreach (var item in list)
            {
                var operDoc = db.XR_INC_OPERSTATI_DOC.Find(item.ID_ALLEGATO);
                string chiave = INCFileManager.GeneraChiave(idDip, false, statoRif, ++index);
                IncentivazioneFile dataFile = new IncentivazioneFile()
                {
                    IdDipendente = idDip,
                    Stato = statoRif,
                    Chiave = chiave,
                    Titolo = operDoc.COD_TITLE,
                    Descrizione = operDoc.DES_ALLEGATO,
                    MatricolaApprv = CommonHelper.GetCurrentUserMatricola(),
                    Tag = tag,
                    Caricato = true,
                    Approvato = true,
                    ReadOnly = true
                };
                FileManager.UploadFile(dip.MATRICOLA, "INC", operDoc.NME_FILENAME, operDoc.OBJ_OBJECT, chiave, null, Newtonsoft.Json.JsonConvert.SerializeObject(dataFile));
            }
        }

        public ActionResult GetModuloRich(int idDip)
        {
            var db = new IncentiviEntities();
            var dip = db.XR_INC_DIPENDENTI.Find(idDip);

            string filtroMatr = dip.MATRICOLA;
            var stato = CessazioneHelper.GetPraticaOperStato(dip);
            if (CessazioneHelper.InactiveState().Contains(stato.ID_STATO))
                filtroMatr = "$" + filtroMatr + "$";


            var dbTal = new myRaiDataTalentia.TalentiaEntities();
            var mod = dbTal.XR_MOD_DIPENDENTI.FirstOrDefault(x => x.ID_PERSONA == dip.ID_PERSONA && x.MATRICOLA == filtroMatr && x.DATA_COMPILAZIONE == dip.DTA_RICHIESTA && x.COD_MODULO == dip.COD_GRUPPO);
            if (mod != null)
            {
                MemoryStream ms = new MemoryStream(mod.PDF_MODULO);
                return new FileStreamResult(ms, "application/pdf") { FileDownloadName = "Modulo richiesta " + dip.ANAGPERS.DES_COGNOMEPERS.TitleCase() + " " + dip.ANAGPERS.DES_NOMEPERS.TitleCase() + ".pdf" };
            }
            else
                return View("~/Views/Shared/404.cshtml");
        }

        public ActionResult GetStringaFiltri(CessazioneFilter filter)
        {
            string text = "";

            if (filter.QualFilter != null && filter.QualFilter.Any())
            {
                var db = new IncentiviEntities();
                int[] tmp = filter.QualFilter.Select(x => Convert.ToInt32(x)).ToArray();
                var dbQualFilter = db.XR_INC_ADD_FILTER.Where(x => tmp.Contains(x.ID_FILTER)).Select(x => x.COD_FILTER).ToList();

                if (dbQualFilter.Count() == 1)
                    text += " di " + dbQualFilter[0];
                else
                    text += " di " + String.Join(",", dbQualFilter);
            }

            switch (filter.Tipologia)
            {
                case "1":
                    text += " di incentivazione";
                    break;
                case "2":
                    text += " di risoluzione consensuale";
                    break;
                default:
                    break;
            }

            if (!String.IsNullOrWhiteSpace(filter.Stato))
            {
                int intStato = Convert.ToInt32(filter.Stato);
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    text += " " + db.XR_INC_STATI.Single(x => x.ID_STATO == intStato).DES_FILTRO;
                }
            }

            switch (filter.InCarico)
            {
                case "0":
                    text += " non in carico";
                    break;
                case "1":
                    text += " prese in carico";
                    break;
            }

            if (!String.IsNullOrWhiteSpace(filter.Matricola))
                text += " con matricola " + filter.Matricola;
            if (!String.IsNullOrWhiteSpace(filter.Nominativo))
                text += " di dipendenti con il nominativo contenente '" + filter.Nominativo + "'";

            if (!String.IsNullOrWhiteSpace(filter.DataCessazione))
                text += " con data cessazione " + filter.DataCessazione.Substring(6, 2) + "/" + filter.DataCessazione.Substring(4, 2) + "/" + filter.DataCessazione.Substring(0, 4);

            if (!String.IsNullOrWhiteSpace(filter.Sede))
                text += " per la sede di " + filter.Sede;


            switch (filter.Causa)
            {
                case "0":
                    text += " senza cause o vertenze";
                    break;
                case "1":
                    text += " con cause o vertenze";
                    break;
                default:
                    break;
            }

            if (!String.IsNullOrWhiteSpace(filter.CodiceGruppo))
            {
                Dictionary<string, string> gruppi = HrisHelper.GetParametroJson<Dictionary<string, string>>(HrisParam.IncentiviGruppi);
                if (gruppi != null && gruppi.TryGetValue(filter.CodiceGruppo, out string desGruppo))
                    text += " del gruppo'" + desGruppo + "'";

            }

            if (!String.IsNullOrWhiteSpace(filter.DataRichiesta))
                text += " con richiesta in data " + filter.DataRichiesta;

            if (filter.TipoVertenza != null && filter.TipoVertenza.Any())
            {
                var parLimiti = HrisHelper.GetParametriJson<myRaiData.Incentivi.XR_HRIS_PARAM>(HrisParam.IncentiviParametri);
                var parTipiVert = parLimiti.FirstOrDefault(x => x.COD_PARAM == "TipologieVertenze");

                var dict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(parTipiVert.COD_VALUE1);
                if (filter.TipoVertenza.Count() == 1)
                    text += " con " + dict[filter.TipoVertenza[0]].ToLower();
                else
                {
                    text += " con " + String.Join(", ", dict.Where(x => filter.TipoVertenza.Take(filter.TipoVertenza.Length - 1).Contains(x.Key)).Select(x => x.Value.ToLower())) + " o " + dict[filter.TipoVertenza[filter.TipoVertenza.Length - 1]].ToLower();
                }
            }

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { Filtro = text }
            };
        }

        public ActionResult CaricaDatiDB2(string matricola)
        {
            Uri u = new Uri(new myRaiServiceHub.it.rai.servizi.wiahrss.ezService().Url);

            BasicHttpBinding binding = new BasicHttpBinding();
            EndpointAddress address = new EndpointAddress(u);
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;
            binding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
            binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;

            binding.MessageEncoding = WSMessageEncoding.Text;
            binding.TextEncoding = Encoding.UTF8;
            binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;

            var _client = new myRaiServiceHub.ServiceBonus100.ezServiceSoapClient(binding, address);

            if (_client.ClientCredentials != null)
            {
                _client.ClientCredentials.Windows.ClientCredential = CommonHelper.GetUtenteServizioCredentials();
                _client.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Delegation;
            }
            _client.Open();

            var requestInterceptor = new InspectorBehavior();
            _client.Endpoint.Behaviors.Add(requestInterceptor);

            decimal incen = 0, lordo = 0, netto = 0, c_537 = 0;
            string dtreg = "";

            try
            {
                string rifConteggio = HrisHelper.GetParametro<string>(HrisParam.IncentiviRiferimentoConteggi);
                if (String.IsNullOrWhiteSpace(rifConteggio))
                    rifConteggio = "18";

                var resp = _client.TLIQ_DATI_MATRICOLA("0" + matricola, rifConteggio);

                foreach (DataRow item in resp.Rows)
                {
                    switch (item.Field<string>("ID_DATO"))
                    {
                        case "C_537":
                            c_537 = (decimal)item["IMPORTO"];
                            break;
                        case "DTREG":
                            string tmp = item.Field<string>("TESTO");
                            dtreg = tmp.Substring(8, 2) + "/" + tmp.Substring(5, 2) + "/" + tmp.Substring(0, 4);
                            break;
                        case "INCEN":
                            incen = (decimal)item["IMPORTO"];
                            break;
                        case "LORDO":
                            lordo = (decimal)item["IMPORTO"];
                            break;
                        case "NETTO":
                            netto = (decimal)item["IMPORTO"];
                            break;
                    }
                }
            }
            catch (Exception)
            {

            }

            string requestXML = requestInterceptor.LastRequestXML;
            string responseXML = requestInterceptor.LastResponseXML;

            _client.Close();

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { c_537 = c_537.ToString().Replace('.', ','), incen = incen.ToString().Replace('.', ','), lordo = lordo.ToString().Replace('.', ','), netto = netto.ToString().Replace('.', ','), dtreg = dtreg }
            };
        }
        public ActionResult CaricaDatiIBAN(int idDip, string sceltaConto)
        {
            IncentiviEntities db = new IncentiviEntities();
            XR_INC_DIPENDENTI dip = db.XR_INC_DIPENDENTI.FirstOrDefault(x => x.ID_DIPENDENTE == idDip);

            string banca = "";
            string iban = "";
            string intestatario = "";

            var cc = dip.ANAGPERS.XR_DATIBANCARI.OrderByDescending(y => y.DTA_FINE).FirstOrDefault(x => x.XR_UTILCONTO.Count() == 1 && x.XR_UTILCONTO.First().COD_UTILCONTO == "01");
            if (sceltaConto == "Y" || sceltaConto == "C" || sceltaConto == "V")
            {
                banca = CezanneHelper.GetNomeBanca(cc.COD_ABI, cc.COD_CAB);
                iban = cc.COD_IBAN.Trim();
                intestatario = cc.DES_INTESTATARIO.Trim();
            }
            else if (sceltaConto == "B")
            {
                banca = "-";
                iban = "-";
                intestatario = "-";
            }

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { banca = banca, iban = iban, intestatario = intestatario }
            };
        }

        public static SituazioneDebitoriaVM GetSituazioneDebitoria(string matricola)
        {
            SituazioneDebitoriaVM model = new SituazioneDebitoriaVM();
            model.Data = new List<SituazioneDebitoriaModel>();

            try
            {
                DateTime? data = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

                if (!data.HasValue)
                {
                    data = DateTime.Now;
                }

                model.CurrentDate = data.Value;
                var resultData = SituazioneDebitoriaManager.GetSituazioneDebitoria(matricola);

                if (resultData != null && resultData.Any())
                {
                    foreach (var itm in resultData)
                    {
                        if (data.HasValue)
                        {

                            if ((itm.AnnoA == 0) || (itm.AnnoDa == 0))
                            {
                                model.Data.Add(new SituazioneDebitoriaModel()
                                {
                                    Addebito = itm.Addebito,
                                    Descrizione = itm.Descrizione,
                                    ImportoRata = itm.ImportoRata,
                                    ImportoRateResidue = itm.ImportoRateResidue,
                                    MeseA = "",
                                    MeseDa = "",
                                    NumeroRate = itm.NumeroRate,
                                    NumeroRateResidue = itm.NumeroRateResidue
                                });
                            }
                            else
                            {

                                DateTime? t1 = null;
                                DateTime? t2 = null;

                                if (itm.AnnoDa > 0 && itm.IntMeseDa > 0)
                                {
                                    t1 = new DateTime(itm.AnnoDa, itm.IntMeseDa, 1);
                                }
                                else if (itm.AnnoDa > 0 && itm.IntMeseDa == 0)
                                {
                                    t1 = DateTime.MinValue;
                                    itm.MeseDa = itm.AnnoDa.ToString();
                                }

                                if (itm.AnnoA > 0 && itm.IntMeseA > 0)
                                {
                                    t2 = new DateTime(itm.AnnoA, itm.IntMeseA, 1);
                                }
                                else if (itm.AnnoA > 0 && itm.IntMeseA == 0)
                                {
                                    t2 = DateTime.MaxValue;
                                }

                                if (t1.HasValue &&
                                    t2.HasValue &&
                                    t1 <= data &&
                                    data <= t2)
                                {
                                    model.Data.Add(new SituazioneDebitoriaModel()
                                    {
                                        Addebito = itm.Addebito,
                                        Descrizione = itm.Descrizione,
                                        ImportoRata = itm.ImportoRata,
                                        ImportoRateResidue = itm.ImportoRateResidue,
                                        MeseA = String.IsNullOrEmpty(itm.MeseA) ? String.Empty : itm.MeseA,
                                        MeseDa = String.IsNullOrEmpty(itm.MeseDa) ? String.Empty : itm.MeseDa,
                                        NumeroRate = itm.NumeroRate,
                                        NumeroRateResidue = itm.NumeroRateResidue
                                    });
                                }
                            }


                        }
                    }
                }

                //return model;
            }
            catch (Exception ex)
            {
                //return PartialView("~/Views/Shared/TblError.cshtml", new HandleErrorInfo(ex, "SituazioneDebitoriaController", "LoadTableDebiti"));
            }

            return model;
        }

        public ActionResult Widget_Costi()
        {
            string queryBudget = HrisHelper.GetParametro<string>(HrisParam.IncentiviQueryBudget);

            CessazioneCosti model = new CessazioneCosti();
            model.MaxValue = HrisHelper.GetParametro<decimal>(HrisParam.IncentiviCostoMax);

            var db = new IncentiviEntities();
            model.CurrentValue = db.Database.SqlQuery<decimal>(queryBudget).FirstOrDefault();


            return View("subpartial/Widget_Costi", model);
        }
        public ActionResult Modal_Costi()
        {
            string queryBudget = HrisHelper.GetParametro<string>(HrisParam.IncentiviQueryBudget);

            CessazioneCosti model = new CessazioneCosti();
            model.MaxValue = HrisHelper.GetParametro<decimal>(HrisParam.IncentiviCostoMax);

            var db = new IncentiviEntities();
            model.CurrentValue = db.Database.SqlQuery<decimal>(queryBudget).FirstOrDefault();

            string[] abilSaving = HrisHelper.GetParametri<string>(HrisParam.IncentiviFilterAbilSaving);
            if (abilSaving[0] == "FALSE" || abilSaving[1].Contains(CommonHelper.GetCurrentUserMatricola()))
            {
                var querySavingParam = myRaiHelper.HrisHelper.GetParametri<string>(myRaiHelper.HrisParam.IncentiviQuerySaving);
                string querySaving = "";
                if (querySavingParam != null && querySavingParam.Any())
                {
                    querySaving = querySavingParam[0].Replace("__MIN_YEAR__", querySavingParam[1]).Replace("__MAX_YEAR__", querySavingParam[2]);
                    try
                    {
                        model.Saving = db.Database.SqlQuery<CessazioneSaving>(querySaving).ToList();
                    }
                    catch (Exception)
                    {

                    }
                }
            }

            return View("subpartial/Modal_Costi", model);
        }

        #region CreazioneNuovaPratica
        [HttpPost]
        public ActionResult RicercaAnagrafiche(RicercaAnagrafica model)
        {
            ElencoAnagrafiche elencoAnagrafiche = CessazioneManager.GetElencoAnagrafiche(model);

            return View("~/Views/Cessazione/subpartial/" + model.ResultView + ".cshtml", elencoAnagrafiche);
        }
        [HttpGet]
        public ActionResult GetAnteprimaPratica(int idPersona)
        {
            IncentiviEntities db = new IncentiviEntities();
            CessazioneModel model = new CessazioneModel()
            {
                Pratica = new XR_INC_DIPENDENTI()
                {
                    ID_PERSONA = idPersona,
                    ANAGPERS = db.ANAGPERS.FirstOrDefault(x => x.ID_PERSONA == idPersona),
                    SINTESI1 = db.SINTESI1.FirstOrDefault(x => x.ID_PERSONA == idPersona),
                }
            };
            model.Pratica.MATRICOLA = model.Pratica.SINTESI1.COD_MATLIBROMAT;
            model.IsPreview = true;

            return View("~/Views/Cessazione/subpartial/DettaglioIncentivato.cshtml", model);
        }

        public ActionResult ShowAggiungiDip(int idPersona)
        {
            IncentiviEntities db = new IncentiviEntities();
            AggiuntaDipendente dip = new AggiuntaDipendente();
            dip.IdPersona = idPersona;
            var sint = db.SINTESI1.FirstOrDefault(x => x.ID_PERSONA == idPersona);
            dip.Matricola = sint.COD_MATLIBROMAT;
            dip.Cognome = sint.DES_COGNOMEPERS;
            dip.Nome = sint.DES_NOMEPERS;
            dip.Sede = sint.DES_CITTASEDE.TitleCase();

            dip.DataAssunzione = sint.DTA_INIZIO_CR.GetValueOrDefault();
            dip.DataAnzianita = sint.DTA_ANZCONV.GetValueOrDefault();
            dip.DataCessazione = sint.DTA_FINE_CR.GetValueOrDefault();

            return View("~/Views/Cessazione/subpartial/AggiuntaDipendente.cshtml", dip);
        }

        public ActionResult AggiungiDip(AggiuntaDipendente model)
        {
            CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime tms);

            IncentiviEntities db = new IncentiviEntities();

            XR_INC_DIPENDENTI dip = new XR_INC_DIPENDENTI();
            dip.ID_DIPENDENTE = db.XR_INC_DIPENDENTI.GeneraPrimaryKey();
            dip.ID_PERSONA = model.IdPersona;
            dip.MATRICOLA = model.Matricola;

            dip.DATA_CESSAZIONE = model.DataCessazione;
            dip.DATA_ASSUNZIONE = model.DataAssunzione;
            dip.DATA_ANZIANITA = model.DataAnzianita;

            if (model.DataPagamento.HasValue && model.DataPagamento.Value > new DateTime(2000, 1, 1))
                dip.DATA_PAGAMENTO = model.DataPagamento;

            dip.UNA_TANTUM_LORDA = model.UnaTantum;
            dip.INCENTIVO_LORDO = model.IncentivoLordo;

            dip.SEDE = model.Sede;

            if (!String.IsNullOrWhiteSpace(model.Parttime))
                dip.PART_TIME = model.Parttime;

            if (!String.IsNullOrWhiteSpace(model.CauseVertenze))
                dip.CAUSE_VERTENZE = model.CauseVertenze;

            dip.ID_TIPOLOGIA = model.Tipologia;

            dip.COD_USER = codUser;
            dip.COD_TERMID = codTermid;
            dip.TMS_TIMESTAMP = tms;
            db.XR_INC_DIPENDENTI.Add(dip);

            var firstState = db.XR_WKF_WORKFLOW.Where(x => x.ID_TIPOLOGIA == model.Tipologia).OrderBy(x => x.ORDINE).FirstOrDefault();

            XR_INC_OPERSTATI statoDaAvviare = new XR_INC_OPERSTATI();
            statoDaAvviare.ID_OPER = db.XR_INC_OPERSTATI.GeneraPrimaryKey();
            statoDaAvviare.ID_DIPENDENTE = dip.ID_DIPENDENTE;
            statoDaAvviare.ID_PERSONA = CezanneHelper.GetIdPersona(CommonHelper.GetCurrentUserMatricola());
            statoDaAvviare.ID_STATO = firstState.ID_STATO;
            statoDaAvviare.DATA = DateTime.Now;
            statoDaAvviare.COD_USER = codUser;
            statoDaAvviare.COD_TERMID = codTermid;
            statoDaAvviare.TMS_TIMESTAMP = tms;
            db.XR_INC_OPERSTATI.Add(statoDaAvviare);

            if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                return Content("OK");
            else
                return Content("Errore durante il salvataggio");
        }
        #endregion

        #region GestionePratica
        public ActionResult PrendiPratica(int idDip)
        {
            RilasciaPratica(idDip);
            CessazioneHelper.SalvaStato(null, idDip, (int)IncStato.InCarico, CommonHelper.GetCurrentIdPersona());
            return Content("OK");
        }
        public ActionResult RilasciaPraticaAjax(int idDip)
        {
            RilasciaPratica(idDip);
            return Content("OK");
        }
        public void RilasciaPratica(int idDip)
        {
            IncentiviEntities db = new IncentiviEntities();
            XR_INC_OPERSTATI stato = db.XR_INC_OPERSTATI.FirstOrDefault(x => x.ID_DIPENDENTE == idDip && x.ID_STATO == (int)IncStato.InCarico);
            if (stato != null)
            {
                db.XR_INC_OPERSTATI.Remove(stato);
                DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
            }
        }
        public ActionResult EliminaStato(int idOper)
        {
            try
            {
                IncentiviEntities db = new IncentiviEntities();
                XR_INC_OPERSTATI oper = db.XR_INC_OPERSTATI.FirstOrDefault(x => x.ID_OPER == idOper);
                if (oper == null)
                    return Content("not_found");
                else
                {
                    db.XR_INC_OPERSTATI_DOC.RemoveWhere(x => x.ID_OPER == idOper);
                    db.XR_INC_OPERSTATI_NOTE.RemoveWhere(x => x.ID_OPER == idOper);
                    db.XR_INC_OPERSTATI.Remove(oper);
                    if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                        return Content("OK");
                    else
                        return Content("Errore durante il salvataggio");
                }

            }
            catch (Exception)
            {
                return Content("error");
            }
        }
        public ActionResult AvviaPratica(int idDip, bool vincoloBCCR = false, string causeVertenze = "", bool pignoramento = false, bool estAnticipata = false, bool cessioneQuinto = false)
        {
            string result = "OK";

            if (vincoloBCCR || !String.IsNullOrWhiteSpace(causeVertenze) || pignoramento || estAnticipata || cessioneQuinto)
            {
                bool modified = false;
                IncentiviEntities db = new IncentiviEntities();
                XR_INC_DIPENDENTI dip = db.XR_INC_DIPENDENTI.FirstOrDefault(x => x.ID_DIPENDENTE == idDip);
                if (!String.IsNullOrWhiteSpace(causeVertenze)
                    && (String.IsNullOrWhiteSpace(dip.CAUSE_VERTENZE) || dip.CAUSE_VERTENZE != causeVertenze))
                {
                    dip.CAUSE_VERTENZE = causeVertenze;
                    modified = true;
                }

                if (vincoloBCCR)
                {
                    dip.IND_PROPRIO_IBAN = "B";
                    modified = true;
                }

                if (pignoramento)
                {
                    dip.IND_PIGNORAMENTO = true;
                    modified = true;
                }

                if (estAnticipata)
                {
                    dip.IND_ESTIN_ANT_PRESTITO = true;
                    modified = true;
                }

                if (cessioneQuinto)
                {
                    dip.IND_CESSIONE_QUINTO_TFR = true;
                    modified = true;
                }

                CessazioneHelper.GetImportoAltreTrattenute(db, dip);

                if (modified)
                {
                    CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime tms);
                    dip.COD_USER = codUser;
                    dip.COD_TERMID = codTermid;
                    dip.TMS_TIMESTAMP = tms;
                    DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                }
            }

            SalvaStato(idDip, (int)IncStato.Controllato);
            RilasciaPratica(idDip);
            return Content(result);
        }
        public ActionResult SubmitStatoControllato(int idDip, bool vincoloBCCR = false, string causeVertenze = "")
        {
            string result = "OK";

            if (vincoloBCCR || !String.IsNullOrWhiteSpace(causeVertenze))
            {
                bool modified = false;
                IncentiviEntities db = new IncentiviEntities();
                XR_INC_DIPENDENTI dip = db.XR_INC_DIPENDENTI.FirstOrDefault(x => x.ID_DIPENDENTE == idDip);
                if (!String.IsNullOrWhiteSpace(causeVertenze)
                    && (String.IsNullOrWhiteSpace(dip.CAUSE_VERTENZE) || dip.CAUSE_VERTENZE != causeVertenze))
                {
                    dip.CAUSE_VERTENZE = causeVertenze;
                    modified = true;
                }

                if (vincoloBCCR)
                {
                    dip.IND_PROPRIO_IBAN = "B";
                    modified = true;
                }

                if (modified)
                {
                    CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime tms);
                    dip.COD_USER = codUser;
                    dip.COD_TERMID = codTermid;
                    dip.TMS_TIMESTAMP = tms;
                    DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                }
            }

            SalvaStato(idDip, (int)IncStato.Controllato);
            RilasciaPratica(idDip);
            return Content(result);
        }

        private void InvalidaStatoInternal(int idOper)
        {
            IncentiviEntities db = new IncentiviEntities();
            XR_INC_OPERSTATI oper = db.XR_INC_OPERSTATI.FirstOrDefault(x => x.ID_OPER == idOper);
            oper.DATA_FINE_VALIDITA = DateTime.Now;
            DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());

        }
        private int SalvaStato(int idDip, int stato)
        {
            return CessazioneHelper.SalvaStato(idDip, stato);
        }
        public ActionResult EliminaPratica(int idDip)
        {

            IncentiviEntities db = new IncentiviEntities();
            XR_INC_DIPENDENTI dip = db.XR_INC_DIPENDENTI.FirstOrDefault(x => x.ID_DIPENDENTE == idDip);
            /*
             * XR_INC_OPERSTATI_DOC
             * XR_INC_OPERSTATI_NOTE
             * XR_INC_OPERSTATI
             * XR_INC_DIPENDENTI_NOTE
             * XR_INC_SIBA
             * XR_INC_DIPENDENTI
            */
            foreach (var oper in dip.XR_INC_OPERSTATI)
            {
                db.XR_INC_OPERSTATI_DOC.RemoveWhere(x => x.ID_OPER == oper.ID_OPER);
                db.XR_INC_OPERSTATI_NOTE.RemoveWhere(x => x.ID_OPER == oper.ID_OPER);
            }
            db.XR_INC_OPERSTATI.RemoveWhere(x => x.ID_DIPENDENTE == dip.ID_DIPENDENTE);
            db.XR_INC_DIPENDENTI_NOTE.RemoveWhere(x => x.ID_DIPENDENTE == dip.ID_DIPENDENTE);
            db.XR_INC_SIBA.RemoveWhere(x => x.ID_DIPENDENTE == dip.ID_DIPENDENTE);
            db.XR_INC_DIPENDENTI.Remove(dip);

            string result = "";
            if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), "HRIS - Cessazione"))
                result = "OK";
            else
                result = "Errore durante il salvataggio";

            return Content(result);
        }
        public ActionResult AnnullaPratica(int idDip, string nota = "")
        {
            SalvaStato(idDip, (int)IncStato.RichiestaAnnullata);
            if (!String.IsNullOrWhiteSpace(nota))
                InternalAggiungiNotaPratica(idDip, nota);
            RilasciaPratica(idDip);

            var db = new IncentiviEntities();
            var dip = db.XR_INC_DIPENDENTI.Find(idDip);
            RimuoviRichiestaIncentivi(dip, "a", nota);

            return Content("OK");
        }
        public ActionResult AnnullaStato(int idDip, int idStato)
        {
            var db = new IncentiviEntities();
            var dip = db.XR_INC_DIPENDENTI.Find(idDip);
            var oper = CessazioneHelper.GetPraticaOperStato(dip);
            oper.DATA_FINE_VALIDITA = DateTime.Now;

            if (idStato == (int)IncStato.RichiestaAccettata)
                CessazioneManager.RendiAllegatiEffettivi(idDip, db, idStato, (int)IncStato.TempFileEstratti);
            else if (idStato == (int)IncStato.RecessoEffettuato)
            {
                CessazioneManager.RendiAllegatiEffettivi(idDip, db, (int)IncStato.FileProposta, (int)IncStato.TempFileProposta);
                CessazioneManager.RendiAllegatiEffettivi(idDip, db, (int)IncStato.FileAccettazione, (int)IncStato.TempFileAccettazione);
            }


            DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
            RilasciaPratica(idDip);
            return Content("OK");
        }

        public ActionResult CheckLimiteBudget(int idDip)
        {
            CessazioneCosti model = new CessazioneCosti();
            var tmp = HrisHelper.GetParametro(HrisParam.IncentiviCostoMax);

            model.MaxValue = Convert.ToDecimal(tmp.COD_VALUE1);
            string dicitura = tmp.COD_VALUE2 ?? "";
            string messaggio = tmp.COD_VALUE3 ?? "";

            string queryBudget = HrisHelper.GetParametro<string>(HrisParam.IncentiviQueryBudget);

            var db = new IncentiviEntities();
            model.CurrentValue = db.Database.SqlQuery<decimal>(queryBudget).FirstOrDefault();

            var dip = db.XR_INC_DIPENDENTI.Find(idDip);
            var importo = dip.INCENTIVO_LORDO_IP.GetValueOrDefault() + dip.UNA_TANTUM_LORDA_IP.GetValueOrDefault() + CessazioneHelper.CalcoloOneri(dip.SINTESI1.COD_QUALIFICA, dip.UNA_TANTUM_LORDA_IP.GetValueOrDefault());

            string result = "OK";
            if (model.CurrentValue + importo > model.MaxValue)
            {
                result = "Secondo i calcoli provvisori il budget di ##DICIT_LIMITE## di euro è stato già raggiunto dalle pratiche convalidate, con questa pratica l'impegno sarà di € ##IMPORTO_POST##";
                result = messaggio.Replace("##DICIT_LIMITE##", dicitura).Replace("##IMPORTO_POST##", (model.CurrentValue + importo).ToString("N2"));
            }

            return Content(result);
        }
        public ActionResult IncApprovazioneRichiesta(CessazioneModel modDip, bool prosegui, bool _stato, string _nota, string _oper)
        {
            string result = "";
            bool rimuoviRichiesta = false;

            IncentiviEntities db = new IncentiviEntities();
            var inc = db.XR_INC_DIPENDENTI.Find(modDip.Pratica.ID_DIPENDENTE);
            inc.DATA_PENSIONE_ANTICIPATA = modDip.Pratica.DATA_PENSIONE_ANTICIPATA;
            inc.DATA_ARRIVO_DOC = modDip.Pratica.DATA_ARRIVO_DOC;
            inc.DATA_RICH_INT = modDip.Pratica.DATA_RICH_INT;
            inc.DATA_ARRIVO_INT = modDip.Pratica.DATA_ARRIVO_INT;

            if (modDip.Pratica.NOT_TIP_ACCERT != inc.NOT_TIP_ACCERT || modDip.Pratica.NOT_REQ_MATURATO != inc.NOT_REQ_MATURATO)
            {
                inc.NUM_MENS_PRINC = null;
                inc.NUM_MENS_AGG = null;
                TaskHelper.AddBatchRunnerTask("IncentiviFNL930OnDemand", out var taskErr, note: String.Format("Matricola {0} - IdDip: {1}", inc.MATRICOLA, inc.ID_DIPENDENTE));
            }
            inc.NOT_TIP_ACCERT = modDip.Pratica.NOT_TIP_ACCERT;
            inc.IND_INVALIDITA = modDip.PercipientePensioneInvalidita ? 1 : 0;

            inc.NOT_REQ_MATURATO = modDip.Pratica.NOT_REQ_MATURATO;
            inc.DATA_RICH_TESS_CONTR = modDip.Pratica.DATA_RICH_TESS_CONTR;
            inc.NOT_RICH_TESS_CONTR = _oper == "tesscontr" ? _nota : modDip.Pratica.NOT_RICH_TESS_CONTR;

            CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime tms);
            inc.COD_USER = codUser;
            inc.COD_TERMID = codTermid;
            inc.TMS_TIMESTAMP = tms;

            if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
            {
                if (prosegui)
                {
                    inc.DATA_CESSAZIONE = inc.DATA_USCITA_RICH;

                    int intState = 0;
                    if (_stato)
                        intState = (int)IncStato.RichiestaAccettata;
                    else
                    {
                        rimuoviRichiesta = true;
                        if (_oper == "d")
                            intState = (int)IncStato.RichiestaDecaduta;
                        else
                            intState = (int)IncStato.RichiestaRifiutata;
                    }

                    if (_oper == "d")
                    {
                        InternalGestioneSollecito(modDip.Pratica.ID_DIPENDENTE, "DecadenzaEstratti", _nota);
                    }
                    else
                    {
                        CessazioneHelper.SalvaStato(db, modDip.Pratica.ID_DIPENDENTE, intState, CommonHelper.GetCurrentIdPersona());
                        CessazioneManager.RendiAllegatiEffettivi(modDip.Pratica.ID_DIPENDENTE, db, (int)IncStato.TempFileEstratti, intState);
                        CessazioneManager.RendiAllegatiDipEffettivi(modDip.Pratica.ID_DIPENDENTE, modDip.Pratica.MATRICOLA, db, (int)IncStato.RichiestaInserita, intState);
                        DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());

                        //NC In questo caso manda mail al dip
                        if (prosegui)
                        {
                            List<XR_HRIS_PARAM> param = HrisHelper.GetParametriJson<XR_HRIS_PARAM>(HrisParam.IncentiviParametri);
                            var paramMail = param.FirstOrDefault(x => x.COD_PARAM == "MailApprovazioneRichiesta");
                            if (paramMail != null)
                            {
                                GestoreMail mail = new GestoreMail();
                                string subj = paramMail.COD_VALUE1;
                                string dest = CessazioneHelper.GetMailDip(inc); //CommonTasks.GetEmailPerMatricola(inc.MATRICOLA);
                                string from = paramMail.COD_VALUE2;
                                string destCC = !String.IsNullOrWhiteSpace(paramMail.COD_VALUE3) ? paramMail.COD_VALUE3 : null;

                                string body = paramMail.COD_VALUE4;
                                mail.InvioMail(from, subj, dest, destCC, subj, null, body, "VAI AL SITO");
                            }
                        }
                    }
                }

                if (prosegui)
                {
                    if (!String.IsNullOrWhiteSpace(_nota))
                        InternalAggiungiNotaPratica(inc.ID_DIPENDENTE, _nota, 0);
                    RilasciaPratica(modDip.Pratica.ID_DIPENDENTE);
                }

                if (rimuoviRichiesta && _oper != "d")
                    RimuoviRichiestaIncentivi(inc, _oper, _nota);

                result = "OK";
            }
            else
            {
                result = "Errore nel salvataggio";
            }

            return Content(result);
        }

        private static string SerializeObject<T>(T oggetto) where T : class
        {
            string result;
            var emptyNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
            var serializer = new XmlSerializer(oggetto.GetType());
            var settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            using (Utf8StringWriter sw = new Utf8StringWriter())
            {
                using (var wri = XmlWriter.Create(sw, settings))
                {
                    serializer.Serialize(wri, oggetto, emptyNamespaces);
                    result = sw.ToString();
                }
            }

            return result;
        }
        public ActionResult GeneraAccettazione(int idDip)
        {
            string result = null;
            var db = new IncentiviEntities();
            var dip = db.XR_INC_DIPENDENTI.Find(idDip);

            string[] rifGruppo = HrisHelper.GetParametri<string>(HrisParam.IncentiviRifGruppo);
            var codGruppo = rifGruppo[0];
            var annoRif = rifGruppo[1];

            var template = CessazioneHelper.GetTemplate(db, "Accettazione", dip.ID_DIPENDENTE, "Accettazione", true, dip.SINTESI1.COD_QUALIFICA);

            string currentMatr = CommonHelper.GetCurrentUserMatricola();
            string currentPMatr = CommonHelper.GetCurrentUserPMatricola();

            if (!CessazioneHelper.GetRuoloProtocollo(CommonHelper.GetCurrentUserMatricola(), out string idRuolo))
                return Content("L'utente non è abilitato a generare l'accettazione");

            string nominativoDestinatario = dip.SINTESI1.Nominativo().TitleCase();
            string protOgg = String.Format("Esodi {0} - Accettazione proposta {1}", annoRif, nominativoDestinatario);

            CreaProtocollo outputServizio;
            bool creaProt = false;
            int versione = -1;
            string oldProt = dip.GetField<string>("ProtocolloAccettazione", null);
            DateTime? dataAccettazione = null;

            if (!String.IsNullOrWhiteSpace(oldProt))
            {
                outputServizio = new CreaProtocollo()
                {
                    Identificativo = oldProt,
                    Id_documento = dip.GetField("ProtocolloIdDocumento", ""),
                };
                versione = dip.GetField<int>("ProtocolloVersione", -1);
                dataAccettazione = dip.GetField<DateTime?>("DataProtocolloAccettazione");
                if (!dataAccettazione.HasValue)
                    dataAccettazione = DateTime.Now;
                creaProt = true;
            }
            else
            {
                creaProt = DaFirmareManager.CreaProtocollo(idRuolo, protOgg, currentMatr, dip.MATRICOLA, currentPMatr, out outputServizio);
                dataAccettazione = DateTime.Now;
            }

            if (!creaProt)
                result = "Errore durante la creazione del protocollo";
            else
            {
                byte[] output = null;
                try
                {
                    #region AcroFields
                    PdfReader reader = new PdfReader(template.TEMPLATE);
                    MemoryStream ms = new MemoryStream();
                    PdfStamper stamper = new PdfStamper(reader, ms, '4');

                    stamper.AcroFields.SetField("fieldProtocollo", outputServizio.Identificativo);
                    stamper.AcroFields.SetField("fieldNome", (dip.SINTESI1.COD_SESSO == "M" ? "Sig." : "Sig.ra") + " " + (dip.SINTESI1.DES_COGNOMEPERS + " " + dip.SINTESI1.DES_NOMEPERS).TitleCase());
                    stamper.AcroFields.SetField("fieldDataAccettazione", dataAccettazione.Value.ToString("dd/MM/yyyy"));
                    stamper.AcroFields.SetField("fieldDataRich", dip.DATA_FIRMA_DIP.Value.ToString("dd/MM/yyyy"));
                    stamper.AcroFields.SetField("fieldDataCessazione", dip.DATA_CESSAZIONE.Value.ToString("dd/MM/yyyy"));

                    stamper.Writer.CloseStream = false;
                    stamper.FormFlattening = true;
                    stamper.Close();

                    ms.Position = 0;
                    output = ms.ToArray();
                    ms.Flush();
                    reader.Close();
                    #endregion
                }
                catch (Exception ex)
                {
                    return Content(ex.Message);
                }

                dip.SetField("ProtocolloAccettazione", outputServizio.Identificativo);
                dip.SetField("DataProtocolloAccettazione", dataAccettazione);
                dip.SetField("ProtocolloIdDocumento", outputServizio.Id_documento);

                string base64String = Convert.ToBase64String(output, 0, output.Length);
                dip.SetField("AccettazioneBase64", base64String);

                versione++;
                dip.SetField("ProtocolloVersione", versione);

                bool insProt = DaFirmareManager.InserisciAllegatoProtocollo(idRuolo, currentPMatr, outputServizio.Id_documento, protOgg + ".pdf", protOgg, base64String, "1", versione.ToString());
                if (!insProt)
                    result = "Errore durante il caricamento del protocollo";
                else
                {
                    #region AggiuntaFileSuInc
                    int idOper = CessazioneHelper.SalvaStato(db, dip.ID_DIPENDENTE, (int)IncStato.TempFileAccettazione, saveState: false);

                    CezanneHelper.GetCampiFirma(out string codUser, out string termid, out var tms);
                    var doc = new myRaiData.Incentivi.XR_INC_OPERSTATI_DOC();
                    doc.ID_ALLEGATO = db.XR_INC_OPERSTATI_DOC.GeneraPrimaryKey();
                    doc.ID_OPER = idOper;
                    doc.NME_FILENAME = protOgg + ".pdf";
                    doc.COD_TITLE = "Accettazione proposta firmata";
                    doc.DES_ALLEGATO = "Prot. " + outputServizio.Identificativo;
                    doc.CONTENT_TYPE = "application/pdf";
                    doc.OBJ_OBJECT = output;
                    doc.NMB_SIZE = output.Length;
                    doc.NOT_TAG = "Accettazione";
                    doc.IND_RILEVANTE = true;
                    doc.COD_USER = codUser;
                    doc.COD_TERMID = termid;
                    doc.TMS_TIMESTAMP = dataAccettazione.Value;
                    db.XR_INC_OPERSTATI_DOC.Add(doc);
                    #endregion

                    dip.SetField("AccettazioneGenerata", dataAccettazione.Value);

                    if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                    {
                        result = "OK";
                    }
                    else
                        result = "Errore durante il salvataggio";
                }
            }

            return Content(result);
        }
        public ActionResult UploadFileAccettazione(int idDip, HttpPostedFileBase _file)
        {
            string result = "";

            string[] rifGruppo = HrisHelper.GetParametri<string>(HrisParam.IncentiviRifGruppo);
            var codGruppo = rifGruppo[0];
            var annoRif = rifGruppo[1];
            string currentMatr = CommonHelper.GetCurrentUserMatricola();
            string currentPMatr = CommonHelper.GetCurrentUserPMatricola();

            var db = new IncentiviEntities();
            var dip = db.XR_INC_DIPENDENTI.Find(idDip);

            if (!CessazioneHelper.GetRuoloProtocollo(CommonHelper.GetCurrentUserMatricola(), out string idRuolo))
                return Content("L'utente non è abilitato a generare l'accettazione");

            string nominativoDestinatario = dip.SINTESI1.Nominativo().TitleCase();
            string protOgg = String.Format("Esodi {0} - Accettazione proposta {1}", annoRif, nominativoDestinatario);

            CreaProtocollo outputServizio;
            int versione = -1;
            string oldProt = dip.GetField<string>("ProtocolloAccettazione", null);
            DateTime? dataAccettazione = null;

            outputServizio = new CreaProtocollo()
            {
                Identificativo = oldProt,
                Id_documento = dip.GetField("ProtocolloIdDocumento", ""),
            };
            versione = dip.GetField<int>("ProtocolloVersione", -1);
            dataAccettazione = dip.GetField<DateTime?>("DataProtocolloAccettazione");

            byte[] output = null;
            using (MemoryStream ms = new MemoryStream())
            {
                _file.InputStream.CopyTo(ms);
                output = ms.ToArray();
            }

            string base64String = Convert.ToBase64String(output, 0, output.Length);
            dip.SetField("AccettazioneBase64", base64String);

            versione++;
            dip.SetField("ProtocolloVersione", versione);

            bool insProt = DaFirmareManager.InserisciAllegatoProtocollo(idRuolo, currentPMatr, outputServizio.Id_documento, protOgg + ".pdf", protOgg, base64String, "1", versione.ToString());
            if (!insProt)
                result = "Errore durante il caricamento del protocollo";
            else
            {
                #region AggiuntaFileSuInc
                //In questo caso serve solo a recuperare l'id
                int idOper = CessazioneHelper.SalvaStato(db, dip.ID_DIPENDENTE, (int)IncStato.TempFileAccettazione, saveState: false);

                //Prendo il file dell'accettazione caricato e lo invalido (cosi da conservare un backup)
                CezanneHelper.GetCampiFirma(out string codUser, out string termid, out var tms);

                myRaiCommonTasks.Helpers.FileResult files = null;
                var fileAcc = InternalGetAllegatiByTags(ref files, dip.ID_DIPENDENTE, dip.MATRICOLA, db, false, true, new string[] { "Accettazione" }, false, false, (int)IncStato.TempFileAccettazione);
                if (fileAcc != null && fileAcc.Count() == 1)
                {
                    var acc = db.XR_INC_OPERSTATI_DOC.Find(fileAcc.FirstOrDefault().ID_ALLEGATO);
                    acc.VALID_DTA_END = tms;
                }

                var doc = new myRaiData.Incentivi.XR_INC_OPERSTATI_DOC();
                doc.ID_ALLEGATO = db.XR_INC_OPERSTATI_DOC.GeneraPrimaryKey();
                doc.ID_OPER = idOper;
                doc.NME_FILENAME = protOgg + ".pdf";
                doc.COD_TITLE = "Accettazione proposta firmata";
                doc.DES_ALLEGATO = "Prot. " + outputServizio.Identificativo;
                doc.CONTENT_TYPE = "application/pdf";
                doc.OBJ_OBJECT = output;
                doc.NMB_SIZE = output.Length;
                doc.NOT_TAG = "Accettazione";
                doc.IND_RILEVANTE = true;
                doc.COD_USER = codUser;
                doc.COD_TERMID = termid;
                doc.TMS_TIMESTAMP = dataAccettazione.Value;
                db.XR_INC_OPERSTATI_DOC.Add(doc);
                #endregion

                dip.SetField("AccettazioneGenerata", dataAccettazione.Value);

                if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                {
                    result = "OK";
                }
                else
                    result = "Errore durante il salvataggio";
            }

            return Content(result);
        }
        public ActionResult InviaAccettazioneSep(int idDip)
        {
            string result = null;
            var db = new IncentiviEntities();
            var dip = db.XR_INC_DIPENDENTI.Find(idDip);
            dip.DTA_ACCETT_AZ = dip.GetField<DateTime?>("DataProtocolloAccettazione");

            List<XR_HRIS_PARAM> listParam = HrisHelper.GetParametriJson<XR_HRIS_PARAM>(HrisParam.IncentiviParametri);
            var param = listParam.FirstOrDefault(x => x.COD_PARAM == "LimiteFirmaAccettazioneAzienda");
            dip.SetField("LimiteFirmaAccettazioneAzienda", dip.DTA_ACCETT_AZ.Value.Date.AddDays(Convert.ToDouble(param.COD_VALUE1)));

            param = listParam.FirstOrDefault(x => x.COD_PARAM == "LimiteRecessoOnline");
            dip.SetField("LimiteRecessoOnline", dip.DTA_ACCETT_AZ.Value.Date.AddDays(Convert.ToDouble(param.COD_VALUE1)));

            var template = CessazioneHelper.GetTemplate(db, "Accettazione", dip.ID_DIPENDENTE, "Accettazione", true, dip.SINTESI1.COD_QUALIFICA);

            myRaiCommonTasks.Helpers.FileResult files = null;
            var fileAcc = InternalGetAllegatiByTags(ref files, idDip, dip.MATRICOLA, db, false, false, new string[] { "Accettazione" }, false, false, (int)IncStato.TempFileAccettazione);
            if (fileAcc != null && fileAcc.Count() == 1)
            {
                var acc = db.XR_INC_OPERSTATI_DOC.Find(fileAcc.FirstOrDefault().ID_ALLEGATO);

                var accTemplate = CessazioneHelper.GetTemplate(db, "Traccia_996", 0, "Accettazione", false);
                IncentivazioneFile fileInfo = new IncentivazioneFile()
                {
                    IdDipendente = dip.ID_DIPENDENTE,
                    Caricato = true,
                    Template = accTemplate.ID_TEMPLATE,
                    Chiave = INCFileManager.GeneraChiave(dip.ID_DIPENDENTE, true, (int)IncStato.TempFileAccettazione, accTemplate.ID_TEMPLATE),
                    FileName = acc.NME_FILENAME,
                    Length = acc.NMB_SIZE.GetValueOrDefault(),
                    Modulo = dip.COD_GRUPPO,
                    Approvato = true,
                    Titolo = "Accettazione",
                    Descrizione = "Accettazione proposta",
                    ReadOnly = true,
                    Tag = "Accettazione",
                    Stato = (int)IncStato.RichiestaAccettata
                };
                var fileResult = FileManager.UploadFile(dip.MATRICOLA, "INC", "Accettazione firmata.pdf", acc.OBJ_OBJECT, fileInfo.Chiave, null, Newtonsoft.Json.JsonConvert.SerializeObject(fileInfo));
                if (fileResult.Esito)
                {
                    GestoreMail mail = new GestoreMail();

                    var paramMail = HrisHelper.GetParametro(HrisParam.IncentiviAbilitaMailAccettazione);
                    var templateMail = CessazioneHelper.GetTemplate(db, "TemplateMail", dip.ID_DIPENDENTE, "Accettazione", false, dip.SINTESI1.COD_QUALIFICA);

                    string oggetto = paramMail.COD_VALUE3;
                    oggetto = CessazioneHelper.ReplaceToken(dip, oggetto);
                    string corpo = CessazioneHelper.ReplaceToken(dip, templateMail.TEMPLATE_TEXT);
                    string destc = CessazioneHelper.GetMailDip(dip); //CommonHelper.GetEmailPerMatricola(dip.MATRICOLA);
                    string mittente = paramMail.COD_VALUE2;
                    string cc = mittente;

                    //var response = mail.InvioMail(corpo, oggetto, destc, cc, mittente, attachments, null, null);
                    var response = mail.InvioMail(corpo, oggetto, destc, cc, mittente, null, null, null);

                    if (response != null && response.Errore != null)
                    {
                        MyRai_LogErrori err = new MyRai_LogErrori()
                        {
                            applicativo = "Portale",
                            data = DateTime.Now,
                            provenienza = "ModuliController - InvioMailIncentivazione",
                            error_message = response.Errore + " per " + destc
                        };

                        using (digiGappEntities dbDG = new digiGappEntities())
                        {
                            dbDG.MyRai_LogErrori.Add(err);
                            dbDG.SaveChanges();
                        }

                        result = "L'accettazione è stata resa disponibile al dipendente, tuttavia si è verificato un errore nell'invio della mail.";
                    }
                    else
                    {
                        dip.DTA_ACCETT_AZ = DateTime.Now;
                        if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                            result = "OK";
                        else
                            result = "L'accettazione è stata resa disponibile al dipendente, tuttavia si è verificato un problema durante il salvataggio";
                    }
                }
                else
                {
                    result = "Non è stato possibile rendere disponibile il file al dipendente";
                }
            }
            else if (fileAcc != null && fileAcc.Count() > 1)
            {
                result = "Sono state trovate più versioni dell'accettazione";
            }
            else
            {
                result = "Accettazione non trovata";
            }

            return Content(result);
        }

        public ActionResult test()
        {
            string output = "";
            bool result = false;
            DateTime ora = DateTime.Now;
            string tmpProt = "";

            try
            {
                //id_ruolo = "42936";

                //URL:  http://raiconnectcoll.servizi.rai.it/rai_ruo_ws.asmx
                string user = "srv_raiconnect_ruo";
                string pwd = "6E6asOXO";
                rai_ruo_ws client = new rai_ruo_ws();
                client.Credentials = new System.Net.NetworkCredential(user, pwd);
                //client.UseDefaultCredentials = true;

                string base64 = "JVBERi0xLjYNJeLjz9MNCjExIDAgb2JqDTw8L0xpbmVhcml6ZWQgMS9MIDk2OTYvTyAxMy9FIDU1MjgvTiAxL1QgOTM5My9IIFsgNDYwIDE1MV0+Pg1lbmRvYmoNICAgICAgICAgICAgICAgICAgICAgDQoxOSAwIG9iag08PC9EZWNvZGVQYXJtczw8L0NvbHVtbnMgNC9QcmVkaWN0b3IgMTI+Pi9GaWx0ZXIvRmxhdGVEZWNvZGUvSURbPEU0Q0M1MEIxNzYxMUZENDhCMzQwQTlEMDA4M0UzOEZGPjxFMTY5NkY3NzkxOTcyQTRFOUZFNUY3MTE4NDUwQjc4Rj5dL0luZGV4WzExIDE1XS9JbmZvIDEwIDAgUi9MZW5ndGggNTgvUHJldiA5Mzk0L1Jvb3QgMTIgMCBSL1NpemUgMjYvVHlwZS9YUmVmL1dbMSAyIDFdPj5zdHJlYW0NCmjeYmJkEGBgYmAKBhIMPkCCcRuI1QUkeDNBrEggwbISSLyRYmBiZJgNEmNgRCf+M+79AxBgAO1ZB+0NCmVuZHN0cmVhbQ1lbmRvYmoNc3RhcnR4cmVmDQowDQolJUVPRg0KICAgICAgICANCjI1IDAgb2JqDTw8L0ZpbHRlci9GbGF0ZURlY29kZS9JIDg4L0xlbmd0aCA2Ny9PIDcyL1MgMzg+PnN0cmVhbQ0KaN5iYGBgY2BgkmAAAqGXDKiAEYhZGDgaFJDE2KCYgSGSgYf1guFBBgZThqdQ1aIzoLrug3UyKN+E8m8CBBgAPysIgw0KZW5kc3RyZWFtDWVuZG9iag0xMiAwIG9iag08PC9NZXRhZGF0YSAyIDAgUi9PdXRsaW5lcyA2IDAgUi9QYWdlcyA5IDAgUi9UeXBlL0NhdGFsb2c+Pg1lbmRvYmoNMTMgMCBvYmoNPDwvQ29udGVudHMgMTQgMCBSL0Nyb3BCb3hbMC4wIDAuMCA2MTIuMCA3OTIuMF0vTWVkaWFCb3hbMC4wIDAuMCA2MTIuMCA3OTIuMF0vUGFyZW50IDkgMCBSL1Jlc291cmNlczw8L0ZvbnQ8PC9DMF8wIDI0IDAgUj4+L1Byb2NTZXRbL1BERi9UZXh0XT4+L1JvdGF0ZSAwL1R5cGUvUGFnZT4+DWVuZG9iag0xNCAwIG9iag08PC9MZW5ndGggODg+PnN0cmVhbQ0KQlQKMCBpIAovUmVsYXRpdmVDb2xvcmltZXRyaWMgcmkKL0MwXzAgMTIgVGYKMTA5LjM5NyA3MzIuMzI5IFRkCjwwMDI0MDA0QTAwNDIwMDUwPlRqCkVUCg0KZW5kc3RyZWFtDWVuZG9iag0xNSAwIG9iag08PC9GaWx0ZXIvRmxhdGVEZWNvZGUvRmlyc3QgMzQvTGVuZ3RoIDMwODEvTiA1L1R5cGUvT2JqU3RtPj5zdHJlYW0NCmjevJlbixzJEYX/Sj1qMaIz8p6wDOiyQlrw2qzWlqGoh1mpPdswmhEzLbD+vc8XWS2tMQt+8sNUZWflJTIuJ07kxLCEJdpS4hLjkkJfYlpqsrTErLe+fP/94S8PH44Pp7ubJ28+HO/Op/OX7w4/H29Oj+eHL0+efbj/9fjd4e3nT59ujx/1eQlXV5rz7PE9P0YfhxdvXr49nher2urnw4vrT6+Pp5vfzkstdnh5nOOephoOr26vbx6Xenh1f3d+/vz+X+vTOIJ/0uSeWWzzj6+uP55uvzz58+nudH+3/PXh/rvZfbo9psWa70PHT9cfj4dffvzxxQ+v/zQHa+xTyf759vrBR7w9PxzP7387/HT/8PH61rveTelyCIc35+vb0/tndze3xyUc3p6PH/++9HD45cunow9F+ofTp/P9w+Ef+6Fyan7+59ePR4b88fao5cuj1nxz98/7JQaX+uW7xYJ2/s/VsRBf337+9czemsoA5PidNId3a1hLkD1jW2KrS7K+5aUsuYelrq3UpZnpa9qGdllSrovZmoPGdGb1JWmMv5Nt0mOsPpfluvfHsRa5CsN5pzaWVuQwQx7U+1JrWVoqS6ldHqVvpt9VgmRbkn7XlpaS+tI1vuWkv6axvMtSdcbc5HTatyWJGtIymslLcET96XfSuJTmu2jdedgspQ+1+5LlvCXyLhJVwiOH5ka9o46W1RclbzeNZ29poWh+kVqS9kq1KQa0dpIskqt2Ka/FJZehNdEYOkn+dpW1uo0mv3Q1jbFaGZvsJ+2ad1mIa25ttutYEkNz1pimbxKqYQkpFaUlhA8SUn996GAILIH0OyYE0B/r6vCjsXZ1C2ymE5g0ltk4tXUokjubcZISpN2hCZIaLeqUrg0soShvWqig1WzTqjZP25KsUHdLa/6lPU/P3Ox907L7XPbrwftaiHIgUzBW9w5rba0Vp+qLdXMPsR71N/e2LgfNabMuxbh38EZ5UtZQUMtyeIUN+rt7hEnh8oot4gXaGy+Ioa45mvpkKVnXPQGnt90LrKyy/BYNRw/uATHaKstvMQKC0/oonzBCF3L8FesXeSAehzGzQivKYFHejB5koJVzDs2r8swa29QhnsxvQ2fI3eTJGBbDEyJSaNYBo1yvaHLBEO4JxV004fZSUq1MkEaHTtKl1YaVkaja/CB/kt6km4YOg4za5S1dYdo1xT1Mh2itz/Wl/MISBYG1pFBgjsMppAw8UJ7H7zhsLU0HHjqoZO0ofEihu+NHGaITdjJAY45ggJBCkUW4nTtOVb2fME9SQJQhS9EcgXsLYzqQjwWNygxjRYiHKLJWHFLrFfO10VnV+7Kej2GsIor+LGVk7d+BoMiZqjsAygdKKsbOGK5MWBEYloTzyriZYAL7eCc3lnBdsu59Cp68Q5z/Zi/py2XuyX8XA/6I0jAcdhE4JQ4sT5VRCO0sbMtDv2WA3ADq7uPcw3SQ0vJso0AUVC8bsGnckgyS/bD6bXEl4pOiNZWyZR084+U+pq0oKQnLbBA5Upai2DHP8VGKkFzy9C3j9Wlitg69esTKITBuzjO6c7Y1CuqyFJgzXKEAazpPmt9L/jYP3C1t7+9zHngqw/u8Cu7v6zb7Ng8l6/yzv8x5DeW3Oa+hi33d3r/NAz4Vid4vBPF56BSHZR76HpcoVGRjgwq0ei7aSpBu1PacFrRukJd20web8OgfbGI1oV0EHyQjtbfiCUjtQjQL8qQwbwNZ3TaPiKQ1SKJEYSKY24xOKb36d/2+zAMN8u7ZOekw2kOKBrI7HpvJ60qUMbo8ZH03RC6bJ2p9I8cUGYTcTrooRKIbZEZgkTFmQg0OJh4JVYqL2qsCMNmjStG7lk6kEYkQANYrnq+aoI25fgbgH9mTw/xWQBwYiJMEWwVcm0eroxopQ2dIkrejp5kaCg4K5MuJy2ir6xYdhplyFMHrTii2KqevIEXiu5Bo2FalLyAXklENUlE9pYhcKM0MfddY6Qv00B9pBsfaKuwXQgERiaBVdUICnFfQ2m0UJK/21fnEkPc+WHOdhAWIV7A4aZEd65RX77gqLW8VmWQjB+0K88nNobPmvsrBNGC4g8BmakmrYGVzFiSlks9qIR9l9WlcnVAkCBeJUB8QS57zviGHAVHqRt6HfcF+astrbECjhCE/t8mGKkbaYbR27Sunh3bXXh0pam+rMyQZowruqgzPbwxbR3S+UYHl0d34VbApHuAsCjbXyNFwiwC7a86omiKsTua1AZcTKaVUBVrDEEo1LQLh3dNHEwMVnG9NUN3cwWCcccXRGqlLSp99RcHHuOp5GHla6q7LRprKYfYJyZR2tqYggyeBtK2EVciytYIjl7leqascZcOxcX7QVOnI+Qn5nzQ++xRocsbfpTh9V05uNlNdg9N0Ut7W5GRtD44GkybtSd9NATEDRGsrI3hKjbzJ79HTYhtpFeJvTajWQDTv0/5jslp4QHdiD0sofsYumToo532AmnlK7QZFqJ5Wu5wddgxi9ig9QDhDdOKZSHO8CfowbTO/f3vjD7QvFYEjaZ5BTRAAmOjfg7gl961LNVA1liCVnbwq8IAn+Ar6z14JuC+UHZ3Rq3yxc4adi/aaFOBNfQJWYkFzewUk6dPZ2qxWunTfpYchXXVl1N7nmr1TncwKpkufI5jLPMIcOxrvycK8XwAF52PvIZAYAELhLVCG1wYdasi5Rp6CD6HrKLP0GULl4oTatiE0HZQcEnjgRHvJ1BVog6AhUEXmq8+9KDhMJZOV8nChtSZEcxsUJBBCAcnocfW5ZI0xDzQVXVzRDhpGxkruPJcUmZ17kgaHo7zSJ9WNk5pJaNSIq9cMJB78jUQVSJBl0iimd6gTGbe4CJNSNa/G8EW297nBMWspO5NAnOycm6IqQmopQbUCzFnbBnUnpEmT76mhg+q753mlW3UwmAQaETb7o/uqasCoSYbBG6xf875+5dFmfbCza+cb3mB4r34WNbyS6/sO3eu5sO9A4RNIZb6D62tcdhj+qPsOlDVhXHagOKCM9B2oJY1i0XcwljLbpTZZzaj/5sjII4V9ZGIeJvaRFE8mx/HdTSrRo8z9zC8CoAO+ldKQK89QkJEwfArOGtPUqxVmUBuhf+OChmrPK1ATxKWiUs4aUlD9zu7OA6LFah1s2d0Z3yC2cWtiSjlrgJXKDx2c7uYJkxAhyc5qf7Le1KdrZs8R+GBXPaxg1UOeo2BSI/utCASQfAwncjeFH5FiOJSTU/hE9ILfORJwpEO1Ni8bKvAHAYebAaNhbsvbxYB4K41ZRPExRU/Cajhtc7Rr3DukvYqE5pD9+8y+ZCoQmCxMtMBzEwXHfi0Dz++gj5DLTS6TCmPgBOChlzTCMY8HB3pZiKxgzbnGIPaSH6SL9HTICkNgVFZ1RPPqS1HWh0r3SA9skqsZNdqaHSQmEDHMbwxYzYdrrh67qaPoeLscadaxGjIwyl7cGOWjTcJgNCQJl45+LZCMb7ZfDCTjW5ypQQ0GKMn7ZUAivJPzLaYkvpHE5SByfG5DdsqkBt9wWN8Bh0W5HnzkLIPgFB9JjZT8oord0XYiSfvuhL6nweINRkJ4Et84kae+qXl9y36iQoNvXHAxJUd/TMKgBt/QVPeGloLBFx+J8rNfVBkNiY6xXcnUUgnduyxeJXp3pxr5/9bff1R7X+puLmmM2w8rcVI2NaLzOHOo8ju42V2cyhlFlJWd9BmFtlGdu030cB5nVFI2yyjvzk5zjGLKqJpc25RNsDkjrehxWTIPJ3TGDZl5LYM1VBj9F38zv2KhDEJZRh10oXCGWqy0mYvVYEOndOzVxlceZ4CYHvN+0ub3PnmbUQFB5oxMaWT0vZtTjz0JURrpMembGn0/LLmihmkkNYxHdlMZCVePMd2HmkiPOBM31ZHVi96pdYxixlVGNWOElKukos7qN56ZhiSDpMz9yAOVItJ3KGO9wBZ8wq8jyn6RFvwMfrE2NS3tVa4xq2dX9oAbj51vV3jWtEGM+41Itf02pO43JHECcq/bdnWlgpVL8u1/unyf/3f4cH13Ztwj//Xggv2Hu/f3H053N4fLfzqevv566b7ftt//7e6kQcdF/syUr9fvV1f/FmAAjuEuLQ0KZW5kc3RyZWFtDWVuZG9iag0xNiAwIG9iag08PC9GaWx0ZXIvRmxhdGVEZWNvZGUvTGVuZ3RoIDIwPj5zdHJlYW0NCkiJamBgYOAAYgWFBoAAAwAG4wFJDQplbmRzdHJlYW0NZW5kb2JqDTE3IDAgb2JqDTw8L0ZpbHRlci9GbGF0ZURlY29kZS9MZW5ndGggODQ1L1N1YnR5cGUvQ0lERm9udFR5cGUwQz4+c3RyZWFtDQpIiXyTbUwTSRjHd61li256Cq45WeiOBu4+WLDiG5zRKBwqKNpASTyj8QpdYL22W3a3B0tArFolklN5q6UeZxutJ2cvGDTqnUeIiQnxk4nGxBjfYvzA5dSoMZmBqfF21y+XmNx8+GV+88z8J/MkQxKzZxEkSS5yVVWVV2xZWi34BdHvlMTCGr4p6HVLerEQsSi3h7ahPBJ9ORvZaAZfxencjIGZFvMjegnK/gItmY9y6YIZK51PmLVAIoOwEh9Jmszb6BHr+UoP71cERS0XA6okNDUrYHlpqcOuc7nBYoMrDa4xWGIHxQ6Hw2CxwZXAyAK1qqzwPhlU+htEKSBKboX3FIGNXi8womUg8TIv/agvfnoNEGTAC0ozLwG3VmwStPMS7wGK5PbwPrf0AxD1yn+08X+uAoIfaFmgzi/oVqtoizJw+z3LtBTRuKVBDPoVSeDlomWbal1qgAclwMM3ft5bbeQTZUQV4SQ+DaN3NcRFsoKMkk9Qd884ejlOaswfN6HuhdPOdPcHJ4VKpm0MXo/78XrYb0YZ6QlGn0Hd0rpBw7BuSDd9p2bW6cKOUfR+lLz21oRm4LfM19/VuQpYbMez4AK4A9pfwMVwLSQmv1+c5Eb6wjfDlw/dWnRS3Jz4hsUyXo053IhXQBJvgD/de3qm71duOHXu/tAfFiva2jUJp6+RY8/g0gcmVDHGhJLmcKjzSBfbrsauc3A7NRG60nzBNhz0DfjYmgNVHrHV0ta5/0BHjpJsHR6K9J0+Yev/bSARjVpODcb6z7Djv9eXcyoVVMsCO1otrrYW766cbX85747cuHT+su1ke29ntNMSisQPxtiyf5g2tbm95ZjF23BuYjTxy9gwFx+8EY9HLFYY70mida3v32U9e7PmYfYbyKIUcyFx9vxIMCEG5KAvcFZOctlTMDOdYpLU1t7DvXdy4Ctqr7p7/85DFpmaDB0/Wp2D66nNh8PbwrYWyor6ulKIS5GXpuDxKRMcQHWMAy/YgEux6bHWybzXcM4DuArOK3qN53GhfczzP+04E+ft3FNd6vobZkHzzdu3Oav+sTIRR3+VmmGzOmKo8nRkCBZEh2KjkQw8+DNli286upfO7KHnXpzzcO6/AgwA/K2HoQ0KZW5kc3RyZWFtDWVuZG9iag0xOCAwIG9iag08PC9GaWx0ZXIvRmxhdGVEZWNvZGUvTGVuZ3RoIDI0OD4+c3RyZWFtDQpIiVyQ3WrEIBCF732Kudy9WEzStFAIwrKlkIv+0LQPYHSSCo2KMRd5+466bKEDCh8z58xh+KV/6q2JwN+DUwNGmIzVAVe3BYUw4mwsqxvQRsUr5V8t0jNO4mFfIy69nRzrOuAf1Fxj2OFw1m7EI+NvQWMwdobD12U4Ah82739wQRuhAiFA40RGL9K/ygWBZ9mp19Q3cT+R5m/ic/cITea6hFFO4+qlwiDtjKyrqAR0z1SCodX/+m1RjZP6liFNNy1NV1V7JxK1TaaHutC50GOm+6pQ8b06pA10CLjFV1sIlDxfK0dOYY3F20G980Cq9NivAAMAqsp3yA0KZW5kc3RyZWFtDWVuZG9iag0xIDAgb2JqDTw8L0ZpbHRlci9GbGF0ZURlY29kZS9GaXJzdCAxNC9MZW5ndGggMTI5L04gMy9UeXBlL09ialN0bT4+c3RyZWFtDQpo3jJTMFAwVzCxVLBQsDRRsLHRd84vzStRMNR3yywqLgFKGSgE6fskwpkhlQWp+v6lJTmZeanFdnZADY5ArSCZgMSiVKBOM4iyzJKcVI2AxPTMvESFstL8kkRNsGKXaENjsIKIyCiQzZZGCnmlOTmx+sH67vkh+XZ2AAEGAA+vKFINCmVuZHN0cmVhbQ1lbmRvYmoNMiAwIG9iag08PC9MZW5ndGggMzIwOC9TdWJ0eXBlL1hNTC9UeXBlL01ldGFkYXRhPj5zdHJlYW0NCjw/eHBhY2tldCBiZWdpbj0i77u/IiBpZD0iVzVNME1wQ2VoaUh6cmVTek5UY3prYzlkIj8+Cjx4OnhtcG1ldGEgeG1sbnM6eD0iYWRvYmU6bnM6bWV0YS8iIHg6eG1wdGs9IkFkb2JlIFhNUCBDb3JlIDcuMS1jMDAwIDc5LjQyNWRjODcsIDIwMjEvMTAvMjctMTY6MjA6MzIgICAgICAgICI+CiAgIDxyZGY6UkRGIHhtbG5zOnJkZj0iaHR0cDovL3d3dy53My5vcmcvMTk5OS8wMi8yMi1yZGYtc3ludGF4LW5zIyI+CiAgICAgIDxyZGY6RGVzY3JpcHRpb24gcmRmOmFib3V0PSIiCiAgICAgICAgICAgIHhtbG5zOnhtcD0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wLyIKICAgICAgICAgICAgeG1sbnM6ZGM9Imh0dHA6Ly9wdXJsLm9yZy9kYy9lbGVtZW50cy8xLjEvIgogICAgICAgICAgICB4bWxuczp4bXBNTT0iaHR0cDovL25zLmFkb2JlLmNvbS94YXAvMS4wL21tLyIKICAgICAgICAgICAgeG1sbnM6cGRmPSJodHRwOi8vbnMuYWRvYmUuY29tL3BkZi8xLjMvIj4KICAgICAgICAgPHhtcDpNb2RpZnlEYXRlPjIwMjItMDItMTZUMTI6NTM6NTArMDE6MDA8L3htcDpNb2RpZnlEYXRlPgogICAgICAgICA8eG1wOkNyZWF0ZURhdGU+MjAyMi0wMi0xNlQxMjo1MzoxNSswMTowMDwveG1wOkNyZWF0ZURhdGU+CiAgICAgICAgIDx4bXA6TWV0YWRhdGFEYXRlPjIwMjItMDItMTZUMTI6NTM6NTArMDE6MDA8L3htcDpNZXRhZGF0YURhdGU+CiAgICAgICAgIDx4bXA6Q3JlYXRvclRvb2w+QWRvYmUgQWNyb2JhdCBQcm8gREMgKDY0LWJpdCkgMjEuMTEuMjAwMzk8L3htcDpDcmVhdG9yVG9vbD4KICAgICAgICAgPGRjOmZvcm1hdD5hcHBsaWNhdGlvbi9wZGY8L2RjOmZvcm1hdD4KICAgICAgICAgPHhtcE1NOkRvY3VtZW50SUQ+dXVpZDowYjNmYWZhMi1hNDc0LTQ5MzctYjM4Yy05YTQ1ODFlZGRlMzk8L3htcE1NOkRvY3VtZW50SUQ+CiAgICAgICAgIDx4bXBNTTpJbnN0YW5jZUlEPnV1aWQ6MmU5OWZjZmMtNmY4OS00ZjI5LTk0YmMtZmY1ZTc5ZDM2NmFlPC94bXBNTTpJbnN0YW5jZUlEPgogICAgICAgICA8cGRmOlByb2R1Y2VyPkFkb2JlIEFjcm9iYXQgUHJvIERDICg2NC1iaXQpIDIxLjExLjIwMDM5PC9wZGY6UHJvZHVjZXI+CiAgICAgIDwvcmRmOkRlc2NyaXB0aW9uPgogICA8L3JkZjpSREY+CjwveDp4bXBtZXRhPgogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgIAogICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgCiAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAgICAKICAgICAgICAgICAgICAgICAgICAgICAgICAgCjw/eHBhY2tldCBlbmQ9InciPz4NCmVuZHN0cmVhbQ1lbmRvYmoNMyAwIG9iag08PC9GaWx0ZXIvRmxhdGVEZWNvZGUvRmlyc3QgNC9MZW5ndGggNDkvTiAxL1R5cGUvT2JqU3RtPj5zdHJlYW0NCmjeslQwULCx0XfOL80rUTDU985MKY42NAYKBsXqh1QWpOoHJKanFtvZAQQYAOA1C68NCmVuZHN0cmVhbQ1lbmRvYmoNNCAwIG9iag08PC9GaWx0ZXIvRmxhdGVEZWNvZGUvRmlyc3QgNS9MZW5ndGggMTI0L04gMS9UeXBlL09ialN0bT4+c3RyZWFtDQpo3jI0UDBQsLHRdy5KTSzJzM9zSSxJ1XCxMjIwAiJDM0MjU2NDU20DQ3UDA3VNiKr8Ig3HlPykVAXH5KL8pMQShYCifAUXZ4UYDTMT3aTMkhhNBSNDPUNDPSMDA2NLTX3f/BQsppoawE0F6k8pTU4lzVg7O4AAAwB6ATH0DQplbmRzdHJlYW0NZW5kb2JqDTUgMCBvYmoNPDwvRGVjb2RlUGFybXM8PC9Db2x1bW5zIDQvUHJlZGljdG9yIDEyPj4vRmlsdGVyL0ZsYXRlRGVjb2RlL0lEWzxFNENDNTBCMTc2MTFGRDQ4QjM0MEE5RDAwODNFMzhGRj48RTE2OTZGNzc5MTk3MkE0RTlGRTVGNzExODQ1MEI3OEY+XS9JbmZvIDEwIDAgUi9MZW5ndGggNDgvUm9vdCAxMiAwIFIvU2l6ZSAxMS9UeXBlL1hSZWYvV1sxIDIgMV0+PnN0cmVhbQ0KaN5iYgACJkbRGQxMDIz3gQTvVSDB0Avi3gRK3PEHcRkYYQTTPyDByAAQYACxsAYdDQplbmRzdHJlYW0NZW5kb2JqDXN0YXJ0eHJlZg0KMTE2DQolJUVPRg0K";

                tmpProt = client.inserisciAllegato("42900", "P103650", "29415502", "test.pdf", "test", base64, "1", "1");
                InserisciAllegato outputServizio = new InserisciAllegato();

                var p = SerializerHelper.DeserializeXml(tmpProt, outputServizio.GetType());
                outputServizio = (InserisciAllegato)p;

                if (outputServizio.Errore != null &&
                    outputServizio.Errore.Id_errore != "0" &&
                    !String.IsNullOrEmpty(outputServizio.Errore.Text))
                {
                    throw new Exception(outputServizio.Errore.Text);
                }

                result = true;
                output = tmpProt;
            }
            catch (Exception ex)
            {
                output = ex.Message;
            }

            return Content(output);
        }

        [HttpPost]
        public ActionResult ResetProposta(int idDip)
        {
            string result = "";

            var db = new IncentiviEntities();
            var dip = db.XR_INC_DIPENDENTI.Find(idDip);

            DateTime scadenza = DateTime.Now;

            //Controllo se è già presente una proposta accettata
            myRaiCommonTasks.Helpers.FileResult files = null;
            var fileProp = InternalGetAllegatiByTags(ref files, dip.ID_DIPENDENTE, dip.MATRICOLA, db, false, false, new string[] { "Proposta" }, true, true, (int)IncStato.TempFileProposta);
            if (fileProp != null && fileProp.Any())
            {
                bool hasVersionAccettata = false;
                var incFile = fileProp.FirstOrDefault(x => !x.IsExternal && x.ElencoTag.Contains("Rai per Me"));
                if (incFile != null)
                {
                    hasVersionAccettata = true;
                    var dbFile = db.XR_INC_OPERSTATI_DOC.Find(incFile.ID_ALLEGATO);
                    dbFile.VALID_DTA_END = scadenza;
                }

                /*
                 * Se non c'è la versione accettata su czn (perchè non è stata accettata o perchè assente)
                 * verifica che sia presente una versione su raiperme.
                 * Se c'è, la importa su czn, ovviamente con scadenza oggi
                 */
                if (!hasVersionAccettata)
                {
                    var rpmFile = fileProp.FirstOrDefault(x => x.IsExternal);
                    if (rpmFile != null)
                    {
                        var file = FileManager.GetFile(rpmFile.FileId.Value);
                        if (file.Esito)
                        {
                            IncentivazioneFile info = Newtonsoft.Json.JsonConvert.DeserializeObject<IncentivazioneFile>(file.Files[0].Json);

                            int idOper = SalvaStato(info.IdDipendente, (int)IncStato.TempFileProposta);

                            XR_INC_OPERSTATI_DOC doc = new XR_INC_OPERSTATI_DOC();
                            doc.ID_ALLEGATO = db.XR_INC_OPERSTATI_DOC.GeneraPrimaryKey();
                            doc.ID_OPER = idOper;
                            doc.NME_FILENAME = file.Files[0].NomeFile;
                            doc.COD_TITLE = info.Titolo;
                            doc.DES_ALLEGATO = info.Descrizione;
                            doc.CONTENT_TYPE = file.Files[0].MimeType;
                            doc.OBJ_OBJECT = file.Files[0].ContentByte;
                            doc.NMB_SIZE = file.Files[0].Length;
                            doc.IND_RILEVANTE = true;
                            doc.COD_USER = file.Files[0].MatricolaCreatore;
                            doc.COD_TERMID = "RAIPERME";
                            doc.NOT_TAG = "Rai per Me" + (!String.IsNullOrWhiteSpace(info.Tag) ? ";" + info.Tag : "");
                            doc.TMS_TIMESTAMP = file.Files[0].DataCreazione;
                            doc.VALID_DTA_INI = scadenza;
                            doc.VALID_DTA_END = scadenza;
                            db.XR_INC_OPERSTATI_DOC.Add(doc);
                        }
                        else
                            ;//result = file.Errore;
                    }
                }
            }

            /*
             *  Backup della proposta inviata e della mail 
             */
            var templProposta = db.XR_INC_TEMPLATE.FirstOrDefault(w => w.ID_DIPENDENTE == dip.ID_DIPENDENTE && w.COD_TIPO == "PropostaPDF" && w.COD_USER == "ADMIN");
            if (templProposta != null)
                templProposta.VALID_DTA_END = scadenza;

            var templateMail = db.XR_INC_TEMPLATE.Where(w => w.ID_DIPENDENTE == dip.ID_DIPENDENTE && w.COD_TIPO == "Mail");
            foreach (var item in templateMail)
                item.VALID_DTA_END = scadenza;

            /*
             * Creo una nota come promemoria che in data x è stato resettato l'invio della proposta y
             */
            string nota = $"Annullata proposta inviata il {dip.DATA_INVIO_PROP.Value.ToString("dd/MM/yyyy")}";
            if (dip.DATA_FIRMA_DIP.HasValue)
            {
                nota += $", firmata dal dipendente il {dip.DATA_FIRMA_DIP.Value.ToString("dd/MM/yyyy")})";
            }
            CessazioneHelper.AggiungiNotaPratica(db, dip.ID_DIPENDENTE, nota, CommonHelper.GetCurrentIdPersona(), false, CommonHelper.GetCurrentUserMatricola());

            dip.DATA_FIRMA_DIP = null;
            dip.DATA_INVIO_PROP = null;
            dip.NOT_INVIO_PROP = null;

            if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
            {
                /*
                 * Una volta effettuato su czn l'eventuale backup
                 * Pulisco la situazione su rpm
                 */
                bool hasError = false;
                result = "";
                foreach (var item in fileProp.Where(x => x.IsExternal))
                {
                    var res = FileManager.DeleteFile(item.FileId.Value);
                    if (!res.Esito)
                    {
                        hasError = true;
                        result += res.Errore + "<br/>";
                    }
                }
                if (!hasError)
                    result = "OK";
            }
            else
                result = "Errore durante il salvataggio";


            return Content(result);
        }

        [HttpPost]
        public ActionResult ResetAccettazione(int idDip)
        {
            string result = "";

            var db = new IncentiviEntities();
            var dip = db.XR_INC_DIPENDENTI.Find(idDip);

            DateTime scadenza = DateTime.Now;


            myRaiCommonTasks.Helpers.FileResult files = null;
            var fileAccett = InternalGetAllegatiByTags(ref files, dip.ID_DIPENDENTE, dip.MATRICOLA, db, false, false, new string[] { "Accettazione" }, false, false, (int)IncStato.TempFileAccettazione);
            var incFile = fileAccett.FirstOrDefault();
            if (incFile != null)
            {
                var dbFile = db.XR_INC_OPERSTATI_DOC.Find(incFile.ID_ALLEGATO);
                dbFile.VALID_DTA_END = scadenza;
            }

            if (dip.DTA_ACCETT_AZ.HasValue)
            {
                string nota = $"Annullata accettazione generata il {dip.DTA_ACCETT_AZ.Value.ToString("dd/MM/yyyy")}";
                if (dip.DATA_FIRMA_DIP_ACCETT_AZ.HasValue)
                {
                    nota += $", firmata dal dipendente il {dip.DATA_FIRMA_DIP_ACCETT_AZ.Value.ToString("dd/MM/yyyy")})";
                }
                CessazioneHelper.AggiungiNotaPratica(db, dip.ID_DIPENDENTE, nota, CommonHelper.GetCurrentIdPersona(), false, CommonHelper.GetCurrentUserMatricola());
            }

            dip.SetField("BCK_DTA_ACCETT_AZ", dip.DTA_ACCETT_AZ);
            dip.SetField("BCK_DATA_FIRMA_DIP_ACCETT_AZ", dip.DATA_FIRMA_DIP_ACCETT_AZ);
            dip.SetField("BCK_DataProtocolloAccettazione", dip.GetField<DateTime?>("DataProtocolloAccettazione"));
            dip.SetField("BCK_AccettazioneBase64", dip.GetField<string>("AccettazioneBase64"));
            dip.DTA_ACCETT_AZ = null;
            dip.DATA_FIRMA_DIP_ACCETT_AZ = null;
            dip.SetField<DateTime?>("DataProtocolloAccettazione", null);
            dip.SetField<string>("AccettazioneBase64", null);

            if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
            {
                /*
                 * Una volta effettuato su czn l'eventuale backup
                 * Pulisco la situazione su rpm
                 */
                bool hasError = false;
                result = "";

                var resFileRPM = CessazioneManager.InternalGetAllegatiDip(ref files, idDip, dip.MATRICOLA, (int)IncStato.TempFileAccettazione, (int)IncStato.TempFileAccettazione, "Accettazione", false, true);
                if (resFileRPM != null && resFileRPM.Any())
                {
                    foreach (var item in resFileRPM)
                    {
                        var res = FileManager.DeleteFile(item.FileId.Value);
                        if (!res.Esito)
                        {
                            hasError = true;
                            result += res.Errore + "<br/>";
                        }
                    }
                }

                if (!hasError)
                    result = "OK";
            }
            else
                result = "Errore durante il salvataggio";

            return Content(result);
        }

        public ActionResult InviaAccettazione(int idDip)
        {
            string result = null;
            var db = new IncentiviEntities();
            var dip = db.XR_INC_DIPENDENTI.Find(idDip);
            dip.DTA_ACCETT_AZ = DateTime.Now;

            string[] rifGruppo = HrisHelper.GetParametri<string>(HrisParam.IncentiviRifGruppo);
            var codGruppo = rifGruppo[0];
            var annoRif = rifGruppo[1];

            List<XR_HRIS_PARAM> listParam = HrisHelper.GetParametriJson<XR_HRIS_PARAM>(HrisParam.IncentiviParametri);
            var param = listParam.FirstOrDefault(x => x.COD_PARAM == "LimiteFirmaAccettazioneAzienda");
            dip.SetField("LimiteFirmaAccettazioneAzienda", dip.DTA_ACCETT_AZ.Value.Date.AddDays(Convert.ToDouble(param.COD_VALUE1)));

            param = listParam.FirstOrDefault(x => x.COD_PARAM == "LimiteRecessoOnline");
            dip.SetField("LimiteRecessoOnline", dip.DTA_ACCETT_AZ.Value.Date.AddDays(Convert.ToDouble(param.COD_VALUE1)));

            var template = CessazioneHelper.GetTemplate(db, "Accettazione", dip.ID_DIPENDENTE, "Accettazione", true, dip.SINTESI1.COD_QUALIFICA);

            string currentMatr = CommonHelper.GetCurrentUserMatricola();
            string currentPMatr = CommonHelper.GetCurrentUserPMatricola();
            string idRuolo = template.DES_TEMPLATE;
            string nominativoDestinatario = dip.SINTESI1.Nominativo().TitleCase();
            string protOgg = String.Format("Esodi {0} - Accettazione proposta {1}", annoRif, nominativoDestinatario);
            CreaProtocollo outputServizio;

            bool creaProt = false;
            int versione = -1;
            string oldProt = dip.GetField<string>("ProtocolloIdDocumento", null);
            if (!String.IsNullOrWhiteSpace(oldProt))
            {
                outputServizio = new CreaProtocollo()
                {
                    Identificativo = oldProt,
                    Id_documento = dip.GetField("ProtocolloIdDocumento", ""),
                };
                versione = dip.GetField<int>("ProtocolloVersione", -1);

                creaProt = true;
            }
            else
            {
                creaProt = DaFirmareManager.CreaProtocollo(idRuolo, protOgg, currentMatr, dip.MATRICOLA, currentPMatr, out outputServizio);
            }

            if (!creaProt)
                result = "Errore durante la creazione del protocollo";
            else
            {
                #region AcroFields
                PdfReader reader = new PdfReader(template.TEMPLATE);
                MemoryStream ms = new MemoryStream();
                PdfStamper stamper = new PdfStamper(reader, ms, '4');

                stamper.AcroFields.SetField("fieldProtocollo", outputServizio.Identificativo);
                stamper.AcroFields.SetField("fieldNome", (dip.SINTESI1.COD_SESSO == "M" ? "Sig." : "Sig.ra") + " " + (dip.SINTESI1.DES_COGNOMEPERS + " " + dip.SINTESI1.DES_NOMEPERS).TitleCase());
                stamper.AcroFields.SetField("fieldDataAccettazione", dip.DTA_ACCETT_AZ.Value.ToString("dd/MM/yyyy"));
                stamper.AcroFields.SetField("fieldDataRich", dip.DATA_FIRMA_DIP.Value.ToString("dd/MM/yyyy"));
                stamper.AcroFields.SetField("fieldDataCessazione", dip.DATA_CESSAZIONE.Value.ToString("dd/MM/yyyy"));

                stamper.Writer.CloseStream = false;
                stamper.FormFlattening = true;
                stamper.Close();

                ms.Position = 0;
                byte[] output = ms.ToArray();
                ms.Flush();
                reader.Close();
                #endregion

                dip.SetField("ProtocolloAccettazione", outputServizio.Identificativo);
                dip.SetField("DataProtocolloAccettazione", DateTime.Now);
                dip.SetField("ProtocolloIdDocumento", outputServizio.Id_documento);

                string base64String = Convert.ToBase64String(output, 0, output.Length);

                dip.SetField("AccettazioneBase64", base64String);

                versione++;
                dip.SetField("ProtocolloVersione", versione);

                bool insProt = DaFirmareManager.InserisciAllegatoProtocollo(idRuolo, currentPMatr, outputServizio.Id_documento, protOgg + ".pdf", protOgg, base64String, "1", versione.ToString());
                if (!insProt)
                    result = "Errore durante il caricamento del protocollo";
                else
                {
                    #region AggiuntaFileSuInc
                    int idOper = CessazioneHelper.SalvaStato(db, dip.ID_DIPENDENTE, (int)IncStato.TempFileAccettazione, saveState: false);

                    CezanneHelper.GetCampiFirma(out string codUser, out string termid, out var tms);
                    var doc = new myRaiData.Incentivi.XR_INC_OPERSTATI_DOC();
                    doc.ID_ALLEGATO = db.XR_INC_OPERSTATI_DOC.GeneraPrimaryKey();
                    doc.ID_OPER = idOper;
                    doc.NME_FILENAME = protOgg + ".pdf";
                    doc.COD_TITLE = "Accettazione proposta firmata";
                    doc.DES_ALLEGATO = "Prot. " + outputServizio.Identificativo;
                    doc.CONTENT_TYPE = "application/pdf";
                    doc.OBJ_OBJECT = output;
                    doc.NMB_SIZE = output.Length;
                    doc.NOT_TAG = "Accettazione";
                    doc.IND_RILEVANTE = true;
                    doc.COD_USER = codUser;
                    doc.COD_TERMID = termid;
                    doc.TMS_TIMESTAMP = dip.DTA_ACCETT_AZ.Value;
                    db.XR_INC_OPERSTATI_DOC.Add(doc);
                    #endregion

                    if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                    {
                        #region AggiuntaFileRPM
                        var accTemplate = CessazioneHelper.GetTemplate(db, "Traccia_996", 0, "Accettazione", false);
                        IncentivazioneFile fileInfo = new IncentivazioneFile()
                        {
                            IdDipendente = dip.ID_DIPENDENTE,
                            Caricato = true,
                            Template = accTemplate.ID_TEMPLATE,
                            Chiave = INCFileManager.GeneraChiave(dip.ID_DIPENDENTE, true, (int)IncStato.TempFileAccettazione, accTemplate.ID_TEMPLATE),
                            FileName = protOgg + ".pdf",
                            Length = output.Length,
                            Modulo = dip.COD_GRUPPO,
                            Approvato = true,
                            Titolo = "Accettazione",
                            Descrizione = "Accettazione proposta",
                            ReadOnly = true,
                            Tag = "Accettazione",
                            Stato = (int)IncStato.RichiestaAccettata
                        };
                        FileManager.UploadFile(dip.MATRICOLA, "INC", "Accettazione firmata.pdf", output, fileInfo.Chiave, null, Newtonsoft.Json.JsonConvert.SerializeObject(fileInfo));
                        #endregion

                        #region InvioMail
                        GestoreMail mail = new GestoreMail();

                        //List<Attachement> attachments = new List<Attachement>();

                        //Attachement a = new Attachement()
                        //{
                        //    AttachementName = protOgg + ".pdf",
                        //    AttachementType = "Application/PDF",
                        //    AttachementValue = output
                        //};
                        //attachments.Add(a);
                        var paramMail = HrisHelper.GetParametro(HrisParam.IncentiviAbilitaMailAccettazione);
                        var templateMail = CessazioneHelper.GetTemplate(db, "TemplateMail", dip.ID_DIPENDENTE, "Accettazione", false, dip.SINTESI1.COD_QUALIFICA);

                        string oggetto = paramMail.COD_VALUE3;
                        oggetto = CessazioneHelper.ReplaceToken(dip, oggetto);
                        string corpo = templateMail.TEMPLATE_TEXT;
                        string destc = CessazioneHelper.GetMailDip(dip); //CommonHelper.GetEmailPerMatricola(dip.MATRICOLA);
                        string mittente = paramMail.COD_VALUE2;
                        string cc = mittente;

                        //var response = mail.InvioMail(corpo, oggetto, destc, cc, mittente, attachments, null, null);
                        var response = mail.InvioMail(corpo, oggetto, destc, cc, mittente, null, null, null);

                        if (response != null && response.Errore != null)
                        {
                            MyRai_LogErrori err = new MyRai_LogErrori()
                            {
                                applicativo = "Portale",
                                data = DateTime.Now,
                                provenienza = "ModuliController - InvioMailIncentivazione",
                                error_message = response.Errore + " per " + destc
                            };

                            using (digiGappEntities dbDG = new digiGappEntities())
                            {
                                dbDG.MyRai_LogErrori.Add(err);
                                dbDG.SaveChanges();
                            }

                            result = "L'accettazione è stata caricata correttamente, tuttavia si è verificato un errore nell'invio della mail.";
                        }
                        else
                            result = "OK";
                        #endregion
                    }
                    else
                        result = "Errore durante il salvataggio";
                }
            }

            return Content(result);
        }

        public ActionResult ReloadContData(int idDip)
        {
            var db = new IncentiviEntities();
            var dip = db.XR_INC_DIPENDENTI.Find(idDip);
            dip.NUM_MENS_PRINC = null;
            dip.NUM_MENS_AGG = null;
            TaskHelper.AddBatchRunnerTask("IncentiviFNL930OnDemand", out var taskErr, note: String.Format("Matricola {0} - IdDip: {1}", dip.MATRICOLA, dip.ID_DIPENDENTE));
            if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                return Content("OK");
            else
                return Content("Errore durante il salvataggio");
        }

        public ActionResult ModificaDatiAvviaPratica(CessazioneModel modDip, bool prosegui, bool _stato, string _nota, string _oper, bool _inviaMail)
        {
            string result = "";
            bool rimuoviRichiesta = false;

            var param = HrisHelper.GetParametriJson<XR_HRIS_PARAM>(HrisParam.IncentiviParametri);
            var tmpQualExFissa = param.Where(x => x.COD_PARAM == "QualificheExFissa").Select(x => x.COD_VALUE1).FirstOrDefault() ?? "M,A7";
            string[] qualExFissa = tmpQualExFissa.Split(',');

            IncentiviEntities db = new IncentiviEntities();
            if (!ModelState.IsValid)
                result = "Errore nei dati";
            else
            {
                XR_INC_DIPENDENTI dip = db.XR_INC_DIPENDENTI.Find(modDip.Pratica.ID_DIPENDENTE);
                if (dip == null)
                    result = "Dipendente non trovato";
                else
                {
                    bool runConv = false;
                    if (dip.DATA_CESSAZIONE != modDip.Pratica.DATA_CESSAZIONE
                        || dip.INCENTIVO_LORDO != modDip.Pratica.INCENTIVO_LORDO
                        || dip.UNA_TANTUM_LORDA != modDip.Pratica.UNA_TANTUM_LORDA
                        || dip.TFR_LORDO_AZ_IP != modDip.Pratica.TFR_LORDO_AZ_IP
                        || dip.TFR_LORDO_INPS_IP != modDip.Pratica.TFR_LORDO_INPS_IP
                        || dip.TFR_NETTO != modDip.Pratica.TFR_NETTO
                        || dip.ALIQ_TFR != modDip.Pratica.ALIQ_TFR
                        || dip.EX_FISSA != modDip.Pratica.EX_FISSA
                        || dip.NUM_MENS_PRINC_DEC != modDip.Pratica.NUM_MENS_PRINC_DEC
                        || dip.NUM_MENS_AGG_DEC != modDip.Pratica.NUM_MENS_AGG_DEC)
                    {
                        //Dato che sono cambiati i dati fondamentali per le proposta,
                        //questa deve essere cancellata e rigenerate
                        var templProposta = db.XR_INC_TEMPLATE.FirstOrDefault(w => w.ID_DIPENDENTE != null && w.ID_DIPENDENTE == dip.ID_DIPENDENTE && w.COD_TIPO == "PropostaPDF" && w.COD_USER == "ADMIN");
                        if (templProposta != null)
                        {
                            db.XR_INC_TEMPLATE.Remove(templProposta);
                            runConv = true;
                        }
                    }

                    if (dip.DATA_CESSAZIONE != modDip.Pratica.DATA_CESSAZIONE)
                    {
                        dip.NUM_MENS_PRINC = null;
                        dip.NUM_MENS_AGG = null;
                        TaskHelper.AddBatchRunnerTask("IncentiviFNL930OnDemand", out var taskErr, note: String.Format("Matricola {0} - IdDip: {1}", dip.MATRICOLA, dip.ID_DIPENDENTE));
                        runConv = false;
                    }

                    if (runConv)
                        TaskHelper.AddBatchRunnerTask("IncentiviPropostaOnDemand", out var taskErr, dip.MATRICOLA, note: String.Format("Matricola {0} - IdDip: {1}", dip.MATRICOLA, dip.ID_DIPENDENTE));

                    dip.DATA_ANZIANITA = modDip.Pratica.DATA_ANZIANITA;
                    dip.PART_TIME = modDip.Pratica.PART_TIME;
                    if (modDip.TipiVertenze != null && modDip.TipiVertenze.Any(x => !String.IsNullOrWhiteSpace(x)))
                        dip.IND_TIPO_VERTENZE = String.Join(";", modDip.TipiVertenze.Where(x => !String.IsNullOrWhiteSpace(x)));
                    else
                        dip.IND_TIPO_VERTENZE = null;
                    dip.CAUSE_VERTENZE = modDip.Pratica.CAUSE_VERTENZE;
                    dip.UNA_TANTUM_LORDA = modDip.Pratica.UNA_TANTUM_LORDA;
                    dip.INCENTIVO_LORDO = modDip.Pratica.INCENTIVO_LORDO;

                    dip.DATA_CESSAZIONE = modDip.Pratica.DATA_CESSAZIONE;
                    dip.DATA_PAGAMENTO = modDip.Pratica.DATA_PAGAMENTO;
                    dip.CELLULARE = modDip.Pratica.CELLULARE;
                    dip.MAIL = modDip.Pratica.MAIL;

                    dip.EX_FISSA = modDip.Pratica.EX_FISSA;
                    dip.NUM_MENS_PRINC_DEC = modDip.Pratica.NUM_MENS_PRINC_DEC;
                    dip.NUM_MENS_AGG_DEC = modDip.Pratica.NUM_MENS_AGG_DEC;

                    dip.INCENTIVO_LORDO_IP = modDip.Pratica.INCENTIVO_LORDO;
                    dip.UNA_TANTUM_LORDA_IP = modDip.Pratica.UNA_TANTUM_LORDA;
                    if (qualExFissa.Any(x => dip.SINTESI1.COD_QUALIFICA.StartsWith(x)) && dip.EX_FISSA.HasValue && dip.EX_FISSA.Value > 0)
                    {
                        dip.INCENTIVO_LORDO_IP += dip.EX_FISSA.Value;
                    }

                    dip.TFR_LORDO_INPS_IP = modDip.Pratica.TFR_LORDO_INPS_IP;
                    dip.TFR_LORDO_AZ_IP = modDip.Pratica.TFR_LORDO_AZ_IP;
                    dip.TFR_NETTO = modDip.Pratica.TFR_NETTO;
                    dip.ALIQ_TFR = modDip.Pratica.ALIQ_TFR;

                    dip.DATA_INVIO_PROP = modDip.Pratica.DATA_INVIO_PROP;
                    dip.DATA_FIRMA_DIP = modDip.Pratica.DATA_FIRMA_DIP;

                    dip.DTA_ACCETT_AZ = modDip.Pratica.DTA_ACCETT_AZ;
                    dip.DATA_FIRMA_DIP_ACCETT_AZ = modDip.Pratica.DATA_FIRMA_DIP_ACCETT_AZ;
                    dip.DTA_RECESSO = modDip.Pratica.DTA_RECESSO;

                    dip.SetField("VeicoloTipologia", modDip.VeicoloTipologia);
                    dip.SetField("VeicoloTarga", modDip.VeicoloTarga);
                    dip.SetField("TfrAnte2007FPGCI", modDip.TfrAnte2007FPGCI);

                    dip.SetField("LimiteProposta", modDip.Scadenze.LimiteProposta);

                    dip.COD_USER = CommonHelper.GetCurrentUserMatricola();
                    dip.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
                    dip.TMS_TIMESTAMP = DateTime.Now;
                    DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());

                    if (prosegui)
                    {

                        if (_stato)
                        {
                            SalvaStato(modDip.Pratica.ID_DIPENDENTE, (int)IncStato.RecessoEffettuato);
                            CessazioneManager.RendiAllegatiEffettivi(modDip.Pratica.ID_DIPENDENTE, db, (int)IncStato.TempFileProposta, (int)IncStato.FileProposta);
                            CessazioneManager.RendiAllegatiEffettivi(modDip.Pratica.ID_DIPENDENTE, db, (int)IncStato.TempFileAccettazione, (int)IncStato.FileAccettazione);

                            //Serve solo ad approvare la proposta firmata in caso la pratica venga fatta proseguire
                            //Al momento non ci sono altri file che devono essere inseriti
                            CessazioneManager.RendiAllegatiDipEffettivi(modDip.Pratica.ID_DIPENDENTE, modDip.Pratica.MATRICOLA, db, (int)IncStato.RichiestaAccettata, (int)IncStato.FileProposta, "Proposta");

                            string qualSaltaUfficioPrestiti = HrisHelper.GetParametro<string>(HrisParam.IncentiviQualificheSaltaUfficioPrestiti);
                            if (!String.IsNullOrWhiteSpace(qualSaltaUfficioPrestiti)
                                    && qualSaltaUfficioPrestiti.Split(',').Any(x => dip.SINTESI1.COD_QUALIFICA.StartsWith(x)))
                                SalvaStato(modDip.Pratica.ID_DIPENDENTE, (int)IncStato.Controllato);

                            if (_inviaMail)
                            {
                                var template = CessazioneHelper.GetTemplate(db, "TemplateMail", dip.ID_DIPENDENTE, "RecessoEffettuato", true, dip.SINTESI1.COD_QUALIFICA);
                                if (template != null)
                                {
                                    string mailOggetto = CessazioneHelper.ReplaceToken(dip, template.DES_TEMPLATE);
                                    string mailTesto = CessazioneHelper.ReplaceToken(dip, template.TEMPLATE_TEXT);

                                    string mailDest = CessazioneHelper.GetMailDip(dip); //CommonTasks.GetEmailPerMatricola(dip.MATRICOLA);
                                    GestoreMail mail = new GestoreMail();
                                    string mittente = CessazioneHelper.GetIndirizzoMail("FromRecessoEffettuato");
                                    var response = mail.InvioMail(mailTesto, mailOggetto, mailDest, mittente, mittente, null, null);
                                    if (response != null && response.Errore != null)
                                        HrisHelper.LogOperazione("IncInvioMail", String.Format("{0} - Invio mail fallito - {1}", dip.MATRICOLA, mailOggetto), false, response.Errore);
                                    else
                                        HrisHelper.LogOperazione("IncInvioMail", String.Format("{0} - Invio mail {1}", dip.MATRICOLA, mailOggetto), true);
                                }
                            }
                        }
                        else
                        {
                            if (_oper == "d" && dip.DATA_INVIO_PROP.HasValue && !dip.DATA_FIRMA_DIP.HasValue)
                            {
                                ;
                            }
                            else
                            {
                                rimuoviRichiesta = true;
                                int stato = 0;
                                switch (_oper)
                                {
                                    case "r":
                                        stato = (int)IncStato.RichiestaRifiutata;
                                        break;
                                    case "d":
                                        stato = (int)IncStato.RichiestaDecaduta;
                                        break;
                                    case "pr":
                                        stato = (int)IncStato.PropostaRifiutata;
                                        break;
                                    default:
                                        stato = (int)IncStato.RichiestaRifiutata;
                                        break;
                                }

                                SalvaStato(modDip.Pratica.ID_DIPENDENTE, stato);
                                if (!String.IsNullOrWhiteSpace(_nota))
                                    InternalAggiungiNotaPratica(dip.ID_DIPENDENTE, _nota);

                                CessazioneManager.RendiAllegatiEffettivi(modDip.Pratica.ID_DIPENDENTE, db, (int)IncStato.TempFileProposta, stato);
                                CessazioneManager.RendiAllegatiEffettivi(modDip.Pratica.ID_DIPENDENTE, db, (int)IncStato.TempFileAccettazione, stato);

                                CessazioneManager.RendiAllegatiDipEffettivi(modDip.Pratica.ID_DIPENDENTE, modDip.Pratica.MATRICOLA, db, (int)IncStato.RichiestaAccettata, stato, "Proposta");
                            }
                        }
                        DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());


                        RilasciaPratica(modDip.Pratica.ID_DIPENDENTE);
                    }

                    if (_oper == "d" && dip.DATA_INVIO_PROP.HasValue && !dip.DATA_FIRMA_DIP.HasValue)
                    {
                        InternalGestioneSollecito(dip.ID_DIPENDENTE, "DecadenzaProposta", _nota);
                    }

                    result = "OK";

                    if (rimuoviRichiesta)
                        RimuoviRichiestaIncentivi(dip, _oper, _nota);
                }
            }

            return Content(result);
        }
        public ActionResult Modifica_Email(int idDip, string mail)
        {
            string result = "";

            var db = new IncentiviEntities();
            var dip = db.XR_INC_DIPENDENTI.Find(idDip);
            dip.MAIL = mail;
            if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                result = "OK";
            else
                result = "Errore durante il salvataggio";

            return Content(result);
        }
        public ActionResult Modifica_Cellulare(int idDip, string cellulare)
        {
            string result = "";

            var db = new IncentiviEntities();
            var dip = db.XR_INC_DIPENDENTI.Find(idDip);
            dip.CELLULARE = cellulare;
            if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                result = "OK";
            else
                result = "Errore durante il salvataggio";

            return Content(result);
        }
        public ActionResult Modifica_Contenzioso(int idDip, string cont)
        {
            string result = "";

            var db = new IncentiviEntities();
            var dip = db.XR_INC_DIPENDENTI.Find(idDip);
            dip.IND_TIPO_VERTENZE = cont;
            if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                result = "OK";
            else
                result = "Errore durante il salvataggio";

            return Content(result);
        }

        public static void RimuoviRichiestaIncentivi(XR_INC_DIPENDENTI dip, string _oper, string _nota)
        {
            CessazioneManager.RimuoviModuloRichiesta(dip, _oper, _nota);
        }

        public ActionResult GetDatePagamento(string dataCessazione)
        {
            DateTime? filtro = null;
            DateTime tmp;
            if (!String.IsNullOrWhiteSpace(dataCessazione) && DateTime.TryParseExact(dataCessazione, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out tmp))
                filtro = tmp;

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = CessazioneManager.GetDatePagamenti(filtro)
            };
        }
        public ActionResult GestioneSollecito(int idDip, string _oper)
        {
            InternalGestioneSollecito(idDip, _oper);

            return Content("OK");
        }

        private static void InternalGestioneSollecito(int idDip, string _oper, string nota = null)
        {
            //Oper => anzichè un'operazione viene passato direttamente il nome del template

            string[] rifGruppo = HrisHelper.GetParametri<string>(HrisParam.IncentiviRifGruppo);
            List<XR_HRIS_PARAM> listParam = HrisHelper.GetParametriJson<XR_HRIS_PARAM>(HrisParam.IncentiviParametri);
            var codGruppo = rifGruppo[0];
            var annoRif = rifGruppo[1];
            var db = new IncentiviEntities();
            var pratica = db.XR_INC_DIPENDENTI.Find(idDip);

            bool mailInvia = true;
            string mailTesto = "";
            string mailOggetto = "";
            string mailTemplate = _oper;
            string logOper = "";

            bool rilasciaPratica = false;
            bool hasInCarico = false;
            bool rimuoviRichiesta = false;
            bool hasNota = false;
            string notaPratica = null;

            if (_oper == "SollecitoEstratti")
            {
                logOper = "RichiestaInseritaSollecito";
            }
            else if (_oper == "DecadenzaEstratti")
            {
                logOper = "RichiestaInseritaDecadenza";

                hasNota = true;
                notaPratica = !String.IsNullOrWhiteSpace(nota) ? nota : "Mancata produzione della documentazione contributiva.";

                //In questo caso deve impostare la decadenza della pratica
                CessazioneHelper.SalvaStato(db, pratica.ID_DIPENDENTE, (int)IncStato.RichiestaDecaduta, pratica.ID_PERSONA);
                CessazioneManager.RendiAllegatiEffettivi(pratica.ID_DIPENDENTE, db, (int)IncStato.TempFileEstratti, (int)IncStato.RichiestaDecaduta);
                CessazioneManager.RendiAllegatiDipEffettivi(pratica.ID_DIPENDENTE, pratica.MATRICOLA, db, (int)IncStato.RichiestaInserita, (int)IncStato.RichiestaDecaduta);

                rilasciaPratica = true;
                rimuoviRichiesta = true;
            }
            else if (_oper == "DecadenzaProposta")
            {
                logOper = "PropostaDecadenza";

                hasNota = true;
                notaPratica = !String.IsNullOrWhiteSpace(nota) ? nota : "Mancato invio proposta.";

                CessazioneHelper.SalvaStato(db, pratica.ID_DIPENDENTE, (int)IncStato.PropostaDecaduta, pratica.ID_PERSONA);
                CessazioneManager.RendiAllegatiEffettivi(pratica.ID_DIPENDENTE, db, (int)IncStato.TempFileProposta, (int)IncStato.PropostaDecaduta);
                CessazioneManager.RendiAllegatiEffettivi(pratica.ID_DIPENDENTE, db, (int)IncStato.TempFileAccettazione, (int)IncStato.PropostaDecaduta);

                CessazioneManager.RendiAllegatiDipEffettivi(pratica.ID_DIPENDENTE, pratica.MATRICOLA, db, (int)IncStato.RichiestaAccettata, (int)IncStato.PropostaDecaduta, "Proposta");
                //CessazioneManager.RendiAllegatiDipEffettivi(pratica.ID_DIPENDENTE, pratica.MATRICOLA, db, (int)IncStato.TempFileAccettazione, (int)IncStato.RichiestaDecaduta);

                rilasciaPratica = true;
                rimuoviRichiesta = true;
            }

            if (rilasciaPratica)
            {
                XR_INC_OPERSTATI inCarico = db.XR_INC_OPERSTATI.FirstOrDefault(x => x.ID_DIPENDENTE == pratica.ID_DIPENDENTE && x.ID_STATO == (int)IncStato.InCarico);
                if (inCarico != null)
                {
                    db.XR_INC_OPERSTATI.Remove(inCarico);
                    hasInCarico = true;
                }
            }

            if (hasNota)
                CessazioneHelper.AggiungiNotaPratica(db, pratica.ID_DIPENDENTE, notaPratica, pratica.ID_PERSONA);

            if (rimuoviRichiesta)
                RimuoviRichiestaIncentivi(pratica, "d", notaPratica);

            if (hasInCarico || hasNota)
                DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());

            if (mailInvia)
            {
                var param = listParam.FirstOrDefault(x => x.COD_PARAM == "LimiteConsegnaEstratti");
                var template = CessazioneHelper.GetTemplate(db, "TemplateMail", pratica.ID_DIPENDENTE, mailTemplate, false);
                mailOggetto = CessazioneHelper.ReplaceToken(pratica, template.DES_TEMPLATE);
                mailTesto = CessazioneHelper.ReplaceToken(pratica, template.TEMPLATE_TEXT)
                                .Replace("__LIMITECONSEGNAESTRATTI__", param.COD_VALUE1);

                string mailDest = CessazioneHelper.GetMailDip(pratica); //CommonTasks.GetEmailPerMatricola(pratica.MATRICOLA);
                GestoreMail mail = new GestoreMail();
                string mittente = CessazioneHelper.GetIndirizzoMail(_oper);
                var response = mail.InvioMail(mailTesto, mailOggetto, mailDest, mittente, mittente, null, null);
                if (response != null && response.Errore != null)
                    HrisHelper.LogOperazione("IncInvioMail", String.Format("{0} - Invio mail fallito - {1}", pratica.MATRICOLA, mailOggetto), false, response.Errore);
                else
                    HrisHelper.LogOperazione("IncInvioMail", String.Format("{0} - Invio mail {1}", pratica.MATRICOLA, mailOggetto), true);

                var incParamMail = new XR_INC_PARAM_MAIL()
                {
                    COD_CAMPO = logOper,
                    COD_MATRICOLA = pratica.MATRICOLA,
                    VALORE = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                    COD_SERVIZIO_ESCL = CommonHelper.GetCurrentUserMatricola(),
                    ID_DIPENDENTE = pratica.ID_DIPENDENTE
                };
                db.XR_INC_PARAM_MAIL.Add(incParamMail);
                DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
            }
        }

        #endregion

        #region Utility
        public static string GetBoxDescription(IncStato stato)
        {
            string description = "";
            using (IncentiviEntities db = new IncentiviEntities())
            {
                XR_INC_STATI rifStato = db.XR_INC_STATI.FirstOrDefault(x => x.ID_STATO == (int)stato);
                description = rifStato.NOME_BOX;
            }
            return description;
        }

        public static List<DateTime> GetElencoDate(IncStato stato, CessazioneTipo tipo)
        {
            List<DateTime> elencoDate = new List<DateTime>();

            IncentiviEntities db = new IncentiviEntities();
            var isAnyState = CessazioneHelper.IsAnyState((int)stato);
            var tmp = db.XR_INC_DIPENDENTI.Where(x => x.ID_TIPOLOGIA == (int)tipo).Where(isAnyState).Where(x => x.DATA_CESSAZIONE.HasValue);


            switch (stato)
            {
                case IncStato.Conteggio:
                    elencoDate.AddRange(tmp.Where(x => x.DATA_CESSAZIONE.HasValue).Select(x => x.DATA_CESSAZIONE.Value).Distinct());
                    break;
                case IncStato.VerbaleCaricato:
                    elencoDate.AddRange(tmp.Where(x => x.DATA_PAGAMENTO.HasValue).Select(x => x.DATA_PAGAMENTO.Value).Distinct());
                    break;
            }

            return elencoDate;
        }
        public static DateTime? GetDataAnzCat(int idPersona)
        {
            DateTime? anzCat = HrisHelper.GetDataAnzCat(idPersona);

            return anzCat;
        }

        public ActionResult SelezioneDipendenti()
        {
            return View("~/Views/Cessazione/subpartial/SelezioneDipendenti.cshtml");
        }
        #endregion

        #region CaricamentoDati

        private DateTime? CalcolaDataPagamento(DateTime dataCessazione)
        {
            DateTime? dataPagamento = null;

            if (dataCessazione.CompareTo(new DateTime(2018, 09, 30)) == 0)
                dataPagamento = new DateTime(2018, 11, 29);
            else if (dataCessazione.CompareTo(new DateTime(2018, 10, 31)) == 0)
                dataPagamento = new DateTime(2019, 1, 10);
            else if (dataCessazione.CompareTo(new DateTime(2018, 11, 30)) == 0)
                dataPagamento = new DateTime(2019, 2, 4);
            else if (dataCessazione.CompareTo(new DateTime(2018, 12, 31)) == 0)
                dataPagamento = new DateTime(2019, 3, 4);

            return dataPagamento;
        }

        private void UpdateDataPagamento()
        {
            IncentiviEntities db = new IncentiviEntities();
            foreach (var dip in db.XR_INC_DIPENDENTI)
            {
                dip.DATA_PAGAMENTO = CalcolaDataPagamento(dip.DATA_CESSAZIONE.Value);
            }
            DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
        }

        private static string GetPrepRappr(string tipo, string sede, string genere)
        {
            string result = "";

            if (!String.IsNullOrWhiteSpace(genere))
            {
                switch (tipo)
                {
                    case "RAI":
                        if (sede == "PALERMO")
                            result = "";
                        else if (sede == "MILANO")
                            result = genere == "M" ? "dal " : "dalla ";
                        else
                            result = genere == "M" ? "del " : "della ";
                        break;
                    case "SIND":
                        if (sede == "PALERMO" || sede == "MILANO")
                            result = "";
                        else if (sede == "VENEZIA")
                            result = genere == "M" ? "del " : "della ";
                        else
                            result = genere == "M" ? "il " : "la ";
                        break;
                    case "IND":
                        if (sede == "PALERMO" || sede == "MILANO" || sede == "ROMA")
                            result = "";
                        else if (sede == "VENEZIA")
                            result = genere == "M" ? "del " : "della ";
                        else
                            result = genere == "M" ? "il " : "la ";
                        break;
                    default:
                        break;
                }
            }

            return result;
        }

        private void UpdateRapprRai()
        {
            string line;
            string cod_user = "BATCHSESSION";
            string terminal = System.Web.HttpContext.Current.Request.UserHostAddress;

            System.Diagnostics.Debug.WriteLine("Aggiornamento Rappr.Rai - Inizio");
            IncentiviEntities db = new IncentiviEntities();

            // Read the file and display it line by line.  
            System.IO.StreamReader file = new System.IO.StreamReader(@"C:\tmp\ElencoRapprRai.csv", Encoding.Default);
            line = file.ReadLine();
            while ((line = file.ReadLine()) != null)
            {
                System.Diagnostics.Debug.Write(line);

                string[] elements = line.Split(';');

                if (String.IsNullOrWhiteSpace(elements[0])) continue;

                string sede = elements[0];
                string cognome = elements[1];
                string nome = elements[2];
                string titolo = elements[3];

                ANAGPERS anagPers = db.ANAGPERS.FirstOrDefault(x => x.DES_COGNOMEPERS == cognome && x.DES_NOMEPERS == nome);
                if (anagPers != null)
                {
                    if (!db.XR_INC_RAPPRRAI.Any(x => x.ID_PERSONA == anagPers.ID_PERSONA && x.SEDE == sede))
                    {
                        XR_INC_RAPPRRAI rappr = new XR_INC_RAPPRRAI();
                        rappr.ID_RAPPRRAI = db.XR_INC_RAPPRRAI.GeneraPrimaryKey();
                        rappr.ID_PERSONA = anagPers.ID_PERSONA;
                        rappr.SEDE = elements[0];
                        rappr.TITOLO = GetPrepRappr("RAI", rappr.SEDE, anagPers.COD_SESSO) + titolo;
                        rappr.COD_USER = cod_user;
                        rappr.COD_TERMID = terminal;
                        rappr.TMS_TIMESTAMP = DateTime.Now;
                        db.XR_INC_RAPPRRAI.Add(rappr);
                        DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                    }
                }
            }

            System.Diagnostics.Debug.WriteLine("Aggiornamento Rappr.Rai - Fine");

            file.Close();
        }
        private void UpdateRapprIndu()
        {
            string line;
            string cod_user = "BATCHSESSION";
            string terminal = System.Web.HttpContext.Current.Request.UserHostAddress;

            System.Diagnostics.Debug.WriteLine("Aggiornamento Rappr.Unindustria - Inizio");
            IncentiviEntities db = new IncentiviEntities();

            // Read the file and display it line by line.  
            System.IO.StreamReader file = new System.IO.StreamReader(@"C:\tmp\ElencoRapprUnindustria.csv", Encoding.Default);
            line = file.ReadLine();
            while ((line = file.ReadLine()) != null)
            {
                System.Diagnostics.Debug.Write(line);

                string[] elements = line.Split(';');

                if (String.IsNullOrWhiteSpace(elements[0])) continue;

                string sede = elements[0];
                string cognome = elements[1];
                string nome = elements[2];
                string titolo = elements[3];
                string genere = elements[4];

                XR_INC_RAPPRINDUSTRIA rapprInd = db.XR_INC_RAPPRINDUSTRIA.FirstOrDefault(x => x.SEDE == sede && x.COGNOME == cognome && x.NOME == nome);
                if (rapprInd == null)
                {
                    rapprInd = db.XR_INC_RAPPRINDUSTRIA.FirstOrDefault(x => x.SEDE == sede && x.COGNOME == "-" && x.NOME == "-");
                    if (rapprInd != null)
                    {
                        rapprInd.COGNOME = elements[1];
                        rapprInd.NOME = elements[2];
                        rapprInd.TITOLO = GetPrepRappr("IND", sede, genere) + titolo;
                        rapprInd.GENERE = genere;
                    }
                    else
                    {
                        rapprInd = new XR_INC_RAPPRINDUSTRIA();
                        rapprInd.ID_RAPPRINDUSTRIA = db.XR_INC_RAPPRINDUSTRIA.GeneraPrimaryKey();
                        rapprInd.COGNOME = elements[1];
                        rapprInd.NOME = elements[2];
                        rapprInd.SEDE = elements[0];
                        rapprInd.TITOLO = GetPrepRappr("IND", sede, genere) + titolo;
                        rapprInd.GENERE = genere;
                        rapprInd.ORGANIZZAZIONE = "";
                        rapprInd.COD_USER = cod_user;
                        rapprInd.COD_TERMID = terminal;
                        rapprInd.TMS_TIMESTAMP = DateTime.Now;
                        db.XR_INC_RAPPRINDUSTRIA.Add(rapprInd);
                    }
                    DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                }
            }

            System.Diagnostics.Debug.WriteLine("Aggiornamento Rappr.Unindustria - Fine");

            file.Close();
        }
        #endregion

        #region GestioneDocumenti
        private static MemoryStream InternalProspetto(IncentiviEntities db, XR_INC_DIPENDENTI dip, int pageNum = 0, bool addSign = true)
        {
            //Prima di creare il prospetto, verifica che non ci sia un prospetto allegato allo stato dei dati contabili.
            //Questo è un "ovveride" nel caso in cui ci siano dei problemi con i conteggio
            var ovveride = db.XR_INC_OPERSTATI_DOC.FirstOrDefault(x => x.XR_INC_OPERSTATI.ID_DIPENDENTE == dip.ID_DIPENDENTE && x.XR_INC_OPERSTATI.ID_STATO == (int)IncStato.FileOverride
                                                                     && x.XR_INC_OPERSTATI.DATA_FINE_VALIDITA == null);
            if (ovveride != null)
                return new MemoryStream(ovveride.OBJ_OBJECT);

            Uri u = new Uri(new myRaiServiceHub.it.rai.servizi.wiahrss.ezService().Url);

            BasicHttpBinding binding = new BasicHttpBinding();
            EndpointAddress address = new EndpointAddress(u);
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;
            binding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
            binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;

            binding.MessageEncoding = WSMessageEncoding.Text;
            binding.TextEncoding = Encoding.UTF8;
            binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;

            var _client = new myRaiServiceHub.ServiceBonus100.ezServiceSoapClient(binding, address);

            if (_client.ClientCredentials != null)
            {
                _client.ClientCredentials.Windows.ClientCredential = CommonHelper.GetUtenteServizioCredentials();
                _client.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Delegation;
            }
            _client.Open();

            var requestInterceptor = new InspectorBehavior();
            _client.Endpoint.Behaviors.Add(requestInterceptor);

            // aggiornamento dei dati
            string rifConteggio = HrisHelper.GetParametro<string>(HrisParam.IncentiviRiferimentoConteggi);
            if (String.IsNullOrWhiteSpace(rifConteggio))
                rifConteggio = "18";
            var resp = _client.TLIQ_PROSPETTO_MATRICOLA("0" + dip.MATRICOLA, rifConteggio);
            string nominativo = dip.ANAGPERS.DES_NOMEPERS.TitleCase() + " " + dip.ANAGPERS.DES_COGNOMEPERS.TitleCase();
            string nomeFile = "Prospetto " + dip.ANAGPERS.DES_COGNOMEPERS.TitleCase() + " " + dip.ANAGPERS.DES_NOMEPERS.TitleCase();

            string logoIcon = "";
            string firmaPng = "";
            string infoSoc = "";

            logoIcon = GetImageDoc(db.XR_INC_TEMPLATE.FirstOrDefault(x => x.NME_TEMPLATE == "LOGO"));
            infoSoc = GetImageDoc(db.XR_INC_TEMPLATE.FirstOrDefault(x => x.NME_TEMPLATE == "INTESTAZIONE"));
            if (addSign)
            {
                firmaPng = GetImageDoc(db.XR_INC_TEMPLATE.FirstOrDefault(x => x.NME_TEMPLATE == "FIRMA"));
            }

            MemoryStream prospetto = CessazioneManager.CreaPDFProspetto(logoIcon, firmaPng, infoSoc, nomeFile, nominativo, dip.DATA_CESSAZIONE.Value.ToString("dd/MM/yyyy"), resp.Rows, pageNum);

            string requestXML = requestInterceptor.LastRequestXML;
            string responseXML = requestInterceptor.LastResponseXML;

            _client.Close();

            return prospetto;
        }

        private static string GetImageDoc(XR_INC_TEMPLATE template)
        {
            string result = null;
            if (template != null)
                result = Convert.ToBase64String(template.TEMPLATE);
            return result;
        }

        public ActionResult GetDoc(int idDoc)
        {
            using (IncentiviEntities db = new IncentiviEntities())
            {
                XR_INC_OPERSTATI_DOC doc = db.XR_INC_OPERSTATI_DOC.FirstOrDefault(x => x.ID_ALLEGATO == idDoc);
                if (doc == null)
                    return View("~/Views/Shared/404.cshtml");
                else
                    //return new FileStreamResult(new MemoryStream(doc.OBJ_OBJECT), doc.CONTENT_TYPE) { FileDownloadName = doc.NME_FILENAME };
                    return new FileContentResult(doc.OBJ_OBJECT, doc.CONTENT_TYPE) { FileDownloadName = doc.NME_FILENAME };
            }
        }
        public ActionResult GetDipFile(string key)
        {
            var file = FileManager.GetFileByChiave(key);
            if (file.Esito)
                return new FileStreamResult(new MemoryStream(file.Files[0].ContentByte), file.Files[0].MimeType) { FileDownloadName = file.Files[0].NomeFile };
            else
                return new HttpNotFoundResult();
        }
        public ActionResult ApprovaFileDip(string key, int idStato)
        {
            string result = "";
            var file = FileManager.GetFileByChiave(key);
            if (file.Esito)
            {
                IncentivazioneFile info = Newtonsoft.Json.JsonConvert.DeserializeObject<IncentivazioneFile>(file.Files[0].Json);

                IncentiviEntities db = new IncentiviEntities();
                int idOper = SalvaStato(info.IdDipendente, idStato);

                XR_INC_OPERSTATI_DOC doc = new XR_INC_OPERSTATI_DOC();
                doc.ID_ALLEGATO = db.XR_INC_OPERSTATI_DOC.GeneraPrimaryKey();
                doc.ID_OPER = idOper;
                doc.NME_FILENAME = file.Files[0].NomeFile;
                doc.COD_TITLE = info.Titolo;
                doc.DES_ALLEGATO = info.Descrizione;
                doc.CONTENT_TYPE = file.Files[0].MimeType;
                doc.OBJ_OBJECT = file.Files[0].ContentByte;
                doc.NMB_SIZE = file.Files[0].Length;
                doc.IND_RILEVANTE = true;
                doc.COD_USER = file.Files[0].MatricolaCreatore;
                doc.COD_TERMID = "RAIPERME";
                doc.NOT_TAG = "Rai per Me" + (!String.IsNullOrWhiteSpace(info.Tag) ? ";" + info.Tag : "");
                doc.TMS_TIMESTAMP = file.Files[0].DataCreazione;
                db.XR_INC_OPERSTATI_DOC.Add(doc);
                if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                {
                    var dbDg = new digiGappEntities();
                    var dbFile = dbDg.MyRai_Files.Find(file.Files[0].Id);
                    info.Approvato = true;
                    info.MatricolaApprv = CommonHelper.GetCurrentUserMatricola();
                    dbFile.Json = Newtonsoft.Json.JsonConvert.SerializeObject(info);
                    DBHelper.Save(dbDg, CommonHelper.GetCurrentUserMatricola());

                    result = "OK";
                }
                else
                    result = "Errore durante il salvataggio del file";
            }
            else
                result = file.Errore;

            return Content(result);
        }
        public ActionResult RifiutaFileDip(string key, string nota, string tipoRifiuto)
        {
            //var db = new IncentiviEntities();
            //var pratica = db.XR_INC_DIPENDENTI.Find(idDipendente);
            //pratica.DATA_RICH_INT = DateTime.Now;
            //if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))

            var file = FileManager.GetFileByChiave(key);
            bool forceUpdate = false;
            if (file.Esito)
            {
                IncentivazioneFile info = Newtonsoft.Json.JsonConvert.DeserializeObject<IncentivazioneFile>(file.Files[0].Json);
                var db = new IncentiviEntities();
                var pratica = db.XR_INC_DIPENDENTI.Find(info.IdDipendente);

                if (info.Template.HasValue)
                {
                    var template = db.XR_INC_TEMPLATE.Find(info.Template.Value);
                    if (template != null)
                    {
                        var dict = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(template.TEMPLATE_TEXT);
                        string propName, intCheckName, intPropName = "";
                        object intDate = null;
                        bool checkTipo = false;

                        if (dict.TryGetValue("CampoDataAggiornamento", out propName))
                        {

                            //Controllo se è presente anche un campo di richiesta integrazioni
                            if (dict.TryGetValue("CampoRichiestaInt", out intCheckName))
                            {
                                checkTipo = true;
                                dict.TryGetValue("CampoDataAggiornamentoInt", out intPropName);
                                intDate = pratica.GetType().GetProperty(intCheckName).GetValue(pratica, null);
                            }

                            if (intDate == null)
                            {
                                if (tipoRifiuto == "I")
                                    pratica.GetType().GetProperty(propName).SetValue(pratica, null, null);
                                else if (checkTipo && tipoRifiuto == "P")
                                    pratica.GetType().GetProperty(intCheckName).SetValue(pratica, DateTime.Now, null);
                            }
                            else if (intDate != null)
                                pratica.GetType().GetProperty(intPropName).SetValue(pratica, null, null);

                            DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                            forceUpdate = true;
                        }
                    }
                }

                //Invio mail al dipendente
                List<XR_HRIS_PARAM> param = HrisHelper.GetParametriJson<XR_HRIS_PARAM>(HrisParam.IncentiviParametri);
                var paramMail = param.FirstOrDefault(x => x.COD_PARAM == "MailRifiutoFile" + tipoRifiuto);
                if (paramMail != null)
                {
                    GestoreMail mail = new GestoreMail();
                    string subj = paramMail.COD_VALUE1;
                    string dest = CessazioneHelper.GetMailDip(pratica); //CommonTasks.GetEmailPerMatricola(pratica.MATRICOLA);
                    string from = paramMail.COD_VALUE2;
                    string destCC = !String.IsNullOrWhiteSpace(paramMail.COD_VALUE3) ? paramMail.COD_VALUE3 : null;
                    string destCCN = !String.IsNullOrWhiteSpace(paramMail.COD_VALUE4) ? paramMail.COD_VALUE4 : null;

                    string body = $@"<span style='-webkit-text-size-adjust: 100%;-webkit-tap-highlight-color: rgba(0,0,0,0);box-sizing: border-box;display: inline;max-width: 100%;margin-bottom: 5px;font-size: 14px;font-weight: 600;line-height: normal;color: #859fad;'>Titolo</span>
				                     <span style='-webkit-text-size-adjust: 100%;-webkit-tap-highlight-color: rgba(0,0,0,0);box-sizing: border-box;display: inline;max-width: 100%;margin-bottom: 5px;font-size: 14px;font-weight: 400;line-height: normal;color: #0a3247;'>{(info != null ? (info.Titolo.UpperFirst() ?? "") : "-")}</span><br/>
                                     <span style='-webkit-text-size-adjust: 100%;-webkit-tap-highlight-color: rgba(0,0,0,0);box-sizing: border-box;display: inline;max-width: 100%;margin-bottom: 5px;font-size: 14px;font-weight: 600;line-height: normal;color: #859fad;'>Descrizione</span>
				                     <span style='-webkit-text-size-adjust: 100%;-webkit-tap-highlight-color: rgba(0,0,0,0);box-sizing: border-box;display: inline;max-width: 100%;margin-bottom: 5px;font-size: 14px;font-weight: 400;line-height: normal;color: #0a3247;'>{(info != null ? (info.Descrizione.UpperFirst() ?? "") : "-")}</span><br/>
                                     <span style='-webkit-text-size-adjust: 100%;-webkit-tap-highlight-color: rgba(0,0,0,0);box-sizing: border-box;display: inline;max-width: 100%;margin-bottom: 5px;font-size: 14px;font-weight: 600;line-height: normal;color: #859fad;'>Nome file</span>
									 <span style='-webkit-text-size-adjust: 100%;-webkit-tap-highlight-color: rgba(0,0,0,0);box-sizing: border-box;display: inline;max-width: 100%;margin-bottom: 5px;font-size: 14px;font-weight: 400;line-height: normal;color: #0a3247;'>{file.Files[0].NomeFile}</span><br/>
                                     <span style='-webkit-text-size-adjust: 100%;-webkit-tap-highlight-color: rgba(0,0,0,0);box-sizing: border-box;display: inline;max-width: 100%;margin-bottom: 5px;font-size: 14px;font-weight: 600;line-height: normal;color: #859fad;'>Motivo</span>
                                     <span style='-webkit-text-size-adjust: 100%;-webkit-tap-highlight-color: rgba(0,0,0,0);box-sizing: border-box;display: inline;max-width: 100%;margin-bottom: 5px;font-size: 14px;font-weight: 400;line-height: normal;color: #0a3247;'>{nota}</span><br/>
                                    ";
                    mail.InvioMail(from, subj, dest, destCC, subj, null, body, "VAI AL SITO", destinatariCCN: destCCN);
                }


                var dbDg = new digiGappEntities();
                info.Approvato = false;
                info.Nota = nota;
                info.MatricolaApprv = CommonHelper.GetCurrentUserMatricola();
                var dbFile = dbDg.MyRai_Files.Find(file.Files[0].Id);
                dbFile.Json = Newtonsoft.Json.JsonConvert.SerializeObject(info);
                DBHelper.Save(dbDg, CommonHelper.GetCurrentUserMatricola());
                return Content(!forceUpdate ? "OK" : "UPDATE");

            }
            else
                return Content(file.Errore);
        }

        public ActionResult EliminaDoc(int idOper, int idDoc)
        {
            IncentiviEntities db = new IncentiviEntities();

            db.XR_INC_OPERSTATI_DOC.RemoveWhere(x => x.ID_ALLEGATO == idDoc);
            DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());

            var listVerb = db.XR_INC_OPERSTATI.FirstOrDefault(x => x.ID_OPER == idOper)
                .XR_INC_OPERSTATI_DOC.OrderBy(y => y.TMS_TIMESTAMP)
                .Select(z => new
                {
                    idVerbale = z.ID_ALLEGATO,
                    persona = CezanneHelper.GetNominativoByMatricola(z.COD_USER),
                    dataUpload = z.TMS_TIMESTAMP.ToString("dd/MM/yyyy HH:mm"),
                    descr = z.DES_ALLEGATO
                });

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = listVerb
            };
        }
        public ActionResult EliminaGenericDoc(int idDoc)
        {
            IncentiviEntities db = new IncentiviEntities();

            var doc = db.XR_INC_OPERSTATI_DOC.Find(idDoc);
            db.XR_INC_OPERSTATI_DOC.Remove(doc);

            string result = "";
            if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                result = "OK";
            else
                result = "Errore durante il salvataggio";

            return Content(result);
        }


        public static List<CessazioneBozzaSample> GetUrlBozze()
        {
            List<CessazioneBozzaSample> result = new List<CessazioneBozzaSample>();
            var bozze = HrisHelper.GetParametriJson<CessazioneBozzaSample>(HrisParam.IncentiviBozzeVerbali);
            return bozze;
        }
        public ActionResult CreaBozzaVerbale(CessazioneBozzaSample model)// string descrizione, string sede = "", string templateName = "", string qualifica = "", string sindacato="")
        {
            IncentiviEntities db = new IncentiviEntities();
            MemoryStream st = CessazioneHelper.SampleBozzaVerbale(model, db);
            string nomeFile = "Verbale " + model.Descrizione + ".docx";
            return File(st, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", nomeFile);
        }

        public ActionResult CreaVerbaleDoc(int idDip, bool originale = false)
        {
            MemoryStream st;
            string nomeFile;
            CessazioneHelper.CreaBozzaVerbale(idDip, originale, out st, out nomeFile);
            return File(st, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", nomeFile);
        }

        public ActionResult AggiungiVerbaleModificato(int idOper, HttpPostedFileBase _fileUpload, string fileName, string descr)
        {
            IncentiviEntities db = new IncentiviEntities();

            XR_INC_OPERSTATI_DOC doc = new XR_INC_OPERSTATI_DOC();
            doc.ID_ALLEGATO = db.XR_INC_OPERSTATI_DOC.GeneraPrimaryKey();
            doc.ID_OPER = idOper;
            doc.NME_FILENAME = fileName;
            doc.DES_ALLEGATO = descr;
            doc.CONTENT_TYPE = _fileUpload.ContentType;
            doc.NOT_TAG = "BozzaVerbale";
            using (MemoryStream ms = new MemoryStream())
            {
                _fileUpload.InputStream.CopyTo(ms);
                doc.OBJ_OBJECT = ms.ToArray();
            }
            doc.IND_RILEVANTE = true;
            doc.COD_USER = CommonHelper.GetCurrentUserMatricola();
            doc.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
            doc.TMS_TIMESTAMP = DateTime.Now;
            db.XR_INC_OPERSTATI_DOC.Add(doc);
            if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                return Content("OK");
            else
                return Content("Errore durante il salvataggio");
        }

        public ActionResult CreaProspetto(int idDip, bool addSign = true)
        {
            IncentiviEntities db = new IncentiviEntities();
            XR_INC_DIPENDENTI dip = db.XR_INC_DIPENDENTI.FirstOrDefault(x => x.ID_DIPENDENTE == idDip);

            try
            {
                MemoryStream ms = InternalProspetto(db, dip, 0, addSign);
                return new FileStreamResult(ms, "application/pdf") { FileDownloadName = "Prospetto " + dip.ANAGPERS.DES_COGNOMEPERS.TitleCase() + " " + dip.ANAGPERS.DES_NOMEPERS.TitleCase() + ".pdf" };
            }
            catch (Exception)
            {
                return View("~/Views/Shared/404.cshtml");
            }

        }

        public ActionResult CreaRiepilogo(int idDip)
        {
            IncentiviEntities db = new IncentiviEntities();
            XR_INC_DIPENDENTI dip = db.XR_INC_DIPENDENTI.Include("SINTESI1").FirstOrDefault(x => x.ID_DIPENDENTE == idDip);

            try
            {
                int pageNum = 0;
                MemoryStream riepilogo = CessazioneManager.CreaPDFRiepilogo(db, dip, Cons.RAI_ICON, out pageNum);

                //MemoryStream prospetto = InternalProspetto(dip, 0);
                MemoryStream verbale = null;
                if (dip.XR_INC_OPERSTATI.Any(x => x.DATA_FINE_VALIDITA == null && x.ID_STATO == (int)IncStato.VerbaleCaricato))
                {
                    XR_INC_OPERSTATI_DOC doc = dip.XR_INC_OPERSTATI.FirstOrDefault(x => x.ID_STATO == (int)IncStato.VerbaleCaricato).XR_INC_OPERSTATI_DOC.First();
                    verbale = new MemoryStream(doc.OBJ_OBJECT);
                }

                return new FileStreamResult(CessazioneManager.CreaPDFTotale(riepilogo, null, verbale), "application/pdf") { FileDownloadName = "Riepilogo " + dip.ANAGPERS.DES_COGNOMEPERS.TitleCase() + " " + dip.ANAGPERS.DES_NOMEPERS.TitleCase() + ".pdf" }; ;

            }
            catch (Exception ex)
            {
                throw ex; //return View( "~/Views/Shared/404.cshtml" );
            }
        }
        public ActionResult CreaRiepilogoZip(CessazioneTipo tipo = CessazioneTipo.Incentivazione)
        {
            string currentDir = @"c:\tmp\riepiloghi";

            using (IncentiviEntities db = new IncentiviEntities())
            {
                var isConclusa = CessazioneHelper.IsCurrentState((int)IncStato.Conclusa);
                var list = db.XR_INC_DIPENDENTI
                                .Include("SINTESI1")
                                .Include("XR_INC_OPERSTATI")
                                .Include("XR_INC_OPERSTATI.XR_INC_OPERSTATI_DOC")
                                .Where(x => x.ID_TIPOLOGIA == (int)tipo)
                                .Where(isConclusa).OrderBy(a => a.DATA_APPUNTAMENTO);

                foreach (var dip in list)
                {
                    int pageNum = 0;
                    MemoryStream riepilogo = CessazioneManager.CreaPDFRiepilogo(db, dip, Cons.RAI_ICON, out pageNum);

                    XR_INC_OPERSTATI_DOC doc = dip.XR_INC_OPERSTATI.FirstOrDefault(x => x.ID_STATO == (int)IncStato.VerbaleCaricato).XR_INC_OPERSTATI_DOC.First();
                    MemoryStream verbale = new MemoryStream(doc.OBJ_OBJECT);

                    MemoryStream totale = CessazioneManager.CreaPDFTotale(riepilogo, null, verbale);

                    using (FileStream file = new FileStream(Path.Combine(currentDir, String.Format("{0}_{1:yyyy_MM_dd}.pdf", dip.MATRICOLA, dip.DATA_APPUNTAMENTO)), FileMode.Create, System.IO.FileAccess.Write))
                    {
                        byte[] bytes = new byte[totale.Length];
                        totale.Read(bytes, 0, (int)totale.Length);
                        file.Write(bytes, 0, bytes.Length);
                        totale.Close();
                    }
                }

                return Content("OK");
            }
        }

        public ActionResult RiepilogoPagamenti(DateTime? data, string exportType = "", CessazioneTipo tipo = CessazioneTipo.Incentivazione)
        {
            IncentiviEntities db = new IncentiviEntities();

            var isVerbaleCaricato = CessazioneHelper.IsAnyState((int)IncStato.VerbaleCaricato);
            var isConteggioEffettuato = CessazioneHelper.IsAnyState((int)IncStato.Conteggio);

            var tmp = db.XR_INC_DIPENDENTI.Where(x => (!data.HasValue || x.DATA_PAGAMENTO == data));
            if (tipo == CessazioneTipo.Incentivazione)
                tmp = tmp.Where(x => x.ID_TIPOLOGIA == (int)CessazioneTipo.Incentivazione).Where(isVerbaleCaricato);
            else
                tmp = tmp.Where(x => x.ID_TIPOLOGIA == (int)CessazioneTipo.RisoluzioneConsensuale).Where(isConteggioEffettuato);

            IQueryable<XR_INC_DIPENDENTI> dip = tmp.OrderBy(w => w.DATA_PAGAMENTO.Value);

            string logoIcon = "";
            string infoSoc = "";


            logoIcon = GetImageDoc(db.XR_INC_TEMPLATE.FirstOrDefault(x => x.NME_TEMPLATE == "LOGO"));
            //infoSoc = GetImageDoc(dbDG.MyRai_Incentivi_Template.FirstOrDefault(x => x.Sede == "INTESTAZIONE"));

            try
            {
                string nomeFile = "Riepilogo SIBA" + (data.HasValue ? " " + data.Value.ToString("MMMM yyyy") : ""); ;

                if (String.IsNullOrWhiteSpace(exportType) || exportType == "pdf")
                {
                    MemoryStream riepilogo = CessazioneManager.CreaPDFPagamenti(logoIcon, infoSoc, nomeFile, dip);
                    return new FileStreamResult(riepilogo, "application/pdf") { FileDownloadName = nomeFile + ".pdf" }; ;
                }
                else if (exportType == "xlsx")
                {
                    MemoryStream riepilogo = CessazioneManager.CreaXLSPagamenti(logoIcon, infoSoc, nomeFile, dip);
                    return new FileStreamResult(riepilogo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = nomeFile + ".xlsx" }; ;
                }
                else
                {
                    return View("~/Views/Shared/404.cshtml");
                }
            }
            catch (Exception ex)
            {
                throw ex;
                //return View("~/Views/Shared/404.cshtml");
            }
        }

        public ActionResult Provvista(DateTime? data, string exportType = "", CessazioneTipo tipo = CessazioneTipo.Incentivazione)
        {
            IncentiviEntities db = new IncentiviEntities();



            Expression<Func<XR_INC_DIPENDENTI, bool>> funcFilterMatr = CessazioneHelper.FuncFilterMatr(db);
            var isConteggio = CessazioneHelper.IsAnyState((int)IncStato.Conteggio);

            IQueryable<XR_INC_DIPENDENTI> dip = db.XR_INC_DIPENDENTI
                .Where(funcFilterMatr)
                .Where(isConteggio)
                .Where(x => x.ID_TIPOLOGIA == (int)tipo)
                .Where(x => (!data.HasValue || x.DATA_CESSAZIONE == data))
                .Where(x=>x.DATA_PAGAMENTO.HasValue)
                        .OrderBy(w => w.DATA_PAGAMENTO.Value);

            string logoIcon = "";
            string infoSoc = "";

            logoIcon = GetImageDoc(db.XR_INC_TEMPLATE.FirstOrDefault(x => x.NME_TEMPLATE == "LOGO"));

            try
            {
                string nomeFile = "Provvista" + (data.HasValue ? " " + data.Value.ToString("MMMM yyyy") : "");
                if (exportType == "XLS")
                {
                    MemoryStream riepilogo = CessazioneManager.CreaXLSProvvista(logoIcon, infoSoc, nomeFile, dip);
                    return new FileStreamResult(riepilogo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = nomeFile + ".xlsx" };
                }
                else
                {
                    MemoryStream riepilogo = CessazioneManager.CreaPDFProvvista(!data.HasValue, logoIcon, infoSoc, nomeFile, dip);
                    return new FileStreamResult(riepilogo, "application/pdf") { FileDownloadName = nomeFile + ".pdf" };
                }

            }
            catch (Exception ex)
            {
                throw ex;
                //return View("~/Views/Shared/404.cshtml");
            }
            //}
        }

        public ActionResult Costi(DateTime? data, string exportType = "", CessazioneTipo tipo = CessazioneTipo.Incentivazione, string codGruppo = "")
        {
            IncentiviEntities db = new IncentiviEntities();

            Expression<Func<XR_INC_DIPENDENTI, bool>> funcFilterMatr = CessazioneHelper.FuncFilterMatr(db);
            var isConteggio = CessazioneHelper.IsAnyState((int)IncStato.Conteggio);

            IQueryable<XR_INC_DIPENDENTI> dip = db.XR_INC_DIPENDENTI;

            if (!String.IsNullOrWhiteSpace(codGruppo))
                dip = dip.Where(x => x.COD_GRUPPO == codGruppo);

            dip = dip.Where(funcFilterMatr)
                        .Where(isConteggio)
                        .Where(x => x.ID_TIPOLOGIA == (int)tipo)
                        .Where(x => (!data.HasValue || x.DATA_CESSAZIONE == data))
                                .OrderBy(w => w.DATA_PAGAMENTO.Value);

            string logoIcon = "";
            string infoSoc = "";

            logoIcon = GetImageDoc(db.XR_INC_TEMPLATE.FirstOrDefault(x => x.NME_TEMPLATE == "LOGO"));

            try
            {
                string nomeFile = "Costi" + (data.HasValue ? " " + data.Value.ToString("MMMM yyyy") : "");
                MemoryStream riepilogo = CessazioneManager.CreaPDFCosti(!data.HasValue, logoIcon, infoSoc, nomeFile, dip);
                return new FileStreamResult(riepilogo, "application/pdf") { FileDownloadName = nomeFile + ".pdf" };
            }
            catch (Exception ex)
            {
                throw ex;
                //return View("~/Views/Shared/404.cshtml");
            }
            //}
        }

        public ActionResult CronologiaVerbali(int idOper)
        {
            IncentiviEntities db = new IncentiviEntities();
            var listVerb = db.XR_INC_OPERSTATI.FirstOrDefault(x => x.ID_OPER == idOper)
                .XR_INC_OPERSTATI_DOC.Where(x => x.NOT_TAG == "BozzaVerbale").OrderBy(y => y.TMS_TIMESTAMP)
                .Select(z => new
                {
                    idVerbale = z.ID_ALLEGATO,
                    persona = CezanneHelper.GetNominativoByMatricola(z.COD_USER),
                    dataUpload = z.TMS_TIMESTAMP.ToString("dd/MM/yyyy HH:mm"),
                    descr = z.DES_ALLEGATO
                });

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = listVerb
            };
        }

        public ActionResult CronologiaBozzeVerbali(int idDip)
        {
            IncentiviEntities db = new IncentiviEntities();
            var listVerb = db.XR_INC_TEMPLATE.Where(x => x.COD_TIPO == "VerbaleDOC" && x.ID_DIPENDENTE == idDip)
                .OrderBy(y => y.VALID_DTA_INI).ToList()
                .Select(z => new
                {
                    idVerbale = z.ID_TEMPLATE,
                    persona = CezanneHelper.GetNominativoByMatricola(z.COD_USER),
                    dataUpload = z.VALID_DTA_INI.ToString("dd/MM/yyyy HH:mm"),
                    descr = z.DES_TEMPLATE ?? ""
                });

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = listVerb
            };
        }

        public ActionResult Widget_allegati(int idDip, int stato)
        {
            var db = new IncentiviEntities();
            //List<XR_INC_OPERSTATI_DOC> Allegati = InternalGetAllegati(idDip, db, true, (int)IncStato.TempFileEstratti);
            string matricola = db.XR_INC_DIPENDENTI.Where(x => x.ID_DIPENDENTE == idDip).Select(x => x.MATRICOLA).FirstOrDefault();
            myRaiCommonTasks.Helpers.FileResult files = null;
            List<XR_INC_OPERSTATI_DOC> Allegati = InternalGetAllegati(ref files, idDip, matricola, db, true, false, stato);

            return View("subpartial/widget_allegati", Allegati);
        }
        public ActionResult Dettaglio_OperAllegati(int idDip, int stato)
        {
            CessazioneAllContainer container = new CessazioneAllContainer();

            var db = new IncentiviEntities();
            string matricola = db.XR_INC_DIPENDENTI.Where(x => x.ID_DIPENDENTE == idDip).Select(x => x.MATRICOLA).FirstOrDefault();

            List<int> stati = new List<int>();
            stati.Add(stato);
            if (stato == (int)IncStato.RichiestaInserita)
            {
                stati.Add((int)IncStato.TempFileEstratti);
            }
            else if (stato == (int)IncStato.RichiestaAccettata)
            {
                stati.Add((int)IncStato.RichiestaInserita);
                stati.Add((int)IncStato.RichiestaRifiutata);
            }
            else if (stato == (int)IncStato.RecessoEffettuato)
            {
                stati.Add((int)IncStato.FileProposta);
                stati.Add((int)IncStato.FileAccettazione);
            }

            List<int> enabledStates = null;
            bool applyFilters = CessazioneHelper.GetEnabledStates(null, CommonHelper.GetCurrentUserMatricola(), AbilOper.Writing, out enabledStates);

            container.Stato = stato;
            container.IdDipendente = idDip;

            if (!applyFilters || CessazioneHelper.IsRoleAdmin(CommonHelper.GetCurrentUserMatricola()))
                container.EnabledAdd = true;
            else
            {
                //Per controllare l'abilitazione, devo controllare lo stato precedente nel workflow.
                var inc = db.XR_INC_DIPENDENTI.Find(idDip);
                var workflow = inc.XR_WKF_TIPOLOGIA.XR_WKF_WORKFLOW.OrderBy(x => x.ORDINE).Select(x => x.ID_STATO).ToList();
                int pos = workflow.IndexOf(stato);
                if (pos > 0)
                    container.EnabledAdd = enabledStates.Contains(workflow[pos - 1]);
                else
                    container.EnabledAdd = false;
            }

            myRaiCommonTasks.Helpers.FileResult files = null;
            container.Allegati = InternalGetAllegati(ref files, idDip, matricola, db, false, false, stati.ToArray());

            return View("subpartial/Dettaglio_OperAllegati", container);
        }

        private static List<XR_INC_OPERSTATI_DOC> InternalGetAllegati(ref myRaiCommonTasks.Helpers.FileResult files, int idDip, string matricola, IncentiviEntities db, bool addNew, bool disableDelete, params int[] stati)
        {
            return InternalGetAllegatiByTags(ref files, idDip, matricola, db, addNew, disableDelete, null, false, true, stati);
        }
        private static List<XR_INC_OPERSTATI_DOC> InternalGetAllegatiByTags(ref myRaiCommonTasks.Helpers.FileResult files, int idDip, string matricola, IncentiviEntities db, bool addNew, bool disableDelete, string[] tags, bool ancheApprovati, bool loadRpmFile, params int[] stati)
        {
            var tmp = db.XR_INC_OPERSTATI_DOC.Include("XR_INC_OPERSTATI").Where(x => x.XR_INC_OPERSTATI.ID_DIPENDENTE == idDip && x.VALID_DTA_END == null);

            if (stati != null && stati.Any())
            {
                if (!stati.Contains((int)IncStato.Appuntamento))
                    tmp = tmp.Where(x => stati.Contains(x.XR_INC_OPERSTATI.ID_STATO));
                else
                {
                    Expression<Func<XR_INC_OPERSTATI_DOC, bool>> filter = x => x.XR_INC_OPERSTATI.ID_STATO == (int)IncStato.Appuntamento && (x.NOT_TAG == null || x.NOT_TAG != "BozzaVerbale");
                    var tmpStati = stati.Where(x => x != (int)IncStato.Appuntamento);
                    if (tmpStati != null && tmpStati.Any())
                        filter = LinqHelper.PutInOrTogether(filter, x => tmpStati.Contains(x.XR_INC_OPERSTATI.ID_STATO));

                    tmp = tmp.Where(filter);
                }
            }

            if (tags != null && tags.Any())
                tmp = tmp.Where(x => tags.Any(y => x.NOT_TAG.Contains(y)));

            var allegati = tmp.Select(x => new
            {
                x.ID_ALLEGATO,
                x.COD_TITLE,
                x.CONTENT_TYPE,
                x.DES_ALLEGATO,
                x.NMB_SIZE,
                x.NME_FILENAME,
                x.COD_USER,
                x.TMS_TIMESTAMP,
                x.NOT_TAG,
                ID_STATO_RIF = x.XR_INC_OPERSTATI.ID_STATO
            }).ToList().Select(y => new XR_INC_OPERSTATI_DOC()
            {
                ID_ALLEGATO = y.ID_ALLEGATO,
                COD_TITLE = y.COD_TITLE,
                CONTENT_TYPE = y.CONTENT_TYPE,
                DES_ALLEGATO = y.DES_ALLEGATO,
                NMB_SIZE = y.NMB_SIZE,
                NME_FILENAME = y.NME_FILENAME,
                COD_USER = y.COD_USER,
                TMS_TIMESTAMP = y.TMS_TIMESTAMP,
                ID_STATO_RIF = y.ID_STATO_RIF,
                NOT_TAG = y.NOT_TAG,
                ElencoTag = y.NOT_TAG != null ? y.NOT_TAG.Split(';') : null,
                DisableDelete = disableDelete || ((y.NOT_TAG != null && y.NOT_TAG.Contains("Rai per Me")) || stati.Contains((int)IncStato.RichiestaInserita))
            });

            var Allegati = new List<XR_INC_OPERSTATI_DOC>();
            Allegati.AddRange(allegati);
            if (addNew && !stati.Contains((int)IncStato.RichiestaInserita))
            {
                Allegati.Add(new XR_INC_OPERSTATI_DOC()
                {
                    ID_ALLEGATO = 0,
                    //COD_TITLE = "Aggiungi un nuovo file",
                    ID_STATO_RIF = stati[0]
                });
            }

            if (loadRpmFile)
            {
                if (stati.Contains((int)IncStato.TempFileEstratti))
                    Allegati.AddRange(CessazioneManager.InternalGetAllegatiDip(ref files, idDip, matricola, (int)IncStato.TempFileEstratti, (int)IncStato.RichiestaInserita, null, false, ancheApprovati));
                else if (stati.Contains((int)IncStato.TempFileProposta))
                    Allegati.AddRange(CessazioneManager.InternalGetAllegatiDip(ref files, idDip, matricola, (int)IncStato.TempFileProposta, (int)IncStato.RichiestaAccettata, "Proposta", false, ancheApprovati));
                else if (stati.Contains((int)IncStato.TempFileAccettazione))
                {
                    Allegati.AddRange(CessazioneManager.InternalGetAllegatiDip(ref files, idDip, matricola, (int)IncStato.TempFileAccettazione, (int)IncStato.RichiestaAccettata, "Accettazione", false, ancheApprovati));
                    Allegati.AddRange(CessazioneManager.InternalGetAllegatiDip(ref files, idDip, matricola, (int)IncStato.TempFileAccettazione, (int)IncStato.RichiestaAccettata, "Recesso", false, ancheApprovati));
                    Allegati.AddRange(CessazioneManager.InternalGetAllegatiDip(ref files, idDip, matricola, (int)IncStato.TempFileAccettazione, (int)IncStato.RichiestaAccettata, null, false, ancheApprovati));
                }
            }

            return Allegati;
        }

        #endregion

        #region NotePratica
        public ActionResult GetNoteDip()
        {
            XLWorkbook workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Elenco note");

            int counter = 1;
            worksheet.Cell(counter, 1).Value = "Matricola";
            worksheet.Cell(counter, 2).Value = "Nominativo";
            worksheet.Cell(counter, 3).Value = "Nota";
            worksheet.Cell(counter, 4).Value = "Autore";
            worksheet.Cell(counter, 5).Value = "Conto Vincolato";
            worksheet.Cell(counter, 6).Value = "Causa/Pignoramento";
            worksheet.Cell(counter, 7).Value = "Vincolo BCCR";

            IncentiviEntities db = new IncentiviEntities();
            foreach (var dip in db.XR_INC_DIPENDENTI)
            {
                var datiBancari = dip.ANAGPERS.XR_DATIBANCARI.OrderByDescending(y => y.DTA_FINE).FirstOrDefault(x => x.XR_UTILCONTO.Count() == 1 && x.XR_UTILCONTO.First().COD_UTILCONTO == "01");

                if (dip.XR_INC_DIPENDENTI_NOTE != null && dip.XR_INC_DIPENDENTI_NOTE.Count > 0)
                {
                    foreach (var nota in dip.XR_INC_DIPENDENTI_NOTE)
                    {
                        counter++;
                        worksheet.Cell(counter, 1).Value = dip.MATRICOLA;
                        worksheet.Cell(counter, 2).Value = dip.ANAGPERS.DES_COGNOMEPERS.TitleCase() + " " + dip.ANAGPERS.DES_NOMEPERS.TitleCase();
                        worksheet.Cell(counter, 3).Value = nota.NOTA;
                        worksheet.Cell(counter, 4).Value = nota.ANAGPERS.DES_COGNOMEPERS.TitleCase() + " " + nota.ANAGPERS.DES_NOMEPERS.TitleCase();
                        worksheet.Cell(counter, 5).Value = datiBancari.IND_VINCOLATO == "Y";
                        worksheet.Cell(counter, 6).Value = dip.CAUSE_VERTENZE;
                        worksheet.Cell(counter, 7).Value = String.IsNullOrWhiteSpace(dip.IND_PROPRIO_IBAN) ? "non inserito" : dip.IND_PROPRIO_IBAN == "B" ? "Vincolo presente" : "Nessun Vincolo";
                    }

                }
                else
                {
                    counter++;
                    worksheet.Cell(counter, 1).Value = dip.MATRICOLA;
                    worksheet.Cell(counter, 2).Value = dip.ANAGPERS.DES_COGNOMEPERS.TitleCase() + " " + dip.ANAGPERS.DES_NOMEPERS.TitleCase();
                    worksheet.Cell(counter, 3).Value = "";
                    worksheet.Cell(counter, 4).Value = "";
                    worksheet.Cell(counter, 5).Value = datiBancari.IND_VINCOLATO == "Y";
                    worksheet.Cell(counter, 6).Value = dip.CAUSE_VERTENZE;
                    worksheet.Cell(counter, 7).Value = String.IsNullOrWhiteSpace(dip.IND_PROPRIO_IBAN) ? "non inserito" : dip.IND_PROPRIO_IBAN == "B" ? "Vincolo presente" : "Nessun Vincolo";
                }
            }

            worksheet.Columns().AdjustToContents();

            MemoryStream M = new MemoryStream();
            workbook.SaveAs(M);
            M.Position = 0;

            return new FileStreamResult(M, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = "ElencoNote.xlsx" };
        }
        private string InternalAggiungiNotaPratica(int idDipendente, string notaPratica, int idPersona = 0, string tag = "")
        {
            string result = "";

            if (CessazioneHelper.AggiungiNotaPratica(null, idDipendente, notaPratica, idPersona, true, "", tag))
                result = "OK";
            else
                result = "Errore nel salvataggio";

            return result;
        }
        public ActionResult InviaNotaPratica(int idNota)
        {
            string result = "";

            IncentiviEntities db = new IncentiviEntities();
            var nota = db.XR_INC_DIPENDENTI_NOTE.Find(idNota);
            if (nota == null)
                result = "Nota non trovata";
            else if (!nota.NOT_TAG.Contains("Dipendente"))
                result = "La nota non è predisposta per l'invio al dipendente";
            else
            {
                string[] rifGruppo = HrisHelper.GetParametri<string>(HrisParam.IncentiviRifGruppo);
                var codGruppo = rifGruppo[0];
                var annoRif = rifGruppo[1];

                string mittente = CessazioneHelper.GetIndirizzoMail("FromNota");
                string cc = mittente;

                //var template = db.XR_INC_TEMPLATE.FirstOrDefault(x => x.COD_TIPO == "TemplateMail" && x.NME_TEMPLATE == "NotificaNota");
                var dip = nota.XR_INC_DIPENDENTI;
                var template = CessazioneHelper.GetTemplate(db, "TemplateMail", dip.ID_DIPENDENTE, "NotificaNota", false);

                GestoreMail mail = new GestoreMail();
                string mailDest = CessazioneHelper.GetMailDip(dip); //CommonHelper.GetEmailPerMatricola(dip.MATRICOLA);
                string mailOggetto = CessazioneHelper.ReplaceToken(dip, template.DES_TEMPLATE);
                string mailCorpo = template.TEMPLATE_TEXT.Replace("__TESTO_NOTA__", nota.NOTA);
                var response = mail.InvioMail(mailCorpo, mailOggetto, mailDest, cc, mittente, null, null);
                if (response != null && response.Errore != null)
                {
                    result = "Errore durante l'invio della mail";
                }
                else
                {
                    nota.DTA_INVIO_DIP = DateTime.Now;
                    if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                        result = "OK";
                    else
                        result = "L'invio è stato effettuato con successo, tuttavia i dati non sono stati aggiornati correttamente.";
                }
            }

            return Content(result);
        }
        public ActionResult AggiungiNotaPratica(int idDipendente, string notaPratica, bool notificaDip = false, string tag = "")
        {
            if (notificaDip)
                tag += (!String.IsNullOrWhiteSpace(tag) ? ";" : "") + "Dipendente";

            string result = InternalAggiungiNotaPratica(idDipendente, notaPratica, 0, tag);

            return Content(result);
        }
        public ActionResult CancellaNotaPratica(int idNotaPratica)
        {
            string result = "";

            IncentiviEntities db = new IncentiviEntities();
            XR_INC_DIPENDENTI_NOTE nota = db.XR_INC_DIPENDENTI_NOTE.FirstOrDefault(x => x.ID_NOTA == idNotaPratica);
            if (nota != null)
            {
                db.XR_INC_DIPENDENTI_NOTE.Remove(nota);
                try
                {
                    if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                        result = "OK";
                    else
                        result = "Errore nella cancellazione";
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
        public ActionResult ModificaNotaPratica(int idNota, string notaPratica, bool notificaDip = false)
        {
            string result = "";

            int idPersona = CommonHelper.GetCurrentIdPersona();
            IncentiviEntities db = new IncentiviEntities();
            XR_INC_DIPENDENTI_NOTE nota = db.XR_INC_DIPENDENTI_NOTE.Find(idNota);
            nota.NOTA_ORIG = nota.NOTA;
            nota.NOTA = notaPratica;
            nota.LAST_MOD_ID_PERSONA = idPersona;
            nota.LAST_MOD_USER = CommonHelper.GetCurrentUserMatricola();
            nota.LAST_MOD_TIMESTAMP = DateTime.Now;

            List<string> tag = new List<string>();
            if (!String.IsNullOrWhiteSpace(nota.NOT_TAG))
                tag.AddRange(nota.NOT_TAG.Split(';'));

            if (notificaDip && !tag.Contains("Dipendente"))
                tag.Add("Dipendente");
            else if (!notificaDip && tag.Contains("Dipendente"))
                tag.Remove("Dipendente");

            nota.NOT_TAG = String.Join(";", tag);

            try
            {
                if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                    result = "OK";
                else
                    result = "Errore nel salvataggio";
            }
            catch (Exception)
            {
                result = "Errore nel salvataggio";
            }
            return Content(result);
        }

        [HttpGet]
        public ActionResult GetNotePratica(int idDip, string openFunction)
        {
            int idPersona = CezanneHelper.GetIdPersona(CommonHelper.GetCurrentUserMatricola());

            XR_INC_DIPENDENTI incentivato = new XR_INC_DIPENDENTI();
            IncentiviEntities db = new IncentiviEntities();
            CessazioneModel model = new CessazioneModel();
            model.Pratica = db.XR_INC_DIPENDENTI.FirstOrDefault(x => x.ID_DIPENDENTE == idDip);
            model.OpenFunction = openFunction;

            return PartialView("~/Views/Cessazione/subpartial/Dettaglio_Note.cshtml", model);
        }
        #endregion

        #region NoteStato
        public ActionResult AggiungiNotaStato(int idOper, string notaPratica)
        {
            string result = "";

            IncentiviEntities db = new IncentiviEntities();
            XR_INC_OPERSTATI_NOTE nota = new XR_INC_OPERSTATI_NOTE();
            nota.ID_NOTA = db.XR_INC_OPERSTATI_NOTE.GeneraPrimaryKey();
            nota.ID_OPER = idOper;
            nota.ID_PERSONA = CezanneHelper.GetIdPersona(CommonHelper.GetCurrentUserMatricola());
            nota.NOTA = notaPratica;
            nota.COD_USER = CommonHelper.GetCurrentUserMatricola();
            nota.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
            nota.TMS_TIMESTAMP = DateTime.Now;
            db.XR_INC_OPERSTATI_NOTE.Add(nota);

            try
            {
                if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                    result = "OK";
                else
                    result = "Errore nel salvataggio";
            }
            catch (Exception)
            {
                result = "Errore nel salvataggio";
            }

            return Content(result);
        }
        public ActionResult CancellaNotaStato(int idNotaStato)
        {
            string result = "";

            IncentiviEntities db = new IncentiviEntities();
            XR_INC_OPERSTATI_NOTE nota = db.XR_INC_OPERSTATI_NOTE.FirstOrDefault(x => x.ID_NOTA == idNotaStato);
            if (nota != null)
            {
                db.XR_INC_OPERSTATI_NOTE.Remove(nota);
                try
                {
                    if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                        result = "OK";
                    else
                        result = "Errore nella cancellazione";
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
        public ActionResult GetNoteStato(int idOper)
        {
            int idPersona = CezanneHelper.GetIdPersona(CommonHelper.GetCurrentUserMatricola());

            IncentiviEntities db = new IncentiviEntities();
            XR_INC_OPERSTATI oper = db.XR_INC_OPERSTATI.FirstOrDefault(x => x.ID_OPER == idOper);

            return PartialView("~/Views/Cessazione/subpartial/Dettaglio_OperNote.cshtml", oper);
        }
        #endregion

        #region SubmitStati
        public ActionResult ModificaDatiContabili(XR_INC_DIPENDENTI modDip, bool vincoloBCCR = false, string causeVertenze = "", bool pignoramento = false, bool estAnticipata = false, bool cessioneQuinto = false)
        {
            string result = "";

            IncentiviEntities db = new IncentiviEntities();
            if (!ModelState.IsValid)
                result = "Errore nei dati";
            else
            {
                XR_INC_DIPENDENTI dip = db.XR_INC_DIPENDENTI.FirstOrDefault(x => x.ID_DIPENDENTE == modDip.ID_DIPENDENTE);
                if (dip == null)
                    result = "Dipendente non trovato";
                else
                {
                    string qualSaltaUfficioPrestiti = HrisHelper.GetParametro<string>(HrisParam.IncentiviQualificheSaltaUfficioPrestiti);
                    bool saltaUff = !String.IsNullOrWhiteSpace(qualSaltaUfficioPrestiti) && qualSaltaUfficioPrestiti.Split(',').Any(x => dip.SINTESI1.COD_QUALIFICA.StartsWith(x));
                    if (saltaUff)
                    {
                        if (vincoloBCCR || !String.IsNullOrWhiteSpace(causeVertenze) || pignoramento || estAnticipata || cessioneQuinto)
                        {
                            if (!String.IsNullOrWhiteSpace(causeVertenze)
                                && (String.IsNullOrWhiteSpace(dip.CAUSE_VERTENZE) || dip.CAUSE_VERTENZE != causeVertenze))
                                dip.CAUSE_VERTENZE = causeVertenze;

                            if (vincoloBCCR)
                                dip.IND_PROPRIO_IBAN = "B";

                            if (pignoramento)
                                dip.IND_PIGNORAMENTO = true;

                            if (estAnticipata)
                                dip.IND_ESTIN_ANT_PRESTITO = true;

                            if (cessioneQuinto)
                                dip.IND_CESSIONE_QUINTO_TFR = true;
                        }
                    }
                    CessazioneHelper.GetImportoAltreTrattenute(db, dip);

                    //I dati ricavati da DB2 relativi all'incentivo lordo sono comprensivi di incentivo e ex-fissa
                    //Per tale motivo qui il dato è da scorporare
                    decimal incentivoTotale = modDip.INCENTIVO_LORDO.GetValueOrDefault();
                    if (incentivoTotale > 0 && dip.EX_FISSA > 0)
                        incentivoTotale = incentivoTotale - dip.EX_FISSA.GetValueOrDefault();

                    dip.INCENTIVO_LORDO = incentivoTotale;
                    dip.UNA_TANTUM_LORDA = modDip.UNA_TANTUM_LORDA;
                    dip.IMPORTO_LORDO = modDip.IMPORTO_LORDO;
                    dip.IMPORTO_NETTO = modDip.IMPORTO_NETTO;
                    dip.COD_USER = CommonHelper.GetCurrentUserMatricola();
                    dip.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
                    dip.TMS_TIMESTAMP = DateTime.Now;
                    DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());

                    if (saltaUff)
                        SalvaStato(modDip.ID_DIPENDENTE, (int)IncStato.Controllato);

                    SalvaStato(modDip.ID_DIPENDENTE, (int)IncStato.Conteggio);
                    result = "OK";
                    RilasciaPratica(modDip.ID_DIPENDENTE);

                }
            }

            return Content(result);
        }
        
        public ActionResult ModificaDatiBozza(CessazioneModel modDip, bool prosegui = false)
        {
            string result = "";

            IncentiviEntities db = new IncentiviEntities();
            if (!ModelState.IsValid)
                result = "Errore nei dati";
            else
            {
                XR_INC_DIPENDENTI dip = db.XR_INC_DIPENDENTI.FirstOrDefault(x => x.ID_DIPENDENTE == modDip.Pratica.ID_DIPENDENTE);
                if (dip == null)
                    result = "Dipendente non trovato";
                else
                {
                    dip.DATA_PAGAMENTO = modDip.Pratica.DATA_PAGAMENTO;

                    dip.DATA_APPUNTAMENTO = modDip.Pratica.DATA_APPUNTAMENTO;
                    dip.NOT_LUOGO_APPUNTAMENTO = modDip.Pratica.NOT_LUOGO_APPUNTAMENTO;

                    dip.IND_ITL = modDip.IndItl;
                    dip.NUM_BOZZA_GIORNI = modDip.Pratica.NUM_BOZZA_GIORNI;

                    dip.ID_RAPPRRAI = modDip.Pratica.ID_RAPPRRAI;
                    dip.ID_SIGLASIND = modDip.Pratica.ID_SIGLASIND;
                    dip.ID_RAPPRSINDACATO = modDip.Pratica.ID_RAPPRSINDACATO;
                    dip.ID_RAPPRINDUSTRIA = modDip.Pratica.ID_RAPPRINDUSTRIA;
                    dip.IND_PROPRIO_IBAN = modDip.Pratica.IND_PROPRIO_IBAN;
                    dip.BANCA = modDip.Pratica.BANCA;
                    dip.IBAN = modDip.Pratica.IBAN;
                    dip.INTESTATARIO_CONTO = modDip.Pratica.INTESTATARIO_CONTO;

                    dip.DATA_BOZZA_INVIO = modDip.Pratica.DATA_BOZZA_INVIO;
                    dip.DATA_BOZZA_RICEZIONE = modDip.Pratica.DATA_BOZZA_RICEZIONE;

                    dip.COD_USER = CommonHelper.GetCurrentUserMatricola();
                    dip.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
                    dip.TMS_TIMESTAMP = DateTime.Now;
                    if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                    {
                        result = "OK";
                        if (prosegui)
                        {
                            SalvaStato(dip.ID_DIPENDENTE, (int)IncStato.BozzaVerbale);
                            RilasciaPratica(dip.ID_DIPENDENTE);
                        }
                    }
                    else
                        result = "Errore durante il salvataggio dei dati";
                }
            }

            return Content(result);
        }
        public ActionResult ModificaDatiAppuntamento(CessazioneModel modDip, bool prosegui, bool sendReminder = false)
        {
            string result = "";

            IncentiviEntities db = new IncentiviEntities();
            if (!ModelState.IsValid)
                result = "Errore nei dati";
            else
            {
                XR_INC_DIPENDENTI dip = db.XR_INC_DIPENDENTI.FirstOrDefault(x => x.ID_DIPENDENTE == modDip.Pratica.ID_DIPENDENTE);
                if (dip == null)
                    result = "Dipendente non trovato";
                else
                {
                    if (!dip.DATA_BOZZA_INVIO.HasValue &&
                        (dip.DATA_APPUNTAMENTO.GetValueOrDefault() != modDip.Pratica.DATA_APPUNTAMENTO.GetValueOrDefault()
                        || dip.IND_ITL.GetValueOrDefault() != modDip.IndItl
                        || dip.ID_RAPPRRAI.GetValueOrDefault() != modDip.Pratica.ID_RAPPRRAI.GetValueOrDefault()
                        || dip.ID_SIGLASIND.GetValueOrDefault() != modDip.Pratica.ID_SIGLASIND.GetValueOrDefault()
                        || dip.ID_RAPPRSINDACATO.GetValueOrDefault() != modDip.Pratica.ID_RAPPRSINDACATO.GetValueOrDefault()
                        || dip.ID_RAPPRINDUSTRIA.GetValueOrDefault() != modDip.Pratica.ID_RAPPRINDUSTRIA.GetValueOrDefault()
                        || dip.IND_PROPRIO_IBAN != modDip.Pratica.IND_PROPRIO_IBAN
                        || dip.BANCA != modDip.Pratica.BANCA
                        || dip.IBAN != modDip.Pratica.IBAN
                        || dip.INTESTATARIO_CONTO != modDip.Pratica.INTESTATARIO_CONTO
                        || dip.DATA_PAGAMENTO != modDip.Pratica.DATA_PAGAMENTO
                        || dip.GetField<string>("VeicoloTipologia")!=modDip.VeicoloTipologia
                        || dip.GetField<string>("VeicoloTarga") != modDip.VeicoloTarga)
                        )
                    {
                        //Dato che sono cambiati i dati per la bozza,
                        //questa deve essere cancellata e rigenerata
                        var oldPDF = db.XR_INC_TEMPLATE.Where(x => x.VALID_DTA_INI < DateTime.Now && (x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now))
                                                       .FirstOrDefault(x => x.COD_TIPO == "VerbalePDF" && x.ID_DIPENDENTE == modDip.Pratica.ID_DIPENDENTE);

                        if (oldPDF != null)
                        {
                            db.XR_INC_TEMPLATE.Remove(oldPDF);
                            TaskHelper.AddBatchRunnerTask("IncentiviVerbaleOnDemand", out var taskErr, arguments:dip.MATRICOLA, note: String.Format("Matricola {0} - IdDip: {1}", dip.MATRICOLA, dip.ID_DIPENDENTE));
                        }
                    }

                    dip.DATA_PAGAMENTO = modDip.Pratica.DATA_PAGAMENTO;

                    dip.DATA_APPUNTAMENTO = modDip.Pratica.DATA_APPUNTAMENTO;
                    dip.NOT_LUOGO_APPUNTAMENTO = modDip.Pratica.NOT_LUOGO_APPUNTAMENTO;

                    dip.IND_ITL = modDip.IndItl;
                    dip.NUM_BOZZA_GIORNI = modDip.Pratica.NUM_BOZZA_GIORNI;

                    dip.DATA_BOZZA_INVIO = modDip.Pratica.DATA_BOZZA_INVIO;
                    dip.DATA_BOZZA_RICEZIONE = modDip.Pratica.DATA_BOZZA_RICEZIONE;

                    dip.ID_RAPPRRAI = modDip.Pratica.ID_RAPPRRAI;
                    dip.ID_SIGLASIND = modDip.Pratica.ID_SIGLASIND;
                    dip.ID_RAPPRSINDACATO = modDip.Pratica.ID_RAPPRSINDACATO;
                    dip.ID_RAPPRINDUSTRIA = modDip.Pratica.ID_RAPPRINDUSTRIA;
                    dip.IND_PROPRIO_IBAN = modDip.Pratica.IND_PROPRIO_IBAN;
                    dip.BANCA = modDip.Pratica.BANCA;
                    dip.IBAN = modDip.Pratica.IBAN;
                    dip.INTESTATARIO_CONTO = modDip.Pratica.INTESTATARIO_CONTO;

                    dip.SetField("VeicoloTipologia", modDip.VeicoloTipologia);
                    dip.SetField("VeicoloTarga", modDip.VeicoloTarga);

                    dip.COD_USER = CommonHelper.GetCurrentUserMatricola();
                    dip.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
                    dip.TMS_TIMESTAMP = DateTime.Now;

                    if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                    {
                        result = "OK";
                        int idOper = 0;
                        if (prosegui)
                            idOper = SalvaStato(dip.ID_DIPENDENTE, (int)IncStato.Appuntamento);
                        else
                            idOper = dip.XR_INC_OPERSTATI.Where(x => x.ID_STATO == (int)IncStato.Appuntamento && x.DATA_FINE_VALIDITA == null).Select(x => x.ID_OPER).FirstOrDefault();

                        //Se è arrivato qui, controlla se è già stato inviato e accettato il verbale. 
                        //Se sì, controlla se presente un template VerbaleDOC. 
                        //Se è presente, lo copia in XR_INC_OPERSTATI_DOC rimuovendo il watermark

                        if (idOper > 0)
                        {
                            if (dip.DATA_BOZZA_RICEZIONE != null && !db.XR_INC_OPERSTATI_DOC.Any(x => x.ID_OPER == idOper))
                            {
                                var modifiedVerb = db.XR_INC_TEMPLATE.FirstOrDefault(w => w.ID_DIPENDENTE != null && w.ID_DIPENDENTE == dip.ID_DIPENDENTE && w.COD_TIPO == "VerbaleDOC" && w.VALID_DTA_END == null);
                                if (modifiedVerb != null)
                                {
                                    MemoryStream st = CessazioneHelper.IncludeWatermark(modifiedVerb);

                                    XR_INC_OPERSTATI_DOC doc = new XR_INC_OPERSTATI_DOC();
                                    doc.ID_ALLEGATO = db.XR_INC_OPERSTATI_DOC.GeneraPrimaryKey();
                                    doc.ID_OPER = idOper;
                                    doc.NME_FILENAME = "Verbale " + dip.SINTESI1.Nominativo() + ".docx";
                                    doc.DES_ALLEGATO = "Bozza verbale accettata il " + dip.DATA_BOZZA_RICEZIONE.Value.ToString("dd/MM/yyyy");
                                    doc.CONTENT_TYPE = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                                    doc.OBJ_OBJECT = st.ToArray();
                                    doc.NOT_TAG = "BozzaVerbale";
                                    doc.COD_USER = modifiedVerb.COD_USER;
                                    doc.COD_TERMID = modifiedVerb.COD_TERMID;
                                    doc.TMS_TIMESTAMP = modifiedVerb.TMS_TIMESTAMP;
                                    doc.IND_RILEVANTE = true;
                                    db.XR_INC_OPERSTATI_DOC.Add(doc);
                                    DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                                }
                            }
                        }
                    }
                    else
                        result = "Errore durante il salvataggio dei dati";
                }
            }

            return Content(result);
        }

        public ActionResult VerbaleRifiutato(int idDip)
        {
            IncentiviEntities db = new IncentiviEntities();
            XR_INC_DIPENDENTI dip = db.XR_INC_DIPENDENTI.FirstOrDefault(x => x.ID_DIPENDENTE == idDip);
            InvalidaStatoInternal(dip.XR_INC_OPERSTATI.FirstOrDefault(x => x.ID_STATO == (int)IncStato.Appuntamento).ID_OPER);
            InvalidaStatoInternal(dip.XR_INC_OPERSTATI.FirstOrDefault(x => x.ID_STATO == (int)IncStato.Conteggio).ID_OPER);

            string testoNota = String.Format("Verbale rifiutato in data {0}\r\nIncentivo lordo:{1:N} €\r\nImporto lordo:{2:N} €\r\nImporto netto:{3:N}",
                                            DateTime.Today.ToString("dd/MM/yyyy"), (dip.INCENTIVO_LORDO ?? 0 + dip.EX_FISSA ?? 0), dip.IMPORTO_LORDO, dip.IMPORTO_NETTO);
            InternalAggiungiNotaPratica(dip.ID_DIPENDENTE, testoNota, dip.ID_PERSONA);

            dip.DATA_APPUNTAMENTO = null;
            dip.ID_RAPPRINDUSTRIA = null;
            dip.ID_RAPPRRAI = null;
            dip.ID_RAPPRSINDACATO = null;
            dip.ID_SIGLASIND = null;

            //dip.INCENTIVO_LORDO = null;
            //dip.IMPORTO_LORDO = null;
            //dip.IMPORTO_NETTO = null;

            //dip.IND_PROPRIO_IBAN = null;
            //dip.BANCA = null;
            //dip.IBAN = null;
            //dip.INTESTATARIO_CONTO = null;

            dip.COD_USER = CommonHelper.GetCurrentUserMatricola();
            dip.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
            dip.TMS_TIMESTAMP = DateTime.Now;
            DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());

            RilasciaPratica(idDip);
            return Content("OK");
        }
        public ActionResult ModificaDatiVerbaleFirma(int idDip)
        {
            IncentiviEntities db = new IncentiviEntities();

            SalvaStato(idDip, (int)IncStato.VerbaleFirmato);
            RilasciaPratica(idDip);
            return Content("OK");
        }
        public ActionResult ModificaDatiVerbale(int idDip, HttpPostedFileBase _fileUpload, string fileName, string descr = "Verbale scansionato")
        {
            IncentiviEntities db = new IncentiviEntities();

            int idOper = SalvaStato(idDip, (int)IncStato.VerbaleCaricato);

            XR_INC_OPERSTATI_DOC doc = new XR_INC_OPERSTATI_DOC();
            doc.ID_ALLEGATO = db.XR_INC_OPERSTATI_DOC.GeneraPrimaryKey();
            doc.ID_OPER = idOper;
            doc.NME_FILENAME = Path.ChangeExtension(fileName, ".pdf");
            doc.DES_ALLEGATO = descr;
            doc.CONTENT_TYPE = _fileUpload.ContentType;
            using (MemoryStream ms = new MemoryStream())
            {
                _fileUpload.InputStream.CopyTo(ms);
                doc.OBJ_OBJECT = ms.ToArray();
            }
            doc.COD_USER = CommonHelper.GetCurrentUserMatricola();
            doc.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
            doc.TMS_TIMESTAMP = DateTime.Now;
            doc.IND_RILEVANTE = true;
            db.XR_INC_OPERSTATI_DOC.Add(doc);
            DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());

            RilasciaPratica(idDip);
            return Content("OK");
        }

        [HttpPost]
        public ActionResult UploadFile(int idDip, HttpPostedFileBase _file, string fileName, string tipologia, string titolo, string desc, int stato, string[] tags)
        {
            IncentiviEntities db = new IncentiviEntities();
            //Creo uno stato temporaneo in quanto l'accettazione/rifiuto possono non esssere contestuali al caricamento del file
            //int idOper = SalvaStato(idDip, (int)IncStato.TempFileEstratti);
            int idOper = SalvaStato(idDip, stato);

            XR_INC_OPERSTATI_DOC doc = new XR_INC_OPERSTATI_DOC();
            doc.ID_ALLEGATO = db.XR_INC_OPERSTATI_DOC.GeneraPrimaryKey();
            doc.ID_OPER = idOper;
            doc.NME_FILENAME = fileName;
            doc.COD_TITLE = titolo;
            doc.DES_ALLEGATO = desc;
            doc.CONTENT_TYPE = _file.ContentType;
            using (MemoryStream ms = new MemoryStream())
            {
                _file.InputStream.CopyTo(ms);
                doc.OBJ_OBJECT = ms.ToArray();
            }
            doc.NMB_SIZE = _file.ContentLength;
            if (tags != null && tags.Any())
                doc.NOT_TAG = String.Join(";", tags);
            doc.IND_RILEVANTE = true;
            doc.COD_USER = CommonHelper.GetCurrentUserMatricola();
            doc.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
            doc.TMS_TIMESTAMP = DateTime.Now;
            db.XR_INC_OPERSTATI_DOC.Add(doc);
            DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());

            return Content("OK");
        }

        public ActionResult ModificaDatiPagamento(XR_INC_DIPENDENTI modDip)
        {
            string result = "";

            IncentiviEntities db = new IncentiviEntities();

            XR_INC_DIPENDENTI dip = db.XR_INC_DIPENDENTI.FirstOrDefault(x => x.ID_DIPENDENTE == modDip.ID_DIPENDENTE);
            if (dip == null)
                result = "Dipendente non trovato";
            else
            {
                //dip.IMPORTO_NETTO = modDip.IMPORTO_NETTO;
                //dip.COD_USER = CommonHelper.GetCurrentUserMatricola();
                //dip.COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress;
                //dip.TMS_TIMESTAMP = DateTime.Now;
                //db.SaveChanges();
                //SalvaStato(modDip.ID_DIPENDENTE, (int)IncStato.Cedolini);
                RilasciaPratica(modDip.ID_DIPENDENTE);
                result = "OK";
            }

            return Content(result);
        }

        public ActionResult CreateICSAppuntamento(int idDip)
        {
            IncentiviEntities db = new IncentiviEntities();
            var dip = db.XR_INC_DIPENDENTI.First(x => x.ID_DIPENDENTE == idDip);
            string nominativo = dip.ANAGPERS.DES_COGNOMEPERS.TitleCase() + " " + dip.ANAGPERS.DES_NOMEPERS.TitleCase();

            var icalStringbuilder = new StringBuilder();

            icalStringbuilder.AppendLine("BEGIN:VCALENDAR");
            icalStringbuilder.AppendLine("PRODID:RaiPerMe");
            icalStringbuilder.AppendLine("VERSION:2.0");

            icalStringbuilder.AppendLine("BEGIN:VEVENT");
            icalStringbuilder.AppendLine("SUMMARY;LANGUAGE=it-it:" + "Incentivi 2018: Appuntamento " + nominativo);
            icalStringbuilder.AppendLine("CLASS:PUBLIC");
            icalStringbuilder.AppendLine(string.Format("CREATED:{0:yyyyMMddTHHmmssZ}", DateTime.UtcNow));
            icalStringbuilder.AppendLine(string.Format("DESCRIPTION;ENCODING=QUOTED-PRINTABLE:Appuntamento {0}=0D=0ASigla Sindacale: {1}", nominativo, dip.XR_INC_SIGLESINDACALI != null ? dip.XR_INC_SIGLESINDACALI.SINDACATO : " da decidere"));
            icalStringbuilder.AppendLine(string.Format("DTSTART:{0:yyyyMMddTHHmmssZ}", dip.DATA_APPUNTAMENTO.Value));
            icalStringbuilder.AppendLine(string.Format("DTEND:{0:yyyyMMddTHHmmssZ}", dip.DATA_APPUNTAMENTO.Value.AddMinutes(30)));
            icalStringbuilder.AppendLine("SEQUENCE:0");
            icalStringbuilder.AppendLine("UID:" + Guid.NewGuid());
            icalStringbuilder.AppendLine(string.Format("LOCATION:{0}", dip.SEDE).Trim());
            icalStringbuilder.AppendLine("END:VEVENT");
            icalStringbuilder.AppendLine("END:VCALENDAR");

            var bytes = Encoding.UTF8.GetBytes(icalStringbuilder.ToString());

            string matricola = CommonHelper.GetCurrentUserMatricola();

            MailSender invia = new MailSender();
            Email eml = new Email();
            eml.From = CommonHelper.GetEmailPerMatricola(matricola);
            eml.toList = new string[] { CommonHelper.GetEmailPerMatricola(matricola) };
            eml.ContentType = "text/html";
            eml.Priority = 2;
            eml.SendWhen = DateTime.Now.AddSeconds(1);
            eml.AttachementsList = new Attachement[]
            {
                new Attachement()
                {
                    AttachementName = "Appuntamento "+nominativo+".ics",
                    AttachementType = "",
                    AttachementValue = bytes
                }
            };

            eml.Body = "<b>Appuntamento " + nominativo + "</b><br/>";
            eml.Subject = "Incentivi 2018: Appuntamento " + nominativo;

            try
            {
                invia.Send(eml);
                return Content("OK");
            }
            catch (Exception)
            {
                return Content("Si è verificato un errore imprevisto");
            }
        }
        public ActionResult InvalidaStato(int idOper)
        {
            InvalidaStatoInternal(idOper);
            return Content("OK");
        }
        #endregion

        #region SelectList
        public static List<SelectListItem> getTipologie(bool addDefault = false)
        {
            List<SelectListItem> lista = new List<SelectListItem>();

            if (addDefault)
                lista.Add(new SelectListItem() { Value = "", Text = "Seleziona la tipologia di cessazione" });

            lista.AddRange(CessazioneManager.GetTipologie().Select(x => new SelectListItem() { Value = x.ID_TIPOLOGIA.ToString(), Text = x.DES_TIPOLOGIA }));

            return lista;
        }
        public static List<ListItem> getStati()
        {
            IncentiviEntities db = new IncentiviEntities();
            List<ListItem> lista = new List<ListItem>();
            List<int> enabledStates = null;
            bool applyFilterStates = CessazioneHelper.GetEnabledStates(db, CommonHelper.GetCurrentUserMatricola(), AbilOper.Reading, out enabledStates);

            var listStati = HrisHelper.GetParametro(HrisParam.IncentiviStatiVerbale);
            if (listStati != null && CessazioneHelper.EnabledToAnySubFunc(CommonHelper.GetCurrentUserMatricola(), listStati.COD_VALUE2.Split(',')))
                enabledStates.AddRange(listStati.COD_VALUE1.Split(',').Select(x => Convert.ToInt32(x)));

            foreach (var stato in db.XR_INC_STATI.Where(x => x.ID_STATO > 0 && (!applyFilterStates || enabledStates.Contains(x.ID_STATO))))
            {
                ListItem item = new ListItem()
                {
                    Value = stato.ID_STATO.ToString(),
                    Text = stato.DESCRIZIONE
                };
                lista.Add(item);
            }

            return lista;
        }
        public static List<SelectListGroup> GetStatiTipologia()
        {
            IncentiviEntities db = new IncentiviEntities();
            List<SelectListGroup> result = new List<SelectListGroup>();

            List<int> enabledStates = null;
            bool applyFilterStates = CessazioneHelper.GetEnabledStates(db, CommonHelper.GetCurrentUserMatricola(), AbilOper.Reading, out enabledStates);

            var listStati = HrisHelper.GetParametro(HrisParam.IncentiviStatiVerbale);
            if (listStati != null && CessazioneHelper.EnabledToAnySubFunc(CommonHelper.GetCurrentUserMatricola(), listStati.COD_VALUE2.Split(',')))
                enabledStates.AddRange(listStati.COD_VALUE1.Split(',').Select(x => Convert.ToInt32(x)));

            var tmp = db.XR_WKF_WORKFLOW.Where(x => x.XR_WKF_TIPOLOGIA.COD_PREFIX_TABELLA == "INC" && (!applyFilterStates || enabledStates.Contains(x.ID_STATO)));

            foreach (var gr in tmp.GroupBy(x => x.XR_WKF_TIPOLOGIA.DES_TIPOLOGIA))
            {
                SelectListGroup group = new SelectListGroup();
                group.Name = gr.Key;
                group.ListItems = new List<ListItem>();
                foreach (var item in gr.OrderBy(x => x.ORDINE))
                {
                    group.ListItems.Add(new ListItem()
                    {
                        Value = item.ID_STATO.ToString(),
                        Text = item.XR_INC_STATI.DESCRIZIONE
                    });
                }
                result.Add(group);
            }
            return result;
        }
        public static List<ListItem> getGruppi()
        {
            List<ListItem> lista = new List<ListItem>();

            Dictionary<string, string> gruppi = HrisHelper.GetParametroJson<Dictionary<string, string>>(HrisParam.IncentiviGruppi);
            lista.Add(new ListItem() { Value = "", Text = "" });
            if (gruppi != null)
            {
                foreach (var item in gruppi)
                {
                    lista.Add(new ListItem()
                    {
                        Value = item.Key,
                        Text = item.Value
                    });
                }
            }

            return lista;
        }
        public static List<ListItem> getRappRai(string sede)
        {
            IncentiviEntities db = new IncentiviEntities();
            List<ListItem> lista = new List<ListItem>();

            if (sede.Contains('-'))
                sede = sede.Remove(sede.IndexOf('-')).Trim();

            DateTime d = DateTime.Today;

            foreach (var rappr in db.XR_INC_RAPPRRAI.Where(x => (x.DATA_FINE_VALIDITA == null || x.DATA_FINE_VALIDITA > d) && x.SEDE != null && x.SEDE.ToUpper() == sede.ToUpper()))
            {
                ListItem item = new ListItem()
                {
                    Value = rappr.ID_RAPPRRAI.ToString(),
                    Text = rappr.ANAGPERS.DES_COGNOMEPERS.TitleCase() + " " + rappr.ANAGPERS.DES_NOMEPERS.TitleCase()
                };
                lista.Add(item);
            }


            foreach (var rappr in db.XR_INC_RAPPRRAI.Where(x => x.DATA_FINE_VALIDITA == null && x.SEDE == null))
            {
                ListItem item = new ListItem()
                {
                    Value = rappr.ID_RAPPRRAI.ToString(),
                    Text = rappr.ID_RAPPRRAI > 0 ? rappr.ANAGPERS.DES_COGNOMEPERS.TitleCase() + " " + rappr.ANAGPERS.DES_NOMEPERS.TitleCase() : "Inserimento manuale"
                };
                lista.Add(item);
            }

            if (lista.Count() == 1)
                lista[0].Selected = true;

            return lista;
        }
        public static List<ListItem> getSiglaSind(string sede, string qualifica)
        {
            IncentiviEntities db = new IncentiviEntities();
            bool isRome = sede.ToUpper().Contains("ROMA");

            DateTime d = DateTime.Today;
            List<ListItem> lista = new List<ListItem>();
            foreach (var sind in db.XR_INC_SIGLESINDACALI.Where(x => (x.DATA_FINE_VALIDITA == null || x.DATA_FINE_VALIDITA > d) && (isRome && x.SEDE == "Roma" || (!isRome && (x.SEDE == "Altro" || x.SEDE == sede)))))
            {
                if (String.IsNullOrWhiteSpace(sind.QUALIFICHE_INCLUSE) || sind.QUALIFICHE_INCLUSE.Split(',').Any(x => qualifica.StartsWith(x)))
                {
                    ListItem item = new ListItem()
                    {
                        Value = sind.ID_SIGLASIND.ToString(),
                        Text = sind.SINDACATO
                    };
                    lista.Add(item);
                }
            }

            return lista;
        }
        public static List<ListItem> getRapprSind(int? idSind, string sede)
        {
            IncentiviEntities db = new IncentiviEntities();
            List<ListItem> lista = new List<ListItem>();

            if (sede.Contains('-'))
                sede = sede.Remove(sede.IndexOf('-')).Trim();

            if (idSind != null)
            {
                DateTime d = DateTime.Today;
                foreach (var rappr in db.XR_INC_RAPPRSINDACATO.Where(x => (x.DATA_FINE_VALIDITA == null || x.DATA_FINE_VALIDITA > d) && x.ID_SIGLA == idSind && (x.SEDE == null || x.SEDE.ToUpper() == sede.ToUpper())))
                {
                    ListItem item = new ListItem()
                    {
                        Value = rappr.ID_RAPPRSINDACATO.ToString(),
                        Text = !String.IsNullOrWhiteSpace(rappr.COGNOME) ? rappr.COGNOME.TitleCase() + " " + rappr.NOME.TitleCase() : "Inserimento manuale"
                    };
                    lista.Add(item);
                }
            }

            return lista;
        }
        public ActionResult getAJAXRapprSind(int idSind, string sede)
        {
            IncentiviEntities db = new IncentiviEntities();

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { result = getRapprSind(idSind, sede) }
            };
        }
        public static List<ListItem> getRapprInd(string sede)
        {
            IncentiviEntities db = new IncentiviEntities();
            List<ListItem> lista = new List<ListItem>();

            if (sede.Contains('-'))
                sede = sede.Remove(sede.IndexOf('-')).Trim();

            DateTime d = DateTime.Today;

            foreach (var rappr in db.XR_INC_RAPPRINDUSTRIA.Where(x => (x.DATA_FINE_VALIDITA == null || x.DATA_FINE_VALIDITA > d) && x.SEDE != null && x.SEDE.ToUpper() == sede.ToUpper()))
            {
                ListItem item = new ListItem()
                {
                    Value = rappr.ID_RAPPRINDUSTRIA.ToString(),
                    Text = !String.IsNullOrWhiteSpace(rappr.COGNOME) ? rappr.COGNOME.TitleCase() + " " + rappr.NOME.TitleCase() : "Inserimento manuale"
                };
                lista.Add(item);
            }

            foreach (var rappr in db.XR_INC_RAPPRINDUSTRIA.Where(x => (x.DATA_FINE_VALIDITA == null || x.DATA_FINE_VALIDITA > d) && x.SEDE == null))
            {
                ListItem item = new ListItem()
                {
                    Value = rappr.ID_RAPPRINDUSTRIA.ToString(),
                    Text = !String.IsNullOrWhiteSpace(rappr.COGNOME) ? rappr.COGNOME.TitleCase() + " " + rappr.NOME.TitleCase() : "Inserimento manuale"
                };
                lista.Add(item);
            }

            if (lista.Count() == 1)
                lista[0].Selected = true;

            return lista;
        }
        public static List<ListItem> getScelteIban(XR_INC_DIPENDENTI dip)
        {
            List<ListItem> lista = new List<ListItem>();

            if (!String.IsNullOrWhiteSpace(dip.IND_PROPRIO_IBAN) && dip.IND_PROPRIO_IBAN == "B")
            {
                ListItem listItem = new ListItem()
                {
                    Value = "B",
                    Text = "VINCOLO BCCR",
                    Selected = true
                };
                lista.Add(listItem);
            }
            else
            {
                var cc = dip.ANAGPERS.XR_DATIBANCARI.OrderByDescending(y => y.DTA_FINE).FirstOrDefault(x => x.XR_UTILCONTO.Count() == 1 && x.XR_UTILCONTO.First().COD_UTILCONTO == "01");

                if (cc.IND_VINCOLATO == "Y")
                {
                    ListItem listItem = new ListItem()
                    {
                        Value = "V",
                        Text = "VINCOLATO",
                        Selected = true
                    };
                    lista.Add(listItem);
                }
                else if (cc.IND_CONGELATO == "Y")
                {
                    ListItem listItem = new ListItem()
                    {
                        Value = "C",
                        Text = "CONGELATO",
                        Selected = true
                    };
                    lista.Add(listItem);
                }
                else
                {
                    ListItem listItem = new ListItem()
                    {
                        Value = "Y",
                        Text = "Accredito cedolino"
                    };
                    lista.Add(listItem);
                    listItem = new ListItem()
                    {
                        Value = "N",
                        Text = "Altro conto corrente"
                    };
                    lista.Add(listItem);
                }
            }

            return lista;
        }
        public static List<ListItem> getInCaricoList()
        {
            List<ListItem> lista = new List<ListItem>();

            ListItem listItem = new ListItem()
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
        public static List<ListItem> getDataCessazione()
        {
            Expression<Func<XR_INC_DIPENDENTI, bool>> funcFilterAbilSede = CessazioneHelper.FuncFilterAbilSede();
            List<ListItem> lista = new List<ListItem>();

            ListItem itemDataCess = new ListItem();
            using (IncentiviEntities db = new IncentiviEntities())
            {
                foreach (var item in db.XR_INC_DIPENDENTI.Where(funcFilterAbilSede).Select(x => x.DATA_CESSAZIONE).Distinct())
                {
                    itemDataCess = new ListItem()
                    {
                        Value = item.Value.ToString("yyyyMMdd"),
                        Text = item.Value.ToString("dd/MM/yyyy")
                    };
                    lista.Add(itemDataCess);
                }
            }

            return lista;
        }
        public static List<ListItem> getSedi()
        {
            Expression<Func<XR_INC_DIPENDENTI, bool>> funcFilterAbilSede = CessazioneHelper.FuncFilterAbilSede();
            List<ListItem> lista = new List<ListItem>();

            ListItem itemSede = new ListItem();
            using (IncentiviEntities db = new IncentiviEntities())
            {
                foreach (var item in db.XR_INC_DIPENDENTI.Where(funcFilterAbilSede).Select(x => x.SEDE).Distinct())
                {
                    itemSede = new ListItem()
                    {
                        Value = item,
                        Text = item.TitleCase()
                    };
                    lista.Add(itemSede);
                }
            }

            return lista;
        }
        public static List<ListItem> getCause()
        {
            List<ListItem> lista = new List<ListItem>();

            ListItem listItem = new ListItem()
            {
                Value = "0",
                Text = "No"
            };
            lista.Add(listItem);
            listItem = new ListItem()
            {
                Value = "1",
                Text = "Sì"
            };
            lista.Add(listItem);

            return lista;
        }
        public static List<SelectListItem> GetDateMaturazioneRequisiti()
        {
            var db = new IncentiviEntities();
            return db.XR_INC_DATE_REQUISITI.ToList().Select(x => new SelectListItem { Value = x.COD_REQUISITO, Text = x.DES_REQUISITO }).ToList();
        }
        #endregion

        #region GestioneBozzeMail    
        [HttpPost]
        public ActionResult Modal_BozzaMail(int idDip, string tipo)
        {
            CessazioneBozza bozza = InternalGetDipBozza(idDip, tipo);
            return View("subpartial/Modal_BozzaMail", bozza);
        }

        private static CessazioneBozza InternalGetDipBozza(int idDip, string tipo)
        {
            CessazioneBozza bozza = new CessazioneBozza();
            bozza.TipologiaBozza = tipo;
            var db = new IncentiviEntities();
            var dip = db.XR_INC_DIPENDENTI.Find(idDip);

            if (tipo == "proposta")
            {
                string filtroTemplate = "";
                if (dip.NOT_TIP_ACCERT == "Quota100" || dip.NOT_TIP_ACCERT == "Quota102")
                {
                    if (dip.NOT_TIP_SCELTA == "Quota100" || dip.NOT_TIP_SCELTA == "Quota102")
                        filtroTemplate = "BozzaA";
                    else
                        filtroTemplate = "BozzaC";
                }
                else
                {
                    if (dip.SINTESI1.COD_QUALIFICA.StartsWith("M") || dip.SINTESI1.COD_QUALIFICA.StartsWith("A7"))
                    {
                        if (dip.NUM_MENS_AGG_DEC.GetValueOrDefault() > 0)
                            filtroTemplate = "BozzaB_Agg";
                        else
                            filtroTemplate = "BozzaB_NoAgg";
                    }
                    else
                    {
                        if ((dip.NOT_TIP_SCELTA == "Quota100" || dip.NOT_TIP_SCELTA == "Quota102") && dip.IND_INVALIDITA.GetValueOrDefault() == 1)
                            filtroTemplate = "BozzaD";
                        else
                            filtroTemplate = "BozzaB";
                    }
                }

                var template = CessazioneHelper.GetTemplate(db, "Mail", idDip, filtroTemplate, true);
                if (template.ID_DIPENDENTE.HasValue)
                    bozza.HtmlTextLastMod = String.Format("Ultima modifica di {0} il {1:dd/MM/yyyy} alle {1:HH:mm}", CezanneHelper.GetNominativoByMatricola(template.COD_USER), template.TMS_TIMESTAMP);

                //Richiamo la funzione per prendere la descrizione del template standard
                var templSt = CessazioneHelper.GetTemplate(db, "Mail", idDip, filtroTemplate, false);
                if (templSt != null)
                    bozza.TemplateBozza = templSt.DES_TEMPLATE;

                bozza.IdDipendente = idDip;
                bozza.TipoVertenze = dip.IND_TIPO_VERTENZE;
                bozza.Codice = template.NME_TEMPLATE;
                bozza.HtmlText = template.TEMPLATE_TEXT;

                string[] paramsToken = HrisHelper.GetParametri<string>(HrisParam.IncentiviMensilitaAggiuntive);
                string token = dip.NUM_MENS_AGG_DEC.GetValueOrDefault() == 0 ? paramsToken[1] : paramsToken[0];

                string[] paramsStraToken = HrisHelper.GetParametri<string>(HrisParam.IncentiviTokenStra);
                string tokenStra = !String.IsNullOrWhiteSpace(dip.CAUSE_VERTENZE) ? paramsStraToken[0] : "";

                string dataFnl930 = HrisHelper.GetParametro<string>(HrisParam.IncentiviEsecuzioneFNL930);
                DateTime dataExec;
                DateTime.TryParseExact(dataFnl930, "dd/MM/yyyy HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out dataExec);

                bozza.HtmlText = bozza.HtmlText
                                    .Replace("__DATA_CESSAZIONE__", dip.DATA_CESSAZIONE.Value.ToString("dd/MM/yyyy"))
                                    .Replace("__INCENTIVO_NUM__", dip.INCENTIVO_LORDO_IP.GetValueOrDefault().ToString("N2"))
                                    .Replace("__INCENTIVO_LETT__", dip.INCENTIVO_LORDO_IP.GetValueOrDefault().AmountToReadableString())
                                    .Replace("__UNA_TANTUM_NUM__", dip.UNA_TANTUM_LORDA_IP.GetValueOrDefault().ToString("N2"))
                                    .Replace("__UNA_TANTUM_LETT__", dip.UNA_TANTUM_LORDA_IP.GetValueOrDefault().AmountToReadableString())
                                    .Replace("__TFR_LORDO_NUM__", dip.TFR_LORDO_AZ_IP.GetValueOrDefault().ToString())
                                    .Replace("__TFR_LORDO_LETT__", dip.TFR_LORDO_AZ_IP.GetValueOrDefault().AmountToReadableString())
                                    .Replace("__TFR_LORDO_AZ__", dip.TFR_LORDO_AZ_IP.GetValueOrDefault().ToString("N2"))
                                    .Replace("__TFR_NETTO__", dip.TFR_NETTO.GetValueOrDefault().ToString("N2"))
                                    .Replace("__ALIQ_TFR__", dip.ALIQ_TFR.GetValueOrDefault().ToString("N2"))
                                    .Replace("__NUM_MENS_PRINC__", dip.NUM_MENS_PRINC_DEC.GetValueOrDefault().ToString())
                                    .Replace("__TOKEN_MENS_AGG__", token)
                                    .Replace("__NUM_MENS_AGG__", dip.NUM_MENS_AGG_DEC.GetValueOrDefault().ToString())
                                    .Replace("__TOKEN_STRA__", tokenStra)
                                    .Replace("__DATA_AGG_TFR__", dip.DATA_TFR.HasValue ? dip.DATA_TFR.Value.ToString("dd/MM/yyyy") : dataExec.ToString("dd/MM/yyyy"));

                var templateAll = CessazioneHelper.GetTemplate(db, "PropostaPDF", idDip, "", true);
                if (templateAll != null)
                {
                    bozza.HasPDFTemplate = true;
                    if (templateAll.COD_USER != "ADMIN")
                        bozza.TemplateLastMod = String.Format("Ultima modifica di {0} il {1:dd/MM/yyyy} alle {1:HH:mm}", CezanneHelper.GetNominativoByMatricola(templateAll.COD_USER), templateAll.TMS_TIMESTAMP);
                    else
                        bozza.TemplateLastMod = String.Format("Proposta generata automaticamente il {0:dd/MM/yyyy} alle {0:HH:mm}", templateAll.TMS_TIMESTAMP);
                }
                else
                {
                    bozza.HasPDFTemplate = false;
                }

                if (dip.DATA_INVIO_PROP.HasValue)
                {
                    bozza.IsViewMode = true;
                    bozza.InfoInvio = dip.NOT_INVIO_PROP;
                }

                bozza.IndirizziCC = "";
                //var extraCC = db.XR_INC_PARAM_MAIL.Where(x => x.COD_CAMPO == "CC" && x.COD_SERVIZIO.Contains(dip.SINTESI1.COD_SERVIZIO)).Select(x => x.VALORE);
                //if (extraCC != null && extraCC.Any())
                //    bozza.IndirizziCC = String.Join(";", extraCC);

                var tmpExtraCC = db.XR_INC_PARAM_MAIL.Where(x => x.COD_CAMPO == "CC").ToList();

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
                            bozza.IndirizziCC += (!String.IsNullOrWhiteSpace(bozza.IndirizziCC) ? ";" : "") + item;
                        }
                    }
                }

                bozza.AbilitaInvio = true;
                bozza.AbilitaGestione = true;
            }
            else
            {
                var filtroTemplate = "BozzaBase";

                var template = CessazioneHelper.GetTemplate(db, "MailVerbale", idDip, filtroTemplate, true);
                if (template.ID_DIPENDENTE.HasValue)
                    bozza.HtmlTextLastMod = String.Format("Ultima modifica di {0} il {1:dd/MM/yyyy} alle {1:HH:MM}", CezanneHelper.GetNominativoByMatricola(template.COD_USER), template.TMS_TIMESTAMP);

                //Richiamo la funzione per prendere la descrizione del template standard
                var templSt = CessazioneHelper.GetTemplate(db, "MailVerbale", idDip, filtroTemplate, false);
                if (templSt != null)
                    bozza.TemplateBozza = templSt.DES_TEMPLATE;

                bozza.IdDipendente = idDip;
                bozza.Codice = template.NME_TEMPLATE;
                bozza.HtmlText = template.TEMPLATE_TEXT;

                string tokenIban = "";
                if (dip.IND_PROPRIO_IBAN != "B")
                    tokenIban = HrisHelper.GetParametro<string>(HrisParam.IncentiviMailVerbaleTokenIban);

                string tokenQIO = "";
                var paramTokenQIO = HrisHelper.GetParametro(HrisParam.IncentiviMailVerbaleTokenQIO);
                var qualTokenQIOExcl = paramTokenQIO.COD_VALUE2.Split(',');
                //if (!dip.SINTESI1.COD_QUALIFICA.StartsWith("M7") && !dip.SINTESI1.COD_QUALIFICA.StartsWith("A7") && !dip.SINTESI1.COD_QUALIFICA.StartsWith("A01"))
                if (dip.ID_SIGLASIND == null && !qualTokenQIOExcl.Any(x => dip.SINTESI1.COD_QUALIFICA.StartsWith(x)))
                {
                    if (!String.IsNullOrWhiteSpace(tokenIban))
                        tokenQIO = paramTokenQIO.COD_VALUE1;
                    else
                        tokenQIO = paramTokenQIO.COD_VALUE3;
                }

                string tokenExtraSind = "";
                using (var dbTal = new myRaiDataTalentia.TalentiaEntities())
                {
                    var extraSind = db.XR_HRIS_PARAM.FirstOrDefault(x => x.COD_PARAM == "IncentiviExtraSind" && x.COD_VALUE1 == dip.SEDE.ToUpper().Trim());
                    if (extraSind != null)
                        tokenExtraSind = ", " + extraSind.COD_VALUE2;
                }

                string tokenITL = "";
                //if (dip.IND_ITL.GetValueOrDefault())
                tokenITL = HrisHelper.GetParametro<string>(HrisParam.IncentiviMailVerbaleTokenItl);

                string tokenRoma = "";
                if (dip.SEDE.ToUpper().Contains("ROMA"))
                    tokenRoma = HrisHelper.GetParametro<string>(HrisParam.IncentiviMailVerbaleTokenRoma);

                string dataFnl930 = HrisHelper.GetParametro<string>(HrisParam.IncentiviEsecuzioneFNL930);
                DateTime dataExec;
                DateTime.TryParseExact(dataFnl930, "dd/MM/yyyy HH:mm:ss", null, System.Globalization.DateTimeStyles.None, out dataExec);

                bozza.HtmlText = bozza.HtmlText
                                    .Replace("__NOMINATIVO__", dip.SINTESI1.DES_NOMEPERS.TitleCase() + " " + dip.SINTESI1.DES_COGNOMEPERS.TitleCase())
                                    .Replace("__TOKEN_IBAN__", tokenIban)
                                    .Replace("__TOKEN_QIO__", tokenQIO)
                                    .Replace("__EXTRA_SIND__", tokenExtraSind)
                                    .Replace("__TOKEN_ITL__", tokenITL)
                                    .Replace("__TOKEN_ROMA__", tokenRoma);

                if (dip.NUM_BOZZA_GIORNI.HasValue)
                    bozza.HtmlText = bozza.HtmlText.Replace("__NUM_GIORNI_LIMITE__", dip.NUM_BOZZA_GIORNI.Value.ToString());

                if (dip.DATA_APPUNTAMENTO.HasValue)
                {
                    bozza.HtmlText = bozza.HtmlText
                                        .Replace("__GIORNO_APPUNTAMENTO__", dip.DATA_APPUNTAMENTO.Value.ToString("dd/MM/yyyy"))
                                        .Replace("__ORA_APPUNTAMENTO__", dip.DATA_APPUNTAMENTO.Value.ToString("HH:mm"));
                }

                if (!String.IsNullOrWhiteSpace(dip.NOT_LUOGO_APPUNTAMENTO))
                    bozza.HtmlText = bozza.HtmlText
                                        .Replace("__LUOGO_APPUNTAMENTO__", dip.NOT_LUOGO_APPUNTAMENTO);

                var templateAll = CessazioneHelper.GetTemplate(db, "VerbalePDF", idDip, "", true);
                if (templateAll != null)
                {
                    bozza.HasPDFTemplate = true;
                    if (templateAll.COD_USER != "ADMIN")
                        bozza.TemplateLastMod = String.Format("Ultima modifica di {0} il {1:dd/MM/yyyy} alle {1:HH:mm}", CezanneHelper.GetNominativoByMatricola(templateAll.COD_USER), templateAll.TMS_TIMESTAMP);
                    else
                        bozza.TemplateLastMod = String.Format("Proposta generata automaticamente il {0:dd/MM/yyyy} alle {0:HH:mm}", templateAll.TMS_TIMESTAMP);
                }
                else
                {
                    bozza.HasPDFTemplate = false;
                }

                if (dip.DATA_BOZZA_INVIO.HasValue)
                {
                    bozza.IsViewMode = true;
                    bozza.InfoInvio = dip.NOT_BOZZA_INVIO;
                }

                bozza.IndirizziCC = "";
                //var extraCC = db.XR_INC_PARAM_MAIL.Where(x => x.COD_CAMPO == "CC Verbale" && x.COD_SERVIZIO.Contains(dip.SINTESI1.COD_SERVIZIO)).Select(x => x.VALORE);
                //if (extraCC != null && extraCC.Any())
                //    bozza.IndirizziCC = String.Join(";", extraCC);

                var tmpExtraCC = db.XR_INC_PARAM_MAIL.Where(x => x.COD_CAMPO == "CC Verbale").ToList();
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
                            bozza.IndirizziCC += (!String.IsNullOrWhiteSpace(bozza.IndirizziCC) ? ";" : "") + item;
                        }
                    }
                }

                bozza.HasCronologia = db.XR_INC_TEMPLATE.Any(x => x.COD_TIPO == "VerbaleDOC" && x.ID_DIPENDENTE == idDip);

                if (dip.SEDE.ToUpper() == "ROMA")
                {
                    bozza.AbilitaInvio = true;
                    bozza.AbilitaGestione = true;
                }
                else
                {
                    bozza.AbilitaInvio = dip.IND_SBLOCCA_PRATICA.GetValueOrDefault() == 1;
                    string matricola = CommonHelper.GetCurrentUserMatricola();
                    if (CessazioneHelper.EnabledToAnySubFunc(matricola, "ADM", "GEST"))
                        bozza.AbilitaGestione = true;
                    else
                        bozza.AbilitaGestione = AuthHelper.EnabledToSubFunc(matricola, CessazioneHelper.INCENTIVI_INC_EXTRA, "CONTENZIOSO");
                }
            }

            return bozza;
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Save_BozzaMail(CessazioneBozza bozza, bool _invioMail, bool _includiProposta = false, int _sbloccaPratica = 0)
        {
            List<XR_HRIS_PARAM> listParam = HrisHelper.GetParametriJson<XR_HRIS_PARAM>(HrisParam.IncentiviParametri);
            var db = new IncentiviEntities();
            var dip = db.XR_INC_DIPENDENTI.Find(bozza.IdDipendente);

            string codTipo = "";
            if (bozza.TipologiaBozza == "proposta")
                codTipo = "Mail";
            else
                codTipo = "MailVerbale";


            var old = db.XR_INC_TEMPLATE.Where(x => x.VALID_DTA_INI < DateTime.Now && (x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now)).FirstOrDefault(x => x.COD_TIPO == codTipo && x.ID_DIPENDENTE == bozza.IdDipendente);
            if (old != null && old.TEMPLATE_TEXT != bozza.HtmlText)
                old.VALID_DTA_END = DateTime.Now;

            if (old == null || old.TEMPLATE_TEXT != bozza.HtmlText)
            {
                XR_INC_TEMPLATE template = new XR_INC_TEMPLATE()
                {
                    NME_TEMPLATE = bozza.Codice,
                    ID_DIPENDENTE = bozza.IdDipendente,
                    TEMPLATE_TEXT = bozza.HtmlText,
                    COD_TIPO = codTipo
                };
                CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime timestamp);
                template.VALID_DTA_INI = timestamp;
                template.COD_USER = codUser;
                template.COD_TERMID = codTermid;
                template.TMS_TIMESTAMP = timestamp;

                db.XR_INC_TEMPLATE.Add(template);
            }

            if (bozza.TipologiaBozza == "verbale")
                dip.IND_SBLOCCA_PRATICA = _sbloccaPratica;

            var result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
            if (result)
            {
                if (_invioMail)
                {
                    GestoreMail mail = new GestoreMail();
                    List<Attachement> attach = new List<Attachement>();

                    string tipoDoc = "";
                    string nomeAllegato = "";
                    HrisParam paramAbilita = HrisParam.IncentiviAbilitaMail;
                    string codCampoCC = "";
                    string destinatario = "";
                    if (bozza.TipologiaBozza == "proposta")
                    {
                        tipoDoc = "PropostaPDF";
                        nomeAllegato = "Proposta";
                        paramAbilita = HrisParam.IncentiviAbilitaMail;
                        codCampoCC = "CC";
                        destinatario = CessazioneHelper.GetMailDip(dip); //CommonHelper.GetEmailPerMatricola(dip.MATRICOLA);
                    }
                    else
                    {
                        tipoDoc = "VerbalePDF";
                        nomeAllegato = "Bozza verbale";
                        paramAbilita = HrisParam.IncentiviAbilitaMailVerbale;
                        codCampoCC = "CC Verbale";
                        destinatario = CessazioneHelper.GetMailDip(dip); //dip.MAIL;
                    }

                    if (bozza.TipologiaBozza == "proposta")
                    {
                        if (_includiProposta)
                        {
                            var template = CessazioneHelper.GetTemplate(db, tipoDoc, dip.ID_DIPENDENTE, "", true);
                            if (template != null)
                            {
                                attach.Add(new Attachement()
                                {
                                    AttachementName = nomeAllegato + " " + dip.SINTESI1.DES_COGNOMEPERS.TitleCase() + ".pdf",
                                    AttachementValue = template.TEMPLATE,
                                    AttachementType = "application/pdf"
                                });
                            }
                        }

                        var listAll = CessazioneHelper.GetListaAllegati(db, "MailAllegatiExtra", dip.ID_DIPENDENTE, dip.SINTESI1.DES_CITTASEDE, dip.SINTESI1.COD_QUALIFICA);
                        if (listAll != null && listAll.Any())
                        {
                            attach.AddRange(listAll.Select(x => new Attachement()
                            {
                                AttachementName = x.NME_TEMPLATE,
                                AttachementValue = x.TEMPLATE,
                                AttachementType = x.CONTENT_TYPE
                            }));
                        }
                    }
                    else
                    {
                        var template = CessazioneHelper.GetTemplate(db, tipoDoc, dip.ID_DIPENDENTE, "", true);
                        if (template != null)
                        {
                            attach.Add(new Attachement()
                            {
                                AttachementName = nomeAllegato + " " + dip.SINTESI1.DES_COGNOMEPERS.TitleCase() + ".pdf",
                                AttachementValue = template.TEMPLATE,
                                AttachementType = "application/pdf"
                            });
                        }

                        var ms = InternalProspetto(db, dip, 0, false);
                        attach.Add(new Attachement()
                        {
                            AttachementName = "Prospetto " + dip.SINTESI1.Nominativo().ToUpper() + ".pdf",
                            AttachementValue = ms.ToArray(),
                            AttachementType = "application/pdf"
                        });


                        var listAll = CessazioneHelper.GetListaAllegati(db, "MailVerbaleAllegatiExtra", dip.ID_DIPENDENTE, dip.SINTESI1.DES_CITTASEDE, dip.SINTESI1.COD_QUALIFICA);
                        if (listAll != null && listAll.Any())
                        {
                            attach.AddRange(listAll.Select(x => new Attachement()
                            {
                                AttachementName = x.NME_TEMPLATE,
                                AttachementValue = x.TEMPLATE,
                                AttachementType = x.CONTENT_TYPE
                            }));
                        }
                    }

                    string currentMatr = CommonHelper.GetCurrentUserMatricola();

                    myRaiData.Incentivi.XR_HRIS_PARAM parametro = HrisHelper.GetParametro(paramAbilita);
                    string mittente = "";
                    if (bozza.TipologiaBozza == "verbale")
                    {

                        var mailParam = db.XR_INC_PARAM_MAIL.FirstOrDefault(x => x.COD_CAMPO == "FROM_VERBALE" && x.COD_MATRICOLA.Contains(currentMatr));
                        if (mailParam != null)
                            mittente = mailParam.VALORE;
                        else
                        {
                            var dbTal = new myRaiDataTalentia.TalentiaEntities();
                            var paramRifContenzioso = db.XR_HRIS_PARAM.FirstOrDefault(x => x.COD_PARAM == "IncentiviRifContenzioso");
                            if (paramRifContenzioso != null && !String.IsNullOrWhiteSpace(paramRifContenzioso.COD_VALUE1))
                            {
                                AbilSubFunc abilSubFunc = null;
                                var filtroContenzioso = AuthHelper.EnabledToSubFunc(paramRifContenzioso.COD_VALUE1, CessazioneHelper.INCENTIVI_INC_EXTRA, "VERBALI", out abilSubFunc)
                                            || AuthHelper.EnabledToSubFunc(paramRifContenzioso.COD_VALUE1, CessazioneHelper.INCENTIVI_INC_EXTRA, "CONTENZIOSO", out abilSubFunc);

                                if (filtroContenzioso)
                                {
                                    var abilCat = AuthHelper.EnabledCategory(paramRifContenzioso.COD_VALUE1, CessazioneHelper.INCENTIVI_INC_EXTRA, abilSubFunc.Nome);
                                    //if (!String.IsNullOrWhiteSpace(dip.CAUSE_VERTENZE) || (!String.IsNullOrWhiteSpace(abilSubFunc.CategorieAbilitate..CAT_INCLUSE) && filtroContenzioso.CAT_INCLUSE.Split(',').Any(x => dip.SINTESI1.COD_QUALIFICA.StartsWith(x))))
                                    //if (!String.IsNullOrWhiteSpace(dip.CAUSE_VERTENZE) || (abilCat.HasFilter && abilCat.CategorieIncluse.Any(x => dip.SINTESI1.COD_QUALIFICA.StartsWith(x))))
                                    if (CessazioneHelper.IsConteziosoDip(dip) || (abilCat.HasFilter && abilCat.CategorieIncluse.Any(x => dip.SINTESI1.COD_QUALIFICA.StartsWith(x))))
                                    {
                                        var mailCont = db.XR_INC_PARAM_MAIL.FirstOrDefault(x => x.COD_CAMPO == "FROM_VERBALE" && x.COD_GRUPPO == abilSubFunc.Nome);
                                        if (mailCont != null)
                                            mittente = mailCont.VALORE;
                                    }
                                }
                            }

                        }
                    }

                    if (String.IsNullOrWhiteSpace(mittente))
                        mittente = parametro.COD_VALUE2;

                    string cc = mittente;
                    var tmpExtraCC = db.XR_INC_PARAM_MAIL.Where(x => x.COD_CAMPO == codCampoCC).ToList();

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
                                cc += ";" + item;
                            }
                        }
                    }

                    string oggetto = parametro.COD_VALUE3;
                    oggetto = oggetto.Replace("__COGNOME__", dip.SINTESI1.DES_COGNOMEPERS.TitleCase())
                                     .Replace("__NOME__", dip.SINTESI1.DES_NOMEPERS.TitleCase());

                    if (bozza.TipologiaBozza == "verbale")
                        cc = CommonHelper.GetEmailPerMatricola(currentMatr) + (!String.IsNullOrWhiteSpace(cc) ? ";" + cc : "");

                    var response = mail.InvioMail(bozza.HtmlText, oggetto, destinatario, cc, mittente, attach);
                    if (response != null && response.Errore != null)
                    {
                        return Content("Errore durante l'invio della mail");
                    }

                    if (bozza.TipologiaBozza == "proposta")
                    {
                        dip.DATA_INVIO_PROP = DateTime.Today;
                        dip.NOT_INVIO_PROP = String.Format("Mail inviata da {0} il {1:dd/MM/yyyy}", CezanneHelper.GetNominativoByMatricola(CommonHelper.GetCurrentUserMatricola()), dip.DATA_INVIO_PROP.Value);
                        if (!_includiProposta)
                            dip.NOT_INVIO_PROP += " - proposta non allegata";


                        var param = listParam.FirstOrDefault(x => x.COD_PARAM == "LimiteProposta");
                        dip.SetField("LimiteProposta", dip.DATA_INVIO_PROP.Value.Date.AddDays(Convert.ToDouble(param.COD_VALUE1)));
                    }
                    else
                    {
                        dip.DATA_BOZZA_INVIO = DateTime.Today;
                        dip.NOT_BOZZA_INVIO = String.Format("Mail inviata da {0} il {1:dd/MM/yyyy}", CezanneHelper.GetNominativoByMatricola(CommonHelper.GetCurrentUserMatricola()), dip.DATA_BOZZA_INVIO.Value);
                    }

                    if (!DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                    {
                        return Content("L'invio della mail è andato a buon fine, tuttavia non è stata aggiornato il dato di invio");
                    }

                }
                return Content("OK");
            }
            else
                return Content("Errore durante il salvataggio");
        }


        public ActionResult UploadBozzaMail(HttpPostedFileBase _file, int idDip, string tipo, string descrizione = "")
        {
            var db = new IncentiviEntities();

            string tipoDoc = "";
            string nomeAllegato = "";
            if (tipo == "proposta")
            {
                tipoDoc = "PropostaPDF";
                nomeAllegato = "Proposta";
            }
            else
            {
                tipoDoc = "VerbaleDOC";
                nomeAllegato = "Bozza verbale";
            }

            var old = db.XR_INC_TEMPLATE.Where(x => x.VALID_DTA_INI < DateTime.Now && (x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now)).FirstOrDefault(x => x.COD_TIPO == tipoDoc && x.ID_DIPENDENTE == idDip);
            if (old != null)
                old.VALID_DTA_END = DateTime.Now;

            if (tipo == "verbale")
            {
                var oldPDF = db.XR_INC_TEMPLATE.Where(x => x.VALID_DTA_INI < DateTime.Now && (x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now)).FirstOrDefault(x => x.COD_TIPO == "VerbalePDF" && x.ID_DIPENDENTE == idDip);
                if (oldPDF != null)
                    db.XR_INC_TEMPLATE.Remove(oldPDF);
            }

            XR_INC_TEMPLATE template = new XR_INC_TEMPLATE()
            {
                NME_TEMPLATE = nomeAllegato,
                ID_DIPENDENTE = idDip,
                COD_TIPO = tipoDoc,
                DES_TEMPLATE = descrizione
            };
            using (MemoryStream ms = new MemoryStream())
            {
                _file.InputStream.CopyTo(ms);
                template.TEMPLATE = ms.ToArray();
            }
            CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime timestamp);
            template.VALID_DTA_INI = timestamp;
            template.COD_USER = codUser;
            template.COD_TERMID = codTermid;
            template.TMS_TIMESTAMP = timestamp;

            db.XR_INC_TEMPLATE.Add(template);



            if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
            {
                if (tipo == "verbale")
                {
                    var dip = db.XR_INC_DIPENDENTI.Find(idDip);
                    TaskHelper.AddBatchRunnerTask("IncentiviVerbaleOnDemand", out var taskErr, arguments: dip.MATRICOLA, note: String.Format("Matricola {0} - IdDip: {1}", dip.MATRICOLA, dip.ID_DIPENDENTE));
                }
                return Content("OK");
            }
            else
                return Content("Errore durante il salvataggio");
        }

        public ActionResult GetBozzaMail(int idDip, string tipo)
        {
            //Prende la proposta in formato Word, per poi poterla modificare
            var db = new IncentiviEntities();
            var dip = db.XR_INC_DIPENDENTI.Find(idDip);

            if (tipo == "proposta")
            {
                if (CessazioneHelper.GetPropostaDocMail(db, dip, out MemoryStream st, out string nomeFile))
                    return new FileStreamResult(st, "application/vnd.openxmlformats-officedocument.wordprocessingml.document") { FileDownloadName = nomeFile };
                else
                    return View("~/Views/Shared/404.cshtml");
            }
            else
            {
                CessazioneHelper.CreaBozzaVerbale(idDip, true, out MemoryStream st, out string nomeFile, true);
                return new FileStreamResult(st, "application/vnd.openxmlformats-officedocument.wordprocessingml.document") { FileDownloadName = nomeFile };
            }
        }

        public ActionResult GetDocTemplate(int idTemplate)
        {
            var db = new IncentiviEntities();
            var template = db.XR_INC_TEMPLATE.Find(idTemplate);
            var dip = db.XR_INC_DIPENDENTI.Find(template.ID_DIPENDENTE);
            string nomeFile = "Verbale " + dip.SINTESI1.DES_COGNOMEPERS.TitleCase() + " " + dip.SINTESI1.DES_NOMEPERS.TitleCase() + " - " + template.VALID_DTA_INI.ToString("yyyyMMdd_HHmm") + ".docx";
            return File(template.TEMPLATE, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", nomeFile);
        }

        public ActionResult GetPropostaMail(int idDip, string tipo)
        {
            //Prende la proposta in PDF, pronta per essere inviata
            var db = new IncentiviEntities();
            string tipoDoc = tipo + "PDF";
            string titolo = "Modulo";
            if (tipo == "verbale")
                titolo = "Bozza verbale";

            var template = CessazioneHelper.GetTemplate(db, tipoDoc, idDip, "", true);
            if (template != null)
            {
                var dip = db.XR_INC_DIPENDENTI.Find(idDip);
                string nomeFile = titolo + " " + dip.SINTESI1.Nominativo().TitleCase() + ".pdf";
                MemoryStream st = new MemoryStream(template.TEMPLATE);
                return new FileStreamResult(st, "application/pdf") { FileDownloadName = nomeFile };
            }
            else
                return View("~/Views/Shared/404.cshtml");
        }

        [HttpPost]
        public ActionResult RestoreDefaultTemplate(int idDip, string tipo)
        {
            var db = new IncentiviEntities();
            string codTipo = "";
            if (tipo == "proposta")
                codTipo = "Mail";
            else
                codTipo = "MailVerbale";

            var old = db.XR_INC_TEMPLATE.Where(x => x.VALID_DTA_INI < DateTime.Now && (x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now)).FirstOrDefault(x => x.COD_TIPO == codTipo && x.ID_DIPENDENTE == idDip);
            if (old != null)
            {
                old.VALID_DTA_END = DateTime.Now;
            }

            if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                return Content("OK");
            else
                return Content("Errore durante il salvataggio");
        }
        #endregion

        #region Esportazioni
        public ActionResult Export_Costi()
        {
            string queryBudget = HrisHelper.GetParametro<string>(HrisParam.IncentiviQueryBudget);

            CessazioneCosti model = new CessazioneCosti();
            model.MaxValue = HrisHelper.GetParametro<decimal>(HrisParam.IncentiviCostoMax);

            var db = new IncentiviEntities();
            model.CurrentValue = db.Database.SqlQuery<decimal>(queryBudget).FirstOrDefault();

            bool enabledSaving = false;
            string[] abilSaving = HrisHelper.GetParametri<string>(HrisParam.IncentiviFilterAbilSaving);
            if (abilSaving == null || abilSaving[0] == "FALSE" || abilSaving[1].Contains(CommonHelper.GetCurrentUserMatricola()))
            {
                enabledSaving = true;
                var querySavingParam = myRaiHelper.HrisHelper.GetParametri<string>(myRaiHelper.HrisParam.IncentiviQuerySaving);
                string querySaving = "";
                if (querySavingParam != null && querySavingParam.Any())
                {
                    querySaving = querySavingParam[0].Replace("__MIN_YEAR__", querySavingParam[1]).Replace("__MAX_YEAR__", querySavingParam[2]);
                    try
                    {
                        model.Saving = db.Database.SqlQuery<CessazioneSaving>(querySaving).ToList();
                    }
                    catch (Exception)
                    {

                    }
                }
            }

            var strUltimoAgg = "Basato su calcoli provvisori";
            var ultimoAgg = myRaiHelper.HrisHelper.GetParametro<string>(myRaiHelper.HrisParam.IncentiviEsecuzioneFNL930);
            if (!String.IsNullOrWhiteSpace(ultimoAgg))
            {
                strUltimoAgg += " aggiornati al " + ultimoAgg;
            }

            string codGruppo = HrisHelper.GetParametro<string>(HrisParam.IncentiviRifGruppo);

            #region Query
            var fAnn = CessazioneHelper.IsCurrentState((int)IncStato.RichiestaAnnullata);
            var fDec = CessazioneHelper.IsCurrentState((int)IncStato.RichiestaDecaduta);
            var fRif = CessazioneHelper.IsCurrentState((int)IncStato.RichiestaRifiutata);
            var numAnn = db.XR_INC_DIPENDENTI.Where(x => x.COD_GRUPPO == codGruppo).Where(fAnn).Count();
            var numDec = db.XR_INC_DIPENDENTI.Where(x => x.COD_GRUPPO == codGruppo).Where(fDec).Count();
            var numRif = db.XR_INC_DIPENDENTI.Where(x => x.COD_GRUPPO == codGruppo).Where(fRif).Count();

            var fIns = CessazioneHelper.IsCurrentState((int)IncStato.RichiestaInserita);
            var fVer = CessazioneHelper.IsCurrentState((int)IncStato.RichiestaAccettata);
            var fAvv = CessazioneHelper.IsAnyState((int)IncStato.RecessoEffettuato);
            var listIns = db.XR_INC_DIPENDENTI
                            .Where(x => x.COD_GRUPPO == codGruppo)
                            .Where(fIns)
                            .Join(db.XR_INC_DIP_IPOTESI.Where(x => x.IS_VALID && x.COD_IPOTESI == "Q100"), dip => dip.ID_DIPENDENTE, ip => ip.ID_DIPENDENTE, (dip, ip) => new { dip, ip })
                            .Select(x => new
                            {
                                x.dip.MATRICOLA,
                                x.dip.PROGR_RICHIESTA,
                                x.dip.INCENTIVO_LORDO_IP,
                                x.dip.UNA_TANTUM_LORDA_IP,
                                x.dip.DATA_INVIO_PROP,
                                x.dip.DATA_FIRMA_DIP,
                                x.dip.DTA_RECESSO,
                                x.dip.SINTESI1.COD_QUALIFICA,
                                Stato = (int)IncStato.RichiestaInserita,
                                Q100_INCENTIVO_LORDO_IP = x.ip.INCENTIVO_LORDO_IP,
                                Q100_UNA_TANTUM_LORDA_IP = x.ip.UNA_TANTUM_LORDA_IP
                            }).ToList();

            var listVer = db.XR_INC_DIPENDENTI
                            .Where(x => x.COD_GRUPPO == codGruppo)
                            .Where(fVer)
                            .Join(db.XR_INC_DIP_IPOTESI.Where(x => x.IS_VALID && x.COD_IPOTESI == "Q100"), dip => dip.ID_DIPENDENTE, ip => ip.ID_DIPENDENTE, (dip, ip) => new { dip, ip })
                            .Select(x => new
                            {
                                x.dip.MATRICOLA,
                                x.dip.PROGR_RICHIESTA,
                                x.dip.INCENTIVO_LORDO_IP,
                                x.dip.UNA_TANTUM_LORDA_IP,
                                x.dip.DATA_INVIO_PROP,
                                x.dip.DATA_FIRMA_DIP,
                                x.dip.DTA_RECESSO,
                                x.dip.SINTESI1.COD_QUALIFICA,
                                Stato = (int)IncStato.RichiestaAccettata,
                                Q100_INCENTIVO_LORDO_IP = x.ip.INCENTIVO_LORDO_IP,
                                Q100_UNA_TANTUM_LORDA_IP = x.ip.UNA_TANTUM_LORDA_IP
                            }).ToList();

            var listAvv = db.XR_INC_DIPENDENTI
                                        .Where(x => x.COD_GRUPPO == codGruppo)
                            .Where(fAvv)
                            .Join(db.XR_INC_DIP_IPOTESI.Where(x => x.IS_VALID && x.COD_IPOTESI == "Q100"), dip => dip.ID_DIPENDENTE, ip => ip.ID_DIPENDENTE, (dip, ip) => new { dip, ip })
                            .Select(x => new
                            {
                                x.dip.MATRICOLA,
                                x.dip.PROGR_RICHIESTA,
                                x.dip.INCENTIVO_LORDO_IP,
                                x.dip.UNA_TANTUM_LORDA_IP,
                                x.dip.DATA_INVIO_PROP,
                                x.dip.DATA_FIRMA_DIP,
                                x.dip.DTA_RECESSO,
                                x.dip.SINTESI1.COD_QUALIFICA,
                                Stato = (int)IncStato.RecessoEffettuato,
                                Q100_INCENTIVO_LORDO_IP = x.ip.INCENTIVO_LORDO_IP,
                                Q100_UNA_TANTUM_LORDA_IP = x.ip.UNA_TANTUM_LORDA_IP
                            }).ToList();
            #endregion

            #region Calcoli
            var list = listIns;
            list.AddRange(listVer);
            list.AddRange(listAvv);
            var tmp = list.OrderBy(x => x.PROGR_RICHIESTA).ToList();

            decimal impMaxPratiche = 0;
            int numPratiche = 0;

            int daContrUnder = 0;
            decimal impDaContrUnder = 0;
            int contrUnder = 0;
            decimal impContrUnder = 0;
            int confUnder = 0;
            decimal impConfUnder = 0;
            int avviateUnder = 0;
            decimal impAvviateUnder = 0;

            int numPraticheOver = 0;
            decimal impPraticheOver = 0;

            int daContrOver = 0;
            decimal impDaContrOver = 0;
            int contrOver = 0;
            decimal impContrOver = 0;
            int confOver = 0;
            decimal impConfOver = 0;

            decimal impMaxPraticheQ100 = 0;
            int numPraticheQ100 = 0;

            int daContrUnderQ100 = 0;
            decimal impDaContrUnderQ100 = 0;
            int contrUnderQ100 = 0;
            decimal impContrUnderQ100 = 0;
            int confUnderQ100 = 0;
            decimal impConfUnderQ100 = 0;
            int avviateUnderQ100 = 0;
            decimal impAvviateUnderQ100 = 0;

            decimal impMaxPraticheOverQ100 = 0;
            int numPraticheOverQ100 = 0;

            int daContrOverQ100 = 0;
            decimal impDaContrOverQ100 = 0;
            int contrOverQ100 = 0;
            decimal impContrOverQ100 = 0;
            int confOverQ100 = 0;
            decimal impConfOverQ100 = 0;
            int avviateOverQ100 = 0;
            decimal impAvviateOverQ100 = 0;

            List<string> matrUnder = new List<string>();
            List<string> matrUnderQ100 = new List<string>();

            for (int i = 0; i < tmp.Count; i++)
            {
                decimal incentivoLordo = tmp[i].INCENTIVO_LORDO_IP.GetValueOrDefault();
                decimal unaTantum = tmp[i].UNA_TANTUM_LORDA_IP.GetValueOrDefault();
                decimal impegnoPers = incentivoLordo + unaTantum + CessazioneHelper.CalcoloOneri(tmp[i].COD_QUALIFICA, unaTantum);

                decimal incentivoLordoQ100 = tmp[i].Q100_INCENTIVO_LORDO_IP.GetValueOrDefault();
                decimal unaTantumQ100 = tmp[i].Q100_UNA_TANTUM_LORDA_IP.GetValueOrDefault();
                decimal impegnoPersQ100 = incentivoLordoQ100 + unaTantumQ100 + CessazioneHelper.CalcoloOneri(tmp[i].COD_QUALIFICA, unaTantumQ100);

                int stato = tmp[i].Stato;

                if (impMaxPratiche + impegnoPers <= model.MaxValue)
                {
                    impMaxPratiche = impMaxPratiche + impegnoPers;
                    numPratiche++;
                    matrUnder.Add(tmp[i].MATRICOLA);

                    switch (stato)
                    {
                        case (int)IncStato.RichiestaInserita:
                            daContrUnder++;
                            impDaContrUnder += (impegnoPers);
                            break;
                        case (int)IncStato.RichiestaAccettata:
                            if (!tmp[i].DATA_FIRMA_DIP.HasValue)
                            {
                                contrUnder++;
                                impContrUnder += (impegnoPers);
                            }
                            else if (!tmp[i].DTA_RECESSO.HasValue)
                            {
                                confUnder++;
                                impConfUnder += (impegnoPers);
                            }
                            else
                            {
                                avviateUnder++;
                                impAvviateUnder += (impegnoPers);
                            }
                            break;
                        case (int)IncStato.RecessoEffettuato:
                            avviateUnder++;
                            impAvviateUnder += (impegnoPers);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    impPraticheOver = impPraticheOver + impegnoPers;
                    numPraticheOver++;
                    switch (stato)
                    {
                        case (int)IncStato.RichiestaInserita:
                            daContrOver++;
                            impDaContrOver += (impegnoPers);
                            break;
                        case (int)IncStato.RichiestaAccettata:
                            if (!tmp[i].DATA_FIRMA_DIP.HasValue)
                            {
                                contrOver++;
                                impContrOver += (impegnoPers);
                            }
                            else if (!tmp[i].DTA_RECESSO.HasValue)
                            {
                                confOver++;
                                impConfOver += (impegnoPers);
                            }
                            break;
                        default:
                            break;
                    }
                }

                if (impMaxPraticheQ100 + impegnoPersQ100 <= model.MaxValue)
                {
                    impMaxPraticheQ100 += impegnoPersQ100;
                    numPraticheQ100++;
                    matrUnderQ100.Add(tmp[i].MATRICOLA);

                    switch (stato)
                    {
                        case (int)IncStato.RichiestaInserita:
                            daContrUnderQ100++;
                            impDaContrUnderQ100 += (impegnoPersQ100);
                            break;
                        case (int)IncStato.RichiestaAccettata:
                            if (!tmp[i].DATA_FIRMA_DIP.HasValue)
                            {
                                contrUnderQ100++;
                                impContrUnderQ100 += (impegnoPersQ100);
                            }
                            else if (!tmp[i].DTA_RECESSO.HasValue)
                            {
                                confUnderQ100++;
                                impConfUnderQ100 += (impegnoPersQ100);
                            }
                            else
                            {
                                avviateUnderQ100++;
                                impAvviateUnderQ100 += (impegnoPersQ100);
                            }
                            break;
                        case (int)IncStato.RecessoEffettuato:
                            avviateUnderQ100++;
                            impAvviateUnderQ100 += (impegnoPersQ100);
                            break;
                        default:
                            break;
                    }

                }
                else
                {
                    impMaxPraticheOverQ100 += impegnoPersQ100;
                    numPraticheOverQ100++;

                    switch (stato)
                    {
                        case (int)IncStato.RichiestaInserita:
                            daContrOverQ100++;
                            impDaContrOverQ100 += (impegnoPersQ100);
                            break;
                        case (int)IncStato.RichiestaAccettata:
                            if (!tmp[i].DATA_FIRMA_DIP.HasValue)
                            {
                                contrOverQ100++;
                                impContrOverQ100 += (impegnoPersQ100);
                            }
                            else if (!tmp[i].DTA_RECESSO.HasValue)
                            {
                                confOverQ100++;
                                impConfOverQ100 += (impegnoPersQ100);
                            }
                            else
                            {
                                avviateOverQ100++;
                                impAvviateOverQ100 += (impegnoPersQ100);
                            }
                            break;
                        case (int)IncStato.RecessoEffettuato:
                            avviateOverQ100++;
                            impAvviateOverQ100 += (impegnoPersQ100);
                            break;
                        default:
                            break;
                    }
                }
            }
            #endregion

            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Analisi costi");
            int row = 0;

            #region ImpegnoAttuale
            row = 2;
            ws.Cell(row, 1).SetValue<string>("Impegno attuale");
            ws.Cell(row, 1).Style.Font.FontSize = 18;
            ws.Range(row, 1, row, 3).Merge();

            row = 3;
            ws.Cell(row, 1).SetValue<string>(strUltimoAgg);
            ws.Cell(row, 1).Style.Font.SetItalic(true);
            ws.Range(row, 1, row, 3).Merge();

            row = 4;
            ws.Cell(row, 1).SetValue<string>(String.Format("{0:N2} %", model.Percentage)).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right; ;
            //string percFormat = "0.00%";
            //ws.Cell(row, 1).DataType = XLCellValues.Number;
            //ws.Cell(row, 1).Style.NumberFormat.Format = percFormat;

            ws.Cell(row, 2).SetValue<string>(String.Format("{0:N2} € su {1:N2} €", model.CurrentValue, model.MaxValue));
            ws.Cell(row, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Range(row, 2, row, 3).Merge();

            row = 5;
            ws.Cell(row, 1).SetValue<string>("Stato");
            ws.Cell(row, 2).SetValue<string>("Numero");
            ws.Cell(row, 3).SetValue<string>("Importo");
            row = 6;
            ws.Cell(row, 1).SetValue<string>("Situazione contributive verificate");
            ws.Cell(row, 2).SetValue<int>(contrUnder).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(row, 3).SetValue<decimal>(impContrUnder).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            row = 7;
            ws.Cell(row, 1).SetValue<string>("Proposte accettate");
            ws.Cell(row, 2).SetValue<int>(confUnder).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(row, 3).SetValue<decimal>(impConfUnder).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            row = 8;
            ws.Cell(row, 1).SetValue<string>("Recesso online");
            ws.Cell(row, 2).SetValue<int>(avviateUnder).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(row, 3).SetValue<decimal>(impAvviateUnder).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            var tableAct = ws.Range(5, 1, 8, 3).CreateTable();
            tableAct.ShowTotalsRow = true;
            tableAct.Field(0).TotalsRowLabel = "Totale";
            tableAct.Field(1).TotalsRowFunction = XLTotalsRowFunction.Sum;
            tableAct.Field(2).TotalsRowFunction = XLTotalsRowFunction.Sum;
            ws.Range(6, 3, 9, 3).Style.NumberFormat.Format = "#,##0.00 €";

            row = 11;
            ws.Cell(row, 1).SetValue<string>("Stato");
            ws.Cell(row, 2).SetValue<string>("Numero");
            row = 12;
            ws.Cell(row, 1).SetValue<string>("Annullate");
            ws.Cell(row, 2).SetValue<int>(numAnn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            row = 13;
            ws.Cell(row, 1).SetValue<string>("Rifiutate");
            ws.Cell(row, 2).SetValue<int>(numRif).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            row = 14;
            ws.Cell(row, 1).SetValue<string>("Decadute");
            ws.Cell(row, 2).SetValue<int>(numDec).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            var tableRif = ws.Range(11, 1, 14, 2).CreateTable();
            tableRif.ShowTotalsRow = true;
            tableRif.Field(0).TotalsRowLabel = "Totale";
            tableRif.Field(1).TotalsRowFunction = XLTotalsRowFunction.Sum;

            ws.Range(12, 3, 15, 3).Style.NumberFormat.Format = "#,##0.00 €";
            #endregion

            #region ImpegnoMassimoSostenibile
            row = 17;
            ws.Cell(row, 1).SetValue<string>("Impegno massimo sostenibile");
            ws.Cell(row, 1).Style.Font.FontSize = 18;
            ws.Range(row, 1, row, 3).Merge();

            row = 18;
            ws.Cell(row, 2).SetValue<string>(String.Format("Budget disponibile: {0:N2} €", model.MaxValue));
            ws.Range(row, 2, row, 3).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            row = 19;
            ws.Cell(row, 1).SetValue<string>("Stato");
            ws.Cell(row, 2).SetValue<string>("Numero");
            ws.Cell(row, 3).SetValue<string>("Importo");
            row = 20;
            ws.Cell(row, 1).SetValue<string>("Da verificare");
            ws.Cell(row, 2).SetValue<int>(daContrUnder).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(row, 3).SetValue<decimal>(impDaContrUnder).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            row = 21;
            ws.Cell(row, 1).SetValue<string>("Situazione contributive verificate");
            ws.Cell(row, 2).SetValue<int>(contrUnder).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(row, 3).SetValue<decimal>(impContrUnder).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            row = 22;
            ws.Cell(row, 1).SetValue<string>("Proposte accettate");
            ws.Cell(row, 2).SetValue<int>(confUnder).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(row, 3).SetValue<decimal>(impConfUnder).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            row = 23;
            ws.Cell(row, 1).SetValue<string>("Recesso online");
            ws.Cell(row, 2).SetValue<int>(avviateUnder).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(row, 3).SetValue<decimal>(impAvviateUnder).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            var tableSost = ws.Range(19, 1, 23, 3).CreateTable();
            tableSost.ShowTotalsRow = true;
            tableSost.Field(0).TotalsRowLabel = "Totale";
            tableSost.Field(1).TotalsRowFunction = XLTotalsRowFunction.Sum;
            tableSost.Field(2).TotalsRowFunction = XLTotalsRowFunction.Sum;

            ws.Range(20, 3, 24, 3).Style.NumberFormat.Format = "#,##0.00 €";

            row = 25;
            ws.Cell(row, 1).SetValue<string>("In attesa");
            ws.Cell(row, 2).SetValue<int>(daContrOver).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(row, 3).SetValue<decimal>(impDaContrOver).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            if (enabledSaving)
            {
                row = 27;
                ws.Cell(row, 1).SetValue<string>("Risparmio conseguibile");
                ws.Cell(row, 1).Style.Font.FontSize = 18;
                ws.Range(row, 1, row, 2).Merge();
                row = 28;
                ws.Cell(row, 1).SetValue<string>("Anno");
                ws.Cell(row, 2).SetValue<string>("Importo");

                foreach (var item in model.Saving.Where(x => matrUnder.Contains(x.matricola_dp)).GroupBy(x => x.Anno))
                {
                    row++;
                    ws.Cell(row, 1).SetValue<string>(String.Format("Anno {0}", item.Key));
                    ws.Cell(row, 2).SetValue<decimal>(item.Sum(x => x.Risparmio.GetValueOrDefault())).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                }

                var tableRisp = ws.Range(28, 1, row, 2).CreateTable();
                tableRisp.ShowTotalsRow = true;
                tableRisp.Field(0).TotalsRowLabel = "Totale";
                tableRisp.Field(1).TotalsRowFunction = XLTotalsRowFunction.Sum;

                ws.Range(29, 2, row + 1, 2).Style.NumberFormat.Format = "#,##0.00 €";
            }
            #endregion

            #region ImpegnoMassimoSostenibileQ100
            row = 17;
            ws.Cell(row, 7).SetValue<string>("Impegno massimo sostenibile Quota 100 Rai");
            ws.Cell(row, 7).Style.Font.FontSize = 18;
            ws.Range(row, 7, row, 9).Merge();

            row = 18;
            ws.Cell(row, 8).SetValue<string>(String.Format("Budget disponibile: {0:N2} €", model.MaxValue));
            ws.Range(row, 8, row, 9).Merge().Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            row = 19;
            ws.Cell(row, 7).SetValue<string>("Stato");
            ws.Cell(row, 8).SetValue<string>("Numero");
            ws.Cell(row, 9).SetValue<string>("Importo");
            row = 20;
            ws.Cell(row, 7).SetValue<string>("Da verificare");
            ws.Cell(row, 8).SetValue<int>(daContrUnderQ100).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(row, 9).SetValue<decimal>(impDaContrUnderQ100).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            row = 21;
            ws.Cell(row, 7).SetValue<string>("Situazione contributive verificate");
            ws.Cell(row, 8).SetValue<int>(contrUnderQ100).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(row, 9).SetValue<decimal>(impContrUnderQ100).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            row = 22;
            ws.Cell(row, 7).SetValue<string>("Proposte accettate");
            ws.Cell(row, 8).SetValue<int>(confUnderQ100).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(row, 9).SetValue<decimal>(impConfUnderQ100).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            row = 23;
            ws.Cell(row, 7).SetValue<string>("Recesso online");
            ws.Cell(row, 8).SetValue<int>(avviateUnderQ100).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(row, 9).SetValue<decimal>(impAvviateUnderQ100).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            var tableSostQ100 = ws.Range(19, 7, 23, 9).CreateTable();
            tableSostQ100.ShowTotalsRow = true;
            tableSostQ100.Field(0).TotalsRowLabel = "Totale";
            tableSostQ100.Field(1).TotalsRowFunction = XLTotalsRowFunction.Sum;
            tableSostQ100.Field(2).TotalsRowFunction = XLTotalsRowFunction.Sum;

            ws.Range(20, 9, 24, 9).Style.NumberFormat.Format = "#,##0.00 €";

            row = 25;
            ws.Cell(row, 7).SetValue<string>("In attesa");
            ws.Cell(row, 8).SetValue<int>(daContrOverQ100).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(row, 9).SetValue<decimal>(impDaContrOverQ100).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            if (enabledSaving)
            {
                row = 27;
                ws.Cell(row, 7).SetValue<string>("Risparmio conseguibile");
                ws.Cell(row, 7).Style.Font.FontSize = 18;
                ws.Range(row, 7, row, 8).Merge();

                row = 28;
                ws.Cell(row, 7).SetValue<string>("Anno");
                ws.Cell(row, 8).SetValue<string>("Importo");

                foreach (var item in model.Saving.Where(x => matrUnderQ100.Contains(x.matricola_dp)).GroupBy(x => x.Anno))
                {
                    row++;
                    ws.Cell(row, 7).SetValue<string>(String.Format("Anno {0}", item.Key));
                    ws.Cell(row, 8).SetValue<decimal>(item.Sum(x => x.Risparmio.GetValueOrDefault())).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                }

                var tableRispQ100 = ws.Range(28, 7, row, 8).CreateTable();
                tableRispQ100.ShowTotalsRow = true;
                tableRispQ100.Field(0).TotalsRowLabel = "Totale";
                tableRispQ100.Field(1).TotalsRowFunction = XLTotalsRowFunction.Sum;

                ws.Range(29, 8, row + 1, 8).Style.NumberFormat.Format = "#,##0.00 €";
            }
            #endregion

            //ws.Rows(2, row + 1).Height = 20;

            ws.Column(1).Width = 35;
            ws.Columns(2, 3).Width = 15;
            ws.Column(7).Width = 35;
            ws.Columns(8, 9).Width = 15;

            ws.Range(2, 1, row + 1, 9).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            MemoryStream ms = null;
            ms = new MemoryStream();
            wb.SaveAs(ms);
            ms.Position = 0;

            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = "Analisi costi.xlsx" };
        }

        public ActionResult Export(string tipoExport)
        {
            string fileName = "";
            MemoryStream ms = null;

            if (tipoExport == "nPratiche")
            {
                ms = ExportNPratiche();
                fileName = "Riepilogo richieste per direzione.xlsx";
            }
            else if (tipoExport == "riepilogoPratiche")
            {
                ms = CessazioneManager.ExportRiepilogoPratiche();
                fileName = "Riepilogo richieste.xlsx";
            }
            else if (tipoExport == "riepilogoSaving")
            {
                ms = CessazioneManager.ExportRiepilogoSaving();
                fileName = String.Format("Incentivi al {0:dd.MM.yyyy}.xlsx", DateTime.Today);
            }
            else if (tipoExport == "riepilogoSavingCumulativo")
            {
                ms = CessazioneManager.ExportRiepilogoSaving(true);
                fileName = String.Format("Incentivi al {0:dd.MM.yyyy}.xlsx", DateTime.Today);
            }
            else if (tipoExport == "riepilogoAnnullate")
            {
                ms = CessazioneManager.ExportRiepilogoPratiche("inattive");
                fileName = "Riepilogo richieste annullate.xlsx";
            }
            else
            {
                return View("~/Views/Shared/404.cshtml");
            }

            return File(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        private MemoryStream ExportNPratiche()
        {
            MemoryStream ms = new MemoryStream();
            XLWorkbook wb = new XLWorkbook();

            string codGruppo = HrisHelper.GetParametro<string>(HrisParam.IncentiviRifGruppo);

            IncentiviEntities db = new IncentiviEntities();
            var elenco = db.XR_INC_DIPENDENTI.Where(x => x.ID_TIPOLOGIA == (int)CessazioneTipo.Incentivazione && x.COD_GRUPPO == codGruppo).Select(x => new CessazioneModel
            {
                Pratica = x,
                Stato = x.XR_INC_OPERSTATI.Where(y => !y.XR_INC_STATI.IND_SYSTEM.Value && !y.DATA_FINE_VALIDITA.HasValue)
                          .Select(w => w.XR_INC_STATI).OrderByDescending(z => z.XR_WKF_WORKFLOW.FirstOrDefault(a => a.ID_TIPOLOGIA == x.ID_TIPOLOGIA).ORDINE)
                          .FirstOrDefault(),
                Sintesi = x.SINTESI1,
                Qualifica = x.SINTESI1.QUALIFICA,
                QualStd = x.SINTESI1.QUALIFICA.TB_QUALSTD
            }).ToList();

            //InternalExportNPratiche(wb, elenco, "Riepilogo");
            int row = 1;
            var ws = wb.AddWorksheet("Riepilogo pratiche");

            foreach (var ser in elenco.GroupBy(x => x.Sintesi.COD_SERVIZIO).OrderBy(y => y.Key))
            {
                string descrizione = ser.First().Sintesi.DES_SERVIZIO;
                descrizione = descrizione.Replace("&", "E")
                                         .Replace("/", "")
                                         .Replace("\\", "")
                                         .Replace("?", "")
                                         .Replace(":", "")
                                         .Replace("[", "")
                                         .Replace("[", "");

                if (descrizione.StartsWith("'"))
                    descrizione = descrizione.Substring(1);
                if (descrizione.EndsWith("'"))
                    descrizione = descrizione.Substring(0, descrizione.Length - 1);

                List<string> qualGG = new List<string>() { "M", "A7" };
                List<string> qualNotQIO = new List<string>() { "A01", "M", "A7" };
                List<int> statiDecRif = new List<int>() { (int)IncStato.RichiestaDecaduta, (int)IncStato.RichiestaRifiutata };


                IXLRange range = null;

                if (row != 1)
                    row += 4;

                #region FirstTable
                int startRowFirstTable = row;
                ws.Cell(row, 1).SetValue(descrizione);
                ws.Cell(row, 1).Style.Font.Bold = true;
                ws.Range(row, 1, row, 3).Merge();
                foreach (var data in ser.GroupBy(x => x.Pratica.DATA_CESSAZIONE.Value).OrderBy(x => x.Key))
                {
                    ws.Cell(row, 4).SetValue(data.Key);
                    foreach (var figPro in data.GroupBy(x => x.QualStd.DES_QUALSTD).OrderBy(x => x.Key))
                    {
                        ws.Cell(row, 5).SetValue(figPro.Key);
                        ws.Cell(row, 6).SetValue(figPro.Count());
                        row++;
                    }
                }

                ws.Cell(row, 1).SetValue(descrizione);
                ws.Cell(row, 6).SetValue(ser.Count());
                ws.Range(row, 6, row + 1, 6).Merge();

                row++;
                ws.Cell(row, 1).SetValue("Totale");

                range = ws.Range(startRowFirstTable, 1, row, 6);
                range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                range = ws.Range(startRowFirstTable, 4, row - 2, 4);
                range.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 32, 96);
                range.Style.Font.Bold = true;
                range.Style.Font.FontColor = XLColor.White;
                range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                range = ws.Range(startRowFirstTable, 5, row - 2, 6);
                range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                range.Style.Alignment.WrapText = true;

                range = ws.Range(startRowFirstTable, 6, row, 6);
                range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                range = ws.Range(row - 1, 1, row, 6);
                range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                range.Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217);
                range.Style.Font.Bold = true;

                range = ws.Range(startRowFirstTable, 1, row - 2, 3);
                range.Merge();
                ws.Cell(startRowFirstTable, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                #endregion

                row += 2;

                #region secondTable
                int startRowSecondTable = row;
                ws.Cell(row, 1).SetValue("Richieste adesione pervenute");
                ws.Cell(row, 1).Style.Alignment.WrapText = true;
                ws.Range(row, 1, row + 1, 1).Merge();

                ws.Cell(row, 2).SetValue("di cui:");
                ws.Range(row, 2, row, 3).Merge();

                ws.Cell(row, 4).SetValue("proposte inviate ai dipendenti");
                ws.Range(row, 4, row, 5).Merge();

                ws.Cell(row, 6).SetValue("accettazioni");
                ws.Range(row, 6, row, 7).Merge();

                ws.Cell(row, 8).SetValue("esclusioni dall’iniziativa per rinuncia e/o decadenza dai termini");
                ws.Range(row, 8, row, 9).Merge();

                ws.Cell(row, 10).SetValue("RECESSO ON LINE CESSAZIONE");
                ws.Range(row, 10, row + 1, 10).Merge();

                row++;
                ws.Cell(row, 2).SetValue("personale giornalisti");
                ws.Cell(row, 3).SetValue("quadri impiegati ed operai");

                ws.Cell(row, 4).SetValue("personale giornalisti");
                ws.Cell(row, 5).SetValue("quadri impiegati ed operai");

                ws.Cell(row, 6).SetValue("personale giornalisti");
                ws.Cell(row, 7).SetValue("quadri impiegati ed operai");

                ws.Cell(row, 8).SetValue("personale giornalisti");
                ws.Cell(row, 9).SetValue("quadri impiegati ed operai");

                range = ws.Range(startRowSecondTable, 1, row, 10);
                range.Style.Font.Italic = true;
                range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                range.Style.Alignment.WrapText = true;

                ws.Range(startRowSecondTable, 1, row, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 225, 242);
                ws.Range(startRowSecondTable, 2, row, 3).Style.Fill.BackgroundColor = XLColor.FromArgb(180, 198, 231);
                ws.Range(startRowSecondTable, 4, row, 5).Style.Fill.BackgroundColor = XLColor.FromArgb(142, 169, 219);
                ws.Range(startRowSecondTable, 6, row, 7).Style.Fill.BackgroundColor = XLColor.FromArgb(132, 151, 176);
                ws.Range(startRowSecondTable, 8, row, 9).Style.Fill.BackgroundColor = XLColor.FromArgb(68, 114, 196);
                ws.Range(startRowSecondTable, 10, row, 10).Style.Fill.BackgroundColor = XLColor.FromArgb(217, 225, 242);

                row++;
                ws.Cell(row, 1).SetValue(ser.Count());

                var gg = ser.Where(x => qualGG.Any(y => x.Sintesi.COD_QUALIFICA.StartsWith(y)));
                var qio = ser.Where(x => !qualNotQIO.Any(y => x.Sintesi.COD_QUALIFICA.StartsWith(y)));
                ws.Cell(row, 2).SetValue(gg.Count());
                ws.Cell(row, 3).SetValue(qio.Count());

                ws.Cell(row, 4).SetValue(gg.Where(x => x.Pratica.DATA_INVIO_PROP.HasValue).Count());
                ws.Cell(row, 5).SetValue(qio.Where(x => x.Pratica.DATA_INVIO_PROP.HasValue).Count());

                ws.Cell(row, 6).SetValue(gg.Where(x => x.Pratica.DATA_FIRMA_DIP.HasValue).Count());
                ws.Cell(row, 7).SetValue(qio.Where(x => x.Pratica.DATA_FIRMA_DIP.HasValue).Count());

                ws.Cell(row, 8).SetValue(gg.Where(x => statiDecRif.Contains(x.Stato.ID_STATO)).Count());
                ws.Cell(row, 9).SetValue(qio.Where(x => statiDecRif.Contains(x.Stato.ID_STATO)).Count());
                ws.Cell(row, 10).SetValue(ser.Where(x => x.Pratica.DTA_RECESSO.HasValue).Count());
                range = ws.Range(row, 1, row, 10);
                range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                #endregion

                for (int i = startRowSecondTable; i < row; i++)
                    ws.Row(i).Height = 60;
            }

            for (int i = 1; i <= 10; i++)
                ws.Column(i).Width = 15;



            ms = new MemoryStream();
            wb.SaveAs(ms);
            ms.Position = 0;

            return ms;
        }

        public ActionResult GetIban()
        {
            if (CommonHelper.GetCurrentUserMatricola() != "103650")
                return Redirect("/Home/notAuth");

            IncentiviEntities db = new IncentiviEntities();

            XLWorkbook workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Elenco dati");
            int counter = 1;
            worksheet.Cell(counter, 1).Value = "Matricola";
            worksheet.Cell(counter, 2).Value = "Nominativo";
            worksheet.Cell(counter, 3).Value = "Selezione";
            worksheet.Cell(counter, 4).Value = "Iban";
            worksheet.Cell(counter, 5).Value = "Vincolo BCCR";
            worksheet.Cell(counter, 6).Value = "Data cessazione";
            worksheet.Cell(counter, 7).Value = "Data pagamento";

            DateTime limitMin = new DateTime(2020, 2, 1);
            DateTime limitMax = new DateTime(2020, 2, 28);

            var isCurrentState = CessazioneHelper.IsCurrentState((int)IncStato.VerbaleCaricato);

            foreach (var dip in db.XR_INC_DIPENDENTI.Where(isCurrentState)
                .Where(x => x.DATA_PAGAMENTO.Value <= limitMax
                    && x.DATA_PAGAMENTO.Value > limitMin)
                .OrderBy(z => z.MATRICOLA))
            {
                counter++;

                worksheet.Cell(counter, 1).SetValue<string>(dip.MATRICOLA);
                worksheet.Cell(counter, 2).SetValue<string>(dip.ANAGPERS.DES_COGNOMEPERS + " " + dip.ANAGPERS.DES_NOMEPERS);
                if (String.IsNullOrWhiteSpace(dip.IND_PROPRIO_IBAN) || dip.IND_PROPRIO_IBAN == "Y")
                {
                    var datiBancari = dip.ANAGPERS.XR_DATIBANCARI.OrderByDescending(y => y.DTA_FINE).FirstOrDefault(x => x.XR_UTILCONTO.Count() == 1 && x.XR_UTILCONTO.First().COD_UTILCONTO == "01");
                    worksheet.Cell(counter, 3).SetValue<string>("Presente a sistema");
                    worksheet.Cell(counter, 4).SetValue<string>(datiBancari.COD_IBAN);
                }
                else if (dip.IND_PROPRIO_IBAN != "B")
                {
                    worksheet.Cell(counter, 3).SetValue<string>("Selezione utente");
                    worksheet.Cell(counter, 4).SetValue<string>(dip.IBAN);
                }
                else
                {
                    worksheet.Cell(counter, 3).SetValue<string>("Vincolo BCCR");
                    worksheet.Cell(counter, 4).SetValue<string>("IT86W0200809440000500043357");
                }

                worksheet.Cell(counter, 6).SetValue<string>(dip.DATA_CESSAZIONE.Value.ToString("dd/MM/yyyy"));
                worksheet.Cell(counter, 7).SetValue<string>(dip.DATA_PAGAMENTO.Value.ToString("dd/MM/yyyy"));
            }

            worksheet.Columns().AdjustToContents();

            MemoryStream M = new MemoryStream();
            workbook.SaveAs(M);
            M.Position = 0;

            return new FileStreamResult(M, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = "Elenco.xlsx" };
        }

        public ActionResult ExportAppuntamenti()
        {
            IncentiviEntities db = new IncentiviEntities();

            var isAppuntamento = CessazioneHelper.IsCurrentState((int)IncStato.Appuntamento);

            var dipendenti = db.XR_INC_DIPENDENTI.Where(isAppuntamento).OrderBy(x => x.DATA_APPUNTAMENTO).ToList()
                .Select(b => new
                {
                    Matricola = b.MATRICOLA,
                    Nominativo = b.SINTESI1.DES_COGNOMEPERS.TitleCase() + " " + b.SINTESI1.DES_NOMEPERS.TitleCase(),
                    DataAppuntamento = b.DATA_APPUNTAMENTO.HasValue ? b.DATA_APPUNTAMENTO.Value.ToString("dd/MM/yyyy") : "",
                    OrarioAppuntamento = b.DATA_APPUNTAMENTO.HasValue ? b.DATA_APPUNTAMENTO.Value.ToString("HH:mm") : "",
                    Sede = b.SEDE,
                    Sindacato = b.XR_INC_SIGLESINDACALI != null ? b.XR_INC_SIGLESINDACALI.SINDACATO : "",
                    RapprRai = b.XR_INC_RAPPRRAI != null && b.XR_INC_RAPPRRAI.ID_RAPPRRAI > 0 ? b.XR_INC_RAPPRRAI.ANAGPERS.DES_COGNOMEPERS.TitleCase() + " " + b.XR_INC_RAPPRRAI.ANAGPERS.DES_NOMEPERS.TitleCase() : "",
                    InCarico = b.XR_INC_OPERSTATI.FirstOrDefault(c => c.ID_STATO == (int)IncStato.InCarico) != null ? b.XR_INC_OPERSTATI.FirstOrDefault(c => c.ID_STATO == (int)IncStato.InCarico).SINTESI1.Nominativo() : ""
                });

            XLWorkbook workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Elenco appuntamenti");

            var table = worksheet.Cell(1, 1).InsertTable(dipendenti, "TabellaAppuntamenti", true);

            var sheetPivot = workbook.Worksheets.Add("Pivot");
            var pt = sheetPivot.PivotTables.Add("TabellaPivot", sheetPivot.Cell(1, 1), table.AsRange());
            pt.MergeAndCenterWithLabels = true;
            pt.AutofitColumns = true;
            pt.ColumnLabels.Add("Sindacato");
            pt.RowLabels.Add("DataAppuntamento");
            pt.RowLabels.Add("OrarioAppuntamento");
            pt.Values.Add("Matricola").SummaryFormula = XLPivotSummary.Count;

            worksheet.Columns().AdjustToContents();
            sheetPivot.Columns().AdjustToContents();

            MemoryStream ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;


            string nomeFile = "Elenco appuntamenti";
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = nomeFile + ".xlsx" };
        }
        #endregion

        #region CertificatoServizio
        public ActionResult LoadCertServ()
        {
            var db = new IncentiviEntities();
            var file = System.IO.File.ReadAllBytes(@"C:\Users\Nik\Desktop\certificato di servizio bozza.docx");
            CezanneHelper.GetCampiFirma(out CampiFirma campiFirma);
            var template = new XR_INC_TEMPLATE()
            {
                COD_USER = campiFirma.CodUser,
                COD_TERMID = campiFirma.CodTermid,
                TMS_TIMESTAMP = campiFirma.Timestamp,
                COD_TIPO = "CertificatoServizio",
                CONTENT_TYPE = "",
                DES_TEMPLATE = "",
                IND_BODY = true,
                IND_HEADER = true,
                IND_FOOTER = true,
                IND_SIGN = false,
                NME_TEMPLATE = "",
                CAT_ESCLUSE = "A01,A7,M",
                TEMPLATE = file,
                VALID_DTA_INI = campiFirma.Timestamp
            };
            db.XR_INC_TEMPLATE.Add(template);
            db.SaveChanges();

            return Content("OK");
        }
        [HttpPost]
        public ActionResult GestioneCertServ(int idDip)
        {
            var certificato = CessazioneHelper.GetCertificatoServizio(idDip);

            return View("subpartial/Modal_Certificato", certificato);
        }
        [HttpPost]
        public ActionResult GeneraCertificatoServizio(int idDip, string primo, string secondo)
        {
            bool result = false;
            string errorMsg = "";
            result = CessazioneHelper.GeneraCertificatoServizio(idDip, primo, secondo, out errorMsg);
            if (result)
            {
                //Aggiunta task
                TaskHelper.AddBatchRunnerTask("IncentiviConvertiCertificati", out var taskError, arguments: idDip.ToString() + " " + (CommonHelper.IsProduzione() ? "prod" : "svil"));
            }

            return Json(new { esito = result, errorMsg = errorMsg }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult ModificaCertificato(int idDip)
        {
            var db = new IncentiviEntities();
            var dip = db.XR_INC_DIPENDENTI.Find(idDip);
            dip.SetField<DateTime?>("DataCertServizio", null);
            var template = CessazioneHelper.GetTemplate(db, "CertificatoServizioPDF", idDip, "", true, dip.SINTESI1.COD_QUALIFICA);
            if (template != null)
                template.VALID_DTA_END = DateTime.Now;
            DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());

            return GestioneCertServ(idDip);
        }
        public ActionResult DownloadCertificato(int id)
        {
            var db = new IncentiviEntities();
            var dip = db.XR_INC_DIPENDENTI.Find(id);
            var templatePDF = CessazioneHelper.GetTemplate(db, "CertificatoServizioPDF", id, "", true, dip.SINTESI1.COD_QUALIFICA);
            Response.AddHeader("Content-Disposition", String.Format("inline; filename=Certificato di servizio {0} - {1}.pdf", dip.MATRICOLA, dip.SINTESI1.Nominativo().TitleCase()));
            return new FileContentResult(templatePDF.TEMPLATE, "application/pdf");
        }
        [HttpPost]
        public ActionResult InviaCertServ(int idDip)
        {
            bool result = false;
            string errorMsg = "";

            var db = new IncentiviEntities();
            var dip = db.XR_INC_DIPENDENTI.Find(idDip);

            var templatePDF = CessazioneHelper.GetTemplate(db, "CertificatoServizioPDF", dip.ID_DIPENDENTE, "", true, dip.SINTESI1.COD_QUALIFICA);
            var template = CessazioneHelper.GetTemplate(db, "TemplateMail", dip.ID_DIPENDENTE, "MailCertificatoServizio", true, dip.SINTESI1.COD_QUALIFICA);
            if (template != null)
            {
                string mailOggetto = CessazioneHelper.ReplaceToken(dip, template.DES_TEMPLATE);
                string mailTesto = CessazioneHelper.ReplaceToken(dip, template.TEMPLATE_TEXT);

                string mailDest = CessazioneHelper.GetMailDip(dip); //CommonTasks.GetEmailPerMatricola(dip.MATRICOLA);
                GestoreMail mail = new GestoreMail();
                string mittente = CessazioneHelper.GetIndirizzoMail("FromCertificatoServizio");
                string mailCC = mittente;
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

                var attachment = new List<myRaiCommonTasks.sendMail.Attachement>();
                attachment.Add(new myRaiCommonTasks.sendMail.Attachement()
                {
                    AttachementName = String.Format("Certificato di servizio {0} - {1}.pdf", dip.MATRICOLA, dip.SINTESI1.Nominativo().TitleCase()),
                    AttachementType = "application/pdf",
                    AttachementValue = templatePDF.TEMPLATE
                });

                var response = mail.InvioMail(mailTesto, mailOggetto, mailDest, mittente, mittente, attachment, null, mailCCN);
                if (response != null && response.Errore != null)
                {
                    HrisHelper.LogOperazione("IncInvioMail", String.Format("{0} - Invio mail certificato servizio fallito - {1}", dip.MATRICOLA, mailOggetto), false, response.Errore);
                }
                else
                {
                    HrisHelper.LogOperazione("IncInvioMail", String.Format("{0} - Invio mail certificato servizio {1}", dip.MATRICOLA, mailOggetto), true);
                    dip.SetField<DateTime?>("DataInvioCertServizio", DateTime.Now);
                    dip.SetField<string>("UtenteInvioCertServizio", CommonHelper.GetCurrentUserMatricola());

                    result = DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola());
                    if (!result)
                        errorMsg = "La mail è stata inviata correttamente, tuttavia si è verificato un errore nel salvataggio";
                }
            }
            else
                errorMsg = "Invio mail non riuscito";

            return Json(new { esito = result, errorMsg = errorMsg }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult RestoreDefaultTemplateCert(int idDip)
        {
            var db = new IncentiviEntities();
            string codTipo = "TemplateMail";
            string name = "MailCertificatoServizio";

            var dip = db.XR_INC_DIPENDENTI.Find(idDip);
            var templateMail = CessazioneHelper.GetTemplate(db, "TemplateMail", dip.ID_DIPENDENTE, "MailCertificatoServizio", true, dip.SINTESI1.COD_QUALIFICA);
            
            if (templateMail != null && templateMail.ID_DIPENDENTE.HasValue)
                templateMail.VALID_DTA_END = DateTime.Now;
            
            if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                return Content("OK");
            else
                return Content("Errore durante il salvataggio");
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult SaveTemplateMailCert(int idDip, string testo)
        {
            string message = "";
            var db = new IncentiviEntities();
            string codTipo = "TemplateMail";
            string name = "MailCertificatoServizio";

            var dip = db.XR_INC_DIPENDENTI.Find(idDip);
            var templateMail = CessazioneHelper.GetTemplate(db, "TemplateMail", dip.ID_DIPENDENTE, "MailCertificatoServizio", true, dip.SINTESI1.COD_QUALIFICA);
            string _textAsEditor = CessazioneHelper.ReplaceToken(dip, templateMail.TEMPLATE_TEXT.Replace("\r\n", "\n"));
            if (_textAsEditor != testo)
            {
                CezanneHelper.GetCampiFirma(out var campiFirma);
                if (templateMail.ID_DIPENDENTE.HasValue)
                    templateMail.VALID_DTA_END = campiFirma.Timestamp;

                var newTemplate = new XR_INC_TEMPLATE()
                {
                    CAT_ESCLUSE = null,
                    CAT_INCLUSE = null,
                    COD_TERMID = campiFirma.CodTermid,
                    COD_USER = campiFirma.CodUser,
                    COD_TIPO = "TemplateMail",
                    CONTENT_TYPE = "",
                    DES_TEMPLATE = "",
                    DIR_ESCLUSE = null,
                    DIR_INCLUSE = null,
                    ID_DIPENDENTE = dip.ID_DIPENDENTE,
                    IND_BODY = true,
                    IND_FOOTER = true,
                    IND_HEADER = true,
                    IND_SIGN = false,
                    NME_TEMPLATE = "MailCertificatoServizio",
                    SEDE = null,
                    TEMPLATE_TEXT = testo,
                    TMS_TIMESTAMP = campiFirma.Timestamp,
                    VALID_DTA_END = null,
                    VALID_DTA_INI = campiFirma.Timestamp
                };
                db.XR_INC_TEMPLATE.Add(newTemplate);

                if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                    message = "OK";
                else
                    message = "Errore durante il salvataggio";
            }
            else
                message = "OK"; //Non essendoci variazione va bene cosi

            return Content(message);
        }
        #endregion
    }
}
