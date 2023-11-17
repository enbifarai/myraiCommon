using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel
{
    public class POHmodel
    {
        public POHmodel()
        {
            GiorniPOH = new List<SingleDay>();
            GiorniROH = new List<SingleDay>();
        }
        public List<SingleDay> GiorniPOH { get; set; }
        public List<SingleDay> GiorniROH { get; set; }
        public int? Anno { get; set; }
       
    }
    public class SingleDay
    {
        public DateTime data { get; set; }
        public int minuti { get; set; }
        public int IdStato { get; set; }
        public int IdRichiesta { get; set; }
        public string codice { get; set; }

        public int SaldoAttualeCompresaEccezione { get; set; }
        public string SaldoAttualeCompresaEccezioneHHMM { get; set; }
    }
}