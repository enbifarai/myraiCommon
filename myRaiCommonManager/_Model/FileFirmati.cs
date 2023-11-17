using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace myRaiCommonModel
{
    public class FileFirmati
    {
        
        public string IP_CLI { get; set; }
        public string[] PDFDATA { get; set; }
        public string UID { get; set; }
        public string esito { get; set; }
        public string descrErrore { get; set; }
        public string TOKEN { get; set; }
        //public string[] FIRMA { get; set; }
        public string[] PDF { get; set; }

    }
}
