using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace myRaiCommonModel.cvModels.Pdf
{
    public class LeftColumnManager
    {
        PdfContentByte canvas;

        Image image;

        List<RenderableItem> listItemRow;

        float llx = 24f, lly = 24f, urx = 142f, ury = PageSize.A4.Height - 24f;

        float imageWidth = 120f, imageHeight = 120f;

        public LeftColumnManager(PdfContentByte canvas)
        {
            this.canvas = canvas;

            listItemRow = new List<RenderableItem>();
        }

        public LeftColumnManager(PdfContentByte canvas, List<RenderableItem> rowItemList, string base64)
        {
            this.canvas = canvas;

            try
            {
                string pattern = @"data:image/(gif|png|jpeg|jpg);base64,";
                string imgString = Regex.Replace(base64, pattern, string.Empty);
                if (!String.IsNullOrWhiteSpace(imgString))
                    image = Image.GetInstance(Convert.FromBase64String(imgString));
            }
            catch (Exception)
            {
                //TODO: ERROR ON BASE64
            }

            listItemRow = rowItemList;
        }

        public void Render(bool firstPage)
        {
            float marginTop = ury;

            if (firstPage)
            {
                if (image != null)
                {
                    image.ScaleToFit(imageWidth, imageHeight);
                    image.SetAbsolutePosition(lly, ury - imageHeight);
                    canvas.AddImage(image);
                }

                marginTop = ury - imageHeight - 7f;
            }

            ColumnText ct = new ColumnText(canvas);
            ct.SetSimpleColumn(llx, lly, urx, marginTop);

            foreach (RenderableItem item in listItemRow)
            {
                if (item.newLine)
                    item.RenderNewLine(ct);
                else
                    item.Render(ct);
            }

            ct.Go();
        }
    }
}