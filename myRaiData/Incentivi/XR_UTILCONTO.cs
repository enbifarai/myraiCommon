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
    
    public partial class XR_UTILCONTO
    {
        public int ID_XR_UTILCONTO { get; set; }
        public int ID_XR_DATIBANCARI { get; set; }
        public string COD_UTILCONTO { get; set; }
        public string IND_CHANGED { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public Nullable<System.DateTime> TMS_TIMESTAMP { get; set; }
    
        public virtual XR_DATIBANCARI XR_DATIBANCARI { get; set; }
    }
}
