using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel.cvModels.Pdf
{
    public class SingleValue : RenderableItem
    {
        public string value { get; set; }

        public override void Render(ColumnText ct)
        {
            ct.AddElement(new Paragraph(value, new FontManager().Normal) { IndentationLeft = this.IndentationLeft });
        }

        public virtual void Render(ColumnText ct, Font keyFont, Font valueFont)
        {
            ct.AddElement(new Paragraph(value, valueFont) { IndentationLeft = this.IndentationLeft });
        }

        public virtual void Render(Document document, Font keyFont, Font valueFont)
        {
            document.Add(new Paragraph(value, valueFont) { IndentationLeft = this.IndentationLeft });
        }

        public override void Render(Document document)
        {
            document.Add(new Paragraph(value, new FontManager().Normal) { IndentationLeft = this.IndentationLeft });
        }
    }

    //public class MultiLineValue : RenderableItem
    //{
    //    public SingleValue[] value { get; set; }

    //    public MultiLineValue()
    //    {
    //        value = new SingleValue[0];
    //    }

    //    public override void Render(ColumnText ct)
    //    {
    //        foreach(SingleValue s in value)
    //        {
    //            s.Render(ct);
    //            //ct.AddElement(new Paragraph(s, new FontManager().Normal) { IndentationLeft = this.IndentationLeft });
    //        }
    //    }

    //    public virtual void Render(ColumnText ct, Font keyFont, Font valueFont)
    //    {
    //        foreach (SingleValue s in value)
    //        {
    //            s.Render(ct, keyFont, valueFont);
    //            //ct.AddElement(new Paragraph(s, valueFont) { IndentationLeft = this.IndentationLeft });
    //        }
    //    }

    //    public virtual void Render(Document document, Font keyFont, Font valueFont)
    //    {
    //        foreach (SingleValue s in value)
    //        {
    //            s.Render(document, keyFont, valueFont);
    //            //document.Add(new Paragraph(s, valueFont) { IndentationLeft = this.IndentationLeft });
    //        }
    //    }

    //    public override void Render(Document document)
    //    {
    //        foreach (SingleValue s in value)
    //        {
    //            s.Render(document);
    //            //document.Add(new Paragraph(s, new FontManager().Normal) { IndentationLeft = this.IndentationLeft });
    //        }
    //    }
    //}
}