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
    
    public partial class B2RaiPlace_Eventi_Prenotazione
    {
        public int id { get; set; }
        public int matricola { get; set; }
        public int posti { get; set; }
        public int idProgramma { get; set; }
        public int idEvento { get; set; }
    
        public virtual B2RaiPlace_Eventi_Programma B2RaiPlace_Eventi_Programma { get; set; }
        public virtual B2RaiPlace_Eventi_Evento B2RaiPlace_Eventi_Evento { get; set; }
    }
}