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
    
    public partial class MyRai_Regole_Tematiche
    {
        public MyRai_Regole_Tematiche()
        {
            this.MyRai_Regole_SchedeEccezioni_Tematiche = new HashSet<MyRai_Regole_SchedeEccezioni_Tematiche>();
        }
    
        public int id { get; set; }
        public string tematica { get; set; }
    
        public virtual ICollection<MyRai_Regole_SchedeEccezioni_Tematiche> MyRai_Regole_SchedeEccezioni_Tematiche { get; set; }
    }
}