using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiService.classi
{
    public class RecuperaPdfResponse
    {
        public RecuperaPdfResponse()
        {
            esito = true;
            error = null;
        }
        public bool esito { get; set; }
        public string error { get; set; }
        public  Pdf[] PdfList { get; set; }

    }
    public class Pdf
    {
        public string tipo { get; set; }
        public byte[] content { get; set; }
        public Int32 ID { get; set; }
        public String stato_pdf { get; set; }
        public Int32 numero_versione { get; set; }
        public DateTime DataDa { get; set; }
        public DateTime DataA { get; set; }
    }
}