using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using ClosedXML.Excel;
using myRai.DataAccess;
using myRaiData;
using iTextSharp.text.pdf;
using myRaiHelper;
using myRaiCommonModel;
using myRaiCommonManager;
using myRaiServiceHub.it.rai.servizi.sendmail;
using WebGrease.Css.Extensions;
using myRai.Business;
using myRai.Models;

namespace myRai.Controllers
{
    public class EventsController : BaseCommonController
    {
        public string miamatricola = CommonHelper.GetCurrentUserMatricola( );

        public static bool noSession = false;

        protected override void OnActionExecuting ( ActionExecutingContext filterContext )
        {
            if ( filterContext.ActionDescriptor.ActionName.ToUpper( ) == "INDEX" )
            {
                noSession = !String.IsNullOrWhiteSpace( Request["backurl"] );
            }
            else if ( filterContext.ActionDescriptor.ActionName.ToUpper( ) == "GETPDF" )
            {
                //in questo caso non è necessario controllare la session
                return;
            }

            if ( !noSession )
                base.OnActionExecuting( filterContext );
        }

        public static string GetIdPrenotazione ( B2RaiPlace_Eventi_Anagrafica evento )
        {
            return evento.B2RaiPlace_Eventi_Evento.id + evento.matricola;
        }

        public ActionResult Index ( string backurl , string id )
        {
            EventsClientModel model = EventsManager.InitModel( backurl , id );
            return View( model );
        }

        public ActionResult listaEventi ( )
        {
            EventsClientModel model = EventsManager.getModel( );
            return View( model );
        }

        /// <summary>
        /// Visualizzazione della pagina con la lista degli eventi per il quale l'utente è amministratore
        /// </summary>
        /// <returns></returns>
        public PartialViewResult EventiPrenotati ( )
        {
            EventiPrenotatiVM model = new EventiPrenotatiVM( );

            model.Eventi = EventsManager.GetEventiPrenotati( );

            return PartialView( model );
        }

        /// <summary>
        /// Download dei dati in formato excel
        /// </summary>
        /// <param name="idEvento"></param>
        /// <returns></returns>
        public ActionResult EsportaEventiPrenotati ( int idEvento )
        {
            try
            {
                digiGappEntities db = new digiGappEntities( );
                var evento = db.B2RaiPlace_Eventi_Evento.FirstOrDefault( x => x.id == idEvento );

                if ( evento != null ) //eventi != null && eventi.Any())
                {
                    List<Prenotati> eventi = EventsManager.GetPrenotati( idEvento );

                    XLWorkbook workbook = new XLWorkbook( );
                    string titolo = evento.titolo.Replace( "\\" , "" )
                                                .Replace( "/" , "" )
                                                .Replace( "*" , "" )
                                                .Replace( "?" , "" )
                                                .Replace( ":" , "" )
                                                .Replace( "[" , "" )
                                                .Replace( "]" , "" );

                    var worksheet = workbook.Worksheets.Add( titolo.Length > 30 ? titolo.Substring( 0 , 27 ) + ".." : titolo );

                    int offset = 0;

                    int row = 1;
                    if ( evento.ticket.GetValueOrDefault( ) )
                    {
                        worksheet.Cell( row , 1 ).Value = "BADGE";
                        offset = 1;
                    }
                    worksheet.Cell( row , 1 + offset ).Value = "ID";
                    worksheet.Cell( row , 2 + offset ).Value = "DATA EVENTO";
                    worksheet.Cell( row , 3 + offset ).Value = "MATRICOLA";
                    worksheet.Cell( row , 4 + offset ).Value = "DIPENDENTE";
                    worksheet.Cell( row , 5 + offset ).Value = "TELEFONO";
                    worksheet.Cell( row , 6 + offset ).Value = "NOME";
                    worksheet.Cell( row , 7 + offset ).Value = "COGNOME";
                    worksheet.Cell( row , 8 + offset ).Value = "GENERE";
                    worksheet.Cell( row , 9 + offset ).Value = "DATA DI NASCITA";
                    worksheet.Cell( row , 10 + offset ).Value = "CITTA' DI NASCITA";
                    worksheet.Cell( row , 11 + offset ).Value = "CESPITE";
                    worksheet.Cell( row , 12 + offset ).Value = "GRADO DI PARENTELA";
                    worksheet.Cell( row , 13 + offset ).Value = "MAIL";
                    worksheet.Cell( row , 14 + offset ).Value = "SEDE INSEDIAMENTO";
                    worksheet.Cell(row, 15 + offset).Value = "NOTA";
                    worksheet.Row( row ).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Row( row ).Style.Font.Bold = true;
                    WriteRows( evento , eventi , worksheet , ref offset , ref row );
                    worksheet.Columns( ).AdjustToContents( );

                    MemoryStream ms = new MemoryStream( );
                    workbook.SaveAs( ms );
                    ms.Position = 0;

                    return new FileStreamResult( ms , "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" ) { FileDownloadName = "ExportPrenotazioni.xlsx" };
                }
                else
                {
                    throw new Exception( "Si è verificato un errore nel reperimento dei dati dell'evento richiesto" );
                }
            }
            catch ( Exception ex )
            {
                return Content( ex.Message );
            }
        }

