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
    
    public partial class XR_PRV_CAMPAGNA_DECORRENZA
    {
        public int ID_DECORRENZA { get; set; }
        public int ID_CAMPAGNA { get; set; }
        public System.DateTime DT_DECORRENZA { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
    
        public virtual XR_PRV_CAMPAGNA XR_PRV_CAMPAGNA { get; set; }
    }
}
