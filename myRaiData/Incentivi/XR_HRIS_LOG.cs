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
    
    public partial class XR_HRIS_LOG
    {
        public int ID_LOG { get; set; }
        public int ID_PERSONA { get; set; }
        public string COD_MATRICOLA { get; set; }
        public string DES_OPERAZIONE { get; set; }
        public string NOT_PARAMETRI { get; set; }
        public bool IND_ESITO { get; set; }
        public string NOT_ERRORE { get; set; }
        public string NOT_ESITO { get; set; }
        public string NOT_ADDITIONAL_INFO { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public Nullable<System.DateTime> TMS_TIMESTAMP { get; set; }
    }
}
