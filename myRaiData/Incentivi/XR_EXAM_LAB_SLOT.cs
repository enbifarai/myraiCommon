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
    
    public partial class XR_EXAM_LAB_SLOT
    {
        public int ID_SLOT { get; set; }
        public int ID_LAB { get; set; }
        public string DAYS_OF_WEEK { get; set; }
        public int INIZIO_HH { get; set; }
        public int INIZIO_MM { get; set; }
        public int FINE_HH { get; set; }
        public int FINE_MM { get; set; }
        public Nullable<int> NMB_MAX_ESAMI { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
    }
}