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
    
    public partial class XR_VAL_OPER_STATE
    {
        public int ID_OPER { get; set; }
        public int ID_STATE { get; set; }
        public int ID_EVALUATION { get; set; }
        public int ID_PERSONA { get; set; }
        public System.DateTime VALID_DTA_INI { get; set; }
        public Nullable<System.DateTime> VALID_DTA_END { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
    
        public virtual SINTESI1 SINTESI1 { get; set; }
        public virtual XR_VAL_EVALUATION XR_VAL_EVALUATION { get; set; }
        public virtual XR_VAL_STATE XR_VAL_STATE { get; set; }
    }
}
