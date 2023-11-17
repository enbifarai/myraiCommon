using iTextSharp.text;
using iTextSharp.text.pdf;
using myRaiCommonModel.Detassazione;
using myRaiCommonTasks;
using myRaiCommonTasks.sendMail;
using myRaiData;
using myRaiHelper;
using MyRaiServiceInterface.MyRaiServiceReference1;
using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;

namespace myRai.Controllers
{
    public class DetassazioneController : Controller
    {
        #region View Modelli Richieste/Rinuncia
        public ActionResult GetFormDetassazione ( int anno, string codice, string codiceDetassazione )
        {
            DetassazioneModel model = new DetassazioneModel( );
            model.Matricola = CommonHelper.GetCurrentUserMatricola( );
            string nominativo = String.Format( "{0} {1}" , UtenteHelper.EsponiAnagrafica( )._nome, UtenteHelper.EsponiAnagrafica( )._cognome );

            // controllo che per lo più è per evitare errori di nominativi vuoti su sviluppo
            if ( String.IsNullOrEmpty( UtenteHelper.EsponiAnagrafica( )._nome ) || String.IsNullOrEmpty( UtenteHelper.EsponiAnagrafica( )._cognome ) )
            {
                using ( HRPADBEntities nomeDb = new HRPADBEntities( ) )
                {
                    var item = nomeDb.T_DetaxNew.Where( w => w.Matricola_T_DetaxNew.Equals( model.Matricola ) ).FirstOrDefault( );
                    if ( item != null )
                    {
                        nominativo = item.Nominativo_T_DetaxNew.Trim( );
                    }
                }
            }

            model.Nominativo = nominativo;
            model.Codice = codice;
            model.CodiceDetassazione = codiceDetassazione;
            model.CodiceFiscale = UtenteHelper.EsponiAnagrafica( )._cf;
            model.DataDiNascita = UtenteHelper.EsponiAnagrafica( )._dataNascita.GetValueOrDefault( );
            model.LuogoDiNascita = UtenteHelper.EsponiAnagrafica( )._comuneNascita;
            model.Sesso = UtenteHelper.EsponiAnagrafica( )._genere;
            model.Anno = anno;

            if (String.IsNullOrEmpty(codice))
            {
                return null;
            }
            else
            {
                // Rinuncia
                if (codice.ToUpper().Equals("1C"))
                {
                    return View( "~/Views/Detassazione/FormDetassazioneRinuncia.cshtml", model );
                }
                else if ( codice.ToUpper( ).Equals( "2C" ) )
                {
                    return View( "~/Views/Detassazione/FormDetassazione2C.cshtml" , model );
                }
            }

            return View("~/Views/Detassazione/FormDetassazione.cshtml");
        }

        public ActionResult GetPDFViewer ( int anno , string codice , string codiceDetassazione )
        {
            DetassazioneModel model = new DetassazioneModel( );
            model.Matricola = CommonHelper.GetCurrentUserMatricola( );
            model.Nominativo = String.Format( "{0} {1}" , UtenteHelper.EsponiAnagrafica( )._nome , UtenteHelper.EsponiAnagrafica( )._cognome );
            model.Codice = codice;
            model.CodiceDetassazione = codiceDetassazione;
            model.CodiceFiscale = UtenteHelper.EsponiAnagrafica( )._cf;
            model.DataDiNascita = UtenteHelper.EsponiAnagrafica( )._dataNascita.GetValueOrDefault( );
            model.LuogoDiNascita = UtenteHelper.EsponiAnagrafica( )._comuneNascita;
            model.Sesso = UtenteHelper.EsponiAnagrafica( )._genere;
            model.Anno = anno;

            return View( "~/Views/Detassazione/FormDetassazioneVisualizzaPDF.cshtml" , model );
        }

        public ActionResult GetPDF ( string matricola , int anno ,  string codiceDetassazione )
        {
            byte[] result = null;
            try
            {
                using ( HRPADBEntities db = new HRPADBEntities( ) )
                {
                    var item = db.T_DetaxNew.Where( w => w.Anno_T_DetaxNew.Equals( anno ) && w.Matricola_T_DetaxNew.Equals( matricola ) && w.CodiceDetassazione_T_DetaxNew.Equals( codiceDetassazione ) ).FirstOrDefault( );

                    if ( item != null )
                    {
                        result = item.PDF_T_DetaxNew;
                    }
                }
            }
            catch ( Exception ex )
            {
                throw new Exception( ex.Message );
            }

            if (result == null)
            {
                return null;
            }
            return new FileContentResult( result , "application/pdf" );
        }

