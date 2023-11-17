using DocumentFormat.OpenXml.EMMA;
using myRai.DataAccess;
using myRaiCommonManager;
using myRaiCommonModel;
using myRaiCommonModel.ess;
using myRaiCommonModel.Services;
using myRaiData;
using myRaiData.Incentivi;
using myRaiHelper;
using MyRaiServiceInterface.MyRaiServiceReference1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Web;
using System.Web.Mvc;
using static myRaiHelper.AccessoFileHelper;

namespace myRaiGestionale.Controllers
{
    public class AnagraficaController : BaseCommonController
    {
        public ActionResult TestDirectory()
        {
            string result = "";

            string pathDir = HrisHelper.GetParametro<string>(HrisParam.PathModificaAnagrafica);
            try
            {
                ImpersonationHelper.Impersonate("RAI", "srvcezanneweb", "ac42t3pf", delegate
                {
                    var tmp = System.IO.Directory.GetFiles(pathDir);
                    result = String.Join("\r\n", tmp);
                    System.IO.File.WriteAllText(System.IO.Path.Combine(pathDir, "NC_test_file.txt"), "Questo è un test");
                });
            }
            catch (Exception ex)
            {

                result = ex.Message;
            }

            return Content(result);
        }


        public ActionResult Index(string m = "", int idPersona = 0, bool isNeoMatr = false)
        {
            if (String.IsNullOrWhiteSpace(m) && idPersona == 0)
            {
                return View("IndexSearch");
            }
            else
            {
                AnagraficaModel model = new AnagraficaModel();
                model = AnagraficaManager.GetAnagrafica(m, idPersona, null, true);

                // una volta calcolati i dati anagrafici ed il resto dei box da mostrare nella scheda anagrafica
                // controlliamo se per l'utente corrente, ci sono azioni da svolgere che sono in scadenza
                // in riferimento alla matricola della scheda anagrafica calcolata.
                model.Cose_Da_Fare = Widget_Da_Fare_Manager.GetAzioniDaFare(
                    UtenteHelper.Matricola(),
                    (UtenteHelper.Matricola() == model.Matricola) ? null : model.Matricola);

                HrisHelper.LogOperazione("Anagrafica", $"Matricola:{m} IdPersona:{idPersona} CodErrore:{model.CodErrorMsg} Errore:{model.ErrorMsg}", String.IsNullOrWhiteSpace(model.CodErrorMsg));

                if (model.CodErrorMsg == "401")
                    return new RedirectResult("/Home/notAuth");
                else if (model.CodErrorMsg == "404")
                    return View("404");
                else
                    return View("Index", model);
            }
        }

        public ActionResult View_DatiDipendente(string m = "", int idPersona = 0, bool isNeoMatr = false, string customFunc = "")
        {
            AnagraficaModel model = null;


            SezListLoadOption[] sezList =
            {
                new SezListLoadOption(SezioniAnag.Anagrafici, false ),
                new SezListLoadOption(SezioniAnag.Recapiti, false ),
                new SezListLoadOption(SezioniAnag.Bancari, true ),
                new SezListLoadOption(SezioniAnag.Debitoria, true ),
                new SezListLoadOption(SezioniAnag.Domicilio, false ),
                new SezListLoadOption(SezioniAnag.Formazione, true ),
                new SezListLoadOption(SezioniAnag.Presenze, true ),
                new SezListLoadOption(SezioniAnag.Residenza, false),
                new SezListLoadOption(SezioniAnag.Retribuzione, true),
                new SezListLoadOption(SezioniAnag.TipoContratti, true),
                new SezListLoadOption(SezioniAnag.Ruoli, true),
                new SezListLoadOption(SezioniAnag.Sedi, true),
                new SezListLoadOption(SezioniAnag.Servizi,true),
                new SezListLoadOption(SezioniAnag.Qualifiche, true),
                //new SezListLoadOption(SezioniAnag.StatoRapporto, false),
                new SezListLoadOption(SezioniAnag.TitoliStudio, false),
                new SezListLoadOption(SezioniAnag.Contenzioso, true),
                new SezListLoadOption(SezioniAnag.MieiDoc, true),
                new SezListLoadOption(SezioniAnag.Cedolini, true),
                new SezListLoadOption(SezioniAnag.Trasferte, true),
                new SezListLoadOption(SezioniAnag.SpeseProduzione, true),
                new SezListLoadOption(SezioniAnag.Documenti, true)
            };

            model = AnagraficaManager.GetAnagrafica(m, idPersona, new AnagraficaLoader(sezList), true, customFunc);

            if (model.isAbilitatoGestionale)
            {
                model.Lingue = AnagraficaManager.GetLingue(model.Matricola);
                model.Cdigitali = AnagraficaManager.GetCompDigitali(model.Matricola);
            }

            return View(model);
        }

        public ActionResult Modal_DatiDipendente(string m, int idPersona = 0, SezioniAnag[] sezList = null, string customFunc = "")
        {


            if (sezList == null)
            {
                sezList = new SezioniAnag[] {
                    SezioniAnag.Anagrafici,
                    SezioniAnag.Residenza,
                    SezioniAnag.Domicilio,
                    SezioniAnag.Bancari,
                    SezioniAnag.TitoliStudio,
                    SezioniAnag.StatoRapporto,
                    SezioniAnag.Struttura,
                    SezioniAnag.Trasferte,
                    SezioniAnag.SpeseProduzione,
                    SezioniAnag.Familiari
                };
            }

            var loader = new AnagraficaLoader(sezList)
            {
                EnabledAdd = false,
                EnabledDelete = false,
                EnabledModify = false
            };

            AnagraficaModel model = AnagraficaManager.GetAnagrafica(m, idPersona, loader, true, customFunc);

            return View(model);
        }

        public ActionResult Widget_DatiDipendente(string m = "", int idPersona = 0, bool isNeoMatr = false, bool actionToAnagrafica = false, bool actionState = false, SezioniAnag[] paramList = null, bool showCV = true, bool showInc = true, string customFunc = "", bool fromAssunzioni = false)
        {

            AnagraficaModel model = null;

            SezioniAnag[] sezList =
            {
                SezioniAnag.StatoRapporto,
                SezioniAnag.Struttura,
                SezioniAnag.Curricula
            };
            model = AnagraficaManager.GetAnagrafica(m, idPersona, new AnagraficaLoader(sezList), false, customFunc, fromAssunzioni);
            model.ActionToAnagrafica = actionToAnagrafica;
            model.ActionState = actionState;
            model.ShowCV = showCV;
            model.ShowInc = showInc;

            return View(model);
        }

