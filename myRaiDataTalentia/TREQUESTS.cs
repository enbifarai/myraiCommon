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
    
    public partial class TREQUESTS
    {
        public TREQUESTS()
        {
            this.TREQUESTS_STEP = new HashSet<TREQUESTS_STEP>();
        }
    
        public int ID_TREQUESTS { get; set; }
        public Nullable<int> ID_TRREQREASON { get; set; }
        public Nullable<int> ID_WFSTAT { get; set; }
        public int ID_TPRIORITY { get; set; }
        public int ID_TPERIODS { get; set; }
        public int ID_TREASONS { get; set; }
        public Nullable<int> ID_UNITAORG { get; set; }
        public Nullable<int> ID_PERSONA { get; set; }
        public Nullable<int> ID_CORSOPLAN { get; set; }
        public Nullable<int> ID_CORSO { get; set; }
        public string DES_NEED { get; set; }
        public System.DateTime DTA_DATE { get; set; }
        public Nullable<int> QTA_JOKERS { get; set; }
        public string NOT_NOTE { get; set; }
        public string IND_SELFREQUEST { get; set; }
        public string IND_HRPROF { get; set; }
        public string IND_DONE { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
        public int ID_REQUESTNEED { get; set; }
        public string COD_SOURCE { get; set; }
        public string IND_MANAPPROVED { get; set; }
        public string NOT_MANAPPROVED { get; set; }
        public string IND_SYSTEM { get; set; }
        public string NME_REQUEST { get; set; }
        public string COD_SYSTEM_REASON { get; set; }
        public string COD_IMPRESA { get; set; }
        public string COD_SEDE { get; set; }
        public Nullable<int> ID_TRAININGTYPE { get; set; }
        public Nullable<decimal> NUM_AVGFTERATIOJOKERS { get; set; }
        public Nullable<int> ID_TRASTRATEGY { get; set; }
    
        public virtual ICollection<TREQUESTS_STEP> TREQUESTS_STEP { get; set; }
        public virtual SINTESI1 SINTESI1 { get; set; }
        public virtual CORSO CORSO { get; set; }
    }
}
