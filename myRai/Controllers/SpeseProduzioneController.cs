using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using myRaiData;
using myRaiCommonModel;
using myRaiCommonModel.Pagination;
using myRaiCommonModel.Services;
using System.Web;

namespace myRai.Controllers
{

    public class SpeseProduzioneController : Controller  
    {
        private readonly SpeseDiProduzioneEntities dbContext = new SpeseDiProduzioneEntities();
        private SpeseDiProduzioneServizio speseDiProduzioneServizio;
        List<SpeseProduzioneViewModel> result = new List<SpeseProduzioneViewModel>();
        public SpeseProduzioneController()
        {
            speseDiProduzioneServizio = new SpeseDiProduzioneServizio(dbContext);
        }
        public ActionResult Index()
        {
            ViewBag.ListaStati = speseDiProduzioneServizio.GetListaStati().ToList();
            return View();
        }
        public ActionResult LoadTableSpese(int? page, string stato,string idFSP, DateTime? dataal, DateTime? datadal, bool isAperte = false)
        {
            PaginatedList<SpeseProduzioneViewModel> listaSpese;
            try
            {
                listaSpese =PaginatedList<SpeseProduzioneViewModel>.CratePaginatedList(speseDiProduzioneServizio.GetSpeseProduzione(isAperte, dataal, datadal, stato, idFSP),page.Value);
                ViewBag.IsAperte = isAperte;
            }
            catch (Exception ex)
            {
                listaSpese = new PaginatedList<SpeseProduzioneViewModel>();
            }
            return PartialView("TBodySpeseProduzione", listaSpese);
        }
        public ActionResult GetDettaglioFoglioSpese(decimal id_FoglioSpese, bool isAperte)
        {
            List<SpeseProduzioneViewModel> viewmodel = null;
            try
            {
                viewmodel = speseDiProduzioneServizio.GetDettaglioFoglioSpese(id_FoglioSpese, isAperte).ToList();
                foreach (var item in viewmodel)
                {
                    TempData["TipoTarghetta"] = item.TipoTarghetta;
                }
            }
            catch (Exception ex)
            {
                viewmodel = new List<SpeseProduzioneViewModel>();

            }
            return PartialView("GetDettaglioFoglioSpese", viewmodel);
        }
        public ActionResult GetDescrizioneModalDettaglio(decimal id_FoglioSpese)
        {
            SpeseProduzioneViewModel viewmodel = new SpeseProduzioneViewModel();
            try
            {
                ViewBag.DescrizioniAndImportiAnticipi = speseDiProduzioneServizio.GetDesctizioniAndImportiAnticipi(id_FoglioSpese);
                ViewBag.VociRendicontiSegreteria = speseDiProduzioneServizio.GetDescrizioneFromTFSP02ConTipoTaghettaSegreteria(id_FoglioSpese);
                ViewBag.VociRendicontiDipendente = speseDiProduzioneServizio.GetDescrizioneFromTFSP02ConTipoTaghettaDipendente(id_FoglioSpese);
                ViewBag.VociRendicontiPersonale = speseDiProduzioneServizio.GetDescrizioneFromTFSP02ConTipoTaghettaPersonale(id_FoglioSpese);
                ViewBag.VociRendicontiContabilita = speseDiProduzioneServizio.GetDescrizioneFromTFSP02ConTipoTaghettaContabilita(id_FoglioSpese);
                ViewBag.TarghettaAndImportiRendicontiSegreteria = speseDiProduzioneServizio.GetDescrizioniAndImportiRendicontiConTarghettaInsegreteria(id_FoglioSpese);
                ViewBag.TarghettaAndImportiRendicontiDipendente = speseDiProduzioneServizio.GetDescrizioniAndImportiRendicontiConTarghettaAlDipendente(id_FoglioSpese);
                ViewBag.TarghettaAndImportiRendicontiContabilita = speseDiProduzioneServizio.GetDescrizioniAndImportiRendicontiConTarghettaInContabilita(id_FoglioSpese);
                ViewBag.TarghettaAndImportiRendicontiPersonale = speseDiProduzioneServizio.GetDescrizioniAndImportiRendicontiConTarghettaAlPersonale(id_FoglioSpese);
                viewmodel.TipoTarghetta = TempData["TipoTarghetta"].ToString();
            }
            catch (Exception)
                {
                    

            }
            return PartialView("GetDescrizioneModalDettaglio", viewmodel);
        }


        public ActionResult GetRiepilogoImportoMeseCorrente()
        {
            SpeseProduzioneViewModel viewmodel = new SpeseProduzioneViewModel();
            try
            {
                viewmodel.sommaImportiPerMeseCorrente = speseDiProduzioneServizio.GetRiepilogoImportoMeseCorrente().ToList().Sum(s => (double)s.MP_Importo);
                ViewBag.SommaImporti = viewmodel.sommaImportiPerMeseCorrente;
            }
            catch (Exception ex)
            {

                throw ex;
            }
            return PartialView("GetRiepilogoImportoMeseCorrente", viewmodel);
        }

        public ActionResult AggiungiDocumento()
        {

            return PartialView("_AggiuntaDocumentoSpeseProduzione");
        }

        public ActionResult SaveDocumentoSpesaProduzione(string nomeDocumento, HttpPostedFileBase fileUpload1)
        {

            return Content("OK");
        }




    }
}
