using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using myRaiData;
using myRaiData.Incentivi;
using Newtonsoft.Json;

namespace myRaiCommonModel
{
    public class NodeModel
    {
        public int id { get; set; }
        public int pid { get; set; }
        public string Direzione { get; set; }
        public string Missione { get; set; }
        public string Nominativo { get; set; }
        
        public string Mansione { get; set; }
        //public string img { get; set; }
        [DisplayName("Data fine validità")]
        public string Data_Di_Validita { get; set; }
        public int Numero_Risorse_Strut { get; set; }
        public char[] Tags { get; set; }
    }
}
