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
    
    public partial class XR_INC_RAPPRSINDACATO
    {
        public XR_INC_RAPPRSINDACATO()
        {
            this.XR_INC_DIPENDENTI = new HashSet<XR_INC_DIPENDENTI>();
        }
    
        public int ID_RAPPRSINDACATO { get; set; }
        public int ID_SIGLA { get; set; }
        public string COGNOME { get; set; }
        public string NOME { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
        public string TITOLO { get; set; }
        public string GENERE { get; set; }
        public Nullable<System.DateTime> DATA_FINE_VALIDITA { get; set; }
        public string SEDE { get; set; }
    
        public virtual ICollection<XR_INC_DIPENDENTI> XR_INC_DIPENDENTI { get; set; }
        public virtual XR_INC_SIGLESINDACALI XR_INC_SIGLESINDACALI { get; set; }
    }
}
