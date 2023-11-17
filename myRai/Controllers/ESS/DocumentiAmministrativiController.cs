using System;
using System.Linq;
using System.Web.Mvc;
using System.IO;
using myRaiCommonModel.ess;
using myRaiCommonManager;
using myRaiHelper;
using myRaiCommonModel.DocFirmaModels;
using myRaiServiceHub.it.rai.servizi.hrpaga;

namespace myRai.Controllers.ess
{
    public class DocumentiAmministrativiController : Controller
    {
        public ActionResult Index(bool notifiche = false)
        {
            // Assume that IsInitialized  
            // returns either true or false.  
            return  GetDocUltimi(notifiche);
        }

        public ActionResult GetDocUltimi(bool notifiche)
        {
            HrPaga hrPaga = new HrPaga();
            hrPaga.Credentials = System.Net.CredentialCache.DefaultCredentials;
            ListaDocumenti result = hrPaga.ElencoDocumentiPersonali("*", String.Empty, String.Empty);
            BustaPagaModel model;
            model = DocumentiPersonaliManager.GetBustaPagaModel(true, false, result);
            model.nonLetti = false;
           
            model.menuSidebar = UtenteHelper.getSidebarModel();// new sidebarModel(3);
            model.flagUltimoAnno = true;
            model.daleggere = result.ListaDatiDocumenti.Where(g => ((g.FlagLetto == 2)&&(g.ID.Length > 0))).Count();
            model.nonLetti = notifiche;
            if (model.daleggere == 1)
            {
                model.ultimabusta = result.ListaDatiDocumenti.Where(g => g.FlagLetto == 2).FirstOrDefault().DataContabile;
                model.ultimabusta = CommonHelper.TraduciMeseDaNumLett(model.ultimabusta.Substring(4, 2)) + " " + model.ultimabusta.Substring(0, 4);
            }

            var query = result.ListaDatiDocumenti.SelectMany(x => x.DescrittivaTipoDoc)
                .GroupBy(f => f)
                .Select(g => new { Name = g.Key, Count = g.Count() });

            foreach (var result2 in query)
            {
                var f = result2;
                Console.WriteLine("Name: {0}, Count: {1}", result2.Name, result2.Count);
            }

            return View(model);
        }

        public ActionResult GetBustaPaga(bool UltimoAnno, bool notifiche = false)
        {
            BustaPagaModel model;

            HrPaga hrPaga = new HrPaga( );
            hrPaga.Credentials = System.Net.CredentialCache.DefaultCredentials;
            ListaDocumenti result = hrPaga.ElencoDocumentiPersonali( "*" , String.Empty , String.Empty );

            model = DocumentiPersonaliManager.GetBustaPagaModel(UltimoAnno,false, result);
            model.descrizioneTipo = "";
            model.flagUltimoAnno = UltimoAnno;
            model.daleggere = result.ListaDatiDocumenti.Where(g => g.FlagLetto == 2).Count();
            if (notifiche)
            {
                model = DocumentiPersonaliManager.GetDocumentiNonLettiModel(result);
                model.descrizioneTipo = "non letti";
            }
            //  model.daleggere = 2;
            return View("~/Views/DocumentiAmministrativi/subpartial/elenco.cshtml", model);
        }


        public ActionResult GetDocPerTipo(string tipoDoc)
        {
            BustaPagaModel model;

            HrPaga hrPaga = new HrPaga( );
            hrPaga.Credentials = System.Net.CredentialCache.DefaultCredentials;
            ListaDocumenti result = hrPaga.ElencoDocumentiPersonali( tipoDoc , String.Empty , String.Empty );
            ListaNavBar lista = hrPaga.ElencoTipoDocumento( );

            model = DocumentiPersonaliManager.GetDocumentiperTipoModel(tipoDoc, result);
            model.descrizioneTipo = " di tipo " + lista.Elementi.Where(a => a.Split('|')[0] == tipoDoc).FirstOrDefault().Split('|')[1];
            //  model.daleggere = 2;
            return View("~/Views/DocumentiAmministrativi/subpartial/elenco.cshtml", model);
        }

        public ActionResult GetDocDaLeggere()
        {
            BustaPagaModel model;

            HrPaga hrPaga = new HrPaga( );
            hrPaga.Credentials = System.Net.CredentialCache.DefaultCredentials;
            ListaDocumenti result = hrPaga.ElencoDocumentiPersonali( "*" , String.Empty , String.Empty );
            ListaNavBar lista = hrPaga.ElencoTipoDocumento( );

            model = DocumentiPersonaliManager.GetDocumentiNonLettiModel(result);
            model.descrizioneTipo = "non letti";
            //  model.daleggere = 2;
            return View("~/Views/DocumentiAmministrativi/subpartial/elenco.cshtml", model);
        }

        public ActionResult GetPdf ( string idPdf , string datacompetenza , string datacontabile , string datapubblicazione , string nota , string titolo , string nomefile )
        {
            PDFmodel model = new PDFmodel( )
            {
                titolo = titolo ,
                datacompetenza = datacompetenza ,
                datacontabile = datacontabile ,
                datapubblicazione = datapubblicazione ,
                nota = nota ,
                idDocumento = idPdf ,
                nomefile = nomefile
            };

            return View( "~/Views/DocumentiAmministrativi/subpartial/_pdfviewer.cshtml" , model );
        }

        public ActionResult GetPdfBinary(string idDocumento, string nomefile)
        {
            it.rai.servizi.hrpaga.HrPaga hrPaga = new it.rai.servizi.hrpaga.HrPaga();
            hrPaga.Credentials = System.Net.CredentialCache.DefaultCredentials;
            it.rai.servizi.hrpaga.Documento result = hrPaga.ByteDoc(idDocumento, String.Empty, String.Empty);

            byte[] byteArray = result.FileDoc;

            MemoryStream pdfStream = new MemoryStream();
            pdfStream.Write(byteArray, 0, byteArray.Length);
            pdfStream.Position = 0;

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "documento",
                Inline = true,
            };

            Response.AddHeader("Content-Disposition", "inline; filename=" + nomefile + ".pdf");
            return File(byteArray, "application/pdf");
        }
    }
}