using ClosedXML.Excel;
using myRaiCommonModel;
using myRaiData.Incentivi;
using myRaiHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Objects.SqlClient;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace myRaiGestionale.Controllers
{
    public class RicercaController : Controller
    {
        #region Ricerca Widget

        /// <summary>
        /// Il metodo cerca i dati dei dipendenti in base ai filtri passati e mostra i risultati nella modale
        /// </summary>
        /// <param name="nominativoDipendente"></param>
        /// <param name="matricola"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult RicercaDipendente(string nominativoDipendente, string matricola, string action, string actionText, string provenienza)
        {
            CercaDipendenteVM model = new CercaDipendenteVM();
            model.Filtri = new CercaDipendenteModel();
            model.Filtri.NominativoDipendente = nominativoDipendente;
            model.Filtri.Matricola = matricola;
            if (provenienza == "NODIP")
                model.Filtri.EscludiCessati = false;
            model.Action = action;
            model.ActionText = actionText;
            model.Provenienza = provenienza;
            model.CercaDipendentiResult = new List<CercaDipendentiItem>();

            model.CercaDipendentiResult = CercaDipendenti(model);
            return View("~/Views/CercaDipendente/CercaDipendenteModal.cshtml", model);
        }

        [HttpPost]
        public ActionResult RicercaDipendenteInModal(CercaDipendenteVM model)
        {
            //CercaDipendenteVM model = new CercaDipendenteVM();
            //model.Filtri = new CercaDipendenteModel();
            //model.Filtri.NominativoDipendente = Request.Form["Filtri.NominativoDipendente"];
            //model.Filtri.Matricola = Request.Form["Filtri.Matricola"];
            //model.Action = Request.Form["Action"];
            //model.ActionText = Request.Form["ActionText"];
            //model.Provenienza = Request.Form["Provenienza"];
            model.CercaDipendentiResult = new List<CercaDipendentiItem>();

            model.CercaDipendentiResult = CercaDipendenti(model);

            return View("~/Views/CercaDipendente/_tableResults.cshtml", model);
        }
        [HttpPost]
        public ActionResult RicercaDipendentiSearchBar(CercaDipendenteVM model)
        {
            model.CercaDipendentiResult = CercaDipendenti(model);
            return View("~/Views/CercaDipendente/_tableResults.cshtml", model);
        }
        public ActionResult DrawSearchBar()
        {
            var model = new CercaDipendenteVM()
            {
                Provenienza = "SEARCHBAR"
            };
            model.Filtri = new CercaDipendenteModel();
            var filtriAggiuntivi = new List<HrisInfoAggiuntive>();
            string matricola = CommonHelper.GetCurrentUserMatricola();

            using (IncentiviEntities db = new IncentiviEntities())
            {
                try {
                    var filtri = db.XR_HRIS_RICERCA_FILTRI.Where(x => x.Tipo_regola == null || x.Tipo_regola == "*" || x.Tipo_regola == "DB_PARAM").ToList();
                
            filtri=filtri.Where(x=>(x.Lst_matr_incl == null || x.Lst_matr_incl.Contains(matricola))
&& (x.Lst_matr_excl == null || !x.Lst_matr_excl.Contains(matricola))
&& (x.Abil_func == null || AuthHelper.EnabledToAnySubFunc(matricola, x.Abil_func, x.Abil_subfunc != null ? x.Abil_subfunc.Split(',') : null))).ToList();
               
                    foreach (var filtro in filtri)
                {
                    var elenco = db.Database.SqlQuery<HrisInfoAggiuntive>(filtro.QueryFiltri).Select(x => new HrisInfoAggiuntive()
                    {
                        NomeFiltro = filtro.NomeFiltro,
                        Valore = x.Valore,
                        Descrizione = x.Descrizione
                    }).ToList();
                    filtriAggiuntivi.AddRange(elenco);

                }
                }
                catch (Exception e)
                {
                    var bo = e;
                }
            }

            model.Filtri.FiltriAggiuntivi = filtriAggiuntivi.OrderBy(x => x.Descrizione).ToList();

            if (CommonHelper.GetCurrentUserMatricola()=="103650")
                 model.Filtri.RicercaDinamicaItems = myRaiCommonManager.AnagraficaManager.GetRicercaDinamicaItems();

              return PartialView("~/Views/CercaDipendente/SearchBar.cshtml", model);
        }
        public ActionResult DrawFormExt()
        {
            var model = new CercaDipendenteVM()
            {
                Provenienza = "SEARCHBAR"
            };
            model.Filtri = new CercaDipendenteModel();
            model.Filtri = new CercaDipendenteModel();
            var filtriAggiuntivi = new List<HrisInfoAggiuntive>();
            string matricola = CommonHelper.GetCurrentUserMatricola();
            using (IncentiviEntities db = new IncentiviEntities())
            {
                var filtri = db.XR_HRIS_RICERCA_FILTRI.Where(x => x.Tipo_regola == null || x.Tipo_regola == "*" || x.Tipo_regola == "DB_PARAM").ToList();
                filtri = filtri.Where(x => (x.Lst_matr_incl == null || x.Lst_matr_incl.Contains(matricola))
      && (x.Lst_matr_excl == null || !x.Lst_matr_excl.Contains(matricola))
      && (x.Abil_func == null || AuthHelper.EnabledToAnySubFunc(matricola, x.Abil_func, x.Abil_subfunc != null ? x.Abil_subfunc.Split(',') : null))).ToList();
                foreach (var filtro in filtri)
                {
                    var elenco = db.Database.SqlQuery<HrisInfoAggiuntive>(filtro.QueryFiltri).Select(x => new HrisInfoAggiuntive()
                    {
                        NomeFiltro = filtro.NomeFiltro,
                        Valore = x.Valore,
                        Descrizione = x.Descrizione
                    }).ToList();
                    filtriAggiuntivi.AddRange(elenco);

                }
            }
            model.Filtri.FiltriAggiuntivi = filtriAggiuntivi.OrderBy(x => x.Descrizione).ToList();

            return PartialView("~/Views/CercaDipendente/FormEsteso.cshtml", model);
        }


        public static List<CercaDipendentiItem> CercaDipendenti(CercaDipendenteVM model)
        {
            string nominativoDipendente = model.Filtri.NominativoDipendente;
            string matricola = model.Filtri.Matricola;
            string provenienza = model.Provenienza;
            List<CercaDipendentiItem> result = new List<CercaDipendentiItem>();

            bool esito = false;
            string errorMsg = "";



            try
            {
                bool doSearch = provenienza == "SEARCHBAR" || !string.IsNullOrEmpty(matricola) || !string.IsNullOrEmpty(nominativoDipendente);

                if (doSearch)
                {
                    string hrisAbil = AuthHelper.HrisFuncAbil(CommonHelper.GetCurrentUserMatricola());
                    if (String.IsNullOrWhiteSpace(hrisAbil))
                        hrisAbil = "HRCE";

                    using (IncentiviEntities db = new IncentiviEntities())
                    {
                        string abilKey = "HRCE";
                        if (hrisAbil != "HRCE")
                            abilKey = "HRIS_PERS";
                        string currentMatr = CommonHelper.GetCurrentUserMatricola();

                        var tmpSint = AuthHelper.SintesiFilter(db.SINTESI1.AsQueryable(), currentMatr, provenienza, abilKey);

                        bool anyFilter = false;

                        if (!String.IsNullOrWhiteSpace(model.Filtri.Query))
                        {
                            anyFilter = true;
                            if (char.IsDigit(model.Filtri.Query[0]))
                                matricola = model.Filtri.Query;
                            else
                                nominativoDipendente = model.Filtri.Query;
                        }

                        if (!string.IsNullOrEmpty(matricola))
                        {
                            anyFilter = true;
                            if (matricola.Contains(','))
                                tmpSint = tmpSint.Where(w => !String.IsNullOrEmpty(w.COD_MATLIBROMAT) && matricola.Contains(w.COD_MATLIBROMAT));
                            else
                                tmpSint = tmpSint.Where(w => !String.IsNullOrEmpty(w.COD_MATLIBROMAT) && w.COD_MATLIBROMAT.Contains(matricola));
                        }

                        if (!string.IsNullOrEmpty(nominativoDipendente))
                        {
                            anyFilter = true;
                            tmpSint = tmpSint.Where(w => (w.DES_NOMEPERS.Trim() + " " + w.DES_COGNOMEPERS.Trim()).ToUpper().Contains(nominativoDipendente.ToUpper())
                                                   || (w.DES_COGNOMEPERS.Trim() + " " + w.DES_NOMEPERS.Trim()).ToUpper().Contains(nominativoDipendente.ToUpper()));
                        }

                        if (model.Filtri.Azienda != null && model.Filtri.Azienda.Any())
                        {
                            anyFilter = true;
                            tmpSint = tmpSint.Where(x => model.Filtri.Azienda.Contains(x.COD_IMPRESACR));
                        }

                        if (model.Filtri.Categoria != null && model.Filtri.Categoria.Any())
                        {
                            anyFilter = true;
                            tmpSint = tmpSint.Where(x => model.Filtri.Categoria.Contains(x.COD_QUALIFICA));
                        }

                        if (!String.IsNullOrWhiteSpace(model.Filtri.NascitaDa) && Int32.TryParse(model.Filtri.NascitaDa, out int NascitaDa))
                        {
                            anyFilter = true;
                            tmpSint = tmpSint.Where(x => SqlFunctions.DatePart("yyyy", x.DTA_NASCITAPERS) >= NascitaDa);
                        }

                        if (!String.IsNullOrWhiteSpace(model.Filtri.NascitaA) && Int32.TryParse(model.Filtri.NascitaA, out int NascitaA))
                        {
                            anyFilter = true;
                            tmpSint = tmpSint.Where(x => SqlFunctions.DatePart("yyyy", x.DTA_NASCITAPERS) <= NascitaA);
                        }

                        if (model.Filtri.Sesso != null && model.Filtri.Sesso.Any())
                        {
                            anyFilter = true;
                            tmpSint = tmpSint.Where(x => model.Filtri.Sesso.Contains(x.COD_SESSO));
                        }

                        if (!String.IsNullOrWhiteSpace(model.Filtri.CodiceFiscale))
                        {
                            anyFilter = true;
                            tmpSint = tmpSint.Where(x => x.CSF_CFSPERSONA.StartsWith(model.Filtri.CodiceFiscale));
                        }

                        if (model.Filtri.AssunzioneDa.HasValue)
                        {
                            anyFilter = true;
                            tmpSint = tmpSint.Where(x => x.DTA_ANZCONV >= model.Filtri.AssunzioneDa);
                        }

                        if (model.Filtri.AssunzioneA.HasValue)
                        {
                            anyFilter = true;
                            tmpSint = tmpSint.Where(x => x.DTA_ANZCONV <= model.Filtri.AssunzioneA);
                        }

                        if (model.Filtri.CessazioneDa.HasValue)
                        {
                            anyFilter = true;
                            tmpSint = tmpSint.Where(x => x.DTA_FINE_CR >= model.Filtri.CessazioneDa);
                        }

                        if (model.Filtri.CessazioneA.HasValue)
                        {
                            anyFilter = true;
                            tmpSint = tmpSint.Where(x => x.DTA_FINE_CR <= model.Filtri.CessazioneA);
                        }

                        if (model.Filtri.TipiContratto != null && model.Filtri.TipiContratto.Any())
                        {
                            anyFilter = true;
                            tmpSint = tmpSint.Where(x => model.Filtri.TipiContratto.Contains(x.COD_TPCNTR));
                        }

                        if (model.Filtri.Sedi != null && model.Filtri.Sedi.Any())
                        {
                            anyFilter = true;
                            tmpSint = tmpSint.Where(x => model.Filtri.Sedi.Contains(x.COD_SEDE));
                        }

                        if (model.Filtri.Servizi != null && model.Filtri.Servizi.Any())
                        {
                            anyFilter = true;
                            tmpSint = tmpSint.Where(x => model.Filtri.Servizi.Contains(x.COD_SERVIZIO));
                        }

                        if (model.Filtri.Sezioni != null && model.Filtri.Sezioni.Any())
                        {
                            anyFilter = true;
                            tmpSint = tmpSint.Where(x => model.Filtri.Sezioni.Contains(x.COD_UNITAORG));
                        }

                        if (model.Filtri.EscludiCessati)
                            tmpSint = tmpSint.Where(x => x.DTA_FINE_CR == null || x.DTA_FINE_CR > DateTime.Today);

                        //Per ora escludiamo chi non ha relazioni di lavoro
                        tmpSint = tmpSint.Where(x => x.ID_COMPREL != null);
                        List<HrisInfoAggiuntive> elenco = new List<HrisInfoAggiuntive>();
                        if (model.Filtri.FiltriAggiuntivi != null && model.Filtri.FiltriAggiuntivi.Any())
                        {

                            foreach (var filtro in model.Filtri.FiltriAggiuntivi.GroupBy(x => x.NomeFiltro))
                            {
                                var queryElenco = db.XR_HRIS_RICERCA_FILTRI.Where(x => x.NomeFiltro == filtro.Key).FirstOrDefault().QueryElenco;
                                string query = "";
                                foreach (var valore in model.Filtri.FiltriAggiuntivi.Where(x => x.NomeFiltro == filtro.Key))
                                {
                                    query = query + "'" + valore.Valore + "'";
                                }
                                query = queryElenco.Replace("[ELENCO]", query.Replace(@"''", @"','"));
                                var matricoleFiltrate = db.Database.SqlQuery<HrisInfoAggiuntive>(query).Select(x => new HrisInfoAggiuntive()
                                {
                                    NomeFiltro = filtro.Key,
                                    Valore = x.Valore,
                                    Descrizione = x.Descrizione,
                                    Matricola = x.Matricola
                                }).ToList();
                                elenco.AddRange(matricoleFiltrate);
                            }
                            anyFilter = true;
                            var tot = model.Filtri.FiltriAggiuntivi.GroupBy(x => x.NomeFiltro).Count();
                            var tuttiFiltri = elenco.GroupBy(x => new { x.Matricola, x.NomeFiltro }).GroupBy(x => x.Key.Matricola).Select(x => Tuple.Create(x.Key, x.Count())).Where(x => x.Item2 == tot).Select(x => new { matricola = x.Item1 }).ToList();
                            elenco = elenco.Where(x => tuttiFiltri.Select(y => y.matricola).Contains(x.Matricola)).GroupBy(x => new { x.Matricola, x.NomeFiltro, x.Valore, x.Descrizione }).Select(x => new HrisInfoAggiuntive
                            {
                                Matricola = x.Key.Matricola,
                                Descrizione = x.Key.Descrizione,
                                NomeFiltro = x.Key.NomeFiltro,
                                Valore = x.Key.Valore
                            }).ToList();
                            var tmp = tmpSint.AsEnumerable().Join(tuttiFiltri.AsEnumerable(), x => x.COD_MATLIBROMAT, y => y.matricola, (x, y) => new { s = x, agg = y }).Select(s => new CercaDipendentiItem()
                            {
                                MATRICOLA = s.s.COD_MATLIBROMAT,
                                NOME = s.s.DES_NOMEPERS,
                                COGNOME = s.s.DES_COGNOMEPERS,
                                SECONDO_COGNOME = s.s.DES_SECCOGNOME,
                                DATA_ASSUNZIONE = s.s.DTA_ANZCONV,
                                DATA_CESSAZIONE = s.s.DTA_FINE_CR,
                                CONTRATTO = s.s.DES_TPCNTR,
                                ID_PERSONA = s.s.ID_PERSONA,
                                COD_SEDE = s.s.COD_SEDE,
                                DES_SEDE = s.s.DES_SEDE,
                                COD_SERVIZIO = s.s.COD_SERVIZIO,
                                DES_SERVIZIO = s.s.DES_SERVIZIO,
                                COD_UNITAORG = s.s.COD_UNITAORG,
                                DES_UNITAORG = s.s.DES_DENOMUNITAORG,
                                CF = s.s.CSF_CFSPERSONA,
                                SESSO = s.s.COD_SESSO,
                                AZIENDA = s.s.COD_SOGGETTOCR,
                                CATEGORIA = s.s.DES_QUALIFICA,
                                AnnoNascita = s.s.DTA_NASCITAPERS.Value.Year,
                                SmartWorker = false, //s.XR_STATO_RAPPORTO.Any(x=>x.DTA_INIZIO<=DateTime.Now && x.DTA_FINE>=DateTime.Now && x.COD_STATO_RAPPORTO=="SW")
                                FiltriAggiuntivi = elenco.Where(x => x.Matricola == s.s.COD_MATLIBROMAT).OrderBy(x => x.Descrizione).ToList()
                            });
                            result = tmp.Distinct().ToList();
                            esito = true;
                        }
                        else
                        {
                            if (anyFilter)
                            {
                                //  var prova = tmpSint.AsEnumerable().GroupJoin(elenco.AsEnumerable(), x => x.COD_MATLIBROMAT, y => y.Matricola, (x, y) => new { s = x, agg = y });
                                var tmp = tmpSint.AsEnumerable().Select(s => new CercaDipendentiItem()
                                {
                                    MATRICOLA = s.COD_MATLIBROMAT,
                                    NOME = s.DES_NOMEPERS,
                                    COGNOME = s.DES_COGNOMEPERS,
                                    SECONDO_COGNOME = s.DES_SECCOGNOME,
                                    DATA_ASSUNZIONE = s.DTA_ANZCONV,
                                    DATA_CESSAZIONE = s.DTA_FINE_CR,
                                    CONTRATTO = s.DES_TPCNTR,
                                    ID_PERSONA = s.ID_PERSONA,
                                    COD_SEDE = s.COD_SEDE,
                                    DES_SEDE = s.DES_SEDE,
                                    COD_SERVIZIO = s.COD_SERVIZIO,
                                    DES_SERVIZIO = s.DES_SERVIZIO,
                                    COD_UNITAORG = s.COD_UNITAORG,
                                    DES_UNITAORG = s.DES_DENOMUNITAORG,
                                    CF = s.CSF_CFSPERSONA,
                                    SESSO = s.COD_SESSO,
                                    AZIENDA = s.COD_SOGGETTOCR,
                                    CATEGORIA = s.DES_QUALIFICA,
                                    AnnoNascita = s.DTA_NASCITAPERS.Value.Year,
                                    SmartWorker = false //s.XR_STATO_RAPPORTO.Any(x=>x.DTA_INIZIO<=DateTime.Now && x.DTA_FINE>=DateTime.Now && x.COD_STATO_RAPPORTO=="SW")

                                });

                                result = tmp.ToList();
                                esito = true;
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                result = new List<CercaDipendentiItem>();
            }

            if (provenienza == "SEARCHBAR")
            {
                HrisHelper.LogOperazione("RicercaAnagrafica",
                        String.Format("Matricola: {0}, Nominativo: {1}",
                            !String.IsNullOrWhiteSpace(matricola) ? matricola : "[vuoto]",
                            !String.IsNullOrWhiteSpace(nominativoDipendente) ? nominativoDipendente : "[vuoto]"),
                        esito, errorMsg);
            }

            if (model.Filtri.Dinamiche != null && model.Filtri.Dinamiche.Any())
            {
                
                var db = new IncentiviEntities();
                foreach (var item in model.Filtri.Dinamiche)
                {
                    if (item.StartsWith("SW_CD"))
                    {
                        string Intestazione = GetIntestazioneTabellaPerRicercaDinamica(item);
                        if (model.Filtri.FiltriAggiuntivi == null) model.Filtri.FiltriAggiuntivi = new List<HrisInfoAggiuntive>();
                        model.Filtri.FiltriAggiuntivi.Add(new HrisInfoAggiuntive()
                        {
                            Descrizione = "",
                            NomeFiltro = Intestazione,
                            Matricola = "",
                            Valore = ""
                        });
                        var dbTal = new myRaiDataTalentia.TalentiaEntities();
                        DateTime Dnow = DateTime.Now;
                        var q = dbTal.XR_STATO_RAPPORTO.Where(x => x.VALID_DTA_INI < Dnow &&
                                    (x.VALID_DTA_END == null || x.VALID_DTA_END > Dnow) &&
                                    x.DTA_INIZIO < Dnow && x.DTA_FINE > Dnow &&
                                    (x.COD_TIPO_ACCORDO == "Consensuale" || x.COD_TIPO_ACCORDO == "Deroga")) ;
                        var ListaSR = new List<myRaiDataTalentia.XR_STATO_RAPPORTO>();
                        if (item == "SW_CD")
                            ListaSR = q.ToList();
                        if (item == "SW_CD_SW")
                            ListaSR = q.Where(x => x.COD_STATO_RAPPORTO == "SW").ToList();
                        if (item == "SW_CD_SWN")
                            ListaSR = q.Where(x => x.COD_STATO_RAPPORTO == "SW_N").ToList();
                        if (item == "SW_CD_SWP")
                            ListaSR = q.Where(x => x.COD_STATO_RAPPORTO == "SW_P").ToList();
                        if (item == "SW_CD_SWR")
                            ListaSR = q.Where(x => x.COD_STATO_RAPPORTO == "SW_R").ToList();
                        result = result.Where(x => ListaSR.Select(z => z.MATRICOLA).Contains(x.MATRICOLA)).ToList();
                        foreach (var m in result)
                        {
                            var RichMat = ListaSR.Where(x => x.MATRICOLA == m.MATRICOLA).FirstOrDefault();
                            string Text = RichMat.COD_STATO_RAPPORTO;
                            m.FiltriAggiuntivi = new List<HrisInfoAggiuntive>();
                            m.FiltriAggiuntivi.Add(new HrisInfoAggiuntive()
                            {
                                Descrizione = Text,
                                Matricola = m.MATRICOLA,
                                NomeFiltro = Intestazione,
                                Valore = Text 
                            });
                        }
                    }
                    if (item == "MAT")
                    {
                        string Intestazione = GetIntestazioneTabellaPerRicercaDinamica(item);

                        if (model.Filtri.FiltriAggiuntivi == null) model.Filtri.FiltriAggiuntivi = new List<HrisInfoAggiuntive>();
                        model.Filtri.FiltriAggiuntivi.Add(new HrisInfoAggiuntive() { Descrizione = "",
                            NomeFiltro = Intestazione, Matricola = "", Valore = "" });

                        var query = db.XR_MAT_RICHIESTE.Where(x => x.ECCEZIONE != "SW" && x.XR_WKF_OPERSTATI.Max(z => z.ID_STATO) < 80)
                        .ToList();
                        result = result.Where(x => query.Select (z=>z.MATRICOLA).Contains(x.MATRICOLA)).ToList();
                        foreach (var m in result)
                        {
                            var RichMat = query.Where(x => x.MATRICOLA == m.MATRICOLA).FirstOrDefault();
                            string Text =RichMat.ECCEZIONE+ " "+ ( RichMat.INIZIO_GIUSTIFICATIVO == null ?
                                (RichMat.DATA_INIZIO_MATERNITA.Value.ToString("dd/MM/yyyy") + "-" + RichMat.DATA_INIZIO_MATERNITA.Value.ToString("dd/MM/yyyy"))
                                :
                                (RichMat.INIZIO_GIUSTIFICATIVO.Value.ToString("dd/MM/yyyy") + "-" + RichMat.FINE_GIUSTIFICATIVO.Value.ToString("dd/MM/yyyy")));

                            m.FiltriAggiuntivi = new List<HrisInfoAggiuntive>();
                            m.FiltriAggiuntivi.Add(new HrisInfoAggiuntive()
                            {
                                Descrizione = Text,
                                Matricola = m.MATRICOLA,
                                NomeFiltro = Intestazione,
                                Valore = query.Where(x=>x.MATRICOLA==m.MATRICOLA).Select (x=>x.ECCEZIONE).FirstOrDefault()
                            });
                        }

                    }
                }
            }
            return result;
        }
        public static string GetIntestazioneTabellaPerRicercaDinamica(string codice)
        {
            var db = new IncentiviEntities();
            return db.XR_HRIS_PARAM.Where(x => x.COD_PARAM == "RicercaDinamica" && x.COD_VALUE2 == codice).Select(x => x.COD_VALUE4).FirstOrDefault();
        }

        public ActionResult EsportaResult(CercaDipendenteVM model)
        {
            IncentiviEntities db = new IncentiviEntities();
            var elencoCampi = db.XR_HRIS_EXPORTANAG.Where(x => x.IND_VISIBLE).Select(x => new ExportDipendenti() { CodCampo = x.COD_CAMPO, DesCampo = x.DES_CAMPO, CheckDefault = x.IND_DEFAULT.Equals(null) ? false : x.IND_DEFAULT.Value }).ToList();
            elencoCampi.AddRange(db.XR_HRIS_RICERCA_FILTRI.Where(x => x.Esportabile == true).Select(x => new ExportDipendenti() { CodCampo = x.NomeFiltro, DesCampo = (x.Testata == null ? x.NomeFiltro : x.Testata), CheckDefault = false }).ToList());

            model.CercaDipendentiResult = CercaDipendenti(model);
            var campi = model.CampidaEsportare[0].Split(',');
            XLWorkbook workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Risultati");
            for (int i = 0; i < campi.Count(); i++)
            {
                ws.Cell(1, i + 1).SetValue(elencoCampi.FirstOrDefault(x => x.CodCampo == campi[i]).DesCampo);
            }
            // ws.Range(1, 1, 1, campi.Count()).Style.Fill.SetBackgroundColor(XLColor.FromHtml("#E0E7EB")).Font.SetBold(true);

            int row = 1;
            if (model.CercaDipendentiResult != null && model.CercaDipendentiResult.Any())
            {
                foreach (var item in model.CercaDipendentiResult.OrderBy(x => x.GetType().GetProperty(campi[0]).GetValue(x, null)))
                {
                    row++;
                    for (int i = 0; i < campi.Count(); i++)
                    {
                        if (item.GetType().GetProperty(campi[i]) != null)
                        {
                            ws.Cell(row, i + 1).SetValue(item.GetType().GetProperty(campi[i]).GetValue(item, null));
                        }
                        else
                        {
                            ws.Cell(row, i + 1).SetValue(item.FiltriAggiuntivi.Where(x => x.NomeFiltro == campi[i].TrimEnd()).FirstOrDefault().Descrizione);
                        }
                    }
                }
                ws.Range(1, 1, row, campi.Count()).CreateTable();

                ws.Columns().AdjustToContents();

                MemoryStream ms = new MemoryStream();
                workbook.SaveAs(ms);
                ms.Position = 0;


                byte[] data = ms.ToArray();
                return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Risultati.xlsx");
            }
            else
            {
                ws.Range(1, 1, row, campi.Count()).CreateTable();

                ws.Columns().AdjustToContents();

                MemoryStream ms = new MemoryStream();
                workbook.SaveAs(ms);
                ms.Position = 0;


                byte[] data = ms.ToArray();
                return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Risultati.xlsx");
            } 
        }

      

        public ActionResult ModalExportDipendenti()
        {
            IncentiviEntities db = new IncentiviEntities();
            var campi = db.XR_HRIS_EXPORTANAG.Where(x => x.IND_VISIBLE).Select(x => new ExportDipendenti() { CodCampo = x.COD_CAMPO, DesCampo = x.DES_CAMPO, CheckDefault = x.IND_DEFAULT.Equals(null) ? false : x.IND_DEFAULT.Value }).ToList();
            var filtri = db.XR_HRIS_RICERCA_FILTRI.Where(x => x.Esportabile == true && (x.Tipo_regola==null || x.Tipo_regola=="*" || x.Tipo_regola == "DB_PARAM")).ToList();
            string matricola = CommonHelper.GetCurrentUserMatricola();

            filtri = filtri.Where(x => (x.Lst_matr_incl == null || x.Lst_matr_incl.Contains(matricola))
  && (x.Lst_matr_excl == null || !x.Lst_matr_excl.Contains(matricola))
  && (x.Abil_func == null || AuthHelper.EnabledToAnySubFunc(matricola, x.Abil_func, x.Abil_subfunc != null ? x.Abil_subfunc.Split(',') : null))).ToList();
            campi.AddRange(filtri.Select(x => new ExportDipendenti() { CodCampo = x.NomeFiltro, DesCampo = (x.Testata == null ? x.NomeFiltro : x.Testata), CheckDefault = false }).ToList());
            return View("~/Views/CercaDipendente/ModalExportDipendenti.cshtml", campi);
        }
        public string SalvaRicerca(CercaDipendenteVM model)
        {
            XR_HRIS_REGOLE_VOCI_MENU regolaVoceMenu = new XR_HRIS_REGOLE_VOCI_MENU(){
                TIPO_REGOLA = "DB_PARAM",
                CONTESTO = "Gestionale",
                LST_MATR_INCL = CommonHelper.GetCurrentUserMatricola(),
                JSON = JsonConvert.SerializeObject(model),
                NOME_VOCE=model.NomeRicerca
                
            };
            IncentiviEntities db = new IncentiviEntities();
            db.XR_HRIS_REGOLE_VOCI_MENU.Add(regolaVoceMenu);
            db.SaveChanges();
            return "ok";
        }
            #endregion
        }
}
