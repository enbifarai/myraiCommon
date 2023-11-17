using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel.cvModels.Pdf
{
    public class SmallItalic : Italic
    {

        public override void Render(ColumnText ct)
        {
            ct.AddElement(new Paragraph(value, new FontManager().SmallItalic) { Alignment = Element.ALIGN_CENTER });
        }
    }
}