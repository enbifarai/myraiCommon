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
    
    public partial class XR_PRV_STATI
    {
        public XR_PRV_STATI()
        {
            this.XR_PRV_OPERSTATI = new HashSet<XR_PRV_OPERSTATI>();
        }
    
        public int ID_STATO { get; set; }
        public string DESCRIZIONE { get; set; }
        public string NOME_BOX { get; set; }
        public string DES_FILTRO { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
    
        public virtual ICollection<XR_PRV_OPERSTATI> XR_PRV_OPERSTATI { get; set; }
    }
}
