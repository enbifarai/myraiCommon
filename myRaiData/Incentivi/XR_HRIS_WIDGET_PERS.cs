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
    
    public partial class XR_HRIS_WIDGET_PERS
    {
        public int ID_WIDGET_PERS { get; set; }
        public int ID_WIDGET { get; set; }
        public int ID_PERSONA { get; set; }
        public int NMR_ORDINE { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
    
        public virtual XR_HRIS_WIDGET XR_HRIS_WIDGET { get; set; }
    }
}