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
    
    public partial class XR_TSK_TASK
    {
        public int ID { get; set; }
        public string MATRICOLA_CREATORE { get; set; }
        public System.DateTime DATA_CREAZIONE { get; set; }
        public Nullable<System.DateTime> DATA_ESECUZIONE { get; set; }
        public Nullable<bool> IND_ESITO { get; set; }
        public string NOT_ERRORE { get; set; }
        public bool IND_RIESEGUI { get; set; }
        public Nullable<int> ID_ORIGINALE { get; set; }
        public string COD_TIPOLOGIA { get; set; }
        public string INPUT { get; set; }
        public string OUTPUT { get; set; }
        public Nullable<bool> IND_RUNNING { get; set; }
        public Nullable<System.DateTime> DATA_ESECUZIONE_FINE { get; set; }
        public string COD_SOTTOTIPOLOGIA { get; set; }
        public Nullable<System.DateTime> DATA_PROGRAMMATA { get; set; }
        public Nullable<int> SCHEDULE_PARENT { get; set; }
    
        public virtual XR_TSK_SCHEDULER XR_TSK_SCHEDULER { get; set; }
    }
}
