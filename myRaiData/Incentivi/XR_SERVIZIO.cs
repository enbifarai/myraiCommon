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
    
    public partial class XR_SERVIZIO
    {
        public int ID_XR_SERVIZIO { get; set; }
        public string COD_EVSERVIZIO { get; set; }
        public int ID_PERSONA { get; set; }
        public System.DateTime DTA_INIZIO { get; set; }
        public string COD_SERVIZIO { get; set; }
        public System.DateTime DTA_FINE { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
        public Nullable<int> PROG_EVENTO { get; set; }
    
        public virtual SINTESI1 SINTESI1 { get; set; }
        public virtual XR_TB_SERVIZIO XR_TB_SERVIZIO { get; set; }
        public virtual SINTESI1 HISTORY { get; set; }
        public virtual ANAGPERS ANAGPERS { get; set; }
    }
}
