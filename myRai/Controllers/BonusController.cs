using iTextSharp.text;
using iTextSharp.text.pdf;
using myRai.Business;
using myRai.Models;
using myRaiCommonModel;
using myRaiCommonTasks;
using myRaiCommonTasks.sendMail;
using myRaiData;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Web.Mvc;
using System.Diagnostics;

namespace myRai.Controllers
{
    public class BonusController : Controller
    {
        public ActionResult GetFormBonus100 ( int anno )
        {
            Bonus100Model model = new Bonus100Model( );
            model.Anno = anno;

            return View( "~/Views/Bonus/FormBonus100Bis.cshtml" , model );
        }

        private Bonus100Model GetModel ( int anno )
        {
            DateTime dataCompilazione = DateTime.Now;

            Bonus100Model model = new Bonus100Model( );

            model.GiaScelto = true;

            #region Bonus 100 EURO

            try
            {
                string dtStart = "";
                string dtEnd = "";

                using ( digiGappEntities dbDG = new digiGappEntities( ) )
                {
                    var parametro = dbDG.MyRai_ParametriSistema.FirstOrDefault( x => x.Chiave == "Bonus100DateRange" );
                    if ( parametro != null )
                    {
                        dtStart = parametro.Valore1;
                        dtEnd = parametro.Valore2;
                    }
                }

                DateTime dataInizio = DateTime.Now;
                DateTime dataFine = DateTime.Now;
                DateTime.TryParseExact( dtStart , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out dataInizio );
                DateTime.TryParseExact( dtEnd , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out dataFine );

                if ( dataInizio <= DateTime.Now && dataFine >= DateTime.Now )
                {
                    string urlServizio = System.Configuration.ConfigurationManager.AppSettings["URLwiahrss"];

                    Uri u = new Uri( urlServizio );

                    BasicHttpBinding binding = new BasicHttpBinding( );
                    EndpointAddress address = new EndpointAddress( u );
                    binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;
                    binding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
                    binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;

                    binding.MessageEncoding = WSMessageEncoding.Text;
                    binding.TextEncoding = Encoding.UTF8;
                    binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;

                    var _client = new myRaiServiceHub.ServiceBonus100.ezServiceSoapClient( binding , address );

                    if ( _client.ClientCredentials != null )
                    {
                        _client.ClientCredentials.Windows.ClientCredential = new NetworkCredential( CommonHelper.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[0] , CommonHelper.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[1] );
                        _client.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Delegation;
                    }
                    _client.Open( );

                    var requestInterceptor = new InspectorBehavior( );
                    _client.Endpoint.Behaviors.Add( requestInterceptor );

                    var clientResponse = _client.TBONUS_100EURO( CommonManager.GetCurrentUserMatricola7chars( ) );

                    if ( clientResponse != null && clientResponse.Rows != null && clientResponse.Rows.Count > 0 )
                    {
                        var r = clientResponse.Rows[0];

                        string risposta = r.ItemArray[16].ToString( );
                        string dtRisposta = r.ItemArray[17].ToString( );

                        if ( !String.IsNullOrEmpty( risposta ) )
                        {
                            risposta = risposta.Trim( );
                            if ( risposta.Equals( "\0" ) )
                            {
                                risposta = "";
                            }
                        }

                        if ( !String.IsNullOrEmpty( dtRisposta ) )
                        {
                            dtRisposta = dtRisposta.Trim( );
                            if ( dtRisposta.Equals( "01/01/0001 00:00:00" ) )
                            {
                                dtRisposta = "";
                            }
                        }

                        //DIRITTO = ‘S’
                        //SOGLIA_40_60 = ‘40’
                        //BONUS_SPETTANTE > 0
                        //DICHIARAZIONE = ‘S’

                        if ( !String.IsNullOrEmpty( risposta ) && !String.IsNullOrEmpty( dtRisposta ) )
                        {
                            model.Scelta1 = ( risposta == "1" );
                            model.Scelta2 = ( risposta == "0" );

                            DateTime.TryParseExact( dtRisposta , "dd/MM/yyyy HH:mm:ss" , null , System.Globalization.DateTimeStyles.None , out dataCompilazione );

                            model.DataCompilazione = dataCompilazione;
                        }
                    }

                    string requestXML = requestInterceptor.LastRequestXML;
                    string responseXML = requestInterceptor.LastResponseXML;

                    _client.Close( );
                }
            }
            catch ( Exception ex )
            {

            }

            #endregion

            return model;
        }

