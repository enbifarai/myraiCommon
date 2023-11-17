using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel.Tech
{
    public class TechApprovazioneRichiesteVM
    {
        public double GiorniAttesaApprovazione { get; set; }

        public double GiorniAttesaConsuntivazioneRichiesta { get; set; }

        public double GiorniConsuntivazioneEccezioneUtente { get; set; }
    }


    public class TechApprovatoriProduzioneVM
    {
        public TechApprovatoriProduzioneVM ( )
        {
            this.ApprovatoriStati = new List<TechApprovatoreProduzioneItem>( );
            this.DataDA = null;
            this.DataA = null;
        }

        public DateTime? DataDA { get; set; }
        public DateTime? DataA { get; set; }

        public List<TechApprovatoreProduzioneItem> ApprovatoriStati { get; set; }
    }

    public class TechApprovatoreProduzioneItem
    {
        public string Nominativo { get; set; }
        public string Matricola { get; set; }
        public int InApprovazione { get; set; }
        public int Approvate { get; set; }
        public string SedeGapp { get; set; }
    }
}