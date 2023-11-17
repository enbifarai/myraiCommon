using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using iTextSharp.text;
using iTextSharp.text.pdf;
using myRaiCommonManager;
using myRaiCommonModel;
using myRaiData;
using myRaiHelper;

namespace myRai.Controllers
{
    public class StampaFeriePermessiController
    {
        public byte[] StampaPdf(List<CalendarioDay> giorniCal, List<TipoPermessoFerieUsato> tipiGiornata, int anno, string logoRai)
        {
            byte[] bytes = null;
            int currentY = 480;
            const int lStartX = 25;
            const int fontSize = 10;
            int border = 0;
            int textAlign = 0;

            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED);
            Font myFont = new Font(bf, fontSize, Font.NORMAL);
            Font myFontBold = new Font(bf, fontSize, Font.BOLD);

            if (FeriePermessiControllerScope.Instance.giorniCalendario == null)
                throw new Exception("Si è verificato un errore durante la creazione del pdf.\nDati calendario non trovati.");

            string titolo = "Ferie Permessi per l'anno " + anno.ToString();

            byte[] png;

            using (digiGappEntities db = new digiGappEntities())
            {
                var imagePath = db.MyRai_Incentivi_Template.FirstOrDefault(x => x.Sede == "LOGO");
                png = imagePath.Template;
            }

            using (MemoryStream ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4.Rotate(), 10, 10, 10, 10);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                writer.PageEvent = new CalITextEvents(giorniCal, anno, png);
                document.Open();

