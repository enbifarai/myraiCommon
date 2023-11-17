using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel.DocFirmaModels
{
    public class PDFmodel
    {
        public int idPdf { get; set; }
        public string datainizio { get; set; }
        public string datafine { get; set; }
        public string codSede { get; set; }
        public string PDFbase64 {get;set;}
        public string PdfCountInCarrello { get; set; }
        public string PdfCarrelloDisabledAttribute { get; set; }
        public string PdfEliminaDaCarrelloDisabledAttribute { get; set; }

        public string titolo { get; set; }
        public string datacompetenza { get; set; }
        public string datacontabile { get; set; }
        public string datapubblicazione { get; set; }
        public string nota { get; set; }
        public string idDocumento { get; set; }
        public string nomefile { get; set; }
    }
}