        private static void WriteRows ( B2RaiPlace_Eventi_Evento evento , List<Prenotati> eventi , IXLWorksheet worksheet , ref int offset , ref int row )
        {
            foreach ( var item in eventi )
            {
                row++;

                if ( evento.ticket.GetValueOrDefault( ) )
                {
                    worksheet.Cell( row , 1 ).Value = evento.id + item.Matricola;
                    offset = 1;
                }
                worksheet.Cell( row , 1 + offset ).Value = item.Id.ToString( );
                worksheet.Cell( row , 2 + offset ).Value = item.DataEvento.ToString( "dd/MM/yyyy" );
                worksheet.Cell( row , 3 + offset ).Value = item.Matricola;
                worksheet.Cell( row , 4 + offset ).Value = item.Dipendente;
                worksheet.Cell( row , 5 + offset ).SetValue<string>( item.Telefono );
                worksheet.Cell( row , 6 + offset ).Value = item.Nome;
                worksheet.Cell( row , 7 + offset ).Value = item.Cognome;
                worksheet.Cell( row , 8 + offset ).Value = item.Genere;
                worksheet.Cell( row , 9 + offset ).Value = item.DataNascita.ToString( "dd/MM/yyyy" );
                worksheet.Cell( row , 10 + offset ).Value = item.Citta;
                worksheet.Cell( row , 11 + offset ).Value = item.Luogo;
                worksheet.Cell( row , 12 + offset ).Value = item.Grado;
                worksheet.Cell( row , 13 + offset ).Value = item.Mail;
                worksheet.Cell( row , 14 + offset ).Value = item.SedeInsediamento;
                worksheet.Cell(row, 15 + offset).Value = item.Nota;
            }
        }

        public ActionResult EsportaEventi ( string ids )
        {
            try
            {
                digiGappEntities db = new digiGappEntities( );
                List<int> idsInt = ids.Split( ',' ).Select( x => Convert.ToInt32( x ) ).ToList( );

                var eventi = db.B2RaiPlace_Eventi_Evento.Where( x => idsInt.Contains( x.id ) );

                if ( eventi != null && eventi.Count( ) > 0 ) //eventi != null && eventi.Any())
                {
                    XLWorkbook workbook = new XLWorkbook( );
                    var worksheet = workbook.Worksheets.Add( "Eventi" );
                    int offset = 0;

                    int row = 1;
                    worksheet.Cell( row , 1 ).Value = "BADGE";
                    offset = 1;
                    worksheet.Cell( row , 1 + offset ).Value = "ID";
                    worksheet.Cell( row , 2 + offset ).Value = "DATA EVENTO";
                    worksheet.Cell( row , 3 + offset ).Value = "MATRICOLA";
                    worksheet.Cell( row , 4 + offset ).Value = "DIPENDENTE";
                    worksheet.Cell( row , 5 + offset ).Value = "TELEFONO";
                    worksheet.Cell( row , 6 + offset ).Value = "NOME";
                    worksheet.Cell( row , 7 + offset ).Value = "COGNOME";
                    worksheet.Cell( row , 8 + offset ).Value = "GENERE";
                    worksheet.Cell( row , 9 + offset ).Value = "DATA DI NASCITA";
                    worksheet.Cell( row , 10 + offset ).Value = "CITTA' DI NASCITA";
                    worksheet.Cell( row , 11 + offset ).Value = "CESPITE";
                    worksheet.Cell( row , 12 + offset ).Value = "GRADO DI PARENTELA";
                    worksheet.Cell( row , 13 + offset ).Value = "MAIL";
                    worksheet.Cell( row , 14 + offset ).Value = "SEDE INSEDIAMENTO";
                    worksheet.Cell(row, 15 + offset).Value = "NOTA";
                    worksheet.Row( row ).Style.Fill.BackgroundColor = XLColor.Yellow;
                    worksheet.Row( row ).Style.Font.Bold = true;

                    foreach ( var evento in eventi )
                    {
                        List<Prenotati> prenEvento = EventsManager.GetPrenotati( evento.id );

                        row++;
                        worksheet.Range( worksheet.Cell( row , 1 ) , worksheet.Cell( row , 15 ) ).Merge( );
                        worksheet.Cell( row , 1 ).Value = evento.titolo;
                        worksheet.Cell( row , 1 ).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell( row , 1 ).Style.Fill.BackgroundColor = XLColor.LightGreen;

                        WriteRows( evento , prenEvento , worksheet , ref offset , ref row );
                    }

                    worksheet.Columns( ).AdjustToContents( );

                    MemoryStream ms = new MemoryStream( );
                    workbook.SaveAs( ms );
                    ms.Position = 0;

                    return new FileStreamResult( ms , "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" ) { FileDownloadName = "ExportPrenotazioni.xlsx" };
                }
                else
                {
                    throw new Exception( "Si è verificato un errore nel reperimento dei dati dell'evento richiesto" );
                }
            }
            catch ( Exception ex )
            {
                return Content( ex.Message );
            }
        }


        public ActionResult getTot ( )
        {
            EventsClientModel model = EventsManager.getModel( );
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                Data = new
                {
                    totEventi = model.EventiPrenotati.Count( ) ,
                    totPren = model.EventiPrenotati.Sum( z => z.B2RaiPlace_Eventi_Anagrafica.Where( x => x.confermata == true && x.matricola == CommonHelper.GetCurrentUserMatricola( ) ).Count( ) )
                }
            };
        }

