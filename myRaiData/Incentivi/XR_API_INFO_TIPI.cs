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
    
    public partial class XR_API_INFO_TIPI
    {
        public XR_API_INFO_TIPI()
        {
            this.XR_API_INFO_DA_INVIARE = new HashSet<XR_API_INFO_DA_INVIARE>();
        }
    
        public int ID { get; set; }
        public string CODICE { get; set; }
        public string DESCRIZIONE { get; set; }
    
        public virtual ICollection<XR_API_INFO_DA_INVIARE> XR_API_INFO_DA_INVIARE { get; set; }
    }
}
