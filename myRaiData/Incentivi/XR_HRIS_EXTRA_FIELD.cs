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
    
    public partial class XR_HRIS_EXTRA_FIELD
    {
        public int ID_FIELD { get; set; }
        public int ID_TIPOLOGIA { get; set; }
        public int ID_GESTIONE { get; set; }
        public string MATRICOLA { get; set; }
        public string COD_FIELD { get; set; }
        public Nullable<int> INT_VALUE { get; set; }
        public Nullable<decimal> DEC_VALUE { get; set; }
        public string STR_VALUE { get; set; }
        public Nullable<bool> BOOL_VALUE { get; set; }
        public Nullable<System.DateTime> DATE_VALUE { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
    
        public virtual XR_WKF_RICHIESTE XR_WKF_RICHIESTE { get; set; }
    }
}