//------------------------------------------------------------------------------
// <auto-generated>
//    Codice generato da un modello.
//
//    Le modifiche manuali a questo file potrebbero causare un comportamento imprevisto dell'applicazione.
//    Se il codice viene rigenerato, le modifiche manuali al file verranno sovrascritte.
// </auto-generated>
//------------------------------------------------------------------------------

namespace myRai.Data.CurriculumVitae
{
    using System;
    using System.Collections.Generic;
    
    public partial class V_CVCorsiRai
    {
        public string matricola { get; set; }
        public string codice { get; set; }
        public Nullable<System.DateTime> DataInizioDate { get; set; }
        public string DataInizio { get; set; }
        public string DataFine { get; set; }
        public string TitoloCorso { get; set; }
        public Nullable<decimal> Durata { get; set; }
        public string Societa { get; set; }
        public int flagImage { get; set; }
    }
}