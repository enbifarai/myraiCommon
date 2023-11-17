using myRaiCommonModel;
using myRaiData.Incentivi;
using myRaiGestionale.Services;
using myRaiGestionale.RepositoryServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using myRaiCommonModel.Pagination;
using myRaiHelper;
using myRaiCommonManager;
using Xceed.Words.NET;
using System.IO;

namespace myRaiGestionale.Controllers
{
    public class PianoFormativoController : BaseCommonController
    {
        //
        // GET: /PianoFormativo/
        private IncentiviEntities dbContext = new IncentiviEntities();
        private PianoFormativoServizio pianoFormativoServizio;
        private ImmatricolazioneServizio immatricolazioneServizio;

        private AssQualRepository AssQualRepository;
        private GestioneDatiDaVisualizzare gestioneDatiDaVisualizzare;
        private AnagraficaRepository anagraficaRepository;
        private JobAssignRepository jobAssignRepository;
        private ImmatricolazioneRepository immatricolazioneRepository;

        public PianoFormativoController()
        {
            //Upload();
            pianoFormativoServizio = new PianoFormativoServizio(dbContext);
            anagraficaRepository = new AnagraficaRepository(dbContext);
            jobAssignRepository = new JobAssignRepository(dbContext);
            immatricolazioneRepository = new ImmatricolazioneRepository(dbContext);
            gestioneDatiDaVisualizzare = new GestioneDatiDaVisualizzare(dbContext);
        }
        public ActionResult Index(string id = null)
        {
            ViewBag.idPersona = id != null ? id : "0";
            return View();
        }
        public ActionResult GetIdJobAssign(int idPersona)
        {
            bool esiste = true;
            if (dbContext.JOBASSIGN.Join(dbContext.RUOLO, job => job.COD_RUOLO, ruolo => ruolo.COD_RUOLO, (job, ruolo)
                              => new { job.ID_PERSONA, job.ID_JOBASSIGN, ruolo.COD_RUOLOAGGREG }).Where(x => x.ID_PERSONA == idPersona && x.COD_RUOLOAGGREG == "PROFORM").ToList().Count() == 0)
                esiste = false;
            return Json(esiste);
        }
        /*        private void DownloadFile()
                {
                    byte[] template = dbContext.CZNMMDOC.Where(w => w.NME_FILENAME == "NewPfi.docx").Select(s => s.OBJ_OBJECT).FirstOrDefault();
                    string filename = "C:/Users/alessia.corsini/Downloads/provapfi";

                        System.IO.File.WriteAllBytes(filename,template);

                }
           */
        private void Upload()
        {
            string filename = @"C:\Users\Nik\Desktop\NewPfi.docx";
            byte[] template = System.IO.File.ReadAllBytes(filename);

            CZNMMDOC rect = dbContext.CZNMMDOC.Where(w => w.NME_FILENAME == "NewPfi.docx").FirstOrDefault();
            //dbContext.CZNMMDOC.Remove(rect);

            bool noData = rect == null;

            try
            {
                if (noData)
                {
                    rect = new CZNMMDOC();
                    rect.COD_EXTENSION = ".docx";
                    rect.COD_TERMID = "ADMIN";
                    rect.COD_USER = "Developer";
                    rect.DES_CZNMMDOC = "RAI_PFI";
                    rect.ID_CZNMMDOC = 159829291;
                    //rect.ID_DATASOURCE = 153862878;
                    rect.ID_DOCSCAT = null;
                    rect.ID_LANG = null;
                    rect.IND_PRIVATE = "N";
                    rect.NBR_FILESIZE = 142536;
                    rect.NME_ATTACHNAME = "PFI";
                    rect.NME_FILENAME = "NewPfi.docx";
                    rect.TMS_TIMESTAMP = DateTime.Today;
                }

                rect.OBJ_OBJECT = template;
                if (noData)
                    dbContext.CZNMMDOC.Add(rect);
                dbContext.SaveChanges();

            }
            catch (Exception e)
            {
                var ex = e;
            }
        }