        public ActionResult GetFormBonus100ReadOnly ( int anno , string scelta )
        {
            Bonus100Model model = GetModel( anno );

            return View( "~/Views/Bonus/FormBonus100Bis.cshtml" , model );
        }

        public ActionResult GetFormBonus100PDFViewer ( int anno )
        {
            Bonus100Model model = GetModel( anno );

            return View( "~/Views/Bonus/FormBonus100PDFViewer.cshtml" , model );
        }

        public ActionResult GetPDF ( int anno )
        {
            Bonus100Model model = GetModel( anno );

            int scelta = ( model.Scelta1 ) ? 1 : 0;

            byte[] myArrayOfBytes = null;
            try
            {
                DateTime dataNascita = Utente.EsponiAnagrafica( )._dataNascita.GetValueOrDefault( );
                string dNascita = "";

                if ( dataNascita.Equals( DateTime.MinValue ) ||
                    dataNascita <= DateTime.MinValue )
                {
                    dNascita = "";
                }
                else
                {
                    dNascita = dataNascita.ToString( "dd/MM/yyyy" );
                }

                // creazione del pdf
                myArrayOfBytes = CreazionePDF( CommonManager.GetCurrentUserMatricola( ) ,
                                                    String.Format( "{0} {1}" , Utente.EsponiAnagrafica( )._nome , Utente.EsponiAnagrafica( )._cognome ) ,
                                                    Utente.EsponiAnagrafica( )._provinciaNascita ,
                                                    Utente.EsponiAnagrafica( )._comuneNascita ,
                                                    dNascita ,
                                                    Utente.EsponiAnagrafica( )._genere ,
                                                     scelta ,
                                                     model.DataCompilazione.GetValueOrDefault( ) );

                if ( myArrayOfBytes == null )
                {
                    throw new Exception( "Errore nella creazione del PDF" );
                }
            }
            catch ( Exception ex )
            {
                throw new Exception( ex.Message );
            }

            if ( myArrayOfBytes == null )
            {
                return null;
            }
            return new FileContentResult( myArrayOfBytes , "application/pdf" );
        }

        private string Salva ( string scelta )
        {
            string esito = "";
            try
            {
                string urlServizio = System.Configuration.ConfigurationManager.AppSettings["URLwiahrss"];

                Uri u = new Uri( urlServizio );

                BasicHttpBinding binding = new BasicHttpBinding( );
                EndpointAddress address = new EndpointAddress( u );
                binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;
                binding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
                binding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;

                binding.MessageEncoding = WSMessageEncoding.Text;
                binding.TextEncoding = Encoding.UTF8;
                binding.Security.Mode = BasicHttpSecurityMode.TransportCredentialOnly;

                var _client = new myRaiServiceHub.ServiceBonus100.ezServiceSoapClient( binding , address );

                if ( _client.ClientCredentials != null )
                {
                    _client.ClientCredentials.Windows.ClientCredential = new NetworkCredential( CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[0] , CommonManager.GetParametri<string>( EnumParametriSistema.AccountUtenteServizio )[1] );
                    _client.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Delegation;
                }
                _client.Open( );

                var requestInterceptor = new InspectorBehavior( );
                _client.Endpoint.Behaviors.Add( requestInterceptor );

                // aggiornamento dei dati
                var clientResponse = _client.TUPDATE_BONUS_100EURO(myRaiHelper.CommonHelper.GetCurrentUserMatricola7chars( ) , scelta );

                // verifica dei dati aggiornati
                if ( !String.IsNullOrEmpty(clientResponse) )
                {
                    if (clientResponse != "OK")
                    {
                        esito = clientResponse;
                    }
                }
                else
                {
                    // se non trova i dati dell'utente non deve mostrare il widget
                    esito = "Non è stato possibile aggiornare i dati. Record non trovato";
                }

                string requestXML = requestInterceptor.LastRequestXML;
                string responseXML = requestInterceptor.LastResponseXML;

                _client.Close( );
            }
            catch ( Exception ex )
            {
                esito = ex.Message;
                CommonTasks.LogErrore( ex.Message , "SalvataggioBonus100Euro - WEB - Azione Salva" );
            }

            return esito;
        }

