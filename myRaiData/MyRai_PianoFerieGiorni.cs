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
    
    public partial class MyRai_PianoFerieGiorni
    {
        public int id { get; set; }
        public string matricola { get; set; }
        public System.DateTime data { get; set; }
        public System.DateTime data_inserimento { get; set; }
        public string nota_responsabile { get; set; }
        public string nota_matricola { get; set; }
        public Nullable<System.DateTime> nota_data { get; set; }
        public string eccezione { get; set; }
        public string provenienza { get; set; }
        public Nullable<System.DateTime> data_swap_turno { get; set; }
    }
}