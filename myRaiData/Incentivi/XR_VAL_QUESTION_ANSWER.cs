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
    
    public partial class XR_VAL_QUESTION_ANSWER
    {
        public int ID_QST_ANSWER { get; set; }
        public int ID_QUESTION { get; set; }
        public Nullable<int> VALUE_INT { get; set; }
        public string VALUE_STR { get; set; }
        public Nullable<decimal> VALUE_DECIMAL { get; set; }
        public string DESCRIPTION { get; set; }
        public string NOT_HELP { get; set; }
        public System.DateTime VALID_DTA_INI { get; set; }
        public Nullable<System.DateTime> VALID_DTA_END { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
        public int NUM_ORDER { get; set; }
        public Nullable<int> ID_ANSWER { get; set; }
    
        public virtual XR_VAL_QUESTION XR_VAL_QUESTION { get; set; }
        public virtual XR_VAL_ANSWER XR_VAL_ANSWER { get; set; }
    }
}
