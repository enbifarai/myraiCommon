//------------------------------------------------------------------------------
// <auto-generated>
//    Codice generato da un modello.
//
//    Le modifiche manuali a questo file potrebbero causare un comportamento imprevisto dell'applicazione.
//    Se il codice viene rigenerato, le modifiche manuali al file verranno sovrascritte.
// </auto-generated>
//------------------------------------------------------------------------------

namespace myRaiDataTalentia
{
    using System;
    using System.Collections.Generic;
    
    public partial class DOCE_EDIZ
    {
        public int ID_DOCE_EDIZ { get; set; }
        public int ID_EDIZIONE { get; set; }
        public int ID_PERSONA { get; set; }
        public string IND_TIPOPARTEC { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
        public Nullable<decimal> QTA_HOURSDURATION { get; set; }
    
        public virtual SINTESI1 SINTESI1 { get; set; }
    }
}