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
    
    public partial class XR_MAT_EVIDENZE_9000
    {
        public int ID { get; set; }
        public string TRACCIATO { get; set; }
        public System.DateTime DATA_IMPORT { get; set; }
        public int ANNO_FILE { get; set; }
        public int MESE_FILE { get; set; }
        public string MATRICOLA { get; set; }
        public System.DateTime DATA_INIZIO_PERIODO { get; set; }
        public System.DateTime DATA_FINE_PERIODO { get; set; }
        public string ECCEZIONE { get; set; }
        public Nullable<int> ID_RICHIESTA { get; set; }
        public Nullable<int> ID_TRACCIATO_RICHIAMATO { get; set; }
        public bool ATTIVA { get; set; }
    }
}
