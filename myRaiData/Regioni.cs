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
    
    public partial class Regioni
    {
        public Regioni()
        {
            this.Province = new HashSet<Province>();
        }
    
        public int Id_Regione { get; set; }
        public string Nome_Regione { get; set; }
    
        public virtual ICollection<Province> Province { get; set; }
    }
}
