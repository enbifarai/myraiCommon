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
    
    public partial class XR_WKF_STATI
    {
        public XR_WKF_STATI()
        {
            this.XR_WKF_OPERSTATI_GENERIC = new HashSet<XR_WKF_OPERSTATI_GENERIC>();
            this.XR_WKF_WORKFLOW = new HashSet<XR_WKF_WORKFLOW>();
        }
    
        public int ID_WKF_TIPOLOGIA { get; set; }
        public int ID_STATO { get; set; }
        public string COD_NOME { get; set; }
        public string DESCRIZIONE { get; set; }
        public bool IND_SYSTEM { get; set; }
        public System.DateTime VALID_DTA_INI { get; set; }
        public Nullable<System.DateTime> VALID_DTA_END { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
    
        public virtual ICollection<XR_WKF_OPERSTATI_GENERIC> XR_WKF_OPERSTATI_GENERIC { get; set; }
        public virtual XR_WKF_STATI XR_WKF_STATI1 { get; set; }
        public virtual XR_WKF_STATI XR_WKF_STATI2 { get; set; }
        public virtual ICollection<XR_WKF_WORKFLOW> XR_WKF_WORKFLOW { get; set; }
    }
}
