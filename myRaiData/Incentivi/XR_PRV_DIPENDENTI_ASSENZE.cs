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
    
    public partial class XR_PRV_DIPENDENTI_ASSENZE
    {
        public int ID_ASSENZA { get; set; }
        public int ID_DIPENDENTE { get; set; }
        public int ANNO { get; set; }
        public double QUANTITA { get; set; }
        public string UNITA_MISURA { get; set; }
        public string COD_ECCEZ_PADRE { get; set; }
        public string DESC_COD_ECCEZ_PADRE { get; set; }
        public string COD_ECCEZIONE { get; set; }
        public string DES_ECCEZIONE { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
        public Nullable<int> MESE { get; set; }
    
        public virtual XR_PRV_DIPENDENTI XR_PRV_DIPENDENTI { get; set; }
    }
}
