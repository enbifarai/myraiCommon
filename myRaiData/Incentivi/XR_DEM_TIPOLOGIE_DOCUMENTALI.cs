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
    
    public partial class XR_DEM_TIPOLOGIE_DOCUMENTALI
    {
        public XR_DEM_TIPOLOGIE_DOCUMENTALI()
        {
            this.XR_DEM_DOCUMENTI = new HashSet<XR_DEM_DOCUMENTI>();
            this.XR_DEM_TIPIDOC_COMPORTAMENTO = new HashSet<XR_DEM_TIPIDOC_COMPORTAMENTO>();
        }
    
        public string Codice { get; set; }
        public string Descrizione { get; set; }
        public bool Attivo { get; set; }
        public string MatricoleAbilitate { get; set; }
        public string MatricoleDisabilitate { get; set; }
    
        public virtual ICollection<XR_DEM_DOCUMENTI> XR_DEM_DOCUMENTI { get; set; }
        public virtual ICollection<XR_DEM_TIPIDOC_COMPORTAMENTO> XR_DEM_TIPIDOC_COMPORTAMENTO { get; set; }
    }
}
