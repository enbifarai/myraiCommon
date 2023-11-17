using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel.cvModels.Pdf
{
    public class ContentBlockInfo
    {
        public List<RenderableItem> blockItemInfoList { get; set; }

        public string Title { get; set; }

        public ContentBlockInfo(string title, List<RenderableItem> blockItemInfoList)
        {
            // TODO: Complete member initialization
            this.Title = title;
            this.blockItemInfoList = blockItemInfoList;
        }

        internal void Render(Document document, float indentationLeft)
        {
            Chunk c = new Chunk(this.Title, new FontManager("", BaseColor.BLACK).H4);
            c.Font.Color = BaseColor.BLACK;
            //            c.setLineHeight(48);
            Paragraph title = new Paragraph(c);

            float chunkWidth = c.GetWidthPoint() + 12;

            float columnLeftWidth = indentationLeft;

            PdfPTable table = new PdfPTable(3);
            table.SetTotalWidth(new float[] { columnLeftWidth, chunkWidth, PageSize.A4.Width - 48 - columnLeftWidth - chunkWidth });
            table.LockedWidth = true;

            table.AddCell(new PdfPCell() { Rowspan = 2, Border = 0 });

            float h = 20;

            PdfPCell cell2 = new PdfPCell(new Phrase(c));
            cell2.Rowspan = 2;
            cell2.HorizontalAlignment = Element.ALIGN_LEFT; //0=Left, 1=Centre, 2=Right
            cell2.VerticalAlignment = Element.ALIGN_MIDDLE; //0=Left, 1=Centre, 2=Right
            cell2.Border = 0;
            cell2.PaddingLeft = 0;
            cell2.MinimumHeight = h;
            table.AddCell(cell2);

            PdfPCell lcell = new PdfPCell();
            lcell.Border = 1;
            lcell.DisableBorderSide(Rectangle.TOP_BORDER);
            lcell.DisableBorderSide(Rectangle.LEFT_BORDER);
            lcell.DisableBorderSide(Rectangle.RIGHT_BORDER);

            lcell.MinimumHeight = h;

            table.AddCell(lcell);


            lcell = new PdfPCell();
            lcell.Border = 1;
            lcell.BorderColor = BaseColor.LIGHT_GRAY;
            lcell.DisableBorderSide(Rectangle.BOTTOM_BORDER);
            lcell.DisableBorderSide(Rectangle.LEFT_BORDER);
            lcell.DisableBorderSide(Rectangle.RIGHT_BORDER);
            lcell.MinimumHeight = h;

            table.AddCell(lcell);
            document.Add(table);

            int count = blockItemInfoList.Count;

            foreach (RenderableItem item in blockItemInfoList)
            {
                item.IndentationLeft = indentationLeft;

                if (item.GetType().Equals(typeof(BlockItemInfo)))
                {
                    ((BlockItemInfo)item).Render(document, new FontManager("",BaseColor.BLACK).Normal, new FontManager().Italic);
                }
                else
                {
                    item.Render(document);
                }

                count--;

                if (count > 0)
                    document.Add(Chunk.NEWLINE);

            }

        }
    }
}