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
    
    public partial class XR_PRV_AREA
    {
        public XR_PRV_AREA()
        {
            this.XR_PRV_CAMPAGNA_BUDGET = new HashSet<XR_PRV_CAMPAGNA_BUDGET>();
            this.XR_PRV_DIREZIONE = new HashSet<XR_PRV_DIREZIONE>();
        }
    
        public int ID_AREA { get; set; }
        public string NOME { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
        public Nullable<int> ORDINE { get; set; }
        public string LV_ABIL { get; set; }
    
        public virtual ICollection<XR_PRV_CAMPAGNA_BUDGET> XR_PRV_CAMPAGNA_BUDGET { get; set; }
        public virtual ICollection<XR_PRV_DIREZIONE> XR_PRV_DIREZIONE { get; set; }
    }
}
