//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace myRaiData.Incentivi
{
    using System;
    using System.Collections.Generic;
    
    public partial class XR_MAT_MACROCATEGORIE
    {
        public XR_MAT_MACROCATEGORIE()
        {
            this.XR_MAT_CATEGORIE = new HashSet<XR_MAT_CATEGORIE>();
        }
    
        public int ID { get; set; }
        public string NOME { get; set; }
        public string SESSO { get; set; }
        public bool ATTIVA { get; set; }
        public int ORDINE { get; set; }
    
        public virtual ICollection<XR_MAT_CATEGORIE> XR_MAT_CATEGORIE { get; set; }
    }
}
