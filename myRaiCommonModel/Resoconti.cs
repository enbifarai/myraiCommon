using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using myRaiHelper;
using MyRaiServiceInterface.it.rai.servizi.digigappws;

namespace myRaiCommonModel
{
    public class ResocontiModel
    {
        public sidebarModel menuSidebar { get; set; }

        public List<SedePresenzeModel> SediPresenze { get; set; }

        public bool onlypreview { get; set; }
    }
    public class SedePresenzeModel
    {
        public SedePresenzeModel()
        {
            MieiReparti = new List<string>();
			this.PDFdaRigenerare = false;
        }
        public List<DettaglioDifferenze> DifferenzeCachePDF { get; set; }
        public DateTime DataDa { get; set; }
        public DateTime DataA { get; set; }
        public string CodiceSede { get; set; }
        public string DescrizioneSede { get; set; }
        public presenzeResponse presenze { get; set; }
        public Boolean IsPDFpresent { get; set; }
        public DateTime? DataPDFgenerato { get; set; }
        public DateTime? DataPDFfirmato { get; set; }
        public Boolean IsNextWeekBrowsable { get; set; }
        public Boolean IsCurrentWeekPrimaDelBlocco { get; set; }

        public Boolean IsPDFapprovabile { get; set; } //sono passati N giorni dalla domenica?
        public Boolean MieiRepartiGiaVisionati { get; set; }
        public Boolean SonoResponsabileDiReparti { get; set; }
        public int PdfAncoraDaGenerare { get; set; }

        public DateTime? DataInizioPDFseUnoSoloDaGenerare { get; set; }
        public List<string> MieiReparti { get; set; }
        public Dictionary<string,string> MieiRepartiDes { get; set; }
        

		/// <summary>
		/// Indica se ci sono pdf da rigenerare per quella sede
		/// </summary>
		public bool PDFdaRigenerare { get; set; }

		/// <summary>
		/// Numero di pdf da rigenerare
		/// </summary>
		public int ConteggioPDFDaRigenerare { get; set; }

        public List<DateTime> DateConPDFDaRigenerareOFirmare { get; set; }
    }

    public class DettaglioDifferenze
    {
        public string matricola { get; set; }
        public string deltaTotalePDF { get; set; }
        public string deltaTotaleCache { get; set; }
    }

    public class NeedToReprint
    {
        public Boolean Esito { get; set; }
        public List<DettaglioDifferenze> DettaglioDiff { get; set; }
    }
}