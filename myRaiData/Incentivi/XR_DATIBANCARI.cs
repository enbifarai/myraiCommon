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
    
    public partial class XR_DATIBANCARI
    {
        public XR_DATIBANCARI()
        {
            this.XR_UTILCONTO = new HashSet<XR_UTILCONTO>();
        }
    
        public int ID_XR_DATIBANCARI { get; set; }
        public int ID_PERSONA { get; set; }
        public System.DateTime DTA_INIZIO { get; set; }
        public string COD_IBAN { get; set; }
        public string COD_ABI { get; set; }
        public string COD_CAB { get; set; }
        public Nullable<System.DateTime> DTA_FINE { get; set; }
        public string COD_TIPOCONTO { get; set; }
        public string COD_SUBTPCONTO { get; set; }
        public string COD_STATO { get; set; }
        public string IND_CONGELATO { get; set; }
        public string IND_VINCOLATO { get; set; }
        public string IND_CHANGED { get; set; }
        public string IND_DELETE { get; set; }
        public string DES_INTESTATARIO { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public Nullable<System.DateTime> TMS_TIMESTAMP { get; set; }
    
        public virtual ICollection<XR_UTILCONTO> XR_UTILCONTO { get; set; }
        public virtual ANAGPERS ANAGPERS { get; set; }
    }
}
