using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiCommonModel.cvModels.Pdf
{
    public class KeyValue : RenderableItem
    {
        public string key { get; set; }
        public string value { get; set; }
        public Chunk chk { get; set; }
        public override void Render(ColumnText ct)
        {
            ct.AddElement(new Paragraph(key, new FontManager("",BaseColor.BLACK).H4) { IndentationLeft = this.IndentationLeft });
            ct.AddElement(new Paragraph(value, new FontManager("", BaseColor.BLACK).Normal) { IndentationLeft = this.IndentationLeft });
            ct.AddElement(new Paragraph(chk == null? new Chunk(): chk ) { IndentationLeft = this.IndentationLeft });
        }

        public virtual void Render(ColumnText ct, Font keyFont, Font valueFont)
        {
            ct.AddElement(new Paragraph(key, keyFont) { IndentationLeft = this.IndentationLeft });
            ct.AddElement(new Paragraph(value, valueFont) { IndentationLeft = this.IndentationLeft });
            ct.AddElement(new Paragraph(chk == null ? new Chunk() : chk) { IndentationLeft = this.IndentationLeft });
        }

        public virtual void Render(Document document, Font keyFont, Font valueFont)
        {
            document.Add(new Paragraph(key, keyFont) { IndentationLeft = this.IndentationLeft });
            document.Add(new Paragraph(value, valueFont) { IndentationLeft = this.IndentationLeft });
            document.Add(new Paragraph(chk == null ? new Chunk() : chk) { IndentationLeft = this.IndentationLeft });
        }

        public override void Render(Document document)
        {
            document.Add(new Paragraph(key, new FontManager("", BaseColor.BLACK).H4) { IndentationLeft = this.IndentationLeft });
            document.Add(new Paragraph(value, new FontManager("", BaseColor.BLACK).Normal) { IndentationLeft = this.IndentationLeft });
            document.Add(new Paragraph(chk == null ? new Chunk() : chk) { IndentationLeft = this.IndentationLeft });
        }

    }

    public class KeyValueJustified : RenderableItem
    {
        public string key { get; set; }
        public string value { get; set; }
        public Chunk chk { get; set; }
        public override void Render(ColumnText ct)
        {
            ct.AddElement(new Paragraph(key, new FontManager("", BaseColor.BLACK).H4) { IndentationLeft = this.IndentationLeft, Alignment=Element.ALIGN_JUSTIFIED });
            ct.AddElement(new Paragraph(value, new FontManager("", BaseColor.BLACK).Normal) { IndentationLeft = this.IndentationLeft, Alignment = Element.ALIGN_JUSTIFIED });
            ct.AddElement(new Paragraph(chk == null ? new Chunk() : chk) { IndentationLeft = this.IndentationLeft, Alignment = Element.ALIGN_JUSTIFIED });
        }

        public virtual void Render(ColumnText ct, Font keyFont, Font valueFont)
        {
            ct.AddElement(new Paragraph(key, keyFont) { IndentationLeft = this.IndentationLeft, Alignment = Element.ALIGN_JUSTIFIED });
            ct.AddElement(new Paragraph(value, valueFont) { IndentationLeft = this.IndentationLeft, Alignment = Element.ALIGN_JUSTIFIED });
            ct.AddElement(new Paragraph(chk == null ? new Chunk() : chk) { IndentationLeft = this.IndentationLeft, Alignment = Element.ALIGN_JUSTIFIED });
        }

        public virtual void Render(Document document, Font keyFont, Font valueFont)
        {
            document.Add(new Paragraph(key, keyFont) { IndentationLeft = this.IndentationLeft, Alignment = Element.ALIGN_JUSTIFIED });
            document.Add(new Paragraph(value, valueFont) { IndentationLeft = this.IndentationLeft, Alignment = Element.ALIGN_JUSTIFIED });
            document.Add(new Paragraph(chk == null ? new Chunk() : chk) { IndentationLeft = this.IndentationLeft, Alignment = Element.ALIGN_JUSTIFIED });
        }

        public override void Render(Document document)
        {
            document.Add(new Paragraph(key, new FontManager("", BaseColor.BLACK).H4) { IndentationLeft = this.IndentationLeft, Alignment = Element.ALIGN_JUSTIFIED });
            document.Add(new Paragraph(value, new FontManager("", BaseColor.BLACK).Normal) { IndentationLeft = this.IndentationLeft, Alignment = Element.ALIGN_JUSTIFIED });
            document.Add(new Paragraph(chk == null ? new Chunk() : chk) { IndentationLeft = this.IndentationLeft, Alignment = Element.ALIGN_JUSTIFIED });
        }

    }

    public class KeyValueInline : RenderableItem
    {
        public string key { get; set; }
        public string value { get; set; }

        public string separator { get; set; }

        public KeyValueInline()
        {
            separator = ":";
        }

        public override void Render(ColumnText ct)
        {
            Paragraph p = new Paragraph();
            p.Alignment = Element.ALIGN_JUSTIFIED;
            p.Font = new FontManager("", BaseColor.BLACK).Normal;
            p.IndentationLeft = this.IndentationLeft ;
            if (!String.IsNullOrWhiteSpace(key))
            {
                p.Add(new Chunk(key, new FontManager("", BaseColor.BLACK).Bold));
                p.Add(new Chunk(separator + " "));
            }
            p.Add(new Chunk(value));

            ct.AddElement(p);
        }

        public virtual void Render(ColumnText ct, Font keyFont, Font valueFont)
        {
            Paragraph p = new Paragraph();
            p.Alignment = Element.ALIGN_JUSTIFIED;
            p.Font = valueFont;
            p.IndentationLeft = this.IndentationLeft;
            if (!String.IsNullOrWhiteSpace(key))
            {
                p.Add(new Chunk(key, keyFont));
                p.Add(new Chunk(separator + " "));
            }
            p.Add(new Chunk(value));

            ct.AddElement(p);
        }

        public virtual void Render(Document document, Font keyFont, Font valueFont)
        {
            Paragraph p = new Paragraph();
            p.Alignment = Element.ALIGN_JUSTIFIED;
            p.Font = valueFont;
            p.IndentationLeft = this.IndentationLeft;
            if (!String.IsNullOrWhiteSpace(key))
            {
                p.Add(new Chunk(key, keyFont));
                p.Add(new Chunk(separator + " "));
            }
            p.Add(new Chunk(value));

            document.Add(p);
        }

        public override void Render(Document document)
        {
            Paragraph p = new Paragraph();
            p.Alignment = Element.ALIGN_JUSTIFIED;
            p.Font = new FontManager("", BaseColor.BLACK).Normal;
            p.IndentationLeft = this.IndentationLeft;
            if (!String.IsNullOrWhiteSpace(key))
            {
                p.Add(new Chunk(key, new FontManager("", BaseColor.BLACK).Bold));
                p.Add(new Chunk(separator + " "));
            }
            p.Add(new Chunk(value));

            document.Add(p);
        }
    }

    public class KeyValueInlineBold : RenderableItem
    {
        public string key { get; set; }
        public string value { get; set; }
        public string subvalue { get; set; }

        public string separator { get; set; }

        public KeyValueInlineBold()
        {
            separator = ":";
        }

        public override void Render(ColumnText ct)
        {
            Paragraph p = new Paragraph();
            p.Alignment = Element.ALIGN_JUSTIFIED;
            p.Font = new FontManager("", BaseColor.BLACK).Normal;
            p.IndentationLeft = this.IndentationLeft;
            if (!String.IsNullOrWhiteSpace(key))
            {
                p.Add(new Chunk(key, new FontManager("", BaseColor.BLACK).Bold));
                p.Add(new Chunk(separator + " "));
            }
            p.Add(new Chunk(value));

            ct.AddElement(p);

            if (!String.IsNullOrWhiteSpace(subvalue))
                ct.AddElement(new Paragraph(subvalue, new FontManager("", BaseColor.BLACK).Italic)
                {
                    Alignment = Element.ALIGN_JUSTIFIED,
                    IndentationLeft = this.IndentationLeft
                });
        }

        public virtual void Render(ColumnText ct, Font keyFont, Font valueFont)
        {
            Paragraph p = new Paragraph();
            p.Alignment = Element.ALIGN_JUSTIFIED;
            p.Font = valueFont;
            p.IndentationLeft = this.IndentationLeft;
            if (!String.IsNullOrWhiteSpace(key))
            {
                p.Add(new Chunk(key, keyFont));
                p.Add(new Chunk(separator + " "));
            }
            p.Add(new Chunk(value));

            ct.AddElement(p);

            if (!String.IsNullOrWhiteSpace(subvalue))
                ct.AddElement(new Paragraph(subvalue, new FontManager("", BaseColor.BLACK).Italic)
                {
                    Alignment = Element.ALIGN_JUSTIFIED,
                    IndentationLeft = this.IndentationLeft
                });
        }

        public virtual void Render(Document document, Font keyFont, Font valueFont)
        {
            Paragraph p = new Paragraph();
            p.Alignment = Element.ALIGN_JUSTIFIED;
            p.Font = valueFont;
            p.IndentationLeft = this.IndentationLeft;
            if (!String.IsNullOrWhiteSpace(key))
            {
                p.Add(new Chunk(key, keyFont));
                p.Add(new Chunk(separator + " "));
            }
            p.Add(new Chunk(value));

            document.Add(p);

            if (!String.IsNullOrWhiteSpace(subvalue))
                document.Add(new Paragraph(subvalue, new FontManager("", BaseColor.BLACK).Italic)
                {
                    Alignment = Element.ALIGN_JUSTIFIED,
                    IndentationLeft = this.IndentationLeft
                });
        }

        public override void Render(Document document)
        {
            Paragraph p = new Paragraph();
            p.Alignment = Element.ALIGN_JUSTIFIED;
            p.Font = new FontManager("", BaseColor.BLACK).Normal;
            p.IndentationLeft = this.IndentationLeft;
            if (!String.IsNullOrWhiteSpace(key))
            {
                p.Add(new Chunk(key, new FontManager("", BaseColor.BLACK).Bold));
                p.Add(new Chunk(separator + " "));
            }
            p.Add(new Chunk(value));

            document.Add(p);

            if (!String.IsNullOrWhiteSpace(subvalue))
                document.Add(new Paragraph(subvalue, new FontManager("", BaseColor.BLACK).Italic)
                {
                    Alignment = Element.ALIGN_JUSTIFIED,
                    IndentationLeft = this.IndentationLeft
                });
        }
    }

    public class KeyValueList: RenderableItem
    {
        public string key { get; set; }
        public List<RenderableItem> values { get; set; }

        public Chunk chk { get; set; }

        public KeyValueList()
        {
            values = new List<RenderableItem>();
        }

        public override void Render(ColumnText ct)
        {
            ct.AddElement(new Paragraph(key, new FontManager("", BaseColor.BLACK).H4) { IndentationLeft = this.IndentationLeft });
            foreach (var item in values)
            {
                item.IndentationLeft = this.IndentationLeft;
                item.Render(ct);
            }
            if (chk!=null)
                ct.AddElement(new Paragraph(chk) { IndentationLeft = this.IndentationLeft, Alignment = Element.ALIGN_JUSTIFIED });
        }

        public virtual void Render(ColumnText ct, Font keyFont, Font valueFont)
        {
            ct.AddElement(new Paragraph(key, keyFont) { IndentationLeft = this.IndentationLeft });
            foreach (var item in values)
            {
                item.IndentationLeft = this.IndentationLeft;
                item.Render(ct);
            }
            if (chk != null)
                ct.AddElement(new Paragraph(chk) { IndentationLeft = this.IndentationLeft, Alignment = Element.ALIGN_JUSTIFIED });
        }

        public virtual void Render(Document document, Font keyFont, Font valueFont)
        {
            document.Add(new Paragraph(key, keyFont) { IndentationLeft = this.IndentationLeft });
            foreach (var item in values)
            {
                item.IndentationLeft = this.IndentationLeft;
                item.Render(document);
            }
            if (chk!=null)
                document.Add(new Paragraph( chk) { IndentationLeft = this.IndentationLeft, Alignment = Element.ALIGN_JUSTIFIED });
        }

        public override void Render(Document document)
        {
            document.Add(new Paragraph(key, new FontManager("", BaseColor.BLACK).H4) { IndentationLeft = this.IndentationLeft });
            foreach (var item in values)
            {
                item.IndentationLeft = this.IndentationLeft;
                item.Render(document);
            }
            if (chk != null)
                document.Add(new Paragraph(chk) { IndentationLeft = this.IndentationLeft, Alignment = Element.ALIGN_JUSTIFIED });
        }

    }
}
