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
    
    public partial class MyRai_Visualizzazione_Giornate_Da_Segreteria
    {
        public int Id { get; set; }
        public Nullable<int> IdRichiesta { get; set; }
        public bool Visualizzato { get; set; }
        public string MatricolaVisualizzatore { get; set; }
        public string UtenteVisualizzatore { get; set; }
        public System.DateTime DataCreazione { get; set; }
        public System.DateTime DataUltimoAccesso { get; set; }
        public System.DateTime DataRichiesta { get; set; }
        public string Matricola { get; set; }
    }
}
