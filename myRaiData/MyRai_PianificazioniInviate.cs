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
    
    public partial class MyRai_PianificazioniInviate
    {
        public int id { get; set; }
        public string Matricola { get; set; }
        public int Anno { get; set; }
        public int Mese { get; set; }
        public System.DateTime DataInvio { get; set; }
        public string SedeGapp { get; set; }
        public int MaxPianificabili { get; set; }
        public string Tipologia { get; set; }
        public string MatricolaApprovatore { get; set; }
        public Nullable<System.DateTime> DataApprovazione { get; set; }
        public Nullable<System.DateTime> DataRifiuto { get; set; }
        public string Note { get; set; }
        public string Varie { get; set; }
    }
}
