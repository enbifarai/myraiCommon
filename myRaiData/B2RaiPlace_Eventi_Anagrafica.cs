//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace myRaiData
{
    using System;
    using System.Collections.Generic;
    
    public partial class B2RaiPlace_Eventi_Anagrafica
    {
        public int id { get; set; }
        public int id_evento { get; set; }
        public string matricola { get; set; }
        public string nome { get; set; }
        public string cognome { get; set; }
        public Nullable<System.DateTime> data_nascita { get; set; }
        public string genere { get; set; }
        public string email { get; set; }
        public string telefono { get; set; }
        public string tipo_documento { get; set; }
        public string codice_documento { get; set; }
        public string grado_parentela { get; set; }
        public string sede { get; set; }
        public string note { get; set; }
        public Nullable<bool> overbooking { get; set; }
        public Nullable<bool> cerimoniale { get; set; }
        public System.DateTime data_prenotazione { get; set; }
        public bool confermata { get; set; }
        public string sede_insediamento { get; set; }
        public string citta_nascita { get; set; }
        public string Dipendente { get; set; }
        public string note_evento { get; set; }
        public Nullable<System.DateTime> data_inizio_fruizione { get; set; }
        public Nullable<System.TimeSpan> tempo_fruizione { get; set; }
    
        public virtual B2RaiPlace_Eventi_Evento B2RaiPlace_Eventi_Evento { get; set; }
    }
}