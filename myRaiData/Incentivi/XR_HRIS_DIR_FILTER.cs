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
    
    public partial class XR_HRIS_DIR_FILTER
    {
        public XR_HRIS_DIR_FILTER()
        {
            this.XR_DEM_DOCUMENTI = new HashSet<XR_DEM_DOCUMENTI>();
            this.XR_HRIS_PROTOCOLLI = new HashSet<XR_HRIS_PROTOCOLLI>();
        }
    
        public int ID_AREA_FILTER { get; set; }
        public string COD_AREA_FILTER { get; set; }
        public string DESCRIPTION { get; set; }
        public string DIR_INCLUDED { get; set; }
        public string DIR_EXCLUDED { get; set; }
    
        public virtual ICollection<XR_DEM_DOCUMENTI> XR_DEM_DOCUMENTI { get; set; }
        public virtual ICollection<XR_HRIS_PROTOCOLLI> XR_HRIS_PROTOCOLLI { get; set; }
    }
}
