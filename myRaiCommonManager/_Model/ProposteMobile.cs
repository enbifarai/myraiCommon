using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel
{
    public class ProposteAutoMob
    {
        public List<GiornataMob> giornate { get; set; }
        public ProposteAutoMob()
        {
            this.giornate = new List<GiornataMob>();
        }
    }



    public class GiornataMob
    {
        public GiornataMob()
        {
            this.eccezioni = new List<EccezioneMob>();
        }
        public string data { get; set; }
        public List<EccezioneMob> eccezioni { get; set; }
        
    }
    public class EccezioneMob
    {
        public string cod { get; set; }
        public string desc { get; set; }
        public string dalle { get; set; }
        public string alle { get; set; }
        public string qta { get; set; }
        public int caratteri { get; set; }
        public string motivo { get; set; }
    }
}