        public ActionResult InizializzazioneTablePianiFormativi()
        {
            return PartialView("_subpartial/InizializzazioneTablePianiFormativi");
        }
        public ActionResult GetPianiFormativi(string tab, int? page)
        {
            /*PaginatedList<RicercaPianoFormativo> listaPianiFormativi = new PaginatedList<RicercaPianoFormativo>();*/
            ElencoPianiFormativi elenco = new ElencoPianiFormativi();
            elenco.daPianificare = new PaginatedList<RicercaPianoFormativo>();
            elenco.pianificati = new PaginatedList<RicercaPianoFormativo>();
            try
            {
                var lista = pianoFormativoServizio.GetPianiFormativi();
                elenco.tab = tab;
                elenco.totDaPianificare = 0;
                elenco.totPianificati = 0;
                switch (tab.ToLower())
                {
                    case "p":
                        elenco.pianificati = PaginatedList<RicercaPianoFormativo>.CratePaginatedList(lista.Where(x => x.IsRuoloAggreg == true), page.Value);
                        elenco.totPianificati = lista.Where(x => x.IsRuoloAggreg == true).Count();
                        break;
                    case "d":
                        elenco.daPianificare = PaginatedList<RicercaPianoFormativo>.CratePaginatedList(lista.Where(x => x.IsRuoloAggreg == false), page.Value);
                        elenco.totDaPianificare = lista.Where(x => x.IsRuoloAggreg == false).Count();
                        break;
                    default:
                        elenco.pianificati = PaginatedList<RicercaPianoFormativo>.CratePaginatedList(lista.Where(x => x.IsRuoloAggreg == true), page.Value);
                        elenco.totPianificati = lista.Where(x => x.IsRuoloAggreg == true).Count();
                        elenco.daPianificare = PaginatedList<RicercaPianoFormativo>.CratePaginatedList(lista.Where(x => x.IsRuoloAggreg == false), page.Value);
                        elenco.totDaPianificare = lista.Where(x => x.IsRuoloAggreg == false).Count();
                        break;

                }
                //   listaPianiFormativi = PaginatedList<RicercaPianoFormativo>.CratePaginatedList(pianoFormativoServizio.GetPianiFormativi(), page.Value);
            }
            catch (Exception e)
            {
                /*listaPianiFormativi = new PaginatedList<RicercaPianoFormativo>();*/
                elenco = new ElencoPianiFormativi();
            }
            return PartialView("_subpartial/ElencoPianiFormativi", elenco);
        }
        public ActionResult FiltraPratichePianiFormativi(string nome, string matricola, string sezione, string tutor, string tab, int? page)
        {
            PaginatedList<RicercaPianoFormativo> listaPianiFormativi = null;
            ElencoPianiFormativi elenco = new ElencoPianiFormativi();
            elenco.daPianificare = new PaginatedList<RicercaPianoFormativo>();
            elenco.pianificati = new PaginatedList<RicercaPianoFormativo>();
            elenco.totDaPianificare = 0;
            elenco.totPianificati = 0;
            try
            {
                var lista = pianoFormativoServizio.FiltraPianiFormativi(nome, matricola, sezione, tutor);
                switch (tab.ToLower())
                {
                    case "p":
                        elenco.pianificati = PaginatedList<RicercaPianoFormativo>.CratePaginatedList(lista.Where(x => x.IsRuoloAggreg == true), page.Value);
                        elenco.totPianificati = lista.Where(x => x.IsRuoloAggreg == true).Count();
                        break;
                    case "d":
                        elenco.daPianificare = PaginatedList<RicercaPianoFormativo>.CratePaginatedList(lista.Where(x => x.IsRuoloAggreg == false), page.Value);
                        elenco.totDaPianificare = lista.Where(x => x.IsRuoloAggreg == false).Count();
                        break;
                    default:
                        elenco.pianificati = PaginatedList<RicercaPianoFormativo>.CratePaginatedList(lista.Where(x => x.IsRuoloAggreg == true), page.Value);
                        elenco.daPianificare = PaginatedList<RicercaPianoFormativo>.CratePaginatedList(lista.Where(x => x.IsRuoloAggreg == false), page.Value);
                        elenco.totPianificati = lista.Where(x => x.IsRuoloAggreg == true).Count();
                        elenco.totDaPianificare = lista.Where(x => x.IsRuoloAggreg == false).Count();
                        break;

                }
                elenco.tab = tab;
            }
            catch (Exception e)
            {
                listaPianiFormativi = new PaginatedList<RicercaPianoFormativo>();
            }
            return PartialView("_subpartial/ElencoPianiFormativi", elenco);
        }
        #region SEZIONI_TAB 
        public ActionResult GetDatiApprendista(int id)
        {
            List<StudioModel> listaTitoliDiStudi = new List<StudioModel>();
            bool existTitolo = pianoFormativoServizio.CheckExistTitoliDiStudioApprendista(id);
            if (existTitolo)
            {
                listaTitoliDiStudi = pianoFormativoServizio.GetTitoliDiStudioById(id);
            }
            ViewBag.idPersona = id;

            return PartialView("_subpartial/SezioneDatiApprendista", listaTitoliDiStudi);
        }
        public ActionResult GetSezioneCompetenze(string codiceRuolo, List<Competenza> competenze = null)
        {
            Competenze viewmodel = new Competenze();
            try
            {
                viewmodel.ListaCompetenze = pianoFormativoServizio.GetCompetenze(codiceRuolo);
                if (competenze != null)
                {
                    for (int i = 0; i < viewmodel.ListaCompetenze.Count(); i++)
                    {
                        if (competenze.Where(x => x.CodiceRequisito == viewmodel.ListaCompetenze[i].CodiceRequisito).Count() > 0)
                            viewmodel.ListaCompetenze[i].SelectedCodiceCompetenza = true;
                        else viewmodel.ListaCompetenze[i].SelectedCodiceCompetenza = false;


                    }


                }

            }
            catch (Exception ex)
            {

                viewmodel = new Competenze();
            }

            return PartialView("_subpartial/SezioneTableCompetenze", viewmodel);
        }
        public ActionResult GetSezioneEsperienzeLavorative(int id)
        {
            List<EsperienzeLavorativeViewModel> viewmodel = new List<EsperienzeLavorativeViewModel>();
            ViewBag.idPersona = id;
            viewmodel = pianoFormativoServizio.GetEsperienzeLavorativeById(id);
            return PartialView("_subpartial/SezioneEsperienzeLavorative", viewmodel);
        }
        public ActionResult GetSezioneTutor(int id)
        {
            List<TutorPianoFormativoVM> tutorvw = new List<TutorPianoFormativoVM>();
            ViewBag.IdPersona = id;
            tutorvw = pianoFormativoServizio.GetListaTutorById(id);
            return PartialView("_subpartial/SezioneTutor", tutorvw);
        }

