//------------------------------------------------------------------------------
// <auto-generated>
//    Codice generato da un modello.
//
//    Le modifiche manuali a questo file potrebbero causare un comportamento imprevisto dell'applicazione.
//    Se il codice viene rigenerato, le modifiche manuali al file verranno sovrascritte.
// </auto-generated>
//------------------------------------------------------------------------------

namespace myRaiData.CurriculumVitae
{
    using System;
    using System.Collections.Generic;
    
    public partial class TSVPrenSlot
    {
        public int Id_Slot { get; set; }
        public string CodSedeCont { get; set; }
        public int Id_Stanza { get; set; }
        public Nullable<System.DateTime> DataDispo { get; set; }
        public Nullable<System.DateTime> OrarioInizioDispo { get; set; }
        public Nullable<System.DateTime> OrarioFineDispo { get; set; }
        public Nullable<int> NumPostiDispo { get; set; }
        public string CodGruppoValutati { get; set; }
        public string Utente { get; set; }
        public string TipoAgg { get; set; }
        public Nullable<System.DateTime> DataOraAgg { get; set; }
    }
}
