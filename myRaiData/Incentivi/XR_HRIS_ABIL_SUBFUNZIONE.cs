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
    
    public partial class XR_HRIS_ABIL_SUBFUNZIONE
    {
        public XR_HRIS_ABIL_SUBFUNZIONE()
        {
            this.XR_HRIS_ABIL = new HashSet<XR_HRIS_ABIL>();
            this.XR_HRIS_ABIL_PROFILO_ASSOCIAZIONI = new HashSet<XR_HRIS_ABIL_PROFILO_ASSOCIAZIONI>();
        }
    
        public int ID_SUBFUNZ { get; set; }
        public int ID_FUNZIONE { get; set; }
        public string COD_SUBFUNZIONE { get; set; }
        public string DES_SUBFUNZIONE { get; set; }
        public bool IND_ATTIVO { get; set; }
        public bool IND_CREATE { get; set; }
        public bool IND_READ { get; set; }
        public bool IND_UPDATE { get; set; }
        public bool IND_DELETE { get; set; }
        public string NOT_UFFICIO { get; set; }
        public bool IND_NOFILTERS { get; set; }
    
        public virtual ICollection<XR_HRIS_ABIL> XR_HRIS_ABIL { get; set; }
        public virtual XR_HRIS_ABIL_FUNZIONE XR_HRIS_ABIL_FUNZIONE { get; set; }
        public virtual ICollection<XR_HRIS_ABIL_PROFILO_ASSOCIAZIONI> XR_HRIS_ABIL_PROFILO_ASSOCIAZIONI { get; set; }
    }
}
