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
    
    public partial class XR_DEM_TIPI_DOCUMENTO
    {
        public XR_DEM_TIPI_DOCUMENTO()
        {
            this.XR_RICHIESTE = new HashSet<XR_RICHIESTE>();
            this.XR_DEM_DOCUMENTI = new HashSet<XR_DEM_DOCUMENTI>();
        }
    
        public int Id { get; set; }
        public string Codice { get; set; }
        public string Descrizione { get; set; }
        public bool Consenti_Rifiuto { get; set; }
        public bool FirmaObbligatoria { get; set; }
        public bool ApprovazioneObbligatoria { get; set; }
    
        public virtual ICollection<XR_RICHIESTE> XR_RICHIESTE { get; set; }
        public virtual ICollection<XR_DEM_DOCUMENTI> XR_DEM_DOCUMENTI { get; set; }
    }
}