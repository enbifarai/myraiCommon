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
    
    public partial class UNITAORG
    {
        public UNITAORG()
        {
            this.INCARLAV = new HashSet<INCARLAV>();
        }
    
        public int ID_UNITAORG { get; set; }
        public string COD_UNITAORG { get; set; }
        public string DES_DENOMUNITAORG { get; set; }
        public string NOT_UNITAORG { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
    
        public virtual ICollection<INCARLAV> INCARLAV { get; set; }
    }
}
