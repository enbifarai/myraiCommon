//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace myRaiData.Incentivi
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
        public Nullable<System.DateTime> DTA_INVIO { get; set; }
    }
}