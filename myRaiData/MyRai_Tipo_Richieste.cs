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
    
    public partial class MyRai_Tipo_Richieste
    {
        public MyRai_Tipo_Richieste()
        {
            this.MyRai_Richieste = new HashSet<MyRai_Richieste>();
        }
    
        public int id_tipo_richiesta { get; set; }
        public string descrizione_tipo_richiesta { get; set; }
    
        public virtual ICollection<MyRai_Richieste> MyRai_Richieste { get; set; }
    }
}
