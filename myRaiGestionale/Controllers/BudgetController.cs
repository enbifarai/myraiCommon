using ClosedXML.Excel;
using myRai.DataControllers;
using myRaiHelper;
using myRaiCommonManager;
using myRaiCommonModel;
using myRaiData;
using myRaiData.Incentivi;
using myRaiHelper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace myRai.Controllers
{
    public class BudgetController : BaseCommonController
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!PoliticheRetributiveHelper.EnabledToBudget(CommonHelper.GetCurrentUserMatricola()))
            {
                filterContext.Result = new RedirectResult("/Home/notAuth");
                return;
            }

            SessionHelper.Set("GEST_SECTION", "GESTIONE");

            base.OnActionExecuting(filterContext);
        }

        public BudgetController()
        {
            this.BudgetDataController = new BudgetDataController();
        }

        #region RAPIDO

        public ActionResult IndexRapido(int? idCamp = null)
        {
            // reperimento delle aree da renderizzare
            BudgetVM model = new BudgetVM();
            model.InfoCampagna = new InfoCampagna();
            model.Campagne = new List<InfoCampagna>();

            model.Campagne = this.BudgetDataController.GetCampagne(loadClosed: true);

            if (idCamp.HasValue)
            {
                // reperimento della campagna corrente
                model.InfoCampagna = BudgetDataController.GetCampagna(CommonHelper.GetCurrentUserMatricola(), idCamp.Value);
            }
            else
            {
                // reperimento della campagna corrente
                model.InfoCampagna = BudgetDataController.GetCurrentCampagna(CommonHelper.GetCurrentUserMatricola());
            }

            if (model.InfoCampagna != null)
            {
                decimal budgetAnnuo = 0;
                decimal budgetPeriodo = 0;
                decimal costoAnnuo = 0;
                decimal costoPeriodo = 0;

                this.BudgetDataController.GetBudgetAnnuoPeriodoCampagna(model.InfoCampagna, out budgetAnnuo, out budgetPeriodo);
                model.InfoCampagna.BudgetPeriodo = budgetPeriodo;
                model.InfoCampagna.BudgetAnno = budgetAnnuo;

                this.BudgetDataController.GetCostoAnnuoPeriodoCampagna(model.InfoCampagna, out costoAnnuo, out costoPeriodo);
                model.InfoCampagna.CostoPeriodo = costoPeriodo;
                model.InfoCampagna.CostoAnno = costoAnnuo;
            }
            model.ShowReportPoliticheRetributive = true;

            return View("~/Views/Budget/Rapid/Index.cshtml", model);
        }

        public ActionResult GetAreaData(int idArea, int idCamp, int? anno = null)
        {
            BudgetVM model = new BudgetVM();
            model.InfoCampagna = new InfoCampagna();
            model.Campagne = new List<InfoCampagna>();

            model = this.GetBudgetVM(idCamp, idArea, false, anno);

            model.AnnoSelezionato = anno;

            return View("~/Views/Budget/Rapid/AreaDiv.cshtml", model);
        }
        #endregion

        public ActionResult Index(int? idCamp = null)
        {
            return IndexRapido(idCamp);
        }

        [HttpGet]
        public ActionResult SimulazioneBudgetDirezione(int id, int idCamp, int? anno = null)
        {
            SimulazioneBudgetDirezioneVM model = new SimulazioneBudgetDirezioneVM();
            model.IsVisualizzazione = false;
            model.IdDirezione = id;

            model.IdCampagna = idCamp;
            model.DipendentiConProvvedimento = new List<PRV_Dipendente>();
            model.BudgetRiepilogo = new BudgetRiepilogo();

            List<int> campagneContenute = this.BudgetDataController.GetCampagneContenute(idCamp);

            if (campagneContenute == null || campagneContenute.Count() == 0)
            {
                #region pannello Lista utenti
                model.DipendentiConProvvedimento = this.BudgetDataController.GetDipendentiInDirezione(id, idCamp, anno);
                #endregion

                #region Calcolo Budget
                decimal costoDirezione = this.BudgetDataController.GetCostoDirezione(id, idCamp, anno);

                model.BudgetRiepilogo.Area = this.BudgetDataController.GetBudgetAreaByDirezione(id, idCamp);
                model.BudgetRiepilogo.Direzione = this.BudgetDataController.GetBudgetDirezione(id, idCamp);
                model.BudgetRiepilogo.Delta = model.BudgetRiepilogo.Direzione - costoDirezione;

                if (anno.HasValue && anno.GetValueOrDefault() > DateTime.Now.Year)
                {
                    model.BudgetRiepilogo.Area = 0;
                    model.BudgetRiepilogo.Direzione = 0;
                    model.BudgetRiepilogo.Delta = 0;
                }

                model.BudgetRiepilogo.TotaleProvv = costoDirezione;

                #endregion

                #region Consolidata
                if (model.DipendentiConProvvedimento != null && model.DipendentiConProvvedimento.Any())
                {
                    // se sono tutti consolidati allora la direzione è da considerarsi consolidata
                    //model.IsConsolidata = (model.DipendentiConProvvedimento.Count(w => !w.IsConsolidato) == 0);
                    model.IsConsolidata = !model.DipendentiConProvvedimento.Any(w => !w.IsConsolidato);
                }
                #endregion
            }
            else
            {
                model.DipendentiConProvvedimento = this.BudgetDataController.GetDipendentiInDirezione(id, campagneContenute, anno);

                decimal costoDirezione = this.BudgetDataController.GetCostoDirezione(id, campagneContenute, anno);
                decimal budgetDirezione = this.BudgetDataController.GetBudgetDirezione(id, campagneContenute);
                model.BudgetRiepilogo.Area = this.BudgetDataController.GetBudgetAreaByDirezione(id, campagneContenute);
                model.BudgetRiepilogo.Direzione = budgetDirezione;
                model.BudgetRiepilogo.Delta = model.BudgetRiepilogo.Direzione - costoDirezione;

                if (anno.HasValue && anno.GetValueOrDefault() != DateTime.Now.Year)
                {
                    model.BudgetRiepilogo.Area = 0;
                    model.BudgetRiepilogo.Direzione = 0;
                    model.BudgetRiepilogo.Delta = 0;
                }

                model.BudgetRiepilogo.TotaleProvv = costoDirezione;

                model.IsConsolidata = false;
                model.IsVisualizzazione = true;
            }

            #region Calcolo Provvedimenti richiesti

            model.RiepilogoPromozioni = new RiepilogoPromozioni();
            model.RiepilogoPromozioni.Livello0A0 = new Promozione()
            {
                Descrizione = "Passaggio discrezionale LIV. 0A.0",
                NumeroProvvedimenti = 0,
                Costo = 0
            };

            model.RiepilogoPromozioni.Livello1 = new Promozione()
            {
                Descrizione = "Passaggio discrezionale LIV. 1",
                NumeroProvvedimenti = 0,
                Costo = 0
            };

            model.RiepilogoPromozioni.Livello3 = new Promozione()
            {
                Descrizione = "Passaggio discrezionale LIV. 3",
                NumeroProvvedimenti = 0,
                Costo = 0
            };

            model.RiepilogoPromozioni.Livello4 = new Promozione()
            {
                Descrizione = "Passaggio discrezionale LIV. 4",
                NumeroProvvedimenti = 0,
                Costo = 0
            };

            model.RiepilogoPromozioni.Gratifiche = new Promozione()
            {
                Descrizione = "Gratifiche discrezionale",
                NumeroProvvedimenti = 0,
                Costo = 0
            };

            #endregion

            model.AnnoSelezionato = anno;

            return View("~/Views/Budget/_gestisciBudgetDirezione.cshtml", model);
        }

        public ActionResult AggiornaDatiUtente(int idDirezione, int idCamp, int idDip)
        {
            SimulazioneBudgetDirezioneVM model = new SimulazioneBudgetDirezioneVM();

            model.IdDirezione = idDirezione;

            model.IdCampagna = idCamp;

            model.DipendentiConProvvedimento = new List<PRV_Dipendente>();

            model.DipendentiConProvvedimento = this.BudgetDataController.GetDipendenteInDirezione(idDip, idDirezione, idCamp);

            return View("~/Views/Budget/Panels/_trTblUtenti.cshtml", model);
        }

        [HttpGet]
        public ActionResult GetProvvedimenti(int idDipendente)
        {
            List<Provvedimento> provvedimenti = new List<Provvedimento>();
            List<Provvedimento> result = new List<Provvedimento>();

            DateTime current = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            current = current.AddYears(-5);

            provvedimenti = this.BudgetDataController.GetProvvedimenti(idDipendente);

            if (provvedimenti != null && provvedimenti.Any())
            {
                var pro = provvedimenti.Where(w => w.Data >= current).ToList();

                if (pro != null && pro.Any())
                {
                    result = pro;
                }
            }
            return View("~/Views/Budget/Panels/_provvedimenti.cshtml", result);
        }

        public ActionResult RicalcolaBudget(int idDirezione, int idCamp, List<RicalcoloBudgetElement> elementi, int? anno = null)
        {
            SimulazioneBudgetDirezioneVM model = new SimulazioneBudgetDirezioneVM();

            model.IdDirezione = idDirezione;

            model.IdCampagna = idCamp;

            model.AnnoSelezionato = anno;

            #region Calcolo Budget

            model.BudgetRiepilogo = new BudgetRiepilogo();

            model.BudgetRiepilogo.Area = this.BudgetDataController.GetBudgetAreaByDirezione(idDirezione, idCamp);

            model.BudgetRiepilogo.Direzione = this.BudgetDataController.GetBudgetDirezione(idDirezione, idCamp);

            decimal tot = 0;

            if (elementi != null && elementi.Any())
            {
                tot = elementi.Sum(w => w.ValoreToEuro);
            }
            else
            {
                tot = this.BudgetDataController.GetCostoDirezione(idDirezione, idCamp, anno);
            }

            model.BudgetRiepilogo.TotaleProvv = tot;

            model.BudgetRiepilogo.Delta = model.BudgetRiepilogo.Direzione - tot;

            #endregion

            return View("~/Views/Budget/Panels/_panelBudget.cshtml", model);
        }

        public ActionResult ResetTblUtenti(int idDirezione, int idCamp, int? anno = null)
        {
            SimulazioneBudgetDirezioneVM model = new SimulazioneBudgetDirezioneVM();

            model.IdDirezione = idDirezione;

            model.IdCampagna = idCamp;

            model.AnnoSelezionato = anno;

            #region pannello Lista utenti

            model.DipendentiConProvvedimento = new List<PRV_Dipendente>();

            model.DipendentiConProvvedimento = this.BudgetDataController.GetDipendentiInDirezione(idDirezione, idCamp, anno);

            #endregion

            return View("~/Views/Budget/Panels/_tblUtenti.cshtml", model);
        }

        public ActionResult ScaricaLettere(int idDirezione, int idCamp, int? anno = null, string provs = null)
        {
            try
            {
                List<int> listProvs = new List<int>();
                if (!String.IsNullOrWhiteSpace(provs))
                    listProvs.AddRange(provs.Split(',').Select(x => Convert.ToInt32(x)));

                string nomeFile = "";
                MemoryStream outputMemStream = PoliticheRetributiveManager.CreaLettere(out nomeFile, idDirezione, idCamp, anno, listProvs);
                return new FileStreamResult(outputMemStream, "application/pdf") { FileDownloadName = nomeFile };
            }
            catch (Exception ex)
            {
                throw ex;
                //return View("~/Views/Shared/404.cshtml");
            }
        }

        #region esportazioni

        #region PRIVATE
        private void ScriviIntestazioneExcel(InfoCampagna campagna, ref IXLWorksheet worksheet, int startRow = 0)
        {
            try
            {
                startRow += 1;
                worksheet.Cell(startRow, 1).Value = campagna.NomeCampagna;
                worksheet.Cell(startRow, 1).Style.Font.FontSize = 15;
                worksheet.Cell(startRow, 1).Style.Border.BottomBorder = XLBorderStyleValues.None;
                worksheet.Cell(startRow, 1).Style.Border.TopBorder = XLBorderStyleValues.None;
                worksheet.Cell(startRow, 1).Style.Border.LeftBorder = XLBorderStyleValues.None;
                worksheet.Cell(startRow, 1).Style.Border.RightBorder = XLBorderStyleValues.None;
                worksheet.Cell(startRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Row(startRow).Height = 60;
                worksheet.Row(startRow).Style.Fill.BackgroundColor = XLColor.NoColor;
                worksheet.Range(startRow, 1, startRow, 19).Merge();
                worksheet.Cell(startRow, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            }
            catch (Exception ex)
            {

            }
        }

        private void ScriviNomeDirezioneExcel(string nome, ref IXLWorksheet worksheet, int startRow = 0)
        {
            try
            {
                int counter = 2 + startRow;
                worksheet.Cell(counter, 1).Value = nome;
                worksheet.Cell(counter, 1).Style.Font.FontSize = 13;
                worksheet.Cell(counter, 1).Style.Border.BottomBorder = XLBorderStyleValues.None;
                worksheet.Cell(counter, 1).Style.Border.TopBorder = XLBorderStyleValues.None;
                worksheet.Cell(counter, 1).Style.Border.LeftBorder = XLBorderStyleValues.None;
                worksheet.Cell(counter, 1).Style.Border.RightBorder = XLBorderStyleValues.None;
                worksheet.Cell(counter, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(counter, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Row(counter).Height = 40;
                worksheet.Row(counter).Style.Fill.BackgroundColor = XLColor.NoColor;
                worksheet.Range(counter, 1, counter, 19).Merge();
            }
            catch (Exception ex)
            {

            }
        }

        private void ScriviIntestazioniColonneExcel(ref IXLWorksheet worksheet, int startRow = 0)
        {
            try
            {
                int counter = 3 + startRow;

                worksheet.Cell(counter, 1).Value = "PROVV. RICHIESTO";
                worksheet.Cell(counter, 1).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 1).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 1).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 1).Style.Border.RightBorder = XLBorderStyleValues.Thick;

                worksheet.Cell(counter, 2).Value = "STRUTTURA";
                worksheet.Cell(counter, 2).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 2).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 2).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 2).Style.Border.RightBorder = XLBorderStyleValues.Thick;

                worksheet.Cell(counter, 3).Value = "SEDE";
                worksheet.Cell(counter, 3).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 3).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 3).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 3).Style.Border.RightBorder = XLBorderStyleValues.Thick;

                worksheet.Cell(counter, 4).Value = "NOME";
                worksheet.Cell(counter, 4).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 4).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 4).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 4).Style.Border.RightBorder = XLBorderStyleValues.Thick;

                worksheet.Cell(counter, 5).Value = "MATRICOLA";
                worksheet.Cell(counter, 5).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 5).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 5).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 5).Style.Border.RightBorder = XLBorderStyleValues.Thick;

                worksheet.Cell(counter, 6).Value = "DATA DI NASCITA";
                worksheet.Cell(counter, 6).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 6).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 6).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 6).Style.Border.RightBorder = XLBorderStyleValues.Thick;

                worksheet.Cell(counter, 7).Value = "DATA DI ASSUNZIONE";
                worksheet.Cell(counter, 7).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 7).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 7).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 7).Style.Border.RightBorder = XLBorderStyleValues.Thick;

                worksheet.Cell(counter, 8).Value = "ANZIANITA' DI LIVELLO";
                worksheet.Cell(counter, 8).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 8).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 8).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 8).Style.Border.RightBorder = XLBorderStyleValues.Thick;

                worksheet.Cell(counter, 9).Value = "PROFILO";
                worksheet.Cell(counter, 9).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 9).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 9).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 9).Style.Border.RightBorder = XLBorderStyleValues.Thick;

                worksheet.Cell(counter, 10).Value = "LIVELLO";
                worksheet.Cell(counter, 10).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 10).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 10).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 10).Style.Border.RightBorder = XLBorderStyleValues.Thick;

                worksheet.Cell(counter, 11).Value = "ULTIMI PROVVEDIMENTI";
                worksheet.Cell(counter, 11).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 11).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 11).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 11).Style.Border.RightBorder = XLBorderStyleValues.Thick;

                worksheet.Cell(counter, 12).Value = "RAL";
                worksheet.Cell(counter, 12).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 12).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 12).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 12).Style.Border.RightBorder = XLBorderStyleValues.Thick;

                worksheet.Cell(counter, 13).Value = "RETRIBUZIONE VARIABILE ULTIMI 12 MESI";
                worksheet.Cell(counter, 13).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 13).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 13).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 13).Style.Border.RightBorder = XLBorderStyleValues.Thick;

                worksheet.Cell(counter, 14).Value = "REPERIBILITA' ULTIMI 12 MESI";
                worksheet.Cell(counter, 14).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 14).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 14).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 14).Style.Border.RightBorder = XLBorderStyleValues.Thick;

                worksheet.Cell(counter, 15).Value = "LIVELLO RICHIESTO";
                worksheet.Cell(counter, 15).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 15).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 15).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 15).Style.Border.RightBorder = XLBorderStyleValues.Thick;

                worksheet.Cell(counter, 16).Value = "DECORRENZA RICHIESTA";
                worksheet.Cell(counter, 16).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 16).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 16).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 16).Style.Border.RightBorder = XLBorderStyleValues.Thick;

                worksheet.Cell(counter, 17).Value = "COSTO PERIODO";
                worksheet.Cell(counter, 17).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 17).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 17).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 17).Style.Border.RightBorder = XLBorderStyleValues.Thick;

                worksheet.Cell(counter, 18).Value = "COSTO ANNUO";
                worksheet.Cell(counter, 18).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 18).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 18).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 18).Style.Border.RightBorder = XLBorderStyleValues.Thick;

                worksheet.Cell(counter, 19).Value = "NOTE";
                worksheet.Cell(counter, 19).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 19).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 19).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                worksheet.Cell(counter, 19).Style.Border.RightBorder = XLBorderStyleValues.Thick;
                worksheet.Row(counter).Height = 30;

                for (int i = 1; i <= 19; i++)
                {
                    worksheet.Cell(counter + 1, i).Value = "";

                    worksheet.Cell(counter, i).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                    worksheet.Cell(counter, i).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                    worksheet.Cell(counter, i).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                    worksheet.Cell(counter, i).Style.Border.RightBorder = XLBorderStyleValues.Thick;
                    worksheet.Range(counter, i, counter + 1, i).Merge();
                    worksheet.Cell(counter, i).Style.Font.FontSize = 15;
                    worksheet.Cell(counter, i).Style.Font.Bold = true;
                    worksheet.Cell(counter, i).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(counter, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(counter, i).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(counter, i).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 32, 96);
                    worksheet.Cell(counter, i).Style.Alignment.WrapText = true;
                    worksheet.Cell(counter + 1, i).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                    worksheet.Cell(counter + 1, i).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                    worksheet.Cell(counter + 1, i).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                    worksheet.Cell(counter + 1, i).Style.Border.RightBorder = XLBorderStyleValues.Thick;
                }
                worksheet.Row(counter + 1).Height = 30;
            }
            catch (Exception ex)
            {

            }
        }

        private List<ReportItem> PreparaOggettiPerExcel(string direzione, List<PRV_Dipendente> dipendenti, List<TabellaItem> rows,
            ref decimal totAnno, ref decimal totPeriodo, ref decimal totProvvSelAnno, ref decimal totProvvSelPeriodo, ref decimal totProvvSelStraordinario)
        {

            List<ReportItem> items = new List<ReportItem>();
            try
            {
                if (dipendenti != null && dipendenti.Any())
                {
                    foreach (var d in dipendenti)
                    {
                        DateTime oggi = DateTime.Now;
                        oggi = oggi.AddYears(-5);

                        string provvedimenti = "";
                        string profilo = "";
                        string provv = "";
                        string vertenze = "";

                        if (rows != null && rows.Any())
                        {
                            var f = rows.Where(w => w.IdDipendente.Equals(d.IdDipendente)).FirstOrDefault();

                            if (f != null)
                            {
                                d.IdProvvedimento = f.IdTipologia;
                            }
                            else
                            {
                                throw new Exception("Errore nel reperimento del provvedimento selezionato");
                            }
                        }

                        #region creazione stringa elenco provvedimenti

                        if (d.Provvedimenti != null && d.Provvedimenti.Any())
                        {
                            string separatore = " ";
                            foreach (var p in d.Provvedimenti)
                            {
                                string descr = ((ProvvedimentiEnum)p.IdProvvedimento).GetDescription();

                                provvedimenti += String.Format("{0}{1} {2}\r\n", separatore, p.Data.ToString("yyyy/MM"), descr);
                                separatore = " - ";
                            }
                        }
                        provvedimenti = provvedimenti.Trim();

                        #endregion

                        #region stringa profilo

                        if (!String.IsNullOrEmpty(d.DescRuolo) && !String.IsNullOrEmpty(d.CodRuolo))
                        {
                            profilo = d.DescRuolo.Replace(d.CodRuolo, "");
                            profilo = profilo.Replace("-", "");
                            profilo = profilo.Trim();
                        }

                        #endregion

                        #region stringa Provvedimento selezionato

                        var provvedimento = this.BudgetDataController.GetProvvedimento(d.IdProvvedimento);


                        if (provvedimento != null)
                        {
                            provv = provvedimento.DESCRIZIONE;
                        }

                        #endregion

                        #region Reperimento del record del provvedimento selezionato

                        //var provvSel = this.BudgetDataController.GetVariazioneDipendente(d.IdDipendente, d.IdProvvedimento);
                        var provvSel = PoliticheRetributiveHelper.GetDipProvEffettivo(d.IdDipendente);

                        if (provvSel == null)
                        {
                            throw new Exception("Errore nel reperimento del provvedimento selezionato");
                        }

                        d.CostoAnnuo = provvSel.COSTO_ANNUO;
                        d.CostoPeriodo = provvSel.COSTO_PERIODO;
                        d.CostoConStraordinario = provvSel.COSTO_REC_STR.GetValueOrDefault();
                        d.LivPrevisto = provvSel.LIV_PREVISTO;
                        #endregion

                        ReportItem r = new ReportItem()
                        {
                            Direzione = direzione,
                            LivelloRichiesto = String.IsNullOrEmpty(d.LivPrevisto) ? d.LivAttuale : d.LivPrevisto,
                            Livello = d.LivAttuale,
                            CostoRegime = String.Format("€ {0:N2}", d.CostoAnnuo),
                            CostoPeriodo = String.Format("€ {0:N2}", d.CostoPeriodo),
                            Nominativo = d.Nominativo,
                            RAL = String.Format("€ {0:N2}", d.RAL),
                            UltimiProvv = provvedimenti,
                            DecorrenzaRichiesta = (d.Decorrenza.HasValue ? d.Decorrenza.Value.ToString("dd/MM/yyyy") : ""),
                            Profilo = profilo,
                            ProvvRichiesto = provv,
                            Struttura = d.Struttura,
                            Note = d.Note,
                            Vertenze = vertenze,
                            DataAssunzione = d.DataAssunzione,
                            DataNascita = d.DataNascita,
                            AnzianitaLivello = d.AnzianitaLivello,
                            Reperibilita = String.Format("€ {0:N2}", d.Reperibilita),
                            RetribuzioneVariabile = String.Format("€ {0:N2}", d.RetribuzioneVariabile),
                            Assenze = String.Format("{0} giorni di assenza", d.Assenze.ToString()),
                            AggregatoSede = d.AggregatoSede,
                            PartTime = d.PartTime,
                            Matricola = d.Matricola
                        };

                        if (r.ProvvRichiesto.ToUpper().Equals("GRATIFICA"))
                        {
                            r.CostoRegime = "€ 0,00";
                            totAnno += 0;
                            totProvvSelAnno += 0;
                        }
                        else
                        {
                            totAnno += d.CostoAnnuo;
                            totProvvSelAnno += provvSel.COSTO_ANNUO;
                        }

                        totProvvSelPeriodo += provvSel.COSTO_PERIODO;
                        totProvvSelStraordinario += provvSel.COSTO_REC_STR.GetValueOrDefault();
                        totPeriodo += d.CostoPeriodo;

                        if (r.ProvvRichiesto.ToUpper().Equals("AUMENTO DI MERITO") ||
                            r.ProvvRichiesto.ToUpper().Equals("GRATIFICA"))
                        {
                            r.LivelloRichiesto = "";
                        }

                        if (String.IsNullOrEmpty(r.Note))
                        {
                            r.Note = "";
                        }

                        if (r.PartTime == "NO PART_TIME")
                        {
                            r.PartTime = "";
                        }
                        using (IncentiviEntities db = new IncentiviEntities())
                        {
                            // se ha cause aperte
                            if (PoliticheRetributiveManager.HasCauseAperte(db, d.Matricola, d.IdDipendente))
                            {
                                r.Cause = "Contenzioso\r\n";
                            }

                            if (PoliticheRetributiveManager.HasVertSindacali(db, d.Matricola))
                            {
                                r.Vertenze = "Vert. sindacali\r\n";
                            }

                            if (PoliticheRetributiveManager.HasVertStragiudiziali(db, d.IdDipendente))
                            {
                                r.Vertenze += "Vert. stragiudiziali\r\n";
                            }
                        }

                        if (d.Assenze != null && d.Assenze.Any())
                        {
                            if (d.Assenze.Sum(w => w.Quantita) > 30)
                            {
                                foreach (var a in d.Assenze)
                                {
                                    r.Assenze += String.Format("{0} - {1}\r\n", a.Codice, a.Quantita);
                                }
                            }
                        }

                        if (String.IsNullOrEmpty(r.Assenze))
                        {
                            r.Note += String.Format("{0}\r\n{1}\r\n{2}r\n{3}", r.Vertenze, r.Cause, r.Assenze, r.PartTime);
                        }
                        else
                        {
                            r.Note += String.Format("{0}\r\n{1}\r\n{2}", r.Vertenze, r.Cause, r.PartTime);
                        }
                        r.Note = r.Note.Trim();

                        items.Add(r);
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return items;
        }

        #endregion

        public ActionResult PreparaEsportazioneXLSX(int idCamp, int idDir, List<TabellaItem> rows, int? anno = null)
        {
            // creazione della GUID
            string handle = idDir.ToString() + idCamp.ToString();
            string nomeFile = "";

            try
            {

                // reperimento della direzione
                var direzione = this.BudgetDataController.GetDirezione(idDir);

                // reperimento della campagna
                var campagna = this.BudgetDataController.GetCampagna(CommonHelper.GetCurrentUserMatricola(), idCamp, null, anno);

                List<int?> dt = new List<int?>();
                List<int> _dt = new List<int>();
                _dt = campagna.Decorrenze.OrderBy(w => w).Select(w => w.Year).Distinct().ToList();

                bool forcedNull = false;

                if (_dt == null || !_dt.Any())
                {
                    forcedNull = true;
                    dt.Add(null);
                }
                else
                {
                    _dt.ForEach(w =>
                    {
                        dt.Add(w);
                    });
                    dt.Add(null);
                }

                XLWorkbook workbook = new XLWorkbook();

                foreach (var myDate in dt)
                {
                    List<ReportItem> items = new List<ReportItem>();

                    if (forcedNull)
                    {
                        anno = null;
                    }
                    else
                    {
                        anno = myDate;
                    }

                    IXLWorksheet worksheet;
                    workbook.Worksheets.TryGetWorksheet(String.Format("{0} {1} ", anno, direzione.CODICE).Truncate(25), out worksheet);

                    if (worksheet == null)
                    {
                        worksheet = workbook.Worksheets.Add(String.Format("{0} {1} ", anno, direzione.CODICE).Truncate(25));
                        worksheet.PageSetup.PaperSize = XLPaperSize.A3Paper;
                        worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                        worksheet.PageSetup.PagesWide = 1;
                    }

                    int counter = 4;
                    decimal totAnno = 0;
                    decimal totPeriodo = 0;
                    decimal totProvvSelAnno = 0;
                    decimal totProvvSelPeriodo = 0;
                    decimal totProvvSelStraordinario = 0;

                    #region Intestazione
                    this.ScriviIntestazioneExcel(campagna, ref worksheet);
                    #endregion

                    #region Nome direzione
                    this.ScriviNomeDirezioneExcel(String.Format("{0} - {1}", direzione.NOME, direzione.CODICE), ref worksheet);
                    #endregion

                    #region Intestazioni colonne
                    this.ScriviIntestazioniColonneExcel(ref worksheet);
                    #endregion

                    #region Preparazione oggetto per la stampa su XLSX
                    List<PRV_Dipendente> dipendenti = new List<PRV_Dipendente>();
                    if (campagna.CampagneContenute.Count() == 0)
                        dipendenti = this.BudgetDataController.GetDipendentiInDirezione(idDir, idCamp, anno);
                    else
                        dipendenti = this.BudgetDataController.GetDipendentiInDirezione(idDir, campagna.CampagneContenute, anno);


                    if (dipendenti != null && dipendenti.Any())
                    {
                        foreach (var d in dipendenti)
                        {
                            DateTime oggi = DateTime.Now;
                            oggi = oggi.AddYears(-5);

                            string provvedimenti = "";
                            string profilo = "";
                            string provv = "";
                            string vertenze = "";

                            if (rows != null && rows.Any())
                            {
                                var f = rows.Where(w => w.IdDipendente.Equals(d.IdDipendente)).FirstOrDefault();

                                if (f != null)
                                {
                                    d.IdProvvedimento = f.IdTipologia;
                                }
                                else
                                {
                                    throw new Exception("Errore nel reperimento del provvedimento selezionato");
                                }
                            }

                            #region creazione stringa elenco provvedimenti

                            if (d.Provvedimenti != null && d.Provvedimenti.Any())
                            {
                                string separatore = " ";
                                foreach (var p in d.Provvedimenti)
                                {
                                    string descr = ((ProvvedimentiEnum)p.IdProvvedimento).GetDescription();

                                    provvedimenti += String.Format("{0}{1} {2}\r\n", separatore, p.Data.ToString("yyyy/MM"), descr);
                                    separatore = " - ";
                                }
                            }
                            provvedimenti = provvedimenti.Trim();

                            #endregion

                            #region stringa profilo

                            if (!String.IsNullOrEmpty(d.DescRuolo) && !String.IsNullOrEmpty(d.CodRuolo))
                            {
                                profilo = d.DescRuolo.Replace(d.CodRuolo, "");
                                profilo = profilo.Replace("-", "");
                                profilo = profilo.Trim();
                            }

                            #endregion

                            #region stringa Provvedimento selezionato

                            var provvedimento = this.BudgetDataController.GetProvvedimento(d.IdProvvedimento);

                            if (provvedimento != null)
                            {
                                provv = provvedimento.DESCRIZIONE;
                            }

                            #endregion

                            #region Reperimento del record del provvedimento selezionato
                            XR_PRV_DIPENDENTI_VARIAZIONI provvSel = new XR_PRV_DIPENDENTI_VARIAZIONI();

                            if ((d.IdProvvedimento == (int)ProvvedimentiEnum.Nessuno) ||
                                (d.IdProvvedimento == (int)ProvvedimentiEnum.CUSNessuno))
                            {
                                d.CostoAnnuo = 0;
                                d.CostoPeriodo = 0;
                                d.CostoConStraordinario = 0;
                                d.LivPrevisto = d.LivAttuale;
                            }
                            else
                            {
                                //provvSel = this.BudgetDataController.GetVariazioneDipendente(d.IdDipendente, d.IdProvvedimento);
                                provvSel = PoliticheRetributiveHelper.GetDipProvEffettivo(d.IdDipendente);

                                if (provvSel == null)
                                {
                                    throw new Exception("Errore nel reperimento del provvedimento selezionato");
                                }

                                d.CostoAnnuo = provvSel.COSTO_ANNUO;
                                d.CostoPeriodo = provvSel.COSTO_PERIODO;
                                d.CostoConStraordinario = provvSel.COSTO_REC_STR.GetValueOrDefault();
                                d.LivPrevisto = provvSel.LIV_PREVISTO;
                            }
                            #endregion

                            ReportItem r = new ReportItem()
                            {
                                Direzione = direzione.NOME,
                                LivelloRichiesto = String.IsNullOrEmpty(d.LivPrevisto) ? d.LivAttuale : d.LivPrevisto,
                                Livello = d.LivAttuale,
                                CostoRegime = String.Format("€ {0:N2}", d.CostoAnnuo),
                                CostoPeriodo = String.Format("€ {0:N2}", d.CostoPeriodo),
                                Nominativo = d.Nominativo,
                                RAL = String.Format("€ {0:N2}", d.RAL),
                                UltimiProvv = provvedimenti,
                                DecorrenzaRichiesta = (d.Decorrenza.HasValue ? d.Decorrenza.Value.ToString("dd/MM/yyyy") : ""),
                                Profilo = profilo,
                                ProvvRichiesto = provv,
                                Struttura = d.Struttura,
                                Note = d.Note,
                                Vertenze = vertenze,
                                DataAssunzione = d.DataAssunzione,
                                DataNascita = d.DataNascita,
                                AnzianitaLivello = d.AnzianitaLivello,
                                Reperibilita = String.Format("€ {0:N2}", d.Reperibilita),
                                RetribuzioneVariabile = String.Format("€ {0:N2}", d.RetribuzioneVariabile),
                                Assenze = "",
                                AggregatoSede = d.AggregatoSede,
                                PartTime = d.PartTime,
                                Matricola = d.Matricola
                            };

                            if (r.ProvvRichiesto.ToUpper().Equals("GRATIFICA"))
                            {
                                r.CostoRegime = "€ 0,00";
                                totAnno += 0;
                                totProvvSelAnno += 0;
                            }
                            else
                            {
                                totAnno += d.CostoAnnuo;
                                totProvvSelAnno += provvSel.COSTO_ANNUO;
                            }

                            totProvvSelPeriodo += provvSel.COSTO_PERIODO;
                            totProvvSelStraordinario += provvSel.COSTO_REC_STR.GetValueOrDefault();
                            totPeriodo += d.CostoPeriodo;

                            if (r.ProvvRichiesto.ToUpper().Equals("AUMENTO DI MERITO") ||
                                r.ProvvRichiesto.ToUpper().Equals("GRATIFICA"))
                            {
                                r.LivelloRichiesto = "";
                            }

                            if (String.IsNullOrEmpty(r.Note))
                            {
                                r.Note = "";
                            }

                            if (r.PartTime == "NO PART_TIME")
                            {
                                r.PartTime = "";
                            }
                            using (IncentiviEntities db = new IncentiviEntities())
                            {
                                // se ha cause aperte
                                if (PoliticheRetributiveManager.HasCauseAperte(db, d.Matricola, d.IdDipendente))
                                {
                                    r.Cause = "Contenzioso\r\n";
                                }

                                if (PoliticheRetributiveManager.HasVertSindacali(db, d.Matricola))
                                {
                                    r.Vertenze = "Vert. sindacali\r\n";
                                }

                                if (PoliticheRetributiveManager.HasVertStragiudiziali(db, d.IdDipendente))
                                {
                                    r.Vertenze += "Vert. stragiudiziali\r\n";
                                }
                            }

                            if (d.Assenze != null && d.Assenze.Any())
                            {
                                if (d.Assenze.Sum(w => w.Quantita) > 30)
                                {
                                    r.Assenze = String.Format("{0} giorni di assenza\r\n", d.Assenze.Sum(w => w.Quantita));
                                    foreach (var a in d.Assenze)
                                    {
                                        r.Assenze += String.Format("{0} - {1}\r\n", a.Codice, a.Quantita);
                                    }
                                }
                            }

                            if (!String.IsNullOrEmpty(r.Assenze))
                            {
                                r.Note += String.Format("{0}\r\n{1}\r\n{2}\r\n{3}", r.Vertenze, r.Cause, r.Assenze, r.PartTime);
                            }
                            else
                            {
                                r.Note += String.Format("{0}\r\n{1}\r\n{2}", r.Vertenze, r.Cause, r.PartTime);
                            }
                            r.Note = r.Note.Trim();

                            items.Add(r);
                        }
                    }

                    #endregion

                    #region Stampa Corpo Excel
                    if (items != null && items.Any())
                    {
                        var provvs = items.GroupBy(w => w.ProvvRichiesto).Distinct();

                        var groups = items.GroupBy(w => w.ProvvRichiesto).Select(g => new
                        {
                            Provvedimento = g.Key
                        });

                        int contaTipologia = groups.Count();

                        var p = items.GroupBy(w => w.ProvvRichiesto).OrderBy(w => w.Key);

                        var ordered = p.Where(w => w.Key.ToUpper().Equals("PROMOZIONE")).ToList();
                        ordered.AddRange(p.Where(w => w.Key.ToUpper().Equals("AUMENTO DI MERITO")).ToList());
                        ordered.AddRange(p.Where(w => w.Key.ToUpper().Equals("GRATIFICA")).ToList());
                        //ordered.AddRange(p.Where(w => w.Key.ToUpper().Equals("PROMOZIONE SENZA ASSORBIMENTO")).ToList());

                        foreach (var itemsGroup in ordered)
                        {
                            worksheet.Cell(counter + 1, 1).SetValue<string>(itemsGroup.Key);
                            worksheet.Cell(counter + 1, 1).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                            worksheet.Cell(counter + 1, 1).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                            worksheet.Cell(counter + 1, 1).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                            worksheet.Cell(counter + 1, 1).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                            decimal regimeParziale = 0;
                            decimal periodoParziale = 0;

                            foreach (var i in itemsGroup)
                            {
                                counter++;

                                worksheet.Cell(counter, 2).SetValue<string>(i.Struttura);
                                worksheet.Cell(counter, 3).SetValue<string>(i.AggregatoSede);
                                worksheet.Cell(counter, 4).SetValue<string>(i.Nominativo);
                                worksheet.Cell(counter, 5).SetValue<string>(i.Matricola);
                                worksheet.Cell(counter, 6).SetValue<string>((!i.DataNascita.HasValue) ? "" : i.DataNascita.Value.ToString("dd/MM/yyyy"));
                                worksheet.Cell(counter, 7).SetValue<string>((!i.DataAssunzione.HasValue) ? "" : i.DataAssunzione.Value.ToString("dd/MM/yyyy"));
                                worksheet.Cell(counter, 8).SetValue<string>((!i.AnzianitaLivello.HasValue) ? "" : i.AnzianitaLivello.Value.ToString("dd/MM/yyyy"));
                                worksheet.Cell(counter, 9).SetValue<string>(i.Profilo);
                                worksheet.Cell(counter, 10).SetValue<string>(i.Livello);
                                worksheet.Cell(counter, 11).SetValue<string>(i.UltimiProvv);
                                worksheet.Cell(counter, 12).SetValue<string>(i.RAL);
                                worksheet.Cell(counter, 13).SetValue<string>(i.RetribuzioneVariabile);
                                worksheet.Cell(counter, 14).SetValue<string>(i.Reperibilita);
                                worksheet.Cell(counter, 15).SetValue<string>(i.LivelloRichiesto);
                                worksheet.Cell(counter, 16).SetValue<string>(i.DecorrenzaRichiesta);
                                worksheet.Cell(counter, 17).SetValue<string>(i.CostoPeriodo);
                                worksheet.Cell(counter, 18).SetValue<string>(i.CostoRegime);
                                worksheet.Cell(counter, 19).SetValue<string>(i.Note.Trim());
                                worksheet.Column(19).AdjustToContents();

                                for (int nCell = 1; nCell <= 19; nCell++)
                                {
                                    worksheet.Cell(counter, nCell).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                    worksheet.Cell(counter, nCell).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                    worksheet.Cell(counter, nCell).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                    worksheet.Cell(counter, nCell).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                    worksheet.Cell(counter, nCell).Style.Alignment.WrapText = true;
                                    if ((nCell >= 12 && nCell <= 14) ||
                                         (nCell >= 17 && nCell <= 18))
                                    {
                                        worksheet.Cell(counter, nCell).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                    }
                                    else
                                    {
                                        worksheet.Cell(counter, nCell).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    }

                                    worksheet.Cell(counter, nCell).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    worksheet.Cell(counter, nCell).Style.Font.FontSize = 12;
                                }

                                worksheet.Row(counter).Height = 50;
                                worksheet.Row(counter).Style.Alignment.SetWrapText(true);

                                string tempRegimeParziale = String.IsNullOrEmpty(i.CostoRegime) ? "0" : i.CostoRegime;
                                string tempPeriodoParziale = String.IsNullOrEmpty(i.CostoPeriodo) ? "0" : i.CostoPeriodo;

                                tempRegimeParziale = tempRegimeParziale.Replace("€", "");
                                tempRegimeParziale = tempRegimeParziale.Trim();

                                tempPeriodoParziale = tempPeriodoParziale.Replace("€", "");
                                tempPeriodoParziale = tempPeriodoParziale.Trim();

                                decimal reg = Convert.ToDecimal(tempRegimeParziale, System.Globalization.CultureInfo.CurrentCulture);
                                decimal per = Convert.ToDecimal(tempPeriodoParziale, System.Globalization.CultureInfo.CurrentCulture);

                                regimeParziale += reg;
                                periodoParziale += per;
                            }

                            #region totale parziale
                            counter++;
                            worksheet.Cell(counter, 1).SetValue<string>("Totale parziale");
                            worksheet.Cell(counter, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            worksheet.Cell(counter, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            worksheet.Cell(counter, 1).Style.Font.FontSize = 12;
                            worksheet.Cell(counter, 1).Style.Font.Bold = true;

                            worksheet.Cell(counter, 17).SetValue<string>(String.Format("€ {0:N2}", periodoParziale));
                            worksheet.Cell(counter, 18).SetValue<string>(String.Format("€ {0:N2}", regimeParziale));
                            worksheet.Cell(counter, 19).SetValue<string>("");

                            worksheet.Cell(counter, 17).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            worksheet.Cell(counter, 17).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            worksheet.Cell(counter, 17).Style.Font.FontSize = 12;
                            worksheet.Cell(counter, 17).Style.Font.Bold = true;

                            worksheet.Cell(counter, 18).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            worksheet.Cell(counter, 18).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            worksheet.Cell(counter, 18).Style.Font.FontSize = 12;
                            worksheet.Cell(counter, 18).Style.Font.Bold = true;

                            if (periodoParziale > 0)
                            {
                                worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(0, 176, 80);
                            }
                            else if (periodoParziale < 0)
                            {
                                worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(255, 0, 0);
                            }
                            else
                            {
                                worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
                            }

                            if (regimeParziale > 0)
                            {
                                worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(0, 176, 80);
                            }
                            else if (regimeParziale < 0)
                            {
                                worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(255, 0, 0);
                            }
                            else
                            {
                                worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
                            }

                            for (int nCell = 1; nCell <= 19; nCell++)
                            {
                                worksheet.Cell(counter, nCell).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                worksheet.Cell(counter, nCell).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                worksheet.Cell(counter, nCell).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                worksheet.Cell(counter, nCell).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                worksheet.Cell(counter, nCell).Style.Alignment.WrapText = true;
                            }
                            worksheet.Range(counter, 1, counter, 16).Merge();
                            worksheet.Row(counter).Height = 30;

                            #endregion

                            counter++;
                        }

                        counter++;
                        worksheet.Cell(counter, 1).SetValue<string>("Totale complessivo");
                        worksheet.Cell(counter, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(counter, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell(counter, 1).Style.Font.FontSize = 12;
                        worksheet.Cell(counter, 1).Style.Font.Bold = true;

                        worksheet.Cell(counter, 17).SetValue<string>(String.Format("€ {0:N2}", totPeriodo));
                        worksheet.Cell(counter, 18).SetValue<string>(String.Format("€ {0:N2}", totAnno));
                        worksheet.Cell(counter, 19).SetValue<string>("");

                        worksheet.Cell(counter, 17).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(counter, 17).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell(counter, 17).Style.Font.FontSize = 12;
                        worksheet.Cell(counter, 17).Style.Font.Bold = true;

                        worksheet.Cell(counter, 18).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(counter, 18).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell(counter, 18).Style.Font.FontSize = 12;
                        worksheet.Cell(counter, 18).Style.Font.Bold = true;

                        if (totPeriodo > 0)
                        {
                            worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(0, 176, 80);
                        }
                        else if (totPeriodo < 0)
                        {
                            worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(255, 0, 0);
                        }
                        else
                        {
                            worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
                        }

                        if (totAnno > 0)
                        {
                            worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(0, 176, 80);
                        }
                        else if (totAnno < 0)
                        {
                            worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(255, 0, 0);
                        }
                        else
                        {
                            worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
                        }

                        for (int nCell = 1; nCell <= 19; nCell++)
                        {
                            worksheet.Cell(counter, nCell).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                            worksheet.Cell(counter, nCell).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                            worksheet.Cell(counter, nCell).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                            worksheet.Cell(counter, nCell).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                            worksheet.Cell(counter, nCell).Style.Alignment.WrapText = true;
                        }
                        worksheet.Range(counter, 1, counter, 16).Merge();
                        worksheet.Row(counter).Height = 30;

                        counter++;
                    }

                    #endregion

                    decimal budgetAnno = 0;
                    decimal budgetPeriodo = 0;
                    if (campagna.CampagneContenute.Count() == 0)
                        this.BudgetDataController.GetBudgetAnnuoPeriodoDirezione(idDir, idCamp, out budgetAnno, out budgetPeriodo);
                    else
                        this.BudgetDataController.GetBudgetAnnuoPeriodoDirezione(idDir, campagna.CampagneContenute, out budgetAnno, out budgetPeriodo);

                    decimal tot = 0;
                    tot = totProvvSelAnno;

                    if (rows == null || !rows.Any())
                    {
                        if (campagna.CampagneContenute.Count() == 0)
                            tot = this.BudgetDataController.GetCostoDirezione(idDir, idCamp);
                        else
                            tot = this.BudgetDataController.GetCostoDirezione(idDir, campagna.CampagneContenute);
                    }

                    decimal budgetDeltaAnno = budgetAnno - totProvvSelAnno;
                    decimal budgetDeltaPeriodo = budgetPeriodo - totProvvSelPeriodo;

                    if (anno.HasValue && anno.Value != DateTime.Now.Year)
                    {
                        budgetAnno = 0;
                        budgetPeriodo = 0;
                        budgetDeltaAnno = 0;
                        budgetDeltaPeriodo = 0;
                    }

                    #region Riepilogo Budget
                    counter = counter + 2;
                    worksheet.Cell(counter, 1).SetValue<string>("Budget");
                    worksheet.Cell(counter, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(counter, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    worksheet.Cell(counter, 1).Style.Font.FontSize = 12;
                    worksheet.Cell(counter, 1).Style.Font.Bold = true;

                    worksheet.Cell(counter, 17).SetValue<string>(String.Format("€ {0:N2}", budgetPeriodo));
                    worksheet.Cell(counter, 18).SetValue<string>(String.Format("€ {0:N2}", budgetAnno));
                    worksheet.Cell(counter, 19).SetValue<string>("");

                    worksheet.Cell(counter, 17).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(counter, 17).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    worksheet.Cell(counter, 17).Style.Font.FontSize = 12;
                    worksheet.Cell(counter, 17).Style.Font.Bold = true;

                    worksheet.Cell(counter, 18).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(counter, 18).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    worksheet.Cell(counter, 18).Style.Font.FontSize = 12;
                    worksheet.Cell(counter, 18).Style.Font.Bold = true;

                    if (budgetPeriodo > 0)
                    {
                        worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(0, 176, 80);
                    }
                    else if (budgetPeriodo < 0)
                    {
                        worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(255, 0, 0);
                    }
                    else
                    {
                        worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
                    }

                    if (budgetAnno > 0)
                    {
                        worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(0, 176, 80);
                    }
                    else if (budgetAnno < 0)
                    {
                        worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(255, 0, 0);
                    }
                    else
                    {
                        worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
                    }

                    for (int nCell = 1; nCell <= 19; nCell++)
                    {
                        worksheet.Cell(counter, nCell).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(counter, nCell).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(counter, nCell).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(counter, nCell).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(counter, nCell).Style.Alignment.WrapText = true;
                    }
                    worksheet.Range(counter, 1, counter, 16).Merge();
                    worksheet.Row(counter).Height = 30;

                    #endregion

                    #region Riepilogo Delta
                    counter++;
                    worksheet.Cell(counter, 1).SetValue<string>("Delta");
                    worksheet.Cell(counter, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(counter, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    worksheet.Cell(counter, 1).Style.Font.FontSize = 12;
                    worksheet.Cell(counter, 1).Style.Font.Bold = true;

                    worksheet.Cell(counter, 17).SetValue<string>(String.Format("€ {0:N2}", budgetDeltaPeriodo));
                    worksheet.Cell(counter, 18).SetValue<string>(String.Format("€ {0:N2}", budgetDeltaAnno));
                    worksheet.Cell(counter, 19).SetValue<string>("");

                    worksheet.Cell(counter, 17).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(counter, 17).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    worksheet.Cell(counter, 17).Style.Font.FontSize = 12;
                    worksheet.Cell(counter, 17).Style.Font.Bold = true;

                    worksheet.Cell(counter, 18).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(counter, 18).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    worksheet.Cell(counter, 18).Style.Font.FontSize = 12;
                    worksheet.Cell(counter, 18).Style.Font.Bold = true;

                    if (budgetDeltaPeriodo > 0)
                    {
                        worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(0, 176, 80);
                    }
                    else if (budgetDeltaPeriodo < 0)
                    {
                        worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(255, 0, 0);
                    }
                    else
                    {
                        worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
                    }

                    if (budgetDeltaAnno > 0)
                    {
                        worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(0, 176, 80);
                    }
                    else if (budgetDeltaAnno < 0)
                    {
                        worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(255, 0, 0);
                    }
                    else
                    {
                        worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
                    }

                    for (int nCell = 1; nCell <= 19; nCell++)
                    {
                        worksheet.Cell(counter, nCell).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(counter, nCell).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(counter, nCell).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(counter, nCell).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(counter, nCell).Style.Alignment.WrapText = true;
                    }
                    worksheet.Range(counter, 1, counter, 16).Merge();
                    worksheet.Row(counter).Height = 30;

                    #endregion

                    for (int nCell = 1; nCell <= 18; nCell++)
                    {
                        var ncol = worksheet.Column(nCell);
                        ncol.Width = 24;
                    }
                    var lastCol = worksheet.Column(19);
                    lastCol.Width = 50;
                }

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    memoryStream.Position = 0;
                    Session[handle] = memoryStream.ToArray();
                }
                nomeFile = String.Format("Report_{0}_{1}", campagna.Id, direzione.CODICE);
            }
            catch (Exception ex)
            {
                return new EmptyResult();
            }

            return new JsonResult()
            {
                Data = new { FileGuid = handle, FileName = nomeFile }
            };
        }

        /// <summary>
        /// Esporta simulazione
        /// </summary>
        /// <param name="idDir"></param>
        /// <param name="idCamp"></param>
        /// <returns></returns>
        public virtual ActionResult DownloadXLSX(int idDir, int idCamp, int? anno = null)
        {
            string handle = idDir.ToString() + idCamp.ToString();
            if (Session[handle] != null)
            {
                byte[] data = Session[handle] as byte[];
                Session[handle] = null;
                return File(data, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", handle + ".xlsx");
            }
            else
            {
                return new EmptyResult();
            }
        }

        /// <summary>
        /// Esporta direzioni
        /// </summary>
        /// <param name="idCamp"></param>
        /// <param name="idGruppo"></param>
        /// <returns></returns>
        public ActionResult EsportaAreaXLSXByGruppo(int idCamp, int idGruppo)
        {
            var campagna = this.BudgetDataController.GetCampagna(CommonHelper.GetCurrentUserMatricola(), idCamp);
            int idArea = 0;

            if (campagna != null && campagna.DettaglioCampagna != null && campagna.DettaglioCampagna.Any())
            {
                var gruppo = campagna.DettaglioCampagna.Skip(idGruppo - 1).Take(1).FirstOrDefault();

                if (gruppo != null)
                {
                    idArea = gruppo.IdArea;
                }
            }

            return EsportaAreaXLSX(idCamp, idArea);
        }

        /// <summary>
        /// Esporta direzioni
        /// Esporta dati direzione
        /// </summary>
        /// <param name="idCamp"></param>
        /// <param name="idArea"></param>
        /// <param name="anno">Eventuale anno selezionato</param>
        /// <returns></returns>
        public ActionResult EsportaAreaXLSX(int idCamp, int idArea, int? idDirezione = null, int? anno = null, bool soloCause = false)
        {
            var campagna = this.BudgetDataController.GetCampagna(CommonHelper.GetCurrentUserMatricola(), idCamp);
            List<int> campagneContenute = campagna.CampagneContenute;
            int counter = 0;

            List<int?> dt = new List<int?>();
            List<int> _dt = new List<int>();
            _dt = campagna.Decorrenze.OrderBy(w => w).Select(w => w.Year).Distinct().ToList();

            bool forcedNull = false;

            if (_dt == null || !_dt.Any())
            {
                forcedNull = true;
                dt.Add(null);
            }
            else
            {
                _dt.ForEach(w =>
                {
                    dt.Add(w);
                });
                dt.Add(null);
            }

            XLWorkbook workbook = new XLWorkbook();
            MemoryStream ms = new MemoryStream();

            foreach (var myDate in dt)
            {
                if (forcedNull)
                {
                    anno = null;
                }
                else
                {
                    anno = myDate;
                }

                List<PRV_DIREZIONE> listaOrdinata = new List<PRV_DIREZIONE>();

                if (!idDirezione.HasValue)
                {
                    List<PRV_DIREZIONE> direzioni = new List<PRV_DIREZIONE>();
                    if (campagneContenute.Count() == 0)
                        direzioni = this.BudgetDataController.GetDirezioniByIdArea(CommonHelper.GetCurrentUserMatricola(), idArea, idCamp, anno);
                    else
                        direzioni = this.BudgetDataController.GetDirezioniByIdArea(CommonHelper.GetCurrentUserMatricola(), idArea, campagneContenute, anno);

                    listaOrdinata = direzioni.OrderBy(w => w.Ordine.GetValueOrDefault()).ToList();
                }
                else
                {
                    var tempDir = this.BudgetDataController.GetDirezione(idDirezione.Value);
                    listaOrdinata.Add(new PRV_DIREZIONE()
                    {
                        ID_AREA = idArea,
                        ID_DIREZIONE = tempDir.ID_DIREZIONE,
                        CODICE = tempDir.CODICE,
                        NOME = tempDir.NOME,
                        Ordine = tempDir.ORDINE
                    });
                }

                bool enableQIO = PoliticheRetributiveHelper.EnableVis(PolRetrChiaveEnum.BudgetQIO);
                bool enableRS = PoliticheRetributiveHelper.EnableVis(PolRetrChiaveEnum.BudgetRS);
                if (enableQIO && enableRS && campagneContenute.Count() > 0)
                {
                    int idAreaRs = 0;
                    using (IncentiviEntities db = new IncentiviEntities())
                    {
                        idAreaRs = db.XR_PRV_AREA.FirstOrDefault(x => x.LV_ABIL.Contains(PoliticheRetributiveHelper.BUDGETRS_HRGA_SOTTO_FUNC)).ID_AREA;
                        List<PRV_DIREZIONE> direzioni2 = new List<PRV_DIREZIONE>();

                        var cercaDir = this.BudgetDataController.GetDirezioniByIdArea(CommonHelper.GetCurrentUserMatricola(), idAreaRs, campagneContenute, anno);

                        foreach (var item in cercaDir)
                        {
                            var dir = listaOrdinata.FirstOrDefault(x => x.CODICE == item.CODICE);
                            if (dir != null)
                            {
                                dir.ORGANICO += item.ORGANICO;
                                dir.ORGANICO_FEMMINILE = item.ORGANICO_FEMMINILE;
                                dir.ORGANICO_MASCHILE = item.ORGANICO_MASCHILE;
                                dir.BUDGET = item.BUDGET;
                                dir.BUDGET_PERIODO = item.BUDGET_PERIODO;
                                dir.ORGANICO_AD = item.ORGANICO_AD;
                                dir.ORGANICO_MASCHILE_AD = item.ORGANICO_MASCHILE_AD;
                                dir.ORGANICO_FEMMINILE_AD = item.ORGANICO_FEMMINILE_AD;
                                dir.ORGANICO_CONTABILE = item.ORGANICO_CONTABILE;
                            }
                            else
                            {
                                listaOrdinata.Add(item);
                            }
                        }
                    }
                }

                try
                {
                    if (listaOrdinata == null || !listaOrdinata.Any())
                    {
                        throw new Exception("Direzione mancante o non trovata");
                    }

                    foreach (var direz in listaOrdinata)
                    {
                        int idDir = direz.ID_DIREZIONE;
                        List<ReportItem> items = new List<ReportItem>();
                        var direzione = this.BudgetDataController.GetDirezione(idDir);
                        List<PRV_Dipendente> dipendenti = new List<PRV_Dipendente>();
                        if (campagneContenute.Count() == 0)
                            dipendenti = this.BudgetDataController.GetDipendentiInDirezione(idDir, idCamp, anno);
                        else
                            dipendenti = this.BudgetDataController.GetDipendentiInDirezione(idDir, campagneContenute, anno);

                        if ((dipendenti == null || !dipendenti.Any()) && !idDirezione.HasValue)
                        {
                            continue;
                        }

                        IXLWorksheet worksheet;
                        string worksheetName = "";
                        if (anno.HasValue)
                            worksheetName = String.Format("{0} {1}", anno, direzione.NOME).Truncate(25);
                        else
                            worksheetName = direzione.NOME.Truncate(25);

                        workbook.Worksheets.TryGetWorksheet(worksheetName, out worksheet);

                        if (worksheet == null)
                        {
                            worksheet = workbook.Worksheets.Add(worksheetName.Replace ("'",""));
                            worksheet.PageSetup.PaperSize = XLPaperSize.A3Paper;
                            worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                            worksheet.PageSetup.PagesWide = 1;
                        }

                        counter = 0;
                        decimal totAnno = 0;
                        decimal totPeriodo = 0;

                        #region Intestazione
                        this.ScriviIntestazioneExcel(campagna, ref worksheet);
                        counter++;
                        #endregion

                        #region Nome direzione
                        this.ScriviNomeDirezioneExcel(String.Format("{0} - {1}", direzione.NOME, direzione.CODICE), ref worksheet);
                        counter++;
                        #endregion

                        #region Intestazioni colonne
                        this.ScriviIntestazioniColonneExcel(ref worksheet);
                        counter += 2;
                        #endregion

                        #region Preparazione oggetto per la stampa su XLSX

                        if (dipendenti != null && dipendenti.Any())
                        {
                            using (IncentiviEntities db = new IncentiviEntities())
                            {
                                foreach (var d in dipendenti)
                                {
                                    string provvedimenti = "";
                                    if (d.Provvedimenti != null && d.Provvedimenti.Any())
                                    {
                                        string separatore = " ";
                                        foreach (var p in d.Provvedimenti)
                                        {
                                            string descr = ((ProvvedimentiEnum)p.IdProvvedimento).GetDescription();

                                            provvedimenti += String.Format("{0}{1} {2}\r\n", separatore, p.Data.ToString("yyyy/MM"), descr);
                                            separatore = " - ";
                                        }
                                    }

                                    provvedimenti = provvedimenti.Trim();

                                    string profilo = "";

                                    if (!String.IsNullOrEmpty(d.DescRuolo) &&
                                        !String.IsNullOrEmpty(d.CodRuolo))
                                    {
                                        profilo = d.DescRuolo.Replace(d.CodRuolo, "");
                                        profilo = profilo.Replace("-", "");
                                        profilo = profilo.Trim();
                                    }

                                    var provvedimento = db.XR_PRV_PROV.Where(w => w.ID_PROV.Equals(d.IdProvvedimento)).FirstOrDefault();
                                    string provv = "";

                                    if (provvedimento != null)
                                    {
                                        provv = provvedimento.DESCRIZIONE;
                                    }

                                    string provvRichDes = "";
                                    var provvRich = db.XR_PRV_PROV.Where(x => x.ID_PROV.Equals(d.IdProvvedimentoRich)).FirstOrDefault();
                                    if (provvRich != null)
                                        provvRichDes = provvRich.DESCRIZIONE;

                                    DateTime oggi = DateTime.Now;
                                    oggi = oggi.AddYears(-5);

                                    ReportItem r = new ReportItem()
                                    {
                                        Direzione = direzione.NOME,
                                        LivelloRichiesto = String.IsNullOrEmpty(d.LivPrevisto) ? d.LivAttuale : d.LivPrevisto,
                                        Livello = d.LivAttuale,
                                        CostoRegime = String.Format("€ {0:N2}", d.CostoAnnuo),
                                        CostoPeriodo = String.Format("€ {0:N2}", d.CostoPeriodo),
                                        Nominativo = d.Nominativo,
                                        RAL = String.Format("€ {0:N2}", d.RAL),
                                        UltimiProvv = provvedimenti,
                                        DecorrenzaRichiesta = (d.Decorrenza.HasValue ? d.Decorrenza.Value.ToString("dd/MM/yyyy") : ""),
                                        Profilo = profilo,
                                        ProvvRichiesto = provv,
                                        Struttura = d.Struttura,
                                        Note = d.Note,
                                        DataAssunzione = d.DataAssunzione,
                                        DataNascita = d.DataNascita,
                                        AnzianitaLivello = d.AnzianitaLivello,
                                        Reperibilita = String.Format("€ {0:N2}", d.Reperibilita),
                                        RetribuzioneVariabile = String.Format("€ {0:N2}", d.RetribuzioneVariabile),
                                        Assenze = "",
                                        AggregatoSede = d.AggregatoSede,
                                        Matricola = d.Matricola,
                                        ProvvOriginale = provvRichDes
                                    };

                                    if (r.ProvvRichiesto.ToUpper().Equals("GRATIFICA"))
                                    {
                                        r.CostoRegime = "€ 0,00";
                                        totAnno += 0;
                                    }
                                    else
                                    {
                                        totAnno += d.CostoAnnuo;
                                    }

                                    totPeriodo += d.CostoPeriodo;

                                    if (String.IsNullOrEmpty(r.Note))
                                    {
                                        r.Note = "";
                                    }

                                    if (r.ProvvRichiesto.ToUpper().Equals("AUMENTO DI MERITO") ||
                                        r.ProvvRichiesto.ToUpper().Equals("GRATIFICA"))
                                    {
                                        r.LivelloRichiesto = "";
                                    }

                                    // se ha cause aperte
                                    if (PoliticheRetributiveManager.HasCauseAperte(db, d.Matricola, d.IdDipendente))
                                    {
                                        r.Cause = "Contenzioso\r\n";
                                    }

                                    if (PoliticheRetributiveManager.HasVertSindacali(db, d.Matricola))
                                    {
                                        r.Vertenze = "Vert. sindacali\r\n";
                                    }

                                    if (PoliticheRetributiveManager.HasVertStragiudiziali(db, d.IdDipendente))
                                    {
                                        r.Vertenze += "Vert. stragiudiziali\r\n";
                                    }

                                    if (PoliticheRetributiveManager.HasProvvedimentoDisciplinari(db, d.IdDipendente))
                                    {
                                        r.Vertenze += "Provv. disciplinari\r\n";
                                    }

                                    if (d.Assenze != null && d.Assenze.Any())
                                    {
                                        if (d.Assenze.Sum(w => w.Quantita) > 30)
                                        {
                                            r.Assenze = String.Format("{0} giorni di assenza\r\n", d.Assenze.Sum(w => w.Quantita));
                                            foreach (var a in d.Assenze)
                                            {
                                                r.Assenze += String.Format("{0} - {1}\r\n", a.Codice, a.Quantita);
                                            }
                                        }
                                    }

                                    if (!String.IsNullOrEmpty(r.Assenze))
                                    {
                                        r.Note += String.Format("{0}\r\n{1}\r\n{2}", r.Vertenze, r.Cause, r.Assenze);
                                    }
                                    else
                                    {
                                        r.Note += String.Format("{0}\r\n{1}", r.Vertenze, r.Cause);
                                    }
                                    r.Note = r.Note.Trim();

                                    if (!soloCause || (!String.IsNullOrWhiteSpace(r.Cause) || !String.IsNullOrWhiteSpace(r.Vertenze)))
                                        items.Add(r);
                                }
                            }
                        }

                        #endregion

                        #region Stampa Corpo Excel
                        if (items != null && items.Any())
                        {
                            var provvs = items.GroupBy(w => w.ProvvRichiesto).Distinct();

                            var groups = items.GroupBy(w => w.ProvvRichiesto).Select(g => new
                            {
                                Provvedimento = g.Key
                            });

                            int contaTipologia = groups.Count();

                            var p = items.GroupBy(w => w.ProvvRichiesto).OrderBy(w => w.Key);

                            var ordered = p.Where(w => w.Key.ToUpper().Equals("PROMOZIONE")).ToList();
                            ordered.AddRange(p.Where(w => w.Key.ToUpper().Equals("AUMENTO DI MERITO")).ToList());
                            ordered.AddRange(p.Where(w => w.Key.ToUpper().Equals("GRATIFICA")).ToList());
                            ordered.AddRange(p.Where(w => w.Key.ToUpper().Equals("PROMOZIONE SENZA ASSORBIMENTO")).ToList());

                            foreach (var itemsGroup in ordered)
                            {
                                worksheet.Cell(counter + 1, 1).SetValue<string>(itemsGroup.Key);
                                worksheet.Cell(counter + 1, 1).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                worksheet.Cell(counter + 1, 1).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                worksheet.Cell(counter + 1, 1).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                worksheet.Cell(counter + 1, 1).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                                decimal regimeParziale = 0;
                                decimal periodoParziale = 0;

                                foreach (var i in itemsGroup)
                                {
                                    counter++;

                                    worksheet.Cell(counter, 2).SetValue<string>(i.Struttura);
                                    worksheet.Cell(counter, 3).SetValue<string>(i.AggregatoSede);
                                    worksheet.Cell(counter, 4).SetValue<string>(i.Nominativo);
                                    worksheet.Cell(counter, 5).SetValue<string>(i.Matricola);
                                    worksheet.Cell(counter, 6).SetValue<string>((!i.DataNascita.HasValue) ? "" : i.DataNascita.Value.ToString("dd/MM/yyyy"));
                                    worksheet.Cell(counter, 7).SetValue<string>((!i.DataAssunzione.HasValue) ? "" : i.DataAssunzione.Value.ToString("dd/MM/yyyy"));
                                    worksheet.Cell(counter, 8).SetValue<string>((!i.AnzianitaLivello.HasValue) ? "" : i.AnzianitaLivello.Value.ToString("dd/MM/yyyy"));
                                    worksheet.Cell(counter, 9).SetValue<string>(i.Profilo);
                                    worksheet.Cell(counter, 10).SetValue<string>(i.Livello);
                                    worksheet.Cell(counter, 11).SetValue<string>(i.UltimiProvv);
                                    worksheet.Cell(counter, 12).SetValue<string>(i.RAL);
                                    worksheet.Cell(counter, 13).SetValue<string>(i.RetribuzioneVariabile);
                                    worksheet.Cell(counter, 14).SetValue<string>(i.Reperibilita);
                                    worksheet.Cell(counter, 15).SetValue<string>(i.LivelloRichiesto);
                                    worksheet.Cell(counter, 16).SetValue<string>(i.DecorrenzaRichiesta);
                                    worksheet.Cell(counter, 17).SetValue<string>(i.CostoPeriodo);
                                    worksheet.Cell(counter, 18).SetValue<string>(i.CostoRegime);
                                    worksheet.Cell(counter, 19).SetValue<string>(i.Note.Trim());

                                    for (int nCell = 1; nCell <= 19; nCell++)
                                    {
                                        worksheet.Cell(counter, nCell).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                        worksheet.Cell(counter, nCell).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                        worksheet.Cell(counter, nCell).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                        worksheet.Cell(counter, nCell).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                        worksheet.Cell(counter, nCell).Style.Alignment.WrapText = true;
                                        if ((nCell >= 12 && nCell <= 14) ||
                                             (nCell >= 17 && nCell <= 18))
                                        {
                                            worksheet.Cell(counter, nCell).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                        }
                                        else
                                        {
                                            worksheet.Cell(counter, nCell).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                        }

                                        worksheet.Cell(counter, nCell).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                        worksheet.Cell(counter, nCell).Style.Font.FontSize = 12;
                                    }

                                    worksheet.Row(counter).Height = 48;
                                    worksheet.Row(counter).Style.Alignment.SetWrapText(true);

                                    string tempRegimeParziale = String.IsNullOrEmpty(i.CostoRegime) ? "0" : i.CostoRegime;
                                    string tempPeriodoParziale = String.IsNullOrEmpty(i.CostoPeriodo) ? "0" : i.CostoPeriodo;

                                    tempRegimeParziale = tempRegimeParziale.Replace("€", "");
                                    tempRegimeParziale = tempRegimeParziale.Trim();

                                    tempPeriodoParziale = tempPeriodoParziale.Replace("€", "");
                                    tempPeriodoParziale = tempPeriodoParziale.Trim();

                                    decimal reg = Convert.ToDecimal(tempRegimeParziale, System.Globalization.CultureInfo.CurrentCulture);
                                    decimal per = Convert.ToDecimal(tempPeriodoParziale, System.Globalization.CultureInfo.CurrentCulture);

                                    regimeParziale += reg;
                                    periodoParziale += per;
                                }

                                #region totale parziale
                                counter++;
                                worksheet.Cell(counter, 1).SetValue<string>("Totale parziale");
                                worksheet.Cell(counter, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                worksheet.Cell(counter, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                worksheet.Cell(counter, 1).Style.Font.FontSize = 12;
                                worksheet.Cell(counter, 1).Style.Font.Bold = true;

                                worksheet.Cell(counter, 17).SetValue<string>(String.Format("€ {0:N2}", periodoParziale));
                                worksheet.Cell(counter, 18).SetValue<string>(String.Format("€ {0:N2}", regimeParziale));
                                worksheet.Cell(counter, 19).SetValue<string>("");

                                worksheet.Cell(counter, 17).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                worksheet.Cell(counter, 17).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                worksheet.Cell(counter, 17).Style.Font.FontSize = 12;
                                worksheet.Cell(counter, 17).Style.Font.Bold = true;

                                worksheet.Cell(counter, 18).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                worksheet.Cell(counter, 18).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                worksheet.Cell(counter, 18).Style.Font.FontSize = 12;
                                worksheet.Cell(counter, 18).Style.Font.Bold = true;

                                if (periodoParziale > 0)
                                {
                                    worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(0, 176, 80);
                                }
                                else if (periodoParziale < 0)
                                {
                                    worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(255, 0, 0);
                                }
                                else
                                {
                                    worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
                                }

                                if (regimeParziale > 0)
                                {
                                    worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(0, 176, 80);
                                }
                                else if (regimeParziale < 0)
                                {
                                    worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(255, 0, 0);
                                }
                                else
                                {
                                    worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
                                }

                                for (int nCell = 1; nCell <= 19; nCell++)
                                {
                                    worksheet.Cell(counter, nCell).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                    worksheet.Cell(counter, nCell).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                    worksheet.Cell(counter, nCell).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                    worksheet.Cell(counter, nCell).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                    worksheet.Cell(counter, nCell).Style.Alignment.WrapText = true;
                                }
                                worksheet.Range(counter, 1, counter, 16).Merge();
                                worksheet.Row(counter).Height = 30;

                                #endregion
                                counter++;
                            }

                            counter++;
                            worksheet.Cell(counter, 1).SetValue<string>("Totale complessivo");
                            worksheet.Cell(counter, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            worksheet.Cell(counter, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            worksheet.Cell(counter, 1).Style.Font.FontSize = 12;
                            worksheet.Cell(counter, 1).Style.Font.Bold = true;

                            worksheet.Cell(counter, 17).SetValue<string>(String.Format("€ {0:N2}", totPeriodo));
                            worksheet.Cell(counter, 18).SetValue<string>(String.Format("€ {0:N2}", totAnno));
                            worksheet.Cell(counter, 19).SetValue<string>("");

                            worksheet.Cell(counter, 17).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            worksheet.Cell(counter, 17).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            worksheet.Cell(counter, 17).Style.Font.FontSize = 12;
                            worksheet.Cell(counter, 17).Style.Font.Bold = true;

                            worksheet.Cell(counter, 18).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            worksheet.Cell(counter, 18).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            worksheet.Cell(counter, 18).Style.Font.FontSize = 12;
                            worksheet.Cell(counter, 18).Style.Font.Bold = true;

                            if (totPeriodo > 0)
                            {
                                worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(0, 176, 80);
                            }
                            else if (totPeriodo < 0)
                            {
                                worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(255, 0, 0);
                            }
                            else
                            {
                                worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
                            }

                            if (totAnno > 0)
                            {
                                worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(0, 176, 80);
                            }
                            else if (totAnno < 0)
                            {
                                worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(255, 0, 0);
                            }
                            else
                            {
                                worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
                            }

                            for (int nCell = 1; nCell <= 19; nCell++)
                            {
                                worksheet.Cell(counter, nCell).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                worksheet.Cell(counter, nCell).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                worksheet.Cell(counter, nCell).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                worksheet.Cell(counter, nCell).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                worksheet.Cell(counter, nCell).Style.Alignment.WrapText = true;
                            }
                            worksheet.Range(counter, 1, counter, 16).Merge();
                            worksheet.Row(counter).Height = 30;

                            counter++;
                        }

                        #endregion

                        decimal budgetAnno = 0;
                        decimal budgetPeriodo = 0;
                        decimal costoAnno = 0;
                        decimal costoPeriodo = 0;
                        decimal budgetDeltaAnno = 0;
                        decimal budgetDeltaPeriodo = 0;

                        if (campagneContenute.Count() == 0)
                        {
                            this.BudgetDataController.GetBudgetAnnuoPeriodoDirezione(idDir, idCamp, out budgetAnno, out budgetPeriodo);
                            this.BudgetDataController.GetCostoAnnuoPeriodoDirezione(idDir, idCamp, out costoAnno, out costoPeriodo, anno);
                        }
                        else
                        {
                            this.BudgetDataController.GetBudgetAnnuoPeriodoDirezione(idDir, campagneContenute, out budgetAnno, out budgetPeriodo);
                            this.BudgetDataController.GetCostoAnnuoPeriodoDirezione(idDir, campagneContenute, out costoAnno, out costoPeriodo, anno);
                        }

                        budgetDeltaAnno = budgetAnno - costoAnno;
                        budgetDeltaPeriodo = budgetPeriodo - costoPeriodo;

                        if (anno.HasValue && anno.Value != DateTime.Now.Year)
                        {
                            budgetAnno = 0;
                            //costoAnno = 0;
                            costoPeriodo = 0;
                            budgetPeriodo = 0;
                            budgetDeltaAnno = 0;
                            budgetDeltaPeriodo = 0;
                        }

                        #region Riepilogo Budget
                        counter = counter + 2;
                        worksheet.Cell(counter, 1).SetValue<string>("Budget");
                        worksheet.Cell(counter, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(counter, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell(counter, 1).Style.Font.FontSize = 12;
                        worksheet.Cell(counter, 1).Style.Font.Bold = true;

                        worksheet.Cell(counter, 17).SetValue<string>(String.Format("€ {0:N2}", budgetPeriodo));
                        worksheet.Cell(counter, 18).SetValue<string>(String.Format("€ {0:N2}", budgetAnno));
                        worksheet.Cell(counter, 19).SetValue<string>("");

                        worksheet.Cell(counter, 17).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(counter, 17).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell(counter, 17).Style.Font.FontSize = 12;
                        worksheet.Cell(counter, 17).Style.Font.Bold = true;

                        worksheet.Cell(counter, 18).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(counter, 18).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell(counter, 18).Style.Font.FontSize = 12;
                        worksheet.Cell(counter, 18).Style.Font.Bold = true;

                        if (budgetPeriodo > 0)
                        {
                            worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(0, 176, 80);
                        }
                        else if (budgetPeriodo < 0)
                        {
                            worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(255, 0, 0);
                        }
                        else
                        {
                            worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
                        }

                        if (budgetAnno > 0)
                        {
                            worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(0, 176, 80);
                        }
                        else if (budgetAnno < 0)
                        {
                            worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(255, 0, 0);
                        }
                        else
                        {
                            worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
                        }

                        for (int nCell = 1; nCell <= 19; nCell++)
                        {
                            worksheet.Cell(counter, nCell).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                            worksheet.Cell(counter, nCell).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                            worksheet.Cell(counter, nCell).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                            worksheet.Cell(counter, nCell).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                            worksheet.Cell(counter, nCell).Style.Alignment.WrapText = true;
                        }
                        worksheet.Range(counter, 1, counter, 16).Merge();
                        worksheet.Row(counter).Height = 30;

                        #endregion

                        #region Riepilogo Delta
                        counter++;
                        worksheet.Cell(counter, 1).SetValue<string>("Delta");
                        worksheet.Cell(counter, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(counter, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell(counter, 1).Style.Font.FontSize = 12;
                        worksheet.Cell(counter, 1).Style.Font.Bold = true;

                        worksheet.Cell(counter, 17).SetValue<string>(String.Format("€ {0:N2}", budgetDeltaPeriodo));
                        worksheet.Cell(counter, 18).SetValue<string>(String.Format("€ {0:N2}", budgetDeltaAnno));
                        worksheet.Cell(counter, 19).SetValue<string>("");

                        worksheet.Cell(counter, 17).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(counter, 17).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell(counter, 17).Style.Font.FontSize = 12;
                        worksheet.Cell(counter, 17).Style.Font.Bold = true;

                        worksheet.Cell(counter, 18).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(counter, 18).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell(counter, 18).Style.Font.FontSize = 12;
                        worksheet.Cell(counter, 18).Style.Font.Bold = true;

                        if (budgetDeltaPeriodo > 0)
                        {
                            worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(0, 176, 80);
                        }
                        else if (budgetDeltaPeriodo < 0)
                        {
                            worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(255, 0, 0);
                        }
                        else
                        {
                            worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
                        }

                        if (budgetDeltaAnno > 0)
                        {
                            worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(0, 176, 80);
                        }
                        else if (budgetDeltaAnno < 0)
                        {
                            worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(255, 0, 0);
                        }
                        else
                        {
                            worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
                        }

                        for (int nCell = 1; nCell <= 19; nCell++)
                        {
                            worksheet.Cell(counter, nCell).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                            worksheet.Cell(counter, nCell).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                            worksheet.Cell(counter, nCell).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                            worksheet.Cell(counter, nCell).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                            worksheet.Cell(counter, nCell).Style.Alignment.WrapText = true;
                        }
                        worksheet.Range(counter, 1, counter, 16).Merge();
                        worksheet.Row(counter).Height = 30;

                        #endregion

                        if (items.Any(x => x.ProvvRichiesto.ToUpper().Equals("NESSUNO")))
                        {
                            counter += 2;
                            worksheet.Cell(counter, 2).SetValue<string>("RICHIESTE ESCLUSE:");
                            var p = items.Where(x => x.ProvvRichiesto.ToUpper().Equals("NESSUNO")).GroupBy(x => x.ProvvOriginale);
                            var ordered = p.Where(w => w.Key.ToUpper().Equals("PROMOZIONE")).ToList();
                            ordered.AddRange(p.Where(w => w.Key.ToUpper().Equals("AUMENTO DI MERITO")).ToList());
                            ordered.AddRange(p.Where(w => w.Key.ToUpper().Equals("GRATIFICA")).ToList());
                            ordered.AddRange(p.Where(w => w.Key.ToUpper().Equals("PROMOZIONE SENZA ASSORBIMENTO")).ToList());

                            foreach (var itemsGroup in ordered)
                            {
                                worksheet.Cell(counter + 1, 1).SetValue<string>(itemsGroup.Key);
                                worksheet.Cell(counter + 1, 1).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                worksheet.Cell(counter + 1, 1).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                worksheet.Cell(counter + 1, 1).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                worksheet.Cell(counter + 1, 1).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                                decimal regimeParziale = 0;
                                decimal periodoParziale = 0;

                                foreach (var i in itemsGroup)
                                {
                                    counter++;

                                    worksheet.Cell(counter, 2).SetValue<string>(i.Struttura);
                                    worksheet.Cell(counter, 3).SetValue<string>(i.AggregatoSede);
                                    worksheet.Cell(counter, 4).SetValue<string>(i.Nominativo);
                                    worksheet.Cell(counter, 5).SetValue<string>(i.Matricola);
                                    worksheet.Cell(counter, 6).SetValue<string>((!i.DataNascita.HasValue) ? "" : i.DataNascita.Value.ToString("dd/MM/yyyy"));
                                    worksheet.Cell(counter, 7).SetValue<string>((!i.DataAssunzione.HasValue) ? "" : i.DataAssunzione.Value.ToString("dd/MM/yyyy"));
                                    worksheet.Cell(counter, 8).SetValue<string>((!i.AnzianitaLivello.HasValue) ? "" : i.AnzianitaLivello.Value.ToString("dd/MM/yyyy"));
                                    worksheet.Cell(counter, 9).SetValue<string>(i.Profilo);
                                    worksheet.Cell(counter, 10).SetValue<string>(i.Livello);
                                    worksheet.Cell(counter, 11).SetValue<string>(i.UltimiProvv);
                                    worksheet.Cell(counter, 12).SetValue<string>(i.RAL);
                                    worksheet.Cell(counter, 13).SetValue<string>(i.RetribuzioneVariabile);
                                    worksheet.Cell(counter, 14).SetValue<string>(i.Reperibilita);
                                    worksheet.Cell(counter, 15).SetValue<string>(i.LivelloRichiesto);
                                    worksheet.Cell(counter, 16).SetValue<string>(i.DecorrenzaRichiesta);
                                    worksheet.Cell(counter, 17).SetValue<string>(i.CostoPeriodo);
                                    worksheet.Cell(counter, 18).SetValue<string>(i.CostoRegime);
                                    worksheet.Cell(counter, 19).SetValue<string>(i.Note.Trim());

                                    for (int nCell = 1; nCell <= 19; nCell++)
                                    {
                                        worksheet.Cell(counter, nCell).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                        worksheet.Cell(counter, nCell).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                        worksheet.Cell(counter, nCell).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                        worksheet.Cell(counter, nCell).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                        worksheet.Cell(counter, nCell).Style.Alignment.WrapText = true;
                                        if ((nCell >= 12 && nCell <= 14) ||
                                             (nCell >= 17 && nCell <= 18))
                                        {
                                            worksheet.Cell(counter, nCell).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                        }
                                        else
                                        {
                                            worksheet.Cell(counter, nCell).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                        }

                                        worksheet.Cell(counter, nCell).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                        worksheet.Cell(counter, nCell).Style.Font.FontSize = 12;
                                    }

                                    worksheet.Row(counter).Height = 48;
                                    worksheet.Row(counter).Style.Alignment.SetWrapText(true);

                                    string tempRegimeParziale = String.IsNullOrEmpty(i.CostoRegime) ? "0" : i.CostoRegime;
                                    string tempPeriodoParziale = String.IsNullOrEmpty(i.CostoPeriodo) ? "0" : i.CostoPeriodo;

                                    tempRegimeParziale = tempRegimeParziale.Replace("€", "");
                                    tempRegimeParziale = tempRegimeParziale.Trim();

                                    tempPeriodoParziale = tempPeriodoParziale.Replace("€", "");
                                    tempPeriodoParziale = tempPeriodoParziale.Trim();

                                    decimal reg = Convert.ToDecimal(tempRegimeParziale, System.Globalization.CultureInfo.CurrentCulture);
                                    decimal per = Convert.ToDecimal(tempPeriodoParziale, System.Globalization.CultureInfo.CurrentCulture);

                                    regimeParziale += reg;
                                    periodoParziale += per;
                                }

                            }
                        }

                        for (int nCell = 1; nCell <= 18; nCell++)
                        {
                            var ncol = worksheet.Column(nCell);
                            ncol.Width = 24;
                        }
                        var lastCol = worksheet.Column(19);
                        lastCol.Width = 50;
                    }

                    #region Riepiloghi figli

                    if (idDirezione.HasValue)
                    {
                        List<string> elencoDirezioni = new List<string>();
                        List<string> elencoFigli = new List<string>();

                        // se la direzione è valorizzata allora la stampa deve produrre un file excel per la singola direzione.
                        // visto che ci sono delle direzioni che hanno lo stesso dirigente si è deciso di accorparle in un 
                        // unico file excel singolo tab, questo significa che se la direzione è una di quelle che deve essere accorpata
                        // allora andrà stampata anche la direzione ad essa associata.

                        using (digiGappEntities _db = new digiGappEntities())
                        {
                            var parametriSistema = _db.MyRai_ParametriSistema.Where(w => w.Chiave.Equals("DirezioniAccorpatePerLaStampa")).FirstOrDefault();

                            if (parametriSistema != null)
                            {
                                string json = parametriSistema.Valore1;

                                var result = JsonConvert.DeserializeObject<DirezioniAccorpatePerLaStampaList>(json);

                                elencoDirezioni = result.DirezioniAccorpatePerLaStampa.Select(p => p.Direzione).ToList();
                            }
                        }

                        if (elencoDirezioni != null && elencoDirezioni.Any())
                        {
                            foreach (var item in elencoDirezioni)
                            {
                                var _temp = item.Split('|').ToList();
                                if (_temp != null && _temp.Any())
                                {
                                    int idToFind = int.Parse(_temp[0]);
                                    if (idToFind.Equals(idDirezione.Value))
                                    {
                                        _temp.RemoveAt(0);
                                        elencoFigli = _temp;
                                        break;
                                    }
                                }
                            }

                            if (elencoFigli != null && elencoFigli.Any())
                            {
                                var direzione = this.BudgetDataController.GetDirezione(idDirezione.Value);
                                IXLWorksheet worksheet;
                                workbook.Worksheets.TryGetWorksheet(String.Format("{0} {1}", anno, direzione.NOME).Truncate(25), out worksheet);

                                if (worksheet == null)
                                {
                                    worksheet = workbook.Worksheets.Add(String.Format("{0} {1}", direzione.NOME, direzione.CODICE).Truncate(25));
                                    worksheet.PageSetup.PaperSize = XLPaperSize.A3Paper;
                                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                                    worksheet.PageSetup.PagesWide = 1;
                                }

                                foreach (var figlio in elencoFigli)
                                {
                                    var idDir = int.Parse(figlio);
                                    var items = new List<ReportItem>();
                                    direzione = this.BudgetDataController.GetDirezione(idDir);
                                    //var dipendenti = this.BudgetDataController.GetDipendentiInDirezione ( idDir , idCamp );
                                    List<PRV_Dipendente> dipendenti = new List<PRV_Dipendente>();
                                    if (campagneContenute.Count() == 0)
                                        dipendenti = this.BudgetDataController.GetDipendentiInDirezione(idDir, idCamp, anno);
                                    else
                                        dipendenti = this.BudgetDataController.GetDipendentiInDirezione(idDir, campagneContenute, anno);


                                    counter += 10;
                                    decimal totAnno = 0;
                                    decimal totPeriodo = 0;

                                    #region Intestazione
                                    this.ScriviIntestazioneExcel(campagna, ref worksheet, counter);
                                    #endregion

                                    #region Nome direzione
                                    this.ScriviNomeDirezioneExcel(String.Format("{0} - {1}", direzione.NOME, direzione.CODICE), ref worksheet, counter);
                                    #endregion

                                    #region Intestazioni colonne
                                    this.ScriviIntestazioniColonneExcel(ref worksheet, counter);
                                    #endregion

                                    #region Preparazione oggetto per la stampa su XLSX

                                    counter += 4;
                                    if (dipendenti != null && dipendenti.Any())
                                    {
                                        using (IncentiviEntities db = new IncentiviEntities())
                                        {
                                            foreach (var d in dipendenti)
                                            {
                                                string provvedimenti = "";
                                                if (d.Provvedimenti != null && d.Provvedimenti.Any())
                                                {
                                                    string separatore = " ";
                                                    foreach (var p in d.Provvedimenti)
                                                    {
                                                        string descr = ((ProvvedimentiEnum)p.IdProvvedimento).GetDescription();

                                                        provvedimenti += String.Format("{0}{1} {2}\r\n", separatore, p.Data.ToString("yyyy/MM"), descr);
                                                        separatore = " - ";
                                                    }
                                                }

                                                provvedimenti = provvedimenti.Trim();

                                                string profilo = "";

                                                if (!String.IsNullOrEmpty(d.DescRuolo) &&
                                                    !String.IsNullOrEmpty(d.CodRuolo))
                                                {
                                                    profilo = d.DescRuolo.Replace(d.CodRuolo, "");
                                                    profilo = profilo.Replace("-", "");
                                                    profilo = profilo.Trim();
                                                }

                                                var provvedimento = db.XR_PRV_PROV.Where(w => w.ID_PROV.Equals(d.IdProvvedimento)).FirstOrDefault();
                                                string provv = "";

                                                if (provvedimento != null)
                                                {
                                                    provv = provvedimento.DESCRIZIONE;
                                                }

                                                DateTime oggi = DateTime.Now;
                                                oggi = oggi.AddYears(-5);

                                                ReportItem r = new ReportItem()
                                                {
                                                    Direzione = direzione.NOME,
                                                    LivelloRichiesto = String.IsNullOrEmpty(d.LivPrevisto) ? d.LivAttuale : d.LivPrevisto,
                                                    Livello = d.LivAttuale,
                                                    CostoRegime = String.Format("€ {0:N2}", d.CostoAnnuo),
                                                    CostoPeriodo = String.Format("€ {0:N2}", d.CostoPeriodo),
                                                    Nominativo = d.Nominativo,
                                                    RAL = String.Format("€ {0:N2}", d.RAL),
                                                    UltimiProvv = provvedimenti,
                                                    DecorrenzaRichiesta = (d.Decorrenza.HasValue ? d.Decorrenza.Value.ToString("dd/MM/yyyy") : ""),
                                                    Profilo = profilo,
                                                    ProvvRichiesto = provv,
                                                    Struttura = d.Struttura,
                                                    Note = d.Note,
                                                    DataAssunzione = d.DataAssunzione,
                                                    DataNascita = d.DataNascita,
                                                    AnzianitaLivello = d.AnzianitaLivello,
                                                    Reperibilita = String.Format("€ {0:N2}", d.Reperibilita),
                                                    RetribuzioneVariabile = String.Format("€ {0:N2}", d.RetribuzioneVariabile),
                                                    Assenze = "",
                                                    AggregatoSede = d.AggregatoSede,
                                                    Matricola = d.Matricola
                                                };

                                                if (r.ProvvRichiesto.ToUpper().Equals("GRATIFICA"))
                                                {
                                                    r.CostoRegime = "€ 0,00";
                                                    totAnno += 0;
                                                }
                                                else
                                                {
                                                    totAnno += d.CostoAnnuo;
                                                }

                                                totPeriodo += d.CostoPeriodo;

                                                if (String.IsNullOrEmpty(r.Note))
                                                {
                                                    r.Note = "";
                                                }

                                                if (r.ProvvRichiesto.ToUpper().Equals("AUMENTO DI MERITO") ||
                                                    r.ProvvRichiesto.ToUpper().Equals("GRATIFICA"))
                                                {
                                                    r.LivelloRichiesto = "";
                                                }

                                                // se ha cause aperte
                                                if (PoliticheRetributiveManager.HasCauseAperte(db, d.Matricola, d.IdDipendente))
                                                {
                                                    r.Cause = "Contenzioso\r\n";
                                                }

                                                if (PoliticheRetributiveManager.HasVertSindacali(db, d.Matricola))
                                                {
                                                    r.Vertenze = "Vert. sindacali\r\n";
                                                }

                                                if (PoliticheRetributiveManager.HasVertStragiudiziali(db, d.IdDipendente))
                                                {
                                                    r.Vertenze += "Vert. stragiudiziali\r\n";
                                                }


                                                if (PoliticheRetributiveManager.HasProvvedimentoDisciplinari(db, d.IdDipendente))
                                                {
                                                    r.Vertenze += "Provv. disciplinari\r\n";

                                                }
                                                if (d.Assenze != null && d.Assenze.Any())
                                                {
                                                    if (d.Assenze.Sum(w => w.Quantita) > 30)
                                                    {
                                                        r.Assenze = String.Format("{0} giorni di assenza\r\n", d.Assenze.Sum(w => w.Quantita));
                                                        foreach (var a in d.Assenze)
                                                        {
                                                            r.Assenze += String.Format("{0} - {1}\r\n", a.Codice, a.Quantita);
                                                        }
                                                    }
                                                }

                                                if (!String.IsNullOrEmpty(r.Assenze))
                                                {
                                                    r.Note += String.Format("{0}\r\n{1}\r\n{2}", r.Vertenze, r.Cause, r.Assenze);
                                                }
                                                else
                                                {
                                                    r.Note += String.Format("{0}\r\n{1}", r.Vertenze, r.Cause);
                                                }
                                                r.Note = r.Note.Trim();
                                                if (!soloCause || (!String.IsNullOrWhiteSpace(r.Cause) || !String.IsNullOrWhiteSpace(r.Vertenze)))
                                                    items.Add(r);
                                            }
                                        }
                                    }

                                    #endregion

                                    #region Stampa Corpo Excel
                                    if (items != null && items.Any())
                                    {
                                        var provvs = items.GroupBy(w => w.ProvvRichiesto).Distinct();

                                        var groups = items.GroupBy(w => w.ProvvRichiesto).Select(g => new
                                        {
                                            Provvedimento = g.Key
                                        });

                                        int contaTipologia = groups.Count();

                                        var p = items.GroupBy(w => w.ProvvRichiesto).OrderBy(w => w.Key);

                                        var ordered = p.Where(w => w.Key.ToUpper().Equals("PROMOZIONE")).ToList();
                                        ordered.AddRange(p.Where(w => w.Key.ToUpper().Equals("AUMENTO DI MERITO")).ToList());
                                        ordered.AddRange(p.Where(w => w.Key.ToUpper().Equals("GRATIFICA")).ToList());
                                        ordered.AddRange(p.Where(w => w.Key.ToUpper().Equals("PROMOZIONE SENZA ASSORBIMENTO")).ToList());

                                        foreach (var itemsGroup in ordered)
                                        {
                                            worksheet.Cell(counter + 1, 1).SetValue<string>(itemsGroup.Key);
                                            worksheet.Cell(counter + 1, 1).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                            worksheet.Cell(counter + 1, 1).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                            worksheet.Cell(counter + 1, 1).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                            worksheet.Cell(counter + 1, 1).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                                            decimal regimeParziale = 0;
                                            decimal periodoParziale = 0;

                                            foreach (var i in itemsGroup)
                                            {
                                                counter++;

                                                worksheet.Cell(counter, 2).SetValue<string>(i.Struttura);
                                                worksheet.Cell(counter, 3).SetValue<string>(i.AggregatoSede);
                                                worksheet.Cell(counter, 4).SetValue<string>(i.Nominativo);
                                                worksheet.Cell(counter, 5).SetValue<string>(i.Matricola);
                                                worksheet.Cell(counter, 6).SetValue<string>((!i.DataNascita.HasValue) ? "" : i.DataNascita.Value.ToString("dd/MM/yyyy"));
                                                worksheet.Cell(counter, 7).SetValue<string>((!i.DataAssunzione.HasValue) ? "" : i.DataAssunzione.Value.ToString("dd/MM/yyyy"));
                                                worksheet.Cell(counter, 8).SetValue<string>((!i.AnzianitaLivello.HasValue) ? "" : i.AnzianitaLivello.Value.ToString("dd/MM/yyyy"));
                                                worksheet.Cell(counter, 9).SetValue<string>(i.Profilo);
                                                worksheet.Cell(counter, 10).SetValue<string>(i.Livello);
                                                worksheet.Cell(counter, 11).SetValue<string>(i.UltimiProvv);
                                                worksheet.Cell(counter, 12).SetValue<string>(i.RAL);
                                                worksheet.Cell(counter, 13).SetValue<string>(i.RetribuzioneVariabile);
                                                worksheet.Cell(counter, 14).SetValue<string>(i.Reperibilita);
                                                worksheet.Cell(counter, 15).SetValue<string>(i.LivelloRichiesto);
                                                worksheet.Cell(counter, 16).SetValue<string>(i.DecorrenzaRichiesta);
                                                worksheet.Cell(counter, 17).SetValue<string>(i.CostoPeriodo);
                                                worksheet.Cell(counter, 18).SetValue<string>(i.CostoRegime);
                                                worksheet.Cell(counter, 19).SetValue<string>(i.Note.Trim());

                                                for (int nCell = 1; nCell <= 19; nCell++)
                                                {
                                                    worksheet.Cell(counter, nCell).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                                    worksheet.Cell(counter, nCell).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                                    worksheet.Cell(counter, nCell).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                                    worksheet.Cell(counter, nCell).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                                    worksheet.Cell(counter, nCell).Style.Alignment.WrapText = true;
                                                    if ((nCell >= 12 && nCell <= 14) ||
                                                         (nCell >= 17 && nCell <= 18))
                                                    {
                                                        worksheet.Cell(counter, nCell).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                                    }
                                                    else
                                                    {
                                                        worksheet.Cell(counter, nCell).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                                    }

                                                    worksheet.Cell(counter, nCell).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                                    worksheet.Cell(counter, nCell).Style.Font.FontSize = 12;
                                                }

                                                worksheet.Row(counter).Height = 48;
                                                worksheet.Row(counter).Style.Alignment.SetWrapText(true);

                                                string tempRegimeParziale = String.IsNullOrEmpty(i.CostoRegime) ? "0" : i.CostoRegime;
                                                string tempPeriodoParziale = String.IsNullOrEmpty(i.CostoPeriodo) ? "0" : i.CostoPeriodo;

                                                tempRegimeParziale = tempRegimeParziale.Replace("€", "");
                                                tempRegimeParziale = tempRegimeParziale.Trim();

                                                tempPeriodoParziale = tempPeriodoParziale.Replace("€", "");
                                                tempPeriodoParziale = tempPeriodoParziale.Trim();

                                                decimal reg = Convert.ToDecimal(tempRegimeParziale, System.Globalization.CultureInfo.CurrentCulture);
                                                decimal per = Convert.ToDecimal(tempPeriodoParziale, System.Globalization.CultureInfo.CurrentCulture);

                                                regimeParziale += reg;
                                                periodoParziale += per;
                                            }

                                            #region totale parziale
                                            counter++;
                                            worksheet.Cell(counter, 1).SetValue<string>("Totale parziale");
                                            worksheet.Cell(counter, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                            worksheet.Cell(counter, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                            worksheet.Cell(counter, 1).Style.Font.FontSize = 12;
                                            worksheet.Cell(counter, 1).Style.Font.Bold = true;

                                            worksheet.Cell(counter, 17).SetValue<string>(String.Format("€ {0:N2}", periodoParziale));
                                            worksheet.Cell(counter, 18).SetValue<string>(String.Format("€ {0:N2}", regimeParziale));
                                            worksheet.Cell(counter, 19).SetValue<string>("");

                                            worksheet.Cell(counter, 17).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                            worksheet.Cell(counter, 17).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                            worksheet.Cell(counter, 17).Style.Font.FontSize = 12;
                                            worksheet.Cell(counter, 17).Style.Font.Bold = true;

                                            worksheet.Cell(counter, 18).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                            worksheet.Cell(counter, 18).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                            worksheet.Cell(counter, 18).Style.Font.FontSize = 12;
                                            worksheet.Cell(counter, 18).Style.Font.Bold = true;

                                            if (periodoParziale > 0)
                                            {
                                                worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(0, 176, 80);
                                            }
                                            else if (periodoParziale < 0)
                                            {
                                                worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(255, 0, 0);
                                            }
                                            else
                                            {
                                                worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
                                            }

                                            if (regimeParziale > 0)
                                            {
                                                worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(0, 176, 80);
                                            }
                                            else if (regimeParziale < 0)
                                            {
                                                worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(255, 0, 0);
                                            }
                                            else
                                            {
                                                worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
                                            }

                                            for (int nCell = 1; nCell <= 19; nCell++)
                                            {
                                                worksheet.Cell(counter, nCell).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                                worksheet.Cell(counter, nCell).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                                worksheet.Cell(counter, nCell).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                                worksheet.Cell(counter, nCell).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                                worksheet.Cell(counter, nCell).Style.Alignment.WrapText = true;
                                            }
                                            worksheet.Range(counter, 1, counter, 16).Merge();
                                            worksheet.Row(counter).Height = 30;

                                            #endregion
                                            counter++;
                                        }

                                        counter++;
                                        worksheet.Cell(counter, 1).SetValue<string>("Totale complessivo");
                                        worksheet.Cell(counter, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                        worksheet.Cell(counter, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                        worksheet.Cell(counter, 1).Style.Font.FontSize = 12;
                                        worksheet.Cell(counter, 1).Style.Font.Bold = true;

                                        worksheet.Cell(counter, 17).SetValue<string>(String.Format("€ {0:N2}", totPeriodo));
                                        worksheet.Cell(counter, 18).SetValue<string>(String.Format("€ {0:N2}", totAnno));
                                        worksheet.Cell(counter, 19).SetValue<string>("");

                                        worksheet.Cell(counter, 17).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                        worksheet.Cell(counter, 17).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                        worksheet.Cell(counter, 17).Style.Font.FontSize = 12;
                                        worksheet.Cell(counter, 17).Style.Font.Bold = true;

                                        worksheet.Cell(counter, 18).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                        worksheet.Cell(counter, 18).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                        worksheet.Cell(counter, 18).Style.Font.FontSize = 12;
                                        worksheet.Cell(counter, 18).Style.Font.Bold = true;

                                        if (totPeriodo > 0)
                                        {
                                            worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(0, 176, 80);
                                        }
                                        else if (totPeriodo < 0)
                                        {
                                            worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(255, 0, 0);
                                        }
                                        else
                                        {
                                            worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
                                        }

                                        if (totAnno > 0)
                                        {
                                            worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(0, 176, 80);
                                        }
                                        else if (totAnno < 0)
                                        {
                                            worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(255, 0, 0);
                                        }
                                        else
                                        {
                                            worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
                                        }

                                        for (int nCell = 1; nCell <= 19; nCell++)
                                        {
                                            worksheet.Cell(counter, nCell).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                            worksheet.Cell(counter, nCell).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                            worksheet.Cell(counter, nCell).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                            worksheet.Cell(counter, nCell).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                            worksheet.Cell(counter, nCell).Style.Alignment.WrapText = true;
                                        }
                                        worksheet.Range(counter, 1, counter, 16).Merge();
                                        worksheet.Row(counter).Height = 30;

                                        counter++;
                                    }

                                    #endregion

                                    //decimal budgetAnno = this.BudgetDataController.GetBudgetDirezione ( idDir , idCamp );
                                    //decimal budgetPeriodo = this.BudgetDataController.GetBudgetPeriodoDirezione ( idDir , idCamp );
                                    //decimal budgetDeltaAnno = budgetAnno - this.BudgetDataController.GetCostoDirezione ( idDir , idCamp );
                                    //decimal budgetDeltaPeriodo = budgetPeriodo - this.BudgetDataController.GetCostoPeriodoDirezione ( idDir , idCamp );
                                    decimal budgetAnno = 0;
                                    decimal budgetPeriodo = 0;
                                    decimal costoAnno = 0;
                                    decimal costoPeriodo = 0;
                                    decimal budgetDeltaAnno = 0;
                                    decimal budgetDeltaPeriodo = 0;

                                    if (campagna.CampagneContenute.Count() == 0)
                                    {
                                        this.BudgetDataController.GetBudgetAnnuoPeriodoDirezione(idDir, idCamp, out budgetAnno, out budgetPeriodo);
                                        this.BudgetDataController.GetCostoAnnuoPeriodoDirezione(idDir, idCamp, out costoAnno, out costoPeriodo);
                                    }
                                    else
                                    {
                                        this.BudgetDataController.GetBudgetAnnuoPeriodoDirezione(idDir, campagneContenute, out budgetAnno, out budgetPeriodo);
                                        this.BudgetDataController.GetCostoAnnuoPeriodoDirezione(idDir, campagneContenute, out costoAnno, out costoPeriodo);
                                    }
                                    budgetDeltaAnno = budgetAnno - costoAnno;
                                    budgetDeltaPeriodo = budgetPeriodo - costoPeriodo;

                                    #region Riepilogo Budget
                                    counter = counter + 2;
                                    worksheet.Cell(counter, 1).SetValue<string>("Budget");
                                    worksheet.Cell(counter, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    worksheet.Cell(counter, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                    worksheet.Cell(counter, 1).Style.Font.FontSize = 12;
                                    worksheet.Cell(counter, 1).Style.Font.Bold = true;

                                    worksheet.Cell(counter, 17).SetValue<string>(String.Format("€ {0:N2}", budgetPeriodo));
                                    worksheet.Cell(counter, 18).SetValue<string>(String.Format("€ {0:N2}", budgetAnno));
                                    worksheet.Cell(counter, 19).SetValue<string>("");

                                    worksheet.Cell(counter, 17).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    worksheet.Cell(counter, 17).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                    worksheet.Cell(counter, 17).Style.Font.FontSize = 12;
                                    worksheet.Cell(counter, 17).Style.Font.Bold = true;

                                    worksheet.Cell(counter, 18).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    worksheet.Cell(counter, 18).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                    worksheet.Cell(counter, 18).Style.Font.FontSize = 12;
                                    worksheet.Cell(counter, 18).Style.Font.Bold = true;

                                    if (budgetPeriodo > 0)
                                    {
                                        worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(0, 176, 80);
                                    }
                                    else if (budgetPeriodo < 0)
                                    {
                                        worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(255, 0, 0);
                                    }
                                    else
                                    {
                                        worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
                                    }

                                    if (budgetAnno > 0)
                                    {
                                        worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(0, 176, 80);
                                    }
                                    else if (budgetAnno < 0)
                                    {
                                        worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(255, 0, 0);
                                    }
                                    else
                                    {
                                        worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
                                    }

                                    for (int nCell = 1; nCell <= 19; nCell++)
                                    {
                                        worksheet.Cell(counter, nCell).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                        worksheet.Cell(counter, nCell).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                        worksheet.Cell(counter, nCell).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                        worksheet.Cell(counter, nCell).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                        worksheet.Cell(counter, nCell).Style.Alignment.WrapText = true;
                                    }
                                    worksheet.Range(counter, 1, counter, 16).Merge();
                                    worksheet.Row(counter).Height = 30;

                                    #endregion

                                    #region Riepilogo Delta
                                    counter++;
                                    worksheet.Cell(counter, 1).SetValue<string>("Delta");
                                    worksheet.Cell(counter, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    worksheet.Cell(counter, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                    worksheet.Cell(counter, 1).Style.Font.FontSize = 12;
                                    worksheet.Cell(counter, 1).Style.Font.Bold = true;

                                    worksheet.Cell(counter, 17).SetValue<string>(String.Format("€ {0:N2}", budgetDeltaPeriodo));
                                    worksheet.Cell(counter, 18).SetValue<string>(String.Format("€ {0:N2}", budgetDeltaAnno));
                                    worksheet.Cell(counter, 19).SetValue<string>("");

                                    worksheet.Cell(counter, 17).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    worksheet.Cell(counter, 17).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                    worksheet.Cell(counter, 17).Style.Font.FontSize = 12;
                                    worksheet.Cell(counter, 17).Style.Font.Bold = true;

                                    worksheet.Cell(counter, 18).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                    worksheet.Cell(counter, 18).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                    worksheet.Cell(counter, 18).Style.Font.FontSize = 12;
                                    worksheet.Cell(counter, 18).Style.Font.Bold = true;

                                    if (budgetDeltaPeriodo > 0)
                                    {
                                        worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(0, 176, 80);
                                    }
                                    else if (budgetDeltaPeriodo < 0)
                                    {
                                        worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(255, 0, 0);
                                    }
                                    else
                                    {
                                        worksheet.Cell(counter, 17).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
                                    }

                                    if (budgetDeltaAnno > 0)
                                    {
                                        worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(0, 176, 80);
                                    }
                                    else if (budgetDeltaAnno < 0)
                                    {
                                        worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(255, 0, 0);
                                    }
                                    else
                                    {
                                        worksheet.Cell(counter, 18).Style.Font.FontColor = XLColor.FromArgb(0, 0, 0);
                                    }

                                    for (int nCell = 1; nCell <= 19; nCell++)
                                    {
                                        worksheet.Cell(counter, nCell).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                        worksheet.Cell(counter, nCell).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                        worksheet.Cell(counter, nCell).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                        worksheet.Cell(counter, nCell).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                        worksheet.Cell(counter, nCell).Style.Alignment.WrapText = true;
                                    }
                                    worksheet.Range(counter, 1, counter, 16).Merge();
                                    worksheet.Row(counter).Height = 30;

                                    #endregion

                                    if (items.Any(x => x.ProvvRichiesto.ToUpper().Equals("NESSUNO")))
                                    {
                                        counter += 2;
                                        worksheet.Cell(counter, 2).SetValue<string>("RICHIESTE ESCLUSE:");
                                        var p = items.Where(x => x.ProvvRichiesto.ToUpper().Equals("NESSUNO")).GroupBy(x => x.ProvvOriginale);
                                        var ordered = p.Where(w => w.Key.ToUpper().Equals("PROMOZIONE")).ToList();
                                        ordered.AddRange(p.Where(w => w.Key.ToUpper().Equals("AUMENTO DI MERITO")).ToList());
                                        ordered.AddRange(p.Where(w => w.Key.ToUpper().Equals("GRATIFICA")).ToList());
                                        ordered.AddRange(p.Where(w => w.Key.ToUpper().Equals("PROMOZIONE SENZA ASSORBIMENTO")).ToList());

                                        foreach (var itemsGroup in ordered)
                                        {
                                            worksheet.Cell(counter + 1, 1).SetValue<string>(itemsGroup.Key);
                                            worksheet.Cell(counter + 1, 1).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                            worksheet.Cell(counter + 1, 1).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                            worksheet.Cell(counter + 1, 1).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                            worksheet.Cell(counter + 1, 1).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                                            decimal regimeParziale = 0;
                                            decimal periodoParziale = 0;

                                            foreach (var i in itemsGroup)
                                            {
                                                counter++;

                                                worksheet.Cell(counter, 2).SetValue<string>(i.Struttura);
                                                worksheet.Cell(counter, 3).SetValue<string>(i.AggregatoSede);
                                                worksheet.Cell(counter, 4).SetValue<string>(i.Nominativo);
                                                worksheet.Cell(counter, 5).SetValue<string>(i.Matricola);
                                                worksheet.Cell(counter, 6).SetValue<string>((!i.DataNascita.HasValue) ? "" : i.DataNascita.Value.ToString("dd/MM/yyyy"));
                                                worksheet.Cell(counter, 7).SetValue<string>((!i.DataAssunzione.HasValue) ? "" : i.DataAssunzione.Value.ToString("dd/MM/yyyy"));
                                                worksheet.Cell(counter, 8).SetValue<string>((!i.AnzianitaLivello.HasValue) ? "" : i.AnzianitaLivello.Value.ToString("dd/MM/yyyy"));
                                                worksheet.Cell(counter, 9).SetValue<string>(i.Profilo);
                                                worksheet.Cell(counter, 10).SetValue<string>(i.Livello);
                                                worksheet.Cell(counter, 11).SetValue<string>(i.UltimiProvv);
                                                worksheet.Cell(counter, 12).SetValue<string>(i.RAL);
                                                worksheet.Cell(counter, 13).SetValue<string>(i.RetribuzioneVariabile);
                                                worksheet.Cell(counter, 14).SetValue<string>(i.Reperibilita);
                                                worksheet.Cell(counter, 15).SetValue<string>(i.LivelloRichiesto);
                                                worksheet.Cell(counter, 16).SetValue<string>(i.DecorrenzaRichiesta);
                                                worksheet.Cell(counter, 17).SetValue<string>(i.CostoPeriodo);
                                                worksheet.Cell(counter, 18).SetValue<string>(i.CostoRegime);
                                                worksheet.Cell(counter, 19).SetValue<string>(i.Note.Trim());

                                                for (int nCell = 1; nCell <= 19; nCell++)
                                                {
                                                    worksheet.Cell(counter, nCell).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                                                    worksheet.Cell(counter, nCell).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                                                    worksheet.Cell(counter, nCell).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                                                    worksheet.Cell(counter, nCell).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                                                    worksheet.Cell(counter, nCell).Style.Alignment.WrapText = true;
                                                    if ((nCell >= 12 && nCell <= 14) ||
                                                         (nCell >= 17 && nCell <= 18))
                                                    {
                                                        worksheet.Cell(counter, nCell).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                                                    }
                                                    else
                                                    {
                                                        worksheet.Cell(counter, nCell).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                                    }

                                                    worksheet.Cell(counter, nCell).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                                                    worksheet.Cell(counter, nCell).Style.Font.FontSize = 12;
                                                }

                                                worksheet.Row(counter).Height = 48;
                                                worksheet.Row(counter).Style.Alignment.SetWrapText(true);

                                                string tempRegimeParziale = String.IsNullOrEmpty(i.CostoRegime) ? "0" : i.CostoRegime;
                                                string tempPeriodoParziale = String.IsNullOrEmpty(i.CostoPeriodo) ? "0" : i.CostoPeriodo;

                                                tempRegimeParziale = tempRegimeParziale.Replace("€", "");
                                                tempRegimeParziale = tempRegimeParziale.Trim();

                                                tempPeriodoParziale = tempPeriodoParziale.Replace("€", "");
                                                tempPeriodoParziale = tempPeriodoParziale.Trim();

                                                decimal reg = Convert.ToDecimal(tempRegimeParziale, System.Globalization.CultureInfo.CurrentCulture);
                                                decimal per = Convert.ToDecimal(tempPeriodoParziale, System.Globalization.CultureInfo.CurrentCulture);

                                                regimeParziale += reg;
                                                periodoParziale += per;
                                            }

                                        }
                                    }

                                    for (int nCell = 1; nCell <= 18; nCell++)
                                    {
                                        var ncol = worksheet.Column(nCell);
                                        ncol.Width = 24;
                                    }
                                    var lastCol = worksheet.Column(19);
                                    lastCol.Width = 50;
                                }
                            }
                        }
                    }

                    #endregion
                }
                catch (Exception ex)
                {

                }
            }

            if (workbook.Worksheets == null || !workbook.Worksheets.Any())
            {
                IXLWorksheet worksheet;
                workbook.Worksheets.TryGetWorksheet(String.Format("{0}", campagna.NomeCampagna).Truncate(25), out worksheet);

                if (worksheet == null)
                {
                    worksheet = workbook.Worksheets.Add(String.Format("{0}", campagna.NomeCampagna).Truncate(25));
                    worksheet.PageSetup.PaperSize = XLPaperSize.A3Paper;
                    worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                    worksheet.PageSetup.PagesWide = 1;
                }
                //throw new Exception( "Nessun dipendente trovato per le direzioni coinvolte" );
            }

            workbook.SaveAs(ms);
            ms.Position = 0;
            string nomeFile = "";

            if (idDirezione.HasValue)
            {
                nomeFile = String.Format("Report_Campagna_{0}_Direzione_{1}", campagna.Id, idDirezione.Value);
            }
            else
            {
                nomeFile = String.Format("Report_Campagna_{0}_Area_{1}", campagna.Id, idArea);
            }


            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = nomeFile + ".xlsx" };
        }

        /// <summary>
        /// Esporta riepilogo
        /// </summary>
        /// <param name="idCamp"></param>
        /// <param name="idArea"></param>
        /// <returns></returns>
        public ActionResult EsportaRiepilogoArea(int idCamp, int idArea, int? anno = null)
        {

            XLWorkbook workbook = new XLWorkbook();
            MemoryStream ms = new MemoryStream();
            var cultureInfo = CultureInfo.GetCultureInfo("it-IT");

            BudgetVM model = new BudgetVM();

            decimal _percentualePD = 0;
            decimal _percentualeMD = 0;
            decimal _percentualeGD = 0;

            int percentualePD = 0;
            int percentualeMD = 0;
            int percentualeGD = 0;

            try
            {
                List<InfoCampagna> campagne = this.BudgetDataController.GetCampagne(true);
                campagne.RemoveAll(w => w.Id != idCamp);

                List<int> anniDecorrenze = new List<int>();
                List<DateTime> dateDecorrenze = new List<DateTime>();

                campagne.ForEach(w =>
               {
                   dateDecorrenze.AddRange(w.Decorrenze.Distinct().ToList());
               });

                anniDecorrenze = dateDecorrenze.Select(w => w.Year).Distinct().ToList();

                model = this.GetBudgetVM(idCamp, idArea, false, anno);
                var area = this.BudgetDataController.GetArea(idArea);

                // nome del tab excel
                var worksheet = workbook.Worksheets.Add(area.NOME.Truncate(25));
                worksheet.PageSetup.PaperSize = XLPaperSize.A3Paper;
                worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
                worksheet.PageSetup.PagesWide = 1;

                int row = 1;

                #region Intestazione
                worksheet.Cell(row, 1).Value = String.Format("{0} - {1}", area.NOME, model.InfoCampagna.NomeCampagna);
                worksheet.Cell(row, 1).Style.Font.FontSize = 15;
                worksheet.Cell(row, 1).Style.Border.BottomBorder = XLBorderStyleValues.None;
                worksheet.Cell(row, 1).Style.Border.TopBorder = XLBorderStyleValues.None;
                worksheet.Cell(row, 1).Style.Border.LeftBorder = XLBorderStyleValues.None;
                worksheet.Cell(row, 1).Style.Border.RightBorder = XLBorderStyleValues.None;
                worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Row(1).Height = 60;
                worksheet.Row(1).Style.Fill.BackgroundColor = XLColor.NoColor;
                worksheet.Range(row, 1, row, 12).Merge();
                #endregion

                #region Intestazioni colonne1
                row++;
                worksheet.Cell(row, 1).Value = "Direzione";
                worksheet.Cell(row, 2).Value = "Organico contabile";
                worksheet.Cell(row, 3).Value = "Organico ai fini della ripartizione del budget";
                worksheet.Cell(row, 4).Value = "Budget";
                worksheet.Cell(row, 5).Value = "";
                worksheet.Cell(row, 6).Value = "Costo";
                worksheet.Cell(row, 7).Value = "";
                worksheet.Cell(row, 8).Value = "Provvedimenti";
                worksheet.Cell(row, 9).Value = "";
                worksheet.Cell(row, 10).Value = "";
                worksheet.Cell(row, 11).Value = "";
                worksheet.Cell(row, 12).Value = "";
                worksheet.Cell(row, 13).Value = "";
                worksheet.Cell(row, 14).Value = "Tot.";
                worksheet.Cell(row, 15).Value = "% su organico";
                worksheet.Cell(row, 16).Value = "Delta";
                worksheet.Cell(row, 17).Value = "";

                worksheet.Row(row).Height = 27;
                worksheet.Cell(row, 2).Style.Alignment.WrapText = true;
                worksheet.Cell(row, 3).Style.Alignment.WrapText = true;

                worksheet.Range(row, 4, row, 5).Merge();
                worksheet.Range(row, 6, row, 7).Merge();
                worksheet.Range(row, 8, row, 13).Merge();
                worksheet.Range(row, 16, row, 17).Merge();

                for (int i = 1; i <= 17; i++)
                {
                    worksheet.Cell(row, i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(row, i).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, i).Style.Font.FontSize = 15;
                    worksheet.Cell(row, i).Style.Font.Bold = true;
                    worksheet.Cell(row, i).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, i).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 32, 96);
                }

                worksheet.Cell(row, 4).Style.Fill.BackgroundColor = XLColor.FromArgb(132, 151, 176);
                worksheet.Cell(row, 5).Style.Fill.BackgroundColor = XLColor.FromArgb(132, 151, 176);
                worksheet.Cell(row, 6).Style.Fill.BackgroundColor = XLColor.FromArgb(132, 151, 176);
                worksheet.Cell(row, 7).Style.Fill.BackgroundColor = XLColor.FromArgb(132, 151, 176);
                worksheet.Cell(row, 8).Style.Fill.BackgroundColor = XLColor.FromArgb(132, 151, 176);
                worksheet.Cell(row, 9).Style.Fill.BackgroundColor = XLColor.FromArgb(132, 151, 176);
                worksheet.Cell(row, 10).Style.Fill.BackgroundColor = XLColor.FromArgb(132, 151, 176);
                worksheet.Cell(row, 11).Style.Fill.BackgroundColor = XLColor.FromArgb(132, 151, 176);
                worksheet.Cell(row, 12).Style.Fill.BackgroundColor = XLColor.FromArgb(132, 151, 176);
                worksheet.Cell(row, 13).Style.Fill.BackgroundColor = XLColor.FromArgb(132, 151, 176);
                worksheet.Cell(row, 16).Style.Fill.BackgroundColor = XLColor.FromArgb(132, 151, 176);
                worksheet.Cell(row, 17).Style.Fill.BackgroundColor = XLColor.FromArgb(132, 151, 176);


                #endregion

                #region Intestazioni colonne2
                row++;
                worksheet.Cell(row, 1).Value = "";
                worksheet.Cell(row, 2).Value = "";
                worksheet.Cell(row, 3).Value = "";
                worksheet.Cell(row, 4).Value = "Di periodo";
                worksheet.Cell(row, 5).Value = "Annuo";
                worksheet.Cell(row, 6).Value = "Di periodo";
                worksheet.Cell(row, 7).Value = "Annuo";
                worksheet.Cell(row, 8).Value = "PD";
                worksheet.Cell(row, 9).Value = "%";
                worksheet.Cell(row, 10).Value = "MD";
                worksheet.Cell(row, 11).Value = "%";
                worksheet.Cell(row, 12).Value = "GD";
                worksheet.Cell(row, 13).Value = "%";
                worksheet.Cell(row, 14).Value = "";
                worksheet.Cell(row, 15).Value = "";
                worksheet.Cell(row, 16).Value = "Di periodo";
                worksheet.Cell(row, 17).Value = "Annuo";

                for (int i = 1; i <= 17; i++)
                {
                    worksheet.Cell(row, i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(row, i).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, i).Style.Font.FontSize = 15;
                    worksheet.Cell(row, i).Style.Font.Bold = true;
                    worksheet.Cell(row, i).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, i).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 32, 96);
                }

                worksheet.Row(row).Height = 25;

                worksheet.Range(row - 1, 1, row, 1).Merge();
                worksheet.Range(row - 1, 2, row + 1, 2).Merge();
                worksheet.Range(row - 1, 3, row + 1, 3).Merge();
                worksheet.Range(row - 1, 14, row, 14).Merge();
                worksheet.Range(row - 1, 15, row + 1, 15).Merge();
                worksheet.Range(row, 16, row + 1, 16).Merge();
                worksheet.Range(row, 17, row + 1, 17).Merge();

                #endregion

                #region Intestazioni colonne3
                row++;

                worksheet.Cell(row, 1).Value = "";
                worksheet.Cell(row, 2).Value = "";
                worksheet.Cell(row, 3).Value = "";
                worksheet.Cell(row, 4).Value = "";
                worksheet.Cell(row, 5).Value = "";
                worksheet.Cell(row, 6).Value = "";
                worksheet.Cell(row, 7).Value = "";
                worksheet.Cell(row, 8).Value = "";
                worksheet.Cell(row, 9).Value = "";
                worksheet.Cell(row, 10).Value = "";
                worksheet.Cell(row, 11).Value = "";
                worksheet.Cell(row, 12).Value = "";
                worksheet.Cell(row, 13).Value = "";
                worksheet.Cell(row, 14).Value = "";
                worksheet.Cell(row, 15).Value = "";
                worksheet.Cell(row, 16).Value = "";
                worksheet.Cell(row, 17).Value = "";

                for (int i = 1; i <= 17; i++)
                {
                    worksheet.Cell(row, i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, i).Style.Font.FontSize = 15;
                    worksheet.Cell(row, i).Style.Font.Bold = true;
                    worksheet.Cell(row, i).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, i).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, i).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 32, 96);
                }

                worksheet.Row(row).Height = 25;
                //worksheet.Row(row).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 32, 96);
                #endregion

                if (model.ReportDirezioni != null && model.ReportDirezioni.Any())
                {
                    var listaOrdinata = model.ReportDirezioni.OrderBy(w => w.Ordine.GetValueOrDefault());

                    foreach (var r in listaOrdinata)
                    {
                        row++;
                        worksheet.Cell(row, 1).Value = CommonHelper.ToTitleCase(r.Direzione);
                        worksheet.Cell(row, 2).Value = r.OrganicoContabile;
                        worksheet.Cell(row, 3).Value = r.OrganicoRipartizione;

                        if (anno.HasValue && anno.GetValueOrDefault() == anniDecorrenze[anniDecorrenze.Count() - 1] && anniDecorrenze.Count() > 1)
                        {
                            worksheet.Cell(row, 4).Value = String.Format(cultureInfo, "€ {0:N2} ", 0);
                            worksheet.Cell(row, 5).Value = String.Format(cultureInfo, "€ {0:N2} ", 0);
                            worksheet.Cell(row, 6).Value = String.Format(cultureInfo, "€ {0:N2} ", 0);
                            worksheet.Cell(row, 7).Value = String.Format(cultureInfo, "€ {0:N2} ", r.CostoAnnoCorrente);
                        }
                        else
                        {
                            worksheet.Cell(row, 4).Value = String.Format(cultureInfo, "€ {0:N2} ", r.BudgetPeriodoCorrente);
                            worksheet.Cell(row, 5).Value = String.Format(cultureInfo, "€ {0:N2} ", r.BudgetAnnoCorrente);
                            worksheet.Cell(row, 6).Value = String.Format(cultureInfo, "€ {0:N2} ", r.CostoPeriodoAnnoCorrente);
                            worksheet.Cell(row, 7).Value = String.Format(cultureInfo, "€ {0:N2} ", r.CostoAnnoCorrente);
                        }
                        worksheet.Cell(row, 8).Value = r.Promozioni;

                        _percentualePD = 0;
                        _percentualeMD = 0;
                        _percentualeGD = 0;
                        percentualePD = 0;
                        percentualeMD = 0;
                        percentualeGD = 0;

                        if (r.OrganicoRipartizione > 0)
                        {
                            _percentualePD = (decimal)((decimal)r.Promozioni / (decimal)r.OrganicoRipartizione);
                            _percentualePD *= 100;

                            _percentualeMD = (decimal)((decimal)r.Aumenti / (decimal)r.OrganicoRipartizione);
                            _percentualeMD *= 100;

                            _percentualeGD = (decimal)((decimal)r.Gratifiche / (decimal)r.OrganicoRipartizione);
                            _percentualeGD *= 100;

                            percentualePD = (int)Math.Round(_percentualePD);
                            percentualeMD = (int)Math.Round(_percentualeMD);
                            percentualeGD = (int)Math.Round(_percentualeGD);
                        }

                        worksheet.Cell(row, 9).Value = String.Format("{0}%", percentualePD);
                        worksheet.Cell(row, 10).Value = r.Aumenti;
                        worksheet.Cell(row, 11).Value = String.Format("{0}%", percentualeMD);
                        worksheet.Cell(row, 12).Value = r.Gratifiche;
                        worksheet.Cell(row, 13).Value = String.Format("{0}%", percentualeGD);
                        worksheet.Cell(row, 14).Value = r.TotProvv;
                        worksheet.Cell(row, 15).Value = String.Format("{0}%", r.PercentualeSuOrganico);
                        if (anno.HasValue && anno.GetValueOrDefault() == anniDecorrenze[anniDecorrenze.Count() - 1] && anniDecorrenze.Count() > 1)
                        {
                            worksheet.Cell(row, 16).Value = String.Format(cultureInfo, "€ {0:N2}", 0);
                        }
                        else
                        {
                            worksheet.Cell(row, 16).Value = String.Format(cultureInfo, "€ {0:N2}", r.DeltaSuCostoPeriodo);
                        }

                        worksheet.Cell(row, 17).Value = String.Format(cultureInfo, "€ {0:N2}", r.DeltaSuCostoAnnoCorrente);

                        if (r.DeltaSuCostoPeriodo < 0)
                        {
                            worksheet.Cell(row, 16).Style.Font.FontColor = XLColor.Red;
                        }
                        else if (r.DeltaSuCostoPeriodo > 0)
                        {
                            worksheet.Cell(row, 16).Style.Font.FontColor = XLColor.Green;
                        }

                        if (r.DeltaSuCostoAnnoCorrente < 0)
                        {
                            worksheet.Cell(row, 17).Style.Font.FontColor = XLColor.Red;
                        }
                        else if (r.DeltaSuCostoAnnoCorrente > 0)
                        {
                            worksheet.Cell(row, 17).Style.Font.FontColor = XLColor.Green;
                        }

                        for (int i = 1; i <= 17; i++)
                        {
                            worksheet.Cell(row, i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                            worksheet.Cell(row, i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                            worksheet.Cell(row, i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                            worksheet.Cell(row, i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                            if (i == 1)
                            {
                                worksheet.Cell(row, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            }
                            else
                            {
                                worksheet.Cell(row, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            }
                        }
                        worksheet.Row(row).Height = 27;
                    }

                    row++;

                    int orgRipartizione = listaOrdinata.Sum(w => w.OrganicoRipartizione);

                    _percentualePD = 0;
                    _percentualeMD = 0;
                    _percentualeGD = 0;
                    percentualePD = 0;
                    percentualeMD = 0;
                    percentualeGD = 0;

                    if (orgRipartizione > 0)
                    {
                        _percentualePD = (decimal)((decimal)listaOrdinata.Sum(w => w.Promozioni) / (decimal)orgRipartizione);
                        _percentualePD *= 100;

                        _percentualeMD = (decimal)((decimal)listaOrdinata.Sum(w => w.Aumenti) / (decimal)orgRipartizione);
                        _percentualeMD *= 100;

                        _percentualeGD = (decimal)((decimal)listaOrdinata.Sum(w => w.Gratifiche) / (decimal)orgRipartizione);
                        _percentualeGD *= 100;

                        percentualePD = (int)Math.Round(_percentualePD);
                        percentualeMD = (int)Math.Round(_percentualeMD);
                        percentualeGD = (int)Math.Round(_percentualeGD);
                    }

                    int sumOrganicoContabile = listaOrdinata.Sum(x => x.OrganicoContabile);
                    int sumTotProvv = listaOrdinata.Sum(w => w.TotProvv);

                    worksheet.Cell(row, 1).Value = "TOTALE";
                    worksheet.Cell(row, 2).Value = sumOrganicoContabile;
                    worksheet.Cell(row, 3).Value = listaOrdinata.Sum(x => x.OrganicoRipartizione);

                    if (anno.HasValue && anno.GetValueOrDefault() == anniDecorrenze[anniDecorrenze.Count() - 1] && anniDecorrenze.Count() > 1)
                    {
                        worksheet.Cell(row, 4).Value = String.Format(cultureInfo, "€ {0:N2} ", 0);
                        worksheet.Cell(row, 5).Value = String.Format(cultureInfo, "€ {0:N2} ", 0);
                        worksheet.Cell(row, 6).Value = String.Format(cultureInfo, "€ {0:N2} ", 0);
                        worksheet.Cell(row, 7).Value = String.Format(cultureInfo, "€ {0:N2} ", listaOrdinata.Sum(w => w.CostoAnnoCorrente));
                    }
                    else
                    {
                        worksheet.Cell(row, 4).Value = String.Format(cultureInfo, "€ {0:N2} ", listaOrdinata.Sum(w => w.BudgetPeriodoCorrente));
                        worksheet.Cell(row, 5).Value = String.Format(cultureInfo, "€ {0:N2} ", listaOrdinata.Sum(w => w.BudgetAnnoCorrente));
                        worksheet.Cell(row, 6).Value = String.Format(cultureInfo, "€ {0:N2} ", listaOrdinata.Sum(w => w.CostoPeriodoAnnoCorrente));
                        worksheet.Cell(row, 7).Value = String.Format(cultureInfo, "€ {0:N2} ", listaOrdinata.Sum(w => w.CostoAnnoCorrente));
                    }

                    worksheet.Cell(row, 8).Value = listaOrdinata.Sum(w => w.Promozioni);
                    worksheet.Cell(row, 9).Value = String.Format("{0}%", percentualePD);
                    worksheet.Cell(row, 10).Value = listaOrdinata.Sum(w => w.Aumenti);
                    worksheet.Cell(row, 11).Value = String.Format("{0}%", percentualeMD);
                    worksheet.Cell(row, 12).Value = listaOrdinata.Sum(w => w.Gratifiche);
                    worksheet.Cell(row, 13).Value = String.Format("{0}%", percentualeGD);

                    worksheet.Cell(row, 14).Value = sumTotProvv;

                    int percentualeSuOrganico = 0;
                    if (sumTotProvv > 0 && sumOrganicoContabile > 0)
                    {
                        decimal _pOrg = (decimal)((decimal)sumTotProvv / (decimal)sumOrganicoContabile);
                        _pOrg *= 100;

                        percentualeSuOrganico = (int)Math.Round(_pOrg);
                    }

                    worksheet.Cell(row, 15).Value = String.Format("{0}%", percentualeSuOrganico);
                    if (anno.HasValue && anno.GetValueOrDefault() == anniDecorrenze[anniDecorrenze.Count() - 1] && anniDecorrenze.Count() > 1)
                    {
                        worksheet.Cell(row, 16).Value = String.Format(cultureInfo, "€ {0:N2}", 0);
                    }
                    else
                    {
                        worksheet.Cell(row, 16).Value = String.Format(cultureInfo, "€ {0:N2}", listaOrdinata.Sum(w => w.DeltaSuCostoPeriodo));
                    }

                    worksheet.Cell(row, 17).Value = String.Format(cultureInfo, "€ {0:N2}", listaOrdinata.Sum(w => w.DeltaSuCostoAnnoCorrente));

                    if (listaOrdinata.Sum(w => w.DeltaSuCostoPeriodo) < 0)
                    {
                        worksheet.Cell(row, 16).Style.Font.FontColor = XLColor.Red;
                    }
                    else if (listaOrdinata.Sum(w => w.DeltaSuCostoPeriodo) > 0)
                    {
                        worksheet.Cell(row, 16).Style.Font.FontColor = XLColor.Green;
                    }

                    if (listaOrdinata.Sum(w => w.DeltaSuCostoAnnoCorrente) < 0)
                    {
                        worksheet.Cell(row, 17).Style.Font.FontColor = XLColor.Red;
                    }
                    else if (listaOrdinata.Sum(w => w.DeltaSuCostoAnnoCorrente) > 0)
                    {
                        worksheet.Cell(row, 17).Style.Font.FontColor = XLColor.Green;
                    }

                    for (int i = 1; i <= 17; i++)
                    {
                        worksheet.Cell(row, i).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, i).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, i).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, i).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(row, i).Style.Font.FontSize = 15;
                        worksheet.Cell(row, i).Style.Font.Bold = true;
                        worksheet.Cell(row, i).Style.Font.FontColor = XLColor.White;
                        worksheet.Cell(row, i).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(row, i).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 32, 96);
                    }
                    worksheet.Row(row).Height = 27;
                }

                worksheet.Columns().AdjustToContents();

                #region dimensioni colonne               

                var col1 = worksheet.Column(1);
                if (col1.Width <= 40)
                {
                    col1.Width = 40;
                }
                else
                {
                    col1.Width += 5;
                }

                for (int ii = 2; ii <= 3; ii++)
                {
                    var col2_3 = worksheet.Column(ii);
                    if (col2_3.Width <= 25)
                    {
                        col2_3.Width = 25;
                    }
                    else
                    {
                        col2_3.Width += 5;
                    }
                    col2_3.Hide();
                }

                for (int j = 4; j <= 7; j++)
                {
                    var jCol = worksheet.Column(j);
                    if (jCol.Width <= 23)
                    {
                        jCol.Width = 23;
                    }
                    else
                    {
                        jCol.Width += 5;
                    }
                }

                for (int k = 8; k <= 14; k++)
                {
                    var kCol = worksheet.Column(k);
                    if (kCol.Width <= 10)
                    {
                        kCol.Width = 10;
                    }
                    else
                    {
                        kCol.Width += 5;
                    }
                }

                var col15 = worksheet.Column(15);
                if (col15.Width <= 18)
                {
                    col15.Width = 18;
                }
                else
                {
                    col15.Width += 5;
                }

                for (int k = 16; k <= 17; k++)
                {
                    var kCol = worksheet.Column(k);
                    if (kCol.Width <= 15)
                    {
                        kCol.Width = 15;
                    }
                    else
                    {
                        kCol.Width += 5;
                    }
                }

                worksheet.Column(13).Hide();
                worksheet.Column(11).Hide();
                worksheet.Column(9).Hide();
                worksheet.Column(3).Hide();
                worksheet.Column(2).Hide();
                #endregion

                workbook.SaveAs(ms);
                ms.Position = 0;
            }
            catch (Exception ex)
            {

            }

            string nomeFile = String.Format("RiepilogoArea_{0}_Campagna_{1}", idArea, idCamp);
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = nomeFile + ".xlsx" };
        }

        /// <summary>
        /// Esporta riepilogo
        /// </summary>
        /// <param name="idCamp"></param>
        /// <param name="idGruppo"></param>
        /// <returns></returns>
        public ActionResult EsportaRiepilogoAreaByGruppo(int idCamp, int idGruppo)
        {
            var campagna = this.BudgetDataController.GetCampagna(CommonHelper.GetCurrentUserMatricola(), idCamp);
            int idArea = 0;

            if (campagna != null && campagna.DettaglioCampagna != null && campagna.DettaglioCampagna.Any())
            {
                var gruppo = campagna.DettaglioCampagna.OrderBy(w => w.NomeArea).Skip(idGruppo - 1).Take(1).FirstOrDefault();

                if (gruppo != null)
                {
                    idArea = gruppo.IdArea;
                }
            }

            return EsportaRiepilogoArea(idCamp, idArea);
        }

        public ActionResult ReportPromozioniCampagna(int idCamp, string costo = "")
        {
            XLWorkbook workbook = new XLWorkbook();
            MemoryStream ms = new MemoryStream();
            var cultureInfo = CultureInfo.GetCultureInfo("it-IT");

            bool addAnnuo = !String.IsNullOrWhiteSpace(costo) && costo.Contains("annuo");
            bool addPeriodo = !String.IsNullOrWhiteSpace(costo) && costo.Contains("periodo");

            // nome del tab excel
            var worksheet = workbook.Worksheets.Add("Riepilogo");
            worksheet.PageSetup.PaperSize = XLPaperSize.A3Paper;
            worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            worksheet.PageSetup.PagesWide = 1;

            int defaultCell = 5; // imposta il numero minimo di colonne a 5

            List<PRV_DIREZIONE> direzioni = new List<PRV_DIREZIONE>();
            RiepilogoCampagna report = new RiepilogoCampagna();

            report.TipologiaProvvedimento = "Promozione";
            report.Items = new List<RiepilogoCampagnaItem>();

            try
            {
                // calcolo delle colonne che dovrà avere il foglio excel
                // le colonne di base sono Direzione, Conteggio, Note
                // le colonne variabili sono quelle riferite alle decorrenze (di solito 2)
                int countDecorrenze = 0;

                List<int> campagneContenute = this.BudgetDataController.GetCampagneContenute(idCamp);

                List<XR_PRV_CAMPAGNA_DECORRENZA> decorrenzeOrdinate = new List<XR_PRV_CAMPAGNA_DECORRENZA>();
                List<XR_PRV_CAMPAGNA_DECORRENZA> decorrenze = new List<XR_PRV_CAMPAGNA_DECORRENZA>();
                if (campagneContenute.Count() == 0)
                    decorrenze = this.BudgetDataController.GetDecorrenzeCampagna(idCamp);
                else
                    decorrenze = this.BudgetDataController.GetDecorrenzeCampagna(campagneContenute);

                if (decorrenze != null && decorrenze.Any())
                {
                    decorrenzeOrdinate = decorrenze.OrderBy(w => w.DT_DECORRENZA).ToList();
                    countDecorrenze = decorrenze.Count;
                }

                defaultCell = 4 + countDecorrenze;

                int row = 1;

                #region Intestazione
                worksheet.Cell(row, 1).Value = "Riepilogo promozioni";
                worksheet.Cell(row, 1).Style.Font.FontSize = 15;
                worksheet.Cell(row, 1).Style.Border.BottomBorder = XLBorderStyleValues.None;
                worksheet.Cell(row, 1).Style.Border.TopBorder = XLBorderStyleValues.None;
                worksheet.Cell(row, 1).Style.Border.LeftBorder = XLBorderStyleValues.None;
                worksheet.Cell(row, 1).Style.Border.RightBorder = XLBorderStyleValues.None;
                worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Row(1).Height = 60;
                worksheet.Row(1).Style.Fill.BackgroundColor = XLColor.NoColor;
                worksheet.Range(row, 1, row, defaultCell).Merge();
                #endregion

                #region Intestazioni colonne1
                row++;
                worksheet.Cell(row, 1).Value = "Direzione";
                worksheet.Cell(row, 2).Value = "Richieste";
                worksheet.Cell(row, 3).Value = "Concordato";
                worksheet.Cell(row, 4).Value = "Decorrenza";
                for (int i = 1; i < countDecorrenze; i++)
                {
                    worksheet.Cell(row, 4 + i).Value = "";
                }

                for (int j = 1; j <= defaultCell; j++)
                {
                    worksheet.Cell(row, j).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                    worksheet.Cell(row, j).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                    worksheet.Cell(row, j).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                    worksheet.Cell(row, j).Style.Border.RightBorder = XLBorderStyleValues.Thick;
                    worksheet.Cell(row, j).Style.Font.FontSize = 15;
                    worksheet.Cell(row, j).Style.Font.Bold = true;
                    worksheet.Cell(row, j).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, j).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(row, j).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, j).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 32, 96);
                    worksheet.Cell(row, j).Style.Alignment.WrapText = true;
                }

                worksheet.Cell(row, 4).Style.Fill.BackgroundColor = XLColor.FromArgb(132, 151, 176);

                worksheet.Cell(row, defaultCell).Value = "Note";

                worksheet.Range(row, 4, row, (defaultCell - 1)).Merge();

                worksheet.Row(row).Height = 27;

                worksheet.Range(row, 1, row + 1, 1).Merge();
                worksheet.Range(row, 2, row + 1, 2).Merge();
                worksheet.Range(row, 3, row + 1, 3).Merge();
                worksheet.Range(row, defaultCell, row + 1, defaultCell).Merge();

                int rifCol = defaultCell;
                if (addAnnuo)
                {
                    rifCol++;
                    worksheet.Cell(row, rifCol).Value = "Costo annuo";

                    worksheet.Range(row, rifCol, row + 1, rifCol).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                    worksheet.Range(row, rifCol, row + 1, rifCol).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                    worksheet.Range(row, rifCol, row + 1, rifCol).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                    worksheet.Range(row, rifCol, row + 1, rifCol).Style.Border.RightBorder = XLBorderStyleValues.Thick;
                    worksheet.Range(row, rifCol, row + 1, rifCol).Style.Font.FontSize = 15;
                    worksheet.Range(row, rifCol, row + 1, rifCol).Style.Font.Bold = true;
                    worksheet.Range(row, rifCol, row + 1, rifCol).Style.Font.FontColor = XLColor.White;
                    worksheet.Range(row, rifCol, row + 1, rifCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Range(row, rifCol, row + 1, rifCol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Range(row, rifCol, row + 1, rifCol).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 32, 96);
                    worksheet.Range(row, rifCol, row + 1, rifCol).Style.Alignment.WrapText = true;

                    worksheet.Range(row, rifCol, row + 1, rifCol).Merge();
                }
                if (addPeriodo)
                {
                    rifCol++;
                    worksheet.Cell(row, rifCol).Value = "Costo periodo";

                    worksheet.Range(row, rifCol, row + 1, rifCol).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                    worksheet.Range(row, rifCol, row + 1, rifCol).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                    worksheet.Range(row, rifCol, row + 1, rifCol).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                    worksheet.Range(row, rifCol, row + 1, rifCol).Style.Border.RightBorder = XLBorderStyleValues.Thick;
                    worksheet.Range(row, rifCol, row + 1, rifCol).Style.Font.FontSize = 15;
                    worksheet.Range(row, rifCol, row + 1, rifCol).Style.Font.Bold = true;
                    worksheet.Range(row, rifCol, row + 1, rifCol).Style.Font.FontColor = XLColor.White;
                    worksheet.Range(row, rifCol, row + 1, rifCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Range(row, rifCol, row + 1, rifCol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Range(row, rifCol, row + 1, rifCol).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 32, 96);
                    worksheet.Range(row, rifCol, row + 1, rifCol).Style.Alignment.WrapText = true;

                    worksheet.Range(row, rifCol, row + 1, rifCol).Merge();
                }

                #endregion

                #region Intestazioni colonne2
                row++;
                worksheet.Cell(row, 1).Value = "";
                worksheet.Cell(row, 2).Value = "";
                worksheet.Cell(row, 3).Value = "";


                for (int i = 0; i < countDecorrenze; i++)
                {
                    worksheet.Cell(row, 4 + i).Value = decorrenzeOrdinate.ElementAt(i).DT_DECORRENZA.Date.ToString("MMMM");
                }

                worksheet.Cell(row, defaultCell).Value = "";

                for (int j = 1; j <= defaultCell; j++)
                {
                    worksheet.Cell(row, j).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                    worksheet.Cell(row, j).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                    worksheet.Cell(row, j).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                    worksheet.Cell(row, j).Style.Border.RightBorder = XLBorderStyleValues.Thick;
                    worksheet.Cell(row, j).Style.Font.FontSize = 15;
                    worksheet.Cell(row, j).Style.Font.Bold = true;
                    worksheet.Cell(row, j).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, j).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(row, j).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, j).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 32, 96);
                    worksheet.Cell(row, j).Style.Alignment.WrapText = true;
                }

                worksheet.Row(row).Height = 25;
                #endregion

                var campagna = this.BudgetDataController.GetCampagna(CommonHelper.GetCurrentUserMatricola(), idCamp);
                if (campagna != null)
                {
                    if (campagna.DettaglioCampagna != null &&
                        campagna.DettaglioCampagna.Any())
                    {
                        foreach (var a in campagna.DettaglioCampagna)
                        {
                            var area = this.BudgetDataController.GetArea(a.IdArea);

                            if (campagneContenute.Count() == 0)
                            {
                                report.Items.AddRange(GetReportDirezioniPromozioni(area, idCamp, decorrenzeOrdinate));
                            }
                            else
                            {
                                List<RiepilogoCampagnaItem> tmp = new List<RiepilogoCampagnaItem>();
                                foreach (var item in campagneContenute)
                                {
                                    tmp.AddRange(GetReportDirezioniPromozioni(area, item, decorrenzeOrdinate));
                                }

                                foreach (var group in tmp.GroupBy(x => x.Direzione))
                                {
                                    RiepilogoCampagnaItem item = new RiepilogoCampagnaItem();
                                    item.Decorrenze = new List<int>();
                                    item.Decorrenze.AddRange(group.SelectMany(x => x.Decorrenze));
                                    item.Concordato = group.Sum(x => x.Concordato);
                                    item.Richiesto = group.Sum(x => x.Richiesto);
                                    item.Note = "";
                                    item.Direzione = group.Key;
                                    item.CostoAnnuo = group.Sum(x => x.CostoAnnuo);
                                    item.CostoPeriodo = group.Sum(x => x.CostoPeriodo);
                                    report.Items.Add(item);
                                }
                            }
                        }
                    }
                }

                if (report.Items != null &&
                    report.Items.Any())
                {
                    var items = report.Items.OrderBy(w => w.Direzione).ToList();
                    foreach (var r in items)
                    {
                        if (r.Richiesto == 0)
                        {
                            continue;
                        }
                        row++;
                        worksheet.Cell(row, 1).Value = CommonHelper.ToTitleCase(r.Direzione);
                        worksheet.Cell(row, 2).Value = r.Richiesto;
                        worksheet.Cell(row, 3).Value = r.Concordato;

                        for (int i = 0; i < countDecorrenze; i++)
                        {
                            worksheet.Cell(row, 4 + i).Value = r.Decorrenze.ElementAt(i);
                        }

                        worksheet.Cell(row, defaultCell).Value = r.Note;

                        for (int i = 1; i <= defaultCell; i++)
                        {
                            worksheet.Cell(row, i).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                            worksheet.Cell(row, i).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                            worksheet.Cell(row, i).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                            worksheet.Cell(row, i).Style.Border.RightBorder = XLBorderStyleValues.Thick;
                            worksheet.Cell(row, i).Style.Font.FontSize = 12;
                            worksheet.Cell(row, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            worksheet.Cell(row, i).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            worksheet.Cell(row, i).Style.Alignment.WrapText = true;
                            if (i == 1 || i == defaultCell)
                            {
                                worksheet.Cell(row, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                            }
                            else
                            {
                                worksheet.Cell(row, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                            }
                        }

                        rifCol = defaultCell;
                        if (addAnnuo)
                        {
                            worksheet.Cell(row, ++rifCol).SetValue(String.Format("€ {0:N2}", r.CostoAnnuo));
                            worksheet.Cell(row, rifCol).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                            worksheet.Cell(row, rifCol).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                            worksheet.Cell(row, rifCol).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                            worksheet.Cell(row, rifCol).Style.Border.RightBorder = XLBorderStyleValues.Thick;
                            worksheet.Cell(row, rifCol).Style.Font.FontSize = 12;
                            worksheet.Cell(row, rifCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            worksheet.Cell(row, rifCol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            worksheet.Cell(row, rifCol).Style.Alignment.WrapText = true;
                            worksheet.Cell(row, rifCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        }
                        if (addPeriodo)
                        {
                            worksheet.Cell(row, ++rifCol).SetValue(String.Format("€ {0:N2}", r.CostoPeriodo));
                            worksheet.Cell(row, rifCol).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                            worksheet.Cell(row, rifCol).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                            worksheet.Cell(row, rifCol).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                            worksheet.Cell(row, rifCol).Style.Border.RightBorder = XLBorderStyleValues.Thick;
                            worksheet.Cell(row, rifCol).Style.Font.FontSize = 12;
                            worksheet.Cell(row, rifCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                            worksheet.Cell(row, rifCol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                            worksheet.Cell(row, rifCol).Style.Alignment.WrapText = true;
                            worksheet.Cell(row, rifCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        }

                        worksheet.Row(row).Height = 35;

                    }

                    row++;
                    worksheet.Cell(row, 1).Value = "TOTALE";
                    worksheet.Cell(row, 2).Value = report.Items.Sum(w => w.Richiesto);
                    worksheet.Cell(row, 3).Value = report.Items.Sum(w => w.Concordato);

                    for (int i = 0; i < countDecorrenze; i++)
                    {
                        worksheet.Cell(row, 4 + i).Value = report.Items.Sum(w => w.Decorrenze.ElementAt(i));
                    }

                    worksheet.Cell(row, defaultCell).Value = "";

                    for (int i = 1; i <= defaultCell; i++)
                    {
                        worksheet.Cell(row, i).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                        worksheet.Cell(row, i).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                        worksheet.Cell(row, i).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                        worksheet.Cell(row, i).Style.Border.RightBorder = XLBorderStyleValues.Thick;
                        worksheet.Cell(row, i).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(row, i).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(row, i).Style.Alignment.WrapText = true;
                        worksheet.Cell(row, i).Style.Font.FontSize = 15;
                        worksheet.Cell(row, i).Style.Font.Bold = true;
                        worksheet.Cell(row, i).Style.Font.FontColor = XLColor.White;

                        worksheet.Cell(row, i).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 32, 96);
                    }
                    rifCol = defaultCell;
                    if (addAnnuo)
                    {
                        worksheet.Cell(row, ++rifCol).SetValue(String.Format("€ {0:N2}", report.Items.Sum(x => x.CostoAnnuo)));
                        worksheet.Cell(row, rifCol).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                        worksheet.Cell(row, rifCol).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                        worksheet.Cell(row, rifCol).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                        worksheet.Cell(row, rifCol).Style.Border.RightBorder = XLBorderStyleValues.Thick;
                        worksheet.Cell(row, rifCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell(row, rifCol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(row, rifCol).Style.Alignment.WrapText = true;
                        worksheet.Cell(row, rifCol).Style.Font.FontSize = 15;
                        worksheet.Cell(row, rifCol).Style.Font.Bold = true;
                        worksheet.Cell(row, rifCol).Style.Font.FontColor = XLColor.White;

                        worksheet.Cell(row, rifCol).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 32, 96);
                    }
                    if (addPeriodo)
                    {
                        worksheet.Cell(row, ++rifCol).SetValue(String.Format("€ {0:N2}", report.Items.Sum(x => x.CostoPeriodo)));
                        worksheet.Cell(row, rifCol).Style.Border.BottomBorder = XLBorderStyleValues.Thick;
                        worksheet.Cell(row, rifCol).Style.Border.TopBorder = XLBorderStyleValues.Thick;
                        worksheet.Cell(row, rifCol).Style.Border.LeftBorder = XLBorderStyleValues.Thick;
                        worksheet.Cell(row, rifCol).Style.Border.RightBorder = XLBorderStyleValues.Thick;
                        worksheet.Cell(row, rifCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell(row, rifCol).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(row, rifCol).Style.Alignment.WrapText = true;
                        worksheet.Cell(row, rifCol).Style.Font.FontSize = 15;
                        worksheet.Cell(row, rifCol).Style.Font.Bold = true;
                        worksheet.Cell(row, rifCol).Style.Font.FontColor = XLColor.White;

                        worksheet.Cell(row, rifCol).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 32, 96);
                    }
                    worksheet.Row(row).Height = 35;

                }

                #region dimensioni colonne
                var firstCol = worksheet.Column(1);
                firstCol.Width = 25;

                for (int i = 2; i <= defaultCell - 1; i++)
                {
                    var col1 = worksheet.Column(i);
                    col1.Width = 22;
                }

                var lastCol = worksheet.Column(defaultCell);
                lastCol.Width = 50;

                rifCol = defaultCell;
                if (addAnnuo)
                {
                    rifCol++;
                    worksheet.Column(rifCol).Width = 30;
                }
                if (addPeriodo)
                {
                    rifCol++;
                    worksheet.Column(rifCol).Width = 30;
                }

                #endregion

                workbook.SaveAs(ms);
                ms.Position = 0;
            }
            catch (Exception ex)
            {

            }

            string nomeFile = String.Format("ReportPromozioniCampagna_{0}", idCamp);
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = nomeFile + ".xlsx" };
        }

        private FileStreamResult ReportPoliticheRetributiveSingolaCampagna(int idCamp)
        {
            XLWorkbook workbook = new XLWorkbook();
            MemoryStream ms = new MemoryStream();
            var cultureInfo = CultureInfo.GetCultureInfo("it-IT");

            // nome del tab excel
            var worksheet = workbook.Worksheets.Add("ReportPoliticheRetrib");
            worksheet.PageSetup.PaperSize = XLPaperSize.A3Paper;
            worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            worksheet.PageSetup.PagesWide = 1;

            int defaultCell = 9; // imposta il numero minimo di colonne a 9 (n decorrenze ma per l'anno corrente)

            List<PRV_DIREZIONE> direzioni = new List<PRV_DIREZIONE>();
            ReportPoliticheRetributive report = new ReportPoliticheRetributive();

            try
            {
                int countDecorrenze = 0;
                // prende le campagne alle quali sono abilitato
                List<InfoCampagna> campagne = this.BudgetDataController.GetCampagne(true);

                if (idCamp == 0)
                {
                    campagne.RemoveAll(w => w.Id == 0);
                }
                else
                {
                    campagne.RemoveAll(w => w.Id != idCamp);
                }

                if (idCamp == 0)
                {
                    report.Intestazione = String.Format("POLITICHE RETRIBUTIVE {0}", DateTime.Now.Year);
                }
                else
                {
                    report.Intestazione = campagne.First().NomeCampagna;
                }

                List<int> anniDecorrenze = new List<int>();
                List<DateTime> dateDecorrenze = new List<DateTime>();

                campagne.ForEach(w =>
               {
                   dateDecorrenze.AddRange(w.Decorrenze.Distinct().ToList());
               });

                anniDecorrenze = dateDecorrenze.Select(w => w.Year).Distinct().ToList();

                countDecorrenze = anniDecorrenze.Count();

                defaultCell += (countDecorrenze - 1) * 4;
                int row = 1;

                #region Preparazione Oggetto

                report.RiepilogoPerAnno = new List<ReportPoliticheRetributiveYearItem>();

                for (int idx = 1; idx <= countDecorrenze; idx++)
                {
                    decimal _budgetA = 0;
                    decimal _budgetP = 0;
                    decimal _costoA = 0;
                    decimal _costoP = 0;

                    decimal budgetA = 0;
                    decimal budgetP = 0;
                    decimal costoA = 0;
                    decimal costoP = 0;

                    foreach (var campagna in campagne)
                    {
                        this.BudgetDataController.GetBudgetAnnuoPeriodoCampagna(campagna, out _budgetA, out _budgetP);
                        this.BudgetDataController.GetCostoAnnuoPeriodoCampagna(campagna, out _costoA, out _costoP, anniDecorrenze[0]);

                        budgetA += _budgetA;
                        budgetP += _budgetP;
                        costoA += _costoA;
                        costoP += _costoP;
                    }

                    if (idx > 1)
                    {
                        report.RiepilogoPerAnno.Add(new ReportPoliticheRetributiveYearItem()
                        {
                            Budget = budgetA,
                            Costo = costoA,
                            Delta = budgetA - costoA
                        });
                    }
                    else
                    {
                        report.RiepilogoPerAnno.Add(new ReportPoliticheRetributiveYearItem()
                        {
                            Budget = budgetP,
                            Costo = costoP,
                            Delta = budgetP - costoP
                        });
                    }
                }
                report.RiepilogoProvvedimenti = new ReportPoliticheRetributiveDetailsItem();
                report.RiepilogoRisorse = new ReportPoliticheRetributiveRisorsePanelDetailsItem();
                report.DettaglioAltroPersonale = new ReportPoliticheRetributiveTipologiaItem();
                report.DettaglioFunzionariF1 = new ReportPoliticheRetributiveTipologiaItem();
                report.DettaglioPoliticheRetributive = new ReportPoliticheRetributiveTipologiaItem();

                var isProm = PoliticheRetributiveHelper.AnyOfProv(PoliticheRetributiveHelper.GetProvBySigla(null, PoliticheRetributiveHelper.SIGLA_PROMOZIONI));
                var isAumento = PoliticheRetributiveHelper.AnyOfProv(PoliticheRetributiveHelper.GetProvBySigla(null, PoliticheRetributiveHelper.SIGLA_AUMENTI));
                var isGratifica = PoliticheRetributiveHelper.AnyOfProv(PoliticheRetributiveHelper.GetProvBySigla(null, PoliticheRetributiveHelper.SIGLA_GRATIFICHE));

                foreach (var campagna in campagne)
                {
                    #region Totali provvedimenti
                    report.RiepilogoProvvedimenti.Passaggi += this.BudgetDataController.GetSumProvvedimenti(campagna.Id, isProm, true, anniDecorrenze[0]);

                    report.RiepilogoProvvedimenti.IncrementiRetributivi += this.BudgetDataController.GetSumProvvedimenti(campagna.Id, isAumento, true, anniDecorrenze[0]);

                    report.RiepilogoProvvedimenti.Gratifiche += this.BudgetDataController.GetSumProvvedimenti(campagna.Id, isGratifica, true, anniDecorrenze[0]);
                    #endregion

                    #region Totali provvedimenti anno prossimo
                    report.RiepilogoRisorse.Anticipazioni += this.BudgetDataController.GetCostoAnnoCampagna(campagna.Id, false, anniDecorrenze[anniDecorrenze.Count() - 1]);

                    report.RiepilogoRisorse.TotaleInterventi += this.BudgetDataController.GetSumProvvedimenti(campagna.Id, isProm, false, anniDecorrenze[anniDecorrenze.Count() - 1])
                                                                + this.BudgetDataController.GetSumProvvedimenti(campagna.Id, isAumento, false, anniDecorrenze[anniDecorrenze.Count() - 1])
                                                                + this.BudgetDataController.GetSumProvvedimenti(campagna.Id, isGratifica, false, anniDecorrenze[anniDecorrenze.Count() - 1]);
                    #endregion
                }

                List<PRV_Dipendente> dipendenti = new List<PRV_Dipendente>();

                if (idCamp == 0)
                {
                    dipendenti = this.BudgetDataController.GetDipendentiAllCampagne(DateTime.Now.Year, anniDecorrenze[anniDecorrenze.Count() - 1], true);
                }
                else
                {
                    dipendenti = this.BudgetDataController.GetDipendentiCampagna(idCamp, DateTime.Now.Year, anniDecorrenze[anniDecorrenze.Count() - 1], true);
                }

                foreach (var w in dipendenti)
                {
                    bool isF1 = (w.CategoriaPrevista == "Q11" || w.CategoriaPrevista == "Q13");
                    bool isF1Super = CezanneHelper.GetFSuperCat().Contains(w.CategoriaPrevista);

                    if (w.Area.NOME.ToUpper().Contains("AREA STAFF"))
                    {
                        if (isF1)
                        {
                            report.DettaglioFunzionariF1.Staff1 += (w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoLivello ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoLivello ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoMerito ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoMerito) ? w.CostoAnnuo : 0;

                            report.DettaglioFunzionariF1.Staff2 += (w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoLivello ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoLivello ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoMerito ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoMerito ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.Gratifica ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSGratifica) ? 1 : 0;
                        }
                        else
                        {
                            report.DettaglioAltroPersonale.Staff1 += (w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoLivello ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoLivello ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoMerito ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoMerito) ? w.CostoAnnuo : 0;

                            report.DettaglioAltroPersonale.Staff2 += (w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoLivello ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoLivello ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoMerito ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoMerito ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.Gratifica ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSGratifica) ? 1 : 0;
                        }
                    }
                    else if (w.Area.NOME.ToUpper().Contains("AREA PRODUZIONE"))
                    {
                        if (isF1)
                        {
                            report.DettaglioFunzionariF1.Prod1 += (w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoLivello ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoLivello ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoMerito ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoMerito) ? w.CostoAnnuo : 0;

                            report.DettaglioFunzionariF1.Prod2 += (w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoLivello ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoLivello ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoMerito ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoMerito ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.Gratifica ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSGratifica) ? 1 : 0;
                        }
                        else
                        {
                            report.DettaglioAltroPersonale.Prod1 += (w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoLivello ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoLivello ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoMerito ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoMerito) ? w.CostoAnnuo : 0;

                            report.DettaglioAltroPersonale.Prod2 += (w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoLivello ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoLivello ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoMerito ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoMerito ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.Gratifica ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSGratifica) ? 1 : 0;
                        }
                    }
                    else if (w.Area.NOME.ToUpper().Contains("AREA EDITORIALE"))
                    {
                        if (isF1)
                        {
                            report.DettaglioFunzionariF1.Edit1 += (w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoLivello ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoLivello ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoMerito ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoMerito) ? w.CostoAnnuo : 0;

                            report.DettaglioFunzionariF1.Edit2 += (w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoLivello ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoLivello ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoMerito ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoMerito ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.Gratifica ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSGratifica) ? 1 : 0;
                        }
                        else
                        {
                            report.DettaglioAltroPersonale.Edit1 += (w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoLivello ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoLivello ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoMerito ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoMerito) ? w.CostoAnnuo : 0;

                            report.DettaglioAltroPersonale.Edit2 += (w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoLivello ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoLivello ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoMerito ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoMerito ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.Gratifica ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSGratifica) ? 1 : 0;
                        }
                    }
                    else if (w.Area.NOME.ToUpper().Contains("AREA RISORSE CHIAVE"))
                    {
                        if (isF1Super)
                        {
                            report.DettaglioPoliticheRetributive.Staff1 += (w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoLivello ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoLivello ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoMerito ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoMerito) ? w.CostoAnnuo : 0;

                            report.DettaglioPoliticheRetributive.Staff2 += (w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoLivello ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoLivello ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoMerito ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoMerito ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.Gratifica ||
                                                                    w.IdProvvedimento == (int)ProvvedimentiEnum.CUSGratifica) ? 1 : 0;
                        }
                        else if (!String.IsNullOrEmpty(w.MacroArea) && w.MacroArea.Contains("AREA STAFF"))
                        {
                            if (isF1)
                            {
                                report.DettaglioFunzionariF1.Staff1 += (w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoLivello ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoLivello ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoMerito ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoMerito) ? w.CostoAnnuo : 0;

                                report.DettaglioFunzionariF1.Staff2 += (w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoLivello ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoLivello ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoMerito ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoMerito ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.Gratifica ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSGratifica) ? 1 : 0;
                            }
                            else
                            {
                                report.DettaglioAltroPersonale.Staff1 += (w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoLivello ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoLivello ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoMerito ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoMerito) ? w.CostoAnnuo : 0;

                                report.DettaglioAltroPersonale.Staff2 += (w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoLivello ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoLivello ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoMerito ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoMerito ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.Gratifica ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSGratifica) ? 1 : 0;
                            }
                        }
                        else if (!String.IsNullOrEmpty(w.MacroArea) && w.MacroArea.Contains("AREA PRODUZIONE"))
                        {
                            if (isF1)
                            {
                                report.DettaglioFunzionariF1.Prod1 += (w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoLivello ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoLivello ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoMerito ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoMerito) ? w.CostoAnnuo : 0;

                                report.DettaglioFunzionariF1.Prod2 += (w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoLivello ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoLivello ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoMerito ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoMerito ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.Gratifica ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSGratifica) ? 1 : 0;
                            }
                            else
                            {
                                report.DettaglioAltroPersonale.Prod1 += (w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoLivello ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoLivello ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoMerito ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoMerito) ? w.CostoAnnuo : 0;

                                report.DettaglioAltroPersonale.Prod2 += (w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoLivello ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoLivello ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoMerito ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoMerito ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.Gratifica ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSGratifica) ? 1 : 0;
                            }
                        }
                        else if (!String.IsNullOrEmpty(w.MacroArea) && w.MacroArea.Contains("AREA EDITORIALE"))
                        {
                            if (isF1)
                            {
                                report.DettaglioFunzionariF1.Edit1 += (w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoLivello ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoLivello ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoMerito ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoMerito) ? w.CostoAnnuo : 0;

                                report.DettaglioFunzionariF1.Edit2 += (w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoLivello ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoLivello ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoMerito ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoMerito ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.Gratifica ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSGratifica) ? 1 : 0;
                            }
                            else
                            {
                                report.DettaglioAltroPersonale.Edit1 += (w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoLivello ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoLivello ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoMerito ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoMerito) ? w.CostoAnnuo : 0;

                                report.DettaglioAltroPersonale.Edit2 += (w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoLivello ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoLivello ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoMerito ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoMerito ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.Gratifica ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSGratifica) ? 1 : 0;
                            }
                        }
                    }
                }

                #endregion

                #region Intestazione
                worksheet.Cell(row, 1).Value = report.Intestazione;
                worksheet.Cell(row, 1).Style.Font.FontSize = 15;
                worksheet.Cell(row, 1).Style.Border.BottomBorder = XLBorderStyleValues.None;
                worksheet.Cell(row, 1).Style.Border.TopBorder = XLBorderStyleValues.None;
                worksheet.Cell(row, 1).Style.Border.LeftBorder = XLBorderStyleValues.None;
                worksheet.Cell(row, 1).Style.Border.RightBorder = XLBorderStyleValues.None;
                worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, 1).Style.Alignment.WrapText = true;
                worksheet.Row(row).Height = 27;
                worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(155, 194, 230);
                worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.White;
                worksheet.Range(row, 1, row, defaultCell).Merge();
                #endregion

                // aggiunge riga vuota
                row++;
                worksheet.Cell(row, 1).Value = "";
                worksheet.Range(row, 1, row, defaultCell).Merge();
                worksheet.Row(row).Height = 20;

                // aggiunge l'intestazione per l'ultimo pannello, quello coi riepiloghi dei provvedimenti
                row++;
                worksheet.Cell(row, 1).Value = "";
                worksheet.Range(row, 1, row, defaultCell - 4).Merge();
                worksheet.Cell(row, defaultCell - 3).Value = "n. provvedimenti";
                worksheet.Cell(row, defaultCell - 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, defaultCell - 3).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, defaultCell - 3).Style.Font.Bold = true;
                worksheet.Range(row, defaultCell - 3, row, defaultCell).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Range(row, defaultCell - 3, row, defaultCell).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                worksheet.Range(row, defaultCell - 3, row, defaultCell).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Range(row, defaultCell - 3, row, defaultCell).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                worksheet.Range(row, defaultCell - 3, row, defaultCell).Merge();
                worksheet.Row(row).Height = 20;

                #region Intestazioni colonne1
                row++;
                int col = 1;

                worksheet.Cell(row, col).Value = "";

                for (int idxDec = 1; idxDec <= countDecorrenze; idxDec++)
                {
                    col++;
                    if (idxDec > 1)
                    {
                        worksheet.Cell(row, col).Value = "BUDGET ANNUO";
                    }
                    else
                    {
                        worksheet.Cell(row, col).Value = "BUDGET PERIODO";
                    }
                    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(31, 78, 120);
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                    worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    col++;
                    if (idxDec > 1)
                    {
                        worksheet.Cell(row, col).Value = "COSTO ANNUO (" + anniDecorrenze[0] + ")";
                    }
                    else
                    {
                        worksheet.Cell(row, col).Value = "COSTO PERIODO (" + anniDecorrenze[0] + ")";
                    }
                    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(31, 78, 120);
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                    worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    col++;
                    worksheet.Cell(row, col).Value = "DELTA";
                    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(31, 78, 120);
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                    worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    col++;
                    worksheet.Cell(row, col).Value = " ";
                }

                col++;
                worksheet.Cell(row, col).Value = "Passaggi";
                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(226, 239, 218);
                worksheet.Cell(row, col).Style.Font.FontColor = XLColor.Black;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                col++;
                worksheet.Cell(row, col).Value = "Incrementi retributivi";
                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(226, 239, 218);
                worksheet.Cell(row, col).Style.Font.FontColor = XLColor.Black;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                col++;
                worksheet.Cell(row, col).Value = "Gratifiche";
                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(226, 239, 218);
                worksheet.Cell(row, col).Style.Font.FontColor = XLColor.Black;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                col++;
                worksheet.Cell(row, col).Value = "Totale";
                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(226, 239, 218);
                worksheet.Cell(row, col).Style.Font.FontColor = XLColor.Black;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                worksheet.Row(row).Height = 45;
                #endregion

                #region Corpo
                col = 1;
                row++;

                worksheet.Cell(row, col).Value = "TOTALE";
                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(31, 78, 120);
                worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                for (int idxDec = 1; idxDec <= countDecorrenze; idxDec++)
                {
                    col++;
                    worksheet.Cell(row, col).Value = String.Format(cultureInfo, "€ {0:N2}", report.RiepilogoPerAnno[idxDec - 1].Budget);
                    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                    worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    col++;
                    worksheet.Cell(row, col).Value = String.Format(cultureInfo, "€ {0:N2}", report.RiepilogoPerAnno[idxDec - 1].Costo);
                    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                    worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    col++;
                    worksheet.Cell(row, col).Value = String.Format(cultureInfo, "€ {0:N2}", report.RiepilogoPerAnno[idxDec - 1].Delta);
                    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                    worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                    if (report.RiepilogoPerAnno[idxDec - 1].Delta > 0)
                    {
                        worksheet.Cell(row, col).Style.Font.FontColor = XLColor.FromArgb(0, 176, 80);
                    }
                    else if (report.RiepilogoPerAnno[idxDec - 1].Delta < 0)
                    {
                        worksheet.Cell(row, col).Style.Font.FontColor = XLColor.FromArgb(255, 0, 0);
                    }

                    col++;
                    worksheet.Cell(row, col).Value = " ";
                }

                col++;
                worksheet.Cell(row, col).Value = String.Format(cultureInfo, "{0}", report.RiepilogoProvvedimenti.Passaggi);
                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(226, 239, 218);
                worksheet.Cell(row, col).Style.Font.FontColor = XLColor.Black;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                col++;
                worksheet.Cell(row, col).Value = String.Format(cultureInfo, "{0}", report.RiepilogoProvvedimenti.IncrementiRetributivi);
                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(226, 239, 218);
                worksheet.Cell(row, col).Style.Font.FontColor = XLColor.Black;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                col++;
                worksheet.Cell(row, col).Value = String.Format(cultureInfo, "{0}", report.RiepilogoProvvedimenti.Gratifiche);
                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(226, 239, 218);
                worksheet.Cell(row, col).Style.Font.FontColor = XLColor.Black;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                col++;

                int totaleProvvedimneti = report.RiepilogoProvvedimenti.Passaggi + report.RiepilogoProvvedimenti.IncrementiRetributivi + report.RiepilogoProvvedimenti.Gratifiche;

                worksheet.Cell(row, col).Value = String.Format(cultureInfo, "{0}", totaleProvvedimneti);
                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(226, 239, 218);
                worksheet.Cell(row, col).Style.Font.FontColor = XLColor.Black;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                worksheet.Row(row).Height = 27;

                row++;
                worksheet.Cell(row, 1).Value = "";
                worksheet.Range(row, 1, row, defaultCell).Merge();
                worksheet.Row(row).Height = 20;

                row++;
                col = 1;
                worksheet.Cell(row, col).Value = "RISORSE";
                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
                worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(row, col).Style.Font.Bold = true;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                worksheet.Range(row, 1, row, 2).Merge();
                worksheet.Range(row, 3, row, defaultCell).Merge();
                worksheet.Row(row).Height = 27;

                row++;
                col = 1;
                worksheet.Cell(row, col).Value = "ANTICIPAZIONI POLITICHE RETRIBUTIVE " + anniDecorrenze[anniDecorrenze.Count() - 1] + " COSTO COMPLESSIVO:";
                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(31, 78, 120);
                worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(row, col).Style.Font.Bold = true;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                col++;
                worksheet.Cell(row, col).Value = String.Format(cultureInfo, "€ {0:N2}", report.RiepilogoRisorse.Anticipazioni);
                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                col++;
                worksheet.Cell(row, col).Value = "Totale interventi";
                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(31, 78, 120);
                worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(row, col).Style.Font.Bold = true;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                col++;
                worksheet.Cell(row, col).Value = report.RiepilogoRisorse.TotaleInterventi.ToString();
                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                worksheet.Row(row).Height = 50;

                row++;
                worksheet.Cell(row, 1).Value = "";
                worksheet.Range(row, 1, row, defaultCell).Merge();
                worksheet.Row(row).Height = 20;

                IncentiviEntities dbI = new IncentiviEntities();

                List<string> categoryIncluded = null;
                List<string> categoryExcluded = null;

                bool filtro = PoliticheRetributiveHelper.GetEnabledCategory(dbI, CommonHelper.GetCurrentUserMatricola(), out categoryIncluded, out categoryExcluded);

                string catPrevista = "Q10";
                bool enableQIO = PoliticheRetributiveHelper.EnableVis(PolRetrChiaveEnum.BudgetQIO);
                bool enableRS = PoliticheRetributiveHelper.EnableVis(PolRetrChiaveEnum.BudgetRS);

                List<XR_PRV_AREA> area = new List<XR_PRV_AREA>();
                area = PoliticheRetributiveHelper.GetEnabledAreas(dbI, CommonHelper.GetCurrentUserMatricola());

                if (enableRS && (!filtro || categoryIncluded.Any(a => catPrevista.StartsWith(a)) && !categoryExcluded.Any(b => catPrevista.StartsWith(b))))
                {
                    #region Dettaglio Funzionari Super
                    row++;
                    worksheet.Cell(row, 1).Value = "";
                    worksheet.Range(row, 1, row, defaultCell).Merge();
                    worksheet.Row(row).Height = 20;

                    row++;
                    col = 1;
                    worksheet.Cell(row, col).Value = "Dettaglio Funzionari Super";
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.Black;
                    worksheet.Cell(row, col).Style.Font.Bold = true;
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                    worksheet.Range(row, 1, row, 3).Merge();
                    worksheet.Row(row).Height = 20;
                    worksheet.Range(row, 1, row, 3).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(row, 1, row, 3).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(row, 1, row, 3).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(row, 1, row, 3).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(row, 4, row, defaultCell).Merge();

                    if (area.Any(w => w.NOME.Equals("AREA RISORSE CHIAVE")))
                    {
                        row++;
                        col = 1;
                        worksheet.Cell(row, col).Value = "AREA RISORSE CHIAVE";
                        worksheet.Cell(row, col).Style.Font.Bold = true;
                        worksheet.Cell(row, col).Style.Font.FontColor = XLColor.Black;
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                        worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        col++;
                        worksheet.Cell(row, col).Value = String.Format(cultureInfo, "€ {0:N2}", report.DettaglioPoliticheRetributive.Staff1);
                        worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                        worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                        worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        col++;
                        worksheet.Cell(row, col).Value = report.DettaglioPoliticheRetributive.Staff2;
                        worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                        worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                        worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        worksheet.Row(row).Height = 20;

                        row++;
                        col = 1;
                        worksheet.Cell(row, col).Value = "TOTALE";
                        worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                        worksheet.Cell(row, col).Style.Font.Bold = true;
                        worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                        worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        col++;
                        worksheet.Cell(row, col).Value = String.Format(cultureInfo, "€ {0:N2}", report.DettaglioPoliticheRetributive.Staff1);
                        worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                        worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                        worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        col++;
                        worksheet.Cell(row, col).Value = report.DettaglioPoliticheRetributive.Staff2;
                        worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                        worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                        worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        worksheet.Row(row).Height = 20;
                    }

                    #endregion
                }

                // aggiunge riga vuota
                row++;
                worksheet.Cell(row, 1).Value = "";
                worksheet.Range(row, 1, row, defaultCell).Merge();
                worksheet.Row(row).Height = 20;
                catPrevista = "Q11";

                if ((!filtro || categoryIncluded.Any(a => catPrevista.StartsWith(a)) && !categoryExcluded.Any(b => catPrevista.StartsWith(b))))
                {
                    #region Dettaglio Funzionari F1
                    row++;
                    col = 1;
                    worksheet.Cell(row, col).Value = "Dettaglio Funzionari F1";
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.Black;
                    worksheet.Cell(row, col).Style.Font.Bold = true;
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                    worksheet.Range(row, 1, row, 3).Merge();
                    worksheet.Row(row).Height = 20;
                    worksheet.Range(row, 1, row, 3).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(row, 1, row, 3).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(row, 1, row, 3).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(row, 1, row, 3).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(row, 4, row, defaultCell).Merge();

                    if (area.Any(w => w.NOME.Equals("AREA STAFF")))
                    {
                        row++;
                        col = 1;
                        worksheet.Cell(row, col).Value = "AREA STAFF";
                        worksheet.Cell(row, col).Style.Font.Bold = true;
                        worksheet.Cell(row, col).Style.Font.FontColor = XLColor.Black;
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                        worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        col++;
                        worksheet.Cell(row, col).Value = String.Format(cultureInfo, "€ {0:N2}", report.DettaglioFunzionariF1.Staff1);
                        worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                        worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                        worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        col++;
                        worksheet.Cell(row, col).Value = report.DettaglioFunzionariF1.Staff2;
                        worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                        worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                        worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        worksheet.Row(row).Height = 20;
                    }

                    if (area.Any(w => w.NOME.Equals("AREA EDITORIALE")))
                    {
                        row++;
                        col = 1;
                        worksheet.Cell(row, col).Value = "AREA EDITORIALE";
                        worksheet.Cell(row, col).Style.Font.FontColor = XLColor.Black;
                        worksheet.Cell(row, col).Style.Font.Bold = true;
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                        worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        col++;
                        worksheet.Cell(row, col).Value = String.Format(cultureInfo, "€ {0:N2}", report.DettaglioFunzionariF1.Edit1);
                        worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                        worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                        worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        col++;
                        worksheet.Cell(row, col).Value = report.DettaglioFunzionariF1.Edit2;
                        worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                        worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                        worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        worksheet.Row(row).Height = 20;
                    }

                    if (area.Any(w => w.NOME.Equals("AREA PRODUZIONE")))
                    {
                        row++;
                        col = 1;
                        worksheet.Cell(row, col).Value = "AREA PRODUZIONE";
                        worksheet.Cell(row, col).Style.Font.FontColor = XLColor.Black;
                        worksheet.Cell(row, col).Style.Font.Bold = true;
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                        worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        col++;
                        worksheet.Cell(row, col).Value = String.Format(cultureInfo, "€ {0:N2}", report.DettaglioFunzionariF1.Prod1);
                        worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                        worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                        worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        col++;
                        worksheet.Cell(row, col).Value = report.DettaglioFunzionariF1.Prod2;
                        worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                        worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                        worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        worksheet.Row(row).Height = 20;
                    }

                    if (area.Any(w => w.NOME.Equals("AREA STAFF")) ||
                        area.Any(w => w.NOME.Equals("AREA EDITORIALE")) ||
                        area.Any(w => w.NOME.Equals("AREA PRODUZIONE")))
                    {
                        row++;
                        col = 1;
                        worksheet.Cell(row, col).Value = "TOTALE";
                        worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                        worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                        worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        col++;
                        worksheet.Cell(row, col).Value = String.Format(cultureInfo, "€ {0:N2}", report.DettaglioFunzionariF1.Staff1 + report.DettaglioFunzionariF1.Edit1 + report.DettaglioFunzionariF1.Prod1);
                        worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                        worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                        worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        col++;
                        worksheet.Cell(row, col).Value = report.DettaglioFunzionariF1.Staff2 + report.DettaglioFunzionariF1.Edit2 + report.DettaglioFunzionariF1.Prod2;
                        worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                        worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                        worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        worksheet.Row(row).Height = 20;
                    }

                    #endregion Dettaglio Funzionari F1
                }

                row++;
                worksheet.Cell(row, 1).Value = "";
                worksheet.Range(row, 1, row, defaultCell).Merge();
                worksheet.Row(row).Height = 20;

                #region Dettaglio Altro Personale		

                row++;
                col = 1;
                worksheet.Cell(row, col).Value = "Dettaglio Altro Personale";
                worksheet.Cell(row, col).Style.Font.FontColor = XLColor.Black;
                worksheet.Cell(row, col).Style.Font.Bold = true;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                worksheet.Range(row, 1, row, 3).Merge();
                worksheet.Row(row).Height = 20;
                worksheet.Range(row, 1, row, 3).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Range(row, 1, row, 3).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                worksheet.Range(row, 1, row, 3).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Range(row, 1, row, 3).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                worksheet.Range(row, 4, row, defaultCell).Merge();

                if (area.Any(w => w.NOME.Equals("AREA STAFF")))
                {
                    row++;
                    col = 1;
                    worksheet.Cell(row, col).Value = "AREA STAFF";
                    worksheet.Cell(row, col).Style.Font.Bold = true;
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.Black;
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                    worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                    col++;
                    worksheet.Cell(row, col).Value = String.Format(cultureInfo, "€ {0:N2}", report.DettaglioAltroPersonale.Staff1);
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                    worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                    col++;
                    worksheet.Cell(row, col).Value = report.DettaglioAltroPersonale.Staff2;
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                    worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    worksheet.Row(row).Height = 20;
                }


                if (area.Any(w => w.NOME.Equals("AREA EDITORIALE")))
                {
                    row++;
                    col = 1;
                    worksheet.Cell(row, col).Value = "AREA EDITORIALE";
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.Black;
                    worksheet.Cell(row, col).Style.Font.Bold = true;
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                    worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                    col++;
                    worksheet.Cell(row, col).Value = String.Format(cultureInfo, "€ {0:N2}", report.DettaglioAltroPersonale.Edit1);
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                    worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                    col++;
                    worksheet.Cell(row, col).Value = report.DettaglioAltroPersonale.Edit2;
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                    worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    worksheet.Row(row).Height = 20;
                }


                if (area.Any(w => w.NOME.Equals("AREA PRODUZIONE")))
                {
                    row++;
                    col = 1;
                    worksheet.Cell(row, col).Value = "AREA PRODUZIONE";
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.Black;
                    worksheet.Cell(row, col).Style.Font.Bold = true;
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                    worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                    col++;
                    worksheet.Cell(row, col).Value = String.Format(cultureInfo, "€ {0:N2}", report.DettaglioAltroPersonale.Prod1);
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                    worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                    col++;
                    worksheet.Cell(row, col).Value = report.DettaglioAltroPersonale.Prod2;
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                    worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    worksheet.Row(row).Height = 20;
                }

                if (area.Any(w => w.NOME.Equals("AREA STAFF")) ||
                    area.Any(w => w.NOME.Equals("AREA EDITORIALE")) ||
                    area.Any(w => w.NOME.Equals("AREA PRODUZIONE")))
                {
                    row++;
                    col = 1;
                    worksheet.Cell(row, col).Value = "TOTALE";
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, col).Style.Font.Bold = true;
                    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                    worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                    col++;
                    worksheet.Cell(row, col).Value = String.Format(cultureInfo, "€ {0:N2}", report.DettaglioAltroPersonale.Staff1 + report.DettaglioAltroPersonale.Edit1 + report.DettaglioAltroPersonale.Prod1);
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                    worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                    col++;
                    worksheet.Cell(row, col).Value = report.DettaglioAltroPersonale.Staff2 + report.DettaglioAltroPersonale.Edit2 + report.DettaglioAltroPersonale.Prod2;
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                    worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    worksheet.Row(row).Height = 20;
                }

                #endregion Dettaglio Altro Personale


                #endregion

                var ncol = worksheet.Column(1);
                ncol.Width = 24;

                for (int nCell = 2; nCell <= defaultCell - 4; nCell++)
                {
                    ncol = worksheet.Column(nCell);
                    ncol.Width = 13;
                }

                ncol = worksheet.Column(5);
                ncol.Width = 0.92;

                if (countDecorrenze > 1)
                {
                    ncol = worksheet.Column(9);
                    ncol.Width = 0.92;
                }

                for (int nCell = defaultCell - 3; nCell <= defaultCell; nCell++)
                {
                    ncol = worksheet.Column(nCell);
                    ncol.Width = 10;
                }

                workbook.SaveAs(ms);
                ms.Position = 0;
            }
            catch (Exception ex)
            {

            }

            string nomeFile = String.Format("ReportPoliticheRetributive_{0}", DateTime.Now.Year);
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = nomeFile + ".xlsx" };
        }

        private FileStreamResult ReportPoliticheRetributiveAllCampagne(List<int> ids = null)
        {
            XLWorkbook workbook = new XLWorkbook();
            MemoryStream ms = new MemoryStream();
            var cultureInfo = CultureInfo.GetCultureInfo("it-IT");

            // nome del tab excel
            var worksheet = workbook.Worksheets.Add("ReportPoliticheRetrib");
            worksheet.PageSetup.PaperSize = XLPaperSize.A3Paper;
            worksheet.PageSetup.PageOrientation = XLPageOrientation.Landscape;
            worksheet.PageSetup.PagesWide = 1;

            int defaultCell = 9; // imposta il numero minimo di colonne a 9 (n decorrenze ma per l'anno corrente)
            bool enableQIO = PoliticheRetributiveHelper.EnableVis(PolRetrChiaveEnum.BudgetQIO);
            bool enableRS = PoliticheRetributiveHelper.EnableVis(PolRetrChiaveEnum.BudgetRS);

            List<PRV_DIREZIONE> direzioni = new List<PRV_DIREZIONE>();
            ReportPoliticheRetributive report = new ReportPoliticheRetributive();

            try
            {
                int countDecorrenze = 0;
                // prende le campagne alle quali sono abilitato
                List<InfoCampagna> campagne = this.BudgetDataController.GetCampagne(true);

                if (ids != null && ids.Any())
                {
                    campagne.RemoveAll(w => !ids.Contains(w.Id));
                }
                else
                {
                    campagne.RemoveAll(w => w.Id <= 2);
                }

                report.Intestazione = String.Format("POLITICHE RETRIBUTIVE {0}", DateTime.Now.Year);

                List<int> anniDecorrenze = new List<int>();
                List<DateTime> dateDecorrenze = new List<DateTime>();

                campagne.ForEach(w =>
               {
                   dateDecorrenze.AddRange(w.Decorrenze.Distinct().ToList());
               });

                anniDecorrenze = dateDecorrenze.Select(w => w.Year).Distinct().ToList();

                //2021-06-23 - Nel caso ci sia una sola decorrenza non viene visualizzato il costo annuo
                //Come soluzione temporanea, viene aggiunta una decorrenza uguale alla prima
                if (anniDecorrenze.Count() == 1)
                    anniDecorrenze.Add(anniDecorrenze[0]);

                countDecorrenze = anniDecorrenze.Count();

                defaultCell += (countDecorrenze - 1) * 4;
                int row = 1;

                #region Preparazione Oggetto

                report.RiepilogoPerAnno = new List<ReportPoliticheRetributiveYearItem>();

                for (int idx = 1; idx <= countDecorrenze; idx++)
                {
                    decimal _budgetA = 0;
                    decimal _budgetP = 0;
                    decimal _costoA = 0;
                    decimal _costoP = 0;

                    decimal budgetA = 0;
                    decimal budgetP = 0;
                    decimal costoA = 0;
                    decimal costoP = 0;

                    foreach (var campagna in campagne)
                    {
                        this.BudgetDataController.GetBudgetAnnuoPeriodoCampagna(campagna, out _budgetA, out _budgetP);
                        this.BudgetDataController.GetCostoAnnuoPeriodoCampagna(campagna, out _costoA, out _costoP, anniDecorrenze[0]);

                        budgetA += _budgetA;
                        budgetP += _budgetP;
                        costoA += _costoA;
                        costoP += _costoP;
                    }

                    if (idx > 1)
                    {
                        report.RiepilogoPerAnno.Add(new ReportPoliticheRetributiveYearItem()
                        {
                            Budget = budgetA,
                            Costo = costoA,
                            Delta = budgetA - costoA
                        });
                    }
                    else
                    {
                        report.RiepilogoPerAnno.Add(new ReportPoliticheRetributiveYearItem()
                        {
                            Budget = budgetP,
                            Costo = costoP,
                            Delta = budgetP - costoP
                        });
                    }
                }
                report.RiepilogoProvvedimenti = new ReportPoliticheRetributiveDetailsItem();
                report.RiepilogoRisorse = new ReportPoliticheRetributiveRisorsePanelDetailsItem();
                report.DettaglioAltroPersonale = new ReportPoliticheRetributiveTipologiaItem();
                report.DettaglioFunzionariF1 = new ReportPoliticheRetributiveTipologiaItem();
                report.DettaglioPoliticheRetributive = new ReportPoliticheRetributiveTipologiaItem();
                report.DettaglioAllCampagne = new List<ReportPoliticheRetributiveDettaglioPiano>();

                var isProm = PoliticheRetributiveHelper.AnyOfProv(PoliticheRetributiveHelper.GetProvBySigla(null, PoliticheRetributiveHelper.SIGLA_PROMOZIONI));
                var isAumento = PoliticheRetributiveHelper.AnyOfProv(PoliticheRetributiveHelper.GetProvBySigla(null, PoliticheRetributiveHelper.SIGLA_AUMENTI));
                var isGratifica = PoliticheRetributiveHelper.AnyOfProv(PoliticheRetributiveHelper.GetProvBySigla(null, PoliticheRetributiveHelper.SIGLA_GRATIFICHE));

                foreach (var campagna in campagne)
                {
                    #region Totali provvedimenti
                    report.RiepilogoProvvedimenti.Passaggi += this.BudgetDataController.GetSumProvvedimenti(campagna.Id, isProm, true, anniDecorrenze[0]);

                    report.RiepilogoProvvedimenti.IncrementiRetributivi += this.BudgetDataController.GetSumProvvedimenti(campagna.Id, isAumento, true, anniDecorrenze[0]);

                    report.RiepilogoProvvedimenti.Gratifiche += this.BudgetDataController.GetSumProvvedimenti(campagna.Id, isGratifica, true, anniDecorrenze[0]);

                    #endregion

                    #region Totali provvedimenti anno prossimo
                    report.RiepilogoRisorse.Anticipazioni += this.BudgetDataController.GetCostoAnnoCampagna(campagna.Id, false, anniDecorrenze[anniDecorrenze.Count() - 1]);

                    report.RiepilogoRisorse.TotaleInterventi += this.BudgetDataController.GetSumProvvedimenti(campagna.Id, isProm, false, anniDecorrenze[anniDecorrenze.Count() - 1])
                                + this.BudgetDataController.GetSumProvvedimenti(campagna.Id, isAumento, false, anniDecorrenze[anniDecorrenze.Count() - 1])
                                + this.BudgetDataController.GetSumProvvedimenti(campagna.Id, isGratifica, false, anniDecorrenze[anniDecorrenze.Count() - 1]);
                    #endregion
                }

                List<PRV_Dipendente> dipendenti = new List<PRV_Dipendente>();
                report.DettaglioAllCampagne = new List<ReportPoliticheRetributiveDettaglioPiano>();

                //dipendenti = this.BudgetDataController.GetDipendentiAllCampagne(DateTime.Now.Year, anniDecorrenze[anniDecorrenze.Count() - 1], true);
                dipendenti = this.BudgetDataController.GetDipendentiAllCampagne(DateTime.Now.Year, anniDecorrenze[anniDecorrenze.Count() - 1], true, ids);

                var listaPiano = dipendenti.Select(w => new { ID = w.IdCampagna, Nome = w.Campagna.NOME }).Distinct();

                using (var db = new IncentiviEntities())
                {
                    var funcFilter = PoliticheRetributiveHelper.FuncFilterCampagna();
                    var rslt = db.XR_PRV_CAMPAGNA.Where(funcFilter).Where(x => x.DTA_FINE == null || x.DTA_FINE.Value > DateTime.Today).ToList();

                    if (rslt != null && rslt.Any())
                    {
                        listaPiano.Where(w => rslt.Select(y => y.ID_CAMPAGNA).ToList().Contains(w.ID.Value));
                    }
                }

                IncentiviEntities dbI = new IncentiviEntities();
                List<XR_PRV_AREA> areas = new List<XR_PRV_AREA>();
                areas = PoliticheRetributiveHelper.GetEnabledAreas(dbI, CommonHelper.GetCurrentUserMatricola());

                var lstPiano = listaPiano.Where(w => w.ID > 2).OrderBy(w => w.ID);
                var lstPiano2 = listaPiano.Where(w => w.ID <= 2).OrderBy(w => w.ID);

                // per ogni piano deve creare un BOX riepilogativo
                foreach (var p in lstPiano)
                {
                    ReportPoliticheRetributiveDettaglioPiano piano = new ReportPoliticheRetributiveDettaglioPiano();
                    piano.Items = new List<ReportPoliticheRetributiveDettaglioPianoItem>();
                    piano.Intestazione = p.Nome;

                    // reperimento delle aree per il piano in oggetto
                    var listaAree = dipendenti.Where(w => w.IdCampagna == p.ID).Select(w => new { IdArea = w.IdArea, Nome = w.Area.NOME, LV_ABIL = w.Area.LV_ABIL, Ordine = w.Area.ORDINE }).Distinct();

                    listaAree = listaAree.OrderBy(w => w.Ordine);

                    // per ogni area deve sommare le promozioni, aumenti e promozioni + aumenti + gratifiche
                    foreach (var area in listaAree)
                    {
                        ReportPoliticheRetributiveDettaglioPianoItem i = new ReportPoliticheRetributiveDettaglioPianoItem();
                        i.Descrizione = area.Nome;

                        var listaDipendeti = dipendenti.Where(w => w.IdArea.Equals(area.IdArea.Value) && w.IdCampagna.Value == p.ID.Value);

                        if (listaDipendeti != null && listaDipendeti.Any())
                        {
                            //listaDipendeti.ForEach(w =>
                            foreach (var w in listaDipendeti)
                            {
                                i.Colonna1 += (w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoLivello ||
                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoLivello ||
                                        w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoMerito ||
                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoMerito) ? w.CostoAnnuo : 0;

                                i.Colonna2 += (w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoLivello ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoLivello ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoMerito ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoMerito) ? 1 : 0;
                            }
                        }

                        piano.Items.Add(i);
                    }
                    report.DettaglioAllCampagne.Add(piano);
                }

                foreach (var p in lstPiano2)
                {
                    ReportPoliticheRetributiveDettaglioPiano piano = new ReportPoliticheRetributiveDettaglioPiano();
                    piano.Items = new List<ReportPoliticheRetributiveDettaglioPianoItem>();
                    piano.Intestazione = p.Nome;

                    // reperimento delle aree per il piano in oggetto
                    var listaAree = dipendenti.Where(w => w.IdCampagna == p.ID).Select(w => new { IdArea = w.IdArea, Nome = w.Area.NOME, LV_ABIL = w.Area.LV_ABIL, Ordine = w.Area.ORDINE }).Distinct();

                    listaAree = listaAree.OrderBy(w => w.Ordine);

                    // per ogni area deve sommare le promozioni, aumenti e promozioni + aumenti + gratifiche
                    foreach (var area in listaAree)
                    {
                        ReportPoliticheRetributiveDettaglioPianoItem i = new ReportPoliticheRetributiveDettaglioPianoItem();
                        i.Descrizione = area.Nome;

                        var listaDipendeti = dipendenti.Where(w => w.IdArea.Equals(area.IdArea.Value) && w.IdCampagna.Value == p.ID.Value);

                        if (listaDipendeti != null && listaDipendeti.Any())
                        {
                            foreach (var w in listaDipendeti)
                            {
                                i.Colonna1 += (w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoLivello ||
                                                                       w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoLivello ||
                                                                       w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoMerito ||
                                                                       w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoMerito) ? w.CostoAnnuo : 0;

                                i.Colonna2 += (w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoLivello ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoLivello ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.AumentoMerito ||
                                                                        w.IdProvvedimento == (int)ProvvedimentiEnum.CUSAumentoMerito) ? 1 : 0;
                            }
                        }

                        piano.Items.Add(i);
                    }
                    report.DettaglioAllCampagne.Add(piano);
                }

                #endregion

                #region Intestazione
                worksheet.Cell(row, 1).Value = report.Intestazione;
                worksheet.Cell(row, 1).Style.Font.FontSize = 15;
                worksheet.Cell(row, 1).Style.Border.BottomBorder = XLBorderStyleValues.None;
                worksheet.Cell(row, 1).Style.Border.TopBorder = XLBorderStyleValues.None;
                worksheet.Cell(row, 1).Style.Border.LeftBorder = XLBorderStyleValues.None;
                worksheet.Cell(row, 1).Style.Border.RightBorder = XLBorderStyleValues.None;
                worksheet.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, 1).Style.Alignment.WrapText = true;
                worksheet.Row(row).Height = 27;
                worksheet.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.FromArgb(155, 194, 230);
                worksheet.Cell(row, 1).Style.Font.FontColor = XLColor.White;
                worksheet.Range(row, 1, row, defaultCell).Merge();
                #endregion

                // aggiunge riga vuota
                row++;
                worksheet.Cell(row, 1).Value = "";
                worksheet.Range(row, 1, row, defaultCell).Merge();
                worksheet.Row(row).Height = 20;

                // aggiunge l'intestazione per l'ultimo pannello, quello coi riepiloghi dei provvedimenti
                row++;
                worksheet.Cell(row, 1).Value = "";
                worksheet.Range(row, 1, row, defaultCell - 4).Merge();
                worksheet.Cell(row, defaultCell - 3).Value = "n. provvedimenti";
                worksheet.Cell(row, defaultCell - 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, defaultCell - 3).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, defaultCell - 3).Style.Font.Bold = true;
                worksheet.Range(row, defaultCell - 3, row, defaultCell).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Range(row, defaultCell - 3, row, defaultCell).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                worksheet.Range(row, defaultCell - 3, row, defaultCell).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Range(row, defaultCell - 3, row, defaultCell).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                worksheet.Range(row, defaultCell - 3, row, defaultCell).Merge();
                worksheet.Row(row).Height = 20;

                #region Intestazioni colonne1
                row++;
                int col = 1;

                worksheet.Cell(row, col).Value = "";

                for (int idxDec = 1; idxDec <= countDecorrenze; idxDec++)
                {
                    col++;
                    if (idxDec > 1)
                    {
                        worksheet.Cell(row, col).Value = "BUDGET ANNUO";
                    }
                    else
                    {
                        worksheet.Cell(row, col).Value = "BUDGET PERIODO";
                    }
                    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(31, 78, 120);
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                    worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    col++;
                    if (idxDec > 1)
                    {
                        worksheet.Cell(row, col).Value = "COSTO ANNUO (" + anniDecorrenze[0] + ")";
                    }
                    else
                    {
                        worksheet.Cell(row, col).Value = "COSTO PERIODO (" + anniDecorrenze[0] + ")";
                    }
                    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(31, 78, 120);
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                    worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    col++;
                    worksheet.Cell(row, col).Value = "DELTA";
                    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(31, 78, 120);
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                    worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    col++;
                    worksheet.Cell(row, col).Value = " ";
                }

                col++;
                worksheet.Cell(row, col).Value = "Passaggi";
                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(226, 239, 218);
                worksheet.Cell(row, col).Style.Font.FontColor = XLColor.Black;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                col++;
                worksheet.Cell(row, col).Value = "Incrementi retributivi";
                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(226, 239, 218);
                worksheet.Cell(row, col).Style.Font.FontColor = XLColor.Black;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                col++;
                worksheet.Cell(row, col).Value = "Gratifiche";
                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(226, 239, 218);
                worksheet.Cell(row, col).Style.Font.FontColor = XLColor.Black;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                col++;
                worksheet.Cell(row, col).Value = "Totale";
                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(226, 239, 218);
                worksheet.Cell(row, col).Style.Font.FontColor = XLColor.Black;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                worksheet.Row(row).Height = 45;
                #endregion

                #region Corpo
                col = 1;
                row++;

                worksheet.Cell(row, col).Value = "TOTALE";
                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(31, 78, 120);
                worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                for (int idxDec = 1; idxDec <= countDecorrenze; idxDec++)
                {
                    col++;
                    worksheet.Cell(row, col).Value = String.Format(cultureInfo, "€ {0:N2}", report.RiepilogoPerAnno[idxDec - 1].Budget);
                    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                    worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    col++;
                    worksheet.Cell(row, col).Value = String.Format(cultureInfo, "€ {0:N2}", report.RiepilogoPerAnno[idxDec - 1].Costo);
                    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                    worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    col++;
                    worksheet.Cell(row, col).Value = String.Format(cultureInfo, "€ {0:N2}", report.RiepilogoPerAnno[idxDec - 1].Delta);
                    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                    worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                    if (report.RiepilogoPerAnno[idxDec - 1].Delta > 0)
                    {
                        worksheet.Cell(row, col).Style.Font.FontColor = XLColor.FromArgb(0, 176, 80);
                    }
                    else if (report.RiepilogoPerAnno[idxDec - 1].Delta < 0)
                    {
                        worksheet.Cell(row, col).Style.Font.FontColor = XLColor.FromArgb(255, 0, 0);
                    }

                    col++;
                    worksheet.Cell(row, col).Value = " ";
                }

                col++;
                worksheet.Cell(row, col).Value = String.Format(cultureInfo, "{0}", report.RiepilogoProvvedimenti.Passaggi);
                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(226, 239, 218);
                worksheet.Cell(row, col).Style.Font.FontColor = XLColor.Black;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                col++;
                worksheet.Cell(row, col).Value = String.Format(cultureInfo, "{0}", report.RiepilogoProvvedimenti.IncrementiRetributivi);
                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(226, 239, 218);
                worksheet.Cell(row, col).Style.Font.FontColor = XLColor.Black;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                col++;
                worksheet.Cell(row, col).Value = String.Format(cultureInfo, "{0}", report.RiepilogoProvvedimenti.Gratifiche);
                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(226, 239, 218);
                worksheet.Cell(row, col).Style.Font.FontColor = XLColor.Black;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                col++;

                int totaleProvvedimneti = report.RiepilogoProvvedimenti.Passaggi + report.RiepilogoProvvedimenti.IncrementiRetributivi + report.RiepilogoProvvedimenti.Gratifiche;

                worksheet.Cell(row, col).Value = String.Format(cultureInfo, "{0}", totaleProvvedimneti);
                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(226, 239, 218);
                worksheet.Cell(row, col).Style.Font.FontColor = XLColor.Black;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                worksheet.Row(row).Height = 27;

                row++;
                worksheet.Cell(row, 1).Value = "";
                worksheet.Range(row, 1, row, defaultCell).Merge();
                worksheet.Row(row).Height = 20;

                #region RISORSE
                row++;
                col = 1;
                worksheet.Cell(row, col).Value = "RISORSE";
                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 112, 192);
                worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(row, col).Style.Font.Bold = true;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                worksheet.Range(row, 1, row, 2).Merge();
                worksheet.Range(row, 3, row, defaultCell).Merge();
                worksheet.Row(row).Height = 27;

                row++;
                col = 1;
                worksheet.Cell(row, col).Value = "ANTICIPAZIONI POLITICHE RETRIBUTIVE " + anniDecorrenze[anniDecorrenze.Count() - 1] + " COSTO COMPLESSIVO:";
                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(31, 78, 120);
                worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(row, col).Style.Font.Bold = true;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                col++;
                worksheet.Cell(row, col).Value = String.Format(cultureInfo, "€ {0:N2}", report.RiepilogoRisorse.Anticipazioni);
                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                col++;
                worksheet.Cell(row, col).Value = "Totale interventi";
                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(31, 78, 120);
                worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(row, col).Style.Font.Bold = true;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                col++;
                worksheet.Cell(row, col).Value = report.RiepilogoRisorse.TotaleInterventi.ToString();
                worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                worksheet.Row(row).Height = 50;

                row++;
                worksheet.Cell(row, 1).Value = "";
                worksheet.Range(row, 1, row, defaultCell).Merge();
                worksheet.Row(row).Height = 20;
                #endregion

                #region Dettaglio PIANO

                foreach (var reportPiano in report.DettaglioAllCampagne)
                {
                    row++;
                    col = 1;
                    worksheet.Cell(row, col).Value = reportPiano.Intestazione;
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.Black;
                    worksheet.Cell(row, col).Style.Font.Bold = true;
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                    worksheet.Range(row, 1, row, 3).Merge();
                    worksheet.Row(row).Height = 30;
                    worksheet.Range(row, 1, row, 3).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(row, 1, row, 3).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(row, 1, row, 3).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(row, 1, row, 3).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    worksheet.Range(row, 4, row, defaultCell).Merge();

                    foreach (var i in reportPiano.Items)
                    {
                        row++;
                        col = 1;
                        worksheet.Cell(row, col).Value = i.Descrizione;
                        worksheet.Cell(row, col).Style.Font.Bold = true;
                        worksheet.Cell(row, col).Style.Font.FontColor = XLColor.Black;
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                        worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        col++;
                        worksheet.Cell(row, col).Value = String.Format(cultureInfo, "€ {0:N2}", i.Colonna1);
                        worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                        worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                        worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                        col++;
                        worksheet.Cell(row, col).Value = i.Colonna2;
                        worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                        worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                        worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                        worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                        worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                        worksheet.Row(row).Height = 20;
                    }

                    row++;
                    col = 1;
                    worksheet.Cell(row, col).Value = "TOTALE";
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, col).Style.Font.Bold = true;
                    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                    worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                    col++;
                    worksheet.Cell(row, col).Value = String.Format(cultureInfo, "€ {0:N2}", reportPiano.Items.Sum(w => w.Colonna1));
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                    worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;

                    col++;
                    worksheet.Cell(row, col).Value = reportPiano.Items.Sum(w => w.Colonna2);
                    worksheet.Cell(row, col).Style.Font.FontColor = XLColor.White;
                    worksheet.Cell(row, col).Style.Fill.BackgroundColor = XLColor.FromArgb(172, 185, 202);
                    worksheet.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                    worksheet.Cell(row, col).Style.Alignment.WrapText = true;
                    worksheet.Cell(row, col).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.TopBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.LeftBorder = XLBorderStyleValues.Thin;
                    worksheet.Cell(row, col).Style.Border.RightBorder = XLBorderStyleValues.Thin;
                    worksheet.Row(row).Height = 20;

                    row++;
                    worksheet.Cell(row, 1).Value = "";
                    worksheet.Range(row, 1, row, defaultCell).Merge();
                    worksheet.Row(row).Height = 20;
                }

                #endregion Dettaglio PIANO

                #endregion

                var ncol = worksheet.Column(1);
                ncol.Width = 24;

                for (int nCell = 2; nCell <= defaultCell - 4; nCell++)
                {
                    ncol = worksheet.Column(nCell);
                    ncol.Width = 13;
                }

                ncol = worksheet.Column(5);
                ncol.Width = 0.92;

                if (countDecorrenze > 1)
                {
                    ncol = worksheet.Column(9);
                    ncol.Width = 0.92;
                }

                for (int nCell = defaultCell - 3; nCell <= defaultCell; nCell++)
                {
                    ncol = worksheet.Column(nCell);
                    ncol.Width = 10;
                }

                workbook.SaveAs(ms);
                ms.Position = 0;
            }
            catch (Exception ex)
            {

            }

            string nomeFile = String.Format("ReportPoliticheRetributive_{0}", DateTime.Now.Year);
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = nomeFile + ".xlsx" };
        }


        public ActionResult ReportPoliticheRetributiveSelected(string selezione)
        {
            FileStreamResult myFile = null;

            List<string> idstringa = new List<string>();
            List<int> ids = new List<int>();
            if (!String.IsNullOrEmpty(selezione))
            {
                idstringa = selezione.Split(',').ToList();

                if (idstringa != null && idstringa.Any())
                {
                    foreach (var s in idstringa)
                    {
                        int item = int.Parse(s);
                        ids.Add(item);
                    }
                }
            }

            if (ids != null && ids.Any())
            {
                try
                {
                    myFile = ReportPoliticheRetributiveAllCampagne(ids);
                }
                catch (Exception ex)
                {
                    myFile = null;
                }
                return myFile;
            }

            return null;
        }

        /// <summary>
        /// Stampa del report delle politiche retributive per la singola campagna o 
        /// per tutte le campagne
        /// </summary>
        /// <param name="idCamp">0 - tutte le campagne. N identificativo della campagna</param>
        /// <returns></returns>
        public ActionResult ReportPoliticheRetributive(int idCamp)
        {
            FileStreamResult myFile = null;
            try
            {
                if (idCamp == 0)
                {
                    myFile = ReportPoliticheRetributiveAllCampagne();
                }
                else
                {
                    myFile = ReportPoliticheRetributiveSingolaCampagna(idCamp);
                }
            }
            catch (Exception ex)
            {
                myFile = null;
            }
            return myFile;
        }
        #endregion

        #region Salvataggio

        public ActionResult SalvaSimulazione(int idDir, int idCamp, List<TabellaItem> rows, int? anno = null)
        {
            SalvaSimulazioneResponse result = new SalvaSimulazioneResponse();
            try
            {
                result.Esito = this.BudgetDataController.SalvaSimulazione(CommonHelper.GetCurrentUserMatricola(), idDir, idCamp, rows, anno);
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Consolidamento
        public ActionResult ConsolidaSimulazione(int idDir, int idCamp, List<TabellaItem> rows)
        {
            SalvaSimulazioneResponse result = new SalvaSimulazioneResponse();
            try
            {
                result.Esito = this.BudgetDataController.ConsolidaSimulazione(CommonHelper.GetCurrentUserMatricola(), idDir, idCamp, rows);
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SbloccaSimulazione(int idDir, int idCamp)
        {
            SalvaSimulazioneResponse result = new SalvaSimulazioneResponse();
            try
            {
                result.Esito = this.BudgetDataController.SbloccaSimulazione(CommonHelper.GetCurrentUserMatricola(), idDir, idCamp);
            }
            catch (Exception ex)
            {
                result.Esito = false;
                result.Errore = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region private
        private void CaricaProvvedimenti(int? selectedItem = null)
        {
            var provv = this.BudgetDataController.GetProvvedimenti();

            var result = new List<KeyValuePair<int, string>>();

            foreach (var item in provv)
            {
                result.Add(new KeyValuePair<int, string>(item.ID_PROV, item.NOME));
            }

            ViewBag.Provvedimenti = new SelectList(result, "Key", "value", selectedItem);
        }

        private BudgetDataController BudgetDataController { get; set; }

        private ReportDirezione GestioneDirezioneCanone(int idCamp, int idArea)
        {
            ReportDirezione result = this.BudgetDataController.GetDirezioneNelleSedi("20", idCamp, idArea);
            return result;
        }

        private BudgetVM GetBudgetVM(int? idCamp = null, int? idArea = null, bool allCampagne = true, int? anno = null)
        {
            BudgetVM model = new BudgetVM();
            model.InfoCampagna = new InfoCampagna();
            model.ReportDirezioni = new List<ReportDirezione>();
            model.Campagne = new List<InfoCampagna>();

            if (allCampagne)
            {
                model.Campagne = this.BudgetDataController.GetCampagne();
            }
            else
            {
                model.Campagne = new List<InfoCampagna>();
            }

            if (idCamp.HasValue)
            {
                // reperimento della campagna corrente
                model.InfoCampagna = BudgetDataController.GetCampagna(CommonHelper.GetCurrentUserMatricola(), idCamp.Value, idArea, anno);
            }
            else
            {
                // reperimento della campagna corrente
                model.InfoCampagna = BudgetDataController.GetCurrentCampagna(CommonHelper.GetCurrentUserMatricola(), anno);
            }

            // cicla tutte le aree per reperire le direzioni ed i dati riguardanti le singole direzioni
            if (model.InfoCampagna != null && model.InfoCampagna.DettaglioCampagna != null &&
                model.InfoCampagna.DettaglioCampagna.Any())
            {
                DettaglioCampagna areaFilter = new DettaglioCampagna();

                // se è stata passata l'area allora verranno filtrati i dati per la sola area desiderata
                if (idArea.HasValue)
                {
                    areaFilter = model.InfoCampagna.DettaglioCampagna.Where(w => w.IdArea.Equals(idArea.Value)).FirstOrDefault();

                    if (areaFilter != null)
                        model.InfoCampagna.DettaglioCampagna.RemoveAll(w => !w.IdArea.Equals(areaFilter.IdArea));
                }

                foreach (var area in model.InfoCampagna.DettaglioCampagna)
                {
                    if (model.InfoCampagna.CampagneContenute == null || model.InfoCampagna.CampagneContenute.Count() == 0)
                        model.ReportDirezioni.AddRange(GetReportDirezioni(area, model.InfoCampagna.Id, anno));
                    else
                        model.ReportDirezioni.AddRange(GetReportDirezioni(area, model.InfoCampagna.CampagneContenute, anno));
                }
            }
            return model;
        }

        private List<ReportDirezione> GetReportDirezioni(DettaglioCampagna area, int idCampagna, int? anno = null)
        {
            List<ReportDirezione> reportDirezioni = new List<ReportDirezione>();
            List<PRV_DIREZIONE> direzioni = new List<PRV_DIREZIONE>();

            direzioni = this.BudgetDataController.GetDirezioniByIdArea(CommonHelper.GetCurrentUserMatricola(), area.IdArea, idCampagna, anno);

            if (direzioni != null && direzioni.Any())
            {
                int ordine = 1;
                int totaleDipendentiArea = direzioni.Sum(w => w.ORGANICO);
                var direzioniOrdinate = direzioni.OrderBy(w => w.Ordine.GetValueOrDefault());

                var isProm = PoliticheRetributiveHelper.AnyOfProv(PoliticheRetributiveHelper.GetProvBySigla(null, PoliticheRetributiveHelper.SIGLA_PROMOZIONI));
                var isAumento = PoliticheRetributiveHelper.AnyOfProv(PoliticheRetributiveHelper.GetProvBySigla(null, PoliticheRetributiveHelper.SIGLA_AUMENTI));
                var isGratifica = PoliticheRetributiveHelper.AnyOfProv(PoliticheRetributiveHelper.GetProvBySigla(null, PoliticheRetributiveHelper.SIGLA_GRATIFICHE));

                var dirs = direzioniOrdinate.Select(x => x.ID_DIREZIONE);
                var distribuzioneProv = this.BudgetDataController.GetDistrProvvByDirezione(idCampagna, dirs, null, false, anno);

                foreach (var dir in direzioniOrdinate)
                {
                    ReportDirezione temp = new ReportDirezione();
                    temp.Id = dir.ID_DIREZIONE;
                    temp.Area = area.NomeArea;
                    temp.Direzione = dir.NOME;
                    temp.OrganicoContabile = dir.ORGANICO_CONTABILE.GetValueOrDefault();
                    temp.OrganicoRipartizione = dir.ORGANICO_AD;
                    //temp.OrganicoRipartizione = dir.ORGANICO - this.BudgetDataController.GetProvvedimentiByDirezione(dir.ID_DIREZIONE, DateTime.Today.Year);

                    //temp.Promozioni = this.BudgetDataController.GetSumProvvByDirezione(idCampagna, dir.ID_DIREZIONE, isProm, null, false, anno);
                    //temp.Aumenti = this.BudgetDataController.GetSumProvvByDirezione(idCampagna, dir.ID_DIREZIONE, isAumento, null, false, anno);
                    //temp.Gratifiche = this.BudgetDataController.GetSumProvvByDirezione(idCampagna, dir.ID_DIREZIONE, isGratifica, null, false, anno);
                    var distro = distribuzioneProv.FirstOrDefault(x => x.IdDir == dir.ID_DIREZIONE);
                    if (distro != null)
                    {
                        int nProv = 0;
                        temp.Promozioni = distro.Report.TryGetValue(PoliticheRetributiveHelper.SIGLA_PROMOZIONI, out nProv) ? nProv : 0;
                        temp.Aumenti = distro.Report.TryGetValue(PoliticheRetributiveHelper.SIGLA_AUMENTI, out nProv) ? nProv : 0;
                        temp.Gratifiche = distro.Report.TryGetValue(PoliticheRetributiveHelper.SIGLA_GRATIFICHE, out nProv) ? nProv : 0;
                    }
                    else
                    {
                        temp.Promozioni = 0;
                        temp.Aumenti = 0;
                        temp.Gratifiche = 0;

                    }

                    decimal costoAnnuo = 0;
                    decimal costoPeriodo = 0;
                    this.BudgetDataController.GetCostoAnnuoPeriodoDirezione(dir.ID_DIREZIONE, idCampagna, out costoAnnuo, out costoPeriodo, anno);
                    temp.CostoPeriodoAnnoCorrente = costoPeriodo;
                    temp.CostoAnnoCorrente = costoAnnuo;

                    if (anno.HasValue && anno.GetValueOrDefault() > DateTime.Now.Year)
                    {
                        temp.CostoPeriodoAnnoCorrente = 0;
                    }

                    temp.TotProvv = temp.Promozioni + temp.Gratifiche + temp.Aumenti;

                    if (temp.OrganicoContabile > 0)
                    {
                        decimal _pOrg = (decimal)((decimal)temp.TotProvv / (decimal)temp.OrganicoContabile);
                        _pOrg *= 100;

                        temp.PercentualeSuOrganico = (int)Math.Round(_pOrg);
                    }
                    else
                    {
                        temp.PercentualeSuOrganico = 0;
                    }

                    temp.BudgetAnnoCorrente = dir.BUDGET;

                    if (anno.HasValue && anno.GetValueOrDefault() != DateTime.Now.Year)
                    {
                        temp.BudgetAnnoCorrente = 0;
                    }

                    temp.BudgetPeriodoCorrente = this.BudgetDataController.GetBudgetPeriodoDirezione(dir.ID_DIREZIONE, idCampagna);
                    temp.DeltaSuCostoAnnoCorrente = temp.BudgetAnnoCorrente - temp.CostoAnnoCorrente;
                    temp.DeltaSuCostoPeriodo = temp.BudgetPeriodoCorrente - temp.CostoPeriodoAnnoCorrente;
                    temp.IsConsolidata = this.BudgetDataController.DirezioneIsConsolidata(idCampagna, dir.ID_DIREZIONE);

                    temp.CostoPeriodoAnnoPrecedente = 0;
                    temp.BudgetAnnoPrecedente = 0;
                    temp.CostoAnnoPrecedente = 0;
                    temp.CostoRegime = 0;
                    temp.BudgetPeriodoAnnoPrecedente = 0;
                    temp.CostoRecuperoStraordinario = 0;
                    temp.DeltaSuCostoAnnoPrecedente = temp.BudgetAnnoPrecedente - temp.CostoAnnoPrecedente;
                    temp.Ordine = ordine;
                    temp.Visibled = temp.TotProvv > 0;

                    reportDirezioni.Add(temp);
                    ordine++;
                }
            }

            return reportDirezioni;
        }

        private List<ReportDirezione> GetReportDirezioni(DettaglioCampagna area, List<int> idCampagnas, int? anno = null)
        {
            List<ReportDirezione> reportDirezioni = new List<ReportDirezione>();
            List<PRV_DIREZIONE> direzioni = new List<PRV_DIREZIONE>();

            direzioni = this.BudgetDataController.GetDirezioniByIdArea(CommonHelper.GetCurrentUserMatricola(), area.IdArea, idCampagnas);

            if (direzioni != null && direzioni.Any())
            {
                int ordine = 1;
                int totaleDipendentiArea = direzioni.Sum(w => w.ORGANICO);
                var direzioniOrdinate = direzioni.OrderBy(w => w.Ordine.GetValueOrDefault());

                var isProm = PoliticheRetributiveHelper.AnyOfProv(PoliticheRetributiveHelper.GetProvBySigla(null, PoliticheRetributiveHelper.SIGLA_PROMOZIONI));
                var isAumento = PoliticheRetributiveHelper.AnyOfProv(PoliticheRetributiveHelper.GetProvBySigla(null, PoliticheRetributiveHelper.SIGLA_AUMENTI));
                var isGratifica = PoliticheRetributiveHelper.AnyOfProv(PoliticheRetributiveHelper.GetProvBySigla(null, PoliticheRetributiveHelper.SIGLA_GRATIFICHE));

                foreach (var dir in direzioniOrdinate)
                {
                    ReportDirezione temp = new ReportDirezione();
                    temp.Id = dir.ID_DIREZIONE;
                    temp.Codice = dir.CODICE;
                    temp.Area = area.NomeArea;
                    temp.Direzione = dir.NOME;
                    temp.OrganicoContabile = dir.ORGANICO_CONTABILE.GetValueOrDefault();
                    temp.OrganicoRipartizione = dir.ORGANICO_AD;
                    //temp.OrganicoRipartizione = dir.ORGANICO - this.BudgetDataController.GetProvvedimentiByDirezione(dir.ID_DIREZIONE, DateTime.Today.Year);
                    temp.Promozioni = this.BudgetDataController.GetSumProvvByDirezione(idCampagnas, dir.ID_DIREZIONE, isProm, null, false, anno);

                    temp.Aumenti = this.BudgetDataController.GetSumProvvByDirezione(idCampagnas, dir.ID_DIREZIONE, isAumento, null, false, anno);

                    temp.Gratifiche = this.BudgetDataController.GetSumProvvByDirezione(idCampagnas, dir.ID_DIREZIONE, isGratifica, null, false, anno);


                    decimal costoAnnuo = 0;
                    decimal costoPeriodo = 0;
                    this.BudgetDataController.GetCostoAnnuoPeriodoDirezione(dir.ID_DIREZIONE, idCampagnas, out costoAnnuo, out costoPeriodo, anno);
                    temp.CostoPeriodoAnnoCorrente = costoPeriodo;
                    temp.CostoAnnoCorrente = costoAnnuo;
                    temp.TotProvv = temp.Promozioni + temp.Gratifiche + temp.Aumenti;

                    if (temp.OrganicoContabile > 0)
                    {
                        decimal _pOrg = (decimal)((decimal)temp.TotProvv / (decimal)temp.OrganicoContabile);
                        _pOrg *= 100;

                        temp.PercentualeSuOrganico = (int)Math.Round(_pOrg);
                    }
                    else
                    {
                        temp.PercentualeSuOrganico = 0;
                    }

                    temp.BudgetAnnoCorrente = dir.BUDGET;
                    temp.BudgetPeriodoCorrente = dir.BUDGET_PERIODO;
                    temp.DeltaSuCostoAnnoCorrente = temp.BudgetAnnoCorrente - temp.CostoAnnoCorrente;
                    temp.DeltaSuCostoPeriodo = temp.BudgetPeriodoCorrente - temp.CostoPeriodoAnnoCorrente;
                    temp.IsConsolidata = false; //this.BudgetDataController.DirezioneIsConsolidata(idCampagna, dir.ID_DIREZIONE);

                    temp.CostoPeriodoAnnoPrecedente = 0;
                    temp.BudgetAnnoPrecedente = 0;
                    temp.CostoAnnoPrecedente = 0;
                    temp.CostoRegime = 0;
                    temp.BudgetPeriodoAnnoPrecedente = 0;
                    temp.CostoRecuperoStraordinario = 0;
                    temp.DeltaSuCostoAnnoPrecedente = temp.BudgetAnnoPrecedente - temp.CostoAnnoPrecedente;
                    temp.Ordine = ordine;
                    temp.Visibled = temp.TotProvv > 0;

                    reportDirezioni.Add(temp);
                    ordine++;
                }
            }

            bool enableQIO = PoliticheRetributiveHelper.EnableVis(PolRetrChiaveEnum.BudgetQIO);
            bool enableRS = PoliticheRetributiveHelper.EnableVis(PolRetrChiaveEnum.BudgetRS);

            if (enableQIO == enableRS && idCampagnas.Count() > 1)
            {
                int idAreaRs = 0;
                using (IncentiviEntities db = new IncentiviEntities())
                {
                    idAreaRs = db.XR_PRV_AREA.FirstOrDefault(x => x.LV_ABIL.Contains(PoliticheRetributiveHelper.BUDGETRS_HRGA_SOTTO_FUNC)).ID_AREA;
                    var direzioni2 = this.BudgetDataController.GetDirezioniByIdArea(CommonHelper.GetCurrentUserMatricola(), idAreaRs, idCampagnas).Where(x => direzioni.Select(y => y.CODICE).Contains(x.CODICE));

                    var isProm = PoliticheRetributiveHelper.AnyOfProv(PoliticheRetributiveHelper.GetProvBySigla(db, PoliticheRetributiveHelper.SIGLA_PROMOZIONI));
                    var isAumento = PoliticheRetributiveHelper.AnyOfProv(PoliticheRetributiveHelper.GetProvBySigla(db, PoliticheRetributiveHelper.SIGLA_AUMENTI));
                    var isGratifica = PoliticheRetributiveHelper.AnyOfProv(PoliticheRetributiveHelper.GetProvBySigla(db, PoliticheRetributiveHelper.SIGLA_GRATIFICHE));

                    foreach (var dir in direzioni2)
                    {
                        var temp = reportDirezioni.FirstOrDefault(x => x.Codice == dir.CODICE);
                        //temp.OrganicoContabile += dir.ORGANICO_CONTABILE.GetValueOrDefault();
                        temp.OrganicoRipartizione += dir.ORGANICO_AD;
                        temp.Promozioni += this.BudgetDataController.GetSumProvvByDirezione(idCampagnas, dir.ID_DIREZIONE, isProm, null, false, anno);

                        temp.Aumenti += this.BudgetDataController.GetSumProvvByDirezione(idCampagnas, dir.ID_DIREZIONE, isAumento, null, false, anno);

                        temp.Gratifiche += this.BudgetDataController.GetSumProvvByDirezione(idCampagnas, dir.ID_DIREZIONE, isGratifica, null, false, anno);

                        decimal costoAnnuo = 0;
                        decimal costoPeriodo = 0;
                        this.BudgetDataController.GetCostoAnnuoPeriodoDirezione(dir.ID_DIREZIONE, idCampagnas, out costoAnnuo, out costoPeriodo);
                        temp.CostoPeriodoAnnoCorrente += costoPeriodo;
                        temp.CostoAnnoCorrente += costoAnnuo;
                        temp.TotProvv = temp.Promozioni + temp.Gratifiche + temp.Aumenti;

                        if (temp.OrganicoContabile > 0)
                        {
                            decimal _pOrg = (decimal)((decimal)temp.TotProvv / (decimal)temp.OrganicoContabile);
                            _pOrg *= 100;

                            temp.PercentualeSuOrganico = (int)Math.Round(_pOrg);
                        }
                        else
                        {
                            temp.PercentualeSuOrganico = 0;
                        }

                        temp.BudgetAnnoCorrente += dir.BUDGET;
                        temp.BudgetPeriodoCorrente += dir.BUDGET_PERIODO;
                        temp.DeltaSuCostoAnnoCorrente = temp.BudgetAnnoCorrente - temp.CostoAnnoCorrente;
                        temp.DeltaSuCostoPeriodo = temp.BudgetPeriodoCorrente - temp.CostoPeriodoAnnoCorrente;
                        temp.IsConsolidata = false; //this.BudgetDataController.DirezioneIsConsolidata(idCampagna, dir.ID_DIREZIONE);

                        temp.CostoPeriodoAnnoPrecedente = 0;
                        temp.BudgetAnnoPrecedente = 0;
                        temp.CostoAnnoPrecedente = 0;
                        temp.CostoRegime = 0;
                        temp.BudgetPeriodoAnnoPrecedente = 0;
                        temp.CostoRecuperoStraordinario = 0;
                        temp.DeltaSuCostoAnnoPrecedente = temp.BudgetAnnoPrecedente - temp.CostoAnnoPrecedente;
                        temp.Visibled = temp.TotProvv > 0;
                    }
                }
            }

            return reportDirezioni;
        }

        private List<RiepilogoCampagnaItem> GetReportDirezioniPromozioni(XR_PRV_AREA area, int idCampagna, List<XR_PRV_CAMPAGNA_DECORRENZA> decorrenze)
        {
            List<RiepilogoCampagnaItem> result = new List<RiepilogoCampagnaItem>();

            var direz = this.BudgetDataController.GetDirezioniByIdArea(CommonHelper.GetCurrentUserMatricola(), area.ID_AREA, idCampagna);

            if (direz != null && direz.Any())
            {
                var listaOrdinata = direz.OrderBy(w => w.Ordine.GetValueOrDefault());

                var isProm = PoliticheRetributiveHelper.AnyOfProv(PoliticheRetributiveHelper.GetProvBySigla(null, PoliticheRetributiveHelper.SIGLA_PROMOZIONI));
                var isAumento = PoliticheRetributiveHelper.AnyOfProv(PoliticheRetributiveHelper.GetProvBySigla(null, PoliticheRetributiveHelper.SIGLA_AUMENTI));
                var isGratifica = PoliticheRetributiveHelper.AnyOfProv(PoliticheRetributiveHelper.GetProvBySigla(null, PoliticheRetributiveHelper.SIGLA_GRATIFICHE));

                foreach (var direzione in listaOrdinata)
                {
                    RiepilogoCampagnaItem item = new RiepilogoCampagnaItem();

                    item.Decorrenze = new List<int>();
                    int promozioni = 0;
                    int consolidati = 0;

                    if (decorrenze != null && decorrenze.Any())
                    {
                        foreach (var dec in decorrenze)
                        {
                            promozioni = this.BudgetDataController.GetSumProvvByDirezione(idCampagna, direzione.ID_DIREZIONE, isProm,
                                                dec.DT_DECORRENZA);

                            item.Decorrenze.Add(promozioni);

                            consolidati = this.BudgetDataController.GetSumProvvConsolidatiByDirezione(idCampagna, direzione.ID_DIREZIONE, isProm,
                                                dec.DT_DECORRENZA);

                            item.Concordato = consolidati;
                        }
                    }

                    promozioni = this.BudgetDataController.GetSumProvvByDirezione(idCampagna, direzione.ID_DIREZIONE, isProm,
                                        null, true);

                    this.BudgetDataController.GetCostoAnnuoPeriodoDirezione(direzione.ID_DIREZIONE, idCampagna, out decimal costoAnnuo, out decimal costoPeriodo,
                        provvSelector: isProm
                        );
                    item.CostoAnnuo = costoAnnuo;
                    item.CostoPeriodo = costoPeriodo;

                    item.Decorrenze.Add(promozioni);

                    item.Richiesto = item.Decorrenze.Sum(w => w);
                    item.Concordato = 0;
                    item.Note = "";
                    item.Direzione = direzione.NOME;

                    result.Add(item);
                }
            }

            return result;
        }

        #endregion

        public ActionResult EsportaRiepiloProvv(string anni = null)
        {
            List<int> listAnni = new List<int>();
            if (!String.IsNullOrWhiteSpace(anni))
                listAnni.AddRange(anni.Split(',').Select(x => Convert.ToInt32(x)));

            IncentiviEntities db = new IncentiviEntities();

            int anno = DateTime.Now.Year;
            if (listAnni == null || listAnni.Count() == 0)
                listAnni.Add(anno);

            XLWorkbook workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Riepilogo");
            MemoryStream ms = new MemoryStream();

            var funcFilterAbilServizio = PoliticheRetributiveHelper.FuncFilterDirezione(db);
            var funcFilterAbilMatr = PoliticheRetributiveHelper.FuncFilterMatr(db);
            var funcFilterAreaPratica = PoliticheRetributiveHelper.FuncFilterAreaPratica();

            var notAnyNessuna = PoliticheRetributiveHelper.NotAnyOfProv();



            List<IGrouping<string, ReportResult>> list = null;
            var tmp = PoliticheRetributiveHelper.GetPratiche(db, false)
                        .Where(x => x.ID_CAMPAGNA > 2 &&
                                    x.DECORRENZA != null && listAnni.Contains(x.DECORRENZA.Value.Year))
                        .Where(notAnyNessuna);
            //.Where(x=> x.ID_CAMPAGNA>2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa))

            bool usePrvVariazioniFlag = HrisHelper.GetParametro<string>(HrisParam.PRetribPrvSuVariazioni) == "TRUE";
            if (!usePrvVariazioniFlag)
                list = tmp.Select(x => new ReportResult
                {
                    Prov = x.XR_PRV_PROV_EFFETTIVO.SIGLA,
                    Pratica = x
                }).GroupBy(y => y.Prov).OrderByDescending(z => z.Key).ToList();
            else
                list = tmp.Select(x => new ReportResult
                {
                    Prov = x.XR_PRV_DIPENDENTI_VARIAZIONI.FirstOrDefault(y => y.IND_PRV_EFFETTIVO.HasValue && y.IND_PRV_EFFETTIVO.Value).XR_PRV_PROV.SIGLA,
                    Pratica = x
                }).GroupBy(y => y.Prov).OrderByDescending(z => z.Key).ToList();


            List<XR_PRV_DIPENDENTI_VARIAZIONI> provvedimenti = new List<XR_PRV_DIPENDENTI_VARIAZIONI>();

            string strDec = "";
            foreach (var item in listAnni)
            {
                if (strDec != "") strDec += " - ";
                strDec += String.Format("DEC. {0} E CONTABILIZZATI NEL {1}", item, item + 1);
            }


            worksheet.Cell(1, 2).SetValue(String.Format("PROVVEDIMENTI GESTIONALI FORMALIZZATI CON {0}", strDec));
            worksheet.Range(1, 2, 1, 10).Merge();


            worksheet.Column(3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Column(4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int row = 2;
            foreach (var item in list)
            {
                row++;
                #region Scrittura intestazione
                worksheet.Row(row).Style.Font.Bold = true;
                worksheet.Row(row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Row(row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                worksheet.Cell(row, 1).SetValue("MT");
                worksheet.Cell(row, 2).SetValue("NOME");
                worksheet.Cell(row, 3).SetValue("PROVV.\r\nRICHIESTO");
                worksheet.Cell(row, 4).SetValue("DECORRENZA");
                if (item.Key == "PD")
                {
                    worksheet.Cell(row, 5).SetValue("DIFF. RAL");
                    worksheet.Cell(row, 6).SetValue("LV. A");
                    worksheet.Cell(row, 7).SetValue("CAT. A");
                }
                else
                {
                    worksheet.Cell(row, 5).SetValue("IMPORTO");
                }
                #endregion

                provvedimenti.Clear();

                foreach (var dip in item.OrderBy(x => x.Pratica.MATRICOLA))
                {
                    row++;
                    var prov = PoliticheRetributiveHelper.GetDipProvEffettivo(dip.Pratica);
                    string nominativo = dip.Pratica.SINTESI1.DES_COGNOMEPERS + " " + dip.Pratica.SINTESI1.DES_NOMEPERS;

                    worksheet.Cell(row, 1).SetValue(dip.Pratica.MATRICOLA);
                    worksheet.Cell(row, 2).SetValue(nominativo.ToUpper());
                    worksheet.Cell(row, 3).SetValue(item.Key);
                    worksheet.Cell(row, 4).SetValue(dip.Pratica.DECORRENZA.Value.ToString("MM/yyyy"));
                    if (prov != null)
                        worksheet.Cell(row, 5).SetValue(prov.DIFF_RAL).Style.NumberFormat.Format = "#,###,##0.00";

                    if (item.Key == "PD")
                    {
                        string catPrevista = "";
                        if (!String.IsNullOrWhiteSpace(dip.Pratica.CAT_RICHIESTA))
                            catPrevista = dip.Pratica.CAT_RICHIESTA;
                        else if (prov != null)
                            catPrevista = prov.CAT_PREVISTA;

                        if (prov != null)
                            worksheet.Cell(row, 6).SetValue(prov.LIV_PREVISTO);
                        worksheet.Cell(row, 7).SetValue(catPrevista);
                    }

                    provvedimenti.Add(prov);
                }

                row++;
                worksheet.Row(row).Style.Font.Bold = true;
                worksheet.Cell(row, 4).SetValue("TOTALE");
                worksheet.Cell(row, 5).SetValue(provvedimenti.Where(x => x != null).Sum(x => x.DIFF_RAL)).Style.NumberFormat.Format = "#,###,##0.00";

                row++;
                worksheet.Row(row).Style.Font.Bold = true;
                string textTot = "Totale ";
                switch (item.Key)
                {
                    case "PD":
                        textTot += "promozioni";
                        break;
                    case "MD":
                        textTot += "incrementi retributivi";
                        break;
                    case "GD":
                        textTot += "gratifiche";
                        break;
                    default:
                        break;
                }

                textTot += " n. " + provvedimenti.Count().ToString();
                worksheet.Cell(row, 2).SetValue(textTot);
                worksheet.Range(row, 2, row, 4).Merge();

            }

            worksheet.Columns().AdjustToContents();
            worksheet.Rows().AdjustToContents();
            workbook.SaveAs(ms);
            ms.Position = 0;
            string nomeFile = "Riepilogo provvedimenti";
            if (listAnni.Count() == 1)
                nomeFile += " anno " + listAnni.First().ToString();
            else
                nomeFile += " anni " + String.Join(" - ", listAnni);

            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = nomeFile + ".xlsx" };
        }

        public ActionResult EsportaRiepilogoCause(int idCamp, int idArea, int tipoProv, int? annoDec = null)
        {
            IncentiviEntities db = new IncentiviEntities();

            int anno = DateTime.Now.Year;
            if (annoDec.HasValue)
                anno = annoDec.Value;

            XLWorkbook workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Riepilogo");
            MemoryStream ms = new MemoryStream();

            var funcFilterAbilServizio = PoliticheRetributiveHelper.FuncFilterDirezione(db);
            var funcFilterAbilMatr = PoliticheRetributiveHelper.FuncFilterMatr(db);
            var funcFilterAreaPratica = PoliticheRetributiveHelper.FuncFilterAreaPratica();
            var funcFilterCampagne = PoliticheRetributiveHelper.FuncFilterCampagna();
            var funcFilterDir = PoliticheRetributiveHelper.FuncFilterDir(db);
            bool enableQIO = PoliticheRetributiveHelper.EnableVis(PolRetrChiaveEnum.BudgetQIO);
            bool enableRS = PoliticheRetributiveHelper.EnableVis(PolRetrChiaveEnum.BudgetRS);

            IQueryable<XR_PRV_DIPENDENTI> list = db.XR_PRV_DIPENDENTI
            .Where(funcFilterAreaPratica)
            .Where(funcFilterAbilServizio)
            .Where(funcFilterAbilMatr);

            list = list.Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa));

            if (idCamp > 0)
                list = list.Where(x => x.ID_CAMPAGNA == idCamp);
            else
            {
                var listCampagne = db.XR_PRV_CAMPAGNA.Where(funcFilterCampagne).Where(x => x.ID_CAMPAGNA > 2).Select(x => x.ID_CAMPAGNA).ToList();
                list = list.Where(x => x.ID_CAMPAGNA != null && listCampagne.Contains(x.ID_CAMPAGNA.Value));
            }

            if (idCamp > 0)
            {
                list = list.Where(x => x.XR_PRV_DIREZIONE.ID_AREA == idArea);
            }
            else
            {
                if (enableQIO != enableRS)
                    list = list.Where(x => x.XR_PRV_DIREZIONE.ID_AREA == idArea);
            }

            if (annoDec.HasValue)
                list = list.Where(x => x.DECORRENZA != null && x.DECORRENZA.Value.Year == anno);

            if (tipoProv == 1)
                list = list.Where(x => x.XR_PRV_DIPENDENTI_CAUSESERV.Any());
            else if (tipoProv == 2)
                list = list.Where(x => x.XR_PRV_DIPENDENTI_PROVVD.Any());
            else
                list = list.Where(x => false);

            string desArea = db.XR_PRV_AREA.FirstOrDefault(x => x.ID_AREA == idArea).NOME;

            int row = 2;
            string fileName = "";
            if (tipoProv == 1)
            {
                fileName = "Elenco Dipendenti con cause";
                worksheet.Cell(row, 2).SetValue("Elenco dipendenti con cause");
                worksheet.Range(row, 2, row, 4).Merge();
                foreach (var dir in list.GroupBy(x => x.XR_PRV_DIREZIONE.NOME))
                {
                    row += 2;
                    worksheet.Cell(row, 1).SetValue(dir.Key);
                    worksheet.Range(row, 1, row, 2).Merge();
                    row++;
                    int startRow = row;
                    worksheet.Cell(row, 1).SetValue("MT").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    worksheet.Cell(row, 2).SetValue("Nominativo").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    foreach (var dip in dir)
                    {
                        row++;
                        worksheet.Cell(row, 1).SetValue(dip.MATRICOLA);
                        worksheet.Cell(row, 2).SetValue((dip.SINTESI1.DES_COGNOMEPERS + " " + dip.SINTESI1.DES_NOMEPERS).TitleCase());
                    }

                    worksheet.Range(startRow, 1, row, 2).CreateTable("Cause" + dir.First().XR_PRV_DIREZIONE.CODICE);
                }
            }
            else if (tipoProv == 2)
            {
                fileName = "Elenco Dipendenti con provvedimenti disciplinari";
                worksheet.Cell(row, 2).SetValue("Elenco dipendenti con provvedimenti disciplinari");
                worksheet.Range(row, 2, row, 4).Merge();
                row += 2;
                int startRow = row;

                worksheet.Cell(row, 1).SetValue("MT").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 2).SetValue("Nominativo").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 3).SetValue("Direzione").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 4).SetValue("Tipo prov.").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 5).SetValue("Descrizione").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                worksheet.Cell(row, 6).SetValue("Anno").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                foreach (var dip in list.OrderBy(x => x.XR_PRV_DIREZIONE.NOME).ThenBy(x => x.MATRICOLA))
                {
                    string nominativo = (dip.SINTESI1.DES_COGNOMEPERS + " " + dip.SINTESI1.DES_NOMEPERS).TitleCase();
                    string direzione = dip.XR_PRV_DIREZIONE.NOME;

                    foreach (var provvd in dip.XR_PRV_DIPENDENTI_PROVVD.OrderBy(x => x.DT_DATE))
                    {
                        row++;
                        worksheet.Cell(row, 1).SetValue(dip.MATRICOLA);
                        worksheet.Cell(row, 2).SetValue(nominativo);
                        worksheet.Cell(row, 3).SetValue(direzione);
                        worksheet.Cell(row, 4).SetValue(provvd.NOT_PROVVEDIMENTO);

                        string descrizione = (provvd.NOT_TESTO1 ?? "") + " " + (provvd.NOT_TESTO2 ?? "");
                        worksheet.Cell(row, 5).SetValue(descrizione);

                        if (provvd.DT_DATE.HasValue)
                            worksheet.Cell(row, 6).SetValue(provvd.DT_DATE.Value.Year);
                    }
                }

                worksheet.Range(startRow, 1, row, 6).CreateTable("ElencoDipProvvD");
            }

            fileName += " " + desArea;

            worksheet.Columns().AdjustToContents();
            worksheet.Rows().AdjustToContents();
            workbook.SaveAs(ms);
            ms.Position = 0;
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = fileName + ".xlsx" };
        }

        public ActionResult ReportProvvedimentiLiv()
        {
            XLWorkbook workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Riepilogo");
            var worksheetPivot = workbook.Worksheets.Add("Riepilogo Pivot");
            MemoryStream ms = new MemoryStream();

            IncentiviEntities db = new IncentiviEntities();
            var listDir = db.XR_PRV_DIREZIONE.Where(x => !x.XR_PRV_AREA.NOME.Contains("CHIAVE"));//.OrderBy(x=>x.XR_PRV_AREA.ORDINE).ThenBy(x=>x.ORDINE);

            var anyLivello = PoliticheRetributiveHelper.AnyOfProv(PoliticheRetributiveHelper.GetProvBySigla(db, PoliticheRetributiveHelper.SIGLA_PROMOZIONI));

            var list = PoliticheRetributiveHelper.GetPratiche(db, false, "XR_PRV_DIREZIONE")
                .Where(x => x.ID_CAMPAGNA > 2)
                .Where(anyLivello);

            list = list.Where(x => x.ID_CAMPAGNA > 2 || 
                                    !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa));

            int row = 1;

            worksheet.Cell(row, 1).SetValue("Area");
            worksheet.Cell(row, 2).SetValue("Direzione");
            worksheet.Cell(row, 3).SetValue("Matricola");
            worksheet.Cell(row, 4).SetValue("Nominativo");
            worksheet.Cell(row, 5).SetValue("Liv.Orig.");
            worksheet.Cell(row, 6).SetValue("Cat.Orig.");
            worksheet.Cell(row, 7).SetValue("Liv.Dest.");
            worksheet.Cell(row, 8).SetValue("Cat.Dest.");
            worksheet.Cell(row, 9).SetValue("Assorb.");

            foreach (var dir in listDir)
            {
                string areaNome = dir.XR_PRV_AREA.NOME;
                string dirNome = dir.CODICE + " - " + dir.NOME;
                foreach (var dip in list.Where(x => x.XR_PRV_DIREZIONE.CODICE == dir.CODICE).OrderBy(x => x.MATRICOLA))
                {
                    row++;
                    var prov = PoliticheRetributiveHelper.GetDipProvEffettivo(dip);

                    worksheet.Cell(row, 1).SetValue(areaNome);
                    worksheet.Cell(row, 2).SetValue(dirNome);
                    worksheet.Cell(row, 3).SetValue(dip.MATRICOLA);
                    worksheet.Cell(row, 4).SetValue(dip.SINTESI1.Nominativo());
                    worksheet.Cell(row, 5).SetValue(prov.LIV_ATTUALE);
                    worksheet.Cell(row, 6).SetValue(dip.SINTESI1.COD_QUALIFICA);
                    worksheet.Cell(row, 7).SetValue(prov.LIV_PREVISTO);
                    if (!String.IsNullOrWhiteSpace(dip.CAT_RICHIESTA))
                        worksheet.Cell(row, 8).SetValue(dip.CAT_RICHIESTA);
                    else
                        worksheet.Cell(row, 8).SetValue(prov.CAT_PREVISTA);
                    worksheet.Cell(row, 9).SetValue(prov.ID_PROV == (int)ProvvedimentiEnum.AumentoLivello || dip.ID_PROV_EFFETTIVO == (int)ProvvedimentiEnum.CUSAumentoLivello ? "S" : "N");
                }
            }

            var table = worksheet.Range(1, 1, row, 9).CreateTable("RiepilogoCasistiche");

            var pivot = worksheetPivot.PivotTables.Add("PivotTable", worksheetPivot.Cell(1, 1), table.AsRange());
            pivot.RowLabels.Add("Area");
            pivot.RowLabels.Add("Direzione");
            pivot.RowLabels.Add("Liv.Orig.");
            pivot.RowLabels.Add("Liv.Dest.");

            pivot.Values.Add("Matricola");
            pivot.Values.First().SummaryFormula = XLPivotSummary.Count;


            string fileName = "Report aumento di livello";
            worksheet.Columns().AdjustToContents();
            worksheet.Rows().AdjustToContents();
            workbook.SaveAs(ms);
            ms.Position = 0;
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = fileName + ".xlsx" };
        }

        public ActionResult ReportLettere(string provs, string piani, string aree, int esito = 1)
        {
            List<int> provvList = new List<int>();
            if (!String.IsNullOrWhiteSpace(provs))
                provvList.AddRange(provs.Split(',').Select(x => Convert.ToInt32(x)));

            List<int> pianiList = new List<int>();
            if (!String.IsNullOrWhiteSpace(piani))
                pianiList.AddRange(piani.Split(',').Select(x => Convert.ToInt32(x)));

            List<int> areeList = new List<int>();
            if (!String.IsNullOrWhiteSpace(aree))
                areeList.AddRange(aree.Split(',').Select(x => Convert.ToInt32(x)));

            IncentiviEntities db = new IncentiviEntities();
            XLWorkbook workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Riepilogo");
            MemoryStream ms = new MemoryStream();

            var notAnyNessuna = PoliticheRetributiveHelper.NotAnyOfProv();
            var tmp = PoliticheRetributiveHelper.GetPratiche(db, false)
                        .Where(x => x.ID_CAMPAGNA > 2)
                        .Where(notAnyNessuna);

            tmp = tmp.Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa));

            ProvvStatoEnum provvStato = ProvvStatoEnum.Consegnato;

            tmp = tmp.Where(x => x.XR_PRV_OPERSTATI.Where(y => y.DATA_FINE_VALIDITA == null).Max(z => z.ID_STATO) == (int)provvStato);

            switch (esito)
            {
                case 1:
                    tmp = tmp.Where(x => x.STATO_LETTERA.Value == 1);
                    break;
                case 2:
                    tmp = tmp.Where(x => x.STATO_LETTERA.Value == 2);
                    break;
                case 3:
                    tmp = tmp.Where(x => x.STATO_LETTERA.Value == 3);
                    break;
                default:
                    break;
            }

            if (provs != null && provs.Count() > 0)
            {
                var anyOfThisProv = PoliticheRetributiveHelper.AnyOfProv(PoliticheRetributiveHelper.GetProvByNumber(db, provvList));
                tmp = tmp.Where(anyOfThisProv);
            }

            if (pianiList != null && pianiList.Count() > 0)
            {
                tmp = tmp.Where(x => x.ID_CAMPAGNA != null && pianiList.Contains(x.ID_CAMPAGNA.Value));
            }

            if (areeList != null && areeList.Count() > 0)
            {
                tmp = tmp.Where(x => areeList.Contains(x.XR_PRV_DIREZIONE.ID_AREA));
            }

            bool usePrvVariazioniFlag = HrisHelper.GetParametro<string>(HrisParam.PRetribPrvSuVariazioni) == "TRUE";

            List<ReportResult> list = null;
            if (!usePrvVariazioniFlag)
                list = tmp.Select(x => new ReportResult
                {
                    Prov = x.XR_PRV_PROV_EFFETTIVO.SIGLA,
                    Pratica = x
                })
               .OrderBy(z => z.Pratica.MATRICOLA).ToList();
            else
                list = tmp.Select(x => new ReportResult
                {
                    Prov = x.XR_PRV_DIPENDENTI_VARIAZIONI.FirstOrDefault(y => y.IND_PRV_EFFETTIVO.HasValue && y.IND_PRV_EFFETTIVO.Value).XR_PRV_PROV.SIGLA,
                    Pratica = x
                })
                .OrderBy(z => z.Pratica.MATRICOLA).ToList(); ;

            List<XR_PRV_DIPENDENTI_VARIAZIONI> provvedimenti = new List<XR_PRV_DIPENDENTI_VARIAZIONI>();

            worksheet.Cell(1, 2).SetValue("LETTERE CONSEGNATE");
            worksheet.Range(1, 2, 1, 10).Merge();

            worksheet.Column(3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Column(4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int row = 2;

            #region Scrittura intestazione
            worksheet.Row(row).Style.Font.Bold = true;
            worksheet.Row(row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Row(row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            worksheet.Cell(row, 1).SetValue("MT");
            worksheet.Cell(row, 2).SetValue("NOME");
            worksheet.Cell(row, 3).SetValue("CAMPAGNA");
            worksheet.Cell(row, 4).SetValue("DIREZIONE");
            worksheet.Cell(row, 5).SetValue("PROVV.\r\nRICHIESTO");
            worksheet.Cell(row, 6).SetValue("DECORRENZA");
            worksheet.Cell(row, 7).SetValue("IMPORTO");
            worksheet.Cell(row, 8).SetValue("COSTO DI\r\nPERIODO");
            worksheet.Cell(row, 9).SetValue("COSTO\r\nANNUO");
            worksheet.Cell(row, 10).SetValue("LV. DA");
            worksheet.Cell(row, 11).SetValue("LV. A");
            worksheet.Cell(row, 12).SetValue("CAT. DA");
            worksheet.Cell(row, 13).SetValue("CAT. A");
            worksheet.Cell(row, 14).SetValue("MANSIONE");
            worksheet.Cell(row, 15).SetValue("PROFILO PROF.");
            #endregion

            var listIdPersona = list.Where(x => x.Prov == "PD").Select(x => x.Pratica.ID_PERSONA);
            var listAssQual = db.ASSQUAL.Where(x => listIdPersona.Contains(x.ID_PERSONA)).ToList();

            foreach (var dip in list)
            {
                row++;

                var prov = PoliticheRetributiveHelper.GetDipProvEffettivo(dip.Pratica);
                string nominativo = dip.Pratica.SINTESI1.DES_COGNOMEPERS + " " + dip.Pratica.SINTESI1.DES_NOMEPERS;

                worksheet.Cell(row, 1).SetValue(dip.Pratica.MATRICOLA);
                worksheet.Cell(row, 2).SetValue(nominativo.ToUpper());
                worksheet.Cell(row, 3).SetValue(dip.Pratica.XR_PRV_CAMPAGNA.NOME.Trim());
                worksheet.Cell(row, 4).SetValue(dip.Pratica.XR_PRV_DIREZIONE.NOME.Trim());
                worksheet.Cell(row, 5).SetValue(dip.Prov);
                worksheet.Cell(row, 6).SetValue(dip.Pratica.DECORRENZA.HasValue ? dip.Pratica.DECORRENZA.Value.ToString("dd/MM/yyyy") : "");
                worksheet.Cell(row, 7).SetValue(prov.DIFF_RAL).Style.NumberFormat.Format = "#,###,##0.00";
                worksheet.Cell(row, 8).SetValue(prov.COSTO_PERIODO).Style.NumberFormat.Format = "#,###,##0.00";
                worksheet.Cell(row, 9).SetValue(prov.COSTO_ANNUO).Style.NumberFormat.Format = "#,###,##0.00";
                if (dip.Prov == "PD")
                {
                    string catPrec = "";
                    if (dip.Pratica.DECORRENZA.HasValue)
                    {
                        DateTime rif = dip.Pratica.DECORRENZA.Value.AddDays(-1);
                        ASSQUAL precAssqual = listAssQual.FirstOrDefault(x => x.ID_PERSONA == dip.Pratica.ID_PERSONA && x.DTA_INIZIO <= rif && rif <= x.DTA_FINE);
                        if (rif != null)
                            catPrec = precAssqual.COD_QUALIFICA;
                    }

                    string catPrevista = "";
                    if (!String.IsNullOrWhiteSpace(dip.Pratica.CAT_RICHIESTA))
                        catPrevista = dip.Pratica.CAT_RICHIESTA;
                    else
                        catPrevista = prov.CAT_PREVISTA;

                    worksheet.Cell(row, 10).SetValue(prov.LIV_ATTUALE);
                    worksheet.Cell(row, 11).SetValue(prov.LIV_PREVISTO);
                    worksheet.Cell(row, 12).SetValue(catPrec);
                    worksheet.Cell(row, 13).SetValue(catPrevista);
                    worksheet.Cell(row, 14).SetValue(!String.IsNullOrWhiteSpace(dip.Pratica.COD_MANSIONE) ? dip.Pratica.COD_MANSIONE : "");
                }
                else
                {
                    worksheet.Cell(row, 10).SetValue("");
                    worksheet.Cell(row, 11).SetValue("");
                    worksheet.Cell(row, 12).SetValue("");
                    worksheet.Cell(row, 13).SetValue("");
                    worksheet.Cell(row, 14).SetValue("");
                }

                string profiloProf = "";

                try
                {
                    string COD_RUOLO = dip.Pratica.SINTESI1.COD_RUOLO;
                    string DES_RUOLO = dip.Pratica.SINTESI1.DES_RUOLO;
                    DES_RUOLO = DES_RUOLO.Replace(COD_RUOLO, "");
                    DES_RUOLO = DES_RUOLO.Replace("-", "");
                    DES_RUOLO = DES_RUOLO.Trim();
                    profiloProf = DES_RUOLO;
                }
                catch(Exception ex)
                {
                    profiloProf = "";
                }
                
                worksheet.Cell(row, 15).SetValue(profiloProf);
            }

            worksheet.Columns().AdjustToContents();
            worksheet.Rows().AdjustToContents();
            workbook.SaveAs(ms);
            ms.Position = 0;
            string nomeFile = "Riepilogo lettere consegnate";

            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = nomeFile + ".xlsx" };
        }

        public ActionResult ReportGestioneManuale(string provs, int esito = 1)
        {
            List<int> provvList = new List<int>();
            if (!String.IsNullOrWhiteSpace(provs))
                provvList.AddRange(provs.Split(',').Select(x => Convert.ToInt32(x)));

            //string nomeFile = "";
            //MemoryStream outputMemStream = PoliticheRetributiveManager.CreaRiepilogoProvv(esito, provvedimenti);
            //return new FileStreamResult(outputMemStream, "application/pdf") { FileDownloadName = nomeFile };


            IncentiviEntities db = new IncentiviEntities();
            XLWorkbook workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Riepilogo");
            MemoryStream ms = new MemoryStream();

            var notAnyNessuna = PoliticheRetributiveHelper.NotAnyOfProv();
            var tmp = PoliticheRetributiveHelper.GetPratiche(db, false)
                        .Where(x => x.ID_CAMPAGNA > 2)
                        .Where(notAnyNessuna)
                        .Where(x => x.IND_PRATICA_EXT != null && x.IND_PRATICA_EXT.Value);

            tmp = tmp.Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa));

            if (provs != null && provs.Count() > 0)
            {
                var anyOfThisProv = PoliticheRetributiveHelper.AnyOfProv(PoliticheRetributiveHelper.GetProvByNumber(db, provvList));
                tmp = tmp.Where(anyOfThisProv);
            }

            var list = tmp.Select(x => new
            {
                Prov = x.XR_PRV_PROV_EFFETTIVO.SIGLA,
                Pratica = x
            })
                        .OrderBy(z => z.Pratica.MATRICOLA).ToList();

            List<XR_PRV_DIPENDENTI_VARIAZIONI> provvedimenti = new List<XR_PRV_DIPENDENTI_VARIAZIONI>();


            worksheet.Cell(1, 2).SetValue("LETTERE CONSEGNATE");
            worksheet.Range(1, 2, 1, 10).Merge();


            worksheet.Column(3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Column(4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            int row = 2;

            #region Scrittura intestazione
            worksheet.Row(row).Style.Font.Bold = true;
            worksheet.Row(row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Row(row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            worksheet.Cell(row, 1).SetValue("MT");
            worksheet.Cell(row, 2).SetValue("NOME");
            worksheet.Cell(row, 3).SetValue("PROVV.\r\nRICHIESTO");
            worksheet.Cell(row, 4).SetValue("DECORRENZA");
            worksheet.Cell(row, 5).SetValue("IMPORTO");
            worksheet.Cell(row, 6).SetValue("COSTO DI\r\nPERIODO");
            worksheet.Cell(row, 7).SetValue("COSTO\r\nANNUO");
            worksheet.Cell(row, 8).SetValue("LV. DA");
            worksheet.Cell(row, 9).SetValue("LV. A");
            worksheet.Cell(row, 10).SetValue("CAT. DA");
            worksheet.Cell(row, 11).SetValue("CAT. A");
            worksheet.Cell(row, 12).SetValue("MANSIONE");
            #endregion

            var listIdPersona = list.Where(x => x.Prov == "PD").Select(x => x.Pratica.ID_PERSONA);
            var listAssQual = db.ASSQUAL.Where(x => listIdPersona.Contains(x.ID_PERSONA)).ToList();

            foreach (var dip in list)
            {
                row++;
                var prov = PoliticheRetributiveHelper.GetDipProvEffettivo(dip.Pratica);
                string nominativo = dip.Pratica.SINTESI1.DES_COGNOMEPERS + " " + dip.Pratica.SINTESI1.DES_NOMEPERS;

                worksheet.Cell(row, 1).SetValue(dip.Pratica.MATRICOLA);
                worksheet.Cell(row, 2).SetValue(nominativo.ToUpper());
                worksheet.Cell(row, 3).SetValue(dip.Prov);
                worksheet.Cell(row, 4).SetValue(dip.Pratica.DECORRENZA.HasValue ? dip.Pratica.DECORRENZA.Value.ToString("MM/yyyy") : "");
                worksheet.Cell(row, 5).SetValue(prov.DIFF_RAL).Style.NumberFormat.Format = "#,###,##0.00";
                worksheet.Cell(row, 6).SetValue(prov.COSTO_PERIODO).Style.NumberFormat.Format = "#,###,##0.00";
                worksheet.Cell(row, 7).SetValue(prov.COSTO_ANNUO).Style.NumberFormat.Format = "#,###,##0.00";
                if (dip.Prov == "PD")
                {
                    string catPrec = "";
                    if (dip.Pratica.DECORRENZA.HasValue)
                    {
                        DateTime rif = dip.Pratica.DECORRENZA.Value.AddDays(-1);
                        ASSQUAL precAssqual = listAssQual.FirstOrDefault(x => x.ID_PERSONA == dip.Pratica.ID_PERSONA && x.DTA_INIZIO <= rif && rif <= x.DTA_FINE);
                        if (rif != null)
                            catPrec = precAssqual.COD_QUALIFICA;
                    }

                    string catPrevista = "";
                    if (!String.IsNullOrWhiteSpace(dip.Pratica.CAT_RICHIESTA))
                        catPrevista = dip.Pratica.CAT_RICHIESTA;
                    else
                        catPrevista = prov.CAT_PREVISTA;

                    worksheet.Cell(row, 8).SetValue(prov.LIV_ATTUALE);
                    worksheet.Cell(row, 9).SetValue(prov.LIV_PREVISTO);
                    worksheet.Cell(row, 10).SetValue(catPrec);
                    worksheet.Cell(row, 11).SetValue(catPrevista);
                    worksheet.Cell(row, 12).SetValue(!String.IsNullOrWhiteSpace(dip.Pratica.COD_MANSIONE) ? dip.Pratica.COD_MANSIONE : "");
                }
                else
                {
                    worksheet.Cell(row, 8).SetValue("");
                    worksheet.Cell(row, 9).SetValue("");
                    worksheet.Cell(row, 10).SetValue("");
                    worksheet.Cell(row, 11).SetValue("");
                    worksheet.Cell(row, 11).SetValue("");
                }
            }

            worksheet.Columns().AdjustToContents();
            worksheet.Rows().AdjustToContents();
            workbook.SaveAs(ms);
            ms.Position = 0;
            string nomeFile = "Riepilogo gestione manuale";

            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = nomeFile + ".xlsx" };
        }

        public ActionResult ReportPassaggiCategoria()
        {
            IncentiviEntities db = new IncentiviEntities();
            XLWorkbook workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Riepilogo");
            MemoryStream ms = new MemoryStream();

            var notAnyNessuna = PoliticheRetributiveHelper.NotAnyOfProv();
            var tmp = PoliticheRetributiveHelper.GetPratiche(db, false, "SINTESI1")
                        .Where(x => x.ID_CAMPAGNA > 2)
                        .Where(notAnyNessuna)
                        .Where(x => x.IND_PRATICA_EXT == null || !x.IND_PRATICA_EXT.Value);

            tmp = tmp.Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa));

            var anyOfThisProv = PoliticheRetributiveHelper.AnyOfProv(PoliticheRetributiveHelper.GetProvBySigla(db, PoliticheRetributiveHelper.SIGLA_PROMOZIONI));
            tmp = tmp.Where(anyOfThisProv);

            var list = tmp.OrderBy(x => x.XR_PRV_DIREZIONE.NOME).ThenBy(x => x.MATRICOLA).ToList();

            int row = 1;

            #region Scrittura intestazione
            worksheet.Row(row).Style.Font.Bold = true;
            worksheet.Row(row).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Row(row).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            worksheet.Cell(row, 1).SetValue("Sede");
            worksheet.Cell(row, 2).SetValue("Matricola");
            worksheet.Cell(row, 3).SetValue("Nominativo");
            worksheet.Cell(row, 4).SetValue("Direzione");
            worksheet.Cell(row, 5).SetValue("Decorrenza");
            worksheet.Cell(row, 6).SetValue("Nuova categoria");
            #endregion

            foreach (var item in list)
            {
                row++;
                worksheet.Cell(row, 1).SetValue(item.SINTESI1.DES_SEDE);
                worksheet.Cell(row, 2).SetValue(item.MATRICOLA);
                worksheet.Cell(row, 3).SetValue(item.SINTESI1.Nominativo());
                worksheet.Cell(row, 4).SetValue(item.XR_PRV_DIREZIONE.NOME);
                if (item.DECORRENZA.HasValue)
                    worksheet.Cell(row, 5).SetValue(String.Format("{0:dd/MM/yyyy}", item.DECORRENZA));

                string catPrevista = "";
                if (!String.IsNullOrWhiteSpace(item.CAT_RICHIESTA))
                    catPrevista = item.CAT_RICHIESTA;
                else
                {
                    var prov = PoliticheRetributiveHelper.GetDipProvEffettivo(item);
                    catPrevista = prov.CAT_PREVISTA;
                }

                worksheet.Cell(row, 6).SetValue(catPrevista);
            }

            worksheet.Range(1, 1, row, 6).CreateTable("PassaggiCategoria");

            worksheet.Columns().AdjustToContents();
            worksheet.Rows().AdjustToContents();
            workbook.SaveAs(ms);
            ms.Position = 0;
            string nomeFile = "Riepilogo promozioni";

            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = nomeFile + ".xlsx" };
        }

        public ActionResult ReportNumeri()
        {
            IncentiviEntities db = new IncentiviEntities();
            XLWorkbook workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Riepilogo");
            MemoryStream ms = new MemoryStream();

            var notAnyNessuna = PoliticheRetributiveHelper.NotAnyOfProv();
            var tmp = PoliticheRetributiveHelper.GetPratiche(db, false, "SINTESI1")
                        .Where(x => x.ID_CAMPAGNA > 2)
                        .Where(notAnyNessuna)
                        .Where(x => x.IND_PRATICA_EXT == null || !x.IND_PRATICA_EXT.Value);

            tmp = tmp.Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa));

            List<int> decodProvs = new List<int>();
            decodProvs.AddRange(PoliticheRetributiveHelper.GetProvBySigla(db, PoliticheRetributiveHelper.SIGLA_PROMOZIONI));
            decodProvs.AddRange(PoliticheRetributiveHelper.GetProvBySigla(db, PoliticheRetributiveHelper.SIGLA_AUMENTI));
            var anyOfThisProv = PoliticheRetributiveHelper.AnyOfProv(decodProvs.ToArray());
            tmp = tmp.Where(anyOfThisProv);


            List<ReportResult> list = null;
            bool usePrvVariazioniFlag = HrisHelper.GetParametro<string>(HrisParam.PRetribPrvSuVariazioni) == "TRUE";
            if (!usePrvVariazioniFlag)
                list = tmp.Select(x => new ReportResult
                {
                    Provv = x.ID_PROV_EFFETTIVO == (int)ProvvedimentiEnum.AumentoMerito || x.ID_PROV_EFFETTIVO == (int)ProvvedimentiEnum.CUSAumentoMerito ? "MD" : "PD",
                    Area = x.XR_PRV_DIREZIONE.XR_PRV_AREA.NOME,
                    //Pratica = x
                }).ToList();
            else
                list = tmp.Select(x => new ReportResult
                {
                    Provv = x.XR_PRV_DIPENDENTI_VARIAZIONI.FirstOrDefault(y => y.IND_PRV_EFFETTIVO.HasValue && y.IND_PRV_EFFETTIVO.Value).XR_PRV_PROV.SIGLA,
                    Area = x.XR_PRV_DIREZIONE.XR_PRV_AREA.NOME,
                    //Pratica = x
                }).ToList();

            int row = 3;

            foreach (var area in list.GroupBy(x => x.Area))
            {
                worksheet.Range(row, 1, row + 1, 1).Merge();
                worksheet.Cell(row, 1).SetValue(area.Key);

                worksheet.Range(row, 2, row + 1, 2).Merge();
                worksheet.Cell(row, 2).SetValue(area.Count()).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                worksheet.Cell(row, 3).SetValue("Meriti");
                worksheet.Cell(row + 1, 3).SetValue("Promozioni");
                worksheet.Cell(row, 4).SetValue(area.Count(x => x.Provv == "MD"));
                worksheet.Cell(row + 1, 4).SetValue(area.Count(x => x.Provv == "PD"));

                worksheet.Range(row, 1, row + 1, 4).Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                worksheet.Range(row, 1, row + 1, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;

                row += 2;
            }



            worksheet.Cell(row, 1).SetValue("Totale");
            worksheet.Cell(row, 2).SetValue(list.Count());
            worksheet.Range(row, 1, row, 4).Style.Border.OutsideBorder = XLBorderStyleValues.Medium;

            worksheet.Range(3, 1, row, 4).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.Columns().AdjustToContents();
            worksheet.Column(1).Width = 25;

            worksheet.Rows().Height = 20;

            workbook.SaveAs(ms);
            ms.Position = 0;
            string nomeFile = "Conteggio provvedimenti";

            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = nomeFile + ".xlsx" };
        }

        public ActionResult ReportProvRal(string idCamp, string tipoProv)
        {
            var campagne = idCamp.Split(',').Select(x => Convert.ToInt32(x));
            var elencoProv = tipoProv.Split(',');

            XLWorkbook workbook = new XLWorkbook();

            //string desProv = "";
            //switch (tipoProv)
            //{
            //    case PoliticheRetributiveHelper.SIGLA_PROMOZIONI:
            //        desProv = "promozioni";
            //        break;
            //    case PoliticheRetributiveHelper.SIGLA_AUMENTI:
            //        desProv = "aumenti";
            //        break;
            //    case PoliticheRetributiveHelper.SIGLA_GRATIFICHE:
            //        desProv = "gratifiche";
            //        break;
            //    default:
            //        break;
            //}

            var ws = workbook.Worksheets.Add("Riepilogo");
            var wsPivot = workbook.Worksheets.Add("Riepilogo RAL");
            var wsCount = workbook.Worksheets.Add("Riepilogo provvedimenti");
            var wsDirGen = workbook.Worksheets.Add("Riepilogo direzioni-genere");
            var wsPianoGen = workbook.Worksheets.Add("Riepilogo piano-genere");
            MemoryStream ms = new MemoryStream();

            var db = new IncentiviEntities();
            var isProv = PoliticheRetributiveHelper.AnyOfProv(PoliticheRetributiveHelper.GetProvBySigla(db, elencoProv));
            
            var list = db.XR_PRV_DIPENDENTI
                            .Include("SINTESI1")
                            .Include("XR_PRV_CAMPAGNA")
                            .Include("XR_PRV_DIREZIONE")
                            .Include("XR_PRV_DIREZIONE.XR_PRV_AREA")
                            .Include("XR_PRV_DIPENDENTI_RAL")
                            .Include("XR_PRV_DIPENDENTI_VARIAZIONI")
                            .Include("XR_PRV_DIPENDENTI_VARIAZIONI.XR_PRV_PROV")
                            .Where(x => campagne.Contains(x.ID_CAMPAGNA.Value))
                            .Where(isProv)
                            .OrderBy(x=>x.XR_PRV_CAMPAGNA.DTA_INIZIO)
                            .ThenBy(x=>x.XR_PRV_CAMPAGNA.NOME)
                            .ThenBy(x=>x.XR_PRV_DIREZIONE.XR_PRV_AREA.ORDINE)
                            .ThenBy(x=>x.XR_PRV_DIREZIONE.ORDINE)
                            .ThenBy(x=>x.MATRICOLA);

            int row = 1;

            ws.Cell(row, 1).SetValue("Piano");
            ws.Cell(row, 2).SetValue("Area");
            ws.Cell(row, 3).SetValue("Direzione");
            ws.Cell(row, 4).SetValue("Matricola");
            ws.Cell(row, 5).SetValue("Nominativo");
            ws.Cell(row, 6).SetValue("Sesso");
            ws.Cell(row, 7).SetValue("Sede");
            ws.Cell(row, 8).SetValue("Decorrenza richiesta");
            ws.Cell(row, 9).SetValue("Tipo provvedimento");
            ws.Cell(row, 10).SetValue("Categoria");
            ws.Cell(row, 11).SetValue("RAL");
            ws.Cell(row, 12).SetValue("Incremento RAL");
            ws.Cell(row, 13).SetValue("Costo annuo");
            ws.Cell(row, 14).SetValue("Costo periodo");

            foreach (var item in list)
            {                
                var prov = PoliticheRetributiveHelper.GetDipProvEffettivo(item);
                if (prov == null)
                {
                    // se tra i provvedimenti non è stato chiesto 
                    // NESSUN PROVVEDIMENTO, allora scarta l'elemento
                    if (!elencoProv.Contains(""))
                    {
                        continue;
                    }

                    // se tra i provvedimenti è stato chiesto 
                    // NESSUN PROVVEDIMENTO, allora crea un elemento fittizio per 
                    // poter continuare l'elaborazione
                    prov = new XR_PRV_DIPENDENTI_VARIAZIONI()
                    {
                        XR_PRV_PROV = new XR_PRV_PROV()
                        {
                            SIGLA = PoliticheRetributiveHelper.SIGLA_NESSUNO,
                            NOME = "Nessuno"
                        },
                        CAT_PREVISTA = item.SINTESI1.COD_QUALIFICA,
                        COSTO_ANNUO = 0,
                        COSTO_PERIODO = 0
                    };

                }

                row++;
                ws.Cell(row, 1).SetValue(item.XR_PRV_CAMPAGNA.NOME.Trim());
                ws.Cell(row, 2).SetValue(item.XR_PRV_DIREZIONE.XR_PRV_AREA.NOME.Trim());
                ws.Cell(row, 3).SetValue(item.XR_PRV_DIREZIONE.NOME.Trim());
                ws.Cell(row, 4).SetValue(item.MATRICOLA);
                ws.Cell(row, 5).SetValue(item.SINTESI1.Nominativo());
                ws.Cell(row, 6).SetValue(item.SINTESI1.COD_SESSO);
                ws.Cell(row, 7).SetValue(item.SINTESI1.DES_SEDE);
                ws.Cell(row, 8).SetValue(item.DECORRENZA);
                
                string qual = "";
                if (prov.XR_PRV_PROV.SIGLA == PoliticheRetributiveHelper.SIGLA_PROMOZIONI)
                {
                    if (!String.IsNullOrWhiteSpace(item.CAT_RICHIESTA))
                        qual = item.CAT_RICHIESTA;
                    else
                        qual = prov.CAT_PREVISTA;
                }

                if (String.IsNullOrEmpty(qual) || qual.ToUpper() == "UNDEFINED")
                {
                    // va caricata la qualifica giusta
                    var variazione = db.XR_PRV_DIPENDENTI_VARIAZIONI.Where(w => w.ID_DIPENDENTE == item.ID_DIPENDENTE 
                                        && w.ID_PROV == item.ID_PROV_EFFETTIVO).FirstOrDefault();

                    if (variazione != null)
                    {
                        item.CAT_RICHIESTA = variazione.CAT_PREVISTA;
                        qual = variazione.CAT_PREVISTA;
                    }
                }

                ws.Cell(row, 9).SetValue(prov.XR_PRV_PROV.NOME);
                ws.Cell(row, 10).SetValue(qual);

                decimal? ral = item.RAL_ATTUALE;
                if (!ral.HasValue)
                {
                    ral = item.XR_PRV_DIPENDENTI_RAL.OrderByDescending(w => w.DT_RAL).Select(x => x.IMPORTO).FirstOrDefault();
                }
                    
                //decimal? ral = item.XR_PRV_DIPENDENTI_RAL.OrderByDescending(w => w.DT_RAL).Select(x => x.IMPORTO).FirstOrDefault();
                if (!ral.HasValue)
                    ral = 0;
                ws.Cell(row, 11).SetValue(ral);
                //ws.Cell(row, 10).Style.NumberFormat.Format = "€ #,##0.00";
                //ws.Cell(row, 10).DataType = XLDataType.Number;

                var diffRal = item.XR_PRV_DIPENDENTI_VARIAZIONI.Where(w => w.ID_PROV == item.ID_PROV_EFFETTIVO).FirstOrDefault();
                if (diffRal != null)
                {
                    ws.Cell(row, 12).SetValue(diffRal.DIFF_RAL);
                }

                ws.Cell(row, 13).SetValue(prov.COSTO_ANNUO);
                //                ws.Cell(row, 11).SetValue(prov.COSTO_ANNUO);
                //ws.Cell(row, 11).Style.NumberFormat.Format = "€ #,##0.00";
                //ws.Cell(row, 11).DataType = XLDataType.Number;

                ws.Cell(row, 14).SetValue(prov.COSTO_PERIODO);
                //ws.Cell(row, 12).SetValue(prov.COSTO_PERIODO);
                //ws.Cell(row, 12).Style.NumberFormat.Format = "€ #,##0.00";
                //ws.Cell(row, 12).DataType = XLDataType.Number;
            }

            IXLRange range = null;

            //range = ws.Range(2, 11, row, 11);
            //range.Style.NumberFormat.Format = "€ #,##0.00";
            //range.DataType = XLDataType.Number;

            //range = ws.Range(2, 12, row, 12);
            //range.Style.NumberFormat.Format = "€ #,##0.00";
            //range.DataType = XLDataType.Number;

            for(int idx = 11; idx <=14; idx++)
            {
                range = ws.Range(2, idx, row, idx);
                range.Style.NumberFormat.Format = "€ #,##0.00";
                range.DataType = XLDataType.Number;
            }
            
            var table = ws.Range(1, 1, row, 14).CreateTable("TabellaProm");
            var pivotRAL = wsPivot.PivotTables.Add("PivotTable", wsPivot.Cell(1, 1), table.AsRange());
            //pivotRAL.SetAutofitColumns(true);
            pivotRAL.RowLabels.Add("Area").SetLayout(XLPivotLayout.Tabular).AddSubtotal(XLSubtotalFunction.Sum);
            pivotRAL.RowLabels.Add("Direzione").SetLayout(XLPivotLayout.Tabular).AddSubtotal(XLSubtotalFunction.Sum);
            pivotRAL.RowLabels.Add("Decorrenza richiesta").SetLayout(XLPivotLayout.Tabular).AddSubtotal(XLSubtotalFunction.Sum);
            pivotRAL.RowLabels.Add("Nominativo").SetLayout(XLPivotLayout.Tabular).AddSubtotal(XLSubtotalFunction.Sum);
            pivotRAL.Values.Add("RAL").SetSummaryFormula(XLPivotSummary.Sum).NumberFormat.Format = "€ #,##0.00";
            wsPivot.Column(1).Width = 30;
            wsPivot.Column(2).Width = 50;
            wsPivot.Column(3).Width = 50;
            wsPivot.Column(3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            wsPivot.Column(4).Width = 40;
            wsPivot.Column(5).Width = 30;

            var pivotCount = wsCount.PivotTables.Add("PivotCount", wsCount.Cell(1, 1), table.AsRange());
            //pivotCount.SetAutofitColumns(true);
            pivotCount.RowLabels.Add("Area").AddSubtotal(XLSubtotalFunction.Count);
            pivotCount.RowLabels.Add("Direzione").AddSubtotal(XLSubtotalFunction.Count);
            pivotCount.RowLabels.Add("Decorrenza richiesta");
            pivotCount.Values.Add("Matricola").SetSummaryFormula(XLPivotSummary.Count);
            wsCount.Column(1).Width = 50;

            var pivotDirGen = wsDirGen.PivotTables.Add("PivotDirGen", wsDirGen.Cell(1, 1), table.AsRange());
            pivotDirGen.RowLabels.Add("Direzione").AddSubtotal(XLSubtotalFunction.Count);
            pivotDirGen.RowLabels.Add("Tipo provvedimento");
            pivotDirGen.ColumnLabels.Add("Sesso");
            pivotDirGen.Values.Add("Matricola").SetSummaryFormula(XLPivotSummary.Count);
            wsDirGen.Column(1).Width = 60;
            wsDirGen.Column(4).Width = 50;

            var pivotPianoGen = wsPianoGen.PivotTables.Add("PivotPianoGen", wsPianoGen.Cell(1, 1), table.AsRange());
            pivotPianoGen.RowLabels.Add("Piano");
            pivotPianoGen.ColumnLabels.Add("Sesso");
            var pivotPianoGenMatr = pivotPianoGen.Values.Add("Matricola", "Dip");
            pivotPianoGenMatr.SetSummaryFormula(XLPivotSummary.Count);

            var pivotPianoGenMatrPerc = pivotPianoGen.Values.Add("Matricola", "%");
            pivotPianoGenMatrPerc.SetSummaryFormula(XLPivotSummary.Count);
            pivotPianoGenMatrPerc.ShowAsPercentageOfRow();
            pivotPianoGenMatrPerc.NumberFormat.NumberFormatId = 10;

            pivotPianoGen.Values.Add("Incremento RAL").SetSummaryFormula(XLPivotSummary.Sum).NumberFormat.Format = "€ #,##0.00";
            pivotPianoGen.Values.Add("Costo annuo").SetSummaryFormula(XLPivotSummary.Sum).NumberFormat.Format = "€ #,##0.00";
            pivotPianoGen.Values.Add("Costo periodo").SetSummaryFormula(XLPivotSummary.Sum).NumberFormat.Format = "€ #,##0.00";
            wsPianoGen.Column(1).Width = 60;
            wsPianoGen.Column(2).Width = 15;
            wsPianoGen.Column(3).Width = 15;
            wsPianoGen.Column(4).Width = 15;
            wsPianoGen.Column(5).Width = 15;
            wsPianoGen.Column(6).Width = 15;
            wsPianoGen.Column(7).Width = 50;
            wsPianoGen.Column(8).Width = 20;
            wsPianoGen.Column(9).Width = 20;
            wsPianoGen.Column(10).Width = 20;

            string fileName = "Report provvedimenti";
            ws.Columns().AdjustToContents();
            ws.Rows().AdjustToContents();
            
            workbook.SaveAs(ms);
            ms.Position = 0;
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = fileName + ".xlsx" };
        }

        public ActionResult EsportaRiepilogoStragiudiziale(int idCamp, int idArea, int tipoProv, int? annoDec = null)
        {
            IncentiviEntities db = new IncentiviEntities();

            int anno = DateTime.Now.Year;
            if (annoDec.HasValue)
                anno = annoDec.Value;

            XLWorkbook workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Riepilogo");
            MemoryStream ms = new MemoryStream();

            var funcFilterAbilServizio = PoliticheRetributiveHelper.FuncFilterDirezione(db);
            var funcFilterAbilMatr = PoliticheRetributiveHelper.FuncFilterMatr(db);
            var funcFilterAreaPratica = PoliticheRetributiveHelper.FuncFilterAreaPratica();
            var funcFilterCampagne = PoliticheRetributiveHelper.FuncFilterCampagna();
            var funcFilterDir = PoliticheRetributiveHelper.FuncFilterDir(db);
            bool enableQIO = PoliticheRetributiveHelper.EnableVis(PolRetrChiaveEnum.BudgetQIO);
            bool enableRS = PoliticheRetributiveHelper.EnableVis(PolRetrChiaveEnum.BudgetRS);

            IQueryable<XR_PRV_DIPENDENTI> list = db.XR_PRV_DIPENDENTI
            .Where(funcFilterAreaPratica)
            .Where(funcFilterAbilServizio)
            .Where(funcFilterAbilMatr);

            list = list.Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa));

            if (idCamp > 0)
                list = list.Where(x => x.ID_CAMPAGNA == idCamp);
            else
            {
                var listCampagne = db.XR_PRV_CAMPAGNA.Where(funcFilterCampagne).Where(x => x.ID_CAMPAGNA > 2).Select(x => x.ID_CAMPAGNA).ToList();
                list = list.Where(x => x.ID_CAMPAGNA != null && listCampagne.Contains(x.ID_CAMPAGNA.Value));
            }

            if (idArea > 0)
            {
                list = list.Where(x => x.XR_PRV_DIREZIONE.ID_AREA == idArea);
            }
            else
            {
                if (enableQIO != enableRS)
                    list = list.Where(x => x.XR_PRV_DIREZIONE.ID_AREA == idArea);
            }

            if (annoDec.HasValue)
                list = list.Where(x => x.DECORRENZA != null && x.DECORRENZA.Value.Year == anno);

            list = list.Where(x => x.XR_PRV_DIPENDENTI_STRAGIUDIZIALE.Any());
            string desArea = db.XR_PRV_AREA.FirstOrDefault(x => x.ID_AREA == idArea).NOME;

            int row = 2;
            string fileName = "";
            fileName = "Elenco Dipendenti con stragiudiziale";
            worksheet.Cell(row, 2).SetValue("Elenco dipendenti con stragiudiziale");
            worksheet.Range(row, 2, row, 4).Merge();
            row += 2;
            int startRow = row;

            worksheet.Cell(row, 1).SetValue("MT").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, 2).SetValue("Nominativo").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, 3).SetValue("Direzione").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, 4).SetValue("Numero dossier").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, 5).SetValue("Oggetto").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Cell(row, 6).SetValue("Anno").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            foreach (var dip in list.OrderBy(x => x.XR_PRV_DIREZIONE.NOME).ThenBy(x => x.MATRICOLA))
            {
                string nominativo = (dip.SINTESI1.DES_COGNOMEPERS + " " + dip.SINTESI1.DES_NOMEPERS).TitleCase();
                string direzione = dip.XR_PRV_DIREZIONE.NOME;

                foreach (var strad in dip.XR_PRV_DIPENDENTI_STRAGIUDIZIALE.OrderBy(x => x.DTA_CREAZIONE))
                {
                    row++;
                    worksheet.Cell(row, 1).SetValue(dip.MATRICOLA);
                    worksheet.Cell(row, 2).SetValue(nominativo);
                    worksheet.Cell(row, 3).SetValue(direzione);
                    worksheet.Cell(row, 4).SetValue(strad.NUMERO_DOSSIER);
                    worksheet.Cell(row, 5).SetValue(strad.OGGETTO);

                    if (strad.DTA_CREAZIONE.HasValue)
                    {
                        worksheet.Cell(row, 6).SetValue(strad.DTA_CREAZIONE.Value.Year);
                    }
                    else
                    {
                        worksheet.Cell(row, 6).SetValue("");
                    }
                }
            }

            worksheet.Range(startRow, 1, row, 6).CreateTable("ElencoDipStragiudiziale");

            fileName += " " + desArea;

            worksheet.Columns().AdjustToContents();
            worksheet.Rows().AdjustToContents();
            workbook.SaveAs(ms);
            ms.Position = 0;
            return new FileStreamResult(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = fileName + ".xlsx" };
        }

        #region Tracciato TEX3041
        public ActionResult GeneraTracciatoTEX3041()
        {
            IncentiviEntities db = new IncentiviEntities();
            List<TEXT3041> elementi = new List<TEXT3041>();
            List<int> provvList = new List<int>();

            provvList.Add(1); // PD - Promozione
            provvList.Add(2); // MD - Aumento di merito
            //provvList.Add(3); // GD - Gratifica
            provvList.Add(4); // PD - Promozione senza assorbimento

            provvList.Add(6); // PD - Promozione
            provvList.Add(7); // PD - Promozione senza assorbimento
            provvList.Add(8); // MD - Aumento di merito
            //provvList.Add(9); // GD - Gratifica

            var notAnyNessuna = PoliticheRetributiveHelper.NotAnyOfProv();
            var tmp = PoliticheRetributiveHelper.GetPratiche(db, false)
                        .Where(x => x.ID_CAMPAGNA > 2)
                        .Where(notAnyNessuna);

            DateTime dataLimite = new DateTime(2023, 4, 1);

            tmp = tmp.Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa));
            tmp = tmp.Where(w => w.ID_TEMPLATE != null);
            tmp = tmp.Where(w => w.DECORRENZA != null && w.DECORRENZA < dataLimite);

            ProvvStatoEnum provvStato = ProvvStatoEnum.Consegnato;

            tmp = tmp.Where(x => x.XR_PRV_OPERSTATI.Where(y => y.DATA_FINE_VALIDITA == null).Max(z => z.ID_STATO) == (int)provvStato);
            tmp = tmp.Where(x => x.STATO_LETTERA.Value == 1);

            var anyOfThisProv = PoliticheRetributiveHelper.AnyOfProv(PoliticheRetributiveHelper.GetProvByNumber(db, provvList));
            tmp = tmp.Where(anyOfThisProv);

            List<XR_PRV_DIPENDENTI> lista = new List<XR_PRV_DIPENDENTI>();
            lista = tmp.ToList();

            if (lista != null && lista.Any())
            {
                foreach (var l in lista)
                {
                    // se è FSUPER ed è un provvedimento e area RISORSE CHIAVE
                    // e il provvedimento non è merito allora non lo deve considerare
                    if (l.ID_CAMPAGNA == 1396886732 &&
                        l.XR_PRV_DIREZIONE.ID_AREA == 503234587 &&
                        l.ID_PROV_EFFETTIVO != 2 && l.ID_PROV_EFFETTIVO != 8)
                    {
                        continue;
                    }

                    TEXT3041 item = new TEXT3041();

                    if (l.ID_PROV_EFFETTIVO.GetValueOrDefault() == 1 ||
                            l.ID_PROV_EFFETTIVO.GetValueOrDefault() == 4 ||
                            l.ID_PROV_EFFETTIVO.GetValueOrDefault() == 6 ||
                            l.ID_PROV_EFFETTIVO.GetValueOrDefault() == 7)
                    {
                        item.CodiceEvento = "PC";
                    }
                    else if (l.ID_PROV_EFFETTIVO.GetValueOrDefault() == 2 ||
                            l.ID_PROV_EFFETTIVO.GetValueOrDefault() == 8)
                    {
                        item.CodiceEvento = "MR";
                    }

                    item.Matricola = l.MATRICOLA;
                    item.DataDecorrenza = l.DECORRENZA.GetValueOrDefault();

                    if (item.CodiceEvento == "PC")
                    {
                        item.CategoriaArrivo = l.XR_PRV_DIPENDENTI_VARIAZIONI.FirstOrDefault(w => w.ID_PROV == l.ID_PROV_EFFETTIVO.Value).CAT_PREVISTA;
                        var s = db.SINTESI1.Where(w => w.COD_MATLIBROMAT == l.MATRICOLA).FirstOrDefault();
                        item.CategoriaPartenza = s.COD_QUALIFICA.Trim();
                    }
                    else
                    {
                        item.CategoriaArrivo = "";
                        item.CategoriaPartenza = "";
                    }
                    
                    if (item.CodiceEvento == "MR")
                    {
                        item.ImportoMerito = l.XR_PRV_DIPENDENTI_VARIAZIONI.FirstOrDefault(w => w.ID_PROV == l.ID_PROV_EFFETTIVO.Value).DIFF_RAL;
                    }
                    else
                    {
                        item.ImportoMerito = 0;
                    }

                    

                    //if (l.ID_PROV_EFFETTIVO.GetValueOrDefault() > 4)
                    //{
                    //    // è un provvedimento custom
                    //    item.CodiceEvento = "AD";
                    //}

                    elementi.Add(item);
                }
            }

            if (elementi != null && elementi.Any())
            {
                string path = @"C:\RAI\TRACCIATOTEX3041.txt";
                foreach (var line in elementi)
                {
                    System.IO.File.AppendAllText(path, line.GeneraTraccia() + "\r\n");
                }
            }
            return null;
        }
        #endregion


        #region Tracciato 274 - Gratifica
        public JsonResult GeneraTracciato274()
        {
            dynamic result = new
            {
                esito = true,
                error = ""
            };

            try
            {
                IncentiviEntities db = new IncentiviEntities();
                List<Tracciato274> elementi = new List<Tracciato274>();
                List<int> provvList = new List<int>();

                provvList.Add(3); // GD - Gratifica
                provvList.Add(9); // GD - Gratifica

                //provvList.Add(1); // PD - Promozione
                //provvList.Add(2); // MD - Aumento di merito
                //provvList.Add(4); // PD - Promozione senza assorbimento

                //provvList.Add(6); // PD - Promozione
                //provvList.Add(7); // PD - Promozione senza assorbimento
                //provvList.Add(8); // MD - Aumento di merito

                var notAnyNessuna = PoliticheRetributiveHelper.NotAnyOfProv();
                var tmp = PoliticheRetributiveHelper.GetPratiche(db, false)
                            .Where(x => x.ID_CAMPAGNA > 2)
                            .Where(notAnyNessuna);

                DateTime dataLimite = new DateTime(2023, 4, 1);

                tmp = tmp.Where(x => x.ID_CAMPAGNA > 2 || !x.XR_PRV_OPERSTATI.Any(y => y.ID_STATO == (int)ProvvStatoEnum.Conclusa));
                tmp = tmp.Where(w => w.ID_TEMPLATE != null);
                tmp = tmp.Where(w => w.DECORRENZA != null && w.DECORRENZA < dataLimite);

                ProvvStatoEnum provvStato = ProvvStatoEnum.Consegnato;

                tmp = tmp.Where(x => x.XR_PRV_OPERSTATI.Where(y => y.DATA_FINE_VALIDITA == null).Max(z => z.ID_STATO) == (int)provvStato);
                tmp = tmp.Where(x => x.STATO_LETTERA.Value == 1);

                var anyOfThisProv = PoliticheRetributiveHelper.AnyOfProv(PoliticheRetributiveHelper.GetProvByNumber(db, provvList));
                tmp = tmp.Where(anyOfThisProv);

                List<XR_PRV_DIPENDENTI> lista = new List<XR_PRV_DIPENDENTI>();
                lista = tmp.ToList();

                if (lista != null && lista.Any())
                {
                    CezanneHelper.GetCampiFirma(out var campiFirma);

                    foreach (var l in lista)
                    {
                        //// se è FSUPER ed è un provvedimento e area RISORSE CHIAVE
                        //// e il provvedimento non è merito allora non lo deve considerare
                        //if (l.ID_CAMPAGNA == 1396886732 &&
                        //    l.XR_PRV_DIREZIONE.ID_AREA == 503234587 &&
                        //    l.ID_PROV_EFFETTIVO != 2 && l.ID_PROV_EFFETTIVO != 8)
                        //{
                        //    continue;
                        //}

                        //XR_PRV_OPERSTATI op = new XR_PRV_OPERSTATI();
                        //op.ID_OPER = db.XR_PRV_OPERSTATI.GeneraPrimaryKey();
                        //op.ID_DIPENDENTE = l.ID_DIPENDENTE;
                        //op.ID_PERSONA = CommonHelper.GetCurrentIdPersona();
                        //op.ID_STATO = (int)ProvvStatoEnum.CedoliniElaborati;
                        //op.DATA = DateTime.Now;
                        //op.COD_USER = campiFirma.CodUser;
                        //op.COD_TERMID = campiFirma.CodTermid;
                        //op.TMS_TIMESTAMP = DateTime.Now;
                        //db.XR_PRV_OPERSTATI.Add(op);

                        Tracciato274 item = new Tracciato274();

                        item.Matricola = l.MATRICOLA;
                        item.AnnoCompetenza = "23";
                        item.MeseCompetenza = "04";
                        item.TipoCedolino = " ";
                        item.Codice = "274";
                        item.IVCodice = "1";
                        item.VCodice = "2";
                        item.DaDefinire = "";
                        item.Importo = l.XR_PRV_DIPENDENTI_VARIAZIONI.FirstOrDefault(w => w.ID_PROV == l.ID_PROV_EFFETTIVO.Value).DIFF_RAL;
                        item.MeseDal = "";
                        item.AnnoDal = "";
                        item.MeseAl = "";
                        item.AnnoAl = "";
                        item.W = "";
                        item.Descrittiva = "";

                        elementi.Add(item);
                    }
                }

                if (elementi != null && elementi.Any())
                {
                    DateTime ora = DateTime.Now;

                    List<string> tracciati = new List<string>();
                    string path = @"C:\RAI\TRACCIATO274_temp_" + ora.ToString("yyyyMMdd_HHmmss") + ".txt";
                    foreach (var line in elementi)
                    {
                        //if (line.Matricola != "564610" )
                        //{
                            string traccia = line.GeneraTraccia();
                            System.IO.File.AppendAllText(path, traccia + "\r\n");
                            tracciati.Add(traccia);
                        //}
                    }

                    return Json(result, JsonRequestBehavior.AllowGet);

                    if (tracciati != null && tracciati.Any())
                    {
                        //Scrittura sul db del DEW attraverso il servizio
                        MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.WSDew service = new MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.WSDew();
                        service.Credentials = new System.Net.NetworkCredential("srvruofpo", "zaq22?mk");

                        XR_MAT_ELENCO_TASK task = new XR_MAT_ELENCO_TASK();
                        task.ID = 34;
                        task.TIPO = "TRACCIATO";
                        task.NOME_TASK = "274 - GRATIFICA DISCR. ";
                        task.DESCRIZIONE_TASK = "Inserimento del tracciato record in DEW - POL RETR";
                        task.ID_TRACCIATO_DEW = 380;
                        task.PROGRESSIVO_TRACCIATO_DEW = 1;
                        task.APPKEY = "ABLZP1rCQ3FdXXe8";
                        task.OBBLIGATORIO_PER_CONCLUSIONE = null;
                        string annomese = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString().PadLeft(2, '0');

                        MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.Records_Inseriti DewResponse = service.InserisciRecords(
                            task.APPKEY, annomese, tracciati.ToArray(), task.ID_TRACCIATO_DEW.GetValueOrDefault(), "HRIS", "332783"); // katia di rienzo

                        if (DewResponse.Esito == 0)
                        {
                            result = new
                            {
                                esito = true,
                                error = ""
                            };

                            db.SaveChanges();
                        }
                        else
                        {
                            result = new
                            {
                                esito = false,
                                error = DewResponse.StringaErrore
                            };
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                result = new
                {
                    esito = false,
                    error = ex.Message
                };
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion






        #region Tracciato 274 - Gratifica
        public JsonResult GeneraTracciato274DaFile()
        {
            dynamic result = new
            {
                esito = true,
                error = ""
            };

            try
            {
                string pathFileInput = @"C:\RAI\GRATIFICHE POLITICHE RETRIBUTIVE PRODUZIONE SINISCALCHI.xlsx";
                var wb = new XLWorkbook(pathFileInput);
                var ws = wb.Worksheet("Foglio1");

                int row = 2;

                string matricola = String.Empty;
                string nominativo = String.Empty;
                string campagna = String.Empty;
                string direzione = String.Empty;
                string provvedimento = String.Empty;
                DateTime decorrenza;
                decimal importo = 0;
                decimal costoPeriodo = 0;
                decimal costoAnnuo = 0;

                IncentiviEntities db = new IncentiviEntities();
                List<Tracciato274> elementi = new List<Tracciato274>();

                while (!String.IsNullOrWhiteSpace(matricola = ws.Cell(row, 1).GetValue<string>()))
                {
                    Console.WriteLine("Elemento " + row);

                    try
                    {
                        matricola = ws.Cell(row, 1).GetValue<string>().Trim();
                        nominativo = ws.Cell(row, 2).GetValue<string>().Trim();
                        campagna = ws.Cell(row, 3).GetValue<string>().Trim();
                        direzione = ws.Cell(row, 4).GetValue<string>().Trim();
                        provvedimento = ws.Cell(row, 5).GetValue<string>().Trim();
                        decorrenza = ws.Cell(row, 6).GetValue<DateTime>();
                        importo = ws.Cell(row, 7).GetValue<decimal>();
                        costoPeriodo = ws.Cell(row, 8).GetValue<decimal>();
                        costoAnnuo = ws.Cell(row, 9).GetValue<decimal>();

                        if (String.IsNullOrWhiteSpace(matricola) ||
                            matricola.Length != 6)
                        {
                            string err = String.Format("Errore, matricola {0} alla riga {1}", matricola, row);
                            throw new Exception(err);
                        }

                        Tracciato274 item = new Tracciato274();

                        item.Matricola = matricola;
                        item.AnnoCompetenza = "23";
                        item.MeseCompetenza = "04";
                        item.TipoCedolino = " ";
                        item.Codice = "274";
                        item.IVCodice = "1";
                        item.VCodice = "2";
                        item.DaDefinire = "";
                        item.Importo = importo;
                        item.MeseDal = "";
                        item.AnnoDal = "";
                        item.MeseAl = "";
                        item.AnnoAl = "";
                        item.W = "";
                        item.Descrittiva = "";

                        elementi.Add(item);
                    }
                    catch (Exception ex)
                    {
                        
                    }

                    row++;
                }

                if (elementi != null && elementi.Any())
                {
                    DateTime ora = DateTime.Now;

                    List<string> tracciati = new List<string>();
                    string path = @"C:\RAI\TRACCIATO274_GD_SINISCALCHI" + ora.ToString("yyyyMMdd_HHmmss") + ".txt";
                    foreach (var line in elementi)
                    {
                        string traccia = line.GeneraTraccia();
                        System.IO.File.AppendAllText(path, traccia + "\r\n");
                        tracciati.Add(traccia);
                    }

                    if (tracciati != null && tracciati.Any())
                    {
                        //Scrittura sul db del DEW attraverso il servizio
                        MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.WSDew service = new MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.WSDew();
                        service.Credentials = new System.Net.NetworkCredential("srvruofpo", "zaq22?mk");

                        XR_MAT_ELENCO_TASK task = new XR_MAT_ELENCO_TASK();
                        task.ID = 34;
                        task.TIPO = "TRACCIATO";
                        task.NOME_TASK = "274 - GRATIFICA DISCR. ";
                        task.DESCRIZIONE_TASK = "Inserimento del tracciato record in DEW - POL RETR";
                        task.ID_TRACCIATO_DEW = 380;
                        task.PROGRESSIVO_TRACCIATO_DEW = 1;
                        task.APPKEY = "ABLZP1rCQ3FdXXe8";
                        task.OBBLIGATORIO_PER_CONCLUSIONE = null;
                        string annomese = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString().PadLeft(2, '0');

                        MyRaiServiceInterface.it.rai.servizi.svilruoesercizio.Records_Inseriti DewResponse = service.InserisciRecords(
                            task.APPKEY, annomese, tracciati.ToArray(), task.ID_TRACCIATO_DEW.GetValueOrDefault(), "HRIS", "872254"); // siniscalchi

                        if (DewResponse.Esito == 0)
                        {
                            result = new
                            {
                                esito = true,
                                error = ""
                            };

                            db.SaveChanges();
                        }
                        else
                        {
                            result = new
                            {
                                esito = false,
                                error = DewResponse.StringaErrore
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                result = new
                {
                    esito = false,
                    error = ex.Message
                };
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

    }
}