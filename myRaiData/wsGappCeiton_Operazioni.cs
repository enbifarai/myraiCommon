//------------------------------------------------------------------------------
// <auto-generated>
//    Codice generato da un modello.
//
//    Le modifiche manuali a questo file potrebbero causare un comportamento imprevisto dell'applicazione.
//    Se il codice viene rigenerato, le modifiche manuali al file verranno sovrascritte.
// </auto-generated>
//------------------------------------------------------------------------------

namespace myRaiData
{
    using System;
    using System.Collections.Generic;
    
    public partial class wsGappCeiton_Operazioni
    {
        public string WorkOrderID { get; set; }
        public int Progressivo { get; set; }
        public Nullable<System.DateTime> Data_Riferimento { get; set; }
        public string Turno_Attuale_Gapp { get; set; }
        public string Turno_Pianificato { get; set; }
        public string Cod_Eccezione { get; set; }
        public string Tipo_Operazione { get; set; }
        public bool Esito { get; set; }
        public string Desc_Messaggio_Errore { get; set; }
        public Nullable<System.DateTime> Data_Operazione { get; set; }
    
        public virtual wsGappCeiton_Richieste wsGappCeiton_Richieste { get; set; }
    }
}
