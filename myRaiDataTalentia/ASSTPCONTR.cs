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
    
    public partial class ASSTPCONTR
    {
        public int ID_ASSTPCONTR { get; set; }
        public System.DateTime DTA_INIZIO { get; set; }
        public int ID_PERSONA { get; set; }
        public string COD_TPCNTR { get; set; }
        public string COD_EVCNTR { get; set; }
        public System.DateTime DTA_FINE { get; set; }
        public string NOT_NOTA { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
    
        public virtual ANAGPERS ANAGPERS { get; set; }
    }
}
