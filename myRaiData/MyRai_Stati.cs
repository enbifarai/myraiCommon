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
    
    public partial class MyRai_Stati
    {
        public MyRai_Stati()
        {
            this.MyRai_Eccezioni_Richieste = new HashSet<MyRai_Eccezioni_Richieste>();
            this.MyRai_ApprovazioneMassiva = new HashSet<MyRai_ApprovazioneMassiva>();
        }
    
        public int id_stato { get; set; }
        public string descrizione_stato { get; set; }
    
        public virtual ICollection<MyRai_Eccezioni_Richieste> MyRai_Eccezioni_Richieste { get; set; }
        public virtual ICollection<MyRai_ApprovazioneMassiva> MyRai_ApprovazioneMassiva { get; set; }
    }
}