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
    
    public partial class XR_VAL_QUESTION_GROUP
    {
        public XR_VAL_QUESTION_GROUP()
        {
            this.XR_VAL_QUESTION = new HashSet<XR_VAL_QUESTION>();
            this.XR_VAL_QUESTION_GROUP1 = new HashSet<XR_VAL_QUESTION_GROUP>();
        }
    
        public int ID_QST_GROUP { get; set; }
        public string NAME { get; set; }
        public string DESCRIPTION { get; set; }
        public System.DateTime VALID_DTA_INI { get; set; }
        public Nullable<System.DateTime> VALID_DTA_END { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
        public Nullable<int> ID_QST_GROUP_MACRO { get; set; }
        public string NOT_ADDITIONAL_INFO { get; set; }
    
        public virtual ICollection<XR_VAL_QUESTION> XR_VAL_QUESTION { get; set; }
        public virtual ICollection<XR_VAL_QUESTION_GROUP> XR_VAL_QUESTION_GROUP1 { get; set; }
        public virtual XR_VAL_QUESTION_GROUP XR_VAL_QUESTION_GROUP2 { get; set; }
    }
}
