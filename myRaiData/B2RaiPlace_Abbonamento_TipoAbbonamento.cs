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
    
    public partial class B2RaiPlace_Abbonamento_TipoAbbonamento
    {
        public B2RaiPlace_Abbonamento_TipoAbbonamento()
        {
            this.B2RaiPlace_Abbonamento_Richieste = new HashSet<B2RaiPlace_Abbonamento_Richieste>();
        }
    
        public int Id_Tipo_Abbonamento { get; set; }
        public string TipoAbbonamento { get; set; }
    
        public virtual ICollection<B2RaiPlace_Abbonamento_Richieste> B2RaiPlace_Abbonamento_Richieste { get; set; }
    }
}
