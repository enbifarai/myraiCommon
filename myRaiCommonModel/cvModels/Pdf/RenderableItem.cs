using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel.cvModels.Pdf
{
    public abstract class RenderableItem
    {
        public bool newLine = true;

        public float IndentationLeft { get; set; }

        public abstract void Render(ColumnText ct);

        public abstract void Render(Document document);

        public virtual void RenderNewLine(ColumnText ct)
        {
            ct.AddElement(Chunk.NEWLINE);
            Render(ct);
        }

        public virtual void RenderNewLine(Document document)
        {
            document.Add(Chunk.NEWLINE);
            Render(document);
        }
    }
}