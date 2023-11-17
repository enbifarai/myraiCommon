using System;
using System.Linq;
using System.Web.Mvc;
using myRaiCommonManager;
using myRaiCommonModel.DocFirmaModels;
using myRaiHelper;
using MyRaiServiceInterface.it.rai.servizi.digigappws;

namespace myRai.Controllers
{
    public class DocumentiDaFirmareController : BaseCommonController
    {
        Boolean monitor = true;
        public ActionResult Index()
        {
            DocumentiDaFirmareModel model = new DocumentiDaFirmareModel();
            model.Sedi = DaFirmareManager.GetSediDaFirmare();
            if (model.Sedi != null && model.Sedi.Count() > 0) model.Sedi[0].Selezionata = true;

            model.Mese = DaFirmareManager.GetMeseAnno(model.Sedi[1].CodiceSede, DateTime.Now.Month, DateTime.Now.Year);
            model.Giornate = DaFirmareManager.GetGiornateModel(model.Sedi[1].CodiceSede, DateTime.Now.Month, DateTime.Now.Year);
            return View(model);
        }

        public ActionResult GetMeseAjax(string cod, int anno, int mese)
        {
            MeseAnnoModel model = DaFirmareManager.GetMeseAnno(cod, mese, anno);
            return View("_menumese", model);
        }

        public ActionResult GetGiornateAjax(string cod, int anno, int mese)
        {
            GiornateModel model = DaFirmareManager.GetGiornateModel(cod, mese, anno);
            return View("_giornate", model);
        }
        public ActionResult GetPdf(string datainizio, string datafine, string codsede)
        { 
            WSDigigapp datiBack = new WSDigigapp();
            datiBack.Credentials =new System.Net.NetworkCredential( CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[0], CommonHelper.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio)[1]  );
           
            var response = datiBack.getRiepilogo( CommonHelper.GetCurrentUserMatricola(), UtenteHelper.Nominativo(), datainizio, datafine, codsede, 75);
            if (!String.IsNullOrWhiteSpace(response.errore))
            {
                if (response.raw == null) return Content("*" + response.errore);
                else return Content("*" + response.errore + "-" + response.raw);
            }
            if (response == null) return Content("* Risposta null dal servizio");

            byte[] b = response.PDF;
            if (b == null || b.Length == 0) return Content("* PDF nullo o vuoto");

            string s = Convert.ToBase64String(b);
            PDFmodel model = new PDFmodel() { 
                datainizio=datainizio , 
                datafine=datafine,
                codSede=codsede,
                PDFbase64=s };
            
            return View("_pdfviewer", model);
        }
       
        public ActionResult GetEccezioniGiornata(string giorno,string codsede,string eticsede)
        {
            EccezioniGiornataModel model = DaFirmareManager.getEccezioniGiornataModel(giorno,codsede,eticsede);

            return View("_eccezioniGiornata",model);
        }
    }
}