using System;
using System.Linq;
using System.Web.Mvc;
using myRaiData;
using MyRaiServiceInterface.MyRaiServiceReference1;
using myRaiHelper;
using myRaiCommonModel;
using System.Collections.Generic;
using myRaiDataTalentia;
using System.Diagnostics;
using myRaiCommonTasks;
using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Globalization;
using System.IO;
using myRaiCommonTasks.sendMail;
using myRai.Business;
using System.ServiceModel;
using System.Text;
using Utente = myRai.Models.Utente;
using Logger = myRaiHelper.Logger;

namespace myRai.Controllers
{
    public class ModuliController : BaseCommonController
    {
        #region MODULO SMARTWORKING

        public ActionResult RicaricaWidget ( )
        {
            SectionDayModel model = new SectionDayModel( );
            List<WidgetModuloBox_Azione> btns = new List<WidgetModuloBox_Azione>( );

            bool giaScelto = false;
            string sceltaSmart2020 = "";
            DateTime? dataCompilazioneSW2020 = null;

            // verifica se l'utente ha già effettuato la scelta
            string mt = CommonHelper.GetCurrentUserMatricola( );
            using ( TalentiaEntities dbTalentia = new TalentiaEntities( ) )
            {
                bool exist = dbTalentia.XR_MOD_DIPENDENTI.Count( w => w.MATRICOLA.Equals( mt ) && w.COD_MODULO.Equals( "SMARTW2020" ) ) > 0;

                if ( !exist )
                {
                    giaScelto = false;
                }
                else
                {
                    var items = dbTalentia.XR_MOD_DIPENDENTI.Where( w => w.MATRICOLA.Equals( mt ) && w.COD_MODULO.Equals( "SMARTW2020" ) ).ToList( );

                    if ( items != null && items.Any( ) )
                    {
                        DateTime? dtComp = items.First( ).DATA_COMPILAZIONE;
                        dataCompilazioneSW2020 = dtComp;
                        giaScelto = true;
                    }
                }
            }

            if ( giaScelto )
            {
                btns.Add( new WidgetModuloBox_Azione( )
                {
                    TestoBottone = "Visualizza" ,
                    UrlBottone = Url.Action( "VisualizzaModuloSmartWorking" , "Moduli" , new { codiceModulo = "SMARTW2020" } )
                } );

                model.SmartWorkingWidget = new WidgetModuloBox( )
                {
                    WidgetId = "WdgSmartWorking" ,
                    Anno = DateTime.Now.Year ,
                    GiaScelto = true ,
                    HaDiritto = true ,
                    Titolo = "Modulo per esercizio diritto smart working" ,
                    Scelta = sceltaSmart2020 ,
                    DataCompilazione = dataCompilazioneSW2020 ,
                    Sottotitolo = "Modulo compilato in data " + dataCompilazioneSW2020.GetValueOrDefault( ).ToString( "dd/MM/yyyy" ) ,
                    Bottoni = btns
                };
            }
            else
            {
                btns.Add( new WidgetModuloBox_Azione( )
                {
                    TestoBottone = "Compila" ,
                    UrlBottone = Url.Action( "CompilaModuloSmartWorking" , "Moduli" , new { codiceModulo = "SMARTW2020" } )
                } );

                model.SmartWorkingWidget = new WidgetModuloBox( )
                {
                    WidgetId = "WdgSmartWorking" ,
                    Anno = DateTime.Now.Year ,
                    GiaScelto = giaScelto ,
                    HaDiritto = true ,
                    Titolo = "Modulo per esercizio diritto smart working" ,
                    Scelta = sceltaSmart2020 ,
                    DataCompilazione = null ,
                    Bottoni = btns
                };
            }
            return View( "~/Views/Scrivania/subpartial/boxModulo.cshtml" , model.SmartWorkingWidget );
        }

        private string GetSceltaEnumByDescription ( string sceltaDescription )
        {
            // sceltaDescription es: LavoratoreDisabile

            ModuloSmart2020SelectionEnum myEnum = ( ModuloSmart2020SelectionEnum ) Enum.Parse( typeof( ModuloSmart2020SelectionEnum ) , sceltaDescription );
            int val = ( int ) myEnum;

            return val.ToString( );
        }

        private string GetSceltaEnumByDescriptionOLD ( string sceltaDescription )
        {
            // sceltaDescription es: LavoratoreDisabile

            ModuloSmart2020SelectionEnumOLD myEnum = ( ModuloSmart2020SelectionEnumOLD ) Enum.Parse( typeof( ModuloSmart2020SelectionEnumOLD ) , sceltaDescription );
            int val = ( int ) myEnum;

            return val.ToString( );
        }

        public ActionResult CompilaModuloSmartWorking ( string codiceModulo )
        {
            ModuloVM model = new ModuloVM( );
            model.WidgetId = "SmartWorkingWidget";
            model.Matricola = CommonManager.GetCurrentUserMatricola( );
            model.Nominativo = String.Format( "{0} {1}" , Utente.EsponiAnagrafica( )._nome , Utente.EsponiAnagrafica( )._cognome );
            model.Sesso = Utente.EsponiAnagrafica( )._genere;

            return View( "~/Views/Moduli/ModuloSmartWorking.cshtml" , model );
        }

        public ActionResult VisualizzaModuloSmartWorking ( string codiceModulo )
        {

            ModuloVM model = new ModuloVM( );
            model.WidgetId = "SmartWorkingWidget";
            model.Matricola = CommonManager.GetCurrentUserMatricola( );
            model.Nominativo = String.Format( "{0} {1}" , Utente.EsponiAnagrafica( )._nome , Utente.EsponiAnagrafica( )._cognome );
            model.Sesso = Utente.EsponiAnagrafica( )._genere;
            model.CodiceModulo = "SMARTW2020";

            DateTime? dataCompilazioneSW2020 = null;

            // verifica se l'utente ha già effettuato la scelta
            string mt = CommonManager.GetCurrentUserMatricola( );
            using ( TalentiaEntities dbTalentia = new TalentiaEntities( ) )
            {
                bool exist = dbTalentia.XR_MOD_DIPENDENTI.Count( w => w.MATRICOLA.Equals( mt ) && w.COD_MODULO.Equals( "SMARTW2020" ) ) > 0;

                if ( !exist )
                {
                    return null;
                }
                else
                {
                    var items = dbTalentia.XR_MOD_DIPENDENTI.Where( w => w.MATRICOLA.Equals( mt ) && w.COD_MODULO.Equals( "SMARTW2020" ) ).ToList( );

                    if ( items != null && items.Any( ) )
                    {
                        DateTime? dtComp = items.First( ).DATA_COMPILAZIONE;
                        dataCompilazioneSW2020 = dtComp;

                        //foreach ( var item in items )
                        //{
                        //    string s = "";
                        //    string sel = "";

                        //    if ( item.DATA_COMPILAZIONE.GetValueOrDefault( ) <= new DateTime( 2020 , 11 , 24 ) )
                        //    {
                        //        s = GetSceltaEnumByDescriptionOLD( item.SCELTA );
                        //    }
                        //    else
                        //    {
                        //        // se in scelta c'è # allora ci sono delle date
                        //        if ( item.SCELTA.Contains( "#" ) )
                        //        {
                        //            sel = item.SCELTA.Split( '#' )[0];
                        //            var dateDaSeparare = item.SCELTA.Split( '#' )[1];
                        //            s = GetSceltaEnumByDescription( sel );

                        //            // se la scelta è la 7 allora vanno valorizzate le date di 
                        //            if ( s == "7" )
                        //            {
                        //                var date = dateDaSeparare.Split( '-' ).ToList( );
                        //                DateTime dt1 = DateTime.MinValue;
                        //                DateTime dt2 = DateTime.MinValue;
                        //                DateTime.TryParseExact( date[0] , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out dt1 );
                        //                DateTime.TryParseExact( date[1] , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out dt2 );
                        //                model.Scelta7_Dal = dt1;
                        //                model.Scelta7_Al = dt2;
                        //            }

                        //            if ( s == "8" )
                        //            {
                        //                var date = dateDaSeparare.Split( '-' ).ToList( );
                        //                DateTime dt1 = DateTime.MinValue;
                        //                DateTime dt2 = DateTime.MinValue;
                        //                DateTime.TryParseExact( date[0] , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out dt1 );
                        //                DateTime.TryParseExact( date[1] , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out dt2 );
                        //                model.Scelta8_Dal = dt1;
                        //                model.Scelta8_Al = dt2;
                        //            }
                        //        }
                        //        else
                        //        {
                        //            s = GetSceltaEnumByDescription( item.SCELTA );
                        //        }
                        //    }

                        //    if (s == "1")
                        //    {
                        //        model.Scelta1 = true;
                        //    }

                        //    if ( s == "2" )
                        //    {
                        //        model.Scelta2 = true;
                        //    }

                        //    if ( s == "3" )
                        //    {
                        //        model.Scelta3 = true;
                        //    }

                        //    if ( s == "4" )
                        //    {
                        //        model.Scelta4 = true;
                        //    }

                        //    if ( s == "5" )
                        //    {
                        //        model.Scelta5 = true;
                        //    }

                        //    if ( s == "6" )
                        //    {
                        //        model.Scelta6 = true;
                        //    }

                        //    if ( s == "7" )
                        //    {
                        //        model.Scelta7 = true;
                        //    }

                        //    if ( s == "8" )
                        //    {
                        //        model.Scelta8 = true;
                        //    }

                        //    if ( s == "71" )
                        //    {
                        //        model.Scelta7_1 = true;
                        //    }

                        //    if ( s == "72" )
                        //    {
                        //        model.Scelta7_2 = true;
                        //    }

                        //    if ( s == "73" )
                        //    {
                        //        model.Scelta7_3 = true;
                        //    }

                        //    if ( s == "74" )
                        //    {
                        //        model.Scelta7_4 = true;
                        //    }

                        //    if ( s == "75" )
                        //    {
                        //        model.Scelta7_5 = true;
                        //    }

                        //    if ( s == "76" )
                        //    {
                        //        model.Scelta7_6 = true;
                        //    }

                        //    if ( s == "77" )
                        //    {
                        //        model.Scelta7_7 = true;
                        //    }

                        //    if ( s == "78" )
                        //    {
                        //        model.Scelta7_8 = true;
                        //    }

                        //    if ( s == "81" )
                        //    {
                        //        model.Scelta8_1 = true;
                        //    }

                        //    if ( s == "82" )
                        //    {
                        //        model.Scelta8_2 = true;
                        //    }

                        //    if ( s == "83" )
                        //    {
                        //        model.Scelta8_3 = true;
                        //    }

                        //    if ( s == "84" )
                        //    {
                        //        model.Scelta8_4 = true;
                        //    }

                        //    if ( s == "85" )
                        //    {
                        //        model.Scelta8_5 = true;
                        //    }
                        //}
                    }
                }
            }

            model.DataCompilazione = dataCompilazioneSW2020;

            return View( "~/Views/Moduli/ModuloSmartWorkingReadOnly.cshtml" , model );
        }

        public ActionResult SalvaScelta ( string scelta, string dateSelezionate )
        {
            string result = "OK";
            DateTime dataCompilazione = DateTime.Now;
            try
            {
                // se tutto ok allora genera il pdf e lo invia tramite mail
                string matricola = CommonManager.GetCurrentUserMatricola( );

                List<ModuloSmart2020Selezioni> selezioni = new List<ModuloSmart2020Selezioni>( );
                selezioni = GetSelezioniByString(scelta , dateSelezionate);

                if (selezioni == null)
                {
                    throw new Exception("Errore nel reperimento dei dati di selezione");
                }

                // creazione del pdf
                byte[] myArrayOfBytes = CreazionePDF(CommonManager.GetCurrentUserMatricola( ) ,
                                                    String.Format("{0} {1}" , Utente.EsponiAnagrafica( )._nome , Utente.EsponiAnagrafica( )._cognome) ,
                                                    Utente.EsponiAnagrafica( )._genere ,
                                                    selezioni ,
                                                    dataCompilazione );

                if ( myArrayOfBytes == null )
                {
                    result = "Errore nella creazione del PDF";
                    throw new Exception( result );
                }

                string oSerialized = Newtonsoft.Json.JsonConvert.SerializeObject(selezioni);

                // salva scelta
                string esito = Salva(oSerialized, dataCompilazione, myArrayOfBytes);

                // se è vuoto allora update completato
                // altrimenti sarà valorizzato con la descrizione dell'errore
                if ( !String.IsNullOrEmpty( esito ) )
                {
                    try
                    {
                        Logger.LogErrori(new MyRai_LogErrori()
                        {
                            applicativo = "Portale",
                            data = DateTime.Now,
                            matricola = CommonManager.GetCurrentUserMatricola(),
                            error_message = "SalvaScelta Modulo SW - Errore: " + esito + " dati: " + oSerialized,
                            provenienza = new StackFrame(1, true).GetMethod().Name
                        }, CommonManager.GetCurrentUserMatricola());
                    }
                    catch (Exception ex)
                    {

                    }


                    throw new Exception( esito );
                }

                try
                {
                    Logger.LogAzione(new MyRai_LogAzioni()
                    {
                        applicativo = "Portale" ,
                        data = DateTime.Now ,
                        matricola = CommonManager.GetCurrentUserMatricola(),
                        descrizione_operazione = "Salvataggio scelta smart2020 - scelta - " + oSerialized,
                        operazione = "Salvataggio SmartWorking" ,
                        provenienza = new StackFrame( 1 , true ).GetMethod( ).Name
                    }, CommonManager.GetCurrentUserMatricola());
                }
                catch ( Exception ex )
                {

                }

                // se tutto ok
                // invia mail con la copia del modulo compilato
                InvioMail( matricola , myArrayOfBytes );

            }
            catch ( Exception ex )
            {
                result = "Errore nella creazione del PDF: " + ex.Message;
            }

            return Content( result );
        }

        private List<ModuloSmart2020Selezioni> GetSelezioniByString (string scelta , string dateSelezionate)
        {
            List<ModuloSmart2020Selezioni> lista1 = new List<ModuloSmart2020Selezioni>( );
            List<ModuloSmart2020Selezioni> lista2 = new List<ModuloSmart2020Selezioni>( );
            List<ModuloSmart2020Selezioni> result = new List<ModuloSmart2020Selezioni>( );

            try
            {
                List<string> scelte = new List<string>( );
                List<string> selezioni = new List<string>( );

                // per prima cosa viene splittata la stringa in base al carattere '-'
                // in modo da avere n stringhe formate nel modo seguente:
                // "9|" + Scelta9_Dal + "#" + Scelta9_Al;
                // dove il 9 indica la selezione e i valori dopo il pipe le due date selezionate
                selezioni.AddRange(dateSelezionate.Split('-').ToList( ));

                if (selezioni == null || !selezioni.Any( ))
                {
                    throw new Exception("Non è stata effettuata alcuna selezione");
                }

                if (!String.IsNullOrEmpty(scelta))
                {
                    scelte.AddRange(scelta.Split(';').ToList( ));
                    if (scelte == null || !scelte.Any( ))
                    {
                        throw new Exception("Nessuna scelta effettuata");
                    }

                    foreach(var s in scelte)
                    {
                        if (String.IsNullOrEmpty(s))
                        {
                            continue;
                        }
                        int myInt = int.Parse(s);
                        ModuloSmart2020SelectionEnum selezionato = ( ModuloSmart2020SelectionEnum ) myInt;
                        lista1.Add(new ModuloSmart2020Selezioni( )
                        {
                            Selezione = selezionato
                        });
                    }
                }
                else
                {
                    throw new Exception("Nessuna scelta effettuata");
                }

                foreach (var s in selezioni)
                {
                    if (s == "")
                    {
                        continue;
                    }
                    List<string> splitPipe = new List<string>( );
                    List<string> splitCancelletto = new List<string>( );

                    splitPipe.AddRange(s.Split('|').ToList( ));

                    if (splitPipe == null || !splitPipe.Any( ))
                    {
                        throw new Exception("Non è stata effettuata alcuna selezione");
                    }

                    int myInt = int.Parse(splitPipe[0]);
                    ModuloSmart2020SelectionEnum selezionato = ( ModuloSmart2020SelectionEnum ) myInt;

                    splitCancelletto.AddRange(splitPipe[1].Split('#').ToList( ));

                    if (splitCancelletto == null || !splitCancelletto.Any( ))
                    {
                        throw new Exception("Non è stata effettuata alcuna selezione");
                    }
                    DateTime dt1 = DateTime.MinValue;
                    DateTime dt2 = DateTime.MinValue;
                    int count = 1;
                    foreach (var d in splitCancelletto)
                    {
                        bool b = false;
                        if (String.IsNullOrEmpty(d))
                        {
                            count++;
                            continue;
                        }

                        if (count == 1)
                        {
                            b = DateTime.TryParseExact(d , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out dt1);
                        }
                        else
                        {
                            b = DateTime.TryParseExact(d , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out dt2);
                        }

                        if (!b)
                        {
                            throw new Exception("Errore in conversione date");
                        }
                        count++;
                    }

                    DateTime? dataDal = null;
                    DateTime? dataAl = null;

                    if (dt1 != DateTime.MinValue)
                    {
                        dataDal = dt1;
                    }

                    if (dt2 != DateTime.MinValue)
                    {
                        dataAl = dt2;
                    }

                    lista2.Add(new ModuloSmart2020Selezioni( )
                    {
                        Selezione = selezionato ,
                        DataSelezionataDal = dataDal ,
                        DataSelezionataAl = dataAl
                    });
                }

                foreach(var l1 in lista1)
                {
                    var s = l1.Selezione;
                    var item = lista2.Where(w => w.Selezione.Equals(s)).FirstOrDefault( );
                    if (item != null)
                    {
                        l1.DataSelezionataDal = item.DataSelezionataDal;
                        l1.DataSelezionataAl = item.DataSelezionataAl;
                        result.Add(item);
                    }
                    else
                    {
                        result.Add(l1);
                    }
                }

                result = result.OrderBy(w => ( int ) w.Selezione).ToList( );
            }
            catch (Exception ex)
            {
                result = null;
            }
            return result;
        }

        private string Salva ( string oSerialized , DateTime dataCompilazione, byte[] modulo )
        {
            string esito = "";
            try
            {
                string mt = CommonManager.GetCurrentUserMatricola( );
                using ( TalentiaEntities dbTalentia = new TalentiaEntities( ) )
                {
                    bool exist = dbTalentia.XR_MOD_DIPENDENTI.Count( w => w.MATRICOLA.Equals( mt ) && w.COD_MODULO.Equals( "SMARTW2020" ) ) > 0;

                    if ( !exist )
                    {
                        //Reperimento dei dati del dipendente
                        XR_MOD_DIPENDENTI toAdd = null;

                        var info = ( from s in dbTalentia.SINTESI1
                                     join q in dbTalentia.QUALIFICA
                                     on s.COD_QUALIFICA equals q.COD_QUALIFICA
                                     where s.COD_MATLIBROMAT == mt
                                     select new
                                     {
                                         ID_PERSONA = s.ID_PERSONA ,
                                         COD_RUOLO = s.COD_RUOLO ,
                                         COD_QUALSTD = q.COD_QUALSTD
                                     } ).FirstOrDefault( );

                        if ( info != null )
                        {
                                        toAdd = new XR_MOD_DIPENDENTI( )
                                        {
                                            COD_MANSIONE = info.COD_RUOLO ,
                                            COD_MODULO = "SMARTW2020" ,
                                            COD_PROFILO_PROF = info.COD_QUALSTD ,
                                            COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress ,
                                COD_USER = CommonManager.GetCurrentUserMatricola( ) ,
                                            DATA_COMPILAZIONE = dataCompilazione ,
                                            DATA_INIZIO = null ,
                                            DATA_SCADENZA = null ,
                                            ID_PERSONA = info.ID_PERSONA ,
                                            IND_STATO = "0" ,
                                            MATRICOLA = mt ,
                                            PDF_MODULO = modulo ,
                                SCELTA = oSerialized ,
                                            TMS_TIMESTAMP = DateTime.Now ,
                                            XR_MOD_DIPENDENTI1 = dbTalentia.XR_MOD_DIPENDENTI.GeneraPrimaryKey( )
                                        };

                                        dbTalentia.XR_MOD_DIPENDENTI.Add( toAdd );
                                        dbTalentia.SaveChanges( );
                        }
                        else
                        {
                            throw new Exception( "Impossibile reperire i dati utente" );
                        }
                    }
                    else
                    {
                        throw new Exception( "L'utente ha già effettuato la scelta" );
                    }
                }
            }
            catch ( Exception ex )
            {
                esito = ex.Message;
                CommonTasks.LogErrore( ex.Message , "SalvataggioSmart2020 - WEB - Azione Salva" );
            }

            return esito;
        }