        /// <summary>
        /// Questo metodo si occupa di modificare i pdf template per la stampa dei badge per l'evento bimbo rai
        /// Il primo passo è quello di duplicare i records sulla tabella B2RaiPlace_Eventi_Pdf, 
        /// creando 10 rows una per ogni nprenotazioni da 1 a 10.
        /// Una volta creati i records lanciando questo script verranno sovrascritti i pdf presenti nel db per
        /// l'evento passato. Questa modalità è necessaria in quanto nel pdf c'è la data fissa e quindi
        /// quando cambia la data bisogna caricare i nuovi template.
        /// </summary>
        /// <param name="idEvento"></param>
        /// <returns></returns>
        public ActionResult UpdatePDF ( int idEvento )
        {
            //var db = new myRaiData.digiGappEntities( );

            //var pren = new B2RaiPlace_Eventi_Pdf( );
            //byte[] bytes = System.IO.File.ReadAllBytes( @"C:\tmp\template bimbo rai\badge1.pdf" );
            //pren = db.B2RaiPlace_Eventi_Pdf.Where( a => a.id_evento == idEvento && a.npenotazioni == 1 ).FirstOrDefault( );
            //pren.pdf = bytes;
            //DBHelper.Save( db );

            //bytes = System.IO.File.ReadAllBytes( @"C:\tmp\template bimbo rai\badge2.pdf" );
            //pren = db.B2RaiPlace_Eventi_Pdf.Where( a => a.id_evento == idEvento && a.npenotazioni == 2 ).FirstOrDefault( );
            //pren.pdf = bytes;
            //DBHelper.Save( db );

            //bytes = System.IO.File.ReadAllBytes( @"C:\tmp\template bimbo rai\badge3.pdf" );
            //pren = db.B2RaiPlace_Eventi_Pdf.Where( a => a.id_evento == idEvento && a.npenotazioni == 3 ).FirstOrDefault( );
            //pren.pdf = bytes;
            //DBHelper.Save( db );

            //bytes = System.IO.File.ReadAllBytes( @"C:\tmp\template bimbo rai\badge4.pdf" );
            //pren = db.B2RaiPlace_Eventi_Pdf.Where( a => a.id_evento == idEvento && a.npenotazioni == 4 ).FirstOrDefault( );
            //pren.pdf = bytes;
            //DBHelper.Save( db );

            //bytes = System.IO.File.ReadAllBytes( @"C:\tmp\template bimbo rai\badge5.pdf" );
            //pren = db.B2RaiPlace_Eventi_Pdf.Where( a => a.id_evento == idEvento && a.npenotazioni == 5 ).FirstOrDefault( );
            //pren.pdf = bytes;
            //DBHelper.Save( db );

            //bytes = System.IO.File.ReadAllBytes( @"C:\tmp\template bimbo rai\badge6.pdf" );
            //pren = db.B2RaiPlace_Eventi_Pdf.Where( a => a.id_evento == idEvento && a.npenotazioni == 6 ).FirstOrDefault( );
            //pren.pdf = bytes;
            //DBHelper.Save( db );

            //bytes = System.IO.File.ReadAllBytes( @"C:\tmp\template bimbo rai\badge7.pdf" );
            //pren = db.B2RaiPlace_Eventi_Pdf.Where( a => a.id_evento == idEvento && a.npenotazioni == 7 ).FirstOrDefault( );
            //pren.pdf = bytes;
            //DBHelper.Save( db );

            //bytes = System.IO.File.ReadAllBytes( @"C:\tmp\template bimbo rai\badge8.pdf" );
            //pren = db.B2RaiPlace_Eventi_Pdf.Where( a => a.id_evento == idEvento && a.npenotazioni == 8 ).FirstOrDefault( );
            //pren.pdf = bytes;
            //DBHelper.Save( db );

            //bytes = System.IO.File.ReadAllBytes( @"C:\tmp\template bimbo rai\badge9.pdf" );
            //pren = db.B2RaiPlace_Eventi_Pdf.Where( a => a.id_evento == idEvento && a.npenotazioni == 9 ).FirstOrDefault( );
            //pren.pdf = bytes;
            //DBHelper.Save( db );

            //bytes = System.IO.File.ReadAllBytes( @"C:\tmp\template bimbo rai\badge10.pdf" );
            //pren = db.B2RaiPlace_Eventi_Pdf.Where( a => a.id_evento == idEvento && a.npenotazioni == 10 ).FirstOrDefault( );
            //pren.pdf = bytes;
            //DBHelper.Save( db );

            return null;
        }


