//------------------------------------------------------------------------------
// <auto-generated>
//    Codice generato da un modello.
//
//    Le modifiche manuali a questo file potrebbero causare un comportamento imprevisto dell'applicazione.
//    Se il codice viene rigenerato, le modifiche manuali al file verranno sovrascritte.
// </auto-generated>
//------------------------------------------------------------------------------

namespace myRaiDataTalentia
{
    using System;
    using System.Collections.Generic;
    
    public partial class XR_SSV_CODICE_OTP
    {
        public int ID_CODICE_OTP { get; set; }
        public int ID_EVENTO { get; set; }
        public string MATRICOLA { get; set; }
        public string IND_UTILIZZO { get; set; }
        public System.DateTime DTA_UTILIZZO { get; set; }
        public System.DateTime DTA_SCADENZA { get; set; }
        public string COD_FUNZIONE { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
    }
}