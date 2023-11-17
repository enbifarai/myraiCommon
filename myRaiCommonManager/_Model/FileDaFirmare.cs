using System;
using System.Collections.Generic;

namespace myRaiCommonModel
{
    public class FileDaFirmare
    {
        public string SID { get; set; }
        public string IP_CLI { get; set; }
        public string UID { get; set; }
        public string[] PDF { get; set; }
        public PDFVALUE[] PDFVALUE { get; set; }

    }

    public class FIELD
    {
        public string TYPE { get; set; }
        public string PAGE { get; set; }
        public string NAME { get; set; }
        public string X { get; set; }
        public string Y { get; set; }
        public string WIDTH { get; set; }
        public string HEIGHT { get; set; }
        public string REQ { get; set; }
        public string VALUE { get; set; }
    }

    public class PDFVALUE
    {
        public FIELD[] FIELD { get; set; }
    }
}
