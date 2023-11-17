using iTextSharp.text;
using iTextSharp.text.pdf;
using myRaiHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel.cvModels.Pdf
{
    public class ITextEvents : PdfPageEventHelper
    {
        string imagePath = "";
        List<RenderableItem> rowItemList;
        string updatedValue;

        public ITextEvents(string image, List<RenderableItem> rowItemList)
        {
            this.imagePath = image;
            this.rowItemList = rowItemList;
        }

        private void addLeftItemInfo(ColumnText ct, string key, string value)
        {
            FontManager fm = new FontManager();

            ct.AddElement(new Paragraph(key, fm.H4));
            ct.AddElement(new Paragraph(value, fm.Normal));
            ct.AddElement(Chunk.NEWLINE);
        }

        public override void OnEndPage(PdfWriter writer, Document document)
        {
            //string path = @"FONTS\opensans.ttf";

            //FontManager fm = new FontManager(HttpContext.Current.Server.MapPath("~/FONTS/opensans.ttf"), new7 BaseColor(255, 255, 255));
            FontManager fm = new FontManager(CommonHelper.GetFilePath("~/FONTS/opensans.ttf"), new BaseColor(255, 255, 255));

            //BaseFont customfont = BaseFont.CreateFont(path, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            //Font _font = new Font(customfont, 12, Font.NORMAL, new BaseColor(255, 255, 255));

            Font _font = fm.N12;

            //Phrase header = new Phrase("this is a header", _font);
            //Phrase footer = new Phrase("this is a footer", _font);
            //ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER,
            //        header,
            //        (document.Right - document.Left) / 2 + document.LeftMargin,
            //        document.Top + 10, 0);


            PdfContentByte canvas = writer.DirectContentUnder;


            Image image = Image.GetInstance(Cons.RAI_ICON);
            image.ScaleAbsolute(24, 24);
            image.SetAbsolutePosition(PageSize.A4.Width - 24, PageSize.A4.Height - 24);
            canvas.AddImage(image);

            var rect = new iTextSharp.text.Rectangle(0, 0, 166, PageSize.A4.Height);
            rect.BorderWidth = 0;
            rect.BackgroundColor = new BaseColor(229, 229, 229, 50);
            canvas.Rectangle(rect);

            //ColumnText.ShowTextAligned(cb, Element.ALIGN_CENTER,
            //        footer,
            //        (document.Right - document.Left) / 2 + document.LeftMargin,
            //        document.Bottom - 10, 0);

            PdfShading shading = PdfShading.SimpleAxial(writer, PageSize.A4.Width - 24f, 700, PageSize.A4.Width, 700, new BaseColor(180, 237, 80), new BaseColor(180, 237, 80));
            PdfShadingPattern pattern = new PdfShadingPattern(shading);
            ShadingColor color = new ShadingColor(pattern);

            PdfPTable t = new PdfPTable(3);
            t.SetTotalWidth(new float[] { 24, PageSize.A4.Width - 48, 24 });

            t.LockedWidth = true;



            t.AddCell(new PdfPCell() { Border = 0 });

            t.AddCell(new PdfPCell() { FixedHeight = 24f, Border = 0, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });

            t.AddCell(new PdfPCell(new Phrase(string.Format("{0}", writer.PageNumber), _font)) { FixedHeight = 24f, BackgroundColor = color, Border = 0, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE });

            t.WriteSelectedRows(0, -1, 0, 24f, writer.DirectContent);


            

            PdfContentByte cb = writer.DirectContent;

            LeftColumnManager lm = new LeftColumnManager(cb, rowItemList, this.imagePath);

            lm.Render(writer.PageNumber.Equals(1));
        }
    }
}