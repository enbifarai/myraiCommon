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
    
    public partial class XR_MAT_SEGNALAZIONI_COMUNICAZIONI
    {
        public XR_MAT_SEGNALAZIONI_COMUNICAZIONI()
        {
            this.XR_MAT_ALLEGATI = new HashSet<XR_MAT_ALLEGATI>();
        }
    
        public int ID { get; set; }
        public int ID_SEGNALAZIONE { get; set; }
        public string NOTA { get; set; }
        public string MATRICOLA_FROM { get; set; }
        public string MATRICOLA_TO { get; set; }
        public System.DateTime TIMESTAMP { get; set; }
    
        public virtual XR_MAT_SEGNALAZIONI XR_MAT_SEGNALAZIONI { get; set; }
        public virtual ICollection<XR_MAT_ALLEGATI> XR_MAT_ALLEGATI { get; set; }
    }
}
