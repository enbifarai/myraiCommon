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
    
    public partial class B2RaiPlace_RegistroOmaggi_Motivo
    {
        public B2RaiPlace_RegistroOmaggi_Motivo()
        {
            this.B2RaiPlace_RegistroOmaggi_Omaggio = new HashSet<B2RaiPlace_RegistroOmaggi_Omaggio>();
        }
    
        public int Id_Motivo_Omaggio { get; set; }
        public string Descrizione { get; set; }
    
        public virtual ICollection<B2RaiPlace_RegistroOmaggi_Omaggio> B2RaiPlace_RegistroOmaggi_Omaggio { get; set; }
    }
}