                PdfContentByte cb = writer.DirectContent;
                currentY = ((CalITextEvents)writer.PageEvent).currentY;
                currentY = writeIntestazione(cb, document, anno, logoRai, currentY);
                currentY = writeSpaceRow(document, cb, currentY);
                currentY = buildCalendar(giorniCal, anno, document, cb, currentY);
                currentY -= 10;
                currentY = buildLegenda(tipiGiornata, document, cb, currentY);
                document.Close();
                writer.Close();
                bytes = ms.ToArray();
            }
            return bytes;
        }


        #region sezioni
        private int writeIntestazione(PdfContentByte pcb, Document document, int anno, string img, int currentY)
        {
            const int fontSize = 10;
            const int lStartX = 25;

            int border = 0; // none
            int textAlign = 0; // left

            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED);
            Font myFont = new Font(bf, fontSize, Font.NORMAL);
            Font myFontBold = new Font(bf, fontSize, Font.BOLD);

            //PdfPTable tableCestini = new PdfPTable(3);
            PdfPTable tab = new PdfPTable(2);

            #region impostazioni tabella
            tab.DefaultCell.BorderWidth = 1;
            tab.TotalWidth = document.PageSize.Width - 50;
            tab.LockedWidth = true;
            int[] widths = new int[] { 300, 200 };
            tab.SetWidths(widths);
            #endregion

            tab.AddCell(writeCell("Ferie e permessi anno " + anno.ToString(), border, 1, 1, myFontBold));

            //tableCestini.AddCell(this.WriteCell("Destinatari pasti", border, 3, 1, myFontBold));
            //tableCestini.AddCell(this.WriteCell(" ", border, 3, 1, myFont));

            //tableCestini.AddCell(this.WriteCell("Destinatario ", border, 1, textAlign, myFontBold));
            //tableCestini.AddCell(this.WriteCell("Tipologia ", border, 1, textAlign, myFontBold));
            //tableCestini.AddCell(this.WriteCell("Codice ", border, 1, textAlign, myFontBold));
            //tableCestini.AddCell(this.WriteCell(" ", border, 3, 1, myFont));
            //tableCestini.WriteSelectedRows(0, (tableCestini.Rows.Count + 1), lStartX, currentY, cb);
            tab.WriteSelectedRows(0, (tab.Rows.Count + 1), lStartX, currentY, pcb);

            currentY = currentY - (int)tab.CalculateHeights();
            tab.FlushContent();
            return currentY;
        }
        private int buildCalendar(List<CalendarioDay> giorni, int anno, Document doc, PdfContentByte cb, int currentY)
        {
            int res = 0;
            const int fontSize = 10;
            const int fontSizeSmall = 10;
            const int lStartX = 25;
            int border = 1;
            int textAlign = 0;
            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font fontNormale = new Font(bf, fontSize, Font.NORMAL);
            Font fontBold = new Font(bf, fontSize, Font.BOLD);
            Font fontTab = new Font(bf, fontSizeSmall, Font.NORMAL);

            Font fontTabHeader = new Font(bf, fontSizeSmall, Font.NORMAL);

            PdfPTable tb = new PdfPTable(32);
            tb.DefaultCell.BorderWidth = 1;
            tb.TotalWidth = doc.PageSize.Width - 50;
            tb.LockedWidth = true;
            // questo set verrà riportata per ogni riga?
            int[] widths = new int[] { 100, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, };
            tb.SetWidths(widths);

            // riga vuota
            #region prima riga tabellone

            tb.AddCell(writeCellMonth(" ", border, 1, textAlign, fontTab));
            for (int i = 1; i < 32; i++)
            {
                //tb.AddCell(writeCell(i.ToString(), border, 1, 1, fontTabBold));
                tb.AddCell(writeCellTabHeader(i.ToString(), 1, 1, fontTabHeader, false, cb));
            }
            //tb.WriteSelectedRows(0, (tb.Rows.Count + 1), lStartX, currentY, cb);

            res = currentY - (int)tb.CalculateHeights();

            #endregion
            CalendarioDay currDayMonth;
            string contenutoCella = " ";
            bool festivo = false;

            // cb.Ellipse(x - rx, y + ry, x + rx, y - ry);

            DateTime dtt = new DateTime(2018, 5, 10);
            #region griglia principale
            for (int m = 1; m < 13; m++)
            {
                // scorro i mesi
                tb.AddCell(writeCellMonth(" " + CommonHelper.TraduciMeseDaNumLett(m.ToString("00")), border, 1, textAlign, fontTab));
                for (int g = 1; g < 32; g++)
                {
                    // scorro i giorni
                    currDayMonth = giorni.Find(f => f.giorno.Day == g && f.giorno.Month == m);

                    if (currDayMonth == null)
                    {
                        contenutoCella = " ";
                        festivo = true;
                    }
                    else
                    {
                        if (currDayMonth.giorno.CompareTo(dtt) == 0)
                            dtt = DateTime.Now;
                        festivo = currDayMonth.giorno.DayOfWeek == DayOfWeek.Saturday;
                        festivo = !festivo ? currDayMonth.giorno.DayOfWeek == DayOfWeek.Sunday : festivo;
                        festivo = !festivo ? (currDayMonth.tipoGiornata == "A" || currDayMonth.tipoGiornata == "B") : festivo;

                        if (currDayMonth.Frazione == "")
                            contenutoCella = " ";
                        else
                            contenutoCella = currDayMonth.tipoFeriePermesso;


                    }

                    if (currDayMonth != null)
                        tb.AddCell(writeCellTab(contenutoCella, 1, 1, fontTab, festivo, cb, currDayMonth.Frazione));
                    else
                        tb.AddCell(writeCellTab("", 1, 1, fontTab, festivo, cb, true));

                }


            }
            tb.WriteSelectedRows(0, tb.Rows.Count + 1, lStartX, currentY, cb);
            res = currentY - (int)tb.CalculateHeights();
            #endregion
            return res;
        }
        private int buildLegenda(List<TipoPermessoFerieUsato> tipiGiornata, Document doc, PdfContentByte cb, int currentY)
        {
            int res = 0;
            const int fontSize = 10;
            System.Drawing.Color colorPie = System.Drawing.Color.White;
            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            Font fontNormale = new Font(bf, fontSize, Font.NORMAL);
            Font fontBold = new Font(bf, fontSize, Font.BOLD);


            //const int lStartX = 650;
            const int lStartX = 25;
            int border = 1;
            int textAlign = 0;
            PdfPTable tb = new PdfPTable(7);
            tb.DefaultCell.BorderWidth = 1;
            //tb.TotalWidth = 160;
            tb.TotalWidth = 420;
            tb.LockedWidth = true;
            bool alternato = false;
            int[] widths = new int[] {
                30,
                140,
                60,
                50,
                50,
                60,
                50 };
            tb.SetWidths(widths);

            // testata fissa
            tb.AddCell(writeCellLegenda(cb, "", fontNormale, Rectangle.LEFT_BORDER | Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER, alternato));
            tb.AddCell(writeCellLegenda(cb, "", fontNormale, Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER, alternato));
            tb.AddCell(writeCellLegenda(cb, "Anno Prec.", fontNormale, Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.LEFT_BORDER|Rectangle.RIGHT_BORDER, alternato));
            tb.AddCell(writeCellLegenda(cb, "Spettanti", fontNormale, Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER| Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER, alternato));
            tb.AddCell(writeCellLegenda(cb, "Fruite", fontNormale, Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER, alternato));
            tb.AddCell(writeCellLegenda(cb, "Pianificate", fontNormale, Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER, alternato));
            tb.AddCell(writeCellLegenda(cb, "Residue", fontNormale, Rectangle.TOP_BORDER | Rectangle.RIGHT_BORDER | Rectangle.BOTTOM_BORDER | Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER, alternato));



            // righe
            for (int i = 0; i < tipiGiornata.Count; i++)
            {


                alternato = (i % 2) != 0;

                if (i == 0)
                {
                    tb.AddCell(writePieCell(cb, tipiGiornata[i].sigla, Rectangle.LEFT_BORDER, alternato));
                    tb.AddCell(writeCellLegenda(cb, tipiGiornata[i].tipoDesc + " (" + tipiGiornata[i].sigla.ToUpper().Trim() + ")", fontNormale, Rectangle.NO_BORDER, alternato));
                    tb.AddCell(writeCellLegenda(cb, tipiGiornata[i].resoconto.AnnoPrec.ToString(), fontNormale, Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER, alternato));
                    tb.AddCell(writeCellLegenda(cb, tipiGiornata[i].resoconto.Spettanti.ToString(), fontNormale, Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER, alternato));
                    tb.AddCell(writeCellLegenda(cb, tipiGiornata[i].resoconto.Usufruite.ToString(), fontNormale, Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER, alternato));
                    tb.AddCell(writeCellLegenda(cb, tipiGiornata[i].resoconto.Pianificate.ToString(), fontNormale, Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER, alternato));
                    tb.AddCell(writeCellLegenda(cb, tipiGiornata[i].resoconto.Residue.ToString(), fontNormale, Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER, alternato));
                }
                else if (i == tipiGiornata.Count - 1)
                {
                    tb.AddCell(writePieCell(cb, tipiGiornata[i].sigla, Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER, alternato));
                    tb.AddCell(writeCellLegenda(cb, tipiGiornata[i].tipoDesc + " (" + tipiGiornata[i].sigla.ToUpper().Trim() + ")", fontNormale, Rectangle.BOTTOM_BORDER, alternato));
                    tb.AddCell(writeCellLegenda(cb, tipiGiornata[i].resoconto.AnnoPrec.ToString(), fontNormale, Rectangle.BOTTOM_BORDER | Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER, alternato));
                    tb.AddCell(writeCellLegenda(cb, tipiGiornata[i].resoconto.Spettanti.ToString(), fontNormale, Rectangle.BOTTOM_BORDER | Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER, alternato));
                    tb.AddCell(writeCellLegenda(cb, tipiGiornata[i].resoconto.Usufruite.ToString(), fontNormale, Rectangle.BOTTOM_BORDER | Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER, alternato));
                    tb.AddCell(writeCellLegenda(cb, tipiGiornata[i].resoconto.Pianificate.ToString(), fontNormale, Rectangle.BOTTOM_BORDER | Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER, alternato));
                    tb.AddCell(writeCellLegenda(cb, tipiGiornata[i].resoconto.Residue.ToString(), fontNormale, Rectangle.BOTTOM_BORDER | Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER, alternato));
                }
                else
                {
                    tb.AddCell(writePieCell(cb, tipiGiornata[i].sigla, Rectangle.LEFT_BORDER, alternato));
                    tb.AddCell(writeCellLegenda(cb, tipiGiornata[i].tipoDesc + " (" + tipiGiornata[i].sigla.ToUpper().Trim() + ")", fontNormale, Rectangle.NO_BORDER, alternato));
                    tb.AddCell(writeCellLegenda(cb, tipiGiornata[i].resoconto.AnnoPrec.ToString(), fontNormale, Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER, alternato));
                    tb.AddCell(writeCellLegenda(cb, tipiGiornata[i].resoconto.Spettanti.ToString(), fontNormale, Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER, alternato));
                    tb.AddCell(writeCellLegenda(cb, tipiGiornata[i].resoconto.Usufruite.ToString(), fontNormale, Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER, alternato));
                    tb.AddCell(writeCellLegenda(cb, tipiGiornata[i].resoconto.Pianificate.ToString(), fontNormale, Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER, alternato));
                    tb.AddCell(writeCellLegenda(cb, tipiGiornata[i].resoconto.Residue.ToString(), fontNormale, Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER, alternato));
                }
            }

            //for (int i = 0; i < tipiGiornata.Count; i++) {
            //    if (i == 0) {
            //        tb.AddCell(writePieCell(cb, tipiGiornata[i].sigla, Rectangle.LEFT_BORDER | Rectangle.TOP_BORDER));
            //        tb.AddCell(writeCellLegenda(cb, tipiGiornata[i].tipoDesc, fontNormale, Rectangle.TOP_BORDER | Rectangle.RIGHT_BORDER));
            //    }
            //    else if (i == tipiGiornata.Count-1)
            //    {
            //        tb.AddCell(writePieCell(cb, tipiGiornata[i].sigla, Rectangle.LEFT_BORDER | Rectangle.BOTTOM_BORDER));
            //        tb.AddCell(writeCellLegenda(cb, tipiGiornata[i].tipoDesc, fontNormale, Rectangle.RIGHT_BORDER | Rectangle.BOTTOM_BORDER));

            //    }
            //    else {
            //        tb.AddCell(writePieCell(cb, tipiGiornata[i].sigla, Rectangle.LEFT_BORDER));
            //        tb.AddCell(writeCellLegenda(cb, tipiGiornata[i].tipoDesc, fontNormale, Rectangle.RIGHT_BORDER));    
            //    }


            //}

            // ferie
            //tb.AddCell(writePieCell(cb, "fe",Rectangle.LEFT_BORDER | Rectangle.TOP_BORDER));
            //tb.AddCell(writeCellLegenda(cb, "Ferie", fontNormale,Rectangle.TOP_BORDER | Rectangle.RIGHT_BORDER));
            //// Extra Festività
            //tb.AddCell(writePieCell(cb, "pf",Rectangle.LEFT_BORDER));
            //tb.AddCell(writeCellLegenda(cb, "Extra Festività", fontNormale,Rectangle.RIGHT_BORDER));
            //// Permesso
            //tb.AddCell(writePieCell(cb, "pr", Rectangle.LEFT_BORDER));
            //tb.AddCell(writeCellLegenda(cb, "Permesso", fontNormale, Rectangle.RIGHT_BORDER));
            //// Permesso Giornalisti
            //tb.AddCell(writePieCell(cb, "pg", Rectangle.LEFT_BORDER));
            //tb.AddCell(writeCellLegenda(cb, "Permesso Giornalisti", fontNormale, Rectangle.RIGHT_BORDER));
            //// Mancati Non Lavorati
            //tb.AddCell(writePieCell(cb, "mn", Rectangle.LEFT_BORDER));
            //tb.AddCell(writeCellLegenda(cb, "Mancati Non Lavorati", fontNormale, Rectangle.RIGHT_BORDER));
            //// Mancati Riposi
            //tb.AddCell(writePieCell(cb, "mr", Rectangle.LEFT_BORDER));
            //tb.AddCell(writeCellLegenda(cb, "Mancati Riposi", fontNormale, Rectangle.RIGHT_BORDER));
            ////Mancati Festivi


            tb.WriteSelectedRows(0, tb.Rows.Count + 1, lStartX, currentY, cb);
            res = currentY - (int)tb.CalculateHeights();
            return res;
        }
        #endregion
        #region singoli elementi
        private PdfPCell writeCell(string text, int border, int colspan, int textAlign, Font f)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, f));
            cell.Border = border;
            cell.HorizontalAlignment = textAlign; //0=Left, 1=Centre, 2=Right
            cell.Colspan = colspan;

            return cell;
        }
        private PdfPCell writeCellMonth(string text, int border, int colspan, int textAlign, Font f)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, f));

            cell.BorderColor = new BaseColor(128, 128, 128);
            cell.Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER | Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER;
            cell.HorizontalAlignment = textAlign; //0=Left, 1=Centre, 2=Right
            cell.Colspan = colspan;
            cell.FixedHeight = 20;
            return cell;
        }
        private PdfPCell writeCellTab(string text, int border, int textAlign, Font f, bool festivo, PdfContentByte cb, string frazione)
        {
            PdfPCell cell;
            string cFestivo = "#f0f0f0";
            string cNormale = "#ffffff";
            if (text != " " && !text.ToUpper().StartsWith("M"))
            {
                //pie-fe 48bfff
                //pie-pf 1d92f5
                //pie-pr 3431a4
                //pie-pg d26a5c
                //pie-mn EAA921
                //pie-mr 1d92f5
                //pie-mf f88201
                System.Drawing.Color colorePie = System.Drawing.ColorTranslator.FromHtml("#1d92f5");
                colorePie = getColorPie(text.ToLower().Trim());

                System.Drawing.Color bkCell = System.Drawing.ColorTranslator.FromHtml(cNormale);

                if (festivo)
                    bkCell = System.Drawing.ColorTranslator.FromHtml(cFestivo);

                iTextSharp.text.Image img = drawCircle(cb, colorePie, frazione, bkCell);


                cell = new PdfPCell(img);

            }
            else
            {
                if (text.ToUpper().StartsWith("M")) text = "";
                cell = new PdfPCell(new Phrase(text, f));
            }

            cell.BorderColor = new BaseColor(128, 128, 128);
            cell.Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER | Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER;
            cell.HorizontalAlignment = textAlign; //0=Left, 1=Centre, 2=Right

            BaseColor c = new BaseColor(System.Drawing.ColorTranslator.FromHtml(cNormale));
            if (festivo)
                c = new BaseColor(System.Drawing.ColorTranslator.FromHtml(cFestivo));

            cell.BackgroundColor = c;

            cell.FixedHeight = 20;

            return cell;
        }
        private PdfPCell writeCellTab(string text, int border, int textAlign, Font f, bool festivo, PdfContentByte cb, bool notExist = false)
        {
            PdfPCell cell;

            cell = new PdfPCell(new Phrase(text, f));

            cell.BorderColor = new BaseColor(128, 128, 128);
            cell.Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER | Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER;
            cell.HorizontalAlignment = textAlign; //0=Left, 1=Centre, 2=Right

            if (notExist)
                cell.BackgroundColor = new BaseColor(179, 179, 179);
            else if (festivo)
                cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml("#f0f0f0"));

            cell.FixedHeight = 20;

            return cell;
        }
        private PdfPCell writeCellTabHeader(string text, int border, int textAlign, Font f, bool festivo, PdfContentByte cb)
        {
            PdfPCell cell;
            cell = new PdfPCell(new Phrase(text, f));

            cell.BorderColor = new BaseColor(128, 128, 128);
            cell.Border = Rectangle.LEFT_BORDER | Rectangle.RIGHT_BORDER | Rectangle.TOP_BORDER | Rectangle.BOTTOM_BORDER;
            cell.HorizontalAlignment = textAlign; //0=Left, 1=Centre, 2=Right

            if (festivo)
                cell.BackgroundColor = BaseColor.YELLOW;

            cell.FixedHeight = 20;

            return cell;
        }
        public static iTextSharp.text.Image drawCircle(PdfContentByte contentByte, System.Drawing.Color color, string frazione, System.Drawing.Color backColor)
        {
            //contentByte.SaveState();
            var template = contentByte.CreateTemplate(260, 260);

            template.MoveTo(38.33889376f, 67.35513328f);

            int arcPosStartY = 60;
            int arcPosStartX = 60;
            int arcPosEndX = 200;
            int arcPosEndY = 200;
            int recPosStartX = 60;
            int recPosStartY = 60;
            int recW = 150;
            int recH = 75;
            if (frazione.StartsWith("I"))
            {
                template.Arc(arcPosStartX, arcPosStartY, arcPosEndX, arcPosEndY, 0, 360);
                template.SetRGBColorFill(color.R, color.G, color.B);
                template.Fill();
            }
            if (frazione.StartsWith("M"))
            {
                template.Arc(arcPosStartX, arcPosStartY, arcPosEndX, arcPosEndY, 270, 180);
                template.SetRGBColorFill(color.R, color.G, color.B);
                template.Fill();

                template.Arc(arcPosStartX, arcPosStartY, arcPosEndX, arcPosEndY, 90, 180);
                template.SetRGBColorFill(System.Drawing.Color.White.R, System.Drawing.Color.White.G, System.Drawing.Color.White.B);
                template.Fill();

                template.Arc(arcPosStartX, arcPosStartY, arcPosEndX, arcPosEndY, 90, 360);
                template.SetRGBColorStroke(color.R, color.G, color.B);
                template.Stroke();
            }
            if (frazione.StartsWith("U"))
            {
                template.Arc(arcPosStartX, arcPosStartY, arcPosEndX, arcPosEndY, 270, 180);
                template.SetRGBColorFill(color.R, color.G, color.B);
                template.Fill();


                template.Arc(arcPosStartX, arcPosStartY, arcPosEndX, arcPosEndY, 90, 180);
                template.SetRGBColorFill(System.Drawing.Color.White.R, System.Drawing.Color.White.G, System.Drawing.Color.White.B);
                template.Fill();

                template.Arc(arcPosStartX, arcPosStartY, arcPosEndX, arcPosEndY, 180, 180);
                template.SetRGBColorFill(System.Drawing.Color.White.R, System.Drawing.Color.White.G, System.Drawing.Color.White.B);
                template.Fill();

                template.Arc(arcPosStartX, arcPosStartY, arcPosEndX, arcPosEndY, 0, 360);
                template.SetRGBColorStroke(color.R, color.G, color.B);
                template.Stroke();
                //template.Rectangle(recPosStartX, recPosStartY, recW, recH);

                //template.SetRGBColorFill(backColor.R, backColor.G, backColor.B);
                //template.Fill();
            }


            var img = iTextSharp.text.Image.GetInstance(template);

            return img;
        }
        private int writeSpaceRow(Document doc, PdfContentByte cb, int currentY)
        {
            int res = 0;
            const int lStartX = 25;
            PdfPTable tb = new PdfPTable(1);
            tb.DefaultCell.BorderWidth = 0;
            tb.TotalWidth = doc.PageSize.Width;
            tb.LockedWidth = true;
            int[] w = new int[] { 500 };
            tb.SetWidths(w);
            tb.AddCell(" ");
            tb.WriteSelectedRows(0, (tb.Rows.Count + 1), lStartX, currentY, cb);
            res = currentY - (int)tb.CalculateHeights();
            tb.FlushContent();
            return res;
        }
        private PdfPCell writePieCell(PdfContentByte cb, string tipoFeriePermesso, int bordo, bool alternato)
        {
            PdfPCell cell;

            System.Drawing.Color color = System.Drawing.Color.White;
            color = getColorPie(tipoFeriePermesso);
            iTextSharp.text.Image img = drawCircle(cb, color, "I", System.Drawing.Color.White);
            cell = new PdfPCell(img);
            cell.BorderColor = new BaseColor(128, 128, 128);
            cell.Border = bordo;
            cell.HorizontalAlignment = 1;
            string backC = "#ffffff";

            if (alternato)
                backC = "#f0f0f0";

            cell.BackgroundColor = new BaseColor(System.Drawing.ColorTranslator.FromHtml(backC));

            cell.FixedHeight = 15;
            return cell;
        }
        private PdfPCell writeCellLegenda(PdfContentByte cb, string testo, Font f, int bordo, bool alternato)
        {
            PdfPCell cell = new PdfPCell(new Phrase(" "+testo, f));
            cell.BorderColor = new BaseColor(128, 128, 128);
            cell.Border = bordo;
            cell.HorizontalAlignment = 0;
            cell.FixedHeight = 15;
            System.Drawing.Color sfondo = System.Drawing.ColorTranslator.FromHtml("#ffffff");


            if (alternato)
                sfondo = System.Drawing.ColorTranslator.FromHtml("#f0f0f0");

            cell.BackgroundColor = new BaseColor(sfondo);

            return cell;
        }
        #endregion
        private System.Drawing.Color getColorPie(string siglaTipoPermesso)
        {
            System.Drawing.Color color = System.Drawing.Color.White;
            switch (siglaTipoPermesso.Trim().ToLower())
            {
                case "fe":
                    color = System.Drawing.ColorTranslator.FromHtml("#48bfff");
                    break;
                case "pf":
                    color = System.Drawing.ColorTranslator.FromHtml("#1d92f5");
                    break;
                case "pr":
                    color = System.Drawing.ColorTranslator.FromHtml("#3431a4");
                    break;
                case "pg":
                    color = System.Drawing.ColorTranslator.FromHtml("#d26a5c");
                    break;
                case "mn":
                    color = System.Drawing.ColorTranslator.FromHtml("#00aa00");
                    break;
                case "rn":
                    color = System.Drawing.ColorTranslator.FromHtml("#00aa00");
                    break;
                case "mr":
                    color = System.Drawing.ColorTranslator.FromHtml("#EAA921");
                    break;
                case "rr":
                    color = System.Drawing.ColorTranslator.FromHtml("#EAA921");
                    break;
                case "mf":
                    color = System.Drawing.ColorTranslator.FromHtml("#f88201");
                    break;
                case "rf":
                    color = System.Drawing.ColorTranslator.FromHtml("#f88201");
                    break;
            }

            return color;
        }
        private string getDefaultColorPie()
        {
            string t = CommonHelper.GetTema( CommonHelper.GetCurrentUserMatricola());
            string res = "#1d92f5";
            switch (t.ToLower().Trim())
            {
                case "blue":
                    res = "#1d92f5";
                    break;
                case "orange":
                    res = "#e85700";
                    break;
                case "green":
                    res = "#16a77f";
                    break;
                case "violet":
                    res = "#ce6f98";
                    break;
                default:
                    res = "#1d92f5";
                    break;
            }
            return res;
        }
    }

    public class CalITextEvents : PdfPageEventHelper
    {

        #region constructors
        public CalITextEvents(List<CalendarioDay> giorni, int anno, byte[] logo)//, string logo)
        {
            _days = new List<CalendarioDay>();
            _days = giorni;
            _anno = anno;
            _imgPath = logo;
        }
        #endregion
        PdfContentByte cb;
        PdfTemplate headerTemplate, footerTemplate;
        DateTime dataStampa = DateTime.Now;
        #region campi
        List<CalendarioDay> _days;
        int _anno;
        //string _imgPath;
        byte[] _imgPath;
        private string _header;
        int _currentY = 480;
        #endregion
        #region proprietà
        public string header
        {
            get
            {
                return _header;
            }
            set
            {
                _header = value;
            }
        }
        public int currentY
        {
            get
            {
                return _currentY;
            }
            set
            {
                _currentY = value;
            }
        }

        const int LSTARTX = 25;
        const int FONTSIZE = 10;
        BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED);
        #endregion
        #region handlers
        public override void OnStartPage(PdfWriter writer, Document document)
        {
            // Creazione del PdfContentByte per il posizionamento esatto degli elementi sul documento
            PdfContentByte cb = writer.DirectContent;

            footerTemplate = cb.CreateTemplate(PageSize.A4.Rotate().Width - 50, 50);
            int intestazioneHeight = this.WriteIntestazione(cb, document);

            headerTemplate = cb.CreateTemplate(PageSize.A4.Rotate().Width - 50, intestazioneHeight);
            cb.AddTemplate(headerTemplate, document.LeftMargin, document.PageSize.GetTop(document.TopMargin));
        }
        public override void OnOpenDocument(PdfWriter writer, Document document)
        {
            try
            {
                // Aggiunta metadata al documento
                document.AddAuthor("digiGapp");

                document.AddCreator("digiGapp con l'ausilio di iTextSharp");

                document.AddKeywords("PDF Calendario annuale ");

                document.AddSubject("Riepilogo ferie permessi");

                document.AddTitle(String.Format("Calendario annuale"));
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public override void OnEndPage(PdfWriter writer, Document document)
        {
            base.OnEndPage(writer, document);

            int pageN = writer.PageNumber;
            //String text = "Pagina " + pageN.ToString() + " di ";
            string text = "Situazione al ";

            float len = bf.GetWidthPoint(text, FONTSIZE);

            iTextSharp.text.Rectangle pageSize = document.PageSize;

            cb = writer.DirectContent;

            cb.SetRGBColorFill(100, 100, 100);

            cb.BeginText();
            cb.SetFontAndSize(bf, FONTSIZE);
            cb.SetTextMatrix(document.LeftMargin+15, document.PageSize.GetBottom(document.BottomMargin)+15);
            cb.ShowText(text);
            cb.EndText();

            text = CommonHelper.GetNominativoPerMatricola( CommonHelper.GetCurrentUserMatricola( ) ).TitleCase( );
            float len2 = bf.GetWidthPoint(text, FONTSIZE);
            cb.BeginText();
            cb.SetFontAndSize(bf, FONTSIZE);
            cb.SetTextMatrix(document.GetRight(document.RightMargin) - len2, document.PageSize.GetBottom(document.BottomMargin) + 10);
            cb.ShowText(text);
            cb.EndText();

            footerTemplate = cb.CreateTemplate(200, 50);
            cb.AddTemplate(footerTemplate, document.LeftMargin+len+15, document.PageSize.GetBottom(document.BottomMargin)+15);
        }
        public override void OnCloseDocument(PdfWriter writer, Document document)
        {
            base.OnCloseDocument(writer, document);

            footerTemplate.BeginText();
            footerTemplate.SetFontAndSize(bf, FONTSIZE);
            footerTemplate.SetTextMatrix(0, 0);
            footerTemplate.ShowText(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
            footerTemplate.EndText();

            //string text = CommonHelper.GetNominativoPerMatricola(CommonHelper.GetCurrentUserMatricola());

            //float len = bf.GetWidthPoint(text, FONTSIZE);

            //iTextSharp.text.Rectangle pageSize = document.PageSize;

            //cb = writer.DirectContent;

            //cb.SetRGBColorFill(100, 100, 100);

            
        }
        #endregion
        #region private methods
        private PdfPCell writeCell(string text, int border, int colspan, int textAlign, Font f)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text, f));
            cell.Border = border;
            cell.HorizontalAlignment = textAlign; //0=Left, 1=Centre, 2=Right
            cell.Colspan = colspan;
            return cell;
        }
        private int WriteIntestazione(PdfContentByte cb, Document document)
        {

            const int lStartX = 25;
            const int fontSize = 10;

            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.NOT_EMBEDDED);
            Font myFont = new Font(bf, fontSize, Font.NORMAL);
            Font myFontBold = new Font(bf, fontSize, Font.BOLD);

            Font titleFont = new Font(bf, 14, Font.BOLD);

            int border = 0; // none
            int textAlign = 0; // left

            // disegno del logo
            iTextSharp.text.Image png = iTextSharp.text.Image.GetInstance(_imgPath);
            png.ScaleAbsolute(45, 45);
            png.SetAbsolutePosition(25, (PageSize.A4.Rotate().Height - 25 - 45));
            cb.AddImage(png);

            PdfPTable table = new PdfPTable(1);
            table.DefaultCell.Border = Rectangle.NO_BORDER;
            table.DefaultCell.BorderWidth = 0;
            table.TotalWidth = document.PageSize.Width - 50;
            table.LockedWidth = true;
            int[] widths = new int[] { 500 };
            table.SetWidths(widths);

            table.AddCell(this.writeCell("Ferie e permessi per l'anno " + _anno.ToString(), border, 5, 1, titleFont));


            table.WriteSelectedRows(0, table.Rows.Count + 1, lStartX, _currentY, cb);

            _currentY = _currentY - (int)table.CalculateHeights();

            this.currentY = _currentY;

            return (int)table.CalculateHeights();
        }
        private PdfPCell WriteLine(int border, int textAlign)
        {
            Chunk c = new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(0.0F, 530.0F, BaseColor.YELLOW, textAlign, 1));
            PdfPCell cellSeparator = new PdfPCell(new Phrase(c));
            cellSeparator.Border = border;
            cellSeparator.HorizontalAlignment = textAlign; //0=Left, 1=Centre, 2=Right
            return cellSeparator;
        }
        #endregion
    }
}
