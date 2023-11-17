using Detax.Helpers;
using Detax.it.rai.servizi.hrgb;
using Detax.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using myRaiData;
using myRaiData.Incentivi;
using myRaiHelper;
using MyRaiServiceInterface.MyRaiServiceReference1;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
//using Anagrafica = myRaiHelper.UtenteHelper.Anagrafica;

namespace Detax
{
    class Program
    {
        public class SceltaDetassazioneJson
        {
            public string Modello { get; set; }

            public string SceltaTab1 { get; set; }

            public string SceltaTab2 { get; set; }
        }

        public class Report
        {
            public string Societa { get; set; }

            public int BustaPaga { get; set; }

            public int Craipi50 { get; set; }

            public int Craipi100 { get; set; }

            public int Welfare50 { get; set; }

            public int Welfare100 { get; set; }
        }

        static void Main ( string[] args )
        {
            //LanciarePrimaDellaDeleteSullaTabella( );
            //ImportaDipendenti( );
            //RigeneraPDFDipendenti( );
            ExportScelteDipendente2();
           
        }

        public static void ExportTotali()
        {
            Output.WriteLine("Avvio ExportTotali " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

            string societa = "";
            string tx = "";
            List<DETASSAZIONE> items = new List<DETASSAZIONE>();
            List<Report> report = new List<Report>();
            using (IncentiviEntities db = new IncentiviEntities())
            {
                int anno = DateTime.Now.Year;
                string query = "SELECT [Id]" +
                                 ",[Matricola]" +
                                 ",[Anno]" +
                                 ",[CodiceDetassazione]" +
                                 ",[DataCompilazione]" +
                                 ",[Modello]" +
                                 ",[Scelta]" +
                                 ",[Nominativo]" +
                                 ",[ModelloAssegnato]" +
                                 ",[Applicazione]" +
                                 ",[Giorni]" +
                                 ",[RAL]" +
                                 ",[Categoria]" +
                                 ",NULL as PDF" +
                                 ",[DataLettura]" +
                                 ",[Importo]" +
                                 ",[PdrControllo]" +
                                 "  FROM[CZNDB].[dbo].[DETASSAZIONE]" +
                                 "  where CodiceDetassazione = 'DETAX'" +
                                 "  and Anno = #ANNO#";

                query = query.Replace("#ANNO#", anno.ToString());
                items = db.Database.SqlQuery<DETASSAZIONE>(query).ToList();

                societa = "";
                tx = "";
                tx = String.Format("SOCIETA;BUSTA PAGA;50% CRAIPI;100% CRAIPI;50% WELFARE;100% WELFARE;TOTALE");
                ScriviSuFile(tx, "REPORTSCELTEPDR");
                Output.WriteLine(tx);

                if (items != null && items.Any())
                {
                    int count = 1;
                    int tot = items.Count();
                    foreach (var i in items)
                    {
                        societa = db.SINTESI1.Where(x => x.COD_MATLIBROMAT == i.Matricola).Select(x => x.COD_IMPRESACR).FirstOrDefault();
                        Console.WriteLine(count + " di " + tot);
                        tx = "";

                        var codifica = db.CODIFYIMP.Where(w => w.COD_IMPRESA.Equals(societa)).FirstOrDefault();
                        if (codifica != null)
                        {
                            string soc = codifica.COD_SOGGETTO;
                            bool daAggiornare = false;
                            Report tempElement = report.Where(w => w.Societa == soc).FirstOrDefault();
                            if (tempElement == null)
                            {
                                tempElement = new Report()
                                {
                                    Societa = soc
                                };
                                daAggiornare = true;
                            }

                            string scelta = ConvertiScelta(i);

                            // Blank (scelta non effettuata) / Cedo     - Per destinare il PDR in cedolino
                            // CRA50                                    - Destinare il 50 % del PDR a CRAIPI e il 50 % a cedolino
                            // CRA100                                   - Destinare il 100 % del PDR a CRAIPI
                            // WEL50                                    - Destinare il 50 % del PDR a Welfare
                            // WEL100                                   - Destinare il 100 % del PDR a Welfare
                            // CEDO                                     - Confermare l'applicazione della tassazione ordinaria
                            // RINUN                                    - Richiedere l'applicazione della tassazione ordinaria
                            switch(scelta)
                            {
                                case "":
                                    tempElement.BustaPaga++;
                                    break;
                                case "CRA50":
                                    tempElement.Craipi50++;
                                    break;
                                case "CRA100":
                                    tempElement.Craipi100++;
                                    break;
                                case "WEL50":
                                    tempElement.Welfare50++;
                                    break;
                                case "WEL100":
                                    tempElement.Welfare100++;
                                    break;
                                case "CEDO":
                                    tempElement.BustaPaga++;
                                    break;
                                case "RINUN":
                                    tempElement.BustaPaga++;
                                    break;
                            }

                            if (daAggiornare)
                            {
                                report.Add(tempElement);
                            }
                        }
                        else
                        {
                            tx = String.Format("{0} scartato - società non trovata {1}", i.Matricola, societa);
                            ScriviFile(tx, "ExportScelteDipendente_NoSocieta");
                        }

                        count++;
                    }
                }
            }

            foreach(var r in report)
            {
                tx = String.Format("SOCIETA;BUSTA PAGA;50% CRAIPI;100% CRAIPI;50% WELFARE;100% WELFARE;TOTALE");

                int _tempTot = r.BustaPaga + r.Craipi50 + r.Craipi100 + r.Welfare50 + r.Welfare100;

                tx = String.Format("{0};{1};{2};{3};{4};{5};{6}", r.Societa, r.BustaPaga, r.Craipi50,
                    r.Craipi100, r.Welfare50, r.Welfare100, _tempTot);
                ScriviSuFile(tx, "REPORTSCELTEPDR");
                Output.WriteLine(tx);
            }

            Output.WriteLine("Fine ExportTotali " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
        }

       
        //private static void LanciarePrimaDellaDeleteSullaTabella ( )
        //{
        //    try
        //    {
        //        MyRaiService1Client servizioWCF = new MyRaiService1Client( );
        //        using ( HRPADBEntities db = new HRPADBEntities( ) )
        //        {
        //            var items = db.T_DetaxNew.Where( w => !String.IsNullOrEmpty( w.Modello_T_DetaxNew ) ).ToList( );

        //            if ( items != null && items.Any( ) )
        //            {
        //                foreach ( var i in items )
        //                {
        //                    // chiamata CICS
        //                    var risposta = servizioWCF.ResetModuloDetassazione( "P" + i.Matricola_T_DetaxNew , i.Matricola_T_DetaxNew , i.Modello_T_DetaxNew.Equals( "2C" ) ? " " : i.Applicazione_T_DetaxNew );

        //                    if ( !risposta.Esito )
        //                    {
        //                        throw new Exception( risposta.Errore );
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    catch ( Exception ex )
        //    {

        //    }
        //}

        /// <summary>
        /// Importa da file txt i dipendenti da inserire nel db T_DetaxNew
        /// </summary>
        private static void ImportaDipendenti()
        {
            int counter = 0;
            int errori = 0;
            string line;
            List<int> ids = new List<int>();
            string matricola = "";
            string nominativo = "";
            string modelloAssegnato = "";
            string modelloFirmato = "";
            string scelta = "";
            string applicazione = "";

            string sRAL = "";
            string sGiorni = "";
            string categoria = "";
            string sImporto = "";
            decimal importo = 0.0m;

            List<Sintesi1Flat> sintesi = new List<Sintesi1Flat>();
            List<string> log = new List<string>();

            using (IncentiviEntities db = new IncentiviEntities())
            {
                sintesi = db.SINTESI1.Where(w => w.DTA_FINE_CR > DateTime.Today).ToList().Select(w => new Sintesi1Flat
                {
                    Matricola = w.COD_MATLIBROMAT,
                    Nominativo = w.DES_COGNOMEPERS.Trim() + " " + w.DES_NOMEPERS.Trim()
                }).ToList();
            }

            //MyRaiService1Client wcf = new MyRaiService1Client();

            // lettura del file txt
            System.IO.StreamReader file = new System.IO.StreamReader(@"C:\RAI\FILEOUT.txt");
            while ((line = file.ReadLine()) != null)
            {
                matricola = "";
                nominativo = "";
                modelloAssegnato =
                modelloFirmato = "";
                scelta = "";
                applicazione = "";
                sRAL = "";
                sGiorni = "";
                importo = 0.0m;
                sImporto = "";
                categoria = "";
                counter++;
                log = new List<string>();
                Console.WriteLine(counter);
                try
                {
                    string riga = line;
                    // Es: 000035;750;381;161201;      ;003557400;000000365;A;S13;   ;   ;         ;1C.  . .S;
                    // ES 2022: 000035;750;381;161201;      ;004038591;000000365; ;X93;   ;   ;         ;1C.  . .S;               :00216080:                                          
                    string dataOraAccettazione = "";
                    DateTime dataAccettazione = DateTime.Now;

                    List<string> rigaSplittata = riga.Split(';').ToList();
                    matricola = rigaSplittata[0];
                    string valori = rigaSplittata[12];
                    string PdrControllo = rigaSplittata[7];
                    log.Add(matricola);

                    sRAL = rigaSplittata[5];
                    sGiorni = rigaSplittata[6];
                    categoria = rigaSplittata[8];
                    sImporto = rigaSplittata[14];

                    int giorni = 0;
                    int ral = 0;

                    if (!String.IsNullOrEmpty(sGiorni))
                    {
                        giorni = int.Parse(sGiorni);
                    }

                    if (!String.IsNullOrEmpty(sRAL))
                    {
                        ral = int.Parse(sRAL);
                    }

                    if (!String.IsNullOrEmpty(sImporto))
                    {
                        importo = decimal.Parse(sImporto);
                        importo = importo / 100;
                    }

                    try
                    {
                        if (rigaSplittata.Count == 14 &&
                            rigaSplittata[13].Length > 0)
                        {
                            dataOraAccettazione = rigaSplittata[13].Substring(25, 8);

                            DateTime.TryParseExact(dataOraAccettazione, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out dataAccettazione);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Add("errore conversione data");
                        ScriviFile(string.Join("|", log));
                        continue;
                    }

                    List<string> valoriSplit = valori.Split('.').ToList();
                    //if (!String.IsNullOrEmpty(valoriSplit[0]) && valoriSplit[0].ToUpper() != "ND")
                    if (!String.IsNullOrEmpty(valoriSplit[0]))
                    {
                        var item = sintesi.Where(w => w.Matricola == matricola).FirstOrDefault();
                        if (item != null)
                        {
                            nominativo = item.Nominativo;
                        }

                        if (String.IsNullOrWhiteSpace(nominativo))
                        {
                            log.Add("errore recupera utente");
                            ScriviFile(string.Join("|", log));
                            ScriviFile(string.Join("|", log), "NOSINTESI1");
                            continue;
                        }

                        modelloAssegnato = valoriSplit[0];
                        modelloFirmato = valoriSplit[1];
                        scelta = valoriSplit[2];
                        applicazione = valoriSplit[3];

                        if (!String.IsNullOrEmpty(modelloAssegnato))
                        {
                            modelloAssegnato = modelloAssegnato.Trim();

                            // dati incongruenti
                            if (modelloAssegnato.Length == 0)
                            {
                                log.Add("Modello assegnato non presente");
                                ScriviFile(string.Join("|", log));
                                continue;
                            }
                        }
                        else
                        {
                            log.Add("Modello assegnato non presente");
                            ScriviFile(string.Join("|", log));
                            continue;
                        }

                        if (!String.IsNullOrEmpty(modelloFirmato))
                        {
                            modelloFirmato = modelloFirmato.Trim();
                            if (modelloFirmato.Length == 0)
                                modelloFirmato = null;
                        }

                        // se ha firmato il modello ed il modello è 2C
                        // allora verifica che ci sia stata una scelta
                        if (!String.IsNullOrEmpty(modelloFirmato))
                        {
                            scelta = scelta.Trim();
                            if (modelloFirmato.Equals("2C") && scelta.Length == 0)
                            {
                                // dati incongruenti
                                log.Add("Scelta assente");
                                ScriviFile(string.Join("|", log));
                                continue;
                            }
                            if (scelta.Length == 0)
                                scelta = null;
                        }

                        if (String.IsNullOrEmpty(nominativo))
                        {
                            // dati incongruenti
                            log.Add("Manca il nominativo");
                            ScriviFile(string.Join("|", log));
                            continue;
                        }

                        nominativo = nominativo.Trim();

                        if (String.IsNullOrEmpty(modelloAssegnato))
                        {
                            modelloAssegnato = null;
                        }

                        if (String.IsNullOrEmpty(modelloFirmato))
                        {
                            modelloFirmato = null;
                        }

                        if (String.IsNullOrWhiteSpace(scelta))
                        {
                            scelta = null;
                        }

                        if (String.IsNullOrEmpty(applicazione))
                        {
                            applicazione = null;
                        }

                        using (IncentiviEntities db = new IncentiviEntities())
                        {
                            int anno = DateTime.Now.Year;
                            //bool exists = db.DETASSAZIONE.Where(w => w.Matricola.Equals(matricola) && (w.DataCompilazione != null && w.DataCompilazione.Value.Year == anno)).Count() > 0;
                            bool exists = db.DETASSAZIONE.Where(w => w.Matricola.Equals(matricola) && w.Anno == anno).Count() > 0;
                            if (!exists )
                            {
                                DETASSAZIONE newItem = new DETASSAZIONE()
                                {
                                    Matricola = matricola,
                                    Anno = anno,
                                    Applicazione = applicazione,
                                    Categoria = categoria,
                                    CodiceDetassazione = "DETAX",
                                    DataCompilazione = null,
                                    Giorni = giorni,
                                    ModelloAssegnato = modelloAssegnato,
                                    Modello = modelloFirmato,
                                    Nominativo = nominativo,
                                    Pdf = null,
                                    RAL = ral,
                                    Scelta = scelta,
                                    Importo = importo,
                                    PdrControllo = PdrControllo
                                };

                                db.DETASSAZIONE.Add(newItem);
                                db.SaveChanges();
                                log.Add("Aggiunto");
                            }
                            else
                            {
                                log.Add("Già presente");
                            }
                        }
                    }
                    else
                    {
                        log.Add("ND");
                    }
                }
                catch (Exception ex)
                {
                    errori++;
                    log.Add("Errore " + ex.Message);
                    ScriviFile(string.Join("|", log));
                    continue;
                }

                string joined = string.Join("|", log);
                ScriviFile(joined);
            }

            Console.WriteLine("Letti: " + counter);
            Console.WriteLine("Errori: " + errori);
            Console.ReadLine();
        }

        //private static void RigeneraPDFDipendenti()
        //{
        //    try
        //    {
        //        using ( HRPADBEntities db = new HRPADBEntities( ) )
        //        {
        //            var items = db.T_DetaxNew.Where( w => !String.IsNullOrEmpty( w.Modello_T_DetaxNew ) && w.PDF_T_DetaxNew == null ).ToList( );

        //            if ( items != null && items.Any( ) )
        //            {
        //                foreach ( var i in items )
        //                {
        //                    Anagrafica anagrafica = new Anagrafica( );

        //                    Service service = new Service( );

        //                    string str_temp = service.EsponiAnagrafica( "RAICV;" + i.Matricola_T_DetaxNew + ";;E;0" );
        //                    string[] temp = str_temp.ToString( ).Split( ';' );

        //                    if ( ( temp != null ) && ( temp.Count( ) > 16 ) )
        //                    {
        //                        anagrafica = CaricaAnagrafica( temp );
        //                    }
        //                    else
        //                    {
        //                        continue;
        //                    }

        //                    string nome = anagrafica._nome;
        //                    string cognome = anagrafica._cognome;
        //                    string nominativo = String.Format( "{0} {1}" , anagrafica._nome , anagrafica._cognome );

        //                    DateTime dataNascita = anagrafica._dataNascita.GetValueOrDefault( );
        //                    string dNascita = "";

        //                    if ( dataNascita.Equals( DateTime.MinValue ) ||
        //                        dataNascita <= DateTime.MinValue )
        //                    {
        //                        dNascita = "";
        //                    }
        //                    else
        //                    {
        //                        dNascita = dataNascita.ToString( "dd/MM/yyyy" );
        //                    }

        //                    if ( String.IsNullOrEmpty( nome ) || String.IsNullOrEmpty( cognome ) )
        //                    {
        //                        nominativo = i.Nominativo_T_DetaxNew.Trim( );
        //                    }

        //                    // creazione del pdf
        //                    byte[] myArrayOfBytes = null;

        //                    if (i.Modello_T_DetaxNew.Equals("1C"))
        //                    {
        //                        myArrayOfBytes = CreazionePDF( i.Matricola_T_DetaxNew , nominativo ,
        //                            anagrafica._cf ,
        //                            anagrafica._comuneNascita ,
        //                            dNascita ,
        //                            anagrafica._genere );
        //                    }
        //                    else
        //                    {
        //                        myArrayOfBytes = CreazionePDF2C( i.Matricola_T_DetaxNew , nominativo ,
        //                                                            anagrafica._cf ,
        //                                                            anagrafica._comuneNascita ,
        //                                                            dNascita ,
        //                                                            anagrafica._genere ,
        //                                                            int.Parse( i.Scelta_T_DetaxNew ) );
        //                    }

        //                    if ( myArrayOfBytes == null )
        //                    {
        //                        continue;
        //                    }

        //                    i.PDF_T_DetaxNew = myArrayOfBytes;
        //                    db.SaveChanges( );
        //                }
        //            }
        //        }
        //    }
        //    catch(Exception ex)
        //    {

        //    }
        //}

        //public static Anagrafica CaricaAnagrafica ( string[] resp )
        //{
        //    if ( ( resp != null ) && ( resp.Count( ) > 1 ) )
        //    {
        //        Anagrafica utente = new Anagrafica( );

        //        utente._cognome = resp[2];
        //        utente._nome = resp[1];
        //        //utente._foto = CommonHelper.GetImmagineBase64( resp[0] );
        //        utente._comuneNascita = resp[12];
        //        utente._contratto = resp[6];
        //        utente._figProfessionale = resp[8];
        //        utente._qualifica = resp[10];
        //        utente._dataNascita = ( resp[11] ) != null ? CommonHelper.ConvertToDate( resp[11] ) : Convert.ToDateTime( null );
        //        utente._comuneNascita = resp[12];
        //        utente._provinciaNascita = resp[24];
        //        utente._matricola = resp[0];
        //        utente._logo = resp[14];
        //        utente._dataAssunzione = ( resp[3] ) != null ? CommonHelper.ConvertToDate( resp[3] ) : Convert.ToDateTime( null );//CommonHelper.ConvertToDate(resp[3]);
        //                                                                                                                           //campi aggiuntivi
        //        utente._codiceFigProf = resp[7];
        //        utente._codiceContratto = resp[5];
        //        utente._codiceQualifica = resp[9];
        //        utente._sedeApppartenenza = resp[17];
        //        utente._genere = resp[18];
        //        utente._cf = resp[19];
        //        utente._indirizzoresidenza = resp[20];
        //        utente._capresidenza = resp[21];
        //        utente._comuneresidenza = resp[22];
        //        utente._provinciaresidenza = resp[23];
        //        utente._nazionalita = resp[13];
        //        utente._categoria = resp[25];

        //        utente.SedeContabile = resp[29];
        //        utente.SedeContabileDescrizione = resp[30];
        //        utente._statoNascita = resp[13];
        //        utente._email = resp[15];
        //        utente._telefono = resp[16];
        //        utente._sezContabile = resp[27];

        //        return utente;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

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
                    tableRighe.AddCell( WriteCell( "RINUNCIA ALL’APPLICAZIONE DELL’IMPOSTA SOSTITUTIVA DEL 5% SUL PREMIO DI RISULTATO PER L’ANNO 2022" , border , 2 , 1 , myFontIntestazione ) );
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
                    tableRighe.AddCell( WriteCell( "all’applicazione dell’imposta sostitutiva del 5% sulle somme corrisposte nell’anno 2023 a titolo di premio di risultato anno 2022 e chiede, pertanto, che sia applicata sul premio in questione la tassazione ordinaria." , border , 2 , 0 , myFontIntestazione ) );
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
                    tableRighe.AddCell( WriteCell( "RICHIESTA DI APPLICAZIONE DELL’IMPOSTA SOSTITUTIVA DEL 5% SUL PREMIO DI RISULTATO PER L’ANNO 2022" , border , 3 , 1 , myFontIntestazione ) );
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
                    tableRighe.AddCell( WriteCell( "che nell'anno 2022 non ha percepito redditi di lavoro dipendente (inclusi gli eventuali premi di risultato assoggettati al 10% riportati al punto 572 della Certificazione Unica 2023 per i redditi 2022 ed eventuali indennita' di disoccupazione, di maternita', di malattia, ecc) e/o redditi da pensione da altro sostituto d'imposta diverso da Rai/Rai Cinema/Rai Way/Rai Com " , border , 2 , 0 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( " " , border , 3 , 1 , myFontIntestazione ) );

                    tableRighe.AddCell( WriteCell( "ovvero," , border , 3 , 0 , myFontIntestazione ) );
                    tableRighe.AddCell( WriteCell( " " , border , 3 , 1 , myFontIntestazione ) );
                    tableRighe.AddCell( writeCellTab( cb , border , 1 , 1 , ( scelta == 2 ) ) );
                    tableRighe.AddCell( WriteCell( "che nell'anno 2022 ha percepito redditi di lavoro dipendente (inclusi gli eventuali premi di risultato assoggettati al 10% riportati al punto 572 della Certificazione Unica 2023 per i redditi 2022), e/o indennita' sostitutive (come ad esempio indennita' di disoccupazione, di maternita', di malattia, ecc) e/o redditi da pensione anche da altri sostituti d'imposta per un importo che sommato ai redditi percepiti da Rai/Rai Cinema/Rai Way/Rai Com non è superiore a euro 80.000." , border , 2 , 0 , myFontIntestazione ) );
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


        public static void ExportScelteDipendente2()
        {
            Output.WriteLine("Avvio ExportScelteDipendente " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

            using (IncentiviEntities db = new IncentiviEntities())
            {
                int anno = DateTime.Now.Year;

                var items = db.DETASSAZIONE.Where(x => x.Anno == anno && x.CodiceDetassazione == "DETAX")
                    .Select(x => new { x.Matricola, x.Importo, x.DataCompilazione, x.DataLettura,x.ModelloAssegnato,x.Scelta }).ToList();

                //var items = (from detax in db.DETASSAZIONE
                //             where detax.Anno == anno &&
                //             detax.CodiceDetassazione == "DETAX"
                //             select detax).ToList();

                string societa = "";
                string tx = "";
                //data.ToString("yyyyMMdd")
                tx = String.Format("MATRICOLA;SOCIETA;SCELTA;IMPORTO");
                ScriviFile(tx, "ExportScelteDipendente");
                Output.WriteLine(tx);

                if (items != null && items.Any())
                {
                    int count = 1;
                    int tot = items.Count();
                    foreach (var i in items)
                    {
                        societa = db.SINTESI1.Where(x => x.COD_MATLIBROMAT == i.Matricola).Select(x => x.COD_IMPRESACR).FirstOrDefault();
                        Console.WriteLine(count + " di " + tot);
                        tx = "";

                        //if (societa != "B")
                        //{
                        var codifica = db.CODIFYIMP.Where(w => w.COD_IMPRESA.Equals(societa)).FirstOrDefault();
                        if (codifica != null)
                        {
                            string dataScelta = "";
                            if (i.DataCompilazione.HasValue)
                            {
                                dataScelta = i.DataCompilazione.GetValueOrDefault().ToString("yyyyMMdd");
                            }

                            string dataVisualizzazione = "";
                            if (i.DataLettura.HasValue)
                            {
                                dataVisualizzazione = i.DataLettura.GetValueOrDefault().ToString("yyyyMMdd");
                            }
                            string soc = codifica.COD_SOGGETTO;
                            tx = String.Format("{0};{1};{2};{3}", i.Matricola, soc,  ConvertiScelta2(i.ModelloAssegnato,i.Scelta), i.Importo);
                            ScriviFile(tx, "ExportScelteDipendente");
                            Output.WriteLine(tx);
                        }
                        else
                        {
                            tx = String.Format("{0} scartato - società non trovata {1}", i.Matricola, societa);
                            ScriviFile(tx, "ExportScelteDipendente_NoSocieta");
                        }
                        //}
                        //else
                        //{
                        //    tx = String.Format("{0} scartato - RAiWay codice società: {1}", i.Matricola, societa);
                        //    ScriviFile(tx, "ExportScelteDipendente_RaiWay");                            
                        //}

                        count++;
                    }
                }
            }

            Output.WriteLine("Fine ExportScelteDipendente " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
        }
        public static void ExportScelteDipendente()
        {
            Output.WriteLine("Avvio ExportScelteDipendente " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));

            using (IncentiviEntities db = new IncentiviEntities())
            {
                int anno = DateTime.Now.Year;

                var items = (from detax in db.DETASSAZIONE
                             where detax.Anno == anno &&
                             detax.CodiceDetassazione == "DETAX"
                             select detax).ToList();

                string societa = "";
                string tx = "";
                //data.ToString("yyyyMMdd")
                tx = String.Format("MATRICOLA;SOCIETA;DATA COMPILAZIONE;MODELLO ASSEGNATO;SCELTA;DATA VISUALIZZAZIONE");
                ScriviFile(tx, "ExportScelteDipendente");
                Output.WriteLine(tx);

                if (items != null && items.Any())
                {
                    int count = 1;
                    int tot = items.Count();
                    foreach (var i in items)
                    {
                        societa = db.SINTESI1.Where(x => x.COD_MATLIBROMAT == i.Matricola).Select(x => x.COD_IMPRESACR).FirstOrDefault();
                        Console.WriteLine(count + " di " + tot);
                        tx = "";

                        //if (societa != "B")
                        //{
                            var codifica = db.CODIFYIMP.Where(w => w.COD_IMPRESA.Equals(societa)).FirstOrDefault();
                            if (codifica != null)
                            {
                                string dataScelta = "";
                                if (i.DataCompilazione.HasValue)
                                {
                                    dataScelta = i.DataCompilazione.GetValueOrDefault().ToString("yyyyMMdd");
                                }

                                string dataVisualizzazione = "";
                                if (i.DataLettura.HasValue)
                                {
                                    dataVisualizzazione = i.DataLettura.GetValueOrDefault().ToString("yyyyMMdd");
                                }
                                string soc = codifica.COD_SOGGETTO;
                                tx = String.Format("{0};{1};{2};{3};{4};{5}", i.Matricola, soc, dataScelta, i.ModelloAssegnato, ConvertiScelta(i), dataVisualizzazione);
                                ScriviFile(tx, "ExportScelteDipendente");
                                Output.WriteLine(tx);
                            }
                            else
                            {
                                tx = String.Format("{0} scartato - società non trovata {1}", i.Matricola, societa);
                                ScriviFile(tx, "ExportScelteDipendente_NoSocieta");
                            }
                        //}
                        //else
                        //{
                        //    tx = String.Format("{0} scartato - RAiWay codice società: {1}", i.Matricola, societa);
                        //    ScriviFile(tx, "ExportScelteDipendente_RaiWay");                            
                        //}

                        count++;
                    }
                }
            }

            Output.WriteLine("Fine ExportScelteDipendente " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sceltaJson"></param>
        /// <returns></returns>
        private static string ConvertiScelta(DETASSAZIONE item)
        {
            // Blank (scelta non effettuata) / Cedo     - Per destinare il PDR in cedolino
            // CRA50                                    - Destinare il 50 % del PDR a CRAIPI e il 50 % a cedolino
            // CRA100                                   - Destinare il 100 % del PDR a CRAIPI
            // WEL50                                    - Destinare il 50 % del PDR a Welfare
            // WEL100                                   - Destinare il 100 % del PDR a Welfare
            // CEDO                                     - Confermare l'applicazione della tassazione ordinaria
            // RINUN                                    - Richiedere l'applicazione della tassazione ordinaria

            string sceltaJson = item.Scelta;
            string result = "";

            SceltaDetassazioneJson scelta = new SceltaDetassazioneJson();
            if (!String.IsNullOrEmpty( sceltaJson))
            {
                scelta = Newtonsoft.Json.JsonConvert.DeserializeObject<SceltaDetassazioneJson>(sceltaJson);

                if (item.ModelloAssegnato == "2C" || item.ModelloAssegnato == "4C")
                {
                    if (scelta.SceltaTab1 == "4")
                    {
                        // "Confermare l'applicazione della tassazione ordinaria."
                        result = "CEDO";
                    }
                    else
                    {
                        result = scelta.SceltaTab2;
                    }
                }
                else if (item.ModelloAssegnato == "1C")
                {
                    if (scelta.SceltaTab1 == "2")
                    {
                        // "Richiedere l'applicazione della tassazione ordinaria."
                        result = "RINUN";
                    }
                    else
                    {
                        result = scelta.SceltaTab2;
                    }
                }
            }
            return result;
        }
        private static string ConvertiScelta2(string ModelloAssegnato, string Scelta)
        {
            // Blank (scelta non effettuata) / Cedo     - Per destinare il PDR in cedolino
            // CRA50                                    - Destinare il 50 % del PDR a CRAIPI e il 50 % a cedolino
            // CRA100                                   - Destinare il 100 % del PDR a CRAIPI
            // WEL50                                    - Destinare il 50 % del PDR a Welfare
            // WEL100                                   - Destinare il 100 % del PDR a Welfare
            // CEDO                                     - Confermare l'applicazione della tassazione ordinaria
            // RINUN                                    - Richiedere l'applicazione della tassazione ordinaria

            string sceltaJson = Scelta;
            string result = "";

            SceltaDetassazioneJson scelta = new SceltaDetassazioneJson();
            if (!String.IsNullOrEmpty(sceltaJson))
            {
                scelta = Newtonsoft.Json.JsonConvert.DeserializeObject<SceltaDetassazioneJson>(sceltaJson);

                if (ModelloAssegnato == "2C" || ModelloAssegnato == "4C")
                {
                    if (scelta.SceltaTab1 == "4")
                    {
                        // "Confermare l'applicazione della tassazione ordinaria."
                        result = "CEDO";
                    }
                    else
                    {
                        result = scelta.SceltaTab2;
                    }
                }
                else if (ModelloAssegnato == "1C")
                {
                    if (scelta.SceltaTab1 == "2")
                    {
                        // "Richiedere l'applicazione della tassazione ordinaria."
                        result = "RINUN";
                    }
                    else
                    {
                        result = scelta.SceltaTab2;
                    }
                }
            }
            return result;
        }
        private static void ScriviFile ( string msg , string nomeFile = "" )
        {
            if ( !String.IsNullOrEmpty( msg ) )
            {
                string filelog1 = "";
                var location = System.Reflection.Assembly.GetEntryAssembly( ).Location;

                var directoryPath = Path.GetDirectoryName( location );
                var logPath = Path.Combine( directoryPath , nomeFile );
                if ( !Directory.Exists( logPath ) )
                    Directory.CreateDirectory( logPath );

                filelog1 = Path.Combine( logPath , "ConsoleLog_" + DateTime.Now.ToString( "yyyyMMdd" ) + ".txt" );

                File.AppendAllText( filelog1 , DateTime.Now.ToString( "dd/MM/yyyy HH:mm:ss" ) + " " + msg + "\r\n" );
            }
        }

        private static void ScriviSuFile(string msg, string nomeFile = "")
        {
            if (!String.IsNullOrEmpty(msg))
            {
                string filelog1 = "";
                var location = System.Reflection.Assembly.GetEntryAssembly().Location;

                var directoryPath = Path.GetDirectoryName(location);
                var logPath = Path.Combine(directoryPath, nomeFile);
                if (!Directory.Exists(logPath))
                    Directory.CreateDirectory(logPath);

                filelog1 = Path.Combine(logPath, "ConsoleLog_" + DateTime.Now.ToString("yyyyMMdd") + ".txt");

                File.AppendAllText(filelog1, msg + "\r\n");
            }
        }
    }
}
