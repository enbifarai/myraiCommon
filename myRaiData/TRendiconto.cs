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
    
    public partial class TRendiconto
    {
        public decimal ID { get; set; }
        public System.DateTime Data_Creazione { get; set; }
        public string Stato { get; set; }
        public string Gruppo_procuratore { get; set; }
        public string Procuratore { get; set; }
        public System.DateTime Procuratore_DtConv { get; set; }
        public string Modalita_Pagamento { get; set; }
        public short Autorizzazioni_Stampate { get; set; }
        public System.DateTime Data_Stampa_Autorizzazione { get; set; }
        public System.DateTime Data_Scadenza_Versamento { get; set; }
        public System.DateTime Data_Consegna_Cartaceo { get; set; }
        public string Targhetta_Attiva { get; set; }
        public string Utente { get; set; }
        public System.DateTime DataAgg { get; set; }
        public string TipoAgg { get; set; }
    }
}