        public ActionResult ConfermaPasswordERinuncia(string pwd, int anno, string codiceDetassazione)
        {
            string result = "OK";
            string matricola = CommonHelper.GetCurrentUserMatricola( );

            try
            {
                using ( PrincipalContext pc = new PrincipalContext( ContextType.Domain , "RAI" ) )
                {
                    try
                    {
                        bool valido = pc.ValidateCredentials( CommonHelper.GetCurrentUserPMatricola( ) , pwd );
                        if (!valido)
                        {
                            result = "Password errata.";
                        }
                    }
                    catch ( Exception ex )
                    {
                        result = ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            if ( result.Equals( "OK" ) )
            {
                DateTime dataNascita = UtenteHelper.EsponiAnagrafica( )._dataNascita.GetValueOrDefault( );
                string dNascita = "";

                if (dataNascita.Equals(DateTime.MinValue) ||
                    dataNascita <= DateTime.MinValue)
                {
                    dNascita = "";
                }
                else
                {
                    dNascita = dataNascita.ToString( "dd/MM/yyyy" );
                }

                try
                {
                    string nome = UtenteHelper.EsponiAnagrafica( )._nome;
                    string cognome = UtenteHelper.EsponiAnagrafica( )._cognome;
                    string nominativo = String.Format( "{0} {1}" , UtenteHelper.EsponiAnagrafica( )._nome , UtenteHelper.EsponiAnagrafica( )._cognome );

                    // controllo che per lo più è per evitare errori di nominativi vuoti su sviluppo
                    if (String.IsNullOrEmpty( nome ) || String.IsNullOrEmpty( cognome ) )
                    {
                        using ( HRPADBEntities nomeDb = new HRPADBEntities( ) )
                        {
                            var item = nomeDb.T_DetaxNew.Where( w => w.Matricola_T_DetaxNew.Equals( matricola ) ).FirstOrDefault( );
                            if (item != null)
                            {
                                nominativo = item.Nominativo_T_DetaxNew.Trim( );
                            }
                        }
                    }

                    // creazione del pdf
                    byte[] myArrayOfBytes = CreazionePDF( matricola , nominativo,
                                                        UtenteHelper.EsponiAnagrafica( )._cf ,
                                                        UtenteHelper.EsponiAnagrafica( )._comuneNascita ,
                                                        dNascita ,
                                                        UtenteHelper.EsponiAnagrafica( )._genere );

                    if ( myArrayOfBytes == null)
                    {
                        result = "Errore nella creazione del PDF";
                        throw new Exception( result );
                    }

                    // salvataggio dati sul db
                    using ( HRPADBEntities db = new HRPADBEntities( ) )
                    {
                        var exists = db.T_DetaxNew.Where( w => w.Matricola_T_DetaxNew.Equals( matricola ) ).FirstOrDefault( );

                        if (exists != null && !exists.Data_T_DetaxNew.HasValue)
                        {
                            exists.Data_T_DetaxNew = DateTime.Now;
                            exists.Modello_T_DetaxNew = "1C"; // codice del modello di rinuncia
                            exists.Scelta_T_DetaxNew = "0";   // il modello non prevede scelte quindi di default è 0
                            exists.PDF_T_DetaxNew = myArrayOfBytes;
                            exists.CodiceDetassazione_T_DetaxNew = "DETAX";
                            db.SaveChanges( );
                        }
                        else
                        {
                            throw new Exception( "Scelta già effettuata" );
                        }
                    }

                    SetSceltaDetassazioneResponse risposta = new SetSceltaDetassazioneResponse( );

                    try
                    {
                        ScriviLogAzione( CommonHelper.GetCurrentUserMatricola( ) , "ConfermaPasswordERinuncia" , "1C - 0" );

                        // chiamata CICS
                        MyRaiService1Client servizioWCF = new MyRaiService1Client( );
                        risposta = servizioWCF.SetSceltaDetassazione( CommonHelper.GetCurrentUserPMatricola( ) , CommonHelper.GetCurrentUserMatricola( ) , DateTime.Now , "1C" , 0 );
                    }
                    catch(Exception ex)
                    {
                        if (risposta == null)
                        {
                            risposta = new SetSceltaDetassazioneResponse( )
                            {
                                Esito = false,
                                Errore = ex.Message
                            };
                        }
                        else if (String.IsNullOrEmpty(risposta.Errore))
                        {
                            risposta = new SetSceltaDetassazioneResponse( )
                            {
                                Esito = false ,
                                Errore = ex.Message
                            };
                        }
                    }

                    // se esito false
                    // rollback
                    if ( !risposta.Esito || result != "OK")
                    {
                        try
                        {
                            if ( result != "OK" )
                            {
                                ScriviLogAzione( CommonHelper.GetCurrentUserMatricola( ) , "ConfermaPasswordERinuncia" , "Errore Rollback - " + result );
                            }
                            else
                            {
                                ScriviLogAzione( CommonHelper.GetCurrentUserMatricola( ) , "ConfermaPasswordERinuncia" , "Errore Rollback - " + risposta.Errore );
                            }
                            
                            using ( HRPADBEntities db = new HRPADBEntities( ) )
                            {
                                var item = db.T_DetaxNew.Where( w => w.Anno_T_DetaxNew.Equals( anno ) && w.Matricola_T_DetaxNew.Equals( matricola ) && w.CodiceDetassazione_T_DetaxNew.Equals( codiceDetassazione ) ).FirstOrDefault( );

                                if ( item != null )
                                {
                                    item.Data_T_DetaxNew = null;
                                    item.Modello_T_DetaxNew = null;
                                    item.PDF_T_DetaxNew = null;
                                    item.Scelta_T_DetaxNew = null;
                                    db.SaveChanges( );
                                    result = risposta.Errore;
                                    if ( String.IsNullOrEmpty( risposta.Errore ) )
                                    {
                                        throw new Exception( "Si è verificato un errore. Impossibile continuare con la scelta." );
                                    }
                                    else
                                    {
                                        throw new Exception( risposta.Errore );
                                    }
                                }
                            }
                        }
                        catch ( Exception ex )
                        {
                            throw new Exception( ex.Message );
                        }
                    }

                    if (result == "OK")
                    {
                        // se tutto ok
                        // invia mail con la copia del modulo compilato
                        InvioMail( matricola , String.Format( "{0} {1}" , UtenteHelper.EsponiAnagrafica( )._nome , UtenteHelper.EsponiAnagrafica( )._cognome ) , myArrayOfBytes , myRaiCommonTasks.CommonTasks.EnumParametriSistema.MailTemplateModuloDetassazione1C );
                    }
                }
                catch (Exception ex)
                {
                    result = ex.Message;
                }
            }

            ScriviLogAzione( CommonHelper.GetCurrentUserMatricola( ) , "ConfermaPasswordERinuncia" , result );

            return Content( result );
        }

        public ActionResult ConfermaPasswordSalva2C ( string pwd , int anno , string codiceDetassazione, int scelta )
        {
            string result = "OK";
            string matricola = CommonHelper.GetCurrentUserMatricola( );

            try
            {
                using ( PrincipalContext pc = new PrincipalContext( ContextType.Domain , "RAI" ) )
                {
                    try
                    {
                        bool valido = pc.ValidateCredentials( CommonHelper.GetCurrentUserPMatricola( ) , pwd );
                        if ( !valido )
                        {
                            result = "Password errata.";
                        }
                    }
                    catch ( Exception ex )
                    {
                        result = ex.Message;
                    }
                }
            }
            catch ( Exception ex )
            {
                result = ex.Message;
            }

            if ( result.Equals( "OK" ) )
            {
                DateTime dataNascita = UtenteHelper.EsponiAnagrafica( )._dataNascita.GetValueOrDefault( );
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

                try
                {
                    ScriviLogAzione( CommonHelper.GetCurrentUserMatricola( ) , "ConfermaPasswordSalva2C" , "2C - " + scelta.ToString( ) );

                    string nome = UtenteHelper.EsponiAnagrafica( )._nome;
                    string cognome = UtenteHelper.EsponiAnagrafica( )._cognome;
                    string nominativo = String.Format( "{0} {1}" , UtenteHelper.EsponiAnagrafica( )._nome , UtenteHelper.EsponiAnagrafica( )._cognome );

                    // controllo che per lo più è per evitare errori di nominativi vuoti su sviluppo
                    if ( String.IsNullOrEmpty( nome ) || String.IsNullOrEmpty( cognome ) )
                    {
                        using ( HRPADBEntities nomeDb = new HRPADBEntities( ) )
                        {
                            var item = nomeDb.T_DetaxNew.Where( w => w.Matricola_T_DetaxNew.Equals( matricola ) ).FirstOrDefault( );
                            if ( item != null )
                            {
                                nominativo = item.Nominativo_T_DetaxNew.Trim( );
                            }
                        }
                    }

                    // creazione del pdf
                    byte[] myArrayOfBytes = CreazionePDF2C( matricola , nominativo ,
                                                        UtenteHelper.EsponiAnagrafica( )._cf ,
                                                        UtenteHelper.EsponiAnagrafica( )._comuneNascita ,
                                                        dNascita ,
                                                        UtenteHelper.EsponiAnagrafica( )._genere ,
                                                        scelta );

                    if ( myArrayOfBytes == null )
                    {
                        result = "Errore nella creazione del PDF";
                        throw new Exception( result );
                    }

                    // salvataggio dati sul db
                    using ( HRPADBEntities db = new HRPADBEntities( ) )
                    {
                        var exists = db.T_DetaxNew.Where( w => w.Matricola_T_DetaxNew.Equals( matricola ) ).FirstOrDefault( );

                        if ( exists != null && !exists.Data_T_DetaxNew.HasValue )
                        {
                            exists.Data_T_DetaxNew = DateTime.Now;
                            exists.Modello_T_DetaxNew = "2C"; // codice del modello di rinuncia
                            exists.Scelta_T_DetaxNew = scelta.ToString( );
                            exists.PDF_T_DetaxNew = myArrayOfBytes;
                            exists.CodiceDetassazione_T_DetaxNew = "DETAX";
                            db.SaveChanges( );
                        }
                        else
                        {
                            throw new Exception( "Scelta già effettuata" );
                        }
                    }

                    SetSceltaDetassazioneResponse risposta = new SetSceltaDetassazioneResponse( );

                    try
                    {
                        // chiamata CICS
                        MyRaiService1Client servizioWCF = new MyRaiService1Client( );
                        risposta = servizioWCF.SetSceltaDetassazione( CommonHelper.GetCurrentUserPMatricola( ) , CommonHelper.GetCurrentUserMatricola( ) , DateTime.Now , "2C" , scelta );
                    }
                    catch ( Exception ex )
                    {
                        if ( risposta == null )
                        {
                            risposta = new SetSceltaDetassazioneResponse( )
                            {
                                Esito = false ,
                                Errore = ex.Message
                            };
                        }
                        else if ( String.IsNullOrEmpty( risposta.Errore ) )
                        {
                            risposta = new SetSceltaDetassazioneResponse( )
                            {
                                Esito = false ,
                                Errore = ex.Message
                            };
                        }
                    }

                    // se esito false
                    // rollback
                    if ( !risposta.Esito || result != "OK" )
                    {
                        try
                        {
                            if ( result != "OK" )
                            {
                                ScriviLogAzione( CommonHelper.GetCurrentUserMatricola( ) , "ConfermaPasswordSalva2C" , "Errore Rollback - " + result );
                            }
                            else
                            {
                                ScriviLogAzione( CommonHelper.GetCurrentUserMatricola( ) , "ConfermaPasswordSalva2C" , "Errore Rollback - " + risposta.Errore );
                            }

                            using ( HRPADBEntities db = new HRPADBEntities( ) )
                            {
                                var item = db.T_DetaxNew.Where( w => w.Anno_T_DetaxNew.Equals( anno ) && w.Matricola_T_DetaxNew.Equals( matricola ) && w.CodiceDetassazione_T_DetaxNew.Equals( codiceDetassazione ) ).FirstOrDefault( );

                                if ( item != null )
                                {
                                    item.Data_T_DetaxNew = null;
                                    item.Modello_T_DetaxNew = null;
                                    item.PDF_T_DetaxNew = null;
                                    item.Scelta_T_DetaxNew = null;
                                    db.SaveChanges( );
                                    result = risposta.Errore;
                                    if (String.IsNullOrEmpty(risposta.Errore))
                                    {
                                        throw new Exception( "Si è verificato un errore. Impossibile continuare con la scelta." );
                                    }
                                    else
                                    {
                                        throw new Exception( risposta.Errore );
                                    }
                                }
                            }
                        }
                        catch ( Exception ex )
                        {
                            throw new Exception( ex.Message );
                        }
                    }

                    if ( result == "OK" )
                    {
                        // se tutto ok
                        // invia mail con la copia del modulo compilato
                        InvioMail( matricola , String.Format( "{0} {1}" , UtenteHelper.EsponiAnagrafica( )._nome , UtenteHelper.EsponiAnagrafica( )._cognome ) , myArrayOfBytes , myRaiCommonTasks.CommonTasks.EnumParametriSistema.MailTemplateModuloDetassazione2C );
                    }
                }
                catch ( Exception ex )
                {
                    result = ex.Message;
                }
            }
            ScriviLogAzione( CommonHelper.GetCurrentUserMatricola( ) , "ConfermaPasswordSalva2C" , result );
            return Content( result );
        }
        #endregion
        
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

        private static byte[] CreazionePDF ( string matricola , string nominativo , string cf , string luogo , string data , string sesso )
        {
            try
            {
                var cultureInfo = CultureInfo.GetCultureInfo( "it-IT" );

                byte[] bytes = null;
                const int fontIntestazione = 12;
                const int fontCorpo = 10;
                const int fontSize = 9;

                int border = 0; // none

                BaseFont bf = BaseFont.CreateFont( BaseFont.HELVETICA , BaseFont.CP1250 , BaseFont.NOT_EMBEDDED );
                Font myFontIntestazione = new Font( bf , fontIntestazione , Font.NORMAL );
                Font myFontIntestazioneBold = new Font( bf , fontIntestazione , Font.BOLD );
                Font myFontCorpo = new Font( bf , fontCorpo , Font.NORMAL );
                Font myFont = new Font( bf , fontSize , Font.NORMAL );
                Font myFontBold = new Font( bf , fontSize , Font.BOLD );
                Font myFontBoldMini = new Font( bf , 7 , Font.BOLD );
                Font myFontBoldDisclaimer = new Font( bf , 6 , Font.BOLD );

                using ( MemoryStream ms = new MemoryStream( ) )
                {
                    // Creazione dell'istanza del documento, impostando tipo di pagina e margini
                    Document document = new Document( PageSize.A4 , 25 , 25 , 25 , 25 );
                    PdfWriter writer = PdfWriter.GetInstance( document , ms );

                    document.Open( );

                    PdfContentByte cb = writer.DirectContent;
                    PdfPTable tableRighe = new PdfPTable( 2 );
                    tableRighe.DefaultCell.BorderWidth = 0;
                    tableRighe.TotalWidth = document.PageSize.Width - 50;
                    tableRighe.LockedWidth = true;
                    int[] rowsWidths = new int[] { 250 , 250 };
                    tableRighe.SetWidths( rowsWidths );
                    tableRighe.AddCell( WriteCell( "RINUNCIA ALL’APPLICAZIONE DELL’IMPOSTA SOSTITUTIVA DEL 10% SUL PREMIO DI RISULTATO PER L’ANNO 2018" , border , 2 , 1 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( " " , border , 2 , 1 , myFontIntestazione ) );
                    Phrase phrase = new Phrase( );
                    phrase.Add( new Chunk( "Matricola: " , myFontIntestazione ) );
                    phrase.Add( new Chunk( matricola , myFontIntestazioneBold ) );
                    tableRighe.AddCell( WriteCell( phrase , border , 2 , 0 ) );
                    tableRighe.AddCell( WriteCell( " " , border , 2 , 1 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( " " , border , 2 , 1 , myFontIntestazione ) );
                    string tx = "";
                    phrase = new Phrase( );

                    if ( sesso.ToUpper( ).Equals( "F" ) )
                    {
                        phrase.Add( new Chunk( "La sottoscritta " , myFontIntestazione ) );
                        phrase.Add( new Chunk( nominativo , myFontIntestazioneBold ) );
                        phrase.Add( new Chunk( " nata a " , myFontIntestazione ) );
                        phrase.Add( new Chunk( luogo , myFontIntestazioneBold ) );
                        phrase.Add( new Chunk( " il " , myFontIntestazione ) );
                        phrase.Add( new Chunk( data , myFontIntestazioneBold ) );
                        phrase.Add( new Chunk( " Codice Fiscale " , myFontIntestazione ) );
                        phrase.Add( new Chunk( cf , myFontIntestazioneBold ) );
                        phrase.Add( new Chunk( " dipendente dell’azienda Rai/Rai Cinema/Rai Way/Rai Com " , myFontIntestazione ) );
                    }
                    else
                    {
                        phrase.Add( new Chunk( "Il sottoscritto " , myFontIntestazione ) );
                        phrase.Add( new Chunk( nominativo , myFontIntestazioneBold ) );
                        phrase.Add( new Chunk( " nato a " , myFontIntestazione ) );
                        phrase.Add( new Chunk( luogo , myFontIntestazioneBold ) );
                        phrase.Add( new Chunk( " il " , myFontIntestazione ) );
                        phrase.Add( new Chunk( data , myFontIntestazioneBold ) );
                        phrase.Add( new Chunk( " Codice Fiscale " , myFontIntestazione ) );
                        phrase.Add( new Chunk( cf , myFontIntestazioneBold ) );
                        phrase.Add( new Chunk( " dipendente dell’azienda Rai/Rai Cinema/Rai Way/Rai Com " , myFontIntestazione ) );
                    }
                    tableRighe.AddCell( WriteCell( phrase , border , 2 , 0 ) );
                    tableRighe.AddCell( WriteCell( " " , border , 2 , 1 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( "RINUNCIA" , border , 2 , 1 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( " " , border , 2 , 1 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( "all’applicazione dell’imposta sostitutiva del 10% sulle somme corrisposte nell’anno 2019 a titolo di premio di risultato anno 2018 e chiede, pertanto, che sia applicata sul premio in questione la tassazione ordinaria." , border , 2 , 0 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( " " , border , 2 , 1 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( " " , border , 2 , 1 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( "Data " + DateTime.Now.ToString( "dd/MM/yyyy" ) , border , 2 , 0 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( " " , border , 2 , 0 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( " " , border , 2 , 1 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( " " , border , 1 , 0 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( "Firma" , border , 1 , 1 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( " " , border , 1 , 0 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( nominativo , border , 1 , 1 , myFontIntestazione ) );
                    tableRighe.WriteSelectedRows( 0 , tableRighe.Rows.Count , 25 , 780 , cb );
                    tableRighe.FlushContent( );
                    tableRighe.DeleteBodyRows( );

                    document.Close( );
                    writer.Close( );
                    bytes = ms.ToArray( );
                    return bytes;
                }
            }
            catch ( Exception ex )
            {
                throw new Exception( ex.Message );
            }
        }

        private static byte[] CreazionePDF2C ( string matricola , string nominativo , string cf , string luogo , string data , string sesso , int scelta )
        {
            try
            {
                var cultureInfo = CultureInfo.GetCultureInfo( "it-IT" );

                byte[] bytes = null;
                const int fontIntestazione = 12;
                const int fontCorpo = 10;
                const int fontSize = 9;

                int border = 0; // none

                BaseFont bf = BaseFont.CreateFont( BaseFont.HELVETICA , BaseFont.CP1250 , BaseFont.NOT_EMBEDDED );
                Font myFontIntestazione = new Font( bf , fontIntestazione , Font.NORMAL );
                Font myFontIntestazioneBold = new Font( bf , fontIntestazione , Font.BOLD );
                Font myFontCorpo = new Font( bf , fontCorpo , Font.NORMAL );
                Font myFont = new Font( bf , fontSize , Font.NORMAL );
                Font myFontBold = new Font( bf , fontSize , Font.BOLD );
                Font myFontBoldMini = new Font( bf , 7 , Font.BOLD );
                Font myFontBoldDisclaimer = new Font( bf , 6 , Font.BOLD );

                using ( MemoryStream ms = new MemoryStream( ) )
                {
                    // Creazione dell'istanza del documento, impostando tipo di pagina e margini
                    Document document = new Document( PageSize.A4 , 25 , 25 , 25 , 25 );
                    PdfWriter writer = PdfWriter.GetInstance( document , ms );

                    document.Open( );

                    PdfContentByte cb = writer.DirectContent;
                    PdfPTable tableRighe = new PdfPTable( 3 );
                    tableRighe.DefaultCell.BorderWidth = 0;
                    tableRighe.TotalWidth = document.PageSize.Width - 50;
                    tableRighe.LockedWidth = true;
                    int[] rowsWidths = new int[] { 25 , 225 , 250 };
                    tableRighe.SetWidths( rowsWidths );
                    tableRighe.AddCell( WriteCell( "RICHIESTA DI APPLICAZIONE DELL’IMPOSTA SOSTITUTIVA DEL 10% SUL PREMIO DI RISULTATO PER L’ANNO 2018" , border , 3 , 1 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( " " , border , 3 , 1 , myFontIntestazione ) );
                    Phrase phrase = new Phrase( );
                    phrase.Add( new Chunk( "Matricola: " , myFontIntestazione ) );
                    phrase.Add( new Chunk( matricola , myFontIntestazioneBold ) );
                    tableRighe.AddCell( WriteCell( phrase , border , 3 , 0 ) );
                    tableRighe.AddCell( WriteCell( " " , border , 3 , 1 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( " " , border , 3 , 1 , myFontIntestazione ) );

                    phrase = new Phrase( );

                    if ( sesso.ToUpper( ).Equals( "F" ) )
                    {
                        phrase.Add( new Chunk( "La sottoscritta " , myFontIntestazione ) );
                        phrase.Add( new Chunk( nominativo , myFontIntestazioneBold ) );
                        phrase.Add( new Chunk( " nata a " , myFontIntestazione ) );
                        phrase.Add( new Chunk( luogo , myFontIntestazioneBold ) );
                        phrase.Add( new Chunk( " il " , myFontIntestazione ) );
                        phrase.Add( new Chunk( data , myFontIntestazioneBold ) );
                        phrase.Add( new Chunk( " Codice Fiscale " , myFontIntestazione ) );
                        phrase.Add( new Chunk( cf , myFontIntestazioneBold ) );
                        phrase.Add( new Chunk( " dipendente dell’azienda Rai/Rai Cinema/Rai Way/Rai Com " , myFontIntestazione ) );
                    }
                    else
                    {
                        phrase.Add( new Chunk( "Il sottoscritto " , myFontIntestazione ) );
                        phrase.Add( new Chunk( nominativo , myFontIntestazioneBold ) );
                        phrase.Add( new Chunk( " nato a " , myFontIntestazione ) );
                        phrase.Add( new Chunk( luogo , myFontIntestazioneBold ) );
                        phrase.Add( new Chunk( " il " , myFontIntestazione ) );
                        phrase.Add( new Chunk( data , myFontIntestazioneBold ) );
                        phrase.Add( new Chunk( " Codice Fiscale " , myFontIntestazione ) );
                        phrase.Add( new Chunk( cf , myFontIntestazioneBold ) );
                        phrase.Add( new Chunk( " dipendente dell’azienda Rai/Rai Cinema/Rai Way/Rai Com " , myFontIntestazione ) );
                    }
                    tableRighe.AddCell( WriteCell( phrase , border , 3 , 0 ) );
                    tableRighe.AddCell( WriteCell( " " , border , 3 , 1 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( "DICHIARA" , border , 3 , 1 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( " " , border , 3 , 1 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( "sotto la propria responsabilita'," , border , 3 , 0 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( " " , border , 3 , 1 , myFontIntestazione ) );
                    tableRighe.AddCell( writeCellTab( cb , border , 1 , 1 , ( scelta == 1 ) ) );
                    tableRighe.AddCell( WriteCell( "che nell'anno 2018 non ha percepito redditi di lavoro dipendente (inclusi gli eventuali premi di risultato assoggettati al 10% riportati al punto 572 della Certificazione Unica 2019 per i redditi 2018 ed eventuali indennita' di disoccupazione, di maternita', di malattia, ecc) e/o redditi da pensione da altro sostituto d'imposta diverso da Rai/Rai Cinema/Rai Way/Rai Com " , border , 2 , 0 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( " " , border , 3 , 1 , myFontIntestazione ) );

                    tableRighe.AddCell( WriteCell( "ovvero," , border , 3 , 0 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( " " , border , 3 , 1 , myFontIntestazione ) );
                    tableRighe.AddCell( writeCellTab( cb , border , 1 , 1 , ( scelta == 2 ) ) );
                    tableRighe.AddCell( WriteCell( "che nell'anno 2018 ha percepito redditi di lavoro dipendente (inclusi gli eventuali premi di risultato assoggettati al 10% riportati al punto 572 della Certificazione Unica 2019 per i redditi 2018), e/o indennita' sostitutive (come ad esempio indennita' di disoccupazione, di maternita', di malattia, ecc) e/o redditi da pensione anche da altri sostituti d'imposta per un importo che sommato ai redditi percepiti da Rai/Rai Cinema/Rai Way/Rai Com non è superiore a euro 80.000." , border , 2 , 0 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( " " , border , 3 , 1 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( " " , border , 3 , 1 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( "Data " + DateTime.Now.ToString( "dd/MM/yyyy" ) , border , 3 , 0 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( " " , border , 3 , 0 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( " " , border , 2 , 0 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( "Firma" , border , 1 , 1 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( " " , border , 2 , 0 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( nominativo , border , 1 , 1 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( " " , border , 3 , 0 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( " " , border , 3 , 0 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( " " , border , 3 , 0 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( "N.B.: eventuali redditi erogati da Rai Pubblicita' sono da considerare come redditi erogati da altro sostituto d'imposta." , border , 3 , 0 , myFontIntestazione ) );
                    tableRighe.WriteSelectedRows( 0 , tableRighe.Rows.Count , 25 , 780 , cb );
                    tableRighe.FlushContent( );
                    tableRighe.DeleteBodyRows( );

                    document.Close( );
                    writer.Close( );
                    bytes = ms.ToArray( );
                    return bytes;
                }
            }
            catch ( Exception ex )
            {
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
        private void InvioMail ( string matricola , string nomeUtente, byte[] pdf, myRaiCommonTasks.CommonTasks.EnumParametriSistema template )
        {
            try
            {
                string[] MailParams = myRaiCommonTasks.CommonTasks.GetParametri<string>( template );

                string body = MailParams[0];
                string MailSubject = MailParams[1];

                string[] mailParams2 = myRaiCommonTasks.CommonTasks.GetParametri<string>( myRaiCommonTasks.CommonTasks.EnumParametriSistema.MailTemplateModuloDetassazioneFrom );
                string from = mailParams2[0];

                string dest = myRaiCommonTasks.CommonTasks.GetEmailPerMatricola( matricola.TrimStart( 'P' ) );

                if ( String.IsNullOrWhiteSpace( dest ) )
                    dest = matricola + "@rai.it";

                GestoreMail mail = new GestoreMail( );

                //dest = "RUO.SIP.PRESIDIOOPEN@rai.it";

                body = body.Replace( "#DATA#" , DateTime.Now.ToString( "dd/MM/yyyy" ) );

                List<Attachement> attachments = new List<Attachement>( );

                Attachement a = new Attachement( )
                {
                    AttachementName = "Modulo.pdf" ,
                    AttachementType = "Application/PDF" ,
                    AttachementValue = pdf
                };

                attachments.Add( a );

                var response = mail.InvioMail( body , MailSubject , dest , "raiplace.selfservice@rai.it" , from  , attachments);

                if ( response != null && response.Errore != null )
                {
                    myRaiData.MyRai_LogErrori err = new MyRai_LogErrori( )
                    {
                        applicativo = "Portale" ,
                        data = DateTime.Now ,
                        provenienza = "Detassazione - InvioMail" ,
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
                myRaiData.MyRai_LogErrori err = new MyRai_LogErrori( )
                {
                    applicativo = "Portale" ,
                    data = DateTime.Now ,
                    provenienza = "Rifornimenti - InvioMail" ,
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