        public ActionResult Header_DatiDipendente(string m = "", int idPersona = 0, bool isNeoMatr = false, bool actionToAnagrafica = false, string viewInfo = null, string customFunc = "")
        {
            SezioniAnag[] sezList =
            {
                SezioniAnag.StatoRapporto,
                SezioniAnag.Struttura
            };
            AnagraficaModel model = null;
            model = AnagraficaManager.GetAnagrafica(m, idPersona, new AnagraficaLoader(sezList), true, customFunc);
            model.ActionToAnagrafica = actionToAnagrafica;

            if (!String.IsNullOrWhiteSpace(viewInfo))
                model.ViewInfo = viewInfo.Split(',');

            return View(model);
        }

        public ActionResult GetRichieste(string m)
        {
            RichiestaLoader loader = new RichiestaLoader() { Matricola = m };
            loader.Tipologie.Add(TipoRichiestaAnag.IBAN);
            loader.Tipologie.Add(TipoRichiestaAnag.VariazioneContrattuale);

            var richieste = AnagraficaManager.GetRichieste(loader);
            return View("subpartial/Elenco_Richieste", richieste);
        }
        public ActionResult Modal_Richiesta(string m, TipoRichiestaAnag tipo, int id)
        {
            var richiesta = AnagraficaManager.GetRichiesta(m, tipo, id);
            return View("subpartial/Modal_richiesta", richiesta);
        }

        public ActionResult GetGestioni(string m)
        {
            var gestioni = AnagraficaManager.GetGestioni(m);
            return View("subpartial/Elenco_Gestioni", gestioni);
        }

        public ActionResult Load_DatiAnagrafica(string m, string customFunc = "")
        {
            AnagraficaModel model = AnagraficaManager.GetAnagrafica(m, null, new AnagraficaLoader(SezioniAnag.Anagrafici), true, customFunc);
            return View("subpartial/Tab_DatiAnagrafici", model.DatiAnagrafici);
        }
        public ActionResult Modal_Anagrafica(string m)
        {
            AnagraficaModel model = AnagraficaManager.GetAnagrafica(m, null, new AnagraficaLoader(SezioniAnag.Anagrafici));
            return View("subpartial/Modal_DatiAnagrafici", model.DatiAnagrafici);
        }
        public ActionResult Save_Anagrafica(AnagraficaDatiAnag model)
        {
            if (AnagraficaManager.Save_Anagrafica(model, out string errorMsg))
                return Content("OK");
            else
                return Content(errorMsg);
        }

        public ActionResult Load_DatiRecapiti(string m, string customFunc = "")
        {
            AnagraficaModel model = AnagraficaManager.GetAnagrafica(m, null, new AnagraficaLoader(SezioniAnag.Recapiti), true, customFunc);
            return View("subpartial/Tab_DatiRecapiti", model.DatiRecapiti);
        }
        public ActionResult Modal_Recapiti(string m)
        {
            AnagraficaModel model = AnagraficaManager.GetAnagrafica(m, null, new AnagraficaLoader(SezioniAnag.Recapiti));
            return View("subpartial/Modal_DatiRecapiti", model.DatiRecapiti);
        }
        public ActionResult Save_Recapiti(AnagraficaRecapiti model)
        {
            if (AnagraficaManager.Save_Recapiti(model, out string errorMsg))
                return Content("OK");
            else
                return Content(errorMsg);
        }

        public ActionResult Load_DatiResidenzaDomicilio(string m, string customFunc = "")
        {
            AnagraficaModel model = AnagraficaManager.GetAnagrafica(m, null, new AnagraficaLoader(SezioniAnag.Residenza, SezioniAnag.Domicilio), true, customFunc);
            return View("subpartial/Tab_DatiResidenzaDomicilio", model.DatiResidenzaDomicilio);
        }
        public ActionResult Modal_Indirizzo(string m, string tipologia, bool nuovo)
        {
            SezioniAnag sezione = SezioniAnag.NonDefinito;
            if (tipologia == "residenza")
                sezione = SezioniAnag.Residenza;
            else if (tipologia == "domicilio")
                sezione = SezioniAnag.Domicilio;

            AnagraficaModel model = AnagraficaManager.GetAnagrafica(m, null, new AnagraficaLoader(sezione));

            IndirizzoModel indirizzo = new IndirizzoModel();
            switch (sezione)
            {
                case SezioniAnag.Residenza:
                    indirizzo.Tipologia = IndirizzoType.Residenza;
                    if (model.DatiResidenzaDomicilio.Residenza == null || model.DatiResidenzaDomicilio.Residenza.IsNew)
                    {
                        //Assunzione, quindi primo indirizzo
                        indirizzo.G_PrimoIndirizzo = true;
                        indirizzo.G_CambioRes = model.DataAssunzione;
                        indirizzo.Decorrenza = model.DataAssunzione;
                    }
                    else if (!nuovo)
                    {
                        indirizzo = model.DatiResidenzaDomicilio.Residenza;
                        indirizzo.G_CambioRes = indirizzo.Decorrenza;
                    }
                    break;
                case SezioniAnag.Domicilio:
                    indirizzo.Tipologia = IndirizzoType.Domicilio;
                    if (!nuovo)
                        indirizzo = model.DatiResidenzaDomicilio.Domicilio;
                    break;
                default:
                    break;
            }

            indirizzo.IsNew = nuovo;

            indirizzo.IdPersona = model.IdPersona;
            indirizzo.Matricola = model.Matricola;

            string hrisAbil = AuthHelper.HrisFuncAbil(CommonHelper.GetCurrentUserMatricola());
            if (String.IsNullOrWhiteSpace(hrisAbil))
                hrisAbil = "HRCE";

            if (hrisAbil == "HRCE")
                indirizzo.G_Contabilita = AuthHelper.EnabledToSubFunc(CommonHelper.GetCurrentUserMatricola(), "HRCE", "RESIDENZA_AC");
            else
                indirizzo.G_Contabilita = AuthHelper.EnabledToSubFunc(CommonHelper.GetCurrentUserMatricola(), "HRIS_PERS", "RESADM");

            return View("subpartial/Modal_DatiIndirizzo", indirizzo);
        }


