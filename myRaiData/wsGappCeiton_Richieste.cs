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
    
    public partial class wsGappCeiton_Richieste
    {
        public wsGappCeiton_Richieste()
        {
            this.wsGappCeiton_Operazioni = new HashSet<wsGappCeiton_Operazioni>();
        }
    
        public int ID { get; set; }
        public string WorkOrderID { get; set; }
        public string Matricola { get; set; }
        public Nullable<System.DateTime> Data_Riferimento { get; set; }
        public string Origine { get; set; }
        public string Destinazione { get; set; }
        public Nullable<bool> Esito { get; set; }
        public string Desc_Messaggio_Errore_Flusso { get; set; }
        public Nullable<int> Numero_Operazioni { get; set; }
        public Nullable<System.DateTime> Data_Operazione { get; set; }
        public string Messaggio_Soap { get; set; }
    
        public virtual ICollection<wsGappCeiton_Operazioni> wsGappCeiton_Operazioni { get; set; }
    }
}