        public ActionResult GetPdf ( string idEvento , string matricola , bool p = false )
        {
            var db = new digiGappEntities( );


            //int idE = 466;
            //var pren = new B2RaiPlace_Eventi_Pdf();
            //byte[] bytes = System.IO.File.ReadAllBytes(@"C:\tmp\template bimbo rai\badge1.pdf");
            //pren = db.B2RaiPlace_Eventi_Pdf.Where(a =>a.id_evento==idE && a.npenotazioni == 1).FirstOrDefault();
            //pren.pdf = bytes;
            //bytes = System.IO.File.ReadAllBytes(@"C:\tmp\template bimbo rai\badge2.pdf");
            //pren = db.B2RaiPlace_Eventi_Pdf.Where(a => a.id_evento == idE && a.npenotazioni == 2).FirstOrDefault();
            //pren.pdf = bytes;
            //bytes = System.IO.File.ReadAllBytes(@"C:\tmp\template bimbo rai\badge3.pdf");
            //pren = db.B2RaiPlace_Eventi_Pdf.Where(a => a.id_evento == idE && a.npenotazioni == 3).FirstOrDefault();
            //pren.pdf = bytes;
            //bytes = System.IO.File.ReadAllBytes(@"C:\tmp\template bimbo rai\badge4.pdf");
            //pren = db.B2RaiPlace_Eventi_Pdf.Where(a => a.id_evento == idE && a.npenotazioni == 4).FirstOrDefault();
            //pren.pdf = bytes;
            //bytes = System.IO.File.ReadAllBytes(@"C:\tmp\template bimbo rai\badge5.pdf");
            //pren = db.B2RaiPlace_Eventi_Pdf.Where(a => a.id_evento == idE && a.npenotazioni == 5).FirstOrDefault();
            //pren.pdf = bytes;
            //bytes = System.IO.File.ReadAllBytes(@"C:\tmp\template bimbo rai\badge6.pdf");
            //pren = db.B2RaiPlace_Eventi_Pdf.Where(a => a.id_evento == idE && a.npenotazioni == 6).FirstOrDefault();
            //pren.pdf = bytes;
            //bytes = System.IO.File.ReadAllBytes(@"C:\tmp\template bimbo rai\badge7.pdf");
            //pren = db.B2RaiPlace_Eventi_Pdf.Where(a => a.id_evento == idE && a.npenotazioni == 7).FirstOrDefault();
            //pren.pdf = bytes;
            //bytes = System.IO.File.ReadAllBytes(@"C:\tmp\template bimbo rai\badge8.pdf");
            //pren = db.B2RaiPlace_Eventi_Pdf.Where(a => a.id_evento == idE && a.npenotazioni == 8).FirstOrDefault();
            //pren.pdf = bytes;
            //bytes = System.IO.File.ReadAllBytes(@"C:\tmp\template bimbo rai\badge9.pdf");
            //pren = db.B2RaiPlace_Eventi_Pdf.Where(a => a.id_evento == idE && a.npenotazioni == 9).FirstOrDefault();
            //pren.pdf = bytes;
            //bytes = System.IO.File.ReadAllBytes(@"C:\tmp\template bimbo rai\badge10.pdf");
            //pren = db.B2RaiPlace_Eventi_Pdf.Where(a => a.id_evento == idE && a.npenotazioni == 10).FirstOrDefault();
            //pren.pdf = bytes;

            //DBHelper.Save(db);


            if ( matricola.StartsWith( "P" ) )
            {
                matricola = matricola.Replace( "P" , "" );
            }

            int iidEvento = Convert.ToInt32( idEvento );
            B2RaiPlace_Eventi_Evento evento = db.B2RaiPlace_Eventi_Evento.Where( a => a.id == iidEvento ).FirstOrDefault( );

            int numeroPrenotazioni = evento.B2RaiPlace_Eventi_Anagrafica.Where( x => x.matricola == matricola && x.confermata == true ).Count( );

            B2RaiPlace_Eventi_Pdf pdf = db.B2RaiPlace_Eventi_Pdf.Where( a => a.B2RaiPlace_Eventi_Evento.id == iidEvento && a.npenotazioni == numeroPrenotazioni ).FirstOrDefault( );

            if ( p )
            {
                return GetPdfBinary( pdf.id , matricola , true );
            }

            EventsPDFmodel model = new EventsPDFmodel( )
            {
                id = pdf.id ,
                idEvento = evento.id ,
                matricola = matricola ,
                dataEvento = Convert.ToDateTime( evento.data_inizio ) ,
                NomeEvento = evento.titolo
            };

            return View( "~/Views/Events/subpartial/_pdfviewer.cshtml" , model );
        }
        public ActionResult GetPdfBinary ( int idPdf , string matricola , bool returnfile = false )
        {
            var db = new myRaiData.digiGappEntities( );
            var pdf = db.B2RaiPlace_Eventi_Pdf.Where( x => x.id == idPdf ).FirstOrDefault( );

            if (pdf == null) return null;

            byte[] byteArray = pdf.pdf;
            PdfReader reader = new PdfReader( byteArray );
            MemoryStream ms = new MemoryStream( );
            PdfStamper stamper = new PdfStamper( reader , ms , '4' ); //Creating the PDF in version 1.4
            //Method 1
            stamper.FormFlattening = true;
            //Method 1
            string[] fields = stamper.AcroFields.Fields.Select( x => x.Key ).ToArray( );
            List<B2RaiPlace_Eventi_Anagrafica> campi = db.B2RaiPlace_Eventi_Anagrafica.Where( a => a.id_evento == pdf.B2RaiPlace_Eventi_Evento.id && a.matricola == matricola && a.confermata == true ).ToList( );
            for ( int key = 0 ; key <= fields.Count( ) - 1 ; key++ )
            {
                stamper.AcroFields.SetFieldProperty( fields[key] , "setfflags" , PdfFormField.FF_READ_ONLY , null );
                int numeroMax = 0;
                if ( fields[key].Substring( fields[key].Length - 2 , 2 ) == "10" )
                {
                    numeroMax = Convert.ToInt32( fields[key].Substring( fields[key].Length - 2 , 2 ) ) - 1;
                }
                else
                {
                    numeroMax = Convert.ToInt32( fields[key].Substring( fields[key].Length - 1 , 1 ) ) - 1;
                }
                if ( campi.Count > numeroMax )
                {
                    if ( ( fields[key].Length >= 4 && fields[key].Substring( 0 , 4 ) == "nome" ) )
                    {
                        stamper.AcroFields.SetField( fields[key] , campi[numeroMax].nome != null && campi[numeroMax].cognome != null ? campi[numeroMax].nome.ToUpper( ) + " " + campi[numeroMax].cognome.ToUpper( ) : "" );
                    }
                    else if ( fields[key].Length >= 4 && fields[key].Substring( 0 , 4 ) == "Note" )
                    {
                        stamper.AcroFields.SetField( fields[key] , campi[numeroMax].note_evento != null ? campi[numeroMax].note_evento.ToUpper( ) : "" );
                    }
                    else if ( fields[key].Length >= 7 && fields[key].Substring( 0 , 6 ) == "ospite" && fields[key].Substring( 6 , 1 ) != "d" )
                    {
                        stamper.AcroFields.SetField( fields[key] , campi[numeroMax].Dipendente != null ? campi[numeroMax].Dipendente.ToUpper( ) : "" );
                    }
                    else if ( fields[key].Length >= 2 && fields[key].Substring( 0 , 2 ) == "ID" )
                    {
                        stamper.AcroFields.SetField( fields[key] , GetIdPrenotazione( campi[numeroMax] ) );
                    }
                }
            }
            stamper.Writer.CloseStream = false;
            stamper.Close( );
            ms.Position = 0;
            byteArray = ms.ToArray( );
            ms.Flush( );
            reader.Close( );

            if ( returnfile )
                return File( byteArray , "application/pdf" , "badge.pdf" );

            MemoryStream pdfStream = new MemoryStream( );
            pdfStream.Write( byteArray , 0 , byteArray.Length );

            pdfStream.Position = 0;

            Response.AppendHeader( "content-disposition" , "inline; filename=Badge.pdf" );
            return new FileStreamResult( pdfStream , "application/pdf" );
        }

