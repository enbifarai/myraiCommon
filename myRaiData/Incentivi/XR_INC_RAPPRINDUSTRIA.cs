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
    
    public partial class XR_INC_RAPPRINDUSTRIA
    {
        public XR_INC_RAPPRINDUSTRIA()
        {
            this.XR_INC_DIPENDENTI = new HashSet<XR_INC_DIPENDENTI>();
        }
    
        public int ID_RAPPRINDUSTRIA { get; set; }
        public string COGNOME { get; set; }
        public string NOME { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
        public string SEDE { get; set; }
        public string ORGANIZZAZIONE { get; set; }
        public string TITOLO { get; set; }
        public string GENERE { get; set; }
        public Nullable<System.DateTime> DATA_FINE_VALIDITA { get; set; }
    
        public virtual ICollection<XR_INC_DIPENDENTI> XR_INC_DIPENDENTI { get; set; }
    }
}
