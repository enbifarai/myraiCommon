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
    
    public partial class XR_PRV_TEMPLATE
    {
        public int ID_TEMPLATE { get; set; }
        public string NOME { get; set; }
        public Nullable<int> ID_PROV { get; set; }
        public string CAT_INCLUSE { get; set; }
        public string CAT_ESCLUSE { get; set; }
        public bool IND_BODY { get; set; }
        public bool IND_HEADER { get; set; }
        public bool IND_FOOTER { get; set; }
        public byte[] TEMPLATE { get; set; }
        public System.DateTime VALID_DTA_INI { get; set; }
        public Nullable<System.DateTime> VALID_DTA_END { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
        public string DIR_INCLUSE { get; set; }
        public string DIR_ESCLUSE { get; set; }
        public bool IND_SIGN { get; set; }
        public string TEMPLATE_TEXT { get; set; }
    }
}
