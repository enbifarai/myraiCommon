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
    
    public partial class XR_VAL_QUAL_FILTER
    {
        public XR_VAL_QUAL_FILTER()
        {
            this.XR_VAL_CAMPAIGN_SHEET_QUAL = new HashSet<XR_VAL_CAMPAIGN_SHEET_QUAL>();
            this.XR_VAL_CAMPAIGN_QUAL = new HashSet<XR_VAL_CAMPAIGN_QUAL>();
        }
    
        public int ID_QUAL_FILTER { get; set; }
        public string COD_QUAL_FILTER { get; set; }
        public string DESCRIPTION { get; set; }
        public string QUAL_INCLUDED { get; set; }
        public string QUAL_EXCLUDED { get; set; }
        public Nullable<int> LEVEL { get; set; }
    
        public virtual ICollection<XR_VAL_CAMPAIGN_SHEET_QUAL> XR_VAL_CAMPAIGN_SHEET_QUAL { get; set; }
        public virtual ICollection<XR_VAL_CAMPAIGN_QUAL> XR_VAL_CAMPAIGN_QUAL { get; set; }
    }
}
