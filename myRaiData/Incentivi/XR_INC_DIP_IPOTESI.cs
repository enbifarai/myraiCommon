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
    
    public partial class XR_INC_DIP_IPOTESI
    {
        public int ID_IPOTESI { get; set; }
        public int ID_DIPENDENTE { get; set; }
        public string COD_IPOTESI { get; set; }
        public System.DateTime DATA_RIF { get; set; }
        public int ID_PERSONA { get; set; }
        public string MATRICOLA { get; set; }
        public bool IS_VALID { get; set; }
        public Nullable<decimal> INCENTIVO_LORDO_IP { get; set; }
        public Nullable<decimal> UNA_TANTUM_LORDA_IP { get; set; }
        public Nullable<decimal> TFR_LORDO_INPS_IP { get; set; }
        public Nullable<decimal> TFR_LORDO_AZ_IP { get; set; }
        public Nullable<decimal> TFR_NETTO { get; set; }
        public Nullable<decimal> ALIQ_TFR { get; set; }
        public Nullable<decimal> NUM_MENS_PRINC_DEC { get; set; }
        public Nullable<decimal> NUM_MENS_AGG_DEC { get; set; }
        public Nullable<decimal> EX_FISSA { get; set; }
        public string NOT_TIP_SCELTA { get; set; }
        public string NOT_REQ_MATURATO { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
    }
}
