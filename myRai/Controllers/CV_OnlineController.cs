using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using myRai.DataAccess;
using System.Data.SqlClient;
using System.Net;
using System.Data;
using System.IO;
using System.Globalization;
using iTextSharp.text.pdf;
using System.Text.RegularExpressions;
using ClosedXML.Excel;
using iTextSharp.text;
using myRaiData;
//using myRai.it.rai.servizi.sendmail;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using myRaiCommonModel.cvModels;
using myRaiCommonModel.cvModels.Pdf;
using myRaiHelper;
using myRaiCommonModel;
using myRaiCommonManager;
using myRai.Data.CurriculumVitae;
using myRaiCommonModel.raiplace;
using myRaiServiceHub.it.rai.servizi.hrce;
using CommonManager = myRaiHelper.CommonHelper;
using Utente = myRaiHelper.UtenteHelper;
using myRaiCommonTasks.sendMail;
using WebGrease.Css.Extensions;

namespace myRai.Controllers
{
    public static class cvModelEntitiesExtension
    {
        public static void SetCommandTimeout(this DbContext dbContext, int timeOut)
        {
            ((IObjectContextAdapter)dbContext).ObjectContext.CommandTimeout = timeOut;
        }
    }

    public partial class CV_OnlineController : BaseCommonController
    {
        private string _serverPath { get; set; }

        cvModel cvBox = new cvModel();

        public CV_OnlineController()
        {
            cvBox = new cvModel();

            if (Server != null)
            {
                FontManager.FontPath = Server.MapPath("~/assets/fontG/open-sans-v13-latin-300.ttf");
            }
        }

        public CV_OnlineController(string serverMapPath, string fontPath)
        {
            cvBox = new cvModel();

            if (Server == null)
            {
                FontManager.FontPath = (!String.IsNullOrEmpty(fontPath) ? fontPath : "");
                this._serverPath = serverMapPath;
            }
            else
            {
                FontManager.FontPath = Server.MapPath("~/assets/fontG/open-sans-v13-latin-300.ttf");
            }
        }

        private string StringToDate(string date)
        {
            string result = null;

            if (!String.IsNullOrWhiteSpace(date))
                result = date.Substring(6, 2) + "/" + date.Substring(4, 2) + "/" + date.Substring(0, 4);

            return result;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
        }

        public ActionResult Stat()
        {
            if ( !Utente.IsAdmin( CommonManager.GetCurrentUserMatricola( ) ) )
                return Content( "NON AUTORIZZATO" );

            cv_ModelEntities cvEnt = new cv_ModelEntities();
            cvEnt.SetCommandTimeout(500);
            string statisticheSQL = String.Empty;

            using (digiGappEntities db = new digiGappEntities())
            {
                var sql = db.MyRai_ParametriSistema.Where(p => p.Chiave.Equals("CVSPPercMassiva", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                string sezAbil = CommonManager.GetParametro<string>(EnumParametriSistema.CVEditorialiSezContabiliAbilitate);
                if (sql != null)
                    statisticheSQL = sql.Valore1.Replace("#sezAbil", "''" + sezAbil.Replace(",", "'',''") + "''");
            }

            var watch = System.Diagnostics.Stopwatch.StartNew();

            List<CommonManager.GetPercentualCVResults> myTest = new List<CommonManager.GetPercentualCVResults>( );
            myTest.AddRange( cvEnt.Database.SqlQuery<CommonManager.GetPercentualCVResults>( statisticheSQL ) );

            XLWorkbook workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Stats CV");

            int counter = 0;
            worksheet.Cell(3 + counter, 1).Value = "Matricola";
            worksheet.Cell(3 + counter, 2).Value = "Figura professionale";
            worksheet.Cell(3 + counter, 3).Value = "Direzione";
            worksheet.Cell(3 + counter, 4).Value = "Compilazione";
            worksheet.Cell(3 + counter, 5).Value = "Box obbligatori";
            worksheet.Cell(3 + counter, 6).Value = "Mancanti";
            worksheet.Cell(3 + counter, 7).Value = "Incomplete";
            worksheet.Cell(3 + counter, 8).Value = "Numero sezioni compilate";
            worksheet.Cell(3 + counter, 9).Value = "Sezioni compilate";
            worksheet.Cell(3 + counter, 10).Value = "Numero sezioni compilate (coeff. 0)";
            worksheet.Cell(3 + counter, 11).Value = "Sezioni compilate (coeff. 0)";

            foreach (var item in myTest)
            {
                counter++;
                worksheet.Cell(3 + counter, 1).SetValue<string>(item.Matricola);
                worksheet.Cell(3 + counter, 2).SetValue<string>(item.FiguraProf);
                worksheet.Cell(3 + counter, 3).SetValue<string>(item.Servizio);
                worksheet.Cell(3 + counter, 4).SetValue<string>(item.Percentuale.ToString() + " %");
                worksheet.Cell(3 + counter, 5).SetValue<int>(item.BoxValPerc);
                worksheet.Cell(3 + counter, 6).SetValue<string>(item.Mancanti);
                worksheet.Cell(3 + counter, 7).SetValue<string>(item.Incomplete);
                worksheet.Cell(3 + counter, 8).SetValue<int>(item.NumComplete);
                worksheet.Cell(3 + counter, 9).SetValue<string>(item.Complete);
                worksheet.Cell(3 + counter, 10).SetValue<int>(item.NumCompleteNoCoeff);
                worksheet.Cell(3 + counter, 11).SetValue<string>(item.CompleteNoCoeff);
            }

            counter += 2;
            worksheet.Cell(3 + counter, 1).Value = myTest.Count().ToString() + " matricole";
            worksheet.Cell(3 + counter, 2).Value = Math.Round((float)myTest.Sum(x => x.Percentuale) / (float)myTest.Count(), 2).ToString() + " %";

            worksheet.Columns().AdjustToContents();
            worksheet.Cell(1, 1).Value = "Stato compilazione CV al " + DateTime.Now.ToString("dd/MM/yyyy HH.mm");
            worksheet.Cell(1, 1).Style.Font.Bold = true;

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            counter += 2;
            worksheet.Cell(3 + counter, 1).Value = "Tempo di esecuzione: ";
            worksheet.Cell(3 + counter, 2).Value = elapsedMs.ToString();

            MemoryStream M = new MemoryStream();
            workbook.SaveAs(M);
            M.Position = 0;

            return new FileStreamResult(M, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = "StatsCV.xlsx" };
        }

        public ActionResult GetDataTable(string table, string file = "")
        {
            if (!CommonManager.PdfAutorizzato(CommonManager.GetCurrentUserMatricola()))
                return Content("NON AUTORIZZATO");

            if (String.IsNullOrEmpty(file))
                file = "";

            var dbCV = new cv_ModelEntities();

            XLWorkbook workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Dati " + file);

            int counter = 0;

            SqlCommand sqlCommand = new SqlCommand();
            SqlDataAdapter da = new SqlDataAdapter(sqlCommand);

            DataSet ds = new DataSet();
            using (SqlConnection sqlConnection = new SqlConnection())
            {
                sqlConnection.ConnectionString = new cv_ModelEntities().Database.Connection.ConnectionString;
                sqlCommand.Connection = sqlConnection;
                sqlCommand.CommandType = CommandType.Text;
                sqlCommand.CommandText = "select * from " + table;

                da.Fill(ds);
            }

            int indexCol = 0;
            foreach (DataColumn col in ds.Tables[0].Columns)
            {
                indexCol++;
                worksheet.Cell(3 + counter, indexCol).SetValue<string>(col.ColumnName);
            }

            counter++;
            worksheet.Cell(3 + counter, 1).InsertData(ds.Tables[0].AsEnumerable());

            worksheet.Columns().AdjustToContents();
            worksheet.Cell(1, 1).Value = "Stato compilazione CV al " + DateTime.Now.ToString("dd/MM/yyyy HH.mm");
            worksheet.Cell(1, 1).Style.Font.Bold = true;

            MemoryStream M = new MemoryStream();
            workbook.SaveAs(M);
            M.Position = 0;

            return new FileStreamResult(M, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = file + ".xlsx" };
        }

        public ActionResult OnBoarding(int idItem)
        {
            var db = new myRaiData.digiGappEntities();
            OnBoardingModel model = new OnBoardingModel();

            model.ItemList = db.MyRai_OnBoarding.Where(x => x.attivo == true).OrderBy(x => x.progressivo).ToList();

            if (idItem == 0)
            {
                model.ItemSelected = model.ItemList.FirstOrDefault();
                model.idPrevItem = 0;
                if (model.ItemSelected != null)
                {
                    myRaiData.MyRai_OnBoarding obnext = model.ItemList.Where(x => x.progressivo > model.ItemSelected.progressivo)
                        .OrderBy(x => x.progressivo).FirstOrDefault();
                    model.idNextItem = obnext == null ? 0 : obnext.id;
                }
            }
            else
            {
                model.ItemSelected = model.ItemList.Where(x => x.id == idItem).FirstOrDefault();
                if (model.ItemSelected != null)
                {
                    myRaiData.MyRai_OnBoarding obnext = model.ItemList.Where(x => x.progressivo > model.ItemSelected.progressivo)
                       .OrderBy(x => x.progressivo).FirstOrDefault();
                    model.idNextItem = obnext == null ? 0 : obnext.id;

                    myRaiData.MyRai_OnBoarding obprev = model.ItemList.Where(x => x.progressivo < model.ItemSelected.progressivo)
                       .OrderByDescending(x => x.progressivo).FirstOrDefault();
                    model.idPrevItem = obprev == null ? 0 : obprev.id;
                }
            }

            return View(model);
        }

        public ActionResult Index()
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities();
            cvBox.listaBox = new List<cvModel.Box>();
            cvBox.menuSidebar = new sidebarModel( CommonManager.GetCurrentUserMatricola( ) , CommonManager.GetCurrentUserPMatricola( ) , 5 );

            var lista = CommonManager.GetListaBox(cvEnt);

            foreach (TCVBox_V2 boxSingolo in lista)
            {
                cvModel.Box appo = new cvModel.Box();
                appo._titolo = boxSingolo.Titolo;
                appo._sottotitolo = boxSingolo.Sottotitolo;
                appo._icon = boxSingolo.Icona;
                appo._colore = boxSingolo.Stile;
                appo._url = boxSingolo.Link;
                appo._funzioniAggiungi = boxSingolo.FunzioniAggiungi;
                appo._titoloAggiungi = boxSingolo.TitoloAggiungi;
                appo._idMenu = boxSingolo.Id_box;
                cvBox.listaBox.Add(appo);
            }

            string errore = "";
            cvBox.CanPrenMappatura = VerificaRequisitiMappatura(Utente.Matricola(), out errore);

            //check privacy flag
            bool privacyAccepted = ProfiloPersonaleManager.PrivacyAccepted(Utente.EsponiAnagrafica()._matricola);
            cvBox.privacy = new cvModel.Privacy();
            cvBox.privacy.privacyAccepted = privacyAccepted;

            if (cvBox.privacy.privacyAccepted)
            {
                cvBox.privacy.privacyAcceptedAt = ProfiloPersonaleManager.PrivacyAcceptedAt(Utente.EsponiAnagrafica()._matricola);
            }

            cvBox.privacy.infoPrivacy = new Dictionary<string, string>();
            cvBox.privacy.infoPrivacy.Add("Termini", "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum. Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt. Neque porro quisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt ut labore et dolore magnam aliquam quaerat voluptatem.");
            cvBox.privacy.infoPrivacy.Add("Lorem Ipsum", "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum. Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt. Neque porro quisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt ut labore et dolore magnam aliquam quaerat voluptatem.");
            cvBox.privacy.infoPrivacy.Add("Dolor sit", "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum. Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt. Neque porro quisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt ut labore et dolore magnam aliquam quaerat voluptatem.");
            cvBox.privacy.infoPrivacy.Add("Legal sum", "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum. Sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium, totam rem aperiam, eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo. Nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit, sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt. Neque porro quisquam est, qui dolorem ipsum quia dolor sit amet, consectetur, adipisci velit, sed quia non numquam eius modi tempora incidunt ut labore et dolore magnam aliquam quaerat voluptatem.");
            cvBox.privacy.infoPrivacy.Add("", CommonManager.getTestoPrivacy());

            return View(cvBox);
        }

        public ActionResult Privacy()
        {
            return View("~/Views/CV_Online/Privacy/BoxView.cshtml", cvBox);
        }

        [HttpGet]
        public ActionResult UpdatePrivacy()
        {
            bool privacyUpdated = ProfiloPersonaleManager.AcceptPrivacy(
           Utente.EsponiAnagrafica()._matricola);

            string ex = "nok";

            if (privacyUpdated)
            {
                ex = "ok";
            }

            return Json(ex, JsonRequestBehavior.AllowGet);
        }

        

        public ActionResult Pdf(string matricola = null, string msgUltimoAgg = null)
        {
            if (!CommonManager.IsProduzione())
                return PdfTest(matricola, msgUltimoAgg);

            var result = CV_OnlineManager.GetCVPdf(Server.MapPath("~"), Server.MapPath("~/assets/fontG/open-sans-v13-latin-300.ttf"), matricola, msgUltimoAgg, out int error);
            if (error!=0)
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

        

        #region Controller Box Anagrafico
        public ActionResult BoxAnagrafico()
        {
            return View("~/Views/CV_Online/partials/_box_anagrafico.cshtml");
        }
        #endregion

        #region Controller Languages
        public ActionResult languages()
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities();
            cvBox.lingue = new List<cvModel.Languages>();

            //carico il modello CvModel.studies
            string matricola = Utente.EsponiAnagrafica()._matricola;
            List<TCVLingue> lingue = (cvEnt.TCVLingue.Where(m => m.Matricola == matricola).OrderBy(y => y.CodLinguaLiv)).ToList();

            foreach (TCVLingue lang in lingue)
            {
                cvModel.Languages frk = new cvModel.Languages( CommonManager.GetCurrentUserMatricola( ) );

                frk._altraLingua = lang.AltraLingua;
                frk._codLingua = lang.CodLingua;
                frk._codLinguaLiv = lang.CodLinguaLiv;
                frk._dataOraAgg = lang.DataOraAgg;
                frk._livAscolto = lang.LivAscolto;
                frk._livInterazione = lang.LivInterazione;
                frk._livLettura = lang.LivLettura;
                frk._livProdOrale = lang.LivProdOrale;
                frk._livScritto = lang.LivScritto;
                frk._matricola = lang.Matricola;
                frk._stato = lang.Stato;
                frk._tipoAgg = lang.TipoAgg;

                //Descrizione Lingua
                DLingua tmp_lingua = cvEnt.DLingua.Where(m => m.CodLingua == lang.CodLingua).First();
                frk._descLingua = tmp_lingua.DescLingua;

                //Flag dello Stato
                frk._flagStato = tmp_lingua.FlagStato;

                //Descrizione Livello di Lingua
                DLinguaLiv tmp_lingualiv = cvEnt.DLinguaLiv.Where(m => m.CodLinguaLiv == lang.CodLinguaLiv).FirstOrDefault();
                if (tmp_lingualiv != null)
                    frk._descLinguaLiv = tmp_lingualiv.DescLinguaLiv;

                cvBox.lingue.Add(frk);
            }

            ViewBag.idMenu = 20;

            return View("~/Views/CV_Online/languages/BoxView.cshtml", cvBox);
        }

        [HttpPost]
        public ActionResult InsertLanguage(cvModel.Languages lingue)
        {
            string result;

            if (ModelState.IsValid)
            {
                string matricola = Utente.EsponiAnagrafica()._matricola;
                cv_ModelEntities cvEnt = new cv_ModelEntities();

                TCVLingue language = new TCVLingue();

                //riempimento oggetto Language ->TCVLingue
                language.Matricola = matricola;
                language.AltraLingua = "";
                language.CodLingua = lingue._codLingua;
                language.CodLinguaLiv = lingue._codLinguaLiv;
                language.DataOraAgg = DateTime.Now;
                language.LivAscolto = lingue._livAscolto;
                language.LivInterazione = lingue._livInterazione;
                language.LivLettura = lingue._livLettura;
                language.LivProdOrale = lingue._livProdOrale;
                language.LivScritto = lingue._livScritto;
                language.Note = lingue._note;
                language.Stato = "S";
                language.TipoAgg = "I";

                try
                {
                    cvEnt.TCVLingue.Add(language);
                    if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                        throw new Exception( "Errore salvataggio dati DB" );
                    result = "ok";
                }
                catch (Exception)
                {
                    result = "Errore nel salvataggio dei dati";
                }
            }
            else
            {
                result = "Dati del modello non validi";
            }

            return Content(result);
        }

        [HttpGet]
        public ActionResult DeleteLanguages(string matricola, string codLingua)
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities();

            if (matricola == null && codLingua == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            TCVLingue lingua = cvEnt.TCVLingue.Find( matricola , codLingua );
            if (lingua == null)
            {
                return HttpNotFound();
            }

            try
            {
                cvEnt.TCVLingue.Remove(lingua);
                if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                    throw new Exception( "Errore salvataggio dati DB" );
            }
            catch (Exception exc)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.idMenu = 20;
            return Content("Ok");
        }

        /// <summary>
        /// action per la creazione della view di modifica per Languages
        /// </summary>
        /// <param name="language"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create_ViewLanguages(cvModel.Languages language)
        {
            cv_ModelEntities db = new cv_ModelEntities();
            var row = db.TCVLingue.Where(x => x.Matricola == language._matricola && x.CodLingua == language._codLingua).FirstOrDefault();
            if (row != null) language._note = row.Note;

            ModelState.Clear();

            string matricola = language._matricola;
            string codLingua = language._codLingua;

            if (matricola == null || codLingua == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View("~/Views/CV_Online/languages/ModificaLanguage.cshtml", language);
        }

        [HttpPost]
        public ActionResult ModificaLanguage(cvModel.Languages language)
        {
            string result;

            if (ModelState.IsValid)
            {
                string matricola = language._matricola;
                string codLingua = language._codLingua;

                cv_ModelEntities cvEnt = new cv_ModelEntities();
                TCVLingue lingua = cvEnt.TCVLingue.Find(matricola, codLingua);

                lingua.AltraLingua = "";
                lingua.CodLingua = codLingua;
                lingua.CodLinguaLiv = language._codLinguaLiv;
                lingua.DataOraAgg = DateTime.Now;
                lingua.LivAscolto = language._livAscolto;
                lingua.LivInterazione = language._livInterazione;
                lingua.LivLettura = language._livLettura;
                lingua.LivProdOrale = language._livProdOrale;
                lingua.LivScritto = language._livScritto;
                lingua.Matricola = matricola;
                lingua.Note = language._note;
                lingua.Stato = "S";
                lingua.TipoAgg = "A";

                try
                {
                    if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                        throw new Exception( "Errore salvataggio dati DB" );
                    result = "ok";
                }
                catch (Exception)
                {
                    result = "Errore nel salvataggio dei dati";
                }
            }
            else
            {
                result = "Dati del modello non validi";
            }

            return Content(result);
        }

        #endregion

        #region Controller Experiences
        [HttpGet]
        public ActionResult experiences()
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities();
            cvBox.experencies = new List<cvModel.Experiences>();
            string matricola = Utente.EsponiAnagrafica()._matricola;
            string codFigProf = Utente.EsponiAnagrafica()._codiceFigProf;

            var esperienze = cvEnt.TCVEsperExRai.Where(x => x.Matricola == matricola).OrderByDescending(z => z.DataInizio).ToList();

            foreach (TCVEsperExRai esp in esperienze)
            {
                cvModel.Experiences frk_esp = new cvModel.Experiences( CommonManager.GetCurrentUserMatricola( ) );
                frk_esp._areaAtt = esp.AreaAtt;
                frk_esp._codContinente = esp.CodContinente;
                frk_esp._descContinente = "";

                if (esp.CodContinente != null)
                {
                    var desContinente = cvEnt.DContinente.FirstOrDefault(x => x.CodContinente == esp.CodContinente);
                    if (desContinente != null)
                        frk_esp._descContinente = desContinente.DesContinente;
                }

                frk_esp._codRedazione = esp.CodRedazione;
                frk_esp._descRedazione = "";

                if (esp.CodRedazione != null)
                {
                    var desRed = cvEnt.DRedazione.FirstOrDefault(x => x.CodRedazione == esp.CodRedazione);
                    if (desRed != null)
                        frk_esp._descRedazione = desRed.DesRedazione;
                }

                frk_esp._codiceFiguraProf = esp.CodiceFiguraProf;
                frk_esp._codDirezione = esp.CodDirezione;
                frk_esp._direzione = esp.Direzione;

                if ((esp.CodDirezione != null) && (esp.CodDirezione != "-1") && (esp.CodDirezione != ""))
                {
                    var dir = cvEnt.VDServizioCV.FirstOrDefault(x => x.Codice == esp.CodDirezione);
                    if (dir != null)
                        frk_esp._direzione = dir.Descrizione;
                }

                frk_esp._codSocieta = esp.CodSocieta;
                frk_esp._societa = esp.Societa;

                if ((esp.CodSocieta != null) && (esp.CodSocieta != "") && esp.CodSocieta != "-1")
                {
                    var societa = cvEnt.VDSocieta.FirstOrDefault(x => x.Codice == esp.CodSocieta);
                    if (societa != null)
                        frk_esp._societa = societa.Descrizione;
                }

                frk_esp._stato = esp.Stato;
                frk_esp._dataFine = esp.DataFine;
                frk_esp._dataInizio = esp.DataInizio;
                frk_esp._dataOraAgg = esp.DataOraAgg;
                frk_esp._descrizioneEsp = esp.DescrizioneEsp;
                frk_esp._flagEspEstero = esp.FlagEspEstero;
                frk_esp._flagEspRai = esp.FlagEspRai;

                if ((codFigProf == "MAA") || (codFigProf == "MBA"))
                {
                    frk_esp._isGiornalista = "1";
                }
                else
                {
                    frk_esp._isGiornalista = "0";
                }

                frk_esp._localitaEsp = esp.LocalitaEsp;
                frk_esp._matricola = esp.Matricola;
                frk_esp._nazione = esp.Nazione;
                frk_esp._prog = esp.Prog;
                frk_esp._tipoAgg = esp.TipoAgg;
                frk_esp._titoloEspQual = esp.TitoloEspQual;
                frk_esp._ultRuolo = esp.UltRuolo;
                frk_esp._budgetGest = esp.CodBudgetGest;
                frk_esp._risorseGest = esp.CodRisorseGest;
                frk_esp._procura = esp.CodProcura;
                frk_esp._codLocalitaEsp = esp.CodLocalitaEsp;
                frk_esp._codIndustry = esp.CodSettore;
                frk_esp._industry = esp.Settore;
                frk_esp._codFigProExtra = esp.CodFigProExtra;
                frk_esp._figProExtra = esp.FigProExtra;
                cvBox.experencies.Add(frk_esp);
            }

            ViewBag.idMenu = 24;
            return View("~/Views/CV_Online/experiences/BoxView.cshtml", cvBox);
        }

        [HttpPost]
        public ActionResult Create_DettaglioRai(cvModel.Experiences experiences)
        {
            ModelState.Clear();
            return View("~/Views/CV_Online/experiences/DettaglioRai.cshtml", experiences);
        }

        [HttpPost]
        public ActionResult Create_DettaglioExtraRai(cvModel.Experiences experiences)
        {
            ModelState.Clear();
            return View("~/Views/CV_Online/experiences/DettaglioExtraRai.cshtml", experiences);
        }

        [HttpPost]
        public ActionResult InsertExperiences(cvModel.Experiences experiences)
        {
            string result = "";
            string matricola;
            int prog;

            matricola = Utente.EsponiAnagrafica()._matricola;

            cv_ModelEntities cvEnt = new cv_ModelEntities();
            TCVEsperExRai esper = new TCVEsperExRai();

            if (String.IsNullOrEmpty(experiences._annoInizio))
            {
                result = "error-data";
                return Content(result);
            }

            //nel caso sia stata indicata una data completa del giorno, controllo che sia valida
            if (experiences._giornoInizio != "00")
            {
                try
                {
                    DateTime test = new DateTime(Int32.Parse(experiences._annoInizio), Int32.Parse(experiences._meseInizio), Int32.Parse(experiences._giornoInizio));
                }
                catch
                {
                    result = "Data di inizio esperienza non valida";
                    return Content(result);
                }
            }

            if (String.IsNullOrEmpty(experiences._annoFine) || experiences._annoFine == "9999")
            {
                esper.DataFine = null;
            }
            else
            {
                //nel caso sia stata indicata una data completa del giorno, controllo che sia valida
                if (experiences._giornoFine != "00")
                {
                    try
                    {
                        DateTime test = new DateTime(Int32.Parse(experiences._annoFine), Int32.Parse(experiences._meseFine), Int32.Parse(experiences._giornoFine));
                    }
                    catch
                    {
                        result = "Data di fine esperienza non valida";
                        return Content(result);
                    }
                }

                esper.DataFine = experiences._annoFine + experiences._meseFine + experiences._giornoInizio;
            }

            int annoFine = !String.IsNullOrWhiteSpace(experiences._annoFine) ? Convert.ToInt32(experiences._annoFine) : 9999;
            int annoInizio = Convert.ToInt32(experiences._annoInizio);
            int meseFine = !String.IsNullOrWhiteSpace(experiences._meseFine) ? Convert.ToInt32(experiences._meseFine) : 12;
            int meseInizio = !String.IsNullOrWhiteSpace(experiences._meseInizio) ? Convert.ToInt32(experiences._meseInizio) : 1;
            int giornoFine = !String.IsNullOrWhiteSpace(experiences._giornoFine) ? Convert.ToInt32(experiences._giornoFine) : 31;
            int giornoInizio = !String.IsNullOrWhiteSpace(experiences._giornoInizio) ? Convert.ToInt32(experiences._giornoInizio) : 1;

            if (annoFine < annoInizio
                || (annoFine == annoInizio && meseFine < meseInizio)
                || (annoFine == annoInizio && meseFine == meseInizio && giornoFine < giornoInizio))
            {
                result = "Data di fine minore di data inizio";
                return Content(result);
            }

            esper.AreaAtt = experiences._areaAtt;
            esper.CodContinente = experiences._codContinente;
            esper.CodDirezione = experiences._codDirezione;
            esper.CodiceFiguraProf = experiences._codiceFiguraProf;
            esper.CodRedazione = experiences._codRedazione;
            esper.CodSocieta = experiences._codSocieta;
            esper.DataInizio = experiences._annoInizio + experiences._meseInizio + experiences._giornoInizio;

            esper.DataOraAgg = DateTime.Now;
            esper.DescrizioneEsp = experiences._descrizioneEsp;
            esper.Direzione = experiences._direzione;

            if (!String.IsNullOrEmpty(experiences._nazione) &&
                !experiences._nazione.Equals("ITALIA", StringComparison.InvariantCultureIgnoreCase))
            {
                esper.FlagEspEstero = "1";
            }
            else
            {
                esper.FlagEspEstero = "0";
            }

            esper.FlagEspRai = experiences._flagEspRai;
            esper.LocalitaEsp = experiences._localitaEsp;
            esper.Matricola = matricola;
            esper.Nazione = experiences._nazione;
            esper.Societa = experiences._societa;
            esper.Stato = "S";
            esper.TipoAgg = "I";
            esper.TitoloEspQual = experiences._titoloEspQual;
            esper.UltRuolo = experiences._ultRuolo;
            esper.CodBudgetGest = experiences._budgetGest;
            esper.CodRisorseGest = experiences._risorseGest;
            esper.CodProcura = experiences._procura;
            esper.CodLocalitaEsp = experiences._codLocalitaEsp;
            esper.CodSettore = experiences._codIndustry;
            esper.Settore = experiences._industry;
            esper.CodFigProExtra = experiences._codFigProExtra;
            esper.FigProExtra = experiences._figProExtra;
            //calcolo del prog
            var tmp = cvEnt.TCVEsperExRai.Where(x => x.Matricola == matricola);
            if (tmp.Count() == 0)
            {
                prog = 1;
            }
            else
            {
                var nro_prog = (cvEnt.TCVEsperExRai.Where(x => x.Matricola == matricola)).Max(x => x.Prog);
                prog = Convert.ToInt32(nro_prog) + 1;
            }
            esper.Prog = prog;
            try
            {
                cvEnt.TCVEsperExRai.Add(esper);
                if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                    throw new Exception( "Errore salvataggio dati DB" );
                result = "ok";
            }
            catch (Exception exc)
            {
                result = exc.Message;
            }


            return Content(result);
        }

        [HttpPost]
        public ActionResult Create_ModificaDettaglioRai(cvModel.Experiences experiences)
        {
            string matricola = Utente.EsponiAnagrafica()._matricola;
            int prog = experiences._prog;

            if (matricola == null || prog == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View("~/Views/CV_Online/experiences/ModificaDettaglioRai.cshtml", experiences);
        }

        [HttpPost]
        public ActionResult Create_ModificaDettaglioExtraRai(cvModel.Experiences experiences)
        {
            string matricola = Utente.EsponiAnagrafica()._matricola;
            int prog = experiences._prog;

            if (matricola == null || prog == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View("~/Views/CV_Online/experiences/ModificaDettaglioExtraRai.cshtml", experiences);
        }

        private bool CopiaEsperienza(cvModel.Experiences experiences)
        {
            bool isAdded = false;

            ActionResult result = InsertExperiences(experiences);
            if (result != null)
            {
                ContentResult cResult = result as ContentResult;
                if (cResult == null || cResult.Content != "ok")
                    isAdded = false;
                else
                    isAdded = true;
            }

            return isAdded;
        }

        [HttpPost]
        public ActionResult Create_CopiaDettaglioRai(cvModel.Experiences experiences)
        {
            if (!CopiaEsperienza(experiences))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            string matricola = Utente.EsponiAnagrafica()._matricola;
            int prog = experiences._prog;

            if (matricola == null || prog == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View("~/Views/CV_Online/experiences/ModificaDettaglioRai.cshtml", experiences);
        }

        [HttpPost]
        public ActionResult Create_CopiaDettaglioExtraRai(cvModel.Experiences experiences)
        {
            if (!CopiaEsperienza(experiences))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            string matricola = Utente.EsponiAnagrafica()._matricola;
            int prog = experiences._prog;

            if (matricola == null || prog == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View("~/Views/CV_Online/experiences/ModificaDettaglioExtraRai.cshtml", experiences);
        }

        [HttpGet]
        public ActionResult DeleteExperiences(string matricola, int prog)
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities();

            if ((matricola == null) && (prog < 0))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TCVEsperExRai master = cvEnt.TCVEsperExRai.Find(matricola, prog);
            if (master == null)
            {
                return HttpNotFound();
            }
            try
            {
                cvEnt.TCVEsperExRai.Remove(master);
                if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                    throw new Exception( "Errore salvataggio dati DB" );
            }
            catch (Exception exc)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.idMenu = 19;
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult ModificaExperiences(cvModel.Experiences experiences)
        {
            string result = "";
            string matricola = experiences._matricola;
            int prog = experiences._prog;

            cv_ModelEntities cvEnt = new cv_ModelEntities();
            TCVEsperExRai esper = new TCVEsperExRai();

            esper = cvEnt.TCVEsperExRai.Find(matricola, prog);

            if (esper == null)
            {
                return HttpNotFound();
            }

            esper.AreaAtt = experiences._areaAtt;
            esper.CodContinente = experiences._codContinente;
            esper.CodDirezione = experiences._codDirezione;
            esper.CodiceFiguraProf = experiences._codiceFiguraProf;
            esper.CodRedazione = experiences._codRedazione;
            esper.CodSocieta = experiences._codSocieta;

            if (String.IsNullOrEmpty(experiences._annoInizio))
            {
                result = "error-data";
                return Content(result);
            }

            //nel caso sia stata indicata una data completa del giorno, controllo che sia valida
            if (experiences._giornoInizio != "00")
            {
                try
                {
                    DateTime test = new DateTime(Int32.Parse(experiences._annoInizio), Int32.Parse(experiences._meseInizio), Int32.Parse(experiences._giornoInizio));
                }
                catch
                {
                    result = "Data di inizio esperienza non valida";
                    return Content(result);
                }
            }

            esper.DataInizio = experiences._annoInizio + experiences._meseInizio + experiences._giornoInizio;

            if (String.IsNullOrEmpty(experiences._annoFine) || experiences._annoFine == "9999")
            {
                esper.DataFine = null;
            }
            else
            {
                if (experiences._giornoFine != "00")
                {
                    try
                    {
                        DateTime test = new DateTime(Int32.Parse(experiences._annoFine), Int32.Parse(experiences._meseFine), Int32.Parse(experiences._giornoFine));
                    }
                    catch
                    {
                        result = "Data di fine esperienza non valida";
                        return Content(result);
                    }
                }
                esper.DataFine = experiences._annoFine + experiences._meseFine + experiences._giornoFine;
            }

            int annoFine = !String.IsNullOrWhiteSpace(experiences._annoFine) ? Convert.ToInt32(experiences._annoFine) : 9999;
            int annoInizio = Convert.ToInt32(experiences._annoInizio);
            int meseFine = !String.IsNullOrWhiteSpace(experiences._meseFine) ? Convert.ToInt32(experiences._meseFine) : 12;
            int meseInizio = !String.IsNullOrWhiteSpace(experiences._meseInizio) ? Convert.ToInt32(experiences._meseInizio) : 1;
            int giornoFine = !String.IsNullOrWhiteSpace(experiences._giornoFine) ? Convert.ToInt32(experiences._giornoFine) : 31;
            int giornoInizio = !String.IsNullOrWhiteSpace(experiences._giornoInizio) ? Convert.ToInt32(experiences._giornoInizio) : 1;

            if (annoFine < annoInizio
                || (annoFine == annoInizio && meseFine < meseInizio)
                || (annoFine == annoInizio && meseFine == meseInizio && giornoFine < giornoInizio))
            {
                result = "Data di fine minore di data inizio";
                return Content(result);
            }

            esper.DataOraAgg = DateTime.Now;
            esper.DescrizioneEsp = experiences._descrizioneEsp;
            esper.Direzione = experiences._direzione;
            if (!String.IsNullOrEmpty(experiences._nazione) &&
                !experiences._nazione.Equals("ITALIA", StringComparison.InvariantCultureIgnoreCase))
            {
                esper.FlagEspEstero = "1";
            }
            else
            {
                esper.FlagEspEstero = "0";
            }

            esper.FlagEspRai = experiences._flagEspRai;
            esper.LocalitaEsp = experiences._localitaEsp;
            esper.Matricola = matricola;
            esper.Nazione = experiences._nazione;
            esper.Societa = experiences._societa;
            esper.Stato = "S";
            esper.TipoAgg = "A";
            esper.TitoloEspQual = experiences._titoloEspQual;
            esper.UltRuolo = experiences._ultRuolo;
            esper.CodBudgetGest = experiences._budgetGest;
            esper.CodRisorseGest = experiences._risorseGest;
            esper.CodProcura = experiences._procura;
            esper.CodLocalitaEsp = experiences._codLocalitaEsp;
            esper.CodSettore = experiences._codIndustry;
            esper.Settore = experiences._industry;
            esper.CodFigProExtra = experiences._codFigProExtra;
            esper.FigProExtra = experiences._figProExtra;

            try
            {
                if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                    throw new Exception( "Errore salvataggio dati DB" );
                result = "ok";
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return Content(result);
        }

        #endregion

        #region Controller Studies
        public ActionResult studies()
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities();
            cvBox.curricula = new List<cvModel.Studies>();

            //carico il modello CvModel.studies
            string matricola = Utente.EsponiAnagrafica()._matricola;
            var istruzione = (cvEnt.TCVIstruzione.Where(m => m.Matricola == matricola)).ToList();
            var specializz = (cvEnt.TCVSpecializz.Where(m => m.Matricola == matricola)).ToList();

            foreach (TCVIstruzione istr in istruzione)
            {
                cvModel.Studies frk = new cvModel.Studies( CommonManager.GetCurrentUserMatricola( ) );
                //freak - riempire i campi
                using (var ctx = new cv_ModelEntities())
                {
                    var param = new SqlParameter("@param", istr.CodTitolo);
                    List<Utente.CV_DescTitoloLogo> tmp = ctx.Database.SqlQuery<Utente.CV_DescTitoloLogo>("exec sp_GETDESCTITOLO @param", param).ToList();

                    frk._descTipoTitolo = tmp[0].DescTipoTitolo; //tmp.GetValue(0,0).ToString();
                    frk._descTitolo = tmp[0].DescTitolo;//tmp.GetValue(0, 1).ToString();
                    frk._logo = tmp[0].Logo;//tmp.GetValue(0, 2).ToString();
                    //recupero descNazione tramite codNazione
                    // freak - imposto un valore a frk._codNazione
                    frk._codNazione = istr.CodNazione;

                    try
                    {
                        var sql2 = ctx.Database.SqlQuery<string>("SELECT DES_NAZIONE FROM DNAZIONE WHERE COD_SIGLANAZIONE = '" + frk._codNazione + "'").ToList();
                        if (sql2 != null && sql2.Count > 0) frk._descNazione = sql2[0];
                    }
                    catch
                    {
                        frk._descNazione = "";
                    }
                }
                frk._codTitolo = istr.CodTitolo;
                frk._codTipoTitolo = istr.CodTipoTitolo;
                frk._corsoLaurea = istr.CorsoLaurea;
                frk._dataFine = istr.AnnoFine;
                frk._dataInizio = istr.AnnoInizio;
                frk._dataoraAgg = (DateTime)istr.DataOraAgg;
                frk._durata = istr.Durata;
                frk._indirizzoStudi = (istr.IndirizzoStudi != null) ? istr.IndirizzoStudi : "";

                if (istr.CodIstituto == "-1")
                {
                    frk._istituto = (istr.Istituto != null) ? istr.Istituto : "";
                }
                else
                {
                    frk._istituto = cvEnt.DAteneoCV.Where(a => a.Codice == istr.CodIstituto).Select(x => x.Descrizione).FirstOrDefault();
                }

                frk._localitaStudi = (istr.LocalitaStudi != null) ? istr.LocalitaStudi : "";
                frk._lode = ((istr.Lode == null) || (istr.Lode == " ")) ? ' ' : Convert.ToChar(istr.Lode);
                frk._matricola = istr.Matricola;
                frk._prog = -1;
                frk._scala = istr.Scala;
                frk._stato = Convert.ToChar(istr.Stato);
                frk._tipoAgg = Convert.ToChar(istr.TipoAgg);
                frk._titoloSpecializ = null;
                frk._titoloTesi = istr.TitoloTesi;
                frk._voto = istr.Voto;
                frk._riconoscimento = "";
                /*
                 * FREAK - Aggiungere il campo ***_codIstituto***
                 * */
                frk._codIstituto = istr.CodIstituto;
                frk._tableTarget = "I";
                cvBox.curricula.Add(frk);
            }

            foreach (TCVSpecializz spec in specializz)
            {
                cvModel.Studies frk = new cvModel.Studies( CommonManager.GetCurrentUserMatricola( ) );
                //freak - riempire i campi
                using (var ctx = new cv_ModelEntities())
                {
                    var param = new SqlParameter("@param", spec.TipoSpecial);

                    List<Utente.CV_DescTitoloLogo> tmp = ctx.Database.SqlQuery<Utente.CV_DescTitoloLogo>("exec sp_GETDESCTITOLO @param", param).ToList();
                    frk._descTipoTitolo = tmp[0].DescTipoTitolo; //tmp.GetValue(0,0).ToString();
                    frk._descTitolo = tmp[0].DescTitolo;//tmp.GetValue(0, 1).ToString();
                    frk._logo = tmp[0].Logo;//tmp.GetValue(0, 2).ToString();
                    if (spec.TipoSpecial == "999")
                    {
                        frk._descTipoTitolo = "Specializzazione";
                        frk._logo = "Master";
                    }

                    //recupero descNazione tramite codNazione
                    // freak - imposto un valore a frk._codNazione
                    frk._codNazione = spec.CodNazione;
                    try
                    {
                        var sql2 = ctx.Database.SqlQuery<string>("SELECT DES_NAZIONE FROM DNAZIONE WHERE COD_SIGLANAZIONE = '" + frk._codNazione + "'").ToList();
                        if (sql2 != null && sql2.Count > 0) frk._descNazione = sql2[0];
                    }
                    catch
                    {
                        frk._descNazione = "";
                    }
                }
                frk._codTitolo = spec.TipoSpecial;
                frk._corsoLaurea = "";// spec.Titolo; //freak - da controllare con Massimo Tesoro

                if (spec.DataFine != null)
                    frk._dataFine = spec.DataFine.Substring(0, 4);

                if (spec.DataInizio != null)
                    frk._dataInizio = spec.DataInizio.Substring(0, 4);

                frk._dataoraAgg = (DateTime)spec.DataOraAgg;
                frk._durata = spec.Durata;
                frk._indirizzoStudi = (spec.IndirizzoSpecial != null) ? spec.IndirizzoSpecial : "";
                frk._istituto = (spec.Istituto != null) ? spec.Istituto : "";
                frk._localitaStudi = (spec.LocalitaSpecial != null) ? spec.LocalitaSpecial : "";
                frk._lode = ((spec.Lode == null) || (spec.Lode == " ")) ? ' ' : Convert.ToChar(spec.Lode);
                frk._matricola = spec.Matricola;
                frk._prog = spec.Prog;
                frk._scala = spec.Scala;
                frk._stato = Convert.ToChar(spec.Stato);
                frk._tipoAgg = Convert.ToChar(spec.TipoAgg);
                frk._titoloSpecializ = spec.Titolo;
                frk._titoloTesi = ""; //freak - da controllare con Massimo Tesoro
                frk._voto = spec.Voto;
                /*
                 * FREAK - Aggiungere il campo ***_codIstituto***
                 * */
                frk._codIstituto = spec.CodIstituto;
                frk._tableTarget = "S";

                cvBox.curricula.Add(frk);
            }

            ViewBag.idMenu = 17;

            cvBox.curricula = cvBox.curricula.OrderByDescending(x => x.DataConseguimento ?? DateTime.MaxValue).ToList();

            return View("~/Views/CV_Online/studies/BoxView.cshtml", cvBox);
        }

        [HttpPost]
        public ActionResult InsertCV(cvModel.Studies curricula)
        {
            return this.SaveDiploma(curricula);
        }

        private List<TCVBox_V2> GetListaBox(cv_ModelEntities cvEnt)
        {
            //freak - creare la lista da poi ciclare per creare i box
            string figurapro;
            figurapro = Utente.EsponiAnagrafica()._codiceFigProf;
            var innerJoinQuery =
            (from box in cvEnt.TCVBox_V2
             join boxDettaglio in cvEnt.TCVBox_Figuraprof_V2 on box.Id_box equals boxDettaglio.Id_box
             where (boxDettaglio.CodiceFiguraPro == figurapro)
             orderby boxDettaglio.Posizione
             select box).ToList();

            List<TCVBox_V2> completa = new List<TCVBox_V2>();
            int count = innerJoinQuery.Count();
            if (count > 0)
            {
                completa = innerJoinQuery; //se il count > 0 prendo come riferimento la selezione con l'inner join
            }
            else
            {
                //lista completa della tabella TCVBox_V2 con posizione != null
                var lista = (cvEnt.TCVBox_V2.Where(pos => pos.Posizione != null).OrderBy(d => d.Posizione)).ToList();
                completa = lista;
            }

            return completa;
        }

        [HttpGet]
        public ActionResult DeleteStudiesMaster(string matricola, int prog)
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities();

            if ((matricola == null) && (prog < 0))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TCVSpecializz master = cvEnt.TCVSpecializz.Find(matricola, prog);
            if (master == null)
            {
                return HttpNotFound();
            }
            try
            {
                cvEnt.TCVSpecializz.Remove(master);
                if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                    throw new Exception( "Errore salvataggio dati DB" );
            }
            catch (Exception exc)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.idMenu = 17;
            return Content("Ok");
        }

        [HttpGet]
        public ActionResult DeleteStudiesDiplomaLaurea(string matricola, string codTitolo)
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities();

            if ((matricola == null) && (codTitolo == null))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TCVIstruzione laurea_diploma = cvEnt.TCVIstruzione.Find(matricola, codTitolo);
            if (laurea_diploma == null)
            {
                return HttpNotFound();
            }
            try
            {
                cvEnt.TCVIstruzione.Remove(laurea_diploma);
                if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                    throw new Exception( "Errore salvataggio dati DB" );
            }
            catch (Exception exc)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadGateway);
            }

            ViewBag.idMenu = 17;
            return Content("Ok");
        }

        /// <summary>
        /// Controller per Modifica del CV Studies (Laurea; Master; Diploma)
        /// </summary>
        /// <param name="curricula"></param>
        /// <returns></returns>
        public ActionResult ModificaStudies( cvModel.Studies curricula )
        {
            string logo;
            string matricola, codTitolo;
            string data_inizio, data_fine, frk_tmp;
            int prog;

            logo = curricula._logo;
            matricola = curricula._matricola;
            codTitolo = curricula._codTitolo;
            prog = curricula._prog;
            if (logo == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ModelState.Clear();
            var db = new cv_ModelEntities();
            switch (logo)
            {
                case "Diploma":
                    this.GetIstituti("DI", curricula._codTitolo);
                    return View("~/Views/CV_Online/studies/ViewDiploma.cshtml", curricula);
                case "Laurea":
                    if ((matricola == null) || (codTitolo == null))
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }

                    var ist = db.TCVIstruzione.Where(x => x.Matricola == curricula._matricola && x.CodTitolo == curricula._codTitolo)
                            .FirstOrDefault();
                    if (ist != null)
                    {
                        curricula._riconoscimento = ist.Riconoscimento;
                        curricula._lode = ist.Lode.FirstOrDefault();
                    }

                    return View("~/Views/CV_Online/studies/ViewLaurea.cshtml", curricula);
                case "Master":
                    frk_tmp = curricula._dataInizio;
                    if (frk_tmp == null)
                    {
                        data_inizio = "";
                    }
                    else
                    {
                        data_inizio = frk_tmp;
                    }
                    frk_tmp = curricula._dataFine;
                    if (frk_tmp == null)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                    }
                    else
                    {
                        data_fine = frk_tmp;
                    }

                    curricula._dataInizio = data_inizio;
                    curricula._dataFine = data_fine;


                    var spec = db.TCVSpecializz.Where(x => x.Matricola == curricula._matricola && x.Prog == curricula._prog)
                            .FirstOrDefault();
                    if (spec != null)
                    {
                        curricula._riconoscimento = spec.Riconoscimento;
                        curricula._lode = spec.Lode.FirstOrDefault();
                        curricula._note = spec.Note;
                    }
                    this.GetIstituti("MA", curricula._codTitolo);
                    return View("~/Views/CV_Online/studies/ViewMaster.cshtml", curricula);
                default:
                    return HttpNotFound();
            }
        }

