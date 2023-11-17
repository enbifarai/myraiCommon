using ClosedXML.Excel;
using myRaiCommonManager;
using myRaiCommonModel;
using myRaiData.Incentivi;
using myRaiDataTalentia;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;

namespace myRaiGestionale.Controllers
{
    public partial class StatiRapporto2Controller : Controller
    {
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
                        massimoGiorniSeDeroga = 10;
                    }
                    // dirigenti giornalisti
                    else if (ut.COD_QUALIFICA.Trim().ToUpper().StartsWith("A7"))
                    {
                        massimoGiorniSeDeroga = 4;
                    }
                    // giornalisti
                    else if (ut.COD_QUALIFICA.Trim().ToUpper().StartsWith("M"))
                    {
                        massimoGiorniSeDeroga = 4;
                    }
                    else
                    {
                        massimoGiorniSeDeroga = 10;
                    }
                }

                EventoModel e = new EventoModel()
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
                };

                XR_MOD_DIPENDENTI tempModelloFirmato = null;

                using (TalentiaEntities dtTal = new TalentiaEntities())
                {
                    tempModelloFirmato = dtTal.XR_MOD_DIPENDENTI.Where(w =>
                                            w.COD_MODULO == "SMARTW2020"
                                            && w.MATRICOLA == model.Matricola
                                            && w.DATA_COMPILAZIONE != null)
                                            .OrderByDescending(w => w.DATA_COMPILAZIONE).FirstOrDefault();

                }

                if (tempModelloFirmato != null)
                {
                    e.LavoratoreFragile = true;
                    // se è una scelta per di un lavoratore fragile, deve
                    // calcolare la scelta effettuata dallo stesso
                    List<ModuloSmart2020Selezioni> selezioni = new List<ModuloSmart2020Selezioni>();
                    selezioni = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ModuloSmart2020Selezioni>>(tempModelloFirmato.SCELTA);
                    var selezionato = selezioni;
                    int i_selezionato = (int)selezionato.FirstOrDefault().Selezione;
                    string valoreSelezionato = String.Empty;

                    switch (i_selezionato)
                    {
                        case ((int)ModuloSmart2020SelectionEnum.Scelta50):
                            valoreSelezionato = "PATO";
                            break;

                        case ((int)ModuloSmart2020SelectionEnum.Scelta60):
                            valoreSelezionato = "DISA";
                            break;

                        case ((int)ModuloSmart2020SelectionEnum.Scelta1000):
                            valoreSelezionato = "IMMU";
                            break;
                    }

                    if (!String.IsNullOrEmpty(valoreSelezionato))
                    {
                        e.LavoratoreFragile_Scelta = valoreSelezionato;
                    }
                }

                model.DatiStatiRapporti.Eventi.Add(e);
            }

            return View("~/Views/StatiRapporto/Modal_GestStatoRapporto.cshtml", model);
        }

        public ActionResult Esporta2(RicercaStati ricerca)
        {
            MemoryStream ms = null;
            try
            {
                var wb = new XLWorkbook();
                var res = CercaDipendenti(ricerca, true, -1, -1);

                // Creazione dell'array che conterrà i valori distinti del campo Servizio
                var serviziDistinct = res.List.Select(r => r.Servizio).Distinct().ToArray();

                // Creazione dell'array finale che include la stringa "Totale" e i valori distinti del campo Servizio
                string[] totaliServizi = new string[serviziDistinct.Length + 1];
                totaliServizi[0] = "Totale";
                Array.Copy(serviziDistinct, 0, totaliServizi, 1, serviziDistinct.Length);

                foreach (var servizio in totaliServizi)
                {
                    var ws = wb.AddWorksheet(servizio.Replace("'", "").Replace(@"\", "").Replace(".", "").Replace("/", ""));
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
                                ws.Row(i).Style.Fill.SetBackgroundColor(XLColor.FromArgb(220, 230, 241));
                            else
                                ws.Row(i).Style.Fill.SetBackgroundColor(XLColor.White);
                        }
                    }

                    ws.Column(13).Width = 10;
                    ws.Column(14).Width = 10;
                }
                ms = new MemoryStream();
                wb.SaveAs(ms);
                ms.Position = 0;
            }
            catch (Exception ex)
            {

            }
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = $"Esportazione al {DateTime.Now.ToString("dd-MM-yyyy")}.xlsx" };
        }

        private ResultStati CercaDipendenti(RicercaStati ricerca, bool conRecord = true, int skip = 0, int take = 10)
        {
            ResultStati model = new ResultStati();
            try
            {
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
                if (item.QUALIFICA != null && item.QUALIFICA.TB_QUALSTD != null)
                    anag.FiguraProfessionale = item.QUALIFICA.TB_QUALSTD.DES_QUALSTD.UpperFirst();

                AnagraficaManager.CaricaDatiStatoRapporto(anag, db, sintModel, item.XR_STATO_RAPPORTO, listModuli, listRich);

                model.List.Add(anag);
            }

            model.Showed = skip + model.List.Count();

            }
            catch (Exception ex)
            {

            }

            return model;
        }

        public void DuplicaPrecedenteRiga(int row, IXLWorksheet ws)
        {
            for (int i = 1; i <= 19; i++)
            {
                ws.Cell(row, i).SetValue(ws.Cell(row - 1, i).Value);
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
            DateTime oggi = DateTime.Today;

            var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Dati");

            var db = new TalentiaEntities();
            var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.AsQueryable(), CommonHelper.GetCurrentUserMatricola(), "", StatiRapportoController.GetSWAbilFunc());
            var listMatrAbil = tmpSint.Select(x => x.COD_MATLIBROMAT);

            var listStati = db.XR_STATO_RAPPORTO.Include("SINTESI1").Include("XR_STATO_RAPPORTO_INFO")
                            .Where(x => x.COD_TIPO_ACCORDO == "Consensuale" && x.VALID_DTA_END == null)
                            .Where(x => listMatrAbil.Contains(x.MATRICOLA))
                            .Where(w => w.DTA_INIZIO.Year >= oggi.Year ||  oggi.Year <= w.DTA_FINE.Year )
                            .OrderBy(x => x.MATRICOLA).ThenBy(x => x.DTA_NOTIF_DIP)
                            .ToList();

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

            DateTime minDate = listStati.Where(x => x.COD_STATO_RAPPORTO == "SW_N" || x.COD_STATO_RAPPORTO == "SW_P").Select(x => x.DTA_INIZIO_VISUALIZZAZIONE.HasValue ? x.DTA_INIZIO_VISUALIZZAZIONE.Value : x.DTA_NOTIF_DIP.HasValue ? x.DTA_NOTIF_DIP.Value : x.TMS_TIMESTAMP).Min(x => x);
            var maxDate = listStati.Where(x => x.DTA_SCADENZA != null && (x.COD_STATO_RAPPORTO == "SW_N" || x.COD_STATO_RAPPORTO == "SW_P")).Select(x => x.DTA_SCADENZA.Value).Max(x => x);

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

                        to = item.DTA_SCADENZA.GetValueOrDefault();

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

    }
}