        #endregion
        public ActionResult GetTutor(string filter, string nominativo, string matricola = null, int idPersona = 0, bool isSearch = false)
        {
            List<RicercaPianoFormativo> listaTutor = new List<RicercaPianoFormativo>();
            List<TutorPianoFormativoVM> listaTutor_ = new List<TutorPianoFormativoVM>();
            RicercaPianoFormativo vuoto = new RicercaPianoFormativo();
            vuoto.IdPersona = 0;
            vuoto.Nominativo = "";
            listaTutor.Add(vuoto);

            try
            {
                if (!isSearch)
                {
                    listaTutor.AddRange(pianoFormativoServizio.GetTutor(filter.ToUpper()));
                }
                else
                {
                    listaTutor_ = pianoFormativoServizio.GetTutor(matricola, nominativo.ToUpper());
                    ViewBag.IdPersona = idPersona;

                    return PartialView("_subpartial/TableBodyResultSerchTutorSelected", listaTutor_);
                }

            }
            catch (Exception ex)
            {

            }
            return Json(listaTutor.Select(x => new SelectListItem { Value = x.IdPersona.ToString(), Text = x.Nominativo }), JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetSezioni(string filter)
        {
            List<RicercaPianoFormativo> listaSezioni;
            try
            {
                listaSezioni = pianoFormativoServizio.GetSezioni(filter.ToUpper());

            }
            catch (Exception ex)
            {
                listaSezioni = new List<RicercaPianoFormativo>();
            }
            return Json(listaSezioni.Select(x => new SelectListItem { Value = x.codSezione, Text = x.SelectedSezione }), JsonRequestBehavior.AllowGet);
        }
        public ActionResult PianificaNuovoPianoFormativo(int id)
        {

            DatiApprendistato viewmodel = new DatiApprendistato();
            try
            {
                //  XR_IMM_IMMATRICOLAZIONI immatricolazione = new XR_IMM_IMMATRICOLAZIONI();
                var imma = dbContext.XR_IMM_IMMATRICOLAZIONI.FirstOrDefault(x => x.ID_PERSONA == id);
                // immatricolazione =  immatricolazioneRepository.GetById(id);
                viewmodel.Matricola = imma.COD_MATDIP;
                gestioneDatiDaVisualizzare.GetDescrizioniByEntityData(imma);
                viewmodel.IdPersona = imma.ID_PERSONA.Value;
                viewmodel.TipologiaApprendistati = pianoFormativoServizio.GetTipologieApprendistati();
                ViewBag.IdPersona = imma.ID_PERSONA.Value;
            }
            catch (Exception ex)
            {

                viewmodel = new DatiApprendistato();
            }
            return PartialView("_subpartial/PianificaNuovoPianoFormativo", viewmodel);
        }
        public ActionResult ModificaPianoFormativo(int id)
        {

            DatiApprendistato viewmodel = new DatiApprendistato();
            try
            {

                viewmodel = pianoFormativoServizio.GetDatiApprendistato(id);
                var imma = dbContext.XR_IMM_IMMATRICOLAZIONI.FirstOrDefault(x => x.ID_PERSONA == id);
                //   viewmodel.Matricola = imma ==null ?"0": imma.COD_MATDIP;
                viewmodel.Matricola = dbContext.JNAGPERS.FirstOrDefault(x => x.ID_PERSONA == id).COD_MATDIP;
                viewmodel.IdPersona = id;
                ViewBag.IdPersona = id;
                ViewBag.NuovaImm = imma == null ? false : true;

            }
            catch (Exception ex)
            {

                viewmodel = new DatiApprendistato();
            }
            return PartialView("_subpartial/modificaPianoFormativo", viewmodel);
        }
        [HttpGet]
        public ActionResult DettaglioPianoFormativo(int id)
        {
            DatiApprendistato viewmodel = new DatiApprendistato();
            try
            {
                viewmodel = pianoFormativoServizio.GetDatiApprendistato(id);
                XR_IMM_IMMATRICOLAZIONI imma = dbContext.XR_IMM_IMMATRICOLAZIONI.FirstOrDefault(x => x.ID_PERSONA == id);
                if (imma != null)
                {
                    viewmodel.Matricola = imma.COD_MATDIP;
                }
                else viewmodel.Matricola = "";
                viewmodel.IdPersona = id;
                ViewBag.IdPersona = id;
                ViewBag.NuovaImm = imma == null ? false : true;
            }
            catch (Exception ex)
            {

                viewmodel = new DatiApprendistato();
            }
            return PartialView("_subpartial/DettaglioPianoFormativo", viewmodel);
        }
        #region  MODALI(insert/edit)
        public ActionResult ModalInserimentoTutor(int idPersona)
        {
            ViewBag.IdPersona = idPersona;
            return PartialView("_subpartial/Modal_FormTutor");
        }
        public ActionResult ModalDettaglioTutor(TutorPianoFormativoVM tutor)
        {

            TutorPianoFormativoVM viewmodel = new TutorPianoFormativoVM();
            viewmodel.IsNew = true;
            var DalStr = tutor.DalStr.Split('/');
            DateTime dal = new DateTime(int.Parse(DalStr[2]), int.Parse(DalStr[1]), int.Parse(DalStr[0]));
            var AlStr = tutor.AlStr.Split('/');
            DateTime Al = new DateTime(int.Parse(AlStr[2]), int.Parse(AlStr[1]), int.Parse(AlStr[0]));
            viewmodel.Al = Al;
            viewmodel.AlStr = tutor.AlStr;
            viewmodel.Dal = dal;
            viewmodel.DalStr = tutor.DalStr;
            viewmodel.Categoria = tutor.Categoria;
            viewmodel.Nominativo = tutor.Nominativo;
            viewmodel.MatricolaTutor = tutor.MatricolaTutor;
            viewmodel.Nota = tutor.Nota;

            return PartialView("_subpartial/FormDettaglioTutorSelezionato", viewmodel);

        }
        public ActionResult ModalInserimentoTitoloDiStudio(int idPersona)
        {
            StudioModel newTitoloDiStudio = new StudioModel();
            ViewBag.IdPersona = idPersona;


            return PartialView("~/Views/Anagrafica/subpartial/Form_TitoliStudio.cshtml", newTitoloDiStudio);
        }
        public ActionResult ModalModificaTitoloDiStudio(StudioModel titoloDiStudio)
        {

            ViewBag.IdPersona = titoloDiStudio.IdPersona;
            return PartialView("~/Views/Anagrafica/subpartial/Form_TitoliStudio.cshtml", titoloDiStudio);
        }
        public ActionResult ModalDettaglioTitoloDiStudio(StudioModel modelEntity)
        {
            ViewBag.IdPersona = modelEntity.IdPersona;
            return PartialView("~/Views/Anagrafica/subpartial/Form_TitoliStudio.cshtml", modelEntity);
        }

        public ActionResult ModalInserimentoEsperienzaLavorativa(int idPersona)
        {
            EsperienzeLavorativeViewModel viewmodel = new EsperienzeLavorativeViewModel();
            viewmodel.IdPersona = idPersona;
            return PartialView("_subpartial/Modal_FormEsperienzeLavorative", viewmodel);
        }
        public ActionResult ModalDettaglioEsperienzaLavorativa(EsperienzeLavorativeViewModel modelEntity)
        {

            return PartialView("_subpartial/Modal_FormEsperienzeLavorative", modelEntity);
        }
        #endregion
        [HttpPost]
        public ActionResult GetDescrizioneTitoliDiStudio(int codTipoTitolo, string codTitolo, int idPersona)
        {
            StudioModel datiApprentistaVM = new StudioModel();
            object result = "";
            try
            {

                datiApprentistaVM.IdPersona = idPersona;
                datiApprentistaVM.DesTipoTitolo = pianoFormativoServizio.GetTipologiaTitoloDiStudio(codTipoTitolo);
                datiApprentistaVM.DesTitolo = pianoFormativoServizio.GetTitoliDiStudio(codTitolo);
                ViewBag.DatiTitoliDiStudio = datiApprentistaVM;
                result = new { Data = datiApprentistaVM, Esito = true };
            }
            catch (Exception ec)
            {
                datiApprentistaVM = new StudioModel();
                result = new { Data = datiApprentistaVM, Esito = false };

            }



            return Json(new { Data = result, JsonRequestBehavior.AllowGet });
        }
        public static List<SelectListItem> GetProfiliFormativi(string filter)
        {
            List<SelectListItem> items = new List<SelectListItem>();


            var tmp = new PianoFormativoServizio(new IncentiviEntities());

            switch (filter)
            {
                case "profiliFormativi":
                    items.AddRange(tmp.GetProfiliFormativi(filter).Select(x => new SelectListItem { Value = x.CodiceRuolo, Text = x.ProfiloFormativo }));
                    break;
                default:
                    break;
            }

            return items;
        }
        public ActionResult FormDettaglioTutorSelected(string tutorSelected, int idPersona)
        {
            TutorPianoFormativoVM viewmodel = new TutorPianoFormativoVM();
            ViewBag.IdPersona = idPersona;
            viewmodel = pianoFormativoServizio.GetTutorByMatricola(tutorSelected);
            viewmodel.Oid = dbContext.XR_TUTOR.GeneraPrimaryKey();
            var imm = dbContext.XR_IMM_IMMATRICOLAZIONI.Where(x => x.ID_PERSONA == idPersona).OrderBy(x => x.TMS_TIMESTAMP).FirstOrDefault();
            if (imm == null)
            {
                var com = dbContext.COMPREL.Where(x => x.ID_PERSONA == idPersona).OrderBy(x => x.TMS_TIMESTAMP).FirstOrDefault();
                viewmodel.Dal = com.DTA_HIRE;
                viewmodel.DalStr = com.DTA_HIRE.ToString();
                viewmodel.AlStr = com.DTA_FINE.ToString();
                viewmodel.Al = com.DTA_FINE;
            }
            else
            {
                viewmodel.Dal = imm.DTA_INIZIO;
                viewmodel.DalStr = imm.DTA_INIZIO.ToString();
                viewmodel.AlStr = imm.DTA_FINE.ToString();
                viewmodel.Al = imm.DTA_FINE;
            }

            if (dbContext.XR_TUTOR.Any(x=>x.ID_PERSONA==idPersona))
            {
                viewmodel.Dal = DateTime.Today;
                viewmodel.DalStr = DateTime.Today.ToString();
            }

            return PartialView("_subpartial/FormDettaglioTutorSelezionato", viewmodel);
        }
        public ActionResult SalvaPianoFormativo(NuovoPianoFormativo entityArrays)
        {
            bool esito;
            if (!entityArrays.Modifica)
            {
                esito = pianoFormativoServizio.InserimentoNuovoPianoFormativo(entityArrays);
            }
            else
            {
                esito = pianoFormativoServizio.ModificaPianoFormativo(entityArrays);
            }
            if (esito)

                return Json(new { Data = "OK" });
            else

                return Json(new { Data = "Operazione non riuscita" });
        }

        public ActionResult GeneraWordPianoFormativoIndividuale(int idPersona)
        {

            DocX doc = null;
            var anagrafica = dbContext.ANAGPERS.Where(x => x.ID_PERSONA == idPersona).SingleOrDefault();
            if (anagrafica != null)
            {
                doc = pianoFormativoServizio.HandleData(idPersona);
            }
            MemoryStream st = new MemoryStream();
            doc.SaveAs(st);
            st.Position = 0;
            string nomeFile = "PFI " + anagrafica.DES_COGNOMEPERS + " " + anagrafica.DES_NOMEPERS + ".docx";
            return File(st, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", nomeFile);
        }
        public ActionResult EliminaPiano(int idPersona, int idJobAssign)
        {
            string result = "";
            try
            {
                if (idPersona != 0)
                {
                    if (!pianoFormativoServizio.Elimina(idPersona, idJobAssign))
                        result = "Si è verificato un errore durante la cancellazione";
                    else
                        result = "OK";
                }
                string parametri = "Cancellazione Piano formativo (persona:" + idPersona.ToString() + "), esito: " + result;
                HrisHelper.LogOperazione("PFCancellazione", parametri, true);
            }
            catch (Exception ex)
            {
                HrisHelper.LogOperazione("PFCancellazione", "Cancellazione Piano formativo (persona:" + idPersona.ToString() + ")", false, null, null, ex);
                return Content(ex.Message);
                throw;
            }
            return Content(result);
        }

    }
}
