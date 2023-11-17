using System;
using System.Collections.Generic;

namespace myRaiCommonModel
{
    public class TechDashModel
    {
        public TechDashModel()
        {
            RichiestePerData = new List<data_Rich>();
            MatricoleAllaData = new List<data_Matr>();
            SediPdf = new List<sediPdfModel>();
           // AverageResponseTime = new double();
           
        }
        public List<data_Rich> RichiestePerData { get; set; }
        public List<data_Matr> MatricoleAllaData { get; set; }
        public List<sediPdfModel> SediPdf { get; set; }
        public ApprovazioniPendingModel SediDaAppr { get; set; }
        public double AverageResponseTime { get; set; }
        public double AverageResponseTimeHH { get; set; }
        public IntervalloOsservazione Intervallo { get; set; }
       
        
    }
    public class data_Rich
    {
        public DateTime da { get; set; }
        public string data { get; set; }
        public int richiesteTOT { get; set; }
    }
    public class data_Matr
    {
        public DateTime da { get; set; }
        public string data { get; set; }
        public int matricoleTOT { get; set; }
    }

    public class IntervalloOsservazione {

        public IntervalloOsservazione()
        {
            avg = 0;
            avg1 = 0;
            Avg_Call = new List<call_Avg>();
        }
        public DateTime da { get; set; }
        public DateTime a { get; set; }
        
        public double avg { get; set; }

        public double avg1 { get; set; }
        public int IntervalloMin { get; set; }
        public int IntervalloRefresh { get; set; }
        public List<call_Avg> Avg_Call { get; set; }



    }
    public class call_Avg
    {
        
        public string NomeChiamata { get; set; }
        public double AvgCall { get; set; }
    }
}