        public ActionResult Save_Indirizzo(IndirizzoModel model)
        {
            bool result = AnagraficaManager.Save_DatiIndirizzo(null, model, out string errorMsg, out var postActions);
            return Json(new { result = result, errorMsg = errorMsg });
        }

        public ActionResult Modal_StatiRapporto(string m, int idStato)
        {
            AnagraficaModel tmp = AnagraficaManager.GetAnagrafica(m, null, new AnagraficaLoader(SezioniAnag.StatoRapporto));

            var model = tmp.DatiStatiRapporti.Eventi.FirstOrDefault(x => x.IdEvento == idStato);
            if (model == null)
            {
                var lastEvent = tmp.DatiStatiRapporti.Eventi.LastOrDefault();

                model = new EventoModel()
                {
                    IdEvento = 0,
                    IdPersona = tmp.IdPersona,
                    Matricola = tmp.Matricola,
                    DataInizio = lastEvent != null ? lastEvent.DataFine.AddDays(1) : DateTime.Today,
                    DataFine = new DateTime(2099, 12, 31),
                    MostraProposta = true
                };
            }
            model.MinDate = tmp.DataAssunzione;
            model.MaxDate = tmp.DataCessazione;// new DateTime(2099, 12, 31);

            return View("subpartial/Modal_DatiStatoRapporto", model);
        }
        public ActionResult Save_StatiRapporto(EventoModel model)
        {
            //var dataScadenza = Request.Form["DataScadenza"];
            //if (!model.DataScadenza.HasValue && !String.IsNullOrWhiteSpace(dataScadenza))
            //{
            //    model.DataScadenza = DateTime.ParseExact(dataScadenza, "dd/MM/yyyy HH:mm", null);
            //}

            if (AnagraficaManager.Save_StatiRapporto(model, out string errorMsg))
                return Content("OK");
            else
                return Content(errorMsg);
        }
        public ActionResult Delete_StatiRappporto(int idRecord, bool deleteApi)
        {
            if (AnagraficaManager.Delete_StatoRapporto(idRecord, out string errorMsg))
            {
                if (deleteApi)
                {
                    AnagraficaManager.InserisciApiAnnulla(idRecord);
                }
                return Content("OK");
            }

            else
                return Content(errorMsg);
        }

        public ActionResult Load_DatiStudio(string m, string customFunc = "")
        {
            AnagraficaModel model = AnagraficaManager.GetAnagrafica(m, null, new AnagraficaLoader(SezioniAnag.TitoliStudio), true, customFunc);
            return View("subpartial/Tab_DatiTitoliStudio", model.DatiTitoliStudio);
        }
        public ActionResult Modal_TitoloStudio(string m, string cod)
        {
            var anag = AnagraficaManager.GetAnagrafica(m, null, new AnagraficaLoader(SezioniAnag.TitoliStudio));
            StudioModel studio = anag.DatiTitoliStudio.Studi.FirstOrDefault(x => x.CodTitolo == cod);
            if (studio == null)
            {
                studio = new StudioModel();
                studio.IdPersona = anag.IdPersona;
                studio.Matricola = anag.Matricola;
            }

            return View("subpartial/Modal_TitoliStudio", studio);
        }
        public ActionResult Save_DatiStudio(StudioModel model)
        {
            if (AnagraficaManager.Save_DatiStudio(model, out string errorMsg))
                return Content("OK");
            else
                return Content(errorMsg);
        }
        public ActionResult Delete_DatiStudio(string m, string cod)
        {
            var anag = AnagraficaManager.GetAnagrafica(m, null, null);
            if (AnagraficaManager.Delete_DatiStudio(anag.IdPersona, cod, out string errorMsg))
                return Content("OK");
            else
                return Content(errorMsg);
        }

        public ActionResult Load_DatiIban(string m, bool fromModal = false, string customFunc = "")
        {
            AnagraficaModel model = AnagraficaManager.GetAnagrafica(m, null, new AnagraficaLoader(SezioniAnag.Bancari)
            {
                EnabledAdd = !fromModal,
                EnabledDelete = !fromModal,
                EnabledModify = !fromModal
            }, true, customFunc);

            return View("subpartial/Tab_DatiBancari", model.DatiBancari);
        }
        public ActionResult Modal_DatiIban(string m, int id, IbanType tipo)
        {
            AnagraficaModel model = AnagraficaManager.GetAnagrafica(m, null, new AnagraficaLoader(SezioniAnag.Bancari));

            IbanModel iban = model.DatiBancari.Ibans.FirstOrDefault(x => x.IdDatiBancari == id && x.Tipologia == tipo);
            if (iban == null)
            {
                iban = new IbanModel();
                iban.Tipologia = tipo;
                iban.IdPersona = model.IdPersona;
                iban.Matricola = model.Matricola;
            }
            iban.IbanLiberi = model.DatiBancari.IbanLiberi;

            return View("subpartial/Modal_DatiIban", iban);
        }
        public ActionResult Save_DatiIBan(IbanModel model)
        {
            if (AnagraficaManager.Save_DatiBancari(model, out string errorMsg))
                return Content("OK");
            else
                return Content(errorMsg);
        }
        public ActionResult Delete_DatiIban(string m, int id, IbanType tipo)
        {
            if (AnagraficaManager.Delete_DatiBancari(m, id, tipo, out string errorMsg))
                return Content("OK");
            else
                return Content(errorMsg);
        }
        public ActionResult Delete_ModIban(int idEv)
        {
            if (AnagraficaManager.Delete_ModificaIban(idEv, out string errorMsg))
                return Content("OK");
            else
                return Content(errorMsg);
        }
        public ActionResult Convalida_ModificaIban(string m, int id)
        {
            if (AnagraficaManager.Convalida_ModificaIban(m, id, out string errorMsg))
                return Content("OK");
            else
                return Content(errorMsg);
        }
        public ActionResult Load_StoricoIndirizzo(int idPersona, IndirizzoType tipologia, bool nuovo)
        {
            var storico = AnagraficaManager.StoricoDatiIndirizzo(idPersona, tipologia, nuovo);
            return View("subpartial/Widget_DatiIndirizzo_Storico", storico);
        }
        public ActionResult Load_StoricoIban(int idPersona, IbanType tipologia)
        {
            var storico = AnagraficaManager.StoricoDatiIban(idPersona, tipologia);
            return View("subpartial/Widget_DatiIban_Storico", storico);
        }

