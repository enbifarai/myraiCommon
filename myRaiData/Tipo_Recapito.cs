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
    
    public partial class Tipo_Recapito
    {
        public Tipo_Recapito()
        {
            this.Recapiti = new HashSet<Recapiti>();
        }
    
        public int Id_Tipo_Recapito { get; set; }
        public string Descrizione_Recapito { get; set; }
    
        public virtual ICollection<Recapiti> Recapiti { get; set; }
    }
}
