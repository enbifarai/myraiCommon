using iTextSharp.text;
using iTextSharp.text.pdf;
using myRaiHelper;
using System;
using System.Collections.Generic;

namespace myRaiCommonModel.cvModels.Pdf
{
    public class RightContentManager
    {
        PdfContentByte canvas;

        Document document;

        public float IndentationLeft { get; set; }

        public string Title { get; set; }

        public string SubTitle { get; set; }

        List<ContentBlockInfo> blockInfoList;

        public RightContentManager(PdfContentByte canvas, Document document, List<ContentBlockInfo> blockInfoList)
        {
            this.canvas = canvas;
            this.document = document;
            this.blockInfoList = blockInfoList;
        }

        public void Render()
        {
            try
            {
                Paragraph title = new Paragraph(this.Title, new FontManager().H1);
                //title.IndentationLeft = this.IndentationLeft;
                //document.Add(title);

                Paragraph subTitle = new Paragraph(this.SubTitle, new FontManager().H3);
                //subTitle.IndentationLeft = this.IndentationLeft;
                //document.Add(subTitle);

                PdfPTable t = new PdfPTable(3);
                t.SetTotalWidth(new float[] { this.IndentationLeft + 48, PageSize.A4.Width - this.IndentationLeft - 48, 48f });

                t.LockedWidth = true;

                t.AddCell(new PdfPCell() { Border = 0 });

                PdfPCell cell = new PdfPCell()
                {
                    FixedHeight = 110f,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_TOP,
                    Border = 0
                };

                cell.AddElement(title);
                cell.AddElement(subTitle);

                t.AddCell(cell);



                t.AddCell(new PdfPCell() { Border = 0 });

                t.SpacingAfter = 24f;

                document.Add(t);



                int count = this.blockInfoList.Count;


                foreach (ContentBlockInfo blockInfo in this.blockInfoList)
                {
                    blockInfo.Render(document, this.IndentationLeft);
                    
                    count--;

                    if (count > 0)
                    {
                        if (!CommonHelper.IsProduzione())
                        {
                            float vp = canvas.PdfWriter.GetVerticalPosition(true);
                            float vp2 = canvas.PdfWriter.GetVerticalPosition(false);
                            if (vp < 150)
                                document.NewPage();
                            else
                                document.Add(Chunk.NEWLINE);
                        }
                        else
                        {
                            document.Add(Chunk.NEWLINE);
                        }
                    }
                }
            }
            catch (Exception e)
            { 
            }

        }

    }
}