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
    
    public partial class XR_DEM_AZIONI_AUTOMATICHE
    {
        public XR_DEM_AZIONI_AUTOMATICHE()
        {
            this.XR_DEM_AZIONI_AUTOMATICHE_TASK = new HashSet<XR_DEM_AZIONI_AUTOMATICHE_TASK>();
        }
    
        public int ID { get; set; }
        public int ID_DOC { get; set; }
        public int ID_STATO_INIZIO { get; set; }
        public int ID_STATO_FINE { get; set; }
        public string MATRICOLA { get; set; }
        public System.DateTime DATA_CREAZIONE { get; set; }
        public int ORDINE { get; set; }
        public bool ESITO { get; set; }
    
        public virtual ICollection<XR_DEM_AZIONI_AUTOMATICHE_TASK> XR_DEM_AZIONI_AUTOMATICHE_TASK { get; set; }
        public virtual XR_DEM_DOCUMENTI XR_DEM_DOCUMENTI { get; set; }
        public virtual XR_DEM_STATI XR_DEM_STATI { get; set; }
        public virtual XR_DEM_STATI XR_DEM_STATI1 { get; set; }
    }
}
