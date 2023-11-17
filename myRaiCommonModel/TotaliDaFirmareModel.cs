using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace myRaiCommonModel
{
    public class TotaliDaFirmareModel
    {
        public int Totale { get; set; }
        public List<TotDaFirmare> RowPerSede { get; set; }
    }
    public class TotDaFirmare
    {
        public string codiceSede { get; set; }
        public string descSede { get; set; }
        public int InCarrello { get; set; }
        public int TotaleSede { get; set; }
    }
}