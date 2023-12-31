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
    
    public partial class XR_RICHIESTE
    {
        public XR_RICHIESTE()
        {
            this.XR_DEM_DOCUMENTI = new HashSet<XR_DEM_DOCUMENTI>();
        }
    
        public int Id { get; set; }
        public int Id_Tipologia { get; set; }
        public int Id_Stato { get; set; }
        public System.DateTime DataCreazione { get; set; }
        public string MatricolaRichiesta { get; set; }
        public int IdPersonaRichiesta { get; set; }
        public string MatricolaOperatore { get; set; }
        public Nullable<int> IdPersonaOperatore { get; set; }
        public byte[] Timestamp { get; set; }
        public string Note { get; set; }
        public int Id_WKF_Tipologia { get; set; }
        public string Modello { get; set; }
    
        public virtual XR_DEM_STATI XR_DEM_STATI { get; set; }
        public virtual XR_DEM_TIPI_DOCUMENTO XR_DEM_TIPI_DOCUMENTO { get; set; }
        public virtual XR_WKF_TIPOLOGIA XR_WKF_TIPOLOGIA { get; set; }
        public virtual ICollection<XR_DEM_DOCUMENTI> XR_DEM_DOCUMENTI { get; set; }
    }
}
