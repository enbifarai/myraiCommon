using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel.Controllo
{
    public class Report
    {

        public List<sitMese> Monthly { get; set; }
    }

    public class sitMese
    {
        public int numrichieste { get; set; }
        public int nummatricole { get; set; }
        public string mese { get; set; }
    }
}