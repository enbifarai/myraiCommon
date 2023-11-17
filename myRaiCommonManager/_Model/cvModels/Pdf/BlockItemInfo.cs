using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel.cvModels.Pdf
{
    public class BlockItemInfo : KeyValue
    {
        public override void Render(ColumnText ct)
        {
            ct.AddElement(new Paragraph(key, new FontManager("",BaseColor.BLACK).Normal));
            ct.AddElement(new Paragraph(value, new FontManager("", BaseColor.BLACK).Normal));
            ct.AddElement(new Paragraph(chk));


        }
    }
}