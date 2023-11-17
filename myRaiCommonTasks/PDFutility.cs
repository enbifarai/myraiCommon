using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace myRaiCommonTasks
{
    public class PDFutility
    {
        public static byte[] AggiungiImmagineSuPdf(byte[] pdf, string immagine, float x, float y)
        {
            PdfReader reader = new PdfReader(pdf);

            int totalPages = reader.NumberOfPages;

            iTextSharp.text.Document document = new iTextSharp.text.Document(reader.GetPageSizeWithRotation(1));
            MemoryStream MS = new MemoryStream();
            PdfWriter writer = PdfWriter.GetInstance(document, MS);

            document.Open();
            for (int pageNumber = 1; pageNumber <= totalPages; pageNumber++)
            {
                document.NewPage();
                PdfImportedPage page = writer.GetImportedPage(reader, pageNumber);
                writer.DirectContent.AddTemplate(page, 0, 0);
            }

            iTextSharp.text. Image image = iTextSharp.text.Image.GetInstance(immagine);
            image.SetAbsolutePosition(x, y);

            PdfContentByte contentByte = writer.DirectContent;
            contentByte.AddImage(image);

            document.Close();
            writer.Close();
            reader.Close();

            return MS.ToArray();
        }
    }
}
