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
    
    public partial class MyRai_FormTipologieDomande
    {
        public MyRai_FormTipologieDomande()
        {
            this.MyRai_FormDomande = new HashSet<MyRai_FormDomande>();
        }
    
        public int id { get; set; }
        public string tipologia { get; set; }
        public Nullable<bool> scelta_risposte { get; set; }
    
        public virtual ICollection<MyRai_FormDomande> MyRai_FormDomande { get; set; }
    }
}
