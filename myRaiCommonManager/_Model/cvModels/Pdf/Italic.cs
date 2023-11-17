using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel.cvModels.Pdf
{
    public class Italic : RenderableItem
    {
        public string value { get; set; }

        public override void RenderNewLine(ColumnText ct)
        {
            Render(ct);
        }

        public override void Render(ColumnText ct)
        {
            ct.AddElement(new Paragraph(value, new FontManager("", BaseColor.BLACK).Italic) { IndentationLeft = this.IndentationLeft });
        }

        public override void Render(Document document)
        {
            document.Add(new Paragraph(value, new FontManager("", BaseColor.BLACK).Italic) { IndentationLeft = this.IndentationLeft });
        }

    }
}