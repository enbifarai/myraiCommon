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
    
    public partial class MyRai_Regole_TipiDocumentazione
    {
        public MyRai_Regole_TipiDocumentazione()
        {
            this.MyRai_Regole_SchedeEccezioni = new HashSet<MyRai_Regole_SchedeEccezioni>();
        }
    
        public int id { get; set; }
        public string TipoDocumentazione { get; set; }
    
        public virtual ICollection<MyRai_Regole_SchedeEccezioni> MyRai_Regole_SchedeEccezioni { get; set; }
    }
}