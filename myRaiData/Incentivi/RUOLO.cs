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
    
    public partial class RUOLO
    {
        public RUOLO()
        {
            this.JOBASSIGN = new HashSet<JOBASSIGN>();
        }
    
        public string COD_RUOLO { get; set; }
        public Nullable<int> ID_COMPSTR { get; set; }
        public string COD_LIVEQUIV { get; set; }
        public string COD_QUALSTD { get; set; }
        public string COD_FASCIARET { get; set; }
        public int ID_JOBPROFILE { get; set; }
        public string DES_RUOLO { get; set; }
        public System.DateTime DTA_INIZIO { get; set; }
        public string COD_RUOLOAGGREG { get; set; }
        public System.DateTime DTA_FINE { get; set; }
        public string NOT_NOTA { get; set; }
        public string IND_WEBVISIBLE { get; set; }
        public decimal IMP_MIDPOINT { get; set; }
        public decimal IMP_HIRINGCOST { get; set; }
        public decimal IMP_TRAINIGCOST { get; set; }
        public decimal PRC_WEIGHT { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
    
        public virtual ICollection<JOBASSIGN> JOBASSIGN { get; set; }
    }
}