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
    
    public partial class TipoProcura
    {
        public TipoProcura()
        {
            this.Procure = new HashSet<Procure>();
        }
    
        public int IdTipoProcura { get; set; }
        public string Descrizione { get; set; }
        public Nullable<int> Ordine { get; set; }
        public string MatrVar { get; set; }
        public Nullable<System.DateTime> DataAggiornamento { get; set; }
    
        public virtual ICollection<Procure> Procure { get; set; }
    }
}