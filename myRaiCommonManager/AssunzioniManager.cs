using iTextSharp.text;
using iTextSharp.text.pdf;
using myRaiCommonModel;
using myRaiData.Incentivi;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Xceed.Words.NET;

namespace myRaiCommonManager
{
    public class WriteCellsClass
    {
        #region PRIVATE

        public static PdfPCell WriteCell(string text, int border, int colspan, int textAlign, Font f, float paddingTop = 0)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, f));
            cell.Border = border;
            cell.VerticalAlignment = Element.ALIGN_TOP;
            cell.HorizontalAlignment = textAlign; //0=Left, 1=Centre, 2=Right
            cell.Colspan = colspan;
            cell.PaddingTop = paddingTop;
            return cell;
        }

        public static PdfPCell WriteCell(iTextSharp.text.Image img, int border, int colspan, int textAlign, float paddingTop = 0)
        {
            PdfPCell cell = new PdfPCell(img);
            cell.Border = border;
            cell.VerticalAlignment = Element.ALIGN_TOP;
            cell.HorizontalAlignment = textAlign; //0=Left, 1=Centre, 2=Right
            cell.Colspan = colspan;
            cell.PaddingTop = paddingTop;
            return cell;
        }

        public static PdfPCell WriteCell(Phrase text, int border, int colspan, int textAlign, float paddingTop = 0)
        {
            PdfPCell cell = new PdfPCell(text);
            cell.Border = border;
            cell.HorizontalAlignment = textAlign; //0=Left, 1=Centre, 2=Right
            cell.VerticalAlignment = Element.ALIGN_TOP;
            cell.PaddingTop = paddingTop;
            cell.Colspan = colspan;
            return cell;
        }

        public static PdfPCell WriteCell(Paragraph text, int border, int colspan, int textAlign, float paddingTop = 0)
        {
            PdfPCell cell = new PdfPCell(text);
            cell.Border = border;
            cell.HorizontalAlignment = textAlign; //0=Left, 1=Centre, 2=Right
            cell.VerticalAlignment = Element.ALIGN_TOP;
            cell.Colspan = colspan;
            cell.PaddingTop = paddingTop;
            return cell;
        }

        public static iTextSharp.text.Image drawCircle(PdfContentByte contentByte, bool full)
        {
            var template = contentByte.CreateTemplate(20, 20);

            int arcPosStartY = 0;
            int arcPosStartX = 0;
            int arcPosEndX = 10;
            int arcPosEndY = 10;

            template.Arc(arcPosStartX, arcPosStartY, arcPosEndX, arcPosEndY, 0, 360);
            if (full)
            {
                template.SetRGBColorFill(0, 0, 0);
                template.Fill();
            }
            template.SetRGBColorStroke(0, 0, 0);
            template.Stroke();

            var img = iTextSharp.text.Image.GetInstance(template);

            return img;
        }

        public static iTextSharp.text.Image drawCircleMin(PdfContentByte contentByte, bool full)
        {
            var template = contentByte.CreateTemplate(10, 10);

            int arcPosStartY = 0;
            int arcPosStartX = 0;
            int arcPosEndX = 5;
            int arcPosEndY = 5;

            template.Arc(arcPosStartX, arcPosStartY, arcPosEndX, arcPosEndY, 0, 360);
            if (full)
            {
                template.SetRGBColorFill(0, 0, 0);
                template.Fill();
            }
            template.SetRGBColorStroke(0, 0, 0);
            template.Stroke();

            var img = iTextSharp.text.Image.GetInstance(template);

            return img;
        }

        public static PdfPCell writeCellTab(PdfContentByte cb, int border, int colspan, int textAlign, bool full)
        {
            PdfPCell cell;
            iTextSharp.text.Image img = drawCircle(cb, full);
            cell = new PdfPCell(img);
            cell.Border = border;
            cell.HorizontalAlignment = textAlign; //0=Left, 1=Centre, 2=Right
            cell.Colspan = colspan;
            return cell;
        }

        #endregion
    }

    public class AssunzioniManagerPdfPageEventHelper : PdfPageEventHelper
    {
        public const int fontCorpo = 12;
        public const string FONTNAME = "Calibri"; //"Times-Roman"
        public string _imgPath = "";
        public string codSocieta = "0";
        private int pagina = 1;

        public AssunzioniManagerPdfPageEventHelper(string codiceSocieta)
        {
            codSocieta = codiceSocieta;
            pagina = 1;
        }

        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            try
            {
                base.OnOpenDocument(writer, document);
            }
            catch (DocumentException de)
            {
            }
            catch (System.IO.IOException ioe)
            {
            }
        }

        public override void OnStartPage(PdfWriter writer, Document document)
        {
            base.OnStartPage(writer, document);
            pagina++;
            Image png = null;

            if (codSocieta == "8")
            {
                // Rai Com Spa
                _imgPath = System.Web.HttpContext.Current.Server.MapPath("~/assets/img/Rai Com_Logo.png");
                png = Image.GetInstance(_imgPath);
            }
            else if (codSocieta == "B")
            {
                // Rai Way
                _imgPath = System.Web.HttpContext.Current.Server.MapPath("~/assets/img/Rai Way_Logo.png");
                png = Image.GetInstance(_imgPath);
            }
            else if (codSocieta == "C")
            {
                // Rai Cinema
                _imgPath = System.Web.HttpContext.Current.Server.MapPath("~/assets/img/Rai Cinema_Logo.png");
                png = Image.GetInstance(_imgPath);
            }
            else
            {
                // Rai
                _imgPath = System.Web.HttpContext.Current.Server.MapPath("~/assets/img/LogoPDF.png");
                png = Image.GetInstance(_imgPath);
            }

            if (pagina > 2)
            {
                if (codSocieta == "8")
                {
                    // Rai Com Spa
                    png.ScaleAbsolute(120, 45);
                }
                else if (codSocieta == "B")
                {
                    // Rai Way
                    png.ScaleAbsolute(120, 45);
                }
                else if (codSocieta == "C")
                {
                    // Rai Cinema
                    png.ScaleAbsolute(150, 45);
                }
                else
                {
                    // Rai
                    png.ScaleAbsolute(20, 20);
                }
                png.SetAbsolutePosition(25, 800);

            }
            else
            {
                if (codSocieta == "8")
                {
                    // Rai Com Spa
                    png.ScaleAbsolute(120, 45);
                }
                else if (codSocieta == "B")
                {
                    // Rai Way
                    png.ScaleAbsolute(120, 45);
                }
                else if (codSocieta == "C")
                {
                    // Rai Cinema
                    png.ScaleAbsolute(150, 45);
                }
                else
                {
                    // Rai
                    png.ScaleAbsolute(45, 45);
                }
                png.SetAbsolutePosition(25, 750);
            }
            PdfContentByte cb = writer.DirectContent;
            cb.AddImage(png);
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            const int fontCorpoFirma = 7;
            base.OnEndPage(writer, document);
            BaseColor colorFirma = new BaseColor(System.Drawing.Color.Blue);
            Font myFontFirma = FontFactory.GetFont("Times-Roman", fontCorpoFirma, Font.NORMAL, colorFirma);
            Font myFontFirmaBold = FontFactory.GetFont("Times-Roman", fontCorpoFirma, Font.BOLD, colorFirma);

            PdfPTable tableFooter = new PdfPTable(4);
            tableFooter.DefaultCell.BorderWidth = 0;
            tableFooter.TotalWidth = 550;
            tableFooter.LockedWidth = true;
            var tableFooterWidth = new int[] { 137, 137, 137, 137 };
            tableFooter.SetWidths(tableFooterWidth);

            tableFooter.AddCell(WriteCellsClass.WriteCell("Rai - RadioTelevisione Italiana Spa", 0, 4, 0, myFontFirmaBold, 0));
            tableFooter.AddCell(WriteCellsClass.WriteCell("Sede legale Viale Mazzini, 14 - 00195 Roma", 0, 4, 0, myFontFirma, 0));
            //tableFooter.AddCell(WriteCellsClass.WriteCell("www.rai.it", 0, 4, 0, myFontFirmaBold, 0));
            tableFooter.AddCell(WriteCellsClass.WriteCell("Cap. Soc. Euro 242.518.100,00 Interamente versato", 0, 4, 0, myFontFirma, 0));
            tableFooter.AddCell(WriteCellsClass.WriteCell("Ufficio del Registro delle Imprese di Roma", 0, 4, 0, myFontFirma, 0));
            tableFooter.AddCell(WriteCellsClass.WriteCell("© RAI 2015 - tutti i diritti riservati. P.Iva 06382641006", 0, 4, 0, myFontFirma, 0));
            tableFooter.WriteSelectedRows(0, -1, 60, document.Bottom, writer.DirectContent);
        }

        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            base.OnCloseDocument(writer, document);
        }
    }

    public class AssunzioniManager
    {
        #region PUBLIC TD

        public static byte[] GeneraPdf_Bozza_Impiegato_TD(AssunzioniVM assunzione)
        {
            try
            {
                //var cultureInfo = CultureInfo.GetCultureInfo("it-IT");

                //byte[] bytes = null;
                //const int fontIntestazione = 12;
                //const int fontCorpo = 12;
                //const string FONTNAME = "Calibri"; //"Times-Roman"
                //const string SPAZIO = " ";

                //BaseColor color = new BaseColor(System.Drawing.Color.Black);
                //Font myFontIntestazione = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.NORMAL, color);
                //Font myFontIntestazioneBold = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.BOLD, color);
                //Font myFontIntestazioneBoldItalic = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.BOLDITALIC, color);
                //Font myFontCorpo = FontFactory.GetFont(FONTNAME, fontCorpo, Font.NORMAL, color);
                //Font myFontCorpoBold = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.BOLD, color);
                //Font myFontCorpoSottolineato = FontFactory.GetFont(FONTNAME, fontCorpo, Font.UNDERLINE, color);
                //Font myFontCorpoItalic = FontFactory.GetFont(FONTNAME, fontCorpo, Font.ITALIC, color);
                //Font myFontInterlinea = FontFactory.GetFont(FONTNAME, 7, Font.NORMAL, color);

                IncentiviEntities db = new IncentiviEntities();
                byte[] bytes = null;

                string sesso = assunzione.Genere.GetValueOrDefault().ToString();
                string nominativo = String.Format("{0} {1}", assunzione.Cognome.Trim(), assunzione.Nome.Trim());
                string mansione = GetRuoloByCode(assunzione.SelectedMansione);
                string categoria = GetCategoriaByCode(assunzione.SelectedCategoria);
                string ruolo = GetRuoloByCategoria(assunzione.SelectedCategoria);
                string sede = GetSedeByCode(assunzione.SelectedSede);
                string direzione = GetDirezioneByCode(assunzione.SelectedServizio);
                string stipendio = GetStipendio(assunzione.SelectedCategoria, assunzione.SelectedTipoMinimo);

                DateTime fineFormazione = DateTime.Today;

                var xx = db.XR_HRIS_TEMPLATE.Where(x => x.NME_TEMPLATE == "BOZZA IMPIEGATO TD").FirstOrDefault();
                if (xx != null)
                {
                    Stream stream = new MemoryStream(xx.TEMPLATE);
                    DocX doc = DocX.Load(stream);
                    doc.ReplaceText("#GENERE#", sesso.ToUpper().Equals("F") ? "Sig.ra" : "Sig");
                    doc.ReplaceText("#DATAODIERNA#", DateTime.Today.ToString("dd/MM/yyyy"));
                    doc.ReplaceText("#NOMINATIVO#", nominativo);
                    doc.ReplaceText("#DATAINIZIO#", assunzione.DataInizio.ToString("dd/MM/yyyy"));
                    if (assunzione.DataFine.HasValue)
                        doc.ReplaceText("#DATAFINE#", assunzione.DataFine.Value.ToString("dd/MM/yyyy"));
                    doc.ReplaceText("#MANSIONE#", mansione);
                    doc.ReplaceText("#CATEGORIA#", categoria);
                    doc.ReplaceText("#DIREZIONE#", direzione);
                    doc.ReplaceText("#SEDE#", sede);
                    doc.ReplaceText("#STIPENDIOCIFRA#", stipendio);
                    doc.ReplaceText("#STIPENDIO#", stipendio);

                    MemoryStream ms = new MemoryStream();
                    doc.SaveAs(ms);

                    bytes = ms.GetBuffer();
                }

                return bytes;

                //using (MemoryStream ms = new MemoryStream())
                //{
                //    string _imgPath = System.Web.HttpContext.Current.Server.MapPath("~/assets/img/LogoPDF.png");
                //    iTextSharp.text.Image png = iTextSharp.text.Image.GetInstance(_imgPath);
                //    string codSocieta = "";

                //    // verifica la società di appartenenza per caricare il logo adatto
                //    using (IncentiviEntities sdb = new IncentiviEntities())
                //    {
                //        var item = sdb.SINTESI1.Where(w => w.COD_MATLIBROMAT.Equals(assunzione.Matricola)).FirstOrDefault();

                //        if (item != null)
                //        {
                //            codSocieta = item.COD_IMPRESACR;
                //        }
                //    }

                //    // Creazione dell'istanza del documento, impostando tipo di pagina e margini
                //    Document document = new Document(PageSize.A4, 60, 60, 60, 80);
                //    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                //    writer.PageEvent = new AssunzioniManagerPdfPageEventHelper(codSocieta);
                //    document.Open();
                //    PdfContentByte cb = writer.DirectContent;
                //    Paragraph p = null;

                //    Phrase phrase = new Phrase();
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    //phrase.Add(new Chunk("RUO/GSR/6340/22/", myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();

                //    if (sesso.ToUpper().Equals("F"))
                //    {
                //        phrase.Add(new Chunk("Sig.ra ", myFontCorpo));
                //    }
                //    else
                //    {
                //        phrase.Add(new Chunk("Sig. ", myFontCorpo));
                //    }

                //    phrase.Add(new Chunk(nominativo, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_RIGHT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk("ROMA", myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_RIGHT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk("Roma, " + DateTime.Today.ToString("dd/MM/yyyy"), myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk("Assunzione a tempo determinato", myFontCorpoSottolineato));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par1 = String.Format("Le comunichiamo che Ella viene assunta, ai sensi dell’art. 1, comma 2, " +
                //        "del decreto legge 12 luglio 2018, n. 87, convertito con modificazioni dalla legge 9 agosto " +
                //        "2018, n. 96, con contratto di lavoro subordinato a tempo determinato dal {0} al {1} (u.g.s.), " +
                //        "in qualità di {2} con inquadramento al livello {3}, presso la Direzione {4}, con sede " +
                //        "di lavoro in {5}.", 
                //        assunzione.DataInizio.ToString("dd/MM/yyyy"), 
                //        assunzione.DataFine.GetValueOrDefault().ToString("dd/MM/yyyy"),
                //        mansione,
                //        categoria,
                //        direzione,
                //        sede);
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par1, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par2 = String.Format("Il Suo rapporto è regolato dal vigente Contratto Collettivo di Lavoro " +
                //        "per quadri, impiegati ed operai dipendenti dalla RAI e dalle Aziende del Gruppo nella parte in " +
                //        "cui esso e’ applicabile ai sensi del combinato disposto degli artt. 3 “Ambito di applicazione " +
                //        "del Contratto” e 8 “Rapporto di lavoro a termine”, contratto del quale Ella ha preso visione, " +
                //        "che dichiara di accettare integralmente ed al quale espressamente si rinvia in ordine ad orario " +
                //        "di lavoro, ferie ed a quant’altro non previsto nella presente lettera per quanto concerne il " +
                //        "trattamento economico e normativo a Lei applicabile.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par2, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par3 = String.Format("La Sua assunzione a  termine è subordinata ad un periodo di prova di 20 " +
                //        "giorni durante il quale è in facoltà  delle parti  risolvere il rapporto di lavoro senza obbligo " +
                //        "di preavviso ne' corresponsione di alcuna indennità.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par3, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par4 = String.Format("Le verrà corrisposto uno stipendio di Euro 1.108,58=(millecentotto/58) " +
                //        "lordi mensili nonché l’indennità di contingenza nella misura prevista dalle vigenti " +
                //        "disposizioni.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par4, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par5 = String.Format("La retribuzione prevista in contratto costituisce esclusivamente la " +
                //        "remunerazione concordata per la Sua prestazione lavorativa. Con specifico riferimento alla " +
                //        "Normativa in materia di anticorruzione, Lei dichiara e garantisce che, nello svolgimento della " +
                //        "Sua prestazione lavorativa, non terrà, direttamente o indirettamente, comportamenti, omissivi " +
                //        "e/o commissivi, che possano comportare la violazione della normativa anticorruzione " +
                //        "(per quanto applicabile a RAI medesima) e/o del Piano Triennale di Prevenzione della Corruzione " +
                //        "(e protocolli nello stesso previsti) adottato da RAI ai sensi della legge n.190/2012, di cui " +
                //        "è tenuto ad acquisire conoscenza sul sito intranet della RAI (http://www.raiplace.rai.it), " +
                //        "nell’area tematica “Norme e Procedure” – “Governance, Controllo e Conformità” – “Anticorruzione”.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par5, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par6 = String.Format("Ella prende atto che l`Azienda non è tenuta a riconoscere e non " +
                //        "riconoscerà la rinnovazione o la proroga del presente contratto che non sia stata formalizzata " +
                //        "per iscritto e sottoscritta da un procuratore dell`azienda munito dei relativi poteri.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par6, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par7 = String.Format("Ai sensi dell’art. 13 del Regolamento UE 2016/679 (Regolamento Generale " +
                //        "sulla protezione dei dati personali), La informiamo che i dati personali da Lei forniti verranno " +
                //        "utilizzati da Rai-Radiotelevisione italiana Spa (titolare del trattamento) – anche tramite " +
                //        "collaboratori esterni - con modalità manuali, informatiche e telematiche, a mezzo inserimento " +
                //        "in banche dati gestite dalla Azienda Rai medesima.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par7, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par8 = String.Format("In particolare, unitamente alla stipula del presente contratto, " +
                //        "Le viene consegnata l’informativa di cui al richiamato art. 13 Regolamento UE 2016/679, " +
                //        "che ne costituisce parte integrante. Ella dovrà aver cura di leggerla attentamente e di " +
                //        "restituircela debitamente datata e sottoscritta per presa visione. Ad ogni buon conto, sul " +
                //        "sito intranet della Rai (http://www.raiplace.rai.it, nell’area tematica " +
                //        "“Norme e Procedure” – “Governance, Controllo e Conformità” – “Privacy”), troverà comunque " +
                //        "pubblicato il testo dell’informativa in questione.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par8, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par9 = String.Format("Ella, preso  atto dei Principi etici  generali di onestà e osservanza " +
                //        "della legge, pluralismo, professionalità,  imparzialità,  correttezza, riservatezza, trasparenza, " +
                //        "diligenza, lealtà e buona  fede nonché  del contenuto  tutto del  Codice etico del  Gruppo  " +
                //        "RAI - che  dichiara di conoscere  globalmente  e  nelle  sue  singole  parti, avendone  presa " +
                //        "completa e piena  visione su base cartacea e/o attraverso collegamento telematico al sito " +
                //        "intranet della Rai (http://www.raiplace.rai.it, nell’area tematica “Norme e Procedure” – " +
                //        "“Governance, Controllo e Conformità” – “Codice Etico”)  - si  impegna, per tutta  la durata  " +
                //        "del presente  contratto, ad  attenersi al Codice stesso, osservando un comportamento ad esso " +
                //        "pienamente conforme e che non  risulti lesivo  dell'immagine e, comunque, dei  valori morali " +
                //        "e materiali   in   cui  il   Gruppo  RAI  si  riconosce  e  che  applica nell'esercizio  della  " +
                //        "propria  attività,  anche  con  riferimento  ai rapporti con i terzi.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par9, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par10 = String.Format("Ella, inoltre, espressamente dichiara di aver ricevuto copia cartacea " +
                //        "e di aver recepito i contenuti del Manuale informativo per la Sicurezza, nella versione " +
                //        "predisposta dalla RAI medesima in relazione alle sue specificità. Tale documento, anche in " +
                //        "riferimento ad eventuali possibili aggiornamenti, è altresì disponibile in formato elettronico " +
                //        "attraverso collegamento telematico al sito intranet http://www.raiplace.rai.it, nell’area " +
                //        "tematica “Norme e Procedure” – “Salute, Sicurezza e Ambiente – Tutela delle persone”.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par10, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par11 = String.Format("Ella si impegna, ove tale adempimento non sia da Lei già stato " +
                //        "documentatamente effettuato, a fruire, all'atto della Sua immissione in servizio e " +
                //        "preventivamente all'inizio della sua mansione lavorativa, del Corso Multimediale sulla " +
                //        "sicurezza nei luoghi di lavoro. Tale corso, della durata indicativa di 6 ore, ha l'obiettivo " +
                //        "di erogare e certificare, previa verifica finale dell'apprendimento, la formazione di base " +
                //        "sui rischi specifici connessi al Suo profilo professionale. La fruizione del corso è " +
                //        "possibile da qualsiasi postazione di lavoro informatica connessa alla rete aziendale, " +
                //        "sul sito intranet http://www.raiplace.rai.it, nell’area tematica “RAI ACADEMY” – “Catalogo " +
                //        "Corsi”.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par11, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par12 = String.Format("Ella, altresì, espressamente  dichiara  di conoscere - avendone " +
                //        "presa completa e piena  visione su base cartacea e/o attraverso collegamento telematico al " +
                //        "sito intranet della Rai (http://www.raiplace.rai.it, nell’area tematica “Norme e Procedure” " +
                //        "– “Governance, Controllo e Conformità”) il Modello di Organizzazione, Gestione e Controllo " +
                //        "RAI ex d.lgs. 231/2001 sulla responsabilità  amministrativa da reato degli enti - nella " +
                //        "versione  messa a disposizione dalla Rai  medesima  -  e  si  impegna  a  tenere,  nell' " +
                //        "esecuzione  delle prestazioni, comportamenti in linea con i principi contenuti in detto " +
                //        "Modello Organizzativo \"231\".");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par12, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par13 = String.Format("Con riferimento al diritto di precedenza riconosciuto dalla legge " +
                //        "nelle assunzioni a tempo indeterminato (o a tempo determinato, in caso di lavoratrici che " +
                //        "abbiano fruito del congedo di maternità) si rinvia integralmente a quanto previsto dall’art. " +
                //        "24 del decreto legislativo 15.06.2015, n.81.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par13, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par14 = String.Format("Ad ogni effetto di legge, Le comunichiamo che abbiamo inserito il " +
                //        "Suo nominativo  nel  Libro Unico  del  Lavoro di cui all'art. 39 del  D.L. 25 giugno 2008, " +
                //        "n. 112, convertito  nella L. n. 133/2008 e che, con la consegna di copia del presente " +
                //        "contratto, l'azienda intende  assolvere agli obblighi comunicativi di cui al D.Lgs 26 maggio " +
                //        "1997, n. 152. ");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par14, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par15 = String.Format("Con la sottoscrizione del presente  contratto, Lei si impegna " +
                //        "infine a compilare e restituire completi in tutte le loro parti alle competenti strutture " +
                //        "di  gestione del Personale i formulari di  dichiarazione che Le saranno sottoposti al fine " +
                //        "di acquisire le informazioni  essenziali per la corretta  gestione del Suo rapporto di lavoro, " +
                //        "quali, a titolo meramente  esemplificativo e  non  esaustivo, la dichiarazione  sulla spettanza " +
                //        "delle detrazioni  d'imposta, la dichiarazione circa l'anzianità assicurativa e contributiva " +
                //        "posseduta, la modulistica ai fini della scelta circa la destinazione del TFR (ove necessaria).");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par15, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par16 = String.Format("Nel restituire copia della presente da Lei firmata in segno di " +
                //        "integrale accettazione, La invitiamo a consegnare i seguenti documenti:");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par16, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    PdfPTable tableDetail = new PdfPTable(2);
                //    tableDetail.DefaultCell.BorderWidth = 0;
                //    tableDetail.TotalWidth = 550;
                //    tableDetail.LockedWidth = true;
                //    var tableDetailWidth = new int[] { 50, 500 };
                //    tableDetail.SetWidths(tableDetailWidth);

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    Phrase cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("eventuale libretto di iscrizione alla Gestione ex ENPALS dell’INPS ove " +
                //        "da lei posseduto in relazione a precedenti rapporti di lavoro nello spettacolo;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("certificato di nascita;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("stato di famiglia con eventuale autorizzazione per assegni familiari;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("gli altri documenti eventualmente necessari per perfezionare " +
                //        "l’assunzione di cui sopra ivi inclusi se del caso quelli utili al godimento delle " +
                //        "agevolazioni normative e/o contributive di legge.", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    document.Add(tableDetail);
                //    tableDetail.FlushContent();
                //    tableDetail.DeleteBodyRows();

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par17 = String.Format("Cordiali Saluti");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par17, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par18 = String.Format("Il sottoscritto da' e prende atto consapevolmente, e ad ogni effetto " +
                //        "della vigente normativa, che il rapporto di lavoro costituito ai sensi di quanto sopra " +
                //        "convenuto, avrà termine il {0} e che, alla fine dell`orario di lavoro di detto giorno " +
                //        "cesseranno le prestazioni lavorative del sottoscritto senza bisogno di ulteriore o diversa " +
                //        "intimazione e senza necessità di preavviso, essendo detti intimazione e preavviso già " +
                //        "contenuti nella pattuita fissazione del termine delle prestazioni del sottoscritto alla " +
                //        "data del {1}."
                //        , assunzione.DataFine.GetValueOrDefault().ToString("dd/MM/yyyy")
                //        , assunzione.DataFine.GetValueOrDefault().ToString("dd/MM/yyyy"));
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par18, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    PdfPTable tableFirma = new PdfPTable(4);
                //    tableFirma.DefaultCell.BorderWidth = 0;
                //    tableFirma.TotalWidth = 550;
                //    tableFirma.LockedWidth = true;
                //    var tableFirmaWidth = new int[] { 137, 137, 137, 137 };
                //    tableFirma.SetWidths(tableFirmaWidth);
                //    tableFirma.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 1, myFontCorpo, 0));
                //    tableFirma.AddCell(WriteCellsClass.WriteCell("FIRMA", 0, 2, 1, myFontCorpo, 0));

                //    document.Add(tableFirma);
                //    tableFirma.FlushContent();
                //    tableFirma.DeleteBodyRows();

                //    document.Close();
                //    writer.Close();
                //    bytes = ms.ToArray();
                //    return bytes;
                //}
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static byte[] GeneraPdf_Bozza_Orchestrale_TD(AssunzioniVM assunzione)
        {
            try
            {



                IncentiviEntities db = new IncentiviEntities();
                byte[] bytes = null;

                string sesso = assunzione.Genere.GetValueOrDefault().ToString();
                string nominativo = String.Format("{0} {1}", assunzione.Cognome.Trim(), assunzione.Nome.Trim());
                string mansione = GetRuoloByCode(assunzione.SelectedMansione);
                string categoria = GetCategoriaByCode(assunzione.SelectedCategoria);
                string ruolo = GetRuoloByCategoria(assunzione.SelectedCategoria);
                string sede = GetSedeByCode(assunzione.SelectedSede);
                string direzione = GetDirezioneByCode(assunzione.SelectedServizio);
                string stipendio = GetStipendio(assunzione.SelectedCategoria, assunzione.SelectedTipoMinimo);

                DateTime fineFormazione = DateTime.Today;

                var xx = db.XR_HRIS_TEMPLATE.Where(x => x.NME_TEMPLATE == "BOZZA ORCHESTRALE TD").FirstOrDefault();
                if (xx != null)
                {
                    Stream stream = new MemoryStream(xx.TEMPLATE);
                    DocX doc = DocX.Load(stream);
                    doc.ReplaceText("#GENERE#", sesso.ToUpper().Equals("F") ? "Sig.ra" : "Sig");
                    doc.ReplaceText("#DATAODIERNA#", DateTime.Today.ToString("dd/MM/yyyy"));
                    doc.ReplaceText("#NOMINATIVO#", nominativo);
                    doc.ReplaceText("#DATAINIZIO#", assunzione.DataInizio.ToString("dd/MM/yyyy"));
                    if (assunzione.DataFine.HasValue)
                        doc.ReplaceText("#DATAFINE#", assunzione.DataFine.Value.ToString("dd/MM/yyyy"));
                    doc.ReplaceText("#MANSIONE#", mansione);
                    doc.ReplaceText("#CATEGORIA#", categoria);
                    doc.ReplaceText("#DIREZIONE#", direzione);
                    doc.ReplaceText("#SEDE#", sede);
                    doc.ReplaceText("#STIPENDIOCIFRA#", stipendio);
                    doc.ReplaceText("#STIPENDIO#", stipendio);
                    MemoryStream ms = new MemoryStream();
                    doc.SaveAs(ms);

                    bytes = ms.GetBuffer();
                }

                return bytes;
                //var cultureInfo = CultureInfo.GetCultureInfo("it-IT");

                //byte[] bytes = null;
                //const int fontIntestazione = 12;
                //const int fontCorpo = 12;
                //const string FONTNAME = "Calibri"; //"Times-Roman"
                //const string SPAZIO = " ";

                //BaseColor color = new BaseColor(System.Drawing.Color.Black);
                //Font myFontIntestazione = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.NORMAL, color);
                //Font myFontIntestazioneBold = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.BOLD, color);
                //Font myFontIntestazioneBoldItalic = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.BOLDITALIC, color);
                //Font myFontCorpo = FontFactory.GetFont(FONTNAME, fontCorpo, Font.NORMAL, color);
                //Font myFontCorpoBold = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.BOLD, color);
                //Font myFontCorpoSottolineato = FontFactory.GetFont(FONTNAME, fontCorpo, Font.UNDERLINE, color);
                //Font myFontCorpoItalic = FontFactory.GetFont(FONTNAME, fontCorpo, Font.ITALIC, color);
                //Font myFontInterlinea = FontFactory.GetFont(FONTNAME, 7, Font.NORMAL, color);

                //string sesso = assunzione.Genere.GetValueOrDefault().ToString();
                //string nominativo = String.Format("{0} {1}", assunzione.Cognome.Trim(), assunzione.Nome.Trim());
                //string mansione = GetRuoloByCode(assunzione.SelectedMansione);
                //string categoria = GetCategoriaByCode(assunzione.SelectedCategoria);
                //string ruolo = GetRuoloByCategoria(assunzione.SelectedCategoria);
                //string sede = GetSedeByCode(assunzione.SelectedSede);
                //string direzione = GetDirezioneByCode(assunzione.SelectedServizio);

                //using (MemoryStream ms = new MemoryStream())
                //{
                //    string _imgPath = System.Web.HttpContext.Current.Server.MapPath("~/assets/img/LogoPDF.png");
                //    iTextSharp.text.Image png = iTextSharp.text.Image.GetInstance(_imgPath);
                //    string codSocieta = "";

                //    // verifica la società di appartenenza per caricare il logo adatto
                //    using (IncentiviEntities sdb = new IncentiviEntities())
                //    {
                //        var item = sdb.SINTESI1.Where(w => w.COD_MATLIBROMAT.Equals(assunzione.Matricola)).FirstOrDefault();

                //        if (item != null)
                //        {
                //            codSocieta = item.COD_IMPRESACR;
                //        }
                //    }

                //    // Creazione dell'istanza del documento, impostando tipo di pagina e margini
                //    Document document = new Document(PageSize.A4, 60, 60, 60, 80);
                //    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                //    writer.PageEvent = new AssunzioniManagerPdfPageEventHelper(codSocieta);
                //    document.Open();
                //    PdfContentByte cb = writer.DirectContent;
                //    Paragraph p = null;

                //    Phrase phrase = new Phrase();
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    //phrase.Add(new Chunk("RUO/GSR/6340/22/", myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();

                //    if (sesso.ToUpper().Equals("F"))
                //    {
                //        phrase.Add(new Chunk("Sig.ra ", myFontCorpo));
                //    }
                //    else
                //    {
                //        phrase.Add(new Chunk("Sig. ", myFontCorpo));
                //    }

                //    phrase.Add(new Chunk(nominativo, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_RIGHT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk("ROMA", myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_RIGHT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk("Roma, " + DateTime.Today.ToString("dd/MM/yyyy"), myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk("Assunzione a tempo determinato", myFontCorpoSottolineato));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par1 = String.Format("Le comunichiamo che Ella viene assunta con contratto di lavoro " +
                //        "subordinato a tempo determinato dal {0} al {1} (u.g.s.), in qualità di {2} con " +
                //        "inquadramento in categoria contrattuale {3} e ruolo di {4} presso " +
                //        "l’Orchestra Sinfonica Nazionale con sede di lavoro in {5}.",
                //        assunzione.DataInizio.ToString("dd/MM/yyyy"),
                //        assunzione.DataFine.GetValueOrDefault().ToString("dd/MM/yyyy"),
                //        mansione,
                //        categoria,
                //        ruolo,
                //        sede);
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par1, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par2 = String.Format("La Sua assunzione viene effettuata ai sensi degli artt. 19 e ss. " +
                //        "del decreto legislativo 15 giugno 2015, n. 81 e s.m.i. ed in attuazione dell’accordo " +
                //        "sindacale in materia di lavoro stagionale sottoscritto in data 24 ottobre 2018 tra l’Azienda " +
                //        "e le Organizzazioni Sindacali.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par2, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par3 = String.Format("Il Suo rapporto e’ regolato dal vigente Contratto Collettivo di " +
                //        "Lavoro per i professori d’orchestra della RAI, contratto del quale Ella ha preso visione " +
                //        "e che dichiara di accettare integralmente, nonché dalle disposizioni di servizio impartite " +
                //        "da questa società. ");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par3, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par4 = String.Format("Le verrà corrisposto uno stipendio di Euro 1.415,38 " +
                //        "(millequattrocentoquindici/trentotto) lordi mensili, una indennità rimborso spese " +
                //        "professionali di Euro 62,00=(sessantadue), l’indennità di contingenza nella misura " +
                //        "prevista dalle vigenti disposizioni, ed inoltre, ai sensi e per i fini del sesto comma " +
                //        "dell’art. 3 del citato Contratto Collettivo, una speciale indennità mensile commisurata " +
                //        "al 40% dello stipendio e della contingenza.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par4, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par5 = String.Format("La retribuzione prevista in contratto costituisce esclusivamente " +
                //        "la remunerazione concordata per la Sua prestazione lavorativa. Con specifico riferimento " +
                //        "alla Normativa in materia di anticorruzione, Lei dichiara e garantisce che, nello " +
                //        "svolgimento della Sua prestazione lavorativa, non terrà, direttamente o indirettamente, " +
                //        "comportamenti, omissivi e/o commissivi, che possano comportare la violazione della " +
                //        "normativa anticorruzione (per quanto applicabile a RAI medesima) e/o del Piano Triennale " +
                //        "di Prevenzione della Corruzione (e protocolli nello stesso previsti) adottato da RAI " +
                //        "ai sensi della legge n.190/2012, di cui è tenuto ad acquisire conoscenza sul sito intranet " +
                //        "della RAI (http://www.raiplace.rai.it), nell’area tematica “Norme e Procedure” – " +
                //        "“Governance, Controllo e Conformità” – “Anticorruzione”.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par5, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par6 = String.Format("Ella prende atto che l`Azienda non è tenuta a riconoscere e non " +
                //        "riconoscerà la rinnovazione o la proroga del presente contratto che non sia stata " +
                //        "formalizzata per iscritto e sottoscritta da un procuratore dell`azienda munito dei " +
                //        "relativi poteri.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par6, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par7 = String.Format("Ai sensi dell’art. 13 del Regolamento UE 2016/679 " +
                //        "(Regolamento Generale sulla protezione dei dati personali), La informiamo che i " +
                //        "dati personali da Lei forniti verranno utilizzati da Rai-Radiotelevisione italiana Spa " +
                //        "(titolare del trattamento) – anche tramite collaboratori esterni - con modalità manuali, " +
                //        "informatiche e telematiche, a mezzo inserimento in banche dati gestite dalla Azienda " +
                //        "Rai medesima.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par7, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par8 = String.Format("In particolare, unitamente alla stipula del presente contratto, " +
                //        "Le viene consegnata l’informativa di cui al richiamato art. 13 Regolamento UE 2016/679, " +
                //        "che ne costituisce parte integrante. Ella dovrà aver cura di leggerla attentamente e di " +
                //        "restituircela debitamente datata e sottoscritta per presa visione. Ad ogni buon conto, sul " +
                //        "sito intranet della Rai (http://www.raiplace.rai.it, nell’area tematica " +
                //        "“Norme e Procedure” – “Governance, Controllo e Conformità” – “Privacy”), troverà comunque " +
                //        "pubblicato il testo dell’informativa in questione.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par8, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par9 = String.Format("Ella, preso  atto dei Principi etici  generali di onestà e osservanza " +
                //        "della legge, pluralismo, professionalità,  imparzialità,  correttezza, riservatezza, trasparenza, " +
                //        "diligenza, lealtà e buona fede nonché  del contenuto  tutto del  Codice etico del  Gruppo  " +
                //        "RAI - che  dichiara di conoscere  globalmente  e  nelle  sue  singole  parti, avendone  presa " +
                //        "completa e piena  visione su base cartacea e/o attraverso collegamento telematico al sito " +
                //        "intranet della Rai (http://www.raiplace.rai.it, nell’area tematica “Norme e Procedure” – " +
                //        "“Governance, Controllo e Conformità” – “Codice Etico”)  - si  impegna, per tutta  la durata  " +
                //        "del presente  contratto, ad  attenersi al Codice stesso, osservando un comportamento ad esso " +
                //        "pienamente conforme e che non  risulti lesivo  dell'immagine e, comunque, dei  valori morali " +
                //        "e materiali in cui il Gruppo RAI si riconosce e che applica nell'esercizio della " +
                //        "propria attività, anche con riferimento ai rapporti con i terzi.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par9, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par10 = String.Format("Ella, inoltre, espressamente dichiara di aver ricevuto copia cartacea " +
                //        "e di aver recepito i contenuti del Manuale informativo per la Sicurezza, nella versione " +
                //        "predisposta dalla RAI medesima in relazione alle sue specificità. Tale documento, anche in " +
                //        "riferimento ad eventuali possibili aggiornamenti, è altresì disponibile in formato elettronico " +
                //        "attraverso collegamento telematico al sito intranet http://www.raiplace.rai.it, nell’area " +
                //        "tematica “Norme e Procedure” – “Salute, Sicurezza e Ambiente – Tutela delle persone”.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par10, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par11 = String.Format("Ella si impegna, ove tale adempimento non sia da Lei già stato " +
                //        "documentatamente effettuato, a fruire, all'atto della Sua immissione in servizio e " +
                //        "preventivamente all'inizio della sua mansione lavorativa, del Corso Multimediale sulla " +
                //        "sicurezza nei luoghi di lavoro. Tale corso, della durata indicativa di 6 ore, ha l'obiettivo " +
                //        "di erogare e certificare, previa verifica finale dell'apprendimento, la formazione di base " +
                //        "sui rischi specifici connessi al Suo profilo professionale. La fruizione del corso è " +
                //        "possibile da qualsiasi postazione di lavoro informatica connessa alla rete aziendale, " +
                //        "sul sito intranet http://www.raiplace.rai.it, nell’area tematica “RAI ACADEMY” – “Catalogo " +
                //        "Corsi”.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par11, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par12 = String.Format("Ella, altresì, espressamente  dichiara  di conoscere - avendone " +
                //        "presa completa e piena  visione su base cartacea e/o attraverso collegamento telematico al " +
                //        "sito intranet della Rai (http://www.raiplace.rai.it, nell’area tematica “Norme e Procedure” " +
                //        "– “Governance, Controllo e Conformità”) il Modello di Organizzazione, Gestione e Controllo " +
                //        "RAI ex d.lgs. 231/2001 sulla responsabilità  amministrativa da reato degli enti - nella " +
                //        "versione  messa a disposizione dalla Rai medesima - e si impegna a tenere, nell' " +
                //        "esecuzione delle prestazioni, comportamenti in linea con i principi contenuti in detto " +
                //        "Modello Organizzativo \"231\".");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par12, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par13 = String.Format("Con riferimento al diritto di precedenza riconosciuto dalla legge " +
                //        "nelle assunzioni a tempo indeterminato (o a tempo determinato, in caso di lavoratrici che " +
                //        "abbiano fruito del congedo di maternità) si rinvia integralmente a quanto previsto dall’art. " +
                //        "24 del decreto legislativo 15.06.2015, n.81.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par13, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par14 = String.Format("Ad ogni effetto di legge, Le comunichiamo che abbiamo inserito il " +
                //        "Suo nominativo  nel  Libro Unico  del  Lavoro di cui all'art. 39 del  D.L. 25 giugno 2008, " +
                //        "n. 112, convertito  nella L. n. 133/2008 e che, con la consegna di copia del presente " +
                //        "contratto, l'azienda intende  assolvere agli obblighi comunicativi di cui al D.Lgs 26 maggio " +
                //        "1997, n. 152. ");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par14, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par15 = String.Format("Con la sottoscrizione del presente contratto, Lei si impegna " +
                //        "infine a compilare e restituire completi in tutte le loro parti alle competenti strutture " +
                //        "di gestione del Personale i formulari di dichiarazione che Le saranno sottoposti al fine " +
                //        "di acquisire le informazioni essenziali per la corretta gestione del Suo rapporto di lavoro, " +
                //        "quali, a titolo meramente esemplificativo e non esaustivo, la dichiarazione sulla spettanza " +
                //        "delle detrazioni d'imposta, la dichiarazione circa l'anzianità assicurativa e contributiva " +
                //        "posseduta, la modulistica ai fini della scelta circa la destinazione del TFR (ove necessaria).");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par15, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par16 = String.Format("Nel restituire copia della presente da Lei firmata in segno di " +
                //        "integrale accettazione, La invitiamo a consegnare i seguenti documenti:");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par16, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    PdfPTable tableDetail = new PdfPTable(2);
                //    tableDetail.DefaultCell.BorderWidth = 0;
                //    tableDetail.TotalWidth = 550;
                //    tableDetail.LockedWidth = true;
                //    var tableDetailWidth = new int[] { 50, 500 };
                //    tableDetail.SetWidths(tableDetailWidth);

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    Phrase cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("eventuale libretto di iscrizione alla Gestione ex ENPALS dell’INPS ove " +
                //        "da lei posseduto in relazione a precedenti rapporti di lavoro nello spettacolo;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("certificato di nascita;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("stato di famiglia con eventuale autorizzazione per assegni familiari;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("gli altri documenti eventualmente necessari per perfezionare " +
                //        "l’assunzione di cui sopra ivi inclusi se del caso quelli utili al godimento delle " +
                //        "agevolazioni normative e/o contributive di legge.", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    document.Add(tableDetail);
                //    tableDetail.FlushContent();
                //    tableDetail.DeleteBodyRows();

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par17 = String.Format("Cordiali Saluti");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par17, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par18 = String.Format("Il sottoscritto da' e prende atto consapevolmente, e ad ogni effetto " +
                //        "della vigente normativa, che il rapporto di lavoro costituito ai sensi di quanto sopra " +
                //        "convenuto, avrà termine il {0} e che, alla fine dell`orario di lavoro di detto giorno " +
                //        "cesseranno le prestazioni lavorative del sottoscritto senza bisogno di ulteriore o diversa " +
                //        "intimazione e senza necessità di preavviso, essendo detti intimazione e preavviso già " +
                //        "contenuti nella pattuita fissazione del termine delle prestazioni del sottoscritto alla " +
                //        "data del {1}."
                //        , assunzione.DataFine.GetValueOrDefault().ToString("dd/MM/yyyy")
                //        , assunzione.DataFine.GetValueOrDefault().ToString("dd/MM/yyyy"));
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par18, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    PdfPTable tableFirma = new PdfPTable(4);
                //    tableFirma.DefaultCell.BorderWidth = 0;
                //    tableFirma.TotalWidth = 550;
                //    tableFirma.LockedWidth = true;
                //    var tableFirmaWidth = new int[] { 137, 137, 137, 137 };
                //    tableFirma.SetWidths(tableFirmaWidth);
                //    tableFirma.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 1, myFontCorpo, 0));
                //    tableFirma.AddCell(WriteCellsClass.WriteCell("FIRMA", 0, 2, 1, myFontCorpo, 0));

                //    document.Add(tableFirma);
                //    tableFirma.FlushContent();
                //    tableFirma.DeleteBodyRows();

                //    document.Close();
                //    writer.Close();
                //    bytes = ms.ToArray();
                //    return bytes;
                //}
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static byte[] GeneraPdf_Bozza_Orchestrale_TD_660(AssunzioniVM assunzione)
        {
            try
            {

                IncentiviEntities db = new IncentiviEntities();
                byte[] bytes = null;

                string sesso = assunzione.Genere.GetValueOrDefault().ToString();
                string nominativo = String.Format("{0} {1}", assunzione.Cognome.Trim(), assunzione.Nome.Trim());
                string mansione = GetRuoloByCode(assunzione.SelectedMansione);
                string categoria = GetCategoriaByCode(assunzione.SelectedCategoria);
                string ruolo = GetRuoloByCategoria(assunzione.SelectedCategoria);
                string sede = GetSedeByCode(assunzione.SelectedSede);
                string direzione = GetDirezioneByCode(assunzione.SelectedServizio);
                string stipendio = GetStipendio(assunzione.SelectedCategoria, assunzione.SelectedTipoMinimo);

                DateTime fineFormazione = DateTime.Today;

                var xx = db.XR_HRIS_TEMPLATE.Where(x => x.NME_TEMPLATE == "BOZZA ORCHESTRALE TD CON 6.60%").FirstOrDefault();
                if (xx != null)
                {
                    Stream stream = new MemoryStream(xx.TEMPLATE);
                    DocX doc = DocX.Load(stream);
                    doc.ReplaceText("#GENERE#", sesso.ToUpper().Equals("F") ? "Sig.ra" : "Sig");
                    doc.ReplaceText("#DATAODIERNA#", DateTime.Today.ToString("dd/MM/yyyy"));
                    doc.ReplaceText("#NOMINATIVO#", nominativo);
                    doc.ReplaceText("#DATAINIZIO#", assunzione.DataInizio.ToString("dd/MM/yyyy"));
                    if (assunzione.DataFine.HasValue)
                        doc.ReplaceText("#DATAFINE#", assunzione.DataFine.Value.ToString("dd/MM/yyyy"));
                    doc.ReplaceText("#MANSIONE#", mansione);
                    doc.ReplaceText("#CATEGORIA#", categoria);
                    doc.ReplaceText("#DIREZIONE#", direzione);
                    doc.ReplaceText("#SEDE#", sede);
                    doc.ReplaceText("#STIPENDIOCIFRA#", stipendio);
                    doc.ReplaceText("#STIPENDIO#", stipendio);
                    MemoryStream ms = new MemoryStream();
                    doc.SaveAs(ms);

                    bytes = ms.GetBuffer();
                }

                return bytes;
                //var cultureInfo = CultureInfo.GetCultureInfo("it-IT");

                //byte[] bytes = null;
                //const int fontIntestazione = 12;
                //const int fontCorpo = 12;
                //const string FONTNAME = "Calibri"; //"Times-Roman"
                //const string SPAZIO = " ";

                //BaseColor color = new BaseColor(System.Drawing.Color.Black);
                //Font myFontIntestazione = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.NORMAL, color);
                //Font myFontIntestazioneBold = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.BOLD, color);
                //Font myFontIntestazioneBoldItalic = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.BOLDITALIC, color);
                //Font myFontCorpo = FontFactory.GetFont(FONTNAME, fontCorpo, Font.NORMAL, color);
                //Font myFontCorpoBold = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.BOLD, color);
                //Font myFontCorpoSottolineato = FontFactory.GetFont(FONTNAME, fontCorpo, Font.UNDERLINE, color);
                //Font myFontCorpoItalic = FontFactory.GetFont(FONTNAME, fontCorpo, Font.ITALIC, color);
                //Font myFontInterlinea = FontFactory.GetFont(FONTNAME, 7, Font.NORMAL, color);

                //string sesso = assunzione.Genere.GetValueOrDefault().ToString();
                //string nominativo = String.Format("{0} {1}", assunzione.Cognome.Trim(), assunzione.Nome.Trim());
                //string mansione = GetRuoloByCode(assunzione.SelectedMansione);
                //string categoria = GetCategoriaByCode(assunzione.SelectedCategoria);
                //string ruolo = GetRuoloByCategoria(assunzione.SelectedCategoria);
                //string sede = GetSedeByCode(assunzione.SelectedSede);
                //string direzione = GetDirezioneByCode(assunzione.SelectedServizio);


                //using (MemoryStream ms = new MemoryStream())
                //{
                //    string _imgPath = System.Web.HttpContext.Current.Server.MapPath("~/assets/img/LogoPDF.png");
                //    iTextSharp.text.Image png = iTextSharp.text.Image.GetInstance(_imgPath);
                //    string codSocieta = "";

                //    // verifica la società di appartenenza per caricare il logo adatto
                //    using (IncentiviEntities sdb = new IncentiviEntities())
                //    {
                //        var item = sdb.SINTESI1.Where(w => w.COD_MATLIBROMAT.Equals(assunzione.Matricola)).FirstOrDefault();

                //        if (item != null)
                //        {
                //            codSocieta = item.COD_IMPRESACR;
                //        }
                //    }

                //    // Creazione dell'istanza del documento, impostando tipo di pagina e margini
                //    Document document = new Document(PageSize.A4, 60, 60, 60, 80);
                //    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                //    writer.PageEvent = new AssunzioniManagerPdfPageEventHelper(codSocieta);
                //    document.Open();
                //    PdfContentByte cb = writer.DirectContent;
                //    Paragraph p = null;

                //    Phrase phrase = new Phrase();
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    //phrase.Add(new Chunk("RUO/GSR/6340/22/", myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();

                //    if (sesso.ToUpper().Equals("F"))
                //    {
                //        phrase.Add(new Chunk("Sig.ra ", myFontCorpo));
                //    }
                //    else
                //    {
                //        phrase.Add(new Chunk("Sig. ", myFontCorpo));
                //    }

                //    phrase.Add(new Chunk(nominativo, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_RIGHT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk("ROMA", myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_RIGHT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk("Roma, " + DateTime.Today.ToString("dd/MM/yyyy"), myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk("Assunzione a tempo determinato", myFontCorpoSottolineato));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par1 = String.Format("Le comunichiamo che Ella viene assunta con contratto di lavoro " +
                //        "subordinato a tempo determinato dal {0} al {1} (u.g.s.), in qualità di {2} con " +
                //        "inquadramento in categoria contrattuale {3} e ruolo di {4} presso " +
                //        "l’Orchestra Sinfonica Nazionale con sede di lavoro in {5}.",
                //        assunzione.DataInizio.ToString("dd/MM/yyyy"),
                //        assunzione.DataFine.GetValueOrDefault().ToString("dd/MM/yyyy"),
                //        mansione,
                //        categoria,
                //        ruolo,
                //        sede);
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par1, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par2 = String.Format("La Sua assunzione viene effettuata ai sensi degli artt. 19 e ss. " +
                //        "del decreto legislativo 15 giugno 2015, n. 81 e s.m.i. ed in attuazione dell’accordo " +
                //        "sindacale in materia di lavoro stagionale sottoscritto in data 24 ottobre 2018 tra l’Azienda " +
                //        "e le Organizzazioni Sindacali.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par2, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par3 = String.Format("Il Suo rapporto e’ regolato dal vigente Contratto Collettivo di " +
                //        "Lavoro per i professori d’orchestra della RAI, contratto del quale Ella ha preso visione " +
                //        "e che dichiara di accettare integralmente, nonché dalle disposizioni di servizio impartite " +
                //        "da questa società. ");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par3, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par4 = String.Format("Le verrà corrisposto uno stipendio di Euro 1.415,38 " +
                //        "(millequattrocentoquindici/trentotto) lordi mensili, una maggiorazione del " +
                //        "6,60% dello stipendio minimo della categoria ai sensi dell’art. 45 comma 6 " +
                //        "del citato contratto collettivo, una indennità rimborso spese professionali " +
                //        "di Euro 68,17=(sessantotto/diciassette), l’indennità di contingenza nella misura " +
                //        "prevista dalle vigenti disposizioni, ed inoltre, ai sensi e per i fini del " +
                //        "sesto comma dell’art. 3 del citato Contratto Collettivo, una speciale indennità " +
                //        "mensile commisurata al 40% dello stipendio e della contingenza.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par4, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par5 = String.Format("La retribuzione prevista in contratto costituisce esclusivamente " +
                //        "la remunerazione concordata per la Sua prestazione lavorativa. Con specifico riferimento " +
                //        "alla Normativa in materia di anticorruzione, Lei dichiara e garantisce che, nello " +
                //        "svolgimento della Sua prestazione lavorativa, non terrà, direttamente o indirettamente, " +
                //        "comportamenti, omissivi e/o commissivi, che possano comportare la violazione della " +
                //        "normativa anticorruzione (per quanto applicabile a RAI medesima) e/o del Piano Triennale " +
                //        "di Prevenzione della Corruzione (e protocolli nello stesso previsti) adottato da RAI " +
                //        "ai sensi della legge n.190/2012, di cui è tenuto ad acquisire conoscenza sul sito intranet " +
                //        "della RAI (http://www.raiplace.rai.it), nell’area tematica “Norme e Procedure” – " +
                //        "“Governance, Controllo e Conformità” – “Anticorruzione”.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par5, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par6 = String.Format("Ella prende atto che l`Azienda non è tenuta a riconoscere e non " +
                //        "riconoscerà la rinnovazione o la proroga del presente contratto che non sia stata " +
                //        "formalizzata per iscritto e sottoscritta da un procuratore dell`azienda munito dei " +
                //        "relativi poteri.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par6, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par7 = String.Format("Ai sensi dell’art. 13 del Regolamento UE 2016/679 " +
                //        "(Regolamento Generale sulla protezione dei dati personali), La informiamo che i " +
                //        "dati personali da Lei forniti verranno utilizzati da Rai-Radiotelevisione italiana Spa " +
                //        "(titolare del trattamento) – anche tramite collaboratori esterni - con modalità manuali, " +
                //        "informatiche e telematiche, a mezzo inserimento in banche dati gestite dalla Azienda " +
                //        "Rai medesima.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par7, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par8 = String.Format("In particolare, unitamente alla stipula del presente contratto, " +
                //        "Le viene consegnata l’informativa di cui al richiamato art. 13 Regolamento UE 2016/679, " +
                //        "che ne costituisce parte integrante. Ella dovrà aver cura di leggerla attentamente e di " +
                //        "restituircela debitamente datata e sottoscritta per presa visione. Ad ogni buon conto, sul " +
                //        "sito intranet della Rai (http://www.raiplace.rai.it, nell’area tematica " +
                //        "“Norme e Procedure” – “Governance, Controllo e Conformità” – “Privacy”), troverà comunque " +
                //        "pubblicato il testo dell’informativa in questione.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par8, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par9 = String.Format("Ella, preso  atto dei Principi etici  generali di onestà e osservanza " +
                //        "della legge, pluralismo, professionalità,  imparzialità,  correttezza, riservatezza, trasparenza, " +
                //        "diligenza, lealtà e buona fede nonché  del contenuto  tutto del  Codice etico del  Gruppo  " +
                //        "RAI - che  dichiara di conoscere  globalmente  e  nelle  sue  singole  parti, avendone  presa " +
                //        "completa e piena  visione su base cartacea e/o attraverso collegamento telematico al sito " +
                //        "intranet della Rai (http://www.raiplace.rai.it, nell’area tematica “Norme e Procedure” – " +
                //        "“Governance, Controllo e Conformità” – “Codice Etico”)  - si  impegna, per tutta  la durata  " +
                //        "del presente  contratto, ad  attenersi al Codice stesso, osservando un comportamento ad esso " +
                //        "pienamente conforme e che non  risulti lesivo  dell'immagine e, comunque, dei  valori morali " +
                //        "e materiali in cui il Gruppo RAI si riconosce e che applica nell'esercizio della " +
                //        "propria attività, anche con riferimento ai rapporti con i terzi.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par9, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par10 = String.Format("Ella, inoltre, espressamente dichiara di aver ricevuto copia cartacea " +
                //        "e di aver recepito i contenuti del Manuale informativo per la Sicurezza, nella versione " +
                //        "predisposta dalla RAI medesima in relazione alle sue specificità. Tale documento, anche in " +
                //        "riferimento ad eventuali possibili aggiornamenti, è altresì disponibile in formato elettronico " +
                //        "attraverso collegamento telematico al sito intranet http://www.raiplace.rai.it, nell’area " +
                //        "tematica “Norme e Procedure” – “Salute, Sicurezza e Ambiente – Tutela delle persone”.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par10, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par11 = String.Format("Ella si impegna, ove tale adempimento non sia da Lei già stato " +
                //        "documentatamente effettuato, a fruire, all'atto della Sua immissione in servizio e " +
                //        "preventivamente all'inizio della sua mansione lavorativa, del Corso Multimediale sulla " +
                //        "sicurezza nei luoghi di lavoro. Tale corso, della durata indicativa di 6 ore, ha l'obiettivo " +
                //        "di erogare e certificare, previa verifica finale dell'apprendimento, la formazione di base " +
                //        "sui rischi specifici connessi al Suo profilo professionale. La fruizione del corso è " +
                //        "possibile da qualsiasi postazione di lavoro informatica connessa alla rete aziendale, " +
                //        "sul sito intranet http://www.raiplace.rai.it, nell’area tematica “RAI ACADEMY” – “Catalogo " +
                //        "Corsi”.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par11, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par12 = String.Format("Ella, altresì, espressamente  dichiara  di conoscere - avendone " +
                //        "presa completa e piena  visione su base cartacea e/o attraverso collegamento telematico al " +
                //        "sito intranet della Rai (http://www.raiplace.rai.it, nell’area tematica “Norme e Procedure” " +
                //        "– “Governance, Controllo e Conformità”) il Modello di Organizzazione, Gestione e Controllo " +
                //        "RAI ex d.lgs. 231/2001 sulla responsabilità  amministrativa da reato degli enti - nella " +
                //        "versione  messa a disposizione dalla Rai medesima - e si impegna a tenere, nell' " +
                //        "esecuzione delle prestazioni, comportamenti in linea con i principi contenuti in detto " +
                //        "Modello Organizzativo \"231\".");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par12, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par13 = String.Format("Con riferimento al diritto di precedenza riconosciuto dalla legge " +
                //        "nelle assunzioni a tempo indeterminato (o a tempo determinato, in caso di lavoratrici che " +
                //        "abbiano fruito del congedo di maternità) si rinvia integralmente a quanto previsto dall’art. " +
                //        "24 del decreto legislativo 15.06.2015, n.81.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par13, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par14 = String.Format("Ad ogni effetto di legge, Le comunichiamo che abbiamo inserito il " +
                //        "Suo nominativo  nel  Libro Unico  del  Lavoro di cui all'art. 39 del  D.L. 25 giugno 2008, " +
                //        "n. 112, convertito  nella L. n. 133/2008 e che, con la consegna di copia del presente " +
                //        "contratto, l'azienda intende  assolvere agli obblighi comunicativi di cui al D.Lgs 26 maggio " +
                //        "1997, n. 152. ");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par14, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par15 = String.Format("Con la sottoscrizione del presente contratto, Lei si impegna " +
                //        "infine a compilare e restituire completi in tutte le loro parti alle competenti strutture " +
                //        "di gestione del Personale i formulari di dichiarazione che Le saranno sottoposti al fine " +
                //        "di acquisire le informazioni essenziali per la corretta gestione del Suo rapporto di lavoro, " +
                //        "quali, a titolo meramente esemplificativo e non esaustivo, la dichiarazione sulla spettanza " +
                //        "delle detrazioni d'imposta, la dichiarazione circa l'anzianità assicurativa e contributiva " +
                //        "posseduta, la modulistica ai fini della scelta circa la destinazione del TFR (ove necessaria).");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par15, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par16 = String.Format("Nel restituire copia della presente da Lei firmata in segno di " +
                //        "integrale accettazione, La invitiamo a consegnare i seguenti documenti:");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par16, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    PdfPTable tableDetail = new PdfPTable(2);
                //    tableDetail.DefaultCell.BorderWidth = 0;
                //    tableDetail.TotalWidth = 550;
                //    tableDetail.LockedWidth = true;
                //    var tableDetailWidth = new int[] { 50, 500 };
                //    tableDetail.SetWidths(tableDetailWidth);

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    Phrase cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("eventuale libretto di iscrizione alla Gestione ex ENPALS dell’INPS ove " +
                //        "da lei posseduto in relazione a precedenti rapporti di lavoro nello spettacolo;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("certificato di nascita;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("stato di famiglia con eventuale autorizzazione per assegni familiari;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("gli altri documenti eventualmente necessari per perfezionare " +
                //        "l’assunzione di cui sopra ivi inclusi se del caso quelli utili al godimento delle " +
                //        "agevolazioni normative e/o contributive di legge.", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    document.Add(tableDetail);
                //    tableDetail.FlushContent();
                //    tableDetail.DeleteBodyRows();

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par17 = String.Format("Cordiali Saluti");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par17, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par18 = String.Format("Il sottoscritto da' e prende atto consapevolmente, e ad ogni effetto " +
                //        "della vigente normativa, che il rapporto di lavoro costituito ai sensi di quanto sopra " +
                //        "convenuto, avrà termine il {0} e che, alla fine dell`orario di lavoro di detto giorno " +
                //        "cesseranno le prestazioni lavorative del sottoscritto senza bisogno di ulteriore o diversa " +
                //        "intimazione e senza necessità di preavviso, essendo detti intimazione e preavviso già " +
                //        "contenuti nella pattuita fissazione del termine delle prestazioni del sottoscritto alla " +
                //        "data del {1}."
                //        , assunzione.DataFine.GetValueOrDefault().ToString("dd/MM/yyyy")
                //        , assunzione.DataFine.GetValueOrDefault().ToString("dd/MM/yyyy"));
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par18, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    PdfPTable tableFirma = new PdfPTable(4);
                //    tableFirma.DefaultCell.BorderWidth = 0;
                //    tableFirma.TotalWidth = 550;
                //    tableFirma.LockedWidth = true;
                //    var tableFirmaWidth = new int[] { 137, 137, 137, 137 };
                //    tableFirma.SetWidths(tableFirmaWidth);
                //    tableFirma.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 1, myFontCorpo, 0));
                //    tableFirma.AddCell(WriteCellsClass.WriteCell("FIRMA", 0, 2, 1, myFontCorpo, 0));

                //    document.Add(tableFirma);
                //    tableFirma.FlushContent();
                //    tableFirma.DeleteBodyRows();

                //    document.Close();
                //    writer.Close();
                //    bytes = ms.ToArray();
                //    return bytes;
                //}
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static byte[] GeneraPdf_Bozza_Orchestrale_Prestazioni_Ridotte_TD(AssunzioniVM assunzione)
        {
            try
            {
                //var cultureInfo = CultureInfo.GetCultureInfo("it-IT");

                //byte[] bytes = null;
                //const int fontIntestazione = 12;
                //const int fontCorpo = 12;
                //const string FONTNAME = "Calibri"; //"Times-Roman"
                //const string SPAZIO = " ";

                //BaseColor color = new BaseColor(System.Drawing.Color.Black);
                //Font myFontIntestazione = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.NORMAL, color);
                //Font myFontIntestazioneBold = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.BOLD, color);
                //Font myFontIntestazioneBoldItalic = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.BOLDITALIC, color);
                //Font myFontCorpo = FontFactory.GetFont(FONTNAME, fontCorpo, Font.NORMAL, color);
                //Font myFontCorpoBold = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.BOLD, color);
                //Font myFontCorpoSottolineato = FontFactory.GetFont(FONTNAME, fontCorpo, Font.UNDERLINE, color);
                //Font myFontCorpoItalic = FontFactory.GetFont(FONTNAME, fontCorpo, Font.ITALIC, color);
                //Font myFontInterlinea = FontFactory.GetFont(FONTNAME, 7, Font.NORMAL, color);

                //string sesso = assunzione.Genere.GetValueOrDefault().ToString();
                //string nominativo = String.Format("{0} {1}", assunzione.Cognome.Trim(), assunzione.Nome.Trim());
                //string mansione = GetRuoloByCode(assunzione.SelectedMansione);
                //string categoria = GetCategoriaByCode(assunzione.SelectedCategoria);
                //string ruolo = GetRuoloByCategoria(assunzione.SelectedCategoria);
                //string sede = GetSedeByCode(assunzione.SelectedSede);
                //string direzione = GetDirezioneByCode(assunzione.SelectedServizio);

                IncentiviEntities db = new IncentiviEntities();
                byte[] bytes = null;

                string sesso = assunzione.Genere.GetValueOrDefault().ToString();
                string nominativo = String.Format("{0} {1}", assunzione.Cognome.Trim(), assunzione.Nome.Trim());
                string mansione = GetRuoloByCode(assunzione.SelectedMansione);
                string categoria = GetCategoriaByCode(assunzione.SelectedCategoria);
                string ruolo = GetRuoloByCategoria(assunzione.SelectedCategoria);
                string sede = GetSedeByCode(assunzione.SelectedSede);
                string direzione = GetDirezioneByCode(assunzione.SelectedServizio);
                string stipendio = GetStipendio(assunzione.SelectedCategoria, assunzione.SelectedTipoMinimo);

                DateTime fineFormazione = DateTime.Today;

                var xx = db.XR_HRIS_TEMPLATE.Where(x => x.NME_TEMPLATE == "BOZZA ORCHESTRALE TD PRESTAZIONI RIDOTTE").FirstOrDefault();
                if (xx != null)
                {
                    Stream stream = new MemoryStream(xx.TEMPLATE);
                    DocX doc = DocX.Load(stream);
                    doc.ReplaceText("#GENERE#", sesso.ToUpper().Equals("F") ? "Sig.ra" : "Sig");
                    doc.ReplaceText("#DATAODIERNA#", DateTime.Today.ToString("dd/MM/yyyy"));
                    doc.ReplaceText("#NOMINATIVO#", nominativo);
                    doc.ReplaceText("#DATAINIZIO#", assunzione.DataInizio.ToString("dd/MM/yyyy"));
                    if (assunzione.DataFine.HasValue)
                        doc.ReplaceText("#DATAFINE#", assunzione.DataFine.Value.ToString("dd/MM/yyyy"));
                    doc.ReplaceText("#MANSIONE#", mansione);
                    doc.ReplaceText("#CATEGORIA#", categoria);
                    doc.ReplaceText("#DIREZIONE#", direzione);
                    doc.ReplaceText("#SEDE#", sede);
                    doc.ReplaceText("#STIPENDIOCIFRA#", stipendio);
                    doc.ReplaceText("#STIPENDIO#", stipendio);
                    MemoryStream ms = new MemoryStream();
                    doc.SaveAs(ms);

                    bytes = ms.GetBuffer();
                }

                return bytes;

                //using (MemoryStream ms = new MemoryStream())
                //{
                //    string _imgPath = System.Web.HttpContext.Current.Server.MapPath("~/assets/img/LogoPDF.png");
                //    iTextSharp.text.Image png = iTextSharp.text.Image.GetInstance(_imgPath);
                //    string codSocieta = "";

                //    // verifica la società di appartenenza per caricare il logo adatto
                //    using (IncentiviEntities sdb = new IncentiviEntities())
                //    {
                //        var item = sdb.SINTESI1.Where(w => w.COD_MATLIBROMAT.Equals(assunzione.Matricola)).FirstOrDefault();

                //        if (item != null)
                //        {
                //            codSocieta = item.COD_IMPRESACR;
                //        }
                //    }

                //    // Creazione dell'istanza del documento, impostando tipo di pagina e margini
                //    Document document = new Document(PageSize.A4, 60, 60, 60, 80);
                //    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                //    writer.PageEvent = new AssunzioniManagerPdfPageEventHelper(codSocieta);
                //    document.Open();
                //    PdfContentByte cb = writer.DirectContent;
                //    Paragraph p = null;

                //    Phrase phrase = new Phrase();
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    //phrase.Add(new Chunk("RUO/GSR/6340/22/", myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();

                //    if (sesso.ToUpper().Equals("F"))
                //    {
                //        phrase.Add(new Chunk("Sig.ra ", myFontCorpo));
                //    }
                //    else
                //    {
                //        phrase.Add(new Chunk("Sig. ", myFontCorpo));
                //    }

                //    phrase.Add(new Chunk(nominativo, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_RIGHT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk("ROMA", myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_RIGHT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk("Roma, " + DateTime.Today.ToString("dd/MM/yyyy"), myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk("Assunzione a tempo determinato", myFontCorpoSottolineato));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par1 = String.Format("Le comunichiamo che Ella viene assunta con contratto di lavoro " +
                //        "subordinato a tempo determinato dal {0} al {1} (u.g.s.), in qualità di {2} con " +
                //        "inquadramento in categoria contrattuale {3} e ruolo di {4} presso " +
                //        "l’Orchestra Sinfonica Nazionale con sede di lavoro in {5}.",
                //        assunzione.DataInizio.ToString("dd/MM/yyyy"),
                //        assunzione.DataFine.GetValueOrDefault().ToString("dd/MM/yyyy"),
                //        mansione,
                //        categoria,
                //        ruolo,
                //        sede);
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par1, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par2 = String.Format("La Sua assunzione viene effettuata ai sensi degli artt. 19 e ss. " +
                //        "del decreto legislativo 15 giugno 2015, n. 81 e s.m.i. ed in attuazione dell’accordo " +
                //        "sindacale in materia di lavoro stagionale sottoscritto in data 24 ottobre 2018 tra l’Azienda " +
                //        "e le Organizzazioni Sindacali.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par2, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par2Bis = String.Format("Il Suo rapporto di lavoro è costituito per prestazioni ridotte che avranno luogo nelle seguenti date:");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par2Bis, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par3 = String.Format("Il Suo rapporto e’ regolato dal vigente Contratto Collettivo di " +
                //        "Lavoro per i professori d’orchestra della RAI, contratto del quale Ella ha preso visione " +
                //        "e che dichiara di accettare integralmente, nonché dalle disposizioni di servizio impartite " +
                //        "da questa società. ");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par3, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par4 = String.Format("Le verrà corrisposto uno stipendio di Euro 1.415,38 " +
                //        "(millequattrocentoquindici/trentotto) lordi mensili, una indennità rimborso spese " +
                //        "professionali di Euro 68,17=(sessantotto/diciassette), l’indennità di contingenza nella misura " +
                //        "prevista dalle vigenti disposizioni, ed inoltre, ai sensi e per i fini del sesto comma " +
                //        "dell’art. 3 del citato Contratto Collettivo, una speciale indennità mensile commisurata " +
                //        "al 40% dello stipendio e della contingenza.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par4, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par5 = String.Format("La retribuzione prevista in contratto costituisce esclusivamente " +
                //        "la remunerazione concordata per la Sua prestazione lavorativa. Con specifico riferimento " +
                //        "alla Normativa in materia di anticorruzione, Lei dichiara e garantisce che, nello " +
                //        "svolgimento della Sua prestazione lavorativa, non terrà, direttamente o indirettamente, " +
                //        "comportamenti, omissivi e/o commissivi, che possano comportare la violazione della " +
                //        "normativa anticorruzione (per quanto applicabile a RAI medesima) e/o del Piano Triennale " +
                //        "di Prevenzione della Corruzione (e protocolli nello stesso previsti) adottato da RAI " +
                //        "ai sensi della legge n.190/2012, di cui è tenuto ad acquisire conoscenza sul sito intranet " +
                //        "della RAI (http://www.raiplace.rai.it), nell’area tematica “Norme e Procedure” – " +
                //        "“Governance, Controllo e Conformità” – “Anticorruzione”.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par5, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par6 = String.Format("Ella prende atto che l`Azienda non è tenuta a riconoscere e non " +
                //        "riconoscerà la rinnovazione o la proroga del presente contratto che non sia stata " +
                //        "formalizzata per iscritto e sottoscritta da un procuratore dell`azienda munito dei " +
                //        "relativi poteri.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par6, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par7 = String.Format("Ai sensi dell’art. 13 del Regolamento UE 2016/679 " +
                //        "(Regolamento Generale sulla protezione dei dati personali), La informiamo che i " +
                //        "dati personali da Lei forniti verranno utilizzati da Rai-Radiotelevisione italiana Spa " +
                //        "(titolare del trattamento) – anche tramite collaboratori esterni - con modalità manuali, " +
                //        "informatiche e telematiche, a mezzo inserimento in banche dati gestite dalla Azienda " +
                //        "Rai medesima.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par7, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par8 = String.Format("In particolare, unitamente alla stipula del presente contratto, " +
                //        "Le viene consegnata l’informativa di cui al richiamato art. 13 Regolamento UE 2016/679, " +
                //        "che ne costituisce parte integrante. Ella dovrà aver cura di leggerla attentamente e di " +
                //        "restituircela debitamente datata e sottoscritta per presa visione. Ad ogni buon conto, sul " +
                //        "sito intranet della Rai (http://www.raiplace.rai.it, nell’area tematica " +
                //        "“Norme e Procedure” – “Governance, Controllo e Conformità” – “Privacy”), troverà comunque " +
                //        "pubblicato il testo dell’informativa in questione.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par8, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par9 = String.Format("Ella, preso  atto dei Principi etici  generali di onestà e osservanza " +
                //        "della legge, pluralismo, professionalità,  imparzialità,  correttezza, riservatezza, trasparenza, " +
                //        "diligenza, lealtà e buona fede nonché  del contenuto  tutto del  Codice etico del  Gruppo  " +
                //        "RAI - che  dichiara di conoscere  globalmente  e  nelle  sue  singole  parti, avendone  presa " +
                //        "completa e piena  visione su base cartacea e/o attraverso collegamento telematico al sito " +
                //        "intranet della Rai (http://www.raiplace.rai.it, nell’area tematica “Norme e Procedure” – " +
                //        "“Governance, Controllo e Conformità” – “Codice Etico”)  - si  impegna, per tutta  la durata  " +
                //        "del presente  contratto, ad  attenersi al Codice stesso, osservando un comportamento ad esso " +
                //        "pienamente conforme e che non  risulti lesivo  dell'immagine e, comunque, dei  valori morali " +
                //        "e materiali in cui il Gruppo RAI si riconosce e che applica nell'esercizio della " +
                //        "propria attività, anche con riferimento ai rapporti con i terzi.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par9, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par10 = String.Format("Ella, inoltre, espressamente dichiara di aver ricevuto copia cartacea " +
                //        "e di aver recepito i contenuti del Manuale informativo per la Sicurezza, nella versione " +
                //        "predisposta dalla RAI medesima in relazione alle sue specificità. Tale documento, anche in " +
                //        "riferimento ad eventuali possibili aggiornamenti, è altresì disponibile in formato elettronico " +
                //        "attraverso collegamento telematico al sito intranet http://www.raiplace.rai.it, nell’area " +
                //        "tematica “Norme e Procedure” – “Salute, Sicurezza e Ambiente – Tutela delle persone”.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par10, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par11 = String.Format("Ella si impegna, ove tale adempimento non sia da Lei già stato " +
                //        "documentatamente effettuato, a fruire, all'atto della Sua immissione in servizio e " +
                //        "preventivamente all'inizio della sua mansione lavorativa, del Corso Multimediale sulla " +
                //        "sicurezza nei luoghi di lavoro. Tale corso, della durata indicativa di 6 ore, ha l'obiettivo " +
                //        "di erogare e certificare, previa verifica finale dell'apprendimento, la formazione di base " +
                //        "sui rischi specifici connessi al Suo profilo professionale. La fruizione del corso è " +
                //        "possibile da qualsiasi postazione di lavoro informatica connessa alla rete aziendale, " +
                //        "sul sito intranet http://www.raiplace.rai.it, nell’area tematica “RAI ACADEMY” – “Catalogo " +
                //        "Corsi”.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par11, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par12 = String.Format("Ella, altresì, espressamente  dichiara  di conoscere - avendone " +
                //        "presa completa e piena  visione su base cartacea e/o attraverso collegamento telematico al " +
                //        "sito intranet della Rai (http://www.raiplace.rai.it, nell’area tematica “Norme e Procedure” " +
                //        "– “Governance, Controllo e Conformità”) il Modello di Organizzazione, Gestione e Controllo " +
                //        "RAI ex d.lgs. 231/2001 sulla responsabilità  amministrativa da reato degli enti - nella " +
                //        "versione  messa a disposizione dalla Rai medesima - e si impegna a tenere, nell' " +
                //        "esecuzione delle prestazioni, comportamenti in linea con i principi contenuti in detto " +
                //        "Modello Organizzativo \"231\".");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par12, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par13 = String.Format("Con riferimento al diritto di precedenza riconosciuto dalla legge " +
                //        "nelle assunzioni a tempo indeterminato (o a tempo determinato, in caso di lavoratrici che " +
                //        "abbiano fruito del congedo di maternità) si rinvia integralmente a quanto previsto dall’art. " +
                //        "24 del decreto legislativo 15.06.2015, n.81.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par13, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par14 = String.Format("Ad ogni effetto di legge, Le comunichiamo che abbiamo inserito il " +
                //        "Suo nominativo  nel  Libro Unico  del  Lavoro di cui all'art. 39 del  D.L. 25 giugno 2008, " +
                //        "n. 112, convertito  nella L. n. 133/2008 e che, con la consegna di copia del presente " +
                //        "contratto, l'azienda intende  assolvere agli obblighi comunicativi di cui al D.Lgs 26 maggio " +
                //        "1997, n. 152. ");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par14, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par15 = String.Format("Con la sottoscrizione del presente contratto, Lei si impegna " +
                //        "infine a compilare e restituire completi in tutte le loro parti alle competenti strutture " +
                //        "di gestione del Personale i formulari di dichiarazione che Le saranno sottoposti al fine " +
                //        "di acquisire le informazioni essenziali per la corretta gestione del Suo rapporto di lavoro, " +
                //        "quali, a titolo meramente esemplificativo e non esaustivo, la dichiarazione sulla spettanza " +
                //        "delle detrazioni d'imposta, la dichiarazione circa l'anzianità assicurativa e contributiva " +
                //        "posseduta, la modulistica ai fini della scelta circa la destinazione del TFR (ove necessaria).");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par15, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par16 = String.Format("Nel restituire copia della presente da Lei firmata in segno di " +
                //        "integrale accettazione, La invitiamo a consegnare i seguenti documenti:");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par16, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    PdfPTable tableDetail = new PdfPTable(2);
                //    tableDetail.DefaultCell.BorderWidth = 0;
                //    tableDetail.TotalWidth = 550;
                //    tableDetail.LockedWidth = true;
                //    var tableDetailWidth = new int[] { 50, 500 };
                //    tableDetail.SetWidths(tableDetailWidth);

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    Phrase cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("eventuale libretto di iscrizione alla Gestione ex ENPALS dell’INPS ove " +
                //        "da lei posseduto in relazione a precedenti rapporti di lavoro nello spettacolo;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("certificato di nascita;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("stato di famiglia con eventuale autorizzazione per assegni familiari;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("gli altri documenti eventualmente necessari per perfezionare " +
                //        "l’assunzione di cui sopra ivi inclusi se del caso quelli utili al godimento delle " +
                //        "agevolazioni normative e/o contributive di legge.", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    document.Add(tableDetail);
                //    tableDetail.FlushContent();
                //    tableDetail.DeleteBodyRows();

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par17 = String.Format("Cordiali Saluti");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par17, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par18 = String.Format("Il sottoscritto da' e prende atto consapevolmente, e ad ogni effetto " +
                //        "della vigente normativa, che il rapporto di lavoro costituito ai sensi di quanto sopra " +
                //        "convenuto, avrà termine il {0} e che, alla fine dell`orario di lavoro di detto giorno " +
                //        "cesseranno le prestazioni lavorative del sottoscritto senza bisogno di ulteriore o diversa " +
                //        "intimazione e senza necessità di preavviso, essendo detti intimazione e preavviso già " +
                //        "contenuti nella pattuita fissazione del termine delle prestazioni del sottoscritto alla " +
                //        "data del {1}."
                //        , assunzione.DataFine.GetValueOrDefault().ToString("dd/MM/yyyy")
                //        , assunzione.DataFine.GetValueOrDefault().ToString("dd/MM/yyyy"));
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par18, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    PdfPTable tableFirma = new PdfPTable(4);
                //    tableFirma.DefaultCell.BorderWidth = 0;
                //    tableFirma.TotalWidth = 550;
                //    tableFirma.LockedWidth = true;
                //    var tableFirmaWidth = new int[] { 137, 137, 137, 137 };
                //    tableFirma.SetWidths(tableFirmaWidth);
                //    tableFirma.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 1, myFontCorpo, 0));
                //    tableFirma.AddCell(WriteCellsClass.WriteCell("FIRMA", 0, 2, 1, myFontCorpo, 0));

                //    document.Add(tableFirma);
                //    tableFirma.FlushContent();
                //    tableFirma.DeleteBodyRows();

                //    document.Close();
                //    writer.Close();
                //    bytes = ms.ToArray();
                //    return bytes;
                //}
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region PUBLIC TI

        public static byte[] GeneraPdf_Bozza_Impiegato_TI(AssunzioniVM assunzione)
        {
            try
            {
                //var cultureInfo = CultureInfo.GetCultureInfo("it-IT");
                IncentiviEntities db = new IncentiviEntities();
                byte[] bytes = null;
                string sesso = assunzione.Genere.GetValueOrDefault().ToString();
                string nominativo = String.Format("{0} {1}", assunzione.Cognome.Trim(), assunzione.Nome.Trim());
                string mansione = GetRuoloByCode(assunzione.SelectedMansione);
                string categoria = GetCategoriaByCode(assunzione.SelectedCategoria);
                string ruolo = GetRuoloByCategoria(assunzione.SelectedCategoria);
                string sede = GetSedeByCode(assunzione.SelectedSede);
                string direzione = GetDirezioneByCode(assunzione.SelectedServizio);
                string stipendio = GetStipendio(assunzione.SelectedCategoria, assunzione.SelectedTipoMinimo);

                var xx = db.XR_HRIS_TEMPLATE.Where(x => x.NME_TEMPLATE == "BOZZA ASSUNZIONE IMPIEGATO TI").FirstOrDefault();
                if (xx != null)
                {
                    Stream stream = new MemoryStream(xx.TEMPLATE);
                    DocX doc = DocX.Load(stream);
                    doc.ReplaceText("#GENERE#", sesso.ToUpper().Equals("F") ? "Sig.ra" : "Sig");
                    doc.ReplaceText("#DATAODIERNA#", DateTime.Today.ToString("dd/MM/yyyy"));
                    doc.ReplaceText("#NOMINATIVO#", nominativo);
                    doc.ReplaceText("#DATAINIZIO#", assunzione.DataInizio.ToString("dd/MM/yyyy"));
                    doc.ReplaceText("#MANSIONE#", mansione);
                    doc.ReplaceText("#CATEGORIA#", categoria);
                    doc.ReplaceText("#DIREZIONE#", direzione);
                    doc.ReplaceText("#SEDE#", sede);
                    doc.ReplaceText("#STIPENDIOCIFRA#", stipendio);
                    doc.ReplaceText("#STIPENDIO#", stipendio);
                    MemoryStream ms = new MemoryStream();
                    doc.SaveAs(ms);

                    bytes = ms.GetBuffer();
                }





                return bytes;
                //const int fontIntestazione = 12;
                //const int fontCorpo = 12;
                //const string FONTNAME = "Calibri"; //"Times-Roman"
                //const string SPAZIO = " ";

                //BaseColor color = new BaseColor(System.Drawing.Color.Black);
                //Font myFontIntestazione = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.NORMAL, color);
                //Font myFontIntestazioneBold = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.BOLD, color);
                //Font myFontIntestazioneBoldItalic = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.BOLDITALIC, color);
                //Font myFontCorpo = FontFactory.GetFont(FONTNAME, fontCorpo, Font.NORMAL, color);
                //Font myFontCorpoBold = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.BOLD, color);
                //Font myFontCorpoSottolineato = FontFactory.GetFont(FONTNAME, fontCorpo, Font.UNDERLINE, color);
                //Font myFontCorpoItalic = FontFactory.GetFont(FONTNAME, fontCorpo, Font.ITALIC, color);
                //Font myFontInterlinea = FontFactory.GetFont(FONTNAME, 7, Font.NORMAL, color);

                //string sesso = assunzione.Genere.GetValueOrDefault().ToString();
                //string nominativo = String.Format("{0} {1}", assunzione.Cognome.Trim(), assunzione.Nome.Trim());
                //string mansione = GetRuoloByCode(assunzione.SelectedMansione);
                //string categoria = GetCategoriaByCode(assunzione.SelectedCategoria);
                //string ruolo = GetRuoloByCategoria(assunzione.SelectedCategoria);
                //string sede = GetSedeByCode(assunzione.SelectedSede);
                //string direzione = GetDirezioneByCode(assunzione.SelectedServizio);

                //using (MemoryStream ms = new MemoryStream())
                //{
                //    string _imgPath = System.Web.HttpContext.Current.Server.MapPath("~/assets/img/LogoPDF.png");
                //    iTextSharp.text.Image png = iTextSharp.text.Image.GetInstance(_imgPath);
                //    string codSocieta = "";

                //    // verifica la società di appartenenza per caricare il logo adatto
                //    using (IncentiviEntities sdb = new IncentiviEntities())
                //    {
                //        var item = sdb.SINTESI1.Where(w => w.COD_MATLIBROMAT.Equals(assunzione.Matricola)).FirstOrDefault();

                //        if (item != null)
                //        {
                //            codSocieta = item.COD_IMPRESACR;
                //        }
                //    }

                //    // Creazione dell'istanza del documento, impostando tipo di pagina e margini
                //    Document document = new Document(PageSize.A4, 60, 60, 60, 80);
                //    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                //    writer.PageEvent = new AssunzioniManagerPdfPageEventHelper(codSocieta);
                //    document.Open();
                //    PdfContentByte cb = writer.DirectContent;
                //    Paragraph p = null;

                //    Phrase phrase = new Phrase();
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    //phrase.Add(new Chunk("RUO/GSR/6340/22/", myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();

                //    if (sesso.ToUpper().Equals("F"))
                //    {
                //        phrase.Add(new Chunk("Sig.ra ", myFontCorpo));
                //    }
                //    else
                //    {
                //        phrase.Add(new Chunk("Sig. ", myFontCorpo));
                //    }

                //    phrase.Add(new Chunk(nominativo, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_RIGHT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk("ROMA", myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_RIGHT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk("Roma, " + DateTime.Today.ToString("dd/MM/yyyy"), myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk("Assunzione in servizio", myFontCorpoSottolineato));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par1 = String.Format("Le comunichiamo che Ella viene assunta, a decorrere dal " +
                //        "{0}, con contratto di lavoro subordinato a tempo indeterminato in qualità di " +
                //        "“{1}”, con inquadramento al {2}, presso la " +
                //        "RAI-Radiotelevisione italiana – Direzione {3} - Sede di {4}.",
                //        assunzione.DataInizio.ToString("dd/MM/yyyy"),
                //        mansione,
                //        categoria,
                //        direzione,
                //        sede);
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par1, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par2 = String.Format("Il Suo rapporto di lavoro è regolato dal vigente Contratto " +
                //        "Collettivo di Lavoro per quadri, impiegati ed operai dipendenti dalla Rai " +
                //        "Radiotelevisione italiana S.p.A. e dalle Società del Gruppo, contratto del quale Ella " +
                //        "ha preso visione, che dichiara di accettare integralmente ed al quale si rinvia in " +
                //        "ordine ad orario di lavoro, ferie, termini del preavviso ed a quant’altro non " +
                //        "espressamente previsto nella presente lettera per quanto concerne il trattamento " +
                //        "economico e normativo a Lei applicabile.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par2, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par3 = String.Format("La sua assunzione è subordinata ad un periodo di prova di 6 mesi " +
                //        "durante il quale è facoltà delle parti risolvere il rapporto di lavoro senza obbligo di " +
                //        "preavviso ne corresponsione di alcuna identità.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par3, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par4 = String.Format("Le verranno corrisposti uno stipendio di € 1.076,19 " +
                //        "(millesettantasei/19) lordi mensili quale minimo previsto dal sopra citato Contratto " +
                //        "Collettivo di Lavoro e l’indennità di contingenza nella misura prevista dalle " +
                //        "disposizioni vigenti.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par4, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par5 = String.Format("La retribuzione prevista in contratto costituisce esclusivamente " +
                //        "la remunerazione concordata per la Sua prestazione lavorativa. Con specifico riferimento " +
                //        "alla Normativa in materia di anticorruzione, Lei dichiara e garantisce che, nello " +
                //        "svolgimento della Sua prestazione lavorativa, non terrà, direttamente o indirettamente, " +
                //        "comportamenti, omissivi e/o commissivi, che possano comportare la violazione della " +
                //        "normativa anticorruzione (per quanto applicabile a RAI medesima) e/o del Piano Triennale " +
                //        "di Prevenzione della Corruzione (e protocolli nello stesso previsti) adottato da RAI " +
                //        "ai sensi della legge n.190/2012, di cui è tenuto ad acquisire conoscenza sul sito intranet " +
                //        "della RAI (http://www.raiplace.rai.it), nell’area tematica “Norme e Procedure” – " +
                //        "Governance, Controllo e Conformità” – “Anticorruzione”.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par5, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par6 = String.Format("Ai sensi dell’art. 13 del Regolamento UE 2016/679 (Regolamento " +
                //        "Generale sulla protezione dei dati personali), La informiamo che i dati personali da Lei " +
                //        "forniti verranno utilizzati da Rai-Radiotelevisione italiana Spa (titolare del trattamento) " +
                //        "– anche tramite collaboratori esterni - con modalità manuali, informatiche e telematiche, " +
                //        "a mezzo inserimento in banche dati gestite dalla Azienda Rai medesima. ");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par6, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par7 = String.Format("In particolare, unitamente alla stipula del presente contratto, " +
                //        "Le viene consegnata l’informativa di cui  al  richiamato art. 13 Regolamento UE 2016/679, " +
                //        "che ne costituisce parte integrante.  Ella dovrà aver cura di leggerla attentamente e di " +
                //        "restituircela debitamente datata e sottoscritta per presa visione. Ad ogni buon conto, " +
                //        "sul sito intranet della Rai (http://www.raiplace.rai.it, nell’area tematica “Norme e Procedure” " +
                //        "– “Governance, Controllo e Conformità” – “Privacy”), troverà comunque pubblicato il testo " +
                //        "dell’informativa in questione.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par7, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par8 = String.Format("Ella, preso atto dei Principi etici generali di onestà e osservanza " +
                //        "della legge, pluralismo, professionalità, imparzialità, correttezza, riservatezza, trasparenza, " +
                //        "diligenza, lealtà e buona fede nonché del contenuto tutto del Codice etico del Gruppo RAI - " +
                //        "che dichiara di conoscere globalmente e nelle sue singole parti, avendone presa completa e " +
                //        "piena visione su base cartacea e/o attraverso collegamento telematico al sito intranet " +
                //        "della Rai (http://www.raiplace.rai.it, nell’area tematica “Norme e Procedure” – " +
                //        "“Governance, Controllo e Conformità” – “Codice Etico”) - si impegna, per tutta la durata " +
                //        "del presente  contratto, ad attenersi al Codice stesso, osservando un comportamento ad esso " +
                //        "pienamente conforme e che non risulti lesivo dell'immagine e, comunque, dei valori morali e " +
                //        "materiali in cui il Gruppo RAI si riconosce e che applica nell'esercizio della propria attività, " +
                //        "anche con riferimento ai rapporti con i terzi.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par8, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par9 = String.Format("Ella, inoltre, espressamente dichiara di aver ricevuto copia cartacea " +
                //        "e di aver recepito i contenuti del Manuale informativo per la Sicurezza, nella versione " +
                //        "predisposta dalla RAI medesima in relazione alle sue specificità. Tale documento, anche in " +
                //        "riferimento ad eventuali possibili aggiornamenti, è altresì disponibile in formato elettronico " +
                //        "attraverso collegamento telematico al sito intranet (http://www.raiplace.rai.it, nell'area " +
                //        "tematica “Norme e Procedure” – “Salute, Sicurezza e Ambiente – Tutela delle persone”.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par9, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par10 = String.Format("Ella si impegna, ove tale adempimento non sia da Lei già stato " +
                //        "documentatamente effettuato, a fruire, all'atto della Sua immissione in servizio e " +
                //        "preventivamente all'inizio della sua mansione lavorativa, del Corso Multimediale sulla " +
                //        "sicurezza nei luoghi di lavoro. Tale corso, della durata indicativa di 6 ore, ha l'obiettivo " +
                //        "di erogare e certificare, previa verifica finale dell'apprendimento, la formazione di base " +
                //        "sui rischi specifici connessi al Suo profilo professionale. La fruizione del corso è " +
                //        "possibile da qualsiasi postazione di lavoro informatica connessa alla rete aziendale, " +
                //        "sul sito intranet http://www.raiplace.rai.it, nell’area tematica “RAI ACADEMY” – “Catalogo " +
                //        "Corsi”.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par10, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par11 = String.Format("Ella, altresì, espressamente  dichiara  di conoscere - avendone " +
                //        "presa completa e piena  visione su base cartacea e/o attraverso collegamento telematico al " +
                //        "sito intranet della Rai (http://www.raiplace.rai.it, nell’area tematica “Norme e Procedure” " +
                //        "– “Governance, Controllo e Conformità”) il Modello di Organizzazione, Gestione e Controllo " +
                //        "RAI ex d.lgs. 231/2001 sulla responsabilità  amministrativa da reato degli enti - nella " +
                //        "versione  messa a disposizione dalla Rai medesima - e si impegna a tenere, nell' " +
                //        "esecuzione delle prestazioni, comportamenti in linea con i principi contenuti in detto " +
                //        "Modello Organizzativo \"231\".");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par11, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par12 = String.Format("Ad ogni effetto di legge, Le comunichiamo che abbiamo inserito il " +
                //        "Suo nominativo nel Libro Unico del Lavoro di cui all'art. 39 del D.L. 25 giugno 2008, " +
                //        "n. 112, convertito nella L. n. 133/2008 e che, con la consegna di copia del presente " +
                //        "contratto, l'azienda intende assolvere agli obblighi comunicativi di cui al D.Lgs 26 maggio " +
                //        "1997, n. 152. ");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par12, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par13 = String.Format("Ella prende atto che l’Azienda non è tenuta a riconoscere e non " +
                //        "riconoscerà qualsivoglia modifica del presente contratto che non sia stata formalizzata " +
                //        "per iscritto e sottoscritta da un procuratore dell’Azienda medesima munito dei relativi poteri.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par13, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par14 = String.Format("Con la sottoscrizione del presente contratto, Lei si impegna " +
                //        "infine a compilare e restituire completi in tutte le loro parti alle competenti strutture " +
                //        "di gestione del Personale i formulari di dichiarazione che Le saranno sottoposti al fine " +
                //        "di acquisire le informazioni essenziali per la corretta gestione del Suo rapporto di lavoro, " +
                //        "quali, a titolo meramente esemplificativo e non esaustivo, la dichiarazione sulla spettanza " +
                //        "delle detrazioni d'imposta, la dichiarazione circa l'anzianità assicurativa e contributiva " +
                //        "posseduta, la modulistica ai fini della scelta circa la destinazione del TFR (ove necessaria).");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par14, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par15 = String.Format("Nel restituire copia della presente da Lei datata e sottoscritta " +
                //        "in segno di integrale accettazione di quanto ivi contenuto, La invitiamo, inoltre, a " +
                //        "consegnare i seguenti documenti: ");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par15, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    PdfPTable tableDetail = new PdfPTable(2);
                //    tableDetail.DefaultCell.BorderWidth = 0;
                //    tableDetail.TotalWidth = 550;
                //    tableDetail.LockedWidth = true;
                //    var tableDetailWidth = new int[] { 50, 500 };
                //    tableDetail.SetWidths(tableDetailWidth);

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    Phrase cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("eventuale libretto di iscrizione alla Gestione ex ENPALS dell’INPS ove " +
                //        "da lei posseduto in relazione a precedenti rapporti di lavoro nello spettacolo;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("certificati di nascita, cittadinanza, residenza e stato di famiglia " +
                //        "con eventuale autorizzazione per assegni familiari; ", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("titoli di studio;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("copia documento di identità;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("codice fiscale;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("gli altri documenti eventualmente necessari per perfezionare " +
                //        "l’assunzione di cui sopra ivi inclusi se del caso quelli utili al godimento delle " +
                //        "agevolazioni normative e/o contributive di legge.", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    document.Add(tableDetail);
                //    tableDetail.FlushContent();
                //    tableDetail.DeleteBodyRows();

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par16 = String.Format("Cordiali Saluti");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par16, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    //phrase = new Phrase();
                //    //phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    //p = new Paragraph(phrase);
                //    //((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    //document.Add(p);

                //    //phrase = new Phrase();
                //    //phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    //p = new Paragraph(phrase);
                //    //((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    //document.Add(p);                    

                //    //PdfPTable tableFirma = new PdfPTable(4);
                //    //tableFirma.DefaultCell.BorderWidth = 0;
                //    //tableFirma.TotalWidth = 550;
                //    //tableFirma.LockedWidth = true;
                //    //var tableFirmaWidth = new int[] { 137, 137, 137, 137 };
                //    //tableFirma.SetWidths(tableFirmaWidth);
                //    //tableFirma.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 1, myFontCorpo, 0));
                //    //tableFirma.AddCell(WriteCellsClass.WriteCell("FIRMA", 0, 2, 1, myFontCorpo, 0));

                //    //document.Add(tableFirma);
                //    //tableFirma.FlushContent();
                //    //tableFirma.DeleteBodyRows();

                //    document.Close();
                //    writer.Close();

                //}
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static byte[] GeneraPdf_Bozza_Apprendistato_Impiegato(AssunzioniVM assunzione)
        {
            try
            {
                //var cultureInfo = CultureInfo.GetCultureInfo("it-IT");
                IncentiviEntities db = new IncentiviEntities();
                byte[] bytes = null;
                //const int fontIntestazione = 12;
                //const int fontCorpo = 12;
                //const string FONTNAME = "Calibri"; //"Times-Roman"
                //const string SPAZIO = " ";

                //BaseColor color = new BaseColor(System.Drawing.Color.Black);
                //Font myFontIntestazione = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.NORMAL, color);
                //Font myFontIntestazioneBold = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.BOLD, color);
                //Font myFontIntestazioneBoldItalic = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.BOLDITALIC, color);
                //Font myFontCorpo = FontFactory.GetFont(FONTNAME, fontCorpo, Font.NORMAL, color);
                //Font myFontCorpoBold = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.BOLD, color);
                //Font myFontCorpoSottolineato = FontFactory.GetFont(FONTNAME, fontCorpo, Font.UNDERLINE, color);
                //Font myFontCorpoItalic = FontFactory.GetFont(FONTNAME, fontCorpo, Font.ITALIC, color);
                //Font myFontInterlinea = FontFactory.GetFont(FONTNAME, 7, Font.NORMAL, color);

                string sesso = assunzione.Genere.GetValueOrDefault().ToString();
                string nominativo = String.Format("{0} {1}", assunzione.Cognome.Trim(), assunzione.Nome.Trim());
                string mansione = GetRuoloByCode(assunzione.SelectedMansione);
                string categoria = GetCategoriaByCode(assunzione.SelectedCategoria);
                string ruolo = GetRuoloByCategoria(assunzione.SelectedCategoria);
                string sede = GetSedeByCode(assunzione.SelectedSede);
                string direzione = GetDirezioneByCode(assunzione.SelectedServizio);
                string stipendio = GetStipendio(assunzione.SelectedCategoria, assunzione.SelectedTipoMinimo);

                var xx = db.XR_HRIS_TEMPLATE.Where(x => x.NME_TEMPLATE == "BOZZA ASSUNZIONE APPRENDISTATO IMPIEGATO").FirstOrDefault();
                if (xx != null)
                {
                    Stream stream = new MemoryStream(xx.TEMPLATE);
                    DocX doc = DocX.Load(stream);
                    doc.ReplaceText("#GENERE#", sesso.ToUpper().Equals("F") ? "Sig.ra" : "Sig");
                    doc.ReplaceText("#DATAODIERNA#", DateTime.Today.ToString("dd/MM/yyyy"));
                    doc.ReplaceText("#NOMINATIVO#", nominativo);
                    doc.ReplaceText("#DATAINIZIO#", assunzione.DataInizio.ToString("dd/MM/yyyy"));
                    doc.ReplaceText("#MANSIONE#", mansione);
                    doc.ReplaceText("#CATEGORIA#", categoria);
                    doc.ReplaceText("#DIREZIONE#", direzione);
                    doc.ReplaceText("#SEDE#", sede);
                    doc.ReplaceText("#STIPENDIOCIFRA#", stipendio);
                    doc.ReplaceText("#STIPENDIO#", stipendio);
                    MemoryStream ms = new MemoryStream();
                    doc.SaveAs(ms);

                    bytes = ms.GetBuffer();
                }

                return bytes;

                //using (MemoryStream ms = new MemoryStream())
                //{
                //    string _imgPath = System.Web.HttpContext.Current.Server.MapPath("~/assets/img/LogoPDF.png");
                //    iTextSharp.text.Image png = iTextSharp.text.Image.GetInstance(_imgPath);
                //    string codSocieta = "";

                //    // verifica la società di appartenenza per caricare il logo adatto
                //    using (IncentiviEntities sdb = new IncentiviEntities())
                //    {
                //        var item = sdb.SINTESI1.Where(w => w.COD_MATLIBROMAT.Equals(assunzione.Matricola)).FirstOrDefault();

                //        if (item != null)
                //        {
                //            codSocieta = item.COD_IMPRESACR;
                //        }
                //    }

                //    // Creazione dell'istanza del documento, impostando tipo di pagina e margini
                //    Document document = new Document(PageSize.A4, 60, 60, 60, 80);
                //    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                //    writer.PageEvent = new AssunzioniManagerPdfPageEventHelper(codSocieta);
                //    document.Open();
                //    PdfContentByte cb = writer.DirectContent;
                //    Paragraph p = null;

                //    Phrase phrase = new Phrase();
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    //phrase.Add(new Chunk("RUO/GSR/6340/22/", myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();

                //    if (sesso.ToUpper().Equals("F"))
                //    {
                //        phrase.Add(new Chunk("Sig.ra ", myFontCorpo));
                //    }
                //    else
                //    {
                //        phrase.Add(new Chunk("Sig. ", myFontCorpo));
                //    }

                //    phrase.Add(new Chunk(nominativo, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_RIGHT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk("ROMA", myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_RIGHT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk("Roma, " + DateTime.Today.ToString("dd/MM/yyyy"), myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk("Assunzione in servizio", myFontCorpoSottolineato));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par1 = String.Format("Le comunichiamo che, a decorrere dal {0}, Ella viene assunta, " +
                //        "presso la RAI-Radiotelevisione italiana – Direzione {1} – sede di {2}, con contratto " +
                //        "di apprendistato professionalizzante finalizzato al conseguimento della qualificazione " +
                //        "nel profilo professionale di “{3}” di livello 5 (cinque).",
                //        assunzione.DataInizio.ToString("dd/MM/yyyy"),
                //        direzione,
                //        sede,
                //        mansione);
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par1, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par2 = String.Format("Per quanto non espressamente previsto nella presente lettera, " +
                //        "si rinvia alle disposizioni di cui agli artt. 41 e ss. del D. Lgs. 15 giugno 2015, " +
                //        "n. 81 nonché, in particolare per quanto riguarda l’orario di lavoro e le ferie, alle " +
                //        "previsioni del vigente Contratto Collettivo di Lavoro per Quadri Impiegati ed Operai " +
                //        "dipendenti dalla RAI e dalle Società del Gruppo, contratto del quale Ella ha preso " +
                //        "visione e che dichiara di accettare senza riserva alcuna.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par2, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par3 = String.Format("L’assunzione è subordinata al positivo superamento di un " +
                //        "periodo di prova di 60 giorni, durante i quali ciascuna delle parti sarà libera di " +
                //        "recedere dal contratto senza obbligo di preavviso né corresponsione di alcuna indennità.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par3, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par4 = String.Format("Il periodo di formazione avrà la durata di 36 mesi ed il percorso " +
                //        "formativo si concluderà il          .");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par4, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par5 = String.Format("Nel caso in cui sopraggiungessero assenze di significativa durata " +
                //        "che possano influire sul rispetto del piano formativo originariamente concordato, si " +
                //        "applicano le disposizioni in materia di slittamento del termine finale del contratto di " +
                //        "cui all’ “articolo 10 - Apprendistato professionalizzante” - dell’Accordo Collettivo del " +
                //        "7/2/2013.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par5, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par6 = String.Format("Costituisce parte integrante del presente contratto il piano " +
                //        "formativo individuale, avente ad oggetto i contenuti, l’articolazione e le modalità di " +
                //        "erogazione della formazione c.d. professionalizzante.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par6, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par7 = String.Format("Ella sarà inizialmente inquadrata al livello 7 (sette) in " +
                //        "qualità di “{0}”.", mansione);
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par7, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par8 = String.Format("Decorsi 24 mesi dall’inizio del rapporto e per i successivi 12, " +
                //        "Ella sarà invece inquadrata al livello 6 (sei) del medesimo profilo professionale.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par8, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par9 = String.Format("Al termine del periodo di formazione, in base agli esiti della " +
                //        "formazione svolta e valutate le competenze acquisite, Ella conseguirà la qualificazione " +
                //        "nel profilo professionale di “{0}” di livello 5 (cinque).", mansione);
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par9, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par10 = String.Format("Durante il periodo di formazione, a titolo di retribuzione, " +
                //        "in aggiunta all’indennità di contingenza nella misura prevista dalle disposizioni " +
                //        "vigenti, Le verrà corrisposto uno stipendio di:");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par10, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    PdfPTable tableDetail = new PdfPTable(2);
                //    tableDetail.DefaultCell.BorderWidth = 0;
                //    tableDetail.TotalWidth = 550;
                //    tableDetail.LockedWidth = true;
                //    var tableDetailWidth = new int[] { 50, 500 };
                //    tableDetail.SetWidths(tableDetailWidth);

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    Phrase cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("Euro 910,86 (novecentodieci/86) lordi mensili per i primi 24 mesi, " +
                //        "quale minimo previsto dal Contratto Collettivo di Lavoro per il livello 7 (sette); ", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("Euro 980,96 (novecentottanta/96) lordi mensili per i successivi " +
                //        "12 mesi, quale minimo previsto dal richiamato C.C.L. per il livello 6 (sei).", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    document.Add(tableDetail);
                //    tableDetail.FlushContent();
                //    tableDetail.DeleteBodyRows();

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par11 = String.Format("Al termine del periodo di formazione, ciascuna delle parti " +
                //        "potrà recedere dal contratto dandone comunicazione scritta e osservando un preavviso " +
                //        "di almeno 15 (quindici) giorni decorrente dalla suddetta scadenza. Diversamente, il " +
                //        "rapporto proseguirà tra le parti come ordinario rapporto di lavoro subordinato a " +
                //        "tempo indeterminato senza soluzione di continuità.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par11, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par12 = String.Format("In tal caso, al termine del periodo di formazione, in aggiunta " +
                //        "all’indennità di contingenza, Le verrà corrisposto uno stipendio di Euro 1.076,19 " +
                //        "(millesettantasei/19) lordi mensili quale minimo previsto dal richiamato C.C.L. " +
                //        "per il livello 5 (cinque).");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par12, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par13 = String.Format("Si precisa che gli importi dei minimi di stipendio sopra " +
                //        "indicati potranno subire variazioni in caso di rinnovo del vigente C.C.L.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par13, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par14 = String.Format("La retribuzione prevista in contratto costituisce esclusivamente " +
                //        "la remunerazione concordata per la Sua prestazione lavorativa. Con specifico riferimento " +
                //        "alla Normativa in materia di anticorruzione, Lei dichiara e garantisce che, nello " +
                //        "svolgimento della Sua prestazione lavorativa, non terrà, direttamente o indirettamente, " +
                //        "comportamenti, omissivi e/o commissivi, che possano comportare la violazione della " +
                //        "normativa anticorruzione (per quanto applicabile a RAI medesima) e/o del Piano Triennale " +
                //        "di Prevenzione della Corruzione (e protocolli nello stesso previsti) adottato da RAI " +
                //        "ai sensi della legge n.190/2012, di cui è tenuto ad acquisire conoscenza sul sito intranet " +
                //        "della RAI (http://www.raiplace.rai.it), nell’area tematica “Norme e Procedure” – " +
                //        "“Anticorruzione”.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par14, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par15 = String.Format("Ai sensi dell’art. 13 del Regolamento UE 2016/679 " +
                //        "(Regolamento Generale sulla protezione dei dati personali), La informiamo che i " +
                //        "dati personali da Lei forniti verranno utilizzati da Rai-Radiotelevisione italiana " +
                //        "Spa (titolare del trattamento) – anche tramite collaboratori esterni - con modalità " +
                //        "manuali, informatiche e telematiche, a mezzo inserimento in banche dati gestite dalla " +
                //        "Azienda Rai medesima. ");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par15, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par16 = String.Format("In particolare, unitamente alla stipula del presente contratto, " +
                //        "Le viene consegnata l’informativa di cui al  richiamato art. 13 Regolamento UE 2016/679, " +
                //        "che ne costituisce parte integrante. Ella dovrà aver cura di leggerla attentamente e " +
                //        "di restituircela debitamente datata e sottoscritta per presa visione.Ad ogni buon conto, " +
                //        "sul sito  intranet  della  Rai(http://www.raiplace.rai.it, nell’area tematica " +
                //        "“Norme e Procedure” – “Privacy”) troverà comunque pubblicato il testo dell’informativa " +
                //        "in questione.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par16, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par17 = String.Format("Ella, preso atto dei Principi etici generali di onestà e " +
                //        "osservanza della legge, pluralismo, professionalità, imparzialità, correttezza, " +
                //        "riservatezza, trasparenza, diligenza, lealtà e buona  fede nonché  del contenuto  " +
                //        "tutto del  Codice etico del  Gruppo  RAI - che  dichiara di conoscere globalmente " +
                //        "e nelle sue singole parti, avendone  presa completa e  piena  visione su base " +
                //        "cartacea e / o attraverso collegamento telematico al sito internet www.rai.it/trasparenza, " +
                //        "nell’area tematica “Governance” - si impegna, per tutta la durata del presente " +
                //        "contratto, ad attenersi al Codice stesso, osservando un comportamento ad esso " +
                //        "pienamente conforme e che non  risulti lesivo dell'immagine e, comunque, dei " +
                //        "valori morali e  materiali in cui il Gruppo RAI si riconosce e che applica nell'esercizio " +
                //        "della propria attività, anche con riferimento ai rapporti con i terzi.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par17, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par18 = String.Format("Ella, inoltre, espressamente dichiara di aver ricevuto copia " +
                //        "cartacea e  di  aver  recepito  i  contenuti  del  Manuale  informativo per  la Sicurezza, " +
                //        "nella versione predisposta dalla RAI medesima in  relazione alle  sue  specificità. Tale " +
                //        "documento,  anche  in riferimento  ad eventuali possibili aggiornamenti, è altresì " +
                //        "disponibile in formato elettronico attraverso  collegamento  telematico al sito " +
                //        "intranet http://www.raiplace.rai.it, nell’area tematica “Norme e Procedure” – “Salute, " +
                //        "Sicurezza e Ambiente”.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par18, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par19 = String.Format("Ella si impegna, ove tale adempimento non sia da Lei già stato " +
                //        "documentatamente  effettuato, a fruire, all'atto della Sua immissione in servizio e " +
                //        "preventivamente all'inizio della sua   mansione lavorativa,  del Corso Multimediale " +
                //        "sulla sicurezza nei luoghi di lavoro. Tale corso, della durata indicativa di 6 ore, " +
                //        "ha l'obiettivo di erogare e certificare, previa verifica finale  dell'apprendimento, " +
                //        "la formazione  di  base  sui  rischi specifici connessi al Suo profilo professionale. " +
                //        "La fruizione del  corso  è  possibile  da   qualsiasi postazione di lavoro informatica " +
                //        "connessa alla rete aziendale, sul sito intranet http://www.raiplace.rai.it, nell’area " +
                //        "tematica “RAI ACADEMY” – “Calendario Corsi”.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par19, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par20 = String.Format("Ella, altresì, espressamente  dichiara  di conoscere - " +
                //        "avendone presa completa e piena  visione su base cartacea e/o attraverso collegamento " +
                //        "telematico al sito  internet  www.rai.it/trasparenza, nell’area tematica  " +
                //        "“Governance” -  il Modello Organizzativo RAI ex d.lgs. 231/2001 sulla responsabilità  " +
                //        "amministrativa da reato degli enti - nella versione  messa a disposizione dalla Rai " +
                //        " medesima  -  e  si  impegna  a  tenere,  nell' esecuzione  delle prestazioni, " +
                //        "comportamenti in linea con i principi  contenuti in detto Modello Organizzativo \"231\".");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par20, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par21 = String.Format("Ad ogni effetto di legge, Le comunichiamo che abbiamo inserito il " +
                //        "Suo nominativo nel Libro Unico del Lavoro di cui all'art. 39 del D.L. 25 giugno 2008, n. 112, " +
                //        "convertito nella L. n. 133/2008 e che, con la consegna di copia del presente contratto, " +
                //        "l'Azienda intende assolvere agli obblighi comunicativi di cui al D.Lgs. 26 maggio 1997, " +
                //        "n. 152.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par21, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par22 = String.Format("Ella prende atto che l’Azienda non è tenuta a riconoscere e non " +
                //        "riconoscerà qualsivoglia modifica del presente contratto che non sia stata formalizzata " +
                //        "per iscritto e sottoscritta da un procuratore dell’Azienda medesima munito dei " +
                //        "relativi poteri. ");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par22, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par23 = String.Format("Con la sottoscrizione del presente contratto, Lei si impegna a " +
                //        "compilare e restituire completi in tutte le loro parti alle competenti strutture di" +
                //        " gestione del Personale i formulari di  dichiarazione che Le saranno sottoposti al fine " +
                //        "di acquisire le informazioni  essenziali per la corretta gestione del Suo rapporto di " +
                //        "lavoro, quali, a titolo meramente esemplificativo e non esaustivo, la dichiarazione " +
                //        "sulla spettanza delle detrazioni d'imposta, la dichiarazione circa l'anzianità " +
                //        "assicurativa e contributiva posseduta, la modulistica ai fini della scelta circa la " +
                //        "destinazione del TFR (ove necessaria).");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par23, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par24 = String.Format("Nel restituire copia della presente da Lei datata e sottoscritta " +
                //        "in segno di integrale accettazione di quanto ivi contenuto, La invitiamo, inoltre, " +
                //        "a consegnare i seguenti documenti:");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par24, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    tableDetail = new PdfPTable(2);
                //    tableDetail.DefaultCell.BorderWidth = 0;
                //    tableDetail.TotalWidth = 550;
                //    tableDetail.LockedWidth = true;
                //    tableDetailWidth = new int[] { 50, 500 };
                //    tableDetail.SetWidths(tableDetailWidth);

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("eventuale libretto di iscrizione alla Gestione ex " +
                //        "ENPALS dell’INPS ove da Lei posseduto in relazione a precedenti rapporti " +
                //        "di lavoro nello spettacolo;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("certificati di nascita, cittadinanza, residenza e stato " +
                //        "di famiglia con eventuale autorizzazione per assegni familiari;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("titoli di studio;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("copia documento di identità;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("codice fiscale;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("gli altri documenti eventualmente necessari per perfezionare " +
                //        "l'assunzione di cui sopra, ivi inclusi, se del caso, quelli utili al godimento " +
                //        "delle agevolazioni normative e/o contributive di legge.", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    document.Add(tableDetail);
                //    tableDetail.FlushContent();
                //    tableDetail.DeleteBodyRows();

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par25 = String.Format("Cordiali Saluti");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par25, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    //phrase = new Phrase();
                //    //phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    //p = new Paragraph(phrase);
                //    //((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    //document.Add(p);

                //    //phrase = new Phrase();
                //    //phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    //p = new Paragraph(phrase);
                //    //((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    //document.Add(p);                    

                //    //PdfPTable tableFirma = new PdfPTable(4);
                //    //tableFirma.DefaultCell.BorderWidth = 0;
                //    //tableFirma.TotalWidth = 550;
                //    //tableFirma.LockedWidth = true;
                //    //var tableFirmaWidth = new int[] { 137, 137, 137, 137 };
                //    //tableFirma.SetWidths(tableFirmaWidth);
                //    //tableFirma.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 1, myFontCorpo, 0));
                //    //tableFirma.AddCell(WriteCellsClass.WriteCell("FIRMA", 0, 2, 1, myFontCorpo, 0));

                //    //document.Add(tableFirma);
                //    //tableFirma.FlushContent();
                //    //tableFirma.DeleteBodyRows();

                //    document.Close();
                //    writer.Close();
                //    bytes = ms.ToArray();
                //    return bytes;
                //}
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static byte[] GeneraPdf_Bozza_Apprendistato_Specializzato_Produzione(AssunzioniVM assunzione)
        {
            try
            {
                //var cultureInfo = CultureInfo.GetCultureInfo("it-IT");

                IncentiviEntities db = new IncentiviEntities();
                byte[] bytes = null;
                //const int fontIntestazione = 12;
                //const int fontCorpo = 12;
                //const string FONTNAME = "Calibri"; //"Times-Roman"
                //const string SPAZIO = " ";

                //BaseColor color = new BaseColor(System.Drawing.Color.Black);
                //Font myFontIntestazione = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.NORMAL, color);
                //Font myFontIntestazioneBold = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.BOLD, color);
                //Font myFontIntestazioneBoldItalic = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.BOLDITALIC, color);
                //Font myFontCorpo = FontFactory.GetFont(FONTNAME, fontCorpo, Font.NORMAL, color);
                //Font myFontCorpoBold = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.BOLD, color);
                //Font myFontCorpoSottolineato = FontFactory.GetFont(FONTNAME, fontCorpo, Font.UNDERLINE, color);
                //Font myFontCorpoItalic = FontFactory.GetFont(FONTNAME, fontCorpo, Font.ITALIC, color);
                //Font myFontInterlinea = FontFactory.GetFont(FONTNAME, 7, Font.NORMAL, color);

                string sesso = assunzione.Genere.GetValueOrDefault().ToString();
                string nominativo = String.Format("{0} {1}", assunzione.Cognome.Trim(), assunzione.Nome.Trim());
                string mansione = GetRuoloByCode(assunzione.SelectedMansione);
                string categoria = GetCategoriaByCode(assunzione.SelectedCategoria);
                string ruolo = GetRuoloByCategoria(assunzione.SelectedCategoria);
                string sede = GetSedeByCode(assunzione.SelectedSede);
                string direzione = GetDirezioneByCode(assunzione.SelectedServizio);
                string stipendio = GetStipendio(assunzione.SelectedCategoria, assunzione.SelectedTipoMinimo);

                DateTime fineFormazione = DateTime.Today;

                var xx = db.XR_HRIS_TEMPLATE.Where(x => x.NME_TEMPLATE == "BOZZA ASSUNZIONE APPRENDISTATO SPECIALIZZATO PRODUZIONE").FirstOrDefault();
                if (xx != null)
                {
                    Stream stream = new MemoryStream(xx.TEMPLATE);
                    DocX doc = DocX.Load(stream);
                    doc.ReplaceText("#GENERE#", sesso.ToUpper().Equals("F") ? "Sig.ra" : "Sig");
                    doc.ReplaceText("#DATAODIERNA#", DateTime.Today.ToString("dd/MM/yyyy"));
                    doc.ReplaceText("#NOMINATIVO#", nominativo);
                    doc.ReplaceText("#DATAINIZIO#", assunzione.DataInizio.ToString("dd/MM/yyyy"));
                    doc.ReplaceText("#MANSIONE#", mansione);
                    doc.ReplaceText("#CATEGORIA#", categoria);
                    doc.ReplaceText("#DIREZIONE#", direzione);
                    doc.ReplaceText("#SEDE#", sede);
                    doc.ReplaceText("#STIPENDIOCIFRA#", stipendio);
                    doc.ReplaceText("#STIPENDIO#", stipendio);
                    MemoryStream ms = new MemoryStream();
                    doc.SaveAs(ms);

                    bytes = ms.GetBuffer();
                }

                return bytes;

                //using (MemoryStream ms = new MemoryStream())
                //{
                //    string _imgPath = System.Web.HttpContext.Current.Server.MapPath("~/assets/img/LogoPDF.png");
                //    iTextSharp.text.Image png = iTextSharp.text.Image.GetInstance(_imgPath);
                //    string codSocieta = "";

                //    // verifica la società di appartenenza per caricare il logo adatto
                //    using (IncentiviEntities sdb = new IncentiviEntities())
                //    {
                //        var item = sdb.SINTESI1.Where(w => w.COD_MATLIBROMAT.Equals(assunzione.Matricola)).FirstOrDefault();

                //        if (item != null)
                //        {
                //            codSocieta = item.COD_IMPRESACR;
                //        }
                //    }

                //    // Creazione dell'istanza del documento, impostando tipo di pagina e margini
                //    Document document = new Document(PageSize.A4, 60, 60, 60, 80);
                //    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                //    writer.PageEvent = new AssunzioniManagerPdfPageEventHelper(codSocieta);
                //    document.Open();
                //    PdfContentByte cb = writer.DirectContent;
                //    Paragraph p = null;

                //    Phrase phrase = new Phrase();
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    //phrase.Add(new Chunk("RUO/GSR/6340/22/", myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();

                //    if (sesso.ToUpper().Equals("F"))
                //    {
                //        phrase.Add(new Chunk("Sig.ra ", myFontCorpo));
                //    }
                //    else
                //    {
                //        phrase.Add(new Chunk("Sig. ", myFontCorpo));
                //    }

                //    phrase.Add(new Chunk(nominativo, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_RIGHT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk("ROMA", myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_RIGHT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk("Roma, " + DateTime.Today.ToString("dd/MM/yyyy"), myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk("Assunzione in servizio", myFontCorpoSottolineato));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par1 = String.Format("Le comunichiamo che, a decorrere dal {0}, Ella viene " +
                //        "assunta presso la RAI-Radiotelevisione italiana – Direzione {1} – sede di {2}, " +
                //        "con contratto di apprendistato professionalizzante, finalizzato al conseguimento " +
                //        "della qualifica di “specializzato della produzione” di livello 8 (otto).",
                //        assunzione.DataInizio.ToString("dd/MM/yyyy"),
                //        direzione,
                //        sede,
                //        mansione);
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par1, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par2 = String.Format("Per quanto non espressamente previsto nella presente lettera, " +
                //        "si rinvia alle disposizioni di cui agli artt. 41 e ss. del D. Lgs. 15 giugno 2015, " +
                //        "n. 81 nonché, in particolare per quanto riguarda l’orario di lavoro e le ferie, alle " +
                //        "previsioni del vigente Contratto Collettivo di Lavoro per Quadri Impiegati ed Operai " +
                //        "dipendenti dalla RAI e dalle Società del Gruppo, contratto del quale Ella ha preso " +
                //        "visione e che dichiara di accettare senza riserva alcuna.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par2, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par3 = String.Format("L’assunzione è subordinata al positivo superamento di un periodo " +
                //        "di prova di 45 giorni, durante i quali ciascuna delle parti sarà libera di recedere dal " +
                //        "contratto senza obbligo di preavviso né corresponsione di alcuna indennità.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par3, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par3Bis = String.Format("Le precisiamo che, in relazione alla natura delle Sue mansioni, " +
                //        "Ella sarà tenuta a guidare gli automezzi sociali, per lo svolgimento dei compiti affidatiLe.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par3Bis, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par4 = String.Format("Il periodo di formazione avrà la durata di 30 mesi ed il percorso " +
                //        "formativo si concluderà il {0} (2 anni e 6 mesi dalla data di assunzione).", fineFormazione.ToString("dd/MM/yyyy"));
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par4, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par5 = String.Format("Nel caso in cui sopraggiungessero assenze di significativa durata " +
                //        "che possano influire sul rispetto del piano formativo originariamente concordato, si " +
                //        "applicano le disposizioni in materia di slittamento del termine finale del contratto di " +
                //        "cui all’ “articolo 10 - Apprendistato professionalizzante” - dell’Accordo Collettivo del " +
                //        "7/2/2013.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par5, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par6 = String.Format("Costituisce parte integrante del presente contratto il piano " +
                //        "formativo individuale, avente ad oggetto i contenuti, l’articolazione e le modalità di " +
                //        "erogazione della formazione c.d. professionalizzante.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par6, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par7 = String.Format("In relazione a quanto previsto dal vigente C.C.L. " +
                //        "in materia di apprendistato professionalizzante, Ella sarà inquadrata al livello 9 " +
                //        "(nove) in qualità di “specializzato della produzione”.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par7, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par9 = String.Format("Al termine del periodo di formazione, in base agli esiti " +
                //        "della formazione svolta e valutate le competenze acquisite, Ella conseguirà la " +
                //        "qualificazione nel profilo professionale di “specializzato della produzione” " +
                //        "di livello 8 (otto).", mansione);
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par9, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par10 = String.Format("Durante il periodo di formazione, a titolo di retribuzione, in " +
                //        "aggiunta all’indennità di contingenza nella misura prevista dalle disposizioni vigenti, " +
                //        "Le verrà corrisposto uno stipendio di Euro 750,40 (settecentocinquanta/40) lordi mensili, " +
                //        "quale minimo previsto dal Contratto Collettivo di Lavoro per il livello 9.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par10, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par11 = String.Format("Le verrà corrisposta, altresì, l’indennità mancata limitazione " +
                //        "orario di lavoro, di cui all’articolo 34 del citato Contratto Collettivo di Lavoro, " +
                //        "nella misura dell’8% dello stipendio individuale e dell’indennità di contingenza.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par11, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par12 = String.Format("Al termine del periodo di formazione, ciascuna delle parti potrà " +
                //        "recedere dal contratto dandone comunicazione scritta e osservando un preavviso di almeno " +
                //        "15 (quindici) giorni decorrente dalla suddetta scadenza. Diversamente, il rapporto " +
                //        "proseguirà tra le parti come ordinario rapporto di lavoro subordinato a tempo indeterminato " +
                //        "senza soluzione di continuità.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par12, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par12Bis = String.Format("In tal caso, al termine del periodo di formazione, " +
                //        "in aggiunta all’indennità di contingenza, Le verrà corrisposto uno stipendio di " +
                //        "Euro 845,78 (ottocentoquarantacinque/78) lordi mensili quale minimo previsto dal " +
                //        "richiamato C.C.L. per il livello 8 (otto).");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par12Bis, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par13 = String.Format("Si precisa che gli importi dei minimi di stipendio sopra " +
                //        "indicati potranno subire variazioni in caso di rinnovo del vigente C.C.L.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par13, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par14 = String.Format("La retribuzione prevista in contratto costituisce esclusivamente " +
                //        "la remunerazione concordata per la Sua prestazione lavorativa. Con specifico riferimento " +
                //        "alla Normativa in materia di anticorruzione, Lei dichiara e garantisce che, nello " +
                //        "svolgimento della Sua prestazione lavorativa, non terrà, direttamente o indirettamente, " +
                //        "comportamenti, omissivi e/o commissivi, che possano comportare la violazione della " +
                //        "normativa anticorruzione (per quanto applicabile a RAI medesima) e/o del Piano Triennale " +
                //        "di Prevenzione della Corruzione (e protocolli nello stesso previsti) adottato da RAI " +
                //        "ai sensi della legge n.190/2012, di cui è tenuto ad acquisire conoscenza sul sito intranet " +
                //        "della RAI (http://www.raiplace.rai.it), nell’area tematica “Norme e Procedure” – " +
                //        "“Anticorruzione”.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par14, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par15 = String.Format("Ai sensi dell’art. 13 del Regolamento UE 2016/679 " +
                //        "(Regolamento Generale sulla protezione dei dati personali), La informiamo che i " +
                //        "dati personali da Lei forniti verranno utilizzati da Rai-Radiotelevisione italiana " +
                //        "Spa (titolare del trattamento) – anche tramite collaboratori esterni - con modalità " +
                //        "manuali, informatiche e telematiche, a mezzo inserimento in banche dati gestite dalla " +
                //        "Azienda Rai medesima. ");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par15, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par16 = String.Format("In particolare, unitamente alla stipula del presente contratto, " +
                //        "Le viene consegnata l’informativa di cui al  richiamato art. 13 Regolamento UE 2016/679, " +
                //        "che ne costituisce parte integrante. Ella dovrà aver cura di leggerla attentamente e " +
                //        "di restituircela debitamente datata e sottoscritta per presa visione.Ad ogni buon conto, " +
                //        "sul sito  intranet  della  Rai(http://www.raiplace.rai.it, nell’area tematica " +
                //        "“Norme e Procedure” – “Privacy”) troverà comunque pubblicato il testo dell’informativa " +
                //        "in questione.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par16, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par17 = String.Format("Ella, preso atto dei Principi etici generali di onestà e " +
                //        "osservanza della legge, pluralismo, professionalità, imparzialità, correttezza, " +
                //        "riservatezza, trasparenza, diligenza, lealtà e buona  fede nonché  del contenuto  " +
                //        "tutto del  Codice etico del  Gruppo  RAI - che  dichiara di conoscere globalmente " +
                //        "e nelle sue singole parti, avendone  presa completa e  piena  visione su base " +
                //        "cartacea e / o attraverso collegamento telematico al sito internet www.rai.it/trasparenza, " +
                //        "nell’area tematica “Governance” - si impegna, per tutta la durata del presente " +
                //        "contratto, ad attenersi al Codice stesso, osservando un comportamento ad esso " +
                //        "pienamente conforme e che non  risulti lesivo dell'immagine e, comunque, dei " +
                //        "valori morali e  materiali in cui il Gruppo RAI si riconosce e che applica nell'esercizio " +
                //        "della propria attività, anche con riferimento ai rapporti con i terzi.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par17, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par18 = String.Format("Ella, inoltre, espressamente dichiara di aver ricevuto copia " +
                //        "cartacea e  di  aver  recepito  i  contenuti  del  Manuale  informativo per  la Sicurezza, " +
                //        "nella versione predisposta dalla RAI medesima in  relazione alle  sue  specificità. Tale " +
                //        "documento,  anche  in riferimento  ad eventuali possibili aggiornamenti, è altresì " +
                //        "disponibile in formato elettronico attraverso  collegamento  telematico al sito " +
                //        "intranet http://www.raiplace.rai.it, nell’area tematica “Norme e Procedure” – “Salute, " +
                //        "Sicurezza e Ambiente – Tutela delle persone”.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par18, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par19 = String.Format("Ella si impegna, ove tale adempimento non sia da Lei già stato " +
                //        "documentatamente  effettuato, a fruire, all'atto della Sua immissione in servizio e " +
                //        "preventivamente all'inizio della sua   mansione lavorativa,  del Corso Multimediale " +
                //        "sulla sicurezza nei luoghi di lavoro. Tale corso, della durata indicativa di 6 ore, " +
                //        "ha l'obiettivo di erogare e certificare, previa verifica finale  dell'apprendimento, " +
                //        "la formazione  di  base  sui  rischi specifici connessi al Suo profilo professionale. " +
                //        "La fruizione del  corso  è  possibile  da   qualsiasi postazione di lavoro informatica " +
                //        "connessa alla rete aziendale, sul sito intranet http://www.raiplace.rai.it, nell’area " +
                //        "tematica “RAI ACADEMY” – “Calendario Corsi”.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par19, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par20 = String.Format("Ella, altresì, espressamente  dichiara  di conoscere - " +
                //        "avendone presa completa e piena  visione su base cartacea e/o attraverso collegamento " +
                //        "telematico al sito  internet  www.rai.it/trasparenza, nell’area tematica  " +
                //        "“Governance” -  il Modello Organizzativo RAI ex d.lgs. 231/2001 sulla responsabilità  " +
                //        "amministrativa da reato degli enti - nella versione  messa a disposizione dalla Rai " +
                //        " medesima  -  e  si  impegna  a  tenere,  nell' esecuzione  delle prestazioni, " +
                //        "comportamenti in linea con i principi  contenuti in detto Modello Organizzativo \"231\".");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par20, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par21 = String.Format("Ad ogni effetto di legge, Le comunichiamo che abbiamo inserito il " +
                //        "Suo nominativo nel Libro Unico del Lavoro di cui all'art. 39 del D.L. 25 giugno 2008, n. 112, " +
                //        "convertito nella L. n. 133/2008 e che, con la consegna di copia del presente contratto, " +
                //        "l'Azienda intende assolvere agli obblighi comunicativi di cui al D.Lgs. 26 maggio 1997, " +
                //        "n. 152.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par21, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par22 = String.Format("Ella prende atto che l’Azienda non è tenuta a riconoscere e non " +
                //        "riconoscerà qualsivoglia modifica del presente contratto che non sia stata formalizzata " +
                //        "per iscritto e sottoscritta da un procuratore dell’Azienda medesima munito dei " +
                //        "relativi poteri. ");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par22, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par23 = String.Format("Con la sottoscrizione del presente contratto, Lei si impegna a " +
                //        "compilare e restituire completi in tutte le loro parti alle competenti strutture di" +
                //        " gestione del Personale i formulari di  dichiarazione che Le saranno sottoposti al fine " +
                //        "di acquisire le informazioni  essenziali per la corretta gestione del Suo rapporto di " +
                //        "lavoro, quali, a titolo meramente esemplificativo e non esaustivo, la dichiarazione " +
                //        "sulla spettanza delle detrazioni d'imposta, la dichiarazione circa l'anzianità " +
                //        "assicurativa e contributiva posseduta, la modulistica ai fini della scelta circa la " +
                //        "destinazione del TFR (ove necessaria).");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par23, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par24 = String.Format("Nel restituire copia della presente da Lei datata e sottoscritta " +
                //        "in segno di integrale accettazione di quanto ivi contenuto, La invitiamo, inoltre, " +
                //        "a consegnare i seguenti documenti:");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par24, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    PdfPTable tableDetail = new PdfPTable(2);
                //    tableDetail.DefaultCell.BorderWidth = 0;
                //    tableDetail.TotalWidth = 550;
                //    tableDetail.LockedWidth = true;
                //    var tableDetailWidth = new int[] { 50, 500 };
                //    tableDetail.SetWidths(tableDetailWidth);

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    Phrase cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("eventuale libretto di iscrizione alla Gestione ex " +
                //        "ENPALS dell’INPS ove da Lei posseduto in relazione a precedenti rapporti " +
                //        "di lavoro nello spettacolo;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("certificati di nascita, cittadinanza, residenza e stato " +
                //        "di famiglia con eventuale autorizzazione per assegni familiari;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("titoli di studio;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("copia documento di identità;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("codice fiscale;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("gli altri documenti eventualmente necessari per perfezionare " +
                //        "l'assunzione di cui sopra, ivi inclusi, se del caso, quelli utili al godimento " +
                //        "delle agevolazioni normative e/o contributive di legge.", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    document.Add(tableDetail);
                //    tableDetail.FlushContent();
                //    tableDetail.DeleteBodyRows();

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par25 = String.Format("Cordiali Saluti");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par25, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    //phrase = new Phrase();
                //    //phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    //p = new Paragraph(phrase);
                //    //((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    //document.Add(p);

                //    //phrase = new Phrase();
                //    //phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    //p = new Paragraph(phrase);
                //    //((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    //document.Add(p);                    

                //    //PdfPTable tableFirma = new PdfPTable(4);
                //    //tableFirma.DefaultCell.BorderWidth = 0;
                //    //tableFirma.TotalWidth = 550;
                //    //tableFirma.LockedWidth = true;
                //    //var tableFirmaWidth = new int[] { 137, 137, 137, 137 };
                //    //tableFirma.SetWidths(tableFirmaWidth);
                //    //tableFirma.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 1, myFontCorpo, 0));
                //    //tableFirma.AddCell(WriteCellsClass.WriteCell("FIRMA", 0, 2, 1, myFontCorpo, 0));

                //    //document.Add(tableFirma);
                //    //tableFirma.FlushContent();
                //    //tableFirma.DeleteBodyRows();

                //    document.Close();
                //    writer.Close();
                //    bytes = ms.ToArray();
                //    return bytes;
                //}
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static byte[] GeneraPdf_Bozza_Apprendistato_Tecnico_Laureato(AssunzioniVM assunzione)
        {
            try
            {
                //var cultureInfo = CultureInfo.GetCultureInfo("it-IT");

                //byte[] bytes = null;
                //const int fontIntestazione = 12;
                //const int fontCorpo = 12;
                //const string FONTNAME = "Calibri"; //"Times-Roman"
                //const string SPAZIO = " ";

                //BaseColor color = new BaseColor(System.Drawing.Color.Black);
                //Font myFontIntestazione = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.NORMAL, color);
                //Font myFontIntestazioneBold = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.BOLD, color);
                //Font myFontIntestazioneBoldItalic = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.BOLDITALIC, color);
                //Font myFontCorpo = FontFactory.GetFont(FONTNAME, fontCorpo, Font.NORMAL, color);
                //Font myFontCorpoBold = FontFactory.GetFont(FONTNAME, fontIntestazione, Font.BOLD, color);
                //Font myFontCorpoSottolineato = FontFactory.GetFont(FONTNAME, fontCorpo, Font.UNDERLINE, color);
                //Font myFontCorpoItalic = FontFactory.GetFont(FONTNAME, fontCorpo, Font.ITALIC, color);
                //Font myFontInterlinea = FontFactory.GetFont(FONTNAME, 7, Font.NORMAL, color);

                //string sesso = assunzione.Genere.GetValueOrDefault().ToString();
                //string nominativo = String.Format("{0} {1}", assunzione.Cognome.Trim(), assunzione.Nome.Trim());
                //string mansione = GetRuoloByCode(assunzione.SelectedMansione);
                //string categoria = GetCategoriaByCode(assunzione.SelectedCategoria);
                //string ruolo = GetRuoloByCategoria(assunzione.SelectedCategoria);
                //string sede = GetSedeByCode(assunzione.SelectedSede);
                //string direzione = GetDirezioneByCode(assunzione.SelectedServizio);


                IncentiviEntities db = new IncentiviEntities();
                byte[] bytes = null;

                string sesso = assunzione.Genere.GetValueOrDefault().ToString();
                string nominativo = String.Format("{0} {1}", assunzione.Cognome.Trim(), assunzione.Nome.Trim());
                string mansione = GetRuoloByCode(assunzione.SelectedMansione);
                string categoria = GetCategoriaByCode(assunzione.SelectedCategoria);
                string ruolo = GetRuoloByCategoria(assunzione.SelectedCategoria);
                string sede = GetSedeByCode(assunzione.SelectedSede);
                string direzione = GetDirezioneByCode(assunzione.SelectedServizio);
                string stipendio = GetStipendio(assunzione.SelectedCategoria, assunzione.SelectedTipoMinimo);

                DateTime fineFormazione = DateTime.Today;

                var xx = db.XR_HRIS_TEMPLATE.Where(x => x.NME_TEMPLATE == "BOZZA ASSUNZIONE APPRENDISTATO TECNICO LAUREATO").FirstOrDefault();
                if (xx != null)
                {
                    Stream stream = new MemoryStream(xx.TEMPLATE);
                    DocX doc = DocX.Load(stream);
                    doc.ReplaceText("#GENERE#", sesso.ToUpper().Equals("F") ? "Sig.ra" : "Sig");
                    doc.ReplaceText("#DATAODIERNA#", DateTime.Today.ToString("dd/MM/yyyy"));
                    doc.ReplaceText("#NOMINATIVO#", nominativo);
                    doc.ReplaceText("#DATAINIZIO#", assunzione.DataInizio.ToString("dd/MM/yyyy"));
                    doc.ReplaceText("#MANSIONE#", mansione);
                    doc.ReplaceText("#CATEGORIA#", categoria);
                    doc.ReplaceText("#DIREZIONE#", direzione);
                    doc.ReplaceText("#SEDE#", sede);
                    doc.ReplaceText("#STIPENDIOCIFRA#", stipendio);
                    doc.ReplaceText("#STIPENDIO#", stipendio);
                    MemoryStream ms = new MemoryStream();
                    doc.SaveAs(ms);

                    bytes = ms.GetBuffer();
                }

                return bytes;

                //using (MemoryStream ms = new MemoryStream())
                //{
                //    string _imgPath = System.Web.HttpContext.Current.Server.MapPath("~/assets/img/LogoPDF.png");
                //    iTextSharp.text.Image png = iTextSharp.text.Image.GetInstance(_imgPath);
                //    string codSocieta = "";

                //    // verifica la società di appartenenza per caricare il logo adatto
                //    using (IncentiviEntities sdb = new IncentiviEntities())
                //    {
                //        var item = sdb.SINTESI1.Where(w => w.COD_MATLIBROMAT.Equals(assunzione.Matricola)).FirstOrDefault();

                //        if (item != null)
                //        {
                //            codSocieta = item.COD_IMPRESACR;
                //        }
                //    }

                //    // Creazione dell'istanza del documento, impostando tipo di pagina e margini
                //    Document document = new Document(PageSize.A4, 60, 60, 60, 80);
                //    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                //    writer.PageEvent = new AssunzioniManagerPdfPageEventHelper(codSocieta);
                //    document.Open();
                //    PdfContentByte cb = writer.DirectContent;
                //    Paragraph p = null;

                //    Phrase phrase = new Phrase();
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    //phrase.Add(new Chunk("RUO/GSR/6340/22/", myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontIntestazioneBold));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();

                //    if (sesso.ToUpper().Equals("F"))
                //    {
                //        phrase.Add(new Chunk("Sig.ra ", myFontCorpo));
                //    }
                //    else
                //    {
                //        phrase.Add(new Chunk("Sig. ", myFontCorpo));
                //    }

                //    phrase.Add(new Chunk(nominativo, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_RIGHT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk("ROMA", myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_RIGHT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk("Roma, " + DateTime.Today.ToString("dd/MM/yyyy"), myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk("Assunzione in servizio", myFontCorpoSottolineato));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par1 = String.Format("Le comunichiamo che, a decorrere dal {0}, Ella viene assunta, " +
                //        "presso la RAI-Radiotelevisione italiana – Direzione {1} – sede di {2}, con contratto " +
                //        "di apprendistato professionalizzante finalizzato al conseguimento della qualificazione " +
                //        "nel profilo professionale di “{3}” di livello 2 (due).",
                //        assunzione.DataInizio.ToString("dd/MM/yyyy"),
                //        direzione,
                //        sede,
                //        mansione);
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par1, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par2 = String.Format("Per quanto non espressamente previsto nella presente lettera, " +
                //        "si rinvia alle disposizioni di cui agli artt. 41 e ss. del D. Lgs. 15 giugno 2015, " +
                //        "n. 81 nonché, in particolare per quanto riguarda l’orario di lavoro e le ferie, alle " +
                //        "previsioni del vigente Contratto Collettivo di Lavoro per Quadri Impiegati ed Operai " +
                //        "dipendenti dalla RAI e dalle Società del Gruppo, contratto del quale Ella ha preso " +
                //        "visione e che dichiara di accettare senza riserva alcuna.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par2, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par3 = String.Format("L’assunzione è subordinata al positivo superamento di un " +
                //        "periodo di prova di 90 giorni, durante i quali ciascuna delle parti sarà libera di " +
                //        "recedere dal contratto senza obbligo di preavviso né corresponsione di alcuna indennità.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par3, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par3Bis = String.Format("Le precisiamo che, in relazione alla natura delle Sue mansioni, " +
                //        "qualora richiestoLe, Ella sarà tenuta a guidare gli automezzi sociali, per lo svolgimento " +
                //        "dei compiti affidatiLe.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par3Bis, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par4 = String.Format("Il periodo di formazione avrà la durata di 36 mesi ed il percorso " +
                //        "formativo si concluderà il          .");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par4, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par5 = String.Format("Nel caso in cui sopraggiungessero assenze di significativa durata " +
                //        "che possano influire sul rispetto del piano formativo originariamente concordato, si " +
                //        "applicano le disposizioni in materia di slittamento del termine finale del contratto di " +
                //        "cui all’ “articolo 10 - Apprendistato professionalizzante” - dell’Accordo Collettivo del " +
                //        "7/2/2013.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par5, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par6 = String.Format("Costituisce parte integrante del presente contratto il piano " +
                //        "formativo individuale, avente ad oggetto i contenuti, l’articolazione e le modalità di " +
                //        "erogazione della formazione c.d. professionalizzante.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par6, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par7 = String.Format("Ella sarà inizialmente inquadrata al livello 4 (quattro) in " +
                //        "qualità di “{0}”.", mansione);
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par7, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par8 = String.Format("Decorsi 24 mesi dall’inizio del rapporto e per i successivi 12, " +
                //        "Ella sarà invece inquadrata al livello 3 (tre) del medesimo profilo professionale.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par8, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par9 = String.Format("Al termine del periodo di formazione, in base agli esiti della " +
                //        "formazione svolta e valutate le competenze acquisite, Ella conseguirà la qualificazione " +
                //        "nel profilo professionale di “{0}” di livello 2 (due).", mansione);
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par9, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par10 = String.Format("Durante il periodo di formazione, a titolo di retribuzione, " +
                //        "in aggiunta all’indennità di contingenza nella misura prevista dalle disposizioni " +
                //        "vigenti, Le verrà corrisposto uno stipendio di:");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par10, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    PdfPTable tableDetail = new PdfPTable(2);
                //    tableDetail.DefaultCell.BorderWidth = 0;
                //    tableDetail.TotalWidth = 550;
                //    tableDetail.LockedWidth = true;
                //    var tableDetailWidth = new int[] { 50, 500 };
                //    tableDetail.SetWidths(tableDetailWidth);

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    Phrase cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("Euro 1.207,93 (milleduecentosette/93) lordi mensili per i primi " +
                //        "24 mesi, quale minimo previsto dal Contratto Collettivo di Lavoro per il livello 4 " +
                //        "(quattro); ", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("a decorrere dal             e per i successivi 12 mesi, uno stipendio di " +
                //        "Euro 1.303,18 (milletrecentotre/18) lordi mensili, quale minimo previsto dal richiamato " +
                //        "C.C.L. per il livello 3 (tre);", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("un importo “ad personam” di Euro 450,00 (quattrocentocinquanta/00) " +
                //        "lordi mensili, assorbibile con l’assegnazione del compenso previsto al punto 3 della " +
                //        "nota a verbale in calce all’art. 35 del vigente C.C.L.;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("a decorrere dal             un forfait straordinari di Euro 333,34 " +
                //        "(trecentotrentatre/34) lordi mensili, a compenso del lavoro straordinario feriale, " +
                //        "notturno, domenicale e festivo che Ella sia eventualmente tenuta ad effettuare.", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    document.Add(tableDetail);
                //    tableDetail.FlushContent();
                //    tableDetail.DeleteBodyRows();

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par11 = String.Format("Al termine del periodo di formazione, ciascuna delle parti " +
                //        "potrà recedere dal contratto dandone comunicazione scritta e osservando un preavviso " +
                //        "di almeno 15 (quindici) giorni decorrente dalla suddetta scadenza. Diversamente, il " +
                //        "rapporto proseguirà tra le parti come ordinario rapporto di lavoro subordinato a " +
                //        "tempo indeterminato senza soluzione di continuità.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par11, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par12 = String.Format("In tal caso, al termine del periodo di formazione, " +
                //        "in aggiunta all’indennità di contingenza, Le verrà corrisposto uno stipendio " +
                //        "di Euro 1.408,58 (millequattrocentootto/58) lordi mensili quale minimo previsto " +
                //        "dal richiamato C.C.L. per il livello 2 ( due) e Le verrà assegnato il compenso, " +
                //        "pari al 8% dello stipendio individuale e dell’indennità di contingenza, previsto" +
                //        " al punto 3 della nota a verbale in calce all’art. 35 del vigente C.C.L..");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par12, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par13 = String.Format("Si precisa che gli importi dei minimi di stipendio sopra " +
                //        "indicati potranno subire variazioni in caso di rinnovo del vigente C.C.L.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par13, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par14 = String.Format("La retribuzione prevista in contratto costituisce esclusivamente " +
                //        "la remunerazione concordata per la Sua prestazione lavorativa. Con specifico riferimento " +
                //        "alla Normativa in materia di anticorruzione, Lei dichiara e garantisce che, nello " +
                //        "svolgimento della Sua prestazione lavorativa, non terrà, direttamente o indirettamente, " +
                //        "comportamenti, omissivi e/o commissivi, che possano comportare la violazione della " +
                //        "normativa anticorruzione (per quanto applicabile a RAI medesima) e/o del Piano Triennale " +
                //        "di Prevenzione della Corruzione (e protocolli nello stesso previsti) adottato da RAI " +
                //        "ai sensi della legge n.190/2012, di cui è tenuto ad acquisire conoscenza sul sito intranet " +
                //        "della RAI (http://www.raiplace.rai.it), nell’area tematica “Norme e Procedure” – " +
                //        "“Anticorruzione”.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par14, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_LEFT;
                //    document.Add(p);

                //    string par15 = String.Format("Ai sensi dell’art. 13 del Regolamento UE 2016/679 " +
                //        "(Regolamento Generale sulla protezione dei dati personali), La informiamo che i " +
                //        "dati personali da Lei forniti verranno utilizzati da Rai-Radiotelevisione italiana " +
                //        "Spa (titolare del trattamento) – anche tramite collaboratori esterni - con modalità " +
                //        "manuali, informatiche e telematiche, a mezzo inserimento in banche dati gestite dalla " +
                //        "Azienda Rai medesima. ");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par15, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par16 = String.Format("In particolare, unitamente alla stipula del presente contratto, " +
                //        "Le viene consegnata l’informativa di cui al  richiamato art. 13 Regolamento UE 2016/679, " +
                //        "che ne costituisce parte integrante. Ella dovrà aver cura di leggerla attentamente e " +
                //        "di restituircela debitamente datata e sottoscritta per presa visione.Ad ogni buon conto, " +
                //        "sul sito  intranet  della  Rai(http://www.raiplace.rai.it, nell’area tematica " +
                //        "“Norme e Procedure” – “Privacy”) troverà comunque pubblicato il testo dell’informativa " +
                //        "in questione.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par16, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par17 = String.Format("Ella, preso atto dei Principi etici generali di onestà e " +
                //        "osservanza della legge, pluralismo, professionalità, imparzialità, correttezza, " +
                //        "riservatezza, trasparenza, diligenza, lealtà e buona  fede nonché  del contenuto  " +
                //        "tutto del  Codice etico del  Gruppo  RAI - che  dichiara di conoscere globalmente " +
                //        "e nelle sue singole parti, avendone  presa completa e  piena  visione su base " +
                //        "cartacea e / o attraverso collegamento telematico al sito internet www.rai.it/trasparenza, " +
                //        "nell’area tematica “Governance” - si impegna, per tutta la durata del presente " +
                //        "contratto, ad attenersi al Codice stesso, osservando un comportamento ad esso " +
                //        "pienamente conforme e che non  risulti lesivo dell'immagine e, comunque, dei " +
                //        "valori morali e  materiali in cui il Gruppo RAI si riconosce e che applica nell'esercizio " +
                //        "della propria attività, anche con riferimento ai rapporti con i terzi.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par17, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par18 = String.Format("Ella, inoltre, espressamente dichiara di aver ricevuto copia " +
                //        "cartacea e  di  aver  recepito  i  contenuti  del  Manuale  informativo per  la Sicurezza, " +
                //        "nella versione predisposta dalla RAI medesima in  relazione alle  sue  specificità. Tale " +
                //        "documento,  anche  in riferimento  ad eventuali possibili aggiornamenti, è altresì " +
                //        "disponibile in formato elettronico attraverso  collegamento  telematico al sito " +
                //        "intranet http://www.raiplace.rai.it, nell’area tematica “Norme e Procedure” – “Salute, " +
                //        "Sicurezza e Ambiente – Tutela delle persone”.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par18, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par19 = String.Format("Ella si impegna, ove tale adempimento non sia da Lei già " +
                //        "stato documentatamente effettuato, a fruire, all'atto della Sua immissione in servizio " +
                //        "e preventivamente all'inizio della sua mansione lavorativa, del Corso Multimediale " +
                //        "sulla sicurezza nei luoghi di lavoro. Tale corso, della durata indicativa di 6 ore, " +
                //        "ha l'obiettivo di erogare e certificare, previa verifica finale dell'apprendimento, " +
                //        "la formazione di base sui rischi specifici connessi al Suo profilo professionale. " +
                //        "La fruizione del corso è possibile da qualsiasi postazione di lavoro informatica " +
                //        "connessa alla rete aziendale, sul sito intranet http://www.raiplace.rai.it, " +
                //        "nell’area tematica “RAI ACADEMY” – “Catalogo Corsi”.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par19, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par20 = String.Format("Ella, altresì, espressamente  dichiara  di conoscere - " +
                //        "avendone presa completa e piena  visione su base cartacea e/o attraverso collegamento " +
                //        "telematico al sito  internet  www.rai.it/trasparenza, nell’area tematica  " +
                //        "“Governance” -  il Modello Organizzativo RAI ex d.lgs. 231/2001 sulla responsabilità  " +
                //        "amministrativa da reato degli enti - nella versione  messa a disposizione dalla Rai " +
                //        " medesima  -  e  si  impegna  a  tenere,  nell' esecuzione  delle prestazioni, " +
                //        "comportamenti in linea con i principi  contenuti in detto Modello Organizzativo \"231\".");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par20, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par21 = String.Format("Ad ogni effetto di legge, Le comunichiamo che abbiamo inserito il " +
                //        "Suo nominativo nel Libro Unico del Lavoro di cui all'art. 39 del D.L. 25 giugno 2008, n. 112, " +
                //        "convertito nella L. n. 133/2008 e che, con la consegna di copia del presente contratto, " +
                //        "l'Azienda intende assolvere agli obblighi comunicativi di cui al D.Lgs. 26 maggio 1997, " +
                //        "n. 152.");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par21, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par22 = String.Format("Ella prende atto che l’Azienda non è tenuta a riconoscere e non " +
                //        "riconoscerà qualsivoglia modifica del presente contratto che non sia stata formalizzata " +
                //        "per iscritto e sottoscritta da un procuratore dell’Azienda medesima munito dei " +
                //        "relativi poteri. ");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par22, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par23 = String.Format("Con la sottoscrizione del presente contratto, Lei si impegna a " +
                //        "compilare e restituire completi in tutte le loro parti alle competenti strutture di" +
                //        " gestione del Personale i formulari di  dichiarazione che Le saranno sottoposti al fine " +
                //        "di acquisire le informazioni  essenziali per la corretta gestione del Suo rapporto di " +
                //        "lavoro, quali, a titolo meramente esemplificativo e non esaustivo, la dichiarazione " +
                //        "sulla spettanza delle detrazioni d'imposta, la dichiarazione circa l'anzianità " +
                //        "assicurativa e contributiva posseduta, la modulistica ai fini della scelta circa la " +
                //        "destinazione del TFR (ove necessaria).");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par23, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par24 = String.Format("Nel restituire copia della presente da Lei datata e sottoscritta " +
                //        "in segno di integrale accettazione di quanto ivi contenuto, La invitiamo, inoltre, " +
                //        "a consegnare i seguenti documenti:");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par24, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    tableDetail = new PdfPTable(2);
                //    tableDetail.DefaultCell.BorderWidth = 0;
                //    tableDetail.TotalWidth = 550;
                //    tableDetail.LockedWidth = true;
                //    tableDetailWidth = new int[] { 50, 500 };
                //    tableDetail.SetWidths(tableDetailWidth);

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("eventuale libretto di iscrizione alla Gestione ex " +
                //        "ENPALS dell’INPS ove da Lei posseduto in relazione a precedenti rapporti " +
                //        "di lavoro nello spettacolo;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("certificati di nascita, cittadinanza, residenza e stato " +
                //        "di famiglia;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("titoli di studio;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("copia documento di identità;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("codice fiscale;", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" • ", 0, 1, 2, myFontCorpo, 0));
                //    cellPhrase = new Phrase();
                //    cellPhrase.Add(new Chunk("gli altri documenti eventualmente necessari per perfezionare " +
                //        "l'assunzione di cui sopra, ivi inclusi, se del caso, quelli utili al godimento " +
                //        "delle agevolazioni normative e/o contributive di legge.", myFontCorpo));
                //    p = new Paragraph(cellPhrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(p, 0, 1, 0, 0));
                //    tableDetail.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 0, myFontInterlinea, 0));

                //    document.Add(tableDetail);
                //    tableDetail.FlushContent();
                //    tableDetail.DeleteBodyRows();

                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    string par25 = String.Format("Cordiali Saluti");
                //    phrase = new Phrase();
                //    phrase.Add(new Chunk(par25, myFontCorpo));
                //    p = new Paragraph(phrase);
                //    ((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    document.Add(p);

                //    //phrase = new Phrase();
                //    //phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    //p = new Paragraph(phrase);
                //    //((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    //document.Add(p);

                //    //phrase = new Phrase();
                //    //phrase.Add(new Chunk(SPAZIO, myFontCorpo));
                //    //p = new Paragraph(phrase);
                //    //((Paragraph)p).Alignment = Element.ALIGN_JUSTIFIED;
                //    //document.Add(p);                    

                //    //PdfPTable tableFirma = new PdfPTable(4);
                //    //tableFirma.DefaultCell.BorderWidth = 0;
                //    //tableFirma.TotalWidth = 550;
                //    //tableFirma.LockedWidth = true;
                //    //var tableFirmaWidth = new int[] { 137, 137, 137, 137 };
                //    //tableFirma.SetWidths(tableFirmaWidth);
                //    //tableFirma.AddCell(WriteCellsClass.WriteCell(" ", 0, 2, 1, myFontCorpo, 0));
                //    //tableFirma.AddCell(WriteCellsClass.WriteCell("FIRMA", 0, 2, 1, myFontCorpo, 0));

                //    //document.Add(tableFirma);
                //    //tableFirma.FlushContent();
                //    //tableFirma.DeleteBodyRows();

                //    document.Close();
                //    writer.Close();
                //    bytes = ms.ToArray();
                //    return bytes;
                //}
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region PUBLIC RECUPERO INFORMAZIONI

        public static string GetRuoloByCode(string code)
        {
            string result = String.Empty;

            if (!String.IsNullOrEmpty(code))
            {
                code = code.Trim().ToUpper();
                using (IncentiviEntities czndb = new IncentiviEntities())
                {
                    var item = czndb.RUOLO.Where(w => w.COD_RUOLO == code && w.DTA_FINE >= DateTime.Today).FirstOrDefault();

                    if (item != null)
                    {
                        result = item.DES_RUOLO;
                        result = result.Replace(code + " - ", "");
                        result = result.Trim();
                        result = CommonHelper.ToTitleCase(result);
                    }
                }
            }

            return result;
        }

        public static string GetCategoriaByCode(string code)
        {
            string result = String.Empty;

            if (!String.IsNullOrEmpty(code))
            {
                code = code.Trim().ToUpper();
                using (IncentiviEntities czndb = new IncentiviEntities())
                {
                    var item = czndb.QUALIFICA.Where(w => w.COD_QUALIFICA == code).FirstOrDefault();

                    if (item != null)
                    {
                        result = item.DES_QUALIFICA;
                        result = result.Replace(code + " - ", "");
                        result = result.Trim();
                        result = CommonHelper.ToTitleCase(result);
                    }
                }
            }

            return result;
        }

        public static string GetRuoloByCategoria(string code)
        {
            string result = String.Empty;

            if (!String.IsNullOrEmpty(code))
            {
                code = code.Trim().ToUpper();
                using (IncentiviEntities czndb = new IncentiviEntities())
                {
                    var item = czndb.QUALIFICA.Where(w => w.COD_QUALIFICA == code).FirstOrDefault();

                    if (item != null)
                    {
                        string codQual = item.COD_QUALSTD;
                        var obj = czndb.TB_QUALSTD.Where(w => w.COD_QUALSTD == codQual).FirstOrDefault();

                        if (obj != null)
                        {
                            result = obj.DES_QUALSTD;
                            result = result.Trim();
                            result = CommonHelper.ToTitleCase(result);
                        }
                    }
                }
            }

            return result;
        }

        public static string GetSedeByCode(string code)
        {
            string result = String.Empty;

            if (!String.IsNullOrEmpty(code))
            {
                code = code.Trim().ToUpper();
                using (IncentiviEntities czndb = new IncentiviEntities())
                {
                    var item = czndb.SEDE.Where(x => x.DTA_INIZIO <= DateTime.Today &&
                                                x.DTA_FINE >= DateTime.Today &&
                                                x.COD_SEDE == code).FirstOrDefault();

                    if (item != null)
                    {
                        result = item.DES_SEDE;
                        result = result.Replace(code + " - ", "");
                        result = result.Trim();
                        result = CommonHelper.ToTitleCase(result);
                    }
                }
            }

            return result;
        }

        public static string GetDirezioneByCode(string code)
        {
            string result = String.Empty;

            if (!String.IsNullOrEmpty(code))
            {
                code = code.Trim().ToUpper();
                using (IncentiviEntities czndb = new IncentiviEntities())
                {
                    var item = czndb.XR_TB_SERVIZIO.Where(x => x.COD_SERVIZIO == code).FirstOrDefault();

                    if (item != null)
                    {
                        result = item.DES_SERVIZIO;
                        result = result.Replace(code + " - ", "");
                        result = result.Trim();
                        result = CommonHelper.ToTitleCase(result);
                    }
                }
            }

            return result;
        }
        public static string GetStipendio(string categoria, string tipoMinimo)
        {
            string result = String.Empty;

            if (!String.IsNullOrWhiteSpace(categoria) && !string.IsNullOrWhiteSpace(tipoMinimo))
            {
                categoria = categoria.Trim().ToUpper();
                tipoMinimo = tipoMinimo.Trim().ToUpper();
                using (IncentiviEntities czndb = new IncentiviEntities())
                {
                    var item = czndb.XR_TB_MINIMI.Where(x => x.CATEGORIA.ToUpper() == categoria && x.TIPOMINIMO.ToUpper() == tipoMinimo).FirstOrDefault();

                    if (item != null)
                    {
                        result = item.MINIMO.ToString();
                    }
                }
            }

            return result;
        }

        #endregion

    }
}