        public ActionResult SalvaScelta ( int scelta )
        {
            string result = "OK";

            try
            {
                // salva scelta
                string esito = Salva( scelta.ToString( ) );

                // se è vuoto allora update completato
                // altrimenti sarà valorizzato con la descrizione dell'errore
                if ( !String.IsNullOrEmpty( esito ) )
                {
                    throw new Exception( esito );
                }

                try
                {
                    myRaiHelper.Logger.LogAzione( new MyRai_LogAzioni( )
                    {
                        applicativo = "Portale" ,
                        data = DateTime.Now ,
                        matricola = CommonManager.GetCurrentUserMatricola( ) ,
                        descrizione_operazione = "Salvataggio scelta bonus100 - scelta - " + scelta.ToString( ) ,
                        operazione = "Salvataggio Bonus 100" ,
                        provenienza = new StackFrame( 1 , true ).GetMethod( ).Name
                    } , CommonManager.GetCurrentUserMatricola( ) );
                }
                catch ( Exception ex )
                {

                }

                // se tutto ok allora genera il pdf e lo invia tramite mail
                string matricola = CommonManager.GetCurrentUserMatricola( );
                DateTime dataNascita = Utente.EsponiAnagrafica( )._dataNascita.GetValueOrDefault( );
                string dNascita = "";

                if ( dataNascita.Equals( DateTime.MinValue ) ||
                    dataNascita <= DateTime.MinValue )
                {
                    dNascita = "";
                }
                else
                {
                    dNascita = dataNascita.ToString( "dd/MM/yyyy" );
                }

                // creazione del pdf
                byte[] myArrayOfBytes = CreazionePDF( CommonManager.GetCurrentUserMatricola( ) ,
                                                    String.Format( "{0} {1}" , Utente.EsponiAnagrafica( )._nome , Utente.EsponiAnagrafica( )._cognome ) ,
                                                    Utente.EsponiAnagrafica( )._provinciaNascita ,
                                                    Utente.EsponiAnagrafica( )._comuneNascita ,
                                                    dNascita ,
                                                    Utente.EsponiAnagrafica( )._genere ,
                                                    scelta ,
                                                    DateTime.Now );

                if ( myArrayOfBytes == null )
                {
                    result = "Errore nella creazione del PDF";
                    throw new Exception( result );
                }

                // se tutto ok
                // invia mail con la copia del modulo compilato
                InvioMail( matricola , myArrayOfBytes , CommonTasks.EnumParametriSistema.MailTemplateModuloBonus100 );

            }
            catch ( Exception ex )
            {
                result = ex.Message;
                CommonTasks.LogErrore( ex.Message , "SalvataggioBonus100Euro - WEB - Azione SalvaScelta" );
            }

            return Content( result );

        }

        #region Creazione PDF

        private static PdfPCell WriteCell ( string text , int border , int colspan , int textAlign , Font f )
        {
            PdfPCell cell = new PdfPCell( new Phrase( text , f ) );
            cell.Border = border;
            cell.HorizontalAlignment = textAlign; //0=Left, 1=Centre, 2=Right
            cell.Colspan = colspan;
            return cell;
        }

