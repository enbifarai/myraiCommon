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
    public class BustaPagaController : Controller
    {
        public ActionResult Index ( bool notifiche = false )
        {
            return GetBustaPagaUltime( notifiche );
        }

        public ActionResult GetBustaPagaUltime ( bool notifiche )
        {
            HrPaga hrPaga = new HrPaga( );
            hrPaga.Credentials = System.Net.CredentialCache.DefaultCredentials;
            ListaDocumenti result = hrPaga.ElencoDocumentiPersonali( "00" , String.Empty , String.Empty );

            BustaPagaModel model = DocumentiPersonaliManager.GetBustaPagaModel( true , false , result );
            model.menuSidebar = UtenteHelper.getSidebarModel();// new sidebarModel(3);
            model.flagUltimoAnno = true;
            model.descrizioneTipo = "";
            model.daleggere = result.ListaDatiDocumenti.Where( g => g.FlagLetto == 2 ).Count( );
            model.nonLetti = notifiche;
            if ( model.daleggere == 1 )
            {
                model.ultimabusta = result.ListaDatiDocumenti.Where( g => g.FlagLetto == 2 ).FirstOrDefault( ).DataContabile;
                model.ultimabusta = CommonHelper.TraduciMeseDaNumLett( model.ultimabusta.Substring( 4 , 2 ) ) + " " + model.ultimabusta.Substring( 0 , 4 );
            }


            // model.daleggere = 2;
            return View( model );
        }

        public ActionResult GetBustaPaga ( bool UltimoAnno , bool notifiche = false )
        {
            BustaPagaModel model;

            HrPaga hrPaga = new HrPaga( );
            hrPaga.Credentials = System.Net.CredentialCache.DefaultCredentials;
            ListaDocumenti result = hrPaga.ElencoDocumentiPersonali( "00" , String.Empty , String.Empty );

            model = DocumentiPersonaliManager.GetBustaPagaModel( UltimoAnno , false , result );
            model.flagUltimoAnno = UltimoAnno;
            model.descrizioneTipo = "";
            model.daleggere = result.ListaDatiDocumenti.Where( g => g.FlagLetto == 2 ).Count( );

            if ( notifiche )
            {
                model = DocumentiPersonaliManager.GetDocumentiNonLettiModel( result );
                model.descrizioneTipo = "non letti";
            }

            return View( "~/Views/BustaPaga/subpartial/elenco.cshtml" , model );
        }

        public ActionResult GetDocDaLeggere ( )
        {
            BustaPagaModel model;
            HrPaga hrPaga = new HrPaga( );
            hrPaga.Credentials = System.Net.CredentialCache.DefaultCredentials;
            ListaDocumenti result = hrPaga.ElencoDocumentiPersonali( "00" , String.Empty , String.Empty );
            ListaNavBar lista = hrPaga.ElencoTipoDocumento( );

            model = DocumentiPersonaliManager.GetDocumentiNonLettiModel( result );
            model.descrizioneTipo = "non letti";
            return View( "~/Views/BustaPaga/subpartial/elenco.cshtml" , model );
        }

        public ActionResult GetPdf ( string idPdf , string datacompetenza , string datacontabile , string datapubblicazione , string nota , string nomefile )
        {
            PDFmodel model = new PDFmodel( )
            {
                datacompetenza = datacompetenza ,
                datacontabile = datacontabile ,
                datapubblicazione = datapubblicazione ,
                nota = nota ,
                idDocumento = idPdf ,
                nomefile = nomefile
            };

            return View( "~/Views/BustaPaga/subpartial/_pdfviewer.cshtml" , model );
        }

        public ActionResult GetPdfBinary ( string idDocumento , string nomefile )
        {
            it.rai.servizi.hrpaga.HrPaga hrPaga = new it.rai.servizi.hrpaga.HrPaga( );
            hrPaga.Credentials = System.Net.CredentialCache.DefaultCredentials;
            it.rai.servizi.hrpaga.Documento result = hrPaga.ByteDoc( idDocumento , String.Empty , String.Empty );
            byte[] byteArray = result.FileDoc;
            MemoryStream pdfStream = new MemoryStream( );
            pdfStream.Write( byteArray , 0 , byteArray.Length );
            pdfStream.Position = 0;

            /* Response.AppendHeader("content-disposition", "inline; file" + nomefile + ".pdf");
             Response.AppendHeader("content-type", "application/pdf");
             Response.AppendHeader("Expires", DateTime.Now.AddMinutes(5.0).ToString());
             Response.AppendHeader("Cache-Control", "no-cache, must-revalidate");

             Response.AppendHeader("Pragma", "no-cache");
             Response.Expires = -1;
             Response.AddHeader("content-length", byteArray.Length.ToString());

             Response.AddHeader("Content-Disposition", "inline; filename=" + nomefile + ".pdf");
             /*  Response.AppendHeader("content-type", "application/pdf");
               Response.AppendHeader("Expires", DateTime.Now.AddMinutes(5.0).ToString());
               Response.AppendHeader("Cache-Control", "no-cache, must-revalidate");

               Response.AppendHeader("Pragma", "no-cache");
               Response.Expires = -1;
               Response.AddHeader("content-length", byteArray.Length.ToString());*/


            Response.AddHeader( "Content-Disposition" , "inline; filename=" + nomefile + ".pdf" );
            //return new FileStreamResult(pdfStream, "application/pdf");
            return File( byteArray , "application/pdf" );

            //  return new FileStreamResult(pdfStream, "application/pdf");
        }
    }
}