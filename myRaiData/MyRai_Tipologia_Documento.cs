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
    
    public partial class MyRai_Tipologia_Documento
    {
        public MyRai_Tipologia_Documento()
        {
            this.MyRai_DocumentiDipendente = new HashSet<MyRai_DocumentiDipendente>();
        }
    
        public int id { get; set; }
        public string TipologiaDocumento { get; set; }
        public bool RiAssociabile { get; set; }
    
        public virtual ICollection<MyRai_DocumentiDipendente> MyRai_DocumentiDipendente { get; set; }
    }
}
