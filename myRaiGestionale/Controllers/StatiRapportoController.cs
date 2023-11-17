using ClosedXML.Excel;
using myRai.DataAccess;
using myRaiCommonManager;
using myRaiCommonManager.Model.Smartworking;
using myRaiCommonModel;
using myRaiCommonTasks;
using myRaiData.Incentivi;
using myRaiDataTalentia;
using myRaiHelper;
using myRaiHelper.InfoComunicazione;
using myRaiHelper.RicercaComunicazione;
using myRaiHelper.Task;
using Org.BouncyCastle.Asn1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace myRaiGestionale.Controllers
{
    public class AnagEcc
    {
        public string matricola { get; set; }
        public DateTime data { get; set; }
        public string cod_eccez_padre { get; set; }
        public string desc_cod_eccez_padre { get; set; }
        public string cod_eccezione { get; set; }
        public string desc_eccezione { get; set; }
        public string unita_misura { get; set; }
        public decimal? quantita_numero { get; set; }
        public decimal? quantita_ore { get; set; }
        public string tipo_giorno { get; set; }

        public string cod_orario { get; set; }
        public string desc_orario { get; set; }
        public int prevista_presenza { get; set; }
    }

    public class RicercaStati : CercaDipendenteModel
    {
        public bool SoloAttivi { get; set; }
        public bool IncludiAccordiNonSottoscritti { get; set; }
        public string Stato { get; set; }
        public string RichiestaGiorniAggiuntivi { get; set; }
        public string[] CatRichieste { get; set; }
        public bool IncludiDettagli { get; set; }
        public bool OrderByErrore { get; set; }
    }


    public class ResultStati
    {
        public int Showed { get; set; }
        public int TotNumber { get; set; }
        public List<AnagraficaModel> List { get; set; }
    }

    public class StatiRapportoController : BaseCommonController
    {
        public StatiRapportoController()
        {

        }
        public static string GetSWAbilFunc()
        {
            string func = HrisHelper.GetParametro<string>(HrisParam.SWAbilFunc);
            if (String.IsNullOrWhiteSpace(func))
                func = "HRIS_SW";

            return func;
        }

        public ActionResult Test()
        {
            //AnagraficaManager.UpdateSWRequest(out var output, out var errore);
            //return new FileStreamResult(SmartworkingManager.CreaZip(), "application/zip");
            return Content("OK");
        }
        public ActionResult Index(bool? errorTop)
        {
            bool orderByErrore = false;
            if (errorTop.HasValue && errorTop.Value)
            {
                orderByErrore = true;
            }
            return View(new RicercaStati() { SoloAttivi = true, IncludiAccordiNonSottoscritti = false, OrderByErrore = orderByErrore });
        }

        public ActionResult RichiediEsecuzioneClic()
        {
            string errore = "";
            if (TaskHelper.AddBatchRunnerTask("Smartworking", out errore))
                return Content("OK");
            else
                return Content(errore);
        }

        public ActionResult RicercaDipendente(RicercaStati model, int skip = 0, int take = 10)
        {
            var result = CercaDipendenti(model, true, skip, take);

            return View("Elenco_Dipendenti", result);
        }


        private ResultStati CercaDipendenti(RicercaStati ricerca, bool conRecord = true, int skip = 0, int take = 10)
        {
            ResultStati model = new ResultStati();
            model.List = new List<AnagraficaModel>();

            var db = new TalentiaEntities();
            var dbCez = new IncentiviEntities();

            //se si vuole considerare solo le persone con record attivi
            //var tmp = db.XR_STATO_RAPPORTO
            //            .Include("SINTESI1")
            //            .Where(x => true);
            var tmp = db.SINTESI1
                        .Include("XR_STATO_RAPPORTO").Include("XR_STATO_RAPPORTO.XR_STATO_RAPPORTO_INFO")
                        .Include("QUALIFICA").Include("QUALIFICA.TB_QUALSTD")
                        .AsQueryable();

            tmp = AuthHelper.SintesiFilter(tmp, CommonHelper.GetCurrentUserMatricola(), "", StatiRapportoController.GetSWAbilFunc());

            if (!String.IsNullOrWhiteSpace(ricerca.Matricola))
                tmp = tmp.Where(x => ricerca.Matricola.Contains(x.COD_MATLIBROMAT));

            if (!String.IsNullOrWhiteSpace(ricerca.NominativoDipendente))
                tmp = tmp.Where(x => (x.DES_NOMEPERS + " " + x.DES_COGNOMEPERS).ToUpper().Contains(ricerca.NominativoDipendente.ToUpper())
                                || (x.DES_COGNOMEPERS + " " + x.DES_NOMEPERS).ToUpper().Contains(ricerca.NominativoDipendente.ToUpper()));

            //tmp = tmp.Where(x => !x.XR_STATO_RAPPORTO.Any() || x.XR_STATO_RAPPORTO.Any(y => y.VALID_DTA_END == null));


            Expression<Func<XR_STATO_RAPPORTO, bool>> filterStato = null;

            if (!String.IsNullOrWhiteSpace(ricerca.Codice))
                //tmp = tmp.Where(x => x.XR_STATO_RAPPORTO.Any(y => y.COD_STATO_RAPPORTO == ricerca.Codice && y.VALID_DTA_END==null));
                filterStato = LinqHelper.PutInAndTogether(filterStato, y => y.COD_STATO_RAPPORTO == ricerca.Codice && y.VALID_DTA_END == null);

            if (!String.IsNullOrWhiteSpace(ricerca.TipoAccordo))
                //tmp = tmp.Where(x => x.XR_STATO_RAPPORTO.Any(y => y.COD_TIPO_ACCORDO == ricerca.TipoAccordo && y.VALID_DTA_END == null));
                filterStato = LinqHelper.PutInAndTogether(filterStato, y => y.COD_TIPO_ACCORDO == ricerca.TipoAccordo && y.VALID_DTA_END == null);

            if (ricerca.SoloAttivi)
            {
                var listStati = new List<string>() { "SW", "SW_P" };
                if (ricerca.IncludiAccordiNonSottoscritti)
                {
                    listStati.Add("SW_N");
                    listStati.Add("SW_R");
                }

                filterStato = LinqHelper.PutInAndTogether(filterStato, x => listStati.Contains(x.COD_STATO_RAPPORTO) && x.VALID_DTA_END == null && x.DTA_INIZIO <= DateTime.Today && x.DTA_FINE >= DateTime.Today
                                                                            || (x.COD_STATO_RAPPORTO == "SW_P" && (x.DTA_INIZIO_VISUALIZZAZIONE == null || x.DTA_INIZIO_VISUALIZZAZIONE < DateTime.Now) && x.DTA_SCADENZA >= DateTime.Now));
            }

            if (filterStato != null)
                tmp = tmp.Where(x => x.XR_STATO_RAPPORTO.Any() && x.XR_STATO_RAPPORTO.AsQueryable().Any(filterStato));

            tmp = tmp.Where(x => x.ID_COMPREL != null);

            //switch (ricerca.RichiestaGiorniAggiuntivi)
            //{
            //    case "SI":
            //        tmp = tmp.Where(x => listMatrRich.Contains(x.COD_MATLIBROMAT));
            //        break;
            //    case "NO":
            //        tmp = tmp.Where(x => !listMatrRich.Contains(x.COD_MATLIBROMAT));
            //        break;
            //    default:
            //        break;
            //}
            var statiSw = AnagraficaManager.RichiesteStatiSW();
            var tmpListRich = dbCez.XR_MAT_RICHIESTE.Include("XR_MAT_CATEGORIE").Include("XR_WKF_OPERSTATI")
                .Where(x => x.ECCEZIONE == "SW")
                .Where(x => statiSw.Contains(x.XR_WKF_OPERSTATI.Where(y => y.ID_GESTIONE == x.ID && y.COD_TIPO_PRATICA == "SW").Max(y => y.ID_STATO)));

            if (ricerca.CatRichieste != null && ricerca.CatRichieste.Any())
            {
                var intAry = ricerca.CatRichieste.Select(x => Convert.ToInt32(x));
                var listMatrRich = tmpListRich.Where(x => intAry.Contains(x.CATEGORIA)).Select(x => x.MATRICOLA).ToArray();

                if (intAry.Contains(-1))
                {
                    var listAll = tmpListRich.Select(x => x.MATRICOLA).ToArray();
                    tmp = tmp.Where(x => !listAll.Contains(x.COD_MATLIBROMAT) || listMatrRich.Contains(x.COD_MATLIBROMAT));
                }
                else if (intAry.Contains(0))
                {
                    var listAll = tmpListRich.Select(x => x.MATRICOLA).ToArray();
                    tmp = tmp.Where(x => listAll.Contains(x.COD_MATLIBROMAT));
                }
                else
                {
                    tmp = tmp.Where(x => listMatrRich.Contains(x.COD_MATLIBROMAT));
                }
            }

            tmp = tmp.OrderBy(x => x.COD_MATLIBROMAT);

            model.TotNumber = tmp.Count();

            if (skip > -1 && take > -1)
                tmp = tmp.Skip(skip).Take(take);

            var result = tmp.ToList();

            List<XR_MOD_DIPENDENTI> listModuli = null;
            var listMatr = result.Select(x => x.COD_MATLIBROMAT).ToList();
            listModuli = db.XR_MOD_DIPENDENTI.Where(x => x.COD_MODULO == "SMARTW2020" && listMatr.Contains(x.MATRICOLA))
            .Select(x => new
            {
                x.COD_MODULO,
                x.DATA_COMPILAZIONE,
                x.DATA_LETTURA,
                x.MATRICOLA,
                x.ID_PERSONA,
                x.SCELTA,
            })
            .ToList()
            .Select(x => new XR_MOD_DIPENDENTI()
            {
                COD_MODULO = x.COD_MODULO,
                DATA_COMPILAZIONE = x.DATA_COMPILAZIONE,
                DATA_LETTURA = x.DATA_LETTURA,
                MATRICOLA = x.MATRICOLA,
                ID_PERSONA = x.ID_PERSONA,
                SCELTA = x.SCELTA
            }).ToList();

            List<XR_MAT_RICHIESTE> listRich = tmpListRich.Where(x => listMatr.Contains(x.MATRICOLA)).ToList();

            foreach (var item in result)
            {
                SintesiModel sintModel = null;

                AnagraficaModel anag = new AnagraficaModel()
                {
                    IdPersona = item.ID_PERSONA,
                    Matricola = item.COD_MATLIBROMAT
                };

                sintModel = new SintesiModel(item);
                anag.Matricola = item.COD_MATLIBROMAT;
                anag.Nome = item.DES_NOMEPERS;
                anag.Cognome = item.DES_COGNOMEPERS;// + (!String.IsNullOrWhiteSpace(item.DES_SECCOGNOME) ? " " + item.DES_SECCOGNOME : "");
                anag.DataAssunzione = item.DTA_INIZIO_CR.GetValueOrDefault();
                anag.DataCessazione = item.DTA_FINE_CR.GetValueOrDefault();
                anag.Sede = CezanneHelper.GetDes(item.COD_SEDE, item.DES_SEDE);
                anag.Servizio = CezanneHelper.GetDes(item.COD_SERVIZIO, item.DES_SERVIZIO);
                anag.Sezione = CezanneHelper.GetDes(item.COD_UNITAORG, item.DES_DENOMUNITAORG);
                anag.TipoContratto = CezanneHelper.GetDes(item.COD_TPCNTR, item.DES_TPCNTR);
                anag.CodSoggettoCr = item.COD_SOGGETTOCR;
                if (item.QUALIFICA != null && item.QUALIFICA.TB_QUALSTD != null)
                    anag.FiguraProfessionale = item.QUALIFICA.TB_QUALSTD.DES_QUALSTD.UpperFirst();

                AnagraficaManager.CaricaDatiStatoRapporto(anag, db, sintModel, item.XR_STATO_RAPPORTO, listModuli, listRich, ricerca.OrderByErrore);

                model.List.Add(anag);
            }

            model.Showed = skip + model.List.Count();

            return model;
        }

        public ActionResult Esporta(RicercaStati ricerca)
        {
            var wb = new XLWorkbook();
   

            var res = CercaDipendenti(ricerca, true, -1, -1);
    

            // Creazione dell'array che conterrà i valori distinti del campo Servizio
            var serviziDistinct = res.List.Select(r => r.Servizio).Distinct().ToArray();

            // Creazione dell'array finale che include la stringa "Totale" e i valori distinti del campo Servizio
            string[] totaliServizi = new string[serviziDistinct.Length +1];
            totaliServizi[0] = "Totale";
            Array.Copy(serviziDistinct, 0, totaliServizi, 1, serviziDistinct.Length);

            foreach (var servizio in totaliServizi)
            {
                var ws = wb.AddWorksheet(servizio.Replace("'", "").Replace(@"\","").Replace(".", "").Replace("/", ""));
                ws.Cell(1, 1).SetValue("Matricola");
                ws.Cell(1, 2).SetValue("Cognome");
                ws.Cell(1, 3).SetValue("Nome");
                ws.Cell(1, 4).SetValue("Data assunzione");
                ws.Cell(1, 5).SetValue("Data cessazione");
                ws.Cell(1, 6).SetValue("Sede");
                ws.Cell(1, 7).SetValue("Servizio");
                ws.Cell(1, 8).SetValue("Sezione");
                ws.Cell(1, 9).SetValue("Figura professionale");
                ws.Cell(1, 10).SetValue("Profilo professionale");
                ws.Cell(1, 11).SetValue("Tipo accordo");
                ws.Cell(1, 12).SetValue("Stato");
                ws.Cell(1, 13).SetValue("Inizio");
                ws.Cell(1, 14).SetValue("Fine");
                int offset = 0;
                if (ricerca.IncludiDettagli)
                {
                    ws.Cell(1, 15).SetValue("Inizio Periodo");
                    ws.Cell(1, 16).SetValue("Fine Periodo");
                    offset = 2;
                }
                ws.Cell(1, 15 + offset).SetValue("Avente Diritto");
                ws.Cell(1, 16 + offset).SetValue("Giorni mensili");
                ws.Cell(1, 17 + offset).SetValue("Giorni extra");
                ws.Cell(1, 18 + offset).SetValue("Richiesta giorni aggiuntivi");
                var dbTal = new TalentiaEntities();
                var dbSt = new myRaiData.Incentivi.IncentiviEntities();
                var matricole = res.List.Select(z => z.Matricola).ToList();
                var ListSintesi = dbSt.SINTESI1.Where(x => matricole.Contains(x.COD_MATLIBROMAT) &&
                x.DTA_FINE_CR > DateTime.Now).ToList();

                var listWkfStati = dbSt.XR_MAT_STATI.ToList();

                int row = 2;
                ResultStati newRes = new ResultStati();
                newRes.List = res.List;
                if (servizio != "Totale")
                {
                    newRes.List = res.List.Where(r => r.Servizio == servizio).ToList();

                }

                foreach (var item in newRes.List)
                {


                    ws.Cell(row, 1).SetValue(item.Matricola);
                    ws.Cell(row, 2).SetValue(item.Cognome);
                    ws.Cell(row, 3).SetValue(item.Nome);
                    ws.Cell(row, 4).SetValue(item.DataAssunzione);
                    ws.Cell(row, 5).SetValue(item.DataCessazione);
                    ws.Cell(row, 6).SetValue(item.Sede);
                    ws.Cell(row, 7).SetValue(item.Servizio);
                    ws.Cell(row, 8).SetValue(item.Sezione);


                    var sint = ListSintesi.FirstOrDefault(x => x.COD_MATLIBROMAT == item.Matricola);
                    //string ruolo = ListSintesi.Where(x => x.COD_MATLIBROMAT == item.Matricola).Select(z => z.DES_RUOLO).FirstOrDefault();
                    //if (!String.IsNullOrWhiteSpace(ruolo) && ruolo.Contains('-')) ruolo = ruolo.Split('-')[1].Trim();
                    ws.Cell(row, 9).SetValue(item.FiguraProfessionale);
                    if (sint != null)
                        ws.Cell(row, 10).SetValue(CezanneHelper.GetDes(sint.COD_RUOLO, sint.DES_RUOLO));

                    bool isActive = true;
                    var tmp = item.DatiStatiRapporti.Eventi.Where(x => x.DataInizio <= DateTime.Today && x.DataFine > DateTime.Today && !x.ValiditaFine.HasValue);
                    if (!String.IsNullOrWhiteSpace(ricerca.Codice))
                        tmp = tmp.Where(y => y.Codice == ricerca.Codice);

                    if (!String.IsNullOrWhiteSpace(ricerca.TipoAccordo))
                        tmp = tmp.Where(x => x.TipologiaAccordo == ricerca.TipoAccordo);

                    var current = tmp.FirstOrDefault();
                    if (current == null)
                    {
                        isActive = false;
                        var tmp2 = item.DatiStatiRapporti.Eventi.AsEnumerable();
                        if (!String.IsNullOrWhiteSpace(ricerca.Codice))
                            tmp2 = tmp2.Where(y => y.Codice == ricerca.Codice);

                        if (!String.IsNullOrWhiteSpace(ricerca.TipoAccordo))
                            tmp2 = tmp2.Where(x => x.TipologiaAccordo == ricerca.TipoAccordo);

                        current = tmp2.OrderByDescending(x => x.DataInizio).FirstOrDefault();
                    }

                    if (current != null)
                    {
                        if (!String.IsNullOrWhiteSpace(current.TipologiaAccordo))
                        {
                            ws.Cell(row, 11).SetValue(current.TipologiaAccordo);
                        }
                        ws.Cell(row, 12).SetValue(current.Descrizione);
                        ws.Cell(row, 13).SetValue(current.DataInizio);
                        ws.Cell(row, 14).SetValue(current.DataFine);
                    }

                    if (item.DatiStatiRapporti.IsAventeDiritto)
                    {
                        ws.Cell(row, 15 + offset).SetValue(item.DatiStatiRapporti.AventeDirittoSelezione.FirstOrDefault().Selezione.GetDescription());

                    }


                    if (current.Info != null && current.Info.Any())
                    {
                        var currentInfo = current.Info.FirstOrDefault(x => x.DataInizio <= DateTime.Today && x.DataFine > DateTime.Today);
                        if (currentInfo != null)
                        {
                            ws.Cell(row, 16).SetValue(currentInfo.NumeroGiorniMax);
                            ws.Cell(row, 17).SetValue(currentInfo.NumeroGiorniExtra);
                        }
                    }

                    if (item.DatiStatiRapporti.Richieste != null && item.DatiStatiRapporti.Richieste.Any())
                    {
                        string content = "";
                        foreach (var rich in item.DatiStatiRapporti.Richieste)
                        {
                            string[] sw = rich.XR_MAT_CATEGORIE.DESCRIZIONE_ECCEZIONE.Split(',');
                            var oper = rich.XR_WKF_OPERSTATI.OrderByDescending(y => y.ID_STATO).FirstOrDefault();
                            var stato = listWkfStati.FirstOrDefault(x => x.ID_STATO == oper.ID_STATO);

                            content += rich.XR_MAT_CATEGORIE.TITOLO + " - ";
                            content += rich.XR_MAT_CATEGORIE.SOTTO_TITOLO + " - ";

                            switch (rich.XR_MAT_CATEGORIE.TIPO_AGGIORNAMENTO_STATO)
                            {
                                case "AGGIUNTIVO":
                                    content += "Giorni approvati: " + rich.GIORNI_APPROVATI.GetValueOrDefault().ToString();
                                    if (rich.DATA_INIZIO_SW.HasValue)
                                        content += " - Periodo: " + rich.DATA_INIZIO_SW.Value.ToString("dd/MM/yyyy") + " - " + rich.DATA_FINE_SW.Value.ToString("dd/MM/yyyy");
                                    break;
                                case "PERIODO":
                                    content += "Giorni: " + sw[1];
                                    if (rich.INIZIO_GIUSTIFICATIVO.HasValue)
                                        content += " - Periodo: " + rich.INIZIO_GIUSTIFICATIVO.Value.ToString("dd/MM/yyyy") + " - " + rich.FINE_GIUSTIFICATIVO.Value.ToString("dd/MM/yyyy");
                                    break;
                                default:
                                    break;
                            }


                            if (rich.XR_MAT_CATEGORIE.TIPO_AGGIORNAMENTO_STATO == "AGGIUNTIVO")
                            {
                                content += "Giorni approvati: " + rich.GIORNI_APPROVATI.GetValueOrDefault().ToString();
                            }
                            else
                            {
                                content += "Giorni: " + sw[1];
                            }

                            content += "\n";
                        }
                        if (content != null) content = content.Trim('\n');
                        ws.Cell(row, 18 + offset).SetValue(content);
                        ws.Cell(row, 18 + offset).Style.Alignment.WrapText = true;
                    }

                    row++;
                    bool dettagli = ricerca.IncludiDettagli;

                    // added-max
                    if (current != null && dettagli)
                    {
                        var StatiRapportoInfo = dbTal.XR_STATO_RAPPORTO_INFO.Where(x => x.ID_STATO_RAPPORTO == current.IdEvento
                            && x.VALID_DTA_INI <= DateTime.Now &&
                            (x.VALID_DTA_END == null || x.VALID_DTA_END >= DateTime.Now))
                            .OrderBy(x => x.DTA_INIZIO)
                            .ToList();

                        if (StatiRapportoInfo.Any())
                        {
                            int counter = 0;
                            foreach (var sr in StatiRapportoInfo)
                            {

                                counter++;
                                if (counter == 1)
                                {
                                    ws.Cell(row - 1, 15).SetValue(sr.DTA_INIZIO);

                                    ws.Cell(row - 1, 16).SetValue(sr.DTA_FINE);
                                    ws.Cell(row - 1, 16 + offset).SetValue(sr.NUM_GIORNI_MAX);
                                    ws.Cell(row - 1, 17 + offset).SetValue(sr.NUM_GIORNI_EXTRA);
                                    continue;
                                }
                                DuplicaPrecedenteRiga(row, ws);
                                ws.Cell(row, 15).SetValue(sr.DTA_INIZIO);

                                ws.Cell(row, 16).SetValue(sr.DTA_FINE);
                                ws.Cell(row, 16 + offset).SetValue(sr.NUM_GIORNI_MAX);
                                ws.Cell(row, 17 + offset).SetValue(sr.NUM_GIORNI_EXTRA);
                                row++;
                            }
                        }
                    }


                }
                ws.Range(1, 1, row - 1, 18 + offset).CreateTable();
                ws.Columns().AdjustToContents();

                if (ricerca.IncludiDettagli)
                {
                    string matr = "";
                    bool lit = false;
                    for (int i = 2; i <= row; i++)
                    {
                        string m = ws.Cell(i, 1).Value.ToString();
                        if (!String.IsNullOrWhiteSpace(m) && m != matr)
                        {
                            matr = m;
                            lit = !lit;
                        }

                        if (lit)
                            //if (ws.Cell(i, 1).Value != null && !String.IsNullOrWhiteSpace(ws.Cell(i, 1).Value.ToString()))
                            ws.Row(i).Style.Fill.SetBackgroundColor(XLColor.FromArgb(220, 230, 241));
                        else
                            ws.Row(i).Style.Fill.SetBackgroundColor(XLColor.White);
                    }
                }

                ws.Column(13).Width = 10;
                ws.Column(14).Width = 10;
            }
            MemoryStream ms = null;
            ms = new MemoryStream();
            wb.SaveAs(ms);
            ms.Position = 0;

            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = $"Esportazione al {DateTime.Now.ToString("dd-MM-yyyy")}.xlsx" };
        }
        public void DuplicaPrecedenteRiga(int row, IXLWorksheet ws )
        {
            for (int i = 1; i <= 19; i++)
            {
                ws.Cell(row, i).SetValue(ws.Cell(row-1,i).Value);
            }
        }
        public ActionResult Report(string tipologia)
        {
            MemoryStream ms = null;
            switch (tipologia)
            {
                case "Consensuale":
                    ms = ReportConsensuale();
                    return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = $"Esportazione al {DateTime.Now.ToString("dd-MM-yyyy")}.xlsx" };
                default:
                    return View("~/Views/Shared/404.cshtml");
            }
        }
        public MemoryStream ReportConsensuale()
        {
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Dati");

            var db = new TalentiaEntities();
            var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.AsQueryable(), CommonHelper.GetCurrentUserMatricola(), "", StatiRapportoController.GetSWAbilFunc());
            var listMatrAbil = tmpSint.Select(x => x.COD_MATLIBROMAT);

            var listStati = db.XR_STATO_RAPPORTO.Include("SINTESI1").Include("XR_STATO_RAPPORTO_INFO")
                            .Where(x => x.COD_TIPO_ACCORDO == "Consensuale" && x.VALID_DTA_END == null)
                            .Where(x => listMatrAbil.Contains(x.MATRICOLA))
                            .OrderBy(x => x.MATRICOLA).ThenBy(x => x.DTA_NOTIF_DIP)
                            .ToList();

            //var listStati = tmpListStati.Where(x => listMatrAbil.Contains(x.MATRICOLA));

            ws.Cell(1, 1).SetValue("Matricola");
            ws.Cell(1, 2).SetValue("Cognome");
            ws.Cell(1, 3).SetValue("Nome");
            ws.Cell(1, 4).SetValue("Data assunzione");
            ws.Cell(1, 5).SetValue("Sede");
            ws.Cell(1, 6).SetValue("Servizio");
            ws.Cell(1, 7).SetValue("Sezione");
            ws.Cell(1, 8).SetValue("Figura professionale");
            ws.Cell(1, 9).SetValue("Inizio");
            ws.Cell(1, 10).SetValue("Fine");
            ws.Cell(1, 11).SetValue("Giorni per mese");
            ws.Cell(1, 12).SetValue("Giorni extra");
            ws.Cell(1, 13).SetValue("Stato");
            ws.Cell(1, 14).SetValue("Data notifica");
            ws.Cell(1, 15).SetValue("Data lettura");
            ws.Cell(1, 16).SetValue("Data compilazione");
            ws.Cell(1, 17).SetValue("Assenze");
            ws.Cell(1, 18).SetValue("Da controllare");

            var listMod = db.XR_MOD_DIPENDENTI
                            .Where(x => x.COD_MODULO == "AccordoIndividualeDipendentiSW2022")
                            .Where(x => listMatrAbil.Contains(x.MATRICOLA))
                            .Select(x => new
                            {
                                XR_MOD_DIPENDENTI1 = x.XR_MOD_DIPENDENTI1,
                                MATRICOLA = x.MATRICOLA,
                                SCELTA = x.SCELTA,
                                DATA_COMPILAZIONE = x.DATA_COMPILAZIONE,
                                DATA_LETTURA = x.DATA_LETTURA,
                                DATI_PROTOCOLLO = x.DATI_PROTOCOLLO
                            })
                            .ToList()
                            .Select(x => new XR_MOD_DIPENDENTI
                            {
                                XR_MOD_DIPENDENTI1 = x.XR_MOD_DIPENDENTI1,
                                MATRICOLA = x.MATRICOLA,
                                SCELTA = x.SCELTA,
                                DATA_COMPILAZIONE = x.DATA_COMPILAZIONE,
                                DATA_LETTURA = x.DATA_LETTURA,
                                DATI_PROTOCOLLO = x.DATI_PROTOCOLLO
                            });

            var listMatrSwN = listStati.Where(x => x.COD_STATO_RAPPORTO == "SW_N" || x.COD_STATO_RAPPORTO == "SW_P").Select(x => x.MATRICOLA);

            DateTime minDate = listStati.Where(x => x.COD_STATO_RAPPORTO == "SW_N" || x.COD_STATO_RAPPORTO == "SW_P").Select(x => x.DTA_INIZIO_VISUALIZZAZIONE.HasValue ? x.DTA_INIZIO_VISUALIZZAZIONE.Value : x.DTA_NOTIF_DIP.HasValue ? x.DTA_NOTIF_DIP.Value :x.TMS_TIMESTAMP).Min(x => x);
            var maxDate = listStati.Where(x =>x.DTA_SCADENZA!=null &&( x.COD_STATO_RAPPORTO == "SW_N" || x.COD_STATO_RAPPORTO == "SW_P")).Select(x => x.DTA_SCADENZA.Value).Max(x => x);


            var dbCez = new IncentiviEntities();
            var queryAssenze = GetQueryAssenzeMulti(listMatrSwN, minDate, maxDate);
            var queryOrari = GetQueryOrariMulti(listMatrSwN, minDate, maxDate);
            var assenze = dbCez.Database.SqlQuery<AnagEcc>(queryAssenze).ToList();
            var orari = dbCez.Database.SqlQuery<AnagEcc>(queryOrari).ToList();

            int row = 2;
            foreach (var grMatr in listStati.GroupBy(x => x.MATRICOLA).OrderBy(x => x.Key))
            {
                foreach (var gr in grMatr.GroupBy(x => new { x.DTA_INIZIO, x.DTA_FINE }).OrderBy(x => x.Key.DTA_INIZIO).ThenBy(x => x.Key.DTA_FINE))
                {
                    int countElem = gr.Count();
                    int rowElem = 0;

                    //foreach (var item in gr.OrderBy(x => x.DTA_NOTIF_DIP).ThenBy(x => x.DTA_INIZIO))
                    {
                        //rowElem++;
                        var item = gr.OrderByDescending(x => x.DTA_NOTIF_DIP).FirstOrDefault();
                        rowElem = countElem;

                        ws.Cell(row, 1).SetValue(item.SINTESI1.COD_MATLIBROMAT);
                        ws.Cell(row, 2).SetValue(item.SINTESI1.DES_COGNOMEPERS.TitleCase());
                        ws.Cell(row, 3).SetValue(item.SINTESI1.DES_NOMEPERS.TitleCase());
                        ws.Cell(row, 4).SetValue(item.SINTESI1.DTA_INIZIO_CR.Value.ToString("dd/MM/yyyy"));
                        ws.Cell(row, 5).SetValue(item.SINTESI1.DES_SEDE);
                        ws.Cell(row, 6).SetValue(item.SINTESI1.DES_SERVIZIO);
                        ws.Cell(row, 7).SetValue(item.SINTESI1.DES_DENOMUNITAORG);
                        ws.Cell(row, 8).SetValue(item.SINTESI1.QUALIFICA.TB_QUALSTD.DES_QUALSTD.UpperFirst());

                        //var tmp = item.Where(x => x.DTA_INIZIO <= DateTime.Today && x.DTA_FINE > DateTime.Today);

                        //var current = tmp.FirstOrDefault();
                        //if (current == null)
                        //{
                        //    var tmp2 = item.AsEnumerable();
                        //    current = tmp2.OrderByDescending(x => x.DTA_INIZIO).FirstOrDefault();
                        //}
                        var current = item;

                        if (current != null)
                        {
                            ws.Cell(row, 9).SetValue(current.DTA_INIZIO);
                            ws.Cell(row, 10).SetValue(current.DTA_FINE);
                        }

                        var info = current.XR_STATO_RAPPORTO_INFO.FirstOrDefault(x => x.VALID_DTA_END == null);
                        if (info != null)
                        {
                            ws.Cell(row, 11).SetValue(info.NUM_GIORNI_MAX);
                            ws.Cell(row, 12).SetValue(info.NUM_GIORNI_EXTRA);
                        }

                        string stato = "";
                        XR_MOD_DIPENDENTI mod = null;
                        if (current.ID_MOD_DIPENDENTI.HasValue)
                            mod = listMod.FirstOrDefault(x => x.XR_MOD_DIPENDENTI1 == current.ID_MOD_DIPENDENTI);
                        else if (rowElem == countElem)
                            mod = listMod.FirstOrDefault(x => x.MATRICOLA == item.MATRICOLA);

                        bool nonSelezionato = false;
                        switch (current.COD_STATO_RAPPORTO)
                        {
                            case "SW":
                                stato = "Accettato";
                                break;
                            case "SW_P":
                                stato = "Scelta non effettuata";
                                nonSelezionato = true;
                                break;
                            case "SW_R":
                                stato = "Rifiutato";
                                break;
                            case "SW_N":
                                stato = "Scelta non effettuata";
                                nonSelezionato = true;
                                break;
                            default:
                                break;
                        }
                        ws.Cell(row, 13).SetValue(stato);
                        ws.Cell(row, 14).SetValue(current.DTA_NOTIF_DIP);
                        if (mod != null)
                        {
                            ws.Cell(row, 15).SetValue(mod.DATA_LETTURA);
                            ws.Cell(row, 16).SetValue(mod.DATA_COMPILAZIONE);
                        }

                        if (nonSelezionato)
                        {
                            DateTime from, to;
                            if (item.DTA_INIZIO_VISUALIZZAZIONE.HasValue)
                                from = item.DTA_INIZIO_VISUALIZZAZIONE.Value;
                            else if (item.DTA_NOTIF_DIP.HasValue)
                                from = item.DTA_NOTIF_DIP.Value;
                            else
                                from = item.TMS_TIMESTAMP;

                            to = item.DTA_SCADENZA.Value;

                            var matrAssenze = assenze.Where(x => x.matricola == item.MATRICOLA && x.data >= from && x.data <= to);
                            string assenza = String.Join("\n", matrAssenze.Select(x => x.data.ToString("dd/MM/yyyy") + " - " + x.desc_eccezione));

                            //recupero i dati orari
                            bool daControllare = false;
                            var matrOrari = orari.Where(x => x.matricola == item.MATRICOLA && x.data >= from && x.data <= to);
                            if (matrOrari.Count() != 5)
                                daControllare = true;
                            else
                            {
                                //quanti giorni erano di prevista presenza?
                                int ggPrev = matrOrari.Count(x => x.prevista_presenza > 0);
                                if (matrAssenze.Count() >= ggPrev)
                                    daControllare = false; //Era assente tutti i giorni lavorativi previsti
                                else
                                    daControllare = true; //Era presente qualche giorno
                            }

                            ws.Cell(row, 17).SetValue(assenza);
                            ws.Cell(row, 18).SetValue(daControllare ? "Sì" : "No");
                        }

                        row++;
                    }
                }
            }
            ws.Range(1, 1, row - 1, 18).CreateTable();
            ws.Range(1, 1, row - 1, 18).Style.Alignment.SetWrapText(true).Alignment.Vertical = XLAlignmentVerticalValues.Top;
            ws.Columns().AdjustToContents();

            MemoryStream ms = null;
            ms = new MemoryStream();
            wb.SaveAs(ms);
            ms.Position = 0;

            return ms;
        }

        private static string GetQueryAssenze(IEnumerable<string> elencoMatr)
        {
            string query = $@"
                    SELECT t0.matricola_dp as Matricola,  
                    t2.data,
                    CASE   
                        WHEN t3.cod_eccez_padre IS NULL  
                            THEN 'Varie'  
                        ELSE t3.cod_eccez_padre  
                        END AS cod_eccez_padre,  
                    CASE   
                        WHEN t3.desc_cod_eccez_padre IS NULL  
                            THEN 'VARIE'  
                        ELSE t3.desc_cod_eccez_padre  
                        END AS desc_cod_eccez_padre,  
                    t3.cod_eccezione,  
                    t3.desc_eccezione,  
                    CASE   
                        WHEN t3.unita_misura = 'G'  
                            THEN 'GG'  
                        WHEN t3.unita_misura = 'H'  
                            THEN 'Ore'  
                        WHEN t3.unita_misura = 'K'  
                            THEN 'Km'  
                        WHEN t3.unita_misura = 'N'  
                            THEN 'Q.tà'  
                        ELSE t3.unita_misura  
                        END AS unita_misura, 
                    t1.quantita_numero, 
                    t1.quantita_ore, 
                    t2.tipo_giorno  
                    FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0  
                    INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_ECCEZIONI] t1 ON (t1.SKY_MATRICOLA = t0.sky_anagrafica_unica)  
                    INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] t2 ON (t2.sky_tempo = t1.SKY_DATA)  
                    INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ECCEZIONE] t3 ON (t3.sky_eccezione = t1.SKY_eccezione)  
                    WHERE (  
                        t0.matricola_dp IN ('{String.Join("','", elencoMatr)}')  
                        AND ( t2.data between '2022-03-24' and '2022-03-28' ) 
	                    and t3.flag_eccez='C' and t3.flag_macroassen='G' and t3.cod_eccez_padre not in ('TIS')
	                    ) 
                    order by t2.data";

            return query;
        }
        private static string GetQueryAssenzeSingola(string matricola, DateTime from, DateTime to)
        {
            string query = $@"
                    SELECT t0.matricola_dp as Matricola,  
                    t2.data,
                    CASE   
                        WHEN t3.cod_eccez_padre IS NULL  
                            THEN 'Varie'  
                        ELSE t3.cod_eccez_padre  
                        END AS cod_eccez_padre,  
                    CASE   
                        WHEN t3.desc_cod_eccez_padre IS NULL  
                            THEN 'VARIE'  
                        ELSE t3.desc_cod_eccez_padre  
                        END AS desc_cod_eccez_padre,  
                    t3.cod_eccezione,  
                    t3.desc_eccezione,  
                    CASE   
                        WHEN t3.unita_misura = 'G'  
                            THEN 'GG'  
                        WHEN t3.unita_misura = 'H'  
                            THEN 'Ore'  
                        WHEN t3.unita_misura = 'K'  
                            THEN 'Km'  
                        WHEN t3.unita_misura = 'N'  
                            THEN 'Q.tà'  
                        ELSE t3.unita_misura  
                        END AS unita_misura, 
                    t1.quantita_numero, 
                    t1.quantita_ore, 
                    t2.tipo_giorno  
                    FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0  
                    INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_ECCEZIONI] t1 ON (t1.SKY_MATRICOLA = t0.sky_anagrafica_unica)  
                    INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] t2 ON (t2.sky_tempo = t1.SKY_DATA)  
                    INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ECCEZIONE] t3 ON (t3.sky_eccezione = t1.SKY_eccezione)  
                    WHERE (  
                        t0.matricola_dp='{matricola}' 
                        AND ( t2.data between '{from.ToString("yyyy-MM-dd")}' and '{to.ToString("yyyy-MM-dd")}' ) 
	                    and t3.flag_eccez='C' and t3.flag_macroassen='G' and t3.cod_eccez_padre not in ('TIS')
	                    ) 
                    order by t2.data";

            return query;
        }
        private static string GetQueryAssenzeMulti(IEnumerable<string> elencoMatr, DateTime from, DateTime to)
        {
            string query = $@"
                    SELECT t0.matricola_dp as Matricola,  
                    t2.data,
                    CASE   
                        WHEN t3.cod_eccez_padre IS NULL  
                            THEN 'Varie'  
                        ELSE t3.cod_eccez_padre  
                        END AS cod_eccez_padre,  
                    CASE   
                        WHEN t3.desc_cod_eccez_padre IS NULL  
                            THEN 'VARIE'  
                        ELSE t3.desc_cod_eccez_padre  
                        END AS desc_cod_eccez_padre,  
                    t3.cod_eccezione,  
                    t3.desc_eccezione,  
                    CASE   
                        WHEN t3.unita_misura = 'G'  
                            THEN 'GG'  
                        WHEN t3.unita_misura = 'H'  
                            THEN 'Ore'  
                        WHEN t3.unita_misura = 'K'  
                            THEN 'Km'  
                        WHEN t3.unita_misura = 'N'  
                            THEN 'Q.tà'  
                        ELSE t3.unita_misura  
                        END AS unita_misura, 
                    t1.quantita_numero, 
                    t1.quantita_ore, 
                    t2.tipo_giorno  
                    FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0  
                    INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_ECCEZIONI] t1 ON (t1.SKY_MATRICOLA = t0.sky_anagrafica_unica)  
                    INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] t2 ON (t2.sky_tempo = t1.SKY_DATA)  
                    INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ECCEZIONE] t3 ON (t3.sky_eccezione = t1.SKY_eccezione)  
                    WHERE (  
                        t0.matricola_dp IN ('{String.Join("','", elencoMatr)}')  
                        AND ( t2.data between '{from.ToString("yyyy-MM-dd")}' and '{to.ToString("yyyy-MM-dd")}' ) 
	                    and t3.flag_eccez='C' and t3.flag_macroassen='G' and t3.cod_eccez_padre not in ('TIS')
	                    ) 
                    order by t2.data";

            return query;
        }

        private static string GetQueryOrari(IEnumerable<string> elencoMatr)
        {
            string result = $@"SELECT distinct t0.matricola_dp as Matricola,  
                                t2.data,
                                orario.cod_orario,
					            orario.desc_orario,
					            orario.prevista_presenza
                                FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0  
                                INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_GIORNO_X_DIP] t1 ON (t1.SKY_MATRICOLA = t0.sky_anagrafica_unica)  
					            INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ORARIO] orario on (t1.sky_orario_Reale=orario.sky_orario)
                                INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] t2 ON (t2.sky_tempo = t1.SKY_DATA)  
                                WHERE (  
                                    t0.matricola_dp IN ('{String.Join("','", elencoMatr)}')  
                                    AND ( t2.data between '2022-03-24' and '2022-03-28' ) 
	                                ) 
                                order by t2.data";
            return result;
        }
        private static string GetQueryOrariSingola(string matricola, DateTime from, DateTime to)
        {
            string result = $@"SELECT distinct t0.matricola_dp as Matricola,  
                                t2.data,
                                orario.cod_orario,
					            orario.desc_orario,
					            orario.prevista_presenza
                                FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0  
                                INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_GIORNO_X_DIP] t1 ON (t1.SKY_MATRICOLA = t0.sky_anagrafica_unica)  
					            INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ORARIO] orario on (t1.sky_orario_Reale=orario.sky_orario)
                                INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] t2 ON (t2.sky_tempo = t1.SKY_DATA)  
                                WHERE (  
                                    t0.matricola_dp ='{matricola}'
                                    AND ( t2.data between '{from.ToString("yyyy-MM-dd")}' and '{to.ToString("yyyy-MM-dd")}' ) 
	                                ) 
                                order by t2.data";
            return result;
        }
        private static string GetQueryOrariMulti(IEnumerable<string> elencoMatr, DateTime from, DateTime to)
        {
            string result = $@"SELECT distinct t0.matricola_dp as Matricola,  
                                t2.data,
                                orario.cod_orario,
					            orario.desc_orario,
					            orario.prevista_presenza
                                FROM [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ANAGRAFICA_UNICA] t0  
                                INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2F_GIORNO_X_DIP] t1 ON (t1.SKY_MATRICOLA = t0.sky_anagrafica_unica)  
					            INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_ORARIO] orario on (t1.sky_orario_Reale=orario.sky_orario)
                                INNER JOIN [LINK_HRDW_LIV2].[HR_LIV2].[dbo].[L2D_TEMPO] t2 ON (t2.sky_tempo = t1.SKY_DATA)  
                                WHERE (  
                                    t0.matricola_dp IN ('{String.Join("','", elencoMatr)}')  
                                    AND ( t2.data between '{from.ToString("yyyy-MM-dd")}' and '{to.ToString("yyyy-MM-dd")}' ) 
	                                ) 
                                order by t2.data";
            return result;
        }

        [HttpPost]
        public ActionResult AggiornaAPI(string matr)
        {
            var db = new TalentiaEntities();
            DateTime Dnow = DateTime.Now;

            var row = db.XR_STATO_RAPPORTO.Where(x =>
                          x.MATRICOLA == matr &&
                          x.COD_STATO_RAPPORTO == "SW" &&
                          x.DTA_INIZIO <= Dnow && x.DTA_FINE >= Dnow &&
                          x.VALID_DTA_INI <= Dnow &&
                          (x.VALID_DTA_END == null || x.VALID_DTA_END > Dnow) &&
                          x.COD_TIPO_ACCORDO == "Consensuale").FirstOrDefault();

            if (row == null)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = "Accordo firmato non trovato" }
                };
            }
            try
            {
                if (  !AnagraficaManager. EsisteAPInuovaPerMatricola(row.MATRICOLA, row.DTA_INIZIO, row.DTA_FINE))
                {
                    AnagraficaManager.InserisciApiNuova(row);
                }
                    
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = ex.Message}
                };
            }

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Data = new { esito = true}
            };
        }
        public ActionResult InfoApiCF(string cf)
        {
            //TokenResponse TR = SmartworkingManager.APIgetToken();
            myRaiHelper.Token.TokenResponse TR = myRaiHelper.Token.TokenAPI.GetToken();
            if (TR == null)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = "Impossibile negoziare la comunicazione col server" }
                };
            }
            RicercaComunicazioniResponse Response =RicercaComunicazioneApi. RicercaComunicazioni(cf, TR.access_token);
            return View(Response);
        }
        public ActionResult InfoCom(string codice, string matricola)
        {

            // TokenResponse TR = SmartworkingManager.APIgetToken();
            myRaiHelper.Token.TokenResponse TR = myRaiHelper.Token.TokenAPI.GetToken();
            if (TR == null)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = "Impossibile negoziare la comunicazione col server" }
                };
            }



            InfoComunicazioneResponse Response = InfoComunicazioneAPI. InfoComunicazione(codice, TR.access_token);

            return View(Response);
        }

        public ActionResult CheckPrimaModificaCom(string matricola)
        {
           
            var db = new IncentiviEntities();
            if (db.XR_SW_API.Any(x => x.MATRICOLA == matricola && x.TIPOLOGIA_API == "I"))
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false }
                };
            }
            else
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true }
                };
            }
        }
        public ActionResult GetAPIfound(string matricola)
        {
            APIfoundModel APIfound = new APIfoundModel();
            APIfound.APIs = GetApiSWmatricola(matricola);
            APIfound.matricola = matricola;

            var db = new IncentiviEntities();
            APIfound.CodiciComunicazioniAnnullate = db.XR_SW_API.Where(x => x.TIPOLOGIA_API == "A").Select(x => x.CODICE_COMUNICAZIONE_API).ToList();

            APIfound.CF = db.SINTESI1.Where(x => x.COD_MATLIBROMAT == matricola).Select(x => x.CSF_CFSPERSONA).FirstOrDefault();
            return View("_APIfound", APIfound);
        }

        public void TryUpdateRowSwAPI(int idPersona)
        {
            var db = new TalentiaEntities();
            var sint = db.SINTESI1.Where(x => x.ID_PERSONA == idPersona).FirstOrDefault();
            if (sint != null)
            {
                string matr = sint.COD_MATLIBROMAT;
                string cf = sint.CSF_CFSPERSONA;
                if (String.IsNullOrWhiteSpace(cf) || String.IsNullOrWhiteSpace(matr)) return;

                var dbCzn = new IncentiviEntities();
                var rowApi = dbCzn.XR_SW_API.Where(x => x.TIPOLOGIA_API == "I" && x.MATRICOLA == matr).FirstOrDefault();
                if (rowApi == null)
                {
                    try
                    {
                        myRaiHelper.Token.TokenResponse Token = myRaiHelper.Token.TokenAPI.GetToken();
                        if (Token != null)
                        {
                            System.Net.WebClient wb = myRaiHelper.Token.TokenAPI.GetWebClient(Token.access_token);
                            string esito = myRaiHelper.Token.TokenAPI.AllineaDaRestVersoDBmatricola(matr, cf, wb, Token.access_token);

                            if (String.IsNullOrWhiteSpace(esito))
                                myRaiHelper.Logger.LogAzione(new myRaiData.MyRai_LogAzioni()
                                {
                                    descrizione_operazione = "Allineata matr " + matr,
                                    provenienza = "TryUpdateRowSwAPI",
                                    operazione = "Allineata da REST a DB matr " + matr
                                });
                            else
                                myRaiHelper.Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                                {
                                    provenienza = "TryUpdateRowSwAPI",
                                    error_message = "matr " + matr + ":" + esito
                                });
                        }
                    }
                    catch (Exception ex)
                    {
                        myRaiHelper.Logger.LogErrori(new myRaiData.MyRai_LogErrori()
                        {
                            provenienza = "TryUpdateRowSwAPI",
                            error_message = "matr " + matr + ":" + ex.ToString()
                        });
                    }
                }
            }
        }
        public ActionResult Modal_Dipendente(int idPersona)
        {
            var db = new TalentiaEntities();

            TryUpdateRowSwAPI (idPersona);

            AnagraficaModel anag = AnagraficaManager.GetAnagrafica(null, idPersona, new AnagraficaLoader(SezioniAnag.StatoRapporto), false);

            
            return View(anag);
        }

        private List<XR_SW_API> GetApiSWmatricola(string matricola)
        {
            var db = new IncentiviEntities();
            var list = db.XR_SW_API.Where(x => x.MATRICOLA == matricola).OrderBy(x => x.ID).ToList();
            return list;
        }

        [HttpPost]
        public ActionResult AnnullaCom(string CodiceComunicazione)
        {
            var db = new IncentiviEntities();
            var APIriferimento = db.XR_SW_API.Where(x => x.CODICE_COMUNICAZIONE_API == CodiceComunicazione).FirstOrDefault();
            XR_SW_API api = new XR_SW_API();
            api.PERIODO_DAL = APIriferimento.PERIODO_DAL;
            api.PERIODO_AL = APIriferimento.PERIODO_AL;
            api.MATRICOLA = APIriferimento.MATRICOLA;
            api.MATRICOLA_OPERATORE = CommonHelper.GetCurrentUserMatricola();
            api.DATA_CREAZIONE = DateTime.Now;
            api.TIPOLOGIA_API = "A"; // A = ANNULLA
            api.CODICE_COMUNICAZIONE_API = CodiceComunicazione;
            db.XR_SW_API.Add(api);
            try
            {
                db.SaveChanges();
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = true }
                };
            }
            catch (Exception ex)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = ex.Message }
                };
            }

        }

        [HttpPost]
        public ActionResult ModificaCom(string CodiceComunicazione, string d1, string d2)
        {
            DateTime D1;
            DateTime D2;
            if (!DateTime.TryParseExact(d1, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D1) ||
                !DateTime.TryParseExact(d2, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D2) ||
                D2 < D1
                )
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = "Periodo non valido" }
                };
            }
            var db = new IncentiviEntities();
            var APIriferimento = db.XR_SW_API.Where(x => x.CODICE_COMUNICAZIONE_API == CodiceComunicazione).FirstOrDefault();

            var row = db.XR_SW_API.Where(x => x.MATRICOLA == APIriferimento.MATRICOLA && x.PERIODO_DAL == D1 && x.PERIODO_AL == D2).FirstOrDefault();
            if (row != null)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = "Periodo gia presente" }
                };
            }
            else
            {
                XR_SW_API api = new XR_SW_API();
                api.PERIODO_DAL = D1;
                api.PERIODO_AL = D2;
                api.MATRICOLA = APIriferimento.MATRICOLA;
                api.MATRICOLA_OPERATORE = CommonHelper.GetCurrentUserMatricola();
                api.DATA_CREAZIONE = DateTime.Now;
                api.TIPOLOGIA_API = "M"; // M = MODIFICA
                api.CODICE_COMUNICAZIONE_API = CodiceComunicazione;
                db.XR_SW_API.Add(api);
                try
                {
                    db.SaveChanges();
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = true }
                    };
                }
                catch (Exception ex)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = false, errore = ex.Message }
                    };
                }

            }
        }
        public ActionResult ProponiModificaCom(string CodiceComunicazione)
        {
            var db = new IncentiviEntities();
            var com = db.XR_SW_API.Where(x => x.CODICE_COMUNICAZIONE_API == CodiceComunicazione).FirstOrDefault();
            ModificaComunicazioneModel model = new ModificaComunicazioneModel() { 
             CodiceComunicazione=CodiceComunicazione,
             Matricola= com.MATRICOLA,
              DataInizio =com.PERIODO_DAL,
              DataFine= com.PERIODO_AL
            };
            return View("_modificaComunicazione", model);
        }
        public ActionResult ProponiNuovaCom(string matricola)
        {
            var dbTal = new TalentiaEntities();
            var item = dbTal.XR_STATO_RAPPORTO.Where(x => x.MATRICOLA == matricola
                                            && (x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now))
                .OrderByDescending(x => x.DTA_INIZIO).FirstOrDefault();

            var db = new IncentiviEntities();
            NuovaComunicazioneModel model = new NuovaComunicazioneModel() {  Matricola =matricola};
           
            if (item != null)
            {
                if (! db.XR_SW_API.Any(x => x.MATRICOLA == matricola && x.PERIODO_DAL == item.DTA_INIZIO && 
                        x.PERIODO_AL == item.DTA_FINE))
                {
                    model.DataInizio = item.DTA_INIZIO;
                    model.DataFine = item.DTA_FINE;
                }
            }

            return View("_nuovaComunicazione", model);
        }
        [HttpPost]
        public ActionResult AggiungiNuovaCom(string matricola, string d1, string d2)
        {
            DateTime D1;
            DateTime D2;
            if (!DateTime.TryParseExact(d1, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D1) ||
                !DateTime.TryParseExact(d2, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out D2) ||
                D2<D1
                )
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = "Periodo non valido" }
                };
            }

            var db = new IncentiviEntities();
            var row = db.XR_SW_API.Where(x => x.MATRICOLA == matricola && x.PERIODO_DAL == D1 && x.PERIODO_AL == D2).FirstOrDefault();
            if (row != null)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                    Data = new { esito = false, errore = "Periodo gia presente" }
                };
            }
            else
            {
                XR_SW_API api = new XR_SW_API();
                api.PERIODO_DAL = D1;
                api.PERIODO_AL = D2;
                api.MATRICOLA = matricola;
                api.MATRICOLA_OPERATORE = CommonHelper.GetCurrentUserMatricola();
                api.DATA_CREAZIONE = DateTime.Now;
                api.TIPOLOGIA_API = "I"; // I = NUOVA
                db.XR_SW_API.Add(api);
                try
                {
                    db.SaveChanges();
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = true }
                    };
                }
                catch (Exception ex)
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                        Data = new { esito = false, errore = ex.Message }
                    };
                }

            }
        }
        
        //public ActionResult Modal_Richiesta(int idRichiesta)
        //{
        //    var db = new IncentiviEntities();
        //    var model = db.XR_MAT_RICHIESTE.Find(idRichiesta);
        //    return View("_Import_Preview_Dati", model);
        //}

        public ActionResult DownloadAccordo(int id)
        {
            var db = new TalentiaEntities();
            var modulo = db.XR_MOD_DIPENDENTI.Find(id);
            var sint = db.SINTESI1.Find(modulo.ID_PERSONA);
            //Response.AddHeader("Content-Disposition", "inline;filename=\"Accordo individuale "+sint.Nominativo()+".pdf\"");

            return File(modulo.PDF_MODULO, "application/pdf", "Accordo individuale " + sint.Nominativo() + ".pdf");
        }

        public ActionResult Modal_GestStatoRapporto(int idPersona, int idEvento)
        {
            var model = AnagraficaManager.GetAnagrafica(null, idPersona, new AnagraficaLoader(SezioniAnag.StatoRapporto));
            DateTime min = DateTime.Today;

            if (model.DatiStatiRapporti != null && model.DatiStatiRapporti.Eventi != null && model.DatiStatiRapporti.Eventi.Any())
            {
                if (model.DatiStatiRapporti.Eventi.Any(x => x.ValiditaFine == null))
                    min = model.DatiStatiRapporti.Eventi.Where(x => x.ValiditaFine == null).Max(x => x.DataFine).AddDays(1);
                model.DatiStatiRapporti.Eventi.RemoveWhere(x => x.IdEvento != idEvento);
            }

            if (idEvento == 0)
            {
                int massimoGiorniSeDeroga = 0;
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    var ut = db.SINTESI1.Where(w => w.ID_PERSONA == idPersona).FirstOrDefault();
                    
                    // se l'utente è un Dirigente allora il codice protocollatore dovrà essere quello di Dirigenti e Assimilati
                    if (ut.COD_QUALIFICA.Trim().ToUpper().StartsWith("A01"))
                    {
                        massimoGiorniSeDeroga = 12;
                    }
                    // dirigenti giornalisti
                    else if (ut.COD_QUALIFICA.Trim().ToUpper().StartsWith("A7"))
                    {
                        massimoGiorniSeDeroga = 12;
                    }
                    // giornalisti
                    else if (ut.COD_QUALIFICA.Trim().ToUpper().StartsWith("M"))
                    {
                        massimoGiorniSeDeroga = 10;
                    }
                    else
                    {
                        massimoGiorniSeDeroga = 12;
                    }
                }

                model.DatiStatiRapporti.Eventi.Add(new EventoModel()
                {
                    Matricola = model.Matricola,
                    IdPersona = model.IdPersona,
                    MinDate = model.DataAssunzione,
                    MaxDate = model.DataCessazione,
                    DataInizio = DateTime.Today,
                    DataFine = DateTime.Today.AddDays(1),
                    Codice = "SW",
                    TipologiaAccordo = "Unilaterale",
                    MostraProposta = true,
                    MassimoGiorniSeDeroga = massimoGiorniSeDeroga
                });
            }

            return View(model);
        }

        public ActionResult Modal_GestGiorni(int idEvento)
        {
            var db = new TalentiaEntities();
            var item = db.XR_STATO_RAPPORTO.Find(idEvento);

            EventoModel model = new EventoModel()
            {
                IdPersona = item.ID_PERSONA,
                IdEvento = item.ID_STATO_RAPPORTO,
                Codice = item.COD_STATO_RAPPORTO,
                Descrizione = item.XR_TB_STATO_RAPPORTO.DES_STATO_RAPPORTO,
                CodiceEvento = "",
                DataInizio = item.DTA_INIZIO,
                DataFine = item.DTA_FINE,
                TipologiaAccordo = item.COD_TIPO_ACCORDO,
                NotificaDipendente = item.DTA_NOTIF_DIP,
                NotificaEnte = item.DTA_NOTIF_ENTE,
                ValiditaInizio = item.VALID_DTA_INI,
                ValiditaFine = item.VALID_DTA_END,
                IdEventoPrec = item.ID_STATO_RAPPORTO_ORIG,
                DataScadenza = item.DTA_SCADENZA,
                DataPresentazioneProposta = item.DTA_INIZIO_VISUALIZZAZIONE,
                BloccaDataInizio = item.FLG_FORZA_INIZIO_ACCORDO.GetValueOrDefault(),
                Tipo = TipoEvento.Stato,
                Modulo = item.ID_MOD_DIPENDENTI
            };

            model.Info = new List<EventoModelInfo>();
            if (item.XR_STATO_RAPPORTO_INFO != null && item.XR_STATO_RAPPORTO_INFO.Any(x => x.VALID_DTA_END == null))
            {
                model.Info.AddRange(item.XR_STATO_RAPPORTO_INFO.Where(x => x.VALID_DTA_END == null).Select(x => new EventoModelInfo()
                {
                    DataInizio = x.DTA_INIZIO,
                    DataFine = x.DTA_FINE,
                    NumeroGiorniMax = x.NUM_GIORNI_MAX,
                    NumeroGiorniExtra = x.NUM_GIORNI_EXTRA
                }));
            }

            return View(model);
        }

        public ActionResult Save_Giorni(int idEvento, List<EventoModelInfo> info)
        {
            var db = new TalentiaEntities();
            var stato = db.XR_STATO_RAPPORTO.Find(idEvento);
            CezanneHelper.GetCampiFirma(out string codUser, out string codTermid, out DateTime now);
            foreach (var item in stato.XR_STATO_RAPPORTO_INFO)
                item.VALID_DTA_END = now;

            foreach (var item in info)
            {
                stato.XR_STATO_RAPPORTO_INFO.Add(new XR_STATO_RAPPORTO_INFO()
                {
                    DTA_INIZIO = item.DataInizio,
                    DTA_FINE = item.DataFine,
                    NUM_GIORNI_MAX = item.NumeroGiorniMax,
                    NUM_GIORNI_EXTRA = item.NumeroGiorniExtra,
                    VALID_DTA_INI = now,
                    COD_USER = codUser,
                    COD_TERMID = codTermid,
                    TMS_TIMESTAMP = now
                });
            }

            if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), "Aggiornamento giorni SW"))
                return Content("OK");
            else
                return Content("Errore durante il salvataggio");
        }

        public ActionResult Modal_Ricerca()
        {
            CercaDipendenteVM model = new CercaDipendenteVM();

            return View("", model);
        }

        public ActionResult Modal_AggiuntaMassiva()
        {
            return View();
        }

        public ActionResult ScaricaTemplate(string importType)
        {
            string fileName = "";
            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Foglio1");

            ws.Cell(1, 1).SetValue("PMatricola");
            ws.Cell(1, 2).SetValue("Nominativo");
            ws.Cell(1, 3).SetValue("Direzione");
            ws.Cell(1, 4).SetValue("Data inizio");
            ws.Cell(1, 5).SetValue("Data fine");

            if (importType == "UPDATE_SW")
            {
                fileName = "Aggiornamento giorni.xlsx";
                ws.Cell(1, 6).SetValue("Numero giorni");
                ws.Cell(1, 7).SetValue("Numero giorni extra");
            }
            else if (importType == "SW_P")
            {
                fileName = "Proposta accordo lavoro agile.xlsx";
                ws.Cell(1, 6).SetValue("Numero giorni");
                ws.Cell(1, 7).SetValue("Numero giorni extra");
                ws.Cell(1, 8).SetValue("Scadenza accordo");
                ws.Cell(1, 9).SetValue("Decorrenza accordo");
            }
            else if (importType == "FRAGILI")
            {
                fileName = "Fragili.xlsx";
                ws.Cell(1, 6).SetValue("Casistica");
                ws.Cell(1, 7).SetValue("Numero giorni");
                ws.Cell(1, 8).SetValue("Numero giorni extra");
                ws.Cell(1, 9).SetValue("Data visita medica");
            }
            else
            {
                fileName = "Smart working.xlsx";
                ws.Cell(1, 6).SetValue("Tipologia");
                ws.Cell(1, 7).SetValue("Numero giorni");
                ws.Cell(1, 8).SetValue("Numero giorni extra");
            }

            ws.Columns().AdjustToContents();

            MemoryStream ms = null;
            ms = new MemoryStream();
            wb.SaveAs(ms);
            ms.Position = 0;
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = fileName };
        }

        public ActionResult GetBozza(string ipotesi)
        {
            string codTipo = "MailSWFragili";
            XR_HRIS_TEMPLATE template = null;
            IncentiviEntities db = new IncentiviEntities();
            var query = db.XR_HRIS_TEMPLATE.Where(x => x.COD_TIPO.ToUpper() == codTipo.ToUpper() && x.NME_TEMPLATE == ipotesi && x.ID_GESTIONE == null && x.ID_TIPOLOGIA == null);
            query = query.Where(x => x.VALID_DTA_INI < DateTime.Now && (x.VALID_DTA_END == null || x.VALID_DTA_END > DateTime.Now));

            template = query.FirstOrDefault();

            bool result = template != null;
            string testo = result ? template.TEMPLATE_TEXT : null;
            string oggetto = result ? !String.IsNullOrWhiteSpace(template.MAIL_OGGETTO) ? template.MAIL_OGGETTO : template.DES_TEMPLATE : null;

            return new JsonResult() { Data = new { esito = result, testo = testo, oggetto = oggetto }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public ActionResult CaricaDati(HttpPostedFileBase importFile, string importType, bool importClic, string importAccordo)
        {
            string errorMsg = "";
            List<EventoModel> eventiMessaggi = new List<EventoModel>();

            if (importFile != null && importFile.ContentLength > 0)
            {
                var wb = new XLWorkbook(importFile.InputStream);
                bool foundWs = wb.TryGetWorksheet("foglio1", out IXLWorksheet ws);
                if (!foundWs)
                {
                    errorMsg = "Non è stato trovato il foglio denominato 'Foglio1'";
                }
                else
                {
                    int row = 2;
                    string cellValue = "";

                    while (!String.IsNullOrWhiteSpace(cellValue = ws.Cell(row, 1).GetValue<string>()))
                    {
                        string pMatricola = null;
                        string nominativo = null;
                        DateTime? periodoDal = null;
                        DateTime? periodoAl = null;
                        string internalImportType = importType;
                        try
                        {
                            using (var db = new TalentiaEntities())
                            {
                                var dbDigi = new myRaiData.digiGappEntities();
                                pMatricola = ws.Cell(row, 1).GetValue<string>().ToUpper();
                                nominativo = ws.Cell(row, 2).GetValue<string>();
                                string direzione = ws.Cell(row, 3).GetValue<string>();
                                periodoDal = ws.Cell(row, 4).GetValue<DateTime?>();
                                periodoAl = ws.Cell(row, 5).GetValue<DateTime?>();

                                bool anyChanges = false;
                                bool internalImportClic = importClic;

                                string tipologia = "";
                                int? numGiorni = null;
                                int? numGiorniExtra = null;
                                DateTime? scadenza = null;
                                string fragili = "";
                                DateTime? dataVisita = null;
                                DateTime? inizioProposta = null;

                                if (internalImportType == "UPDATE_SW")
                                {
                                    numGiorni = ws.Cell(row, 6).GetValue<int?>();
                                    numGiorniExtra = ws.Cell(row, 7).GetValue<int?>();
                                }
                                else if (internalImportType == "SW_P")
                                {
                                    numGiorni = ws.Cell(row, 6).GetValue<int?>();
                                    numGiorniExtra = ws.Cell(row, 7).GetValue<int?>();
                                    scadenza = ws.Cell(row, 8).GetValue<DateTime?>();
                                    inizioProposta = ws.Cell(row, 9).GetValue<DateTime?>();
                                }
                                else if (internalImportType == "FRAGILI")
                                {
                                    fragili = ws.Cell(row, 6).GetValue<string>();
                                    numGiorni = ws.Cell(row, 7).GetValue<int?>();
                                    numGiorniExtra = ws.Cell(row, 8).GetValue<int?>();
                                    dataVisita = ws.Cell(row, 9).GetValue<DateTime?>();
                                }
                                else
                                {
                                    tipologia = ws.Cell(row, 6).GetValue<string>();
                                    numGiorni = ws.Cell(row, 7).GetValue<int?>();
                                    numGiorniExtra = ws.Cell(row, 8).GetValue<int?>();
                                }

                                string matricola = pMatricola.Trim().Replace("P", "");
                                var sint = db.SINTESI1.Include("XR_STATO_RAPPORTO").Include("XR_STATO_RAPPORTO.XR_STATO_RAPPORTO_INFO").FirstOrDefault(x => x.COD_MATLIBROMAT == matricola);

                                if (sint == null)
                                {
                                    eventiMessaggi.Add(new EventoModel()
                                    {
                                        IdEvento = -2,
                                        Matricola = pMatricola,
                                        DataInizio = periodoDal.Value,
                                        DataFine = periodoAl.Value,
                                        DescrizioneEvento = "Matricola non trovata"
                                    });
                                    row++;
                                    continue;
                                }
                                else if (periodoDal.HasValue && periodoAl.HasValue && periodoAl < periodoDal)
                                {
                                    eventiMessaggi.Add(new EventoModel()
                                    {
                                        IdEvento = -3,
                                        Matricola = pMatricola,
                                        DataInizio = periodoDal.Value,
                                        DataFine = periodoAl.Value,
                                        DescrizioneEvento = "Data fine antecedente data inizio"
                                    });
                                    row++;
                                    continue;
                                }

                                DateTime? dtaInvio = null;
                                if (internalImportType == "FRAGILI")
                                {
                                    var template = HrisHelper.GetTemplate(sint, null, "MailSWFragili", fragili);
                                    if (template != null)
                                    {
                                        Dictionary<string, object> additionalToken = new Dictionary<string, object>();
                                        additionalToken.Add("DATA_INIZIO", periodoDal);
                                        additionalToken.Add("DATA_FINE", periodoAl);
                                        additionalToken.Add("NUMERO_GIORNI", numGiorni.GetValueOrDefault() + numGiorniExtra.GetValueOrDefault());
                                        additionalToken.Add("DATA_VISITA", dataVisita);

                                        GestoreMail mail = new myRaiCommonTasks.GestoreMail();
                                        string mailOggetto = HrisHelper.ReplaceToken(sint, null, template.MAIL_OGGETTO, "__", additionalToken);
                                        string mailMittente = template.MAIL_MITTENTE;
                                        string mailDestinario = String.Format("p{0}@rai.it", sint.COD_MATLIBROMAT);//CommonTasks.GetEmailPerMatricola(sintesi.COD_MATLIBROMAT);
                                        string mailTesto = HrisHelper.ReplaceToken(sint, null, template.TEMPLATE_TEXT, "__", additionalToken);
                                        var response = mail.InvioMail(mailTesto, mailOggetto, mailDestinario, mailMittente, mailMittente, null, null, null);
                                        if (response != null && !response.Esito)
                                        {
                                            eventiMessaggi.Add(new EventoModel()
                                            {
                                                IdEvento = -71,
                                                Matricola = pMatricola,
                                                DataInizio = periodoDal.Value,
                                                DataFine = periodoAl.Value,
                                                DescrizioneEvento = "Errore invio mail: " + response.Errore
                                            });
                                        }
                                        else
                                        {
                                            dtaInvio = DateTime.Now;
                                            HrisHelper.LogOperazione("Smartworking", String.Format("{0} - Inviata mail ipotesi {1} - Inizio:{2:dd/MM/yyyy} - Fine:{3:dd/MM/yyyy} - Giorni:{4} - Extra:{5}", matricola, fragili, periodoDal, periodoAl, numGiorni, numGiorniExtra, dataVisita), true);
                                        }

                                        switch (fragili)
                                        {
                                            case "I-1":
                                            case "I-3A":
                                            case "I-3B":
                                            case "I-4A":
                                            case "I-4B":
                                                internalImportType = "SW";
                                                importAccordo = "Unilaterale";
                                                internalImportClic = true;
                                                tipologia = "N";
                                                break;
                                            case "I-2":
                                            case "I-2A":
                                            case "I-2B-A":
                                            case "I-2B-B":
                                            case "I-2B-C":
                                            case "I-2C":
                                            case "I-5A":
                                            case "I-5B":
                                                internalImportType = "UPDATE_SW";
                                                importAccordo = "Consensuale";
                                                internalImportClic = false;
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        eventiMessaggi.Add(new EventoModel()
                                        {
                                            IdEvento = -70,
                                            Matricola = pMatricola,
                                            DataInizio = periodoDal.Value,
                                            DataFine = periodoAl.Value,
                                            DescrizioneEvento = "Tipologia non trovata"
                                        });
                                        row++;
                                        continue;
                                    }
                                }

                                if (internalImportType == "SW_P")
                                {
                                    if (sint.XR_STATO_RAPPORTO.Any(x => x.COD_STATO_RAPPORTO == internalImportType && x.DTA_INIZIO == periodoDal && x.DTA_FINE == periodoAl && x.VALID_DTA_END == null))
                                    {
                                        eventiMessaggi.Add(new EventoModel()
                                        {
                                            IdEvento = -7,
                                            Matricola = pMatricola,
                                            DataInizio = periodoDal.Value,
                                            DataFine = periodoAl.Value,
                                            DescrizioneEvento = "Periodo già presente"
                                        });
                                        row++;
                                        continue;
                                    }
                                    else
                                    {
                                        EventoModel evento = new EventoModel()
                                        {
                                            Codice = internalImportType,
                                            DataInizio = periodoDal.Value,
                                            DataFine = periodoAl.Value,
                                            IdPersona = sint.ID_PERSONA,
                                            Matricola = sint.COD_MATLIBROMAT,
                                            NumeroGiorniMax = numGiorni,
                                            NumeroGiorniExtra = numGiorniExtra,
                                            Tipo = TipoEvento.Stato,
                                            DataScadenza = scadenza,
                                            TipologiaAccordo = "Consensuale"
                                        };
                                        AnagraficaManager.GestisciRecordStato(evento, db, "Y");
                                        anyChanges = true;
                                    }
                                }
                                else if (internalImportType == "UPDATE_SW")
                                {
                                    //Cerco il periodo entro cui è contenuto il periodo ricevuto
                                    var rapporto = sint.XR_STATO_RAPPORTO.FirstOrDefault(x => (x.COD_STATO_RAPPORTO == "SW" || x.COD_STATO_RAPPORTO == "SW_N" || x.COD_STATO_RAPPORTO == "SW_P") && x.DTA_INIZIO <= periodoAl && periodoDal <= x.DTA_FINE && x.VALID_DTA_END == null);
                                    if (rapporto == null)
                                    {
                                        eventiMessaggi.Add(new EventoModel()
                                        {
                                            IdEvento = -80,
                                            Matricola = pMatricola,
                                            DataInizio = periodoDal.Value,
                                            DataFine = periodoAl.Value,
                                            DescrizioneEvento = "Periodo non trovato"
                                        });
                                        row++;
                                        continue;
                                    }

                                    DateTime dtaInizio = periodoDal.Value;
                                    if (dtaInizio.Day != 1)
                                        dtaInizio = dtaInizio.AddDays(-(dtaInizio.Day - 1));
                                    if (dtaInizio < rapporto.DTA_INIZIO)
                                        dtaInizio = rapporto.DTA_INIZIO;

                                    CezanneHelper.GetCampiFirma(out string cod_user, out string cod_termid, out DateTime tms_timestamp);
                                    if (rapporto.XR_STATO_RAPPORTO_INFO == null || !rapporto.XR_STATO_RAPPORTO_INFO.Any(x => x.VALID_DTA_END == null))
                                    {
                                        rapporto.XR_STATO_RAPPORTO_INFO.Add(new XR_STATO_RAPPORTO_INFO()
                                        {
                                            DTA_INIZIO = periodoDal.Value,
                                            DTA_FINE = periodoAl.Value,
                                            NUM_GIORNI_MAX = numGiorni,
                                            NUM_GIORNI_EXTRA = numGiorniExtra,
                                            VALID_DTA_INI = tms_timestamp,
                                            VALID_DTA_END = null,
                                            COD_USER = cod_user,
                                            COD_TERMID = cod_termid,
                                            TMS_TIMESTAMP = tms_timestamp,
                                            DTA_INVIO = dtaInvio,
                                            IPOTESI_FRAGILI = !String.IsNullOrWhiteSpace(fragili) ? fragili.Replace("I-", "") : null,
                                            DTA_VISITA_MEDICA = dataVisita
                                        });
                                    }
                                    else
                                    {
                                        var rifPeriod = rapporto.XR_STATO_RAPPORTO_INFO.FirstOrDefault(x => x.VALID_DTA_END == null && x.DTA_INIZIO == dtaInizio && x.DTA_FINE == periodoAl);
                                        if (rifPeriod != null)
                                        {
                                            //Se trovo lo stesso periodo è il caso semplice, annullo il vecchio (così rimane traccia)
                                            //E creo il nuovo record con le nuove date
                                            rifPeriod.VALID_DTA_END = tms_timestamp;
                                            rapporto.XR_STATO_RAPPORTO_INFO.Add(new XR_STATO_RAPPORTO_INFO()
                                            {
                                                DTA_INIZIO = periodoDal.Value,
                                                DTA_FINE = periodoAl.Value,
                                                NUM_GIORNI_MAX = numGiorni,
                                                NUM_GIORNI_EXTRA = numGiorniExtra,
                                                VALID_DTA_INI = tms_timestamp,
                                                VALID_DTA_END = null,
                                                COD_USER = cod_user,
                                                COD_TERMID = cod_termid,
                                                TMS_TIMESTAMP = tms_timestamp,
                                                DTA_INVIO = dtaInvio,
                                                IPOTESI_FRAGILI = !String.IsNullOrWhiteSpace(fragili) ? fragili.Replace("I-", "") : null,
                                                DTA_VISITA_MEDICA = dataVisita
                                            });
                                        }
                                        else
                                        {
                                            //rapporto.XR_STATO_RAPPORTO_INFO.Add(new XR_STATO_RAPPORTO_INFO()
                                            //{
                                            //    DTA_INIZIO = periodoDal.Value < rapporto.DTA_INIZIO ? rapporto.DTA_INIZIO : periodoDal.Value,
                                            //    DTA_FINE = periodoAl.Value,
                                            //    NUM_GIORNI_MAX = numGiorni,
                                            //    NUM_GIORNI_EXTRA = numGiorniExtra,
                                            //    VALID_DTA_INI = tms_timestamp,
                                            //    VALID_DTA_END = null,
                                            //    COD_USER = cod_user,
                                            //    COD_TERMID = cod_termid,
                                            //    TMS_TIMESTAMP = tms_timestamp,
                                            //    DTA_INVIO = dtaInvio,
                                            //    IPOTESI_FRAGILI = !String.IsNullOrWhiteSpace(fragili) ? fragili.Replace("I-", "") : null,
                                            //    DTA_VISITA_MEDICA = dataVisita
                                            //});

                                            var periodiACavallo = rapporto.XR_STATO_RAPPORTO_INFO.Where(x => x.VALID_DTA_END == null && x.DTA_INIZIO <= periodoAl && dtaInizio <= x.DTA_FINE).ToList();
                                            foreach (var item in periodiACavallo)
                                            {
                                                item.VALID_DTA_END = tms_timestamp;
                                                //Se il periodo è tutto contenuto, lo ricopio con i giorni approvati
                                                if (item.DTA_INIZIO >= dtaInizio && item.DTA_FINE <= periodoAl)
                                                {
                                                    rapporto.XR_STATO_RAPPORTO_INFO.Add(new myRaiDataTalentia.XR_STATO_RAPPORTO_INFO()
                                                    {
                                                        DTA_INIZIO = item.DTA_INIZIO,
                                                        DTA_FINE = item.DTA_FINE,
                                                        NUM_GIORNI_MAX = numGiorni,
                                                        NUM_GIORNI_EXTRA = numGiorniExtra,
                                                        VALID_DTA_INI = tms_timestamp,
                                                        VALID_DTA_END = null,
                                                        COD_USER = cod_user,
                                                        COD_TERMID = cod_termid,
                                                        TMS_TIMESTAMP = tms_timestamp,
                                                        DTA_INVIO = !String.IsNullOrWhiteSpace(fragili) ? dtaInvio : item.DTA_INVIO,
                                                        IPOTESI_FRAGILI = !String.IsNullOrWhiteSpace(fragili) ? fragili.Replace("I-", "") : item.IPOTESI_FRAGILI,
                                                        DTA_VISITA_MEDICA = !String.IsNullOrWhiteSpace(fragili) ? dataVisita : item.DTA_VISITA_MEDICA
                                                    });
                                                }
                                                //Se il periodo contiene tutta la richiesta, lo spezzo
                                                else if (item.DTA_INIZIO < dtaInizio && item.DTA_FINE > periodoAl)
                                                {
                                                    rapporto.XR_STATO_RAPPORTO_INFO.Add(new myRaiDataTalentia.XR_STATO_RAPPORTO_INFO()
                                                    {
                                                        DTA_INIZIO = item.DTA_INIZIO,
                                                        DTA_FINE = dtaInizio.AddDays(-1),
                                                        NUM_GIORNI_MAX = item.NUM_GIORNI_MAX,
                                                        NUM_GIORNI_EXTRA = item.NUM_GIORNI_EXTRA.GetValueOrDefault(),
                                                        VALID_DTA_INI = tms_timestamp,
                                                        VALID_DTA_END = null,
                                                        COD_USER = cod_user,
                                                        COD_TERMID = cod_termid,
                                                        TMS_TIMESTAMP = tms_timestamp,
                                                        DTA_INVIO = item.DTA_INVIO,
                                                        IPOTESI_FRAGILI = item.IPOTESI_FRAGILI,
                                                        DTA_VISITA_MEDICA = item.DTA_VISITA_MEDICA
                                                    });
                                                    rapporto.XR_STATO_RAPPORTO_INFO.Add(new myRaiDataTalentia.XR_STATO_RAPPORTO_INFO()
                                                    {
                                                        DTA_INIZIO = dtaInizio,
                                                        DTA_FINE = periodoAl,
                                                        NUM_GIORNI_MAX = numGiorni,
                                                        NUM_GIORNI_EXTRA = numGiorniExtra,
                                                        VALID_DTA_INI = tms_timestamp,
                                                        VALID_DTA_END = null,
                                                        COD_USER = cod_user,
                                                        COD_TERMID = cod_termid,
                                                        TMS_TIMESTAMP = tms_timestamp,
                                                        DTA_INVIO = !String.IsNullOrWhiteSpace(fragili) ? dtaInvio : item.DTA_INVIO,
                                                        IPOTESI_FRAGILI = !String.IsNullOrWhiteSpace(fragili) ? fragili.Replace("I-", "") : item.IPOTESI_FRAGILI,
                                                        DTA_VISITA_MEDICA = !String.IsNullOrWhiteSpace(fragili) ? dataVisita : item.DTA_VISITA_MEDICA
                                                    });
                                                    rapporto.XR_STATO_RAPPORTO_INFO.Add(new myRaiDataTalentia.XR_STATO_RAPPORTO_INFO()
                                                    {
                                                        DTA_INIZIO = periodoAl.Value.AddDays(1),
                                                        DTA_FINE = item.DTA_FINE,
                                                        NUM_GIORNI_MAX = item.NUM_GIORNI_MAX,
                                                        NUM_GIORNI_EXTRA = item.NUM_GIORNI_EXTRA,
                                                        VALID_DTA_INI = tms_timestamp,
                                                        VALID_DTA_END = null,
                                                        COD_USER = cod_user,
                                                        COD_TERMID = cod_termid,
                                                        TMS_TIMESTAMP = tms_timestamp,
                                                        DTA_INVIO = item.DTA_INVIO,
                                                        IPOTESI_FRAGILI = item.IPOTESI_FRAGILI,
                                                        DTA_VISITA_MEDICA = item.DTA_VISITA_MEDICA
                                                    });
                                                }
                                                //Se il periodo inizia prima della richiesta e finisce prima della richiesta
                                                else if (item.DTA_INIZIO < dtaInizio && item.DTA_FINE < periodoAl)
                                                {
                                                    rapporto.XR_STATO_RAPPORTO_INFO.Add(new myRaiDataTalentia.XR_STATO_RAPPORTO_INFO()
                                                    {
                                                        DTA_INIZIO = item.DTA_INIZIO,
                                                        DTA_FINE = dtaInizio.AddDays(-1),
                                                        NUM_GIORNI_MAX = item.NUM_GIORNI_MAX,
                                                        NUM_GIORNI_EXTRA = item.NUM_GIORNI_EXTRA.GetValueOrDefault(),
                                                        VALID_DTA_INI = tms_timestamp,
                                                        VALID_DTA_END = null,
                                                        COD_USER = cod_user,
                                                        COD_TERMID = cod_termid,
                                                        TMS_TIMESTAMP = tms_timestamp,
                                                        DTA_INVIO = item.DTA_INVIO,
                                                        IPOTESI_FRAGILI = item.IPOTESI_FRAGILI,
                                                        DTA_VISITA_MEDICA = item.DTA_VISITA_MEDICA
                                                    });
                                                    rapporto.XR_STATO_RAPPORTO_INFO.Add(new myRaiDataTalentia.XR_STATO_RAPPORTO_INFO()
                                                    {
                                                        DTA_INIZIO = dtaInizio,
                                                        DTA_FINE = item.DTA_FINE,
                                                        NUM_GIORNI_MAX = numGiorni,
                                                        NUM_GIORNI_EXTRA = numGiorniExtra,
                                                        VALID_DTA_INI = tms_timestamp,
                                                        VALID_DTA_END = null,
                                                        COD_USER = cod_user,
                                                        COD_TERMID = cod_termid,
                                                        TMS_TIMESTAMP = tms_timestamp,
                                                        DTA_INVIO = !String.IsNullOrWhiteSpace(fragili) ? dtaInvio : item.DTA_INVIO,
                                                        IPOTESI_FRAGILI = !String.IsNullOrWhiteSpace(fragili) ? fragili.Replace("I-", "") : item.IPOTESI_FRAGILI,
                                                        DTA_VISITA_MEDICA = !String.IsNullOrWhiteSpace(fragili) ? dataVisita : item.DTA_VISITA_MEDICA
                                                    });
                                                    rapporto.XR_STATO_RAPPORTO_INFO.Add(new myRaiDataTalentia.XR_STATO_RAPPORTO_INFO()
                                                    {
                                                        DTA_INIZIO = item.DTA_FINE.Value.AddDays(1),
                                                        DTA_FINE = periodoAl,
                                                        NUM_GIORNI_MAX = numGiorni,
                                                        NUM_GIORNI_EXTRA = numGiorniExtra,
                                                        VALID_DTA_INI = tms_timestamp,
                                                        VALID_DTA_END = null,
                                                        COD_USER = cod_user,
                                                        COD_TERMID = cod_termid,
                                                        TMS_TIMESTAMP = tms_timestamp,
                                                        DTA_INVIO = !String.IsNullOrWhiteSpace(fragili) ? dtaInvio : item.DTA_INVIO,
                                                        IPOTESI_FRAGILI = !String.IsNullOrWhiteSpace(fragili) ? fragili.Replace("I-", "") : item.IPOTESI_FRAGILI,
                                                        DTA_VISITA_MEDICA = !String.IsNullOrWhiteSpace(fragili) ? dataVisita : item.DTA_VISITA_MEDICA
                                                    });
                                                }
                                                //Se il periodo inizia prima della richiesta ma finisce lo stesso giorno
                                                else if (item.DTA_INIZIO < dtaInizio && item.DTA_FINE == periodoAl)
                                                {
                                                    rapporto.XR_STATO_RAPPORTO_INFO.Add(new myRaiDataTalentia.XR_STATO_RAPPORTO_INFO()
                                                    {
                                                        DTA_INIZIO = item.DTA_INIZIO,
                                                        DTA_FINE = dtaInizio.AddDays(-1),
                                                        NUM_GIORNI_MAX = item.NUM_GIORNI_MAX,
                                                        NUM_GIORNI_EXTRA = item.NUM_GIORNI_EXTRA.GetValueOrDefault(),
                                                        VALID_DTA_INI = tms_timestamp,
                                                        VALID_DTA_END = null,
                                                        COD_USER = cod_user,
                                                        COD_TERMID = cod_termid,
                                                        TMS_TIMESTAMP = tms_timestamp,
                                                        DTA_INVIO = item.DTA_INVIO,
                                                        IPOTESI_FRAGILI = item.IPOTESI_FRAGILI,
                                                        DTA_VISITA_MEDICA = item.DTA_VISITA_MEDICA
                                                    });
                                                    rapporto.XR_STATO_RAPPORTO_INFO.Add(new myRaiDataTalentia.XR_STATO_RAPPORTO_INFO()
                                                    {
                                                        DTA_INIZIO = dtaInizio,
                                                        DTA_FINE = periodoAl,
                                                        NUM_GIORNI_MAX = numGiorni,
                                                        NUM_GIORNI_EXTRA = numGiorniExtra,
                                                        VALID_DTA_INI = tms_timestamp,
                                                        VALID_DTA_END = null,
                                                        COD_USER = cod_user,
                                                        COD_TERMID = cod_termid,
                                                        TMS_TIMESTAMP = tms_timestamp,
                                                        DTA_INVIO = item.DTA_INVIO,
                                                        IPOTESI_FRAGILI = item.IPOTESI_FRAGILI,
                                                        DTA_VISITA_MEDICA = item.DTA_VISITA_MEDICA
                                                    });
                                                }
                                                else if (item.DTA_INIZIO >= dtaInizio && item.DTA_FINE > periodoAl)
                                                {
                                                    rapporto.XR_STATO_RAPPORTO_INFO.Add(new myRaiDataTalentia.XR_STATO_RAPPORTO_INFO()
                                                    {
                                                        DTA_INIZIO = item.DTA_INIZIO,
                                                        DTA_FINE = periodoAl,
                                                        NUM_GIORNI_MAX = numGiorni,
                                                        NUM_GIORNI_EXTRA = numGiorniExtra,
                                                        VALID_DTA_INI = tms_timestamp,
                                                        VALID_DTA_END = null,
                                                        COD_USER = cod_user,
                                                        COD_TERMID = cod_termid,
                                                        TMS_TIMESTAMP = tms_timestamp,
                                                        DTA_INVIO = item.DTA_INVIO,
                                                        IPOTESI_FRAGILI = item.IPOTESI_FRAGILI,
                                                        DTA_VISITA_MEDICA = item.DTA_VISITA_MEDICA
                                                    });
                                                    rapporto.XR_STATO_RAPPORTO_INFO.Add(new myRaiDataTalentia.XR_STATO_RAPPORTO_INFO()
                                                    {
                                                        DTA_INIZIO = periodoAl.Value.AddDays(1),
                                                        DTA_FINE = item.DTA_FINE,
                                                        NUM_GIORNI_MAX = item.NUM_GIORNI_MAX,
                                                        NUM_GIORNI_EXTRA = item.NUM_GIORNI_EXTRA.GetValueOrDefault(),
                                                        VALID_DTA_INI = tms_timestamp,
                                                        VALID_DTA_END = null,
                                                        COD_USER = cod_user,
                                                        COD_TERMID = cod_termid,
                                                        TMS_TIMESTAMP = tms_timestamp,
                                                        DTA_INVIO = item.DTA_INVIO,
                                                        IPOTESI_FRAGILI = item.IPOTESI_FRAGILI,
                                                        DTA_VISITA_MEDICA = item.DTA_VISITA_MEDICA
                                                    });
                                                }
                                                else
                                                {
                                                    eventiMessaggi.Add(new EventoModel()
                                                    {
                                                        IdEvento = -85,
                                                        Matricola = pMatricola,
                                                        DataInizio = periodoDal.Value,
                                                        DataFine = periodoAl.Value,
                                                        DescrizioneEvento = "Errore nella gestione del periodo"
                                                    });
                                                    row++;
                                                    continue;
                                                }
                                            }

                                        }
                                    }


                                }
                                else
                                {
                                    XR_STATO_RAPPORTO record = null;
                                    CezanneHelper.GetCampiFirma(out string cod_user, out string cod_termid, out DateTime tms_timestamp);
                                    if (sint.XR_STATO_RAPPORTO == null || sint.XR_STATO_RAPPORTO.Where(x => x.COD_STATO_RAPPORTO == internalImportType && x.COD_TIPO_ACCORDO == importAccordo && x.VALID_DTA_END == null).Count() == 0)
                                    {
                                        var newRecord = new myRaiDataTalentia.XR_STATO_RAPPORTO
                                        {
                                            ID_STATO_RAPPORTO = db.XR_STATO_RAPPORTO.GeneraPrimaryKey(),
                                            ID_PERSONA = sint.ID_PERSONA,
                                            MATRICOLA = sint.COD_MATLIBROMAT,
                                            COD_STATO_RAPPORTO = internalImportType,
                                            DTA_INIZIO = periodoDal.Value,
                                            DTA_FINE = periodoAl.Value,
                                            COD_TIPO_ACCORDO = importAccordo,
                                            IND_AUTOM = "Y"
                                        };
                                        newRecord.COD_USER = cod_user;
                                        newRecord.COD_TERMID = cod_termid;
                                        newRecord.TMS_TIMESTAMP = tms_timestamp;
                                        newRecord.VALID_DTA_INI = tms_timestamp;
                                        if (!importClic)
                                        {
                                            newRecord.DTA_NOTIF_ENTE = tms_timestamp;
                                            newRecord.NOT_NOTA = "Da non notificare a ClicLavoro";
                                        }

                                        db.XR_STATO_RAPPORTO.Add(newRecord);
                                        record = newRecord;
                                        anyChanges = true;
                                    }
                                    else
                                    {
                                        //Se una proroga o rientro in servizio, bisogna aggiornare l'ultimo record utile
                                        //Negli altri casi, ricollocazione o nuova collocazione, bisogna creare un nuovo record,
                                        //dopo aver controllato se c'è un periodo contiguo, e quindi farlo diventare una proroga

                                        //Verifica esistenza periodo con stesse date
                                        int idRif = 0;
                                        if (sint.XR_STATO_RAPPORTO.Any(x => x.COD_STATO_RAPPORTO == internalImportType && x.COD_TIPO_ACCORDO == importAccordo && x.DTA_INIZIO == periodoDal && x.DTA_FINE == periodoAl && x.VALID_DTA_END == null))
                                        {
                                            eventiMessaggi.Add(new EventoModel()
                                            {
                                                IdEvento = -7,
                                                Matricola = pMatricola,
                                                DataInizio = periodoDal.Value,
                                                DataFine = periodoAl.Value,
                                                DescrizioneEvento = "Periodo già presente"
                                            });
                                            row++;

                                            //Potrebbe essere un aggiornamento di giorni, per cui invece che uscire reperisce il record e lo aggiorna
                                            record = sint.XR_STATO_RAPPORTO.FirstOrDefault(x => x.COD_STATO_RAPPORTO == internalImportType && x.COD_TIPO_ACCORDO == importAccordo && x.DTA_INIZIO == periodoDal && x.DTA_FINE == periodoAl && x.VALID_DTA_END == null);
                                            //continue;
                                            internalImportClic = false;
                                        }
                                        else if (tipologia == "P")
                                        {
                                            record = sint.XR_STATO_RAPPORTO.Where(x => x.COD_STATO_RAPPORTO == internalImportType && x.VALID_DTA_END == null).OrderByDescending(x => x.DTA_INIZIO).FirstOrDefault();
                                            if (periodoAl < record.DTA_INIZIO)
                                            {
                                                eventiMessaggi.Add(new EventoModel()
                                                {
                                                    IdEvento = -4,
                                                    Matricola = pMatricola,
                                                    DataInizio = periodoDal.Value,
                                                    DataFine = periodoAl.Value,
                                                    DescrizioneEvento = "Data fine antecedente a data inizio corrente"
                                                });
                                                row++;
                                                continue;
                                            }

                                            record.IND_AUTOM = "Y";
                                            record.DTA_FINE = periodoAl.Value;
                                            record.COD_USER = cod_user;
                                            record.COD_TERMID = cod_termid;
                                            record.TMS_TIMESTAMP = tms_timestamp;
                                            anyChanges = true;
                                        }
                                        else
                                        {
                                            //record = sint.XR_STATO_RAPPORTO.FirstOrDefault(x => x.COD_STATO_RAPPORTO == internalImportType && (x.DTA_FINE == periodoDal || x.DTA_FINE == periodoDal.Value.AddDays(-1)));
                                            //if (record != null)
                                            //{
                                            //    if (periodoAl < record.DTA_INIZIO)
                                            //    {
                                            //        eventiMessaggi.Add(new EventoModel()
                                            //        {
                                            //            IdEvento = -5,
                                            //            Matricola = pMatricola,
                                            //            DataInizio = periodoDal.Value,
                                            //            DataFine = periodoAl.Value,
                                            //            DescrizioneEvento = "Data fine antecedente a data inizio corrente"
                                            //        });
                                            //        row++;
                                            //        continue;
                                            //    }

                                            //    record.IND_AUTOM = "Y";
                                            //    record.DTA_FINE = periodoAl.Value;
                                            //    record.COD_USER = cod_user;
                                            //    record.COD_TERMID = cod_termid;
                                            //    record.TMS_TIMESTAMP = tms_timestamp;
                                            //    anyChanges = true;
                                            //}
                                            //else 
                                            if (sint.XR_STATO_RAPPORTO.Any(x => x.COD_STATO_RAPPORTO == internalImportType && x.DTA_INIZIO == periodoDal && periodoAl != x.DTA_FINE && x.VALID_DTA_END == null))
                                            {
                                                record = sint.XR_STATO_RAPPORTO.FirstOrDefault(x => x.COD_STATO_RAPPORTO == internalImportType && x.DTA_INIZIO == periodoDal && periodoAl != x.DTA_FINE && x.VALID_DTA_END == null);
                                                record.IND_AUTOM = "Y";
                                                record.DTA_FINE = periodoAl.Value;
                                                record.COD_USER = cod_user;
                                                record.COD_TERMID = cod_termid;
                                                record.TMS_TIMESTAMP = tms_timestamp;
                                                anyChanges = true;
                                            }
                                            else
                                            {
                                                //controllo per verificare la presenza di periodi sovrapposti
                                                if (sint.XR_STATO_RAPPORTO.Any(x => x.COD_STATO_RAPPORTO == internalImportType && x.DTA_INIZIO <= periodoAl && periodoDal <= x.DTA_FINE && x.VALID_DTA_END == null))
                                                {
                                                    eventiMessaggi.Add(new EventoModel()
                                                    {
                                                        IdEvento = -6,
                                                        Matricola = pMatricola,
                                                        DataInizio = periodoDal.Value,
                                                        DataFine = periodoAl.Value,
                                                        DescrizioneEvento = "Periodo sovrapposto a periodo esistente"
                                                    });
                                                    row++;
                                                    continue;
                                                }

                                                var newRecord = new myRaiDataTalentia.XR_STATO_RAPPORTO
                                                {
                                                    ID_STATO_RAPPORTO = db.XR_STATO_RAPPORTO.GeneraPrimaryKey(),
                                                    ID_PERSONA = sint.ID_PERSONA,
                                                    MATRICOLA = sint.COD_MATLIBROMAT,
                                                    COD_STATO_RAPPORTO = internalImportType,
                                                    DTA_INIZIO = periodoDal.Value,
                                                    DTA_FINE = periodoAl.Value,
                                                    COD_TIPO_ACCORDO = importAccordo,
                                                    IND_AUTOM = "Y"
                                                };

                                                newRecord.COD_USER = cod_user;
                                                newRecord.COD_TERMID = cod_termid;
                                                newRecord.TMS_TIMESTAMP = tms_timestamp;
                                                newRecord.VALID_DTA_INI = tms_timestamp;
                                                db.XR_STATO_RAPPORTO.Add(newRecord);
                                                record = newRecord;
                                                anyChanges = true;
                                            }
                                        }
                                    }

                                    if (record.XR_STATO_RAPPORTO_INFO != null && numGiorni.HasValue)
                                    {
                                        foreach (var item in record.XR_STATO_RAPPORTO_INFO)
                                            item.VALID_DTA_END = DateTime.Now;
                                    }

                                    if (numGiorni.HasValue)
                                    {
                                        record.XR_STATO_RAPPORTO_INFO.Add(new XR_STATO_RAPPORTO_INFO()
                                        {
                                            DTA_INIZIO = periodoDal.Value,
                                            DTA_FINE = periodoAl.Value,
                                            NUM_GIORNI_MAX = numGiorni,
                                            NUM_GIORNI_EXTRA = numGiorniExtra,
                                            VALID_DTA_INI = tms_timestamp,
                                            VALID_DTA_END = null,
                                            COD_USER = cod_user,
                                            COD_TERMID = cod_termid,
                                            TMS_TIMESTAMP = tms_timestamp,
                                            DTA_INVIO = dtaInvio,
                                            IPOTESI_FRAGILI = !String.IsNullOrWhiteSpace(fragili) ? fragili.Replace("I-", "") : null,
                                            DTA_VISITA_MEDICA = dataVisita
                                        });
                                    }

                                    if (internalImportType == "SW" && internalImportClic && sint != null && sint.COD_IMPRESACR == "0")
                                    {
                                        //scrivo il record su MyRai_Importazioni
                                        var toAdd = new myRaiData.MyRai_Importazioni();
                                        toAdd.Matricola = "ImportaProrogheSWDaCSV";
                                        toAdd.Tabella = "ProrogaModuloSmartWorking2020";
                                        toAdd.Parametro1 = pMatricola;
                                        toAdd.Parametro2 = nominativo;
                                        toAdd.Parametro3 = direzione;
                                        toAdd.Parametro4 = periodoDal.Value.ToString("dd/MM/yyyy");
                                        toAdd.Parametro5 = periodoAl.Value.ToString("dd/MM/yyyy");
                                        toAdd.Parametro6 = "";
                                        toAdd.Parametro7 = tipologia;
                                        toAdd.Parametro8 = "";
                                        toAdd.Parametro9 = periodoDal.Value.ToString("dd/MM/yyyy");
                                        toAdd.Parametro10 = periodoAl.Value.ToString("dd/MM/yyyy");
                                        toAdd.Parametro12 = "";
                                        toAdd.Parametro13 = null;
                                        toAdd.Parametro15 = "HRIS_AttivaSW_" + record.ID_STATO_RAPPORTO;
                                        dbDigi.MyRai_Importazioni.Add(toAdd);
                                    }

                                    if (internalImportClic)
                                        record.DTA_NOTIF_ENTE = null;
                                    else
                                        record.DTA_NOTIF_ENTE = record.DTA_NOTIF_ENTE ?? DateTime.Now;
                                }

                                row++;

                                if (!DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola(), "Import stati rapporto"))
                                {
                                    eventiMessaggi.Add(new EventoModel()
                                    {
                                        IdEvento = -99,
                                        Matricola = pMatricola,
                                        DataInizio = periodoDal.Value,
                                        DataFine = periodoAl.Value,
                                        DescrizioneEvento = "Errore durante il salvataggio"
                                    });
                                }
                                else if (!DBHelper.Save(dbDigi, CommonHelper.GetCurrentUserMatricola()))
                                {
                                    eventiMessaggi.Add(new EventoModel()
                                    {
                                        IdEvento = -99,
                                        Matricola = pMatricola,
                                        DataInizio = periodoDal.Value,
                                        DataFine = periodoAl.Value,
                                        DescrizioneEvento = "Errore durante la predisposizione per ClicLavoro"
                                    });
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            eventiMessaggi.Add(new EventoModel()
                            {
                                IdEvento = -98,
                                Matricola = pMatricola,
                                DataInizio = periodoDal.GetValueOrDefault(),
                                DataFine = periodoAl.GetValueOrDefault(),
                                DescrizioneEvento = "Errore nei dati"
                            });
                            row++;
                        }

                    }
                }
            }
            else
            {
                errorMsg = "Nessun file da caricare trovato";
            }

            if (!String.IsNullOrWhiteSpace(errorMsg))
            {
                eventiMessaggi.Add(new EventoModel()
                {
                    IdEvento = -1,
                    DescrizioneEvento = errorMsg
                });
            }
            else
            {
                eventiMessaggi.Add(new EventoModel()
                {
                    IdEvento = 0,
                    DescrizioneEvento = "Importazione completata"
                });
            }

            //return View("_Import_Preview_Dati", eventiMessaggi);
            return new JsonResult() { Data = eventiMessaggi, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public ActionResult Modal_Recesso(int idPersona, int idEvento)
        {
            var dbTal = new TalentiaEntities();
            var dbCzn = new IncentiviEntities();

            XR_STATO_RAPPORTO rapp = dbTal.XR_STATO_RAPPORTO.Find(idEvento);
            XR_WKF_RICHIESTE richRecesso = null;
            if (rapp.ID_RICH_RECESSO.GetValueOrDefault() > 0)
                richRecesso = dbCzn.XR_WKF_RICHIESTE.FirstOrDefault(x => x.XR_WKF_TIPOLOGIA.COD_TIPOLOGIA == "SW_RECESSO" && x.ID_GESTIONE == rapp.ID_RICH_RECESSO.Value);
            else
                richRecesso = new XR_WKF_RICHIESTE() { ID_PERSONA = idPersona };

            Recesso recesso = new Recesso(richRecesso, true);
            recesso.Rapporto = rapp;
            recesso.Modulo = dbTal.XR_MOD_DIPENDENTI.Find(rapp.ID_MOD_DIPENDENTI);

            return View("Modal_Recesso", recesso);
        }

        [HttpPost]
        public ActionResult SalvaRecesso(Recesso model)
        {
            bool result = false;
            string errorMsg = null;
            var dbTal = new TalentiaEntities();
            var dbCzn = new IncentiviEntities();

            XR_STATO_RAPPORTO rapp = dbTal.XR_STATO_RAPPORTO.Find(model.Rapporto.ID_STATO_RAPPORTO);
            XR_WKF_TIPOLOGIA tipologia = dbCzn.XR_WKF_TIPOLOGIA.FirstOrDefault(x => x.COD_TIPOLOGIA == "SW_RECESSO");

            //Questa action viene richiamata in fase di creazione
            CezanneHelper.GetCampiFirma(out var campiFirma);
            XR_WKF_RICHIESTE rich = new XR_WKF_RICHIESTE()
            {
                DTA_CREAZIONE = campiFirma.Timestamp,
                ID_TIPOLOGIA = tipologia.ID_TIPOLOGIA,
                MATRICOLA = rapp.MATRICOLA,
                ID_PERSONA = rapp.ID_PERSONA,
                MODELLO = null,
                COD_TIPO = "SW Recesso",
                COD_USER = campiFirma.CodUser,
                COD_TERMID = campiFirma.CodTermid,
                TMS_TIMESTAMP = campiFirma.Timestamp
            };
            rich.SetField("Tipologia", (int)model.Tipologia, campiFirma);
            rich.SetField("Nota", model.Nota, campiFirma);
            rich.SetField("RichiestaMatr", CommonHelper.GetCurrentUserMatricola(), campiFirma);
            rich.SetField("Approvato", true, campiFirma);
            rich.SetField("ApprData", campiFirma.Timestamp, campiFirma);
            rich.SetField("Provenienza", (int)CommonHelper.GetApplicationType(), campiFirma);

            //Generazione pdf
            if (model.Tipologia == TipoRecesso.GiustificatoMotivo)
            {
                rich.SetTemplate("SW_Recesso", "ApprPdf", null, campiFirma);
            }

            WorkflowHelper.AddStatoBis(dbCzn, rich, 10);
            WorkflowHelper.AddStatoBis(dbCzn, rich, 20);

            dbCzn.XR_WKF_RICHIESTE.Add(rich);

            int newId = 0;
            if (DBHelper.Save(dbCzn, CommonHelper.GetCurrentUserMatricola(), "Creazione richiesta recesso"))
            {
                DateTime dataFine = rapp.DTA_FINE;
                //aggiornamento giorni
                switch (model.Tipologia)
                {
                    case TipoRecesso.Nessuna:
                        break;
                    case TipoRecesso.Ordinario:
                        dataFine = DateTime.Today.AddDays(70);
                        break;
                    case TipoRecesso.GiustificatoMotivo:
                        dataFine = DateTime.Today.AddDays(10);
                        break;
                    default:
                        break;
                }

                rapp.VALID_DTA_END = campiFirma.Timestamp;
                XR_STATO_RAPPORTO newRapp = new XR_STATO_RAPPORTO()
                {
                    ID_STATO_RAPPORTO = dbTal.XR_STATO_RAPPORTO.GeneraPrimaryKey(),
                    ID_PERSONA = rapp.ID_PERSONA,
                    MATRICOLA = rapp.MATRICOLA,
                    COD_STATO_RAPPORTO = rapp.COD_STATO_RAPPORTO,
                    DTA_INIZIO = rapp.DTA_INIZIO,
                    DTA_FINE = dataFine,
                    COD_TIPO_ACCORDO = rapp.COD_TIPO_ACCORDO,
                    VALID_DTA_INI = campiFirma.Timestamp,
                    DTA_NOTIF_DIP = rapp.DTA_NOTIF_DIP,
                    IND_AUTOM = "N",
                    ID_STATO_RAPPORTO_ORIG = rapp.ID_STATO_RAPPORTO,
                    DTA_SCADENZA = rapp.DTA_SCADENZA,
                    DTA_INIZIO_VISUALIZZAZIONE = rapp.DTA_INIZIO_VISUALIZZAZIONE,
                    FLG_FORZA_INIZIO_ACCORDO = rapp.FLG_FORZA_INIZIO_ACCORDO,
                    ID_RICH_RECESSO = rich.ID_GESTIONE,
                    COD_USER = campiFirma.CodUser,
                    COD_TERMID = campiFirma.CodTermid,
                    TMS_TIMESTAMP = campiFirma.Timestamp
                };

                var listInfo = rapp.XR_STATO_RAPPORTO_INFO.Where(x => x.VALID_DTA_END == null).ToList();
                foreach (var info in listInfo)
                {
                    if (info.DTA_FINE <= dataFine)
                    {
                        newRapp.XR_STATO_RAPPORTO_INFO.Add(new XR_STATO_RAPPORTO_INFO()
                        {
                            COD_TERMID = campiFirma.CodTermid,
                            COD_USER = campiFirma.CodUser,
                            DTA_FINE = info.DTA_FINE,
                            DTA_INIZIO = info.DTA_INIZIO,
                            NUM_GIORNI_EXTRA = info.NUM_GIORNI_EXTRA,
                            NUM_GIORNI_MAX = info.NUM_GIORNI_MAX,
                            NUM_GIORNI_MIN = info.NUM_GIORNI_MIN,
                            TMS_TIMESTAMP = info.TMS_TIMESTAMP,
                            VALID_DTA_END = info.VALID_DTA_END,
                            VALID_DTA_INI = info.VALID_DTA_INI,
                            DTA_INVIO = info.DTA_INVIO,
                            DTA_VISITA_MEDICA = info.DTA_VISITA_MEDICA,
                            ID_RICH = info.ID_RICH,
                            IPOTESI_FRAGILI = info.IPOTESI_FRAGILI
                        });
                    }
                    else if (info.DTA_INIZIO < dataFine && dataFine < info.DTA_FINE)
                    {
                        newRapp.XR_STATO_RAPPORTO_INFO.Add(new XR_STATO_RAPPORTO_INFO()
                        {
                            COD_TERMID = campiFirma.CodTermid,
                            COD_USER = campiFirma.CodUser,
                            DTA_FINE = dataFine,
                            DTA_INIZIO = info.DTA_INIZIO,
                            NUM_GIORNI_EXTRA = info.NUM_GIORNI_EXTRA,
                            NUM_GIORNI_MAX = info.NUM_GIORNI_MAX,
                            NUM_GIORNI_MIN = info.NUM_GIORNI_MIN,
                            TMS_TIMESTAMP = info.TMS_TIMESTAMP,
                            VALID_DTA_END = null,
                            VALID_DTA_INI = info.VALID_DTA_INI,
                            DTA_INVIO = info.DTA_INVIO,
                            DTA_VISITA_MEDICA = info.DTA_VISITA_MEDICA,
                            ID_RICH = info.ID_RICH,
                            IPOTESI_FRAGILI = info.IPOTESI_FRAGILI
                        });
                    }
                    else
                    {
                        //Non lo deve riportare
                    }
                }
                dbTal.XR_STATO_RAPPORTO.Add(newRapp);

                if (DBHelper.Save(dbTal, CommonHelper.GetCurrentUserMatricola(), "Aggiornamento rapporto con richiesta recesso"))
                {
                    result = true;
                    newId = newRapp.ID_STATO_RAPPORTO;
                }
                else
                    errorMsg = "Errore durante l'aggiornamento dello smart working";
            }
            else
                errorMsg = "Errore durante la creazione della richiesta";

            return Json(new { esito = result, errorMsg = errorMsg, id_stato_rapporto = newId }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DownloadRecesso(int idRecesso)
        {
            var db = new IncentiviEntities();
            XR_WKF_TIPOLOGIA tipologia = db.XR_WKF_TIPOLOGIA.FirstOrDefault(x => x.COD_TIPOLOGIA == "SW_RECESSO");
            var rich = db.XR_WKF_RICHIESTE.Find(idRecesso, tipologia.ID_TIPOLOGIA);
            byte[] modulo = rich.GetTemplate("SW_Recesso", "ApprPdf");
            return File(modulo, "application/pdf", "Recesso " + CommonHelper.GetNominativoPerMatricola(rich.MATRICOLA) + ".pdf");
        }
        public ActionResult DownloadRecessoFirmato(int idRecesso)
        {
            var db = new IncentiviEntities();
            XR_WKF_TIPOLOGIA tipologia = db.XR_WKF_TIPOLOGIA.FirstOrDefault(x => x.COD_TIPOLOGIA == "SW_RECESSO");
            var rich = db.XR_WKF_RICHIESTE.Find(idRecesso, tipologia.ID_TIPOLOGIA);
            byte[] modulo = rich.GetTemplate("SW_Recesso", "RicevutaPdf");
            return File(modulo, "application/pdf", "Recesso controfirmato " + CommonHelper.GetNominativoPerMatricola(rich.MATRICOLA) + ".pdf");
        }
        public ActionResult UploadControfirmato(HttpPostedFileBase _file, int _idRIch)
        {
            var db = new IncentiviEntities();
            XR_WKF_TIPOLOGIA tipologia = db.XR_WKF_TIPOLOGIA.FirstOrDefault(x => x.COD_TIPOLOGIA == "SW_RECESSO");
            var rich = db.XR_WKF_RICHIESTE.Find(_idRIch, tipologia.ID_TIPOLOGIA);
            byte[] modulo = null;
            if (_file != null)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    _file.InputStream.CopyTo(ms);
                    modulo = ms.ToArray();
                }
            }
            CezanneHelper.GetCampiFirma(out var campiFirma);
            rich.SetField("RicevutaData", campiFirma.Timestamp, campiFirma);
            rich.SetTemplate("SW_Recesso", "RicevutaPdf", modulo, campiFirma);
            if (DBHelper.Save(db, CommonHelper.GetCurrentUserMatricola()))
                return Content("OK");
            else
                return Content("Errore durante l'upload");
        }
        public ActionResult ApprovaRecesso(Recesso model)
        {
            bool result = false;
            string errorMsg = null;
            var dbTal = new TalentiaEntities();
            var dbCzn = new IncentiviEntities();

            XR_STATO_RAPPORTO rapp = dbTal.XR_STATO_RAPPORTO.Find(model.Rapporto.ID_STATO_RAPPORTO);
            XR_WKF_TIPOLOGIA tipologia = dbCzn.XR_WKF_TIPOLOGIA.FirstOrDefault(x => x.COD_TIPOLOGIA == "SW_RECESSO");
            XR_WKF_RICHIESTE rich = dbCzn.XR_WKF_RICHIESTE.Find(model.Richiesta.ID_GESTIONE, tipologia.ID_TIPOLOGIA);

            int stato = 20;
            if (!model.Approvato.GetValueOrDefault())
                stato = 30;

            CezanneHelper.GetCampiFirma(out var campiFirma);
            WorkflowHelper.AddStatoBis(dbCzn, rich, stato);
            rich.SetField("Approvato", model.Approvato, campiFirma);
            rich.SetField("ApprData", campiFirma.Timestamp, campiFirma);
            rich.SetField("NotaApprovazione", model.NotaApprovazione, campiFirma);

            int newId = 0;
            if (DBHelper.Save(dbCzn, CommonHelper.GetCurrentUserMatricola(), "Creazione richiesta recesso"))
            {
                if (!model.Approvato.GetValueOrDefault())
                    newId = model.Rapporto.ID_STATO_RAPPORTO;
                else
                {
                    DateTime dataFine = rapp.DTA_FINE;
                    //aggiornamento giorni
                    switch (model.Tipologia)
                    {
                        case TipoRecesso.Nessuna:
                            break;
                        case TipoRecesso.Ordinario:
                            dataFine = DateTime.Today.AddDays(70);
                            break;
                        case TipoRecesso.GiustificatoMotivo:
                            dataFine = DateTime.Today.AddDays(10);
                            break;
                        default:
                            break;
                    }

                    rapp.VALID_DTA_END = campiFirma.Timestamp;
                    XR_STATO_RAPPORTO newRapp = new XR_STATO_RAPPORTO()
                    {
                        ID_STATO_RAPPORTO = dbTal.XR_STATO_RAPPORTO.GeneraPrimaryKey(),
                        ID_PERSONA = rapp.ID_PERSONA,
                        MATRICOLA = rapp.MATRICOLA,
                        COD_STATO_RAPPORTO = rapp.COD_STATO_RAPPORTO,
                        DTA_INIZIO = rapp.DTA_INIZIO,
                        DTA_FINE = dataFine,
                        COD_TIPO_ACCORDO = rapp.COD_TIPO_ACCORDO,
                        VALID_DTA_INI = campiFirma.Timestamp,
                        DTA_NOTIF_DIP = rapp.DTA_NOTIF_DIP,
                        IND_AUTOM = "N",
                        ID_STATO_RAPPORTO_ORIG = rapp.ID_STATO_RAPPORTO,
                        DTA_SCADENZA = rapp.DTA_SCADENZA,
                        DTA_INIZIO_VISUALIZZAZIONE = rapp.DTA_INIZIO_VISUALIZZAZIONE,
                        FLG_FORZA_INIZIO_ACCORDO = rapp.FLG_FORZA_INIZIO_ACCORDO,
                        ID_RICH_RECESSO = rich.ID_GESTIONE,
                        COD_USER = campiFirma.CodUser,
                        COD_TERMID = campiFirma.CodTermid,
                        TMS_TIMESTAMP = campiFirma.Timestamp
                    };

                    var listInfo = rapp.XR_STATO_RAPPORTO_INFO.Where(x => x.VALID_DTA_END == null).ToList();
                    foreach (var info in listInfo)
                    {
                        if (info.DTA_FINE <= dataFine)
                        {
                            newRapp.XR_STATO_RAPPORTO_INFO.Add(new XR_STATO_RAPPORTO_INFO()
                            {
                                COD_TERMID = campiFirma.CodTermid,
                                COD_USER = campiFirma.CodUser,
                                DTA_FINE = info.DTA_FINE,
                                DTA_INIZIO = info.DTA_INIZIO,
                                NUM_GIORNI_EXTRA = info.NUM_GIORNI_EXTRA,
                                NUM_GIORNI_MAX = info.NUM_GIORNI_MAX,
                                NUM_GIORNI_MIN = info.NUM_GIORNI_MIN,
                                TMS_TIMESTAMP = info.TMS_TIMESTAMP,
                                VALID_DTA_END = info.VALID_DTA_END,
                                VALID_DTA_INI = info.VALID_DTA_INI,
                                DTA_INVIO = info.DTA_INVIO,
                                DTA_VISITA_MEDICA = info.DTA_VISITA_MEDICA,
                                ID_RICH = info.ID_RICH,
                                IPOTESI_FRAGILI = info.IPOTESI_FRAGILI
                            });
                        }
                        else if (info.DTA_INIZIO < dataFine && dataFine < info.DTA_FINE)
                        {
                            newRapp.XR_STATO_RAPPORTO_INFO.Add(new XR_STATO_RAPPORTO_INFO()
                            {
                                COD_TERMID = campiFirma.CodTermid,
                                COD_USER = campiFirma.CodUser,
                                DTA_FINE = dataFine,
                                DTA_INIZIO = info.DTA_INIZIO,
                                NUM_GIORNI_EXTRA = info.NUM_GIORNI_EXTRA,
                                NUM_GIORNI_MAX = info.NUM_GIORNI_MAX,
                                NUM_GIORNI_MIN = info.NUM_GIORNI_MIN,
                                TMS_TIMESTAMP = info.TMS_TIMESTAMP,
                                VALID_DTA_END = null,
                                VALID_DTA_INI = info.VALID_DTA_INI,
                                DTA_INVIO = info.DTA_INVIO,
                                DTA_VISITA_MEDICA = info.DTA_VISITA_MEDICA,
                                ID_RICH = info.ID_RICH,
                                IPOTESI_FRAGILI = info.IPOTESI_FRAGILI
                            });
                        }
                        else
                        {
                            //Non lo deve riportare
                        }
                    }
                    dbTal.XR_STATO_RAPPORTO.Add(newRapp);

                    if (DBHelper.Save(dbTal, CommonHelper.GetCurrentUserMatricola(), "Aggiornamento rapporto con richiesta recesso"))
                    {
                        result = true;
                        newId = newRapp.ID_STATO_RAPPORTO;
                    }
                    else
                        errorMsg = "Errore durante l'aggiornamento dello smart working";
                }
            }
            else
                errorMsg = "Errore durante la creazione della richiesta";


            return Json(new { esito = result, errorMsg = errorMsg, id_stato_rapporto = newId }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ConcludiRecesso(int idRich)
        {
            bool result = false;
            string errorMsg = null;
            var dbCzn = new IncentiviEntities();

            XR_WKF_TIPOLOGIA tipologia = dbCzn.XR_WKF_TIPOLOGIA.FirstOrDefault(x => x.COD_TIPOLOGIA == "SW_RECESSO");
            var rich = dbCzn.XR_WKF_RICHIESTE.Find(idRich, tipologia.ID_TIPOLOGIA);
            WorkflowHelper.AddStatoBis(dbCzn, rich, 100);

            result = DBHelper.Save(dbCzn, CommonHelper.GetCurrentUserMatricola(), "Conclusione richiesta recesso");
            if (!result)
                errorMsg = "Errore durante il salvataggio dei dati";

            return Json(new { esito = result, errorMsg = errorMsg }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RaccoltaClic()
        {
            var db = new TalentiaEntities();
            var dbDigi = new myRaiData.digiGappEntities();
            var records = db.XR_STATO_RAPPORTO.Include("SINTESI1").Where(x => x.COD_STATO_RAPPORTO == "SW" && x.VALID_DTA_END == null && x.DTA_NOTIF_ENTE == null && x.COD_TIPO_ACCORDO == "Consensuale").ToList();

            foreach (var record in records)
            {
                var toAdd = new myRaiData.MyRai_Importazioni();
                toAdd.Matricola = "ImportaProrogheSWDaCSV";
                toAdd.Tabella = "ProrogaModuloSmartWorking2020";
                toAdd.Parametro1 = "P" + record.MATRICOLA;
                toAdd.Parametro2 = record.SINTESI1.Nominativo();
                toAdd.Parametro3 = record.SINTESI1.DES_SERVIZIO;
                toAdd.Parametro4 = record.DTA_INIZIO.ToString("dd/MM/yyyy");
                toAdd.Parametro5 = record.DTA_FINE.ToString("dd/MM/yyyy");
                toAdd.Parametro6 = "";
                toAdd.Parametro7 = "N";
                toAdd.Parametro8 = "";
                toAdd.Parametro9 = record.DTA_INIZIO.ToString("dd/MM/yyyy");
                toAdd.Parametro10 = record.DTA_FINE.ToString("dd/MM/yyyy");
                toAdd.Parametro12 = "";
                toAdd.Parametro13 = null;
                toAdd.Parametro15 = "HRIS_AttivaSW_" + record.ID_STATO_RAPPORTO;
                dbDigi.MyRai_Importazioni.Add(toAdd);
            }

            if (DBHelper.Save(dbDigi, CommonHelper.GetCurrentUserMatricola()))
            {
                return Content("OK");
            }
            else
            {
                return Content("Errore durante il salvataggio");
            }
        }

        public static List<SelectListItem> GetRichiesteCat()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            //list.Add(new SelectListItem() { Value = "", Text = "" });
            list.Add(new SelectListItem() { Value = "-1", Text = "Nessuna richiesta" });
            list.Add(new SelectListItem() { Value = "0", Text = "Tutti i tipi di richiesta" });

            var db = new IncentiviEntities();

            foreach (var item in db.XR_MAT_CATEGORIE.Where(x => x.CAT == "SW"))
            {
                list.Add(new SelectListItem() { Value = item.ID.ToString(), Text = String.Format("<div style='line-height:1;display:inline;' data-sw-cat><span class='rai-font-md'>{0}</span><br/><span class='rai-font-sm'>{1}</span></div>", item.TITOLO, item.SOTTO_TITOLO.Replace("\"", "'")) });
                //list.Add(new SelectListItem() { Value = item.ID.ToString(), Text = String.Format("{0} - {1}", item.ID, item.SOTTO_CAT) });
            }

            return list;
        }
    }

  
}
