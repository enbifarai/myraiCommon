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
    
    public partial class Documenti
    {
        public int Id_Documento { get; set; }
        public string Titolo_Documento { get; set; }
        public string Testo_Documento { get; set; }
        public string Link_Documento { get; set; }
        public string Target_Link_Documento { get; set; }
        public string Data_Inizio { get; set; }
        public string Data_Fine { get; set; }
        public string Flag_Attivo { get; set; }
        public string Utente { get; set; }
        public string Data_Inserimento { get; set; }
    }
}