        public ActionResult GetPDF(string matricola, string codiceModulo)
        {
            byte[] byteArray = null;
            string nomefile = "modulo";

            using (TalentiaEntities dbTalentia = new TalentiaEntities( ))
            {
                bool exist = dbTalentia.XR_MOD_DIPENDENTI.Count(w => w.MATRICOLA.Equals(matricola) && w.COD_MODULO.Equals(codiceModulo)) > 0;

                if (!exist)
                {
                    return null;
                }
                else
                {
                    var items = dbTalentia.XR_MOD_DIPENDENTI.Where(w => w.MATRICOLA.Equals(matricola) && w.COD_MODULO.Equals(codiceModulo)).FirstOrDefault( );
                    if (items != null)
                    {
                        byteArray = items.PDF_MODULO;
                    }
                }
            }

            MemoryStream pdfStream = new MemoryStream( );
            pdfStream.Write(byteArray , 0 , byteArray.Length);
            pdfStream.Position = 0;

            var cd = new System.Net.Mime.ContentDisposition
            {
                FileName = "documento" ,
                Inline = true ,
            };

            Response.AddHeader("Content-Disposition" , "inline; filename=" + nomefile + ".pdf");
            return File(byteArray , "application/pdf");
        }
        #endregion

        #region Creazione PDF

        private static PdfPCell WriteCell (string text , int border , int colspan , int textAlign , Font f , float paddingTop = 0)
        {
            PdfPCell cell = new PdfPCell( new Phrase( text , f ) );
            cell.Border = border;
            cell.VerticalAlignment = Element.ALIGN_TOP;
            cell.HorizontalAlignment = textAlign; //0=Left, 1=Centre, 2=Right
            cell.Colspan = colspan;
            cell.PaddingTop = paddingTop;
            return cell;
        }

        private static PdfPCell WriteCell (iTextSharp.text.Image img , int border , int colspan , int textAlign , float paddingTop = 0)
        {
            PdfPCell cell = new PdfPCell(img);
            cell.Border = border;
            cell.VerticalAlignment = Element.ALIGN_TOP;
            cell.HorizontalAlignment = textAlign; //0=Left, 1=Centre, 2=Right
            cell.Colspan = colspan;
            cell.PaddingTop = paddingTop;
            return cell;
        }

        private static PdfPCell WriteCell ( Phrase text , int border , int colspan , int textAlign, float paddingTop = 0 )
        {
            PdfPCell cell = new PdfPCell( text );
            cell.Border = border;
            cell.HorizontalAlignment = textAlign; //0=Left, 1=Centre, 2=Right
            cell.VerticalAlignment = Element.ALIGN_TOP;
            cell.PaddingTop = paddingTop;

            cell.Colspan = colspan;
            return cell;
        }

        private static PdfPCell WriteCell ( Paragraph text , int border , int colspan , int textAlign , float paddingTop = 0)
        {
            PdfPCell cell = new PdfPCell( text );
            cell.Border = border;
            cell.HorizontalAlignment = textAlign; //0=Left, 1=Centre, 2=Right
            cell.VerticalAlignment = Element.ALIGN_TOP;
            cell.Colspan = colspan;
            cell.PaddingTop = paddingTop;
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

        public static iTextSharp.text.Image drawCircleMin ( PdfContentByte contentByte , bool full )
        {
            var template = contentByte.CreateTemplate( 10 , 10 );

            int arcPosStartY = 0;
            int arcPosStartX = 0;
            int arcPosEndX = 5;
            int arcPosEndY = 5;

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

        private static byte[] CreazionePDF ( string matricola , string nominativo , string sesso , List<ModuloSmart2020Selezioni> selezioni , DateTime dataCreazione )
        {
            try
            {
                var cultureInfo = CultureInfo.GetCultureInfo( "it-IT" );

                byte[] bytes = null;
                const int fontIntestazione = 12;
                const int fontCorpo = 11;
                const int fontNote = 9;

                DateTime dt1 = DateTime.MinValue;
                DateTime dt2 = DateTime.MinValue;
                DateTime dt3 = DateTime.MinValue;
                DateTime dt4 = DateTime.MinValue;
                string testo = "";

                List<string> date = new List<string>( );

                BaseFont bf = BaseFont.CreateFont( BaseFont.HELVETICA , BaseFont.CP1250 , BaseFont.NOT_EMBEDDED );
                BaseColor color = new BaseColor( System.Drawing.Color.Black );
                Font arial = FontFactory.GetFont( "Arial" , 28 , Font.NORMAL , color );

                Font myFontIntestazione = FontFactory.GetFont( "Arial" , fontIntestazione , Font.NORMAL , color );
                Font myFontIntestazioneBold = FontFactory.GetFont( "Arial" , fontIntestazione , Font.BOLD , color );
                Font myFontCorpo = FontFactory.GetFont( "Arial" , fontCorpo , Font.NORMAL , color );
                Font myFontCorpoBold = FontFactory.GetFont( "Arial" , fontIntestazione , Font.BOLD , color );
                Font myFontNote = FontFactory.GetFont( "Arial" , fontNote , Font.NORMAL , color );
                Font myFontCorpoSottolineato = FontFactory.GetFont( "Arial" , fontCorpo , Font.UNDERLINE , color );
                Font myFontCorpoItalic = FontFactory.GetFont( "Arial" , fontCorpo , Font.ITALIC , color );

                using ( MemoryStream ms = new MemoryStream( ) )
                {
                    // Creazione dell'istanza del documento, impostando tipo di pagina e margini
                    Document document = new Document( PageSize.A4 , 25 , 25 , 25 , 25 );
                    PdfWriter writer = PdfWriter.GetInstance( document , ms );

                    document.Open( );

                    PdfContentByte cb = writer.DirectContent;

                    string _imgPath = System.Web.HttpContext.Current.Server.MapPath( "~/assets/img/LogoPDF.png" );

                    iTextSharp.text.Image png = iTextSharp.text.Image.GetInstance( _imgPath );
                    png.ScaleAbsolute( 45 , 45 );
                    png.SetAbsolutePosition( 25 , 750 );
                    cb.AddImage( png );

                    Phrase phrase = new Phrase( );
                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );

                    Paragraph p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;

                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    string tx = "MODULO PER L’ESERCIZIO DEL DIRITTO ALLO SMART WORKING";

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
                    phrase.Add( new Chunk( tx , myFontIntestazioneBold ) );
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

                    tx = "OGGETTO: Manifestazione del diritto a rendere la prestazione in modalita' di lavoro agile";

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

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
                        phrase.Add( new Chunk( "Io sottoscritta " , myFontCorpo ) );
                        phrase.Add( new Chunk( nominativo , myFontCorpoBold ) );
                        phrase.Add( new Chunk( ", matricola aziendale n. " , myFontCorpo ) );
                        phrase.Add( new Chunk( matricola , myFontCorpoBold ) );
                    }
                    else
                    {
                        phrase.Add( new Chunk( "Io sottoscritto " , myFontCorpo ) );
                        phrase.Add( new Chunk( nominativo , myFontCorpoBold ) );
                        phrase.Add( new Chunk( ", matricola aziendale n. " , myFontCorpo ) );
                        phrase.Add( new Chunk( matricola , myFontCorpoBold ) );
                    }

                    phrase.Add( new Chunk( " intendo esercitare, con la sottoscrizione del presente modulo, il mio diritto allo svolgimento dell'attivita' lavorativa nella modalita' del lavoro agile ( " , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;

                    phrase.Add( new Chunk( " smart working " , myFontCorpoItalic ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;

                    phrase.Add( new Chunk( " ) per tutta la durata prevista dalla specifica normativa emergenziale di riferimento. " , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( "Pertanto, " , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;

                    phrase.Add( new Chunk( " sotto la mia responsabilita' " , myFontCorpoSottolineato) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;

                    phrase.Add( new Chunk( ", consapevole delle possibili conseguenze derivanti da dichiarazioni non veritiere, come accertate all'esito dei controlli che potranno essere eseguiti dall'Azienda, dichiaro, a questi fini, di appartenere ad una delle seguenti categorie:" , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    #region SELEZIONE 7
                    testo = ModuloSmart2020SelectionEnum.Scelta7.GetDescription( );
                    Phrase imgPhrase = new Paragraph(new Chunk(drawCircle(cb , (selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta7) > 0)) , 1f , 0f));
                    imgPhrase.Add(new Chunk(testo , myFontCorpo));
                    p = new Paragraph(imgPhrase);
                    (( Paragraph ) p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);
                    #endregion

                    #region SELEZIONE 8
                    testo = ModuloSmart2020SelectionEnum.Scelta8.GetDescription( );
                    imgPhrase = new Paragraph(new Chunk(drawCircle(cb , (selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta8) > 0)) , 1f , 0f));
                    imgPhrase.Add(new Chunk(testo , myFontCorpo));
                    p = new Paragraph(imgPhrase);
                    (( Paragraph ) p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase( );
                    phrase.Add(new Chunk(" " , myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    (( Paragraph ) p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #endregion

                    #region SELEZIONE 9
                    testo = ModuloSmart2020SelectionEnum.Scelta9.GetDescription( );
                    var item9 = selezioni.Where(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta9).FirstOrDefault( );
                    if (item9 != null)
                    {
                        dt1 = item9.DataSelezionataDal.Value;
                        dt2 = item9.DataSelezionataAl.Value;                        
                        testo = testo.Replace("#DATADAL#" , dt1.ToString("dd/MM/yyyy"));
                        testo = testo.Replace("#DATAAL#" , dt2.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        testo = testo.Replace("#DATADAL#" , "_________");
                        testo = testo.Replace("#DATAAL#" , "_________");
                    }

                    imgPhrase = new Paragraph(new Chunk(drawCircle(cb , (selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta9) > 0)) , 1f , 0f));
                    imgPhrase.Add( new Chunk(testo , myFontCorpo) );
                    p = new Paragraph( imgPhrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    #region SCELTA 91

                    string tempTX = ModuloSmart2020SelectionEnum.Scelta9_1.GetDescription( );
                    var Scelta9_1 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta9_1) > 0;

                    PdfPTable tableDetail = new PdfPTable(3);
                    tableDetail.DefaultCell.BorderWidth = 0;
                    tableDetail.TotalWidth = 550;
                    tableDetail.LockedWidth = true;
                    int[] tableDetailWidth = new int[] { 15, 15 , 535 };
                    tableDetail.SetWidths(tableDetailWidth);
                    tableDetail.AddCell(WriteCell(" " , 0 , 1 , 0 , myFontCorpo , 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb , Scelta9_1) , 0 , 1 , 0));
                    tableDetail.AddCell(WriteCell(tempTX , 0 , 1 , 0 , myFontCorpo, 7));

                    #endregion

                    #region SCELTA 92

                    tempTX = ModuloSmart2020SelectionEnum.Scelta9_2.GetDescription( );
                    var Scelta9_2 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta9_2) > 0;
                    tableDetail.AddCell(WriteCell(" " , 0 , 1 , 0 , myFontCorpo , 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb , Scelta9_2) , 0 , 1 , 0));
                    tableDetail.AddCell(WriteCell(tempTX , 0 , 1 , 0 , myFontCorpo , 7));

                    #endregion

                    #region SCELTA 93

                    tempTX = ModuloSmart2020SelectionEnum.Scelta9_3.GetDescription( );
                    var Scelta9_3 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta9_3) > 0;
                    tableDetail.AddCell(WriteCell(" " , 0 , 1 , 0 , myFontCorpo , 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb , Scelta9_3) , 0 , 1 , 0));
                    tableDetail.AddCell(WriteCell(tempTX , 0 , 1 , 0 , myFontCorpo , 7));

                    #endregion

                    document.Add(tableDetail);
                    tableDetail.FlushContent( );
                    tableDetail.DeleteBodyRows( );

                    phrase = new Phrase( );
                    phrase.Add(new Chunk(" " , myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    (( Paragraph ) p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase( );
                    phrase.Add(new Chunk("A tal fine, dichiaro:" , myFontCorpo));
                    p = new Paragraph(phrase);
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    #region SCELTA 94
                    tempTX = ModuloSmart2020SelectionEnum.Scelta9_4.GetDescription( );
                    var Scelta9_4 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta9_4) > 0;

                    tableDetail = new PdfPTable(3);
                    tableDetail.DefaultCell.BorderWidth = 0;
                    tableDetail.TotalWidth = 550;
                    tableDetail.LockedWidth = true;
                    tableDetail.SetWidths(tableDetailWidth);
                    tableDetail.AddCell(WriteCell(" " , 0 , 1 , 0 , myFontCorpo , 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb , Scelta9_4) , 0 , 1 , 0));
                    tableDetail.AddCell(WriteCell(tempTX , 0 , 1 , 0 , myFontCorpo , 7));

                    #endregion

                    #region SCELTA 96

                    tempTX = ModuloSmart2020SelectionEnum.Scelta9_6.GetDescription( );
                    var Scelta9_6 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta9_6) > 0;
                    tableDetail.AddCell(WriteCell(" " , 0 , 1 , 0 , myFontCorpo , 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb , Scelta9_6) , 0 , 1 , 0));
                    tableDetail.AddCell(WriteCell(tempTX , 0 , 1 , 0 , myFontCorpo , 7));

                    #endregion

                    document.Add(tableDetail);
                    tableDetail.FlushContent( );
                    tableDetail.DeleteBodyRows( );

                    phrase = new Phrase( );
                    phrase.Add(new Chunk(" " , myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    (( Paragraph ) p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk("Sono consapevole che il diritto allo svolgimento della prestazione nella forma del lavoro agile e' comunque subordinato alla verifica della compatibilita' della mansione con tale modalita'.", myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #endregion

                    #region SELEZIONE 10
                    testo = ModuloSmart2020SelectionEnum.Scelta10.GetDescription();
                    var Scelta10 = selezioni.Where(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta10).FirstOrDefault();
                    if (Scelta10 != null)
                    {
                        dt1 = Scelta10.DataSelezionataDal.Value;
                        dt2 = Scelta10.DataSelezionataAl.Value;
                        testo = testo.Replace("#DATADAL#", dt1.ToString("dd/MM/yyyy"));
                        testo = testo.Replace("#DATAAL#", dt2.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        testo = testo.Replace("#DATADAL#", "_________");
                        testo = testo.Replace("#DATAAL#", "_________");
                    }

                    imgPhrase = new Paragraph(new Chunk(drawCircle(cb, (selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta10) > 0)), 1f, 0f));
                    imgPhrase.Add(new Chunk(testo, myFontCorpo));
                    p = new Paragraph( imgPhrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #region SCELTA 101

                    tempTX = ModuloSmart2020SelectionEnum.Scelta10_1.GetDescription();
                    var Scelta10_1 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta10_1) > 0;

                    tableDetail = new PdfPTable(3);
                    tableDetail.DefaultCell.BorderWidth = 0;
                    tableDetail.TotalWidth = 550;
                    tableDetail.LockedWidth = true;
                    tableDetailWidth = new int[] { 15, 15, 535 };
                    tableDetail.SetWidths(tableDetailWidth);
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta10_1), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));

                    #endregion

                    #region SCELTA 102

                    tempTX = ModuloSmart2020SelectionEnum.Scelta10_2.GetDescription();
                    var Scelta10_2 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta10_2) > 0;
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta10_2), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));

                    #endregion

                    #region SCELTA 103

                    tempTX = ModuloSmart2020SelectionEnum.Scelta10_3.GetDescription();
                    var Scelta10_3 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta10_3) > 0;
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta10_3), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));

                    #endregion

                    document.Add(tableDetail);
                    tableDetail.FlushContent();
                    tableDetail.DeleteBodyRows();

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk("A tal fine, dichiaro:", myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #region SCELTA 104
                    tempTX = ModuloSmart2020SelectionEnum.Scelta10_4.GetDescription();
                    var Scelta10_4 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta10_4) > 0;

                    tableDetail = new PdfPTable(3);
                    tableDetail.DefaultCell.BorderWidth = 0;
                    tableDetail.TotalWidth = 550;
                    tableDetail.LockedWidth = true;
                    tableDetail.SetWidths(tableDetailWidth);
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta10_4), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));

                    #endregion

                    #region SCELTA 106