        private static PdfPCell WriteCell ( Phrase text , int border , int colspan , int textAlign )
        {
            PdfPCell cell = new PdfPCell( text );
            cell.Border = border;
            cell.HorizontalAlignment = textAlign; //0=Left, 1=Centre, 2=Right

            cell.Colspan = colspan;
            return cell;
        }

        private static PdfPCell WriteCell ( Paragraph text , int border , int colspan , int textAlign )
        {
            PdfPCell cell = new PdfPCell( text );
            cell.Border = border;
            cell.HorizontalAlignment = textAlign; //0=Left, 1=Centre, 2=Right

            cell.Colspan = colspan;
            return cell;
        }

        public static iTextSharp.text.Image drawCircle ( PdfContentByte contentByte , bool full )
        {
            var template = contentByte.CreateTemplate( 20 , 20 );

            int arcPosStartY = 0;
            int arcPosStartX = 0;
            int arcPosEndX = 10;
            int arcPosEndY = 10;

            template.Arc( arcPosStartX , arcPosStartY , arcPosEndX , arcPosEndY , 0 , 360 );
            if ( full )
            {
                template.SetRGBColorFill( 0 , 0 , 0 );
                template.Fill( );
            }
            template.SetRGBColorStroke( 0 , 0 , 0 );
            template.Stroke( );

            var img = iTextSharp.text.Image.GetInstance( template );

            return img;
        }

        private static PdfPCell writeCellTab ( PdfContentByte cb , int border , int colspan , int textAlign , bool full )
        {
            PdfPCell cell;
            iTextSharp.text.Image img = drawCircle( cb , full );
            cell = new PdfPCell( img );
            cell.Border = border;
            cell.HorizontalAlignment = textAlign; //0=Left, 1=Centre, 2=Right
            cell.Colspan = colspan;
            return cell;
        }

