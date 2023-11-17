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
    
    public partial class B2RaiPlace_Eventi_Evento
    {
        public B2RaiPlace_Eventi_Evento()
        {
            this.B2RaiPlace_Eventi_Anagrafica = new HashSet<B2RaiPlace_Eventi_Anagrafica>();
            this.B2RaiPlace_Eventi_Campi = new HashSet<B2RaiPlace_Eventi_Campi>();
            this.B2RaiPlace_Eventi_Prenotazione = new HashSet<B2RaiPlace_Eventi_Prenotazione>();
            this.B2RaiPlace_Eventi_Sede = new HashSet<B2RaiPlace_Eventi_Sede>();
            this.B2RaiPlace_Eventi_Utenti = new HashSet<B2RaiPlace_Eventi_Utenti>();
            this.B2RaiPlace_Eventi_Pdf = new HashSet<B2RaiPlace_Eventi_Pdf>();
            this.B2RaiPlace_Eventi_Utenti_Abilitati = new HashSet<B2RaiPlace_Eventi_Utenti_Abilitati>();
        }
    
        public int id { get; set; }
        public Nullable<int> id_programma { get; set; }
        public string titolo { get; set; }
        public Nullable<System.DateTime> data_inizio { get; set; }
        public Nullable<System.DateTime> data_fine { get; set; }
        public string luogo { get; set; }
        public Nullable<System.DateTime> data_inizio_prenotazione { get; set; }
        public Nullable<System.DateTime> data_fine_prenotazione { get; set; }
        public Nullable<int> numero_totale { get; set; }
        public Nullable<int> numero_massimo { get; set; }
        public Nullable<bool> matricole_abilitate { get; set; }
        public Nullable<bool> sedi_abilitate { get; set; }
        public Nullable<bool> ticket { get; set; }
        public Nullable<int> overbooking { get; set; }
        public Nullable<int> cerimoniale { get; set; }
        public string testo_mail { get; set; }
        public Nullable<bool> vedi_insediamento { get; set; }
        public Nullable<int> limite_eta { get; set; }
        public string sede_contabile { get; set; }
        public Nullable<bool> attiva_streaming { get; set; }
        public Nullable<bool> pulsante_aggiungi { get; set; }
        public Nullable<bool> pulsante_attivo { get; set; }
        public string pulsante_testo { get; set; }
        public string pulsante_link { get; set; }
        public string video_poster { get; set; }
        public string extra_param { get; set; }
        public string video_link { get; set; }
    
        public virtual ICollection<B2RaiPlace_Eventi_Anagrafica> B2RaiPlace_Eventi_Anagrafica { get; set; }
        public virtual ICollection<B2RaiPlace_Eventi_Campi> B2RaiPlace_Eventi_Campi { get; set; }
        public virtual B2RaiPlace_Eventi_Programma B2RaiPlace_Eventi_Programma { get; set; }
        public virtual ICollection<B2RaiPlace_Eventi_Prenotazione> B2RaiPlace_Eventi_Prenotazione { get; set; }
        public virtual ICollection<B2RaiPlace_Eventi_Sede> B2RaiPlace_Eventi_Sede { get; set; }
        public virtual ICollection<B2RaiPlace_Eventi_Utenti> B2RaiPlace_Eventi_Utenti { get; set; }
        public virtual ICollection<B2RaiPlace_Eventi_Pdf> B2RaiPlace_Eventi_Pdf { get; set; }
        public virtual ICollection<B2RaiPlace_Eventi_Utenti_Abilitati> B2RaiPlace_Eventi_Utenti_Abilitati { get; set; }
    }
}