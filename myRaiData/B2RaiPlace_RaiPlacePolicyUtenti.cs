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
    
    public partial class B2RaiPlace_RaiPlacePolicyUtenti
    {
        public int Id_RaiPlacePolicyUtenti { get; set; }
        public string Matrciola { get; set; }
        public System.DateTime DataConferma { get; set; }
        public Nullable<int> Fk_Id_RaiPlacePolicy { get; set; }
        public string nome { get; set; }
        public string cognome { get; set; }
        public string societa { get; set; }
        public string ip { get; set; }
        public string user_agent { get; set; }
        public Nullable<bool> Flag_Scaricato { get; set; }
    
        public virtual B2RaiPlace_RaiPlacePolicy B2RaiPlace_RaiPlacePolicy { get; set; }
    }
}
