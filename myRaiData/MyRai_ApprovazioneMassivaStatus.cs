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
    
    public partial class MyRai_ApprovazioneMassivaStatus
    {
        public MyRai_ApprovazioneMassivaStatus()
        {
            this.MyRai_ApprovazioneMassiva = new HashSet<MyRai_ApprovazioneMassiva>();
        }
    
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descrizione { get; set; }
    
        public virtual ICollection<MyRai_ApprovazioneMassiva> MyRai_ApprovazioneMassiva { get; set; }
    }
}