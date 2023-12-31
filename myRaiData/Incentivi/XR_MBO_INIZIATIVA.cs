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
    
    public partial class XR_MBO_INIZIATIVA
    {
        public XR_MBO_INIZIATIVA()
        {
            this.XR_MBO_SCHEDA = new HashSet<XR_MBO_SCHEDA>();
        }
    
        public int ID_INIZIATIVA { get; set; }
        public string NME_NOME { get; set; }
        public string DES_DESCRIZIONE { get; set; }
        public System.DateTime DTA_INI_ASSEGNAZIONE { get; set; }
        public System.DateTime DTA_END_ASSEGNAZIONE { get; set; }
        public System.DateTime DTA_INI_VALUT { get; set; }
        public System.DateTime DTA_END_VALUT { get; set; }
        public decimal DEC_IMPORTO_MAX_TOP { get; set; }
        public decimal DEC_IMPORTO_MAX_FULL { get; set; }
        public decimal DEC_IMPORTO_MAX_MANAGER { get; set; }
        public System.DateTime VALID_DTA_INI { get; set; }
        public Nullable<System.DateTime> VALID_DTA_END { get; set; }
        public string COD_USER { get; set; }
        public string COD_TERMID { get; set; }
        public System.DateTime TMS_TIMESTAMP { get; set; }
        public Nullable<int> ID_SCHEDA_VAL { get; set; }
        public Nullable<decimal> DEC_COEFF_DECURTAZIONE { get; set; }
        public Nullable<decimal> DEC_COEFF_GESTIONALE { get; set; }
    
        public virtual ICollection<XR_MBO_SCHEDA> XR_MBO_SCHEDA { get; set; }
    }
}
