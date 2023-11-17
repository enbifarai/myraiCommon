using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myRaiCommonModel
{
    public class sediPdfModel
    {
        public sediPdfModel()
        {
            files = new List<pdfFile> ();
        }
        public string sede { get; set; }
        public List<pdfFile> files { get; set; }
    }
    public class pdfFile
    {
        public DateTime dal { get; set; }
        public DateTime al { get; set; }
        public int id { get; set; }
        public string status { get; set; }
        public int versione { get; set; }
        public string matricola_conv { get; set; }
        public string matricola_stampa { get; set; }
        public DateTime? data_conv { get; set; }
        public DateTime? data_stampa { get; set; }
    }

    public class sediDaApprovareModel
    {
        public sediDaApprovareModel()
        {
           
        }
        public string sede { get; set; }
        public int richiesteDaApprovare { get; set; }
        public string[] NominativiL1 { get; set; }
       
    }
    public class ApprovazioniPendingModel
    {
        public ApprovazioniPendingModel()
        {
            MesePrec = new List<sediDaApprovareModel>();
            MeseCorr = new List<sediDaApprovareModel>();
        }
        public List<sediDaApprovareModel> MesePrec { get; set; }
        public List<sediDaApprovareModel> MeseCorr { get; set; }
    }
}
