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
    
    public partial class XR_HRIS_ABIL_ASSOC_MODELLO
    {
        public int ID_ABIL_MODELLO { get; set; }
        public int ID_ABIL { get; set; }
        public int ID_MODELLO { get; set; }
    
        public virtual XR_HRIS_ABIL XR_HRIS_ABIL { get; set; }
        public virtual XR_HRIS_ABIL_MODELLO XR_HRIS_ABIL_MODELLO { get; set; }
    }
}