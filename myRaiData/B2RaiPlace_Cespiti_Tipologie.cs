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
    
    public partial class B2RaiPlace_Cespiti_Tipologie
    {
        public B2RaiPlace_Cespiti_Tipologie()
        {
            this.B2RaiPlace_Cespiti_MailFornitori = new HashSet<B2RaiPlace_Cespiti_MailFornitori>();
        }
    
        public int Id_cespite { get; set; }
        public string Codice { get; set; }
        public string Descrizione { get; set; }
        public string Citta { get; set; }
        public string CodiceGAPP { get; set; }
    
        public virtual ICollection<B2RaiPlace_Cespiti_MailFornitori> B2RaiPlace_Cespiti_MailFornitori { get; set; }
    }
}
