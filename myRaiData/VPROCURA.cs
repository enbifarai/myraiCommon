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
    
    public partial class VPROCURA
    {
        public string MATRICOLA { get; set; }
        public string TIPO { get; set; }
        public System.DateTime DATA_ATTIVAZIONE { get; set; }
        public Nullable<System.DateTime> DATA_REVOCA { get; set; }
        public string ID_DOC { get; set; }
        public string ID_DOC_REVOCA { get; set; }
        public Nullable<decimal> IMPORTO { get; set; }
    }
}
