using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel.cvModels.Pdf
{
    public class LineBreak : RenderableItem
    {

        public override void RenderNewLine(ColumnText ct)
        {
            Render(ct);
        }

        public override void Render(ColumnText ct)
        {
            Chunk linebreak = new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(1f, 100f, BaseColor.LIGHT_GRAY, Element.ALIGN_LEFT, 1));
            ct.SetIndent(this.IndentationLeft, true);
            ct.AddElement(linebreak);
        }

        public override void Render(Document document)
        {
            Chunk linebreak = new Chunk(new iTextSharp.text.pdf.draw.LineSeparator(1f, 100f, BaseColor.LIGHT_GRAY, Element.ALIGN_LEFT, 1));
            document.Add(linebreak);
        }
    }
}