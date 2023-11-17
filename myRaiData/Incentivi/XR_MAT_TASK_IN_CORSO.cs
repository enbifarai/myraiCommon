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
    
    public partial class XR_MAT_TASK_IN_CORSO
    {
        public int ID { get; set; }
        public int ID_RICHIESTA { get; set; }
        public int ID_TASK { get; set; }
        public string MATRICOLA_OPERATORE { get; set; }
        public System.DateTime DATA_CREAZIONE { get; set; }
        public int PROGRESSIVO { get; set; }
        public System.DateTime ESEGUIBILE_DA_DATA { get; set; }
        public System.DateTime ESEGUIBILE_FINO_A_DATA { get; set; }
        public Nullable<System.DateTime> DATA_ULTIMO_TENTATIVO { get; set; }
        public string ERRORE_BATCH { get; set; }
        public string INPUT { get; set; }
        public string OUTPUT { get; set; }
        public string BLOCCATA_DA_OPERATORE { get; set; }
        public Nullable<System.DateTime> BLOCCATA_DATETIME { get; set; }
        public bool TERMINATA { get; set; }
        public string NOTE { get; set; }
        public int MESE { get; set; }
        public int ANNO { get; set; }
        public string SISTEMA_OUTPUT { get; set; }
        public Nullable<int> ID_TABELLA_OUTPUT { get; set; }
        public Nullable<System.DateTime> DATA_ORA_CESTINATO { get; set; }
        public bool ERRORE_RESETTABILE { get; set; }
        public string MESE_ANNO_PAGATO { get; set; }
        public string MESE_ANNO_PAGATO_JSON { get; set; }
        public Nullable<int> RIMBORSO_TRACCIATO { get; set; }
        public bool TRACCIATO_DA_DEM { get; set; }
    
        public virtual XR_MAT_ELENCO_TASK XR_MAT_ELENCO_TASK { get; set; }
        public virtual XR_MAT_RICHIESTE XR_MAT_RICHIESTE { get; set; }
    }
}
