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
    
    public partial class MyRai_TipologieFiles
    {
        public MyRai_TipologieFiles()
        {
            this.MyRai_Files = new HashSet<MyRai_Files>();
            this.MyRai_TipologieFiles1 = new HashSet<MyRai_TipologieFiles>();
        }
    
        public string Codice { get; set; }
        public string Descrizione { get; set; }
        public string CodicePadre { get; set; }
    
        public virtual ICollection<MyRai_Files> MyRai_Files { get; set; }
        public virtual ICollection<MyRai_TipologieFiles> MyRai_TipologieFiles1 { get; set; }
        public virtual MyRai_TipologieFiles MyRai_TipologieFiles2 { get; set; }
    }
}