        public ActionResult Load_SitDebit(string m, bool fromModal = false, string customFunc = "")
        {
            AnagraficaModel model = AnagraficaManager.GetAnagrafica(m, null, new AnagraficaLoader(SezioniAnag.Debitoria)
            {
                EnabledAdd = !fromModal,
                EnabledDelete = !fromModal,
                EnabledModify = !fromModal
            }, true, customFunc);
            return View("subpartial/Tab_SituazioneDebitoria", model.DatiSituazioneDebitoria);
        }
        public ActionResult Load_DatiFamiliari(string m)
        {
            AnagraficaDatiFamiliari model = new AnagraficaDatiFamiliari();
            var db = new IncentiviEntities();
            model.RecordsCensimento = db.XR_MAT_CENSIMENTO_CF_CONGEDI_EXTRA
                .Where(x => x.MATRICOLA == m)
                .OrderBy(x => x.GRADO_PARENTELA)
                .ToList();

            model.DataCompilazioneCensimento = model.RecordsCensimento.OrderBy(x => x.DATA_ULTIMO_UPDATE)
                .Select(x => x.DATA_ULTIMO_UPDATE).FirstOrDefault();

            if (!model.RecordsCensimento.Any())
            {
                model.OrigineDati = "AIMP";
                var RowAIMP = db.XR_MAT_CENSIMENTO_IMPORT_AIMP.Where(x => x.MATRICOLA == m).ToList();
                foreach (var row in RowAIMP)
                {
                    var itemFromAIMP = new XR_MAT_CENSIMENTO_CF_CONGEDI_EXTRA()
                    {
                        CARICO = row.PERCENTUALE_CARICO,
                        CF = row.CF,
                        DATA_NASCITA = row.DATA_NASCITA,
                        GRADO_PARENTELA = row.GRADO_PARENTELA,
                        NOMINATIVO = row.NOMINATIVO,
                        DISABILE = row.HANDICAP,
                        DATA_AFFIDAMENTO = null,
                        DATA_INIZIO_CARICO = row.DATA_INIZIO_CARICO,
                    };
                    if (row.ANNO_AFFIDAMENTO != null)
                    {
                        DateTime.TryParseExact("01/01/" + row.ANNO_AFFIDAMENTO.ToString(), "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime Daff);
                        itemFromAIMP.DATA_AFFIDAMENTO = Daff;
                    }
                    model.RecordsCensimento.Add(itemFromAIMP);
                }
            }
            else
                model.OrigineDati = "CENS";

            model.DichiarazioneFB3000 =
                db.XR_MAT_CENSIMENTO_CF_ESITO_DICH
                .Where(x => x.MATRICOLA == m)
                .Select(x => x.RISPOSTA).FirstOrDefault();


            return View("subpartial/modal_datifamiliari", model);
        }
        public ActionResult Load_DatiRetribuzione(string m, bool fromModal = false, string customFunc = "")
        {
            AnagraficaModel model = AnagraficaManager.GetAnagrafica(m, null, new AnagraficaLoader(SezioniAnag.Retribuzione)
            {
                EnabledAdd = !fromModal,
                EnabledDelete = !fromModal,
                EnabledModify = !fromModal
            }, true, customFunc);
            return View("subpartial/Tab_DatiRetribuzione", model.DatiRedditi);
        }
        public ActionResult Load_DatiCedolini(string m, string codice = "", bool fromModal = false, string customFunc = "")
        {
            AnagraficaLoader loader = new AnagraficaLoader(SezioniAnag.Cedolini)
            {
                EnabledAdd = !fromModal,
                EnabledDelete = !fromModal,
                EnabledModify = !fromModal
            };
            loader.AddFiltro("CedoCodice", codice);
            AnagraficaModel model = AnagraficaManager.GetAnagrafica(m, null, loader, true, customFunc);
            return View("subpartial/Tab_DatiCedolino", model.DatiCedolini);
        }
        public ActionResult Load_DatiFormazione(string m, bool fromModal = false, string customFunc = "")
        {
            AnagraficaModel model = AnagraficaManager.GetAnagrafica(m, null, new AnagraficaLoader(SezioniAnag.Formazione)
            {
                EnabledAdd = !fromModal,
                EnabledDelete = !fromModal,
                EnabledModify = !fromModal
            }, true, customFunc);
            return View("subpartial/Tab_DatiFormazione", model.DatiFormazione);
        }
        public ActionResult Load_DatiPresenze(string m, int anno = 0, bool fromModal = false, string customFunc = "")
        {
            AnagraficaModel model = AnagraficaManager.GetAnagrafica(m, null, new AnagraficaLoader(anno, SezioniAnag.Presenze)
            {
                EnabledAdd = !fromModal,
                EnabledDelete = !fromModal,
                EnabledModify = !fromModal
            }, true, customFunc);
            return View("subpartial/Tab_DatiPresenza", model.DatiPresenze);
        }

        public ActionResult Load_DatiContrattuali(string m = "", int idPersona = 0, bool isNeoMatr = false, bool fromModal = false, string customFunc = "")
        {
            AnagraficaModel model = null;
            SezListLoadOption[] sezList =
            {
                new SezListLoadOption(SezioniAnag.Qualifiche, false),
                new SezListLoadOption(SezioniAnag.Ruoli, false),
                new SezListLoadOption(SezioniAnag.Sedi, false),
                new SezListLoadOption(SezioniAnag.Servizi,false),
                new SezListLoadOption(SezioniAnag.TipoContratti, false),
                new SezListLoadOption(SezioniAnag.Sezioni, false)
            };

            model = AnagraficaManager.GetAnagrafica(m, idPersona, new AnagraficaLoader(sezList)
            {
                EnabledAdd = !fromModal,
                EnabledDelete = !fromModal,
                EnabledModify = !fromModal
            }, true, customFunc);

            return View("subpartial/Tab_Carriera", model);
        }

        public ActionResult Load_DatiTrasferte(string m, string codice = "", bool fromModal = false, string customFunc = "")
        {
            AnagraficaLoader loader = new AnagraficaLoader(SezioniAnag.Trasferte)
            {
                EnabledAdd = !fromModal,
                EnabledDelete = !fromModal,
                EnabledModify = !fromModal
            };
            // loader.AddFiltro("CedoCodice", codice);
            AnagraficaModel model = AnagraficaManager.GetAnagrafica(m, null, loader, true, customFunc);
            return View("subpartial/ElencoTrasferte", model.DatiTrasferte);
        }

        [HttpPost]
        public PartialViewResult LoadTabDatiTrasferte(string matricola, int page = 0, int size = 0, TrasferteMacroStato macroStato = TrasferteMacroStato.Aperte, string data = "")
        {

            AnagraficaTrasferte model = new AnagraficaTrasferte();
            model = AnagraficaManager.GetTrasferte(matricola, page, size, macroStato, data);

            return PartialView("~/Views/Anagrafica/subpartial/Tab_DatiTrasferte.cshtml", model);

        }

        public ActionResult ModalDettaglioTrasferta(string id)
        {

            DettaglioTrasfertaVM model = new DettaglioTrasfertaVM();
            model = AnagraficaManager.GetDettaglioTrasferta(id);
            return PartialView("~/Views/Anagrafica/subpartial/DettaglioTrasferta.cshtml", model);

        }

        public PartialViewResult ModalBigliettiTrasferta(string foglioViaggio)
        {
            try
            {
                DettaglioTrasfertaVM vm = new DettaglioTrasfertaVM();

                vm.FoglioViaggio = TrasferteManager.GetFoglioViaggio(foglioViaggio);
                vm.FViaggio = foglioViaggio;

                if (vm.FoglioViaggio != null)
                {
                    vm.Itinerario = TrasferteManager.GetItinerario(foglioViaggio, vm.FoglioViaggio.MATRICOLA_DP);
                }

                return PartialView("~/Views/Anagrafica/subpartial/BigliettiTrasferta.cshtml", vm);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public ActionResult Load_DatiSpeseProduzione(string m, string codice = "", bool fromModal = false, string customFunc = "")
        {
            AnagraficaLoader loader = new AnagraficaLoader(SezioniAnag.SpeseProduzione)
            {
                EnabledAdd = !fromModal,
                EnabledDelete = !fromModal,
                EnabledModify = !fromModal
            };
            // loader.AddFiltro("CedoCodice", codice);
            AnagraficaModel model = AnagraficaManager.GetAnagrafica(m, null, loader, true, customFunc);
            return View("subpartial/ElencoSpeseProduzione", model.DatiSpeseProduzione);
        }
        [HttpPost]
        public PartialViewResult LoadTabDatiSpeseProduzione(string matricola, int page = 0, int size = 0, bool macroStato = true, string data = "")
        {

            AnagraficaSpeseProduzione model = new AnagraficaSpeseProduzione();
            model = AnagraficaManager.GetSpeseProduzione(matricola, page, size, macroStato, data);

            return PartialView("~/Views/Anagrafica/subpartial/Tab_DatiSpeseProduzione.cshtml", model);

        }

        public ActionResult ModalDettaglioSpeseProduzione(decimal id, bool isAperte)
        {
            List<SpeseProduzioneViewModel> viewmodel = null;
            try
            {

                SpeseDiProduzioneEntities dbContext = new SpeseDiProduzioneEntities();
                SpeseDiProduzioneServizio SpeseDiProduzioneServizio = new SpeseDiProduzioneServizio(dbContext);
                viewmodel = SpeseDiProduzioneServizio.GetDettaglioFoglioSpese(id, isAperte).ToList();
                foreach (var item in viewmodel)
                {
                    TempData["TipoTarghetta"] = item.TipoTarghetta;
                }
                ViewBag.DescrizioniAndImportiAnticipi = SpeseDiProduzioneServizio.GetDesctizioniAndImportiAnticipi(id);
                ViewBag.VociRendicontiSegreteria = SpeseDiProduzioneServizio.GetDescrizioneFromTFSP02ConTipoTaghettaSegreteria(id);
                ViewBag.VociRendicontiDipendente = SpeseDiProduzioneServizio.GetDescrizioneFromTFSP02ConTipoTaghettaDipendente(id);
                ViewBag.VociRendicontiPersonale = SpeseDiProduzioneServizio.GetDescrizioneFromTFSP02ConTipoTaghettaPersonale(id);
                ViewBag.VociRendicontiContabilita = SpeseDiProduzioneServizio.GetDescrizioneFromTFSP02ConTipoTaghettaContabilita(id);
                ViewBag.TarghettaAndImportiRendicontiSegreteria = SpeseDiProduzioneServizio.GetDescrizioniAndImportiRendicontiConTarghettaInsegreteria(id);
                ViewBag.TarghettaAndImportiRendicontiDipendente = SpeseDiProduzioneServizio.GetDescrizioniAndImportiRendicontiConTarghettaAlDipendente(id);
                ViewBag.TarghettaAndImportiRendicontiContabilita = SpeseDiProduzioneServizio.GetDescrizioniAndImportiRendicontiConTarghettaInContabilita(id);
                ViewBag.TarghettaAndImportiRendicontiPersonale = SpeseDiProduzioneServizio.GetDescrizioniAndImportiRendicontiConTarghettaAlPersonale(id);
            }
            catch (Exception ex)
            {
                viewmodel = new List<SpeseProduzioneViewModel>();

            }
            return PartialView("subpartial/DettaglioSpesaProduzione", viewmodel);

        }
        public ActionResult VisualizzaVoce(decimal id_FoglioSpese, short progressivo)
        {
            SpeseProduzioneVoce voce = new SpeseProduzioneVoce();
            SpeseDiProduzioneEntities dbContext = new SpeseDiProduzioneEntities();

            string statoFoglio = dbContext.TFoglio_Spese.FirstOrDefault(x => x.ID == id_FoglioSpese).Stato;

            var val = from cambi in dbContext.TCambioValuta
                      join tfsp in dbContext.TFSP02 on cambi.Valuta equals tfsp.Codice.TrimEnd()
                      where tfsp.NomeTab == "2008" && cambi.ID == id_FoglioSpese
                      select new { cambi, tfsp.Descrizione };
            List<ValutaSpese> valute = val.Select(x => new ValutaSpese()
            {
                Valuta = x.cambi.Valuta.TrimEnd(),
                cambioMedio = x.cambi.Cambio_Medio,
                descrizione = x.cambi.Valuta.TrimEnd() + " - " + x.Descrizione
            }).ToList();

            valute.Add(dbContext.TFSP02.Where(x => x.NomeTab == "2008" && x.Codice == "EUR").Select(x => new ValutaSpese()
            {
                Valuta = x.Codice.TrimEnd(),
                cambioMedio = 1,
                descrizione = x.Descrizione
            }).FirstOrDefault());



            try
            {
                voce = dbContext.TRendiconto_Voci.Where(x => x.ID == id_FoglioSpese && x.ProgressivoVoce == progressivo).Select(x => new SpeseProduzioneVoce()
                {
                    CambioVoce = x.Cambio_Applicato,
                    SelectedCentro = x.CdC,
                    ConCarta = (statoFoglio == "OC" || statoFoglio == "MM") ? true : false,
                    DataSpesa = x.Data_Prima_Attivazione,
                    Id = x.ID,
                    Importo = x.Importo,
                    ProgressivoVoce = x.ProgressivoVoce,
                    SelectedTipologia = x.Tipo_Voce,
                    SelectedValuta = x.Valuta,
                    SelectedVoce = x.Voce,
                    TipoTarghetta = x.Tipo,
                    ValoreEuro = x.Valore_in_Euro,
                    DescVoce = dbContext.TFSP02.FirstOrDefault(y => y.NomeTab == "2005" && y.Codice == x.Voce).Descrizione
                }).FirstOrDefault();
                voce.Valute = valute;
                var files = myRaiCommonTasks.Helpers.FileManager.GetFileByChiave(voce.Id.ToString() + "_" + voce.ProgressivoVoce.ToString()).Files;
                if (files.Count() > 0)
                {
                    voce.IdFile = files[0].Id;
                    voce.NomeFile = files[0].NomeFile;
                    voce.SizeFile = files[0].Length;
                }
            }
            catch (Exception e)
            {
                var bo = e;
            }


            return PartialView("subpartial/VisualizzaVoceSpesa", voce);
        }
        public ActionResult GetDoc(int idFile)
        {
            var file = myRaiCommonTasks.Helpers.FileManager.GetFile(idFile).Files[0];
            if (file == null)
                return View("~/Views/Shared/404.cshtml");
            else
                return new FileStreamResult(new System.IO.MemoryStream(file.ContentByte), file.MimeType);

        }

        [HttpPost]
        public ActionResult Modal_VariazioniContratti(string m, string view = "", string customFunc = "")
        {
            AnagraficaModel model = null;
            SezListLoadOption[] sezList =
            {
                new SezListLoadOption(SezioniAnag.Qualifiche, false),
                new SezListLoadOption(SezioniAnag.Ruoli, false),
                new SezListLoadOption(SezioniAnag.Sedi, false),
                new SezListLoadOption(SezioniAnag.Servizi,false),
                new SezListLoadOption(SezioniAnag.TipoContratti, false),
                new SezListLoadOption(SezioniAnag.Sezioni, false)
            };

            model = AnagraficaManager.GetAnagrafica(m, 0, new AnagraficaLoader(sezList), true, customFunc);

            if (String.IsNullOrWhiteSpace(view))
            {
                view = "subpartial/Modal_VariazioniContratti";
            }

            return View(view, model);
        }
        [HttpPost]
        public ActionResult Tab_VariazioniContratti(string m, string dataInizio, string dataFine, string view, string customFunc = "")
        {
            DateTime inizio = dataInizio.ToDateTime("yyyyMMdd");
            DateTime fine = dataFine.ToDateTime("yyyyMMdd");

            AnagraficaModel model = null;
            SezListLoadOption[] sezList =
            {
                new SezListLoadOption(SezioniAnag.Qualifiche, false),
                new SezListLoadOption(SezioniAnag.Ruoli, false),
                new SezListLoadOption(SezioniAnag.Sedi, false),
                new SezListLoadOption(SezioniAnag.Servizi,false),
                new SezListLoadOption(SezioniAnag.TipoContratti, false),
                new SezListLoadOption(SezioniAnag.Sezioni, false)
            };

            var loader = new AnagraficaLoader(sezList);
            loader.CarrieraInizio = inizio;
            loader.CarrieraFine = fine;
            model = AnagraficaManager.GetAnagrafica(m, 0, loader, true, customFunc);

            return View(view, model);
        }

        public ActionResult Modal_DatiContr(int idPersona, TipoEvento tipo)
        {
            return View("~/Views/Anagrafica/subpartial/Modal_DatiContrattuali.cshtml", AnagraficaManager.GetDatiContrattuali(idPersona, tipo));
        }
        public ActionResult Save_RichDatiContrattuali(EventoModel model)
        {
            if (AnagraficaManager.Insert_RichDatiContr(model, out string errorMsg))
                return Content("OK");
            else
                return Content(errorMsg);
        }
        public ActionResult Save_DatiContrattuali(EventoModel model, bool approva)
        {
            if (AnagraficaManager.Save_DatiContrattuali(model, approva, out string errorMsg))
                return Content("OK");
            else
                return Content(errorMsg);
        }

        public ActionResult GetCittadinanza(string filter, string value)
        {
            return Json(AnagraficaManager.GetCittadinanza(filter, value), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetComuni(string filter, string value)
        {
            return Json(AnagraficaManager.GetComuni(filter, value), JsonRequestBehavior.AllowGet);
        }
        public static List<SelectListItem> GetStatiCivile()
        {
            return AnagraficaManager.GetStatiCivile();
        }
        public static List<SelectListItem> GetStatiRapporto()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem(){Value="SW", Text="Smart working", Selected=true}
            };
        }
        public static List<SelectListItem> GetTipologieIban(List<IbanType> list)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            result.Add(new SelectListItem() { Value = "", Text = "Seleziona un valore" });
            foreach (var item in list)
                result.Add(new SelectListItem() { Value = item.ToString(), Text = item.GetAmbientValue() });

            return result;
        }
        public static List<SelectListItem> GetTipiStudi()
        {
            var list = AnagraficaManager.GetTipiStudi();

            List<SelectListItem> result = new List<SelectListItem>();
            result.Add(new SelectListItem() { Value = "", Text = "Seleziona un valore" });
            foreach (var item in list)
                result.Add(new SelectListItem() { Value = item.COD_LIVELLOSTUDIO.ToString(), Text = item.DES_LIVELLOSTUDIO });

            return result;
        }

        public ActionResult GetStudi(int codTipo)
        {
            var list = AnagraficaManager.GetStudi(codTipo);

            List<SelectListItem> result = new List<SelectListItem>();
            foreach (var item in list)
                result.Add(new SelectListItem() { Value = item.COD_STUDIO.ToString(), Text = item.DES_STUDIO });

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        public static List<SelectListItem> GetScaleVoti()
        {
            var list = AnagraficaManager.GetScaleVoti();

            List<SelectListItem> result = new List<SelectListItem>();
            result.Add(new SelectListItem() { Value = "", Text = "Seleziona un valore" });
            foreach (var item in list)
                result.Add(new SelectListItem() { Value = item.COD_TIPOPUNTEGGIO.ToString(), Text = item.DES_TIPOPUNTEGGIO });

            return result;
        }
        public static List<SelectListItem> GetAtenei()
        {
            var list = AnagraficaManager.GetAtenei();

            List<SelectListItem> result = new List<SelectListItem>();
            result.Add(new SelectListItem() { Value = "", Text = "Seleziona un valore" });
            foreach (var item in list)
                result.Add(new SelectListItem() { Value = item.COD_ATENEO.ToString(), Text = item.DES_ATENEO });

            return result;
        }
        public ActionResult GetAnagBanca(string iban)
        {
            var db = AnagraficaManager.GetDb();
            if (iban.Length == 27)
            {
                var codPaese = iban.Substring(0, 2);
                var codChk = iban.Substring(2, 2);
                var codCin = iban.Substring(4, 1);
                var codAbi = iban.Substring(5, 5);
                var codCab = iban.Substring(10, 5);
                var codCC = iban.Substring(15);

                XR_ANAGBANCA anagBanca = db.XR_ANAGBANCA.Include("TB_COMUNE").FirstOrDefault(x => x.COD_ABI == codAbi && x.COD_CAB == codCab);

                if (anagBanca != null)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "ok", agenzia = anagBanca.DES_RAG_SOCIALE.Trim(), indirizzo = anagBanca.DES_INDIRIZZO.Trim(), citta = anagBanca.TB_COMUNE != null ? anagBanca.TB_COMUNE.DES_CITTA : "" }
                    };
                }
                else
                {
                    HrisHelper.AddSegnalazione("Informazioni mancanti", "HRIS: Decodifica informazioni IBAN", String.Format("ABI:{0} - CAB {1}: anagrafica banca non trovata", codAbi, codCab));

                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { result = "No" }
                    };
                }
            }
            else

            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { result = "No" }
                };
            }
        }

        public ActionResult GetSedi(string filter, string value)
        {
            return Json(AnagraficaManager.GetSedi(filter, value), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetServizi(string filter, string value)
        {
            return Json(AnagraficaManager.GetServizi(filter, value), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetSezioni(string filter, string value, string codiceRif = "")
        {
            return Json(AnagraficaManager.GetSezioni(filter, value, codiceRif), JsonRequestBehavior.AllowGet);
        }
        public static List<SelectListItem> GetEventi(TipoEvento tipo)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            result.AddRange(AnagraficaManager.GetEventi(tipo));
            return result;
        }

        public ActionResult CambiaCodiceCont(string matricola, string codice)
        {
            //NC - Questa è una soluzione momentanea finchè non viene implementata
            //la tabella dei codici contabili
            var db = new myRaiData.digiGappEntities();
            var newCode = new myRaiData.MyRai_CacheFunzioni()
            {
                data_creazione = DateTime.Now,
                oggetto = matricola,
                funzione = "AssicurazioneInfortuni",
                dati_serial = codice
            };
            db.MyRai_CacheFunzioni.Add(newCode);

            if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                return Content("OK");
            else
                return Content("Errore durante il salvataggio");
        }

        #region Contenzioso
        public ActionResult Load_Contenzioso(string m, bool fromModal = false, string customFunc = "")
        {
            AnagraficaModel model = AnagraficaManager.GetAnagrafica(m, null, new AnagraficaLoader(SezioniAnag.Contenzioso)
            {
                EnabledAdd = !fromModal,
                EnabledDelete = !fromModal,
                EnabledModify = !fromModal
            }, true, customFunc);
            return View("subpartial/Tab_DatiContenzioso", model);
        }
        #endregion

        #region Dematerializzazione
        //public ActionResult Modal_RichiestaDematerializzazione ( string m, int id )
        //{
        //    RichiestaAnag richiesta = new RichiestaAnag( );
        //    richiesta.IdPersona = id;
        //    richiesta.Matricola = m;

        //    AnagraficaModel model = null;
        //    SezioniAnag sezione = SezioniAnag.NonDefinito;
        //    model = AnagraficaManager.GetAnagrafica( m , new AnagraficaLoader( sezione ) );

        //    if (model != null )
        //    {
        //        richiesta.IdPersona = model.IdPersona;
        //        richiesta.Matricola = model.Matricola;
        //    }

        //    return View( "subpartial/Modal_RichiestaDichiarazione" , richiesta );
        //}

        //public ActionResult Modal_InserimentoDocDem ( string m , int id )
        //{
        //    InsRicModel richiesta = new InsRicModel( );
        //    richiesta.IdPersona = id;
        //    richiesta.Matricola = m;

        //    AnagraficaModel model = null;
        //    SezioniAnag sezione = SezioniAnag.NonDefinito;
        //    model = AnagraficaManager.GetAnagrafica( m , new AnagraficaLoader( sezione ) );

        //    if ( model != null )
        //    {
        //        richiesta.IdPersona = model.IdPersona;
        //        richiesta.Matricola = model.Matricola;
        //    }
        //    string matricola = CommonHelper.GetCurrentUserMatricola( );
        //    SessionHelper.Set( matricola + "tipologiaDocumentale" , "" );
        //    SessionHelper.Set( matricola + "tipologiaDocumento" , "" );

        //    return View( "subpartial/Modal_InserimentoDocDem" , richiesta );
        //}

        public ActionResult Load_DematerializzazioneMieiDocumenti(string m, bool fromModal = false, string customFunc = "")
        {
            AnagraficaModel model = AnagraficaManager.GetAnagrafica(m, null, new AnagraficaLoader(SezioniAnag.MieiDoc)
            {
                EnabledAdd = !fromModal,
                EnabledDelete = !fromModal,
                EnabledModify = !fromModal
            }, true, customFunc);
            model.Matricola = m;

            return View("subpartial/DEM_Elenco_MieiDocumenti", model.DematerializzazioneMieiDocumenti);
        }

        #endregion

        #region Documenti personali e amministrativi

        public ActionResult Load_Documenti(string m, bool fromModal = false, string customFunc = "")
        {
            AnagraficaModel model = AnagraficaManager.GetAnagrafica(m, null, new AnagraficaLoader(SezioniAnag.Documenti), false, customFunc);
            ViewBag.listaTipiDocumento = model.DatiDocumenti.TipiDocumentiAmministrativi;
            ViewBag.listaTipiDocumentoPersonali = model.DatiDocumenti.TipiDocumentiPersonali;
            ViewBag.listaAnni = model.DatiDocumenti.AnniDocumentiAmministrativi;
            ViewBag.documentiAmministrativi = model.DatiDocumenti.AbilitatoPerDocumentiAmministrativi;
            ViewBag.documentiPersonali = model.DatiDocumenti.AbilitatoPerDocumentiPersonali;
            return View("subpartial/Tab_DocumentiAmministrativi");
        }

        public ActionResult Load_ElencoDocumentiAmministrativi(string m, string codice, string anno, bool fromModal = false, string customFunc = "")
        {
            var listaDoc = AnagraficaManager.GetDocumentiAmministrativi(m, codice, anno);
            ViewBag.matricola = m;
            return View("subpartial/_ElencoDocumentiAmministrativi", listaDoc);
        }

        public ActionResult ScaricaDocumentoPersonale(string id, string matricola)
        {
            var doc = AnagraficaManager.GetDocumentoPersonale(id, matricola);
            //return File(doc.FileDoc, "application/pdf", string.Format("Documento_{0:ddMMMyyyy}.pdf", DateTime.Now));
            byte[] byteArray = doc.FileDoc;
            string nomefile = string.Format("Documento_{0:ddMMMyyyy}.pdf", DateTime.Now);
            MemoryStream pdfStream = new MemoryStream();
            pdfStream.Write(byteArray, 0, byteArray.Length);
            pdfStream.Position = 0;
            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "documento",
                Inline = true,
            };
            Response.AddHeader("Content-Disposition", "inline; filename=" + nomefile);
            return File(byteArray, "application/pdf");
        }

        public ActionResult Load_ElencoDocumentiPersonali(string m, string codice, bool fromModal = false, string customFunc = "")
        {
            List<ItemDocumentoPersonale> listaDoc = AnagraficaManager.GetDocumentiPersonali(m, codice);
            ViewBag.matricola = m;
            string urlDoc = "#";
            //string urlDoc = "http://svilhrdoc/HrDoc_ClickOnce/HrDoc_ScannerPdf.application?frm=6&matricola={0}&progressivo={1}&datadoc={2}&tipologia={3}&collocazione={4}&protocollo={5}&parolachiave=&testo=&PagTot={6}%A0&operazione=AO&tipofile=False&stampa=true&SoloEliminati=false&NomeDataBase=HRDOC&ID_Pag_Img=0";
            var parametroUrlDoc = CommonHelper.GetParametri<string>(EnumParametriSistema.URLClickOnceDownloadDocumentoPersonale);
            if (parametroUrlDoc != null && parametroUrlDoc.Any())
                urlDoc = parametroUrlDoc[0];
            ViewBag.urlDoc = urlDoc;
            return View("subpartial/_ElencoDocumentiPersonali", listaDoc);
        }

        private string primaMaiuscola(string s)
        {
            if (s.Length > 0)
            {
                string s1 = s.Substring(0, 1).ToUpper();
                if (s.Length > 1)
                    s = s1 + s.Substring(1);
                else
                    s = s1;
            }
            return s;
        }

        public ActionResult GetPdf(string matricola, string idPdf, string datacompetenza, string datacontabile, string datapubblicazione, string nota, string titolo, string nomefile)
        {
            string dataContabile = "";
            string dataCompetenza = "";
            string dataPubblicazione = "";
            string[] format = { "yyyyMMdd" };
            DateTime dataConv;
            if (DateTime.TryParseExact(datacontabile.Substring(0, 6) + "01", format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dataConv))
                dataContabile = primaMaiuscola(dataConv.ToString("MMMM yyyy"));
            if (DateTime.TryParseExact(datacompetenza.Substring(0, 6) + "01", format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dataConv))
                dataCompetenza = primaMaiuscola(dataConv.ToString("MMMM yyyy"));
            if (DateTime.TryParseExact(datapubblicazione, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out dataConv))
                dataPubblicazione = dataConv.ToString("dd/MM/yyyy");
            myRaiCommonModel.DocFirmaModels.PDFmodel model = new myRaiCommonModel.DocFirmaModels.PDFmodel()
            {
                idDocumento = idPdf,
                titolo = titolo,
                datacompetenza = dataCompetenza,
                datacontabile = dataContabile,
                datapubblicazione = dataPubblicazione,
                nota = nota,
                nomefile = nomefile
            };
            ViewBag.matricola = matricola;
            return View("~/Views/Anagrafica/subpartial/_pdfviewer.cshtml", model);
        }


        #endregion

        public ActionResult CVPdf(string matricola = null)
        {
            var result = CV_OnlineManager.GetCVPdf(Server.MapPath("~"), Server.MapPath("~/assets/fontG/open-sans-v13-latin-300.ttf"), matricola, null, out int error);
            if (error != 0)
            {
                switch (error)
                {
                    case 403:
                        return View("~/Views/Shared/NonAbilitatoError2.cshtml");
                    default:
                        break;
                }
            }
            return new FileStreamResult(result, "application/pdf");
        }

    }
}
