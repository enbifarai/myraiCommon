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
    
    public partial class TSVIntervScheda
    {
        public int Id_IntervScheda { get; set; }
        public string Matricola { get; set; }
        public string Note { get; set; }
        public string InizFormaz { get; set; }
        public string Feedback { get; set; }
        public Nullable<System.DateTime> DataInterv { get; set; }
        public string MatricolaComm { get; set; }
        public string Utente { get; set; }
        public string TipoAgg { get; set; }
        public Nullable<System.DateTime> DataOraAgg { get; set; }
        public Nullable<bool> FlagJobRotation { get; set; }
        public string JobRotation { get; set; }
        public Nullable<bool> FlagDispoTrasferte { get; set; }
        public string DispoTrasferte { get; set; }
        public Nullable<bool> FlagDispoTrasferimenti { get; set; }
        public string DispoTrasferimenti { get; set; }
        public string OrientaInnova { get; set; }
    }
}