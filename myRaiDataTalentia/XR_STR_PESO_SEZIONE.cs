//------------------------------------------------------------------------------
// <auto-generated>
//    Codice generato da un modello.
//
//    Le modifiche manuali a questo file potrebbero causare un comportamento imprevisto dell'applicazione.
//    Se il codice viene rigenerato, le modifiche manuali al file verranno sovrascritte.
// </auto-generated>
//------------------------------------------------------------------------------

namespace myRaiDataTalentia
{
    using System;
    using System.Collections.Generic;
    
    public partial class XR_STR_PESO_SEZIONE
    {
        public int id_peso { get; set; }
        public int id_sezione { get; set; }
        public string data_inizio_validita { get; set; }
        public string data_fine_validita { get; set; }
        public Nullable<int> punteggio { get; set; }
        public Nullable<int> grade { get; set; }
        public string cod_user { get; set; }
        public string cod_termid { get; set; }
        public Nullable<System.DateTime> tms_timestamp { get; set; }
    }
}