        /// <summary>
        /// modifica del Currivulum
        /// </summary>
        /// <param name="curricula"></param>
        /// <param name="PKvalueOLD"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ModificaCV(cvModel.Studies curricula, string PKvalueOLD)
        {
            bool isScala, isDataFine;
            string result = "";
            isScala = false;
            isDataFine = false;

            if (ModelState.IsValid)
            {
                string table;
                string matricola;
                string codTitolo;
                cv_ModelEntities cvEnt = new cv_ModelEntities();

                matricola = Utente.EsponiAnagrafica()._matricola;
                codTitolo = curricula._codTitolo;
                table = curricula._tableTarget;

                switch (table)
                {
                    case "I":
                        string codTitoloOld = PKvalueOLD;
                        TCVIstruzione istruzioneDelete = cvEnt.TCVIstruzione.Find(matricola, PKvalueOLD);
                        try
                        {
                            cvEnt.TCVIstruzione.Remove(istruzioneDelete);
                            if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                                throw new Exception( "Errore salvataggio dati DB" );
                        }
                        catch (Exception ex)
                        {

                        }

                        TCVIstruzione istruzione = new TCVIstruzione();
                        istruzione.Matricola = matricola;
                        istruzione.CodTipoTitolo = curricula._codTipoTitolo;
                        istruzione.CodTitolo = curricula._codTitolo;
                        istruzione.AnnoInizio = curricula._dataInizio;
                        istruzione.AnnoFine = curricula._dataFine;
                        istruzione.Scala = curricula._scala;
                        istruzione.Voto = curricula._voto;
                        istruzione.Lode = (curricula._lode == 'S') ? "S" : "N";
                        istruzione.Durata = "";
                        istruzione.Istituto = curricula._istituto;
                        istruzione.CorsoLaurea = curricula._corsoLaurea;
                        istruzione.TitoloTesi = curricula._titoloTesi;
                        istruzione.Stato = "S";
                        istruzione.TipoAgg = "A";
                        istruzione.DataOraAgg = DateTime.Now;
                        istruzione.LocalitaStudi = curricula._localitaStudi;
                        istruzione.CodNazione = curricula._codNazione;
                        istruzione.IndirizzoStudi = curricula._indirizzoStudi;
                        istruzione.Riconoscimento = curricula._riconoscimento;
                        istruzione.CodIstituto = curricula._codIstituto;

                        try
                        {
                            cvEnt.TCVIstruzione.Add(istruzione);
                            if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                                throw new Exception( "Errore salvataggio dati DB" );
                            result = "ok";
                        }
                        catch (Exception exc)
                        {
                            result = exc.Message;
                        }

                        break;
                    case "S":
                        //inseirmento in TCVSpecializz
                        string[] data_tmp;
                        string data_inizio, data_fine;
                        int prog;
                        int progOld;

                        //FREAK: Gestione modifica --- ricerca elemento OLD, cancellarlo e reinserimento nuovo elemento
                        progOld = Convert.ToInt32(PKvalueOLD);

                        TCVSpecializz specializzDelete = cvEnt.TCVSpecializz.Find(matricola, progOld);

                        try
                        {
                            cvEnt.TCVSpecializz.Remove(specializzDelete);
                            if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                                throw new Exception( "Errore salvataggio dati DB" );
                        }
                        catch (Exception ex)
                        {
                        }

                        data_tmp = curricula._dataInizio.Split('/');
                        if (data_tmp.Count() == 3)
                        {
                            data_inizio = data_tmp[2] + data_tmp[1] + data_tmp[0];
                        }
                        else
                        {
                            data_inizio = "";
                        }

                        data_tmp = curricula._dataFine.Split('/');
                        if (data_tmp.Count() == 3)
                        {
                            data_fine = data_tmp[2] + data_tmp[1] + data_tmp[0];
                        }
                        else
                        {
                            result = "data";
                            return Content(result);
                        }

                        TCVSpecializz specializz = new TCVSpecializz();

                        //rieempimento di specializz
                        var tmp = cvEnt.TCVSpecializz.Where(x => x.Matricola == matricola);
                        if (tmp.Count() == 0)
                        {
                            prog = 1;
                        }
                        else
                        {
                            var nro_prog = (cvEnt.TCVSpecializz.Where(x => x.Matricola == matricola)).Max(x => x.Prog);
                            prog = Convert.ToInt32(nro_prog) + 1;
                        }

                        specializz.DataInizio = data_inizio;
                        specializz.DataFine = data_fine;
                        specializz.Stato = "S";
                        specializz.TipoAgg = "A";
                        specializz.Matricola = matricola;
                        specializz.Durata = "";
                        specializz.TipoSpecial = curricula._codTitolo;
                        specializz.Titolo = curricula._titoloSpecializ;
                        specializz.Istituto = curricula._istituto;
                        specializz.Voto = curricula._voto;
                        specializz.Scala = curricula._scala;
                        specializz.Note = curricula._note;
                        specializz.Lode = (curricula._lode == 'S') ? "S" : "N";
                        specializz.LocalitaSpecial = curricula._localitaStudi;
                        specializz.CodNazione = curricula._codNazione;
                        specializz.IndirizzoSpecial = curricula._indirizzoStudi;
                        specializz.Riconoscimento = curricula._riconoscimento;
                        specializz.DataOraAgg = DateTime.Now;
                        specializz.Prog = prog;
                        specializz.CodIstituto = curricula._codIstituto;

                        try
                        {
                            cvEnt.TCVSpecializz.Add(specializz);
                            if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                                throw new Exception( "Errore salvataggio dati DB" );
                            result = "ok";
                        }
                        catch (Exception exc)
                        {
                            result = exc.Message;
                        }
                        break;
                    default:
                        result = "error";
                        break;
                }
            }
            else
            {
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var elem in modelState.Errors)
                    {
                        switch (elem.ErrorMessage)
                        {
                            case "scala":
                                isScala = true;
                                break;
                            case "datafine":
                                isDataFine = true;
                                break;
                            default:
                                break;
                        }
                    }
                }
                if (isScala)
                {
                    result += "scala";
                }
                if (isDataFine)
                {
                    result += "datafine";
                }
            }
            return Content(result);
        }

        /// <summary>
        /// Reperimento degli istituti in base alla tipologia, Master, Laurea, Diploma
        /// </summary>
        /// <param name="tipoIstituto">Possibili valori: MA, </param>
        /// <param name="selectedItem"></param>
        private void GetIstituti(string tipoIstituto, string selectedItem = null)
        {
            string matricola = Utente.EsponiAnagrafica()._matricola;
            List<DTitolo> results = new List<DTitolo>();
            try
            {
                using (cv_ModelEntities db = new cv_ModelEntities())
                {
                    results = (from titolo in db.DTitolo
                               where titolo.CodTipoTitolo.Equals(tipoIstituto, StringComparison.InvariantCultureIgnoreCase)
                               select titolo).ToList();

                    results = results.OrderBy(c => c.DescTitolo).ToList();
                }

                var institutes = new List<Lookup>( );
                // Inserimento dell'elemento vuoto
                institutes.Add(new Lookup()
                {
                    Id = new Nullable<int>(),
                    Codice = null,
                    Description = null
                });

                if (results != null && results.Any())
                {
                    results.ForEach(d =>
                    {
                        bool selected = false;
                        if (!String.IsNullOrEmpty(selectedItem))
                        {
                            if (selectedItem.Equals(d.CodTitolo, StringComparison.InvariantCultureIgnoreCase))
                                selected = true;
                        }

                        institutes.Add(new Lookup()
                        {
                            Codice = d.CodTitolo,
                            Description = d.DescTitolo,
                            Id = new Nullable<int>(),
                            Selected = selected
                        });
                    });
                }

                ViewBag.Institutes = institutes;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Visualizzazione della view in modifica
        /// </summary>
        /// <param name="codTitolo">Codice dell'elemento per il quale si intende effettuare la modifica</param>
        /// <param name="tableTarget"></param>
        /// <param name="progressivo"></param>
        /// <returns></returns>
        public ActionResult ModificaStudi(string codTitolo, string tableTarget, int progressivo)
        {
            try
            {
                // reperimento della matricola dell'utente chiamante
                string matricola = Utente.EsponiAnagrafica()._matricola;

                cvModel.Studies result = null;

                if (String.IsNullOrEmpty(codTitolo))
                    throw new Exception("Dati mancanti. Impossibile reperire i dati.");

                if (tableTarget.Equals("I", StringComparison.InvariantCultureIgnoreCase))
                {
                    result = this.GetStudi(codTitolo);
                }
                else if (tableTarget.Equals("S", StringComparison.InvariantCultureIgnoreCase))
                {
                    result = this.GetStudiMaster(codTitolo, progressivo);
                }
                else
                {
                    throw new Exception("Impossibile reperire i dati. Paramentri mancanti.");
                }

                return View("~/Views/CV_Online/studies/ViewStudies.cshtml", result);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Reperimento dei dati per un diploma o Laurea
        /// </summary>
        /// <param name="codice"></param>
        /// <returns></returns>
		private cvModel.Studies GetStudi(string codice)
        {
            try
            {
                cvModel.Studies result = null;
                // reperimento della matricola dell'utente chiamante
                string matricola = Utente.EsponiAnagrafica()._matricola;

                using (cv_ModelEntities db = new cv_ModelEntities())
                {
                    var tipologia = db.DTitolo.Where(t => t.CodTitolo.Equals(codice, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                    if (tipologia == null)
                        throw new Exception("Errore nel reperimento dei dati.\r\nTabella:DTitolo\r\nCodice titolo:" + codice);

                    int livello = -1;
                    Int32.TryParse(tipologia.Livello, out livello);

                    // 90 è il livello assegnato al master
                    if (livello < 90)
                    {
                        var toModifyIst = db.TCVIstruzione.Where(e => e.Matricola.Equals(matricola, StringComparison.InvariantCultureIgnoreCase) &&
                           e.CodTitolo.Equals(codice, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                        if (toModifyIst == null)
                            throw new Exception("Si è verificato un errore nel reperimento dei dati\r\nTabella: TCVIstruzione\r\nCodice:" + codice);

                        if (String.IsNullOrEmpty(toModifyIst.CodTipoTitolo))
                        {
                            toModifyIst.CodTipoTitolo = tipologia.CodTipoTitolo;
                        }

                        result = new cvModel.Studies( CommonManager.GetCurrentUserMatricola( ) )
                        {
                            _codIstituto = toModifyIst.CodIstituto ,
                            _codNazione = toModifyIst.CodNazione ,
                            _codTipoTitolo = toModifyIst.CodTipoTitolo ,
                            _codTitolo = toModifyIst.CodTitolo ,
                            _corsoLaurea = toModifyIst.CorsoLaurea ,
                            _dataFine = toModifyIst.AnnoFine ,
                            _dataInizio = toModifyIst.AnnoInizio ,
                            _dataoraAgg = toModifyIst.DataOraAgg ,
                            _descNazione = null ,
                            _descTipoTitolo = null ,
                            _descTitolo = null ,
                            _durata = toModifyIst.Durata ,
                            _indirizzoStudi = toModifyIst.IndirizzoStudi ,
                            _istituto = toModifyIst.Istituto ,
                            _localitaStudi = toModifyIst.LocalitaStudi ,
                            _lode = !String.IsNullOrEmpty( toModifyIst.Lode ) ? toModifyIst.Lode[0] : new Nullable<char>( ) ,
                            _logo = null ,
                            _matricola = toModifyIst.Matricola ,
                            _note = null ,
                            _prog = -1 ,
                            _riconoscimento = toModifyIst.Riconoscimento ,
                            _scala = toModifyIst.Scala ,
                            _stato = ( !String.IsNullOrEmpty( toModifyIst.Stato ) ? toModifyIst.Stato[0] : new char( ) ) ,
                            _tableTarget = "I" ,
                            _tipoAgg = ( !String.IsNullOrEmpty( toModifyIst.TipoAgg ) ? toModifyIst.TipoAgg[0] : new char( ) ) ,
                            _titoloSpecializ = null ,
                            _titoloTesi = toModifyIst.TitoloTesi ,
                            _voto = toModifyIst.Voto ,
                            OldCodTitolo = toModifyIst.CodTitolo
                        };
                    }
                    else
                    {
                        throw new Exception("Si è verificato un errore nel reperimento dei dati");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Reperimento dei dati per una specializzazione
        /// </summary>
        /// <param name="codice"></param>
		/// <param name="progressivo"></param>
        /// <returns></returns>
		private cvModel.Studies GetStudiMaster(string codice, int progressivo)
        {
            try
            {
                cvModel.Studies result = null;
                // reperimento della matricola dell'utente chiamante
                string matricola = Utente.EsponiAnagrafica()._matricola;

                using (cv_ModelEntities db = new cv_ModelEntities())
                {

                    // Questo perchè su db nella tabella [TCVSpecializz] la colonna [TipoSpecial]
                    // va sistemata, al suo interno ci sono codici anche di una sola cifra ad esempio "2"
                    // mentre la colonna dovrebbe essere con valori di 3 cifre es: "002".
                    // Se così non fosse al momento della renderizzazione della view di edit il campo relativo
                    // alla tipologia Es: master universitario non sarebbe valorizzato correttamente.
                    if (codice.Length < 3)
                    {
                        int code = -1;
                        Int32.TryParse(codice, out code);
                        codice = String.Format("{0:000}", code);
                    }

                    var specializzazione = db.TCVSpecializz.Where(
                        e => e.Matricola.Equals(matricola, StringComparison.InvariantCultureIgnoreCase) &&
                            e.TipoSpecial.Equals(codice, StringComparison.InvariantCultureIgnoreCase) &&
                            e.Prog.Equals(progressivo)).FirstOrDefault();

                    if (specializzazione == null)
                        throw new Exception("Si è verificato un errore nel reperimento dei dati\r\nTabella:TCVSpecializz\r\nTipoSecial:" + codice + "\r\nProg:" + progressivo);

                    result = new cvModel.Studies( CommonManager.GetCurrentUserMatricola( ) )
                    {
                        _codIstituto = specializzazione.CodIstituto ,
                        _codNazione = specializzazione.CodNazione ,
                        _codTipoTitolo = "MA" ,
                        _codTitolo = codice ,
                        _dataoraAgg = specializzazione.DataOraAgg ,
                        _descNazione = null ,
                        _descTipoTitolo = null ,
                        _descTitolo = null ,
                        _durata = specializzazione.Durata ,
                        _indirizzoStudi = specializzazione.IndirizzoSpecial ,
                        _istituto = specializzazione.Istituto ,
                        _localitaStudi = specializzazione.LocalitaSpecial ,
                        _lode = !String.IsNullOrEmpty( specializzazione.Lode ) ? specializzazione.Lode[0] : new Nullable<char>( ) ,
                        _logo = null ,
                        _matricola = specializzazione.Matricola ,
                        _note = specializzazione.Note ,
                        _prog = specializzazione.Prog ,
                        _riconoscimento = specializzazione.Riconoscimento ,
                        _scala = specializzazione.Scala ,
                        _stato = ( !String.IsNullOrEmpty( specializzazione.Stato ) ? specializzazione.Stato[0] : new char( ) ) ,
                        _tableTarget = "S" ,
                        _tipoAgg = ( !String.IsNullOrEmpty( specializzazione.TipoAgg ) ? specializzazione.TipoAgg[0] : new char( ) ) ,
                        _titoloSpecializ = specializzazione.Titolo ,
                        _titoloTesi = null ,
                        _voto = specializzazione.Voto
                    };

                    // data inizio e data fine vanno modificati dal formato yyyymmdd al formato dd/mm/yyyy
                    string newDateStart = "";
                    string newDateStop = "";

                    if (specializzazione.DataInizio != null)
                        newDateStart = specializzazione.DataInizio.Substring(0, 4);
                    if (specializzazione.DataFine != null)
                        newDateStop = specializzazione.DataFine.Substring(0, 4);

                    result._dataInizio = newDateStart;
                    result._dataFine = newDateStop;
                }
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Reperimento dell'elenco delle università in base al tipo di laurea selezionata
        /// </summary>
        /// <param name="tipoLaurea"></param>
        /// <returns></returns>
        public ActionResult GetLauree(string tipoLaurea)
        {
            try
            {
                if (String.IsNullOrEmpty(tipoLaurea))
                    throw new Exception("Tipo laurea non trovato");

                cvModel.Studies _myModel = new cvModel.Studies( CommonManager.GetCurrentUserMatricola( ) );
                _myModel._codTipoTitolo = tipoLaurea;

                return Json(_myModel.Lauree, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #region Salvataggio dati diploma

        /// <summary>
        /// Metodo per il salvataggio di un diploma
        /// </summary>
        /// <param name="diploma">Dati del diploma da salvare</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveDiploma(cvModel.Studies diploma)
        {
            try
            {
                // matricola dell'utente corrente
                string matricola = Utente.EsponiAnagrafica()._matricola;

                // Imposta il codice tipo titolo a Diploma di maturità
                diploma._codTipoTitolo = "DI";

                // verifica del modello
                if (this.IsValidDiplomaModel(diploma))
                {
                    bool insertMode = true;

                    // verifica se l'elemento è da considerarsi in modalità inserimento o modifica
                    using (cv_ModelEntities db = new cv_ModelEntities())
                    {
                        // Se OldCodTitolo è nullo allora siamo in inserimento
                        if (diploma.OldCodTitolo == null)
                        {
                            var element = db.TCVIstruzione.Where(e => e.Matricola.Equals(matricola, StringComparison.InvariantCultureIgnoreCase) && e.CodTitolo.Equals(diploma._codTitolo, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                            if (element != null)
                                insertMode = false;
                        }   // se in modalità modifica ed il codice titolo è lo stesso allora è un update
                        else if (diploma.OldCodTitolo.Equals(diploma._codTitolo, StringComparison.InvariantCultureIgnoreCase))
                        {
                            var element = db.TCVIstruzione.Where(e => e.Matricola.Equals(matricola, StringComparison.InvariantCultureIgnoreCase) && e.CodTitolo.Equals(diploma._codTitolo, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                            if (element != null)
                                insertMode = false;
                        }
                        else
                        {
                            // se OldCodTitolo è valorizzato, ma è diverso da _codTitolo, allora è un update
                            // ma essendo cambiata la chiave si dovrà procedere con la cancellazione del record 
                            // corrente e con l'inserimento di uno nuovo.
                            var element = db.TCVIstruzione.Where(e => e.Matricola.Equals(matricola, StringComparison.InvariantCultureIgnoreCase) && e.CodTitolo.Equals(diploma.OldCodTitolo, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                            if (element != null)
                            {
                                db.TCVIstruzione.Remove(element);
                                insertMode = true;
                                //db.SaveChanges();
                                if ( !DBHelper.Save( db , CommonManager.GetCurrentUserMatricola( ) ) )
                                    throw new Exception( "Errore salvataggio dati db" );
                            }
                        }
                    }

                    if (insertMode)
                        this.InsertDiploma(diploma);
                    else
                        this.UpdateDiploma(diploma);

                    return Content("ok");
                }
                else
                {
                    return Content("Alcuni dati risultano non corretti o assenti");
                }
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        /// <summary>
        /// Metodo per la verifica dell'integrità dei dati per l'inserimento o aggiornamento di un diploma
        /// </summary>
        /// <param name="diploma"></param>
        /// <returns></returns>
        private bool IsValidDiplomaModel(cvModel.Studies diploma)
        {
            try
            {
                bool isValid = true;

                if (String.IsNullOrEmpty(diploma._codTitolo))
                    isValid = false;

                if (String.IsNullOrEmpty(diploma._istituto))
                    isValid = false;

                if (String.IsNullOrEmpty(diploma._dataFine))
                    isValid = false;

                if (String.IsNullOrEmpty(diploma._dataInizio))
                    isValid = false;

                if (String.IsNullOrEmpty(diploma._codTipoTitolo))
                    isValid = false;

                return isValid;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Inserimento di un nuovo diploma
        /// </summary>
        /// <param name="diploma"></param>
        private void InsertDiploma(cvModel.Studies diploma)
        {
            try
            {
                string matricola = Utente.EsponiAnagrafica()._matricola;

                //inserimento in TCVIstruzione
                TCVIstruzione toInsert = new TCVIstruzione();

                toInsert.Matricola = matricola;
                toInsert.CodTipoTitolo = diploma._codTipoTitolo;
                toInsert.CodTitolo = diploma._codTitolo;
                toInsert.AnnoInizio = diploma._dataInizio;
                toInsert.AnnoFine = diploma._dataFine;
                toInsert.Scala = diploma._scala;
                toInsert.Voto = diploma._voto;
                toInsert.Istituto = diploma._istituto;
                toInsert.Stato = "S";
                toInsert.TipoAgg = "I";
                toInsert.DataOraAgg = DateTime.Now;
                toInsert.LocalitaStudi = diploma._localitaStudi;
                toInsert.CodNazione = diploma._codNazione;
                toInsert.IndirizzoStudi = diploma._indirizzoStudi;
                toInsert.Riconoscimento = diploma._riconoscimento;
                toInsert.CodIstituto = null;
                toInsert.Lode = null;
                toInsert.Durata = null;
                toInsert.CorsoLaurea = null;
                toInsert.TitoloTesi = null;

                using ( cv_ModelEntities db = new cv_ModelEntities( ) )
                {
                    db.TCVIstruzione.Add( toInsert );
                    if ( !DBHelper.Save( db , CommonManager.GetCurrentUserMatricola( ) ) )
                        throw new Exception( "Errore salvataggio dati db" );
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Modifica dei dati di un diploma esistente
        /// </summary>
        /// <param name="diploma"></param>
        private void UpdateDiploma(cvModel.Studies diploma)
        {
            try
            {
                string matricola = Utente.EsponiAnagrafica()._matricola;

                using (cv_ModelEntities db = new cv_ModelEntities())
                {
                    var toUpdate = db.TCVIstruzione.Where(itm => itm.Matricola.Equals(matricola, StringComparison.CurrentCultureIgnoreCase) &&
                                            itm.CodTitolo.Equals(diploma._codTitolo)).FirstOrDefault();

                    if (toUpdate == null)
                        throw new Exception("Diploma non trovato");

                    toUpdate.CodTipoTitolo = diploma._codTipoTitolo;
                    toUpdate.CodTitolo = diploma._codTitolo;
                    toUpdate.AnnoInizio = diploma._dataInizio;
                    toUpdate.AnnoFine = diploma._dataFine;
                    toUpdate.Scala = diploma._scala;
                    toUpdate.Voto = diploma._voto;
                    toUpdate.Istituto = diploma._istituto;
                    toUpdate.Stato = "S";
                    toUpdate.TipoAgg = "A";
                    toUpdate.DataOraAgg = DateTime.Now;
                    toUpdate.LocalitaStudi = diploma._localitaStudi;
                    toUpdate.CodNazione = diploma._codNazione;
                    toUpdate.IndirizzoStudi = diploma._indirizzoStudi;
                    toUpdate.Riconoscimento = diploma._riconoscimento;
                    toUpdate.CodIstituto = null;
                    toUpdate.Lode = null;
                    toUpdate.Durata = null;
                    toUpdate.CorsoLaurea = null;
                    toUpdate.TitoloTesi = null;

                    //db.SaveChanges();
                    if ( !DBHelper.Save( db , CommonManager.GetCurrentUserMatricola( ) ) )
                        throw new Exception( "Errore salvataggio dati db" );
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region Salvataggio dati Laurea

        /// <summary>
        /// Metodo per il salvataggio di una laurea
        /// </summary>
        /// <param name="laurea">Dati della laurea da inserire o aggiornare</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveLaurea(cvModel.Studies laurea)
        {
            try
            {
                // matricola dell'utente corrente
                string matricola = Utente.EsponiAnagrafica()._matricola;

                // se ho un codice istituto, ma la descrizione è nulla, allora va recuperata dal DB
                if (!laurea._codIstituto.Equals("-1", StringComparison.InvariantCultureIgnoreCase) &&
                    String.IsNullOrEmpty(laurea._istituto))
                {
                    string _istituto = null;
                    using (cv_ModelEntities db = new cv_ModelEntities())
                    {
                        var element = db.DAteneoCV.Where(a => a.Codice.Equals(laurea._codIstituto, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                        if (element == null)
                            throw new Exception("Si è verificato un errore nel reperimento dell'ateneo");

                        _istituto = element.Descrizione;
                    }

                    laurea._istituto = _istituto;
                }

                if (IsValidLaureaModel(laurea))
                {
                    bool insertMode = true;

                    // verifica se in inserimento o modifica
                    using (cv_ModelEntities db = new cv_ModelEntities())
                    {
                        var element = db.TCVIstruzione.Where(e => e.Matricola.Equals(matricola, StringComparison.InvariantCultureIgnoreCase) &&
                            e.CodTitolo.Equals(laurea._codTitolo, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                        if (element != null)
                            insertMode = false;
                    }

                    if (insertMode)
                        this.InsertLaurea(laurea);
                    else
                        this.UpdateLaurea(laurea);

                    return Content("ok");
                }
                else
                {
                    return Content("Alcuni dati risultano non corretti o assenti");
                }
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        /// <summary>
        /// Metodo per la verifica dell'integrità dei dati per l'inserimento o aggiornamento di una Laurea
        /// </summary>
        /// <param name="diploma"></param>
        /// <returns></returns>
        private bool IsValidLaureaModel(cvModel.Studies laurea)
        {
            try
            {
                bool isValid = true;

                if (String.IsNullOrEmpty(laurea._codIstituto))
                    isValid = false;

                //if (String.IsNullOrEmpty(laurea._codNazione))
                //    isValid = false;

                // se il codice istituto è -1 allora si deve inserire il nuovo istituto, ciò significa che
                // _istituto dovrà essere valorizzato
                if (laurea._codIstituto.Equals("-1", StringComparison.InvariantCultureIgnoreCase) &&
                    String.IsNullOrEmpty(laurea._istituto))
                    isValid = false;

                //if (String.IsNullOrEmpty(laurea._localitaStudi))
                //    isValid = false;

                //if (String.IsNullOrEmpty(laurea._indirizzoStudi))
                //    isValid = false;

                if (String.IsNullOrEmpty(laurea._dataFine))
                    isValid = false;

                if (String.IsNullOrEmpty(laurea._dataInizio))
                    isValid = false;

                //if (String.IsNullOrEmpty(laurea._voto))
                //    isValid = false;

                if (String.IsNullOrEmpty(laurea._codTipoTitolo))
                    isValid = false;

                if (String.IsNullOrEmpty(laurea._corsoLaurea))
                    isValid = false;

                //if (String.IsNullOrEmpty(laurea._scala))
                //    isValid = false;

                //if (String.IsNullOrEmpty(laurea._titoloTesi))
                //    isValid = false;

                return isValid;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Inserimento di un nuovo titolo di studio di tipo laurea
        /// </summary>
        /// <param name="laurea"></param>
        private void InsertLaurea(cvModel.Studies laurea)
        {
            try
            {
                string matricola = Utente.EsponiAnagrafica()._matricola;

                //inserimento in TCVIstruzione
                TCVIstruzione istruzione = new TCVIstruzione();

                istruzione.Matricola = matricola;
                istruzione.CodTipoTitolo = laurea._codTipoTitolo;
                istruzione.CodTitolo = laurea._codTitolo;
                istruzione.AnnoInizio = laurea._dataInizio;
                istruzione.AnnoFine = laurea._dataFine;
                istruzione.Scala = laurea._scala;
                istruzione.Voto = laurea._voto;
                istruzione.Lode = (laurea._lode == 'S') ? "S" : "N";
                istruzione.Istituto = laurea._istituto;
                istruzione.CorsoLaurea = laurea._corsoLaurea;
                istruzione.TitoloTesi = laurea._titoloTesi;
                istruzione.Stato = "S";
                istruzione.TipoAgg = "I";
                istruzione.DataOraAgg = DateTime.Now;
                istruzione.LocalitaStudi = laurea._localitaStudi;
                istruzione.CodNazione = laurea._codNazione;
                istruzione.IndirizzoStudi = laurea._indirizzoStudi;
                istruzione.Riconoscimento = laurea._riconoscimento;
                istruzione.CodIstituto = laurea._codIstituto;
                istruzione.Durata = null;

                using (cv_ModelEntities db = new cv_ModelEntities())
                {
                    db.TCVIstruzione.Add(istruzione);
                    //db.SaveChanges();
                    if ( !DBHelper.Save( db , CommonManager.GetCurrentUserMatricola( ) ) )
                        throw new Exception( "Errore salvataggio dati db" );
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Aggiornamento dei dati relativi ad una laurea
        /// </summary>
        /// <param name="laurea"></param>
        private void UpdateLaurea(cvModel.Studies laurea)
        {
            try
            {
                string matricola = Utente.EsponiAnagrafica()._matricola;

                using (cv_ModelEntities db = new cv_ModelEntities())
                {
                    var toUpdate = db.TCVIstruzione.Where(itm => itm.Matricola.Equals(matricola, StringComparison.CurrentCultureIgnoreCase) &&
                                            itm.CodTitolo.Equals(laurea._codTitolo)).FirstOrDefault();

                    if (toUpdate == null)
                        throw new Exception("Laurea non trovata");

                    toUpdate.CodTipoTitolo = laurea._codTipoTitolo;
                    toUpdate.CodTitolo = laurea._codTitolo;
                    toUpdate.AnnoInizio = laurea._dataInizio;
                    toUpdate.AnnoFine = laurea._dataFine;
                    toUpdate.Scala = laurea._scala;
                    toUpdate.Voto = laurea._voto;
                    toUpdate.Lode = (laurea._lode == 'S') ? "S" : "N";
                    toUpdate.Istituto = laurea._istituto;
                    toUpdate.CorsoLaurea = laurea._corsoLaurea;
                    toUpdate.TitoloTesi = laurea._titoloTesi;
                    toUpdate.Stato = "S";
                    toUpdate.TipoAgg = "A";
                    toUpdate.DataOraAgg = DateTime.Now;
                    toUpdate.LocalitaStudi = laurea._localitaStudi;
                    toUpdate.CodNazione = laurea._codNazione;
                    toUpdate.IndirizzoStudi = laurea._indirizzoStudi;
                    toUpdate.Riconoscimento = laurea._riconoscimento;
                    toUpdate.CodIstituto = laurea._codIstituto;
                    toUpdate.Durata = null;

                    //db.SaveChanges();
                    if ( !DBHelper.Save( db , CommonManager.GetCurrentUserMatricola( ) ) )
                        throw new Exception( "Errore salvataggio dati db" );
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region Salvataggio dati Master (Specializzazione)

        /// <summary>
        /// Salvataggio dei dati di un Master/Specializzazione
        /// </summary>
        /// <param name="master"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SaveMaster(cvModel.Studies master)
        {
            try
            {
                // matricola dell'utente corrente
                string matricola = Utente.EsponiAnagrafica()._matricola;

                // se ho un codice istituto, ma la descrizione è nulla, allora va recuperata dal DB
                if (!master._codIstituto.Equals("-1", StringComparison.InvariantCultureIgnoreCase) &&
                    String.IsNullOrEmpty(master._istituto))
                {
                    string _istituto = null;
                    using (cv_ModelEntities db = new cv_ModelEntities())
                    {
                        var element = db.DAteneoCV.Where(a => a.Codice.Equals(master._codIstituto, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                        if (element == null)
                            throw new Exception("Si è verificato un errore nel reperimento dell'ateneo");

                        _istituto = element.Descrizione;
                    }
                    master._istituto = _istituto;
                }

                if (IsValidMasterModel(master))
                {
                    bool insertMode = true;

                    // verifica se in inserimento o modifica
                    using (cv_ModelEntities db = new cv_ModelEntities())
                    {
                        var element = db.TCVSpecializz.Where(e => e.Matricola.Equals(matricola, StringComparison.InvariantCultureIgnoreCase) &&
                            //e.CodIstituto.Equals(master._codIstituto, StringComparison.InvariantCultureIgnoreCase) &&
                            e.Prog == master._prog).FirstOrDefault();

                        if (element != null)
                            insertMode = false;
                    }

                    if (insertMode)
                        this.InsertMaster(master);
                    else
                        this.UpdateMaster(master);

                    return Content("ok");
                }
                else
                {
                    return Content("Alcuni dati risultano non corretti o assenti");
                }
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        /// <summary>
        /// Metodo per la verifica dell'integrità dei dati per l'inserimento o aggiornamento di un master
        /// </summary>
        /// <param name="master"></param>
        /// <returns></returns>
        private bool IsValidMasterModel(cvModel.Studies master)
        {
            try
            {
                bool isValid = true;

                if (String.IsNullOrEmpty(master._codIstituto))
                    isValid = false;

                // se il codice istituto è -1 allora si deve inserire il nuovo istituto, ciò significa che
                // _istituto dovrà essere valorizzato
                if (master._codIstituto.Equals("-1", StringComparison.InvariantCultureIgnoreCase) &&
                    String.IsNullOrEmpty(master._istituto))
                    isValid = false;

                if (String.IsNullOrEmpty(master._dataFine))
                    isValid = false;

                if (String.IsNullOrEmpty(master._dataInizio))
                    isValid = false;

                return isValid;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Inserimento di un nuovo Master associato all'utente corrente
        /// </summary>
        /// <param name="master"></param>
        private void InsertMaster(cvModel.Studies master)
        {
            try
            {
                string matricola = Utente.EsponiAnagrafica()._matricola;

                //inseirmento in TCVSpecializz
                TCVSpecializz specializz = new TCVSpecializz();
                string[] data_tmp;
                string data_inizio = "", data_fine = "";
                int prog;

                if (master._dataInizio != "")
                    data_inizio = master._dataInizio + "0101";
                if (master._dataFine != "")
                    data_fine = master._dataFine + "0101";

                using (cv_ModelEntities db = new cv_ModelEntities())
                {
                    var tmp = db.TCVSpecializz.Where(x => x.Matricola.Equals(matricola, StringComparison.InvariantCultureIgnoreCase));

                    if (tmp.Count() == 0)
                    {
                        prog = 1;
                    }
                    else
                    {
                        var nro_prog = (db.TCVSpecializz.Where(x => x.Matricola.Equals(matricola, StringComparison.InvariantCultureIgnoreCase))).Max(x => x.Prog);
                        prog = Convert.ToInt32(nro_prog) + 1;
                    }

                    //rieempimento di specializz
                    specializz.DataInizio = data_inizio;
                    specializz.DataFine = data_fine;
                    specializz.Stato = "S";
                    specializz.TipoAgg = "I";
                    specializz.Matricola = matricola;
                    specializz.Durata = null;
                    specializz.TipoSpecial = master._codTitolo;
                    specializz.Titolo = master._titoloSpecializ;
                    specializz.Istituto = master._istituto;
                    specializz.Voto = master._voto;
                    specializz.Scala = master._scala;
                    specializz.Note = master._note;
                    specializz.Lode = (master._lode == 'S') ? "S" : "N";
                    specializz.LocalitaSpecial = master._localitaStudi;
                    specializz.CodNazione = master._codNazione;
                    specializz.IndirizzoSpecial = master._indirizzoStudi;
                    specializz.Riconoscimento = master._riconoscimento;
                    specializz.DataOraAgg = DateTime.Now;
                    specializz.Prog = prog;
                    specializz.CodIstituto = master._codIstituto;

                    db.TCVSpecializz.Add(specializz);
                    //db.SaveChanges();
                    if ( !DBHelper.Save( db , CommonManager.GetCurrentUserMatricola( ) ) )
                        throw new Exception( "Errore salvataggio dati db" );
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// Aggiornamento dati di un master
        /// </summary>
        /// <param name="master"></param>
        private void UpdateMaster(cvModel.Studies master)
        {
            try
            {
                string[] data_tmp;
                string data_inizio = "", data_fine = "";
                int prog;
                string matricola = Utente.EsponiAnagrafica()._matricola;

                if (master._dataInizio != "")
                    data_inizio = master._dataInizio + "0101";

                if (master._dataFine != "")
                    data_fine = master._dataFine + "0101";
                else
                    throw new Exception("Data fine non valorizzata");

                using (cv_ModelEntities db = new cv_ModelEntities())
                {
                    var tmp = db.TCVSpecializz.Where(x => x.Matricola.Equals(matricola, StringComparison.InvariantCultureIgnoreCase));

                    if (tmp.Count() == 0)
                    {
                        prog = 1;
                    }
                    else
                    {
                        var nro_prog = (db.TCVSpecializz.Where(x => x.Matricola.Equals(matricola, StringComparison.InvariantCultureIgnoreCase))).Max(x => x.Prog);
                        prog = Convert.ToInt32(nro_prog) + 1;
                    }

                    var toUpdate = db.TCVSpecializz.Where(x => x.Matricola.Equals(matricola, StringComparison.InvariantCultureIgnoreCase) && x.Prog == master._prog).FirstOrDefault();

                    if (toUpdate == null)
                        throw new Exception("Specializzazione non trovata");

                    toUpdate.DataInizio = data_inizio;
                    toUpdate.DataFine = data_fine;
                    toUpdate.Stato = "S";
                    toUpdate.TipoAgg = "A";
                    toUpdate.Matricola = matricola;
                    toUpdate.Durata = null;
                    toUpdate.TipoSpecial = master._codTitolo;
                    toUpdate.Titolo = master._titoloSpecializ;
                    toUpdate.Istituto = master._istituto;
                    toUpdate.Voto = master._voto;
                    toUpdate.Scala = master._scala;
                    toUpdate.Note = master._note;
                    toUpdate.Lode = (master._lode == 'S') ? "S" : "N";
                    toUpdate.LocalitaSpecial = master._localitaStudi;
                    toUpdate.CodNazione = master._codNazione;
                    toUpdate.IndirizzoSpecial = master._indirizzoStudi;
                    toUpdate.Riconoscimento = master._riconoscimento;
                    toUpdate.DataOraAgg = DateTime.Now;
                    //toUpdate.Prog = prog;
                    toUpdate.CodIstituto = master._codIstituto;

                    //db.SaveChanges();
                    if ( !DBHelper.Save( db , CommonManager.GetCurrentUserMatricola( ) ) )
                        throw new Exception( "Errore salvataggio dati db" );
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #endregion

        #region Controller AreeInteresse
        public ActionResult AreeInteresse()
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities();
            cvBox.areeInteresse = new List<cvModel.AreeInteresse>();

            //carico il modello CvModel.areeInteresse
            string descAreaOrg, descServizio, descTipoDispo, descAreaGeo;
            string matricola = Utente.EsponiAnagrafica()._matricola;
            List<TCVAreaIntAz> interesse = cvEnt.TCVAreaIntAz.Where(m => m.Matricola == matricola).ToList();

            foreach (var area in interesse)
            {
                cvModel.AreeInteresse frk = new cvModel.AreeInteresse( CommonManager.GetCurrentUserMatricola( ) );

                frk._areeIntDispo = area.AreeIntDispo;
                frk._codAreaGeo = area.CodAreaGeo;
                frk._codAreaOrg = area.CodAreaOrg;
                frk._codServizio = area.CodServizio;
                frk._codTipoDispo = area.CodTipoDispo;
                frk._dataOraAgg = area.DataOraAgg;
                frk._flagEsteroDispo = area.FlagEsteroDispo;
                frk._matricola = area.Matricola;
                frk._profIntDispo = area.ProfIntDispo;
                frk._prog = area.Prog;
                frk._stato = area.Stato;
                frk._tipoAgg = area.TipoAgg;

                var frk_geogio = cvEnt.DAreaGeoGio.Where(m => m.CodAreaGeoGio == area.CodAreaGeo).ToList();
                if (frk_geogio.Count > 0)
                {
                    descAreaGeo = frk_geogio.First().DesAreaGeoGio;
                }
                else
                {
                    descAreaGeo = null;
                }

                var frk_org = cvEnt.DAreaOrg.Where(m => m.CodAreaOrg == area.CodAreaOrg).ToList();
                if (frk_org.Count > 0)
                {
                    descAreaOrg = frk_org.First().DesAreaOrg;
                }
                else
                {
                    descAreaOrg = null;
                }

                var frk_tipodisto = cvEnt.DTipoDispo.Where(m => m.CodTipoDispo == area.CodTipoDispo).ToList();
                if (frk_tipodisto.Count > 0)
                {
                    descTipoDispo = frk_tipodisto.First().DescTipoDispo;
                }
                else
                {
                    descTipoDispo = null;
                }

                var frk_servizio = cvEnt.VDServizioCV.Where(m => m.Codice == area.CodServizio).ToList();
                if (frk_servizio.Count > 0)
                {
                    descServizio = frk_servizio.First().Descrizione;
                }
                else
                {
                    descServizio = null;
                }
                frk._descAreaGeo = descAreaGeo;
                frk._descAreaOrg = descAreaOrg;
                frk._descServizio = descServizio;
                frk._descTipoDispo = descTipoDispo;

                cvBox.areeInteresse.Add(frk);
            }

            ViewBag.idMenu = 22;
            return View("~/Views/CV_Online/AreeInteresse/BoxView.cshtml", cvBox);
        }

        [HttpPost]
        public ActionResult InsertAreeInteresse(cvModel.AreeInteresse areeInteresse, string[] _listaLocalita)
        {
            string result = "";

            if (ModelState.IsValid)
            {
                string matricola = Utente.EsponiAnagrafica()._matricola;
                int prog, c;

                cv_ModelEntities cvEnt = new cv_ModelEntities();
                TCVAreaIntAz interesse = new TCVAreaIntAz();
                TCVAreaIntAzEstero listaAreaGeo = new TCVAreaIntAzEstero();

                c = (_listaLocalita != null) ? _listaLocalita.Count() : 0;

                interesse.AreeIntDispo = areeInteresse._areeIntDispo;
                interesse.CodAreaGeo = areeInteresse._codAreaGeo;
                interesse.CodAreaOrg = areeInteresse._codAreaOrg;

                if (!String.IsNullOrEmpty(areeInteresse._codServizio))
                {
                    interesse.CodServizio = areeInteresse._codServizio.Trim();
                }

                interesse.CodTipoDispo = areeInteresse._codTipoDispo;
                interesse.DataOraAgg = DateTime.Now;
                interesse.FlagEsteroDispo = (areeInteresse._flagEsteroDispo == "S") ? "S" : "";
                interesse.Matricola = matricola;
                interesse.ProfIntDispo = areeInteresse._profIntDispo;
                interesse.Stato = "S";
                interesse.TipoAgg = "I";

                var tmp = cvEnt.TCVAreaIntAz.Where(x => x.Matricola == matricola);
                if (tmp.Count() == 0)
                {
                    prog = 1;
                }
                else
                {
                    var nro_prog = (cvEnt.TCVAreaIntAz.Where(x => x.Matricola == matricola)).Max(x => x.Prog);
                    prog = Convert.ToInt32(nro_prog) + 1;
                }
                interesse.Prog = prog;

                for (var i = 0; i < c; i++)
                {
                    listaAreaGeo.Codice = _listaLocalita[i];
                    listaAreaGeo.Matricola = matricola;
                    listaAreaGeo.Prog = prog;
                    try
                    {
                        cvEnt.TCVAreaIntAzEstero.Add(listaAreaGeo);
                        if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                            throw new Exception( "Errore salvataggio dati DB" );
                    }
                    catch (Exception ex)
                    {
                    }
                }

                try
                {
                    cvEnt.TCVAreaIntAz.Add(interesse);
                    if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                        throw new Exception( "Errore salvataggio dati DB" );
                    result = "ok";
                }
                catch (Exception ex)
                {
                    result = "error";
                }
            }
            else
            {
                result = "invalid";
            }

            return Content(result);
        }

        [HttpGet]
        public ActionResult DeleteAreeInteresse(string matricola, int prog)
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities();
            //FREAK - Cancellare dati dalla tabella TCVAreaIntAzEstero
            if (matricola == null || prog == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TCVAreaIntAz interesse = new TCVAreaIntAz();
            List<TCVAreaIntAzEstero> deleteAreaGeo = cvEnt.TCVAreaIntAzEstero.Where(x => x.Matricola == matricola && x.Prog == prog).ToList();
            interesse = cvEnt.TCVAreaIntAz.Find(matricola, prog);
            if (interesse == null)
            {
                return HttpNotFound();
            }
            try
            {
                cvEnt.TCVAreaIntAz.Remove(interesse);
                if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                    throw new Exception( "Errore salvataggio dati DB" );
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            //cancellare tutti i valori da TCVAreaintAzEster
            foreach (var elemDelete in deleteAreaGeo)
            {
                try
                {
                    cvEnt.TCVAreaIntAzEstero.Remove(elemDelete);
                    if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                        throw new Exception( "Errore salvataggio dati DB" );
                }
                catch (Exception ex)
                {

                }
            }

            ViewBag.idMenu = 22;
            return Content("Ok");
        }

        [HttpPost]
        public ActionResult Create_ViewAreeInteresse(cvModel.AreeInteresse interesse)
        {
            ModelState.Clear();
            string matricola = interesse._matricola;
            int prog = interesse._prog;

            if (matricola == null || prog == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            interesse.AreaInteresseItems = interesse.getAreeInteresse();
            return View("~/Views/CV_Online/AreeInteresse/ViewAreeInteresse.cshtml", interesse);
        }

        [HttpPost]
        public ActionResult ModificaAreeInteresse(cvModel.AreeInteresse interesse, string[] _listaLocalita)
        {
            string result = "";
            cv_ModelEntities cvEnt = new cv_ModelEntities();
            TCVAreaIntAz frk_interesse = new TCVAreaIntAz();
            TCVAreaIntAzEstero listaAreaGeo = new TCVAreaIntAzEstero();
            string matricola;
            int prog, c;

            c = (_listaLocalita != null) ? _listaLocalita.Count() : 0;

            matricola = interesse._matricola;
            prog = interesse._prog;

            List<TCVAreaIntAzEstero> deleteAreaGeo = cvEnt.TCVAreaIntAzEstero.Where(x => x.Matricola == matricola && x.Prog == prog).ToList();

            frk_interesse = cvEnt.TCVAreaIntAz.Find(matricola, prog);
            if (frk_interesse == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid)
            {
                frk_interesse.Matricola = matricola;
                frk_interesse.Prog = prog;
                frk_interesse.AreeIntDispo = interesse._areeIntDispo;
                frk_interesse.CodAreaGeo = interesse._codAreaGeo;
                frk_interesse.CodAreaOrg = interesse._codAreaOrg;
                frk_interesse.CodTipoDispo = interesse._codTipoDispo;
                frk_interesse.DataOraAgg = DateTime.Now;
                frk_interesse.FlagEsteroDispo = (interesse._flagEsteroDispo == "S") ? "S" : "";
                frk_interesse.ProfIntDispo = interesse._profIntDispo;
                frk_interesse.Stato = "S";
                frk_interesse.TipoAgg = "A";

                //cancellare tutti i valori da TCVAreaintAzEster
                foreach (var elemDelete in deleteAreaGeo)
                {
                    try
                    {
                        cvEnt.TCVAreaIntAzEstero.Remove(elemDelete);
                        if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                            throw new Exception( "Errore salvataggio dati DB" );
                    }
                    catch (Exception ex)
                    {
                    }
                }

                for (var i = 0; i < c; i++)
                {
                    listaAreaGeo.Codice = _listaLocalita[i];
                    listaAreaGeo.Matricola = matricola;
                    listaAreaGeo.Prog = prog;
                    try
                    {
                        cvEnt.TCVAreaIntAzEstero.Add(listaAreaGeo);
                        if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                            throw new Exception( "Errore salvataggio dati DB" );
                    }
                    catch (Exception ex)
                    {
                    }
                }

                try
                {
                    if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                        throw new Exception( "Errore salvataggio dati DB" );
                    result = "ok";
                }
                catch (Exception ex)
                {
                    result = "error";
                }
            }
            else
            {
                result = "invalid";
            }
            return Content(result);
        }
        #endregion

        #region Controller CompetenzeDigitali

        public ActionResult CompetenzeDigitali()
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities();
            cvBox.competenzeDigitali = new List<cvModel.CompetenzeDigitali>();
            List<string> codici = new List<string> { "010", "020", "030", "040", "050" };

            //carico il modello CvModel.competenzeDigitali
            string matricola = Utente.EsponiAnagrafica()._matricola;

            //Se inseriti di default, non si ha una reale percezione di chi abbia compilato i record
            if (cvEnt.TCVCompDigit.Where(m => m.Matricola == matricola).ToList().Count == 0)
            {
                //inserisco i primi record di default
                foreach (var elem in codici)
                {
                    cvModel.CompetenzeDigitali frk_comepetenze = new cvModel.CompetenzeDigitali();
                    string descCompDigit, descCompDigitLiv, descCompDigitLivLunga;

                    frk_comepetenze._codCompDigit = elem;
                    frk_comepetenze._matricola = matricola;
                    frk_comepetenze._stato = "S";
                    frk_comepetenze._tipoAgg = "I";
                    frk_comepetenze._dataOraAgg = DateTime.Now;
                    descCompDigitLiv = "Nessun dato inserito";
                    descCompDigitLivLunga = "Nessun dato inserito";
                    descCompDigit = (from compDigit in cvEnt.DCompDigit
                                     where compDigit.CodCompDigit == elem
                                     select compDigit.DescCompDigit).First().ToString();
                    frk_comepetenze._descCompDigit = descCompDigit;
                    frk_comepetenze._descCompDigitLiv = descCompDigitLiv;
                    frk_comepetenze._descCompDigitLivLunga = descCompDigitLivLunga;

                    //---------------------------------------
                    cvBox.competenzeDigitali.Add(frk_comepetenze);
                }
            }
            else
            {
                List<TCVCompDigit> competenze = cvEnt.TCVCompDigit.Where(m => m.Matricola == matricola).OrderByDescending(a => a.CodCompDigitLiv).ToList();

                foreach (var elem in competenze)
                {
                    cvModel.CompetenzeDigitali frk_comepetenze = new cvModel.CompetenzeDigitali();
                    string descCompDigit, descCompDigitLiv, descCompDigitLivLunga;
                    string codCompDigit, codCompDigitLiv;

                    frk_comepetenze._codCompDigit = elem.CodCompDigit;
                    frk_comepetenze._matricola = matricola;
                    frk_comepetenze._stato = elem.Stato;
                    frk_comepetenze._tipoAgg = elem.TipoAgg;
                    frk_comepetenze._dataOraAgg = elem.DataOraAgg;
                    frk_comepetenze._codCompDigitLiv = elem.CodCompDigitLiv;
                    if (String.IsNullOrEmpty(elem.CodCompDigitLiv.Trim()))
                    {
                        descCompDigitLiv = "Nessun dato inserito";
                        descCompDigitLivLunga = "Nessun dato inserito";
                    }
                    else
                    {
                        descCompDigitLiv = (from compDigitLiv in cvEnt.DCompDigitLiv
                                            where (compDigitLiv.CodCompDigit == elem.CodCompDigit && compDigitLiv.CodCompDigitLiv == elem.CodCompDigitLiv)
                                            select compDigitLiv.DescCompDigitLiv).First().ToString();
                        descCompDigitLivLunga = (from compDigitLivLunga in cvEnt.DCompDigitLiv
                                                 where (compDigitLivLunga.CodCompDigit == elem.CodCompDigit && compDigitLivLunga.CodCompDigitLiv == elem.CodCompDigitLiv)
                                                 select compDigitLivLunga.DescCompDigitLivLunga).First().ToString();
                    }
                    descCompDigit = (from compDigit in cvEnt.DCompDigit
                                     where compDigit.CodCompDigit == elem.CodCompDigit
                                     select compDigit.DescCompDigit).First().ToString();
                    frk_comepetenze._descCompDigit = descCompDigit;
                    frk_comepetenze._descCompDigitLiv = descCompDigitLiv;
                    frk_comepetenze._descCompDigitLivLunga = descCompDigitLivLunga;

                    //---------------------------------------
                    cvBox.competenzeDigitali.Add(frk_comepetenze);
                }
            }

            ViewBag.idMenu = 39;
            return View("~/Views/CV_Online/CompetenzeDigitali/BoxView.cshtml", cvBox);
        }

        [HttpPost]
        public ActionResult InsertCompetenzeDigitali(string[] compDigitLiv)
        {
            List<string> codici = new List<string> { "010", "020", "030", "040", "050" };
            string result;
            string matricola;
            cv_ModelEntities cvEnt = new cv_ModelEntities();

            matricola = Utente.EsponiAnagrafica()._matricola;

            if (ModelState.IsValid)
            {
                for (var i = 0; i < 5; i++)
                {
                    string codCompDigit, codCompDigitLiv;

                    codCompDigit = codici[i];
                    codCompDigitLiv = String.IsNullOrWhiteSpace(compDigitLiv[i]) ? String.Empty : compDigitLiv[i];

                    TCVCompDigit frk_competenze = new TCVCompDigit();
                    frk_competenze = cvEnt.TCVCompDigit.Find(matricola, codCompDigit);
                    bool newComp = false;
                    if (frk_competenze == null)
                    {
                        newComp = true;
                        frk_competenze = new TCVCompDigit();
                        frk_competenze.Matricola = matricola;
                        frk_competenze.CodCompDigit = codCompDigit;
                    }
                    frk_competenze.CodCompDigitLiv = codCompDigitLiv;
                    frk_competenze.DataOraAgg = DateTime.Now;
                    frk_competenze.Stato = "S";
                    frk_competenze.TipoAgg = "A";

                    try
                    {
                        if (newComp) cvEnt.TCVCompDigit.Add(frk_competenze);
                        if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                            throw new Exception( "Errore salvataggio dati DB" );
                    }
                    catch (Exception exc)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                    }
                }
                result = "ok";
            }
            else
            {
                result = "invalid";
            }
            return Content(result);
        }

        [HttpPost]
        public ActionResult Create_ViewCompetenzeDigitali(cvModel.CompetenzeDigitali compDigit)
        {
            string matricola = compDigit._matricola;

            if (matricola == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View("~/Views/CV_Online/CompetenzeDigitali/ViewCompetenzeDigitali.cshtml", compDigit);
        }

        #endregion

        #region Controller Formazione

        public ActionResult Formazione()
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities();
            cvBox.formazione = new List<cvModel.Formazione>();

            string matricola = Utente.EsponiAnagrafica()._matricola;
            List<TCVFormExRai> formaz = new List<TCVFormExRai>();

            formaz = cvEnt.TCVFormExRai.Where(x => x.Matricola == matricola).ToList();
            foreach (var elem in formaz)
            {
                cvModel.Formazione frk_formazione = new cvModel.Formazione( CommonManager.GetCurrentUserMatricola( ) );

                frk_formazione._anno = elem.Anno;
                frk_formazione._codNazione = elem.CodNazione;
                frk_formazione._corso = elem.Corso;
                frk_formazione._dataOraAgg = elem.DataOraAgg;
                frk_formazione._descNazione = (from naz in cvEnt.DNazione
                                               where naz.COD_SIGLANAZIONE == elem.CodNazione
                                               select naz.DES_NAZIONE).ToString();
                frk_formazione._durata = elem.Durata;
                frk_formazione._localitaCorso = elem.LocalitaCorso;
                frk_formazione._matricola = elem.Matricola;
                frk_formazione._note = elem.Note;
                frk_formazione._presso = elem.Presso;
                frk_formazione._prog = elem.Prog;
                frk_formazione._stato = elem.Stato;
                frk_formazione._tipoAgg = elem.TipoAgg;

                cvBox.formazione.Add(frk_formazione);
            }

            ViewBag.idMenu = 19;
            return View("~/Views/CV_Online/Formazione/BoxView.cshtml", cvBox);
        }

        [HttpPost]
        public ActionResult InsertFormazione(cvModel.Formazione frmz)
        {
            string result = "";
            int prog;

            if (ModelState.IsValid)
            {
                string matricola = Utente.EsponiAnagrafica()._matricola;
                cv_ModelEntities cvEnt = new cv_ModelEntities();

                TCVFormExRai formazione = new TCVFormExRai();

                formazione.Anno = frmz._anno;
                formazione.CodNazione = frmz._codNazione;
                formazione.Corso = frmz._corso;
                formazione.DataOraAgg = DateTime.Now;
                formazione.Durata = frmz._durata;
                formazione.LocalitaCorso = frmz._localitaCorso;
                formazione.Matricola = matricola;
                formazione.Note = frmz._note;
                formazione.Presso = frmz._presso;
                formazione.Stato = "S";
                formazione.TipoAgg = "I";

                //freak - Gestione de campo Prog
                var tmp = cvEnt.TCVFormExRai.Where(x => x.Matricola == matricola);
                if (tmp.Count() == 0)
                {
                    prog = 1;
                }
                else
                {
                    var nro_prog = (cvEnt.TCVFormExRai.Where(x => x.Matricola == matricola)).Max(x => x.Prog);
                    prog = Convert.ToInt32(nro_prog) + 1;
                }
                formazione.Prog = prog;

                try
                {
                    cvEnt.TCVFormExRai.Add(formazione);
                    if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                        throw new Exception( "Errore salvataggio dati DB" );
                    result = "ok";
                }
                catch (Exception ex)
                {
                    result = ex.Message;
                }
            }
            else
            {
                result = "invalid";
            }

            return Content(result);
        }

        [HttpGet]
        public ActionResult DeleteFormazione(string matricola, int prog)
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities();

            if ((matricola == null) && (prog < 0))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TCVFormExRai master = cvEnt.TCVFormExRai.Find(matricola, prog);
            if (master == null)
            {
                return HttpNotFound();
            }
            try
            {
                cvEnt.TCVFormExRai.Remove(master);
                if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                    throw new Exception( "Errore salvataggio dati DB" );
            }
            catch (Exception exc)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //FREAK - RIEMPIMENTO DEL MODELLO CVBOX E DELLE LISTE SPECIALIZZAZIONI E ISTRUZIONE
            ViewBag.idMenu = 19;
            return Content("Ok");
        }

        [HttpPost]
        public ActionResult Create_ViewFormazione(cvModel.Formazione formazione)
        {
            ModelState.Clear();

            string matricola = formazione._matricola;
            int prog = formazione._prog;

            if (matricola == null || prog == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            return View("~/Views/CV_Online/Formazione/ViewFormazione.cshtml", formazione);
        }

        [HttpPost]
        public ActionResult ModificaFormazione(cvModel.Formazione frmz)
        {
            string result = "";

            if (ModelState.IsValid)
            {
                string matricola = frmz._matricola;
                int prog = frmz._prog;

                cv_ModelEntities cvEnt = new cv_ModelEntities();
                TCVFormExRai formazione = new TCVFormExRai();

                formazione = cvEnt.TCVFormExRai.Find(matricola, prog);

                if (formazione == null)
                {
                    return HttpNotFound();
                }

                formazione.Anno = frmz._anno;
                formazione.CodNazione = frmz._codNazione;
                formazione.Corso = frmz._corso;
                formazione.DataOraAgg = DateTime.Now;
                formazione.Durata = frmz._durata;
                formazione.LocalitaCorso = frmz._localitaCorso;
                formazione.Note = frmz._note;
                formazione.Presso = frmz._presso;
                formazione.Stato = "S";
                formazione.TipoAgg = "A";

                try
                {
                    if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                        throw new Exception( "Errore salvataggio dati DB" );
                    result = "ok";
                }
                catch (Exception ex)
                {
                    result = ex.Message;
                }
            }
            else
            {
                result = "invalid";
            }

            return Content(result);
        }

        #endregion

        #region Controller ConoscenzeInformatiche

        public ActionResult knowledges()
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities();
            cvBox.conoscenzeInformatiche = new List<cvModel.ConoscenzeInformatiche>();

            //carico il modello CvModel.conoscenzeInformatiche
            string matricola = Utente.EsponiAnagrafica()._matricola;
            List<TCVConInfo> TCVConInfo = new List<TCVConInfo>();

            TCVConInfo = cvEnt.TCVConInfo.Where(x => x.Matricola == matricola).ToList();
            var allList = from conInfo in cvEnt.DConInfo
                          join gruppoConInfo in cvEnt.DGruppoConInfo on conInfo.CodGruppoConInfo equals gruppoConInfo.CodGruppoConInfo
                          select new
                          {
                              CodConInfo = conInfo.CodConInfo,
                              DescInfo = conInfo.DescConInfo,
                              CodGruppoConInfo = conInfo.CodGruppoConInfo,
                              CodPosizione = conInfo.CodPosizione,
                              DescGruppoConInfo = gruppoConInfo.DescGruppoConInfo
                          };

            foreach (var elem in allList)
            {
                cvModel.ConoscenzeInformatiche know = new cvModel.ConoscenzeInformatiche();
                know._codConInfo = elem.CodConInfo;
                know._codGruppoConInfo = elem.CodGruppoConInfo;
                know._codPosizione = elem.CodPosizione;
                know._descConInfo = elem.DescInfo;
                know._descGruppoConInfo = elem.DescGruppoConInfo;

                var tmp = TCVConInfo.Where(x => x.CodConInfo == elem.CodConInfo);
                if (elem.CodGruppoConInfo != "99")
                {
                    if (tmp.Count() > 0)
                    {
                        know._selectedConInfo = true;
                        know._altraConInfo = tmp.First().AltraConInfo;
                        know._codConInfoLiv = tmp.First().CodConInfoLiv;
                        know._note = tmp.First().Note;
                        know._matricola = tmp.First().Matricola;
                        know._prog = tmp.First().Prog;
                    }
                    else
                    {
                        know._selectedConInfo = false;
                        know._altraConInfo = "";
                        know._codConInfoLiv = "";
                        know._note = "";
                        know._matricola = "";
                        know._prog = 0;
                    }
                }
                cvBox.conoscenzeInformatiche.Add(know);
            }

            var tmp_lista9999 = TCVConInfo.Where(x => x.CodConInfo == "9999").ToList();
            foreach (var elem_2 in tmp_lista9999)
            {
                cvModel.ConoscenzeInformatiche know = new cvModel.ConoscenzeInformatiche();

                know._altraConInfo = elem_2.AltraConInfo;
                know._codConInfo = elem_2.CodConInfo;
                know._codConInfoLiv = elem_2.CodConInfoLiv;
                know._codGruppoConInfo = "99";
                know._dataOraAgg = elem_2.DataOraAgg;
                know._matricola = elem_2.Matricola;
                know._note = elem_2.Note;
                know._prog = elem_2.Prog;
                know._selectedConInfo = true;
                know._stato = elem_2.Stato;
                know._tipoAgg = elem_2.TipoAgg;

                cvBox.conoscenzeInformatiche.Add(know);
            }

            ViewBag.idMenu = 21;
            return View("~/Views/CV_Online/knowledges/BoxView.cshtml", cvBox);
        }

        [HttpPost]
        public ActionResult EditConoscenzeInformatiche(cvModel.ConoscenzeInformatiche[] know)
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities();
            string matricola = Utente.EsponiAnagrafica()._matricola;
            //freak - per inserire prima cancello tutti i dati nella tabella relativa alla matricola, poi filtro dal modello 
            // - solo i valori che sono stati checkati tramite la proprietà _selectedConInfo.
            // - In fase di Aggiunta devo fare distinzione dei codConInfo, causa della proprietà Prog
            //elimino tutte le righe nella tabella

            try
            {
                //cvEnt.TCVConInfo.Where(x => x.Matricola == matricola).ForEach(y => cvEnt.TCVConInfo.Remove(y));
                foreach (var item in cvEnt.TCVConInfo.Where(x => x.Matricola == matricola))
                {
                    cvEnt.TCVConInfo.Remove(item);
                }

                foreach (var item_coninfo in know.Where(x => x._selectedConInfo && x._codConInfo != "9999"))
                {
                    TCVConInfo ConInfo = new TCVConInfo();

                    ConInfo.AltraConInfo = item_coninfo._altraConInfo;
                    ConInfo.CodConInfo = item_coninfo._codConInfo;
                    ConInfo.CodConInfoLiv = item_coninfo._codConInfoLiv;
                    ConInfo.DataOraAgg = DateTime.Now;
                    ConInfo.Matricola = matricola;
                    ConInfo.Note = item_coninfo._note;
                    ConInfo.Prog = 1;
                    ConInfo.Stato = "S";
                    ConInfo.TipoAgg = "I";
                    cvEnt.TCVConInfo.Add(ConInfo);
                }

                int prog = 0;
                foreach (var item_infoextra in know.Where(x => x._selectedConInfo && x._codConInfo == "9999"))
                {
                    TCVConInfo ConInfo = new TCVConInfo();
                    prog++;
                    ConInfo.AltraConInfo = item_infoextra._altraConInfo;
                    ConInfo.CodConInfo = item_infoextra._codConInfo;
                    ConInfo.CodConInfoLiv = item_infoextra._codConInfoLiv;
                    ConInfo.DataOraAgg = DateTime.Now;
                    ConInfo.Matricola = matricola;
                    ConInfo.Note = item_infoextra._note;
                    ConInfo.Prog = prog;
                    ConInfo.Stato = "S";
                    ConInfo.TipoAgg = "I";
                    cvEnt.TCVConInfo.Add(ConInfo);
                }

                if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                    throw new Exception( "Errore salvataggio dati DB" );

            }
            catch (Exception)
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
            }

            return Content("Ok");
        }

        #endregion

        #region AltreInfo

        public ActionResult CaricaAltreInfoModal()
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities();
            cvBox.altreInformazioni = new cvModel.AltreInfo( CommonManager.GetCurrentUserMatricola( ) );

            string matricola = Utente.EsponiAnagrafica()._matricola;
            TCVAltreInf altreInformazioni;
            List<TCVAltreInfPat> listaPatenti = cvEnt.TCVAltreInfPat.Where(x => x.Matricola == matricola).ToList();
            try
            {
                altreInformazioni = cvEnt.TCVAltreInf.Where(x => x.Matricola == matricola).FirstOrDefault();
            }
            catch (Exception ex)
            {
                altreInformazioni = null;
            }

            if (altreInformazioni != null)
            {
                cvModel.AltreInfo frk_altreInfo = new cvModel.AltreInfo( CommonManager.GetCurrentUserMatricola( ) );

                frk_altreInfo._dataOraAgg = altreInformazioni.DataOraAgg;
                frk_altreInfo._email = altreInformazioni.EMail;
                frk_altreInfo._matricola = matricola;
                frk_altreInfo._note = altreInformazioni.Note;
                frk_altreInfo._numTel1 = altreInformazioni.NumTel1;
                frk_altreInfo._numTel2 = altreInformazioni.NumTel2;
                frk_altreInfo._sitoWeb = altreInformazioni.Sitoweb;
                frk_altreInfo._stato = altreInformazioni.Stato;
                frk_altreInfo._tipoAgg = altreInformazioni.TipoAgg;
                frk_altreInfo._tipoTel1 = altreInformazioni.TipoTel1;
                frk_altreInfo._tipoTel2 = altreInformazioni.TipoTel2;

                frk_altreInfo._tipoPatente = new List<DTipoPatente>();

                if (listaPatenti.Count > 0)
                {
                    foreach (var elem in listaPatenti)
                    {
                        DTipoPatente item = new DTipoPatente();
                        item = cvEnt.DTipoPatente.Where(x => x.CodTipoPatente == elem.CodTipoPatente).First();
                        frk_altreInfo._tipoPatente.Add(item);
                    }
                }
                else
                {
                    frk_altreInfo._tipoPatente = null;
                }
                cvBox.altreInformazioni = frk_altreInfo;
            }
            ViewBag.idMenu = 18;
            return View("~/Views/CV_Online/AltreInfo/partials/_modalInserimento.cshtml", cvBox.altreInformazioni);
        }

        public ActionResult AltreInfo()
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities();
            cvBox.altreInformazioni = new cvModel.AltreInfo( CommonManager.GetCurrentUserMatricola( ) );

            //carico il modello CvModel.studies
            string matricola = Utente.EsponiAnagrafica()._matricola;
            TCVAltreInf altreInformazioni;
            List<TCVAltreInfPat> listaPatenti = cvEnt.TCVAltreInfPat.Where(x => x.Matricola == matricola).ToList();
            try
            {
                altreInformazioni = cvEnt.TCVAltreInf.Where(x => x.Matricola == matricola).FirstOrDefault();
            }
            catch (Exception ex)
            {
                altreInformazioni = null;
            }

            if (altreInformazioni != null)
            {
                cvModel.AltreInfo frk_altreInfo = new cvModel.AltreInfo( CommonManager.GetCurrentUserMatricola( ) );
                /*
                 * 
                 * FREAK - INSERIRE LA PARTE RELATIVA AI DATI DI DOMICILIO E RESIDENZA, PRESI DA O UN SERVIZIO 
                 * O DA UNA STORED PROCEDURE
                 * 
                 * */
                frk_altreInfo._dataOraAgg = altreInformazioni.DataOraAgg;
                frk_altreInfo._email = altreInformazioni.EMail;
                frk_altreInfo._matricola = matricola;
                frk_altreInfo._note = altreInformazioni.Note;
                frk_altreInfo._numTel1 = altreInformazioni.NumTel1;
                frk_altreInfo._numTel2 = altreInformazioni.NumTel2;
                frk_altreInfo._sitoWeb = altreInformazioni.Sitoweb;
                frk_altreInfo._stato = altreInformazioni.Stato;
                frk_altreInfo._tipoAgg = altreInformazioni.TipoAgg;
                frk_altreInfo._tipoTel1 = altreInformazioni.TipoTel1;
                frk_altreInfo._tipoTel2 = altreInformazioni.TipoTel2;

                frk_altreInfo._tipoPatente = new List<DTipoPatente>();

                if (listaPatenti.Count > 0)
                {
                    foreach (var elem in listaPatenti)
                    {
                        DTipoPatente item = new DTipoPatente();
                        item = cvEnt.DTipoPatente.Where(x => x.CodTipoPatente == elem.CodTipoPatente).First();
                        frk_altreInfo._tipoPatente.Add(item);
                    }
                }
                else
                {
                    frk_altreInfo._tipoPatente = null;
                }
                cvBox.altreInformazioni = frk_altreInfo;
            }
            ViewBag.idMenu = 18;
            return View("~/Views/CV_Online/AltreInfo/BoxView.cshtml", cvBox);
        }

        public ActionResult InsertAltreInfo(cvModel.AltreInfo altreInfo, string[] tipoPatente)
        {
            if (!ModelState.IsValid)
                return Content("Errore nell'inserimento dei dati");

            string matricola = Utente.EsponiAnagrafica()._matricola;

            cv_ModelEntities cvEnt = new cv_ModelEntities();

            //FREAK - INSERIMENTO O MODIFICA DEL RECORD NELLA TABELLA TCVALTREINF E 

            TCVAltreInf elemAltreInfo = cvEnt.TCVAltreInf.Find(matricola);

            if (elemAltreInfo != null)
            {
                //Devo fare un Update
                elemAltreInfo.DataOraAgg = DateTime.Now;
                elemAltreInfo.EMail = altreInfo._email;
                elemAltreInfo.Note = altreInfo._note;
                elemAltreInfo.NumTel1 = altreInfo._numTel1;
                elemAltreInfo.NumTel2 = altreInfo._numTel2;
                elemAltreInfo.Sitoweb = altreInfo._sitoWeb;
                elemAltreInfo.Stato = "S";
                elemAltreInfo.TipoAgg = "I";
                elemAltreInfo.TipoTel1 = altreInfo._tipoTel1;
                elemAltreInfo.TipoTel2 = altreInfo._tipoTel2;

                try
                {
                    //cvEnt.SaveChanges();
                    if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                        throw new Exception( "Errore salvataggio dati DB" );
                }
                catch (Exception ex)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                }
                //inserisco i dati nella tabella TCVAltreInfPat
                //cancello prima tutti gli elementi
                List<TCVAltreInfPat> listaPatenti = cvEnt.TCVAltreInfPat.Where(x => x.Matricola == matricola).ToList();
                if (listaPatenti.Count() > 0)
                {
                    try
                    {
                        foreach (var item in listaPatenti)
                        {

                            cvEnt.TCVAltreInfPat.Remove(item);
                        }
                        if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                            throw new Exception( "Errore salvataggio dati DB" );
                    }
                    catch (Exception ex)
                    {
                        return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                    }
                }
                //inserisco i nuovi dati
                if (tipoPatente != null)
                {
                    foreach (var codPatenti in tipoPatente)
                    {
                        TCVAltreInfPat elem_insert = new TCVAltreInfPat();
                        elem_insert.CodTipoPatente = codPatenti;
                        elem_insert.Matricola = matricola;

                        try
                        {
                            cvEnt.TCVAltreInfPat.Add(elem_insert);
                            //cvEnt.SaveChanges();
                            if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                                throw new Exception( "Errore salvataggio dati DB" );
                        }
                        catch (Exception ex)
                        {
                            return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                        }
                    }
                }
            }
            else
            {
                //devo fare un Insert
                TCVAltreInf insAltreInfo = new TCVAltreInf();
                insAltreInfo.DataOraAgg = DateTime.Now;
                insAltreInfo.Matricola = matricola;
                insAltreInfo.EMail = altreInfo._email;
                insAltreInfo.Note = altreInfo._note;
                insAltreInfo.NumTel1 = altreInfo._numTel1;
                insAltreInfo.NumTel2 = altreInfo._numTel2;
                insAltreInfo.Sitoweb = altreInfo._sitoWeb;
                insAltreInfo.Stato = "S";
                insAltreInfo.TipoAgg = "A";
                insAltreInfo.TipoTel1 = altreInfo._tipoTel1;
                insAltreInfo.TipoTel2 = altreInfo._tipoTel2;

                try
                {
                    cvEnt.TCVAltreInf.Add(insAltreInfo);
                    if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                        throw new Exception( "Errore salvataggio dati DB" );
                }
                catch (Exception ex)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                }
                //inserisco i nuovi dati
                try
                {
                    foreach (var codPatenti in tipoPatente)
                    {
                        TCVAltreInfPat elem_insert = new TCVAltreInfPat();
                        elem_insert.CodTipoPatente = codPatenti;
                        elem_insert.Matricola = matricola;

                        cvEnt.TCVAltreInfPat.Add(elem_insert);
                        if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                            throw new Exception( "Errore salvataggio dati DB" );
                    }
                }
                catch (Exception ex)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                }
            }
            return Content("Ok");
        }

        #endregion

        #region ImpegniEditoriali RAI

        public ActionResult ImpegniRAI()
        {
            string matricola = Utente.EsponiAnagrafica()._matricola;

            cv_ModelEntities cvEnt = new cv_ModelEntities();
            cvBox.impegniEditoriali = new List<cvModel.ImpegniRAI>();

            //carico la lista impegniEditoriali 
            int prog = 0;
            List<Data.CurriculumVitae.TSVEsperProd> esperProd = cvEnt.TSVEsperProd.Where(f => f.STATO == "C" && f.COD_ANAGRAFIA != "IR" && f.MATRICOLA == matricola).OrderByDescending(p => p.FINE_PERIODO_ESP).ToList();
            foreach (var elem in esperProd)
            {
                string tmp_ruolo;
                cvModel.ImpegniRAI impegniRai = new cvModel.ImpegniRAI();
                prog++;
                impegniRai._desTitoloDefinit = elem.DES_TITOLO_DEFINIT;
                impegniRai._idEsperienze = Convert.ToInt32(elem.ID_ESPERIENZE);
                impegniRai._matricola = matricola;
                impegniRai._matricolaSpett = elem.COD_MATRICOLA;
                impegniRai._progDaStampare = prog;
                tmp_ruolo = cvEnt.DConProf.Where(x => x.CodConProf == elem.COD_RUOLO).Select(x => x.DescConProf).FirstOrDefault();
                //gestione date
                impegniRai._ruolo = tmp_ruolo;
                impegniRai._dtDataInizio = elem.INIZIO_PERIODO_ESP.Substring(6, 2) + "/" + elem.INIZIO_PERIODO_ESP.Substring(4, 2) + "/" + elem.INIZIO_PERIODO_ESP.Substring(0, 4);
                impegniRai._dtDataFine = elem.FINE_PERIODO_ESP.Substring(6, 2) + "/" + elem.FINE_PERIODO_ESP.Substring(4, 2) + "/" + elem.FINE_PERIODO_ESP.Substring(0, 4);

                cvBox.impegniEditoriali.Add(impegniRai);
            }

            ViewBag.idMenu = 41;
            return View("~/Views/CV_Online/ImpegniRAI/BoxView.cshtml", cvBox);
        }

        #endregion

        #region Controller CompetenzeRAI

        public ActionResult CompetenzeRAI()
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities();
            cvBox.competenzeRai = new List<cvModel.CompetenzeRAI>();

            //carico il modello CvModel.competenzeRai
            string matricola = Utente.EsponiAnagrafica()._matricola;
            //prelevo la figura professionale
            string figura_profesisonale = Utente.EsponiAnagrafica()._codiceFigProf;

            List<TCVConProf> tcvConProf = cvEnt.TCVConProf.Where(x => x.Matricola == matricola).ToList();
            List<Data.CurriculumVitae.DConProf> dConProf = cvEnt.DConProf.Where(x =>
                x.FiguraProfessionale == figura_profesisonale && x.Stato == "0").OrderBy(x => x.DescConProf).ToList();

            foreach (var elem in dConProf)
            {
                cvModel.CompetenzeRAI frk_compRai = new cvModel.CompetenzeRAI();

                frk_compRai._codConProf = elem.CodConProf;
                frk_compRai._descConProf = elem.DescConProf;
                frk_compRai._figuraProfessionale = elem.FiguraProfessionale;
                frk_compRai._matricola = matricola;
                frk_compRai.descrittiva_lunga = elem.DescConProfLunga;

                var tmp = tcvConProf.Where(x => x.CodConProf == elem.CodConProf).ToList();
                if (tmp.Count() > 0)
                {
                    frk_compRai._dataOraAgg = tmp.First().DataOraAgg;
                    frk_compRai._flagExtraRai = tmp.First().FlagExtraRai;
                    frk_compRai._flagPrincipale = tmp.First().FlagPrincipale;
                    frk_compRai._flagSecondario = tmp.First().FlagSecondario;
                    frk_compRai._stato = tmp.First().Stato;
                    frk_compRai._tipoAgg = tmp.First().TipoAgg;

                    if (frk_compRai._flagPrincipale == "1")
                    {
                        frk_compRai._flagChoice = "P";
                    }
                    else if (frk_compRai._flagSecondario == "1")
                    {
                        frk_compRai._flagChoice = "S";
                    }
                    else
                    {
                        frk_compRai._flagChoice = " ";
                    }
                }
                else
                {
                    frk_compRai._dataOraAgg = null;
                    frk_compRai._flagChoice = "";
                    frk_compRai._flagExtraRai = "";
                    frk_compRai._flagPrincipale = "";
                    frk_compRai._flagSecondario = "";
                    frk_compRai._stato = "";
                    frk_compRai._tipoAgg = "";
                }

                cvBox.competenzeRai.Add(frk_compRai);
            }
            ViewBag.idMenu = 42;
            return View("~/Views/CV_Online/CompetenzeRAI/BoxView.cshtml", cvBox);
        }

        [HttpPost]
        public ActionResult EditCompetenzeRai(cvModel.CompetenzeRAI[] compRai)
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities();
            string matricola = Utente.EsponiAnagrafica()._matricola;

            //cancello tutti gli elementi in TCVConProf
            List<TCVConProf> delete_list = cvEnt.TCVConProf.Where(x => x.Matricola == matricola
                && cvEnt.DConProf.Any(z => z.TipoConProf == "A" && z.CodConProf == x.CodConProf)
                ).ToList();
            foreach (TCVConProf delete in delete_list)
            {
                try
                {
                    //cvEnt.TCVConInfo.Remove(delete);
                    cvEnt.TCVConProf.Remove(delete);
                    //cvEnt.SaveChanges();
                    if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                        throw new Exception( "Errore salvataggio dati DB" );
                }
                catch (Exception ex)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                }
            }
            //filtro la lista solo con gli elementi selezionati
            var lista_check = compRai.Where(x => x._codConProf != null).ToList();
            foreach (var elem in lista_check)
            {
                string extraRai, principale, secondario;
                TCVConProf conProf = new TCVConProf();

                conProf.AltraConProf = "R";
                conProf.CodConProf = elem._codConProf;
                conProf.CodConProfLiv = "";
                conProf.DataOraAgg = DateTime.Now;
                if (elem._flagExtraRai == "1")
                {
                    extraRai = "1";
                }
                else
                {
                    extraRai = "0";
                }

                switch (elem._flagChoice)
                {
                    case "P":
                        principale = "1";
                        secondario = "0";
                        break;
                    case "S":
                        principale = "0";
                        secondario = "1";
                        break;
                    default:
                        principale = "0";
                        secondario = "0";
                        break;
                }
                conProf.FlagExtraRai = extraRai;
                conProf.FlagPrincipale = principale;
                conProf.FlagSecondario = secondario;
                conProf.Matricola = matricola;
                conProf.Prog = 1;
                conProf.Stato = "S";
                conProf.TipoAgg = "I";

                try
                {
                    cvEnt.TCVConProf.Add(conProf);
                    //cvEnt.SaveChanges();
                    if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                        throw new Exception( "Errore salvataggio dati DB" );
                }
                catch (Exception ex)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                }
            }
            return Content("Ok");
        }

        #endregion

        #region Controller CompetenzeSpecialistiche
        public ActionResult CompetenzeSpecialistiche()
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities();
            cvBox.competenzeSpecialistiche = new List<cvModel.CompetenzeSpecialistiche>();

            //carico il modello CvModel.studies
            string matricola = Utente.EsponiAnagrafica()._matricola;
            string figura_professionale = Utente.EsponiAnagrafica()._codiceFigProf;
            string fpForzata = GetFiguraProfessionaleForzata(matricola);
            if (fpForzata != null) figura_professionale = fpForzata;

            List<Data.CurriculumVitae.DConProf> DConProf = cvEnt.DConProf.Where(x => x.FiguraProfessionale == figura_professionale && x.Stato == "0").ToList();
            List<TCVConProf> TCVConProf = cvEnt.TCVConProf.Where(x => x.Matricola == matricola).ToList();

            foreach (var elem in DConProf)
            {
                string tmp_descConProf;
                cvModel.CompetenzeSpecialistiche frk_compSpec = new cvModel.CompetenzeSpecialistiche();

                var tmp_tcvConProf = TCVConProf.Where(x => x.CodConProf == elem.CodConProf);

                frk_compSpec._codConProf = elem.CodConProf;
                frk_compSpec._codConProfAggr = elem.CodConProfAggr;
                frk_compSpec._dataOraAgg = null;
                frk_compSpec._descConProf = elem.DescConProf;
                frk_compSpec._descConProfLunga = elem.DescConProfLunga;
                frk_compSpec._figuraProfessionale = figura_professionale;
                //setto il flag _isSelected
                if (tmp_tcvConProf.Count() > 0)
                {
                    frk_compSpec._isSelected = true;
                    frk_compSpec._codConProfLiv = tmp_tcvConProf.First().CodConProfLiv;
                    frk_compSpec._flagPrincipale = tmp_tcvConProf.First().FlagPrincipale;
                }
                else
                {
                    frk_compSpec._isSelected = false;
                    frk_compSpec._codConProfLiv = null;
                }
                //setto il flag _isTitle
                if ((elem.DescConProf.Contains("skill")))
                {
                    frk_compSpec._isTitle = true;
                }
                else
                {
                    frk_compSpec._isTitle = false;
                }
                frk_compSpec._matricola = matricola;
                frk_compSpec._posizione = Convert.ToInt32(elem.Posizione);
                frk_compSpec._prog = 1;
                frk_compSpec._stato = "";
                frk_compSpec._tipoAgg = "";

                cvBox.competenzeSpecialistiche.Add(frk_compSpec);
            }
            ViewBag.idMenu = 43;
            return View("~/Views/CV_Online/CompetenzeSpecialistiche/BoxView.cshtml", cvBox);
        }

        public ActionResult EditCompetenzeSpecialistiche(cvModel.CompetenzeSpecialistiche[] compSpec, string[] flagPrinc)
        {
            if (flagPrinc == null)
            {
                flagPrinc = new string[0];
            }
            cv_ModelEntities cvEnt = new cv_ModelEntities();
            string matricola = Utente.EsponiAnagrafica()._matricola;

            //cancello tutti gli elementi in TCVConProf
            List<TCVConProf> delete_list = cvEnt.TCVConProf.Where(x => x.Matricola == matricola
                  && cvEnt.DConProf.Any(z => z.TipoConProf == "C" && z.CodConProf == x.CodConProf)
                ).ToList();
            foreach (TCVConProf delete in delete_list)
            {
                try
                {
                    cvEnt.TCVConProf.Remove(delete);
                    if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                        throw new Exception( "Errore salvataggio dati DB" );
                }
                catch (Exception ex)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                }
            }

            List<cvModel.CompetenzeSpecialistiche> listaTot = compSpec.Where(x => x._codConProfLiv != null || flagPrinc.Contains(x._codConProf)).ToList();
            foreach (var item in listaTot)
            {
                TCVConProf conProf = new TCVConProf();

                conProf.CodConProf = item._codConProf;
                conProf.CodConProfLiv = item._codConProfLiv;
                conProf.DataOraAgg = DateTime.Now;
                conProf.Matricola = matricola;
                conProf.Prog = 1;
                conProf.Stato = "S";
                conProf.TipoAgg = "I";
                conProf.AltraConProf = "S";

                if (flagPrinc.Contains(item._codConProf))
                {
                    conProf.FlagPrincipale = "1";
                }
                else
                {
                    conProf.FlagPrincipale = "0";
                }

                try
                {
                    cvEnt.TCVConProf.Add(conProf);
                    if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                        throw new Exception( "Errore salvataggio dati DB" );
                }
                catch (Exception ex)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                }
            }
            return Content("Ok");
        }

        #endregion

        #region Controller Certificazioni
        public ActionResult Certificazioni()
        {
            cvBox.certificazioni = new List<cvModel.Certificazioni>();
            cv_ModelEntities cvEnt = new cv_ModelEntities();
            var matricola = Utente.EsponiAnagrafica()._matricola;
            var certificazioni = cvEnt.TCVCertifica.Where(c => c.Matricola == matricola);
            var descalb = cvEnt.DAlboProf.ToList();
            foreach (var certif in certificazioni)
            {
                string dataini = null; ;
                string datafin = null;
                string datapub = null;
                string databrev = null;
                string dataalbo = null;

                if (certif.MeseIni != null)
                {
                    var mese = new DateTime(Convert.ToInt16(certif.AnnoIni), Convert.ToInt16(certif.MeseIni), 01).ToString("MMMM");
                    dataini = mese.Substring(0, 1).ToUpper() + mese.Substring(1) + " " + certif.AnnoIni;
                }

                if (certif.MeseFin != null)
                {
                    var mese = new DateTime(Convert.ToInt16(certif.AnnoFin), Convert.ToInt16(certif.MeseFin), 01).ToString("MMMM");
                    datafin = mese.Substring(0, 1).ToUpper() + mese.Substring(1) + " " + certif.AnnoFin;
                }

                if (certif.DataPubblica != null)
                {
                    datapub = new DateTime(Convert.ToInt16(certif.DataPubblica.Substring(0, 4)), Convert.ToInt16(certif.DataPubblica.Substring(4, 2)), Convert.ToInt16(certif.DataPubblica.Substring(6, 2))).ToString("dd/MM/yyyy");
                }

                if (certif.DataBrevetto != null)
                {
                    databrev = new DateTime(Convert.ToInt16(certif.DataBrevetto.Substring(0, 4)), Convert.ToInt16(certif.DataBrevetto.Substring(4, 2)), Convert.ToInt16(certif.DataBrevetto.Substring(6, 2))).ToString("dd/MM/yyyy");
                }

                if (certif.DataAlboProf != null)
                {
                    dataalbo = new DateTime(Convert.ToInt16(certif.DataAlboProf.Substring(0, 4)), Convert.ToInt16(certif.DataAlboProf.Substring(4, 2)), Convert.ToInt16(certif.DataAlboProf.Substring(6, 2))).ToString("dd/MM/yyyy");
                }

                cvModel.Certificazioni cer = new cvModel.Certificazioni( CommonManager.GetCurrentUserMatricola( ) )
                {
                    _annoFin = certif.AnnoFin ,
                    _annoIni = certif.AnnoIni ,
                    _autCertifica = certif.AutCertifica ,
                    _codAlboProf = certif.CodAlboProf ,
                    _dataAlboProf = dataalbo ,
                    _dataBrevetto = databrev ,
                    _dataOraAgg = certif.DataOraAgg ,
                    _dataPubblica = datapub ,
                    _descAlboProf = descalb.Where( x => x.CodAlboProf == certif.CodAlboProf ).Select( x => x.DescAlboProf ).FirstOrDefault( ) ,
                    _editorePubblica = certif.EditorePubblica ,
                    _flagRegBrevetto = certif.FlagRegBrevetto ,
                    _inventore = certif.Inventore ,
                    _matricola = certif.Matricola ,
                    _meseFin = certif.MeseFin ,
                    _meseIni = certif.MeseIni ,
                    _dataIni = dataini ,
                    _dataFin = datafin ,
                    _nomeCertifica = certif.NomeCertifica ,
                    _noteAlboProf = certif.NoteAlboProf ,
                    _noteBrevetto = certif.NoteBrevetto ,
                    _notePubblica = certif.NotePubblica ,
                    _numBrevetto = certif.NumBrevetto ,
                    _numLicenza = certif.NumLicenza ,
                    _pressoAlboProf = certif.PressoAlboProf ,
                    _prog = certif.Prog ,
                    _tipo = certif.Tipo ,
                    _tipoAgg = certif.TipoAgg ,
                    _tipoBrevetto = certif.TipoBrevetto ,
                    _titoloPubblica = certif.TitoloPubblica ,
                    _uffBrevetto = certif.UffBrevetto ,
                    _urlBrevetto = certif.UrlBrevetto ,
                    _urlCertifica = certif.UrlCertifica ,
                    _urlPubblica = certif.UrlPubblica ,
                    //FREAK - AGGIUNGERE NOTE CERTIFICAZIONE
                    _noteCertifica = certif.NoteCertifica ,
                    AnnoPubblicazione = certif.AnnoPubblicazione ,
                    MesePubblicazione = certif.MesePubblicazione == null ? null : ( ( int ) certif.MesePubblicazione ).ToString( ) ,
                    GiornoPubblicazione = certif.GiornoPubblicazione ,
                    _tipoPubblicazione = certif.TipoPubblicazione ,
                    _tipoPubRiferimento = certif.TipoPubRiferimento ,
                    _riferimentoPub = certif.RiferimentoPub
                };

                cvBox.certificazioni.Add(cer);
            }

            ViewBag.idMenu = 40;
            return View("~/Views/CV_Online/Certificazioni/BoxView.cshtml", cvBox);
        }

        [HttpPost]
        public ActionResult Create_DettaglioAttestato(cvModel.Certificazioni certificazione)
        {
            ModelState.Clear();
            certificazione._tipo = "1";
            return View("~/Views/CV_Online/Certificazioni/DettaglioAttestato.cshtml", certificazione);
        }

        [HttpPost]
        public ActionResult Create_DettaglioPubblicazione(cvModel.Certificazioni certificazione)
        {
            ModelState.Clear();
            certificazione._tipo = "2";
            return View("~/Views/CV_Online/Certificazioni/DettaglioPubblicazione.cshtml", certificazione);
        }

        [HttpPost]
        public ActionResult Create_DettaglioBrevetto(cvModel.Certificazioni certificazione)
        {
            ModelState.Clear();
            certificazione._tipo = "3";
            return View("~/Views/CV_Online/Certificazioni/DettaglioBrevetto.cshtml", certificazione);
        }

        [HttpPost]
        public ActionResult Create_DettaglioAlbo(cvModel.Certificazioni attestato)
        {
            ModelState.Clear();
            attestato._tipo = "4";
            return View("~/Views/CV_Online/Certificazioni/DettaglioAlbo.cshtml", attestato);
        }

        [HttpPost]
        public ActionResult Create_DettaglioPremio(cvModel.Certificazioni certificazione)
        {
            ModelState.Clear();
            certificazione._tipo = "5";
            return View("~/Views/CV_Online/Certificazioni/DettaglioPremio.cshtml", certificazione);
        }

        [HttpPost]
        public ActionResult InsertCertificazioni(cvModel.Certificazioni certificazione)
        {
            string result = "";
            string matricola;
            int prog;
            matricola = Utente.EsponiAnagrafica()._matricola;
            cv_ModelEntities cvEnt = new cv_ModelEntities();
            TCVCertifica certdb = new TCVCertifica();
            certdb.AnnoFin = certificazione._annoFin;
            certdb.AnnoIni = certificazione._annoIni;
            certdb.AutCertifica = certificazione._autCertifica;
            certdb.CodAlboProf = certificazione._codAlboProf;
            if (certificazione._dataAlboProf != null)
            {
                certdb.DataAlboProf = certificazione._dataAlboProf.Substring(6, 4) + certificazione._dataAlboProf.Substring(3, 2) + certificazione._dataAlboProf.Substring(0, 2);
            }
            if (certificazione._dataBrevetto != null)
            {
                certdb.DataBrevetto = certificazione._dataBrevetto.Substring(6, 4) + certificazione._dataBrevetto.Substring(3, 2) + certificazione._dataBrevetto.Substring(0, 2);
            }
            if (certificazione._dataPubblica != null)
            {
                certdb.DataPubblica = certificazione._dataPubblica.Substring(6, 4) + certificazione._dataPubblica.Substring(3, 2) + certificazione._dataPubblica.Substring(0, 2);
            }
            certdb.EditorePubblica = certificazione._editorePubblica;
            certdb.FlagRegBrevetto = certificazione._flagRegBrevetto;
            certdb.Inventore = certificazione._inventore;
            certdb.Matricola = matricola;
            certdb.MeseFin = certificazione._meseFin;
            certdb.MeseIni = certificazione._meseIni;
            certdb.NomeCertifica = certificazione._nomeCertifica;
            certdb.NoteAlboProf = certificazione._noteAlboProf;
            certdb.NoteBrevetto = certificazione._noteBrevetto;
            certdb.NotePubblica = certificazione._notePubblica;
            certdb.NumBrevetto = certificazione._numBrevetto;
            certdb.NumLicenza = certificazione._numLicenza;
            certdb.PressoAlboProf = certificazione._pressoAlboProf;
            certdb.TipoBrevetto = certificazione._tipoBrevetto;
            certdb.TitoloPubblica = certificazione._titoloPubblica;
            certdb.UffBrevetto = certificazione._uffBrevetto;
            certdb.UrlBrevetto = certificazione._urlBrevetto;
            certdb.UrlCertifica = certificazione._urlCertifica;
            certdb.UrlPubblica = certificazione._urlPubblica;
            certdb.TipoPubblicazione = certificazione._tipoPubblicazione;
            certdb.TipoPubRiferimento = certificazione._tipoPubRiferimento;
            certdb.RiferimentoPub = certificazione._riferimentoPub;
            certdb.DataOraAgg = DateTime.Now;
            certdb.TipoAgg = "I";
            certdb.Tipo = certificazione._tipo;
            //FREAK - AGGIUNGERE NOTE CERTIFICAZIONI
            certdb.NoteCertifica = certificazione._noteCertifica;

            certdb.AnnoPubblicazione = certificazione.AnnoPubblicazione;
            if (String.IsNullOrWhiteSpace(certificazione.MesePubblicazione))
                certdb.MesePubblicazione = null;
            else
                certdb.MesePubblicazione = Convert.ToInt32(certificazione.MesePubblicazione);

            if (certificazione.MesePubblicazione != null)
                certdb.GiornoPubblicazione = certificazione.GiornoPubblicazione;


            //    //calcolo del prog
            var tmp = cvEnt.TCVCertifica.Where(x => x.Matricola == matricola);
            if (tmp.Count() == 0)
            {
                prog = 1;
            }
            else
            {
                var nro_prog = (cvEnt.TCVCertifica.Where(x => x.Matricola == matricola)).Max(x => x.Prog);
                prog = Convert.ToInt32(nro_prog) + 1;
            }
            certdb.Prog = prog;

            // verifica se lo stesso campo è stato già inserito
            // ad esempio se ho già inserito che appartengo all'albo dei giornalisti, dall'anno 2018
            // allora il sistema non deve consentirmi di inserire un'altra volta la stessa iscrizione
            bool canSave = true;
            switch (certdb.Tipo)
            {
                case "1": // Attestato
                    canSave = this.CanInsertAttestato(matricola, certdb.NumLicenza, certdb.NomeCertifica);
                    result = "L'attestato è già presente.";
                    break;
                case "2": // Pubblicazione
                    canSave = this.CanInsertPubblicazione(matricola, certdb.DataPubblica, certdb.TitoloPubblica);
                    result = "La pubblicazione è già presente.";
                    break;
                case "3": // Brevetto
                    canSave = this.CanInsertBrevetto(matricola, certdb.NumBrevetto, certdb.TipoBrevetto);
                    result = "Il brevetto è già presente.";
                    break;
                case "4": // Albo
                    canSave = this.CanInsertAlbo(matricola, certdb.DataAlboProf, certdb.PressoAlboProf);
                    result = "L'iscrizione all'albo è già presente.";
                    break;
                case "5": // Premio
                    canSave = this.CanInsertPremio(matricola, certdb.NomeCertifica, certdb.DataBrevetto);
                    result = "Il premio è già presente.";
                    break;
            }

            try
            {
                if (canSave)
                {
                    cvEnt.TCVCertifica.Add(certdb);
                    //cvEnt.SaveChanges();
                    if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                        throw new Exception( "Errore salvataggio dati DB" );
                    result = "ok";
                }
            }
            catch (Exception exc)
            {
                result = exc.Message;
            }

            return Content(result);
        }

        /// <summary>
        /// Verifica se l'albo che si intende inserire è già presente nel db
        /// </summary>
        /// <param name="matricola">Matricola dell'utente per il quale si desidera inserire l'elemento</param>
        /// <param name="dataAlboProf">Data di iscrizione all'albo</param>
        /// <param name="presso">Luogo presso il quale si è iscritto all'albo</param>
        /// <returns>True se è possibile inserire il nuovo elemento, False altrimenti</returns>
        private bool CanInsertAlbo(string matricola, string dataAlboProf, string presso)
        {
            bool iCan = true;
            try
            {
                using (cv_ModelEntities db = new cv_ModelEntities())
                {
                    var element = db.TCVCertifica.Where(c => c.Matricola.Equals(matricola, StringComparison.InvariantCultureIgnoreCase) &&
                        c.Tipo.Equals("4", StringComparison.InvariantCultureIgnoreCase) &&
                        c.DataAlboProf.Equals(dataAlboProf, StringComparison.InvariantCultureIgnoreCase) &&
                        c.PressoAlboProf.Equals(presso, StringComparison.InvariantCultureIgnoreCase)).ToList();

                    if (element != null && element.Any())
                    {
                        iCan = false;
                    }
                }
                return iCan;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matricola"></param>
        /// <param name="numero"></param>
        /// <param name="titolo"></param>
        /// <returns></returns>
        private bool CanInsertBrevetto(string matricola, string numero, string titolo)
        {
            bool iCan = true;
            try
            {
                using (cv_ModelEntities db = new cv_ModelEntities())
                {
                    var element = db.TCVCertifica.Where(c => c.Matricola.Equals(matricola, StringComparison.InvariantCultureIgnoreCase) &&
                        c.Tipo.Equals("3", StringComparison.InvariantCultureIgnoreCase) &&
                        c.TipoBrevetto.Equals(titolo, StringComparison.InvariantCultureIgnoreCase) &&
                        c.NumBrevetto.Equals(numero, StringComparison.InvariantCultureIgnoreCase)).ToList();

                    if (element != null && element.Any())
                    {
                        iCan = false;
                    }
                }
                return iCan;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matricola"></param>
        /// <param name="dataPubblicazione"></param>
        /// <param name="titolo"></param>
        /// <returns></returns>
        private bool CanInsertPubblicazione(string matricola, string dataPubblicazione, string titolo)
        {
            bool iCan = true;
            try
            {
                using (cv_ModelEntities db = new cv_ModelEntities())
                {
                    var element = db.TCVCertifica.Where(c => c.Matricola.Equals(matricola, StringComparison.InvariantCultureIgnoreCase) &&
                        c.Tipo.Equals("2", StringComparison.InvariantCultureIgnoreCase) &&
                        c.DataPubblica.Equals(dataPubblicazione, StringComparison.InvariantCultureIgnoreCase) &&
                        c.TitoloPubblica.Equals(titolo, StringComparison.InvariantCultureIgnoreCase)).ToList();

                    if (element != null && element.Any())
                    {
                        iCan = false;
                    }
                }
                return iCan;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matricola"></param>
        /// <param name="numeroLicenza"></param>
        /// <param name="nomeAttestato"></param>
        /// <returns></returns>
        private bool CanInsertAttestato(string matricola, string numeroLicenza, string nomeAttestato)
        {
            bool iCan = true;
            try
            {
                using (cv_ModelEntities db = new cv_ModelEntities())
                {
                    var element = db.TCVCertifica.Where(c => c.Matricola.Equals(matricola, StringComparison.InvariantCultureIgnoreCase) &&
                        c.Tipo.Equals("1", StringComparison.InvariantCultureIgnoreCase) &&
                        c.NumLicenza.Equals(numeroLicenza, StringComparison.InvariantCultureIgnoreCase) &&
                        c.NomeCertifica.Equals(nomeAttestato, StringComparison.InvariantCultureIgnoreCase)).ToList();

                    if (element != null && element.Any())
                    {
                        iCan = false;
                    }
                }
                return iCan;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="matricola"></param>
        /// <param name="nomePremio"></param>
        /// <returns></returns>
        private bool CanInsertPremio(string matricola, string nomePremio, string datapremio)
        {
            bool iCan = true;
            try
            {
                using (cv_ModelEntities db = new cv_ModelEntities())
                {
                    var element = db.TCVCertifica.Where(c => c.Matricola.Equals(matricola, StringComparison.InvariantCultureIgnoreCase) &&
                        c.Tipo.Equals("5", StringComparison.InvariantCultureIgnoreCase) &&
                        c.NomeCertifica.Equals(nomePremio, StringComparison.InvariantCultureIgnoreCase) &&
                        c.DataBrevetto==datapremio).ToList();

                    if (element != null && element.Any())
                    {
                        iCan = false;
                    }
                }
                return iCan;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [HttpPost]
        public ActionResult DeleteCertificazione(string matricola, int prog)
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities();

            if (String.IsNullOrEmpty(matricola) && (prog <= 0))
            {
                return Content("Dati del contesto non validi");
            }
            TCVCertifica cert = cvEnt.TCVCertifica.Find(matricola, prog);

            if (cert == null)
            {
                return Content("Elemento non trovato");
            }

            try
            {
                cvEnt.TCVCertifica.Remove(cert);
                //cvEnt.SaveChanges();
                if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                    throw new Exception( "Errore salvataggio dati DB" );
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }

            ViewBag.idMenu = 40;
            return Content("ok");
        }

        [HttpPost]
        public ActionResult Create_ModificaDettaglioCertificazione(cvModel.Certificazioni certificazione)
        {

            string matricola = Utente.EsponiAnagrafica()._matricola;
            int prog = certificazione._prog;

            if (String.IsNullOrEmpty(matricola) || prog == 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            switch (certificazione._tipo)
            {
                case "1":
                    return View("~/Views/CV_Online/Certificazioni/ModificaDettaglioAttestato.cshtml", certificazione);
                case "2":

                    cv_ModelEntities db = new cv_ModelEntities();
                    var dbrow = db.TCVCertifica.Where(x => x.Matricola == certificazione._matricola && x.Prog == certificazione._prog).FirstOrDefault();
                    if (dbrow != null)
                    {
                        certificazione.AnnoPubblicazione = dbrow.AnnoPubblicazione;
                        certificazione.MesePubblicazione = dbrow.MesePubblicazione != null ? dbrow.MesePubblicazione.ToString() : null;
                        certificazione.GiornoPubblicazione = dbrow.GiornoPubblicazione;
                    }
                    certificazione.MesiPubblicazione = certificazione.getMesi();
                    certificazione.GiorniPubblicazione = certificazione.getGiorni();
                    return View("~/Views/CV_Online/Certificazioni/ModificaDettaglioPubblicazione.cshtml", certificazione);
                case "3":
                    return View("~/Views/CV_Online/Certificazioni/ModificaDettaglioBrevetto.cshtml", certificazione);
                case "4":
                    return View("~/Views/CV_Online/Certificazioni/ModificaDettaglioAlbo.cshtml", certificazione);
                case "5":
                    return View("~/Views/CV_Online/Certificazioni/ModificaDettaglioPremio.cshtml", certificazione);
            }
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        [HttpPost]
        public ActionResult ModificaCertificazioni(cvModel.Certificazioni certificazione)
        {
            string result = "";
            string matricola = certificazione._matricola;
            int prog = certificazione._prog;

            cv_ModelEntities cvEnt = new cv_ModelEntities();
            TCVCertifica certdb = new TCVCertifica();

            certdb = cvEnt.TCVCertifica.Find(matricola, prog);

            if (certdb == null)
            {
                result = "Errore nel reperimento dati";
                return Content(result);
            }

            certdb.AnnoFin = certificazione._annoFin;
            certdb.AnnoIni = certificazione._annoIni;
            certdb.AutCertifica = certificazione._autCertifica;
            certdb.CodAlboProf = certificazione._codAlboProf;

            if (certificazione._dataAlboProf != null)
            {
                certdb.DataAlboProf = certificazione._dataAlboProf.Substring(6, 4) + certificazione._dataAlboProf.Substring(3, 2) + certificazione._dataAlboProf.Substring(0, 2);
            }
            else
            {
                certdb.DataAlboProf = null;
            }

            if (certificazione._dataBrevetto != null)
            {
                certdb.DataBrevetto = certificazione._dataBrevetto.Substring(6, 4) + certificazione._dataBrevetto.Substring(3, 2) + certificazione._dataBrevetto.Substring(0, 2);
            }
            else
            {
                certdb.DataBrevetto = null;
            }

            if (certificazione._dataPubblica != null)
            {
                certdb.DataPubblica = certificazione._dataPubblica.Substring(6, 4) + certificazione._dataPubblica.Substring(3, 2) + certificazione._dataPubblica.Substring(0, 2);
            }
            else
            {
                certdb.DataPubblica = null;
            }
            certdb.AnnoPubblicazione = certificazione.AnnoPubblicazione;

            if (String.IsNullOrWhiteSpace(certificazione.MesePubblicazione))
            {
                certdb.MesePubblicazione = null;
                certdb.GiornoPubblicazione = null;
            }
            else certdb.MesePubblicazione = Convert.ToInt32(certificazione.MesePubblicazione);

            if (certdb.MesePubblicazione != null)
                certdb.GiornoPubblicazione = certificazione.GiornoPubblicazione;

            Regex rx = new Regex(@"^(ht|f)tp(s?)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_=]*)?$");
            if (certificazione._tipoPubblicazione == "DIGITALE")
            {
                if (certificazione._riferimentoPub==null || !rx.IsMatch(certificazione._riferimentoPub))
                {
                    result = "Inserire un URL di riferimento";
                    return Content(result);
                }
            }
            if (certificazione._tipoPubblicazione != null)
            {

                if (certificazione._tipoPubRiferimento == null || certificazione._riferimentoPub == null)
                {
                    result = "Specificare i dettagli sul tipo pubblicazione";
                    return Content(result);
                }
            }
            certdb.EditorePubblica = certificazione._editorePubblica;
            certdb.FlagRegBrevetto = certificazione._flagRegBrevetto;
            certdb.Inventore = certificazione._inventore;
            certdb.Matricola = matricola;
            certdb.MeseFin = certificazione._meseFin;
            certdb.MeseIni = certificazione._meseIni;
            certdb.NomeCertifica = certificazione._nomeCertifica;
            certdb.NoteAlboProf = certificazione._noteAlboProf;
            certdb.NoteBrevetto = certificazione._noteBrevetto;
            certdb.NotePubblica = certificazione._notePubblica;
            certdb.NumBrevetto = certificazione._numBrevetto;
            certdb.NumLicenza = certificazione._numLicenza;
            certdb.PressoAlboProf = certificazione._pressoAlboProf;
            certdb.TipoBrevetto = certificazione._tipoBrevetto;
            certdb.TitoloPubblica = certificazione._titoloPubblica;
            certdb.UffBrevetto = certificazione._uffBrevetto;
            certdb.UrlBrevetto = certificazione._urlBrevetto;
            certdb.UrlCertifica = certificazione._urlCertifica;
            certdb.UrlPubblica = certificazione._urlPubblica;
            certdb.TipoPubblicazione = certificazione._tipoPubblicazione;
            certdb.TipoPubRiferimento = certificazione._tipoPubRiferimento;
            certdb.RiferimentoPub = certificazione._riferimentoPub;
            certdb.DataOraAgg = DateTime.Now;
            certdb.TipoAgg = "A";
            certdb.NoteCertifica = certificazione._noteCertifica;

            try
            {
                //cvEnt.SaveChanges();
                if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                    throw new Exception( "Errore salvataggio dati DB" );
                result = "ok";
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return Content(result);
        }

        private string VerificaObbligatori(cvModel.Certificazioni certificazione)
        {
            string error = null;
            if (certificazione._tipo == "1")
            {
                if (string.IsNullOrWhiteSpace(certificazione._nomeCertifica) || string.IsNullOrEmpty(certificazione._nomeCertifica))
                {
                    error += "campo nome certificazione obbligatorio;";
                }


                if (string.IsNullOrWhiteSpace(certificazione._dataIni) || string.IsNullOrEmpty(certificazione._dataIni))
                {
                    error += "campo data da obbligatorio;";
                }
                if (string.IsNullOrWhiteSpace(certificazione._dataFin) || string.IsNullOrEmpty(certificazione._dataFin))
                {
                    error += "campo data a obbligatorio;";
                }


            }
            if (certificazione._tipo == "2")
            {
                if (string.IsNullOrWhiteSpace(certificazione._titoloPubblica) || string.IsNullOrEmpty(certificazione._titoloPubblica))
                {
                    error += "campo titolo pubblicazione obbligatorio;";
                }
                if (string.IsNullOrWhiteSpace(certificazione._dataPubblica) || string.IsNullOrEmpty(certificazione._dataPubblica))
                {
                    error += "campo data pubblicazione;";
                }

            }
            if (certificazione._tipo == "3")
            {
                if (string.IsNullOrWhiteSpace(certificazione._tipoBrevetto) || string.IsNullOrEmpty(certificazione._tipoBrevetto))
                {
                    error += "campo titolo brevetto obbligatorio;";
                }
                if (string.IsNullOrWhiteSpace(certificazione._flagRegBrevetto) || string.IsNullOrEmpty(certificazione._flagRegBrevetto))
                {
                    error += "selezionare lo stato del brevetto;";
                }
                if (string.IsNullOrWhiteSpace(certificazione._dataBrevetto) || string.IsNullOrEmpty(certificazione._dataBrevetto))
                {
                    error += "campo data concessione obbligatorio;";
                }
            }
            if (certificazione._tipo == "4")
            {
                if (string.IsNullOrWhiteSpace(certificazione._codAlboProf) || string.IsNullOrEmpty(certificazione._codAlboProf))
                {
                    error += "selezionare albo professionale;";
                }
                if (string.IsNullOrWhiteSpace(certificazione._dataAlboProf) || string.IsNullOrEmpty(certificazione._dataAlboProf))
                {
                    error += "campo data iscrizione obbligatorio;";
                }
            }
            if (certificazione._tipo == "5")
            {
                if (string.IsNullOrWhiteSpace(certificazione._nomeCertifica) || string.IsNullOrEmpty(certificazione._nomeCertifica))
                {
                    error += "campo nome premio obbligatorio;";
                }

                if (string.IsNullOrWhiteSpace(certificazione._dataIni) || string.IsNullOrEmpty(certificazione._dataIni))
                {
                    error += "campo data da obbligatorio;";
                }
            }
            if (!ModelState.IsValid)
            {
                var errori = ModelState.Values.Where(E => E.Errors.Count > 0).SelectMany(E => E.Errors).Select(E => E.ErrorMessage).ToList();

                foreach (var err in errori)
                {
                    error += err + ";";

                }
            }
            return error;
        }

        public static List<System.Web.UI.WebControls.ListItem> getTipiPubblicazione()
        {
            List<System.Web.UI.WebControls.ListItem> ls = new List<System.Web.UI.WebControls.ListItem>();
            ls.Add(new System.Web.UI.WebControls.ListItem("CARTACEA", "CARTACEA"));
            ls.Add(new System.Web.UI.WebControls.ListItem("DIGITALE", "DIGITALE"));
            return ls;
        }
        #endregion

        #region Controller Allegati
        public ActionResult Allegati()
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities();
            cvBox.allegati = new List<cvModel.Allegati>();
            string matricola = Utente.EsponiAnagrafica()._matricola;
            var allegati = cvEnt.TCVAllegato.Where(x => x.Matricola == matricola && x.Stato != "#").ToList();

            foreach (TCVAllegato all in allegati)
            {
                cvModel.Allegati frk_all = new cvModel.Allegati();

                frk_all._contentType = all.ContentType;
                frk_all._dataOraAgg = all.DataOraAgg;
                frk_all._id = all.Id;
                frk_all._idBox = all.Id_box;
                frk_all._matricola = matricola;
                frk_all._name = all.Name;
                frk_all._pathName = all.Path_name;
                frk_all._size = all.Size;
                frk_all._stato = all.Stato;
                frk_all._tipoAgg = all.TipoAgg;
                frk_all._note = all.Note;

                cvBox.allegati.Add(frk_all);
            }
            ViewBag.idMenu = 23;
            return View("~/Views/CV_Online/Allegati/BoxView.cshtml", cvBox);
        }

        [HttpPost]
        public ActionResult GetParameterAllegati(string dimensioneFile)
        {
            string matricola = Utente.EsponiAnagrafica()._matricola;
            cv_ModelEntities cvEnt = new cv_ModelEntities();
            if (cvEnt.TCVAllegato.Where(a => a.Matricola == matricola && a.Stato != "#").Count() >= Convert.ToInt32(CommonManager.GetParametri<string>(EnumParametriSistema.NumeriMassimoAllegati)[0]))
            {
                return Content("Non si possono inserire più di " + CommonManager.GetParametri<string>(EnumParametriSistema.NumeriMassimoAllegati)[0] + " Allegati");
            }
            else
            {
                if (Convert.ToDouble(Convert.ToDouble(dimensioneFile) / 1024 / 1024) > Convert.ToDouble(CommonManager.GetParametri<string>(EnumParametriSistema.DimensioneMassimaAllegati)[0]))
                {
                    return Content("Dimensione massima consentita " + CommonManager.GetParametri<string>(EnumParametriSistema.DimensioneMassimaAllegati)[0] + " MB");

                }
                else
                {
                    return Content("");
                }
            }
        }

        [HttpPost]
        public ActionResult InsertAllegati(cvModel.Allegati allegato, HttpPostedFileBase _fileUpload)
        {
            string matricola;
            string rootPath;
            string basePath;
            string pathComplete;
            string pathForAllegato;
            string configPath = String.Empty;

            // reperimento del path della directory virtuale
            using (digiGappEntities db = new digiGappEntities())
            {
                var row = db.MyRai_ParametriSistema.Where(p => p.Chiave.Equals("ShareAllegati", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                if (row != null)
                {
                    configPath = row.Valore1;
                }
                else
                {
                    configPath = "\\cv_media";
                }
            }

            matricola = Utente.EsponiAnagrafica()._matricola;
            var tmp = "~";

            rootPath = Path.Combine(Server.MapPath(tmp), configPath);

            basePath = Path.Combine(rootPath, matricola);

            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }
            //salvataggio del file in allegato o setto il pathName
            if ((_fileUpload != null) && (allegato._pathName == null))
            {
                //pathComplete = basePath + Path.GetFileName(_fileUpload.FileName);

                pathComplete = Path.Combine(basePath, Path.GetFileName(_fileUpload.FileName));
                pathForAllegato = Path.GetFileName(_fileUpload.FileName);

                try
                {
                    _fileUpload.SaveAs(pathComplete);
                }
                catch (Exception exc)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
            else
            {
                pathForAllegato = allegato._pathName;
            }
            cv_ModelEntities cvEnt = new cv_ModelEntities();
            TCVAllegato tmp_all = new TCVAllegato();

            if ((_fileUpload != null) && (allegato._pathName == null))
            {
                if (_fileUpload.ContentType.Length >= 50)
                {
                    tmp_all.ContentType = _fileUpload.ContentType.Substring(0, 50);
                }
                else
                {
                    tmp_all.ContentType = _fileUpload.ContentType;
                }
                tmp_all.Size = _fileUpload.ContentLength;
            }
            else
            {
                tmp_all.ContentType = "website";
                tmp_all.Size = 0;
            }
            tmp_all.DataOraAgg = DateTime.Now;
            tmp_all.Id_box = null;
            tmp_all.Matricola = matricola;
            tmp_all.Name = allegato._name;
            tmp_all.Path_name = pathForAllegato;
            tmp_all.Note = allegato._note;
            tmp_all.Stato = "S";
            tmp_all.TipoAgg = "I";

            try
            {
                cvEnt.TCVAllegato.Add(tmp_all);
                //cvEnt.SaveChanges();
                if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                    throw new Exception( "Errore salvataggio dati DB" );
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.idMenu = 23;
            return Content("Ok");
        }

        [HttpGet]
        public ActionResult DeleteAllegati(int id)
        {
            string basepath;
            cv_ModelEntities cvEnt = new cv_ModelEntities();

            string matricola = Utente.EsponiAnagrafica()._matricola;

            TCVAllegato allegato = cvEnt.TCVAllegato.Find(id);
            if (allegato == null)
            {
                return HttpNotFound();
            }
            try
            {
                //cancellaione del file allegato
                //var tmp = Request.AppRelativeCurrentExecutionFilePath;
                basepath = CancellaAllegatoFisico(matricola, allegato);
                cvEnt.TCVAllegato.Remove(allegato);

                if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                    throw new Exception( "Errore salvataggio dati DB" );
            }
            catch (Exception exc)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ViewBag.idMenu = 23;
            return Content("Ok");
        }

        private string CancellaAllegatoFisico(string matricola, TCVAllegato allegato)
        {
            string basepath;
            var tmp = "~";
            basepath = Server.MapPath(tmp);

            string configPath = String.Empty;

            // reperimento del path della directory virtuale
            using (digiGappEntities db = new digiGappEntities())
            {
                var row = db.MyRai_ParametriSistema.Where(p => p.Chiave.Equals("ShareAllegati", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                if (row != null)
                {
                    configPath = row.Valore1;
                }
                else
                {
                    configPath = "\\cv_media";
                }
            }

            string path = Path.Combine(basepath, configPath, matricola, allegato.Path_name);

            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
            return basepath;
        }

        public ActionResult GetFile(int id)
        {
            string path = String.Empty;
            string matricola = String.Empty;
            string filename = String.Empty;
            string contentType = String.Empty;

            string configPath = String.Empty;

            // reperimento del path della directory virtuale
            configPath = GetShareAllegati();

            FileController fControl = new FileController();

            using (cv_ModelEntities db = new cv_ModelEntities())
            {
                var allegato = db.TCVAllegato.Where(a => a.Id == id).FirstOrDefault();

                if (allegato != null)
                {
                    matricola = allegato.Matricola;
                    path = Path.Combine(configPath, matricola);
                    filename = allegato.Path_name;
                    contentType = allegato.ContentType;
                }
            }

            string filePath = Path.Combine(Server.MapPath("~"), path, filename);

            return fControl.Get(filePath, matricola, CommonManager.PdfAutorizzato, contentType);
        }

        private string GetShareAllegati()
        {
            return CV_OnlineManager.GetShareAllegati();
        }

        public ActionResult ModificaAllegato(int idAllegato)
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities();
            TCVAllegato tmp_all = null;
            if (idAllegato > 0)
                tmp_all = cvEnt.TCVAllegato.FirstOrDefault(x => x.Id == idAllegato);
            else
                tmp_all = new TCVAllegato();

            cvModel.Allegati autopres = new cvModel.Allegati();
            autopres._contentType = tmp_all.ContentType;
            autopres._dataOraAgg = tmp_all.DataOraAgg;
            autopres._id = tmp_all.Id;
            autopres._idBox = tmp_all.Id_box;
            autopres._matricola = tmp_all.Matricola;
            autopres._name = tmp_all.Name;
            autopres._pathName = tmp_all.Path_name;
            autopres._size = tmp_all.Size;
            autopres._stato = tmp_all.Stato;
            autopres._tipoAgg = tmp_all.TipoAgg;
            autopres._note = tmp_all.Note;

            return View("~/Views/CV_Online/Allegati/_modalInserimento.cshtml", autopres);
        }

        public ActionResult UpdateAllegato(cvModel.AutoPresentazioneBox allegato, HttpPostedFileBase _fileUpload)
        {
            string matricola;
            string rootPath;
            string basePath;
            string pathComplete;
            string pathForAllegato;
            string configPath = String.Empty;

            // reperimento del path della directory virtuale
            using (digiGappEntities db = new digiGappEntities())
            {
                var row = db.MyRai_ParametriSistema.Where(p => p.Chiave.Equals("ShareAllegati", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                if (row != null)
                {
                    configPath = row.Valore1;
                }
                else
                {
                    configPath = "\\cv_media";
                }
            }

            matricola = Utente.EsponiAnagrafica()._matricola;
            var tmp = "~";

            rootPath = Path.Combine(Server.MapPath(tmp), configPath);

            basePath = Path.Combine(rootPath, matricola);

            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            cv_ModelEntities cvEnt = new cv_ModelEntities();
            TCVAllegato tmp_all = null;
            if (allegato._id > 0)
                tmp_all = cvEnt.TCVAllegato.FirstOrDefault(x => x.Id == allegato._id);
            else
                tmp_all = new TCVAllegato();

            if (allegato._id > 0 && ((_fileUpload != null) || tmp_all.Path_name != allegato._pathName))
            {
                CancellaAllegatoFisico(matricola, tmp_all);
            }

            //salvataggio del file in allegato o setto il pathName
            if ((_fileUpload != null) && (allegato._pathName == null))
            {
                //pathComplete = basePath + Path.GetFileName(_fileUpload.FileName);
                pathComplete = Path.Combine(basePath, Path.GetFileName(_fileUpload.FileName));
                pathForAllegato = Path.GetFileName(_fileUpload.FileName);

                try
                {
                    _fileUpload.SaveAs(pathComplete);
                }
                catch (Exception exc)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
            }
            else
            {
                pathForAllegato = allegato._pathName;
            }

            if ((_fileUpload != null) && (allegato._pathName == null))
            {
                if (_fileUpload.ContentType.Length >= 50)
                {
                    tmp_all.ContentType = _fileUpload.ContentType.Substring(0, 50);
                }
                else
                {
                    tmp_all.ContentType = _fileUpload.ContentType;
                }
                tmp_all.Size = _fileUpload.ContentLength;
            }
            else if (tmp_all.Path_name != allegato._pathName)
            {
                tmp_all.ContentType = "website";
                tmp_all.Size = 0;
            }
            tmp_all.DataOraAgg = DateTime.Now;
            tmp_all.Id_box = null;
            tmp_all.Matricola = matricola;
            tmp_all.Name = allegato._name;
            tmp_all.Path_name = pathForAllegato;
            tmp_all.Note = allegato._note;
            tmp_all.Stato = "S";
            tmp_all.TipoAgg = "I";

            try
            {
                if (allegato._id == 0)
                    cvEnt.TCVAllegato.Add(tmp_all);

                //cvEnt.SaveChanges();
                if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                    throw new Exception( "Errore salvataggio dati DB" );
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.idMenu = 23;
            return Content("Ok");
        }

        #endregion

        #region Controller Autopresentazione
        public ActionResult AutoPresentazione()
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities();
            cvBox.AutoPresentazione = new cvModel.AutoPresentazioneBox();
            //carico il modello CvModel.allegati
            string matricola = Utente.EsponiAnagrafica()._matricola;
            var autoPres = cvEnt.TCVAllegato.FirstOrDefault(x => x.Matricola == matricola && x.Stato == "#");

            if (autoPres != null)
            {
                cvBox.AutoPresentazione._contentType = autoPres.ContentType;
                cvBox.AutoPresentazione._dataOraAgg = autoPres.DataOraAgg;
                cvBox.AutoPresentazione._id = autoPres.Id;
                cvBox.AutoPresentazione._idBox = autoPres.Id_box;
                cvBox.AutoPresentazione._matricola = autoPres.Matricola;
                cvBox.AutoPresentazione._name = autoPres.Name;
                cvBox.AutoPresentazione._pathName = autoPres.Path_name;
                cvBox.AutoPresentazione._size = autoPres.Size;
                cvBox.AutoPresentazione._stato = autoPres.Stato;
                cvBox.AutoPresentazione._tipoAgg = autoPres.TipoAgg;
                cvBox.AutoPresentazione._note = autoPres.Note;
            }
            //---------------------------------
            ViewBag.idMenu = 45;
            return View("~/Views/CV_Online/Autopresentazione/BoxView.cshtml", cvBox);
        }

        [HttpPost]
        public ActionResult InsertAutopresentazione(cvModel.AutoPresentazioneBox allegato, HttpPostedFileBase inputAttachFile)
        {
            string matricola;
            string rootPath;
            string basePath;
            string pathComplete;
            string pathForAllegato;
            string configPath = String.Empty;

            // reperimento del path della directory virtuale
            using (digiGappEntities db = new digiGappEntities())
            {
                var row = db.MyRai_ParametriSistema.Where(p => p.Chiave.Equals("ShareAllegati", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();

                if (row != null)
                {
                    configPath = row.Valore1;
                }
                else
                {
                    configPath = "\\cv_media";
                }
            }

            matricola = Utente.EsponiAnagrafica()._matricola;

            cv_ModelEntities cvEnt = new cv_ModelEntities();
            TCVAllegato tmp_all = null;
            if (allegato._id > 0)
                tmp_all = cvEnt.TCVAllegato.FirstOrDefault(x => x.Id == allegato._id);
            else
                tmp_all = new TCVAllegato();

            if (String.IsNullOrWhiteSpace(allegato._pathName) && String.IsNullOrWhiteSpace(allegato._note))
            {
                //cancellaione del file allegato
                CancellaAllegatoFisico(matricola, tmp_all);
                cvEnt.TCVAllegato.Remove(tmp_all);
            }
            else
            {
                bool needUpload = false;

                if (!String.IsNullOrWhiteSpace(tmp_all.Path_name))
                {
                    if (tmp_all.Path_name != allegato._pathName)
                    {
                        needUpload = true;
                        CancellaAllegatoFisico(matricola, tmp_all);
                        tmp_all.Name = "";
                        tmp_all.Path_name = "";
                        tmp_all.Size = 0;
                        tmp_all.ContentType = "";
                    }
                }
                else
                    needUpload = true;

                if (!String.IsNullOrWhiteSpace(allegato._pathName) && needUpload)
                {
                    var tmp = "~";

                    rootPath = Path.Combine(Server.MapPath(tmp), configPath);
                    basePath = Path.Combine(rootPath, matricola);

                    if (!Directory.Exists(basePath))
                        Directory.CreateDirectory(basePath);

                    //salvataggio del file in allegato o setto il pathName
                    if (inputAttachFile != null)
                    {
                        pathComplete = Path.Combine(basePath, Path.GetFileName(inputAttachFile.FileName));
                        pathForAllegato = Path.GetFileName(inputAttachFile.FileName);

                        try
                        {
                            inputAttachFile.SaveAs(pathComplete);
                        }
                        catch (Exception exc)
                        {
                            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                        }
                    }

                    if (inputAttachFile.ContentType.Length >= 50)
                        tmp_all.ContentType = inputAttachFile.ContentType.Substring(0, 50);
                    else
                        tmp_all.ContentType = inputAttachFile.ContentType;
                    tmp_all.Size = inputAttachFile.ContentLength;
                    tmp_all.Path_name = inputAttachFile.FileName;
                    tmp_all.Name = allegato._name;
                }


                tmp_all.DataOraAgg = DateTime.Now;
                tmp_all.Id_box = null;
                tmp_all.Matricola = matricola;
                tmp_all.Note = allegato._note;
                tmp_all.Stato = "#";
                tmp_all.TipoAgg = "I";
            }

            try
            {
                if (allegato._id == 0)
                    cvEnt.TCVAllegato.Add(tmp_all);
                //cvEnt.SaveChanges();
                if ( !DBHelper.Save( cvEnt , CommonManager.GetCurrentUserMatricola( ) ) )
                    throw new Exception( "Errore salvataggio dati DB" );
            }
            catch (Exception exc)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            ViewBag.idMenu = 45;
            return Content("ok");
        }

        [HttpPost]
        public ActionResult ModificaAutopresentazione(int idAutopresentazione)
        {
            cv_ModelEntities cvEnt = new cv_ModelEntities();
            TCVAllegato tmp_all = null;
            if (idAutopresentazione > 0)
                tmp_all = cvEnt.TCVAllegato.FirstOrDefault(x => x.Id == idAutopresentazione);
            else
                tmp_all = new TCVAllegato();

            cvModel.AutoPresentazioneBox autopres = new cvModel.AutoPresentazioneBox();
            autopres._contentType = tmp_all.ContentType;
            autopres._dataOraAgg = tmp_all.DataOraAgg;
            autopres._id = tmp_all.Id;
            autopres._idBox = tmp_all.Id_box;
            autopres._matricola = tmp_all.Matricola;
            autopres._name = tmp_all.Name;
            autopres._pathName = tmp_all.Path_name;
            autopres._size = tmp_all.Size;
            autopres._stato = tmp_all.Stato;
            autopres._tipoAgg = tmp_all.TipoAgg;
            autopres._note = tmp_all.Note;

            return View("~/Views/CV_Online/Autopresentazione/partials/_modalInserimento.cshtml", autopres);
        }
        #endregion

        #region Controller Mappatura
        public bool VerificaRequisitiMappatura(string matricola, out string errore)
        {
            bool isEnabled = false;
            errore = "";

            using (cv_ModelEntities ent = new cv_ModelEntities())
            {
                TSVPrenElencoDip t = ent.TSVPrenElencoDip.Where(x => x.Matricola == matricola).FirstOrDefault();
                if (t == null)
                {
                    errore += "La matricola " + matricola + " non è nell'elenco dipendenti abilitati\r\n";
                }
                else
                {
                    if (t.StatoPrenota == "A")
                    {
                        errore += "La matricola " + matricola + " non è ancora abilitata (stato A)";
                    }
                    else
                    {
                        int perc = CommonManager.GetPercentualCV(matricola);
                        if (perc < 100)
                        {
                            errore += "Non è stata completata la compilazione del CV.\r\n";
                        }

                        //if (ent.TCVConProf.Where(x => x.Matricola == matricola).Count() < 4)
                        //{
                        //    errore += "Non sono state indicate le minime competenze specialistiche.\r\n";
                        //}
                        //else if (ent.TCVIstruzione.Where(x => x.Matricola == matricola).Count() == 0)
                        //{
                        //    errore += "Non sono stati indicati i titoli di studio.\r\n";
                        //}
                        //else if (ent.TCVEsperExRai.Where(x => x.Matricola == matricola && x.FlagEspRai == "1").Count() == 0)
                        //{
                        //    errore += "Non sono state indicate le esperienze professionali RAI\r\n";
                        //}
                        else
                        {
                            DateTime dateRif = DateTime.Today;
                            TSVPrenSlot tpslot = ent.TSVPrenSlot.Where(x => x.CodGruppoValutati == t.CodGruppoValutati && x.OrarioInizioDispo != x.OrarioFineDispo && x.DataDispo> dateRif)
                                .OrderBy(x => x.DataDispo)
                                .FirstOrDefault();

                            if ((tpslot != null) && (tpslot.CodGruppoValutati.CompareTo("999") < 0)) //blocco  eliminato dal gruppo 025 (TGR Lazio) - 09/01/2017
                            {
                                if (DateTime.Now > tpslot.DataDispo) //datadispo è 00:00
                                {
                                    errore += "E' troppo tardi per effettuare la prenotazione.\r\n";
                                }
                            }
                        }
                    }
                }
            }

            isEnabled = String.IsNullOrWhiteSpace(errore);
            return isEnabled;
        }

        public ActionResult ShowMappatura(string filtroData = "", string orario = "")
        {
            string matricola = CommonManager.GetCurrentUserMatricola( );
            cv_ModelEntities db = new cv_ModelEntities();
            var prenDip = db.TSVPrenElencoDip.FirstOrDefault(x => x.Matricola == matricola);
            DateTime dateRif = DateTime.Today;

            cvModel.Mappatura mappatura = new cvModel.Mappatura();
            mappatura.CanBook = true;

            if (prenDip == null)
            {
                mappatura.CanBook = false;
                mappatura.MessageCannotBook = "La matricola non risulta appartenente ad alcun gruppo.";
            }
            else
            {
                mappatura.Prenotazione = db.TSVPrenPrenota.FirstOrDefault(x => x.Matricola == matricola);
                if (mappatura.Prenotazione != null)
                {
                    mappatura.prenSlot = db.TSVPrenSlot.FirstOrDefault(x => x.Id_Slot == mappatura.Prenotazione.Id_Slot);
                    mappatura.prenStanza = db.TSVPrenStanza.FirstOrDefault(x => x.Id_Stanza == mappatura.prenSlot.Id_Stanza);
                }

                mappatura.ElencoSlot = db.TSVPrenSlot.Where(x => x.DataDispo > dateRif && x.CodGruppoValutati == prenDip.CodGruppoValutati && x.NumPostiDispo > 0);
                if (mappatura.ElencoSlot.Count() > 0)
                {
                    var tmpElencoStanze = mappatura.ElencoSlot.Select(x => x.Id_Stanza).Distinct();
                    mappatura.ElencoStanze = db.TSVPrenStanza.Where(x => tmpElencoStanze.Contains(x.Id_Stanza));
                }
                else
                {
                    mappatura.CanBook = false;
                    mappatura.MessageCannotBook = "Non ci sono slot disponibili";
                }

                if (mappatura.prenSlot != null && dateRif.AddDays(3) < mappatura.ElencoSlot.Min(x => x.DataDispo))
                    mappatura.CanModify = true;
            }

            return View("~/Views/CV_Online/Mappatura/_modalPrenotazione.cshtml", mappatura);
        }

        [HttpPost]
        public ActionResult CancellaPrenotazione(int idPrenotazione)
        {
            string matricola = CommonManager.GetCurrentUserMatricola( );
            string contentResult = "OK";

            using (cv_ModelEntities db = new cv_ModelEntities())
            {
                TSVPrenPrenota prenotazione = db.TSVPrenPrenota.FirstOrDefault(x => x.Id_Prenota == idPrenotazione);
                if (prenotazione != null)
                {
                    TSVPrenSlot slot = db.TSVPrenSlot.FirstOrDefault(x => x.Id_Slot == prenotazione.Id_Slot);
                    if (slot != null)
                    {
                        slot.NumPostiDispo++;
                        db.TSVPrenPrenota.Remove(prenotazione);

                        TSVPrenElencoDip tpDip = db.TSVPrenElencoDip.FirstOrDefault(x => x.Matricola == matricola);
                        if (tpDip != null && tpDip.StatoPrenota != "S")
                            tpDip.StatoPrenota = "N";

                        if (!DBHelper.Save(db, "CV Cancellazione prenotazione mappatura"))
                            contentResult = "Errore durante la cancellazione della prenotazione";
                        else
                        {
                            MappaturaInviaMail(matricola, "CANC");
                        }
                    }
                    else
                    {
                        contentResult = "Slot non trovato";
                    }
                }
                else
                {
                    contentResult = "Prenotazione non trovata";
                }
            }
            return Content(contentResult);
        }

        [HttpPost]
        public ActionResult SalvaPrenotazione(int idSlot, bool sovrascrivi)
        {
            string contentResult = "OK";
            string matricola = CommonManager.GetCurrentUserMatricola( );

            using (cv_ModelEntities db = new cv_ModelEntities())
            {
                bool canBook = true;
                TSVPrenPrenota prenotazione = db.TSVPrenPrenota.FirstOrDefault(x => x.Matricola == matricola);
                if (prenotazione != null)
                {
                    if (sovrascrivi)
                    {
                        TSVPrenSlot slot = db.TSVPrenSlot.FirstOrDefault(x => x.Id_Slot == prenotazione.Id_Slot);
                        if (slot != null)
                        {
                            slot.NumPostiDispo++;
                            db.TSVPrenPrenota.Remove(prenotazione);
                        }
                        else
                        {
                            contentResult = "Slot prenotazione precedente non trovato";
                            canBook = false;
                        }
                    }
                    else
                    {
                        contentResult = "Hai già effettuato una prenotazione";
                        canBook = false;
                    }
                }

                if (canBook)
                {
                    TSVPrenSlot slot = db.TSVPrenSlot.FirstOrDefault(x => x.Id_Slot == idSlot);
                    if (slot != null)
                    {
                        if (slot.NumPostiDispo > 0)
                        {
                            slot.NumPostiDispo--;
                            prenotazione = new TSVPrenPrenota()
                            {
                                Matricola = matricola,
                                Id_Slot = idSlot,
                                TipoAgg = "I",
                                DataOraAgg = DateTime.Now
                            };
                            db.TSVPrenPrenota.Add(prenotazione);

                            TSVPrenElencoDip tpDip = db.TSVPrenElencoDip.FirstOrDefault(x => x.Matricola == matricola);
                            if (tpDip != null)
                                tpDip.StatoPrenota = "P";

                            if (!DBHelper.Save(db, "CV Prenotazione mappatura"))
                                contentResult = "Errore durante la " + (sovrascrivi ? "modifica della prenotazione" : "prenotazione");
                            else
                            {

                                //Invio mail
                                TSVPrenStanza stanza = db.TSVPrenStanza.FirstOrDefault(x => x.Id_Stanza == slot.Id_Stanza);
                                Data.CurriculumVitae.DSedeContabile sedeCont = db.DSedeContabile.FirstOrDefault(x => x.codice == stanza.CodSedeCont);

                                MappaturaInviaMail(matricola, "ADD",
                                    slot.DataDispo.Value.ToString("dd/MM/yyyy"),
                                    sedeCont.descrizione ?? "-",
                                    stanza.DesStanza,
                                    slot.OrarioInizioDispo.Value.ToString("HH:mm") + " - " + slot.OrarioFineDispo.Value.ToString("HH:mm"),
                                    stanza.Link);
                            }
                        }
                        else
                        {
                            contentResult = "Lo slot non ha più posti disponibli";
                        }
                    }
                    else
                    {
                        contentResult = "Slot non trovato";
                    }
                }
            }

            return Content(contentResult);
        }

        private void MappaturaInviaMail(string matricola, string operation, string dataPren = "", string sedepren = "", string stanzaPren = "", string orarioPren = "", string linkRiunione="")
        {

            string[] mailParam;
            List<Attachement> Allegati = new List<Attachement>();

            if (operation == "ADD")
            {
                mailParam = CommonManager.GetParametri<string>(EnumParametriSistema.MailMappaturaPrenotazione);
                mailParam[0] = mailParam[0]
                    .Replace("#dataPren", dataPren)
                    .Replace("#sedePren", sedepren)
                    .Replace("#stanzaPren", stanzaPren)
                    .Replace("#orarioPren", orarioPren)
                    .Replace("#urlPren", linkRiunione);

                //FileStreamResult something = Pdf() as FileStreamResult;
                //Attachement pdf = new Attachement();
                //pdf.AttachementName = "CV " + Utente.Nominativo() + ".pdf";
                //pdf.AttachementType = something.ContentType;
                //using (MemoryStream ms = new MemoryStream())
                //{
                //    something.FileStream.CopyTo(ms);
                //    pdf.AttachementValue = ms.ToArray();
                //}

                //Allegati.Add(pdf);
            }
            else
            {
                mailParam = CommonManager.GetParametri<string>(EnumParametriSistema.MailMappaturaCancellazione);
            }

            mailParam[0] = mailParam[0].Replace("#sitoCV", "");

            var mail = new myRaiCommonTasks.GestoreMail();
            mail.InvioMail(mailParam[0], mailParam[1], CommonManager.GetEmailPerMatricola(matricola), null, "[CG] RaiPlace - Self Service <raiplace.selfservice@rai.it>", Allegati);

            }
        #endregion

        public string GetFiguraProfessionaleForzata(string matricola)
        {
            return CV_OnlineManager.GetFiguraProfessionaleForzata(matricola);
        }
        
        public ActionResult PdfTest(string matricola = null, string msgUltimoAgg = null)
        {
            if (Server != null)
            {
                FontManager.FontPath = Server.MapPath("~/assets/fontG/open-sans-v13-latin-300.ttf");
            }
            CultureInfo culture = new CultureInfo("it-IT");
            TextInfo textInfo = culture.TextInfo;

            Utente.Anagrafica anagrafica = new Utente.Anagrafica();
            if (matricola == null || matricola == CommonManager.GetCurrentUserMatricola())
                anagrafica = Utente.EsponiAnagrafica();
            else
            {
                if (!CommonManager.PdfAutorizzato(CommonManager.GetCurrentUserMatricola()))
                {
                    return View("~/Views/Shared/NonAbilitatoError2.cshtml");
                }

                it.rai.servizi.hrgb.Service wsAnag = new it.rai.servizi.hrgb.Service();
                try
                {
                    wsAnag.Credentials = new System.Net.NetworkCredential(CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]);
                    string str_temp = wsAnag.EsponiAnagrafica("RAICV;" + matricola + ";;E;0");
                    string[] temp = str_temp.ToString().Split(';');
                    if ((temp != null) && (temp.Count() > 16))
                    {
                        anagrafica = Utente.CaricaAnagrafica(temp);
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }

            }

            float indentationLeft = 168f;

            string selMatricola = !String.IsNullOrWhiteSpace(matricola) ? matricola : CommonManager.GetCurrentUserMatricola();

            cv_ModelEntities cvEnt = new cv_ModelEntities();
            var dip = cvEnt.TDipendenti.FirstOrDefault(x => x.Matricola == matricola && x.Flag_ultimo_record == "$");
            string codFigProf = "";
            string sezMatricola = "";

            if (dip != null)
            {
                codFigProf = dip.Figura_pro;
                sezMatricola = dip.Servizio.Substring(0, 2);
            }
            else
            {
                selMatricola = anagrafica._matricola;
                codFigProf = anagrafica._codiceFigProf;
                sezMatricola = anagrafica._sezContabile;
            }

            MemoryStream workStream = new MemoryStream();
            iTextSharp.text.Document document = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 24, 24, 24, 48);
            PdfWriter writer = PdfWriter.GetInstance(document, workStream);
            writer.CloseStream = false;

            List<RenderableItem> rowItemList = new List<RenderableItem>();

            string[] inquadramento = anagrafica._inquadramento.ToString().Split(';');
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

            rowItemList.Add(new KeyValue { key = "CONTRATTO", value = anagrafica._contratto });
            rowItemList.Add(new KeyValue { key = "FIGURA PROFESSIONALE", value = textInfo.ToTitleCase(anagrafica._figProfessionale.ToLower()) });
            rowItemList.Add(new KeyValue { key = "MATRICOLA", value = anagrafica._matricola });

            string indirizzo = "";

            try
            {
                string[] AccountUtenteServizio = CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio);
                myRai.it.rai.servizi.hrce.hrce_ws hrcews = new myRai.it.rai.servizi.hrce.hrce_ws();
                myRai.it.rai.servizi.hrce.retData retdata = new myRai.it.rai.servizi.hrce.retData();
                hrcews.Credentials = new System.Net.NetworkCredential(AccountUtenteServizio[0], AccountUtenteServizio[1], "RAI");
                List<string> matricole = new List<string>();
                matricole.Add(selMatricola);
                string[] AppKeyhrce = CommonManager.GetParametri<string>(EnumParametriSistema.AppKeyhrce);

                Boolean usaServ = CommonManager.GetParametro<Boolean>(EnumParametriSistema.UsaServizioPerProfiloPersonale);
                if (usaServ)
                    retdata = hrcews.getDatiUtente(AppKeyhrce[0], matricole.ToArray());
                else
                    retdata.ds = ProfiloPersonaleManager.GetProfiloPersonaleFromDB(selMatricola);


                DataTable dr_recapiti = null;
                if (retdata.ds != null)
                    dr_recapiti = retdata.ds.Tables["Table1"];

                string matricolaUtente = selMatricola;
                if (dr_recapiti != null && dr_recapiti.Rows[0]["Matricola"].ToString() != matricolaUtente)
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

                    indirizzo = textInfo.ToTitleCase(indirizzo.ToLower());

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
            rowItemList.Add(new KeyValue { key = "DATA DI NASCITA", value = String.Format("{0:dd/MM/yyyy}", anagrafica._dataNascita) });
            rowItemList.Add(new KeyValue { key = "INDIRIZZO", value = indirizzo });
            rowItemList.Add(new LineBreak());
            rowItemList.Add(new KeyValue { key = "EMAIL", value = anagrafica._email });
            rowItemList.Add(new KeyValue { key = "TELEFONO AZIENDALE", value = anagrafica._telefono });

            #region ULTIMO AGGIORNAMENTO
            var aa = cvEnt.sp_GETDTAGGCV(selMatricola);
            var rbb = aa.ToList();

            if (rbb.Count > 0 && rbb[0] != null)
            {
                string updatedAt = rbb[0].Value.ToString("dd MMMM yyyy HH:mm:ss", culture);

                rowItemList.Add(new LineBreak());

                if (!String.IsNullOrEmpty(msgUltimoAgg))
                {
                    rowItemList.Add(new SmallItalic { value = string.Format("{0} {1}", msgUltimoAgg, updatedAt) });
                }
                else
                {
                    rowItemList.Add(new SmallItalic { value = string.Format("Aggiornato al: {0}", updatedAt) });
                }
            }

            #endregion

            string image = anagrafica._foto;

            writer.PageEvent = new ITextEvents(image, rowItemList);

            document.Open();

            List<ContentBlockInfo> contentBlockInfoList = new List<ContentBlockInfo>();

            List<RenderableItem> blockItemInfoList = new List<RenderableItem>();

            #region AUTOPRESENTAZIONE
            blockItemInfoList = new List<RenderableItem>();
            var autopresList = cvEnt.TCVAllegato.Where(x => x.Matricola == selMatricola && x.Stato == "#").ToList();
            if (autopresList != null && autopresList.Count > 0)
            {
                var autopresentazione = autopresList[0];
                if (!String.IsNullOrWhiteSpace(autopresentazione.Path_name) || !String.IsNullOrWhiteSpace(autopresentazione.Note))
                {
                    KeyValueJustified block = new KeyValueJustified();
                    block.key = "";

                    if (!String.IsNullOrWhiteSpace(autopresentazione.Path_name))
                    {
                        string baseUrl = System.Web.HttpContext.Current.Request.Url.Scheme + "://" + System.Web.HttpContext.Current.Request.Url.Authority;

                        if (autopresentazione.ContentType == "website")
                        {
                            string link = autopresentazione.Path_name;
                            block.chk = new Chunk("Clicca qui per vedere l'autopresentazione", new FontManager("", BaseColor.BLACK).Italic).SetAction(new PdfAction(link, false));
                        }
                        else
                        {
                            string link = baseUrl + "/cv_online/GetFile/" + autopresentazione.Id;
                            block.chk = new Chunk("Clicca qui per vedere l'autopresentazione", new FontManager("", BaseColor.BLACK).Italic).SetAction(new PdfAction(link, false));
                        }
                    }

                    block.value = autopresentazione.Note;

                    blockItemInfoList.Add(block);
                }

                if (blockItemInfoList.Count > 0)
                {
                    contentBlockInfoList.Add(new ContentBlockInfo("MI PRESENTO", blockItemInfoList));
                }
            }
            blockItemInfoList = new List<RenderableItem>();
            #endregion

            #region ESPERIENZE
            var esperienze = cvEnt.TCVEsperExRai.Where(x => x.Matricola == selMatricola && x.DataFine == null).OrderByDescending(z => z.DataFine).ToList();

            foreach (TCVEsperExRai esp in esperienze)
            {
                PdfExperienceTest(cvEnt, codFigProf, blockItemInfoList, esp);
            }

            esperienze = cvEnt.TCVEsperExRai.Where(x => x.Matricola == selMatricola && x.DataFine != null).OrderByDescending(z => z.DataFine).ToList();
            foreach (TCVEsperExRai esp in esperienze)
            {
                PdfExperienceTest(cvEnt, codFigProf, blockItemInfoList, esp);
            }

            if (blockItemInfoList.Count > 0)
            {
                contentBlockInfoList.Add(new ContentBlockInfo("ESPERIENZE", blockItemInfoList));
            }
            #endregion


            if ((codFigProf != "XAA" && codFigProf != "XDA" || CommonManager.AbilitatoEditoriali(sezMatricola))
                || cvEnt.TSVEsperProd.Any(f => f.STATO == "C" && f.COD_ANAGRAFIA != "IR" && f.MATRICOLA == selMatricola)
                || cvEnt.TCVConProf.Any(x => x.Matricola == selMatricola))
            {
                #region IMPEGNI EDITORIALI RAI

                blockItemInfoList = new List<RenderableItem>();

                cvBox.impegniEditoriali = new List<cvModel.ImpegniRAI>();

                int prog = 0;
                List<Data.CurriculumVitae.TSVEsperProd> esperProd = cvEnt.TSVEsperProd.Where(f => f.STATO == "C" && f.COD_ANAGRAFIA != "IR" && f.MATRICOLA == selMatricola).OrderByDescending(p => p.FINE_PERIODO_ESP).ToList();
                foreach (var elem in esperProd)
                {
                    string tmp_ruolo = "";
                    cvModel.ImpegniRAI impegniRai = new cvModel.ImpegniRAI();
                    prog++;
                    impegniRai._desTitoloDefinit = elem.DES_TITOLO_DEFINIT;
                    impegniRai._idEsperienze = Convert.ToInt32(elem.ID_ESPERIENZE);
                    impegniRai._matricola = selMatricola;
                    impegniRai._matricolaSpett = elem.COD_MATRICOLA;
                    impegniRai._progDaStampare = prog;
                    var conProf = cvEnt.DConProf.Where(x => x.CodConProf == elem.COD_RUOLO).FirstOrDefault();
                    if (conProf != null)
                        tmp_ruolo = conProf.DescConProf;

                    //gestione date
                    impegniRai._ruolo = tmp_ruolo;
                    impegniRai._dtDataInizio = elem.INIZIO_PERIODO_ESP.Substring(6, 2) + "/" + elem.INIZIO_PERIODO_ESP.Substring(4, 2) + "/" + elem.INIZIO_PERIODO_ESP.Substring(0, 4);
                    impegniRai._dtDataFine = elem.FINE_PERIODO_ESP.Substring(6, 2) + "/" + elem.FINE_PERIODO_ESP.Substring(4, 2) + "/" + elem.FINE_PERIODO_ESP.Substring(0, 4);


                    if (!string.IsNullOrEmpty(impegniRai._ruolo) && !string.IsNullOrEmpty(impegniRai._desTitoloDefinit))
                    {
                        int fyear = Int32.Parse(impegniRai._dtDataInizio.Substring(6, 4));
                        int fmonth = Int32.Parse(impegniRai._dtDataInizio.Substring(3, 2));
                        int fday = Int32.Parse(impegniRai._dtDataInizio.Substring(0, 2));

                        DateTime fdate = new DateTime(fyear, fmonth, fday);

                        int tyear = Int32.Parse(impegniRai._dtDataFine.Substring(6, 4));
                        int tmonth = Int32.Parse(impegniRai._dtDataFine.Substring(3, 2));
                        int tday = Int32.Parse(impegniRai._dtDataFine.Substring(0, 2));

                        DateTime tdate = new DateTime(tyear, tmonth, tday);

                        string fromMonth = fdate.ToString("MMMM", culture);

                        string toMonth = tdate.ToString("MMMM", culture);

                        string value = string.Format("da {0} {1} a {2} {3}", fromMonth, fyear, toMonth, tyear);

                        if (toMonth.Equals(DateTime.Now.ToString("MMMM", culture)))
                        {
                            value = string.Format("attuale da {0} {1}", fromMonth, fyear);
                        }

                        string key = string.Format("{0} | {1}", impegniRai._ruolo, impegniRai._desTitoloDefinit);

                        blockItemInfoList.Add(new KeyValueInlineBold
                        {
                            key = impegniRai._ruolo,
                            separator = "|",
                            value = impegniRai._desTitoloDefinit,
                            subvalue = value
                        });
                    }
                }
                if (blockItemInfoList.Count > 0)
                {
                    contentBlockInfoList.Add(new ContentBlockInfo("IMPEGNI EDITORIALI RAI", blockItemInfoList));
                }

                #endregion

                #region ATTIVITà E COMPETENZE EDITORIALI

                blockItemInfoList = new List<RenderableItem>();

                cvBox.competenzeRai = new List<cvModel.CompetenzeRAI>();

                //prelevo la figura professionale
                string figura_profesisonale = anagrafica._codiceFigProf;

                List<TCVConProf> tcvConProf = cvEnt.TCVConProf.Where(x => x.Matricola == selMatricola).ToList();
                List<Data.CurriculumVitae.DConProf> dConProf =
                    //cvEnt.DConProf.Where(x => x.FiguraProfessionale == figura_profesisonale && x.Stato=="0").ToList();
                    cvEnt.DConProf.Where(x => x.FiguraProfessionale == "XAA" || x.FiguraProfessionale == "XDA" && x.Stato == "0").ToList();

                foreach (var elem in dConProf)
                {
                    cvModel.CompetenzeRAI frk_compRai = new cvModel.CompetenzeRAI();

                    frk_compRai._codConProf = elem.CodConProf;
                    frk_compRai._descConProf = elem.DescConProf;
                    frk_compRai._figuraProfessionale = elem.FiguraProfessionale;
                    frk_compRai._matricola = selMatricola;

                    var tmp = tcvConProf.Where(x => x.CodConProf == elem.CodConProf).ToList();
                    if (tmp.Count() > 0)
                    {
                        frk_compRai._dataOraAgg = tmp.First().DataOraAgg;
                        frk_compRai._flagExtraRai = tmp.First().FlagExtraRai;
                        frk_compRai._flagPrincipale = tmp.First().FlagPrincipale;
                        frk_compRai._flagSecondario = tmp.First().FlagSecondario;
                        frk_compRai._stato = tmp.First().Stato;
                        frk_compRai._tipoAgg = tmp.First().TipoAgg;

                        if (frk_compRai._flagPrincipale == "1")
                        {
                            frk_compRai._flagChoice = "P";
                        }
                        else if (frk_compRai._flagSecondario == "1")
                        {
                            frk_compRai._flagChoice = "S";
                        }
                        else
                        {
                            frk_compRai._flagChoice = " ";
                        }
                    }
                    else
                    {
                        frk_compRai._dataOraAgg = null;
                        frk_compRai._flagChoice = "";
                        frk_compRai._flagExtraRai = "";
                        frk_compRai._flagPrincipale = "";
                        frk_compRai._flagSecondario = "";
                        frk_compRai._stato = "";
                        frk_compRai._tipoAgg = "";
                    }

                    if (!string.IsNullOrEmpty(frk_compRai._descConProf) && frk_compRai._flagPrincipale.Length > 0)
                    {
                        string raiExtraRai = !String.IsNullOrWhiteSpace(frk_compRai._flagExtraRai) && !frk_compRai._flagExtraRai.Equals("0", StringComparison.InvariantCultureIgnoreCase) ? "Extra Rai" : "Rai";
                        string value = frk_compRai._flagChoice == "P" ? "Principale" : frk_compRai._flagChoice == "S" ? "Secondaria" : "";

                        string key = string.Format("{0} | {1}", frk_compRai._descConProf, raiExtraRai);

                        blockItemInfoList.Add(new KeyValueInlineBold
                        {
                            key = frk_compRai._descConProf,
                            separator = "|",
                            value = raiExtraRai,
                            subvalue = value
                        });
                    }
                }

                if (blockItemInfoList.Count > 0)
                {
                    contentBlockInfoList.Add(new ContentBlockInfo(("attività e competenze editoriali").ToUpper(), blockItemInfoList));
                }

                #endregion
            }

            #region TITOLI DI STUDIO E SPECIALIZZAZIONI

            blockItemInfoList = new List<RenderableItem>();
            cvBox.curricula = new List<cvModel.Studies>();

            var istruzione = (cvEnt.TCVIstruzione.Where(m => m.Matricola == selMatricola).OrderByDescending(a => a.AnnoFine)).ToList();
            var specializz = (cvEnt.TCVSpecializz.Where(m => m.Matricola == selMatricola).OrderByDescending(a => a.DataFine)).ToList();

            #region Istruzione
            foreach (TCVIstruzione istr in istruzione)
            {
                cvModel.Studies frk = new cvModel.Studies( CommonManager.GetCurrentUserMatricola( ) );
                //freak - riempire i campi
                //using (var ctx = new cv_ModelEntities())
                {
                    var param = new SqlParameter("@param", istr.CodTitolo);
                    //var param_naz = new SqlParameter("@param_naz", istr.CodNazione);
                    //freak - devo crearmi una classe con i tre campi "descTipoTitolo descTitolo Logo
                    //var param_naz = new SqlParameter("@param_naz", "AND");
                    List<Utente.CV_DescTitoloLogo> tmp = cvEnt.Database.SqlQuery<Utente.CV_DescTitoloLogo>("exec sp_GETDESCTITOLO @param", param).ToList();

                    frk._descTipoTitolo = tmp[0].DescTipoTitolo; //tmp.GetValue(0,0).ToString();
                    frk._descTitolo = tmp[0].DescTitolo;//tmp.GetValue(0, 1).ToString();
                    frk._logo = tmp[0].Logo;//tmp.GetValue(0, 2).ToString();
                    //recupero descNazione tramite codNazione
                    // freak - imposto un valore a frk._codNazione
                    frk._codNazione = istr.CodNazione;

                    if (!String.IsNullOrWhiteSpace(frk._codNazione))
                    {
                        try
                        {
                            var sql2 = cvEnt.Database.SqlQuery<string>("SELECT DES_NAZIONE FROM DNAZIONE WHERE COD_SIGLANAZIONE = '" + frk._codNazione + "'").ToList();
                            frk._descNazione = sql2[0];
                        }
                        catch
                        {
                            frk._descNazione = "";
                        }
                    }
                }
                frk._codTitolo = istr.CodTitolo;
                frk._codTipoTitolo = istr.CodTipoTitolo;
                frk._corsoLaurea = istr.CorsoLaurea;
                frk._dataFine = istr.AnnoFine;
                frk._dataInizio = istr.AnnoInizio;
                frk._dataoraAgg = (DateTime)istr.DataOraAgg;
                frk._durata = istr.Durata;
                frk._indirizzoStudi = (istr.IndirizzoStudi != null) ? istr.IndirizzoStudi : "";
                frk._istituto = (istr.Istituto != null) ? istr.Istituto : "";
                frk._localitaStudi = (istr.LocalitaStudi != null) ? istr.LocalitaStudi : "";
                frk._lode = ((istr.Lode == null) || (istr.Lode == " ")) ? ' ' : Convert.ToChar(istr.Lode);
                frk._matricola = istr.Matricola;
                frk._prog = -1;
                frk._scala = istr.Scala;
                frk._stato = Convert.ToChar(istr.Stato);
                frk._tipoAgg = Convert.ToChar(istr.TipoAgg);
                frk._titoloSpecializ = null;
                frk._titoloTesi = istr.TitoloTesi;
                frk._voto = istr.Voto;
                frk._riconoscimento = "";
                frk._tableTarget = "I";
                if (!string.IsNullOrEmpty(frk._descTitolo))
                {
                    string voto = !string.IsNullOrEmpty(frk._voto) ? string.Format(" {0}", frk._voto) : "";
                    string lode = frk._lode == 'S' ? " con Lode" : "";
                    string istituto = !string.IsNullOrEmpty(frk._istituto) ? string.Format("{0}", frk._istituto) : "";
                    string localitaStudi = !string.IsNullOrEmpty(frk._localitaStudi) ? string.Format("{0}", frk._localitaStudi) : "";
                    string tiplogia = !string.IsNullOrEmpty(frk._logo) ? string.Format("{0}", frk._logo) : "";

                    string key = "";
                    string value = "";

                    key += tiplogia + " | " + frk._descTitolo;

                    KeyValueList keyValueList = new KeyValueList();

                    if (tiplogia == "Diploma")
                    {
                        keyValueList.values.Add(new KeyValueInline() { key = "Tipo di Diploma", value = frk._descTipoTitolo });
                        keyValueList.values.Add(new KeyValueInline() { key = "Istituto", value = istituto + (localitaStudi != "" ? " - " + localitaStudi : "") });
                    }
                    else
                    {

                        keyValueList.values.Add(new KeyValueInline() { key = "Tipo di Laurea", value = frk._descTipoTitolo });
                        keyValueList.values.Add(new KeyValueInline() { key = "Classe di Laurea", value = frk._descTitolo });
                        keyValueList.values.Add(new KeyValueInline() { key = "Corso di Laurea", value = frk._corsoLaurea });
                        keyValueList.values.Add(new KeyValueInline() { key = "Università/Altro Ente", value = istituto + (localitaStudi != "" ? " - " + localitaStudi : "") });
                    }

                    keyValueList.values.Add(new KeyValueInline() { key = "Anno conseguimento", value = frk._dataFine });

                    if ((!string.IsNullOrEmpty(voto)) && (!string.IsNullOrEmpty(frk._scala)))
                    {
                        keyValueList.values.Add(new KeyValueInline() { key = "Voto", value = String.Format("{0} su {2}{1}", voto, lode, frk._scala) });
                    }
                    else if ((!string.IsNullOrEmpty(voto)) && (string.IsNullOrEmpty(frk._scala)))
                    {
                        keyValueList.values.Add(new KeyValueInline() { key = "Voto", value = String.Format("{0}{1}", voto, lode) });
                    }

                    if (!String.IsNullOrWhiteSpace(frk._titoloTesi))
                        keyValueList.values.Add(new KeyValueInline() { key = "Titolo tesi", value = frk._titoloTesi });

                    blockItemInfoList.Add(keyValueList);
                }
            }
            #endregion

            #region Specializzazione
            foreach (TCVSpecializz spec in specializz)
            {
                cvModel.Studies frk = new cvModel.Studies( CommonManager.GetCurrentUserMatricola( ) );
                //freak - riempire i campi
                using (var ctx = new cv_ModelEntities())
                {
                    var param = new SqlParameter("@param", spec.TipoSpecial);
                    //var param_naz = new SqlParameter("@param_naz", );
                    //freak - devo crearmi una classe con i tre campi "descTipoTitolo descTitolo Logo
                    //var param_naz = new SqlParameter("@param_naz", "AND");
                    List<Utente.CV_DescTitoloLogo> tmp = ctx.Database.SqlQuery<Utente.CV_DescTitoloLogo>("exec sp_GETDESCTITOLO @param", param).ToList();
                    frk._descTipoTitolo = tmp[0].DescTipoTitolo; //tmp.GetValue(0,0).ToString();
                    frk._descTitolo = tmp[0].DescTitolo;//tmp.GetValue(0, 1).ToString();
                    frk._logo = tmp[0].Logo;//tmp.GetValue(0, 2).ToString();
                    if (spec.TipoSpecial == "999")
                    {
                        frk._descTipoTitolo = "Specializzazione";
                        frk._logo = "Master";
                    }
                    //recupero descNazione tramite codNazione
                    // freak - imposto un valore a frk._codNazione
                    frk._codNazione = spec.CodNazione;
                    if (!String.IsNullOrWhiteSpace(frk._codNazione))
                    {
                        try
                        {
                            var sql2 = ctx.Database.SqlQuery<string>("SELECT DES_NAZIONE FROM DNAZIONE WHERE COD_SIGLANAZIONE = '" + frk._codNazione + "'").ToList();
                            frk._descNazione = sql2[0];
                        }
                        catch
                        {
                            frk._descNazione = "";
                        }
                    }
                }
                frk._codTitolo = spec.TipoSpecial;
                frk._corsoLaurea = "";// spec.Titolo; //freak - da controllare con Massimo Tesoro
                //frk._dataFine = spec.DataFine.Substring(6, 2) + "/" + spec.DataFine.Substring(4, 2) + "/" + spec.DataFine.Substring(0, 4);
                //frk._dataInizio = (spec.DataInizio.Length == 8) ? spec.DataInizio.Substring(6, 2) + "/" + spec.DataInizio.Substring(4, 2) + "/" + spec.DataInizio.Substring(0, 4) : "";
                frk._dataFine = !String.IsNullOrWhiteSpace(spec.DataFine) ? spec.DataFine.Substring(0, 4) : "";
                frk._dataInizio = !String.IsNullOrWhiteSpace(spec.DataInizio) ? spec.DataInizio.Substring(0, 4) : "";
                frk._dataoraAgg = (DateTime)spec.DataOraAgg;
                frk._durata = spec.Durata;
                frk._indirizzoStudi = (spec.IndirizzoSpecial != null) ? spec.IndirizzoSpecial : "";
                frk._istituto = (spec.Istituto != null) ? spec.Istituto : "";
                frk._localitaStudi = (spec.LocalitaSpecial != null) ? spec.LocalitaSpecial : "";
                frk._lode = ((spec.Lode == null) || (spec.Lode == " ")) ? ' ' : Convert.ToChar(spec.Lode);
                frk._matricola = spec.Matricola;
                frk._prog = spec.Prog;
                frk._scala = spec.Scala;
                frk._stato = Convert.ToChar(spec.Stato);
                frk._tipoAgg = Convert.ToChar(spec.TipoAgg);
                frk._titoloSpecializ = spec.Titolo;
                frk._titoloTesi = ""; //freak - da controllare con Massimo Tesoro
                frk._voto = spec.Voto;
                //frk._riconoscimento = (spec._riconoscimento != null) ? spec._riconoscimento : "";
                frk._tableTarget = "S";

                if (!string.IsNullOrEmpty(frk._descTitolo))
                {
                    string voto = !string.IsNullOrEmpty(frk._voto) ? string.Format(" {0}", frk._voto) : "";
                    string lode = frk._lode == 'S' ? " con Lode" : "";
                    string istituto = !string.IsNullOrEmpty(frk._istituto) ? string.Format("{0}", frk._istituto) : "";
                    string localitaStudi = !string.IsNullOrEmpty(frk._localitaStudi) ? string.Format("{0}", frk._localitaStudi) : "";
                    string tipologia = !string.IsNullOrEmpty(frk._logo) ? string.Format("{0}", frk._logo) : "";
                    string titoloSpec = !string.IsNullOrWhiteSpace(frk._titoloSpecializ) ? string.Format("{0}", frk._titoloSpecializ) : "";

                    string key = "";

                    key = frk._descTitolo;
                    if (tipologia != "") key += " | " + tipologia;
                    if (titoloSpec != "") key += " | " + titoloSpec;

                    KeyValueList keyValueList = new KeyValueList();
                    keyValueList.values.Add(new KeyValueInline() { key = "Tipo di Specializzazione", value = frk._descTitolo });
                    if (!String.IsNullOrWhiteSpace(titoloSpec))
                        keyValueList.values.Add(new KeyValueInline() { key = "Ambito/Titolo", value = titoloSpec });
                    keyValueList.values.Add(new KeyValueInline() { key = "Università/Altro Ente", value = istituto + (localitaStudi != "" ? " - " + localitaStudi : "") });
                    if ((!string.IsNullOrEmpty(voto)) && (!string.IsNullOrEmpty(frk._scala)))
                    {
                        keyValueList.values.Add(new KeyValueInline() { key = "Voto", value = String.Format("{0} su {2}{1}", voto, lode, frk._scala) });
                    }
                    else if ((!string.IsNullOrEmpty(voto)) && (string.IsNullOrEmpty(frk._scala)))
                    {
                        keyValueList.values.Add(new KeyValueInline() { key = "Voto", value = String.Format("{0}{1}", voto, lode) });
                    }

                    keyValueList.values.Add(new KeyValueInline() { key = "Anno conseguimento", value = frk._dataFine });

                    if (!String.IsNullOrWhiteSpace(frk._titoloTesi))
                        keyValueList.values.Add(new KeyValueInline() { key = "Titolo tesi", value = frk._titoloTesi });

                    blockItemInfoList.Add(keyValueList);
                }
            }
            #endregion

            if (blockItemInfoList.Count > 0)
            {
                contentBlockInfoList.Add(new ContentBlockInfo(("TITOLI DI STUDIO E SPECIALIZZAZIONI").ToUpper(), blockItemInfoList));
            }

            #endregion

            #region CERTIFICAZIONI E PREMI

            blockItemInfoList = new List<RenderableItem>();
            var certificazioni = cvEnt.TCVCertifica.Where(c => c.Matricola == selMatricola);
            var descalb = cvEnt.DAlboProf.ToList();
            foreach (var certif in certificazioni)
            {
                string dataini = null; ;
                string datafin = null;
                string datapub = null;
                string databrev = null;
                string dataalbo = null;
                if (certif.MeseIni != null)
                {
                    var mese = new DateTime(Convert.ToInt16(certif.AnnoIni), Convert.ToInt16(certif.MeseIni), 01).ToString("MMMM");
                    dataini = mese.Substring(0, 1).ToUpper() + mese.Substring(1) + " " + certif.AnnoIni;
                }
                if (certif.MeseFin != null)
                {
                    var mese = new DateTime(Convert.ToInt16(certif.AnnoFin), Convert.ToInt16(certif.MeseFin), 01).ToString("MMMM");
                    datafin = mese.Substring(0, 1).ToUpper() + mese.Substring(1) + " " + certif.AnnoFin;
                }
                if (certif.DataPubblica != null)
                {
                    datapub = new DateTime(Convert.ToInt16(certif.DataPubblica.Substring(0, 4)), Convert.ToInt16(certif.DataPubblica.Substring(4, 2)), Convert.ToInt16(certif.DataPubblica.Substring(6, 2))).ToString("dd/MM/yyyy");
                }
                if (certif.DataBrevetto != null)
                {
                    databrev = new DateTime(Convert.ToInt16(certif.DataBrevetto.Substring(0, 4)), Convert.ToInt16(certif.DataBrevetto.Substring(4, 2)), Convert.ToInt16(certif.DataBrevetto.Substring(6, 2))).ToString("dd/MM/yyyy");
                }
                if (certif.DataAlboProf != null)
                {
                    dataalbo = new DateTime(Convert.ToInt16(certif.DataAlboProf.Substring(0, 4)), Convert.ToInt16(certif.DataAlboProf.Substring(4, 2)), Convert.ToInt16(certif.DataAlboProf.Substring(6, 2))).ToString("dd/MM/yyyy");
                }
                cvModel.Certificazioni cer = new cvModel.Certificazioni( CommonManager.GetCurrentUserMatricola( ) )
                {
                    _annoFin = certif.AnnoFin,
                    _annoIni = certif.AnnoIni,
                    _autCertifica = certif.AutCertifica,
                    _codAlboProf = certif.CodAlboProf,
                    _dataAlboProf = dataalbo,
                    _dataBrevetto = databrev,
                    _dataOraAgg = certif.DataOraAgg,
                    _dataPubblica = datapub,
                    _descAlboProf = descalb.Where(x => x.CodAlboProf == certif.CodAlboProf).Select(x => x.DescAlboProf).FirstOrDefault(),
                    _editorePubblica = certif.EditorePubblica,
                    _flagRegBrevetto = certif.FlagRegBrevetto,
                    _inventore = certif.Inventore,
                    _matricola = certif.Matricola,
                    _meseFin = certif.MeseFin,
                    _meseIni = certif.MeseIni,
                    _dataIni = dataini,
                    _dataFin = datafin,
                    _nomeCertifica = certif.NomeCertifica,
                    _noteAlboProf = certif.NoteAlboProf,
                    _noteBrevetto = certif.NoteBrevetto,
                    _notePubblica = certif.NotePubblica,
                    _numBrevetto = certif.NumBrevetto,
                    _numLicenza = certif.NumLicenza,
                    _pressoAlboProf = certif.PressoAlboProf,
                    _prog = certif.Prog,
                    _tipo = certif.Tipo,
                    _tipoAgg = certif.TipoAgg,
                    _tipoBrevetto = certif.TipoBrevetto,
                    _titoloPubblica = certif.TitoloPubblica,
                    _uffBrevetto = certif.UffBrevetto,
                    _urlBrevetto = certif.UrlBrevetto,
                    _urlCertifica = certif.UrlCertifica,
                    _urlPubblica = certif.UrlPubblica,
                    _tipoPubblicazione = certif.TipoPubblicazione,
                    _tipoPubRiferimento = certif.TipoPubRiferimento,
                    _riferimentoPub = certif.RiferimentoPub,
                    AnnoPubblicazione = certif.AnnoPubblicazione
                    //MesePubblicazione = certif.MesePubblicazione,
                    //GiornoPubblicazione = certif.GiornoPubblicazione
                };

                string etichettaTitolo = "";
                string tipoCerticifazioneLabel = "";
                string annoCertLabel = "";
                string nomeCert = "";
                string dataLabel = "";
                string linkLabel = "";
                string autCertifica = "Autorità attestato";

                string key = "";

                switch (cer._tipo)
                {
                    case "1":
                        tipoCerticifazioneLabel = "Attestato";
                        etichettaTitolo = "Nome Attestato";
                        annoCertLabel = "Anno conseguimento";
                        nomeCert = "Attestato";
                        key = cer._nomeCertifica;
                        break;
                    case "2":
                        tipoCerticifazioneLabel = "Pubblicazione";
                        etichettaTitolo = "Titolo";
                        annoCertLabel = "Anno pubblicazione";
                        key = cer._titoloPubblica;
                        break;
                    case "3":
                        tipoCerticifazioneLabel = "Brevetto";
                        etichettaTitolo = "Titolo Brevetto";
                        annoCertLabel = "Anno conseguimento";
                        dataLabel = "Data concessione";
                        linkLabel = "Url Brevetto";
                        key = cer._tipoBrevetto;
                        break;
                    case "4":
                        tipoCerticifazioneLabel = "Iscrizione";
                        annoCertLabel = "Data iscrizione";
                        key = cer._descAlboProf;
                        break;
                    case "5":
                        tipoCerticifazioneLabel = "Premio";
                        etichettaTitolo = "Nome premio";
                        annoCertLabel = "Anno conseguimento";
                        nomeCert = "Premio";
                        dataLabel = "Data conseguimento";
                        linkLabel = "Link approfondimenti";
                        autCertifica = "Ente erogatore";
                        key = cer._nomeCertifica;
                        break;
                }

                if (!string.IsNullOrEmpty(cer._annoFin))
                {
                    annoCertLabel = string.Format("{0} {1}", annoCertLabel, cer._annoFin);
                }
                else
                {
                    annoCertLabel = "";
                }

                KeyValueList kvl = new KeyValueList();
                //key = tipoCerticifazioneLabel + ": " + key;
                //kvl.key = key;

                kvl.values.Add(new KeyValueInline() { key = "Tipologia", value = tipoCerticifazioneLabel });

                if (cer._tipo != "4")
                    kvl.values.Add(new KeyValueInline() { key = etichettaTitolo, value = key });

                if (!string.IsNullOrEmpty(cer._annoFin))
                    kvl.values.Add(new KeyValueInline() { key = annoCertLabel, value = annoCertLabel });

                if (!string.IsNullOrEmpty(cer._numLicenza))
                    kvl.values.Add(new KeyValueInline() { key = "Numero Licenza", value = cer._numLicenza });
                if (!string.IsNullOrEmpty(cer._autCertifica))
                    kvl.values.Add(new KeyValueInline() { key = autCertifica, value = cer._autCertifica });
                if (!string.IsNullOrEmpty(cer._urlCertifica))
                    kvl.values.Add(new KeyValueInline() { key = "Url attestato", value = cer._urlCertifica });
                if (!string.IsNullOrEmpty(cer._noteCertifica))
                    kvl.values.Add(new KeyValueInline() { key = "Descrizione", value = cer._noteCertifica });
                if (!string.IsNullOrEmpty(cer._numBrevetto))
                    kvl.values.Add(new KeyValueInline() { key = "Numero Brevetto", value = cer._numBrevetto });
                if (!string.IsNullOrEmpty(cer._uffBrevetto))
                    kvl.values.Add(new KeyValueInline() { key = "Ufficio brevetti", value = cer._uffBrevetto });
                if (!string.IsNullOrEmpty(cer._inventore))
                    kvl.values.Add(new KeyValueInline() { key = "Inventore/co-inventori", value = cer._inventore });
                if (!string.IsNullOrEmpty(cer._dataBrevetto))
                    kvl.values.Add(new KeyValueInline() { key = dataLabel, value = cer._dataBrevetto });
                if (!string.IsNullOrEmpty(cer._urlBrevetto))
                    kvl.values.Add(new KeyValueInline() { key = linkLabel, value = cer._urlBrevetto });
                if (!string.IsNullOrEmpty(cer._noteBrevetto))
                    kvl.values.Add(new KeyValueInline() { key = "Descrizione", value = cer._noteBrevetto });
                if (!string.IsNullOrEmpty(cer._editorePubblica))
                    kvl.values.Add(new KeyValueInline() { key = "Editore", value = cer._editorePubblica });
                if (!string.IsNullOrEmpty(cer._urlPubblica))
                    kvl.values.Add(new KeyValueInline() { key = "Url", value = cer._urlPubblica });
                if (!string.IsNullOrEmpty(cer._notePubblica))
                    kvl.values.Add(new KeyValueInline() { key = "Descrizione", value = cer._notePubblica });
                if (cer.GiornoPubblicazione != null)
                    kvl.values.Add(new KeyValueInline() { key = "Giorno Pubblicazione", value = cer.GiornoPubblicazione.ToString() });
                if (!string.IsNullOrEmpty(cer.MesePubblicazione))
                    kvl.values.Add(new KeyValueInline() { key = "Mese Pubblicazione", value = cer.MesePubblicazione });
                if (cer.AnnoPubblicazione != null)
                    kvl.values.Add(new KeyValueInline() { key = "Anno Pubblicazione", value = cer.AnnoPubblicazione.ToString() });
                if (!string.IsNullOrEmpty(cer._descAlboProf))
                    kvl.values.Add(new KeyValueInline() { key = "Albo Professionale", value = cer._descAlboProf });
                if (!string.IsNullOrEmpty(cer._descAlboProf))
                    kvl.values.Add(new KeyValueInline() { key = "Data iscrizione", value = cer._dataAlboProf });
                if (!string.IsNullOrEmpty(cer._pressoAlboProf))
                    kvl.values.Add(new KeyValueInline() { key = "Presso", value = cer._pressoAlboProf });
                if (!string.IsNullOrEmpty(cer._noteAlboProf))
                    kvl.values.Add(new KeyValueInline() { key = "Descrizione", value = cer._noteAlboProf });
                if (!string.IsNullOrEmpty(cer._tipoPubblicazione))
                    kvl.values.Add(new KeyValueInline() { key = "Tipo pubblicazione", value = cer._tipoPubblicazione });
                if (!string.IsNullOrEmpty(cer._tipoPubRiferimento))
                    kvl.values.Add(new KeyValueInline() { key = "Tipo riferimento", value = cer._tipoPubRiferimento });
                if (!string.IsNullOrEmpty(cer._riferimentoPub))
                    kvl.values.Add(new KeyValueInline() { key = "Riferimento", value = cer._riferimentoPub });

                blockItemInfoList.Add(kvl);
            }

            if (blockItemInfoList.Count > 0)
            {
                contentBlockInfoList.Add(new ContentBlockInfo("CERTIFICAZIONI, PUBBLICAZIONI E PREMI", blockItemInfoList));
            }

            #endregion

            #region COMPETENZE INFORMATICHE

            blockItemInfoList = new List<RenderableItem>();

            cvBox.conoscenzeInformatiche = new List<cvModel.ConoscenzeInformatiche>();

            List<TCVConInfo> TCVConInfo = new List<TCVConInfo>();

            TCVConInfo = cvEnt.TCVConInfo.Where(x => x.Matricola == selMatricola).ToList();
            var allList = from conInfo in cvEnt.DConInfo
                          join gruppoConInfo in cvEnt.DGruppoConInfo on conInfo.CodGruppoConInfo equals gruppoConInfo.CodGruppoConInfo
                          select new
                          {
                              CodConInfo = conInfo.CodConInfo,
                              DescInfo = conInfo.DescConInfo,
                              CodGruppoConInfo = conInfo.CodGruppoConInfo,
                              CodPosizione = conInfo.CodPosizione,
                              DescGruppoConInfo = gruppoConInfo.DescGruppoConInfo,
                          };


            foreach (var grp in allList.GroupBy(x => x.CodGruppoConInfo))
            {
                KeyValueList kvl = new KeyValueList();
                kvl.key = grp.First().DescGruppoConInfo;

                foreach (var elem in grp.OrderBy(x => x.DescInfo))
                {
                    cvModel.ConoscenzeInformatiche know = new cvModel.ConoscenzeInformatiche();
                    know._codConInfo = elem.CodConInfo;
                    know._codGruppoConInfo = elem.CodGruppoConInfo;
                    know._codPosizione = elem.CodPosizione;
                    know._descConInfo = elem.DescInfo;
                    know._descGruppoConInfo = elem.DescGruppoConInfo;
                    var tmp = TCVConInfo.Where(x => x.CodConInfo == elem.CodConInfo);
                    if (elem.CodGruppoConInfo != "99")
                    {
                        if (tmp.Count() > 0)
                        {
                            know._selectedConInfo = true;
                            know._altraConInfo = tmp.First().AltraConInfo;
                            know._codConInfoLiv = tmp.First().CodConInfoLiv;
                            know._note = tmp.First().Note;
                            know._matricola = tmp.First().Matricola;
                            know._prog = tmp.First().Prog;
                        }
                        else
                        {
                            know._selectedConInfo = false;
                            know._altraConInfo = "";
                            know._codConInfoLiv = "";
                            know._note = "";
                            know._matricola = "";
                            know._prog = 0;
                        }
                    }
                    //string value = string.Format("{0} | {1}", know._codGruppoConInfo, know._descConInfo);
                    string value = know._descConInfo;

                    int level;
                    Int32.TryParse(know._codConInfoLiv, out level);

                    if (level > 0)
                    {
                        //blockItemInfoList.Add(new KeyLevel
                        //{
                        //    key = value,
                        //    level = level,
                        //    range = 3
                        //});
                        kvl.values.Add(new KeyLevel
                        {
                            key = value,
                            level = level,
                            range = 3
                        });
                    }
                }

                if (kvl.values.Count > 0)
                    blockItemInfoList.Add(kvl);
            }

            KeyValueList kvlAltro = new KeyValueList();
            kvlAltro.key = "Altro";
            var tmp_lista9999 = TCVConInfo.Where(x => x.CodConInfo == "9999").ToList();
            foreach (var elem_2 in tmp_lista9999.OrderByDescending(a => a.CodConInfoLiv))
            {
                cvModel.ConoscenzeInformatiche know = new cvModel.ConoscenzeInformatiche();

                know._altraConInfo = elem_2.AltraConInfo;
                know._codConInfo = elem_2.CodConInfo;
                know._codConInfoLiv = elem_2.CodConInfoLiv;
                know._codGruppoConInfo = "99";
                know._dataOraAgg = elem_2.DataOraAgg;
                know._matricola = elem_2.Matricola;
                know._note = elem_2.Note;
                know._prog = elem_2.Prog;
                know._selectedConInfo = true;
                know._stato = elem_2.Stato;
                know._tipoAgg = elem_2.TipoAgg;

                string value = string.Format("{1}",
                    know._codGruppoConInfo, know._altraConInfo);

                int level;
                Int32.TryParse(know._codConInfoLiv, out level);

                if (level > 0)
                {
                    kvlAltro.values.Add(new KeyLevel
                    {
                        key = value,
                        level = level,
                        range = 3
                    });
                }
            }
            if (kvlAltro.values.Count() > 0)
            {
                blockItemInfoList.Add(kvlAltro);
            }

            if (blockItemInfoList.Count > 0)
            {
                contentBlockInfoList.Add(new ContentBlockInfo("COMPETENZE INFORMATICHE (autovalutazione)", blockItemInfoList));
            }

            #endregion

            #region COMPETENZE DIGITALI

            blockItemInfoList = new List<RenderableItem>();

            List<TCVCompDigit> competenze = cvEnt.TCVCompDigit.Where(m => m.Matricola == selMatricola).ToList();

            if (competenze.Count > 0)
            {
                foreach (var elem in competenze.OrderByDescending(a => a.CodCompDigitLiv))
                {
                    cvModel.CompetenzeDigitali frk_comepetenze = new cvModel.CompetenzeDigitali();
                    string descCompDigit, descCompDigitLiv, descCompDigitLivLunga;

                    frk_comepetenze._codCompDigit = elem.CodCompDigit;
                    frk_comepetenze._matricola = selMatricola;
                    frk_comepetenze._stato = elem.Stato;
                    frk_comepetenze._tipoAgg = elem.TipoAgg;
                    frk_comepetenze._dataOraAgg = elem.DataOraAgg;
                    frk_comepetenze._codCompDigitLiv = elem.CodCompDigitLiv;
                    if (elem.CodCompDigitLiv == "")
                    {
                        descCompDigitLiv = "";
                        descCompDigitLivLunga = "";
                    }
                    else
                    {
                        descCompDigitLiv = (from compDigitLiv in cvEnt.DCompDigitLiv
                                            where (compDigitLiv.CodCompDigit == elem.CodCompDigit && compDigitLiv.CodCompDigitLiv == elem.CodCompDigitLiv)
                                            select compDigitLiv.DescCompDigitLiv).First().ToString();
                        descCompDigitLivLunga = (from compDigitLivLunga in cvEnt.DCompDigitLiv
                                                 where (compDigitLivLunga.CodCompDigit == elem.CodCompDigit && compDigitLivLunga.CodCompDigitLiv == elem.CodCompDigitLiv)
                                                 select compDigitLivLunga.DescCompDigitLivLunga).First().ToString();
                    }
                    descCompDigit = (from compDigit in cvEnt.DCompDigit
                                     where compDigit.CodCompDigit == elem.CodCompDigit
                                     select compDigit.DescCompDigit).First().ToString();
                    frk_comepetenze._descCompDigit = descCompDigit;
                    frk_comepetenze._descCompDigitLiv = descCompDigitLiv;
                    frk_comepetenze._descCompDigitLivLunga = Regex.Replace(descCompDigitLivLunga, "<.*?>", String.Empty);

                    if (!string.IsNullOrEmpty(frk_comepetenze._descCompDigit))
                    {
                        string value = frk_comepetenze._descCompDigitLivLunga;

                        string key = "";// frk_comepetenze._descCompDigit + " | ";
                        if (!String.IsNullOrWhiteSpace(frk_comepetenze._descCompDigitLiv))
                            key += frk_comepetenze._descCompDigitLiv;
                        else
                            key += "nd";

                        blockItemInfoList.Add(new KeyValueInlineBold
                        {
                            key = frk_comepetenze._descCompDigit,
                            value = key,
                            separator = "|"
                        });
                    }
                }
            }

            if (blockItemInfoList.Count > 0)
            {
                contentBlockInfoList.Add(new ContentBlockInfo("COMPETENZE DIGITALI (autovalutazione)", blockItemInfoList));
            }

            #endregion

            #region COMPETENZE LINGUISTICHE

            blockItemInfoList = new List<RenderableItem>();
            cvBox.lingue = new List<cvModel.Languages>();

            List<TCVLingue> lingue = (cvEnt.TCVLingue.Where(m => m.Matricola == selMatricola).OrderBy(y => y.CodLinguaLiv)).ToList();

            foreach (TCVLingue lang in lingue)
            {
                cvModel.Languages frk = new cvModel.Languages( CommonManager.GetCurrentUserMatricola( ) );

                frk._altraLingua = lang.AltraLingua;
                frk._codLingua = lang.CodLingua;
                frk._codLinguaLiv = lang.CodLinguaLiv;
                frk._dataOraAgg = lang.DataOraAgg;
                frk._livAscolto = lang.LivAscolto;
                frk._livInterazione = lang.LivInterazione;
                frk._livLettura = lang.LivLettura;
                frk._livProdOrale = lang.LivProdOrale;
                frk._livScritto = lang.LivScritto;
                frk._matricola = lang.Matricola;
                frk._stato = lang.Stato;
                frk._tipoAgg = lang.TipoAgg;

                //Descrizione Lingua
                DLingua tmp_lingua = cvEnt.DLingua.Where(m => m.CodLingua == lang.CodLingua).First();
                frk._descLingua = tmp_lingua.DescLingua;

                //Flag dello Stato
                frk._flagStato = tmp_lingua.FlagStato;

                //Descrizione Livello di Lingua
                DLinguaLiv tmp_lingualiv = cvEnt.DLinguaLiv.Where(m => m.CodLinguaLiv == lang.CodLinguaLiv).First();
                frk._descLinguaLiv = tmp_lingualiv.DescLinguaLiv;

                if (!string.IsNullOrEmpty(frk._descLingua))
                {
                    string key = textInfo.ToTitleCase(frk._descLingua) + "\r\n";

                    string value = "";
                    value += "Ascolto " + (!string.IsNullOrEmpty(frk._livAscolto) ? frk._livAscolto : "nd") + " - ";
                    value += "Lettura " + (!string.IsNullOrEmpty(frk._livLettura) ? frk._livLettura : "nd") + " - ";
                    value += "Interazione " + (!string.IsNullOrEmpty(frk._livInterazione) ? frk._livInterazione : "nd") + " - ";
                    value += "Produzione orale " + (!string.IsNullOrEmpty(frk._livProdOrale) ? frk._livProdOrale : "nd") + " - ";
                    value += "Scritto " + (!string.IsNullOrEmpty(frk._livScritto) ? frk._livScritto : "nd");

                    blockItemInfoList.Add(new KeyValue
                    {
                        key = key,
                        value = value
                    });
                }
            }

            if (blockItemInfoList.Count > 0)
            {
                contentBlockInfoList.Add(new ContentBlockInfo("COMPETENZE LINGUISTICHE (autovalutazione)", blockItemInfoList));
            }

            #endregion

            #region COMPETENZE SPECIALISTICHE

            blockItemInfoList = new List<RenderableItem>();

            cvBox.competenzeSpecialistiche = new List<cvModel.CompetenzeSpecialistiche>();

            string figura_professionale = anagrafica._codiceFigProf;

            string fpForzata = GetFiguraProfessionaleForzata(selMatricola);
            if (fpForzata != null) figura_professionale = fpForzata;

            List<Data.CurriculumVitae.DConProf> DConProf =
                cvEnt.DConProf.Where(x => x.FiguraProfessionale == figura_professionale && x.Stato == "0" && x.FiguraProfessionale != "XAA" && x.FiguraProfessionale != "XDA").ToList();
            List<TCVConProf> TCVConProf = cvEnt.TCVConProf.Where(x => x.Matricola == selMatricola).ToList();

            foreach (var item in DConProf.GroupBy(x => x.CodConProfAggr))
            {
                var key = item.FirstOrDefault(x => x.CodConProf == item.Key);

                KeyValueList kvl = new KeyValueList();
                if (key != null)
                    kvl.key = key.DescConProf;

                foreach (var elem in item.OrderBy(y => y.Posizione))
                {
                    if (elem.CodConProf == item.Key) continue;

                    cvModel.CompetenzeSpecialistiche frk_compSpec = new cvModel.CompetenzeSpecialistiche();

                    var tmp_tcvConProf = TCVConProf.Where(x => x.CodConProf == elem.CodConProf);

                    frk_compSpec._codConProf = elem.CodConProf;
                    frk_compSpec._codConProfAggr = elem.CodConProfAggr;
                    frk_compSpec._dataOraAgg = null;
                    frk_compSpec._descConProf = elem.DescConProf;
                    frk_compSpec._descConProfLunga = elem.DescConProfLunga;
                    frk_compSpec._figuraProfessionale = figura_professionale;

                    //setto il flag _isSelected
                    if (tmp_tcvConProf.Count() > 0)
                    {
                        frk_compSpec._isSelected = true;
                        frk_compSpec._codConProfLiv = tmp_tcvConProf.First().CodConProfLiv;
                        frk_compSpec._flagPrincipale = tmp_tcvConProf.First().FlagPrincipale;
                    }
                    else
                    {
                        frk_compSpec._isSelected = false;
                        frk_compSpec._codConProfLiv = null;
                    }

                    //setto il flag _isTitle
                    if ((elem.DescConProf.Contains("skill")))
                    {
                        frk_compSpec._isTitle = true;
                    }
                    else
                    {
                        frk_compSpec._isTitle = false;
                    }

                    frk_compSpec._matricola = selMatricola;
                    frk_compSpec._posizione = Convert.ToInt32(elem.Posizione);
                    frk_compSpec._prog = 1;
                    frk_compSpec._stato = "";
                    frk_compSpec._tipoAgg = "";

                    //string value = string.Format("{0} | {1}",
                    //    frk_compSpec._figuraProfessionale, frk_compSpec._descConProf);
                    string value = frk_compSpec._descConProf;

                    int level;
                    Int32.TryParse(frk_compSpec._codConProfLiv, out level);

                    if (level > 0)
                    {
                        kvl.values.Add(new KeyLevel
                        {
                            key = value,
                            level = level,
                            range = 3,
                            favorite = frk_compSpec._flagPrincipale == "1"
                        });
                    }
                }

                if (kvl.values.Count() > 0)
                {
                    blockItemInfoList.Add(kvl);
                }
            }

            if (blockItemInfoList.Count > 0)
            {
                contentBlockInfoList.Add(new ContentBlockInfo("COMPETENZE SPECIALISTICHE", blockItemInfoList));
            }

            #endregion

            #region AREE DI INTERESSE

            blockItemInfoList = new List<RenderableItem>();
            cvBox.areeInteresse = new List<cvModel.AreeInteresse>();

            string descAreaOrg, descServizio, descTipoDispo, descAreaGeo;

            List<TCVAreaIntAz> interesse = cvEnt.TCVAreaIntAz.Where(m => m.Matricola == selMatricola).ToList();

            foreach (var area in interesse)
            {
                cvModel.AreeInteresse frk = new cvModel.AreeInteresse( CommonManager.GetCurrentUserMatricola( ) );

                frk._areeIntDispo = area.AreeIntDispo;
                frk._codAreaGeo = area.CodAreaGeo;
                frk._codAreaOrg = area.CodAreaOrg;
                frk._codServizio = area.CodServizio;
                frk._codTipoDispo = area.CodTipoDispo;
                frk._dataOraAgg = area.DataOraAgg;
                frk._flagEsteroDispo = area.FlagEsteroDispo;
                frk._matricola = area.Matricola;
                frk._profIntDispo = area.ProfIntDispo;
                frk._prog = area.Prog;
                frk._stato = area.Stato;
                frk._tipoAgg = area.TipoAgg;

                var frk_geogio = cvEnt.DAreaGeoGio.Where(m => m.CodAreaGeoGio == area.CodAreaGeo).ToList();

                if (frk_geogio.Count > 0)
                {
                    descAreaGeo = frk_geogio.First().DesAreaGeoGio;
                }
                else
                {
                    descAreaGeo = null;
                }

                descAreaOrg = null;
                var frk_org = frk.AreaInteresseItems.FirstOrDefault(x => x.Value == area.CodAreaOrg);

                if (frk_org != null)
                    descAreaOrg = frk_org.Text;

                var frk_tipodisto = cvEnt.DTipoDispo.Where(m => m.CodTipoDispo == area.CodTipoDispo).ToList();

                if (frk_tipodisto.Count > 0)
                {
                    descTipoDispo = frk_tipodisto.First().DescTipoDispo;
                }
                else
                {
                    descTipoDispo = null;
                }

                var frk_servizio = cvEnt.VDServizioCV.Where(m => m.Codice == area.CodServizio).ToList();

                if (frk_servizio.Count > 0)
                {
                    descServizio = frk_servizio.First().Descrizione;
                }
                else
                {
                    descServizio = null;
                }

                frk._descAreaGeo = descAreaGeo;
                frk._descAreaOrg = descAreaOrg;
                frk._descServizio = descServizio;
                frk._descTipoDispo = descTipoDispo;

                List<string> elencoSedi = new List<string>();
                //cvEnt.TCVAreaIntAzEstero.Where(x => x.Matricola == area.Matricola && x.Prog == area.Prog).Select(y => y.Codice).ForEach(delegate (string codice)
                //{
                //    var sede = cvEnt.DTabellaCV.FirstOrDefault(x => x.NomeTabella == "LocalitaEsp" && x.Codice == codice);
                //    if (sede != null)
                //        elencoSedi.Add(sede.Descrizione);
                //});

                foreach (var item in cvEnt.TCVAreaIntAzEstero.Where(x => x.Matricola == area.Matricola && x.Prog == area.Prog).Select(y => y.Codice))
                {
                    var sede = cvEnt.DTabellaCV.FirstOrDefault(x => x.NomeTabella == "LocalitaEsp" && x.Codice == item);
                    if (sede != null)
                        elencoSedi.Add(sede.Descrizione);
                }

                KeyValueList kvl = new KeyValueList();
                kvl.values = new List<RenderableItem>();

                kvl.key = frk._descAreaOrg;
                if (!String.IsNullOrWhiteSpace(frk._areeIntDispo))
                    kvl.values.Add(new KeyValueInline() { key = "Note", value = (frk._areeIntDispo != null ? frk._areeIntDispo.Trim() : "") });
                if (!String.IsNullOrWhiteSpace(frk._areeIntDispo))
                    kvl.values.Add(new KeyValueInline() { key = "Profili di interesse", value = (frk._areeIntDispo != null ? frk._areeIntDispo.Trim() : "") });
                if (!String.IsNullOrWhiteSpace(frk._descTipoDispo))
                    kvl.values.Add(new KeyValueInline() { key = "Interesse allo svolgimento di mansioni diverse da quelle attualmente svolte, nel rispetto delle competenze professionali acquisite", value = frk._descTipoDispo });
                kvl.values.Add(new KeyValueInline() { key = "Interesse a svolgere la propria attività anche all'estero", value = frk._flagEsteroDispo == "S" ? "Sì" : "No" });
                if (elencoSedi != null && elencoSedi.Count() > 0)
                    kvl.values.Add(new KeyValueInline() { key = "Presso", value = string.Join(", ", elencoSedi) });

                blockItemInfoList.Add(kvl);
            }

            if (blockItemInfoList.Count > 0)
            {
                contentBlockInfoList.Add(new ContentBlockInfo("AREE DI INTERESSE AZIENDALE", blockItemInfoList));
            }

            #endregion

            #region SU DI TE

            blockItemInfoList = new List<RenderableItem>();
            matricola = anagrafica._matricola;
            TCVAltreInf altreInformazioni;
            List<TCVAltreInfPat> listaPatenti = cvEnt.TCVAltreInfPat.Where(x => x.Matricola == matricola).ToList();
            try
            {
                altreInformazioni = cvEnt.TCVAltreInf.Where(x => x.Matricola == matricola).FirstOrDefault();
            }
            catch (Exception ex)
            {
                altreInformazioni = null;
            }

            if (altreInformazioni != null)
            {
                cvModel.AltreInfo frk_altreInfo = new cvModel.AltreInfo( CommonManager.GetCurrentUserMatricola( ) );
                frk_altreInfo._dataOraAgg = altreInformazioni.DataOraAgg;
                frk_altreInfo._email = altreInformazioni.EMail;
                frk_altreInfo._matricola = matricola;
                frk_altreInfo._note = altreInformazioni.Note;
                frk_altreInfo._numTel1 = altreInformazioni.NumTel1;
                frk_altreInfo._numTel2 = altreInformazioni.NumTel2;
                frk_altreInfo._sitoWeb = altreInformazioni.Sitoweb;
                frk_altreInfo._stato = altreInformazioni.Stato;
                frk_altreInfo._tipoAgg = altreInformazioni.TipoAgg;
                frk_altreInfo._tipoTel1 = altreInformazioni.TipoTel1;
                frk_altreInfo._tipoTel2 = altreInformazioni.TipoTel2;

                frk_altreInfo._tipoPatente = new List<DTipoPatente>();

                if (listaPatenti.Count > 0)
                {
                    foreach (var elem in listaPatenti)
                    {
                        DTipoPatente item = new DTipoPatente();
                        item = cvEnt.DTipoPatente.Where(x => x.CodTipoPatente == elem.CodTipoPatente).First();
                        frk_altreInfo._tipoPatente.Add(item);
                    }
                }
                else
                {
                    frk_altreInfo._tipoPatente = null;
                }
                cvBox.altreInformazioni = frk_altreInfo;

                string key = "";
                string value = "";

                KeyValueList kvl = new KeyValueList();

                if (!String.IsNullOrEmpty(frk_altreInfo._email))
                {
                    kvl.values.Add(new KeyValueInline() { key = "Email personale", value = frk_altreInfo._email });
                    //key = string.Format("Email personale: {0}\r\n", frk_altreInfo._email);
                }
                if (!String.IsNullOrEmpty(frk_altreInfo._numTel1) && !String.IsNullOrEmpty(frk_altreInfo._tipoTel1))
                {
                    kvl.values.Add(new KeyValueInline() { key = frk_altreInfo._tipoTel1, value = frk_altreInfo._numTel1 });
                    //key += string.Format("{0}: {1}\r\n", frk_altreInfo._tipoTel1, frk_altreInfo._numTel1);
                }
                if (!String.IsNullOrEmpty(frk_altreInfo._numTel2) && !String.IsNullOrEmpty(frk_altreInfo._tipoTel2))
                {
                    kvl.values.Add(new KeyValueInline() { key = frk_altreInfo._tipoTel2, value = frk_altreInfo._numTel2 });
                    //key += string.Format("{0}: {1}\r\n", frk_altreInfo._tipoTel2, frk_altreInfo._numTel2);
                }
                if (!String.IsNullOrEmpty(frk_altreInfo._sitoWeb))
                {
                    kvl.values.Add(new KeyValueInline() { key = "Sito web/blog/pagina social", value = frk_altreInfo._sitoWeb });
                    //key += string.Format("Sito web/blog/pagina social: {0}\r\n", frk_altreInfo._sitoWeb);
                }
                if (!String.IsNullOrEmpty(frk_altreInfo._indirizzo_domicilio))
                {
                    kvl.values.Add(new KeyValueInline() { key = "Indirizzo domicilio", value = frk_altreInfo._indirizzo_domicilio });
                    //key += string.Format("Indirizzo domicilio: {0}\r\n", frk_altreInfo._indirizzo_domicilio);
                }
                if (!String.IsNullOrEmpty(frk_altreInfo._indirizzo_residenza))
                {
                    kvl.values.Add(new KeyValueInline() { key = "Indirizzo residenza", value = frk_altreInfo._indirizzo_residenza });
                    //key += string.Format("Indirizzo residenza: {0}\r\n", frk_altreInfo._indirizzo_residenza);
                }

                if (frk_altreInfo._tipoPatente != null && frk_altreInfo._tipoPatente.Any())
                {
                    key += string.Format("Patente:");
                    frk_altreInfo._tipoPatente.ForEach(p =>
                    {
                        key += String.Format(" {0} ", p.DescTipoPatente);
                    });

                    key += "\r\n";
                }

                if (!String.IsNullOrEmpty(frk_altreInfo._note))
                {
                    kvl.values.Add(new KeyValueInline() { key = "Ulteriori informazioni", value = frk_altreInfo._note });
                    //key += string.Format("Ulteriori informazioni: {0}", frk_altreInfo._note);
                }

                if (kvl.values.Count > 0)
                {
                    blockItemInfoList.Add(kvl);
                }

                if (blockItemInfoList.Count > 0)
                {
                    contentBlockInfoList.Add(new ContentBlockInfo("SU DI TE", blockItemInfoList));
                }
            }
            #endregion

            #region Corsi Rai
            blockItemInfoList = new List<RenderableItem>();
            List<V_CVCorsiRai> listaRai = new List<V_CVCorsiRai>();

            try
            {
                //listaRai = cvEnt.V_CVCorsiRai.Where(m => m.matricola == selMatricola).OrderByDescending(x => x.DataInizioDate).ToList();
                listaRai = RaiAcademyManager.GetCorsiFatti(selMatricola).OrderByDescending(x => x.DataInizioDate).ToList();
            }
            catch (Exception ex)
            {

            }

            foreach (var elem in listaRai)
            {
                if (!string.IsNullOrEmpty(elem.Societa) && !string.IsNullOrEmpty(elem.TitoloCorso))
                {
                    string DataInizio = String.Empty;
                    string DataFine = String.Empty;
                    if (!string.IsNullOrEmpty(elem.DataInizio))
                    {
                        DataInizio = "Dal " + elem.DataInizio;
                    }
                    if (!string.IsNullOrEmpty(elem.DataFine))
                    {
                        DataFine = "Al " + elem.DataFine;
                    }

                    string key = String.Empty;
                    string value = String.Empty;
                    string periodo = DataInizio + " " + DataFine;


                    KeyValueList keyValueList = new KeyValueList();
                    keyValueList.values = new List<RenderableItem>();

                    if (!string.IsNullOrEmpty(elem.TitoloCorso))
                        //keyValueList.key += "Titolo: " + elem.TitoloCorso; //key += "\r\n" + "Titolo: " + elem.TitoloCorso;
                        keyValueList.values.Add(new KeyValueInline() { key = "Titolo", value = elem.TitoloCorso });

                    if (!string.IsNullOrEmpty(elem.Societa))
                        keyValueList.values.Add(new KeyValueInline() { key = "Società", value = elem.Societa });
                    if (!string.IsNullOrEmpty(elem.DataInizio))
                        keyValueList.values.Add(new KeyValueInline() { key = "Periodo", value = periodo });
                    if (elem.Durata != null)
                        keyValueList.values.Add(new KeyValueInline() { key = "Durata", value = elem.Durata.Value.ToString() + " ore" });

                    blockItemInfoList.Add(keyValueList);
                }
            }

            if (blockItemInfoList.Count > 0)
            {
                contentBlockInfoList.Add(new ContentBlockInfo("CORSI DI FORMAZIONE RAI (Dati prelevati dagli archivi aziendali)", blockItemInfoList));
            }

            #endregion

            #region CORSI DI FORMAZIONE

            blockItemInfoList = new List<RenderableItem>();

            cvBox.formazione = new List<cvModel.Formazione>();

            List<TCVFormExRai> formaz = new List<TCVFormExRai>();

            formaz = cvEnt.TCVFormExRai.Where(x => x.Matricola == selMatricola).ToList();
            foreach (var elem in formaz)
            {
                cvModel.Formazione frk_formazione = new cvModel.Formazione( CommonManager.GetCurrentUserMatricola( ) );

                frk_formazione._anno = elem.Anno;
                frk_formazione._codNazione = elem.CodNazione;
                frk_formazione._corso = elem.Corso;
                frk_formazione._dataOraAgg = elem.DataOraAgg;
                if (!string.IsNullOrEmpty(elem.CodNazione))
                {
                    frk_formazione._descNazione = cvEnt.DNazione.Where(a => a.COD_SIGLANAZIONE == elem.CodNazione).FirstOrDefault().DES_NAZIONE;
                }
                else
                {
                    frk_formazione._descNazione = "";
                }
                frk_formazione._durata = elem.Durata;
                frk_formazione._localitaCorso = elem.LocalitaCorso;
                frk_formazione._matricola = elem.Matricola;
                frk_formazione._note = elem.Note;
                frk_formazione._presso = elem.Presso;
                frk_formazione._prog = elem.Prog;
                frk_formazione._stato = elem.Stato;
                frk_formazione._tipoAgg = elem.TipoAgg;

                KeyValueList kvl = new KeyValueList();
                if (!string.IsNullOrEmpty(frk_formazione._corso))
                    //kvl.key = "Titolo: " + frk_formazione._corso;
                    kvl.values.Add(new KeyValueInline() { key = "Titolo", value = frk_formazione._corso });

                if (!string.IsNullOrEmpty(frk_formazione._presso))
                    kvl.values.Add(new KeyValueInline() { key = "Società", value = frk_formazione._presso });
                //value += "\r\n" + "Società: " + frk_formazione._presso;
                if (!string.IsNullOrEmpty(frk_formazione._anno))
                    kvl.values.Add(new KeyValueInline() { key = "Anno", value = frk_formazione._anno });
                //value += "\r\n" + "Anno: " + frk_formazione._anno;
                if (!string.IsNullOrEmpty(frk_formazione._durata))
                    kvl.values.Add(new KeyValueInline() { key = "Durata", value = frk_formazione._durata });
                //value += "\r\n" + "Durata: " + frk_formazione._durata;
                if (!string.IsNullOrEmpty(frk_formazione._localitaCorso))
                    kvl.values.Add(new KeyValueInline() { key = "Città", value = frk_formazione._localitaCorso });
                //value += "\r\n" + "Città: " + frk_formazione._localitaCorso;
                if (!string.IsNullOrEmpty(frk_formazione._descNazione))
                    kvl.values.Add(new KeyValueInline() { key = "Paese", value = frk_formazione._descNazione });
                //value += "\r\n" + "Paese: " + frk_formazione._descNazione;
                if (!string.IsNullOrEmpty(frk_formazione._note))
                    kvl.values.Add(new KeyValueInline() { key = "Note", value = frk_formazione._presso });
                //value += "\r\n" + "Note: " + frk_formazione._note;

                blockItemInfoList.Add(kvl);
            }

            if (blockItemInfoList.Count > 0)
            {
                contentBlockInfoList.Add(new ContentBlockInfo("CORSI DI FORMAZIONE EXTRA RAI", blockItemInfoList));
            }

            #endregion

            #region Allegati
            blockItemInfoList = new List<RenderableItem>();
            var allegati = cvEnt.TCVAllegato.Where(x => x.Matricola == selMatricola && x.Stato != "#").ToList();

            string homeUrl = System.Web.HttpContext.Current.Request.Url.Scheme + "://" + System.Web.HttpContext.Current.Request.Url.Authority;

            foreach (TCVAllegato all in allegati)
            {
                cvModel.Allegati frk_all = new cvModel.Allegati();

                frk_all._contentType = all.ContentType;
                frk_all._dataOraAgg = all.DataOraAgg;
                frk_all._id = all.Id;
                frk_all._idBox = all.Id_box;
                frk_all._matricola = selMatricola;
                frk_all._name = all.Name;
                frk_all._pathName = all.Path_name;
                frk_all._size = all.Size;
                frk_all._stato = all.Stato;
                frk_all._tipoAgg = all.TipoAgg;
                frk_all._note = all.Note;

                KeyValueList kvl = new KeyValueList();

                KeyValueJustified block = new KeyValueJustified();
                block.key = "";
                string configPath = GetShareAllegati();

                if (frk_all._contentType == "website")
                {
                    string link = frk_all._pathName;
                    kvl.chk = new Chunk("Clicca qui per vedere " + frk_all._name, new FontManager("", BaseColor.BLACK).Italic).SetAction(new PdfAction(link, false));
                }
                else
                {
                    string link = homeUrl + "/cv_online/GetFile/" + frk_all._id;
                    kvl.chk = new Chunk("Clicca qui per vedere " + frk_all._name, new FontManager("", BaseColor.BLACK).Italic).SetAction(new PdfAction(link, false));
                }
                
                block.value = frk_all._note;

                string formato_file = "File Generico";
                if (frk_all._contentType.Contains("audio"))
                    formato_file = "File Audio";
                else if (frk_all._contentType.Contains("video"))
                    formato_file = "File Video";
                else if (frk_all._contentType.Contains("image"))
                    formato_file = "File Immagine";
                else if (frk_all._contentType.Contains("word"))
                    formato_file = "File Word";
                else if (frk_all._contentType.Contains("excel"))
                    formato_file = "File Excel";
                else if (frk_all._contentType.Contains("pdf"))
                    formato_file = "File Pdf";
                else if (frk_all._contentType.Contains("website"))
                    formato_file = "Link a sito web";

                kvl.values.Add(new KeyValueInline() { key = "Tipologia", value = formato_file });
                kvl.values.Add(new KeyValueInline() { key = "Titolo", value = frk_all._name });
                kvl.values.Add(new KeyValueInline() { key = "Descrizione", value = frk_all._note });
                //kvl.values.Add(block);

                blockItemInfoList.Add(kvl);
            }

            if (blockItemInfoList.Count > 0)
            {
                contentBlockInfoList.Add(new ContentBlockInfo("CONTRIBUTI MULTIMEDIALI", blockItemInfoList));
            }
            #endregion

            RightContentManager rm = new RightContentManager(writer.DirectContentUnder, document, contentBlockInfoList);

            rm.Title = string.Format("{0} {1}", anagrafica._nome, anagrafica._cognome);

            rm.SubTitle = anagrafica._figProfessionale;

            rm.IndentationLeft = indentationLeft;

            rm.Render();

            document.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            return new FileStreamResult(workStream, "application/pdf");
        }

        private static void PdfExperienceTest(cv_ModelEntities cvEnt, string codFigProf, List<RenderableItem> blockItemInfoList, TCVEsperExRai esp)
        {
            KeyValueList kvl = new KeyValueList();

            cvModel.Experiences frk_esp = new cvModel.Experiences( CommonManager.GetCurrentUserMatricola( ) );
            frk_esp._areaAtt = esp.AreaAtt;
            frk_esp._codContinente = esp.CodContinente;

            if (esp.CodContinente != null)
            {
                frk_esp._descContinente = cvEnt.DContinente.Where(x => x.CodContinente == esp.CodContinente).First().DesContinente.ToString();
            }
            else
            {
                frk_esp._descContinente = "";
            }

            frk_esp._codRedazione = esp.CodRedazione;

            if (esp.CodRedazione != null)
            {
                frk_esp._descRedazione = cvEnt.DRedazione.Where(x => x.CodRedazione == esp.CodRedazione).First().DesRedazione.ToString();
            }
            else
            {
                frk_esp._descRedazione = "";
            }

            if (esp.CodiceFiguraProf != null)
            {
                var figuraProf = frk_esp.ListeFigureRai.FirstOrDefault(a => a.CodiceFiguraPro == esp.CodiceFiguraProf);
                if (figuraProf != null)
                    frk_esp._codiceFiguraProf = figuraProf.DescriFiguraPro;
                else
                    frk_esp._codiceFiguraProf = "";
            }
            else if (esp.CodFigProExtra != null && esp.CodFigProExtra != "" && esp.CodFigProExtra != "-1")
            {
                frk_esp._codiceFiguraProf = cvEnt.DTabellaCV.Where(x => x.NomeTabella == "FigProExtra" && x.Codice == esp.CodFigProExtra).FirstOrDefault().Descrizione.ToString();
            }
            else
            {
                frk_esp._codiceFiguraProf = "";
            }

            frk_esp._codDirezione = esp.CodDirezione;

            if (esp.CodDirezione == "-1")
            {
                frk_esp._direzione = esp.Direzione;
            }
            else
            {
                if (esp.CodDirezione != null && esp.CodDirezione != "")
                {
                    frk_esp._direzione = cvEnt.VDServizioCV.Where(x => x.Codice == esp.CodDirezione).First().Descrizione.ToString();
                }
                else
                {
                    frk_esp._direzione = "";
                }
            }

            frk_esp._codSocieta = esp.CodSocieta;

            if ((esp.CodSocieta != null) && (esp.CodSocieta != "") && (esp.CodSocieta != "-1"))
            {
                frk_esp._societa = cvEnt.VDSocieta.Where(x => x.Codice == esp.CodSocieta).First().Descrizione.ToString();
            }
            else
            {
                frk_esp._societa = esp.Societa;
            }

            frk_esp._stato = esp.Stato;
            frk_esp._dataFine = esp.DataFine;
            frk_esp._dataInizio = esp.DataInizio;
            frk_esp._dataOraAgg = esp.DataOraAgg;
            frk_esp._descrizioneEsp = esp.DescrizioneEsp;
            frk_esp._flagEspEstero = esp.FlagEspEstero;
            frk_esp._flagEspRai = esp.FlagEspRai;

            if ((codFigProf == "MAA") || (codFigProf == "MBA"))
            {
                frk_esp._isGiornalista = "1";
            }
            else
            {
                frk_esp._isGiornalista = "0";
            }

            frk_esp._localitaEsp = esp.LocalitaEsp;
            frk_esp._matricola = esp.Matricola;
            frk_esp._nazione = esp.Nazione;
            frk_esp._prog = esp.Prog;
            frk_esp._tipoAgg = esp.TipoAgg;
            frk_esp._titoloEspQual = esp.TitoloEspQual;
            frk_esp._ultRuolo = esp.UltRuolo;

            if (esp.CodBudgetGest != null && esp.CodBudgetGest != "" && esp.CodBudgetGest != "-1")
            {
                frk_esp._budgetGest = cvEnt.DTabellaCV.Where(x => x.NomeTabella == "Budget" && x.Codice == esp.CodBudgetGest).FirstOrDefault().Descrizione.ToString();
            }
            else
            {
                frk_esp._budgetGest = "";

            }

            if (esp.CodSettore != null && esp.CodSettore != "" && esp.CodSettore != "-1")
            {
                frk_esp._codIndustry = cvEnt.DTabellaCV.Where(x => x.NomeTabella == "Settore" && x.Codice == esp.CodSettore).FirstOrDefault().Descrizione.ToString();
            }
            else if (esp.CodSettore == "-1")
            {
                frk_esp._codIndustry = esp.Settore;
            }
            else
            {
                frk_esp._codIndustry = "";

            }

            if (esp.CodRisorseGest != null && esp.CodRisorseGest != "" && esp.CodRisorseGest != "-1")
            {
                frk_esp._risorseGest = cvEnt.DTabellaCV.Where(x => x.NomeTabella == "Risorse" && x.Codice == esp.CodRisorseGest).FirstOrDefault().Descrizione.ToString();
            }
            else
            {
                frk_esp._risorseGest = "";

            }

            frk_esp._procura = esp.CodProcura;

            if (!string.IsNullOrEmpty(frk_esp._ultRuolo))
            {
                bool dataInizioValida = !String.IsNullOrWhiteSpace(frk_esp._dataInizio);// && frk_esp._dataInizio.Trim().Length == 8;

                int fyear = 0;
                int fmonth = 0;
                int fday = 0;

                DateTime fdate;
                string fromMonth = "";

                string dataInizio = "";
                string dataFine = "";

                string txInizio = "dal";
                string txFine = "al";

                Int32.TryParse(frk_esp._annoInizio, out fyear);
                Int32.TryParse(frk_esp._meseInizio, out fmonth);
                Int32.TryParse(frk_esp._giornoInizio, out fday);

                if (fday > 0)
                {
                    try
                    {
                        fdate = new DateTime(fyear, fmonth, fday);
                    }
                    catch
                    {
                        dataInizioValida = false;
                    }
                }

                if (dataInizioValida)
                {
                    fromMonth = CommonManager.TraduciMeseDaNumLett(frk_esp._meseInizio);

                    if (fday > 0)
                    {
                        if (fday == 1 || fday == 8 || fday == 11)
                            dataInizio = "dall'";
                        else
                            dataInizio = "dal ";
                        dataInizio += fday + " " + fromMonth + " " + fyear;
                    }
                    else if (fmonth > 0)
                    {
                        dataInizio = "da " + fromMonth + " " + fyear;
                    }
                    else
                    {
                        dataInizio = "dal " + fyear;
                    }
                }

                string value = "";
                if (!string.IsNullOrEmpty(frk_esp._societa))
                    kvl.values.Add(new KeyValueInline() { key = "Società", value = frk_esp._societa });
                //value += "\r\n" + "Società: " + frk_esp._societa;
                if (!string.IsNullOrEmpty(frk_esp._codIndustry))
                    kvl.values.Add(new KeyValueInline() { key = "Settore", value = frk_esp._codIndustry });
                //value += "\r\n" + "Settore: " + frk_esp._codIndustry;
                if (!string.IsNullOrEmpty(frk_esp._direzione))
                    kvl.values.Add(new KeyValueInline() { key = "Direzione", value = frk_esp._direzione });
                //value += "\r\n" + "Direzione: " + frk_esp._direzione;
                if (!string.IsNullOrEmpty(frk_esp._descRedazione))
                    kvl.values.Add(new KeyValueInline() { key = "Redazione", value = frk_esp._descRedazione });
                //value += "\r\n" + "Redazione: " + frk_esp._descRedazione;
                if (frk_esp._dataFine == null)
                {
                    if (dataInizioValida)
                        kvl.values.Add(new KeyValueInline() { key = "Periodo", value = dataInizio + " - in corso" });
                    //value += "\r\n" + "Periodo: " + dataInizio + " - in corso";
                    else
                        kvl.values.Add(new KeyValueInline() { key = "Periodo", value = "non disponibile" });
                    //value += "\r\n" + "Periodo: non disponibile";
                }
                else
                {
                    bool dataFineValida = true;

                    int tyear = 0;
                    int tmonth = 0;
                    int tday = 0;
                    Int32.TryParse(frk_esp._annoFine, out tyear);
                    Int32.TryParse(frk_esp._meseFine, out tmonth);
                    Int32.TryParse(frk_esp._giornoFine, out tday);

                    if (tday > 0)
                    {
                        try
                        {
                            fdate = new DateTime(tyear, tmonth, tday);
                        }
                        catch
                        {
                            dataFineValida = false;
                        }
                    }

                    if (dataFineValida)
                    {
                        string toMonth = CommonManager.TraduciMeseDaNumLett(frk_esp._meseFine);

                        if (tday == 1 ||
                            tday == 8 ||
                            tday == 11)
                            txFine = "all'";

                        if (tday > 0)
                        {
                            if (tday == 1 || tday == 8 || tday == 11)
                                dataFine = "all'";
                            else
                                dataFine = "al ";
                            dataFine += tday + " " + toMonth + " " + tyear;
                        }
                        else if (tmonth > 0)
                        {
                            dataFine = "a " + toMonth + " " + tyear;
                        }
                        else
                        {
                            dataFine = "al " + tyear;
                        }
                    }

                    if (dataInizioValida && dataFineValida)
                        kvl.values.Add(new KeyValueInline() { key = "Periodo", value = dataInizio + " " + dataFine });
                    //value += "\r\n" + "Periodo: " + dataInizio + " " + dataFine;
                    else
                        kvl.values.Add(new KeyValueInline() { key = "Periodo", value = "non disponibile" });
                    //value += "\r\n" + "Periodo: non disponibile";

                    // se la data di fine è il mese e l'anno corrente ed
                    // il giorno termine esperienza è superiore alla data odierna,
                    // allora verrà stampata la dicitura "attuale da mese anno".
                    // Altrimenti verrà stampata la dicitura "da mese anno a mese anno"
                    // in precedenza calcolata
                    if (dataInizioValida && dataFineValida &&
                        tmonth.Equals(DateTime.Now.Month) &&
                        tday >= DateTime.Now.Day &&
                        tyear.Equals(DateTime.Now.Year))
                    {
                        kvl.values.Add(new KeyValueInline() { key = "Periodo", value = "attuale " + dataInizio });
                        //value += "\r\n" + "Periodo: attuale " + dataInizio;
                    }
                }

                if (!string.IsNullOrEmpty(frk_esp._codiceFiguraProf))
                    kvl.values.Add(new KeyValueInline() { key = "Figura Professionale", value = frk_esp._codiceFiguraProf });
                //value += "\r\n" + "Figura Professionale: " + frk_esp._codiceFiguraProf;
                if (!string.IsNullOrEmpty(frk_esp._localitaEsp))
                    kvl.values.Add(new KeyValueInline() { key = "Località", value = frk_esp._localitaEsp });
                //value += "\r\n" + "Località: " + frk_esp._localitaEsp;
                if (!string.IsNullOrEmpty(frk_esp._nazione))
                    kvl.values.Add(new KeyValueInline() { key = "Nazione", value = frk_esp._nazione.TitleCase() });
                //value += "\r\n" + "Nazione: " + frk_esp._nazione.TitleCase();
                if (!string.IsNullOrEmpty(frk_esp._areaAtt))
                    kvl.values.Add(new KeyValueInline() { key = "Area Attività", value = frk_esp._areaAtt });
                //value += "\r\n" + "Area Attività: " + frk_esp._areaAtt;

                if (!string.IsNullOrEmpty(frk_esp._titoloEspQual))
                    kvl.values.Add(new KeyValueInline() { key = "Titolo Esperienza qualificante", value = frk_esp._titoloEspQual });
                //value += "\r\n" + "Titolo Esperienza qualificante: " + frk_esp._titoloEspQual;

                //value += "\r\n" + "Descrizione sintetica dell'esperienza: " + frk_esp._descrizioneEsp;
                if (!string.IsNullOrEmpty(frk_esp._budgetGest))
                    kvl.values.Add(new KeyValueInline() { key = "Budget", value = frk_esp._budgetGest });
                //value += "\r\n" + "Budget: " + frk_esp._budgetGest;
                if (!string.IsNullOrEmpty(frk_esp._procura))
                    kvl.values.Add(new KeyValueInline() { key = "Procura", value = frk_esp._procura == "1" ? "No" : "Sì" });
                //value += frk_esp._procura == "1" ? "\r\n" + "Procura: No" : "\r\n" + "Procura: Si";
                if (!string.IsNullOrEmpty(frk_esp._risorseGest))
                    kvl.values.Add(new KeyValueInline() { key = "Risorse Gestite/Coordinate", value = frk_esp._risorseGest });
                //value += "\r\n" + "Risorse Gestite/Coordinate: " + frk_esp._risorseGest;

                string key = String.Empty;
                if (!string.IsNullOrEmpty(frk_esp._areaAtt))
                    key += "Attività: " + frk_esp._ultRuolo;

                //kvl.key = key;
                if (!string.IsNullOrEmpty(frk_esp._areaAtt))
                    kvl.values.Insert(0, new KeyValueInline() { key = "Attività", value = frk_esp._ultRuolo });

                if (!string.IsNullOrEmpty(frk_esp._descrizioneEsp))
                    kvl.values.Add(new KeyValueInline() { key = "Descrizione sintetica dell'esperienza", value = frk_esp._descrizioneEsp });

                blockItemInfoList.Add(kvl);
                //blockItemInfoList.Add(new KeyValue
                //{
                //    key = key,
                //    value = value.Trim()
                //});
            }
        }
    }
}