                    tempTX = ModuloSmart2020SelectionEnum.Scelta10_6.GetDescription();
                    var Scelta10_6 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta10_6) > 0;
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta10_6), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));

                    #endregion

                    document.Add(tableDetail);
                    tableDetail.FlushContent();
                    tableDetail.DeleteBodyRows();

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk("Sono consapevole che il diritto allo svolgimento della prestazione nella forma del lavoro agile e' comunque subordinato alla verifica della compatibilita' della mansione con tale modalita'.", myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);
                    #endregion

                    #region SELEZIONE 11
                    testo = ModuloSmart2020SelectionEnum.Scelta11.GetDescription( );
                    var Scelta11 = selezioni.Where(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta11).FirstOrDefault( );
                    if (Scelta11 != null)
                    {
                        dt1 = Scelta11.DataSelezionataDal.Value;
                        dt2 = Scelta11.DataSelezionataAl.Value;
                        testo = testo.Replace("#DATADAL#", dt1.ToString("dd/MM/yyyy"));
                        testo = testo.Replace("#DATAAL#", dt2.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        testo = testo.Replace("#DATADAL#", "_________");
                        testo = testo.Replace("#DATAAL#", "_________");
                    }

                    imgPhrase = new Paragraph(new Chunk(drawCircle(cb , (selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta11) > 0)) , 1f , 0f));
                    imgPhrase.Add(new Chunk(testo , myFontCorpo));
                    p = new Paragraph(imgPhrase);
                    (( Paragraph ) p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add(new Chunk("A tal fine, dichiaro:" , myFontCorpo));
                    p = new Paragraph(phrase);
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    #region SCELTA 111
                    tempTX = ModuloSmart2020SelectionEnum.Scelta11_1.GetDescription( );
                    var Scelta11_1 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta11_1) > 0;

                    tableDetail = new PdfPTable(3);
                    tableDetail.DefaultCell.BorderWidth = 0;
                    tableDetail.TotalWidth = 550;
                    tableDetail.LockedWidth = true;
                    tableDetail.SetWidths(tableDetailWidth);
                    tableDetail.AddCell(WriteCell(" " , 0 , 1 , 0 , myFontCorpo , 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb , Scelta11_1) , 0 , 1 , 0));
                    tableDetail.AddCell(WriteCell(tempTX , 0 , 1 , 0 , myFontCorpo , 7));                    
                    #endregion

                    #region SCELTA 113

                    tempTX = ModuloSmart2020SelectionEnum.Scelta11_3.GetDescription( );
                    var Scelta11_3 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta11_3) > 0;
                    tableDetail.AddCell(WriteCell(" " , 0 , 1 , 0 , myFontCorpo , 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb , Scelta11_3) , 0 , 1 , 0));
                    tableDetail.AddCell(WriteCell(tempTX , 0 , 1 , 0 , myFontCorpo , 7));

                    #endregion

                    document.Add(tableDetail);
                    tableDetail.FlushContent( );
                    tableDetail.DeleteBodyRows( );

                    phrase = new Phrase( );
                    phrase.Add(new Chunk(" " , myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase();
                    phrase.Add(new Chunk("Sono consapevole che il diritto allo svolgimento della prestazione nella forma del lavoro agile e' comunque subordinato alla verifica della compatibilita' della mansione con tale modalita'.", myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);
                    #endregion

                    #region SELEZIONE 12
                    testo = ModuloSmart2020SelectionEnum.Scelta12.GetDescription();
                    var Scelta12 = selezioni.Where(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta12).FirstOrDefault();
                    if (Scelta12 != null)
                    {
                        dt1 = Scelta12.DataSelezionataDal.Value;
                        testo = testo.Replace("#DATADAL#", dt1.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        testo = testo.Replace("#DATADAL#", "_________");
                    }

                    imgPhrase = new Paragraph(new Chunk(drawCircle(cb, (selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta12) > 0)), 1f, 0f));
                    imgPhrase.Add(new Chunk(testo, myFontCorpo));
                    p = new Paragraph(imgPhrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #region SCELTA 121
                    tempTX = ModuloSmart2020SelectionEnum.Scelta12_1.GetDescription();
                    var Scelta12_1 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta12_1) > 0;

                    tableDetail = new PdfPTable(3);
                    tableDetail.DefaultCell.BorderWidth = 0;
                    tableDetail.TotalWidth = 550;
                    tableDetail.LockedWidth = true;
                    tableDetail.SetWidths(tableDetailWidth);
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta12_1), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));
                    #endregion

                    #region SCELTA 123

                    tempTX = ModuloSmart2020SelectionEnum.Scelta12_3.GetDescription();
                    var Scelta12_3 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta12_3) > 0;
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta12_3), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));

                    #endregion

                    document.Add(tableDetail);
                    tableDetail.FlushContent();
                    tableDetail.DeleteBodyRows();
                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk("Sono consapevole che il diritto allo svolgimento della prestazione nella forma del lavoro agile e' comunque subordinato alla verifica della compatibilita' della mansione con tale modalita'.", myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);
                    #endregion

                    #region SELEZIONE 13
                    testo = ModuloSmart2020SelectionEnum.Scelta13.GetDescription();
                    var Scelta13 = selezioni.Where(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta13).FirstOrDefault();
                    if (Scelta13 != null)
                    {
                        dt1 = Scelta13.DataSelezionataDal.Value;
                        dt2 = Scelta13.DataSelezionataAl.Value;
                        testo = testo.Replace("#DATADAL#", dt1.ToString("dd/MM/yyyy"));
                        testo = testo.Replace("#DATAAL#", dt2.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        testo = testo.Replace("#DATADAL#", "_________");
                        testo = testo.Replace("#DATAAL#", "_________");
                    }

                    imgPhrase = new Paragraph(new Chunk(drawCircle(cb, (selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta13) > 0)), 1f, 0f));
                    imgPhrase.Add(new Chunk(testo, myFontCorpo));
                    p = new Paragraph(imgPhrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #region SCELTA 131
                    tempTX = ModuloSmart2020SelectionEnum.Scelta13_1.GetDescription();
                    var Scelta13_1 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta13_1) > 0;

                    tableDetail = new PdfPTable(3);
                    tableDetail.DefaultCell.BorderWidth = 0;
                    tableDetail.TotalWidth = 550;
                    tableDetail.LockedWidth = true;
                    tableDetail.SetWidths(tableDetailWidth);
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta13_1), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));
                    #endregion

                    #region SCELTA 132

                    tempTX = ModuloSmart2020SelectionEnum.Scelta13_2.GetDescription();
                    var Scelta13_2 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta13_2) > 0;
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta13_2), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));

                    #endregion

                    #region SCELTA 133

                    tempTX = ModuloSmart2020SelectionEnum.Scelta13_3.GetDescription();
                    var Scelta13_3 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta13_3) > 0;
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta13_3), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));

                    #endregion

                    document.Add(tableDetail);
                    tableDetail.FlushContent();
                    tableDetail.DeleteBodyRows();
                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);
                    #endregion

                    #region SELEZIONE 14
                    testo = ModuloSmart2020SelectionEnum.Scelta14.GetDescription();
                    var Scelta14 = selezioni.Where(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta14).FirstOrDefault();
                    if (Scelta14 != null)
                    {
                        dt1 = Scelta14.DataSelezionataDal.Value;
                        dt2 = Scelta14.DataSelezionataAl.Value;
                        testo = testo.Replace("#DATADAL#", dt1.ToString("dd/MM/yyyy"));
                        testo = testo.Replace("#DATAAL#", dt2.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        testo = testo.Replace("#DATADAL#", "_________");
                        testo = testo.Replace("#DATAAL#", "_________");
                    }

                    imgPhrase = new Paragraph(new Chunk(drawCircle(cb, (selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta14) > 0)), 1f, 0f));
                    imgPhrase.Add(new Chunk(testo, myFontCorpo));
                    p = new Paragraph(imgPhrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #region SCELTA 141
                    tempTX = ModuloSmart2020SelectionEnum.Scelta14_1.GetDescription();
                    var Scelta14_1 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta14_1) > 0;

                    tableDetail = new PdfPTable(3);
                    tableDetail.DefaultCell.BorderWidth = 0;
                    tableDetail.TotalWidth = 550;
                    tableDetail.LockedWidth = true;
                    tableDetail.SetWidths(tableDetailWidth);
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta14_1), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));
                    #endregion

                    #region SCELTA 142

                    tempTX = ModuloSmart2020SelectionEnum.Scelta14_2.GetDescription();
                    var Scelta14_2 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta14_2) > 0;
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta14_2), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));

                    #endregion

                    #region SCELTA 143

                    tempTX = ModuloSmart2020SelectionEnum.Scelta14_3.GetDescription();
                    var Scelta14_3 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta14_3) > 0;
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta14_3), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));

                    #endregion

                    document.Add(tableDetail);
                    tableDetail.FlushContent();
                    tableDetail.DeleteBodyRows();

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #endregion

                    #region SELEZIONE 15
                    testo = ModuloSmart2020SelectionEnum.Scelta15.GetDescription();
                    var Scelta15 = selezioni.Where(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta15).FirstOrDefault();
                    if (Scelta15 != null)
                    {
                        dt1 = Scelta15.DataSelezionataDal.Value;
                        dt2 = Scelta15.DataSelezionataAl.Value;
                        testo = testo.Replace("#DATADAL#", dt1.ToString("dd/MM/yyyy"));
                        testo = testo.Replace("#DATAAL#", dt2.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        testo = testo.Replace("#DATADAL#", "_________");
                        testo = testo.Replace("#DATAAL#", "_________");
                    }

                    imgPhrase = new Paragraph(new Chunk(drawCircle(cb, (selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta15) > 0)), 1f, 0f));
                    imgPhrase.Add(new Chunk(testo, myFontCorpo));
                    p = new Paragraph(imgPhrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #region SCELTA 151
                    tempTX = ModuloSmart2020SelectionEnum.Scelta15_1.GetDescription();
                    var Scelta15_1 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta15_1) > 0;

                    tableDetail = new PdfPTable(3);
                    tableDetail.DefaultCell.BorderWidth = 0;
                    tableDetail.TotalWidth = 550;
                    tableDetail.LockedWidth = true;
                    tableDetail.SetWidths(tableDetailWidth);
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta15_1), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));
                    #endregion

                    #region SCELTA 152

                    tempTX = ModuloSmart2020SelectionEnum.Scelta15_2.GetDescription();
                    var Scelta15_2 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta15_2) > 0;
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta15_2), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));

                    #endregion

                    #region SCELTA 153

                    tempTX = ModuloSmart2020SelectionEnum.Scelta15_3.GetDescription();
                    var Scelta15_3 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta15_3) > 0;
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta15_3), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));

                    #endregion

                    document.Add(tableDetail);
                    tableDetail.FlushContent();
                    tableDetail.DeleteBodyRows();

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #endregion

                    #region SELEZIONE 16
                    testo = ModuloSmart2020SelectionEnum.Scelta16.GetDescription();
                    var Scelta16 = selezioni.Where(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta16).FirstOrDefault();
                    if (Scelta16 != null)
                    {
                        dt1 = Scelta16.DataSelezionataDal.Value;
                        dt2 = Scelta16.DataSelezionataAl.Value;
                        testo = testo.Replace("#DATADAL#", dt1.ToString("dd/MM/yyyy"));
                        testo = testo.Replace("#DATAAL#", dt2.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        testo = testo.Replace("#DATADAL#", "_________");
                        testo = testo.Replace("#DATAAL#", "_________");
                    }

                    imgPhrase = new Paragraph(new Chunk(drawCircle(cb, (selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta16) > 0)), 1f, 0f));
                    imgPhrase.Add(new Chunk(testo, myFontCorpo));
                    p = new Paragraph(imgPhrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #endregion

                    #region SELEZIONE 17
                    testo = ModuloSmart2020SelectionEnum.Scelta17.GetDescription();
                    var Scelta17 = selezioni.Where(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta17).FirstOrDefault();
                    if (Scelta17 != null)
                    {
                        dt1 = Scelta17.DataSelezionataDal.Value;
                        //dt2 = Scelta17.DataSelezionataAl.Value;
                        testo = testo.Replace("#DATADAL#", dt1.ToString("dd/MM/yyyy"));
                        //testo = testo.Replace("#DATAAL#", dt2.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        testo = testo.Replace("#DATADAL#", "_________");
                        //testo = testo.Replace("#DATAAL#", "_________");
                    }

                    imgPhrase = new Paragraph(new Chunk(drawCircle(cb, (selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta17) > 0)), 1f, 0f));
                    imgPhrase.Add(new Chunk(testo, myFontCorpo));
                    p = new Paragraph(imgPhrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #endregion

                    #region SELEZIONE 18
                    testo = ModuloSmart2020SelectionEnum.Scelta18.GetDescription();
                    var Scelta18 = selezioni.Where(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta18).FirstOrDefault();
                    if (Scelta18 != null)
                    {
                        dt1 = Scelta18.DataSelezionataDal.Value;
                        dt2 = Scelta18.DataSelezionataAl.Value;
                        testo = testo.Replace("#DATADAL#", dt1.ToString("dd/MM/yyyy"));
                        testo = testo.Replace("#DATAAL#", dt2.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        testo = testo.Replace("#DATADAL#", "_________");
                        testo = testo.Replace("#DATAAL#", "_________");
                    }

                    imgPhrase = new Paragraph(new Chunk(drawCircle(cb, (selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta18) > 0)), 1f, 0f));
                    imgPhrase.Add(new Chunk(testo, myFontCorpo));
                    p = new Paragraph(imgPhrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #region SCELTA 181
                    tempTX = ModuloSmart2020SelectionEnum.Scelta18_1.GetDescription();
                    var Scelta18_1 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta18_1) > 0;

                    tableDetail = new PdfPTable(3);
                    tableDetail.DefaultCell.BorderWidth = 0;
                    tableDetail.TotalWidth = 550;
                    tableDetail.LockedWidth = true;
                    tableDetail.SetWidths(tableDetailWidth);
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta18_1), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));
                    #endregion

                    #region SCELTA 182

                    tempTX = ModuloSmart2020SelectionEnum.Scelta18_2.GetDescription();
                    var Scelta18_2 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta18_2) > 0;
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta18_2), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));

                    #endregion

                    #region SCELTA 183

                    tempTX = ModuloSmart2020SelectionEnum.Scelta18_3.GetDescription();
                    var Scelta18_3 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta18_3) > 0;
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta18_3), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));

                    #endregion

                    document.Add(tableDetail);
                    tableDetail.FlushContent();
                    tableDetail.DeleteBodyRows();

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #endregion

                    #region SELEZIONE 19
                    testo = ModuloSmart2020SelectionEnum.Scelta19.GetDescription();
                    var Scelta19 = selezioni.Where(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta19).FirstOrDefault();
                    if (Scelta19 != null)
                    {
                        dt1 = Scelta19.DataSelezionataDal.Value;
                        dt2 = Scelta19.DataSelezionataAl.Value;
                        testo = testo.Replace("#DATADAL#", dt1.ToString("dd/MM/yyyy"));
                        testo = testo.Replace("#DATAAL#", dt2.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        testo = testo.Replace("#DATADAL#", "_________");
                        testo = testo.Replace("#DATAAL#", "_________");
                    }

                    imgPhrase = new Paragraph(new Chunk(drawCircle(cb, (selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta19) > 0)), 1f, 0f));
                    imgPhrase.Add(new Chunk(testo, myFontCorpo));
                    p = new Paragraph(imgPhrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #region SCELTA 191
                    tempTX = ModuloSmart2020SelectionEnum.Scelta19_1.GetDescription();
                    var Scelta19_1 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta19_1) > 0;

                    tableDetail = new PdfPTable(3);
                    tableDetail.DefaultCell.BorderWidth = 0;
                    tableDetail.TotalWidth = 550;
                    tableDetail.LockedWidth = true;
                    tableDetail.SetWidths(tableDetailWidth);
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta19_1), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));
                    #endregion

                    #region SCELTA 192

                    tempTX = ModuloSmart2020SelectionEnum.Scelta19_2.GetDescription();
                    var Scelta19_2 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta19_2) > 0;
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta19_2), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));

                    #endregion

                    #region SCELTA 193

                    tempTX = ModuloSmart2020SelectionEnum.Scelta19_3.GetDescription();
                    var Scelta19_3 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta19_3) > 0;
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta19_3), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));

                    #endregion

                    document.Add(tableDetail);
                    tableDetail.FlushContent();
                    tableDetail.DeleteBodyRows();

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #endregion

                    #region SELEZIONE 20
                    testo = ModuloSmart2020SelectionEnum.Scelta20.GetDescription();
                    var Scelta20 = selezioni.Where(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta20).FirstOrDefault();
                    if (Scelta20 != null)
                    {
                        dt1 = Scelta20.DataSelezionataDal.Value;
                        dt2 = Scelta20.DataSelezionataAl.Value;
                        testo = testo.Replace("#DATADAL#", dt1.ToString("dd/MM/yyyy"));
                        testo = testo.Replace("#DATAAL#", dt2.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        testo = testo.Replace("#DATADAL#", "_________");
                        testo = testo.Replace("#DATAAL#", "_________");
                    }

                    imgPhrase = new Paragraph(new Chunk(drawCircle(cb, (selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta20) > 0)), 1f, 0f));
                    imgPhrase.Add(new Chunk(testo, myFontCorpo));
                    p = new Paragraph(imgPhrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);
                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #region SCELTA 201
                    tempTX = ModuloSmart2020SelectionEnum.Scelta20_1.GetDescription();
                    var Scelta20_1 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta20_1) > 0;

                    tableDetail = new PdfPTable(3);
                    tableDetail.DefaultCell.BorderWidth = 0;
                    tableDetail.TotalWidth = 550;
                    tableDetail.LockedWidth = true;
                    tableDetail.SetWidths(tableDetailWidth);
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta20_1), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));
                    #endregion

                    #region SCELTA 202

                    tempTX = ModuloSmart2020SelectionEnum.Scelta20_2.GetDescription();
                    var Scelta20_2 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta20_2) > 0;
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta20_2), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));

                    #endregion

                    #region SCELTA 203

                    tempTX = ModuloSmart2020SelectionEnum.Scelta20_3.GetDescription();
                    var Scelta20_3 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta20_3) > 0;
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta20_3), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));

                    #endregion

                    document.Add(tableDetail);
                    tableDetail.FlushContent();
                    tableDetail.DeleteBodyRows();

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #endregion

                    #region SELEZIONE 21
                    testo = ModuloSmart2020SelectionEnum.Scelta21.GetDescription();
                    var Scelta21 = selezioni.Where(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta21).FirstOrDefault();
                    if (Scelta21 != null)
                    {
                        dt1 = Scelta21.DataSelezionataDal.Value;
                        dt2 = Scelta21.DataSelezionataAl.Value;
                        testo = testo.Replace("#DATADAL#", dt1.ToString("dd/MM/yyyy"));
                        testo = testo.Replace("#DATAAL#", dt2.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        testo = testo.Replace("#DATADAL#", "_________");
                        testo = testo.Replace("#DATAAL#", "_________");
                    }

                    imgPhrase = new Paragraph(new Chunk(drawCircle(cb, (selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta21) > 0)), 1f, 0f));
                    imgPhrase.Add(new Chunk(testo, myFontCorpo));
                    p = new Paragraph(imgPhrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #endregion

                    #region SELEZIONE 22
                    testo = ModuloSmart2020SelectionEnum.Scelta22.GetDescription();
                    var Scelta22 = selezioni.Where(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta22).FirstOrDefault();
                    if (Scelta22 != null)
                    {
                        dt1 = Scelta22.DataSelezionataDal.Value;
                        //dt2 = Scelta22.DataSelezionataAl.Value;
                        testo = testo.Replace("#DATADAL#", dt1.ToString("dd/MM/yyyy"));
                        //testo = testo.Replace("#DATAAL#", dt2.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        testo = testo.Replace("#DATADAL#", "_________");
                        //testo = testo.Replace("#DATAAL#", "_________");
                    }

                    imgPhrase = new Paragraph(new Chunk(drawCircle(cb, (selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta22) > 0)), 1f, 0f));
                    imgPhrase.Add(new Chunk(testo, myFontCorpo));
                    p = new Paragraph(imgPhrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #endregion

                    #region SELEZIONE 23
                    testo = ModuloSmart2020SelectionEnum.Scelta23.GetDescription();
                    var Scelta23 = selezioni.Where(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta23).FirstOrDefault();
                    if (Scelta23 != null)
                    {
                        dt1 = Scelta23.DataSelezionataDal.Value;
                        dt2 = Scelta23.DataSelezionataAl.Value;
                        testo = testo.Replace("#DATADAL#", dt1.ToString("dd/MM/yyyy"));
                        testo = testo.Replace("#DATAAL#", dt2.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        testo = testo.Replace("#DATADAL#", "_________");
                        testo = testo.Replace("#DATAAL#", "_________");
                    }

                    imgPhrase = new Paragraph(new Chunk(drawCircle(cb, (selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta23) > 0)), 1f, 0f));
                    imgPhrase.Add(new Chunk(testo, myFontCorpo));
                    p = new Paragraph(imgPhrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #region SCELTA 231
                    tempTX = ModuloSmart2020SelectionEnum.Scelta23_1.GetDescription();
                    var Scelta23_1 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta23_1) > 0;

                    tableDetail = new PdfPTable(3);
                    tableDetail.DefaultCell.BorderWidth = 0;
                    tableDetail.TotalWidth = 550;
                    tableDetail.LockedWidth = true;
                    tableDetail.SetWidths(tableDetailWidth);
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta23_1), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));
                    #endregion

                    #region SCELTA 232

                    tempTX = ModuloSmart2020SelectionEnum.Scelta23_2.GetDescription();
                    var Scelta23_2 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta23_2) > 0;
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta23_2), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));

                    #endregion

                    #region SCELTA 233

                    tempTX = ModuloSmart2020SelectionEnum.Scelta23_3.GetDescription();
                    var Scelta23_3 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta23_3) > 0;
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta23_3), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));

                    #endregion

                    document.Add(tableDetail);
                    tableDetail.FlushContent();
                    tableDetail.DeleteBodyRows();

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #endregion

                    #region SELEZIONE 33
                    testo = ModuloSmart2020SelectionEnum.Scelta33.GetDescription();
                    var Scelta33 = selezioni.Where(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta33).FirstOrDefault();
                    if (Scelta33 != null)
                    {
                        dt1 = Scelta33.DataSelezionataDal.Value;
                        dt2 = Scelta33.DataSelezionataAl.Value;
                        testo = testo.Replace("#DATADAL#", dt1.ToString("dd/MM/yyyy"));
                        testo = testo.Replace("#DATAAL#", dt2.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        testo = testo.Replace("#DATADAL#", "_________");
                        testo = testo.Replace("#DATAAL#", "_________");
                    }

                    imgPhrase = new Paragraph(new Chunk(drawCircle(cb, (selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta33) > 0)), 1f, 0f));
                    imgPhrase.Add(new Chunk(testo, myFontCorpo));
                    p = new Paragraph(imgPhrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #region SCELTA 331
                    tempTX = ModuloSmart2020SelectionEnum.Scelta33_1.GetDescription();
                    var Scelta33_1 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta33_1) > 0;

                    tableDetail = new PdfPTable(3);
                    tableDetail.DefaultCell.BorderWidth = 0;
                    tableDetail.TotalWidth = 550;
                    tableDetail.LockedWidth = true;
                    tableDetail.SetWidths(tableDetailWidth);
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta33_1), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));
                    #endregion

                    #region SCELTA 332

                    tempTX = ModuloSmart2020SelectionEnum.Scelta23_2.GetDescription();
                    var Scelta33_2 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta33_2) > 0;
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta33_2), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));

                    #endregion

                    #region SCELTA 333

                    tempTX = ModuloSmart2020SelectionEnum.Scelta33_3.GetDescription();
                    var Scelta33_3 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta33_3) > 0;
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta33_3), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));

                    #endregion

                    document.Add(tableDetail);
                    tableDetail.FlushContent();
                    tableDetail.DeleteBodyRows();

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #endregion

                    #region SELEZIONE 24
                    testo = ModuloSmart2020SelectionEnum.Scelta24.GetDescription();
                    var Scelta24 = selezioni.Where(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta24).FirstOrDefault();
                    if (Scelta24 != null)
                    {
                        dt1 = Scelta24.DataSelezionataDal.Value;
                        dt2 = Scelta24.DataSelezionataAl.Value;
                        testo = testo.Replace("#DATADAL#", dt1.ToString("dd/MM/yyyy"));
                        testo = testo.Replace("#DATAAL#", dt2.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        testo = testo.Replace("#DATADAL#", "_________");
                        testo = testo.Replace("#DATAAL#", "_________");
                    }

                    imgPhrase = new Paragraph(new Chunk(drawCircle(cb, (selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta24) > 0)), 1f, 0f));
                    imgPhrase.Add(new Chunk(testo, myFontCorpo));
                    p = new Paragraph(imgPhrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #region SCELTA 241
                    tempTX = ModuloSmart2020SelectionEnum.Scelta24_1.GetDescription();
                    var Scelta24_1 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta24_1) > 0;

                    tableDetail = new PdfPTable(3);
                    tableDetail.DefaultCell.BorderWidth = 0;
                    tableDetail.TotalWidth = 550;
                    tableDetail.LockedWidth = true;
                    tableDetail.SetWidths(tableDetailWidth);
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta24_1), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));
                    #endregion

                    #region SCELTA 242

                    tempTX = ModuloSmart2020SelectionEnum.Scelta24_2.GetDescription();
                    var Scelta24_2 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta24_2) > 0;
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta24_2), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));

                    #endregion

                    #region SCELTA 243

                    tempTX = ModuloSmart2020SelectionEnum.Scelta24_3.GetDescription();
                    var Scelta24_3 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta24_3) > 0;
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta24_3), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));

                    #endregion

                    document.Add(tableDetail);
                    tableDetail.FlushContent();
                    tableDetail.DeleteBodyRows();

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #endregion

                    #region SELEZIONE 25
                    testo = ModuloSmart2020SelectionEnum.Scelta25.GetDescription();
                    var Scelta25 = selezioni.Where(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta25).FirstOrDefault();
                    if (Scelta25 != null)
                    {
                        dt1 = Scelta25.DataSelezionataDal.Value;
                        dt2 = Scelta25.DataSelezionataAl.Value;
                        testo = testo.Replace("#DATADAL#", dt1.ToString("dd/MM/yyyy"));
                        testo = testo.Replace("#DATAAL#", dt2.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        testo = testo.Replace("#DATADAL#", "_________");
                        testo = testo.Replace("#DATAAL#", "_________");
                    }

                    imgPhrase = new Paragraph(new Chunk(drawCircle(cb, (selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta25) > 0)), 1f, 0f));
                    imgPhrase.Add(new Chunk(testo, myFontCorpo));
                    p = new Paragraph(imgPhrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #endregion

                    #region SELEZIONE 26
                    testo = ModuloSmart2020SelectionEnum.Scelta26.GetDescription();
                    var Scelta26 = selezioni.Where(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta26).FirstOrDefault();
                    if (Scelta26 != null)
                    {
                        dt1 = Scelta26.DataSelezionataDal.Value;
                        //dt2 = Scelta26.DataSelezionataAl.Value;
                        testo = testo.Replace("#DATADAL#", dt1.ToString("dd/MM/yyyy"));
                        //testo = testo.Replace("#DATAAL#", dt2.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        testo = testo.Replace("#DATADAL#", "_________");
                        //testo = testo.Replace("#DATAAL#", "_________");
                    }

                    imgPhrase = new Paragraph(new Chunk(drawCircle(cb, (selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta26) > 0)), 1f, 0f));
                    imgPhrase.Add(new Chunk(testo, myFontCorpo));
                    p = new Paragraph(imgPhrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #endregion

                    #region SELEZIONE 27
                    testo = ModuloSmart2020SelectionEnum.Scelta27.GetDescription();
                    var Scelta27 = selezioni.Where(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta27).FirstOrDefault();
                    if (Scelta27 != null)
                    {
                        dt1 = Scelta27.DataSelezionataDal.Value;
                        dt2 = Scelta27.DataSelezionataAl.Value;
                        testo = testo.Replace("#DATADAL#", dt1.ToString("dd/MM/yyyy"));
                        testo = testo.Replace("#DATAAL#", dt2.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        testo = testo.Replace("#DATADAL#", "_________");
                        testo = testo.Replace("#DATAAL#", "_________");
                    }

                    imgPhrase = new Paragraph(new Chunk(drawCircle(cb, (selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta27) > 0)), 1f, 0f));
                    imgPhrase.Add(new Chunk(testo, myFontCorpo));
                    p = new Paragraph(imgPhrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #region SCELTA 271
                    tempTX = ModuloSmart2020SelectionEnum.Scelta27_1.GetDescription();
                    var Scelta27_1 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta27_1) > 0;

                    tableDetail = new PdfPTable(3);
                    tableDetail.DefaultCell.BorderWidth = 0;
                    tableDetail.TotalWidth = 550;
                    tableDetail.LockedWidth = true;
                    tableDetail.SetWidths(tableDetailWidth);
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta27_1), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));
                    #endregion

                    #region SCELTA 272

                    tempTX = ModuloSmart2020SelectionEnum.Scelta27_2.GetDescription();
                    var Scelta27_2 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta27_2) > 0;
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta27_2), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));

                    #endregion

                    #region SCELTA 273

                    tempTX = ModuloSmart2020SelectionEnum.Scelta27_3.GetDescription();
                    var Scelta27_3 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta27_3) > 0;
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta27_3), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));

                    #endregion

                    document.Add(tableDetail);
                    tableDetail.FlushContent();
                    tableDetail.DeleteBodyRows();

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #endregion

                    #region SELEZIONE 34
                    testo = ModuloSmart2020SelectionEnum.Scelta34.GetDescription();
                    var Scelta34 = selezioni.Where(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta34).FirstOrDefault();
                    if (Scelta34 != null)
                    {
                        dt1 = Scelta34.DataSelezionataDal.Value;
                        dt2 = Scelta34.DataSelezionataAl.Value;
                        testo = testo.Replace("#DATADAL#", dt1.ToString("dd/MM/yyyy"));
                        testo = testo.Replace("#DATAAL#", dt2.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        testo = testo.Replace("#DATADAL#", "_________");
                        testo = testo.Replace("#DATAAL#", "_________");
                    }

                    imgPhrase = new Paragraph(new Chunk(drawCircle(cb, (selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta34) > 0)), 1f, 0f));
                    imgPhrase.Add(new Chunk(testo, myFontCorpo));
                    p = new Paragraph(imgPhrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #region SCELTA 341
                    tempTX = ModuloSmart2020SelectionEnum.Scelta34_1.GetDescription();
                    var Scelta34_1 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta34_1) > 0;

                    tableDetail = new PdfPTable(3);
                    tableDetail.DefaultCell.BorderWidth = 0;
                    tableDetail.TotalWidth = 550;
                    tableDetail.LockedWidth = true;
                    tableDetail.SetWidths(tableDetailWidth);
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta34_1), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));
                    #endregion

                    #region SCELTA 342

                    tempTX = ModuloSmart2020SelectionEnum.Scelta34_2.GetDescription();
                    var Scelta34_2 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta34_2) > 0;
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta34_2), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));

                    #endregion

                    #region SCELTA 343

                    tempTX = ModuloSmart2020SelectionEnum.Scelta34_3.GetDescription();
                    var Scelta34_3 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta34_3) > 0;
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta34_3), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));

                    #endregion

                    document.Add(tableDetail);
                    tableDetail.FlushContent();
                    tableDetail.DeleteBodyRows();

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #endregion

                    #region SELEZIONE 28
                    testo = ModuloSmart2020SelectionEnum.Scelta28.GetDescription();
                    var Scelta28 = selezioni.Where(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta28).FirstOrDefault();
                    if (Scelta28 != null)
                    {
                        dt1 = Scelta28.DataSelezionataDal.Value;
                        dt2 = Scelta28.DataSelezionataAl.Value;
                        testo = testo.Replace("#DATADAL#", dt1.ToString("dd/MM/yyyy"));
                        testo = testo.Replace("#DATAAL#", dt2.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        testo = testo.Replace("#DATADAL#", "_________");
                        testo = testo.Replace("#DATAAL#", "_________");
                    }

                    imgPhrase = new Paragraph(new Chunk(drawCircle(cb, (selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta28) > 0)), 1f, 0f));
                    imgPhrase.Add(new Chunk(testo, myFontCorpo));
                    p = new Paragraph(imgPhrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #region SCELTA 281
                    tempTX = ModuloSmart2020SelectionEnum.Scelta28_1.GetDescription();
                    var Scelta28_1 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta28_1) > 0;

                    tableDetail = new PdfPTable(3);
                    tableDetail.DefaultCell.BorderWidth = 0;
                    tableDetail.TotalWidth = 550;
                    tableDetail.LockedWidth = true;
                    tableDetail.SetWidths(tableDetailWidth);
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta28_1), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));
                    #endregion

                    #region SCELTA 282

                    tempTX = ModuloSmart2020SelectionEnum.Scelta28_2.GetDescription();
                    var Scelta28_2 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta28_2) > 0;
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta28_2), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));

                    #endregion

                    #region SCELTA 283

                    tempTX = ModuloSmart2020SelectionEnum.Scelta28_3.GetDescription();
                    var Scelta28_3 = selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta28_3) > 0;
                    tableDetail.AddCell(WriteCell(" ", 0, 1, 0, myFontCorpo, 7));
                    tableDetail.AddCell(WriteCell(drawCircle(cb, Scelta28_3), 0, 1, 0));
                    tableDetail.AddCell(WriteCell(tempTX, 0, 1, 0, myFontCorpo, 7));

                    #endregion

                    document.Add(tableDetail);
                    tableDetail.FlushContent();
                    tableDetail.DeleteBodyRows();

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #endregion

                    #region SELEZIONE 29
                    testo = ModuloSmart2020SelectionEnum.Scelta29.GetDescription();
                    var Scelta29 = selezioni.Where(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta29).FirstOrDefault();
                    if (Scelta29 != null)
                    {
                        dt1 = Scelta29.DataSelezionataDal.Value;
                        dt2 = Scelta29.DataSelezionataAl.Value;
                        testo = testo.Replace("#DATADAL#", dt1.ToString("dd/MM/yyyy"));
                        testo = testo.Replace("#DATAAL#", dt2.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        testo = testo.Replace("#DATADAL#", "_________");
                        testo = testo.Replace("#DATAAL#", "_________");
                    }

                    imgPhrase = new Paragraph(new Chunk(drawCircle(cb, (selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta29) > 0)), 1f, 0f));
                    imgPhrase.Add(new Chunk(testo, myFontCorpo));
                    p = new Paragraph(imgPhrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #endregion

                    #region SELEZIONE 30
                    testo = ModuloSmart2020SelectionEnum.Scelta30.GetDescription();
                    var Scelta30 = selezioni.Where(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta30).FirstOrDefault();
                    if (Scelta30 != null)
                    {
                        dt1 = Scelta30.DataSelezionataDal.Value;
                        //dt2 = Scelta30.DataSelezionataAl.Value;
                        testo = testo.Replace("#DATADAL#", dt1.ToString("dd/MM/yyyy"));
                        //testo = testo.Replace("#DATAAL#", dt2.ToString("dd/MM/yyyy"));
                    }
                    else
                    {
                        testo = testo.Replace("#DATADAL#", "_________");
                        //testo = testo.Replace("#DATAAL#", "_________");
                    }

                    imgPhrase = new Paragraph(new Chunk(drawCircle(cb, (selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta30) > 0)), 1f, 0f));
                    imgPhrase.Add(new Chunk(testo, myFontCorpo));
                    p = new Paragraph(imgPhrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #endregion

                    #region SELEZIONE 31
                    testo = ModuloSmart2020SelectionEnum.Scelta31.GetDescription();
                    var Scelta31 = selezioni.Where(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta31).FirstOrDefault();

                    imgPhrase = new Paragraph(new Chunk(drawCircle(cb, (selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta31) > 0)), 1f, 0f));
                    imgPhrase.Add(new Chunk(testo, myFontCorpo));
                    p = new Paragraph(imgPhrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk("Sono consapevole che il diritto allo svolgimento della prestazione nella modalita' del lavoro agile e' escluso ove l'attivita' lavorativa \"richieda necessariamente la presenza fisica\".", myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);
                    #endregion

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    #region SELEZIONE 32
                    testo = ModuloSmart2020SelectionEnum.Scelta32.GetDescription();
                    var Scelta32 = selezioni.Where(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta32).FirstOrDefault();

                    imgPhrase = new Paragraph(new Chunk(drawCircle(cb, (selezioni.Count(w => w.Selezione == ModuloSmart2020SelectionEnum.Scelta32) > 0)), 1f, 0f));
                    imgPhrase.Add(new Chunk(testo, myFontCorpo));
                    p = new Paragraph(imgPhrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk("Sono consapevole che il diritto allo svolgimento della prestazione nella modalita' del lavoro agile e' escluso ove l'attivita' lavorativa \"richieda necessariamente la presenza fisica\".", myFontCorpo));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);
                    #endregion

                    phrase = new Phrase();
                    phrase.Add(new Chunk(" ", myFontIntestazioneBold));
                    p = new Paragraph(phrase);
                    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add(p);

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( "Mi impegno a comunicare, tempestivamente," , myFontCorpo ) );

                    phrase.Add( new Chunk( "qualsiasi variazione di quanto oggetto della presente richiesta" , myFontCorpoSottolineato ) );

                    phrase.Add( new Chunk(", tenuto conto che il venir meno di uno dei requisiti legittimanti comporta la decadenza dal diritto in questione (es. compimento del 16-esimo anno di eta' del figlio, intervenuta guarigione dall'infezione da SARS COVID–19, ecc.)." , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add(new Chunk("Sono infine disponibile a presentare ogni utile documentazione (es. dichiarazione sullo " , myFontCorpo));
                    phrase.Add(new Chunk("status" , myFontCorpoItalic));
                    phrase.Add(new Chunk(" lavorativo dell'altro genitore, verbale di riconoscimento della disabilita', certificazione attestante la sussistenza di disturbi specifici dell'apprendimento o di bisogni educativi speciali, certificazione del servizio di medicina legale della ASL che attesti una situazione di rischio derivante da immunodepressione, esiti da patologie oncologiche o dallo svolgimento di relative terapie salvavita, ecc.), che potra' essermi eventualmente richiesta, ivi incluso, se necessario, l'eventuale consenso al trattamento ove si tratti di dati di soggetti terzi non gia' noti all'Azienda, ai sensi dell'attuale disciplina sulla ", myFontCorpo));
                    phrase.Add(new Chunk("privacy." , myFontCorpoItalic));
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
        private void InvioMail ( string matricola , byte[] pdf )
        {
            try
            {
                string dest = CommonTasks.GetEmailPerMatricola( matricola );

                if ( String.IsNullOrWhiteSpace( dest ) )
                    dest = "P" + matricola + "@rai.it";

                // per test
                //dest = "RUO.SIP.PRESIDIOOPEN@rai.it";
                //dest = "francesco.buonavita80@gmail.com";

                GestoreMail mail = new GestoreMail( );

                List<Attachement> attachments = new List<Attachement>( );

                Attachement a = new Attachement( )
                {
                    AttachementName = "Modulo.pdf" ,
                    AttachementType = "Application/PDF" ,
                    AttachementValue = pdf
                };

                attachments.Add( a );

                string corpo = "In allegato alla presente il modulo compilato. ";
                var response = mail.InvioMail( "[CG] RaiPlace - Self Service <raiplace.selfservice@rai.it>" ,
                    " SelfService del Dipendente - Notifiche ESERCIZIO DIRITTO SMART WORKING" ,
                    dest ,
                    "raiplace.selfservice@rai.it" ,
                    "ESERCIZIO DIRITTO SMART WORKING" ,
                    "" ,
                    corpo ,
                    null ,
                    null ,
                    attachments );

                if ( response != null && response.Errore != null )
                {
                    MyRai_LogErrori err = new MyRai_LogErrori( )
                    {
                        applicativo = "Portale" ,
                        data = DateTime.Now ,
                        provenienza = "ModuliController - InvioMail" ,
                        error_message = response.Errore + " per " + dest
                    };

                    using ( digiGappEntities db = new digiGappEntities( ) )
                    {
                        db.MyRai_LogErrori.Add( err );
                        db.SaveChanges( );
                    }
                }

                // reperimento degli indirizzi email dei dipendendi degli uffici del personale
            }
            catch ( Exception ex )
            {
                MyRai_LogErrori err = new MyRai_LogErrori( )
                {
                    applicativo = "Portale" ,
                    data = DateTime.Now ,
                    provenienza = "ModuliController - WEB - InvioMail" ,
                    error_message = ex.Message
                };

                using ( digiGappEntities db = new digiGappEntities( ) )
                {
                    db.MyRai_LogErrori.Add( err );
                    db.SaveChanges( );
                }
            }
        }

        #endregion


        public ActionResult getFile ( string app )
        {
            try
            {
                string destinatario = null;

                if ( string.IsNullOrWhiteSpace( app ) )
                { throw new InvalidOperationException( "Parametro non valido" ); }

                if ( app.Contains( "ModuloInfortuni" ))   //ROUTE: http://raiperme.intranet.rai.it/Moduli/getFile?app=ModuloInfortuni
                {
                    var srv = new MyRaiService1Client( );
                    ;
                    var ut = srv.GetRecuperaUtente( CommonManager.GetCurrentUserMatricola7chars( ) , DateTime.Now.ToString( "ddMMyyyy" ) );
                    string categoria = ut.data.categoria;
                    string contratto = ut.data.forma_contratto;
                    destinatario = contratto == "9" ? "TI" : "TD";               //Solo per impiegati a Tempo Ind. altrimenti per Tempo Det.
                    if ( categoria.StartsWith( "A0" ) )
                    { destinatario = "A0x"; }    //Solo per Dirigenti (matricola test: 531838)
                    if ( categoria.StartsWith( "A7" ) )
                    { destinatario = "A7x"; }    //Solo per Dirigenti Giornalisti (matricola test: 538311)
                    if ( categoria.StartsWith( "M" ) )
                    { destinatario = "Mxx"; }     //Solo per Giornalisti
                }

                using ( var db = new digiGappEntities( ) )
                {
                    var file = db.MyRai_Moduli.FirstOrDefault( n =>
                             n.codice_applicazione == app &&
                             n.stato_attivo == true &&
                             n.destinatario == destinatario );

                    if ( file == null )
                    { throw new InvalidOperationException( "Allegato non esistente" ); }

                    if ( file.bytes_content == null )
                    { throw new InvalidOperationException( "Allegato non disponibile" ); }

                    byte[] fileContent = file.bytes_content;
                    string fileName = string.IsNullOrWhiteSpace( file.nome ) ? app : file.nome.Trim( );
                    string fileType = string.IsNullOrWhiteSpace( file.estensione ) ? "pdf" : file.estensione.Trim( ).ToLower( );
                    string fileMime = "application/octet-stream";

                    switch ( fileType )
                    {
                        case "pdf":
                            fileMime = "application/pdf";
                            break;
                        case "jpg":
                            fileMime = "image/jpeg";
                            break;
                        case "png":
                            fileMime = "image/png";
                            break;
                        case "bmp":
                            fileMime = "image/bmp";
                            break;
                    }
                    Response.AddHeader( "Content-Disposition" , "inline; filename=" + fileName + "." + fileType );
                    return File( fileContent , fileMime );
                }
            }
            catch ( InvalidOperationException ex )
            {
                return Http404( );
                //return Content("<script language='javascript' type='text/javascript'>alert('" + ex.Message + "');</script>"); 
            }
        }


        #region PROROGA SW

        public ActionResult CompilaProrogaModuloSmartWorking ( string codiceModulo )
        {
            ModuloVM model = new ModuloVM( );
            model.WidgetId = "ProrogaSmartWorkingWidget";
            model.Matricola = CommonManager.GetCurrentUserMatricola( );
            model.Nominativo = String.Format( "{0} {1}" , Utente.EsponiAnagrafica( )._nome , Utente.EsponiAnagrafica( )._cognome );
            model.Sesso = Utente.EsponiAnagrafica( )._genere;
            SetLetturaProrogaModuloSmartWorking( );

            return View( "~/Views/Moduli/ProrogaModuloSmartWorking.cshtml" , model );
        }

        private void SetLetturaProrogaModuloSmartWorking ( )
        {
            try
            {
                string mt = CommonManager.GetCurrentUserMatricola( );
                using ( TalentiaEntities dbTalentia = new TalentiaEntities( ) )
                {
                    bool exist = dbTalentia.XR_MOD_DIPENDENTI.Count( w => w.MATRICOLA.Equals( mt ) && w.COD_MODULO.Equals( "PROROGASMARTW2020" ) ) > 0;

                    //Reperimento dei dati del dipendente
                    XR_MOD_DIPENDENTI toAdd = null;

                    var info = ( from s in dbTalentia.SINTESI1
                                 join q in dbTalentia.QUALIFICA
                                 on s.COD_QUALIFICA equals q.COD_QUALIFICA
                                 where s.COD_MATLIBROMAT == mt
                                 select new
                                 {
                                     ID_PERSONA = s.ID_PERSONA ,
                                     COD_RUOLO = s.COD_RUOLO ,
                                     COD_QUALSTD = q.COD_QUALSTD
                                 } ).FirstOrDefault( );

                    if ( !exist )
                    {
                        if ( info != null )
                        {
                            toAdd = new XR_MOD_DIPENDENTI( )
                            {
                                COD_MANSIONE = info.COD_RUOLO ,
                                COD_MODULO = "PROROGASMARTW2020" ,
                                COD_PROFILO_PROF = info.COD_QUALSTD ,
                                COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress ,
                                COD_USER = CommonManager.GetCurrentUserMatricola( ) ,
                                DATA_COMPILAZIONE = null ,
                                DATA_LETTURA = DateTime.Now ,
                                DATA_INIZIO = null ,
                                DATA_SCADENZA = null ,
                                ID_PERSONA = info.ID_PERSONA ,
                                IND_STATO = "0" ,
                                MATRICOLA = mt ,
                                PDF_MODULO = null ,
                                SCELTA = "" ,
                                TMS_TIMESTAMP = DateTime.Now ,
                                XR_MOD_DIPENDENTI1 = dbTalentia.XR_MOD_DIPENDENTI.GeneraPrimaryKey( )
                            };

                            dbTalentia.XR_MOD_DIPENDENTI.Add( toAdd );
                            dbTalentia.SaveChanges( );
                        }
                        else
                        {
                            throw new Exception( "Impossibile reperire i dati utente" );
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                CommonTasks.LogErrore( ex.Message , "SalvataggioSmart2020 - WEB - Azione Salva" );
            }
        }

        public ActionResult SalvaSceltaProroga ( string scelta )
        {
            string result = "OK";
            DateTime dataCompilazione = DateTime.Now;
            try
            {
                // se tutto ok allora genera il pdf e lo invia tramite mail
                string matricola = CommonManager.GetCurrentUserMatricola( );

                // creazione del pdf
                byte[] myArrayOfBytes = CreazionePDFProroga( CommonManager.GetCurrentUserMatricola( ) ,
                                                    String.Format( "{0} {1}" , Utente.EsponiAnagrafica( )._nome , Utente.EsponiAnagrafica( )._cognome ) ,
                                                    Utente.EsponiAnagrafica( )._genere ,
                                                    scelta ,
                                                    dataCompilazione );

                if ( myArrayOfBytes == null )
                {
                    result = "Errore nella creazione del PDF";
                    throw new Exception( result );
                }

                // salva scelta
                string esito = SalvaProroga( scelta , dataCompilazione , myArrayOfBytes );

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
                        descrizione_operazione = "Salvataggio scelta proroga SW - scelta - " + scelta.ToString( ) ,
                        operazione = "Salvataggio PROROGASMARTW2020" ,
                        provenienza = new StackFrame( 1 , true ).GetMethod( ).Name
                    } , CommonManager.GetCurrentUserMatricola( ) );
                }
                catch ( Exception ex )
                {

                }

                // se tutto ok
                // invia mail con la copia del modulo compilato
                InvioMailProroga( matricola , myArrayOfBytes );

            }
            catch ( Exception ex )
            {
            }

            return Content( result );
        }

        private string SalvaProroga ( string scelta , DateTime dataCompilazione , byte[] modulo )
        {
            string esito = "";
            try
            {
                string mt = CommonManager.GetCurrentUserMatricola( );
                using ( TalentiaEntities dbTalentia = new TalentiaEntities( ) )
                {
                    bool exist = dbTalentia.XR_MOD_DIPENDENTI.Count( w => w.MATRICOLA.Equals( mt ) && w.COD_MODULO.Equals( "PROROGASMARTW2020" ) ) > 0;

                    //Reperimento dei dati del dipendente
                    XR_MOD_DIPENDENTI toAdd = null;

                    var info = ( from s in dbTalentia.SINTESI1
                                 join q in dbTalentia.QUALIFICA
                                 on s.COD_QUALIFICA equals q.COD_QUALIFICA
                                 where s.COD_MATLIBROMAT == mt
                                 select new
                                 {
                                     ID_PERSONA = s.ID_PERSONA ,
                                     COD_RUOLO = s.COD_RUOLO ,
                                     COD_QUALSTD = q.COD_QUALSTD
                                 } ).FirstOrDefault( );

                    if ( !exist )
                    {
                        if ( info != null )
                        {
                            if ( !String.IsNullOrEmpty( scelta ) )
                            {
                                toAdd = new XR_MOD_DIPENDENTI( )
                                {
                                    COD_MANSIONE = info.COD_RUOLO ,
                                    COD_MODULO = "PROROGASMARTW2020" ,
                                    COD_PROFILO_PROF = info.COD_QUALSTD ,
                                    COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress ,
                                    COD_USER = CommonManager.GetCurrentUserMatricola( ) ,
                                    DATA_COMPILAZIONE = dataCompilazione ,
                                    DATA_LETTURA = dataCompilazione,
                                    DATA_INIZIO = null ,
                                    DATA_SCADENZA = null ,
                                    ID_PERSONA = info.ID_PERSONA ,
                                    IND_STATO = "0" ,
                                    MATRICOLA = mt ,
                                    PDF_MODULO = modulo ,
                                    SCELTA = scelta ,
                                    TMS_TIMESTAMP = DateTime.Now ,
                                    XR_MOD_DIPENDENTI1 = dbTalentia.XR_MOD_DIPENDENTI.GeneraPrimaryKey( )
                                };

                                dbTalentia.XR_MOD_DIPENDENTI.Add( toAdd );
                                dbTalentia.SaveChanges( );
                            }
                        }
                        else
                        {
                            throw new Exception( "Impossibile reperire i dati utente" );
                        }
                    }
                    else
                    {
                        if ( info != null )
                        {
                            if ( !String.IsNullOrEmpty( scelta ) )
                            {
                                var toUpdate = dbTalentia.XR_MOD_DIPENDENTI.Where( w => w.MATRICOLA.Equals( mt ) && w.COD_MODULO.Equals( "PROROGASMARTW2020" ) ).FirstOrDefault( );

                                toUpdate.DATA_COMPILAZIONE = dataCompilazione;
                                toUpdate.PDF_MODULO = modulo;
                                toUpdate.SCELTA = scelta;
                                toUpdate.TMS_TIMESTAMP = DateTime.Now;

                                dbTalentia.SaveChanges( );
                            }
                        }
                        else
                        {
                            throw new Exception( "Impossibile reperire i dati utente" );
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                esito = ex.Message;
                CommonTasks.LogErrore( ex.Message , "SalvataggioSmart2020 - WEB - Azione Salva" );
            }

            return esito;
        }

        private static byte[] CreazionePDFProroga ( string matricola , string nominativo , string sesso , string scelta , DateTime dataCreazione )
        {
            try
            {
                var cultureInfo = CultureInfo.GetCultureInfo( "it-IT" );

                byte[] bytes = null;
                const int fontIntestazione = 12;
                const int fontCorpo = 11;
                const int fontNote = 9;

                BaseFont bf = BaseFont.CreateFont( BaseFont.HELVETICA , BaseFont.CP1250 , BaseFont.NOT_EMBEDDED );
                Font myFontIntestazione = new Font( bf , fontIntestazione , Font.NORMAL );
                Font myFontIntestazioneBold = new Font( bf , fontIntestazione , Font.BOLD );
                Font myFontCorpo = new Font( bf , fontCorpo , Font.NORMAL );
                Font myFontCorpoBold = new Font( bf , fontCorpo , Font.BOLD );
                Font myFontNote = new Font( bf , fontNote , Font.NORMAL );
                Font myFontCorpoSottolineato = new Font( bf , fontCorpo , Font.UNDERLINE );
                Font myFontCorpoItalic = new Font( bf , fontCorpo , Font.ITALIC );
                Font myFontCorpoItalicBold = new Font( bf , fontCorpo , Font.BOLDITALIC );

                using ( MemoryStream ms = new MemoryStream( ) )
                {
                    // Creazione dell'istanza del documento, impostando tipo di pagina e margini
                    Document document = new Document( PageSize.A4 , 25 , 25 , 25 , 25 );
                    PdfWriter writer = PdfWriter.GetInstance( document , ms );

                    document.Open( );

                    PdfContentByte cb = writer.DirectContent;

                    string _imgPath = System.Web.HttpContext.Current.Server.MapPath( "~/assets/img/LogoPDF.png" );

                    iTextSharp.text.Image png = iTextSharp.text.Image.GetInstance( _imgPath );
                    png.ScaleAbsolute( 45 , 45 );
                    png.SetAbsolutePosition( 25 , 750 );
                    cb.AddImage( png );

                    Phrase phrase = new Phrase( );
                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );

                    Paragraph p = new Paragraph( phrase );
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
                    phrase.Add( new Chunk( nominativo , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_RIGHT;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( matricola , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_RIGHT;
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
                    phrase.Add( new Chunk( "Dopo una prima fase emergenziale, in ragione del miglioramento della situazione sanitaria sul piano nazionale e nella prospettiva di garantire la graduale e piena ripresa dell'attivita' produttiva, l'Azienda sta realizzando un graduale, progressivo e sicuro rientro del personale presso le sedi di appartenenza, sempre nel piu' assoluto rispetto delle disposizioni vigenti in materia di sicurezza dei luoghi di lavoro." , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( "Il " , myFontCorpo ) );
                    phrase.Add( new Chunk( "15 ottobre p.v. " , myFontCorpoBold ) );


                    phrase.Add( new Chunk( "avra' termine, salvo proroga, lo stato di emergenza, in forza del quale i datori di lavoro hanno potuto collocare il personale in " , myFontCorpo ) );

                    phrase.Add( new Chunk( "smart working" , myFontCorpoItalic ) );

                    phrase.Add( new Chunk( " sulla base di una semplice comunicazione unilaterale al singolo lavoratore." , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );                    

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( "Visto l'attuale contesto sanitario, l'Azienda ritiene comunque ancora essenziale continuare a ricorrere a tale modalita' di svolgimento della prestazione lavorativa come misura di prevenzione del contagio da COVID – 19 , cosi' da poter attuare il necessario distanziamento all'interno dei locali aziendali , dovendo l'attivita' produttiva svolgersi nel pieno rispetto della disciplina in materia di salute e sicurezza sui luoghi di lavoro." , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( "Cio' premesso e considerando, altresi', che, in base alla disciplina ordinaria in materia , il ricorso al lavoro agile risulta subordinato alla conclusione di un accordo individuale con il singolo dipendente interessato , ai sensi dell'art. 18 della legge 22 maggio 2017 , n. 81 , " , myFontCorpo ) );

                    bool accettato = bool.Parse( scelta );

                    if ( accettato )
                    {
                        phrase.Add( new Chunk( "ha accettato in data " + dataCreazione.ToString("dd/MM/yyyy HH:mm:ss") + " di rendere la Sua attivita' lavorativa in regime di " , myFontCorpoBold ) );
                    }
                    else
                    {
                        phrase.Add( new Chunk( "ha rifiutato in data " + dataCreazione.ToString( "dd/MM/yyyy HH:mm:ss" ) + " di rendere la Sua attivita' lavorativa in regime di " , myFontCorpoBold ) );
                    }

                    phrase.Add( new Chunk( "smart working" , myFontCorpoItalicBold ) );

                    phrase.Add( new Chunk( " dal 16 ottobre 2020 fino al 31 gennaio 2021" , myFontCorpoBold ) );

                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( "Le forme, le modalita' e l'articolazione della prestazione, determinate in forza delle esigenze organizzative e produttive manifestate dalla Sua Struttura / Direzione di appartenenza , restano quelle gia' in essere ovvero Le saranno comunicate con anticipo in caso di loro eventuale modifica oppure in caso di prima collocazione o successiva ricollocazione in " , myFontCorpo ) );

                    phrase.Add( new Chunk( "smart working." , myFontCorpoItalic ) );

                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( "Fatta peraltro salva l'ipotesi di " , myFontCorpo ) );
                    phrase.Add( new Chunk( "smart working" , myFontCorpoItalic ) );
                    phrase.Add( new Chunk( " continuativo - relativo cioe' alla prestazione \"da remoto\" per l'intero orario di lavoro settimanale -, anche legata a particolari condizioni del lavoratore oggetto di specifica tutela , laddove , in relazione alle esigenze organizzative e produttive della struttura / Direzione di appartenenza , sia necessaria un'alternanza tra attivita' lavorativa \"in presenza\" ed attivita' \"da remoto\" su base settimanale / plurisettimanale , ecc., tale alternanza , cosi' come pianificata dal Responsabile di riferimento , sara' comunicata al singolo dipendente con preavviso a mezzo e-mail , non appena disponibile la relativa programmazione." , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( "E', in generale, confermata la possibilita', durante lo " , myFontCorpo ) );
                    phrase.Add( new Chunk( "smart working" , myFontCorpoItalic ) );
                    phrase.Add( new Chunk( ", sempre nel rispetto delle norme in materia distanziamento sui luoghi di lavoro e delle altre misure di prevenzione , di chiamare comunque il lavoratore a rendere la prestazione \"in presenza\", a fronte di sopravvenute esigenze lavorative , da parte del Responsabile di riferimento , anche in giornate diverse da quelle pianificate; le eventuali giornate che possano richiedere la prestazione \"in presenza\" saranno rese note con preavviso." , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase.Add( new Chunk( "L'attivita' lavorativa da remoto potra' essere svolta presso l'abitazione del dipendente ovvero altro luogo dallo stesso ritenuto idoneo , con le dovute cautele ed accortezze necessarie in questa fase ancora emergenziale." , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( "Per quanto concerne la disciplina applicabile alla prestazione in regime di " , myFontCorpo ) );
                    phrase.Add( new Chunk( "smart working" , myFontCorpoItalic ) );
                    phrase.Add( new Chunk( ", si rimanda, oltre che agli artt. 18 e ss. Legge 22 maggio 2017, n. 81, all'apposito Regolamento disponibile nell'intranet aziendale , che abbiamo provveduto ad aggiornare in alcuni passaggi , alla luce delle sopravvenute novita' normative, lo scorso luglio. Cogliamo , altresi' , l'occasione per ribadire la necessita' di costante consultazione della apposita sezione di RAI Place dedicata allo " , myFontCorpo ) );
                    phrase.Add( new Chunk( "smart working" , myFontCorpoItalic ) );
                    phrase.Add( new Chunk( "anche sotto i profili relativi alla salute e sicurezza, ricordando che, in quella sezione, e' possibile trovare tutta la documentazione aziendale allo scopo rilevante ed i supporti tecnici necessari." , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( "L'Azienda si riserva, in generale, la possibilita' di far cessare anticipatamente lo " , myFontCorpo ) );
                    phrase.Add( new Chunk( "smart working" , myFontCorpoItalic ) );
                    phrase.Add( new Chunk( " rispetto alla data comunicata - dandone adeguato preavviso -, a fronte di esigenze produttive e organizzative che richiedano , in modo costante , lo svolgimento della prestazione lavorativa \"in presenza\"." , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontCorpo ) );
                        p = new Paragraph( phrase );
                        ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                        document.Add( p );

                        phrase = new Phrase( );
                    phrase.Add( new Chunk( "Resta fermo che, laddove venga prorogato lo stato di emergenza, ai sensi dell'art. 90 del D.L.n. 34 / 2020 , convertito nella L.n. 77 / 2020 , la presente proposta cessera' di avere efficacia e l'Azienda potra' esercitare la facolta' di disporre in via unilaterale la proroga o la collocazione in " , myFontCorpo ) );
                    phrase.Add( new Chunk( "smart working." , myFontCorpoItalic ) );
                        p = new Paragraph( phrase );
                        ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                        document.Add( p );

                        phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontCorpo ) );
                        p = new Paragraph( phrase );
                        ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                        document.Add( p );

                        phrase = new Phrase( );
                    phrase.Add( new Chunk( "Da ultimo, rammentiamo che il lavoratore in " , myFontCorpo ) );
                    phrase.Add( new Chunk( "smart working" , myFontCorpoItalic ) );
                    phrase.Add( new Chunk( " e' comunque tenuto alla fruizione delle ferie secondo la pianificazione a suo tempo presentata ed approvata dal Responsabile." , myFontCorpo ) );
                        p = new Paragraph( phrase );
                        ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                        document.Add( p );

                        phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontCorpo ) );
                        p = new Paragraph( phrase );
                        ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                        document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    if ( accettato )
                    {
                        phrase = new Phrase( );
                        phrase.Add( new Chunk( "Modulo accettato elettronicamente " + dataCreazione.ToString( "dd/MM/yyyy" ) , myFontCorpo ) );
                        p = new Paragraph( phrase );
                        ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                        document.Add( p );
                    }
                    else
                    {
                        phrase = new Phrase( );
                        phrase.Add( new Chunk( "Modulo rifiutato elettronicamente " + dataCreazione.ToString( "dd/MM/yyyy" ) , myFontCorpo ) );
                        p = new Paragraph( phrase );
                        ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                        document.Add( p );
                    }

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

        public ActionResult RicaricaProrogaSWWidget ( )
        {
            SectionDayModel model = new SectionDayModel( );
            List<WidgetModuloBox_Azione> btns = new List<WidgetModuloBox_Azione>( );

            bool giaScelto = false;
            string sceltaSmart2020 = "";
            DateTime? dataCompilazioneSW2020 = null;
            DateTime? dataLettura = null;

            // verifica se l'utente ha già effettuato la scelta
            string mt = CommonManager.GetCurrentUserMatricola( );
            using ( TalentiaEntities dbTalentia = new TalentiaEntities( ) )
            {
                bool exist = dbTalentia.XR_MOD_DIPENDENTI.Count( w => w.MATRICOLA.Equals( mt ) && w.COD_MODULO.Equals( "PROROGASMARTW2020" ) ) > 0;

                if ( !exist )
                {
                    giaScelto = false;
                }
                else
                {
                    var item = dbTalentia.XR_MOD_DIPENDENTI.Where( w => w.MATRICOLA.Equals( mt ) && w.COD_MODULO.Equals( "PROROGASMARTW2020" ) ).FirstOrDefault( );

                    if ( item != null )
                    {
                        DateTime? dtComp = item.DATA_COMPILAZIONE;
                        dataCompilazioneSW2020 = dtComp;
                        dataLettura = item.DATA_LETTURA;
                        giaScelto = true;
                        sceltaSmart2020 = item.SCELTA;
                    }
                }
            }

            if ( giaScelto )
            {
                btns.Add( new WidgetModuloBox_Azione( )
                {
                    TestoBottone = "Visualizza" ,
                    UrlBottone = Url.Action( "VisualizzaModuloProrogaSmartWorkingReadOnly" , "Moduli" , new { codiceModulo = "PROROGASMARTW2020" } )
                } );

                model.SmartWorkingWidget = new WidgetModuloBox( )
                {
                    WidgetId = "WdgProrogaSmartWorking" ,
                    Anno = DateTime.Now.Year ,
                    GiaScelto = true ,
                    HaDiritto = true ,
                    Titolo = "Modulo accordo smart working" ,
                    Scelta = sceltaSmart2020 ,
                    DataLettura = dataLettura,
                    DataCompilazione = dataCompilazioneSW2020 ,
                    Sottotitolo = "Modulo compilato in data " + dataCompilazioneSW2020.GetValueOrDefault( ).ToString( "dd/MM/yyyy" ) ,
                    Bottoni = btns
                };
            }
            else
            {
                btns.Add( new WidgetModuloBox_Azione( )
                {
                    TestoBottone = "Compila" ,
                    UrlBottone = Url.Action( "CompilaProrogaModuloSmartWorking" , "Moduli" , new { codiceModulo = "PROROGASMARTW2020" } )
                } );

                model.SmartWorkingWidget = new WidgetModuloBox( )
                {
                    WidgetId = "WdgProrogaSmartWorking" ,
                    Anno = DateTime.Now.Year ,
                    GiaScelto = giaScelto ,
                    HaDiritto = true ,
                    Titolo = "Modulo accordo smart working" ,
                    Scelta = sceltaSmart2020 ,
                    DataLettura = dataLettura ,
                    DataCompilazione = null ,
                    Bottoni = btns
                };
            }
            return View( "~/Views/Scrivania/subpartial/boxModulo.cshtml" , model.SmartWorkingWidget );
        }

        private void InvioMailProroga ( string matricola , byte[] pdf )
        {
            try
            {
                string dest = CommonTasks.GetEmailPerMatricola( matricola );

                if ( String.IsNullOrWhiteSpace( dest ) )
                    dest = "P" + matricola + "@rai.it";

                //// per test
                //dest = "RUO.SIP.PRESIDIOOPEN@rai.it";

                GestoreMail mail = new GestoreMail( );

                List<Attachement> attachments = new List<Attachement>( );

                Attachement a = new Attachement( )
                {
                    AttachementName = "Modulo.pdf" ,
                    AttachementType = "Application/PDF" ,
                    AttachementValue = pdf
                };

                attachments.Add( a );

                string corpo = "In allegato alla presente mail il \"MODULO ACCORDO SMART WORKING\" da lei compilato. ";
                var response = mail.InvioMail( "[CG] RaiPlace - Self Service <raiplace.selfservice@rai.it>" ,
                    " SelfService del Dipendente - Notifiche MODULO ACCORDO SMART WORKING" ,
                    dest ,
                    "raiplace.selfservice@rai.it" ,
                    "MODULO ACCORDO SMART WORKING" ,
                    "Copia modulo accordo smart working" ,
                    corpo ,
                    "VAI AL SITO" ,
                    "http://raiperme.rai.it" ,
                    attachments );

                if ( response != null && response.Errore != null )
                {
                    MyRai_LogErrori err = new MyRai_LogErrori( )
                    {
                        applicativo = "Portale" ,
                        data = DateTime.Now ,
                        provenienza = "ModuliController - InvioMailProroga" ,
                        error_message = response.Errore + " per " + dest
                    };

                    using ( digiGappEntities db = new digiGappEntities( ) )
                    {
                        db.MyRai_LogErrori.Add( err );
                        db.SaveChanges( );
                    }
                }

                // reperimento degli indirizzi email dei dipendendi degli uffici del personale
            }
            catch ( Exception ex )
            {
                MyRai_LogErrori err = new MyRai_LogErrori( )
                {
                    applicativo = "Portale" ,
                    data = DateTime.Now ,
                    provenienza = "ModuliController - WEB - InvioMailProroga" ,
                    error_message = ex.Message
                };

                using ( digiGappEntities db = new digiGappEntities( ) )
                {
                    db.MyRai_LogErrori.Add( err );
                    db.SaveChanges( );
                }
            }
        }

        public ActionResult VisualizzaModuloProrogaSmartWorkingReadOnly ( string codiceModulo )
        {

            ModuloVM model = new ModuloVM( );
            model.WidgetId = "WdgProrogaSmartWorking";
            model.Matricola = CommonManager.GetCurrentUserMatricola( );
            model.Nominativo = String.Format( "{0} {1}" , Utente.EsponiAnagrafica( )._nome , Utente.EsponiAnagrafica( )._cognome );
            model.Sesso = Utente.EsponiAnagrafica( )._genere;

            DateTime? dataCompilazioneSW2020 = null;

            // verifica se l'utente ha già effettuato la scelta
            string mt = CommonManager.GetCurrentUserMatricola( );
            using ( TalentiaEntities dbTalentia = new TalentiaEntities( ) )
            {
                bool exist = dbTalentia.XR_MOD_DIPENDENTI.Count( w => w.MATRICOLA.Equals( mt ) && w.COD_MODULO.Equals( codiceModulo ) ) > 0;

                if ( !exist )
                {
                    return null;
                }
                else
                {
                    var item = dbTalentia.XR_MOD_DIPENDENTI.Where( w => w.MATRICOLA.Equals( mt ) && w.COD_MODULO.Equals( codiceModulo ) ).FirstOrDefault( );

                    if ( item != null )
                    {
                        DateTime? dtComp = item.DATA_COMPILAZIONE;
                        dataCompilazioneSW2020 = dtComp;
                        model.Scelta1 = bool.Parse( item.SCELTA );
                    }
                }
            }

            model.DataCompilazione = dataCompilazioneSW2020;

            return View( "~/Views/Moduli/ProrogaModuloSmartWorkingReadOnly.cshtml" , model );
        }

        


        #endregion



        //LOADER FILE (LASCIARE COMMENTATO)
        //public ActionResult loadFile(string app, string dest, string titolo, string ext, string descr)
        //{
        //    try
        //    {
        //        string filename = @"D:\"+dest+".pdf";  //path file locale da caricare
        //        byte[] file = System.IO.File.ReadAllBytes(filename);
        //        using (var db = new digiGappEntities())
        //        {
        //            db.MyRai_Moduli.Add(new MyRai_Moduli
        //            {
        //                codice_applicazione = app,
        //                destinatario = dest,
        //                stato_attivo = true,
        //                bytes_content = file,
        //                nome = titolo,
        //                estensione = ext,
        //                descrittiva = descr,
        //                data_caricamento = DateTime.Now,
        //                matricola_caricamento = "P" + CommonManager.GetCurrentRealUsername(),
        //            });
        //            db.SaveChanges();
        //        }
        //        return Content("<script language='javascript' type='text/javascript'>alert('Caricamento file RIUSCITO!');</script>");
        //    }
        //    catch (Exception)
        //    {
        //        return Content("<script language='javascript' type='text/javascript'>alert('Caricamento file NON RIUSCITO!');</script>");
        //    }
        //}

        #region RINUNCIA 2020

        private ModuloVM GetModuloRinunciaModel()
        {
            ModuloVM model = new ModuloVM( );
            model.WidgetId = "WdgRinuncia2020";
            model.Matricola = CommonManager.GetCurrentUserMatricola( );
            model.Nominativo = String.Format( "{0} {1}" , Utente.EsponiAnagrafica( )._nome.Trim( ) , Utente.EsponiAnagrafica( )._cognome.Trim( ) );
            model.Sesso = Utente.EsponiAnagrafica( )._genere;
            model.DataNascita = Utente.EsponiAnagrafica( )._dataNascita;
            model.LuogoNascita = Utente.EsponiAnagrafica( )._comuneNascita;
            model.CodiceFiscale = Utente.EsponiAnagrafica( )._cf;
            List<ParametriSistemaValoreJson> parametriDB = parametriDB = CommonManager.GetParametriJson(EnumParametriSistema.ModuloRinuncia2020Params);
            model.Anno = DateTime.Now.Year;

            if (parametriDB != null && parametriDB.Any( ))
            {
                var moduloRinuncia2020AnnoRiferimento = parametriDB.Where(w => w.Attributo == "ModuloRinuncia2020AnnoRiferimento").FirstOrDefault( );

                if (moduloRinuncia2020AnnoRiferimento != null)
                {
                    if (!String.IsNullOrEmpty(moduloRinuncia2020AnnoRiferimento.Valore1))
                    {
                        model.Anno = int.Parse(moduloRinuncia2020AnnoRiferimento.Valore1);
                    }
                }
            }

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
                    _client.ClientCredentials.Windows.ClientCredential = CommonHelper.GetUtenteServizioCredentials();
                    _client.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Delegation;
                }
                _client.Open( );

                var requestInterceptor = new InspectorBehavior( );
                _client.Endpoint.Behaviors.Add( requestInterceptor );

                var clientResponse = _client.TRINUNCIA_BONUS( CommonManager.GetCurrentUserMatricola7chars( ) );
                if ( clientResponse != null && clientResponse.Rows != null && clientResponse.Rows.Count > 0 )
                {
                    var r = clientResponse.Rows[0];

                    string matricola = r.ItemArray[0].ToString( );
                    string annoRiferimento = r.ItemArray[1].ToString( );
                    string risposta = r.ItemArray[2].ToString( );
                    string dataAggiornamento = r.ItemArray[3].ToString( );

                    if ( !String.IsNullOrEmpty( dataAggiornamento ) )
                    {
                        dataAggiornamento = dataAggiornamento.Trim( );
                        if ( dataAggiornamento.Equals( "01/01/0001 00:00:00" ) )
                        {
                            dataAggiornamento = "";
                        }

                        if ( !string.IsNullOrEmpty( dataAggiornamento ) )
                        {
                            DateTime tmp;
                            DateTime.TryParseExact( dataAggiornamento , "dd/MM/yyyy HH:mm:ss" , null , System.Globalization.DateTimeStyles.None , out tmp );
                            model.DataCompilazione = tmp;
                        }
                    }

                    if (!String.IsNullOrEmpty(annoRiferimento))
                    {
                        annoRiferimento = annoRiferimento.Trim( );
                        if (annoRiferimento.Equals("") || annoRiferimento.Equals("0001"))
                        {
                            annoRiferimento = "";
                        }

                        if (!string.IsNullOrEmpty(annoRiferimento))
                        {
                            model.Anno = int.Parse(annoRiferimento);
                        }
                    }

                    if ( !String.IsNullOrEmpty( risposta ) )
                    {
                        bool risp = risposta.Trim( ).ToUpper( ) == "1";

                        if ( risp )
                        {
                            model.GiaScelto = true;
                        }
                    }
                }
                else
                {
                    model.GiaScelto = false;
                }

                string requestXML = requestInterceptor.LastRequestXML;
                string responseXML = requestInterceptor.LastResponseXML;

                _client.Close( );

                // se ha già scelto allora verifica la possibilità di mostrare il bottone annulla richiesta
                if (model.GiaScelto)
                {
                    model.BtnAnnullaSceltaEnabled = false;
                    model.BtnAnnullaSceltaTitleMessage = "Conferma azione";
                    model.BtnAnnullaSceltaText = "Annulla scelta";
                    model.BtnAnnullaSceltaConfirmMessage = "<br><p class='rai-font-md'>Sicuro di voler annullare la scelta fatta?</p><p class='rai-font-md'>Annullando il modulo riceverà una mail di conferma. Successivamente sarà possibile poter ricompilare il modulo.</p>";                    

                    if ( parametriDB != null && parametriDB.Any( ) )
                    {
                        var abilitato = parametriDB.Where( w => w.Attributo == "ModuloRinuncia2020BottoneAnnullaSceltaEnabled" ).FirstOrDefault( );

                        model.BtnAnnullaSceltaEnabled = bool.Parse( abilitato.Valore1 );

                        if (model.BtnAnnullaSceltaEnabled)
                        {
                            var btnText = parametriDB.Where( w => w.Attributo == "ModuloRinuncia2020BottoneAnnullaSceltaTesto" ).FirstOrDefault( );

                            if ( !String.IsNullOrEmpty( btnText.Valore1 ) )
                            {
                                model.BtnAnnullaSceltaText = btnText.Valore1;
                            }

                            var confermaBox = parametriDB.Where( w => w.Attributo == "ModuloRinuncia2020BottoneAnnullaSceltaMessaggioConferma" ).FirstOrDefault( );

                            if ( !String.IsNullOrEmpty( confermaBox.Valore1 ) )
                            {
                                model.BtnAnnullaSceltaTitleMessage = btnText.Valore1;
                            }

                            if ( !String.IsNullOrEmpty( confermaBox.Valore2 ) )
                            {
                                model.BtnAnnullaSceltaConfirmMessage = confermaBox.Valore2;
                            }
                        }
                    }
                }
            }
            catch ( Exception ex )
            {
                model = null;
            }
            return model;
        }

        public ActionResult VisualizzaModuloRinuncia2020 ( string codiceModulo )
        {

            ModuloVM model = GetModuloRinunciaModel( );

            if (model == null)
            {
                throw new Exception( "Errore nel caricamento dei dati" );
            }

            return View( "~/Views/Moduli/ModuloRinuncia2020.cshtml" , model );
        }

        public ActionResult SalvaSceltaModuloRinuncia ( string annoRif )
        {
            string result = "OK";
            DateTime dataCompilazione = DateTime.Now;
            try
            {
                // se tutto ok allora genera il pdf e lo invia tramite mail
                string matricola = CommonManager.GetCurrentUserMatricola( );

                string esito = SalvaSceltaModuloRinuncia_Internal( annoRif );

                // se è vuoto allora update completato
                // altrimenti sarà valorizzato con la descrizione dell'errore
                if ( !String.IsNullOrEmpty( esito ) )
                {
                    throw new Exception( esito );
                }

                try
                {
                    Logger.LogAzione( new MyRai_LogAzioni( )
                    {
                        applicativo = "Portale" ,
                        data = DateTime.Now ,
                        matricola = CommonManager.GetCurrentUserMatricola( ) ,
                        descrizione_operazione = "SalvaSceltaModuloRinuncia " + annoRif.ToString() ,
                        operazione = "Salvataggio Modulo Rinuncia" ,
                        provenienza = new StackFrame( 1 , true ).GetMethod( ).Name
                    } , CommonManager.GetCurrentUserMatricola( ) );
                }
                catch ( Exception ex )
                {

                }

                // se tutto ok
                // invia mail 
                InvioMailModuloRinuncia( matricola );
            }
            catch ( Exception ex )
            {
                result = ex.Message;
            }

            return Content( result );
        }

        private string SalvaSceltaModuloRinuncia_Internal ( string annoRif )
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
                    _client.ClientCredentials.Windows.ClientCredential = CommonHelper.GetUtenteServizioCredentials();
                    _client.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Delegation;
                }
                _client.Open( );

                var requestInterceptor = new InspectorBehavior( );
                _client.Endpoint.Behaviors.Add( requestInterceptor );

                // aggiornamento dei dati
                var clientResponse = _client.TSAVE_RINUNCIA_BONUS( CommonManager.GetCurrentUserMatricola7chars( ) , annoRif , "1" );

                // verifica dei dati aggiornati
                if ( !String.IsNullOrEmpty( clientResponse ) )
                {
                    if ( clientResponse != "OK" )
                    {
                        esito = clientResponse;
                    }
                }
                else
                {
                    esito = "Non è stato possibile salvare i dati";
                }

                string requestXML = requestInterceptor.LastRequestXML;
                string responseXML = requestInterceptor.LastResponseXML;

                _client.Close( );
            }
            catch ( Exception ex )
            {
                esito = ex.Message;
                CommonTasks.LogErrore( ex.Message , "SalvaSceltaModuloRinuncia - WEB - Azione Salva" );
            }

            return esito;
        }

        public ActionResult RicaricaModuloRinuncia2020 ( )
        {
            return RedirectToAction( "GetWidgetRinuncia2020" , "Scrivania" );
        }

        private void InvioMailModuloRinuncia ( string matricola )
        {
            try
            {
                string dest = CommonTasks.GetEmailPerMatricola( matricola );

                if ( String.IsNullOrWhiteSpace( dest ) )
                    dest = "P" + matricola + "@rai.it";

                GestoreMail mail = new GestoreMail( );
                string corpo = "La richiesta di Rinuncia al Trattamento integrativo e Ulteriore detrazione è stata registrata con successo.";

                List<ParametriSistemaValoreJson> parametriDB = parametriDB = CommonManager.GetParametriJson( EnumParametriSistema.ModuloRinuncia2020Params );

                if ( parametriDB != null && parametriDB.Any( ) )
                {
                    var txRinunciaRow = parametriDB.Where( w => w.Attributo == "ModuloRinuncia2020TestoMailAnnullaRinuncia" ).FirstOrDefault( );

                    if ( txRinunciaRow != null && !String.IsNullOrEmpty( txRinunciaRow.Valore1 ) )
                    {
                        corpo = txRinunciaRow.Valore1;
                    }
                }
                
                var response = mail.InvioMail( corpo , " SelfService del Dipendente - Notifiche Modulo di Rinuncia al Trattamento integrativo e Ulteriore detrazione" , dest , "raiplace.selfservice@rai.it" , "[CG] RaiPlace - Self Service <raiplace.selfservice@rai.it>" , null , null );

                if ( response != null && response.Errore != null )
                {
                    MyRai_LogErrori err = new MyRai_LogErrori( )
                    {
                        applicativo = "Portale" ,
                        data = DateTime.Now ,
                        provenienza = "ModuliController - InvioMailModuloRinuncia" ,
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
                    provenienza = "ModuliController - WEB - InvioMailModuloRinuncia" ,
                    error_message = ex.Message
                };

                using ( digiGappEntities db = new digiGappEntities( ) )
                {
                    db.MyRai_LogErrori.Add( err );
                    db.SaveChanges( );
                }
            }
        }

        public ActionResult AnnullaSceltaModuloRinuncia( string annoRif )
        {
            string result = "OK";
            DateTime dataCompilazione = DateTime.Now;
            try
            {
                // se tutto ok allora genera il pdf e lo invia tramite mail
                string matricola = CommonManager.GetCurrentUserMatricola( );

                string esito = AnnullaSceltaModuloRinuncia_Internal( annoRif );

                // se è vuoto allora update completato
                // altrimenti sarà valorizzato con la descrizione dell'errore
                if ( !String.IsNullOrEmpty( esito ) )
                {
                    throw new Exception( esito );
                }

                try
                {
                    Logger.LogAzione( new MyRai_LogAzioni( )
                    {
                        applicativo = "Portale" ,
                        data = DateTime.Now ,
                        matricola = CommonManager.GetCurrentUserMatricola( ) ,
                        descrizione_operazione = "AnnullaSceltaModuloRinuncia_Internal " + annoRif.ToString() ,
                        operazione = "Salvataggio Modulo Rinuncia" ,
                        provenienza = new StackFrame( 1 , true ).GetMethod( ).Name
                    } , CommonManager.GetCurrentUserMatricola( ) );
                }
                catch ( Exception ex )
                {

                }

                // se tutto ok
                // invia mail 
                InvioMailModuloRinunciaAnnullata( matricola, dataCompilazione );
            }
            catch ( Exception ex )
            {
                result = ex.Message;
            }

            return Content( result );
        }

        private string AnnullaSceltaModuloRinuncia_Internal ( string annoRif )
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
                    _client.ClientCredentials.Windows.ClientCredential = CommonHelper.GetUtenteServizioCredentials();
                    _client.ClientCredentials.Windows.AllowedImpersonationLevel = System.Security.Principal.TokenImpersonationLevel.Delegation;
                }
                _client.Open( );

                var requestInterceptor = new InspectorBehavior( );
                _client.Endpoint.Behaviors.Add( requestInterceptor );

                // aggiornamento dei dati
                var clientResponse = _client.TRESET_RINUNCIA_BONUS( CommonManager.GetCurrentUserMatricola7chars( ) , annoRif );

                // verifica dei dati aggiornati
                if ( !String.IsNullOrEmpty( clientResponse ) )
                {
                    if ( clientResponse != "OK" )
                    {
                        esito = clientResponse;
                    }
                }
                else
                {
                    esito = "Non è stato possibile salvare i dati";
                }

                string requestXML = requestInterceptor.LastRequestXML;
                string responseXML = requestInterceptor.LastResponseXML;

                _client.Close( );
            }
            catch ( Exception ex )
            {
                esito = ex.Message;
                CommonTasks.LogErrore( ex.Message , "AnnullaSceltaModuloRinuncia_Internal - WEB - Azione Annulla" );
            }

            return esito;
        }

        private void InvioMailModuloRinunciaAnnullata ( string matricola, DateTime data)
        {
            try
            {
                string dest = CommonTasks.GetEmailPerMatricola( matricola );

                if ( String.IsNullOrWhiteSpace( dest ) )
                    dest = "P" + matricola + "@rai.it";

                GestoreMail mail = new GestoreMail( );

                string corpo = "A seguito della Sua richiesta del " + data.ToString( "dd/MM/yyyy HH:mm:ss" ) + " il modulo Rinuncia al Trattamento integrativo e Ulteriore detrazione è stato annullato con successo.";

                var response = mail.InvioMail( corpo , " SelfService del Dipendente - Notifica annullamento Modulo di Rinuncia al Trattamento integrativo e Ulteriore detrazione" , dest , "raiplace.selfservice@rai.it" , "[CG] RaiPlace - Self Service <raiplace.selfservice@rai.it>" , null , null );

                if ( response != null && response.Errore != null )
                {
                    MyRai_LogErrori err = new MyRai_LogErrori( )
                    {
                        applicativo = "Portale" ,
                        data = DateTime.Now ,
                        provenienza = "ModuliController - InvioMailModuloRinuncia" ,
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
                    provenienza = "ModuliController - WEB - InvioMailModuloRinuncia" ,
                    error_message = ex.Message
                };

                using ( digiGappEntities db = new digiGappEntities( ) )
                {
                    db.MyRai_LogErrori.Add( err );
                    db.SaveChanges( );
                }
            }
        }

        #endregion

        #region INCENTIVAZIONE012021

        /// <summary>
        /// Metodo che visualizza la pagina di edit del widget
        /// </summary>
        /// <returns></returns>
        public ActionResult CompilaIncentivazione012021 ( )
        {
            ModuloVM model = new ModuloVM( );
            model.WidgetId = "WdgIncentivazione012021";
            model.Matricola = CommonManager.GetCurrentUserMatricola( );
            model.Nominativo = String.Format( "{0} {1}" , Utente.EsponiAnagrafica( )._nome , Utente.EsponiAnagrafica( )._cognome );
            model.Sesso = Utente.EsponiAnagrafica( )._genere;
            model.DataNascita = Utente.EsponiAnagrafica( )._dataNascita;
            model.LuogoNascita = Utente.EsponiAnagrafica( )._comuneNascita;
            model.CodiceFiscale = Utente.EsponiAnagrafica( )._cf;
            model.IncentivazioneWidgetData = new Incentivazione012021VM( );
            model.IncentivazioneWidgetData.Nome = Utente.EsponiAnagrafica( )._nome;
            model.IncentivazioneWidgetData.Cognome = Utente.EsponiAnagrafica( )._cognome;
            model.IncentivazioneWidgetData.IsGiornalista = Utente.TipoDipendente( ).Equals( "G" );
            model.IncentivazioneWidgetData.Azienda = Utente.EsponiAnagrafica( )._logo;
            model.IncentivazioneWidgetData.Date = GetDate( );

            using ( digiGappEntities dbDG = new digiGappEntities( ) )
            {
                var parametro = dbDG.MyRai_ParametriSistema.FirstOrDefault( x => x.Chiave == "Incentivazione012021Protocollo" );

                if ( parametro != null )
                {
                    model.IncentivazioneWidgetData.EtichettaProtocollo = String.IsNullOrEmpty( parametro.Valore1 ) ? "" : parametro.Valore1;
                }
            }
            // deve verificare se l'utente farà 61 anni entro la data massima che può inviare         
            var today = DateTime.Today;

            // prima data disponibile
            DateTime limiteMinimo = CalcolaPrimaDataUtile( );
            
            // ultima data disponibile
            DateTime limiteMassimo = CalcolaUltimaDataUtile( limiteMinimo );

            var age = limiteMassimo.Year - Utente.EsponiAnagrafica( )._dataNascita.GetValueOrDefault( ).Year;

            // se è nato in un mese e giorno successivo ad oggi sottrae un anno perchè ancora non ha compiuto gli anni
            if ( Utente.EsponiAnagrafica( )._dataNascita.GetValueOrDefault( ).Date > limiteMassimo.AddYears( -age ) )
                age--;

            if ( age == 60 )
            {
                // bisogna verificare se compirà gli anni entro i 3 mesi
                DateTime toCompare = new DateTime( DateTime.Now.Year , Utente.EsponiAnagrafica( )._dataNascita.GetValueOrDefault( ).Month , Utente.EsponiAnagrafica( )._dataNascita.GetValueOrDefault( ).Day );

                if ( Utente.EsponiAnagrafica( )._dataNascita.GetValueOrDefault( ).Month <= limiteMassimo.Month &&
                    ( toCompare.Date > today.Date && ( DateTime.Now.Year - Utente.EsponiAnagrafica( )._dataNascita.GetValueOrDefault( ).Year > 60 ) ) )
                {
                    age++;
                }
            }

            // se non ha almeno 61 anni allora il widget non va mostrato
            if ( age < 61 )
            {
                // qualcosa è andato storno perchè l'utente non dovrebbe essere arrivato qui
                model.IncentivazioneWidgetData.Compie61Anni = false;
            }
            else 
            {
                model.IncentivazioneWidgetData.Compie61Anni = true;
            }

            // se non è giornalista deve verificare se il primo check deve essere abilitato
            if ( !model.IncentivazioneWidgetData.IsGiornalista )
            {
                // verifica se l'utente nel corso dell'anno compirà 62 anni            
                today = new DateTime( DateTime.Now.Year , 12 , 31 );
                age = today.Year - Utente.EsponiAnagrafica( )._dataNascita.GetValueOrDefault( ).Year;

                // Nel caso in cui sia nato in un anno bisestile
                if ( Utente.EsponiAnagrafica( )._dataNascita.GetValueOrDefault( ).Date > today.AddYears( -age ) )
                    age--;

                // se nell'arco dell'anno farà 62 anni, allora può essere abilitata il primo checkbox quota100
                if ( age >= 62 )
                {
                    model.IncentivazioneWidgetData.Compie62Anni = true;
                }
            }

            var forzato = VerificaSeMatricolaForzata( );

            switch ( forzato )
            {
                case ModuloIncentivazione012021SimulazioneEnum.All:
                    model.IncentivazioneWidgetData.IsGiornalista = false;
                    model.IncentivazioneWidgetData.Compie62Anni = true;
                    model.IncentivazioneWidgetData.Compie61Anni = true;
                    break;

                case ModuloIncentivazione012021SimulazioneEnum.Dip61:
                    model.IncentivazioneWidgetData.IsGiornalista = false;
                    model.IncentivazioneWidgetData.Compie62Anni = false;
                    model.IncentivazioneWidgetData.Compie61Anni = true;
                    break;

                case ModuloIncentivazione012021SimulazioneEnum.Dip62:
                    model.IncentivazioneWidgetData.IsGiornalista = false;
                    model.IncentivazioneWidgetData.Compie62Anni = true;
                    model.IncentivazioneWidgetData.Compie61Anni = true;
                    break;

                case ModuloIncentivazione012021SimulazioneEnum.Gior61:
                    model.IncentivazioneWidgetData.IsGiornalista = true;
                    model.IncentivazioneWidgetData.Compie62Anni = false;
                    model.IncentivazioneWidgetData.Compie61Anni = true;
                    break;

                case ModuloIncentivazione012021SimulazioneEnum.Gior62:
                    model.IncentivazioneWidgetData.IsGiornalista = true;
                    model.IncentivazioneWidgetData.Compie62Anni = true;
                    model.IncentivazioneWidgetData.Compie61Anni = true;
                    break;
            }

            return View( "~/Views/Moduli/Incentivazione012021/ModuloIncentivazione012021.cshtml" , model );
        }

        public ActionResult SalvaIncentivazione012021 ( string scelta , string dateSelezionate )
        {
            string result = "OK";
            DateTime dataCompilazione = DateTime.Now;
            try
            {
                // se tutto ok allora genera il pdf e lo invia tramite mail
                string matricola = CommonManager.GetCurrentUserMatricola( );

                // creazione del pdf
                byte[] myArrayOfBytes = CreazionePDFIncentivazione012021( CommonManager.GetCurrentUserMatricola( ) ,
                                                    scelta ,
                                                    dateSelezionate ,
                                                    dataCompilazione , "" );

                if ( myArrayOfBytes == null )
                {
                    result = "Errore nella creazione del PDF";
                    throw new Exception( result );
                }

                // salva scelta
                string esito = SalvaSceltaIncentivi012021( scelta.ToString( ) , dateSelezionate , dataCompilazione , myArrayOfBytes );

                // se è vuoto allora update completato
                // altrimenti sarà valorizzato con la descrizione dell'errore
                if ( !String.IsNullOrEmpty( esito ) )
                {
                    throw new Exception( esito );
                }

                try
                {
                    Logger.LogAzione( new MyRai_LogAzioni( )
                    {
                        applicativo = "Portale" ,
                        data = DateTime.Now ,
                        matricola = CommonManager.GetCurrentUserMatricola( ) ,
                        descrizione_operazione = "Salvataggio scelta smart2020 - scelta - " + scelta.ToString( ) ,
                        operazione = "Salvataggio SmartWorking" ,
                        provenienza = new StackFrame( 1 , true ).GetMethod( ).Name
                    } , CommonManager.GetCurrentUserMatricola( ) );
                }
                catch ( Exception ex )
                {

                }

                // se tutto ok
                // invia mail con la copia del modulo compilato
                InvioMailIncentivazione012021( matricola , myArrayOfBytes );

            }
            catch ( Exception ex )
            {
            }

            return Content( result );
        }

        public ActionResult RicaricaWidgetIncentivazione012021()
        {
            SectionDayModel model = new SectionDayModel( );
            List<WidgetModuloBox_Azione> btns = new List<WidgetModuloBox_Azione>( );

            bool giaScelto = false;
            string scelta = "";
            DateTime? dataCompilazione = null;
            DateTime dtScelta;

            // verifica se l'utente ha già effettuato la scelta
            string mt = CommonManager.GetCurrentUserMatricola( );
            using ( TalentiaEntities dbTalentia = new TalentiaEntities( ) )
            {
                bool exist = dbTalentia.XR_MOD_DIPENDENTI.Count( w => w.MATRICOLA.Equals( mt ) && w.COD_MODULO.Equals( "INCENTIVAZIONE012021" ) ) > 0;

                if ( !exist )
                {
                    giaScelto = false;
                }
                else
                {
                    var items = dbTalentia.XR_MOD_DIPENDENTI.Where( w => w.MATRICOLA.Equals( mt ) && w.COD_MODULO.Equals( "INCENTIVAZIONE012021" ) ).ToList( );

                    if ( items != null && items.Any( ) )
                    {
                        DateTime? dtComp = items.First( ).DATA_COMPILAZIONE;
                        dataCompilazione = dtComp;
                        giaScelto = true;

                        foreach ( var item in items )
                        {
                            string sel = "";
                            var separati = item.SCELTA.Split( '#' ).ToList( );
                            sel = separati[0];
                            string mydate = separati[1];
                            
                            DateTime.TryParseExact( mydate , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out dtScelta );

                            List<string> scelte = new List<string>( );
                            scelte = sel.Split( ';' ).ToList( );

                            foreach ( var s in scelte )
                            {
                                ModuloIncentivazione012021Enum myEnum = ( ModuloIncentivazione012021Enum ) Enum.Parse( typeof( ModuloIncentivazione012021Enum ) , s );
                                int val = ( int ) myEnum;

                                if ( !String.IsNullOrEmpty( scelta ) )
                                {
                                    scelta += ";";
                                }
                                scelta += sel;
                            }
                        }
                    }
                }
            }

            if ( giaScelto )
            {
                btns.Add( new WidgetModuloBox_Azione( )
                {
                    TestoBottone = "Visualizza" ,
                    UrlBottone = Url.Action( "VisualizzaModuloIncentivazione012021" , "Moduli" , new { codiceModulo = "INCENTIVAZIONE012021" } )
                } );

                model.Incentivazione012021Widget = new WidgetModuloBox( )
                {
                    WidgetId = "WdgIncentivazione012021" ,
                    Anno = DateTime.Now.Year ,
                    GiaScelto = true ,
                    HaDiritto = true ,
                    Titolo = "Modulo incentivazione" ,
                    Scelta = scelta ,
                    DataCompilazione = dataCompilazione ,
                    Sottotitolo = "Modulo compilato in data " + dataCompilazione.GetValueOrDefault( ).ToString( "dd/MM/yyyy" ) ,
                    Bottoni = btns
                };
            }
            else
            {
                btns.Add( new WidgetModuloBox_Azione( )
                {
                    TestoBottone = "Compila" ,
                    UrlBottone = Url.Action( "CompilaIncentivazione012021" , "Moduli" , new { codiceModulo = "INCENTIVAZIONE012021" } )
                } );

                model.Incentivazione012021Widget = new WidgetModuloBox( )
                {
                    WidgetId = "WdgIncentivazione012021" ,
                    Anno = DateTime.Now.Year ,
                    GiaScelto = giaScelto ,
                    HaDiritto = true ,
                    Titolo = "Modulo incentivazione" ,
                    Scelta = scelta ,
                    DataCompilazione = null ,
                    Bottoni = btns
                };
            }
            return View( "~/Views/Scrivania/subpartial/boxModulo.cshtml" , model.Incentivazione012021Widget );
        }

        public ActionResult VisualizzaModuloIncentivazione012021 ( string codiceModulo )
        {
            ModuloVM model = new ModuloVM( );
            model.WidgetId = "WdgIncentivazione012021";
            model.Matricola = CommonManager.GetCurrentUserMatricola( );
            model.Nominativo = String.Format( "{0} {1}" , Utente.EsponiAnagrafica( )._nome , Utente.EsponiAnagrafica( )._cognome );
            model.Sesso = Utente.EsponiAnagrafica( )._genere;
            model.DataNascita = Utente.EsponiAnagrafica( )._dataNascita;
            model.LuogoNascita = Utente.EsponiAnagrafica( )._comuneNascita;
            model.CodiceFiscale = Utente.EsponiAnagrafica( )._cf;
            model.IncentivazioneWidgetData = new Incentivazione012021VM( );
            model.IncentivazioneWidgetData.Nome = Utente.EsponiAnagrafica( )._nome;
            model.IncentivazioneWidgetData.Cognome = Utente.EsponiAnagrafica( )._cognome;
            model.IncentivazioneWidgetData.IsGiornalista = Utente.TipoDipendente( ).Equals( "G" );
            model.IncentivazioneWidgetData.Azienda = Utente.EsponiAnagrafica( )._logo;

            using ( digiGappEntities dbDG = new digiGappEntities( ) )
            {
                var parametro = dbDG.MyRai_ParametriSistema.FirstOrDefault( x => x.Chiave == "Incentivazione012021Protocollo" );

                if ( parametro != null )
                {
                    model.IncentivazioneWidgetData.EtichettaProtocollo = String.IsNullOrEmpty( parametro.Valore1 ) ? "" : parametro.Valore1;
                }
            }
            // deve verificare se l'utente farà 61 anni entro la data massima che può inviare         
            var today = DateTime.Today;

            // prima data disponibile
            DateTime limiteMinimo = CalcolaPrimaDataUtile( );

            // ultima data disponibile
            DateTime limiteMassimo = CalcolaUltimaDataUtile( limiteMinimo );

            var age = limiteMassimo.Year - Utente.EsponiAnagrafica( )._dataNascita.GetValueOrDefault( ).Year;

            // se è nato in un mese e giorno successivo ad oggi sottrae un anno perchè ancora non ha compiuto gli anni
            if ( Utente.EsponiAnagrafica( )._dataNascita.GetValueOrDefault( ).Date > limiteMassimo.AddYears( -age ) )
                age--;

            if ( age == 60 )
            {
                // bisogna verificare se compirà gli anni entro i 3 mesi
                DateTime toCompare = new DateTime( DateTime.Now.Year , Utente.EsponiAnagrafica( )._dataNascita.GetValueOrDefault( ).Month , Utente.EsponiAnagrafica( )._dataNascita.GetValueOrDefault( ).Day );

                if ( Utente.EsponiAnagrafica( )._dataNascita.GetValueOrDefault( ).Month <= limiteMassimo.Month &&
                    ( toCompare.Date > today.Date && ( DateTime.Now.Year - Utente.EsponiAnagrafica( )._dataNascita.GetValueOrDefault( ).Year > 60 ) ) )
                {
                    age++;
                }
            }

            // se non ha almeno 61 anni allora il widget non va mostrato
            if ( age < 61 )
            {
                // qualcosa è andato storno perchè l'utente non dovrebbe essere arrivato qui
                model.IncentivazioneWidgetData.Compie61Anni = false;
            }
            else
            {
                model.IncentivazioneWidgetData.Compie61Anni = true;
            }

            // se non è giornalista deve verificare se il primo check deve essere abilitato
            if ( !model.IncentivazioneWidgetData.IsGiornalista )
            {
                // verifica se l'utente nel corso dell'anno compirà 62 anni            
                today = new DateTime( DateTime.Now.Year , 12 , 31 );
                age = today.Year - Utente.EsponiAnagrafica( )._dataNascita.GetValueOrDefault( ).Year;

                // Nel caso in cui sia nato in un anno bisestile
                if ( Utente.EsponiAnagrafica( )._dataNascita.GetValueOrDefault( ).Date > today.AddYears( -age ) )
                    age--;

                // se nell'arco dell'anno farà 62 anni, allora può essere abilitata il primo checkbox quota100
                if ( age >= 62 )
                {
                    model.IncentivazioneWidgetData.Compie62Anni = true;
                }
            }

            var forzato = VerificaSeMatricolaForzata( );

            switch ( forzato )
            {
                case ModuloIncentivazione012021SimulazioneEnum.All:
                    model.IncentivazioneWidgetData.IsGiornalista = false;
                    model.IncentivazioneWidgetData.Compie62Anni = true;
                    model.IncentivazioneWidgetData.Compie61Anni = true;
                    break;

                case ModuloIncentivazione012021SimulazioneEnum.Dip61:
                    model.IncentivazioneWidgetData.IsGiornalista = false;
                    model.IncentivazioneWidgetData.Compie62Anni = false;
                    model.IncentivazioneWidgetData.Compie61Anni = true;
                    break;

                case ModuloIncentivazione012021SimulazioneEnum.Dip62:
                    model.IncentivazioneWidgetData.IsGiornalista = false;
                    model.IncentivazioneWidgetData.Compie62Anni = true;
                    model.IncentivazioneWidgetData.Compie61Anni = true;
                    break;

                case ModuloIncentivazione012021SimulazioneEnum.Gior61:
                    model.IncentivazioneWidgetData.IsGiornalista = true;
                    model.IncentivazioneWidgetData.Compie62Anni = false;
                    model.IncentivazioneWidgetData.Compie61Anni = true;
                    break;

                case ModuloIncentivazione012021SimulazioneEnum.Gior62:
                    model.IncentivazioneWidgetData.IsGiornalista = true;
                    model.IncentivazioneWidgetData.Compie62Anni = true;
                    model.IncentivazioneWidgetData.Compie61Anni = true;
                    break;
            }

            bool giaScelto = false;
            string scelta = "";
            DateTime? dataCompilazione = null;
            DateTime dtScelta = DateTime.Now;

            // verifica se l'utente ha già effettuato la scelta
            string mt = CommonManager.GetCurrentUserMatricola( );
            using ( TalentiaEntities dbTalentia = new TalentiaEntities( ) )
            {
                bool exist = dbTalentia.XR_MOD_DIPENDENTI.Count( w => w.MATRICOLA.Equals( mt ) && w.COD_MODULO.Equals( "INCENTIVAZIONE012021" ) ) > 0;

                if ( !exist )
                {
                    giaScelto = false;
                }
                else
                {
                    var items = dbTalentia.XR_MOD_DIPENDENTI.Where( w => w.MATRICOLA.Equals( mt ) && w.COD_MODULO.Equals( "INCENTIVAZIONE012021" ) ).ToList( );

                    if ( items != null && items.Any( ) )
                    {
                        DateTime? dtComp = items.First( ).DATA_COMPILAZIONE;
                        dataCompilazione = dtComp;
                        model.DataCompilazione = dataCompilazione;
                        giaScelto = true;

                        foreach ( var item in items )
                        {
                            string sel = "";
                            var separati = item.SCELTA.Split( '#' ).ToList( );
                            sel = separati[0];
                            string mydate = separati[1];

                            DateTime.TryParseExact( mydate , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out dtScelta );

                            SelectListItem itemList = new SelectListItem( )
                            {
                                Value = dtScelta.ToString("dd/MM/yyyy"),
                                Text = dtScelta.ToString( "dd/MM/yyyy" )
                            };

                            model.IncentivazioneWidgetData.Scelta_Inc_Data = dtScelta;

                            model.IncentivazioneWidgetData.Date = new List<SelectListItem>( );
                            model.IncentivazioneWidgetData.Date.Add( itemList );

                            List<string> scelte = new List<string>( );
                            scelte = sel.Split( ';' ).ToList( );

                            foreach ( var s in scelte )
                            {
                                ModuloIncentivazione012021Enum myEnum = ( ModuloIncentivazione012021Enum ) Enum.Parse( typeof( ModuloIncentivazione012021Enum ) , s );
                                int val = ( int ) myEnum;

                                switch ( val )
                                {
                                    case 1:
                                        model.IncentivazioneWidgetData.Scelta_Inc_1 = true;
                                        break;
                                    case 2:
                                        model.IncentivazioneWidgetData.Scelta_Inc_2 = true;
                                        break;
                                    case 21:
                                        model.IncentivazioneWidgetData.Scelta_Inc_2_1 = true;
                                        break;
                                    case 22:
                                        model.IncentivazioneWidgetData.Scelta_Inc_2_2 = true;
                                        break;
                                    case 23:
                                        model.IncentivazioneWidgetData.Scelta_Inc_2_3 = true;
                                        break;

                                }

                                if ( !String.IsNullOrEmpty( scelta ) )
                                {
                                    scelta += ";";
                                }
                                scelta += sel;
                            }
                        }
                    }
                }
            }

            if (!giaScelto)
            {
                // ritorna il metodo compila
            }

            return View( "~/Views/Moduli/Incentivazione012021/ModuloIncentivazione012021ReadOnly.cshtml" , model );
        }

        private string SalvaSceltaIncentivi012021 ( string scelta , string dataSelezionata , DateTime dataCompilazione , byte[] modulo )
        {
            string esito = "";
            try
            {
                string mt = CommonManager.GetCurrentUserMatricola( );
                using ( TalentiaEntities dbTalentia = new TalentiaEntities( ) )
                {
                    bool exist = dbTalentia.XR_MOD_DIPENDENTI.Count( w => w.MATRICOLA.Equals( mt ) && w.COD_MODULO.Equals( "INCENTIVAZIONE012021" ) ) > 0;

                    if ( !exist )
                    {
                        //Reperimento dei dati del dipendente
                        XR_MOD_DIPENDENTI toAdd = null;

                        var info = ( from s in dbTalentia.SINTESI1
                                     join q in dbTalentia.QUALIFICA
                                     on s.COD_QUALIFICA equals q.COD_QUALIFICA
                                     where s.COD_MATLIBROMAT == mt
                                     select new
                                     {
                                         ID_PERSONA = s.ID_PERSONA ,
                                         COD_RUOLO = s.COD_RUOLO ,
                                         COD_QUALSTD = q.COD_QUALSTD
                                     } ).FirstOrDefault( );

                        if ( info != null )
                        {
                            List<string> scelte = new List<string>( );
                            scelte = scelta.Split( ';' ).ToList( );

                            string scelteEstese = "";
                            foreach ( var s in scelte )
                            {
                                if ( !String.IsNullOrEmpty( s ) )
                                {
                                    int myInt = int.Parse( s );
                                    ModuloIncentivazione012021Enum myEnum = ( ModuloIncentivazione012021Enum ) myInt;
                                    string sel = myEnum.ToString( );
                                    if ( String.IsNullOrEmpty( scelteEstese ) )
                                    {
                                        scelteEstese = sel;
                                    }
                                    else
                                    {
                                        scelteEstese += ";" + sel;
                                    }
                                }
                            }

                            scelteEstese += "#" + dataSelezionata;

                            toAdd = new XR_MOD_DIPENDENTI( )
                            {
                                COD_MANSIONE = info.COD_RUOLO ,
                                COD_MODULO = "INCENTIVAZIONE012021" ,
                                COD_PROFILO_PROF = info.COD_QUALSTD ,
                                COD_TERMID = System.Web.HttpContext.Current.Request.UserHostAddress ,
                                COD_USER = CommonManager.GetCurrentUserMatricola( ) ,
                                DATA_COMPILAZIONE = dataCompilazione ,
                                DATA_INIZIO = null ,
                                DATA_SCADENZA = null ,
                                ID_PERSONA = info.ID_PERSONA ,
                                IND_STATO = "0" ,
                                MATRICOLA = mt ,
                                PDF_MODULO = modulo ,
                                SCELTA = scelteEstese ,
                                TMS_TIMESTAMP = DateTime.Now ,
                                XR_MOD_DIPENDENTI1 = dbTalentia.XR_MOD_DIPENDENTI.GeneraPrimaryKey( )
                            };

                            dbTalentia.XR_MOD_DIPENDENTI.Add( toAdd );
                            dbTalentia.SaveChanges( );
                        }
                        else
                        {
                            throw new Exception( "Impossibile reperire i dati utente" );
                        }
                    }
                    else
                    {
                        throw new Exception( "L'utente ha già effettuato la scelta" );
                    }
                }
            }
            catch ( Exception ex )
            {
                esito = ex.Message;
                CommonTasks.LogErrore( ex.Message , "SalvaSceltaIncentivi012021 - WEB - Azione Salva" );
            }

            return esito;
        }

        private List<SelectListItem> GetDate ( )
        {
            List<SelectListItem> result = new List<SelectListItem>( );

            result.Add( new SelectListItem( )
            {
                Value = "",
                Text = "Seleziona una data"
            } );

            // deve verificare se l'utente farà 61 anni entro la data massima che può inviare         
            var today = DateTime.Today;

            // prima data disponibile
            DateTime limiteMinimo = CalcolaPrimaDataUtile( );

            // ultima data disponibile
            DateTime limiteMassimo = CalcolaUltimaDataUtile( limiteMinimo );

            DateTime dataNascita = Utente.EsponiAnagrafica( )._dataNascita.GetValueOrDefault( );

            var age = limiteMassimo.Year - Utente.EsponiAnagrafica( )._dataNascita.GetValueOrDefault( ).Year;

            // se è nato in un mese e giorno successivo ad oggi sottrae un anno perchè ancora non ha compiuto gli anni
            if ( Utente.EsponiAnagrafica( )._dataNascita.GetValueOrDefault( ).Date > limiteMassimo.AddYears( -age ) )
                age--;

            if ( age == 60 )
            {
                // bisogna verificare se compirà gli anni entro i 3 mesi
                DateTime toCompare = new DateTime( DateTime.Now.Year , Utente.EsponiAnagrafica( )._dataNascita.GetValueOrDefault( ).Month , Utente.EsponiAnagrafica( )._dataNascita.GetValueOrDefault( ).Day );

                if ( Utente.EsponiAnagrafica( )._dataNascita.GetValueOrDefault( ).Month <= limiteMassimo.Month &&
                  ( toCompare.Date > today.Date && ( DateTime.Now.Year - Utente.EsponiAnagrafica( )._dataNascita.GetValueOrDefault( ).Year > 60 ) ) )
                {
                    age++;
                }
            }

            var forzato = VerificaSeMatricolaForzata( );

            switch ( forzato )
            {
                case ModuloIncentivazione012021SimulazioneEnum.All:
                    age = 62;
                    break;

                case ModuloIncentivazione012021SimulazioneEnum.Dip61:
                    age = 61;
                    break;

                case ModuloIncentivazione012021SimulazioneEnum.Dip62:
                    age = 62;
                    break;

                case ModuloIncentivazione012021SimulazioneEnum.Gior61:
                    age = 61;
                    break;

                case ModuloIncentivazione012021SimulazioneEnum.Gior62:
                    age = 62;
                    break;
            }

            DateTime dtStart = limiteMinimo;

            while ( dtStart <= limiteMassimo )
            {
                // se age == 61 allora significa che l'utente compirà 61 anni nell'anno oppure li ha già compiuti
                // in tutti i casi, bisogna controllare che le date che andranno a formare la combobox, rispettino il 
                // requisito:
                // il modulo può essere trasmesso se il dipendente compie 61 entro la data indicata di uscita 
                // ( es. facendo 61 anni a giugno, il dipendente può inviare la richiesta da marzo;
                if ( age == 61 )
                {
                    // se l'utente quest'anno farà 62 anni
                    if (DateTime.Now.Year - dataNascita.Year >= 62 )
                    {
                        result.Add( new SelectListItem( )
                        {
                            Value = dtStart.ToString( "dd/MM/yyyy" ) ,
                            Text = dtStart.ToString( "dd/MM/yyyy" )
                        } );
                    }
                    else if ( dataNascita.Month <= dtStart.Month )
                    {
                        // se l'utente ha già compiuto i 61 anni allora nella combo può essere inserita la
                        // data calcolata

                        result.Add( new SelectListItem( )
                        {
                            Value = dtStart.ToString( "dd/MM/yyyy" ) ,
                            Text = dtStart.ToString( "dd/MM/yyyy" )
                        } );
                    }
                }
                else if (age > 61)
                {
                    result.Add( new SelectListItem( )
                    {
                        Value = dtStart.ToString( "dd/MM/yyyy" ) ,
                        Text = dtStart.ToString( "dd/MM/yyyy" )
                    } );
                }

                DateTime tmp = new DateTime( dtStart.Year , dtStart.Month , 1 );
                tmp = tmp.AddMonths( 2 );
                tmp = tmp.AddDays( -1 );
                dtStart = tmp;
            }

            return result;
        }

        private DateTime CalcolaPrimaDataUtile()
        {
            // calcola il primo giorno del mese corrente. Es il giorno 28/01/2021 calcola 01/01/2021
            DateTime primoDelMese = new DateTime( DateTime.Now.Year , DateTime.Now.Month , 1 );

            // aggiunge 2 mesi. Es: da 01/01/2021 calcola 01/03/2021
            DateTime limiteMinimo = primoDelMese.AddMonths( 2 );

            // Calcola l'ultimo giorno del mese, successivo al mese in cui si visualizza il widget
            // Es. da 01/03/2021 passa al 28/02/2021, che è la prima data utile per la domanda
            limiteMinimo = limiteMinimo.AddDays( -1 );

            return limiteMinimo;
        }

        private DateTime CalcolaUltimaDataUtile(DateTime primaDataUtile )
        {
            // aggiunge 3 mesi. Es: da 28/02/2021 calcola 31/05/2021
            DateTime tmp = primaDataUtile.AddMonths( 3 );

            DateTime limiteMassimo = new DateTime( tmp.Year , tmp.Month , 1 );

            // Calcola la data massima inseribile, 
            // La data di cessazione dal servizio non potrà essere superiore alla fine del terzo mese successivo a quello di presentazione della domanda
            // Es. da 01/05/2021 passa al 30/04/2021, che è la data massima inseribile per una domanda inserita a gennaio 2021
            limiteMassimo = limiteMassimo.AddDays( -1 );

            return limiteMassimo;
        }

        private static byte[] CreazionePDFIncentivazione012021 ( string matricola , string scelta , string dataSelezionata , DateTime dataCreazione, string protocollo )
        {
            try
            {
                var cultureInfo = CultureInfo.GetCultureInfo( "it-IT" );

                byte[] bytes = null;
                const int fontIntestazione = 12;
                const int fontCorpo = 11;
                const int fontNote = 9;
                DateTime dt1;

                DateTime.TryParseExact( dataSelezionata , "dd/MM/yyyy" , null , System.Globalization.DateTimeStyles.None , out dt1 );

                BaseFont bf = BaseFont.CreateFont( BaseFont.HELVETICA , BaseFont.CP1250 , BaseFont.NOT_EMBEDDED );
                BaseColor color = new BaseColor( System.Drawing.Color.Black );
                Font arial = FontFactory.GetFont( "Arial" , 28 , Font.NORMAL , color );

                Font myFontIntestazione = FontFactory.GetFont( "Arial" , fontIntestazione , Font.NORMAL , color );
                Font myFontIntestazioneBold = FontFactory.GetFont( "Arial" , fontIntestazione , Font.BOLD , color );
                Font myFontCorpo = FontFactory.GetFont( "Arial" , fontCorpo , Font.NORMAL , color );
                Font myFontCorpoBold = FontFactory.GetFont( "Arial" , fontIntestazione , Font.BOLD , color );
                Font myFontNote = FontFactory.GetFont( "Arial" , fontNote , Font.NORMAL , color );
                Font myFontCorpoSottolineato = FontFactory.GetFont( "Arial" , fontCorpo , Font.UNDERLINE , color );
                Font myFontCorpoItalic = FontFactory.GetFont( "Arial" , fontCorpo , Font.ITALIC , color );

                using ( MemoryStream ms = new MemoryStream( ) )
                {
                    // Creazione dell'istanza del documento, impostando tipo di pagina e margini
                    Document document = new Document( PageSize.A4 , 25 , 25 , 25 , 25 );
                    PdfWriter writer = PdfWriter.GetInstance( document , ms );

                    document.Open( );

                    PdfContentByte cb = writer.DirectContent;

                    string _imgPath = System.Web.HttpContext.Current.Server.MapPath( "~/assets/img/LogoPDF.png" );

                    iTextSharp.text.Image png = iTextSharp.text.Image.GetInstance( _imgPath );
                    png.ScaleAbsolute( 45 , 45 );
                    png.SetAbsolutePosition( 25 , 750 );
                    cb.AddImage( png );

                    Phrase phrase = new Phrase( );
                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );

                    Paragraph p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;

                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    string tx = "MODULO INCENTIVAZIONE 2021";

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
                    phrase.Add( new Chunk( tx , myFontIntestazioneBold ) );
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

                    string nominativo = String.Format( "{0} {1}" , Utente.EsponiAnagrafica( )._cognome.Trim( ) , Utente.EsponiAnagrafica( )._nome.Trim( ) );

                    phrase = new Phrase( );
                    if ( Utente.EsponiAnagrafica( )._genere.ToUpper( ).Equals( "F" ) )
                    {
                        phrase.Add( new Chunk( "La sottoscritta " , myFontCorpo ) );
                        phrase.Add( new Chunk( nominativo , myFontCorpoBold ) );
                        phrase.Add( new Chunk( " Matr. Aziendale " , myFontCorpo ) );
                        phrase.Add( new Chunk( matricola , myFontCorpoBold ) );
                        phrase.Add( new Chunk( " Nata a " , myFontCorpo ) );
                        phrase.Add( new Chunk( Utente.EsponiAnagrafica( )._comuneNascita.Trim( ) , myFontCorpoBold ) );
                    }
                    else
                    {
                        phrase.Add( new Chunk( "Il sottoscritto " , myFontCorpo ) );
                        phrase.Add( new Chunk( nominativo , myFontCorpoBold ) );
                        phrase.Add( new Chunk( " Matr. Aziendale " , myFontCorpo ) );
                        phrase.Add( new Chunk( matricola , myFontCorpoBold ) );
                        phrase.Add( new Chunk( " Nato a " , myFontCorpo ) );
                        phrase.Add( new Chunk( Utente.EsponiAnagrafica( )._comuneNascita.Trim( ) , myFontCorpoBold ) );
                    }

                    phrase.Add( new Chunk( " il " , myFontCorpo ) );
                    phrase.Add( new Chunk( Utente.EsponiAnagrafica( )._dataNascita.GetValueOrDefault( ).ToString( "dd/MM/yyyy" ) , myFontCorpoBold ) );

                    phrase.Add( new Chunk( " codice fiscale " , myFontCorpo ) );
                    phrase.Add( new Chunk( Utente.EsponiAnagrafica( )._cf , myFontCorpoBold ) );

                    phrase.Add( new Chunk( " dipendente dell'azienda " , myFontCorpo ) );
                    phrase.Add( new Chunk( Utente.EsponiAnagrafica( )._logo , myFontCorpoBold ) );

                    string prot = "";

                    using ( digiGappEntities dbDG = new digiGappEntities( ) )
                    {
                        var parametro = dbDG.MyRai_ParametriSistema.FirstOrDefault( x => x.Chiave == "Incentivazione012021Protocollo" );

                        if ( parametro != null )
                        {
                            prot = String.IsNullOrEmpty( parametro.Valore1 ) ? "" : parametro.Valore1;
                        }
                    }

                    phrase.Add( new Chunk( " in applicazione, ed ai sensi, della circolare prot. " + prot , myFontCorpo ) );
                    //phrase.Add( new Chunk( protocollo , myFontCorpoBold ) );

                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( "DICHIARA" , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_CENTER;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( "con la presente la volonta' di aderire all'iniziativa di incentivazione all'esodo su base volontaria alle condizioni e con le modalita' definite nella citata circolare." , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    string riga = String.Format( "Allo scopo, manifesta l’intenzione di cessare dal servizio in data {0} (u.g.s.), previa valutazione aziendale di compatibilità con le esigenze produttive. " , dataSelezionata );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( riga , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( "Inoltre, al fine di individuare la sua appartenenza ad una delle due platee definite dalla circolare e comunque la propria complessiva anzianita' assicurativa e contributiva nell'obiettivo di verificare la data di raggiungimento dei requisiti pensionistici, ovvero il possesso degli stessi, sotto la propria responsabilita'" , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( "DICHIARA" , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_CENTER;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    Phrase imgPhrase = new Phrase( );

                    if ( !Utente.TipoDipendente( ).Equals( "G" ) )
                    {
                        imgPhrase = new Paragraph( new Chunk( drawCircle( cb , ( scelta.Split( ';' ).Contains( "1" ) ) ) , 1f , 0f ) );
                        imgPhrase.Add( new Chunk( "di aver conseguito o di conseguire nel corso del 2021 i requisiti per la pensione anticipata \"in quota 100\"" , myFontCorpoItalic ) );
                        p = new Paragraph( imgPhrase );
                        ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                        document.Add( p );

                        phrase = new Phrase( );
                        phrase.Add( new Chunk( " " , myFontCorpo ) );
                        p = new Paragraph( phrase );
                        ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                        document.Add( p );
                    }

                    imgPhrase = new Phrase( );
                    imgPhrase = new Paragraph( new Chunk( drawCircle( cb , ( scelta.Split( ';' ).Contains( "2" ) ) ) , 1f , 0f ) );
                    imgPhrase.Add( new Chunk( "di non avere/non poter conseguire i requisiti per la pensione anticipata in \"quota 100\", ma di conseguire quale primo trattamento pensionistico utile" , myFontCorpo ) );
                    p = new Paragraph( imgPhrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( drawCircle( cb , ( scelta.Split( ';' ).Contains( "21" ) ) ) , 1f , 0f ) );
                    phrase.Add( new Chunk( "la pensione di vecchiaia" , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_CENTER;

                    phrase.Add( new Chunk( "     " , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_CENTER;

                    phrase.Add( new Chunk( drawCircle( cb , ( scelta.Split( ';' ).Contains( "22" ) ) ) , 1f , 0f ) );
                    phrase.Add( new Chunk( "la pensione anticipata" , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_CENTER;

                    phrase.Add( new Chunk( "     " , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_CENTER;

                    phrase.Add( new Chunk( drawCircle( cb , ( scelta.Split( ';' ).Contains( "23" ) ) ) , 1f , 0f ) );
                    phrase.Add( new Chunk( "la pensione di anzianita'" , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_CENTER;

                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( "SI IMPEGNA" , myFontIntestazioneBold ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_CENTER;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( "Nell'obiettivo di consentire la verifica di quanto sopra attestato, quale condizione di ammissione all'incentivazione ad acquisire sollecitamente ed a trasmettere ai competenti settori aziendali, mediante la casella di posta elettronica dedicata esodi2021@rai.it, la propria posizione contributiva complessiva (estratti contributivi), con riferimento a tutti i regimi previdenziali di assicurazione nel corso dell’intera vita lavorativa." , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( " " , myFontCorpo ) );
                    p = new Paragraph( phrase );
                    ( ( Paragraph ) p ).Alignment = Element.ALIGN_JUSTIFIED;
                    document.Add( p );

                    phrase = new Phrase( );
                    phrase.Add( new Chunk( "E' consapevole che fino alla consegna della suddetta documentazione la propria posizione e dunque la propria istanza non potra' essere oggetto di compiuta valutazione da parte dell'azienda." , myFontCorpo ) );
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="destinatario"></param>
        /// <param name="sedeGAPP"></param>
        private void InvioMailIncentivazione012021 ( string matricola , byte[] pdf )
        {
            try
            {
                string dest = CommonTasks.GetEmailPerMatricola( matricola );

                if ( String.IsNullOrWhiteSpace( dest ) )
                    dest = "P" + matricola + "@rai.it";

                //dest = "RUO.SIP.PRESIDIOOPEN@rai.it";

                GestoreMail mail = new GestoreMail( );

                List<Attachement> attachments = new List<Attachement>( );

                Attachement a = new Attachement( )
                {
                    AttachementName = "Modulo.pdf" ,
                    AttachementType = "Application/PDF" ,
                    AttachementValue = pdf
                };

                attachments.Add( a );

                string oggetto = " SelfService del Dipendente - Notifiche modulo incentivazione 2021";
                string corpo = "In allegato alla presente il modulo compilato. ";

                string titolo = "MODULO INCENTIVAZIONE 2021";
                string sottotitolo = "";

                MyRai_ParametriSistema parametro = null;

                using ( digiGappEntities dbDG = new digiGappEntities( ) )
                {
                    parametro = dbDG.MyRai_ParametriSistema.FirstOrDefault( x => x.Chiave == "Incentivazione012021InvioMailOggettoCorpo" );

                    if (parametro != null)
                    {
                        oggetto = String.IsNullOrEmpty( parametro.Valore1 ) ? oggetto : parametro.Valore1;
                        corpo = String.IsNullOrEmpty( parametro.Valore2 ) ? corpo : parametro.Valore2;
                    }

                    parametro = dbDG.MyRai_ParametriSistema.FirstOrDefault( x => x.Chiave == "Incentivazione012021InvioMailTitoli" );
                    if ( parametro != null )
                    {
                        titolo = String.IsNullOrEmpty( parametro.Valore1 ) ? titolo : parametro.Valore1;
                        sottotitolo = String.IsNullOrEmpty( parametro.Valore2 ) ? sottotitolo : parametro.Valore2;
                    }
                }

                var response = mail.InvioMail( "[CG] RaiPlace - Self Service <raiplace.selfservice@rai.it>" ,
                    oggetto ,
                    dest ,
                    "raiplace.selfservice@rai.it" ,
                    titolo ,
                    sottotitolo ,
                    corpo ,
                    null ,
                    null ,
                    attachments );

                if ( response != null && response.Errore != null )
                {
                    MyRai_LogErrori err = new MyRai_LogErrori( )
                    {
                        applicativo = "Portale" ,
                        data = DateTime.Now ,
                        provenienza = "ModuliController - InvioMailIncentivazione012021" ,
                        error_message = response.Errore + " per " + dest
                    };

                    using ( digiGappEntities db = new digiGappEntities( ) )
                    {
                        db.MyRai_LogErrori.Add( err );
                        db.SaveChanges( );
                    }
                }

                // reperimento degli indirizzi email dei dipendendi degli uffici del personale
            }
            catch ( Exception ex )
            {
                MyRai_LogErrori err = new MyRai_LogErrori( )
                {
                    applicativo = "Portale" ,
                    data = DateTime.Now ,
                    provenienza = "ModuliController - WEB - InvioMailIncentivazione012021" ,
                    error_message = ex.Message
                };

                using ( digiGappEntities db = new digiGappEntities( ) )
                {
                    db.MyRai_LogErrori.Add( err );
                    db.SaveChanges( );
                }
            }
        }

        private ModuloIncentivazione012021SimulazioneEnum VerificaSeMatricolaForzata ()
        {
            MyRai_ParametriSistema parametro = null;
            ModuloIncentivazione012021SimulazioneEnum result = ModuloIncentivazione012021SimulazioneEnum.None;

            using ( digiGappEntities dbDG = new digiGappEntities( ) )
            {
                parametro = dbDG.MyRai_ParametriSistema.FirstOrDefault( x => x.Chiave == "Incentivazione012021WidgetMatricoleForzate" );
            }

            if (parametro != null)
            {
                string mt = Utente.Matricola( );
                var tmp = parametro.Valore1.Split( ';' ).ToList( );

                if ( tmp.Where( w => w.Contains( mt ) ).Count() > 0 )
                {
                    var item = tmp.Where( w => w.Contains( mt ) ).FirstOrDefault( );
                    var opz = item.Split( ':' ).ToList( );

                    if (opz == null)
                    {
                        result = ModuloIncentivazione012021SimulazioneEnum.All;
                    }
                    else
                    {
                        string sub = "";
                        if (opz.Count > 1)
                        {
                            sub = opz[1];
                        }

                        if (String.IsNullOrEmpty(sub))
                        {
                            result = ModuloIncentivazione012021SimulazioneEnum.All;
                        }
                        else
                        {
                            switch (sub)
                            {
                                case "*":
                                    result = ModuloIncentivazione012021SimulazioneEnum.All;
                                    break;

                                case "G61":
                                    result = ModuloIncentivazione012021SimulazioneEnum.Gior61;
                                    break;

                                case "G62":
                                    result = ModuloIncentivazione012021SimulazioneEnum.Gior62;
                                    break;

                                case "*61":
                                    result = ModuloIncentivazione012021SimulazioneEnum.Dip61;
                                    break;

                                case "*62":
                                    result = ModuloIncentivazione012021SimulazioneEnum.Dip62;
                                    break;

                                default:
                                    result = ModuloIncentivazione012021SimulazioneEnum.All;
                                    break;
                            }
                        }
                    }
                }
            }

            return result;
        }
        #endregion
    }
}