        public ActionResult getPostiDisponibili ( int idevento )
        {
            var db = new digiGappEntities( );

            var evento = db.B2RaiPlace_Eventi_Evento.Where( x => x.id == idevento ).FirstOrDefault( );
            if ( evento == null )
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { errore = "Evento non individuato" }
                };
            }
            else
            {
                int disp = ( int ) evento.numero_totale - evento.B2RaiPlace_Eventi_Anagrafica.Count( );
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = disp , errore = "" }
                };
            }
        }

        public ActionResult getInfoEvento ( int id )
        {
            var db = new digiGappEntities( );

            var evento = db.B2RaiPlace_Eventi_Evento.Where( x => x.id == id ).Select( x => new InfoEvento
            {
                Evento = x ,
                matricola = miamatricola ,

            } ).FirstOrDefault( );

            if ( evento.Evento.id_programma != null )
                evento.PrenotazioniStessoProgramma = EventsManager.MiePrenotazioniPerProgramma( ( int ) evento.Evento.id_programma , CommonHelper.GetCurrentUserMatricola( ) );
            return View( "infoevento" , evento );
        }
        public ActionResult controlEvento ( int id )
        {
            string matr = CommonManager.GetCurrentUserMatricola();

            var db = new myRaiData.digiGappEntities();
            int ContaEvento = db.B2RaiPlace_Eventi_Evento.Where( x => x.id == id && x.data_fine_prenotazione > DateTime.Now &&
                     x.data_inizio_prenotazione < DateTime.Now ).Count( );
            myRaiData.B2RaiPlace_Eventi_Evento ev = db.B2RaiPlace_Eventi_Evento.Where(x => x.id == id).FirstOrDefault();
            if (ev != null && !String.IsNullOrWhiteSpace(ev.sede_contabile) && ev.sede_contabile != myRai.Models.Utente.EsponiAnagrafica().SedeContabile)
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = -1 }
                };
            }
            else if ( ev != null && ev.B2RaiPlace_Eventi_Utenti_Abilitati != null && ev.B2RaiPlace_Eventi_Utenti_Abilitati.Count( ) > 0 && !ev.B2RaiPlace_Eventi_Utenti_Abilitati.Any( x => x.Matricola == matr ) )
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = -2 }
                };
            }
            else return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = ContaEvento }
                };
        }

        public ActionResult getPostiDisponibiliAll ( )
        {
            var db = new myRaiData.digiGappEntities( );

            var posti = db.B2RaiPlace_Eventi_Evento
            .Select( x => new
            {
                idevento = x.id ,
                posti_disp = x.numero_totale - x.B2RaiPlace_Eventi_Anagrafica.Count( ) ,
                dataEvento = x.data_inizio
            } ).OrderByDescending( a => a.dataEvento ).ToList( );

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                Data = new { result = posti }
            };
        }

        [HttpPost]
        public JsonResult GetPrenotazioneByID ( int idPrenotazione )
        {
            var db = new myRaiData.digiGappEntities( );
            string DataNascita = db.B2RaiPlace_Eventi_Anagrafica.Where(a => a.id == idPrenotazione).Select(a => a.data_nascita).FirstOrDefault() != null?  Convert.ToDateTime(db.B2RaiPlace_Eventi_Anagrafica.Where(a => a.id == idPrenotazione).Select(a => a.data_nascita).FirstOrDefault().Value).ToString("dd/MM/yyyy") : "";
            var prenotazioni = db.B2RaiPlace_Eventi_Anagrafica.Where( a => a.id == idPrenotazione ).Select( x => new
            {

                nome = x.nome ,
                cognome = x.cognome ,
                datanascita = DataNascita ,
                genere = x.genere ,
                email = x.email ,
                citta = x.citta_nascita ,
                telefono = x.telefono ,
                tipo_documento = x.tipo_documento ,
                documento = x.codice_documento ,
                grado = x.grado_parentela ,
                specificaInsediamento = x.sede_insediamento ,
                sedeInsediamento = x.B2RaiPlace_Eventi_Evento.vedi_insediamento,
                note = x.note

            } ).FirstOrDefault( );


            return Json( prenotazioni , JsonRequestBehavior.AllowGet );
        }


        [HttpPost]
        public ActionResult SetPrenotazione ( AnagraficaPrenotazone model )
        {

            /*if (String.IsNullOrWhiteSpace(model.nome))
            {
                return Content("Nome Obbligatorio");
            }
            if (String.IsNullOrWhiteSpace(model.cognome))
            {
                return Content("Cognome Obbligatorio");
            }
            if (String.IsNullOrWhiteSpace(model.dataNascita))
            {
                return Content("Data di Nascita Obbligatoria");
            }
            if (String.IsNullOrWhiteSpace(model.email))
            {
                return Content("Email Obbligatoria");
            }
            if (String.IsNullOrWhiteSpace(model.genere))
            {
                return Content("Genere Obbligatorio");
            }
            
            if (String.IsNullOrWhiteSpace(model.numeroDocumento))
            {
                return Content("Numero Documento Obbligatorio");
            }

            if (String.IsNullOrWhiteSpace(model.tipoDocumento))
            {
                return Content("Tipo Documento Obbligatorio");
            }

            if (model.sedeInsediamento)
            {
                if (String.IsNullOrWhiteSpace(model.specificaInsediamento))
                {
                    return Content("Sede Insediamento Obbligatoria");
                }
            }
            */
            var db = new myRaiData.digiGappEntities( );
            var eventoDB = db.B2RaiPlace_Eventi_Evento.Where( x => x.id == model.idevento ).FirstOrDefault( );
            if ( eventoDB != null && eventoDB.id_programma != null )
            {
                int? ma = EventsManager.PrenotazioniMaxPerProgramma( ( int ) eventoDB.id_programma );
                if ( ma != null )
                {
                    int m=EventsManager.MiePrenotazioniPerProgramma((int)eventoDB.id_programma, CommonManager.GetCurrentUserMatricola());
                    if ( m > ma || ( m == ma && model.idPrenotazione == 0 ) )
                    {
                        return Content( "E' stato raggiunto il limite per il programma in oggetto" );
                    }
                }

            }
            if ( !String.IsNullOrWhiteSpace( model.email ) )
            {
                if (!CommonManager.EmailIsValid(model.email))
                {
                    return Content( "Email non valida" );
                }
            }
            DateTime? d = null;
            if ( !String.IsNullOrWhiteSpace( model.datanascita ) )
            {
                DateTime dt;
                if ( !DateTime.TryParseExact( model.datanascita , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out dt ) || dt.Year < 1900 )
                {
                    return Content( "Data di nascita non valida" );
                }
                else {
                    d = dt;
                }
            }

            //salva su DB la prenotazione in anagrafica


            string matr = CommonManager.GetCurrentUserMatricola();
            var pren = new B2RaiPlace_Eventi_Anagrafica( );

            B2RaiPlace_Eventi_Evento evento = db.B2RaiPlace_Eventi_Evento.FirstOrDefault( x => x.id == model.idevento );
            if ( evento.limite_eta.HasValue && evento.limite_eta.Value > 0 && !RispettaLimiteEta( evento.limite_eta.Value , d ) )
            {
                return Content( "I minori di " + evento.limite_eta + " anni non possono partecipare" );
            }

            if ( model.idPrenotazione > 0 )
            {
                pren = db.B2RaiPlace_Eventi_Anagrafica.Where( x => x.id == model.idPrenotazione ).FirstOrDefault( );
            }
            else
            {
                pren = db.B2RaiPlace_Eventi_Anagrafica.Where( x => x.id_evento == model.idevento &&
                                                                     x.confermata == false &&
                                                                     x.matricola == matr ).FirstOrDefault( );
            }
            if ( pren == null )
            {
                return Content( "Non hai ulteriori posti disponibili." );
            }
            pren.codice_documento = model.numeroDocumento;
            pren.cognome = model.cognome;
            pren.nome = model.nome;
            pren.telefono = model.telefono;

            pren.tipo_documento = model.tipoDocumento;
            pren.confermata = true;
            pren.data_nascita = d;
            pren.data_prenotazione = DateTime.Now;
            pren.genere = model.genere;
            pren.grado_parentela = model.grado;
            pren.email = model.email;
            pren.citta_nascita = model.citta;
            pren.Dipendente = Utente.Nominativo();
            pren.sede = Utente.SedeGapp(DateTime.Now);
            if ( model.sedeInsediamento )
                pren.sede_insediamento = model.specificaInsediamento;
            pren.note = !String.IsNullOrWhiteSpace(model.nota) ? model.nota : "";

            if ( DBHelper.Save( db , CommonHelper.GetCurrentUserMatricola( ) ) )
            {
                return Content( "OK" );
            }
            else
            {
                return Content( "Errore salvataggio DB" );
            }
        }

        private bool RispettaLimiteEta ( int limite , DateTime? d )
        {
            bool rispetta = false;

            if ( d.HasValue )
            {
                DateTime today = DateTime.Now;
                int age = today.Year - d.Value.Year;
                if ( today.Month < d.Value.Month || ( today.Month == d.Value.Month && today.Day < d.Value.Day ) )
                    age--;

                rispetta = age >= limite;
            }

            return rispetta;
        }

        [HttpPost]
        public ActionResult GestisciNotifiche ( int idEvento )
        {
            var db = new myRaiData.digiGappEntities( );

            string matricola = CommonManager.GetCurrentUserMatricola();
            B2RaiPlace_Eventi_Evento evento = db.B2RaiPlace_Eventi_Evento.Where( a => a.id == idEvento ).FirstOrDefault( );
            string testo = String.Empty;
            if ( evento.B2RaiPlace_Eventi_Anagrafica.Where( a => a.matricola == matricola && a.confermata == true ).Count( ) > 0 )
            {
                testo = "Per l'evento " + evento.titolo + " hai " + evento.B2RaiPlace_Eventi_Anagrafica.Where( a => a.matricola == matricola && a.confermata == true ).Count( ) + " posti prenotati";
            }
            else
            {
                testo = "Per l'evento " + evento.titolo + " sono state cancellate tutte le prenotazioni";
            }
            if (db.MyRai_Notifiche.Where(a =>a.categoria== "Prenotazione Evento" &&  a.inserita_da == matricola && a.id_riferimento == evento.id).Count() > 0)
            {
                NotificheManager.ModificaNotifica( testo ,
                                    "Prenotazione Evento" ,
                                    matricola ,
                                    matricola ,
                                    evento.id );
                InviaEmail( evento , matricola );
            }
            else
            {
                NotificheManager.InserisciNotifica( testo ,
                                "Prenotazione Evento" ,
                                matricola ,
                                matricola ,
                                evento.id );
                InviaEmail( evento , matricola );

            }

            return Content( "OK" );
        }

        private void InviaEmail ( B2RaiPlace_Eventi_Evento evento , string matricola )
        {
            digiGappEntities db = new digiGappEntities( );
            MailSender invia = new MailSender( );
            Email eml = new Email( );
            eml.From = CommonManager.GetParametri<string>(EnumParametriSistema.MailEventiFrom)[0].ToString();

            eml.ContentType = "text/html";
            eml.Priority = 2;
            eml.SendWhen = DateTime.Now.AddSeconds( 1 );

            eml.Subject = "Prenotazione Evento";
            eml.toList = new string[] { myRai.Models.Utente.EsponiAnagrafica()._email };

            List<B2RaiPlace_Eventi_Anagrafica> anagrafiche = evento.B2RaiPlace_Eventi_Anagrafica.Where( a => a.matricola == matricola && a.confermata == true ).ToList( );

            if ( anagrafiche.Count( ) > 0 )
            {
                eml.Body = CommonManager.GetParametri<string>(EnumParametriSistema.MailEventiBody)[0].ToString();
            }
            else
            {
                eml.Body = CommonManager.GetParametri<string>(EnumParametriSistema.MailEventiBodyDel)[0].ToString();
            }
            eml.Body = eml.Body.Replace( "#nomeevento" , evento.titolo ).Replace( "#numero_richieste" , anagrafiche.Count( ).ToString( ) );
            string testo = String.Empty;

            foreach ( B2RaiPlace_Eventi_Anagrafica anagrafica in anagrafiche )
            {
                if ( evento.ticket.GetValueOrDefault( ) )
                {
                    testo += "N.ro badge<b>:" + GetIdPrenotazione( anagrafica ) + "<b/><br>";
                }

                testo += "Nominativo:" + anagrafica.nome + " " + anagrafica.cognome + "<br>";
                if ( anagrafica.telefono != null )
                {
                    testo += "Telefono:" + anagrafica.telefono + "<br>";
                }
                if ( anagrafica.data_nascita != null )
                {
                    testo += "Data Nascita:" + anagrafica.data_nascita.Value.ToString( "dd/MM/yyyy" ) + "<br>";
                }
                testo += "<br><br>";
            }
            testo += "Luogo:" + evento.luogo + "<br>";
            testo += "Data:" + evento.data_inizio.Value.ToString( "dd/MM/yyyy" ) + "<br>";
            if ( evento.testo_mail != null )
            {
                testo += "<br><br><br><font color='red'>" + evento.testo_mail + "</font>";
            }
            testo += "<br><br>";
            eml.Body = eml.Body.Replace( "#testo" , testo );

            //eml.toList = new string[] { "andrea.martis@3wlab.it" };
            string[] AccountUtenteServizio = CommonManager.GetParametri<string>(EnumParametriSistema.AccountUtenteServizio);
            invia.Credentials = new System.Net.NetworkCredential( AccountUtenteServizio[0] , AccountUtenteServizio[1] , "RAI" );

            try
            {
                invia.Send( eml );

            }
            catch ( Exception ex )
            {

            }
        }

        [HttpPost]
        public ActionResult opzionaPostiEvento ( int idevento )
        {
            var db = new myRaiData.digiGappEntities();
            var evento = db.B2RaiPlace_Eventi_Evento.Where( x => x.id == idevento ).FirstOrDefault( );
            if ( evento == null )
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = "Evento non individuato nel DB" }
                };
            }

            rimuoviOpzioniPostiEvento( idevento );
            int GiaConfermate = EventsManager.PrenotazioniConfermate(idevento, CommonManager.GetCurrentUserMatricola());

            int disp = ( int ) evento.numero_totale - evento.B2RaiPlace_Eventi_Anagrafica.Count( );
            if ( disp <= 0 )
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = "Posti terminati per questo evento" }
                };
            }

            int DaOpzionare = ( disp >=
                ( int ) evento.numero_massimo - GiaConfermate ? ( int ) evento.numero_massimo - GiaConfermate : disp );

            for ( int i = 1 ; i <= DaOpzionare ; i++ )
            {
                B2RaiPlace_Eventi_Anagrafica a = new B2RaiPlace_Eventi_Anagrafica( )
                {
                    id_evento = idevento ,
                    matricola = CommonManager.GetCurrentUserMatricola(),
                    data_prenotazione = DateTime.Now ,
                    confermata = false
                };
                db.B2RaiPlace_Eventi_Anagrafica.Add( a );
            }

            if ( DBHelper.Save( db , CommonHelper.GetCurrentUserMatricola( ) ) )
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = "OK" }
                };
            }
            else
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = "Errore salvataggio DB" }
                };
            }
        }

        [HttpPost]
        public ActionResult AggiornaDataPrenotazione ( int idevento )
        {
            var db = new myRaiData.digiGappEntities();
            string matricola = CommonManager.GetCurrentUserMatricola();
            List<B2RaiPlace_Eventi_Anagrafica> anagrafiche = db.B2RaiPlace_Eventi_Anagrafica.Where( x => x.B2RaiPlace_Eventi_Evento.id == idevento && x.matricola == matricola && x.confermata == false ).ToList( );

            foreach ( B2RaiPlace_Eventi_Anagrafica anagrafica in anagrafiche )
            {
                anagrafica.data_prenotazione = DateTime.Now;
            }

            if ( DBHelper.Save( db, CommonHelper.GetCurrentUserMatricola() ) )
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = "OK" }
                };
            }
            else
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = "Errore salvataggio DB" }
                };
            }
        }

        [HttpPost]
        public ActionResult rimuoviOpzioniPostiEvento ( int idevento )
        {
            var db = new myRaiData.digiGappEntities();
            string matr = CommonManager.GetCurrentUserMatricola();
            var nonConf = db.B2RaiPlace_Eventi_Anagrafica.Where( x =>
                 x.matricola == matr && x.confermata == false && x.id_evento == idevento );

            foreach ( var item in nonConf )
            {
                db.B2RaiPlace_Eventi_Anagrafica.Remove( item );
            }
            if ( DBHelper.Save( db, CommonHelper.GetCurrentUserMatricola() ) )
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = "OK" }
                };
            }
            else
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = "Errore salvataggio DB" }
                };
            }
        }

        public ActionResult getPostiPrenotati ( int idevento )
        {
            var db = new myRaiData.digiGappEntities();
            var model = db.B2RaiPlace_Eventi_Anagrafica.Where( x =>
                         x.matricola == miamatricola && x.confermata == true && x.id_evento == idevento )
                        .OrderBy( x => x.cognome )
                        .ThenBy( x => x.nome )
                        .ToList( );
            return View( "postiprenotati" , model );
        }

        public ActionResult getInfoPostiPrenotati ( int idevento )
        {
            var db = new myRaiData.digiGappEntities();
            var model = db.B2RaiPlace_Eventi_Anagrafica.Where( x =>
                         x.matricola == miamatricola && x.confermata == true && x.id_evento == idevento )
                        .OrderBy( x => x.cognome )
                        .ThenBy( x => x.nome )
                        .ToList( );
            return View( "postiinfoprenotati" , model );
        }

        public ActionResult SedeInsediamentoPossibile ( int idevento )
        {
            var db = new myRaiData.digiGappEntities();

            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                Data = new { result = db.B2RaiPlace_Eventi_Evento.Where( a => a.id == idevento ).FirstOrDefault( ).vedi_insediamento }
            };
        }

        public ActionResult SettaCampi ( int idevento )
        {
            var db = new myRaiData.digiGappEntities();
            
            var Lista = db.B2RaiPlace_Eventi_Campi.Where( a => a.id_evento == idevento ).Select( x => new
            {
                id = x.id ,
                idEvento = x.id_evento ,
                descrizione = x.Descrizione ,
                visibilie = x.Visibile ,
                obbligatorio = x.Obbligatorio

            } ).ToList( );
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                Data = new { result = Lista }
            };
        }

        public ActionResult prenotazionePossibile ( int idevento )
        {
            var db = new myRaiData.digiGappEntities();
            int quantita = db.B2RaiPlace_Eventi_Anagrafica.Where( x =>
                         x.matricola == miamatricola && x.confermata == true && x.id_evento == idevento )
                        .Count( );
            int? maxPerMatricola = db.B2RaiPlace_Eventi_Evento.Where( x => x.id == idevento ).Select( x => x.numero_massimo ).FirstOrDefault( );
            return new JsonResult
            {
                JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                Data = new { result = ( quantita < maxPerMatricola ) }
            };
        }


        public ActionResult eliminaAnagraficaPrenotazione ( int id )
        {
            var db = new myRaiData.digiGappEntities();
            var anag = db.B2RaiPlace_Eventi_Anagrafica.Where( x => x.id == id ).FirstOrDefault( );
            if ( anag == null )
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = "Anagrafica non individuata nel DB" }
                };
            }
            else
            {

                anag.confermata = false;

                if ( DBHelper.Save( db, CommonHelper.GetCurrentUserMatricola() ) )
                {
                    // reperimento dell'identificativo evento
                    int idEvento = anag.id_evento;
                    // conta le prenotazioni attive per quell'evento
                    // effettuate dallo stesso utente che ha effettuato la
                    // prenotazione che si sta rimuovendo
                    int prenotati = db.B2RaiPlace_Eventi_Anagrafica.Count( x => x.id_evento == anag.id_evento &&
                                                                            x.confermata &&
                                                                            x.matricola.Equals( anag.matricola ) );

                    // se sono state rimosse tutte le prenotazioni per quell'evento
                    // allora verrà rimossa la notifica di prenotazione evento
                    if ( prenotati <= 0 )
                    {
                        EliminaNotifica( idEvento );
                    }

                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                        Data = new { result = "OK" }
                    };
                }
                else
                {
                    return new JsonResult
                    {
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                        Data = new { result = "Errore salvataggio DB" }
                    };
                }
            }
        }

        public ActionResult eliminaPrenEvento ( int idEvento )
        {
            string matricola = CommonHelper.GetCurrentUserMatricola( );

            var db = new digiGappEntities( );
            var prenEvento = db.B2RaiPlace_Eventi_Anagrafica.Where( x => x.id_evento == idEvento && x.matricola == matricola );
            //prenEvento.ForEach( x => x.confermata = false );
            foreach (var item in prenEvento)
                item.confermata = false;
            
            if ( DBHelper.Save( db, CommonHelper.GetCurrentUserMatricola() ) )
            {
                EliminaNotifica( idEvento );

                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = "OK" }
                };
            }
            else
            {
                return new JsonResult
                {
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet ,
                    Data = new { result = "Errore salvataggio DB" }
                };
            }
        }

        /// <summary>
        /// Rimozione della notifica associata all'evento
        /// </summary>
        /// <param name="idEvento"></param>
        private void EliminaNotifica ( int idEvento )
        {
            try
            {
                using ( digiGappEntities db = new digiGappEntities( ) )
                {
                    var toRemove = db.MyRai_Notifiche.Where( w => w.id_riferimento.Equals( idEvento ) &&
                                                                w.categoria.Equals( "Prenotazione Evento" ) ).FirstOrDefault( );

                    if ( toRemove != null )
                    {
                        db.MyRai_Notifiche.Remove( toRemove );
                        db.SaveChanges( );
                    }
                }
            }
            catch ( Exception ex )
            {
                throw new Exception( ex.Message );
            }
        }
    }
}