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
    
    public partial class TRestituzione_Totale
    {
        public decimal ID { get; set; }
        public short Progressivo { get; set; }
        public short Restituzioni_Stampate { get; set; }
        public System.DateTime Data_Stampa { get; set; }
        public System.DateTime Data_Scadenza_Incasso { get; set; }
        public string Utente_Restituzione { get; set; }
        public string Utente { get; set; }
        public System.DateTime DataAgg { get; set; }
        public string TipoAgg { get; set; }
    }
}
