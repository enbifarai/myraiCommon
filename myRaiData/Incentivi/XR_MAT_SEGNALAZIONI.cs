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
    
    public partial class XR_MAT_SEGNALAZIONI
    {
        public XR_MAT_SEGNALAZIONI()
        {
            this.XR_MAT_SEGNALAZIONI_COMUNICAZIONI = new HashSet<XR_MAT_SEGNALAZIONI_COMUNICAZIONI>();
        }
    
        public int ID { get; set; }
        public int ID_RICHIESTA { get; set; }
        public string APERTA_DA { get; set; }
        public bool RISOLTA { get; set; }
        public System.DateTime DATA_APERTURA { get; set; }
    
        public virtual ICollection<XR_MAT_SEGNALAZIONI_COMUNICAZIONI> XR_MAT_SEGNALAZIONI_COMUNICAZIONI { get; set; }
        public virtual XR_MAT_RICHIESTE XR_MAT_RICHIESTE { get; set; }
    }
}