        private static byte[] CreazionePDF ( string matricola , string nominativo , string prov , string luogo , string data , string sesso , int scelta , DateTime dataCreazione )
        {
            try
            {
                var cultureInfo = CultureInfo.GetCultureInfo( "it-IT" );

                byte[] bytes = null;
                const int fontIntestazione = 12;
                const int fontCorpo = 11;

                int border = 0; // none

                BaseFont bf = BaseFont.CreateFont( BaseFont.HELVETICA , BaseFont.CP1250 , BaseFont.NOT_EMBEDDED );
                Font myFontIntestazione = new Font( bf , fontIntestazione , Font.NORMAL );
                Font myFontIntestazioneBold = new Font( bf , fontIntestazione , Font.BOLD );
                Font myFontCorpo = new Font( bf , fontCorpo , Font.NORMAL );
                Font myFontCorpoBold = new Font( bf , fontCorpo , Font.BOLD );

                using ( MemoryStream ms = new MemoryStream( ) )
                {
                    // Creazione dell'istanza del documento, impostando tipo di pagina e margini
                    Document document = new Document( PageSize.A4 , 25 , 25 , 25 , 25 );
                    PdfWriter writer = PdfWriter.GetInstance( document , ms );

                    document.Open( );

                    float startWrite = 780;
                    PdfContentByte cb = writer.DirectContent;

                    Phrase phrase = new Phrase( );
                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );

                    Paragraph p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;

                    document.Add( p );

                    //float ascent = bf.GetAscentPoint( " " , 12 );
                    //float descent = bf.GetDescentPoint( " " , 12 );
                    //float height = ascent - descent;

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    string tx = "DICHIARAZIONE PER LA FRUIZIONE DEL PREMIO RICONOSCIUTO AI LAVORATORI DIPENDENTI CHE NEL MESE DI MARZO 2020 HANNO RESO LA LORO ATTIVITA' LAVORATIVA PRESSO LA SEDE AZIENDALE E/O IN TRASFERTA";

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( tx , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    if ( sesso.ToUpper( ).Equals( "F" ) )
                    {
                        phrase.Add( new Chunk( "La sottoscritta " , myFontCorpo ) );
                        phrase.Add( new Chunk( nominativo , myFontCorpoBold ) );
                        phrase.Add( new Chunk( " nata il " , myFontCorpo ) );
                        phrase.Add( new Chunk( data , myFontCorpoBold ) );
                        phrase.Add( new Chunk( " a " , myFontCorpo ) );
                        phrase.Add( new Chunk( luogo , myFontCorpoBold ) );
                        phrase.Add( new Chunk( " prov( " , myFontCorpo ) );
                        phrase.Add( new Chunk( prov , myFontCorpoBold ) );
                        phrase.Add( new Chunk( " ) matr. n. " , myFontCorpo ) );
                        phrase.Add( new Chunk( matricola , myFontCorpoBold ) );
                    }
                    else
                    {
                        phrase.Add( new Chunk( "Il sottoscritto " , myFontCorpo ) );
                        phrase.Add( new Chunk( nominativo , myFontCorpoBold ) );
                        phrase.Add( new Chunk( " nato il " , myFontCorpo ) );
                        phrase.Add( new Chunk( data , myFontCorpoBold ) );
                        phrase.Add( new Chunk( " a " , myFontCorpo ) );
                        phrase.Add( new Chunk( luogo , myFontCorpoBold ) );
                        phrase.Add( new Chunk( " prov( " , myFontCorpo ) );
                        phrase.Add( new Chunk( prov , myFontCorpoBold ) );
                        phrase.Add( new Chunk( " ) matr. n. " , myFontCorpo ) );
                        phrase.Add( new Chunk( matricola , myFontCorpoBold ) );
                    }                    

                    phrase.Add( new Chunk( " ai fini del riconoscimento del premio monetario previsto dall'articolo 63 del Decreto Legge n. 18/2020 in relazione al requisito reddituale relativo all'anno d'imposta 2019 " , myFontCorpo ) );
                   
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( "dichiara (ai sensi degli artt. 46 e 47 del DPR n. 445/2000) quanto segue:" , myFontIntestazione ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_CENTER;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    Phrase imgPhrase = new Paragraph( new Chunk( drawCircle( cb , ( scelta == 1 ) ) , 1f , 0f ) );
                    imgPhrase.Add( new Chunk( "di NON aver percepito, nel corso dell'anno 2019, redditi complessivi da lavoro dipendente (*) per un importo superiore a 40.000 euro e pertanto di aver diritto all'erogazione del premio;" , myFontCorpo ) );
                    p = new Paragraph( imgPhrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    imgPhrase = new Phrase( );
                    imgPhrase = new Paragraph( new Chunk( drawCircle( cb , ( scelta == 0 ) ) , 1f , 0f ) );
                    imgPhrase.Add( new Chunk( "di aver percepito, nel corso dell'anno 2019, redditi complessivi da lavoro dipendente (*) per un importo superiore a 40.000 euro e pertanto di NON aver diritto alla corresponsione del premio." , myFontCorpo ) );
                    p = new Paragraph( imgPhrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( "Data " + dataCreazione.ToString( "dd/MM/yyyy" ) , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( "(*) ai fini del calcolo dell'importo del reddito complessivo da lavoro dipendente si deve tener conto delle seguenti retribuzioni eventualmente percepite: stipendi, cassa integrazione, indennità di disoccupazione, indennita' di mobilita', indennita' di malattia, indennita' di maternita', etc. Tali dati sono contenuti nei punti 1 e/o 2 delle CU 2020 (redditi 2019) rilasciate al dipendente dai sostituti che nel 2019 hanno corrisposto le somme elencate (a titolo esemplificativo, dal datore di lavoro, dall'ente previdenziale, etc). A tali importi vanno aggiunti quelli eventualmente esposti nei punti 455, 456, 463 e 465 della CU 2020, nonche' la quota di abbattimento del reddito di lavoro dipendente percepito dai residenti a Campione d'Italia riportata nelle annotazioni con il codice \"CA\" della CU 2020. Sono invece esplicitamente esclusi dal calcolo i redditi assimilati a quelli di lavoro dipendente( ad es , borse di studio , collaborazioni coordinate e continuative , etc ) e le pensioni." , myFontCorpo ) );

                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;

                    document.Add( p );
                    document.Close( );
                    writer.Close( );
                    bytes = ms.ToArray( );
                    return bytes;
                }
            }
            catch ( Exception ex )
            {
                CommonTasks.LogErrore( ex.Message , "Bonus100Euro - WEB - Azione CreazionePDF" );
                throw new Exception( ex.Message );
            }
        }

        #endregion

        #region Invio Mail

        /// <summary>
        /// 
        /// </summary>
        /// <param name="destinatario"></param>
        /// <param name="sedeGAPP"></param>
        private void InvioMail ( string matricola , byte[] pdf , CommonTasks.EnumParametriSistema template )
        {
            try
            {
                string[] MailParams = myRaiCommonTasks.CommonTasks.GetParametri<string>( template );

                string body = MailParams[0];
                string MailSubject = MailParams[1];

                string[] mailParams2 = CommonTasks.GetParametri<string>( CommonTasks.EnumParametriSistema.MailTemplateModuloDetassazioneFrom );
                string from = mailParams2[0];

                string dest = CommonTasks.GetEmailPerMatricola( matricola );

                if ( String.IsNullOrWhiteSpace( dest ) )
                    dest = "P" + matricola + "@rai.it";

                GestoreMail mail = new GestoreMail( );

                body = body.Replace( "#DATA#" , DateTime.Now.ToString( "dd/MM/yyyy" ) );

                List<Attachement> attachments = new List<Attachement>( );

                Attachement a = new Attachement( )
                {
                    AttachementName = "Modulo.pdf" ,
                    AttachementType = "Application/PDF" ,
                    AttachementValue = pdf
                };

                attachments.Add( a );

                var response = mail.InvioMail( body , MailSubject , dest , "raiplace.selfservice@rai.it" , from , attachments );

                if ( response != null && response.Errore != null )
                {
                    MyRai_LogErrori err = new MyRai_LogErrori( )
                    {
                        applicativo = "Portale" ,
                        data = DateTime.Now ,
                        provenienza = "Bonus100 - InvioMail" ,
                        error_message = response.Errore + " per " + dest
                    };

                    using ( digiGappEntities db = new digiGappEntities( ) )
                    {
                        db.MyRai_LogErrori.Add( err );
                        db.SaveChanges( );
                    }
                }
            }
            catch ( Exception ex )
            {
                MyRai_LogErrori err = new MyRai_LogErrori( )
                {
                    applicativo = "Portale" ,
                    data = DateTime.Now ,
                    provenienza = "Bonus100Euro - WEB - InvioMail" ,
                    error_message = ex.Message
                };

                using ( digiGappEntities db = new digiGappEntities( ) )
                {
                    db.MyRai_LogErrori.Add( err );
                    db.SaveChanges( );
                }
            }
        }

        private void ScriviLogAzione ( string matricola , string metodo , string msg )
        {
            if ( !String.IsNullOrEmpty( msg ) )
            {
                using ( digiGappEntities db = new digiGappEntities( ) )
                {
                    db.MyRai_LogAzioni.Add( new MyRai_LogAzioni( )
                    {
                        matricola = matricola ,
                        data = DateTime.Now ,
                        operazione = metodo ,
                        applicativo = "Portale" ,
                        descrizione_operazione = msg ,
                        provenienza = "DetassazioneController"
                    } );
                    db.SaveChanges( );
                }
            }
        }
        #endregion
    }
}