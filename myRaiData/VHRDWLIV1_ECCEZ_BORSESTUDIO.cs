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
    
    public partial class VHRDWLIV1_ECCEZ_BORSESTUDIO
    {
        public string Id { get; set; }
        public string matricola { get; set; }
        public string cod_eccez { get; set; }
        public Nullable<System.DateTime> data_documento { get; set; }
        public Nullable<decimal> quantita_numero { get; set; }
        public Nullable<int> quantita_ore { get; set; }
        public Nullable<decimal> importo { get; set; }
        public Nullable<int> inizio_eccez { get; set; }
        public Nullable<int> fine_eccez { get; set; }
        public string flag_dato_autom { get; set; }
        public string cod_uorg { get; set; }
        public string cod_df { get; set; }
        public string cod_matricola_produzione { get; set; }
        public string mese_contabile { get; set; }
        public string stato_eccezione { get; set; }
        public Nullable<System.DateTime> data_immissione { get; set; }
        public Nullable<System.DateTime> data_riepilogo { get; set; }
        public string codice_orario { get; set; }
        public string utente { get; set; }
        public Nullable<decimal> IMPORTO_MEDIO { get; set; }
        public string COD_MEDIA { get; set; }
    }
}
